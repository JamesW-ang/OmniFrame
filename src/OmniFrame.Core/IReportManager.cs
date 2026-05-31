using System;
using System.Data;

namespace OmniFrame.Core
{
    /// <summary>
    /// 报表管理器接口
    /// </summary>
    public interface IReportManager
    {
        DataTable GenerateProductionReport(DateTime startDate, DateTime endDate);
        DataTable GenerateEquipmentStatusReport(DateTime startDate, DateTime endDate);
        DataTable GenerateAlarmReport(DateTime startDate, DateTime endDate);
        bool ExportReportToExcel(DataTable dt, string fileName);
        DataTable GenerateDailyReport(DateTime date);
        DataTable GenerateWeeklyReport(DateTime date);
        DataTable GenerateMonthlyReport(DateTime date);
    }
}
