using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Data;
using HelperDialog = OmniFrame.Theme.DialogHelper;
using OmniFrame.Theme;
using OmniFrame.Core;

namespace OmniFrame
{
    /// <summary>
    /// 操作日志窗体类 - 系统操作日志查询与管理
        /// </summary>
    public partial class OperationLogForm : Form
    {
        private List<OperationLog> logs = new List<OperationLog>();
        private int currentPage = 1;
        private const int pageSize = 100;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OperationLogForm()
        {
            InitializeComponent();

            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        /// <summary>
        /// 窗体加载事件 - 初始化查询条件
        /// </summary>
        private void OperationLogForm_Load(object sender, EventArgs e)
        {
            dateTimePickerStart.Value = DateTime.Now.AddMonths(-1);
            dateTimePickerEnd.Value = DateTime.Now;
            comboBoxActionType.SelectedIndex = 0;
            UpdateUserList();
            LoadLogs();
            UpdateLogGrid();
        }

        /// <summary>
        /// 加载日志数据Model层
        /// </summary>
        private void LoadLogs()
        {
            logs = OperationLogService.QueryLogs(
                dateTimePickerStart.Value,
                dateTimePickerEnd.Value,
                comboBoxActionType.SelectedItem.ToString(),
                comboBoxUser.SelectedItem.ToString()
            );
        }

        /// <summary>
        /// 更新用户列表
        /// </summary>
        private void UpdateUserList()
        {
            var users = new List<string> { "全部" };
            users.AddRange(new string[] { "admin", "operator", "engineer", "guest" });
            comboBoxUser.Items.Clear();
            foreach (var user in users)
            {
                comboBoxUser.Items.Add(user);
            }
            comboBoxUser.SelectedIndex = 0;
        }

        /// <summary>
        /// 更新日志表格
        /// </summary>
        private void UpdateLogGrid()
        {
            int totalPages = (logs.Count + pageSize - 1) / pageSize;
            if (currentPage > totalPages)
            {
                currentPage = totalPages > 0 ? totalPages : 1;
            }

            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, logs.Count);
            var pageLogs = logs.GetRange(startIndex, endIndex - startIndex);

            dataGridViewLogs.Rows.Clear();
            foreach (var log in pageLogs)
            {
                dataGridViewLogs.Rows.Add(
                    log.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.User,
                    log.ActionType,
                    log.Description,
                    log.Success ? "成功" : "失败"
                );
            }

            labelPageInfo.Text = $"第 {currentPage} 页，共 {totalPages} 页";
            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
        }

        /// <summary>
        /// 查询按钮
        /// </summary>
        private void btnQuery_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            LoadLogs();
            UpdateLogGrid();
        }

        /// <summary>
        /// 导出按钮+
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel文件 (*.xlsx)|*.xlsx|CSV文件 (*.csv)|*.csv";
                saveFileDialog.Title = "导出日志";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportLogs(saveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// 导出日志实现        /// 支持多种导出格式
        /// </summary>
        private void ExportLogs(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("时间,用户,操作类型,操作描述,操作结果");
                    foreach (var log in logs)
                    {
                        writer.WriteLine($"{log.Time.ToString("yyyy-MM-dd HH:mm:ss")},{log.User},{log.ActionType},{log.Description},{(log.Success ? "成功" : "失败")}");
                    }
                }
                HelperDialog.ShowSuccess("日志导出成功");
            }
            catch (Exception ex)
            {
                HelperDialog.ShowError($"导出失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空按钮
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (HelperDialog.Confirm("确认清空", "确定要清空所有操作日志吗？此操作不可恢复。"))
            {
                logs.Clear();
                UpdateLogGrid();
                HelperDialog.ShowSuccess("日志已清空");
            }
        }

        /// <summary>
        /// 上一页按钮
        /// </summary>
        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateLogGrid();
            }
        }

        /// <summary>
        /// 下一页按钮
        /// </summary>
        private void btnNextPage_Click(object sender, EventArgs e)
        {
            int totalPages = (logs.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateLogGrid();
            }
        }
    }
}
