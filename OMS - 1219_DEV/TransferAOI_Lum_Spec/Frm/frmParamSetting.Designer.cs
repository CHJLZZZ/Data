
namespace OpticalMeasuringSystem
{
    partial class frmParamSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmParamSetting));
            this.CCDScrollBarV = new System.Windows.Forms.VScrollBar();
            this.CCDScrollBarH = new System.Windows.Forms.HScrollBar();
            this.PnlGrabViewer = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.button14 = new System.Windows.Forms.Button();
            this.comboBox5 = new System.Windows.Forms.ComboBox();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.button17 = new System.Windows.Forms.Button();
            this.toolStripMenuItem_LeftTop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_RightBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.propertyGrid_29M = new System.Windows.Forms.PropertyGrid();
            this.BtnSaveConfig = new MaterialSkin.Controls.MaterialButton();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.mbtnLoadImage = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveImage = new MaterialSkin.Controls.MaterialButton();
            this.mbtnCapture = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.mtbxExposureTime = new MaterialSkin.Controls.MaterialTextBox();
            this.mbtnSetExposureTime = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel127 = new MaterialSkin.Controls.MaterialLabel();
            this.mtbxGain = new MaterialSkin.Controls.MaterialMaskedTextBox();
            this.mbtnSetGain = new MaterialSkin.Controls.MaterialButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRoiSave_ThirdParty = new MaterialSkin.Controls.MaterialButton();
            this.btnRoiShow_ThirdParty = new MaterialSkin.Controls.MaterialButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRoiSave_AutoExpTime = new MaterialSkin.Controls.MaterialButton();
            this.btnRoiShow_AutoExpTime = new MaterialSkin.Controls.MaterialButton();
            this.label1 = new System.Windows.Forms.Label();
            this.PnlGrabViewer.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // CCDScrollBarV
            // 
            this.CCDScrollBarV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CCDScrollBarV.Location = new System.Drawing.Point(1818, 80);
            this.CCDScrollBarV.Name = "CCDScrollBarV";
            this.tableLayoutPanel1.SetRowSpan(this.CCDScrollBarV, 3);
            this.CCDScrollBarV.Size = new System.Drawing.Size(20, 829);
            this.CCDScrollBarV.TabIndex = 26;
            this.CCDScrollBarV.Visible = false;
            this.CCDScrollBarV.ValueChanged += new System.EventHandler(this.CCDScrollBarV_ValueChanged);
            // 
            // CCDScrollBarH
            // 
            this.CCDScrollBarH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CCDScrollBarH.Location = new System.Drawing.Point(1754, 927);
            this.CCDScrollBarH.Name = "CCDScrollBarH";
            this.CCDScrollBarH.Size = new System.Drawing.Size(20, 21);
            this.CCDScrollBarH.TabIndex = 27;
            this.CCDScrollBarH.ValueChanged += new System.EventHandler(this.CCDScrollBarH_ValueChanged);
            // 
            // PnlGrabViewer
            // 
            this.PnlGrabViewer.BackColor = System.Drawing.Color.Black;
            this.PnlGrabViewer.ContextMenuStrip = this.contextMenuStrip1;
            this.PnlGrabViewer.Controls.Add(this.groupBox3);
            this.PnlGrabViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlGrabViewer.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.PnlGrabViewer.Location = new System.Drawing.Point(486, 85);
            this.PnlGrabViewer.Margin = new System.Windows.Forms.Padding(5);
            this.PnlGrabViewer.Name = "PnlGrabViewer";
            this.tableLayoutPanel1.SetRowSpan(this.PnlGrabViewer, 3);
            this.PnlGrabViewer.Size = new System.Drawing.Size(1327, 819);
            this.PnlGrabViewer.TabIndex = 32;
            this.PnlGrabViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PnlGrabViewer_MouseDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBox10);
            this.groupBox3.Controls.Add(this.textBox11);
            this.groupBox3.Controls.Add(this.button14);
            this.groupBox3.Controls.Add(this.comboBox5);
            this.groupBox3.Controls.Add(this.button15);
            this.groupBox3.Controls.Add(this.button16);
            this.groupBox3.Controls.Add(this.comboBox6);
            this.groupBox3.Controls.Add(this.button17);
            this.groupBox3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox3.Location = new System.Drawing.Point(1832, 213);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox3.Size = new System.Drawing.Size(917, 167);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Hot Pixel";
            // 
            // textBox10
            // 
            this.textBox10.Location = new System.Drawing.Point(542, 45);
            this.textBox10.Margin = new System.Windows.Forms.Padding(5);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(194, 29);
            this.textBox10.TabIndex = 42;
            // 
            // textBox11
            // 
            this.textBox11.Location = new System.Drawing.Point(10, 47);
            this.textBox11.Margin = new System.Windows.Forms.Padding(5);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(194, 29);
            this.textBox11.TabIndex = 41;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(657, 100);
            this.button14.Margin = new System.Windows.Forms.Padding(5);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(112, 53);
            this.button14.TabIndex = 40;
            this.button14.Text = "Move";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // comboBox5
            // 
            this.comboBox5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox5.FormattingEnabled = true;
            this.comboBox5.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.comboBox5.Location = new System.Drawing.Point(542, 102);
            this.comboBox5.Margin = new System.Windows.Forms.Padding(5);
            this.comboBox5.Name = "comboBox5";
            this.comboBox5.Size = new System.Drawing.Size(102, 29);
            this.comboBox5.TabIndex = 39;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(748, 45);
            this.button15.Margin = new System.Windows.Forms.Padding(5);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(145, 48);
            this.button15.TabIndex = 37;
            this.button15.Text = "Init B";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(125, 100);
            this.button16.Margin = new System.Windows.Forms.Padding(5);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(112, 48);
            this.button16.TabIndex = 36;
            this.button16.Text = "Move";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // comboBox6
            // 
            this.comboBox6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.comboBox6.Location = new System.Drawing.Point(10, 102);
            this.comboBox6.Margin = new System.Windows.Forms.Padding(5);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(102, 29);
            this.comboBox6.TabIndex = 35;
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(217, 45);
            this.button17.Margin = new System.Windows.Forms.Padding(5);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(145, 48);
            this.button17.TabIndex = 33;
            this.button17.Text = "Init A";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // toolStripMenuItem_LeftTop
            // 
            this.toolStripMenuItem_LeftTop.Name = "toolStripMenuItem_LeftTop";
            this.toolStripMenuItem_LeftTop.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem_RightBottom
            // 
            this.toolStripMenuItem_RightBottom.Name = "toolStripMenuItem_RightBottom";
            this.toolStripMenuItem_RightBottom.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuSave
            // 
            this.toolStripMenuSave.BackColor = System.Drawing.Color.Yellow;
            this.toolStripMenuSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripMenuSave.ForeColor = System.Drawing.Color.Black;
            this.toolStripMenuSave.Name = "toolStripMenuSave";
            this.toolStripMenuSave.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuSave.Text = "存圖 ";
            this.toolStripMenuSave.Click += new System.EventHandler(this.toolStripMenuSave_Click);
            // 
            // propertyGrid_29M
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.propertyGrid_29M, 4);
            this.propertyGrid_29M.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid_29M.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGrid_29M.Location = new System.Drawing.Point(5, 165);
            this.propertyGrid_29M.Margin = new System.Windows.Forms.Padding(5);
            this.propertyGrid_29M.Name = "propertyGrid_29M";
            this.propertyGrid_29M.Size = new System.Drawing.Size(470, 679);
            this.propertyGrid_29M.TabIndex = 0;
            this.propertyGrid_29M.ToolbarVisible = false;
            // 
            // BtnSaveConfig
            // 
            this.BtnSaveConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.BtnSaveConfig, 4);
            this.BtnSaveConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSaveConfig.Depth = 0;
            this.BtnSaveConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnSaveConfig.Font = new System.Drawing.Font("微軟正黑體", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BtnSaveConfig.HighEmphasis = true;
            this.BtnSaveConfig.Icon = null;
            this.BtnSaveConfig.Location = new System.Drawing.Point(2, 851);
            this.BtnSaveConfig.Margin = new System.Windows.Forms.Padding(2);
            this.BtnSaveConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSaveConfig.Name = "BtnSaveConfig";
            this.BtnSaveConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSaveConfig.Size = new System.Drawing.Size(476, 56);
            this.BtnSaveConfig.TabIndex = 37;
            this.BtnSaveConfig.Text = "儲存參數";
            this.BtnSaveConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSaveConfig.UseAccentColor = false;
            this.BtnSaveConfig.UseVisualStyleBackColor = true;
            this.BtnSaveConfig.Click += new System.EventHandler(this.BtnSaveConfig_Click);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 4;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel6, 4);
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.Controls.Add(this.mbtnLoadImage, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.btnSaveImage, 3, 0);
            this.tableLayoutPanel6.Controls.Add(this.mbtnCapture, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.materialLabel1, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.mtbxExposureTime, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.mbtnSetExposureTime, 3, 1);
            this.tableLayoutPanel6.Controls.Add(this.materialLabel127, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.mtbxGain, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.mbtnSetGain, 3, 2);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel1.SetRowSpan(this.tableLayoutPanel6, 2);
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(474, 154);
            this.tableLayoutPanel6.TabIndex = 67;
            // 
            // mbtnLoadImage
            // 
            this.mbtnLoadImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mbtnLoadImage.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.mbtnLoadImage.Depth = 0;
            this.mbtnLoadImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mbtnLoadImage.HighEmphasis = true;
            this.mbtnLoadImage.Icon = ((System.Drawing.Image)(resources.GetObject("mbtnLoadImage.Icon")));
            this.mbtnLoadImage.Location = new System.Drawing.Point(120, 2);
            this.mbtnLoadImage.Margin = new System.Windows.Forms.Padding(2);
            this.mbtnLoadImage.MouseState = MaterialSkin.MouseState.HOVER;
            this.mbtnLoadImage.Name = "mbtnLoadImage";
            this.mbtnLoadImage.NoAccentTextColor = System.Drawing.Color.Empty;
            this.mbtnLoadImage.Size = new System.Drawing.Size(114, 47);
            this.mbtnLoadImage.TabIndex = 66;
            this.mbtnLoadImage.Text = "Load";
            this.mbtnLoadImage.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.mbtnLoadImage.UseAccentColor = false;
            this.mbtnLoadImage.UseVisualStyleBackColor = true;
            this.mbtnLoadImage.Click += new System.EventHandler(this.btnLoadImg_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveImage.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveImage.Depth = 0;
            this.btnSaveImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveImage.HighEmphasis = true;
            this.btnSaveImage.Icon = ((System.Drawing.Image)(resources.GetObject("btnSaveImage.Icon")));
            this.btnSaveImage.Location = new System.Drawing.Point(356, 2);
            this.btnSaveImage.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveImage.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveImage.Size = new System.Drawing.Size(116, 47);
            this.btnSaveImage.TabIndex = 67;
            this.btnSaveImage.Text = "Save";
            this.btnSaveImage.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveImage.UseAccentColor = false;
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // mbtnCapture
            // 
            this.mbtnCapture.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mbtnCapture.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.mbtnCapture.Depth = 0;
            this.mbtnCapture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mbtnCapture.HighEmphasis = true;
            this.mbtnCapture.Icon = ((System.Drawing.Image)(resources.GetObject("mbtnCapture.Icon")));
            this.mbtnCapture.Location = new System.Drawing.Point(2, 2);
            this.mbtnCapture.Margin = new System.Windows.Forms.Padding(2);
            this.mbtnCapture.MouseState = MaterialSkin.MouseState.HOVER;
            this.mbtnCapture.Name = "mbtnCapture";
            this.mbtnCapture.NoAccentTextColor = System.Drawing.Color.Empty;
            this.mbtnCapture.Size = new System.Drawing.Size(114, 47);
            this.mbtnCapture.TabIndex = 58;
            this.mbtnCapture.Text = "Grab";
            this.mbtnCapture.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.mbtnCapture.UseAccentColor = false;
            this.mbtnCapture.UseVisualStyleBackColor = true;
            this.mbtnCapture.Click += new System.EventHandler(this.mbtnCapture_Click);
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.BackColor = System.Drawing.Color.Transparent;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel1.Location = new System.Drawing.Point(2, 53);
            this.materialLabel1.Margin = new System.Windows.Forms.Padding(2);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(114, 47);
            this.materialLabel1.TabIndex = 69;
            this.materialLabel1.Text = "Exposure Time";
            this.materialLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtbxExposureTime
            // 
            this.mtbxExposureTime.AnimateReadOnly = false;
            this.mtbxExposureTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel6.SetColumnSpan(this.mtbxExposureTime, 2);
            this.mtbxExposureTime.Depth = 0;
            this.mtbxExposureTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtbxExposureTime.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.mtbxExposureTime.Hint = "micro seconds";
            this.mtbxExposureTime.LeadingIcon = null;
            this.mtbxExposureTime.Location = new System.Drawing.Point(120, 53);
            this.mtbxExposureTime.Margin = new System.Windows.Forms.Padding(2);
            this.mtbxExposureTime.MaxLength = 50;
            this.mtbxExposureTime.MouseState = MaterialSkin.MouseState.OUT;
            this.mtbxExposureTime.Multiline = false;
            this.mtbxExposureTime.Name = "mtbxExposureTime";
            this.mtbxExposureTime.Size = new System.Drawing.Size(232, 50);
            this.mtbxExposureTime.TabIndex = 71;
            this.mtbxExposureTime.Text = "";
            this.mtbxExposureTime.TrailingIcon = null;
            // 
            // mbtnSetExposureTime
            // 
            this.mbtnSetExposureTime.AutoSize = false;
            this.mbtnSetExposureTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mbtnSetExposureTime.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.mbtnSetExposureTime.Depth = 0;
            this.mbtnSetExposureTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mbtnSetExposureTime.HighEmphasis = true;
            this.mbtnSetExposureTime.Icon = null;
            this.mbtnSetExposureTime.Location = new System.Drawing.Point(356, 53);
            this.mbtnSetExposureTime.Margin = new System.Windows.Forms.Padding(2);
            this.mbtnSetExposureTime.MouseState = MaterialSkin.MouseState.HOVER;
            this.mbtnSetExposureTime.Name = "mbtnSetExposureTime";
            this.mbtnSetExposureTime.NoAccentTextColor = System.Drawing.Color.Empty;
            this.mbtnSetExposureTime.Size = new System.Drawing.Size(116, 47);
            this.mbtnSetExposureTime.TabIndex = 72;
            this.mbtnSetExposureTime.Text = "Set";
            this.mbtnSetExposureTime.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.mbtnSetExposureTime.UseAccentColor = false;
            this.mbtnSetExposureTime.UseVisualStyleBackColor = true;
            this.mbtnSetExposureTime.Click += new System.EventHandler(this.mbtnSetExposureTime_Click);
            // 
            // materialLabel127
            // 
            this.materialLabel127.AutoSize = true;
            this.materialLabel127.Depth = 0;
            this.materialLabel127.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialLabel127.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel127.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.materialLabel127.Location = new System.Drawing.Point(2, 104);
            this.materialLabel127.Margin = new System.Windows.Forms.Padding(2);
            this.materialLabel127.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel127.Name = "materialLabel127";
            this.materialLabel127.Size = new System.Drawing.Size(114, 48);
            this.materialLabel127.TabIndex = 75;
            this.materialLabel127.Text = "Gain";
            this.materialLabel127.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtbxGain
            // 
            this.mtbxGain.AllowPromptAsInput = true;
            this.mtbxGain.AnimateReadOnly = false;
            this.mtbxGain.AsciiOnly = false;
            this.mtbxGain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.mtbxGain.BeepOnError = false;
            this.tableLayoutPanel6.SetColumnSpan(this.mtbxGain, 2);
            this.mtbxGain.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            this.mtbxGain.Depth = 0;
            this.mtbxGain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtbxGain.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.mtbxGain.HidePromptOnLeave = false;
            this.mtbxGain.HideSelection = true;
            this.mtbxGain.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Default;
            this.mtbxGain.LeadingIcon = null;
            this.mtbxGain.Location = new System.Drawing.Point(120, 104);
            this.mtbxGain.Margin = new System.Windows.Forms.Padding(2);
            this.mtbxGain.Mask = "";
            this.mtbxGain.MaxLength = 32767;
            this.mtbxGain.MouseState = MaterialSkin.MouseState.OUT;
            this.mtbxGain.Name = "mtbxGain";
            this.mtbxGain.PasswordChar = '\0';
            this.mtbxGain.PrefixSuffixText = null;
            this.mtbxGain.PromptChar = '_';
            this.mtbxGain.ReadOnly = false;
            this.mtbxGain.RejectInputOnFirstFailure = false;
            this.mtbxGain.ResetOnPrompt = true;
            this.mtbxGain.ResetOnSpace = true;
            this.mtbxGain.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mtbxGain.SelectedText = "";
            this.mtbxGain.SelectionLength = 0;
            this.mtbxGain.SelectionStart = 0;
            this.mtbxGain.ShortcutsEnabled = true;
            this.mtbxGain.Size = new System.Drawing.Size(232, 48);
            this.mtbxGain.SkipLiterals = true;
            this.mtbxGain.TabIndex = 76;
            this.mtbxGain.TabStop = false;
            this.mtbxGain.Text = "0";
            this.mtbxGain.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.mtbxGain.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            this.mtbxGain.TrailingIcon = null;
            this.mtbxGain.UseSystemPasswordChar = false;
            this.mtbxGain.ValidatingType = null;
            // 
            // mbtnSetGain
            // 
            this.mbtnSetGain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mbtnSetGain.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.mbtnSetGain.Depth = 0;
            this.mbtnSetGain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mbtnSetGain.HighEmphasis = true;
            this.mbtnSetGain.Icon = null;
            this.mbtnSetGain.Location = new System.Drawing.Point(356, 104);
            this.mbtnSetGain.Margin = new System.Windows.Forms.Padding(2);
            this.mbtnSetGain.MouseState = MaterialSkin.MouseState.HOVER;
            this.mbtnSetGain.Name = "mbtnSetGain";
            this.mbtnSetGain.NoAccentTextColor = System.Drawing.Color.Empty;
            this.mbtnSetGain.Size = new System.Drawing.Size(116, 48);
            this.mbtnSetGain.TabIndex = 77;
            this.mbtnSetGain.Text = "SET";
            this.mbtnSetGain.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.mbtnSetGain.UseAccentColor = false;
            this.mbtnSetGain.UseVisualStyleBackColor = true;
            this.mbtnSetGain.Click += new System.EventHandler(this.mbtnSetGain_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.BtnSaveConfig, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.CCDScrollBarV, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.PnlGrabViewer, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.propertyGrid_29M, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 64);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1838, 909);
            this.tableLayoutPanel1.TabIndex = 37;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(481, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1337, 80);
            this.tableLayoutPanel2.TabIndex = 39;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.Silver;
            this.tableLayoutPanel4.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.OutsetPartial;
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel4.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnRoiSave_ThirdParty, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnRoiShow_ThirdParty, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tableLayoutPanel4.Location = new System.Drawing.Point(423, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(394, 74);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(6, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 68);
            this.label2.TabIndex = 40;
            this.label2.Text = "ROI Setting : ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRoiSave_ThirdParty
            // 
            this.btnRoiSave_ThirdParty.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRoiSave_ThirdParty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRoiSave_ThirdParty.Depth = 0;
            this.btnRoiSave_ThirdParty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRoiSave_ThirdParty.HighEmphasis = true;
            this.btnRoiSave_ThirdParty.Icon = null;
            this.btnRoiSave_ThirdParty.Location = new System.Drawing.Point(281, 5);
            this.btnRoiSave_ThirdParty.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoiSave_ThirdParty.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRoiSave_ThirdParty.Name = "btnRoiSave_ThirdParty";
            this.btnRoiSave_ThirdParty.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRoiSave_ThirdParty.Size = new System.Drawing.Size(108, 64);
            this.btnRoiSave_ThirdParty.TabIndex = 39;
            this.btnRoiSave_ThirdParty.Text = "儲存\r\nROI";
            this.btnRoiSave_ThirdParty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRoiSave_ThirdParty.UseAccentColor = false;
            this.btnRoiSave_ThirdParty.UseVisualStyleBackColor = true;
            this.btnRoiSave_ThirdParty.Click += new System.EventHandler(this.btnRoiSave_ThirdParty_Click);
            // 
            // btnRoiShow_ThirdParty
            // 
            this.btnRoiShow_ThirdParty.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRoiShow_ThirdParty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRoiShow_ThirdParty.Depth = 0;
            this.btnRoiShow_ThirdParty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRoiShow_ThirdParty.HighEmphasis = true;
            this.btnRoiShow_ThirdParty.Icon = null;
            this.btnRoiShow_ThirdParty.Location = new System.Drawing.Point(178, 5);
            this.btnRoiShow_ThirdParty.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoiShow_ThirdParty.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRoiShow_ThirdParty.Name = "btnRoiShow_ThirdParty";
            this.btnRoiShow_ThirdParty.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRoiShow_ThirdParty.Size = new System.Drawing.Size(96, 64);
            this.btnRoiShow_ThirdParty.TabIndex = 39;
            this.btnRoiShow_ThirdParty.Text = "顯示\r\nROI";
            this.btnRoiShow_ThirdParty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRoiShow_ThirdParty.UseAccentColor = false;
            this.btnRoiShow_ThirdParty.UseVisualStyleBackColor = true;
            this.btnRoiShow_ThirdParty.Click += new System.EventHandler(this.btnRoiShow_ThirdParty_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.BackColor = System.Drawing.Color.Silver;
            this.tableLayoutPanel3.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.OutsetPartial;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel3.Controls.Add(this.btnRoiSave_AutoExpTime, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnRoiShow_AutoExpTime, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(394, 74);
            this.tableLayoutPanel3.TabIndex = 0;
            this.tableLayoutPanel3.Visible = false;
            // 
            // btnRoiSave_AutoExpTime
            // 
            this.btnRoiSave_AutoExpTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRoiSave_AutoExpTime.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRoiSave_AutoExpTime.Depth = 0;
            this.btnRoiSave_AutoExpTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRoiSave_AutoExpTime.HighEmphasis = true;
            this.btnRoiSave_AutoExpTime.Icon = null;
            this.btnRoiSave_AutoExpTime.Location = new System.Drawing.Point(281, 5);
            this.btnRoiSave_AutoExpTime.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoiSave_AutoExpTime.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRoiSave_AutoExpTime.Name = "btnRoiSave_AutoExpTime";
            this.btnRoiSave_AutoExpTime.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRoiSave_AutoExpTime.Size = new System.Drawing.Size(108, 64);
            this.btnRoiSave_AutoExpTime.TabIndex = 38;
            this.btnRoiSave_AutoExpTime.Text = "儲存\r\nROI";
            this.btnRoiSave_AutoExpTime.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRoiSave_AutoExpTime.UseAccentColor = false;
            this.btnRoiSave_AutoExpTime.UseVisualStyleBackColor = true;
            this.btnRoiSave_AutoExpTime.Click += new System.EventHandler(this.btn_RoiSave_AutoExpTime_Click);
            // 
            // btnRoiShow_AutoExpTime
            // 
            this.btnRoiShow_AutoExpTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRoiShow_AutoExpTime.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRoiShow_AutoExpTime.Depth = 0;
            this.btnRoiShow_AutoExpTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRoiShow_AutoExpTime.HighEmphasis = true;
            this.btnRoiShow_AutoExpTime.Icon = null;
            this.btnRoiShow_AutoExpTime.Location = new System.Drawing.Point(178, 5);
            this.btnRoiShow_AutoExpTime.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoiShow_AutoExpTime.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRoiShow_AutoExpTime.Name = "btnRoiShow_AutoExpTime";
            this.btnRoiShow_AutoExpTime.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRoiShow_AutoExpTime.Size = new System.Drawing.Size(96, 64);
            this.btnRoiShow_AutoExpTime.TabIndex = 38;
            this.btnRoiShow_AutoExpTime.Text = "顯示\r\nROI";
            this.btnRoiShow_AutoExpTime.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRoiShow_AutoExpTime.UseAccentColor = false;
            this.btnRoiShow_AutoExpTime.UseVisualStyleBackColor = true;
            this.btnRoiShow_AutoExpTime.Click += new System.EventHandler(this.btnRoiShow_AutoExpTime_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 68);
            this.label1.TabIndex = 39;
            this.label1.Text = "自動曝光 : ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmParamSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1844, 978);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "frmParamSetting";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 3, 5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parameter Setting";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmParamSetting_FormClosed);
            this.Load += new System.EventHandler(this.frmParamSetting_Load);
            this.PnlGrabViewer.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.VScrollBar CCDScrollBarV;
        private System.Windows.Forms.HScrollBar CCDScrollBarH;
        private System.Windows.Forms.Panel PnlGrabViewer;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_LeftTop;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_RightBottom;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuSave;
        private System.Windows.Forms.PropertyGrid propertyGrid_29M;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MaterialSkin.Controls.MaterialButton btnRoiSave_AutoExpTime;
        private MaterialSkin.Controls.MaterialButton btnRoiShow_AutoExpTime;
        private MaterialSkin.Controls.MaterialButton BtnSaveConfig;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private MaterialSkin.Controls.MaterialButton btnRoiSave_ThirdParty;
        private MaterialSkin.Controls.MaterialButton btnRoiShow_ThirdParty;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private MaterialSkin.Controls.MaterialButton mbtnLoadImage;
        private MaterialSkin.Controls.MaterialButton btnSaveImage;
        private MaterialSkin.Controls.MaterialButton mbtnCapture;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialTextBox mtbxExposureTime;
        private MaterialSkin.Controls.MaterialButton mbtnSetExposureTime;
        private MaterialSkin.Controls.MaterialLabel materialLabel127;
        private MaterialSkin.Controls.MaterialMaskedTextBox mtbxGain;
        private MaterialSkin.Controls.MaterialButton mbtnSetGain;
    }
}