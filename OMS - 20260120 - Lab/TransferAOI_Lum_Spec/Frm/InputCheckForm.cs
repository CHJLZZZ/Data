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
    public partial class InputCheckForm : MaterialForm
    {
        public event Action<bool, double, double, double> isCheck;

        private bool CheckFlag = false;

        public InputCheckForm()
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
                double Lum = (double)Num_Lum.Value;
                double Cx = (double)Num_Cx.Value;
                double Cy = (double)Num_Cy.Value;

                if ( Cx == 0 || Cy == 0)
                {
                    MessageBox.Show("請輸入正確數值");
                    return;
                }


                CheckFlag = true;
                this.Close();
            }
        }

        private void FlowCheckForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            double Lum = (double)Num_Lum.Value;
            double Cx = (double)Num_Cx.Value;
            double Cy = (double)Num_Cy.Value;

            isCheck?.Invoke(CheckFlag, Lum, Cx, Cy);
        }
    }
}
