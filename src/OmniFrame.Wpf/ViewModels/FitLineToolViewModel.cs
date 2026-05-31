using System.Collections.ObjectModel;
using System.Windows.Input;
namespace OmniFrame.Wpf.ViewModels
{
    public class FitLineToolViewModel : ViewModelBase
    {
        public ObservableCollection<FitPoint> Points { get; } = new();
        // 拟合参数
        private int _threshold = 128;    public int Threshold { get => _threshold; set => Set(ref _threshold, value); }
        private double _sigma = 1.5;     public double Sigma { get => _sigma; set => Set(ref _sigma, value); }
        private int _segWidth = 10;      public int SegWidth { get => _segWidth; set => Set(ref _segWidth, value); }
        private int _segStep = 5;        public int SegStep { get => _segStep; set => Set(ref _segStep, value); }
        private int _segNum = 50;        public int SegNum { get => _segNum; set => Set(ref _segNum, value); }
        private int _maxPoints = 200;    public int MaxPoints { get => _maxPoints; set => Set(ref _maxPoints, value); }
        private int _iterations = 10;    public int Iterations { get => _iterations; set => Set(ref _iterations, value); }
        private double _distFactor = 2.0; public double DistFactor { get => _distFactor; set => Set(ref _distFactor, value); }
        // 结果
        private string _resultStart = "--";  public string ResultStart { get => _resultStart; set => Set(ref _resultStart, value); }
        private string _resultEnd = "--";    public string ResultEnd { get => _resultEnd; set => Set(ref _resultEnd, value); }
        private string _resultAngle = "--";  public string ResultAngle { get => _resultAngle; set => Set(ref _resultAngle, value); }
        private double _rSquared;            public double RSquared { get => _rSquared; set => Set(ref _rSquared, value); }
        private string _imagePath = "";      public string ImagePath { get => _imagePath; set => Set(ref _imagePath, value); }
        public ICommand RunCommand { get; } public ICommand OpenFileCommand { get; }
        public FitLineToolViewModel()
        {
            RunCommand = new RelayCommand(Run); OpenFileCommand = new RelayCommand(() => ImagePath = "选择图片...");
            Points.Add(new FitPoint { X = 0, Y = 0 }); Points.Add(new FitPoint { X = 10, Y = 10.2 }); Points.Add(new FitPoint { X = 20, Y = 19.8 });
        }
        private void Run() { ResultStart = "(0.0, 0.0)"; ResultEnd = "(20.0, 19.8)"; ResultAngle = "45.0°"; RSquared = 0.9998; }
    }
    public class FitPoint { public double X { get; set; } public double Y { get; set; } }
}
