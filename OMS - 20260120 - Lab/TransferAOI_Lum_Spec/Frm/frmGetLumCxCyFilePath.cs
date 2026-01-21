using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using System.Threading;
using BaseTool;

namespace OpticalMeasuringSystem
{
    public partial class frmGetLumCxCyFilePath : MaterialForm
    {
        public event Action<string> Get_LumCxCy_FilePath;

        public frmGetLumCxCyFilePath()
        {
            InitializeComponent();

            if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance)
            {
                this.pB_LumCxCy_DataExample.Visible = false;
                this.pB_Lum_DataExample.Visible = true;
                this.pB_Lum_DataExample.Location = new System.Drawing.Point(18, 104);
            }
            else if (GlobalVar.SD.MeasureMode == EnumMeasureMode.Luminance_Chroma)
            {
                this.pB_LumCxCy_DataExample.Visible = true;
                this.pB_Lum_DataExample.Visible = false;
                this.pB_LumCxCy_DataExample.Location = new System.Drawing.Point(18, 104);
            }
        }

        //[STAThread]
        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Thread t = new Thread((ThreadStart)(() => {
                    OpenFileDialog openFileDialog = new OpenFileDialog();

                    openFileDialog.Title = "Select csv file";
                    openFileDialog.Filter = "csv files (*.csv)|*.csv";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.FileInfo fInfo = new System.IO.FileInfo(openFileDialog.FileName);
                        Get_LumCxCy_FilePath?.Invoke(fInfo.FullName);
                    }
                }));

                // Run your code from a thread that joins the STA Thread
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

            }
            catch
            {

            }
            finally
            {
                this.Close();
            }
        }
    }

        
}
