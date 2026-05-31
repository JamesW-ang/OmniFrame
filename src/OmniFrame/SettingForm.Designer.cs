namespace OmniFrame
{
    partial class SettingForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlNav = new System.Windows.Forms.Panel();
            this.btnUserManage = new System.Windows.Forms.Button();
            this.btnNetwork = new System.Windows.Forms.Button();
            this.pnlDeviceNav = new System.Windows.Forms.Panel();
            this.btnIoConfig = new System.Windows.Forms.Button();
            this.btnPlcConfig = new System.Windows.Forms.Button();
            this.btnMotionCard = new System.Windows.Forms.Button();
            this.btnDevice = new System.Windows.Forms.Button();
            this.btnSystem = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabSystem = new System.Windows.Forms.TabPage();
            this.grpSafetyParams = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.numAlarmDelay = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.cbSoftwareLimit = new System.Windows.Forms.CheckBox();
            this.grpRunParams = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.numHomeSpeed = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numDefaultAccel = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numDefaultSpeed = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.grpBasicInfo = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtDeviceSN = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDeviceModel = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDeviceName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabMotion = new System.Windows.Forms.TabPage();
            this.pnlMotionDetail = new System.Windows.Forms.Panel();
            this.dgvAxisParams = new System.Windows.Forms.DataGridView();
            this.label13 = new System.Windows.Forms.Label();
            this.numAxisCount = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.txtMotionIP = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.pnlMotionList = new System.Windows.Forms.Panel();
            this.btnRemoveMotion = new System.Windows.Forms.Button();
            this.btnAddMotion = new System.Windows.Forms.Button();
            this.dgvMotionCards = new System.Windows.Forms.DataGridView();
            this.tabPlc = new System.Windows.Forms.TabPage();
            this.pnlPlcDetail = new System.Windows.Forms.Panel();
            this.dgvRegisterMap = new System.Windows.Forms.DataGridView();
            this.label20 = new System.Windows.Forms.Label();
            this.numPlcPort = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.txtPlcIP = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.cbPlcBrand = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.pnlPlcList = new System.Windows.Forms.Panel();
            this.btnRemovePlc = new System.Windows.Forms.Button();
            this.btnAddPlc = new System.Windows.Forms.Button();
            this.dgvPlcList = new System.Windows.Forms.DataGridView();
            this.tabIo = new System.Windows.Forms.TabPage();
            this.pnlOutputList = new System.Windows.Forms.Panel();
            this.btnRemoveOutput = new System.Windows.Forms.Button();
            this.btnAddOutput = new System.Windows.Forms.Button();
            this.dgvOutputPoints = new System.Windows.Forms.DataGridView();
            this.pnlInputList = new System.Windows.Forms.Panel();
            this.btnRemoveInput = new System.Windows.Forms.Button();
            this.btnAddInput = new System.Windows.Forms.Button();
            this.dgvInputPoints = new System.Windows.Forms.DataGridView();
            this.tabNetwork = new System.Windows.Forms.TabPage();
            this.grpRemoteConfig = new System.Windows.Forms.GroupBox();
            this.cbRemoteEnable = new System.Windows.Forms.CheckBox();
            this.numRemotePort = new System.Windows.Forms.NumericUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.grpMqttConfig = new System.Windows.Forms.GroupBox();
            this.txtMqttTopic = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.numMqttPort = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.txtMqttBroker = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.grpMesConfig = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cbMesProtocol = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.numMesPort = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMesIP = new System.Windows.Forms.TextBox();
            this.labelMesIP = new System.Windows.Forms.Label();
            this.tabUser = new System.Windows.Forms.TabPage();
            this.pnlUserEdit = new System.Windows.Forms.Panel();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.btnDeleteUser = new System.Windows.Forms.Button();
            this.btnAddUser = new System.Windows.Forms.Button();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.cbUserRole = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.txtNewUsername = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.dgvUserList = new System.Windows.Forms.DataGridView();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnExportConfig = new System.Windows.Forms.Button();
            this.btnImportConfig = new System.Windows.Forms.Button();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlNav.SuspendLayout();
            this.pnlDeviceNav.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabSystem.SuspendLayout();
            this.grpSafetyParams.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAlarmDelay)).BeginInit();
            this.grpRunParams.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDefaultAccel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDefaultSpeed)).BeginInit();
            this.grpBasicInfo.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabMotion.SuspendLayout();
            this.pnlMotionDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAxisParams)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAxisCount)).BeginInit();
            this.pnlMotionList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMotionCards)).BeginInit();
            this.tabPlc.SuspendLayout();
            this.pnlPlcDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRegisterMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPlcPort)).BeginInit();
            this.pnlPlcList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlcList)).BeginInit();
            this.tabIo.SuspendLayout();
            this.pnlOutputList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputPoints)).BeginInit();
            this.pnlInputList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputPoints)).BeginInit();
            this.tabNetwork.SuspendLayout();
            this.grpRemoteConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRemotePort)).BeginInit();
            this.grpMqttConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMqttPort)).BeginInit();
            this.grpMesConfig.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMesPort)).BeginInit();
            this.tabUser.SuspendLayout();
            this.pnlUserEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).BeginInit();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pnlNav);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1084, 590);
            this.splitContainer1.SplitterDistance = 219;
            this.splitContainer1.TabIndex = 0;
            // 
            // pnlNav
            // 
            this.pnlNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlNav.Controls.Add(this.btnUserManage);
            this.pnlNav.Controls.Add(this.btnNetwork);
            this.pnlNav.Controls.Add(this.pnlDeviceNav);
            this.pnlNav.Controls.Add(this.btnDevice);
            this.pnlNav.Controls.Add(this.btnSystem);
            this.pnlNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNav.Location = new System.Drawing.Point(0, 0);
            this.pnlNav.Name = "pnlNav";
            this.pnlNav.Size = new System.Drawing.Size(219, 590);
            this.pnlNav.TabIndex = 0;
            // 
            // btnUserManage
            // 
            this.btnUserManage.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnUserManage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserManage.Location = new System.Drawing.Point(0, 240);
            this.btnUserManage.Name = "btnUserManage";
            this.btnUserManage.Size = new System.Drawing.Size(219, 40);
            this.btnUserManage.TabIndex = 4;
            this.btnUserManage.Text = "用户管理";
            this.btnUserManage.UseVisualStyleBackColor = true;
            this.btnUserManage.Click += new System.EventHandler(this.btnUserManage_Click);
            // 
            // btnNetwork
            // 
            this.btnNetwork.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNetwork.Location = new System.Drawing.Point(0, 200);
            this.btnNetwork.Name = "btnNetwork";
            this.btnNetwork.Size = new System.Drawing.Size(219, 40);
            this.btnNetwork.TabIndex = 3;
            this.btnNetwork.Text = "网络配置";
            this.btnNetwork.UseVisualStyleBackColor = true;
            this.btnNetwork.Click += new System.EventHandler(this.btnNetwork_Click);
            // 
            // pnlDeviceNav
            // 
            this.pnlDeviceNav.Controls.Add(this.btnIoConfig);
            this.pnlDeviceNav.Controls.Add(this.btnPlcConfig);
            this.pnlDeviceNav.Controls.Add(this.btnMotionCard);
            this.pnlDeviceNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDeviceNav.Location = new System.Drawing.Point(0, 80);
            this.pnlDeviceNav.Name = "pnlDeviceNav";
            this.pnlDeviceNav.Size = new System.Drawing.Size(219, 120);
            this.pnlDeviceNav.TabIndex = 2;
            // 
            // btnIoConfig
            // 
            this.btnIoConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnIoConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIoConfig.Location = new System.Drawing.Point(0, 80);
            this.btnIoConfig.Name = "btnIoConfig";
            this.btnIoConfig.Size = new System.Drawing.Size(219, 40);
            this.btnIoConfig.TabIndex = 2;
            this.btnIoConfig.Text = "  IO模块";
            this.btnIoConfig.UseVisualStyleBackColor = true;
            this.btnIoConfig.Click += new System.EventHandler(this.btnIoConfig_Click);
            // 
            // btnPlcConfig
            // 
            this.btnPlcConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnPlcConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlcConfig.Location = new System.Drawing.Point(0, 40);
            this.btnPlcConfig.Name = "btnPlcConfig";
            this.btnPlcConfig.Size = new System.Drawing.Size(219, 40);
            this.btnPlcConfig.TabIndex = 1;
            this.btnPlcConfig.Text = "  PLC";
            this.btnPlcConfig.UseVisualStyleBackColor = true;
            this.btnPlcConfig.Click += new System.EventHandler(this.btnPlcConfig_Click);
            // 
            // btnMotionCard
            // 
            this.btnMotionCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnMotionCard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMotionCard.Location = new System.Drawing.Point(0, 0);
            this.btnMotionCard.Name = "btnMotionCard";
            this.btnMotionCard.Size = new System.Drawing.Size(219, 40);
            this.btnMotionCard.TabIndex = 0;
            this.btnMotionCard.Text = "  运动卡";
            this.btnMotionCard.UseVisualStyleBackColor = true;
            this.btnMotionCard.Click += new System.EventHandler(this.btnMotionCard_Click);
            // 
            // btnDevice
            // 
            this.btnDevice.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDevice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDevice.Location = new System.Drawing.Point(0, 40);
            this.btnDevice.Name = "btnDevice";
            this.btnDevice.Size = new System.Drawing.Size(219, 40);
            this.btnDevice.TabIndex = 1;
            this.btnDevice.Text = "设备配置";
            this.btnDevice.UseVisualStyleBackColor = true;
            this.btnDevice.Click += new System.EventHandler(this.btnDevice_Click);
            // 
            // btnSystem
            // 
            this.btnSystem.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSystem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSystem.Location = new System.Drawing.Point(0, 0);
            this.btnSystem.Name = "btnSystem";
            this.btnSystem.Size = new System.Drawing.Size(219, 40);
            this.btnSystem.TabIndex = 0;
            this.btnSystem.Text = "系统配置";
            this.btnSystem.UseVisualStyleBackColor = true;
            this.btnSystem.Click += new System.EventHandler(this.btnSystem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabSystem);
            this.tabControl1.Controls.Add(this.tabMotion);
            this.tabControl1.Controls.Add(this.tabPlc);
            this.tabControl1.Controls.Add(this.tabIo);
            this.tabControl1.Controls.Add(this.tabNetwork);
            this.tabControl1.Controls.Add(this.tabUser);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(861, 590);
            this.tabControl1.TabIndex = 0;
            // 
            // tabSystem
            // 
            this.tabSystem.Controls.Add(this.grpSafetyParams);
            this.tabSystem.Controls.Add(this.grpRunParams);
            this.tabSystem.Controls.Add(this.grpBasicInfo);
            this.tabSystem.Location = new System.Drawing.Point(8, 50);
            this.tabSystem.Name = "tabSystem";
            this.tabSystem.Padding = new System.Windows.Forms.Padding(3);
            this.tabSystem.Size = new System.Drawing.Size(845, 532);
            this.tabSystem.TabIndex = 0;
            this.tabSystem.Text = "系统配置";
            this.tabSystem.UseVisualStyleBackColor = true;
            // 
            // grpSafetyParams
            // 
            this.grpSafetyParams.Controls.Add(this.tableLayoutPanel3);
            this.grpSafetyParams.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSafetyParams.Location = new System.Drawing.Point(3, 230);
            this.grpSafetyParams.Name = "grpSafetyParams";
            this.grpSafetyParams.Padding = new System.Windows.Forms.Padding(10);
            this.grpSafetyParams.Size = new System.Drawing.Size(839, 100);
            this.grpSafetyParams.TabIndex = 2;
            this.grpSafetyParams.TabStop = false;
            this.grpSafetyParams.Text = "安全参数";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.numAlarmDelay, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.cbSoftwareLimit, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(10, 47);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(819, 43);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // numAlarmDelay
            // 
            this.numAlarmDelay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numAlarmDelay.Location = new System.Drawing.Point(153, 24);
            this.numAlarmDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numAlarmDelay.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAlarmDelay.Name = "numAlarmDelay";
            this.numAlarmDelay.Size = new System.Drawing.Size(100, 44);
            this.numAlarmDelay.TabIndex = 2;
            this.numAlarmDelay.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(136, 22);
            this.label7.TabIndex = 1;
            this.label7.Text = "报警延时(毫秒)：";
            // 
            // cbSoftwareLimit
            // 
            this.cbSoftwareLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbSoftwareLimit.AutoSize = true;
            this.cbSoftwareLimit.Location = new System.Drawing.Point(3, 3);
            this.cbSoftwareLimit.Name = "cbSoftwareLimit";
            this.cbSoftwareLimit.Size = new System.Drawing.Size(144, 15);
            this.cbSoftwareLimit.TabIndex = 0;
            this.cbSoftwareLimit.Text = "软件限位使能";
            this.cbSoftwareLimit.UseVisualStyleBackColor = true;
            // 
            // grpRunParams
            // 
            this.grpRunParams.Controls.Add(this.tableLayoutPanel2);
            this.grpRunParams.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpRunParams.Location = new System.Drawing.Point(3, 130);
            this.grpRunParams.Name = "grpRunParams";
            this.grpRunParams.Padding = new System.Windows.Forms.Padding(10);
            this.grpRunParams.Size = new System.Drawing.Size(839, 100);
            this.grpRunParams.TabIndex = 1;
            this.grpRunParams.TabStop = false;
            this.grpRunParams.Text = "运行参数";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.numHomeSpeed, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.label6, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.numDefaultAccel, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.numDefaultSpeed, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 47);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(819, 43);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // numHomeSpeed
            // 
            this.numHomeSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numHomeSpeed.Location = new System.Drawing.Point(532, 24);
            this.numHomeSpeed.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numHomeSpeed.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numHomeSpeed.Name = "numHomeSpeed";
            this.numHomeSpeed.Size = new System.Drawing.Size(100, 44);
            this.numHomeSpeed.TabIndex = 5;
            this.numHomeSpeed.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(412, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 22);
            this.label6.TabIndex = 4;
            this.label6.Text = "回原点速度：";
            // 
            // numDefaultAccel
            // 
            this.numDefaultAccel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numDefaultAccel.Location = new System.Drawing.Point(532, 3);
            this.numDefaultAccel.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numDefaultAccel.Name = "numDefaultAccel";
            this.numDefaultAccel.Size = new System.Drawing.Size(100, 44);
            this.numDefaultAccel.TabIndex = 3;
            this.numDefaultAccel.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(412, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 21);
            this.label5.TabIndex = 2;
            this.label5.Text = "默认加速度：";
            // 
            // numDefaultSpeed
            // 
            this.numDefaultSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numDefaultSpeed.Location = new System.Drawing.Point(123, 3);
            this.numDefaultSpeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numDefaultSpeed.Name = "numDefaultSpeed";
            this.numDefaultSpeed.Size = new System.Drawing.Size(100, 44);
            this.numDefaultSpeed.TabIndex = 1;
            this.numDefaultSpeed.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "默认速度：";
            // 
            // grpBasicInfo
            // 
            this.grpBasicInfo.Controls.Add(this.tableLayoutPanel1);
            this.grpBasicInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpBasicInfo.Location = new System.Drawing.Point(3, 3);
            this.grpBasicInfo.Name = "grpBasicInfo";
            this.grpBasicInfo.Padding = new System.Windows.Forms.Padding(10);
            this.grpBasicInfo.Size = new System.Drawing.Size(839, 127);
            this.grpBasicInfo.TabIndex = 0;
            this.grpBasicInfo.TabStop = false;
            this.grpBasicInfo.Text = "基本信息";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtDeviceSN, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtDeviceModel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtDeviceName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 47);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(819, 70);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // txtDeviceSN
            // 
            this.txtDeviceSN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeviceSN.Location = new System.Drawing.Point(123, 49);
            this.txtDeviceSN.Name = "txtDeviceSN";
            this.txtDeviceSN.Size = new System.Drawing.Size(693, 44);
            this.txtDeviceSN.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "序列号：";
            // 
            // txtDeviceModel
            // 
            this.txtDeviceModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeviceModel.Location = new System.Drawing.Point(123, 26);
            this.txtDeviceModel.Name = "txtDeviceModel";
            this.txtDeviceModel.Size = new System.Drawing.Size(693, 44);
            this.txtDeviceModel.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "设备型号：";
            // 
            // txtDeviceName
            // 
            this.txtDeviceName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeviceName.Location = new System.Drawing.Point(123, 3);
            this.txtDeviceName.Name = "txtDeviceName";
            this.txtDeviceName.Size = new System.Drawing.Size(693, 44);
            this.txtDeviceName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "设备名称：";
            // 
            // tabMotion
            // 
            this.tabMotion.Controls.Add(this.pnlMotionDetail);
            this.tabMotion.Controls.Add(this.pnlMotionList);
            this.tabMotion.Location = new System.Drawing.Point(8, 39);
            this.tabMotion.Name = "tabMotion";
            this.tabMotion.Padding = new System.Windows.Forms.Padding(3);
            this.tabMotion.Size = new System.Drawing.Size(845, 543);
            this.tabMotion.TabIndex = 1;
            this.tabMotion.Text = "运动卡配置";
            this.tabMotion.UseVisualStyleBackColor = true;
            // 
            // pnlMotionDetail
            // 
            this.pnlMotionDetail.Controls.Add(this.dgvAxisParams);
            this.pnlMotionDetail.Controls.Add(this.label13);
            this.pnlMotionDetail.Controls.Add(this.numAxisCount);
            this.pnlMotionDetail.Controls.Add(this.label12);
            this.pnlMotionDetail.Controls.Add(this.txtMotionIP);
            this.pnlMotionDetail.Controls.Add(this.label11);
            this.pnlMotionDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMotionDetail.Location = new System.Drawing.Point(3, 200);
            this.pnlMotionDetail.Name = "pnlMotionDetail";
            this.pnlMotionDetail.Size = new System.Drawing.Size(839, 340);
            this.pnlMotionDetail.TabIndex = 1;
            // 
            // dgvAxisParams
            // 
            this.dgvAxisParams.AllowUserToAddRows = false;
            this.dgvAxisParams.AllowUserToDeleteRows = false;
            this.dgvAxisParams.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAxisParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAxisParams.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvAxisParams.Location = new System.Drawing.Point(0, 62);
            this.dgvAxisParams.Name = "dgvAxisParams";
            this.dgvAxisParams.RowHeadersWidth = 51;
            this.dgvAxisParams.RowTemplate.Height = 27;
            this.dgvAxisParams.Size = new System.Drawing.Size(839, 278);
            this.dgvAxisParams.TabIndex = 5;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(350, 25);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(127, 36);
            this.label13.TabIndex = 4;
            this.label13.Text = "轴数量：";
            // 
            // numAxisCount
            // 
            this.numAxisCount.Location = new System.Drawing.Point(450, 22);
            this.numAxisCount.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numAxisCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAxisCount.Name = "numAxisCount";
            this.numAxisCount.Size = new System.Drawing.Size(100, 44);
            this.numAxisCount.TabIndex = 3;
            this.numAxisCount.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(30, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(124, 36);
            this.label12.TabIndex = 2;
            this.label12.Text = "IP地址：";
            // 
            // txtMotionIP
            // 
            this.txtMotionIP.Location = new System.Drawing.Point(120, 22);
            this.txtMotionIP.Name = "txtMotionIP";
            this.txtMotionIP.Size = new System.Drawing.Size(200, 44);
            this.txtMotionIP.TabIndex = 1;
            this.txtMotionIP.Text = "192.168.1.100";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(183, 36);
            this.label11.TabIndex = 0;
            this.label11.Text = "运动卡详情：";
            // 
            // pnlMotionList
            // 
            this.pnlMotionList.Controls.Add(this.btnRemoveMotion);
            this.pnlMotionList.Controls.Add(this.btnAddMotion);
            this.pnlMotionList.Controls.Add(this.dgvMotionCards);
            this.pnlMotionList.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMotionList.Location = new System.Drawing.Point(3, 3);
            this.pnlMotionList.Name = "pnlMotionList";
            this.pnlMotionList.Size = new System.Drawing.Size(839, 197);
            this.pnlMotionList.TabIndex = 0;
            // 
            // btnRemoveMotion
            // 
            this.btnRemoveMotion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveMotion.Location = new System.Drawing.Point(739, 160);
            this.btnRemoveMotion.Name = "btnRemoveMotion";
            this.btnRemoveMotion.Size = new System.Drawing.Size(80, 30);
            this.btnRemoveMotion.TabIndex = 2;
            this.btnRemoveMotion.Text = "移除";
            this.btnRemoveMotion.UseVisualStyleBackColor = true;
            // 
            // btnAddMotion
            // 
            this.btnAddMotion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddMotion.Location = new System.Drawing.Point(649, 160);
            this.btnAddMotion.Name = "btnAddMotion";
            this.btnAddMotion.Size = new System.Drawing.Size(80, 30);
            this.btnAddMotion.TabIndex = 1;
            this.btnAddMotion.Text = "添加";
            this.btnAddMotion.UseVisualStyleBackColor = true;
            // 
            // dgvMotionCards
            // 
            this.dgvMotionCards.AllowUserToAddRows = false;
            this.dgvMotionCards.AllowUserToDeleteRows = false;
            this.dgvMotionCards.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMotionCards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMotionCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvMotionCards.Location = new System.Drawing.Point(0, 0);
            this.dgvMotionCards.Name = "dgvMotionCards";
            this.dgvMotionCards.RowHeadersWidth = 51;
            this.dgvMotionCards.RowTemplate.Height = 27;
            this.dgvMotionCards.Size = new System.Drawing.Size(839, 150);
            this.dgvMotionCards.TabIndex = 0;
            // 
            // tabPlc
            // 
            this.tabPlc.Controls.Add(this.pnlPlcDetail);
            this.tabPlc.Controls.Add(this.pnlPlcList);
            this.tabPlc.Location = new System.Drawing.Point(8, 39);
            this.tabPlc.Name = "tabPlc";
            this.tabPlc.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlc.Size = new System.Drawing.Size(845, 543);
            this.tabPlc.TabIndex = 2;
            this.tabPlc.Text = "PLC配置";
            this.tabPlc.UseVisualStyleBackColor = true;
            // 
            // pnlPlcDetail
            // 
            this.pnlPlcDetail.Controls.Add(this.dgvRegisterMap);
            this.pnlPlcDetail.Controls.Add(this.label20);
            this.pnlPlcDetail.Controls.Add(this.numPlcPort);
            this.pnlPlcDetail.Controls.Add(this.label19);
            this.pnlPlcDetail.Controls.Add(this.txtPlcIP);
            this.pnlPlcDetail.Controls.Add(this.label18);
            this.pnlPlcDetail.Controls.Add(this.cbPlcBrand);
            this.pnlPlcDetail.Controls.Add(this.label17);
            this.pnlPlcDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlcDetail.Location = new System.Drawing.Point(3, 200);
            this.pnlPlcDetail.Name = "pnlPlcDetail";
            this.pnlPlcDetail.Size = new System.Drawing.Size(839, 340);
            this.pnlPlcDetail.TabIndex = 1;
            // 
            // dgvRegisterMap
            // 
            this.dgvRegisterMap.AllowUserToAddRows = false;
            this.dgvRegisterMap.AllowUserToDeleteRows = false;
            this.dgvRegisterMap.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRegisterMap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRegisterMap.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvRegisterMap.Location = new System.Drawing.Point(0, 82);
            this.dgvRegisterMap.Name = "dgvRegisterMap";
            this.dgvRegisterMap.RowHeadersWidth = 51;
            this.dgvRegisterMap.RowTemplate.Height = 27;
            this.dgvRegisterMap.Size = new System.Drawing.Size(839, 258);
            this.dgvRegisterMap.TabIndex = 7;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(10, 70);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(183, 36);
            this.label20.TabIndex = 6;
            this.label20.Text = "寄存器映射：";
            // 
            // numPlcPort
            // 
            this.numPlcPort.Location = new System.Drawing.Point(450, 35);
            this.numPlcPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPlcPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPlcPort.Name = "numPlcPort";
            this.numPlcPort.Size = new System.Drawing.Size(100, 44);
            this.numPlcPort.TabIndex = 5;
            this.numPlcPort.Value = new decimal(new int[] {
            502,
            0,
            0,
            0});
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(350, 38);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(127, 36);
            this.label19.TabIndex = 4;
            this.label19.Text = "端口号：";
            // 
            // txtPlcIP
            // 
            this.txtPlcIP.Location = new System.Drawing.Point(120, 35);
            this.txtPlcIP.Name = "txtPlcIP";
            this.txtPlcIP.Size = new System.Drawing.Size(200, 44);
            this.txtPlcIP.TabIndex = 3;
            this.txtPlcIP.Text = "192.168.1.101";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(30, 38);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(124, 36);
            this.label18.TabIndex = 2;
            this.label18.Text = "IP地址：";
            // 
            // cbPlcBrand
            // 
            this.cbPlcBrand.FormattingEnabled = true;
            this.cbPlcBrand.Items.AddRange(new object[] {
            "ModbusTCP",
            "S7-1200",
            "S7-1500",
            "三菱"});
            this.cbPlcBrand.Location = new System.Drawing.Point(120, 5);
            this.cbPlcBrand.Name = "cbPlcBrand";
            this.cbPlcBrand.Size = new System.Drawing.Size(200, 44);
            this.cbPlcBrand.TabIndex = 1;
            this.cbPlcBrand.Text = "ModbusTCP";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(30, 8);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(149, 36);
            this.label17.TabIndex = 0;
            this.label17.Text = "PLC品牌：";
            // 
            // pnlPlcList
            // 
            this.pnlPlcList.Controls.Add(this.btnRemovePlc);
            this.pnlPlcList.Controls.Add(this.btnAddPlc);
            this.pnlPlcList.Controls.Add(this.dgvPlcList);
            this.pnlPlcList.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPlcList.Location = new System.Drawing.Point(3, 3);
            this.pnlPlcList.Name = "pnlPlcList";
            this.pnlPlcList.Size = new System.Drawing.Size(839, 197);
            this.pnlPlcList.TabIndex = 0;
            // 
            // btnRemovePlc
            // 
            this.btnRemovePlc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemovePlc.Location = new System.Drawing.Point(739, 160);
            this.btnRemovePlc.Name = "btnRemovePlc";
            this.btnRemovePlc.Size = new System.Drawing.Size(80, 30);
            this.btnRemovePlc.TabIndex = 2;
            this.btnRemovePlc.Text = "移除";
            this.btnRemovePlc.UseVisualStyleBackColor = true;
            // 
            // btnAddPlc
            // 
            this.btnAddPlc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddPlc.Location = new System.Drawing.Point(649, 160);
            this.btnAddPlc.Name = "btnAddPlc";
            this.btnAddPlc.Size = new System.Drawing.Size(80, 30);
            this.btnAddPlc.TabIndex = 1;
            this.btnAddPlc.Text = "添加";
            this.btnAddPlc.UseVisualStyleBackColor = true;
            // 
            // dgvPlcList
            // 
            this.dgvPlcList.AllowUserToAddRows = false;
            this.dgvPlcList.AllowUserToDeleteRows = false;
            this.dgvPlcList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPlcList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPlcList.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvPlcList.Location = new System.Drawing.Point(0, 0);
            this.dgvPlcList.Name = "dgvPlcList";
            this.dgvPlcList.RowHeadersWidth = 51;
            this.dgvPlcList.RowTemplate.Height = 27;
            this.dgvPlcList.Size = new System.Drawing.Size(839, 150);
            this.dgvPlcList.TabIndex = 0;
            // 
            // tabIo
            // 
            this.tabIo.Controls.Add(this.pnlOutputList);
            this.tabIo.Controls.Add(this.pnlInputList);
            this.tabIo.Location = new System.Drawing.Point(8, 39);
            this.tabIo.Name = "tabIo";
            this.tabIo.Padding = new System.Windows.Forms.Padding(3);
            this.tabIo.Size = new System.Drawing.Size(845, 543);
            this.tabIo.TabIndex = 3;
            this.tabIo.Text = "IO配置";
            this.tabIo.UseVisualStyleBackColor = true;
            // 
            // pnlOutputList
            // 
            this.pnlOutputList.Controls.Add(this.btnRemoveOutput);
            this.pnlOutputList.Controls.Add(this.btnAddOutput);
            this.pnlOutputList.Controls.Add(this.dgvOutputPoints);
            this.pnlOutputList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlOutputList.Location = new System.Drawing.Point(3, 262);
            this.pnlOutputList.Name = "pnlOutputList";
            this.pnlOutputList.Size = new System.Drawing.Size(839, 278);
            this.pnlOutputList.TabIndex = 1;
            // 
            // btnRemoveOutput
            // 
            this.btnRemoveOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveOutput.Location = new System.Drawing.Point(739, 240);
            this.btnRemoveOutput.Name = "btnRemoveOutput";
            this.btnRemoveOutput.Size = new System.Drawing.Size(80, 30);
            this.btnRemoveOutput.TabIndex = 2;
            this.btnRemoveOutput.Text = "删除";
            this.btnRemoveOutput.UseVisualStyleBackColor = true;
            // 
            // btnAddOutput
            // 
            this.btnAddOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOutput.Location = new System.Drawing.Point(649, 240);
            this.btnAddOutput.Name = "btnAddOutput";
            this.btnAddOutput.Size = new System.Drawing.Size(80, 30);
            this.btnAddOutput.TabIndex = 1;
            this.btnAddOutput.Text = "添加";
            this.btnAddOutput.UseVisualStyleBackColor = true;
            // 
            // dgvOutputPoints
            // 
            this.dgvOutputPoints.AllowUserToAddRows = false;
            this.dgvOutputPoints.AllowUserToDeleteRows = false;
            this.dgvOutputPoints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvOutputPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOutputPoints.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvOutputPoints.Location = new System.Drawing.Point(0, 0);
            this.dgvOutputPoints.Name = "dgvOutputPoints";
            this.dgvOutputPoints.RowHeadersWidth = 51;
            this.dgvOutputPoints.RowTemplate.Height = 27;
            this.dgvOutputPoints.Size = new System.Drawing.Size(839, 230);
            this.dgvOutputPoints.TabIndex = 0;
            // 
            // pnlInputList
            // 
            this.pnlInputList.Controls.Add(this.btnRemoveInput);
            this.pnlInputList.Controls.Add(this.btnAddInput);
            this.pnlInputList.Controls.Add(this.dgvInputPoints);
            this.pnlInputList.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInputList.Location = new System.Drawing.Point(3, 3);
            this.pnlInputList.Name = "pnlInputList";
            this.pnlInputList.Size = new System.Drawing.Size(839, 277);
            this.pnlInputList.TabIndex = 0;
            // 
            // btnRemoveInput
            // 
            this.btnRemoveInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveInput.Location = new System.Drawing.Point(739, 240);
            this.btnRemoveInput.Name = "btnRemoveInput";
            this.btnRemoveInput.Size = new System.Drawing.Size(80, 30);
            this.btnRemoveInput.TabIndex = 2;
            this.btnRemoveInput.Text = "删除";
            this.btnRemoveInput.UseVisualStyleBackColor = true;
            // 
            // btnAddInput
            // 
            this.btnAddInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddInput.Location = new System.Drawing.Point(649, 240);
            this.btnAddInput.Name = "btnAddInput";
            this.btnAddInput.Size = new System.Drawing.Size(80, 30);
            this.btnAddInput.TabIndex = 1;
            this.btnAddInput.Text = "添加";
            this.btnAddInput.UseVisualStyleBackColor = true;
            // 
            // dgvInputPoints
            // 
            this.dgvInputPoints.AllowUserToAddRows = false;
            this.dgvInputPoints.AllowUserToDeleteRows = false;
            this.dgvInputPoints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInputPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInputPoints.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvInputPoints.Location = new System.Drawing.Point(0, 0);
            this.dgvInputPoints.Name = "dgvInputPoints";
            this.dgvInputPoints.RowHeadersWidth = 51;
            this.dgvInputPoints.RowTemplate.Height = 27;
            this.dgvInputPoints.Size = new System.Drawing.Size(839, 230);
            this.dgvInputPoints.TabIndex = 0;
            // 
            // tabNetwork
            // 
            this.tabNetwork.Controls.Add(this.grpRemoteConfig);
            this.tabNetwork.Controls.Add(this.grpMqttConfig);
            this.tabNetwork.Controls.Add(this.grpMesConfig);
            this.tabNetwork.Location = new System.Drawing.Point(8, 39);
            this.tabNetwork.Name = "tabNetwork";
            this.tabNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.tabNetwork.Size = new System.Drawing.Size(845, 543);
            this.tabNetwork.TabIndex = 4;
            this.tabNetwork.Text = "网络配置";
            this.tabNetwork.UseVisualStyleBackColor = true;
            // 
            // grpRemoteConfig
            // 
            this.grpRemoteConfig.Controls.Add(this.cbRemoteEnable);
            this.grpRemoteConfig.Controls.Add(this.numRemotePort);
            this.grpRemoteConfig.Controls.Add(this.label16);
            this.grpRemoteConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpRemoteConfig.Location = new System.Drawing.Point(3, 260);
            this.grpRemoteConfig.Name = "grpRemoteConfig";
            this.grpRemoteConfig.Size = new System.Drawing.Size(839, 100);
            this.grpRemoteConfig.TabIndex = 2;
            this.grpRemoteConfig.TabStop = false;
            this.grpRemoteConfig.Text = "远程监控配置";
            // 
            // cbRemoteEnable
            // 
            this.cbRemoteEnable.AutoSize = true;
            this.cbRemoteEnable.Location = new System.Drawing.Point(30, 60);
            this.cbRemoteEnable.Name = "cbRemoteEnable";
            this.cbRemoteEnable.Size = new System.Drawing.Size(159, 40);
            this.cbRemoteEnable.TabIndex = 2;
            this.cbRemoteEnable.Text = "启用监控";
            this.cbRemoteEnable.UseVisualStyleBackColor = true;
            // 
            // numRemotePort
            // 
            this.numRemotePort.Location = new System.Drawing.Point(150, 30);
            this.numRemotePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numRemotePort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numRemotePort.Name = "numRemotePort";
            this.numRemotePort.Size = new System.Drawing.Size(100, 44);
            this.numRemotePort.TabIndex = 1;
            this.numRemotePort.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(30, 32);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(155, 36);
            this.label16.TabIndex = 0;
            this.label16.Text = "监控端口：";
            // 
            // grpMqttConfig
            // 
            this.grpMqttConfig.Controls.Add(this.txtMqttTopic);
            this.grpMqttConfig.Controls.Add(this.label15);
            this.grpMqttConfig.Controls.Add(this.numMqttPort);
            this.grpMqttConfig.Controls.Add(this.label14);
            this.grpMqttConfig.Controls.Add(this.txtMqttBroker);
            this.grpMqttConfig.Controls.Add(this.label10);
            this.grpMqttConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpMqttConfig.Location = new System.Drawing.Point(3, 130);
            this.grpMqttConfig.Name = "grpMqttConfig";
            this.grpMqttConfig.Size = new System.Drawing.Size(839, 130);
            this.grpMqttConfig.TabIndex = 1;
            this.grpMqttConfig.TabStop = false;
            this.grpMqttConfig.Text = "MQTT配置";
            // 
            // txtMqttTopic
            // 
            this.txtMqttTopic.Location = new System.Drawing.Point(150, 90);
            this.txtMqttTopic.Name = "txtMqttTopic";
            this.txtMqttTopic.Size = new System.Drawing.Size(200, 44);
            this.txtMqttTopic.TabIndex = 5;
            this.txtMqttTopic.Text = "auto/device";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(30, 92);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(172, 36);
            this.label15.TabIndex = 4;
            this.label15.Text = "默认Topic：";
            // 
            // numMqttPort
            // 
            this.numMqttPort.Location = new System.Drawing.Point(150, 60);
            this.numMqttPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numMqttPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMqttPort.Name = "numMqttPort";
            this.numMqttPort.Size = new System.Drawing.Size(100, 44);
            this.numMqttPort.TabIndex = 3;
            this.numMqttPort.Value = new decimal(new int[] {
            1883,
            0,
            0,
            0});
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(30, 62);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(127, 36);
            this.label14.TabIndex = 2;
            this.label14.Text = "端口号：";
            // 
            // txtMqttBroker
            // 
            this.txtMqttBroker.Location = new System.Drawing.Point(150, 30);
            this.txtMqttBroker.Name = "txtMqttBroker";
            this.txtMqttBroker.Size = new System.Drawing.Size(200, 44);
            this.txtMqttBroker.TabIndex = 1;
            this.txtMqttBroker.Text = "192.168.1.10";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(30, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(188, 36);
            this.label10.TabIndex = 0;
            this.label10.Text = "Broker地址：";
            // 
            // grpMesConfig
            // 
            this.grpMesConfig.Controls.Add(this.tableLayoutPanel4);
            this.grpMesConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpMesConfig.Location = new System.Drawing.Point(3, 3);
            this.grpMesConfig.Name = "grpMesConfig";
            this.grpMesConfig.Padding = new System.Windows.Forms.Padding(10);
            this.grpMesConfig.Size = new System.Drawing.Size(839, 127);
            this.grpMesConfig.TabIndex = 0;
            this.grpMesConfig.TabStop = false;
            this.grpMesConfig.Text = "MES配置";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.cbMesProtocol, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.numMesPort, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.txtMesIP, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelMesIP, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(10, 47);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(819, 70);
            this.tableLayoutPanel4.TabIndex = 6;
            // 
            // cbMesProtocol
            // 
            this.cbMesProtocol.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbMesProtocol.FormattingEnabled = true;
            this.cbMesProtocol.Items.AddRange(new object[] {
            "TCP",
            "HTTP",
            "OPC UA"});
            this.cbMesProtocol.Location = new System.Drawing.Point(123, 49);
            this.cbMesProtocol.Name = "cbMesProtocol";
            this.cbMesProtocol.Size = new System.Drawing.Size(200, 44);
            this.cbMesProtocol.TabIndex = 5;
            this.cbMesProtocol.Text = "TCP";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 46);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 24);
            this.label9.TabIndex = 4;
            this.label9.Text = "协议选择：";
            // 
            // numMesPort
            // 
            this.numMesPort.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numMesPort.Location = new System.Drawing.Point(123, 26);
            this.numMesPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numMesPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMesPort.Name = "numMesPort";
            this.numMesPort.Size = new System.Drawing.Size(100, 44);
            this.numMesPort.TabIndex = 3;
            this.numMesPort.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 23);
            this.label8.TabIndex = 2;
            this.label8.Text = "端口号：";
            // 
            // txtMesIP
            // 
            this.txtMesIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMesIP.Location = new System.Drawing.Point(123, 3);
            this.txtMesIP.Name = "txtMesIP";
            this.txtMesIP.Size = new System.Drawing.Size(693, 44);
            this.txtMesIP.TabIndex = 1;
            this.txtMesIP.Text = "192.168.1.200";
            // 
            // labelMesIP
            // 
            this.labelMesIP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMesIP.AutoSize = true;
            this.labelMesIP.Location = new System.Drawing.Point(3, 0);
            this.labelMesIP.Name = "labelMesIP";
            this.labelMesIP.Size = new System.Drawing.Size(101, 23);
            this.labelMesIP.TabIndex = 0;
            this.labelMesIP.Text = "MES服务器IP：";
            // 
            // tabUser
            // 
            this.tabUser.Controls.Add(this.pnlUserEdit);
            this.tabUser.Controls.Add(this.dgvUserList);
            this.tabUser.Location = new System.Drawing.Point(8, 39);
            this.tabUser.Name = "tabUser";
            this.tabUser.Padding = new System.Windows.Forms.Padding(3);
            this.tabUser.Size = new System.Drawing.Size(845, 543);
            this.tabUser.TabIndex = 5;
            this.tabUser.Text = "用户管理";
            this.tabUser.UseVisualStyleBackColor = true;
            // 
            // pnlUserEdit
            // 
            this.pnlUserEdit.Controls.Add(this.btnChangePassword);
            this.pnlUserEdit.Controls.Add(this.btnDeleteUser);
            this.pnlUserEdit.Controls.Add(this.btnAddUser);
            this.pnlUserEdit.Controls.Add(this.txtNewPassword);
            this.pnlUserEdit.Controls.Add(this.label23);
            this.pnlUserEdit.Controls.Add(this.cbUserRole);
            this.pnlUserEdit.Controls.Add(this.label22);
            this.pnlUserEdit.Controls.Add(this.txtNewUsername);
            this.pnlUserEdit.Controls.Add(this.label21);
            this.pnlUserEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlUserEdit.Location = new System.Drawing.Point(3, 342);
            this.pnlUserEdit.Name = "pnlUserEdit";
            this.pnlUserEdit.Size = new System.Drawing.Size(839, 198);
            this.pnlUserEdit.TabIndex = 1;
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangePassword.Location = new System.Drawing.Point(689, 150);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(120, 30);
            this.btnChangePassword.TabIndex = 8;
            this.btnChangePassword.Text = "修改密码";
            this.btnChangePassword.UseVisualStyleBackColor = true;
            // 
            // btnDeleteUser
            // 
            this.btnDeleteUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteUser.Location = new System.Drawing.Point(559, 150);
            this.btnDeleteUser.Name = "btnDeleteUser";
            this.btnDeleteUser.Size = new System.Drawing.Size(120, 30);
            this.btnDeleteUser.TabIndex = 7;
            this.btnDeleteUser.Text = "删除用户";
            this.btnDeleteUser.UseVisualStyleBackColor = true;
            // 
            // btnAddUser
            // 
            this.btnAddUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddUser.Location = new System.Drawing.Point(429, 150);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Size = new System.Drawing.Size(120, 30);
            this.btnAddUser.TabIndex = 6;
            this.btnAddUser.Text = "添加用户";
            this.btnAddUser.UseVisualStyleBackColor = true;
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.Location = new System.Drawing.Point(150, 100);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '*';
            this.txtNewPassword.Size = new System.Drawing.Size(200, 44);
            this.txtNewPassword.TabIndex = 5;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(30, 102);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(99, 36);
            this.label23.TabIndex = 4;
            this.label23.Text = "密码：";
            // 
            // cbUserRole
            // 
            this.cbUserRole.FormattingEnabled = true;
            this.cbUserRole.Items.AddRange(new object[] {
            "操作员",
            "工程师",
            "管理员"});
            this.cbUserRole.Location = new System.Drawing.Point(150, 65);
            this.cbUserRole.Name = "cbUserRole";
            this.cbUserRole.Size = new System.Drawing.Size(200, 44);
            this.cbUserRole.TabIndex = 3;
            this.cbUserRole.Text = "操作员";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(30, 68);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(99, 36);
            this.label22.TabIndex = 2;
            this.label22.Text = "角色：";
            // 
            // txtNewUsername
            // 
            this.txtNewUsername.Location = new System.Drawing.Point(150, 30);
            this.txtNewUsername.Name = "txtNewUsername";
            this.txtNewUsername.Size = new System.Drawing.Size(200, 44);
            this.txtNewUsername.TabIndex = 1;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(30, 32);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(127, 36);
            this.label21.TabIndex = 0;
            this.label21.Text = "用户名：";
            // 
            // dgvUserList
            // 
            this.dgvUserList.AllowUserToAddRows = false;
            this.dgvUserList.AllowUserToDeleteRows = false;
            this.dgvUserList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUserList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserList.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvUserList.Location = new System.Drawing.Point(3, 3);
            this.dgvUserList.Name = "dgvUserList";
            this.dgvUserList.RowHeadersWidth = 51;
            this.dgvUserList.RowTemplate.Height = 27;
            this.dgvUserList.Size = new System.Drawing.Size(839, 357);
            this.dgvUserList.TabIndex = 0;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnExportConfig);
            this.pnlBottom.Controls.Add(this.btnImportConfig);
            this.pnlBottom.Controls.Add(this.btnSaveConfig);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 590);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1084, 60);
            this.pnlBottom.TabIndex = 1;
            // 
            // btnExportConfig
            // 
            this.btnExportConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportConfig.Location = new System.Drawing.Point(900, 15);
            this.btnExportConfig.Name = "btnExportConfig";
            this.btnExportConfig.Size = new System.Drawing.Size(80, 30);
            this.btnExportConfig.TabIndex = 2;
            this.btnExportConfig.Text = "导出配置";
            this.btnExportConfig.UseVisualStyleBackColor = true;
            this.btnExportConfig.Click += new System.EventHandler(this.btnExportConfig_Click);
            // 
            // btnImportConfig
            // 
            this.btnImportConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImportConfig.Location = new System.Drawing.Point(810, 15);
            this.btnImportConfig.Name = "btnImportConfig";
            this.btnImportConfig.Size = new System.Drawing.Size(80, 30);
            this.btnImportConfig.TabIndex = 1;
            this.btnImportConfig.Text = "导入配置";
            this.btnImportConfig.UseVisualStyleBackColor = true;
            this.btnImportConfig.Click += new System.EventHandler(this.btnImportConfig_Click);
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveConfig.ForeColor = System.Drawing.Color.White;
            this.btnSaveConfig.Location = new System.Drawing.Point(710, 15);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(90, 30);
            this.btnSaveConfig.TabIndex = 0;
            this.btnSaveConfig.Text = "保存配置";
            this.btnSaveConfig.UseVisualStyleBackColor = false;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 650);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlBottom);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MinimumSize = new System.Drawing.Size(867, 520);
            this.Name = "SettingForm";
            this.Text = "系统配置";
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingForm_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlNav.ResumeLayout(false);
            this.pnlDeviceNav.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabSystem.ResumeLayout(false);
            this.grpSafetyParams.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAlarmDelay)).EndInit();
            this.grpRunParams.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDefaultAccel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDefaultSpeed)).EndInit();
            this.grpBasicInfo.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabMotion.ResumeLayout(false);
            this.pnlMotionDetail.ResumeLayout(false);
            this.pnlMotionDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAxisParams)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAxisCount)).EndInit();
            this.pnlMotionList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMotionCards)).EndInit();
            this.tabPlc.ResumeLayout(false);
            this.pnlPlcDetail.ResumeLayout(false);
            this.pnlPlcDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRegisterMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPlcPort)).EndInit();
            this.pnlPlcList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlcList)).EndInit();
            this.tabIo.ResumeLayout(false);
            this.pnlOutputList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputPoints)).EndInit();
            this.pnlInputList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputPoints)).EndInit();
            this.tabNetwork.ResumeLayout(false);
            this.grpRemoteConfig.ResumeLayout(false);
            this.grpRemoteConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRemotePort)).EndInit();
            this.grpMqttConfig.ResumeLayout(false);
            this.grpMqttConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMqttPort)).EndInit();
            this.grpMesConfig.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMesPort)).EndInit();
            this.tabUser.ResumeLayout(false);
            this.pnlUserEdit.ResumeLayout(false);
            this.pnlUserEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel pnlNav;
        private System.Windows.Forms.Button btnUserManage;
        private System.Windows.Forms.Button btnNetwork;
        private System.Windows.Forms.Panel pnlDeviceNav;
        private System.Windows.Forms.Button btnIoConfig;
        private System.Windows.Forms.Button btnPlcConfig;
        private System.Windows.Forms.Button btnMotionCard;
        private System.Windows.Forms.Button btnDevice;
        private System.Windows.Forms.Button btnSystem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSystem;
        private System.Windows.Forms.GroupBox grpSafetyParams;
        private System.Windows.Forms.NumericUpDown numAlarmDelay;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbSoftwareLimit;
        private System.Windows.Forms.GroupBox grpRunParams;
        private System.Windows.Forms.NumericUpDown numHomeSpeed;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numDefaultAccel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numDefaultSpeed;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grpBasicInfo;
        private System.Windows.Forms.TextBox txtDeviceSN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDeviceModel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDeviceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabMotion;
        private System.Windows.Forms.Panel pnlMotionDetail;
        private System.Windows.Forms.DataGridView dgvAxisParams;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown numAxisCount;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtMotionIP;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel pnlMotionList;
        private System.Windows.Forms.Button btnRemoveMotion;
        private System.Windows.Forms.Button btnAddMotion;
        private System.Windows.Forms.DataGridView dgvMotionCards;
        private System.Windows.Forms.TabPage tabPlc;
        private System.Windows.Forms.Panel pnlPlcDetail;
        private System.Windows.Forms.DataGridView dgvRegisterMap;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.NumericUpDown numPlcPort;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtPlcIP;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox cbPlcBrand;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Panel pnlPlcList;
        private System.Windows.Forms.Button btnRemovePlc;
        private System.Windows.Forms.Button btnAddPlc;
        private System.Windows.Forms.DataGridView dgvPlcList;
        private System.Windows.Forms.TabPage tabIo;
        private System.Windows.Forms.Panel pnlOutputList;
        private System.Windows.Forms.Button btnRemoveOutput;
        private System.Windows.Forms.Button btnAddOutput;
        private System.Windows.Forms.DataGridView dgvOutputPoints;
        private System.Windows.Forms.Panel pnlInputList;
        private System.Windows.Forms.Button btnRemoveInput;
        private System.Windows.Forms.Button btnAddInput;
        private System.Windows.Forms.DataGridView dgvInputPoints;
        private System.Windows.Forms.TabPage tabNetwork;
        private System.Windows.Forms.GroupBox grpRemoteConfig;
        private System.Windows.Forms.CheckBox cbRemoteEnable;
        private System.Windows.Forms.NumericUpDown numRemotePort;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.GroupBox grpMqttConfig;
        private System.Windows.Forms.TextBox txtMqttTopic;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numMqttPort;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtMqttBroker;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox grpMesConfig;
        private System.Windows.Forms.ComboBox cbMesProtocol;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numMesPort;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtMesIP;
        private System.Windows.Forms.Label labelMesIP;
        private System.Windows.Forms.TabPage tabUser;
        private System.Windows.Forms.Panel pnlUserEdit;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.Button btnDeleteUser;
        private System.Windows.Forms.Button btnAddUser;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.ComboBox cbUserRole;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox txtNewUsername;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.DataGridView dgvUserList;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnExportConfig;
        private System.Windows.Forms.Button btnImportConfig;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
    }
}
