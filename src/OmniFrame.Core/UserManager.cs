using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using BCrypt.Net;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 用户权限级别
        /// </summary>
    public enum UserLevel
    {
        Operator = 0,       // 操作员
        FAE = 1,            // FAE
        Adjustor = 2,       // 调机员
        Engineer = 3,       // 工程师
        Administrator = 4   // 管理员
    }

    /// <summary>
    /// 用户信息
        /// </summary>
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public UserLevel Level { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
        public string Description { get; set; }

        public UserInfo()
        {
            CreateTime = DateTime.Now;
            IsActive = true;
            MustChangePassword = false;
        }

        public bool HasPermission(UserLevel requiredLevel)
        {
            return Level >= requiredLevel;
        }
    }

    /// <summary>
    /// 登录结果
        /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo User { get; set; }
        public bool MustChangePassword { get; set; }
    }

    /// <summary>
    /// 用户管理器 - 管理用户登录、权限、密码等
        /// </summary>
    public class UserManager : IUserManager
    {
        private readonly object _lock = new object();
        internal List<UserInfo> _users;
        private string _userFilePath;
        private UserInfo _currentUser;
        private bool _isDisposed;

        public UserInfo CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;
        public int UserCount => _users.Count;

        public event EventHandler<UserInfo> UserLoggedIn;
        public event EventHandler<UserInfo> UserLoggedOut;
        public event EventHandler<UserInfo> UserAdded;
        public event EventHandler<UserInfo> UserRemoved;

        /// <summary>
        /// 获取用户管理器实例（从DI容器解析）
        /// </summary>

        public UserManager()
        {
            _users = new List<UserInfo>();
            _userFilePath = "Config/Users.xml";
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化用户管理器...");

                // 确保配置目录存在
                string configDir = Path.GetDirectoryName(_userFilePath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 加载用户列表
                LoadUsers();

                // 如果没有用户，创建默认管理员账户
                if (_users.Count == 0)
                {
                    CreateDefaultAdmin();
                }
                else
                {
                    // 检查并更新旧格式的密码哈希
                    bool updated = false;
                    foreach (var user in _users)
                    {
                        // 检查是否为bcrypt格式哈希
                        if (!user.PasswordHash.StartsWith("$2a$") && !user.PasswordHash.StartsWith("$2b$") && !user.PasswordHash.StartsWith("$2y$"))
                        {
                            // 更新为默认密码"admin123"的bcrypt哈希
                            user.PasswordHash = HashPassword("admin123");
                            user.MustChangePassword = true;
                            updated = true;
                            Logger.Info($"用户 {user.UserId} 的密码哈希已更新为bcrypt格式，默认密码: admin123");
                        }
                    }
                    
                    // 如果有更新，保存用户列表
                    if (updated)
                    {
                        SaveUsers();
                    }
                }

                Logger.Info($"用户管理器初始化完成，共 {_users.Count} 个用户");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("用户管理器初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 创建默认管理员账户
        /// </summary>
        private void CreateDefaultAdmin()
        {
            string adminPassword = Environment.GetEnvironmentVariable("OMNIFRAME_ADMIN_INITIAL_PASSWORD");
            bool isDefaultPassword = false;
            if (string.IsNullOrEmpty(adminPassword))
            {
                adminPassword = "admin123";
                isDefaultPassword = true;
                Logger.Warning("环境变量 OMNIFRAME_ADMIN_INITIAL_PASSWORD 未设置，使用默认密码 admin123");
            }

            var admin = new UserInfo
            {
                UserId = "admin",
                UserName = "Administrator",
                PasswordHash = HashPassword(adminPassword),
                Level = UserLevel.Administrator,
                Department = "System",
                MustChangePassword = true,
                Description = isDefaultPassword ? "系统默认管理员账户（使用默认密码）" : "系统默认管理员账户"
            };

            _users.Add(admin);
            SaveUsers();

            Logger.Warning("创建了默认管理员账户 (admin)，首次登录必须修改密码!");
        }

        /// <summary>
        /// 加载用户列表
        /// </summary>
        private void LoadUsers()
        {
            if (!File.Exists(_userFilePath))
                return;

            try
            {
                var serializer = new XmlSerializer(typeof(List<UserInfo>));
                using (var reader = new StreamReader(_userFilePath))
                {
                    _users = (List<UserInfo>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载用户列表失败", ex);
                _users = new List<UserInfo>();
            }
        }

        /// <summary>
        /// 保存用户列表
        /// </summary>
        private void SaveUsers()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<UserInfo>));
                using (var writer = new StreamWriter(_userFilePath))
                {
                    serializer.Serialize(writer, _users);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("保存用户列表失败", ex);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        public LoginResult Login(string userId, string password)
        {
            lock (_lock)
            {
                if (_currentUser != null)
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "已有用户登录，请先登出"
                    };
                }

                var user = _users.FirstOrDefault(u => u.UserId == userId && u.IsActive);
                if (user == null)
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "用户名或密码错误"
                    };
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "用户名或密码错误"
                    };
                }

                user.LastLoginTime = DateTime.Now;
                _currentUser = user;

                UserLoggedIn?.Invoke(this, user);
                Logger.Info($"用户登录成功: {user.UserName} ({user.Level})");

                return new LoginResult
                {
                    Success = true,
                    Message = "登录成功",
                    User = user,
                    MustChangePassword = user.MustChangePassword
                };
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            lock (_lock)
            {
                if (_currentUser != null)
                {
                    Logger.Info($"用户登出: {_currentUser.UserName}");
                    UserLoggedOut?.Invoke(this, _currentUser);
                    _currentUser = null;
                }
            }
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        public bool AddUser(UserInfo user, string password)
        {
            lock (_lock)
            {
                if (_currentUser == null || !_currentUser.HasPermission(UserLevel.Administrator))
                {
                    Logger.Warning("权限不足，无法添加用户");
                    return false;
                }

                if (_users.Any(u => u.UserId == user.UserId))
                {
                    Logger.Warning($"用户ID已存在: {user.UserId}");
                    return false;
                }

                if (!ValidatePasswordComplexity(password))
                {
                    Logger.Warning("密码复杂度不足，至少8位，含大小写字母和数字");
                    return false;
                }

                user.PasswordHash = HashPassword(password);
                user.MustChangePassword = true;
                _users.Add(user);
                SaveUsers();

                UserAdded?.Invoke(this, user);
                Logger.Info($"添加用户成功: {user.UserName}，用户首次登录必须修改密码");
                return true;
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public bool RemoveUser(string userId)
        {
            lock (_lock)
            {
                if (_currentUser == null || !_currentUser.HasPermission(UserLevel.Administrator))
                {
                    Logger.Warning("权限不足，无法删除用户");
                    return false;
                }

                if (userId == _currentUser.UserId)
                {
                    Logger.Warning("不能删除当前登录用户");
                    return false;
                }

                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                    return false;

                _users.Remove(user);
                SaveUsers();

                UserRemoved?.Invoke(this, user);
                Logger.Info($"删除用户: {user.UserName}");
                return true;
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            lock (_lock)
            {
                if (_currentUser == null)
                    return false;

                if (!VerifyPassword(oldPassword, _currentUser.PasswordHash))
                {
                    Logger.Warning("原密码错误");
                    return false;
                }

                if (!ValidatePasswordComplexity(newPassword))
                {
                    Logger.Warning("密码复杂度不足，至少8位，含大小写字母和数字");
                    return false;
                }

                _currentUser.PasswordHash = HashPassword(newPassword);
                _currentUser.MustChangePassword = false;
                SaveUsers();

                Logger.Info($"用户 {_currentUser.UserName} 修改密码成功");
                return true;
            }
        }

        /// <summary>
        /// 重置用户密码（管理员功能）
        /// </summary>
        public bool ResetPassword(string userId, string newPassword)
        {
            lock (_lock)
            {
                if (_currentUser == null || !_currentUser.HasPermission(UserLevel.Administrator))
                {
                    Logger.Warning("权限不足，无法重置密码");
                    return false;
                }

                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                    return false;

                if (!ValidatePasswordComplexity(newPassword))
                {
                    Logger.Warning("密码复杂度不足，至少8位，含大小写字母和数字");
                    return false;
                }

                user.PasswordHash = HashPassword(newPassword);
                user.MustChangePassword = true;
                SaveUsers();

                Logger.Info($"重置用户 {user.UserName} 密码成功，用户下次登录必须修改密码");
                return true;
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        public bool UpdateUser(UserInfo updatedUser)
        {
            lock (_lock)
            {
                if (_currentUser == null)
                    return false;

                var user = _users.FirstOrDefault(u => u.UserId == updatedUser.UserId);
                if (user == null)
                    return false;

                // 检查权限
                if (_currentUser.UserId != user.UserId && !_currentUser.HasPermission(UserLevel.Administrator))
                {
                    Logger.Warning("权限不足，无法更新其他用户信息");
                    return false;
                }

                // 非管理员不能修改权限级别
                if (_currentUser.Level != UserLevel.Administrator)
                {
                    updatedUser.Level = user.Level;
                }

                // 保留密码
                updatedUser.PasswordHash = user.PasswordHash;

                int index = _users.IndexOf(user);
                _users[index] = updatedUser;
                SaveUsers();

                Logger.Info($"更新用户信息: {updatedUser.UserName}");
                return true;
            }
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        public List<UserInfo> GetAllUsers()
        {
            lock (_lock)
            {
                return _users.ToList();
            }
        }

        /// <summary>
        /// 获取指定级别及以上的用户
        /// </summary>
        public List<UserInfo> GetUsersByLevel(UserLevel minLevel)
        {
            lock (_lock)
            {
                return _users.Where(u => u.Level >= minLevel).ToList();
            }
        }

        /// <summary>
        /// 检查当前用户权限
        /// </summary>
        public bool CheckPermission(UserLevel requiredLevel)
        {
            return _currentUser?.HasPermission(requiredLevel) ?? false;
        }

        /// <summary>
        /// 密码哈希
        /// </summary>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// 验证密码（仅支持 bcrypt）
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                Logger.Warning("密码哈希格式不兼容（非bcrypt），请联系管理员重置密码");
                return false;
            }
        }

        /// <summary>
        /// 验证密码复杂度
        /// </summary>
        private bool ValidatePasswordComplexity(string password)
        {
            // 至少8位，含大小写字母和数字
            if (password.Length < 8)
                return false;
            if (!Regex.IsMatch(password, "[A-Z]"))
                return false;
            if (!Regex.IsMatch(password, "[a-z]"))
                return false;
            if (!Regex.IsMatch(password, "[0-9]"))
                return false;
            return true;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Logout();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
