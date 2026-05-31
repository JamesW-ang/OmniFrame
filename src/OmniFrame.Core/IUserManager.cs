using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 用户管理器接口
    /// </summary>
    public interface IUserManager : IDisposable
    {
        UserInfo CurrentUser { get; }
        bool IsLoggedIn { get; }
        int UserCount { get; }

        event EventHandler<UserInfo> UserLoggedIn;
        event EventHandler<UserInfo> UserLoggedOut;
        event EventHandler<UserInfo> UserAdded;
        event EventHandler<UserInfo> UserRemoved;

        bool Initialize();
        LoginResult Login(string userId, string password);
        void Logout();
        bool AddUser(UserInfo user, string password);
        bool RemoveUser(string userId);
        bool ChangePassword(string oldPassword, string newPassword);
        bool ResetPassword(string userId, string newPassword);
        bool UpdateUser(UserInfo updatedUser);
        List<UserInfo> GetAllUsers();
        List<UserInfo> GetUsersByLevel(UserLevel minLevel);
        bool CheckPermission(UserLevel requiredLevel);
    }
}
