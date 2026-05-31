using System.Collections.ObjectModel;
using OmniFrame.Core;
namespace OmniFrame.Wpf.ViewModels
{
    public class OperationLogViewModel : ViewModelBase
    {
        private readonly IAuditLogger _auditLogger;
        public ObservableCollection<LogEntry> Entries { get; } = new();
        public OperationLogViewModel(IAuditLogger auditLogger) { _auditLogger = auditLogger; Load(); }
        private void Load() { Entries.Add(new LogEntry { Time = "10:30:00", User = "admin", Action = "登录", Detail = "用户登录成功" }); Entries.Add(new LogEntry { Time = "10:35:00", User = "admin", Action = "启动", Detail = "系统启动" }); }
    }
    public class LogEntry { public string Time { get; set; } public string User { get; set; } public string Action { get; set; } public string Detail { get; set; } }
}
