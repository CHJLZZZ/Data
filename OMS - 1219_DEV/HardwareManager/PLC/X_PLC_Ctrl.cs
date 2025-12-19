using BaseTool;
using CommonBase.Logger;
using Mitsubishi.Ether;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareManager
{
    public class X_PLC_Ctrl
    {
        public event Action<PlcData> UpdateUI;
        private MxLCPUUDP m_Mx;
        private string IP;
        private int Timeout;

        private InfoManager info;

        public PlcData Plc2Pc = new PlcData();

        private bool Scaning = false;
        private bool ScanMemory = false;

        public bool IsOpen { get => m_Mx.IsOpen(); }

        private bool JogRun = false;
        private bool MoveRun = false;
        public bool IsRun { get => MoveRun; }

        private MoveStatus FlowStatus = MoveStatus.Running;
        public MoveStatus Status { get => FlowStatus; }


        public X_PLC_Ctrl(InfoManager info)
        {
            m_Mx = new MxLCPUUDP();
            this.info = info;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            if (!isAlm)
            {
                info.General($"[X PLC] {Msg}");
            }
            else
            {
                info.Error($"[X PLC] {Msg}");
            }
        }

        #region Connect

        public bool Connect(string IP, int Timeout)
        {
            try
            {
                m_Mx.Close();

                Thread.Sleep(500);

                this.IP = IP;
                this.Timeout = Timeout;

                m_Mx.TimeOut = this.Timeout;
                m_Mx.Open(this.IP);

                SaveLog($"Connect Success - {this.IP}");

                ScanMemory = true;
                Thread PlcThread = new Thread(new ThreadStart(PlcFlow));
                PlcThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Connect Fail - {ex.Message}", true);
                return false;
            }
        }

        public bool Reconnect()
        {
            Thread ReconnectThread = new Thread(new ThreadStart(ReconnectFlow));
            ReconnectThread.Start();

            return true;
        }

        private void ReconnectFlow()
        {
            try
            {
                //中止掃描流程
                ScanMemory = false;

                //確認掃描流程中止
                while (Scaning)
                {

                }

                Connect(this.IP, this.Timeout);
            }
            catch (Exception ex)
            {
                SaveLog(ex.Message);
            }
        }

        #endregion

        #region Scan Memory

        private void PlcFlow()
        {
            Scaning = true;

            while (ScanMemory)
            {
                Thread.Sleep(100);
                ReciveData();
                //UpdataData();

                UpdateUI?.Invoke(this.Plc2Pc);
            }
            Thread.Sleep(100);

            Scaning = false;
        }

        private void ReciveData()
        {
            try
            {
                this.Plc2Pc.AlarmCode = ReadInt("D1100");
                this.Plc2Pc.WarningCode = ReadInt("D1101");

                this.Plc2Pc.AxisReady = ReadInt("M1110");
                this.Plc2Pc.ServoOn = ReadInt("M1111");
                this.Plc2Pc.ServoInPos = ReadInt("M1113");
                this.Plc2Pc.TorqueImt = ReadInt("M1115");
                this.Plc2Pc.ServoAlarm = ReadInt("M1116");
                this.Plc2Pc.ServoWar = ReadInt("M1117");
                this.Plc2Pc.OPRReq = ReadInt("M1103");
                this.Plc2Pc.OPRCmp = ReadInt("M1104");
                this.Plc2Pc.LimitR = ReadInt("M1100");
                this.Plc2Pc.ORG = ReadInt("M1102");
                this.Plc2Pc.LimitF = ReadInt("M1101");

                this.Plc2Pc.PosCmp = ReadInt("X14");
                this.Plc2Pc.AxisBusy = ReadInt("X0C");

                this.Plc2Pc.JogSpeed = ReadInt32("D1103");
                this.Plc2Pc.MovePos = ReadInt32("D1105") / 10.0;
                this.Plc2Pc.MoveSpeed = ReadInt32("D1109");

                this.Plc2Pc.AxisPos = ReadInt32("D1107") / 10.0;
            }
            catch (Exception ex)
            {
                //SaveLog($"ReciveData Fail :{ex.Message}",true);
            }
        }

        #endregion

        #region Read & Write

        public int[] ReadDevice(string Block, int Size)
        {
            int[] Ressult = new int[Size];
            this.m_Mx.ReadDeviceBlock(Block, Size, ref Ressult);

            return Ressult;
        }

        public int ReadInt(string Block)
        {
            int Data = this.m_Mx.GetDevice(Block);
            //int Data = this.m_Mx.ReadInt32(Block);

            return Data;
        }

        public int ReadInt32(string Block)
        {
            int Data = this.m_Mx.ReadInt32(Block);

            return Data;
        }

        private void WriteDevice(string Block, int Size, int[] Bits)
        {
            this.m_Mx.WriteDeviceBlock(Block, Size, Bits);
        }

        public void WriteInt(string Block, int Bits)
        {
            try
            {
                this.m_Mx.WriteInt32(Block, Bits);
            }
            catch (Exception)
            {
                WriteInt(Block, Bits);
            }
            //this.m_Mx.SetDevice(Block, Bits);
        }

        #endregion

        #region Method

        public void ServoOn()
        {
            WriteInt("X1000", 1);
        }

        public void ServoOff()
        {
            WriteInt("X1000", 0);
        }

        public void SetMovePos(double MovePos)
        {
            WriteInt("D1105", (int)(MovePos * 10.0));
        }

        public void SetMoveSpeed(int Speed)
        {
            WriteInt("D1109", Speed);
        }

        public void SetJogSpeed(int Speed)
        {
            WriteInt("D1103", Speed);
        }

        public void Stop()
        {
            WriteInt("X1011", 1);
            Thread.Sleep(500);
            WriteInt("X1011", 0);

            WriteInt("X1013", 0);
            WriteInt("X1014", 0);
            WriteInt("X1015", 0);
            WriteInt("X1016", 0);
        }

        public void AlarmReset()
        {
            WriteInt("X1001", 1);
            Thread.Sleep(500);
            WriteInt("X1001", 0);
        }

        public void Home()
        {
            WriteInt("X1012", 1);
            Thread.Sleep(500);
            WriteInt("X1012", 0);
        }

        public void JogStart(bool Forward)
        {
            string Block = (Forward) ? "X1013" : "X1014";

            JogRun = true;

            Thread JogThread = new Thread(() =>
            {
                //Jog On
                WriteInt(Block, 1);

                while (JogRun)
                {
                    //Wait Jog Off
                }

                //Jog Off
                WriteInt(Block, 0);
                Stop();
            });

            JogThread.Start();
        }

        public void JogStop()
        {
            JogRun = false;
        }

        public void Move(MoveMode Mode)
        {
            string Block = (Mode == MoveMode.AbvMove) ? "X1015" : "X1016";

            WriteInt(Block, 1);
            Thread.Sleep(5000);
            WriteInt(Block, 0);
        }

        public void AbsMove(double Pos)
        {
            string Block = "X1015";

            if (!MoveRun)
            {
                MoveRun = true;

                Thread MoveThread = new Thread(() =>
                {
                    TimeManager TM = new TimeManager(60 * 1000);

                    int Step = 0;
                    FlowStatus = MoveStatus.Running;

                    while (MoveRun)
                    {
                        if (TM.IsTimeOut())
                        {
                            FlowStatus = MoveStatus.Alarm;
                            MoveRun = false;
                            break;
                        }

                        switch (Step)
                        {
                            case 0: //Set Pos
                                {
                                    SetMovePos(Pos);
                                    Thread.Sleep(500);
                                    Step = 1;
                                }
                                break;

                            case 1: //Check InPos
                                {
                                    if (this.Plc2Pc.MovePos == Pos)
                                    {
                                        Step = 2;
                                    }
                                    else
                                    {
                                        Step = 0;
                                    }
                                }
                                break;

                            case 2: //Move On
                                {
                                    WriteInt(Block, 1);
                                    Thread.Sleep(500);
                                    Step = 3;
                                }
                                break;

                            case 3:
                                {
                                    if (this.Plc2Pc.AxisBusy == 0)
                                    {
                                        if (Math.Abs(this.Plc2Pc.AxisPos - Pos) < 10)
                                        {
                                            FlowStatus = MoveStatus.Finish;
                                            MoveRun = false;
                                        }
                                    }
                                    else
                                    {
                                        Step = 2;
                                    }
                                }
                                break;
                        }
                    }

                    WriteInt(Block, 0);
                    Stop();
                });

                MoveThread.Start();
            }
            else
            {
                MoveRun = false;
                Thread.Sleep(100);
                AbsMove(Pos);
            }
        }


        #endregion

        public class PlcData
        {
            [DisplayName("Alarm Code")]
            public int AlarmCode { get; set; } = 0;

            [DisplayName("Warning Code")]
            public int WarningCode { get; set; } = 0;

            [DisplayName("Axis Ready")]
            public int AxisReady { get; set; } = 0;

            [DisplayName("Servo On")]
            public int ServoOn { get; set; } = 0;

            [DisplayName("Servo InPos")]
            public int ServoInPos { get; set; } = 0;

            [DisplayName("Torque Imt")]
            public int TorqueImt { get; set; } = 0;

            [DisplayName("Servo Alarm")]
            public int ServoAlarm { get; set; } = 0;

            [DisplayName("Servo War")]
            public int ServoWar { get; set; } = 0;

            [DisplayName("Pos. Cmp.")]
            public int PosCmp { get; set; } = 0;

            [DisplayName("OPR. Req.")]
            public int OPRReq { get; set; } = 0;

            [DisplayName("OPR. Cmp.")]
            public int OPRCmp { get; set; } = 0;

            [DisplayName("Limit-")]
            public int LimitR { get; set; } = 0;

            [DisplayName("ORG")]
            public int ORG { get; set; } = 0;

            [DisplayName("Limit+")]
            public int LimitF { get; set; } = 0;

            [DisplayName("[Jog Para] Jog Speed")]
            public int JogSpeed { get; set; } = 1;

            [DisplayName("[Move Para] Move Speed")]
            public int MoveSpeed { get; set; } = 0;

            [DisplayName("[Move Para] Move Pos")]
            public double MovePos { get; set; } = 0;

            [DisplayName("[Axis Status] Axis Busy")]
            public int AxisBusy { get; set; } = 0;

            [DisplayName("[Axis Status] Axis Pos")]
            public double AxisPos { get; set; } = 0;
        }

        public enum MoveMode
        {
            AbvMove,
            IncMove,
        }

        public enum MoveStatus
        {
            Running,
            Finish,
            Alarm,
        }
    }


}
