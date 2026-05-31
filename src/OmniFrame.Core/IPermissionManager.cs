using System;

namespace OmniFrame.Core
{
    /// <summary>
    /// 权限管理接口
    /// </summary>
    public interface IPermissionManager : IDisposable
    {
        void Initialize(IUserManager userManager);
        bool CheckPermission(UserLevel requiredLevel, string operation = null);
        bool CheckAndExecute(UserLevel requiredLevel, string operation, Action action, string parameters = null, string oldValue = null, string newValue = null);
        UserLevel GetCurrentUserLevel();
        UserInfo GetCurrentUser();
    }
}
