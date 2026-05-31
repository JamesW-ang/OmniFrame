namespace OmniFrame
{
    partial class BlockCutMesConfigForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageHttp = new System.Windows.Forms.TabPage();
            this.panelHttp = new System.Windows.Forms.Panel();
            this.btnTestHttp = new System.Windows.Forms.Button();
            this.lblHttpStatus = new System.Windows.Forms.Label();
            this.lblHttpUrl = new System.Windows.Forms.Label();
            this._txtHttpUrl = new System.Windows.Forms.TextBox();
            this.tabPageMqtt = new System.Windows.Forms.TabPage();
            this.panelMqtt = new System.Windows.Forms.Panel();
            this.btnTestMqtt = new System.Windows.Forms.Button();
            this.lblMqttStatus = new System.Windows.Forms.Label();
            this.lblMqttPort = new System.Windows.Forms.Label();
            this._txtMqttPort = new System.Windows.Forms.TextBox();
            this.lblMqttBroker = new System.Windows.Forms.Label();
            this._txtMqttBroker = new System.Windows.Forms.TextBox();
            this.tabPageSecurity = new System.Windows.Forms.TabPage();
            this.panelSecurity = new System.Windows.Forms.Panel();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.lblDeviceNo = new System.Windows.Forms.Label();
            this._txtDeviceNo = new System.Windows.Forms.TextBox();
            this.lblAesKey = new System.Windows.Forms.Label();
            this._txtAesKey = new System.Windows.Forms.TextBox();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.panelGeneral = new System.Windows.Forms.Panel();
            this.lblLastComm = new System.Windows.Forms.Label();
            this.lblSimulationStatus = new System.Windows.Forms.Label();
            this._chkSimulation = new System.Windows.Forms.CheckBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageHttp.SuspendLayout();
            this.panelHttp.SuspendLayout();
            this.tabPageMqtt.SuspendLayout();
            this.panelMqtt.SuspendLayout();
            this.tabPageSecurity.SuspendLayout();
            this.panelSecurity.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.panelGeneral.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageHttp);
            this.tabControl1.Controls.Add(this.tabPageMqtt);
            this.tabControl1.Controls.Add(this.tabPageSecurity);
            this.tabControl1.Controls.Add(this.tabPageGeneral);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(600, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageHttp
            // 
            this.tabPageHttp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tabPageHttp.Controls.Add(this.panelHttp);
            this.tabPageHttp.Location = new System.Drawing.Point(4, 25);
            this.tabPageHttp.Name = "tabPageHttp";
            this.tabPageHttp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHttp.Size = new System.Drawing.Size(592, 421);
            this.tabPageHttp.TabIndex = 0;
            this.tabPageHttp.Text = "HTTP 配置";
            // 
            // panelHttp
            // 
            this.panelHttp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.panelHttp.Controls.Add(this.btnTestHttp);
            this.panelHttp.Controls.Add(this.lblHttpStatus);
            this.panelHttp.Controls.Add(this.lblHttpUrl);
            this.panelHttp.Controls.Add(this._txtHttpUrl);
            this.panelHttp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHttp.Location = new System.Drawing.Point(3, 3);
            this.panelHttp.Name = "panelHttp";
            this.panelHttp.Size = new System.Drawing.Size(586, 415);
            this.panelHttp.TabIndex = 0;
            // 
            // btnTestHttp
            // 
            this.btnTestHttp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(140)))));
            this.btnTestHttp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestHttp.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnTestHttp.ForeColor = System.Drawing.Color.White;
            this.btnTestHttp.Location = new System.Drawing.Point(20, 80);
            this.btnTestHttp.Name = "btnTestHttp";
            this.btnTestHttp.Size = new System.Drawing.Size(120, 35);
            this.btnTestHttp.TabIndex = 3;
            this.btnTestHttp.Text = "连接测试";
            this.btnTestHttp.UseVisualStyleBackColor = false;
            // 
            // lblHttpStatus
            // 
            this.lblHttpStatus.AutoSize = true;
            this.lblHttpStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblHttpStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblHttpStatus.Location = new System.Drawing.Point(160, 88);
            this.lblHttpStatus.Name = "lblHttpStatus";
            this.lblHttpStatus.Size = new System.Drawing.Size(80, 17);
            this.lblHttpStatus.TabIndex = 2;
            this.lblHttpStatus.Text = "未测试";
            // 
            // lblHttpUrl
            // 
            this.lblHttpUrl.AutoSize = true;
            this.lblHttpUrl.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblHttpUrl.ForeColor = System.Drawing.Color.White;
            this.lblHttpUrl.Location = new System.Drawing.Point(20, 20);
            this.lblHttpUrl.Name = "lblHttpUrl";
            this.lblHttpUrl.Size = new System.Drawing.Size(68, 17);
            this.lblHttpUrl.TabIndex = 1;
            this.lblHttpUrl.Text = "服务地址";
            // 
            // _txtHttpUrl
            // 
            this._txtHttpUrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtHttpUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtHttpUrl.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtHttpUrl.ForeColor = System.Drawing.Color.White;
            this._txtHttpUrl.Location = new System.Drawing.Point(20, 40);
            this._txtHttpUrl.Name = "_txtHttpUrl";
            this._txtHttpUrl.Size = new System.Drawing.Size(540, 22);
            this._txtHttpUrl.TabIndex = 0;
            // 
            // tabPageMqtt
            // 
            this.tabPageMqtt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tabPageMqtt.Controls.Add(this.panelMqtt);
            this.tabPageMqtt.Location = new System.Drawing.Point(4, 25);
            this.tabPageMqtt.Name = "tabPageMqtt";
            this.tabPageMqtt.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMqtt.Size = new System.Drawing.Size(592, 421);
            this.tabPageMqtt.TabIndex = 1;
            this.tabPageMqtt.Text = "MQTT 配置";
            // 
            // panelMqtt
            // 
            this.panelMqtt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.panelMqtt.Controls.Add(this.btnTestMqtt);
            this.panelMqtt.Controls.Add(this.lblMqttStatus);
            this.panelMqtt.Controls.Add(this.lblMqttPort);
            this.panelMqtt.Controls.Add(this._txtMqttPort);
            this.panelMqtt.Controls.Add(this.lblMqttBroker);
            this.panelMqtt.Controls.Add(this._txtMqttBroker);
            this.panelMqtt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMqtt.Location = new System.Drawing.Point(3, 3);
            this.panelMqtt.Name = "panelMqtt";
            this.panelMqtt.Size = new System.Drawing.Size(586, 415);
            this.panelMqtt.TabIndex = 0;
            // 
            // btnTestMqtt
            // 
            this.btnTestMqtt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(140)))));
            this.btnTestMqtt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestMqtt.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnTestMqtt.ForeColor = System.Drawing.Color.White;
            this.btnTestMqtt.Location = new System.Drawing.Point(20, 110);
            this.btnTestMqtt.Name = "btnTestMqtt";
            this.btnTestMqtt.Size = new System.Drawing.Size(120, 35);
            this.btnTestMqtt.TabIndex = 4;
            this.btnTestMqtt.Text = "连接测试";
            this.btnTestMqtt.UseVisualStyleBackColor = false;
            // 
            // lblMqttStatus
            // 
            this.lblMqttStatus.AutoSize = true;
            this.lblMqttStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblMqttStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblMqttStatus.Location = new System.Drawing.Point(160, 118);
            this.lblMqttStatus.Name = "lblMqttStatus";
            this.lblMqttStatus.Size = new System.Drawing.Size(80, 17);
            this.lblMqttStatus.TabIndex = 3;
            this.lblMqttStatus.Text = "未测试";
            // 
            // lblMqttPort
            // 
            this.lblMqttPort.AutoSize = true;
            this.lblMqttPort.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblMqttPort.ForeColor = System.Drawing.Color.White;
            this.lblMqttPort.Location = new System.Drawing.Point(20, 65);
            this.lblMqttPort.Name = "lblMqttPort";
            this.lblMqttPort.Size = new System.Drawing.Size(68, 17);
            this.lblMqttPort.TabIndex = 2;
            this.lblMqttPort.Text = "端口";
            // 
            // _txtMqttPort
            // 
            this._txtMqttPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtMqttPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtMqttPort.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtMqttPort.ForeColor = System.Drawing.Color.White;
            this._txtMqttPort.Location = new System.Drawing.Point(20, 85);
            this._txtMqttPort.Name = "_txtMqttPort";
            this._txtMqttPort.Size = new System.Drawing.Size(150, 22);
            this._txtMqttPort.TabIndex = 1;
            // 
            // lblMqttBroker
            // 
            this.lblMqttBroker.AutoSize = true;
            this.lblMqttBroker.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblMqttBroker.ForeColor = System.Drawing.Color.White;
            this.lblMqttBroker.Location = new System.Drawing.Point(20, 20);
            this.lblMqttBroker.Name = "lblMqttBroker";
            this.lblMqttBroker.Size = new System.Drawing.Size(68, 17);
            this.lblMqttBroker.TabIndex = 0;
            this.lblMqttBroker.Text = "Broker";
            // 
            // _txtMqttBroker
            // 
            this._txtMqttBroker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtMqttBroker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtMqttBroker.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtMqttBroker.ForeColor = System.Drawing.Color.White;
            this._txtMqttBroker.Location = new System.Drawing.Point(20, 40);
            this._txtMqttBroker.Name = "_txtMqttBroker";
            this._txtMqttBroker.Size = new System.Drawing.Size(540, 22);
            this._txtMqttBroker.TabIndex = 0;
            // 
            // tabPageSecurity
            // 
            this.tabPageSecurity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tabPageSecurity.Controls.Add(this.panelSecurity);
            this.tabPageSecurity.Location = new System.Drawing.Point(4, 25);
            this.tabPageSecurity.Name = "tabPageSecurity";
            this.tabPageSecurity.Size = new System.Drawing.Size(592, 421);
            this.tabPageSecurity.TabIndex = 2;
            this.tabPageSecurity.Text = "安全配置";
            // 
            // panelSecurity
            // 
            this.panelSecurity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.panelSecurity.Controls.Add(this.btnGenerateKey);
            this.panelSecurity.Controls.Add(this.lblDeviceNo);
            this.panelSecurity.Controls.Add(this._txtDeviceNo);
            this.panelSecurity.Controls.Add(this.lblAesKey);
            this.panelSecurity.Controls.Add(this._txtAesKey);
            this.panelSecurity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSecurity.Location = new System.Drawing.Point(0, 0);
            this.panelSecurity.Name = "panelSecurity";
            this.panelSecurity.Size = new System.Drawing.Size(592, 421);
            this.panelSecurity.TabIndex = 0;
            // 
            // btnGenerateKey
            // 
            this.btnGenerateKey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(0)))));
            this.btnGenerateKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateKey.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnGenerateKey.ForeColor = System.Drawing.Color.White;
            this.btnGenerateKey.Location = new System.Drawing.Point(20, 110);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(120, 35);
            this.btnGenerateKey.TabIndex = 4;
            this.btnGenerateKey.Text = "生成密钥";
            this.btnGenerateKey.UseVisualStyleBackColor = false;
            // 
            // lblDeviceNo
            // 
            this.lblDeviceNo.AutoSize = true;
            this.lblDeviceNo.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblDeviceNo.ForeColor = System.Drawing.Color.White;
            this.lblDeviceNo.Location = new System.Drawing.Point(20, 20);
            this.lblDeviceNo.Name = "lblDeviceNo";
            this.lblDeviceNo.Size = new System.Drawing.Size(68, 17);
            this.lblDeviceNo.TabIndex = 3;
            this.lblDeviceNo.Text = "设备编号";
            // 
            // _txtDeviceNo
            // 
            this._txtDeviceNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtDeviceNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtDeviceNo.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtDeviceNo.ForeColor = System.Drawing.Color.White;
            this._txtDeviceNo.Location = new System.Drawing.Point(20, 40);
            this._txtDeviceNo.Name = "_txtDeviceNo";
            this._txtDeviceNo.Size = new System.Drawing.Size(540, 22);
            this._txtDeviceNo.TabIndex = 2;
            // 
            // lblAesKey
            // 
            this.lblAesKey.AutoSize = true;
            this.lblAesKey.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblAesKey.ForeColor = System.Drawing.Color.White;
            this.lblAesKey.Location = new System.Drawing.Point(20, 65);
            this.lblAesKey.Name = "lblAesKey";
            this.lblAesKey.Size = new System.Drawing.Size(68, 17);
            this.lblAesKey.TabIndex = 1;
            this.lblAesKey.Text = "AES 密钥";
            // 
            // _txtAesKey
            // 
            this._txtAesKey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._txtAesKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtAesKey.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtAesKey.ForeColor = System.Drawing.Color.White;
            this._txtAesKey.Location = new System.Drawing.Point(20, 85);
            this._txtAesKey.Name = "_txtAesKey";
            this._txtAesKey.PasswordChar = '*';
            this._txtAesKey.Size = new System.Drawing.Size(540, 22);
            this._txtAesKey.TabIndex = 0;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tabPageGeneral.Controls.Add(this.panelGeneral);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 25);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Size = new System.Drawing.Size(592, 421);
            this.tabPageGeneral.TabIndex = 3;
            this.tabPageGeneral.Text = "通用配置";
            // 
            // panelGeneral
            // 
            this.panelGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.panelGeneral.Controls.Add(this.lblLastComm);
            this.panelGeneral.Controls.Add(this.lblSimulationStatus);
            this.panelGeneral.Controls.Add(this._chkSimulation);
            this.panelGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGeneral.Location = new System.Drawing.Point(0, 0);
            this.panelGeneral.Name = "panelGeneral";
            this.panelGeneral.Size = new System.Drawing.Size(592, 421);
            this.panelGeneral.TabIndex = 0;
            // 
            // lblLastComm
            // 
            this.lblLastComm.AutoSize = true;
            this.lblLastComm.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblLastComm.ForeColor = System.Drawing.Color.Gray;
            this.lblLastComm.Location = new System.Drawing.Point(20, 70);
            this.lblLastComm.Name = "lblLastComm";
            this.lblLastComm.Size = new System.Drawing.Size(120, 17);
            this.lblLastComm.TabIndex = 2;
            this.lblLastComm.Text = "上次通信: 未连接";
            // 
            // lblSimulationStatus
            // 
            this.lblSimulationStatus.AutoSize = true;
            this.lblSimulationStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblSimulationStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblSimulationStatus.Location = new System.Drawing.Point(150, 45);
            this.lblSimulationStatus.Name = "lblSimulationStatus";
            this.lblSimulationStatus.Size = new System.Drawing.Size(80, 17);
            this.lblSimulationStatus.TabIndex = 1;
            this.lblSimulationStatus.Text = "已禁用";
            // 
            // _chkSimulation
            // 
            this._chkSimulation.AutoSize = true;
            this._chkSimulation.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this._chkSimulation.ForeColor = System.Drawing.Color.White;
            this._chkSimulation.Location = new System.Drawing.Point(20, 43);
            this._chkSimulation.Name = "_chkSimulation";
            this._chkSimulation.Size = new System.Drawing.Size(108, 21);
            this._chkSimulation.TabIndex = 0;
            this._chkSimulation.Text = "启用仿真模式";
            this._chkSimulation.UseVisualStyleBackColor = true;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.panelBottom.Controls.Add(this.btnReset);
            this.panelBottom.Controls.Add(this.btnSave);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 450);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(600, 60);
            this.panelBottom.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnReset.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnReset.ForeColor = System.Drawing.Color.White;
            this.btnReset.Location = new System.Drawing.Point(380, 15);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 35);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "恢复默认";
            this.btnReset.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(140)))));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(490, 15);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存配置";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // BlockCutMesConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(600, 510);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panelBottom);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlockCutMesConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MES 配置";
            this.tabControl1.ResumeLayout(false);
            this.tabPageHttp.ResumeLayout(false);
            this.panelHttp.ResumeLayout(false);
            this.panelHttp.PerformLayout();
            this.tabPageMqtt.ResumeLayout(false);
            this.panelMqtt.ResumeLayout(false);
            this.panelMqtt.PerformLayout();
            this.tabPageSecurity.ResumeLayout(false);
            this.panelSecurity.ResumeLayout(false);
            this.panelSecurity.PerformLayout();
            this.tabPageGeneral.ResumeLayout(false);
            this.panelGeneral.ResumeLayout(false);
            this.panelGeneral.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageHttp;
        private System.Windows.Forms.Panel panelHttp;
        private System.Windows.Forms.TabPage tabPageMqtt;
        private System.Windows.Forms.Panel panelMqtt;
        private System.Windows.Forms.TabPage tabPageSecurity;
        private System.Windows.Forms.Panel panelSecurity;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.Panel panelGeneral;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox _txtHttpUrl;
        private System.Windows.Forms.Label lblHttpUrl;
        private System.Windows.Forms.Label lblHttpStatus;
        private System.Windows.Forms.Button btnTestHttp;
        private System.Windows.Forms.TextBox _txtMqttBroker;
        private System.Windows.Forms.Label lblMqttBroker;
        private System.Windows.Forms.Label lblMqttPort;
        private System.Windows.Forms.TextBox _txtMqttPort;
        private System.Windows.Forms.Label lblMqttStatus;
        private System.Windows.Forms.Button btnTestMqtt;
        private System.Windows.Forms.TextBox _txtAesKey;
        private System.Windows.Forms.Label lblAesKey;
        private System.Windows.Forms.Button btnGenerateKey;
        private System.Windows.Forms.Label lblDeviceNo;
        private System.Windows.Forms.TextBox _txtDeviceNo;
        private System.Windows.Forms.CheckBox _chkSimulation;
        private System.Windows.Forms.Label lblSimulationStatus;
        private System.Windows.Forms.Label lblLastComm;
    }
}
