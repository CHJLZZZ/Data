
namespace OpticalMeasuringSystem
{
    partial class frmFourColorMatrixMethod
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
            this.PnlUCViewer = new System.Windows.Forms.Panel();
            this.Dgv_Roi = new System.Windows.Forms.DataGridView();
            this.ROI_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CenterX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CenterY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Radius = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Luminance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSaveFourColorCalibration = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel4 = new MaterialSkin.Controls.MaterialLabel();
            this.btnRemoveRoi = new MaterialSkin.Controls.MaterialButton();
            this.tbxMatrixColumn = new MaterialSkin.Controls.MaterialTextBox();
            this.btnAddRoi = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.tbxMatrixRow = new MaterialSkin.Controls.MaterialTextBox();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Roi)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PnlUCViewer
            // 
            this.PnlUCViewer.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.SetColumnSpan(this.PnlUCViewer, 10);
            this.PnlUCViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlUCViewer.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.PnlUCViewer.Location = new System.Drawing.Point(3, 3);
            this.PnlUCViewer.Name = "PnlUCViewer";
            this.PnlUCViewer.Size = new System.Drawing.Size(868, 519);
            this.PnlUCViewer.TabIndex = 2;
            this.PnlUCViewer.MouseEnter += new System.EventHandler(this.PnlUCViewer_MouseEnter);
            this.PnlUCViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PnlUCViewer_MouseMove);
            // 
            // Dgv_Roi
            // 
            this.Dgv_Roi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Dgv_Roi.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ROI_Name,
            this.CenterX,
            this.CenterY,
            this.Radius,
            this.Luminance,
            this.Cx,
            this.Cy});
            this.tableLayoutPanel1.SetColumnSpan(this.Dgv_Roi, 10);
            this.Dgv_Roi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dgv_Roi.Location = new System.Drawing.Point(3, 578);
            this.Dgv_Roi.Name = "Dgv_Roi";
            this.Dgv_Roi.RowTemplate.Height = 24;
            this.Dgv_Roi.Size = new System.Drawing.Size(868, 152);
            this.Dgv_Roi.TabIndex = 24;
            this.Dgv_Roi.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Dgv_Roi_CellMouseUp);
            this.Dgv_Roi.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Dgv_Roi_CellValueChanged);
            this.Dgv_Roi.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Dgv_Roi_KeyDown);
            // 
            // ROI_Name
            // 
            this.ROI_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ROI_Name.HeaderText = "Name";
            this.ROI_Name.Name = "ROI_Name";
            // 
            // CenterX
            // 
            this.CenterX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CenterX.HeaderText = "Center X";
            this.CenterX.Name = "CenterX";
            // 
            // CenterY
            // 
            this.CenterY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CenterY.HeaderText = "Center Y";
            this.CenterY.Name = "CenterY";
            // 
            // Radius
            // 
            this.Radius.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Radius.HeaderText = "Radius";
            this.Radius.Name = "Radius";
            // 
            // Luminance
            // 
            this.Luminance.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Luminance.HeaderText = "Lum";
            this.Luminance.Name = "Luminance";
            // 
            // Cx
            // 
            this.Cx.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Cx.HeaderText = "Cx";
            this.Cx.Name = "Cx";
            // 
            // Cy
            // 
            this.Cy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Cy.HeaderText = "Cy";
            this.Cy.Name = "Cy";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.btnSaveFourColorCalibration, 9, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel4, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnRemoveRoi, 7, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxMatrixColumn, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnAddRoi, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.PnlUCViewer, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbxMatrixRow, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Dgv_Roi, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 64);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 77F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(874, 733);
            this.tableLayoutPanel1.TabIndex = 25;
            // 
            // btnSaveFourColorCalibration
            // 
            this.btnSaveFourColorCalibration.AllowDrop = true;
            this.btnSaveFourColorCalibration.AutoSize = false;
            this.btnSaveFourColorCalibration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveFourColorCalibration.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveFourColorCalibration.Depth = 0;
            this.btnSaveFourColorCalibration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveFourColorCalibration.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSaveFourColorCalibration.HighEmphasis = true;
            this.btnSaveFourColorCalibration.Icon = global::OpticalMeasuringSystem.Properties.Resources.Save2;
            this.btnSaveFourColorCalibration.Image = global::OpticalMeasuringSystem.Properties.Resources.add;
            this.btnSaveFourColorCalibration.Location = new System.Drawing.Point(750, 531);
            this.btnSaveFourColorCalibration.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveFourColorCalibration.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveFourColorCalibration.Name = "btnSaveFourColorCalibration";
            this.btnSaveFourColorCalibration.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveFourColorCalibration.Size = new System.Drawing.Size(120, 38);
            this.btnSaveFourColorCalibration.TabIndex = 44;
            this.btnSaveFourColorCalibration.Text = "Save";
            this.btnSaveFourColorCalibration.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSaveFourColorCalibration.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveFourColorCalibration.UseAccentColor = false;
            this.btnSaveFourColorCalibration.UseVisualStyleBackColor = true;
            this.btnSaveFourColorCalibration.Click += new System.EventHandler(this.btnSaveFourColorCalibration_Click);
            // 
            // materialLabel4
            // 
            this.materialLabel4.AutoSize = true;
            this.materialLabel4.Depth = 0;
            this.materialLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialLabel4.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel4.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel4.Location = new System.Drawing.Point(401, 525);
            this.materialLabel4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel4.Name = "materialLabel4";
            this.materialLabel4.Size = new System.Drawing.Size(74, 50);
            this.materialLabel4.TabIndex = 45;
            this.materialLabel4.Text = "ROI Edit : ";
            this.materialLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnRemoveRoi
            // 
            this.btnRemoveRoi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemoveRoi.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRemoveRoi.Depth = 0;
            this.btnRemoveRoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemoveRoi.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.btnRemoveRoi.HighEmphasis = true;
            this.btnRemoveRoi.Icon = global::OpticalMeasuringSystem.Properties.Resources.delete;
            this.btnRemoveRoi.Location = new System.Drawing.Point(606, 531);
            this.btnRemoveRoi.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRemoveRoi.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRemoveRoi.Name = "btnRemoveRoi";
            this.btnRemoveRoi.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRemoveRoi.Size = new System.Drawing.Size(116, 38);
            this.btnRemoveRoi.TabIndex = 46;
            this.btnRemoveRoi.Text = "Del";
            this.btnRemoveRoi.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRemoveRoi.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRemoveRoi.UseAccentColor = false;
            this.btnRemoveRoi.UseVisualStyleBackColor = true;
            // 
            // tbxMatrixColumn
            // 
            this.tbxMatrixColumn.AnimateReadOnly = false;
            this.tbxMatrixColumn.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxMatrixColumn.Depth = 0;
            this.tbxMatrixColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxMatrixColumn.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbxMatrixColumn.LeadingIcon = null;
            this.tbxMatrixColumn.Location = new System.Drawing.Point(257, 528);
            this.tbxMatrixColumn.MaxLength = 50;
            this.tbxMatrixColumn.MouseState = MaterialSkin.MouseState.OUT;
            this.tbxMatrixColumn.Multiline = false;
            this.tbxMatrixColumn.Name = "tbxMatrixColumn";
            this.tbxMatrixColumn.Size = new System.Drawing.Size(118, 50);
            this.tbxMatrixColumn.TabIndex = 42;
            this.tbxMatrixColumn.Text = "";
            this.tbxMatrixColumn.TrailingIcon = null;
            // 
            // btnAddRoi
            // 
            this.btnAddRoi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddRoi.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddRoi.Depth = 0;
            this.btnAddRoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddRoi.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.btnAddRoi.HighEmphasis = true;
            this.btnAddRoi.Icon = global::OpticalMeasuringSystem.Properties.Resources.add;
            this.btnAddRoi.Location = new System.Drawing.Point(482, 531);
            this.btnAddRoi.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddRoi.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddRoi.Name = "btnAddRoi";
            this.btnAddRoi.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddRoi.Size = new System.Drawing.Size(116, 38);
            this.btnAddRoi.TabIndex = 43;
            this.btnAddRoi.Text = "Add";
            this.btnAddRoi.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAddRoi.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddRoi.UseAccentColor = false;
            this.btnAddRoi.UseVisualStyleBackColor = true;
            this.btnAddRoi.Click += new System.EventHandler(this.Btn_AddRoi_Click);
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel2.Location = new System.Drawing.Point(177, 525);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(74, 50);
            this.materialLabel2.TabIndex = 44;
            this.materialLabel2.Text = "Column : ";
            this.materialLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbxMatrixRow
            // 
            this.tbxMatrixRow.AnimateReadOnly = false;
            this.tbxMatrixRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxMatrixRow.Depth = 0;
            this.tbxMatrixRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxMatrixRow.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbxMatrixRow.LeadingIcon = null;
            this.tbxMatrixRow.Location = new System.Drawing.Point(53, 528);
            this.tbxMatrixRow.MaxLength = 50;
            this.tbxMatrixRow.MouseState = MaterialSkin.MouseState.OUT;
            this.tbxMatrixRow.Multiline = false;
            this.tbxMatrixRow.Name = "tbxMatrixRow";
            this.tbxMatrixRow.Size = new System.Drawing.Size(118, 50);
            this.tbxMatrixRow.TabIndex = 41;
            this.tbxMatrixRow.Text = "";
            this.tbxMatrixRow.TrailingIcon = null;
            // 
            // materialLabel3
            // 
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel3.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel3.Location = new System.Drawing.Point(3, 525);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(44, 50);
            this.materialLabel3.TabIndex = 43;
            this.materialLabel3.Text = "Row : ";
            this.materialLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frmFourColorMatrixMethod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 800);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmFourColorMatrixMethod";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Four Color Matrix Method Data Create";
            this.Load += new System.EventHandler(this.frmUserCalibration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Roi)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PnlUCViewer;
        private System.Windows.Forms.DataGridView Dgv_Roi;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MaterialSkin.Controls.MaterialTextBox tbxMatrixColumn;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private MaterialSkin.Controls.MaterialTextBox tbxMatrixRow;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialButton btnSaveFourColorCalibration;
        private MaterialSkin.Controls.MaterialLabel materialLabel4;
        private MaterialSkin.Controls.MaterialButton btnRemoveRoi;
        private MaterialSkin.Controls.MaterialButton btnAddRoi;
        private System.Windows.Forms.DataGridViewTextBoxColumn ROI_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn CenterX;
        private System.Windows.Forms.DataGridViewTextBoxColumn CenterY;
        private System.Windows.Forms.DataGridViewTextBoxColumn Radius;
        private System.Windows.Forms.DataGridViewTextBoxColumn Luminance;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cx;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cy;
    }
}