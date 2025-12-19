
namespace OpticalMeasuringSystem
{
    partial class frmOneColorCorrection
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
            this.dgvRoi = new System.Windows.Forms.DataGridView();
            this.ROI_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CenterX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CenterY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Radius = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Luminance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripRoiEditor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmi_RoiEditor_New = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_RoiEditor_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_RoiEditor_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tbxCailbrationFile = new System.Windows.Forms.TextBox();
            this.pnlOneColorCalibrationViewer = new System.Windows.Forms.Panel();
            this.lblMatrixColumn = new MaterialSkin.Controls.MaterialLabel();
            this.lbxRecipeList = new System.Windows.Forms.ListBox();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.lblMatrixRow = new MaterialSkin.Controls.MaterialLabel();
            this.tbxMatrixRow = new System.Windows.Forms.TextBox();
            this.tbxMatrixColumn = new System.Windows.Forms.TextBox();
            this.btnSave = new MaterialSkin.Controls.MaterialButton();
            this.btnLoad = new MaterialSkin.Controls.MaterialButton();
            this.AutoLum_btn = new MaterialSkin.Controls.MaterialButton();
            this.btnLoad_Lum_Cx_Cy = new MaterialSkin.Controls.MaterialButton();
            this.AddCIELuminance_mcb = new MaterialSkin.Controls.MaterialCheckbox();
            this.contextMenuStripLoadData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmi_LoadData_New = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_LoadData_Load = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_LoadData_Rename = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_LoadData_Delete = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoi)).BeginInit();
            this.contextMenuStripRoiEditor.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStripLoadData.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvRoi
            // 
            this.dgvRoi.AllowUserToAddRows = false;
            this.dgvRoi.AllowUserToResizeColumns = false;
            this.dgvRoi.AllowUserToResizeRows = false;
            this.dgvRoi.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvRoi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRoi.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ROI_Name,
            this.CenterX,
            this.CenterY,
            this.Radius,
            this.Luminance,
            this.Cx,
            this.Cy});
            this.tableLayoutPanel1.SetColumnSpan(this.dgvRoi, 8);
            this.dgvRoi.ContextMenuStrip = this.contextMenuStripRoiEditor;
            this.dgvRoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRoi.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvRoi.Location = new System.Drawing.Point(3, 652);
            this.dgvRoi.Name = "dgvRoi";
            this.dgvRoi.RowHeadersWidth = 62;
            this.tableLayoutPanel1.SetRowSpan(this.dgvRoi, 5);
            this.dgvRoi.RowTemplate.Height = 24;
            this.dgvRoi.Size = new System.Drawing.Size(1143, 225);
            this.dgvRoi.TabIndex = 24;
            this.dgvRoi.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvRoi_CellMouseClick);
            this.dgvRoi.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRoi_CellValueChanged);
            this.dgvRoi.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvRoi_KeyDown);
            // 
            // ROI_Name
            // 
            this.ROI_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ROI_Name.HeaderText = "Name";
            this.ROI_Name.MinimumWidth = 8;
            this.ROI_Name.Name = "ROI_Name";
            this.ROI_Name.ReadOnly = true;
            // 
            // CenterX
            // 
            this.CenterX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CenterX.HeaderText = "Center X";
            this.CenterX.MinimumWidth = 8;
            this.CenterX.Name = "CenterX";
            // 
            // CenterY
            // 
            this.CenterY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CenterY.HeaderText = "Center Y";
            this.CenterY.MinimumWidth = 8;
            this.CenterY.Name = "CenterY";
            // 
            // Radius
            // 
            this.Radius.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Radius.HeaderText = "Radius (pixels)";
            this.Radius.MinimumWidth = 8;
            this.Radius.Name = "Radius";
            // 
            // Luminance
            // 
            this.Luminance.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Luminance.HeaderText = "Luminance (nits)";
            this.Luminance.MinimumWidth = 8;
            this.Luminance.Name = "Luminance";
            // 
            // Cx
            // 
            this.Cx.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Cx.HeaderText = "Cx";
            this.Cx.MinimumWidth = 8;
            this.Cx.Name = "Cx";
            // 
            // Cy
            // 
            this.Cy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Cy.HeaderText = "Cy";
            this.Cy.MinimumWidth = 8;
            this.Cy.Name = "Cy";
            // 
            // contextMenuStripRoiEditor
            // 
            this.contextMenuStripRoiEditor.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripRoiEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_RoiEditor_New,
            this.tsmi_RoiEditor_Copy,
            this.tsmi_RoiEditor_Delete});
            this.contextMenuStripRoiEditor.Name = "contextMenuStripRoiEditor";
            this.contextMenuStripRoiEditor.Size = new System.Drawing.Size(137, 94);
            this.contextMenuStripRoiEditor.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripRoiEditor_Opening);
            // 
            // tsmi_RoiEditor_New
            // 
            this.tsmi_RoiEditor_New.Name = "tsmi_RoiEditor_New";
            this.tsmi_RoiEditor_New.Size = new System.Drawing.Size(136, 30);
            this.tsmi_RoiEditor_New.Text = "New";
            this.tsmi_RoiEditor_New.Click += new System.EventHandler(this.tsmi_RoiEditor_New_Click);
            // 
            // tsmi_RoiEditor_Copy
            // 
            this.tsmi_RoiEditor_Copy.Name = "tsmi_RoiEditor_Copy";
            this.tsmi_RoiEditor_Copy.Size = new System.Drawing.Size(136, 30);
            this.tsmi_RoiEditor_Copy.Text = "Copy";
            this.tsmi_RoiEditor_Copy.Click += new System.EventHandler(this.tsmi_RoiEditor_Copy_Click);
            // 
            // tsmi_RoiEditor_Delete
            // 
            this.tsmi_RoiEditor_Delete.Name = "tsmi_RoiEditor_Delete";
            this.tsmi_RoiEditor_Delete.Size = new System.Drawing.Size(136, 30);
            this.tsmi_RoiEditor_Delete.Text = "Delete";
            this.tsmi_RoiEditor_Delete.Click += new System.EventHandler(this.tsmi_RoiEditor_Delete_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.55293F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.54832F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.942051F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.47657F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.726779F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.40481F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.75168F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.62469F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.807022F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.165142F));
            this.tableLayoutPanel1.Controls.Add(this.tbxCailbrationFile, 9, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvRoi, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.pnlOneColorCalibrationViewer, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblMatrixColumn, 8, 3);
            this.tableLayoutPanel1.Controls.Add(this.lbxRecipeList, 8, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel3, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblMatrixRow, 8, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbxMatrixRow, 9, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbxMatrixColumn, 9, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnSave, 9, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnLoad, 8, 4);
            this.tableLayoutPanel1.Controls.Add(this.AutoLum_btn, 9, 6);
            this.tableLayoutPanel1.Controls.Add(this.btnLoad_Lum_Cx_Cy, 8, 5);
            this.tableLayoutPanel1.Controls.Add(this.AddCIELuminance_mcb, 8, 6);
            this.tableLayoutPanel1.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 73);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1374, 880);
            this.tableLayoutPanel1.TabIndex = 25;
            // 
            // tbxCailbrationFile
            // 
            this.tbxCailbrationFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbxCailbrationFile.Location = new System.Drawing.Point(1259, 14);
            this.tbxCailbrationFile.Name = "tbxCailbrationFile";
            this.tbxCailbrationFile.Size = new System.Drawing.Size(109, 39);
            this.tbxCailbrationFile.TabIndex = 59;
            // 
            // pnlOneColorCalibrationViewer
            // 
            this.pnlOneColorCalibrationViewer.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.SetColumnSpan(this.pnlOneColorCalibrationViewer, 8);
            this.pnlOneColorCalibrationViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOneColorCalibrationViewer.Location = new System.Drawing.Point(3, 3);
            this.pnlOneColorCalibrationViewer.Name = "pnlOneColorCalibrationViewer";
            this.tableLayoutPanel1.SetRowSpan(this.pnlOneColorCalibrationViewer, 2);
            this.pnlOneColorCalibrationViewer.Size = new System.Drawing.Size(1143, 643);
            this.pnlOneColorCalibrationViewer.TabIndex = 2;
            this.pnlOneColorCalibrationViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlOneColorCalibrationViewer_MouseDown);
            this.pnlOneColorCalibrationViewer.MouseEnter += new System.EventHandler(this.pnlOneColorCalibrationViewer_MouseEnter);
            this.pnlOneColorCalibrationViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlOneColorCalibrationViewer_MouseMove);
            this.pnlOneColorCalibrationViewer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlOneColorCalibrationViewer_MouseUp);
            // 
            // lblMatrixColumn
            // 
            this.lblMatrixColumn.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMatrixColumn.AutoSize = true;
            this.lblMatrixColumn.Depth = 0;
            this.lblMatrixColumn.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblMatrixColumn.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.lblMatrixColumn.Location = new System.Drawing.Point(1193, 695);
            this.lblMatrixColumn.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMatrixColumn.Name = "lblMatrixColumn";
            this.lblMatrixColumn.Size = new System.Drawing.Size(60, 17);
            this.lblMatrixColumn.TabIndex = 40;
            this.lblMatrixColumn.Text = "Column : ";
            this.lblMatrixColumn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbxRecipeList
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.lbxRecipeList, 2);
            this.lbxRecipeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxRecipeList.FormattingEnabled = true;
            this.lbxRecipeList.ItemHeight = 30;
            this.lbxRecipeList.Location = new System.Drawing.Point(1152, 71);
            this.lbxRecipeList.Name = "lbxRecipeList";
            this.lbxRecipeList.Size = new System.Drawing.Size(219, 575);
            this.lbxRecipeList.TabIndex = 46;
            this.lbxRecipeList.SelectedIndexChanged += new System.EventHandler(this.lbxRecipeList_SelectedIndexChanged);
            this.lbxRecipeList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbxRecipeList_MouseDown);
            // 
            // materialLabel3
            // 
            this.materialLabel3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel3.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel3.Location = new System.Drawing.Point(1178, 25);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(49, 17);
            this.materialLabel3.TabIndex = 47;
            this.materialLabel3.Text = "Current";
            this.materialLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMatrixRow
            // 
            this.lblMatrixRow.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMatrixRow.AutoSize = true;
            this.lblMatrixRow.Depth = 0;
            this.lblMatrixRow.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblMatrixRow.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.lblMatrixRow.Location = new System.Drawing.Point(1215, 658);
            this.lblMatrixRow.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMatrixRow.Name = "lblMatrixRow";
            this.lblMatrixRow.Size = new System.Drawing.Size(38, 17);
            this.lblMatrixRow.TabIndex = 39;
            this.lblMatrixRow.Text = "Row : ";
            this.lblMatrixRow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbxMatrixRow
            // 
            this.tbxMatrixRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbxMatrixRow.Location = new System.Drawing.Point(1259, 652);
            this.tbxMatrixRow.Name = "tbxMatrixRow";
            this.tbxMatrixRow.Size = new System.Drawing.Size(109, 39);
            this.tbxMatrixRow.TabIndex = 57;
            // 
            // tbxMatrixColumn
            // 
            this.tbxMatrixColumn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbxMatrixColumn.Location = new System.Drawing.Point(1259, 688);
            this.tbxMatrixColumn.Name = "tbxMatrixColumn";
            this.tbxMatrixColumn.Size = new System.Drawing.Size(109, 39);
            this.tbxMatrixColumn.TabIndex = 58;
            // 
            // btnSave
            // 
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true;
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(1260, 728);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(64, 28);
            this.btnSave.TabIndex = 49;
            this.btnSave.Text = "Save";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSave.UseAccentColor = false;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLoad.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLoad.Depth = 0;
            this.btnLoad.HighEmphasis = true;
            this.btnLoad.Icon = null;
            this.btnLoad.Location = new System.Drawing.Point(1153, 728);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLoad.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLoad.Size = new System.Drawing.Size(64, 28);
            this.btnLoad.TabIndex = 50;
            this.btnLoad.Text = "Load";
            this.btnLoad.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLoad.UseAccentColor = false;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // AutoLum_btn
            // 
            this.AutoLum_btn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AutoLum_btn.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.AutoLum_btn.Depth = 0;
            this.AutoLum_btn.HighEmphasis = true;
            this.AutoLum_btn.Icon = null;
            this.AutoLum_btn.Location = new System.Drawing.Point(1260, 807);
            this.AutoLum_btn.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.AutoLum_btn.MouseState = MaterialSkin.MouseState.HOVER;
            this.AutoLum_btn.Name = "AutoLum_btn";
            this.AutoLum_btn.NoAccentTextColor = System.Drawing.Color.Empty;
            this.AutoLum_btn.Size = new System.Drawing.Size(90, 36);
            this.AutoLum_btn.TabIndex = 27;
            this.AutoLum_btn.Text = "AutoLum";
            this.AutoLum_btn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.AutoLum_btn.UseAccentColor = false;
            this.AutoLum_btn.UseVisualStyleBackColor = true;
            this.AutoLum_btn.Click += new System.EventHandler(this.AutoLum_btn_Click);
            // 
            // btnLoad_Lum_Cx_Cy
            // 
            this.btnLoad_Lum_Cx_Cy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad_Lum_Cx_Cy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.btnLoad_Lum_Cx_Cy, 2);
            this.btnLoad_Lum_Cx_Cy.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLoad_Lum_Cx_Cy.Depth = 0;
            this.btnLoad_Lum_Cx_Cy.HighEmphasis = true;
            this.btnLoad_Lum_Cx_Cy.Icon = null;
            this.btnLoad_Lum_Cx_Cy.Location = new System.Drawing.Point(1153, 768);
            this.btnLoad_Lum_Cx_Cy.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLoad_Lum_Cx_Cy.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLoad_Lum_Cx_Cy.Name = "btnLoad_Lum_Cx_Cy";
            this.btnLoad_Lum_Cx_Cy.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLoad_Lum_Cx_Cy.Size = new System.Drawing.Size(217, 27);
            this.btnLoad_Lum_Cx_Cy.TabIndex = 63;
            this.btnLoad_Lum_Cx_Cy.Text = "Load Lum Cx Cy";
            this.btnLoad_Lum_Cx_Cy.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLoad_Lum_Cx_Cy.UseAccentColor = false;
            this.btnLoad_Lum_Cx_Cy.UseVisualStyleBackColor = true;
            this.btnLoad_Lum_Cx_Cy.Click += new System.EventHandler(this.btnLoad_Lum_Cx_Cy_Click);
            // 
            // AddCIELuminance_mcb
            // 
            this.AddCIELuminance_mcb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddCIELuminance_mcb.AutoSize = true;
            this.AddCIELuminance_mcb.Depth = 0;
            this.AddCIELuminance_mcb.Location = new System.Drawing.Point(1149, 801);
            this.AddCIELuminance_mcb.Margin = new System.Windows.Forms.Padding(0);
            this.AddCIELuminance_mcb.MouseLocation = new System.Drawing.Point(-1, -1);
            this.AddCIELuminance_mcb.MouseState = MaterialSkin.MouseState.HOVER;
            this.AddCIELuminance_mcb.Name = "AddCIELuminance_mcb";
            this.AddCIELuminance_mcb.ReadOnly = false;
            this.AddCIELuminance_mcb.Ripple = true;
            this.AddCIELuminance_mcb.Size = new System.Drawing.Size(107, 37);
            this.AddCIELuminance_mcb.TabIndex = 26;
            this.AddCIELuminance_mcb.Text = "AddLum";
            this.AddCIELuminance_mcb.UseVisualStyleBackColor = true;
            // 
            // contextMenuStripLoadData
            // 
            this.contextMenuStripLoadData.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripLoadData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_LoadData_New,
            this.tsmi_LoadData_Load,
            this.tsmi_LoadData_Rename,
            this.tsmi_LoadData_Delete});
            this.contextMenuStripLoadData.Name = "contextMenuStripLoadData";
            this.contextMenuStripLoadData.Size = new System.Drawing.Size(151, 124);
            // 
            // tsmi_LoadData_New
            // 
            this.tsmi_LoadData_New.Name = "tsmi_LoadData_New";
            this.tsmi_LoadData_New.Size = new System.Drawing.Size(150, 30);
            this.tsmi_LoadData_New.Text = "New";
            this.tsmi_LoadData_New.Click += new System.EventHandler(this.tsmi_LoadData_New_Click);
            // 
            // tsmi_LoadData_Load
            // 
            this.tsmi_LoadData_Load.Name = "tsmi_LoadData_Load";
            this.tsmi_LoadData_Load.Size = new System.Drawing.Size(150, 30);
            this.tsmi_LoadData_Load.Text = "Load";
            this.tsmi_LoadData_Load.Click += new System.EventHandler(this.tsmi_LoadData_Load_Click);
            // 
            // tsmi_LoadData_Rename
            // 
            this.tsmi_LoadData_Rename.Name = "tsmi_LoadData_Rename";
            this.tsmi_LoadData_Rename.Size = new System.Drawing.Size(150, 30);
            this.tsmi_LoadData_Rename.Text = "Rename";
            this.tsmi_LoadData_Rename.Click += new System.EventHandler(this.tsmi_LoadData_Rename_Click);
            // 
            // tsmi_LoadData_Delete
            // 
            this.tsmi_LoadData_Delete.Name = "tsmi_LoadData_Delete";
            this.tsmi_LoadData_Delete.Size = new System.Drawing.Size(150, 30);
            this.tsmi_LoadData_Delete.Text = "Delete";
            this.tsmi_LoadData_Delete.Click += new System.EventHandler(this.tsmi_LoadData_Delete_Click);
            // 
            // frmOneColorCorrection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1396, 917);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DrawerUseColors = true;
            this.Name = "frmOneColorCorrection";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "One Color Correction Data Create";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmOneColorCorrection_FormClosing);
            this.Load += new System.EventHandler(this.frmOneColorCorrection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoi)).EndInit();
            this.contextMenuStripRoiEditor.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStripLoadData.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvRoi;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRoiEditor;
        private System.Windows.Forms.ToolStripMenuItem tsmi_RoiEditor_Copy;
        private System.Windows.Forms.Panel pnlOneColorCalibrationViewer;
        private System.Windows.Forms.ToolStripMenuItem tsmi_RoiEditor_New;
        private System.Windows.Forms.ToolStripMenuItem tsmi_RoiEditor_Delete;
        private MaterialSkin.Controls.MaterialLabel lblMatrixColumn;
        private System.Windows.Forms.ListBox lbxRecipeList;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialButton btnLoad;
        private MaterialSkin.Controls.MaterialButton btnSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLoadData;
        private System.Windows.Forms.ToolStripMenuItem tsmi_LoadData_Load;
        private System.Windows.Forms.ToolStripMenuItem tsmi_LoadData_Delete;
        private System.Windows.Forms.ToolStripMenuItem tsmi_LoadData_Rename;
        private System.Windows.Forms.ToolStripMenuItem tsmi_LoadData_New;
        private System.Windows.Forms.TextBox tbxMatrixColumn;
        private System.Windows.Forms.TextBox tbxCailbrationFile;
        private MaterialSkin.Controls.MaterialButton btnLoad_Lum_Cx_Cy;
        private MaterialSkin.Controls.MaterialLabel lblMatrixRow;
        private System.Windows.Forms.TextBox tbxMatrixRow;
        private System.Windows.Forms.DataGridViewTextBoxColumn ROI_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn CenterX;
        private System.Windows.Forms.DataGridViewTextBoxColumn CenterY;
        private System.Windows.Forms.DataGridViewTextBoxColumn Radius;
        private System.Windows.Forms.DataGridViewTextBoxColumn Luminance;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cx;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cy;
        private MaterialSkin.Controls.MaterialCheckbox AddCIELuminance_mcb;
        private MaterialSkin.Controls.MaterialButton AutoLum_btn;
    }
}