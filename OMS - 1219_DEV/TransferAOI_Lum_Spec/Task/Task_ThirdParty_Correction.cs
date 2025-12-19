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
    public class Task_ThirdParty_Correction
    {
        private cls29MSetting RD;
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;

        //Task
        private Task_BaseAlign BaseAlign = null;

        //Flow
        private bool FlowRun = false;
        private UnitStatus CheckForm_IsCheck = UnitStatus.Idle;
        private double CheckForm_Lum = 0;
        private double CheckForm_Cx = 0;
        private double CheckForm_Cy = 0;

        private LumCorrectionPara GlobalCorrData => GlobalVar.CorrData;

        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_ThirdParty_Correction(cls29MSetting param, InfoManager Info, HardwareUnit Hardware)
        {
            this.RD = param;
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

        public void Start()
        {
            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            Thread mThread = new Thread(() => Flow());
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

        public void Flow()
        {
            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            Task_AutoExpTime AutoExpTime = null;
            AutoExpTime_FlowPara AutoExpTime_Para = new AutoExpTime_FlowPara();

            double AreaMean = 0.0;

            int ND_Gray_Idx = 0;
            int LT_Idx = 0;
            int XYZ_Idx = 0;

            List<CorrectionData> CorrData = new List<CorrectionData>();

            CorrectionData NowCorrData = new CorrectionData();

            string OutputFolder = $@"D:\OMS\Third Party\Spectral Correction\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            string M_ND = "";
            string M_Gray = "";
            double M_Light = 0;
            string M_XYZ = "";

            string[] XYZ_Ary = { "R", "G", "B" };

            while (FlowRun)
            {
                switch (NowStep)
                {
                    #region Check & Init 

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
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Hardware.Grabber != null);
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

                            //Check LT List
                            bool Check_LT_List = (GlobalCorrData.Para_List.Count > 0);
                            if (!Check_LT_List)
                            {
                                SaveLog($"Check Parameter Fail : Light List Count = 0", true);
                            }
                            Check &= Check_LT_List;

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

                            NowStep = FlowStep.FW_ND_Move;
                        }
                        break;

                    #endregion

                    case FlowStep.FW_ND_Move:
                        {
                            int FW_ND = (int)GlobalCorrData.Para_List[ND_Gray_Idx].ND;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        GlobalVar.Device.NdFilter.ChangeWheel(FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Hardware.Motor.Move(-1, -1, -1, FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            // 獲取枚舉成員的 FieldInfo
                            FieldInfo field = GlobalCorrData.Para_List[ND_Gray_Idx].ND.GetType().GetField(GlobalCorrData.Para_List[ND_Gray_Idx].ND.ToString());
                            // 獲取 DescriptionAttribute
                            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                            M_ND = attribute.Description;
                            M_Gray = $"{GlobalCorrData.Para_List[ND_Gray_Idx].TargetGray}";

                            string Remark = $"{M_ND}_{M_Gray}";
                            SaveLog($"[{Remark}] FW-ND Move to {FW_ND}");
                            NowStep = FlowStep.Check_FW_ND_InPos;
                        }
                        break;

                    case FlowStep.Check_FW_ND_InPos:
                        {
                            bool NdReady = false;

                            // Change Wheel Ready
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
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
                                            NdReady = (this.Hardware.Motor.MoveFlowStatus() == UnitStatus.Finish && this.Hardware.Motor.MoveStatus_FW2() == UnitStatus.Finish);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        NdReady = true;
                                    }
                                    break;
                            }

                            string Remark = $"{M_ND}_{M_Gray}";

                            if (NdReady)
                            {
                                SaveLog($"[{Remark}] FW-ND Move Finish");

                                NowStep = FlowStep.Show_CheckForm;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"[{Remark}] FW-ND Move Timeout", true);

                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    #region 確認視窗 : 確認亮度

                    case FlowStep.Show_CheckForm:
                        {
                            if (GlobalCorrData.Para_List.Count == 0)
                            {
                                SaveLog($"CorrData Count == 0", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            if (GlobalCorrData.Para_List[ND_Gray_Idx].Light_List.Count == 0)
                            {
                                SaveLog($"Light Count == 0", true);
                                NowStep = FlowStep.Alarm;
                                break;
                            }

                            M_Light = GlobalCorrData.Para_List[ND_Gray_Idx].Light_List[LT_Idx];

                            CheckForm_IsCheck = UnitStatus.Running;
                            CheckForm_Lum = 0;
                            CheckForm_Cx = 0;
                            CheckForm_Cy = 0;

                            Form mainForm = Application.OpenForms["frmMain"];
                            mainForm.BeginInvoke((MethodInvoker)delegate {
                                InputCheckForm CheckForm = new InputCheckForm();

                                CheckForm.isCheck -= CheckForm_isCheck;
                                CheckForm.isCheck += CheckForm_isCheck;

                                CheckForm.SetPare($"請確認亮度 = {M_Light} nits\r\n請輸入數值");

                                CheckForm.ShowDialog();
                                CheckForm.BringToFront();
                            }
                            );

                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit";

                            SaveLog($"[{Remark}] Show Check Form");

                            NowStep = FlowStep.Check_NextStep;
                        }
                        break;

                    case FlowStep.Check_NextStep:
                        {
                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit";

                            switch (CheckForm_IsCheck)
                            {
                                case UnitStatus.Finish:
                                    {
                                        double Y = CheckForm_Lum;
                                        double Cx = CheckForm_Cx;
                                        double Cy = CheckForm_Cy;

                                        NowCorrData.std[0] = Cx * (Y / Cy);
                                        NowCorrData.std[1] = Y;
                                        NowCorrData.std[2] = (1 - Cx - Cy) * (Y / Cy);

                                        SaveLog($"[{Remark}] Check Form OK");
                                        NowStep = FlowStep.FW_XYZ_Move;
                                    }
                                    break;

                                case UnitStatus.Idle:
                                    {
                                        SaveLog($"[{Remark}] Check Form NG", true);
                                        NowStep = FlowStep.Alarm;
                                    }
                                    break;
                            }
                        }
                        break;

                    #endregion

                    #region 使用AIC拍照

                    case FlowStep.FW_XYZ_Move:
                        {
                            int FW_XYZ = XYZ_Idx;
                            M_XYZ = XYZ_Ary[FW_XYZ];

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        GlobalVar.Device.XyzFilter.ChangeWheel(FW_XYZ);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Hardware.Motor.Move(-1, -1, FW_XYZ, -1);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";

                            SaveLog($"[{Remark}] FW-XYZ Move to {FW_XYZ}");

                            NowStep = FlowStep.Check_FW_XYZ_InPos;
                        }
                        break;

                    case FlowStep.Check_FW_XYZ_InPos:
                        {
                            bool XyzReady = false;

                            // Change Wheel Ready
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        XyzReady = GlobalVar.Device.XyzFilter.IsReady;
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
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        XyzReady = true;
                                    }
                                    break;
                            }

                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";

                            if (XyzReady)
                            {
                                SaveLog($"[{Remark}] FW-XYZ Move Finish");
                                NowStep = FlowStep.AutoExposureTime;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"[{Remark}] FW-XYZ Move Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.AutoExposureTime:
                        {
                            RD.AutoExposureTargetGray[0] = GlobalCorrData.Para_List[ND_Gray_Idx].TargetGray;
                            RD.AutoExposureTargetGray[1] = GlobalCorrData.Para_List[ND_Gray_Idx].TargetGray;
                            RD.AutoExposureTargetGray[2] = GlobalCorrData.Para_List[ND_Gray_Idx].TargetGray;

                            RD.AutoExposureRoiX = RD.ThirdPartyRoiX;
                            RD.AutoExposureRoiY = RD.ThirdPartyRoiY;
                            RD.AutoExposureRoiWidth = RD.ThirdPartyRoiWidth;
                            RD.AutoExposureRoiHeight = RD.ThirdPartyRoiHeight;

                            AutoExpTime = new Task_AutoExpTime(RD, XYZ_Idx, this.Info, this.Hardware.Grabber);
                            AutoExpTime.Start(XYZ_Idx);

                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";
                            SaveLog($"[{Remark}] Auto ExposureTime Start");

                            Thread.Sleep(100);
                            NowStep = FlowStep.Check_AutoExposureTime;
                        }
                        break;

                    case FlowStep.Check_AutoExposureTime:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";

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
                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";

                            double ExpTime = AutoExpTime.Result.ExpTime;
                            this.Hardware.Grabber.SetExposureTime(ExpTime);
                            this.Hardware.Grabber.Grab();
                            SaveLog($"[{Remark}] Capture,  ExpTime = {ExpTime}");

                            Thread.Sleep(100);
                            int ND = (int)GlobalCorrData.Para_List[ND_Gray_Idx].ND;
                            double Brightness = M_Light;

                            string LT_Remark = $"ND{ND}_{Brightness}nit";
                            string SaveFolder = $@"{OutputFolder}\{LT_Remark}\";

                            Directory.CreateDirectory(SaveFolder);
                            string ImgPath = $@"{SaveFolder}\{(FW_XYZ_Remark)XYZ_Idx}.tif";
                            MIL.MbufSave(ImgPath, this.Hardware.Grabber.grabImage);

                            SaveLog($"[{Remark}] Save Image : {ImgPath}");

                            NowCorrData.exp[XYZ_Idx] = ExpTime;

                            NowStep = FlowStep.Cal_CenterGray;
                        }
                        break;

                    case FlowStep.Cal_CenterGray:
                        {
                            RectRegionInfo Info = new RectRegionInfo();
                            Info.StartX = RD.ThirdPartyRoiX;
                            Info.StartY = RD.ThirdPartyRoiY;
                            Info.Width = RD.ThirdPartyRoiWidth;
                            Info.Height = RD.ThirdPartyRoiHeight;

                            AreaMean = MyMilIp.StatRectMean(this.Hardware.Grabber.grabImage, Info);

                            string Remark = $"{M_ND}_{M_Gray}_{M_Light}nit_{M_XYZ}";
                            SaveLog($"[{Remark}] Cal CenterGray, Mean = {AreaMean}");

                            NowCorrData.mean[XYZ_Idx] = AreaMean;

                            NowStep = FlowStep.AddCorrData;
                        }
                        break;

                    case FlowStep.AddCorrData:
                        {
                            NowCorrData.norm[XYZ_Idx] = (NowCorrData.mean[XYZ_Idx] / (NowCorrData.exp[XYZ_Idx] / 1000000));
                            NowStep = FlowStep.Check_XYZ_Idx;
                        }
                        break;

                    #endregion

                    #region 確認下一RUN(亮度)

                    case FlowStep.Check_XYZ_Idx:
                        {
                            XYZ_Idx++;

                            if (XYZ_Idx < 3)
                            {
                                NowStep = FlowStep.FW_XYZ_Move;
                            }
                            else
                            {
                                CorrData.Add(NowCorrData);

                                NowStep = FlowStep.Check_LT_Idx;
                            }
                        }
                        break;

                    case FlowStep.Check_LT_Idx:
                        {
                            NowCorrData = new CorrectionData();

                            XYZ_Idx = 0;
                            LT_Idx++;

                            if (LT_Idx < GlobalCorrData.Para_List[ND_Gray_Idx].Light_List.Count)
                            {
                                NowStep = FlowStep.Show_CheckForm;
                            }
                            else
                            {
                                NowStep = FlowStep.RecordCorrCsv;
                            }
                        }
                        break;

                    case FlowStep.RecordCorrCsv:
                        {
                            CorrectionResult CorrResult = CalCorrResult(ref CorrData, GlobalCorrData.Para_List[ND_Gray_Idx]);

                            GlobalCorrData.Para_List[ND_Gray_Idx].X_Coefficient.Alpha = CorrResult.alpha_avg[0];
                            GlobalCorrData.Para_List[ND_Gray_Idx].Y_Coefficient.Alpha = CorrResult.alpha_avg[1];
                            GlobalCorrData.Para_List[ND_Gray_Idx].Z_Coefficient.Alpha = CorrResult.alpha_avg[2];

                            GlobalCorrData.Create(GlobalVar.CorrData, GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml");

                            string Remark = $"{M_ND}_{M_Gray}";
                            WriteCorrData($@"{OutputFolder}\{Remark}.csv", Remark, CorrData, CorrResult);

                            NowStep = FlowStep.Check_ND_Gray_Idx;


                        }
                        break;

                    case FlowStep.Check_ND_Gray_Idx:
                        {
                            CorrData = new List<CorrectionData>();

                            XYZ_Idx = 0;
                            LT_Idx = 0;
                            ND_Gray_Idx++;

                            if (ND_Gray_Idx < GlobalCorrData.Para_List.Count)
                            {
                                NowStep = FlowStep.FW_ND_Move;
                            }
                            else
                            {
                                NowStep = FlowStep.Finish;
                            }
                        }
                        break;



                    #endregion



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
            }
        }

        private void CheckForm_isCheck(bool Check, double Lum, double Cx, double Cy)
        {
            CheckForm_IsCheck = (Check) ? UnitStatus.Finish : UnitStatus.Idle;

            CheckForm_Lum = Lum;
            CheckForm_Cx = Cx;
            CheckForm_Cy = Cy;
        }

        public enum FlowStep
        {
            //Check & Init 
            Check_Device = 0,
            Check_Parameter,
            Init,

            FW_ND_Move,
            Check_FW_ND_InPos,

            //確認視窗 : 調整光源
            Show_CheckForm,
            Check_NextStep,

            FW_XYZ_Move,
            Check_FW_XYZ_InPos,
            AutoExposureTime,
            Check_AutoExposureTime,
            Capture,
            Cal_CenterGray,
            AddCorrData,

            //確認下一RUN(亮度)
            Check_XYZ_Idx = 600,
            Check_LT_Idx,
            RecordCorrCsv,
            Check_ND_Gray_Idx,


            Finish,
            Alarm,
        }

        #region Method

        private bool WriteRowData(string SavePath, RecordPara Para)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Target Brightness (nit)");
                    TitleList.Add("Real Brightness (nit)");
                    TitleList.Add("Cx");
                    TitleList.Add("Cy");
                    TitleList.Add("Filter XYZ");
                    TitleList.Add("Filter ND");
                    TitleList.Add("ExposureTime");
                    TitleList.Add("Area Mean");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{Para.TristimulusY_Target}");
                InfoList.Add($"{Para.TristimulusY}");
                InfoList.Add($"{Para.Cx}");
                InfoList.Add($"{Para.Cy}");
                InfoList.Add($"{Para.FilterXYZ}");
                InfoList.Add($"{Para.FilterND}");
                InfoList.Add($"{Para.ExposureTime}");
                InfoList.Add($"{Para.AreaMean}");

                string Info = string.Join(",", InfoList);

                SB.Append(Info + "\r\n");

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write RowData Fail : {ex.Message}");
                return false;
            }
        }

        private bool WriteCorrData(string SavePath, string Remark, List<CorrectionData> Data, CorrectionResult Result)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add(Remark);
                    TitleList.Add("X_mean");
                    TitleList.Add("Y_mean");
                    TitleList.Add("Z_mean");
                    TitleList.Add("X_exp");
                    TitleList.Add("Y_exp");
                    TitleList.Add("Z_exp");
                    TitleList.Add("X_norm");
                    TitleList.Add("Y_norm");
                    TitleList.Add("Z_norm");
                    TitleList.Add("X_std");
                    TitleList.Add("Y_std");
                    TitleList.Add("Z_std");
                    TitleList.Add("X_linear");
                    TitleList.Add("Y_linear");
                    TitleList.Add("Z_linear");
                    TitleList.Add("Alpha_X");
                    TitleList.Add("Alpha_Y");
                    TitleList.Add("Alpha_Z");

                    string Title = string.Join(",", TitleList);
                    SB.Append(Title + "\r\n");
                }

                for (int i = 0; i < Data.Count; i++)
                {
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{i + 1}");

                    InfoList.Add($"{Data[i].mean[0]}");
                    InfoList.Add($"{Data[i].mean[1]}");
                    InfoList.Add($"{Data[i].mean[2]}");
                    InfoList.Add($"{Data[i].exp[0]}");
                    InfoList.Add($"{Data[i].exp[1]}");
                    InfoList.Add($"{Data[i].exp[2]}");
                    InfoList.Add($"{Data[i].norm[0]}");
                    InfoList.Add($"{Data[i].norm[1]}");
                    InfoList.Add($"{Data[i].norm[2]}");
                    InfoList.Add($"{Data[i].std[0]}");
                    InfoList.Add($"{Data[i].std[1]}");
                    InfoList.Add($"{Data[i].std[2]}");
                    InfoList.Add($"{Data[i].linear[0]}");
                    InfoList.Add($"{Data[i].linear[1]}");
                    InfoList.Add($"{Data[i].linear[2]}");
                    InfoList.Add($"{Data[i].alpha[0]}");
                    InfoList.Add($"{Data[i].alpha[1]}");
                    InfoList.Add($"{Data[i].alpha[2]}");

                    string Info = string.Join(",", InfoList);
                    SB.Append(Info + "\r\n");
                }

                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");

                SB.Append($"Alpha_X_avg,{Result.alpha_avg[0]}" + "\r\n");
                SB.Append($"Alpha_Y_avg,{Result.alpha_avg[1]}" + "\r\n");
                SB.Append($"Alpha_Z_avg,{Result.alpha_avg[2]}" + "\r\n");

                SB.Append("\r\n");

                SB.Append($"Alpha_X_std,{Result.alpha_std[0]}" + "\r\n");
                SB.Append($"Alpha_Y_std,{Result.alpha_std[1]}" + "\r\n");
                SB.Append($"Alpha_Z_std,{Result.alpha_std[2]}" + "\r\n");

                SB.Append("\r\n");

                SB.Append($"X Channel,{Result.alpha_std[0] < 0.05}" + "\r\n");
                SB.Append($"Y Channel,{Result.alpha_std[1] < 0.05}" + "\r\n");
                SB.Append($"Z Channel,{Result.alpha_std[2] < 0.05}" + "\r\n");

                //Info

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write RowData Fail : {ex.Message}");
                return false;
            }
        }

        private CorrectionResult CalCorrResult(ref List<CorrectionData> Para, CorrectionPara CorrPara)
        {
            CorrectionResult Result = new CorrectionResult();

            CorrectionCoefficient[] Coefficient = new CorrectionCoefficient[]
            {
                CorrPara.X_Coefficient,
                CorrPara.Y_Coefficient,
                CorrPara.Z_Coefficient,
            };

            for (int xyz = 0; xyz < 3; xyz++)
            {
                double Slope = Coefficient[xyz].Slope;
                double Intercept = Coefficient[xyz].Intercept;

                List<double> alphaList = new List<double>();

                for (int i = 0; i < Para.Count; i++)
                {
                    double normal = Para[i].norm[xyz];
                    Para[i].linear[xyz] = Slope * normal + Intercept;
                    Para[i].alpha[xyz] = Para[i].std[xyz] / Para[i].linear[xyz];

                    alphaList.Add(Para[i].alpha[xyz]);
                }

                Result.alpha_avg[xyz] = alphaList.Average();
                Result.alpha_std[xyz] = Calculate.CalculateStandard(alphaList) / alphaList.Average();
            }

            return Result;
        }

        public class RecordPara
        {
            public double TristimulusY_Target { get; set; } = 0.0;
            public double TristimulusY { get; set; } = 0.0;
            public double Cx { get; set; } = 0.0;
            public double Cy { get; set; } = 0.0;

            public string FilterXYZ { get; set; } = "";
            public string FilterND { get; set; } = "";
            public double ExposureTime { get; set; } = 0.0;
            public double AreaMean { get; set; } = 0.0;
        }

        public class CorrectionData
        {
            public double[] mean { get; set; } = new double[3];
            public double[] exp { get; set; } = new double[3];
            public double[] norm { get; set; } = new double[3];
            public double[] std { get; set; } = new double[3];
            public double[] linear { get; set; } = new double[3];
            public double[] alpha { get; set; } = new double[3];
        }

        public class CorrectionResult
        {
            public double[] alpha_avg { get; set; } = new double[3];
            public double[] alpha_std { get; set; } = new double[3];

        }

        #endregion
    }


}
