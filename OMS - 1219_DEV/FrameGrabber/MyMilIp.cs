using CommonBase.Config;
//using DocumentFormat.OpenXml.Drawing;
using LightMeasure;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;
//using DocumentFormat.OpenXml.Drawing.Charts;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.MessageBox;

using BaseTool;
using FrameGrabber;

public static class MyMilIp
{
    public static CameraBase MainCamera = null;
    private static MIL_ID MilContextId = MIL.M_NULL;           // Model identifier.

    private static MIL_ID MilGrayImage = MIL.M_NULL;
    private static MIL_ID MilTargetImage = MIL.M_NULL;         // Target image buffer identifier.
    private static MIL_ID DefaultGraphicContext = MIL.M_DEFAULT;
    private static MIL_ID MilOverlayImage = MIL.M_NULL;

    //Light Measure
    public static LightMeasureConfig correctionConfig;

    public static MeasureData predictData;

    public enum EnCalibrationState
    {
        None,
        Working,
        Successful,
        Failed,
    }

    //User Calibration
    public static bool IsUserCalibrationFinish = false;

    public static EnCalibrationState IsUserCoeffFinish = EnCalibrationState.None;

    #region --- General ---

    #region --- SpotROI ---

    private static void SpotROI(System.Drawing.Rectangle ROI, ref MIL_ID MilSrc)
    {
        // Allocate Dest
        MIL_ID MilDest = MIL.M_NULL;
        MIL_INT w = MIL.MbufInquire(MilSrc, MIL.M_SIZE_X, MIL.M_NULL);
        MIL_INT h = MIL.MbufInquire(MilSrc, MIL.M_SIZE_Y, MIL.M_NULL);
        MIL.MbufAllocColor(MyMil.MilSystem, 1, w, h, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref MilDest);

        // Save as source image
        MIL.MbufCopyColor2d(MilSrc, MilDest, MIL.M_ALL_BANDS, ROI.X, ROI.Y, MIL.M_ALL_BANDS, ROI.X, ROI.Y, ROI.Width, ROI.Height);
        MIL.MbufCopy(MilDest, MilSrc);

        // Free
        MilNetHelper.MilBufferFree(ref MilDest);
    }

    #endregion --- SpotROI ---

    #region --- RestoreAndConvert ---

    private static void RestoreAndConvert(string Filename, ref MIL_ID NewProc)
    {
        MIL_ID TmpMilID = MIL.M_NULL;
        MIL_INT SizeX = 0;
        MIL_INT SizeY = 0;
        MIL_INT Band = 0;
        MIL_INT Bit = 0;

        try
        {
            /* Load the buffer in a temporary buffer. */
            MIL.MbufRestore(Filename, MyMil.MilSystem, ref TmpMilID);
            MIL.MbufInquire(TmpMilID, MIL.M_SIZE_X, ref SizeX);
            MIL.MbufInquire(TmpMilID, MIL.M_SIZE_Y, ref SizeY);
            MIL.MbufInquire(TmpMilID, MIL.M_SIZE_BAND, ref Band);
            MIL.MbufInquire(TmpMilID, MIL.M_SIZE_BIT, ref Bit);

            /* Allocate a unsigned 8 bit buffer for processing. */
            MIL.MbufAlloc2d(MyMil.MilSystem, SizeX, SizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref NewProc);

            /* Convert input buffer to luminance. */
            if (Band > 1)
            {
                throw new Exception("Unsupport Image Format. ( Band: " + Band + ")");
            }
            else
            {
                MIL.MimArith(TmpMilID, 32, NewProc, MIL.M_DIV_CONST);
                //MbufCopy(TmpBuffer.Id, NewProcBuffer.Id);
            }
        }
        catch 
        {
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref TmpMilID);
        }
    }

    #endregion --- RestoreAndConvert ---

    #region --- CloneBuffer ---

    private static void CloneBuffer(MIL_ID SrcBuffer, ref MIL_ID DstBuffer, bool WithData = false)
    {
        MilNetHelper.MilBufferFree(ref DstBuffer);

        if (!WithData)
            MIL.MbufClone(SrcBuffer, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref DstBuffer);
        else
            MIL.MbufClone(SrcBuffer, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COPY_SOURCE_DATA, ref DstBuffer);
    }

    #endregion --- CloneBuffer ---

    #endregion --- General ---

    #region --- Create Buf ---

    #region --- AllocBuf ---

    public static void AllocBuf(ref MIL_ID TargetImg, MIL_INT ImageWidth, MIL_INT ImageHeight, MIL_INT ImageDepth, MIL_INT ImageSign, bool ConvertGray = false)
    {
        MilNetHelper.MilBufferFree(ref TargetImg);

        if (ImageDepth > 8)
        {
            ImageDepth = 16;
        }
        else
        {
            ImageDepth = 8;
        }

        MIL.MbufAlloc2d(MyMil.MilSystem,
                        ImageWidth,
                        ImageHeight,
                        ImageDepth + ImageSign,
                        MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP,
                        ref TargetImg);

        if (ImageDepth > 8)
        {
            MIL.MbufControl(TargetImg, MIL.M_MIN, 0);
            MIL.MbufControl(TargetImg, MIL.M_MAX, 4095);
        }
    }

    #endregion --- AllocBuf ---

    #region --- AllocBuf ---

    public static void AllocBuf(ref MIL_ID TargetImg, string ImgPath, int Bits, bool ConvertGray = false)
    {
        double SizeX = 0.0;
        double SizeY = 0.0;
        MIL_ID SourceImg = MIL.M_NULL;

        try
        {
            MIL.MbufRestore(ImgPath, MyMil.MilSystem, ref SourceImg);

            MilNetHelper.MilBufferFree(ref TargetImg);

            MIL.MbufAlloc2d(MyMil.MilSystem,
                            MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                            MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                            Bits + MIL.M_UNSIGNED,
                            MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC,
                            ref TargetImg);

            MIL.MbufCopy(SourceImg, TargetImg);

            if (Bits > 8)
            {
                MIL.MbufControl(TargetImg, MIL.M_MIN, 0);
                MIL.MbufControl(TargetImg, MIL.M_MAX, 4095);
            }

            if (ConvertGray)
            {
                MIL.MimConvert(SourceImg, TargetImg, MIL.M_RGB_TO_L);    //將RGB轉換成灰階存進image buffer
            }
        }
        catch 
        {
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref SourceImg);
        }
    }

    #endregion --- AllocBuf ---

    #region --- AllocBufColor ---

    public static void AllocBufColor(ref MIL_ID TargetImg, string ImgPath, int Bits, bool ConvertGray = false)
    {
        MIL_ID SourceImg = MIL.M_NULL;

        MIL.MbufRestore(ImgPath, MyMil.MilSystem, ref SourceImg);

        double SizeX = 0.0;
        double SizeY = 0.0;

        MilNetHelper.MilBufferFree(ref TargetImg);

        MIL.MbufAllocColor(MyMil.MilSystem, 3,
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                       Bits + MIL.M_UNSIGNED,
                       MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref TargetImg);

        MIL.MbufCopyColor(SourceImg, TargetImg, MIL.M_ALL_BANDS);

        if (ConvertGray)
        {
            MIL.MimConvert(SourceImg, TargetImg, MIL.M_RGB_TO_L);    //將RGB轉換成灰階存進image buffer
        }

        // Free
        MilNetHelper.MilBufferFree(ref SourceImg);
    }

    #endregion --- AllocBufColor ---

    #endregion --- Create Buf ---

    #region --- Load Image ---

    #region --- LoadImage ---

    public static void LoadImage(ref MIL_ID Img, string ImgPath, IntPtr HandlePanel, bool ConvertGray = false)
    {
        MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);

        MilNetHelper.MilBufferFree(ref MilTargetImage);

        MIL.MbufRestore(ImgPath, MyMil.MilSystem, ref MilTargetImage);

        // Load target image into image buffers and display it.

        MIL.MdispSelectWindow(MyMil.MilMainDisplay, MilTargetImage, HandlePanel);

        //MIL.MdispSelect(MyMil.MilDisplay, MilTargetImage);

        double SizeX = 0.0;
        double SizeY = 0.0;

        MilNetHelper.MilBufferFree(ref MilGrayImage);

        MIL.MbufAlloc2d(MyMil.MilSystem,
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                       8 + MIL.M_UNSIGNED,
                       MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MilGrayImage);

        MilNetHelper.MilBufferFree(ref Img);

        MIL.MbufAlloc2d(MyMil.MilSystem,
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                       MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                       8 + MIL.M_UNSIGNED,
                       MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref Img);

        MIL.MbufCopy(MilTargetImage, MilGrayImage);
        MIL.MbufCopy(MilTargetImage, Img);

        if (ConvertGray)
        {
            MIL.MimConvert(MilTargetImage, MilGrayImage, MIL.M_RGB_TO_L);    //將RGB轉換成灰階存進image buffer
        }
    }

    #endregion --- LoadImage ---

    #region --- LoadImage ---

    public static void LoadImage(string ImgPath, IntPtr HandlePanel, bool ConvertGray = false)
    {
        MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);

        MilNetHelper.MilBufferFree(ref MilTargetImage);

        MIL.MbufRestore(ImgPath, MyMil.MilSystem, ref MilTargetImage);

        // Load target image into image buffers and display it.

        MIL.MdispSelectWindow(MyMil.MilMainDisplay, MilTargetImage, HandlePanel);

        //MIL.MdispSelect(MyMil.MilDisplay, MilTargetImage);

        double SizeX = 0.0;
        double SizeY = 0.0;
        double Type = 0.0;

        MilNetHelper.MilBufferFree(ref MilGrayImage);

        MIL.MbufAlloc2d(MyMil.MilSystem,
                      MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                      MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                      MIL.MbufDiskInquire(ImgPath, MIL.M_TYPE, ref Type),
                      MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MilGrayImage);

        MIL.MbufCopy(MilTargetImage, MilGrayImage);

        if (ConvertGray)
        {
            MIL.MimConvert(MilTargetImage, MilGrayImage, MIL.M_RGB_TO_L);    //將RGB轉換成灰階存進image buffer
        }
    }

    #endregion --- LoadImage ---

    #region --- LoadImage ---

    public static void LoadImage(string ImgPath, bool ConvertGray = false)
    {
        MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);

        MilNetHelper.MilBufferFree(ref MilTargetImage);

        MIL.MbufRestore(ImgPath, MyMil.MilSystem, ref MilTargetImage);

        double SizeX = 0.0;
        double SizeY = 0.0;

        MilNetHelper.MilBufferFree(ref MilGrayImage);

        MIL.MbufAlloc2d(MyMil.MilSystem,
                      MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_X, ref SizeX),
                      MIL.MbufDiskInquire(ImgPath, MIL.M_SIZE_Y, ref SizeY),
                      8 + MIL.M_UNSIGNED,
                      MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MilGrayImage);

        MIL.MbufCopy(MilTargetImage, MilGrayImage);

        if (ConvertGray)
        {
            MIL.MimConvert(MilTargetImage, MilGrayImage, MIL.M_RGB_TO_L);    //將RGB轉換成灰階存進image buffer
        }
    }

    #endregion --- LoadImage ---

    #endregion --- Load Image ---

    #region --- Image Process ---

    #region --- Dilate ---

    public static void Dilate(MIL_ID MilGrayImage)
    {
        MIL.MimDilate(MilGrayImage, MilGrayImage, 1, MIL.M_GRAYSCALE);
    }

    #endregion --- Dilate ---

    #region --- BinaryAdaptiveSeed ---

    public static byte[] BinaryAdaptiveSeed(string SavePath = null)
    {
        if (MilContextId != MIL.M_NULL)  // 2023/04/07 Rick add
        {
            MIL.MimFree(MilContextId);
            MilContextId = MIL.M_NULL;
        }

        MIL.MimAlloc(MyMil.MilSystem, MIL.M_BINARIZE_ADAPTIVE_FROM_SEED_CONTEXT, MIL.M_DEFAULT, ref MilContextId);

        MIL.MimControl(MilContextId, MIL.M_NB_SEED_ITERATIONS, 1);

        MIL.MimBinarizeAdaptive(MilContextId, MilGrayImage, MIL.M_DEFAULT, MIL.M_NULL, MilGrayImage, MIL.M_NULL, MIL.M_DEFAULT);

        int X = 0, Y = 0;
        MIL_INT SizeX = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_X, ref X);
        MIL_INT SizeY = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_Y, ref Y);

        byte[] TargetAry = new byte[SizeX * SizeY];

        MIL.MbufGet(MilGrayImage, TargetAry);

        if (SavePath != null)
        {
            MIL.MbufSave(SavePath, MilGrayImage);
        }

        return TargetAry;
    }

    #endregion --- BinaryAdaptiveSeed ---

    #region --- BinaryAdaptive_PseudoMedian ---

    public static byte[] BinaryAdaptive_PseudoMedian(int DilateCnt, string SavePath = null)
    {
        MIL.MimAlloc(MyMil.MilSystem, MIL.M_BINARIZE_ADAPTIVE_CONTEXT, MIL.M_DEFAULT, ref MilContextId);
        MIL.MimControl(MilContextId, MIL.M_THRESHOLD_MODE, MIL.M_PSEUDOMEDIAN);
        MIL.MimControl(MilContextId, MIL.M_LOCAL_DIMENSION, 3);

        MIL.MimBinarizeAdaptive(MilContextId, MilGrayImage, MIL.M_NULL, MIL.M_NULL, MilGrayImage, MIL.M_NULL, MIL.M_DEFAULT);

        if (DilateCnt > 0)
        {
            MIL.MimDilate(MilGrayImage, MilGrayImage, DilateCnt, MIL.M_GRAYSCALE);
        }

        int X = 0, Y = 0;
        MIL_INT SizeX = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_X, ref X);
        MIL_INT SizeY = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_Y, ref Y);

        byte[] TargetAry = new byte[SizeX * SizeY];

        MIL.MbufGet(MilGrayImage, TargetAry);

        if (SavePath != null)
        {
            MIL.MbufSave(SavePath, MilGrayImage);
        }

        return TargetAry;
    }

    #endregion --- BinaryAdaptive_PseudoMedian ---

    #region --- BinaryAdaptive_LocalMean ---

    public static byte[] BinaryAdaptive_LocalMean(int DilateCnt, string SavePath = null)
    {
        MIL.MimAlloc(MyMil.MilSystem, MIL.M_BINARIZE_ADAPTIVE_CONTEXT, MIL.M_DEFAULT, ref MilContextId);

        // Control Block for Binarize Adaptive Im Context4
        MIL.MimControl(MilContextId, MIL.M_THRESHOLD_MODE, MIL.M_LOCAL_MEAN);
        MIL.MimControl(MilContextId, MIL.M_LOCAL_DIMENSION, 39);
        MIL.MimControl(MilContextId, MIL.M_MINIMUM_CONTRAST, 0.2);

        MIL.MimBinarizeAdaptive(MilContextId, MilGrayImage, MIL.M_NULL, MIL.M_NULL, MilGrayImage, MIL.M_NULL, MIL.M_DEFAULT);
        //MIL.MimDilate(MilGrayImage, MilGrayImage, 2, MIL.M_GRAYSCALE);
        if (DilateCnt > 0)
        {
            MIL.MimDilate(MilGrayImage, MilGrayImage, DilateCnt, MIL.M_GRAYSCALE);
        }
        int X = 0, Y = 0;
        MIL_INT SizeX = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_X, ref X);
        MIL_INT SizeY = MIL.MbufInquire(MilGrayImage, MIL.M_SIZE_Y, ref Y);

        byte[] TargetAry = new byte[SizeX * SizeY];

        MIL.MbufGet(MilGrayImage, TargetAry);

        if (SavePath != null)
        {
            MIL.MbufSave(SavePath, MilGrayImage);
        }

        return TargetAry;
    }

    #endregion --- BinaryAdaptive_LocalMean ---

    #region --- Smooth ---

    public static void Smooth(MIL_ID image, int iteration)
    {
        for (int i = 0; i < iteration - 1; i++)
        {
            MIL.MimConvolve(image, image, MIL.M_SMOOTH);
        }
    }

    #endregion --- Smooth ---

    #endregion --- Image Process ---

    #region --- 尋邊 ---

    #region --- FindBorlder ---

    public static double[] FindBorlder(FindBorlderType Type)
    {
        MIL_ID MilResult = MIL.M_NULL;
        // Type = 0 = Horizontal
        // Type = 1 = Vertical

        MIL.MmeasAllocMarker(MyMil.MilSystem, MIL.M_EDGE, MIL.M_DEFAULT, ref MilResult);
        MIL.MmeasSetMarker(MilResult, MIL.M_FILTER_SMOOTHNESS, 100, MIL.M_NULL);
        MIL.MmeasSetMarker(MilResult, MIL.M_EDGEVALUE_MIN, 0.7, MIL.M_NULL);
        MIL.MmeasSetMarker(MilResult, MIL.M_NUMBER, MIL.M_ALL, MIL.M_NULL);

        if (Type == FindBorlderType.Horizontal)
        {
            MIL.MmeasSetMarker(MilResult, MIL.M_POLARITY, MIL.M_NEGATIVE, MIL.M_NULL);

            MIL.MmeasSetMarker(MilResult, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
        }

        if (Type == FindBorlderType.Vertical)
        {
            MIL.MmeasSetMarker(MilResult, MIL.M_POLARITY, MIL.M_POSITIVE, MIL.M_NULL);

            MIL.MmeasSetMarker(MilResult, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
        }

        MIL_INT NumResults = 0;
        MIL.MmeasFindMarker(MIL.M_DEFAULT, MilGrayImage, MilResult, MIL.M_DEFAULT);
        MIL.MmeasGetResult(MilResult, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref NumResults);

        // Model position in x found.
        double[] X = new double[NumResults];
        // Model position in y found.
        double[] Y = new double[NumResults];

        MIL.MmeasGetResult(MilResult, MIL.M_POSITION_X, X);
        MIL.MmeasGetResult(MilResult, MIL.M_POSITION_Y, Y);

        if (NumResults != 0)
        {
            // Re-enable the overlay display after all annotations are done.
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);

            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
            MIL.MmeasDraw(MIL.M_DEFAULT, MilResult, MilTargetImage, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT);
        }

        // Free
        MIL.MmeasFree(MilResult);
        MilResult = MIL.M_NULL;

        double[] Result = new double[] { };

        if (Type == FindBorlderType.Horizontal)
            Result = Y;
        if (Type == FindBorlderType.Vertical)
            Result = X;

        return Result;
    }

    #endregion --- FindBorlder ---

    #endregion --- 尋邊 ---

    #region --- Auto Target ---

    #region --- PixelInfo ---

    public class PixelInfo
    {
        public int X;
        public int Y;
        public int MaxPixel;

        public PixelInfo()
        {
            X = 0;
            Y = 0;
            MaxPixel = 0;
        }
    }

    #endregion --- PixelInfo ---

    #region --- AutoRoiLargeArea ---

    public static int AutoRoiLargeArea(MIL_ID img, int roiWidth, int roiHeight, double area1, double area2, ref int[] result)
    {
        result = new int[4];
        int resultCode = 0;
        double mainArea = 0.0;
        double subArea = 0.0;
        double mainBoxX1 = 0.0;
        double mainBoxX2 = 0.0;
        double mainBoxY1 = 0.0;
        double mainBoxY2 = 0.0;
        double boxX1 = 0.0;
        double boxX2 = 0.0;
        double boxY1 = 0.0;
        double boxY2 = 0.0;
        double gravityX1 = 0.0;
        double gravityY1 = 0.0;
        double gravityX2 = 0.0;
        double gravityY2 = 0.0;
        double maxGrayValue = 0.0;

        MIL_INT totalNumber1 = 0;
        MIL_INT totalNumber2 = 0;
        MIL_INT totalNumber3 = -1;
        MIL_INT bit = 0;
        MIL_INT sizeX = 0;
        MIL_INT sizeY = 0;
        // Blob
        MIL_ID blobContext1 = MIL.M_NULL;
        MIL_ID blobResult1 = MIL.M_NULL;
        MIL_ID blobContext2 = MIL.M_NULL;
        MIL_ID blobResult2 = MIL.M_NULL;
        MIL_ID blobContext3 = MIL.M_NULL;
        MIL_ID blobResult3 = MIL.M_NULL;
        // Graphic
        MIL_ID graphicsContext = MIL.M_NULL;
        // MIL Image
        MIL_ID mimBinarizedestination = MIL.M_NULL;
        MIL_ID mimBinarizedestination2 = MIL.M_NULL;
        MIL_ID mimArithdestination = MIL.M_NULL;
        MIL_ID mblobDrawdestination = MIL.M_NULL;
        MIL_ID MbufCopyColor2ddestination = MIL.M_NULL;

        try
        {
            // 取得原圖資訊
            MIL.MbufInquire(img, MIL.M_SIZE_X, ref sizeX);
            MIL.MbufInquire(img, MIL.M_SIZE_Y, ref sizeY);
            MIL.MbufInquire(img, MIL.M_SIZE_BIT, ref bit);
            maxGrayValue = Math.Pow(2, Convert.ToDouble((int)bit));

            // allocate all
            MIL.MbufClone(img, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mimBinarizedestination);
            MIL.MbufClone(img, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mimBinarizedestination2);
            MIL.MbufClone(img, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mimArithdestination);
            MIL.MbufAllocColor(MyMil.MilSystem, 1, sizeX, sizeY, bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mblobDrawdestination);
            MIL.MbufAllocColor(MyMil.MilSystem, 1, roiWidth, roiHeight, bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MbufCopyColor2ddestination);

            // Blob
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext1);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext2);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext3);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult1);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult2);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult3);
            // Graphic
            MIL.MgraAlloc(MyMil.MilSystem, ref graphicsContext);

            // blob context: 找出最大面積的亮區
            MIL.MblobControl(blobContext1, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(blobContext1, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
            MIL.MblobControl(blobContext1, MIL.M_SORT1, MIL.M_AREA);
            MIL.MblobControl(blobContext1, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            // blob context2 : 在亮區裡面找灰階最高的區域
            MIL.MblobControl(blobContext2, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(blobContext2, MIL.M_MEAN_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(blobContext2, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
            MIL.MblobControl(blobContext2, MIL.M_SORT1, MIL.M_MEAN_PIXEL);
            MIL.MblobControl(blobContext2, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            // blob context3 : 在ROI裡檢查是否有暗區
            MIL.MblobControl(blobContext3, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(blobContext3, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
            MIL.MblobControl(blobContext3, MIL.M_SORT1, MIL.M_AREA);
            MIL.MblobControl(blobContext3, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            // 自動二值化 找出最大面積的區域
            MIL.MimBinarize(img, mimBinarizedestination, MIL.M_BIMODAL + MIL.M_GREATER, 0.0, maxGrayValue);
            MIL.MblobCalculate(blobContext1, mimBinarizedestination, img, blobResult1);
            MIL.MblobGetResult(blobResult1, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber1);

            #region 沒找到區域回傳 -1

            if (totalNumber1 <= 0)
            {
                // Blob
                MIL.MblobFree(blobResult1); blobResult1 = MIL.M_NULL;
                MIL.MblobFree(blobContext1); blobContext1 = MIL.M_NULL;
                MIL.MblobFree(blobResult2); blobResult2 = MIL.M_NULL;
                MIL.MblobFree(blobContext2); blobContext2 = MIL.M_NULL;
                MIL.MblobFree(blobResult3); blobResult3 = MIL.M_NULL;
                MIL.MblobFree(blobContext3); blobContext3 = MIL.M_NULL;
                // Graphic
                MIL.MgraFree(graphicsContext); graphicsContext = MIL.M_NULL;
                // MIL Image
                MilNetHelper.MilBufferFree(ref mimBinarizedestination);
                MilNetHelper.MilBufferFree(ref mimBinarizedestination2);
                MilNetHelper.MilBufferFree(ref mimArithdestination);
                MilNetHelper.MilBufferFree(ref mblobDrawdestination);
                MilNetHelper.MilBufferFree(ref MbufCopyColor2ddestination);

                return -1;
            }

            #endregion 沒找到區域回傳 -1

            #region 沒找到足夠大的區域回傳 -2

            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_AREA + MIL.M_TYPE_DOUBLE, ref mainArea);
            if (mainArea < area1)
            {
                // Blob
                MIL.MblobFree(blobResult1); blobResult1 = MIL.M_NULL;
                MIL.MblobFree(blobContext1); blobContext1 = MIL.M_NULL;
                MIL.MblobFree(blobResult2); blobResult2 = MIL.M_NULL;
                MIL.MblobFree(blobContext2); blobContext2 = MIL.M_NULL;
                MIL.MblobFree(blobResult3); blobResult3 = MIL.M_NULL;
                MIL.MblobFree(blobContext3); blobContext3 = MIL.M_NULL;
                // Graphic
                MIL.MgraFree(graphicsContext); graphicsContext = MIL.M_NULL;
                // MIL Image
                MilNetHelper.MilBufferFree(ref mimBinarizedestination);
                MilNetHelper.MilBufferFree(ref mimBinarizedestination2);
                MilNetHelper.MilBufferFree(ref mimArithdestination);
                MilNetHelper.MilBufferFree(ref mblobDrawdestination);
                MilNetHelper.MilBufferFree(ref MbufCopyColor2ddestination);

                return -2;
            }

            #endregion 沒找到足夠大的區域回傳 -2

            // 取得最大面積區域邊界 & 重心位置
            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref mainBoxX1);
            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref mainBoxX2);
            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref mainBoxY1);
            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref mainBoxY2);

            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_CENTER_OF_GRAVITY_X + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref gravityX1);
            MIL.MblobGetResult(blobResult1, MIL.M_BLOB_INDEX(0), MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref gravityY1);

            // 把最大面積的區域畫出來
            MIL.MgraColor(graphicsContext, 1.0);
            MIL.MbufClear(mblobDrawdestination, 0.0);
            MIL.MblobDraw(graphicsContext, blobResult1, mblobDrawdestination, MIL.M_DRAW_BLOBS, MIL.M_BLOB_INDEX(0), MIL.M_DEFAULT);
            MIL.MimArith(mblobDrawdestination, img, mimArithdestination, MIL.M_MULT);

            // 比例二值化 濾出大面積區域較亮的區域
            MIL.MimBinarize(mimArithdestination, mimBinarizedestination2, MIL.M_PERCENTILE_VALUE + MIL.M_GREATER, 95.0, MIL.M_NULL);

            // 濾除雜訊
            MIL.MimErode(mimBinarizedestination2, mimBinarizedestination2, 5, MIL.M_BINARY);
            MIL.MimDilate(mimBinarizedestination2, mimBinarizedestination2, 5, MIL.M_BINARY);

            // 找出所有亮區裡最亮的區域
            MIL.MblobCalculate(blobContext2, mimBinarizedestination2, mimArithdestination, blobResult2);
            MIL.MblobGetResult(blobResult2, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber2);

            #region 沒找到區域回傳 -3

            if (totalNumber2 <= 0)
            {
                // 用比例應該不會沒找到?
                return -3;
            }

            #endregion 沒找到區域回傳 -3

            #region 沒找到足夠大的區域回傳 -4, result 寫入最大區域重心位置

            MIL.MblobGetResult(blobResult2, MIL.M_BLOB_INDEX(0), MIL.M_AREA + MIL.M_TYPE_DOUBLE, ref subArea);
            if (subArea < area2)
            {
                // ROI尺寸 奇偶數處理
                if (roiWidth % 2 == 0)
                {
                    result[0] = Convert.ToInt32(gravityX1) - roiWidth / 2;
                    result[1] = Convert.ToInt32(gravityX1) + roiWidth / 2 - 1;
                }
                else
                {
                    result[0] = Convert.ToInt32(gravityX1) - (roiWidth - 1) / 2;
                    result[1] = Convert.ToInt32(gravityX1) + (roiWidth - 1) / 2;
                }

                if (roiHeight % 2 == 0)
                {
                    result[2] = Convert.ToInt32(gravityY1) - roiHeight / 2;
                    result[3] = Convert.ToInt32(gravityY1) + roiHeight / 2 - 1;
                }
                else
                {
                    result[2] = Convert.ToInt32(gravityY1) - (roiHeight - 1) / 2;
                    result[3] = Convert.ToInt32(gravityY1) + (roiHeight - 1) / 2;
                }

                return -4;
            }

            #endregion 沒找到足夠大的區域回傳 -4, result 寫入最大區域重心位置

            MIL.MblobGetResult(blobResult2, MIL.M_BLOB_INDEX(0), MIL.M_CENTER_OF_GRAVITY_X + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref gravityX2);
            MIL.MblobGetResult(blobResult2, MIL.M_BLOB_INDEX(0), MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref gravityY2);

            // ROI尺寸 奇偶數處理
            if (roiWidth % 2 == 0)
            {
                result[0] = Convert.ToInt32(gravityX2) - roiWidth / 2;
                result[1] = Convert.ToInt32(gravityX2) + roiWidth / 2 - 1;
            }
            else
            {
                result[0] = Convert.ToInt32(gravityX2) - (roiWidth - 1) / 2;
                result[1] = Convert.ToInt32(gravityX2) + (roiWidth - 1) / 2;
            }

            if (roiHeight % 2 == 0)
            {
                result[2] = Convert.ToInt32(gravityY2) - roiHeight / 2;
                result[3] = Convert.ToInt32(gravityY2) + roiHeight / 2 - 1;
            }
            else
            {
                result[2] = Convert.ToInt32(gravityY2) - (roiHeight - 1) / 2;
                result[3] = Convert.ToInt32(gravityY2) + (roiHeight - 1) / 2;
            }

            while (totalNumber3 != 0)
            {
                MIL.MbufClear(MbufCopyColor2ddestination, 0.0);
                MIL.MbufCopyColor2d(mblobDrawdestination,
                                    MbufCopyColor2ddestination,
                                    MIL.M_ALL_BANDS,
                                    result[0],
                                    result[2],
                                    MIL.M_ALL_BANDS,
                                    0,
                                    0,
                                    roiWidth,
                                    roiHeight);

                // 定值二值化 找出不在最大面積區域的區域
                MIL.MimBinarize(MbufCopyColor2ddestination, MbufCopyColor2ddestination, MIL.M_FIXED + MIL.M_GREATER, 0.0, MIL.M_NULL);
                MIL.MimArith(MbufCopyColor2ddestination, MIL.M_NULL, MbufCopyColor2ddestination, MIL.M_NOT);
                MIL.MblobCalculate(blobContext3, MbufCopyColor2ddestination, MIL.M_NULL, blobResult3);
                MIL.MblobGetResult(blobResult3, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber3);

                if (totalNumber3 == 0) break;

                // ROI尺寸 邊界補償
                MIL.MblobGetResult(blobResult3, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_DOUBLE, ref boxX1);
                MIL.MblobGetResult(blobResult3, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_DOUBLE, ref boxX2);
                MIL.MblobGetResult(blobResult3, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_DOUBLE, ref boxY1);
                MIL.MblobGetResult(blobResult3, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_DOUBLE, ref boxY2);

                if (boxX1 == 0 && boxX2 != roiWidth - 1)
                {
                    result[0] += Convert.ToInt32(boxX2 - boxX1 + 1);
                    result[1] += Convert.ToInt32(boxX2 - boxX1 + 1);
                }
                else if (boxX2 == roiWidth - 1 && boxX1 != 0)
                {
                    result[0] -= Convert.ToInt32(boxX2 - boxX1 + 1);
                    result[1] -= Convert.ToInt32(boxX2 - boxX1 + 1);
                }

                if (boxY1 == 0 && boxY2 != roiHeight - 1)
                {
                    result[2] += Convert.ToInt32(boxY2 - boxY1 + 1);
                    result[3] += Convert.ToInt32(boxY2 - boxY1 + 1);
                }
                else if (boxY2 == roiHeight - 1 && boxY1 != 0)
                {
                    result[2] -= Convert.ToInt32(boxY2 - boxY1 + 1);
                    result[3] -= Convert.ToInt32(boxY2 - boxY1 + 1);
                }
                totalNumber3--;
            }

            resultCode = 1;
        }
        catch
        {
        }
        finally
        {
            // Blob
            if (blobResult1 != MIL.M_NULL)
            {
                MIL.MblobFree(blobResult1);
                blobResult1 = MIL.M_NULL;
            }
            if (blobContext1 != MIL.M_NULL)
            {
                MIL.MblobFree(blobContext1);
                blobContext1 = MIL.M_NULL;
            }

            if (blobResult2 != MIL.M_NULL)
            {
                MIL.MblobFree(blobResult2);
                blobResult2 = MIL.M_NULL;
            }
            if (blobContext2 != MIL.M_NULL)
            {
                MIL.MblobFree(blobContext2);
                blobContext2 = MIL.M_NULL;
            }

            if (blobResult3 != MIL.M_NULL)
            {
                MIL.MblobFree(blobResult3);
                blobResult3 = MIL.M_NULL;
            }
            if (blobContext3 != MIL.M_NULL)
            {
                MIL.MblobFree(blobContext3);
                blobContext3 = MIL.M_NULL;
            }

            // Graphic
            if (blobResult1 != MIL.M_NULL)
            {
                MIL.MgraFree(graphicsContext);
                graphicsContext = MIL.M_NULL;
            }

            // MIL Image
            MilNetHelper.MilBufferFree(ref mimBinarizedestination);
            MilNetHelper.MilBufferFree(ref mimBinarizedestination2);
            MilNetHelper.MilBufferFree(ref mimArithdestination);
            MilNetHelper.MilBufferFree(ref mblobDrawdestination);
            MilNetHelper.MilBufferFree(ref MbufCopyColor2ddestination);
        }
        return resultCode;
    }

    #endregion --- AutoRoiLargeArea ---

    #region --- AutoRoiBrightestPoint ---
    public static PixelInfo AutoRoiBrightestPoint(MIL_ID img, double grayBypassPercent = 0.0)
    {
        PixelInfo maxPixel = new PixelInfo();
        int totalNumber1 = -1;
        int totalNumber2 = -1;

        MIL_INT bit = 0;
        MIL_INT sizeX = 0;
        MIL_INT sizeY = 0;

        MIL_ID Gra_Context = MIL.M_NULL;
        MIL_ID MimBinary = MIL.M_NULL;
        MIL_ID MimDrawBlob = MIL.M_NULL;
        MIL_ID BlobContext1 = MIL.M_NULL;
        MIL_ID BlobContext2 = MIL.M_NULL;
        MIL_ID BlobResult1 = MIL.M_NULL;
        MIL_ID BlobResult2 = MIL.M_NULL;

        try
        {// 取得原圖資訊
            MIL.MbufInquire(img, MIL.M_SIZE_X, ref sizeX);
            MIL.MbufInquire(img, MIL.M_SIZE_Y, ref sizeY);
            MIL.MbufInquire(img, MIL.M_SIZE_BIT, ref bit);

            MIL.MbufClone(img, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimBinary);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext1);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext2);
            MIL.MbufAllocColor(MyMil.MilSystem, 1, sizeX, sizeY, bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MimDrawBlob);

            // Post-Alloc Block for MblobDraw's destination
            MIL.MbufClear(MimDrawBlob, 0.0);

            MIL.MgraAlloc(MyMil.MilSystem, ref Gra_Context);

            // Post-Alloc Block for Gra Context
            MIL.MgraColor(Gra_Context, 1.0);

            // Control Block for Blob Context
            MIL.MblobControl(BlobContext1, MIL.M_BLOB_CONTRAST, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_MAX_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_MEAN_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_SORT1, MIL.M_MAX_PIXEL);
            MIL.MblobControl(BlobContext1, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            MIL.MblobControl(BlobContext2, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext2, MIL.M_MAX_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext2, MIL.M_SORT1, MIL.M_MAX_PIXEL);
            MIL.MblobControl(BlobContext2, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult1);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult2);

            // 亮度在前1%的點
            MIL.MimBinarize(img, MimBinary, MIL.M_PERCENTILE_VALUE + MIL.M_IN_RANGE, 99 - grayBypassPercent, 100 - grayBypassPercent);
            MIL.MblobCalculate(BlobContext1, MimBinary, img, BlobResult1);
            MIL.MblobGetResult(BlobResult1, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber1);

            if (totalNumber1 > 0)
            {
                for (int i = 0; i < totalNumber1; i++)
                {
                    double boxX1 = 0.0;
                    double boxY1 = 0.0;

                    MIL.MblobGetResult(BlobResult1, MIL.M_BLOB_INDEX(i), MIL.M_MAX_PIXEL + MIL.M_TYPE_MIL_INT, ref maxPixel.MaxPixel);
                    MIL.MblobDraw(Gra_Context, BlobResult1, MimDrawBlob, MIL.M_DRAW_BLOBS, MIL.M_BLOB_INDEX(i), MIL.M_DEFAULT);
                    MIL.MimArith(MimDrawBlob, img, MimDrawBlob, MIL.M_MULT);
                    MIL.MimBinarize(MimDrawBlob, MimDrawBlob, MIL.M_FIXED + MIL.M_EQUAL, maxPixel.MaxPixel, MIL.M_NULL);
                    MIL.MblobCalculate(BlobContext2, MimDrawBlob, img, BlobResult2);
                    MIL.MblobGetResult(BlobResult2, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber2);

                    if (totalNumber2 > 0)
                    {
                        MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxX1);
                        MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxY1);
                        maxPixel.X = (int)boxX1;
                        maxPixel.Y = (int)boxY1;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                MIL.MimBinarize(img, MimBinary, MIL.M_BIMODAL + MIL.M_LESS_OR_EQUAL, MIL.M_NULL, MIL.M_NULL);
                MIL.MblobCalculate(BlobContext1, MimBinary, img, BlobResult1);
                MIL.MblobGetResult(BlobResult1, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber1);

                for (int i = 0; i < totalNumber1; i++)
                {
                    double boxX1 = 0.0;
                    double boxY1 = 0.0;

                    MIL.MblobGetResult(BlobResult1, MIL.M_BLOB_INDEX(i), MIL.M_MAX_PIXEL + MIL.M_TYPE_MIL_INT, ref maxPixel.MaxPixel);
                    MIL.MblobDraw(Gra_Context, BlobResult1, MimDrawBlob, MIL.M_DRAW_BLOBS, MIL.M_BLOB_INDEX(i), MIL.M_DEFAULT);
                    MIL.MimArith(MimDrawBlob, img, MimDrawBlob, MIL.M_MULT);
                    MIL.MimBinarize(MimDrawBlob, MimDrawBlob, MIL.M_FIXED + MIL.M_EQUAL, maxPixel.MaxPixel, MIL.M_NULL);
                    MIL.MblobCalculate(BlobContext2, MimDrawBlob, img, BlobResult2);
                    MIL.MblobGetResult(BlobResult2, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber2);

                    if (totalNumber2 > 0)
                    {
                        MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxX1);
                        MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxY1);
                        maxPixel.X = (int)boxX1;
                        maxPixel.Y = (int)boxY1;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DialogResult resul = System.Windows.Forms.MessageBox.Show($"{ex.Message}");
        }
        finally
        {
            if (BlobResult1 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobResult1);
                BlobResult1 = MIL.M_NULL;
            }

            if (BlobContext1 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobContext1);
                BlobContext1 = MIL.M_NULL;
            }

            if (BlobResult2 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobResult2);
                BlobResult2 = MIL.M_NULL;
            }

            if (BlobContext2 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobContext2);
                BlobContext2 = MIL.M_NULL;
            }

            if (Gra_Context != MIL.M_NULL)
            {
                MIL.MgraFree(Gra_Context);
                Gra_Context = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref MimDrawBlob);
            MilNetHelper.MilBufferFree(ref MimBinary);
        }

        return maxPixel;
    }
    #endregion

    #region --- AutoRoiBrightestPoint ---
    public static PixelInfo AutoRoiBrightestPoint(MIL_ID img, System.Drawing.Rectangle roi, double grayBypassPercent = 0.0)
    {
        PixelInfo maxPixel = new PixelInfo();
        int totalNumber1 = -1;
        int totalNumber2 = -1;

        MIL_INT bit = 0;
        MIL_INT sizeX = 0;
        MIL_INT sizeY = 0;

        MIL_ID spotRoiImage = MIL.M_NULL;
        MIL_ID Gra_Context = MIL.M_NULL;
        MIL_ID MimBinary = MIL.M_NULL;
        MIL_ID MimDrawBlob = MIL.M_NULL;
        MIL_ID BlobContext1 = MIL.M_NULL;
        MIL_ID BlobContext2 = MIL.M_NULL;
        MIL_ID BlobResult1 = MIL.M_NULL;
        MIL_ID BlobResult2 = MIL.M_NULL;

        try
        {
            MIL.MbufClone(img, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref spotRoiImage);
            MIL.MbufClear(spotRoiImage, 0.0);
            MIL.MbufCopy(img, spotRoiImage);
            SpotROI(roi, ref spotRoiImage);

            // 取得原圖資訊
            MIL.MbufInquire(spotRoiImage, MIL.M_SIZE_X, ref sizeX);
            MIL.MbufInquire(spotRoiImage, MIL.M_SIZE_Y, ref sizeY);
            MIL.MbufInquire(spotRoiImage, MIL.M_SIZE_BIT, ref bit);

            MIL.MbufClone(spotRoiImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimBinary);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext1);
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext2);
            MIL.MbufAllocColor(MyMil.MilSystem, 1, sizeX, sizeY, bit + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MimDrawBlob);

            MIL.MgraAlloc(MyMil.MilSystem, ref Gra_Context);

            // Post-Alloc Block for Gra Context
            MIL.MgraColor(Gra_Context, 1.0);

            // Control Block for Blob Context
            MIL.MblobControl(BlobContext1, MIL.M_BLOB_CONTRAST, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_MAX_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_MEAN_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext1, MIL.M_SORT1, MIL.M_MAX_PIXEL);
            MIL.MblobControl(BlobContext1, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            MIL.MblobControl(BlobContext2, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext2, MIL.M_MAX_PIXEL, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext2, MIL.M_SORT1, MIL.M_MAX_PIXEL);
            MIL.MblobControl(BlobContext2, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);

            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult1);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult2);

            // 亮度在前1%的點
            MIL.MimBinarize(spotRoiImage, MimBinary, MIL.M_PERCENTILE_VALUE + MIL.M_IN_RANGE, 99 - grayBypassPercent, 100 - grayBypassPercent);
            MIL.MblobCalculate(BlobContext1, MimBinary, spotRoiImage, BlobResult1);
            MIL.MblobGetResult(BlobResult1, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber1);

            for (int i = 0; i < totalNumber1; i++)
            {
                double boxX1 = 0.0;
                double boxY1 = 0.0;

                MIL.MblobGetResult(BlobResult1, MIL.M_BLOB_INDEX(i), MIL.M_MAX_PIXEL + MIL.M_TYPE_MIL_INT, ref maxPixel.MaxPixel);
                MIL.MblobDraw(Gra_Context, BlobResult1, MimDrawBlob, MIL.M_DRAW_BLOBS, MIL.M_BLOB_INDEX(i), MIL.M_DEFAULT);
                MIL.MimArith(MimDrawBlob, spotRoiImage, MimDrawBlob, MIL.M_MULT);
                MIL.MimBinarize(MimDrawBlob, MimDrawBlob, MIL.M_FIXED + MIL.M_EQUAL, maxPixel.MaxPixel, MIL.M_NULL);
                MIL.MblobCalculate(BlobContext2, MimDrawBlob, spotRoiImage, BlobResult2);
                MIL.MblobGetResult(BlobResult2, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref totalNumber2);

                if (totalNumber2 > 0)
                {
                    MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxX1);
                    MIL.MblobGetResult(BlobResult2, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_GRAYSCALE + MIL.M_TYPE_MIL_DOUBLE, ref boxY1);
                    maxPixel.X = (int)boxX1;
                    maxPixel.Y = (int)boxY1;
                    break;
                }
                else
                {
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
        finally
        {
            if (BlobResult1 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobResult1);
                BlobResult1 = MIL.M_NULL;
            }

            if (BlobContext1 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobContext1);
                BlobContext1 = MIL.M_NULL;
            }

            if (BlobResult2 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobResult2);
                BlobResult2 = MIL.M_NULL;
            }

            if (BlobContext2 != MIL.M_NULL)
            {
                MIL.MblobFree(BlobContext2);
                BlobContext2 = MIL.M_NULL;
            }

            if (Gra_Context != MIL.M_NULL)
            {
                MIL.MgraFree(Gra_Context);
                Gra_Context = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref MimDrawBlob);
            MilNetHelper.MilBufferFree(ref MimBinary);
            MilNetHelper.MilBufferFree(ref spotRoiImage);
        }

        return maxPixel;
    }
    #endregion

    #endregion --- Auto Target ---

    #region --- Auto Exposure ---

    #region --- AutoExposure ---

    //public static double AutoExposure(MIL_ID InputImg, int OrgExposureTime, int Max_ExposureTime, double TargetGray, double SmallErrorRange)
    //{
    //    double ResultExposureTime = 0;

    //    int OverCount = 10;

    //    double Dig_ExposureTime = 0;
    //    double GrayMean = 0;
    //    double ErrorRange = 0;
    //    double ErrorRatio = 0;

    //    double Exp_Last = 0;
    //    double Exp_Tmp = 0;
    //    double Mean_Last = 0;
    //    double Mean_Tmp = 0;
    //    double Exp_Next = 0;

    //    int OverLimit = 3;
    //    int OverLowLimit = 3;

    //    Dig_ExposureTime = OrgExposureTime * 1000.0;
    //    MIL.MdigControl(Mil_Digitizer, MIL.M_TIMER_DURATION + MIL.M_TIMER1, Dig_ExposureTime);

    //    Thread.Sleep(100);

    //    try
    //    {
    //        do
    //        {
    //            GrayMean = Get_GrayMean(InputImg, "MEAN");
    //            ErrorRatio = 1 - (GrayMean / TargetGray);
    //            ErrorRange = Math.Abs(ErrorRatio) * 100;

    //            if (ErrorRatio <= SmallErrorRange)
    //            {
    //                ResultExposureTime = Math.Round((Dig_ExposureTime / 1000), 0);

    //                if (ResultExposureTime <= 1000)
    //                { //維持原來的曝光時間 , 2011/04/06 Rick add
    //                    ResultExposureTime = 1000;
    //                }
    //                else if (ResultExposureTime >= Max_ExposureTime)
    //                {
    //                    ResultExposureTime = Max_ExposureTime;
    //                }
    //                break;
    //                //return ResultExposureTime;
    //            }
    //            else
    //            {
    //                if (Exp_Last == 0 || Mean_Last == 0) //1st
    //                {
    //                    Mean_Last = GrayMean;
    //                    Exp_Last = Dig_ExposureTime;

    //                    if (TargetGray > GrayMean) //目標>當前
    //                    {
    //                        Dig_ExposureTime += 50000000; //50 ms
    //                        Exp_Tmp = Dig_ExposureTime;

    //                        if (Dig_ExposureTime > Max_ExposureTime * 1000)
    //                        {
    //                            Dig_ExposureTime = 2000000000; //2000ms
    //                            OverLimit--;
    //                        }
    //                    }
    //                    else //當前>目標
    //                    {
    //                        Dig_ExposureTime -= 50000000; //50 ms
    //                        if (Dig_ExposureTime < 16000000) //16 ms
    //                        {
    //                            Dig_ExposureTime = 16000000; //16 ms
    //                            OverLowLimit--;
    //                        }
    //                    }

    //                    MIL.MdigControl(Mil_Digitizer, MIL.M_TIMER_DURATION + MIL.M_TIMER1, Dig_ExposureTime);

    //                    Thread.Sleep(100);
    //                    OverCount--;
    //                }
    //                else
    //                {
    //                    Mean_Tmp = GrayMean;
    //                    Exp_Tmp = Dig_ExposureTime;

    //                    if (TargetGray < Mean_Tmp) //如果 目標<當前
    //                    {
    //                        if (Exp_Last == Exp_Tmp)
    //                        {
    //                            MessageBox.Show("[DynamicAdjExposureTime2] 目標Mean值小於最低曝光時間(16ms),請確認目標Mean值設定是否正確");
    //                            ResultExposureTime = 0;
    //                            break;

    //                        }

    //                        if (Mean_Tmp == Mean_Last)
    //                        {
    //                            Exp_Next = Exp_Tmp - 50000000; // -50sm

    //                            if (Exp_Next < 16000000)       // 16ms
    //                            {
    //                                Exp_Next = 16000000;       // 16ms
    //                                OverLowLimit = OverLowLimit - 1;
    //                            }

    //                            Dig_ExposureTime = Exp_Next;
    //                        }
    //                        else
    //                        {
    //                            if (Mean_Last > Mean_Tmp)
    //                            {
    //                                Exp_Next = (Exp_Last * (Mean_Tmp - TargetGray) + Exp_Tmp * (TargetGray - Mean_Last)) / (Mean_Tmp - Mean_Last);
    //                            }
    //                            else
    //                            {
    //                                Exp_Next = (Exp_Tmp * (Mean_Last - TargetGray) + Exp_Last * (TargetGray - Mean_Tmp)) / (Mean_Last - Mean_Tmp);
    //                            }

    //                            //若調整幅度大於+-50ms則調50ms
    //                            if (Exp_Next < 0 || Exp_Tmp - Exp_Next < 5000000)
    //                            {
    //                                Exp_Next = Exp_Tmp - 50000000;

    //                                if (Exp_Next < 16000000)
    //                                {
    //                                    Exp_Next = 16000000;
    //                                    OverLowLimit--;
    //                                }
    //                            }
    //                            else if (Exp_Next > Max_ExposureTime * 1000)
    //                            {
    //                                Exp_Next = Max_ExposureTime * 1000;
    //                                OverLimit--;
    //                            }
    //                            else if (Exp_Tmp - Exp_Next > 50000000)
    //                            {
    //                                Exp_Next = Exp_Tmp - 50000000;
    //                            }

    //                            Dig_ExposureTime = Exp_Next;

    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (Exp_Tmp == Exp_Last)
    //                        {
    //                            MessageBox.Show("[DynamicAdjExposureTime3] 目標Mean值大於最大曝光時間(" + Max_ExposureTime / 1000 + "ms),請確認目標Mean值設定是否正確");
    //                            ResultExposureTime = 0;
    //                            break;

    //                        }
    //                        if (Mean_Tmp == Mean_Last)
    //                        {
    //                            Exp_Next = Exp_Tmp + 50000000;

    //                            if (Exp_Next > Max_ExposureTime * 1000)
    //                            {
    //                                Exp_Next = Max_ExposureTime * 1000;
    //                                OverLimit--;
    //                            }

    //                            Dig_ExposureTime = Exp_Next;
    //                        }
    //                        else
    //                        {
    //                            //Exp_N = (this->m_Gray_Targe - Mean_L) * ((Exposure_L - Exposure_T) / (Mean_L - Mean_T)) + Exposure_L
    //                            //↑不是一行就解決的算式嗎??? ↓還寫下面這樣長幹嘛???
    //                            if (Mean_Tmp > Mean_Last)
    //                            {
    //                                Exp_Next = (Exp_Tmp * (TargetGray - Mean_Last) + Exp_Last * (Mean_Tmp - TargetGray)) / (Mean_Tmp - Mean_Last);
    //                            }
    //                            else
    //                            {
    //                                Exp_Next = (Exp_Last * (TargetGray - Mean_Tmp) + Exp_Tmp * (Mean_Last - TargetGray)) / (Mean_Last - Mean_Tmp);
    //                            }

    //                            if (Exp_Next < 16000000)               //16000 ms
    //                            {
    //                                Exp_Next = 16000000;               //16000 ms
    //                                OverLowLimit--;
    //                            }
    //                            else if (Exp_Next > 2000000000 || Exp_Next - Exp_Tmp < 5000000)    //2000000 ms , 5000ms
    //                            {
    //                                Exp_Next = Exp_Tmp + 50000000;  //5000ms

    //                                if (Exp_Next > Max_ExposureTime * 1000)
    //                                {
    //                                    Exp_Next = Max_ExposureTime * 1000;
    //                                    OverLimit--;
    //                                }

    //                            }
    //                            else if (Exp_Next - Exp_Tmp > 50000000)  //5000ms
    //                            {
    //                                Exp_Next = Exp_Tmp + 50000000;
    //                            }

    //                            Dig_ExposureTime = Exp_Next;
    //                        }
    //                    }

    //                    MIL.MdigControl(Mil_Digitizer, MIL.M_TIMER_DURATION + MIL.M_TIMER1, Dig_ExposureTime);

    //                    Thread.Sleep(100);

    //                    Mean_Last = Mean_Tmp;
    //                    Exp_Last = Exp_Tmp;
    //                    OverCount = OverCount - 1;

    //                }

    //                if (OverLimit == 0 || OverLowLimit == 0 || OverCount == 0)  //2011/09/21 Rick modify
    //                {
    //                    MessageBox.Show("已超過調整次數");

    //                    ResultExposureTime = 0;
    //                    break;
    //                }
    //            }
    //        } while (ErrorRange > Convert.ToDouble(SmallErrorRange));
    //        return ResultExposureTime;

    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.ToString());
    //        return 0;

    //    }
    //}

    //public static int Get_GrayMean(MIL_ID Img, string MethodType)
    //{
    //    MIL_ID ImageI = MIL.M_NULL;
    //    MIL_ID _Img_16U_OriResize_NonPage = MIL.M_NULL;

    //    try
    //    {
    //        MIL.MdigControl(Mil_Digitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);
    //        MIL.MdigControl(Mil_Digitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
    //        MIL.MdigControl(Mil_Digitizer, MIL.M_GRAB_EXPOSURE_SOURCE, MIL.M_SOFTWARE);
    //        MIL.MdigGrab(Mil_Digitizer, Img);
    //        MIL.MdigControl(Mil_Digitizer, MIL.M_GRAB_EXPOSURE + MIL.M_TIMER1, MIL.M_ACTIVATE);
    //        MIL.MdigGrabWait(Mil_Digitizer, MIL.M_GRAB_END);

    //        return CalculateCentralMean(Img, MethodType);
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }

    //}

    #endregion --- AutoExposure ---

    #region --- CalculateCentralMean ---

    public static int CalculateCentralMean(MIL_ID InputImg, string MethodType)
    {
        int RtnValue = 0;
        double CentralMean = 0;

        MIL_ID InputCenterImg = MIL.M_NULL;
        MIL_ID m_MeanStatisticResult = MIL.M_NULL;

        MIL_INT Size_X = 0;
        MIL_INT Size_Y = 0;

        try
        {
            Size_X = MIL.MbufInquire(InputImg, MIL.M_SIZE_X, ref Size_X);
            Size_Y = MIL.MbufInquire(InputImg, MIL.M_SIZE_Y, ref Size_Y);

            MIL.MbufChild2d(InputImg, Size_X / 3, Size_Y / 3, Size_X / 3, Size_Y / 3, ref InputCenterImg);
            MIL.MimAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref m_MeanStatisticResult);

            if (MethodType == "MEAN")
            {
                MIL_ID StatCntxId = MIL.MimAlloc(MyMil.MilSystem, MIL.M_STATISTICS_CONTEXT, MIL.M_DEFAULT, MIL.M_NULL);
                MIL_ID StatRstId = MIL.MimAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_STATISTICS_RESULT, MIL.M_NULL);

                MIL.MimControl(StatCntxId, MIL.M_STAT_MEAN, MIL.M_ENABLE);
                MIL.MimControl(StatCntxId, MIL.M_STAT_STANDARD_DEVIATION, MIL.M_ENABLE);
                MIL.MimStatCalculate(StatCntxId, InputImg, StatRstId, MIL.M_DEFAULT);
                MIL.MimGetResult(StatRstId, MIL.M_STAT_MEAN + MIL.M_TYPE_MIL_DOUBLE, ref CentralMean);

                //MIL.MimStat(InputCenterImg, m_MeanStatisticResult, MIL.M_MEAN, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                MIL.MimGetResult(m_MeanStatisticResult, MIL.M_MEAN + MIL.M_TYPE_FLOAT, ref CentralMean);
                RtnValue = (int)CentralMean;

                // Free
                if (StatCntxId != MIL.M_NULL)
                {
                    MIL.MimFree(StatCntxId);
                    StatCntxId = MIL.M_NULL;
                }

                if (StatRstId != MIL.M_NULL)
                {
                    MIL.MimFree(StatRstId);
                    StatRstId = MIL.M_NULL;
                }
            }

            // Free
            MilNetHelper.MilBufferFree(ref InputCenterImg);
            MilNetHelper.MilBufferFree(ref m_MeanStatisticResult);

            return RtnValue;
        }
        catch (Exception)
        {
            return 0;
            throw;
        }
    }

    #endregion --- CalculateCentralMean ---

    #endregion --- Auto Exposure ---

    #region --- Auto Focus ---

    #region --- EvaluateFocusScore ---

    public static int EvaluateFocusScore(MIL_ID image)
    {
        MIL_INT FocusScore = 0;
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

        return ((int)FocusScore);
    }

    #endregion --- EvaluateFocusScore ---

    public class DigHookUserData
    {
        public MIL_ID SourceImage;
        public MIL_ID FocusImage;
        public MIL_ID Display;
        public int Iteration;
    }

    #region --- MoveLensHookFunction ---

    private static MIL_INT MoveLensHookFunction(MIL_INT HookType, MIL_INT Position, IntPtr UserDataHookPtr)
    {
        // this is how to check if the user data is null, the IntPtr class
        // contains a member, Zero, which exists solely for this purpose
        if (!IntPtr.Zero.Equals(UserDataHookPtr))
        {
            // get the handle to the DigHookUserData object back from the IntPtr
            GCHandle hUserData = GCHandle.FromIntPtr(UserDataHookPtr);

            // get a reference to the DigHookUserData object
            DigHookUserData UserData = hUserData.Target as DigHookUserData;

            // Here, the lens position must be changed according to the Position parameter.
            // In that case, we simulate the lens position change followed by a grab.
            if (HookType == MIL.M_CHANGE || HookType == MIL.M_ON_FOCUS)
            {
                MIL_INT FocusScore = 0;

                SimulateGrabFromCamera(UserData.SourceImage, UserData.FocusImage, (int)Position, UserData.Display);

                MIL.MdigFocus(MIL.M_NULL,
                              UserData.FocusImage,
                              MIL.M_DEFAULT,
                              null,
                              MIL.M_NULL,
                              MIL.M_DEFAULT,
                              MIL.M_DEFAULT,
                              MIL.M_DEFAULT,
                              MIL.M_DEFAULT,
                              MIL.M_EVALUATE,
                              ref FocusScore);

                // Draw position cursor.
                DrawCursor(MyMil.MilMainDisplay, Position, FocusScore);
                UserData.Iteration++;
            }
        }

        return 0;
    }

    #endregion --- MoveLensHookFunction ---

    private static int bestFocusPosition = -1;

    #region --- SimulateGrabFromCamera ---

    public static void SimulateGrabFromCamera(MIL_ID SourceImage, MIL_ID FocusImage, MIL_INT Iteration, MIL_ID AnnotationDisplay)
    {
        int NbSmoothNeeded = 0;                 // Number of smooths needed.

        MIL_INT BufType = 0;                        // Buffer type.
        MIL_INT BufSizeX = 0;                       // Buffer size X.
        MIL_INT BufSizeY = 0;                       // Buffer size Y.
        int Smooth = 0;                         // Smooth index.
        MIL_ID TempBuffer = MIL.M_NULL;         // Temporary buffer.
        MIL_ID SourceOwnerSystem = MIL.M_NULL;  // Owner system of the source buffer.

        // Compute number of smooths needed to simulate focus.
        NbSmoothNeeded = (int)Math.Abs(Iteration - bestFocusPosition);

        // Buffer inquires.
        BufType = MIL.MbufInquire(FocusImage, MIL.M_TYPE, MIL.M_NULL);
        BufSizeX = MIL.MbufInquire(FocusImage, MIL.M_SIZE_X, MIL.M_NULL);
        BufSizeY = MIL.MbufInquire(FocusImage, MIL.M_SIZE_Y, MIL.M_NULL);

        if (NbSmoothNeeded == 0)
        {
            // Directly copy image source to destination.
            MIL.MbufCopy(SourceImage, FocusImage);
        }
        else if (NbSmoothNeeded == 1)
        {
            // Directly convolve image from source to destination.
            MIL.MimConvolve(SourceImage, FocusImage, MIL.M_SMOOTH);
        }
        else
        {
            SourceOwnerSystem = (MIL_ID)MIL.MbufInquire(SourceImage, MIL.M_OWNER_SYSTEM, MIL.M_NULL);

            // Allocate temporary buffer.
            MIL.MbufAlloc2d(SourceOwnerSystem, BufSizeX, BufSizeY, BufType, MIL.M_IMAGE + MIL.M_PROC, ref TempBuffer);

            // Perform first smooth.
            MIL.MimConvolve(SourceImage, TempBuffer, MIL.M_SMOOTH);

            // Perform smooths.
            for (Smooth = 1; Smooth < NbSmoothNeeded - 1; Smooth++)
            {
                MIL.MimConvolve(TempBuffer, TempBuffer, MIL.M_SMOOTH);
            }

            // Perform last smooth.
            MIL.MimConvolve(TempBuffer, FocusImage, MIL.M_SMOOTH);

            // Free temporary buffer.
            MilNetHelper.MilBufferFree(ref TempBuffer);
        }
    }

    #endregion --- SimulateGrabFromCamera ---

    #region --- DrawCursor ---

    private static void DrawCursor(MIL_ID AnnotationDisplay, MIL_INT Position, MIL_INT Score)
    {
        MIL_ID AnnotationImage = MIL.M_NULL;
        MIL_INT BufSizeX = 0;
        MIL_INT BufSizeY = 0;

        // Prepare for overlay annotations.
        MIL.MdispControl(AnnotationDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
        MIL.MdispControl(AnnotationDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);
        MIL.MdispInquire(AnnotationDisplay, MIL.M_OVERLAY_ID, ref AnnotationImage);
        MIL.MbufInquire(AnnotationImage, MIL.M_SIZE_X, ref BufSizeX);
        MIL.MbufInquire(AnnotationImage, MIL.M_SIZE_Y, ref BufSizeY);
        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);

        MIL.MgraControl(MIL.M_DEFAULT, MIL.M_FONT_X_SCALE, 10);
        MIL.MgraControl(MIL.M_DEFAULT, MIL.M_FONT_Y_SCALE, 10);

        MIL.MgraText(MIL.M_DEFAULT, AnnotationImage, 10, 10, $"Position: {Position},\t Focus Score: {Score}");
    }

    #endregion --- DrawCursor ---

    #endregion --- Auto Focus ---

    #region --- Hot Pixel ---

    #region --- HotPixel ---

    public static void HotPixel(ref MIL_ID SourceImg, int Pitch, double B_TH, double D_TH)
    {
        //PreProcCore.ImgPreProcCore PreProc = null;

        //try
        //{
        //    PreProc = new PreProcCore.ImgPreProcCore(MyMil.MilApplication, MyMil.MilSystem, 192);
        //    PreProc.FunCalHotPixelImg(SourceImg, B_TH, D_TH, Pitch, false);
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception(ex.Message);
        //}
    }

    #endregion --- HotPixel ---

    #endregion --- Hot Pixel ---

    #region --- FFC ---

    #region --- FFC ---

    public static void FFC(MIL_ID SourceImg, FfcCalibrationFilePath CalibrationFile, ref MIL_ID TargetImg)
    {
        MIL_ID MilFlatFieldContext = MIL.M_NULL;

        MIL_ID milOffset = MIL.M_NULL;
        MIL_ID milFlat = MIL.M_NULL;
        MIL_ID milDark = MIL.M_NULL;

        try
        {
            MilFlatFieldContext = MIL.MimAlloc(MyMil.MilSystem, MIL.M_FLAT_FIELD_CONTEXT, MIL.M_DEFAULT, MIL.M_NULL);

            MIL.MbufRestore(CalibrationFile.Offset, MyMil.MilSystem, ref milOffset);
            MIL.MbufRestore(CalibrationFile.Flat, MyMil.MilSystem, ref milFlat);
            MIL.MbufRestore(CalibrationFile.Dark, MyMil.MilSystem, ref milDark);

            MIL.MimControl(MilFlatFieldContext, MIL.M_GAIN_CONST, MIL.M_AUTOMATIC);
            MIL.MimControl(MilFlatFieldContext, MIL.M_OFFSET_IMAGE, milOffset);
            MIL.MimControl(MilFlatFieldContext, MIL.M_FLAT_IMAGE, milFlat);
            MIL.MimControl(MilFlatFieldContext, MIL.M_DARK_IMAGE, milDark);

            MIL.MimFlatField(MilFlatFieldContext, SourceImg, TargetImg, MIL.M_PREPROCESS);
            MIL.MimFlatField(MilFlatFieldContext, SourceImg, TargetImg, MIL.M_DEFAULT);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            if (MilFlatFieldContext != MIL.M_NULL)
            {
                MIL.MimFree(MilFlatFieldContext);
                MilFlatFieldContext = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref milOffset);
            MilNetHelper.MilBufferFree(ref milFlat);
            MilNetHelper.MilBufferFree(ref milDark);
        }
    }

    #endregion --- FFC ---

    #region --- FFC ---

    public static void FFC(MIL_ID SourceImg, MIL_ID offset, MIL_ID flat, MIL_ID dark, ref MIL_ID TargetImg)
    {
        MIL_ID MilFlatFieldContext = MIL.M_NULL;

        try
        {
            MilFlatFieldContext = MIL.MimAlloc(MyMil.MilSystem, MIL.M_FLAT_FIELD_CONTEXT, MIL.M_DEFAULT, MIL.M_NULL);

            MIL.MimControl(MilFlatFieldContext, MIL.M_GAIN_CONST, MIL.M_AUTOMATIC);
            MIL.MimControl(MilFlatFieldContext, MIL.M_OFFSET_IMAGE, offset);
            MIL.MimControl(MilFlatFieldContext, MIL.M_FLAT_IMAGE, flat);
            MIL.MimControl(MilFlatFieldContext, MIL.M_DARK_IMAGE, dark);

            MIL.MimFlatField(MilFlatFieldContext, SourceImg, TargetImg, MIL.M_PREPROCESS);
            MIL.MimFlatField(MilFlatFieldContext, SourceImg, TargetImg, MIL.M_DEFAULT);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            if (MilFlatFieldContext != MIL.M_NULL)
            {
                MIL.MimFree(MilFlatFieldContext);
                MilFlatFieldContext = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref offset);
            MilNetHelper.MilBufferFree(ref flat);
            MilNetHelper.MilBufferFree(ref dark);
        }
    }

    #endregion --- FFC ---

    #endregion --- FFC ---

    #region --- Combine Image ---

    public static void CombineImage(XyzFilePath CombineFile, string OutputPath, string FileName)
    {
        MIL_ID milSource = MIL.M_NULL;
        MIL_ID milSourceColor = MIL.M_NULL;

        MIL_INT ImgSizeX = 0;
        MIL_INT ImgSizeY = 0;
        MIL_INT ImgBits = 0;
        MIL_INT ImgSign = 0;
        MIL_INT ImgBand = 0;

        try
        {
            #region Proc Red Image

            MIL.MbufDiskInquire(CombineFile.X, MIL.M_SIZE_X, ref ImgSizeX);
            MIL.MbufDiskInquire(CombineFile.X, MIL.M_SIZE_Y, ref ImgSizeY);
            MIL.MbufDiskInquire(CombineFile.X, MIL.M_SIZE_BIT, ref ImgBits);
            MIL.MbufDiskInquire(CombineFile.X, MIL.M_SIGN, ref ImgSign);
            MIL.MbufDiskInquire(CombineFile.X, MIL.M_SIZE_BAND, ref ImgBand);

            // Allocate Single-Channel Image
            MIL.MbufAlloc2d(MIL.M_DEFAULT_HOST, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE, ref milSource);

            // Allocate Color Image
            MIL.MbufAllocColor(MIL.M_DEFAULT_HOST, 3, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE, ref milSourceColor);

            if (ImgBand != 1)
            {
                throw new Exception($"Unsupported Image Band {ImgBand}");
            }
            // TODO: 3-Channel Image
            //else if (ImgBand == 3)
            //{
            //    MIL.MbufAllocColor(MyMil.MilSystem, 3, ImgSizeX, ImgSizeY, ImgBits + MIL.M_UNSIGNED, MIL.M_IMAGE, milSource);
            //}

            MIL.MbufLoad(CombineFile.X, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_RED, 0, 0, ImgSizeX, ImgSizeY);

            #endregion Proc Red Image

            #region Proc Green Image

            // Check Format of Green Image
            if (ImgSizeX != MIL.MbufDiskInquire(CombineFile.Y, MIL.M_SIZE_X, MIL.M_NULL))
            {
                throw new Exception("Image G's sizeX was not equal to Image R's sizeX!");
            }
            if (ImgSizeY != MIL.MbufDiskInquire(CombineFile.Y, MIL.M_SIZE_Y, MIL.M_NULL))
            {
                throw new Exception("Image G's sizeY was not equal to Image R's sizeY!");
            }
            if (ImgBits != MIL.MbufDiskInquire(CombineFile.Y, MIL.M_SIZE_BIT, MIL.M_NULL))
            {
                throw new Exception("Image G's bits was not equal to Image R's bits!");
            }
            if (ImgSign != MIL.MbufDiskInquire(CombineFile.Y, MIL.M_SIGN, MIL.M_NULL))
            {
                throw new Exception("Image G's sign was not equal to Image R's sign!");
            }
            if (ImgBand != MIL.MbufDiskInquire(CombineFile.Y, MIL.M_SIZE_BAND, MIL.M_NULL))
            {
                throw new Exception("Image G's bands was not equal to Image R's bands!");
            }

            MIL.MbufLoad(CombineFile.Y, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_GREEN, 0, 0, ImgSizeX, ImgSizeY);

            #endregion Proc Green Image

            #region Proc Blue Image

            // Check Format of Green Image
            if (ImgSizeX != MIL.MbufDiskInquire(CombineFile.Z, MIL.M_SIZE_X, MIL.M_NULL))
            {
                throw new Exception("Image B's sizeX was not equal to Image R's sizeX!");
            }
            if (ImgSizeY != MIL.MbufDiskInquire(CombineFile.Z, MIL.M_SIZE_Y, MIL.M_NULL))
            {
                throw new Exception("Image B's sizeY was not equal to Image R's sizeY!");
            }
            if (ImgBits != MIL.MbufDiskInquire(CombineFile.Z, MIL.M_SIZE_BIT, MIL.M_NULL))
            {
                throw new Exception("Image B's bits was not equal to Image R's bits!");
            }
            if (ImgSign != MIL.MbufDiskInquire(CombineFile.Z, MIL.M_SIGN, MIL.M_NULL))
            {
                throw new Exception("Image B's sign was not equal to Image R's sign!");
            }
            if (ImgBand != MIL.MbufDiskInquire(CombineFile.Z, MIL.M_SIZE_BAND, MIL.M_NULL))
            {
                throw new Exception("Image B's bands was not equal to Image R's bands!");
            }

            MIL.MbufLoad(CombineFile.Z, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_BLUE, 0, 0, ImgSizeX, ImgSizeY);

            #endregion Proc Blue Image

            MIL.MbufExport(string.Format("{0}CI1.tif", OutputPath), MIL.M_TIFF, milSourceColor);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref milSource);
            MilNetHelper.MilBufferFree(ref milSourceColor);
        }
    }

    #endregion --- Combine Image ---

    #region --- MeasureLight ---

    #region --- MeasureLightCreate ---

    #region --- MeasureLightCreate ---
    public static void MeasureLightCreate(string ImgPathX, string ImgPathY, string ImgPathZ, bool[] xyzEnable, string OutputPah)
    {
        MIL_ID imgX = MIL.M_NULL;
        MIL_ID imgY = MIL.M_NULL;
        MIL_ID imgZ = MIL.M_NULL;

        MilColorImage colorImage = null;
        MeasureData correctionData = null;

        LightMeasurer lightMeasurer = null;
        MilColorImage coeffImage = null;

        try
        {
            // Import filter x, y ,z image for create
            MIL.MbufImport(
                ImgPathX,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgX);

            MIL.MbufImport(
                ImgPathY,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgY);

            MIL.MbufImport(
                ImgPathZ,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgZ);

            // Allocate MilColorImage
            colorImage = new MilColorImage(imgX, imgY, imgZ);

            // Allocate MeasureData for correction
            correctionData = new MeasureData();

            // Image Information
            correctionData.ImageInfo.Width = 6576;
            correctionData.ImageInfo.Height = 4384;

            // Measure Matrix
            correctionData.MeasureMatrixInfo.Row = 2;
            correctionData.MeasureMatrixInfo.Column = 2;

            // Exposure time for filter x, y ,z image
            correctionData.ExposureTime.X = 74822;
            correctionData.ExposureTime.Y = 67772;
            correctionData.ExposureTime.Z = 179332;

            // Measure position's four color correction data
            FourColorData p1 = new FourColorData();
            p1.Index = 1;
            p1.CieChroma.Luminance = 434;
            p1.CieChroma.Cx = 0.31894;
            p1.CieChroma.Cy = 0.36952;
            p1.CircleRegionInfo.CenterX = 384;
            p1.CircleRegionInfo.CenterY = 344;
            p1.CircleRegionInfo.Radius = 320;

            FourColorData p2 = new FourColorData();
            p2.Index = 2;
            p2.CieChroma.Luminance = 436;
            p2.CieChroma.Cx = 0.32958;
            p2.CieChroma.Cy = 0.35596;
            p2.CircleRegionInfo.CenterX = 6184;
            p2.CircleRegionInfo.CenterY = 344;
            p2.CircleRegionInfo.Radius = 320;

            FourColorData p3 = new FourColorData();
            p3.Index = 3;
            p3.CieChroma.Luminance = 439;
            p3.CieChroma.Cx = 0.30395;
            p3.CieChroma.Cy = 0.35365;
            p3.CircleRegionInfo.CenterX = 360;
            p3.CircleRegionInfo.CenterY = 3992;
            p3.CircleRegionInfo.Radius = 320;

            FourColorData p4 = new FourColorData();
            p4.Index = 4;
            p4.CieChroma.Luminance = 424;
            p4.CieChroma.Cx = 0.31968;
            p4.CieChroma.Cy = 0.34596;
            p4.CircleRegionInfo.CenterX = 6224;
            p4.CircleRegionInfo.CenterY = 4024;
            p4.CircleRegionInfo.Radius = 320;

            correctionData.DataList.Add(p1);
            correctionData.DataList.Add(p2);
            correctionData.DataList.Add(p3);
            correctionData.DataList.Add(p4);

            // Create correction data
            lightMeasurer = new LightMeasurer(MyMil.MilSystem);
            lightMeasurer.CreateCorrectionData(
                colorImage,
                xyzEnable,
                ref correctionData);

            // Create coefficient image
            lightMeasurer.CreateCoeffImage(
                correctionData,
                ref coeffImage);

            // Save correction data to xml
            LightMeasureConfig config = new LightMeasureConfig();
            config.CorrectionData = correctionData;
            config.WriteWithoutCrypto(
                string.Format("{0}\\CorrectionFile.xml", OutputPah), false);

            MIL.MbufSave(string.Format(
                "{0}\\CoeffX.tif", OutputPah), coeffImage.ImgX);
            MIL.MbufSave(string.Format(
                "{0}\\CoeffY.tif", OutputPah), coeffImage.ImgY);
            MIL.MbufSave(string.Format(
                "{0}\\CoeffZ.tif", OutputPah), coeffImage.ImgZ);
        }
        catch (Exception ex)
        {
            MilNetHelper.MilBufferFree(ref imgX);
            MilNetHelper.MilBufferFree(ref imgY);
            MilNetHelper.MilBufferFree(ref imgZ);

            throw new Exception(ex.ToString());
        }
        finally
        {
            if (coeffImage != null)
            {
                coeffImage.Free();
            }

            if (colorImage != null)
            {
                colorImage.Free();
            }
        }
    }
    #endregion

    #endregion --- MeasureLightCreate ---

    #region --- MeasureLightPredict ---

    public static void MeasureLightPredict(XyzFilePath PredictFile, XyzFilePath CorrectionFile, MeasureData predictData, bool[] xyzEnable, string OutputPath)
    {
        LightMeasurer measurer = null;
        MilColorImage colorImage = null;
        MilColorImage coeffImage = null;
        MilColorImage tristimulusImage = null;
        MilChromaImage chromaImage = null;

        try
        {
            measurer = new LightMeasurer(MyMil.MilSystem);

            LoadAndAllocTestColorImage(
                PredictFile.X,
                PredictFile.Y,
                PredictFile.Z,
                ref colorImage);

            LoadAndAllocTestColorImage(
                CorrectionFile.X,
                CorrectionFile.Y,
                CorrectionFile.Z,
                ref coeffImage);

            measurer.PredictChromaImage(
                colorImage,
                coeffImage,
                predictData,
                ref tristimulusImage,
                ref chromaImage);

            MIL.MbufSave(string.Format("{0}\\ImgTristimulusX.tif", OutputPath), tristimulusImage.ImgX);
            MIL.MbufSave(string.Format("{0}\\ImgTristimulusY.tif", OutputPath), tristimulusImage.ImgY);
            MIL.MbufSave(string.Format("{0}\\ImgTristimulusZ.tif", OutputPath), tristimulusImage.ImgZ);
            MIL.MbufSave(string.Format("{0}\\ImgChromaX.tif", OutputPath), chromaImage.ImgCx);
            MIL.MbufSave(string.Format("{0}\\ImgChromaY.tif", OutputPath), chromaImage.ImgCy);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format($"btnPredictChromaImage_Click Error - {ex.Message}"));
        }
        finally
        {
            if (colorImage != null)
            {
                colorImage.Free();
            }

            if (coeffImage != null)
            {
                coeffImage.Free();
            }

            if (tristimulusImage != null)
            {
                tristimulusImage.Free();
            }

            if (chromaImage != null)
            {
                chromaImage.Free();
            }
        }
    }

    #endregion --- MeasureLightPredict ---

    #region --- MeasureLightPredict ---
    public static void MeasureLightPredict(MilColorImage PredictFile,
                                           MilColorImage CorrectionFile,
                                           MeasureData predictData,
                                           ref MilColorImage tristimulusImage,
                                           ref MilChromaImage chromaImage)
    {
        LightMeasurer measurer = null;
        tristimulusImage = null;
        chromaImage = null;
        try
        {
            measurer = new LightMeasurer(MyMil.MilSystem);

            measurer.PredictChromaImage(PredictFile,
                                        CorrectionFile,
                                        predictData,
                                        ref tristimulusImage,
                                        ref chromaImage);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("btnPredictChromaImage_Click Error - {0}", ex.Message));
        }
        finally
        {
            if (PredictFile != null)
            {
                PredictFile.Free();
            }

            if (CorrectionFile != null)
            {
                CorrectionFile.Free();
            }
        }
    }
    #endregion

    #region --- MesureTristimulusYPredict ---
    public static void Mesure_TristimulusY_Predict(MilMonoImage PredictFile,
                                                    MilColorImage CorrectionFile,
                                                    MeasureData predictData,
                                                    ref MilColorImage tristimulusImage)
    {
        LightMeasurer measurer = null;
        tristimulusImage = null;
        try
        {
            measurer = new LightMeasurer(MyMil.MilSystem);

            measurer.PredictTristimulusYImage(PredictFile,
                                        CorrectionFile,
                                        predictData,
                                        ref tristimulusImage);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Mesure_TristimulusY_Predict Error - {0}", ex.Message));
        }
        finally
        {
            if (PredictFile != null)
            {
                PredictFile.Free();
            }

            if (CorrectionFile != null)
            {
                CorrectionFile.Free();
            }
        }

    }
    #endregion

    #region --- LoadAndAllocTestColorImage ---

    public static void LoadAndAllocTestColorImage(string pathX, string pathY, string pathZ, ref MilColorImage colorImage)
    {
        MIL_ID imgX = MIL.M_NULL;
        MIL_ID imgY = MIL.M_NULL;
        MIL_ID imgZ = MIL.M_NULL;

        try
        {
            MIL.MbufImport(
                pathX,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgX);

            MIL.MbufImport(
                pathY,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgY);

            MIL.MbufImport(
                pathZ,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgZ);

            colorImage = new MilColorImage(imgX, imgY, imgZ);
        }
        catch (Exception ex)
        {
            FreeTestImage(ref imgX, ref imgY, ref imgZ);

            throw ex;
        }
    }

    #endregion --- LoadAndAllocTestColorImage ---

    #region --- FreeTestImage ---

    private static void FreeTestImage(ref MIL_ID imgX, ref MIL_ID imgY, ref MIL_ID imgZ)
    {
        MilNetHelper.MilBufferFree(ref imgX);
        MilNetHelper.MilBufferFree(ref imgY);
        MilNetHelper.MilBufferFree(ref imgZ);
    }

    #endregion --- FreeTestImage ---

    #endregion --- MeasureLight ---

    #region "--- Color Mura ---"

    public static void ColorMura_ColorImg(ColorMura_DetectParam Param, String Image_Out_FolderPath)
    {
        MIL_INT ImgSizeX = 0;
        MIL_INT ImgSizeY = 0;
        MIL_INT ImgBits = 0;
        MIL_INT ImgSign = 0;
        MIL_INT ImgBand = 0;

        MIL_ID milSource = MIL.M_NULL;
        MIL_ID milSourceColor = MIL.M_NULL;
        MIL_ID milImg_32F_diff = MIL.M_NULL;
        MIL_ID milImg_16U_diff = MIL.M_NULL;

        MIL_ID mil_Numerator = MIL.M_NULL;
        MIL_ID mil_Denominator = MIL.M_NULL;
        MIL_ID milBlobAnalysis = MIL.M_NULL;
        MIL_ID milBlobResult = MIL.M_NULL;

        int numerator_idx = Param.Numerator_Channel;
        int denominator_idx = Param.Denominator_Channel;
        int areaOpen = Param.Open;
        int dilate = Param.Dilate;
        int minArea = Param.MinArea;
        int threshold_High = Param.ThresHigh;
        int threshold_Low = Param.ThresLow;
        bool Debug = false;

        try
        {
            // Load Color Image
            MIL.MbufDiskInquire(Param.ImgColor_Path, MIL.M_SIZE_X, ref ImgSizeX);
            MIL.MbufDiskInquire(Param.ImgColor_Path, MIL.M_SIZE_Y, ref ImgSizeY);
            MIL.MbufDiskInquire(Param.ImgColor_Path, MIL.M_SIZE_BIT, ref ImgBits);
            MIL.MbufDiskInquire(Param.ImgColor_Path, MIL.M_SIGN, ref ImgSign);
            MIL.MbufDiskInquire(Param.ImgColor_Path, MIL.M_SIZE_BAND, ref ImgBand);

            if (ImgBand != 3)
                throw new Exception("Band of Image is not 3!");

            // Color Image
            MIL.MbufAllocColor(MyMil.MilSystem, 3, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE + MIL.M_PROC, ref milSourceColor);
            MIL.MbufLoad(Param.ImgColor_Path, milSourceColor);

            //if (Debug)
            //{
            //	ImgPath = String::Format("{0}\\ColorMura-Img_ColorSource.tif", Image_Out_FolderPath);
            //	MbufExport(ConvSysStrToWChar(ImgPath), M_TIFF, milSourceColor);
            //}

            // Difference Image
            MIL.MbufAlloc2d(MyMil.MilSystem, ImgSizeX, ImgSizeY, 32 + MIL.M_FLOAT, MIL.M_PROC + MIL.M_IMAGE, ref milImg_32F_diff);
            MIL.MbufAlloc2d(MyMil.MilSystem, ImgSizeX, ImgSizeY, 16 + MIL.M_UNSIGNED, MIL.M_PROC + MIL.M_IMAGE + MIL.M_DISP, ref milImg_16U_diff);

            switch (numerator_idx)
            {
                case 0: // R
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_RED, MIL.M_NULL);
                    break;

                case 1: // G
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_GREEN, MIL.M_NULL);
                    break;

                case 2: // B
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_BLUE, MIL.M_NULL);
                    break;
            }

            switch (denominator_idx)
            {
                case 0: // R
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_RED, MIL.M_NULL);
                    break;

                case 1: // G
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_GREEN, MIL.M_NULL);
                    break;

                case 2: // B
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_BLUE, MIL.M_NULL);
                    break;
            }

            MIL.MimArith(mil_Numerator, mil_Denominator, milImg_32F_diff, MIL.M_DIV);

            //if (Debug)
            //{
            //	ImgPath = String::Format("{0}\\ColorMura-Img_32F_diff.tif", Image_Out_FolderPath);
            //	MbufExport(ConvSysStrToWChar(ImgPath), M_TIFF, milImg_32F_diff);
            //}

            MIL.MimArith(milImg_32F_diff, 4095, milImg_16U_diff, MIL.M_MULT_CONST + MIL.M_SATURATION);

            if (Debug)
            {
                Param.ImgColor_Path = string.Format("{0}\\[2-1]ColorMura-Img_16U_diff.tif", Image_Out_FolderPath);
                MIL.MbufExport(Param.ImgColor_Path, MIL.M_TIFF, milImg_16U_diff);
            }

            MIL.MimBinarize(milImg_16U_diff, milImg_16U_diff, MIL.M_IN_RANGE, threshold_Low, threshold_High);

            //
            MIL.MimMorphic(milImg_16U_diff, milImg_16U_diff, MIL.M_3X3_RECT, MIL.M_AREA_OPEN, areaOpen, MIL.M_BINARY);
            if (dilate > 0)
                MIL.MimDilate(milImg_16U_diff, milImg_16U_diff, dilate, MIL.M_BINARY);
            if (Debug)
            {
                Param.ImgColor_Path = string.Format("{0}\\[2-2]ColorMura-Img_16U_dilate.tif", Image_Out_FolderPath);
                MIL.MbufExport(Param.ImgColor_Path, MIL.M_TIFF, milImg_16U_diff);
            }

            // Filter Area
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobAnalysis);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobResult);

            MIL.MblobControl(milBlobAnalysis, MIL.M_ALL_FEATURES, MIL.M_DISABLE);
            MIL.MblobControl(milBlobAnalysis, MIL.M_BOX, MIL.M_ENABLE);

            MIL.MblobCalculate(milBlobAnalysis, milImg_16U_diff, MIL.M_NULL, milBlobResult);

            // Filter Blob
            MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, minArea, MIL.M_NULL);

            //this->ImgDraw_Blob(milImg_16U_diff);

            MIL.MbufClear(milImg_16U_diff, 0L);
            MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milImg_16U_diff, MIL.M_DRAW_BLOBS, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

            MIL.MbufExport(string.Format("{0}\\ColorMuraResult.tif", Image_Out_FolderPath), MIL.M_TIFF, milImg_16U_diff);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref milSource);

            // Child of milSourceColor
            MilNetHelper.MilBufferFree(ref mil_Numerator);

            // Child of milSourceColor
            MilNetHelper.MilBufferFree(ref mil_Denominator);

            MilNetHelper.MilBufferFree(ref milSourceColor);

            MilNetHelper.MilBufferFree(ref milImg_32F_diff);

            MilNetHelper.MilBufferFree(ref milImg_16U_diff);

            if (milBlobAnalysis != null)
            {
                MIL.MblobFree(milBlobAnalysis);
                milBlobAnalysis = MIL.M_NULL;
            }
            if (milBlobResult != null)
            {
                MIL.MblobFree(milBlobResult);
                milBlobResult = MIL.M_NULL;
            }
        }
    }

    public static void ColorMura(ColorMura_DetectParam Param, String Image_Out_FolderPath)
    {
        MIL_INT ImgSizeX = 0;
        MIL_INT ImgSizeY = 0;
        MIL_INT ImgBits = 0;
        MIL_INT ImgSign = 0;
        MIL_INT ImgBand = 0;

        MIL_ID milSource = MIL.M_NULL;
        MIL_ID milSourceColor = MIL.M_NULL;
        MIL_ID milImg_32F_diff = MIL.M_NULL;
        MIL_ID milImg_16U_diff = MIL.M_NULL;

        MIL_ID mil_Numerator = MIL.M_NULL;
        MIL_ID mil_Denominator = MIL.M_NULL;
        MIL_ID milBlobAnalysis = MIL.M_NULL;
        MIL_ID milBlobResult = MIL.M_NULL;

        int numerator_idx = Param.Numerator_Channel;
        int denominator_idx = Param.Denominator_Channel;
        int areaOpen = Param.Open;
        int dilate = Param.Dilate;
        int minArea = Param.MinArea;
        int threshold_High = Param.ThresHigh;
        int threshold_Low = Param.ThresLow;
        bool Debug = false;

        try
        {
            // Load Red Image
            if (!File.Exists(Param.ImgX_Path))
            {
                throw new Exception("Image R File Path Error !");
            }
            else
            {
                MIL.MbufDiskInquire(Param.ImgX_Path, MIL.M_SIZE_X, ref ImgSizeX);
                MIL.MbufDiskInquire(Param.ImgX_Path, MIL.M_SIZE_Y, ref ImgSizeY);
                MIL.MbufDiskInquire(Param.ImgX_Path, MIL.M_SIZE_BIT, ref ImgBits);
                MIL.MbufDiskInquire(Param.ImgX_Path, MIL.M_SIGN, ref ImgSign);
                MIL.MbufDiskInquire(Param.ImgX_Path, MIL.M_SIZE_BAND, ref ImgBand);

                if (ImgBand == 1)
                {
                    MIL.MbufAlloc2d(MyMil.MilSystem, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE + MIL.M_PROC, ref milSource);
                }
                else if (ImgBand == 3)
                {
                    MIL.MbufAllocColor(MyMil.MilSystem, 3, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE + MIL.M_PROC, ref milSource);
                }
                else
                {
                    throw new Exception(string.Format("Unknow Image Band.[{0}]", ImgBand));
                }

                // Color Image
                MIL.MbufAllocColor(MyMil.MilSystem, 3, ImgSizeX, ImgSizeY, ImgBits + ImgSign, MIL.M_IMAGE + MIL.M_PROC, ref milSourceColor);
            }
            MIL.MbufLoad(Param.ImgX_Path, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_RED, 0, 0, ImgSizeX, ImgSizeY);

            // Load Green Image
            if (!File.Exists(Param.ImgY_Path))
            {
                throw new Exception("Image G File Path Error !");
            }
            else
            {
                if (ImgSizeX != MIL.MbufDiskInquire(Param.ImgY_Path, MIL.M_SIZE_X, MIL.M_NULL))
                    throw new Exception("Image G's sizeX was not equal to Image R's sizeX  !");
                if (ImgSizeY != MIL.MbufDiskInquire(Param.ImgY_Path, MIL.M_SIZE_Y, MIL.M_NULL))
                    throw new Exception("Image G's sizeY was not equal to Image R's sizeY  !");
                if (ImgBits != MIL.MbufDiskInquire(Param.ImgY_Path, MIL.M_SIZE_BIT, MIL.M_NULL))
                    throw new Exception("Image G's bits was not equal to Image R's bits  !");
                if (ImgSign != MIL.MbufDiskInquire(Param.ImgY_Path, MIL.M_SIGN, MIL.M_NULL))
                    throw new Exception("Image G's sign was not equal to Image R's sign  !");
                if (ImgBand != MIL.MbufDiskInquire(Param.ImgY_Path, MIL.M_SIZE_BAND, MIL.M_NULL))
                    throw new Exception("Image G's bands was not equal to Image R's bands  !");
            }
            MIL.MbufLoad(Param.ImgY_Path, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_GREEN, 0, 0, ImgSizeX, ImgSizeY);

            // Load Blue Image
            if (!File.Exists(Param.ImgZ_Path))
            {
                throw new Exception("Image B File Path Error !");
            }
            else
            {
                if (ImgSizeX != MIL.MbufDiskInquire(Param.ImgZ_Path, MIL.M_SIZE_X, MIL.M_NULL))
                    throw new Exception("Image B's sizeX was not equal to Image R's sizeX  !");
                if (ImgSizeY != MIL.MbufDiskInquire(Param.ImgZ_Path, MIL.M_SIZE_Y, MIL.M_NULL))
                    throw new Exception("Image B's sizeY was not equal to Image R's sizeY  !");
                if (ImgBits != MIL.MbufDiskInquire(Param.ImgZ_Path, MIL.M_SIZE_BIT, MIL.M_NULL))
                    throw new Exception("Image B's bits was not equal to Image R's bits  !");
                if (ImgSign != MIL.MbufDiskInquire(Param.ImgZ_Path, MIL.M_SIGN, MIL.M_NULL))
                    throw new Exception("Image B's sign was not equal to Image R's sign  !");
                if (ImgBand != MIL.MbufDiskInquire(Param.ImgZ_Path, MIL.M_SIZE_BAND, MIL.M_NULL))
                    throw new Exception("Image B's bands was not equal to Image R's bands  !");
            }
            MIL.MbufLoad(Param.ImgZ_Path, milSource);
            MIL.MbufCopyColor2d(milSource, milSourceColor, MIL.M_RED, 0, 0, MIL.M_BLUE, 0, 0, ImgSizeX, ImgSizeY);

            // Difference Image
            MIL.MbufAlloc2d(MyMil.MilSystem, ImgSizeX, ImgSizeY, 32 + MIL.M_FLOAT, MIL.M_PROC + MIL.M_IMAGE, ref milImg_32F_diff);
            MIL.MbufAlloc2d(MyMil.MilSystem, ImgSizeX, ImgSizeY, 16 + MIL.M_UNSIGNED, MIL.M_PROC + MIL.M_IMAGE + MIL.M_DISP, ref milImg_16U_diff);

            switch (numerator_idx)
            {
                case 0: // R
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_RED, MIL.M_NULL);
                    break;

                case 1: // G
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_GREEN, MIL.M_NULL);
                    break;

                case 2: // B
                    mil_Numerator = MIL.MbufChildColor(milSourceColor, MIL.M_BLUE, MIL.M_NULL);
                    break;
            }

            switch (denominator_idx)
            {
                case 0: // R
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_RED, MIL.M_NULL);
                    break;

                case 1: // G
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_GREEN, MIL.M_NULL);
                    break;

                case 2: // B
                    mil_Denominator = MIL.MbufChildColor(milSourceColor, MIL.M_BLUE, MIL.M_NULL);
                    break;
            }

            MIL.MimArith(mil_Numerator, mil_Denominator, milImg_32F_diff, MIL.M_DIV);

            //if (Debug)
            //{
            //	ImgPath = String::Format("{0}\\ColorMura-Img_32F_diff.tif", Image_Out_FolderPath);
            //	MbufExport(ConvSysStrToWChar(ImgPath), M_TIFF, milImg_32F_diff);
            //}

            MIL.MimArith(milImg_32F_diff, 4095, milImg_16U_diff, MIL.M_MULT_CONST + MIL.M_SATURATION);

            if (Debug)
            {
                Param.ImgColor_Path = string.Format("{0}\\[2-1]ColorMura-Img_16U_diff.tif", Image_Out_FolderPath);
                MIL.MbufExport(Param.ImgColor_Path, MIL.M_TIFF, milImg_16U_diff);
            }

            MIL.MimBinarize(milImg_16U_diff, milImg_16U_diff, MIL.M_IN_RANGE, threshold_Low, threshold_High);

            //
            MIL.MimMorphic(milImg_16U_diff, milImg_16U_diff, MIL.M_3X3_RECT, MIL.M_AREA_OPEN, areaOpen, MIL.M_BINARY);
            if (dilate > 0)
                MIL.MimDilate(milImg_16U_diff, milImg_16U_diff, dilate, MIL.M_BINARY);
            if (Debug)
            {
                Param.ImgColor_Path = string.Format("{0}\\[2-2]ColorMura-Img_16U_dilate.tif", Image_Out_FolderPath);
                MIL.MbufExport(Param.ImgColor_Path, MIL.M_TIFF, milImg_16U_diff);
            }

            // Filter Area
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobAnalysis);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milBlobResult);

            MIL.MblobControl(milBlobAnalysis, MIL.M_ALL_FEATURES, MIL.M_DISABLE);
            MIL.MblobControl(milBlobAnalysis, MIL.M_BOX, MIL.M_ENABLE);

            MIL.MblobCalculate(milBlobAnalysis, milImg_16U_diff, MIL.M_NULL, milBlobResult);

            // Filter Blob
            MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, minArea, MIL.M_NULL);

            //this->ImgDraw_Blob(milImg_16U_diff);

            MIL.MbufClear(milImg_16U_diff, 0L);
            MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milImg_16U_diff, MIL.M_DRAW_BLOBS, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

            Param.ImgColor_Path = string.Format("{0}\\ColorMuraResult.tif", Image_Out_FolderPath);
            MIL.MbufExport(Param.ImgColor_Path, MIL.M_TIFF, milImg_16U_diff);

            //MgraControl(this->imageDisplay->MilGraphics, M_COLOR, M_COLOR_MAGENTA);
            //MblobDraw(this->imageDisplay->MilGraphics, milBlobResult,
            //	this->imageDisplay->MilGraphicList, M_DRAW_BLOBS, M_INCLUDED_BLOBS, M_DEFAULT);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref milSource);

            // Child of milSourceColor
            MilNetHelper.MilBufferFree(ref mil_Numerator);

            // Child of milSourceColor
            MilNetHelper.MilBufferFree(ref mil_Denominator);

            MilNetHelper.MilBufferFree(ref milSourceColor);
            MilNetHelper.MilBufferFree(ref milImg_32F_diff);
            MilNetHelper.MilBufferFree(ref milImg_16U_diff);

            if (milBlobAnalysis != null)
            {
                MIL.MblobFree(milBlobAnalysis);
                milBlobAnalysis = MIL.M_NULL;
            }
            if (milBlobResult != null)
            {
                MIL.MblobFree(milBlobResult);
                milBlobResult = MIL.M_NULL;
            }
        }
    }

    #endregion Color Mura

    #region "--- Calibration ---"

    public static void Calibration(MIL_ID SourceImg, string FilePath, ref MIL_ID OutputImg)
    {
        MIL_ID CalibrationData = MIL.M_NULL;
        MIL_ID CalibImg = MIL.M_NULL;

        try
        {
            if (File.Exists(FilePath))
            {
                MIL.McalRestore(FilePath, MyMil.MilSystem, MIL.M_DEFAULT, ref CalibrationData);
            }

            MIL_INT sizeX = MIL.MbufInquire(SourceImg, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT sizeY = MIL.MbufInquire(SourceImg, MIL.M_SIZE_Y, MIL.M_NULL);

            // MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref OutputImg);
            MIL.MbufCopy(SourceImg, OutputImg);

            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref CalibImg);
            MIL.MbufCopy(SourceImg, CalibImg);

            MIL.MbufClear(OutputImg, 0L);

            // MIL.MbufExport(OutputPath + @"\Img_before_Cal_NonPage.tif", MIL.M_TIFF, OutputImg);
            MIL.McalTransformImage(CalibImg, OutputImg, CalibrationData, MIL.M_BILINEAR + MIL.M_OVERSCAN_ENABLE, MIL.M_DEFAULT, MIL.M_DEFAULT);
            // MIL.MbufExport(OutputPath + @"\Img_after_Cal_NonPage.tif", MIL.M_TIFF, OutputImg);
        }
        catch (Exception)
        {
            OutputImg = MIL.M_NULL;
            throw;
        }
        finally
        {
            // Free
            if (CalibrationData != MIL.M_NULL)
            {
                MIL.McalFree(CalibrationData);
                CalibrationData = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref CalibImg);
        }
    }

    public static void Calibration(MIL_ID CalibrationData, ref MIL_ID image)
    {
        MIL_ID CalibImg = MIL.M_NULL;

        try
        {
            MIL_INT sizeX = MIL.MbufInquire(image, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT sizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y, MIL.M_NULL);

            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref CalibImg);
            MIL.MbufCopy(image, CalibImg);

            MIL.MbufClear(image, 0L);

            MIL.McalTransformImage(CalibImg, image, CalibrationData, MIL.M_BILINEAR + MIL.M_OVERSCAN_ENABLE, MIL.M_DEFAULT, MIL.M_DEFAULT);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            //if (CalibrationData != MIL.M_NULL)
            //{
            //    MIL.McalFree(CalibrationData);
            //    CalibrationData = MIL.M_NULL;
            //}

            MilNetHelper.MilBufferFree(ref CalibImg);
        }
    }

    public static void Calibration_SecondCorrect(LaserCalibrationParam CalData)
    {
        MIL_ID FlexDiff = MIL.M_NULL;
        MIL_ID TempId = MIL.M_NULL;
        MIL_ID TemplateId = MIL.M_NULL;
        MIL_ID WaitCorrectAlignId = MIL.M_NULL;
        MIL_ID CorrectId = MIL.M_NULL;
        MIL_ID RigidAlignedBuffer = MIL.M_NULL;
        MIL_ID calCorrectId = MIL.M_NULL;
        MIL_ID TargetBufferId = MIL.M_NULL;
        MIL_ID RigidDiff = MIL.M_NULL;
        MIL_ID Diff = MIL.M_NULL;

        try
        {
            //m_IsDebug = CalData.IsDebug;
            m_IsDebug = true;
            m_TemplatePath = CalData.Template_Path;
            m_TargetPath = CalData.Target_Path;
            //m_WaitCorrectPath = CalData.WaitCorrectId;
            m_ROI = CalData.ROI;

            // 讀取template
            MIL.MbufRestore(m_TemplatePath, MyMil.MilSystem, ref TempId);
            MIL_INT TemplateW = MIL.MbufInquire(TempId, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT TemplateH = MIL.MbufInquire(TempId, MIL.M_SIZE_Y, MIL.M_NULL);
            MIL.MbufAlloc2d(MyMil.MilSystem, TemplateW, TemplateH, 8, MIL.M_IMAGE + MIL.M_PROC, ref TemplateId);
            MIL.MimArith(TempId, 32, TemplateId, MIL.M_DIV_CONST);

            // 讀取target
            RestoreAndConvert(m_TargetPath, ref TargetBufferId);
            CloneBuffer(TemplateId, ref Diff);

            // 讀取WaitCorrect
            //MIL.MbufRestore(m_WaitCorrectPath, MyMil.MilSystem, ref WaitCorrectId);
            CloneBuffer(CalData.WaitCorrectId, ref WaitCorrectAlignId);

            // 比較template跟target的差異
            MIL.MimArith(TargetBufferId, TemplateId, Diff, MIL.M_SUB_ABS);
            Debug_MbufSave("diff_source.bmp", Diff);

            // 先將target旋轉+平移 讓影像接近template
            CloneBuffer(TargetBufferId, ref RigidAlignedBuffer);
            RigidAlignment(TemplateId, TargetBufferId, ref RigidAlignedBuffer, CalData.WaitCorrectId, ref WaitCorrectAlignId);
            Debug_MbufSave("WaitCorrectId.bmp", WaitCorrectAlignId);

            // 比較template跟旋轉+平移後的target的差異
            CloneBuffer(TemplateId, ref RigidDiff);
            MIL.MimArith(RigidAlignedBuffer, TemplateId, RigidDiff, MIL.M_SUB_ABS);
            Debug_MbufSave("diff_rigdi.bmp", RigidDiff);

            // Template只留下ROI
            SpotROI(m_ROI, ref TemplateId);
            ReconstructPoint(ref TemplateId);
            Debug_MbufSave("Template_ROI.bmp", TemplateId);

            // Target只留下ROI
            SpotROI(m_ROI, ref RigidAlignedBuffer);
            ReconstructPoint(ref RigidAlignedBuffer);
            Debug_MbufSave("Target_ROI.bmp", RigidAlignedBuffer);

            // Calibration
            MIL.McalAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_CalId);
            Calculate(TemplateId, RigidAlignedBuffer);

            // 用calibration結果校正影像
            CloneBuffer(WaitCorrectAlignId, ref calCorrectId);
            Transform(WaitCorrectAlignId, ref calCorrectId);
            MIL.MbufSave(CalData.Output_Path, calCorrectId);

            // 釋放calibration模組
            if (m_CalId != MIL.M_NULL)
            {
                MIL.McalFree(m_CalId);
                m_CalId = MIL.M_NULL;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Calibration failed. : {ex.Message}");
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref Diff);
            MilNetHelper.MilBufferFree(ref RigidDiff);
            MilNetHelper.MilBufferFree(ref TargetBufferId);
            MilNetHelper.MilBufferFree(ref calCorrectId);
            MilNetHelper.MilBufferFree(ref RigidAlignedBuffer);
            MilNetHelper.MilBufferFree(ref CorrectId);
            MilNetHelper.MilBufferFree(ref WaitCorrectAlignId);
            MilNetHelper.MilBufferFree(ref TemplateId);
            MilNetHelper.MilBufferFree(ref TempId);
            MilNetHelper.MilBufferFree(ref FlexDiff);
        }
    }

    private static void ReconstructPoint(ref MIL_ID MilSrc)
    {
        MIL_ID MilSmooth = MIL.M_NULL;
        MIL_ID MilErode = MIL.M_NULL;
        MIL_ID MilDilate = MIL.M_NULL;

        MIL.MbufClone(MilSrc, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSmooth);
        MIL.MbufClone(MilSmooth, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilErode);
        MIL.MbufClone(MilErode, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilDilate);

        //MIL.MimConvolve(MilSrc, MilSmooth, MIL.M_SMOOTH + MIL.M_OVERSCAN_ENABLE);
        MIL.MimErode(MilSrc, MilErode, 1, MIL.M_GRAYSCALE);
        MIL.MimDilate(MilErode, MilDilate, 3, MIL.M_GRAYSCALE);
        MIL.MimBinarize(MilDilate, MilDilate, MIL.M_FIXED + MIL.M_GREATER, 12.0, MIL.M_NULL);
        MIL.MbufSave("d:\\1.bmp", MilDilate);

        MIL.MbufClear(MilSrc, 0.0);
        MIL.MbufCopy(MilDilate, MilSrc);

        // Free
        MilNetHelper.MilBufferFree(ref MilDilate);
        MilNetHelper.MilBufferFree(ref MilErode);
        MilNetHelper.MilBufferFree(ref MilSmooth);
    }

    private static bool RigidAlignment(MIL_ID TemplateBufferId, MIL_ID TargetBufferId, ref MIL_ID DstBufferId, MIL_ID TargetSourceId, ref MIL_ID DstTargetSourceId)
    {
        bool AlignSucceed = false;

        MIL_ID RegContextId = MIL.MregAlloc(MIL.M_DEFAULT_HOST, MIL.M_CORRELATION, MIL.M_DEFAULT, MIL.M_NULL);
        MIL_ID RegResultId = MIL.MregAllocResult(MIL.M_DEFAULT_HOST, MIL.M_DEFAULT, MIL.M_NULL);

        MIL.MregControl(RegContextId, MIL.M_CONTEXT, MIL.M_TRANSFORMATION_TYPE, MIL.M_TRANSLATION_ROTATION);

        MIL_ID[] ImageArray = new MIL_ID[2] { TemplateBufferId, TargetBufferId };
        MIL_INT Result = 0;
        MIL.MregCalculate(RegContextId, ImageArray, RegResultId, 2, MIL.M_DEFAULT);
        MIL.MregGetResult(RegResultId, MIL.M_GENERAL, MIL.M_RESULT + MIL.M_TYPE_MIL_INT, ref Result);

        if (MIL.M_SUCCESS == Result)
        {
            AlignSucceed = true;
            MIL_ID TransMat = MIL.M_NULL;
            MIL.MregGetResult(RegResultId, 1, MIL.M_TRANSFORMATION_MATRIX_ID + MIL.M_TYPE_MIL_ID, ref TransMat);
            MIL.MimWarp(TargetBufferId, DstBufferId, TransMat, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_BILINEAR + MIL.M_OVERSCAN_CLEAR);
            MIL.MimWarp(TargetSourceId, DstTargetSourceId, TransMat, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_BILINEAR + MIL.M_OVERSCAN_CLEAR);
        }
        MIL.MregFree(RegResultId);
        MIL.MregFree(RegContextId);

        return AlignSucceed;
    }

    public static void Debug_MbufSave(string filename, MIL_ID img)
    {
        if (m_IsDebug)
        {
            MIL.MbufSave($@"D:\\{filename}", img);
        }
    }

    private static bool m_IsDebug = true;

    private static string m_TemplatePath;

    private static string m_TargetPath;

    private static System.Drawing.Rectangle m_ROI;

    public static void Calculate(MIL_ID MilTemplateImage, MIL_ID MilTargetImage)
    {
        MIL_ID BlobContext = MIL.M_NULL;
        MIL_ID TemplateBlobResult = MIL.M_NULL;
        MIL_ID TargetBlobResult = MIL.M_NULL;

        MIL_ID TemplateBlobDraw = MIL.M_NULL;
        MIL_ID TargetBlobDraw = MIL.M_NULL;
        MIL_ID TemplateGC = MIL.M_NULL;
        MIL_ID TargetGC = MIL.M_NULL;

        MIL_INT TemplateBlobs = 0;
        MIL_INT TargetBlobs = 0;

        try
        {
            MIL.MbufAllocColor(MyMil.MilSystem,
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_BAND, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_X, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_Y, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_BIT, MIL.M_NULL) + MIL.M_UNSIGNED,
                               MIL.M_IMAGE + MIL.M_PROC,
                               ref TemplateBlobDraw);
            MIL.MbufAllocColor(MyMil.MilSystem,
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_BAND, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_X, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_Y, MIL.M_NULL),
                               MIL.MbufInquire(MilTemplateImage, MIL.M_SIZE_BIT, MIL.M_NULL) + MIL.M_UNSIGNED,
                               MIL.M_IMAGE + MIL.M_PROC,
                               ref TargetBlobDraw);

            MIL.MgraAlloc(MyMil.MilSystem, ref TemplateGC);
            MIL.MgraAlloc(MyMil.MilSystem, ref TargetGC);
            MIL.MgraColor(TemplateGC, 255.0);
            MIL.MgraColor(TargetGC, 255.0);

            // Allocate Blob Context, Template跟Target共用
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext);
            MIL.MblobControl(BlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_BINARY, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(BlobContext, MIL.M_SORT2, MIL.M_BOX_Y_MIN);
            MIL.MblobControl(BlobContext, MIL.M_SORT1, MIL.M_BOX_X_MIN);

            // 濾出Template Image的雷射網格格點
            MIL.MimErode(MilTemplateImage, MilTemplateImage, 3, MIL.M_GRAYSCALE);
            MIL.MimDilate(MilTemplateImage, MilTemplateImage, 5, MIL.M_GRAYSCALE);
            MIL.MimArith(MilTemplateImage, MIL.M_NULL, MilTemplateImage, MIL.M_NOT);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref TemplateBlobResult);
            MIL.MblobCalculate(BlobContext, MilTemplateImage, MIL.M_NULL, TemplateBlobResult);
            MIL.MblobSelect(TemplateBlobResult, MIL.M_DELETE, MIL.M_AREA, MIL.M_OUT_RANGE, 4000, 6000);
            MIL.MblobGetResult(TemplateBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TemplateBlobs);

            // 濾出Target Image的雷射網格格點
            MIL.MimErode(MilTargetImage, MilTargetImage, 3, MIL.M_GRAYSCALE);
            MIL.MimDilate(MilTargetImage, MilTargetImage, 5, MIL.M_GRAYSCALE);
            MIL.MimArith(MilTargetImage, MIL.M_NULL, MilTargetImage, MIL.M_NOT);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref TargetBlobResult);
            MIL.MblobCalculate(BlobContext, MilTargetImage, MIL.M_NULL, TargetBlobResult);
            MIL.MblobSelect(TargetBlobResult, MIL.M_DELETE, MIL.M_AREA, MIL.M_OUT_RANGE, 4000, 6000);
            MIL.MblobGetResult(TargetBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TargetBlobs);

            // Template的格點畫方框, Target畫十字
            MIL.MblobDraw(TemplateGC, TemplateBlobResult, TemplateBlobDraw, MIL.M_DRAW_BLOBS, MIL.M_ALL_BLOBS, MIL.M_DEFAULT);
            MIL.MbufSave("d:\\Template_Grid.bmp", TemplateBlobDraw);
            MIL.MblobDraw(TargetGC, TargetBlobResult, TargetBlobDraw, MIL.M_DRAW_BOX_CENTER, MIL.M_ALL_BLOBS, MIL.M_DEFAULT);
            MIL.MbufSave("d:\\Target_Cross.bmp", TargetBlobDraw);

            // 方框跟十字做 and, 濾掉Target邊緣多的格點
            MIL.MimArith(TemplateBlobDraw, TargetBlobDraw, TargetBlobDraw, MIL.M_AND);

            // 再次做 Blob, 得到Target濾除後的Blob
            MIL.MblobCalculate(BlobContext, TargetBlobDraw, MIL.M_NULL, TargetBlobResult);
            MIL.MblobSelect(TargetBlobResult, MIL.M_DELETE, MIL.M_AREA, MIL.M_OUT_RANGE, 40, 50);
            MIL.MblobGetResult(TargetBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TargetBlobs);
            MIL.MblobDraw(TargetGC, TargetBlobResult, TargetBlobDraw, MIL.M_DRAW_BOX_CENTER, MIL.M_ALL_BLOBS, MIL.M_DEFAULT);

            TemplateX = new double[TemplateBlobs];
            TemplateY = new double[TemplateBlobs];
            double[] TargetX = new double[TemplateBlobs];
            double[] TargetY = new double[TemplateBlobs];
            Sort_TargetX = new double[TemplateBlobs];
            Sort_TargetY = new double[TemplateBlobs];

            MIL.MblobGetResult(TemplateBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, TemplateX);
            MIL.MblobGetResult(TemplateBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, TemplateY);
            MIL.MblobGetResult(TargetBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, TargetX);
            MIL.MblobGetResult(TargetBlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, TargetY);

            // 因為mil sort後, 歪斜網格仍會錯位, 所以先重排
            for (int i = 0; i < TemplateBlobs; i++)
            {
                bool IsMatch = false;

                MIL_INT SX = 0;
                MIL_INT SY = 0;
                MIL_INT EX = 0;
                MIL_INT EY = 0;
                MIL_INT TemplateGX = 0;
                MIL_INT TemplateGY = 0;

                System.Drawing.Rectangle rect = new System.Drawing.Rectangle();

                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_CENTER_OF_GRAVITY_X + MIL.M_TYPE_MIL_INT, ref TemplateGX);
                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_TYPE_MIL_INT, ref TemplateGY);
                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT, ref SX);
                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT, ref SY);
                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT, ref EX);
                MIL.MblobGetResult(TemplateBlobResult, MIL.M_BLOB_INDEX(i), MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT, ref EY);

                rect.X = (int)SX;
                rect.Y = (int)SY;
                rect.Width = (int)(EX - SX);
                rect.Height = (int)(EY - SY);

                for (int j = 0; j < TargetBlobs; j++)
                {
                    MIL_INT GX = 0;
                    MIL_INT GY = 0;
                    MIL.MblobGetResult(TargetBlobResult, MIL.M_BLOB_INDEX(j), MIL.M_CENTER_OF_GRAVITY_X + MIL.M_TYPE_MIL_INT, ref GX);
                    MIL.MblobGetResult(TargetBlobResult, MIL.M_BLOB_INDEX(j), MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_TYPE_MIL_INT, ref GY);

                    // 十字有落在方框裡才視為同一個index
                    if (rect.Contains(new System.Drawing.Point((int)GX, (int)GY)))
                    {
                        Sort_TargetX[i] = GX;
                        Sort_TargetY[i] = GY;
                        IsMatch = true;
                        break;
                    }
                }

                // 找不到的話就直接塞template的值
                if (!IsMatch)
                {
                    Sort_TargetX[i] = TemplateGX;
                    Sort_TargetY[i] = TemplateGY;
                }
            }

            MIL.McalList(m_CalId, Sort_TargetX, Sort_TargetY, TemplateX, TemplateY, MIL.M_NULL, TemplateBlobs, MIL.M_LINEAR_INTERPOLATION, MIL.M_DEFAULT);
        }
        catch
        {
        }
        finally
        {
            if (TemplateBlobResult != MIL.M_NULL)
            {
                MIL.MblobFree(TemplateBlobResult);
                TemplateBlobResult = MIL.M_NULL;
            }

            if (TargetBlobResult != MIL.M_NULL)
            {
                MIL.MblobFree(TargetBlobResult);
                TargetBlobResult = MIL.M_NULL;
            }

            if (BlobContext != MIL.M_NULL)
            {
                MIL.MblobFree(BlobContext);
                BlobContext = MIL.M_NULL;
            }

            if (TemplateGC != MIL.M_NULL)
            {
                MIL.MgraFree(TemplateGC);
                TemplateGC = MIL.M_NULL;
            }

            if (TargetGC != MIL.M_NULL)
            {
                MIL.MgraFree(TargetGC);
                TargetGC = MIL.M_NULL;
            }

            MilNetHelper.MilBufferFree(ref TemplateBlobDraw);
            MilNetHelper.MilBufferFree(ref TargetBlobDraw);
        }
    }

    public static void Transform(MIL_ID SrcBufferId, ref MIL_ID DstBufferId)
    {
        MIL.McalUniform(DstBufferId, 0, 0, 1, 1, 0, MIL.M_DEFAULT);
        MIL.McalTransformImage(SrcBufferId, DstBufferId, m_CalId, MIL.M_BILINEAR + MIL.M_OVERSCAN_CLEAR, MIL.M_DEFAULT, MIL.M_WARP_IMAGE + MIL.M_USE_DESTINATION_CALIBRATION);
    }

    private static double[] TemplateX;

    private static double[] TemplateY;

    private static double[] Sort_TargetX;

    private static double[] Sort_TargetY;

    private static MIL_ID m_CalId;

    #endregion Calibration

    #region "--- Warp ---"

    public static void Warp(MIL_ID SourceImg, Point Point1, Point Point2, Point Point3, Point Point4, Point CoeffPoint1, Point CoeffPoint2, MIL_ID OutputImg)
    {
        MIL_ID WarpParIn = MIL.M_NULL;
        MIL_ID WarpParOut = MIL.M_NULL;

        float x1, y1, x2, y2, x3, y3, x4, y4, x1d, y1d, x3d, y3d;
        x1 = (float)Point1.X;
        y1 = (float)Point1.Y;
        x2 = (float)Point2.X;
        y2 = (float)Point2.Y;
        x3 = (float)Point3.X;
        y3 = (float)Point3.Y;
        x4 = (float)Point4.X;
        y4 = (float)Point4.Y;
        x1d = (float)CoeffPoint1.X;
        y1d = (float)CoeffPoint1.Y;
        x3d = (float)CoeffPoint2.X;
        y3d = (float)CoeffPoint2.Y;

        try
        {
            float[] WarpParArray = { x1, y1, x2, y2, x3, y3, x4, y4, x1d, y1d, x3d, y3d };
            float[,] WarpOutPutArray =
                            {
                                { 0, 0, 0},
                                { 0, 0, 0},
                                { 0, 0, 0}
                            };

            WarpParIn = MIL.MbufAlloc1d(MyMil.MilSystem, 12, 32 + MIL.M_FLOAT, MIL.M_ARRAY, MIL.M_NULL);
            WarpParOut = MIL.MbufAlloc2d(MyMil.MilSystem, 3, 3, 32 + MIL.M_FLOAT, MIL.M_ARRAY, MIL.M_NULL);

            MIL.MbufPut(WarpParIn, WarpParArray);
            MIL.MbufPut(WarpParOut, WarpOutPutArray);
            MIL.MgenWarpParameter(WarpParIn, WarpParOut, MIL.M_NULL, MIL.M_WARP_4_CORNER, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);
            MIL.MbufGet(WarpParOut, WarpOutPutArray);

            MIL.MimWarp(SourceImg, OutputImg, WarpParOut, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_BILINEAR + MIL.M_OVERSCAN_FAST);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    #endregion Warp

    #region "--- 4-Color Cailbration ---"

    public static List<MeasureUnit> ul = new List<MeasureUnit>();

    public static void LoadAndAllocTestMatrixImage(
            string pathM00, string pathM01, string pathM02,
            string pathM10, string pathM11, string pathM12,
            string pathM20, string pathM21, string pathM22,
            ref MilMatrixImage matrixImage)
    {
        MIL_ID imgM00 = MIL.M_NULL;
        MIL_ID imgM01 = MIL.M_NULL;
        MIL_ID imgM02 = MIL.M_NULL;

        MIL_ID imgM10 = MIL.M_NULL;
        MIL_ID imgM11 = MIL.M_NULL;
        MIL_ID imgM12 = MIL.M_NULL;

        MIL_ID imgM20 = MIL.M_NULL;
        MIL_ID imgM21 = MIL.M_NULL;
        MIL_ID imgM22 = MIL.M_NULL;

        try
        {
            MIL.MbufImport(
                pathM00,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM00);
            MIL.MbufImport(
                pathM01,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM01);
            MIL.MbufImport(
                pathM02,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM02);

            MIL.MbufImport(
                pathM10,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM10);
            MIL.MbufImport(
                pathM11,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM11);
            MIL.MbufImport(
                pathM12,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM12);

            MIL.MbufImport(
                pathM20,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM20);
            MIL.MbufImport(
                pathM21,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM21);
            MIL.MbufImport(
                pathM22,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref imgM22);

            matrixImage = new MilMatrixImage(
                imgM00, imgM01, imgM02,
                imgM10, imgM11, imgM12,
                imgM20, imgM21, imgM22);
        }
        catch (Exception ex)
        {
            MilNetHelper.MilBufferFree(ref imgM00);
            MilNetHelper.MilBufferFree(ref imgM01);
            MilNetHelper.MilBufferFree(ref imgM02);

            MilNetHelper.MilBufferFree(ref imgM10);
            MilNetHelper.MilBufferFree(ref imgM11);
            MilNetHelper.MilBufferFree(ref imgM12);

            MilNetHelper.MilBufferFree(ref imgM20);
            MilNetHelper.MilBufferFree(ref imgM21);
            MilNetHelper.MilBufferFree(ref imgM22);

            throw ex;
        }
    }

    public static void LoadAndAllocTestMonoImage(
            string path,
            ref MilMonoImage monoImage)
    {
        MIL_ID img = MIL.M_NULL;

        try
        {
            MIL.MbufImport(
                path,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                 MyMil.MilSystem,
                ref img);

            monoImage = new MilMonoImage(img);
        }
        catch (Exception ex)
        {
            MilNetHelper.MilBufferFree(ref img);
            throw ex;
        }
    }

    public class MeasureUnit
    {
        public String RoiName = "";
        public MIL_INT RoiLableID = -1;
        public int CenterX = 0;
        public int CenterY = 0;
        public int Radius = 0;
        public double Luminance = 0.0;
        public double Cx = 0.0;
        public double Cy = 0.0;

        //for Rect
        public int X = 0;
        public int Y = 0;
        public int W = 0;
        public int H = 0;

    }

    //private void LoadAndAllocTestColorImage(
    //        string pathX,
    //        string pathY,
    //        string pathZ,
    //        ref MilColorImage colorImage)
    //{
    //    MIL_ID imgX = MIL.M_NULL;
    //    MIL_ID imgY = MIL.M_NULL;
    //    MIL_ID imgZ = MIL.M_NULL;

    //    try
    //    {
    //        MIL.MbufImport(
    //            pathX,
    //            MIL.M_DEFAULT,
    //            MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
    //            MyMil.MilSystem,
    //            ref imgX);

    //        MIL.MbufImport(
    //            pathY,
    //            MIL.M_DEFAULT,
    //            MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
    //            MyMil.MilSystem,
    //            ref imgY);

    //        MIL.MbufImport(
    //            pathZ,
    //            MIL.M_DEFAULT,
    //            MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
    //            MyMil.MilSystem,
    //            ref imgZ);

    //        colorImage = new MilColorImage(imgX, imgY, imgZ);
    //    }
    //    catch (Exception ex)
    //    {
    //         FreeTestImage(ref imgX, ref imgY, ref imgZ);

    //        throw ex;
    //    }
    //}

    #endregion 4-Color Cailbration

    #region --- PixelAlign ---

    public static void PixelAlign(PixelAlignParam param)
    {
        MIL_ID grayImage = MIL.M_NULL;
        MIL_ID rotatedImage = MIL.M_NULL;
        BlobData[,] cogData = null;
        BlobData[,] cogDatasPitch = null;

        try
        {
            MIL.MbufImport(param.Image_Path, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MyMil.MilSystem, ref grayImage);

            //EnumBPPatternType patternType = EnumBPPatternType.NormalPitch;  // BPPatternType.NormalPitch / OddRow_Offset_DoublePitch
            //EnumBPSearchMethod searchMethod = EnumBPSearchMethod.SearchByPitch;        // BPSearchMethod.SearchByPitch / BPSearchMethod.LocalSearch

            PixelAligner pixelAligner = new PixelAligner(MyMil.MilSystem);
            //cogData = pixelAligner.Calculate(grayImage, param.panelInfo, param.setting,
            //    patternType, searchMethod, AlignRunType.Normal, ref cogDatasPitch);

            CogDataResultFile resultFile = new CogDataResultFile();
            resultFile.AddData(param.panelInfo, cogData);

            resultFile.WriteWithoutCrypto(
                string.Format(
                    "{0}\\CogData.xml",
                    param.setting.outPath),
                false);

            //bool? flag =  cbDrawCog.IsChecked;
            //if (flag != null
            //    && (bool)flag)
            //{
            DrawCog(param.setting.outPath, rotatedImage, param.panelInfo, cogData, cogDatasPitch);
            //}

            //long timespent = sw.ElapsedMilliseconds;
        }
        catch
        {
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref grayImage);

            if (cogDatasPitch != null)
                cogDatasPitch = null;
        }
    }

    #endregion --- PixelAlign ---

    #region --- PixelStat ---
    public static void PixelStat(PixelStatParam param)
    {
        MilColorImage tristimulusImage = new MilColorImage();
        MilChromaImage chromaImage = new MilChromaImage();

        BlobData[,] cogData = null;

        MilColorImage tristimulusStatImage = new MilColorImage();
        MilChromaImage chromaStatImage = new MilChromaImage();

        MIL_ID grayImage = MIL.M_NULL;

        try
        {
            MIL.MbufImport(
                param.Image_Path,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref grayImage);

            LoadAndAllocTestColorImage(param.TristimulusX_Path, param.TristimulusY_Path, param.TristimulusZ_Path, ref tristimulusImage);
            LoadAndAllocTestChromaImage(param.ChromaCx_Path.Trim(), param.ChromaCy_Path, ref chromaImage);


            //LoadAndAllocTestChromaImage("D:\\29_Test\\PixelAlign\\CX.tif", param.ChromaCy_Path, ref chromaImage);

            CogDataResultFile resultFile = new CogDataResultFile();
            resultFile.ReadWithoutCrypto(
                param.CogDataFile_Path);

            resultFile.ConvertToArray(out cogData);

            LightMeasurer measurer = new LightMeasurer(MyMil.MilSystem);

            measurer.PredictStatisticImage(cogData,
                                           param.panelInfo,
                                           param.radius,
                                           tristimulusImage,
                                           chromaImage,
                                           ref tristimulusStatImage,
                                           ref chromaStatImage);

            MIL.MbufSave(string.Format("{0}\\ImgStatTristimulusX.tif", param.setting.outPath), tristimulusStatImage.ImgX);
            MIL.MbufSave(string.Format("{0}\\ImgStatTristimulusY.tif", param.setting.outPath), tristimulusStatImage.ImgY);
            MIL.MbufSave(string.Format("{0}\\ImgStatTristimulusZ.tif", param.setting.outPath), tristimulusStatImage.ImgZ);
            MIL.MbufSave(string.Format("{0}\\ImgStatChromaX.tif", param.setting.outPath), chromaStatImage.ImgCx);
            MIL.MbufSave(string.Format("{0}\\ImgStatChromaY.tif", param.setting.outPath), chromaStatImage.ImgCy);

            //System.Diagnostics.Stopwatch tmpStopwatch = new System.Diagnostics.Stopwatch();
            //tmpStopwatch.Start();

            if (param.DrawCircle)
            {
                DrawCircle(param.Output_Path, grayImage, param.panelInfo, cogData, param.radius);

            }
            //tmpStopwatch.Stop();
        }
        catch (Exception ex)
        {
            string str = ex.ToString();
        }
        finally
        {
            if (tristimulusImage != null)
            {
                tristimulusImage.Free();
            }

            if (chromaImage != null)
            {
                chromaImage.Free();
            }

            if (tristimulusStatImage != null)
            {
                tristimulusStatImage.Free();
            }

            if (chromaStatImage != null)
            {
                chromaStatImage.Free();
            }
        }
    }
    #endregion

    #region --- DrawCog ---

    private static void DrawCog(
        string path,
        MIL_ID image,
        PanelInfo panelInfo,
        BlobData[,] cogDatas,
        BlobData[,] cogDatasPitch)
    {
        MIL_ID image_8U = MIL.M_NULL;
        MIL_ID image_Color = MIL.M_NULL;

        MIL_INT sizeX = MIL.MbufInquire(image, MIL.M_SIZE_X);
        MIL_INT sizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y);

        MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
            MIL.M_IMAGE + MIL.M_PROC, ref image_8U);
        MIL.MbufClear(image_8U, 0L);

        MIL.MbufAllocColor(MyMil.MilSystem, 3, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
            MIL.M_IMAGE + MIL.M_PROC, ref image_Color);
        MIL.MbufClear(image_Color, 0L);

        MIL.MimArith(image, 16, image_8U, MIL.M_DIV_CONST);

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

        for (int y = 0; y < panelInfo.ResolutionY; y++)
        {
            for (int x = 0; x < panelInfo.ResolutionX; x++)
            {
                BlobData blob = cogDatas[y, x];

                // Draw Pitch Pitch
                if (cogDatasPitch != null)
                {
                    BlobData blob_Pitch = cogDatasPitch[y, x];
                    if (blob_Pitch != null)
                    {
                        blob_Pitch.CogX = Math.Round(blob_Pitch.CogX, MidpointRounding.AwayFromZero);
                        blob_Pitch.CogY = Math.Round(blob_Pitch.CogY, MidpointRounding.AwayFromZero);

                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                        MIL.MgraDot(MIL.M_DEFAULT, image_Color, blob_Pitch.CogX, blob_Pitch.CogY);
                    }
                }

                if (blob != null)
                {
                    blob.CogX = Math.Round(blob.CogX, MidpointRounding.AwayFromZero);
                    blob.CogY = Math.Round(blob.CogY, MidpointRounding.AwayFromZero);

                    if (blob.UsePitch)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                    }
                    else
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    }

                    //
                    if (x == 0 && y == 0)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    }

                    MIL.MgraDot(
                        MIL.M_DEFAULT,
                        image_Color,
                        blob.CogX,
                        blob.CogY);
                }
            }
        }

        MIL.MbufSave(
            string.Format(@"{0}\DrawCogColor.tif", path),
            image_Color);

        // Free
        MilNetHelper.MilBufferFree(ref image_Color);
        MilNetHelper.MilBufferFree(ref image_8U);
    }

    #endregion --- DrawCog ---

    #region --- LoadAndAllocTestChromaImage ---

    public static void LoadAndAllocTestChromaImage(string pathCx, string pathCy, ref MilChromaImage chromaImage)
    {
        MIL_ID imgCx = MIL.M_NULL;
        MIL_ID imgCy = MIL.M_NULL;
        MIL_ID imgCz = MIL.M_NULL;

        try
        {
            MIL.MbufImport(
                pathCx,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgCx);

            MIL.MbufImport(
                pathCy,
                MIL.M_DEFAULT,
                MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS,
                MyMil.MilSystem,
                ref imgCy);

            chromaImage = new MilChromaImage(imgCx, imgCy);
        }
        catch (Exception ex)
        {
            FreeTestImage(ref imgCx, ref imgCy, ref imgCz);

            throw ex;
        }
    }

    #endregion --- LoadAndAllocTestChromaImage ---

    #region --- DrawCircle ---

    private static void DrawCircle(string path, MIL_ID image, PanelInfo panelInfo, BlobData[,] cogDatas, int radius)
    {
        MIL_ID image_8U = MIL.M_NULL;
        MIL_ID image_Color = MIL.M_NULL;

        MIL_INT sizeX = MIL.MbufInquire(image, MIL.M_SIZE_X);
        MIL_INT sizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y);

        MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
            MIL.M_IMAGE + MIL.M_PROC, ref image_8U);
        MIL.MbufClear(image_8U, 0L);

        MIL.MbufAllocColor(MyMil.MilSystem, 3, sizeX, sizeY, 8 + MIL.M_UNSIGNED,
            MIL.M_IMAGE + MIL.M_PROC, ref image_Color);
        MIL.MbufClear(image_Color, 0L);

        MIL.MimArith(image, 16, image_8U, MIL.M_DIV_CONST);

        for (int i = 0; i < 3; i++)
        {
            MIL.MbufCopyColor2d(image_8U, image_Color, 0, 0, 0, (MIL_INT)i, 0, 0, sizeX, sizeY);
        }

        for (int y = 0; y < panelInfo.ResolutionY; y++)
        {
            for (int x = 0; x < panelInfo.ResolutionX; x++)
            {
                BlobData blob = cogDatas[y, x];

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);

                if (blob != null)
                {
                    blob.CogX = Math.Round(blob.CogX, MidpointRounding.AwayFromZero);
                    blob.CogY = Math.Round(blob.CogY, MidpointRounding.AwayFromZero);

                    if (blob.UsePitch)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                    }
                    else
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    }
                    //
                    if (x == 0 && y == 0)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    }

                    MIL.MgraDot(MIL.M_DEFAULT, image_Color, blob.CogX, blob.CogY);
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);

                    MIL.MgraArc(MIL.M_DEFAULT, image_Color, blob.CogX, blob.CogY, radius, radius, 0, 360);
                }
            }
        }

        MIL.MbufSave(
            string.Format(@"{0}\DrawCircle.tif", path),
            image_Color);

        // Free
        MilNetHelper.MilBufferFree(ref image_Color);
        MilNetHelper.MilBufferFree(ref image_8U);
    }

    #endregion --- DrawCircle ---

    #region --- Heat Map ---

    #region --- GetLimit ---

    public static void GetLimit(MIL_ID referenceImage, ref float orgMin, ref float orgMax)
    {
        MIL_INT sizeX = MIL.MbufInquire(referenceImage, MIL.M_SIZE_X, MIL.M_NULL);
        MIL_INT sizeY = MIL.MbufInquire(referenceImage, MIL.M_SIZE_Y, MIL.M_NULL);

        float[] arrayGrayScale = new float[(int)sizeX * (int)sizeY];
        List<float> arrayList = new List<float>();

        MIL.MbufGet2d(referenceImage, 0, 0, sizeX, sizeY, arrayGrayScale);
        arrayList.AddRange(arrayGrayScale);
        arrayList.RemoveAll(item => float.IsNaN(item));
        orgMax = arrayList.Max();
        orgMin = arrayList.Min();
    }

    #endregion --- GetLimit ---

    #region --- RemapImageTo16Unsign ---

    public static MIL_ID RemapImageTo16Unsign(MIL_ID referenceImage, ref float minPixel, ref float maxPixel, float lowThreshold = -1, float hightThreshold = -1)
    {
        MIL_ID mimConstraintBound = MIL.M_NULL;
        MIL_ID mimRemoveZero = MIL.M_NULL;
        MIL_ID mimOffset = MIL.M_NULL;
        MIL_ID mimRemap = MIL.M_NULL;

        MIL_INT sizeX = MIL.MbufInquire(referenceImage, MIL.M_SIZE_X, MIL.M_NULL);
        MIL_INT sizeY = MIL.MbufInquire(referenceImage, MIL.M_SIZE_Y, MIL.M_NULL);

        float[] arrayGrayScale = new float[(int)sizeX * (int)sizeY];
        List<float> arrayList = new List<float>();

        int offset = 0;
        double scale = 0.0;
        float constraintMinPixel = -1;
        float constraintMaxPixel = -1;

        // 取得原圖最大.最小灰階值
        MIL.MbufGet2d(referenceImage, 0, 0, sizeX, sizeY, arrayGrayScale);
        arrayList.AddRange(arrayGrayScale);
        arrayList.RemoveAll(item => float.IsNaN(item));
        maxPixel = arrayList.Max();
        minPixel = arrayList.Min();
        arrayList.Clear();

        mimConstraintBound = ConstraintBound(referenceImage, lowThreshold, hightThreshold, MainCamera);

        // Allocate Image
        MIL.MbufAllocColor(MyMil.MilSystem, 1, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref mimRemap);
        MIL.MbufClone(mimConstraintBound, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mimRemoveZero);
        MIL.MbufClone(mimConstraintBound, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mimOffset);

        // 統計影像最大值
        MIL.MbufGet2d(mimConstraintBound, 0, 0, sizeX, sizeY, arrayGrayScale);
        arrayList.AddRange(arrayGrayScale);
        arrayList.RemoveAll(item => float.IsNaN(item));
        constraintMaxPixel = arrayList.Max();
        arrayList.Clear();

        // 找出灰階值是0的點 應該是calibration造成邊緣為0
        MIL.MimBinarize(mimConstraintBound, mimRemoveZero, MIL.M_FIXED + MIL.M_EQUAL, 0.0, MIL.M_NULL);

        // 把0點的值填最大值+1 避免0點影響正規化
        MIL.MimArith(mimRemoveZero, constraintMaxPixel + 1, mimRemoveZero, MIL.M_MULT_CONST);
        MIL.MimArith(mimRemoveZero, mimConstraintBound, mimRemoveZero, MIL.M_ADD);

        // 統計影像除0點外的最小值
        MIL.MbufGet2d(mimRemoveZero, 0, 0, sizeX, sizeY, arrayGrayScale);
        arrayList = new List<float>();
        arrayList.AddRange(arrayGrayScale);
        arrayList.RemoveAll(item => float.IsNaN(item));
        constraintMinPixel = arrayList.Min();

        // 把影像灰階拉成 1-65535
        offset = Convert.ToInt32(Math.Floor(constraintMinPixel));
        MIL.MimArith(mimConstraintBound, -offset, mimOffset, MIL.M_ADD_CONST + MIL.M_FLOAT_PROC);
        scale = 65535.0 / Convert.ToDouble(constraintMaxPixel - offset);
        MIL.MimArith(mimOffset, scale, mimRemap, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);
        MIL.MbufGet2d(mimRemap, 0, 0, sizeX, sizeY, arrayGrayScale);

        // 清理buffer
        MilNetHelper.MilBufferFree(ref mimConstraintBound);
        MilNetHelper.MilBufferFree(ref mimRemoveZero);
        MilNetHelper.MilBufferFree(ref mimOffset);

        return mimRemap;
    }

    #endregion --- RemapImageTo16Unsign ---

    #region --- ConstraintBound ---

    public static MIL_ID ConstraintBound(MIL_ID referenceImage, float lowThreshold, float hightThreshold, CameraBase MainCamera)
    {
        if (hightThreshold < lowThreshold) return MIL.M_NULL;

        MIL_ID MimResultImage = MIL.M_NULL;

        // Constraint Upper Bound
        MIL_ID MimBinarizeHighThreshold = MIL.M_NULL;
        MIL_ID MimBinarizeHighThresholdNot = MIL.M_NULL;
        MIL_ID MimRemapdestination1 = MIL.M_NULL;
        MIL_ID MimRemapdestination2 = MIL.M_NULL;
        MIL_ID MimArithdestination1 = MIL.M_NULL;
        MIL_ID MimArithdestination2 = MIL.M_NULL;

        // Constraint Lower Bound
        MIL_ID MimBinarizeLowThreshold = MIL.M_NULL;
        MIL_ID MimBinarizeLowThresholdNot = MIL.M_NULL;
        MIL_ID MimRemapdestination3 = MIL.M_NULL;
        MIL_ID MimRemapdestination4 = MIL.M_NULL;
        MIL_ID MimArithdestination3 = MIL.M_NULL;
        MIL_ID MimArithdestination4 = MIL.M_NULL;

        MIL_INT sizeX = MIL.MbufInquire(referenceImage, MIL.M_SIZE_X, MIL.M_NULL);
        MIL_INT sizeY = MIL.MbufInquire(referenceImage, MIL.M_SIZE_Y, MIL.M_NULL);

        try
        {
            #region --- Constraint Upper Bound ---

            if (hightThreshold >= 0 && hightThreshold <= MainCamera.MaxGrayScale)
            {
                MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MimBinarizeHighThreshold);
                MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MimBinarizeHighThresholdNot);

                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimRemapdestination1);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimRemapdestination2);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimArithdestination1);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimArithdestination2);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimResultImage);
                MIL.MbufCopy(referenceImage, MimResultImage);

                MIL.MbufControl(MimRemapdestination1, MIL.M_MAX, 1.0);
                MIL.MbufControl(MimRemapdestination1, MIL.M_MIN, 0.0);

                MIL.MbufControl(MimRemapdestination2, MIL.M_MAX, hightThreshold);
                MIL.MbufControl(MimRemapdestination2, MIL.M_MIN, 0.0);

                // 找比hightThreshold灰階高的地方
                MIL.MimBinarize(MimResultImage, MimBinarizeHighThreshold, MIL.M_FIXED + MIL.M_GREATER_OR_EQUAL, hightThreshold, MIL.M_NULL);

                // 把比hightThreshold灰階高的地方塗0 其餘塗1
                MIL.MimArith(MimBinarizeHighThreshold, MIL.M_NULL, MimBinarizeHighThresholdNot, MIL.M_NOT);
                MIL.MimRemap(MIL.M_DEFAULT, MimBinarizeHighThresholdNot, MimRemapdestination1, MIL.M_DEFAULT);

                // 和原圖相乘 移除比hightThreshold灰階高的地方
                MIL.MimArith(MimRemapdestination1, MimResultImage, MimArithdestination1, MIL.M_MULT);

                // 再把比hightThreshold灰階高的地方塗hightThreshold
                MIL.MimRemap(MIL.M_DEFAULT, MimBinarizeHighThreshold, MimRemapdestination2, MIL.M_DEFAULT);
                MIL.MimArith(MimRemapdestination2, MimArithdestination1, MimArithdestination2, MIL.M_ADD);

                MIL.MbufCopy(MimArithdestination2, MimResultImage);
            }

            #endregion --- Constraint Upper Bound ---

            #region --- Constraint Lower Bound ---

            // 用MimArithdestination2繼續做

            if (lowThreshold >= 0 && lowThreshold <= MainCamera.MaxGrayScale)
            {
                MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MimBinarizeLowThreshold);
                MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref MimBinarizeLowThresholdNot);

                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimRemapdestination3);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimRemapdestination4);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimArithdestination3);
                MIL.MbufClone(referenceImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MimArithdestination4);

                MIL.MbufControl(MimRemapdestination3, MIL.M_MAX, 1.0);
                MIL.MbufControl(MimRemapdestination3, MIL.M_MIN, 0.0);

                MIL.MbufControl(MimRemapdestination4, MIL.M_MAX, lowThreshold);
                MIL.MbufControl(MimRemapdestination4, MIL.M_MIN, 0.0);

                // 找比lowtThreshold灰階低的地方
                MIL.MimBinarize(MimResultImage, MimBinarizeLowThreshold, MIL.M_FIXED + MIL.M_LESS_OR_EQUAL, lowThreshold, MIL.M_NULL);

                // 把比lowtThreshold灰階低的地方塗0 其餘塗1
                MIL.MimArith(MimBinarizeLowThreshold, MIL.M_NULL, MimBinarizeLowThresholdNot, MIL.M_NOT);
                MIL.MimRemap(MIL.M_DEFAULT, MimBinarizeLowThresholdNot, MimRemapdestination3, MIL.M_DEFAULT);

                // 和原圖相乘 移除比lowtThreshold灰階低的地方
                MIL.MimArith(MimRemapdestination3, MimResultImage, MimArithdestination3, MIL.M_MULT);

                // 再把比lowtThreshold灰階高的地方塗lowtThreshold
                MIL.MimRemap(MIL.M_DEFAULT, MimBinarizeLowThreshold, MimRemapdestination4, MIL.M_DEFAULT);
                MIL.MimArith(MimRemapdestination4, MimArithdestination3, MimArithdestination4, MIL.M_ADD);

                MIL.MbufCopy(MimArithdestination4, MimResultImage);
            }

            #endregion --- Constraint Lower Bound ---

            return MimResultImage;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return MIL.M_NULL;
        }
        finally
        {
            #region --- Free ---
            MilNetHelper.MilBufferFree(ref MimBinarizeHighThreshold);
            MilNetHelper.MilBufferFree(ref MimBinarizeHighThresholdNot);
            MilNetHelper.MilBufferFree(ref MimRemapdestination1);
            MilNetHelper.MilBufferFree(ref MimRemapdestination2);
            MilNetHelper.MilBufferFree(ref MimArithdestination1);
            MilNetHelper.MilBufferFree(ref MimArithdestination2);

            MilNetHelper.MilBufferFree(ref MimBinarizeLowThreshold);
            MilNetHelper.MilBufferFree(ref MimBinarizeLowThresholdNot);
            MilNetHelper.MilBufferFree(ref MimRemapdestination3);
            MilNetHelper.MilBufferFree(ref MimRemapdestination4);
            MilNetHelper.MilBufferFree(ref MimArithdestination3);
            MilNetHelper.MilBufferFree(ref MimArithdestination4);
            #endregion
        }
    }

    #endregion --- ConstraintBound ---

    #region --- DrawHeatMap ---

    public static MIL_ID DrawHeatMap(MIL_ID sourceImage)
    {
        MIL_ID Customlookuptable = MIL.M_NULL;
        MIL_ID MimLutMapdestination = MIL.M_NULL;

        MIL_INT sizeX = MIL.MbufInquire(sourceImage, MIL.M_SIZE_X, MIL.M_NULL);
        MIL_INT sizeY = MIL.MbufInquire(sourceImage, MIL.M_SIZE_Y, MIL.M_NULL);

        try
        {
            string table = @"./HeatMapTable_16JET";
            MIL.MbufImport(table, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MyMil.MilSystem, ref Customlookuptable);
            MIL.MbufAllocColor(MyMil.MilSystem, 3, sizeX, sizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref MimLutMapdestination);
            MIL.MimLutMap(sourceImage, MimLutMapdestination, Customlookuptable);

            return MimLutMapdestination;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return MIL.M_NULL;
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref Customlookuptable);
        }
    }

    #endregion --- DrawHeatMap ---

    #region --- DrawRuler ---

    public static void DrawRuler(ref MIL_ID ruler, int imageWidth, int imageHeight, float low, float high, int partNumber = 10)
    {
        List<float> value = new List<float>();
        List<float> position = new List<float>();
        float dValue = (high - low) / 10;
        float dPision = (imageWidth - 40) / 10;

        for (int i = 0; i < partNumber; i++)
        {
            value.Add(low + i * dValue);
        }
        value.Add(high);    // 避免最後一個分位被truncate

        for (int i = 0; i < partNumber; i++)
        {
            position.Add(20 + i * dPision);
        }
        position.Add(imageWidth - 20);    // 避免最後一個分位被truncate

        MilNetHelper.MilBufferFree(ref ruler);

        // allocate 畫比例尺的圖
        MIL.MbufAlloc2d(MyMil.MilSystem,
                        imageWidth,
                        imageHeight,
                        8 + MIL.M_UNSIGNED,
                        MIL.M_IMAGE + MIL.M_PROC,
                        ref ruler);

        // 底色塗系統button face的顏色
        MIL.MbufClear(ruler, 242);

        // 字體設定: 字->黑色; 字底色->系統button face的顏色; 字大小->4.0
        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLACK);
        MIL.MgraBackColor(MIL.M_DEFAULT, MIL.M_COLOR888(242, 242, 242));
        MIL.MgraControl(MIL.M_DEFAULT, MIL.M_FONT_SIZE, 4.0);

        // 橫線
        MIL.MgraLine(MIL.M_DEFAULT, ruler, 20, 3, imageWidth - 20, 3);

        for (int i = 0; i < partNumber + 1; i++)
        {
            // 顯示刻度 第一個跟最後一個刻度的數字內縮
            if (i == 0)
            {
                MIL.MgraText(MIL.M_DEFAULT, ruler, position[0], 5, value[0].ToString("0.00"));
            }
            else if (i == partNumber)
            {
                int l = position[partNumber].ToString("0.00").Length;
                MIL.MgraText(MIL.M_DEFAULT, ruler, position[partNumber] - 5 * l, 5, value[partNumber].ToString("0.00"));
            }
            else
            {
                MIL.MgraText(MIL.M_DEFAULT, ruler, position[i] - 10, 5, value[i].ToString("0.00"));
            }

            // 每個刻度的直線
            MIL.MgraLine(MIL.M_DEFAULT, ruler, position[i], 0, position[i], 3);
        }
    }

    #endregion --- DrawRuler ---

    #endregion --- Heat Map ---

    #region --- FindCorner ---

    private static void CalculateIntersectionPoint(double[] Line1, double[] Line2, ref Point IntersectionPoint)
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

    public static void Linear_Interpolation(double baseX1, double baseY1, double baseX2, double baseY2, double targetX, ref double targetY)
    {
        //y=mx+c m=(y1-y2)/(x1-x2) c=y-mx
        double m, c;
        m = (baseY1 - baseY2) / (baseX1 - baseX2);
        c = baseY1 - m * baseX1;
        targetY = m * targetX + c;
    }

    public static void Linear_Regression(PointD[] parray, double targetX, ref double targetY)
    {
        if (parray.Length < 2)
        {
            targetY = 0;
            return;
        }

        double averagex = 0, averagey = 0;
        foreach (PointD p in parray)
        {
            averagex += p.X;
            averagey += p.Y;
        }
        averagex /= parray.Length;
        averagey /= parray.Length;

        double numerator = 0;
        double denominator = 0;
        foreach (PointD p in parray)
        {
            numerator += (p.X - averagex) * (p.Y - averagey);
            denominator += (p.X - averagex) * (p.X - averagex);
        }

        double RCA = numerator / denominator;
        double RCB = averagey - RCA * averagex;
        targetY = RCA * targetX + RCB;
    }

    public static void FindCornerbyEdgeFinder(MIL_ID sourceimage, ref Point[] cornerpoint)
    {
        //    P0----------P2
        //    |           |
        //    |           |
        //    P1----------P3

        MIL_INT ImageSizeX = MIL.M_NULL;
        MIL_INT ImageSizeY = MIL.M_NULL;
        MIL_ID MBinaryImage = MIL.M_NULL;
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
            MIL.MbufInquire(sourceimage, MIL.M_SIZE_X, ref ImageSizeX);
            MIL.MbufInquire(sourceimage, MIL.M_SIZE_Y, ref ImageSizeY);

            //binary
            MIL.MbufAlloc2d(MyMil.MilSystem, ImageSizeX, ImageSizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MBinaryImage);
            MIL.MimBinarize(sourceimage, MBinaryImage, MIL.M_BIMODAL + MIL.M_GREATER_OR_EQUAL, MIL.M_NULL, MIL.M_NULL);

            //blob
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobContext);
            MIL.MblobControl(MBlobContext, MIL.M_FOREGROUND_VALUE, MIL.M_NON_ZERO);
            MIL.MblobControl(MBlobContext, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);
            MIL.MblobControl(MBlobContext, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(MBlobContext, MIL.M_SORT1, MIL.M_AREA);
            MIL.MblobControl(MBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobResult);
            MIL.MblobCalculate(MBlobContext, MBinaryImage, MIL.M_NULL, MBlobResult);
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
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_HORIZONTAL, MIL.M_NULL);
                MIL.MmeasFindMarker(MIL.M_DEFAULT, sourceimage, MEdgeMarker, MIL.M_LINE_EQUATION);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                if (EdgeCount == 0) { continue; }
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[0]);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[1]);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref HLineCoef[2]);

                //vertical
                MIL.MmeasSetMarker(MEdgeMarker, MIL.M_ORIENTATION, MIL.M_VERTICAL, MIL.M_NULL);
                MIL.MmeasFindMarker(MIL.M_DEFAULT, sourceimage, MEdgeMarker, MIL.M_LINE_EQUATION);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref EdgeCount);
                if (EdgeCount == 0) { continue; }
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_A + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[0]);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_B + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[1]);
                MIL.MmeasGetResult(MEdgeMarker, MIL.M_LINE_C + MIL.M_TYPE_MIL_DOUBLE, ref VLineCoef[2]);

                //crosspoint
                CalculateIntersectionPoint(HLineCoef, VLineCoef, ref cornerpoint[cornerindex]);
            }

        }
        catch 
        {

        }
        finally
        {
            MilNetHelper.MilBufferFree(ref MBinaryImage);
            MilNetHelper.MilBlobFree(ref MBlobContext);
            MilNetHelper.MilBlobFree(ref MBlobResult);
            MilNetHelper.MilMeasFree(ref MEdgeMarker);
        }
    }
    #endregion

    #region --- LMK ---
    public static string FindExtremebyConvolution(MIL_ID sourceimage, System.Drawing.Size kernelsize, int roipadding, string imagename, string SaveFolder, ref Point[] MinPeakPosition, ref Point[] MaxPeakPosition, ref double[] PeakValue)
    {
        MIL_INT ImageSizeX = MIL.M_NULL;
        MIL_INT ImageSizeY = MIL.M_NULL;
        MIL_INT BufType = MIL.M_NULL;
        MIL_ID MBinaryImage = MIL.M_NULL;
        MIL_ID MROIImage = MIL.M_NULL;
        MIL_ID MBlobContext = MIL.M_NULL;
        MIL_ID MBlobResult = MIL.M_NULL;
        MIL_ID MKernel = MIL.M_NULL;
        MIL_ID MConvolutionImage = MIL.M_NULL;
        MIL_ID MIPResult = MIL.M_NULL;
        MIL_ID MDrawImage = MIL.M_NULL;
        MIL_ID MGraContext = MIL.M_NULL;
        int BlobCount = 0;
        int[] BlobboxInfo = new int[4]; //offsetx, offsety, sizex, sizey
        Rectangle ROIRect = new Rectangle();
        int PeakCount = 0;
        int[] PeakPositionX = new int[1];
        int[] PeakPositionY = new int[1];
        int[] DrawRegion = new int[] { 10, 14 };

        try
        {
            MIL.MbufInquire(sourceimage, MIL.M_SIZE_X, ref ImageSizeX);
            MIL.MbufInquire(sourceimage, MIL.M_SIZE_Y, ref ImageSizeY);
            MIL.MbufInquire(sourceimage, MIL.M_TYPE, ref BufType);

            //binary
            MIL.MbufAlloc2d(MyMil.MilSystem, ImageSizeX, ImageSizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MBinaryImage);
            //MIL.MbufClear(MBinaryImage, MIL.M_COLOR_BLACK);
            //MIL.MimBinarize(sourceimage, MBinaryImage, MIL.M_FIXED + MIL.M_IN_RANGE, 500, 1000); //recipe
            MIL.MimBinarize(sourceimage, MBinaryImage, MIL.M_BIMODAL + MIL.M_GREATER_OR_EQUAL, MIL.M_NULL, MIL.M_NULL);

            //blob
            MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobContext);
            MIL.MblobControl(MBlobContext, MIL.M_FOREGROUND_VALUE, MIL.M_NON_ZERO);
            MIL.MblobControl(MBlobContext, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY);
            MIL.MblobControl(MBlobContext, MIL.M_BOX, MIL.M_ENABLE);
            MIL.MblobControl(MBlobContext, MIL.M_SORT1, MIL.M_AREA);
            MIL.MblobControl(MBlobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
            MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MBlobResult);
            MIL.MblobCalculate(MBlobContext, MBinaryImage, MIL.M_NULL, MBlobResult);
            MIL.MblobGetResult(MBlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT32, ref BlobCount);
            if (BlobCount == 0) { return "NoBlob"; }
            MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[0]);
            MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[1]);
            MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_X + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[2]);
            MIL.MblobGetResult(MBlobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_Y + MIL.M_TYPE_MIL_INT32, ref BlobboxInfo[3]);
            ROIRect.X = BlobboxInfo[0];
            ROIRect.Y = BlobboxInfo[1];
            ROIRect.Width = BlobboxInfo[2];
            ROIRect.Height = BlobboxInfo[3];

            //padding
            ROIRect.X += roipadding;
            ROIRect.Y += roipadding;
            ROIRect.Width -= roipadding * 2;
            ROIRect.Height -= roipadding * 2;

            //cut roi
            MIL.MbufAlloc2d(MyMil.MilSystem, (MIL_INT)ROIRect.Width, (MIL_INT)ROIRect.Height, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref MROIImage);
            MIL.MbufTransfer(sourceimage, MROIImage, ROIRect.X, ROIRect.Y, ROIRect.Width, ROIRect.Height, MIL.M_DEFAULT,
                                                0, 0, ROIRect.Width, ROIRect.Height, MIL.M_DEFAULT,
                                                MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);

            //[Smooth]n*n convolution
            if (kernelsize.Width % 2 == 0 | kernelsize.Height % 2 == 0) { return "KernelSizeError"; }
            byte[] KernelData = Enumerable.Repeat<byte>(1, kernelsize.Width * kernelsize.Height).ToArray();
            MIL.MbufAlloc2d(MyMil.MilSystem, (MIL_INT)kernelsize.Width, (MIL_INT)kernelsize.Height, 8 + MIL.M_UNSIGNED, MIL.M_KERNEL, ref MKernel);
            MIL.MbufPut(MKernel, KernelData);
            MIL.MbufControl(MKernel, MIL.M_NORMALIZATION_FACTOR, kernelsize.Width * kernelsize.Height);
            MIL.MbufControl(MKernel, MIL.M_OVERSCAN, MIL.M_MIRROR);
            MIL.MimConvolve(MROIImage, MROIImage, MKernel);

            //cut border redundant region because of mirror
            ROIRect.X += (int)Math.Floor((double)kernelsize.Width / 2);
            ROIRect.Y += (int)Math.Floor((double)kernelsize.Height / 2);
            ROIRect.Width -= (kernelsize.Width - 1);
            ROIRect.Height -= (kernelsize.Height - 1);
            MIL.MbufAlloc2d(MyMil.MilSystem, (MIL_INT)ROIRect.Width, (MIL_INT)ROIRect.Height, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref MConvolutionImage);
            MIL.MbufTransfer(MROIImage, MConvolutionImage, (MIL_INT)Math.Floor((double)kernelsize.Width / 2), (MIL_INT)Math.Floor((double)kernelsize.Height / 2), ROIRect.Width, ROIRect.Height, MIL.M_DEFAULT,
                                                0, 0, ROIRect.Width, ROIRect.Height, MIL.M_DEFAULT,
                                                MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);

            //Min/Max value 0:min, 1:max
            //Array.Clear(PeakValue, 0, PeakValue.Length);
            MIL.MimAllocResult(MyMil.MilSystem, 2, MIL.M_EXTREME_LIST + MIL.M_FLOAT, ref MIPResult);
            MIL.MimFindExtreme(MConvolutionImage, MIPResult, MIL.M_MIN_VALUE + MIL.M_MAX_VALUE);
            MIL.MimGetResult1d(MIPResult, 0, 2, MIL.M_VALUE + MIL.M_TYPE_MIL_DOUBLE, PeakValue);
            MilNetHelper.MilMimFree(ref MIPResult);

            //position of min value
            MIL.MimAllocResult(MyMil.MilSystem, (MIL_INT)ROIRect.Width * ROIRect.Height, MIL.M_EVENT_LIST, ref MIPResult);
            MIL.MimLocateEvent(MConvolutionImage, MIPResult, MIL.M_EQUAL, PeakValue[0], MIL.M_NULL);
            MIL.MimGetResult(MIPResult, MIL.M_NB_EVENT + MIL.M_TYPE_MIL_INT32, ref PeakCount); //peak count definitely is >=1
            Array.Resize(ref PeakPositionX, PeakCount);
            //Array.Clear(PeakPositionX, 0, PeakCount);
            Array.Resize(ref PeakPositionY, PeakCount);
            //Array.Clear(PeakPositionY, 0, PeakCount);
            Array.Resize(ref MinPeakPosition, PeakCount);
            //Array.Clear(MinPeakPosition, 0, PeakCount);
            MIL.MimGetResult1d(MIPResult, 0, PeakCount, MIL.M_POSITION_X + MIL.M_TYPE_MIL_INT32, PeakPositionX);
            MIL.MimGetResult1d(MIPResult, 0, PeakCount, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_INT32, PeakPositionY);
            for (int peakIndex = 0; peakIndex < PeakCount; peakIndex++)
            {
                MinPeakPosition[peakIndex].X = ROIRect.X + PeakPositionX[peakIndex];
                MinPeakPosition[peakIndex].Y = ROIRect.Y + PeakPositionY[peakIndex];
            }
            MilNetHelper.MilMimFree(ref MIPResult);

            //position of max value
            MIL.MimAllocResult(MyMil.MilSystem, (MIL_INT)ROIRect.Width * ROIRect.Height, MIL.M_EVENT_LIST, ref MIPResult);
            MIL.MimLocateEvent(MConvolutionImage, MIPResult, MIL.M_EQUAL, PeakValue[1], MIL.M_NULL);
            MIL.MimGetResult(MIPResult, MIL.M_NB_EVENT + MIL.M_TYPE_MIL_INT32, ref PeakCount); //peak count must be >=1
            Array.Resize(ref PeakPositionX, PeakCount);
            //Array.Clear(PeakPositionX, 0, PeakCount);
            Array.Resize(ref PeakPositionY, PeakCount);
            //Array.Clear(PeakPositionY, 0, PeakCount);
            Array.Resize(ref MaxPeakPosition, PeakCount);
            //Array.Clear(MaxPeakPosition, 0, PeakCount);
            MIL.MimGetResult1d(MIPResult, 0, PeakCount, MIL.M_POSITION_X + MIL.M_TYPE_MIL_INT32, PeakPositionX);
            MIL.MimGetResult1d(MIPResult, 0, PeakCount, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_INT32, PeakPositionY);
            for (int peakIndex = 0; peakIndex < PeakCount; peakIndex++)
            {
                MaxPeakPosition[peakIndex].X = ROIRect.X + PeakPositionX[peakIndex];
                MaxPeakPosition[peakIndex].Y = ROIRect.Y + PeakPositionY[peakIndex];
            }
            //MilNetHelper.MilMimFree(ref MIPResult);

            //draw & save
            MIL.MbufAlloc2d(MyMil.MilSystem, ImageSizeX, ImageSizeY, BufType, MIL.M_IMAGE + MIL.M_PROC, ref MDrawImage);
            MIL.MbufCopy(sourceimage, MDrawImage);
            MIL.MgraAlloc(MyMil.MilSystem, ref MGraContext);
            MIL.MgraColor(MGraContext, MIL.M_COLOR_BLACK);
            foreach (Point P in MinPeakPosition)
            {
                for (int drawSize = DrawRegion[0]; drawSize <= DrawRegion[1]; drawSize++)
                {
                    MIL.MgraRect(MGraContext, MDrawImage, P.X - drawSize, P.Y - drawSize, P.X + drawSize, P.Y + drawSize);
                }
            }
            foreach (Point P in MaxPeakPosition)
            {
                for (double drawSize = DrawRegion[0]; drawSize <= DrawRegion[1]; drawSize += 0.5)
                {
                    MIL.MgraArc(MGraContext, MDrawImage, P.X, P.Y, drawSize, drawSize, 0, 360);
                }
            }
            MIL.MgraRect(MGraContext, MDrawImage, ROIRect.X, ROIRect.Y, ROIRect.X + ROIRect.Width - 1, ROIRect.Y + ROIRect.Height - 1);
            MIL.MbufExport(SaveFolder + "\\ExtremePos_Convolution_" + imagename + ".tif", MIL.M_TIFF + MIL.M_COMPRESSION_NONE, MDrawImage);
            return "OK";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref MBinaryImage);
            MilNetHelper.MilBufferFree(ref MROIImage);
            MilNetHelper.MilBlobFree(ref MBlobContext);
            MilNetHelper.MilBlobFree(ref MBlobResult);
            MilNetHelper.MilBufferFree(ref MKernel);
            MilNetHelper.MilBufferFree(ref MConvolutionImage);
            MilNetHelper.MilMimFree(ref MIPResult);
            MilNetHelper.MilBufferFree(ref MDrawImage);
            MIL.MgraFree(MGraContext);
        }
    }
    #endregion

    #region --- Get Roi Mean ---

    /// <summary>
    ///  This function use mil calculate the mean value within a circular ROI
    /// </summary>
    /// 
    #region --- StatCircleMean ---

    public static double StatCircleMean(
        MIL_ID img,
        CircleRegionInfo regionInfo,
        double BypassPercent = 0.0)
    {
        double mean = 0;
        MIL_ID mask = MIL.M_NULL;
        MIL_ID BinaryImage = MIL.M_NULL;
        MIL_ID BinaryChildImage = MIL.M_NULL;
        MIL_ID MaskChildImage = MIL.M_NULL;

        try
        {
            MIL_INT sizeX = MIL.MbufInquire(img, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT sizeY = MIL.MbufInquire(img, MIL.M_SIZE_Y, MIL.M_NULL);

            MIL.MbufAlloc2d(
                MyMil.MilSystem,
                sizeX,
                sizeY,
                1 + MIL.M_UNSIGNED,
                MIL.M_IMAGE + MIL.M_PROC,
                ref mask);
            MIL.MbufClear(mask, 0);

            MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);

            MIL.MgraArcFill(
                MIL.M_DEFAULT,
                mask,
                (double)regionInfo.CenterX,
                (double)regionInfo.CenterY,
                (double)regionInfo.Radius,
                (double)regionInfo.Radius,
                0,
                360);

            if (BypassPercent > 0)
            {
                System.Drawing.Rectangle childrect = new System.Drawing.Rectangle(regionInfo.CenterX - regionInfo.Radius, regionInfo.CenterY - regionInfo.Radius, regionInfo.Radius * 2 + 1, regionInfo.Radius * 2 + 1);
                MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref BinaryImage);
                MIL.MbufChild2d(mask, childrect.X, childrect.Y, childrect.Width, childrect.Height, ref MaskChildImage);
                MIL.MimBinarize(img, BinaryImage, MIL.M_PERCENTILE_VALUE + MIL.M_LESS, 100 - BypassPercent, MIL.M_NULL);
                MIL.MbufChild2d(BinaryImage, childrect.X, childrect.Y, childrect.Width, childrect.Height, ref BinaryChildImage);
                MIL.MimArith(MaskChildImage, BinaryChildImage, MaskChildImage, MIL.M_MULT);
            }

            //MIL.MbufSave(@"D:\Test\maskCircle.tif", mask);

            mean = StatMaskMean(img, mask);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("[StatCircleMean] {0}", ex.Message));
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref MaskChildImage);
            MilNetHelper.MilBufferFree(ref BinaryChildImage);
            MilNetHelper.MilBufferFree(ref mask);
            MilNetHelper.MilBufferFree(ref BinaryImage);
        }

        return mean;
    }

    #endregion

    /// <summary>
    ///  This function use mil calculate the mean value within a rect ROI
    /// </summary>
    /// 
    #region --- StatRectMean ---
    public static double StatRectMean(MIL_ID img, RectRegionInfo regionInfo)
    {
        double mean = 0;
        MIL_ID mask = MIL.M_NULL;

        try
        {
            MIL_INT sizeX = MIL.MbufInquire(img, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT sizeY = MIL.MbufInquire(img, MIL.M_SIZE_Y, MIL.M_NULL);

            MIL.MbufAlloc2d(
                MyMil.MilSystem,
                sizeX,
                sizeY,
                1 + MIL.M_UNSIGNED,
                MIL.M_IMAGE + MIL.M_PROC,
                ref mask);

            MIL.MbufClear(mask, 0);

            MIL.MgraRectFill(
                MIL.M_DEFAULT,
                mask,
                (double)regionInfo.StartX,
                (double)regionInfo.StartY,
                (double)(regionInfo.StartX + regionInfo.Width),
                (double)(regionInfo.StartY + regionInfo.Height));

            MIL.MbufSave(@"D:\maskRect.tif", mask);

            mean = StatMaskMean(img, mask);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("[StatCircleMean] {0}", ex.Message));
        }
        finally
        {
            MilNetHelper.MilBufferFree(ref mask);
        }

        return mean;
    }
    #endregion

    /// <summary>
    ///  This function use mil calculate the mean value within a mask
    /// </summary>
    /// 
    #region --- StatMaskMean ---
    public static double StatMaskMean(MIL_ID img, MIL_ID mask)
    {
        double mean = 0;
        MIL_ID statContext = MIL.M_NULL;
        MIL_ID statResult = MIL.M_NULL;

        try
        {
            MIL.MimAlloc(MyMil.MilSystem, MIL.M_STATISTICS_CONTEXT, MIL.M_DEFAULT, ref statContext);
            MIL.MimControl(statContext, MIL.M_STAT_MEAN, MIL.M_ENABLE);

            MIL.MimAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_STATISTICS_RESULT, ref statResult);

            MIL.MbufSetRegion(img, mask, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
            MIL.MimStatCalculate(statContext, img, statResult, MIL.M_DEFAULT);
            MIL.MimGetResult(statResult, MIL.M_STAT_MEAN, ref mean);
            MIL.MbufSetRegion(img, MIL.M_NULL, MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("[StatMaskMean] {0}", ex.Message));
        }
        finally
        {
            if (statResult != MIL.M_NULL)
            {
                MIL.MimFree(statResult);
                statResult = MIL.M_NULL;
            }

            if (statContext != MIL.M_NULL)
            {
                MIL.MimFree(statContext);
                statContext = MIL.M_NULL;
            }
        }

        return mean;
    }

    #endregion

    #endregion

    #region --- SpectralCorr --

    public static void SpectralCorr(MilColorImage PredictFile, double[] ExpTime, CorrectionPara CorrData, ref MilColorImage tristimulusImage, ref MilChromaImage chromaImage)
    {
        tristimulusImage = null;

        MIL_ID imgTristimulusX = MIL.M_NULL;
        MIL_ID imgTristimulusY = MIL.M_NULL;
        MIL_ID imgTristimulusZ = MIL.M_NULL;
        MIL_ID imgXYZ_Sum = MIL.M_NULL;
        MIL_ID imgChromaX = MIL.M_NULL;
        MIL_ID imgChromaY = MIL.M_NULL;


        try
        {
            MIL_INT sizeX = MIL.MbufInquire(PredictFile.ImgX, MIL.M_SIZE_X);
            MIL_INT sizeY = MIL.MbufInquire(PredictFile.ImgX, MIL.M_SIZE_Y);

            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusX);
            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);
            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusZ);
            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgXYZ_Sum);
            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgChromaX);
            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgChromaY);

            MIL.MbufClear(imgTristimulusX, 0);
            MIL.MbufClear(imgTristimulusY, 0);
            MIL.MbufClear(imgTristimulusZ, 0);
            MIL.MbufClear(imgXYZ_Sum, 0);
            MIL.MbufClear(imgChromaX, 0);
            MIL.MbufClear(imgChromaY, 0);

            ExpTime[0] /= 1000000;
            MIL.MimArith(PredictFile.ImgX, ExpTime[0], imgTristimulusX, MIL.M_DIV_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusX, CorrData.X_Coefficient.Slope, imgTristimulusX, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusX, CorrData.X_Coefficient.Intercept, imgTristimulusX, MIL.M_ADD_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusX, CorrData.X_Coefficient.Alpha, imgTristimulusX, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);

            ExpTime[1] /= 1000000;
            MIL.MimArith(PredictFile.ImgY, ExpTime[1], imgTristimulusY, MIL.M_DIV_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Slope, imgTristimulusY, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Intercept, imgTristimulusY, MIL.M_ADD_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Alpha, imgTristimulusY, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);

            ExpTime[2] /= 1000000;
            MIL.MimArith(PredictFile.ImgZ, ExpTime[2], imgTristimulusZ, MIL.M_DIV_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusZ, CorrData.Z_Coefficient.Slope, imgTristimulusZ, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusZ, CorrData.Z_Coefficient.Intercept, imgTristimulusZ, MIL.M_ADD_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusZ, CorrData.Z_Coefficient.Alpha, imgTristimulusZ, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);

            tristimulusImage = new MilColorImage(imgTristimulusX, imgTristimulusY, imgTristimulusZ);

            // X + Y + Z
            MIL.MimArith(imgTristimulusX, imgTristimulusY, imgXYZ_Sum, MIL.M_ADD);
            MIL.MimArith(imgXYZ_Sum, imgTristimulusZ, imgXYZ_Sum, MIL.M_ADD);

            // ChromaX, Y
            MIL.MimArith(imgTristimulusX, imgXYZ_Sum, imgChromaX, MIL.M_DIV);
            MIL.MimArith(imgTristimulusY, imgXYZ_Sum, imgChromaY, MIL.M_DIV);

            chromaImage = new MilChromaImage(imgChromaX, imgChromaY);

        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("SpectralCorr Error - {0}", ex.Message));
        }
        finally
        {
            if (PredictFile != null)
            {
                PredictFile.Free();
            }

            if(imgXYZ_Sum != MIL.M_NULL)
            {
                MIL.MbufFree(imgXYZ_Sum);
            }
        }
    }

    public static void SpectralCorrY(MilMonoImage PredictFile, double ExpTime, CorrectionPara CorrData, ref MilColorImage tristimulusImage)
    {
        tristimulusImage = null;

        MIL_ID imgTristimulusX = MIL.M_NULL;
        MIL_ID imgTristimulusY = MIL.M_NULL;
        MIL_ID imgTristimulusZ = MIL.M_NULL;

        try
        {
            MIL_INT sizeX = MIL.MbufInquire(PredictFile.Img, MIL.M_SIZE_X);
            MIL_INT sizeY = MIL.MbufInquire(PredictFile.Img, MIL.M_SIZE_Y);

            MIL.MbufAlloc2d(MyMil.MilSystem, sizeX, sizeY, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);

            MIL.MbufClear(imgTristimulusY, 0);

            ExpTime /= 1000000;
            MIL.MimArith(PredictFile.Img, ExpTime, imgTristimulusY, MIL.M_DIV_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Slope, imgTristimulusY, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Intercept, imgTristimulusY, MIL.M_ADD_CONST + MIL.M_FLOAT_PROC);
            MIL.MimArith(imgTristimulusY, CorrData.Y_Coefficient.Alpha, imgTristimulusY, MIL.M_MULT_CONST + MIL.M_FLOAT_PROC);

            tristimulusImage = new MilColorImage(imgTristimulusX, imgTristimulusY, imgTristimulusZ);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Mesure_TristimulusY_Predict Error - {0}", ex.Message));
        }
        finally
        {
            if (PredictFile != null)
            {
                PredictFile.Free();
            }
        }

    }

    #endregion
}

public class myTimer
{
    public double timeSpend;
    private DateTime startTime;
    private DateTime endTime;

    public myTimer()
    {
        timeSpend = 0.0;
        startTime = DateTime.Now;
        endTime = DateTime.Now;
    }

    public void Start()
    {
        startTime = DateTime.Now;
    }

    public void Stop()
    {
        endTime = DateTime.Now;

        TimeSpan ts = endTime.Subtract(startTime);
        timeSpend = ts.TotalMilliseconds;
    }
}


public enum FindBorlderType
{
    Horizontal,
    Vertical
}

public class XyzFilePath
{
    public string X;
    public string Y;
    public string Z;
}

public class FfcCalibrationFilePath
{
    public string Offset;
    public string Flat;
    public string Dark;
}


public class LaserCalibrationParam
{
    public bool IsDebug;
    public string Template_Path;
    public string Target_Path;
    public string Output_Path;
    public System.Drawing.Rectangle ROI;
    public MIL_ID WaitCorrectId = MIL.M_NULL;
}

public class PixelAlignParam
{
    public string Image_Path;
    public PanelInfo panelInfo;
    public PixelAlignSetting setting;

    public PixelAlignParam()
    {
        panelInfo = new PanelInfo();
        setting = new PixelAlignSetting();
    }
}

public class PixelStatParam
{
    public bool DrawCircle = false;
    public string Image_Path;
    public string Output_Path;
    public string CogDataFile_Path;
    public string TristimulusX_Path;
    public string TristimulusY_Path;
    public string TristimulusZ_Path;
    public string ChromaCx_Path;
    public string ChromaCy_Path;
    public int radius;
    public PanelInfo panelInfo;
    public PixelAlignSetting setting;

    public PixelStatParam()
    {
        panelInfo = new PanelInfo();
        setting = new PixelAlignSetting();
    }
}


public class ColorMura_DetectParam
{
    public string ImgColor_Path;
    public string ImgX_Path;
    public string ImgY_Path;
    public string ImgZ_Path;

    public int ThresHigh;
    public int ThresLow;
    public int Open;
    public int Dilate;
    public int MinArea;

    public int Numerator_Channel;
    public int Denominator_Channel;
}

