using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace HardwareManager
{
    public class PatGenBase
    {
        public int RetryLimit;
        public int CommCount;
        public bool IsConnect;

        public PatGenBase()
        {
            this.RetryLimit = 1;
            this.CommCount = 0;
            this.IsConnect = false;
        }

        public virtual void Initial()
        {

        }

        public virtual bool Open(string PortName, int BaudRate)
        {
            return false;
        }

        public virtual void Close()
        {

        }

        public virtual void SetColor(byte R, byte G, byte B)
        {

        }

        public virtual void Dispose()
        {

        }

    }


    public class PatGen_RS232 : PatGenBase
    {
        private SerialPort ComPort = null;
        public bool isRecive = false;
        // Com Pots
        private String[] ComPorts;

        #region --- Initial ---
        public override void Initial()
        {
            if (this.ComPort != null) this.ComPort = null;
            this.ComPort = new SerialPort();
        }
        #endregion

        #region --- Open ---
        public override bool Open(string PortName, int BaudRate = 57600)
        {
            this.ComPorts = SerialPort.GetPortNames();

            if (! this.ComPorts.Contains(PortName))
            {
                this.ComPort = null;
                this.IsConnect = false;
                return false;
            }
            else
            {
                if (this.ComPort.IsOpen)
                    this.ComPort.Close();

                // Setting
                this.ComPort.PortName = PortName;
                this.ComPort.BaudRate = BaudRate;
                this.ComPort.Parity = Parity.None;
                this.ComPort.DataBits = 8;
                this.ComPort.StopBits = StopBits.One;

                this.ComPort.Open();
                this.IsConnect = true;

                return true;
            }
        }
        #endregion

        #region --- Close ---
        public override void Close()
        {
            if (!this.IsConnect) return;

            this.ComPort.Close();
            this.IsConnect = false;
        }
        #endregion

        #region --- SetColor ---
        public override void SetColor(byte R, byte G, byte B)
        {
            byte[] buffer = { 0x46, 0x00, 0x00, 0x00 };  //0x46 : 是RD 訂的 Command flag

            buffer[1] = (byte)R;
            buffer[2] = (byte)G;
            buffer[3] = (byte)B;

            try
            {
                if (this.ComPort.IsOpen)
                {
                    this.isRecive = false;
                    this.ComPort.Write(buffer, 0, buffer.Length);             
                }
                else
                {
                    throw new Exception("Port is Close");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SetColor: {ex.Message}");
            }

        }
        #endregion

        #region --- comport_DataReceived ---
        private void comport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            this.isRecive = true;
        }
        #endregion 

        #region --- Dispose ---
        public override void Dispose()
        {
            if (this.ComPort.IsOpen)
            {
                this.ComPort.Close();
                this.ComPort = null;
            }
        }
        #endregion

    }

}