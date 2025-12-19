using BaseTool;
using CommonBase.Logger;
using FrameGrabber;
using HardwareManager;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OpticalMeasuringSystem
{
    public class Task_FFC_Create
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private IMotorControl Motor = null;
        private D65_Light_Ctrl D65 = null;
        private ClsUrRobotInterface Robot = null;
        private X_PLC_Ctrl PLC = null;
        private MilDigitizer Grabber;

        //Task
        private Task_BaseAlign BaseAlign = null;

        //Flow
        private bool FlowRun_Auto = false;
        private bool FlowRun_Single = false;
        private UnitStatus CheckForm_IsCheck = UnitStatus.Idle;

        public bool IsRun
        {
            get => FlowRun_Auto && FlowRun_Single;
        }

        private SingleFlowStep NowStep_FlatSingle = SingleFlowStep.Init;
        public SingleFlowStep SingleStep
        {
            get => NowStep_FlatSingle;
        }

        public Task_FFC_Create()
        {
        }

        public Task_FFC_Create(InfoManager Info, IMotorControl Motor, D65_Light_Ctrl D65, ClsUrRobotInterface Robot, X_PLC_Ctrl PLC, MilDigitizer Grabber)
        {
            this.Info = Info;
            this.Motor = Motor;
            this.D65 = D65;
            this.Robot = Robot;
            this.PLC = PLC;
            this.Grabber = Grabber;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            string LogTitle = "FFC";

            if (!isAlm)
            {
                this.Info.General($"[{LogTitle}] {Msg}");
            }
            else
            {
                this.Info.Error($"[{LogTitle}] {Msg}");
            }

            LogMsg?.Invoke(Msg);
        }

        public void Start(FFC_FlowPara SourcePara)
        {
            if (FlowRun_Auto || FlowRun_Single)
            {
                FlowRun_Auto = false;
                FlowRun_Single = false;
                Thread.Sleep(100);
            }

            FFC_FlowPara Para = (FFC_FlowPara)SourcePara;
            Thread mThread = new Thread(() => SingleFlow(Para));
            mThread.Start();

        }

        public void Stop()
        {
            this.FlowRun_Auto = false;
            this.FlowRun_Single = false;

            if (this.BaseAlign != null)
            {
                this.BaseAlign.Stop();
            }
        }

        #region SingleFlow

        public void SingleFlow(FFC_FlowPara Para)
        {
            TimeManager TM_FW_Move = new TimeManager();
            TimeManager TM_Light = new TimeManager();

            NowStep_FlatSingle = SingleFlowStep.Init;
            FlowRun_Single = true;

            Task_AutoExpTime AutoExpTime = null;
            AutoExpTime_FlowPara AutoExpTime_Para = new AutoExpTime_FlowPara();

            int LT_Idx = 0;
            int WD_Idx = 0;
            int Img_Idx = 0;

            Para.OutputFolder = $@"{Para.OutputFolder}\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            while (FlowRun_Single)
            {
                switch (NowStep_FlatSingle)
                {
                    case SingleFlowStep.Init:
                        {
                            SaveLog($"Flow Start");
                            NowStep_FlatSingle = SingleFlowStep.Check_Device;
                            //NowStep_FlatSingle = SingleFlowStep.Show_CheckForm;
                        }
                        break;

                    case SingleFlowStep.Check_Device:
                        {
                            //Check Motor
                            bool Check_Motor = true;
                            Check_Motor = (this.Motor != null);
                            if (Check_Motor)
                            {
                                Check_Motor = this.Motor.IsWork();
                                if (!Check_Motor)
                                {
                                    SaveLog($"Check Device Fail : Motor is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail : Motor is null", true);
                            }

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

                            //Check D65
                            bool Check_D65 = true;
                            Check_D65 = (this.D65 != null);
                            if (Check_D65)
                            {
                                Check_D65 = this.D65.IsConnect;
                                if (!Check_D65)
                                {
                                    SaveLog($"Check Device Fail : D65 Light is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  D65 Light is null", true);
                            }

                            //Check Robot
                            bool Check_Robot = true;
                            Check_Robot = (this.Robot != null);
                            if (Check_Robot)
                            {
                                Check_Robot = this.Robot.IsConnected();
                                if (!Check_Robot)
                                {
                                    SaveLog($"Check Device Fail : Robot is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  Robot is null", true);
                            }

                            //Check PLC
                            bool Check_PLC = true;
                            Check_PLC = (this.PLC != null);
                            if (Check_PLC)
                            {
                                Check_PLC = this.PLC.IsOpen;
                                if (!Check_PLC)
                                {
                                    SaveLog($"Check Device Fail : PLC is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  PLC is null", true);
                            }

                            //Check Grabber
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Grabber != null);
                            if (!Check_Grabber)
                            {
                                SaveLog($"Check Device Fail :  Grabber is null", true);
                            }

                            if (Check_Motor && Check_FW && Check_D65 && Check_Robot && Check_PLC && Check_Grabber)
                            {
                                SaveLog($"Check Devie OK");
                                NowStep_FlatSingle = SingleFlowStep.Check_Parameter;
                            }
                            else
                            {
                                NowStep_FlatSingle = SingleFlowStep.Alarm;
                            }
                        }
                        break;

                    case SingleFlowStep.Check_Parameter:
                        {
                            bool Check = true;

                            //Check System
                            bool Check_System = (GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST");
                            if (!Check_System)
                            {
                                Check = false;
                                SaveLog($"Check Parameter Fail : MIL System = M_SYSTEM_HOST", true);
                            }

                            //Check LT List
                            bool Check_LT_List = (Para.Light_List.Count > 0);
                            if (!Check_LT_List)
                            {
                                Check = false;
                                SaveLog($"Check Parameter Fail : Light List Count = 0", true);
                            }

                            //Check WD List
                            bool Check_WD_List = (Para.WD_List.Count > 0);
                            if (!Check_WD_List)
                            {
                                Check = false;
                                SaveLog($"Check Parameter Fail : WD List Count = 0", true);
                            }

                            if (Check)
                            {
                                SaveLog($"Check Parameter OK");
                                NowStep_FlatSingle = SingleFlowStep.Flow_Init;
                            }
                            else
                            {
                                NowStep_FlatSingle = SingleFlowStep.Alarm;
                            }
                        }
                        break;

                    #region 初始化

                    case SingleFlowStep.Flow_Init:
                        {
                            Directory.CreateDirectory(Para.OutputFolder);
                            FFC_FlowPara.WriteXML(Para, $@"{Para.OutputFolder}\Config.xml");
                            LT_Idx = 0;
                            WD_Idx = 0;
                            Img_Idx = 0;

                            int Channel = Para.Light_Channel;
                            D65.SetSwitch(Channel, true);
                            SaveLog($"Flow Init");

                            NowStep_FlatSingle = SingleFlowStep.Init_Finish;
                        }
                        break;

                    case SingleFlowStep.Init_Finish:
                        {
                            NowStep_FlatSingle = SingleFlowStep.Aperture_Move;
                        }
                        break;

                    #endregion

                    #region 靠近待測物

                    case SingleFlowStep.Aperture_Move:
                        {
                            int AperturePos = Para.Aperture_Pos;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Motor.Move(-1, AperturePos, -1, -1);
                                        SaveLog($"Aperture Move , WD Idx = {WD_Idx} , AperturePos = {AperturePos}");
                                        NowStep_FlatSingle = SingleFlowStep.Check_Aperture_InPos;
                                        NowStep_FlatSingle = SingleFlowStep.Check_Aperture_InPos;

                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        NowStep_FlatSingle = SingleFlowStep.CloseToPanel;
                                    }
                                    break;
                            }


                        }
                        break;

                    case SingleFlowStep.Check_Aperture_InPos:
                        {
                            if (!this.Motor.IsMoveProc())
                            {
                                switch (this.Motor.MoveFlowStatus())
                                {
                                    case UnitStatus.Finish:
                                        {
                                            //NowStep_FlatSingle = SingleFlowStep.CloseToPanel;
                                            NowStep_FlatSingle = SingleFlowStep.Light_On;

                                            SaveLog($"Check Focuser Move OK");
                                        }
                                        break;

                                    default:
                                        {
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                            SaveLog($"Check Focuser Move Fail", true);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case SingleFlowStep.CloseToPanel:
                        {
                            double X = Para.PLC_X_Pos_Closed;
                            this.PLC.AbsMove(X);

                            SaveLog($"PLC Move , X = {X}");
                            NowStep_FlatSingle = SingleFlowStep.Check_CloseToPanel;
                        }
                        break;

                    case SingleFlowStep.Check_CloseToPanel:
                        {
                            if (!PLC.IsRun)
                            {
                                switch (PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"PLC Move Suceed");
                                            NowStep_FlatSingle = SingleFlowStep.Light_On;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"PLC Move Fail", true);
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region LED On

                    case SingleFlowStep.Light_On:
                        {
                            TM_Light.SetDelay(3000);
                            int Channel = Para.Light_Channel;

                            D65.SetSwitch(Channel, true);

                            SaveLog($"D65 Set Light On , Channel = {Channel} ");

                            NowStep_FlatSingle = SingleFlowStep.Check_Light_On;
                        }
                        break;

                    case SingleFlowStep.Check_Light_On:
                        {
                            switch (D65.FunctionStatus)
                            {
                                case UnitStatus.Finish:
                                    {
                                        TM_Light.SetDelay(Para.Light_Delay);
                                        SaveLog($"Check D65 Light ON OK");
                                        NowStep_FlatSingle = SingleFlowStep.Light_SetBrightness;
                                    }
                                    break;

                                case UnitStatus.Alarm:
                                    {
                                        SaveLog($"Check D65 Light ON Fail", true);
                                        NowStep_FlatSingle = SingleFlowStep.Alarm;
                                    }
                                    break;

                            }

                            if (TM_Light.IsTimeOut())
                            {
                                SaveLog($"Check D65 Light ON Fail", true);
                                NowStep_FlatSingle = SingleFlowStep.Alarm;
                            }
                        }
                        break;

                    #endregion

                    #region  設定光源+切換ND (ND搭配光源)      

                    case SingleFlowStep.Light_SetBrightness:
                        {
                            int Channel = Para.Light_Channel;
                            int Brightness = Para.Light_List[LT_Idx].Light_Brightness;

                            D65.SetBrightnessFlow(Channel, Brightness);

                            SaveLog($"D65 Set Light , Light Idx =({LT_Idx}) , Channel = {Channel} , Brightness = {Brightness}");

                            NowStep_FlatSingle = SingleFlowStep.FW_ND_Move;
                        }
                        break;

                    case SingleFlowStep.FW_ND_Move:
                        {
                            int FW_ND = (int)Para.Light_List[LT_Idx].FW_ND;

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
                                        this.Motor.Move(-1, -1, -1, FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {

                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            SaveLog($"FW Move , ND = {FW_ND}");
                            NowStep_FlatSingle = SingleFlowStep.Check_Light_SetBrightness;

                        }
                        break;

                    case SingleFlowStep.Check_Light_SetBrightness:
                        {
                            if (!D65.Run)
                            {
                                switch (D65.Status)
                                {
                                    case UnitStatus.Finish:
                                        {
                                            TM_Light.SetDelay(Para.Light_Delay);
                                            SaveLog($"Check D65 Light Init OK");
                                            NowStep_FlatSingle = SingleFlowStep.Check_FW_ND_InPos;
                                        }
                                        break;

                                    case UnitStatus.Alarm:
                                        {
                                            SaveLog($"Check D65 Light Init Fail", true);
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case SingleFlowStep.Check_FW_ND_InPos:
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
                                        if (!this.Motor.IsMoveProc())
                                        {
                                            NdReady = (Motor.MoveFlowStatus() == UnitStatus.Finish && this.Motor.MoveStatus_FW2() == UnitStatus.Finish);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        NdReady = true;
                                    }
                                    break;
                            }

                            if (NdReady)
                            {
                                SaveLog($"Check FW Move OK");
                                NowStep_FlatSingle = SingleFlowStep.Focuser_Move;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Check FW Move Timeout", true);
                                    NowStep_FlatSingle = SingleFlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region 移動+切換XYZ (自動曝光，全FOV，X/Y/Z Filter 各一張)

                    case SingleFlowStep.Focuser_Move:
                        {
                            int FocusPos = Para.WD_List[WD_Idx].Focuser_Pos;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                case EnumMotoControlPackage.MS5515M:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Motor.Move(FocusPos, -1, -1, -1);
                                    }
                                    break;
                            }

                            SaveLog($"Focuser Move , WD Idx = {WD_Idx} , FocuserPos = {FocusPos}");
                            NowStep_FlatSingle = SingleFlowStep.Check_Focuser_InPos;
                        }
                        break;

                    case SingleFlowStep.Check_Focuser_InPos:
                        {

                            if (!this.Motor.IsMoveProc())
                            {
                                switch (this.Motor.MoveFlowStatus())
                                {
                                    case UnitStatus.Finish:
                                        {
                                            NowStep_FlatSingle = SingleFlowStep.FW_XYZ_Move;
                                            SaveLog($"Check Focuser Move OK");
                                        }
                                        break;

                                    default:
                                        {
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                            SaveLog($"Check Focuser Move Fail", true);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case SingleFlowStep.FW_XYZ_Move:
                        {
                            Thread.Sleep(100);
                            int FW_XYZ = Img_Idx;

                            if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
                            {
                                if (FW_XYZ == 1)
                                {
                                    FW_XYZ = -1;
                                }
                                else
                                {
                                    NowStep_FlatSingle = SingleFlowStep.Check_Image_List;
                                    break;                                    
                                }
                            }

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
                                        this.Motor.Move(-1, -1, FW_XYZ, -1);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);
                            SaveLog($"FW Move , Img Idx = {Img_Idx} , XYZ = {FW_XYZ}");
                            NowStep_FlatSingle = SingleFlowStep.Check_FW_XYZ_InPos;
                        }
                        break;

                    case SingleFlowStep.Check_FW_XYZ_InPos:
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
                                        if (!this.Motor.IsMoveProc())
                                        {
                                            XyzReady = (Motor.MoveFlowStatus() == UnitStatus.Finish && this.Motor.MoveStatus_FW1() == UnitStatus.Finish);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        XyzReady = true;
                                    }
                                    break;
                            }

                            if (XyzReady)
                            {
                                SaveLog($"Check FW Move OK");

                                NowStep_FlatSingle = SingleFlowStep.AutoExposureTime;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Check FW Move Timeout", true);
                                    NowStep_FlatSingle = SingleFlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case SingleFlowStep.AutoExposureTime:
                        {
                            if (TM_Light.IsTimeOut())
                            {
                                AutoExpTime = new Task_AutoExpTime(Para.Para_AutoExpTime, Img_Idx, this.Info, this.Grabber);
                                AutoExpTime.Start(Img_Idx);
                                SaveLog($"Auto ExposureTime Start");

                                Thread.Sleep(100);
                                NowStep_FlatSingle = SingleFlowStep.Check_AutoExposureTime;
                            }
                        }
                        break;

                    case SingleFlowStep.Check_AutoExposureTime:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                switch (AutoExpTime.State)
                                {
                                    case Task_AutoExpTime.EnState.Finish:
                                        {
                                            SaveLog($"Auto ExposureTime Finish");
                                            NowStep_FlatSingle = SingleFlowStep.Capture;
                                        }
                                        break;

                                    case Task_AutoExpTime.EnState.Warning:
                                    case Task_AutoExpTime.EnState.Alarm:
                                        {
                                            SaveLog($"Auto ExposureTime Fail", true);
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case SingleFlowStep.Capture:
                        {
                            double ExpTime = AutoExpTime.Result.ExpTime;
                            Grabber.SetExposureTime(ExpTime);
                            Grabber.Grab();
                            SaveLog($"Capture , ExpTime = {ExpTime}");

                            Thread.Sleep(100);

                            string WD_Remark = $"{Para.WD_List[WD_Idx].Remark}mm";
                            string ND_Remark = $"ND{(int)Para.Light_List[LT_Idx].FW_ND}_Flat";

                            string SaveFolder = $@"{Para.OutputFolder}\FFC\{WD_Remark}\{ND_Remark}\";
                            Directory.CreateDirectory(SaveFolder);
                            string ImgPath = $@"{SaveFolder}\{(FW_XYZ_Remark)Img_Idx}.tif";
                            MIL.MbufSave(ImgPath, Grabber.grabImage);
                            SaveLog($"Save Image : {ImgPath}");

                            NowStep_FlatSingle = SingleFlowStep.RecordCsv;
                        }
                        break;

                    case SingleFlowStep.RecordCsv:
                        {
                            NowStep_FlatSingle = SingleFlowStep.Check_Image_List;
                        }
                        break;

                    #endregion

                    #region 確認下一RUN

                    case SingleFlowStep.Check_Image_List:
                        {
                            Img_Idx++;

                            if (Img_Idx < 3)
                            {
                                NowStep_FlatSingle = SingleFlowStep.FW_XYZ_Move;
                            }
                            else
                            {
                                NowStep_FlatSingle = SingleFlowStep.Check_WD_List;
                            }
                        }
                        break;

                    case SingleFlowStep.Check_WD_List:
                        {
                            Img_Idx = 0;
                            WD_Idx++;

                            if (WD_Idx < Para.WD_List.Count)
                            {
                                NowStep_FlatSingle = SingleFlowStep.Focuser_Move;
                            }
                            else
                            {
                                NowStep_FlatSingle = SingleFlowStep.Check_LT_List;
                            }
                        }
                        break;

                    case SingleFlowStep.Check_LT_List:
                        {
                            Img_Idx = 0;
                            WD_Idx = 0;
                            LT_Idx++;

                            if (LT_Idx < Para.Light_List.Count)
                            {
                                NowStep_FlatSingle = SingleFlowStep.Light_SetBrightness;
                            }
                            else
                            {
                                NowStep_FlatSingle = SingleFlowStep.Light_SetBrightness_Low;
                            }
                        }
                        break;

                    #endregion

                    #region Turn Off LED

                    case SingleFlowStep.Light_SetBrightness_Low:
                        {
                            int Channel = Para.Light_Channel;
                            int Brightness = 25;

                            D65.SetBrightnessFlow(Channel, Brightness);

                            SaveLog($"D65 Set Light , Light Idx =({LT_Idx}) , Channel = {Channel} , Brightness = {Brightness}");

                            NowStep_FlatSingle = SingleFlowStep.Light_Off;
                        }
                        break;

                    case SingleFlowStep.Light_Off:
                        {
                            if (!D65.Run)
                            {
                                switch (D65.Status)
                                {
                                    case UnitStatus.Finish:
                                        {
                                            int Channel = Para.Light_Channel;
                                            D65.SetSwitch(Channel, false);
                                            NowStep_FlatSingle = SingleFlowStep.Show_CheckForm;
                                        }
                                        break;

                                    case UnitStatus.Alarm:
                                        {
                                            SaveLog($"Check D65 Light OFF Fail", true);
                                            NowStep_FlatSingle = SingleFlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion


                    #region 確認Dark+Offset流程

                    case SingleFlowStep.Show_CheckForm:
                        {
                            CheckForm_IsCheck = UnitStatus.Running;

                            Form mainForm = Application.OpenForms["ManualForm"];
                            mainForm.BeginInvoke((MethodInvoker)delegate {
                                FlowCheckForm CheckForm = new FlowCheckForm();

                                CheckForm.isCheck -= CheckForm_isCheck;
                                CheckForm.isCheck += CheckForm_isCheck;
                                CheckForm.SetPare("取Dark/Offset圖?\r\n請確認關閉鏡頭蓋");
                                CheckForm.ShowDialog();
                                CheckForm.BringToFront();
                            }
                            );

                            NowStep_FlatSingle = SingleFlowStep.Check_NextStep;
                        }
                        break;

                    case SingleFlowStep.Check_NextStep:
                        {
                            switch (CheckForm_IsCheck)
                            {
                                case UnitStatus.Finish:
                                    {
                                        double ExpTime = Para.DarkOffset_ExpTime;
                                        Grabber.SetExposureTime(ExpTime);
                                        Grabber.Grab();
                                        SaveLog($"Capture , ExpTime = {ExpTime}");

                                        Thread.Sleep(100);

                                        string[] ImgType = new string[] { "Dark", "Offset" };

                                        for (int Wd = 0; Wd < Para.WD_List.Count; Wd++)
                                            for (int Lt = 0; Lt < Para.Light_List.Count; Lt++)
                                                for (int Img = 0; Img < 3; Img++)
                                                    for (int t = 0; t < ImgType.Length; t++)
                                                    {
                                                        if (GlobalVar.SD.XYZFilter_PositionMode == EnumXYZFilter_PositionMode.AIL_2_Positions)
                                                        {
                                                            if (Img != 1) continue;
                                                        }

                                                        string WD_Remark = $"{Para.WD_List[Wd].Remark}mm";
                                                        string ND_Remark = $"ND{(int)Para.Light_List[Lt].FW_ND}_{ImgType[t]}";

                                                        string SaveFolder = $@"{Para.OutputFolder}\FFC\{WD_Remark}\{ND_Remark}\";
                                                        Directory.CreateDirectory(SaveFolder);

                                                        string ImgPath = $@"{SaveFolder}\{(FW_XYZ_Remark)Img}.tif";
                                                        MIL.MbufSave(ImgPath, Grabber.grabImage);

                                                        SaveLog($"Save Image : {ImgPath}");
                                                    }

                                        NowStep_FlatSingle = SingleFlowStep.Finish;
                                    }
                                    break;


                                case UnitStatus.Idle:
                                    {
                                        NowStep_FlatSingle = SingleFlowStep.Finish;
                                    }
                                    break;
                            }
                        }
                        break;

                    #endregion


                    case SingleFlowStep.Finish:
                        {
                            SaveLog($"Flow Finish");

                            FlowRun_Single = false;
                        }
                        break;

                    case SingleFlowStep.Alarm:
                        {
                            SaveLog($"Flow Alarm", true);
                            FlowRun_Single = false;
                        }
                        break;
                }

                MonitorStep?.Invoke($"[{(int)NowStep_FlatSingle}] {NowStep_FlatSingle}");
            }
        }

        private void CheckForm_isCheck(bool Check)
        {
            CheckForm_IsCheck = (Check) ? UnitStatus.Finish : UnitStatus.Idle;
        }

        public enum SingleFlowStep
        {
            //初始化
            Init = 0,
            Check_Device,
            Check_Parameter,
            Flow_Init,
            Init_Finish,

            //靠近待測物
            Aperture_Move,
            Check_Aperture_InPos,

            CloseToPanel = 400,
            Check_CloseToPanel,

            //開啟LED
            Light_On,
            Check_Light_On,

            //設定光源+切換ND (ND搭配光源)
            Light_SetBrightness = 500,
            FW_ND_Move,
            Check_Light_SetBrightness,
            Check_FW_ND_InPos,

            //移動+切換XYZ (自動曝光，全FOV，X/Y/Z Filter 各一張)
            Focuser_Move,
            Check_Focuser_InPos,
            FW_XYZ_Move,
            Check_FW_XYZ_InPos,
            AutoExposureTime,
            Check_AutoExposureTime,
            Capture,
            RecordCsv,

            //確認下一RUN(工作距離)
            Check_Image_List = 600,
            Check_WD_List,
            Check_LT_List,

            Light_SetBrightness_Low,
            Light_Off,

            Show_CheckForm,
            Check_NextStep,

            Finish = 5000,
            Alarm = 9999,
        }

        #endregion
    }

    public class FFC_FlowPara
    {
        [Category("FFC Para"), DisplayName("01. Output Folder"), Description("Output Folder"), ReadOnly(true), XmlIgnore]
        public string OutputFolder { get; set; } = @"D:\OMS\FFC Create\";

        [Category("FFC Para"), DisplayName("02. Aperture Pos"), Description("Aperture Pos")]
        public int Aperture_Pos { get; set; } = 0;

        [Category("FFC Para"), DisplayName("03. Capture Pos (PLC X)"), Description("Capture Pos (PLC X)"), Browsable(false)]
        public double PLC_X_Pos_Closed { get; set; } = 0;

        [Category("FFC Para"), DisplayName("04. Light Channel"), Description("Light Cannel")]
        public int Light_Channel { get; set; } = 1;

        [Category("FFC Para"), DisplayName("05. Light Delay (ms)"), Description("Light Delay (ms)")]
        public int Light_Delay { get; set; } = 1000;

        [Category("FFC Para"), DisplayName("06. Light List"), Description("Light List (Brighness + ND)")]
        public List<LightPare> Light_List { get; set; } = new List<LightPare>();

        [Category("FFC Para"), DisplayName("07. WD List"), Description("WD List (AF)")]
        public List<WdPara> WD_List { get; set; } = new List<WdPara>();

        [Category("FFC Para"), DisplayName("08. AutoExpTime Para"), Description("AutoExpTime Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AutoExpTime_FlowPara Para_AutoExpTime { get; set; } = new AutoExpTime_FlowPara();

        [Category("FFC Para"), DisplayName("09. Dark/Offset ExpTime"), Description("Dark/Offset ExpTime")]
        public int DarkOffset_ExpTime { get; set; } = 100;



        public void Clone(FFC_FlowPara Source)
        {
            this.OutputFolder = Source.OutputFolder;
            this.Aperture_Pos = Source.Aperture_Pos;
            this.PLC_X_Pos_Closed = Source.PLC_X_Pos_Closed;
            this.Light_Channel = Source.Light_Channel;
            this.Light_Delay = Source.Light_Delay;

            this.Light_List = new List<LightPare>();
            foreach (LightPare LT in Source.Light_List)
            {
                this.Light_List.Add(LT);
            }

            this.WD_List = new List<WdPara>();
            foreach (WdPara Wd in Source.WD_List)
            {
                this.WD_List.Add(Wd);
            }
            this.Para_AutoExpTime.Clone(Source.Para_AutoExpTime);
            this.DarkOffset_ExpTime = Source.DarkOffset_ExpTime;

        }

        public static void WriteXML(FFC_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(FFC_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static FFC_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                FFC_FlowPara m;

                serializer = new XmlSerializer(typeof(FFC_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (FFC_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }

        public class LightPare
        {
            [Category("Light Para"), DisplayName("01. Light Brightness"), Description("Remark")]
            public int Light_Brightness { get; set; } = 100;


            [Category("Light Para"), DisplayName("02. FW - ND"), Description("FW - ND")]
            [TypeConverter(typeof(EnumTypeConverter))]
            public FW_ND_Remark FW_ND { get; set; } = FW_ND_Remark.ND_12_5;

            public override string ToString()
            {
                return $"{(int)FW_ND}";
            }
        }

        public class WdPara
        {
            [Category("WD Para"), DisplayName("01. Remark (mm)"), Description("Remark (mm)")]
            public string Remark { get; set; } = "";

            [Category("WD Para"), DisplayName("02. Focuser Pos"), Description("Focuser Pos")]
            public int Focuser_Pos { get; set; } = 0;

            public override string ToString()
            {
                return Remark;
            }
        }


        public class ImagePara
        {
            [Category("Image Para"), DisplayName("01. Remark"), Description("Remark")]
            public string Remark { get; set; } = "";

            [Category("Image Para"), DisplayName("02. FW - XYZ"), Description("FW - XYZ")]
            public FW_XYZ_Remark FW_XYZ { get; set; } = FW_XYZ_Remark.X;

            public override string ToString()
            {
                return Remark;
            }
        }
    }

}
