using BaseTool;
using CommonBase.Logger;
using FrameGrabber;
using HardwareManager;
using LightMeasure;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OpticalMeasuringSystem
{
    public class Task_MonoChromatorMeasure
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;
        private MonoChromatorMeasure_FlowPara FlowPara = new MonoChromatorMeasure_FlowPara();

        //Task
        private Task_BaseAlign BaseAlign = null;

        //Flow
        private bool FlowRun = false;

        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_MonoChromatorMeasure(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            if (!isAlm)
            {
                this.Info.General($"{Msg}");
            }
            else
            {
                this.Info.Error($"{Msg}");
            }

            LogMsg?.Invoke(Msg);
        }

        public void Start(MonoChromatorMeasure_FlowPara SourcePara)
        {

            this.FlowPara.Clone(SourcePara);

            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            Thread mThread = new Thread(() => Flow(this.FlowPara));
            mThread.Start();
        }

        public void Stop()
        {
            this.FlowRun = false;

            if (this.BaseAlign != null)
            {
                this.BaseAlign.Stop();
            }
        }

        public void Flow(MonoChromatorMeasure_FlowPara Para)
        {
            Hardware.CornerstoneB.IsFlowing = true;

            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            Task_AutoExpTime AutoExpTime = null;
            AutoExpTime_FlowPara AutoExpTime_Para = new AutoExpTime_FlowPara();

            double AreaMean = 0.0;

            string OutputFolder = $@"D:\OMS\MonoChromator Measure\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            string RawDataPath = $@"{OutputFolder}\RawData.csv";

            string[] XYZ_Ary = { "X", "Y", "Z" };
            string[] ND_Ary = { "12.5%", "1.56%", "0.25%", "100%" };


            List<string> Filter_List = new List<string>();

            bool[] FilterXYZ_Enable = { Para.XyzFilter_Enable_X, Para.XyzFilter_Enable_Y, Para.XyzFilter_Enable_Z };
            bool[] FilterND_Enable = { Para.NdFilter_Enable_12_5, Para.NdFilter_Enable_1_56, Para.NdFilter_Enable_0_25, Para.NdFilter_Enable_100 };

            for (int xyz = 0; xyz < FilterXYZ_Enable.Length; xyz++)
                for (int nd = 0; nd < FilterND_Enable.Length; nd++)
                {
                    if (FilterXYZ_Enable[xyz] && FilterND_Enable[nd])
                    {
                        Filter_List.Add($"{xyz}_{nd}");
                    }
                }

            List<double> WaveLength_List = new List<double>();
            for (double i = Para.WaveLength_Start; i < Para.WaveLength_End; i += Para.WaveLength_Gap)
            {
                WaveLength_List.Add(i);
            }
            WaveLength_List.Add(Para.WaveLength_End);
            WaveLength_List = WaveLength_List.Distinct().ToList();

            int Filter_Idx = 0;
            int WaveLength_Idx = 0;

            int Target_XYZ = 0;
            int Target_ND = 0;
            double Target_WaveLength = 0;

            int Init_RetryCnt = 0;
            int GoWave_RetryCnt = 0;
            TimeManager GoWave_Timeout = new TimeManager();

            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check FW
                            bool Check_FW = true;
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter == null || GlobalVar.Device.NdFilter == null)
                                        {
                                            SaveLog($"Check Device Fail : FW is null", true);
                                            Check_FW = false;
                                            break;
                                        }

                                        if (!GlobalVar.Device.XyzFilter.is_FW_Work || !GlobalVar.Device.NdFilter.is_FW_Work)
                                        {
                                            SaveLog($"Check Device Fail : FW is not Work", true);
                                            Check_FW = false;
                                            break;
                                        }
                                    }
                                    break;
                            }

                            Check &= Check_FW;

                            //Check Grabber
                            bool Check_Grabber = (this.Hardware.Grabber != null);
                            if (!Check_Grabber)
                            {
                                SaveLog($"Check Device Fail :  Grabber is null", true);
                            }

                            Check &= Check_Grabber;

                            if (Check)
                            {
                                SaveLog($"Check Devie OK");
                                NowStep = FlowStep.Check_Parameter;
                            }
                            else
                            {
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.Check_Parameter:
                        {
                            bool Check = true;

                            //Check System
                            bool Check_System = (GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST");
                            if (!Check_System)
                            {
                                SaveLog($"Check Parameter Fail : MIL System = M_SYSTEM_HOST", true);
                            }
                            Check &= Check_System;

                            //Check Filter List
                            bool Check_Filter_List = (Filter_List.Count > 0);
                            if (!Check_Filter_List)
                            {
                                SaveLog($"Check Parameter Fail : Filter List Count = 0", true);
                            }
                            Check &= Check_Filter_List;

                            //Check LT List
                            bool Check_WaveLength_List = (WaveLength_List.Count > 0);
                            if (!Check_WaveLength_List)
                            {
                                SaveLog($"Check Parameter Fail : WaveLength List Count = 0", true);
                            }
                            Check &= Check_WaveLength_List;

                            if (Check)
                            {
                                SaveLog($"Check Parameter OK");
                                NowStep = FlowStep.Init;
                            }
                            else
                            {
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.Init:
                        {
                            SaveLog($"Flow Start");

                            Directory.CreateDirectory(OutputFolder);

                            List<string> ColHeader = Filter_List.ToList();
                            List<string> RowHeader = new List<string>();

                            foreach (double WaveLength in WaveLength_List)
                            {
                                RowHeader.Add($"{WaveLength} nm");
                            }

                            bool Rtn = this.BuildRawData(RawDataPath, ColHeader, RowHeader);

                            if (!Rtn)
                            {
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            NowStep = FlowStep.MonoChromator_Init;
                        }
                        break;

                    case FlowStep.MonoChromator_Init:
                        {
                            bool Rtn = true;

                            Rtn = Hardware.CornerstoneB.SET_SHUTTER(true);
                            if (Rtn)
                            {
                                SaveLog($"[MonoChromator] Set Sutter On");
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Set Sutter Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            Rtn = Hardware.CornerstoneB.SET_GRATing(Para.MonoChromator_Grating);
                            if (Rtn)
                            {
                                SaveLog($"[MonoChromator] Set Grating : {Para.MonoChromator_Grating}");
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Set Grating : {Para.MonoChromator_Grating} , Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            Rtn = Hardware.CornerstoneB.SET_FILTER(Para.MonoChromator_Filter);
                            if (Rtn)
                            {
                                SaveLog($"[MonoChromator] Set Filter : {Para.MonoChromator_Filter}");
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Set Filter : {Para.MonoChromator_Filter} , Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            NowStep = FlowStep.Check_MonoChromator_Init;
                        }
                        break;

                    case FlowStep.Check_MonoChromator_Init:
                        {
                            bool Rtn = true;

                            bool Check_Shuuter = true;
                            bool Shutter_OnOff = false;
                            Rtn = Hardware.CornerstoneB.GET_SHUTTER(ref Shutter_OnOff);
                            if (Rtn)
                            {
                                Check_Shuuter = (Shutter_OnOff == true);

                                if (!Check_Shuuter)
                                {
                                    SaveLog($"[MonoChromator] Check Sutter On Fail , Target = ON , Now = {Shutter_OnOff}", true);
                                }
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Get Sutter Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            bool Check_Grating = true;
                            string GratingMsg = "";
                            Rtn = Hardware.CornerstoneB.GET_GRATing(ref GratingMsg);
                            if (Rtn)
                            {
                                string[] Msgs = GratingMsg.Split(',');
                                int GratingIdx = Convert.ToInt32(Msgs[0]);

                                Check_Grating = (GratingIdx == Para.MonoChromator_Grating);
                                if (!Check_Grating)
                                {
                                    SaveLog($"[MonoChromator] Check Grating Fail , Target = {Para.MonoChromator_Grating} , Now = {GratingIdx}", true);
                                }
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Get Grating Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            bool Check_Filter = true;
                            int FilterIdx = 0;
                            Rtn = Hardware.CornerstoneB.GET_FILTER_SET(ref FilterIdx);
                            if (Rtn)
                            {
                                Check_Filter = (FilterIdx == Para.MonoChromator_Filter);
                                if (!Check_Filter)
                                {
                                    SaveLog($"[MonoChromator] Check Filter Fail , Target = {Para.MonoChromator_Filter} , Now = {FilterIdx}", true);
                                }
                            }
                            else
                            {
                                SaveLog($"[MonoChromator] Get Filter Function Error", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            bool CheckAll = Check_Shuuter && Check_Grating && Check_Filter;

                            if (CheckAll)
                            {
                                NowStep = FlowStep.Filter_Change;
                            }
                            else
                            {
                                Init_RetryCnt++;

                                if (Init_RetryCnt <= 3)
                                {
                                    SaveLog($"[MonoChromator] Init Retry : {Init_RetryCnt}/3");
                                    NowStep = FlowStep.MonoChromator_Init;
                                }
                                else
                                {
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.Filter_Change:
                        {
                            WaveLength_Idx = 0;
                            string[] Filter = Filter_List[Filter_Idx].Split('_');

                            Target_XYZ = Convert.ToInt16(Filter[0]);
                            Target_ND = Convert.ToInt16(Filter[1]);

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        GlobalVar.Device.XyzFilter.ChangeWheel(Target_XYZ);
                                        GlobalVar.Device.NdFilter.ChangeWheel(Target_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Hardware.Motor.Move(-1, -1, Target_XYZ, Target_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            SaveLog($"Filter Change : XYZ = {Target_XYZ} , ND = {Target_ND}");

                            NowStep = FlowStep.Check_Filter_Change;
                        }
                        break;

                    case FlowStep.Check_Filter_Change:
                        {
                            bool XyzReady = false;
                            bool NdReady = false;

                            // Change Wheel Ready
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        XyzReady = GlobalVar.Device.XyzFilter.IsReady;
                                        NdReady = GlobalVar.Device.NdFilter.IsReady;
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (!this.Hardware.Motor.IsMoveProc())
                                        {
                                            XyzReady = (this.Hardware.Motor.MoveFlowStatus() == UnitStatus.Finish && this.Hardware.Motor.MoveStatus_FW1() == UnitStatus.Finish);
                                            NdReady = (this.Hardware.Motor.MoveFlowStatus() == UnitStatus.Finish && this.Hardware.Motor.MoveStatus_FW2() == UnitStatus.Finish);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        XyzReady = true;
                                        NdReady = true;
                                    }
                                    break;
                            }

                            if (XyzReady && NdReady)
                            {
                                SaveLog($"Filter Change Finish");
                                NowStep = FlowStep.WaveLength_Change;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Filter Change Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.WaveLength_Change:
                        {
                            Target_WaveLength = WaveLength_List[WaveLength_Idx];
                            bool Rtn = Hardware.CornerstoneB.SET_GOWAVE(Target_WaveLength);

                            if (Rtn)
                            {
                                SaveLog($"WaveLength Change : {Target_WaveLength}");
                                NowStep = FlowStep.Check_WaveLength_Change;

                                GoWave_Timeout = new TimeManager(5000);
                            }
                            else
                            {
                                SaveLog($"Set WaveLength Function Error", true);
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.Check_WaveLength_Change:
                        {
                            double Tolerance = Para.WaveLength_Tolerance_nm;

                            double MaxValue = Target_WaveLength + Tolerance;
                            double MinValue = Target_WaveLength - Tolerance;

                            double WaveLentgh_Current = 0;

                            bool Rtn = Hardware.CornerstoneB.GET_WAVE(ref WaveLentgh_Current);

                            if (Rtn)
                            {
                                bool Match = (WaveLentgh_Current >= MinValue && WaveLentgh_Current <= MaxValue);

                                if (Match)
                                {
                                    SaveLog($"Check WaveLength Finish : {WaveLentgh_Current} ({Target_WaveLength}±{Tolerance})");
                                    Thread.Sleep(Para.WaveLength_Delay);
                                    NowStep = FlowStep.AutoExposureTime;
                                }
                                else
                                {
                                    if (GoWave_Timeout.IsTimeOut())
                                    {
                                        GoWave_RetryCnt++;

                                        if (GoWave_RetryCnt <= 3)
                                        {
                                            SaveLog($"Check WaveLength Timeout , Retry : {GoWave_RetryCnt}/3");
                                            NowStep = FlowStep.WaveLength_Change;
                                        }
                                        else
                                        {
                                            SaveLog($"Check WaveLength Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                            }
                            else
                            {
                                SaveLog($"Get WaveLength Function Error", true);
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.AutoExposureTime:
                        {
                            AutoExpTime = new Task_AutoExpTime(Para.AutoExpTime_Para, Target_XYZ, this.Info, this.Hardware.Grabber);
                            AutoExpTime.Start(Target_XYZ);

                            string XYZ_Msg = XYZ_Ary[Target_XYZ];
                            string ND_Msg = ND_Ary[Target_ND];
                            string Remark = $"{XYZ_Msg}_{ND_Msg}_{Target_WaveLength}nm";

                            SaveLog($"[{Remark}] Auto ExposureTime Start");

                            Thread.Sleep(100);

                            NowStep = FlowStep.Check_AutoExposureTime;
                        }
                        break;

                    case FlowStep.Check_AutoExposureTime:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                string XYZ_Msg = XYZ_Ary[Target_XYZ];
                                string ND_Msg = ND_Ary[Target_ND];
                                string Remark = $"{XYZ_Msg}_{ND_Msg}_{Target_WaveLength}nm";

                                switch (AutoExpTime.State)
                                {
                                    case Task_AutoExpTime.EnState.Finish:
                                        {
                                            SaveLog($"[{Remark}] Auto ExposureTime Finish");
                                            NowStep = FlowStep.Capture;
                                        }
                                        break;

                                    case Task_AutoExpTime.EnState.Warning:
                                    case Task_AutoExpTime.EnState.Alarm:
                                        {
                                            SaveLog($"[{Remark}] Auto ExposureTime Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Capture:
                        {
                            string XYZ_Msg = XYZ_Ary[Target_XYZ];
                            string ND_Msg = ND_Ary[Target_ND];
                            string Remark = $"{XYZ_Msg}_{ND_Msg}_{Target_WaveLength}nm";

                            double ExpTime = AutoExpTime.Result.ExpTime;
                            this.Hardware.Grabber.SetExposureTime(ExpTime);
                            this.Hardware.Grabber.Grab();

                            SaveLog($"[{Remark}] Capture,  ExpTime = {ExpTime}");

                            string ImgPath = $@"{OutputFolder}\{Remark}.tif";

                            MIL.MbufSave(ImgPath, this.Hardware.Grabber.grabImage);


                            NowStep = FlowStep.GetImageData;
                        }
                        break;

                    case FlowStep.GetImageData:
                        {
                            string XYZ_Msg = XYZ_Ary[Target_XYZ];
                            string ND_Msg = ND_Ary[Target_ND];
                            string Remark = $"{XYZ_Msg}_{ND_Msg}_{Target_WaveLength}nm";

                            MIL_INT SizeX = MIL.MbufInquire(this.Hardware.Grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
                            MIL_INT SizeY = MIL.MbufInquire(this.Hardware.Grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

                            int CenterX = ((int)SizeX / 2);
                            int CenterY = ((int)SizeY / 2);

                            switch (Para.Region_Type)
                            {
                                case EnumRegionMethod.Circle:
                                    {
                                        int Radius = Para.Region_CircleRadius;

                                        CircleRegionInfo Info = new CircleRegionInfo();
                                        Info.CenterX = CenterX;
                                        Info.CenterY = CenterY;
                                        Info.Radius = Radius;

                                        AreaMean = MyMilIp.StatCircleMean(this.Hardware.Grabber.grabImage, Info);
                                        SaveLog($"[{Remark}] Cal CenterGray (Circle) , Radius = {Radius} , Mean = {AreaMean}");
                                    }
                                    break;

                                case EnumRegionMethod.Rect:
                                    {
                                        int W = Para.Region_RectSize.Width;
                                        int H = Para.Region_RectSize.Height;

                                        RectRegionInfo Info = new RectRegionInfo();
                                        Info.StartX = CenterX - (W / 2);
                                        Info.StartY = CenterY - (H / 2);
                                        Info.Width = W;
                                        Info.Height = H;

                                        AreaMean = MyMilIp.StatRectMean(this.Hardware.Grabber.grabImage, Info);
                                        SaveLog($"[{Remark}] Cal CenterGray (Rect) , Size = ({W},{H}) , Mean = {AreaMean}");
                                    }
                                    break;
                            }

                            //TODO Record

                            bool Rtn = this.UpdateRawData(RawDataPath, Filter_Idx, WaveLength_Idx, AreaMean);

                            if (!Rtn)
                            {
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            NowStep = FlowStep.Check_WaveLength_Idx;
                        }
                        break;

                    case FlowStep.Check_WaveLength_Idx:
                        {
                            WaveLength_Idx++;

                            if (WaveLength_Idx < WaveLength_List.Count())
                            {
                                NowStep = FlowStep.WaveLength_Change;
                            }
                            else
                            {
                                NowStep = FlowStep.Check_Filter_Idx;
                            }
                        }
                        break;

                    case FlowStep.Check_Filter_Idx:
                        {
                            Filter_Idx++;

                            if (Filter_Idx < Filter_List.Count())
                            {
                                NowStep = FlowStep.Filter_Change;
                            }
                            else
                            {
                                NowStep = FlowStep.Finish;
                            }
                        }
                        break;

                    case FlowStep.Finish:
                        {
                            SaveLog($"Flow Finish");
                            FlowRun = false;
                        }
                        break;

                    case FlowStep.Alarm:
                        {
                            SaveLog($"Flow Alarm", true);
                            FlowRun = false;
                        }
                        break;
                }

                MonitorStep?.Invoke($"[{(int)NowStep}] {NowStep}");

                Thread.Sleep(10);
            }

            Hardware.CornerstoneB.IsFlowing = false;

        }


        public enum FlowStep
        {
            Check_Device = 0,
            Check_Parameter,
            Init,
            MonoChromator_Init,
            Check_MonoChromator_Init,

            Filter_Change,
            Check_Filter_Change,
            WaveLength_Change,
            Check_WaveLength_Change,
            AutoExposureTime,
            Check_AutoExposureTime,

            Capture,
            GetImageData,

            Check_WaveLength_Idx,
            Check_Filter_Idx,

            Finish,
            Alarm,
        }

        #region Method

        public bool BuildRawData(string SavePath, List<string> ColHeader, List<string> RowHeader)
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    List<string> Lines = new List<string>();
                    string ColHeaderMsg = "";

                    foreach (string Col in ColHeader)
                    {
                        ColHeaderMsg += $",{Col}";
                    }

                    Lines.Add(ColHeaderMsg);

                    foreach (string Row in RowHeader)
                    {
                        string LineMsg = Row;

                        foreach (string Col in ColHeader)
                        {
                            LineMsg += $",";
                        }

                        Lines.Add(LineMsg);
                    }

                    File.WriteAllLines(SavePath, Lines);
                }

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Build RawData Fail : {ex.Message}");
                return false;
            }
        }


        public bool UpdateRawData(string SavePath, int ColIndex, int RowIndex, double Value)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var lines = File.ReadAllLines(SavePath).ToList();

                    var rowData = lines[RowIndex + 1].Split(',');
                    rowData[ColIndex + 1] = $"{Value}"; // 更新值
                    lines[RowIndex + 1] = string.Join(",", rowData);

                    File.WriteAllLines(SavePath, lines);
                }

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write RawData Fail : {ex.Message}");
                return false;
            }
        }



        #endregion
    }

    public class MonoChromatorMeasure_FlowPara
    {
        [Category("A. 初始化"), DisplayName("01. 單光儀 Grating")]
        public int MonoChromator_Grating { get; set; } = 0;

        [Category("A. 初始化"), DisplayName("02. 單光儀 Filter")]
        public int MonoChromator_Filter { get; set; } = 0;



        [Category("B. 轉盤組合"), DisplayName("01. XYZ轉盤 : X")]
        public bool XyzFilter_Enable_X { get; set; } = true;

        [Category("B. 轉盤組合"), DisplayName("02. XYZ轉盤 : Y")]
        public bool XyzFilter_Enable_Y { get; set; } = true;

        [Category("B. 轉盤組合"), DisplayName("03. XYZ轉盤 : Z")]
        public bool XyzFilter_Enable_Z { get; set; } = true;

        [Category("B. 轉盤組合"), DisplayName("11. ND轉盤 : 12.5%")]
        public bool NdFilter_Enable_12_5 { get; set; } = false;

        [Category("B. 轉盤組合"), DisplayName("12. ND轉盤 : 1.56%")]
        public bool NdFilter_Enable_1_56 { get; set; } = false;

        [Category("B. 轉盤組合"), DisplayName("13. ND轉盤 : 0.25%")]
        public bool NdFilter_Enable_0_25 { get; set; } = false;

        [Category("B. 轉盤組合"), DisplayName("14. ND轉盤 : 100%")]
        public bool NdFilter_Enable_100 { get; set; } = true;

        [Category("C. 波長組合"), DisplayName("01. 波長起始 (nm)")]
        public double WaveLength_Start { get; set; } = 400;

        [Category("C. 波長組合"), DisplayName("02. 波長結束 (nm)")]
        public double WaveLength_End { get; set; } = 700;

        [Category("C. 波長組合"), DisplayName("03. 波長間距 (nm)")]
        public double WaveLength_Gap { get; set; } = 5;

        [Category("C. 波長組合"), DisplayName("04. 波長容許值 (nm)")]
        public double WaveLength_Tolerance_nm { get; set; } = 0.1;

        [Category("C. 波長組合"), DisplayName("05. 波長切換Delay (ms)")]
        public int WaveLength_Delay { get; set; } = 1000;


        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("D. 拍照"), DisplayName("01. 自動曝光參數")]
        public AutoExpTime_FlowPara AutoExpTime_Para { get; set; } = new AutoExpTime_FlowPara();

        [Category("D. 拍照"), DisplayName("02. 取值形狀")]
        public EnumRegionMethod Region_Type { get; set; } = EnumRegionMethod.Circle;

        [Category("D. 拍照"), DisplayName("03. 圓形半徑 (FOV中間)")]
        public int Region_CircleRadius { get; set; } = 10;

        [Category("D. 拍照"), DisplayName("04. 矩形長寬 (FOV中間)")]
        public Size Region_RectSize { get; set; } = new Size(100, 100);



        public void Clone(MonoChromatorMeasure_FlowPara Source)
        {
            //A
            this.MonoChromator_Grating = Source.MonoChromator_Grating;
            this.MonoChromator_Filter = Source.MonoChromator_Filter;

            //B
            this.XyzFilter_Enable_X = Source.XyzFilter_Enable_X;
            this.XyzFilter_Enable_Y = Source.XyzFilter_Enable_Y;
            this.XyzFilter_Enable_Z = Source.XyzFilter_Enable_Z;
            this.NdFilter_Enable_12_5 = Source.NdFilter_Enable_12_5;
            this.NdFilter_Enable_1_56 = Source.NdFilter_Enable_1_56;
            this.NdFilter_Enable_0_25 = Source.NdFilter_Enable_0_25;
            this.NdFilter_Enable_100 = Source.NdFilter_Enable_100;

            //C
            this.WaveLength_Start = Source.WaveLength_Start;
            this.WaveLength_End = Source.WaveLength_End;
            this.WaveLength_Gap = Source.WaveLength_Gap;
            this.WaveLength_Tolerance_nm = Source.WaveLength_Tolerance_nm;
            this.WaveLength_Delay = Source.WaveLength_Delay;

            //D          
            this.AutoExpTime_Para.Clone(Source.AutoExpTime_Para);
            this.Region_Type = Source.Region_Type;
            this.Region_CircleRadius = Source.Region_CircleRadius;
            this.Region_RectSize = Source.Region_RectSize;
        }

        public static void WriteXML(MonoChromatorMeasure_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(MonoChromatorMeasure_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static MonoChromatorMeasure_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                MonoChromatorMeasure_FlowPara m;

                serializer = new XmlSerializer(typeof(MonoChromatorMeasure_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (MonoChromatorMeasure_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }
    }
}
