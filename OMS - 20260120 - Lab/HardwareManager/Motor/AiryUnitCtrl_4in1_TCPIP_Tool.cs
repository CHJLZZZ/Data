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

    public partial class AiryUnitCtrl_4in1_TCPIP_Tool : MaterialForm
    {
        private AiryUnitCtrl_4in1_TCPIP Motor = null;
        private bool CloseFlag = false;
        public AiryUnitCtrl_4in1_TCPIP_Tool(ref AiryUnitCtrl_4in1_TCPIP Motor)
        {
            InitializeComponent();

            this.Motor = Motor;
            this.Motor.UpdateStatus -= Motor_UpdateStatus;
            this.Motor.UpdateStatus += Motor_UpdateStatus;

            if (this.Motor != null)
            {
                Thread Check = new Thread(() =>
                {
                    while(true)
                    {
                        Thread.Sleep(100);
                        if(!this.Motor.IsMoveProc())
                        {
                            bool Rtn = this.Motor.CheckDevice();
                            if (!Rtn) break;
                        }
                   

                        if(CloseFlag) break;

                    }
                });

                Check.Start();
            }
            
        }

        private void Motor_UpdateStatus(MotorInfo Focuser, MotorInfo Aperture, MotorInfo FW1, MotorInfo FW2)
        {
            this.Invoke(new Action(() =>
            {
                if (DGV_Info.Rows.Count != 4)
                {
                    DGV_Info.RowCount = 4;
                }

                MotorInfo[] UnitInfo = { Focuser, Aperture, FW1, FW2 };
                string[] Name = { "Focuser", "Aperture", "FW1", "FW2" };


                for (int i = 0; i < 4; i++)
                {
                    DGV_Info.Rows[i].Cells[0].Value = Name[i];
                    DGV_Info.Rows[i].Cells[1].Value = UnitInfo[i].Position;
                    DGV_Info.Rows[i].Cells[2].Value = UnitInfo[i].Speed;

                    string State = $"{UnitInfo[i].PinHomeState} / {UnitInfo[i].HomeState} / {UnitInfo[i].OverrideState}";
                    DGV_Info.Rows[i].Cells[3].Value = State;
                }

                Tbx_FocuserLimit.Text = Focuser.Limit.ToString();
                Tbx_ApertureLimit.Text = Aperture.Limit.ToString();
            }));


        }

        private void Btn_All_Home_Click(object sender, EventArgs e)
        {
            this.Motor.Home();
        }

        private void Btn_LED_On_Click(object sender, EventArgs e)
        {
            this.Motor.OpenLed();
        }

        private void Btn_LED_Off_Click(object sender, EventArgs e)
        {
            this.Motor.CloseLed();
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
            int FW1Pos = -1;
            int FW2Pos = -1;

            if (Cbx_Focuser_FlowEable.Checked)
            {
                FocuserPos = (int)Num_Focuser_FlowPos.Value;
            }

            if (Cbx_Aperture_FlowEable.Checked)
            {
                AperturePos = (int)Num_Aperture_FlowPos.Value;
            }

            if (Cbx_FW1_FlowEable.Checked)
            {
                FW1Pos = (int)Num_FW1_FlowPos.Value;
            }

            if (Cbx_FW2_FlowEable.Checked)
            {
                FW2Pos = (int)Num_FW2_FlowPos.Value;
            }

            this.Motor.Move(FocuserPos, AperturePos, FW1Pos, FW2Pos);
        }

        private void AiryUnitCtrl_4in1_TCPIP_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Motor.UpdateStatus -= Motor_UpdateStatus;
            CloseFlag = true;
        }

        private void Btn_MoveFlow_Stop_Click(object sender, EventArgs e)
        {

        }
    }
}
