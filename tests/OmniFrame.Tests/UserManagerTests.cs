using NUnit.Framework;
using OmniFrame.Core;
using System.Collections.Generic;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class UserManagerTests
    {
        private UserManager _userManager;
        private List<UserInfo> _users;

        [SetUp]
        public void Setup()
        {
            _userManager = new UserManager();
            _users = new List<UserInfo>();
            _userManager._users = _users;
        }

        private UserInfo AddUser(string userId, string userName, UserLevel level, string plainPassword)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            var user = new UserInfo
            {
                UserId = userId,
                UserName = userName,
                Level = level,
                PasswordHash = hash,
                IsActive = true,
                MustChangePassword = false
            };
            _users.Add(user);
            return user;
        }

        [Test]
        public void Login_ValidCredentials_ReturnsSuccess()
        {
            AddUser("admin", "Administrator", UserLevel.Administrator, "MyP@ssw0rd");

            var result = _userManager.Login("admin", "MyP@ssw0rd");

            Assert.That(result.Success, Is.True);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.UserId, Is.EqualTo("admin"));
        }

        [Test]
        public void Login_WrongPassword_ReturnsFailure()
        {
            AddUser("admin", "Administrator", UserLevel.Administrator, "MyP@ssw0rd");

            var result = _userManager.Login("admin", "wrong");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("用户名或密码错误"));
        }

        [Test]
        public void Login_UnknownUser_ReturnsFailure()
        {
            var result = _userManager.Login("nonexistent", "password");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("用户名或密码错误"));
        }

        [Test]
        public void Login_InactiveUser_ReturnsFailure()
        {
            var user = AddUser("inactive", "InactiveUser", UserLevel.Operator, "password");
            user.IsActive = false;

            var result = _userManager.Login("inactive", "password");

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void Login_AlreadyLoggedIn_ReturnsFailure()
        {
            AddUser("user1", "User1", UserLevel.Operator, "password");
            _userManager.Login("user1", "password");

            AddUser("user2", "User2", UserLevel.Operator, "password");
            var result = _userManager.Login("user2", "password");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("已有用户登录，请先登出"));
        }

        [Test]
        public void Logout_ClearsCurrentUser()
        {
            AddUser("admin", "Administrator", UserLevel.Administrator, "password");
            _userManager.Login("admin", "password");

            _userManager.Logout();

            Assert.That(_userManager.CurrentUser, Is.Null);
        }

        [Test]
        public void CheckPermission_NotLoggedIn_ReturnsFalse()
        {
            bool result = _userManager.CheckPermission(UserLevel.Operator);
            Assert.That(result, Is.False);
        }

        [Test]
        public void CheckPermission_SufficientLevel_ReturnsTrue()
        {
            AddUser("eng", "Engineer", UserLevel.Engineer, "password");
            _userManager.Login("eng", "password");

            bool result = _userManager.CheckPermission(UserLevel.Operator);
            Assert.That(result, Is.True);
        }

        [Test]
        public void CheckPermission_InsufficientLevel_ReturnsFalse()
        {
            AddUser("op", "Operator", UserLevel.Operator, "password");
            _userManager.Login("op", "password");

            bool result = _userManager.CheckPermission(UserLevel.Administrator);
            Assert.That(result, Is.False);
        }

        [Test]
        public void Login_UpdatesLastLoginTime()
        {
            AddUser("admin", "Administrator", UserLevel.Administrator, "password");
            var before = System.DateTime.Now;

            _userManager.Login("admin", "password");
            var after = System.DateTime.Now;

            var currentUser = _userManager.CurrentUser;
            Assert.That(currentUser.LastLoginTime, Is.Not.Null);
            Assert.That(currentUser.LastLoginTime, Is.GreaterThanOrEqualTo(before));
            Assert.That(currentUser.LastLoginTime, Is.LessThanOrEqualTo(after));
        }
    }
}
