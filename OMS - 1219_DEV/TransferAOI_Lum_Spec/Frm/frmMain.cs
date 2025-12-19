using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CsvHelper;
using System.Globalization;
using HsApiNet;
using System.Threading;
using System.Linq;
using Matrox.MatroxImagingLibrary;
using PylonC.NETSupportLibrary;
using System.Text;
using FTP_Ctrl.Utilities.FTP;
using LightMeasure;
using System.IO.Ports;
using MaterialSkin.Controls;
using MaterialSkin;
using OpticalMeasuringSystem.Frm;
using System.Drawing.Text;
using CommonBase.Logger;
using System.Data;
using BaseTool;
using HardwareManager;

using AUO.SubSystemControl;

using FrameGrabber;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using CommonSettings;

namespace OpticalMeasuringSystem
{

    public delegate void PanelboxDelegate(Panel panel, MIL_ID img);

    public partial class frmMain : MaterialForm
    {

        public static MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;


        // Grabber
        public MilDigitizer grabber;

        // Motor Control
        private IMotorControl motorControl;

        // Task
        private TaskNdPrediction taskNdPrediction;
        private TaskOneColorCalibration taskOneColorCalibration;
        private TaskOneColorDataCreate taskOneColorDataCreate;
        private Task_ThirdParty_Correction task_ThirdParty_Correction;

        public static int ImgW;
        public static int ImgH;

        public InfoManager Info;

        public static IntPtr mainFormDisplayHandle;
        public static bool IsNeedUpdate = false;

        private bool isIdentifyValueOK = false;
        private bool isRoiMode = false;

        private Thread mainThread;
        private Thread updateForm;

        public bool isRun;

        private Process processes;

        private int graphicSelectedLabel = -1;
        private bool isUpdateCameraTemperature = true;

        private Point mousePosition = new Point(-1, -1);

        private ClsUrRobotInterface Robot = null;
        private X_PLC_Ctrl X_PLC = null;
        private D65_Light_Ctrl D65_Light = null;
        private SR3_Ctrl SR3 = null;
        private CornerstoneB_Ctrl CornerstoneB = null;
        private PowerSupply_Ctrl PowerSupply = null;

        private LightSourceA_Ctrl LightSourceA = null;

        public frmMain()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);

            materialSkinManager.ColorScheme = new ColorScheme
              (
                  Color.FromArgb(64, 64, 64),
                  Color.FromArgb(255, 32, 32, 32),
                  Color.FromArgb(255, 32, 32, 32),
                  Color.FromArgb(64, 64, 64),
                  TextShade.WHITE
              );

            InitializeComponent();

            mainFormDisplayHandle = this.PnlGrabViewer.Handle;

            processes = Process.GetCurrentProcess();

            //--- UI ---
        }

        private void HideUI()
        {
            if (GlobalVar.IsSecretHidden)
            {
                toolStripMenuItem1.Visible = false;
                AboutAUOToolStripMenuItem.Visible = false;

                btnLoadFile.Visible = false;
                btnSaveAs.Visible = false;

                toolStripMenuItem_Recipe.Enabled = false;
                toolStripMenuRecipeChoose.Enabled = false;
                toolStripMenuRecipeChoose.Items.Clear();
                toolStripMenuRecipeChoose.Items.Add("AIC Series");
                toolStripMenuRecipeChoose.Text = "AIC Series";
                GlobalVar.Config.RecipeName = "AIC Series";
                LoadRecipe("AIC Series");
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {   
            #region Config

            //SD
            Directory.CreateDirectory(GlobalVar.Config.ConfigPath);

            if (File.Exists(GlobalVar.Config.ConfigPath + @"\NetworkConfig.xml"))
            {
                GlobalVar.SD = GlobalVar.SD.Read(GlobalVar.Config.ConfigPath + @"\NetworkConfig.xml");
            }
            else
            {
                GlobalVar.SD.Create(GlobalVar.SD, GlobalVar.Config.ConfigPath + @"\NetworkConfig.xml");
            }

            GlobalVar.DeviceType = GlobalVar.SD.MotorControl_Package;

            if (File.Exists(GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml"))
            {
                GlobalVar.CorrData = GlobalVar.CorrData.Read(GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml");
            }
            else
            {
                GlobalVar.CorrData.Create(GlobalVar.CorrData, GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml");
            }

            #endregion

            this.lbPanelShowMsg.Text = "";


            #region Log

            //Initail Logger
            Info = new InfoManager(GlobalVar.Config.SystemLogPath,
                GlobalVar.Config.NetworkLogPath,
                GlobalVar.Config.WarningLogPath,
                GlobalVar.Config.ErrorLogPath,
                GlobalVar.Config.DebugLogPath);

            //Assign to rich Text Box
            Info.SetGeneralTextBox(ref this.richTextBox_LoggerGeneral);
            Info.SetNetworkTextBox(ref this.richTextBox_LoggerGeneral);
            Info.SetDebugTextBox(ref this.richTextBox_LoggerDebug);
            Info.SetErrorTextBox(ref this.richTextBox_LoggerError);

            #endregion

            this.Init();

            // 註冊點選graphic回傳label事件
            MyMilDisplay.GraphicSelectValueChangedEvent -= GraphicSelectValueChangedEvent;
            MyMilDisplay.GraphicSelectValueChangedEvent += GraphicSelectValueChangedEvent;

            this.Set_UI();
            this.Set_UI_MeasureType();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.motorControl.UpdateStatus -= MotorControl_UpdateStatus;

            if (Info != null) Info.Dispose();

            if (this.grabber != null)
                this.grabber.Dispose();
            GlobalVar.LightMeasure.TristimulusImage.Free();
            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                GlobalVar.LightMeasure.ChromaImage.Free();

            this.Dispose(); // 要先dispose form才close得掉milsystem

            MyMilDisplay.Close();
            MyMil.Close();

            processes.Kill();
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            LoadRecipe(GlobalVar.Config.RecipeName);

            MyMilDisplay.GrabViewer = this.PnlGrabViewer;

            // UI Reset
            this.ROIAnalysis_UIReset();

            if (!isRun)
            {
                if (toolStripMenuRecipeChoose.Text == "")
                {
                    toolStripMenuRecipeChoose.Focus();
                    MessageBox.Show("Select Recipe First !", "Choose Recipe");
                    return;
                }

                // 清除 ROI 顯示
                this.btn_dataGridViewROI_Clear_Click(btn_dataGridViewROI_Clear, null);

                if (Switch_SpectralCorrection.Checked)
                {

                    HardwareUnit HardwarePara = new HardwareUnit {
                        Grabber = this.grabber,
                        Motor = this.motorControl,
                        PLC = this.X_PLC,
                        D65 = this.D65_Light,
                        Robot = this.Robot,
                        SR3 = this.SR3,
                        LightSourceA = this.LightSourceA,
                    };

                    task_ThirdParty_Correction = new Task_ThirdParty_Correction(GlobalVar.OmsParam, Info, HardwarePara);

                    task_ThirdParty_Correction.Start();
                }
                else
                {
                    frmProcessInfoSetting frmProcessStartSetting = new frmProcessInfoSetting();
                    frmProcessStartSetting.SetProcessInfo -= SetProcessInfoEvent;
                    frmProcessStartSetting.SetProcessInfo += SetProcessInfoEvent;
                    frmProcessStartSetting.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("流程中");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {            
            if (task_ThirdParty_Correction != null)
            {
                task_ThirdParty_Correction.Stop();
            }

            isRun = false;
            Info.General("Stop Click");
        }

        #region Init

        private void Init()
        {
            try
            {
                this.Mil_Init();

                MilDigParam DigPara = new MilDigParam {
                    Camera_Type = GlobalVar.SD.Camera_Type,
                    Camera_CamFile = GlobalVar.SD.Camera_CamFile,
                    Camera_No = GlobalVar.SD.Camera_CamNo,
                    Camera_Bin = GlobalVar.SD.Camera_CamNo
                };

                this.grabber = new MilDigitizer(MyMil.MilSystem, DigPara, this.Info);
                this.grabber.Initial();
                this.grabber.SetExposureTime(300000);

                if (this.grabber.grabImage != null)
                    MyMilDisplay.SetMainDisplayFit(this.grabber.grabImage, this.PnlGrabViewer.Handle);

            }
            catch (Exception ex)
            {
                Info.Error("[Load System Data] " + ex.Message.ToString());
            }

            try
            {
                this.Motor_Init();
            }
            catch (Exception ex)
            {
                Info.Error(ex.Message.ToString());
            }

            try
            {
                this.FW_Init();
            }
            catch (Exception ex)
            {
                Info.Error(ex.Message.ToString());
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.Robot_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.X_PLC_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.D65_Light_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.SR3_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.CornerstoneB_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.LightSourceA_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }

            if (!GlobalVar.IsSecretHidden)
            {
                try
                {
                    this.PowerSupply_Init();
                }
                catch (Exception ex)
                {
                    Info.Error(ex.Message.ToString());
                }
            }
        
            HideUI();
        }

        private void Mil_Init()
        {
            MyMil.Init(GlobalVar.SD.Camera_Type);
            MyMilDisplay.Init();
        }

        private void Motor_Init()
        {
            EnumMotoControlPackage Type = GlobalVar.DeviceType;

            switch (Type)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    {
                        this.motorControl = new AiryMotorCtrl(this.Info);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    {
                        this.motorControl = new AiryUnitCtrl_4in1(this.Info);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    {
                        this.motorControl = new AiryUnitCtrl_4in1_TCPIP(this.Info);
                    }
                    break;

                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    {
                        this.motorControl = new AiryUnitCtrl_211_USB(this.Info);
                    }
                    break;

                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.motorControl = new AuoMotorCtrl(this.Info, GlobalVar.SD);
                    }
                    break;

                case EnumMotoControlPackage.MS5515M:
                    {
                        this.motorControl = new MS5515M(this.Info);
                    }
                    break;

                case EnumMotoControlPackage.CircleTac:
                    {
                        this.motorControl = new CircleTacCtrl(this.Info, GlobalVar.SD.CircleTac_ReadMcu);
                    }
                    break;
            }

            if (GlobalConfig.BypassHardware) return;

            switch (Type)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    {
                       string Comport = GlobalVar.SD.FocuserMotor_ComPort;
                        this.motorControl.Open(Comport);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    {
                        string Comport = GlobalVar.SD.FocuserMotor_ComPort;
                        this.motorControl.Open(Comport);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    {
                        this.motorControl.Open();
                    }
                    break;

                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    {
                        string Comport_1 = GlobalVar.SD.FocuserMotor_ComPort;
                        string Comport_2 = GlobalVar.SD.FocuserMotor_ComPort_2;
                        string Comport_3 = GlobalVar.SD.FocuserMotor_ComPort_3;
                        this.motorControl.Open(Comport_1, Comport_2, Comport_3);
                    }
                    break;

                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        string Comport = GlobalVar.SD.AuoMotor_Comport;
                        this.motorControl.Open(Comport);
                    }
                    break;

                case EnumMotoControlPackage.MS5515M:
                    {
                        string Comport = GlobalVar.SD.FocuserMotor_ComPort;
                        this.motorControl.Open(Comport);
                    }
                    break;

                case EnumMotoControlPackage.CircleTac:
                    {
                        this.motorControl.Open();
                        CircleTacCtrl CircleTac = (CircleTacCtrl)motorControl;
                        CircleTac.Init(GlobalVar.SD.CircleTac_InitPos_Focuser, GlobalVar.SD.CircleTac_InitPos_Aperture, GlobalVar.SD.CircleTac_InitPos_FW1, GlobalVar.SD.CircleTac_InitPos_FW2);
                    }
                    break;
            }

            if (!this.motorControl.IsWork())
            {
                throw new Exception(string.Format($"[{Type}] Motor control not work ! \r\n"));
            }
            else
            {
                this.motorControl.UpdateStatus -= MotorControl_UpdateStatus;
                this.motorControl.UpdateStatus += MotorControl_UpdateStatus;

                Info.General(string.Format($"[{Type}] Motor control work ! "));
            }
        }

        private void MotorControl_UpdateStatus(MotorInfo Focuser, MotorInfo Aperture, MotorInfo FW1, MotorInfo FW2)
        {

            EnumMotoControlPackage Type = GlobalVar.DeviceType;


            switch (Type)
            {

                case EnumMotoControlPackage.CircleTac:
                    {
                        this.Label_FocusPos.Invoke(new Action(() => { this.Label_FocusPos.Text = $"Focuser = {Focuser.Position}"; }));
                        this.Label_Aperture.Invoke(new Action(() => { this.Label_Aperture.Text = $"Aperture = {Aperture.Position}"; }));
                        this.Label_FW1.Invoke(new Action(() => { this.Label_FW1.Text = $"FW1(XYZ) = {FW1.Position}"; }));
                        this.Label_FW2.Invoke(new Action(() => { this.Label_FW2.Text = $"FW2(ND) = {FW2.Position}"; }));
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    {
                        this.Label_FocusPos.Invoke(new Action(() => { this.Label_FocusPos.Text = $"Focuser = {Focuser.Position}"; }));
                        this.Label_Aperture.Invoke(new Action(() => { this.Label_Aperture.Text = $"Aperture = {Aperture.Position}"; }));
                        this.Label_FW1.Invoke(new Action(() => { this.Label_FW1.Text = $"FW1(XYZ) = {FW1.Position}"; }));
                        this.Label_FW2.Invoke(new Action(() => { this.Label_FW2.Text = $"FW2(ND) = {FW2.Position}"; }));
                    }
                    break;

            }


        }

        private void FW_Init()
        {
            if (GlobalConfig.BypassHardware) return;

            String status = "";
            EnumMotoControlPackage Type = GlobalVar.DeviceType;

            switch (Type)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        if (GlobalVar.SD.XYZFilterName != "")
                        {
                            GlobalVar.Device.XyzFilter.Init(GlobalVar.SD.XYZFilterName);
                            // Initial XYZ Filter Position , 2023/07/11 Rick add
                            if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                GlobalVar.Device.XyzFilter.ChangeWheel(0);
                            else
                                status = $"[{Type}] Xyz Filter was not work !" + Environment.NewLine;
                        }

                        if (GlobalVar.SD.NDFilterName != "")
                        {
                            GlobalVar.Device.NdFilter.Init(GlobalVar.SD.NDFilterName);
                            // Initial ND Filter Position , 2023/07/11 Rick add
                            if (GlobalVar.Device.NdFilter.is_FW_Work)
                                GlobalVar.Device.NdFilter.ChangeWheel(0);
                            else
                                status += $"[{Type}] Nd Filter was not work !" + Environment.NewLine;
                        }

                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                //case EnumMotoControlPackage.CircleTac:
                    {
                        if (this.motorControl.IsWork())
                            this.motorControl.Move(-1, -1, 0, 0);
                        else
                            status = $"[{Type}] Xyz Filter & Nd Filter was not work !";
                    }
                    break;

            }

            if (status != "")
                throw new Exception(status);
        }

        private void Robot_Init()
        {
            this.Robot = new ClsUrRobotInterface(Info);

            if (GlobalConfig.BypassHardware) return;

            this.Robot.Connect(GlobalVar.SD.UrRobot_IPAddress, 30000);
        }


        private void X_PLC_Init()
        {
            X_PLC = new X_PLC_Ctrl(this.Info);

            if (GlobalConfig.BypassHardware) return;

            string IP = GlobalVar.SD.X_PLC_IPAddress;
            int Timeout = GlobalVar.SD.X_PLC_Timeout;

            X_PLC.Connect(IP, Timeout);
        }

        private bool D65_Light_Init()
        {
            D65_Light = new D65_Light_Ctrl(this.Info);

            if (GlobalConfig.BypassHardware) return false;

            int Port = GlobalVar.SD.D65_Light_Comtport;
            int BaudRate = GlobalVar.SD.D65_Light_Baudrate;

            return D65_Light.Connect(Port, BaudRate);
        }

        private void SR3_Init()
        {
            SR3 = new SR3_Ctrl(this.Info);

            if (GlobalConfig.BypassHardware) return;

            int Port = GlobalVar.SD.SR3_Comtport;
            int BaudRate = GlobalVar.SD.SR3_Baudrate;

            SR3.Connect(Port, BaudRate);
        }

        private void CornerstoneB_Init()
        {
            CornerstoneB = new CornerstoneB_Ctrl(this.Info);

            if (GlobalConfig.BypassHardware) return;

            int Port = GlobalVar.SD.CornerstoneB_Comtport;
            int BaudRate = GlobalVar.SD.CornerstoneB_Baudrate;

            CornerstoneB.Connect(Port, BaudRate);
        }

        private void LightSourceA_Init()
        {

            string IP = GlobalVar.SD.LightSourceA_IP;
            int Port = GlobalVar.SD.LightSourceA_Port;

            LightSourceA = new LightSourceA_Ctrl(IP, Port, this.Info);

            if (GlobalConfig.BypassHardware) return;

            LightSourceA.CheckConnection();
        }

        private void PowerSupply_Init()
        {
            PowerSupply = new PowerSupply_Ctrl(this.Info);

            if (GlobalConfig.BypassHardware) return;

            int Port = GlobalVar.SD.PowerSupply_Comtport;
            int BaudRate = GlobalVar.SD.PowerSupply_Baudrate;

            PowerSupply.Connect(Port, BaudRate);
        }

        #endregion

        #region Update UI

        private void Set_UI()
        {
            this.splitContainer1.Panel1Collapsed = false;
            this.splitContainer1.Panel2Collapsed = true;
            this.isRoiMode = false;
            this.btnDrawRoi.UseAccentColor = false;
            this.btnDrawRoi.Refresh();


            // 更新UI即時資訊 (ZSystem 相機溫度)
            this.updateForm = new Thread(UpdateUi);
            this.updateForm.Start();

            // Use LINQ to disable Column Sorting
            this.dataGridViewROI.Columns.Cast<DataGridViewColumn>().ToList().ForEach(f => f.SortMode = DataGridViewColumnSortMode.NotSortable);

            // UI
            MyMilDisplay.GrabViewer = this.PnlGrabViewer;
        }

        public void Set_UI_MeasureType()
        {
            string var = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                this.dataGridViewROI.Columns["ColumnCx"].Visible = false;
                this.dataGridViewROI.Columns["ColumnCy"].Visible = false;
                this.Text = $"[{var}] " + @"AUO Optical Measurement System [Luminance]";

                // Save Button UI
                tristimulusXToolStripMenuItem1.Visible = false;
                tristimulusZToolStripMenuItem1.Visible = false;
                chromaXToolStripMenuItem.Visible = false;
                chromaYToolStripMenuItem.Visible = false;

                tristimulusXToolStripMenuItem1.Checked = false;
                tristimulusZToolStripMenuItem1.Checked = false;
                chromaXToolStripMenuItem.Checked = false;
                chromaYToolStripMenuItem.Checked = false;


            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                this.dataGridViewROI.Columns["ColumnCx"].Visible = true;
                this.dataGridViewROI.Columns["ColumnCy"].Visible = true;
                this.Text = $"[{var}] " + @"AUO Optical Measurement System [Luminance\Chroma]";


            }



            // Set Grab View Panel
        }


        private void UpdateUi()
        {
            if (GlobalVar.SD.Camera_Type != "Z_SYSTEM")
            {
                this.lblCameraTemperature.Invoke(new Action(() => { this.lblCameraTemperature.Text = ""; }));
                return;
            }

            while (true)
            {
                if (GlobalVar.SD.Camera_Type == "Z_SYSTEM")
                    this.UpdateCameraTemperature();
                Thread.Sleep(500);
            }
        }

        private void UpdateCameraTemperature()
        {
            if (!isUpdateCameraTemperature || this.grabber == null) return;

            try
            {
                double t = this.grabber.GetTemperature(0);

                this.lblCameraTemperature.Invoke(new Action(() => { this.lblCameraTemperature.Text = $"Camera Temperature: {t} °C"; }));

            }
            catch (Exception ex)
            {
                isUpdateCameraTemperature = false;
                MessageBox.Show($"Update camera temperature failed : {ex.Message}");
            }
        }





        #endregion

        #region --- UI 委派 ---          



        public static void UpdateDisplay(Panel panel, MIL_ID Img)
        {
            if (panel.InvokeRequired)
            {
                PanelboxDelegate action = new PanelboxDelegate(UpdateDisplay);

                panel.BeginInvoke(action, panel, Img);
            }
            else
            {
                MyMilDisplay.SetMainDisplayFit(Img, panel.Handle);
            }
        }

        #endregion --- UI 委派 ---

        #region --- 開始/結束/主流程 ---

        #region --- MainProc ---

        private void MainProc()
        {
            while (true)
            {
                if (taskNdPrediction != null)
                {
                    taskNdPrediction.Run(isRun);
                }

                if (taskOneColorDataCreate != null)
                {
                    taskOneColorDataCreate.Run(isRun, false);
                }

                if (taskOneColorCalibration != null)
                {
                    taskOneColorCalibration.Run(isRun);
                }

                if (IsNeedUpdate)
                {
                    UpdateDisplay(PnlGrabViewer, MyMilDisplay.MilMainDisplayImage[1]);
                    IsNeedUpdate = false;
                }

                if ((taskNdPrediction != null && taskNdPrediction.FinalStatus == EnumFinalStatus.OK) ||
                    (taskOneColorDataCreate != null && taskOneColorDataCreate.FinalStatus == EnumFinalStatus.OK) ||
                    (taskOneColorCalibration != null && taskOneColorCalibration.FinalStatus == EnumFinalStatus.OK))
                {
                    isRun = false;
                    break;
                }
                else if ((taskNdPrediction != null && taskNdPrediction.FinalStatus == EnumFinalStatus.NG) ||
                         (taskOneColorDataCreate != null && taskOneColorDataCreate.FinalStatus == EnumFinalStatus.NG) ||
                         (taskOneColorCalibration != null && taskOneColorCalibration.FinalStatus == EnumFinalStatus.NG))
                {
                    isRun = false;
                    Info.Error("Process failed.");
                    break;
                }

                Thread.Sleep(1);
            }
        }

        #endregion --- MainProc ---




        #endregion --- 開始/結束/主流程 ---

        #region --- 讀取/儲存 Workspace ---

        #region --- btnLoadFile_Click ---

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            MyMilDisplay.GrabViewer = this.PnlGrabViewer;

            OpenFileDialog dialog = new OpenFileDialog();

            try
            {
                dialog.Title = "請選擇AUO量測檔";
                dialog.Filter = "AUO Optical Measurement Results |*.omr";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // 檢查資料夾 (清查資料夾所有檔案 & 創建資料夾)
                    ZipHelper.CheckFolder("./temp");

                    // 解壓縮workspace
                    ZipHelper.DeCompressionZip(dialog.FileName, "./temp");

                    // 檢查解壓縮後 是否有所有應有檔案
                    if (!this.IsOmrImportSuccess("./temp")) return;

                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                    {
                        // 匯入Tristimulus X
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.TristimulusImage.ImgX);
                        if (File.Exists($"./temp/TristimulusX.tif"))
                        {
                            MIL.MbufImport("./temp/TristimulusX.tif",
                                       MIL.M_DEFAULT,
                                       MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                       MyMil.MilSystem,
                                       ref GlobalVar.LightMeasure.TristimulusImage.ImgX);
                        }

                        // 匯入Tristimulus Y
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.TristimulusImage.ImgY);
                        if (File.Exists($"./temp/TristimulusY.tif"))
                        {
                            MIL.MbufImport("./temp" + "\\TristimulusY.tif",
                                           MIL.M_DEFAULT,
                                           MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                           MyMil.MilSystem,
                                           ref GlobalVar.LightMeasure.TristimulusImage.ImgY);
                        }

                        // 匯入Tristimulus Z
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.TristimulusImage.ImgZ);
                        if (File.Exists($"./temp/TristimulusZ.tif"))
                        {
                            MIL.MbufImport("./temp" + "\\TristimulusZ.tif",
                                           MIL.M_DEFAULT,
                                           MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                           MyMil.MilSystem,
                                           ref GlobalVar.LightMeasure.TristimulusImage.ImgZ);
                        }

                        // 匯入Chroma X
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.ChromaImage.ImgCx);
                        if (File.Exists($"./temp/ChromaX.tif"))
                        {
                            MIL.MbufImport("./temp" + "\\ChromaX.tif",
                                       MIL.M_DEFAULT,
                                       MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                       MyMil.MilSystem,
                                       ref GlobalVar.LightMeasure.ChromaImage.ImgCx);
                        }

                        // 匯入Chroma Y
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.ChromaImage.ImgCy);
                        if (File.Exists($"./temp/ChromaY.tif"))
                        {
                            MIL.MbufImport("./temp" + "\\ChromaY.tif",
                                       MIL.M_DEFAULT,
                                       MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                       MyMil.MilSystem,
                                       ref GlobalVar.LightMeasure.ChromaImage.ImgCy);
                        }
                    }
                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                    {
                        // 匯入Tristimulus Y
                        MilNetHelper.MilBufferFree(ref GlobalVar.LightMeasure.TristimulusImage.ImgY);
                        if (File.Exists($"./temp/TristimulusY.tif"))
                        {
                            MIL.MbufImport("./temp" + "\\TristimulusY.tif",
                                           MIL.M_DEFAULT,
                                           MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                                           MyMil.MilSystem,
                                           ref GlobalVar.LightMeasure.TristimulusImage.ImgY);
                        }
                    }

                    // 設定Tristimulus Y為顯示影像
                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 1);
                    MyMilDisplay.SetMainDisplayFit(GlobalVar.LightMeasure.TristimulusImage.ImgY, PnlGrabViewer.Handle, 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ ex.Message}");
            }
            finally
            {
                if (dialog != null)
                {
                    dialog.Dispose();
                }
            }
        }

        #endregion --- btnLoadFile_Click ---

        #region --- btnSaveAs_Click ---

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            MyMilDisplay.GrabViewer = this.PnlGrabViewer;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = @"D:\"; //設定初始路徑
            saveFileDialog.Filter = "AUO Optical Measurement Results | *.omr";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 檢查目前亮色度圖是否都有值
                if (!IsOmrtReadyToSave()) return;

                // 檢查資料夾
                ZipHelper.CheckFolder(@"./temp");

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    // 存出亮色度圖
                    MIL.MbufSave(@"./temp/TristimulusX.tif", GlobalVar.LightMeasure.TristimulusImage.ImgX);
                    MIL.MbufSave(@"./temp/TristimulusY.tif", GlobalVar.LightMeasure.TristimulusImage.ImgY);
                    MIL.MbufSave(@"./temp/TristimulusZ.tif", GlobalVar.LightMeasure.TristimulusImage.ImgZ);
                    MIL.MbufSave(@"./temp/ChromaX.tif", GlobalVar.LightMeasure.ChromaImage.ImgCx);
                    MIL.MbufSave(@"./temp/ChromaY.tif", GlobalVar.LightMeasure.ChromaImage.ImgCy);
                }
                else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                {
                    // 存出亮度圖
                    MIL.MbufSave(@"./temp/TristimulusY.tif", GlobalVar.LightMeasure.TristimulusImage.ImgY);
                }

                // 建立workspace
                ZipHelper.CreateZip(saveFileDialog.FileName, "./temp");

                MessageBox.Show("File packaging is complete.");

                // 檢查資料夾
                ZipHelper.CheckFolder("./temp");
            }
        }

        #endregion --- btnSaveAs_Click ---

        #region --- IsOmrImportSuccess ---

        private bool IsOmrImportSuccess(string folderPath)
        {
            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (!File.Exists($@"{folderPath}\TristimulusX.tif"))
                {
                    MessageBox.Show("Image TristimulusX has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!File.Exists($@"{folderPath}\TristimulusY.tif"))
                {
                    MessageBox.Show("Image TristimulusY has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!File.Exists($@"{folderPath}\TristimulusZ.tif"))
                {
                    MessageBox.Show("Image TristimulusZ has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!File.Exists($@"{folderPath}\ChromaX.tif"))
                {
                    MessageBox.Show("Image ChromaX has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!File.Exists($@"{folderPath}\ChromaY.tif"))
                {
                    MessageBox.Show("Image ChromaY has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (!File.Exists($@"{folderPath}\TristimulusY.tif"))
                {
                    MessageBox.Show("Image TristimulusY has been damaged.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        #endregion --- IsOmrImportSuccess ---

        #region --- IsOmrtReadyToSave ---

        private bool IsOmrtReadyToSave()
        {
            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (GlobalVar.LightMeasure.TristimulusImage.ImgX == MIL.M_NULL)
                {
                    MessageBox.Show("Image TristimulusX CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                {
                    MessageBox.Show("Image TristimulusY CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (GlobalVar.LightMeasure.TristimulusImage.ImgZ == MIL.M_NULL)
                {
                    MessageBox.Show("Image TristimulusZ CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (GlobalVar.LightMeasure.ChromaImage.ImgCx == MIL.M_NULL)
                {
                    MessageBox.Show("Image ChromaX CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (GlobalVar.LightMeasure.ChromaImage.ImgCy == MIL.M_NULL)
                {
                    MessageBox.Show("Image ChromaY CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                {
                    MessageBox.Show("Image TristimulusY CANNOT be null.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        #endregion --- IsOmrtReadyToSave ---

        #endregion --- 讀取/儲存 Workspace ---

        #region --- 單張影像儲存 ---

        #region --- BtnSaveColorResult_Click ---

        private void BtnSaveColorResult_Click(object sender, EventArgs e)
        {
            MyMilDisplay.GrabViewer = this.PnlGrabViewer;

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() != DialogResult.OK) return;

            string path = folderBrowserDialog.SelectedPath;

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (tristimulusXToolStripMenuItem1.Checked && GlobalVar.LightMeasure.TristimulusImage.ImgX != MIL.M_NULL)
                    MIL.MbufSave($@"{path}\TristimulusX.tif", GlobalVar.LightMeasure.TristimulusImage.ImgX);

                if (tristimulusYToolStripMenuItem1.Checked && GlobalVar.LightMeasure.TristimulusImage.ImgY != MIL.M_NULL)
                    MIL.MbufSave($@"{path}\TristimulusY.tif", GlobalVar.LightMeasure.TristimulusImage.ImgY);

                if (tristimulusZToolStripMenuItem1.Checked && GlobalVar.LightMeasure.TristimulusImage.ImgZ != MIL.M_NULL)
                    MIL.MbufSave($@"{path}\TristimulusZ.tif", GlobalVar.LightMeasure.TristimulusImage.ImgZ);

                if (chromaXToolStripMenuItem.Checked && GlobalVar.LightMeasure.ChromaImage.ImgCx != MIL.M_NULL)
                    MIL.MbufSave($@"{path}\ChromaX.tif", GlobalVar.LightMeasure.ChromaImage.ImgCx);

                if (chromaYToolStripMenuItem.Checked && GlobalVar.LightMeasure.ChromaImage.ImgCy != MIL.M_NULL)
                    MIL.MbufSave($@"{path}\ChromaY.tif", GlobalVar.LightMeasure.ChromaImage.ImgCy);
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                if (tristimulusYToolStripMenuItem1.Checked)
                    MIL.MbufSave($@"{path}\TristimulusY.tif", GlobalVar.LightMeasure.TristimulusImage.ImgY);
            }
        }

        #endregion --- BtnSaveColorResult_Click ---

        #endregion --- 單張影像儲存 ---

        #region --- ROI分析 ---

        private int currentRowIndex = -1;

        #region --- btnDrawRoi_Click ---

        private void btnDrawRoi_Click(object sender, EventArgs e)
        {
            MyMilDisplay.GrabViewer = this.PnlGrabViewer;

            //MyMilIp.ul = new List<MyMilIp.MeasureUnit>();
            if (splitContainer1.Panel1Collapsed)
            {
                // 展開Panel1
                splitContainer1.Panel1Collapsed = false;
                // 折疊Panel2
                splitContainer1.Panel2Collapsed = true;

                // 清除ROI List
                //MyMilIp.ul.Clear();

                // 重設顯示 (Start 時，要切回這張影像)
                //MyMilDisplay.SetMainDisplayFit(MyMilDisplay.MilMainDisplayImage[1], this.PnlGrabViewer.Handle);

                // 設定Tristimulus Y為顯示影像
                if (GlobalVar.LightMeasure.TristimulusImage.ImgY != MIL.M_NULL)
                {
                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 1);
                    MyMilDisplay.SetMainDisplayFit(GlobalVar.LightMeasure.TristimulusImage.ImgY, PnlGrabViewer.Handle, 1);
                }

                // 重置DataGridView
                //this.currentRowIndex = -1;
                //UpdateDatGridViewROI(currentRowIndex);

                this.isRoiMode = false;

                // 改變按鈕顏色
                btnDrawRoi.UseAccentColor = false;
                btnDrawRoi.Refresh();

                // 隱藏按鈕
                this.btn_dataGridViewROI_Load.Visible = false;
                this.btn_dataGridViewROI_Clear.Visible = false;
                this.btn_dataGridViewROI_SaveCSV.Visible = false;

                // 隱藏ComboBox
                this.lblCalibrationFile.Visible = false;
                this.tbxCalibrationFile.Visible = false;
            }
            else
            {
                if (toolStripMenuRecipeChoose.Text == "")
                {
                    this.toolStripMenuRecipeChoose.Focus();
                    MessageBox.Show("Select Recipe First !", "Choose Recipe");
                    return;
                }

                // 折疊Panel1
                splitContainer1.Panel1Collapsed = true;
                // 展開Panel2
                splitContainer1.Panel2Collapsed = false;

                // CalibrationFile
                this.tbxCalibrationFile.Items.Clear();
                // this.tbxCalibrationFile.Items.Add("自動曝光");
                this.tbxCalibrationFile.Items.Add("Recipe");
                this.tbxCalibrationFile.Items.AddRange(this.GetAllCalibrationFile().ToArray());

                this.isRoiMode = true;

                // 改變按鈕顏色
                btnDrawRoi.UseAccentColor = true;
                btnDrawRoi.Refresh();

                // 顯示按鈕
                this.btn_dataGridViewROI_Load.Visible = true;
                this.btn_dataGridViewROI_Clear.Visible = true;
                this.btn_dataGridViewROI_SaveCSV.Visible = true;

                // 顯示ComboBox
                this.lblCalibrationFile.Visible = true;
                this.tbxCalibrationFile.Visible = true;
            }
        }

        #endregion --- btnDrawRoi_Click ---

        #region --- DataGridViewROI_CellMouseUp ---

        private void DataGridViewROI_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 選取列
            DataGridViewROISelectRow(e.RowIndex);

            // 右鍵展開清單
            if (e.Button == MouseButtons.Right)
            {
                //contextMenuStripRoiEditor.Show(MousePosition.X, MousePosition.Y);
            }

            // 同步選取選中dataGridView的ROI
            int selectGraphicLabel;
            if (this.currentRowIndex < 0) return;
            if (dataGridViewROI.Rows[this.currentRowIndex].Cells[0].Value == null) return;
            if (int.TryParse(dataGridViewROI.Rows[this.currentRowIndex].Cells[0].Value.ToString(), out selectGraphicLabel))
            {
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(selectGraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
            }
        }

        #endregion --- DataGridViewROI_CellMouseUp ---

        #region --- ContextMenuStripRoiEditor_Opening ---

        private void ContextMenuStripRoiEditor_Opening(object sender, CancelEventArgs e)
        {
            //if (currentRowIndex == -1) return;

            if (dataGridViewROI.Rows.Count > 0)
            {
                if (this.currentRowIndex == -1)
                {
                    contextMenuStripRoiEditor.Items[0].Enabled = true;
                    contextMenuStripRoiEditor.Items[1].Enabled = false;
                    contextMenuStripRoiEditor.Items[2].Enabled = false;
                }
                else
                {
                    contextMenuStripRoiEditor.Items[0].Enabled = true;
                    contextMenuStripRoiEditor.Items[1].Enabled = true;
                    contextMenuStripRoiEditor.Items[2].Enabled = true;
                }
            }
            else
            {
                contextMenuStripRoiEditor.Items[0].Enabled = true;
                contextMenuStripRoiEditor.Items[1].Enabled = false;
                contextMenuStripRoiEditor.Items[2].Enabled = false;
            }
        }

        #endregion --- ContextMenuStripRoiEditor_Opening ---

        #region --- AddToolStripMenuItem_Click ---

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 新增ROI
            int addIndex = (int)AddROI(300, 300, 100);

            // 選取新增的ROI
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(addIndex), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

            // 同步dataGridView選取列
            UpdateDatGridViewROIAll(addIndex, enIndexType.graphicLabel);
        }

        #endregion --- AddToolStripMenuItem_Click ---

        #region --- CopyToolStripMenuItem_Click ---

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int centerX, centerY, radius;

            // 取得要複製的ROI
            centerX = Convert.ToInt32(dataGridViewROI.Rows[this.currentRowIndex].Cells[1].Value.ToString());
            centerY = Convert.ToInt32(dataGridViewROI.Rows[this.currentRowIndex].Cells[2].Value.ToString());
            radius = Convert.ToInt32(dataGridViewROI.Rows[this.currentRowIndex].Cells[3].Value.ToString());

            // 新增ROI
            int addIndex = (int)AddROI(centerX, centerY, radius);

            // 選取新增的ROI
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(addIndex), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

            // 同步dataGridView選取列
            UpdateDatGridViewROIAll(addIndex, enIndexType.graphicLabel);
        }

        #endregion --- CopyToolStripMenuItem_Click ---

        #region --- DeleteROIToolStripMenuItem_Click ---

        private void DeleteROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentRowIndex >= 0)
            {
                DelROI(dataGridViewROI.Rows[this.currentRowIndex].Cells[0].Value.ToString());

                this.currentRowIndex = -1;
                UpdateDatGridViewROIAll(this.currentRowIndex);
            }
        }

        #endregion --- DeleteROIToolStripMenuItem_Click ---

        #region --- DataGridViewROISelectRow ---

        /// <summary>
        /// 選取&標記指定的列
        /// </summary>
        /// <param name="rowIndex">要選取&標記的列號</param>
        ///
        private void DataGridViewROISelectRow(int rowIndex)
        {
            for (int i = 0; i < dataGridViewROI.Rows.Count; i++)
            {
                if (i != rowIndex)
                {
                    dataGridViewROI.Rows[i].Selected = false;
                }
                else
                {
                    // 紀錄當前選取的列
                    this.currentRowIndex = rowIndex;
                    dataGridViewROI.Rows[i].Selected = true;
                }
            }
        }

        #endregion --- DataGridViewROISelectRow ---

        #region --- dataGridViewROI_MouseDown ---

        private void dataGridViewROI_MouseDown(object sender, MouseEventArgs e)
        {
            // 選取列
            //DataGridViewROISelectRow(e.RowIndex);

            // 右鍵展開清單
            if (e.Button == MouseButtons.Right)
            {
                //contextMenuStripRoiEditor.Show(MousePosition.X, MousePosition.Y);
            }

            // 同步選取選中dataGridView的ROI
            int selectGraphicLabel;
            if (this.currentRowIndex < 0) return;
            if (dataGridViewROI.Rows.Count < 1) return;
            if (int.TryParse(dataGridViewROI.Rows[this.currentRowIndex].Cells[0].Value.ToString(), out selectGraphicLabel))
            {
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(selectGraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
            }
        }

        #endregion --- dataGridViewROI_MouseDown ---

        private enum enIndexType
        {
            graphicLabel,
            row,
        }

        #region --- UpdateDatGridViewROI ---

        /// <summary>
        /// 更新DataGridView資訊並選取指定列
        /// </summary>
        /// <param name="selectIndex">要選取的列,可不選取</param>
        private void UpdateDatGridViewROI(int selectIndex = -1, enIndexType selectType = enIndexType.row)
        {
            LightMeasurer measurer = null;
            int selectRow = -1;
            double tx, ty, tz, cx, cy;

            measurer = new LightMeasurer(MyMil.MilSystem);
            string[] row = null;

            // 更新DataGridView資訊
            foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
            {
                if (mu.RoiLableID == selectIndex + 1)
                {

                    if (GlobalVar.RoiIsRect)
                    {
                        RectRegionInfo roi = new RectRegionInfo();

                        // 設定ROI
                        roi.StartX = mu.X;
                        roi.StartY = mu.Y;
                        roi.Height = mu.H;
                        roi.Width = mu.W;

                        // 計算Luminance, Cx, Cy

                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (GlobalVar.LightMeasure.TristimulusImage.ImgX == MIL.M_NULL || GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL
                                || GlobalVar.LightMeasure.TristimulusImage.ImgZ == MIL.M_NULL)
                            {
                                tx = 0; ty = 0; tz = 0; cx = 0; cy = 0;
                            }
                            else
                            {
                                tx = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgX, roi);
                                ty = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                                tz = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgZ, roi);
                                                      
                                cx = tx / (tx + ty + tz);
                                cy = ty / (tx + ty + tz);
                            }

                            // 寫進DataGridView
                            //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                            row = new string[] { mu.RoiName, mu.X.ToString(), mu.Y.ToString(),
                            $"{mu.W}*{mu.H}", ty.ToString("0.0000"), cx.ToString("0.0000"), cy.ToString("0.0000") };
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                            {
                                ty = 0; ;
                            }
                            else
                            {
                                ty = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                            }

                            // 寫進DataGridView
                            //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();

                            row = new string[] { mu.RoiName, mu.X.ToString(), mu.Y.ToString(),
                            $"{mu.W}*{mu.H}", ty.ToString("0.0000") };
                        }
                    }
                    else
                    {
                        CircleRegionInfo roi = new CircleRegionInfo();

                        // 設定ROI
                        roi.CenterX = mu.CenterX;
                        roi.CenterY = mu.CenterY;
                        roi.Radius = mu.Radius;

                        // 計算Luminance, Cx, Cy

                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (GlobalVar.LightMeasure.TristimulusImage.ImgX == MIL.M_NULL || GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL
                                || GlobalVar.LightMeasure.TristimulusImage.ImgZ == MIL.M_NULL)
                            {
                                tx = 0; ty = 0; tz = 0; cx = 0; cy = 0;
                            }
                            else
                            {
                                tx = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgX, roi);
                                ty = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                                tz = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgZ, roi);
                                cx = tx / (tx + ty + tz);
                                cy = ty / (tx + ty + tz);
                            }

                            // 寫進DataGridView
                            //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                            row = new string[] { mu.RoiName, mu.CenterX.ToString(), mu.CenterY.ToString(),
                            mu.Radius.ToString(), ty.ToString("0.0000"), cx.ToString("0.0000"), cy.ToString("0.0000") };
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                            {
                                ty = 0; ;
                            }
                            else
                            {
                                ty = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                            }

                            // 寫進DataGridView
                            //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                            row = new string[] { mu.RoiName, mu.CenterX.ToString(), mu.CenterY.ToString(),
                            mu.Radius.ToString(), ty.ToString("0.0000")};
                        }
                    }


                    //dataGridViewROI.Rows[selectIndex].Cells
                    //dataGridViewROI.Rows[selectIndex].Cells[0].Value = mu.RoiName;             //ROI Name : Index
                    //    dataGridViewROI.Rows[selectIndex].Cells[1].Value = mu.CenterX.ToString();  //CenterX
                    //    dataGridViewROI.Rows[selectIndex].Cells[2].Value = mu.CenterY.ToString();  //CenterY
                    //    dataGridViewROI.Rows[selectIndex].Cells[3].Value = mu.Radius.ToString();   //Radius
                    //    dataGridViewROI.Rows[selectIndex].Cells[4].Value = ty.ToString("0.0000");    // Luminance

                    dataGridViewROI.Rows[selectIndex].SetValues(row);
                }

                if (selectIndex.ToString() == mu.RoiName)
                {
                    selectRow = dataGridViewROI.Rows.Count - 1;
                }
            }

            // 選取對應的列
            switch (selectType)
            {
                case enIndexType.graphicLabel:
                    {
                        DataGridViewROISelectRow(selectRow);
                    }
                    break;

                case enIndexType.row:
                    {
                        DataGridViewROISelectRow(selectIndex);
                    }
                    break;
            }
        }

        #endregion --- UpdateDatGridViewROI ---

        #region --- UpdateDatGridViewROIAll ---

        /// <summary>
        /// 更新DataGridView資訊並選取指定列
        /// </summary>
        /// <param name="selectIndex">要選取的列,可不選取</param>
        private void UpdateDatGridViewROIAll(int selectIndex = -1, enIndexType selectType = enIndexType.row)
        {
            dataGridViewROI.Rows.Clear();

            LightMeasurer measurer = null;
            int selectRow = -1;
            double tx, ty, tz, cx, cy;

            measurer = new LightMeasurer(MyMil.MilSystem);
            string[] row = null;

            // 更新DataGridView資訊
            foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
            {
                if (GlobalVar.RoiIsRect)
                {
                    RectRegionInfo roi = new RectRegionInfo();
                    // 設定ROI
                    roi.StartX = mu.X;
                    roi.StartY = mu.Y;
                    roi.Height = mu.H;
                    roi.Width = mu.W;

                    // 計算Luminance, Cx, Cy

                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                    {
                        if (GlobalVar.LightMeasure.TristimulusImage.ImgX == MIL.M_NULL || GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL
                            || GlobalVar.LightMeasure.TristimulusImage.ImgZ == MIL.M_NULL)
                        {
                            tx = 0; ty = 0; tz = 0; cx = 0; cy = 0;
                        }
                        else
                        {
                            tx = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgX, roi);
                            ty = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                            tz = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgZ, roi);
                            cx = tx / (tx + ty + tz);
                            cy = ty / (tx + ty + tz);
                        }

                        // 寫進DataGridView
                        //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                        row = new string[] { mu.RoiName, mu.X.ToString(), mu.Y.ToString(),
                    $"{mu.W}*{mu.H}", ty.ToString("0.0000"), cx.ToString("0.0000"), cy.ToString("0.0000") };
                    }
                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                    {
                        if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                        {
                            ty = 0; ;
                        }
                        else
                        {
                            ty = measurer.StatRectMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                        }

                        // 寫進DataGridView
                        //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                        row = new string[] {mu.RoiName, mu.X.ToString(), mu.Y.ToString(),
                    $"{mu.W}*{mu.H}", ty.ToString("0.0000")};
                    }

                    //row.Cells[0].Value = mu.RoiName;             //ROI Name : Index
                    //row.Cells[1].Value = mu.CenterX.ToString();  //CenterX
                    //row.Cells[2].Value = mu.CenterY.ToString();  //CenterY
                    //row.Cells[3].Value = mu.Radius.ToString();   //Radius
                    //row.Cells[4].Value = ty.ToString("0.00");    // Luminance
                    //row.Cells[5].Value = cx.ToString("0.0000");  //Cx
                    //row.Cells[6].Value = cy.ToString("0.0000");  //Cy

                    if (selectIndex.ToString() == mu.RoiName)
                    {
                        selectRow = dataGridViewROI.Rows.Count - 1;
                    }

                    dataGridViewROI.Rows.Add(row);
                }
                else
                {
                    CircleRegionInfo roi = new CircleRegionInfo();

                    // 設定ROI
                    roi.CenterX = mu.CenterX;
                    roi.CenterY = mu.CenterY;
                    roi.Radius = mu.Radius;

                    // 計算Luminance, Cx, Cy

                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                    {
                        if (GlobalVar.LightMeasure.TristimulusImage.ImgX == MIL.M_NULL || GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL
                            || GlobalVar.LightMeasure.TristimulusImage.ImgZ == MIL.M_NULL)
                        {
                            tx = 0; ty = 0; tz = 0; cx = 0; cy = 0;
                        }
                        else
                        {
                            tx = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgX, roi);
                            ty = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                            tz = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgZ, roi);
                            cx = tx / (tx + ty + tz);
                            cy = ty / (tx + ty + tz);
                        }

                        // 寫進DataGridView
                        //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                        row = new string[] { mu.RoiName, mu.CenterX.ToString(), mu.CenterY.ToString(),
                    mu.Radius.ToString(), ty.ToString("0.0000"), cx.ToString("0.0000"), cy.ToString("0.0000") };
                    }
                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                    {
                        if (GlobalVar.LightMeasure.TristimulusImage.ImgY == MIL.M_NULL)
                        {
                            ty = 0; ;
                        }
                        else
                        {
                            ty = measurer.StatCircleMean(GlobalVar.LightMeasure.TristimulusImage.ImgY, roi);
                        }

                        // 寫進DataGridView
                        //DataGridViewRow row = (DataGridViewRow)dataGridViewROI.Rows[0].Clone();
                        row = new string[] { mu.RoiName, mu.CenterX.ToString(), mu.CenterY.ToString(),
                    mu.Radius.ToString(), ty.ToString("0.0000")};
                    }

                    //row.Cells[0].Value = mu.RoiName;             //ROI Name : Index
                    //row.Cells[1].Value = mu.CenterX.ToString();  //CenterX
                    //row.Cells[2].Value = mu.CenterY.ToString();  //CenterY
                    //row.Cells[3].Value = mu.Radius.ToString();   //Radius
                    //row.Cells[4].Value = ty.ToString("0.00");    // Luminance
                    //row.Cells[5].Value = cx.ToString("0.0000");  //Cx
                    //row.Cells[6].Value = cy.ToString("0.0000");  //Cy

                    if (selectIndex.ToString() == mu.RoiName)
                    {
                        selectRow = dataGridViewROI.Rows.Count - 1;
                    }

                    dataGridViewROI.Rows.Add(row);
                }

            }

            // 選取對應的列
            switch (selectType)
            {
                case enIndexType.graphicLabel:
                    {
                        DataGridViewROISelectRow(selectRow);
                    }
                    break;

                case enIndexType.row:
                    {
                        DataGridViewROISelectRow(selectIndex);
                    }
                    break;
            }
        }

        #endregion --- UpdateDatGridViewROIAll ---

        #region --- AddROI ---

        private MIL_INT AddROI(int centerX, int centerY, int radius)
        {
            MIL_INT graphicsCount = 0;
            MIL_INT graphicsLabel = 0;

            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                graphicsLabel = MyMilDisplay.DrawCircle(centerX, centerY, radius);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref graphicsCount);

                MyMilIp.MeasureUnit unit = new MyMilIp.MeasureUnit();
                unit.RoiName = $"{graphicsCount}";
                unit.RoiLableID = graphicsLabel;
                unit.CenterX = (int)centerX;
                unit.CenterY = (int)centerY;
                unit.Radius = (int)radius;
                MyMilIp.ul.Add(unit);
            }

            return graphicsLabel;
        }

        private MIL_INT AddROI(int x = 300, int y = 300, int w = 0, int h = 0)
        {
            MIL_INT graphicsCount = 0;
            MIL_INT graphicsLabel = 0;

            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                graphicsLabel = MyMilDisplay.DrawRect(x, y, w, h);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref graphicsCount);

                MyMilIp.MeasureUnit unit = new MyMilIp.MeasureUnit();
                unit.RoiName = $"{graphicsCount}";
                unit.RoiLableID = graphicsLabel;
                unit.X = (int)(x);
                unit.Y = (int)(y);
                unit.W = (int)(w);
                unit.H = (int)(h);

                MyMilIp.ul.Add(unit);
            }

            return graphicsLabel;
        }

        #endregion --- AddROI ---

        #region --- DelROI ---

        private void DelROI(string graphicLabel)
        {
            try
            {
                MIL_INT label = Convert.ToInt32(graphicLabel);
                MyMilIp.ul.RemoveAll(t => t.RoiLableID == label);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(label), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
            }
            catch
            {
            }
        }

        #endregion --- DelROI ---

        #region --- RedrawROI ---

        private void RedrawROI()
        {
            if (!isRoiMode || MyMilDisplay.MilMainDisplayImage[1] == MIL.M_NULL) return;

            foreach (MyMilIp.MeasureUnit unit in MyMilIp.ul)
            {
                // 重新繪製ROI圈圈
                MIL_INT lableIndex = MyMilDisplay.DrawCircle(unit.CenterX, unit.CenterY, unit.Radius);

                // 更改graghic label
                unit.RoiName = $"{lableIndex}";
                unit.RoiLableID = lableIndex;
            }
        }

        #endregion --- RedrawROI ---

        #endregion --- ROI分析 ---

        #region --- 熱力圖 ---

        #region --- CbxHeatMap_CheckedChanged ---

        #endregion --- CbxHeatMap_CheckedChanged ---


        #region --- UpdateHeatMapControl ---

        /// <summary>
        /// 依照當前顯示影像顯示熱力圖
        /// 更新UI上熱力圖相關控件顯示
        /// </summary>


        #endregion --- UpdateHeatMapControl ---


        #endregion --- 熱力圖 ---

        #region --- 方法函式 ---

        #region --- LoadRecipe ---

        public static bool LoadRecipe(string RecipeName)
        {
            try
            {
                if (!File.Exists(GlobalVar.Config.ConfigPath + @"\" + RecipeName + @"\param_29M.xml")) return false;
                GlobalVar.OmsParam = GlobalVar.OmsParam.Read(GlobalVar.Config.ConfigPath + @"\" + RecipeName + @"\param_29M.xml");
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion --- LoadRecipe ---

        #region --- ShowSinglePointInfo ---

        private void ShowSinglePointInfo(int eX, int eY)
        {
            MIL_INT x = (MIL_INT)MyMilDisplay.MousePosX;
            MIL_INT y = (MIL_INT)MyMilDisplay.MousePosY;

            if (mousePosition.X == (int)MyMilDisplay.MousePosX && mousePosition.Y == (int)MyMilDisplay.MousePosY)
            {
                return;
            }
            else
            {
                mousePosition = new Point((int)MyMilDisplay.MousePosX, (int)MyMilDisplay.MousePosY);
            }

            if (x >= 0 && y >= 0)
            {
                if (!lbPanelShowMsg.Visible)
                    lbPanelShowMsg.Show();

                lbPanelShowMsg.Text = $"X = {MyMilDisplay.MousePosX.ToString("0")}, Y = {MyMilDisplay.MousePosY.ToString("0")}";

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    if (this.isIdentifyValueOK &&
                        GlobalVar.LightMeasure.TristimulusImage.ImgX != MIL.M_NULL &&
                        GlobalVar.LightMeasure.TristimulusImage.ImgY != MIL.M_NULL &&
                        GlobalVar.LightMeasure.TristimulusImage.ImgZ != MIL.M_NULL)
                    {
                        float[] cur_tx = new float[1] { 0 };
                        float[] cur_ty = new float[1] { 0 };
                        float[] cur_tz = new float[1] { 0 };

                        MIL.MbufGet2d(GlobalVar.LightMeasure.TristimulusImage.ImgX, (MIL_INT)(Math.Round(MyMilDisplay.MousePosX, 0)), (MIL_INT)(Math.Round(MyMilDisplay.MousePosY, 0)), 1, 1, cur_tx);
                        MIL.MbufGet2d(GlobalVar.LightMeasure.TristimulusImage.ImgY, (MIL_INT)(Math.Round(MyMilDisplay.MousePosX, 0)), (MIL_INT)(Math.Round(MyMilDisplay.MousePosY, 0)), 1, 1, cur_ty);
                        MIL.MbufGet2d(GlobalVar.LightMeasure.TristimulusImage.ImgZ, (MIL_INT)(Math.Round(MyMilDisplay.MousePosX, 0)), (MIL_INT)(Math.Round(MyMilDisplay.MousePosY, 0)), 1, 1, cur_tz);

                        double curCx = cur_tx[0] / (cur_tx[0] + cur_ty[0] + cur_tz[0]);
                        double curCy = cur_ty[0] / (cur_tx[0] + cur_ty[0] + cur_tz[0]);

                        string lum = string.Format("Lum = {0} nits", cur_ty[0].ToString("0.0000"));
                        string cx = string.Format("Cx = {0}", curCx.ToString("0.0000"));
                        string cy = string.Format("Cy = {0}", curCy.ToString("0.0000"));
                        lbPanelShowMsg.Text += $"\r\n{lum}\r\n{cx}, {cy}";

                        if (eX > PnlGrabViewer.Width / 2)
                        {
                            eX = eX - lbPanelShowMsg.Width - 20;
                        }
                        if (eY > PnlGrabViewer.Height / 2)
                        {
                            eY = eY - lbPanelShowMsg.Height - 50;
                        }
                        lbPanelShowMsg.Location = new Point(eX + 20, eY + 50);

                    }
                }
                else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                {
                    if (this.isIdentifyValueOK && GlobalVar.LightMeasure.TristimulusImage.ImgY != MIL.M_NULL)
                    {
                        float[] cur_ty = new float[1] { 0 };

                        MIL.MbufGet2d(GlobalVar.LightMeasure.TristimulusImage.ImgY, (MIL_INT)(Math.Round(MyMilDisplay.MousePosX, 0)), (MIL_INT)(Math.Round(MyMilDisplay.MousePosY, 0)), 1, 1, cur_ty);

                        string lum = string.Format("Lum = {0} nits", cur_ty[0].ToString("0.0000"));

                        lbPanelShowMsg.Text += $"\r\n{lum}";

                        if (eX > PnlGrabViewer.Width / 2)
                        {
                            eX = eX - lbPanelShowMsg.Width - 20;
                        }
                        if (eY > PnlGrabViewer.Height / 2)
                        {
                            eY = eY - lbPanelShowMsg.Height - 50;
                        }
                        lbPanelShowMsg.Location = new Point(eX + 20, eY + 50);


                    }
                }
            }
            else
            {
                lbPanelShowMsg.Hide();
            }
        }

        #endregion --- ShowSinglePointInfo ---

        #region --- CheckFileIsOpen ---

        private void CheckFileIsOpen(string filename)
        {
            try
            {
                using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {   // 測試檔案可被開啟後，關閉Stream
                    fileStream.Close();
                }
            }
            catch (IOException)
            {
                // 文件已被開啟
                throw new Exception(string.Format($"File: [{filename}] was open, close file first, before saving !"));
            }
        }

        #endregion --- CheckFileIsOpen ---

        #region --- ROIAnalysis_UIReset ---

        private void ROIAnalysis_UIReset()
        {
            if (splitContainer1.Panel1Collapsed)
            {
                this.btnDrawRoi.PerformClick();

                // 清除ROI List
                MyMilIp.ul.Clear();
            }


        }

        #endregion --- ROIAnalysis_UIReset ---

        #region --- GetAllCalibrationFile ---

        private List<string> GetAllCalibrationFile()
        {
            try
            {
                List<string> calibrationFiles = new List<string>();
                string calibrationFile = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
                string[] files = Directory.GetFiles(calibrationFile);

                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".xml")
                    {
                        calibrationFiles.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }

                return calibrationFiles;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);

                return new List<string>();

            }

        }

        #endregion --- GetAllCalibrationFile ---

        #region --- ROIAnalysis_UIReset ---
        private void CheckFFC()
        {
            //if (this.bootRecipe.Camera_Type == "M_SYSTEM_HOST") return;

            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;
            MIL_INT SizeX_temp = 0;
            MIL_INT SizeY_temp = 0;
            GlobalVar.Config.RecipeName = toolStripMenuRecipeChoose.Text;
            GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";
            string result = "";
            string DarkPath = "";
            string[] XyzFilterPos = { "X", "Y", "Z" };

            SizeX = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
            SizeY = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

            for (int NdFilterPos = 0; NdFilterPos < 4; NdFilterPos++)
            {
                for (int i = 0; i < 3; i++)
                {
                    DarkPath = $@"{GlobalVar.Config.RecipePath}\FFC\ND{NdFilterPos}_Dark\{XyzFilterPos[i]}.tif";

                    if (File.Exists(DarkPath))
                    {
                        SizeX_temp = MIL.MbufDiskInquire(DarkPath, MIL.M_SIZE_X, MIL.M_NULL);
                        SizeY_temp = MIL.MbufDiskInquire(DarkPath, MIL.M_SIZE_Y, MIL.M_NULL);
                        if (SizeX_temp != SizeX)
                            result += $@"Image: [{DarkPath}], Image SizeX[{SizeX_temp}] was not equal to Grab Image SizeX [{SizeX}] !" + Environment.NewLine;

                        if (SizeY_temp != SizeY)
                            result += $@"Image: [{DarkPath}], Image SizeX[{SizeY_temp}] was not equal to Grab Image SizeY [{SizeY}] !" + Environment.NewLine;
                    }
                }

                if (result != "")
                    MessageBox.Show(result);
            }

        }
        #endregion --- ROIAnalysis_UIReset ---


        #endregion --- 方法函式 ---

        #region --- UI ---

        #region --- Button Event ---

        #region --- btn_dataGridViewROI_Load_Click ---

        private void btn_dataGridViewROI_Load_Click(object sender, EventArgs e)
        {
            GlobalVar.RoiIsRect = false;

            if (MyMilIp.ul == null)
            {
                this.btnDrawRoi.Focus();
                MessageBox.Show("Button ROI Analysis First !", "ROI Analysis");
                return;
            }

            if (tbxCalibrationFile.Text == "")
            {
                this.tbxCalibrationFile.Focus();
                MessageBox.Show("Select Calibration File First !", "ROI Analysis");
                return;
            }

            //string calibrationFile = "correctionConfig";
            string calibrationFile = tbxCalibrationFile.Text;
            int radius = 0;

            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

            if (calibrationFile == "自動曝光")
            {
                this.dataGridViewROI.Columns[3].HeaderText = "Size";

                GlobalVar.RoiIsRect = true;

                int X = GlobalVar.OmsParam.AutoExposureRoiX;
                int Y = GlobalVar.OmsParam.AutoExposureRoiY;
                int W = GlobalVar.OmsParam.AutoExposureRoiWidth;
                int H = GlobalVar.OmsParam.AutoExposureRoiHeight;

                // 新增ROI
                int addIndex = (int)AddROI(X, Y, W, H);

                // 選取新增的ROI
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(addIndex), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

                // 同步dataGridView選取列
                UpdateDatGridViewROIAll(addIndex, enIndexType.graphicLabel);
            }
            else if (calibrationFile == "Recipe")
            {
                this.dataGridViewROI.Columns[3].HeaderText = "Size";

                GlobalVar.RoiIsRect = true;

                int X = GlobalVar.OmsParam.ThirdPartyRoiX;
                int Y = GlobalVar.OmsParam.ThirdPartyRoiY;
                int W = GlobalVar.OmsParam.ThirdPartyRoiWidth;
                int H = GlobalVar.OmsParam.ThirdPartyRoiHeight;

                // 新增ROI
                int addIndex = (int)AddROI(X, Y, W, H);

                // 選取新增的ROI
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(addIndex), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

                // 同步dataGridView選取列
                UpdateDatGridViewROIAll(addIndex, enIndexType.graphicLabel);
            }
            else
            {
                this.dataGridViewROI.Columns[3].HeaderText = "Radius";

                string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
                string fullFileName = $@"{oneColorCalibrationFolder}\{calibrationFile}.xml";

                if (File.Exists(fullFileName))
                {
                    MyMilIp.ul.Clear();

                    MyMilIp.correctionConfig = new LightMeasureConfig();
                    MyMilIp.correctionConfig.ReadWithoutCrypto(fullFileName);

                    foreach (FourColorData dt in MyMilIp.correctionConfig.CorrectionData.DataList)
                    {
                        radius = dt.CircleRegionInfo.Radius;

                        // 新增ROI
                        int addIndex = (int)AddROI(dt.CircleRegionInfo.CenterX, dt.CircleRegionInfo.CenterY, radius);

                        // 選取新增的ROI
                        MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                        MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(addIndex), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

                        // 同步dataGridView選取列
                        UpdateDatGridViewROIAll(addIndex, enIndexType.graphicLabel);
                    }
                }
                else
                {
                    MessageBox.Show("There was error opening this document.");
                }
            }
        }

        #endregion --- btn_dataGridViewROI_Load_Click ---

        #region --- btn_dataGridViewROI_Clear_Click ---

        private void btn_dataGridViewROI_Clear_Click(object sender, EventArgs e)
        {
            // 清除ROI List
            if (MyMilIp.ul != null)
            {
                // 清除ROI List
                MyMilIp.ul.Clear();

                // 重設顯示
                //MyMilDisplay.SetMainDisplayFit(MyMilDisplay.MilMainDisplayImage[1], this.PnlGrabViewer.Handle);

                // 重置DataGridView
                this.currentRowIndex = -1;
                UpdateDatGridViewROIAll(this.currentRowIndex);

                // 清除畫布
                MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);
            }
        }

        #endregion --- btn_dataGridViewROI_Clear_Click ---

        #region --- btn_dataGridViewROI_SaveCSV_Click ---

        private void btn_dataGridViewROI_SaveCSV_Click(object sender, EventArgs e)
        {
            StreamWriter writer = null;
            StringBuilder strBuilder = null;
            SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            string filename = "";

            try
            {
                //
                saveFileDialog1.Title = "Save csv Files";
                saveFileDialog1.CheckPathExists = true;
                saveFileDialog1.DefaultExt = "csv";
                saveFileDialog1.Filter = "CSV file (*.csv)|*.csv";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Check if file was open
                    if (File.Exists(saveFileDialog1.FileName))
                        this.CheckFileIsOpen(saveFileDialog1.FileName);

                    writer = new StreamWriter(saveFileDialog1.FileName);
                    strBuilder = new StringBuilder();
                    DataGridViewRow row = null;

                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                    {
                        strBuilder.Append(String.Format("{0},{1},{2},{3},{4},{5},{6} \r\n",
                            "Name", "CenterX", "CenterY", "Radius", "Luminance", "Cx", "Cy"));

                        for (int i = 0; i < dataGridViewROI.Rows.Count; i++)
                        {
                            if (dataGridViewROI.Rows[i] != null)
                            {
                                row = (DataGridViewRow)dataGridViewROI.Rows[i];
                                strBuilder.Append(String.Format("{0},{1},{2},{3},{4},{5},{6} \r\n",
                                    $"Circle_" + row.Cells[0].Value.ToString(),
                                    row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value,
                                    row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value));
                            }
                        }
                    }
                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                    {
                        strBuilder.Append(String.Format("{0},{1},{2},{3},{4} \r\n",
                            "Name", "CenterX", "CenterY", "Radius", "Luminance"));

                        for (int i = 0; i < dataGridViewROI.Rows.Count; i++)
                        {
                            if (dataGridViewROI.Rows[i] != null)
                            {
                                row = (DataGridViewRow)dataGridViewROI.Rows[i];
                                strBuilder.Append(String.Format("{0},{1},{2},{3},{4} \r\n",
                                    $"Circle_" + row.Cells[0].Value.ToString(),
                                    row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value,
                                    row.Cells[4].Value));
                            }
                        }
                    }

                    filename = saveFileDialog1.FileName;
                    writer.WriteLine(strBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
            finally
            {
                saveFileDialog1 = null;

                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }
        }

        #endregion --- btn_dataGridViewROI_SaveCSV_Click ---

        #endregion --- Button Event ---

        #region --- toolStripMenuItem ---

        #region --- System ---

        private void paremeterSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSystemSetting frmSystemSetting = new frmSystemSetting();
            frmSystemSetting.Show();

            frmSystemSetting.UpdataSD -= FrmSystemSetting_UpdataSD;
            frmSystemSetting.UpdataSD += FrmSystemSetting_UpdataSD;
        }

        private void FrmSystemSetting_UpdataSD()
        {
            this.Set_UI_MeasureType();
        }

        private void addRecipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(string.Format(GlobalVar.Config.ConfigPath + @"\{0}", toolStripTextBox_NewRecipeName.Text)) == false)
                Directory.CreateDirectory(string.Format(GlobalVar.Config.ConfigPath + @"\{0}", toolStripTextBox_NewRecipeName.Text));
        }

        private void AboutAUOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutAUO aboutAUO = new frmAboutAUO();
            aboutAUO.ShowDialog();
        }


        #endregion --- System ---

        #region --- 29M ---

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (toolStripMenuRecipeChoose.Text == "")
            {
                toolStripMenuRecipeChoose.Focus();
                MessageBox.Show("Please select recipe!");
            }
            else
            {
                frmParamSetting _29MSettingForm = new frmParamSetting(toolStripMenuRecipeChoose.Text, this.Info,ref grabber);
                _29MSettingForm.ShowDialog();
            }
        }

        private void toolStripMenuManualForm_Click(object sender, EventArgs e)
        {
            // 清除 ROI 顯示
            this.btn_dataGridViewROI_Clear_Click(btn_dataGridViewROI_Clear, null);

            if (toolStripMenuRecipeChoose.Text == "")
            {
                toolStripMenuRecipeChoose.Focus();
                MessageBox.Show("Please select recipe!");
            }
            else
            {
                Form existingForm = Application.OpenForms.OfType<ManualForm>().FirstOrDefault();

                if (existingForm != null)
                {
                    // 如果找到已存在的表單
                    if (existingForm.WindowState == FormWindowState.Minimized)
                    {
                        existingForm.WindowState = FormWindowState.Normal; // 如果最小化則恢復
                    }
                    existingForm.Focus(); // 將焦點設置到現有表單
                }
                else
                {
                    HardwareUnit FormPara = new HardwareUnit {
                        Grabber = this.grabber,
                        Motor = this.motorControl,
                        PLC = this.X_PLC,
                        D65 = this.D65_Light,
                        Robot = this.Robot,
                        SR3 = this.SR3,
                        CornerstoneB = this.CornerstoneB,
                        LightSourceA = this.LightSourceA,
                        PowerSupply = this.PowerSupply,
                    };

                    ManualForm TestForm = new ManualForm(FormPara, Info);
                    TestForm.Show();
                }
            }
        }

        #endregion --- 29M ---

        #region --- Recipe choose ---

        private void recipechoosetoolStripMenuItem_DropDown(object sender, EventArgs e)
        {
            toolStripMenuRecipeChoose.Items.Clear();

            DirectoryInfo dir = new DirectoryInfo(GlobalVar.Config.ConfigPath);
            if (!dir.Exists)
                dir.Create();
            DirectoryInfo[] subDir = dir.GetDirectories();
            foreach (DirectoryInfo d in subDir)
                toolStripMenuRecipeChoose.Items.Add(d.Name);

        }

        private void recipechoosetoolStripMenuItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (toolStripMenuRecipeChoose.Text != "")
                {
                    GlobalVar.Config.RecipeName = toolStripMenuRecipeChoose.Text;

                    // Check FFC
                    this.CheckFFC();

                    LoadRecipe(GlobalVar.Config.RecipeName);
                }
            }
            catch
            {
            }
        }

        #endregion --- Recipe choose ---

        #region --- Setting ---




        #endregion --- Setting ---

        #endregion --- toolStripMenuItem ---

        #region --- PnlGrabViewer ---

        #region --- PnlGrabViewer_MouseMove---

        private void PnlGrabViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isRun) return;

            //if (isRoiMode)
            //{
            if (e.Button == MouseButtons.Left)
            {
                if (this.graphicSelectedLabel == -1) return;
                if (MyMilIp.ul.Count < 1) return;

                int index = MyMilIp.ul.FindIndex(s => s.RoiLableID == this.graphicSelectedLabel);

                 index = 0;


                if (GlobalVar.RoiIsRect)
                {
                    double X = 0;
                    double Y = 0;
                    double W = 0;
                    double H = 0;

                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref X);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref Y);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_HEIGHT + MIL.M_TYPE_MIL_DOUBLE, ref H);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_WIDTH + MIL.M_TYPE_MIL_DOUBLE, ref W);

                    if (index != -1)
                    {
                        MyMilIp.ul[index].X = (int)X;
                        MyMilIp.ul[index].Y = (int)Y;
                        MyMilIp.ul[index].W = (int)W;
                        MyMilIp.ul[index].H = (int)H;
                    }

                }
                else
                {
                    double centerX = 0;
                    double centerY = 0;
                    double radius = 0;

                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_CENTER_X, ref centerX);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_CENTER_Y, ref centerY);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.graphicSelectedLabel), MIL.M_DEFAULT, MIL.M_RADIUS_X, ref radius);

                    if (index != -1)
                    {
                        MyMilIp.ul[index].CenterX = (int)centerX;
                        MyMilIp.ul[index].CenterY = (int)centerY;
                        MyMilIp.ul[index].Radius = (int)radius;
                    }
                }


                UpdateDatGridViewROI(this.currentRowIndex);
            }
            //}
            //else
            //{
            ShowSinglePointInfo(e.X, e.Y);
            //}
        }

        #endregion --- PnlGrabViewer_MouseMove---

        #region --- PnlGrabViewer_MouseLeave---

        private void PnlGrabViewer_MouseLeave(object sender, EventArgs e)
        {        
            PnlGrabViewer.Cursor = Cursors.Default;

            this.lbPanelShowMsg.Text = "";

        }

        #endregion --- PnlGrabViewer_MouseLeave---

        #region --- PnlGrabViewer_MouseEnter---

        private void PnlGrabViewer_MouseEnter(object sender, EventArgs e)
        {         
            try
            {
                if (isRoiMode)
                {
                    PnlGrabViewer.Cursor = Cursors.Cross;
                }
                else
                {


                    this.isIdentifyValueOK = true;
                }
            }
            catch
            {
                this.isIdentifyValueOK = false;
            }
        }

        #endregion --- PnlGrabViewer_MouseEnter---

        #endregion --- PnlGrabViewer ---

        #region --- ComboBox Event ---

        #region --- CbxImageType_SelectedIndexChanged ---



        #endregion --- CbxImageType_SelectedIndexChanged ---

        #endregion --- ComboBox Event ---

        #endregion --- UI ---

        #region --- Event ---

        #region --- SetProcessInfoEvent ---

        /// <summary>
        /// 取得使用者在frmProcessInfoSetting設定的資訊
        /// </summary>
        /// <param name="pattern">跑片畫面</param>
        /// <param name="panelId">片號(目前未使用)</param>
        /// <param name="action">執行流程</param>
        /// <param name="useNd">預計要使用的ND</param>
        /// <param name="calibrationFile">預計要使用的calibration File</param>
        private void SetProcessInfoEvent(string pattern, string panelId, string action, string calibrationFile, string spectralType, string ffcFile)
        {
            if (isRun) return;

            taskOneColorDataCreate = null;
            taskOneColorCalibration = null;
            taskNdPrediction = null;

            GlobalVar.ProcessInfo.Pattern = pattern;
            GlobalVar.ProcessInfo.PanelID = panelId;
            GlobalVar.ProcessInfo.Action = action;
            GlobalVar.ProcessInfo.CalibrationFile = calibrationFile;
            GlobalVar.ProcessInfo.FFCFile = ffcFile;

            MyMilDisplay.SetMainDisplayFit(this.grabber.grabImage, PnlGrabViewer.Handle);

            switch (action)
            {
                case "One Color Correction Data Create":
                    {
                        taskOneColorDataCreate = new TaskOneColorDataCreate(GlobalVar.OmsParam, this.Info, this.grabber, this.motorControl);
                        taskOneColorDataCreate.State = TaskOneColorDataCreate.EnState.Init;
                    }
                    break;

                case "One Color Correction (Calibration)":
                    {
                        taskOneColorCalibration = new TaskOneColorCalibration(GlobalVar.OmsParam, EnumLightConvertType.CorrFile, this.Info, this.grabber, this.motorControl);
                        taskOneColorCalibration.State = TaskOneColorCalibration.EnState.Init;
                    }
                    break;

                case "One Color Correction (Spectral)":
                    {
                        taskOneColorCalibration = new TaskOneColorCalibration(GlobalVar.OmsParam, EnumLightConvertType.Spectral, this.Info, this.grabber, this.motorControl);
                        taskOneColorCalibration.State = TaskOneColorCalibration.EnState.Init;
                    }
                    break;

                case "Predict The ND-Filter To Use":
                    {
                        taskNdPrediction = new TaskNdPrediction(GlobalVar.OmsParam, this.Info, this.grabber, this.motorControl);
                        taskNdPrediction.State = TaskNdPrediction.EnState.Init;
                    }
                    break;
            }

            isRun = true;
            mainThread = new Thread(this.MainProc);
            mainThread.Start();

        }

        #endregion --- SetProcessInfoEvent ---

        #region --- GraphicSelectValueChangedEvent ---

        private void GraphicSelectValueChangedEvent(object obj)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<object>(GraphicSelectValueChangedEvent), obj);
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
            }
            else
            {
                for (int i = 0; i < dataGridViewROI.Rows.Count; i++)
                {
                    this.graphicSelectedLabel = Convert.ToInt32(dataGridViewROI.Rows[i].Cells[0].Value.ToString());

                    if (this.graphicSelectedLabel == (int)obj)
                    {
                        this.DataGridViewROISelectRow(i);
                        break;
                    }
                }
            }
        }



        #endregion --- GraphicSelectValueChangedEvent ---

        #endregion --- Event ---

        private void CorrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCorrSetting frmCorrSetting = new frmCorrSetting();
            frmCorrSetting.Show();

            frmCorrSetting.UpdataSD -= FrmSystemSetting_UpdataSD;
            frmCorrSetting.UpdataSD += FrmSystemSetting_UpdataSD;
        }

    }

}