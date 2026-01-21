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
    public class AiryUnitCtrl_4in1 : IMotorControl
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

        private SerialPort AiryPort;
        private bool isRefreshData = false;
        private bool isWork = false;
        public bool isMoveProc = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
        private UnitStatus moveStatus_FW1 = UnitStatus.Idle;
        private UnitStatus moveStatus_FW2 = UnitStatus.Idle;
        private UnitStatus flowStatus = UnitStatus.Idle;
                
        private int limitF = 140000;
        private int limitR = 0;

        private MotorInfo _Focuser = new MotorInfo();
        private MotorInfo _Aperture = new MotorInfo();
        private MotorInfo _FW1 = new MotorInfo();
        private MotorInfo _FW2 = new MotorInfo();

        private AiryMoveStep moveStep = AiryMoveStep.Init;

        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.Airy_4In1_Wheel_Motor;

        private int Backlash = 40000;
        public AiryUnitCtrl_4in1(InfoManager info)
        {
            this.info = info;
            this.AiryPort = new SerialPort();
            AiryPort.DataReceived -= AiryPort_DataReceived;
            AiryPort.DataReceived += AiryPort_DataReceived;
            this.isWork = false;
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
            return this.isWork;
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
            return this.limitF;
        }

        #endregion --- LimitF ---

        #region --- LimitR ---

        public int Focus_LimitR()
        {
            return this.limitR;
        }

        #endregion --- LimitR ---

        #endregion --- Property ---

        #region --- 方法函式 ---"

        #region --- isConnect ---

        public bool isConnect()
        {
            return this.AiryPort.IsOpen;
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
           if (!AiryPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {  
                        AiryPort.WriteLine($":V{Speed}#");
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        AiryPort.WriteLine($":v{Speed}#");
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
            if (!AiryPort.IsOpen)
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
                            AiryPort.WriteLine(":O1#");
                        }
                        else
                        {
                            AiryPort.WriteLine(":O0#");
                        }
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Switch)
                        {
                            AiryPort.WriteLine(":o1#");
                        }
                        else
                        {
                            AiryPort.WriteLine(":o0#");
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
        private bool CheckDevice()
        {
            //if (!AiryPort.IsOpen)
            //{
            //    SaveLog($"Port is not Open", true);
            //    return false;
            //}
            isRefreshData = false;
            AiryPort.WriteLine(":i#");

            System.Threading.Thread.Sleep(400);

            return true;
        }
        #endregion

        #region --- CloseLed ---
        private bool CloseLed()
        {
            if (!AiryPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            AiryPort.WriteLine(":h#");  
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
            try
            {
                if (!AiryPort.IsOpen)
                {
                    AiryPort.BaudRate = 500000;
                    AiryPort.PortName = Com_Port;
                    AiryPort.Open();

                    Thread.Sleep(500);
                    CloseLed();
                    this.isWork = true;

                    SaveLog($"Open Port : {Com_Port}");
                }
                else
                {
                    AiryPort.Close();
                    SaveLog($"Port Close");
                    Open(Com_Port);
                }

                this.SetOverride(MotorMember.Focuser, true);
                Thread.Sleep(10);
                this.SetOverride(MotorMember.Aperture, true);

                //this.Home(AiryMember.All);

                SaveLog($"[Auto Focus]Open Port : {Com_Port}");
                this.isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"Port Open Fail - {ex.Message}", true);
            }
        }


        public void Open(string Com_Port, string Com_Port_2, string Com_Port_3 )
        {
           
        }

        #endregion --- Open ---

        #region --- Close ---
        public void Close()
        {
            if (this.AiryPort.IsOpen)
            {
                this.AiryPort.Close();
                this.isWork = false;
            }
            this.isMoveProc = false;
        }

        #endregion --- Close ---

        #region --- AiryPort_DataReceived ---
        private void AiryPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();

            string CMD = indata.Substring(0, 2);

            if (CMD == ":i")
            {
                string Para = indata.Substring(2, indata.Length - 4);

                string[] words = Para.Split(',');

                if(words.Length >= 20)
                {
                    _Aperture.Position = Convert.ToInt32(words[0]);
                    _Aperture.Speed = Convert.ToDouble(words[1]);
                    _Focuser.Position = Convert.ToInt32(words[2]);
                    _Focuser.Speed = Convert.ToDouble(words[3]);

                    // FW1 & FW2 相反
                    _FW2.Position = Convert.ToInt32(words[4]);
                    _FW2.Speed = Convert.ToDouble(words[5]);
                    _FW1.Position = Convert.ToInt32(words[6]);
                    _FW1.Speed = Convert.ToDouble(words[7]);

                    // PinHomeState (不是用來判斷 Home)
                    if (Convert.ToInt32(words[8]) == 5)
                        _Aperture.PinHomeState = true;
                    else
                        _Aperture.PinHomeState = false;

                    if (Convert.ToInt32(words[9]) == 5)
                        _Focuser.PinHomeState = true;
                    else
                        _Focuser.PinHomeState = false;

                    if (Convert.ToInt32(words[10]) == 5)
                        _FW2.PinHomeState = true;
                    else
                        _FW2.PinHomeState = false;

                    if (Convert.ToInt32(words[11]) == 5)
                        _FW1.PinHomeState = true;
                    else
                        _FW1.PinHomeState = false;

                    // HomeState
                    if (Convert.ToInt32(words[12]) == 5 || _Aperture.Position == 0)
                        _Aperture.HomeState = true;
                    else
                        _Aperture.HomeState = false;

                    if (Convert.ToInt32(words[13]) == 5 || _Focuser.Position == 0)
                        _Focuser.HomeState = true;
                    else
                        _Focuser.HomeState = false;

                    if (Convert.ToInt32(words[14]) == 5 || _FW2.Position == 0)
                        _FW2.HomeState = true;
                    else
                        _FW2.HomeState = false;

                    if (Convert.ToInt32(words[15]) == 5 || _FW1.Position == 0)
                        _FW1.HomeState = true;
                    else
                        _FW1.HomeState = false;

                    // Override (用來設定是否可以過極限)
                    if (Convert.ToInt32(words[16]) == 1)
                        _Aperture.OverrideState = true;
                    else
                        _Aperture.OverrideState = false;

                    if (Convert.ToInt32(words[17]) == 1)
                        _Focuser.OverrideState = true;
                    else
                        _Focuser.OverrideState = false;

                    if (Convert.ToInt32(words[18]) == 1)
                        _FW2.OverrideState = true;
                    else
                        _FW2.OverrideState = false;

                    if (Convert.ToInt32(words[19]) == 1)
                        _FW1.OverrideState = true;
                    else
                        _FW1.OverrideState = false;

                    this.isRefreshData = true;
                }

                UpdateStatus?.Invoke(_Focuser, _Aperture, _FW1, _FW2);
            }
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

                            if(isActive[0])
                                this.moveStatus_Focuser = UnitStatus.Running;
                            else if(isActive[1])
                                this.moveStatus_Aperture = UnitStatus.Running;
                            else if(isActive[2])
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
                                InPos[2] = (GetPos[2] == TargetPos[2] * 25600) || (!isActive[2]);
                                InPos[3] = (GetPos[3] == TargetPos[3] * 25600) || (!isActive[3]);

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
            this.Backlash = Backlash;
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
                        AiryPort.WriteLine(":H5#");

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
                        AiryPort.WriteLine(":H0#");

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
                        AiryPort.WriteLine(":H1#");

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
            AiryPort.WriteLine(":S#");
            SaveLog("Stop.");

            Reset(device);
        }
        #endregion

        #region --- DirectMove ---

        public bool DirectMove(MotorMember Member, int Position)
        {
               

            if (!AiryPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }            

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Position > 140000)
                            return false; ;  //預防超過 140000
                        AiryPort.WriteLine($":L{Position}#");
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Position > 150000)
                            return false; ;  //預防超過 150000
                        AiryPort.WriteLine($":l{Position}#");
                    }
                    break;

                case MotorMember.FW2:
                    {
                        AiryPort.WriteLine($":F{Position}#");
                    }
                    break;

                case MotorMember.FW1:
                    {
                        AiryPort.WriteLine($":f{Position}#");
                    }
                    break;

                default:
                    {
                        return false;
                    }
            }

            System.Threading.Thread.Sleep(200);

            return true;
        }

        #endregion --- DirectMove ---

        #region --- Move ---

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

        #endregion --- Move ---

        #endregion --- Command ---

    }
}
