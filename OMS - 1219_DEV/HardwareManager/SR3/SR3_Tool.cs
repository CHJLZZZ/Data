using BaseTool;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HardwareManager
{
    public partial class SR3_Tool : MaterialForm
    { 
        private SR3_Ctrl SR3 = null;

        public SR3_Tool(ref SR3_Ctrl SR3)
        {
            InitializeComponent();

            this.SR3 = SR3;

            this.SR3.Update_SendMsg -= SR3_Update_SendMsg;
            this.SR3.Update_SendMsg += SR3_Update_SendMsg;

            this.SR3.Update_RecvMsg -= SR3_Update_RecvMsg;
            this.SR3.Update_RecvMsg += SR3_Update_RecvMsg;

            this.SR3.Update_ResultClear -= SR3_Update_ResultClear;
            this.SR3.Update_ResultClear += SR3_Update_ResultClear;

            this.SR3.Update_MeasureResult -= SR3_Update_MeasureResult;
            this.SR3.Update_MeasureResult += SR3_Update_MeasureResult;
        }

        private void SR3_Update_ResultClear()
        {
            this.Invoke(new Action(() =>
            {
                Rtbx_Result.Text = "";
            }
          ));
        }

        private void SR3_Update_MeasureResult(string Msg)
        {
            this.Invoke(new Action(() =>
            {
                if (Rtbx_Result.Lines.Length == 0)
                {
                    Rtbx_Result.AppendText($"##### {DateTime.Now:yyyy/MM/dd HH:mm:ss}\r\n");
                }

                Rtbx_Result.AppendText($"{Msg}\r\n");
            }
            ));
        }

        private void SR3_Update_SendMsg(string Msg)
        {
            CrossThread.TbxEdit(Msg, Tbx_SendMsg);
        }

        private void SR3_Update_RecvMsg(string Msg)
        {
            CrossThread.TbxEdit(Msg, Tbx_RecvMsg);
        }

        private void Btn_LocalMode_Click(object sender, EventArgs e)
        {
            SR3.SetMode(SR3_Ctrl.DeviceMode.Local);
        }

        private void Btn_RemoteMode_Click(object sender, EventArgs e)
        {
            SR3.SetMode(SR3_Ctrl.DeviceMode.Remote);
        }

        private void Btn_SetMeasureType_Click(object sender, EventArgs e)
        {
            bool SpectralRadiance = Cbx_SpectralRadiance.Checked;

            if(SpectralRadiance)
            {
                SR3.SetResultType(SR3_Ctrl.ResultType.Colorimetric_SpectralRadiance);
            }
            else
            {
                SR3.SetResultType(SR3_Ctrl.ResultType.Colorimetric);
            }
        }

        private void Btn_Measurement_Click(object sender, EventArgs e)
        {
            SR3.Measurement();
        }

        private void Btn_Record_Click(object sender, EventArgs e)
        {
            // Create a SaveFileDialog to request a path and file name to save to.
            SaveFileDialog saveFile1 = new SaveFileDialog();

            // Initialize the SaveFileDialog to specify the RTF extension for the file.
            saveFile1.DefaultExt = "*.txt";
            saveFile1.Filter = "Txt Files|*.txt";
            saveFile1.FileName = $"SR-3A_{DateTime.Now:yyyyMMddHHmmss}";
            // Determine if the user selected a file name from the saveFileDialog.
            if (saveFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFile1.FileName.Length > 0)
            {
                Rtbx_Result.SaveFile(saveFile1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void SR3_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SR3.Update_SendMsg -= SR3_Update_SendMsg;

            this.SR3.Update_RecvMsg -= SR3_Update_RecvMsg;

            this.SR3.Update_ResultClear -= SR3_Update_ResultClear;

            this.SR3.Update_MeasureResult -= SR3_Update_MeasureResult;
        }
    }
}
