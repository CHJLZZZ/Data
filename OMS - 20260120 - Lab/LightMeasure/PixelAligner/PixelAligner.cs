using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;

using BaseTool;

namespace LightMeasure
{
    public class PixelAligner
    {
        private MIL_ID milSystem;
        private double CenterX = 0;
        private double CenterY = 0;

        private PixelAligner()
        {
            this.milSystem = MIL.M_NULL;
        }

        public PixelAligner(MIL_ID milSys)
            : this()
        {
            if(milSys == MIL.M_NULL)
            {
                throw new Exception("milSys is M_NULL.");
            }

            this.milSystem = milSys;
        }

        #region --- Local Fundtion ---

        #region --- CoarseAlign ---
        private void CoarseAlign(
            MIL_ID image,
            CoarseAlignSetting setting,
            string outPath,
            ref MIL_ID maskImage)
        {
            int threshold = setting.Threshold;
            int areaMin = setting.AreaMin;
            int closeNums = setting.CloseNums;
            bool debugImage = setting.DebugImage;

            MIL_ID binImage = MIL.M_NULL;
            MIL_ID blobContext = MIL.M_NULL;
            MIL_ID blobResult = MIL.M_NULL;

            double[] startX = null;
            double[] startY = null;
            double[] endX = null;
            double[] endY = null;
            double[] area = null;

            try
            {
                // Check
                if (threshold <= 0)
                {
                    throw new Exception("threshold <= 0.");
                }

                if (areaMin < 0)
                {
                    throw new Exception("areaMin < 0.");
                }

                MIL_INT sizeX = MIL.MbufInquire(image, MIL.M_SIZE_X);
                MIL_INT sizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC, ref binImage);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC, ref maskImage);
                MIL.MbufClear(binImage, 0L);
                MIL.MbufClear(maskImage, 0L);

                MIL.MblobAlloc(this.milSystem, MIL.M_DEFAULT,
                    MIL.M_DEFAULT, ref blobContext);
                MIL.MblobAllocResult(this.milSystem, MIL.M_DEFAULT,
                    MIL.M_DEFAULT, ref blobResult);

                // Control Block for Blob Context
                MIL.MblobControl(blobContext, MIL.M_BOX, MIL.M_ENABLE);

                // Feature Sort
                MIL.MblobControl(blobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(blobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

                MIL.MimBinarize(image, binImage, MIL.M_FIXED + MIL.M_GREATER,
                    (double)threshold, MIL.M_NULL);
                if (debugImage)
                {
                    MIL.MbufSave(
                        string.Format(@"{0}\binImage.tif", outPath),
                        binImage);
                }

                if (closeNums > 0)
                {
                    MIL.MimClose(binImage, binImage, closeNums, MIL.M_GRAYSCALE);
                }
                if (debugImage)
                {
                    MIL.MbufSave(
                        string.Format(@"{0}\binImageDilate_{1}.tif", outPath, closeNums),
                        binImage);
                }

                MIL.MblobCalculate(blobContext, binImage, MIL.M_NULL, blobResult);
                MIL.MblobSelect(blobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS,
                    (double)areaMin, MIL.M_NULL);

                // Get the total number of selected blobs.
                MIL_INT totalBlobs = 0;
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT,
                    MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalBlobs);

                if (totalBlobs == 0)
                {
                    throw new Exception("Blob = 0");
                }

                startX = new double[totalBlobs];
                startY = new double[totalBlobs];
                endX = new double[totalBlobs];
                endY = new double[totalBlobs];
                area = new double[totalBlobs];

                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MIN, startX);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MIN, startY);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MAX, endX);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MAX, endY);

                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_BOX_AREA, area);

                // TODO: 分裂時處理?

                MIL.MgraRectFill(MIL.M_DEFAULT, maskImage,
                    startX[0], startY[0], endX[0], endY[0]);

                if (debugImage)
                {
                    MIL.MbufSave(
                        string.Format(@"{0}\maskImage.tif", outPath),
                        maskImage);
                }
            }
            catch (Exception ex)
            {
                if (maskImage != MIL.M_NULL)
                {
                    MIL.MbufFree(maskImage);
                    maskImage = MIL.M_NULL;
                }

                throw new Exception(
                    string.Format(
                        "[CoarseAlign] {0}",
                        ex.Message));
            }
            finally
            {
                if (blobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(blobResult);
                    blobResult = MIL.M_NULL;
                }

                if (blobContext != MIL.M_NULL)
                {
                    MIL.MblobFree(blobContext);
                    blobContext = MIL.M_NULL;
                }

                if (binImage != MIL.M_NULL)
                {
                    MIL.MbufFree(binImage);
                    binImage = MIL.M_NULL;
                }
            }
        }
        #endregion             

        #region --- GetLedPos_And_SearchStartPos_ ---
        private List<BlobData> GetLedPos_And_SearchStartPos(MIL_ID srcImage,
            PixelAlignSetting setting)
        {
            MIL_ID milContext = MIL.M_NULL;
            MIL_ID milBlobContext = MIL.M_NULL;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;
            MIL_ID DrawImg = MIL.M_NULL;

            MIL_INT totalBlobs = 0;
            MIL_INT sizeX = MIL.MbufInquire(srcImage, MIL.M_SIZE_X);
            MIL_INT sizeY = MIL.MbufInquire(srcImage, MIL.M_SIZE_Y);

            EnumRepairType repairType = setting.FindPanelPixelSetting.RepairType;
            EnumBinaryMethod binarMethod = setting.FindPanelPixelSetting.BinaryMethod;

            double[] blob_MinX = null;
            double[] blob_MaxX = null;
            double[] blob_MinY = null;
            double[] blob_MaxY = null;

            Rectangle TopRoi = new Rectangle();
            Rectangle LeftRoi = new Rectangle();
            Rectangle RightRoi = new Rectangle();

            int Stx = 0;
            int Sty = 0;
            int w1 = 400;
            int h1 = 200;
            int w2 = 200;
            int h2 = 500;

            bool debugImage = setting.FindPanelPixelSetting.DebugImage;

            try
            {
                // [1] Get Processed Image
                #region --- Get Processed Image ---
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0L);

                MIL.MimAlloc(this.milSystem, MIL.M_BINARIZE_ADAPTIVE_CONTEXT,
                    MIL.M_DEFAULT, ref milContext);

                /* Perform binarization. */
                if (binarMethod == EnumBinaryMethod.AdaptBin)
                {
                    // Adaptive Setting
                    MIL.MimControl(milContext, MIL.M_THRESHOLD_MODE, MIL.M_LOCAL_MEAN);
                    MIL.MimControl(milContext, MIL.M_FOREGROUND_VALUE, MIL.M_FOREGROUND_WHITE);
                    MIL.MimControl(milContext, MIL.M_LOCAL_DIMENSION, (int)setting.FindPanelPixelSetting.ColFindPitchX * 2);
                    MIL.MimControl(milContext, MIL.M_GLOBAL_OFFSET, (int)setting.FindPanelPixelSetting.Threshold);

                    MIL.MimBinarizeAdaptive(milContext, srcImage,
                       MIL.M_NULL, MIL.M_NULL, outImage, MIL.M_NULL, MIL.M_DEFAULT);
                }
                else if (binarMethod == EnumBinaryMethod.SimpleBin)
                {
                    MIL_INT BinValue = MIL.MimBinarize(srcImage, MIL.M_NULL, MIL.M_BIMODAL, MIL.M_NULL, MIL.M_NULL);
                    Console.WriteLine("[GetLedPos_And_SearchStartPos] Auto Binary Value : " + BinValue);

                    MIL.MimBinarize(srcImage, outImage, MIL.M_GREATER_OR_EQUAL, setting.FindPanelPixelSetting.Threshold, MIL.M_NULL);

                }

                //Open
                if (setting.FindPanelPixelSetting.BlobOpen)
                    MIL.MimOpen(outImage, outImage, 1, MIL.M_BINARY);

                if (debugImage)
                {
                    MIL.MbufSave(string.Format(@"{0}\[1]Img_AdaptBinBlob.tif", setting.outPath), outImage);
                }

                #endregion

                // [2] Get Led Position
                #region --- Get Led Position ---

                //--- Blob Calculate Context ---
                MIL.MblobAlloc(this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobContext);
                if (milBlobContext == MIL.M_NULL)
                    throw new Exception("milBlobContext == M_NULL");

                MIL.MblobControl(milBlobContext, MIL.M_ALL_FEATURES, MIL.M_DISABLE);
                MIL.MblobControl(milBlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_BINARY, MIL.M_ENABLE);
                MIL.MblobControl(milBlobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(milBlobContext, MIL.M_FERET_AT_PRINCIPAL_AXIS_ANGLE + MIL.M_BINARY, MIL.M_ENABLE);

                //--- Blob Result ---
                MIL.MblobAllocResult(this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobResult);
                MIL.MblobControl(milBlobResult, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);    //Set it as binarize searching(1 bit data)
                if (milBlobResult == MIL.M_NULL)
                    throw new Exception("milBlobResult == M_NULL");

                //--- Blob Calculate ---
                MIL.MblobCalculate(milBlobContext, outImage, MIL.M_NULL, milBlobResult);

                // Exclude blobs whose area is too small.
                //MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, setting.FindPanelPixelSetting.AreaMin, MIL.M_NULL);

                MIL_INT TotalBlobs = 0;
                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TotalBlobs);

                List<BlobData> blobList = new List<BlobData>();

                double[] CogXs = new double[TotalBlobs];
                double[] CogYs = new double[TotalBlobs];
                double[] CogArea = new double[TotalBlobs];
                double[] CogAngle = new double[TotalBlobs];
                blobList = new List<BlobData>();

                // Get the results.
                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, CogXs);
                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, CogYs);
                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_AREA + MIL.M_BINARY, CogArea);
                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_FERET_AT_PRINCIPAL_AXIS_ANGLE + MIL.M_BINARY, CogAngle);

                double sDis = setting.FindPanelPixelSetting.SeperateDistance;

                //Parallel.For(0, (int)TotalBlobs, i =>
                //{
                switch (repairType)
                {
                    case EnumRepairType.None:
                        for (int i = 0; i < TotalBlobs; i++)
                        {
                            blobList.Add(new BlobData { CogX = CogXs[i], CogY = CogYs[i], CogArea = CogArea[i] });
                        }
                        break;

                    case EnumRepairType.Top:  // Fo G Pattern
                        for (int i = 0; i < TotalBlobs; i++)
                        {
                            if (CogArea[i] > setting.FindPanelPixelSetting.AreaCutMax)
                            {
                                blobList.Add(new BlobData { CogX = CogXs[i], CogY = CogYs[i] + sDis, CogArea = CogArea[i] / 2 });

                                blobList.Add(new BlobData { CogX = CogXs[i], CogY = CogYs[i] - sDis, CogArea = CogArea[i] / 2 });
                            }
                            else
                            {
                                blobList.Add(new BlobData { CogX = CogXs[i], CogY = CogYs[i], CogArea = CogArea[i] });
                            }
                        }
                        break;

                    case EnumRepairType.RightTop_LeftTop:  // For R Pattern
                        for (int i = 0; i < TotalBlobs; i++)
                        {
                            if (CogArea[i] > setting.FindPanelPixelSetting.AreaCutMax)
                            {
                                if (CogArea[i] > setting.FindPanelPixelSetting.AreaCutMax && CogAngle[i] < 90)
                                {
                                    blobList.Add(new BlobData { CogX = CogXs[i] + sDis, CogY = CogYs[i] - sDis, CogArea = CogArea[i] / 2 });

                                    blobList.Add(new BlobData { CogX = CogXs[i] - sDis, CogY = CogYs[i] + sDis, CogArea = CogArea[i] / 2 });
                                }
                                else if (CogArea[i] > setting.FindPanelPixelSetting.AreaCutMax && CogAngle[i] > 90)
                                {
                                    blobList.Add(new BlobData { CogX = CogXs[i] - sDis, CogY = CogYs[i] - sDis, CogArea = CogArea[i] / 2 });

                                    blobList.Add(new BlobData { CogX = CogXs[i] + sDis, CogY = CogYs[i] + sDis, CogArea = CogArea[i] / 2 });
                                }
                            }
                            else
                            {
                                blobList.Add(new BlobData { CogX = CogXs[i], CogY = CogYs[i], CogArea = CogArea[i] });
                            }
                        }
                        break;
                }

                #endregion

                // [3] Get ROI
                #region --- Get ROI ---
                if (setting.FindPanelPixelSetting.FirstPixelX == 0 && setting.FindPanelPixelSetting.FirstPixelY == 0)
                {
                    int closeNums = setting.CoarseAlignSetting.CloseNums;

                    // Full Panel
                    if (closeNums > 0)
                    {
                        MIL.MimClose(outImage, outImage, closeNums, MIL.M_GRAYSCALE);
                    }

                    if (debugImage)
                    {
                        MIL.MbufSave(string.Format(@"{0}\[3]Img_FindEdge.tif", setting.outPath), outImage);
                    }

                    MIL.MblobControl(milBlobContext, MIL.M_ALL_FEATURES, MIL.M_DISABLE);
                    MIL.MblobControl(milBlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_BINARY, MIL.M_ENABLE);
                    MIL.MblobControl(milBlobContext, MIL.M_BOX, MIL.M_ENABLE);

                    MIL.MblobControl(milBlobContext, MIL.M_SORT1, MIL.M_BOX_AREA);
                    MIL.MblobControl(milBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

                    //--- Blob Analysis ---
                    MIL.MblobCalculate(milBlobContext, outImage, MIL.M_NULL, milBlobResult);

                    MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT,
                       MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalBlobs);

                    if (totalBlobs > 0)
                    {
                        blob_MinX = new double[totalBlobs];
                        blob_MinY = new double[totalBlobs];
                        blob_MaxX = new double[totalBlobs];
                        blob_MaxY = new double[totalBlobs];

                        MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MIN, blob_MinX);
                        MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MIN, blob_MinY);
                        MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MAX, blob_MaxX);
                        MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MAX, blob_MaxY);

                        //更新搜尋 Panel Pixel 左上角與右下角點
                        setting.FindPanelPixelSetting.FirstPixelX = (int)blob_MinX[0];
                        setting.FindPanelPixelSetting.FirstPixelY = (int)blob_MinY[0];
                        setting.FindPanelPixelSetting.EndPixelX = (int)blob_MaxX[0];
                        setting.FindPanelPixelSetting.EndPixelY = (int)blob_MaxY[0];

                    }
                    else
                    {
                        throw new Exception(string.Format("[GetLedPos_And_SearchStartPos_] {0}", @"Can not find blob !"));
                    }

                    if (debugImage)
                    {
                        MIL.MbufSave(string.Format(@"{0}\[2]Img_CoarseBlob.tif", setting.outPath), outImage);
                    }
                }
                else
                {
                    int closeNums = setting.CoarseAlignSetting.CloseNums;
                    // Full Panel
                    if (closeNums > 0)
                    {
                        MIL.MimClose(outImage, outImage, closeNums, MIL.M_GRAYSCALE);
                    }
                }
                #endregion

                // [4] Search Edge (上、兩側)
                #region --- Search Edge ---
                int MiddleXLine = (int)((setting.FindPanelPixelSetting.FirstPixelX + setting.FindPanelPixelSetting.EndPixelX) * 0.5);
                int MiddleYLine = (int)((setting.FindPanelPixelSetting.FirstPixelY + setting.FindPanelPixelSetting.EndPixelY) * 0.5);
                int SubEdge = 5;

                Stx = (int)(MiddleXLine - 0.5 * w1);
                Sty = (int)(setting.FindPanelPixelSetting.FirstPixelY - 0.5 * h1);
                if (Stx <= 0) Stx = 0;
                if (Sty <= 0) Sty = 0;
                TopRoi = new Rectangle(Stx, Sty, w1, h1);

                Stx = (int)(setting.FindPanelPixelSetting.FirstPixelX - 0.5 * w2);
                Sty = (int)(MiddleYLine - 0.5 * h2);
                if (Stx <= 0) Stx = 0;
                if (Sty <= 0) Sty = 0;
                LeftRoi = new Rectangle(Stx, Sty, w2, h2);

                Stx = (int)(setting.FindPanelPixelSetting.EndPixelX - 0.5 * w2);
                Sty = (int)(MiddleYLine - 0.5 * h2);
                if (Stx <= 0) Stx = 0;
                if (Sty <= 0) Sty = 0;
                if ((Stx + w2) > sizeX)
                    Stx = (int)(sizeX - w2 - 1);
                RightRoi = new Rectangle(Stx, Sty, w2, h2);

                #region --- Search Edge (上、兩側) ---

                Point[] TopEdgeP = CalculateEdge(outImage, TopRoi, SubEdge, 1, 2);
                TopEdgeP = TopEdgeP.Except(new Point[] { new Point(0, 0) }).ToArray();
                int TopSide = TopEdgeP.Min(t => t.Y);

                Point[] LeftEdgeP = CalculateEdge(outImage, LeftRoi, SubEdge, 2, 1);
                LeftEdgeP = LeftEdgeP.Except(new Point[] { new Point(0, 0) }).ToArray();
                int LeftSide = LeftEdgeP.Min(t => t.X);

                Point[] RightEdgeP = CalculateEdge(outImage, RightRoi, SubEdge, 2, 2);
                RightEdgeP = RightEdgeP.Except(new Point[] { new Point(0, 0) }).ToArray();
                int RightSide = RightEdgeP.Max(t => t.X);

                int CircleR = RightSide - LeftSide;
                int MiddleLine = (RightSide + LeftSide) / 2;

                //更新搜尋 Panel Pixel 左上角與右下角點
                setting.FindPanelPixelSetting.FirstPixelX = LeftSide;
                setting.FindPanelPixelSetting.FirstPixelY = TopSide;
                setting.FindPanelPixelSetting.EndPixelX = RightSide;

                #endregion

                #endregion

                // Save 
                if (debugImage)
                {
                    MIL.MbufAllocColor(this.milSystem, 3, sizeX, sizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref DrawImg);
                    MIL.MbufCopy(srcImage, DrawImg);

                    MIL.MimArith(srcImage, 16, DrawImg, MIL.M_DIV_CONST);

                    if (!Directory.Exists(setting.outPath))
                    {
                        Directory.CreateDirectory(setting.outPath);
                    }

                    //Roi
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    MIL.MgraRect(MIL.M_DEFAULT, DrawImg,
                        TopRoi.X,
                        TopRoi.Y,
                        TopRoi.Width + TopRoi.X,
                        TopRoi.Height + TopRoi.Y
                        );
                    MIL.MgraRect(MIL.M_DEFAULT, DrawImg,
                       LeftRoi.X,
                       LeftRoi.Y,
                       LeftRoi.Width + LeftRoi.X,
                       LeftRoi.Height + LeftRoi.Y
                       );
                    MIL.MgraRect(MIL.M_DEFAULT, DrawImg,
                       RightRoi.X,
                       RightRoi.Y,
                       RightRoi.Width + RightRoi.X,
                       RightRoi.Height + RightRoi.Y
                       );

                    //Point
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                    foreach (Point P in TopEdgeP)
                    {
                        MIL.MgraArcFill(MIL.M_DEFAULT, DrawImg, P.X, P.Y, 2, 2, 0, 360);
                    }
                    foreach (Point P in LeftEdgeP)
                    {
                        MIL.MgraArcFill(MIL.M_DEFAULT, DrawImg, P.X, P.Y, 2, 2, 0, 360);
                    }
                    foreach (Point P in RightEdgeP)
                    {
                        MIL.MgraArcFill(MIL.M_DEFAULT, DrawImg, P.X, P.Y, 2, 2, 0, 360);
                    }

                    //Line
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                    MIL.MgraLine(MIL.M_DEFAULT, DrawImg, LeftSide, TopSide, LeftSide, TopSide + CircleR);
                    MIL.MgraLine(MIL.M_DEFAULT, DrawImg, RightSide, TopSide, RightSide, TopSide + CircleR);
                    MIL.MgraLine(MIL.M_DEFAULT, DrawImg, LeftSide, TopSide, RightSide, TopSide);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MgraLine(MIL.M_DEFAULT, DrawImg, MiddleLine, TopSide, MiddleLine, TopSide + CircleR);

                    MIL.MbufSave(string.Format(@"{0}\[2]IMg_ColorDrawEdges.tif", setting.outPath), DrawImg);
                }

                return blobList;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[GetLedPos_And_SearchStartPos_] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilMimFree(ref milContext);

                MilNetHelper.MilBlobFree(ref milBlobContext);

                MilNetHelper.MilBlobFree(ref milBlobResult);

                MilNetHelper.MilBufferFree(ref outImage);

                MilNetHelper.MilBufferFree(ref DrawImg);

                // Free Blob
                if (blob_MinX != null) blob_MinX = null;
                if (blob_MaxX != null) blob_MaxX = null;
                if (blob_MinY != null) blob_MinY = null;
                if (blob_MaxY != null) blob_MaxY = null;
            }

        }

        #endregion

        #region --- SearchBaseLine ---
        private List<BlobData> SearchBaseLine(MIL_ID Img, List<BlobData> BlobDatas, int BaseLedCnt, PixelAlignSetting setting)
        {
            MIL_ID image_8U = MIL.M_NULL;
            MIL_ID image_Color = MIL.M_NULL;

            double RowPitchX = setting.FindPanelPixelSetting.RowFindPitchX;
            double RowPitchY = setting.FindPanelPixelSetting.RowFindPitchY;
            double ColPitchX = setting.FindPanelPixelSetting.ColFindPitchX;
            double ColPitchY = setting.FindPanelPixelSetting.ColFindPitchY;

            Point StartPos = new Point(
                (int)((setting.FindPanelPixelSetting.FirstPixelX + setting.FindPanelPixelSetting.EndPixelX) * 0.5),
                (int)setting.FindPanelPixelSetting.FirstPixelY);

            try
            {
                //防呆

                if (StartPos.X < 0)
                {
                    throw new Exception("FirstPixelX < 0.");
                }

                if (StartPos.Y < 0)
                {
                    throw new Exception("FirstPixelY < 0.");
                }


                //[1] Search First Point
                #region --- Search First Point ---

                var FirstblobPart = from b in BlobDatas
                                    where Math.Abs(StartPos.Y - b.CogY) < 0.6 * RowPitchY
                                    where Math.Abs(StartPos.X - b.CogX) < 0.6 * ColPitchX
                                    select b;

                List<BlobData> FirstblobPartList = FirstblobPart.ToList();

                BlobData FirstPoint = new BlobData { CogX = StartPos.X, CogY = StartPos.Y };
                FirstPoint = this.SearchPoint(FirstblobPartList, FirstPoint, 0, 0, 0.65 * ColPitchX);

                #endregion

                //[2] Calculate Base Line 
                #region --- Calculate Base Line ---
                List<BlobData> BaseLine = new List<BlobData>();
                BaseLine.Add(FirstPoint);

                //Search Base Line
                BlobData BasePoint = FirstPoint;

                for (int i = 1; i < BaseLedCnt; i++)
                {
                    var blobPart = from b in BlobDatas
                                   where Math.Abs(StartPos.X - b.CogX) < 0.6 * ColPitchX
                                   where Math.Abs(BasePoint.CogY + RowPitchY - b.CogY) < 0.6 * RowPitchY

                                   select b;

                    List<BlobData> blobPartList = blobPart.ToList();

                    BlobData SearchedPoint = this.SearchPoint(
                         blobPartList, BasePoint, RowPitchX, RowPitchY, 0.6 * ColPitchX);

                    BaseLine.Add(SearchedPoint);

                    // Next Base Point
                    BasePoint = new BlobData { CogX = FirstPoint.CogX, CogY = SearchedPoint.CogY };
                }
                #endregion

                //[3] Draw Base Line
                #region --- Draw Base Line ---

                if (setting.FindPanelPixelSetting.DebugImage)
                {
                    MIL_INT Width = MIL.MbufInquire(Img, MIL.M_SIZE_X);
                    MIL_INT Height = MIL.MbufInquire(Img, MIL.M_SIZE_Y);

                    MIL.MbufAlloc2d(this.milSystem, Width, Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref image_8U);
                    MIL.MbufClear(image_8U, 0L);

                    MIL.MbufAllocColor(this.milSystem, 3, Width, Height, 8 + MIL.M_UNSIGNED,
                        MIL.M_IMAGE + MIL.M_PROC, ref image_Color);
                    MIL.MbufClear(image_Color, 0L);

                    MIL.MimArith(Img, 16, image_8U, MIL.M_DIV_CONST);

                    for (int i = 0; i < 3; i++)
                    {
                        MIL.MbufCopyColor2d(
                            image_8U,
                            image_Color,
                            0,
                            0,
                            0,
                            (MIL_INT)i,
                            0,
                            0,
                            Width,
                            Height);
                    }

                    if (!Directory.Exists(setting.outPath))
                    {
                        Directory.CreateDirectory(setting.outPath);
                    }


                    foreach (BlobData Data in BaseLine)
                    {
                        if (Data.UsePitch)
                        {
                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                        }
                        else
                        {
                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                        }
                        MIL.MgraDot(MIL.M_DEFAULT, image_Color, Data.CogX, Data.CogY);
                    }

                    MIL.MbufSave(string.Format(@"{0}\[3]Img_BaseLine.tif", setting.outPath), image_Color);
                }

                #endregion


                return BaseLine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Search Base Line Error : {ex.Message}");
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image_Color);

                MilNetHelper.MilBufferFree(ref image_8U);
            }
        }
        #endregion

        #region --- RotateAlign ---
        private void RotateAlign(MIL_ID srcImage,
            PixelAlignSetting setting,
            EnumAlignDirection alignDirection,
            ref MIL_ID outImage,
            ref double outAngle)
        {
            MIL_ID milZone = MIL.M_NULL;
            MIL_ID milContext = MIL.M_NULL;
            MIL_ID milBlobContext = MIL.M_NULL;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milEdgeMarker = MIL.M_NULL;

            MIL_INT totalBlobs = 0;
            MIL_INT edgeCount = 0;

            MIL_INT sizeX = MIL.MbufInquire(srcImage, MIL.M_SIZE_X);
            MIL_INT sizeY = MIL.MbufInquire(srcImage, MIL.M_SIZE_Y);

            double[] blob_MinX = null;
            double[] blob_MaxX = null;
            double[] blob_MinY = null;
            double[] blob_MaxY = null;
            double RadiusX = 0;
            double RadiusY = 0;
            double angle = 0.0;
            int ROI_StX = 0;
            int ROI_StY = 0;

            try
            {
                MilNetHelper.MilBufferFree(ref outImage);

                // [1] Get Processed Image
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0L);

                MIL.MimAlloc(this.milSystem, MIL.M_BINARIZE_ADAPTIVE_CONTEXT,
                    MIL.M_DEFAULT, ref milContext);

                // Adaptive Setting
                MIL.MimControl(milContext, MIL.M_THRESHOLD_MODE, MIL.M_LOCAL_MEAN);
                MIL.MimControl(milContext, MIL.M_FOREGROUND_VALUE, MIL.M_FOREGROUND_WHITE);
                MIL.MimControl(milContext, MIL.M_LOCAL_DIMENSION, (int)setting.FindPanelPixelSetting.ColFindPitchX * 2);
                MIL.MimControl(milContext, MIL.M_GLOBAL_OFFSET, (int)setting.CoarseAlignSetting.Threshold);

                /* Perform binarization. */
                MIL.MimBinarizeAdaptive(milContext, srcImage,
                   MIL.M_NULL, MIL.M_NULL, outImage, MIL.M_NULL, MIL.M_DEFAULT);

                MIL.MimClose(outImage, outImage, setting.CoarseAlignSetting.CloseNums, MIL.M_BINARY);

                // Save 
                if (setting.CoarseAlignSetting.DebugImage)
                {
                    MIL.MbufSave(
                        string.Format(@"{0}\RotatedAlign_imgPre.tif", setting.outPath),
                        outImage);
                }

                // [2] Get Rotation Center
                //--- Blob Calculate Context ---
                milBlobContext = MIL.MblobAlloc(this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_NULL);
                if (milBlobContext == MIL.M_NULL)
                    throw new Exception("milBlobContext == M_NULL");

                MIL.MblobControl(milBlobContext, MIL.M_ALL_FEATURES, MIL.M_DISABLE);
                MIL.MblobControl(milBlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_BINARY, MIL.M_ENABLE);
                MIL.MblobControl(milBlobContext, MIL.M_BOX, MIL.M_ENABLE);

                MIL.MblobControl(milBlobContext, MIL.M_SORT1, MIL.M_BOX_AREA);
                MIL.MblobControl(milBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

                //--- Blob Result ---
                MIL.MblobAllocResult(this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobResult);
                MIL.MblobControl(milBlobResult, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);    //Set it as binarize searching(1 bit data)
                if (milBlobResult == MIL.M_NULL)
                    throw new Exception("milBlobResult == M_NULL");

                //--- Blob Analysis ---
                MIL.MblobCalculate(milBlobContext, outImage, MIL.M_NULL, milBlobResult);

                ////--- Filter Blob ---	
                //MblobSelect(BlobResult, M_EXCLUDE, M_AREA, M_LESS, minBlobArea, M_NULL);
                //MblobSelect(BlobResult, M_INCLUDE, M_AREA, M_GREATER, minBlobArea, M_NULL);
                //MblobSelect(BlobResult, M_EXCLUDE, M_AREA, M_GREATER, maxBlobArea, M_NULL);

                MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT,
                   MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalBlobs);

                if (totalBlobs > 0)
                {
                    blob_MinX = new double[totalBlobs];
                    blob_MinY = new double[totalBlobs];
                    blob_MaxX = new double[totalBlobs];
                    blob_MaxY = new double[totalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MIN, blob_MinX);
                    MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MIN, blob_MinY);
                    MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MAX, blob_MaxX);
                    MIL.MblobGetResult(milBlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MAX, blob_MaxY);

                    this.CenterX = (blob_MinX[0] + blob_MaxX[0]) * 0.5;
                    this.CenterY = (int)(blob_MinY[0] + blob_MaxY[0]) * 0.5;
                    RadiusX = (blob_MaxX[0] - blob_MinX[0]) * 0.5;
                    RadiusY = (blob_MaxY[0] - blob_MinY[0]) * 0.5;

                    // [3] Get Rotation Angle
                    MIL.MmeasAllocMarker(this.milSystem, MIL.M_EDGE, MIL.M_DEFAULT, ref milEdgeMarker);
                    MIL.MmeasSetMarker(milEdgeMarker, MIL.M_NUMBER, MIL.M_ALL, MIL.M_NULL);        //Default : 1
                    MIL.MmeasSetMarker(milEdgeMarker, MIL.M_POLARITY, MIL.M_POSITIVE, MIL.M_NULL);

                    if (alignDirection == EnumAlignDirection.Vertical)
                    {
                        MIL.MmeasSetMarker(milEdgeMarker, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
                        ROI_StX = Convert.ToInt32(this.CenterX - RadiusX - 40);
                        ROI_StY = Convert.ToInt32(this.CenterY - 100);
                        if (ROI_StX < 0) ROI_StX = 0;
                        if (ROI_StY < 0) ROI_StY = 0;
                        MIL.MbufChild2d(outImage, (MIL_INT)ROI_StX, (MIL_INT)ROI_StY, 80, 200, ref milZone);
                    }
                    else if (alignDirection == EnumAlignDirection.Horizontal)
                    {
                        MIL.MmeasSetMarker(milEdgeMarker, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
                        ROI_StX = Convert.ToInt32(this.CenterX - 100);
                        ROI_StY = Convert.ToInt32(this.CenterY - RadiusY - 40);
                        if (ROI_StX < 0) ROI_StX = 0;
                        if (ROI_StY < 0) ROI_StY = 0;
                        MIL.MbufChild2d(outImage, (MIL_INT)ROI_StX, (MIL_INT)ROI_StY, 200, 80, ref milZone);
                    }

                    // Save
                    if (setting.CoarseAlignSetting.DebugImage)
                    {
                        MIL.MbufSave(
                            string.Format(@"{0}\RotatedAlign_ROI.tif", setting.outPath),
                            milZone);
                    }

                    MIL.MmeasFindMarker(MIL.M_DEFAULT, milZone, milEdgeMarker, MIL.M_ANGLE);
                    MIL.MmeasGetResult(milEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref edgeCount, MIL.M_NULL);

                    if (edgeCount > 0)
                    {
                        MIL.MmeasGetResult(milEdgeMarker, MIL.M_ANGLE + MIL.M_TYPE_MIL_DOUBLE, ref angle, MIL.M_NULL);
                        if (alignDirection == EnumAlignDirection.Vertical)
                            angle = angle - 90;
                        else if (alignDirection == EnumAlignDirection.Horizontal)
                        {
                            if (angle > 270)
                                angle = angle - 360;
                        }
                        // Output : angle
                        outAngle = angle;

                        angle = angle * (-1.0);

                        if (angle != 0)
                            MIL.MimRotate(srcImage, srcImage, angle, this.CenterX, this.CenterY, this.CenterX, this.CenterY, MIL.M_BILINEAR); //M_NEAREST_NEIGHBOR + M_OVERSCAN_ENABLE. 

                        //更新搜尋 Panel Pixel左上角點
                        setting.FindPanelPixelSetting.FirstPixelX = (int)blob_MinX[0];
                        setting.FindPanelPixelSetting.FirstPixelY = (int)blob_MinY[0];
                        setting.FindPanelPixelSetting.EndPixelX = (int)blob_MaxX[0];
                        setting.FindPanelPixelSetting.EndPixelY = (int)blob_MaxY[0];

                        // Save
                        if (setting.CoarseAlignSetting.DebugImage)
                        {
                            MIL.MbufSave(
                                string.Format(@"{0}\RotatedAlign.tif", setting.outPath),
                                srcImage);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format(
                        "[RotateAlign] {0}",
                        @"Can not find edge !"));
                    }

                    // Free & Copy
                    MilNetHelper.MilBufferFree(ref milZone);
                    MilNetHelper.MilBufferFree(ref outImage);

                    //Copy To Output
                    if (outImage != MIL.M_NULL)
                    {
                        if ((MIL.MbufInquire(outImage, MIL.M_SIZE_X) != MIL.MbufInquire(srcImage, MIL.M_SIZE_X)) ||
                        (MIL.MbufInquire(outImage, MIL.M_SIZE_Y) != MIL.MbufInquire(srcImage, MIL.M_SIZE_Y)))
                        {
                            MIL.MbufFree(outImage);
                            outImage = MIL.M_NULL;
                            MIL.MbufClone(srcImage, this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref outImage);
                        }
                    }
                    else
                        MIL.MbufClone(srcImage, this.milSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref outImage);

                    MIL.MbufCopy(srcImage, outImage);

                    return;

                }
                else
                {
                    throw new Exception(string.Format(
                        "[RotateAlign] {0}",
                        @"Can not Find Blob to rotate Image !"));
                }

            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[RotateAlign] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref milZone);

                MilNetHelper.MilMimFree(ref milContext);

                MilNetHelper.MilBlobFree(ref milBlobContext);

                MilNetHelper.MilBlobFree(ref milBlobResult);

                MilNetHelper.MilMeasFree(ref milEdgeMarker);

                blob_MinX = null;
                blob_MaxX = null;
                blob_MinY = null;
                blob_MaxY = null;
            }

        }
        #endregion

        #region --- CalculateEdge ---
        private Point[] CalculateEdge(MIL_ID Img, Rectangle ROI, int SubEdge, int Orientation, int Direction)
        {
            int RoiWidth = ROI.Width;
            int RoiHeight = ROI.Height;
            Point StartP = new Point(ROI.X, ROI.Y);
            MIL_ID MeaMarker = MIL.M_NULL;

            try
            {
                MIL.MmeasAllocMarker(this.milSystem, MIL.M_EDGE, MIL.M_DEFAULT, ref MeaMarker);

                MIL.MmeasSetMarker(MeaMarker, MIL.M_SUB_REGIONS_NUMBER, SubEdge, MIL.M_NULL);

                Point[] EdgePoint = new Point[SubEdge];

                switch (Direction)
                {
                    case 1:
                        MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_ANGLE, 0 + 0, MIL.M_NULL);
                        break;

                    case 2:
                        MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_ANGLE, 180 + 0, MIL.M_NULL);
                        break;
                }

                switch (Orientation)
                {
                    case 0:
                        MIL.MmeasSetMarker(MeaMarker, MIL.M_ORIENTATION, MIL.M_DEFAULT, MIL.M_NULL);
                        break;

                    case 1:
                        MIL.MmeasSetMarker(MeaMarker, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
                        break;

                    case 2:
                        MIL.MmeasSetMarker(MeaMarker, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
                        break;
                }

                MIL.MmeasSetMarker(MeaMarker, MIL.M_NUMBER, MIL.M_ALL, MIL.M_NULL);

                int Gap = ((int)RoiWidth / SubEdge);

                switch (Orientation)
                {
                    case 1:
                        Gap = ((int)RoiWidth / SubEdge);
                        break;

                    case 2:
                        Gap = ((int)RoiHeight / SubEdge);
                        break;
                }

                for (int i = 0; i < SubEdge; i++)
                {
                    switch (Orientation)
                    {
                        case 1:
                            MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_ORIGIN, StartP.X + i * Gap, StartP.Y);
                            MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_SIZE, Gap, RoiHeight - 1);
                            break;

                        case 2:
                            MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_ORIGIN, StartP.X, StartP.Y + i * Gap);
                            MIL.MmeasSetMarker(MeaMarker, MIL.M_BOX_SIZE, RoiWidth - 1, Gap);
                            break;
                    }


                    MIL.MmeasFindMarker(MIL.M_DEFAULT, Img, MeaMarker, MIL.M_DEFAULT);

                    MIL_INT Cnt = 0;

                    MIL.MmeasGetResult(MeaMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref Cnt);

                    if (Cnt > 0)
                    {
                        double[] EdgeX = new double[(int)Cnt];
                        double[] EdgeY = new double[(int)Cnt];
                        MIL.MmeasGetResult(MeaMarker, MIL.M_POSITION_X, EdgeX);
                        MIL.MmeasGetResult(MeaMarker, MIL.M_POSITION_Y, EdgeY);

                        EdgePoint[i] = new Point((int)EdgeX[0], (int)EdgeY[0]);
                    }
                }

                return EdgePoint;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CalculateEdge] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilMeasFree(ref MeaMarker);
            }
        }
        #endregion

        #region --- SearchPoint ---
        private BlobData SearchPoint(
            List<BlobData> blobs,
            BlobData currentPoint,
            double pitchX,
            double pitchY,
            double deltaPitch)
        {
            BlobData point = null;
            double offsetX = 0.0;
            double offsetY = 0.0;
            //bool test = false;

            try
            {
                BlobData expectPoint = new BlobData(currentPoint);

                offsetX = pitchX;
                offsetY = pitchY;

                expectPoint.CogX += offsetX;
                expectPoint.CogY += offsetY;

                // Test tool
                //if(expectPoint.CogX == 7646 && expectPoint.CogX == 6824)
                //{
                //    test = true;
                //}

                var pointData = from b in blobs
                                where this.CheckDistance(b, expectPoint, deltaPitch)
                                   && this.CheckNotCurrent(b, currentPoint)
                                select b;

                // find the point which is most closest to the expectPoint
                foreach (var b in pointData)
                {
                    b.SetDis(EuclideanDistance(b, expectPoint));
                }
                var queryOrder = from e in pointData
                                 orderby e.GetDis()
                                 select e;

                //point = pointData.ElementAtOrDefault(0);
                point = queryOrder.ElementAtOrDefault(0);
            }
            catch (Exception)
            { }

            if (point == null)
            {

                point = new BlobData
                {
                    CogX = currentPoint.CogX + offsetX,
                    CogY = currentPoint.CogY + offsetY,
                    UsePitch = true
                };

                var pointData = from b in blobs
                                where this.CheckDistance(b, point, deltaPitch * 1.2)
                                   && this.CheckNotCurrent(b, point)
                                select b;

                BlobData Retry_point = null;

                Retry_point = pointData.ElementAtOrDefault(0);

                if (Retry_point != null)
                {
                    point = Retry_point;
                }

                //point = new BlobData
                //{
                //    CogX = currentPoint.CogX + offsetX,
                //    CogY = currentPoint.CogY + offsetY,
                //    UsePitch = true
                //};

                //var pointData = from b in blobs
                //                where this.CheckDistance(b, point, deltaPitch * 1.5)
                //                   && this.CheckNotCurrent(b, point)
                //                select b;

                //BlobData Retry_point = null;

                //// find the point which is most closest to the expectPoint
                //foreach (var b in pointData)
                //{
                //    b.SetDis(EuclideanDistance(b, point));
                //}
                //var queryOrder = from e in pointData
                //                 orderby e.GetDis()
                //                 select e;

                //Retry_point = queryOrder.ElementAtOrDefault(0);
                ////Retry_point = pointData.ElementAtOrDefault(0);

                //if (Retry_point != null)
                //{
                //    point = Retry_point;
                //}
            }

            return point;
        }
        #endregion

        #region --- ExtendInValidPoint ---
        private BlobData ExtendInValidPoint(
            BlobData currentPoint,
            double pitchX,
            double pitchY)
        {
            BlobData expectPoint = null;
            double offsetX = 0.0;
            double offsetY = 0.0;

            try
            {
                expectPoint = new BlobData(currentPoint);

                offsetX = pitchX;
                offsetY = pitchY;

                expectPoint.CogX += offsetX;
                expectPoint.CogY += offsetY;
                expectPoint.Valid = false;    // 無效區域點
            }
            catch (Exception)
            { }

            return expectPoint;

        }
        #endregion

        #region --- SearchCogData ---
        private BlobData[,] SearchCogData(MIL_ID Img,
            int[] LedCnt,
            PanelInfo panelInfo,
            List<BlobData> BlobDatas,
            List<BlobData> CenterData,
            PixelAlignSetting setting,
            Point StartPos)
        {
            int SubCnt = 0;
            int SubCntR = 0;
            int SubCntL = 0;
            int FullCnt_R = 0;
            int FullCnt_L = 0;
            int panelSizeX = 0;
            int panelSizeY = 0;
            BlobData SearchedPointR = null;
            BlobData SearchedPointL = null;

            MIL_ID image_8U = MIL.M_NULL;
            MIL_ID image_Color = MIL.M_NULL;
            //List<BlobData> CogDatas = new List<BlobData>();
            BlobData[,] CogDatas = null;

            double RowPitchX = setting.FindPanelPixelSetting.RowFindPitchX;
            double RowPitchY = setting.FindPanelPixelSetting.RowFindPitchY;
            double ColPitchX = setting.FindPanelPixelSetting.ColFindPitchX;
            double ColPitchY = setting.FindPanelPixelSetting.ColFindPitchY;

            double BaseLine = StartPos.X;
            int LedCount = 0;
            bool debugImage = setting.FindPanelPixelSetting.DebugImage;

            try
            {
                //防呆

                if (StartPos.X < 0)
                {
                    throw new Exception("FirstPixelX < 0.");
                }

                if (StartPos.Y < 0)
                {
                    throw new Exception("FirstPixelY < 0.");
                }

                //Search Base Line
                panelSizeX = panelInfo.ResolutionX;
                panelSizeY = panelInfo.ResolutionY;
                CogDatas = new BlobData[panelSizeX, panelSizeY];
                int Data = 0;
                int Gate = -1;
                double serchRangeYRtio = setting.FindPanelPixelSetting.SearchRangeYRatio;

                for (int i = 0; i < CenterData.Count; i++)
                {
                    LedCount = LedCnt[i];

                    BlobData CenterPoint = CenterData[i];

                    var BlobPart = from b in BlobDatas
                                   where Math.Abs(CenterPoint.CogY - b.CogY) < serchRangeYRtio * RowPitchY
                                   select b;

                    //var BlobPart = from b in BlobDatas
                    //               where ((CenterPoint.CogY - b.CogY) < 0.60 * RowPitchY && (CenterPoint.CogY - b.CogY) > -0.2 * RowPitchY)
                    //               select b;

                    List<BlobData> BlobPartList = BlobPart.ToList();

                    double PitchX = ColPitchX;
                    double PitchY = ColPitchY;
                    int L_Data = 0;
                    int R_Data = 0;

                    // 定義 : FullCnt_R、FullCnt_L、R_Data、L_Data
                    if (panelSizeX % 2 == 0)
                    {
                        if (CenterPoint.CogX > BaseLine)
                        {
                            Data = panelSizeX / 2 + 1;
                            FullCnt_R = panelSizeX / 2 - 1;
                            FullCnt_L = panelSizeX / 2 + 1;
                        }
                        else
                        {
                            Data = panelSizeX / 2 - 1;
                            FullCnt_R = panelSizeX / 2 + 1;
                            FullCnt_L = panelSizeX / 2 - 1;
                        }
                    }
                    else
                    {
                        if (CenterPoint.CogX > BaseLine)
                        {
                            FullCnt_R = (panelSizeX - 1) / 2;
                            FullCnt_L = panelSizeX - FullCnt_R;
                        }
                        else
                        {
                            FullCnt_L = (panelSizeX - 1) / 2;
                            FullCnt_R = panelSizeX - FullCnt_L;
                        }

                        Data = (panelSizeX - 1) / 2;
                    }

                    if (LedCount % 2 == 1)
                    {
                        SubCnt = (LedCount - 1) / 2;
                        BlobData BasePointR = CenterPoint;
                        BlobData BasePointL = CenterPoint;

                        Gate += 1;
                        CogDatas[Data, Gate] = new BlobData(CenterPoint);


                        // [1] Full Zone R
                        for (int x = 0; x < FullCnt_R; x++)
                        {
                            if (x < SubCnt)
                            {
                                //[1] Valid Zone
                                SearchedPointR = this.SearchPoint(BlobPartList, BasePointR, PitchX, PitchY, 0.6 * ColPitchX);
                            }
                            else
                            {
                                //[2] InValid Zone
                                SearchedPointR = this.ExtendInValidPoint(BasePointR, PitchX, PitchY);
                            }

                            R_Data = Data + x + 1;
                            if (R_Data < panelSizeX)
                            {
                                CogDatas[R_Data, Gate] = new BlobData(SearchedPointR);
                                BasePointR = SearchedPointR;
                            }

                        }

                        // [2] Full Zone L
                        for (int x = 0; x < FullCnt_L; x++)
                        {
                            if (x < SubCnt)
                            {
                                // [1] Valid Zone
                                SearchedPointL = this.SearchPoint(BlobPartList, BasePointL, -PitchX, PitchY, 0.6 * ColPitchX);
                            }
                            else
                            {
                                //[2] InValid Zone
                                SearchedPointL = this.ExtendInValidPoint(BasePointL, -PitchX, PitchY);
                            }

                            L_Data = Data - x - 1;
                            if (L_Data >= 0)
                            {
                                CogDatas[L_Data, Gate] = new BlobData(SearchedPointL);
                                BasePointL = SearchedPointL;
                            }

                        }

                    }
                    else
                    {
                        BlobData BasePointR = CenterPoint;
                        BlobData BasePointL = CenterPoint;

                        SubCntR = 0;
                        SubCntL = 0;

                        if (CenterPoint.CogX > BaseLine)
                        {
                            SubCntR = (LedCount) / 2 - 1;
                            SubCntL = (LedCount) / 2;
                        }
                        else
                        {
                            SubCntR = (LedCount) / 2;
                            SubCntL = (LedCount) / 2 - 1;
                        }

                        Gate += 1;
                        CogDatas[Data, Gate] = new BlobData(CenterPoint);

                        // [1] Full Zone R
                        for (int x = 0; x < FullCnt_R; x++)
                        {
                            if (x < SubCntR)
                            {   //[1] Valid Zone
                                SearchedPointR = this.SearchPoint(BlobPartList, BasePointR, PitchX, PitchY, 0.6 * ColPitchX);
                            }
                            else
                            {   //[2] InValid Zone
                                SearchedPointR = this.ExtendInValidPoint(BasePointR, PitchX, PitchY);
                            }

                            R_Data = Data + x + 1;
                            if (R_Data < panelSizeX)
                            {
                                CogDatas[R_Data, Gate] = new BlobData(SearchedPointR);
                                BasePointR = SearchedPointR;
                            }

                        }

                        // [1] Full Zone L
                        for (int x = 0; x < FullCnt_L; x++)
                        {
                            if (x < SubCntL)
                            {   //[1] Valid Zone
                                SearchedPointL = this.SearchPoint(BlobPartList, BasePointL, -PitchX, PitchY, 0.6 * ColPitchX);
                            }
                            else
                            {   //[2] InValid Zone
                                SearchedPointL = this.ExtendInValidPoint(BasePointL, -PitchX, PitchY);
                            }

                            L_Data = Data - x - 1;
                            if (L_Data >= 0)
                            {
                                CogDatas[L_Data, Gate] = new BlobData(SearchedPointL);
                                BasePointL = SearchedPointL;
                            }

                        }

                    }
                }

                //#region --- Draw Cog Data ---

                //if (debugImage)
                //{
                //    MIL_INT Width = MIL.MbufInquire(Img, MIL.M_SIZE_X);
                //    MIL_INT Height = MIL.MbufInquire(Img, MIL.M_SIZE_Y);

                //    MIL.MbufAlloc2d(this.milSystem, Width, Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref image_8U);
                //    MIL.MbufClear(image_8U, 0L);

                //    MIL.MbufAllocColor(this.milSystem, 3, Width, Height, 8 + MIL.M_UNSIGNED,
                //        MIL.M_IMAGE + MIL.M_PROC, ref image_Color);
                //    MIL.MbufClear(image_Color, 0L);

                //    MIL.MimArith(Img, 16, image_8U, MIL.M_DIV_CONST);

                //    for (int i = 0; i < 3; i++)
                //    {
                //        MIL.MbufCopyColor2d(
                //            image_8U,
                //            image_Color,
                //            0,
                //            0,
                //            0,
                //            (MIL_INT)i,
                //            0,
                //            0,
                //            Width,
                //            Height);
                //    }

                //    if (!Directory.Exists(setting.outPath))
                //    {
                //        Directory.CreateDirectory(setting.outPath);
                //    }

                //    BlobData blobData;
                //    for (int y = 0; y < panelSizeY; y++)
                //    {
                //        for (int x = 0; x < panelSizeX; x++)
                //        {
                //            blobData = CogDatas[x, y];
                //            if (blobData.Valid)
                //            {
                //                if (blobData.UsePitch)
                //                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                //                else
                //                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                //            }
                //            else
                //            {
                //                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                //            }

                //            MIL.MgraDot(MIL.M_DEFAULT, image_Color, blobData.CogX, blobData.CogY);
                //        }
                //    }

                //    MIL.MbufSave(string.Format(@"{0}\Img_CogData.tif", setting.outPath), image_Color);
                //}

                //#endregion

                return CogDatas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Search Cog Data Error : {ex.Message}");
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image_Color);

                MilNetHelper.MilBufferFree(ref image_8U);
            }

        }

        #endregion

        #region --- PixelAlign ---
        private BlobData[,] PixelAlign(
            MIL_ID image,
            int[] LedCnt,
            PanelInfo panelInfo,
            PixelAlignSetting setting)
        {
            List<BlobData> LedList = null;
            BlobData[,] cogDatas = null;

            MIL_ID maskImage = MIL.M_NULL;
            MIL_ID blobImage = MIL.M_NULL;

            try
            {
                // [1] Calculte Left Tp Point
                LedList = this.GetLedPos_And_SearchStartPos(image, setting);

                // [2] Search Base Line
                List<BlobData> BaseLine = SearchBaseLine(image, LedList, LedCnt.Length, setting);

                // [3] Search Cog Data
                Point StartPoint = new Point(
                     (int)((setting.FindPanelPixelSetting.FirstPixelX + setting.FindPanelPixelSetting.EndPixelX) * 0.5),
                     (int)setting.FindPanelPixelSetting.FirstPixelY
                     );

                cogDatas = SearchCogData(image, LedCnt, panelInfo, LedList, BaseLine, setting, StartPoint);

            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[PixelAlign] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref maskImage);

                MilNetHelper.MilBufferFree(ref blobImage);
            }

            return cogDatas;
        }
        #endregion

        #endregion

        #region --- In-line Function ---

        #region --- GetLedCnt ---
        public int[] GetLedCounts(string CsvPath)
        {
            try
            {
                string[] CsvDatas = File.ReadAllLines(CsvPath);

                List<int> LedCnt = new List<int>();

                foreach (string CsvData in CsvDatas)
                {
                    int Cnt = 0;
                    bool isNum = int.TryParse(CsvData, out Cnt);

                    if (isNum)
                    {
                        LedCnt.Add(Cnt);
                    }
                }

                return LedCnt.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[GetLedCounts] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- Calculate ---
        /// <summary>
        ///  Find panel pixel cog position
        /// </summary>
        /// <param name="image">Source gray image</param>
        /// <param name="LedCnt">Led Count per Row</param>
        /// <param name="panelInfo">Panel information</param>
        /// <param name="setting">Pixel align setting parameters</param>
        /// <param name="outAngle">output Rotated angle</param>
        /// <param name="outImage">output draw Image</param>
        public BlobData[,] Calculate(
             MIL_ID image,
             int[] LedCnt,
             PanelInfo panelInfo,
             PixelAlignSetting setting,
             ref double outAngle,
             ref MIL_ID outImage)
        {
            BlobData[,] cogDatas = null;

            try
            {
                // Check
                if (image == MIL.M_NULL)
                {
                    throw new Exception("image is M_NULL.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (setting == null)
                {
                    throw new Exception("setting is null.");
                }

                // PixelAlign
                cogDatas = this.PixelAlign(image, LedCnt, panelInfo, setting);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[PixelAlign] {0}",
                        ex.Message));
            }


            return cogDatas;
        }
        #endregion 

        #region --- ImageRotationDetect ---
        public void ImageRotationDetect(
            MIL_ID image,
            ref PixelAlignSetting setting,
            ref double outAngle,
            ref MIL_ID processImage)
        {
            MIL_ID imgTemp = MIL.M_NULL;

            double outAngle_1 = 0.0;
            double outAngle_2 = 0.0;

            try
            {
                //Stopwatch sw = new Stopwatch();

                //sw.Restart();

                if (setting.CoarseAlignSetting.RotationAlign)
                {
                    this.RotateAlign(
                    image,
                    setting,
                    EnumAlignDirection.Vertical,
                    ref imgTemp,
                    ref outAngle_1);

                    this.RotateAlign(
                        imgTemp,
                        setting,
                        EnumAlignDirection.Horizontal,
                        ref processImage,
                        ref outAngle_2);

                    outAngle = outAngle_1 + outAngle_2;
                }
                else
                {
                    if (processImage != MIL.M_NULL)
                    {
                        MIL.MbufFree(processImage);
                        processImage = MIL.M_NULL;
                    }

                    MIL.MbufClone(image, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref processImage);
                    MIL.MbufCopy(image, processImage);
                }

                //sw.Stop();
                //Console.WriteLine(string.Format("ImageRotationDetect : {0} ms", sw.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[ImageRotationDetect] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref imgTemp);
            }

        }

        #endregion

        #region --- ImageRotation ---
        public void ImageRotation(
            MIL_ID srcImage,
            ref MIL_ID outimage,
            double angle)
        {
            try
            {
                Stopwatch sw = new Stopwatch();

                sw.Restart();

                MIL.MimRotate(srcImage, outimage, angle, this.CenterX, this.CenterY, this.CenterX, this.CenterY, MIL.M_BILINEAR); //M_NEAREST_NEIGHBOR + M_OVERSCAN_ENABLE. 

                sw.Stop();
                Console.WriteLine(string.Format("ImageRotation : {0} ms", sw.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[ImageRotation] {0}",
                        ex.Message));
            }
        }

        #endregion
             
        #region --- DrawCog ---
        public void DrawCog(
            string path,
            string patternName,
            MIL_ID image,
            PanelInfo panelInfo,
            BlobData[,] cogDatas)
        {
            MIL_ID image_8U = MIL.M_NULL;
            MIL_ID image_Color = MIL.M_NULL;

            MIL_INT sizeX = MIL.MbufInquire(image, MIL.M_SIZE_X);
            MIL_INT sizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y);
            MIL_INT bits = MIL.MbufInquire(image, MIL.M_SIZE_BIT);

            string filePath = "";

            try
            {
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
                MIL.M_IMAGE + MIL.M_PROC, ref image_8U);
                MIL.MbufClear(image_8U, 0L);

                MIL.MbufAllocColor(this.milSystem, 3, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC, ref image_Color);
                MIL.MbufClear(image_Color, 0L);

                if (bits > 8)
                    MIL.MimArith(image, 16, image_8U, MIL.M_DIV_CONST);
                else
                    MIL.MimArith(image, 2, image_8U, MIL.M_DIV_CONST);

                for (int i = 0; i < 3; i++)
                {
                    MIL.MbufCopyColor2d(
                        image_8U,
                        image_Color,
                        0,
                        0,
                        0,
                        (MIL_INT)i,
                        0,
                        0,
                        sizeX,
                        sizeY);
                }

                foreach (BlobData blob in cogDatas)
                {
                    if (blob != null)
                    {
                        blob.CogX = Math.Round(blob.CogX, MidpointRounding.AwayFromZero);
                        blob.CogY = Math.Round(blob.CogY, MidpointRounding.AwayFromZero);

                        if (blob.Valid)
                        {
                            if (blob.UsePitch)
                                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                            else
                                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                        }
                        else
                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);

                        MIL.MgraDot(MIL.M_DEFAULT, image_Color, blob.CogX, blob.CogY);

                    }
                }

                filePath = string.Format(@"{0}\{1}_{2}", path, patternName, "DrawCogColor.tif");

                MIL.MbufSave(filePath, image_Color);              
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[DrawCog] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image_Color);
                MilNetHelper.MilBufferFree(ref image_8U);
            }

        }

        #endregion

        #endregion


        #region --- Check Point ---
        int Gcnt = 0;

        #region --- CheckNotCurrent ---
        private bool CheckNotCurrent(
            BlobData p1,
            BlobData p2)
        {
            bool flag =
                !(p1.CogX == p2.CogX
                && p1.CogY == p2.CogY);

            return flag;
        }
        #endregion

        #region --- CheckDistance ---
        private bool CheckDistance(
            BlobData p1,
            BlobData p2,
            double length)
        {
            double dis = this.EuclideanDistance(p1, p2);

            //Console.WriteLine("Gcnt:" + Gcnt + ", Dis:" + dis);

            Gcnt++;

            return dis >= 0 && dis < length;
        }
        #endregion

        #region --- EuclideanDistance ---
        private double EuclideanDistance(
            BlobData p1,
            BlobData p2)
        {
            double result = 0;

            result = EuclideanDistance(
                p1.CogX, p1.CogY,
                p2.CogX, p2.CogY);

            return result;
        }
        #endregion

        #region --- EuclideanDistance ---
        private double EuclideanDistance(
            double p1x,
            double p1y,
            double p2x,
            double p2y)
        {
            double result = 0;

            double x = p1x - p2x;
            double y = p1y - p2y;

            result = this.PythagorasTheorem(x, y);

            return result;
        }
        #endregion

        #region --- PythagorasTheorem ---
        private double PythagorasTheorem(double x, double y)
        {
            double result = 0;

            result = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            return result;
        }
        #endregion

        #endregion

    }
}
