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
    public partial class frmSystemSetting : MaterialForm
    {
        public event Action UpdataSD;
        public frmSystemSetting()
        {
            InitializeComponent();
        }

        private string paramSystemFilePath = GlobalVar.Config.ConfigPath + @"\NetworkConfig.xml";
        clsSystemSetting paramSystem = new clsSystemSetting();

        private void frmSystemSetting_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(paramSystemFilePath))
                    paramSystem = paramSystem.Read(paramSystemFilePath);
                paramSystem.Create(paramSystem, paramSystemFilePath);
                propertyGrid_System.SelectedObject = paramSystem;
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
                paramSystem.Create(paramSystem, paramSystemFilePath);



                GlobalVar.SD = GlobalVar.SD.Read(GlobalVar.Config.ConfigPath + @"\NetworkConfig.xml");

                UpdataSD?.Invoke();

                MessageBox.Show("Save " + paramSystemFilePath + " OK");
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
