using System.Collections.ObjectModel;
using OmniFrame.Core;
namespace OmniFrame.Wpf.ViewModels
{
    public class EquipmentViewModel : ViewModelBase
    {
        private readonly IDeviceManager _deviceMgr;
        public ObservableCollection<EquipmentItem> Devices { get; } = new();
        private bool _isReady;
        public bool IsReady { get => _isReady; set => Set(ref _isReady, value); }
        public EquipmentViewModel(IDeviceManager deviceMgr) { _deviceMgr = deviceMgr; Load(); }
        private void Load()
        {
            IsReady = _deviceMgr.IsReady;
            Devices.Clear();
            foreach (var n in _deviceMgr.GetMotionDeviceNames())
                Devices.Add(new EquipmentItem { Name = n, Type = "运动控制" });
            foreach (var n in _deviceMgr.GetPlcDeviceNames())
                Devices.Add(new EquipmentItem { Name = n, Type = "PLC" });
        }
    }
    public class EquipmentItem { public string Name { get; set; } public string Type { get; set; } }
}
