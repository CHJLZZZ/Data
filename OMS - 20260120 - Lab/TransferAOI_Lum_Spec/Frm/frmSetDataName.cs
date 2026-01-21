using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpticalMeasuringSystem
{
    public partial class frmSetDataName : MaterialForm
    {
        public event Action<string> SetDataName;

        public frmSetDataName()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string dataName = "";

            try
            {
                dataName = tbxDataName.Text;
                SetDataName?.Invoke(dataName);
            }
            catch 
            {

            }
            finally
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
