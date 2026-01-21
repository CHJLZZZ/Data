using CommonBase.Config;
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


#region --- MyMil ---


public static class MyMil
{
    public static MIL_ID MilApplication = MIL.M_NULL;
    public static MIL_ID MilSystem = MIL.M_NULL;
    public static MIL_ID MilMainDisplay = MIL.M_NULL;
    public static MIL_ID MilSubDisplay = MIL.M_NULL;

    #region --- 方法函式 ---

    #region --- Init ---
    public static void Init(string system)
    {
        MIL.MappAlloc(MIL.M_NULL, MIL.M_DEFAULT, ref MilApplication);

        switch (system)
        {
            case "M_SYSTEM_DEFAULT":
            case "M_SYSTEM_SOLIOS":
            case "M_SYSTEM_RADIENTPRO":
            case "M_SYSTEM_RADIENTEVCL":
            case "M_SYSTEM_RADIENTCXP":
            case "M_SYSTEM_RAPIXOCXP":
                {
                    MIL.MsysAlloc(MIL.M_DEFAULT, system, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem);
                }
                break;

            case "M_SYSTEM_HOST":
            case "Z_SYSTEM":
                {
                    MIL.MsysAlloc(MIL.M_DEFAULT, "M_SYSTEM_HOST", MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem);
                }
                break;
        }

        MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref MilMainDisplay);
        MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref MilSubDisplay);

        // Enable Overlay
        MIL.MdispControl(MilMainDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
    }
    #endregion

    #region --- Close ---
    public static void Close()
    {
        MIL.MappFreeDefault(MilApplication, MilSystem, MilSubDisplay, MIL.M_NULL, MIL.M_NULL);
        MIL.MappFreeDefault(MilApplication, MilSystem, MilMainDisplay, MIL.M_NULL, MIL.M_NULL);
    }
    #endregion

    #endregion
}

#endregion 

#region --- MyMilDisplay --
public static class MyMilDisplay
{
    // --- MilMainDisplayImage 代表影像如下 :
    // 0 -> source image
    // 1 -> original view image
    // 2 -> heat map

    public static MIL_ID[] MilMainDisplayImage = new MIL_ID[] { MIL.M_NULL, MIL.M_NULL, MIL.M_NULL };
    public static MIL_ID MilSubDisplayImage = MIL.M_NULL;

    public static MIL_ID MilGraphicsContext = MIL.M_NULL;
    public static MIL_ID MilGraphicsList = MIL.M_NULL;
    private static MIL_ID MilGrayImage = MIL.M_NULL;
    private static MIL_ID MilTargetImage = MIL.M_NULL;

    public static System.Windows.Forms.Panel GrabViewer { get; set; } = null;

    public static bool ShowGrid = true;
    //public static bool Is
    public static double MousePosX, MousePosY, ZoomValue, GrayValue;
    public static double GridSize = 0.5;
    public static double orgZoom = 0.0;
    public static double oldZoom = 0.0;
    public static int[] MouseGrayValue = new int[1] { 0 };

    private static MIL_DISP_HOOK_FUNCTION_PTR mouseLeftDownEventDelegate;
    private static MIL_DISP_HOOK_FUNCTION_PTR mouseMoveEventDelegate;
    private static MIL_DISP_HOOK_FUNCTION_PTR mouseWheelEventDelegate;
    private static MIL_DISP_HOOK_FUNCTION_PTR keyUpEventDelegate;

    public static event Action<object> GraphicSelectValueChangedEvent;
    public static event Action<int> GraphicDeleteEvent;

    #region --- 方法函式 ---

    #region --- Init ---
    public static void Init()
    {
        // Background Color
        MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_BACKGROUND_COLOR, MIL.M_RGB888(30, 30, 30));

        // Allocate MIL Graphics
        MIL.MgraAllocList(MyMil.MilSystem, MIL.M_DEFAULT, ref MilGraphicsList);
        MIL.MgraAlloc(MyMil.MilSystem, ref MilGraphicsContext);

        // Display mouse event
        mouseLeftDownEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseLeftDownEvent);
        MIL.MdispHookFunction(MyMil.MilMainDisplay, MIL.M_MOUSE_LEFT_BUTTON_DOWN, mouseLeftDownEventDelegate, IntPtr.Zero);
        mouseWheelEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseWheelEvent);
        MIL.MdispHookFunction(MyMil.MilMainDisplay, MIL.M_MOUSE_WHEEL, mouseWheelEventDelegate, IntPtr.Zero);
        mouseMoveEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseMoveEvent);
        MIL.MdispHookFunction(MyMil.MilMainDisplay, MIL.M_MOUSE_MOVE, mouseMoveEventDelegate, IntPtr.Zero);
        keyUpEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayKeyUpEvent);
        MIL.MdispHookFunction(MyMil.MilMainDisplay, MIL.M_KEY_UP, keyUpEventDelegate, IntPtr.Zero);
    }
    #endregion

    #region --- DisplayMouseWheelEvent ---
    private static MIL_INT DisplayMouseWheelEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
    {
        try
        {
            double curZoom = 0;
            double ZoomFit = 0.0;
            int DisplayWidth = 0;
            int DisplayHeight = 0;

            // 沒影像就return
            if (MyMil.MilMainDisplay == MIL.M_NULL) return 0;

            if (GrabViewer != null && MilMainDisplayImage[1] != MIL.M_NULL)
            {
                MIL_INT SizeX = 0;
                MIL_INT SizeY = 0;
                DisplayWidth = GrabViewer.Size.Width;
                DisplayHeight = GrabViewer.Size.Height;

                SizeX = MIL.MbufInquire(MilMainDisplayImage[1], MIL.M_SIZE_X, MIL.M_NULL);
                SizeY = MIL.MbufInquire(MilMainDisplayImage[1], MIL.M_SIZE_Y, MIL.M_NULL);

                if (SizeX > 0 && SizeY > 0)
                {
                    ZoomFit = Math.Min((double)DisplayWidth / (double)SizeX, (double)DisplayHeight / (double)SizeY);
                    //MIL.MdispZoom(MyMil.MilMainDisplay, oldZoom, oldZoom);
                }
            }

            // 不能小於最適尺寸
            MIL.MdispInquire(MyMil.MilMainDisplay, MIL.M_ZOOM_FACTOR_X, ref curZoom);

            if (oldZoom == 0) oldZoom = orgZoom;
            if (curZoom < oldZoom)
            {
                //oldZoom = oldZoom * 0.5 > orgZoom ? oldZoom * 0.5 : orgZoom;
                oldZoom = oldZoom * 0.5;

                if (oldZoom < ZoomFit)
                    oldZoom = ZoomFit;              
            }
            else
            {
                oldZoom = oldZoom * 2;
            }
                   
            MIL.MdispZoom(MyMil.MilMainDisplay, oldZoom, oldZoom);

            //if ((curZoom < orgZoom && oldZoom > orgZoom) || (curZoom > orgZoom && oldZoom < orgZoom))
            //{
            //    oldZoom = orgZoom * curZoom;
            //    MIL.MdispZoom(MyMil.MilMainDisplay, oldZoom, oldZoom);
            //}
            //else if (curZoom < orgZoom)
            //{
            //    oldZoom = orgZoom * curZoom;
            //    MIL.MdispZoom(MyMil.MilMainDisplay, oldZoom, oldZoom);
            //}
        }
        catch 
        {

        }
        return 0;
    }
    #endregion

    #region --- DisplayMouseMoveEvent ---
    private static MIL_INT DisplayMouseMoveEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
    {
        try
        {
            if (MyMil.MilMainDisplay == MIL.M_NULL || MyMil.MilMainDisplay == MIL.M_NULL) return 0;
            MIL.MdispGetHookInfo(eventId, MIL.M_MOUSE_POSITION_BUFFER_X, ref MousePosX);
            MIL.MdispGetHookInfo(eventId, MIL.M_MOUSE_POSITION_BUFFER_Y, ref MousePosY);

            MIL_INT SX = MIL.MbufInquire(MilMainDisplayImage[1], MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT SY = MIL.MbufInquire(MilMainDisplayImage[1], MIL.M_SIZE_Y, MIL.M_NULL);

            MousePosX = Math.Floor(MousePosX + 0.5);
            MousePosY = Math.Floor(MousePosY + 0.5);
            if (MousePosX >= 0 && MousePosX <= SX - 1 && MousePosY >= 0 && MousePosY <= SY - 1)
            {
                MIL.MbufGet2d(MilMainDisplayImage[1], (MIL_INT)MousePosX, (MIL_INT)MousePosY, 1, 1, MouseGrayValue);

                //MIL.MbufSave(@"D:\GrabImage.tif", MilMainDisplayImage[1]);
            }
            else
            {
                MousePosX = -1;
                MousePosY = -1;
            }
            //MIL.MbufGetColor2d(MilDisplay, MIL.M_SINGLE_BAND, 2, (MIL_INT)MousePosX, (MIL_INT)MousePosY, 1, 1, ref GrayValue);
        }
        catch 
        {

        }
        return 0;
    }
    #endregion

    #region --- DisplayMouseLeftDownEvent ---
    private static MIL_INT DisplayMouseLeftDownEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
    {
        try
        {
            MIL_INT listCount = 0;
            MIL_INT selectLabel = 0;

            MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref listCount);
            for (MIL_INT i = 0; i < listCount; i++)
            {
                // Get Selected
                if (MIL.MgraInquireList(MilGraphicsList,
                    MIL.M_GRAPHIC_INDEX(i),
                    MIL.M_DEFAULT,
                    MIL.M_GRAPHIC_SELECTED,
                    MIL.M_NULL) == MIL.M_TRUE)
                {
                    MIL.MgraInquireList(MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE, ref selectLabel);
                    GraphicSelectValueChangedEvent?.Invoke((int)selectLabel);
                    //U.SelectedItem = DicRegion.Where(x => x.Key == selectLabel).FirstOrDefault();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception");
        }

        return 0;
    }
    #endregion

    #region --- DisplayKeyUpEvent ---
    private static MIL_INT DisplayKeyUpEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
    {
        MIL_INT keyValue = 0;
        if (MyMil.MilMainDisplay == MIL.M_NULL || MilMainDisplayImage[1] == MIL.M_NULL) return 0;
        MIL.MdispGetHookInfo(eventId, MIL.M_KEY_VALUE, ref keyValue);

        if (keyValue == MIL.M_KEY_DELETE)
        {
            MIL_INT listCount = 0;
            MIL_INT selectLabel = 0;

            MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref listCount);
            for (MIL_INT i = 0; i < listCount; i++)
            {
                // Get Selected
                if (MIL.MgraInquireList(MilGraphicsList,
                    MIL.M_GRAPHIC_INDEX(i),
                    MIL.M_DEFAULT,
                    MIL.M_GRAPHIC_SELECTED,
                    MIL.M_NULL) == MIL.M_TRUE)
                {
                    MIL.MgraInquireList(MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE, ref selectLabel);
                    MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
                    GraphicDeleteEvent?.Invoke((int)selectLabel);
                    break;
                }
            }
        }
        return 0;
    }
    #endregion

    #region --- Close ---
    public static void Close()
    {
        MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

        MilNetHelper.MilBufferFree(ref MilMainDisplayImage[1]);
        MilNetHelper.MilBufferFree(ref MilMainDisplayImage[2]);
        MilNetHelper.MilBufferFree(ref MilTargetImage);
        MilNetHelper.MilBufferFree(ref MilGrayImage);      

        if (MilGraphicsList != MIL.M_NULL)
        {
            MIL.MgraFree(MilGraphicsList);
            MilGraphicsList = MIL.M_NULL;
        }

        if (MilGraphicsContext != MIL.M_NULL)
        {
            MIL.MgraFree(MilGraphicsContext);
            MilGraphicsContext = MIL.M_NULL;
        }
    }
    #endregion

    private static double curSizeX = 0.0;
    private static double curSizeY = 0.0;

    #region --- SetDisplayImage ---
    public static void SetDisplayImage(ref MIL_ID image, int i = 0)
    {
        //if (MilMainDisplayImage[i] != MIL.M_NULL)   // 2023/04/07 Rick add
        //{
        //    MIL.MbufFree(MilMainDisplayImage[i]);
        //    MilMainDisplayImage[i] = MIL.M_NULL;
        //}

        MIL.MbufClone(image,
                      MIL.M_DEFAULT,
                      MIL.M_DEFAULT,
                      MIL.M_DEFAULT,
                      MIL.M_DEFAULT,
                      MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP,
                      MIL.M_DEFAULT,
                      ref MilMainDisplayImage[i]);

        //MIL.MbufSave(@"D:\SrcImage.tif", image);
        //MIL.MbufSave(@"D:\MilMainDisplayImage_1.tif", MilMainDisplayImage[i]);

        MIL.MbufCopy(image, MilMainDisplayImage[i]);
        //.MIL.MbufSave(@"D:\MilMainDisplayImage_2.tif", MilMainDisplayImage[i]);
    }
    #endregion

    #region --- SetMainDisplayFit ---
    public static void SetMainDisplayFit(MIL_ID Img, IntPtr HandlePanel, int i = 1)
    {      
        if (Img == MIL.M_NULL) return;

        MIL_INT DisplayBits = 0;
        MIL_INT SourceBits = 0;

        if (MilMainDisplayImage[i] != MIL.M_NULL)
            DisplayBits = MIL.MbufInquire(MilMainDisplayImage[i], MIL.M_SIZE_BIT, MIL.M_NULL);

        if (Img != MIL.M_NULL)
            SourceBits = MIL.MbufInquire(Img, MIL.M_SIZE_BIT, MIL.M_NULL);

        try
        {
            if (DisplayBits != SourceBits || MilMainDisplayImage[i] == MIL.M_NULL)
            {
                MilNetHelper.MilBufferFree(ref MilMainDisplayImage[i]);

                MIL.MbufAllocColor(MyMil.MilSystem,
                                   MIL.MbufInquire(Img, MIL.M_SIZE_BAND, MIL.M_NULL),
                                   MIL.MbufInquire(Img, MIL.M_SIZE_X, MIL.M_NULL),
                                   MIL.MbufInquire(Img, MIL.M_SIZE_Y, MIL.M_NULL),
                                   MIL.MbufInquire(Img, MIL.M_SIZE_BIT, MIL.M_NULL) + MIL.M_UNSIGNED,
                                   MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP,
                                   ref MilMainDisplayImage[i]);
            }

            //if (MilMainDisplayImage[i] != MIL.M_NULL)
            //{
            //    MIL.MbufFree(MilMainDisplayImage[i]);
            //    MilMainDisplayImage[i] = MIL.M_NULL;
            //}

            MIL.MbufCopy(Img, MilMainDisplayImage[i]);

            MIL.MdispSelectWindow(MyMil.MilMainDisplay, MilMainDisplayImage[i], HandlePanel);

            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_MOUSE_USE, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, MilGraphicsList);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_KEYBOARD_USE, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_GRAPHIC_LIST_INTERACTIVE, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, MilGraphicsList);

            if (MilMainDisplayImage[i] != MIL.M_NULL)
            {
                curSizeX = MIL.MbufInquire(MilMainDisplayImage[i], MIL.M_SIZE_X, MIL.M_NULL);
                curSizeY = MIL.MbufInquire(MilMainDisplayImage[i], MIL.M_SIZE_Y, MIL.M_NULL);
            }
            else
            {
                throw new Exception("Image is Nothing.");
            }

            //SetGrid(GridSize);

            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_VIEW_MODE, MIL.M_AUTO_SCALE);

            MIL.MdispInquire(MyMil.MilMainDisplay, MIL.M_ZOOM_FACTOR_X, ref orgZoom);
        }
        catch
        {

        }
    }
    #endregion

    #region --- SetSubDisplay ---
    public static void SetSubDisplay(MIL_ID Img, IntPtr HandlePanel)
    {
        try
        {
            MilNetHelper.MilBufferFree(ref MilSubDisplayImage);

            MIL.MbufAllocColor(MyMil.MilSystem,
                               MIL.MbufInquire(Img, MIL.M_SIZE_BAND, MIL.M_NULL),
                               MIL.MbufInquire(Img, MIL.M_SIZE_X, MIL.M_NULL),
                               MIL.MbufInquire(Img, MIL.M_SIZE_Y, MIL.M_NULL),
                               MIL.MbufInquire(Img, MIL.M_SIZE_BIT, MIL.M_NULL) + MIL.M_UNSIGNED,
                               MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP,
                               ref MilSubDisplayImage);
            MIL.MbufCopy(Img, MilSubDisplayImage);

            MIL.MdispSelectWindow(MyMil.MilSubDisplay, MilSubDisplayImage, HandlePanel);

            MIL.MdispControl(MyMil.MilSubDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);
            MIL.MdispControl(MyMil.MilSubDisplay, MIL.M_GRAPHIC_LIST_INTERACTIVE, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilSubDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(MyMil.MilSubDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, MilGraphicsList);
        }
        catch 
        {

        }
    }
    #endregion

    #region --- UpdateMainDisplay ---
    public static void UpdateMainDisplay(MIL_ID Img)
    {
        try
        {
            MIL.MbufCopy(Img, MilMainDisplayImage[1]);
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_UPDATE, MIL.M_NOW);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    #endregion

    #region --- DrawBox ---
    public static void DrawBox(System.Drawing.Point[] TargetPoints, System.Drawing.Point[] SourcePoints)
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
                MIL.MgraClear(MIL.M_DEFAULT, MilGraphicsList);

                // Target Points

                for (int i = 0; i < Counts - 1; i++)
                {
                    XStart[i] = TargetPoints[i].X;
                    YStart[i] = TargetPoints[i].Y;
                    XEnd[i] = TargetPoints[i + 1].X;
                    YEnd[i] = TargetPoints[i + 1].Y;
                }

                XStart[3] = TargetPoints[0].X;
                YStart[3] = TargetPoints[0].Y;
                XEnd[3] = TargetPoints[3].X;
                YEnd[3] = TargetPoints[3].Y;

                MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_RED);
                MIL.MgraLines(MilGraphicsContext, MilGraphicsList, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);

                // Source Points

                for (int i = 0; i < Counts - 1; i++)
                {
                    XStart[i] = SourcePoints[i].X;
                    YStart[i] = SourcePoints[i].Y;
                    XEnd[i] = SourcePoints[i + 1].X;
                    YEnd[i] = SourcePoints[i + 1].Y;
                }

                XStart[3] = SourcePoints[0].X;
                YStart[3] = SourcePoints[0].Y;
                XEnd[3] = SourcePoints[3].X;
                YEnd[3] = SourcePoints[3].Y;

                MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_YELLOW);
                MIL.MgraLines(MilGraphicsContext, MilGraphicsList, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);

                MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_GREEN);
                //MIL.MgraRectFill(MilGraphicsContext, MilGraphicsList, SourcePoints[3].X, SourcePoints[3].Y, SourcePoints[3].X + 1, SourcePoints[3].Y + 1);

                MIL.MgraRectFill(MilGraphicsContext, MilGraphicsList, 595, 5990, 596, 5990 + 1);
                MIL.MgraRectFill(MilGraphicsContext, MilGraphicsList, 0, 0, 1, 1);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
    
    #endregion

    #region --- SetGrid ---
    public static void SetGrid(double ratio)
    {
        MIL.MgraClear(MIL.M_DEFAULT, MilGraphicsList);

        MIL_INT graphicLabel = -1;

        //十字
        MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_YELLOW);
        MIL.MgraLine(MilGraphicsContext, MilGraphicsList, curSizeX * 0.5, 1, curSizeX * 0.5, curSizeY - 1);
        MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);

        MIL.MgraLine(MilGraphicsContext, MilGraphicsList, 1, curSizeY * 0.5, curSizeX - 1, curSizeY * 0.5);
        MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);

        //方框
        MIL_INT startX = (MIL_INT)(curSizeX * (0.5 - ratio / 2.0));
        MIL_INT startY = (MIL_INT)(curSizeY * (0.5 - ratio / 2.0));
        MIL_INT endX = (MIL_INT)(curSizeX - curSizeX * (0.5 - ratio / 2.0));
        MIL_INT endY = (MIL_INT)(curSizeY - curSizeY * (0.5 - ratio / 2.0));

        MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_YELLOW);
        MIL.MgraRect(MilGraphicsContext, MilGraphicsList, startX, startY, endX, endY);
        MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
    }
    #endregion

    #region --- ClearGrid ---
    public static void ClearGrid()
    {
        MIL.MgraClear(MIL.M_DEFAULT, MilGraphicsList);
    }
    #endregion

    #region --- DrawCircle ---
    public static MIL_INT DrawCircle(int centerX, int centerY, int radius)
    {
        MIL_INT graphicLabel = -1;

        try
        {
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

            MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
            MIL.MgraArc(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, centerX, centerY, radius, radius, 0, 360);
            MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);

            // 可以看到
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);

            // 可編輯 要打開能選取. resize
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_EDITABLE, MIL.M_ENABLE);

            // 可以選取
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);

            // 可以改變尺寸 固定圓心
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_FIXED_ASPECT_RATIO);

            // 不能畫弳度
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SPECIFIC_FEATURES_EDITABLE, MIL.M_DISABLE);

            // 不可旋轉
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
        }
        catch
        {

        }

        return graphicLabel;
    }
    #endregion

    #region --- DrawCircle ---
    public static MIL_INT DrawRect(int x, int y, int w,int h)
    {
        MIL_INT graphicLabel = -1;

        try
        {
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

            MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);

            //MIL.MgraArc(MilCtrl.MilGraphicsContext, MilCtrl.MilGraphicsList, 300, 300, 300, 300, 0, 360);
            MIL.MgraRectAngle(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, x, y, w, h, 0, MIL.M_DEFAULT);

            MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);

            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_EDITABLE, MIL.M_ENABLE);
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_NO_CONSTRAINT);


            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
            //MIL.MgraControlList(MilCtrl.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
      }
        catch
        {

        }

        return graphicLabel;
    }
    #endregion

    #region --- DrawAutoRoiResult ---
    public static void DrawAutoRoiResult(int[] arrRoi)
    {
        MIL_INT graphicLabel = -1;

        //方框
        MIL_INT startX = arrRoi[0] - 1;
        MIL_INT endX = arrRoi[1];
        MIL_INT startY = arrRoi[2] - 1;
        MIL_INT endY = arrRoi[3];

        MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR888(100, 220, 240));
        MIL.MgraRect(MilGraphicsContext, MilGraphicsList, startX, startY, endX, endY);
        MIL.MgraInquireList(MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
        MIL.MgraControlList(MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
    }
    #endregion

    #region --- SetText ---
    public static void SetText(double x, double y, string msg)
    {
        MIL.MgraColor(MilGraphicsContext, MIL.M_COLOR_GREEN);
        MIL.MgraText(MilGraphicsContext, MilGraphicsList, x, y, msg);
    }
    #endregion     

    #region --- SetScrollBar ---
    public static void SetScrollBar(int H, int V)
    {
        MIL.MdispPan(MyMil.MilMainDisplay, H, V);
    }
    #endregion

    #region --- ChangeZoom ---
    public static void ChangeZoom(double Scale)
    {
        MIL.MdispZoom(MyMil.MilMainDisplay, Scale, Scale);
    }
    #endregion

    #endregion

}
#endregion