using BaseTool;
using CommonBase.Logger;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareManager
{

    public class D65_Light_Ctrl
    {
        public event Action<string> Update_SendMsg;
        public event Action<string> Update_RecvMsg;
        public event Action<string> Update_Status;
        public event Action<int> Update_Brightness;

        private SerialPort My_SerialPort;
        private InfoManager info;
        private int GetBrightness = 0;

        private UnitStatus DeviceStatus = UnitStatus.Idle;
        public UnitStatus FunctionStatus { get => DeviceStatus; }

        private UnitStatus FlowStatus = UnitStatus.Idle;
        public UnitStatus Status { get => FlowStatus; }

        private bool FlowRun = false;
        public bool Run { get => FlowRun; }


        public bool IsConnect { get => My_SerialPort.IsOpen; }

        public D65_Light_Ctrl(InfoManager info)
        {
            this.info = info;
            My_SerialPort = new SerialPort();
            My_SerialPort.DataReceived -= My_SerialPort_DataReceived;
            My_SerialPort.DataReceived += My_SerialPort_DataReceived;
        }

        private void SaveLog(string Log, bool isAlm = false)
        {
            if (!isAlm)
            {
                info.General($"[D65] {Log}");
            }
            else
            {
                info.Error($"[D65] {Log}");
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
                My_SerialPort.PortName = "Com" + Comport.ToString();
                My_SerialPort.BaudRate = Baudrate;
                My_SerialPort.DataBits = 8;
                My_SerialPort.StopBits = StopBits.One;

                if (!My_SerialPort.IsOpen)
                {
                    //開啟 Serial Port
                    My_SerialPort.Open();
                }

                SaveLog($"Connect Success - COM{Comport}");

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


        private void My_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 接收數據
            SerialPort sp = (SerialPort)sender;
            string receivedData = sp.ReadLine();
            Update_RecvMsg?.Invoke(receivedData);

            bool CheckOK = receivedData.Contains("$");
            bool CheckNG = receivedData.Contains("&");

            if (CheckOK)
            {
                int Idx = receivedData.IndexOf("$");
                receivedData = receivedData.Substring(Idx);

                if (receivedData.Length > 2)
                {
                    string CMD = receivedData.Substring(1, 1);

                    if (CMD == "4") //Read
                    {
                        int Channel = Convert.ToInt16(receivedData.Substring(2, 1));
                        int Value = Convert.ToInt32(receivedData.Substring(3, 3), 16);

                        GetBrightness = Value;
                        Update_Brightness?.Invoke(Value);
                    }
                }

                DeviceStatus = UnitStatus.Finish;
                Update_Status?.Invoke(DeviceStatus.ToString());
            }

            if (CheckNG)
            {
                DeviceStatus = UnitStatus.Alarm;
                Update_Status?.Invoke(DeviceStatus.ToString());
            }

            //switch (receivedData)
            //{
            //    case "$":
            //        _Status = UnitStatus.Finish;
            //        Update_Status?.Invoke(_Status.ToString());

            //        break;

            //    case "&":
            //        _Status = UnitStatus.Alarm;
            //        Update_Status?.Invoke(_Status.ToString());
            //        break;
            //}
        }

        #region Function

        private void SendCMD(string CMD)
        {
            My_SerialPort.Write(CMD);
            DeviceStatus = UnitStatus.Running;
            Update_SendMsg?.Invoke(CMD);
            Update_Status?.Invoke(UnitStatus.Running.ToString());
        }

        private void SetBrightness(int Channel, int Brightness) //開關LED
        {
            int C = Channel;
            string B = Brightness.ToString("X2").PadLeft(3, '0');
            string CMD = $"$3{C}{B}17";
            SendCMD(CMD);
        }

        public void SetSwitch(int Channel, bool OnOff) //開關LED
        {
            int C = Channel;
            string CMD = (OnOff) ? $"$1{C}00017" : $"$2{C}00017";
            SendCMD(CMD);
        }

        public void QueryBrightness(int Channel) //開關LED
        {
            int C = Channel;
            string CMD = $"$4{C}00017";
            SendCMD(CMD);
        }

        #endregion

        public void SetBrightnessFlow(int Channel, int Brightness)
        {
            if (!FlowRun)
            {
                FlowRun = true;

                Thread Flow = new Thread(() =>
                {
                    FlowStatus = UnitStatus.Running;
                    int Step = 0;
                    int Now = 0;
                    int Target = Brightness;

                    double Timeout = 120*1000;
                    TimeManager TM = new TimeManager((int)Timeout);

                    int Dir = 1;

                    while (FlowRun)
                    {
                        if (TM.IsTimeOut())
                        {
                            FlowRun = false;
                            FlowStatus = UnitStatus.Alarm;
                            break;
                        }

                        switch (Step)
                        {
                            case 0: //Check Ori
                                {
                                    this.QueryBrightness(Channel);
                                    Step = 1;
                                }
                                break;

                            case 1:
                                {
                                    switch (DeviceStatus)
                                    {
                                        case UnitStatus.Finish:
                                            {
                                                Now = GetBrightness;

                                                if (Now == Target)
                                                {
                                                    FlowRun = false;
                                                    FlowStatus = UnitStatus.Finish;
                                                }
                                                else
                                                {
                                                    Dir = (Now < Target) ? 1 : -1;
                                                    Step = 2;
                                                }
                                            }
                                            break;

                                        case UnitStatus.Alarm:
                                            {
                                                FlowRun = false;
                                                FlowStatus = UnitStatus.Alarm;
                                            }
                                            break;
                                    }
                                }
                                break;

                            case 2:
                                {
                                    this.SetBrightness(Channel, Now);
                                    Step = 3;
                                }
                                break;

                            case 3:
                                {
                                    switch (DeviceStatus)
                                    {
                                        case UnitStatus.Finish:
                                            {
                                                if (Now == Target) //Finish
                                                {
                                                    FlowRun = false;
                                                    FlowStatus = UnitStatus.Finish;
                                                }
                                                else
                                                {
                                                    Thread.Sleep(100);
                                                    Now += 5 * (Dir);

                                                    if (Dir == 1 && Now > Target) Now = Target;
                                                    if (Dir == -1 && Now < Target) Now = Target;

                                                    Step = 2;
                                                }
                                            }
                                            break;

                                        case UnitStatus.Alarm:
                                            {
                                                FlowRun = false;
                                                FlowStatus = UnitStatus.Alarm;
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                });

                Flow.Start();
            }
            else
            {
                FlowRun = false;
                Thread.Sleep(100);
                SetBrightnessFlow(Channel, Brightness);
            }
        }
    }
}

