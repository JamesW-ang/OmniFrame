using OmniFrame.Theme;

namespace OmniFrame
{
    partial class ReportCenterForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_DataQuery = new System.Windows.Forms.TabPage();
            this.lblRecordCount = new System.Windows.Forms.Label();
            this.listViewData = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxSerialNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnExportData = new System.Windows.Forms.Button();
            this.tabPage_Statistics = new System.Windows.Forms.TabPage();
            this.labelRecordCount = new System.Windows.Forms.Label();
            this.dataGridViewReport = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButtonDaily = new System.Windows.Forms.RadioButton();
            this.radioButtonWeekly = new System.Windows.Forms.RadioButton();
            this.radioButtonMonthly = new System.Windows.Forms.RadioButton();
            this.radioButtonCustom = new System.Windows.Forms.RadioButton();
            this.dateTimePickerStart_Statistics = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.dateTimePickerEnd_Statistics = new System.Windows.Forms.DateTimePicker();
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnExportPDF = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_DataQuery.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabPage_Statistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReport)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_DataQuery);
            this.tabControl1.Controls.Add(this.tabPage_Statistics);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1218, 808);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_DataQuery
            // 
            this.tabPage_DataQuery.Controls.Add(this.lblRecordCount);
            this.tabPage_DataQuery.Controls.Add(this.listViewData);
            this.tabPage_DataQuery.Controls.Add(this.flowLayoutPanel1);
            this.tabPage_DataQuery.Location = new System.Drawing.Point(8, 50);
            this.tabPage_DataQuery.Name = "tabPage_DataQuery";
            this.tabPage_DataQuery.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_DataQuery.Size = new System.Drawing.Size(1202, 750);
            this.tabPage_DataQuery.TabIndex = 0;
            this.tabPage_DataQuery.Text = "数据查询";
            this.tabPage_DataQuery.UseVisualStyleBackColor = true;
            // 
            // lblRecordCount
            // 
            this.lblRecordCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRecordCount.AutoSize = true;
            this.lblRecordCount.Location = new System.Drawing.Point(6, 773);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(130, 36);
            this.lblRecordCount.TabIndex = 8;
            this.lblRecordCount.Text = "记录数: 0";
            // 
            // listViewData
            // 
            this.listViewData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewData.FullRowSelect = true;
            this.listViewData.GridLines = true;
            this.listViewData.HideSelection = false;
            this.listViewData.Location = new System.Drawing.Point(3, 77);
            this.listViewData.Name = "listViewData";
            this.listViewData.Size = new System.Drawing.Size(1196, 670);
            this.listViewData.TabIndex = 1;
            this.listViewData.UseCompatibleStateImageBehavior = false;
            this.listViewData.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "时间";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "序列号";
            this.columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "产品型号";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "结果";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "缺陷代码";
            this.columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "周期时间";
            this.columnHeader6.Width = 80;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "操作员";
            this.columnHeader7.Width = 100;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.textBoxSerialNumber);
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.dateTimePickerStart);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.dateTimePickerEnd);
            this.flowLayoutPanel1.Controls.Add(this.btnQuery);
            this.flowLayoutPanel1.Controls.Add(this.btnExportData);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1196, 74);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 36);
            this.label3.TabIndex = 1;
            this.label3.Text = "序列号：";
            // 
            // textBoxSerialNumber
            // 
            this.textBoxSerialNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxSerialNumber.Location = new System.Drawing.Point(136, 3);
            this.textBoxSerialNumber.Name = "textBoxSerialNumber";
            this.textBoxSerialNumber.Size = new System.Drawing.Size(200, 44);
            this.textBoxSerialNumber.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(342, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 36);
            this.label1.TabIndex = 3;
            this.label1.Text = "时间范围：";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateTimePickerStart.Location = new System.Drawing.Point(503, 3);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerStart.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(659, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 36);
            this.label2.TabIndex = 4;
            this.label2.Text = "至：";
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(736, 3);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerEnd.TabIndex = 6;
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnQuery.Location = new System.Drawing.Point(892, 8);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(75, 33);
            this.btnQuery.TabIndex = 7;
            this.btnQuery.Text = "查询";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnExportData
            // 
            this.btnExportData.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnExportData.Location = new System.Drawing.Point(973, 8);
            this.btnExportData.Name = "btnExportData";
            this.btnExportData.Size = new System.Drawing.Size(75, 34);
            this.btnExportData.TabIndex = 9;
            this.btnExportData.Text = "导出";
            this.btnExportData.UseVisualStyleBackColor = true;
            this.btnExportData.Click += new System.EventHandler(this.btnExportData_Click);
            // 
            // tabPage_Statistics
            // 
            this.tabPage_Statistics.Controls.Add(this.labelRecordCount);
            this.tabPage_Statistics.Controls.Add(this.dataGridViewReport);
            this.tabPage_Statistics.Controls.Add(this.flowLayoutPanel2);
            this.tabPage_Statistics.Location = new System.Drawing.Point(8, 50);
            this.tabPage_Statistics.Name = "tabPage_Statistics";
            this.tabPage_Statistics.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Statistics.Size = new System.Drawing.Size(784, 542);
            this.tabPage_Statistics.TabIndex = 1;
            this.tabPage_Statistics.Text = "统计报表";
            this.tabPage_Statistics.UseVisualStyleBackColor = true;
            // 
            // labelRecordCount
            // 
            this.labelRecordCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelRecordCount.AutoSize = true;
            this.labelRecordCount.Location = new System.Drawing.Point(6, 565);
            this.labelRecordCount.Name = "labelRecordCount";
            this.labelRecordCount.Size = new System.Drawing.Size(130, 36);
            this.labelRecordCount.TabIndex = 10;
            this.labelRecordCount.Text = "记录数: 0";
            // 
            // dataGridViewReport
            // 
            this.dataGridViewReport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewReport.Location = new System.Drawing.Point(3, 77);
            this.dataGridViewReport.Name = "dataGridViewReport";
            this.dataGridViewReport.RowHeadersWidth = 82;
            this.dataGridViewReport.Size = new System.Drawing.Size(778, 462);
            this.dataGridViewReport.TabIndex = 9;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label4);
            this.flowLayoutPanel2.Controls.Add(this.radioButtonDaily);
            this.flowLayoutPanel2.Controls.Add(this.radioButtonWeekly);
            this.flowLayoutPanel2.Controls.Add(this.radioButtonMonthly);
            this.flowLayoutPanel2.Controls.Add(this.radioButtonCustom);
            this.flowLayoutPanel2.Controls.Add(this.dateTimePickerStart_Statistics);
            this.flowLayoutPanel2.Controls.Add(this.label5);
            this.flowLayoutPanel2.Controls.Add(this.dateTimePickerEnd_Statistics);
            this.flowLayoutPanel2.Controls.Add(this.btnGenerateReport);
            this.flowLayoutPanel2.Controls.Add(this.btnExportExcel);
            this.flowLayoutPanel2.Controls.Add(this.btnExportPDF);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(778, 74);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 36);
            this.label4.TabIndex = 0;
            this.label4.Text = "统计日期：";
            // 
            // radioButtonDaily
            // 
            this.radioButtonDaily.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonDaily.AutoSize = true;
            this.radioButtonDaily.Checked = true;
            this.radioButtonDaily.Location = new System.Drawing.Point(164, 5);
            this.radioButtonDaily.Name = "radioButtonDaily";
            this.radioButtonDaily.Size = new System.Drawing.Size(102, 40);
            this.radioButtonDaily.TabIndex = 4;
            this.radioButtonDaily.TabStop = true;
            this.radioButtonDaily.Text = "日度";
            this.radioButtonDaily.UseVisualStyleBackColor = true;
            // 
            // radioButtonWeekly
            // 
            this.radioButtonWeekly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonWeekly.AutoSize = true;
            this.radioButtonWeekly.Location = new System.Drawing.Point(272, 5);
            this.radioButtonWeekly.Name = "radioButtonWeekly";
            this.radioButtonWeekly.Size = new System.Drawing.Size(102, 40);
            this.radioButtonWeekly.TabIndex = 5;
            this.radioButtonWeekly.Text = "周度";
            this.radioButtonWeekly.UseVisualStyleBackColor = true;
            // 
            // radioButtonMonthly
            // 
            this.radioButtonMonthly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonMonthly.AutoSize = true;
            this.radioButtonMonthly.Location = new System.Drawing.Point(380, 5);
            this.radioButtonMonthly.Name = "radioButtonMonthly";
            this.radioButtonMonthly.Size = new System.Drawing.Size(102, 40);
            this.radioButtonMonthly.TabIndex = 6;
            this.radioButtonMonthly.Text = "月度";
            this.radioButtonMonthly.UseVisualStyleBackColor = true;
            // 
            // radioButtonCustom
            // 
            this.radioButtonCustom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonCustom.AutoSize = true;
            this.radioButtonCustom.Location = new System.Drawing.Point(488, 5);
            this.radioButtonCustom.Name = "radioButtonCustom";
            this.radioButtonCustom.Size = new System.Drawing.Size(130, 40);
            this.radioButtonCustom.TabIndex = 7;
            this.radioButtonCustom.Text = "自定义";
            this.radioButtonCustom.UseVisualStyleBackColor = true;
            this.radioButtonCustom.CheckedChanged += new System.EventHandler(this.radioButtonCustom_CheckedChanged);
            // 
            // dateTimePickerStart_Statistics
            // 
            this.dateTimePickerStart_Statistics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateTimePickerStart_Statistics.Location = new System.Drawing.Point(624, 3);
            this.dateTimePickerStart_Statistics.Name = "dateTimePickerStart_Statistics";
            this.dateTimePickerStart_Statistics.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerStart_Statistics.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 36);
            this.label5.TabIndex = 1;
            this.label5.Text = "至：";
            // 
            // dateTimePickerEnd_Statistics
            // 
            this.dateTimePickerEnd_Statistics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateTimePickerEnd_Statistics.Enabled = false;
            this.dateTimePickerEnd_Statistics.Location = new System.Drawing.Point(80, 53);
            this.dateTimePickerEnd_Statistics.Name = "dateTimePickerEnd_Statistics";
            this.dateTimePickerEnd_Statistics.Size = new System.Drawing.Size(150, 44);
            this.dateTimePickerEnd_Statistics.TabIndex = 3;
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnGenerateReport.Location = new System.Drawing.Point(236, 62);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(75, 25);
            this.btnGenerateReport.TabIndex = 8;
            this.btnGenerateReport.Text = "生成报表";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnExportExcel.Location = new System.Drawing.Point(317, 58);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(75, 33);
            this.btnExportExcel.TabIndex = 11;
            this.btnExportExcel.Text = "导出Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnExportPDF
            // 
            this.btnExportPDF.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnExportPDF.Location = new System.Drawing.Point(398, 58);
            this.btnExportPDF.Name = "btnExportPDF";
            this.btnExportPDF.Size = new System.Drawing.Size(75, 33);
            this.btnExportPDF.TabIndex = 12;
            this.btnExportPDF.Text = "导出PDF";
            this.btnExportPDF.UseVisualStyleBackColor = true;
            this.btnExportPDF.Click += new System.EventHandler(this.btnExportPDF_Click);
            // 
            // ReportCenterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1218, 808);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "ReportCenterForm";
            this.Text = "数据报表中心";
            this.Load += new System.EventHandler(this.ReportCenterForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_DataQuery.ResumeLayout(false);
            this.tabPage_DataQuery.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabPage_Statistics.ResumeLayout(false);
            this.tabPage_Statistics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReport)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_DataQuery;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxSerialNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnExportData;
        private System.Windows.Forms.ListView listViewData;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Label lblRecordCount;
        private System.Windows.Forms.TabPage tabPage_Statistics;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButtonDaily;
        private System.Windows.Forms.RadioButton radioButtonWeekly;
        private System.Windows.Forms.RadioButton radioButtonMonthly;
        private System.Windows.Forms.RadioButton radioButtonCustom;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart_Statistics;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd_Statistics;
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportPDF;
        private System.Windows.Forms.DataGridView dataGridViewReport;
        private System.Windows.Forms.Label labelRecordCount;
    }
}
