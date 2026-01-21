using CommonSettings;
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

	public partial class CircleTacCtrl_Tool : MaterialForm
	{
		private CircleTacCtrl Motor = null;
		private int RetryCnt = 0;
		public CircleTacCtrl_Tool(ref CircleTacCtrl Motor)
		{
			InitializeComponent();

			this.Motor = Motor;
			this.Motor.UpdateInfo -= Motor_UpdateInfo;
			this.Motor.UpdateInfo += Motor_UpdateInfo;

			RefreshSettingInfo();

			groupBox7.Visible = !GlobalConfig.IsSecretHidden;
			//LoopCheck();
		}

		private void RefreshSettingInfo()
		{
			string[] FocuserInfo = new string[]
			{
				"Focuser",
				Motor.Focuser_Setting.HomeSpeed.ToString(),
				Motor.Focuser_Setting.RunSpeed.ToString(),
				Motor.Focuser_Setting.ZeroPos.ToString(),
				Motor.Focuser_Setting.Limit.ToString(),
				Motor.Focuser_Setting.Gap.ToString(),
				Motor.Focuser_Setting.CurrentLimit.ToString(),
			};

			string[] ApertureInfo = new string[]
			{
				"Aperture",
				Motor.Aperture_Setting.HomeSpeed.ToString(),
				Motor.Aperture_Setting.RunSpeed.ToString(),
				Motor.Aperture_Setting.ZeroPos.ToString(),
				Motor.Aperture_Setting.Limit.ToString(),
				Motor.Aperture_Setting.Gap.ToString(),
				Motor.Aperture_Setting.CurrentLimit.ToString(),
			};

			string[] FW1Info = new string[]
			{
				"FW1",
				Motor.FW1_Setting.HomeSpeed.ToString(),
				Motor.FW1_Setting.RunSpeed.ToString(),
				Motor.FW1_Setting.ZeroPos.ToString(),
				Motor.FW1_Setting.Limit.ToString(),
				Motor.FW1_Setting.Gap.ToString(),
				Motor.FW1_Setting.CurrentLimit.ToString(),
			};


			string[] FW2Info = new string[]
			{
				"FW2",
				Motor.FW2_Setting.HomeSpeed.ToString(),
				Motor.FW2_Setting.RunSpeed.ToString(),
				Motor.FW2_Setting.ZeroPos.ToString(),
				Motor.FW2_Setting.Limit.ToString(),
				Motor.FW2_Setting.Gap.ToString(),
				Motor.FW2_Setting.CurrentLimit.ToString(),
			};

			DGV_Setting.Rows.Clear();
			DGV_Setting.Rows.Add(FocuserInfo);
			DGV_Setting.Rows.Add(ApertureInfo);
			DGV_Setting.Rows.Add(FW1Info);
			DGV_Setting.Rows.Add(FW2Info);
		}

		private void LoopCheck()
		{
			if (this.Motor != null)
			{
				if (!this.Motor.IsMoveProc())
				{
					bool Rtn = this.Motor.ReadInfo();

					if (!Rtn)
					{
						RetryCnt++;
					}
					else
					{
						RetryCnt = 0;
					}

					if (RetryCnt > 10)
					{
						timer1.Stop();
					}
				}
			}
		}

		private void Motor_UpdateInfo(CircleTacCtrl.MotorStatus Focuser, CircleTacCtrl.MotorStatus Aperture, CircleTacCtrl.MotorStatus FW1, CircleTacCtrl.MotorStatus FW2)
		{
			this.Invoke(new Action(() =>
			{
				Tbx_Focuser_Pos.Text = Focuser.Pos.ToString();
				Label_Focuser_Homed.BackColor = (Focuser.Homed) ? Color.Green : Color.Gray;
				Label_Focuser_Sensor.BackColor = (Focuser.InSensor) ? Color.Green : Color.Gray;
				Label_Focuser_Moving.BackColor = (Focuser.Moving) ? Color.Green : Color.Gray;

				Tbx_Aperture_Pos.Text = Aperture.Pos.ToString();
				Label_Aperture_Homed.BackColor = (Aperture.Homed) ? Color.Green : Color.Gray;
				Label_Aperture_Sensor.BackColor = (Aperture.InSensor) ? Color.Green : Color.Gray;
				Label_Aperture_Moving.BackColor = (Aperture.Moving) ? Color.Green : Color.Gray;

				int FW1_Pos = FW1.Pos / -30720;

				switch (FW1_Pos)
				{
					case 0: Tbx_FW1_Pos.Text = $"X ({FW1.Pos})"; break;
					case 1: Tbx_FW1_Pos.Text = $"Y ({FW1.Pos})"; break;
					case 2: Tbx_FW1_Pos.Text = $"Z ({FW1.Pos})"; break;
				}

				Label_FW1_Homed.BackColor = (FW1.Homed) ? Color.Green : Color.Gray;
				Label_FW1_Sensor.BackColor = (FW1.InSensor) ? Color.Green : Color.Gray;
				Label_FW1_Moving.BackColor = (FW1.Moving) ? Color.Green : Color.Gray;

				int FW2_Pos = FW2.Pos / -30720;

				switch (FW2_Pos)
				{
					case 0: Tbx_FW2_Pos.Text = $"12.5% ({FW2.Pos})"; break;
					case 1: Tbx_FW2_Pos.Text = $"1.56% ({FW2.Pos})"; break;
					case 2: Tbx_FW2_Pos.Text = $"0.25% ({FW2.Pos})"; break;
					case 3: Tbx_FW2_Pos.Text = $"100% ({FW2.Pos})"; break;
				}

				Label_FW2_Homed.BackColor = (FW2.Homed) ? Color.Green : Color.Gray;
				Label_FW2_Sensor.BackColor = (FW2.InSensor) ? Color.Green : Color.Gray;
				Label_FW2_Moving.BackColor = (FW2.Moving) ? Color.Green : Color.Gray;
			}));
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

		private void Btn_Focuser_Home_Click(object sender, EventArgs e)
		{
			try
			{
				bool Rtn = this.Motor.Home(BaseTool.MotorMember.Focuser);

				if (!Rtn)
				{
					MessageBox.Show("Home Fail");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Btn_Aperture_Home_Click(object sender, EventArgs e)
		{
			try
			{
				bool Rtn = this.Motor.Home(BaseTool.MotorMember.Aperture);

				if (!Rtn)
				{
					MessageBox.Show("Home Fail");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Btn_FW1_Home_Click(object sender, EventArgs e)
		{
			try
			{
				bool Rtn = this.Motor.Home(BaseTool.MotorMember.FW1);

				if (!Rtn)
				{
					MessageBox.Show("Home Fail");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Btn_FW2_Home_Click(object sender, EventArgs e)
		{
			try
			{
				bool Rtn = this.Motor.Home(BaseTool.MotorMember.FW2);

				if (!Rtn)
				{
					MessageBox.Show("Home Fail");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			LoopCheck();

		}

		private void Btn_AllHome_Click(object sender, EventArgs e)
		{
			try
			{
				bool Rtn = true;
				string ErrorMsg = "";

				for (int i = 0; i < 4; i++)
				{
					Rtn &= this.Motor.Home((BaseTool.MotorMember)i);
					Thread.Sleep(100);
					ErrorMsg += $"{(BaseTool.MotorMember)i} Home Fail \r\n";
				}

				if (!Rtn)
				{
					MessageBox.Show(ErrorMsg);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
