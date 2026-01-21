using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;

namespace HardwareManager
{
    public partial class OphirLMMeasurement_Tool : MaterialForm
    {
        private OphirLMMeasurement_Ctrl Ctrl = null;

        private int SelectedChannel = 0;

        public OphirLMMeasurement_Tool(OphirLMMeasurement_Ctrl Ctrl)
        {
            InitializeComponent();

            this.Ctrl = Ctrl;
        }

        public void UpdateUI()
        {
            int CH = SelectedChannel;

            FillComboBox(ModeComboBox, Ctrl.ModeList[CH], Ctrl.ModeIndex[CH]);
            FillComboBox(RangeComboBox, Ctrl.RangeList[CH], Ctrl.RangeIndex[CH]);
            FillComboBox(DiffComboBox, Ctrl.DiffuserList[CH], Ctrl.DiffuserIndex[CH]);
            FillComboBox(ThresholdComboBox, Ctrl.ThresholdList[CH], Ctrl.ThresholdIndex[CH]);
            FillComboBox(FilterComboBox, Ctrl.FilterList[CH], Ctrl.FilterIndex[CH]);
            FillComboBox(TriggerComboBox, Ctrl.TriggerList[CH], Ctrl.TriggerIndex[CH]);
            FillComboBox(PulselengthComboBox, Ctrl.PulselengthList[CH], Ctrl.PulselengthIndex[CH]);
            FillComboBox(WavelengthComboBox, Ctrl.WavelengthList[CH], Ctrl.WavelengthIndex[CH]);

            ResetTextBox(ModifyWavelengthTextBox);
            ResetTextBox(AddWavelengthTextBox);
            ResetNumericUpDown(DeleteWavelengthNumericUpDown);

            UpdateTextBox(PfpPulseWidthMinTextBox, Ctrl.PfpPulseWidthMin[CH].ToString());
            UpdateTextBox(PfpPulseWidthMaxTextBox, Ctrl.PfpPulseWidthMax[CH].ToString());
            UpdateTextBox(PfpPulseWidthTextBox, Ctrl.PfpPulseWidth[CH].ToString());

            UpdateTextBox(LowFreqPowerPulseFreqMinTextBox, Ctrl.LowFreqPowerPulseFreqMin[CH].ToString());
            UpdateTextBox(LowFreqPowerPulseFreqMaxTextBox, Ctrl.LowFreqPowerPulseFreqMax[CH].ToString());
            UpdateTextBox(LowFreqPowerPulseFreqTextBox, Ctrl.LowFreqPowerPulseFreq[CH].ToString());

            UpdateTextBox(TimeTextBox, Ctrl.TimeStamp[CH]);
            UpdateTextBox(MeasurementTextBox, Ctrl.Measurement[CH]);
            UpdateTextBox(StatusTextBox, Ctrl.Status[CH]);
            UpdateTextBox(FrequencyTexrBox, Ctrl.Frequency[CH]);
            UpdateTextBox(XPositionTextBox, Ctrl.XPosition[CH]);
            UpdateTextBox(YPositionTextBox, Ctrl.YPosition[CH]);
            UpdateTextBox(SizeTextBox, Ctrl.Size[CH]);
        }

        private void FillComboBox(ComboBox cb, List<Object> options, int ind)
        {
            if (cb.InvokeRequired)
            {
                cb.Invoke(new Action(() => FillComboBox(cb, options, ind)));
                return;
            }

            cb.Items.Clear();
            if (options.Count != 0)
            {
                cb.Items.AddRange(options.ToArray());
                cb.SelectedIndex = ind;
            }
            else
            {
                cb.Items.Add("N/A");
                cb.SelectedIndex = 0;
            }
        }

        private void ResetTextBox(TextBox tbx)
        {
            if (tbx.InvokeRequired)
            {
                tbx.Invoke(new Action(() => ResetTextBox(tbx)));
                return;
            }

            tbx.Text = "";
        }

        private void ResetNumericUpDown(NumericUpDown nud)
        {
            if (nud.InvokeRequired)
            {
                nud.Invoke(new Action(() => ResetNumericUpDown(nud)));
                return;
            }

            nud.Value = nud.Minimum;
        }

        private void UpdateTextBox(TextBox tbx, string Msg)
        {
            if (tbx.InvokeRequired)
            {
                tbx.Invoke(new Action(() => ResetTextBox(tbx)));
                return;
            }

            tbx.Text = Msg;
        }

        private void Rbtn_Channel_CheckedChanged(object sender, EventArgs e)
        {
            if (Rbtn_Channel0.Checked) SelectedChannel = 0;
            if (Rbtn_Channel1.Checked) SelectedChannel = 1;
            if (Rbtn_Channel2.Checked) SelectedChannel = 2;
            if (Rbtn_Channel3.Checked) SelectedChannel = 3;

            UpdateUI();
        }

        private void ModifyButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int Value = Convert.ToInt16(ModifyWavelengthTextBox.Text);
                Ctrl.ModifyWavelength(CH, WavelengthComboBox.SelectedIndex, Value);

                FillComboBox(WavelengthComboBox, Ctrl.WavelengthList[CH], Ctrl.WavelengthIndex[CH]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddWavelengthButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int Value = Convert.ToInt16(AddWavelengthTextBox.Text);
                Ctrl.AddWavelength(CH, Value);

                FillComboBox(WavelengthComboBox, Ctrl.WavelengthList[CH], Ctrl.WavelengthIndex[CH]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteWavelengthButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int Index = (int)DeleteWavelengthNumericUpDown.Value;
                Ctrl.DeleteWavelength(CH, Index);

                FillComboBox(WavelengthComboBox, Ctrl.WavelengthList[CH], Ctrl.WavelengthIndex[CH]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetPfpPulseWidthButton0_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int Value = Convert.ToInt16(PfpPulseWidthTextBox.Text);
                Ctrl.SetPfpPulseWidth(CH, Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetLowFreqPowerPulseFreqButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int Value = Convert.ToInt16(LowFreqPowerPulseFreqTextBox.Text);
                Ctrl.SetLowFreqPowerPulseFreq(CH, Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveSettButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                Ctrl.SaveSettings(CH);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StartCSButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                Ctrl.StartCS(CH);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopCSButton_Click(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                Ctrl.StopCS(CH);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.ModeIndex[CH];
                int SelectedIndex = ModeComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetMode(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RangeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.RangeIndex[CH];
                int SelectedIndex = RangeComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetRange(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DiffComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.DiffuserIndex[CH];
                int SelectedIndex = DiffComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetDiffuser(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ThresholdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.ThresholdIndex[CH];
                int SelectedIndex = ThresholdComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetThreshold(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.FilterIndex[CH];
                int SelectedIndex = FilterComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetFilter(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TriggerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.TriggerIndex[CH];
                int SelectedIndex = TriggerComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetTriggerOnOff(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PulselengthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.PulselengthIndex[CH];
                int SelectedIndex = PulselengthComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetPulseLength(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WavelengthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int CH = SelectedChannel;
                int CurrentIndex = Ctrl.WavelengthIndex[CH];
                int SelectedIndex = WavelengthComboBox.SelectedIndex;

                if (CurrentIndex != SelectedIndex)
                {
                    Ctrl.SetWavelength(CH, SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
