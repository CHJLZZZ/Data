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

    public partial class CircleTacCtrl_Tool_Old : MaterialForm
    {
        private CircleTacCtrl_Old Motor = null;


        public CircleTacCtrl_Tool_Old(ref CircleTacCtrl_Old Motor)
        {
            InitializeComponent();

            this.Motor = Motor;
            this.Motor.UpdateInfo -= Motor_UpdateInfo;
            this.Motor.UpdateInfo += Motor_UpdateInfo;


             LoopCheck();
        }

        private void LoopCheck()
        {
            if (this.Motor != null)
            {
                Thread Check = new Thread(() =>
                {
                    //while (true)
                    {
                        Thread.Sleep(100);

                        if (!this.Motor.IsMoveProc())
                        {
                            this.Motor.GetPosition(BaseTool.MotorMember.Focuser);
                            Thread.Sleep(30);
                            this.Motor.GetPosition(BaseTool.MotorMember.Aperture);
                            Thread.Sleep(30);
                            this.Motor.GetPosition(BaseTool.MotorMember.FW1);
                            Thread.Sleep(30);
                            this.Motor.GetPosition(BaseTool.MotorMember.FW2);
                            Thread.Sleep(30);
                            this.Motor.GetTargetPosition(BaseTool.MotorMember.Focuser);
                            Thread.Sleep(30);
                            this.Motor.GetTargetPosition(BaseTool.MotorMember.Aperture);
                            Thread.Sleep(30);
                            this.Motor.GetTargetPosition(BaseTool.MotorMember.FW1);
                            Thread.Sleep(30);
                            this.Motor.GetTargetPosition(BaseTool.MotorMember.FW2);
                            Thread.Sleep(30);
                            this.Motor.GetHomeState(BaseTool.MotorMember.Focuser);
                            Thread.Sleep(30);
                            this.Motor.GetHomeState(BaseTool.MotorMember.Aperture);
                            Thread.Sleep(30);
                            this.Motor.GetWorkCurrent(BaseTool.MotorMember.Focuser);
                            Thread.Sleep(30);
                            this.Motor.GetWorkCurrent(BaseTool.MotorMember.Aperture);
                            Thread.Sleep(30);
                        }


                        //if (CloseFlag) break;

                    }
                });

                Check.Start();
            }
        }

        private void Motor_UpdateInfo(CircleTacCtrl_Old.HardwareInfo Focuser, CircleTacCtrl_Old.HardwareInfo Aperture, CircleTacCtrl_Old.HardwareInfo FW1, CircleTacCtrl_Old.HardwareInfo FW2)
        {
            this.Invoke(new Action(() =>
            {
                Tbx_FocuserNowPos.Text = Focuser.NowPos.ToString();
                Tbx_FocuserTargetPos.Text = Focuser.TargetPos.ToString();
                Tbx_FocuserHomed.Text = Focuser.HomeState.ToString();
                Tbx_FocuserCurrent.Text = Focuser.WorkCurrent.ToString();
                Tbx_FocuserLimit.Text = Focuser.Limit.ToString();

                Tbx_ApertureNowPos.Text = Aperture.NowPos.ToString();
                Tbx_ApertureTargetPos.Text = Aperture.TargetPos.ToString();
                Tbx_ApertureHomed.Text = Aperture.HomeState.ToString();
                Tbx_ApertureCurrent.Text = Aperture.WorkCurrent.ToString();
                Tbx_ApertureLimit.Text = Aperture.Limit.ToString();

                Tbx_FW1NowPos.Text = FW1.NowPos.ToString();
                Tbx_FW1TargetPos.Text = FW1.TargetPos.ToString();

                Tbx_FW2NowPos.Text = FW2.NowPos.ToString();
                Tbx_FW2TargetPos.Text = FW2.TargetPos.ToString();
            }));

        }


        private void Btn_Set_Backlash_Click(object sender, EventArgs e)
        {
            int Backlash = (int)Num_Backlash.Value;
            this.Motor.SetBacklash(Backlash);
        }

        private void Btn_MoveFlow_Start_Click(object sender, EventArgs e)
        {
            int FocuserPos = -1;
            int AperturePos = -1;
            int FilterXyzPos = -1;
            int FilterNdPos = -1;

            if (Cbx_Focuser_FlowEable.Checked)
            {
                FocuserPos = (int)Num_Focuser_FlowPos.Value;
            }

            if (Cbx_Aperture_FlowEable.Checked)
            {
                AperturePos = (int)Num_Aperture_FlowPos.Value;
            }

            if (Cbx_FilterXyz_FlowEable.Checked)
            {
                FilterXyzPos = (int)Num_FilterXyz_FlowPos.Value;
            }

            if (Cbx_FilterNd_FlowEable.Checked)
            {
                FilterNdPos = (int)Num_FilterNd_FlowPos.Value;
            }

            this.Motor.Move(FocuserPos, AperturePos, FilterXyzPos, FilterNdPos);
        }

        private void AiryUnitCtrl_4in1_TCPIP_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Motor.UpdateInfo -= Motor_UpdateInfo;
        }

        private void Btn_MoveFlow_Stop_Click(object sender, EventArgs e)
        {
            this.Motor.Stop();
        }
           
        private void Btn_FocuserMove_Click(object sender, EventArgs e)
        {
            try
            {
                int Pos = Convert.ToInt32(Tbx_FocuserMovePos.Text);
                bool Rtn = this.Motor.DirectMove(BaseTool.MotorMember.Focuser, Pos);

                if (!Rtn)
                {
                    MessageBox.Show("Move Fail");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_ApertureMove_Click(object sender, EventArgs e)
        {
            try
            {
                int Pos = Convert.ToInt32(Tbx_ApertureMovePos.Text);
                bool Rtn = this.Motor.DirectMove(BaseTool.MotorMember.Aperture, Pos);

                if (!Rtn)
                {
                    MessageBox.Show("Move Fail");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW1Move_Click(object sender, EventArgs e)
        {
            try
            {
                int Pos = Convert.ToInt32(Tbx_FW1MovePos.Text);
                bool Rtn = this.Motor.DirectMove(BaseTool.MotorMember.FW1, Pos);

                if (!Rtn)
                {
                    MessageBox.Show("Move Fail");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW2Move_Click(object sender, EventArgs e)
        {
            try
            {
                int Pos = Convert.ToInt32(Tbx_FW2MovePos.Text);
                bool Rtn = this.Motor.DirectMove(BaseTool.MotorMember.FW2, Pos);

                if (!Rtn)
                {
                    MessageBox.Show("Move Fail");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Focuser_GetNowPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetPosition(BaseTool.MotorMember.Focuser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Aperture_GetNowPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetPosition(BaseTool.MotorMember.Aperture);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW1_GetNowPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetPosition(BaseTool.MotorMember.FW1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW2_GetNowPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetPosition(BaseTool.MotorMember.FW2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Focuser_GetTargetPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetTargetPosition(BaseTool.MotorMember.Focuser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Aperture_GetTargetPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetTargetPosition(BaseTool.MotorMember.Aperture);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW1_GetTargetPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetTargetPosition(BaseTool.MotorMember.FW1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_FW2_GetTargetPos_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetTargetPosition(BaseTool.MotorMember.FW2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Focuser_GetHome_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetHomeState(BaseTool.MotorMember.Focuser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Aperture_GetHomed_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetHomeState(BaseTool.MotorMember.Aperture);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Focuser_GetCurrent_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetWorkCurrent(BaseTool.MotorMember.Focuser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Aperture_GetCurrent_Click(object sender, EventArgs e)
        {
            try
            {
                this.Motor.GetWorkCurrent(BaseTool.MotorMember.Aperture);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
