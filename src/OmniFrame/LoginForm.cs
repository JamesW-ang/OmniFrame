using System;
using System.Drawing;
using System.Windows.Forms;
using OmniFrame.Core;
using OmniFrame.Common;
using OmniFrame.Theme;

namespace OmniFrame
{
    /// <summary>
    /// 登录窗体类 - 用户身份认证入口
        /// </summary>
    public partial class LoginForm : Form
    {
        private readonly IUserManager _userManager;
        private int _failedAttempts;

        /// <summary>
        /// 构造函数（DI容器使用）
        /// </summary>
        public LoginForm(IUserManager userManager)
        {
            _userManager = userManager;
            InitializeComponent();

            // Apply theme
            UiTheme.CurrentTheme = UiTheme.DarkTheme;
            this.ApplyTheme();
        }

        /// <summary>
        /// 登录窗体加载事件
        /// 验证系统管理器状态，聚焦用户名输入框
        /// </summary>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            if (_userManager == null)
            {
                MessageBox.Show("用户管理器未初始化!", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            txtUserId.Focus();
        }

        /// <summary>
        /// 登录按钮点击事件        /// 1. 验证输入合法性（空检查）
        /// 2. 调用UserManager.Login()执行认证
        /// 3. 处理登录结果（成功/失败）
        /// 4. 失败次数过多触发账户锁定策略
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("请输入用户名!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUserId.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入密码!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPassword.Focus();
                return;
            }

            var result = _userManager.Login(userId, password);

            if (result.Success)
            {
                Logger.Info($"用户登录成功: {result.User.UserName}");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _failedAttempts++;
                Logger.Warning($"登录失败: {userId}, 原因: {result.Message}");
                MessageBox.Show(result.Message, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (_failedAttempts >= 3)
                {
                    MessageBox.Show("登录失败次数过多，请稍后再试!", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                else
                {
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }

        /// <summary>
        /// 取消按钮点击事件        /// 关闭登录窗体，返回Cancel结果
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 密码框键盘事件        /// 回车键触发登录操作
        /// </summary>
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
