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
using static Emgu.Util.Platform;
using static OpticalMeasuringSystem.SequentialMove_FlowPara;

namespace OpticalMeasuringSystem
{
    public class Task_SequentialMove
    {
        public event Action<Rectangle> DrawBlock;
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;
        private SequentialMove_FlowPara FlowPara = new SequentialMove_FlowPara();

        //Flow
        private bool FlowRun = false;

        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_SequentialMove(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            if (!isAlm)
            {
                this.Info.General($"{Msg}");
            }
            else
            {
                this.Info.Error($"{Msg}");
            }

            LogMsg?.Invoke(Msg);
        }

        public void Start(SequentialMove_FlowPara SourcePara)
        {
            this.FlowPara.Clone(SourcePara);

            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            Thread mThread = new Thread(() => Flow(this.FlowPara));
            mThread.Start();
        }

        public void Stop()
        {
            this.FlowRun = false;
            this.Hardware.PLC.Stop();
            this.Hardware.Robot.Stop();
        }

        public void Flow(SequentialMove_FlowPara Para)
        {
            TimeManager TM = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            int MoveIdx = 0;

            List<PositionPara> PosList = new List<PositionPara>();

            if (Para.PosA.Enable) PosList.Add(Para.PosA);
            if (Para.PosB.Enable) PosList.Add(Para.PosB);
            if (Para.PosC.Enable) PosList.Add(Para.PosC);
            if (Para.PosD.Enable) PosList.Add(Para.PosD);

            string Title = "";

            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check Robot
                            bool Check_Robot = (this.Hardware.Robot != null);
                            if (!Check_Robot)
                            {
                                SaveLog($"Check Device Fail :  Robot is null", true);
                            }

                            Check &= Check_Robot;

                            //Check PLC
                            bool Check_PLC = (this.Hardware.PLC != null);
                            if (!Check_PLC)
                            {
                                SaveLog($"Check Device Fail :  PLC is null", true);
                            }

                            Check &= Check_PLC;

                            if (Check)
                            {
                                SaveLog($"Check Device OK");
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

                            bool Check_PosCount = (PosList.Count > 0);
                            if (!Check_PosCount)
                            {
                                SaveLog($"Check Parameter Fail :  Pos Count = 0", true);
                            }

                            Check &= Check_PosCount;

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
                            TM.SetDelay(10);
                            NowStep = FlowStep.RobotInit;
                        }
                        break;

                    case FlowStep.RobotInit:
                        {
                            double[] Pos = new double[6];
                            Pos[0] = PosList[MoveIdx].RobotX_Angle;
                            Pos[1] = PosList[MoveIdx].RobotY_Angle;
                            Pos[2] = PosList[MoveIdx].RobotZ_Angle;
                            Pos[3] = PosList[MoveIdx].RobotRoll_Angle;
                            Pos[4] = PosList[MoveIdx].RobotPitch_Angle;
                            Pos[5] = PosList[MoveIdx].RobotYaw_Angle;

                            int[] Dir = new int[6];
                            Dir[0] = (int)PosList[MoveIdx].RobotX_MoveDir;
                            Dir[1] = (int)PosList[MoveIdx].RobotY_MoveDir;
                            Dir[2] = (int)PosList[MoveIdx].RobotZ_MoveDir;
                            Dir[3] = (int)PosList[MoveIdx].RobotRoll_MoveDir;
                            Dir[4] = (int)PosList[MoveIdx].RobotPitch_MoveDir;
                            Dir[5] = (int)PosList[MoveIdx].RobotYaw_MoveDir;

                            double Acc = GlobalVar.SD.UrRobot_Acc;
                            double Speed = GlobalVar.SD.UrRobot_Speed;
                            double Time = GlobalVar.SD.UrRobot_Time;
                            double Blendradius = GlobalVar.SD.UrRobot_Blendradius;

                            SaveLog($"Robot Init , X = {Pos[0].ToString("0.00")} , Y = {Pos[1].ToString("0.00")} , Z = {Pos[2].ToString("0.00")} , Roll = {Pos[3].ToString("0.00")} , Pitch = {Pos[4].ToString("0.00")} , Yaw = {Pos[5].ToString("0.00")}");

                            bool Rtn = this.Hardware.Robot.MoveJ_CustomDirection(Pos, Dir, Acc, Speed, Blendradius);

                            if (Rtn)
                            {
                                SaveLog($"Robot Init Finish");

                                NowStep = FlowStep.PlcMove;
                            }
                            else
                            {
                                SaveLog($"Robot Init , Retry");
                            }
                        }
                        break;

                    case FlowStep.PlcMove:
                        {
                            if (TM.IsTimeOut())
                            {
                                Title = ((char)('A' + MoveIdx)).ToString();

                                double Pos = PosList[MoveIdx].PlcX;

                                this.Hardware.PLC.AbsMove(Pos);

                                SaveLog($"[{Title}] PLC Move , Pos = {Pos.ToString("0.00")}");

                                NowStep = FlowStep.Check_PLC_InPos;
                            }
                        }
                        break;

                    case FlowStep.Check_PLC_InPos:
                        {
                            if (!this.Hardware.PLC.IsRun)
                            {
                                switch (this.Hardware.PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"[{Title}] PLC Move Finish");
                                            NowStep = FlowStep.RobotMove;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"[{Title}] PLC Move Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.RobotMove:
                        {
                            double[] Pos = new double[6];
                            Pos[0] = PosList[MoveIdx].RobotX_Angle;
                            Pos[1] = PosList[MoveIdx].RobotY_Angle;
                            Pos[2] = PosList[MoveIdx].RobotZ_Angle;
                            Pos[3] = PosList[MoveIdx].RobotRoll_Angle;
                            Pos[4] = PosList[MoveIdx].RobotPitch_Angle;
                            Pos[5] = PosList[MoveIdx].RobotYaw_Angle;

                            int[] Dir = new int[6];
                            Dir[0] = (int)PosList[MoveIdx].RobotX_MoveDir;
                            Dir[1] = (int)PosList[MoveIdx].RobotY_MoveDir;
                            Dir[2] = (int)PosList[MoveIdx].RobotZ_MoveDir;
                            Dir[3] = (int)PosList[MoveIdx].RobotRoll_MoveDir;
                            Dir[4] = (int)PosList[MoveIdx].RobotPitch_MoveDir;
                            Dir[5] = (int)PosList[MoveIdx].RobotYaw_MoveDir;

                            double Acc = GlobalVar.SD.UrRobot_Acc;
                            double Speed = GlobalVar.SD.UrRobot_Speed;
                            double Time = GlobalVar.SD.UrRobot_Time;
                            double Blendradius = GlobalVar.SD.UrRobot_Blendradius;

                            SaveLog($"[{Title}] Robot MoveJ , X = {Pos[0].ToString("0.00")} , Y = {Pos[1].ToString("0.00")} , Z = {Pos[2].ToString("0.00")} , Roll = {Pos[3].ToString("0.00")} , Pitch = {Pos[4].ToString("0.00")} , Yaw = {Pos[5].ToString("0.00")}");

                            bool Rtn = this.Hardware.Robot.MoveJ_CustomDirection(Pos, Dir, Acc, Speed, Blendradius);

                            if (Rtn)
                            {
                                SaveLog($"[{Title}] Robot Move Finish");

                                NowStep = FlowStep.Check_MoveIdx;
                            }
                            else
                            {
                                SaveLog($"[{Title}] Robot Move , Retry");
                            }
                        }
                        break;

                    case FlowStep.Check_MoveIdx:
                        {
                            MoveIdx++;

                            if (MoveIdx < PosList.Count)
                            {
                                TM.SetDelay(Para.MoveDelay);
                                NowStep = FlowStep.PlcMove;
                            }
                            else
                            {
                                NowStep = FlowStep.Finish;
                            }
                        }
                        break;

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

                Thread.Sleep(10);
            }
        }

        public enum FlowStep
        {
            Check_Device = 0,
            Check_Parameter,
            Init,

            RobotInit,
            PlcMove,
            Check_PLC_InPos,
            RobotMove,

            Check_MoveIdx,

            Finish,
            Alarm,
        }
    }

    public class SequentialMove_FlowPara
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("A. Position Setting"), DisplayName("01. 位置A")]
        public PositionPara PosA { get; set; } = new PositionPara();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("A. Position Setting"), DisplayName("02. 位置B")]
        public PositionPara PosB { get; set; } = new PositionPara();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("A. Position Setting"), DisplayName("03. 位置C")]
        public PositionPara PosC { get; set; } = new PositionPara();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("A. Position Setting"), DisplayName("04. 位置D")]
        public PositionPara PosD { get; set; } = new PositionPara();

        [Category("B. Flow Setting"), DisplayName("01. 移動等待 (ms)")]
        public int MoveDelay { get; set; } = 10 * 1000;



        public void Clone(SequentialMove_FlowPara Source)
        {
            this.PosA.Clone(Source.PosA);
            this.PosB.Clone(Source.PosB);
            this.PosC.Clone(Source.PosC);
            this.PosD.Clone(Source.PosD);

            this.MoveDelay = Source.MoveDelay;
        }

        public static void WriteXML(SequentialMove_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(SequentialMove_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static SequentialMove_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                SequentialMove_FlowPara m;

                serializer = new XmlSerializer(typeof(SequentialMove_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (SequentialMove_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }

        public class PositionPara
        {
            [Category("A. Position Setting"), DisplayName("01. Enable")]
            public bool Enable { get; set; } = true;


            [Category("A. Position Setting"), DisplayName("11. Robot X : Pos")]
            public double RobotX_Angle { get; set; } = 0;

            [Category("A. Position Setting"), DisplayName("12. Robot Y : Pos")]
            public double RobotY_Angle { get; set; } = 0;

            [Category("A. Position Setting"), DisplayName("13. Robot Z : Pos")]
            public double RobotZ_Angle { get; set; } = 0;

            [Category("A. Position Setting"), DisplayName("14. Robot Roll : Pos")]
            public double RobotRoll_Angle { get; set; } = 0;

            [Category("A. Position Setting"), DisplayName("15. Robot Pitch : Pos")]
            public double RobotPitch_Angle { get; set; } = 0;

            [Category("A. Position Setting"), DisplayName("16. Robot Yaw : Pos")]
            public double RobotYaw_Angle { get; set; } = 0;


            [Category("A. Position Setting"), DisplayName("21. Robot X : MoveDir")]
            public MoveDir RobotX_MoveDir { get; set; } = MoveDir.Auto;
            [Category("A. Position Setting"), DisplayName("22. Robot Y : MoveDir")]
            public MoveDir RobotY_MoveDir { get; set; } = MoveDir.Auto;

            [Category("A. Position Setting"), DisplayName("23. Robot Z : MoveDir")]
            public MoveDir RobotZ_MoveDir { get; set; } = MoveDir.Auto;

            [Category("A. Position Setting"), DisplayName("24. Robot Roll : MoveDir")]
            public MoveDir RobotRoll_MoveDir { get; set; } = MoveDir.Auto;
            [Category("A. Position Setting"), DisplayName("25. Robot Pitch : MoveDir")]
            public MoveDir RobotPitch_MoveDir { get; set; } = MoveDir.Auto;
            [Category("A. Position Setting"), DisplayName("26. Robot Yaw : MoveDir")]
            public MoveDir RobotYaw_MoveDir { get; set; } = MoveDir.Auto;


            [Category("A. Position Setting"), DisplayName("31. PLC X")]
            public double PlcX { get; set; } = 0;

            public void Clone(PositionPara Source)
            {
                this.Enable = Source.Enable;
                this.RobotX_Angle = Source.RobotX_Angle;
                this.RobotY_Angle = Source.RobotY_Angle;
                this.RobotZ_Angle = Source.RobotZ_Angle;
                this.RobotRoll_Angle = Source.RobotRoll_Angle;
                this.RobotPitch_Angle = Source.RobotPitch_Angle;
                this.RobotYaw_Angle = Source.RobotYaw_Angle;
                this.PlcX = Source.PlcX;
            }

            public override string ToString()
            {
                return $"Enable = {Enable}";
            }
        }

        public enum MoveDir
        {
            Reverse = -1,
            Auto = 0,
            Forward = 1,
        }
    }
}
