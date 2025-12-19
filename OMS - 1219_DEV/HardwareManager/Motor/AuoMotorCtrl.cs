using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using BaseTool;
using Motor.Step;

namespace HardwareManager
{
    public class AuoMotorCtrl : IMotorControl
    {
        enum MoveState
        {
            Init,
            MovePos,
            CheckInPos,
            Finish,
            Alarm,
        }

        public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;

        private clsSystemSetting SD;
        private InfoManager info;

        private ClsMotorComm m_Dev = new ClsMotorComm();

        private int[] MotorNo = new int[] { 1, 2 };
        private int[] HomeSpeed = new int[] { 200, 200 };
        private int[] MoveSpeed = new int[] { 500, 500 };
        private AuoMotorInfo[] Info = new AuoMotorInfo[] { new AuoMotorInfo(), new AuoMotorInfo(), };

        private bool isWork = false;

        private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
        private UnitStatus moveStatus_Aperture = UnitStatus.Idle;

        private UnitStatus flowStatus = UnitStatus.Idle;

        private int limitF = 140000;
        private int limitR = 0;

        public bool isMoveProc = false;

        private MoveState MoveStep = MoveState.Init;

        public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.Ascom_Wheel_Auo_Motor;

        public AuoMotorCtrl(InfoManager info, clsSystemSetting SD)
        {
            m_Dev = new ClsMotorComm();
            m_Dev.DebugMode = true;

            this.info = info;
            this.SD = SD;

            this.isWork = false;
        }

        #region --- Interface 成員函式 ---

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
            return UnitStatus.Finish;
        }

        public UnitStatus MoveStatus_FW2()
        {
            return UnitStatus.Finish;
        }

        public UnitStatus MoveFlowStatus()
        {
            return this.flowStatus;
        }

        public bool IsWork()
        {
            return this.isWork;
        }

        public bool IsMoveProc()
        {
            return this.isMoveProc;
        }

        public int NowPos_Focuser()
        {
            this.GetStatus(0);
            return this.Info[0].Position;
        }

        public int NowPos_Aperture()
        {
            this.GetStatus(1);
            return this.Info[1].Position;
        }

        public virtual int NowPos_FW1()
        {
            return -1;
        }

        public virtual int NowPos_FW2()
        {
            return -1;
        }

        public double NowVelocity_Focuser()
        {
            this.GetStatus(0);
            return this.Info[0].Speed;
        }

        public double NowVelocity_Aperture()
        {
            this.GetStatus(1);
            return this.Info[1].Speed;
        }

        public bool IsHome_Focuser()
        {
            this.GetStatus(0);
            return this.Info[0].Home;
        }

        public bool IsHome_Aperture()
        {
            this.GetStatus(1);
            return this.Info[1].Home;
        }

        public int Focus_LimitF()
        {
            return limitF;
        }


        public int Focus_LimitR()
        {
            return limitR;
        }

        #endregion --- Property ---

        #region --- Method ---

        #region --- isConnect ---

        public bool isConnect()
        {
            return this.m_Dev.IsOpen();
        }

        #endregion

        #region --- SaveLog ---

        public void SaveLog(string Log, bool isAlm = false)
        {
            if (isAlm)
            {
                info.Error($"[Auo Motor] {Log}");
            }
            else
            {
                info.General($"[Auo Motor] {Log}");
            }
        }

        #endregion

        #region --- Set_HomeSpeed ---

        public bool Set_HomeSpeed(int MotorIdx, int Speed)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            this.HomeSpeed[MotorIdx] = Speed;
            return true;
        }

        #endregion

        #region --- Set_MoveSpeed ---

        public bool Set_MoveSpeed(int MotorIdx, int Speed)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            this.MoveSpeed[MotorIdx] = Speed;
            return true;
        }

        #endregion

        #region --- SetEnable ---

        public bool SetEnable(MotorMember Member, bool OnOff)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                int No = MotorNo[(int)Member];

                this.m_Dev.SetEnable(No, OnOff);
                GetStatus(No);
                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Port is not Open : {ex.Message}", true);
                return false;
            }
        }

        #endregion

        #region --- SetBacklash ---

        public bool SetBacklash(int No, int Gap)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                this.m_Dev.SetBlacklash(MotorNo[No], Gap);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Port is not Open : {ex.Message}", true);

                return false;
            }
        }

        #endregion

        #region --- GetBacklash ---

        public int GetBacklash(int No)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return 0;
            }

            try
            {
                int Gap = this.m_Dev.GetBlacklash(MotorNo[No]);

                return Gap;
            }
            catch (Exception ex)
            {
                SaveLog($"GetBlacklash Fail : {ex.Message}", true);
                return 0;
            }
        }

        #endregion


        #region --- SetLimitF ---

        public bool SetLimitF(int No, int Pos)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return true;
            }

            try
            {
                this.m_Dev.SetMaxPos(MotorNo[No], Pos);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"SetLimitF Fail : {ex.Message}", true);

                return false;
            }
        }

        #endregion

        #region --- GetLimitF ---

        public int GetLimitF(int No)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return 0;
            }

            try
            {
                int Pos = this.m_Dev.GetMaxPos(MotorNo[No]);

                return Pos;
            }
            catch (Exception ex)
            {
                SaveLog($"GetLimitF Fail : {ex.Message}", true);

                return 0;
            }
        }

        #endregion

        #region --- SetLimitR ---

        public bool SetLimitR(int No, int Pos)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return true;
            }

            try
            {
                this.m_Dev.SetMinPos(MotorNo[No], Pos);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"SetLimitR Fail : {ex.Message}", true);

                return false;
            }
        }

        #endregion

        #region --- GetLimitR ---

        public int GetLimitR(int No)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return 0;
            }

            try
            {
                int Pos = this.m_Dev.GetMinPos(MotorNo[No]);

                return Pos;
            }
            catch (Exception ex)
            {
                SaveLog($"GetLimitR Fail : {ex.Message}", true);

                return 0;
            }
        }

        #endregion

        #region --- GetStatus ---

        public AuoMotorInfo GetStatus(int MotorIdx)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return new AuoMotorInfo();
            }

            try
            {
                ClsMotorComm.Status Status = this.m_Dev.GetStatus(MotorNo[MotorIdx]);

                Info[MotorIdx].Enable = Status.Enable;
                Info[MotorIdx].Position = Status.Position;
                Info[MotorIdx].Home = Status.Home;
                Info[MotorIdx].Limit = Status.Limit;
                Info[MotorIdx].Moving = Status.Moving;
                Info[MotorIdx].Target = Status.Target;
                Info[MotorIdx].Speed = Status.Speed;

                return Info[MotorIdx];

            }
            catch (Exception)
            {

                return new AuoMotorInfo();

            }
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
                string PortNo = new string(Com_Port.Where(char.IsDigit).ToArray());
                int Port = int.Parse(PortNo);

                this.m_Dev.TimeOut = 3000;
                this.m_Dev.Open(Port, 9600, 8, StopBits.One, Parity.None);

                this.isWork = true;

                limitF = this.GetLimitF(0);
                limitR = this.GetLimitR(0);

                SaveLog($"Open Port : {Com_Port}");
            }
            catch (Exception ex)
            {
                SaveLog($"Open Fail - {ex.Message}", true);
                this.isWork = false;
            }
        }

        public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
        {

        }

        #endregion

        #region --- Close ---

        public void Close()
        {
            this.isMoveProc = false;

            try
            {
                if (this.m_Dev != null)
                {
                    this.m_Dev.Close();
                }

                this.isWork = false;
            }
            catch (Exception ex)
            {
                SaveLog($"Close Fail - {ex.Message}", true);
            }
        }

        #endregion

        #region --- GetPosition ---

        public int GetPosition(MotorMember Member)
        {

            this.GetStatus((int)Member);
            return this.Info[(int)Member].Position;
        }

        #endregion

        #region --- GetVelocity ---

        public double GetVelocity(MotorMember Member)
        {
            this.GetStatus((int)Member);
            return this.Info[(int)Member].Speed;
        }

        #endregion

        #region --- Reset ---

        public void Reset(MotorMember Member)
        {
            return;
        }

        #endregion

        #region --- Home ---

        public bool Home(MotorMember Member)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return true;
            }

            try
            {
                int No = MotorNo[(int)Member];
                int Speed = HomeSpeed[(int)Member];

                this.m_Dev.SearchHome(No, Speed, 10 * 1000);
                GetStatus(No);
                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Home Fail : {ex.Message}", true);

                return false;
            }
        }

        public bool Home(MotorMember MemberNo, int Timeout)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return true;
            }

            try
            {
                int No = MotorNo[(int)MemberNo];
                int Speed = HomeSpeed[(int)MemberNo];

                this.m_Dev.SearchHome(No, Speed, Timeout);
                GetStatus(No);
                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Home Fail : {ex.Message}", true);

                return false;
            }
        }

        #endregion

        #region --- Jog ---

        public void Jog(MotorMember Member, int Jog_Dist)
        {
            return;
        }

        #endregion

        #region --- Stop ---

        public void Stop(MotorMember Member)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return;
            }

            int No = MotorNo[(int)Member];

            this.m_Dev.Stop(No);
        }

        #endregion

        #region --- DirectMove ---

        public bool DirectMove(MotorMember Member, int Position)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                int No = MotorNo[(int)Member];
                int Speed = MoveSpeed[(int)Member];

                this.m_Dev.AbsMove(No, Position, Speed, SD.AuoMotor_MoveTimeout);
                //this.m_Dev.AbsMove(1, Position, 500, 200000);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"DirectMove Fail : {ex.Message}", true);

                return false;
            }
        }

        public bool AbsMove(MotorMember Member, int Position, int Timeout)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                int No = MotorNo[(int)Member];
                int Speed = MoveSpeed[(int)Member];

                this.m_Dev.AbsMove(No, Position, Speed, Timeout);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"AbsMove Fail : {ex.Message}", true);

                return false;
            }
        }

        public bool RelMove(MotorMember Member, int Position, int Timeout)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                int No = MotorNo[(int)Member];
                int Speed = MoveSpeed[(int)Member];

                this.m_Dev.RelMove(No, Position, Speed, Timeout);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"RelMove Fail : {ex.Message}", true);

                return false;
            }
        }

        public bool GoHome(MotorMember Member, int Timeout)
        {
            if (!m_Dev.IsOpen())
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            try
            {
                int No = MotorNo[(int)Member];
                int Speed = MoveSpeed[(int)Member];

                this.m_Dev.GoHome(No, Speed, Timeout);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"GoHome Fail : {ex.Message}", true);

                return false;
            }
        }

        #endregion

        #region --- Move ---

        public virtual void Move(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 600000)
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

        public void Move_Proc(int FocuserPos, int AperturePos, int TimeOut)
        {
            TimeManager TM = new TimeManager();
            TimeOut = 600000;
            TM.SetDelay(TimeOut);
            this.MoveStep = MoveState.Init;

            bool[] isActive = new bool[]
            {
                FocuserPos >=0,
                AperturePos >=0,
            };

            int[] TargetPos = new int[]
            {
                FocuserPos,
                AperturePos,
            };

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
                            string Log = "Move Flow Start";

                            bool FunctionCheck = false;
                            for (int i = 0; i < isActive.Length; i++)
                            {
                                if (isActive[i])
                                {
                                    MotorMember Member = (MotorMember)i;
                                    Log += $" , {Member.ToString()} = {TargetPos[i]}";
                                    FunctionCheck = true;
                                }
                            }

                            if (isActive[0])
                            {
                                this.moveStatus_Focuser = UnitStatus.Running;
                                this.Set_MoveSpeed(MotorNo[0], SD.AuoMotor_MoveSpeed);
                            }

                            if (isActive[1])
                            {
                                this.moveStatus_Aperture = UnitStatus.Running;
                                this.Set_MoveSpeed(MotorNo[1], SD.AuoMotor_MoveSpeed);
                            }

                            if (FunctionCheck)
                            {
                                MoveStep = MoveState.MovePos;
                                SaveLog(Log);
                            }
                            else
                            {
                                MoveStep = MoveState.Alarm;
                                SaveLog($"Move Flow Start , Target Pos Error , Alarm ", true);
                            }
                        }
                        break;

                    case MoveState.MovePos:
                        {
                            bool Rtn = true;

                            for (int i = 0; i < isActive.Length; i++)
                            {
                                if (isActive[i])
                                {
                                    MotorMember Member = (MotorMember)i;
                                    int Pos = TargetPos[i];

                                    Thread MoveThread = new Thread(() => DirectMove(Member, Pos));
                                    MoveThread.Start();

                                    SaveLog($"{Member} Move to {TargetPos[i]}");
                                }
                            }

                            if (Rtn)
                            {
                                this.MoveStep = MoveState.CheckInPos;
                            }
                            else
                            {
                                this.MoveStep = MoveState.Alarm;
                            }
                        }
                        break;

                    case MoveState.CheckInPos:
                        {
                            bool[] Check = new bool[isActive.Length];
                            bool Finish = true;

                            for (int i = 0; i < isActive.Length; i++)
                            {
                                if (isActive[i])
                                {
                                    AuoMotorInfo GetInfo = GetStatus(i);

                                    if (GetInfo.Position == TargetPos[i] && !GetInfo.Moving)
                                    {
                                        MotorMember Member = (MotorMember)i;

                                        SaveLog($"{Member} Check InPos OK");

                                        Check[i] = true;
                                        isActive[i] = false;
                                    }
                                    else
                                    {
                                        Check[i] = false;
                                    }
                                }
                                else
                                {
                                    Check[i] = true;
                                }

                                Finish &= Check[i];
                            }

                            if (Finish)
                            {
                                this.MoveStep = MoveState.Finish;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        break;

                    case MoveState.Finish:
                        {
                            Thread.Sleep(1000);

                            this.moveStatus_Focuser = UnitStatus.Finish;
                            this.moveStatus_Aperture = UnitStatus.Finish;
                            this.isMoveProc = false;

                            flowStatus = UnitStatus.Finish;
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

        #endregion

        public virtual void SetBacklash(int Backlash)
        {

        }

        #endregion --- Method ---
    }

    public class AuoMotorInfo
    {
        public bool Enable;
        public bool Home;
        public bool Limit;
        public bool Moving;
        public int Position;
        public int Target;
        public int Speed;

    }

}


