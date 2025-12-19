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
    public class Task_LightSourceA_Create
    {
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


        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_LightSourceA_Create(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            string LogTitle = "LightSourceA Create";

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

        public void Start(LightSourceA_Create_FlowPara SourcePara)
        {
            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            LightSourceA_Create_FlowPara Para = new LightSourceA_Create_FlowPara();
            Para.Clone(SourcePara);

            Thread mThread = new Thread(() => Flow(Para));
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

        public void Flow(LightSourceA_Create_FlowPara Para)
        {
            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();
            TimeManager TM_LightSourceA_Move = new TimeManager();
            TimeManager TM_Reload = new TimeManager();
            TimeManager TM_ReMeasure = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            Task_BaseAlign BaseAlign = null;
            BaseAlign_FlowPara BaseAlign_Para = new BaseAlign_FlowPara();
            Task_BaseAlign.FlowResult BaseAlign_Result = new Task_BaseAlign.FlowResult();

            Task_AutoExpTime AutoExpTime = null;
            AutoExpTime_FlowPara AutoExpTime_Para = new AutoExpTime_FlowPara();

            double CapturePos_RobotX = 0;
            double CapturePos_RobotY = 0;
            double CapturePos_RobotZ = 0;
            double CapturePos_RobotRoll = 0;
            double CapturePos_RobotPitch = 0;
            double CapturePos_RobotYaw = 0;

            int LightSourceA_MoveStep_Base = 0;
            int LightSourceA_MoveStep_Shift = 0;
            int LightSourceA_MoveStep_Move = 0;
            int LightSourceA_MoveStep_Now = 0;


            double AreaMean = 0.0;

            int LT_Idx = 0;
            int Img_Idx = 0;

            RecordPara RecordInfo = new RecordPara();
            List<CorrectionData> CorrData = new List<CorrectionData>();
            CorrectionData NowCorrData = new CorrectionData();

            Para.OutputFolder = $@"{Para.OutputFolder}\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            string CorrOutputFolder = $@"D:\OMS\Third Party\Linear Calibration\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

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

                            //Check SR3
                            bool Check_SR3 = true;
                            Check_SR3 = (this.Hardware.SR3 != null);
                            if (Check_SR3)
                            {
                                Check_SR3 = this.Hardware.SR3.IsConnect;
                                if (!Check_SR3)
                                {
                                    SaveLog($"Check Device Fail : SR3 is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  SR3 is null", true);
                            }
                            Check &= Check_SR3;

                            //Check Robot
                            bool Check_Robot = true;
                            Check_Robot = (this.Hardware.Robot != null);
                            if (Check_Robot)
                            {
                                Check_Robot = this.Hardware.Robot.IsConnected();
                                if (!Check_Robot)
                                {
                                    SaveLog($"Check Device Fail : Robot is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  Robot is null", true);
                            }
                            Check &= Check_Robot;

                            //Check LightSourceA
                            bool Check_LightSourceA = true;
                            Check_LightSourceA = (this.Hardware.LightSourceA != null);
                            if (Check_LightSourceA)
                            {
                                Check_LightSourceA = this.Hardware.LightSourceA.CheckConnection();
                                if (!Check_LightSourceA)
                                {
                                    SaveLog($"Check Device Fail : LightSourceA is not Work", true);
                                }
                            }
                            else
                            {
                                SaveLog($"Check Device Fail :  LightSourceA is null", true);
                            }
                            Check &= Check_LightSourceA;

                            //Check Grabber
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Hardware.Grabber != null);
                            if (!Check_Grabber)
                            {
                                SaveLog($"Check Device Fail :  Grabber is null", true);
                            }

                            Check &= Check_LightSourceA;


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
                            bool Check_LT_List = (Para.Light_List.Count > 0);
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

                            Directory.CreateDirectory(Para.OutputFolder);
                            Directory.CreateDirectory(CorrOutputFolder);

                            LightSourceA_Create_FlowPara.WriteXML(Para, $@"{Para.OutputFolder}\Config.xml");

                            LightSourceA_MoveStep_Base = Para.Light_List[0].LightSourceA_InitStep;
                            LightSourceA_MoveStep_Shift = Para.LightSourceA_ShiftStep;
                            LightSourceA_MoveStep_Move = Para.LightSourceA_MoveStep;
                            LightSourceA_MoveStep_Now = 0;

                            NowStep = FlowStep.Aperture_Move;
                        }
                        break;

                    case FlowStep.Aperture_Move:
                        {
                            int AperturePos = Para.Aperture;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Hardware.Motor.Move(-1, AperturePos, -1, -1);
                                        SaveLog($"Aperture Move , AperturePos = {AperturePos}");
                                        NowStep = FlowStep.Check_Aperture_InPos;
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        NowStep = FlowStep.BaseAlign;
                                    }
                                    break;
                            }
                        }
                        break;

                    case FlowStep.Check_Aperture_InPos:
                        {
                            if (!this.Hardware.Motor.IsMoveProc())
                            {
                                switch (this.Hardware.Motor.MoveFlowStatus())
                                {
                                    case UnitStatus.Finish:
                                        {
                                            NowStep = FlowStep.BaseAlign;
                                            SaveLog($"Check Aperture Move OK");
                                        }
                                        break;

                                    default:
                                        {
                                            NowStep = FlowStep.Alarm;
                                            SaveLog($"Check Aperture Move Alarm", true);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region Base Align

                    case FlowStep.BaseAlign:
                        {
                            BaseAlign = new Task_BaseAlign(this.Info, this.Hardware);
                            BaseAlign.Start(Para.BaseAlign_Para);

                            Thread.Sleep(1000);
                            SaveLog($"Base Align Start");
                            NowStep = FlowStep.CheckBaseAlign;
                        }
                        break;

                    case FlowStep.CheckBaseAlign:
                        {
                            if (!BaseAlign.IsRun)
                            {
                                switch (BaseAlign.Step)
                                {
                                    case Task_BaseAlign.FlowStep.Finish:
                                        {
                                            CapturePos_RobotX = BaseAlign.Result.RobotX;
                                            CapturePos_RobotY = BaseAlign.Result.RobotY;
                                            CapturePos_RobotZ = BaseAlign.Result.RobotZ;
                                            CapturePos_RobotRoll = BaseAlign.Result.RobotRoll;
                                            CapturePos_RobotPitch = BaseAlign.Result.RobotPitch;
                                            CapturePos_RobotYaw = BaseAlign.Result.RobotYaw;

                                            SaveLog($"Check Base Align Finish");
                                            NowStep = FlowStep.Show_CheckForm;
                                        }
                                        break;

                                    default:
                                        {
                                            SaveLog($"Check Base Align Fail , Step = {BaseAlign.Step}", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region 確認視窗 : 移開鏡子

                    case FlowStep.Show_CheckForm:
                        {
                            CheckForm_IsCheck = UnitStatus.Running;

                            Form mainForm = Application.OpenForms["ManualForm"];
                            mainForm.BeginInvoke((MethodInvoker)delegate {
                                FlowCheckForm CheckForm = new FlowCheckForm();

                                CheckForm.isCheck -= CheckForm_isCheck;
                                CheckForm.isCheck += CheckForm_isCheck;

                                CheckForm.SetPare("請將鏡子移開\r\n請確認已移開");
                                CheckForm.ShowDialog();
                                CheckForm.BringToFront();
                            }
                            );

                            SaveLog($"Show UserCheckForm");
                            NowStep = FlowStep.Check_NextStep;
                        }
                        break;

                    case FlowStep.Check_NextStep:
                        {
                            switch (CheckForm_IsCheck)
                            {
                                case UnitStatus.Finish:
                                    {
                                        SaveLog($"UserCheckForm Select OK");
                                        //NowStep = FlowStep.LightSourceA_Home;
                                        NowStep = FlowStep.LightSourceA_MoveToBase;
                                    }
                                    break;


                                case UnitStatus.Idle:
                                    {
                                        SaveLog($"UserCheckForm Select NG", true);
                                        NowStep = FlowStep.Alarm;
                                    }
                                    break;
                            }
                        }
                        break;

                    #endregion

                    #region 使用標準件量測

                    case FlowStep.LightSourceA_Home:
                        {
                            //RecordInfo = new RecordPara();

                            //TM_LightSourceA_Home.SetDelay(WaitMoveTime);
                            //this.Hardware.LightSourceA.MoveHome(LightSourceA_Ctrl.MotorType.X);

                            //SaveLog($"LightSourceA Home");
                            //NowStep = FlowStep.LightSourceA_MoveToBase;
                        }
                        break;

                    case FlowStep.LightSourceA_MoveToBase:
                        {
                            if (TM_LightSourceA_Move.IsTimeOut())
                            {
                                int BaseStep = LightSourceA_MoveStep_Base - LightSourceA_MoveStep_Shift;

                                if (LightSourceA_MoveStep_Now != BaseStep)
                                {
                                    LightSourceA_MoveStep_Now = BaseStep;
                                    this.Hardware.LightSourceA.SetSteps(LightSourceA_Ctrl.MotorType.X, BaseStep);
                                    Thread.Sleep(500);
                                }

                                this.Hardware.LightSourceA.MoveBackward(LightSourceA_Ctrl.MotorType.X);

                                TM_LightSourceA_Move.SetDelay(BaseStep * 30);

                                NowStep = FlowStep.Robot_Move_UseSR3;
                            }
                        }
                        break;

                    case FlowStep.Robot_Move_UseSR3:
                        {
                            double X = Para.MeasurePos_RobotX;
                            double Y = Para.MeasurePos_RobotY;
                            double Z = Para.MeasurePos_RobotZ;
                            double Roll = Para.MeasurePos_RobotRoll * Math.PI / 180D;
                            double Pitch = Para.MeasurePos_RobotPitch * Math.PI / 180D;
                            double Yaw = Para.MeasurePos_RobotYaw * Math.PI / 180D;

                            double Acc = GlobalVar.SD.UrRobot_Acc;
                            double Speed = GlobalVar.SD.UrRobot_Speed;
                            double Time = GlobalVar.SD.UrRobot_Time;
                            double Blendradius = GlobalVar.SD.UrRobot_Blendradius;

                            SaveLog($"Robot Move (Measure Pos) , X = {X} , Y = {Y} , Z = {Z} , Roll = {Roll} , Pitch = {Pitch} , Yaw = {Yaw} " +
                            $", Acc = {Acc} , Speed = {Speed} , Time = {Time} , Blendradius = {Blendradius}");

                            bool Rtn = this.Hardware.Robot.MoveL(X, Y, Z, Roll, Pitch, Yaw, Acc, Speed, Time, Blendradius);

                            if (Rtn)
                            {
                                SaveLog($"Robot Move (Measure Pos) , InPos");
                                NowStep = FlowStep.FW_ND_Move;
                            }
                            else
                            {
                                SaveLog($"Robot Move (Measure Pos) , Retry");
                            }
                        }
                        break;

                    case FlowStep.FW_ND_Move:
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
                                        this.Hardware.Motor.Move(-1, -1, -1, FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);
                            SaveLog($"FW Move , Light Idx = {LT_Idx} , ND = {FW_ND}");
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

                            if (NdReady)
                            {
                                SaveLog($"Check FW Move OK");
                                RecordInfo.FilterND = Para.Light_List[LT_Idx].FW_ND.ToString();

                                NowStep = FlowStep.LightSourceA_Move_InitPos;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Check FW Move Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.LightSourceA_Move_InitPos:
                        {
                            if (TM_LightSourceA_Move.IsTimeOut())
                            {
                                if (LT_Idx != 0)
                                {
                                    int Shift_byBase = (Para.Light_List[LT_Idx].LightSourceA_InitStep - Para.Light_List[LT_Idx - 1].LightSourceA_InitStep);

                                    // if (Shift_byBase != 0)
                                    {
                                        if (LightSourceA_MoveStep_Now != Shift_byBase)
                                        {
                                            LightSourceA_MoveStep_Now = Shift_byBase;
                                            this.Hardware.LightSourceA.SetSteps(LightSourceA_Ctrl.MotorType.X, Shift_byBase);
                                            Thread.Sleep(500);
                                        }

                                        this.Hardware.LightSourceA.MoveBackward(LightSourceA_Ctrl.MotorType.X);
                                    }

                                    TM_LightSourceA_Move.SetDelay(Shift_byBase * 30);
                                }

                                NowStep = FlowStep.SR3_Measurement;
                            }
                        }
                        break;

                    case FlowStep.SR3_Measurement:
                        {
                            if (TM_LightSourceA_Move.IsTimeOut())
                            {
                                this.Hardware.SR3.Measurement(Para.MesureTimeout);

                                SaveLog($"SR3 Measurement");

                                TM_Reload.SetDelay(3000);
                                NowStep = FlowStep.Check_SR3_Measurement;
                            }
                        }
                        break;

                    case FlowStep.Check_SR3_Measurement:
                        {
                            switch (this.Hardware.SR3.Status)
                            {
                                case UnitStatus.Finish:
                                    {
                                        SaveLog($"SR3 Measurement Finish");
                                        NowStep = FlowStep.Check_Light_Match;
                                    }
                                    break;

                                case UnitStatus.Alarm:
                                    {
                                        SaveLog($"SR3 Measurement Fail", true);
                                        NowStep = FlowStep.SR3_Measurement;
                                    }
                                    break;
                            }

                            if (TM_Reload.IsTimeOut())
                            {
                                this.Hardware.SR3.Retry();
                                TM_Reload.Reset();
                            }
                        }
                        break;

                    case FlowStep.Check_Light_Match:
                        {
                            double Y = 0.0;
                            Double.TryParse(Hardware.SR3.Result[5], out Y);

                            double Cx = 0.0;
                            double Cy = 0.0;
                            Double.TryParse(Hardware.SR3.Result[7], out Cx);
                            Double.TryParse(Hardware.SR3.Result[8], out Cy);

                            double TargetValue = Para.Light_List[LT_Idx].TargetBrightness;
                            int Tolerance = Para.Target_Light_Tolerance;

                            double MaxValue = TargetValue * (1 + (Tolerance / 100.0));
                            double MinValue = TargetValue * (1 - (Tolerance / 100.0));

                            bool Match = (Y >= MinValue && Y <= MaxValue);

                            SaveLog($"Check Light Match : {Match} , TargetValue = {TargetValue} , MeasureValue = {Y}");

                            if (Match)
                            {
                                RecordInfo.TristimulusY_Target = TargetValue;
                                RecordInfo.TristimulusY = Y;
                                RecordInfo.Cx = Cx;
                                RecordInfo.Cy = Cy;

                                TM_ReMeasure = new TimeManager(Para.RemeasureDelay);

                                NowStep = FlowStep.Re_SR3_Measurement;
                            }
                            else
                            {
                                if (Y > MaxValue)
                                {
                                    SaveLog($"Measure Y > MaxValue", true);
                                    NowStep = FlowStep.Alarm;
                                    break;
                                }
                                else
                                {
                                    NowStep = FlowStep.LightSourceA_Move;
                                }
                            }
                        }
                        break;

                    case FlowStep.LightSourceA_Move:
                        {
                            if (LightSourceA_MoveStep_Now != LightSourceA_MoveStep_Move)
                            {
                                LightSourceA_MoveStep_Now = LightSourceA_MoveStep_Move;
                                this.Hardware.LightSourceA.SetSteps(LightSourceA_Ctrl.MotorType.X, LightSourceA_MoveStep_Move);
                                Thread.Sleep(500);
                            }

                            this.Hardware.LightSourceA.MoveBackward(LightSourceA_Ctrl.MotorType.X);

                            TM_LightSourceA_Move.SetDelay(LightSourceA_MoveStep_Move * 30);

                            NowStep = FlowStep.SR3_Measurement;
                        }
                        break;

                    case FlowStep.Re_SR3_Measurement:
                        {
                            if (TM_ReMeasure.IsTimeOut())
                            {
                                this.Hardware.SR3.Measurement(Para.MesureTimeout);

                                SaveLog($"SR3 Measurement");

                                TM_Reload.SetDelay(3000);

                                NowStep = FlowStep.Re_Check_SR3_Measurement;
                            }
                        }
                        break;

                    case FlowStep.Re_Check_SR3_Measurement:
                        {
                            switch (this.Hardware.SR3.Status)
                            {
                                case UnitStatus.Finish:
                                    {
                                        SaveLog($"SR3 Measurement Finish");

                                        double Y = 0.0;
                                        double Cx = 0.0;
                                        double Cy = 0.0;

                                        Double.TryParse(Hardware.SR3.Result[5], out Y);
                                        Double.TryParse(Hardware.SR3.Result[7], out Cx);
                                        Double.TryParse(Hardware.SR3.Result[8], out Cy);

                                        double TargetValue = Para.Light_List[LT_Idx].TargetBrightness;

                                        RecordInfo.TristimulusY_Target = TargetValue;
                                        RecordInfo.TristimulusY = Y;
                                        RecordInfo.Cx = Cx;
                                        RecordInfo.Cy = Cy;

                                        NowCorrData.std[0] = Cx * (Y / Cy);
                                        NowCorrData.std[1] = Y;
                                        NowCorrData.std[2] = (1 - Cx - Cy) * (Y / Cy);

                                        SaveLog($"SR3 Re-Measure , TargetValue = {TargetValue} , MeasureValue = {Y}");

                                        NowStep = FlowStep.Robot_Move_UseAIC;
                                    }
                                    break;

                                case UnitStatus.Alarm:
                                    {
                                        SaveLog($"SR3 Measurement Fail", true);
                                        NowStep = FlowStep.Re_SR3_Measurement;
                                    }
                                    break;
                            }

                            if (TM_Reload.IsTimeOut())
                            {
                                this.Hardware.SR3.Retry();
                                TM_Reload.Reset();
                            }
                        }
                        break;

                    #endregion

                    #region 使用AIC拍照

                    case FlowStep.Robot_Move_UseAIC:
                        {
                            double X = CapturePos_RobotX;
                            double Y = CapturePos_RobotY;
                            double Z = CapturePos_RobotZ;
                            double Roll = CapturePos_RobotRoll * Math.PI / 180D;
                            double Pitch = CapturePos_RobotPitch * Math.PI / 180D;
                            double Yaw = CapturePos_RobotYaw * Math.PI / 180D;

                            double Acc = GlobalVar.SD.UrRobot_Acc;
                            double Speed = GlobalVar.SD.UrRobot_Speed;
                            double Time = GlobalVar.SD.UrRobot_Time;
                            double Blendradius = GlobalVar.SD.UrRobot_Blendradius;

                            SaveLog($"Robot Move (Capture Pos) , X = {X} , Y = {Y} , Z = {Z} , Roll = {Roll} , Pitch = {Pitch} , Yaw = {Yaw} " +
                             $", Acc = {Acc} , Speed = {Speed} , Time = {Time} , Blendradius = {Blendradius}");

                            bool Rtn = this.Hardware.Robot.MoveL(X, Y, Z, Roll, Pitch, Yaw, Acc, Speed, Time, Blendradius);

                            if (Rtn)
                            {
                                SaveLog($"Robot Move (Capture Pos) , InPos");
                                NowStep = FlowStep.FW_XYZ_Move;
                            }
                            else
                            {
                                SaveLog($"Robot Move (Capture Pos) , Retry");
                            }
                        }
                        break;

                    case FlowStep.FW_XYZ_Move:
                        {
                            int FW_XYZ = Img_Idx;

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
                            SaveLog($"FW Move , Img Idx = {Img_Idx} , XYZ = {FW_XYZ}");
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

                            if (XyzReady)
                            {
                                SaveLog($"Check FW Move OK");
                                RecordInfo.FilterXYZ = ((FW_XYZ_Remark)Img_Idx).ToString();

                                NowStep = FlowStep.AutoExposureTime;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Check FW Move Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.AutoExposureTime:
                        {
                            AutoExpTime = new Task_AutoExpTime(Para.AutoExpTime_Para, Img_Idx, this.Info, this.Hardware.Grabber);
                            AutoExpTime.Start(Img_Idx);
                            SaveLog($"Auto ExposureTime Start");

                            Thread.Sleep(100);
                            NowStep = FlowStep.Check_AutoExposureTime;
                        }
                        break;

                    case FlowStep.Check_AutoExposureTime:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                switch (AutoExpTime.State)
                                {
                                    case Task_AutoExpTime.EnState.Finish:
                                        {
                                            SaveLog($"Auto ExposureTime Finish");
                                            NowStep = FlowStep.Capture;
                                        }
                                        break;

                                    case Task_AutoExpTime.EnState.Warning:
                                    case Task_AutoExpTime.EnState.Alarm:
                                        {
                                            SaveLog($"Auto ExposureTime Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Capture:
                        {
                            double ExpTime = AutoExpTime.Result.ExpTime;
                            this.Hardware.Grabber.SetExposureTime(ExpTime);
                            this.Hardware.Grabber.Grab();
                            SaveLog($"Capture , ExpTime = {ExpTime}");

                            Thread.Sleep(100);
                            int ND = (int)Para.Light_List[LT_Idx].FW_ND;
                            double Brightness = Para.Light_List[LT_Idx].TargetBrightness;

                            string LT_Remark = $"ND{ND}_{Brightness}nit";
                            string SaveFolder = $@"{Para.OutputFolder}\{LT_Remark}\";

                            Directory.CreateDirectory(SaveFolder);
                            string ImgPath = $@"{SaveFolder}\{(FW_XYZ_Remark)Img_Idx}.tif";
                            MIL.MbufSave(ImgPath, this.Hardware.Grabber.grabImage);
                            SaveLog($"Save Image : {ImgPath}");

                            RecordInfo.ExposureTime = ExpTime;

                            NowStep = FlowStep.Cal_CenterGray;
                        }
                        break;

                    case FlowStep.Cal_CenterGray:
                        {
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
                                        SaveLog($"Cal CenterGray (Circle) , Radius = {Radius} , Mean = {AreaMean}");
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
                                        SaveLog($"Cal CenterGray (Rect) , Size = ({W},{H}) , Mean = {AreaMean}");
                                    }
                                    break;
                            }

                            RecordInfo.AreaMean = AreaMean;


                            NowStep = FlowStep.RecordCsv;
                        }
                        break;

                    case FlowStep.RecordCsv:
                        {
                            WriteRowData($@"{Para.OutputFolder}\RowData.csv", RecordInfo);
                            NowStep = FlowStep.AddCorrData;
                        }
                        break;

                    case FlowStep.AddCorrData:
                        {
                            if (Para.Light_List[LT_Idx].Correction)
                            {
                                NowCorrData.mean[Img_Idx] = RecordInfo.AreaMean;
                                NowCorrData.exp[Img_Idx] = RecordInfo.ExposureTime;
                                NowCorrData.norm[Img_Idx] = (RecordInfo.AreaMean / (RecordInfo.ExposureTime / 1000000));
                            }

                            NowStep = FlowStep.Check_Image_List;
                        }
                        break;

                    #endregion

                    #region 確認下一RUN(亮度)

                    case FlowStep.Check_Image_List:
                        {
                            Img_Idx++;

                            if (Img_Idx < 3)
                            {

                                NowStep = FlowStep.FW_XYZ_Move;
                            }
                            else
                            {
                                if (Para.Light_List[LT_Idx].Correction)
                                {
                                    CorrData.Add(NowCorrData);
                                }

                                NowStep = FlowStep.Check_LT_List;
                            }
                        }
                        break;

                    case FlowStep.Check_LT_List:
                        {
                            NowCorrData = new CorrectionData();

                            Img_Idx = 0;
                            LT_Idx++;

                            if (LT_Idx < Para.Light_List.Count)
                            {
                                //NowStep = FlowStep.LightSourceA_Home;
                                NowStep = FlowStep.Robot_Move_UseSR3;
                                TM_LightSourceA_Move.SetDelay(0);
                            }
                            else
                            {
                                NowStep = FlowStep.RecordCorrCsv;
                            }
                        }
                        break;

                    case FlowStep.RecordCorrCsv:
                        {
                            // 獲取枚舉成員的 FieldInfo
                            FieldInfo field = Para.Light_List[0].FW_ND.GetType().GetField(Para.Light_List[0].FW_ND.ToString());
                            // 獲取 DescriptionAttribute
                            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                            string ND = attribute.Description;
                            string TargetGray = $"{Para.AutoExpTime_Para.AutoExposureTargetGray}";

                            CorrectionResult CorrResult = CalCorrResult(CorrData);

                            string Remark = $"{ND}_{TargetGray}";
                            WriteCorrData($@"{CorrOutputFolder}\{Remark}.csv", Remark, CorrData, CorrResult);

                            CorrectionPara CorrPara = new CorrectionPara {
                                ND = Para.Light_List[0].FW_ND,
                                TargetGray = Para.AutoExpTime_Para.AutoExposureTargetGray,

                                X_Coefficient = new CorrectionCoefficient {
                                    Slope = CorrResult.slope[0],
                                    Intercept = CorrResult.intercept[0],
                                    RSQ = CorrResult.rsq[0],
                                },

                                Y_Coefficient = new CorrectionCoefficient {
                                    Slope = CorrResult.slope[1],
                                    Intercept = CorrResult.intercept[1],
                                    RSQ = CorrResult.rsq[1],
                                },

                                Z_Coefficient = new CorrectionCoefficient {
                                    Slope = CorrResult.slope[2],
                                    Intercept = CorrResult.intercept[2],
                                    RSQ = CorrResult.rsq[2],
                                }
                            };

                            GlobalVar.CorrData.Para_List.Add(CorrPara);
                            GlobalVar.CorrData.Create(GlobalVar.CorrData, GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml");

                            NowStep = FlowStep.Finish;
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

        private void CheckForm_isCheck(bool Check)
        {
            CheckForm_IsCheck = (Check) ? UnitStatus.Finish : UnitStatus.Idle;
        }

        public enum FlowStep
        {
            //Check & Init 
            Check_Device = 0,
            Check_Parameter,
            Init,
            Aperture_Move,
            Check_Aperture_InPos,

            //Base Align
            BaseAlign,
            CheckBaseAlign,

            //確認視窗 : 移開鏡子
            Show_CheckForm,
            Check_NextStep,

            //使用標準件量測
            LightSourceA_Home,
            LightSourceA_MoveToBase,

            Robot_Move_UseSR3,
            FW_ND_Move,
            Check_FW_ND_InPos,
            LightSourceA_Move_InitPos,
            SR3_Measurement,
            Check_SR3_Measurement,
            Check_Light_Match,
            LightSourceA_Move,
            Re_SR3_Measurement,
            Re_Check_SR3_Measurement,

            //使用AIC拍照
            Robot_Move_UseAIC,
            FW_XYZ_Move,
            Check_FW_XYZ_InPos,
            AutoExposureTime,
            Check_AutoExposureTime,
            Capture,
            Cal_CenterGray,
            RecordCsv,
            AddCorrData,

            //確認下一RUN(亮度)
            Check_Image_List = 600,
            Check_LT_List,

            RecordCorrCsv,

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

                    string Info = string.Join(",", InfoList);
                    SB.Append(Info + "\r\n");
                }

                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");
                SB.Append("\r\n");

                SB.Append($"X_slope,{Result.slope[0]}" + "\r\n");
                SB.Append($"X_intercept,{Result.intercept[0]}" + "\r\n");
                SB.Append($"X_R²,{Result.rsq[0]}" + "\r\n");

                SB.Append("\r\n");

                SB.Append($"Y_slope,{Result.slope[1]}" + "\r\n");
                SB.Append($"Y_intercept,{Result.intercept[1]}" + "\r\n");
                SB.Append($"Y_R²,{Result.rsq[1]}" + "\r\n");

                SB.Append("\r\n");

                SB.Append($"Z_slope,{Result.slope[2]}" + "\r\n");
                SB.Append($"Z_intercept,{Result.intercept[2]}" + "\r\n");
                SB.Append($"Z_R²,{Result.rsq[2]}" + "\r\n");

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

        private CorrectionResult CalCorrResult(List<CorrectionData> Para)
        {
            CorrectionResult Result = new CorrectionResult();

            for (int xyz = 0; xyz < 3; xyz++)
            {
                List<double> STD = new List<double>();
                List<double> NORM = new List<double>();

                for (int i = 0; i < Para.Count; i++)
                {
                    STD.Add(Para[i].std[xyz]);
                    NORM.Add(Para[i].norm[xyz]);
                }

                Result.slope[xyz] = Calculate.CalculateSlope(STD, NORM);
                Result.intercept[xyz] = Calculate.CalculateIntercept(STD, NORM);
                Result.rsq[xyz] = Calculate.CalculateRSQ(STD, NORM);
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
        }

        public class CorrectionResult
        {
            public double[] slope { get; set; } = new double[3];
            public double[] intercept { get; set; } = new double[3];
            public double[] rsq { get; set; } = new double[3];
        }

        #endregion
    }

    public class LightSourceA_Create_FlowPara
    {
        [Category("A. Base Align Flow"), DisplayName("01. Base Align FlowPara"), Description("Base Align FlowPara"), Browsable(false)]
        public BaseAlign_FlowPara BaseAlign_Para { get; set; } = new BaseAlign_FlowPara();

        [Category("A. Flow Para"), DisplayName("01. Output Folder"), Description("Output Folder"), ReadOnly(true), XmlIgnore]
        public string OutputFolder { get; set; } = @"D:\OMS\LightSourceA Create\";

        [Category("A. Flow Para"), DisplayName("02. Aperture Pos"), Description("Aperture Pos")]
        public int Aperture { get; set; } = 0;

        [Category("A. Flow Para"), DisplayName("03. Target Light List"), Description("Target Light List")]
        public List<LightPare> Light_List { get; set; } = new List<LightPare>();

        [Category("A. Flow Para"), DisplayName("04. Target Light Tolerance (%)"), Description("Target Light Tolerance (%)")]
        public int Target_Light_Tolerance { get; set; } = 5;

        [Category("A. Flow Para"), DisplayName("05. LightSourceA ShiftStep (-N)"), Description("LightSourceA ShiftStep (-N)")]
        public int LightSourceA_ShiftStep { get; set; } = 100;

        [Category("A. Flow Para"), DisplayName("06. LightSourceA MoveStep (+M)"), Description("LightSourceA MoveStep (+M)")]
        public int LightSourceA_MoveStep { get; set; } = 5;

        [Category("A. Flow Para"), DisplayName("07. Measure Timeout (ms)"), Description("Measure Timeout (ms)")]
        public int MesureTimeout { get; set; } = 60000;

        [Category("A. Flow Para"), DisplayName("08. Re Measure DelayTime (ms)"), Description("Re Measure DelayTime (ms)")]
        public int RemeasureDelay { get; set; } = 60000;

        [Category("B. SR3 Measurement Pos"), DisplayName("01. Robot X"), Description("Robot X")]
        public double MeasurePos_RobotX { get; set; } = 0.0;

        [Category("B. SR3 Measurement Pos"), DisplayName("02. Robot Y"), Description("Robot Y")]
        public double MeasurePos_RobotY { get; set; } = 0.0;

        [Category("B. SR3 Measurement Pos"), DisplayName("03. Robot Z"), Description("Robot Z")]
        public double MeasurePos_RobotZ { get; set; } = 0.0;

        [Category("B. SR3 Measurement Pos"), DisplayName("04. Robot Roll"), Description("Robot Roll")]
        public double MeasurePos_RobotRoll { get; set; } = 0.0;

        [Category("B. SR3 Measurement Pos"), DisplayName("05. Robot Pitch"), Description("Robot Pitch")]
        public double MeasurePos_RobotPitch { get; set; } = 0.0;

        [Category("B. SR3 Measurement Pos"), DisplayName("06. Robot Yaw"), Description("Robot Yaw")]
        public double MeasurePos_RobotYaw { get; set; } = 0.0;

        [Category("C. Auto Exposure Time"), DisplayName("01. AutoExpTime Para"), Description("AutoExpTime Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AutoExpTime_FlowPara AutoExpTime_Para { get; set; } = new AutoExpTime_FlowPara();

        [Category("D. Capture Para"), DisplayName("01. Get Region Type"), Description("Get Region Type")]
        public EnumRegionMethod Region_Type { get; set; } = EnumRegionMethod.Circle;

        [Category("D. Capture Para"), DisplayName("11. Cycle Type - Radius"), Description("Cycle Type - Radius")]
        public int Region_CircleRadius { get; set; } = 10;

        [Category("D. Capture Para"), DisplayName("21. Rect Type - Size"), Description("Rect Type - Size")]
        public Size Region_RectSize { get; set; } = new Size(100, 100);

        public void Clone(LightSourceA_Create_FlowPara Source)
        {
            this.BaseAlign_Para.Clone(Source.BaseAlign_Para);

            //A
            this.OutputFolder = Source.OutputFolder;
            this.Aperture = Source.Aperture;
            this.Light_List = new List<LightPare>();
            foreach (LightPare Light in Source.Light_List)
            {
                Light_List.Add(Light);
            }

            this.Target_Light_Tolerance = Source.Target_Light_Tolerance;
            this.LightSourceA_ShiftStep = Source.LightSourceA_ShiftStep;
            this.LightSourceA_MoveStep = Source.LightSourceA_MoveStep;
            this.MesureTimeout = Source.MesureTimeout;
            this.RemeasureDelay = Source.RemeasureDelay;

            //B
            this.MeasurePos_RobotX = Source.MeasurePos_RobotX;
            this.MeasurePos_RobotY = Source.MeasurePos_RobotY;
            this.MeasurePos_RobotZ = Source.MeasurePos_RobotZ;
            this.MeasurePos_RobotRoll = Source.MeasurePos_RobotRoll;
            this.MeasurePos_RobotPitch = Source.MeasurePos_RobotPitch;
            this.MeasurePos_RobotYaw = Source.MeasurePos_RobotYaw;

            //C
            this.AutoExpTime_Para.Clone(Source.AutoExpTime_Para);

            //D
            this.Region_Type = Source.Region_Type;
            this.Region_CircleRadius = Source.Region_CircleRadius;
            this.Region_RectSize = Source.Region_RectSize;
        }

        public static void WriteXML(LightSourceA_Create_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(LightSourceA_Create_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static LightSourceA_Create_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                LightSourceA_Create_FlowPara m;

                serializer = new XmlSerializer(typeof(LightSourceA_Create_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (LightSourceA_Create_FlowPara)serializer.Deserialize(fs);
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
            [Category("Light Para"), DisplayName("01. Light Brightness"), Description("Brightness")]
            public double TargetBrightness { get; set; } = 100;

            [Category("Light Para"), DisplayName("02. FW - ND"), Description("FW - ND")]
            [TypeConverter(typeof(EnumTypeConverter))]
            public FW_ND_Remark FW_ND { get; set; } = FW_ND_Remark.ND_12_5;

            [Category("Light Para"), DisplayName("03. Init Step"), Description("Init Step")]
            public int LightSourceA_InitStep { get; set; } = 500;

            [Category("Light Para"), DisplayName("04. Correction"), Description("Correction")]
            public bool Correction { get; set; } = true;


            public override string ToString()
            {
                return $"{TargetBrightness}nit , ND{(int)FW_ND}";
            }
        }

    }

}
