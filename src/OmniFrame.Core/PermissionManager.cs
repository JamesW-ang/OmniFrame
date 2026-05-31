using System;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 权限管理核心类
        /// </summary>
    public class PermissionManager : IDisposable, IPermissionManager
    {
        private readonly object _lock = new object();
        private IUserManager _userManager;
        private IAuditLogger _auditLogger;
        private bool _isDisposed;

        public PermissionManager() : this(null)
        {
        }

        public PermissionManager(IAuditLogger auditLogger)
        {
            _auditLogger = auditLogger;
        }

        public void Initialize(IUserManager userManager)
        {
            _userManager = userManager;
            _auditLogger?.Initialize();
        }

        /// <summary>
        /// 检查权限
        /// </summary>
        public bool CheckPermission(UserLevel requiredLevel, string operation = null)
        {
            lock (_lock)
            {
                bool hasPermission = _userManager?.CheckPermission(requiredLevel) ?? false;

                // 记录操作审计
                if (operation != null && (_userManager?.CurrentUser?.Level >= UserLevel.Engineer))
                {
                    _auditLogger?.LogOperation(
                        _userManager.CurrentUser.UserId,
                        _userManager.CurrentUser.UserName,
                        _userManager.CurrentUser.Level,
                        operation,
                        hasPermission);
                }

                return hasPermission;
            }
        }

        /// <summary>
        /// 检查并执行操作
        /// </summary>
        public bool CheckAndExecute(UserLevel requiredLevel, string operation, Action action, string parameters = null, string oldValue = null, string newValue = null)
        {
            lock (_lock)
            {
                bool hasPermission = CheckPermission(requiredLevel, operation);

                if (hasPermission && action != null)
                {
                    action();

                    // 记录参数变更
                    if (parameters != null && (_userManager?.CurrentUser?.Level >= UserLevel.Engineer))
                    {
                        _auditLogger?.LogParameterChange(
                            _userManager.CurrentUser.UserId,
                            _userManager.CurrentUser.UserName,
                            _userManager.CurrentUser.Level,
                            operation,
                            parameters,
                            oldValue,
                            newValue);
                    }
                }

                return hasPermission;
            }
        }

        /// <summary>
        /// 获取当前用户角色
        /// </summary>
        public UserLevel GetCurrentUserLevel()
        {
            return _userManager?.CurrentUser?.Level ?? UserLevel.Operator;
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        public UserInfo GetCurrentUser()
        {
            return _userManager?.CurrentUser;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

}
