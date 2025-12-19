using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MaterialSkin.Controls;
using BaseTool;

namespace OpticalMeasuringSystem
{
    public partial class frmCorrSetting : MaterialForm
    {
        public event Action UpdataSD;
        public frmCorrSetting()
        {
            InitializeComponent();
        }

        private string FilePath = GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml";
        LumCorrectionPara Para = new LumCorrectionPara();

        private void frmSystemSetting_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(FilePath))
                    Para = Para.Read(FilePath);
                Para.Create(Para, FilePath);
                propertyGrid_Para.SelectedObject = Para;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region --- Button Event ---

        #region --- button_Save_Click ---
        private void button_Save_Click(object sender, EventArgs e)
        {
            try
            {
                Para.Create(Para, FilePath);

                GlobalVar.CorrData = GlobalVar.CorrData.Read(GlobalVar.Config.ConfigPath + @"\LumCorrectionPara.xml");

                UpdataSD?.Invoke();

                MessageBox.Show("Save " + FilePath + " OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion


        #region --- button_Cancel_Click ---
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #endregion
    }
}
