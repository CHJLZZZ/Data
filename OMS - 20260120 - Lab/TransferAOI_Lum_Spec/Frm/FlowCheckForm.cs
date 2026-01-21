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
    public partial class FlowCheckForm : MaterialForm
    {
        public event Action<bool> isCheck;

        private bool CheckFlag = false;

        public FlowCheckForm()
        {
            InitializeComponent();
        }

        public void SetPare(string PatternName)
        {
            if (Label_Msg.InvokeRequired)
            {
                Label_Msg.Invoke(new Action(() => { Label_Msg.Text = PatternName; }));
            }
            else
            {
                Label_Msg.Text = PatternName;
            }
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            DialogResult Result = MessageBox.Show("確定完成作業?", "完成確認", MessageBoxButtons.YesNo);
            if (Result == System.Windows.Forms.DialogResult.Yes)
            {
                CheckFlag = true;
                this.Close();
            }
        }

        private void FlowCheckForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isCheck?.Invoke(CheckFlag);
        }
    }
}
