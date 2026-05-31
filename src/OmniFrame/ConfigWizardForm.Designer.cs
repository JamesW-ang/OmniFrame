using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmniFrame
{
    partial class ConfigWizardForm
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.lblStep2 = new System.Windows.Forms.Label();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.lblStep4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pnlStep4 = new System.Windows.Forms.Panel();
            this.label19 = new System.Windows.Forms.Label();
            this.numWatchdogInterval = new System.Windows.Forms.NumericUpDown();
            this.label20 = new System.Windows.Forms.Label();
            this.cbxLogLevel = new System.Windows.Forms.ComboBox();
            this.pnlStep3 = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.txtDataSavePath = new System.Windows.Forms.TextBox();
            this.btnBrowseDataPath = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.btnBrowseLogPath = new System.Windows.Forms.Button();
            this.pnlStep2 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.numWebApiPort = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.numWebSocketPort = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.txtCorsWhitelist = new System.Windows.Forms.TextBox();
            this.pnlStep1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPlcIp = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.numPlcPort = new System.Windows.Forms.NumericUpDown();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.pnlStep4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWatchdogInterval)).BeginInit();
            this.pnlStep3.SuspendLayout();
            this.pnlStep2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWebApiPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWebSocketPort)).BeginInit();
            this.pnlStep1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPlcPort)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(846, 60);
            this.panel1.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(100, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "系统配置向导";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.lblStep1);
            this.panel3.Controls.Add(this.lblStep2);
            this.panel3.Controls.Add(this.lblStep3);
            this.panel3.Controls.Add(this.lblStep4);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 60);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(846, 50);
            this.panel3.TabIndex = 1;
            // 
            // lblStep1
            // 
            this.lblStep1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblStep1.ForeColor = System.Drawing.Color.Blue;
            this.lblStep1.Location = new System.Drawing.Point(94, 13);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(100, 23);
            this.lblStep1.TabIndex = 0;
            this.lblStep1.Text = "●";
            // 
            // lblStep2
            // 
            this.lblStep2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblStep2.ForeColor = System.Drawing.Color.Gray;
            this.lblStep2.Location = new System.Drawing.Point(317, 13);
            this.lblStep2.Name = "lblStep2";
            this.lblStep2.Size = new System.Drawing.Size(57, 23);
            this.lblStep2.TabIndex = 1;
            this.lblStep2.Text = "○";
            // 
            // lblStep3
            // 
            this.lblStep3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblStep3.ForeColor = System.Drawing.Color.Gray;
            this.lblStep3.Location = new System.Drawing.Point(486, 13);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(100, 23);
            this.lblStep3.TabIndex = 2;
            this.lblStep3.Text = "○";
            // 
            // lblStep4
            // 
            this.lblStep4.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblStep4.ForeColor = System.Drawing.Color.Gray;
            this.lblStep4.Location = new System.Drawing.Point(705, 17);
            this.lblStep4.Name = "lblStep4";
            this.lblStep4.Size = new System.Drawing.Size(100, 23);
            this.lblStep4.TabIndex = 3;
            this.lblStep4.Text = "○";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "设备连接参数";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(211, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "网络地址设置";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(380, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "文件路径设置";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(599, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "系统参数设置";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.pnlStep4);
            this.panel4.Controls.Add(this.pnlStep3);
            this.panel4.Controls.Add(this.pnlStep2);
            this.panel4.Controls.Add(this.pnlStep1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 110);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(846, 363);
            this.panel4.TabIndex = 0;
            // 
            // pnlStep4
            // 
            this.pnlStep4.Controls.Add(this.label19);
            this.pnlStep4.Controls.Add(this.numWatchdogInterval);
            this.pnlStep4.Controls.Add(this.label20);
            this.pnlStep4.Controls.Add(this.cbxLogLevel);
            this.pnlStep4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep4.Location = new System.Drawing.Point(0, 0);
            this.pnlStep4.Name = "pnlStep4";
            this.pnlStep4.Size = new System.Drawing.Size(846, 363);
            this.pnlStep4.TabIndex = 0;
            this.pnlStep4.Visible = false;
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(96, 96);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(100, 23);
            this.label19.TabIndex = 0;
            this.label19.Text = "看门狗间隔(ms):";
            // 
            // numWatchdogInterval
            // 
            this.numWatchdogInterval.Location = new System.Drawing.Point(210, 93);
            this.numWatchdogInterval.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numWatchdogInterval.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numWatchdogInterval.Name = "numWatchdogInterval";
            this.numWatchdogInterval.Size = new System.Drawing.Size(200, 21);
            this.numWatchdogInterval.TabIndex = 1;
            this.numWatchdogInterval.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label20
            // 
            this.label20.Location = new System.Drawing.Point(96, 136);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(100, 23);
            this.label20.TabIndex = 2;
            this.label20.Text = "日志级别:";
            // 
            // cbxLogLevel
            // 
            this.cbxLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLogLevel.Items.AddRange(new object[] {
            "Trace",
            "Debug",
            "Info",
            "Warning",
            "Error",
            "Fatal"});
            this.cbxLogLevel.Location = new System.Drawing.Point(210, 134);
            this.cbxLogLevel.Name = "cbxLogLevel";
            this.cbxLogLevel.Size = new System.Drawing.Size(200, 20);
            this.cbxLogLevel.TabIndex = 3;
            // 
            // pnlStep3
            // 
            this.pnlStep3.Controls.Add(this.label15);
            this.pnlStep3.Controls.Add(this.txtDataSavePath);
            this.pnlStep3.Controls.Add(this.btnBrowseDataPath);
            this.pnlStep3.Controls.Add(this.label16);
            this.pnlStep3.Controls.Add(this.txtLogPath);
            this.pnlStep3.Controls.Add(this.btnBrowseLogPath);
            this.pnlStep3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep3.Location = new System.Drawing.Point(0, 0);
            this.pnlStep3.Name = "pnlStep3";
            this.pnlStep3.Size = new System.Drawing.Size(846, 363);
            this.pnlStep3.TabIndex = 1;
            this.pnlStep3.Visible = false;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(80, 43);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(100, 23);
            this.label15.TabIndex = 0;
            this.label15.Text = "数据保存路径:";
            // 
            // txtDataSavePath
            // 
            this.txtDataSavePath.Location = new System.Drawing.Point(180, 40);
            this.txtDataSavePath.Name = "txtDataSavePath";
            this.txtDataSavePath.Size = new System.Drawing.Size(300, 21);
            this.txtDataSavePath.TabIndex = 1;
            // 
            // btnBrowseDataPath
            // 
            this.btnBrowseDataPath.Location = new System.Drawing.Point(480, 40);
            this.btnBrowseDataPath.Name = "btnBrowseDataPath";
            this.btnBrowseDataPath.Size = new System.Drawing.Size(75, 21);
            this.btnBrowseDataPath.TabIndex = 2;
            this.btnBrowseDataPath.Text = "浏览...";
            this.btnBrowseDataPath.Click += new System.EventHandler(this.btnBrowseDataPath_Click);
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(80, 83);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(100, 23);
            this.label16.TabIndex = 3;
            this.label16.Text = "日志保存路径:";
            // 
            // txtLogPath
            // 
            this.txtLogPath.Location = new System.Drawing.Point(180, 80);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(300, 21);
            this.txtLogPath.TabIndex = 4;
            // 
            // btnBrowseLogPath
            // 
            this.btnBrowseLogPath.Location = new System.Drawing.Point(480, 80);
            this.btnBrowseLogPath.Name = "btnBrowseLogPath";
            this.btnBrowseLogPath.Size = new System.Drawing.Size(75, 21);
            this.btnBrowseLogPath.TabIndex = 5;
            this.btnBrowseLogPath.Text = "浏览...";
            this.btnBrowseLogPath.Click += new System.EventHandler(this.btnBrowseLogPath_Click);
            // 
            // pnlStep2
            // 
            this.pnlStep2.Controls.Add(this.label10);
            this.pnlStep2.Controls.Add(this.numWebApiPort);
            this.pnlStep2.Controls.Add(this.label11);
            this.pnlStep2.Controls.Add(this.numWebSocketPort);
            this.pnlStep2.Controls.Add(this.label12);
            this.pnlStep2.Controls.Add(this.txtCorsWhitelist);
            this.pnlStep2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep2.Location = new System.Drawing.Point(0, 0);
            this.pnlStep2.Name = "pnlStep2";
            this.pnlStep2.Size = new System.Drawing.Size(846, 363);
            this.pnlStep2.TabIndex = 2;
            this.pnlStep2.Visible = false;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(80, 43);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(100, 23);
            this.label10.TabIndex = 0;
            this.label10.Text = "Web API端口:";
            // 
            // numWebApiPort
            // 
            this.numWebApiPort.Location = new System.Drawing.Point(180, 40);
            this.numWebApiPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numWebApiPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWebApiPort.Name = "numWebApiPort";
            this.numWebApiPort.Size = new System.Drawing.Size(200, 21);
            this.numWebApiPort.TabIndex = 1;
            this.numWebApiPort.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(80, 83);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 23);
            this.label11.TabIndex = 2;
            this.label11.Text = "WebSocket端口:";
            // 
            // numWebSocketPort
            // 
            this.numWebSocketPort.Location = new System.Drawing.Point(180, 80);
            this.numWebSocketPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numWebSocketPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWebSocketPort.Name = "numWebSocketPort";
            this.numWebSocketPort.Size = new System.Drawing.Size(200, 21);
            this.numWebSocketPort.TabIndex = 3;
            this.numWebSocketPort.Value = new decimal(new int[] {
            8081,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(80, 123);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(100, 23);
            this.label12.TabIndex = 4;
            this.label12.Text = "CORS白名单:";
            // 
            // txtCorsWhitelist
            // 
            this.txtCorsWhitelist.Location = new System.Drawing.Point(180, 120);
            this.txtCorsWhitelist.Name = "txtCorsWhitelist";
            this.txtCorsWhitelist.Size = new System.Drawing.Size(300, 21);
            this.txtCorsWhitelist.TabIndex = 5;
            // 
            // pnlStep1
            // 
            this.pnlStep1.Controls.Add(this.label6);
            this.pnlStep1.Controls.Add(this.txtPlcIp);
            this.pnlStep1.Controls.Add(this.label7);
            this.pnlStep1.Controls.Add(this.numPlcPort);
            this.pnlStep1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep1.Location = new System.Drawing.Point(0, 0);
            this.pnlStep1.Name = "pnlStep1";
            this.pnlStep1.Size = new System.Drawing.Size(846, 363);
            this.pnlStep1.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(80, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 23);
            this.label6.TabIndex = 0;
            this.label6.Text = "PLC IP地址:";
            // 
            // txtPlcIp
            // 
            this.txtPlcIp.Location = new System.Drawing.Point(180, 40);
            this.txtPlcIp.Name = "txtPlcIp";
            this.txtPlcIp.Size = new System.Drawing.Size(200, 21);
            this.txtPlcIp.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(80, 83);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 23);
            this.label7.TabIndex = 2;
            this.label7.Text = "PLC端口:";
            // 
            // numPlcPort
            // 
            this.numPlcPort.Location = new System.Drawing.Point(180, 80);
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
            this.numPlcPort.Size = new System.Drawing.Size(200, 21);
            this.numPlcPort.TabIndex = 3;
            this.numPlcPort.Value = new decimal(new int[] {
            502,
            0,
            0,
            0});
            //
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.btnPrevious);
            this.panel2.Controls.Add(this.btnNext);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 473);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(846, 60);
            this.panel2.TabIndex = 2;
            // 
            // btnPrevious
            // 
            this.btnPrevious.Enabled = false;
            this.btnPrevious.Location = new System.Drawing.Point(310, 15);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(80, 30);
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Text = "上一步";
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(400, 15);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(80, 30);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "下一步";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(490, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ConfigWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 533);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "系统配置向导";
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.pnlStep4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numWatchdogInterval)).EndInit();
            this.pnlStep3.ResumeLayout(false);
            this.pnlStep3.PerformLayout();
            this.pnlStep2.ResumeLayout(false);
            this.pnlStep2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWebApiPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWebSocketPort)).EndInit();
            this.pnlStep1.ResumeLayout(false);
            this.pnlStep1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPlcPort)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}