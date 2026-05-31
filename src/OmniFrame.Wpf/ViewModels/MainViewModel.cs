using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.Core.AdvancedFeatures;

namespace OmniFrame.Wpf.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel — 替代 MainForm 的所有 UI 逻辑。
    /// 管理左侧导航菜单和右侧内容区域切换。
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly ISystemManager _systemMgr;
        private readonly IUserManager _userMgr;
        private readonly IAlarmManager _alarmMgr;
        private readonly IUphManager _uphMgr;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<NavItem> NavItems { get; } = new();

        private NavItem _selectedNavItem;
        public NavItem SelectedNavItem
        {
            get => _selectedNavItem;
            set { if (Set(ref _selectedNavItem, value)) value?.Navigate(); }
        }

        private ViewModelBase _currentView;
        public ViewModelBase CurrentView { get => _currentView; set => Set(ref _currentView, value); }

        private string _statusText = "就绪";
        public string StatusText { get => _statusText; set => Set(ref _statusText, value); }

        private string _userInfo = "未登录";
        public string UserInfo { get => _userInfo; set => Set(ref _userInfo, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; set => Set(ref _isRunning, value); }

        private bool _hasAlarm;
        public bool HasAlarm { get => _hasAlarm; set => Set(ref _hasAlarm, value); }

        private int _activeAlarmCount;
        public int ActiveAlarmCount { get => _activeAlarmCount; set => Set(ref _activeAlarmCount, value); }

        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
        public string CurrentTime { get => _currentTime; set => Set(ref _currentTime, value); }

        public MainViewModel(ISystemManager systemMgr, IUserManager userMgr,
            IAlarmManager alarmMgr, IUphManager uphMgr, IServiceProvider serviceProvider)
        {
            _systemMgr = systemMgr;
            _userMgr = userMgr;
            _alarmMgr = alarmMgr;
            _uphMgr = uphMgr;
            _serviceProvider = serviceProvider;

            BuildNavigation();
            UpdateUserInfo();
        }

        private void BuildNavigation()
        {
            NavItems.Add(new NavItem("📊 BlockCut 生产",     () => NavigateTo<BlockCutViewModel>()));
            NavItems.Add(new NavItem("🏭 生产控制",         () => NavigateTo<BlockCutProductionViewModel>()));
            NavItems.Add(new NavItem("📊 生产统计",         () => NavigateTo<BlockCutStatsViewModel>()));
            NavItems.Add(new NavItem("📋 数据报表",         () => NavigateTo<ReportViewModel>()));
            NavItems.Add(new NavItem("📝 配方管理",         () => NavigateTo<RecipeViewModel>()));
            NavItems.Add(new NavItem("⚙️ 设备控制",         () => NavigateTo<EquipmentViewModel>()));
            NavItems.Add(new NavItem("🏭 工站管理",         () => NavigateTo<StationViewModel>()));
            NavItems.Add(new NavItem("📈 OEE 管理",         () => NavigateTo<OeeViewModel>()));
            NavItems.Add(new NavItem("🔧 系统设置 (6 Tab)", () => NavigateTo<SettingsViewModel>()));
            NavItems.Add(new NavItem("🧩 插件管理",         () => NavigateTo<PluginViewModel>()));
            NavItems.Add(new NavItem("📜 操作日志",         () => NavigateTo<OperationLogViewModel>()));
            NavItems.Add(new NavItem("📷 相机调试",         () => NavigateTo<CameraDebugViewModel>()));
            NavItems.Add(new NavItem("⚙️ 运动轴设置",       () => NavigateTo<MotionSetViewModel>()));
            NavItems.Add(new NavItem("📐 直线拟合工具",     () => NavigateTo<FitLineToolViewModel>()));
            NavItems.Add(new NavItem("📋 工单管理",         () => NavigateTo<WorkManagementViewModel>()));
            NavItems.Add(new NavItem("🔩 轴参数配置",       () => NavigateTo<AxisSetupViewModel>()));
            NavItems.Add(new NavItem("🔬 测量测试",         () => NavigateTo<MeasureTestViewModel>()));
        }

        private void NavigateTo<T>() where T : ViewModelBase
        {
            var vm = (ViewModelBase)_serviceProvider.GetService(typeof(T));
            if (vm != null) CurrentView = vm;
        }

        public void UpdateUserInfo()
        {
            var user = _userMgr?.CurrentUser;
            UserInfo = user != null
                ? $"用户: {user.UserName} | 角色: {user.Level}"
                : "用户: 未登录";

            var stateText = _systemMgr.State switch
            {
                SystemState.Running => "运行中",
                SystemState.Paused => "暂停",
                SystemState.Error => "错误",
                SystemState.EmergencyStop => "急停",
                _ => _systemMgr.IsRunning ? "运行中" : "待机"
            };
            StatusText = $"系统: {stateText}";
            IsRunning = _systemMgr.IsRunning;
            HasAlarm = _alarmMgr?.HasActiveAlarm ?? false;
            ActiveAlarmCount = _alarmMgr?.ActiveAlarmCount ?? 0;
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }
    }

    /// <summary>导航菜单项</summary>
    public class NavItem
    {
        public string Label { get; }
        private readonly Action _action;
        public NavItem(string label, Action navigate) { Label = label; _action = navigate; }
        public void Navigate() => _action?.Invoke();
    }

    /// <summary>临时占位 ViewModel (子页面开发中)</summary>
    public class PlaceholderViewModel : ViewModelBase
    {
        public string ViewName { get; }
        public PlaceholderViewModel(string name) { ViewName = name; }
    }
}
