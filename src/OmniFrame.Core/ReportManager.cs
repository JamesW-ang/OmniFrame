using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 报表管理器
    /// 设计介绍：
    /// 3. 管理各种报表的生成，包括生产报表、设备状态报表、报警报表
    /// 4. 支持按时间范围生成报表，包括日报表、周报表、月报表
    /// 5. 支持报表导出为Excel格式
    /// 6. 使用SQLite数据库存储和查询报表数据
    /// 7. 集成日志系统，记录报表生成和导出的执行情况
    /// 8. 提供完善的异常处理机制
        /// </summary>
    public class ReportManager : IReportManager
    {
        private SqliteHelper _sqliteHelper;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportManager()
        {
            _sqliteHelper = new SqliteHelper(Path.Combine(AppContext.BaseDirectory, "Data", "OmniFrame.db"));
        }

        /// <summary>
        /// 生成生产报表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>生产报表数据</returns>
        public DataTable GenerateProductionReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 验证日期范围
                if (!ValidateDateRange(startDate, endDate))
                {
                    Logger.Warning("日期范围无效");
                    return null;
                }

                string sql = "SELECT * FROM ProductionData WHERE ProductionTime BETWEEN @StartDate AND @EndDate";
                DataTable dt = _sqliteHelper.QueryDataTable(sql, new { StartDate = startDate, EndDate = endDate });
                Logger.Info("生产报表生成成功");
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Error("生产报表生成失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 生成设备状态报表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>设备状态报表数据</returns>
        public DataTable GenerateEquipmentStatusReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 验证日期范围
                if (!ValidateDateRange(startDate, endDate))
                {
                    Logger.Warning("日期范围无效");
                    return null;
                }

                string sql = "SELECT * FROM EquipmentStatus WHERE StatusTime BETWEEN @StartDate AND @EndDate";
                DataTable dt = _sqliteHelper.QueryDataTable(sql, new { StartDate = startDate, EndDate = endDate });
                Logger.Info("设备状态报表生成成功");
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Error("设备状态报表生成失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 生成报警报表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>报警报表数据</returns>
        public DataTable GenerateAlarmReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 验证日期范围
                if (!ValidateDateRange(startDate, endDate))
                {
                    Logger.Warning("日期范围无效");
                    return null;
                }

                string sql = "SELECT * FROM AlarmData WHERE AlarmTime BETWEEN @StartDate AND @EndDate";
                DataTable dt = _sqliteHelper.QueryDataTable(sql, new { StartDate = startDate, EndDate = endDate });
                Logger.Info("报警报表生成成功");
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Error("报警报表生成失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 导出报表为Excel (XLSX)
        /// </summary>
        /// <param name="dt">报表数据</param>
        /// <param name="fileName">文件名</param>
        /// <returns>是否成功</returns>
        public bool ExportReportToExcel(DataTable dt, string fileName)
        {
            try
            {
                if (dt == null || dt.Columns.Count == 0)
                {
                    Logger.Warning("导出报表失败: DataTable为空");
                    return false;
                }

                // Ensure .xlsx extension
                if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = Path.ChangeExtension(fileName, ".xlsx");
                }

                string filePath = Path.Combine(AppContext.BaseDirectory, "Reports", fileName);
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    ExportToCsv(dt, filePath);
                }
                else
                {
                    ExportToXlsx(dt, filePath);
                }

                Logger.Info($"报表导出成功: {filePath} ({dt.Rows.Count} 行)");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("报表导出失败", ex);
                // Fallback: try CSV export
                try
                {
                    string fallbackPath = Path.ChangeExtension(
                        Path.Combine(AppContext.BaseDirectory, "Reports", fileName), ".csv");
                    string fallbackDir = Path.GetDirectoryName(fallbackPath);
                    if (!Directory.Exists(fallbackDir))
                        Directory.CreateDirectory(fallbackDir);
                    ExportToCsv(dt, fallbackPath);
                    Logger.Info($"报表已导出为CSV备选文件: {fallbackPath}");
                    return true;
                }
                catch (Exception csvEx)
                {
                    Logger.Error("CSV备选导出也失败了", csvEx);
                    return false;
                }
            }
        }

        /// <summary>
        /// 导出为XLSX格式 (使用ClosedXML)
        /// </summary>
        private void ExportToXlsx(DataTable dt, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add(dt.TableName ?? "Report");

                // Write headers with styling
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    var cell = ws.Cell(1, col + 1);
                    cell.Value = dt.Columns[col].ColumnName;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Write data rows
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        object value = dt.Rows[row][col];
                        var cell = ws.Cell(row + 2, col + 1);

                        if (value == null || value == DBNull.Value)
                        {
                            cell.Value = "";
                        }
                        else if (value is DateTime dtVal)
                        {
                            cell.Value = dtVal;
                            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                        }
                        else if (value is double || value is float || value is decimal)
                        {
                            cell.Value = Convert.ToDouble(value);
                        }
                        else if (value is int || value is long)
                        {
                            cell.Value = Convert.ToInt64(value);
                        }
                        else
                        {
                            cell.Value = value.ToString();
                        }
                    }
                }

                // Auto-fit columns
                ws.Columns().AdjustToContents(1, dt.Rows.Count + 1);

                workbook.SaveAs(filePath);
            }
        }

        /// <summary>
        /// 导出为CSV格式（UTF-8 with BOM）
        /// </summary>
        private static void ExportToCsv(DataTable dt, string filePath)
        {
            var sb = new StringBuilder();

            // Write headers
            var headers = new List<string>();
            foreach (DataColumn col in dt.Columns)
            {
                headers.Add(EscapeCsvField(col.ColumnName));
            }
            sb.AppendLine(string.Join(",", headers));

            // Write data rows
            foreach (DataRow row in dt.Rows)
            {
                var fields = new List<string>();
                foreach (DataColumn col in dt.Columns)
                {
                    object value = row[col];
                    string strValue = value?.ToString() ?? "";
                    fields.Add(EscapeCsvField(strValue));
                }
                sb.AppendLine(string.Join(",", fields));
            }

            // Write UTF-8 with BOM for Excel compatibility
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// 对CSV字段进行转义处理
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            bool needsQuotes = field.Contains(",") || field.Contains("\"") ||
                               field.Contains("\n") || field.Contains("\r");

            if (needsQuotes)
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        /// <summary>
        /// 生成日报表
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>日报表数据</returns>
        public DataTable GenerateDailyReport(DateTime date)
        {
            DateTime startDate = date.Date;
            DateTime endDate = startDate.AddDays(1).AddSeconds(-1);
            return GenerateProductionReport(startDate, endDate);
        }

        /// <summary>
        /// 生成周报表
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>周报表数据</returns>
        public DataTable GenerateWeeklyReport(DateTime date)
        {
            int daysToSubtract = (int)date.DayOfWeek;
            if (daysToSubtract == 0) daysToSubtract = 7;
            DateTime startDate = date.Date.AddDays(-daysToSubtract + 1);
            DateTime endDate = startDate.AddDays(7).AddSeconds(-1);
            return GenerateProductionReport(startDate, endDate);
        }

        /// <summary>
        /// 生成月报表
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>月报表数据</returns>
        public DataTable GenerateMonthlyReport(DateTime date)
        {
            DateTime startDate = new DateTime(date.Year, date.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddSeconds(-1);
            return GenerateProductionReport(startDate, endDate);
        }

        /// <summary>
        /// 验证日期范围
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>日期范围是否有效</returns>
        private bool ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            // 开始日期不能晚于结束日期
            if (startDate > endDate)
                return false;

            // 日期范围不能超过1年
            if ((endDate - startDate).TotalDays > 366)
                return false;

            // 日期不能是未来日期
            if (startDate > DateTime.Now || endDate > DateTime.Now)
                return false;

            return true;
        }
    }
}
