using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManager
{
    public class PatternGenerator : PatGenBase
    {
        public PatGen_RS232 PatGen_RS232 = null;

        private ePatGenCommType CommType;

        public PatternGenerator(string type)
        {
            switch (type.ToUpper())
            {
                case "RS232":
                {
                    this.CommType = ePatGenCommType.RS232;
                    if (this.PatGen_RS232 != null)
                        this.PatGen_RS232.Dispose();

                    this.PatGen_RS232 = new PatGen_RS232();
                    this.PatGen_RS232.Initial();
                }
                break;

                case "ETHERNET":
                {
                    this.CommType = ePatGenCommType.EtherNet;
                }
                break;
            }          

        }

        #region --- Connect ---
        public bool Connect(string PortName, int BaudRate)
        {
            switch (this.CommType)
            {
                case ePatGenCommType.RS232:
                    {
                        if (this.PatGen_RS232.IsConnect) return true;

                        return this.PatGen_RS232.Open(PortName, BaudRate);
                    }

                case ePatGenCommType.EtherNet:
                    {
                        return false;
                    }
            }

            return false;
        }
        #endregion

        #region --- Disconnect ---
        public void Disconnect()
        {
            switch (this.CommType)
            {
                case ePatGenCommType.RS232:
                    {
                        if (! this.PatGen_RS232.IsConnect) return;

                        this.PatGen_RS232.Close();
                    }
                    break;

                case ePatGenCommType.EtherNet:
                    {
                    }
                    break;
            }
        }

        #endregion

        #region --- SetPattern ---
        public void SetPattern(byte R, byte G, byte B)
        {
            switch (this.CommType)
            {
                case ePatGenCommType.RS232:
                    {
                        if (!this.PatGen_RS232.IsConnect) return;

                        this.PatGen_RS232.SetColor(R, G, B);
                    }
                    break;

                case ePatGenCommType.EtherNet:
                    {
                    }
                    break;
            }
        }

        #endregion

        #region --- Free ---
        public void Free()
        {
            switch (this.CommType)
            {
                case ePatGenCommType.RS232:
                    {
                        this.PatGen_RS232.Dispose();
                    }
                    break;

                case ePatGenCommType.EtherNet:
                    {
                    }
                    break;
            }
        }

        #endregion

    }




    #region --- enum ---

    #region --- ePatGenCommType ---
    public enum ePatGenCommType
    {
        RS232 = 0,
        EtherNet
    }
    #endregion



    #endregion
}
