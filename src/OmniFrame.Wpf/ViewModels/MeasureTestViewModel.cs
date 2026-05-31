using System.Windows.Input;
namespace OmniFrame.Wpf.ViewModels
{
    public class MeasureTestViewModel : ViewModelBase
    {
        private int _channel = 1;     public int Channel { get => _channel; set => Set(ref _channel, value); }
        private int _warnTimeMs = 500; public int WarnTimeMs { get => _warnTimeMs; set => Set(ref _warnTimeMs, value); }
        private string _elapsed = "0ms"; public string Elapsed { get => _elapsed; set => Set(ref _elapsed, value); }
        private string _cameraStatus = "就绪"; public string CameraStatus { get => _cameraStatus; set => Set(ref _cameraStatus, value); }
        private string _resultText = ""; public string ResultText { get => _resultText; set => Set(ref _resultText, value); }
        public ICommand TestCmd { get; } public ICommand ScanCmd { get; }
        public MeasureTestViewModel() { TestCmd = new RelayCommand(() => { CameraStatus = "测试中..."; Elapsed = "150ms"; ResultText = "OK — (1.234, 5.678)"; CameraStatus = "就绪"; }); ScanCmd = new RelayCommand(() => { CameraStatus = "扫描中..."; ResultText = "扫描完成"; CameraStatus = "就绪"; }); }
    }
}
