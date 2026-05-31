using System.Collections.ObjectModel;
using OmniFrame.Core;
namespace OmniFrame.Wpf.ViewModels
{
    public class StationViewModel : ViewModelBase
    {
        private readonly IStationManager _stationMgr;
        public ObservableCollection<StationDisplayItem> Stations { get; } = new();
        private string _stateText;
        public string StateText { get => _stateText; set => Set(ref _stateText, value); }
        public StationViewModel(IStationManager stationMgr) { _stationMgr = stationMgr; Load(); }
        private void Load()
        {
            StateText = _stationMgr.IsAutoRunning ? "运行中" : _stationMgr.IsPause ? "暂停" : _stationMgr.IsError ? "错误" : "空闲";
            Stations.Clear();
            foreach (var s in _stationMgr.GetAllStations())
                Stations.Add(new StationDisplayItem { Name = s.Name, State = s.State.ToString() });
        }
    }
    public class StationDisplayItem { public string Name { get; set; } public string State { get; set; } }
}
