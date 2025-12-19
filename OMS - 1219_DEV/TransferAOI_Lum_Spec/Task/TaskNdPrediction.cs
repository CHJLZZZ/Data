using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HardwareManager;
using static MyMilIp;
using CommonBase.Logger;
using BaseTool;
using FrameGrabber;
using System.IO;

namespace OpticalMeasuringSystem
{
    class TaskNdPrediction
    {
        public InfoManager infoManager = null;
        private MilDigitizer grabber = null;
        private IMotorControl motorControl = null;

        public string ErrorCode = "";

        public EnumFinalStatus FinalStatus = EnumFinalStatus.Initial; //0:initial, 1:OK, 2:NG

        public EnState State = EnState.Idle;

        public TaskNdPrediction(cls29MSetting param, InfoManager infoManager, MilDigitizer grabber, IMotorControl motorControl)
        {
            this.param = param;
            this.infoManager = infoManager;
            this.grabber = grabber;
            this.motorControl = motorControl;
        }

        public enum EnState
        {
            Idle,
            Init,

            MoveFilter,
            Check_MoveFilter,

            AutoExposureTime,
            Check_AutoExposureTime,

            SetNdFilter,
            SetXyzFilter,

            Finish,
            Alarm,
        }

        private cls29MSetting param;

        private myTimer timer1 = new myTimer(); // 流程時間
        private Task_AutoExpTime AutoExpTime = null;

        private int xyzFilterPostion = -1;
        private int ndFilterPostion = -1;

        private int minExposureTime = 0;


        private string OutputFolder = $@"D:\OMS\ND Prediction\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}\";

        private string[] ResultLog = new string[3];

        #region --- 方法函式 ---

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
        #endregion

        #region --- GetNDInfo ---
        private string GetXYZInfo(int xyzPostion)
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

            return "Unknow ND";
        }

        #endregion


        #region --- CheckState ---
        private void CheckState(bool isRun)
        {
            if (!isRun)
            {
                this.State = EnState.Idle;
                this.FinalStatus = EnumFinalStatus.OK;
                this.infoManager.General("TaskNdPrediction failed. User stop the process.");
            }
        }
        #endregion

        #region --- Run ---
        public void Run(bool isRun)
        {

            this.CheckState(isRun);

            switch (this.State)
            {
                case EnState.Idle:
                    {
                        this.ErrorCode = "";
                    }
                    break;

                case EnState.Init:
                    {
                        ResultLog = new string[3];
                        FinalStatus = EnumFinalStatus.Initial;

                        timer1.Start();

                        this.xyzFilterPostion = 0;
                        this.ndFilterPostion = 3;
                        this.minExposureTime = param.AutoExposureMinTime;

                        this.infoManager.Debug($"Use MinExposure Time {this.minExposureTime}");
                        this.infoManager.General($"Start Prediction Process.");

                        this.OutputFolder = $@"D:\OMS\ND Prediction\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}\";

                        this.State = EnState.MoveFilter;
                    }
                    break;

                case EnState.MoveFilter:
                    {
                        if (GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST")
                        {
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter != null)
                                        {
                                            if (GlobalVar.Device.XyzFilter.is_FW_Work)
                                                GlobalVar.Device.XyzFilter.ChangeWheel(xyzFilterPostion);
                                        }

                                        if (GlobalVar.Device.NdFilter != null)
                                        {
                                            if (GlobalVar.Device.NdFilter.is_FW_Work)
                                                GlobalVar.Device.NdFilter.ChangeWheel(ndFilterPostion);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (this.motorControl.IsWork())
                                            this.motorControl.Move(-1, -1, xyzFilterPostion, ndFilterPostion);
                                    }
                                    break;
                            }

                            this.State = EnState.Check_MoveFilter;
                        }
                        else
                        {
                            this.State = EnState.Alarm;
                        }
                    }
                    break;

                case EnState.Check_MoveFilter:
                    {
                        if (this.CheckAllWheelReady())
                        {
                            this.State = EnState.AutoExposureTime;
                        }
                    }
                    break;

                case EnState.AutoExposureTime:
                    {
                        AutoExpTime = new Task_AutoExpTime(param, xyzFilterPostion, this.infoManager, this.grabber);
                        AutoExpTime.Start(xyzFilterPostion);

                        this.infoManager.General($"Auto ExposureTime Start");

                        Thread.Sleep(100);
                        this.State = EnState.Check_AutoExposureTime;
                    }
                    break;

                case EnState.Check_AutoExposureTime:
                    {
                        if (!AutoExpTime.IsRun)
                        {
                            RecordPara RecordInfo = new RecordPara();
                            RecordInfo.ExposureTime = AutoExpTime.Result.ExpTime;
                            RecordInfo.Gray = AutoExpTime.Result.GrayMean;
                            RecordInfo.XYZ = GetXYZInfo(xyzFilterPostion);
                            RecordInfo.ND = GetNDInfo(ndFilterPostion);
                            RecordInfo.Succed = (AutoExpTime.State == Task_AutoExpTime.EnState.Finish);

                            switch (AutoExpTime.State)
                            {
                                case Task_AutoExpTime.EnState.Finish:
                                    {
                                        this.infoManager.General($"Auto ExposureTime Finish");
                                        this.State = EnState.SetXyzFilter;
                                    }
                                    break;

                                case Task_AutoExpTime.EnState.Warning:
                                case Task_AutoExpTime.EnState.Alarm:
                                    {
                                        this.infoManager.Error($"Auto ExposureTime Fail");
                                        this.State = EnState.SetNdFilter;
                                    }
                                    break;
                            }

                            if (RecordInfo.Succed)
                            {
                                ResultLog[xyzFilterPostion] = $"{RecordInfo.XYZ}, {RecordInfo.ND}, ExpTime = {RecordInfo.ExposureTime}, Gray = {RecordInfo.Gray}";
                            }

                            Directory.CreateDirectory(OutputFolder);

                            MIL.MbufSave($@"{OutputFolder}\{xyzFilterPostion}_{RecordInfo.XYZ}_{ndFilterPostion}_{RecordInfo.ND}.tiff", this.grabber.grabImage);
                            WriteRowData($@"{OutputFolder}\RowData.csv", RecordInfo);
                        }
                    }
                    break;

                case EnState.SetNdFilter:
                    {
                        if (this.ndFilterPostion == 2)
                        {
                            this.State = EnState.Alarm;
                            break;
                        }

                        switch (this.ndFilterPostion)
                        {
                            case 0:
                            case 1:
                                this.ndFilterPostion++;
                                break;
                            case 3:
                                this.ndFilterPostion = 0;
                                break;
                        }

                        this.State = EnState.MoveFilter;
                    }
                    break;

                case EnState.SetXyzFilter:
                    {
                        if (this.xyzFilterPostion == 2)
                        {
                            this.State = EnState.Finish;
                            break;
                        }
                        else
                        {
                            this.xyzFilterPostion++;
                            this.ndFilterPostion = 3;

                        }

                        this.State = EnState.MoveFilter;
                    }
                    break;

                case EnState.Finish:
                    {
                        // 設定要顯示的影像
                        MyMilDisplay.SetDisplayImage(ref this.grabber.grabImage, 1);
                        frmMain.IsNeedUpdate = true;

                        timer1.Stop();
                        this.infoManager.General($"The prediction process Completed.");
                        this.infoManager.Debug($"The predicted flow take {this.timer1.timeSpend} ms");

                        this.infoManager.General(ResultLog[0]);
                        this.infoManager.General(ResultLog[1]);
                        this.infoManager.General(ResultLog[2]);

                        this.FinalStatus = EnumFinalStatus.OK;
                        this.State = EnState.Idle;
                    }
                    break;

                case EnState.Alarm:
                    {
                        if (GlobalVar.Device.XyzFilter.is_FW_Work)
                            GlobalVar.Device.XyzFilter.ChangeWheel(0);

                        string xyz = ((FW_XYZ_Remark)this.xyzFilterPostion).ToString();
                        this.infoManager.General($"The {xyz}-Filter is overexposure when ND-Filter at position 2.");
                        this.infoManager.General("The prediction process is shot down.");
                        this.infoManager.General($"Please REDUCE the minimum exposure time or RAISE the target gray value in 'Parameter Setting'.");

                        this.FinalStatus = EnumFinalStatus.NG;
                        this.State = EnState.Idle;
                    }
                    break;
            }
        }
        #endregion

        #endregion

        private bool CheckAllWheelReady()
        {
            bool XyzReady = false;
            bool NdReady = false;

            if (!motorControl.IsMoveProc())
            {
                // Change Wheel Ready
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

        private bool WriteRowData(string SavePath, RecordPara Para)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("XYZ");
                    TitleList.Add("ND");
                    TitleList.Add("ExposureTime");
                    TitleList.Add("Gray");
                    TitleList.Add("Succed");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{Para.XYZ}");
                InfoList.Add($"{Para.ND}");
                InfoList.Add($"{Para.ExposureTime}");
                InfoList.Add($"{Para.Gray}");
                InfoList.Add($"{Para.Succed}");

                string Info = string.Join(",", InfoList);

                SB.Append(Info + "\r\n");

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                this.infoManager.Error($"Write RowData Fail : {ex.Message}");
                return false;
            }
        }

        public class RecordPara
        {
            public string XYZ { get; set; } = "";
            public string ND { get; set; } = "";
            public double ExposureTime { get; set; } = 0.0;
            public double Gray { get; set; } = 0.0;
            public bool Succed { get; set; } = false;
        }

    }
}
