
namespace HardwareManager
{
    partial class PowerSupply_Tool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.Tbx_SendMsg = new System.Windows.Forms.TextBox();
			this.Tbx_RecvMsg = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.Btn_Off = new System.Windows.Forms.Button();
			this.Btn_On = new System.Windows.Forms.Button();
			this.Tbx_Status_OnOff = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.Tmr_Query = new System.Windows.Forms.Timer(this.components);
			this.panel3 = new System.Windows.Forms.Panel();
			this.Btn_AutoSet_Stop = new System.Windows.Forms.Button();
			this.Num_CostTime = new System.Windows.Forms.NumericUpDown();
			this.Btn_AutoSet_FowardStart = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.Num_Current_End = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.Num_Current_Start = new System.Windows.Forms.NumericUpDown();
			this.Btn_RCL = new System.Windows.Forms.Button();
			this.Btn_Local = new System.Windows.Forms.Button();
			this.Btn_Remote = new System.Windows.Forms.Button();
			this.Btn_AutoSet_ReverseStart = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.Tbx_CMD = new System.Windows.Forms.TextBox();
			this.Btn_Send = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Num_CostTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Num_Current_End)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Num_Current_Start)).BeginInit();
			this.SuspendLayout();
			// 
			// Tbx_SendMsg
			// 
			this.Tbx_SendMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Tbx_SendMsg.Location = new System.Drawing.Point(88, 256);
			this.Tbx_SendMsg.Name = "Tbx_SendMsg";
			this.Tbx_SendMsg.ReadOnly = true;
			this.Tbx_SendMsg.Size = new System.Drawing.Size(295, 35);
			this.Tbx_SendMsg.TabIndex = 10;
			this.Tbx_SendMsg.Text = "-";
			this.Tbx_SendMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// Tbx_RecvMsg
			// 
			this.Tbx_RecvMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Tbx_RecvMsg.Location = new System.Drawing.Point(88, 297);
			this.Tbx_RecvMsg.Name = "Tbx_RecvMsg";
			this.Tbx_RecvMsg.ReadOnly = true;
			this.Tbx_RecvMsg.Size = new System.Drawing.Size(295, 35);
			this.Tbx_RecvMsg.TabIndex = 11;
			this.Tbx_RecvMsg.Text = "-";
			this.Tbx_RecvMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label3.Location = new System.Drawing.Point(7, 259);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 26);
			this.label3.TabIndex = 12;
			this.label3.Text = "Send : ";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label4.Location = new System.Drawing.Point(7, 300);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 26);
			this.label4.TabIndex = 13;
			this.label4.Text = "Recv : ";
			// 
			// Btn_Off
			// 
			this.Btn_Off.BackColor = System.Drawing.Color.Red;
			this.Btn_Off.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_Off.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_Off.Location = new System.Drawing.Point(89, 41);
			this.Btn_Off.Name = "Btn_Off";
			this.Btn_Off.Size = new System.Drawing.Size(80, 40);
			this.Btn_Off.TabIndex = 16;
			this.Btn_Off.Text = "Off";
			this.Btn_Off.UseVisualStyleBackColor = false;
			this.Btn_Off.Click += new System.EventHandler(this.Btn_Off_Click);
			// 
			// Btn_On
			// 
			this.Btn_On.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.Btn_On.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_On.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_On.Location = new System.Drawing.Point(3, 41);
			this.Btn_On.Name = "Btn_On";
			this.Btn_On.Size = new System.Drawing.Size(80, 40);
			this.Btn_On.TabIndex = 17;
			this.Btn_On.Text = "On";
			this.Btn_On.UseVisualStyleBackColor = false;
			this.Btn_On.Click += new System.EventHandler(this.Btn_On_Click);
			// 
			// Tbx_Status_OnOff
			// 
			this.Tbx_Status_OnOff.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Tbx_Status_OnOff.Location = new System.Drawing.Point(175, 45);
			this.Tbx_Status_OnOff.Name = "Tbx_Status_OnOff";
			this.Tbx_Status_OnOff.ReadOnly = true;
			this.Tbx_Status_OnOff.Size = new System.Drawing.Size(80, 35);
			this.Tbx_Status_OnOff.TabIndex = 22;
			this.Tbx_Status_OnOff.Text = "-";
			this.Tbx_Status_OnOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.label8);
			this.panel1.Controls.Add(this.Btn_On);
			this.panel1.Controls.Add(this.Btn_Off);
			this.panel1.Controls.Add(this.Tbx_Status_OnOff);
			this.panel1.Location = new System.Drawing.Point(8, 118);
			this.panel1.Margin = new System.Windows.Forms.Padding(5);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(260, 86);
			this.panel1.TabIndex = 38;
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.Color.Black;
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label8.Dock = System.Windows.Forms.DockStyle.Top;
			this.label8.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label8.ForeColor = System.Drawing.Color.White;
			this.label8.Location = new System.Drawing.Point(0, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(258, 38);
			this.label8.TabIndex = 31;
			this.label8.Text = "On / Off";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Tmr_Query
			// 
			this.Tmr_Query.Tick += new System.EventHandler(this.Tmr_Query_Tick);
			// 
			// panel3
			// 
			this.panel3.AutoSize = true;
			this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel3.Controls.Add(this.Btn_AutoSet_ReverseStart);
			this.panel3.Controls.Add(this.Btn_AutoSet_Stop);
			this.panel3.Controls.Add(this.Num_CostTime);
			this.panel3.Controls.Add(this.Btn_AutoSet_FowardStart);
			this.panel3.Controls.Add(this.label7);
			this.panel3.Controls.Add(this.label6);
			this.panel3.Controls.Add(this.label5);
			this.panel3.Controls.Add(this.Num_Current_End);
			this.panel3.Controls.Add(this.label1);
			this.panel3.Controls.Add(this.Num_Current_Start);
			this.panel3.Location = new System.Drawing.Point(8, 348);
			this.panel3.Margin = new System.Windows.Forms.Padding(5);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(417, 183);
			this.panel3.TabIndex = 40;
			// 
			// Btn_AutoSet_Stop
			// 
			this.Btn_AutoSet_Stop.BackColor = System.Drawing.Color.Red;
			this.Btn_AutoSet_Stop.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_AutoSet_Stop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_AutoSet_Stop.Location = new System.Drawing.Point(253, 135);
			this.Btn_AutoSet_Stop.Name = "Btn_AutoSet_Stop";
			this.Btn_AutoSet_Stop.Size = new System.Drawing.Size(90, 43);
			this.Btn_AutoSet_Stop.TabIndex = 168;
			this.Btn_AutoSet_Stop.Text = "Stop";
			this.Btn_AutoSet_Stop.UseVisualStyleBackColor = false;
			this.Btn_AutoSet_Stop.Click += new System.EventHandler(this.Btn_AutoSet_Stop_Click);
			// 
			// Num_CostTime
			// 
			this.Num_CostTime.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Num_CostTime.Location = new System.Drawing.Point(161, 86);
			this.Num_CostTime.Maximum = new decimal(new int[] {
            36000,
            0,
            0,
            0});
			this.Num_CostTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Num_CostTime.Name = "Num_CostTime";
			this.Num_CostTime.Size = new System.Drawing.Size(130, 35);
			this.Num_CostTime.TabIndex = 44;
			this.Num_CostTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.Num_CostTime.ThousandsSeparator = true;
			this.Num_CostTime.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
			// 
			// Btn_AutoSet_FowardStart
			// 
			this.Btn_AutoSet_FowardStart.BackColor = System.Drawing.Color.Blue;
			this.Btn_AutoSet_FowardStart.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_AutoSet_FowardStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_AutoSet_FowardStart.Location = new System.Drawing.Point(8, 135);
			this.Btn_AutoSet_FowardStart.Name = "Btn_AutoSet_FowardStart";
			this.Btn_AutoSet_FowardStart.Size = new System.Drawing.Size(90, 43);
			this.Btn_AutoSet_FowardStart.TabIndex = 167;
			this.Btn_AutoSet_FowardStart.Text = "緩升";
			this.Btn_AutoSet_FowardStart.UseVisualStyleBackColor = false;
			this.Btn_AutoSet_FowardStart.Click += new System.EventHandler(this.Btn_AutoSet_Start_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label7.Location = new System.Drawing.Point(3, 88);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(152, 26);
			this.label7.TabIndex = 43;
			this.label7.Text = "Cost Time (s) :";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label6.Location = new System.Drawing.Point(248, 43);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(28, 26);
			this.label6.TabIndex = 42;
			this.label6.Text = "~";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label5.Location = new System.Drawing.Point(3, 43);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(103, 26);
			this.label5.TabIndex = 41;
			this.label5.Text = "Current : ";
			// 
			// Num_Current_End
			// 
			this.Num_Current_End.DecimalPlaces = 3;
			this.Num_Current_End.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Num_Current_End.Location = new System.Drawing.Point(282, 41);
			this.Num_Current_End.Maximum = new decimal(new int[] {
            2999,
            0,
            0,
            0});
			this.Num_Current_End.Name = "Num_Current_End";
			this.Num_Current_End.Size = new System.Drawing.Size(130, 35);
			this.Num_Current_End.TabIndex = 32;
			this.Num_Current_End.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.Num_Current_End.Value = new decimal(new int[] {
            625,
            0,
            0,
            131072});
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Black;
			this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(415, 38);
			this.label1.TabIndex = 31;
			this.label1.Text = "Auto Set";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Num_Current_Start
			// 
			this.Num_Current_Start.DecimalPlaces = 3;
			this.Num_Current_Start.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Num_Current_Start.Location = new System.Drawing.Point(112, 41);
			this.Num_Current_Start.Maximum = new decimal(new int[] {
            2999,
            0,
            0,
            0});
			this.Num_Current_Start.Name = "Num_Current_Start";
			this.Num_Current_Start.Size = new System.Drawing.Size(130, 35);
			this.Num_Current_Start.TabIndex = 8;
			this.Num_Current_Start.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// Btn_RCL
			// 
			this.Btn_RCL.BackColor = System.Drawing.Color.Blue;
			this.Btn_RCL.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_RCL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_RCL.Location = new System.Drawing.Point(310, 67);
			this.Btn_RCL.Name = "Btn_RCL";
			this.Btn_RCL.Size = new System.Drawing.Size(90, 43);
			this.Btn_RCL.TabIndex = 168;
			this.Btn_RCL.Text = "RCL 1";
			this.Btn_RCL.UseVisualStyleBackColor = false;
			this.Btn_RCL.Click += new System.EventHandler(this.Btn_RCL_Click);
			// 
			// Btn_Local
			// 
			this.Btn_Local.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_Local.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_Local.ForeColor = System.Drawing.Color.Blue;
			this.Btn_Local.Location = new System.Drawing.Point(8, 67);
			this.Btn_Local.Name = "Btn_Local";
			this.Btn_Local.Size = new System.Drawing.Size(100, 43);
			this.Btn_Local.TabIndex = 169;
			this.Btn_Local.Text = "Local";
			this.Btn_Local.UseVisualStyleBackColor = false;
			this.Btn_Local.Click += new System.EventHandler(this.Btn_Local_Click);
			// 
			// Btn_Remote
			// 
			this.Btn_Remote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_Remote.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_Remote.ForeColor = System.Drawing.Color.Blue;
			this.Btn_Remote.Location = new System.Drawing.Point(114, 67);
			this.Btn_Remote.Name = "Btn_Remote";
			this.Btn_Remote.Size = new System.Drawing.Size(100, 43);
			this.Btn_Remote.TabIndex = 170;
			this.Btn_Remote.Text = "Remote";
			this.Btn_Remote.UseVisualStyleBackColor = false;
			this.Btn_Remote.Click += new System.EventHandler(this.Btn_Remote_Click);
			// 
			// Btn_AutoSet_ReverseStart
			// 
			this.Btn_AutoSet_ReverseStart.BackColor = System.Drawing.Color.Blue;
			this.Btn_AutoSet_ReverseStart.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_AutoSet_ReverseStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_AutoSet_ReverseStart.Location = new System.Drawing.Point(104, 135);
			this.Btn_AutoSet_ReverseStart.Name = "Btn_AutoSet_ReverseStart";
			this.Btn_AutoSet_ReverseStart.Size = new System.Drawing.Size(90, 43);
			this.Btn_AutoSet_ReverseStart.TabIndex = 169;
			this.Btn_AutoSet_ReverseStart.Text = "緩降";
			this.Btn_AutoSet_ReverseStart.UseVisualStyleBackColor = false;
			this.Btn_AutoSet_ReverseStart.Click += new System.EventHandler(this.Btn_AutoSet_ReverseStart_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label2.Location = new System.Drawing.Point(7, 218);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 26);
			this.label2.TabIndex = 172;
			this.label2.Text = "CMD : ";
			// 
			// Tbx_CMD
			// 
			this.Tbx_CMD.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Tbx_CMD.Location = new System.Drawing.Point(88, 215);
			this.Tbx_CMD.Name = "Tbx_CMD";
			this.Tbx_CMD.Size = new System.Drawing.Size(295, 35);
			this.Tbx_CMD.TabIndex = 171;
			this.Tbx_CMD.Text = "-";
			this.Tbx_CMD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// Btn_Send
			// 
			this.Btn_Send.BackColor = System.Drawing.Color.Blue;
			this.Btn_Send.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.Btn_Send.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Btn_Send.Location = new System.Drawing.Point(389, 210);
			this.Btn_Send.Name = "Btn_Send";
			this.Btn_Send.Size = new System.Drawing.Size(79, 43);
			this.Btn_Send.TabIndex = 173;
			this.Btn_Send.Text = "Send";
			this.Btn_Send.UseVisualStyleBackColor = false;
			this.Btn_Send.Click += new System.EventHandler(this.Btn_Send_Click);
			// 
			// PowerSupply_Tool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(474, 539);
			this.Controls.Add(this.Btn_Send);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.Tbx_CMD);
			this.Controls.Add(this.Btn_Remote);
			this.Controls.Add(this.Btn_Local);
			this.Controls.Add(this.Btn_RCL);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.Tbx_RecvMsg);
			this.Controls.Add(this.Tbx_SendMsg);
			this.MaximizeBox = false;
			this.Name = "PowerSupply_Tool";
			this.Sizable = false;
			this.Text = "Power Supply Tool";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.D65_Light_Tool_FormClosing);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Num_CostTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Num_Current_End)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Num_Current_Start)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox Tbx_SendMsg;
        private System.Windows.Forms.TextBox Tbx_RecvMsg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button Btn_Off;
        private System.Windows.Forms.Button Btn_On;
        private System.Windows.Forms.TextBox Tbx_Status_OnOff;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Timer Tmr_Query;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.NumericUpDown Num_CostTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown Num_Current_End;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown Num_Current_Start;
        private System.Windows.Forms.Button Btn_AutoSet_Stop;
        private System.Windows.Forms.Button Btn_AutoSet_FowardStart;
		private System.Windows.Forms.Button Btn_RCL;
		private System.Windows.Forms.Button Btn_Local;
		private System.Windows.Forms.Button Btn_Remote;
		private System.Windows.Forms.Button Btn_AutoSet_ReverseStart;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox Tbx_CMD;
		private System.Windows.Forms.Button Btn_Send;
	}
}