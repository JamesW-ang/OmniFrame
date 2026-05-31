using MotionIO;
using System.Windows.Input;
namespace OmniFrame.Wpf.ViewModels
{
    public class MotionSetViewModel : ViewModelBase
    {
        private readonly Motion _motion;
        private int _selectedAxis = 1;  public int SelectedAxis { get => _selectedAxis; set => Set(ref _selectedAxis, value); }
        private double _velocity = 100; public double Velocity { get => _velocity; set => Set(ref _velocity, value); }
        private double _acceleration = 500; public double Acceleration { get => _acceleration; set => Set(ref _acceleration, value); }
        private double _deceleration = 500; public double Deceleration { get => _deceleration; set => Set(ref _deceleration, value); }
        private double _homeVelocity = 50; public double HomeVelocity { get => _homeVelocity; set => Set(ref _homeVelocity, value); }
        private double _homeOffset = 0;    public double HomeOffset { get => _homeOffset; set => Set(ref _homeOffset, value); }
        private double _softLimitP = 200;  public double SoftLimitP { get => _softLimitP; set => Set(ref _softLimitP, value); }
        private double _softLimitN = -200; public double SoftLimitN { get => _softLimitN; set => Set(ref _softLimitN, value); }
        private bool _enableSoftLimit = true; public bool EnableSoftLimit { get => _enableSoftLimit; set => Set(ref _enableSoftLimit, value); }
        private double _targetPos = 0;     public double TargetPos { get => _targetPos; set => Set(ref _targetPos, value); }
        private double _jogSpeed = 50;     public double JogSpeed { get => _jogSpeed; set => Set(ref _jogSpeed, value); }
        private double _curPos;            public double CurPos { get => _curPos; set => Set(ref _curPos, value); }
        private bool _isServoOn;           public bool IsServoOn { get => _isServoOn; set => Set(ref _isServoOn, value); }
        private string _axisState = "空闲"; public string AxisState { get => _axisState; set => Set(ref _axisState, value); }
        public ICommand AbsMoveCmd { get; } public ICommand RelMoveCmd { get; } public ICommand StopCmd { get; }
        public ICommand HomeCmd { get; } public ICommand ServoOnCmd { get; } public ICommand ServoOffCmd { get; }
        public ICommand ApplyParamsCmd { get; }
        public MotionSetViewModel(Motion motion) { _motion = motion;
            AbsMoveCmd = new RelayCommand(() => Try(() => _motion?.AbsMove(SelectedAxis, TargetPos, JogSpeed)));
            RelMoveCmd = new RelayCommand(() => Try(() => _motion?.RelativeMove(SelectedAxis, TargetPos, JogSpeed)));
            StopCmd = new RelayCommand(() => Try(() => _motion?.StopAxis(SelectedAxis)));
            HomeCmd = new RelayCommand(() => Try(() => { _motion?.Home(SelectedAxis, HomeMode.ORG_P_EZ); AxisState = "回零中"; }));
            ServoOnCmd = new RelayCommand(() => { _motion?.ServoOn(SelectedAxis); IsServoOn = true; });
            ServoOffCmd = new RelayCommand(() => { _motion?.ServoOff(SelectedAxis); IsServoOn = false; });
            ApplyParamsCmd = new RelayCommand(() => Try(() => _motion?.SetAxisParam(SelectedAxis, Acceleration, Deceleration, 0, Velocity, Velocity * 1.2)));
        }
        private void Try(System.Action a) { try { a(); CurPos = _motion?.GetAxisPos(SelectedAxis) ?? 0; AxisState = _motion?.GetAxisState(SelectedAxis).ToString() ?? "--"; } catch (System.Exception ex) { OmniFrame.Common.Logger.Warning($"运动命令失败: {ex.Message}"); } }
    }
}
