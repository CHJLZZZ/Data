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
    public partial class D65_Light_Tool : MaterialForm
    {
        private D65_Light_Ctrl D65;
        public D65_Light_Tool(ref D65_Light_Ctrl Ref)
        {
            InitializeComponent();

            this.D65 = Ref;

            this.D65.Update_SendMsg -= D65_Update_SendMsg;
            this.D65.Update_SendMsg += D65_Update_SendMsg;

            this.D65.Update_RecvMsg -= D65_Update_RecvMsg;
            this.D65.Update_RecvMsg += D65_Update_RecvMsg;

            this.D65.Update_Status -= D65_Update_Status;
            this.D65.Update_Status += D65_Update_Status;

            this.D65.Update_Brightness -= D65_Update_Brightness;
            this.D65.Update_Brightness += D65_Update_Brightness;
        }

        private void D65_Update_Brightness(int Brightness)
        {
            CrossThread.TbxEdit($"{Brightness}", Tbx_Brightness_Get);
        }

        private void D65_Update_Status(string Msg)
        {
            CrossThread.TbxEdit(Msg,Tbx_Status);
        }

        private void D65_Update_RecvMsg(string Msg)
        {
            CrossThread.TbxEdit(Msg, Tbx_RecvMsg);
        }

        private void D65_Update_SendMsg(string Msg)
        {
            CrossThread.TbxEdit(Msg, Tbx_SendMsg);
        }

        private void Btn_Send_Click(object sender, EventArgs e)
        {
           
            int Channel = 1;
            if (RBtn_Channel_1.Checked) Channel = 1;
            if (RBtn_Channel_2.Checked) Channel = 2;
            if (RBtn_Channel_3.Checked) Channel = 3;
            if (RBtn_Channel_4.Checked) Channel = 4;

            int Brightness = (int)Num_Brightness_Set.Value;

            this.D65.SetBrightnessFlow(Channel, Brightness);

            
        }

        private void D65_Light_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.D65.Update_SendMsg -= D65_Update_SendMsg;
            this.D65.Update_RecvMsg -= D65_Update_RecvMsg;
            this.D65.Update_Status -= D65_Update_Status;
        }

        private void Btn_On_Click(object sender, EventArgs e)
        {
            int Channel = 1;
            if (RBtn_Channel_1.Checked) Channel = 1;
            if (RBtn_Channel_2.Checked) Channel = 2;
            if (RBtn_Channel_3.Checked) Channel = 3;
            if (RBtn_Channel_4.Checked) Channel = 4;
            this.D65.SetSwitch(Channel,true);
        }

        private void Btn_Off_Click(object sender, EventArgs e)
        {
            int Channel = 1;
            if (RBtn_Channel_1.Checked) Channel = 1;
            if (RBtn_Channel_2.Checked) Channel = 2;
            if (RBtn_Channel_3.Checked) Channel = 3;
            if (RBtn_Channel_4.Checked) Channel = 4;
            this.D65.SetSwitch(Channel, false);
        }

        private void Btn_Read_Click(object sender, EventArgs e)
        {
            int Channel = 1;
            if (RBtn_Channel_1.Checked) Channel = 1;
            if (RBtn_Channel_2.Checked) Channel = 2;
            if (RBtn_Channel_3.Checked) Channel = 3;
            if (RBtn_Channel_4.Checked) Channel = 4;

            this.D65.QueryBrightness(Channel);
        }
    }
}
