
namespace OpticalMeasuringSystem
{
    partial class frmGetLumCxCyFilePath
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGetLumCxCyFilePath));
            this.lbl_OpticalDataPath = new MaterialSkin.Controls.MaterialLabel();
            this.pB_LumCxCy_DataExample = new System.Windows.Forms.PictureBox();
            this.lbl_DataExample = new MaterialSkin.Controls.MaterialLabel();
            this.btnSelect = new MaterialSkin.Controls.MaterialButton();
            this.pB_Lum_DataExample = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pB_LumCxCy_DataExample)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Lum_DataExample)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_OpticalDataPath
            // 
            this.lbl_OpticalDataPath.AutoSize = true;
            this.lbl_OpticalDataPath.Depth = 0;
            this.lbl_OpticalDataPath.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lbl_OpticalDataPath.Location = new System.Drawing.Point(17, 40);
            this.lbl_OpticalDataPath.MouseState = MaterialSkin.MouseState.HOVER;
            this.lbl_OpticalDataPath.Name = "lbl_OpticalDataPath";
            this.lbl_OpticalDataPath.Size = new System.Drawing.Size(176, 19);
            this.lbl_OpticalDataPath.TabIndex = 0;
            this.lbl_OpticalDataPath.Text = "Select optical data path. ";
            // 
            // pB_LumCxCy_DataExample
            // 
            this.pB_LumCxCy_DataExample.Image = ((System.Drawing.Image)(resources.GetObject("pB_LumCxCy_DataExample.Image")));
            this.pB_LumCxCy_DataExample.Location = new System.Drawing.Point(18, 104);
            this.pB_LumCxCy_DataExample.Name = "pB_LumCxCy_DataExample";
            this.pB_LumCxCy_DataExample.Size = new System.Drawing.Size(225, 112);
            this.pB_LumCxCy_DataExample.TabIndex = 1;
            this.pB_LumCxCy_DataExample.TabStop = false;
            // 
            // lbl_DataExample
            // 
            this.lbl_DataExample.AutoSize = true;
            this.lbl_DataExample.Depth = 0;
            this.lbl_DataExample.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lbl_DataExample.Location = new System.Drawing.Point(17, 73);
            this.lbl_DataExample.MouseState = MaterialSkin.MouseState.HOVER;
            this.lbl_DataExample.Name = "lbl_DataExample";
            this.lbl_DataExample.Size = new System.Drawing.Size(108, 19);
            this.lbl_DataExample.TabIndex = 2;
            this.lbl_DataExample.Text = "Data Example :";
            // 
            // btnSelect
            // 
            this.btnSelect.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSelect.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSelect.Depth = 0;
            this.btnSelect.HighEmphasis = true;
            this.btnSelect.Icon = null;
            this.btnSelect.Location = new System.Drawing.Point(200, 40);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSelect.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSelect.Size = new System.Drawing.Size(74, 36);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Select";
            this.btnSelect.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSelect.UseAccentColor = false;
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // pB_Lum_DataExample
            // 
            this.pB_Lum_DataExample.Image = ((System.Drawing.Image)(resources.GetObject("pB_Lum_DataExample.Image")));
            this.pB_Lum_DataExample.Location = new System.Drawing.Point(249, 104);
            this.pB_Lum_DataExample.Name = "pB_Lum_DataExample";
            this.pB_Lum_DataExample.Size = new System.Drawing.Size(89, 112);
            this.pB_Lum_DataExample.TabIndex = 4;
            this.pB_Lum_DataExample.TabStop = false;
            // 
            // frmGetLumCxCyFilePath
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 235);
            this.Controls.Add(this.pB_Lum_DataExample);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.lbl_DataExample);
            this.Controls.Add(this.pB_LumCxCy_DataExample);
            this.Controls.Add(this.lbl_OpticalDataPath);
            this.FormStyle = MaterialSkin.Controls.MaterialForm.FormStyles.ActionBar_None;
            this.Name = "frmGetLumCxCyFilePath";
            this.Padding = new System.Windows.Forms.Padding(3, 24, 3, 3);
            this.Text = "Select Optical Data File";
            ((System.ComponentModel.ISupportInitialize)(this.pB_LumCxCy_DataExample)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Lum_DataExample)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel lbl_OpticalDataPath;
        private System.Windows.Forms.PictureBox pB_LumCxCy_DataExample;
        private MaterialSkin.Controls.MaterialLabel lbl_DataExample;
        private MaterialSkin.Controls.MaterialButton btnSelect;
        private System.Windows.Forms.PictureBox pB_Lum_DataExample;
    }
}