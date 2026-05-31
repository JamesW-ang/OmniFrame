using System.Collections.ObjectModel;
using OmniFrame.Core.BlockCut;
namespace OmniFrame.Wpf.ViewModels
{
    public class BlockCutProductionViewModel : ViewModelBase
    {
        private readonly BlockCutConfig _cfg;
        public ObservableCollection<ModuleItem> Modules { get; } = new();
        private string _stationNo = "1";   public string StationNo { get => _stationNo; set => Set(ref _stationNo, value); }
        private string _machineNo = "M01";  public string MachineNo { get => _machineNo; set => Set(ref _machineNo, value); }
        private string _operatorName = "admin"; public string OperatorName { get => _operatorName; set => Set(ref _operatorName, value); }
        private string _materialNo = "MAT-001"; public string MaterialNo { get => _materialNo; set => Set(ref _materialNo, value); }
        private int _rowCount = 5;          public int RowCount { get => _rowCount; set => Set(ref _rowCount, value); }
        private int _colCount = 5;          public int ColCount { get => _colCount; set => Set(ref _colCount, value); }
        private string _elapsedTime = "00:00:00"; public string ElapsedTime { get => _elapsedTime; set => Set(ref _elapsedTime, value); }
        public BlockCutProductionViewModel(BlockCutConfig cfg) { _cfg = cfg; InitModules(); }
        private void InitModules() { for (int i = 0; i < 25; i++) Modules.Add(new ModuleItem { Row = i / 5, Col = i % 5, Status = "待加工" }); }
    }
    public class ModuleItem { public int Row { get; set; } public int Col { get; set; } public string Status { get; set; } }
}
