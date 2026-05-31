using System;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 报表中心窗体类 - 生产数据查询与报表生成
        /// </summary>
    public partial class ReportCenterForm : Form
    {
        private ISystemManager _systemManager;
        private IReportManager _reportManager;

        public ReportCenterForm(ISystemManager systemManager, IReportManager reportManager)
        {
            InitializeComponent();
            _systemManager = systemManager;
            _reportManager = reportManager;

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        private void ReportCenterForm_Load(object sender, EventArgs e)
        {
            // 设置默认日期
            dateTimePickerStart.Value = DateTime.Now.AddDays(-7);
            dateTimePickerEnd.Value = DateTime.Now;
            dateTimePickerStart_Statistics.Value = DateTime.Now.Date;
            dateTimePickerEnd_Statistics.Value = DateTime.Now;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            QueryData();
        }

        private void QueryData()
        {
            if (_systemManager.DataMgr == null) return;

            listViewData.Items.Clear();

            var records = _systemManager.DataMgr.QueryProducts(
                textBoxSerialNumber.Text.Trim(),
                dateTimePickerStart.Value,
                dateTimePickerEnd.Value,
                null
            );

            foreach (var record in records)
            {
                var item = new ListViewItem(new[]
                {
                    record.ProductTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    record.SerialNumber,
                    record.ProductModel,
                    record.Result ? "OK" : "NG",
                    record.DefectCode,
                    $"{record.CycleTime:F2}",
                    record.Operator
                });

                if (!record.Result)
                {
                    item.ForeColor = System.Drawing.Color.Red;
                }

                listViewData.Items.Add(item);
            }

            lblRecordCount.Text = $"记录数: {records.Count}";
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV文件|*.csv";
                dialog.FileName = $"ProductionData_{DateTime.Now:yyyyMMddHHmmss}.csv";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (_systemManager.DataMgr.ExportToCsv(dialog.FileName, dateTimePickerStart.Value, dateTimePickerEnd.Value))
                    {
                        MessageBox.Show("导出成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("导出失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startDate = dateTimePickerStart_Statistics.Value;
                DateTime endDate = dateTimePickerEnd_Statistics.Value;
                System.Data.DataTable dt = null;

                if (radioButtonDaily.Checked)
                {
                    dt = _reportManager.GenerateDailyReport(startDate);
                }
                else if (radioButtonWeekly.Checked)
                {
                    dt = _reportManager.GenerateWeeklyReport(startDate);
                }
                else if (radioButtonMonthly.Checked)
                {
                    dt = _reportManager.GenerateMonthlyReport(startDate);
                }
                else if (radioButtonCustom.Checked)
                {
                    dt = _reportManager.GenerateProductionReport(startDate, endDate);
                }

                if (dt != null)
                {
                    dataGridViewReport.DataSource = dt;
                    labelRecordCount.Text = $"记录数: {dt.Rows.Count}";
                }
                else
                {
                    MessageBox.Show("报表生成失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"报表生成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewReport.DataSource != null)
                {
                    System.Data.DataTable dt = (System.Data.DataTable)dataGridViewReport.DataSource;
                    string fileName = $"ProductionReport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                    bool success = _reportManager.ExportReportToExcel(dt, fileName);
                    if (success)
                    {
                        MessageBox.Show("报表导出成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("报表导出失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("请先生成报表", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"报表导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            Logger.Info("PDF导出请求");
        }

        private void radioButtonCustom_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerEnd_Statistics.Enabled = radioButtonCustom.Checked;
        }
    }
}
