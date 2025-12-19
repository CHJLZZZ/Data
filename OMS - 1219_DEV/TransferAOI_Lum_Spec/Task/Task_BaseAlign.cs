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
    public class Task_BaseAlign
    {
        //Device
        private InfoManager Info = null;
        private IMotorControl Motor = null;
        private D65_Light_Ctrl D65 = null;
        private LightSourceA_Ctrl LightSourceA = null;
        private ClsUrRobotInterface Robot = null;
        private X_PLC_Ctrl PLC = null;
        private MilDigitizer Grabber;

        Task_AutoFocus AutoFocus = null;

        Task_AutoPlane AutoPlane = null;
        //Flow
        private bool FlowRun = false;
        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowResult Flow_Result;

        public FlowResult Result
        {
            get => this.Flow_Result;
        }

        private FlowStep NowStep = FlowStep.Init;
        public FlowStep Step
        {
            get => NowStep;
        }


        public Task_BaseAlign(InfoManager Info, HardwareUnit Para)
        {
            this.Info = Info;
            this.Motor = Para.Motor;
            this.D65 = Para.D65;
            this.Robot = Para.Robot;
            this.PLC = Para.PLC;
            this.Grabber = Para.Grabber;
            this.LightSourceA = Para.LightSourceA;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            string LogTitle = "Base Align";

            if (!isAlm)
            {
                this.Info.General($"[{LogTitle}] {Msg}");
            }
            else
            {
                this.Info.Error($"[{LogTitle}] {Msg}");
            }
        }

        public void Start(BaseAlign_FlowPara Para, FlowStep Step = FlowStep.Init)
        {
            if (!FlowRun)
            {
                NowStep = Step;
                Thread mThread = new Thread(() => Flow(Para));
                mThread.Start();
            }
            else
            {
                FlowRun = false;
                Start(Para);
            }
        }

        public void Stop()
        {
            this.FlowRun = false;

            this.AutoFocus.Stop();
            this.AutoPlane.Stop();
        }


        private void Flow(BaseAlign_FlowPara Para)
        {
            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();

            FlowRun = true;

            AutoFocus_FlowPara AutoFoucs_Para = new AutoFocus_FlowPara();

            AutoPlane_FlowPara AutoPlane_Para = new AutoPlane_FlowPara();

            int PLC_X_Offest = 0;
            int PLC_X_ChangeDir_Times = 0;
            int Now_PLC_X_Offset_Dir = 0;
            int Tmp_PLC_X_Offset_Dir = 0;


            int Capture_Index = 0;
            double Current_PLC_X = 0;
            double[] Current_ScreenRatio = new double[2];

            int WD_Idx = 0;
            double AutoWD_Offset = 0.0;
            Para.OutputFolder = $@"{Para.OutputFolder}\BaseAlign\";

            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Init:
                        {
                            AutoFocus = new Task_AutoFocus(Grabber, Motor, Info);
                            AutoPlane = new Task_AutoPlane(MyMil.MilSystem, Grabber, Robot, Info, GlobalVar.SD.NormalAngle_0D);

                            Directory.CreateDirectory(Para.OutputFolder);
                            BaseAlign_FlowPara.WriteXML(Para, $@"{Para.OutputFolder}\Config.xml");

                            AutoFoucs_Para = Para.Para_AutoFocus;
                            AutoPlane_Para = Para.Para_AutoPlane;

                            SaveLog($"Flow Start");
                            NowStep = FlowStep.Check_Device;
                        }
                        break;

                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check Motor
                            bool Check_Motor = true;
                            if (!Para.BypassMotor)
                            {
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
                            }
                            Check &= Check_Motor;

                            //Check FW
                            bool Check_FW = true;
                            if (!Para.BypassFilter)
                            {
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
                            }
                            Check &= Check_FW;

                            //Check Robot
                            bool Check_Robot = true;
                            if (!Para.BypassRobot)
                            {
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
                            }
                            Check &= Check_Robot;


                            //Check PLC X
                            bool Check_PLC = true;
                            if (!Para.BypassPlcX)
                            {
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
                            }
                            Check &= Check_PLC;

                            //Check D65
                            bool Check_D65 = true;
                            if (!Para.BypassD65)
                            {
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
                            }
                            Check &= Check_D65;

                            //Check Grabber
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Grabber != null);
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

                            //Check WD Para
                            bool Check_WD_Para = true;
                            if (Para.Para_AutoWD.Enable && Para.Para_AutoWD.TargetList.Count == 0)
                            {
                                Check_WD_Para = false;
                                SaveLog($"Check Parameter Fail : AutoWD Enable , but WD List = 0", true);
                            }
                            Check &= Check_WD_Para;

                            if (Check)
                            {
                                SaveLog($"Check Parameter OK");
                                NowStep = FlowStep.Flow_Init;
                            }
                            else
                            {
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    #region 初始化

                    case FlowStep.Flow_Init:
                        {
                            PLC_X_Offest = 0;
                            PLC_X_ChangeDir_Times = 0;
                            Now_PLC_X_Offset_Dir = 0;
                            Tmp_PLC_X_Offset_Dir = 0;

                            Capture_Index = 0;
                            Current_PLC_X = 0;
                            Current_ScreenRatio = new double[] { 0, 0 };

                            Directory.CreateDirectory(Para.OutputFolder);

                            SaveLog($"Flow Init");

                            NowStep = FlowStep.Init_MotorMove;
                        }
                        break;

                    case FlowStep.Init_MotorMove:
                        {
                            if (Para.BypassMotor)
                            {
                                NowStep = FlowStep.Init_FilterMove;
                                SaveLog($"Init : Motor Bypass");
                                break;
                            }

                            int Focuser = Para.InitPos_Focuser;
                            int Aperture = Para.InitPos_Aperture;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Motor.Move(Focuser, Aperture, -1, -1);
                                        SaveLog($"Init : Motor Move , Focuser = {Focuser} , Aperture = {Aperture}");
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        this.Motor.Move(Focuser, -1, -1, -1);
                                        SaveLog($"Init : Motor Move , Focuser = {Focuser}");
                                    }
                                    break;
                            }

                            NowStep = FlowStep.Check_Init_Motor_InPos;
                        }
                        break;

                    case FlowStep.Check_Init_Motor_InPos:
                        {
                            if (!this.Motor.IsMoveProc())
                            {
                                switch (this.Motor.MoveFlowStatus())
                                {
                                    case UnitStatus.Finish:
                                        {
                                            NowStep = FlowStep.Init_FilterMove;
                                            SaveLog($"Init : Check Motor Move OK");
                                        }
                                        break;

                                    default:
                                        {
                                            NowStep = FlowStep.Alarm;
                                            SaveLog($"Init : Check Motor Move Fail", true);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Init_FilterMove:
                        {
                            if (Para.BypassFilter)
                            {
                                NowStep = FlowStep.Init_PlcMove;
                                SaveLog($"Init : Filter Bypass");
                                break;
                            }

                            int FW_XYZ = (int)Para.InitPos_FilterXYZ;
                            int FW_ND = (int)Para.InitPos_FilterND;

                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        GlobalVar.Device.XyzFilter.ChangeWheel(FW_XYZ);
                                        GlobalVar.Device.NdFilter.ChangeWheel(FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Motor.Move(-1, -1, FW_XYZ, FW_ND);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {

                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            SaveLog($"Init : Filter Move , XYZ = {FW_XYZ},ND = {FW_ND}");

                            NowStep = FlowStep.Check_Init_Filter_InPos;
                        }
                        break;

                    case FlowStep.Check_Init_Filter_InPos:
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
                                        if (!this.Motor.IsMoveProc())
                                        {
                                            XyzReady = (Motor.MoveFlowStatus() == UnitStatus.Finish && this.Motor.MoveStatus_FW1() == UnitStatus.Finish);
                                            NdReady = (Motor.MoveFlowStatus() == UnitStatus.Finish && this.Motor.MoveStatus_FW2() == UnitStatus.Finish);
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
                                SaveLog($"Init : Check Filter Move OK");
                                NowStep = FlowStep.Init_PlcMove;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Init : Check Filter Move Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.Init_PlcMove:
                        {
                            if (Para.BypassPlcX)
                            {
                                NowStep = FlowStep.Init_RobotMove;
                                SaveLog($"Init : PLC Bypass");
                                break;
                            }

                            double X = Para.InitPos_PlcX;
                            this.PLC.AbsMove(X);

                            SaveLog($"Init : PLC Move , X = {X}");

                            NowStep = FlowStep.Check_Init_PLC_InPos;
                        }
                        break;

                    case FlowStep.Check_Init_PLC_InPos:
                        {
                            if (!PLC.IsRun)
                            {
                                switch (PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"Init : Check PLC Move OK");
                                            NowStep = FlowStep.Init_RobotMove;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"Init : Check PLC Move Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Init_RobotMove:
                        {
                            if (Para.BypassRobot)
                            {
                                NowStep = FlowStep.Init_SetLight_D65;
                                SaveLog($"Init : Robot Bypass");
                                break;
                            }

                            double X = Para.InitPos_RobotX;
                            double Y = Para.InitPos_RobotY;
                            double Z = Para.InitPos_RobotZ;
                            double Roll = Para.InitPos_RobotRoll * Math.PI / 180D;
                            double Pitch = Para.InitPos_RobotPitch * Math.PI / 180D;
                            double Yaw = Para.InitPos_RobotYaw * Math.PI / 180D;

                            double Acc = GlobalVar.SD.UrRobot_Acc;
                            double Speed = GlobalVar.SD.UrRobot_Speed;
                            double Time = GlobalVar.SD.UrRobot_Time;
                            double Blendradius = GlobalVar.SD.UrRobot_Blendradius;

                            SaveLog($"Init : Robot Move , X = {X} , Y = {Y} , Z = {Z} , Roll = {Roll} , Pitch = {Pitch} , Yaw = {Yaw} " +
                            $", Acc = {Acc} , Speed = {Speed} , Time = {Time} , Blendradius = {Blendradius}");

                            bool Rtn = Robot.MoveL(X, Y, Z, Roll, Pitch, Yaw, Acc, Speed, Time, Blendradius);

                            if (Rtn)
                            {
                                SaveLog($"Robot Move , InPos");
                                NowStep = FlowStep.Init_SetLight_D65;
                            }
                            else
                            {
                                SaveLog($"Robot Move , Retry");
                            }
                        }
                        break;


                    case FlowStep.Init_SetLight_D65:
                        {
                            if (Para.BypassD65)
                            {
                                NowStep = FlowStep.Init_Finish;
                                SaveLog($"Init : D65 Bypass");
                                break;
                            }
                            int Channel = Para.Init_D65_Channel;
                            int Brightness = Para.Init_D65_Brightness;

                            D65.SetBrightnessFlow(Channel, Brightness);

                            SaveLog($"Init : D65 Light Init , Channel = {Channel} , Brightness = {Brightness}");

                            NowStep = FlowStep.Check_Init_SetLight_D65;

                            TM.SetDelay(10 * 1000);
                        }
                        break;

                    case FlowStep.Check_Init_SetLight_D65:
                        {
                            if (!D65.Run)
                            {
                                switch (D65.Status)
                                {
                                    case UnitStatus.Finish:
                                        {
                                            SaveLog($"Init : Check D65 Light Init OK");
                                            NowStep = FlowStep.Init_Finish;
                                        }
                                        break;

                                    case UnitStatus.Alarm:
                                        {
                                            SaveLog($"Init : Check D65 Light Init Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Init_Finish:
                        {
                            if (!Para.BaseAlignEable)
                            {
                                SaveLog($"BaseAlign Flow Bypass, Flow Finish");
                                NowStep = FlowStep.Finish;
                                break;
                            }

                            if (Para.BypassPlcX)
                            {
                                SaveLog($"PLC X Bypass, Flow Finish");
                                NowStep = FlowStep.Finish;
                                break;
                            }

                            if (Para.BypassMotor)
                            {
                                SaveLog($"Motor Bypass, Flow Finish");
                                NowStep = FlowStep.Finish;
                                break;
                            }

                            if (Para.BypassRobot)
                            {
                                SaveLog($"Robot Bypass, Flow Finish");
                                NowStep = FlowStep.Finish;
                                break;
                            }

                            Grabber.SetExposureTime(Para.Default_ExpTime);
                            NowStep = FlowStep.PLC_Move;
                        }
                        break;

                    #endregion

                    #region 移動軸 (PLC_X、Robot)

                    case FlowStep.PLC_Move:
                        {
                            double X = Para.InitPos_PlcX + PLC_X_Offest;

                            this.PLC.AbsMove(X);
                            Current_PLC_X = X;

                            SaveLog($"PLC Move , X = {X}");

                            NowStep = FlowStep.Check_PLC_InPos;
                        }
                        break;

                    case FlowStep.Check_PLC_InPos:
                        {
                            if (!PLC.IsRun)
                            {
                                switch (PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"PLC Move Suceed");
                                            NowStep = FlowStep.AutoFocus;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"PLC Move Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region AutoFocus + 確認占屏比

                    case FlowStep.AutoFocus:
                        {
                            AutoFocus.Start(AutoFoucs_Para);
                            SaveLog($"AutoFocus Start");

                            Thread.Sleep(1000);
                            NowStep = FlowStep.Check_AutoFocus;
                        }
                        break;

                    case FlowStep.Check_AutoFocus:
                        {
                            if (!AutoFocus.FlowRun)
                            {
                                switch (AutoFocus.Step)
                                {
                                    case Task_AutoFocus.FlowStep.Finish:
                                        {
                                            SaveLog($"AutoFocus Finish");
                                            NowStep = FlowStep.Cal_ScreenRatio;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.Error:
                                        {
                                            SaveLog($"AutoFocus Error", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.TimeOut:
                                        {
                                            SaveLog($"AutoFocus Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Cal_ScreenRatio:
                        {
                            Grabber.Grab();

                            string SaveFolder = $@"{Para.OutputFolder}\Debug\ScreenRatio_{Capture_Index}\";
                            ScreenRatioResult CalResult = CalScreenRatio(Grabber.grabImage, -1, SaveFolder, Para.Debug);

                            Current_ScreenRatio[0] = CalResult.Width; //W,H
                            Current_ScreenRatio[1] = CalResult.Height; //W,H

                            string W_RatioMsg = $"{Math.Round(Current_ScreenRatio[0] * 100.0, 3)} %";
                            string H_RatioMsg = $"{Math.Round(Current_ScreenRatio[1] * 100.0, 3)} %";

                            SaveLog($"Cal Screen Ratio , Width = {W_RatioMsg} , Height = {H_RatioMsg}");

                            Capture_Index++;

                            NowStep = FlowStep.Check_ScreenRatio;

                            //Record RowData
                            string FilePath = $@"{Para.OutputFolder}\RowData_ScreenRatio.csv";
                            int Index = Capture_Index;
                            double PLC_X = Current_PLC_X;
                            Task_AutoFocus.FlowResult AF_Result = AutoFocus.Result;
                            double[] ScreenRatio = Current_ScreenRatio;

                            WriteRowData_ScreenRatio(FilePath, Index, PLC_X, AF_Result, ScreenRatio);
                        }
                        break;

                    case FlowStep.Check_ScreenRatio:
                        {
                            double ScreenRatio = Para.ScreenRatio / 100.0;
                            double RatioTolerance = Para.ScreenRatio_Tolerance / 100.0;
                            double MaxRatio = ScreenRatio + RatioTolerance;
                            double MinRatio = ScreenRatio - RatioTolerance;
                            string Max_RatioMsg = $"{Math.Round(MaxRatio * 100.0, 3)} %";
                            string Min_RatioMsg = $"{Math.Round(MinRatio * 100.0, 3)} %";

                            SaveLog($"Screen Ratio Spec = ( {Min_RatioMsg} ~ {Max_RatioMsg})");

                            int[] CheckResult = new int[] { -1, -1 }; // 0 = InRange , 1 = Too Big , -1 = Too Small 

                            for (int i = 0; i < 2; i++)
                            {
                                if (Current_ScreenRatio[i] > MaxRatio)
                                {
                                    CheckResult[i] = 1;
                                    break;
                                }
                                else if (Current_ScreenRatio[i] < MinRatio)
                                {
                                    CheckResult[i] = -1;
                                }
                                else
                                {
                                    CheckResult[i] = 0;
                                }
                            }

                            if (CheckResult[0] == 1 || CheckResult[1] == 1) //任何過大 = 往後退
                            {
                                SaveLog($"Check Screen Ratio , Bigger Than Spec ( > {Max_RatioMsg} )", true);

                                Now_PLC_X_Offset_Dir = 1;
                                NowStep = FlowStep.PLC_Move;
                            }
                            else
                            {
                                if (CheckResult[0] == 0 || CheckResult[1] == 0) //都無過大，任何吻合 = 成功
                                {
                                    SaveLog($"Check Screen Ratio , OK");

                                    NowStep = FlowStep.AutoPlane;
                                }
                                else //都無過大，都無吻合 = 過小 = 往前
                                {
                                    SaveLog($"Check Screen Ratio , Smaller Than Spec ( < {Min_RatioMsg} )", true);

                                    Now_PLC_X_Offset_Dir = -1;
                                    NowStep = FlowStep.PLC_Move;
                                }
                            }

                            if (NowStep == FlowStep.PLC_Move)
                            {
                                if (Tmp_PLC_X_Offset_Dir != Now_PLC_X_Offset_Dir) //轉向
                                {
                                    Tmp_PLC_X_Offset_Dir = Now_PLC_X_Offset_Dir;
                                    PLC_X_ChangeDir_Times++;
                                }

                                PLC_X_Offest += (int)(Now_PLC_X_Offset_Dir * 50000 / (PLC_X_ChangeDir_Times));
                                SaveLog($"Cal PLC Offest = {PLC_X_Offest}");
                            }
                        }
                        break;

                    #endregion

                    #region AutoPlane + 確認水平

                    case FlowStep.AutoPlane:
                        {
                            AutoPlane.Start(AutoPlane_Para);
                            SaveLog($"AutoPlane Start");

                            Thread.Sleep(1000);

                            NowStep = FlowStep.Check_AutoPlane;
                        }
                        break;

                    case FlowStep.Check_AutoPlane:
                        {
                            if (!AutoPlane.FlowRun)
                            {
                                switch (AutoPlane.Proc_Step)
                                {
                                    case AutoPlaneAlign_Step.Finish:
                                        {
                                            SaveLog($"AutoPlane Finish");
                                            NowStep = FlowStep.AutoWD_CheckEnable;
                                        }
                                        break;

                                    case AutoPlaneAlign_Step.Error:
                                        {
                                            SaveLog($"AutoPlane Error", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case AutoPlaneAlign_Step.Stop:
                                        {
                                            SaveLog($"AutoPlane Stop", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case AutoPlaneAlign_Step.TimeOut:
                                        {
                                            SaveLog($"AutoPlane Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    #endregion

                    #region Auto WD

                    case FlowStep.AutoWD_CheckEnable:
                        {
                            if (Para.Para_AutoWD.Enable)
                            {
                                SaveLog($"AutoWD Start");

                                WD_Idx = 0;
                                Capture_Index = 0;

                                NowStep = FlowStep.AutoWD_AutoFocus;
                            }
                            else
                            {
                                NowStep = FlowStep.Finish;
                            }
                        }
                        break;

                    case FlowStep.AutoWD_AutoFocus:
                        {
                            AutoFocus.Start(AutoFoucs_Para);
                            SaveLog($"AutoFocus Start");

                            Thread.Sleep(1000);
                            NowStep = FlowStep.AutoWD_Check_AutoFocus;
                        }
                        break;

                    case FlowStep.AutoWD_Check_AutoFocus:
                        {
                            if (!AutoFocus.FlowRun)
                            {
                                switch (AutoFocus.Step)
                                {
                                    case Task_AutoFocus.FlowStep.Finish:
                                        {
                                            SaveLog($"AutoFocus Finish");
                                            NowStep = FlowStep.AutoWD_CalWD;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.Error:
                                        {
                                            SaveLog($"AutoFocus Error", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.TimeOut:
                                        {
                                            SaveLog($"AutoFocus Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.AutoWD_CalWD:
                        {
                            Grabber.Grab();

                            //Target
                            double TargetWD = Para.Para_AutoWD.TargetList[WD_Idx];
                            TargetWD = Math.Round(TargetWD, 3);
                            double Tolerance = Para.Para_AutoWD.Tolerance;
                            double Range_Max = TargetWD + Tolerance;
                            double Range_Min = TargetWD - Tolerance;

                            string SaveFolder = $@"{Para.OutputFolder}\Debug\AutoWD_{TargetWD}mm_{Capture_Index}\";
                            ScreenRatioResult CalResult = CalScreenRatio(Grabber.grabImage, -1, SaveFolder, Para.Debug);

                            //Current
                            double ImageX = CalResult.Width;
                            double RealX = Para.Para_AutoWD.Real_X_Size;
                            double f = GlobalVar.SD.Camera_FocalLength;
                            double PixelSize = GlobalVar.SD.Camera_PixelSize;
                            double CurrentWD = RealX / ImageX * f / PixelSize * 1000.0;
                            CurrentWD = Math.Round(CurrentWD, 3);

                            bool Check = (CurrentWD >= Range_Min && CurrentWD <= Range_Max);

                            SaveLog($"Cal WD , TargetWD = {TargetWD}mm , CurrentWD = {CurrentWD}mm , Check = {Check}");

                            Capture_Index++;

                            if (Check)
                            {
                                SaveLog($"WD Rnage = ({Range_Min} ~ {Range_Max}) , Check OK");

                                NowStep = FlowStep.AutoWD_CheckTargetList;
                            }
                            else
                            {
                                AutoWD_Offset = TargetWD - CurrentWD;
                                SaveLog($"WD Rnage = ({Range_Min} ~ {Range_Max}) , Check Fail , Diff = {AutoWD_Offset}mm");

                                NowStep = FlowStep.AutoWD_PLC_Move;
                            }

                            //Record RowData
                            string FilePath = $@"{Para.OutputFolder}\RowData_AutoWD.csv";
                            double PLC_X = Current_PLC_X;
                            Task_AutoFocus.FlowResult AF_Result = AutoFocus.Result;

                            WriteRowData_AutoWD(FilePath, TargetWD, CurrentWD, PLC_X, AF_Result, Check);
                        }
                        break;

                    case FlowStep.AutoWD_PLC_Move:
                        {
                            double Get_PLC_X = PLC.Plc2Pc.AxisPos;

                            if (Get_PLC_X != 0)
                            {
                                double X = Get_PLC_X + AutoWD_Offset;
                                this.PLC.AbsMove(X);
                                Current_PLC_X = X;

                                SaveLog($"PLC Move , X = {X}");

                                NowStep = FlowStep.AutoWD_Check_PLC_InPos;
                            }
                        }
                        break;

                    case FlowStep.AutoWD_Check_PLC_InPos:
                        {
                            if (!PLC.IsRun)
                            {
                                switch (PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"Check PLC Move OK");
                                            NowStep = FlowStep.AutoWD_AutoFocus;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"Check PLC Move Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.AutoWD_CheckTargetList:
                        {
                            WD_Idx++;
                            Capture_Index = 0;

                            if (WD_Idx < Para.Para_AutoWD.TargetList.Count)
                            {
                                NowStep = FlowStep.AutoWD_PLC_Move;
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
                            Flow_Result = new FlowResult {
                                Focuser = this.Motor.GetPosition(MotorMember.Focuser),
                                Aperture = this.Motor.GetPosition(MotorMember.Aperture),
                                PlcX = PLC.Plc2Pc.AxisPos,
                                RobotX = Robot.m_ClsUrStatus.TcpPose[0],
                                RobotY = Robot.m_ClsUrStatus.TcpPose[1],
                                RobotZ = Robot.m_ClsUrStatus.TcpPose[2],
                                RobotRoll = Robot.m_ClsUrStatus.TcpPose[3] * 180D / Math.PI,
                                RobotPitch = Robot.m_ClsUrStatus.TcpPose[4] * 180D / Math.PI,
                                RobotYaw = Robot.m_ClsUrStatus.TcpPose[5] * 180D / Math.PI,
                            };

                            Flow_Result.SaveToXml($@"{Para.OutputFolder}\Result.xml");

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
            }
        }

        public enum FlowStep
        {
            Init = 0,
            Check_Device,
            Check_Parameter,

            //Init
            Flow_Init,
            Init_MotorMove,
            Check_Init_Motor_InPos,
            Init_FilterMove,
            Check_Init_Filter_InPos,
            Init_PlcMove,
            Check_Init_PLC_InPos,
            Init_RobotMove,

            Init_SetLight_D65,
            Check_Init_SetLight_D65,

            Init_Finish,

            //移動軸 (PLC_X、Robot)
            PLC_Move,
            Check_PLC_InPos,

            //AutoFocus + 確認占屏比
            AutoFocus = 200,
            Check_AutoFocus,
            Cal_ScreenRatio,
            Record_RowData,
            Check_ScreenRatio,

            //AutoPlane + 確認水平
            AutoPlane = 300,
            Check_AutoPlane,

            //Auto WD
            AutoWD_CheckEnable = 400,
            AutoWD_AutoFocus,
            AutoWD_Check_AutoFocus,
            AutoWD_CalWD,
            AutoWD_PLC_Move,
            AutoWD_Check_PLC_InPos,
            AutoWD_CheckTargetList,

            //Finish
            Finish = 5000,
            Alarm = 9999,
        }

        public class FlowResult
        {
            public double PlcX { get; set; } = 0;
            public double RobotX { get; set; } = 0;
            public double RobotY { get; set; } = 0;
            public double RobotZ { get; set; } = 0;
            public double RobotRoll { get; set; } = 0;
            public double RobotPitch { get; set; } = 0;
            public double RobotYaw { get; set; } = 0;
            public double Focuser { get; set; } = 0;
            public double Aperture { get; set; } = 0;

            public void SaveToXml(string filePath)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(FlowResult));
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        serializer.Serialize(writer, this);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving to XML file: {ex.Message}");
                }
            }
        }


        #region Method

        public ScreenRatioResult CalScreenRatio(MIL_ID Img, int Threshold, string SaveFolder, bool Debug = false) //W,H
        {
            MIL_ID blobContext = MIL.M_NULL;
            MIL_ID blobResult = MIL.M_NULL;
            MIL_ID mil_16U_SrcImag = MIL.M_NULL;

            int blobCount = 0;
            double Blob_H = 0.0;
            double Blob_W = 0.0;

            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;

            ScreenRatioResult Result = new ScreenRatioResult();

            try
            {
                SizeX = MIL.MbufInquire(Img, MIL.M_SIZE_X, MIL.M_NULL);
                SizeY = MIL.MbufInquire(Img, MIL.M_SIZE_Y, MIL.M_NULL);
                // Blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult);
                //搜尋最大面積的 Blob
                MIL.MblobControl(blobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(blobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
                MIL.MblobControl(blobContext, MIL.M_SAVE_RUNS, MIL.M_ENABLE);

                MIL.MbufAlloc2d(MyMil.MilSystem, SizeX, SizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_16U_SrcImag);

                MIL.MbufCopy(Img, mil_16U_SrcImag);

                MIL.MimDilate(mil_16U_SrcImag, mil_16U_SrcImag, 5, MIL.M_GRAYSCALE);
                MIL.MimErode(mil_16U_SrcImag, mil_16U_SrcImag, 5, MIL.M_GRAYSCALE);

                if (Debug)
                {
                    Directory.CreateDirectory(SaveFolder);
                    MIL.MbufSave($@"{SaveFolder}/Source.tif", mil_16U_SrcImag);
                }

                if (Threshold == -1)
                {
                    MIL_INT BinValue = MIL.MimBinarize(mil_16U_SrcImag, MIL.M_NULL, MIL.M_BIMODAL, MIL.M_NULL, MIL.M_NULL);
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_GREATER_OR_EQUAL, BinValue, MIL.M_NULL);
                }
                else
                {
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_FIXED + MIL.M_LESS, Threshold, MIL.M_NULL);
                }

                if (Debug)
                {
                    MIL.MbufSave($@"{SaveFolder}/Binary.tif", mil_16U_SrcImag);
                }

                MIL.MblobCalculate(blobContext, mil_16U_SrcImag, MIL.M_NULL, blobResult);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);
                MIL.MblobDraw(MIL.M_DEFAULT, blobResult, mil_16U_SrcImag, MIL.M_DRAW_HOLES, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                if (Debug)
                {
                    MIL.MbufSave($@"{SaveFolder}/Blob.tif", mil_16U_SrcImag);
                }

                if (blobCount > 0)
                {
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_X, ref Blob_W);
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_Y, ref Blob_H);

                    Result.ScreenRatio_W = (double)Blob_W / (double)SizeX;
                    Result.ScreenRatio_H = (double)Blob_H / (double)SizeY;
                    Result.Width = Blob_W;
                    Result.Height = Blob_H;
                }
                else
                {
                    Result = new ScreenRatioResult();
                }
            }
            catch
            {
                Result = new ScreenRatioResult();
            }
            finally
            {
                if (mil_16U_SrcImag != MIL.M_NULL)
                {
                    MIL.MbufFree(mil_16U_SrcImag);
                    mil_16U_SrcImag = MIL.M_NULL;
                }

                if (blobContext != MIL.M_NULL)
                {
                    MIL.MblobFree(blobContext);
                    blobContext = MIL.M_NULL;
                }

                if (blobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(blobResult);
                    blobResult = MIL.M_NULL;
                }
            }

            return Result;
        }

        public class ScreenRatioResult
        {
            public double ScreenRatio_H { get; set; } = 0.0;
            public double ScreenRatio_W { get; set; } = 0.0;
            public double Width { get; set; } = 0.0;
            public double Height { get; set; } = 0.0;
        }

        private bool WriteRowData_ScreenRatio(string SavePath, int Index, double PLC_X_Pos, Task_AutoFocus.FlowResult Focuser_Pos, double[] ScreenRatio)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Index");

                    TitleList.Add("PLC X Pos");

                    TitleList.Add("Focuser Pos");
                    TitleList.Add("Focuser Value");
                    TitleList.Add("Focuser Pos - 2");
                    TitleList.Add("Focuser Value -2");

                    TitleList.Add("Screen Ratio - X");
                    TitleList.Add("Screen Ratio - Y");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{Index}");

                InfoList.Add($"{PLC_X_Pos:0.0}");

                InfoList.Add($"{Focuser_Pos.FocusPos}");
                InfoList.Add($"{Focuser_Pos.FocusValue}");
                InfoList.Add($"{Focuser_Pos.FocusPos_2}");
                InfoList.Add($"{Focuser_Pos.FocusValue_2}");

                InfoList.Add($"{Math.Round(ScreenRatio[0] * 100.0, 3)} % ");
                InfoList.Add($"{Math.Round(ScreenRatio[1] * 100.0, 3)} % ");

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

        private bool WriteRowData_AutoWD(string SavePath, double TargetWD, double CurrentWD, double PLC_X_Pos, Task_AutoFocus.FlowResult Focuser_Pos, bool Status)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Target WD");
                    TitleList.Add("Current WD");
                    TitleList.Add("PLC X Pos");
                    TitleList.Add("Focuser Pos");
                    TitleList.Add("Focuser Value");
                    TitleList.Add("Focuser Pos - 2");
                    TitleList.Add("Focuser Value -2");
                    TitleList.Add("Status");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{TargetWD}");
                InfoList.Add($"{CurrentWD}");
                InfoList.Add($"{PLC_X_Pos:0.0}");
                InfoList.Add($"{Focuser_Pos.FocusPos}");
                InfoList.Add($"{Focuser_Pos.FocusValue}");
                InfoList.Add($"{Focuser_Pos.FocusPos_2}");
                InfoList.Add($"{Focuser_Pos.FocusValue_2}");
                InfoList.Add($"{Status}");

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

        #endregion
    }

    public class BaseAlign_FlowPara
    {
        //Bypass

        #region Bypass Setting

        [Category("A. Bypass Setting"), DisplayName("02. Motor Bypass"), Description("Motor Bypass")]
        public bool BypassMotor { get; set; } = false;

        [Category("A. Bypass Setting"), DisplayName("03. Filter Bypass"), Description("Focuser Bypass")]
        public bool BypassFilter { get; set; } = false;

        [Category("A. Bypass Setting"), DisplayName("04. Robot Bypass"), Description("Robot Bypass")]
        public bool BypassRobot { get; set; } = false;

        [Category("A. Bypass Setting"), DisplayName("05. PlcX Bypass"), Description("PlcX Bypass")]
        public bool BypassPlcX { get; set; } = false;

        [Category("A. Bypass Setting"), DisplayName("06. D65 Bypass"), Description("D65 Bypass")]
        public bool BypassD65 { get; set; } = false;

        #endregion

        #region Init Pos

        //Init Pos
        [Category("B. Init Flow"), DisplayName("01. InitPos - Aperture"), Description("InitPos - Aperture")]
        public int InitPos_Aperture { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("02. InitPos - Focuser"), Description("InitPos - Focuser")]
        public int InitPos_Focuser { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("11. InitPos - Filter ND"), Description("InitPos - FilterND")]
        [TypeConverter(typeof(EnumTypeConverter))]
        public FW_ND_Remark InitPos_FilterND { get; set; } = FW_ND_Remark.ND_0_25;

        [Category("B. Init Flow"), DisplayName("12. InitPos - Filter XYZ"), Description("InitPos - FilterXYZ")]
        public FW_XYZ_Remark InitPos_FilterXYZ { get; set; } = FW_XYZ_Remark.X;

        [Category("B. Init Flow"), DisplayName("21. InitPos - Robot X"), Description("InitPos -  Robot X")]
        public double InitPos_RobotX { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("22. InitPos - Robot Y"), Description("InitPos -  Robot Y")]
        public double InitPos_RobotY { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("23. InitPos - Robot Z"), Description("InitPos -  Robot Z")]
        public double InitPos_RobotZ { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("24. InitPos - Robot Roll"), Description("InitPos -  Robot Roll")]
        public double InitPos_RobotRoll { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("25. InitPos - Robot Pitch"), Description("InitPos -  Robot Pitch")]
        public double InitPos_RobotPitch { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("26. InitPos - Robot Yaw"), Description("InitPos -  Robot Yaw")]
        public double InitPos_RobotYaw { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("31. InitPos - PLC X"), Description("InitPos -  PLC X")]
        public double InitPos_PlcX { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("41. Init - D65 Channel"), Description("Init - D65 Channel")]
        public int Init_D65_Channel { get; set; } = 0;

        [Category("B. Init Flow"), DisplayName("42. Init - D65 Brightness"), Description("Init - D65 Brightness")]
        public int Init_D65_Brightness { get; set; } = 0;

        #endregion

        #region BaseAlign

        [Category("C. BaseAlign Para"), DisplayName("01. Eable"), Description("Eable")]
        public bool BaseAlignEable { get; set; } = true;

        [Category("C. BaseAlign Para"), DisplayName("02. Output Folder"), Description("Output Folder"), Browsable(false), XmlIgnore]
        public string OutputFolder { get; set; } = "";

        [Category("C. BaseAlign Para"), DisplayName("02. Debug"), Description("Debug (SaveImage Flag)")]
        public bool Debug { get; set; } = true;

        [Category("C. BaseAlign Para"), DisplayName("11. Default ExpTime"), Description("ScreenRatio")]
        public double Default_ExpTime { get; set; } = 200000; // N %

        [Category("C. BaseAlign Para"), DisplayName("12. ScreenRatio"), Description("ScreenRatio")]
        public int ScreenRatio { get; set; } = 80; // N %

        [Category("C. BaseAlign Para"), DisplayName("13. ScreenRatio Tolerance"), Description("ScreenRatio Tolerance")]
        public int ScreenRatio_Tolerance { get; set; } = 5; // N %

        [Category("C. BaseAlign Para"), DisplayName("21. AutoFocus Para"), Description("AutoFocus Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AutoFocus_FlowPara Para_AutoFocus { get; set; } = new AutoFocus_FlowPara();

        [Category("C. BaseAlign Para"), DisplayName("31. AutoPlane Para"), Description("AutoPlane Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AutoPlane_FlowPara Para_AutoPlane { get; set; } = new AutoPlane_FlowPara();

        [Category("C. BaseAlign Para"), DisplayName("41. AutoWD Para"), Description("AutoWD Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WD_Para Para_AutoWD { get; set; } = new WD_Para();

        #endregion

        public void Clone(BaseAlign_FlowPara Source)
        {
            //Bypass
            this.BypassMotor = Source.BypassMotor;
            this.BypassFilter = Source.BypassFilter;
            this.BypassRobot = Source.BypassRobot;
            this.BypassPlcX = Source.BypassPlcX;
            this.BypassD65 = Source.BypassD65;

            //Init
            this.InitPos_Aperture = Source.InitPos_Aperture;
            this.InitPos_Focuser = Source.InitPos_Focuser;
            this.InitPos_FilterND = Source.InitPos_FilterND;
            this.InitPos_FilterXYZ = Source.InitPos_FilterXYZ;
            this.InitPos_RobotX = Source.InitPos_RobotX;
            this.InitPos_RobotY = Source.InitPos_RobotY;
            this.InitPos_RobotZ = Source.InitPos_RobotZ;
            this.InitPos_RobotRoll = Source.InitPos_RobotRoll;
            this.InitPos_RobotPitch = Source.InitPos_RobotPitch;
            this.InitPos_RobotYaw = Source.InitPos_RobotYaw;
            this.InitPos_PlcX = Source.InitPos_PlcX;
            this.Init_D65_Channel = Source.Init_D65_Channel;
            this.Init_D65_Brightness = Source.Init_D65_Brightness;

            //BaseAlign
            this.BaseAlignEable = Source.BaseAlignEable;
            this.OutputFolder = Source.OutputFolder;
            this.Debug = Source.Debug;
            this.Default_ExpTime = Source.Default_ExpTime;

            this.ScreenRatio = Source.ScreenRatio;
            this.ScreenRatio_Tolerance = Source.ScreenRatio_Tolerance;

            this.Para_AutoFocus.Clone(Source.Para_AutoFocus);
            this.Para_AutoPlane.Clone(Source.Para_AutoPlane);
            this.Para_AutoWD.Clone(Source.Para_AutoWD);
        }

        public override string ToString()
        {
            return "...";
        }

        public static void WriteXML(BaseAlign_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(BaseAlign_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static BaseAlign_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                BaseAlign_FlowPara m;

                serializer = new XmlSerializer(typeof(BaseAlign_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (BaseAlign_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }

        public class WD_Para
        {
            [Category("WD Para"), DisplayName("01. Eable"), Description("Eable")]
            public bool Enable { get; set; } = false;

            [Category("WD Para"), DisplayName("02. Real Width (mm)"), Description("待測物實際長度 - X (mm)")]
            public double Real_X_Size { get; set; } = 0.0;

            [Category("WD Para"), DisplayName("03. Target WD (mm)"), Description("目標工作距離 (mm)")]
            public List<double> TargetList { get; set; } = new List<double>();

            [Category("WD Para"), DisplayName("04. WD Tolerance (mm)"), Description("WD Tolerance (mm)")]
            public double Tolerance { get; set; } = 5; // N %

            public void Clone(WD_Para Source)
            {
                this.Enable = Source.Enable;
                this.Real_X_Size = Source.Real_X_Size;

                TargetList = new List<double>();

                foreach (double WD in Source.TargetList)
                {
                    TargetList.Add(WD);
                }

                this.Tolerance = Source.Tolerance;
            }

            public override string ToString()
            {
                return "...";
            }
        }
    }


}
