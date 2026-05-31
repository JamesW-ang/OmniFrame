using System.Collections.ObjectModel;
using System.Data;
using OmniFrame.Core;


namespace OmniFrame.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigManager _configMgr;
        private readonly ISystemManager _systemMgr;
        private readonly IUserManager _userMgr;

        // Tab 选择
        private int _selectedTab;
        public int SelectedTab { get => _selectedTab; set { Set(ref _selectedTab, value); LoadTab(value); } }

        // 系统参数
        private string _sysWorkMode = "Auto"; public string SysWorkMode { get => _sysWorkMode; set => Set(ref _sysWorkMode, value); }
        private int _sysWatchdogInterval = 1000; public int SysWatchdogInterval { get => _sysWatchdogInterval; set => Set(ref _sysWatchdogInterval, value); }
        private bool _sysAutoReconnect = true;   public bool SysAutoReconnect { get => _sysAutoReconnect; set => Set(ref _sysAutoReconnect, value); }
        private int _sysHealthPort = 8082;        public int SysHealthPort { get => _sysHealthPort; set => Set(ref _sysHealthPort, value); }

        // 运动控制
        public ObservableCollection<MotionCardItem> MotionCards { get; } = new();
        public ObservableCollection<AxisParamItem> AxisParams { get; } = new();

        // PLC
        public ObservableCollection<PlcConfigItem> PlcConfigs { get; } = new();
        public ObservableCollection<RegisterMapItem> RegisterMaps { get; } = new();

        // IO
        public ObservableCollection<IoPointItem> InputPoints { get; } = new();
        public ObservableCollection<IoPointItem> OutputPoints { get; } = new();

        // 网络
        private string _netMqttBroker = "localhost"; public string NetMqttBroker { get => _netMqttBroker; set => Set(ref _netMqttBroker, value); }
        private int _netMqttPort = 1883;              public int NetMqttPort { get => _netMqttPort; set => Set(ref _netMqttPort, value); }
        private string _netMesUrl = "http://localhost:5000"; public string NetMesUrl { get => _netMesUrl; set => Set(ref _netMesUrl, value); }

        // 用户
        public ObservableCollection<UserDisplayItem> Users { get; } = new();

        public SettingsViewModel(IConfigManager configMgr, ISystemManager systemMgr, IUserManager userMgr)
        {
            _configMgr = configMgr;
            _systemMgr = systemMgr;
            _userMgr = userMgr;
            LoadAll();
        }

        private void LoadAll()
        {
            var sysCfg = _configMgr.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
            SysWorkMode = sysCfg.EnableAutoReconnect ? "Auto" : "Manual";
            SysWatchdogInterval = sysCfg.WatchdogInterval > 0 ? sysCfg.WatchdogInterval : 1000;
            SysAutoReconnect = sysCfg.EnableAutoReconnect;
            SysHealthPort = sysCfg.HealthPort > 0 ? sysCfg.HealthPort : 8082;

            LoadMotionDefaults();
            LoadPlcDefaults();
            LoadIoDefaults();
            LoadUsers();
        }

        private void LoadTab(int tab) { /* 按需加载 */ }
        private void LoadMotionDefaults() { for (int i = 0; i < 2; i++) { MotionCards.Add(new MotionCardItem { Index = i, Brand = "固高GTS", AxisCount = 4 }); AxisParams.Add(new AxisParamItem { AxisNo = i * 4 + 1, Name = $"轴{i * 4 + 1}", Velocity = 100, Acc = 500, Dec = 500 }); } }
        private void LoadPlcDefaults() { PlcConfigs.Add(new PlcConfigItem { Name = "主PLC", Type = "ModbusTCP", IP = "192.168.1.100", Port = 502 }); RegisterMaps.Add(new RegisterMapItem { Name = "急停", Element = "X", Address = 0 }); }
        private void LoadIoDefaults() { for (int i = 0; i < 8; i++) { InputPoints.Add(new IoPointItem { Index = i, Name = $"DI{i}", Port = 0, Pin = i }); OutputPoints.Add(new IoPointItem { Index = i, Name = $"DO{i}", Port = 0, Pin = i }); } }
        private void LoadUsers() { foreach (var u in _userMgr.GetAllUsers()) Users.Add(new UserDisplayItem { UserId = u.UserId, UserName = u.UserName, Level = u.Level.ToString(), IsActive = u.IsActive }); }
    }

    public class MotionCardItem   { public int Index { get; set; } public string Brand { get; set; } public int AxisCount { get; set; } }
    public class PlcConfigItem     { public string Name { get; set; } public string Type { get; set; } public string IP { get; set; } public int Port { get; set; } }
    public class RegisterMapItem   { public string Name { get; set; } public string Element { get; set; } public int Address { get; set; } }
    public class IoPointItem       { public int Index { get; set; } public string Name { get; set; } public int Port { get; set; } public int Pin { get; set; } }
    public class UserDisplayItem   { public string UserId { get; set; } public string UserName { get; set; } public string Level { get; set; } public bool IsActive { get; set; } }
}
