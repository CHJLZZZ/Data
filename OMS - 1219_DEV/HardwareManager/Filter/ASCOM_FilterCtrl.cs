using ASCOM.DriverAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HardwareManager
{
    public class ASCOM_FilterCtrl
    {
        //public event Action<string> SaveLog;
        string HW_NAME = "ASCOM.EFW2.FilterWheel";

        FilterWheel FW;
        short FilterWheelID;

        bool IsWheelReady = false;

        #region "--- Property ---"
        public bool IsReady
        {
            get
            {
                return IsWheelReady;
            }
            set
            {
                IsWheelReady = value;
            }
        }

        public bool is_FW_Work
        {
            get
            {
                if (FW != null)
                    return true;
                else
                    return false;
            }
        }

        public int Position
        {
            get
            {
                if (FW != null && FW.Connected)
                {
                    return FW.Position;
                }

                return -1;
            }
        }
        #endregion

        #region "--- Function ---"

        #region "--- Init ---"
        public void Init(string HW_NAME)
        {
            this.HW_NAME = HW_NAME;

            try
            {
                if (HW_NAME != "")
                {
                    if (FW == null)
                    {
                        FW = new FilterWheel(HW_NAME);
                    }

                    if (FW.Connected == true)
                    {
                        return;
                    }

                    FW.Connected = true;

                    FilterWheelID = FW.Position;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format($"Wheel : [{HW_NAME}] was not ready : {ex.Message} \r\n"));
            }
        }
        #endregion

        #region "--- SelectDevice ---"
        public void SelectDevice()
        {
            FilterWheel.Choose(HW_NAME);
        }
        #endregion

        #region "--- Close ---"
        public void Close()
        {
            if (FW == null)
            {
                return;
            }

            if (FW.Connected == false)
            {
                return;
            }

            FW.Connected = false;
            FW.Dispose();
            FW = null;
        }
        #endregion

        #region "--- ChangeWheel ---"
        public void ChangeWheel(int position)
        {
            IsWheelReady = false;

            if (FW.Position == (short)position)
            {
                IsWheelReady = true;
                return;
            }

            FilterWheelID = (short)position;
            FW.Position = FilterWheelID;

            Thread CheckThread = new Thread(new ThreadStart(CheckFilterPos));
            CheckThread.IsBackground = true;
            CheckThread.Priority = ThreadPriority.Highest;
            CheckThread.Start();
        }
        #endregion

        #region "--- CheckFilterPos ---"
        private void CheckFilterPos()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (FW.Position == FilterWheelID)
                {
                    IsWheelReady = true;
                    break;
                }
            }
            sw.Stop();

        }
        #endregion

        #endregion 
    }

}
