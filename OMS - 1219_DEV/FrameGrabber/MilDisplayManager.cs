using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameGrabber
{
    public class MilDisplayManager
    {
        public event Action<int, int, int> UpdateMousePara;

        public event Action<object> GraphicSelectValueChangedEvent;
        public event Action<int> GraphicDeleteEvent;

        private MIL_ID MilSystem = MIL.M_NULL;

        public MIL_ID MilDisplay = MIL.M_NULL;

        public MIL_ID ROI_MilGraphicsList = MIL.M_NULL;
        private MIL_ID ROI_MilGraphicsContext = MIL.M_NULL;

        public MIL_ID Show_MilGraphicsList = MIL.M_NULL;
        private MIL_ID Show_MilGraphicsContext = MIL.M_NULL;

        private Panel Viewer;
        public MIL_ID MilImage = MIL.M_NULL;


        private MIL_DISP_HOOK_FUNCTION_PTR mouseLeftDownEventDelegate;
        private MIL_DISP_HOOK_FUNCTION_PTR mouseMoveEventDelegate;
        private MIL_DISP_HOOK_FUNCTION_PTR mouseWheelEventDelegate;
        private MIL_DISP_HOOK_FUNCTION_PTR keyUpEventDelegate;

        public double orgZoom = 0.0;
        public double oldZoom = 0.0;

        private double MousePosX;
        private double MousePosY;
        private int[] MouseGrayValue = new int[1] { 0 };

        public MilDisplayManager(ref MIL_ID MilSystem)
        {
            this.MilSystem = MilSystem;
        }

        public void Init()
        {
            MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref MilDisplay);
            MIL.MgraAllocList(MilSystem, MIL.M_DEFAULT, ref ROI_MilGraphicsList);
            MIL.MgraAllocList(MilSystem, MIL.M_DEFAULT, ref Show_MilGraphicsList);

            
            MIL.MgraAlloc(MilSystem, ref ROI_MilGraphicsContext);
            MIL.MgraAlloc(MilSystem, ref Show_MilGraphicsContext);


            // Display mouse event
            mouseLeftDownEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseLeftDownEvent);
            MIL.MdispHookFunction(MilDisplay, MIL.M_MOUSE_LEFT_BUTTON_DOWN, mouseLeftDownEventDelegate, IntPtr.Zero);
            mouseWheelEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseWheelEvent);
            MIL.MdispHookFunction(MilDisplay, MIL.M_MOUSE_WHEEL, mouseWheelEventDelegate, IntPtr.Zero);
            mouseMoveEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayMouseMoveEvent);
            MIL.MdispHookFunction(MilDisplay, MIL.M_MOUSE_MOVE, mouseMoveEventDelegate, IntPtr.Zero);
            keyUpEventDelegate = new MIL_DISP_HOOK_FUNCTION_PTR(DisplayKeyUpEvent);
            MIL.MdispHookFunction(MilDisplay, MIL.M_KEY_UP, keyUpEventDelegate, IntPtr.Zero);

            MIL.MdispControl(MilDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT);
            MIL.MdispControl(MilDisplay, MIL.M_MOUSE_USE, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_KEYBOARD_USE, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_GRAPHIC_LIST_INTERACTIVE, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, ROI_MilGraphicsList);

            MIL.MdispControl(MilDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(MilDisplay, MIL.M_VIEW_MODE, MIL.M_AUTO_SCALE);
        }

        #region --- SetWindow ---

        public void SetWindow(ref MIL_ID Img, ref Panel Pnl)
        {
            if (Img == this.MilImage) return;

            this.Viewer = Pnl;
            this.MilImage = Img;
            MIL.MdispSelectWindow(MilDisplay, this.MilImage, this.Viewer.Handle);
        }
        #endregion

        #region --- SetGrid ---
        public void SetGrid(double ratio)
        {
            MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, Show_MilGraphicsList);

            MIL.MgraClear(MIL.M_DEFAULT, Show_MilGraphicsList);

            MIL_INT graphicLabel = -1;

            double curSizeX = MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
            double curSizeY = MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

            //十字
            MIL.MgraColor(Show_MilGraphicsContext, MIL.M_COLOR_YELLOW);
            MIL.MgraLine(Show_MilGraphicsContext, Show_MilGraphicsList, curSizeX * 0.5, 1, curSizeX * 0.5, curSizeY - 1);
            MIL.MgraInquireList(Show_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);

            MIL.MgraLine(Show_MilGraphicsContext, Show_MilGraphicsList, 1, curSizeY * 0.5, curSizeX - 1, curSizeY * 0.5);
            MIL.MgraInquireList(Show_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);

            //方框
            MIL_INT startX = (MIL_INT)(curSizeX * (0.5 - ratio / 2.0));
            MIL_INT startY = (MIL_INT)(curSizeY * (0.5 - ratio / 2.0));
            MIL_INT endX = (MIL_INT)(curSizeX - curSizeX * (0.5 - ratio / 2.0));
            MIL_INT endY = (MIL_INT)(curSizeY - curSizeY * (0.5 - ratio / 2.0));

            MIL.MgraColor(Show_MilGraphicsContext, MIL.M_COLOR_YELLOW);
            MIL.MgraRect(Show_MilGraphicsContext, Show_MilGraphicsList, startX, startY, endX, endY);
            MIL.MgraInquireList(Show_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
            MIL.MgraControlList(Show_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
        }
        #endregion

        #region --- ClearGrid ---
        public  void ClearGrid()
        {
            MIL.MgraClear(MIL.M_DEFAULT, Show_MilGraphicsList);
            MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, ROI_MilGraphicsList);
        }
        #endregion

        #region Mil Event

        #region --- DisplayMouseWheelEvent ---
        private MIL_INT DisplayMouseWheelEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
        {
            try
            {
                double curZoom = 0;
                double ZoomFit = 0.0;
                int DisplayWidth = 0;
                int DisplayHeight = 0;

                // 沒影像就return
                if (MilDisplay == MIL.M_NULL) return 0;

                if (Viewer != null && MilImage != MIL.M_NULL)
                {
                    MIL_INT SizeX = 0;
                    MIL_INT SizeY = 0;
                    DisplayWidth = Viewer.Size.Width;
                    DisplayHeight = Viewer.Size.Height;

                    SizeX = MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
                    SizeY = MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

                    if (SizeX > 0 && SizeY > 0)
                    {
                        ZoomFit = Math.Min((double)DisplayWidth / (double)SizeX, (double)DisplayHeight / (double)SizeY);
                        //MIL.MdispZoom(MyMil.MilMainDisplay, oldZoom, oldZoom);
                    }
                }

                // 不能小於最適尺寸
                MIL.MdispInquire(MilDisplay, MIL.M_ZOOM_FACTOR_X, ref curZoom);

                if (oldZoom == 0) oldZoom = curZoom;
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

                MIL.MdispZoom(MilDisplay, oldZoom, oldZoom);
            }
            catch 
            {

            }
            return 0;
        }
        #endregion

        #region --- DisplayMouseMoveEvent ---
        private MIL_INT DisplayMouseMoveEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
        {
            try
            {
                if (MilDisplay == MIL.M_NULL) return 0;
                if (MilImage == MIL.M_NULL) return 0;

                MIL.MdispGetHookInfo(eventId, MIL.M_MOUSE_POSITION_BUFFER_X, ref MousePosX);
                MIL.MdispGetHookInfo(eventId, MIL.M_MOUSE_POSITION_BUFFER_Y, ref MousePosY);

                MIL_INT SX = MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT SY = MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

                MousePosX = Math.Floor(MousePosX + 0.5);
                MousePosY = Math.Floor(MousePosY + 0.5);
                if (MousePosX >= 0 && MousePosX <= SX - 1 && MousePosY >= 0 && MousePosY <= SY - 1)
                {
                    MIL.MbufGet2d(MilImage, (MIL_INT)MousePosX, (MIL_INT)MousePosY, 1, 1, MouseGrayValue);
                }
                else
                {
                    MousePosX = -1;
                    MousePosY = -1;
                }

                UpdateMousePara?.Invoke((int)MousePosX, (int)MousePosY, MouseGrayValue[0]);
            }
            catch 
            {

            }
            return 0;
        }
        #endregion

        #region --- DisplayMouseLeftDownEvent ---
        private MIL_INT DisplayMouseLeftDownEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
        {
            try
            {
                MIL_INT listCount = 0;
                MIL_INT selectLabel = 0;

                MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref listCount);
                for (MIL_INT i = 0; i < listCount; i++)
                {
                    // Get Selected
                    if (MIL.MgraInquireList(ROI_MilGraphicsList,
                        MIL.M_GRAPHIC_INDEX(i),
                        MIL.M_DEFAULT,
                        MIL.M_GRAPHIC_SELECTED,
                        MIL.M_NULL) == MIL.M_TRUE)
                    {
                        MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE, ref selectLabel);
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
        private MIL_INT DisplayKeyUpEvent(MIL_INT hookType, MIL_ID eventId, IntPtr userObjectPtr)
        {
            MIL_INT keyValue = 0;
            if (MilDisplay == MIL.M_NULL || MilImage == MIL.M_NULL) return 0;
            MIL.MdispGetHookInfo(eventId, MIL.M_KEY_VALUE, ref keyValue);

            if (keyValue == MIL.M_KEY_DELETE)
            {
                MIL_INT listCount = 0;
                MIL_INT selectLabel = 0;

                MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref listCount);
                for (MIL_INT i = 0; i < listCount; i++)
                {
                    // Get Selected
                    if (MIL.MgraInquireList(ROI_MilGraphicsList,
                        MIL.M_GRAPHIC_INDEX(i),
                        MIL.M_DEFAULT,
                        MIL.M_GRAPHIC_SELECTED,
                        MIL.M_NULL) == MIL.M_TRUE)
                    {
                        MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE, ref selectLabel);
                        MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
                        GraphicDeleteEvent?.Invoke((int)selectLabel);
                        break;
                    }
                }
            }
            return 0;
        }
        #endregion

        #endregion

        #region ROI

        public bool AddROI()
        {
            try
            {
                if (MilImage != MIL.M_NULL)
                {
                    MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, ROI_MilGraphicsList);

                    MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                    MIL_INT GraphicLabel = -1;

                    MIL.MgraColor(ROI_MilGraphicsContext, MIL.M_COLOR_RED);

                    MIL.MgraRect(ROI_MilGraphicsContext, ROI_MilGraphicsList, 100, 100, 500, 500);

                    MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref GraphicLabel);

                    MIL_INT ListCount = 0;

                    MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref ListCount);

                    MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                    MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                    MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteROI()
        {
            try
            {
                MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, ROI_MilGraphicsList);

                MIL_INT numberOfPrimitives = 0;
                MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref numberOfPrimitives);

                MIL_INT selectFlag = 0;
                for (MIL_INT i = 0; i < numberOfPrimitives; i++)
                {
                    // Get Selected
                    MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i),
                        MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, ref selectFlag);

                    if (selectFlag == (MIL_INT)1)
                    {
                        MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_INDEX(i),
                            MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
                        break;
                    }
                }
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public void DrawROI(int[] arrRoi)
        {
            MIL_INT graphicLabel = -1;

            //方框
            MIL_INT startX = arrRoi[0] - 1;
            MIL_INT endX = arrRoi[1];
            MIL_INT startY = arrRoi[2] - 1;
            MIL_INT endY = arrRoi[3];

            MIL.MgraColor(ROI_MilGraphicsContext, MIL.M_COLOR888(100, 220, 240));
            MIL.MgraRect(ROI_MilGraphicsContext, ROI_MilGraphicsList, startX, startY, endX, endY);
            MIL.MgraInquireList(ROI_MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);
            MIL.MgraControlList(ROI_MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_DISABLE);
        }

        #endregion

        #region --- DrawBox ---
        public void DrawBox(System.Drawing.Point[] TargetPoints, System.Drawing.Point[] SourcePoints)
        {
            MIL.MgraClear(MIL.M_DEFAULT, Show_MilGraphicsList);

            MIL.MdispControl(MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, Show_MilGraphicsList);

            int Counts = TargetPoints.Length;
            if (Counts > 1)
            {
                double[] XStart = new double[Counts];
                double[] YStart = new double[Counts];
                double[] XEnd = new double[Counts];
                double[] YEnd = new double[Counts];

                try
                {
                    MIL.MgraClear(MIL.M_DEFAULT, Show_MilGraphicsList);

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

                    MIL.MgraColor(Show_MilGraphicsContext, MIL.M_COLOR_RED);
                    MIL.MgraLines(Show_MilGraphicsContext, Show_MilGraphicsList, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);

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

                    MIL.MgraColor(Show_MilGraphicsContext, MIL.M_COLOR_YELLOW);
                    MIL.MgraLines(Show_MilGraphicsContext, Show_MilGraphicsList, 4, XStart, YStart, XEnd, YEnd, MIL.M_DEFAULT);

                    MIL.MgraColor(Show_MilGraphicsContext, MIL.M_COLOR_GREEN);
                    //MIL.MgraRectFill(MilGraphicsContext, MilGraphicsList, SourcePoints[3].X, SourcePoints[3].Y, SourcePoints[3].X + 1, SourcePoints[3].Y + 1);

                    MIL.MgraRectFill(Show_MilGraphicsContext, Show_MilGraphicsList, 595, 5990, 596, 5990 + 1);
                    MIL.MgraRectFill(Show_MilGraphicsContext, Show_MilGraphicsList, 0, 0, 1, 1);


                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

        }

        #endregion


    }
}
