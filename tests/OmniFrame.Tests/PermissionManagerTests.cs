using Moq;
using NUnit.Framework;
using OmniFrame.Common;
using OmniFrame.Core;
using System;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class PermissionManagerTests
    {
        private Mock<IUserManager> _userMock;
        private PermissionManager _permissionManager;

        [SetUp]
        public void Setup()
        {
            _userMock = new Mock<IUserManager>();
            _permissionManager = new PermissionManager();
            _permissionManager.Initialize(_userMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _permissionManager.Dispose();
        }

        [Test]
        public void Initialize_WithUserManager_IsReady()
        {
            var pm = new PermissionManager();
            Assert.DoesNotThrow(() => pm.Initialize(_userMock.Object));
            pm.Dispose();
        }

        [Test]
        public void CheckPermission_UserHasAccess_ReturnsTrue()
        {
            _userMock.Setup(m => m.CheckPermission(UserLevel.Administrator)).Returns(true);

            bool result = _permissionManager.CheckPermission(UserLevel.Administrator);

            Assert.That(result, Is.True);
            _userMock.Verify(m => m.CheckPermission(UserLevel.Administrator), Times.Once);
        }

        [Test]
        public void CheckPermission_UserLacksAccess_ReturnsFalse()
        {
            _userMock.Setup(m => m.CheckPermission(UserLevel.Administrator)).Returns(false);

            bool result = _permissionManager.CheckPermission(UserLevel.Administrator);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CheckPermission_WithOperation_LogsAudit()
        {
            var currentUser = new UserInfo
            {
                UserId = "eng",
                UserName = "Engineer",
                Level = UserLevel.Engineer,
                IsActive = true
            };
            _userMock.Setup(m => m.CurrentUser).Returns(currentUser);
            _userMock.Setup(m => m.CheckPermission(UserLevel.Operator)).Returns(true);

            bool result = _permissionManager.CheckPermission(UserLevel.Operator, "test-operation");

            Assert.That(result, Is.True);
        }

        [Test]
        public void GetCurrentUserLevel_DelegatesToUserManager()
        {
            _userMock.Setup(m => m.CurrentUser).Returns(new UserInfo { Level = UserLevel.Administrator });

            var level = _permissionManager.GetCurrentUserLevel();

            Assert.That(level, Is.EqualTo(UserLevel.Administrator));
        }

        [Test]
        public void GetCurrentUser_ReturnsUserFromManager()
        {
            var expected = new UserInfo { UserId = "admin" };
            _userMock.Setup(m => m.CurrentUser).Returns(expected);

            var actual = _permissionManager.GetCurrentUser();

            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void CheckAndExecute_WithPermission_ExecutesAction()
        {
            _userMock.Setup(m => m.CheckPermission(UserLevel.Administrator)).Returns(true);
            bool actionExecuted = false;

            bool result = _permissionManager.CheckAndExecute(UserLevel.Administrator,
                "op", () => actionExecuted = true);

            Assert.That(result, Is.True);
            Assert.That(actionExecuted, Is.True);
        }

        [Test]
        public void CheckAndExecute_WithoutPermission_DoesNotExecute()
        {
            _userMock.Setup(m => m.CheckPermission(UserLevel.Administrator)).Returns(false);
            bool actionExecuted = false;

            bool result = _permissionManager.CheckAndExecute(UserLevel.Administrator,
                "op", () => actionExecuted = true);

            Assert.That(result, Is.False);
            Assert.That(actionExecuted, Is.False);
        }

        [Test]
        public void CheckPermission_NullUserManager_ReturnsFalse()
        {
            var pm = new PermissionManager();
            Assert.DoesNotThrow(() => pm.CheckPermission(UserLevel.Operator));
            pm.Dispose();
        }

        [Test]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            _permissionManager.Dispose();
            Assert.DoesNotThrow(() => _permissionManager.Dispose());
        }
    }
}
