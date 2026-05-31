using System.Collections.ObjectModel;
using System.Windows.Input;
using OmniFrame.Core;
namespace OmniFrame.Wpf.ViewModels
{
    public class BlockCutStatsViewModel : ViewModelBase
    {
        private readonly IProductManager _productMgr;
        public ObservableCollection<StatsRecord> Records { get; } = new();
        private string _summary = "总产量: 0 | 良品: 0 | 不良: 0 | 良率: 0%";
        public string Summary { get => _summary; set => Set(ref _summary, value); }
        private string _fromDate = System.DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
        public string FromDate { get => _fromDate; set { Set(ref _fromDate, value); Query(); } }
        private string _toDate = System.DateTime.Today.ToString("yyyy-MM-dd");
        public string ToDate { get => _toDate; set { Set(ref _toDate, value); Query(); } }
        private int _topN = 10;
        public int TopN { get => _topN; set { Set(ref _topN, value); Query(); } }
        public ICommand QueryCommand { get; }
        public BlockCutStatsViewModel(IProductManager productMgr) { _productMgr = productMgr; QueryCommand = new RelayCommand(Query); Query(); }
        private void Query()
        {
            Records.Clear();
            Records.Add(new StatsRecord { Time = "10:30", SerialNo = "SN-001", Result = "OK", CycleTime = 12.5 });
            Records.Add(new StatsRecord { Time = "10:31", SerialNo = "SN-002", Result = "OK", CycleTime = 11.8 });
            Records.Add(new StatsRecord { Time = "10:32", SerialNo = "SN-003", Result = "NG", CycleTime = 15.2 });
            Summary = "总产量: 3 | 良品: 2 | 不良: 1 | 良率: 66.7%";
        }
    }
    public class StatsRecord { public string Time { get; set; } public string SerialNo { get; set; } public string Result { get; set; } public double CycleTime { get; set; } }
}
