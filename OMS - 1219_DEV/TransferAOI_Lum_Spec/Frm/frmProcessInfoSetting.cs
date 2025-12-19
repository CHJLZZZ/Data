using MaterialSkin.Controls;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseTool;
using System.Reflection;

namespace OpticalMeasuringSystem
{
    public partial class frmProcessInfoSetting : MaterialForm
    {
        /// <param name="pattern">跑片畫面</param>
        /// <param name="panelId">片號(目前未使用)</param>
        /// <param name="action">執行流程</param>
        /// <param name="useNd">預計要使用的ND</param>
        /// <param name="calibrationFile">預計要使用的calibration File</param>

        private EnumMotoControlPackage motoControlPackage = GlobalVar.DeviceType;

        public event Action<string, string, string, string, string, string> SetProcessInfo;

        public List<string> calibrationFiles;
        public List<string> ffcFiles;

        public frmProcessInfoSetting()
        {
            InitializeComponent();

            HideUI();

            if (!GlobalVar.OmsParam.FfcIsDoStep)
            {
                Lbl_FFCFile.Visible = false;
                Cbx_FFCFile.Visible = false;
            }

            // UI
            if (GlobalVar.ProcessInfo.ActionIndex != -1)
            {
                this.Cbx_Action.SelectedIndex = GlobalVar.ProcessInfo.ActionIndex;
            }

            if (GlobalVar.ProcessInfo.NdIndex != -1)
            {
                this.Cbx_NdPosition_X.SelectedIndex = GlobalVar.ProcessInfo.NdIndex;
            }

            // CalibrationFile
            this.Cbx_CalibrationFile.Items.Clear();
            this.calibrationFiles = this.GetAllCalibrationFile();

            this.Cbx_CalibrationFile.Items.AddRange(this.calibrationFiles.ToArray());

            //Spectral Type
            this.Cbx_SpectralType.Items.Clear();
            this.Cbx_SpectralType.Items.Add("Auto");

            foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
            {
                // 獲取枚舉成員的 FieldInfo
                FieldInfo field = Para.ND.GetType().GetField(Para.ND.ToString());
                // 獲取 DescriptionAttribute
                DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                string ND = attribute.Description;
                string Gray = Para.TargetGray.ToString();

                this.Cbx_SpectralType.Items.Add($"{ND}_{Gray}");
            }

            // FFC
            this.Cbx_FFCFile.Items.Clear();
            this.ffcFiles = this.GetAllFFCFile();
            this.Cbx_FFCFile.Items.AddRange(this.ffcFiles.ToArray());

            switch (this.motoControlPackage)
            {
                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                    if (GlobalVar.SD.NDFilterName == "")
                    {
                        this.Cbx_NdPosition_X.SelectedIndex = 0;
                        this.Cbx_NdPosition_X.Enabled = false;
                    }
                    break;
            }
        }

        private void HideUI()
        {
            if (GlobalVar.IsSecretHidden)
            {
                this.Lbl_NdPosition.Visible = false;
                this.Cbx_NdPosition_X.Visible = false;
                this.Cbx_NdPosition_Y.Visible = false;
                this.Cbx_NdPosition_Z.Visible = false;

                this.Lbl_CalibrationFile.Visible = false;
                this.Cbx_CalibrationFile.Visible = false;

                Cbx_Action.Items.Clear();
                Cbx_Action.Items.Add("One Color Correction (Spectral)");
                Cbx_Action.Text = "One Color Correction (Spectral)";
            }
        }

        #region --- 方法函式 ---
        #region --- GetAllCalibrationFile ---
        private List<string> GetAllCalibrationFile()
        {
            List<string> calibrationFiles = new List<string>();

            try
            {
                string calibrationFile = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\OneColorCalibration";
                string[] files = Directory.GetFiles(calibrationFile);

                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".xml")
                    {
                        calibrationFiles.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                calibrationFiles = new List<string>();

            }

            return calibrationFiles;
        }
        #endregion

        #region --- GetAllFFCFile ---
        private List<string> GetAllFFCFile()
        {
            List<string> ffcDirectories = new List<string>();
            string ffcDirectory = $@"{GlobalVar.Config.ConfigPath}\{GlobalVar.Config.RecipeName}\FFC";
            string DirName = "";

            if (Directory.Exists(ffcDirectory))
            {
                string[] dirs = Directory.GetDirectories(ffcDirectory);

                foreach (string dir in dirs)
                {
                    DirName = Path.GetFileName(dir);
                    if (!DirName.ToUpper().StartsWith("ND") && !DirName.ToUpper().StartsWith("BACK"))
                    {
                        ffcDirectories.Add(DirName);
                    }
                }
            }

            return ffcDirectories;
        }
        #endregion

        #region --- SelectCalibrationFileIndex ---
        private bool SelectCalibrationFileIndex()
        {
            if (Cbx_CalibrationFile.Text == "")
            {
                MessageBox.Show("請選擇 Calibration File");
                return false;
            }

            return true;
        }

        #endregion

        #region --- SelectFFCFileIndex ---
        private bool SelectFFCFileIndex()
        {
            if (Cbx_FFCFile.Text == "")
            {
                MessageBox.Show("請選擇 FFC File");
                return false;
            }

            return true;
        }

        #endregion

        #region --- Check Spectral Para ---

        private bool CheckSpectralPara(string Type)
        {
            try
            {
                if (Type == "Auto") return true;

                if (Type == "")
                {
                    MessageBox.Show("請選擇 Spectral Type");
                    return false;
                }

                string[] SplitTxt = Type.Split('_');
                string ND = SplitTxt[0];
                string Gray = SplitTxt[1];

                bool Check = false;

                foreach (CorrectionPara Para in GlobalVar.CorrData.Para_List)
                {
                    // 獲取枚舉成員的 FieldInfo
                    FieldInfo field = Para.ND.GetType().GetField(Para.ND.ToString());
                    // 獲取 DescriptionAttribute
                    DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                    string Para_ND = attribute.Description;
                    string Para_Gray = Para.TargetGray.ToString();

                    if (ND == Para_ND && Gray == Para_Gray)
                    {
                        Check = true;
                        break;
                    }
                }

                return Check;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check Spectral Fail : {ex.Message}");
                return false;
            }
        }

        #endregion

        #endregion

        #region --- btn_ProcessSetting_Start_Click ---
        private void btn_ProcessSetting_Start_Click(object sender, EventArgs e)
        {
            GlobalVar.ProcessInfo.ActionIndex = this.Cbx_Action.SelectedIndex;
            GlobalVar.ProcessInfo.NdIndex = this.Cbx_NdPosition_X.SelectedIndex;
            GlobalVar.ProcessInfo.CalibrationFileIndex = this.Cbx_CalibrationFile.SelectedIndex;
            GlobalVar.ProcessInfo.FFCFileIndex = this.Cbx_FFCFile.SelectedIndex;

            if (Tbx_PanelID.Text == "")
            {
                MessageBox.Show("請輸入Panel ID");
                return;
            }

            string PanelID = Tbx_PanelID.Text;
            string Pattern = "W";
            string action = Cbx_Action.Text;
            string CalibrationFile = Cbx_CalibrationFile.Text;
            string SpectralType = Cbx_SpectralType.Text;
            string FFCFile = Cbx_FFCFile.Text;

            bool OneColorCreate = (action == "One Color Correction Data Create");
            bool OneColorCorrection_Calibration = (action == "One Color Correction (Calibration)");
            bool OneColorCorrection_Spectral = (action == "One Color Correction (Spectral)");
            bool NdPredict = (action == "Predict The ND-Filter To Use");

            if (OneColorCorrection_Calibration)
            {
                if (!this.SelectCalibrationFileIndex()) return;

                if (this.Cbx_CalibrationFile.Items.Count < 1)
                {
                    MessageBox.Show("請先做完動作 : One Color Correction Data Create");
                    return;
                }
            }

            if (OneColorCreate || OneColorCorrection_Calibration || OneColorCorrection_Spectral)
            {
                if (GlobalVar.OmsParam.FfcIsDoStep)
                {
                    if (!this.SelectFFCFileIndex()) return;
                }
            }

            if (OneColorCorrection_Spectral)
            {
                if (!this.CheckSpectralPara(SpectralType)) return;
            }

            GlobalVar.OmsParam.FilterWheel_ND_OneColor[0] = Cbx_NdPosition_X.SelectedIndex;
            GlobalVar.OmsParam.FilterWheel_ND_OneColor[1] = Cbx_NdPosition_Y.SelectedIndex;
            GlobalVar.OmsParam.FilterWheel_ND_OneColor[2] = Cbx_NdPosition_Z.SelectedIndex;
            GlobalVar.OmsParam.SpectralType = Cbx_SpectralType.Text;

            string ConfigFolder = GlobalVar.Config.ConfigPath + @"\" + GlobalVar.Config.RecipeName;
            string ConfigPath_29M = ConfigFolder + @"\param_29M.xml";

            GlobalVar.OmsParam.Create(GlobalVar.OmsParam, ConfigPath_29M);

            SetProcessInfo?.Invoke(Pattern, PanelID, action, CalibrationFile, SpectralType, FFCFile);

            this.Close();
        }

        #endregion

        #region --- PanelDataSetting_Load ---
        private void PanelDataSetting_Load(object sender, EventArgs e)
        {
            try
            {
                Cbx_NdPosition_X.SelectedIndex = GlobalVar.OmsParam.FilterWheel_ND_OneColor[0];
                Cbx_NdPosition_Y.SelectedIndex = GlobalVar.OmsParam.FilterWheel_ND_OneColor[1];
                Cbx_NdPosition_Z.SelectedIndex = GlobalVar.OmsParam.FilterWheel_ND_OneColor[2];
                Cbx_SpectralType.Text = GlobalVar.OmsParam.SpectralType;
            }
            catch
            {

            }
        }
        #endregion

        #region --- tbx_ProcessSetting_Action_SelectedIndexChanged ---
        private void tbx_ProcessSetting_Action_SelectedIndexChanged(object sender, EventArgs e)
        {
            string action = Cbx_Action.Text;

            bool OneColorCreate = (action == "One Color Correction Data Create");
            bool OneColorCorrection_Calibration = (action == "One Color Correction (Calibration)");
            bool OneColorCorrection_Spectral = (action == "One Color Correction (Spectral)");
            bool NdPredict = (action == "Predict The ND-Filter To Use");

            this.Lbl_NdPosition.Enabled = (OneColorCreate || OneColorCorrection_Calibration);
            this.Cbx_NdPosition_X.Enabled = (OneColorCreate || OneColorCorrection_Calibration);
            this.Cbx_NdPosition_Y.Enabled = (OneColorCreate || OneColorCorrection_Calibration);
            this.Cbx_NdPosition_Z.Enabled = (OneColorCreate || OneColorCorrection_Calibration);

            this.Lbl_CalibrationFile.Enabled = (OneColorCorrection_Calibration);
            this.Cbx_CalibrationFile.Enabled = (OneColorCorrection_Calibration);

            this.Lbl_SpectralType.Enabled = (OneColorCorrection_Spectral);
            this.Cbx_SpectralType.Enabled = (OneColorCorrection_Spectral);

            this.Lbl_FFCFile.Enabled = (OneColorCreate || OneColorCorrection_Calibration || OneColorCorrection_Spectral);
            this.Cbx_FFCFile.Enabled = (OneColorCreate || OneColorCorrection_Calibration || OneColorCorrection_Spectral);
        }
        #endregion

    }
}
