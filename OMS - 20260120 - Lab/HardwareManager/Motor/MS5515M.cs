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

    #region --- Class[2] : MS5515M ---

    public class MS5515M : IMotorControl
    {
        // for Tool
        public event Action<string, string, string, string> UpdateInfo;

        public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;

        private InfoManager info;

        private int Resolution = 16384;

        private SerialPort SPort;
        private bool isRefreshData = false;
        private bool isWork = false;
        public bool isMoveProc = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus flowStatus = UnitStatus.Idle;

        private int limitF = 280000;
        private int limitR = 0;

        private MotorInfo _Focuser = new MotorInfo();

        private MoveState MoveStep = MoveState.Init;

        private enum MoveState
        {
            Init,
            CheckOriPos,
            CalOffset,
            RelMovePos,
            CheckInPos,
            Finish,
            Alarm,
        }

        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.MS5515M;

        public MS5515M(InfoManager info)
        {
            this.info = info;
            this.SPort = new SerialPort();
            SPort.DtrEnable = true;
            SPort.DataReceived -= AiryPort_DataReceived;
            SPort.DataReceived += AiryPort_DataReceived;
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
            return UnitStatus.Finish;
        }

        #endregion --- MoveStatus_Aperture ---

        #region --- MoveStatus_FW1 ---

        public UnitStatus MoveStatus_FW1()
        {
            return UnitStatus.Finish;
        }

        #endregion --- MoveStatus_FW1 ---

        #region --- MoveStatus_FW2 ---

        public UnitStatus MoveStatus_FW2()
        {
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
            return -1;
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
            return -1;
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
            return true;
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
            return this.SPort.IsOpen;
        }

        #endregion --- isConnect ---

        #region --- SaveLog ---

        public void SaveLog(string Log, bool isAlm = false)
        {
            if (isAlm)
            {
                info.Error($"[MS5515M] {Log}");
            }
            else
            {
                info.General($"[MS5515M] {Log}");
            }
        }

        #endregion --- SaveLog ---

        #region --- MS5515M Command ---

        public bool Set_Speed(int Speed)
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            _Focuser.Speed = Speed;

            return true;
        }

        public bool RelMove_Pos(int Pos)
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            double Speed = _Focuser.Speed;
            int V = (int)Speed * Resolution / 6000;
            byte[] SpeedBytes = IntToFourByteArray(V, true);

            byte[] PosBytes = IntToFourByteArray(Pos, true);

            byte[] Datas = new byte[]
            {
                0x01,
                0x64,
                0x01,
                SpeedBytes[0],
                SpeedBytes[1],
                SpeedBytes[2],
                SpeedBytes[3],
                PosBytes[0],
                PosBytes[1],
                PosBytes[2],
                PosBytes[3],
                0x00,
                0x00,
                0x00
            };
            Datas = CalculateModbusCRC16(Datas);

            string Result = "";

            foreach (byte Data in Datas)
            {
                Result += $"{Data.ToString("X2")} ";
            }

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool RelPos_Angle(int Angle)
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            double Speed = _Focuser.Speed;

            int V = (int)Speed * Resolution / 6000;
            byte[] SpeedBytes = IntToFourByteArray(V, true);

            int A = Angle * Resolution / 360;
            byte[] PosBytes = IntToFourByteArray(A, true);

            byte[] Datas = new byte[]
            {
                0x01,
                0x64,
                0x01,
                SpeedBytes[0],
                SpeedBytes[1],
                SpeedBytes[2],
                SpeedBytes[3],
                PosBytes[0],
                PosBytes[1],
                PosBytes[2],
                PosBytes[3],
                0x00,
                0x00,
                0x00
            };
            Datas = CalculateModbusCRC16(Datas);

            string Result = "";

            foreach (byte Data in Datas)
            {
                Result += $"{Data.ToString("X2")} ";
            }

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool TeachSwtich(bool OnOff)
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            int SwitchByps = OnOff ? 1 : 0;
            byte[] Datas = new byte[]
            {
                0x01,
                0x6A,
                0x03,
                (byte)SwitchByps,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };

            Datas = CalculateModbusCRC16(Datas);

            string Result = "";

            foreach (byte Data in Datas)
            {
                Result += $"{Data.ToString("X2")} ";
            }

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool SearchHome()
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            byte[] Datas = new byte[]
            {
                0x01,
                0x64,
                0x06,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };

            Datas = CalculateModbusCRC16(Datas);

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool GoHome()
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            byte[] Datas = new byte[]
            {
                0x01,
                0x64,
                0x07,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };

            Datas = CalculateModbusCRC16(Datas);

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool MoveStop()
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            byte[] Datas = new byte[]
            {
                0x01,
                0x64,
                0x01,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };

            Datas = CalculateModbusCRC16(Datas);

            SPort.Write(Datas, 0, Datas.Length);

            return true;
        }

        public bool CheckDevice()
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            byte[] Datas = new byte[]
            {
                0x01,
                0x65,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };

            Datas = CalculateModbusCRC16(Datas);


            SPort.Write(Datas, 0, Datas.Length);

            String Result = "";
            foreach (byte Data in Datas)
            {
                Result += $"{Data.ToString("X2")} ";
            }

            return true;
        }

        #endregion

        #region --- Open ---

        public void Open()
        {

        }

        public void Open(string Com_Port)
        {
            try
            {
                if (!SPort.IsOpen)
                {
                    SPort.BaudRate = 115200;
                    SPort.PortName = Com_Port;
                    SPort.Open();

                    Thread.Sleep(500);
                    TeachSwtich(false);

                    this.isWork = true;

                }
                else
                {
                    SPort.Close();
                    SaveLog($"Port Close");
                    Open(Com_Port);

                    Thread.Sleep(1000);

                }

                _Focuser.Speed = 30;

                SaveLog($"Open Port : {Com_Port}");
                this.isWork = true;
            }
            catch (Exception ex)
            {
                SaveLog($"Port Open Fail - {ex.Message}", true);
            }
        }


        public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
        {

        }

        #endregion --- Open ---

        #region --- Close ---

        public void Close()
        {
            if (this.SPort.IsOpen)
            {
                this.SPort.Close();
                this.isWork = false;
            }
            this.isMoveProc = false;
        }

        #endregion --- Close ---

        #region --- AiryPort_DataReceived ---

        private string ReviceData = "";

        private void AiryPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytes = sp.BytesToRead;

            byte[] buffer = new byte[bytes];
            sp.Read(buffer, 0, bytes);

            string[] Hex = new string[bytes];

            for (int i = 0; i < bytes; i++)
            {
                Hex[i] = buffer[i].ToString("X2");
                ReviceData += (Hex[i] + " ");
            }

            if (ReviceData.Contains("01 65 "))
            {
                int Idx = ReviceData.IndexOf("01 65");

                if (ReviceData.Length > Idx + 16 * 3 - 1)
                {
                    string[] SubString = ReviceData.Substring(Idx, 16 * 3 - 1).Split(' ');

                    string Hex_AbsPos = SubString[4] + SubString[5] + SubString[6] + SubString[7];
                    string Hex_RelPos = SubString[8] + SubString[9] + SubString[10] + SubString[11];
                    string Hex_Frequence = SubString[12] + SubString[13];

                    int Status = Convert.ToInt32(SubString[3], 16);
                    int AbsPos = Convert.ToInt32(Hex_AbsPos, 16);
                    int RelPos = Convert.ToInt32(Hex_RelPos, 16);
                    int Frequence = Convert.ToInt32(Hex_Frequence, 16);

                    _Focuser.Position = RelPos;
                    //MotorBusy = (Status != 0);

                    this.isRefreshData = true;
                    UpdateStatus?.Invoke(_Focuser, null, null, null);

                    //for UI
                    UpdateInfo?.Invoke($"{Status}", $"{AbsPos}", $"{RelPos}", $"{Frequence}");

                    ReviceData = "";
                }
            }
        }

        #endregion --- AiryPort_DataReceived ---

        #region --- Move_Proc ---
        private void Move_Proc(int FocuserPos, int TimeOut)
        {
            // Pos = -1 ==> Bypass 
            int Backlash = 3000;
            TimeManager TM = new TimeManager();
            TimeManager TM_Receive = new TimeManager();

            TM.SetDelay(TimeOut);
            this.MoveStep = MoveState.Init;

            int TargetPos = FocuserPos;

            int Offset = 0;

            int Dir = 1;

            int ReCheck = 0;

            while (this.isMoveProc)
            {
                if (TM.IsTimeOut())
                {
                    SaveLog($"Move Flow Running", true);
                    this.isMoveProc = false;
                    break;
                }

                switch (this.MoveStep)
                {
                    case MoveState.Init:
                        {
                            flowStatus = UnitStatus.Running;
                            this.moveStatus_Focuser = UnitStatus.Running;

                            SaveLog($"Move Flow Start , Focuser = {FocuserPos}");

                            this.MoveStep = MoveState.CheckOriPos;
                        }
                        break;

                    case MoveState.CheckOriPos:
                        {
                            ReCheck = 0;
                            isRefreshData = false;

                            bool Rtn = this.CheckDevice();

                            if (Rtn)
                            {
                                SaveLog($"Check Device");
                                TM_Receive.SetDelay(3000);
                                this.MoveStep = MoveState.CalOffset;
                            }
                            else
                            {
                                SaveLog($"Check Device Fail", true);
                                this.MoveStep = MoveState.Alarm;
                            }
                        }
                        break;

                    case MoveState.CalOffset:
                        {
                            if (isRefreshData)
                            {
                                int NowPos = _Focuser.Position;

                                if (FocuserPos > NowPos) //往正，需補背隙
                                {
                                    Dir = 1;
                                    TargetPos = FocuserPos + Backlash;
                                }
                                else
                                {
                                    TargetPos = FocuserPos;
                                    Dir = -1;
                                }

                                Offset = TargetPos - NowPos;

                                this.MoveStep = MoveState.RelMovePos;
                            }
                            else
                            {
                                if (TM_Receive.IsTimeOut())
                                {
                                    this.MoveStep = MoveState.CheckOriPos;
                                }
                            }
                        }
                        break;

                    case MoveState.RelMovePos:
                        {
                            bool Rtn = this.DirectMove(MotorMember.Focuser, Offset);

                            if (Rtn)
                            {
                                SaveLog($"Focuser Move to {TargetPos}");
                                this.MoveStep = MoveState.CheckInPos;
                                TM_Receive.SetDelay(5000);
                            }
                            else
                            {
                                SaveLog($"Focuser Move Fail", true);
                                this.MoveStep = MoveState.Alarm;
                            }
                        }
                        break;

                    case MoveState.CheckInPos:
                        {
                            bool InPos = (_Focuser.Position - 10 <= TargetPos && _Focuser.Position + 10 >= TargetPos);

                            // Check Finish
                            if (InPos)
                            {
                                SaveLog($"InPos = {_Focuser.Position}");

                                this.moveStatus_Focuser = UnitStatus.Finish;
                                Thread.Sleep(300);

                                if (Dir == 1) //往正，有補背隙
                                {
                                    this.MoveStep = MoveState.CalOffset;
                                }
                                else
                                {
                                    SaveLog($"Move Finish");
                                    this.MoveStep = MoveState.Finish;
                                }
                            }
                            else
                            {
                                if (TM_Receive.IsTimeOut())
                                {
                                    TM_Receive.SetDelay(3000);

                                    ReCheck++;
                                    this.CheckDevice();

                                    if (ReCheck > 5)
                                    {
                                        Thread.Sleep(1000);
                                        this.MoveStop();
                                        Thread.Sleep(1000);

                                        this.MoveStep = MoveState.CheckOriPos;

                                    }
                                }
                            }
                        }
                        break;

                    case MoveState.Finish:
                        {
                            flowStatus = UnitStatus.Finish;

                            this.isMoveProc = false;
                            SaveLog($"Move Flow Finish");
                        }
                        break;


                    case MoveState.Alarm:
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

                default:
                    return -1;
            }
        }

        #endregion --- GetPosition ---

        #region --- GetVelocity ---

        public double GetVelocity(MotorMember Member)
        {
            switch (Member)
            {
                case MotorMember.Focuser:
                    return this._Focuser.Speed;
            }

            return -1;
        }

        #endregion --- GetVelocity ---

        #region --- Reset ---

        public void Reset(MotorMember Member)
        {
            return;
        }

        #endregion --- Reset ---

        #region --- Home ---

        public bool Home(MotorMember Member)
        {
            return this.SearchHome();
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

                        Thread PreprocessThread = new Thread(() => Move_Proc(this._Focuser.Position + Jog_Dist, 10000));
                        PreprocessThread.Start();
                    }
                    break;
            }
        }
        #endregion --- Jog ---"

        #region --- Stop ---

        public void Stop(MotorMember device)
        {
            if (device == MotorMember.Focuser)
            {
                this.MoveStop();
                isMoveProc = false;
            }
        }

        #endregion

        #region --- DirectMove ---

        public bool DirectMove(MotorMember Member, int Position)
        {
            if (!SPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            switch (Member)
            {
                case MotorMember.Focuser:
                    {
                        if (Position > 300000)
                            return false; ;  //預防超過 140000
                        RelMove_Pos(Position);
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

                Thread PreprocessThread = new Thread(() => Move_Proc(FocuserPos, TimeOut));
                PreprocessThread.Start();
            }
        }

        #endregion

        #endregion --- Command ---

        #region Data Convert

        public byte[] CalculateModbusCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            byte[] result = data.Concat(new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) }).ToArray();

            return result;
        }

        public byte[] IntToFourByteArray(int value, bool bigEndian = false)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            // 確保數組長度為4
            Array.Resize(ref bytes, 4);

            // 如果系統是little-endian(大多數系統)且要求big-endian,則需要反轉數組
            if (BitConverter.IsLittleEndian && bigEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        #endregion
    }

    #endregion --- Class[2] : AiryUnitCtrl_4in1 ---



}
