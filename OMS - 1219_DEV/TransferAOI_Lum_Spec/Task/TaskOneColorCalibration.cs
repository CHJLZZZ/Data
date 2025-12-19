//using DocumentFormat.OpenXml.Drawing;
using System.Drawing;
using DocumentFormat.OpenXml.EMMA;
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
using System.Reflection;
using System.ComponentModel;

public enum EnumFinalStatus
{
    Initial = 0,
    OK,
    NG
}

public enum EnumLightConvertType
{
    CorrFile,
    Spectral,
}


namespace OpticalMeasuringSystem
{
    class TaskOneColorCalibration
    {
        private InfoManager infoManager = null;
        private MilDigitizer grabber = null;
        private IMotorControl motorControl = null;

        public string ErrorCode = "";

        public EnumFinalStatus FinalStatus = EnumFinalStatus.Initial; //0:initial, 1:OK, 2:NG

        public EnState State = EnState.Idle;
        public EnumLightConvertType LightConvertType = EnumLightConvertType.CorrFile;

        public TaskOneColorCalibration(cls29MSetting param, EnumLightConvertType LightConvertType, InfoManager infoManager, MilDigitizer grabber, IMotorControl motorControl)
        {
            this.param = param;
            this.infoManager = infoManager;
            this.grabber = grabber;
            this.motorControl = motorControl;
            this.LightConvertType = LightConvertType;
        }

        public enum EnState
        {
            Idle,
            Init,
            CheckRecipe,
            PreLoadCalibrationFile,
            StartProcess,

            CheckLightConvertType,

            NdPredict_MoveND,
            NdPredict_CheckMoveND,
            NdPredict_Capture,
            NdPredict_CalGray,

            GetSpectralPara,

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
            LightSourceCorrection,
            PixelAlign,
            PixelStatistic,
            Finish,
            Alarm,
            Warning,
        }

        private cls29MSetting param = new cls29MSetting();

        private myTimer timer1 = new myTimer(); // 流程時間
        private myTimer timer2 = new myTimer(); // 存圖時間
        private myTimer timer3 = new myTimer(); // 自動曝光總時間
        private myTimer timer4 = new myTimer(); // pre-load時間

        private Thread PreLoadCalibrationId;

        private Task_AutoExpTime autoExposure;          // 狀態機-自動曝光
        private TaskBaseCalibration baseCalibrationX;   // 狀態機-X基本校正
        private TaskBaseCalibration baseCalibrationY;   // 狀態機-Y基本校正
        private TaskBaseCalibration baseCalibrationZ;   // 狀態機-Z基本校正

        private BaseCalibrationId CalibrationDataX;     // X校正檔大禮包
        private BaseCalibrationId CalibrationDataY;     // Y校正檔大禮包
        private BaseCalibrationId CalibrationDataZ;     // Z校正檔大禮包

        private MilColorImage colorImage = null;
        private MilMonoImage monoImage = null;
        private MilColorImage coeffImage = null;

        public double Temperature_Start;
        public double Temperature_End;

        private bool isLightCoeffReady = false;
        private bool isActive = false;   // Checked by Measure Mode 

        private int xyzIpStep = -1;
        private bool[] xyzEnable = new bool[3];

        private string SpectralType = "";
        private CorrectionPara SpectralPara = new CorrectionPara();

        private FW_ND_Remark[] NdPredictList = { FW_ND_Remark.ND_100, FW_ND_Remark.ND_12_5, FW_ND_Remark.ND_1_56, FW_ND_Remark.ND_0_25 };

        private int NdPredictIdx = 0;
        private string NdPredictName = "";

        private double[] ExpTimeResult = new double[3];
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

        #region --- LoadBaseCalibrationIdX ---
        private void PreloadCorrectionData()
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                timer4.Start();
                this.infoManager.Debug("[X] Start preLoad BaseCalibrationId");
                string darkX = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Dark\X.tif";
                string flatX = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Flat\X.tif";
                string offsetX = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Offset\X.tif";
                string calibrationX = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{GlobalVar.Device.NdPosition}\X\GridCalibrationRecipe_C1_1.mca";
                this.CalibrationDataX = new BaseCalibrationId();
                this.CalibrationDataX.Load(darkX, flatX, offsetX, calibrationX);
                timer4.Stop();
                this.infoManager.Debug($"[X] PreLoad BaseCalibrationId completed, {timer4.timeSpend} ms");
            }

            timer4.Start();
            this.infoManager.Debug("[Y] Start preLoad BaseCalibrationId");
            string darkY = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Dark\Y.tif";
            string flatY = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Flat\Y.tif";
            string offsetY = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Offset\Y.tif";
            string calibrationY = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{GlobalVar.Device.NdPosition}\Y\GridCalibrationRecipe_C1_1.mca";
            this.CalibrationDataY = new BaseCalibrationId();
            this.CalibrationDataY.Load(darkY, flatY, offsetY, calibrationY);
            timer4.Stop();
            this.infoManager.Debug("[Y] PreLoad BaseCalibrationId completed, {timer4.timeSpend} ms");

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                timer4.Start();
                this.infoManager.Debug("[Z] Start preLoad BaseCalibrationId");
                string darkZ = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Dark\Z.tif";
                string flatZ = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Flat\Z.tif";
                string offsetZ = $@"{GlobalVar.Config.RecipePath}\FFC\{GlobalVar.ProcessInfo.FFCFile}\ND{GlobalVar.Device.NdPosition}_Offset\Z.tif";
                string calibrationZ = $@"{GlobalVar.Config.RecipePath}\Calibration\ND{GlobalVar.Device.NdPosition}\Z\GridCalibrationRecipe_C1_1.mca";
                this.CalibrationDataZ = new BaseCalibrationId();
                this.CalibrationDataZ.Load(darkZ, flatZ, offsetZ, calibrationZ);
                timer4.Stop();
                this.infoManager.Debug("[Z] PreLoad BaseCalibrationId completed");
            }

            if (LightConvertType == EnumLightConvertType.CorrFile)
            {
                timer4.Start();
                this.infoManager.Debug("[All] Start preLoad MeasureLightId, {timer4.timeSpend} ms");
                string calibrationFile = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration\{GlobalVar.ProcessInfo.CalibrationFile}";
                string pathX = $@"{calibrationFile}\CoeffX.tif";
                string pathY = $@"{calibrationFile}\CoeffY.tif";
                string pathZ = $@"{calibrationFile}\CoeffZ.tif";

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    MIL.MbufImport(
                    pathX,
                    MIL.M_DEFAULT,
                    MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                    MyMil.MilSystem,
                    ref imgX);
                }

                MIL.MbufImport(
                    pathY,
                    MIL.M_DEFAULT,
                    MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                    MyMil.MilSystem,
                    ref imgY);

                if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                {
                    MIL.MbufImport(
                    pathZ,
                    MIL.M_DEFAULT,
                    MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                    MyMil.MilSystem,
                    ref imgZ);
                }

                this.coeffImage = new MilColorImage(imgX, imgY, imgZ);
                this.isLightCoeffReady = true;
                timer4.Stop();
                this.infoManager.Debug("[All] PreLoad MeasureLightId completed, {timer4.timeSpend} ms");
            }

        }
        #endregion

        #region --- CheckState ---
        private void CheckState(bool isRun)
        {
            if (!isRun)
            {
                this.State = EnState.Idle;
                this.FinalStatus = EnumFinalStatus.OK;
                this.infoManager.General("TaskOneColorCalibration failed. User stop the process.");
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
                            // Check Xyz Filter Status
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

                        }
                        break;

                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                    case EnumMotoControlPackage.CircleTac:
                        {
                            if (this.motorControl.IsWork())
                            {
                                // Check Xyz Filter Status
                                if (this.motorControl.MoveStatus_FW1() == UnitStatus.Finish)
                                    XyzReady = true;
                                else
                                    XyzReady = false;

                                // Check Nd Filter Status
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

        #region --- 主流程 : Run ---
        public void Run(bool isRun)
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
                        FinalStatus = 0;

                        this.timer1.Start();

                        MyMilIp.predictData = new MeasureData();

                        this.isLightCoeffReady = false;
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

                        GlobalVar.LightMeasure.TristimulusImage.Free();
                        GlobalVar.LightMeasure.ChromaImage.Free();

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
                            this.State = EnState.PreLoadCalibrationFile;
                        }
                        else
                        {
                            this.infoManager.Error("No Recipe has been Chosen");
                            this.State = EnState.Alarm;
                        }

                        this.timer1.Stop();
                        this.infoManager.Debug($"CheckRecipe {this.timer1.timeSpend} ms");
                    }
                    break;

                case EnState.PreLoadCalibrationFile:
                    {
                        PreLoadCalibrationId = new Thread(this.PreloadCorrectionData);

                        PreLoadCalibrationId.Start();

                        State = EnState.StartProcess;
                    }
                    break;

                case EnState.StartProcess:
                    {
                        this.timer1.Start();

                        GlobalVar.Device.XyzPosition = 0;  // XYZ-Filter: 0->X; 1->Y; 2->Z; 3->W (AIC_4_Positions or AIL_4_Positions) ; 0->Y; 1->W

                        this.xyzIpStep = (GlobalVar.SD.XYZFilter_PositionMode != EnumXYZFilter_PositionMode.AIL_2_Positions) ? 0 : 1;

                        this.State = EnState.CheckLightConvertType;

                        this.timer1.Stop();

                        this.infoManager.Debug($"StartProcess {this.timer1.timeSpend} ms");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Process Start");
                    }
                    break;

                case EnState.CheckLightConvertType:
                    {
                        switch (LightConvertType)
                        {
                            case EnumLightConvertType.CorrFile:
                                {
                                    this.State = EnState.MoveAllFilter;
                                }
                                break;

                            case EnumLightConvertType.Spectral:
                                {
                                    this.SpectralType = param.SpectralType;

                                    if (this.SpectralType == "Auto")
                                    {
                                        this.State = EnState.NdPredict_MoveND;
                                    }
                                    else
                                    {
                                        this.State = EnState.GetSpectralPara;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case EnState.NdPredict_MoveND:
                    {
                        bool CheckPara = false;

                        if (GlobalVar.CorrData.Para_List.Count == 0)
                        {
                            this.infoManager.Error($"ND Predict Fail , Correction Data is null");
                            State = EnState.Alarm;
                            break;
                        }

                        if (NdPredictIdx >= NdPredictList.Length)
                        {
                            this.infoManager.Error($"ND Predict Fail , All ND is try to Match");
                            State = EnState.Alarm;
                            break;
                        }

                        // 獲取枚舉成員的 FieldInfo
                        FieldInfo field = this.NdPredictList[NdPredictIdx].GetType().GetField(this.NdPredictList[NdPredictIdx].ToString());
                        // 獲取 DescriptionAttribute
                        DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                        NdPredictName = attribute.Description;

                        foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
                        {
                            if (this.NdPredictList[NdPredictIdx] == Para.ND)
                            {
                                CheckPara = true;
                            }
                        }

                        if (!CheckPara)
                        {
                            this.infoManager.Debug($"ND Predict Fail , Data is not Exist ({NdPredictName}) , Next Run");
                            NdPredictIdx++;
                            break;
                        }

                        this.timer1.Start();

                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        GlobalVar.Device.XyzPosition = 0;
                        GlobalVar.Device.NdPosition = (int)this.NdPredictList[NdPredictIdx];

                        if (this.isActive)
                        {
                            // Change Wheel
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter != null)
                                        {
                                            if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                            {
                                                GlobalVar.Device.XyzFilter.ChangeWheel(GlobalVar.Device.XyzPosition);
                                            }
                                            else
                                            {
                                                this.infoManager.General($"XyzFilter was not work !");
                                            }
                                        }

                                        if (GlobalVar.Device.NdFilter != null)
                                        {
                                            if (GlobalVar.Device.NdFilter.is_FW_Work)
                                            {
                                                GlobalVar.Device.NdFilter.ChangeWheel(GlobalVar.Device.NdPosition);
                                            }
                                            else
                                            {
                                                this.infoManager.General($"NdFilter was not work !");
                                            }
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (this.motorControl.IsWork())
                                        {
                                            this.motorControl.Move(-1, -1, GlobalVar.Device.XyzPosition, GlobalVar.Device.NdPosition);
                                        }
                                    }
                                    break;
                            }

                            this.SetCorrGain(GlobalVar.Device.XyzPosition);

                            this.timer1.Stop();
                            this.infoManager.Debug($"ND Predict [{NdPredictName}] , MoveAllFilter , XYZ->{GlobalVar.Device.XyzPosition},ND->{GlobalVar.Device.NdPosition} {this.timer1.timeSpend} ms");
                        }

                        this.timer1.Start(); // 紀錄轉動時間
                        this.State = EnState.NdPredict_CheckMoveND;
                    }
                    break;

                case EnState.NdPredict_CheckMoveND:
                    {
                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        if (this.CheckAllWheelReady())
                        {
                            this.timer1.Stop();
                            this.infoManager.Debug($"ND Predict [{NdPredictName}] , WaitAllFilterMoveFinish {this.timer1.timeSpend} ms");

                            this.State = EnState.NdPredict_Capture;
                        }
                    }
                    break;

                case EnState.NdPredict_Capture:
                    {
                        double ExpTime = this.param.AutoExposureMinTime;
                        this.grabber.SetExposureTime(ExpTime);
                        this.grabber.Grab();
                        this.infoManager.Debug($"ND Predict [{NdPredictName}] , Capture , ExpTime = {ExpTime} us");

                        this.State = EnState.NdPredict_CalGray;
                    }
                    break;

                case EnState.NdPredict_CalGray:
                    {
                        RectRegionInfo Info = new RectRegionInfo();
                        Info.StartX = this.param.ThirdPartyRoiX;
                        Info.StartY = this.param.ThirdPartyRoiY;
                        Info.Width = this.param.ThirdPartyRoiWidth;
                        Info.Height = this.param.ThirdPartyRoiHeight;

                        double AreaMean = MyMilIp.StatRectMean(this.grabber.grabImage, Info);

                        this.infoManager.Debug($"ND Predict [{NdPredictName}] , CalGary = {AreaMean}");

                        if (AreaMean >= 4095) //假設過曝，換下個ND
                        {
                            this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , OverExposed , Next Run");

                            NdPredictIdx++;
                            this.State = EnState.NdPredict_MoveND;
                        }
                        else
                        {
                            //判斷清單 = 設定參數 + 0 + 4095

                            List<int> SpecValue = new List<int>();

                            foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
                            {
                                if (Para.ND == NdPredictList[NdPredictIdx])
                                {
                                    SpecValue.Add(Para.TargetGray);
                                }
                            }

                            SpecValue.Add(0);
                            SpecValue.Add(4095);
                            SpecValue.Sort();

                            string GrayList = string.Join(", ", SpecValue);
                            string DebugMsg = $"ND Predict [{NdPredictName}] , JugedList(Gray) = {GrayList}";
                            this.infoManager.Debug($"{DebugMsg}");

                            int MatchGray = 0;

                            //判斷範圍
                            for (int i = 0; i < SpecValue.Count - 1; i++)
                            {
                                int RangeStart = SpecValue[i];
                                int RangeEnd = SpecValue[i + 1];

                                if (AreaMean >= RangeStart && AreaMean <= RangeEnd)
                                {
                                    MatchGray = RangeEnd;
                                    break;
                                }
                            }

                            if (MatchGray == 4095)  //沒過曝，也沒匹配，判斷下個ND數量
                            {
                                this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , Match Fail , Retry to Match Next ND");

                                int NextIdx = NdPredictIdx + 1;

                                if (NextIdx >= NdPredictList.Length)
                                {
                                    this.infoManager.Error($"ND Predict , Match Fail , All ND is try to Match");
                                    this.State = EnState.Alarm;
                                    break;
                                }

                                FW_ND_Remark NextND = NdPredictList[NdPredictIdx + 1];
                                List<int> NextGray = new List<int>();

                                FieldInfo field = this.NdPredictList[NdPredictIdx + 1].GetType().GetField(this.NdPredictList[NdPredictIdx + 1].ToString());
                                // 獲取 DescriptionAttribute
                                DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                                string NextNDName = attribute.Description;

                                foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
                                {
                                    if (Para.ND == NextND)
                                    {
                                        NextGray.Add(Para.TargetGray);
                                    }
                                }

                                if (NextGray.Count == 1) //下個ND只有一組，結束
                                {
                                    this.SpectralType = $"{NextNDName}_{NextGray[0]}";

                                    this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , Next ND Data has only 1 Gray , Correction Para is Match = {this.SpectralType}");
                                    this.State = EnState.GetSpectralPara;
                                }
                                else if (NextGray.Count == 0) //下個ND沒資料
                                {
                                    this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , Next ND Data is not Exist , Next Run");

                                    NdPredictIdx++;
                                    this.State = EnState.NdPredict_MoveND;
                                }
                                else //下個0或N組，換下個ND
                                {
                                    this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , Next ND Data has N Grays  , Next Run");

                                    NdPredictIdx++;
                                    this.State = EnState.NdPredict_MoveND;
                                }
                            }
                            else //匹配完成
                            {
                                this.SpectralType = $"{NdPredictName}_{MatchGray}";

                                this.infoManager.Debug($"ND Predict [{NdPredictName}] , Gray = {AreaMean} , Correction Para is Match = {this.SpectralType}");

                                this.State = EnState.GetSpectralPara;
                            }
                        }

                    }
                    break;


                case EnState.GetSpectralPara:
                    {
                        string[] SplitTxt = this.SpectralType.Split('_');

                        string ND = SplitTxt[0];
                        string Gray = SplitTxt[1];

                        SpectralPara = null;

                        foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
                        {
                            // 獲取枚舉成員的 FieldInfo
                            FieldInfo field = Para.ND.GetType().GetField(Para.ND.ToString());
                            // 獲取 DescriptionAttribute
                            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                            string Para_ND = attribute.Description;
                            string Para_Gray = Para.TargetGray.ToString();

                            if (Para_ND == ND && Para_Gray == Gray)
                            {
                                SpectralPara = Para;
                            }
                        }

                        if (SpectralPara == null)
                        {
                            this.infoManager.Error($"Get Spectral Para Fail : {SpectralType}");
                            this.State = EnState.Alarm;
                        }
                        else
                        {
                            this.infoManager.General($"Get Spectral Para Succed : {SpectralType}");
                            this.State = EnState.MoveAllFilter;
                        }
                    }
                    break;

                case EnState.MoveAllFilter:
                    {
                        this.timer1.Start();

                        this.isActive = this.CheckActiveByMeasureMode(this.xyzIpStep);

                        switch (LightConvertType)
                        {
                            case EnumLightConvertType.CorrFile:
                                {
                                    GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[this.xyzIpStep];
                                    GlobalVar.Device.NdPosition = param.FilterWheel_ND_OneColor[this.xyzIpStep];
                                }
                                break;

                            case EnumLightConvertType.Spectral:
                                {
                                    GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[this.xyzIpStep];
                                    GlobalVar.Device.NdPosition = (int)SpectralPara.ND;
                                }
                                break;
                        }

                        if (this.isActive)
                        {
                            // Change Wheel
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter != null)
                                        {
                                            if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                            {
                                                GlobalVar.Device.XyzFilter.ChangeWheel(GlobalVar.Device.XyzPosition);
                                            }
                                            else
                                            {
                                                this.infoManager.General($"XyzFilter was not work !");
                                            }
                                        }

                                        if (GlobalVar.Device.NdFilter != null)
                                        {
                                            if (GlobalVar.Device.NdFilter.is_FW_Work)
                                            {
                                                GlobalVar.Device.NdFilter.ChangeWheel(GlobalVar.Device.NdPosition);
                                            }
                                            else
                                            {
                                                this.infoManager.General($"NdFilter was not work !");
                                            }
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (this.motorControl.IsWork())
                                        {
                                            this.motorControl.Move(-1, -1, GlobalVar.Device.XyzPosition, GlobalVar.Device.NdPosition);
                                        }
                                    }
                                    break;
                            }

                            this.SetCorrGain(GlobalVar.Device.XyzPosition);

                            this.timer1.Stop();
                            this.infoManager.Debug($"MoveAllFilter,XYZ->{GlobalVar.Device.XyzPosition},ND->{GlobalVar.Device.NdPosition} {this.timer1.timeSpend} ms");
                        }

                        State = EnState.WaitAllFilterMoveFinish;

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
                        {
                            this.State = EnState.CheckCalibrationIdIsReady;
                        }
                    }
                    break;

                case EnState.Capture:
                    {
                        if (this.isActive)
                        {
                            AutoExpTime_FlowPara AutoExpTimePara = new AutoExpTime_FlowPara();
                            AutoExpTimePara.AutoExposureEnable = this.param.AutoExposureEnable;
                            AutoExpTimePara.AutoExposureChangeStep = this.param.AutoExposureChangeStep;
                            AutoExpTimePara.AutoExposureMinTime = this.param.AutoExposureMinTime;
                            AutoExpTimePara.MaxExposureTime = this.param.MaxExposureTime;
                            AutoExpTimePara.ExposureTimedDefault = this.param.ExposureTimedDefault[xyzIpStep];
                            AutoExpTimePara.AutoExposureTargetGray = this.param.AutoExposureTargetGray[xyzIpStep];
                            AutoExpTimePara.AutoExposureGrayTolerance = this.param.AutoExposureGrayTolerance;
                            AutoExpTimePara.AutoExposureIsUseRoi = this.param.AutoExposureIsUseRoi;

                            switch (LightConvertType)
                            {
                                case EnumLightConvertType.CorrFile:
                                    {
                                        AutoExpTimePara.AutoExposureRoiX = this.param.AutoExposureRoiX;
                                        AutoExpTimePara.AutoExposureRoiY = this.param.AutoExposureRoiY;
                                        AutoExpTimePara.AutoExposureRoiWidth = this.param.AutoExposureRoiWidth;
                                        AutoExpTimePara.AutoExposureRoiHeight = this.param.AutoExposureRoiHeight;
                                    }
                                    break;

                                case EnumLightConvertType.Spectral:
                                    {
                                        AutoExpTimePara.AutoExposureRoiX = this.param.ThirdPartyRoiX;
                                        AutoExpTimePara.AutoExposureRoiY = this.param.ThirdPartyRoiY;
                                        AutoExpTimePara.AutoExposureRoiWidth = this.param.ThirdPartyRoiWidth;
                                        AutoExpTimePara.AutoExposureRoiHeight = this.param.ThirdPartyRoiHeight;
                                    }
                                    break;
                            }

                            AutoExpTimePara.FixedExposureTime = this.param.FixedExposureTime[xyzIpStep];
                            AutoExpTimePara.IsSaveSourceImage = this.param.AutoExposure_SaveImage;
                            AutoExpTimePara.IsSaveDebugImage = this.param.IsSaveDebugImage;
                            AutoExpTimePara.HotPixelIsDoStep = this.param.HotPixelIsDoStep;
                            AutoExpTimePara.HotPixel_Pitch = this.param.HotPixel_Pitch[xyzIpStep];
                            AutoExpTimePara.HotPixel_BTH = this.param.HotPixel_BTH[xyzIpStep];
                            AutoExpTimePara.HotPixel_DTH = this.param.HotPixel_DTH[xyzIpStep];

                            switch (xyzIpStep)
                            {
                                case 0: AutoExpTimePara.Gain = this.param.CameraGainX; break;
                                case 1: AutoExpTimePara.Gain = this.param.CameraGainY; break;
                                case 2: AutoExpTimePara.Gain = this.param.CameraGainZ; break;
                            }

                            if (LightConvertType == EnumLightConvertType.Spectral)
                            {
                                AutoExpTimePara.AutoExposureTargetGray = SpectralPara.TargetGray;
                            }

                            this.autoExposure = new Task_AutoExpTime(AutoExpTimePara, this.xyzIpStep, this.infoManager, this.grabber);
                            this.autoExposure.State = Task_AutoExpTime.EnState.Init;
                            this.autoExposure.Start();
                        }

                        State = EnState.CaptureFinish;
                    }
                    break;

                case EnState.CaptureFinish:
                    {
                        if (this.isActive)
                        {
                            if (this.autoExposure.IsDone)
                            {
                                if (this.autoExposure.State == Task_AutoExpTime.EnState.Finish)
                                {
                                    if (this.autoExposure.ErrorCode == "")
                                    {
                                        ExpTimeResult[xyzIpStep] = this.autoExposure.Result.ExpTime;
                                        State = EnState.GoNextXyzFilter;
                                    }
                                    else
                                    {   //程序發生錯誤
                                        this.ErrorCode = this.autoExposure.ErrorCode;
                                        State = EnState.Alarm;
                                    }
                                }
                                else if (this.autoExposure.State == Task_AutoExpTime.EnState.Warning)
                                {   //曝光失敗，太亮
                                    this.ErrorCode = this.autoExposure.ErrorCode;
                                    State = EnState.Warning;
                                    this.autoExposure.State = Task_AutoExpTime.EnState.Idle;
                                }
                            }
                        }
                        else
                            State = EnState.GoNextXyzFilter;

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

                        int Idx = this.xyzIpStep + 1;
                        if (Idx == 3) Idx = 0;

                        switch (LightConvertType)
                        {
                            case EnumLightConvertType.CorrFile:
                                {
                                    GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[Idx];
                                    GlobalVar.Device.NdPosition = param.FilterWheel_ND_OneColor[Idx];
                                }
                                break;

                            case EnumLightConvertType.Spectral:
                                {
                                    GlobalVar.Device.XyzPosition = param.FilterWheel_XYZ_OneColor[Idx];
                                    GlobalVar.Device.NdPosition = (int)SpectralPara.ND;
                                }
                                break;
                        }

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

                                        if (GlobalVar.Device.NdFilter.is_FW_Work)
                                            GlobalVar.Device.NdFilter.ChangeWheel(GlobalVar.Device.NdPosition);
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
                            if ((this.xyzIpStep == 0 && this.CalibrationDataX.IsReady()) ||
                                (this.xyzIpStep == 1 && this.CalibrationDataY.IsReady()) ||
                                (this.xyzIpStep == 2 && this.CalibrationDataZ.IsReady()))
                            {
                                State = EnState.IpBaseCalibration;
                            }
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            if (this.CalibrationDataY != null && this.CalibrationDataY.IsReady())
                                State = EnState.IpBaseCalibration;
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
                                            this.baseCalibrationX = new TaskBaseCalibration(this.param,
                                                                                       this.CalibrationDataX,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationX.State = TaskBaseCalibration.EnState.Init;

                                            try
                                            {
                                                this.baseCalibrationX.Run();
                                            }
                                            catch (Exception ex)
                                            {
                                                this.ErrorCode = ex.Message;
                                                this.State = EnState.Alarm;
                                            }
                                        }
                                        break;

                                    case 1:
                                        {
                                            this.baseCalibrationY = new TaskBaseCalibration(this.param,
                                                                                       this.CalibrationDataY,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationY.State = TaskBaseCalibration.EnState.Init;

                                            try
                                            {
                                                this.baseCalibrationY.Run();
                                            }
                                            catch (Exception ex)
                                            {
                                                this.ErrorCode = ex.Message;
                                                this.State = EnState.Alarm;
                                            }
                                        }
                                        break;

                                    case 2:
                                        {
                                            this.autoExposure.ShotDown();
                                            this.baseCalibrationZ = new TaskBaseCalibration(this.param,
                                                                                       this.CalibrationDataZ,
                                                                                       ref this.grabber.grabImage,
                                                                                       this.xyzIpStep,
                                                                                       this.infoManager);

                                            this.baseCalibrationZ.State = TaskBaseCalibration.EnState.Init;

                                            try
                                            {
                                                this.baseCalibrationZ.Run();
                                            }
                                            catch (Exception ex)
                                            {
                                                this.ErrorCode = ex.Message;
                                                this.State = EnState.Alarm;
                                            }
                                        }
                                        break;
                                }
                            }

                            if (this.xyzIpStep != 2)
                            {
                                this.xyzIpStep++;
                                this.State = EnState.WaitAllFilterMoveFinish;

                            }
                            else
                            {
                                this.State = EnState.IpWaitBaseCalibrationX;
                            }
                        }
                        else if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
                        {
                            this.baseCalibrationY = new TaskBaseCalibration(this.param,
                                           this.CalibrationDataY,
                                           ref this.grabber.grabImage,
                                           this.xyzIpStep,
                                           this.infoManager);

                            this.baseCalibrationY.State = TaskBaseCalibration.EnState.Init;

                            try
                            {
                                this.baseCalibrationY.Run();
                            }
                            catch (Exception ex)
                            {
                                this.ErrorCode = ex.Message;
                                this.State = EnState.Alarm;
                            }

                            this.State = EnState.IpWaitBaseCalibrationX;
                        }

                    }
                    break;

                case EnState.IpWaitBaseCalibrationX:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.baseCalibrationX.IsDone)
                            {
                                MyMilIp.predictData.ExposureTime.X = GlobalVar.ProcessInfo.XyzExposureTime[0];
                                if (param.FindExtremeIsDoStep)
                                {
                                    Point[] minPeakPosition = new Point[1];
                                    Point[] maxPeakPosition = new Point[1];
                                    double[] PeakValue = new double[2];
                                    string rtn = MyMilIp.FindExtremebyConvolution(baseCalibrationX.FlowImage, new Size(param.FindExtremeKernelSize[0], param.FindExtremeKernelSize[1]), param.FindExtremeROIPadding, "X", GlobalVar.Config.DirectoryPath, ref minPeakPosition, ref maxPeakPosition, ref PeakValue);
                                    if (rtn == "OK")
                                    {
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: minPeakValue={PeakValue[0]}");
                                        for (int peakindex = 0; peakindex < minPeakPosition.Length; peakindex++)
                                        {
                                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: minPeakPosition[{peakindex}]=({minPeakPosition[peakindex].X},{minPeakPosition[peakindex].Y})");
                                        }
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: maxPeakValue={PeakValue[1]}");
                                        for (int peakindex = 0; peakindex < maxPeakPosition.Length; peakindex++)
                                        {
                                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: maxPeakPosition[{peakindex}]=({maxPeakPosition[peakindex].X},{maxPeakPosition[peakindex].Y})");
                                        }
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: U%={PeakValue[0] / PeakValue[1] * 100}%");
                                    }
                                    else
                                    {
                                        this.infoManager.Error($"[{GlobalVar.ProcessInfo.Pattern}-X] FindExtremebyConvolution: {rtn}");
                                    }

                                }
                                this.State = EnState.IpWaitBaseCalibrationY;
                            }
                        }
                        else
                            this.State = EnState.IpWaitBaseCalibrationY;
                    }
                    break;

                case EnState.IpWaitBaseCalibrationY:
                    {
                        if (this.baseCalibrationY.IsDone)
                        {
                            MyMilIp.predictData.ExposureTime.Y = GlobalVar.ProcessInfo.XyzExposureTime[1];
                            if (param.FindExtremeIsDoStep)
                            {
                                Point[] minPeakPosition = new Point[1];
                                Point[] maxPeakPosition = new Point[1];
                                double[] PeakValue = new double[2];
                                string rtn = MyMilIp.FindExtremebyConvolution(baseCalibrationY.FlowImage, new Size(param.FindExtremeKernelSize[0], param.FindExtremeKernelSize[1]), param.FindExtremeROIPadding, "Y", GlobalVar.Config.DirectoryPath, ref minPeakPosition, ref maxPeakPosition, ref PeakValue);
                                if (rtn == "OK")
                                {
                                    this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: minPeakValue={PeakValue[0]}");
                                    for (int peakindex = 0; peakindex < minPeakPosition.Length; peakindex++)
                                    {
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: minPeakPosition[{peakindex}]=({minPeakPosition[peakindex].X},{minPeakPosition[peakindex].Y})");
                                    }
                                    this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: maxPeakValue={PeakValue[1]}");
                                    for (int peakindex = 0; peakindex < maxPeakPosition.Length; peakindex++)
                                    {
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: maxPeakPosition[{peakindex}]=({maxPeakPosition[peakindex].X},{maxPeakPosition[peakindex].Y})");
                                    }
                                    this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: U%={PeakValue[0] / PeakValue[1] * 100}%");
                                }
                                else
                                {
                                    this.infoManager.Error($"[{GlobalVar.ProcessInfo.Pattern}-Y] FindExtremebyConvolution: {rtn}");
                                }
                            }
                            this.State = EnState.IpWaitBaseCalibrationZ;
                        }
                    }
                    break;

                case EnState.IpWaitBaseCalibrationZ:
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.baseCalibrationZ.IsDone)
                            {
                                MyMilIp.predictData.ExposureTime.Z = GlobalVar.ProcessInfo.XyzExposureTime[2];
                                if (param.FindExtremeIsDoStep)
                                {
                                    Point[] minPeakPosition = new Point[1];
                                    Point[] maxPeakPosition = new Point[1];
                                    double[] PeakValue = new double[2];
                                    string rtn = MyMilIp.FindExtremebyConvolution(baseCalibrationZ.FlowImage, new Size(param.FindExtremeKernelSize[0], param.FindExtremeKernelSize[1]), param.FindExtremeROIPadding, "Z", GlobalVar.Config.DirectoryPath, ref minPeakPosition, ref maxPeakPosition, ref PeakValue);
                                    if (rtn == "OK")
                                    {
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: minPeakValue={PeakValue[0]}");
                                        for (int peakindex = 0; peakindex < minPeakPosition.Length; peakindex++)
                                        {
                                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: minPeakPosition[{peakindex}]=({minPeakPosition[peakindex].X},{minPeakPosition[peakindex].Y})");
                                        }
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: maxPeakValue={PeakValue[1]}");
                                        for (int peakindex = 0; peakindex < maxPeakPosition.Length; peakindex++)
                                        {
                                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: maxPeakPosition[{peakindex}]=({maxPeakPosition[peakindex].X},{maxPeakPosition[peakindex].Y})");
                                        }
                                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: U%={PeakValue[0] / PeakValue[1] * 100}%");
                                    }
                                    else
                                    {
                                        this.infoManager.Error($"[{GlobalVar.ProcessInfo.Pattern}-Z] FindExtremebyConvolution: {rtn}");
                                    }
                                }
                                this.State = EnState.LightSourceCorrection;
                            }
                        }
                        else
                            this.State = EnState.LightSourceCorrection;
                    }
                    break;

                case EnState.LightSourceCorrection:    //產生 TX, TY, TZ, Cx, Cy
                    {
                        switch (LightConvertType)
                        {
                            case EnumLightConvertType.CorrFile:
                                {
                                    if (!this.isLightCoeffReady)
                                    {
                                        this.infoManager.Error($"Light Coeff is not Ready");
                                        this.State = EnState.Alarm;

                                        break;
                                    }

                                    this.timer1.Start();
                                    this.isLightCoeffReady = false;

                                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                                    {
                                        if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                                        {
                                            this.colorImage = new MilColorImage(this.baseCalibrationX.FlowImage, this.baseCalibrationY.FlowImage, this.baseCalibrationZ.FlowImage);
                                            MyMilIp.MeasureLightPredict(this.colorImage, this.coeffImage, MyMilIp.predictData, ref GlobalVar.LightMeasure.TristimulusImage, ref GlobalVar.LightMeasure.ChromaImage);
                                        }
                                        else
                                        {
                                            this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);
                                            MyMilIp.Mesure_TristimulusY_Predict(this.monoImage, this.coeffImage, MyMilIp.predictData, ref GlobalVar.LightMeasure.TristimulusImage);
                                        }
                                    }
                                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                                    {
                                        this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);

                                        MyMilIp.Mesure_TristimulusY_Predict(this.monoImage, this.coeffImage, MyMilIp.predictData, ref GlobalVar.LightMeasure.TristimulusImage);
                                    }

                                    // 設定要顯示的影像
                                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 0);
                                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 1);
                                    frmMain.IsNeedUpdate = true;

                                    this.timer1.Stop();
                                    this.infoManager.Debug($"LightSourceCorrection {timer1.timeSpend} ms");

                                    this.State = EnState.PixelAlign;
                                }
                                break;

                            case EnumLightConvertType.Spectral:
                                {
                                    this.timer1.Start();

                                    if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                                    {
                                        if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                                        {
                                            this.colorImage = new MilColorImage(this.baseCalibrationX.FlowImage, this.baseCalibrationY.FlowImage, this.baseCalibrationZ.FlowImage);
                                            MyMilIp.SpectralCorr(this.colorImage, ExpTimeResult, SpectralPara, ref GlobalVar.LightMeasure.TristimulusImage, ref GlobalVar.LightMeasure.ChromaImage);
                                        }
                                        else
                                        {
                                            this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);
                                            MyMilIp.SpectralCorrY(this.monoImage, ExpTimeResult[1], SpectralPara, ref GlobalVar.LightMeasure.TristimulusImage);
                                        }
                                    }
                                    else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                                    {
                                        this.monoImage = new MilMonoImage(this.baseCalibrationY.FlowImage);
                                        MyMilIp.SpectralCorrY(this.monoImage, ExpTimeResult[1], SpectralPara, ref GlobalVar.LightMeasure.TristimulusImage);
                                    }

                                    MIL_ID[] Img = { GlobalVar.LightMeasure.TristimulusImage.ImgX, GlobalVar.LightMeasure.TristimulusImage.ImgY, GlobalVar.LightMeasure.TristimulusImage.ImgZ };

                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (Img[i] == MIL.M_NULL) continue;
                                        string ImgName = $"{GlobalVar.ProcessInfo.Pattern}_{GlobalVar.ProcessInfo.XyzStr[i]}_Corrected.tif";
                                        MIL.MbufSave(GlobalVar.Config.DirectoryPath + @"\" + ImgName, Img[i]);
                                    }

                                    // 設定要顯示的影像
                                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 0);
                                    MyMilDisplay.SetDisplayImage(ref GlobalVar.LightMeasure.TristimulusImage.ImgY, 1);
                                    frmMain.IsNeedUpdate = true;

                                    this.timer1.Stop();
                                    this.infoManager.Debug($"LightSourceCorrection {timer1.timeSpend} ms");

                                    this.State = EnState.PixelAlign;
                                }
                                break;
                        }
                    }
                    break;

                case EnState.PixelAlign:
                    {
                        if (this.param.PixelAlignIsDoStep)
                        {
                            PixelAlignParam pixelAlignParam = new PixelAlignParam();

                            pixelAlignParam.setting.outPath = GlobalVar.Config.DirectoryPath;

                            pixelAlignParam.panelInfo.ResolutionX = this.param.PixelAlign_panelInfo_ResolutionX[0];
                            pixelAlignParam.panelInfo.ResolutionY = this.param.PixelAlign_panelInfo_ResolutionY[0];

                            pixelAlignParam.setting.CoarseAlignSetting.Threshold = this.param.PixelAlign_Coarse_Threshold[0];
                            pixelAlignParam.setting.CoarseAlignSetting.AreaMin = this.param.PixelAlign_FindPanelPixel_AreaMin[0];
                            pixelAlignParam.setting.CoarseAlignSetting.CloseNums = this.param.PixelAlign_Coarse_CloseNums[0];
                            pixelAlignParam.setting.CoarseAlignSetting.DebugImage = false;

                            pixelAlignParam.setting.FindPanelPixelSetting.AreaMin = this.param.PixelAlign_FindPanelPixel_AreaMin[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.FirstPixelX = this.param.PixelAlign_FindPanelPixel_FirstPixelX[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.FirstPixelY = this.param.PixelAlign_FindPanelPixel_FirstPixelY[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.RowFindPitchX = this.param.PixelAlign_FindPanelPixel_RowFindPitchX[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.RowFindPitchY = this.param.PixelAlign_FindPanelPixel_RowFindPitchY[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.ColFindPitchX = this.param.PixelAlign_FindPanelPixel_ColFindPitchX[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.ColFindPitchY = this.param.PixelAlign_FindPanelPixel_ColFindPitchY[0];
                            pixelAlignParam.setting.FindPanelPixelSetting.DebugImage = false;

                            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                            {
                                if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                                {
                                    pixelAlignParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelAlign(pixelAlignParam);

                                    pixelAlignParam.Image_Path = string.Format("{0}\\ImgChromaX.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelAlign(pixelAlignParam);

                                    pixelAlignParam.Image_Path = string.Format("{0}\\ImgChromaY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelAlign(pixelAlignParam);
                                }
                                else
                                {   // 只能是 TristimulusY
                                    pixelAlignParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelAlign(pixelAlignParam);
                                }

                            }
                            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                            {
                                pixelAlignParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                MyMilIp.PixelAlign(pixelAlignParam);
                            }

                        }

                        this.State = EnState.PixelStatistic;
                    }
                    break;

                case EnState.PixelStatistic:
                    {
                        if (this.param.PixelStatIsDoStep)
                        {
                            PixelStatParam pixelStatParam = new PixelStatParam();

                            pixelStatParam.DrawCircle = false;
                            pixelStatParam.setting.outPath = GlobalVar.Config.DirectoryPath;

                            pixelStatParam.CogDataFile_Path = GlobalVar.Config.DirectoryPath + "\\CogData.xml";
                            pixelStatParam.TristimulusX_Path = string.Format("{0}\\ImgTristimulusX.tif", GlobalVar.Config.DirectoryPath);
                            pixelStatParam.TristimulusY_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                            pixelStatParam.TristimulusZ_Path = string.Format("{0}\\ImgTristimulusZ.tif", GlobalVar.Config.DirectoryPath);
                            pixelStatParam.ChromaCx_Path = string.Format("{0}\\ImgChromaX.tif", GlobalVar.Config.DirectoryPath);
                            pixelStatParam.ChromaCy_Path = string.Format("{0}\\ImgChromaY.tif", GlobalVar.Config.DirectoryPath);
                            pixelStatParam.radius = this.param.PixelAlign_Stat_Radius[0];

                            pixelStatParam.panelInfo.ResolutionX = this.param.PixelAlign_panelInfo_ResolutionX[0];
                            pixelStatParam.panelInfo.ResolutionY = this.param.PixelAlign_panelInfo_ResolutionY[0];

                            pixelStatParam.setting.CoarseAlignSetting.Threshold = this.param.PixelAlign_Coarse_Threshold[0];
                            pixelStatParam.setting.CoarseAlignSetting.AreaMin = this.param.PixelAlign_FindPanelPixel_AreaMin[0];
                            pixelStatParam.setting.CoarseAlignSetting.CloseNums = this.param.PixelAlign_Coarse_CloseNums[0];
                            pixelStatParam.setting.CoarseAlignSetting.DebugImage = false;

                            pixelStatParam.setting.FindPanelPixelSetting.AreaMin = this.param.PixelAlign_FindPanelPixel_AreaMin[0];
                            pixelStatParam.setting.FindPanelPixelSetting.FirstPixelX = this.param.PixelAlign_FindPanelPixel_FirstPixelX[0];
                            pixelStatParam.setting.FindPanelPixelSetting.FirstPixelY = this.param.PixelAlign_FindPanelPixel_FirstPixelY[0];
                            pixelStatParam.setting.FindPanelPixelSetting.RowFindPitchX = this.param.PixelAlign_FindPanelPixel_RowFindPitchX[0];
                            pixelStatParam.setting.FindPanelPixelSetting.RowFindPitchY = this.param.PixelAlign_FindPanelPixel_RowFindPitchY[0];
                            pixelStatParam.setting.FindPanelPixelSetting.ColFindPitchX = this.param.PixelAlign_FindPanelPixel_ColFindPitchX[0];
                            pixelStatParam.setting.FindPanelPixelSetting.ColFindPitchY = this.param.PixelAlign_FindPanelPixel_ColFindPitchY[0];
                            pixelStatParam.setting.FindPanelPixelSetting.DebugImage = false;

                            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                            {
                                if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                                {
                                    pixelStatParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelStat(pixelStatParam);

                                    pixelStatParam.Image_Path = string.Format("{0}\\ImgChromaX.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelStat(pixelStatParam);

                                    pixelStatParam.Image_Path = string.Format("{0}\\ImgChromaY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelStat(pixelStatParam);
                                }
                                else
                                {
                                    pixelStatParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                    MyMilIp.PixelStat(pixelStatParam);
                                }
                            }
                            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                            {
                                pixelStatParam.Image_Path = string.Format("{0}\\ImgTristimulusY.tif", GlobalVar.Config.DirectoryPath);
                                MyMilIp.PixelStat(pixelStatParam);
                            }

                        }

                        this.State = EnState.Finish;
                    }
                    break;

                case EnState.Finish:
                    {
                        this.timer1.Start();

                        this.infoManager.General($"[One Color Calibration] PanelID :[{GlobalVar.ProcessInfo.PanelID}]");
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            if (this.xyzEnable[0] && this.xyzEnable[1] && this.xyzEnable[2])
                            {
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-X] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[0]} us");
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Z] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[2]} us");
                            }
                            else
                            {
                                this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");
                            }
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                            this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}-Y] ExposureTime = {GlobalVar.ProcessInfo.XyzExposureTime[1]} us");

                        //Recorder Temperature
                        this.Temperature_End = this.grabber.GetTemperature();
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Temperature Start = {Temperature_Start}");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Temperature End = {Temperature_End}");

                        this.timer1.Stop();
                        this.infoManager.Debug($"Finish {timer1.timeSpend} ms");
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Process Completed");

                        //Back to X Filter
                        GlobalVar.Device.XyzPosition = 0;

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

                        this.State = EnState.Idle;
                        this.FinalStatus = EnumFinalStatus.OK;
                    }
                    break;

                case EnState.Warning:
                    {
                        this.infoManager.General($"[{GlobalVar.ProcessInfo.Pattern}] Process Interrupted");

                        this.infoManager.Error(this.ErrorCode);
                        this.State = EnState.Idle;
                        this.FinalStatus = EnumFinalStatus.NG;
                    }
                    break;
                case EnState.Alarm:
                    {
                        MilNetHelper.MilBufferFree(ref this.baseCalibrationX.FlowImage);
                        MilNetHelper.MilBufferFree(ref this.baseCalibrationY.FlowImage);
                        MilNetHelper.MilBufferFree(ref this.baseCalibrationZ.FlowImage);

                        this.infoManager.Error(this.ErrorCode);
                        this.State = EnState.Idle;
                        this.FinalStatus = EnumFinalStatus.NG;
                    }
                    break;
            }
        }
        #endregion

        #endregion

    }
}
