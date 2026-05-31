using System.Windows.Input;
using OmniFrame.Core;

namespace OmniFrame.Wpf.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserManager _userManager;
        private string _userId = "admin";
        private string _errorMessage;

        public string UserId { get => _userId; set => Set(ref _userId, value); }
        public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

        public ICommand LoginCommand { get; }

        /// <summary>登录成功时设为 true，Window 据此关闭</summary>
        public bool LoginSucceeded { get; private set; }

        public LoginViewModel(IUserManager userManager)
        {
            _userManager = userManager;
            LoginCommand = new RelayCommand<System.Windows.Controls.PasswordBox>(ExecuteLogin);
        }

        private void ExecuteLogin(System.Windows.Controls.PasswordBox passwordBox)
        {
            ErrorMessage = null;
            var password = passwordBox?.Password ?? "";

            if (string.IsNullOrWhiteSpace(UserId))
            {
                ErrorMessage = "请输入用户名";
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "请输入密码";
                return;
            }

            var result = _userManager.Login(UserId.Trim(), password);
            if (result.Success)
            {
                LoginSucceeded = true;
            }
            else
            {
                ErrorMessage = result.Message ?? "用户名或密码错误";
            }
        }
    }
}
