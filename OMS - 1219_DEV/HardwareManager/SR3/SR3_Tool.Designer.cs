
namespace HardwareManager
{
    partial class SR3_Tool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SR3_Tool));
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Tbx_RecvMsg = new System.Windows.Forms.TextBox();
            this.Tbx_SendMsg = new System.Windows.Forms.TextBox();
            this.Btn_RemoteMode = new System.Windows.Forms.Button();
            this.Btn_LocalMode = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Btn_SetMeasureType = new System.Windows.Forms.Button();
            this.Cbx_SpectralRadiance = new System.Windows.Forms.CheckBox();
            this.Cbx_Colorimetric = new System.Windows.Forms.CheckBox();
            this.Btn_Measurement = new System.Windows.Forms.Button();
            this.Rtbx_Result = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_Record = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(7, 572);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 26);
            this.label4.TabIndex = 19;
            this.label4.Text = "Recv : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(7, 531);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 26);
            this.label3.TabIndex = 18;
            this.label3.Text = "Send : ";
            // 
            // Tbx_RecvMsg
            // 
            this.Tbx_RecvMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_RecvMsg.Location = new System.Drawing.Point(88, 569);
            this.Tbx_RecvMsg.Name = "Tbx_RecvMsg";
            this.Tbx_RecvMsg.ReadOnly = true;
            this.Tbx_RecvMsg.Size = new System.Drawing.Size(139, 35);
            this.Tbx_RecvMsg.TabIndex = 17;
            this.Tbx_RecvMsg.Text = "-";
            this.Tbx_RecvMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Tbx_SendMsg
            // 
            this.Tbx_SendMsg.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Tbx_SendMsg.Location = new System.Drawing.Point(88, 528);
            this.Tbx_SendMsg.Name = "Tbx_SendMsg";
            this.Tbx_SendMsg.ReadOnly = true;
            this.Tbx_SendMsg.Size = new System.Drawing.Size(139, 35);
            this.Tbx_SendMsg.TabIndex = 16;
            this.Tbx_SendMsg.Text = "-";
            this.Tbx_SendMsg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Btn_RemoteMode
            // 
            this.Btn_RemoteMode.BackColor = System.Drawing.Color.Green;
            this.Btn_RemoteMode.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_RemoteMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_RemoteMode.Location = new System.Drawing.Point(6, 90);
            this.Btn_RemoteMode.Name = "Btn_RemoteMode";
            this.Btn_RemoteMode.Size = new System.Drawing.Size(208, 50);
            this.Btn_RemoteMode.TabIndex = 22;
            this.Btn_RemoteMode.Text = "Remote Mode";
            this.Btn_RemoteMode.UseVisualStyleBackColor = false;
            this.Btn_RemoteMode.Click += new System.EventHandler(this.Btn_RemoteMode_Click);
            // 
            // Btn_LocalMode
            // 
            this.Btn_LocalMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Btn_LocalMode.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_LocalMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_LocalMode.Location = new System.Drawing.Point(6, 34);
            this.Btn_LocalMode.Name = "Btn_LocalMode";
            this.Btn_LocalMode.Size = new System.Drawing.Size(208, 50);
            this.Btn_LocalMode.TabIndex = 23;
            this.Btn_LocalMode.Text = "Local Mode";
            this.Btn_LocalMode.UseVisualStyleBackColor = false;
            this.Btn_LocalMode.Click += new System.EventHandler(this.Btn_LocalMode_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Btn_RemoteMode);
            this.groupBox1.Controls.Add(this.Btn_LocalMode);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(6, 67);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 148);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Device Mode";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Btn_SetMeasureType);
            this.groupBox2.Controls.Add(this.Cbx_SpectralRadiance);
            this.groupBox2.Controls.Add(this.Cbx_Colorimetric);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox2.Location = new System.Drawing.Point(6, 245);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(221, 162);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Measure Type";
            // 
            // Btn_SetMeasureTypr
            // 
            this.Btn_SetMeasureType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_SetMeasureType.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_SetMeasureType.ForeColor = System.Drawing.Color.Blue;
            this.Btn_SetMeasureType.Location = new System.Drawing.Point(6, 106);
            this.Btn_SetMeasureType.Name = "Btn_SetMeasureTypr";
            this.Btn_SetMeasureType.Size = new System.Drawing.Size(208, 50);
            this.Btn_SetMeasureType.TabIndex = 22;
            this.Btn_SetMeasureType.Text = "Set";
            this.Btn_SetMeasureType.UseVisualStyleBackColor = false;
            this.Btn_SetMeasureType.Click += new System.EventHandler(this.Btn_SetMeasureType_Click);
            // 
            // Cbx_SpectralRadiance
            // 
            this.Cbx_SpectralRadiance.AutoSize = true;
            this.Cbx_SpectralRadiance.Location = new System.Drawing.Point(6, 70);
            this.Cbx_SpectralRadiance.Name = "Cbx_SpectralRadiance";
            this.Cbx_SpectralRadiance.Size = new System.Drawing.Size(208, 30);
            this.Cbx_SpectralRadiance.TabIndex = 1;
            this.Cbx_SpectralRadiance.Text = "SpectralRadiance";
            this.Cbx_SpectralRadiance.UseVisualStyleBackColor = true;
            // 
            // Cbx_Colorimetric
            // 
            this.Cbx_Colorimetric.AutoSize = true;
            this.Cbx_Colorimetric.Checked = true;
            this.Cbx_Colorimetric.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Cbx_Colorimetric.Enabled = false;
            this.Cbx_Colorimetric.Location = new System.Drawing.Point(6, 34);
            this.Cbx_Colorimetric.Name = "Cbx_Colorimetric";
            this.Cbx_Colorimetric.Size = new System.Drawing.Size(158, 30);
            this.Cbx_Colorimetric.TabIndex = 0;
            this.Cbx_Colorimetric.Text = "Colorimetric";
            this.Cbx_Colorimetric.UseVisualStyleBackColor = true;
            // 
            // Btn_Measurement
            // 
            this.Btn_Measurement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Measurement.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Measurement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Measurement.Location = new System.Drawing.Point(12, 428);
            this.Btn_Measurement.Name = "Btn_Measurement";
            this.Btn_Measurement.Size = new System.Drawing.Size(208, 53);
            this.Btn_Measurement.TabIndex = 23;
            this.Btn_Measurement.Text = "Measurement";
            this.Btn_Measurement.UseVisualStyleBackColor = false;
            this.Btn_Measurement.Click += new System.EventHandler(this.Btn_Measurement_Click);
            // 
            // Rtbx_Result
            // 
            this.Rtbx_Result.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Rtbx_Result.Location = new System.Drawing.Point(233, 106);
            this.Rtbx_Result.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Rtbx_Result.Name = "Rtbx_Result";
            this.Rtbx_Result.ReadOnly = true;
            this.Rtbx_Result.Size = new System.Drawing.Size(469, 498);
            this.Rtbx_Result.TabIndex = 26;
            this.Rtbx_Result.Text = resources.GetString("Rtbx_Result.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(233, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 26);
            this.label1.TabIndex = 27;
            this.label1.Text = "Measurement Result";
            // 
            // Btn_Record
            // 
            this.Btn_Record.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Record.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Record.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Record.Location = new System.Drawing.Point(615, 67);
            this.Btn_Record.Name = "Btn_Record";
            this.Btn_Record.Size = new System.Drawing.Size(87, 33);
            this.Btn_Record.TabIndex = 28;
            this.Btn_Record.Text = "Save";
            this.Btn_Record.UseVisualStyleBackColor = false;
            this.Btn_Record.Click += new System.EventHandler(this.Btn_Record_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numericUpDown1.Location = new System.Drawing.Point(110, 487);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(110, 35);
            this.numericUpDown1.TabIndex = 29;
            this.numericUpDown1.Value = new decimal(new int[] {
            120000,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(7, 489);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 26);
            this.label2.TabIndex = 30;
            this.label2.Text = "Timeout";
            // 
            // SR3_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 614);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.Btn_Record);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Rtbx_Result);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Btn_Measurement);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Tbx_RecvMsg);
            this.Controls.Add(this.Tbx_SendMsg);
            this.Name = "SR3_Tool";
            this.Text = "SR3_Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SR3_Tool_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Tbx_RecvMsg;
        private System.Windows.Forms.TextBox Tbx_SendMsg;
        private System.Windows.Forms.Button Btn_RemoteMode;
        private System.Windows.Forms.Button Btn_LocalMode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Btn_SetMeasureType;
        private System.Windows.Forms.Button Btn_Measurement;
        private System.Windows.Forms.CheckBox Cbx_SpectralRadiance;
        private System.Windows.Forms.CheckBox Cbx_Colorimetric;
        private System.Windows.Forms.RichTextBox Rtbx_Result;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_Record;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
    }
}