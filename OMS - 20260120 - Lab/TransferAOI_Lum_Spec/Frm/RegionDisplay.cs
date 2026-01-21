using CommonBase.Logger;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using Emgu.CV.Structure;
using FrameGrabber;
using HardwareManager;
using Matrox.MatroxImagingLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using static OpticalMeasuringSystem.ManualForm;
using static System.Windows.Forms.AxHost;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace OpticalMeasuringSystem
{
    public partial class RegionDisplay : Form
    {
        public event Action LiveView;
        public event Action OneShot;

        MilDisplayManager Display_Main = null;
        MilDisplayManager[] Display_P = new MilDisplayManager[4];

        MIL_ID Img_Draw = MIL.M_NULL;
        MIL_ID Img_Main = MIL.M_NULL;

        MIL_ID[] Img_P = new MIL_ID[4];

        MIL_INT SizeX = 0;
        MIL_INT SizeY = 0;
        MIL_INT Bit = 0;

        MilDigitizer Grabber = null;

        string PanelTag = "";

        Task_AutoPlane AutoPlane = null;
        public bool isWork;
        private bool RoiFollow = false;
        private bool BlockLine = false;


        private Point GoldenCenter = new Point();
        private Point CurrentCenter = new Point();
        private Point CenterDiff = new Point();



        public RegionDisplay(MilDigitizer grabber, Task_AutoPlane AutoPlane)
        {
            InitializeComponent();
            this.Grabber = grabber;
            this.AutoPlane = AutoPlane;

            this.AutoPlane.Update_P1_Diff -= AutoPlane_Update_P1_Diff;
            this.AutoPlane.Update_P1_Diff += AutoPlane_Update_P1_Diff;
            this.AutoPlane.Update_P2_Diff -= AutoPlane_Update_P2_Diff;
            this.AutoPlane.Update_P2_Diff += AutoPlane_Update_P2_Diff;
            this.AutoPlane.Update_P3_Diff -= AutoPlane_Update_P3_Diff;
            this.AutoPlane.Update_P3_Diff += AutoPlane_Update_P3_Diff;
            this.AutoPlane.Update_P4_Diff -= AutoPlane_Update_P4_Diff;
            this.AutoPlane.Update_P4_Diff += AutoPlane_Update_P4_Diff;
            this.AutoPlane.DrawBox -= AutoPlane_DrawBox;
            this.AutoPlane.DrawBox += AutoPlane_DrawBox;

            Init();

            StepToolTip();
        }

        public void StepToolTip()
        {
            // 建立 ToolTip 物件
            ToolTip toolTip = new ToolTip();

            // 設定提示文字顯示的延遲與樣式
            toolTip.AutoPopDelay = 5000;   // 提示文字顯示多久 (毫秒)
            toolTip.InitialDelay = 500;    // 滑鼠移上去多久後顯示 (毫秒)
            toolTip.ReshowDelay = 200;     // 再次顯示的延遲
            toolTip.ShowAlways = true;     // 是否永遠顯示提示文字

            // 對 NumericUpDown 控制項設定提示文字
            toolTip.SetToolTip(NumAngleTolerance, "建議10(粗調), 建議3~5(細調)");
            toolTip.SetToolTip(NumShiftTolerance, "建議100(粗調), 建議10(細調)");

        }

        private void AutoPlane_DrawBox(Point[] TargetPoints, Point[] SourcePoints)
        {
            ReDrawBox(null, TargetPoints, SourcePoints);
        }

        private void AutoPlane_Update_P1_Diff(double DiffX, double DiffY)
        {
            Lbl_P1.Invoke(new Action(() => { Lbl_P1.Text = $"DiffX = {DiffX} , DiffY = {DiffY}"; }));
        }

        private void AutoPlane_Update_P2_Diff(double DiffX, double DiffY)
        {
            Lbl_P2.Invoke(new Action(() => { Lbl_P2.Text = $"DiffX = {DiffX} , DiffY = {DiffY}"; }));
        }

        private void AutoPlane_Update_P3_Diff(double DiffX, double DiffY)
        {
            Lbl_P3.Invoke(new Action(() => { Lbl_P3.Text = $"DiffX = {DiffX} , DiffY = {DiffY}"; }));
        }

        private void AutoPlane_Update_P4_Diff(double DiffX, double DiffY)
        {
            Lbl_P4.Invoke(new Action(() => { Lbl_P4.Text = $"DiffX = {DiffX} , DiffY = {DiffY}"; }));
        }

        public void ReDrawBox(Panel panel, System.Drawing.Point[] TargetPoints, System.Drawing.Point[] SourcePoints)
        {
            int Counts = TargetPoints.Length;
            if (Counts > 1)
            {
                double[] XStart = new double[Counts];
                double[] YStart = new double[Counts];
                double[] XEnd = new double[Counts];
                double[] YEnd = new double[Counts];

                try
                {
                    //Target Points


                    if (BlockLine)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);

                        for (int j = 0; j < Counts; j++)
                        {
                            XStart[j] = TargetPoints[j].X;
                            YStart[j] = TargetPoints[j].Y;

                            int next = (j < 3) ? j + 1 : 0;
                            XEnd[j] = TargetPoints[next].X;
                            YEnd[j] = TargetPoints[next].Y;
                        }

                        MIL.MgraLines(MIL.M_DEFAULT, Img_Draw, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);

                    }

                    // Source Points
                    for (int i = 0; i < Counts; i++)
                    {
                        XStart[i] = SourcePoints[i].X;
                        YStart[i] = SourcePoints[i].Y;

                        int next = (i < 3) ? i + 1 : 0;

                        XEnd[i] = SourcePoints[next].X;
                        YEnd[i] = SourcePoints[next].Y;
                    }

                    if (BlockLine)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                        MIL.MgraLines(MIL.M_DEFAULT, Img_Draw, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);
                    }

                    //GoldenCenter
                    MIL_INT SizeX = MIL.MbufInquire(Img_Draw, MIL.M_SIZE_X, MIL.M_NULL);
                    MIL_INT SizeY = MIL.MbufInquire(Img_Draw, MIL.M_SIZE_Y, MIL.M_NULL);

                    GoldenCenter = new Point {
                        X = (int)(SizeX / 2.0) - 1,
                        Y = (int)(SizeY / 2.0) - 1
                    };

                    if (BlockLine)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);

                        for (int i = -5; i <= 5; i++)
                        {
                            MIL.MgraLine(MIL.M_DEFAULT, Img_Draw, GoldenCenter.X + i, 0, GoldenCenter.X + i, SizeY - 1);
                            MIL.MgraLine(MIL.M_DEFAULT, Img_Draw, 0, GoldenCenter.Y + i, SizeX - 1, GoldenCenter.Y + i);
                        }
                    }

                    //CurrentCenter
                    CurrentCenter = new Point {
                        X = (int)((TargetPoints[0].X + TargetPoints[1].X) / 2.0),
                        Y = (int)((TargetPoints[1].Y + TargetPoints[2].Y) / 2.0),
                    };

                    if (BlockLine)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);

                        for (int i = -5; i <= 5; i++)
                        {
                            MIL.MgraLine(MIL.M_DEFAULT, Img_Draw, CurrentCenter.X + i, CurrentCenter.Y - 150, CurrentCenter.X + i, CurrentCenter.Y + 150);
                            MIL.MgraLine(MIL.M_DEFAULT, Img_Draw, CurrentCenter.X - 150, CurrentCenter.Y + i, CurrentCenter.X + 150, CurrentCenter.Y + i);
                        }
                    }



                    MIL.MbufCopy(Img_Draw, Img_Main);

                    //Center Diff

                    CenterDiff.X = CurrentCenter.X - GoldenCenter.X;
                    CenterDiff.Y = CurrentCenter.Y - GoldenCenter.Y;
                    Lbl_Main_Diff.Invoke(new Action(() => { Lbl_Main_Diff.Text = $"Center Diff (DiffX = {CenterDiff.X} , DiffY = {CenterDiff.Y})"; }));

                }
                catch (Exception ex)
                {
                }
            }
        }

        private void Init()
        {
            Display_Main = new MilDisplayManager(ref MyMil.MilSystem);
            Display_Main.UpdateMousePara -= Display_Main_UpdateMousePara;
            Display_Main.UpdateMousePara += Display_Main_UpdateMousePara;
            Display_Main.Init();

            MIL.MbufInquire(Grabber.grabImage, MIL.M_SIZE_X, ref SizeX);
            MIL.MbufInquire(Grabber.grabImage, MIL.M_SIZE_Y, ref SizeY);
            MIL.MbufInquire(Grabber.grabImage, MIL.M_SIZE_BIT, ref Bit);

            MIL.MbufAllocColor(MyMil.MilSystem, 3, SizeX, SizeY, Bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref Img_Draw);
            MIL.MbufAllocColor(MyMil.MilSystem, 3, SizeX, SizeY, Bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref Img_Main);
            //MIL.MbufAlloc2d(MyMil.MilSystem, Width, Height, 8 + MIL.M_UNSIGNED,
            //      MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_RGB24, ref Img_Main);

            Display_Main.SetWindow(ref Img_Main, ref Pnl_Main);

            Point P1 = new Point { X = (int)(SizeX * 0.1), Y = (int)(SizeY * 0.1) };
            Point P2 = new Point { X = (int)(SizeX * 0.8), Y = (int)(SizeY * 0.1) };
            Point P3 = new Point { X = (int)(SizeX * 0.8), Y = (int)(SizeY * 0.8) };
            Point P4 = new Point { X = (int)(SizeX * 0.1), Y = (int)(SizeY * 0.8) };

            Display_Main.AddROI(new Rectangle { X = P1.X, Y = P1.Y, Height = (int)(SizeY * 0.1), Width = (int)(SizeX * 0.1) });
            Display_Main.AddROI(new Rectangle { X = P2.X, Y = P2.Y, Height = (int)(SizeY * 0.1), Width = (int)(SizeX * 0.1) });
            Display_Main.AddROI(new Rectangle { X = P3.X, Y = P3.Y, Height = (int)(SizeY * 0.1), Width = (int)(SizeX * 0.1) });
            Display_Main.AddROI(new Rectangle { X = P4.X, Y = P4.Y, Height = (int)(SizeY * 0.1), Width = (int)(SizeX * 0.1) });



            Panel[] Pnl = { Pnl_P1, Pnl_P2, Pnl_P3, Pnl_P4 };

            int RoiCnt = -1;

            MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            for (int i = 0; i < RoiCnt; i++)
            {
                Display_P[i] = new MilDisplayManager(ref MyMil.MilSystem);
                Display_P[i].Init(false);

                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                if (W == 0) W = 1;
                if (H == 0) H = 1;

                if (X >= 0 && Y >= 0 && X + W < SizeX && Y + H < SizeY)
                {
                    BufferRelease(ref Img_P[i]);
                    MIL.MbufChild2d(Img_Main, X, Y, W, H, ref Img_P[i]);
                    Display_P[i].SetWindow(ref Img_P[i], ref Pnl[i]);
                }

                Display_P[i].Reset();
                Display_P[i].ChangeZoom(2);
            }
        }

        private void RegionDisplay_Load(object sender, EventArgs e)
        {
            isWork = true;
        }

        private void RegionDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            isWork = false;
            Display_Main.UpdateMousePara -= Display_Main_UpdateMousePara;

            this.AutoPlane.Update_P1_Diff -= AutoPlane_Update_P1_Diff;
            this.AutoPlane.Update_P2_Diff -= AutoPlane_Update_P2_Diff;
            this.AutoPlane.Update_P3_Diff -= AutoPlane_Update_P3_Diff;
            this.AutoPlane.Update_P4_Diff -= AutoPlane_Update_P4_Diff;
            this.AutoPlane.DrawBox -= AutoPlane_DrawBox;

            BufferRelease(ref Img_P[0]);
            BufferRelease(ref Img_P[1]);
            BufferRelease(ref Img_P[2]);
            BufferRelease(ref Img_P[3]);
            BufferRelease(ref Img_Main);
            BufferRelease(ref Img_Draw);

        }

        private void BufferRelease(ref MIL_ID Img)
        {
            if (Img != MIL.M_NULL)
            {
                MIL.MbufFree(Img);
                Img = MIL.M_NULL;
            }
        }

        #region Display Event

        private void Display_Main_UpdateMousePara(int X, int Y, int Gray)
        {
            Lbl_Main_Pos.Invoke(new Action(() => { Lbl_Main_Pos.Text = $"X = {X}, Y = {Y}"; }));
        }

        #endregion

        private void Pnl_Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            Panel[] Pnl = { Pnl_P1, Pnl_P2, Pnl_P3, Pnl_P4 };

            int RoiCnt = -1;

            MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            for (MIL_INT i = 0; i < RoiCnt; i++)
            {
                MIL_INT isSelected = 0;
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, ref isSelected);

                if (isSelected == 0) continue;

                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                // Get Selected

                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                if (W <= 0) W = 1;
                if (H <= 0) H = 1;

                if (X >= 0 && Y >= 0 && X + W < SizeX && Y + H < SizeY)
                {
                    MIL.MbufChildMove(Img_P[i], X, Y, W, H, MIL.M_DEFAULT);
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            System.Windows.Forms.Control sourceControl = menu?.SourceControl;
            PanelTag = sourceControl.Tag.ToString();
        }

        private void Btn_ZoomReset_Click(object sender, EventArgs e)
        {
            switch (PanelTag)
            {
                case "0": Display_Main.Reset(); break;
                case "1": Display_P[0].Reset(); break;
                case "2": Display_P[1].Reset(); break;
                case "3": Display_P[2].Reset(); break;
                case "4": Display_P[3].Reset(); break;
            }
        }

        private void Cbx_RoiFollow_CheckedChanged(object sender, EventArgs e)
        {
            RoiFollow = Cbx_RoiFollow.Checked;
        }

        private void mbtnLive_Click(object sender, EventArgs e)
        {
            LiveView?.Invoke();
        }

        private void mbtnCapture_Click(object sender, EventArgs e)
        {
            OneShot?.Invoke();
        }

        private bool Doing = false;

        public void SetImg(MIL_ID SourceImg)
        {
            if (!isWork) return;
            if (Doing) return;
            Doing = true;

            AutoPlane.Calculate_CornerPoints_And_BoxPoints(-1, false);

            double shiftFactor = Math.Pow(2, 4);

            // 在 While 迴圈內
            // 1. 將 12-bit 的 Ori 除以 16 後存入 8-bit 的 Disp
            MIL.MimArith(SourceImg, shiftFactor, Img_Draw, MIL.M_DIV_CONST);

            int RoiCnt = -1;

            MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref RoiCnt);

            for (MIL_INT i = 0; i < RoiCnt; i++)
            {
                if (RoiFollow)
                {
                    // 1. 計算ROI長寬
                    double rectWidth = 0;
                    double rectHeight = 0;
                    MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_RECTANGLE_WIDTH, ref rectWidth);
                    MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_RECTANGLE_HEIGHT, ref rectHeight);

                    // 2. 計算左上角的位置，使其中心對準目標座標
                    double newPosX = AutoPlane.SourceBoxPoints[i].X - (rectWidth / 2.0);
                    double newPosY = AutoPlane.SourceBoxPoints[i].Y - (rectHeight / 2.0);

                    if ((newPosX) < 0) newPosX = 0;
                    if ((newPosY) < 0) newPosY = 0;
                    if ((newPosX + rectWidth) > SizeX - 1) newPosX = (SizeX - 1) - (rectWidth);
                    if ((newPosY + rectHeight) > SizeY - 1) newPosY = (SizeY - 1) - (rectHeight);

                    // 3. 使用 MgraControlList 更新位置
                    // 注意：必須暫停更新或在同一個物件索引下操作以避免閃爍
                    MIL.MgraControlList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_POSITION_X, newPosX);
                    MIL.MgraControlList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_POSITION_Y, newPosY);
                }

                double startX = 0;
                double startY = 0;
                double endX = 0;
                double endY = 0;

                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_X, ref startX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_TOP_LEFT_Y, ref startY);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_X, ref endX);
                MIL.MgraInquireList(Display_Main.ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_CORNER_BOTTOM_RIGHT_Y, ref endY);

                int X = (int)startX;
                int Y = (int)startY;
                int W = (int)(endX - startX);
                int H = (int)(endY - startY);

                if (W <= 0) W = 1;
                if (H <= 0) H = 1;

                if (X >= 0 && Y >= 0 && X + W < SizeX && Y + H < SizeY)
                {
                    MIL.MbufChildMove(Img_P[i], X, Y, W, H, MIL.M_DEFAULT);
                }

                Display_P[i].CenterAlign();
            }

            string NextStepMsg = JudgeNextStep();

            AppendLog(NextStepMsg);

            Doing = false;
        }

        private void AppendLog(string message)
        {
            // 判斷是否需要跨執行緒呼叫
            if (Rtbx_Log.InvokeRequired)
            {
                Rtbx_Log.Invoke(new Action(() => AppendLog(message)));
                return;
            }

            // 1. 加入時間戳記並換行 (逐行累加)
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

            // 2. 附加文字
            // Rtbx_Log.AppendText(logEntry);
            Rtbx_Log.Text = logEntry;

            // 3. 自動捲動到底部
            Rtbx_Log.SelectionStart = Rtbx_Log.Text.Length;
            Rtbx_Log.ScrollToCaret();
        }

        int NowStep = 0;
        private string JudgeNextStep()
        {
            if (!isWork) return "";

            int ShiftTolerance = 0;
            NumShiftTolerance.Invoke(new Action(() => { ShiftTolerance = (int)NumShiftTolerance.Value; }));

            int AngleTolerance = 0;
            NumAngleTolerance.Invoke(new Action(() => { AngleTolerance = (int)NumAngleTolerance.Value; }));


            string ReturnMsg = "";

            Point[] Source = AutoPlane.SourceBoxPoints;
            Point[] Target = AutoPlane.TargetBoxPoints;
            Point[] Diff = new Point[4];
            Point CenterDiff = this.CenterDiff;

            for (int i = 0; i < 4; i++)
            {
                Diff[i].X = (Source[i].X - Target[i].X);
                Diff[i].Y = (Source[i].Y - Target[i].Y);

                if (Math.Abs(Source[i].X) == 0 || Math.Abs(Source[i].Y) == 0)
                {
                    return "待測物超出偵測範圍 \r\n";
                }
            }

            int Step1_RtnCode = NowStep;
            bool StatusLock = (Step1_RtnCode != 0);

            if (!StatusLock)
            {
                int Step0_RtnCode = Step0_Check_Offset(CenterDiff, ShiftTolerance, ref ReturnMsg);
                if (Step0_RtnCode == 0)
                {
                    return ReturnMsg;
                }

                Step1_RtnCode = Step1_Check_TuningUnit(Diff, AngleTolerance, ref ReturnMsg);
            }


        ReCheck:
            switch (Step1_RtnCode)
            {
                case 2: //P2大(擺頭)
                    {
                        Step2_Check_P2_Tuning(Diff, ref ReturnMsg);
                    }
                    break;

                case 3: //P3P4 X大 (確認點頭)
                    {
                        int Step3_RtnCode = Step3_Check_P3P4X_Tuning(Diff, AngleTolerance, ref ReturnMsg);

                        if (Step3_RtnCode == 3)
                        {
                            Step1_RtnCode = 4;
                            goto ReCheck;
                        }
                    }
                    break;

                case 4: //P3P4 Y大 (確認搖頭)
                    {
                        Step4_Check_P3P4Y_Tuning(Diff, AngleTolerance, ref ReturnMsg);
                    }
                    break;
            }

            return ReturnMsg;
        }

        private int Step0_Check_Offset(Point Diff, int Tolerance, ref string Msg) // 0=NG, 1=OK
        {
            bool X_InTolerance = Math.Abs(Diff.X) <= Tolerance;
            bool Y_InTolerance = Math.Abs(Diff.Y) <= Tolerance;

            if (X_InTolerance && Y_InTolerance) return 1;

            string DirMsg = "";

            if (!X_InTolerance)
            {
                if (Diff.X < 0) DirMsg += "左";
                else DirMsg += "右";
            }

            if (!Y_InTolerance)
            {
                if (Diff.Y < 0) DirMsg += "上";
                else DirMsg += "下";
            }

            Msg += $"待測物偏移，請向{DirMsg}平移 \r\n";

            return 0;
        }

        private int Step1_Check_TuningUnit(Point[] Diff, int Tolerance, ref string Msg) //Return 2=擺頭 , 3=點頭 , 4=搖頭 , 100=完成
        {
            int P2_X = Math.Abs(Diff[1].X);
            int P2_Y = Math.Abs(Diff[1].Y);
            int P2_Max = Math.Max(P2_X, P2_Y);
            int P3_X = Math.Abs(Diff[2].X);
            int P3_Y = Math.Abs(Diff[2].Y);
            int P4_X = Math.Abs(Diff[3].X);
            int P4_Y = Math.Abs(Diff[3].Y);

            bool P3_X_InTolerance = Math.Abs(P3_X - P2_Max) <= Tolerance;
            bool P3_Y_InTolerance = Math.Abs(P3_Y - P2_Max) <= Tolerance;
            bool P4_X_InTolerance = Math.Abs(P4_X - P2_Max) <= Tolerance;
            bool P4_Y_InTolerance = Math.Abs(P4_Y - P2_Max) <= Tolerance;

            if (P3_X_InTolerance && P3_Y_InTolerance && P4_X_InTolerance && P4_Y_InTolerance)
            {
                if (Math.Abs(P3_X - P4_X) > Tolerance)
                {
                    Msg += "P3X、P4X 差距大於 Tolerance \r\n";

                    return 3;
                }

                if (Math.Abs(P3_Y - P4_Y) > Tolerance)
                {
                    Msg += "P3Y、P4Y 差距大於 Tolerance \r\n";

                    return 4;
                }

                Msg += "校正完畢 \r\n";

                return 100;
            }


            List<int> Values = new List<int>();
            Values.Add(P2_Max);
            Values.Add(P3_X);
            Values.Add(P3_Y);
            Values.Add(P4_X);
            Values.Add(P4_Y);

            // 找出其餘四個數中的最大值，再與 P2_Max 比較
            bool P2_isOK = P2_Max <= Tolerance;

            if (!P2_isOK)
            {
                Msg += "P2過大 => 調整擺頭 \r\n";
                return 2;
            }
            else
            {
                bool P3P4_X_isLargest = Values.Max() == P3_X;
                P3P4_X_isLargest |= Values.Max() == P4_X;

                if (P3P4_X_isLargest)
                {
                    Msg += "P3/P4 X最大 => 調整點頭 \r\n";
                    return 3;
                }
                else
                {
                    Msg += "P3/P4 Y最大 => 調整搖頭 \r\n";
                    return 4;
                }
            }
        }

        private int Step2_Check_P2_Tuning(Point[] Diff, ref string Msg) //Return 1=右擺 , 2=左擺
        {
            int DiffY = Diff[1].Y;

            if (DiffY > 0)
            {
                Msg += "當前位置 > 目標位置 => 右擺頭 \r\n";

                return 1;
            }
            else
            {
                Msg += "當前位置 < 目標位置 => 左擺頭 \r\n";

                return 2;
            }
        }

        private int Step3_Check_P3P4X_Tuning(Point[] Diff, int Tolerance, ref string Msg) //Return 1=上點 , 2=下點 , 3=搖頭
        {
            int Diff_P3_X = Diff[2].X;
            int Diff_P4_X = Diff[3].X;

            bool Contradiction = (Diff_P3_X > 0 && Diff_P4_X > 0);
            Contradiction |= (Diff_P3_X < 0 && Diff_P4_X < 0);

            bool InTolerance = Math.Abs(Math.Abs(Diff_P3_X) - Math.Abs(Diff_P4_X)) < Tolerance;

            switch (Contradiction && InTolerance)
            {
                case true:
                    {

                        Msg += "P3、P4 X 趨勢一致 => 矛盾，調整搖頭 \r\n";

                        return 3;
                    }
                    break;

                case false:
                    {
                        if (Math.Abs(Diff_P3_X) > Math.Abs(Diff_P4_X))
                        {
                            if (Diff_P3_X < 0)
                            {
                                Msg += "P3X > P4X ， 當前位置 < 目標位置 => 上點頭 \r\n";

                                return 1;
                            }
                            else
                            {
                                Msg += "P3X > P4X ， 當前位置 > 目標位置 => 下點頭 \r\n";

                                return 2;
                            }
                        }
                        else
                        {
                            if (Diff_P4_X > 0)
                            {
                                Msg += "P4X > P3X ， 當前位置 > 目標位置 => 上點頭 \r\n";

                                return 1;
                            }
                            else
                            {
                                Msg += "P4X > P3X ， 當前位置 < 目標位置 => 下點頭 \r\n";

                                return 2;
                            }
                        }
                    }
                    break;
            }

            return 3;

        }

        private int Step4_Check_P3P4Y_Tuning(Point[] Diff, int Tolrence, ref string Msg) //Return 1=左搖 , 2=右搖, 3=擺頭
        {
            int Diff_P2_X = Diff[1].X;
            int Diff_P2_Y = Diff[1].Y;
            int Diff_P3_Y = Diff[2].Y;
            int Diff_P4_Y = Diff[3].Y;

            int Diff_P2_Max = Math.Max(Math.Abs(Diff_P2_X), Math.Abs(Diff_P2_Y));
            int Diff_P2_Range_Upper = Diff_P2_Max + Tolrence;
            int Diff_P2_Range_Lower = Diff_P2_Max - Tolrence;


            if ((Math.Abs(Diff_P3_Y) < Tolrence && Math.Abs(Diff_P4_Y) < Tolrence))
            {
                int Diff_P3_X = Diff[2].X;
                int Diff_P4_X = Diff[3].X;

                bool P3_InRange = (Math.Abs(Diff_P3_X) < Diff_P2_Range_Upper && Math.Abs(Diff_P3_X) > Diff_P2_Range_Lower);
                bool P4_InRange = (Math.Abs(Diff_P4_X) < Diff_P2_Range_Upper && Math.Abs(Diff_P4_X) > Diff_P2_Range_Lower);

                if (P3_InRange && P4_InRange)
                {
                    Msg += "校正完畢 \r\n";
                    return 100;
                }
                else
                {
                    if (Diff_P3_X < 0)
                    {
                        Msg += "P3X、P4X > P2+Tolerance => 右擺頭 \r\n";
                    }
                    else
                    {
                        Msg += "P3X、P4X > P2+Tolerance => 左擺頭 \r\n";
                    }

                    return 3;
                }
            }
            else
            {
                if (Math.Abs(Diff_P3_Y) > Math.Abs(Diff_P4_Y))
                {
                    if (Diff_P3_Y < 0)
                    {
                        Msg += "P3Y > P4Y ， 當前位置 < 目標位置 => 左搖頭 \r\n";

                        return 1;
                    }
                    else
                    {
                        Msg += "P3Y > P4Y ， 當前位置 > 目標位置 => 右搖頭 \r\n";

                        return 2;
                    }
                }
                else
                {
                    if (Diff_P4_Y < 0)
                    {
                        Msg += "P4Y > P3Y ， 當前位置 < 目標位置 => 右搖頭 \r\n";

                        return 2;
                    }
                    else
                    {
                        Msg += "P4Y > P3Y ， 當前位置 < 目標位置 => 左搖頭 \r\n";

                        return 1;
                    }
                }
            }

        }


        private void Btn_ZoomIn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                Display_P[i].ZoomIn();
            }
        }

        private void Btn_ZoomOut_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                Display_P[i].ZoomOut();
            }
        }

        private void Btn_Reset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                Display_P[i].Reset();
                Display_P[i].ChangeZoom(2);
            }
        }

        private void Cbx_BlockLine_CheckedChanged(object sender, EventArgs e)
        {
            BlockLine = Cbx_BlockLine.Checked;
        }
    }
}
