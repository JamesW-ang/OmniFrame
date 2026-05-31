using System.Windows.Input;
using OmniFrame.Core.AdvancedFeatures;
namespace OmniFrame.Wpf.ViewModels
{
    public class OeeViewModel : ViewModelBase
    {
        private readonly IOeeManager _oeeMgr;
        private double _availability, _performance, _quality, _oee;
        public double Availability { get => _availability; set => Set(ref _availability, value); }
        public double Performance { get => _performance; set => Set(ref _performance, value); }
        public double Quality { get => _quality; set => Set(ref _quality, value); }
        public double Oee { get => _oee; set => Set(ref _oee, value); }
        public ICommand RefreshCommand { get; }
        public OeeViewModel(IOeeManager oeeMgr) { _oeeMgr = oeeMgr; RefreshCommand = new RelayCommand(Refresh); }
        private void Refresh() { Oee = 85.5; Availability = 92.0; Performance = 95.0; Quality = 97.8; }
    }
}
