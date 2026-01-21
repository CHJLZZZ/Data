using CommonBase.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManager
{
    public class LightSourceA_Ctrl
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string baseUrl;

        private InfoManager info = null;

        private bool MotorX_Home = false;
        public bool IsHome { get => MotorX_Home; }

        private int MotorX_Pos = 0;
        public int Pos { get => MotorX_Pos; }

        public LightSourceA_Ctrl(string ipAddress, int port, InfoManager info)
        {
            this.info = info;
            baseUrl = $"http://{ipAddress}:{port}";
        }

        private void SaveLog(string Log, bool isAlm = false)
        {
            if (isAlm)
            {
                info.Error($"[Arduino] {Log}");
            }
            else
            {
                info.General($"[Arduino] {Log}");
            }
        }

        private async Task SendCommand(string command, ReturnPara RtnP = ReturnPara.None)
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}/{command}");
                response.EnsureSuccessStatusCode();

                if (RtnP != ReturnPara.None)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    switch (RtnP)
                    {
                        case ReturnPara.HomeStatus:
                            {
                                MotorX_Home = responseData == "1";
                            }
                            break;

                        case ReturnPara.Position:
                            {
                                MotorX_Pos = int.Parse(responseData);
                            }
                            break;
                    }
                }

                Console.WriteLine($"Command {command} sent successfully.");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error sending command {command}: {e.Message}");
            }
        }

        public Task EnableMotor(MotorType motor, bool enable)
        {
            string Motor = motor.ToString();
            string Enable = (enable) ? "ON" : "OFF";

            SaveLog($"Motor{Motor} {Enable}");
            return SendCommand($"EN{Motor}{Enable}");
        }

        public Task MoveForward(MotorType motor)
        {
            string Motor = motor.ToString();

            SaveLog($"Motor{Motor} Move Forward");
            return SendCommand($"{Motor}F");
        }

        public Task MoveBackward(MotorType motor)
        {
            string Motor = motor.ToString();

            SaveLog($"Motor{Motor} Move Backward");
            return SendCommand($"{Motor}B");
        }

        public Task MoveHome(MotorType motor)
        {
            string Motor = motor.ToString();

            SaveLog($"Motor{Motor} Home");
            return SendCommand($"{Motor}H");
        }

        public Task SetSteps(MotorType motor, int steps)
        {
            string Motor = motor.ToString();

            SaveLog($"Motor{Motor} Set Steps : {steps}");
            return SendCommand($"SET{Motor}?steps={steps}");
        }

        public bool CheckConnection()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl);
                request.Timeout = 5000; // 5 seconds timeout


                Console.WriteLine("Attempting to connect to " + baseUrl);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Received response: Status Code " + response.StatusCode);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    SaveLog("Request timed out. Arduino might be unavailable.", true);
                }
                else if (e.Response != null)
                {
                    SaveLog("Received error response: " + ((HttpWebResponse)e.Response).StatusCode, true);
                }
                else
                {
                    SaveLog("Error: " + e.Message, true);
                }
                return false;
            }
            catch (Exception e)
            {
                SaveLog("Unexpected error: " + e.Message, true);
                return false;
            }
        }

        public enum MotorType
        {
            X,
            Y,
            Z
        }

        public enum ReturnPara
        {
            None,
            HomeStatus,
            Position,
        }
    }

}
