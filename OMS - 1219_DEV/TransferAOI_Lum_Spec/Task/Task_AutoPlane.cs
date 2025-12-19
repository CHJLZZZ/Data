using System;
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

using Matrox.MatroxImagingLibrary;
using FrameGrabber;
using BaseTool;
using System.ComponentModel;

namespace OpticalMeasuringSystem
{
    //平移：
    //沿X軸左右移動
    //沿Y軸前後移動
    //沿Z軸上下移動

    //旋轉：
    //繞X軸前後旋轉（Roll）(點頭)
    //繞Y軸旋轉。（Pitch）(擺頭)
    //繞Z軸左右旋轉(Yaw）。(搖頭)

    #region --- [1] AutoPlaneAlign_Step ---
    // Adjust_Pitch : 旋轉，影響角度
    // Adjust_Roll : 旋轉，影響 X 偏移量
    // Adjust_Yaw : 旋轉，影響 Y 偏移量

    public enum AutoPlaneAlign_Step
    {
        GetOriSourceCornerPoints_And_TargetBoxPoints = 0,
        Align_LeftTop_Point_And_P2P3_X,
        Adjust_Pitch, //擺頭 (左高右低/左低右高)
        Check_MaxDeviation_Direction,
        Adjust_Roll, //點頭 (上短下長/上長下短)
        Adjust_Yaw, //搖頭 (左短右長/左長右短)

        Stop,
        Error,
        TimeOut,
        Finish,
    }

    #endregion --- [1] AutoFocus_Step ---

    #region --- [2] Enum_Corner ---

    public enum Enum_Corner
    {
        Left_Top = 0,
        Right_Top,
        Right_Bottom,
        Left_Bottom,
    }

    #endregion --- [2] Enum_Corner ---

    #region --- [3] Structure_RobotPos ---
    public struct Structure_RobotPos
    {
        public Structure_RobotPos(double x, double y, double z, double roll, double pitch, double yaw)
        {
            X = x;
            Y = y;
            Z = z;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }

        public double X;
        public double Y;
        public double Z;
        public double Roll;
        public double Pitch;
        public double Yaw;
    };
    #endregion"--- [3] Structure_RobotPos ---"

    #region --- [4] Enum_Deviation_Direction ---

    public enum Enum_Deviation_Direction
    {
        X3 = 0,
        Y3,
        X4,
        Y4,
        P3P4_OK
    }

    #endregion --- [4] Enum_Deviation_Direction ---

    #region --- [5] Enum_Jog ---

    public enum Enum_Jog
    {
        Add = 0,
        Minus
    }

    #endregion --- [5] Enum_Jog ---

    #region --- [6] Enum_RobotDoF ---

    public enum Enum_RobotDoF
    {
        X = 0,
        Y,
        Z,
        Roll,
        Pitch,
        Yaw
    }

    #endregion --- [6] Enum_RobotDoF ---


    public class Task_AutoPlane
    {
        private double _ThetaToRad = Math.PI / 180D;

        private MIL_ID milSys = MIL.M_NULL;
        private CommonBase.Logger.InfoManager info;
        private MilDigitizer grabber;
        private ClsUrRobotInterface urRobot;

        private MIL_ID imgSrc = MIL.M_NULL;

        private Point[] Source_Points = new Point[4];
        private Point[] Target_Points = new Point[4];
        public Structure_RobotPos urRobotPos = new Structure_RobotPos(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
        private double[] MoveStepMutiply = { 1.0, 10.0, 100.0 };  //速度控制
        private double MoveStep = 0;      //速度控制
        private double MoveAcc = 0.15;    //加速度
        private double MoveSpeed = 0.15;  //速度
        private double JogMultiply = 1;      //步伐倍數加成
        private int ImgProc_Threshold = 128;      //二值化閥值

        private bool isNormalAngle_0D = false;
        private int deviation_Tolerance = 3;

        public bool FlowRun = false;
        public AutoPlaneAlign_Step Proc_Step = AutoPlaneAlign_Step.GetOriSourceCornerPoints_And_TargetBoxPoints;


        public Task_AutoPlane(MIL_ID milSys, MilDigitizer grabber, ClsUrRobotInterface urRobot, InfoManager info, bool Camera_isNormalAngle)
        {
            this.milSys = milSys;
            this.info = info;
            this.grabber = grabber;
            this.urRobot = urRobot;
            this.isNormalAngle_0D = Camera_isNormalAngle;
        }

        #region --- Property ---"
        public Point[] TargetBoxPoints
        {
            get => Target_Points;
        }

        public Point[] SourceBoxPoints
        {
            get => Source_Points;
        }
        #endregion --- Property ---"

        #region --- 方法函式 ---"

        #region --- MilBufferFree ---
        public void MilBufferFree(ref MIL_ID img)
        {
            if (img != MIL.M_NULL)
            {
                MIL.MbufFree(img);
                img = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilBlobFree ---
        public void MilBlobFree(ref MIL_ID milBlobContext)
        {
            if (milBlobContext != MIL.M_NULL)
            {
                MIL.MblobFree(milBlobContext);
                milBlobContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilMeasFree ---
        public void MilMeasFree(ref MIL_ID milMeasContext)
        {
            if (milMeasContext != MIL.M_NULL)
            {
                MIL.MmeasFree(milMeasContext);
                milMeasContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilPatternFree ---
        public void MilPatternFree(ref MIL_ID img)
        {
            if (img != MIL.M_NULL)
            {
                MIL.MpatFree(img);
                img = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilMimFree ---
        public void MilMimFree(ref MIL_ID mimContext)
        {
            if (mimContext != MIL.M_NULL)
            {
                MIL.MimFree(mimContext);
                mimContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilCalFree ---
        public void MilCalFree(ref MIL_ID milCalContext)
        {
            if (milCalContext != MIL.M_NULL)
            {
                MIL.McalFree(milCalContext);
                milCalContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- CalculateIntersectionPoint ---
        private void CalculateIntersectionPoint(double[] Line1, double[] Line2, ref Point IntersectionPoint)
        {

            try //A1X+B1Y+C1=0 A2X+B2Y+C2=0
            {
                if ((Line1[0] * Line2[1] - Line1[1] * Line2[0]) == 0)
                {
                    IntersectionPoint.X = 0;
                    IntersectionPoint.Y = 0;
                }
                else
                {
                    IntersectionPoint.X = Convert.ToInt32((Line1[1] * Line2[2] - Line2[1] * Line1[2]) / (Line1[0] * Line2[1] - Line2[0] * Line1[1]));
                    IntersectionPoint.Y = Convert.ToInt32((Line1[0] * Line2[2] - Line2[0] * Line1[2]) / (Line2[0] * Line1[1] - Line1[0] * Line2[1]));
                }
            }
            catch
            {
                IntersectionPoint.X = 0;
                IntersectionPoint.Y = 0;
            }
        }
        #endregion --- CalculateIntersectionPoint ---

        #region --- FindCornerbyEdgeFinder ---
        private void FindCornerbyEdgeFinder(MIL_ID mil_8U_SrcImag, ref Point[] cornerpoint)
        {
            //    P0----------P2
            //    |           |
            //    |           |
            //    P1----------P3

            MIL_INT ImageSizeX = MIL.M_NULL;
            MIL_INT ImageSizeY = MIL.M_NULL;
            MIL_ID MBlobContext = MIL.M_NULL;
            MIL_ID MBlobResult = MIL.M_NULL;
            MIL_ID MEdgeMarker = MIL.M_NULL;
            int BlobCount = 0;
            int[] BlobboxInfo = new int[4]; //minX, minY, maxX, maxY
            Point[] BlobboxCorner = new Point[4]; //0:lefttop, 1:leftdown, 2:righttop, 3:rightdown
            int EdgeCount = 0;
            double[] HLineCoef = new double[3];
            double[] VLineCoef = new double[3];

            try
            {
                MIL.MbufInquire(mil_8U_SrcImag, MIL.M_SIZE_X, ref ImageSizeX);
                MIL.MbufInquire(mil_8U_SrcImag, MIL.M_SIZE_Y, ref ImageSizeY);

                //blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobContext);
                MIL.MblobControl(MBlobContext, MIL.M_FOREGROUND_VALUE, MIL.M_NON_ZERO);
                MIL.MblobControl(MBlobContext, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);
                MIL.MblobControl(MBlobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(MBlobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(MBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobResult);
                MIL.MblobCalculate(MBlobContext, mil_8U_SrcImag, MIL.M_NULL, MBlobResult);
                MIL.MblobGetResult(MBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref BlobCount);
                if (BlobCount == 0) { return; }
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[0]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[1]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[2]);
                MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[3]);

                //blobcorner
                BlobboxCorner[0].X = BlobboxInfo[0];
                BlobboxCorner[0].Y = BlobboxInfo[1];
                BlobboxCorner[1].X = BlobboxInfo[0];
                BlobboxCorner[1].Y = BlobboxInfo[3];
                BlobboxCorner[2].X = BlobboxInfo[2];
                BlobboxCorner[2].Y = BlobboxInfo[1];
                BlobboxCorner[3].X = BlobboxInfo[2];
                BlobboxCorner[3].Y = BlobboxInfo[3];

                //edgeset
                MIL.MmeasAllocMarker(MyMil.MilSystem, MIL.M_EDGE, MIL.M_DEFAULT, ref MEdgeMarker);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_MODE, MIL.M_ENABLE, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_DELTA_POS, 5, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_DELTA_NEG, 5, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_TOLERANCE, 1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_ANGLE_ACCURACY, 0.1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_NUMBER, 1, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_POLARITY, MIL.M_ANY, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_SEARCH_REGION_CLIPPING, MIL.M_MAXIMIZE_AREA, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_FILTER_TYPE, MIL.M_SHEN, MIL.M_NULL);
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_SIZE, (BlobboxInfo[2] - BlobboxInfo[0]) / 3 * 2, (BlobboxInfo[3] - BlobboxInfo[1]) / 3 * 2);

                //findedge & crosspoint
                for (int cornerindex = 0; cornerindex < BlobboxCorner.Length; cornerindex += 1)
                {
                    //center
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_CENTER, BlobboxCorner[cornerindex].X, BlobboxCorner[cornerindex].Y);

                    //horizontal
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_SIZE, (BlobboxInfo[2] - BlobboxInfo[0]) / 3 * 2, (BlobboxInfo[3] - BlobboxInfo[1]) / 8);

                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
                    MIL.MmeasFindMarker(MIL.M_DEFAULT, mil_8U_SrcImag, MEdgeMarker, MIL.M_LINE_EQUATION);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                    if (EdgeCount == 0) { continue; }
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[0]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[1]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[2]);

                    //vertical
                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_BOX_SIZE, (BlobboxInfo[2] - BlobboxInfo[0]) / 8, (BlobboxInfo[3] - BlobboxInfo[1]) / 3 * 2);

                    MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
                    MIL.MmeasFindMarker(MIL.M_DEFAULT, mil_8U_SrcImag, MEdgeMarker, MIL.M_LINE_EQUATION);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                    if (EdgeCount == 0) { continue; }
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[0]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[1]);
                    MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[2]);

                    //crosspoint
                    CalculateIntersectionPoint(HLineCoef, VLineCoef, ref cornerpoint[cornerindex]);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                this.MilBlobFree(ref MBlobContext);
                this.MilBlobFree(ref MBlobResult);
                this.MilMeasFree(ref MEdgeMarker);
            }
        }
        #endregion --- FindCornerbyEdgeFinder ---

        #region --- CalculateCornerPoints_And_BoxPoints ---
        private bool CalculateCornerPoints_And_BoxPoints(MIL_ID sourceImage, bool isGetBox, int Threshold, ref Point[] cornerPoint, ref Point[] BoxPoint)
        {
            //    P0----------P1
            //    |            |
            //    |            |
            //    P3----------P2

            MIL_ID blobContext = MIL.M_NULL;
            MIL_ID blobResult = MIL.M_NULL;
            MIL_ID mil_16U_SrcImag = MIL.M_NULL;

            int[] BlobboxInfo = new int[4];      //minX, minY, maxX, maxY
            Point[] Corner_Point = new Point[4];

            int blobCount = 0;
            bool Debug = false;

            try
            {
                MIL_INT SizeX = MIL.MbufInquire(sourceImage, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT SizeY = MIL.MbufInquire(sourceImage, MIL.M_SIZE_Y, MIL.M_NULL);

                // Blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult);
                //搜尋最大面積的 Blob
                MIL.MblobControl(blobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(blobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
                MIL.MblobControl(blobContext, MIL.M_SAVE_RUNS, MIL.M_ENABLE);

                MIL.MbufAlloc2d(MyMil.MilSystem, SizeX, SizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_16U_SrcImag);

                //MIL.MbufTransfer(sourceImage, mil_16U_SrcImag, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT,
                //    MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);
                MIL.MbufCopy(sourceImage, mil_16U_SrcImag);

                //for (int i = 0; i < 6; i++)
                //    MIL.MimConvolve(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_SMOOTH);

                if (Debug)
                    MIL.MbufSave(@"D:/mil_16U_SrcImag_Before.tif", mil_16U_SrcImag);

                //MIL.MimArith(mil_16U_SrcImag, MIL.M_NULL, mil_16U_SrcImag, MIL.M_NOT);

                if (Threshold == -1)
                {
                    MIL_INT BinValue = MIL.MimBinarize(mil_16U_SrcImag, MIL.M_NULL, MIL.M_BIMODAL, MIL.M_NULL, MIL.M_NULL);
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_GREATER_OR_EQUAL, BinValue, MIL.M_NULL);
                }
                else
                {
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_FIXED + MIL.M_LESS, Threshold, MIL.M_NULL);
                }

                if (Debug)
                    MIL.MbufSave(@"D:/mil_16U_SrcImag_TH.tif", mil_16U_SrcImag);

                //MIL.MimOpen(mil_8U_SrcImag, mil_8U_SrcImag, OpenCount, MIL.M_GRAYSCALE);

                //if (Debug)
                //    MIL.MbufSave(@"D:/mil_8U_SrcImag_Open.tif", mil_8U_SrcImag);

                MIL.MblobCalculate(blobContext, mil_16U_SrcImag, MIL.M_NULL, blobResult);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);
                MIL.MblobDraw(MIL.M_DEFAULT, blobResult, mil_16U_SrcImag, MIL.M_DRAW_HOLES, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                MIL.MimOpen(mil_16U_SrcImag, mil_16U_SrcImag, 2, MIL.M_BINARY);

                if (Debug)
                    MIL.MbufSave(@"D:/mil_16U_SrcImag_After.tif", mil_16U_SrcImag);


                if (cornerPoint == null)
                    cornerPoint = new Point[4];

                this.FindCornerbyEdgeFinder(mil_16U_SrcImag, ref Corner_Point);

                //Left - Top
                cornerPoint[0].X = Corner_Point[0].X;
                cornerPoint[0].Y = Corner_Point[0].Y;
                //Right - Top
                cornerPoint[1].X = Corner_Point[2].X;
                cornerPoint[1].Y = Corner_Point[2].Y;
                //Right - Bottom
                cornerPoint[2].X = Corner_Point[3].X;
                cornerPoint[2].Y = Corner_Point[3].Y;
                //Left - Bottom
                cornerPoint[3].X = Corner_Point[1].X;
                cornerPoint[3].Y = Corner_Point[1].Y;

                // Get Box Points
                if (isGetBox)
                {
                    MIL.MblobCalculate(blobContext, mil_16U_SrcImag, MIL.M_NULL, blobResult);
                    MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                    if (blobCount > 0)
                    {
                        MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[0]);
                        MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[1]);
                        MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[2]);
                        MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[3]);

                        //--- Box Corner ---
                        // Left-Top
                        BoxPoint[0].X = BlobboxInfo[0];
                        BoxPoint[0].Y = BlobboxInfo[1];
                        // Right-Top
                        BoxPoint[1].X = BlobboxInfo[2];
                        BoxPoint[1].Y = BlobboxInfo[1];
                        // Right-Bottom
                        BoxPoint[2].X = BlobboxInfo[2];
                        BoxPoint[2].Y = BlobboxInfo[3];
                        // Left-Bottom
                        BoxPoint[3].X = BlobboxInfo[0];
                        BoxPoint[3].Y = BlobboxInfo[3];
                    }
                    else
                    {
                        Console.WriteLine("Can not Find Align Box !");
                        return false;
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("[CalculateCornerPoints_And_BoxPoints] " + ex.Message + "(" + ex.StackTrace + ")");
            }
            finally
            {
                this.MilBufferFree(ref mil_16U_SrcImag);
                this.MilBlobFree(ref blobContext);
                this.MilBlobFree(ref blobResult);
            }

        }

        #endregion --- CalculateCornerPoints ---

        #region --- Calculate_AutoPlaneAlign ---
        public void Start(AutoPlane_FlowPara Para)
        {
            if (!FlowRun)
            {
                FlowRun = true;
                Thread mThread = new Thread(() => AutoPlaneAlign_Proc(Para));
                mThread.Start();
            }
            else
            {
                FlowRun = false;
                Start(Para);
            }
        }

        public void Stop()
        {
            FlowRun = false;
        }

        #endregion --- Calculate_AutoPlaneAlign ---

        #region --- Calculate_CornerPoints_And_BoxPoints ---
        public void Calculate_CornerPoints_And_BoxPoints(int Threshold)
        {
            this.CalculateCornerPoints_And_BoxPoints(this.grabber.grabImage, true, Threshold, ref this.Source_Points, ref this.Target_Points);

            this.Align_LeftTop_Point_And_P2P3_X(ref this.Source_Points, ref this.Target_Points);

            for (int i = 0; i < this.Source_Points.Length; i++)
            {
                Console.WriteLine($"P{i} DiffX : {Math.Abs(this.Source_Points[i].X - this.Target_Points[i].X)}");
                Console.WriteLine($"P{i} DiffY : {Math.Abs(this.Source_Points[i].Y - this.Target_Points[i].Y)}");
                //Console.WriteLine($"[Target] P{i} (X, Y) : ({this.Target_Points[i].X}, {this.Target_Points[i].Y})");
                //Console.WriteLine($"[Source] P{i} (X, Y) : ({this.Source_Points[i].X}, {this.Source_Points[i].Y})");
            }
        }
        #endregion --- Calculate_CornerPoints_And_BoxPoints ---

        #region --- Update_RobotPosInfo ---
        public bool Update_RobotPosInfo()
        {
            if (this.urRobot != null && this.urRobot.IsConnected())
            {
                double roll = 0.0;
                double pitch = 0.0;
                double yaw = 0.0;

                urRobotPos.X = this.urRobot.m_ClsUrStatus.TcpPose[0];
                urRobotPos.Y = this.urRobot.m_ClsUrStatus.TcpPose[1];
                urRobotPos.Z = this.urRobot.m_ClsUrStatus.TcpPose[2];

                // RV 值 To Radian
                this.urRobot.RvToRpy(
                    this.urRobot.m_ClsUrStatus.TcpPose[3],
                    this.urRobot.m_ClsUrStatus.TcpPose[4],
                    this.urRobot.m_ClsUrStatus.TcpPose[5],
                    ref roll, ref pitch, ref yaw);

                // 目前狀態為 Roll 與 Pitch 顛倒
                urRobotPos.Roll = roll;
                urRobotPos.Pitch = pitch;
                urRobotPos.Yaw = yaw;


                //Console.WriteLine($"urRobotPos.X : {urRobotPos.X}");
                //Console.WriteLine($"urRobotPos.Y : {urRobotPos.Y}");
                //Console.WriteLine($"urRobotPos.Z : {urRobotPos.Z}");            
                //Console.WriteLine($"========= [Update_RobotPosInfo] =========");
                //Console.WriteLine($"urRobotPos.Roll : {urRobotPos.Roll}");
                //Console.WriteLine($"urRobotPos.Pitch : {urRobotPos.Pitch}");
                //Console.WriteLine($"urRobotPos.Yaw : {urRobotPos.Yaw}");

                return true;
            }
            else
                return false;
        }
        #endregion --- Update_RobotPosInfo ---

        #region --- Set_RobotPosInfo (double x, double y, double z, double roll, double pitch, double yaw) ---
        public void Set_RobotPosInfo(double x, double y, double z, double roll, double pitch, double yaw)
        {
            // roll、pitch、yaw : 要 Radian 值
            if (this.urRobot != null && this.urRobot.IsConnected())
            {
                urRobotPos.X = x;
                urRobotPos.Y = y;
                urRobotPos.Z = z;
                urRobotPos.Roll = roll;    // Radian
                urRobotPos.Pitch = pitch;  // Radian
                urRobotPos.Yaw = yaw;      // Radian
            }
        }
        #endregion --- Set_RobotPosInfo ---

        #region --- Set_RobotPosInfo (Enum_RobotDoF dof, Enum_Jog jog, int moveStepIndex) ---
        public void Set_RobotPosInfo(Enum_RobotDoF dof, Enum_Jog jog, int moveStepIndex)
        {
            double Multiply = 1;

            this.MoveStep = this.MoveStepMutiply[moveStepIndex];

            switch (dof)
            {
                case Enum_RobotDoF.X:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.X += 0.1 * this.MoveStep;
                    else
                        this.urRobotPos.X -= 0.1 * this.MoveStep;
                    break;
                case Enum_RobotDoF.Y:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.Y += 0.1 * this.MoveStep;
                    else
                        this.urRobotPos.Y -= 0.1 * this.MoveStep;
                    break;

                case Enum_RobotDoF.Z:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.Z += 0.1 * this.MoveStep;
                    else
                        this.urRobotPos.Z -= 0.1 * this.MoveStep;
                    break;

                // 移動 Scale 為 : 0.01、or 0.1、or 1 度
                case Enum_RobotDoF.Roll:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.Pitch += 0.01 * Multiply * this.MoveStep * _ThetaToRad;  // Radian
                    else
                        this.urRobotPos.Pitch -= 0.01 * Multiply * this.MoveStep * _ThetaToRad;  // Radian
                    break;

                // 移動 Scale 為 : 0.01、or 0.1、or 1 度
                case Enum_RobotDoF.Pitch:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.Roll += 0.01 * Multiply * this.MoveStep * _ThetaToRad;  //Radian
                    else
                        this.urRobotPos.Roll -= 0.01 * Multiply * this.MoveStep * _ThetaToRad;  //Radian
                    break;

                // 移動 Scale 為 : 0.01、or 0.1、or 1 度
                case Enum_RobotDoF.Yaw:
                    if (jog == Enum_Jog.Add)
                        this.urRobotPos.Yaw += 0.01 * Multiply * this.MoveStep * _ThetaToRad;  //Radian
                    else
                        this.urRobotPos.Yaw -= 0.01 * Multiply * this.MoveStep * _ThetaToRad;  //Radian
                    break;
            }
        }
        #endregion --- Set_RobotPosInfo ---

        #region --- MoveToPos ---
        public void MoveToPos()
        {
            // PS.urRobotPos.Roll、urRobotPos.Pitch、urRobotPos.Yaw 要 Radian 值。

            try
            {
                if (this.urRobot != null && this.urRobot.IsConnected())
                {
                    Console.WriteLine($"================== [MoveToPos] ==================");
                    //Console.WriteLine($"[Adjust_Roll] X : {urRobotPos.X }");
                    //Console.WriteLine($"[Adjust_Roll] Y : {urRobotPos.Y }");
                    //Console.WriteLine($"[Adjust_Roll] Z : {urRobotPos.Z }");
                    Console.WriteLine($"Roll : {urRobotPos.Roll}");
                    Console.WriteLine($"Pitch : {urRobotPos.Pitch}");
                    Console.WriteLine($"Yaw : {urRobotPos.Yaw}");

                    double tmpTime = 0.0D;
                    double tmpBlendRadius = 0.0D;

                    // 注意 :目前為 Roll與 Pitch 顛倒
                    this.urRobot.MoveL(urRobotPos.X, urRobotPos.Y, urRobotPos.Z,
                        urRobotPos.Roll, urRobotPos.Pitch, urRobotPos.Yaw,
                        this.MoveAcc, this.MoveSpeed, tmpTime, tmpBlendRadius);

                    // Update Image
                    this.GrabImage();
                }
            }
            catch (Exception ex)
            {
                this.info.Error($"[MoveTcp] {ex.Message}");
            }
        }
        #endregion --- MoveToPos ---

        #region --- MoveStop ---
        public void MoveStop()
        {
            try
            {
                if (this.urRobot != null && this.urRobot.IsConnected())
                {
                    this.urRobot.Stop();

                    // Update Image
                    this.GrabImage();
                }
            }
            catch (Exception ex)
            {
                this.info.Error($"[MoveStop] {ex.Message}");
            }
        }
        #endregion --- MoveStop ---

        #region --- GrabImage ---
        private void GrabImage()
        {
            if (this.grabber != null)
            {
                this.grabber.Grab();
            }
        }
        #endregion

        #region --- IsMoving ---
        public bool IsMoving()
        {
            return this.urRobot.IsMoving();
        }

        #endregion #region --- IsMoving ---

        #region --- Check_MaxDeviation_Direction ---
        private void Check_MaxDeviation_Direction(ref Enum_Deviation_Direction Direction, ref int shiftValue, ref Enum_Corner P_idx, int Tolerance)
        {
            bool isP3P4_OK = true;
            try
            {
                Dictionary<Enum_Deviation_Direction, int> ShiftInfo = new Dictionary<Enum_Deviation_Direction, int>();
                double[] Shift_P3_P4 = new double[4];

                int shift_X3 = Math.Abs(this.Source_Points[2].X - this.Target_Points[2].X);
                ShiftInfo.Add(Enum_Deviation_Direction.X3, shift_X3);
                Shift_P3_P4[0] = shift_X3;

                int shift_Y3 = Math.Abs(this.Source_Points[2].Y - this.Target_Points[2].Y);
                ShiftInfo.Add(Enum_Deviation_Direction.Y3, shift_Y3);
                Shift_P3_P4[1] = shift_Y3;

                int shift_X4 = Math.Abs(this.Source_Points[3].X - this.Target_Points[3].X);
                ShiftInfo.Add(Enum_Deviation_Direction.X4, shift_X4);
                Shift_P3_P4[2] = shift_X4;

                int shift_Y4 = Math.Abs(this.Source_Points[3].Y - this.Target_Points[3].Y);
                ShiftInfo.Add(Enum_Deviation_Direction.Y4, shift_Y4);
                Shift_P3_P4[3] = shift_Y4;

                //[1] Sorting                                     
                var sortedDict = ShiftInfo.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

                //[2] 1st Max Shift Value
                Direction = sortedDict.Keys.ToList()[0];
                shiftValue = sortedDict.Values.ToList()[0];

                // Check P3 P4 OK
                for (int i = 0; i < Shift_P3_P4.Length; i++)
                {
                    if (sortedDict.Values.ToList()[i] > Tolerance)
                    {
                        isP3P4_OK = false;
                        break;
                    }
                }

                if (isP3P4_OK)
                    Direction = Enum_Deviation_Direction.P3P4_OK;

                // Get Corner Point
                for (int i = 0; i < Shift_P3_P4.Length; i++)
                {
                    if (shiftValue == Shift_P3_P4[i])
                    {
                        switch (Direction)
                        {
                            case Enum_Deviation_Direction.X3:
                            case Enum_Deviation_Direction.Y3:
                                P_idx = Enum_Corner.Right_Bottom;
                                break;
                            case Enum_Deviation_Direction.X4:
                            case Enum_Deviation_Direction.Y4:
                                P_idx = Enum_Corner.Left_Bottom;
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                this.info.Error($"[Check_MaxDeviation_Direction] {ex.Message}");
            }
        }
        #endregion --- Check_MaxDeviation_Direction ---

        #region --- Check_AutoPlaneAlign_Finish ---
        private bool Check_AutoPlaneAlign_Finish(int deviation_Tolerance)
        {
            bool OK = true;
            int shiftX = 0;
            int shiftY = 0;

            Console.WriteLine("--- Check_AutoPlaneAlign_Finish ---");
            for (int i = 0; i < this.Source_Points.Length; i++)
            {
                shiftX = Math.Abs(this.Source_Points[i].X - this.Target_Points[i].X);
                shiftY = Math.Abs(this.Source_Points[i].Y - this.Target_Points[i].Y);

                if (!(shiftX <= deviation_Tolerance && shiftY <= deviation_Tolerance))
                {
                    OK = false;
                    if (shiftX > deviation_Tolerance)
                        Console.WriteLine($"P{i} DiffX : {shiftX} > {deviation_Tolerance} was not OK !");
                    if (shiftY > deviation_Tolerance)
                        Console.WriteLine($"P{i} DiffY : {shiftY} > {deviation_Tolerance} was not OK !");

                    break;
                }

            }
            Console.WriteLine("-------------------------------------");

            return OK;
        }
        #endregion --- Check_AutoPlaneAlign_Finish ---

        #region --- Check_AutoPlaneAlign_Finish ---
        private bool Check_AutoPlaneAlign_P2P3_Finish(int deviation_Tolerance)
        {
            bool OK = true;
            int shiftY1 = 0;
            int shiftY2 = 0;
            int shiftX1 = 0;
            int shiftX2 = 0;

            shiftY1 = Math.Abs(this.Source_Points[0].Y - this.Source_Points[1].Y);
            shiftY2 = Math.Abs(this.Source_Points[2].Y - this.Source_Points[3].Y);

            if (!(shiftY1 <= deviation_Tolerance && shiftY2 <= deviation_Tolerance))
                OK = false;

            shiftX1 = Math.Abs(this.Source_Points[0].X - this.Source_Points[3].X);
            shiftX2 = Math.Abs(this.Source_Points[1].X - this.Source_Points[2].X);

            if (!(shiftX1 <= deviation_Tolerance && shiftX2 <= deviation_Tolerance))
                OK = false;

            return OK;
        }
        #endregion --- Check_AutoPlaneAlign_Finish ---

        #region --- Align_LeftTop_Point_And_P2P3_X ---
        private void Align_LeftTop_Point_And_P2P3_X(ref Point[] cornerPoint, ref Point[] boxPoint)
        {
            int shiftX = boxPoint[0].X - cornerPoint[0].X;
            int shiftY = boxPoint[0].Y - cornerPoint[0].Y;

            if (shiftX != 0 || shiftY != 0)
            {
                for (int idx = 0; idx < cornerPoint.Length; idx += 1)
                {
                    boxPoint[idx].X = boxPoint[idx].X - shiftX;
                    boxPoint[idx].Y = boxPoint[idx].Y - shiftY;
                }
            }
            // Left-Top X Align
            boxPoint[(int)Enum_Corner.Left_Bottom].X = cornerPoint[(int)Enum_Corner.Left_Top].X;

            // Align P2,x To S2,x、P3,x To S3,x
            boxPoint[(int)Enum_Corner.Right_Top].X = cornerPoint[(int)Enum_Corner.Right_Top].X;
            boxPoint[(int)Enum_Corner.Right_Bottom].X = cornerPoint[(int)Enum_Corner.Right_Top].X;

            // Align P2、P3 Y
            if (cornerPoint[(int)Enum_Corner.Left_Bottom].Y > cornerPoint[(int)Enum_Corner.Right_Bottom].Y)
            {
                boxPoint[(int)Enum_Corner.Left_Bottom].Y = cornerPoint[(int)Enum_Corner.Left_Bottom].Y;
                boxPoint[(int)Enum_Corner.Right_Bottom].Y = cornerPoint[(int)Enum_Corner.Left_Bottom].Y;
            }
            else
            {
                boxPoint[(int)Enum_Corner.Left_Bottom].Y = cornerPoint[(int)Enum_Corner.Right_Bottom].Y;
                boxPoint[(int)Enum_Corner.Right_Bottom].Y = cornerPoint[(int)Enum_Corner.Right_Bottom].Y;
            }

            //
            Console.WriteLine($"================== [Align_LeftTop_Point_And_P2P3_X] ==================");
            //for (int idx = 0; idx < cornerPoint.Length; idx += 1)
            //{
            //    Console.WriteLine($"[Target Point][P{idx}]  : ({boxPoint[idx].X},  {boxPoint[idx].Y})");
            //}

            //for (int idx = 0; idx < cornerPoint.Length; idx += 1)
            //{
            //    Console.WriteLine($"[Corner Point][P{idx}]  : ({cornerPoint[idx].X},  {cornerPoint[idx].Y})");
            //}

        }
        #endregion

        #region --- Adjust_Pitch ---
        private void Adjust_Pitch(ref Point[] cornerPoint, ref Point[] boxPoint, int P_idx, int Tolerance)
        {
            if (this.Update_RobotPosInfo())
            {   // 轉180度，Pitch - : 左下，右上

                int shiftY = cornerPoint[P_idx].Y - boxPoint[P_idx].Y; //右上角位置
                double Pitch = 0.0;

                //Console.WriteLine($"[Pitch Info ] shiftY : {shiftY}");
                //Console.WriteLine($"[Pitch Info] P_idx : {P_idx}");

                if (Math.Abs(shiftY) >= Tolerance)
                {
                    Console.WriteLine($"[Adjust_Pitch] ============================");

                    double rad = 0.01 * this.JogMultiply * this.MoveStep * _ThetaToRad;  // Theta To Radian
                    Console.WriteLine($"[Adjust_Pitch] rad : {rad}");

                    // 注意 : 目前環境設定 Roll 與 Pitch 顛倒
                    Pitch = urRobotPos.Roll;

                    if (shiftY < 0)
                    {
                        urRobotPos.Roll = this.isNormalAngle_0D ? (urRobotPos.Roll - rad) : (urRobotPos.Roll - rad);
                        if (this.isNormalAngle_0D)  //20240117 Rick modify
                            Console.WriteLine($"[Adjust_Pitch] Pitch : {Pitch} --> {urRobotPos.Roll} ==> Sub Pitch");
                        else  //20240118 Rick modify
                            Console.WriteLine($"[Adjust_Pitch] Pitch : {Pitch} --> {urRobotPos.Roll} ==> Sub Pitch");
                    }
                    else
                    {
                        urRobotPos.Roll = this.isNormalAngle_0D ? (urRobotPos.Roll + rad) : (urRobotPos.Roll + rad);
                        if (this.isNormalAngle_0D)  //20240117 Rick modify
                            Console.WriteLine($"[Adjust_Pitch] Pitch : {Pitch} --> {urRobotPos.Roll} ==> Add Pitch");
                        else  //20240118 Rick modify
                            Console.WriteLine($"[Adjust_Pitch] Pitch : {Pitch} --> {urRobotPos.Roll} ==> Add Pitch");
                    }

                    // Move Robot
                    this.MoveToPos();
                }

            }
        }
        #endregion --- Adjust_Pitch ---

        #region --- Adjust_Roll ---
        private void Adjust_Roll(ref Point[] cornerPoint, ref Point[] boxPoint, int P_idx, int Tolerance)
        {
            if (this.Update_RobotPosInfo())
            {   // 轉180度，Roll - : 上往寬，下往窄

                int shiftX = cornerPoint[P_idx].X - boxPoint[P_idx].X;
                double Roll = 0.0;

                if (Math.Abs(shiftX) > Tolerance)
                {
                    Console.WriteLine($"[Adjust_Roll] ============================");

                    double rad = 0.01 * this.JogMultiply * this.MoveStep * _ThetaToRad;  // Theta To Radian

                    // 注意 : 目前環境設定 Roll 與 Pitch 顛倒
                    Roll = urRobotPos.Pitch;

                    switch (P_idx)
                    {
                        case (int)Enum_Corner.Right_Bottom:
                            //Roll - : X 往左變小、Roll + : X 往右變大
                            Console.WriteLine($"[Adjust_Roll] Right_Bottom");

                            if (shiftX < 0)
                            {
                                urRobotPos.Pitch = this.isNormalAngle_0D ? (urRobotPos.Pitch - rad) : (urRobotPos.Pitch + rad);
                                if (this.isNormalAngle_0D)  //20240117 Rick modify
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Sub Roll");
                                else
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Add Roll");
                            }
                            else
                            {
                                urRobotPos.Pitch = this.isNormalAngle_0D ? (urRobotPos.Pitch + rad) : (urRobotPos.Pitch - rad);
                                if (this.isNormalAngle_0D)  //20240117 Rick modify
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Add Roll");
                                else
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Sub Roll");
                            }

                            break;

                        case (int)Enum_Corner.Left_Bottom:
                            //Roll - : X 往右變大、Roll + : X 往左變小
                            Console.WriteLine($"[Adjust_Roll] Left_Bottom");

                            if (shiftX < 0)
                            {   //20240117 Rick modify
                                urRobotPos.Pitch = this.isNormalAngle_0D ? (urRobotPos.Pitch + rad) : (urRobotPos.Pitch - rad);
                                if (this.isNormalAngle_0D)
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Add Roll");
                                else
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Sub Roll");
                            }
                            else
                            {  //20240117 Rick modify
                                urRobotPos.Pitch = this.isNormalAngle_0D ? (urRobotPos.Pitch - rad) : (urRobotPos.Pitch + rad);
                                if (this.isNormalAngle_0D)
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Sub Roll");
                                else
                                    Console.WriteLine($"[Adjust_Roll] Roll : {Roll} --> {urRobotPos.Pitch} ==> Add Roll");
                            }
                            break;
                    }

                    // Move Robot
                    this.MoveToPos();
                }

            }
        }
        #endregion --- Adjust_Roll ---

        #region --- Adjust_Yaw ---
        private void Adjust_Yaw(ref Point[] cornerPoint, ref Point[] boxPoint, int P_idx, int Tolerance)
        {
            if (this.Update_RobotPosInfo())
            {   // 轉180度，Yaw - :  Y 往下 (右往寬，左往窄)、Yaw + :  Y 往上   

                int shiftY = cornerPoint[P_idx].Y - boxPoint[P_idx].Y;
                double Yaw = 0.0;

                if (Math.Abs(shiftY) > Tolerance)
                {
                    Console.WriteLine($"[Adjust_Yaw] ============================");

                    double rad = 0.01 * this.JogMultiply * this.MoveStep * _ThetaToRad;  // Theta To Radian

                    Yaw = urRobotPos.Yaw;

                    switch (P_idx)
                    {
                        case (int)Enum_Corner.Right_Bottom:

                            Console.WriteLine($"[Adjust_Yaw] Right_Bottom");

                            if (shiftY < 0)
                            {
                                urRobotPos.Yaw = this.isNormalAngle_0D ? (urRobotPos.Yaw + rad) : (urRobotPos.Yaw - rad);

                                if (this.isNormalAngle_0D)  //20240117 Rick modify
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Add Yaw");
                                else
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Sub Yaw");
                            }
                            else
                            {
                                urRobotPos.Yaw = this.isNormalAngle_0D ? (urRobotPos.Yaw - rad) : (urRobotPos.Yaw + rad);

                                if (this.isNormalAngle_0D)
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Sub Yaw");
                                else
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Add Yaw");
                            }

                            break;

                        case (int)Enum_Corner.Left_Bottom:

                            Console.WriteLine($"[Adjust_Yaw] Left_Bottom");

                            if (shiftY < 0)
                            {
                                //20240117 Rick modify
                                urRobotPos.Yaw = this.isNormalAngle_0D ? (urRobotPos.Yaw + rad) : (urRobotPos.Yaw - rad);
                                if (this.isNormalAngle_0D)
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Add Yaw");
                                else
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Sub Yaw");
                            }
                            else
                            {
                                urRobotPos.Yaw = this.isNormalAngle_0D ? (urRobotPos.Yaw - rad) : (urRobotPos.Yaw + rad);
                                //20240117 Rick modify
                                if (this.isNormalAngle_0D)
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Sub Yaw");
                                else
                                    Console.WriteLine($"[Adjust_Yaw] Yaw : {Yaw} --> {urRobotPos.Yaw} ==> Add Yaw");
                            }

                            break;
                    }

                    // Move Robot
                    this.MoveToPos();
                }

            }
        }
        #endregion --- Adjust_Yaw ---

        #region --- AutoPlaneAlign_Proc ---
        // _Para[0] : Move Step Index (0 : slow, 1 : medium, 2 : fast)
        // _Para[1] : Move Tolerance
        // _Para[2] : Move Acc
        // _Para[3] : Move Speed
        // _Para[4] : Jog Multiply
        // _Para[5] : ImgProc Threshold
        private void AutoPlaneAlign_Proc(AutoPlane_FlowPara InputPara)
        {
            AutoPlane_FlowPara Para = InputPara;

            int moveStepIndex = Para.MoveStepIdx;

            this.MoveStep = this.MoveStepMutiply[moveStepIndex];
            this.deviation_Tolerance = Para.Deviation_Tolerance;
            this.MoveAcc = Para.MoveAcc;
            this.MoveSpeed = Para.MoveSpeed;
            this.JogMultiply = Para.JogMultiply;
            this.ImgProc_Threshold = Para.ImgProc_Threshold;

            // Temp
            bool isGetBox = false;
            bool Capture_OK = false;
            Enum_Corner CornerPoint = Enum_Corner.Right_Top;
            int P_idx = 0;
            int Target_shiftY = 0;
            int Target_shift = 0;
            int shiftY = 0;
            int shift = 0;
            int ReGrab_Cnt = 3;
            int Pitch_shiftY = 0;
            double deviation = 0;
            int Time_Sleep = 3000;
            Enum_Deviation_Direction deviateDir = Enum_Deviation_Direction.X3;

            TimeManager TM_TimeOut = new TimeManager();
            TM_TimeOut.SetDelay(6000000);  //單位 : ms， TimeOut : 100分鐘

            if (this.Source_Points == null)
                this.Source_Points = new Point[4];

            this.Proc_Step = AutoPlaneAlign_Step.GetOriSourceCornerPoints_And_TargetBoxPoints;

            try
            {
                while (this.FlowRun) 
                {
                    if (TM_TimeOut.IsTimeOut())
                    {
                        this.Proc_Step = AutoPlaneAlign_Step.TimeOut;
                    }

                    switch (this.Proc_Step)
                    {
                        case AutoPlaneAlign_Step.GetOriSourceCornerPoints_And_TargetBoxPoints:
                            {
                                //Grab Image
                                //this.grabber.Grab();

                                //Initial Source_Points & Target_Points
                                isGetBox = true;
                                Capture_OK = this.CalculateCornerPoints_And_BoxPoints(this.grabber.grabImage, isGetBox, this.ImgProc_Threshold, ref this.Source_Points, ref this.Target_Points);

                                if (Capture_OK)
                                    this.Proc_Step = AutoPlaneAlign_Step.Align_LeftTop_Point_And_P2P3_X;
                                else
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.GetOriSourceCornerPoints_And_TargetBoxPoints;
                                    ReGrab_Cnt = ReGrab_Cnt - 1;
                                }

                                if (ReGrab_Cnt == -1)
                                    this.Proc_Step = AutoPlaneAlign_Step.Error;
                            }
                            break;

                        case AutoPlaneAlign_Step.Align_LeftTop_Point_And_P2P3_X:
                            {
                                this.Align_LeftTop_Point_And_P2P3_X(ref this.Source_Points, ref this.Target_Points);

                                P_idx = (int)Enum_Corner.Right_Top;
                                shiftY = this.Source_Points[P_idx].Y - this.Target_Points[P_idx].Y;
                                Target_shiftY = (int)(0.5 * Math.Abs(shiftY));
                                Console.WriteLine($"[Adjust_Pitch] Target shiftY : {Target_shiftY}");

                                this.Proc_Step = AutoPlaneAlign_Step.Adjust_Pitch;
                            }
                            break;

                        case AutoPlaneAlign_Step.Adjust_Pitch:
                            {
                                this.Adjust_Pitch(ref this.Source_Points, ref this.Target_Points, (int)Enum_Corner.Right_Top, deviation_Tolerance);

                                System.Threading.Thread.Sleep(Time_Sleep);

                                isGetBox = false;
                                this.CalculateCornerPoints_And_BoxPoints(this.grabber.grabImage, isGetBox, this.ImgProc_Threshold, ref this.Source_Points, ref this.Target_Points);

                                this.Align_LeftTop_Point_And_P2P3_X(ref this.Source_Points, ref this.Target_Points);

                                Pitch_shiftY = Math.Abs(this.Source_Points[0].Y - this.Source_Points[1].Y);
                                Console.WriteLine($"[Pitch_shiftY] Deviation Y2 (After) : {Pitch_shiftY}");

                                if (Pitch_shiftY <= deviation_Tolerance)
                                {
                                    if (this.Check_AutoPlaneAlign_Finish(deviation_Tolerance))
                                        this.Proc_Step = AutoPlaneAlign_Step.Finish;
                                    else
                                        this.Proc_Step = AutoPlaneAlign_Step.Check_MaxDeviation_Direction;
                                }
                                else
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Pitch;
                                }
                            }
                            break;

                        case AutoPlaneAlign_Step.Check_MaxDeviation_Direction:
                            {
                                this.Check_MaxDeviation_Direction(ref deviateDir, ref shift, ref CornerPoint, deviation_Tolerance);
                                P_idx = (int)CornerPoint;
                                Target_shift = (int)(0.84 * Math.Abs(shift));

                                if (deviateDir == Enum_Deviation_Direction.X3 || deviateDir == Enum_Deviation_Direction.X4)
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Roll;
                                }
                                else if (deviateDir == Enum_Deviation_Direction.Y3 || deviateDir == Enum_Deviation_Direction.Y4)
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Yaw;
                                }
                                else if (deviateDir == Enum_Deviation_Direction.P3P4_OK)
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Pitch;
                                }
                            }
                            break;

                        case AutoPlaneAlign_Step.Adjust_Roll:
                            {
                                this.Adjust_Roll(ref this.Source_Points, ref this.Target_Points, P_idx, deviation_Tolerance);

                                System.Threading.Thread.Sleep(Time_Sleep);

                                isGetBox = false;
                                this.CalculateCornerPoints_And_BoxPoints(this.grabber.grabImage, isGetBox, this.ImgProc_Threshold, ref this.Source_Points, ref this.Target_Points);

                                deviation = Math.Abs(this.Source_Points[P_idx].X - this.Target_Points[P_idx].X);

                                this.Align_LeftTop_Point_And_P2P3_X(ref this.Source_Points, ref this.Target_Points);

                                Pitch_shiftY = Math.Abs(this.Source_Points[0].Y - this.Source_Points[1].Y);
                                Console.WriteLine($"[Pitch_shiftY] Deviation Y : {Pitch_shiftY}");
                                Console.WriteLine($"[Roll Info] Right_Bottom X Diff : {this.Source_Points[2].X - this.Target_Points[2].X}");
                                Console.WriteLine($"[Roll Info] Left_Bottom X Diff : {this.Source_Points[3].X - this.Target_Points[3].X}");

                                if (Pitch_shiftY <= deviation_Tolerance)
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Check_MaxDeviation_Direction;
                                }
                                else
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Pitch;
                                    Console.WriteLine($"[Re-Adjust_Pitch]");
                                }
                            }
                            break;

                        case AutoPlaneAlign_Step.Adjust_Yaw:
                            {
                                this.Adjust_Yaw(ref this.Source_Points, ref this.Target_Points, P_idx, deviation_Tolerance);

                                System.Threading.Thread.Sleep(Time_Sleep);

                                isGetBox = false;
                                this.CalculateCornerPoints_And_BoxPoints(this.grabber.grabImage, isGetBox, this.ImgProc_Threshold, ref this.Source_Points, ref this.Target_Points);

                                deviation = Math.Abs(this.Source_Points[P_idx].Y - this.Target_Points[P_idx].Y);

                                this.Align_LeftTop_Point_And_P2P3_X(ref this.Source_Points, ref this.Target_Points);

                                Pitch_shiftY = Math.Abs(this.Source_Points[0].Y - this.Source_Points[1].Y);
                                Console.WriteLine($"[Pitch_shiftY] Deviation Y : {Pitch_shiftY}");
                                Console.WriteLine($"[Roll Info] Right_Bottom Y Diff : {this.Source_Points[2].Y - this.Target_Points[2].Y}");
                                Console.WriteLine($"[Roll Info] Left_Bottom Y Diff : {this.Source_Points[3].Y - this.Target_Points[3].Y}");

                                if (Pitch_shiftY <= deviation_Tolerance)
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Check_MaxDeviation_Direction;
                                }
                                else
                                {
                                    this.Proc_Step = AutoPlaneAlign_Step.Adjust_Pitch;
                                    Console.WriteLine($"[Re-Adjust_Pitch]");
                                }
                            }
                            break;

                        case AutoPlaneAlign_Step.Finish:
                            {
                                //Console.WriteLine($"deviation_Tolerance : {deviation_Tolerance}");

                                for (int i = 0; i < this.Source_Points.Length; i++)
                                {
                                    Console.WriteLine($"P{i} DiffX : {Math.Abs(this.Source_Points[i].X - this.Target_Points[i].X)}");
                                    Console.WriteLine($"P{i} DiffY : {Math.Abs(this.Source_Points[i].Y - this.Target_Points[i].Y)}");
                                    Console.WriteLine($"[Target] P{i} (X, Y) : ({this.Target_Points[i].X}, {this.Target_Points[i].Y})");
                                    Console.WriteLine($"[Source] P{i} (X, Y) : ({this.Source_Points[i].X}, {this.Source_Points[i].Y})");
                                }

                                this.FlowRun = false;
                                this.info.General("Auto Plane Align Finish !");
                            }
                            break;

                        case AutoPlaneAlign_Step.Error:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Plane Align  Caption Error !");
                            }
                            break;

                        case AutoPlaneAlign_Step.Stop:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Plane Align  Stoped!");
                            }
                            break;

                        case AutoPlaneAlign_Step.TimeOut:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Plane Align  TimeOut !");
                            }
                            break;
                    }
                } 
            }
            catch 
            {
                this.FlowRun = false;
                this.Proc_Step = AutoPlaneAlign_Step.Error;
            }
            finally
            {
                this.FlowRun = false;
                this.Proc_Step = AutoPlaneAlign_Step.Error;
            }

        }
        #endregion --- AutoPlaneAlign_Proc ---

        #endregion --- 方法函式 ---"



    }

    public class AutoPlane_FlowPara
    {
        [Category("AutoPlane Para"), DisplayName("01. Move Step Idx"), Description("Move Step Idx")]
        public int MoveStepIdx { get; set; } = 0;

        [Category("AutoPlane Para"), DisplayName("02. Deviation Tolerance"), Description("Deviation Tolerance")]
        public int Deviation_Tolerance { get; set; } = 0;

        [Category("AutoPlane Para"), DisplayName("03. Move Acc"), Description("Move Acc")]
        public double MoveAcc { get; set; } = 0.0;

        [Category("AutoPlane Para"), DisplayName("04. Move Speed"), Description("Move Speed")]
        public double MoveSpeed { get; set; } = 0.0;

        [Category("AutoPlane Para"), DisplayName("05. Move Multiply"), Description("Move Multiply")]
        public double JogMultiply { get; set; } = 0.0;

        [Category("AutoPlane Para"), DisplayName("06. Move Threshold"), Description("Move Threshold")]
        public int ImgProc_Threshold { get; set; } = 0;

        public void Clone(AutoPlane_FlowPara Source)
        {
            this.MoveStepIdx = Source.MoveStepIdx;
            this.Deviation_Tolerance = Source.Deviation_Tolerance;
            this.MoveAcc = Source.MoveAcc;
            this.MoveSpeed = Source.MoveSpeed;
            this.JogMultiply = Source.JogMultiply;
            this.ImgProc_Threshold = Source.ImgProc_Threshold;
        }

        public override string ToString()
        {
            return "...";
        }
    }

}
