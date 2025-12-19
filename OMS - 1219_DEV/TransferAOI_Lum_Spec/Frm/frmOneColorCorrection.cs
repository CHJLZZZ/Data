using BaseTool;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MyMil;

namespace OpticalMeasuringSystem
{
    public partial class frmOneColorCorrection : MaterialForm
    {
        private delegate void loadOneColorCalibrationImage();

        public LightMeasureConfig rConfig;
        private List<string> dataNameList;
        private int curDgvRow = -1;
        private int curDgvCol = -1;
        private int currentRadius = -1;
        private MIL_INT selectedROILabel = -1;
        private int selectedMeasureUnitIndex = -1;
        private bool isNeedToUpdateRadius = false;
        private double GraymeanBypassPercent;

        public frmOneColorCorrection(double graymeanBypassPercent)
        {
            InitializeComponent();
            GraymeanBypassPercent = graymeanBypassPercent;
        }

        #region --- Form Event ---

        #region --- frmOneColorCorrection_Load ---
        private void frmOneColorCorrection_Load(object sender, EventArgs e)
        {
            this.LoadImage();
            MyMilIp.ul = new List<MyMilIp.MeasureUnit>();
            MyMilDisplay.GraphicSelectValueChangedEvent -= GraphicSelectValueChangedEvent;
            MyMilDisplay.GraphicDeleteEvent -= GraphicDeleteEvent;

            MyMilDisplay.GraphicSelectValueChangedEvent += GraphicSelectValueChangedEvent;
            MyMilDisplay.GraphicDeleteEvent += GraphicDeleteEvent;

            this.dgvRoi.RowHeadersVisible = false;
            this.dgvRoi.AllowUserToAddRows = true;

            // Use LINQ to disable Column Sorting
            this.dgvRoi.Columns.Cast<DataGridViewColumn>().ToList().ForEach(f => f.SortMode = DataGridViewColumnSortMode.NotSortable);

            // Display
            MyMilDisplay.SetMainDisplayFit(MyMilDisplay.MilMainDisplayImage[1], this.pnlOneColorCalibrationViewer.Handle);
            this.ImageZoonAll();

            // Calibration File
            this.dataNameList = this.GetAllCalibrationFile();
            this.lbxRecipeList.Items.AddRange(this.dataNameList.ToArray());

            MyMilDisplay.GrabViewer = this.pnlOneColorCalibrationViewer;

            // UI
            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                this.dgvRoi.Columns["Cx"].Visible = false;
                this.dgvRoi.Columns["Cy"].Visible = false;
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                this.dgvRoi.Columns["Cx"].Visible = true;
                this.dgvRoi.Columns["Cy"].Visible = true;
            }

            MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_SQUARE_ASPECT_RATIO);

            AddCIELuminance_mcb.Checked = false;

        }
        #endregion

        #region --- frmOneColorCorrection_FormClosing ---
        private void frmOneColorCorrection_FormClosing(object sender, FormClosingEventArgs e)
        {
            //MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL, MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);
        }
        #endregion

        #endregion

        #region --- 方法函式 ---

        #region --- LoadImage ---
        private void LoadImage()
        {
            if (this.pnlOneColorCalibrationViewer.InvokeRequired)
            {
                this.pnlOneColorCalibrationViewer.Invoke(new Action(LoadImage), new object[] { });
            }
            else
            {
                MyMilDisplay.SetMainDisplayFit(MyMilDisplay.MilMainDisplayImage[1], this.pnlOneColorCalibrationViewer.Handle);
            }
        }
        #endregion

        #region --- SaveClibrationData ---
        public void SaveClibrationData(string fileName = "")
        {
            int row;
            int column;
            int exposureTimeX;
            int exposureTimeY;
            int exposureTimeZ;

            string ErrInfo = "";

            if (this.tbxMatrixRow.Text.Length <= 0 || this.tbxMatrixColumn.Text.Length <= 0)
            {
                MessageBox.Show("請輸入矩陣行、列大小。");
                return;
            }

            MyMilIp.correctionConfig = new LightMeasureConfig();
            MyMilIp.correctionConfig.Name = $"{GlobalVar.ProcessInfo.Pattern}";
            
            MIL_INT SX = MIL.MbufInquire(MyMilDisplay.MilMainDisplayImage[1], MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT SY = MIL.MbufInquire(MyMilDisplay.MilMainDisplayImage[1], MIL.M_SIZE_Y, MIL.M_NULL);
            MyMilIp.correctionConfig.CorrectionData.ImageInfo.Width = (int)SX;
            MyMilIp.correctionConfig.CorrectionData.ImageInfo.Height = (int)SY;

            // Check Matrix
            row = Convert.ToInt32(tbxMatrixRow.Text);
            column = Convert.ToInt32(tbxMatrixColumn.Text);
            if (row * column < 4 && (row != 1 && column != 1))
            {
                MessageBox.Show("不支援的矩陣尺寸。\n");
                return;
            }

            MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Row = row;
            MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Column = column;

            // Check Exposure Time
            exposureTimeX = GlobalVar.ProcessInfo.XyzExposureTime[0];
            exposureTimeY = GlobalVar.ProcessInfo.XyzExposureTime[1];
            exposureTimeZ = GlobalVar.ProcessInfo.XyzExposureTime[2];

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                if (exposureTimeX > 0 && exposureTimeY > 0 && exposureTimeZ > 0)
                {
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.X = exposureTimeX;
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.Y = exposureTimeY;
                    MyMilIp.correctionConfig.CorrectionData.ExposureTime.Z = exposureTimeZ;
                }
                else
                {
                    if (GlobalVar.ProcessInfo.XyzExposureTime[0] <= 0)
                        ErrInfo += "X ";
                    if (GlobalVar.ProcessInfo.XyzExposureTime[1] <= 0)
                        ErrInfo += "Y ";
                    if (GlobalVar.ProcessInfo.XyzExposureTime[2] <= 0)
                        ErrInfo += "Z ";

                    MessageBox.Show(ErrInfo + "曝光時間有誤。");
                    return;
                }
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                if (GlobalVar.SD.XYZFilter_PositionMode != BaseTool.EnumXYZFilter_PositionMode.AIL_2_Positions)
                {
                    if (exposureTimeY > 0)
                    {
                        MyMilIp.correctionConfig.CorrectionData.ExposureTime.Y = exposureTimeY;
                    }
                    else
                    {
                        if (GlobalVar.ProcessInfo.XyzExposureTime[1] <= 0)
                            ErrInfo += "Y ";
                        MessageBox.Show(ErrInfo + "曝光時間有誤。");
                        return;
                    }
                }          
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

            string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string fullFileName = $@"{oneColorCalibrationFolder}\{fileName}.xml";

            MyMilIp.correctionConfig.WriteWithoutCrypto($@"{oneColorCalibrationFolder}\correctionConfig.xml", true);
            MyMilIp.correctionConfig.WriteWithoutCrypto(fullFileName, true);

            MyMilIp.IsUserCoeffFinish = MyMilIp.EnCalibrationState.Successful;
        }
        #endregion

        #region --- AddRoi ---
        private MIL_INT AddRoi(int centerX = 300, int centerY = 300)
        {
            MIL_INT ListCount = -1;
            MIL_INT graphicLabel = -1;
           
            int radius = (this.currentRadius > 0 ? this.currentRadius : 300);

            this.dgvRoi.ClearSelection();

            if (MyMilDisplay.MilMainDisplayImage[1] != MIL.M_NULL)
            {
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
                MIL.MgraArc(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, centerX, centerY, radius, radius, 0, 360);

                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS + MIL.M_TYPE_MIL_INT, ref ListCount);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL + MIL.M_TYPE_MIL_INT, ref graphicLabel);

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                //MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_MODE_RESIZE, MIL.M_FIXED_CENTER + MIL.M_SQUARE_ASPECT_RATIO);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(graphicLabel), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

                this.GraphicSelectValueChangedEvent((object)graphicLabel);

                MyMilIp.MeasureUnit unit = new MyMilIp.MeasureUnit();
                unit.RoiName = $"Circle_{ListCount}";
                unit.RoiLableID = graphicLabel;
                unit.CenterX = centerX;
                unit.CenterY = centerY;
                unit.Radius = radius;
                MyMilIp.ul.Add(unit);
            }

            return graphicLabel;
        }
        #endregion

        #region --- DeleteRoi ---
        private void DeleteRoi(string roiName)
        {
            try
            {
                dgvRoi.ClearSelection();

                MyMilIp.MeasureUnit removeUnit = MyMilIp.ul.Find(t => t.RoiName == roiName);

                if (removeUnit == null) return;

                // remove roi in list
                MyMilIp.ul.Remove(removeUnit);

                // renamed remaining roi
                foreach (MyMilIp.MeasureUnit unit in MyMilIp.ul)
                {
                    string sType = unit.RoiName.Split('_')[0];
                    string sOrder = unit.RoiName.Split('_')[1];
                    int iOrder = int.Parse(sOrder);

                    if (iOrder < int.Parse(removeUnit.RoiName.Split('_')[1])) continue;
                    unit.RoiName = $"{sType}_{iOrder - 1}";
                }

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(removeUnit.RoiLableID), MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);

            }
            catch 
            {

            }
        }
        #endregion

        #region --- UpdateRoi ---
        private void UpdateRoi()
        {
            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

            foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
            {
                //MIL_INT graphicLabel = mu.RoiLableID;
                
                //MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);

                MIL.MgraColor(MyMilDisplay.MilGraphicsContext, MIL.M_COLOR_RED);
                MIL.MgraArc(MyMilDisplay.MilGraphicsContext, MyMilDisplay.MilGraphicsList, mu.CenterX, mu.CenterY, mu.Radius, mu.Radius, 0, 360);

                //MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_LAST_LABEL, ref graphicLabel);

                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(mu.RoiLableID), MIL.M_DEFAULT, MIL.M_VISIBLE, MIL.M_TRUE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(mu.RoiLableID), MIL.M_DEFAULT, MIL.M_ROTATABLE, MIL.M_DISABLE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(mu.RoiLableID), MIL.M_DEFAULT, MIL.M_SELECTABLE, MIL.M_ENABLE);
                if (mu.RoiLableID == this.selectedROILabel) { MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(mu.RoiLableID), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE); }
            }

            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_UPDATE, MIL.M_NOW);
        }
        #endregion

        #region --- UpdateDataGridView ---
        private void UpdateDataGridView()
        {
            int curSelectRow = -1;
            foreach (DataGridViewRow row in dgvRoi.SelectedRows)
            {
                if (row.Selected)
                {
                    curSelectRow = row.Index;
                    break;
                }
            }
            //if (curSelectRow == -1) { }
            dgvRoi.Rows.Clear();
            foreach (MyMilIp.MeasureUnit mu in MyMilIp.ul)
            {
                //DataGridViewRow row = (DataGridViewRow)dgvRoi.Rows[0].Clone();
                //row.Cells[0].Value = mu.RoiName;
                //row.Cells[1].Value = mu.CenterX.ToString();
                //row.Cells[2].Value = mu.CenterY.ToString();
                //row.Cells[3].Value = mu.Radius.ToString();
                //row.Cells[4].Value = mu.Luminance.ToString();
                //row.Cells[5].Value = mu.Cx.ToString();
                //row.Cells[6].Value = mu.Cy.ToString();

                string[] row = new string[] { mu.RoiName, mu.CenterX.ToString(), mu.CenterY.ToString(), mu.Radius.ToString(),
                                              mu.Luminance.ToString(), mu.Cx.ToString(), mu.Cy.ToString() };
                dgvRoi.Rows.Add(row);
            }

            for (int i = 0; i < dgvRoi.Rows.Count; i++)
            {
                if (i == curSelectRow)
                {
                    dgvRoi.Rows[i].Selected = true;
                    dgvRoi.FirstDisplayedScrollingRowIndex = i;
                }
                else
                {
                    dgvRoi.Rows[i].Selected = false;
                }
            }
            dgvRoi.Refresh();
        }
        #endregion

        #region --- GetAllCalibrationFile ---
        private List<string> GetAllCalibrationFile()
        {
            List<string> calibrationFiles = new List<string>();
            string calibrationFile = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string[] files = Directory.GetFiles(calibrationFile);

            foreach (string file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".xml")
                {
                    calibrationFiles.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return calibrationFiles;
        }
        #endregion

        #region --- SetDataNameEvent ---
        private void SetDataNameEvent(string dataName)
        {
            int selected = this.lbxRecipeList.SelectedIndex;
            string oldDataName = lbxRecipeList.Items[selected].ToString();

            string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string oldFileName = $@"{oneColorCalibrationFolder}\{oldDataName}.xml";
            string newFileName = $@"{oneColorCalibrationFolder}\{dataName}.xml";

            if (oldFileName == newFileName)
                return;

            try
            {
                // ?
                if (File.Exists(oldFileName.Replace("OneColorCalibration","AutoLuminance")))
                {
                    File.Copy(oldFileName.Replace("OneColorCalibration", "AutoLuminance"), newFileName.Replace("OneColorCalibration", "AutoLuminance"));
                    File.Delete(oldFileName.Replace("OneColorCalibration", "AutoLuminance"));
                }

                if (File.Exists(oldFileName) | Directory.Exists(oneColorCalibrationFolder + "\\" + oldDataName))
                {
                    
                    if(File.Exists(oldFileName)) 
                    { 
                        File.Copy(oldFileName, newFileName);
                        File.Delete(oldFileName);
                    }

                    //rename dir: create, copy, delete
                    if (Directory.Exists(oneColorCalibrationFolder + "\\" + oldDataName))
                    {
                        Directory.CreateDirectory(oneColorCalibrationFolder + "\\" + dataName);
                        string[] fileList = Directory.GetFileSystemEntries(oneColorCalibrationFolder + "\\" + oldDataName);
                        foreach (string file in fileList)
                        {
                            File.Copy(file, oneColorCalibrationFolder + "\\" + dataName + "\\" + Path.GetFileName(file), true);
                        }
                        Directory.Delete(oneColorCalibrationFolder + "\\" + oldDataName, true);
                    }

                    this.lbxRecipeList.Items.Clear();
                    this.dataNameList = this.GetAllCalibrationFile();
                    this.lbxRecipeList.Items.AddRange(this.dataNameList.ToArray());

                    int index = lbxRecipeList.FindString(dataName);
                    if (index != ListBox.NoMatches)
                    {
                        lbxRecipeList.ClearSelected();
                        lbxRecipeList.SetSelected(index, true);
                        this.tbxCailbrationFile.Text = lbxRecipeList.Items[index].ToString();
                    }
                    else
                        this.tbxCailbrationFile.Text = "";
                }
                else
                {
                    throw new Exception("The error occurred while renaming the file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region --- NewDataNameEvent ---
        private void NewDataNameEvent(string dataName)
        {
            try
            {
                this.lbxRecipeList.Items.Clear();
                this.dataNameList = this.GetAllCalibrationFile();
                this.lbxRecipeList.Items.AddRange(this.dataNameList.ToArray());

                for (int i = 0; i < this.lbxRecipeList.Items.Count; i++)
                {
                    if (dataName == this.lbxRecipeList.Items[i].ToString())
                    {
                        MessageBox.Show("Recipe already exists, please change another name !");
                        return;
                    }
                }

                int index = lbxRecipeList.FindString(dataName);
                if (index != ListBox.NoMatches)
                {
                    lbxRecipeList.ClearSelected();
                    lbxRecipeList.SetSelected(index, true);
                    this.tbxCailbrationFile.Text = lbxRecipeList.Items[index].ToString();
                }
                else
                    this.tbxCailbrationFile.Text = "";


                this.lbxRecipeList.Items.Add(dataName);
                lbxRecipeList.SelectedIndex = this.lbxRecipeList.Items.Count - 1;
                lbxRecipeList.Text = dataName;

                MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

                string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
                string fullFileName = $@"{oneColorCalibrationFolder}\{dataName}.xml";

                try
                {
                    MyMilIp.ul.Clear();

                    MyMilIp.correctionConfig = new LightMeasureConfig();
                    // Update Matrix
                    this.tbxMatrixRow.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Row.ToString();
                    this.tbxMatrixColumn.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Column.ToString();

                    foreach (FourColorData dt in MyMilIp.correctionConfig.CorrectionData.DataList)
                    {
                        this.currentRadius = dt.CircleRegionInfo.Radius;

                        int graphicLabel = (int)this.AddRoi(dt.CircleRegionInfo.CenterX, dt.CircleRegionInfo.CenterY);
                        MyMilIp.MeasureUnit mu = MyMilIp.ul.Find(x => x.RoiLableID == graphicLabel);

                        if (mu != null)
                        {
                            mu.Luminance = dt.CieChroma.Luminance;
                            mu.Cx = dt.CieChroma.Cx;
                            mu.Cy = dt.CieChroma.Cy;
                        }

                    }

                    this.UpdateRoi();
                    this.UpdateDataGridView();

                    //this.tbxCailbrationFile.Text = dataName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region --- LoadLumCxCyDataEvent ---
        private void LoadLumCxCyDataEvent(string filePath)
        {
            int rowIdx = 0;
            DataGridViewRow row = null;
            double dbl_value = 0.0;
            bool isNumeric = false;

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
                        {
                            string[] values = line.Split(',');

                            if (rowIdx < dgvRoi.Rows.Count && double.TryParse(values[0], out dbl_value) &&
                                double.TryParse(values[1], out dbl_value) &&
                                double.TryParse(values[2], out dbl_value))
                            {
                                row = (DataGridViewRow)dgvRoi.Rows[rowIdx];
                                // Luminance
                                isNumeric = double.TryParse(values[0], out dbl_value);
                                if (isNumeric)
                                {
                                    row.Cells[4].Value = dbl_value;
                                    MyMilIp.ul[rowIdx].Luminance = dbl_value;
                                }

                                // Cx
                                isNumeric = double.TryParse(values[1], out dbl_value);
                                if (isNumeric)
                                {
                                    row.Cells[5].Value = dbl_value;
                                    MyMilIp.ul[rowIdx].Cx = dbl_value;
                                }

                                // Cy
                                isNumeric = double.TryParse(values[2], out dbl_value);
                                if (isNumeric)
                                {
                                    row.Cells[6].Value = dbl_value;
                                    MyMilIp.ul[rowIdx].Cy = dbl_value;
                                }

                                rowIdx += 1;
                            }
                        }
                        else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
                        {
                            if (rowIdx < dgvRoi.Rows.Count && double.TryParse(line, out dbl_value))
                            {
                                row = (DataGridViewRow)dgvRoi.Rows[rowIdx];
                                // Luminance
                                isNumeric = double.TryParse(line, out dbl_value);
                                if (isNumeric) row.Cells[4].Value = dbl_value;

                                rowIdx += 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }

        }

        #endregion

        #region --- ImageZoonAll ---
        private void ImageZoonAll()
        {
            MIL_ID image = MIL.M_NULL;
            double s = 0.0;

            MIL.MdispInquire(MyMil.MilMainDisplay, MIL.M_SELECTED, ref image);
            if (image == MIL.M_NULL) return;

            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;
            int DisplayWidth = this.pnlOneColorCalibrationViewer.Size.Width;
            int DisplayHeight = this.pnlOneColorCalibrationViewer.Size.Height;

            SizeX = MIL.MbufInquire(image, MIL.M_SIZE_X, MIL.M_NULL);
            SizeY = MIL.MbufInquire(image, MIL.M_SIZE_Y, MIL.M_NULL);

            if (SizeX > 0 && SizeY > 0)
            {
                s = Math.Max((double)DisplayWidth / (double)SizeX, (double)DisplayHeight / (double)SizeY);
                MIL.MdispZoom(MyMil.MilMainDisplay, s, s);
            }
        }
        #endregion

        #endregion


        #region --- Panel Event ---

        #region --- pnlOneColorCalibrationViewer_MouseMove ---
        private void pnlOneColorCalibrationViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.selectedROILabel != -1 && selectedMeasureUnitIndex != -1)
            {
                //MIL_INT ListCount = -1;
                //MIL_INT GraphicLabel = -1;
                //MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS, ref ListCount);
                //for (MIL_INT i = 0; i < ListCount; i++)
                //{
                //    if (MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_NULL) == MIL.M_TRUE)
                //    {
                //        MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE, ref GraphicLabel);
                //        break;
                //    }
                //}
                //if (GraphicLabel == -1) { return; }
                                
                int centerX = 0;
                int centerY = 0;
                int radius = 0;
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.selectedROILabel), MIL.M_DEFAULT, MIL.M_CENTER_X + MIL.M_TYPE_MIL_INT32, ref centerX);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.selectedROILabel), MIL.M_DEFAULT, MIL.M_CENTER_Y + MIL.M_TYPE_MIL_INT32, ref centerY);
                MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(this.selectedROILabel), MIL.M_DEFAULT, MIL.M_RADIUS_X + MIL.M_TYPE_MIL_INT32, ref radius);

                //int index = -1;
                //index = MyMilIp.ul.FindIndex(s => s.RoiLableID == this.selectedROILabel);
                //if (index == -1) { return; }

                if (MyMilIp.ul[selectedMeasureUnitIndex].CenterX != centerX) { MyMilIp.ul[selectedMeasureUnitIndex].CenterX = centerX; }
                if (MyMilIp.ul[selectedMeasureUnitIndex].CenterY != centerY) { MyMilIp.ul[selectedMeasureUnitIndex].CenterY = centerY; }
                if (MyMilIp.ul[selectedMeasureUnitIndex].Radius != radius) 
                { 
                    MyMilIp.ul[selectedMeasureUnitIndex].Radius = radius;
                    currentRadius = radius;
                    isNeedToUpdateRadius = true;
                }
                MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_UPDATE, MIL.M_NOW);
                //UpdateRoi();
                UpdateDataGridView(); 
            }
        }
        #endregion

        #region --- pnlOneColorCalibrationViewer_MouseEnter ---
        private void pnlOneColorCalibrationViewer_MouseEnter(object sender, EventArgs e)
        {
            pnlOneColorCalibrationViewer.Focus();
        }
        #endregion

        #region --- pnlOneColorCalibrationViewer_MouseUp ---
        private void pnlOneColorCalibrationViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (isNeedToUpdateRadius)
            {
                foreach (MyMilIp.MeasureUnit unit in MyMilIp.ul)
                {
                    unit.Radius = currentRadius;
                }
                UpdateRoi();
                UpdateDataGridView();
                isNeedToUpdateRadius = false;
            }
            this.selectedROILabel = -1;
            selectedMeasureUnitIndex = -1;
        }
        #endregion

        #region --- pnlOneColorCalibrationViewer_MouseDown ---
        private void pnlOneColorCalibrationViewer_MouseDown(object sender, MouseEventArgs e)
        {
            MIL_INT listCount = -1;

            MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_LIST, MIL.M_DEFAULT, MIL.M_NUMBER_OF_GRAPHICS + MIL.M_TYPE_MIL_INT, ref listCount);

            for (MIL_INT i = 0; i < listCount; i++)
            {
                if (MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_NULL) == MIL.M_TRUE)
                {
                    MIL.MgraInquireList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_INDEX(i), MIL.M_DEFAULT, MIL.M_LABEL_VALUE + MIL.M_TYPE_MIL_INT, ref this.selectedROILabel);
                    selectedMeasureUnitIndex = MyMilIp.ul.FindIndex(s => s.RoiLableID == this.selectedROILabel);
                    GraphicSelectValueChangedEvent((object)this.selectedROILabel);
                    break;
                }
            }
        }
        #endregion

        #endregion

        #region --- DataGridView Event ---

        #region --- dgvRoi_KeyDown ---
        private void dgvRoi_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    {
                        if (this.curDgvRow >= 0)
                        {
                            DeleteRoi(dgvRoi.Rows[curDgvRow].Cells[0].Value.ToString());
                        }
                        UpdateRoi();
                        UpdateDataGridView();
                    }
                    break;

                case Keys.Enter:
                    {
                        try
                        {
                            //int curROI_ID = Convert.ToInt32(dgvRoi.Rows[curDgvRow].Cells[0].Value.ToString().Split('_')[1]);
                            int index = MyMilIp.ul.FindIndex(x => x.RoiName == dgvRoi.Rows[curDgvRow].Cells[0].Value.ToString());
                            double CellValue = (double)dgvRoi.Rows[this.curDgvRow].Cells[this.curDgvCol].Value;
                            
                            switch (this.curDgvCol)
                            {
                                case 4:
                                    MyMilIp.ul[index].Luminance = CellValue;
                                    break;
                                case 5:
                                    MyMilIp.ul[index].Cx = CellValue;
                                    break;
                                case 6:
                                    MyMilIp.ul[index].Cy = CellValue;
                                    break;
                            }
                        }
                        catch 
                        {

                        }
                    }
                    break;

                    //case Keys.C:
                    //    {
                    //        if (!e.Control) break;

                    //        // 檢查是否有選擇的資料格或行列
                    //        if (dgvRoi.SelectedCells.Count > 0 || dgvRoi.SelectedRows.Count > 0)
                    //        {
                    //            // 將選擇的資料格或行列儲存為純文字格式放入剪貼簿中
                    //            StringBuilder sb = new StringBuilder();

                    //            // 先加入資料列標題
                    //            if (dgvRoi.ColumnHeadersVisible)
                    //            {
                    //                foreach (DataGridViewColumn column in dgvRoi.Columns)
                    //                {
                    //                    sb.Append(column.HeaderText + "\t");
                    //                }

                    //                sb.AppendLine();
                    //            }

                    //            // 加入選擇的資料列
                    //            foreach (DataGridViewRow row in dgvRoi.SelectedRows)
                    //            {
                    //                foreach (DataGridViewCell cell in row.Cells)
                    //                {
                    //                    sb.Append(cell.Value + "\t");
                    //                }

                    //                sb.AppendLine();
                    //            }

                    //            CopyToClipboard(sb.ToString());
                    //        }
                    //        break;
                    //    }
            }
        }
        #endregion

        #region --- dgvRoi_CellMouseUp ---
        //private void dgvRoi_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        //{          
        //    if (e.RowIndex >= 0 && e.RowIndex < MyMilIp.ul.Count)
        //    {
        //        this.curDgvRow = e.RowIndex;
        //        this.curDgvCol = e.ColumnIndex;

        //        MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
        //        MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_INDEX(this.curDgvRow), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

        //        dgvRoi.ClearSelection();
        //        dgvRoi.Rows[this.curDgvRow].Selected = true;
        //    }
        //}
        #endregion

        #region --- dgvRoi_CellMouseDown ---
        //private void dgvRoi_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    if (e.RowIndex >= 0 && e.RowIndex < MyMilIp.ul.Count)
        //    {
        //        this.curDgvCol = e.ColumnIndex;
        //        this.curDgvRow = e.RowIndex;
        //        if (e.Button == MouseButtons.Right) { contextMenuStripRoiEditor.Show(e.Location); }
        //    }
        //    else
        //    {
        //        this.curDgvCol = -1;
        //        this.curDgvRow = -1;
        //    }
        //}
        #endregion

        #region --- dgvRoi_CellMouseClick---
        private void dgvRoi_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < MyMilIp.ul.Count)
            {
                this.curDgvRow = e.RowIndex;
                this.curDgvCol = e.ColumnIndex;
                
                //dgv index = list index, 
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_ALL_SELECTED, MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_FALSE);
                MIL.MgraControlList(MyMilDisplay.MilGraphicsList, MIL.M_GRAPHIC_LABEL(MyMilIp.ul.Find(x => x.RoiName == dgvRoi.Rows[curDgvRow].Cells[0].Value.ToString()).RoiLableID), MIL.M_DEFAULT, MIL.M_GRAPHIC_SELECTED, MIL.M_TRUE);

                dgvRoi.ClearSelection();
                dgvRoi.Rows[this.curDgvRow].Selected = true;

                if (e.Button == MouseButtons.Right) { contextMenuStripRoiEditor.Show(e.Location); }
            }
            else
            {
                this.curDgvCol = -1;
                this.curDgvRow = -1;
            }
        }
        #endregion

        #region --- dgvRoi_CellValueChanged ---
        private void dgvRoi_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvRoi.Rows[e.RowIndex].Cells[0].Value == null ||
                    dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                {
                    return;
                }

                // Update DgvRow & DgvCol Index                
                //this.curDgvCol = e.ColumnIndex;
                //this.curDgvRow = e.RowIndex;

                string cellValue = dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                int index = MyMilIp.ul.FindIndex(x => x.RoiName == dgvRoi.Rows[e.RowIndex].Cells[0].Value.ToString()); //e.RowIndex;

                switch (this.curDgvCol)
                {
                    case 1:  // CenterX
                        {
                            int value = 0;

                            if (int.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }

                            MyMilIp.ul[index].CenterX = value;
                            this.UpdateRoi();
                        }
                        break;
                    case 2:  // CenterY 
                        {
                            int value = 0;

                            if (int.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }

                            MyMilIp.ul[index].CenterY = value;
                            this.UpdateRoi();
                        }
                        break;
                    case 3:  // Radius
                        {
                            int value = 0;

                            if (int.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }

                            foreach (MyMilIp.MeasureUnit unit in MyMilIp.ul)
                            {
                                unit.Radius = value;
                            }
                            this.currentRadius = value;
                            this.UpdateRoi();
                            this.UpdateDataGridView();
                        }
                        break;
                    case 4:  // Luminance
                        {
                            double value = 0.0;

                            if (double.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.0;
                            }

                            MyMilIp.ul[index].Luminance = value;
                        }
                        break;
                    case 5:  // Cx
                        {
                            double value = 0.0;

                            if (double.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.0;
                            }

                            MyMilIp.ul[index].Cx = value;
                        }
                        break;
                    case 6:  // Cy
                        {
                            double value = 0.0;

                            if (double.TryParse(cellValue, out value))
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString();
                            }
                            else
                            {
                                dgvRoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.0;
                            }

                            MyMilIp.ul[index].Cy = value;
                        }
                        break;
                }
            }
            catch 
            {

            }
        }
        #endregion


        #endregion

        #region --- contextMenuStripe Event ---
        private void contextMenuStripRoiEditor_Opening(object sender, CancelEventArgs e)
        {
            if (lbxRecipeList.SelectedIndex < 0)
            {
                contextMenuStripRoiEditor.Items[0].Enabled = false;
                contextMenuStripRoiEditor.Items[1].Enabled = false;
                contextMenuStripRoiEditor.Items[2].Enabled = false;
                
                return;
            }

            if (this.curDgvRow < 1 && MyMilIp.ul.Count < 1)
            {
                contextMenuStripRoiEditor.Items[0].Enabled = true;
                contextMenuStripRoiEditor.Items[1].Enabled = false;
                contextMenuStripRoiEditor.Items[2].Enabled = false;
            }
            else
            {
                contextMenuStripRoiEditor.Items[0].Enabled = true;
                contextMenuStripRoiEditor.Items[1].Enabled = true;
                contextMenuStripRoiEditor.Items[2].Enabled = true;
            }
        }
        #endregion

        #region --- tsmi_RoiEditor Event ---

        #region --- tsmi_RoiEditor_New_Click ---
        private void tsmi_RoiEditor_New_Click(object sender, EventArgs e)
        {
            AddRoi();
            MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_UPDATE, MIL.M_NOW);
            UpdateDataGridView();
        }
        #endregion

        #region --- tsmi_RoiEditor_Copy_Click ---
        private void tsmi_RoiEditor_Copy_Click(object sender, EventArgs e)
        {
            if (this.curDgvRow >= 0)
            {
                //int centerX = int.Parse(this.dgvRoi.Rows[curDgvRow].Cells[1].Value.ToString());
                //int centerY = int.Parse(this.dgvRoi.Rows[curDgvRow].Cells[2].Value.ToString());

                AddRoi((int)dgvRoi.Rows[curDgvRow].Cells[1].Value, (int)dgvRoi.Rows[curDgvRow].Cells[2].Value);
                MIL.MdispControl(MyMil.MilMainDisplay, MIL.M_UPDATE, MIL.M_NOW);
                UpdateDataGridView();
            }                       
        }
        #endregion

        #region --- tsmi_RoiEditor_Delete_Click ---
        private void tsmi_RoiEditor_Delete_Click(object sender, EventArgs e)
        {
            if (this.curDgvRow >= 0)
            {
                DeleteRoi(dgvRoi.Rows[this.curDgvRow].Cells[0].Value.ToString());
                this.UpdateRoi();
                this.UpdateDataGridView();
            }
        }
        #endregion

        #endregion

        #region --- tsmi_LoadData Event ---

        #region --- tsmi_LoadData_New_Click ---
        private void tsmi_LoadData_New_Click(object sender, EventArgs e)
        {
            frmSetDataName frmSetDataName = new frmSetDataName();
            frmSetDataName.SetDataName += NewDataNameEvent;
            frmSetDataName.Show();

            this.ImageZoonAll();
        }
        #endregion

        #region --- tsmi_LoadData_Load_Click ---
        private void tsmi_LoadData_Load_Click(object sender, EventArgs e)
        {
            if (lbxRecipeList.SelectedIndex == -1)
            {
                MessageBox.Show("Select recipe first !");
                return;
            }

            string dataName = "";           

            dataName = lbxRecipeList.Items[lbxRecipeList.SelectedIndex].ToString();
            this.tbxCailbrationFile.Text = dataName;

            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

            string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string fullFileName = $@"{oneColorCalibrationFolder}\{dataName}.xml";

            try
            {
                if (File.Exists(fullFileName))
                {
                    MyMilIp.ul.Clear();

                    MyMilIp.correctionConfig = new LightMeasureConfig();
                    MyMilIp.correctionConfig.ReadWithoutCrypto(fullFileName);

                    // Update Matrix
                    this.tbxMatrixRow.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Row.ToString();
                    this.tbxMatrixColumn.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Column.ToString();                    

                    foreach (FourColorData dt in MyMilIp.correctionConfig.CorrectionData.DataList)
                    {
                        this.currentRadius = dt.CircleRegionInfo.Radius;

                        int graphicLabel = (int)this.AddRoi(dt.CircleRegionInfo.CenterX, dt.CircleRegionInfo.CenterY);
                        MyMilIp.MeasureUnit mu = MyMilIp.ul.Find(x => x.RoiLableID == graphicLabel);

                        if (mu != null)
                        {
                            mu.Luminance = dt.CieChroma.Luminance;
                            mu.Cx = dt.CieChroma.Cx;
                            mu.Cy = dt.CieChroma.Cy;
                        }

                    }

                    this.UpdateRoi();
                    this.UpdateDataGridView();
                    this.ImageZoonAll();                    
                }
                else
                {
                    lbxRecipeList.Items.RemoveAt(lbxRecipeList.SelectedIndex);
                    throw new Exception("The error occurred while loading the file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region --- tsmi_LoadData_Delete_Click ---
        private void tsmi_LoadData_Delete_Click(object sender, EventArgs e)
        {
            if (lbxRecipeList.SelectedIndex == -1)
            {
                MessageBox.Show("Select recipe first !");
                return;
            }

            int selected = lbxRecipeList.SelectedIndex;
            string dataName = lbxRecipeList.Items[selected].ToString();

            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

            string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string fullFileName = $@"{oneColorCalibrationFolder}\{dataName}.xml";
            
            string calibrationFileDir = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration\{dataName}";

            try
            {
                if (MessageBox.Show("Do you want to delete the data? This will not be undone.", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {

                    if (File.Exists(fullFileName.Replace("OneColorCalibration", "AutoLuminance"))) { File.Delete(fullFileName.Replace("OneColorCalibration", "AutoLuminance")); }

                    if (File.Exists(fullFileName) | Directory.Exists(calibrationFileDir))
                    {
                        MyMilIp.ul.Clear();

                        if (File.Exists(fullFileName)) { File.Delete(fullFileName); }

                        if (Directory.Exists(calibrationFileDir)) { Directory.Delete(calibrationFileDir, true); }

                        this.UpdateRoi();
                        this.UpdateDataGridView();

                        this.tbxCailbrationFile.Text = "";
                        this.tbxMatrixRow.Text = "";
                        this.tbxMatrixColumn.Text = "";

                        this.lbxRecipeList.Items.Clear();
                        this.dataNameList = this.GetAllCalibrationFile();
                        this.lbxRecipeList.Items.AddRange(this.dataNameList.ToArray());
                    }
                    else
                    {
                        lbxRecipeList.Items.RemoveAt(selected);
                        throw new Exception("The error occurred while deleting the file.");
                    }
                }

                this.ImageZoonAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region --- tsmi_LoadData_Rename_Click ---
        private void tsmi_LoadData_Rename_Click(object sender, EventArgs e)
        {
            //Func<bool> func = OFD;
            //Task task = new Task(() =>
            //{
            //    string res = (string)this.Invoke(func);//同步！讓UI執行緒自己做
            //});
            //task.Start();

            try
            {
                if (lbxRecipeList.SelectedIndex == -1)
                {
                    MessageBox.Show("Select recipe first !");
                    return;
                }

                frmSetDataName frmSetDataName = new frmSetDataName();
                frmSetDataName.SetDataName += SetDataNameEvent;
                frmSetDataName.Show();

                this.ImageZoonAll();
            }           
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        #endregion

        #endregion

        #region --- Graphic Event ---

        #region --- GraphicSelectValueChangedEvent ---
        private void GraphicSelectValueChangedEvent(object lableId)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<object>(GraphicSelectValueChangedEvent), lableId);
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
                dgvRoi.ClearSelection();


                MyMilIp.MeasureUnit unit = MyMilIp.ul.Find(t => t.RoiLableID.ToString() == lableId.ToString());
                if (unit == null) return;

                for (int i = 0; i < dgvRoi.Rows.Count - 1; i++)
                {
                    if (dgvRoi.Rows[i].Cells[0].Value.ToString() == unit.RoiName)
                    {
                        dgvRoi.Rows[i].Selected = true;
                        break;
                    }
                }

                dgvRoi.Refresh();
            }
        }
        #endregion

        #region --- GraphicDeleteEvent ---
        private void GraphicDeleteEvent(int obj)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<int>(GraphicDeleteEvent), obj);
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
                dgvRoi.ClearSelection();

                MyMilIp.MeasureUnit removeUnit = MyMilIp.ul.Find(t => t.RoiLableID == obj);

                if (removeUnit == null) return;

                int removeOrder = int.Parse(removeUnit.RoiName.Split('_')[1]);

                // remove roi in list
                MyMilIp.ul.RemoveAll(t => t.RoiLableID == obj);

                // renamed remaining roi
                foreach (MyMilIp.MeasureUnit unit in MyMilIp.ul)
                {
                    string sType = unit.RoiName.Split('_')[0];
                    string sOrder = unit.RoiName.Split('_')[1];
                    int iOrder = int.Parse(sOrder);

                    if (iOrder < removeOrder) continue;

                    unit.RoiName = $"{sType}_{iOrder - 1}";
                }

                this.UpdateDataGridView();
            }
        }
        #endregion

        #endregion

        #region --- ListBox Event ---

        #region --- lbxRecipeList_SelectedIndexChanged ---
        private void lbxRecipeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = this.lbxRecipeList.SelectedIndex;
            if (selected == -1) return;
        }
        #endregion

        #region --- lbxRecipeList_MouseDown ---
        private void lbxRecipeList_MouseDown(object sender, MouseEventArgs e)
        {
            //if (lbxRecipeList.SelectedIndex == -1)
            //{
            //    MessageBox.Show("Select recipe firs !");
            //    return;
            //}           

            if (e.Button == MouseButtons.Right)
            {
                contextMenuStripLoadData.Show(lbxRecipeList, e.Location);
            }
        }
        #endregion

        #endregion

        #region --- Button Event ---

        #region --- btnSave_Click ---
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (lbxRecipeList.SelectedIndex == -1)
            {
                MessageBox.Show("Select recipe firs !");
                return;
            }

            int dataListCount = MyMilIp.ul.Count;
            int SettingCount = Convert.ToInt32(tbxMatrixRow.Text) * Convert.ToInt32(tbxMatrixColumn.Text);
            if (dataListCount != SettingCount)
            {
                MessageBox.Show(string.Format("Row * Column數量[{0}]，不等於量測數量[{1}]，請修正 !", SettingCount, dataListCount));
                return;
            }

            string dataName = lbxRecipeList.Items[lbxRecipeList.SelectedIndex].ToString();
            this.SaveClibrationData(dataName);

            GlobalVar.ProcessInfo.CalibrationFile = dataName;

            if (MyMilIp.IsUserCoeffFinish == MyMilIp.EnCalibrationState.Successful)
            {
                this.DialogResult = AddCIELuminance_mcb.Checked ? DialogResult.Yes : DialogResult.No;
                this.Close();
            }

            this.ImageZoonAll();
        }

        #endregion

        #region --- btnLoad_Click ---
        private void btnLoad_Click(object sender, EventArgs e)
        {
            string calibrationFile = this.tbxCailbrationFile.Text;

            if (calibrationFile == "") return;

            MIL.MgraClear(MIL.M_DEFAULT, MyMilDisplay.MilGraphicsList);

            string oneColorCalibrationFolder = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
            string fullFileName = $@"{oneColorCalibrationFolder}\{calibrationFile}.xml";

            if (File.Exists(fullFileName))
            {
                MyMilIp.ul.Clear();

                MyMilIp.correctionConfig = new LightMeasureConfig();
                MyMilIp.correctionConfig.ReadWithoutCrypto(fullFileName);

                // Update Matrix
                this.tbxMatrixRow.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Row.ToString();
                this.tbxMatrixColumn.Text = MyMilIp.correctionConfig.CorrectionData.MeasureMatrixInfo.Column.ToString();

                foreach (FourColorData dt in MyMilIp.correctionConfig.CorrectionData.DataList)
                {
                    this.currentRadius = dt.CircleRegionInfo.Radius;

                    int graphicLabel = (int)this.AddRoi(dt.CircleRegionInfo.CenterX, dt.CircleRegionInfo.CenterY);
                    MyMilIp.MeasureUnit mu = MyMilIp.ul.Find(x => x.RoiLableID == graphicLabel);

                    if (mu != null)
                    {
                        mu.Luminance = dt.CieChroma.Luminance;
                        mu.Cx = dt.CieChroma.Cx;
                        mu.Cy = dt.CieChroma.Cy;
                    }

                }

                this.UpdateRoi();
                this.UpdateDataGridView();
            }
            else
            {
                MessageBox.Show("There was error opening this document.");
            }
        }


        #endregion

        #region --- btnLoad_Lum_Cx_Cy_Click ---

        private void btnLoad_Lum_Cx_Cy_Click(object sender, EventArgs e)
        {
            frmGetLumCxCyFilePath frmGetFullFilePath = new frmGetLumCxCyFilePath();
            frmGetFullFilePath.Show();              
        }

        #endregion

        #endregion

        #region --- AutoLum_btn_Click ---
        private void AutoLum_btn_Click(object sender, EventArgs e)
        {
            try
            {
                //readdata
                string CalibrationFilename = GlobalVar.Config.RecipePath + "\\AutoLuminance\\" + lbxRecipeList.Items[lbxRecipeList.SelectedIndex].ToString() + ".xml";
                if (!File.Exists(CalibrationFilename))
                { 
                    MessageBox.Show(this,CalibrationFilename + " not found! Add Lum first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                GlobalVar.AutoLuminance = ClsAutoLuminance.ReadData(CalibrationFilename);
                if (GlobalVar.AutoLuminance.LuminancePair.Count < 2) 
                {
                    MessageBox.Show(this, "Luminance count < 2, cannot do linear interpolation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }   

                //calculate roi factor
                double targetfactor = 0; //mean/exp.
                FourColorData fourColorData = new FourColorData();
                fourColorData.CircleRegionInfo.CenterX = int.Parse(dgvRoi.Rows[0].Cells[1].Value.ToString());
                fourColorData.CircleRegionInfo.CenterY = int.Parse(dgvRoi.Rows[0].Cells[2].Value.ToString());
                fourColorData.CircleRegionInfo.Radius = int.Parse(dgvRoi.Rows[0].Cells[3].Value.ToString());
                LightMeasurer measurer = new LightMeasurer(MyMil.MilSystem);
                //MyMilIp.correctionConfig.CorrectionData.RegionMethod = EnumRegionMethod.Circle;
                measurer.CalculateRegionMean(MIL.M_NULL, MyMilDisplay.MilMainDisplayImage[1], MIL.M_NULL, EnumRegionMethod.Circle, ref fourColorData, GraymeanBypassPercent);
                targetfactor = fourColorData.GrayMean.Y / GlobalVar.ProcessInfo.XyzExposureTime[1];

                //Linear_Interpolation or Regression
                double resultLuminance = 0;
                if (GlobalVar.AutoLuminance.LuminancePair.Count == 2)
                {
                    MyMilIp.Linear_Interpolation(GlobalVar.AutoLuminance.LuminancePair[0].Factor, GlobalVar.AutoLuminance.LuminancePair[0].Luminance,
                                                 GlobalVar.AutoLuminance.LuminancePair[1].Factor, GlobalVar.AutoLuminance.LuminancePair[1].Luminance, targetfactor, ref resultLuminance);
                }
                else
                {
                    if (targetfactor < GlobalVar.AutoLuminance.LuminancePair[0].Factor)
                    {
                        PointD[] points = new PointD[3];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i].X = GlobalVar.AutoLuminance.LuminancePair[i].Factor;
                            points[i].Y = GlobalVar.AutoLuminance.LuminancePair[i].Luminance;
                        }
                        MyMilIp.Linear_Regression(points, targetfactor, ref resultLuminance);
                    }
                    else if (targetfactor > GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1].Factor)
                    {
                        PointD[] points = new PointD[3];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i].X = GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1 - i].Factor;
                            points[i].Y = GlobalVar.AutoLuminance.LuminancePair[GlobalVar.AutoLuminance.LuminancePair.Count - 1 - i].Luminance;
                        }
                        MyMilIp.Linear_Regression(points, targetfactor, ref resultLuminance);
                    }
                    else
                    {
                        for (int IntervalStartIndex = 0; IntervalStartIndex < GlobalVar.AutoLuminance.LuminancePair.Count - 1; IntervalStartIndex++)
                        {
                            if (GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex].Factor <= targetfactor && targetfactor <= GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex + 1].Factor)
                            {
                                MyMilIp.Linear_Interpolation(GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex].Factor, GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex].Luminance,
                                                             GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex + 1].Factor, GlobalVar.AutoLuminance.LuminancePair[IntervalStartIndex + 1].Luminance, targetfactor, ref resultLuminance);
                                break;
                            }
                        }
                    }
                }
                //write dgvroi.cell[4]、mymilip.ul[].liminance
                dgvRoi.Rows[0].Cells[4].Value = resultLuminance;
                MyMilIp.ul[0].Luminance = resultLuminance;
                //dgvRoi.Rows[1].Cells[0].Value = fourColorData.GrayMean.Y;
                //dgvRoi.Rows[1].Cells[1].Value = GlobalVar.ProcessInfo.XyzExposureTime[1];
                //dgvRoi.Rows[1].Cells[2].Value = targetfactor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}