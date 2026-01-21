using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using BaseTool;

namespace HardwareManager
{
    public class AiryUnitCtrl_211_USB : IMotorControl
    {
        enum AiryMoveStep
        {
            Init,
            CheckOriPos,
            GetOriPos,
            MovePos,
            CheckPos,
            CheckInPos,
            Finish,
            Alarm,
        }

        public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;
        public event Action<string, bool> WriteLog;

        private InfoManager info;

        private SerialPort Motor_Port;
        private SerialPort FW_1_Port;
        private SerialPort FW_2_Port;

        private bool isRefreshData = false;
        private bool Motor_isWork = false;
        private bool FW_1_isWork = false;
        private bool FW_2_isWork = false;

        public bool isMoveProc = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
        private UnitStatus moveStatus_FW1 = UnitStatus.Idle;
        private UnitStatus moveStatus_FW2 = UnitStatus.Idle;
        private UnitStatus flowStatus = UnitStatus.Idle;


        private int Focus_limitF = 3200000;
        private int Focus_limitR = 0;

        private int Aper_limitF = 1700000;
        private int Aper_limitR = 0;

        private MotorInfo _Focuser = new MotorInfo();
        private MotorInfo _Aperture = new MotorInfo();
        private MotorInfo _FW1 = new MotorInfo();
        private MotorInfo _FW2 = new MotorInfo();

        private AiryMoveStep moveStep = AiryMoveStep.Init;

        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.Airy_4In1_Wheel_Motor;

        public AiryUnitCtrl_211_USB(InfoManager info)
        {
            this.info = info;

            this.Motor_Port = new SerialPort();
            Motor_Port.DataReceived -= Motor_Port_DataReceived;
            Motor_Port.DataReceived += Motor_Port_DataReceived;

            this.FW_1_Port = new SerialPort();
            FW_1_Port.DataReceived -= FW_1_Port_DataReceived;
            FW_1_Port.DataReceived += FW_1_Port_DataReceived;

            this.FW_2_Port = new SerialPort();
            FW_2_Port.DataReceived -= FW_2_Port_DataReceived;
            FW_2_Port.DataReceived += FW_2_Port_DataReceived;

            this.Motor_isWork = false;
            this.FW_1_isWork = false;
            this.FW_2_isWork = false;
        }

        #region --- Interface 成員函式 ---

        #region --- MoveStatus_Focuser ---

        public UnitStatus MoveStatus_Focuser()
        {
            return this.moveStatus_Focuser;
        }

        #endregion --- MoveStatus_Focuser ---

        #region --- MoveStatus_Aperture ---

        public UnitStatus MoveStatus_Aperture()
        {
            return this.moveStatus_Aperture;
        }

        #endregion --- MoveStatus_Aperture ---

        #region --- MoveStatus_FW1 ---

        public UnitStatus MoveStatus_FW1()
        {
            return this.moveStatus_FW1;
        }

        #endregion --- MoveStatus_FW1 ---

        #region --- MoveStatus_FW2 ---

        public UnitStatus MoveStatus_FW2()
        {
            return this.moveStatus_FW2;
        }

        #endregion --- MoveStatus_FW2 ---


        #region --- MoveFlowStatus ---

        public UnitStatus MoveFlowStatus()
        {
            return this.flowStatus;
        }

        #endregion --- MoveStatus_FW2 ---

        #region --- IsWork ---

        public bool IsWork()
        {
            return this.Motor_isWork && this.FW_1_isWork && FW_2_isWork;
        }

        #endregion --- IsWork ---

        #region --- IsMoveProc ---

        public bool IsMoveProc()
        {
            return this.isMoveProc;
        }

        #endregion --- IsMoveProc ---        

        #region --- NowPos_Focuser ---

        public int NowPos_Focuser()
        {
            this.CheckDevice();
            return this._Focuser.Position;
        }

        #endregion --- NowPos_Focuser ---

        #region --- NowPos_Aperture ---

        public int NowPos_Aperture()
        {
            this.CheckDevice();
            return this._Aperture.Position;
        }

        #endregion --- NowPos_Aperture ---

        #region --- NowPos_FW1 ---

        public virtual int NowPos_FW1()
        {
            this.CheckDevice();
            return this._FW1.Position / 25600;
        }

        #endregion --- NowPos_FW1 ---

        #region --- NowPos_FW2 ---

        public virtual int NowPos_FW2()
        {
            this.CheckDevice();
            return this._FW2.Position / 25600;
        }

        #endregion --- NowPos_FW2 ---

        #region --- NowVelocity_Focuser ---

        public double NowVelocity_Focuser()
        {
            this.CheckDevice();
            return this._Focuser.Speed;
        }

        #endregion --- NowVelocity_Focuser ---

        #region --- NowVelocity_Aperture ---

        public double NowVelocity_Aperture()
        {
            this.CheckDevice();
            return this._Aperture.Speed;
        }

        #endregion --- NowVelocity_Aperture ---

        #region --- IsHome_Focuser ---

        public bool IsHome_Focuser()
        {
            this.CheckDevice();
            return this._Focuser.HomeState;
        }

        #endregion --- IsHome_Focuser ---

        #region --- IsHome_Aperture ---

        public bool IsHome_Aperture()
        {
            this.CheckDevice();
            return this._Aperture.HomeState;
        }

        #endregion --- IsHome_Aperture ---

        #region --- LimitF ---

        public int Focus_LimitF()
        {
            return this.Focus_limitF;
        }

        public int Aper_LimitF()
        {
            return this.Aper_limitF;
        }

        #endregion --- LimitF ---

        #region --- LimitR ---

        public int Focus_LimitR()
        {
            return this.Focus_limitR;
        }

        public int Aper_LimitR()
        {
            return this.Aper_limitR;
        }

        #endregion --- LimitR ---

        #endregion --- Property ---

        #region --- 方法函式 ---"

        #region --- isConnect ---

        public bool isConnect()
        {
            return this.Motor_Port.IsOpen && this.FW_1_Port.IsOpen && this.FW_2_Port.IsOpen;
        }

        #endregion --- isConnect ---

        #region --- SaveLog ---
        public void SaveLog(string Log, bool isAlm = false)
        {
            WriteLog?.Invoke($"[Auto Focus] {Log}", isAlm);
        }

        #endregion --- SaveLog ---

        #region --- Motor Function ---

        #region --- SetSpeed ---
        private bool SetSpeed(MotorMember Member, double Speed)
        {
            if (!Motor_Port.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        Motor_Port.WriteLine($":V{Speed}#");
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        Motor_Port.WriteLine($":v{Speed}#");
                    }
                    break;

                default:
                    {
                        return false;
                    }
            }

            return true;
        }
        #endregion

        #region --- SetOverride ---
        private bool SetOverride(MotorMember Member, bool Switch)
        {
            if (!Motor_Port.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Switch)
                        {
                            Motor_Port.WriteLine(":O1#");
                        }
                        else
                        {
                            Motor_Port.WriteLine(":O0#");
                        }
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Switch)
                        {
                            Motor_Port.WriteLine(":o1#");
                        }
                        else
                        {
                            Motor_Port.WriteLine(":o0#");
                        }
                    }
                    break;

                default:
                    {
                        return false;
                    }
            }

            return true;
        }
        #endregion        

        #region --- CheckDevice ---
        public bool CheckDevice()
        {
            //if (!AiryPort.IsOpen)
            //{
            //    SaveLog($"Port is not Open", true);
            //    return false;
            //}
            isRefreshData = false;
            Motor_Port.WriteLine(":i#");
            FW_1_Port.WriteLine(":f#");
            FW_2_Port.WriteLine(":f#");


            System.Threading.Thread.Sleep(400);

            return true;
        }
        #endregion

        #region --- CloseLed ---
        private bool CloseLed()
        {
            if (!Motor_Port.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            Motor_Port.WriteLine(":h#");
            return true;
        }
        #endregion

        #endregion

        #region --- Open ---

        public void Open()
        {

        }

        public void Open(string Com_Port)
        {

        }

        public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
        {
            try
            {
                Motor_Open(Com_Port);
                FW_1_Open(Com_Port_2);
                FW_2_Open(Com_Port_3);
            }
            catch (Exception ex)
            {
                SaveLog($"Port Open Fail - {ex.Message}", true);
            }
        }

        public void Motor_Open(string Com_Port)
        {
            try
            {
                if (!Motor_Port.IsOpen)
                {
                    Motor_Port.BaudRate = 500000;
                    Motor_Port.PortName = Com_Port;
                    Motor_Port.Open();

                    Thread.Sleep(500);
                    CloseLed();

                    SaveLog($"Motor Open Port : {Com_Port}");
                }
                else
                {
                    Motor_Port.Close();
                    SaveLog($"Motor Port Close");
                    Motor_Open(Com_Port);
                }

                this.SetOverride(MotorMember.Focuser, true);
                Thread.Sleep(10);
                this.SetOverride(MotorMember.Aperture, true);

                //this.Home(AiryMember.All);

                this.Motor_isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"Motor Port Open Fail - {ex.Message}", true);
            }
        }

        public void FW_1_Open(string Com_Port)
        {
            try
            {
                if (!FW_1_Port.IsOpen)
                {
                    FW_1_Port.BaudRate = 9600;
                    FW_1_Port.PortName = Com_Port;
                    FW_1_Port.Open();

                    Thread.Sleep(500);

                    SaveLog($"FW 1 Open Port : {Com_Port}");
                }
                else
                {
                    FW_1_Port.Close();
                    SaveLog($"FW 1 Port Close");
                    FW_1_Open(Com_Port);
                }

                this.FW_1_isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"FW 1 Open Fail - {ex.Message}", true);
            }
        }

        public void FW_2_Open(string Com_Port)
        {
            try
            {
                if (!FW_2_Port.IsOpen)
                {
                    FW_2_Port.BaudRate = 9600;
                    FW_2_Port.PortName = Com_Port;
                    FW_2_Port.Open();

                    Thread.Sleep(500);

                    SaveLog($"FW 2 Open Port : {Com_Port}");
                }
                else
                {
                    FW_2_Port.Close();
                    SaveLog($"FW 2 Port Close");
                    FW_2_Open(Com_Port);
                }

                this.FW_2_isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"FW 2 Open Fail - {ex.Message}", true);
            }
        }

        #endregion --- Open ---

        #region --- Close ---

        public void Close()
        {
            if (this.Motor_Port.IsOpen)
            {
                this.Motor_Port.Close();
                this.Motor_isWork = false;

                this.FW_1_Port.Close();
                this.FW_1_isWork = false;

                this.FW_2_Port.Close();
                this.FW_2_isWork = false;

            }
            this.isMoveProc = false;
        }

        #endregion --- Close ---

        #region --- AiryPort_DataReceived ---

        private void Motor_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();

            string CMD = indata.Substring(0, 2);

            if (CMD == ":i")
            {
                string Para = indata.Substring(2, indata.Length - 4);

                string[] words = Para.Split(',');

                if (words.Length >= 10)
                {
                    _Focuser.Position = Convert.ToInt32(words[0]);
                    _Focuser.Speed = Convert.ToDouble(words[1]);

                    _Aperture.Position = Convert.ToInt32(words[2]);
                    _Aperture.Speed = Convert.ToDouble(words[3]);

                    _Focuser.PinHomeState = (words[4] == "5");
                    _Aperture.PinHomeState = (words[5] == "5");

                    _Focuser.HomeState = (words[6] == "5");
                    _Aperture.HomeState = (words[7] == "5");

                    _Focuser.OverrideState = (words[8] == "1");
                    _Aperture.OverrideState = (words[9] == "1");

                    this.isRefreshData = true;
                }

                UpdateStatus?.Invoke(_Focuser, _Aperture, _FW1, _FW2);
            }
        }

        private void FW_1_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();

            string CMD = indata.Substring(0, 2);

            if (CMD == ":f")
            {
                string Para = indata.Substring(2, indata.Length - 4);
                int Pos = Convert.ToInt32(Para);
                _FW1.Position = Pos;
                this.isRefreshData = true;
            }

            UpdateStatus?.Invoke(_Focuser, _Aperture, _FW1, _FW2);
        }

        private void FW_2_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();

            string CMD = indata.Substring(0, 2);

            if (CMD == ":f")
            {
                string Para = indata.Substring(2, indata.Length - 4);
                int Pos = Convert.ToInt32(Para);
                _FW2.Position = Pos;
                this.isRefreshData = true;
            }

            UpdateStatus?.Invoke(_Focuser, _Aperture, _FW1, _FW2);
        }

        #endregion --- AiryPort_DataReceived ---

        #region --- HomeAll ---

        public void HomeAll()
        {
            this.Home(MotorMember.All);
        }

        #endregion --- HomeAll ---

        #region --- Move_Proc ---
        private void Move_Proc(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut)
        {
            // Pos = -1 ==> Bypass 

            TimeManager TM = new TimeManager();
            TM.SetDelay(TimeOut);
            this.moveStep = AiryMoveStep.Init;

            bool[] isActive = new bool[4];

            int[] TargetPos = new int[4];

            bool[] isReverse = new bool[2];

            while (this.isMoveProc)
            {
                if (TM.IsTimeOut())
                {
                    SaveLog($"Move Flow Running", true);
                    this.isMoveProc = false;
                    break;
                }

                switch (this.moveStep)
                {
                    case AiryMoveStep.Init:
                        {
                            flowStatus = UnitStatus.Running;

                            isActive = new bool[]
                            {
                                FocuserPos >= 0,
                                AperturePos >= 0,
                                FW1Pos >= 0,
                                FW2Pos >= 0,
                            };

                            TargetPos = new int[]
                            {
                                FocuserPos,
                                AperturePos,
                                FW1Pos,
                                FW2Pos,
                            };

                            SaveLog($"Move Flow Start , Focuser = {FocuserPos} , Aperture = {AperturePos} , FW1 = {FW1Pos} , FW2 = {FW2Pos}");

                            if (isActive[0])
                                this.moveStatus_Focuser = UnitStatus.Running;
                            else if (isActive[1])
                                this.moveStatus_Aperture = UnitStatus.Running;
                            else if (isActive[2])
                                this.moveStatus_FW1 = UnitStatus.Running;
                            else if (isActive[3])
                                this.moveStatus_FW2 = UnitStatus.Running;

                            //調焦需移動，才紀錄原始位置 (補背隙)
                            if (isActive[0] || isActive[1])
                            {
                                this.moveStep = AiryMoveStep.CheckOriPos;
                            }
                            else
                            {
                                this.moveStep = AiryMoveStep.MovePos;
                            }
                        }
                        break;

                    case AiryMoveStep.CheckOriPos:
                        {
                            bool Rtn = this.CheckDevice();

                            if (Rtn)
                            {
                                this.moveStep = AiryMoveStep.GetOriPos;
                            }
                            else
                            {
                                this.moveStep = AiryMoveStep.Alarm;
                            }
                        }
                        break;

                    case AiryMoveStep.GetOriPos:
                        {
                            if (isRefreshData)
                            {
                                int[] OriPos =
                                {
                                    _Focuser.Position,
                                    _Aperture.Position
                                };

                                for (int i = 0; i < 2; i++)
                                {
                                    if (isActive[i])
                                    {
                                        isReverse[i] = TargetPos[i] < OriPos[i];

                                        if (isReverse[i])
                                        {
                                            TargetPos[i] -= 3000;
                                        }
                                    }
                                }

                                SaveLog($"Get Ori Pos , Focuser = {OriPos[0]} , Aperture = {OriPos[1]}");

                                this.moveStep = AiryMoveStep.MovePos;
                            }
                        }
                        break;

                    case AiryMoveStep.MovePos:
                        {
                            bool Rtn = false;

                            for (int i = 0; i < 4; i++)
                            {
                                if (isActive[i])
                                {
                                    MotorMember Member = (MotorMember)i;
                                    Rtn = this.DirectMove(Member, TargetPos[i]);

                                    if (Rtn)
                                    {
                                        SaveLog($"{Member} Move to {TargetPos[i]}");
                                        Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            if (Rtn)
                            {
                                this.moveStep = AiryMoveStep.CheckPos;
                            }
                            else
                            {
                                this.moveStep = AiryMoveStep.Alarm;
                            }
                        }
                        break;

                    case AiryMoveStep.CheckPos:
                        {
                            bool Rtn = this.CheckDevice();

                            if (Rtn)
                            {
                                this.moveStep = AiryMoveStep.CheckInPos;
                            }
                            else
                            {
                                this.moveStep = AiryMoveStep.Alarm;
                            }
                        }
                        break;

                    case AiryMoveStep.CheckInPos:
                        {
                            if (isRefreshData)
                            {
                                int[] GetPos =
                                {
                                    _Focuser.Position,
                                    _Aperture.Position,
                                    _FW1.Position,
                                    _FW2.Position
                                };

                                bool[] InPos = new bool[4];

                                InPos[0] = (GetPos[0] == TargetPos[0]) || (!isActive[0]);
                                InPos[1] = (GetPos[1] == TargetPos[1]) || (!isActive[1]);
                                InPos[2] = (GetPos[2] == TargetPos[2]) || (!isActive[2]);
                                InPos[3] = (GetPos[3] == TargetPos[3]) || (!isActive[3]);

                                // Check Finish
                                if (InPos[0])
                                    this.moveStatus_Focuser = UnitStatus.Finish;
                                if (InPos[1])
                                    this.moveStatus_Aperture = UnitStatus.Finish;
                                if (InPos[2])
                                    this.moveStatus_FW1 = UnitStatus.Finish;
                                if (InPos[3])
                                    this.moveStatus_FW2 = UnitStatus.Finish;

                                if (InPos[0] && InPos[1] && InPos[2] && InPos[3])
                                {
                                    isActive[2] = false;
                                    isActive[3] = false;

                                    //啟用且反向
                                    bool Check_Focuser = (isActive[0] && isReverse[0]);
                                    bool Check_Aperture = (isActive[1] && isReverse[1]);

                                    if (Check_Focuser || Check_Aperture)
                                    {
                                        for (int i = 0; i < 2; i++)
                                        {
                                            if (isActive[i] && isReverse[i])
                                            {
                                                TargetPos[i] += 3000;
                                                isReverse[i] = false;
                                            }
                                            else
                                            {
                                                isActive[i] = false;
                                            }
                                        }

                                        this.moveStep = AiryMoveStep.MovePos;
                                    }
                                    else
                                    {
                                        this.moveStep = AiryMoveStep.Finish;
                                    }

                                    SaveLog($"Move Finish");

                                }
                                else
                                {
                                    this.moveStep = AiryMoveStep.CheckPos;
                                }
                            }
                        }
                        break;

                    case AiryMoveStep.Finish:
                        {
                            flowStatus = UnitStatus.Finish;

                            this.isMoveProc = false;
                            SaveLog($"Move Flow Finish");
                        }
                        break;


                    case AiryMoveStep.Alarm:
                        {
                            flowStatus = UnitStatus.Alarm;

                            this.isMoveProc = false;
                            SaveLog($"Move Flow Alarm", true);
                        }
                        break;
                }
            }
        }

        #endregion--- MoveFlow ---

        public virtual void SetBacklash(int Backlash)
        {

        }

        #endregion --- 方法函式 ---"

        #region --- Command ---

        #region --- GetPosition ---

        public int GetPosition(MotorMember Member)
        {
            this.CheckDevice();

            switch (Member)
            {
                case MotorMember.Focuser:
                    return this._Focuser.Position;

                case MotorMember.Aperture:
                    return this._Aperture.Position;

                case MotorMember.FW1:
                    return this._FW1.Position;

                case MotorMember.FW2:
                    return this._FW2.Position;
            }

            return -1;
        }

        #endregion --- GetPosition ---

        #region --- GetVelocity ---

        public double GetVelocity(MotorMember Member)
        {
            this.CheckDevice();

            switch (Member)
            {
                case MotorMember.Focuser:
                    return this._Focuser.Speed;

                case MotorMember.Aperture:
                    return this._Aperture.Speed;

                case MotorMember.FW1:
                    return this._FW1.Speed;

                case MotorMember.FW2:
                    return this._FW2.Speed;
            }

            return -1;

        }
        #endregion --- GetVelocity ---

        #region --- Reset ---

        public void Reset(MotorMember Member)
        {
            this.CheckDevice();

            switch (Member)
            {
                case MotorMember.Focuser:
                    DirectMove(Member, this._Focuser.Position);
                    break;

                case MotorMember.Aperture:
                    DirectMove(Member, this._Aperture.Position);
                    break;
            }

        }
        #endregion --- Reset ---

        #region --- Home ---
        public bool Home(MotorMember Member)
        {

            switch (Member)
            {
                case MotorMember.All:
                    {
                        SaveLog("Home All");
                        Motor_Port.WriteLine(":H5#");
                        FW_1_Port.WriteLine(":H#");
                        FW_2_Port.WriteLine(":H#");


                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Focuser.HomeState || !this._Aperture.HomeState || !this._FW1.HomeState || !this._FW2.HomeState);

                        this.SetOverride(MotorMember.Focuser, true);
                        this.SetOverride(MotorMember.Aperture, true);

                        this.CloseLed();

                        return true;
                    }

                case MotorMember.Focuser:
                    {
                        SaveLog("Home Focuser");
                        Motor_Port.WriteLine(":H5#");

                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Focuser.HomeState);

                        this.SetOverride(MotorMember.Focuser, true);
                        this.CloseLed();

                        return this._Focuser.HomeState;
                    }

                case MotorMember.Aperture:
                    {
                        SaveLog("Home Aperture");
                        Motor_Port.WriteLine(":H5#");

                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Aperture.HomeState);

                        this.SetOverride(MotorMember.Aperture, true);
                        this.CloseLed();

                        return this._Aperture.HomeState;
                    }

                default:
                    {
                        return false;
                    }
            }

        }
        #endregion

        #region --- Jog ---"
        public void Jog(MotorMember Member, int Jog_Dist)
        {
            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        this.isMoveProc = false;

                        Thread PreprocessThread = new Thread(() => Move_Proc(this._Focuser.Position + Jog_Dist, -1, -1, -1, 10000));
                        PreprocessThread.Start();
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        this.isMoveProc = false;

                        Thread PreprocessThread = new Thread(() => Move_Proc(-1, this._Aperture.Position + Jog_Dist, -1, -1, 10000));
                        PreprocessThread.Start();
                    }
                    break;
            }

        }
        #endregion --- Jog ---"

        #region --- Stop ---
        public void Stop(MotorMember device)
        {
            Motor_Port.WriteLine(":S#");
            SaveLog("Stop.");

            Reset(device);
        }
        #endregion

        #region --- DirectMove ---

        public bool DirectMove(MotorMember Member, int Position)
        {


            if (!Motor_Port.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Position > Focus_limitF)
                            return false; ;  //預防超過 140000
                        Motor_Port.WriteLine($":L{Position}#");
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Position > Aper_limitF)
                            return false; ;  //預防超過 150000
                        Motor_Port.WriteLine($":l{Position}#");
                    }
                    break;

                case MotorMember.FW2:
                    {
                        FW_2_Port.WriteLine($":F{Position}#");
                    }
                    break;

                case MotorMember.FW1:
                    {
                        FW_1_Port.WriteLine($":F{Position}#");
                    }
                    break;

                default:
                    {
                        return false;
                    }
            }

            System.Threading.Thread.Sleep(200);

            this.CheckDevice();

            return true;
        }

        #endregion --- DirectMove ---

        #region --- Move(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut = 60000) ---
        public virtual void Move(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 60000)
        {
            if (!this.isMoveProc)
            {
                this.moveStatus_Focuser = UnitStatus.Idle;
                this.isMoveProc = true;

                Thread PreprocessThread = new Thread(() => Move_Proc(FocuserPos, AperturePos, FilterXyzPos, FilterNdPos, TimeOut));
                PreprocessThread.Start();
            }

        }
        #endregion

        #endregion --- Command ---
    }
}
