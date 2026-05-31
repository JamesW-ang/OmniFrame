using OmniFrame.Core.BlockCut;
namespace OmniFrame.Wpf.ViewModels
{
    public class CameraDebugViewModel : ViewModelBase
    {
        private readonly BlockCutVision _vision;
        private string _status = "相机就绪";
        public string Status { get => _status; set => Set(ref _status, value); }
        private int _exposureMs = 100;
        public int ExposureMs { get => _exposureMs; set => Set(ref _exposureMs, value); }
        public CameraDebugViewModel(BlockCutVision vision) { _vision = vision; }
    }
}
