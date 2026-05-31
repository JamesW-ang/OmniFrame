using System.Collections.ObjectModel;
using System.Windows.Input;
using OmniFrame.Core.PluginSystem;
namespace OmniFrame.Wpf.ViewModels
{
    public class PluginViewModel : ViewModelBase
    {
        private readonly IPluginManager _pluginMgr;
        public ObservableCollection<PluginItem> Plugins { get; } = new();
        private PluginItem _selectedPlugin;
        public PluginItem SelectedPlugin { get => _selectedPlugin; set { Set(ref _selectedPlugin, value); ShowDetail(value); } }
        private string _detailName = "";     public string DetailName { get => _detailName; set => Set(ref _detailName, value); }
        private string _detailType = "";     public string DetailType { get => _detailType; set => Set(ref _detailType, value); }
        private string _detailVersion = "";  public string DetailVersion { get => _detailVersion; set => Set(ref _detailVersion, value); }
        private string _detailStatus = "";   public string DetailStatus { get => _detailStatus; set => Set(ref _detailStatus, value); }
        public ICommand LoadCommand { get; }  public ICommand UnloadCommand { get; }  public ICommand RefreshCommand { get; }
        // 运动测试
        private string _motionTestAxis = "1"; public string MotionTestAxis { get => _motionTestAxis; set => Set(ref _motionTestAxis, value); }
        private string _motionTestPos = "100"; public string MotionTestPos { get => _motionTestPos; set => Set(ref _motionTestPos, value); }
        public ICommand MotionConnectCmd { get; } public ICommand MotionMoveCmd { get; } public ICommand MotionHomeCmd { get; }
        // PLC 测试
        private string _plcTestAddr = "D100";  public string PlcTestAddr { get => _plcTestAddr; set => Set(ref _plcTestAddr, value); }
        private string _plcTestValue = "";     public string PlcTestValue { get => _plcTestValue; set => Set(ref _plcTestValue, value); }
        public ICommand PlcConnectCmd { get; } public ICommand PlcReadCmd { get; } public ICommand PlcWriteCmd { get; }

        public PluginViewModel(IPluginManager pluginMgr)
        {
            _pluginMgr = pluginMgr;
            LoadCommand = new RelayCommand(() => { });
            UnloadCommand = new RelayCommand(() => { });
            RefreshCommand = new RelayCommand(LoadPlugins);
            MotionConnectCmd = new RelayCommand(() => { });
            MotionMoveCmd = new RelayCommand(() => { });
            MotionHomeCmd = new RelayCommand(() => { });
            PlcConnectCmd = new RelayCommand(() => { });
            PlcReadCmd = new RelayCommand(() => { });
            PlcWriteCmd = new RelayCommand(() => { });
            LoadPlugins();
        }
        private void LoadPlugins()
        {
            Plugins.Clear();
            Plugins.Add(new PluginItem { Name = "MockBusinessPlugin", Version = "1.0", Type = "业务", Status = "已加载" });
            Plugins.Add(new PluginItem { Name = "MockMotionPlugin", Version = "1.0", Type = "运动", Status = "已加载" });
            Plugins.Add(new PluginItem { Name = "MockPlcPlugin", Version = "1.0", Type = "PLC", Status = "已加载" });
        }
        private void ShowDetail(PluginItem p)
        {
            if (p == null) return;
            DetailName = p.Name; DetailType = p.Type; DetailVersion = p.Version; DetailStatus = p.Status;
        }
    }
    public class PluginItem { public string Name { get; set; } public string Version { get; set; } public string Type { get; set; } public string Status { get; set; } }
}
