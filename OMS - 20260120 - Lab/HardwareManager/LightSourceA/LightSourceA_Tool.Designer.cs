
namespace HardwareManager
{
    partial class LightSourceA_Tool
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
            this.Btn_Forward = new System.Windows.Forms.Button();
            this.Btn_Disable = new System.Windows.Forms.Button();
            this.Btn_Enable = new System.Windows.Forms.Button();
            this.Btn_Backward = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Rbx_Z = new System.Windows.Forms.RadioButton();
            this.Rbx_Y = new System.Windows.Forms.RadioButton();
            this.Rbx_X = new System.Windows.Forms.RadioButton();
            this.Btn_SetStep = new System.Windows.Forms.Button();
            this.Num_Step = new System.Windows.Forms.NumericUpDown();
            this.Btn_Home = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Step)).BeginInit();
            this.SuspendLayout();
            // 
            // Btn_Forward
            // 
            this.Btn_Forward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Forward.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Forward.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Forward.Location = new System.Drawing.Point(133, 138);
            this.Btn_Forward.Name = "Btn_Forward";
            this.Btn_Forward.Size = new System.Drawing.Size(126, 51);
            this.Btn_Forward.TabIndex = 9;
            this.Btn_Forward.Text = "Dark";
            this.Btn_Forward.UseVisualStyleBackColor = false;
            this.Btn_Forward.Click += new System.EventHandler(this.Btn_Forward_Click);
            // 
            // Btn_Disable
            // 
            this.Btn_Disable.BackColor = System.Drawing.Color.Red;
            this.Btn_Disable.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Disable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Disable.Location = new System.Drawing.Point(241, 66);
            this.Btn_Disable.Name = "Btn_Disable";
            this.Btn_Disable.Size = new System.Drawing.Size(102, 51);
            this.Btn_Disable.TabIndex = 16;
            this.Btn_Disable.Text = "Disable";
            this.Btn_Disable.UseVisualStyleBackColor = false;
            this.Btn_Disable.Click += new System.EventHandler(this.Btn_Disable_Click);
            // 
            // Btn_Enable
            // 
            this.Btn_Enable.BackColor = System.Drawing.Color.Green;
            this.Btn_Enable.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Enable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Enable.Location = new System.Drawing.Point(133, 66);
            this.Btn_Enable.Name = "Btn_Enable";
            this.Btn_Enable.Size = new System.Drawing.Size(102, 51);
            this.Btn_Enable.TabIndex = 17;
            this.Btn_Enable.Text = "Enable";
            this.Btn_Enable.UseVisualStyleBackColor = false;
            this.Btn_Enable.Click += new System.EventHandler(this.Btn_Enable_Click);
            // 
            // Btn_Backward
            // 
            this.Btn_Backward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Backward.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Backward.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Backward.Location = new System.Drawing.Point(265, 138);
            this.Btn_Backward.Name = "Btn_Backward";
            this.Btn_Backward.Size = new System.Drawing.Size(126, 51);
            this.Btn_Backward.TabIndex = 18;
            this.Btn_Backward.Text = "Bright";
            this.Btn_Backward.UseVisualStyleBackColor = false;
            this.Btn_Backward.Click += new System.EventHandler(this.Btn_Backward_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Rbx_Z);
            this.groupBox1.Controls.Add(this.Rbx_Y);
            this.groupBox1.Controls.Add(this.Rbx_X);
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(21, 66);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(106, 174);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Motor";
            // 
            // Rbx_Z
            // 
            this.Rbx_Z.AutoSize = true;
            this.Rbx_Z.Location = new System.Drawing.Point(18, 130);
            this.Rbx_Z.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Rbx_Z.Name = "Rbx_Z";
            this.Rbx_Z.Size = new System.Drawing.Size(44, 32);
            this.Rbx_Z.TabIndex = 2;
            this.Rbx_Z.Text = "Z";
            this.Rbx_Z.UseVisualStyleBackColor = true;
            this.Rbx_Z.CheckedChanged += new System.EventHandler(this.Rbx_CheckedChanged);
            // 
            // Rbx_Y
            // 
            this.Rbx_Y.AutoSize = true;
            this.Rbx_Y.Location = new System.Drawing.Point(18, 82);
            this.Rbx_Y.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Rbx_Y.Name = "Rbx_Y";
            this.Rbx_Y.Size = new System.Drawing.Size(44, 32);
            this.Rbx_Y.TabIndex = 1;
            this.Rbx_Y.Text = "Y";
            this.Rbx_Y.UseVisualStyleBackColor = true;
            this.Rbx_Y.CheckedChanged += new System.EventHandler(this.Rbx_CheckedChanged);
            // 
            // Rbx_X
            // 
            this.Rbx_X.AutoSize = true;
            this.Rbx_X.Checked = true;
            this.Rbx_X.Location = new System.Drawing.Point(18, 35);
            this.Rbx_X.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Rbx_X.Name = "Rbx_X";
            this.Rbx_X.Size = new System.Drawing.Size(45, 32);
            this.Rbx_X.TabIndex = 0;
            this.Rbx_X.TabStop = true;
            this.Rbx_X.Text = "X";
            this.Rbx_X.UseVisualStyleBackColor = true;
            this.Rbx_X.CheckedChanged += new System.EventHandler(this.Rbx_CheckedChanged);
            // 
            // Btn_SetStep
            // 
            this.Btn_SetStep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_SetStep.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_SetStep.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_SetStep.Location = new System.Drawing.Point(328, 196);
            this.Btn_SetStep.Name = "Btn_SetStep";
            this.Btn_SetStep.Size = new System.Drawing.Size(146, 51);
            this.Btn_SetStep.TabIndex = 20;
            this.Btn_SetStep.Text = "SetStep";
            this.Btn_SetStep.UseVisualStyleBackColor = false;
            this.Btn_SetStep.Click += new System.EventHandler(this.Btn_SetStep_Click);
            // 
            // Num_Step
            // 
            this.Num_Step.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Num_Step.Location = new System.Drawing.Point(166, 206);
            this.Num_Step.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Num_Step.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.Num_Step.Name = "Num_Step";
            this.Num_Step.Size = new System.Drawing.Size(157, 36);
            this.Num_Step.TabIndex = 21;
            // 
            // Btn_Home
            // 
            this.Btn_Home.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Home.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Home.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Btn_Home.Location = new System.Drawing.Point(382, 66);
            this.Btn_Home.Name = "Btn_Home";
            this.Btn_Home.Size = new System.Drawing.Size(92, 51);
            this.Btn_Home.TabIndex = 22;
            this.Btn_Home.Text = "Home";
            this.Btn_Home.UseVisualStyleBackColor = false;
            this.Btn_Home.Click += new System.EventHandler(this.Btn_Home_Click);
            // 
            // LightSourceA_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 258);
            this.Controls.Add(this.Btn_Home);
            this.Controls.Add(this.Num_Step);
            this.Controls.Add(this.Btn_SetStep);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Btn_Backward);
            this.Controls.Add(this.Btn_Enable);
            this.Controls.Add(this.Btn_Disable);
            this.Controls.Add(this.Btn_Forward);
            this.Name = "LightSourceA_Tool";
            this.Text = "Arduino_Tool";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Step)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Btn_Forward;
        private System.Windows.Forms.Button Btn_Disable;
        private System.Windows.Forms.Button Btn_Enable;
        private System.Windows.Forms.Button Btn_Backward;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Rbx_Z;
        private System.Windows.Forms.RadioButton Rbx_Y;
        private System.Windows.Forms.RadioButton Rbx_X;
        private System.Windows.Forms.Button Btn_SetStep;
        private System.Windows.Forms.NumericUpDown Num_Step;
        private System.Windows.Forms.Button Btn_Home;
    }
}