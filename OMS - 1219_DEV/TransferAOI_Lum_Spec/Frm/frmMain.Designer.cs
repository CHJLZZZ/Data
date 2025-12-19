namespace OpticalMeasuringSystem
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_System = new System.Windows.Forms.ToolStripMenuItem();
            this.paremeterSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox_NewRecipeName = new System.Windows.Forms.ToolStripTextBox();
            this.addRecipeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutAUOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.CorrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_29M = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuManualForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Recipe = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuRecipeChoose = new System.Windows.Forms.ToolStripComboBox();
            this.contextMenuStripSaveResultItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tristimulusXToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tristimulusYToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tristimulusZToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.chromaXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chromaYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PnlGrabViewer = new System.Windows.Forms.Panel();
            this.lblCameraTemperature = new MaterialSkin.Controls.MaterialLabel();
            this.lbPanelShowMsg = new System.Windows.Forms.Label();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.Switch_SpectralCorrection = new MaterialSkin.Controls.MaterialSwitch();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnDrawRoi = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveResultImage = new MaterialSkin.Controls.MaterialButton();
            this.btnStop = new MaterialSkin.Controls.MaterialButton();
            this.btnStart = new MaterialSkin.Controls.MaterialButton();
            this.btnLoadFile = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveAs = new MaterialSkin.Controls.MaterialButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl_Logger = new System.Windows.Forms.TabControl();
            this.tabPage_GeneralLogger = new System.Windows.Forms.TabPage();
            this.richTextBox_LoggerGeneral = new System.Windows.Forms.RichTextBox();
            this.tabPage_DebugLogger = new System.Windows.Forms.TabPage();
            this.richTextBox_LoggerDebug = new System.Windows.Forms.RichTextBox();
            this.tabPage_ErrorLogger = new System.Windows.Forms.TabPage();
            this.richTextBox_LoggerError = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewROI = new System.Windows.Forms.DataGridView();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnRadius = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnCx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnCy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbxCalibrationFile = new MaterialSkin.Controls.MaterialComboBox();
            this.lblCalibrationFile = new MaterialSkin.Controls.MaterialLabel();
            this.btn_dataGridViewROI_Load = new MaterialSkin.Controls.MaterialButton();
            this.btn_dataGridViewROI_Clear = new MaterialSkin.Controls.MaterialButton();
            this.btn_dataGridViewROI_SaveCSV = new MaterialSkin.Controls.MaterialButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Label_FocusPos = new System.Windows.Forms.Label();
            this.Label_Aperture = new System.Windows.Forms.Label();
            this.Label_FW1 = new System.Windows.Forms.Label();
            this.Label_FW2 = new System.Windows.Forms.Label();
            this.materialCheckbox1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox2 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox3 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox4 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox5 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox6 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox7 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox8 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox9 = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox10 = new MaterialSkin.Controls.MaterialCheckbox();
            this.contextMenuStripRoiEditor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteROIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripPanelInfo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.positionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.luminanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chromaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStripSaveResultItem.SuspendLayout();
            this.PnlGrabViewer.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl_Logger.SuspendLayout();
            this.tabPage_GeneralLogger.SuspendLayout();
            this.tabPage_DebugLogger.SuspendLayout();
            this.tabPage_ErrorLogger.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewROI)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStripRoiEditor.SuspendLayout();
            this.contextMenuStripPanelInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.menuStrip1, 2);
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStrip1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_System,
            this.toolStripMenuItem_29M,
            this.toolStripMenuItem_Recipe,
            this.toolStripMenuRecipeChoose});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1068, 30);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem_System
            // 
            this.toolStripMenuItem_System.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.paremeterSettingToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripMenuItem1,
            this.toolStripSeparator2,
            this.AboutAUOToolStripMenuItem,
            this.toolStripSeparator3,
            this.CorrToolStripMenuItem});
            this.toolStripMenuItem_System.Name = "toolStripMenuItem_System";
            this.toolStripMenuItem_System.Size = new System.Drawing.Size(59, 24);
            this.toolStripMenuItem_System.Text = "System";
            // 
            // paremeterSettingToolStripMenuItem
            // 
            this.paremeterSettingToolStripMenuItem.Name = "paremeterSettingToolStripMenuItem";
            this.paremeterSettingToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.paremeterSettingToolStripMenuItem.Text = "System Setting";
            this.paremeterSettingToolStripMenuItem.Click += new System.EventHandler(this.paremeterSettingToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(173, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox_NewRecipeName,
            this.addRecipeToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.toolStripMenuItem1.Text = "Add Recipe";
            // 
            // toolStripTextBox_NewRecipeName
            // 
            this.toolStripTextBox_NewRecipeName.Name = "toolStripTextBox_NewRecipeName";
            this.toolStripTextBox_NewRecipeName.Size = new System.Drawing.Size(100, 23);
            // 
            // addRecipeToolStripMenuItem
            // 
            this.addRecipeToolStripMenuItem.Name = "addRecipeToolStripMenuItem";
            this.addRecipeToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.addRecipeToolStripMenuItem.Text = "Add Recipe";
            this.addRecipeToolStripMenuItem.Click += new System.EventHandler(this.addRecipeToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // AboutAUOToolStripMenuItem
            // 
            this.AboutAUOToolStripMenuItem.Name = "AboutAUOToolStripMenuItem";
            this.AboutAUOToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.AboutAUOToolStripMenuItem.Text = "About";
            this.AboutAUOToolStripMenuItem.Click += new System.EventHandler(this.AboutAUOToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(173, 6);
            // 
            // CorrToolStripMenuItem
            // 
            this.CorrToolStripMenuItem.Name = "CorrToolStripMenuItem";
            this.CorrToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.CorrToolStripMenuItem.Text = "Correction Setting";
            this.CorrToolStripMenuItem.Click += new System.EventHandler(this.CorrToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_29M
            // 
            this.toolStripMenuItem_29M.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuManualForm});
            this.toolStripMenuItem_29M.Name = "toolStripMenuItem_29M";
            this.toolStripMenuItem_29M.Size = new System.Drawing.Size(47, 24);
            this.toolStripMenuItem_29M.Text = "CAM";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(175, 22);
            this.toolStripMenuItem3.Text = "Parameter Setting";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuManualForm
            // 
            this.toolStripMenuManualForm.Name = "toolStripMenuManualForm";
            this.toolStripMenuManualForm.Size = new System.Drawing.Size(175, 22);
            this.toolStripMenuManualForm.Text = "Manual Form";
            this.toolStripMenuManualForm.Click += new System.EventHandler(this.toolStripMenuManualForm_Click);
            // 
            // toolStripMenuItem_Recipe
            // 
            this.toolStripMenuItem_Recipe.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripMenuItem_Recipe.Enabled = false;
            this.toolStripMenuItem_Recipe.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripMenuItem_Recipe.Name = "toolStripMenuItem_Recipe";
            this.toolStripMenuItem_Recipe.Size = new System.Drawing.Size(61, 24);
            this.toolStripMenuItem_Recipe.Text = "Recipe:";
            // 
            // toolStripMenuRecipeChoose
            // 
            this.toolStripMenuRecipeChoose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripMenuRecipeChoose.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuRecipeChoose.Name = "toolStripMenuRecipeChoose";
            this.toolStripMenuRecipeChoose.Size = new System.Drawing.Size(145, 24);
            this.toolStripMenuRecipeChoose.Sorted = true;
            this.toolStripMenuRecipeChoose.DropDown += new System.EventHandler(this.recipechoosetoolStripMenuItem_DropDown);
            this.toolStripMenuRecipeChoose.SelectedIndexChanged += new System.EventHandler(this.recipechoosetoolStripMenuItem_SelectedIndexChanged);
            // 
            // contextMenuStripSaveResultItem
            // 
            this.contextMenuStripSaveResultItem.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripSaveResultItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tristimulusXToolStripMenuItem1,
            this.tristimulusYToolStripMenuItem1,
            this.tristimulusZToolStripMenuItem1,
            this.chromaXToolStripMenuItem,
            this.chromaYToolStripMenuItem});
            this.contextMenuStripSaveResultItem.Name = "contextMenuStripSaveResultItem";
            this.contextMenuStripSaveResultItem.ShowCheckMargin = true;
            this.contextMenuStripSaveResultItem.Size = new System.Drawing.Size(167, 114);
            // 
            // tristimulusXToolStripMenuItem1
            // 
            this.tristimulusXToolStripMenuItem1.Checked = true;
            this.tristimulusXToolStripMenuItem1.CheckOnClick = true;
            this.tristimulusXToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tristimulusXToolStripMenuItem1.Name = "tristimulusXToolStripMenuItem1";
            this.tristimulusXToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.tristimulusXToolStripMenuItem1.Text = "Tristimulus X";
            // 
            // tristimulusYToolStripMenuItem1
            // 
            this.tristimulusYToolStripMenuItem1.Checked = true;
            this.tristimulusYToolStripMenuItem1.CheckOnClick = true;
            this.tristimulusYToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tristimulusYToolStripMenuItem1.Name = "tristimulusYToolStripMenuItem1";
            this.tristimulusYToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.tristimulusYToolStripMenuItem1.Text = "Tristimulus Y";
            // 
            // tristimulusZToolStripMenuItem1
            // 
            this.tristimulusZToolStripMenuItem1.Checked = true;
            this.tristimulusZToolStripMenuItem1.CheckOnClick = true;
            this.tristimulusZToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tristimulusZToolStripMenuItem1.Name = "tristimulusZToolStripMenuItem1";
            this.tristimulusZToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.tristimulusZToolStripMenuItem1.Text = "Tristimulus Z";
            // 
            // chromaXToolStripMenuItem
            // 
            this.chromaXToolStripMenuItem.Checked = true;
            this.chromaXToolStripMenuItem.CheckOnClick = true;
            this.chromaXToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chromaXToolStripMenuItem.Name = "chromaXToolStripMenuItem";
            this.chromaXToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.chromaXToolStripMenuItem.Text = "Chroma X";
            // 
            // chromaYToolStripMenuItem
            // 
            this.chromaYToolStripMenuItem.Checked = true;
            this.chromaYToolStripMenuItem.CheckOnClick = true;
            this.chromaYToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chromaYToolStripMenuItem.Name = "chromaYToolStripMenuItem";
            this.chromaYToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.chromaYToolStripMenuItem.Text = "Chroma Y";
            // 
            // PnlGrabViewer
            // 
            this.PnlGrabViewer.BackColor = System.Drawing.Color.Black;
            this.PnlGrabViewer.Controls.Add(this.lblCameraTemperature);
            this.PnlGrabViewer.Controls.Add(this.lbPanelShowMsg);
            this.PnlGrabViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlGrabViewer.Location = new System.Drawing.Point(3, 110);
            this.PnlGrabViewer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PnlGrabViewer.Name = "PnlGrabViewer";
            this.tableLayoutPanel3.SetRowSpan(this.PnlGrabViewer, 5);
            this.PnlGrabViewer.Size = new System.Drawing.Size(903, 688);
            this.PnlGrabViewer.TabIndex = 0;
            this.PnlGrabViewer.MouseEnter += new System.EventHandler(this.PnlGrabViewer_MouseEnter);
            this.PnlGrabViewer.MouseLeave += new System.EventHandler(this.PnlGrabViewer_MouseLeave);
            this.PnlGrabViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PnlGrabViewer_MouseMove);
            // 
            // lblCameraTemperature
            // 
            this.lblCameraTemperature.AutoSize = true;
            this.lblCameraTemperature.Depth = 0;
            this.lblCameraTemperature.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblCameraTemperature.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.lblCameraTemperature.HighEmphasis = true;
            this.lblCameraTemperature.Location = new System.Drawing.Point(14, 12);
            this.lblCameraTemperature.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblCameraTemperature.Name = "lblCameraTemperature";
            this.lblCameraTemperature.Size = new System.Drawing.Size(125, 17);
            this.lblCameraTemperature.TabIndex = 32;
            this.lblCameraTemperature.Text = "Camera Tempature";
            this.lblCameraTemperature.UseAccent = true;
            // 
            // lbPanelShowMsg
            // 
            this.lbPanelShowMsg.AutoSize = true;
            this.lbPanelShowMsg.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lbPanelShowMsg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(95)))), ((int)(((byte)(90)))));
            this.lbPanelShowMsg.Location = new System.Drawing.Point(14, 12);
            this.lbPanelShowMsg.Name = "lbPanelShowMsg";
            this.lbPanelShowMsg.Size = new System.Drawing.Size(49, 18);
            this.lbPanelShowMsg.TabIndex = 10;
            this.lbPanelShowMsg.Text = "label1";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 159F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.Controls.Add(this.Switch_SpectralCorrection, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.menuStrip1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.PnlGrabViewer, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.splitContainer1, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel1, 1, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Font = new System.Drawing.Font("微軟正黑體", 29F);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 64);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 7;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.36842F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.63158F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1417, 802);
            this.tableLayoutPanel3.TabIndex = 31;
            // 
            // Switch_SpectralCorrection
            // 
            this.Switch_SpectralCorrection.AutoSize = true;
            this.Switch_SpectralCorrection.Depth = 0;
            this.Switch_SpectralCorrection.Location = new System.Drawing.Point(1217, 0);
            this.Switch_SpectralCorrection.Margin = new System.Windows.Forms.Padding(0);
            this.Switch_SpectralCorrection.MouseLocation = new System.Drawing.Point(-1, -1);
            this.Switch_SpectralCorrection.MouseState = MaterialSkin.MouseState.HOVER;
            this.Switch_SpectralCorrection.Name = "Switch_SpectralCorrection";
            this.Switch_SpectralCorrection.Ripple = true;
            this.Switch_SpectralCorrection.Size = new System.Drawing.Size(192, 30);
            this.Switch_SpectralCorrection.TabIndex = 53;
            this.Switch_SpectralCorrection.Text = "Spectral Correction";
            this.Switch_SpectralCorrection.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.Controls.Add(this.btnDrawRoi, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveResultImage, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStop, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStart, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnLoadFile, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveAs, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 33);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(903, 70);
            this.tableLayoutPanel2.TabIndex = 40;
            // 
            // btnDrawRoi
            // 
            this.btnDrawRoi.AutoSize = false;
            this.btnDrawRoi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDrawRoi.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDrawRoi.Depth = 0;
            this.btnDrawRoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDrawRoi.HighEmphasis = false;
            this.btnDrawRoi.Icon = ((System.Drawing.Image)(resources.GetObject("btnDrawRoi.Icon")));
            this.btnDrawRoi.Location = new System.Drawing.Point(754, 6);
            this.btnDrawRoi.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDrawRoi.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDrawRoi.Name = "btnDrawRoi";
            this.btnDrawRoi.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDrawRoi.Size = new System.Drawing.Size(145, 58);
            this.btnDrawRoi.TabIndex = 32;
            this.btnDrawRoi.Text = "ROI\r\nAnalysis";
            this.btnDrawRoi.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnDrawRoi.UseAccentColor = false;
            this.btnDrawRoi.UseVisualStyleBackColor = true;
            this.btnDrawRoi.Click += new System.EventHandler(this.btnDrawRoi_Click);
            // 
            // btnSaveResultImage
            // 
            this.btnSaveResultImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveResultImage.ContextMenuStrip = this.contextMenuStripSaveResultItem;
            this.btnSaveResultImage.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveResultImage.Depth = 0;
            this.btnSaveResultImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveResultImage.HighEmphasis = false;
            this.btnSaveResultImage.Icon = ((System.Drawing.Image)(resources.GetObject("btnSaveResultImage.Icon")));
            this.btnSaveResultImage.Location = new System.Drawing.Point(604, 6);
            this.btnSaveResultImage.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveResultImage.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveResultImage.Name = "btnSaveResultImage";
            this.btnSaveResultImage.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveResultImage.Size = new System.Drawing.Size(142, 58);
            this.btnSaveResultImage.TabIndex = 38;
            this.btnSaveResultImage.Text = "Save\r\nImage";
            this.btnSaveResultImage.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveResultImage.UseAccentColor = false;
            this.btnSaveResultImage.UseVisualStyleBackColor = true;
            this.btnSaveResultImage.Click += new System.EventHandler(this.BtnSaveColorResult_Click);
            // 
            // btnStop
            // 
            this.btnStop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnStop.Depth = 0;
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStop.HighEmphasis = true;
            this.btnStop.Icon = ((System.Drawing.Image)(resources.GetObject("btnStop.Icon")));
            this.btnStop.Location = new System.Drawing.Point(154, 6);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnStop.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStop.Name = "btnStop";
            this.btnStop.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnStop.Size = new System.Drawing.Size(142, 58);
            this.btnStop.TabIndex = 29;
            this.btnStop.Text = "Stop";
            this.btnStop.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnStop.UseAccentColor = false;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnStart.Depth = 0;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStart.HighEmphasis = true;
            this.btnStart.Icon = ((System.Drawing.Image)(resources.GetObject("btnStart.Icon")));
            this.btnStart.Location = new System.Drawing.Point(4, 6);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnStart.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStart.Name = "btnStart";
            this.btnStart.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnStart.Size = new System.Drawing.Size(142, 58);
            this.btnStart.TabIndex = 28;
            this.btnStart.Text = "Start";
            this.btnStart.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnStart.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnStart.UseAccentColor = false;
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLoadFile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLoadFile.Depth = 0;
            this.btnLoadFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadFile.HighEmphasis = false;
            this.btnLoadFile.Icon = ((System.Drawing.Image)(resources.GetObject("btnLoadFile.Icon")));
            this.btnLoadFile.Location = new System.Drawing.Point(304, 6);
            this.btnLoadFile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLoadFile.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLoadFile.Size = new System.Drawing.Size(142, 58);
            this.btnLoadFile.TabIndex = 40;
            this.btnLoadFile.Text = "Open Workspace";
            this.btnLoadFile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLoadFile.UseAccentColor = false;
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAs.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAs.Depth = 0;
            this.btnSaveAs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAs.HighEmphasis = false;
            this.btnSaveAs.Icon = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Icon")));
            this.btnSaveAs.Location = new System.Drawing.Point(454, 6);
            this.btnSaveAs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAs.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAs.Size = new System.Drawing.Size(142, 58);
            this.btnSaveAs.TabIndex = 41;
            this.btnSaveAs.Text = "Save Workspace";
            this.btnSaveAs.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAs.UseAccentColor = false;
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // splitContainer1
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.splitContainer1, 3);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.splitContainer1.Location = new System.Drawing.Point(912, 209);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl_Logger);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel4);
            this.tableLayoutPanel3.SetRowSpan(this.splitContainer1, 4);
            this.splitContainer1.Size = new System.Drawing.Size(502, 590);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 43;
            // 
            // tabControl_Logger
            // 
            this.tabControl_Logger.Controls.Add(this.tabPage_GeneralLogger);
            this.tabControl_Logger.Controls.Add(this.tabPage_DebugLogger);
            this.tabControl_Logger.Controls.Add(this.tabPage_ErrorLogger);
            this.tabControl_Logger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Logger.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl_Logger.Location = new System.Drawing.Point(0, 0);
            this.tabControl_Logger.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl_Logger.Name = "tabControl_Logger";
            this.tabControl_Logger.SelectedIndex = 0;
            this.tabControl_Logger.Size = new System.Drawing.Size(240, 590);
            this.tabControl_Logger.TabIndex = 16;
            // 
            // tabPage_GeneralLogger
            // 
            this.tabPage_GeneralLogger.Controls.Add(this.richTextBox_LoggerGeneral);
            this.tabPage_GeneralLogger.Location = new System.Drawing.Point(4, 28);
            this.tabPage_GeneralLogger.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_GeneralLogger.Name = "tabPage_GeneralLogger";
            this.tabPage_GeneralLogger.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_GeneralLogger.Size = new System.Drawing.Size(232, 558);
            this.tabPage_GeneralLogger.TabIndex = 0;
            this.tabPage_GeneralLogger.Text = "General";
            this.tabPage_GeneralLogger.UseVisualStyleBackColor = true;
            // 
            // richTextBox_LoggerGeneral
            // 
            this.richTextBox_LoggerGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_LoggerGeneral.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_LoggerGeneral.Location = new System.Drawing.Point(3, 4);
            this.richTextBox_LoggerGeneral.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.richTextBox_LoggerGeneral.Name = "richTextBox_LoggerGeneral";
            this.richTextBox_LoggerGeneral.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox_LoggerGeneral.Size = new System.Drawing.Size(226, 550);
            this.richTextBox_LoggerGeneral.TabIndex = 9;
            this.richTextBox_LoggerGeneral.Text = "";
            // 
            // tabPage_DebugLogger
            // 
            this.tabPage_DebugLogger.Controls.Add(this.richTextBox_LoggerDebug);
            this.tabPage_DebugLogger.Location = new System.Drawing.Point(4, 28);
            this.tabPage_DebugLogger.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_DebugLogger.Name = "tabPage_DebugLogger";
            this.tabPage_DebugLogger.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_DebugLogger.Size = new System.Drawing.Size(232, 558);
            this.tabPage_DebugLogger.TabIndex = 3;
            this.tabPage_DebugLogger.Text = "Debug";
            this.tabPage_DebugLogger.UseVisualStyleBackColor = true;
            // 
            // richTextBox_LoggerDebug
            // 
            this.richTextBox_LoggerDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_LoggerDebug.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_LoggerDebug.Location = new System.Drawing.Point(3, 4);
            this.richTextBox_LoggerDebug.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.richTextBox_LoggerDebug.Name = "richTextBox_LoggerDebug";
            this.richTextBox_LoggerDebug.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox_LoggerDebug.Size = new System.Drawing.Size(226, 550);
            this.richTextBox_LoggerDebug.TabIndex = 10;
            this.richTextBox_LoggerDebug.Text = "";
            // 
            // tabPage_ErrorLogger
            // 
            this.tabPage_ErrorLogger.Controls.Add(this.richTextBox_LoggerError);
            this.tabPage_ErrorLogger.Location = new System.Drawing.Point(4, 28);
            this.tabPage_ErrorLogger.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_ErrorLogger.Name = "tabPage_ErrorLogger";
            this.tabPage_ErrorLogger.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_ErrorLogger.Size = new System.Drawing.Size(232, 558);
            this.tabPage_ErrorLogger.TabIndex = 2;
            this.tabPage_ErrorLogger.Text = "Error";
            this.tabPage_ErrorLogger.UseVisualStyleBackColor = true;
            // 
            // richTextBox_LoggerError
            // 
            this.richTextBox_LoggerError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_LoggerError.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_LoggerError.Location = new System.Drawing.Point(3, 4);
            this.richTextBox_LoggerError.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.richTextBox_LoggerError.Name = "richTextBox_LoggerError";
            this.richTextBox_LoggerError.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox_LoggerError.Size = new System.Drawing.Size(226, 550);
            this.richTextBox_LoggerError.TabIndex = 0;
            this.richTextBox_LoggerError.Text = "";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.Controls.Add(this.dataGridViewROI, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tbxCalibrationFile, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.lblCalibrationFile, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.btn_dataGridViewROI_Load, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.btn_dataGridViewROI_Clear, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.btn_dataGridViewROI_SaveCSV, 2, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(258, 590);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // dataGridViewROI
            // 
            this.dataGridViewROI.AllowUserToAddRows = false;
            this.dataGridViewROI.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewROI.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewROI.ColumnHeadersHeight = 50;
            this.dataGridViewROI.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewROI.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnName,
            this.ColumnX,
            this.ColumnY,
            this.ColumnRadius,
            this.ColumnLum,
            this.ColumnCx,
            this.ColumnCy});
            this.tableLayoutPanel4.SetColumnSpan(this.dataGridViewROI, 3);
            this.dataGridViewROI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewROI.EnableHeadersVisualStyles = false;
            this.dataGridViewROI.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewROI.Name = "dataGridViewROI";
            this.dataGridViewROI.ReadOnly = true;
            this.dataGridViewROI.RowHeadersWidth = 20;
            this.dataGridViewROI.RowTemplate.Height = 24;
            this.dataGridViewROI.Size = new System.Drawing.Size(252, 504);
            this.dataGridViewROI.TabIndex = 0;
            this.dataGridViewROI.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridViewROI_CellMouseUp);
            this.dataGridViewROI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridViewROI_MouseDown);
            // 
            // ColumnName
            // 
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.MinimumWidth = 6;
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            this.ColumnName.Visible = false;
            // 
            // ColumnX
            // 
            this.ColumnX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnX.HeaderText = "X";
            this.ColumnX.MinimumWidth = 6;
            this.ColumnX.Name = "ColumnX";
            this.ColumnX.ReadOnly = true;
            // 
            // ColumnY
            // 
            this.ColumnY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnY.HeaderText = "Y";
            this.ColumnY.MinimumWidth = 6;
            this.ColumnY.Name = "ColumnY";
            this.ColumnY.ReadOnly = true;
            // 
            // ColumnRadius
            // 
            this.ColumnRadius.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnRadius.HeaderText = "Radius";
            this.ColumnRadius.MinimumWidth = 6;
            this.ColumnRadius.Name = "ColumnRadius";
            this.ColumnRadius.ReadOnly = true;
            // 
            // ColumnLum
            // 
            this.ColumnLum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnLum.HeaderText = "Lum";
            this.ColumnLum.MinimumWidth = 6;
            this.ColumnLum.Name = "ColumnLum";
            this.ColumnLum.ReadOnly = true;
            // 
            // ColumnCx
            // 
            this.ColumnCx.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnCx.HeaderText = "Cx";
            this.ColumnCx.MinimumWidth = 6;
            this.ColumnCx.Name = "ColumnCx";
            this.ColumnCx.ReadOnly = true;
            // 
            // ColumnCy
            // 
            this.ColumnCy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnCy.HeaderText = "Cy";
            this.ColumnCy.MinimumWidth = 6;
            this.ColumnCy.Name = "ColumnCy";
            this.ColumnCy.ReadOnly = true;
            // 
            // tbxCalibrationFile
            // 
            this.tbxCalibrationFile.AutoResize = false;
            this.tbxCalibrationFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tableLayoutPanel4.SetColumnSpan(this.tbxCalibrationFile, 2);
            this.tbxCalibrationFile.Depth = 0;
            this.tbxCalibrationFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxCalibrationFile.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.tbxCalibrationFile.DropDownHeight = 118;
            this.tbxCalibrationFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tbxCalibrationFile.DropDownWidth = 121;
            this.tbxCalibrationFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.tbxCalibrationFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.tbxCalibrationFile.FormattingEnabled = true;
            this.tbxCalibrationFile.IntegralHeight = false;
            this.tbxCalibrationFile.ItemHeight = 29;
            this.tbxCalibrationFile.Location = new System.Drawing.Point(89, 513);
            this.tbxCalibrationFile.MaxDropDownItems = 4;
            this.tbxCalibrationFile.MouseState = MaterialSkin.MouseState.OUT;
            this.tbxCalibrationFile.Name = "tbxCalibrationFile";
            this.tbxCalibrationFile.Size = new System.Drawing.Size(166, 35);
            this.tbxCalibrationFile.StartIndex = 3;
            this.tbxCalibrationFile.TabIndex = 48;
            this.tbxCalibrationFile.UseTallSize = false;
            this.tbxCalibrationFile.Visible = false;
            // 
            // lblCalibrationFile
            // 
            this.lblCalibrationFile.AutoSize = true;
            this.lblCalibrationFile.Depth = 0;
            this.lblCalibrationFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCalibrationFile.Font = new System.Drawing.Font("Roboto Medium", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblCalibrationFile.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            this.lblCalibrationFile.Location = new System.Drawing.Point(3, 510);
            this.lblCalibrationFile.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblCalibrationFile.Name = "lblCalibrationFile";
            this.lblCalibrationFile.Size = new System.Drawing.Size(80, 40);
            this.lblCalibrationFile.TabIndex = 47;
            this.lblCalibrationFile.Text = "Calibration File :";
            this.lblCalibrationFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCalibrationFile.Visible = false;
            // 
            // btn_dataGridViewROI_Load
            // 
            this.btn_dataGridViewROI_Load.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_dataGridViewROI_Load.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btn_dataGridViewROI_Load.Depth = 0;
            this.btn_dataGridViewROI_Load.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_dataGridViewROI_Load.HighEmphasis = true;
            this.btn_dataGridViewROI_Load.Icon = null;
            this.btn_dataGridViewROI_Load.Location = new System.Drawing.Point(18, 556);
            this.btn_dataGridViewROI_Load.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btn_dataGridViewROI_Load.MouseState = MaterialSkin.MouseState.HOVER;
            this.btn_dataGridViewROI_Load.Name = "btn_dataGridViewROI_Load";
            this.btn_dataGridViewROI_Load.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btn_dataGridViewROI_Load.Size = new System.Drawing.Size(64, 28);
            this.btn_dataGridViewROI_Load.TabIndex = 44;
            this.btn_dataGridViewROI_Load.Text = "Load";
            this.btn_dataGridViewROI_Load.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btn_dataGridViewROI_Load.UseAccentColor = false;
            this.btn_dataGridViewROI_Load.UseVisualStyleBackColor = true;
            this.btn_dataGridViewROI_Load.Visible = false;
            this.btn_dataGridViewROI_Load.Click += new System.EventHandler(this.btn_dataGridViewROI_Load_Click);
            // 
            // btn_dataGridViewROI_Clear
            // 
            this.btn_dataGridViewROI_Clear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_dataGridViewROI_Clear.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btn_dataGridViewROI_Clear.Depth = 0;
            this.btn_dataGridViewROI_Clear.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_dataGridViewROI_Clear.HighEmphasis = true;
            this.btn_dataGridViewROI_Clear.Icon = null;
            this.btn_dataGridViewROI_Clear.Location = new System.Drawing.Point(102, 556);
            this.btn_dataGridViewROI_Clear.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btn_dataGridViewROI_Clear.MouseState = MaterialSkin.MouseState.HOVER;
            this.btn_dataGridViewROI_Clear.Name = "btn_dataGridViewROI_Clear";
            this.btn_dataGridViewROI_Clear.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btn_dataGridViewROI_Clear.Size = new System.Drawing.Size(66, 28);
            this.btn_dataGridViewROI_Clear.TabIndex = 45;
            this.btn_dataGridViewROI_Clear.Text = "Clear";
            this.btn_dataGridViewROI_Clear.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btn_dataGridViewROI_Clear.UseAccentColor = false;
            this.btn_dataGridViewROI_Clear.UseVisualStyleBackColor = true;
            this.btn_dataGridViewROI_Clear.Visible = false;
            this.btn_dataGridViewROI_Clear.Click += new System.EventHandler(this.btn_dataGridViewROI_Clear_Click);
            // 
            // btn_dataGridViewROI_SaveCSV
            // 
            this.btn_dataGridViewROI_SaveCSV.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_dataGridViewROI_SaveCSV.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btn_dataGridViewROI_SaveCSV.Depth = 0;
            this.btn_dataGridViewROI_SaveCSV.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_dataGridViewROI_SaveCSV.HighEmphasis = true;
            this.btn_dataGridViewROI_SaveCSV.Icon = null;
            this.btn_dataGridViewROI_SaveCSV.Location = new System.Drawing.Point(190, 556);
            this.btn_dataGridViewROI_SaveCSV.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btn_dataGridViewROI_SaveCSV.MouseState = MaterialSkin.MouseState.HOVER;
            this.btn_dataGridViewROI_SaveCSV.Name = "btn_dataGridViewROI_SaveCSV";
            this.btn_dataGridViewROI_SaveCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btn_dataGridViewROI_SaveCSV.Size = new System.Drawing.Size(64, 28);
            this.btn_dataGridViewROI_SaveCSV.TabIndex = 46;
            this.btn_dataGridViewROI_SaveCSV.Text = "Save";
            this.btn_dataGridViewROI_SaveCSV.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btn_dataGridViewROI_SaveCSV.UseAccentColor = false;
            this.btn_dataGridViewROI_SaveCSV.UseVisualStyleBackColor = true;
            this.btn_dataGridViewROI_SaveCSV.Visible = false;
            this.btn_dataGridViewROI_SaveCSV.Click += new System.EventHandler(this.btn_dataGridViewROI_SaveCSV_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel3.SetColumnSpan(this.tableLayoutPanel1, 3);
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.Label_FocusPos, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Label_Aperture, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Label_FW1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Label_FW2, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(912, 33);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel3.SetRowSpan(this.tableLayoutPanel1, 2);
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(502, 170);
            this.tableLayoutPanel1.TabIndex = 51;
            // 
            // Label_FocusPos
            // 
            this.Label_FocusPos.AutoSize = true;
            this.Label_FocusPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label_FocusPos.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_FocusPos.Location = new System.Drawing.Point(3, 3);
            this.Label_FocusPos.Margin = new System.Windows.Forms.Padding(3);
            this.Label_FocusPos.Name = "Label_FocusPos";
            this.Label_FocusPos.Size = new System.Drawing.Size(496, 36);
            this.Label_FocusPos.TabIndex = 0;
            this.Label_FocusPos.Text = "Focuser = ";
            this.Label_FocusPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_Aperture
            // 
            this.Label_Aperture.AutoSize = true;
            this.Label_Aperture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label_Aperture.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_Aperture.Location = new System.Drawing.Point(3, 45);
            this.Label_Aperture.Margin = new System.Windows.Forms.Padding(3);
            this.Label_Aperture.Name = "Label_Aperture";
            this.Label_Aperture.Size = new System.Drawing.Size(496, 36);
            this.Label_Aperture.TabIndex = 1;
            this.Label_Aperture.Text = "Aperture = ";
            this.Label_Aperture.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_FW1
            // 
            this.Label_FW1.AutoSize = true;
            this.Label_FW1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label_FW1.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_FW1.Location = new System.Drawing.Point(3, 87);
            this.Label_FW1.Margin = new System.Windows.Forms.Padding(3);
            this.Label_FW1.Name = "Label_FW1";
            this.Label_FW1.Size = new System.Drawing.Size(496, 36);
            this.Label_FW1.TabIndex = 2;
            this.Label_FW1.Text = "FW1(ND) = ";
            this.Label_FW1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_FW2
            // 
            this.Label_FW2.AutoSize = true;
            this.Label_FW2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label_FW2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_FW2.Location = new System.Drawing.Point(3, 129);
            this.Label_FW2.Margin = new System.Windows.Forms.Padding(3);
            this.Label_FW2.Name = "Label_FW2";
            this.Label_FW2.Size = new System.Drawing.Size(496, 38);
            this.Label_FW2.TabIndex = 3;
            this.Label_FW2.Text = "FW2(XYZ) = ";
            this.Label_FW2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // materialCheckbox1
            // 
            this.materialCheckbox1.Depth = 0;
            this.materialCheckbox1.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox1.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox1.Name = "materialCheckbox1";
            this.materialCheckbox1.ReadOnly = false;
            this.materialCheckbox1.Ripple = true;
            this.materialCheckbox1.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox1.TabIndex = 0;
            this.materialCheckbox1.Text = "materialCheckbox1";
            this.materialCheckbox1.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox2
            // 
            this.materialCheckbox2.Depth = 0;
            this.materialCheckbox2.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox2.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox2.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox2.Name = "materialCheckbox2";
            this.materialCheckbox2.ReadOnly = false;
            this.materialCheckbox2.Ripple = true;
            this.materialCheckbox2.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox2.TabIndex = 0;
            this.materialCheckbox2.Text = "materialCheckbox2";
            this.materialCheckbox2.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox3
            // 
            this.materialCheckbox3.Depth = 0;
            this.materialCheckbox3.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox3.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox3.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox3.Name = "materialCheckbox3";
            this.materialCheckbox3.ReadOnly = false;
            this.materialCheckbox3.Ripple = true;
            this.materialCheckbox3.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox3.TabIndex = 0;
            this.materialCheckbox3.Text = "materialCheckbox3";
            this.materialCheckbox3.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox4
            // 
            this.materialCheckbox4.Depth = 0;
            this.materialCheckbox4.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox4.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox4.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox4.Name = "materialCheckbox4";
            this.materialCheckbox4.ReadOnly = false;
            this.materialCheckbox4.Ripple = true;
            this.materialCheckbox4.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox4.TabIndex = 0;
            this.materialCheckbox4.Text = "materialCheckbox4";
            this.materialCheckbox4.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox5
            // 
            this.materialCheckbox5.Depth = 0;
            this.materialCheckbox5.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox5.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox5.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox5.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox5.Name = "materialCheckbox5";
            this.materialCheckbox5.ReadOnly = false;
            this.materialCheckbox5.Ripple = true;
            this.materialCheckbox5.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox5.TabIndex = 0;
            this.materialCheckbox5.Text = "materialCheckbox5";
            this.materialCheckbox5.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox6
            // 
            this.materialCheckbox6.Depth = 0;
            this.materialCheckbox6.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox6.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox6.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox6.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox6.Name = "materialCheckbox6";
            this.materialCheckbox6.ReadOnly = false;
            this.materialCheckbox6.Ripple = true;
            this.materialCheckbox6.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox6.TabIndex = 0;
            this.materialCheckbox6.Text = "materialCheckbox6";
            this.materialCheckbox6.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox7
            // 
            this.materialCheckbox7.Depth = 0;
            this.materialCheckbox7.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox7.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox7.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox7.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox7.Name = "materialCheckbox7";
            this.materialCheckbox7.ReadOnly = false;
            this.materialCheckbox7.Ripple = true;
            this.materialCheckbox7.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox7.TabIndex = 0;
            this.materialCheckbox7.Text = "materialCheckbox7";
            this.materialCheckbox7.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox8
            // 
            this.materialCheckbox8.Depth = 0;
            this.materialCheckbox8.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox8.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox8.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox8.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox8.Name = "materialCheckbox8";
            this.materialCheckbox8.ReadOnly = false;
            this.materialCheckbox8.Ripple = true;
            this.materialCheckbox8.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox8.TabIndex = 0;
            this.materialCheckbox8.Text = "Algorithm Package";
            this.materialCheckbox8.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox9
            // 
            this.materialCheckbox9.Depth = 0;
            this.materialCheckbox9.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox9.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox9.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox9.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox9.Name = "materialCheckbox9";
            this.materialCheckbox9.ReadOnly = false;
            this.materialCheckbox9.Ripple = true;
            this.materialCheckbox9.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox9.TabIndex = 0;
            this.materialCheckbox9.Text = "Camera";
            this.materialCheckbox9.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox10
            // 
            this.materialCheckbox10.Depth = 0;
            this.materialCheckbox10.Location = new System.Drawing.Point(0, 0);
            this.materialCheckbox10.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox10.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox10.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox10.Name = "materialCheckbox10";
            this.materialCheckbox10.ReadOnly = false;
            this.materialCheckbox10.Ripple = true;
            this.materialCheckbox10.Size = new System.Drawing.Size(104, 37);
            this.materialCheckbox10.TabIndex = 0;
            this.materialCheckbox10.Text = "d";
            this.materialCheckbox10.UseVisualStyleBackColor = true;
            // 
            // contextMenuStripRoiEditor
            // 
            this.contextMenuStripRoiEditor.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripRoiEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.deleteROIToolStripMenuItem});
            this.contextMenuStripRoiEditor.Name = "contextMenuStripRoiEditor";
            this.contextMenuStripRoiEditor.Size = new System.Drawing.Size(197, 70);
            this.contextMenuStripRoiEditor.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripRoiEditor_Opening);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.addToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.addToolStripMenuItem.Text = "Add New ROI";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.copyToolStripMenuItem.Text = "Copy ROI";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // deleteROIToolStripMenuItem
            // 
            this.deleteROIToolStripMenuItem.Name = "deleteROIToolStripMenuItem";
            this.deleteROIToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteROIToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.deleteROIToolStripMenuItem.Text = "Delete ROI";
            this.deleteROIToolStripMenuItem.Click += new System.EventHandler(this.DeleteROIToolStripMenuItem_Click);
            // 
            // contextMenuStripPanelInfo
            // 
            this.contextMenuStripPanelInfo.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripPanelInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.positionToolStripMenuItem,
            this.luminanceToolStripMenuItem,
            this.chromaToolStripMenuItem});
            this.contextMenuStripPanelInfo.Name = "contextMenuStripPanelInfo";
            this.contextMenuStripPanelInfo.Size = new System.Drawing.Size(136, 70);
            // 
            // positionToolStripMenuItem
            // 
            this.positionToolStripMenuItem.Enabled = false;
            this.positionToolStripMenuItem.Name = "positionToolStripMenuItem";
            this.positionToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.positionToolStripMenuItem.Text = "Position";
            // 
            // luminanceToolStripMenuItem
            // 
            this.luminanceToolStripMenuItem.Enabled = false;
            this.luminanceToolStripMenuItem.Name = "luminanceToolStripMenuItem";
            this.luminanceToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.luminanceToolStripMenuItem.Text = "Luminance";
            // 
            // chromaToolStripMenuItem
            // 
            this.chromaToolStripMenuItem.Enabled = false;
            this.chromaToolStripMenuItem.Name = "chromaToolStripMenuItem";
            this.chromaToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.chromaToolStripMenuItem.Text = "Chroma";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1423, 870);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 3, 4);
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AUO   Optical Measurement System";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStripSaveResultItem.ResumeLayout(false);
            this.PnlGrabViewer.ResumeLayout(false);
            this.PnlGrabViewer.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl_Logger.ResumeLayout(false);
            this.tabPage_GeneralLogger.ResumeLayout(false);
            this.tabPage_DebugLogger.ResumeLayout(false);
            this.tabPage_ErrorLogger.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewROI)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStripRoiEditor.ResumeLayout(false);
            this.contextMenuStripPanelInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        //private System.Windows.Forms.ToolStripMenuItem setPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_System;
        private System.Windows.Forms.ToolStripMenuItem paremeterSettingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Recipe;
        private System.Windows.Forms.ToolStripComboBox toolStripMenuRecipeChoose;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_NewRecipeName;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_29M;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuManualForm;
        private MaterialSkin.Controls.MaterialButton btnStart;
        private MaterialSkin.Controls.MaterialButton btnStop;
        private System.Windows.Forms.ToolStripMenuItem addRecipeToolStripMenuItem;
        private System.Windows.Forms.Label lbPanelShowMsg;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox1;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox2;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox3;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox4;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox5;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox6;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox7;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox8;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox9;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox10;
        public System.Windows.Forms.Panel PnlGrabViewer;
        private MaterialSkin.Controls.MaterialButton btnSaveResultImage;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSaveResultItem;
        private System.Windows.Forms.ToolStripMenuItem tristimulusXToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tristimulusYToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tristimulusZToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem chromaXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chromaYToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MaterialSkin.Controls.MaterialButton btnLoadFile;
        private MaterialSkin.Controls.MaterialButton btnSaveAs;
        private MaterialSkin.Controls.MaterialButton btnDrawRoi;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl_Logger;
        private System.Windows.Forms.TabPage tabPage_GeneralLogger;
        private System.Windows.Forms.RichTextBox richTextBox_LoggerGeneral;
        private System.Windows.Forms.TabPage tabPage_DebugLogger;
        private System.Windows.Forms.RichTextBox richTextBox_LoggerDebug;
        private System.Windows.Forms.TabPage tabPage_ErrorLogger;
        private System.Windows.Forms.RichTextBox richTextBox_LoggerError;
        private System.Windows.Forms.DataGridView dataGridViewROI;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRoiEditor;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteROIToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPanelInfo;
        private System.Windows.Forms.ToolStripMenuItem positionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem luminanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chromaToolStripMenuItem;
        private MaterialSkin.Controls.MaterialLabel lblCameraTemperature;
        private MaterialSkin.Controls.MaterialButton btn_dataGridViewROI_Load;
        private MaterialSkin.Controls.MaterialButton btn_dataGridViewROI_Clear;
        private MaterialSkin.Controls.MaterialButton btn_dataGridViewROI_SaveCSV;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnX;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnY;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRadius;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLum;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCx;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCy;
        private MaterialSkin.Controls.MaterialLabel lblCalibrationFile;
        private MaterialSkin.Controls.MaterialComboBox tbxCalibrationFile;
        private System.Windows.Forms.ToolStripMenuItem AboutAUOToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label Label_FocusPos;
        private System.Windows.Forms.Label Label_Aperture;
        private System.Windows.Forms.Label Label_FW1;
        private System.Windows.Forms.Label Label_FW2;
        private System.Windows.Forms.ToolStripMenuItem CorrToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private MaterialSkin.Controls.MaterialSwitch Switch_SpectralCorrection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
    }
}

