using MaterialSkin.Controls;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using CommonBase.Logger;
using BaseTool;
using FrameGrabber;
using CommonSettings;

namespace OpticalMeasuringSystem
{
    public partial class frmParamSetting : MaterialForm
    {
        //private frmMain mainForm = null;
        private cls29MSetting param29M = new cls29MSetting();

        public InfoManager infoManager;
        private MilDigitizer Grabber;

        private MIL_ID SourceImg;

        private int imageWidth;
        private int imageHeight;
        private double Zoom;
        private string ConfigFolder;
        private string ConfigPath_29M;

        //private Point ClickPos;
        private MIL_INT roiGraphicLabel = -1;
        private Enum_ImageType ImageType = Enum_ImageType.Capture;

        public enum Enum_ImageType
        {
            None,
            Capture,
            LoadImage,
        }

        public frmParamSetting(string RecipeName, InfoManager infoManager, ref MilDigitizer Grabber)
        {
            InitializeComponent();
            ConfigFolder = GlobalVar.Config.ConfigPath + @"\" + RecipeName;
            ConfigPath_29M = ConfigFolder + @"\param_29M.xml";
            this.infoManager = infoManager;
            this.Grabber = Grabber;
        }

        private void frmParamSetting_Load(object sender, EventArgs e)
        {
            tableLayoutPanel3.Visible = !GlobalConfig.IsSecretHidden;
            try
            {
                Grabber.SetExposureTime(GlobalConfig.UserKeyinExpTime);
                double ExpTime = Grabber.GetExposureTime();
                mtbxExposureTime.Text = $"{ExpTime}";

                double Gain = Grabber.GetGain();
                mtbxGain.Text = $"{Gain}";
            }
            catch (Exception)
            {

            }

            try
            {
                if (File.Exists(ConfigPath_29M))
                {
                    param29M = param29M.Read(ConfigPath_29M);
                }
                else
                {
                    param29M.Create(param29M, ConfigPath_29M);
                }

                propertyGrid_29M.SelectedObject = param29M;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnLoadImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "請選擇圖片";
            dialog.Filter = "(*.tif;*.tiff;*.bmp;)|*.tif;*.tiff;*.bmp)";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;
                LoadImage(file);
            }
        }

        private delegate void LoadImageUICB(string ImgPath);

        private void LoadImage(string ImgPath)
        {
            if (PnlGrabViewer.InvokeRequired)
            {
                var d = new LoadImageUICB(LoadImage);
                PnlGrabViewer.Invoke(d, new object[] { ImgPath });
            }
            else
            {
                //MilCtrl.AllocBufColor(ref SourceImg, ImgPath, 8);


                MyMilIp.AllocBufColor(ref SourceImg, ImgPath, 16);


                // MIL.MimArith(SourceImg,16, SourceImg,MIL.M_DIV_CONST);
                MyMilDisplay.SetMainDisplayFit(SourceImg, PnlGrabViewer.Handle);


                Bitmap TmpBmp = new Bitmap(ImgPath);
                imageHeight = TmpBmp.Height;
                imageWidth = TmpBmp.Width;

                Zoom = 1;
                MyMilDisplay.ChangeZoom(Zoom);
                SetScrollBar(Zoom);
                CCDScrollBarV.Value = 0;
                CCDScrollBarH.Value = 0;
                MyMilDisplay.SetScrollBar(CCDScrollBarH.Value, CCDScrollBarV.Value);

                ImageType = Enum_ImageType.LoadImage;
            }
        }

        private delegate void GrabCaptureUICB();

        private void GrabCapture()
        {
            if (ImageType == Enum_ImageType.LoadImage)
            {
                if (SourceImg != MIL.M_NULL)
                {
                    MIL.MbufFree(SourceImg);
                }
            }

            this.Grabber.Grab();

            this.SourceImg = this.Grabber.grabImage;

            MyMilDisplay.SetMainDisplayFit(SourceImg, PnlGrabViewer.Handle);

            Zoom = 1;
            MyMilDisplay.ChangeZoom(Zoom);
            SetScrollBar(Zoom);
            CCDScrollBarV.Value = 0;
            CCDScrollBarH.Value = 0;
            MyMilDisplay.SetScrollBar(CCDScrollBarH.Value, CCDScrollBarV.Value);

            ImageType = Enum_ImageType.Capture;
        }

        private void CCDScrollBarV_ValueChanged(object sender, EventArgs e)
        {
            MyMilDisplay.SetScrollBar(CCDScrollBarH.Value, CCDScrollBarV.Value);
        }

        private void CCDScrollBarH_ValueChanged(object sender, EventArgs e)
        {
            MyMilDisplay.SetScrollBar(CCDScrollBarH.Value, CCDScrollBarV.Value);
        }

        private void PnlGrabViewer_MouseDown(object sender, MouseEventArgs e)
        {
            //try
            //{
            //    if (e.Button == MouseButtons.Right)
            //    {
            //        Point start = e.Location;
            //        ClickPos = new Point(
            //        Math.Min(Convert.ToInt32(start.X / Zoom + CCDScrollBarH.Value), Convert.ToInt32(start.X / Zoom + CCDScrollBarH.Value)),
            //        Math.Min(Convert.ToInt32(start.Y / Zoom + CCDScrollBarV.Value), Convert.ToInt32(start.Y / Zoom + CCDScrollBarV.Value)));
            //    }
            //}
            //catch (Exception ex)
            //{

            //    MessageBox.Show(ex.ToString());
            //}
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                param29M.Create(param29M, ConfigPath_29M);
                LoadRecipe(GlobalVar.Config.RecipeName);

                MessageBox.Show("Parameter saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static bool LoadRecipe(string RecipeName)
        {
            try
            {
                GlobalVar.OmsParam = GlobalVar.OmsParam.Read(GlobalVar.Config.ConfigPath + @"\" + RecipeName + @"\param_29M.xml");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetScrollBar(double Scale)
        {
            int BarValueV = 0;
            int BarValueH = 0;


            BarValueV = Convert.ToInt32(imageHeight) - Convert.ToInt32(PnlGrabViewer.Height / Zoom) + CCDScrollBarV.LargeChange - 1;
            BarValueH = Convert.ToInt32(imageWidth) - Convert.ToInt32(PnlGrabViewer.Width / Zoom) + CCDScrollBarH.LargeChange - 1; ;

            if (BarValueV <= 0) BarValueV = 0;
            if (BarValueH <= 0) BarValueH = 0;

            CCDScrollBarV.Maximum = BarValueV;
            CCDScrollBarH.Maximum = BarValueH;
        }

        private void frmParamSetting_FormClosed(object sender, FormClosedEventArgs e)
        {
            MIL.MgraControlList(MyMilDisplay.MilGraphicsList,
                MIL.M_ALL,
                MIL.M_DEFAULT,
                MIL.M_DELETE,
                MIL.M_DEFAULT);

            //if (SourceImg != MIL.M_NULL)
            //{
            //    MIL.MbufFree(SourceImg);
            //}
        }

        private void toolStripMenuSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Wordfile (*.tif;*.bmp;)|*.tif;*.bmp)";
            saveFileDialog.DefaultExt = ".tif";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                MIL.MbufSave(fileName, SourceImg);
            }
        }

        private void btnRoiShow_AutoExpTime_Click(object sender, EventArgs e)
        {
            param29M = param29M.Read(ConfigPath_29M);

            int x = param29M.AutoExposureRoiX;
            int y = param29M.AutoExposureRoiY;
            int w = param29M.AutoExposureRoiWidth;
            int h = param29M.AutoExposureRoiHeight;

            if (w <= 0 || h <= 0)
            {
                MessageBox.Show("No Roi");
            }

            AddROI(x, y, w, h);
        }

        private void btn_RoiSave_AutoExpTime_Click(object sender, EventArgs e)
        {
            try
            {
                if (roiGraphicLabel == -1) return;

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posX);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posY);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_HEIGHT + MIL.M_TYPE_MIL_DOUBLE, ref height);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_WIDTH + MIL.M_TYPE_MIL_DOUBLE, ref width);

                param29M.AutoExposureRoiX = Convert.ToInt32(posX);
                param29M.AutoExposureRoiY = Convert.ToInt32(posY);
                param29M.AutoExposureRoiWidth = Convert.ToInt32(width);
                param29M.AutoExposureRoiHeight = Convert.ToInt32(height);
                BtnSaveConfig_Click(null, null);

                param29M = param29M.Read(ConfigPath_29M);

                propertyGrid_29M.SelectedObject = param29M;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        double posX = 0;
        double posY = 0;
        double width = 0;
        double height = 0;

        private void AddROI(int x = 300, int y = 300, int w = 0, int h = 0)
        {
            if (w <= 0 || h <= 0)
            {
                w = 300;
                h = 300;
            }

            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                if (roiGraphicLabel != -1)
                {
                    MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
                }

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
                //MIL.MgraArc(MilCtrl.MilGraphicsContext, MilCtrl.MilGraphicsList, 300, 300, 300, 300, 0, 360);
                MIL.MgraRectAngle(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, x, y, w, h, 0, MIL.M_DEFAULT);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref roiGraphicLabel);

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_NO_CONSTRAINT);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_EDITABLE, MIL.M_ENABLE);
                //MIL.MgraControlList(MilCtrl.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);
            }
        }

        private void btnRoiShow_ThirdParty_Click(object sender, EventArgs e)
        {
            param29M = param29M.Read(ConfigPath_29M);

            int x = param29M.ThirdPartyRoiX;
            int y = param29M.ThirdPartyRoiY;
            int w = param29M.ThirdPartyRoiWidth;
            int h = param29M.ThirdPartyRoiHeight;
                     
            AddROI(x, y, w, h);
        }

        private void btnRoiSave_ThirdParty_Click(object sender, EventArgs e)
        {
            try
            {
                if (roiGraphicLabel == -1) return;

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posX);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posY);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_HEIGHT + MIL.M_TYPE_MIL_DOUBLE, ref height);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(roiGraphicLabel), MIL.M_DEFAULT, MIL.M_RECTANGLE_WIDTH + MIL.M_TYPE_MIL_DOUBLE, ref width);

                param29M.ThirdPartyRoiX = Convert.ToInt32(posX);
                param29M.ThirdPartyRoiY = Convert.ToInt32(posY);
                param29M.ThirdPartyRoiWidth = Convert.ToInt32(width);
                param29M.ThirdPartyRoiHeight = Convert.ToInt32(height);
                BtnSaveConfig_Click(null, null);

                param29M = param29M.Read(ConfigPath_29M);

                propertyGrid_29M.SelectedObject = param29M;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void mbtnSetExposureTime_Click(object sender, EventArgs e)
        {
            try
            {
                double ExpTime = Convert.ToDouble(mtbxExposureTime.Text);
                this.Grabber.SetExposureTime(ExpTime);

                GlobalConfig.UserKeyinExpTime = ExpTime;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mbtnSetGain_Click(object sender, EventArgs e)
        {
            try
            {
                int gain = Convert.ToInt32(mtbxGain.Text);

                if (gain <= 1)
                {
                    gain = 1;
                    mtbxGain.Text = "1";
                }

                if (gain > 32)
                {
                    gain = 32;
                    mtbxGain.Text = "32";
                }
                this.Grabber.SetCameraGain(gain);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            MIL_ID currentImage = MIL.M_NULL;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Wordfile (*.tif;*.bmp;)|*.tif;*.bmp)";
            SFD.DefaultExt = ".tif";

            if (SFD.ShowDialog() == DialogResult.OK)
            {
                string fileName = SFD.FileName;
                //if (ImgType == 0)
                //{
                //    MIL.MbufExport(fileName, MIL.M_TIFF, GrabImg);

                //}
                //else
                //{
                //    MIL.MbufSave(fileName, TifImg);

                //}
                MIL.MbufSave(fileName, SourceImg);
            }
        }

        private void mbtnCapture_Click(object sender, EventArgs e)
        {
            GrabCapture();
        }
    }
}
