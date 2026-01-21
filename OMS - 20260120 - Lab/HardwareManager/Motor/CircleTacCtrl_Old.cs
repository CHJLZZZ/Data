using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using BaseTool;


namespace HardwareManager
{
	public class CircleTacCtrl_Old : IMotorControl
	{
		enum AiryMoveStep
		{
			Init,
			CheckOriPos,
			GetOriPos,
			MovePos,
			CheckInPos,
			Finish,
			Alarm,
		}

		public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;
		public event Action<HardwareInfo, HardwareInfo, HardwareInfo, HardwareInfo> UpdateInfo;

		private InfoManager info;

		private TcpClient client = new TcpClient();
		private NetworkStream stream;
		private int ConnectPort = 8887;
		private string ConnectIP = "192.168.3.7";

		public bool isMoveProc = false;
		public bool isCompensationMoveFinish_Focuser = false;
		public bool isCompensationMoveFinish_Aperture = false;

		private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
		private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
		private UnitStatus moveStatus_FW1 = UnitStatus.Idle;
		private UnitStatus moveStatus_FW2 = UnitStatus.Idle;
		private UnitStatus flowStatus = UnitStatus.Idle;

		private int Focuser_limitF = 3800000;
		private int Focuser_limitR = 0;
		private int Aperture_limitF = 1600000;
		private int Aperture_limitR = 0;

		private HardwareInfo _Focuser = new HardwareInfo();
		private HardwareInfo _Aperture = new HardwareInfo();
		private HardwareInfo _FW1 = new HardwareInfo();
		private HardwareInfo _FW2 = new HardwareInfo();

		private AiryMoveStep moveStep = AiryMoveStep.Init;

		public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.CircleTac;

		private int Backlash = 5000;

		private int kfocus = (int)(150 * 10000 / 74.2);
		private int kguang = (int)(160 * 10000 / 180);
		private int maxguangzhi = 160 * 10000;

		private bool IsUsemm = false;

		public CircleTacCtrl_Old(InfoManager info)
		{
			this.info = info;

			this._Focuser.Limit = Focuser_limitF;
			this._Aperture.Limit = Aperture_limitF;

		}

		public UnitStatus MoveStatus_Focuser()
		{
			return this.moveStatus_Focuser;
		}

		public UnitStatus MoveStatus_Aperture()
		{
			return this.moveStatus_Aperture;
		}

		public UnitStatus MoveStatus_FW1()
		{
			return this.moveStatus_FW1;
		}

		public UnitStatus MoveStatus_FW2()
		{
			return this.moveStatus_FW2;
		}

		public UnitStatus MoveFlowStatus()
		{
			return this.flowStatus;
		}

		public bool IsWork()
		{
			return true;
		}

		public bool IsMoveProc()
		{
			return this.isMoveProc;
		}

		public int NowPos_Focuser()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.Focuser, ParaType.NowPos, out RtnValue);

			if (Rtn)
			{
				this._Focuser.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Focuser.NowPos;
		}

		public int NowPos_Aperture()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.Aperture, ParaType.NowPos, out RtnValue);

			if (Rtn)
			{
				this._Aperture.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Aperture.NowPos;
		}

		public int NowPos_FW1()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.FW1, ParaType.NowPos, out RtnValue);

			if (Rtn)
			{
				this._FW1.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._FW1.NowPos;
		}

		public int NowPos_FW2()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.FW2, ParaType.NowPos, out RtnValue);

			if (Rtn)
			{
				this._FW2.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._FW2.NowPos;
		}

		public int TargetPos_Focuser()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.Focuser, ParaType.TargetPos, out RtnValue);

			if (Rtn)
			{
				this._Focuser.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Focuser.NowPos;
		}

		public int TargetPos_Aperture()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.Aperture, ParaType.TargetPos, out RtnValue);

			if (Rtn)
			{
				this._Aperture.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Aperture.NowPos;
		}

		public int TargetPos_FW1()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.FW1, ParaType.TargetPos, out RtnValue);

			if (Rtn)
			{
				this._FW1.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._FW1.NowPos;
		}

		public int TargetPos_FW2()
		{
			int RtnValue = 0;
			bool Rtn = ReadInfo(MotorMember.FW2, ParaType.TargetPos, out RtnValue);

			if (Rtn)
			{
				this._FW2.NowPos = RtnValue;
				UpdateHardwareInfo();
			}

			return this._FW2.NowPos;
		}

		public bool IsHome_Focuser()
		{
			int RtnValue;
			bool Rtn = ReadInfo(MotorMember.Focuser, ParaType.HomeStatus, out RtnValue);

			if (Rtn)
			{
				this._Focuser.HomeState = (RtnValue == 5);
				UpdateHardwareInfo();
			}

			return this._Focuser.HomeState;
		}

		public bool IsHome_Aperture()
		{
			int RtnValue;
			bool Rtn = ReadInfo(MotorMember.Aperture, ParaType.HomeStatus, out RtnValue);

			if (Rtn)
			{
				this._Aperture.HomeState = (RtnValue == 5);
				UpdateHardwareInfo();
			}

			return this._Aperture.HomeState;
		}

		public int WorkCurrent_Focuser()
		{
			int RtnValue;
			bool Rtn = ReadInfo(MotorMember.Focuser, ParaType.WorkCurrent, out RtnValue);

			if (Rtn)
			{
				this._Focuser.WorkCurrent = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Focuser.WorkCurrent;
		}

		public int WorkCurrent_Aperture()
		{
			int RtnValue;
			bool Rtn = ReadInfo(MotorMember.Aperture, ParaType.WorkCurrent, out RtnValue);

			if (Rtn)
			{
				this._Aperture.WorkCurrent = RtnValue;
				UpdateHardwareInfo();
			}

			return this._Aperture.WorkCurrent;
		}


		public double NowVelocity_Focuser()
		{
			return 0;
		}

		public double NowVelocity_Aperture()
		{
			return 0;
		}
		public int Focus_LimitF()
		{
			return this.Focuser_limitF;
		}

		public int Aperture_LimitF()
		{
			return this.Aperture_limitF;
		}

		public int Focus_LimitR()
		{
			return this.Focuser_limitR;
		}



		public void SaveLog(string Log, bool isAlm = false)
		{
			if (isAlm)
			{
				info.Error($"[Motor] {Log}");
			}
			else
			{
				info.General($"[Motor] {Log}");
			}
		}

		#region Connect

		public void Open()
		{
			try
			{
				client = new TcpClient();
				IAsyncResult result = client.BeginConnect(IPAddress.Parse(ConnectIP), ConnectPort, null, null);
				bool success = result.AsyncWaitHandle.WaitOne(500, true);

				if (!success)
				{
					SaveLog($"Connect Timeout (500ms)", true);
					return;
				}

				client.EndConnect(result);
				stream = client.GetStream();
				SaveLog($"Connect Succed");
			}
			catch (SocketException ex)
			{
				SaveLog($"Connect Fail (Socket Error) : {ex.Message}", true);
			}
			catch (Exception ex)
			{
				SaveLog($"Connect Fail : {ex.Message}", true);
			}
		}

		public void Open(string Com_Port)
		{
		}

		public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
		{

		}

		public void Close()
		{
			if (client.Connected)
			{
				stream?.Close();
				client?.Close();
				client?.Dispose();
			}

			this.isMoveProc = false;
		}

		public bool CheckConnect()
		{
			try
			{
				byte[] testData = new byte[1] { 0 };
				client.GetStream().Write(testData, 0, testData.Length);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion

		#region Hardware Function

		private bool SendAndReceiveData(byte[] sendData, out byte[] trspData)
		{
			trspData = new byte[sendData.Length];

			try
			{
				client = new TcpClient();
				IAsyncResult result = client.BeginConnect(IPAddress.Parse(ConnectIP), ConnectPort, null, null);
				//bool success = result.AsyncWaitHandle.WaitOne(500, true);

				//if (!success)
				//{
				//    SaveLog($"Connect Timeout (500ms)", true);
				//    return false;
				//}

				client.EndConnect(result);
				stream = client.GetStream();

				if (!client.Connected || stream == null)
				{
					SaveLog($"Send Request Error : Disconnect", true);
					return false;
				}

				stream.Write(sendData, 0, sendData.Length);

				// 等待 1000ms
				stream.ReadTimeout = 1000;

				try
				{
					int bytesRead = stream.Read(trspData, 0, trspData.Length);
					if (bytesRead == sendData.Length && trspData[8] == (byte)trspData.Take(8).Sum(b => (int)b))
					{
						return true;
					}
					else
					{
						SaveLog("Server Reply Error: " + BitConverter.ToString(trspData), true);
						return false;
					}
				}
				catch (Exception ex)
				{
					SaveLog($"SendAndReceiveData Fail : {ex.Message}", true);
					return false;
				}

			}
			catch (Exception ex)
			{
				SaveLog($"Send Request Error : {ex} , {BitConverter.ToString(trspData)}", true);
				return false;
			}
			finally
			{
				stream?.Close();
				client?.Close();
			}
		}

		private bool DiskMove(MotorMember Member, int Pos)
		{
			if (Member != MotorMember.FW1 && Member != MotorMember.FW2)
			{
				SaveLog($"Motor Member Error");
				return false;
			}

			if (Pos < 0 || Pos > 4)
			{
				SaveLog($"Disk Move Pos Should be in 0-4");
				return false;
			}

			Pos++;

			byte[] sendData = { 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x04 };

			sendData[3] = (Member == MotorMember.FW1) ? (byte)0x01 : (byte)0x02;
			sendData[2] = (byte)Pos;
			sendData[8] = (byte)sendData.Take(8).Sum(b => (int)b);

			byte[] trspData = sendData;
			SendAndReceiveData(sendData, out trspData);

			return true;
		}

		//private async void MotorMove(MotorMember Member, int Pos)
		private bool MotorMove(MotorMember Member, int Pos)
		{

			if (Member != MotorMember.Aperture && Member != MotorMember.Focuser)
			{
				SaveLog($"Motor Member Error");
				return false;
			}

			if (IsUsemm)
			{
				if (Member == MotorMember.Focuser)
				{
					Pos = Pos * kfocus;
				}

				if (Member == MotorMember.Aperture)
				{
					Pos = Pos * kguang;
					Pos = maxguangzhi - Pos;
				}
			}

			int LimitF = (Member == MotorMember.Focuser) ? Focuser_limitF : Aperture_limitF;
			int LimitR = (Member == MotorMember.Focuser) ? Focuser_limitR : Aperture_limitR;

			if (Pos > LimitF || Pos < LimitR)
			{
				SaveLog($"Pos Setting Error : Range = {LimitR}~{LimitF}");
				return false;
			}

			byte[] sendData = { 0x01, 0x01, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x04 };

			sendData[3] = (Member == MotorMember.Focuser) ? (byte)0x04 : (byte)0x03;
			sendData[4] = (byte)((Pos >> 24) & 0xFF);
			sendData[5] = (byte)((Pos >> 16) & 0xFF);
			sendData[6] = (byte)((Pos >> 8) & 0xFF);
			sendData[7] = (byte)(Pos & 0xFF);
			sendData[8] = (byte)sendData.Take(8).Sum(b => (int)b);

			byte[] trspData = sendData;
			SendAndReceiveData(sendData, out trspData);

			return true;
		}

		private bool ReadInfo(MotorMember Member, ParaType Para, out int Value)
		{
			if (Member == MotorMember.All)
			{
				Value = 0;
				return false;
			}

			int MemberIdx = 0;

			switch (Member)
			{
				case MotorMember.Focuser: MemberIdx = 4; break;
				case MotorMember.Aperture: MemberIdx = 3; break;
				case MotorMember.FW1: MemberIdx = 1; break;
				case MotorMember.FW2: MemberIdx = 2; break;
			}

			int ParaIdx = 0;

			switch (Para)
			{
				case ParaType.NowPos: ParaIdx = 2; break;
				case ParaType.TargetPos: ParaIdx = 3; break;
				case ParaType.HomeStatus: ParaIdx = 4; break;
				case ParaType.WorkCurrent: ParaIdx = 5; break;
			}

			if (MemberIdx == 1 || MemberIdx == 2)
			{
				ParaIdx++;
				if (ParaIdx == 5 || ParaIdx == 6)
				{
					SaveLog($"{Member} does not support this parameter : {Para} , ", true);
					Value = 0;
					return false;
				}
			}

			byte[] sendData = { 0x01, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x04 };

			sendData[1] = (byte)ParaIdx;
			sendData[3] = (byte)MemberIdx;
			sendData[8] = (byte)sendData.Take(8).Sum(b => (int)b);
			byte[] trspData = sendData;
			bool result = SendAndReceiveData(sendData, out trspData);
			if (!result)
			{
				Value = 0;
				return false;
			}

			Value = ((trspData[4] & 0xFF) << 24)
			   | ((trspData[5] & 0xFF) << 16)
			   | ((trspData[6] & 0xFF) << 8)
			   | (trspData[7] & 0xFF);

			if (MemberIdx == 4)
			{
				if (ParaIdx == 2 || ParaIdx == 3)
				{
					if (IsUsemm)
					{
						Value = (int)((Value + 0.5 * kfocus) / kfocus);
					}
				}
			}

			if (MemberIdx == 3)
			{
				if (ParaIdx == 2 || ParaIdx == 3)
				{
					if (IsUsemm)
					{
						Value = maxguangzhi - Value;
						Value = (int)((Value + 0.5 * kguang) / kguang);
					}
				}
			}

			if (MemberIdx == 1 || MemberIdx == 2)
			{
				if (ParaIdx == 3 || ParaIdx == 4)
				{
					Value = -Value;
					Value = Value / 30720;
				}

			}
			return true;
		}


		private void UpdateHardwareInfo()
		{
			UpdateInfo?.Invoke(_Focuser, _Aperture, _FW1, _FW2);

			MotorInfo FocuserInfo = new MotorInfo { Position = _Focuser.NowPos };
			MotorInfo ApertureInfo = new MotorInfo { Position = _Aperture.NowPos };
			MotorInfo FW1Info = new MotorInfo { Position = _FW1.NowPos };
			MotorInfo FW2Info = new MotorInfo { Position = _FW2.NowPos };

			UpdateStatus?.Invoke(FocuserInfo, ApertureInfo, FW1Info, FW2Info);

		}

		#endregion

		public int GetPosition(MotorMember Member)
		{
			switch (Member)
			{
				case MotorMember.Focuser:
					return NowPos_Focuser();

				case MotorMember.Aperture:
					return NowPos_Aperture();

				case MotorMember.FW1:
					return NowPos_FW1();

				case MotorMember.FW2:
					return NowPos_FW2();
			}

			return -1;
		}

		public int GetTargetPosition(MotorMember Member)
		{
			switch (Member)
			{
				case MotorMember.Focuser:
					return TargetPos_Focuser();

				case MotorMember.Aperture:
					return TargetPos_Aperture();

				case MotorMember.FW1:
					return TargetPos_FW1();

				case MotorMember.FW2:
					return TargetPos_FW2();
			}

			return -1;
		}

		public bool GetHomeState(MotorMember Member)
		{
			switch (Member)
			{
				case MotorMember.Focuser:
					return IsHome_Focuser();

				case MotorMember.Aperture:
					return IsHome_Aperture();
			}

			return false;
		}

		public int GetWorkCurrent(MotorMember Member)
		{
			switch (Member)
			{
				case MotorMember.Focuser:
					return WorkCurrent_Focuser();

				case MotorMember.Aperture:
					return WorkCurrent_Aperture();
			}

			return -1;
		}

		public double GetVelocity(MotorMember Member)
		{
			return 0;
		}

		public void Reset(MotorMember Member)
		{
			return;
		}

		public bool Home(MotorMember Member = MotorMember.All)
		{
			return true;
		}

		public void Jog(MotorMember Member, int Jog_Dist)
		{
			return;
		}

		public void Stop(MotorMember device)
		{
			this.isMoveProc = false;

		}

		public void Stop()
		{
			this.isMoveProc = false;
		}

		public bool DirectMove(MotorMember Member, int Pos)
		{
			//if (!this.client.Connected)
			//{
			//    SaveLog($"TCP is not Connected", true);
			//    return false;
			//}

			switch (Member)
			{
				case MotorMember.Focuser:
				case MotorMember.Aperture:
					{
						return MotorMove(Member, Pos);
					}

				case MotorMember.FW1:
				case MotorMember.FW2:
					{
						return DiskMove(Member, Pos);
					}

				default:
					{
						return false;
					}
			}
		}


		public virtual void Move(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 60000)
		{
			if (!this.isMoveProc)
			{
				this.moveStatus_Focuser = UnitStatus.Idle;
				this.isMoveProc = true;


				Thread PreprocessThread = new Thread(() => Move_Proc(FocuserPos, AperturePos, FilterNdPos, FilterXyzPos, TimeOut));
				PreprocessThread.Start();
			}
		}

		#region --- Move_Proc ---
		private void Move_Proc(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut)
		{
			// Pos = -1 ==> Bypass 

			TimeManager TM = new TimeManager();
			TM.SetDelay(TimeOut);
			this.moveStep = AiryMoveStep.Init;

			bool[] isActive = new bool[4];

			int[] TargetPos = new int[4];

			bool[] isReverse = new bool[2];

			TimeManager TM_Retry = new TimeManager(3000);

			while (this.isMoveProc)
			{
				if (TM.IsTimeOut())
				{
					SaveLog($"Move Flow Timeout", true);
					this.isMoveProc = false;
					break;
				}

				switch (this.moveStep)
				{
					case AiryMoveStep.Init:
						{
							flowStatus = UnitStatus.Running;

							isActive = new bool[]
							{
								FocuserPos >= 0,
								AperturePos >= 0,
								FW1Pos >= 0,
								FW2Pos >= 0,
							};

							TargetPos = new int[]
							{
								FocuserPos,
								AperturePos,
								FW1Pos,
								FW2Pos,
							};

							SaveLog($"Move Flow Start , Focuser = {FocuserPos} , Aperture = {AperturePos} , FW1 = {FW1Pos} , FW2 = {FW2Pos}");

							if (isActive[0]) this.moveStatus_Focuser = UnitStatus.Running;
							if (isActive[1]) this.moveStatus_Aperture = UnitStatus.Running;
							if (isActive[2]) this.moveStatus_FW1 = UnitStatus.Running;
							if (isActive[3]) this.moveStatus_FW2 = UnitStatus.Running;

							//調焦需移動，才紀錄原始位置 (補背隙)
							if (isActive[0] || isActive[1])
							{
								this.moveStep = AiryMoveStep.CheckOriPos;
							}
							else
							{
								this.moveStep = AiryMoveStep.MovePos;
							}
						}
						break;

					case AiryMoveStep.CheckOriPos:
						{
							bool Rtn = true;

							if (isActive[0])
							{
								GetPosition(MotorMember.Focuser);
								Thread.Sleep(100);

								Rtn &= (_Focuser.NowPos != -1);
							}

							if (isActive[1])
							{
								GetPosition(MotorMember.Aperture);
								Thread.Sleep(100);

								Rtn &= (_Aperture.NowPos != -1);
							}

							if (Rtn)
							{
								this.moveStep = AiryMoveStep.GetOriPos;
							}
							else
							{
								this.moveStep = AiryMoveStep.Alarm;
							}
						}
						break;

					case AiryMoveStep.GetOriPos:
						{
							int[] OriPos =
							{
									_Focuser.NowPos,
									_Aperture.NowPos
							 };

							for (int i = 0; i < 2; i++)
							{
								if (isActive[i])
								{
									isReverse[i] = TargetPos[i] < OriPos[i];

									if (isReverse[i])
									{
										TargetPos[i] -= this.Backlash;
									}
								}
							}

							SaveLog($"Get Ori Pos , Focuser = {OriPos[0]} , Aperture = {OriPos[1]}");

							this.moveStep = AiryMoveStep.MovePos;
						}
						break;

					case AiryMoveStep.MovePos:
						{
							bool Rtn = false;

							for (int i = 0; i < 4; i++)
							{
								if (isActive[i])
								{
									MotorMember Member = (MotorMember)i;
									Rtn = this.DirectMove(Member, TargetPos[i]);

									if (Rtn)
									{
										SaveLog($"{Member} Move to {TargetPos[i]}");
										Thread.Sleep(100);
									}
									else
									{
										break;
									}
								}
							}

							if (Rtn)
							{
								TM_Retry.SetDelay(5000);
								this.moveStep = AiryMoveStep.CheckInPos;
							}
							else
							{
								this.moveStep = AiryMoveStep.Alarm;
							}
						}
						break;

					case AiryMoveStep.CheckInPos:
						{
							bool CheckOK = true;

							for (int i = 0; i < 4; i++)
							{
								if (isActive[i])
								{
									MotorMember Member = (MotorMember)i;
									int NowPos = GetPosition(Member);
									Thread.Sleep(100);
									CheckOK &= (NowPos != -1);
								}
							}

							Thread.Sleep(100);

							int[] GetPos =
							{
								_Focuser.NowPos,
								_Aperture.NowPos,
								_FW1.NowPos,
								_FW2.NowPos
							};

							bool[] InPos = new bool[4];

							for (int i = 0; i < 4; i++)
							{
								InPos[i] = (GetPos[i] == TargetPos[i]) || (!isActive[i]);
							}

							// Check Finish
							if (InPos[0]) this.moveStatus_Focuser = UnitStatus.Finish;
							if (InPos[1]) this.moveStatus_Aperture = UnitStatus.Finish;
							if (InPos[2]) this.moveStatus_FW1 = UnitStatus.Finish;
							if (InPos[3]) this.moveStatus_FW2 = UnitStatus.Finish;

							if (InPos[0] && InPos[1] && InPos[2] && InPos[3])
							{
								Thread.Sleep(2000);

								isActive[2] = false;
								isActive[3] = false;

								//啟用且反向
								bool Check_Focuser = (isActive[0] && isReverse[0]);
								bool Check_Aperture = (isActive[1] && isReverse[1]);

								if (Check_Focuser || Check_Aperture)
								{
									for (int i = 0; i < 2; i++)
									{
										if (isActive[i] && isReverse[i])
										{
											TargetPos[i] += this.Backlash;
											isReverse[i] = false;
										}
										else
										{
											isActive[i] = false;
										}
									}

									this.moveStep = AiryMoveStep.MovePos;
								}
								else
								{
									this.moveStep = AiryMoveStep.Finish;
								}

								SaveLog($"Move Finish");

							}
							else
							{
								if (TM_Retry.IsTimeOut())
								{
									this.moveStep = AiryMoveStep.MovePos;
								}
							}
						}
						break;

					case AiryMoveStep.Finish:
						{
							flowStatus = UnitStatus.Finish;

							Thread.Sleep(1500);

							this.isMoveProc = false;
							SaveLog($"Move Flow Finish");
						}
						break;


					case AiryMoveStep.Alarm:
						{
							flowStatus = UnitStatus.Alarm;

							this.isMoveProc = false;
							SaveLog($"Move Flow Alarm", true);
						}
						break;
				}
			}
		}

		#endregion--- MoveFlow ---

		public virtual void SetBacklash(int Backlash)
		{
			this.Backlash = Backlash;
		}

		private enum ParaType
		{
			NowPos,
			TargetPos,
			HomeStatus,
			WorkCurrent,
		}

		public class HardwareInfo
		{
			public int NowPos;
			public int TargetPos;
			public bool HomeState = false;
			public int WorkCurrent = 0;
			public int Limit;
		}
	}
}