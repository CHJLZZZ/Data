using CommonBase.Logger;
using OphirLMMeasurementLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HardwareManager
{
    public class OphirLMMeasurement_Ctrl
    {
        InfoManager info = null;

        public enum SensorProperty { Range, Wavelength, Diffuser, Mode, Pulselength, Threshold, Filter, Trigger };
        public enum StreamMode { Turbo, TurboFrequency, Immediate };


        CoLMMeasurement lm_Co1;

        Dictionary<int, string> statusText = new Dictionary<int, string>();

        public string ErrorCode = "";
        public string ErrorMsg = "";
        public List<object> DeviceList = new List<object>();
        private List<string> HandleList = new List<string>();
        private int HandleIndex = 0;

        public string DeviceLabel = "";
        public string[] ChannelLabel = new string[4];

        public List<object>[] RangeList = new List<object>[4];
        public int[] RangeIndex = new int[4];

        public List<object>[] WavelengthList = new List<object>[4];
        public int[] WavelengthIndex = new int[4];

        public List<object>[] DiffuserList = new List<object>[4];
        public int[] DiffuserIndex = new int[4];

        public List<object>[] ModeList = new List<object>[4];
        public int[] ModeIndex = new int[4];

        public List<object>[] PulselengthList = new List<object>[4];
        public int[] PulselengthIndex = new int[4];

        public List<object>[] ThresholdList = new List<object>[4];
        public int[] ThresholdIndex = new int[4];

        public List<object>[] FilterList = new List<object>[4];
        public int[] FilterIndex = new int[4];

        public List<object>[] TriggerList = new List<object>[4];
        public int[] TriggerIndex = new int[4];

        public List<object> ExtTrigModeList = new List<object>();
        public int ExtTrigModeIndex = 0;
        public int ExtTrigWindowTime = 0;

        public int[] PfpPulseWidthMin = new int[4];
        public int[] PfpPulseWidthMax = new int[4];
        public int[] PfpPulseWidth = new int[4];

        public double[] LowFreqPowerPulseFreqMin = new double[4];
        public double[] LowFreqPowerPulseFreqMax = new double[4];
        public double[] LowFreqPowerPulseFreq = new double[4];

        public string ReadMsg = "";

        public string[] TimeStamp = new string[4];
        public string[] Measurement = new string[4];
        public string[] Status = new string[4];
        public string[] XPosition = new string[4];
        public string[] YPosition = new string[4];
        public string[] Size = new string[4];
        public string[] Frequency = new string[4];

        public OphirLMMeasurement_Ctrl(InfoManager info)
        {
            this.info = info;

            lm_Co1 = new OphirLMMeasurementLib.CoLMMeasurement();

            // Register delegates
            lm_Co1.DataReady += new OphirLMMeasurementLib._ICoLMMeasurementEvents_DataReadyEventHandler(this.DataReadyHandler);
            lm_Co1.PlugAndPlay += new OphirLMMeasurementLib._ICoLMMeasurementEvents_PlugAndPlayEventHandler(this.PlugAndPlayHandler);

            statusText = new Dictionary<int, string>();

            statusText.Add(0, "OK");
            statusText.Add(1, "OVERRANGE");
            statusText.Add(2, "SATURATED");
            statusText.Add(3, "MISSING PULSE");
            statusText.Add(4, "RESET STATE IN ENERGY MEASUREMENT");
            statusText.Add(5, "WAITING");
            statusText.Add(6, "SUMMING");
            statusText.Add(7, "TIMEOUT");
            statusText.Add(8, "PEAK OVER");
            statusText.Add(9, "ENERGY OVER");

            statusText.Add(0x010000, "X OK");                    // x position ok
            statusText.Add(0x010000 + 1, "X ERROR");                 // x position error
            statusText.Add(0x020000, "Y OK");                    // y position ok
            statusText.Add(0x020000 + 1, "Y ERROR");                 // y position error
            statusText.Add(0x030000, "SIZE OK");                 // size ok
            statusText.Add(0x030000 + 1, "SIZE ERROR");              // size error
            statusText.Add(0x030000 + 2, "SIZE WARNING");            // size warning
            statusText.Add(0x040000 + 1, "EVENT - SETTING CHANGED"); // event
            statusText.Add(0x050000, "FREQUENCY");               // frequency

            statusText.Add(0x100000, "TEMPERATURE");             // temperature
            statusText.Add(0x200000, "ALERT HOT");               // alert hot
            statusText.Add(0x300000, "PULSE WIDTH");             // pulse width
            statusText.Add(0x400000, "PFP ENERGY");              // PfP energy
        }

        public void Close()
        {

            try
            {
                lm_Co1.CloseAll();
            }
            catch
            {
            }
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            if (isAlm)
            {
                info.Error(Msg);
            }
            else
            {
                info.General(Msg);
            }
        }

        private void displayNoError()
        {
            ErrorCode = "No Error";
        }

        private void displayError(Exception ex)
        {
            ErrorCode = ex.Message;
        }

        public string GetError()
        {
            int nErrorCode;
            String errorString;

            try
            {
                // NOTE: expected string input format: "0x80040100".
                nErrorCode = Convert.ToInt32(ErrorCode, 16);
                lm_Co1.GetErrorFromCode(nErrorCode, out errorString);
                ErrorMsg = errorString;
            }
            catch (Exception ex)
            {
                displayError(ex);
                ErrorMsg = "";
            }

            return ErrorMsg;
        }

        public string GetLibraryVersion()
        {
            try
            {
                displayNoError();
                int version;
                lm_Co1.GetVersion(out version);
                return Convert.ToString(version);
            }
            catch (Exception ex)
            {
                displayError(ex);

                return "";
            }
        }

        public string GetDriverVersion()
        {
            try
            {
                displayNoError();
                string info;
                lm_Co1.GetDriverVersion(out info);
                return info;
            }
            catch (Exception ex)
            {
                displayError(ex);
                return "";
            }
        }

        public void ScanUsb()
        {
            try
            {
                displayNoError();
                Cursor.Current = Cursors.WaitCursor;
                Object serialNumbers;
                lm_Co1.ScanUSB(out serialNumbers);
                Cursor.Current = Cursors.Default;
                DeviceList.Clear();
                DeviceList.AddRange((Object[])serialNumbers);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void OpenDevice()
        {
            try
            {
                displayNoError();

                if (DeviceList.Count < 0) return;

                string snStr = DeviceList[0].ToString();
                if (snStr == "") return;

                int hDevice;
                lm_Co1.OpenUSBDevice(snStr, out hDevice);
                HandleList.Add(hDevice.ToString());
                HandleIndex = 0;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void CloseDevice()
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.Close(nHandle);
                HandleList.Remove(Convert.ToString(nHandle));
                HandleIndex = -1;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void UpdateInfoButton()
        {
            try
            {
                DeviceLabel = "";
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                String nameStr, romVersion, snStr, headType;
                lm_Co1.GetDeviceInfo(nHandle, out nameStr, out romVersion, out snStr);

                DeviceLabel = nameStr + " (" + snStr + ") " + romVersion;
                bool head_exists;
                for (int chan = 0; chan < 4; chan++)
                {
                    lm_Co1.IsSensorExists(nHandle, chan, out head_exists);
                    if (head_exists)
                    {
                        lm_Co1.GetSensorInfo(nHandle, chan, out snStr, out headType, out nameStr);
                        ChannelLabel[chan] = headType + "- " + nameStr + " (" + snStr + ")";
                    }
                    else
                        ChannelLabel[chan] = "No Head";
                }

            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void Reset()
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.ResetDevice(nHandle);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void ResetAll()
        {
            try
            {
                displayNoError();
                lm_Co1.ResetAllDevices();
                // when succeeded
                HandleList.Clear();   // clear handles combo contents
                DeviceList.Clear();    // clear devices list contents
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void GetRanges(int Channel)
        {
            getProperty(Channel, SensorProperty.Range);
        }

        public void GetWavelengths(int Channel)
        {
            getProperty(Channel, SensorProperty.Wavelength);
        }

        public void GetDiffusers(int Channel)
        {
            getProperty(Channel, SensorProperty.Diffuser);
        }

        public void GetModes(int Channel)
        {
            getProperty(Channel, SensorProperty.Mode);
        }

        public void GetPulseLengths(int Channel)
        {
            getProperty(Channel, SensorProperty.Pulselength);
        }

        public void GetThresholds(int Channel)
        {
            getProperty(Channel, SensorProperty.Threshold);
        }

        public void GetFilters(int Channel)
        {
            getProperty(Channel, SensorProperty.Filter);
        }

        public void GetTriggerOnOff(int Channel)
        {
            getProperty(Channel, SensorProperty.Trigger);
        }

        public void SetRange(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Range, Index);
        }

        public void SetWavelength(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Wavelength, Index);
        }

        public void SetDiffuser(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Diffuser, Index);
        }

        public void SetMode(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Mode, Index);
        }

        public void SetPulseLength(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Pulselength, Index);
        }

        public void SetThreshold(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Threshold, Index);
        }

        public void SetFilter(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Filter, Index);
        }

        public void SetTriggerOnOff(int Channel, int Index)
        {
            setProperty(Channel, SensorProperty.Trigger, Index);
        }

        public void ModifyWavelength(int nChannel, int index, int Value)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                bool modifiable;
                int wlMin, wlMax;
                lm_Co1.GetWavelengthsExtra(nHandle, nChannel, out modifiable, out wlMin, out wlMax);

                if (!modifiable)
                {
                    SaveLog("Wavelengths cannot be modified on this sensor.", true);
                    return;
                }

                lm_Co1.ModifyWavelength(nHandle, nChannel, index, Value);

                // Refresh list of wavelengths for this channel.
                getProperty(nChannel, SensorProperty.Wavelength);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void AddWavelength(int nChannel, int Value)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.AddWavelength(nHandle, nChannel, Value);

                // Refresh list of wavelengths for this channel.
                getProperty(nChannel, SensorProperty.Wavelength);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void DeleteWavelength(int nChannel, int index)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.DeleteWavelength(nHandle, nChannel, index);

                // Refresh list of wavelengths for this channel.
                getProperty(nChannel, SensorProperty.Wavelength);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void GetPfpPulseWidth(int nChannel)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                int value, min, max;
                lm_Co1.GetPulsedPowerPulseWidth(nHandle, nChannel, out value, out min, out max);

                PfpPulseWidthMin[nChannel] = min;
                PfpPulseWidthMax[nChannel] = max;
                PfpPulseWidth[nChannel] = value;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void SetPfpPulseWidth(int nChannel, int value)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.SetPulsedPowerPulseWidth(nHandle, nChannel, value);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void GetLowFreqPowerPulseFreq(int nChannel)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                double value, min, max;
                lm_Co1.GetLowFreqPowerPulseFreq(nHandle, nChannel, out value, out min, out max);

                LowFreqPowerPulseFreqMin[nChannel] = min;
                LowFreqPowerPulseFreqMax[nChannel] = max;
                LowFreqPowerPulseFreq[nChannel] = value;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void SetLowFreqPowerPulseFreq(int nChannel, double value)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.SetLowFreqPowerPulseFreq(nHandle, nChannel, value);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void GetExtTrigModes()
        {
            object options;
            int index;

            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.GetExtTrigModes(nHandle, out index, out options);

                ExtTrigModeList.Clear();
                ExtTrigModeList.AddRange((Object[])options);
                ExtTrigModeIndex = index;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void SetExtTrigModes(int nIndex)
        {
            int nHandle;

            try
            {
                displayNoError();
                ErrorCode = "";            // clear the Last Control Error Code
                nHandle = getCurrentDeviceHandle();

                lm_Co1.SetExtTrigMode(nHandle, nIndex);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void GetExtTrigWindow()
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                int window_time;
                lm_Co1.GetExtTrigWindowTime(nHandle, out window_time);
                ExtTrigWindowTime = window_time;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void ModifyTrigWindow(int nTime)
        {
            int nHandle;

            try
            {
                displayNoError();
                nHandle = getCurrentDeviceHandle();

                lm_Co1.SetExtTrigWindowTime(nHandle, nTime);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void Write(string Msg, bool ReadCheck)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.Write(nHandle, Msg);
                if (ReadCheck) Read();
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void Read()
        {
            try
            {
                ReadMsg = "";

                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                String reply;
                lm_Co1.Read(nHandle, out reply);
                ReadMsg = reply;
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void SaveSettings(int nChannel)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                lm_Co1.SaveSettings(nHandle, nChannel);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void CsConfigure(int nChannel, StreamMode Mode, int csValue)
        {
            try
            {
                displayNoError();
                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                int csMode = (int)Mode;

                lm_Co1.ConfigureStreamMode(nHandle, nChannel, csMode, csValue);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void StartCS(int nChannel)
        {
            bool exists;

            int nHandle = getCurrentDeviceHandle();
            if (nHandle == 0)
                return;

            displayNoError();

            ClearMeasurementsData();

            try
            {
                lm_Co1.IsSensorExists(nHandle, nChannel, out exists);
                if (!exists) return;

                lm_Co1.StartStream(nHandle, nChannel);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void StopCS(int nChannel)
        {
            bool exists;

            int nHandle = getCurrentDeviceHandle();
            if (nHandle == 0)
                return;

            displayNoError();
            try
            {
                lm_Co1.IsSensorExists(nHandle, nChannel, out exists);
                if (!exists) return;

                lm_Co1.StopStream(nHandle, nChannel);
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        public void StopAllCS()
        {
            try
            {
                displayNoError();
                lm_Co1.StopAllStreams();
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }


        private int getCurrentDeviceHandle()
        {
            int h;

            if (HandleList.Count < 1)
            {
                SaveLog("Open device and then choose a device handle from the combo box.", true);
                return 0;
            }

            h = Convert.ToInt32(HandleList[0]);

            if (h == 0)
            {
                SaveLog("Choose a device handle from the combo box.", true);
            }

            return h;
        }

        private void getProperty(int nChannel, SensorProperty prop)
        {
            try
            {
                displayNoError();

                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                // Tag property of Get button is set to the channel number
                // using the Property Editor.

                int index;
                Object options;

                switch (prop)
                {
                    case SensorProperty.Range:
                        lm_Co1.GetRanges(nHandle, nChannel, out index, out options);
                        RangeList[nChannel].Clear();
                        RangeList[nChannel].AddRange((Object[])options);
                        RangeIndex[nChannel] = index;
                        break;

                    case SensorProperty.Wavelength:
                        lm_Co1.GetWavelengths(nHandle, nChannel, out index, out options);
                        WavelengthList[nChannel].Clear();
                        WavelengthList[nChannel].AddRange((Object[])options);
                        WavelengthIndex[nChannel] = index;
                        break;

                    case SensorProperty.Diffuser:
                        lm_Co1.GetDiffuser(nHandle, nChannel, out index, out options);
                        DiffuserList[nChannel].Clear();
                        DiffuserList[nChannel].AddRange((Object[])options);
                        DiffuserIndex[nChannel] = index;
                        break;

                    case SensorProperty.Mode:
                        lm_Co1.GetMeasurementMode(nHandle, nChannel, out index, out options);
                        ModeList[nChannel].Clear();
                        ModeList[nChannel].AddRange((Object[])options);
                        ModeIndex[nChannel] = index;
                        break;

                    case SensorProperty.Pulselength:
                        lm_Co1.GetPulseLengths(nHandle, nChannel, out index, out options);
                        PulselengthList[nChannel].Clear();
                        PulselengthList[nChannel].AddRange((Object[])options);
                        PulselengthIndex[nChannel] = index;
                        break;

                    case SensorProperty.Threshold:
                        lm_Co1.GetThreshold(nHandle, nChannel, out index, out options);
                        ThresholdList[nChannel].Clear();
                        ThresholdList[nChannel].AddRange((Object[])options);
                        ThresholdIndex[nChannel] = index;
                        break;

                    case SensorProperty.Filter:
                        lm_Co1.GetFilter(nHandle, nChannel, out index, out options);
                        FilterList[nChannel].Clear();
                        FilterList[nChannel].AddRange((Object[])options);
                        FilterIndex[nChannel] = index;
                        break;

                    case SensorProperty.Trigger:
                        lm_Co1.GetExtTrigOnOff(nHandle, nChannel, out index, out options);
                        TriggerList[nChannel].Clear();
                        TriggerList[nChannel].AddRange((Object[])options);
                        TriggerIndex[nChannel] = index;
                        break;

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                displayError(ex);
            }

        }

        private void setProperty(int nChannel, SensorProperty prop, int index)
        {
            try
            {
                displayNoError();

                int nHandle = getCurrentDeviceHandle();
                if (nHandle == 0)
                    return;

                switch (prop)
                {
                    case SensorProperty.Range:
                        lm_Co1.SetRange(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Wavelength:
                        lm_Co1.SetWavelength(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Diffuser:
                        lm_Co1.SetDiffuser(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Mode:
                        lm_Co1.SetMeasurementMode(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Pulselength:
                        lm_Co1.SetPulseLength(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Threshold:
                        lm_Co1.SetThreshold(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Filter:
                        lm_Co1.SetFilter(nHandle, nChannel, index);
                        break;

                    case SensorProperty.Trigger:
                        lm_Co1.SetExtTrigOnOff(nHandle, nChannel, index);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        private void ClearMeasurementsData()
        {
            for (int channel = 0; channel < 4; channel++)
            {
                TimeStamp[channel] = "";
                Measurement[channel] = "";
                Status[channel] = "";
                XPosition[channel] = "";
                YPosition[channel] = "";
                Size[channel] = "";
                Frequency[channel] = "";
            }
        }


        private string GetStatus(int index)
        {
            if (statusText.ContainsKey(index))  // if unknown status - ignore it, else - get it
                return statusText[index];
            else
                return "";
        }

        private void DataReadyHandler(int hDevice, int channel)
        // Get the measured data from the OphirCOM object and display it
        {
            try
            {
                object dataArray;
                object timeStampArray;
                object statusArray;

                // Get the measured data from the OphirCOM object
                lm_Co1.GetData(hDevice, channel, out dataArray, out timeStampArray, out statusArray);

                if (HandleIndex < 0) return;
                if (Convert.ToInt32(HandleList[HandleIndex]) != hDevice) return;

                // Extract the data from the arrays 
                if (((double[])dataArray).Length > 0)
                {
                    double[] dataArr = (double[])dataArray;
                    double[] tsArr = (double[])timeStampArray;
                    int[] statusArr = (int[])statusArray;

                    // Initialize measured data from the current displayed data
                    string timestampStr = TimeStamp[channel];
                    string measurementStr = Measurement[channel];
                    string statusStr = Status[channel];
                    string xPositionStr = XPosition[channel];
                    string yPositionStr = YPosition[channel];
                    string sizeStr = Size[channel];
                    string frequencyStr = Frequency[channel];

                    // Values of the possible measurements types
                    int powerEnergyMeasurementType = 0x00;
                    int xPositionMeasurementType = 0x01;
                    int yPositionMeasurementType = 0x02;
                    int sizeMeasurementType = 0x03;
                    int eventMeasurementType = 0x04;
                    int frequencyMeasurementType = 0x05;
                    //  int temperatureMeasurementType  = 0x10;
                    //  int alertHotMeasurementType     = 0x20;
                    //  int pulseWidthMeasurementType   = 0x30;
                    //  int pfpEnergyMeasurementType    = 0x40;

                    // Values of the possible statuses
                    int okStatus = 0;
                    //  int errorStatus                 = 1;
                    int accuracyWarningStatus = 2;
                    //  int settingChangedStatus        = 1;

                    for (int ind = 0; ind < dataArr.Length; ind++)
                    {
                        timestampStr = tsArr[ind].ToString();

                        // Each int type element in statusArr[] holds in its two high
                        // bytes the measurement type and in the two low bytes the status.
                        // Extract these two values.
                        int measurementType = statusArr[ind] / 0x10000;// high bytes 
                        int status = statusArr[ind] % 0x10000;// low bytes

                        // Power or energy measurement
                        if (measurementType == powerEnergyMeasurementType)
                        {
                            measurementStr = dataArr[ind].ToString();
                            statusStr = GetStatus(statusArr[ind]);
                        }
                        // BeamTrack measurements
                        else if (measurementType == xPositionMeasurementType)   // X Position
                        {
                            if (status == okStatus)
                                xPositionStr = dataArr[ind].ToString();
                            else
                                xPositionStr = GetStatus(statusArr[ind]);
                        }
                        else if (measurementType == yPositionMeasurementType)  // Y Position
                        {
                            if (status == okStatus)
                                yPositionStr = dataArr[ind].ToString();
                            else
                                yPositionStr = GetStatus(statusArr[ind]);
                        }
                        else if (measurementType == sizeMeasurementType) // Size
                        {
                            if (status == okStatus || status == accuracyWarningStatus)
                                sizeStr = dataArr[ind].ToString();
                            else
                                sizeStr = GetStatus(statusArr[ind]);
                        }
                        else if (measurementType == eventMeasurementType)
                        {
                            measurementStr = dataArr[ind].ToString();
                            statusStr = GetStatus(statusArr[ind]);
                        }
                        else if (measurementType == frequencyMeasurementType)
                        {
                            frequencyStr = dataArr[ind].ToString();
                        }
                    }//for (int ind = 0;

                    // Display last measured data
                    Status[channel] = timestampStr;
                    Measurement[channel] = measurementStr;
                    Status[channel] = statusStr;
                    Status[channel] = xPositionStr;
                    Status[channel] = yPositionStr;
                    Status[channel] = sizeStr;
                    Status[channel] = frequencyStr;
                }//if (((double[])dataArray).Length > 0)
            }//try
            catch (Exception ex)
            {
                displayError(ex);
            }
        }

        void PlugAndPlayHandler()
        {
            SaveLog("Device has been removed from the USB.", true);
        }
    }
}
