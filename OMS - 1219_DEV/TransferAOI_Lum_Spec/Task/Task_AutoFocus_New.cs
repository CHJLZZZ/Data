using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using HardwareManager;
using BaseTool;

using Matrox.MatroxImagingLibrary;
using ClosedXML.Excel;
using FrameGrabber;
using System.ComponentModel;

namespace OpticalMeasuringSystem
{
    public class Task_AutoFocus_New
    {
        public event Action<int, double, bool> UpdateFocusValue; //Pos, Value, Finish

        private EnumMotoControlPackage MotorType = GlobalVar.DeviceType;

        private InfoManager info;
        private MilDigitizer grabber;
        private IMotorControl motorControl;
        public bool FlowRun = false;

        private FlowStep NowStep = FlowStep.GetOriFocusValue;

        public FlowStep Step
        {
            get => NowStep;
        }
        
        private FlowResult Flow_Result = new FlowResult();
        public FlowResult Result
        {
            get => Flow_Result;
        }

        public Task_AutoFocus_New(MilDigitizer grabber, IMotorControl motorControl, InfoManager info)
        {
            this.info = info;
            this.grabber = grabber;
            this.motorControl = motorControl;
        }

        public void Start(AutoFocus_FlowPara_New Para)
        {
            if (!FlowRun)
            {
                FlowRun = true;
                Thread mThread = new Thread(() => Flow(Para));
                mThread.Start();
            }
            else
            {
                FlowRun = false;
                Thread.Sleep(1000);
                Start(Para);
            }
        }

        public void Stop()
        {
            FlowRun = false;
        }

        private void Flow(AutoFocus_FlowPara_New Para)
        {
            ThreePointsData CurrentPoints = new ThreePointsData();
            int NewPeakPos = 0;
            double NewPeakSharpness = 0;
            int CurrentPointIndex = 0;  // 0: 未初始化, 1: X1, 2: X2, 3: X3
            int IterationCount = 0;
            int MaxIterations = 15;
            int MinPointSpacing = 5;  // 三點最小間距

            int Min_Step = Para.Min_Step;
            bool UseROI = Para.UseROI;
            Rectangle ROI = Para.ROI;

            TimeManager TM = new TimeManager();
            TimeManager TM_TimeOut = new TimeManager();
            TM_TimeOut.SetDelay(MaxIterations * 30000);

            // Initial
            this.NowStep = FlowStep.GetOriFocusValue;
            this.FlowRun = true;

            try
            {
                while (this.FlowRun)
                {
                    if (TM_TimeOut.IsTimeOut())
                    {
                        this.NowStep = FlowStep.TimeOut;
                    }

                    switch (this.NowStep)
                    {
                        case FlowStep.GetOriFocusValue:
                            {
                                int Upper_Limit = Para.Upper_Limit;
                                int Lower_Limit = Para.Lower_Limit;

                                // 取得搜尋範圍
                                if (Upper_Limit == -1)
                                {
                                    Upper_Limit = this.motorControl.Focus_LimitF();
                                    Lower_Limit = this.motorControl.Focus_LimitR();
                                }

                                int Pos_Start = Math.Min(Upper_Limit, Lower_Limit);
                                int Pos_End = Math.Max(Upper_Limit, Lower_Limit);

                                // 初始化三個點位置（確保合理間距）
                                int totalRange = Math.Abs(Pos_End - Pos_Start);
                                int minGap = Math.Max(totalRange / 10, MinPointSpacing);

                                CurrentPoints.X1 = Pos_Start;
                                CurrentPoints.X3 = Pos_End;
                                CurrentPoints.X2 = (Pos_Start + Pos_End) / 2;

                                // 確保間距足夠
                                if (CurrentPoints.X2 - CurrentPoints.X1 < minGap)
                                {
                                    CurrentPoints.X2 = CurrentPoints.X1 + minGap;
                                }

                                if (CurrentPoints.X3 - CurrentPoints.X2 < minGap)
                                {
                                    CurrentPoints.X3 = Math.Min(Pos_End, CurrentPoints.X2 + minGap);
                                }

                                CurrentPointIndex = 1;
                                this.NowStep = FlowStep.InitThreePoints;
                                TM.SetDelay(10);
                            }
                            break;

                        case FlowStep.InitThreePoints:
                            {
                                int TargetPos = 0;

                                if (TM.IsTimeOut())
                                {
                                    // 決定要移動到哪個點
                                    switch (CurrentPointIndex)
                                    {
                                        case 1: TargetPos = CurrentPoints.X1; break;
                                        case 2: TargetPos = CurrentPoints.X2; break;
                                        case 3: TargetPos = CurrentPoints.X3; break;
                                    }

                                    // 移動馬達
                                    switch (MotorType)
                                    {
                                        case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                        case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                        case EnumMotoControlPackage.MS5515M:
                                        case EnumMotoControlPackage.CircleTac:
                                            this.motorControl.Move(TargetPos, -1, -1, -1);
                                            break;
                                    }

                                    this.NowStep = FlowStep.WaitMoveToPoint;
                                    TM.SetDelay(100);
                                }
                            }
                            break;

                        case FlowStep.WaitMoveToPoint:
                            {
                                if (TM.IsTimeOut())
                                {
                                    if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                    {
                                        Thread.Sleep(100);

                                        // 拍照並計算清晰度
                                        this.grabber.Grab();

                                        int TargetPos = 0;
                                        int FocusValue = this.Calculate_FocusValue(this.grabber.grabImage, Para.UseROI, Para.ROI);

                                        // 儲存到對應的點
                                        switch (CurrentPointIndex)
                                        {
                                            case 1:
                                                {
                                                    TargetPos = CurrentPoints.X1;
                                                    CurrentPoints.Y1 = FocusValue;
                                                    this.info.General($"點1: 位置={CurrentPoints.X1}, 清晰度={FocusValue:F2}");
                                                }
                                                break;

                                            case 2:
                                                {
                                                    TargetPos = CurrentPoints.X2;
                                                    CurrentPoints.Y2 = FocusValue;
                                                    this.info.General($"點2: 位置={CurrentPoints.X2}, 清晰度={FocusValue:F2}");
                                                }
                                                break;

                                            case 3:
                                                {
                                                    TargetPos = CurrentPoints.X3;
                                                    CurrentPoints.Y3 = FocusValue;
                                                    this.info.General($"點3: 位置={CurrentPoints.X3}, 清晰度={FocusValue:F2}");
                                                }
                                                break;
                                        }

                                        UpdateFocusValue?.Invoke(TargetPos, FocusValue, false);

                                        // 檢查是否三個初始點都評估完成
                                        if (CurrentPointIndex < 3)
                                        {
                                            CurrentPointIndex++;
                                            this.NowStep = FlowStep.InitThreePoints;
                                        }
                                        else
                                        {
                                            // 三點都評估完成，進入迭代
                                            IterationCount = 0;
                                            this.NowStep = FlowStep.CalculateParabolicPeak;
                                        }

                                        TM.SetDelay(10);
                                    }
                                    else
                                    {
                                        TM.SetDelay(100);
                                    }
                                }
                            }
                            break;

                        case FlowStep.CalculateParabolicPeak:
                            {
                                if (TM.IsTimeOut())
                                {
                                    IterationCount++;
                                    int searchRange = Math.Abs(CurrentPoints.X3 - CurrentPoints.X1);

                                    this.info.General($"--- 迭代 {IterationCount} ---");
                                    this.info.General($"搜尋範圍: [{CurrentPoints.X1}, {CurrentPoints.X3}] = {searchRange}");

                                    // ============ 保護機制 1: 檢查收斂條件 ============
                                    if (searchRange <= Min_Step * 2)
                                    {
                                        this.info.General($"搜尋範圍已小於閾值 ({Min_Step * 2})，準備收斂");
                                        this.NowStep = FlowStep.CheckConvergence;
                                        TM.SetDelay(10);
                                        break;
                                    }

                                    // ============ 保護機制 2: 檢查最大迭代次數 ============
                                    if (IterationCount > MaxIterations)
                                    {
                                        this.info.General($"達到最大迭代次數 {MaxIterations}");
                                        this.NowStep = FlowStep.CheckConvergence;
                                        TM.SetDelay(10);
                                        break;
                                    }

                                    // ============ 保護機制 3: 檢查三點間距 ============
                                    int spacing1 = Math.Abs(CurrentPoints.X2 - CurrentPoints.X1);
                                    int spacing2 = Math.Abs(CurrentPoints.X3 - CurrentPoints.X2);

                                    if (spacing1 < MinPointSpacing || spacing2 < MinPointSpacing)
                                    {
                                        this.info.General($"警告: 三點間距過小 ({spacing1}, {spacing2})，使用保守策略");

                                        // 直接返回當前最大清晰度的位置
                                        if (CurrentPoints.Y1 >= CurrentPoints.Y2 && CurrentPoints.Y1 >= CurrentPoints.Y3)
                                            NewPeakPos = CurrentPoints.X1;
                                        else if (CurrentPoints.Y2 >= CurrentPoints.Y1 && CurrentPoints.Y2 >= CurrentPoints.Y3)
                                            NewPeakPos = CurrentPoints.X2;
                                        else
                                            NewPeakPos = CurrentPoints.X3;

                                        this.info.General($"選擇當前最佳位置: {NewPeakPos}");
                                        this.NowStep = FlowStep.CheckConvergence;
                                        TM.SetDelay(10);
                                        break;
                                    }

                                    // ============ 保護機制 4: 檢查單峰特性 ============
                                    // 確保中點是三點中最高的（單峰函數特性）
                                    bool isMiddleHighest = (CurrentPoints.Y2 >= CurrentPoints.Y1 &&
                                                           CurrentPoints.Y2 >= CurrentPoints.Y3);

                                    if (!isMiddleHighest)
                                    {
                                        this.info.General("警告: 中點清晰度不是最高，峰值可能在區間外");

                                        // 判斷峰值在哪一側
                                        if (CurrentPoints.Y1 > CurrentPoints.Y3)
                                        {
                                            this.info.General("峰值可能在左側，調整搜尋範圍");
                                            // 向左擴展/縮小
                                            CurrentPoints.X3 = CurrentPoints.X2;
                                            CurrentPoints.Y3 = CurrentPoints.Y2;
                                            CurrentPoints.X2 = (CurrentPoints.X1 + CurrentPoints.X2) / 2;

                                            // 重新評估新的中點
                                            CurrentPointIndex = 2;
                                            this.NowStep = FlowStep.InitThreePoints;
                                        }
                                        else
                                        {
                                            this.info.General("峰值可能在右側，調整搜尋範圍");
                                            // 向右擴展/縮小
                                            CurrentPoints.X1 = CurrentPoints.X2;
                                            CurrentPoints.Y1 = CurrentPoints.Y2;
                                            CurrentPoints.X2 = (CurrentPoints.X2 + CurrentPoints.X3) / 2;

                                            // 重新評估新的中點
                                            CurrentPointIndex = 2;
                                            this.NowStep = FlowStep.InitThreePoints;
                                        }

                                        TM.SetDelay(10);
                                        break;
                                    }

                                    // ============ 計算拋物線峰值 ============
                                    NewPeakPos = CalculateParabolicPeak(CurrentPoints);

                                    // ============ 保護機制 5: 峰值位置合理性檢查 ============
                                    // 峰值不應該距離當前範圍太遠
                                    int maxDeviation = (int)(searchRange * 0.3);  // 最多偏離30%

                                    if (NewPeakPos < CurrentPoints.X1 - maxDeviation)
                                    {
                                        this.info.General($"警告: 計算峰值 {NewPeakPos} 超出左邊界過多，裁剪至 {CurrentPoints.X1}");
                                        NewPeakPos = CurrentPoints.X1;
                                    }
                                    else if (NewPeakPos > CurrentPoints.X3 + maxDeviation)
                                    {
                                        this.info.General($"警告: 計算峰值 {NewPeakPos} 超出右邊界過多，裁剪至 {CurrentPoints.X3}");
                                        NewPeakPos = CurrentPoints.X3;
                                    }
                                    else
                                    {
                                        // 標準裁剪到範圍內
                                        NewPeakPos = Math.Max(CurrentPoints.X1, Math.Min(CurrentPoints.X3, NewPeakPos));
                                    }

                                    this.info.General($"拋物線峰值預測: {NewPeakPos}");

                                    // 移動到峰值位置
                                    switch (MotorType)
                                    {
                                        case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                        case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                        case EnumMotoControlPackage.MS5515M:
                                        case EnumMotoControlPackage.CircleTac:
                                            this.motorControl.Move(NewPeakPos, -1, -1, -1);
                                            break;
                                    }

                                    this.NowStep = FlowStep.EvaluateNewPoint;
                                    TM.SetDelay(100);
                                }
                            }
                            break;

                        case FlowStep.EvaluateNewPoint:
                            {
                                if (TM.IsTimeOut())
                                {
                                    if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                    {
                                        System.Threading.Thread.Sleep(100);

                                        // 拍照並計算清晰度
                                        this.grabber.Grab();
                                        NewPeakSharpness = this.Calculate_FocusValue(this.grabber.grabImage, Para.UseROI, Para.ROI);

                                        this.info.General($"新點清晰度: 位置={NewPeakPos}, 清晰度={NewPeakSharpness:F2}");

                                        UpdateFocusValue?.Invoke(NewPeakPos, NewPeakSharpness, false);

                                        this.NowStep = FlowStep.UpdateSearchRange;
                                        TM.SetDelay(10);
                                    }
                                    else
                                    {
                                        TM.SetDelay(100);
                                    }
                                }
                            }
                            break;

                        case FlowStep.UpdateSearchRange:
                            {
                                if (TM.IsTimeOut())
                                {
                                    // 更新三點：保留最優的三個點
                                    UpdateThreePoints(CurrentPoints, NewPeakPos, NewPeakSharpness);

                                    this.info.General($"更新後三點: X1={CurrentPoints.X1}({CurrentPoints.Y1:F2}), " +
                                                    $"X2={CurrentPoints.X2}({CurrentPoints.Y2:F2}), " +
                                                    $"X3={CurrentPoints.X3}({CurrentPoints.Y3:F2})");

                                    // 繼續下一次迭代
                                    this.NowStep = FlowStep.CalculateParabolicPeak;
                                    TM.SetDelay(10);
                                }
                            }
                            break;

                        case FlowStep.CheckConvergence:
                            {
                                if (TM.IsTimeOut())
                                {
                                    // 找出最佳點
                                    int bestPos;
                                    double bestSharpness;

                                    if (CurrentPoints.Y1 >= CurrentPoints.Y2 && CurrentPoints.Y1 >= CurrentPoints.Y3)
                                    {
                                        bestPos = CurrentPoints.X1;
                                        bestSharpness = CurrentPoints.Y1;
                                    }
                                    else if (CurrentPoints.Y2 >= CurrentPoints.Y1 && CurrentPoints.Y2 >= CurrentPoints.Y3)
                                    {
                                        bestPos = CurrentPoints.X2;
                                        bestSharpness = CurrentPoints.Y2;
                                    }
                                    else
                                    {
                                        bestPos = CurrentPoints.X3;
                                        bestSharpness = CurrentPoints.Y3;
                                    }

                                    // 檢查清晰度是否有效
                                    if (bestSharpness == 0)
                                    {
                                        this.NowStep = FlowStep.Error;
                                        break;
                                    }



                                    Flow_Result = new FlowResult {
                                        FocusPos = bestPos,
                                        FocusValue = bestSharpness,
                                    };

                                    this.info.General($"對焦完成！迭代次數: {IterationCount}");
                                    this.info.General($"最佳位置: {bestPos}, 清晰度: {bestSharpness:F2}");

                                    UpdateFocusValue?.Invoke(bestPos, bestSharpness, true);

                                    // 移動到最佳位置
                                    this.NowStep = FlowStep.MoveToFinalPosition;

                                    switch (MotorType)
                                    {
                                        case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                        case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                        case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                        case EnumMotoControlPackage.MS5515M:
                                        case EnumMotoControlPackage.CircleTac:
                                            this.motorControl.Move(bestPos, -1, -1, -1);
                                            break;
                                    }

                                    TM.SetDelay(100);
                                }
                            }
                            break;

                        case FlowStep.MoveToFinalPosition:
                            {
                                if (TM.IsTimeOut())
                                {
                                    if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                    {
                                        this.grabber.Grab();
                                        this.NowStep = FlowStep.Finish;
                                    }
                                    else
                                    {
                                        TM.SetDelay(100);
                                    }
                                }
                            }
                            break;

                        case FlowStep.Finish:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Focus Finish !");
                            }
                            break;

                        case FlowStep.Error:
                            {
                                this.FlowRun = false;
                                this.info.General("Caption Error !");
                            }
                            break;

                        case FlowStep.TimeOut:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Focus TimeOut !");
                            }
                            break;
                    }
                }
            }
            catch
            {
                this.FlowRun = false;
                this.NowStep = FlowStep.Error;
            }
            finally
            {
                CurrentPoints = null;
                TM = null;
                TM_TimeOut = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }


        #region Method

        public int GetFocusValue()
        {
            this.grabber.Grab();
            return this.Calculate_FocusValue(this.grabber.grabImage);
        }

        public int Calculate_FocusValue(MIL_ID image, bool UseROI = false, Rectangle ROI = new Rectangle())
        {
            MIL_INT FocusScore = 0;

            if (UseROI)
            {
                double SizeX = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
                double SizeY = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

                MIL_ID ROI_Image = MIL.M_NULL;

                MIL.MbufChild2d(this.grabber.grabImage,
                           ROI.X, ROI.Y,
                           ROI.Width, ROI.Height,
                           ref ROI_Image);

                MIL.MdigFocus(MIL.M_NULL,
                            ROI_Image,
                            MIL.M_DEFAULT,
                            null,
                            MIL.M_NULL,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_EVALUATE,
                            ref FocusScore);

                MIL.MbufFree(ROI_Image);
            }
            else
            {
                MIL.MdigFocus(MIL.M_NULL,
                            image,
                            MIL.M_DEFAULT,
                            null,
                            MIL.M_NULL,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_EVALUATE,
                            ref FocusScore);
            }

            return ((int)FocusScore);
        }

        /// <summary>
        /// 計算拋物線峰值位置
        /// </summary>
        private int CalculateParabolicPeak(ThreePointsData p)
        {
            double x1 = p.X1;
            double x2 = p.X2;
            double x3 = p.X3;
            double y1 = p.Y1;
            double y2 = p.Y2;
            double y3 = p.Y3;

            double denom = (x1 - x2) * (x1 - x3) * (x2 - x3);

            if (Math.Abs(denom) < 1e-10)
            {
                // 三點共線，返回中點
                return p.X2;
            }

            double a = (x3 * (y2 - y1) + x2 * (y1 - y3) + x1 * (y3 - y2)) / denom;
            double b = (x3 * x3 * (y1 - y2) + x2 * x2 * (y3 - y1) + x1 * x1 * (y2 - y3)) / denom;

            if (Math.Abs(a) < 1e-10)
            {
                // 拋物線退化，返回最大值的位置
                if (y1 >= y2 && y1 >= y3) return p.X1;
                if (y2 >= y1 && y2 >= y3) return p.X2;
                return p.X3;
            }

            double peakX = -b / (2 * a);

            // 如果是開口向下的拋物線，才是有效峰值
            if (a < 0)
                return (int)Math.Round(peakX);
            else
                // 開口向上，返回三點中最大的
                return y2 >= y1 && y2 >= y3 ? p.X2 : (y1 > y3 ? p.X1 : p.X3);
        }

        /// <summary>
        /// 更新三個點：保留最優的三個點組合
        /// </summary>
        private void UpdateThreePoints(ThreePointsData p, int newX, double newY)
        {
            // 根據新點位置，更新三點
            if (newX < p.X2)
            {
                // 新點在左半邊
                if (newY > p.Y2)
                {
                    // 新點更好，縮小到左半區間
                    p.X3 = p.X2;
                    p.Y3 = p.Y2;
                    p.X2 = newX;
                    p.Y2 = newY;
                }
                else
                {
                    // 中點更好，縮小左邊界
                    p.X1 = newX;
                    p.Y1 = newY;
                }
            }
            else if (newX > p.X2)
            {
                // 新點在右半邊
                if (newY > p.Y2)
                {
                    // 新點更好，縮小到右半區間
                    p.X1 = p.X2;
                    p.Y1 = p.Y2;
                    p.X2 = newX;
                    p.Y2 = newY;
                }
                else
                {
                    // 中點更好，縮小右邊界
                    p.X3 = newX;
                    p.Y3 = newY;
                }
            }
            else
            {
                // 新點就是中點（罕見情況）
                p.X2 = newX;
                p.Y2 = newY;
            }
        }

        #endregion

        public class FlowResult
        {
            public int FocusPos { get; set; } = 0;
            public double FocusValue { get; set; } = 0;
        }

        // 三點資料結構
        private class ThreePointsData
        {
            public int X1 { get; set; } = 0; // 位置1
            public int X2 { get; set; } = 0; // 位置2 (中點)
            public int X3 { get; set; } = 0; // 位置3
            public double Y1 { get; set; } = 0.0; // 位置1的清晰度
            public double Y2 { get; set; } = 0.0; // 位置2的清晰度          
            public double Y3 { get; set; } = 0.0; // 位置3的清晰度            
        }

        // 狀態機定義
        public enum FlowStep
        {
            GetOriFocusValue,
            InitThreePoints,
            WaitMoveToPoint,
            CalculateParabolicPeak,
            EvaluateNewPoint,
            UpdateSearchRange,
            CheckConvergence,
            MoveToFinalPosition,
            Finish,
            Error,
            TimeOut
        }
    }

    public class AutoFocus_FlowPara_New
    {
        [Category("AutoFocus Para"), DisplayName("01. Upper Limit"), Description("Upper Limit")]
        public int Upper_Limit { get; set; } = 0;

        [Category("AutoFocus Para"), DisplayName("02. Lower Limit"), Description("Lower Limit")]
        public int Lower_Limit { get; set; } = 0;

        [Category("AutoFocus Para"), DisplayName("03. Min Step"), Description("Min Step")]
        public int Min_Step { get; set; } = 0;        

        [Category("AutoFocus Para"), DisplayName("04. Use ROI"), Description("Use ROI")]
        public bool UseROI { get; set; } = false;

        [Category("AutoFocus Para"), DisplayName("05. ROI"), Description("ROI")]
        public Rectangle ROI { get; set; } = new Rectangle();

        public void Clone(AutoFocus_FlowPara Source)
        {
            this.Upper_Limit = Source.Upper_Limit;
            this.Lower_Limit = Source.Lower_Limit;
            this.Min_Step = Source.Min_Step;
            this.UseROI = Source.UseROI;
            this.ROI = Source.ROI;
        }

        public override string ToString()
        {
            return "...";
        }
    }
}
