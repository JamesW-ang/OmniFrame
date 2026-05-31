namespace OmniFrame
{
    partial class MonitorForm
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
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxAOIResult = new System.Windows.Forms.GroupBox();
            this.tableLayoutAOI = new System.Windows.Forms.TableLayoutPanel();
            this.lblAOIStatus = new System.Windows.Forms.Label();
            this.panelAOIInfo = new System.Windows.Forms.Panel();
            this.lblAOIBoardId = new System.Windows.Forms.Label();
            this.lblAOICycleTime = new System.Windows.Forms.Label();
            this.lblAOIDefectCount = new System.Windows.Forms.Label();
            this.lblYieldBig = new System.Windows.Forms.Label();
            this.groupBoxDefects = new System.Windows.Forms.GroupBox();
            this.listViewDefects = new System.Windows.Forms.ListView();
            this.colDefectCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDefectType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDefectPos = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDefectValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxStats = new System.Windows.Forms.GroupBox();
            this.tableLayoutStats = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalInspected = new System.Windows.Forms.Label();
            this.lblPassCount = new System.Windows.Forms.Label();
            this.lblFailCount = new System.Windows.Forms.Label();
            this.lblPassRate = new System.Windows.Forms.Label();
            this.lblAvgCycleTime = new System.Windows.Forms.Label();
            this.lblCpk = new System.Windows.Forms.Label();
            this.groupBoxAlarms = new System.Windows.Forms.GroupBox();
            this.listViewAlarms = new System.Windows.Forms.ListView();
            this.colAlarmTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAlarmCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAlarmMsg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelTopBar = new System.Windows.Forms.Panel();
            this.lblSystemState = new System.Windows.Forms.Label();
            this.lblModeTag = new System.Windows.Forms.Label();
            this.lblRunTime = new System.Windows.Forms.Label();
            this.lblRecipeName = new System.Windows.Forms.Label();
            this.lblDeviceStatus = new System.Windows.Forms.Label();
            this.tableLayoutMain.SuspendLayout();
            this.groupBoxAOIResult.SuspendLayout();
            this.tableLayoutAOI.SuspendLayout();
            this.panelAOIInfo.SuspendLayout();
            this.groupBoxDefects.SuspendLayout();
            this.groupBoxStats.SuspendLayout();
            this.tableLayoutStats.SuspendLayout();
            this.groupBoxAlarms.SuspendLayout();
            this.panelTopBar.SuspendLayout();
            this.SuspendLayout();

            // tableLayoutMain
            this.tableLayoutMain.ColumnCount = 3;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutMain.Controls.Add(this.groupBoxAOIResult, 0, 1);
            this.tableLayoutMain.Controls.Add(this.groupBoxDefects, 0, 2);
            this.tableLayoutMain.Controls.Add(this.groupBoxStats, 1, 2);
            this.tableLayoutMain.Controls.Add(this.groupBoxAlarms, 2, 2);
            this.tableLayoutMain.Controls.Add(this.panelTopBar, 0, 0);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tableLayoutMain.Size = new System.Drawing.Size(1221, 687);
            this.tableLayoutMain.TabIndex = 0;
            // tableLayoutMain.SetColumnSpan(this.panelTopBar, 3);
            this.tableLayoutMain.SetColumnSpan(this.panelTopBar, 3);

            // groupBoxAOIResult
            this.groupBoxAOIResult.BackColor = System.Drawing.Color.FromArgb(245, 245, 250);
            this.groupBoxAOIResult.Controls.Add(this.tableLayoutAOI);
            this.groupBoxAOIResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAOIResult.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxAOIResult.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.groupBoxAOIResult.Location = new System.Drawing.Point(4, 40);
            this.groupBoxAOIResult.Margin = new System.Windows.Forms.Padding(4, 4, 2, 4);
            this.groupBoxAOIResult.Name = "groupBoxAOIResult";
            this.groupBoxAOIResult.Size = new System.Drawing.Size(726, 369);
            this.groupBoxAOIResult.TabIndex = 0;
            this.groupBoxAOIResult.TabStop = false;
            this.groupBoxAOIResult.Text = "实时检测结果";

            // tableLayoutAOI
            this.tableLayoutAOI.ColumnCount = 2;
            this.tableLayoutAOI.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutAOI.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutAOI.Controls.Add(this.lblAOIStatus, 0, 0);
            this.tableLayoutAOI.Controls.Add(this.panelAOIInfo, 0, 1);
            this.tableLayoutAOI.Controls.Add(this.lblYieldBig, 1, 1);
            this.tableLayoutAOI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutAOI.Location = new System.Drawing.Point(3, 24);
            this.tableLayoutAOI.Name = "tableLayoutAOI";
            this.tableLayoutAOI.RowCount = 2;
            this.tableLayoutAOI.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutAOI.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutAOI.Size = new System.Drawing.Size(720, 342);
            this.tableLayoutAOI.TabIndex = 0;

            // lblAOIStatus
            this.lblAOIStatus.BackColor = System.Drawing.Color.LimeGreen;
            this.lblAOIStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAOIStatus.Font = new System.Drawing.Font("Microsoft YaHei", 48F, System.Drawing.FontStyle.Bold);
            this.lblAOIStatus.ForeColor = System.Drawing.Color.White;
            this.lblAOIStatus.Location = new System.Drawing.Point(8, 5);
            this.lblAOIStatus.Margin = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblAOIStatus.Name = "lblAOIStatus";
            this.lblAOIStatus.Size = new System.Drawing.Size(380, 178);
            this.lblAOIStatus.TabIndex = 0;
            this.lblAOIStatus.Text = "OK";
            this.lblAOIStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // panelAOIInfo
            this.panelAOIInfo.Controls.Add(this.lblAOIBoardId);
            this.panelAOIInfo.Controls.Add(this.lblAOICycleTime);
            this.panelAOIInfo.Controls.Add(this.lblAOIDefectCount);
            this.panelAOIInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAOIInfo.Location = new System.Drawing.Point(3, 191);
            this.panelAOIInfo.Name = "panelAOIInfo";
            this.panelAOIInfo.Size = new System.Drawing.Size(390, 148);
            this.panelAOIInfo.TabIndex = 1;

            this.lblAOIBoardId.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.lblAOIBoardId.Location = new System.Drawing.Point(8, 8);
            this.lblAOIBoardId.Name = "lblAOIBoardId";
            this.lblAOIBoardId.Size = new System.Drawing.Size(374, 35);
            this.lblAOIBoardId.TabIndex = 0;
            this.lblAOIBoardId.Text = "当前板号: ---";
            this.lblAOIBoardId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblAOICycleTime.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
            this.lblAOICycleTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblAOICycleTime.Location = new System.Drawing.Point(8, 51);
            this.lblAOICycleTime.Name = "lblAOICycleTime";
            this.lblAOICycleTime.Size = new System.Drawing.Size(374, 30);
            this.lblAOICycleTime.TabIndex = 1;
            this.lblAOICycleTime.Text = "检测节拍: ---";

            this.lblAOIDefectCount.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
            this.lblAOIDefectCount.ForeColor = System.Drawing.Color.DimGray;
            this.lblAOIDefectCount.Location = new System.Drawing.Point(8, 89);
            this.lblAOIDefectCount.Name = "lblAOIDefectCount";
            this.lblAOIDefectCount.Size = new System.Drawing.Size(374, 30);
            this.lblAOIDefectCount.TabIndex = 2;
            this.lblAOIDefectCount.Text = "缺陷数: 0";
            this.lblAOIDefectCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // lblYieldBig
            this.lblYieldBig.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
            this.lblYieldBig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblYieldBig.Font = new System.Drawing.Font("Microsoft YaHei", 26F, System.Drawing.FontStyle.Bold);
            this.lblYieldBig.ForeColor = System.Drawing.Color.FromArgb(0, 100, 180);
            this.lblYieldBig.Location = new System.Drawing.Point(405, 191);
            this.lblYieldBig.Margin = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblYieldBig.Name = "lblYieldBig";
            this.lblYieldBig.Size = new System.Drawing.Size(307, 148);
            this.lblYieldBig.TabIndex = 2;
            this.lblYieldBig.Text = "良率 98.0%";
            this.lblYieldBig.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tableLayoutAOI.SetColumnSpan(this.lblYieldBig, 1);

            // groupBoxDefects
            this.groupBoxDefects.Controls.Add(this.listViewDefects);
            this.groupBoxDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDefects.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxDefects.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.groupBoxDefects.Location = new System.Drawing.Point(4, 417);
            this.groupBoxDefects.Margin = new System.Windows.Forms.Padding(4, 4, 2, 4);
            this.groupBoxDefects.Name = "groupBoxDefects";
            this.groupBoxDefects.Size = new System.Drawing.Size(726, 266);
            this.groupBoxDefects.TabIndex = 1;
            this.groupBoxDefects.TabStop = false;
            this.groupBoxDefects.Text = "缺陷明细";

            // listViewDefects
            this.listViewDefects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colDefectCode,
                this.colDefectType,
                this.colDefectPos,
                this.colDefectValue});
            this.listViewDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewDefects.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.listViewDefects.FullRowSelect = true;
            this.listViewDefects.GridLines = true;
            this.listViewDefects.HideSelection = false;
            this.listViewDefects.Location = new System.Drawing.Point(3, 24);
            this.listViewDefects.Name = "listViewDefects";
            this.listViewDefects.Size = new System.Drawing.Size(720, 239);
            this.listViewDefects.TabIndex = 0;
            this.listViewDefects.UseCompatibleStateImageBehavior = false;
            this.listViewDefects.View = System.Windows.Forms.View.Details;
            this.listViewDefects.Tag = "EngineerOnly";

            this.colDefectCode.Text = "缺陷代码";
            this.colDefectCode.Width = 120;
            this.colDefectType.Text = "缺陷类型";
            this.colDefectType.Width = 140;
            this.colDefectPos.Text = "位置 (X, Y)";
            this.colDefectPos.Width = 180;
            this.colDefectValue.Text = "测量值";
            this.colDefectValue.Width = 120;

            // groupBoxStats
            this.groupBoxStats.Controls.Add(this.tableLayoutStats);
            this.groupBoxStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxStats.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxStats.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.groupBoxStats.Location = new System.Drawing.Point(734, 417);
            this.groupBoxStats.Margin = new System.Windows.Forms.Padding(2, 4, 4, 4);
            this.groupBoxStats.Name = "groupBoxStats";
            this.groupBoxStats.Size = new System.Drawing.Size(236, 266);
            this.groupBoxStats.TabIndex = 2;
            this.groupBoxStats.TabStop = false;
            this.groupBoxStats.Text = "生产统计";

            // tableLayoutStats
            this.tableLayoutStats.ColumnCount = 1;
            this.tableLayoutStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutStats.Controls.Add(this.lblTotalInspected, 0, 0);
            this.tableLayoutStats.Controls.Add(this.lblPassCount, 0, 1);
            this.tableLayoutStats.Controls.Add(this.lblFailCount, 0, 2);
            this.tableLayoutStats.Controls.Add(this.lblPassRate, 0, 3);
            this.tableLayoutStats.Controls.Add(this.lblAvgCycleTime, 0, 4);
            this.tableLayoutStats.Controls.Add(this.lblCpk, 0, 5);
            this.tableLayoutStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutStats.Location = new System.Drawing.Point(3, 24);
            this.tableLayoutStats.Name = "tableLayoutStats";
            this.tableLayoutStats.RowCount = 6;
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.8F));
            this.tableLayoutStats.Size = new System.Drawing.Size(229, 239);

            this.lblTotalInspected.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTotalInspected.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblTotalInspected.Location = new System.Drawing.Point(3, 12);
            this.lblTotalInspected.Name = "lblTotalInspected";
            this.lblTotalInspected.Size = new System.Drawing.Size(200, 17);
            this.lblTotalInspected.TabIndex = 0;
            this.lblTotalInspected.Text = "总检测: 0";

            this.lblPassCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPassCount.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblPassCount.ForeColor = System.Drawing.Color.Green;
            this.lblPassCount.Location = new System.Drawing.Point(3, 52);
            this.lblPassCount.Name = "lblPassCount";
            this.lblPassCount.Size = new System.Drawing.Size(200, 17);
            this.lblPassCount.TabIndex = 1;
            this.lblPassCount.Text = "OK: 0";

            this.lblFailCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFailCount.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblFailCount.ForeColor = System.Drawing.Color.Red;
            this.lblFailCount.Location = new System.Drawing.Point(3, 92);
            this.lblFailCount.Name = "lblFailCount";
            this.lblFailCount.Size = new System.Drawing.Size(200, 17);
            this.lblFailCount.TabIndex = 2;
            this.lblFailCount.Text = "NG: 0";

            this.lblPassRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPassRate.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblPassRate.Location = new System.Drawing.Point(3, 132);
            this.lblPassRate.Name = "lblPassRate";
            this.lblPassRate.Size = new System.Drawing.Size(200, 17);
            this.lblPassRate.TabIndex = 3;
            this.lblPassRate.Text = "良率: 0.00%";

            this.lblAvgCycleTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvgCycleTime.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblAvgCycleTime.Location = new System.Drawing.Point(3, 172);
            this.lblAvgCycleTime.Name = "lblAvgCycleTime";
            this.lblAvgCycleTime.Size = new System.Drawing.Size(200, 17);
            this.lblAvgCycleTime.TabIndex = 4;
            this.lblAvgCycleTime.Text = "平均节拍: 0.00s";

            this.lblCpk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCpk.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblCpk.ForeColor = System.Drawing.Color.FromArgb(0, 100, 180);
            this.lblCpk.Location = new System.Drawing.Point(3, 212);
            this.lblCpk.Name = "lblCpk";
            this.lblCpk.Size = new System.Drawing.Size(200, 17);
            this.lblCpk.TabIndex = 5;
            this.lblCpk.Text = "CPK: --";
            this.lblCpk.Tag = "EngineerOnly";

            // groupBoxAlarms
            this.groupBoxAlarms.Controls.Add(this.listViewAlarms);
            this.groupBoxAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAlarms.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxAlarms.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.groupBoxAlarms.Location = new System.Drawing.Point(974, 417);
            this.groupBoxAlarms.Margin = new System.Windows.Forms.Padding(2, 4, 4, 4);
            this.groupBoxAlarms.Name = "groupBoxAlarms";
            this.groupBoxAlarms.Size = new System.Drawing.Size(243, 266);
            this.groupBoxAlarms.TabIndex = 3;
            this.groupBoxAlarms.TabStop = false;
            this.groupBoxAlarms.Text = "实时报警";

            // listViewAlarms
            this.listViewAlarms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colAlarmTime,
                this.colAlarmCode,
                this.colAlarmMsg});
            this.listViewAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAlarms.Font = new System.Drawing.Font("Microsoft YaHei", 8F);
            this.listViewAlarms.FullRowSelect = true;
            this.listViewAlarms.GridLines = true;
            this.listViewAlarms.HideSelection = false;
            this.listViewAlarms.Location = new System.Drawing.Point(3, 24);
            this.listViewAlarms.Name = "listViewAlarms";
            this.listViewAlarms.Size = new System.Drawing.Size(237, 239);
            this.listViewAlarms.TabIndex = 0;
            this.listViewAlarms.UseCompatibleStateImageBehavior = false;
            this.listViewAlarms.View = System.Windows.Forms.View.Details;

            this.colAlarmTime.Text = "时间";
            this.colAlarmTime.Width = 70;
            this.colAlarmCode.Text = "代码";
            this.colAlarmCode.Width = 60;
            this.colAlarmMsg.Text = "信息";
            this.colAlarmMsg.Width = 90;

            // panelTopBar
            this.panelTopBar.BackColor = System.Drawing.Color.FromArgb(50, 60, 75);
            this.panelTopBar.Controls.Add(this.lblSystemState);
            this.panelTopBar.Controls.Add(this.lblModeTag);
            this.panelTopBar.Controls.Add(this.lblRunTime);
            this.panelTopBar.Controls.Add(this.lblRecipeName);
            this.panelTopBar.Controls.Add(this.lblDeviceStatus);
            this.panelTopBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTopBar.Location = new System.Drawing.Point(0, 0);
            this.panelTopBar.Margin = new System.Windows.Forms.Padding(0);
            this.panelTopBar.Name = "panelTopBar";
            this.panelTopBar.Size = new System.Drawing.Size(1221, 36);
            this.panelTopBar.TabIndex = 4;

            this.lblSystemState.AutoSize = true;
            this.lblSystemState.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.lblSystemState.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblSystemState.Location = new System.Drawing.Point(12, 8);
            this.lblSystemState.Name = "lblSystemState";
            this.lblSystemState.Size = new System.Drawing.Size(60, 20);
            this.lblSystemState.TabIndex = 0;
            this.lblSystemState.Text = "运行中";

            this.lblModeTag.AutoSize = true;
            this.lblModeTag.Font = new System.Drawing.Font("Microsoft YaHei", 8F, System.Drawing.FontStyle.Bold);
            this.lblModeTag.ForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            this.lblModeTag.Location = new System.Drawing.Point(80, 10);
            this.lblModeTag.Name = "lblModeTag";
            this.lblModeTag.Size = new System.Drawing.Size(86, 16);
            this.lblModeTag.TabIndex = 1;
            this.lblModeTag.Text = "[操作员模式]";

            this.lblRunTime.AutoSize = true;
            this.lblRunTime.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblRunTime.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            this.lblRunTime.Location = new System.Drawing.Point(220, 9);
            this.lblRunTime.Name = "lblRunTime";
            this.lblRunTime.Size = new System.Drawing.Size(115, 17);
            this.lblRunTime.TabIndex = 2;
            this.lblRunTime.Text = "运行时间: 00:00:00";

            this.lblRecipeName.AutoSize = true;
            this.lblRecipeName.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblRecipeName.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            this.lblRecipeName.Location = new System.Drawing.Point(380, 9);
            this.lblRecipeName.Name = "lblRecipeName";
            this.lblRecipeName.Size = new System.Drawing.Size(128, 17);
            this.lblRecipeName.TabIndex = 3;
            this.lblRecipeName.Text = "当前配方: PCB-默认";

            this.lblDeviceStatus.AutoSize = true;
            this.lblDeviceStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblDeviceStatus.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblDeviceStatus.Location = new System.Drawing.Point(540, 9);
            this.lblDeviceStatus.Name = "lblDeviceStatus";
            this.lblDeviceStatus.Size = new System.Drawing.Size(67, 17);
            this.lblDeviceStatus.TabIndex = 4;
            this.lblDeviceStatus.Text = "设备: 正常";
            this.lblDeviceStatus.Tag = "EngineerOnly";

            // MonitorForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1221, 687);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
            this.MinimumSize = new System.Drawing.Size(977, 550);
            this.Name = "MonitorForm";
            this.Text = "AOI 实时监控";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MonitorForm_FormClosing);
            this.Load += new System.EventHandler(this.MonitorForm_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.groupBoxAOIResult.ResumeLayout(false);
            this.tableLayoutAOI.ResumeLayout(false);
            this.panelAOIInfo.ResumeLayout(false);
            this.groupBoxDefects.ResumeLayout(false);
            this.groupBoxStats.ResumeLayout(false);
            this.tableLayoutStats.ResumeLayout(false);
            this.groupBoxAlarms.ResumeLayout(false);
            this.panelTopBar.ResumeLayout(false);
            this.panelTopBar.PerformLayout();
            this.ResumeLayout(false);
        }

        #region 控件声明
        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.GroupBox groupBoxAOIResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutAOI;
        private System.Windows.Forms.Label lblAOIStatus;
        private System.Windows.Forms.Panel panelAOIInfo;
        private System.Windows.Forms.Label lblAOIBoardId;
        private System.Windows.Forms.Label lblAOICycleTime;
        private System.Windows.Forms.Label lblAOIDefectCount;
        private System.Windows.Forms.Label lblYieldBig;
        private System.Windows.Forms.GroupBox groupBoxDefects;
        private System.Windows.Forms.ListView listViewDefects;
        private System.Windows.Forms.ColumnHeader colDefectCode;
        private System.Windows.Forms.ColumnHeader colDefectType;
        private System.Windows.Forms.ColumnHeader colDefectPos;
        private System.Windows.Forms.ColumnHeader colDefectValue;
        private System.Windows.Forms.GroupBox groupBoxStats;
        private System.Windows.Forms.TableLayoutPanel tableLayoutStats;
        private System.Windows.Forms.Label lblTotalInspected;
        private System.Windows.Forms.Label lblPassCount;
        private System.Windows.Forms.Label lblFailCount;
        private System.Windows.Forms.Label lblPassRate;
        private System.Windows.Forms.Label lblAvgCycleTime;
        private System.Windows.Forms.Label lblCpk;
        private System.Windows.Forms.GroupBox groupBoxAlarms;
        private System.Windows.Forms.ListView listViewAlarms;
        private System.Windows.Forms.ColumnHeader colAlarmTime;
        private System.Windows.Forms.ColumnHeader colAlarmCode;
        private System.Windows.Forms.ColumnHeader colAlarmMsg;
        private System.Windows.Forms.Panel panelTopBar;
        private System.Windows.Forms.Label lblSystemState;
        private System.Windows.Forms.Label lblModeTag;
        private System.Windows.Forms.Label lblRunTime;
        private System.Windows.Forms.Label lblRecipeName;
        private System.Windows.Forms.Label lblDeviceStatus;
        #endregion
    }
}
