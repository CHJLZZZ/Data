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
using CommonSettings;

namespace HardwareManager
{

	#region --- Class[1] : AiryUnitCtrl_4in1_TCPIP ---

	public class AiryUnitCtrl_4in1_TCPIP : IMotorControl
	{
		enum AiryMoveStep
		{
			Init,
			CheckOriPos,
			GetOriPos,
			MovePos,
			CheckPos,
			CheckInPos,
			Finish,
			Alarm,
		}

		public event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;


		private InfoManager info;

		private Socket tcpClient;
		private int nPort = 8887;
		private string sIP = "192.168.3.7";
		private bool tcp_Connected = false;
		private EndPoint ipEndPoint = null;
		private Thread tcpReceivedData_Thread = null;
		private byte[] receivedData = new byte[2048];
		private int dataLength = 0;

		private bool isRefreshData = false;
		private bool isWork = false;
		public bool isMoveProc = false;
		public bool isCompensationMoveFinish_Focuser = false;
		public bool isCompensationMoveFinish_Aperture = false;

		private UnitStatus moveStatus_Focuser = UnitStatus.Idle;
		private UnitStatus moveStatus_Aperture = UnitStatus.Idle;
		private UnitStatus moveStatus_FW1 = UnitStatus.Idle;
		private UnitStatus moveStatus_FW2 = UnitStatus.Idle;
		private UnitStatus flowStatus = UnitStatus.Idle;



		private int Focuser_limitF = 140000;
		private int Focuser_limitR = 0;
		private int Aperture_limitF = 140000;


		private MotorInfo _Focuser = new MotorInfo();
		private MotorInfo _Aperture = new MotorInfo();
		private MotorInfo _FW1 = new MotorInfo();
		private MotorInfo _FW2 = new MotorInfo();

		private AiryMoveStep moveStep = AiryMoveStep.Init;

		public BaseTool.EnumMotoControlPackage Type => BaseTool.EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor;

		private int Backlash = 5000;
		public AiryUnitCtrl_4in1_TCPIP(InfoManager info)
		{

			this.info = info;
			this.isWork = false;
			this.tcp_Connected = false;

			this.tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress iPAddress = IPAddress.Parse(sIP);
			this.ipEndPoint = new IPEndPoint(iPAddress, nPort);

		}

		#region --- Interface 成員函式 ---

		#region --- MoveStatus_Focuser ---

		public UnitStatus MoveStatus_Focuser()
		{
			return this.moveStatus_Focuser;
		}

		#endregion --- MoveStatus_Focuser ---

		#region --- MoveStatus_Aperture ---

		public UnitStatus MoveStatus_Aperture()
		{
			return this.moveStatus_Aperture;
		}

		#endregion --- MoveStatus_Aperture ---

		#region --- MoveStatus_FW1 ---

		public UnitStatus MoveStatus_FW1()
		{
			return this.moveStatus_FW1;
		}

		#endregion --- MoveStatus_FW1 ---

		#region --- MoveStatus_FW2 ---

		public UnitStatus MoveStatus_FW2()
		{
			return this.moveStatus_FW2;
		}

		#endregion --- MoveStatus_FW2 ---

		#region --- MoveFlowStatus ---

		public UnitStatus MoveFlowStatus()
		{
			return this.flowStatus;
		}

		#endregion --- MoveStatus_FW2 ---

		#region --- IsWork ---

		public bool IsWork()
		{
			return this.isWork;
		}

		#endregion --- IsWork ---

		#region --- IsMoveProc ---

		public bool IsMoveProc()
		{
			return this.isMoveProc;
		}

		#endregion --- IsMoveProc ---        

		#region --- NowPos_Focuser ---

		public int NowPos_Focuser()
		{
			this.CheckDevice();
			return this._Focuser.Position;
		}

		#endregion --- NowPos_Focuser ---

		#region --- NowPos_Aperture ---

		public int NowPos_Aperture()
		{
			this.CheckDevice();
			return this._Aperture.Position;
		}

		#endregion --- NowPos_Aperture ---

		#region --- NowPos_FW1 ---

		public virtual int NowPos_FW1()
		{
			this.CheckDevice();
			return this._FW1.Position;
		}

		#endregion --- NowPos_FW1 ---

		#region --- NowPos_FW2 ---

		public virtual int NowPos_FW2()
		{
			this.CheckDevice();
			return this._FW2.Position;
		}

		#endregion --- NowPos_FW2 ---

		#region --- NowVelocity_Focuser ---

		public double NowVelocity_Focuser()
		{
			this.CheckDevice();
			return this._Focuser.Speed;
		}

		#endregion --- NowVelocity_Focuser ---

		#region --- NowVelocity_Aperture ---

		public double NowVelocity_Aperture()
		{
			this.CheckDevice();
			return this._Aperture.Speed;
		}

		#endregion --- NowVelocity_Aperture ---

		#region --- IsHome_Focuser ---

		public bool IsHome_Focuser()
		{
			this.CheckDevice();
			return this._Focuser.HomeState;
		}

		#endregion --- IsHome_Focuser ---

		#region --- IsHome_Aperture ---

		public bool IsHome_Aperture()
		{
			this.CheckDevice();
			return this._Aperture.HomeState;
		}

		#endregion --- IsHome_Aperture ---

		#region --- LimitF ---

		public int Focus_LimitF()
		{
			return this.Focuser_limitF;
		}

		public int Aperture_LimitF()
		{
			return this.Aperture_limitF;
		}

		#endregion --- LimitF ---

		#region --- LimitR ---

		public int Focus_LimitR()
		{
			return this.Focuser_limitR;
		}

		#endregion --- LimitR ---

		#endregion --- Property ---

		#region --- Method ---

		#region --- TCP_Connect ---
		private void TCP_Connect()
		{
			this.tcpClient.Connect(this.ipEndPoint);
			this.tcp_Connected = true;

			this.tcpReceivedData_Thread = new Thread(() => OnDataReceived());
			this.tcpReceivedData_Thread.Start();
		}

		#endregion

		#region --- TCP_Close ---
		private void TCP_Close()
		{
			this.tcp_Connected = false;
			this.tcpClient.Close();
		}
		#endregion --- TCP_Connect ---

		#region --- isConnect ---

		public bool isConnect()
		{
			return this.tcp_Connected;
		}

		#endregion --- isConnect ---

		#region --- SaveLog ---
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

		#endregion --- SaveLog ---

		#region --- SetSpeed ---
		private bool SetSpeed(MotorMember Member, double Speed)
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			switch (Member)
			{
				case MotorMember.Focuser:
					{
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":V{Speed}#"));
					}
					break;

				case MotorMember.Aperture:
					{
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":v{Speed}#"));
					}
					break;

				default:
					{
						return false;
					}
			}

			return true;
		}
		#endregion

		#region --- SetOverride ---
		private bool SetOverride(MotorMember Member, bool Switch)
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			switch (Member)
			{
				case MotorMember.Focuser:
					{
						if (Switch)
						{
							this.tcpClient.Send(Encoding.UTF8.GetBytes(":O1#"));
						}
						else
						{
							this.tcpClient.Send(Encoding.UTF8.GetBytes(":O0#"));
						}
					}
					break;

				case MotorMember.Aperture:
					{
						if (Switch)
						{
							this.tcpClient.Send(Encoding.UTF8.GetBytes(":o1#"));
						}
						else
						{
							this.tcpClient.Send(Encoding.UTF8.GetBytes(":o0#"));
						}
					}
					break;

				default:
					{
						return false;
					}
			}

			return true;
		}
		#endregion

		#region --- CheckDevice ---
		public bool CheckDevice()
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			this.isRefreshData = false;
			this.tcpClient.Send(Encoding.UTF8.GetBytes(":i#"));

			System.Threading.Thread.Sleep(200);

			return true;
		}
		#endregion

		#region --- Close/Open Led ---
		public bool CloseLed()
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			this.tcpClient.Send(Encoding.UTF8.GetBytes(":h#"));
			return true;
		}

		public bool OpenLed()
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			this.tcpClient.Send(Encoding.UTF8.GetBytes(":K#"));
			return true;
		}

		#endregion

		#region --- Open ---

		public void Open(string Com_Port)
		{

		}

		public void Open(string Com_Port, string Com_Port_2, string Com_Port_3)
		{

		}

		public void Open()
		{
			try
			{
				if (!this.tcp_Connected)
				{
					this.TCP_Connect();

					Thread.Sleep(500);
					CloseLed();
					this.isWork = true;

					SaveLog($"Connect");
				}
				else
				{
					this.TCP_Close();
					SaveLog($"Disconnect");
					this.TCP_Connect();
				}

				this.SetOverride(MotorMember.Focuser, false);
				Thread.Sleep(10);
				this.SetOverride(MotorMember.Aperture, false);

				//this.Home(AiryMember.All);

				SaveLog($"[Auto Focus]Connect");
				this.isWork = true;
			}
			catch (Exception ex)
			{
				SaveLog($"Port Open Fail - {ex.Message}", true);
			}
		}

		#endregion --- Open ---

		#region --- Close ---
		public void Close()
		{
			if (this.tcp_Connected)
			{
				this.TCP_Close();
				this.isWork = false;
			}
			this.isMoveProc = false;
		}

		#endregion --- Close ---

		#region --- OnDataReceived ---
		private void OnDataReceived()
		{
			while (this.tcp_Connected)
			{
				if (this.tcpClient.Available > 0)
				{
					this.dataLength = tcpClient.Receive(this.receivedData);
					string message = Encoding.UTF8.GetString(this.receivedData, 0, this.dataLength);
					string dataIn = message.Substring(2, message.Length - 4);
					string[] words = dataIn.Split(',');

					if (words.Length >= 20)
					{
						//POS: position
						//SPD: speed
						//PI: status of photointerrupter
						//STAT: status of home
						//OR: status of override 
						//LED PI: status of photointerrupter LED power
						try
						{
							_Aperture.Position = Convert.ToInt32(words[2]);
							_Aperture.Speed = Convert.ToDouble(words[3]);

							_Focuser.Position = Convert.ToInt32(words[0]);
							_Focuser.Speed = Convert.ToDouble(words[1]);

							// FW1 & FW2 
							_FW1.Position = Convert.ToInt32(words[6]);
							_FW1.Speed = Convert.ToDouble(words[5]);
							_FW2.Position = Convert.ToInt32(words[9]);
							_FW2.Speed = Convert.ToDouble(words[8]);

							// PinHomeState (不是用來判斷 Home)
							if (Convert.ToInt32(words[10]) == 5)
								_Focuser.PinHomeState = true;
							else
								_Focuser.PinHomeState = false;

							if (Convert.ToInt32(words[11]) == 5)
								_Aperture.PinHomeState = true;
							else
								_Aperture.PinHomeState = false;

							if (Convert.ToInt32(words[12]) == 5)
								_FW1.PinHomeState = true;
							else
								_FW1.PinHomeState = false;

							if (Convert.ToInt32(words[13]) == 5)
								_FW2.PinHomeState = true;
							else
								_FW2.PinHomeState = false;

							// HomeState
							if (Convert.ToInt32(words[14]) == 5)
								_Focuser.HomeState = true;
							else
								_Focuser.HomeState = false;

							if (Convert.ToInt32(words[15]) == 5)
								_Aperture.HomeState = true;
							else
								_Aperture.HomeState = false;

							if (Convert.ToInt32(words[16]) == 5)
								_FW1.HomeState = true;
							else
								_FW1.HomeState = false;

							if (Convert.ToInt32(words[17]) == 5)
								_FW2.HomeState = true;
							else
								_FW2.HomeState = false;

							// Override (用來設定是否可以過極限)
							if (Convert.ToInt32(words[18]) == 1)
								_Focuser.OverrideState = true;
							else
								_Focuser.OverrideState = false;

							if (Convert.ToInt32(words[19]) == 1)
								_Aperture.OverrideState = true;
							else
								_Aperture.OverrideState = false;

							if (Convert.ToInt32(words[20]) == 1)
								_FW1.OverrideState = true;
							else
								_FW1.OverrideState = false;

							if (Convert.ToInt32(words[21]) == 1)
								_FW2.OverrideState = true;
							else
								_FW2.OverrideState = false;

							// LED Power Status
							bool LED_PowerStatus = (Convert.ToInt32(words[22]) == 1);

							Focuser_limitF = Convert.ToInt32(words[23]);
							Aperture_limitF = Convert.ToInt32(words[24]);

							_Focuser.Limit = Focuser_limitF;
							_Aperture.Limit = Aperture_limitF;
							this.isRefreshData = true;

							UpdateStatus?.Invoke(_Focuser, _Aperture, _FW1, _FW2);

							System.Threading.Thread.Sleep(100);
						}
						catch (Exception ex)
						{
							SaveLog($"OnDataReceived Error : {ex.Message}");
						}
					}
				}
			}
		}
		#endregion --- OnDataReceived ---

		#region --- GetPosition ---

		public int GetPosition(MotorMember Member)
		{
			this.CheckDevice();

			switch (Member)
			{
				case MotorMember.Focuser:
					return this._Focuser.Position;

				case MotorMember.Aperture:
					return this._Aperture.Position;

				case MotorMember.FW1:
					return this._FW1.Position;

				case MotorMember.FW2:
					return this._FW2.Position;
			}

			return -1;
		}

		#endregion --- GetPosition ---

		#region --- GetVelocity ---

		public double GetVelocity(MotorMember Member)
		{
			this.CheckDevice();

			switch (Member)
			{
				case MotorMember.Focuser:
					return this._Focuser.Speed;

				case MotorMember.Aperture:
					return this._Aperture.Speed;

				case MotorMember.FW1:
					return this._FW1.Speed;

				case MotorMember.FW2:
					return this._FW2.Speed;
			}

			return -1;

		}
		#endregion --- GetVelocity ---

		#region --- Reset ---

		public void Reset(MotorMember Member)
		{
			this.CheckDevice();

			switch (Member)
			{
				case MotorMember.Focuser:
					DirectMove(Member, this._Focuser.Position);
					break;

				case MotorMember.Aperture:
					DirectMove(Member, this._Aperture.Position);
					break;
			}

		}
		#endregion --- Reset ---

		#region --- Home ---
		public bool Home(MotorMember Member = MotorMember.All)
		{
			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return true;
			}

			switch (Member)
			{
				case MotorMember.All:
					{
						Thread HomeFlow = new Thread(() =>
						{
							SaveLog("Home All");
							this.tcpClient.Send(Encoding.UTF8.GetBytes(":H5#"));

							do
							{
								this.CheckDevice();
								System.Threading.Thread.Sleep(100);
							} while (!this._Focuser.HomeState || !this._Aperture.HomeState || !this._FW1.HomeState || !this._FW2.HomeState);

							this.SetOverride(MotorMember.Focuser, false);
							this.SetOverride(MotorMember.Aperture, false);

							this.CloseLed();
						});
						HomeFlow.Start();

						return true;
					}

				default:
					{
						return false;
					}
			}
		}
		#endregion

		#region --- Jog ---"
		public void Jog(MotorMember Member, int Jog_Dist)
		{
			switch (Member)
			{
				case MotorMember.Focuser:
					{
						this.isMoveProc = false;

						Thread PreprocessThread = new Thread(() => Move_Proc(this._Focuser.Position + Jog_Dist, -1, -1, -1, 10000));
						PreprocessThread.Start();
					}
					break;

				case MotorMember.Aperture:
					{
						this.isMoveProc = false;

						Thread PreprocessThread = new Thread(() => Move_Proc(-1, this._Aperture.Position + Jog_Dist, -1, -1, 10000));
						PreprocessThread.Start();
					}
					break;
			}

		}
		#endregion --- Jog ---"

		#region --- Stop ---
		public void Stop(MotorMember device)
		{
			this.tcpClient.Send(Encoding.UTF8.GetBytes(":S#"));
			SaveLog("Stop.");

			Reset(device);
		}
		#endregion

		#region --- DirectMove ---

		public bool DirectMove(MotorMember Member, int Position)
		{


			if (!this.tcp_Connected)
			{
				SaveLog($"TCP is not Connected", true);
				return false;
			}

			switch (Member)
			{
				case MotorMember.Focuser:
					{
						if (Position > this.Focuser_limitF)
							return false; ;  //預防超過 140000
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":L{Position}#"));
					}
					break;

				case MotorMember.Aperture:
					{
						if (Position > this.Aperture_limitF)
							return false; ;  //預防超過 150000
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":l{Position}#"));
					}
					break;

				case MotorMember.FW1:
					{
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":F{Position}#"));
					}
					break;

				case MotorMember.FW2:
					{
						this.tcpClient.Send(Encoding.UTF8.GetBytes($":f{Position}#"));
					}
					break;

				default:
					{
						return false;
					}
			}

			System.Threading.Thread.Sleep(200);

			return true;
		}

		#endregion --- DirectMove ---

		#region --- Move(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut = 60000) ---
		public virtual void Move(int FocuserPos, int AperturePos, int FilterXyzPos, int FilterNdPos, int TimeOut = 60000)
		{

			if (!this.isMoveProc)
			{
				this.moveStatus_Focuser = UnitStatus.Idle;
				this.isMoveProc = true;

				if (GlobalConfig.艾里轉盤顛倒)
				{
					int Tmp = FilterXyzPos;
					FilterXyzPos = FilterNdPos;
					FilterNdPos = Tmp;
				}

				Thread PreprocessThread = new Thread(() => Move_Proc(FocuserPos, AperturePos, FilterXyzPos, FilterNdPos, TimeOut));
				PreprocessThread.Start();
			}
		}
		#endregion

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

							if (isActive[0])
								this.moveStatus_Focuser = UnitStatus.Running;
							else if (isActive[1])
								this.moveStatus_Aperture = UnitStatus.Running;
							else if (isActive[2])
								this.moveStatus_FW1 = UnitStatus.Running;
							else if (isActive[3])
								this.moveStatus_FW2 = UnitStatus.Running;

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
							bool Rtn = this.CheckDevice();

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
							if (isRefreshData)
							{
								int[] OriPos =
								{
									_Focuser.Position,
									_Aperture.Position
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
								this.moveStep = AiryMoveStep.CheckPos;
							}
							else
							{
								this.moveStep = AiryMoveStep.Alarm;
							}
						}
						break;

					case AiryMoveStep.CheckPos:
						{
							bool Rtn = this.CheckDevice();

							if (Rtn)
							{
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
							//this.moveStep = AiryMoveStep.Finish;

							bool Rtn = this.CheckDevice();
							Thread.Sleep(100);

							if (isRefreshData)
							{
								int[] GetPos =
								{
									_Focuser.Position,
									_Aperture.Position,
									_FW1.Position,
									_FW2.Position
								};

								bool[] InPos = new bool[4];

								InPos[0] = (GetPos[0] == TargetPos[0]) || (!isActive[0]);
								InPos[1] = (GetPos[1] == TargetPos[1]) || (!isActive[1]);
								InPos[2] = (GetPos[2] == TargetPos[2]) || (!isActive[2]);
								InPos[3] = (GetPos[3] == TargetPos[3]) || (!isActive[3]);

								// Check Finish
								if (InPos[0])
									this.moveStatus_Focuser = UnitStatus.Finish;
								if (InPos[1])
									this.moveStatus_Aperture = UnitStatus.Finish;
								if (InPos[2])
									this.moveStatus_FW1 = UnitStatus.Finish;
								if (InPos[3])
									this.moveStatus_FW2 = UnitStatus.Finish;

								if (InPos[0] && InPos[1] && InPos[2] && InPos[3])
								{
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
						}
						break;

					case AiryMoveStep.Finish:
						{
							flowStatus = UnitStatus.Finish;

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

		#endregion --- Method ---
	}

	#endregion --- Class[2] : AiryUnitCtrl_4in1_TCPIP ---

}