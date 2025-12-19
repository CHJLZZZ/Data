using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using BaseTool;

namespace HardwareManager
{
    public class AiryMotorCtrl : IMotorControl
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

        private Socket tcpClient;
        private int nPort = 8887;
        private string sIP = "192.168.3.7";
        private bool tcp_Connected = false;
        private EndPoint ipEndPoint = null;
        private Thread tcpReceivedData_Thread = null;
   
        private bool LED_PowerStatus = false;

        private bool isRefreshData = false;
        private bool isWork = false;
        public bool isMoveProc = false;
        public bool isCompensationMoveFinish_Focuser = false;
        public bool isCompensationMoveFinish_Aperture = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
        private UnitStatus flowStatus = UnitStatus.Idle;

        private int Focuser_limitF = 140000;
        private int Focuser_limitR = 0;
        private int Aperture_limitF = 140000;

        private MotorInfo _Focuser = new MotorInfo();
        private MotorInfo _Aperture = new MotorInfo();

        private AiryMoveStep moveStep = AiryMoveStep.Init;

        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.Ascom_Wheel_Airy_Motor;

        public AiryMotorCtrl(InfoManager info)
        {
            this.info = info;
            this.isWork = false;
            this.tcp_Connected = false;

            this.tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse(sIP);
            this.ipEndPoint = new IPEndPoint(iPAddress, nPort);

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
        {   // 無使用
            return UnitStatus.Finish;
        }

        #endregion --- MoveStatus_FW1 ---

        #region --- MoveStatus_FW2 ---

        public UnitStatus MoveStatus_FW2()
        {   // 無使用
            return UnitStatus.Finish;
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
            return -1;
        }

        #endregion --- NowPos_FW1 ---

        #region --- NowPos_FW2 ---

        public virtual int NowPos_FW2()
        {
            return -1;
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
            return this.Focuser_limitF;
        }

        #endregion --- LimitF ---

        #region --- LimitR ---

        public int Focus_LimitR()
        {
            return this.Focuser_limitR;
        }

        #endregion --- LimitR ---

        #endregion --- Property ---

        #region --- Method ---"

        #region --- TCP_Connect ---
        private void TCP_Connect()
        {
            this.tcpClient.Connect(this.ipEndPoint);
            this.tcp_Connected = true;

            this.tcpReceivedData_Thread = new Thread(() => OnDataReceived());
            this.tcpReceivedData_Thread.Start();
        }

        #endregion

        #region --- TCP_Close ---
        private void TCP_Close()
        {
            this.tcp_Connected = false;
            this.tcpClient.Close();
        }
        #endregion --- TCP_Connect ---

        #region --- isConnect ---

        public bool isConnect()
        {
            return this.tcp_Connected;
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
            if (!this.tcp_Connected)
            {
                SaveLog($"TCP is not Connected", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":V{Speed}#"));
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":v{Speed}#"));
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
            if (!this.tcp_Connected)
            {
                SaveLog($"TCP is not Connected", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Switch)
                        {
                            this.tcpClient.Send(Encoding.UTF8.GetBytes(":O1#"));
                        }
                        else
                        {
                            this.tcpClient.Send(Encoding.UTF8.GetBytes(":O0#"));
                        }
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Switch)
                        {
                            this.tcpClient.Send(Encoding.UTF8.GetBytes(":o1#"));
                        }
                        else
                        {
                            this.tcpClient.Send(Encoding.UTF8.GetBytes(":o0#"));
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
            this.isRefreshData = false;
            this.tcpClient.Send(Encoding.UTF8.GetBytes(":i#"));

            System.Threading.Thread.Sleep(200);

            return true;
        }
        #endregion

        #region --- CloseLed ---
        private bool CloseLed()
        {
            if (!this.tcp_Connected)
            {
                SaveLog($"TCP is not Connected", true);
                return false;
            }

            this.tcpClient.Send(Encoding.UTF8.GetBytes(":h#"));
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
                if (!this.tcp_Connected)
                {
                    this.TCP_Connect();

                    Thread.Sleep(500);
                    CloseLed();
                    this.isWork = true;

                    SaveLog($"Connect");
                }
                else
                {
                    this.TCP_Close();
                    SaveLog($"Disconnect");
                    this.TCP_Connect();
                }

                this.SetOverride(MotorMember.Focuser, false);
                Thread.Sleep(10);
                this.SetOverride(MotorMember.Aperture, false);

                //this.Home(AiryMember.All);

                SaveLog($"[Auto Focus]Connect");
                this.isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"Port Open Fail - {ex.Message}", true);
            }
        }

        public void Open(string Com_Port, string Com_Port_2 , string Com_Port_3)
        {
          
        }

        #endregion --- Open ---

        #region --- Close ---
        public void Close()
        {
            if (this.tcp_Connected)
            {
                this.TCP_Close();
                this.isWork = false;
            }
            this.isMoveProc = false;
        }

        #endregion --- Close ---

        #region --- OnDataReceived ---
        private void OnDataReceived()
        {
            while (this.tcp_Connected)
            {
                CheckDevice();
                if (this.tcpClient.Available > 0)
                {
                    byte[] data = new byte[2048];

                    int length = tcpClient.Receive(data);
                    string message = Encoding.UTF8.GetString(data, 0, length);

                    string dataIn = message.Substring(2, message.Length - 4);
                    string[] words = dataIn.Split(',');

                    if (words.Length >= 20)
                    {
                        //POS: position
                        //SPD: speed
                        //PI: status of photointerrupter
                        //STAT: status of home
                        //OR: status of override 
                        //LED PI: status of photointerrupter LED power

                        int.TryParse(words[0], out _Focuser.Position);
                        double.TryParse(words[1], out _Focuser.Speed);
                        int.TryParse(words[2], out _Aperture.Position);
                        double.TryParse(words[3], out _Aperture.Speed);

                        // PinHomeState (不是用來判斷 Home)
                        _Focuser.PinHomeState = (words[10] == "5");
                        _Aperture.PinHomeState = (words[11] == "5");

                        // HomeState
                        _Focuser.HomeState = (words[14] == "5");
                        _Aperture.HomeState = (words[15] == "5");

                        _Focuser.OverrideState = (words[18] == "1");
                        _Aperture.OverrideState = (words[19] == "1");

                        LED_PowerStatus = (words[22] == "1");

                        int.TryParse(words[0], out _Focuser.Position);
                        int.TryParse(words[0], out _Focuser.Position);


                        Focuser_limitF = Convert.ToInt32(words[23]);
                        Aperture_limitF = Convert.ToInt32(words[24]);

                        this.isRefreshData = true;

                        UpdateStatus?.Invoke(_Focuser, _Aperture,null,null);

                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }
        #endregion --- OnDataReceived ---

        #region --- HomeAll ---

        public void HomeAll()
        {
            this.Home(MotorMember.All);
        }

        #endregion --- HomeAll ---

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

                default:
                    return 0;
            }
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
                        this.tcpClient.Send(Encoding.UTF8.GetBytes(":H5#"));

                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Focuser.HomeState || !this._Aperture.HomeState );

                        this.SetOverride(MotorMember.Focuser, false);
                        this.SetOverride(MotorMember.Aperture, false);

                        this.CloseLed();

                        return true;
                    }

                case MotorMember.Focuser:
                    {
                        SaveLog("Home Focuser");
                        this.tcpClient.Send(Encoding.UTF8.GetBytes(":H0#"));

                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Focuser.HomeState);

                        this.SetOverride(MotorMember.Focuser, false);
                        this.CloseLed();

                        return this._Focuser.HomeState;
                    }

                case MotorMember.Aperture:
                    {
                        SaveLog("Home Aperture");
                        this.tcpClient.Send(Encoding.UTF8.GetBytes(":H1#"));

                        do
                        {
                            this.CheckDevice();
                            System.Threading.Thread.Sleep(100);
                        } while (!this._Aperture.HomeState);

                        this.SetOverride(MotorMember.Aperture, false);
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

                        Thread PreprocessThread = new Thread(() => Move_Proc(this._Focuser.Position + Jog_Dist, -1, 10000));
                        PreprocessThread.Start();
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        this.isMoveProc = false;

                        Thread PreprocessThread = new Thread(() => Move_Proc(-1, this._Aperture.Position + Jog_Dist, 10000));
                        PreprocessThread.Start();
                    }
                    break;
            }

        }
        #endregion --- Jog ---"

        #region --- Stop ---
        public void Stop(MotorMember device)
        {
            try
            {
                this.tcpClient.Send(Encoding.UTF8.GetBytes(":S#"));
                SaveLog("Stop.");

                Reset(device);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region --- DirectMove ---

        public bool DirectMove(MotorMember Member, int Position)
        {


            if (!this.tcp_Connected)
            {
                SaveLog($"TCP is not Connected", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Position > this.Focuser_limitF)
                            return false; ;  //預防超過 140000
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":L{Position}#"));
                    }
                    break;

                case MotorMember.Aperture:
                    {
                        if (Position > this.Aperture_limitF)
                            return false; ;  //預防超過 150000
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":l{Position}#"));
                    }
                    break;

                case MotorMember.FW1:
                    {
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":F{Position}#"));
                    }
                    break;

                case MotorMember.FW2:
                    {
                        this.tcpClient.Send(Encoding.UTF8.GetBytes($":f{Position}#"));
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

        #region --- Move(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut = 60000) ---
        public virtual void Move(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 60000)
        {
            if (!this.isMoveProc)
            {
                this.moveStatus_Focuser = UnitStatus.Idle;
                this.isMoveProc = true;

                Thread PreprocessThread = new Thread(() => Move_Proc(FocuserPos, AperturePos, TimeOut));
                PreprocessThread.Start();
            }
        }
        #endregion

        #region --- Move_Proc ---
        private void Move_Proc(int FocuserPos, int AperturePos, int TimeOut)
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
                            };

                            TargetPos = new int[]
                            {
                                FocuserPos,
                                AperturePos,
                            };

                            SaveLog($"Move Flow Start , Focuser = {FocuserPos} , Aperture = {AperturePos} ");

                            if (isActive[0])
                                this.moveStatus_Focuser = UnitStatus.Running;
                            else if (isActive[1])
                                this.moveStatus_Aperture = UnitStatus.Running;

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
                                            TargetPos[i] -= 40000;
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

                            for (int i = 0; i < 2; i++)
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
                            //this.moveStep = AiryMoveStep.Finish;

                            if (isRefreshData)
                            {
                                int[] GetPos =
                                {
                                    _Focuser.Position,
                                    _Aperture.Position,
                                };

                                bool[] InPos = new bool[2];

                                InPos[0] = (GetPos[0] == TargetPos[0]) || (!isActive[0]);
                                InPos[1] = (GetPos[1] == TargetPos[1]) || (!isActive[1]);

                                // Check Finish
                                if (InPos[0])
                                    this.moveStatus_Focuser = UnitStatus.Finish;
                                if (InPos[1])
                                    this.moveStatus_Aperture = UnitStatus.Finish;

                                if (InPos[0] && InPos[1])
                                {

                                    //啟用且反向
                                    bool Check_Focuser = (isActive[0] && isReverse[0]);
                                    bool Check_Aperture = (isActive[1] && isReverse[1]);

                                    if (Check_Focuser || Check_Aperture)
                                    {
                                        for (int i = 0; i < 2; i++)
                                        {
                                            if (isActive[i] && isReverse[i])
                                            {
                                                TargetPos[i] += 40000;
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

        #endregion --- Method ---
    }
}