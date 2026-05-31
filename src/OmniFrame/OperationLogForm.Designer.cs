namespace OmniFrame
{
    partial class OperationLogForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.flowLayoutPanelFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.labelTimeRange = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.labelTo = new System.Windows.Forms.Label();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.labelActionType = new System.Windows.Forms.Label();
            this.comboBoxActionType = new System.Windows.Forms.ComboBox();
            this.labelUser = new System.Windows.Forms.Label();
            this.comboBoxUser = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.dataGridViewLogs = new System.Windows.Forms.DataGridView();
            this.columnTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnActionType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.flowLayoutPanelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.labelPageInfo = new System.Windows.Forms.Label();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.flowLayoutPanelFilter.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLogs)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.flowLayoutPanelActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.flowLayoutPanelFilter);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1016, 60);
            this.panelTop.TabIndex = 0;
            // 
            // flowLayoutPanelFilter
            // 
            this.flowLayoutPanelFilter.Controls.Add(this.labelTimeRange);
            this.flowLayoutPanelFilter.Controls.Add(this.dateTimePickerStart);
            this.flowLayoutPanelFilter.Controls.Add(this.labelTo);
            this.flowLayoutPanelFilter.Controls.Add(this.dateTimePickerEnd);
            this.flowLayoutPanelFilter.Controls.Add(this.labelActionType);
            this.flowLayoutPanelFilter.Controls.Add(this.comboBoxActionType);
            this.flowLayoutPanelFilter.Controls.Add(this.labelUser);
            this.flowLayoutPanelFilter.Controls.Add(this.comboBoxUser);
            this.flowLayoutPanelFilter.Controls.Add(this.btnQuery);
            this.flowLayoutPanelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFilter.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelFilter.Name = "flowLayoutPanelFilter";
            this.flowLayoutPanelFilter.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutPanelFilter.Size = new System.Drawing.Size(1016, 60);
            this.flowLayoutPanelFilter.TabIndex = 0;
            // 
            // labelTimeRange
            // 
            this.labelTimeRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTimeRange.AutoSize = true;
            this.labelTimeRange.Location = new System.Drawing.Point(13, 17);
            this.labelTimeRange.Name = "labelTimeRange";
            this.labelTimeRange.Size = new System.Drawing.Size(127, 36);
            this.labelTimeRange.TabIndex = 0;
            this.labelTimeRange.Text = "时间范围";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(146, 13);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerStart.TabIndex = 1;
            // 
            // labelTo
            // 
            this.labelTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(302, 17);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(43, 36);
            this.labelTo.TabIndex = 2;
            this.labelTo.Text = "至";
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(351, 13);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerEnd.TabIndex = 3;
            // 
            // labelActionType
            // 
            this.labelActionType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelActionType.AutoSize = true;
            this.labelActionType.Location = new System.Drawing.Point(507, 17);
            this.labelActionType.Name = "labelActionType";
            this.labelActionType.Size = new System.Drawing.Size(127, 36);
            this.labelActionType.TabIndex = 4;
            this.labelActionType.Text = "操作类型";
            // 
            // comboBoxActionType
            // 
            this.comboBoxActionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxActionType.FormattingEnabled = true;
            this.comboBoxActionType.Items.AddRange(new object[] {
            "全部",
            "登录",
            "配方修改",
            "参数变更",
            "设备操作"});
            this.comboBoxActionType.Location = new System.Drawing.Point(640, 13);
            this.comboBoxActionType.Name = "comboBoxActionType";
            this.comboBoxActionType.Size = new System.Drawing.Size(120, 44);
            this.comboBoxActionType.TabIndex = 5;
            // 
            // labelUser
            // 
            this.labelUser.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(766, 17);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(127, 36);
            this.labelUser.TabIndex = 6;
            this.labelUser.Text = "操作用户";
            // 
            // comboBoxUser
            // 
            this.comboBoxUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUser.FormattingEnabled = true;
            this.comboBoxUser.Location = new System.Drawing.Point(13, 63);
            this.comboBoxUser.Name = "comboBoxUser";
            this.comboBoxUser.Size = new System.Drawing.Size(120, 44);
            this.comboBoxUser.TabIndex = 7;
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(139, 63);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(80, 28);
            this.btnQuery.TabIndex = 8;
            this.btnQuery.Text = "查询";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.dataGridViewLogs);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 60);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1016, 460);
            this.panelMain.TabIndex = 1;
            // 
            // dataGridViewLogs
            // 
            this.dataGridViewLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnTime,
            this.columnUser,
            this.columnActionType,
            this.columnDescription,
            this.columnResult});
            this.dataGridViewLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewLogs.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewLogs.Name = "dataGridViewLogs";
            this.dataGridViewLogs.ReadOnly = true;
            this.dataGridViewLogs.RowHeadersWidth = 82;
            this.dataGridViewLogs.RowTemplate.Height = 23;
            this.dataGridViewLogs.Size = new System.Drawing.Size(1016, 460);
            this.dataGridViewLogs.TabIndex = 0;
            // 
            // columnTime
            // 
            this.columnTime.HeaderText = "时间";
            this.columnTime.MinimumWidth = 10;
            this.columnTime.Name = "columnTime";
            this.columnTime.ReadOnly = true;
            this.columnTime.Width = 180;
            // 
            // columnUser
            // 
            this.columnUser.HeaderText = "用户";
            this.columnUser.MinimumWidth = 10;
            this.columnUser.Name = "columnUser";
            this.columnUser.ReadOnly = true;
            this.columnUser.Width = 120;
            // 
            // columnActionType
            // 
            this.columnActionType.HeaderText = "操作类型";
            this.columnActionType.MinimumWidth = 10;
            this.columnActionType.Name = "columnActionType";
            this.columnActionType.ReadOnly = true;
            this.columnActionType.Width = 120;
            // 
            // columnDescription
            // 
            this.columnDescription.HeaderText = "操作描述";
            this.columnDescription.MinimumWidth = 10;
            this.columnDescription.Name = "columnDescription";
            this.columnDescription.ReadOnly = true;
            this.columnDescription.Width = 400;
            // 
            // columnResult
            // 
            this.columnResult.HeaderText = "操作结果";
            this.columnResult.MinimumWidth = 10;
            this.columnResult.Name = "columnResult";
            this.columnResult.ReadOnly = true;
            this.columnResult.Width = 200;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.flowLayoutPanelActions);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 520);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1016, 40);
            this.panelBottom.TabIndex = 2;
            // 
            // flowLayoutPanelActions
            // 
            this.flowLayoutPanelActions.Controls.Add(this.btnExport);
            this.flowLayoutPanelActions.Controls.Add(this.btnClear);
            this.flowLayoutPanelActions.Controls.Add(this.labelPageInfo);
            this.flowLayoutPanelActions.Controls.Add(this.btnPrevPage);
            this.flowLayoutPanelActions.Controls.Add(this.btnNextPage);
            this.flowLayoutPanelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelActions.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelActions.Name = "flowLayoutPanelActions";
            this.flowLayoutPanelActions.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutPanelActions.Size = new System.Drawing.Size(1016, 40);
            this.flowLayoutPanelActions.TabIndex = 0;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(13, 13);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(80, 25);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(99, 13);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 25);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "清空日志";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // labelPageInfo
            // 
            this.labelPageInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelPageInfo.AutoSize = true;
            this.labelPageInfo.Location = new System.Drawing.Point(185, 10);
            this.labelPageInfo.Name = "labelPageInfo";
            this.labelPageInfo.Size = new System.Drawing.Size(219, 36);
            this.labelPageInfo.TabIndex = 2;
            this.labelPageInfo.Text = "第 1 页，共 1 页";
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnPrevPage.Location = new System.Drawing.Point(410, 15);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 25);
            this.btnPrevPage.TabIndex = 3;
            this.btnPrevPage.Text = "上一页";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnNextPage.Location = new System.Drawing.Point(491, 15);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 25);
            this.btnNextPage.TabIndex = 4;
            this.btnNextPage.Text = "下一页";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // OperationLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 560);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "OperationLogForm";
            this.Text = "操作日志审计";
            this.Load += new System.EventHandler(this.OperationLogForm_Load);
            this.panelTop.ResumeLayout(false);
            this.flowLayoutPanelFilter.ResumeLayout(false);
            this.flowLayoutPanelFilter.PerformLayout();
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLogs)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.flowLayoutPanelActions.ResumeLayout(false);
            this.flowLayoutPanelActions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFilter;
        private System.Windows.Forms.Label labelTimeRange;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.Label labelActionType;
        private System.Windows.Forms.ComboBox comboBoxActionType;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.ComboBox comboBoxUser;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.DataGridView dataGridViewLogs;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnActionType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnResult;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActions;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label labelPageInfo;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnNextPage;
    }
}
