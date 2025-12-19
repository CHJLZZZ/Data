using DocumentFormat.OpenXml.Drawing;
using LightMeasure;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommonBase.Logger;
using HardwareManager;
using BaseTool;
using FrameGrabber;

namespace OpticalMeasuringSystem
{
    class TaskOneColorDataCreate
    {
        private InfoManager infoManager;
        private MilDigitizer grabber = null;
        private IMotorControl motorControl = null;

        public string ErrorCode = "";

        public EnumFinalStatus FinalStatus = EnumFinalStatus.Initial; //0:initial, 1:OK, 2:NG

        public EnState State = EnState.Idle;

        private cls29MSetting param = new cls29MSetting();

        private myTimer timer1 = new myTimer(); // 流程時間
        private myTimer timer2 = new myTimer(); // 存圖時間
        private myTimer timer3 = new myTimer(); // 自動曝光總時間

        private frmOneColorCorrection frmOneColorCorrection;

        private Thread PreLoadBaseCalibrationIdX;
        private Thread PreLoadBaseCalibrationIdY;
        private Thread PreLoadBaseCalibrationIdZ;

        private Task_AutoExpTime autoExposure;
        private TaskBaseCalibration baseCalibrationX;
        private TaskBaseCalibration baseCalibrationY;
        private TaskBaseCalibration baseCalibrationZ;

        private BaseCalibrationId CalibrationDataX;
        private BaseCalibrationId CalibrationDataY;
        private BaseCalibrationId CalibrationDataZ;

        private int ndUse = -1;
        private int xyzIpStep = -1;

        public int ExpTime = 0;
        public int maxExpTime = 0;
        public int curExpTime = 0;
        public int ExpTimeChangeRange = 0;
        public int oldGrayMean = 0;
        public double TargetGray = 0.0;
        public double ErrorTol = 0.0;
        public int TryTimes = 0;
        public double AutoExposureTime = 0.0;
        public int GrayMean = 0;

        public double Temperature_Start;
        public double Temperature_End;

        private bool isActive = false;   // Checked by Measure Mode 
        private bool[] xyzEnable = new bool[3];
        private MilColorImage colorImage = null;
        private MilMonoImage monoImage = null;
        private MilColorImage coeffImage = null;

        private bool debug = false;

        private bool AddCIELuminance;
        public double ResultAutoLuminance = 0;

        public enum EnState
        {
            Idle,
            Init,
            CheckRecipe,
            CheckNdPosition,
            PreLoadCalibrationFile,
            StartProcess,
            MoveAllFilter,
            WaitAllFilterMoveFinish,
            Capture,
            CaptureFinish,
            GoNextXyzFilter,
            CheckCalibrationIdIsReady,
            IpBaseCalibration,
            IpWaitBaseCalibrationX,
            IpWaitBaseCalibrationY,
            IpWaitBaseCalibrationZ,
            ShowOneColorDataCreateForm,
            WaitOneColorDataCreate,
            CreateOneColorData,
            Finish,
            Alarm,
            Warning,
        }

        public TaskOneColorDataCreate(cls29MSetting param, InfoManager infoManager, MilDigitizer grabber, IMotorControl motorControl)
        {
            this.param = param;
            this.infoManager = infoManager;
            this.grabber = grabber;
            this.motorControl = motorControl;
        }

        #region --- 方法函式 ---

        #region --- SetCorrGain ---
        public void SetCorrGain(int filterIndex)
        {
            switch (filterIndex)
            {
                case 0:
                    this.grabber.SetCameraGain(param.CameraGainX);
                    break;
                case 1:
                    this.grabber.SetCameraGain(param.CameraGainY);
                    break;
                case 2:
                    this.grabber.SetCameraGain(param.CameraGainZ);
                    break;
            }
        }
        #endregion

        #region --- ShowOneColorCalibrationDataCreateForm ---
        //[System.STAThread]
        private void ShowOneColorCalibrationDataCreateForm()
        {
            try
            {
                if (this.frmOneColorCorrection == null)
                {
                    this.frmOneColorCorrection = new frmOneColorCorrection(param.AutoExposureRoiGrayBypassPercent);
                    this.frmOneColorCorrection.ShowDialog();
                    AddCIELuminance = frmOneColorCorrection.DialogResult == System.Windows.Forms.DialogResult.Yes;
                }
            }
            catch 
            {

            }
            finally
            {
                this.frmOneColorCorrection = null;
            }
        }
        #endregion

        #region --- LoadBaseCalibrationIdX ---
        private void LoadBaseCalibrationIdX()
        {
            this.infoManager.Debug("[X] Start preLoad BaseCalibrationId");

            string dark = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Dark\X.tif";
            string flat = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Flat\X.tif";
            string offset = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Offset\X.tif";
            string calibration = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{this.ndUse}\X\GridCalibrationRecipe_C1_1.mca";

            this.CalibrationDataX = new BaseCalibrationId();
            this.CalibrationDataX.Load(dark, flat, offset, calibration);

            this.infoManager.Debug("[X] PreLoad BaseCalibrationId completed");
        }
        #endregion

        #region --- LoadBaseCalibrationIdY ---
        private void LoadBaseCalibrationIdY()
        {
            this.infoManager.Debug("[Y] Start preLoad BaseCalibrationId");

            string dark = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Dark\Y.tif";
            string flat = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Flat\Y.tif";
            string offset = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Offset\Y.tif";
            string calibration = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{this.ndUse}\Y\GridCalibrationRecipe_C1_1.mca";

            this.CalibrationDataY = new BaseCalibrationId();
            this.CalibrationDataY.Load(dark, flat, offset, calibration);

            this.infoManager.Debug("[Y] PreLoad BaseCalibrationId completed");
        }
        #endregion

        #region --- LoadBaseCalibrationIdZ ---
        private void LoadBaseCalibrationIdZ()
        {
            this.infoManager.Debug("[Z] Start preLoad BaseCalibrationId");

            string dark = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Dark\Z.tif";
            string flat = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Flat\Z.tif";
            string offset = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{this.ndUse}_Offset\Z.tif";
            string calibration = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{this.ndUse}\Z\GridCalibrationRecipe_C1_1.mca";

            this.CalibrationDataZ = new BaseCalibrationId();
            this.CalibrationDataZ.Load(dark, flat, offset, calibration);

            this.infoManager.Debug("[Z] PreLoad BaseCalibrationId completed");
        }
        #endregion

        #region --- CheckState ---
        private void CheckState(bool isRun)
        {
            if (!isRun)
            {
                this.State = EnState.Idle;
                this.FinalStatus = EnumFinalStatus.OK;
                this.infoManager.General("TaskOneColorDataCreate failed. User stop the process.");
            }
        }
        #endregion

        #region --- CheckActiveByMeasureMode ---
        private bool CheckActiveByMeasureMode(int xyzIpStep)
        {
            switch (GlobalVar.SD.MeasureMode)
            {
                case EnumMeasureMode.Luminance:
                    {
                        if (xyzIpStep == 1)   // Step : Y
                            return true;
                        else
                            return false;
                    }
                case EnumMeasureMode.Luminance_Chroma:
                    {
                        if (this.param.OneColorCalibration_XYZ_Enable[xyzIpStep])
                            return true;
                        else
                            return false;
                    }
            }

            return false;
        }
        #endregion

        #region --- CheckAllWheelReady ---
        private bool CheckAllWheelReady()
        {
            bool XyzReady = false;
            bool NdReady = false;

            // Change Wheel Ready
            if (!motorControl.IsMoveProc())
            {

                switch (GlobalVar.DeviceType)
                {
                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                        {
                            // CheckXyz Filter Status
                            if (GlobalVar.Device.XyzFilter != null)
                            {
                                if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                {
                                    if (GlobalVar.Device.XyzFilter.IsReady)
                                        XyzReady = true;
                                    else
                                        XyzReady = false;
                                }
                                else
                                    XyzReady = true;
                            }
                            else
                                XyzReady = true;

                            // Check Nd Filter Status
                            if (GlobalVar.Device.NdFilter != null)
                            {
                                if (GlobalVar.Device.NdFilter.is_FW_Work)
                                {
                                    if (GlobalVar.Device.NdFilter.IsReady)
                                        NdReady = true;
                                    else
                                        NdReady = false;
                                }
                                else
                                    NdReady = true;
                            }
                            else
                                NdReady = true;
                        }
                        break;

                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            if (this.motorControl.IsWork())
                            {
                                if (this.motorControl.MoveStatus_FW1() == UnitStatus.Finish)
                                    XyzReady = true;
                                else
                                    XyzReady = false;

                                if (this.motorControl.MoveStatus_FW2() == UnitStatus.Finish)
                                    NdReady = true;
                                else
                                    NdReady = false;
                            }
                        }
                        break;

                    case EnumMotoControlPackage.MS5515M:
                        {
                            return true;
                        }
                }
            }

            return (XyzReady && NdReady);
        }
        #endregion

        #region --- Run ---
        //[STAThread]
        public void Run(bool isRun, bool isInline)
        {
            this.CheckState(isRun);

            switch (State)
            {
                case EnState.Idle:
                    {
                        this.ErrorCode = "";
                    }
                    break;

                case EnState.Init:
                    {
                        this.FinalStatus = 0;

                        this.timer1.Start();

                        MyMilIp.predictData = new MeasureData();
                        this.xyzEnable = this.param.OneColorCalibration_XYZ_Enable;

                        string date = DateTime.Now.ToString("yyyy_MMdd");
                        string time = DateTime.Now.ToString("HH_mm_ss");
                        GlobalVar.Config.RecipePath = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}";
                        GlobalVar.Config.DirectoryPath = $@"D:\OpticalMeasurementData\LUM\Backup\{date}\{GlobalVar.ProcessInfo.PanelID}\{time}";

                        Directory.CreateDirectory(GlobalVar.Config.DirectoryPath);
                        Directory.CreateDirectory($@"{GlobalVar.Config.DirectoryPath}\R");
                        Directory.CreateDirectory($@"{GlobalVar.Config.DirectoryPath}\G");
                        Directory.CreateDirectory($@"{GlobalVar.Config.DirectoryPath}\B");
                        Directory.CreateDirectory($@"{GlobalVar.Config.DirectoryPath}\W");


                        GlobalVar.LightMeasure.TristimulusImage = new MilColorImage();

                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                            GlobalVar.LightMeasure.ChromaImage = new MilChromaImage();

                        // Free
                        if (this.colorImage != null)
                        {
                            this.colorImage.Free();
                            this.colorImage = null;
                        }
                        if (this.monoImage != null)
                        {
                            this.monoImage.Free();
                            this.monoImage = null;
                        }
                        if (this.coeffImage != null)
                        {
                            this.coeffImage.Free();
                            this.coeffImage = null;
                        }

                        //Recorder Temperature
                        this.Temperature_Start = this.grabber.GetTemperature();

                        this.timer1.Stop();
                        this.infoManager.Debug($"Init {this.timer1.timeSpend} ms");

                        this.State = EnState.CheckRecipe;
                    }
                    break;

                case EnState.CheckRecipe:
                    {
                        this.timer1.Start();

                        if (GlobalVar.Config.RecipeName != "")
                        {
                            this.State = EnState.CheckNdPosition;
                        }
                        else
                        {
                            this.ErrorCode = "No Recipe has been Chosen";
                            this.State = EnState.Alarm;
                        }

                        this.timer1.Stop();
                        this.infoManager.Debug($"CheckRecipe {this.timer1.timeSpend} ms");
                    }
                    break;

                case EnState.CheckNdPosition:
                    {
                        this.ndUse = GlobalVar.Device.NdPosition;
                        this.State = EnState.PreLoadCalibrationFile;
                    }
                    break;

                case EnState.PreLoadCalibrationFile:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                            {
                                this.PreLoadBaseCalibrationIdX = new Thread(this.LoadBaseCalibrationIdX);
                                this.PreLoadBaseCalibrationIdY = new Thread(this.LoadBaseCalibrationIdY);
                                this.PreLoadBaseCalibrationIdZ = new Thread(this.LoadBaseCalibrationIdZ);

                                this.PreLoadBaseCalibrationIdX.Start();
                                this.PreLoadBaseCalibrationIdY.Start();
                                this.PreLoadBaseCalibrationIdZ.Start();
                            }
                            else
                            {
                                this.PreLoadBaseCalibrationIdY = new Thread(this.LoadBaseCalibrationIdY);
                                this.PreLoadBaseCalibrationIdY.Start();
                            }
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            this.PreLoadBaseCalibrationIdY = new Thread(this.LoadBaseCalibrationIdY);
                            this.PreLoadBaseCalibrationIdY.Start();
                        }

                        this.State = EnState.StartProcess;
                    }
                    break;

                case EnState.StartProcess:
                    {
                        this.timer1.Start();

                        GlobalVar.Device.XyzPosition = 0;          // XYZ-Filter: 0->X; 1->Y; 2->Z; 3->W (AIC_4_Positions or AIL_4_Positions) ; 0->Y; 1->W
                        this.ndUse = GlobalVar.Device.NdPosition;  // ND3-Filter: 0->10%; 1->3%; 2->1%; 3->100%

                        if (GlobalVar.SD.XYZFilter_PositionMode != BaseTool.EnumXYZFilter_PositionMode.AIL_2_Positions)
                            this.xyzIpStep = 0;
                        else
                            this.xyzIpStep = 1;

                        this.State = EnState.MoveAllFilter;

                        this.timer1.Stop();
                        this.infoManager.Debug($"StartProcess {this.timer1.timeSpend} ms");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Process Start");
                    }
                    break;

                case EnState.MoveAllFilter:
                    {
                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[this.xyzIpStep];
                        GlobalVar.Device.NdPosition = param.FilterWheel_ND_OneColor[this.xyzIpStep];
                        this.ndUse = GlobalVar.Device.NdPosition;  // ND3-Filter: 0->10%; 1->3%; 2->1%; 3->100%

                        if (this.isActive && GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST")
                        {
                            this.timer1.Start();

                            // Change Wheel
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter != null)
                                        {
                                            if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                                GlobalVar.Device.XyzFilter.ChangeWheel(GlobalVar.Device.XyzPosition);
                                        }

                                        if (GlobalVar.Device.NdFilter != null)
                                        {
                                            if (GlobalVar.Device.NdFilter.is_FW_Work)
                                                GlobalVar.Device.NdFilter.ChangeWheel(GlobalVar.Device.NdPosition);
                                        }

                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (this.motorControl.IsWork())
                                            this.motorControl.Move(-1, -1, GlobalVar.Device.XyzPosition, GlobalVar.Device.NdPosition);
                                    }
                                    break;
                            }

                            this.SetCorrGain(GlobalVar.Device.XyzPosition);

                            this.timer1.Stop();
                            this.infoManager.Debug($"MoveAllFilter, XYZ Filter To ->{GlobalVar.Device.XyzPosition}, ND Filter To ->{this.ndUse} {timer1.timeSpend} ms");
                        }

                        this.State = EnState.WaitAllFilterMoveFinish;

                        this.timer1.Start(); // 紀錄轉動時間
                    }
                    break;

                case EnState.WaitAllFilterMoveFinish:
                    {
                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        if (this.isActive && GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST")
                        {
                            if (this.CheckAllWheelReady())
                            {
                                this.timer1.Stop();
                                this.infoManager.Debug($"WaitAllFilterMoveFinish {this.timer1.timeSpend} ms");
                                this.State = EnState.Capture;
                            }
                        }
                        else
                            this.State = EnState.CheckCalibrationIdIsReady;
                    }
                    break;

                case EnState.Capture:
                    {
                        if (this.isActive)
                        {
                            if (this.autoExposure != null)
                                this.autoExposure = null;

                            this.autoExposure = new Task_AutoExpTime(this.param, this.xyzIpStep, this.infoManager, this.grabber);
                            this.autoExposure.State = Task_AutoExpTime.EnState.Init;
                            this.autoExposure.Start();
                        }

                        this.State = EnState.CaptureFinish;
                    }
                    break;

                case EnState.CaptureFinish:
                    {
                        if (this.isActive)
                        {
                            if (autoExposure.IsDone)
                            {
                                if (this.autoExposure.State == Task_AutoExpTime.EnState.Finish)
                                {
                                    if (this.autoExposure.ErrorCode == "")
                                    {
                                        this.State = EnState.GoNextXyzFilter;
                                    }
                                    else
                                    {   //程序發生錯誤
                                        this.ErrorCode = this.autoExposure.ErrorCode;
                                        this.State = EnState.Alarm;
                                    }
                                }
                                else if (this.autoExposure.State == Task_AutoExpTime.EnState.Warning)
                                {   //曝光失敗，太亮
                                    this.ErrorCode = this.autoExposure.ErrorCode;
                                    this.State = EnState.Alarm;
                                    this.autoExposure.State = Task_AutoExpTime.EnState.Idle;
                                }
                            }
                        }
                        else
                            this.State = EnState.GoNextXyzFilter;
                    }
                    break;

                case EnState.GoNextXyzFilter:
                    {
                        if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIC_4_Positions || GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_4_Positions)
                        {
                            if (GlobalVar.Device.XyzPosition == 2)
                            {
                                this.State = EnState.IpBaseCalibration;
                                break;
                            }
                        }
                        else if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
                        {
                            if (GlobalVar.Device.XyzPosition == 0)
                            {
                                this.State = EnState.CheckCalibrationIdIsReady;
                                break;
                            }
                        }

                        this.timer1.Start();

                        //switch (GlobalVar.Device.XyzPosition)
                        //{
                        //    case 0:
                        //    case 1:
                        //        {
                        //            GlobalVar.Device.XyzPosition++;
                        //        }
                        //        break;
                        //    case 3:
                        //        {
                        //            GlobalVar.Device.XyzPosition = 0;
                        //        }
                        //        break;
                        //}

                        int Idx = this.xyzIpStep + 1;

                        if (Idx == 3) Idx = 0;
                        GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[Idx];
                        GlobalVar.Device.NdPosition = param.FilterWheel_ND_OneColor[Idx];

                        if (this.isActive)
                        {
                            // Change Wheel
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:

                                    {
                                        if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                            GlobalVar.Device.XyzFilter.ChangeWheel(GlobalVar.Device.XyzPosition);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (this.motorControl.IsWork())
                                            this.motorControl.Move(-1, -1, GlobalVar.Device.XyzPosition, -1);
                                    }
                                    break;
                            }

                            this.SetCorrGain(GlobalVar.Device.XyzPosition);
                        }

                        this.timer1.Stop();

                        this.infoManager.Debug($"GoNextXyzFilter {this.timer1.timeSpend} ms");

                        this.State = EnState.CheckCalibrationIdIsReady;
                    }
                    break;

                case EnState.CheckCalibrationIdIsReady:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                            {
                                if ((this.xyzIpStep == 0 && this.CalibrationDataX.IsReady()) ||
                                    (this.xyzIpStep == 1 && this.CalibrationDataY.IsReady()) ||
                                    (this.xyzIpStep == 2 && this.CalibrationDataZ.IsReady()))
                                {
                                    this.State = EnState.IpBaseCalibration;
                                }
                            }
                            else
                            {
                                if (this.xyzIpStep == 1 && this.CalibrationDataY.IsReady())
                                    this.State = EnState.IpBaseCalibration;
                            }
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            if (this.CalibrationDataY.IsReady())
                                this.State = EnState.IpBaseCalibration;
                        }
                    }
                    break;

                case EnState.IpBaseCalibration:
                    {
                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIC_4_Positions || GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_4_Positions)
                        {
                            if (this.isActive)
                            {
                                switch (this.xyzIpStep)
                                {
                                    case 0:
                                        {
                                            this.baseCalibrationX = new TaskBaseCalibration(param,
                                                                                       this.CalibrationDataX,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationX.State = TaskBaseCalibration.EnState.Init;
                                            this.baseCalibrationX.Run();
                                        }
                                        break;

                                    case 1:
                                        {
                                            this.baseCalibrationY = new TaskBaseCalibration(param,
                                                                                       this.CalibrationDataY,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationY.State = TaskBaseCalibration.EnState.Init;
                                            this.baseCalibrationY.Run();
                                        }
                                        break;

                                    case 2:
                                        {
                                            this.baseCalibrationZ = new TaskBaseCalibration(param,
                                                                                       this.CalibrationDataZ,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationZ.State = TaskBaseCalibration.EnState.Init;
                                            this.baseCalibrationZ.Run();
                                        }
                                        break;
                                }
                            }

                            if (this.xyzIpStep != 2)
                            {
                                this.xyzIpStep++;
                                State = EnState.WaitAllFilterMoveFinish;
                            }
                            else
                            {
                                // Stop Auto Exposuring
                                if (this.autoExposure != null)
                                    this.autoExposure.ShotDown();

                                State = EnState.IpWaitBaseCalibrationX;
                            }
                        }
                        else if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
                        {
                            this.baseCalibrationY = new TaskBaseCalibration(param,
                                           this.CalibrationDataY,
                                           ref this.grabber.grabImage,
                                           this.xyzIpStep,
                                           this.infoManager);

                            this.baseCalibrationY.State = TaskBaseCalibration.EnState.Init;
                            this.baseCalibrationY.Run();

                            // Stop Auto Exposuring
                            if (this.autoExposure != null)
                                this.autoExposure.ShotDown();

                            State = EnState.IpWaitBaseCalibrationX;

                        }

                    }
                    break;

                case EnState.IpWaitBaseCalibrationX:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.baseCalibrationX.IsDone)
                            {
                                State = EnState.IpWaitBaseCalibrationY;
                            }
                        }
                        else
                            State = EnState.IpWaitBaseCalibrationY;

                    }
                    break;

                case EnState.IpWaitBaseCalibrationY:
                    {
                        if (this.baseCalibrationY.IsDone)
                        {
                            State = EnState.IpWaitBaseCalibrationZ;
                        }
                    }
                    break;

                case EnState.IpWaitBaseCalibrationZ:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.baseCalibrationZ.IsDone)
                            {
                                //this.baseCalibrationZ.ShotDown();
                                State = EnState.ShowOneColorDataCreateForm;
                            }
                        }
                        else
                            State = EnState.ShowOneColorDataCreateForm;
                    }
                    break;

                case EnState.ShowOneColorDataCreateForm:
                    {
                        if (!isInline)
                        {
                            this.ShowOneColorCalibrationDataCreateForm();
                            State = EnState.WaitOneColorDataCreate;
                        }
                        else //gamma:autolum.
                        {
                            //load recipe
                            string recipefile = $"{GlobalVar.Config.ConfigPath}\\{GlobalVar.Config.RecipeName}\\OneColorCalibration\\correctionConfig.xml";
                            if (!File.Exists(recipefile))
                            {
                                ErrorCode = recipefile + " not found.";
                                State = EnState.Alarm;
                                break;
                            }
                            LightMeasureConfig correctionConfig = new LightMeasureConfig();
                            correctionConfig.ReadWithoutCrypto(recipefile);
                            recipefile = $"{GlobalVar.Config.ConfigPath}\\{GlobalVar.Config.RecipeName}\\AutoLuminance\\{GlobalVar.ProcessInfo.CalibrationFile}.xml";
                            if (!File.Exists(recipefile))
                            {
                                ErrorCode = recipefile + " not found.";
                                State = EnState.Alarm;
                                break;
                            }
                            GlobalVar.AutoLuminance = ClsAutoLuminance.ReadData(recipefile);
                            if (GlobalVar.AutoLuminance.LuminancePair.Count < 2)
                            {
                                ErrorCode = "Reference luminance count < 2";
                                State = EnState.Alarm;
                                break;
                            }

                            //roi factor
                            double targetfactor = 0; //mean/exp.
                            FourColorData measureData = new FourColorData();
                            measureData.CircleRegionInfo.CenterX = correctionConfig.CorrectionData.DataList[0].CircleRegionInfo.CenterX;
                            measureData.CircleRegionInfo.CenterY = correctionConfig.CorrectionData.DataList[0].CircleRegionInfo.CenterY;
                            measureData.CircleRegionInfo.Radius = correctionConfig.CorrectionData.DataList[0].CircleRegionInfo.Radius;
                            LightMeasurer measurer = new LightMeasurer(MyMil.MilSystem);
                            //MyMilIp.correctionConfig.CorrectionData.RegionMethod = EnumRegionMethod.Circle;
                            measurer.CalculateRegionMean(MIL.M_NULL, baseCalibrationY.FlowImage, MIL.M_NULL, EnumRegionMethod.Circle, ref measureData, param.AutoExposureRoiGrayBypassPercent);
                            targetfactor = measureData.GrayMean.Y / GlobalVar.ProcessInfo.XyzExposureTime[1];
                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] AutoLuminance-Factor: {targetfactor} ( {measureData.GrayMean.Y} / {GlobalVar.ProcessInfo.XyzExposureTime[1]} )");

                            //Linear_Interpolation or Regression                            
                            if (GlobalVar.AutoLuminance.LuminancePair.Count == 2)
                            {
                                MyMilIp.Linear_Interpolation(GlobalVar.AutoLuminance.LuminancePair[0].Factor, GlobalVar.AutoLuminance.LuminancePair[0].Luminance,
                                                             GlobalVar.AutoLuminance.LuminancePair[1].Factor, GlobalVar.AutoLuminance.LuminancePair[1].Luminance, targetfactor, ref ResultAutoLuminance);
                            }
                            else
                            {
                                if (targetfactor < GlobalVar.AutoLuminance.LuminancePair[0].Factor)
                                {
                                    PointD[] points = new PointD[3];
                                    for (int i = 0; i < points.Length; i++)
                                    {
                                        points[i].X = GlobalVar.AutoLuminance.LuminancePair[i].Factor;
                                        points[i].Y = GlobalVar.AutoLuminance.LuminancePair[i].Luminance;
                                    }
                                    MyMilIp.Linear_Regression(points, targetfactor, ref ResultAutoLuminance);
                                }
                                else if (targetfactor > GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1].Factor)
                                {
                                    PointD[] points = new PointD[3];
                                    for (int i = 0; i < points.Length; i++)
                                    {
                                        points[i].X = GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1 - i].Factor;
                                        points[i].Y = GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1 - i].Luminance;
                                    }
                                    MyMilIp.Linear_Regression(points, targetfactor, ref ResultAutoLuminance);
                                }
                                else
                                {
                                    for (int i = 0; i < GlobalVar.AutoLuminance.LuminancePair.Count - 1; i++)
                                    {
                                        if (GlobalVar.AutoLuminance.LuminancePair[i].Factor <= targetfactor && targetfactor <= GlobalVar.AutoLuminance.LuminancePair[i + 1].Factor)
                                        {
                                            MyMilIp.Linear_Interpolation(GlobalVar.AutoLuminance.LuminancePair[i].Factor, GlobalVar.AutoLuminance.LuminancePair[i].Luminance,
                                                                         GlobalVar.AutoLuminance.LuminancePair[i + 1].Factor, GlobalVar.AutoLuminance.LuminancePair[i + 1].Luminance, targetfactor, ref ResultAutoLuminance);
                                            break;
                                        }
                                    }
                                }
                            }
                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] AutoLuminance-Luminance: {ResultAutoLuminance}");

                            State = EnState.Finish;
                        }
                    }
                    break;

                case EnState.WaitOneColorDataCreate:
                    {
                        if (MyMilIp.IsUserCoeffFinish == MyMilIp.EnCalibrationState.Successful)
                        {
                            MyMilIp.IsUserCoeffFinish = MyMilIp.EnCalibrationState.None;
                            State = EnState.CreateOneColorData;
                        }
                        else if (MyMilIp.IsUserCoeffFinish == MyMilIp.EnCalibrationState.Failed | MyMilIp.IsUserCoeffFinish == MyMilIp.EnCalibrationState.None)
                        {
                            MyMilIp.IsUserCoeffFinish = MyMilIp.EnCalibrationState.None;
                            this.ErrorCode = "One color calibration data create failed.";
                            State = EnState.Alarm;
                        }
                    }
                    break;

                case EnState.CreateOneColorData:
                    {
                        LightMeasurer measurer = null;

                        measurer = new LightMeasurer(MyMil.MilSystem);

                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                            {
                                //MIL.MbufSave( @"D:\\baseCalibrationY_FlowImage.tif", this.baseCalibrationY.FlowImage);

                                this.colorImage = new MilColorImage(this.baseCalibrationX.FlowImage,
                                                            this.baseCalibrationY.FlowImage,
                                                            this.baseCalibrationZ.FlowImage);

                                measurer.CreateCorrectionData(colorImage, this.xyzEnable, ref MyMilIp.correctionConfig.CorrectionData, param.AutoExposureRoiGrayBypassPercent);
                                // Get CoeffX、CoeffY、CoeffZ
                                measurer.CreateCoeffImage(MyMilIp.correctionConfig.CorrectionData, ref coeffImage);

                                string calibrationFileDir = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration\{GlobalVar.ProcessInfo.CalibrationFile}";
                                if (!Directory.Exists(calibrationFileDir))
                                    Directory.CreateDirectory(calibrationFileDir);

                                MIL.MbufSave(string.Format("{0}\\CoeffX.tif", calibrationFileDir), coeffImage.ImgX);
                                MIL.MbufSave(string.Format("{0}\\CoeffY.tif", calibrationFileDir), coeffImage.ImgY);
                                MIL.MbufSave(string.Format("{0}\\CoeffZ.tif", calibrationFileDir), coeffImage.ImgZ);
                                //MyMilIp.correctionConfig.WriteWithoutCrypto($@"{calibrationFile}\correctionConfig.xml", false);

                                //if (colorImage != null)
                                //{
                                //    colorImage.Free();
                                //}
                            }
                            else
                            {
                                this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);

                                measurer.CreateCorrectionDataY(monoImage, ref MyMilIp.correctionConfig.CorrectionData, param.AutoExposureRoiGrayBypassPercent);

                                // Get CoeffY
                                measurer.CreateCoeffImageY(MyMilIp.correctionConfig.CorrectionData, ref monoImage);

                                string calibrationFileDir = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration\{GlobalVar.ProcessInfo.CalibrationFile}";
                                if (!Directory.Exists(calibrationFileDir))
                                    Directory.CreateDirectory(calibrationFileDir);

                                MIL.MbufSave(string.Format("{0}\\CoeffY.tif", calibrationFileDir), monoImage.Img);
                                //MyMilIp.correctionConfig.WriteWithoutCrypto($@"{calibrationFile}\correctionConfig.xml", false);

                                //if (monoImage != null)
                                //{
                                //    monoImage.Free();
                                //}
                            }

                            if (AddCIELuminance)
                            {
                                //readdata
                                string AutoLuminanceDir = GlobalVar.Config.RecipePath + "\\AutoLuminance";
                                string CalibrationFilename = AutoLuminanceDir + "\\" + GlobalVar.ProcessInfo.CalibrationFile + ".xml";
                                if (!Directory.Exists(AutoLuminanceDir)) { Directory.CreateDirectory(AutoLuminanceDir); }
                                if (File.Exists(CalibrationFilename))
                                {
                                    GlobalVar.AutoLuminance = ClsAutoLuminance.ReadData(CalibrationFilename);
                                }
                                else
                                {
                                    GlobalVar.AutoLuminance.LuminancePair.Clear();
                                }
                                //set
                                sLuminancePair luminancePair;
                                luminancePair.Factor = MyMilIp.correctionConfig.CorrectionData.DataList[0].GrayMean.Y / MyMilIp.correctionConfig.CorrectionData.ExposureTime.Y;
                                luminancePair.Luminance = MyMilIp.correctionConfig.CorrectionData.DataList[0].CieChroma.Luminance;
                                //write
                                GlobalVar.AutoLuminance.LuminancePair.Add(luminancePair);
                                GlobalVar.AutoLuminance.LuminancePair = GlobalVar.AutoLuminance.LuminancePair.OrderBy(t => t.Factor).ThenBy(t => t.Luminance).ToList();//.Sort((x, y) => x.Factor.CompareTo(y.Factor));
                                ClsAutoLuminance.WriteData(GlobalVar.AutoLuminance, CalibrationFilename);
                            }

                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            MyMilIp.correctionConfig.CorrectionData.ExposureTime.Y = GlobalVar.ProcessInfo.XyzExposureTime[1];

                            this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);

                            measurer.CreateCorrectionDataY(monoImage, ref MyMilIp.correctionConfig.CorrectionData);
                            // Get CoeffY
                            measurer.CreateCoeffImageY(MyMilIp.correctionConfig.CorrectionData, ref monoImage);

                            string calibrationFileDir = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration\{GlobalVar.ProcessInfo.CalibrationFile}";
                            if (!Directory.Exists(calibrationFileDir))
                                Directory.CreateDirectory(calibrationFileDir);

                            MIL.MbufSave(string.Format("{0}\\CoeffY.tif", calibrationFileDir), monoImage.Img);
                            //MyMilIp.correctionConfig.WriteWithoutCrypto($@"{calibrationFile}\correctionConfig.xml", false);

                            //if (monoImage != null)
                            //{
                            //    monoImage.Free();
                            //}
                        }

                        State = EnState.Finish;
                    }
                    break;

                case EnState.Finish:
                    {
                        timer1.Start();

                        this.infoManager.General($"[One Color Data Create] PanelID :[{GlobalVar.ProcessInfo.PanelID}]");
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                            {
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[0]} us");
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[2]} us");
                            }
                            else
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");

                        }
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");
                        }

                        //Recorder Temperature
                        this.Temperature_End = this.grabber.GetTemperature();
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Temperature Start = {Temperature_Start}");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Temperature End = {Temperature_End}");

                        timer1.Stop();
                        this.infoManager.Debug($"Finish {timer1.timeSpend} ms");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Process Completed");

                        if (this.debug && baseCalibrationY != null && baseCalibrationY.FlowImage != MIL.M_NULL) { MIL.MbufSave(@"D:\\baseCalibrationY_FlowImage2.tif", this.baseCalibrationY.FlowImage); }

                        // 設定顯示 base Calibration Y
                        if (baseCalibrationY != null && baseCalibrationY.FlowImage != MIL.M_NULL) { MyMilDisplay.SetDisplayImage(ref this.baseCalibrationY.FlowImage, 1); }


                        // Free
                        if (this.colorImage != null)
                        {
                            this.colorImage.Free();
                            this.colorImage = null;
                        }
                        if (this.monoImage != null)
                        {
                            this.monoImage.Free();
                            this.monoImage = null;
                        }
                        if (this.coeffImage != null)
                        {
                            this.coeffImage.Free();
                            this.coeffImage = null;
                        }

                        State = EnState.Idle;
                        this.FinalStatus = EnumFinalStatus.OK;
                    }
                    break;

                case EnState.Alarm:
                    {
                        this.infoManager.Error(this.ErrorCode);
                        State = EnState.Idle;
                        this.FinalStatus = EnumFinalStatus.NG;
                    }
                    break;
            }
        }
        #endregion

        #endregion

    }
}
