using System.Collections.ObjectModel;
namespace OmniFrame.Wpf.ViewModels
{
    public class AxisSetupViewModel : ViewModelBase
    {
        public ObservableCollection<AxisParamItem> Axes { get; } = new();
        private AxisParamItem _selected;
        public AxisParamItem SelectedAxis { get => _selected; set { Set(ref _selected, value); ShowDetail(value); } }
        private int _axisNo; public int AxisNo { get => _axisNo; set => Set(ref _axisNo, value); }
        private double _vel, _acc, _dec, _homeVel, _homeOffset, _softLimitP, _softLimitN;
        public double Vel { get => _vel; set => Set(ref _vel, value); } public double Acc { get => _acc; set => Set(ref _acc, value); }
        public double Dec { get => _dec; set => Set(ref _dec, value); } public double HomeVel { get => _homeVel; set => Set(ref _homeVel, value); }
        public double HomeOff { get => _homeOffset; set => Set(ref _homeOffset, value); }
        public double LimP { get => _softLimitP; set => Set(ref _softLimitP, value); } public double LimN { get => _softLimitN; set => Set(ref _softLimitN, value); }
        public AxisSetupViewModel() { for (int i = 1; i <= 8; i++) Axes.Add(new AxisParamItem { AxisNo = i, Name = $"轴{i}", Velocity = 100, Acc = 500, Dec = 500, HomeVel = 50, SoftLimP = 200, SoftLimN = -200 }); }
        private void ShowDetail(AxisParamItem a) { if (a == null) return; AxisNo = a.AxisNo; Vel = a.Velocity; Acc = a.Acc; Dec = a.Dec; HomeVel = a.HomeVel; HomeOff = a.HomeOffset; LimP = a.SoftLimP; LimN = a.SoftLimN; }
    }
    public class AxisParamItem { public int AxisNo { get; set; } public string Name { get; set; } public double Velocity { get; set; } public double Acc { get; set; } public double Dec { get; set; } public double HomeVel { get; set; } public double HomeOffset { get; set; } public double SoftLimP { get; set; } public double SoftLimN { get; set; } }
}
