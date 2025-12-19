using BaseTool;
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
    public partial class AuoMotor_Tool : MaterialForm
    {
        private bool Monitor = false;
        private AuoMotorCtrl Motor;

        public AuoMotor_Tool(ref AuoMotorCtrl Motor)
        {
            InitializeComponent();
            this.Motor = Motor;
        }

        private void Btn_Enable_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            Motor.SetEnable(Member, true);
        }

        private void Btn_Disable_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            Motor.SetEnable(Member, false);
        }

        private void bntSearchHome_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            int Speed = (int)Num_HomeSpeed.Value;
            Motor.Set_HomeSpeed(No, Speed);

            int Timeout = (int)Num_HomeTimeout.Value;
            Motor.Home(Member, Timeout);
        }

        private void Btn_SetBacklash_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Gap = (int)Num_SetBacklash.Value;
            Motor.SetBacklash(No, Gap);
        }

        private void Btn_GetBacklash_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Gap = Motor.GetBacklash(No);

            Tbx_GetBacklash.Invoke(new Action(() => Tbx_GetBacklash.Text = $"{Gap}"));
        }

        private void Btn_AbsMove_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            int Pos = (int)Num_AbsPos.Value;

            int Speed = (int)Num_MoveSpeed.Value;

            Motor.Set_MoveSpeed(No, Speed);

            int Timeout = (int)Num_MoveTimeout.Value;

            Motor.AbsMove(Member, Pos, Timeout);
        }

        private void Btn_RelMove_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            int Pos = (int)Num_RelPos.Value;

            int Speed = (int)Num_MoveSpeed.Value;

            Motor.Set_MoveSpeed(No, Speed);

            int Timeout = (int)Num_MoveTimeout.Value;

            Motor.RelMove(Member, Pos, Timeout);
        }

        private void Btn_GoHome_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;
            MotorMember Member = (No == 0) ? MotorMember.Focuser : MotorMember.Aperture;

            int Speed = (int)Num_MoveSpeed.Value;

            Motor.Set_MoveSpeed(No, Speed);

            int Timeout = (int)Num_MoveSpeed.Value;

            Motor.GoHome(Member, Timeout);
        }

        private void Btn_ReadStatus_Click(object sender, EventArgs e)
        {
            int No = 0;

            Cbx_MotorIdx.Invoke(new Action(() => { No = Cbx_MotorIdx.SelectedIndex; }));

            CheckStatus();
        }

        public void CheckStatus()
        {
            int No = 0;

            Cbx_MotorIdx.Invoke(new Action(() => { No = Cbx_MotorIdx.SelectedIndex; }));

            AuoMotorInfo Info = Motor.GetStatus(No);

            PbxEdit(Pbx_Enable, Info.Enable);
            PbxEdit(Pbx_Home, Info.Home);
            PbxEdit(Pbx_Limit, Info.Limit);
            PbxEdit(Pbx_Moving, Info.Moving);

            //this.Invoke(new Action(() =>
            //{
            //    Pbx_Enable.BackColor = (Info.Enable) ? Color.Green : Color.Gray;
            //}));

            //Pbx_Home.Invoke(new Action(() =>
            //{
            //    Pbx_Home.BackColor = (Info.Home) ? Color.Green : Color.Gray;
            //}));

            //Pbx_Limit.Invoke(new Action(() =>
            //{
            //    Pbx_Limit.BackColor = (Info.Limit) ? Color.Green : Color.Gray;
            //}));

            //Pbx_Moving.Invoke(new Action(() =>
            //{
            //    Pbx_Moving.BackColor = (Info.Moving) ? Color.Green : Color.Gray;
            //}));
            TbxEdit(Tbx_CurrentPos, $"{Info.Position}");
            TbxEdit(Tbx_TargetPos, $"{Info.Target}");
            TbxEdit(Tbx_Speed, $"{Info.Speed}");

            //Tbx_CurrentPos.Invoke(new Action(() =>
            //{
            //    Tbx_CurrentPos.Text = $"{Info.Position}";
            //}));

            //Tbx_TargetPos.Invoke(new Action(() =>
            //{
            //    Tbx_TargetPos.Text = $"{Info.Target}";
            //}));

            //Tbx_Speed.Invoke(new Action(() =>
            //{
            //    Tbx_Speed.Text = $"{Info.Speed}";
            //}));
        }

        private delegate void PbxEditUICB(PictureBox Ctrl, bool IsOn);

        private void PbxEdit(PictureBox Ctrl,bool IsOn)
        {
            if (Ctrl.InvokeRequired)
            {
                var d = new PbxEditUICB(PbxEdit);
                Ctrl.Invoke(d, new object[] { Ctrl,IsOn });
            }
            else
            {
                Ctrl.BackColor = (IsOn) ? Color.Green : Color.Gray;
            }
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
                CheckStatus();
                //Thread.Sleep(10);
            }
        }

        private void Btn_SetLimitF_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Limit = (int)Num_SetLimitF.Value;
            Motor.SetLimitF(No, Limit);
        }

        private void Btn_SetLimitR_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Limit = (int)Num_SetLimitR.Value;
            Motor.SetLimitR(No, Limit);
        }

        private void Btn_GetLimitF_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Limit = Motor.GetLimitF(No);

            Tbx_GetLimitF.Invoke(new Action(() => Tbx_GetLimitF.Text = $"{Limit}"));
        }

        private void Btn_GetLimitR_Click(object sender, EventArgs e)
        {
            int No = Cbx_MotorIdx.SelectedIndex;

            int Limit = Motor.GetLimitR(No);

            Tbx_GetLimitR.Invoke(new Action(() => Tbx_GetLimitR.Text = $"{Limit}"));
        }

        private void AuoMotor_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            Monitor = false;

        }
    }
}
