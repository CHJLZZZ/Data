using BaseTool;
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

namespace HardwareManager
{
	public partial class CornerstoneB_Tool : MaterialForm
	{
		private CornerstoneB_Ctrl Ctrl = null;

		public CornerstoneB_Tool(ref CornerstoneB_Ctrl CornerstoneB)
		{
			InitializeComponent();

			this.Ctrl = CornerstoneB;

			this.Ctrl.UPDATE_STATUS -= Ctrl_UPDATE_STATUS;
			this.Ctrl.UPDATE_STATUS += Ctrl_UPDATE_STATUS;

		}

		private void Ctrl_UPDATE_STATUS(bool Shutter_OnOff, string Grating_Msg, int Filter_Idx, double WaveLength)
		{
			this.Invoke(new Action(() =>
			{
				Cbx_GET_SHUTTER.Checked = Shutter_OnOff;
				Tbx_GET_GRATing.Text = Grating_Msg;
				Num_GET_WAVE.Value = (decimal)WaveLength;
				Num_GET_Filter.Value = Filter_Idx;
			}));
		}

		private void SR3_Tool_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Ctrl.UPDATE_STATUS -= Ctrl_UPDATE_STATUS;
		}

		private void Btn_SET_BANDPASS_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_BANDPASS.Value;
			bool Rtn = Ctrl.SET_BANDPASS(Value);
			if (!Rtn) MessageBox.Show("SET BANDPASS Fail");
		}

		private void Btn_GET_BANDPASS_Click(object sender, EventArgs e)
		{
			double Value = 0;
			bool Rtn = Ctrl.GET_BANDPASS(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET BANDPASS Fail");
			}
			else
			{
				MessageBox.Show($"GET BANDPASS : {Value}");
			}
		}

		private void Btn_SET_CALIBRATE_Click(object sender, EventArgs e)
		{
			double Value = (double)Num_CALIBRATE.Value;
			bool Rtn = Ctrl.SET_CALIBRATE(Value);
			if (!Rtn) MessageBox.Show("SET CALIBRATE Fail");
		}

		private void Btn_SET_ECHO_Click(object sender, EventArgs e)
		{
			bool Value = Cbx_ECHO.Checked;
			bool Rtn = Ctrl.SET_ECHO(Value);
			if (!Rtn) MessageBox.Show("SET ECHO Fail");
		}

		private void Btn_GET_ECHO_Click(object sender, EventArgs e)
		{
			bool Value = false;
			bool Rtn = Ctrl.GET_ECHO(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET ECHO Fail");
			}
			else
			{
				MessageBox.Show($"GET ECHO : {Value}");
			}
		}

		private void Btn_GET_ERROR_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_ERROR(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET ERROR Fail");
			}
			else
			{
				MessageBox.Show($"GET ERROR : {Value}");
			}
		}

		private void Btn_SET_FILTER_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_Filter.Value;
			bool Rtn = Ctrl.SET_FILTER(Value);
			if (!Rtn) MessageBox.Show("SET FILTER Fail");
		}

		private void Btn_Get_Filter_Click(object sender, EventArgs e)
		{
			int Value = 0;
			bool Rtn = Ctrl.GET_FILTER(ref Value);

			if (!Rtn)
			{
				MessageBox.Show("GET FILTER Fail");
			}
			else
			{
				Num_GET_Filter.Value = Value;
			}
		}

		private void Btn_GET_FILTERSET_Click(object sender, EventArgs e)
		{
			int Value = 0;
			bool Rtn = Ctrl.GET_FILTER_SET(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET FILTER SET Fail");
			}
			else
			{
				MessageBox.Show($"GET FILTER SET : {Value}");
			}
		}

		private void Btn_SET_FILTERLABEL_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_FILERLABEL_Index.Value;
			string Label = Tbx_FILTERLABEL_Label.Text;
			bool Rtn = Ctrl.SET_FILTERLABEL(Index, Label);
			if (!Rtn) MessageBox.Show("SET FILTER LABEL Fail");
		}

		private void Btn_GET_FILTERLABEL_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_FILERLABEL_Index.Value;
			string Label = "";
			bool Rtn = Ctrl.GET_FILTERLABEL(Index, ref Label);
			if (!Rtn)
			{
				MessageBox.Show("GET FILTER LABEL Fail");
			}
			else
			{
				MessageBox.Show($"GET FILTER LABEL : {Label}");
			}
		}

		private void Btn_FINDHOME_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.FIND_HOME();
			if (!Rtn) MessageBox.Show("FIND HOME Fail");
		}

		private void Btn_SET_GOWAVE_Click(object sender, EventArgs e)
		{
			double Value = (double)Num_GOWAVE.Value;
			bool Rtn = Ctrl.SET_GOWAVE(Value);
			if (!Rtn) MessageBox.Show("SET GO WAVE Fail");
		}

		private void Btn_GET_GOWAVE_Click(object sender, EventArgs e)
		{
			double Value = 0;
			bool Rtn = Ctrl.GET_GOWAVE(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET GO WAVE Fail");
			}
			else
			{
				MessageBox.Show($"GET GO WAVE  : {Value}");
			}
		}

		private void Btn_SET_GRATing_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_GRATing.Value;
			bool Rtn = Ctrl.SET_GRATing(Value);
			if (!Rtn) MessageBox.Show("SET GRATing Fail");
		}

		private void Btn_GET_GRATing_Click(object sender, EventArgs e)
		{
			string Msg = "";
			bool Rtn = Ctrl.GET_GRATing(ref Msg);
			if (!Rtn)
			{
				MessageBox.Show("GET GRATing Fail");
			}
			else
			{
				Tbx_GET_GRATing.Text = Msg;
			}
		}

		private void Btn_SET_GRATFACTOR_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			double Factor = (double)Num_GRAT_Factor.Value;
			bool Rtn = Ctrl.SET_GRATFACTOR(Index, Factor);
			if (!Rtn) MessageBox.Show("SET GRAT Factor Fail");
		}

		private void Btn_GET_GRATFACTOR_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			double Factor = 0;
			bool Rtn = Ctrl.GET_GRATFACTOR(Index, ref Factor);
			if (!Rtn)
			{
				MessageBox.Show("GET GRAT FACTOR Fail");
			}
			else
			{
				MessageBox.Show($"GET GRAT FACTOR : {Factor}");
			}
		}

		private void Btn_SET_GRATLABEL_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			string Label = Tbx_GRAT_Label.Text;
			bool Rtn = Ctrl.SET_GRATLABEL(Index, Label);
			if (!Rtn) MessageBox.Show("SET GRAT LABEL Fail");
		}

		private void Btn_GET_GRATLABEL_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			string Label = "";
			bool Rtn = Ctrl.GET_GRATLABEL(Index, ref Label);
			if (!Rtn)
			{
				MessageBox.Show("GET GRAT LABEL Fail");
			}
			else
			{
				MessageBox.Show($"GET GRAT LABEL : {Label}");
			}
		}

		private void Btn_SET_GRATLINES_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			int Lines = (int)Num_GRAT_Lines.Value;
			bool Rtn = Ctrl.SET_GRATLINES(Index, Lines);
			if (!Rtn) MessageBox.Show("SET GRAT LINES Fail");
		}

		private void Btn_GET_GRATLINES_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			int Lines = 0;
			bool Rtn = Ctrl.GET_GRATLINES(Index, ref Lines);
			if (!Rtn)
			{
				MessageBox.Show("GET GRAT LINES Fail");
			}
			else
			{
				MessageBox.Show($"GET GRAT LINES : {Lines}");
			}
		}

		private void Btn_SET_GRATOFFSET_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			double Offset = (double)Num_GRAT_Offset.Value;
			bool Rtn = Ctrl.SET_GRATOFFSET(Index, Offset);
			if (!Rtn) MessageBox.Show("SET GRAT OFFSET Fail");
		}

		private void Btn_GET_GRATOFFSET_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_GRAT_Index.Value;
			double Offset = 0;
			bool Rtn = Ctrl.GET_GRATOFFSET(Index, ref Offset);
			if (!Rtn)
			{
				MessageBox.Show("GET GRAT OFFSET Fail");
			}
			else
			{
				MessageBox.Show($"GET GRAT OFFSET : {Offset}");
			}
		}

		private void Btn_SET_HANDSHAKE_Click(object sender, EventArgs e)
		{
			bool Value = Cbx_HANDSHAKE.Checked;
			bool Rtn = Ctrl.SET_HANDSHAKE(Value);
			if (!Rtn) MessageBox.Show("SET HANDSHAKE Fail");
		}

		private void Btn_GET_HANDSHAKE_Click(object sender, EventArgs e)
		{
			bool Value = false;
			bool Rtn = Ctrl.GET_HANDSHAKE(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET HANDSHAKE Fail");
			}
			else
			{
				MessageBox.Show($"GET HANDSHAKE : {Value}");
			}
		}

		private void Btn_GET_IDLE_Click(object sender, EventArgs e)
		{
			bool Value = false;
			bool Rtn = Ctrl.GET_IDLE(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET IDLE Fail");
			}
			else
			{
				MessageBox.Show($"GET IDLE : {Value}");
			}
		}

		private void Btn_GET_INFO_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_INFO(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET INFO Fail");
			}
			else
			{
				MessageBox.Show($"GET INFO : {Value}");
			}
		}

		private void Btn_SET_OUTPORT_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_OUTPORT.Value;
			bool Rtn = Ctrl.SET_OUTPORT(Value);
			if (!Rtn) MessageBox.Show("SET OUTPORT Fail");
		}

		private void Btn_GET_OUTPORT_Click(object sender, EventArgs e)
		{
			int Value = 0;
			bool Rtn = Ctrl.GET_OUTPORT(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET OUTPORT Fail");
			}
			else
			{
				MessageBox.Show($"GET OUTPORT : {Value}");
			}
		}

		private void Btn_SET_SHUTTER_Click(object sender, EventArgs e)
		{
			bool Value = Cbx_SHUTTER.Checked;
			bool Rtn = Ctrl.SET_SHUTTER(Value);
			if (!Rtn) MessageBox.Show("SET SHUTTER Fail");
		}

		private void Btn_GET_SHUTTER_Click(object sender, EventArgs e)
		{
			bool Value = false;
			bool Rtn = Ctrl.GET_SHUTTER(ref Value);

			if (!Rtn)
			{
				MessageBox.Show("GET SHUTTER Fail");
			}
			else
			{
				Cbx_GET_SHUTTER.Checked = Value;
			}
		}

		private void Btn_SET_SLITWIDTH_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_SLIT_Index.Value;
			int Width = (int)Num_SLIT_Width.Value;
			bool Rtn = Ctrl.SET_SLITWIDTH(Index, Width);
			if (!Rtn) MessageBox.Show("SET SLIT WIDTH Fail");
		}

		private void Btn_GET_SLITWIDTH_Click(object sender, EventArgs e)
		{
			int Index = (int)Num_SLIT_Index.Value;
			int Width = 0;
			bool Rtn = Ctrl.GET_SLITWIDTH(Index, ref Width);
			if (!Rtn)
			{
				MessageBox.Show("GET SLIT WIDTH Fail");
			}
			else
			{
				MessageBox.Show($"GET SLIT WIDTH : {Width}");
			}
		}

		private void Btn_GET_STB_Click(object sender, EventArgs e)
		{
			int Value = 0;
			bool Rtn = Ctrl.GET_STB(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET STB Fail");
			}
			else
			{
				MessageBox.Show($"GET STB : {Value}");
			}
		}

		private void Btn_SET_STEP_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_STEP.Value;
			bool Rtn = Ctrl.SET_STEP(Value);
			if (!Rtn) MessageBox.Show("SET STEP Fail");
		}

		private void Btn_SET_SLOWSTEP_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_STEP.Value;
			bool Rtn = Ctrl.SET_SLOWSTEP(Value);
			if (!Rtn) MessageBox.Show("SET SLOW STEP Fail");
		}

		private void Btn_SET_STEPSIZE_Click(object sender, EventArgs e)
		{
			int Value = (int)Num_STEPSIZE.Value;
			bool Rtn = Ctrl.SET_STEPSIZE(Value);
			if (!Rtn) MessageBox.Show("SET STEP SIZE Fail");
		}

		private void Btn_GET_STEPSIZE_Click(object sender, EventArgs e)
		{
			int Value = 0;
			bool Rtn = Ctrl.GET_STEPSIZE(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET STEP SIZE Fail");
			}
			else
			{
				MessageBox.Show($"GET STEP SIZE : {Value}");
			}
		}

		private void Btn_GET_SYSTEMERROR_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_SYSTemERRor(ref Value);

			if (!Rtn)
			{
				MessageBox.Show("GET SYSTEM ERROR Fail");
			}
			else
			{
				MessageBox.Show($"GET SYSTEM ERROR : {Value}");
			}
		}

		private void Btn_SET_UNIT_nm_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.SET_UNITS_nm();
			if (!Rtn) MessageBox.Show("SET UNITS(nm) Fail");
		}

		private void Btn_SET_UNIT_um_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.SET_UNITS_um();
			if (!Rtn) MessageBox.Show("SET UNITS(um) Fail");
		}

		private void Btn_SET_UNIT_wm_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.SET_UNITS_wm();
			if (!Rtn) MessageBox.Show("SET UNITS(wm) Fail");
		}

		private void Btn_GET_UNIT_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_UNITS(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET UNITS Fail");
			}
			else
			{
				MessageBox.Show($"GET UNITS : {Value}");
			}
		}

		private void Btn_GET_VERSION_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_VERSION(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET VERSION Fail");
			}
			else
			{
				MessageBox.Show($"GET VERSION : {Value}");
			}
		}

		private void Btn_GET_WAVE_Click(object sender, EventArgs e)
		{
			double Value = 0;
			bool Rtn = Ctrl.GET_WAVE(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET WAVE Fail");
			}
			else
			{
				Num_GET_WAVE.Value = (decimal)Value;
			}
		}

		private void Btn_GET_WAVEMAX_Click(object sender, EventArgs e)
		{
			double Value = 0;
			bool Rtn = Ctrl.GET_WAVEMAX(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET WAVE MAX Fail");
			}
			else
			{
				MessageBox.Show($"GET WAVE MAX : {Value}");
			}
		}

		private void Btn_GET_IDN_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_IDN(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET IDN Fail");
			}
			else
			{
				MessageBox.Show($"GET IDN : {Value}");
			}
		}

		private void Btn_GET_ESR_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_ESR(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET ESR Fail");
			}
			else
			{
				MessageBox.Show($"GET ESR : {Value}");
			}
		}

		private void Btn_SET_OPC_Click(object sender, EventArgs e)
		{
			bool Rtn = Ctrl.SET_OPC();
			if (!Rtn) { MessageBox.Show("SET OPC Fail"); }
		}

		private void Btn_Get_OPC_Click(object sender, EventArgs e)
		{
			string Value = "";
			bool Rtn = Ctrl.GET_OPC(ref Value);
			if (!Rtn)
			{
				MessageBox.Show("GET OPC Fail");
			}
			else
			{
				MessageBox.Show($"GET OPC : {Value}");
			}
		}

		private bool isBusy = false;

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (isBusy) return;   // 避免重入
			isBusy = true;

			Task.Run(() =>
			{
				try
				{
					bool Rtn = Ctrl.IsConnect;

					bool FlowRun = Ctrl.IsFlowing;

					if (Rtn && !FlowRun)
					{
						bool Shutter_OnOff = false;
						bool GET_SHUTTER_Rtn = Ctrl.GET_SHUTTER(ref Shutter_OnOff);

						string Grating_Msg = "";
						bool GET_GRATing_Rtn = Ctrl.GET_GRATing(ref Grating_Msg);

						int Filter_Idx = 0;
						bool GET_FILTER_Rtn = Ctrl.GET_FILTER_SET(ref Filter_Idx);

						double WaveLength = 0;
						bool GET_WAVE_Rtn = Ctrl.GET_WAVE(ref WaveLength);

						return;
						this.Invoke(new Action(() =>
						{
							if (GET_SHUTTER_Rtn)
							{
								Cbx_GET_SHUTTER.Checked = Shutter_OnOff;
							}

							if (GET_GRATing_Rtn)
							{
								Tbx_GET_GRATing.Text = Grating_Msg;
							}

							if (GET_FILTER_Rtn)
							{
								Num_GET_Filter.Value = Filter_Idx;
							}

							if (GET_WAVE_Rtn)
							{
								Num_GET_WAVE.Value = (decimal)WaveLength;
							}
						}));
					}
				}
				catch (Exception)
				{

				}
				finally
				{
					isBusy = false;
				}
			});

			this.Invoke(new Action(() =>
			{
				Cbx_GET_SHUTTER.Checked = Ctrl.Shutter_OnOff;
				Tbx_GET_GRATing.Text = Ctrl.Grating_Msg;
				Num_GET_WAVE.Value = (decimal)Ctrl.Wave;
				Num_GET_Filter.Value = Ctrl.Filter_Idx;
			}));
		}

		private void UpdateCheckBox(CheckBox Ctrl, bool OnOff)
		{
			if (Ctrl.InvokeRequired)
			{
				Ctrl.BeginInvoke(new Action(() =>
				{
					Ctrl.Checked = OnOff;
				}));
			}
			else
			{
				Ctrl.Checked = OnOff;
			}
		}
	}
}
