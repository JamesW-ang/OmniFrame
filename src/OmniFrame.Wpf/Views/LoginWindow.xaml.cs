using System.Windows;
using OmniFrame.Wpf.ViewModels;

namespace OmniFrame.Wpf.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.LoginSucceeded) && _vm.LoginSucceeded)
                {
                    DialogResult = true;
                    Close();
                }
            };
        }
    }
}
