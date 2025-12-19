
namespace HardwareManager
{
    partial class D65_Light_Tool
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
            this.RBtn_Channel_1 = new System.Windows.Forms.RadioButton();
            this.RBtn_Channel_4 = new System.Windows.Forms.RadioButton();
            this.RBtn_Channel_3 = new System.Windows.Forms.RadioButton();
            this.RBtn_Channel_2 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Num_Brightness_Set = new System.Windows.Forms.NumericUpDown();
            this.Btn_Set = new System.Windows.Forms.Button();
            this.Tbx_SendMsg = new System.Windows.Forms.TextBox();
            this.Tbx_RecvMsg = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.Tbx_Status = new System.Windows.Forms.TextBox();
            this.Btn_Off = new System.Windows.Forms.Button();
            this.Btn_On = new System.Windows.Forms.Button();
            this.Btn_Read = new System.Windows.Forms.Button();
            this.Tbx_Brightness_Get = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Brightness_Set)).BeginInit();
            this.SuspendLayout();
            // 
            // RBtn_Channel_1
            // 
            this.RBtn_Channel_1.AutoSize = true;
            this.RBtn_Channel_1.Checked = true;
            this.RBtn_Channel_1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RBtn_Channel_1.Location = new System.Drawing.Point(132, 76);
            this.RBtn_Channel_1.Name = "RBtn_Channel_1";
            this.RBtn_Channel_1.Size = new System.Drawing.Size(43, 30);
            this.RBtn_Channel_1.TabIndex = 2;
            this.RBtn_Channel_1.TabStop = true;
            this.RBtn_Channel_1.Text = "1";
            this.RBtn_Channel_1.UseVisualStyleBackColor = true;
            // 
            // RBtn_Channel_4
            // 
            this.RBtn_Channel_4.AutoSize = true;
            this.RBtn_Channel_4.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RBtn_Channel_4.Location = new System.Drawing.Point(279, 76);
            this.RBtn_Channel_4.Name = "RBtn_Channel_4";
            this.RBtn_Channel_4.Size = new System.Drawing.Size(43, 30);
            this.RBtn_Channel_4.TabIndex = 3;
            this.RBtn_Channel_4.Text = "4";
            this.RBtn_Channel_4.UseVisualStyleBackColor = true;
            // 
            // RBtn_Channel_3
            // 
            this.RBtn_Channel_3.AutoSize = true;
            this.RBtn_Channel_3.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RBtn_Channel_3.Location = new System.Drawing.Point(230, 76);
            this.RBtn_Channel_3.Name = "RBtn_Channel_3";
            this.RBtn_Channel_3.Size = new System.Drawing.Size(43, 30);
            this.RBtn_Channel_3.TabIndex = 4;
            this.RBtn_Channel_3.Text = "3";
            this.RBtn_Channel_3.UseVisualStyleBackColor = true;
            // 
            // RBtn_Channel_2
            // 
            this.RBtn_Channel_2.AutoSize = true;
            this.RBtn_Channel_2.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RBtn_Channel_2.Location = new System.Drawing.Point(181, 76);
            this.RBtn_Channel_2.Name = "RBtn_Channel_2";
            this.RBtn_Channel_2.Size = new System.Drawing.Size(43, 30);
            this.RBtn_Channel_2.TabIndex = 5;
            this.RBtn_Channel_2.Text = "2";
            this.RBtn_Channel_2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(16, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "Channel : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(20, 169);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 26);
            this.label2.TabIndex = 7;
            this.label2.Text = "Brightness : ";
            // 
            // Num_Brightness_Set
            // 
            this.Num_Brightness_Set.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Num_Brightness_Set.Location = new System.Drawing.Point(161, 167);
            this.Num_Brightness_Set.Maximum = new decimal(new int[] {
            2999,
            0,
            0,
            0});
            this.Num_Brightness_Set.Name = "Num_Brightness_Set";
            this.Num_Brightness_Set.Size = new System.Drawing.Size(75, 35);
            this.Num_Brightness_Set.TabIndex = 8;
            this.Num_Brightness_Set.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Num_Brightness_Set.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // Btn_Set
            // 
            this.Btn_Set.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Set.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Set.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Set.Location = new System.Drawing.Point(242, 167);
            this.Btn_Set.Name = "Btn_Set";
            this.Btn_Set.Size = new System.Drawing.Size(89, 35);
            this.Btn_Set.TabIndex = 9;
            this.Btn_Set.Text = "Set";
            this.Btn_Set.UseVisualStyleBackColor = false;
            this.Btn_Set.Click += new System.EventHandler(this.Btn_Send_Click);
            // 
            // Tbx_SendMsg
            // 
            this.Tbx_SendMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_SendMsg.Location = new System.Drawing.Point(97, 317);
            this.Tbx_SendMsg.Name = "Tbx_SendMsg";
            this.Tbx_SendMsg.ReadOnly = true;
            this.Tbx_SendMsg.Size = new System.Drawing.Size(259, 35);
            this.Tbx_SendMsg.TabIndex = 10;
            this.Tbx_SendMsg.Text = "-";
            this.Tbx_SendMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Tbx_RecvMsg
            // 
            this.Tbx_RecvMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_RecvMsg.Location = new System.Drawing.Point(97, 358);
            this.Tbx_RecvMsg.Name = "Tbx_RecvMsg";
            this.Tbx_RecvMsg.ReadOnly = true;
            this.Tbx_RecvMsg.Size = new System.Drawing.Size(259, 35);
            this.Tbx_RecvMsg.TabIndex = 11;
            this.Tbx_RecvMsg.Text = "-";
            this.Tbx_RecvMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(16, 320);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 26);
            this.label3.TabIndex = 12;
            this.label3.Text = "Send : ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(16, 361);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 26);
            this.label4.TabIndex = 13;
            this.label4.Text = "Recv : ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(167, 279);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 26);
            this.label5.TabIndex = 14;
            this.label5.Text = "Status : ";
            // 
            // Tbx_Status
            // 
            this.Tbx_Status.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_Status.Location = new System.Drawing.Point(263, 276);
            this.Tbx_Status.Name = "Tbx_Status";
            this.Tbx_Status.ReadOnly = true;
            this.Tbx_Status.Size = new System.Drawing.Size(93, 35);
            this.Tbx_Status.TabIndex = 15;
            this.Tbx_Status.Text = "-";
            this.Tbx_Status.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Btn_Off
            // 
            this.Btn_Off.BackColor = System.Drawing.Color.Red;
            this.Btn_Off.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Off.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Off.Location = new System.Drawing.Point(116, 116);
            this.Btn_Off.Name = "Btn_Off";
            this.Btn_Off.Size = new System.Drawing.Size(89, 35);
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
            this.Btn_On.Location = new System.Drawing.Point(21, 116);
            this.Btn_On.Name = "Btn_On";
            this.Btn_On.Size = new System.Drawing.Size(89, 35);
            this.Btn_On.TabIndex = 17;
            this.Btn_On.Text = "On";
            this.Btn_On.UseVisualStyleBackColor = false;
            this.Btn_On.Click += new System.EventHandler(this.Btn_On_Click);
            // 
            // Btn_Read
            // 
            this.Btn_Read.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Btn_Read.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Read.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Read.Location = new System.Drawing.Point(242, 208);
            this.Btn_Read.Name = "Btn_Read";
            this.Btn_Read.Size = new System.Drawing.Size(89, 35);
            this.Btn_Read.TabIndex = 18;
            this.Btn_Read.Text = "Get";
            this.Btn_Read.UseVisualStyleBackColor = false;
            this.Btn_Read.Click += new System.EventHandler(this.Btn_Read_Click);
            // 
            // Tbx_Brightness_Get
            // 
            this.Tbx_Brightness_Get.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_Brightness_Get.Location = new System.Drawing.Point(161, 208);
            this.Tbx_Brightness_Get.Name = "Tbx_Brightness_Get";
            this.Tbx_Brightness_Get.ReadOnly = true;
            this.Tbx_Brightness_Get.Size = new System.Drawing.Size(75, 35);
            this.Tbx_Brightness_Get.TabIndex = 20;
            this.Tbx_Brightness_Get.Text = "-";
            this.Tbx_Brightness_Get.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // D65_Light_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 419);
            this.Controls.Add(this.Tbx_Brightness_Get);
            this.Controls.Add(this.Btn_Read);
            this.Controls.Add(this.Btn_On);
            this.Controls.Add(this.Btn_Off);
            this.Controls.Add(this.Tbx_Status);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Tbx_RecvMsg);
            this.Controls.Add(this.Tbx_SendMsg);
            this.Controls.Add(this.Btn_Set);
            this.Controls.Add(this.Num_Brightness_Set);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RBtn_Channel_2);
            this.Controls.Add(this.RBtn_Channel_3);
            this.Controls.Add(this.RBtn_Channel_4);
            this.Controls.Add(this.RBtn_Channel_1);
            this.Name = "D65_Light_Tool";
            this.Text = "D65_Light_Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.D65_Light_Tool_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Num_Brightness_Set)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RBtn_Channel_1;
        private System.Windows.Forms.RadioButton RBtn_Channel_4;
        private System.Windows.Forms.RadioButton RBtn_Channel_3;
        private System.Windows.Forms.RadioButton RBtn_Channel_2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown Num_Brightness_Set;
        private System.Windows.Forms.Button Btn_Set;
        private System.Windows.Forms.TextBox Tbx_SendMsg;
        private System.Windows.Forms.TextBox Tbx_RecvMsg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Tbx_Status;
        private System.Windows.Forms.Button Btn_Off;
        private System.Windows.Forms.Button Btn_On;
        private System.Windows.Forms.Button Btn_Read;
        private System.Windows.Forms.TextBox Tbx_Brightness_Get;
    }
}