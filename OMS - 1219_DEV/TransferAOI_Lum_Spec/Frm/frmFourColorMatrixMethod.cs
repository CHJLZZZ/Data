using LightMeasure;
using MaterialSkin;
using MaterialSkin.Controls;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MyMil;

namespace OpticalMeasuringSystem
{
    public partial class frmFourColorMatrixMethod : MaterialForm
    {
        public frmFourColorMatrixMethod()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        public LightMeasureConfig rConfig;

        private void frmUserCalibration_Load(object sender, EventArgs e)
        {
            this.TopMost = true;

            LoadImage();
            MyMilIp.ul = new List<MyMilIp.MeasureUnit>();

            Dgv_Roi.RowHeadersVisible = false;
            Dgv_Roi.AllowUserToAddRows = true;

            string RConfigConfig = @"D:\OpticalMeasurementData\4Color\R.xml";
            if (GlobalVar.ProcessInfo.Pattern != "R" && File.Exists(RConfigConfig))
            {
                rConfig = new LightMeasureConfig();
                rConfig.ReadWithoutCrypto(@"D:\OpticalMeasurementData\4Color\R.xml");

                foreach (FourColorData data in rConfig.CorrectionData.DataList)
                {
                    AddROI(data.CircleRegionInfo.CenterX,
                           data.CircleRegionInfo.CenterY,
                           data.CircleRegionInfo.Radius);
                }

                tbxMatrixRow.Text = rConfig.CorrectionData.MeasureMatrixInfo.Column.ToString();
            }
            Update_DgvROI();
        }

        private void MilCtrl_GraphicSelectValueChangedEvent(int obj)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<int>(MilCtrl_GraphicSelectValueChangedEvent), obj);
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
            }
            else
            {
                foreach (DataGridViewRow row in Dgv_Roi.Rows)
                {
                    row.Selected = false;
                }

                Dgv_Roi.Rows[obj - 4].Selected = true;
            }
        }

        private delegate void LoadUCImage();

        private void LoadImage()
        {
            if (PnlUCViewer.InvokeRequired)
            {
                PnlUCViewer.Invoke(new Action(LoadImage), new object[] { });
            }
            else
            {
                MyMilDisplay.SetMainDisplayFit(MyMilDisplay.MilMainDisplayImage[1], PnlUCViewer.Handle);
                PnlUCViewer.Refresh();
            }
        }

        private void Btn_AddRoi_Click(object sender, EventArgs e)
        {
            AddROI();
            Update_DgvROI();
        }

        private void AddROI()
        {
            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                MIL_INT GraphicLabel = -1;

                MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
                MIL.MgraArc(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, 300, 300, 300, 300, 0, 360);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref GraphicLabel);

                MIL_INT ListCount = 0;

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref ListCount);

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_SQUARE_ASPECT_RATIO);
                //MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_EDITABLE, MIL.M_ENABLE);
                //MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);

                double centerX = 0;
                double centerY = 0;
                double radius = 0;

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_CENTER_X, ref centerX);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_CENTER_Y, ref centerY);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_RADIUS_X, ref radius);

                MyMilIp.MeasureUnit unit = new MyMilIp.MeasureUnit();
                unit.RoiName = $"Circle_{ListCount}";
                unit.RoiLableID = GraphicLabel;
                unit.CenterX = (int)centerX;
                unit.CenterY = (int)centerY;
                unit.Radius = (int)radius;
                MyMilIp.ul.Add(unit);
            }
        }

        private void AddROI(int centerX, int centerY, int radius)
        {
            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                MIL_INT GraphicLabel = -1;

                MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
                MIL.MgraArc(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, centerX, centerY, radius, radius, 0, 360);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref GraphicLabel);

                MIL_INT ListCount = 0;

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref ListCount);

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_SQUARE_ASPECT_RATIO);
                //MIL.MgraControlList(MilCtrl.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_EDITABLE, MIL.M_ENABLE);
                //MIL.MgraControlList(MilCtrl.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(GraphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);

                MyMilIp.MeasureUnit unit = new MyMilIp.MeasureUnit();
                unit.RoiName = $"Circle_{GraphicLabel}";
                unit.RoiLableID = GraphicLabel;
                unit.CenterX = (int)centerX;
                unit.CenterY = (int)centerY;
                unit.Radius = (int)radius;
                MyMilIp.ul.Add(unit);
            }
        }

        private void DelROI(string roiName)
        {
            try
            {
                string[] s = roiName.Split('_');
                int id = Convert.ToInt32(s[1]);
                MyMilIp.ul.RemoveAll(t => t.RoiLableID == (MIL_INT)(id + 4));   //畫十字的時候用了4個
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL((MIL_INT)(id + 4)), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
            }
            catch
            {

            }
        }

        private void Update_DgvROI()
        {
            Dgv_Roi.Rows.Clear();

            foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
            {
                DataGridViewRow row = (DataGridViewRow)Dgv_Roi.Rows[0].Clone();
                row.Cells[0].Value = $"Circle_{mu.RoiLableID - 4}"; //畫十字的時候用了4個
                row.Cells[1].Value = mu.CenterX.ToString();
                row.Cells[2].Value = mu.CenterY.ToString();
                row.Cells[3].Value = mu.Radius.ToString();

                Dgv_Roi.Rows.Add(row);
            }
        }
        
        private void btnSaveFourColorCalibration_Click(object sender, EventArgs e)
        {
            int row;
            int column;
            MIL_INT sx = 0;
            MIL_INT sy = 0;

            try
            {
                if (tbxMatrixRow.Text.Length <= 0 || tbxMatrixColumn.Text.Length <= 0)
                {
                    MessageBox.Show("請輸入矩陣行、列大小。");
                    return;
                }

                MyMilIp.correctionConfig = new LightMeasureConfig();
                MyMilIp.correctionConfig.Name = $"{GlobalVar.ProcessInfo.Pattern}";

                sx = MIL.MbufInquire(MyMilDisplay.MilMainDisplayImage[1], MIL.M_SIZE_X, MIL.M_NULL);
                sy = MIL.MbufInquire(MyMilDisplay.MilMainDisplayImage[1], MIL.M_SIZE_Y, MIL.M_NULL);
                MyMilIp.correctionConfig.CorrectionData.ImageInfo.Width = (int)sx;
                MyMilIp.correctionConfig.CorrectionData.ImageInfo.Height = (int)sy;

                row = Convert.ToInt32(tbxMatrixRow.Text);
                column = Convert.ToInt32(tbxMatrixColumn.Text);
                if (row * column < 4 && (row != 1 && column != 1))
                {
                    MessageBox.Show("不支援的矩陣尺寸。\n");
                    return;
                }

                MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Row = row;
                MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Column = column;

                if (GlobalVar.ProcessInfo.XyzExposureTime[0] > 0)
                {
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.X = GlobalVar.ProcessInfo.XyzExposureTime[0];
                }

                if (GlobalVar.ProcessInfo.XyzExposureTime[1] > 0)
                {
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.Y = GlobalVar.ProcessInfo.XyzExposureTime[1];
                }

                if (GlobalVar.ProcessInfo.XyzExposureTime[2] > 0)
                {
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.Z = GlobalVar.ProcessInfo.XyzExposureTime[2];
                }


                foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
                {
                    FourColorData dt = new FourColorData();
                    dt.Index = MyMilIp.correctionConfig.CorrectionData.DataList.Count + 1;
                    dt.CircleRegionInfo.CenterX = mu.CenterX;
                    dt.CircleRegionInfo.CenterY = mu.CenterY;
                    dt.CircleRegionInfo.Radius = mu.Radius;

                    dt.CieChroma.Luminance = mu.Luminance;
                    dt.CieChroma.Cx = mu.Cx;
                    dt.CieChroma.Cy = mu.Cy;

                    MyMilIp.correctionConfig.CorrectionData.DataList.Add(dt);

                }

                if (File.Exists($"D:\\OpticalMeasurementData\\4Color\\{GlobalVar.ProcessInfo.Pattern}.xml"))
                {
                    File.Delete($"D:\\OpticalMeasurementData\\4Color\\{GlobalVar.ProcessInfo.Pattern}.xml");
                }

                //MilCtrl.correctionConfig.CorrectionData.                
                MyMilIp.correctionConfig.WriteWithoutCrypto($"D:\\OpticalMeasurementData\\4Color\\{GlobalVar.ProcessInfo.Pattern}.xml", false);

                MyMilIp.IsUserCalibrationFinish = true;
                Close();
            }
            catch
            {
            
            }
        }

        private void Dgv_Roi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (curDgvPosRow >= 0)
                {
                    string n = Dgv_Roi.Rows[curDgvPosRow].Cells[0].Value.ToString();
                    DelROI(n);
                }
                Update_DgvROI();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    int curROI_ID = Convert.ToInt32(Dgv_Roi.Rows[curDgvPosRow].Cells[0].Value.ToString().Split('_')[1]);
                    string CellValue = Dgv_Roi.Rows[curDgvPosRow].Cells[curDgvPosCol].Value.ToString();
                    int index = MyMilIp.ul.FindIndex(x => x.RoiLableID == curROI_ID + 4);

                    switch (curDgvPosCol)
                    {
                        case 4:
                            MyMilIp.ul[index].Luminance = Convert.ToDouble(CellValue);
                            break;
                        case 5:
                            MyMilIp.ul[index].Cx = Convert.ToDouble(CellValue);
                            break;
                        case 6:
                            MyMilIp.ul[index].Cy = Convert.ToDouble(CellValue);
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        public int curDgvPosRow = -1;
        public int curDgvPosCol = -1;

        private void Dgv_Roi_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < MyMilIp.ul.Count)
            {
                curDgvPosCol = e.ColumnIndex;
                curDgvPosRow = e.RowIndex;

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                //MIL_INT SelectLabelID = ((KeyValuePair<MIL_INT, string>)lsbRegionList.SelectedItem).Key;
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(curDgvPosRow + 4), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);
            }
        }

        private void PnlUCViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < MyMilIp.ul.Count; i++)
                {
                    double centerX = 0;
                    double centerY = 0;
                    double radius = 0;

                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(i + 4), MIL.M_DEFAULT, MIL.M_CENTER_X, ref centerX);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(i + 4), MIL.M_DEFAULT, MIL.M_CENTER_Y, ref centerY);
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(i + 4), MIL.M_DEFAULT, MIL.M_RADIUS_X, ref radius);

                    int index = MyMilIp.ul.FindIndex(s => s.RoiLableID == (MIL_INT)(i + 4));

                    if (index != -1)
                    {
                        MyMilIp.ul[index].CenterX = (int)centerX;
                        MyMilIp.ul[index].CenterY = (int)centerY;
                        MyMilIp.ul[index].Radius = (int)radius;
                    }
                }
                Update_DgvROI();
            }
        }

        private void PnlUCViewer_MouseEnter(object sender, EventArgs e)
        {
            PnlUCViewer.Focus();
        }

        private void Dgv_Roi_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int curROI_ID = Convert.ToInt32(Dgv_Roi.Rows[curDgvPosRow].Cells[0].Value.ToString().Split('_')[1]);
                string CellValue = Dgv_Roi.Rows[curDgvPosRow].Cells[curDgvPosCol].Value.ToString();
                int index = MyMilIp.ul.FindIndex(x => x.RoiLableID == curROI_ID + 4);

                switch (curDgvPosCol)
                {
                    case 4:
                        MyMilIp.ul[index].Luminance = Convert.ToDouble(CellValue);
                        break;
                    case 5:
                        MyMilIp.ul[index].Cx = Convert.ToDouble(CellValue);
                        break;
                    case 6:
                        MyMilIp.ul[index].Cy = Convert.ToDouble(CellValue);
                        break;
                }
            }
            catch
            {

            }
        }
    }
}
