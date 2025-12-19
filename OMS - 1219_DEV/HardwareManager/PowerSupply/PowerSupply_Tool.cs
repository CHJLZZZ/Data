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
	public partial class PowerSupply_Tool : MaterialForm
	{
		private PowerSupply_Ctrl Ctrl;

		public PowerSupply_Tool(ref PowerSupply_Ctrl PowerSupply)
		{
			InitializeComponent();

			this.Ctrl = PowerSupply;

			this.Ctrl.Update_SendMsg -= Ctrl_Update_SendMsg;
			this.Ctrl.Update_SendMsg += Ctrl_Update_SendMsg;

			this.Ctrl.Update_RecvMsg -= Ctrl_Update_RecvMsg;
			this.Ctrl.Update_RecvMsg += Ctrl_Update_RecvMsg;

			this.Ctrl.Update_Status -= Ctrl_Update_Status;
			this.Ctrl.Update_Status += Ctrl_Update_Status;
		}

		private void Ctrl_Update_Status(string OnOff, double Current)
		{
			this.Invoke(new Action(() =>
			{
				Tbx_Status_OnOff.Text = OnOff;
			}));
		}

		private void Ctrl_Update_RecvMsg(string Msg)
		{
			CrossThread.TbxEdit(Msg, Tbx_RecvMsg);
		}

		private void Ctrl_Update_SendMsg(string Msg)
		{
			CrossThread.TbxEdit(Msg, Tbx_SendMsg);
		}

		private void D65_Light_Tool_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Ctrl.Update_SendMsg -= Ctrl_Update_SendMsg;
			this.Ctrl.Update_RecvMsg -= Ctrl_Update_RecvMsg;
			this.Ctrl.Update_Status -= Ctrl_Update_Status;
		}

		private void Btn_On_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.OUTP_STAT(true);

			if (!Rtn)
			{
				MessageBox.Show($"Set Output ON Fail");
			}
		}

		private void Btn_Off_Click(object sender, EventArgs e)
		{

			bool Rtn = Ctrl.OUTP_STAT(false);

			if (!Rtn)
			{
				MessageBox.Show($"Set Output OFF Fail");
			}
		}
				
		private bool isBusy = false;

		private void Tmr_Query_Tick(object sender, EventArgs e)
		{
			if (isBusy) return;
			isBusy = true;

			Task.Run(() =>
			{
				try
				{
					bool Rtn = Ctrl.IsConnect;

					if (Rtn && !AutoSet_Run)
					{
						Ctrl.CURR_QueryImmediate();
					}
				}
				catch (Exception ex)
				{

				}
				finally
				{
					isBusy = false;
				}
			});
		}

		private bool AutoSet_Run = false;


		private void Btn_AutoSet_Start_Click(object sender, EventArgs e)
		{
			Flow(true);
		
		}

		private void Flow(bool Forward)
		{
			double Current_Start = (double)Num_Current_Start.Value;
			double Current_End = (double)Num_Current_End.Value;
			double CostTime = (double)Num_CostTime.Value;

			AutoSet_Run = true;

			double GapTime = (CostTime * 1000) / (Math.Abs(Current_End - Current_Start) * 1000);
			double Current = Current_Start;
			double Current_Final = Current_End;

			if (Forward)
			{
				Current = Current_Start;
				Current_Final = Current_End;
			}
			else
			{
				Current = Current_End;
				Current_Final = Current_Start;

			}

			new Thread(() =>
			{
				while (AutoSet_Run)
				{
					if (Forward)
					{
						if (Current >= Current_Final) Current = Current_Final;
					}
					else
					{
						if (Current <= Current_Final) Current = Current_Final;
					}

					bool Rtn = Ctrl.CURR_SetImmediate(Current);

					if (Rtn)
					{
						Ctrl.SaveLog($"Set Current : {Current}");

						if (Forward)
						{
							Current += 0.001;
							Current = Math.Round(Current, 3);
							if (Current > Current_Final) break;
						}
						else
						{
							Current -= 0.001;
							Current = Math.Round(Current, 3);
							if (Current < Current_Final) break;
						}										

						Thread.Sleep((int)GapTime);

					}
					else
					{

					}
				}
			}).Start();
		}

		private void Btn_AutoSet_Stop_Click(object sender, EventArgs e)
		{
			AutoSet_Run = false;
		}

		private void Btn_RCL_Click(object sender, EventArgs e)
		{		
			Ctrl.IEEE_RCL(1);

			//if (!Rtn)
			//{
			//    MessageBox.Show($"Set CH{Channel} Current : {Current} Fail");
			//}
		}

		private void Btn_Local_Click(object sender, EventArgs e)
		{
			Ctrl.LocalMode();
		}

		private void Btn_Remote_Click(object sender, EventArgs e)
		{
			Ctrl.RemoteMode();

		}

		private void Btn_AutoSet_ReverseStart_Click(object sender, EventArgs e)
		{
			Flow(false);
		}

		private void Btn_Send_Click(object sender, EventArgs e)
		{
            
			Ctrl.SendCMD(Tbx_CMD.Text);
		}
	}
}
