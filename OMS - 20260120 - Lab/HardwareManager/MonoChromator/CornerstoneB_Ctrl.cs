using BaseTool;
using CommonBase.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareManager
{
	public class CornerstoneB_Ctrl
	{
		public event Action<bool, string, int, double> UPDATE_STATUS;

		public bool Shutter_OnOff = false;
		public string Grating_Msg = "";
		public int Filter_Idx = 0;
		public double Wave = 0;

		private SerialPort My_SerialPort;
		private InfoManager info;

		private UnitStatus MeasurementStatus = UnitStatus.Idle;
		public UnitStatus Status { get => MeasurementStatus; }

		private List<string> ST_Result = new List<string>();
		public List<string> Result { get => ST_Result; }

		public bool IsConnect { get => My_SerialPort.IsOpen; }

		private TimeManager TM_Receive = new TimeManager();

		public bool IsFlowing = false;

		public CornerstoneB_Ctrl(InfoManager info)
		{
			this.info = info;
			My_SerialPort = new SerialPort();
			My_SerialPort.DtrEnable = true;
			My_SerialPort.RtsEnable = true;

			My_SerialPort.DataReceived -= My_SerialPort_DataReceived;
			My_SerialPort.DataReceived += My_SerialPort_DataReceived;

		}

		public string RequestMsg = "";
		public string ResponseMsg = "";
		public bool GetResponse = false;

		private void My_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			ResponseMsg = My_SerialPort.ReadLine();
			//ResponseMsg = My_SerialPort.ReadExisting();

			if (!ResponseMsg.Contains(RequestMsg))
			{

				ResponseMsg = ResponseMsg.Replace("\r", "");
				ResponseMsg = ResponseMsg.Replace("\n", "");

				GetResponse = true;
			}
		}

		private void UpdateStauts()
		{
			UPDATE_STATUS?.Invoke(Shutter_OnOff, Grating_Msg, Filter_Idx, Wave);
		}

		private void SaveLog(string Log, bool isAlm = false)
		{
			if (!isAlm)
			{
				info.General($"[CornerstoneB] {Log}");
			}
			else
			{
				info.Error($"[CornerstoneB] {Log}");
			}
		}

		public bool Connect(int Comport, int Baudrate = 9600)
		{
			try
			{
				if (My_SerialPort.IsOpen)
				{
					My_SerialPort.Close();
				}

				//設定 Serial Port 參數                
				My_SerialPort.PortName = "COM" + Comport.ToString();
				My_SerialPort.DataBits = 8;
				My_SerialPort.Parity = Parity.None;
				My_SerialPort.StopBits = StopBits.One;

				My_SerialPort.RtsEnable = true;
				My_SerialPort.DtrEnable = true;

				if (!My_SerialPort.IsOpen)
				{
					//開啟 Serial Port
					My_SerialPort.Open();
				}

				return true;
			}
			catch (Exception ex)
			{
				SaveLog(ex.Message, true);
				return false;
			}
		}

		public void Close()
		{
			try
			{
				My_SerialPort.Close();
			}
			catch (Exception ex)
			{
				SaveLog(ex.Message, true);
			}
		}

		#region Function

		private string SendCommand(string CMD, bool WaitResponse = false)
		{
			Stopwatch SW = new Stopwatch();
			SW.Start();

			if (!My_SerialPort.IsOpen)
			{
				SaveLog($"Port is not Open", true);
				return null;
			}

			try
			{
				//SaveLog($"Send Command : {CMD}");

				RequestMsg = CMD;
				ResponseMsg = "";
				GetResponse = false;

				My_SerialPort.WriteLine(CMD);

				if (WaitResponse)
				{
					while (!GetResponse)
					{
						if (SW.ElapsedMilliseconds > 5000) return null;
					}

					return ResponseMsg;

				}
				else
				{
					return "OK";
				}

			}
			catch (TimeoutException)
			{
				SaveLog("Wait Response Timeout！", true);
				return null;
			}
		}

		public bool SET_BANDPASS(int Bandpass)
		{
			string Command = $"bandpass {Bandpass}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_BANDPASS(ref double Bandpass)
		{
			string Command = $"bandpass?";
			string Response = SendCommand(Command, true);

			try
			{
				Bandpass = (Response != null) ? Convert.ToDouble(Response) : -1;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_CALIBRATE(double GratingLocation)
		{
			string Command = $"calibrate {GratingLocation}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool SET_ECHO(bool OnOff)
		{
			int Switch = (OnOff) ? 1 : 0;
			string Command = $"Echo {Switch}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_ECHO(ref bool OnOff)
		{
			string Command = $"echo?";
			string Response = SendCommand(Command, true);

			OnOff = (Response == "1");

			return Response != null;
		}

		public bool GET_ERROR(ref string ErrorMsg)
		{
			string Command = $"error?";
			string Response = SendCommand(Command, true);


			try
			{
				int GetCode = (Response != null) ? Convert.ToInt32(Response) : -1;

				if (GetCode == 0) return true;
				ErrorMsg = "";

				Enum_ErrorCode Code = (Enum_ErrorCode)GetCode;

				FieldInfo field = Code.GetType().GetField(Code.ToString());
				// 獲取 DescriptionAttribute
				DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

				ErrorMsg = attribute.Description;

				return Response != null;
			}
			catch
			{
				return false;
			}

		}

		public bool SET_FILTER(int FilterIndex)
		{
			string Command = $"filter {FilterIndex}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_FILTER(ref int FilterIndex)
		{
			string Command = $"Filter?";
			string Response = SendCommand(Command, true);

			try
			{
				FilterIndex = Convert.ToInt32(Response);
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_FILTER_SET(ref int Filter)
		{
			string Command = $"Filter:set?";
			string Response = SendCommand(Command, true);

			try
			{
				Filter = Convert.ToInt32(Response);
			}
			catch (Exception)
			{
				return false;
			}

			Filter_Idx = Filter;

			UpdateStauts();

			return Response != null;
		}

		public bool SET_FILTERLABEL(int FilterIndex, string Label)
		{
			if (Label.Length > 8) Label = Label.Substring(0, 8);

			string Command = $"Filter{FilterIndex}label {Label}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_FILTERLABEL(int FilterIndex, ref string Label)
		{
			string Command = $"Filter{FilterIndex}label?";
			string Response = SendCommand(Command, true);
			Label = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool FIND_HOME()
		{
			string Command = $"findhome";
			string Response = SendCommand(Command);
			return Response == "OK";
		}
		public bool SET_GOWAVE(double Wavelength)
		{
			string Command = $"gowave {Wavelength}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GOWAVE(ref double Wavelength)
		{
			string Command = $"gowave?";
			string Response = SendCommand(Command, true);

			try
			{
				Wavelength = (Response != null) ? Convert.ToDouble(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_GRATing(int GratingIndex)
		{
			string Command = $"grating {GratingIndex}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GRATing(ref string GratingMsg)
		{
			string Command = $"grating?";
			string Response = SendCommand(Command, true);

			try
			{
				GratingMsg = (Response != null) ? Response : "";
			}
			catch (Exception)
			{
				return false;
			}

			Grating_Msg = GratingMsg;
			UpdateStauts();

			return Response != null;
		}

		public bool SET_GRATFACTOR(int GratingIndex, double Factor)
		{
			string Command = $"Grat{GratingIndex}factor {Factor}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GRATFACTOR(int GratingIndex, ref double Factor)
		{
			string Command = $"grat{GratingIndex}factor?";
			string Response = SendCommand(Command, true);

			try
			{
				Factor = (Response != null) ? Convert.ToDouble(Factor) : -1;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_GRATLABEL(int GratingIndex, string Label)
		{
			if (Label.Length > 8) Label = Label.Substring(0, 8);

			string Command = $"grat{GratingIndex}label {Label}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GRATLABEL(int GratingIndex, ref string Label)
		{
			string Command = $"grat{GratingIndex}label?";
			string Response = SendCommand(Command, true);
			Label = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool SET_GRATLINES(int GratingIndex, int Lines)
		{
			string Command = $"grat{GratingIndex}lines {Lines}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GRATLINES(int GratingIndex, ref int Lines)
		{
			string Command = $"grat{GratingIndex}lines?";
			string Response = SendCommand(Command, true);

			try
			{
				Lines = (Response != null) ? Convert.ToInt32(Response) : -1;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_GRATOFFSET(int GratingIndex, double Offset)
		{
			string Command = $"grat{GratingIndex}offset {Offset}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_GRATOFFSET(int GratingIndex, ref double Offset)
		{
			string Command = $"grat{GratingIndex}offset?";
			string Response = SendCommand(Command, true);

			try
			{
				Offset = (Response != null) ? Convert.ToDouble(Response) : -1;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_HANDSHAKE(bool OnOff)
		{
			int Switch = (OnOff) ? 1 : 0;
			string Command = $"handshake {Switch}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_HANDSHAKE(ref bool OnOff)
		{
			string Command = $"Handshake?";
			string Response = SendCommand(Command, true);

			try
			{
				OnOff = (Response != null) ? (Response == "1") : false;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_IDLE(ref bool OnOff)
		{
			string Command = $"IDLE?";
			string Response = SendCommand(Command, true);

			try
			{
				OnOff = (Response != null) ? (Response == "1") : false;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_INFO(ref string Info)
		{
			string Command = $"INFO?";
			string Response = SendCommand(Command, true);
			Info = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool SET_OUTPORT(int PortIndex)
		{
			string Command = $"outport {PortIndex}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_OUTPORT(ref int PortIndex)
		{
			string Command = $"outport?";
			string Response = SendCommand(Command, true);

			try
			{
				PortIndex = (Response != null) ? Convert.ToInt32(Response) : -1;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_SHUTTER(bool OnOff)
		{
			int Switch = (OnOff) ? 1 : 0;
			string Command = $"shutter {Switch}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_SHUTTER(ref bool OnOff)
		{
			string Command = $"shutter?";
			string Response = SendCommand(Command, true);

			try
			{
				OnOff = (Response != null) ? (Response == "O") : false;
			}
			catch (Exception)
			{
				return false;
			}

			Shutter_OnOff = OnOff;

			UpdateStauts();

			return Response != null;
		}

		public bool SET_SLITWIDTH(int SlitIndex, int Width)
		{
			string Command = $"slit{SlitIndex}microns {Width}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_SLITWIDTH(int SlitIndex, ref int Width)
		{
			string Command = $"slit{SlitIndex}microns?";
			string Response = SendCommand(Command, true);

			try
			{
				Width = (Response != null) ? Convert.ToInt16(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_STB(ref int Result)
		{
			string Command = $"STB?";
			string Response = SendCommand(Command, true);

			try
			{
				Result = (Response != null) ? Convert.ToInt16(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool SET_SLOWSTEP(int Step)
		{
			string Command = $"slowstep {Step}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool SET_STEP(int Step)
		{
			string Command = $"step {Step}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool SET_STEPSIZE(int StepSize)
		{
			string Command = $"stepsize {StepSize}";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_STEPSIZE(ref int StepSize)
		{
			string Command = $"stepsize?";
			string Response = SendCommand(Command, true);

			try
			{
				StepSize = (Response != null) ? Convert.ToInt16(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_SYSTemERRor(ref string Result)
		{
			string Command = $"syst:err?";
			string Response = SendCommand(Command, true);
			Result = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool SET_UNITS_nm()
		{
			string Command = $"units nm";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool SET_UNITS_um()
		{
			string Command = $"units um";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool SET_UNITS_wm()
		{
			string Command = $"units wm";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_UNITS(ref string Unit)
		{
			string Command = $"units?";
			string Response = SendCommand(Command, true);
			Unit = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool GET_VERSION(ref string Version)
		{
			string Command = $"version?";
			string Response = SendCommand(Command, true);
			Version = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool GET_WAVE(ref double Wave)
		{
			string Command = $"wave?";
			string Response = SendCommand(Command, true);

			try
			{
				Wave = (Response != null) ? Convert.ToDouble(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			this.Wave = Wave;
			UpdateStauts();

			return Response != null;
		}

		public bool GET_WAVEMAX(ref double WaveMax)
		{
			string Command = $"wavemax?";
			string Response = SendCommand(Command, true);

			try
			{
				WaveMax = (Response != null) ? Convert.ToDouble(Response) : -999;
			}
			catch (Exception)
			{
				return false;
			}

			return Response != null;
		}

		public bool GET_IDN(ref string Info)
		{
			string Command = $"*IDN?";
			string Response = SendCommand(Command, true);
			Info = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool GET_ESR(ref string Info)
		{
			string Command = $"*ESR?";
			string Response = SendCommand(Command, true);
			Info = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool SET_OPC()
		{
			string Command = $"*OPC";
			string Response = SendCommand(Command);
			return Response == "OK";
		}

		public bool GET_OPC(ref string Info)
		{
			string Command = $"*OPC?";
			string Response = SendCommand(Command, true);
			Info = (Response != null) ? Response : "";
			return Response != null;
		}

		public bool GET_WAI(ref string Info)
		{
			string Command = $"*WAI";
			string Response = SendCommand(Command, true);
			Info = (Response != null) ? Response : "";
			return Response != null;
		}

		#endregion

		public enum Enum_ErrorCode
		{
			[Description("DataType Error")]
			DataType_Error = -104,

			[Description("Parameter NotAllowed")]
			Parameter_NotAllowed = -108,

			[Description("Missing Parameter")]
			Missing_Parameter = -109,

			[Description("Undefined Header")]
			Undefined_Header = -113,

			[Description("Unexpected Number Of Parameters")]
			Unexpected_Number_Of_Parameters = -115,

			[Description("String Data Error")]
			String_Data_Error = -150,

			[Description("Invalid String Data")]
			Invalid_String_Data = -151,

			[Description("Label Too Long")]
			Label_Too_Long = -185,

			[Description("Cmd Queue Overflow")]
			Cmd_Queue_Overflow = -186,

			[Description("Cmd Too Long")]
			Cmd_Too_Long = -187,

			[Description("Parameter Error")]
			Parameter_Error = -220,

			[Description("Data Out Of Range")]
			Data_Out_Of_Range = -222,

			[Description("Illegal Parameter Value")]
			Illegal_Parameter_Value = -224,

			[Description("Storage Fault")]
			Storage_Fault = 320,

			[Description("Calibration Failed")]
			Calibration_Failed = -340,

			[Description("Calibration Data Invalid")]
			Calibration_Data_Invalid = -341,

			[Description("Error Queue Overflow")]
			Error_Queue_Overflow = -350,

			[Description("Serial Num Truncated")]
			Serial_Num_Truncated = -351,

			[Description("Query Unterminated")]
			Query_Unterminated = -420,

			[Description("Unable To Home Grating")]
			Unable_To_Home_Grating = 402,

			[Description("Cannot Reach Wavelength")]
			Cannot_Reach_Wavelength = 403,

			[Description("Grating Halted")]
			Grating_Halted = 404,

			[Description("Filter Wheel Missing")]
			Filter_Wheel_Missing = 501,

			[Description("Filter Wheel Fault")]
			Filter_Wheel_Fault = 502,

			[Description("Filter Halted")]
			Filter_Halted = 504,

			[Description("Slit Controller Missing")]
			Slit_Controller_Missing = 505,

			[Description("Slit Controller Fault")]
			Slit_Controller_Fault = 506,

			[Description("Slit Controller Halted")]
			Slit_Controller_Halted = 507,

			[Description("Unable To Home Slit")]
			Unable_To_Home_Slit = 508,

			[Description("Unable To Home Turret")]
			Unable_To_Home_Turret = 510,

			[Description("Turret Halted")]
			Turret_Halted = 512,

			[Description("Accessory Missing")]
			Accessory_Missing = 520,

			[Description("Mirror End Not Detected")]
			Mirror_End_Not_Detected = 531,
		}
	}
}
