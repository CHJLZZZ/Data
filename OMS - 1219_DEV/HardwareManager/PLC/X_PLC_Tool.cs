using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseTool;

namespace HardwareManager
{
    public partial class X_PLC_Tool : MaterialForm
    {
        private X_PLC_Ctrl PLC;

        public X_PLC_Tool(ref X_PLC_Ctrl Ref)
        {
            InitializeComponent();


            this.PLC = Ref;

            this.PLC.UpdateUI -= PLC_UpdateUI;
            this.PLC.UpdateUI += PLC_UpdateUI;
        }

        private void PLC_BuildUI()
        {
            try
            {
                DGV_PLC2PC_Data.Invoke(new Action(() =>
                {
                    int Cnt = PLC.Plc2Pc.GetType().GetProperties().Length;
                    DGV_PLC2PC_Data.RowCount = Cnt;
                }));
            }
            catch 
            {
            }
        }

        private void PLC_UpdateUI(X_PLC_Ctrl.PlcData PLC_Data)
        {
            try
            {
                PropertyInfo[] Infos = PLC.Plc2Pc.GetType().GetProperties();
                //DGV_PLC2PC_Data.RowCount = Infos.Length;
                for (int i = 0; i < Infos.Length; i++)
                {
                    string DisplayName = BaseTool.PropertyManager.GetDisplayName(Infos[i]);
                    string Value = BaseTool.PropertyManager.GetValue(PLC.Plc2Pc, Infos[i]);
                    DGV_PLC2PC_Data.Rows[i].SetValues(new string[] { DisplayName, Value });
                }
            }
            catch 
            {
            }                        
        }

        private void Btn_Home_Click(object sender, EventArgs e)
        {
            PLC.Home();
        }

        private void Btn_AlmReset_Click(object sender, EventArgs e)
        {
            PLC.AlarmReset();
        }

        private void Btn_AbsMove_Click(object sender, EventArgs e)
        {
            PLC.Move(X_PLC_Ctrl.MoveMode.AbvMove);      
        }

        private void Btn_IncMove_Click(object sender, EventArgs e)
        {
            PLC.Move(X_PLC_Ctrl.MoveMode.IncMove);
        }

        private void Btn_Stop_Click(object sender, EventArgs e)
        {
            PLC.Stop();
        }

        private void X_PLC_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.PLC.UpdateUI -= PLC_UpdateUI;
        }

        private void Btn_JogF_MouseDown(object sender, MouseEventArgs e)
        {
            PLC.JogStart(true);
        }

        private void Btn_JogR_MouseDown(object sender, MouseEventArgs e)
        {
            PLC.JogStart(false);
        }

        private void Btn_JogR_MouseUp(object sender, MouseEventArgs e)
        {
            PLC.JogStop();
        }

        private void Btn_JogF_MouseUp(object sender, MouseEventArgs e)
        {
            PLC.JogStop();
        }

        private void X_PLC_Tool_Load(object sender, EventArgs e)
        {
            PLC_BuildUI();
        }

        private void Btn_SetJogSpeed_Click(object sender, EventArgs e)
        {
            int Speed = (int)Num_JogSpeed.Value;
            PLC.SetJogSpeed(Speed);
        }

        private void Btn_SetMoveSpeed_Click(object sender, EventArgs e)
        {
            int Speed = (int)Num_MoveSpeed.Value;
            PLC.SetMoveSpeed(Speed);
        }

        private void Btn_SetMovePos_Click(object sender, EventArgs e)
        {
            double Pos = (double)Num_MovePos.Value;
            PLC.SetMovePos(Pos);
        }

        private void Btn_ServoOn_Click(object sender, EventArgs e)
        {
            PLC.ServoOn();
        }

        private void Btn_ServoOff_Click(object sender, EventArgs e)
        {
            PLC.ServoOff();
        }

    
    }
}
