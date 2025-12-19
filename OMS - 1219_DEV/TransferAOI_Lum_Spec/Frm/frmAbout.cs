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

namespace OpticalMeasuringSystem.Frm
{
    public partial class frmAboutAUO : MaterialForm
    {
        public frmAboutAUO()
        {
            InitializeComponent();
        }

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }
    }
}
