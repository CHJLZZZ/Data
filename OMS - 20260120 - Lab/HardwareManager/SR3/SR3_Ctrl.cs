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
    public class SR3_Ctrl
    {
        public event Action<string> Update_SendMsg;
        public event Action<string> Update_RecvMsg;
        public event Action<string> Update_MeasureResult;
        public event Action Update_ResultClear;

        private SerialPort My_SerialPort;
        private InfoManager info;

        private ResultType MeasureType = ResultType.Colorimetric_SpectralRadiance;

        private UnitStatus MeasurementStatus = UnitStatus.Idle;
        public UnitStatus Status { get => MeasurementStatus; }

        private List<string> ST_Result = new List<string>();
        public List<string> Result { get => ST_Result; }

        private int ResultCnt_Chroma = 13;

        public bool IsConnect { get => My_SerialPort.IsOpen; }

        private TimeManager TM_Receive = new TimeManager();

        public SR3_Ctrl(InfoManager info)
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
                info.General($"[SR3] {Log}");
            }
            else
            {
                info.Error($"[SR3] {Log}");
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
                My_SerialPort.DataBits = 7;
                My_SerialPort.Parity = Parity.Odd;
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

        private void My_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 接收數據
            SerialPort sp = (SerialPort)sender;
            string receivedData = sp.ReadLine();

            receivedData = receivedData.Replace("\r", "");
            receivedData = receivedData.Replace("\n", "");

            Update_RecvMsg?.Invoke(receivedData);

            if (MeasurementStatus == UnitStatus.Running)
            {
                switch (MeasureType)
                {
                    case ResultType.Colorimetric:
                        {
                            double MeasureResult = 0.0;
                            bool IsMeasureResult = double.TryParse(receivedData, out MeasureResult);

                            if (IsMeasureResult)
                            {
                                ST_Result.Add(receivedData);

                                string RecordMsg = MergeMeasureResult(ST_Result.Count, receivedData);
                                Update_MeasureResult?.Invoke(RecordMsg);
                                TM_Receive.Reset();
                            }
                        }
                        break;

                    case ResultType.Colorimetric_SpectralRadiance:
                        {

                        }
                        break;
                }
            }

            switch (receivedData)
            {
                case "OK":
                    {

                    }
                    break;

                case "NO":
                    {
                        if (IsIgnoreError)
                        {
                            IsIgnoreError = false;
                        }
                        else if (MeasurementStatus == UnitStatus.Running)
                        {
                            MeasurementStatus = UnitStatus.Alarm;
                        }
                    }
                    break;

                case "END":
                    {
                        //如果Alarm(Timeout)，不更新為Finish
                        if (MeasurementStatus == UnitStatus.Running)
                        {
                            switch (MeasureType)
                            {
                                case ResultType.Colorimetric:
                                    {
                                        if (ST_Result.Count == ResultCnt_Chroma)
                                        {
                                            MeasurementStatus = UnitStatus.Finish;
                                        }
                                        else
                                        {
                                            MeasurementStatus = UnitStatus.Alarm;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        private string MergeMeasureResult(int Count, string Msg)
        {
            string MergeMsg = "";

            switch (Count)
            {
                case 001: MergeMsg = $"Measuring field (degree) = {Msg}"; break;
                case 002: MergeMsg = $"Integral time (milli-second) = {Msg}"; break;
                case 003: MergeMsg = $"Radiance = {Msg}"; break;
                case 004: MergeMsg = $"Luminance = {Msg}"; break;
                case 005: MergeMsg = $"Tristimulus values X = {Msg}"; break;
                case 006: MergeMsg = $"Tristimulus values Y = {Msg}"; break;
                case 007: MergeMsg = $"Tristimulus values Z = {Msg}"; break;
                case 008: MergeMsg = $"Chromaticity coordinates x = {Msg}"; break;
                case 009: MergeMsg = $"Chromaticity coordinates y = {Msg}"; break;
                case 010: MergeMsg = $"Chromaticity coordinates u' = {Msg}"; break;
                case 011: MergeMsg = $"Chromaticity coordinates v' = {Msg}"; break;
                case 012: MergeMsg = $"Correlated color temperature (K) = {Msg}"; break;
                case 013: MergeMsg = $"Deviation form B.B.L. = {Msg}"; break;

                default:
                    {
                        bool Check = (Count >= 014 && Count <= 414);
                        if (!Check) return "";

                        string[] SubMsg = Msg.Split(' ');

                        MergeMsg = $"Wavelength {SubMsg[0]}(nm), Sprctral radiance = {SubMsg[1]}";
                    }
                    break;
            }

            return $"[{Count:000}] {MergeMsg}";
        }

        #region Function

        private bool IsIgnoreError = false;

        private bool SendCMD(string CMD)
        {
            if (!My_SerialPort.IsOpen)
            {
                SaveLog($"Port is not Open", true);
                return false;
            }

            My_SerialPort.WriteLine($"{CMD}\r\n");
            Update_SendMsg?.Invoke(CMD);
            return true;
        }

        public bool SetMode(DeviceMode Mode)
        {
            IsIgnoreError = false;

            string CMD = (Mode == DeviceMode.Remote) ? "RM" : "LM";
            return SendCMD(CMD);
        }

        public bool SetResultType(ResultType Type)
        {
            IsIgnoreError = false;

            string CMD = (Type == ResultType.Colorimetric) ? "D1" : "D0";
            this.MeasureType = Type;
            return SendCMD(CMD);
        }

        public bool Retry()
        {
            IsIgnoreError = true;

            string CMD = "Retry";
            return SendCMD(CMD);
        }

        public bool Measurement(int Timeout = 30 * 1000)
        {
            IsIgnoreError = false;

            string CMD = "ST";

            MeasurementStatus = UnitStatus.Running;
            Update_ResultClear?.Invoke();
            ST_Result = new List<string>();

            bool Rtn = SendCMD(CMD);

            if (Rtn)
            {
                Thread.Sleep(100);

                TM_Receive.SetDelay(Timeout);

                Thread CheckRecive = new Thread(() =>
                {
                    while (MeasurementStatus == UnitStatus.Running)
                    {
                        if (TM_Receive.IsTimeOut())
                        {
                            MeasurementStatus = UnitStatus.Alarm;
                            break;
                        }
                    }
                });

                CheckRecive.Start();
            }

            return Rtn;
        }

        #endregion



        public enum DeviceMode
        {
            Remote,
            Local
        }

        public enum ResultType
        {
            Colorimetric,
            Colorimetric_SpectralRadiance,
        }


    }
}
