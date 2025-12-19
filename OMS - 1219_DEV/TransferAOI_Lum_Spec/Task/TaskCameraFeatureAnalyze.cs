using CommonBase.Logger;
using BaseTool;
using HardwareManager;
using LightMeasure;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameGrabber;

namespace OpticalMeasuringSystem
{
    public class TaskCameraFeatureAnalyze : TaskBase
    {
        private Task_AutoExpTime AutoExpTime;          // 狀態機-自動曝光

        public TaskCameraFeatureAnalyze(MilDigitizer grabber, IMotorControl motorControl, InfoManager info) : base(grabber, motorControl, info)
        {

        }

        public override void MainFlow(object InputPara)
        {
            FlowRun = true;
            FlowStep = (int)FlowState.Init;

            FlowPara Para = (FlowPara)InputPara;

            AnalyzeType Type = Para.Type;
            cls29MSetting RecipeData = Para.RecipeData;
            List<Rectangle> RoiList = Para.RoiList.ToList();
            int xyzIpStep = Para.xyzIpStep;
            string OutputFolder = Para.OutputFolder;

            //Feature 1
            List<int> TargetGray = Para.TargetGray.ToList();
            int Gray_Index = 0; // Auto Exposure Time , Now Count
            Feature_1_Result[,] F1_Result = new Feature_1_Result[,] { };

            while (FlowRun)
            {
                switch (FlowStep)
                {
                    case (int)FlowState.Init:
                        {
                            FlowStep = (int)FlowState.CheckType;
                            Directory.CreateDirectory(OutputFolder);
                        }
                        break;

                    case (int)FlowState.CheckType:
                        {
                            switch (Type)
                            {
                                case AnalyzeType.Feature_1:
                                    {
                                        int RoiCnt = RoiList.Count;
                                        int GrayCnt = TargetGray.Count;
                                        F1_Result = new Feature_1_Result[RoiCnt, GrayCnt];

                                        for (int r = 0; r < RoiCnt; r++)
                                        {
                                            for (int g = 0; g < GrayCnt; g++)
                                            {
                                                F1_Result[r, g] = new Feature_1_Result();
                                                F1_Result[r, g].ROI = RoiList[r];
                                                F1_Result[r, g].TargetMean = TargetGray[g];
                                            }
                                        }

                                        FlowStep = (int)FlowState.Feature1_AutoExpTime_CheckCnt;
                                        Info.General("[Camera Feature Anaylze] Flow Start - Feature 1");
                                    }
                                    break;

                                case AnalyzeType.Feature_2:
                                    {
                                        Info.General($"[Camera Feature Anaylze] Flow Start - Feature 1");
                                    }
                                    break;
                            }
                        }
                        break;

                    #region Feature - 1

                    case (int)FlowState.Feature1_AutoExpTime_CheckCnt:
                        {
                            if (Gray_Index < TargetGray.Count)
                            {
                                FlowStep = (int)FlowState.Feature1_AutoExpTime_FlowStart;
                                Info.General($"[Camera Feature Anaylze] Auto Exp Time , Check Count - {Gray_Index + 1}/{TargetGray.Count}");
                            }
                            else
                            {
                                FlowStep = (int)FlowState.Feature1_CalDiff;
                                Info.General($"[Camera Feature Anaylze] Auto Exp Time , Check Count , Finish");
                            }
                        }
                        break;

                    case (int)FlowState.Feature1_AutoExpTime_FlowStart:
                        {
                            int TargetMean = TargetGray[Gray_Index];
                            cls29MSetting RD = Para.RecipeData;
                            RD.AutoExposureTargetGray = new int[] { TargetMean, TargetMean, TargetMean };

                            AutoExpTime = new Task_AutoExpTime(RD, xyzIpStep, this.Info, this.Grabber);
                            AutoExpTime.Start(Gray_Index);

                            Info.General($"[Camera Feature Anaylze] Auto Exp Time Start , Target Gray = { TargetMean }");

                            FlowStep = (int)FlowState.Feature1_AutoExpTime_CheckResult;
                        }
                        break;

                    case (int)FlowState.Feature1_AutoExpTime_CheckResult:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                switch (AutoExpTime.State)
                                {
                                    case Task_AutoExpTime.EnState.Finish:
                                        {
                                            Info.General($"[Camera Feature Anaylze] Auto Exp Time Finish");

                                            for (int r = 0; r < RoiList.Count; r++)
                                            {
                                                F1_Result[r, Gray_Index].GrayMean = AutoExpTime.Result.GrayMean;
                                                F1_Result[r, Gray_Index].ExpTime = AutoExpTime.Result.ExpTime;
                                            }

                                            FlowStep = (int)FlowState.Feature1_GetRoiMean;
                                        }
                                        break;

                                    case Task_AutoExpTime.EnState.Warning:
                                    case Task_AutoExpTime.EnState.Alarm:
                                        {
                                            Info.Error($"[Camera Feature Anaylze] Auto Exp Time Alarm");

                                            FlowStep = (int)FlowState.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case (int)FlowState.Feature1_GetRoiMean:
                        {
                            MIL_ID Img = this.Grabber.grabImage;

                            for (int r = 0; r < RoiList.Count; r++)
                            {
                                //方形
                                Rectangle ROI = RoiList[r];

                                RectRegionInfo Roi_Rec = new RectRegionInfo
                                {
                                    StartX = ROI.X,
                                    StartY = ROI.Y,
                                    Width = ROI.Width,
                                    Height = ROI.Height
                                };

                                double mean = MyMilIp.StatRectMean(Img, Roi_Rec);

                                F1_Result[r, Gray_Index].RoiMean = mean;
                            }

                            Gray_Index++;
                            FlowStep = (int)FlowState.Feature1_AutoExpTime_CheckCnt;
                        }
                        break;

                    case (int)FlowState.Feature1_CalDiff:
                        {
                            for (int r = 0; r < RoiList.Count; r++)
                            {
                                for (int g = 0; g < TargetGray.Count; g++)
                                {
                                    if (g == 0)
                                    {
                                        F1_Result[r, g].DiffGray = 0;
                                        F1_Result[r, g].DiffRatio = 0;
                                    }
                                    else
                                    {
                                        double GrayMean_1 = F1_Result[r, g - 1].GrayMean;
                                        double GrayMean_2 = F1_Result[r, g].GrayMean;
                                        double ExpTime_1 = F1_Result[r, g - 1].ExpTime;
                                        double ExpTime_2 = F1_Result[r, g].ExpTime;

                                        double DiffGray = GrayMean_2 - GrayMean_1;
                                        double DiffRatio = (((GrayMean_2 / GrayMean_1) / (ExpTime_2 / ExpTime_1)) - 1);

                                        if (double.IsNaN(DiffRatio))
                                        {
                                           DiffRatio = 0;
                                        }

                                        F1_Result[r, g].DiffGray = DiffGray;
                                        F1_Result[r, g].DiffRatio = DiffRatio;
                                    }
                                }
                            }

                            FlowStep = (int)FlowState.Feature1_SaveResult;
                        }
                        break;

                    case (int)FlowState.Feature1_SaveResult:
                        {
                            string SavePath = $@"{OutputFolder}\{DateTime.Now.ToString("yyyyMMddHHmmss")}_Feature1.csv";
                            bool Rtn = Feature1_WriteCsv(RecipeData, F1_Result, SavePath);

                            if (Rtn)
                            {
                                FlowStep = (int)FlowState.Finish;
                            }
                            else
                            {
                                FlowStep = (int)FlowState.Alarm;
                            }
                        }
                        break;

                    #endregion

                    case (int)FlowState.Finish:
                        {
                            FlowRun = false;
                        }
                        break;

                    case (int)FlowState.Alarm:
                        {
                            FlowRun = false;
                        }
                        break;
                }
            }
        }

        private bool Feature1_WriteCsv(cls29MSetting RD, Feature_1_Result[,] FeatureResult, string SavePath)
        {
            try
            {
                StringBuilder SB = new StringBuilder();
                int RoiCnt = FeatureResult.GetLength(0);
                int GrayCnt = FeatureResult.GetLength(1);
                string Percent = $"+-{RD.AutoExposureGrayTolerance * 100}%";

                for (int r = 0; r < RoiCnt; r++)
                {
                    //Remark (ROI)
                    string[] Remarks = new string[]
                    {
                        $"ROI = {r}",
                        $"Start X = {FeatureResult[r,0].ROI.X}",
                        $"Start Y = {FeatureResult[r,0].ROI.Y}",
                        $"Width = {FeatureResult[r,0].ROI.Width}",
                        $"Height = {FeatureResult[r,0].ROI.Height}",
                    };

                    string Remark = string.Join(",", Remarks);
                    SB.Append(Remark + "\r\n");

                    //Title
                    string[] Titles = new string[]
                    {
                        "Target Mean",
                        "Flow Mean",
                        "ROI Mean",
                        "Exp Time",
                        "Gray Diff",
                        "Diff Ratio (%)"
                    };

                    string Title = string.Join(",", Titles);
                    SB.Append(Title + "\r\n");

                    //Result
                    for (int g = 0; g < GrayCnt; g++)
                    {
                        string[] Results = new string[]
                        {
                            $"{ FeatureResult[r,g].TargetMean } {Percent}",
                            $"{ FeatureResult[r,g].GrayMean }",
                            $"{ FeatureResult[r,g].RoiMean }",
                            $"{ FeatureResult[r,g].ExpTime }",
                            $"{ FeatureResult[r,g].DiffGray }",
                            $"{ FeatureResult[r,g].DiffRatio *100}%",
                        };

                        string Result = string.Join(",", Results);
                        SB.Append(Result + "\r\n");
                    }
                    //Space
                    SB.Append("\r\n");
                }

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();


                return true;
            }
            catch (Exception ex)
            {
                Info.Error(ex.ToString());
                return false;
            }
        }

        public enum FlowState
        {
            Init = 0,
            CheckType = 10,

            //Feature 1 
            Feature1_AutoExpTime_CheckCnt = 100,
            Feature1_AutoExpTime_FlowStart,
            Feature1_AutoExpTime_CheckResult,
            Feature1_GetRoiMean,
            Feature1_CalDiff,
            Feature1_SaveResult,

            //Feature 2


            Finish,
            Alarm,
        }

        public class FlowPara
        {
            public AnalyzeType Type = AnalyzeType.Feature_1;
            public cls29MSetting RecipeData = new cls29MSetting();
            public List<Rectangle> RoiList = new List<Rectangle>();
            public string OutputFolder = string.Empty;
            public int xyzIpStep = 0;

            //Feature 1
            public List<int> TargetGray = new List<int>();

            //Feature 2
        }

        public class Feature_1_Result
        {
            public Rectangle ROI = new Rectangle();
            public int TargetMean = 0;
            public double GrayMean = 0.0;
            public double RoiMean = 0.0;
            public double ExpTime = 0.0;
            public double DiffGray = 0.0;
            public double DiffRatio = 0.0;
        }

        public enum AnalyzeType
        {
            Feature_1,
            Feature_2
        }
    }
}
