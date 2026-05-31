namespace OmniFrame
{
    partial class MainForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_userRole = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_stationStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_tcpStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_opcStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_mqttStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblViewMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_systemInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.监控界面ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.数据报表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.配方管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.设备控制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.工位管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.系统设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.辅助功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.用户管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.系统日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于系统ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.panel_Main = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStrip1.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_userRole,
            this.toolStripStatusLabel_stationStatus,
            this.toolStripStatusLabel_tcpStatus,
            this.toolStripStatusLabel_opcStatus,
            this.toolStripStatusLabel_mqttStatus,
            this.lblViewMode,
            this.toolStripStatusLabel_systemInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 730);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1000, 25);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_userRole
            // 
            this.toolStripStatusLabel_userRole.Name = "toolStripStatusLabel_userRole";
            this.toolStripStatusLabel_userRole.Size = new System.Drawing.Size(150, 20);
            this.toolStripStatusLabel_userRole.Text = "用户: admin | 角色: 管理员";
            // 
            // toolStripStatusLabel_stationStatus
            // 
            this.toolStripStatusLabel_stationStatus.Name = "toolStripStatusLabel_stationStatus";
            this.toolStripStatusLabel_stationStatus.Size = new System.Drawing.Size(150, 20);
            this.toolStripStatusLabel_stationStatus.Text = "工位: 工位1 | 状态: 运行";
            // 
            // toolStripStatusLabel_tcpStatus
            // 
            this.toolStripStatusLabel_tcpStatus.Name = "toolStripStatusLabel_tcpStatus";
            this.toolStripStatusLabel_tcpStatus.Size = new System.Drawing.Size(80, 20);
            this.toolStripStatusLabel_tcpStatus.Text = "TCP: 已连接";
            // 
            // toolStripStatusLabel_opcStatus
            // 
            this.toolStripStatusLabel_opcStatus.Name = "toolStripStatusLabel_opcStatus";
            this.toolStripStatusLabel_opcStatus.Size = new System.Drawing.Size(80, 20);
            this.toolStripStatusLabel_opcStatus.Text = "OPC: 已连接";
            // 
            // toolStripStatusLabel_mqttStatus
            // 
            this.toolStripStatusLabel_mqttStatus.Name = "toolStripStatusLabel_mqttStatus";
            this.toolStripStatusLabel_mqttStatus.Size = new System.Drawing.Size(80, 20);
            this.toolStripStatusLabel_mqttStatus.Text = "MQTT: 已连接";
            //
            // lblViewMode
            //
            this.lblViewMode.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold);
            this.lblViewMode.ForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            this.lblViewMode.Margin = new System.Windows.Forms.Padding(5, 0, 10, 0);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(100, 20);
            this.lblViewMode.Text = "模式: 操作员";
            this.lblViewMode.Click += new System.EventHandler(this.lblViewMode_Click);
            // 
            // toolStripStatusLabel_systemInfo
            // 
            this.toolStripStatusLabel_systemInfo.Name = "toolStripStatusLabel_systemInfo";
            this.toolStripStatusLabel_systemInfo.Size = new System.Drawing.Size(200, 20);
            this.toolStripStatusLabel_systemInfo.Text = "12:00:00 | 今日产量: 0 | 良率: 0%";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(55)))));
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.辅助功能ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1000, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.监控界面ToolStripMenuItem,
            this.数据报表ToolStripMenuItem,
            this.配方管理ToolStripMenuItem,
            this.设备控制ToolStripMenuItem,
            this.工位管理ToolStripMenuItem,
            this.系统设置ToolStripMenuItem});
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.White;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(68, 24);
            this.toolStripMenuItem1.Text = "主界面";
            // 
            // 监控界面ToolStripMenuItem
            // 
            this.监控界面ToolStripMenuItem.Name = "监控界面ToolStripMenuItem";
            this.监控界面ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.监控界面ToolStripMenuItem.Text = "监控界面";
            this.监控界面ToolStripMenuItem.Click += new System.EventHandler(this.监控界面ToolStripMenuItem_Click);
            // 
            // 数据报表ToolStripMenuItem
            // 
            this.数据报表ToolStripMenuItem.Name = "数据报表ToolStripMenuItem";
            this.数据报表ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.数据报表ToolStripMenuItem.Text = "数据报表";
            this.数据报表ToolStripMenuItem.Click += new System.EventHandler(this.数据报表ToolStripMenuItem_Click);
            // 
            // 配方管理ToolStripMenuItem
            // 
            this.配方管理ToolStripMenuItem.Name = "配方管理ToolStripMenuItem";
            this.配方管理ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.配方管理ToolStripMenuItem.Text = "配方管理";
            this.配方管理ToolStripMenuItem.Click += new System.EventHandler(this.配方管理ToolStripMenuItem_Click);
            // 
            // 设备控制ToolStripMenuItem
            // 
            this.设备控制ToolStripMenuItem.Name = "设备控制ToolStripMenuItem";
            this.设备控制ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.设备控制ToolStripMenuItem.Text = "设备控制";
            this.设备控制ToolStripMenuItem.Click += new System.EventHandler(this.设备控制ToolStripMenuItem_Click);
            // 
            // 工位管理ToolStripMenuItem
            // 
            this.工位管理ToolStripMenuItem.Name = "工位管理ToolStripMenuItem";
            this.工位管理ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.工位管理ToolStripMenuItem.Text = "工位管理";
            this.工位管理ToolStripMenuItem.Click += new System.EventHandler(this.工位管理ToolStripMenuItem_Click);
            // 
            // 系统设置ToolStripMenuItem
            // 
            this.系统设置ToolStripMenuItem.Name = "系统设置ToolStripMenuItem";
            this.系统设置ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.系统设置ToolStripMenuItem.Text = "系统设置";
            this.系统设置ToolStripMenuItem.Click += new System.EventHandler(this.系统设置ToolStripMenuItem_Click);
            // 
            // 辅助功能ToolStripMenuItem
            // 
            this.辅助功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.用户管理ToolStripMenuItem,
            this.系统日志ToolStripMenuItem,
            this.关于系统ToolStripMenuItem});
            this.辅助功能ToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.辅助功能ToolStripMenuItem.Name = "辅助功能ToolStripMenuItem";
            this.辅助功能ToolStripMenuItem.Size = new System.Drawing.Size(82, 24);
            this.辅助功能ToolStripMenuItem.Text = "辅助功能";
            // 
            // 用户管理ToolStripMenuItem
            // 
            this.用户管理ToolStripMenuItem.Name = "用户管理ToolStripMenuItem";
            this.用户管理ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.用户管理ToolStripMenuItem.Text = "用户管理";
            this.用户管理ToolStripMenuItem.Click += new System.EventHandler(this.用户管理ToolStripMenuItem_Click);
            //
            // 系统日志ToolStripMenuItem
            //
            this.系统日志ToolStripMenuItem.Name = "系统日志ToolStripMenuItem";
            this.系统日志ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.系统日志ToolStripMenuItem.Text = "系统日志";
            this.系统日志ToolStripMenuItem.Click += new System.EventHandler(this.系统日志ToolStripMenuItem_Click);
            //
            // 关于系统ToolStripMenuItem
            //
            this.关于系统ToolStripMenuItem.Name = "关于系统ToolStripMenuItem";
            this.关于系统ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.关于系统ToolStripMenuItem.Text = "关于系统";
            this.关于系统ToolStripMenuItem.Click += new System.EventHandler(this.关于系统ToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel_Main);
            this.splitContainer1.Size = new System.Drawing.Size(1000, 702);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 2;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(200, 702);
            this.treeView1.TabIndex = 0;
            // 
            // panel_Main
            // 
            this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Main.Location = new System.Drawing.Point(0, 0);
            this.panel_Main.Name = "panel_Main";
            this.panel_Main.Size = new System.Drawing.Size(796, 702);
            this.panel_Main.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 755);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.IsMdiContainer = true;
            this.MinimumSize = new System.Drawing.Size(800, 604);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动化控制系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_userRole;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_stationStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_tcpStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_opcStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_mqttStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_systemInfo;
        private System.Windows.Forms.ToolStripStatusLabel lblViewMode;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 监控界面ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 数据报表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 配方管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 设备控制ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工位管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 系统设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 辅助功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 用户管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 系统日志ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于系统ToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Panel panel_Main;
    }
}
