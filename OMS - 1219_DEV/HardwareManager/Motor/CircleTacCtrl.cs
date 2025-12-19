using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using BaseTool;
using System.IO;
using Newtonsoft.Json;

namespace HardwareManager
{
    public class CircleTacCtrl : IMotorControl
    {
        //Device No
        //[0] XYZ = FW1
        //[1] ND = FW2
        //[2] Focuser
        //[3] Aperture

        public enum Enum_MoveStep
        {
            Init,
            CheckOriPos,
            GetOriPos,
            MovePos,
            CheckInPos,
            Finish,
            Alarm,
        }

        public enum Enum_InitStep
        {
            Init,
            CheckStatus,
            GoHome,
            CheckHomed,
            MoveToBasePos,
            CheckInPos,
            Finish,
            Alarm,
        }

        public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;
        public event Action<MotorStatus, MotorStatus, MotorStatus, MotorStatus> UpdateInfo;

        private InfoManager info;

        private TcpClient client = new TcpClient();

        private int ConnectPort = 8887;
        private string ConnectIP = "192.168.3.7";

        public bool isMoveProc = false;
        public bool isHomeProc = false;
        public bool isCompensationMoveFinish_Focuser = false;
        public bool isCompensationMoveFinish_Aperture = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
        private UnitStatus moveStatus_FW1 = UnitStatus.Idle;
        private UnitStatus moveStatus_FW2 = UnitStatus.Idle;
        private UnitStatus flowStatus = UnitStatus.Idle;

        private MotorStatus _Focuser_Status = new MotorStatus();
        private MotorStatus _Aperture_Status = new MotorStatus();
        private MotorStatus _FW1_Status = new MotorStatus();
        private MotorStatus _FW2_Status = new MotorStatus();

        private MotorSetting _Focuser_Setting = new MotorSetting();
        private MotorSetting _Aperture_Setting = new MotorSetting();
        private MotorSetting _FW1_Setting = new MotorSetting();
        private MotorSetting _FW2_Setting = new MotorSetting();

        public MotorSetting Focuser_Setting { get => _Focuser_Setting; }
        public MotorSetting Aperture_Setting { get => _Aperture_Setting; }
        public MotorSetting FW1_Setting { get => _FW1_Setting; }
        public MotorSetting FW2_Setting { get => _FW2_Setting; }


        private Enum_MoveStep moveStep = Enum_MoveStep.Init;
        private Enum_InitStep initStep = Enum_InitStep.Init;


        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.CircleTac;


        private bool Doing = false;

        private bool ReadMcu = false;

        public CircleTacCtrl(InfoManager info, bool ReadMcu)
        {
            this.info = info;
            this.ReadMcu = ReadMcu;
        }

        public UnitStatus MoveStatus_Focuser()
        {
            return this.moveStatus_Focuser;
        }

        public UnitStatus MoveStatus_Aperture()
        {
            return this.moveStatus_Aperture;
        }

        public UnitStatus MoveStatus_FW1()
        {
            return this.moveStatus_FW1;
        }

        public UnitStatus MoveStatus_FW2()
        {
            return this.moveStatus_FW2;
        }

        public UnitStatus MoveFlowStatus()
        {
            return this.flowStatus;
        }

        public bool IsWork()
        {
            return true;
        }

        public bool IsMoveProc()
        {
            return this.isMoveProc;
        }

        public int NowPos_Focuser()
        {
            ReadInfo();
            return this._Focuser_Status.Pos;
        }

        public int NowPos_Aperture()
        {
            ReadInfo();
            return this._Aperture_Status.Pos;
        }

        public int NowPos_FW1()
        {
            ReadInfo();
            return this._FW1_Status.Pos;
        }

        public int NowPos_FW2()
        {
            ReadInfo();
            return this._FW2_Status.Pos;
        }

        public bool IsHome_Focuser()
        {
            ReadInfo();
            return this._Focuser_Status.Homed;
        }

        public bool IsHome_Aperture()
        {
            ReadInfo();
            return this._Aperture_Status.Homed;
        }

        public double NowVelocity_Focuser()
        {
            return GetVelocity(MotorMember.Focuser);
        }

        public double NowVelocity_Aperture()
        {
            return GetVelocity(MotorMember.Aperture);
        }
        public int Focus_LimitF()
        {
            return this._Focuser_Setting.Limit;
        }

        public int Aperture_LimitF()
        {
            return this._Aperture_Setting.Limit;
        }

        public int Focus_LimitR()
        {
            return 0;
        }

        public void SaveLog(string Log, bool isAlm = false)
        {
            if (isAlm)
            {
                info.Error($"[Motor] {Log}");
            }
            else
            {
                info.General($"[Motor] {Log}");
            }
        }

        #region Connect

        public void Open()
        {
            try
            {
                if (ReadMcu)
                {
                    ParaInit_ReadMcu();
                }
                else
                {
                    ParaInit_Config();
                }

                SaveLog($"Connect Succed");

            }
            catch (SocketException ex)
            {
                SaveLog($"Connect Fail (Socket Error) : {ex.Message}", true);
            }
            catch (Exception ex)
            {
                SaveLog($"Connect Fail : {ex.Message}", true);
            }
        }

        public void Open(string Com_Port)
        {
        }

        public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
        {

        }


        public void Close()
        {
            //if (client.Connected)
            //{
            //	stream?.Close();
            //	client?.Close();
            //	client?.Dispose();
            //}

            this.isMoveProc = false;
        }

        public bool CheckConnect()
        {
            try
            {
                byte[] testData = new byte[1] { 0 };
                client.GetStream().Write(testData, 0, testData.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Hardware Function

        private void ParaInit_Config()
        {
            MotorSetting Focuser_Setting = new MotorSetting
            {
                HomeSpeed = 50000,
                RunSpeed = 500000,
                ZeroPos = 30000,
                Limit = 3450000,
                Gap = 10000,
                CurrentLimit = 20,
                PosJob = 2000,
                Enable = 1,
            };

            MotorSetting Aperture_Setting = new MotorSetting
            {
                HomeSpeed = 50000,
                RunSpeed = 500000,
                ZeroPos = 30000,
                Limit = 1500000,
                Gap = 10000,
                CurrentLimit = 20,
                PosJob = 2000,
                Enable = 1,
            };

            MotorSetting FW1_Setting = new MotorSetting
            {
                HomeSpeed = 20000,
                RunSpeed = 1000000,
                ZeroPos = -200,
                Limit = 4,
                Gap = 0,
                CurrentLimit = 20,
                PosJob = 2000,
                Enable = 1,
            };

            MotorSetting FW2_Setting = new MotorSetting
            {
                HomeSpeed = 20000,
                RunSpeed = 1000000,
                ZeroPos = -200,
                Limit = 4,
                Gap = 0,
                CurrentLimit = 20,
                PosJob = 2000,
                Enable = 1,
            };

            this.SetMcuPara(Focuser_Setting, Aperture_Setting, FW1_Setting, FW2_Setting);
        }

        private bool SendCommand(string SendData, out string ResponseData)
        {
            Doing = true;

            SendAndReceiveData(SendData, out ResponseData);

            Doing = false;
            ReadInfo();

            return (ResponseData == SendData);
        }

        public bool SendAndReceiveData(string sendData, out string responseData)
        {

            // logflag = true;

            responseData = null;  // 初始化输出参数
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // 异步连接超时控制
                    IAsyncResult connectResult = client.BeginConnect(IPAddress.Parse(ConnectIP), ConnectPort, null, null);

                    if (!connectResult.AsyncWaitHandle.WaitOne(100))
                    {
                        SaveLog($"Connect Timeout", true);
                        return false;
                    }
                    client.EndConnect(connectResult);

                    using (NetworkStream stream = client.GetStream())
                    {


                        // 发送字符串数据（自动添加换行符）
                        byte[] bytesToSend = Encoding.UTF8.GetBytes(sendData + '\n');
                        stream.Write(bytesToSend, 0, bytesToSend.Length);
                        stream.Flush();

                        // 设置读取超时（1000ms）
                        stream.ReadTimeout = 1000;

                        // 一次最多读取1000个字节
                        byte[] buffer = new byte[1000];
                        int bytesRead = 0;

                        try
                        {
                            // 尝试读取数据
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                responseData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                responseData = responseData.Replace("\r", "").Replace("\n", "");  // 删除换行符
                                return true;

                            }

                            SaveLog($"Get Response Fail (Empty)", true);
                            return false;

                        }
                        catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.TimedOut)
                        {
                            SaveLog($"Wait Response Timeout (1000ms)", true);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog($"Send/Receive Error : {ex.Message}", true);
                return false;
            }
        }

        private int ConvertDeviceNo(MotorMember Device)
        {
            switch (Device)
            {
                case MotorMember.FW1: return 0;
                case MotorMember.FW2: return 1;
                case MotorMember.Focuser: return 2;
                case MotorMember.Aperture: return 3;
                default: return -1;
            }
        }

        private MotorSetting GetMotorSetting(MotorMember Device)
        {
            switch (Device)
            {
                case MotorMember.FW1: return _FW1_Setting;
                case MotorMember.FW2: return _FW2_Setting;
                case MotorMember.Focuser: return _Focuser_Setting;
                case MotorMember.Aperture: return _Aperture_Setting;
                default: return null;
            }
        }

        private bool CheckMovePara(MotorMember Device, int Pos)
        {
            MotorSetting Info = null;

            switch (Device)
            {
                case MotorMember.Focuser: Info = _Focuser_Setting; break;
                case MotorMember.Aperture: Info = _Aperture_Setting; break;
                case MotorMember.FW1: Info = _FW1_Setting; break;
                case MotorMember.FW2: Info = _FW2_Setting; break;
            }

            if (Pos > Info.Limit)
            {
                SaveLog($@"{Device} TargetPos > Limit ({Pos} > {Info.Limit})", true);
                return false;
            }

            if (Pos < 0)
            {
                SaveLog($@"{Device} TargetPos < 0 ({Pos} < 0)", true);
                return false;
            }

            if (Pos < 0)
            {
                SaveLog($@"{Device} TargetPos < 0 ({Pos} < 0)", true);
                return false;
            }

            return true;
        }


        public bool ReadInfo()
        {
            if (Doing) return true;
            string Command = "cmdreadinfo(1,1)";

            string ResponseData = "";
            bool Rtn = SendAndReceiveData(Command, out ResponseData);

            if (!Rtn) return false;

            try
            {
                McuInfo Info = JsonConvert.DeserializeObject<McuInfo>(ResponseData);

                int DeviceNo = 0;

                DeviceNo = ConvertDeviceNo(MotorMember.Focuser);
                _Focuser_Status.Pos = (Info.location[DeviceNo]);
                _Focuser_Status.Homed = (Info.isgohome[DeviceNo] == 1);
                _Focuser_Status.InSensor = (Info.isinsenser[DeviceNo] == 1);
                _Focuser_Status.Moving = (Info.islocationrunning[DeviceNo] == 1);

                DeviceNo = ConvertDeviceNo(MotorMember.Aperture);
                _Aperture_Status.Pos = (Info.location[DeviceNo]);
                _Aperture_Status.Homed = (Info.isgohome[DeviceNo] == 1);
                _Aperture_Status.InSensor = (Info.isinsenser[DeviceNo] == 1);
                _Aperture_Status.Moving = (Info.islocationrunning[DeviceNo] == 1);

                DeviceNo = ConvertDeviceNo(MotorMember.FW1);
                _FW1_Status.Pos = (Info.location[DeviceNo]);
                _FW1_Status.Homed = (Info.isgohome[DeviceNo] == 1);
                _FW1_Status.InSensor = (Info.isinsenser[DeviceNo] == 1);
                _FW1_Status.Moving = (Info.islocationrunning[DeviceNo] == 1);

                DeviceNo = ConvertDeviceNo(MotorMember.FW2);
                _FW2_Status.Pos = (Info.location[DeviceNo]);
                _FW2_Status.Homed = (Info.isgohome[DeviceNo] == 1);
                _FW2_Status.InSensor = (Info.isinsenser[DeviceNo] == 1);
                _FW2_Status.Moving = (Info.islocationrunning[DeviceNo] == 1);

                UpdateHardwareInfo();

            }
            catch (Exception ex)
            {
                //SaveLog($"ReadInfo Fail : {ex.Message}", true);
            }

            return true;
        }


        public bool ParaInit_ReadMcu()
        {
            if (Doing) return true;
            string Command = "cmdreadconfig(1,1)";

            string ResponseData = "";
            bool Rtn = SendAndReceiveData(Command, out ResponseData);

            if (!Rtn) return false;

            try
            {
                ConfigMachine Info = JsonConvert.DeserializeObject<ConfigMachine>(ResponseData);

                MotorSetting Focuser_Setting = new MotorSetting
                {
                    HomeSpeed = Info.speedhome[2],
                    RunSpeed = Info.speedrun[2],
                    ZeroPos = Info.poszero[2],
                    Limit = Info.posmax[2],
                    Gap = Info.posgap[2],
                    CurrentLimit = Info.imax[2],
                    PosJob = Info.posjob[2],
                    Enable = Info.isenable[2],
                };

                SaveLog($"ReadMcu => : Focuser" + "\r\n" +
                    $"HomeSpeed = {Focuser_Setting.HomeSpeed}" + "\r\n" +
                    $"RunSpeed = {Focuser_Setting.RunSpeed}" + "\r\n" +
                    $"ZeroPos = {Focuser_Setting.ZeroPos}" + "\r\n" +
                    $"Limit = {Focuser_Setting.Limit}" + "\r\n" +
                    $"Gap = {Focuser_Setting.Gap}" + "\r\n" +
                    $"CurrentLimit = {Focuser_Setting.CurrentLimit}" + "\r\n" +
                    $"PosJob = {Focuser_Setting.PosJob}" + "\r\n" +
                    $"Enable = {Focuser_Setting.Enable}"
                    );

                MotorSetting Aperture_Setting = new MotorSetting
                {
                    HomeSpeed = Info.speedhome[3],
                    RunSpeed = Info.speedrun[3],
                    ZeroPos = Info.poszero[3],
                    Limit = Info.posmax[3],
                    Gap = Info.posgap[3],
                    CurrentLimit = Info.imax[3],
                    PosJob = Info.posjob[3],
                    Enable = Info.isenable[3],
                };

                SaveLog($"ReadMcu => : Aperture" + "\r\n" +
                    $"HomeSpeed = {Aperture_Setting.HomeSpeed}" + "\r\n" +
                    $"RunSpeed = {Aperture_Setting.RunSpeed}" + "\r\n" +
                    $"ZeroPos = {Aperture_Setting.ZeroPos}" + "\r\n" +
                    $"Limit = {Aperture_Setting.Limit}" + "\r\n" +
                    $"Gap = {Aperture_Setting.Gap}" + "\r\n" +
                    $"CurrentLimit = {Aperture_Setting.CurrentLimit}" + "\r\n" +
                    $"PosJob = {Aperture_Setting.PosJob}" + "\r\n" +
                    $"Enable = {Aperture_Setting.Enable}"
                    );


                MotorSetting FW1_Setting = new MotorSetting
                {
                    HomeSpeed = Info.speedhome[0],
                    RunSpeed = Info.speedrun[0],
                    ZeroPos = Info.poszero[0],
                    Limit = Info.posmax[0],
                    Gap = Info.posgap[0],
                    CurrentLimit = Info.imax[0],
                    PosJob = Info.posjob[0],
                    Enable = Info.isenable[0],
                };

                SaveLog($"ReadMcu => : FW1" + "\r\n" +
                  $"HomeSpeed = {FW1_Setting.HomeSpeed}" + "\r\n" +
                  $"RunSpeed = {FW1_Setting.RunSpeed}" + "\r\n" +
                  $"ZeroPos = {FW1_Setting.ZeroPos}" + "\r\n" +
                  $"Limit = {FW1_Setting.Limit}" + "\r\n" +
                  $"Gap = {FW1_Setting.Gap}" + "\r\n" +
                  $"CurrentLimit = {FW1_Setting.CurrentLimit}" + "\r\n" +
                  $"PosJob = {FW1_Setting.PosJob}" + "\r\n" +
                  $"Enable = {FW1_Setting.Enable}"
                  );

                MotorSetting FW2_Setting = new MotorSetting
                {
                    HomeSpeed = Info.speedhome[1],
                    RunSpeed = Info.speedrun[1],
                    ZeroPos = Info.poszero[1],
                    Limit = Info.posmax[1],
                    Gap = Info.posgap[1],
                    CurrentLimit = Info.imax[1],
                    PosJob = Info.posjob[1],
                    Enable = Info.isenable[1],
                };

                SaveLog($"ReadMcu => : FW2" + "\r\n" +
                    $"HomeSpeed = {FW2_Setting.HomeSpeed}" + "\r\n" +
                    $"RunSpeed = {FW2_Setting.RunSpeed}" + "\r\n" +
                    $"ZeroPos = {FW2_Setting.ZeroPos}" + "\r\n" +
                    $"Limit = {FW2_Setting.Limit}" + "\r\n" +
                    $"Gap = {FW2_Setting.Gap}" + "\r\n" +
                    $"CurrentLimit = {FW2_Setting.CurrentLimit}" + "\r\n" +
                    $"PosJob = {FW2_Setting.PosJob}" + "\r\n" +
                    $"Enable = {FW2_Setting.Enable}"
             );

                this.SetMcuPara(Focuser_Setting, Aperture_Setting, FW1_Setting, FW2_Setting);
            }
            catch (Exception ex)
            {
                SaveLog($"ReadMcu Fail : {ex.Message}", true);
            }

            return true;
        }


        private void UpdateHardwareInfo()
        {
            UpdateInfo?.Invoke(_Focuser_Status, _Aperture_Status, _FW1_Status, _FW2_Status);

            MotorInfo FocuserInfo = new MotorInfo { Position = _Focuser_Status.Pos };
            MotorInfo ApertureInfo = new MotorInfo { Position = _Aperture_Status.Pos };
            MotorInfo FW1Info = new MotorInfo { Position = _FW1_Status.Pos };
            MotorInfo FW2Info = new MotorInfo { Position = _FW2_Status.Pos };

            UpdateStatus?.Invoke(FocuserInfo, ApertureInfo, FW1Info, FW2Info);
        }

        #endregion

        public int GetPosition(MotorMember Member)
        {
            switch (Member)
            {
                case MotorMember.Focuser:
                    return NowPos_Focuser();

                case MotorMember.Aperture:
                    return NowPos_Aperture();

                case MotorMember.FW1:
                    return NowPos_FW1();

                case MotorMember.FW2:
                    return NowPos_FW2();
            }

            return -1;
        }

        public double GetVelocity(MotorMember Member)
        {
            switch (Member)
            {
                case MotorMember.FW1: return _FW1_Setting.RunSpeed;
                case MotorMember.FW2: return _FW2_Setting.RunSpeed;
                case MotorMember.Focuser: return _Focuser_Setting.RunSpeed;
                case MotorMember.Aperture: return _Aperture_Setting.RunSpeed;
            }
            return 0;
        }

        public void Reset(MotorMember Member)
        {
            return;
        }

        public bool Home(MotorMember Member = MotorMember.All)
        {
            string ResponseData = "";

            if (Member == MotorMember.All)
            {
                bool Rtn = true;

                for (int i = 0; i < 4; i++)
                {
                    string Command = $"cmdgohome({i},0)";
                    Rtn &= SendCommand(Command, out ResponseData);

                    if (!Rtn)
                    {
                        SaveLog($"Home Fail : {ResponseData}");
                        return false;
                    }
                }

                return Rtn;
            }
            else
            {
                int DeviceNo = ConvertDeviceNo(Member);
                string Command = $"cmdgohome({DeviceNo},0)";
                bool Rtn = SendCommand(Command, out ResponseData);

                if (!Rtn)
                {
                    SaveLog($"Home Fail : {ResponseData}");
                    return false;
                }

                return Rtn;
            }
        }

        public void Jog(MotorMember Member, int Jog_Dist)
        {
            if (Member == MotorMember.FW1 || Member == MotorMember.FW2)
            {
                SaveLog($"Motor Member Error", true);
                return;
            }

            int DeviceNo = ConvertDeviceNo(Member);

            string Command = $"cmdrunjob({DeviceNo},{Jog_Dist})";

            string ResponseData;
            bool Rtn = SendCommand(Command, out ResponseData);

            if (!Rtn)
            {
                SaveLog($"Jog Fail : {ResponseData}");
            }

        }

        public void Stop(MotorMember Member)
        {
            string ResponseData;

            if (Member == MotorMember.All)
            {
                //bool Rtn = true;

                //for (int i = 0; i < 4; i++)
                //{
                //	string Command = $"cmdstop({i},0)";
                //	Rtn &= SendCommand(Command, out ResponseData);

                //	if (!Rtn)
                //	{
                //		SaveLog($"Stop Fail : {ResponseData}");
                //	}
                //}

                Stop(MotorMember.Focuser);
                Stop(MotorMember.Aperture);
                Stop(MotorMember.FW1);
                Stop(MotorMember.FW2);
            }
            else
            {
                int DeviceNo = ConvertDeviceNo(Member);

                bool Moving = false;

                switch (Member)
                {
                    case MotorMember.Focuser: Moving = _Focuser_Status.Moving; break;
                    case MotorMember.Aperture: Moving = _Aperture_Status.Moving; break;
                    case MotorMember.FW1: Moving = _FW1_Status.Moving; break;
                    case MotorMember.FW2: Moving = _FW2_Status.Moving; break;
                }

                if (!Moving) return;

                string Command = $"cmdstop({DeviceNo},0)";
                bool Rtn = SendCommand(Command, out ResponseData);

                if (!Rtn)
                {
                    SaveLog($"Stop Fail : {ResponseData}");
                }
            }
        }

        public void Stop()
        {
            //Stop(MotorMember.All);
            this.isMoveProc = false;
            this.isHomeProc = false;
        }

        public bool SetMcuPara(MotorSetting Focuser_Setting, MotorSetting Aperture_Setting, MotorSetting FW1_Setting, MotorSetting FW2_Setting)
        {
            _Focuser_Setting.Clone(Focuser_Setting);
            _Aperture_Setting.Clone(Aperture_Setting);
            _FW1_Setting.Clone(FW1_Setting);
            _FW2_Setting.Clone(FW2_Setting);
            string ResponseData = "";

            for (int i = 0; i < 4; i++)
            {
                MotorMember Member = (MotorMember)i;

                int DeviceNo = ConvertDeviceNo(Member);

                MotorSetting Setting = GetMotorSetting(Member);

                int[] Para = new int[]
                {
                    Setting.HomeSpeed,
                    Setting.RunSpeed,
                    Setting.ZeroPos,
                    Setting.Limit,
                    Setting.Gap,
                    Setting.CurrentLimit,
                    Setting.PosJob,
                    Setting.Enable
                };

                for (int idx = 0; idx < 8; idx++)
                {
                    string Command = $"cmdsetconfig({idx * 10 + DeviceNo},{Para[idx]})";
                    bool Rtn = SendCommand(Command, out ResponseData);

                    if (!Rtn) return false;
                }
            }

            return true;
        }

        public bool DirectMove(MotorMember Member, int Pos)
        {
            if (!CheckMovePara(Member, Pos)) return false;

            int DeviceNo = ConvertDeviceNo(Member);

            string Command = $"cmdrunpos({DeviceNo},{Pos})";

            string ResponseData = "";

            bool Rtn = SendCommand(Command, out ResponseData);

            if (!Rtn)
            {
                SaveLog($"Move Fail : {ResponseData}", true);
            }

            return Rtn;
        }


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

        #region --- Move_Proc ---
        private void Move_Proc(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut)
        {
            // Pos = -1 ==> Bypass 

            TimeManager TM = new TimeManager();
            TM.SetDelay(TimeOut);
            this.moveStep = Enum_MoveStep.Init;

            bool[] isActive = new bool[4];

            int[] TargetPos = new int[4];

            TimeManager TM_Retry = new TimeManager(3000);

            int RetryCnt = 0;

            while (this.isMoveProc)
            {
                if (TM.IsTimeOut())
                {
                    SaveLog($"Move Flow Timeout", true);
                    this.isMoveProc = false;
                    break;
                }

                switch (this.moveStep)
                {
                    case Enum_MoveStep.Init:
                        {
                            this.moveStatus_Focuser = UnitStatus.Finish;
                            this.moveStatus_Aperture = UnitStatus.Finish;
                            this.moveStatus_FW1 = UnitStatus.Finish;
                            this.moveStatus_FW2 = UnitStatus.Finish;

                            flowStatus = UnitStatus.Running;

                            isActive = new bool[]
                            {
                                FocuserPos >= 0 && Focuser_Setting.Enable == 1,
                                AperturePos >= 0 && Aperture_Setting.Enable == 1,
                                FW1Pos >= 0 && FW1_Setting.Enable == 1,
                                FW2Pos >= 0 && FW2_Setting.Enable == 1,
                            };

                            TargetPos = new int[]
                            {
                                FocuserPos,
                                AperturePos,
                                FW1Pos,
                                FW2Pos,
                            };

                            SaveLog($"Move Flow Start , Focuser = {FocuserPos} , Aperture = {AperturePos} , FW1 = {FW1Pos} , FW2 = {FW2Pos}");

                            bool DoFunction = (isActive[0] || isActive[1] || isActive[2] || isActive[3]);
                            if (!DoFunction)
                            {
                                moveStep = Enum_MoveStep.Finish;
                                break;
                            }

                            if (isActive[0]) this.moveStatus_Focuser = UnitStatus.Running;
                            if (isActive[1]) this.moveStatus_Aperture = UnitStatus.Running;
                            if (isActive[2]) this.moveStatus_FW1 = UnitStatus.Running;
                            if (isActive[3]) this.moveStatus_FW2 = UnitStatus.Running;

                            //調焦需移動，才紀錄原始位置 (補背隙)
                            //if (isActive[0] || isActive[1])
                            //{
                            //	this.moveStep = Enum_MoveStep.CheckOriPos;
                            //}
                            //else
                            {
                                this.moveStep = Enum_MoveStep.MovePos;
                            }
                        }
                        break;

                    case Enum_MoveStep.CheckOriPos:
                        {
                            bool Rtn = ReadInfo();

                            if (Rtn)
                            {
                                this.moveStep = Enum_MoveStep.GetOriPos;
                            }
                            else
                            {
                                this.moveStep = Enum_MoveStep.Alarm;
                            }
                        }
                        break;

                    case Enum_MoveStep.GetOriPos:
                        {
                            int[] OriPos =
                            {
                                    _Focuser_Status.Pos,
                                    _Aperture_Status.Pos
                             };


                            SaveLog($"Get Ori Pos , Focuser = {OriPos[0]} , Aperture = {OriPos[1]}");

                            this.moveStep = Enum_MoveStep.MovePos;
                        }
                        break;

                    case Enum_MoveStep.MovePos:
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
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                            }

                            if (Rtn)
                            {
                                RetryCnt = 0;
                                TM_Retry.SetDelay(5000);
                                this.moveStep = Enum_MoveStep.CheckInPos;
                            }
                            else
                            {
                                if (RetryCnt < 10)
                                {
                                    RetryCnt++;
                                    TM_Retry.SetDelay(300);
                                }
                                else
                                {
                                    this.moveStep = Enum_MoveStep.Alarm;
                                }
                            }
                        }
                        break;

                    case Enum_MoveStep.CheckInPos:
                        {
                            bool CheckOK = ReadInfo();

                            if (CheckOK)
                            {
                                Thread.Sleep(500);

                                bool[] Moving =
                                {
                                    _Focuser_Status.Moving,
                                    _Aperture_Status.Moving,
                                    _FW1_Status.Moving,
                                    _FW2_Status.Moving
                                };

                                bool[] InPos =
                                {
                                    (_Focuser_Status.Pos == TargetPos[0]),
                                    (_Aperture_Status.Pos == TargetPos[1]) ,
                                    (_FW1_Status.Pos == (TargetPos[2] * -30720)),
                                    (_FW2_Status.Pos == (TargetPos[3] * -30720)),
                                };

                                for (int i = 0; i < 4; i++)
                                {
                                    InPos[i] &= !Moving[i];
                                    InPos[i] |= !isActive[i];
                                }


                                // Check Finish
                                if (InPos[0]) this.moveStatus_Focuser = UnitStatus.Finish;
                                if (InPos[1]) this.moveStatus_Aperture = UnitStatus.Finish;
                                if (InPos[2]) this.moveStatus_FW1 = UnitStatus.Finish;
                                if (InPos[3]) this.moveStatus_FW2 = UnitStatus.Finish;

                                if (InPos[0] && InPos[1] && InPos[2] && InPos[3])
                                {
                                    Thread.Sleep(2000);

                                    isActive[2] = false;
                                    isActive[3] = false;

                                    //啟用且反向

                                    this.moveStep = Enum_MoveStep.Finish;

                                    SaveLog($"Move Finish");

                                }
                            }

                            {
                                if (TM_Retry.IsTimeOut())
                                {
                                    this.moveStep = Enum_MoveStep.MovePos;
                                }
                            }
                        }
                        break;

                    case Enum_MoveStep.Finish:
                        {
                            flowStatus = UnitStatus.Finish;

                            //Thread.Sleep(1500);

                            this.isMoveProc = false;
                            SaveLog($"Move Flow Finish");
                        }
                        break;


                    case Enum_MoveStep.Alarm:
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


        public virtual void Init(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 60000)
        {
            if (!this.isHomeProc)
            {
                this.isHomeProc = true;

                Thread mThread = new Thread(() => InitProc(FocuserPos, AperturePos, FilterXyzPos, FilterNdPos, TimeOut));
                mThread.Start();
            }
        }

        #region --- Init_Proc ---

        private void InitProc(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut = 120000)
        {
            TimeManager HomeTimeout = new TimeManager(TimeOut);
            TimeManager TM = new TimeManager(TimeOut);
            initStep = Enum_InitStep.Init;

            int[] TmpPos = new int[4];
            while (isHomeProc)
            {
                if (TM.IsTimeOut())
                {
                    SaveLog($"Home Flow Timeout", true);
                    this.isMoveProc = false;
                    break;
                }

                switch (initStep)
                {
                    case Enum_InitStep.Init:
                        {
                            initStep = Enum_InitStep.CheckStatus;
                            SaveLog($"Init Flow Start");

                        }
                        break;

                    case Enum_InitStep.CheckStatus:
                        {
                            bool Rtn = this.ReadInfo();
                            SaveLog($"ReadInfo");

                            if (Rtn)
                            {
                                initStep = Enum_InitStep.GoHome;
                            }
                            else
                            {
                                initStep = Enum_InitStep.Alarm;
                            }
                        }
                        break;

                    case Enum_InitStep.GoHome:
                        {
                            MotorSetting[] Setting = { _Focuser_Setting, _Aperture_Setting, _FW1_Setting, _FW2_Setting };
                            MotorStatus[] Status = { _Focuser_Status, _Aperture_Status, _FW1_Status, _FW2_Status };

                            bool NeedCheck = true;
                            bool Rtn = true;

                            for (int i = 0; i < 4; i++)
                            {
                                TmpPos[i] = Status[i].Pos;

                                if (Setting[i].Enable == 0) continue;

                                if (!Status[i].Homed)
                                {
                                    Rtn &= this.Home((MotorMember)i);
                                    SaveLog($"{(MotorMember)i} Go Home");

                                    NeedCheck = true;
                                }
                                else
                                {
                                    SaveLog($"{(MotorMember)i} Already Homed");
                                }
                            }

                            if (Rtn)
                            {
                                if (NeedCheck)
                                {
                                    HomeTimeout.SetDelay(10000);
                                    SaveLog($"Check All Homed");
                                    initStep = Enum_InitStep.CheckHomed;
                                }
                                else
                                {
                                    initStep = Enum_InitStep.MoveToBasePos;
                                }
                            }
                            else
                            {
                                initStep = Enum_InitStep.Alarm;
                            }
                        }
                        break;

                    case Enum_InitStep.CheckHomed:
                        {
                            Thread.Sleep(100);
                            bool Rtn = this.ReadInfo();

                            if (!Rtn)
                            {
                                SaveLog("Read Info Fail", true);
                                initStep = Enum_InitStep.Alarm;
                                break;
                            }

                            MotorSetting[] Setting = { _Focuser_Setting, _Aperture_Setting, _FW1_Setting, _FW2_Setting };
                            MotorStatus[] Status = { _Focuser_Status, _Aperture_Status, _FW1_Status, _FW2_Status };

                            bool[] Homed = new bool[4];
                            bool AllHomed = true;

                            for (int i = 0; i < 4; i++)
                            {
                                if (Setting[i].Enable == 0)
                                {
                                    Homed[i] = true;
                                }
                                else
                                {
                                    Homed[i] = Status[i].Homed;
                                }

                                AllHomed &= Homed[i];
                            }

                            if (AllHomed)
                            {
                                Thread.Sleep(1000);
                                initStep = Enum_InitStep.MoveToBasePos;
                            }
                            else
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (Homed[i]) continue;
                                    if (TmpPos[i] != Status[i].Pos)
                                    {
                                        HomeTimeout.SetDelay(5000);
                                        TmpPos[i] = Status[i].Pos;
                                    }
                                }

                                if (HomeTimeout.IsTimeOut())
                                {
                                    initStep = Enum_InitStep.Alarm;
                                }
                            }
                        }
                        break;

                    case Enum_InitStep.MoveToBasePos:
                        {
                            Move(FocuserPos, AperturePos, FW1Pos, FW2Pos, 60000);
                            SaveLog("Move To Base Pos");

                            initStep = Enum_InitStep.CheckInPos;
                        }
                        break;

                    case Enum_InitStep.CheckInPos:
                        {
                            if (!this.IsMoveProc())
                            {
                                switch (this.MoveFlowStatus())
                                {
                                    case UnitStatus.Finish:
                                        {
                                            SaveLog($"Move To Base Pos Succed");

                                            initStep = Enum_InitStep.Finish;
                                        }
                                        break;

                                    default:
                                        {
                                            SaveLog($"Move To Base Pos Fail", true);

                                            initStep = Enum_InitStep.Alarm;

                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case Enum_InitStep.Finish:
                        {
                            this.isHomeProc = false;
                            SaveLog("Init Flow Finish");
                        }
                        break;

                    case Enum_InitStep.Alarm:
                        {
                            this.isHomeProc = false;
                            SaveLog("Init Flow Alarm", true);
                        }
                        break;
                }
            }
        }

        #endregion


        public virtual void SetBacklash(int Backlash)
        {

        }

        public class McuInfo
        {
            // 是否感应到了传感器
            public int[] isinsenser = new int[4];

            // 是否完成gohome
            public int[] isgohome = new int[4];

            // 是否在进行位置运动
            public int[] islocationrunning = new int[4];

            public int[] isfuweirunning = new int[4];

            // 电机位置值
            public int[] location = new int[4];
        }

        public class ConfigMachine
        {
            // 归home速度
            public int[] speedhome { get; set; } = new int[4];
            // 运行速度
            public int[] speedrun { get; set; } = new int[4];
            // 软件0点值
            public int[] poszero { get; set; } = new int[4];
            // 最大posmax值
            public int[] posmax { get; set; } = new int[4];
            // 间隙值

            public int[] posgap { get; set; } = new int[4];
            // 电机电流最大值

            public int[] imax { get; set; } = new int[4];

            // job 运动长度

            public int[] posjob { get; set; } = new int[4];

            public int[] isenable { get; set; } = new int[4];


        }

        public class MotorStatus
        {
            public int Pos { get; set; }
            public bool Homed { get; set; }
            public bool InSensor { get; set; }
            public bool Moving { get; set; }
        }

        public class MotorSetting
        {
            public int HomeSpeed { get; set; }
            public int RunSpeed { get; set; }
            public int ZeroPos { get; set; }
            public int Limit { get; set; }
            public int Gap { get; set; }
            public int CurrentLimit { get; set; }
            public int PosJob { get; set; }
            public int Enable { get; set; } = 1;


            public void Clone(MotorSetting Source)
            {
                this.Enable = Source.Enable;
                this.HomeSpeed = Source.HomeSpeed;
                this.RunSpeed = Source.RunSpeed;
                this.ZeroPos = Source.ZeroPos;
                this.Limit = Source.Limit;
                this.Gap = Source.Gap;
                this.CurrentLimit = Source.CurrentLimit;
                this.PosJob = Source.PosJob;
            }
        }
    }
}