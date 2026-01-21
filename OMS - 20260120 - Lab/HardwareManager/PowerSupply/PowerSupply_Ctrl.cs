using BaseTool;
using CommonBase.Logger;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManager
{
    public class PowerSupply_Ctrl
    {
        public event Action<string> Update_SendMsg;
        public event Action<string> Update_RecvMsg;
        public event Action<string,double> Update_Status;

        private string OnOff = "";
        private double Current = 0.0;

        private SerialPort My_SerialPort;
        private InfoManager info;

        private UnitStatus DeviceStatus = UnitStatus.Idle;
        public UnitStatus Status { get => DeviceStatus; }

        public bool IsConnect { get => My_SerialPort.IsOpen; }

        public PowerSupply_Ctrl(InfoManager info)
        {
            this.info = info;
            My_SerialPort = new SerialPort();
            My_SerialPort.DataReceived -= My_SerialPort_DataReceived;
            My_SerialPort.DataReceived += My_SerialPort_DataReceived;
            //My_SerialPort.NewLine = "\n"; // SCPI 使用 LF 結尾
        }

        public void SaveLog(string Log, bool isAlm = false)
        {
            if (!isAlm) info.General($"[Power Supply] {Log}");
            else info.Error($"[Power Supply] {Log}");
        }

        public bool Connect(int Comport, int Baudrate = 9600)
        {
            try
            {
                if (My_SerialPort.IsOpen) My_SerialPort.Close();

                My_SerialPort.PortName = "COM" + Comport.ToString();
                My_SerialPort.BaudRate = Baudrate;
                //My_SerialPort.RtsEnable = true;
                My_SerialPort.DtrEnable = true;

                //My_SerialPort.DataBits = 8;
                //My_SerialPort.StopBits = StopBits.One;

                My_SerialPort.Open();
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
            try { My_SerialPort.Close(); }
            catch (Exception ex) { SaveLog(ex.Message, true); }
        }

        private void My_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = My_SerialPort.ReadLine();
            Update_RecvMsg?.Invoke(receivedData);
            DeviceStatus = UnitStatus.Finish;
        }

        public bool SendCMD(string CMD)
        {
            try
            {
                if (!My_SerialPort.IsOpen)
                {
                    SaveLog($"Port is not Open", true);
                    return false;
                }

                My_SerialPort.WriteLine(CMD);
                DeviceStatus = UnitStatus.Running;
                Update_SendMsg?.Invoke(CMD);

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Send Command Fail : {ex.Message}", true);
                return false;
            }

        }

        private string QueryCMD(string CMD)
        {
            My_SerialPort.WriteLine(CMD);
            string resp = My_SerialPort.ReadLine();
            Update_SendMsg?.Invoke(CMD);
            Update_RecvMsg?.Invoke(resp);
            return resp;
        }

        #region ABORt Subsystem
        /// <summary>
        /// 停止指定通道正在進行的輸出或序列
        /// PDF 灰底原文：ABORt (@<chanlist>)
        /// 範例：ABORt (@1) → 停止 CH1 的動作
        /// </summary>
        public void ABORt(int channel)
        {
            SendCMD($"ABORt (@{channel})");
        }

        /// <summary>
        /// 停止資料記錄 (Data Log)
        /// PDF 灰底原文：ABORt:DLOG
        /// 範例：ABORt:DLOG → 停止所有通道的資料記錄
        /// </summary>
        public void ABORt_DLOG()
        {
            SendCMD("ABORt:DLOG");
        }
        #endregion

        public void LocalMode()
        {
            SendCMD($"SYSTem:LOCal");
        }

        public void RemoteMode()
        {
            SendCMD($"SYSTem:REMote");
        }

        #region APPL Subsystem
        /// <summary>
        /// 設定指定通道的電壓與電流
        /// PDF 灰底原文：APPL CH1 | CH2 [ ,<voltage> | DEFault | MINimum | MAXimum [ ,<current> | DEFault | MINimum | MAXimum ] ]
        /// 範例：APPL CH1,5,1.5  或 APPL CH1,MAX,MAX
        /// </summary>
        public void APPL(int channel, string voltage, string current)
        {
            // 參數可傳入數值字串 (e.g. "5") 或關鍵字 ("MAX","MIN","DEFault")
            SendCMD($"APPL CH{channel},{voltage},{current}");
        }

        /// <summary>
        /// 查詢指定通道目前的設定值
        /// PDF 灰底原文：APPL? [CH1 | CH2]
        /// 範例：APPL? CH1
        /// </summary>
        public string APPL_Query(int channel)
        {
            return QueryCMD($"APPL? CH{channel}");
        }
        #endregion

        #region CALibration Subsystem
        /// <summary>
        /// 啟用或停用自動保存校正資料
        /// PDF 灰底原文：CALibration:ASAVE ON | OFF | 1 | 0
        /// 查詢：CALibration:ASAVE?
        /// </summary>
        public void CAL_ASAVE(bool enable)
        {
            SendCMD($"CALibration:ASAVE {(enable ? "ON" : "OFF")}");
        }
        public string CAL_ASAVE_Query()
        {
            return QueryCMD("CALibration:ASAVE?");
        }

        /// <summary>
        /// 查詢校正次數
        /// PDF 灰底原文：CALibration:COUNt?
        /// </summary>
        public string CAL_Count()
        {
            return QueryCMD("CALibration:COUNt?");
        }

        /// <summary>
        /// 設定高電流校正值
        /// PDF 灰底原文：CALibration:CURRent:DATA:HIGH <current>,(@<chanlist>)
        /// </summary>
        public void CAL_CurrentHigh(int channel, double current)
        {
            SendCMD($"CALibration:CURRent:DATA:HIGH {current},(@{channel})");
        }

        /// <summary>
        /// 設定低電流校正值
        /// PDF 灰底原文：CALibration:CURRent:DATA:LOW <current>,(@<chanlist>)
        /// </summary>
        public void CAL_CurrentLow(int channel, double current)
        {
            SendCMD($"CALibration:CURRent:DATA:LOW {current},(@{channel})");
        }

        /// <summary>
        /// 設定高電流校正範圍 (MIN/MAX)
        /// PDF 灰底原文：CALibration:CURRent:LEVel:HIGH MINimum | MAXimum,(@<chanlist>)
        /// </summary>
        public void CAL_CurrentLevelHigh(int channel, string level)
        {
            SendCMD($"CALibration:CURRent:LEVel:HIGH {level},(@{channel})");
        }

        /// <summary>
        /// 設定低電流校正範圍 (MIN/MAX)
        /// PDF 灰底原文：CALibration:CURRent:LEVel:LOW MINimum | MAXimum,(@<chanlist>)
        /// </summary>
        public void CAL_CurrentLevelLow(int channel, string level)
        {
            SendCMD($"CALibration:CURRent:LEVel:LOW {level},(@{channel})");
        }

        /// <summary>
        /// 設定校正日期
        /// PDF 灰底原文：CALibration:DATE "<string>"
        /// 查詢：CALibration:DATE?
        /// </summary>
        public void CAL_Date(string dateString)
        {
            SendCMD($"CALibration:DATE \"{dateString}\"");
        }
        public string CAL_Date_Query()
        {
            return QueryCMD("CALibration:DATE?");
        }

        /// <summary>
        /// 保存校正資料
        /// PDF 灰底原文：CALibration:SAVE
        /// </summary>
        public void CAL_Save()
        {
            SendCMD("CALibration:SAVE");
        }

        /// <summary>
        /// 設定安全碼
        /// PDF 灰底原文：CALibration:SECure:CODE <new code>
        /// </summary>
        public void CAL_SecureCode(string code)
        {
            SendCMD($"CALibration:SECure:CODE {code}");
        }

        /// <summary>
        /// 啟用或停用安全狀態
        /// PDF 灰底原文：CALibration:SECure:STATe ON | OFF | 1 | 0, <code>
        /// 查詢：CALibration:SECure:STATe?
        /// </summary>
        public void CAL_SecureState(bool enable, string code)
        {
            SendCMD($"CALibration:SECure:STATe {(enable ? "ON" : "OFF")},{code}");
        }
        public string CAL_SecureState_Query()
        {
            return QueryCMD("CALibration:SECure:STATe?");
        }

        /// <summary>
        /// 設定校正字串
        /// PDF 灰底原文：CALibration:STRing "<string>"
        /// 查詢：CALibration:STRing?
        /// </summary>
        public void CAL_String(string text)
        {
            SendCMD($"CALibration:STRing \"{text}\"");
        }
        public string CAL_String_Query()
        {
            return QueryCMD("CALibration:STRing?");
        }

        /// <summary>
        /// 設定電壓校正值
        /// PDF 灰底原文：CALibration:VOLTage:DATA <numeric value>,(@<chanlist>)
        /// </summary>
        public void CAL_Voltage(int channel, double voltage)
        {
            SendCMD($"CALibration:VOLTage:DATA {voltage},(@{channel})");
        }

        /// <summary>
        /// 設定電壓校正範圍 (MIN/MAX)
        /// PDF 灰底原文：CALibration:VOLTage:LEVel MINimum | MAXimum,(@<chanlist>)
        /// </summary>
        public void CAL_VoltageLevel(int channel, string level)
        {
            SendCMD($"CALibration:VOLTage:LEVel {level},(@{channel})");
        }
        #endregion

        #region CURRent Subsystem
        /// <summary>
        /// 設定立即電流
        /// PDF 灰底原文：CURRent:LEVel:IMMediate:AMPLitude <current> | MINimum | MAXimum | DEFault | UP | DOWN,(@<chanlist>)
        /// 範例：CURR 1.5,(@1) → 設定 CH1 電流為 1.5A
        /// </summary>
        public bool CURR_SetImmediate(double current)
        {
           return SendCMD($"CURR {current}");
        }

        /// <summary>
        /// 查詢立即電流
        /// PDF 灰底原文：CURRent:LEVel:IMMediate:AMPLitude?
        /// 範例：CURR? (@1)
        /// </summary>
        public double CURR_QueryImmediate()
        {
            string Rtn = QueryCMD($"CURR?");
            double Current = Convert.ToDouble(Rtn);

            return Current;
        }

        /// <summary>
        /// 設定電流步進大小
        /// PDF 灰底原文：CURRent:LEVel:IMMediate:STEP:INCRement <current> | DEFault,(@<chanlist>)
        /// 範例：CURR:STEP 0.1,(@1)
        /// </summary>
        public void CURR_SetStep(int channel, string step)
        {
            SendCMD($"CURR:STEP {step},(@{channel})");
        }

        /// <summary>
        /// 查詢電流步進大小
        /// PDF 灰底原文：CURRent:LEVel:IMMediate:STEP:INCRement?
        /// 範例：CURR:STEP? (@1)
        /// </summary>
        public string CURR_QueryStep(int channel)
        {
            return QueryCMD($"CURR:STEP? (@{channel})");
        }

        /// <summary>
        /// 設定觸發電流
        /// PDF 灰底原文：CURRent:LEVel:TRIGgered:AMPLitude <current> | MINimum | MAXimum,(@<chanlist>)
        /// 範例：CURR:TRIG 2.0,(@1)
        /// </summary>
        public void CURR_SetTriggered(int channel, string current)
        {
            SendCMD($"CURR:TRIG {current},(@{channel})");
        }

        /// <summary>
        /// 查詢觸發電流
        /// PDF 灰底原文：CURRent:LEVel:TRIGgered:AMPLitude?
        /// 範例：CURR:TRIG? (@1)
        /// </summary>
        public string CURR_QueryTriggered(int channel)
        {
            return QueryCMD($"CURR:TRIG? (@{channel})");
        }

        /// <summary>
        /// 設定電流模式 (FIXed | STEP | LIST)
        /// PDF 灰底原文：CURRent:MODE FIXed | STEP | LIST,(@<chanlist>)
        /// 範例：CURR:MODE FIX,(@1)
        /// </summary>
        public void CURR_SetMode(int channel, string mode)
        {
            SendCMD($"CURR:MODE {mode},(@{channel})");
        }

        /// <summary>
        /// 查詢電流模式
        /// PDF 灰底原文：CURRent:MODE?
        /// 範例：CURR:MODE? (@1)
        /// </summary>
        public string CURR_QueryMode(int channel)
        {
            return QueryCMD($"CURR:MODE? (@{channel})");
        }

        /// <summary>
        /// 設定過電流保護 (OCP) 閾值
        /// PDF 灰底原文：CURRent:PROTection:LEVel:AMPLitude <current> | MINimum | MAXimum,(@<chanlist>)
        /// 範例：CURR:PROT 2.5,(@1)
        /// </summary>
        public void CURR_SetProtectionLevel(int channel, string level)
        {
            SendCMD($"CURR:PROT {level},(@{channel})");
        }

        /// <summary>
        /// 查詢 OCP 閾值
        /// PDF 灰底原文：CURRent:PROTection:LEVel:AMPLitude?
        /// 範例：CURR:PROT? (@1)
        /// </summary>
        public string CURR_QueryProtectionLevel(int channel)
        {
            return QueryCMD($"CURR:PROT? (@{channel})");
        }

        /// <summary>
        /// 清除 OCP 狀態
        /// PDF 灰底原文：CURRent:PROTection:CLEar (@<chanlist>)
        /// 範例：CURR:PROT:CLEar (@1)
        /// </summary>
        public void CURR_ClearProtection(int channel)
        {
            SendCMD($"CURR:PROT:CLEar (@{channel})");
        }

        /// <summary>
        /// 設定 OCP 延遲時間
        /// PDF 灰底原文：CURRent:PROTection:DELay:TIME <time> | MINimum | MAXimum,(@<chanlist>)
        /// 範例：CURR:PROT:DELay 0.5,(@1)
        /// </summary>
        public void CURR_SetProtectionDelay(int channel, string time)
        {
            SendCMD($"CURR:PROT:DELay {time},(@{channel})");
        }

        /// <summary>
        /// 查詢 OCP 延遲時間
        /// PDF 灰底原文：CURRent:PROTection:DELay:TIME?
        /// 範例：CURR:PROT:DELay? (@1)
        /// </summary>
        public string CURR_QueryProtectionDelay(int channel)
        {
            return QueryCMD($"CURR:PROT:DELay? (@{channel})");
        }

        /// <summary>
        /// 啟用或停用 OCP
        /// PDF 灰底原文：CURRent:PROTection:STATe ON | OFF | 1 | 0,(@<chanlist>)
        /// 範例：CURR:PROT:STATe ON,(@1)
        /// </summary>
        public void CURR_SetProtectionState(int channel, bool enable)
        {
            SendCMD($"CURR:PROT:STATe {(enable ? "ON" : "OFF")},(@{channel})");
        }

        /// <summary>
        /// 查詢 OCP 狀態
        /// PDF 灰底原文：CURRent:PROTection:STATe?
        /// 範例：CURR:PROT:STATe? (@1)
        /// </summary>
        public string CURR_QueryProtectionState(int channel)
        {
            return QueryCMD($"CURR:PROT:STATe? (@{channel})");
        }

        /// <summary>
        /// 查詢 OCP 是否觸發
        /// PDF 灰底原文：CURRent:PROTection:TRIPped?
        /// 範例：CURR:PROT:TRIPped? (@1)
        /// </summary>
        public string CURR_QueryProtectionTripped(int channel)
        {
            return QueryCMD($"CURR:PROT:TRIPped? (@{channel})");
        }
        #endregion

        #region DIGital Subsystem
        /// <summary>
        /// 設定數位輸出資料
        /// PDF 灰底原文：DIGital:OUTPut:DATA <value>
        /// 範例：DIG:OUTP:DATA 255 → 設定輸出資料為 255
        /// </summary>
        public void DIG_OUTP_DATA(int value)
        {
            SendCMD($"DIG:OUTP:DATA {value}");
        }

        /// <summary>
        /// 查詢數位輸出資料
        /// PDF 灰底原文：DIGital:OUTPut:DATA?
        /// 範例：DIG:OUTP:DATA? → 回傳目前輸出資料
        /// </summary>
        public string DIG_OUTP_DATA_Query()
        {
            return QueryCMD("DIG:OUTP:DATA?");
        }

        /// <summary>
        /// 設定數位輸出 Pin 功能
        /// PDF 灰底原文：DIGital:OUTPut:PIN:FUNCtion <function>,<pin>
        /// 範例：DIG:OUTP:PIN:FUNC TRIG,1 → 設定 Pin1 為觸發功能
        /// </summary>
        public void DIG_OUTP_PIN_FUNC(string function, int pin)
        {
            SendCMD($"DIG:OUTP:PIN:FUNC {function},{pin}");
        }

        /// <summary>
        /// 查詢數位輸出 Pin 功能
        /// PDF 灰底原文：DIGital:OUTPut:PIN:FUNCtion?
        /// 範例：DIG:OUTP:PIN:FUNC? 1 → 查詢 Pin1 功能
        /// </summary>
        public string DIG_OUTP_PIN_FUNC_Query(int pin)
        {
            return QueryCMD($"DIG:OUTP:PIN:FUNC? {pin}");
        }

        /// <summary>
        /// 設定數位輸出 Pin 極性
        /// PDF 灰底原文：DIGital:OUTPut:PIN:POLarity NORMal | INVerted,<pin>
        /// 範例：DIG:OUTP:PIN:POL NORM,1 → 設定 Pin1 極性為正常
        /// </summary>
        public void DIG_OUTP_PIN_POL(string polarity, int pin)
        {
            SendCMD($"DIG:OUTP:PIN:POL {polarity},{pin}");
        }

        /// <summary>
        /// 查詢數位輸出 Pin 極性
        /// PDF 灰底原文：DIGital:OUTPut:PIN:POLarity?
        /// 範例：DIG:OUTP:PIN:POL? 1 → 查詢 Pin1 極性
        /// </summary>
        public string DIG_OUTP_PIN_POL_Query(int pin)
        {
            return QueryCMD($"DIG:OUTP:PIN:POL? {pin}");
        }

        /// <summary>
        /// 查詢數位輸入資料
        /// PDF 灰底原文：DIGital:INPut:DATA?
        /// 範例：DIG:INP:DATA? → 回傳數位輸入狀態
        /// </summary>
        public string DIG_INP_DATA_Query()
        {
            return QueryCMD("DIG:INP:DATA?");
        }
        #endregion

        #region DISPlay Subsystem
        /// <summary>
        /// 開啟或關閉顯示視窗
        /// PDF 灰底原文：DISPlay:WINDow:STATe ON | OFF | 1 | 0
        /// 範例：DISP:WIND:STAT ON → 開啟顯示
        /// </summary>
        public void DISP_WIND_STAT(bool enable)
        {
            SendCMD($"DISP:WIND:STAT {(enable ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 查詢顯示視窗狀態
        /// PDF 灰底原文：DISPlay:WINDow:STATe?
        /// 範例：DISP:WIND:STAT? → 回傳顯示狀態
        /// </summary>
        public string DISP_WIND_STAT_Query()
        {
            return QueryCMD("DISP:WIND:STAT?");
        }

        /// <summary>
        /// 設定顯示文字
        /// PDF 灰底原文：DISPlay:WINDow:TEXT:DATA "<string>"
        /// 範例：DISP:WIND:TEXT:DATA \"Hello\" → 在螢幕顯示 Hello
        /// </summary>
        public void DISP_WIND_TEXT_DATA(string text)
        {
            SendCMD($"DISP:WIND:TEXT:DATA \"{text}\"");
        }

        /// <summary>
        /// 查詢顯示文字
        /// PDF 灰底原文：DISPlay:WINDow:TEXT:DATA?
        /// 範例：DISP:WIND:TEXT:DATA? → 回傳目前顯示文字
        /// </summary>
        public string DISP_WIND_TEXT_DATA_Query()
        {
            return QueryCMD("DISP:WIND:TEXT:DATA?");
        }

        /// <summary>
        /// 開啟或關閉顯示文字功能
        /// PDF 灰底原文：DISPlay:WINDow:TEXT:STATe ON | OFF | 1 | 0
        /// 範例：DISP:WIND:TEXT:STAT ON → 開啟文字顯示
        /// </summary>
        public void DISP_WIND_TEXT_STAT(bool enable)
        {
            SendCMD($"DISP:WIND:TEXT:STAT {(enable ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 查詢顯示文字功能狀態
        /// PDF 灰底原文：DISPlay:WINDow:TEXT:STATe?
        /// 範例：DISP:WIND:TEXT:STAT? → 回傳文字顯示狀態
        /// </summary>
        public string DISP_WIND_TEXT_STAT_Query()
        {
            return QueryCMD("DISP:WIND:TEXT:STAT?");
        }
        #endregion

        #region FETCh Subsystem
        /// <summary>
        /// 擷取電壓量測值
        /// PDF 灰底原文：FETCh:VOLTage? (@<chanlist>)
        /// 範例：FETC:VOLT? (@1) → 擷取 CH1 的電壓量測值
        /// </summary>
        public string FETC_VOLT_Query(int channel)
        {
            return QueryCMD($"FETC:VOLT? (@{channel})");
        }

        /// <summary>
        /// 擷取電流量測值
        /// PDF 灰底原文：FETCh:CURRent? (@<chanlist>)
        /// 範例：FETC:CURR? (@1) → 擷取 CH1 的電流量測值
        /// </summary>
        public string FETC_CURR_Query(int channel)
        {
            return QueryCMD($"FETC:CURR? (@{channel})");
        }
        #endregion

        #region IEEE-488 Common Commands
        /// <summary>
        /// 識別儀器
        /// PDF 灰底原文：*IDN?
        /// 範例：*IDN? → 回傳製造商、型號、序號、韌體版本
        /// </summary>
        public string IEEE_IDN()
        {
            return QueryCMD("*IDN?");
        }

        /// <summary>
        /// 重置儀器
        /// PDF 灰底原文：*RST
        /// 範例：*RST → 將儀器回復到預設狀態
        /// </summary>
        public void IEEE_RST()
        {
            SendCMD("*RST");
        }

        /// <summary>
        /// 清除狀態
        /// PDF 灰底原文：*CLS
        /// 範例：*CLS → 清除狀態暫存器與錯誤佇列
        /// </summary>
        public void IEEE_CLS()
        {
            SendCMD("*CLS");
        }

        /// <summary>
        /// 查詢操作完成
        /// PDF 灰底原文：*OPC?
        /// 範例：*OPC? → 回傳 1 表示操作完成
        /// </summary>
        public string IEEE_OPC_Query()
        {
            return QueryCMD("*OPC?");
        }

        /// <summary>
        /// 設定操作完成旗標
        /// PDF 灰底原文：*OPC
        /// 範例：*OPC → 在操作完成時設定 OPC bit
        /// </summary>
        public void IEEE_OPC()
        {
            SendCMD("*OPC");
        }

        /// <summary>
        /// 儲存設定
        /// PDF 灰底原文：*SAV <n>
        /// 範例：*SAV 1 → 儲存目前設定到位置 1
        /// </summary>
        public void IEEE_SAV(int slot)
        {
            SendCMD($"*SAV {slot}");
        }

        /// <summary>
        /// 讀取設定
        /// PDF 灰底原文：*RCL <n>
        /// 範例：*RCL 1 → 從位置 1 讀取設定
        /// </summary>
        public void IEEE_RCL(int slot)
        {
            SendCMD($"*RCL {slot}");

        }

        /// <summary>
        /// 查詢狀態暫存器
        /// PDF 灰底原文：*STB?
        /// 範例：*STB? → 回傳狀態暫存器值
        /// </summary>
        public string IEEE_STB()
        {
            return QueryCMD("*STB?");
        }

        /// <summary>
        /// 查詢錯誤佇列
        /// PDF 灰底原文：*ESR?
        /// 範例：*ESR? → 回傳事件狀態暫存器值
        /// </summary>
        public string IEEE_ESR()
        {
            return QueryCMD("*ESR?");
        }
        #endregion

        #region INITiate Subsystem
        /// <summary>
        /// 啟動觸發序列
        /// PDF 灰底原文：INITiate:IMMediate (@<chanlist>)
        /// 範例：INIT:IMM (@1) → 啟動 CH1 的觸發序列
        /// </summary>
        public void INIT_IMM(int channel)
        {
            SendCMD($"INIT:IMM (@{channel})");
        }

        /// <summary>
        /// 啟動資料記錄 (Data Log)
        /// PDF 灰底原文：INITiate:DLOG
        /// 範例：INIT:DLOG → 啟動資料記錄
        /// </summary>
        public void INIT_DLOG()
        {
            SendCMD("INIT:DLOG");
        }
        #endregion

        #region INSTrument Subsystem
        /// <summary>
        /// 選擇指定的通道
        /// PDF 灰底原文：INSTrument:NSELect <channel>
        /// 範例：INST:NSEL 1 → 選擇 CH1
        /// </summary>
        public void INST_NSEL(int channel)
        {
            SendCMD($"INST:NSEL {channel}");
        }

        /// <summary>
        /// 查詢目前選擇的通道
        /// PDF 灰底原文：INSTrument:NSELect?
        /// 範例：INST:NSEL? → 回傳目前選擇的通道
        /// </summary>
        public string INST_NSEL_Query()
        {
            return QueryCMD("INST:NSEL?");
        }

        /// <summary>
        /// 選擇指定的儀器 (某些多輸出型號支援)
        /// PDF 灰底原文：INSTrument:SELect <instrument>
        /// 範例：INST:SEL 1 → 選擇儀器 1
        /// </summary>
        public void INST_SEL(int instrument)
        {
            SendCMD($"INST:SEL {instrument}");
        }

        /// <summary>
        /// 查詢目前選擇的儀器
        /// PDF 灰底原文：INSTrument:SELect?
        /// 範例：INST:SEL? → 回傳目前選擇的儀器
        /// </summary>
        public string INST_SEL_Query()
        {
            return QueryCMD("INST:SEL?");
        }
        #endregion

        #region LIST Subsystem
        /// <summary>
        /// 設定電流清單
        /// PDF 灰底原文：LIST:CURRent <list of currents>,(@<chanlist>)
        /// 範例：LIST:CURR 1.0,1.5,2.0,(@1)
        /// </summary>
        public void LIST_CURR(int channel, string currents)
        {
            SendCMD($"LIST:CURR {currents},(@{channel})");
        }

        /// <summary>
        /// 查詢電流清單
        /// PDF 灰底原文：LIST:CURRent?
        /// 範例：LIST:CURR? (@1)
        /// </summary>
        public string LIST_CURR_Query(int channel)
        {
            return QueryCMD($"LIST:CURR? (@{channel})");
        }

        /// <summary>
        /// 設定電壓清單
        /// PDF 灰底原文：LIST:VOLTage <list of voltages>,(@<chanlist>)
        /// 範例：LIST:VOLT 3,5,7,(@1)
        /// </summary>
        public void LIST_VOLT(int channel, string voltages)
        {
            SendCMD($"LIST:VOLT {voltages},(@{channel})");
        }

        /// <summary>
        /// 查詢電壓清單
        /// PDF 灰底原文：LIST:VOLTage?
        /// 範例：LIST:VOLT? (@1)
        /// </summary>
        public string LIST_VOLT_Query(int channel)
        {
            return QueryCMD($"LIST:VOLT? (@{channel})");
        }

        /// <summary>
        /// 設定清單步進次數
        /// PDF 灰底原文：LIST:COUNt <count>,(@<chanlist>)
        /// 範例：LIST:COUN 5,(@1)
        /// </summary>
        public void LIST_COUN(int channel, int count)
        {
            SendCMD($"LIST:COUN {count},(@{channel})");
        }

        /// <summary>
        /// 查詢清單步進次數
        /// PDF 灰底原文：LIST:COUNt?
        /// 範例：LIST:COUN? (@1)
        /// </summary>
        public string LIST_COUN_Query(int channel)
        {
            return QueryCMD($"LIST:COUN? (@{channel})");
        }

        /// <summary>
        /// 設定清單觸發源
        /// PDF 灰底原文：LIST:TRIGger:SOURce IMM | EXT | BUS,(@<chanlist>)
        /// 範例：LIST:TRIG:SOUR IMM,(@1)
        /// </summary>
        public void LIST_TRIG_SOUR(int channel, string source)
        {
            SendCMD($"LIST:TRIG:SOUR {source},(@{channel})");
        }

        /// <summary>
        /// 查詢清單觸發源
        /// PDF 灰底原文：LIST:TRIGger:SOURce?
        /// 範例：LIST:TRIG:SOUR? (@1)
        /// </summary>
        public string LIST_TRIG_SOUR_Query(int channel)
        {
            return QueryCMD($"LIST:TRIG:SOUR? (@{channel})");
        }

        /// <summary>
        /// 設定清單觸發延遲
        /// PDF 灰底原文：LIST:DELay <time>,(@<chanlist>)
        /// 範例：LIST:DEL 0.5,(@1)
        /// </summary>
        public void LIST_DEL(int channel, string time)
        {
            SendCMD($"LIST:DEL {time},(@{channel})");
        }

        /// <summary>
        /// 查詢清單觸發延遲
        /// PDF 灰底原文：LIST:DELay?
        /// 範例：LIST:DEL? (@1)
        /// </summary>
        public string LIST_DEL_Query(int channel)
        {
            return QueryCMD($"LIST:DEL? (@{channel})");
        }
        #endregion

        #region LXI Subsystem
        /// <summary>
        /// 設定 LXI 裝置名稱
        /// PDF 灰底原文：LXI:NAME "<string>"
        /// 查詢：LXI:NAME?
        /// 範例：LXI:NAME "PSU01"
        /// </summary>
        public void LXI_NAME(string name)
        {
            SendCMD($"LXI:NAME \"{name}\"");
        }
        public string LXI_NAME_Query()
        {
            return QueryCMD("LXI:NAME?");
        }

        /// <summary>
        /// 設定 LXI 狀態 (ON/OFF)
        /// PDF 灰底原文：LXI:STATe ON | OFF | 1 | 0
        /// 查詢：LXI:STATe?
        /// 範例：LXI:STAT ON
        /// </summary>
        public void LXI_STAT(bool enable)
        {
            SendCMD($"LXI:STAT {(enable ? "ON" : "OFF")}");
        }
        public string LXI_STAT_Query()
        {
            return QueryCMD("LXI:STAT?");
        }

        /// <summary>
        /// 設定 LXI 時間
        /// PDF 灰底原文：LXI:TIME "<string>"
        /// 查詢：LXI:TIME?
        /// 範例：LXI:TIME "2025-11-19 10:30:00"
        /// </summary>
        public void LXI_TIME(string timeString)
        {
            SendCMD($"LXI:TIME \"{timeString}\"");
        }
        public string LXI_TIME_Query()
        {
            return QueryCMD("LXI:TIME?");
        }
        #endregion

        #region MEASure Subsystem
        /// <summary>
        /// 量測電壓
        /// PDF 灰底原文：MEASure:VOLTage? (@<chanlist>)
        /// 範例：MEAS:VOLT? (@1) → 量測 CH1 電壓
        /// </summary>
        public string MEAS_VOLT_Query(int channel)
        {
            return QueryCMD($"MEAS:VOLT? (@{channel})");
        }

        /// <summary>
        /// 量測電流
        /// PDF 灰底原文：MEASure:CURRent? (@<chanlist>)
        /// 範例：MEAS:CURR? (@1) → 量測 CH1 電流
        /// </summary>
        public string MEAS_CURR_Query(int channel)
        {
            return QueryCMD($"MEAS:CURR? (@{channel})");
        }
        #endregion

        #region MMEMory Subsystem
        /// <summary>
        /// 儲存檔案
        /// PDF 灰底原文：MMEMory:STORe "<filename>"
        /// 範例：MMEM:STOR "setup1.sta"
        /// </summary>
        public void MMEM_STOR(string filename)
        {
            SendCMD($"MMEM:STOR \"{filename}\"");
        }

        /// <summary>
        /// 讀取檔案
        /// PDF 灰底原文：MMEMory:LOAD "<filename>"
        /// 範例：MMEM:LOAD "setup1.sta"
        /// </summary>
        public void MMEM_LOAD(string filename)
        {
            SendCMD($"MMEM:LOAD \"{filename}\"");
        }

        /// <summary>
        /// 刪除檔案
        /// PDF 灰底原文：MMEMory:DELete "<filename>"
        /// 範例：MMEM:DEL "setup1.sta"
        /// </summary>
        public void MMEM_DEL(string filename)
        {
            SendCMD($"MMEM:DEL \"{filename}\"");
        }

        /// <summary>
        /// 查詢目錄檔案清單
        /// PDF 灰底原文：MMEMory:CATalog?
        /// 範例：MMEM:CAT? → 回傳檔案清單
        /// </summary>
        public string MMEM_CAT_Query()
        {
            return QueryCMD("MMEM:CAT?");
        }

        /// <summary>
        /// 設定目前目錄
        /// PDF 灰底原文：MMEMory:CDIRectory "<path>"
        /// 查詢：MMEMory:CDIRectory?
        /// 範例：MMEM:CDIR "USER" → 切換到 USER 目錄
        /// </summary>
        public void MMEM_CDIR(string path)
        {
            SendCMD($"MMEM:CDIR \"{path}\"");
        }
        public string MMEM_CDIR_Query()
        {
            return QueryCMD("MMEM:CDIR?");
        }
        #endregion

        #region OUTPut Subsystem
        /// <summary>
        /// 開啟或關閉輸出
        /// PDF 灰底原文：OUTPut:STATe ON | OFF | 1 | 0,(@<chanlist>)
        /// 範例：OUTP ON,(@1) → 開啟 CH1 輸出
        /// </summary>
        public bool OUTP_STAT( bool enable)
        {
            return SendCMD($"OUTP {(enable ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 查詢輸出狀態
        /// PDF 灰底原文：OUTPut:STATe?
        /// 範例：OUTP? (@1) → 查詢 CH1 輸出狀態
        /// </summary>
        public string OUTP_STAT_Query(int channel)
        {
            string Rtn = QueryCMD($"OUTP?");

            OnOff = Rtn;

            return Rtn;
        }

        /// <summary>
        /// 設定輸出保護模式
        /// PDF 灰底原文：OUTPut:PROTection:MODE <mode>,(@<chanlist>)
        /// 範例：OUTP:PROT:MODE SAFE,(@1)
        /// </summary>
        public void OUTP_PROT_MODE(int channel, string mode)
        {
            SendCMD($"OUTP:PROT:MODE {mode},(@{channel})");
        }

        /// <summary>
        /// 查詢輸出保護模式
        /// PDF 灰底原文：OUTPut:PROTection:MODE?
        /// 範例：OUTP:PROT:MODE? (@1)
        /// </summary>
        public string OUTP_PROT_MODE_Query(int channel)
        {
            return QueryCMD($"OUTP:PROT:MODE? (@{channel})");
        }

        /// <summary>
        /// 設定輸出保護狀態 (ON/OFF)
        /// PDF 灰底原文：OUTPut:PROTection:STATe ON | OFF | 1 | 0,(@<chanlist>)
        /// 範例：OUTP:PROT:STAT ON,(@1)
        /// </summary>
        public void OUTP_PROT_STAT(int channel, bool enable)
        {
            SendCMD($"OUTP:PROT:STAT {(enable ? "ON" : "OFF")},(@{channel})");
        }

        /// <summary>
        /// 查詢輸出保護狀態
        /// PDF 灰底原文：OUTPut:PROTection:STATe?
        /// 範例：OUTP:PROT:STAT? (@1)
        /// </summary>
        public string OUTP_PROT_STAT_Query(int channel)
        {
            return QueryCMD($"OUTP:PROT:STAT? (@{channel})");
        }
        #endregion

        #region SENSe Subsystem
        /// <summary>
        /// 設定電壓量測範圍
        /// PDF 灰底原文：SENSe:VOLTage:RANGe <range>,(@<chanlist>)
        /// 範例：SENS:VOLT:RANG 10,(@1)
        /// </summary>
        public void SENS_VOLT_RANG(int channel, string range)
        {
            SendCMD($"SENS:VOLT:RANG {range},(@{channel})");
        }

        /// <summary>
        /// 查詢電壓量測範圍
        /// PDF 灰底原文：SENSe:VOLTage:RANGe?
        /// 範例：SENS:VOLT:RANG? (@1)
        /// </summary>
        public string SENS_VOLT_RANG_Query(int channel)
        {
            return QueryCMD($"SENS:VOLT:RANG? (@{channel})");
        }

        /// <summary>
        /// 設定電流量測範圍
        /// PDF 灰底原文：SENSe:CURRent:RANGe <range>,(@<chanlist>)
        /// 範例：SENS:CURR:RANG 2,(@1)
        /// </summary>
        public void SENS_CURR_RANG(int channel, string range)
        {
            SendCMD($"SENS:CURR:RANG {range},(@{channel})");
        }

        /// <summary>
        /// 查詢電流量測範圍
        /// PDF 灰底原文：SENSe:CURRent:RANGe?
        /// 範例：SENS:CURR:RANG? (@1)
        /// </summary>
        public string SENS_CURR_RANG_Query(int channel)
        {
            return QueryCMD($"SENS:CURR:RANG? (@{channel})");
        }

        /// <summary>
        /// 設定量測平均次數
        /// PDF 灰底原文：SENSe:AVERage:COUNt <count>,(@<chanlist>)
        /// 範例：SENS:AVER:COUN 10,(@1)
        /// </summary>
        public void SENS_AVER_COUN(int channel, int count)
        {
            SendCMD($"SENS:AVER:COUN {count},(@{channel})");
        }

        /// <summary>
        /// 查詢量測平均次數
        /// PDF 灰底原文：SENSe:AVERage:COUNt?
        /// 範例：SENS:AVER:COUN? (@1)
        /// </summary>
        public string SENS_AVER_COUN_Query(int channel)
        {
            return QueryCMD($"SENS:AVER:COUN? (@{channel})");
        }

        /// <summary>
        /// 啟用或停用量測平均
        /// PDF 灰底原文：SENSe:AVERage:STATe ON | OFF | 1 | 0,(@<chanlist>)
        /// 範例：SENS:AVER:STAT ON,(@1)
        /// </summary>
        public void SENS_AVER_STAT(int channel, bool enable)
        {
            SendCMD($"SENS:AVER:STAT {(enable ? "ON" : "OFF")},(@{channel})");
        }

        /// <summary>
        /// 查詢量測平均狀態
        /// PDF 灰底原文：SENSe:AVERage:STATe?
        /// 範例：SENS:AVER:STAT? (@1)
        /// </summary>
        public string SENS_AVER_STAT_Query(int channel)
        {
            return QueryCMD($"SENS:AVER:STAT? (@{channel})");
        }

        /// <summary>
        /// 查詢過電壓保護是否觸發
        /// PDF 灰底原文：SENSe:VOLTage:PROTection:TRIPped?
        /// 範例：SENS:VOLT:PROT:TRIP? (@1)
        /// </summary>
        public string SENS_VOLT_PROT_TRIP_Query(int channel)
        {
            return QueryCMD($"SENS:VOLT:PROT:TRIP? (@{channel})");
        }

        /// <summary>
        /// 查詢過電流保護是否觸發
        /// PDF 灰底原文：SENSe:CURRent:PROTection:TRIPped?
        /// 範例：SENS:CURR:PROT:TRIP? (@1)
        /// </summary>
        public string SENS_CURR_PROT_TRIP_Query(int channel)
        {
            return QueryCMD($"SENS:CURR:PROT:TRIP? (@{channel})");
        }
        #endregion

        #region STATus Subsystem
        /// <summary>
        /// 查詢事件狀態暫存器
        /// PDF 灰底原文：STATus:QUEStionable:EVENt?
        /// 範例：STAT:QUES:EVEN? → 回傳事件暫存器值
        /// </summary>
        public string STAT_QUES_EVEN_Query()
        {
            return QueryCMD("STAT:QUES:EVEN?");
        }

        /// <summary>
        /// 查詢問詢遮罩暫存器
        /// PDF 灰底原文：STATus:QUEStionable:ENABle?
        /// 範例：STAT:QUES:ENAB? → 回傳問詢遮罩值
        /// </summary>
        public string STAT_QUES_ENAB_Query()
        {
            return QueryCMD("STAT:QUES:ENAB?");
        }

        /// <summary>
        /// 設定問詢遮罩暫存器
        /// PDF 灰底原文：STATus:QUEStionable:ENABle <value>
        /// 範例：STAT:QUES:ENAB 16 → 設定遮罩值
        /// </summary>
        public void STAT_QUES_ENAB(int value)
        {
            SendCMD($"STAT:QUES:ENAB {value}");
        }

        /// <summary>
        /// 查詢操作狀態暫存器
        /// PDF 灰底原文：STATus:OPERation:EVENt?
        /// 範例：STAT:OPER:EVEN? → 回傳操作事件暫存器
        /// </summary>
        public string STAT_OPER_EVEN_Query()
        {
            return QueryCMD("STAT:OPER:EVEN?");
        }

        /// <summary>
        /// 查詢操作問詢遮罩暫存器
        /// PDF 灰底原文：STATus:OPERation:ENABle?
        /// 範例：STAT:OPER:ENAB? → 回傳操作遮罩值
        /// </summary>
        public string STAT_OPER_ENAB_Query()
        {
            return QueryCMD("STAT:OPER:ENAB?");
        }

        /// <summary>
        /// 設定操作問詢遮罩暫存器
        /// PDF 灰底原文：STATus:OPERation:ENABle <value>
        /// 範例：STAT:OPER:ENAB 1 → 設定操作遮罩值
        /// </summary>
        public void STAT_OPER_ENAB(int value)
        {
            SendCMD($"STAT:OPER:ENAB {value}");
        }

        /// <summary>
        /// 查詢操作狀態暫存器
        /// PDF 灰底原文：STATus:OPERation:CONDition?
        /// 範例：STAT:OPER:COND? → 回傳操作狀態暫存器
        /// </summary>
        public string STAT_OPER_COND_Query()
        {
            return QueryCMD("STAT:OPER:COND?");
        }

        /// <summary>
        /// 查詢問詢暫存器
        /// PDF 灰底原文：STATus:QUEStionable:CONDition?
        /// 範例：STAT:QUES:COND? → 回傳問詢暫存器
        /// </summary>
        public string STAT_QUES_COND_Query()
        {
            return QueryCMD("STAT:QUES:COND?");
        }
        #endregion

        #region SYSTem Subsystem
        /// <summary>
        /// 查詢錯誤佇列
        /// PDF 灰底原文：SYSTem:ERRor?
        /// 範例：SYST:ERR? → 回傳錯誤代碼與描述
        /// </summary>
        public string SYST_ERR_Query()
        {
            return QueryCMD("SYST:ERR?");
        }

        /// <summary>
        /// 查詢韌體版本
        /// PDF 灰底原文：SYSTem:VERSion?
        /// 範例：SYST:VERS? → 回傳韌體版本
        /// </summary>
        public string SYST_VERS_Query()
        {
            return QueryCMD("SYST:VERS?");
        }

        /// <summary>
        /// 設定語言
        /// PDF 灰底原文：SYSTem:LANGuage <language>
        /// 查詢：SYSTem:LANGuage?
        /// 範例：SYST:LANG SCPI
        /// </summary>
        public void SYST_LANG(string language)
        {
            SendCMD($"SYST:LANG {language}");
        }
        public string SYST_LANG_Query()
        {
            return QueryCMD("SYST:LANG?");
        }

        /// <summary>
        /// 控制蜂鳴器
        /// PDF 灰底原文：SYSTem:BEEPer:STATe ON | OFF | 1 | 0
        /// 查詢：SYSTem:BEEPer:STATe?
        /// 範例：SYST:BEEP:STAT ON
        /// </summary>
        public void SYST_BEEP_STAT(bool enable)
        {
            SendCMD($"SYST:BEEP:STAT {(enable ? "ON" : "OFF")}");
        }
        public string SYST_BEEP_STAT_Query()
        {
            return QueryCMD("SYST:BEEP:STAT?");
        }

        /// <summary>
        /// 發出蜂鳴聲
        /// PDF 灰底原文：SYSTem:BEEPer
        /// 範例：SYST:BEEP → 立即發出蜂鳴聲
        /// </summary>
        public void SYST_BEEP()
        {
            SendCMD("SYST:BEEP");
        }
        #endregion

        #region TRIGger Subsystem
        /// <summary>
        /// 設定觸發源
        /// PDF 灰底原文：TRIGger:SOURce IMM | EXT | BUS,(@<chanlist>)
        /// 範例：TRIG:SOUR IMM,(@1) → 設定 CH1 觸發源為立即
        /// </summary>
        public void TRIG_SOUR(int channel, string source)
        {
            SendCMD($"TRIG:SOUR {source},(@{channel})");
        }

        /// <summary>
        /// 查詢觸發源
        /// PDF 灰底原文：TRIGger:SOURce?
        /// 範例：TRIG:SOUR? (@1)
        /// </summary>
        public string TRIG_SOUR_Query(int channel)
        {
            return QueryCMD($"TRIG:SOUR? (@{channel})");
        }

        /// <summary>
        /// 設定觸發延遲
        /// PDF 灰底原文：TRIGger:DELay <time>,(@<chanlist>)
        /// 範例：TRIG:DEL 0.5,(@1) → 設定 CH1 觸發延遲為 0.5 秒
        /// </summary>
        public void TRIG_DEL(int channel, string time)
        {
            SendCMD($"TRIG:DEL {time},(@{channel})");
        }

        /// <summary>
        /// 查詢觸發延遲
        /// PDF 灰底原文：TRIGger:DELay?
        /// 範例：TRIG:DEL? (@1)
        /// </summary>
        public string TRIG_DEL_Query(int channel)
        {
            return QueryCMD($"TRIG:DEL? (@{channel})");
        }

        /// <summary>
        /// 啟動觸發
        /// PDF 灰底原文：TRIGger:IMMediate (@<chanlist>)
        /// 範例：TRIG:IMM (@1) → 啟動 CH1 觸發
        /// </summary>
        public void TRIG_IMM(int channel)
        {
            SendCMD($"TRIG:IMM (@{channel})");
        }
        #endregion

        #region Triggering Commands
        /// <summary>
        /// 啟動觸發 (立即觸發)
        /// PDF 灰底原文：*TRG
        /// 範例：*TRG → 啟動觸發序列
        /// </summary>
        public void IEEE_TRG()
        {
            SendCMD("*TRG");
        }

        #endregion

        #region VOLTage Subsystem
        /// <summary>
        /// 設定輸出電壓
        /// PDF 灰底原文：VOLTage <voltage>,(@<chanlist>)
        /// 範例：VOLT 5,(@1) → 設定 CH1 電壓為 5V
        /// </summary>
        public void VOLT(int channel, string voltage)
        {
            SendCMD($"VOLT {voltage},(@{channel})");
        }

        /// <summary>
        /// 查詢輸出電壓
        /// PDF 灰底原文：VOLTage?
        /// 範例：VOLT? (@1) → 查詢 CH1 電壓
        /// </summary>
        public string VOLT_Query(int channel)
        {
            return QueryCMD($"VOLT? (@{channel})");
        }

        /// <summary>
        /// 設定過電壓保護值 (OVP)
        /// PDF 灰底原文：VOLTage:PROTection <voltage>,(@<chanlist>)
        /// 範例：VOLT:PROT 6,(@1) → 設定 CH1 OVP 為 6V
        /// </summary>
        public void VOLT_PROT(int channel, string voltage)
        {
            SendCMD($"VOLT:PROT {voltage},(@{channel})");
        }

        /// <summary>
        /// 查詢過電壓保護值 (OVP)
        /// PDF 灰底原文：VOLTage:PROTection?
        /// 範例：VOLT:PROT? (@1)
        /// </summary>
        public string VOLT_PROT_Query(int channel)
        {
            return QueryCMD($"VOLT:PROT? (@{channel})");
        }

        /// <summary>
        /// 啟用或停用過電壓保護 (OVP)
        /// PDF 灰底原文：VOLTage:PROTection:STATe ON | OFF | 1 | 0,(@<chanlist>)
        /// 範例：VOLT:PROT:STAT ON,(@1)
        /// </summary>
        public void VOLT_PROT_STAT(int channel, bool enable)
        {
            SendCMD($"VOLT:PROT:STAT {(enable ? "ON" : "OFF")},(@{channel})");
        }

        /// <summary>
        /// 查詢過電壓保護狀態
        /// PDF 灰底原文：VOLTage:PROTection:STATe?
        /// 範例：VOLT:PROT:STAT? (@1)
        /// </summary>
        public string VOLT_PROT_STAT_Query(int channel)
        {
            return QueryCMD($"VOLT:PROT:STAT? (@{channel})");
        }
        #endregion

    }
}
