using BaseTool;
using CommonBase.Logger;
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
    public class Task_TiltCorrection
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;

        //Task
        private Task_BaseAlign BaseAlign = null;
        private Task_VCurve VCurve = null;


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

        public Task_TiltCorrection(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            string LogTitle = "Tilt Correction";

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

        public void Start(TiltCorrection_FlowPara SourcePara)
        {
            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            TiltCorrection_FlowPara Para = new TiltCorrection_FlowPara();
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

        public void Flow(TiltCorrection_FlowPara Para)
        {
            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            int CycleIdx = 0;

            RecordPara RecordInfo = new RecordPara();

            Para.OutputFolder = $@"{Para.OutputFolder}\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";


            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check Grabber
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Hardware.Grabber != null);
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
                            TiltCorrection_FlowPara.WriteXML(Para, $@"{Para.OutputFolder}\Config.xml");

                            NowStep = FlowStep.BaseAlign;
                        }
                        break;

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
                                            SaveLog($"Check Base Align Finish");
                                            NowStep = FlowStep.VCurve;
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

                    case FlowStep.VCurve:
                        {
                            VCurve = new Task_VCurve(this.Hardware.Grabber, this.Hardware.Motor, this.Info);

                            Para.VCurve_Para.OutputFolder = $@"{Para.OutputFolder}\VCurve\{CycleIdx}\";
                            VCurve.Start(Para.VCurve_Para);

                            Thread.Sleep(1000);
                            SaveLog($"VCurve Start");
                            NowStep = FlowStep.Check_VCurve;
                        }
                        break;

                    case FlowStep.Check_VCurve:
                        {
                            if (!VCurve.FlowRun)
                            {
                                switch (VCurve.Step)
                                {
                                    case Task_VCurve.FlowStep.Finish:
                                        {
                                            SaveLog($"VCurve Finish");
                                            NowStep = FlowStep.CheckNextAction;
                                        }
                                        break;

                                    case Task_VCurve.FlowStep.Error:
                                        {
                                            SaveLog($"AutoFocus Error", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case Task_VCurve.FlowStep.TimeOut:
                                        {
                                            SaveLog($"AutoFocus Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.CheckNextAction:
                        {
                            bool CheckOK = true;

                            if (CheckOK)
                            {
                                NowStep = FlowStep.Finish;
                            }
                            else
                            {
                                BaseAlign = new Task_BaseAlign(this.Info, this.Hardware);
                                BaseAlign.Start(Para.BaseAlign_Para, Task_BaseAlign.FlowStep.AutoFocus);

                                Thread.Sleep(1000);
                                SaveLog($"Base Align Start");
                                NowStep = FlowStep.CheckBaseAlign;

                                CycleIdx++;
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

            //Base Align
            BaseAlign,
            CheckBaseAlign,

            //V Curve
            VCurve,
            Check_VCurve,

            CheckNextAction,

            Finish,
            Alarm,
        }


        private bool WriteRowData(string SavePath, RecordPara Para)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                if (!File.Exists(SavePath))
                {
                    SB.Append($"### ExposureTime = {Para.ExposureTime}" + "\r\n");
                    SB.Append($"### LowerThreashold = {Para.LowerThreashold}" + "\r\n");
                    SB.Append($"### UpperThreashold = {Para.UpperThreashold}" + "\r\n");

                    List<string> TitleList = new List<string>();
                    TitleList.Add("Cycle Index");
                    TitleList.Add("X");
                    TitleList.Add("Y");
                    TitleList.Add("Gray");
                    string Title = string.Join(",", TitleList);
                    SB.Append(Title + "\r\n");
                }

                //Info

                foreach (NoiseInfo Noise in Para.NoiseList)
                {
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{Para.CycleIdx}");
                    InfoList.Add($"{Noise.X}");
                    InfoList.Add($"{Noise.Y}");
                    InfoList.Add($"{Noise.Gray}");

                    string Info = string.Join(",", InfoList);
                    SB.Append(Info + "\r\n");
                }

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

        public class RecordPara
        {
            public int CycleIdx { get; set; } = 0;
            public Task_NoiseDetection.NoiseType Type { get; set; } = Task_NoiseDetection.NoiseType.Dark;
            public double ExposureTime { get; set; } = 0.0;
            public double UpperThreashold { get; set; } = 0.0;
            public double LowerThreashold { get; set; } = 0.0;

            public List<NoiseInfo> NoiseList { get; set; } = new List<NoiseInfo>();
        }

        public class NoiseInfo
        {
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;
            public double Gray { get; set; } = 0.0;
        }
    }

    public class TiltCorrection_FlowPara
    {
        [Category("A. Base Align Flow"), DisplayName("01. Base Align FlowPara"), Description("Base Align FlowPara"), Browsable(false)]
        public BaseAlign_FlowPara BaseAlign_Para { get; set; } = new BaseAlign_FlowPara();

        [Category("A. Flow Para"), DisplayName("01. Output Folder"), Description("Output Folder"), ReadOnly(true), XmlIgnore]
        public string OutputFolder { get; set; } = @"D:\OMS\Tilt Correction\";

        [Category("A. Flow Para"), DisplayName("02. VCurve Para"), Description("VCurve Para")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public VCurve_FlowPara VCurve_Para { get; set; } = new VCurve_FlowPara();

        public void Clone(TiltCorrection_FlowPara Source)
        {
            this.BaseAlign_Para.Clone(Source.BaseAlign_Para);

            //A
            this.OutputFolder = Source.OutputFolder;

            //B
            this.VCurve_Para.Clone(Source.VCurve_Para);
        }

        public static void WriteXML(TiltCorrection_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(TiltCorrection_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static TiltCorrection_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                TiltCorrection_FlowPara m;

                serializer = new XmlSerializer(typeof(TiltCorrection_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (TiltCorrection_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }
    }
}
