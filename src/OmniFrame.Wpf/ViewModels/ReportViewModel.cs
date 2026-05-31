using System;
using System.Data;
using System.Windows.Input;
using OmniFrame.Core;

namespace OmniFrame.Wpf.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly IReportManager _reportMgr;
        private DataTable _reportData;
        public DataTable ReportData { get => _reportData; set => Set(ref _reportData, value); }

        private bool _isDaily = true;
        public bool IsDaily { get => _isDaily; set { if (Set(ref _isDaily, value) && value) { _isWeekly = false; _isMonthly = false; OnPropertyChanged(nameof(IsWeekly)); OnPropertyChanged(nameof(IsMonthly)); GenerateReport(); } } }
        private bool _isWeekly;
        public bool IsWeekly { get => _isWeekly; set { if (Set(ref _isWeekly, value) && value) { _isDaily = false; _isMonthly = false; OnPropertyChanged(nameof(IsDaily)); OnPropertyChanged(nameof(IsMonthly)); GenerateReport(); } } }
        private bool _isMonthly;
        public bool IsMonthly { get => _isMonthly; set { if (Set(ref _isMonthly, value) && value) { _isDaily = false; _isWeekly = false; OnPropertyChanged(nameof(IsDaily)); OnPropertyChanged(nameof(IsWeekly)); GenerateReport(); } } }

        public ReportViewModel(IReportManager reportMgr)
        {
            _reportMgr = reportMgr;
            GenerateReport();
        }

        private void GenerateReport()
        {
            var today = DateTime.Today;
            ReportData = IsDaily ? _reportMgr.GenerateDailyReport(today)
                       : IsWeekly ? _reportMgr.GenerateWeeklyReport(today)
                       : _reportMgr.GenerateMonthlyReport(today);
        }
    }
}
