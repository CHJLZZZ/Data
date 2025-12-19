using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HardwareManager
{
    public partial class MS5515M_Tool : MaterialForm
    {
        private MS5515M Motor;
        private bool Monitor = false;

        public MS5515M_Tool(ref MS5515M Motor)
        {
            InitializeComponent();

            this.Motor = Motor;

            Motor.UpdateInfo -= Motor_UpdateInfo;
            Motor.UpdateInfo += Motor_UpdateInfo;
        }

        private void Motor_UpdateInfo(string Status, string AbsPos, string RelPos, string Frequecy)
        {
            // Tbx_Status.Invoke(new Action(() => Tbx_Status.Text = Status));
            // Tbx_AbsPos.Invoke(new Action(() => Tbx_AbsPos.Text = AbsPos));

            TbxEdit(Tbx_Status, Status);
            TbxEdit(Tbx_AbsPos, AbsPos);
            TbxEdit(Tbx_RelPos, RelPos);
            TbxEdit(Tbx_Frequecy, Frequecy);

            //Tbx_RelPos.Invoke(new Action(() => Tbx_RelPos.Text = RelPos));
            // Tbx_Frequecy.Invoke(new Action(() => Tbx_Frequecy.Text = Frequecy));
        }


        private delegate void TbxEditUICB(TextBox Ctrl, string Msg);

        private void TbxEdit(TextBox Ctrl, string Msg)
        {
            if (Ctrl.InvokeRequired)
            {
                var d = new TbxEditUICB(TbxEdit);
                Ctrl.Invoke(d, new object[] { Ctrl, Msg });
            }
            else
            {
                Ctrl.Text = Msg;
            }
        }


        private void Btn_SearchHome_Click(object sender, EventArgs e)
        {
            Motor.SearchHome();
        }

        private void Btn_GoHome_Click(object sender, EventArgs e)
        {
            Motor.GoHome();
        }

        private void Btn_TeachOn_Click(object sender, EventArgs e)
        {
            Motor.TeachSwtich(true);
        }

        private void Btn_TeachOff_Click(object sender, EventArgs e)
        {
            Motor.TeachSwtich(false);
        }

        private void Btn_SetSpeed_Click(object sender, EventArgs e)
        {
            int Speed = (int)Num_Speed.Value;
            Motor.Set_Speed(Speed);
        }

        private void Btn_Move_Click(object sender, EventArgs e)
        {
            Button Btn = (Button)sender;

            int Dir = (Btn.Tag.ToString() == "-") ? -1 : 1;

            if (Rbtn_Pos.Checked)
            {
                int Pos = (int)Num_Pos.Value;

                Motor.RelMove_Pos(Pos * Dir);
            }

            if (Rbtn_Angle.Checked)
            {
                int Angle = (int)Num_Angle.Value;
                Motor.RelPos_Angle(Angle * Dir);
            }


        }

        private void Cbx_Read_CheckedChanged(object sender, EventArgs e)
        {
            if (Cbx_Read.Checked)
            {
                if (!Monitor)
                {
                    Monitor = true;
                    Thread MonitorThread = new Thread(new ThreadStart(CheckDevice));
                    MonitorThread.Start();
                }
            }
            else
            {
                Monitor = false;
            }
        }

        private void CheckDevice()
        {
            while (Monitor)
            {
                Motor.CheckDevice();
                Thread.Sleep(2000);
            }
        }

        private void MS5515M_Tool_Load(object sender, EventArgs e)
        {

        }

        private void Btn_Move_Click_1(object sender, EventArgs e)
        {
            int Pos = (int)Num_AbsPos.Value;
            Motor.Move(Pos, -1, -1, -1);
        }

        private void Btn_Read_Click(object sender, EventArgs e)
        {
            Motor.CheckDevice();
        }

        private void Btn_Stop_Click(object sender, EventArgs e)
        {
            Motor.Stop(BaseTool.MotorMember.Focuser);
        }
    }
}
