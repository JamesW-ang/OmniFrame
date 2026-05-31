using OmniFrame.Theme;

namespace OmniFrame
{
    partial class ConfigForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label14 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabMotion = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbMotionBrand = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMotionIP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nudAxisCount = new System.Windows.Forms.NumericUpDown();
            this.dgvAxisParams = new System.Windows.Forms.DataGridView();
            this.btnAddAxisParam = new System.Windows.Forms.Button();
            this.btnRemoveAxisParam = new System.Windows.Forms.Button();
            this.tabPlc = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbPlcBrand = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPlcIP = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.nudPlcPort = new System.Windows.Forms.NumericUpDown();
            this.dgvRegisterMaps = new System.Windows.Forms.DataGridView();
            this.btnAddRegisterMap = new System.Windows.Forms.Button();
            this.btnRemoveRegisterMap = new System.Windows.Forms.Button();
            this.tabIo = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvInputs = new System.Windows.Forms.DataGridView();
            this.btnAddInput = new System.Windows.Forms.Button();
            this.btnRemoveInput = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvOutputs = new System.Windows.Forms.DataGridView();
            this.btnAddOutput = new System.Windows.Forms.Button();
            this.btnRemoveOutput = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.dgvTriggerLogics = new System.Windows.Forms.DataGridView();
            this.btnAddTriggerLogic = new System.Windows.Forms.Button();
            this.btnRemoveTriggerLogic = new System.Windows.Forms.Button();
            this.tabSystem = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.label10 = new System.Windows.Forms.Label();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.nudWatchdogInterval = new System.Windows.Forms.NumericUpDown();
            this.dgvAlarmRules = new System.Windows.Forms.DataGridView();
            this.btnAddAlarmRule = new System.Windows.Forms.Button();
            this.btnRemoveAlarmRule = new System.Windows.Forms.Button();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.lstBackupFiles = new System.Windows.Forms.ListBox();
            this.btnRollback = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabMotion.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAxisCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAxisParams)).BeginInit();
            this.tabPlc.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPlcPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRegisterMaps)).BeginInit();
            this.tabIo.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerLogics)).BeginInit();
            this.tabSystem.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWatchdogInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmRules)).BeginInit();
            this.tableLayoutPanel9.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel9, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(838, 634);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(3, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(832, 28);
            this.label14.TabIndex = 0;
            this.label14.Text = "系统配置";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabMotion);
            this.tabControl1.Controls.Add(this.tabPlc);
            this.tabControl1.Controls.Add(this.tabIo);
            this.tabControl1.Controls.Add(this.tabSystem);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 31);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(832, 420);
            this.tabControl1.TabIndex = 1;
            // 
            // tabMotion
            // 
            this.tabMotion.Controls.Add(this.tableLayoutPanel2);
            this.tabMotion.Location = new System.Drawing.Point(8, 50);
            this.tabMotion.Name = "tabMotion";
            this.tabMotion.Padding = new System.Windows.Forms.Padding(3);
            this.tabMotion.Size = new System.Drawing.Size(816, 362);
            this.tabMotion.TabIndex = 0;
            this.tabMotion.Text = "运动卡配置";
            this.tabMotion.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.dgvAxisParams, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnAddAxisParam, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnRemoveAxisParam, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(810, 356);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 6;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmbMotionBrand, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtMotionIP, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.label3, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.nudAxisCount, 5, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(804, 54);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 54);
            this.label1.TabIndex = 0;
            this.label1.Text = "品牌：";
            // 
            // cmbMotionBrand
            // 
            this.cmbMotionBrand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMotionBrand.FormattingEnabled = true;
            this.cmbMotionBrand.Location = new System.Drawing.Point(63, 11);
            this.cmbMotionBrand.Name = "cmbMotionBrand";
            this.cmbMotionBrand.Size = new System.Drawing.Size(94, 44);
            this.cmbMotionBrand.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(163, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 54);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP：";
            // 
            // txtMotionIP
            // 
            this.txtMotionIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMotionIP.Location = new System.Drawing.Point(223, 5);
            this.txtMotionIP.Name = "txtMotionIP";
            this.txtMotionIP.Size = new System.Drawing.Size(144, 44);
            this.txtMotionIP.TabIndex = 3;
            this.txtMotionIP.Text = "192.168.1.100";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(373, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 54);
            this.label3.TabIndex = 4;
            this.label3.Text = "轴数量：";
            // 
            // nudAxisCount
            // 
            this.nudAxisCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudAxisCount.Location = new System.Drawing.Point(453, 5);
            this.nudAxisCount.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudAxisCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAxisCount.Name = "nudAxisCount";
            this.nudAxisCount.Size = new System.Drawing.Size(348, 44);
            this.nudAxisCount.TabIndex = 5;
            this.nudAxisCount.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // dgvAxisParams
            // 
            this.dgvAxisParams.ColumnHeadersHeight = 46;
            this.dgvAxisParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAxisParams.Location = new System.Drawing.Point(3, 63);
            this.dgvAxisParams.Name = "dgvAxisParams";
            this.dgvAxisParams.RowHeadersWidth = 82;
            this.dgvAxisParams.Size = new System.Drawing.Size(804, 211);
            this.dgvAxisParams.TabIndex = 1;
            // 
            // btnAddAxisParam
            // 
            this.btnAddAxisParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAxisParam.Location = new System.Drawing.Point(732, 321);
            this.btnAddAxisParam.Name = "btnAddAxisParam";
            this.btnAddAxisParam.Size = new System.Drawing.Size(75, 28);
            this.btnAddAxisParam.TabIndex = 2;
            this.btnAddAxisParam.Text = "添加";
            this.btnAddAxisParam.UseVisualStyleBackColor = true;
            this.btnAddAxisParam.Click += new System.EventHandler(this.btnAddAxisParam_Click);
            // 
            // btnRemoveAxisParam
            // 
            this.btnRemoveAxisParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveAxisParam.Location = new System.Drawing.Point(732, 280);
            this.btnRemoveAxisParam.Name = "btnRemoveAxisParam";
            this.btnRemoveAxisParam.Size = new System.Drawing.Size(75, 32);
            this.btnRemoveAxisParam.TabIndex = 3;
            this.btnRemoveAxisParam.Text = "删除";
            this.btnRemoveAxisParam.UseVisualStyleBackColor = true;
            this.btnRemoveAxisParam.Click += new System.EventHandler(this.btnRemoveAxisParam_Click);
            // 
            // tabPlc
            // 
            this.tabPlc.Controls.Add(this.tableLayoutPanel4);
            this.tabPlc.Location = new System.Drawing.Point(8, 50);
            this.tabPlc.Name = "tabPlc";
            this.tabPlc.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlc.Size = new System.Drawing.Size(816, 362);
            this.tabPlc.TabIndex = 1;
            this.tabPlc.Text = "PLC配置";
            this.tabPlc.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.dgvRegisterMaps, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.btnAddRegisterMap, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.btnRemoveRegisterMap, 0, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(810, 356);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 6;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.cmbPlcBrand, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel5.Controls.Add(this.txtPlcIP, 3, 0);
            this.tableLayoutPanel5.Controls.Add(this.label6, 4, 0);
            this.tableLayoutPanel5.Controls.Add(this.nudPlcPort, 5, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(804, 54);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 54);
            this.label4.TabIndex = 0;
            this.label4.Text = "品牌：";
            // 
            // cmbPlcBrand
            // 
            this.cmbPlcBrand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPlcBrand.FormattingEnabled = true;
            this.cmbPlcBrand.Location = new System.Drawing.Point(63, 11);
            this.cmbPlcBrand.Name = "cmbPlcBrand";
            this.cmbPlcBrand.Size = new System.Drawing.Size(94, 44);
            this.cmbPlcBrand.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(163, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 54);
            this.label5.TabIndex = 2;
            this.label5.Text = "IP：";
            // 
            // txtPlcIP
            // 
            this.txtPlcIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPlcIP.Location = new System.Drawing.Point(223, 5);
            this.txtPlcIP.Name = "txtPlcIP";
            this.txtPlcIP.Size = new System.Drawing.Size(144, 44);
            this.txtPlcIP.TabIndex = 3;
            this.txtPlcIP.Text = "192.168.1.101";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(373, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 54);
            this.label6.TabIndex = 4;
            this.label6.Text = "端口：";
            // 
            // nudPlcPort
            // 
            this.nudPlcPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPlcPort.Location = new System.Drawing.Point(453, 5);
            this.nudPlcPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPlcPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPlcPort.Name = "nudPlcPort";
            this.nudPlcPort.Size = new System.Drawing.Size(348, 44);
            this.nudPlcPort.TabIndex = 5;
            this.nudPlcPort.Value = new decimal(new int[] {
            502,
            0,
            0,
            0});
            // 
            // dgvRegisterMaps
            // 
            this.dgvRegisterMaps.ColumnHeadersHeight = 46;
            this.dgvRegisterMaps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRegisterMaps.Location = new System.Drawing.Point(3, 63);
            this.dgvRegisterMaps.Name = "dgvRegisterMaps";
            this.dgvRegisterMaps.RowHeadersWidth = 82;
            this.dgvRegisterMaps.Size = new System.Drawing.Size(804, 230);
            this.dgvRegisterMaps.TabIndex = 1;
            // 
            // btnAddRegisterMap
            // 
            this.btnAddRegisterMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddRegisterMap.Location = new System.Drawing.Point(732, 339);
            this.btnAddRegisterMap.Name = "btnAddRegisterMap";
            this.btnAddRegisterMap.Size = new System.Drawing.Size(75, 14);
            this.btnAddRegisterMap.TabIndex = 2;
            this.btnAddRegisterMap.Text = "添加";
            this.btnAddRegisterMap.UseVisualStyleBackColor = true;
            this.btnAddRegisterMap.Click += new System.EventHandler(this.btnAddRegisterMap_Click);
            // 
            // btnRemoveRegisterMap
            // 
            this.btnRemoveRegisterMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveRegisterMap.Location = new System.Drawing.Point(732, 299);
            this.btnRemoveRegisterMap.Name = "btnRemoveRegisterMap";
            this.btnRemoveRegisterMap.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveRegisterMap.TabIndex = 3;
            this.btnRemoveRegisterMap.Text = "删除";
            this.btnRemoveRegisterMap.UseVisualStyleBackColor = true;
            this.btnRemoveRegisterMap.Click += new System.EventHandler(this.btnRemoveRegisterMap_Click);
            // 
            // tabIo
            // 
            this.tabIo.Controls.Add(this.tableLayoutPanel6);
            this.tabIo.Location = new System.Drawing.Point(8, 50);
            this.tabIo.Name = "tabIo";
            this.tabIo.Padding = new System.Windows.Forms.Padding(3);
            this.tabIo.Size = new System.Drawing.Size(816, 362);
            this.tabIo.TabIndex = 2;
            this.tabIo.Text = "IO配置";
            this.tabIo.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.dgvInputs, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.btnAddInput, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.btnRemoveInput, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.label8, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.dgvOutputs, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.btnAddOutput, 0, 5);
            this.tableLayoutPanel6.Controls.Add(this.btnRemoveOutput, 0, 5);
            this.tableLayoutPanel6.Controls.Add(this.label9, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.dgvTriggerLogics, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.btnAddTriggerLogic, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.btnRemoveTriggerLogic, 1, 2);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 6;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 67F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(810, 356);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(314, 25);
            this.label7.TabIndex = 2;
            this.label7.Text = "输入点位：";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvInputs
            // 
            this.dgvInputs.ColumnHeadersHeight = 46;
            this.dgvInputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInputs.Location = new System.Drawing.Point(3, 28);
            this.dgvInputs.Name = "dgvInputs";
            this.dgvInputs.RowHeadersWidth = 82;
            this.dgvInputs.Size = new System.Drawing.Size(314, 69);
            this.dgvInputs.TabIndex = 3;
            // 
            // btnAddInput
            // 
            this.btnAddInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddInput.Location = new System.Drawing.Point(732, 103);
            this.btnAddInput.Name = "btnAddInput";
            this.btnAddInput.Size = new System.Drawing.Size(75, 29);
            this.btnAddInput.TabIndex = 4;
            this.btnAddInput.Text = "添加";
            this.btnAddInput.UseVisualStyleBackColor = true;
            this.btnAddInput.Click += new System.EventHandler(this.btnAddInput_Click);
            // 
            // btnRemoveInput
            // 
            this.btnRemoveInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveInput.Location = new System.Drawing.Point(242, 103);
            this.btnRemoveInput.Name = "btnRemoveInput";
            this.btnRemoveInput.Size = new System.Drawing.Size(75, 29);
            this.btnRemoveInput.TabIndex = 5;
            this.btnRemoveInput.Text = "删除";
            this.btnRemoveInput.UseVisualStyleBackColor = true;
            this.btnRemoveInput.Click += new System.EventHandler(this.btnRemoveInput_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 169);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(314, 152);
            this.label8.TabIndex = 1;
            this.label8.Text = "输出点位：";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvOutputs
            // 
            this.dgvOutputs.ColumnHeadersHeight = 46;
            this.dgvOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOutputs.Location = new System.Drawing.Point(323, 172);
            this.dgvOutputs.Name = "dgvOutputs";
            this.dgvOutputs.RowHeadersWidth = 82;
            this.dgvOutputs.Size = new System.Drawing.Size(484, 146);
            this.dgvOutputs.TabIndex = 6;
            // 
            // btnAddOutput
            // 
            this.btnAddOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOutput.Location = new System.Drawing.Point(732, 324);
            this.btnAddOutput.Name = "btnAddOutput";
            this.btnAddOutput.Size = new System.Drawing.Size(75, 29);
            this.btnAddOutput.TabIndex = 7;
            this.btnAddOutput.Text = "添加";
            this.btnAddOutput.UseVisualStyleBackColor = true;
            this.btnAddOutput.Click += new System.EventHandler(this.btnAddOutput_Click);
            // 
            // btnRemoveOutput
            // 
            this.btnRemoveOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveOutput.Location = new System.Drawing.Point(242, 324);
            this.btnRemoveOutput.Name = "btnRemoveOutput";
            this.btnRemoveOutput.Size = new System.Drawing.Size(75, 29);
            this.btnRemoveOutput.TabIndex = 8;
            this.btnRemoveOutput.Text = "删除";
            this.btnRemoveOutput.UseVisualStyleBackColor = true;
            this.btnRemoveOutput.Click += new System.EventHandler(this.btnRemoveOutput_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(323, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(484, 25);
            this.label9.TabIndex = 0;
            this.label9.Text = "触发逻辑：";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvTriggerLogics
            // 
            this.dgvTriggerLogics.ColumnHeadersHeight = 46;
            this.dgvTriggerLogics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTriggerLogics.Location = new System.Drawing.Point(323, 28);
            this.dgvTriggerLogics.Name = "dgvTriggerLogics";
            this.dgvTriggerLogics.RowHeadersWidth = 82;
            this.dgvTriggerLogics.Size = new System.Drawing.Size(484, 69);
            this.dgvTriggerLogics.TabIndex = 9;
            // 
            // btnAddTriggerLogic
            // 
            this.btnAddTriggerLogic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddTriggerLogic.Location = new System.Drawing.Point(242, 138);
            this.btnAddTriggerLogic.Name = "btnAddTriggerLogic";
            this.btnAddTriggerLogic.Size = new System.Drawing.Size(75, 28);
            this.btnAddTriggerLogic.TabIndex = 10;
            this.btnAddTriggerLogic.Text = "添加";
            this.btnAddTriggerLogic.UseVisualStyleBackColor = true;
            this.btnAddTriggerLogic.Click += new System.EventHandler(this.btnAddTriggerLogic_Click);
            // 
            // btnRemoveTriggerLogic
            // 
            this.btnRemoveTriggerLogic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTriggerLogic.Location = new System.Drawing.Point(732, 138);
            this.btnRemoveTriggerLogic.Name = "btnRemoveTriggerLogic";
            this.btnRemoveTriggerLogic.Size = new System.Drawing.Size(75, 28);
            this.btnRemoveTriggerLogic.TabIndex = 11;
            this.btnRemoveTriggerLogic.Text = "删除";
            this.btnRemoveTriggerLogic.UseVisualStyleBackColor = true;
            this.btnRemoveTriggerLogic.Click += new System.EventHandler(this.btnRemoveTriggerLogic_Click);
            // 
            // tabSystem
            // 
            this.tabSystem.Controls.Add(this.tableLayoutPanel7);
            this.tabSystem.Location = new System.Drawing.Point(8, 50);
            this.tabSystem.Name = "tabSystem";
            this.tabSystem.Padding = new System.Windows.Forms.Padding(3);
            this.tabSystem.Size = new System.Drawing.Size(816, 362);
            this.tabSystem.TabIndex = 3;
            this.tabSystem.Text = "系统配置";
            this.tabSystem.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel8, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.dgvAlarmRules, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.btnAddAlarmRule, 0, 3);
            this.tableLayoutPanel7.Controls.Add(this.btnRemoveAlarmRule, 0, 3);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 4;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(810, 356);
            this.tableLayoutPanel7.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(804, 25);
            this.label10.TabIndex = 0;
            this.label10.Text = "告警规则：";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 4;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.label11, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.txtLogPath, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.label12, 2, 0);
            this.tableLayoutPanel8.Controls.Add(this.nudWatchdogInterval, 3, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 28);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(804, 54);
            this.tableLayoutPanel8.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 54);
            this.label11.TabIndex = 0;
            this.label11.Text = "日志路径：";
            // 
            // txtLogPath
            // 
            this.txtLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogPath.Location = new System.Drawing.Point(83, 5);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(194, 44);
            this.txtLogPath.TabIndex = 1;
            this.txtLogPath.Text = "Logs/";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(283, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(114, 54);
            this.label12.TabIndex = 2;
            this.label12.Text = "看门狗间隔(ms)：";
            // 
            // nudWatchdogInterval
            // 
            this.nudWatchdogInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudWatchdogInterval.Location = new System.Drawing.Point(403, 5);
            this.nudWatchdogInterval.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.nudWatchdogInterval.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudWatchdogInterval.Name = "nudWatchdogInterval";
            this.nudWatchdogInterval.Size = new System.Drawing.Size(398, 44);
            this.nudWatchdogInterval.TabIndex = 3;
            this.nudWatchdogInterval.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // dgvAlarmRules
            // 
            this.dgvAlarmRules.ColumnHeadersHeight = 46;
            this.dgvAlarmRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlarmRules.Location = new System.Drawing.Point(3, 88);
            this.dgvAlarmRules.Name = "dgvAlarmRules";
            this.dgvAlarmRules.RowHeadersWidth = 82;
            this.dgvAlarmRules.Size = new System.Drawing.Size(804, 200);
            this.dgvAlarmRules.TabIndex = 1;
            // 
            // btnAddAlarmRule
            // 
            this.btnAddAlarmRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAlarmRule.Location = new System.Drawing.Point(732, 326);
            this.btnAddAlarmRule.Name = "btnAddAlarmRule";
            this.btnAddAlarmRule.Size = new System.Drawing.Size(75, 27);
            this.btnAddAlarmRule.TabIndex = 2;
            this.btnAddAlarmRule.Text = "添加";
            this.btnAddAlarmRule.UseVisualStyleBackColor = true;
            this.btnAddAlarmRule.Click += new System.EventHandler(this.btnAddAlarmRule_Click);
            // 
            // btnRemoveAlarmRule
            // 
            this.btnRemoveAlarmRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveAlarmRule.Location = new System.Drawing.Point(732, 294);
            this.btnRemoveAlarmRule.Name = "btnRemoveAlarmRule";
            this.btnRemoveAlarmRule.Size = new System.Drawing.Size(75, 26);
            this.btnRemoveAlarmRule.TabIndex = 3;
            this.btnRemoveAlarmRule.Text = "删除";
            this.btnRemoveAlarmRule.UseVisualStyleBackColor = true;
            this.btnRemoveAlarmRule.Click += new System.EventHandler(this.btnRemoveAlarmRule_Click);
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 2;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.btnSave, 1, 0);
            this.tableLayoutPanel9.Controls.Add(this.btnExport, 1, 0);
            this.tableLayoutPanel9.Controls.Add(this.btnImport, 1, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 457);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 1;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(832, 174);
            this.tableLayoutPanel9.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel10);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox1.Size = new System.Drawing.Size(384, 122);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "配置备份";
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 2;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanel10.Controls.Add(this.label13, 0, 0);
            this.tableLayoutPanel10.Controls.Add(this.lstBackupFiles, 0, 1);
            this.tableLayoutPanel10.Controls.Add(this.btnRollback, 1, 1);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(10, 46);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 2;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(364, 66);
            this.tableLayoutPanel10.TabIndex = 0;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(3, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(266, 25);
            this.label13.TabIndex = 0;
            this.label13.Text = "备份文件：";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lstBackupFiles
            // 
            this.lstBackupFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstBackupFiles.FormattingEnabled = true;
            this.lstBackupFiles.ItemHeight = 36;
            this.lstBackupFiles.Location = new System.Drawing.Point(3, 28);
            this.lstBackupFiles.Name = "lstBackupFiles";
            this.lstBackupFiles.Size = new System.Drawing.Size(266, 35);
            this.lstBackupFiles.TabIndex = 1;
            // 
            // btnRollback
            // 
            this.btnRollback.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRollback.Location = new System.Drawing.Point(286, 28);
            this.btnRollback.Name = "btnRollback";
            this.btnRollback.Size = new System.Drawing.Size(75, 29);
            this.btnRollback.TabIndex = 2;
            this.btnRollback.Text = "回滚";
            this.btnRollback.UseVisualStyleBackColor = true;
            this.btnRollback.Click += new System.EventHandler(this.btnRollback_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(754, 141);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(322, 141);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 30);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImport.Location = new System.Drawing.Point(754, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 31);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "导入";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 634);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "ConfigForm";
            this.Text = "系统配置";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabMotion.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAxisCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAxisParams)).EndInit();
            this.tabPlc.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPlcPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRegisterMaps)).EndInit();
            this.tabIo.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerLogics)).EndInit();
            this.tabSystem.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWatchdogInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmRules)).EndInit();
            this.tableLayoutPanel9.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel10.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabMotion;
        private System.Windows.Forms.TabPage tabPlc;
        private System.Windows.Forms.TabPage tabIo;
        private System.Windows.Forms.TabPage tabSystem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private System.Windows.Forms.Button btnRollback;
        private System.Windows.Forms.ListBox lstBackupFiles;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnRemoveAxisParam;
        private System.Windows.Forms.Button btnAddAxisParam;
        private System.Windows.Forms.DataGridView dgvAxisParams;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbMotionBrand;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMotionIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudAxisCount;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button btnRemoveRegisterMap;
        private System.Windows.Forms.Button btnAddRegisterMap;
        private System.Windows.Forms.DataGridView dgvRegisterMaps;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbPlcBrand;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPlcIP;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudPlcPort;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button btnRemoveTriggerLogic;
        private System.Windows.Forms.Button btnAddTriggerLogic;
        private System.Windows.Forms.DataGridView dgvTriggerLogics;
        private System.Windows.Forms.Button btnRemoveOutput;
        private System.Windows.Forms.Button btnAddOutput;
        private System.Windows.Forms.DataGridView dgvOutputs;
        private System.Windows.Forms.Button btnRemoveInput;
        private System.Windows.Forms.Button btnAddInput;
        private System.Windows.Forms.DataGridView dgvInputs;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Button btnRemoveAlarmRule;
        private System.Windows.Forms.Button btnAddAlarmRule;
        private System.Windows.Forms.DataGridView dgvAlarmRules;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudWatchdogInterval;
        private System.Windows.Forms.Label label10;
    }
}
