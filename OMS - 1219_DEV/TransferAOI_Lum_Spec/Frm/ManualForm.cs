using LightMeasure;
using MaterialSkin;
using MaterialSkin.Controls;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using HardwareManager;
using CommonBase.Logger;
using static MyMilIp;
using BaseTool;
using FrameGrabber;
using UrRobotForm;
using CommonSettings;

namespace OpticalMeasuringSystem
{
    public partial class ManualForm : MaterialForm
    {
        private MilDisplayManager MDisplay;

        private EnumMotoControlPackage MotorType = GlobalVar.DeviceType;

        private bool liveViewFlag = false;

        InfoManager Info = null;

        // Hardware
        public MilDigitizer Grabber = null;
        private IMotorControl Motor = null;
        private X_PLC_Ctrl PLC = null;
        private D65_Light_Ctrl D65 = null;
        private SR3_Ctrl SR3 = null;
        private PowerSupply_Ctrl PowerSupply = null;

        private CornerstoneB_Ctrl CornerstoneB = null;

        private LightSourceA_Ctrl LightSourceA = null;
        private ClsUrRobotInterface Robot;

        // AIC Calibration


        private MIL_ID tifImg = MIL.M_NULL;

        private Thread liveViewThread;

        private ASCOM_FilterCtrl xyzFilter;
        private ASCOM_FilterCtrl ndFilter;

        private delegate string GetXyzFilterPosCallback();

        private delegate string GetNdFilterPosCallback();

        private MaterialTextBox corrTextBox = null;

        private SerialPort PatGen = null;

        private delegate void SetLabelMessage(Label label, string sValue);
        private delegate void SetLabelBGColor(Label label, System.Drawing.Color color);
        private delegate void SetListViewMessage(ListView listView, string sValue);
        private delegate void SetNumericMessage(NumericUpDown numeric, double dValue);

        //Task

        private Task_AutoFocus AutoFocus = null;
        private Task_AutoFocus_New AutoFocus_New = null;

        private Task_AutoPlane AutoPlane = null;
        private Task_VCurve VCurve = null;

        private Task_FFC_Create FFC_Create = null;
        private FFC_FlowPara FlowPara_FFC = new FFC_FlowPara();

        private Task_LightSourceA_Create LightSourceA_Create = null;
        private LightSourceA_Create_FlowPara FlowPara_LightSourceA = new LightSourceA_Create_FlowPara();

        private Task_NoiseDetection NoiseDetection = null;
        private NoiseDetection_FlowPara FlowPara_NoiseDetection = new NoiseDetection_FlowPara();

        private Task_TiltCorrection TiltCorrection = null;
        private TiltCorrection_FlowPara FlowPara_TiltCorrection = new TiltCorrection_FlowPara();

        private Task_MonoChromatorMeasure MonoChromatorMeasure = null;
        private MonoChromatorMeasure_FlowPara FlowPara_MonoChromatorMeasure = new MonoChromatorMeasure_FlowPara();

        private Task_AutoPattern AutoPattern = null;
        private AutoPattern_FlowPara FlowPara_AutoPattern = new AutoPattern_FlowPara();

        private Task_AutoWD AutoWD = null;
        private AutoWD_FlowPara FlowPara_AutoWD = new AutoWD_FlowPara();

        public ManualForm(HardwareUnit Para, InfoManager info)
        {
            InitializeComponent();

            this.Grabber = Para.Grabber;
            this.Motor = Para.Motor;
            this.Robot = Para.Robot;
            this.PLC = Para.PLC;
            this.D65 = Para.D65;
            this.SR3 = Para.SR3;
            this.PowerSupply = Para.PowerSupply;
            this.CornerstoneB = Para.CornerstoneB;
            this.LightSourceA = Para.LightSourceA;
            this.Info = info;

            if (this.Motor != null)
            {
                this.Motor.UpdateStatus -= MotorControl_UpdateStatus;
                this.Motor.UpdateStatus += MotorControl_UpdateStatus;
            }

            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.xyzFilter = GlobalVar.Device.XyzFilter;
                        this.ndFilter = GlobalVar.Device.NdFilter;
                    }
                    break;
            }

            //Update UI
            this.UpdateUI();
            MDisplay = new MilDisplayManager(ref MyMil.MilSystem);
            MDisplay.UpdateMousePara -= MDisplay_UpdateMousePara;
            MDisplay.UpdateMousePara += MDisplay_UpdateMousePara;
            MDisplay.Init();
            MDisplay.SetWindow(ref this.Grabber.grabImage, ref PnlGrabViewer);

            SetDevice();
            TaskInit(Para);

            this.HideUI();
        }

        public void TaskInit(HardwareUnit HardwarePara)
        {
            //AutoFocus
            AutoFocus = new Task_AutoFocus(Grabber, Motor, Info);
            AutoFocus.UpdateFocusValue -= TaskAutoFocus_UpdateFocusValue;
            AutoFocus.UpdateFocusValue += TaskAutoFocus_UpdateFocusValue;

            //AutoFocus
            AutoFocus_New = new Task_AutoFocus_New(Grabber, Motor, Info);
            AutoFocus_New.UpdateFocusValue -= TaskAutoFocus_UpdateFocusValue;
            AutoFocus_New.UpdateFocusValue += TaskAutoFocus_UpdateFocusValue;

            //VCurve
            VCurve = new Task_VCurve(Grabber, Motor, Info);

            //AutoPlane
            AutoPlane = new Task_AutoPlane(MyMil.MilSystem, Grabber, this.Robot, this.Info, GlobalVar.SD.NormalAngle_0D);

            //FFC Create
            FFC_Create = new Task_FFC_Create(Info, Motor, D65, Robot, PLC, Grabber);
            FFC_Create.MonitorStep -= FFC_MonitorStep;
            FFC_Create.MonitorStep += FFC_MonitorStep;
            FFC_Create.LogMsg -= FFC_LogMsg;
            FFC_Create.LogMsg += FFC_LogMsg;

            PropertyGrid_FFC_Create_Config.SelectedObject = FlowPara_FFC;

            //LightSourceA Create
            LightSourceA_Create = new Task_LightSourceA_Create(Info, HardwarePara);
            LightSourceA_Create.MonitorStep -= LightSourceA_MonitorStep;
            LightSourceA_Create.MonitorStep += LightSourceA_MonitorStep;
            LightSourceA_Create.LogMsg -= LightSourceA_LogMsg;
            LightSourceA_Create.LogMsg += LightSourceA_LogMsg;
            PropertyGrid_LightSourceA_Create_Config.SelectedObject = FlowPara_LightSourceA.BaseAlign_Para;

            //NoiseDetection Create
            NoiseDetection = new Task_NoiseDetection(Info, HardwarePara);
            NoiseDetection.MonitorStep -= NoiseDetection_MonitorStep;
            NoiseDetection.MonitorStep += NoiseDetection_MonitorStep;
            NoiseDetection.LogMsg -= NoiseDetection_LogMsg;
            NoiseDetection.LogMsg += NoiseDetection_LogMsg;
            PropertyGrid_NoiseDetection_Config.SelectedObject = FlowPara_NoiseDetection.BaseAlign_Para;

            //TiltCorrection
            TiltCorrection = new Task_TiltCorrection(Info, HardwarePara);
            TiltCorrection.MonitorStep -= TiltCorrection_MonitorStep;
            TiltCorrection.MonitorStep += TiltCorrection_MonitorStep;
            TiltCorrection.LogMsg -= TiltCorrection_LogMsg;
            TiltCorrection.LogMsg += TiltCorrection_LogMsg;
            PropertyGrid_TiltCorrection_Config.SelectedObject = FlowPara_TiltCorrection.BaseAlign_Para;

            //MonoChraoatorMeasure
            MonoChromatorMeasure = new Task_MonoChromatorMeasure(Info, HardwarePara);
            MonoChromatorMeasure.MonitorStep -= MonoChromatorMeasure_MonitorStep;
            MonoChromatorMeasure.MonitorStep += MonoChromatorMeasure_MonitorStep;
            MonoChromatorMeasure.LogMsg -= MonoChromatorMeasure_LogMsg;
            MonoChromatorMeasure.LogMsg += MonoChromatorMeasure_LogMsg;
            PropertyGrid_MonoChromatorMeasure_Config.SelectedObject = FlowPara_MonoChromatorMeasure;

            //AutoPattern
            AutoPattern = new Task_AutoPattern(Info, HardwarePara);
            AutoPattern.MonitorStep -= AutoPattern_MonitorStep;
            AutoPattern.MonitorStep += AutoPattern_MonitorStep;
            AutoPattern.LogMsg -= AutoPattern_LogMsg;
            AutoPattern.LogMsg += AutoPattern_LogMsg;
            PropertyGrid_AutoPattern_Config.SelectedObject = FlowPara_AutoPattern;

            //AutoPattern
            AutoWD = new Task_AutoWD(Info, HardwarePara);
            AutoWD.MonitorStep -= AutoWD_MonitorStep;
            AutoWD.MonitorStep += AutoWD_MonitorStep;
            AutoWD.LogMsg -= AutoWD_LogMsg;
            AutoWD.LogMsg += AutoWD_LogMsg;
            PropertyGrid_AutoWD_Config.SelectedObject = FlowPara_AutoWD;
        }



        private void MDisplay_UpdateMousePara(int X, int Y, int Gray)
        {
            lbPanelShowMsg.Invoke(new Action(() => {
                lbPanelShowMsg.Text = $"X = {X}, Y = {Y}, Value ={Gray}";
            }));
        }

        private void SetDevice()
        {
            switch (GlobalVar.DeviceType)
            {
                case EnumMotoControlPackage.MS5515M:
                    {
                        Btn_MotorTool.Text = "MS5515M Tool";
                        Btn_FwTool.Parent = null;
                    }
                    break;

                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        Btn_MotorTool.Text = "Auo Motor Tool";
                        Btn_MotorTool.Enabled = true;
                        Btn_FwTool.Parent = null;
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    {
                        Btn_MotorTool.Text = "Airy Motor Tool";
                        Btn_FwTool.Parent = null;
                    }
                    break;

                case EnumMotoControlPackage.CircleTac:
                    {
                        Btn_MotorTool.Text = "CircleTac Tool";
                        Btn_FwTool.Parent = null;
                    }
                    break;
            }


        }

        private void TaskAutoFocus_UpdateFocusValue(int Pos1, double Value1, bool Finish)
        {
            if (!IsDisposed)
            {
                this.lbl_FocuserPosition.Invoke(new Action(() => { this.lbl_FocuserPosition.Text = Pos1.ToString(); }));
                this.lbl_FocuserValue.Invoke(new Action(() => { this.lbl_FocuserValue.Text = Value1.ToString(); }));
            }
        }

        private void MotorControl_UpdateStatus(MotorInfo Focuser, MotorInfo Aperture, MotorInfo FW1, MotorInfo FW2)
        {
            if (!IsDisposed)
            {
                if (Focuser != null)
                {
                    this.lbl_FocuserPosition.Invoke(new Action(() => {
                        this.lbl_FocuserPosition.Text = $"{Focuser.Position}";
                    }));
                }
                ;

                if (Aperture != null)
                {

                    this.lbl_AperturePosition.Invoke(new Action(() => {
                        this.lbl_AperturePosition.Text = $"{Aperture.Position}";
                    }));
                }
                ;
            }
        }


        private void _29M_Test_Form_Load(object sender, EventArgs e)
        {
            try
            {
                Grabber.SetExposureTime(GlobalConfig.UserKeyinExpTime);
                double ExpTime = Grabber.GetExposureTime();
                mtbxExposureTime.Text = $"{ExpTime}";

                double Gain = Grabber.GetGain();
                mtbxGain.Text = $"{Gain}";
            }
            catch (Exception)
            {

            }




            // Initial Camera Gain
            if (this.Grabber != null)
            {
                //this.Grabber.SetCameraGain(1);
                //this.mtbxGain.Text = "1";
            }
            else
            {
                this.mbtnLive.Enabled = false;
                this.mbtnCapture.Enabled = false;
            }

            // PatGen
            string[] myPort;
            myPort = System.IO.Ports.SerialPort.GetPortNames();
            this.cb_PatGen_ComPort.Items.AddRange(myPort);
            this.PatGen = new SerialPort();

            if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
            {
                cbxXyzFilterPos.Items.Clear();
                cbxXyzFilterPos.Items.Add("Y");
                cbxXyzFilterPos.Items.Add("W");
            }

            // Initial XYZ \ ND
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        if (GlobalVar.Device.XyzFilter != null)
                            this.cbxXyzFilterPos.Text = this.GetXYZInfo(GlobalVar.Device.XyzFilter.Position);

                        if (GlobalVar.Device.NdFilter != null)
                            this.cbxNdFilterPos.Text = this.GetNDInfo(GlobalVar.Device.NdFilter.Position);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                case EnumMotoControlPackage.CircleTac:
                    {
                        if (this.Motor.IsWork())
                        {
                            this.cbxXyzFilterPos.Text = this.GetXYZInfo(this.Motor.NowPos_FW1());
                            this.cbxNdFilterPos.Text = this.GetNDInfo(this.Motor.NowPos_FW2());
                        }
                    }
                    break;

                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    {
                        cbxXyzFilterPos.Items.Clear();
                        cbxXyzFilterPos.Items.Add("-");
                        cbxXyzFilterPos.Items.Add("X");
                        cbxXyzFilterPos.Items.Add("-");
                        cbxXyzFilterPos.Items.Add("Y");
                        cbxXyzFilterPos.Items.Add("-");
                        cbxXyzFilterPos.Items.Add("Z");

                        cbxNdFilterPos.Items.Clear();
                        cbxNdFilterPos.Items.Add("-");
                        cbxNdFilterPos.Items.Add("12.5%");
                        cbxNdFilterPos.Items.Add("-");
                        cbxNdFilterPos.Items.Add("1.56%");
                        cbxNdFilterPos.Items.Add("-");
                        cbxNdFilterPos.Items.Add("0.25%");

                    }
                    break;

            }

            //Mouse Move Event

        }

        private void _29M_Test_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.Motor != null)
            {
                this.Motor.UpdateStatus -= MotorControl_UpdateStatus;
            }

            this.liveViewFlag = false;

            if (this.liveViewThread != null)
            {
                this.liveViewThread.Abort();
            }

            MilNetHelper.MilBufferFree(ref tifImg);

            // PatGen
            if (this.PatGen != null)
            {
                if (this.PatGen.IsOpen)
                    this.PatGen.Close();

                this.PatGen = null;
            }

            //UrRobot
            if (this.Robot != null)
            {
                if (this.Robot.IsConnected())
                    this.Robot.Disconnect();

                this.Robot = null;
            }

            //taskAutoPlaneAlign
            this.timer_AutoPlaneAlign.Enabled = false;

            this.Dispose();
        }

        private MIL_INT DisplayMouseMoveEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
        {
            return 0;
        }

        private void ManualForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                BtnDelRoi_Click(null, null);
            }
        }

        private delegate void LoadImageUICB(string ImgPath);

        #region --- Delegate ---

        #region --- LoadImage ---

        private void LoadImage(string ImgPath)
        {
            if (PnlGrabViewer.InvokeRequired)
            {
                var d = new LoadImageUICB(this.LoadImage);
                PnlGrabViewer.Invoke(d, new object[] { ImgPath });
            }
            else
            {
                AllocBuf(ref tifImg, ImgPath, 16);

                MDisplay.SetWindow(ref tifImg, ref PnlGrabViewer);

            }
        }

        #endregion --- LoadImage ---

        #region --- GetcbxXyzFilterPos ---

        private string GetcbxXyzFilterPos()
        {
            if (cbxXyzFilterPos.InvokeRequired)
            {
                string pos = "";

                // 如果當前執行緒不是UI執行緒，使用委派進行跨執行緒操作
                GetXyzFilterPosCallback updateTextBoxDelegate = new GetXyzFilterPosCallback(GetcbxXyzFilterPos);
                pos = (string)Invoke(updateTextBoxDelegate, new object[] { });

                return pos;
            }
            else
            {
                // 如果是UI執行緒，直接設置TextBox的文字
                return cbxXyzFilterPos.Text;
            }
        }

        #endregion --- GetcbxXyzFilterPos ---

        #region --- GetcbxNdFilterIndex ---

        private string GetcbxNdFilterIndex()
        {
            if (cbxNdFilterPos.InvokeRequired)
            {
                string pos = "";

                // 如果當前執行緒不是UI執行緒，使用委派進行跨執行緒操作
                GetNdFilterPosCallback updateTextBoxDelegate = new GetNdFilterPosCallback(GetcbxNdFilterIndex);
                pos = (string)Invoke(updateTextBoxDelegate, new object[] { });

                return pos;
            }
            else
            {
                // 如果是UI執行緒，直接設置TextBox的文字
                return cbxNdFilterPos.SelectedIndex.ToString();
            }
        }

        #endregion --- GetcbxNdFilterPos ---

        #endregion --- Delegate ---

        #region --- 方法函式 ---

        #region --- UpdateUI ---

        private void UpdateUI()
        {
            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {

                switch (this.MotorType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                        {
                            bool CheckXYZ = (GlobalVar.Device.XyzFilter != null && GlobalVar.Device.XyzFilter.is_FW_Work);
                            this.cbxXyzFilterPos.Visible = CheckXYZ;
                            this.lbl_FiterWheel_XYZFilterPosition.Visible = CheckXYZ;
                            this.mbtnXyzFilterMove.Visible = CheckXYZ;


                            bool CheckND = (GlobalVar.Device.NdFilter != null && GlobalVar.Device.NdFilter.is_FW_Work);
                            this.cbxNdFilterPos.Visible = CheckND;
                            this.lbl_FiterWheel_NdFilterPosition.Visible = CheckND;
                            this.mbtnNdFilterMove.Visible = CheckND;



                        }
                        break;

                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            bool CheckMotor = (this.Motor != null && this.Motor.IsWork());

                            this.mbtnAllHome.Visible = CheckMotor;

                            this.cbxXyzFilterPos.Visible = CheckMotor;
                            this.lbl_FiterWheel_XYZFilterPosition.Visible = CheckMotor;
                            this.mbtnXyzFilterMove.Visible = CheckMotor;

                            this.cbxNdFilterPos.Visible = CheckMotor;
                            this.lbl_FiterWheel_NdFilterPosition.Visible = CheckMotor;
                            this.mbtnNdFilterMove.Visible = CheckMotor;

                            if (!CheckMotor) tP_Device.Parent = null;

                        }
                        break;
                }
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                switch (this.MotorType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                        {
                            // XYZ Filter
                            if (GlobalVar.Device.XyzFilter != null)
                            {
                                if (!GlobalVar.Device.XyzFilter.is_FW_Work)
                                {
                                    this.cbxXyzFilterPos.Visible = false;
                                    this.lbl_FiterWheel_XYZFilterPosition.Visible = false;
                                }
                            }

                            // ND Filter
                            if (GlobalVar.Device.NdFilter != null)
                            {
                                if (!GlobalVar.Device.NdFilter.is_FW_Work)
                                {
                                    this.cbxNdFilterPos.Visible = false;
                                    this.lbl_FiterWheel_NdFilterPosition.Visible = false;
                                }
                            }
                            break;
                        }

                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            if (!this.Motor.IsWork())
                            {
                                GlobalVar.Device.XyzPosition = this.Motor.NowPos_FW1();
                                GlobalVar.Device.NdPosition = this.Motor.NowPos_FW2();

                                this.cbxXyzFilterPos.SelectedIndex = GlobalVar.Device.XyzPosition;
                                this.cbxNdFilterPos.SelectedIndex = GlobalVar.Device.NdPosition;

                                // XYZ Filter
                                this.cbxXyzFilterPos.Enabled = false;
                                this.mbtnXyzFilterSettingExecute.Enabled = false;
                                this.mbtnXyzFilterMove.Enabled = false;
                                // ND Filter
                                this.cbxNdFilterPos.Enabled = false;
                                this.mbtnNdFilterSettingExecute.Enabled = false;
                                this.mbtnNdFilterMove.Enabled = false;
                            }
                        }
                        break;
                }
            }
        }

        #endregion --- UpdateUI ---

        #region --- HideUI ---

        private void HideUI()
        {
            if (GlobalVar.IsSecretHidden)
            {
                #region Task Page

                tP_Device.Parent = null;
                tP_V_Curve.Parent = null;
                tP_AutoPattern.Parent = null;
                tP_MonoChromatorMeasure.Parent = null;
                tP_PatGen.Parent = null;
                tP_FFC.Parent = null;
                tP_LightSourceA.Parent = null;
                tp_NoiseDetection.Parent = null;
                tp_TiltCorrection.Parent = null;
                tP_CameraAnalyze.Parent = null;
                tP_AutoFocus.Parent = null;
                tP_V_Curve.Parent = null;
                tP_AutoPlaneAkign.Parent = null;
                tP_HotPixel.Parent = null;
                tP_Calibration.Parent = null;
                tP_LaserCalibration.Parent = null;
                tP_LightMeasure.Parent = null;
                tP_AutoTatget.Parent = null;
                tP_ColorMura.Parent = null;
                tP_PixelAlign.Parent = null;
                tP_CombineImage.Parent = null;
                tP_Warp.Parent = null;
                tP_HeatMap.Parent = null;
                tP_FormShow.Parent = null;

                #endregion

                #region Tool Button

                //Btn_MotorTool.Parent = null;
                Btn_SR3_UnitForm.Parent = null;
                Btn_CornerstoneB_UnitForm.Parent = null;
                Btn_A_Light_UnitForm.Parent = null;
                Btn_D65_UnitForm.Parent = null;
                Btn_Robot_UnitForm.Parent = null;
                Btn_XPLC_UnitForm.Parent = null;

                #endregion

                #region Camera Page

                PageROI.Parent = null;

                #endregion
            }
        }

        #endregion --- HideUI ---


        #region --- LiveViewProc ---

        private void LiveViewProc()
        {
            while (this.liveViewFlag)
            {
                this.Grabber.Grab();
            }
        }

        #endregion --- LiveViewProc ---

        #region --- GetTextBox ---

        private void GetTextBox(Control fatherControl, string textBoxName)
        {
            if (corrTextBox != null) return;

            Control.ControlCollection sonControls = fatherControl.Controls;
            foreach (Control ctrl in sonControls)
            {
                if (ctrl.Controls.Count != 0)
                {
                    GetTextBox(ctrl, textBoxName);
                }
                else if (ctrl.Name == textBoxName)
                {
                    corrTextBox = (MaterialTextBox)ctrl;
                    break;
                }
            }
        }

        #endregion --- GetTextBox ---

        #region --- GetNDInfo ---

        private string GetNDInfo(int ndPostion)
        {
            switch (ndPostion)
            {
                case 0:
                    return "12.5%";

                case 1:
                    return "1.56%";

                case 2:
                    return "0.25%";

                case 3:
                    return "100%";
            }

            return "Unknow ND";
        }

        #endregion --- GetNDInfo ---

        #region --- GetXYZInfo ---

        private string GetXYZInfo(int xyzPostion)
        {
            if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIC_4_Positions || GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_4_Positions)
            {
                switch (xyzPostion)
                {
                    case 0:
                        return "X";

                    case 1:
                        return "Y";

                    case 2:
                        return "Z";
                }
            }
            else if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
            {
                if (xyzPostion == 0)
                    return "Y";

            }

            return "Unknow XYZ";
        }

        #endregion --- GetXYZInfo ---







        #region --- UrRobot_Jog ---
        private void UrRobot_Jog(Enum_RobotDoF urRobot_DoF, Enum_Jog jog)
        {
            int moveStepIndex = this.cB_UrRobot_JogStep.SelectedIndex;  // 移動步伐大小
            this.timer_AutoPlaneAlign.Enabled = true;

            if (this.AutoPlane != null && this.Robot.IsConnected())
            {
                this.AutoPlane.Set_RobotPosInfo(urRobot_DoF, jog, moveStepIndex);
                this.AutoPlane.MoveToPos();

                //set display
                //this.Update_AutoPlaneAlign_RobotPosInfo();
            }
        }
        #endregion --- UrRobot_Jog ---

        #endregion --- 方法函式 ---

        #region --- Color Mura ---


        #region --- btnColorMuraExecute_Click ---

        private void btnColorMuraExecute_Click(object sender, EventArgs e)
        {
            ColorMura_DetectParam param = new ColorMura_DetectParam();
            param.Numerator_Channel = cbbColorMuraNumerator.SelectedIndex;
            param.Denominator_Channel = cbbColorMuraDenominator.SelectedIndex;
            param.Open = Convert.ToInt32(tbxColorMuraAreaOpen.Text);
            param.Dilate = Convert.ToInt32(tbxColorMuraAreaDilate.Text);
            param.MinArea = Convert.ToInt32(tbxColorMuraMinArea.Text);
            param.ThresHigh = Convert.ToInt32(tbxColorMuraHighThreshold.Text);
            param.ThresLow = Convert.ToInt32(tbxColorMuraLowThreshold.Text);

            param.ImgX_Path = tbxColorMuraColorSourceX.Text;
            param.ImgY_Path = tbxColorMuraColorSourceY.Text;
            param.ImgZ_Path = tbxColorMuraColorSourceZ.Text;

            string output = tbxColorMuraOutput.Text;
            string filePath = $@"{output}\ColorMuraResult.tif";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            ColorMura(param, output);

            if (File.Exists(filePath))
            {
                MIL_ID temp = MIL.M_NULL;
                MIL.MbufRestore(filePath, MyMil.MilSystem, ref temp);
                MilNetHelper.MilBufferFree(ref temp);
            }
        }

        #endregion --- btnColorMuraExecute_Click ---

        #endregion --- Color Mura ---

        #region --- Combine Image ---

        private void Btn_CI_Start_Click(object sender, EventArgs e)
        {
            XyzFilePath CombineFile = new XyzFilePath();
            CombineFile.X = Tbx_CI_SourceImg_X.Text;
            CombineFile.Y = Tbx_CI_SourceImg_Y.Text;
            CombineFile.Z = Tbx_CI_SourceImg_Z.Text;

            string OutputPath = Tbx_CI_OutputPath.Text;

            CombineImage(CombineFile, OutputPath, "CIImg.tif");
        }

        #endregion --- Combine Image ---

        #region --- Form Show ---

        private void btnOneColorCailbrationShow_Click(object sender, EventArgs e)
        {
            frmOneColorCorrection frmOneColorCorrection = new frmOneColorCorrection(GlobalVar.OmsParam.AutoExposureRoiGrayBypassPercent);
            frmOneColorCorrection.ShowDialog();
        }

        #endregion --- Form Show ---

        #region --- UI ---



        #region --- Button Event ---

        #region --- Camera ---

        #region --- mbtnLoadImage_Click ---

        private void mbtnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "請選擇圖片";
            dialog.Filter = "Image File| *.tif; *.tiff; *.bmp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.LoadImage(dialog.FileName);
            }
        }

        #endregion --- mbtnLoadImage_Click ---

        #region --- mbtnLive_Click ---

        private void mbtnLive_Click(object sender, EventArgs e)
        {
            GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";

            MDisplay.SetWindow(ref this.Grabber.grabImage, ref PnlGrabViewer);

            if (!this.liveViewFlag)
            {
                string XyzFilterPos = "Y";
                string NdFilterPos = "0";

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    if (GlobalVar.Device.XyzFilter != null)
                    {
                        if (GlobalVar.Device.XyzFilter.is_FW_Work)
                        {
                            XyzFilterPos = this.GetcbxXyzFilterPos();
                            if (XyzFilterPos == "")
                            {
                                MessageBox.Show("Select xyz filter first !");
                                this.cbxXyzFilterPos.Focus();
                                return;
                            }
                        }
                    }

                    if (GlobalVar.Device.NdFilter != null)
                    {
                        if (GlobalVar.Device.NdFilter.is_FW_Work)
                        {
                            NdFilterPos = this.GetcbxNdFilterIndex();
                            if (NdFilterPos == "")
                            {
                                MessageBox.Show("Select nd filter first !");
                                this.cbxNdFilterPos.Focus();
                                return;
                            }
                        }
                    }

                }


                GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";
                //string DarkPath = $@"{GlobalVar.Config.RecipePath}\FFC\ND{GlobalVar.Device.NdPosition}_Dark\{GlobalVar.ProcessInfo.XyzStr[GlobalVar.Device.XyzPosition]}.tif";

                //set display

                this.liveViewFlag = true;
                this.mbtnLive.UseAccentColor = true;
                if (liveViewThread != null) { liveViewThread = null; }
                liveViewThread = new Thread(LiveViewProc);
                liveViewThread.Start();
            }
            else
            {
                this.liveViewFlag = false;
                this.mbtnLive.UseAccentColor = false;

                if (this.liveViewThread != null)
                    this.liveViewThread = null;
            }
        }

        #endregion --- mbtnLive_Click ---

        #region --- mbtnCapture_Click ---

        private void mbtnCapture_Click(object sender, EventArgs e)
        {
            MDisplay.SetWindow(ref this.Grabber.grabImage, ref PnlGrabViewer);

            this.liveViewFlag = false;
            this.mbtnLive.UseAccentColor = false;

            if (this.liveViewThread != null)
                this.liveViewThread = null;

            // 停止實時取像
            this.liveViewFlag = false;


            try
            {
                //GlobalVar.Device.XyzPosition = 1;
                //GlobalVar.Device.NdPosition = 0;
                //GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";
                //string DarkPath = $@"{GlobalVar.Config.RecipePath}\FFC\ND{GlobalVar.Device.NdPosition}_Dark\{GlobalVar.ProcessInfo.XyzStr[GlobalVar.Device.XyzPosition]}.tif";


                string XyzFilterPos = "Y";
                string NdFilterPos = "0";

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    if (GlobalVar.Device.XyzFilter != null)
                    {
                        if (GlobalVar.Device.XyzFilter.is_FW_Work)
                        {
                            XyzFilterPos = this.GetcbxXyzFilterPos();
                            if (XyzFilterPos == "")
                            {
                                MessageBox.Show("Select xyz filter first !");
                                this.cbxXyzFilterPos.Focus();
                                return;
                            }
                        }
                    }

                    if (GlobalVar.Device.NdFilter != null)
                    {
                        if (GlobalVar.Device.NdFilter.is_FW_Work)
                        {
                            NdFilterPos = this.GetcbxNdFilterIndex();
                            if (NdFilterPos == "")
                            {
                                MessageBox.Show("Select nd filter first !");
                                this.cbxNdFilterPos.Focus();
                                return;
                            }
                        }
                    }
                }

                GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";

                this.Grabber.Grab();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Capture failed. : {ex.Message}");
            }
        }

        #endregion --- mbtnCapture_Click ---

        #region --- mbtnSetExposureTime_Click ---

        private void mbtnSetExposureTime_Click(object sender, EventArgs e)
        {
            try
            {
                double ExposureTime = Convert.ToDouble(mtbxExposureTime.Text);

                if (ExposureTime < 32)
                {
                    ExposureTime = 32;
                    MessageBox.Show("曝光時間，不能低於 32 us !");
                    mtbxExposureTime.Text = ExposureTime.ToString();
                    return;
                }

                this.Grabber.SetExposureTime(ExposureTime);

                GlobalConfig.UserKeyinExpTime = ExposureTime;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion --- mbtnSetExposureTime_Click ---

        #region --- mbtnSetGain_Click ---

        private void mbtnSetGain_Click(object sender, EventArgs e)
        {
            try
            {
                int gain = Convert.ToInt32(mtbxGain.Text);

                if (gain <= 1)
                {
                    gain = 1;
                    mtbxGain.Text = "1";
                }

                if (gain > 32)
                {
                    gain = 32;
                    mtbxGain.Text = "32";
                }
                this.Grabber.SetCameraGain(gain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion --- mbtnSetGain_Click ---

        #region --- mbtnGetTemp_Click ---

        private void MbtnGetTemp_Click(object sender, EventArgs e)
        {
            double Temp = this.Grabber.GetTemperature();
            MessageBox.Show($"Temperature = {Temp}");
        }

        #endregion --- mbtnGetTemp_Click ---

        #region --- btnSaveImage_Click ---

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            MIL_ID currentImage = MIL.M_NULL;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Wordfile (*.tif;*.bmp;)|*.tif;*.bmp)";
            SFD.DefaultExt = ".tif";

            if (SFD.ShowDialog() == DialogResult.OK)
            {
                string fileName = SFD.FileName;
                //if (ImgType == 0)
                //{
                //    MIL.MbufExport(fileName, MIL.M_TIFF, GrabImg);

                //}
                //else
                //{
                //    MIL.MbufSave(fileName, TifImg);

                //}
                MIL.MbufSave(fileName, MDisplay.MilImage);
            }
        }

        #endregion --- btnSaveImage_Click ---

        #endregion --- Camera ---

        #region --- ROI ---

        private void BtnAddRoi_Click(object sender, EventArgs e)
        {
            cbShowGrid.Checked = false;

            bool Rtn = MDisplay.AddROI();

            if (Rtn)
            {
                ROI_UpdateUI();
            }
        }

        private void BtnDelRoi_Click(object sender, EventArgs e)
        {
            bool Rtn = MDisplay.DeleteROI();
            if (Rtn)
            {
                ROI_UpdateUI();
            }
        }


        private void ROI_UpdateUI()
        {
            if (GlobalVar.IsSecretHidden) return;

            int RoiCnt = -1;
            MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT,
                MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            List<Rectangle> RoiList = new List<Rectangle>();


            for (MIL_INT i = 0; i < RoiCnt; i++)
            {
                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                // Get Selected

                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                RoiList.Add(new Rectangle(X, Y, W, H));
            }

            DGV_ROI.Invoke(new Action(() => {
                DGV_ROI.RowCount = RoiCnt;

                for (int i = 0; i < RoiList.Count; i++)
                {
                    DGV_ROI.Rows[i].Cells[0].Value = i.ToString();
                    DGV_ROI.Rows[i].Cells[1].Value = RoiList[i].X.ToString();
                    DGV_ROI.Rows[i].Cells[2].Value = RoiList[i].Y.ToString();
                    DGV_ROI.Rows[i].Cells[3].Value = RoiList[i].Width.ToString();
                    DGV_ROI.Rows[i].Cells[4].Value = RoiList[i].Height.ToString();
                }
            }
            ));

        }

        #endregion

        #region --- Auto Target ---

        #region --- mbtnAutoTarget1Execute_Click ---

        private void mbtnAutoTarget1Execute_Click(object sender, EventArgs e)
        {
            MIL_ID image = MIL.M_NULL;

            try
            {
                int resultCode = 0;
                int[] roi = new int[4] { -1, -1, -1, -1 };
                int roiWidth = Convert.ToInt32(tbxAutoTargetRoiWidth.Text);
                int roiHeight = Convert.ToInt32(tbxAutoTargetRoiHeight.Text);
                double minArea = Convert.ToDouble(tbxAutoTargetMinArea.Text);
                double minSubArea = Convert.ToDouble(tbxAutoTargetMinSubArea.Text);
                string imagePath = tbxAutoTargetSourceImage1.Text;

                this.LoadImage(imagePath);
                MIL.MbufRestore(imagePath, MyMil.MilSystem, ref image);
                resultCode = AutoRoiLargeArea(image, roiWidth, roiHeight, minArea, minSubArea, ref roi);
                MDisplay.DrawROI(roi);
            }
            catch
            {
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);
            }
        }

        #endregion --- mbtnAutoTarget1Execute_Click ---

        #region --- mbtnAutoTarget1Execute_Click ---

        private void mbtnAutoTarget2Execute_Click(object sender, EventArgs e)
        {
            MIL_ID image = MIL.M_NULL;

            try
            {
                PixelInfo maxPixel = new PixelInfo();
                string imagePath = tbxAutoTargetSourceImage2.Text;

                this.LoadImage(imagePath);
                MIL.MbufRestore(imagePath, MyMil.MilSystem, ref image);
                maxPixel = AutoRoiBrightestPoint(image);
                int[] newroi = new int[4] { maxPixel.X, maxPixel.X + 1, maxPixel.Y, maxPixel.Y + 1 };

                MDisplay.DrawROI(newroi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);
            }
        }

        #endregion --- mbtnAutoTarget1Execute_Click ---

        #endregion --- Auto Target ---










        #endregion --- Button Event ---


        #region --- cbxGridSizeSelect_SelectedIndexChanged ---

        private void cbxGridSizeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            double GridSize = cbxGridSizeSelect.SelectedIndex / 10.0;
            if (cbShowGrid.Checked)
            {
                MDisplay.SetGrid(GridSize);
            }
        }

        #endregion --- cbxGridSizeSelect_SelectedIndexChanged ---




        #region --- CheckBox Event ---

        #region --- cbShowGrid_CheckedChanged ---

        private void cbShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            bool ShowGrid = cbShowGrid.Checked;
            double GridSize = cbxGridSizeSelect.SelectedIndex / 10.0;

            if (ShowGrid)
            {
                MDisplay.SetGrid(GridSize);
            }
            else
            {
                MDisplay.ClearGrid();
            }
        }

        #endregion --- cbShowGrid_CheckedChanged ---

        #endregion --- CheckBox Event ---

        #region --- Panel Event ---

        #region --- PnlGrabViewer_MouseMove ---

        private void PnlGrabViewer_MouseMove(object sender, MouseEventArgs e)
        {
            ROI_UpdateUI();
        }

        #endregion --- PnlGrabViewer_MouseMove ---

        #endregion --- Panel Event ---

        #endregion --- UI ---


        //=============================================

        #region Remove Hot Pixel & Dilate & Smooth & CalAF

        private void btnRemoveHotPixelExecute_Click(object sender, EventArgs e)
        {
            int pitch = Convert.ToInt16(tbxHotPixelPitch.Text);
            double darkThreshold = Convert.ToDouble(tbxHotPixelDarkThreshold.Text);
            double brightThreshold = Convert.ToDouble(tbxHotPixelBrightThreshold.Text);

            try
            {
                HotPixel(ref MDisplay.MilImage, pitch, brightThreshold, darkThreshold);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDilateExecute_Click(object sender, EventArgs e)
        {
            Dilate(MDisplay.MilImage);
        }

        private void btnAutoFocusSmooth_Click(object sender, EventArgs e)
        {
            try
            {
                int iteration = Convert.ToInt32(tbxAutoFocusSmoothIteration.Text);
                Smooth(MDisplay.MilImage, iteration);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAutoFocusCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                int score = EvaluateFocusScore(MDisplay.MilImage);
                tbxAutoFocusScore.Text = score.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Remove Hot Pixel & Dilate & Smooth & CalAF

        #region Calibration 

        private void btnLoadCalibrationFile_Click(object sender, EventArgs e)
        {
            // TODO: Filter
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                tbxCalibrationFile.Text = fileDialog.FileName;
            }
        }

        private void btnLoadCalibrationExecute_Click(object sender, EventArgs e)
        {
            string correctionFile = tbxCalibrationFile.Text;

            Calibration(MDisplay.MilImage, correctionFile, ref MDisplay.MilImage);
        }

        #endregion Calibration 

        #region Filter

        private void btnXyzFilterSettingExecute_Click(object sender, EventArgs e)
        {
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.xyzFilter.SelectDevice();
                    }
                    break;
            }
        }

        private void btnNdFilterSettingExecute_Click(object sender, EventArgs e)
        {
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.ndFilter.SelectDevice();
                    }
                    break;
            }
        }

        private void mbtnXyzFilterMove_Click(object sender, EventArgs e)
        {
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.xyzFilter.ChangeWheel(this.cbxXyzFilterPos.SelectedIndex);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                case EnumMotoControlPackage.CircleTac:
                    {
                        if (this.Motor.IsWork())
                        {
                            this.Motor.Move(-1, -1, this.cbxXyzFilterPos.SelectedIndex, -1);
                        }
                    }
                    break;
            }
        }

        private void mbtnNdFilterMove_Click(object sender, EventArgs e)
        {
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        this.ndFilter.ChangeWheel(this.cbxNdFilterPos.SelectedIndex);
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                case EnumMotoControlPackage.CircleTac:
                    {
                        if (this.Motor.IsWork())
                            this.Motor.Move(-1, -1, -1, this.cbxNdFilterPos.SelectedIndex);
                    }
                    break;
            }
        }

        private void mbtnAllHome_Click(object sender, EventArgs e)
        {
            switch (this.MotorType)
            {
                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    {
                        if (this.Motor.IsWork())
                            this.Motor.Home(MotorMember.All);
                    }
                    break;
            }
        }

        private void cbxXyzFilterPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalVar.Device.XyzPosition = this.cbxXyzFilterPos.SelectedIndex;
        }

        private void cbxNdFilterPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbxNdFilterPos.SelectedIndex)
            {
                // 100%
                case 0: GlobalVar.Device.NdPosition = 3; break;
                // 10%
                case 1: GlobalVar.Device.NdPosition = 0; break;
                // 3%
                case 2: GlobalVar.Device.NdPosition = 1; break;
                // 1%
                case 3: GlobalVar.Device.NdPosition = 2; break;
                // 100%
                default: GlobalVar.Device.NdPosition = 3; break;
            }
        }

        #endregion Filter

        #region Warp 

        private void mbtnWarpExecute_Click(object sender, EventArgs e)
        {
            //  (CP1)
            //    P1----------P2
            //    |           |
            //    |           |
            //    P4----------P3
            //               (CP2)

            Point Point1 = new Point(Convert.ToInt32(Tbx_Warp_P1X.Text), Convert.ToInt32(Tbx_Warp_P1Y.Text));
            Point Point2 = new Point(Convert.ToInt32(Tbx_Warp_P2X.Text), Convert.ToInt32(Tbx_Warp_P2Y.Text));
            Point Point3 = new Point(Convert.ToInt32(Tbx_Warp_P3X.Text), Convert.ToInt32(Tbx_Warp_P3Y.Text));
            Point Point4 = new Point(Convert.ToInt32(Tbx_Warp_P4X.Text), Convert.ToInt32(Tbx_Warp_P4Y.Text));
            Point CoeffPoint1 = new Point(Convert.ToInt32(Tbx_Warp_CP1X.Text), Convert.ToInt32(Tbx_Warp_CP1Y.Text));
            Point CoeffPoint2 = new Point(Convert.ToInt32(Tbx_Warp_CP2X.Text), Convert.ToInt32(Tbx_Warp_CP2Y.Text));

            Warp(MDisplay.MilImage, Point1, Point2, Point3, Point4, CoeffPoint1, CoeffPoint2, MDisplay.MilImage);
        }

        #endregion Warp

        #region Pixel Align 

        private void mbtnPixelAlign_Click(object sender, EventArgs e)
        {
            PixelAlignParam param = new PixelAlignParam();

            param.Image_Path = Tbx_PA_ImagePath.Text;
            param.setting.outPath = Tbx_PA_OutputPath.Text;

            param.panelInfo.ResolutionX = int.Parse(Tbx_PA_ResolutionX.Text);
            param.panelInfo.ResolutionY = int.Parse(Tbx_PA_ResolutionY.Text);

            param.setting.CoarseAlignSetting.Threshold = int.Parse(Tbx_PA_Coarse_Threshold.Text);
            param.setting.CoarseAlignSetting.AreaMin = int.Parse(Tbx_PA_Coarse_MinArea.Text);
            param.setting.CoarseAlignSetting.CloseNums = int.Parse(Tbx_PA_Coarse_CloseNum.Text);
            param.setting.CoarseAlignSetting.DebugImage = Cbx_PA_Coarse_Debug.Checked;

            param.setting.FindPanelPixelSetting.AreaMin = int.Parse(Tbx_PA_FindPixel_MinArea.Text);
            param.setting.FindPanelPixelSetting.FirstPixelX = double.Parse(Tbx_PA_FindPixel_FirstX.Text);
            param.setting.FindPanelPixelSetting.FirstPixelY = double.Parse(Tbx_PA_FindPixel_FirstY.Text);
            param.setting.FindPanelPixelSetting.RowFindPitchX = double.Parse(Tbx_PA_FindPixel_RowX.Text);
            param.setting.FindPanelPixelSetting.RowFindPitchY = double.Parse(Tbx_PA_FindPixel_RowY.Text);
            param.setting.FindPanelPixelSetting.ColFindPitchX = double.Parse(Tbx_PA_FindPixel_ColX.Text);
            param.setting.FindPanelPixelSetting.ColFindPitchY = double.Parse(Tbx_PA_FindPixel_ColY.Text);
            //param.setting.FindPanelPixelSetting.DeltaPitch = double.Parse(Tbx_PA_FindPixel_DeltaNum.Text);
            //param.setting.FindPanelPixelSetting.SearchRegion = int.Parse(Tbx_PA_FindPixel_SearchRegion.Text);
            param.setting.FindPanelPixelSetting.DebugImage = Cbx_PA_FindPixel_Debug.Checked;

            PixelAlign(param);
        }

        #endregion Pixel Align 

        #region Light Measure 

        private void btnLightMeasureExecute_Click(object sender, EventArgs e)
        {
            bool[] xyzEnable = GlobalVar.OmsParam.OneColorCalibration_XYZ_Enable;

            XyzFilePath predictFile = new XyzFilePath();
            predictFile.X = tbxLightMeasureSourceX.Text;
            predictFile.Y = tbxLightMeasureSourceY.Text;
            predictFile.Z = tbxLightMeasureSourceZ.Text;

            XyzFilePath coerrctionFile = new XyzFilePath();
            coerrctionFile.X = tbxLightMeasureCoeffX.Text;
            coerrctionFile.Y = tbxLightMeasureCoeffY.Text;
            coerrctionFile.Z = tbxLightMeasureCoeffZ.Text;

            MeasureData predictData = new MeasureData();
            predictData.ExposureTime.X = Convert.ToInt32(tbxLightMeasureExposureTimeX.Text);
            predictData.ExposureTime.Y = Convert.ToInt32(tbxLightMeasureExposureTimeY.Text);
            predictData.ExposureTime.Z = Convert.ToInt32(tbxLightMeasureExposureTimeZ.Text);

            string outputPath = tbxLightMeasureOutputPath.Text;
            MeasureLightPredict(predictFile, coerrctionFile, predictData, xyzEnable, outputPath);
        }

        #endregion Light Measure 

        #region Laser Calibration

        private void btnLaserCalibrationExecute_Click(object sender, EventArgs e)
        {
            LaserCalibrationParam cal = new LaserCalibrationParam();
            cal.IsDebug = cbxLaserCalibrationDebug.Checked;
            cal.Template_Path = tbxLaserCalibrationTemplate.Text;
            cal.Target_Path = tbxLaserCalibrationTarget.Text;
            cal.ROI.X = int.Parse(tbxLaserCalibrationStartX.Text);
            cal.ROI.Y = int.Parse(tbxLaserCalibrationStartY.Text);
            cal.ROI.Width = int.Parse(tbxLaserCalibrationWidth.Text);
            cal.ROI.Height = int.Parse(tbxLaserCalibrationHeight.Text);
            cal.Output_Path = tbxLaserCalibrationOutput.Text;

            MIL.MbufRestore(tbxLaserCalibrationCorrection.Text, MyMil.MilSystem, ref cal.WaitCorrectId);
            //cal.WaitCorrectId = _WaitCorrect;

            Calibration_SecondCorrect(cal);

            // Free
            MilNetHelper.MilBufferFree(ref cal.WaitCorrectId);
        }

        #endregion Laser Calibration

        #region Auto Focus 

        private void btnColorMuraColorExecute_Click(object sender, EventArgs e)
        {
            ColorMura_DetectParam param = new ColorMura_DetectParam();
            param.ImgColor_Path = tbxColorMuraColorSourceImage.Text;

            if (File.Exists(param.ImgColor_Path))
            {
                param.Numerator_Channel = cbbColorMuraNumerator.SelectedIndex;
                param.Denominator_Channel = cbbColorMuraDenominator.SelectedIndex;
                param.Open = Convert.ToInt32(tbxColorMuraAreaOpen.Text);
                param.Dilate = Convert.ToInt32(tbxColorMuraAreaDilate.Text);
                param.MinArea = Convert.ToInt32(tbxColorMuraMinArea.Text);
                param.ThresHigh = Convert.ToInt32(tbxColorMuraHighThreshold.Text);
                param.ThresLow = Convert.ToInt32(tbxColorMuraLowThreshold.Text);

                string Output = tbxColorMuraOutput.Text;
                ColorMura_ColorImg(param, Output);
            }
            else
            {
                MessageBox.Show("Path of Color Image DOES NOT Exist!", "Error");
            }
        }

        private void btn_Focuser_Move_Click(object sender, EventArgs e)
        {
            int MoveTarget = Convert.ToInt32(this.nUD_Focuser_Move.Value);

            if (this.Motor.IsWork())
            {
                this.lbl_FocuserPosition.Text = this.Motor.NowPos_Focuser().ToString();

                switch (this.MotorType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    case EnumMotoControlPackage.MS5515M:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            this.Motor.Move(MoveTarget, -1, -1, -1);
                        }
                        break;
                }
            }
        }

        private void btn_Focuser_AutoFocus_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            if (!this.Motor.IsWork())
            {
                MessageBox.Show("Motor is not work !");
                return;
            }

            AutoFocus_FlowPara param = new AutoFocus_FlowPara();

            // Aperture
            this.lbl_AperturePosition.Text = this.Motor.NowPos_Aperture().ToString();

            // Focuser
            this.lbl_FocuserPosition.Text = "0";
            this.lbl_FocuserValue.Text = "0";
            this.lbl_Focuser_TactTime.Text = "0";

            this.lbl_Focuser_AutoFocus_Info.Text = "Moving";
            this.lbl_Focuser_AutoFocus_Info.BackColor = Color.Red;

            if (this.nUD_Focuser_UpperLimit.Value != -1 && this.nUD_Focuser_LowerLimit.Value != -1)
            {
                param.Upper_Limit = (int)this.nUD_Focuser_UpperLimit.Value;
                param.Lower_Limit = (int)this.nUD_Focuser_LowerLimit.Value;
            }
            else if (this.nUD_Focuser_UpperLimit.Value == -1 || this.nUD_Focuser_LowerLimit.Value == -1)
            {
                param.Upper_Limit = -1;
                param.Lower_Limit = -1;
                this.nUD_Focuser_UpperLimit.Value = -1;
                this.nUD_Focuser_LowerLimit.Value = -1;
            }

            param.Min_Step = (int)this.nUD_Focuser_MinStep.Value;


            #region Read ROI

            int RoiCnt = -1;
            MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT,
                MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            if (RoiCnt == 0)
            {
                param.UseROI = false;
            }
            else
            {
                param.UseROI = true;

                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                // Get Selected

                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(0), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(0), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(0), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(0), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                param.ROI = new Rectangle(X, Y, W, H);
            }

            #endregion

            this.lbl_FocuserPosition.Text = this.Motor.NowPos_Focuser().ToString();
            this.lbl_FocuserValue.Text = "0";

            if (Rbtn_AF_Old.Checked)
            {
                this.AutoFocus.Start(param);

                // Start Check Focuser State Thread
                ThreadStart FocuserState = null;
                Thread FocuserStateThread = null;

                FocuserState = new ThreadStart(Check_Focuser_State);
                FocuserStateThread = new Thread(FocuserState);
                FocuserStateThread.Start();
            }

            if (Rbtn_AF_New.Checked)
            {
                AutoFocus_FlowPara_New NewPara = new AutoFocus_FlowPara_New() {
                    Lower_Limit = param.Lower_Limit,
                    Upper_Limit = param.Upper_Limit,
                    Min_Step = param.Min_Step,
                    ROI = param.ROI,
                    UseROI = param.UseROI,
                };

                this.AutoFocus_New.Start(NewPara);

                // Start Check Focuser State Thread
                ThreadStart FocuserState = null;
                Thread FocuserStateThread = null;

                FocuserState = new ThreadStart(Check_Focuser_State_New);
                FocuserStateThread = new Thread(FocuserState);
                FocuserStateThread.Start();
            }

       
        }

        private void Check_Focuser_State()
        {
            TimeSpan timeSpend;
            double ts;
            DateTime startTime = DateTime.Now;

            do
            {

            } while (this.AutoFocus.FlowRun);

            // Tact Time
            timeSpend = DateTime.Now.Subtract(startTime);
            ts = timeSpend.TotalMilliseconds;
            CrossThread.Set_Label_Message(this.lbl_Focuser_TactTime, ts.ToString());
            CrossThread.Set_Label_Message(this.lbl_FocuserPosition, this.Motor.NowPos_Focuser().ToString());

            if (this.AutoFocus.Step == Task_AutoFocus.FlowStep.Error)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Grab Error");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Red);
            }
            else if (this.AutoFocus.Step == Task_AutoFocus.FlowStep.TimeOut)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Recieve TimeOut");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Orange);
            }
            else if (this.AutoFocus.Step == Task_AutoFocus.FlowStep.Finish)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Focused");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.LightGreen);
            }

            System.Threading.Thread.Sleep(100);
            CrossThread.Set_Label_Message(this.lbl_FocuserPosition, this.AutoFocus.AutoFocusPos.ToString());
            CrossThread.Set_Label_Message(this.lbl_FocuserValue, this.AutoFocus.AutoFocusValue.ToString());
        }

        private void Check_Focuser_State_New()
        {
            TimeSpan timeSpend;
            double ts;
            DateTime startTime = DateTime.Now;

            do
            {

            } while (this.AutoFocus_New.FlowRun);

            // Tact Time
            timeSpend = DateTime.Now.Subtract(startTime);
            ts = timeSpend.TotalMilliseconds;
            CrossThread.Set_Label_Message(this.lbl_Focuser_TactTime, ts.ToString());
            CrossThread.Set_Label_Message(this.lbl_FocuserPosition, this.Motor.NowPos_Focuser().ToString());

            if (this.AutoFocus_New.Step == Task_AutoFocus_New.FlowStep.Error)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Grab Error");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Red);
            }
            else if (this.AutoFocus_New.Step == Task_AutoFocus_New.FlowStep.TimeOut)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Recieve TimeOut");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Orange);
            }
            else if (this.AutoFocus_New.Step == Task_AutoFocus_New.FlowStep.Finish)
            {
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Focused");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.LightGreen);
            }

        }

        private void btn_Focuser_Stop_Click(object sender, EventArgs e)
        {
            if (this.AutoFocus != null)
            {
                this.AutoFocus.FlowRun = false;
                this.Motor.Stop(MotorMember.All);
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Stoped");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Red);
            }

            if (this.AutoFocus_New != null)
            {
                this.AutoFocus_New.FlowRun = false;
                this.Motor.Stop(MotorMember.All);
                CrossThread.Set_Label_Message(this.lbl_Focuser_AutoFocus_Info, "Stoped");
                CrossThread.Set_Label_BGColor(this.lbl_Focuser_AutoFocus_Info, Color.Red);
            }
        }

        private void btn_Aperture_Move_Click(object sender, EventArgs e)
        {
            int MoveTarget = Convert.ToInt32(this.nUD_Aperture_Move.Value);

            if (this.Motor.IsWork())
            {
                this.lbl_AperturePosition.Text = this.Motor.NowPos_Aperture().ToString();

                switch (this.MotorType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            this.Motor.Move(-1, MoveTarget, -1, -1);
                        }
                        break;
                }
            }
        }

        #endregion Auto Focus

        #region  V Curve 

        private void btn_VCurve_Home_Click(object sender, EventArgs e)
        {
            if (this.Motor.IsWork())
            {
                this.lbl_VCurve_Home_Info.Text = "Moving";
                this.lbl_VCurve_Home_Info.BackColor = Color.Red;

                this.Motor.Home(MotorMember.Focuser);

                do
                {
                    this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                    System.Threading.Thread.Sleep(30);
                    this.Update();
                } while (this.Motor.NowPos_Focuser() != 0);

                this.lbl_VCurve_Home_Info.Text = "Home";
                this.lbl_VCurve_Home_Info.BackColor = Color.LightGreen;
                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
            }
        }

        private void btn_VCurve_Move_Click(object sender, EventArgs e)
        {
            int MoveTarget = Convert.ToInt32(this.nUD_VCurve_Move.Value);

            if (this.Motor.IsWork())
            {
                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();

                switch (this.MotorType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    case EnumMotoControlPackage.MS5515M:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            this.Motor.Move(MoveTarget, -1, -1, -1);
                        }
                        break;
                }

                do
                {
                    this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                    System.Threading.Thread.Sleep(30);
                    this.Update();
                } while (this.Motor.NowPos_Focuser() != MoveTarget);

                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                this.lbl_VCurveValue.Text = Convert.ToString(this.AutoFocus.GetFocusValue());

                this.lbl_VCurve_AutoFocus_Info.Text = "";
                this.lbl_VCurve_AutoFocus_Info.BackColor = Color.Transparent;

            }
        }

        private void btn_VCurve_Jog_Forward_Click(object sender, EventArgs e)
        {
            MotorMember device = MotorMember.Focuser;
            int Jog_Dist = Convert.ToInt32(this.nUD_VCurve_Jog.Value);
            int nowPos = this.Motor.NowPos_Focuser();

            if (this.Motor.IsWork())
            {
                this.Motor.Jog(device, Jog_Dist);

                do
                {
                    this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                    System.Threading.Thread.Sleep(30);
                    this.Update();
                } while (this.Motor.NowPos_Focuser() != nowPos + Jog_Dist);

                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                this.lbl_VCurveValue.Text = Convert.ToString(this.AutoFocus.GetFocusValue());

                // Update Focus Image
            }
        }

        private void btn_VCurve_Jog_Backward_Click(object sender, EventArgs e)
        {
            MotorMember device = MotorMember.Focuser;
            int Jog_Dist = Convert.ToInt32(this.nUD_VCurve_Jog.Value);
            int nowPos = this.Motor.NowPos_Focuser();

            if (this.Motor.IsWork())
            {
                this.Motor.Jog(device, Jog_Dist * -1);

                do
                {
                    this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                    System.Threading.Thread.Sleep(30);
                    this.Update();
                } while (this.Motor.NowPos_Focuser() != nowPos - Jog_Dist);

                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                this.lbl_VCurveValue.Text = Convert.ToString(this.AutoFocus.GetFocusValue());

            }
        }

        private void btn_VCurve_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            if (this.Motor.IsWork())
            {
                if (this.cbBox_VCurve_ROI.Text == "") this.cbBox_VCurve_ROI.SelectedIndex = 0;

                VCurve_FlowPara Para = new VCurve_FlowPara();
                Para.OptimalPos = (int)this.nUD_VCurve_OptimalPos.Value;
                Para.PosOffset = (int)this.nUD_VCurve_PosOffset.Value;
                Para.Min_Step = (int)this.nUD_VCurve_MinStep.Value;
                Para.ROILocation = (int)this.cbBox_VCurve_ROI.SelectedIndex;
                Para.ROI_SizeX_P = (int)this.nUD_VCurve_ROI_Size_Percent.Value;
                Para.PartCount = (int)this.nUD_VCurve_Steps.Value;
                Para.OutputFolder = GlobalVar.Config.OutputPath.ToString();

                // Aperture
                this.lbl_AperturePosition.Text = this.Motor.NowPos_Aperture().ToString();

                // VCurve
                this.lbl_VCurvePosition.Text = "0";
                this.lbl_VCurveValue.Text = "0";
                this.lbl_VCurvePosition_2.Text = "0";
                this.lbl_VCurveValue_2.Text = "0";
                this.lbl_VCurve_TactTime.Text = "0";
                this.listView_VCurve_Info.Clear();

                this.lbl_VCurve_AutoFocus_Info.Text = "Moving";
                this.lbl_VCurve_AutoFocus_Info.BackColor = Color.Red;

                this.lbl_VCurvePosition.Text = this.Motor.NowPos_Focuser().ToString();
                this.lbl_VCurveValue.Text = "0";

                //startTime = DateTime.Now;
                this.VCurve.Start(Para);

                // Start Check VCurve State Thread
                ThreadStart VCurveState = null;
                Thread VCurveStateThread = null;

                VCurveState = new ThreadStart(Check_VCurve_State);
                VCurveStateThread = new Thread(VCurveState);
                VCurveStateThread.Start();
            }
        }

        private void Check_VCurve_State()
        {
            TimeSpan timeSpend;
            double ts;
            DateTime startTime = DateTime.Now;

            do
            {
                // Update Value
                CrossThread.Set_Label_Message(this.lbl_VCurvePosition, this.VCurve.AutoFocusPos.ToString());
                CrossThread.Set_Label_Message(this.lbl_VCurveValue, this.VCurve.AutoFocusValue.ToString());
                CrossThread.Set_Label_Message(this.lbl_VCurvePosition_2, this.VCurve.AutoFocusPos_2.ToString());
                CrossThread.Set_Label_Message(this.lbl_VCurveValue_2, this.VCurve.AutoFocusValue_2.ToString());
                //this.Update();

                System.Threading.Thread.Sleep(100);
                //this.Update();
            } while (this.AutoFocus.FlowRun);

            // Tact Time
            timeSpend = DateTime.Now.Subtract(startTime);
            ts = timeSpend.TotalMilliseconds;
            CrossThread.Set_Label_Message(this.lbl_VCurve_TactTime, ts.ToString());
            CrossThread.Set_Label_Message(this.lbl_VCurvePosition, this.Motor.NowPos_Focuser().ToString());
            CrossThread.Set_Label_Message(this.lbl_VCurve_AutoFocus_Info, "Focused");
            CrossThread.Set_Label_BGColor(this.lbl_VCurve_AutoFocus_Info, Color.LightGreen);

            System.Threading.Thread.Sleep(100);
            CrossThread.Set_Label_Message(this.lbl_VCurvePosition, this.VCurve.AutoFocusPos.ToString());
            CrossThread.Set_Label_Message(this.lbl_VCurveValue, this.VCurve.AutoFocusValue.ToString());
            CrossThread.Set_Label_Message(this.lbl_VCurvePosition_2, this.VCurve.AutoFocusPos_2.ToString());
            CrossThread.Set_Label_Message(this.lbl_VCurveValue_2, this.VCurve.AutoFocusValue_2.ToString());

            // Update Focus Image
            MIL.MbufSave(@"D:\Image_Focused.tif", this.Grabber.grabImage);
        }

        private void btn_VCurve_Stop_Click(object sender, EventArgs e)
        {
            if (this.VCurve != null)
            {
                this.VCurve.Stop();
            }
        }

        #endregion  V Curve 

        #region PatGen 

        private void Btn_PatGen_Connect_Click(object sender, EventArgs e)
        {
            if (this.PatGen.IsOpen == false)
            {
                if (this.cb_PatGen_ComPort.Text.Length > 2)
                {
                    try
                    {
                        this.PatGen.PortName = this.cb_PatGen_ComPort.Text;
                        this.PatGen.BaudRate = 57600;
                        this.PatGen.Open();
                    }
                    catch { }
                }
            }
            else if (this.PatGen.IsOpen == true)
            {
                this.PatGen.Close();
            }
        }
        private void Btn_PatGen_Close_Click(object sender, EventArgs e)
        {
            if (this.PatGen.IsOpen == true)
            {
                this.PatGen.Close();
            }
        }

        private void Btn_PatGen_SetPattern_Click(object sender, EventArgs e)
        {
            byte[] buffer = { 0x46, 0x00, 0x00, 0x00 };  //0x46 : 是RD 訂的 Command flag

            buffer[1] = (byte)nUD_PatGen_SetColor_R.Value;
            buffer[2] = (byte)nUD_PatGen_SetColor_G.Value;
            buffer[3] = (byte)nUD_PatGen_SetColor_B.Value;

            try
            {
                if (this.PatGen.IsOpen)
                {
                    this.PatGen.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    this.PatGen.PortName = this.cb_PatGen_ComPort.Text;
                    this.PatGen.BaudRate = 57600;
                    this.PatGen.Open();

                    this.PatGen.Write(buffer, 0, buffer.Length);

                    this.PatGen.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SetColor: {ex.Message}");
            }
        }

        private void btn_PatGen_ChangeDesktop_Click(object sender, EventArgs e)
        {
            string ImgPath = Tbx_DesktopImg.Text;
            DesktopManager.SetBackgroud(ImgPath);
        }

        #endregion PatGen

        #region Auto Plane

        public delegate void PanelPointDelegate(Panel panel, Point[] TargetPoints, Point[] SourcePoints);

        public void ReDrawBox(Panel panel, System.Drawing.Point[] TargetPoints, System.Drawing.Point[] SourcePoints)
        {
            if (panel.InvokeRequired)
            {
                PanelPointDelegate action = new PanelPointDelegate(ReDrawBox);

                panel.BeginInvoke(action, panel, TargetPoints, SourcePoints);
            }
            else
            {
                MDisplay.ClearGrid();
                MDisplay.DrawBox(TargetPoints, SourcePoints);
            }
        }

        private void CalculateIntersectionPoint(double[] Line1, double[] Line2, ref Point IntersectionPoint)
        {

            try //A1X+B1Y+C1=0 A2X+B2Y+C2=0
            {
                if ((Line1[0] * Line2[1] - Line1[1] * Line2[0]) == 0)
                {
                    IntersectionPoint.X = 0;
                    IntersectionPoint.Y = 0;
                }
                else
                {
                    IntersectionPoint.X = Convert.ToInt32((Line1[1] * Line2[2] - Line2[1] * Line1[2]) / (Line1[0] * Line2[1] - Line2[0] * Line1[1]));
                    IntersectionPoint.Y = Convert.ToInt32((Line1[0] * Line2[2] - Line2[0] * Line1[2]) / (Line2[0] * Line1[1] - Line1[0] * Line2[1]));
                }
            }
            catch
            {
                IntersectionPoint.X = 0;
                IntersectionPoint.Y = 0;
            }
        }

        private void FindCornerbyEdgeFinder(MIL_ID mil_8U_SrcImag, ref Point[] cornerpoint)
        {
            //    P0----------P2
            //    |           |
            //    |           |
            //    P1----------P3

            MIL_INT ImageSizeX = MIL.M_NULL;
            MIL_INT ImageSizeY = MIL.M_NULL;
            MIL_ID MBlobContext = MIL.M_NULL;
            MIL_ID MBlobResult = MIL.M_NULL;
            MIL_ID MEdgeMarker = MIL.M_NULL;
            int BlobCount = 0;
            int[] BlobboxInfo = new int[4]; //minX, minY, maxX, maxY
            Point[] BlobboxCorner = new Point[4]; //0:lefttop, 1:leftdown, 2:righttop, 3:rightdown
            int EdgeCount = 0;
            double[] HLineCoef = new double[3];
            double[] VLineCoef = new double[3];

            try
            {
                MIL.MbufInquire(mil_8U_SrcImag, MIL.M_SIZE_X, ref ImageSizeX);
                MIL.MbufInquire(mil_8U_SrcImag, MIL.M_SIZE_Y, ref ImageSizeY);

                //blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobContext);
                MIL.MblobControl(MBlobContext, MIL.M_FOREGROUND_VALUE, MIL.M_NON_ZERO);
                MIL.MblobControl(MBlobContext, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);
                MIL.MblobControl(MBlobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(MBlobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(MBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobResult);
                MIL.MblobCalculate(MBlobContext, mil_8U_SrcImag, MIL.M_NULL, MBlobResult);
                MIL.MblobGetResult(MBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref BlobCount);
                if (BlobCount == 0) { return; }
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[0]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[1]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[2]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[3]);

                //blobcorner
                BlobboxCorner[0].X = BlobboxInfo[0];
                BlobboxCorner[0].Y = BlobboxInfo[1];
                BlobboxCorner[1].X = BlobboxInfo[0];
                BlobboxCorner[1].Y = BlobboxInfo[3];
                BlobboxCorner[2].X = BlobboxInfo[2];
                BlobboxCorner[2].Y = BlobboxInfo[1];
                BlobboxCorner[3].X = BlobboxInfo[2];
                BlobboxCorner[3].Y = BlobboxInfo[3];

                //edgeset
                MIL.MmeasAllocMarker(MyMil.MilSystem, MIL.M_EDGE, MIL.M_DEFAULT, ref MEdgeMarker);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_MODE, MIL.M_ENABLE, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_DELTA_POS, 5, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_DELTA_NEG, 5, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_TOLERANCE, 1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_ACCURACY, 0.1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_NUMBER, 1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_POLARITY, MIL.M_ANY, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_SEARCH_REGION_CLIPPING, MIL.M_MAXIMIZE_AREA, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_FILTER_TYPE, MIL.M_SHEN, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_SIZE, (BlobboxInfo[2] - BlobboxInfo[0]) / 3 * 2, (BlobboxInfo[3] - BlobboxInfo[1]) / 3 * 2);

                //findedge & crosspoint
                for (int cornerindex = 0; cornerindex < BlobboxCorner.Length; cornerindex += 1)
                {
                    //center
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_CENTER, BlobboxCorner[cornerindex].X, BlobboxCorner[cornerindex].Y);

                    //horizontal
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
                    MIL.MmeasFindMarker(MIL.M_DEFAULT, mil_8U_SrcImag, MEdgeMarker, MIL.M_LINE_EQUATION);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                    if (EdgeCount == 0) { continue; }
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[0]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[1]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[2]);

                    //vertical
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
                    MIL.MmeasFindMarker(MIL.M_DEFAULT, mil_8U_SrcImag, MEdgeMarker, MIL.M_LINE_EQUATION);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                    if (EdgeCount == 0) { continue; }
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[0]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[1]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[2]);

                    //crosspoint
                    CalculateIntersectionPoint(HLineCoef, VLineCoef, ref cornerpoint[cornerindex]);
                }

            }
            catch
            {

            }
            finally
            {
                MilNetHelper.MilBlobFree(ref MBlobContext);
                MilNetHelper.MilBlobFree(ref MBlobResult);
                MilNetHelper.MilMeasFree(ref MEdgeMarker);
            }
        }

        private void Check_AutoPlaneAlign_State()
        {
            TimeSpan timeSpend;
            double ts;
            DateTime startTime = DateTime.Now;
            Point[] TargetBoxPoints = this.AutoPlane.TargetBoxPoints;
            Point[] SourceBoxPoints = this.AutoPlane.SourceBoxPoints;

            do
            {
                this.Update_AutoPlaneAlign_RobotPosInfo();

                // Update Focus Image

                TargetBoxPoints = this.AutoPlane.TargetBoxPoints;
                SourceBoxPoints = this.AutoPlane.SourceBoxPoints;
                ReDrawBox(this.PnlGrabViewer, TargetBoxPoints, SourceBoxPoints);

                //Console.WriteLine($"[Draw Target Box Points] : P0{TargetBoxPoints[0].X},{TargetBoxPoints[0].Y}, P1{TargetBoxPoints[1].X},{TargetBoxPoints[1].Y} ");

                System.Threading.Thread.Sleep(300);
            } while (this.AutoPlane.FlowRun);

            // Tact Time
            timeSpend = DateTime.Now.Subtract(startTime);
            ts = timeSpend.TotalMilliseconds;

            if (this.AutoPlane.Proc_Step == AutoPlaneAlign_Step.Stop)
            {
                this.lbl_AutoPlaneAlign_Info.Text = "Stoped";
                this.lbl_AutoPlaneAlign_Info.BackColor = Color.Red;
            }
            else if (this.AutoPlane.Proc_Step == AutoPlaneAlign_Step.Finish)
            {
                CrossThread.Set_Label_Message(this.lbl_AutoPlaneAlign_Info, "Aligned");
                CrossThread.Set_Label_BGColor(this.lbl_AutoPlaneAlign_Info, Color.LightGreen);
            }
            else if (this.AutoPlane.Proc_Step == AutoPlaneAlign_Step.Error)
            {
                CrossThread.Set_Label_Message(this.lbl_AutoPlaneAlign_Info, "Error");
                CrossThread.Set_Label_BGColor(this.lbl_AutoPlaneAlign_Info, Color.Red);
            }

            CrossThread.Set_Label_Message(this.lbl_AutoPlaneAlign_TactTime, ts.ToString());


            System.Threading.Thread.Sleep(1000);

            // Update Focus Image
            ReDrawBox(this.PnlGrabViewer, this.AutoPlane.TargetBoxPoints, this.AutoPlane.SourceBoxPoints);
        }

        private void timer_AutoPlaneAlign_Tick(object sender, EventArgs e)
        {
            if (this.AutoPlane == null)
                return;
            else
                this.Update_AutoPlaneAlign_RobotPosInfo();
        }

        private void Update_AutoPlaneAlign_RobotPosInfo()
        {
            if (this.AutoPlane != null)
            {

                if (this.AutoPlane.Update_RobotPosInfo())
                {
                    double roll = 0.0;
                    double pitch = 0.0;
                    double yaw = 0.0;

                    //Update Robot Info             
                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpX, this.AutoPlane.urRobotPos.X);
                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpY, this.AutoPlane.urRobotPos.Y);
                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpZ, this.AutoPlane.urRobotPos.Z);

                    // Radian To Theta
                    //roll  = Math.Round(this.taskAutoPlaneAlign.urRobotPos.Roll * _RadToTheta, 4);
                    //pitch = Math.Round(this.taskAutoPlaneAlign.urRobotPos.Pitch * _RadToTheta, 4);
                    //yaw   = Math.Round(this.taskAutoPlaneAlign.urRobotPos.Yaw * _RadToTheta, 4);
                    roll = Math.Round(this.AutoPlane.urRobotPos.Roll, 4);
                    pitch = Math.Round(this.AutoPlane.urRobotPos.Pitch, 4);
                    yaw = Math.Round(this.AutoPlane.urRobotPos.Yaw, 4);

                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpRoll, roll);
                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpPitch, pitch);
                    CrossThread.Set_Numeric_Message(this.nUD_UrRobot_TcpYaw, yaw);
                }
            }
        }

        private void btn_UrRobot_DrawBox_Click(object sender, EventArgs e)
        {
            int Threshold = (int)this.nUD_UrRobot_Threshold.Value;



            this.AutoPlane.Calculate_CornerPoints_And_BoxPoints(Threshold);
            ReDrawBox(this.PnlGrabViewer, this.AutoPlane.TargetBoxPoints, this.AutoPlane.SourceBoxPoints);
        }

        private void btn_UrRobot_LoadImage_Click(object sender, EventArgs e)
        {
            MIL_ID imgSrc = MIL.M_NULL;

            OpenFileDialog fileDialog = new OpenFileDialog();

            try
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    MIL.MbufRestore(fileDialog.FileName, MyMil.MilSystem, ref imgSrc);
                }
                else
                {
                    return;
                }

                Point[] Source_Points = new Point[4];
                Point[] Target_Points = new Point[4];

                this.CalculateCornerPoints_And_BoxPoints(imgSrc, true, ref Source_Points, ref Target_Points);

                ReDrawBox(this.PnlGrabViewer, Target_Points, Source_Points);
            }
            catch (Exception ex)
            {
                throw new Exception($"btn_UrRobot_LoadImage_Click: {ex.Message}");
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref imgSrc);
            }
        }

        private void CalculateCornerPoints_And_BoxPoints(MIL_ID sourceImage, bool isGetBox, ref Point[] cornerPoint, ref Point[] BoxPoint)
        {
            //    P0----------P1
            //    |           |
            //    |           |
            //    P3----------P2

            MIL_ID blobContext = MIL.M_NULL;
            MIL_ID blobResult = MIL.M_NULL;
            MIL_ID mil_8U_SrcImag = MIL.M_NULL;

            int[] BlobboxInfo = new int[4];      //minX, minY, maxX, maxY
            Point[] Corner_Point = new Point[4];

            int blobCount = 0;
            int OpenCount = 5;

            bool Debug = false;


            try
            {
                MIL_INT SizeX = MIL.MbufInquire(sourceImage, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT SizeY = MIL.MbufInquire(sourceImage, MIL.M_SIZE_Y, MIL.M_NULL);

                // Blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult);
                //搜尋最大面積的 Blob
                MIL.MblobControl(blobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(blobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

                for (int i = 0; i < 3; i++)
                    MIL.MimConvolve(sourceImage, sourceImage, MIL.M_SMOOTH);

                MIL.MimArith(sourceImage, MIL.M_NULL, sourceImage, MIL.M_NOT);
                MIL.MimBinarize(sourceImage, sourceImage, MIL.M_BIMODAL + MIL.M_GREATER_OR_EQUAL, MIL.M_NULL, MIL.M_NULL);

                MIL.MbufAlloc2d(MyMil.MilSystem, SizeX, SizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_8U_SrcImag);
                MIL.MbufCopy(sourceImage, mil_8U_SrcImag);

                MIL.MblobCalculate(blobContext, mil_8U_SrcImag, MIL.M_NULL, blobResult);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);
                MIL.MblobDraw(MIL.M_DEFAULT, blobResult, mil_8U_SrcImag, MIL.M_DRAW_HOLES, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                MIL.MimOpen(mil_8U_SrcImag, mil_8U_SrcImag, OpenCount, MIL.M_BINARY);

                if (Debug)
                {
                    MIL.MbufSave(@"D:/mil_8U_SrcImag.tif", mil_8U_SrcImag);
                }

                if (cornerPoint == null)
                    cornerPoint = new Point[4];

                this.FindCornerbyEdgeFinder(mil_8U_SrcImag, ref Corner_Point);

                //Left - Top
                cornerPoint[0].X = Corner_Point[0].X;
                cornerPoint[0].Y = Corner_Point[0].Y;
                //Right - Top
                cornerPoint[1].X = Corner_Point[2].X;
                cornerPoint[1].Y = Corner_Point[2].Y;
                //Right - Bottom
                cornerPoint[2].X = Corner_Point[3].X;
                cornerPoint[2].Y = Corner_Point[3].Y;
                //Left - Bottom
                cornerPoint[3].X = Corner_Point[1].X;
                cornerPoint[3].Y = Corner_Point[1].Y;

                // Get Box Points
                if (isGetBox)
                {
                    MIL.MblobCalculate(blobContext, mil_8U_SrcImag, MIL.M_NULL, blobResult);
                    MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[0]);
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[1]);
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[2]);
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[3]);

                    //--- Box Corner ---
                    // Left-Top
                    BoxPoint[0].X = BlobboxInfo[0];
                    BoxPoint[0].Y = BlobboxInfo[1];
                    // Right-Top
                    BoxPoint[1].X = BlobboxInfo[2];
                    BoxPoint[1].Y = BlobboxInfo[1];
                    // Right-Bottom
                    BoxPoint[2].X = BlobboxInfo[2];
                    BoxPoint[2].Y = BlobboxInfo[3];
                    // Left-Bottom
                    BoxPoint[3].X = BlobboxInfo[0];
                    BoxPoint[3].Y = BlobboxInfo[3];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("[CalculateCornerPoints] " + ex.Message + "(" + ex.StackTrace + ")");
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref mil_8U_SrcImag);
                MilNetHelper.MilBlobFree(ref blobContext);
                MilNetHelper.MilBlobFree(ref blobResult);
            }
        }

        private void btn_UrRobot_Connect_Click(object sender, EventArgs e)
        {
            if (this.Robot.IsConnected())
            {
                if (this.Robot.IsConnected())
                    this.Robot.Disconnect();

                this.timer_AutoPlaneAlign.Enabled = false;
                this.lbl_UrRobot_Connected.Text = "Disconnected";
            }
            else if (this.Robot.Connect(GlobalVar.SD.UrRobot_IPAddress, 30000))
            {
                this.lbl_UrRobot_Connected.Text = "Connected";

                // task AutoPlaneAlign
                if (this.Robot.IsConnected() && this.Grabber != null && this.AutoPlane == null)
                    this.AutoPlane = new Task_AutoPlane(MyMil.MilSystem, Grabber, this.Robot, this.Info, GlobalVar.SD.NormalAngle_0D);

                this.timer_AutoPlaneAlign.Enabled = true;
                //this.Update_AutoPlaneAlign_RobotPosInfo();
            }

            // Initial Jog Step : 0 ==> Small
            this.cB_UrRobot_JogStep.SelectedIndex = 0;
        }

        private void btn_UrRobot_Move_Click(object sender, EventArgs e)
        {
            if (this.Robot.IsConnected())
            {
                double x = Convert.ToDouble(this.nUD_UrRobot_TcpX.Value);
                double y = Convert.ToDouble(this.nUD_UrRobot_TcpY.Value);
                double z = Convert.ToDouble(this.nUD_UrRobot_TcpZ.Value);

                // Degree To Radian
                //double roll = Convert.ToDouble(this.nUD_UrRobot_TcpRoll.Value) * _ThetaToRad;
                //double pitch = Convert.ToDouble(this.nUD_UrRobot_TcpPitch.Value) * _ThetaToRad;
                //double yaw = Convert.ToDouble(this.nUD_UrRobot_TcpYaw.Value) * _ThetaToRad;
                double roll = Convert.ToDouble(this.nUD_UrRobot_TcpRoll.Value);
                double pitch = Convert.ToDouble(this.nUD_UrRobot_TcpPitch.Value);
                double yaw = Convert.ToDouble(this.nUD_UrRobot_TcpYaw.Value);

                this.AutoPlane.Set_RobotPosInfo(x, y, z, roll, pitch, yaw);
                this.AutoPlane.MoveToPos();

                do
                {

                    System.Threading.Thread.Sleep(300);
                    this.Update();
                } while (this.AutoPlane.IsMoving());

                System.Threading.Thread.Sleep(500);
            }
        }

        private void btn_UrRobot_AutoPlaneAlign_Click(object sender, EventArgs e)
        {
            if (this.AutoPlane == null)
                return;


            if (this.cB_UrRobot_JogStep.Text == "")
                this.cB_UrRobot_JogStep.SelectedIndex = 0;

            // _Para[0] : Move Step Index (0 : slow, 1 : medium, 2 : fast)
            // _Para[1] : Move Tolerance
            // _Para[2] : Move Acc
            // _Para[3] : Move Speed
            // _Para[4] : Move Jog Multiply
            // _Para[5] : ImgProc Threshold
            AutoPlane_FlowPara param = new AutoPlane_FlowPara();

            param.MoveStepIdx = this.cB_UrRobot_JogStep.SelectedIndex;
            param.Deviation_Tolerance = (int)this.nUD_UrRobot_PosTolerance.Value;
            param.MoveAcc = (double)this.nUD_UrRobot_Acc.Value;
            param.MoveSpeed = (double)this.nUD_UrRobot_Speed.Value;
            param.JogMultiply = (double)this.nUD_UrRobot_JogMultiply.Value;
            param.ImgProc_Threshold = (int)this.nUD_UrRobot_Threshold.Value;

            this.lbl_AutoPlaneAlign_Info.Text = "Moving";
            this.lbl_AutoPlaneAlign_Info.BackColor = Color.Red;

            //startTime = DateTime.Now;
            this.AutoPlane.Start(param);

            // Start Check AutoPlaneAlign State Thread
            ThreadStart AutoPlaneAlignState = null;
            Thread AutoPlaneAlignStateThread = null;

            this.timer_AutoPlaneAlign.Enabled = false;

            AutoPlaneAlignState = new ThreadStart(Check_AutoPlaneAlign_State);
            AutoPlaneAlignStateThread = new Thread(AutoPlaneAlignState);
            AutoPlaneAlignStateThread.Start();

        }

        private void btn_UrRobot_Stop_Click(object sender, EventArgs e)
        {
            if (this.AutoPlane != null)
            {
                this.AutoPlane.MoveStop();
                System.Threading.Thread.Sleep(500);

                this.AutoPlane.FlowRun = false;
                this.AutoPlane.Proc_Step = AutoPlaneAlign_Step.Stop;
                this.lbl_AutoPlaneAlign_Info.Text = "Stoped";
                this.lbl_AutoPlaneAlign_Info.BackColor = Color.Red;
            }
        }

        private void btn_UrRobot_Increase_X_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.X, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_X_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.X, Enum_Jog.Minus);
        }

        private void btn_UrRobot_Increase_Y_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Y, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_Y_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Y, Enum_Jog.Minus);
        }

        private void btn_UrRobot_Increase_Z_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Z, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_Z_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Z, Enum_Jog.Minus);
        }

        private void btn_UrRobot_Increase_Roll_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Roll, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_Roll_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Roll, Enum_Jog.Minus);
        }

        private void btn_UrRobot_Increase_Pitch_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Pitch, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_Pitch_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Pitch, Enum_Jog.Minus);
        }

        private void btn_UrRobot_Increase_Yaw_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Yaw, Enum_Jog.Add);
        }

        private void btn_UrRobot_decrease_Yaw_Click(object sender, EventArgs e)
        {
            this.UrRobot_Jog(Enum_RobotDoF.Yaw, Enum_Jog.Minus);
        }

        #endregion AutoPlane

        #region CameraAnalyze

        private void BtnCameraAnalyzeStart_Click(object sender, EventArgs e)
        {
            #region Read Feature Data

            string[] RtbxDatas = RtbxCameraFeature.Text.Split('\n');
            List<int> FeatureDatas = new List<int>();

            foreach (string Data in RtbxDatas)
            {
                if (Data.Trim().Length == 0)
                {
                    continue;
                }

                FeatureDatas.Add(Convert.ToInt32(Data));
            }

            #endregion

            #region Read ROI

            int RoiCnt = -1;
            MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT,
                MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            List<Rectangle> RoiDatas = new List<Rectangle>();


            for (MIL_INT i = 0; i < RoiCnt; i++)
            {
                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                // Get Selected

                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(MDisplay.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                RoiDatas.Add(new Rectangle(X, Y, W, H));
            }

            #endregion

            TaskCameraFeatureAnalyze TaskFeature = new TaskCameraFeatureAnalyze(Grabber, Motor, Info);
            TaskCameraFeatureAnalyze.FlowPara FlowPara = new TaskCameraFeatureAnalyze.FlowPara {
                OutputFolder = Tbx_CameraFeature.Text,
                RoiList = RoiDatas,
                xyzIpStep = CbxCameraAnalyzeFW.SelectedIndex,
                TargetGray = FeatureDatas,
                RecipeData = GlobalVar.OmsParam,
                Type = TaskCameraFeatureAnalyze.AnalyzeType.Feature_1,
            };

            TaskFeature.Start((object)FlowPara);
        }

        private void BtnCameraFeatureClear_Click(object sender, EventArgs e)
        {
            RtbxCameraFeature.Text = "";
        }

        private void CbxCameraAnalyze_SelectedIndexChanged(object sender, EventArgs e)
        {
            string SelectType = CbxCameraAnalyze.Text;

            switch (SelectType)
            {
                case "Feature - 1": LabelAnalyzeTitle.Text = "Target Mean"; break;
                case "Feature - 2": LabelAnalyzeTitle.Text = "PG Level"; break;
            }
        }

        #endregion CameraAnalyze

        #region Hardware Tool

        private void Btn_MotorTool_Click(object sender, EventArgs e)
        {
            switch (GlobalVar.DeviceType)
            {
                case EnumMotoControlPackage.MS5515M:
                    {
                        Form existingForm = Application.OpenForms.OfType<MS5515M_Tool>().FirstOrDefault();

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
                            // 如果不存在則創建新表單
                            MS5515M motor = (MS5515M)this.Motor;
                            MS5515M_Tool Tool = new MS5515M_Tool(ref motor);
                            Tool.Show();
                        }
                    }
                    break;

                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    {
                        Form existingForm = Application.OpenForms.OfType<AuoMotor_Tool>().FirstOrDefault();

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
                            // 如果不存在則創建新表單
                            AuoMotorCtrl motor = (AuoMotorCtrl)this.Motor;
                            AuoMotor_Tool Tool = new AuoMotor_Tool(ref motor);
                            Tool.Show();
                        }
                    }
                    break;

                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    {
                        Form existingForm = Application.OpenForms.OfType<AiryUnitCtrl_4in1_TCPIP_Tool>().FirstOrDefault();


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
                            // 如果不存在則創建新表單
                            AiryUnitCtrl_4in1_TCPIP motor = (AiryUnitCtrl_4in1_TCPIP)this.Motor;
                            AiryUnitCtrl_4in1_TCPIP_Tool Tool = new AiryUnitCtrl_4in1_TCPIP_Tool(ref motor);
                            Tool.Show();
                        }
                    }
                    break;

                case EnumMotoControlPackage.CircleTac:
                    {
                        Form existingForm = Application.OpenForms.OfType<CircleTacCtrl_Tool>().FirstOrDefault();


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
                            // 如果不存在則創建新表單
                            CircleTacCtrl motor = (CircleTacCtrl)this.Motor;
                            CircleTacCtrl_Tool Tool = new CircleTacCtrl_Tool(ref motor);
                            Tool.Show();
                        }
                    }
                    break;

                case EnumMotoControlPackage.CircleTac_Old:
                    {
                        Form existingForm = Application.OpenForms.OfType<CircleTacCtrl_Tool_Old>().FirstOrDefault();


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
                            // 如果不存在則創建新表單
                            CircleTacCtrl_Old motor = (CircleTacCtrl_Old)this.Motor;
                            CircleTacCtrl_Tool_Old Tool = new CircleTacCtrl_Tool_Old(ref motor);
                            Tool.Show();
                        }
                    }
                    break;
            }
        }

        private void Btn_Robot_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<RobotUnitForm>().FirstOrDefault();

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
                RobotUnitForm Test = new RobotUnitForm();
                Test.SetMachine(ref Robot);
                ////RobotUnitForm Test = new RobotUnitForm(ref urRobot);
                Test.Show();
            }
        }

        private void Btn_XPLC_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<X_PLC_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                X_PLC_Tool Tool = new X_PLC_Tool(ref PLC);
                Tool.Show();
            }
        }

        private void Btn_D65_Tool_Click(object sender, EventArgs e)
        {

            Form existingForm = Application.OpenForms.OfType<D65_Light_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                D65_Light_Tool Tool = new D65_Light_Tool(ref D65);
                Tool.Show();
            }
        }

        private void Btn_FwTool_Click(object sender, EventArgs e)
        {

        }

        private void Btn_SR3_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<SR3_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                SR3_Tool Tool = new SR3_Tool(ref SR3);
                Tool.Show();
            }
        }

        private void Btn_CornerstoneB_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<CornerstoneB_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                CornerstoneB_Tool Tool = new CornerstoneB_Tool(ref CornerstoneB);
                Tool.Show();
            }
        }

        private void Btn_PowerSupply_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<PowerSupply_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                PowerSupply_Tool Tool = new PowerSupply_Tool(ref PowerSupply);
                Tool.Show();
            }
        }


        private void Btn_A_Light_UnitForm_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms.OfType<LightSourceA_Tool>().FirstOrDefault();

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
                // 如果不存在則創建新表單
                LightSourceA_Tool Tool = new LightSourceA_Tool(ref LightSourceA);
                Tool.Show();
            }
        }

        #endregion Hardware Tool

        #region FFC  

        private void Btn_FFC_Create_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                FFC_FlowPara.WriteXML(FlowPara_FFC, file);
            }
        }

        private void Btn_FFC_Create_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_FFC = FFC_FlowPara.ReadXML(dialog.FileName);
                PropertyGrid_FFC_Create_Config.SelectedObject = FlowPara_FFC;
            }
        }

        private void Btn_FFC_Create_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            FFC_FlowPara Para = new FFC_FlowPara();
            Para.Clone(this.FlowPara_FFC);

            FFC_Create.Start(Para);
        }

        private void FFC_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_FFC_Create_LogMsg);
        }

        private void FFC_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_FFC_Create_StepMonitor);
        }

        private void btnFfcExecute_Click(object sender, EventArgs e)
        {
            FfcCalibrationFilePath correctionFile = new FfcCalibrationFilePath();
            correctionFile.Offset = tbxFfcOffset.Text;
            correctionFile.Flat = tbxFfcFlat.Text;
            correctionFile.Dark = tbxFfcDark.Text;

            FFC(MDisplay.MilImage, correctionFile, ref MDisplay.MilImage);
        }

        private void Btn_FFC_Create_Stop_Click(object sender, EventArgs e)
        {
            if (FFC_Create != null)
            {
                FFC_Create.Stop();
            }
        }

        #endregion  FFC Create 

        #region Heat Map 

        private void GetImageBound(string path)
        {
            MIL_ID image = MIL.M_NULL;
            float min = 0;
            float max = 0;

            MIL.MbufRestore(path, MyMil.MilSystem, ref image);
            MyMilIp.GetLimit(image, ref min, ref max);

            lblImageMinimum.Text = min.ToString();
            lblImageMaximum.Text = max.ToString();
        }

        private void btnHeatMapExecute_Click(object sender, EventArgs e)
        {
            MIL_ID sourceImage = MIL.M_NULL;
            MIL_ID remapImage = MIL.M_NULL;
            MIL_ID heatMapImage = MIL.M_NULL;

            float low = Convert.ToSingle(tbxHeatMapMin.Text);
            float high = Convert.ToSingle(tbxHeatMapMax.Text);
            float min = 0;
            float max = 4095;

            MIL.MbufRestore(tbxHeatMapSourceImage.Text, MyMil.MilSystem, ref sourceImage);

            remapImage = RemapImageTo16Unsign(sourceImage, ref min, ref max, low, high);
            heatMapImage = DrawHeatMap(remapImage);


            // Free
            MilNetHelper.MilBufferFree(ref sourceImage);
            MilNetHelper.MilBufferFree(ref remapImage);
            MilNetHelper.MilBufferFree(ref heatMapImage);
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            int max = Math.Max(this.trackBar1.Value, this.trackBar2.Value);
            int min = Math.Min(this.trackBar1.Value, this.trackBar2.Value);

            this.tbxHeatMapMax.Text = max.ToString();
            this.tbxHeatMapMin.Text = min.ToString();

            this.trackBar1.Value = min;
            this.trackBar2.Value = max;

            this.trackBar1.Refresh();
            this.trackBar2.Refresh();
        }

        private void btnHeatMapMin_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.trackBar1.Value = Convert.ToInt32(this.tbxHeatMapMin.Text);
                    this.trackBar1.Refresh();
                }
            }
            catch   //小數會錯
            {
            }
        }

        private void btnHeatMapMax_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.trackBar2.Value = Convert.ToInt32(this.tbxHeatMapMax.Text);
                    this.trackBar2.Refresh();
                }
            }
            catch
            {
            }
        }

        #endregion Heat Map 

        #region File Load

        private void FileLoad_Click(object sender, EventArgs e)
        {
            string corrTextBoxTag = ((MaterialButton)sender).Tag.ToString();

            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                corrTextBox = null;
                GetTextBox(Btn_AutosPattern_SampleCsv, corrTextBoxTag);

                if (corrTextBox != null)
                {
                    corrTextBox.Text = fileDialog.FileName;
                }
                else
                {
                    MessageBox.Show("write path failed, check tag");
                    return;
                }

                switch (corrTextBoxTag)
                {
                    case "tbxHeatMapSourceImage":
                        GetImageBound(fileDialog.FileName);
                        break;
                }
            }
            else
            {
                return;
            }
        }




        #endregion

        private void ManualForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.liveViewFlag = false;

            AutoFocus.UpdateFocusValue -= TaskAutoFocus_UpdateFocusValue;
            AutoFocus_New.UpdateFocusValue -= TaskAutoFocus_UpdateFocusValue;

            FFC_Create.MonitorStep -= FFC_MonitorStep;
            FFC_Create.LogMsg -= FFC_LogMsg;

            LightSourceA_Create.MonitorStep -= LightSourceA_MonitorStep;
            LightSourceA_Create.LogMsg -= LightSourceA_LogMsg;

            NoiseDetection.MonitorStep -= NoiseDetection_MonitorStep;
            NoiseDetection.LogMsg -= NoiseDetection_LogMsg;

            TiltCorrection.MonitorStep -= TiltCorrection_MonitorStep;
            TiltCorrection.LogMsg -= TiltCorrection_LogMsg;

            MonoChromatorMeasure.MonitorStep -= MonoChromatorMeasure_MonitorStep;
            MonoChromatorMeasure.LogMsg -= MonoChromatorMeasure_LogMsg;

            AutoPattern.MonitorStep -= AutoPattern_MonitorStep;
            AutoPattern.LogMsg -= AutoPattern_LogMsg;

            AutoWD.MonitorStep -= AutoWD_MonitorStep;
            AutoWD.LogMsg -= AutoWD_LogMsg;
        }

        #region LightSourceA

        private void RBtn_LightSourceA_Para_CheckedChanged(object sender, EventArgs e)
        {
            if (RBtn_LightSourceA_BaseAlign.Checked)
            {
                PropertyGrid_LightSourceA_Create_Config.SelectedObject = FlowPara_LightSourceA.BaseAlign_Para;
            }
            ;

            if (RBtn_LightSourceA_Flow.Checked)
            {
                PropertyGrid_LightSourceA_Create_Config.SelectedObject = FlowPara_LightSourceA;
            }
            ;
        }

        private void Btn_LightSourceA_Create_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_LightSourceA = LightSourceA_Create_FlowPara.ReadXML(dialog.FileName);

                if (RBtn_LightSourceA_BaseAlign.Checked)
                {
                    PropertyGrid_LightSourceA_Create_Config.SelectedObject = FlowPara_LightSourceA.BaseAlign_Para;
                }
                ;

                if (RBtn_LightSourceA_Flow.Checked)
                {
                    PropertyGrid_LightSourceA_Create_Config.SelectedObject = FlowPara_LightSourceA;
                }
                ;
            }
        }

        private void Btn_LightSourceA_Create_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                LightSourceA_Create_FlowPara.WriteXML(FlowPara_LightSourceA, file);
            }
        }

        private void Btn_LightSourceA_Create_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            LightSourceA_Create_FlowPara Para = new LightSourceA_Create_FlowPara();
            Para.Clone(this.FlowPara_LightSourceA);

            LightSourceA_Create.Start(Para);
        }

        private void LightSourceA_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_LightSourceA_Create_LogMsg);
        }

        private void LightSourceA_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_LightSourceA_Create_StepMonitor);
        }

        private void Btn_LightSourceA_Create_Stop_Click(object sender, EventArgs e)
        {
            if (LightSourceA_Create != null)
            {
                LightSourceA_Create.Stop();
            }
        }

        private void MenuItem_LightSourceA_GetRobot_Click(object sender, EventArgs e)
        {
            if (this.Robot != null && this.Robot.IsConnected())
            {
                try
                {
                    double X = this.Robot.m_ClsUrStatus.TcpPose[0];
                    double Y = this.Robot.m_ClsUrStatus.TcpPose[1];
                    double Z = this.Robot.m_ClsUrStatus.TcpPose[2];
                    double roll = this.Robot.m_ClsUrStatus.TcpPose[3] * 180D / Math.PI;
                    double pitch = this.Robot.m_ClsUrStatus.TcpPose[4] * 180D / Math.PI;
                    double yaw = this.Robot.m_ClsUrStatus.TcpPose[5] * 180D / Math.PI;

                    if (PropertyGrid_LightSourceA_Create_Config.SelectedObject == FlowPara_LightSourceA.BaseAlign_Para)
                    {
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotX = X;
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotY = Y;
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotZ = Z;
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotRoll = roll;
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotPitch = pitch;
                        FlowPara_LightSourceA.BaseAlign_Para.InitPos_RobotYaw = yaw;
                    }
                    else
                    {
                        FlowPara_LightSourceA.MeasurePos_RobotX = X;
                        FlowPara_LightSourceA.MeasurePos_RobotY = Y;
                        FlowPara_LightSourceA.MeasurePos_RobotZ = Z;
                        FlowPara_LightSourceA.MeasurePos_RobotRoll = roll;
                        FlowPara_LightSourceA.MeasurePos_RobotPitch = pitch;
                        FlowPara_LightSourceA.MeasurePos_RobotYaw = yaw;
                    }

                    PropertyGrid_LightSourceA_Create_Config.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Get Robot Pos Fail : {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Error! Robot is not Connected");
            }
        }


        #endregion

        #region Noise Detection

        private void MenuItem_NoiseDetection_GetRobot_Click(object sender, EventArgs e)
        {
            if (this.Robot != null && this.Robot.IsConnected())
            {
                try
                {
                    double X = this.Robot.m_ClsUrStatus.TcpPose[0];
                    double Y = this.Robot.m_ClsUrStatus.TcpPose[1];
                    double Z = this.Robot.m_ClsUrStatus.TcpPose[2];
                    double roll = this.Robot.m_ClsUrStatus.TcpPose[3] * 180D / Math.PI;
                    double pitch = this.Robot.m_ClsUrStatus.TcpPose[4] * 180D / Math.PI;
                    double yaw = this.Robot.m_ClsUrStatus.TcpPose[5] * 180D / Math.PI;


                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotX = X;
                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotY = Y;
                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotZ = Z;
                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotRoll = roll;
                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotPitch = pitch;
                    FlowPara_NoiseDetection.BaseAlign_Para.InitPos_RobotYaw = yaw;

                    PropertyGrid_NoiseDetection_Config.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Get Robot Pos Fail : {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Error! Robot is not Connected");
            }
        }


        private void NoiseDetection_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_NoiseDetection_LogMsg);
        }

        private void NoiseDetection_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_NoiseDetection_StepMonitor);
        }

        private void Btn_NoiseDetection_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_NoiseDetection = NoiseDetection_FlowPara.ReadXML(dialog.FileName);

                if (RBtn_NoiseDetection_BaseAlign.Checked)
                {
                    PropertyGrid_NoiseDetection_Config.SelectedObject = FlowPara_NoiseDetection.BaseAlign_Para;
                }
                ;

                if (RBtn_NoiseDetection_Flow.Checked)
                {
                    PropertyGrid_NoiseDetection_Config.SelectedObject = FlowPara_NoiseDetection;
                }
                ;
            }
        }

        private void Btn_NoiseDetection_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                NoiseDetection_FlowPara.WriteXML(FlowPara_NoiseDetection, file);
            }
        }

        private void RBtn_NoiseDetection_Para_CheckedChanged(object sender, EventArgs e)
        {
            if (RBtn_NoiseDetection_BaseAlign.Checked)
            {
                PropertyGrid_NoiseDetection_Config.SelectedObject = FlowPara_NoiseDetection.BaseAlign_Para;
            }
            ;

            if (RBtn_NoiseDetection_Flow.Checked)
            {
                PropertyGrid_NoiseDetection_Config.SelectedObject = FlowPara_NoiseDetection;
            }
            ;
        }

        private void Btn_NoiseDetection_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            NoiseDetection_FlowPara Para = new NoiseDetection_FlowPara();
            Para.Clone(this.FlowPara_NoiseDetection);

            NoiseDetection.Start(Para);
        }

        private void Btn_NoiseDetection_Stop_Click(object sender, EventArgs e)
        {
            if (NoiseDetection != null)
            {
                NoiseDetection.Stop();
            }
        }

        #endregion

        #region Tilt Correction

        private void TiltCorrection_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_TiltCorrection_LogMsg);
        }

        private void TiltCorrection_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_TiltCorrection_StepMonitor);
        }

        private void Btn_TiltCorrection_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_TiltCorrection = TiltCorrection_FlowPara.ReadXML(dialog.FileName);

                if (RBtn_TiltCorrection_BaseAlign.Checked)
                {
                    PropertyGrid_TiltCorrection_Config.SelectedObject = FlowPara_TiltCorrection.BaseAlign_Para;
                }
                ;

                if (RBtn_TiltCorrection_Flow.Checked)
                {
                    PropertyGrid_TiltCorrection_Config.SelectedObject = FlowPara_TiltCorrection;
                }
                ;
            }
        }

        private void Btn_TiltCorrection_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                TiltCorrection_FlowPara.WriteXML(FlowPara_TiltCorrection, file);
            }
        }

        private void RBtn_TiltCorrection_Flow_CheckedChanged(object sender, EventArgs e)
        {
            if (RBtn_TiltCorrection_BaseAlign.Checked)
            {
                PropertyGrid_TiltCorrection_Config.SelectedObject = FlowPara_TiltCorrection.BaseAlign_Para;
            }
            ;

            if (RBtn_TiltCorrection_Flow.Checked)
            {
                PropertyGrid_TiltCorrection_Config.SelectedObject = FlowPara_TiltCorrection;
            }
            ;
        }

        private void Btn_TiltCorrection_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            TiltCorrection_FlowPara Para = new TiltCorrection_FlowPara();
            Para.Clone(this.FlowPara_TiltCorrection);

            TiltCorrection.Start(Para);
        }

        private void Btn_TiltCorrection_Stop_Click(object sender, EventArgs e)
        {
            if (TiltCorrection != null)
            {
                TiltCorrection.Stop();
            }
        }

        #endregion

        #region MonoChromatorMeasure

        private void Btn_MonoChromatorMeasure_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_MonoChromatorMeasure = MonoChromatorMeasure_FlowPara.ReadXML(dialog.FileName);
                PropertyGrid_MonoChromatorMeasure_Config.SelectedObject = FlowPara_MonoChromatorMeasure;
            }
        }

        private void Btn_MonoChromatorMeasure_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                MonoChromatorMeasure_FlowPara.WriteXML(FlowPara_MonoChromatorMeasure, file);
            }
        }

        private void Btn_MonoChromatorMeasure_Create_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            MonoChromatorMeasure_FlowPara Para = new MonoChromatorMeasure_FlowPara();
            Para.Clone(this.FlowPara_MonoChromatorMeasure);

            MonoChromatorMeasure.Start(Para);
        }

        private void Btn_MonoChromatorMeasure_Create_Stop_Click(object sender, EventArgs e)
        {
            if (MonoChromatorMeasure != null)
            {
                MonoChromatorMeasure.Stop();
            }
        }

        private void MonoChromatorMeasure_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_MonoChromatorMeasure_LogMsg);
        }

        private void MonoChromatorMeasure_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_MonoChromatorMeasure_StepMonitor);
        }

        private void Btn_MonoChromatorMeasure_SampleCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.csv*)|*.csv*";
            dialog.FileName = "Pattern.csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                try
                {
                    StringBuilder SB = new StringBuilder();

                    List<string> TitleList = new List<string>();

                    TitleList.Add("Wave Length");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");

                    //Info
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{50}");

                    string Info = string.Join(",", InfoList);

                    SB.Append(Info + "\r\n");

                    StreamWriter SW = new StreamWriter(file, true, encoding: Encoding.GetEncoding("GB2312"));
                    SW.Write(SB);
                    SW.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Create Sample csv Fail : {ex.Message}");
                }
            }
        }

        #endregion

        #region AutoPattern

        private void Btn_AutoPattern_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_AutoPattern = AutoPattern_FlowPara.ReadXML(dialog.FileName);
                PropertyGrid_AutoPattern_Config.SelectedObject = FlowPara_AutoPattern;
            }
        }

        private void Btn_AutoPattern_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                AutoPattern_FlowPara.WriteXML(FlowPara_AutoPattern, file);
            }
        }

        private void Btn_AutoPattern_SampleCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.csv*)|*.csv*";
            dialog.FileName = "Pattern.csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                try
                {
                    StringBuilder SB = new StringBuilder();

                    List<string> TitleList = new List<string>();

                    TitleList.Add("R");
                    TitleList.Add("G");
                    TitleList.Add("B");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");

                    //Info
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{255}");
                    InfoList.Add($"{255}");
                    InfoList.Add($"{255}");

                    string Info = string.Join(",", InfoList);

                    SB.Append(Info + "\r\n");

                    StreamWriter SW = new StreamWriter(file, true, encoding: Encoding.GetEncoding("GB2312"));
                    SW.Write(SB);
                    SW.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Create Sample csv Fail : {ex.Message}");
                }
            }
        }


        private void Btn_AutoPattern_Create_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            AutoPattern_FlowPara Para = new AutoPattern_FlowPara();
            Para.Clone(this.FlowPara_AutoPattern);

            AutoPattern.Start(Para);
        }

        private void Btn_AutoPattern_Create_Stop_Click(object sender, EventArgs e)
        {
            if (AutoPattern != null)
            {
                AutoPattern.Stop();
            }
        }

        private void AutoPattern_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_AutoPattern_LogMsg);
        }

        private void AutoPattern_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_AutoPattern_StepMonitor);
        }

        #endregion

        #region AutoWD

        private void AutoWD_LogMsg(string Msg)
        {
            CrossThread.LabelEdit(Msg, Label_AutoWD_LogMsg);
        }

        private void AutoWD_MonitorStep(string Step)
        {
            CrossThread.LabelEdit(Step, Label_AutoWD_StepMonitor);
        }

        private void Btn_AutoWD_LoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "讀取檔案";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FlowPara_AutoWD = AutoWD_FlowPara.ReadXML(dialog.FileName);
                PropertyGrid_AutoWD_Config.SelectedObject = FlowPara_AutoWD;
            }
        }

        private void Btn_AutoWD_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.xml*)|*.xml*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                if (!file.Contains(".xml"))
                {
                    file += ".xml";
                }

                AutoWD_FlowPara.WriteXML(FlowPara_AutoWD, file);
            }
        }

        private void Btn_AutoWD_SampleCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "檔案儲存";
            dialog.Filter = "(*.csv*)|*.csv*";
            dialog.FileName = "WD_List.csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;

                try
                {
                    StringBuilder SB = new StringBuilder();

                    List<string> TitleList = new List<string>();

                    TitleList.Add("WD");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");

                    //Info
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{420}");

                    string Info = string.Join(",", InfoList);

                    SB.Append(Info + "\r\n");

                    StreamWriter SW = new StreamWriter(file, true, encoding: Encoding.GetEncoding("GB2312"));
                    SW.Write(SB);
                    SW.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Create Sample csv Fail : {ex.Message}");
                }
            }
        }

        private void Btn_AutoWD_Start_Click(object sender, EventArgs e)
        {
            this.liveViewFlag = false;

            AutoWD_FlowPara Para = new AutoWD_FlowPara();
            Para.Clone(this.FlowPara_AutoWD);

            AutoWD.Start(Para);
        }

        private void Btn_AutoWD_Stop_Click(object sender, EventArgs e)
        {
            if (AutoWD != null)
            {
                AutoWD.Stop();
            }
        }

        private void Btn_ReadCsv_Click(object sender, EventArgs e)
        {
            try
            {
                List<double> WD_List = new List<double>();

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "讀取檔案";
                dialog.Filter = "(*.csv*)|*.csv*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] CsvLines = File.ReadAllLines(dialog.FileName);

                    foreach (string CsvLine in CsvLines)
                    {
                        string Value = CsvLine.Split(',')[0];
                        double WD = 0.0;
                        bool ConvertSucced = double.TryParse(Value, out WD);

                        if (ConvertSucced)
                        {
                            WD_List.Add(WD);
                        }
                    }

                    if (WD_List.Count > 0)
                    {
                        FlowPara_AutoWD.WD_List = new double[] { };
                        FlowPara_AutoWD.WD_List = WD_List.ToArray();
                    }

                    PropertyGrid_AutoWD_Config.SelectedObject = FlowPara_AutoWD;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Measurement

        private void Btn_Measurement_SaveConfig_Click(object sender, EventArgs e)
        {

        }







        #endregion

        private void CalWD_Click(object sender, EventArgs e)
        {
            if (!liveViewFlag)
            {
                Grabber.Grab();
            }

            double RealX = Convert.ToDouble(Tbx_RealSizeX.Text);
            double Result = AutoWD.GetWD(RealX);
            Tbx_WD.Text = $"{Result}";
        }
    }

    public class HardwareUnit
    {
        public MilDigitizer Grabber { get; set; } = null;
        public ClsUrRobotInterface Robot { get; set; } = null;
        public X_PLC_Ctrl PLC { get; set; } = null;
        public D65_Light_Ctrl D65 { get; set; } = null;
        public SR3_Ctrl SR3 { get; set; } = null;
        public CornerstoneB_Ctrl CornerstoneB { get; set; } = null;
        public PowerSupply_Ctrl PowerSupply { get; set; } = null;
        public LightSourceA_Ctrl LightSourceA { get; set; } = null;
        public IMotorControl Motor { get; set; } = null;
    }


}