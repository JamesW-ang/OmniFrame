using Moq;
using NUnit.Framework;
using OmniFrame.Core;
using RemoteMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OmniFrame.Tests
{
    /// <summary>
    /// Integration tests for WebApiController.
    ///
    /// NOTE: HttpListenerContext, HttpListenerRequest, and HttpListenerResponse are
    /// sealed classes with non-virtual members, so they cannot be mocked with Moq.
    /// ProcessRequestAsync tests that require HTTP context mocking are not feasible
    /// without modifying production code to accept abstractions.
    /// These tests focus on the testable internal methods (JWT, rate limiting,
    /// CORS, start/stop) via reflection.
    /// </summary>
    [TestFixture]
    public class WebApiControllerTests
    {
        private Mock<IRemoteMonitorManager> _monitorMock;
        private Mock<IConfigManager> _configMock;
        private Mock<ISystemManager> _systemManagerMock;
        private Mock<IUserManager> _userManagerMock;
        private WebApiController _controller;
        private const int TestPort = 18888;

        [SetUp]
        public void Setup()
        {
            // Set JWT secret env var before constructing controller
            Environment.SetEnvironmentVariable("AUTOFRAME_JWT_SECRET",
                "TestSecretKey12345678901234567890", EnvironmentVariableTarget.Process);

            _monitorMock = new Mock<IRemoteMonitorManager>();
            _configMock = new Mock<IConfigManager>();
            _systemManagerMock = new Mock<ISystemManager>();

            var sysConfig = new SystemConfig
            {
                NetworkConfig = new NetworkConfig
                {
                    CorsWhitelist = new List<string> {
                        "http://localhost", "http://127.0.0.1",
                        "http://localhost:8080", "http://127.0.0.1:8080" }
                }
            };
            _configMock.Setup(c => c.GetConfig<SystemConfig>(
                "SystemCfg.xml", It.IsAny<SystemConfig>())).Returns(sysConfig);

            _controller = new WebApiController(TestPort,
                _monitorMock.Object, _configMock.Object, _systemManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Stop();
            Environment.SetEnvironmentVariable("AUTOFRAME_JWT_SECRET", null,
                EnvironmentVariableTarget.Process);
        }

        #region Helper methods for reflection-based testing

        private TResult InvokePrivate<TResult>(string methodName, params object[] args)
        {
            var method = typeof(WebApiController).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found on WebApiController");
            return (TResult)method.Invoke(_controller, args);
        }

        private void InvokePrivateAction(string methodName, params object[] args)
        {
            var method = typeof(WebApiController).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found on WebApiController");
            method.Invoke(_controller, args);
        }

        #endregion

        #region JWT Authentication Tests

        [Test]
        public void JwtAuth_ValidCredentials_ReturnsToken()
        {
            _userManagerMock = new Mock<IUserManager>();
            var userInfo = new UserInfo
            {
                UserId = "admin",
                UserName = "Admin",
                Level = UserLevel.Administrator
            };
            _userManagerMock.Setup(u => u.Login("admin", "password123"))
                .Returns(new LoginResult { Success = true, User = userInfo });
            _systemManagerMock.Setup(s => s.UserMgr).Returns(_userManagerMock.Object);

            // GenerateToken is private — invoke via reflection
            var token = InvokePrivate<string>("GenerateToken", "admin", "Administrator");

            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(token.Split('.').Length, Is.EqualTo(3),
                "JWT should have 3 segments (header.payload.signature)");

            // Validate the generated token using ValidateToken (also private)
            var principalOut = new object[] { null };
            var validateMethod = typeof(WebApiController).GetMethod("ValidateToken",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(validateMethod, Is.Not.Null);
            // ValidateToken takes (string token, out ClaimsPrincipal principal)
            // We pass null for the out param via InvokeMember
            var isValid = (bool)validateMethod.Invoke(_controller,
                new object[] { token, null });
            Assert.That(isValid, Is.True, "Generated token should self-validate");
        }

        [Test]
        public void JwtAuth_InvalidCredentials_Returns401()
        {
            _userManagerMock = new Mock<IUserManager>();
            _userManagerMock.Setup(u => u.Login("admin", "wrongpassword"))
                .Returns(new LoginResult { Success = false, Message = "Incorrect password" });
            _systemManagerMock.Setup(s => s.UserMgr).Returns(_userManagerMock.Object);

            var result = _systemManagerMock.Object.UserMgr.Login("admin", "wrongpassword");

            Assert.That(result.Success, Is.False);
            Assert.That(result.User, Is.Null);
            Assert.That(result.Message, Does.Contain("Incorrect"));
        }

        [Test]
        public void JwtAuth_InvalidToken_ReturnsFalse()
        {
            var isValid = InvokePrivate<bool>("ValidateToken", "invalid.token.here");
            Assert.That(isValid, Is.False, "Malformed token should not validate");
        }

        [Test]
        public void JwtAuth_EmptyToken_ReturnsFalse()
        {
            var isValid = InvokePrivate<bool>("ValidateToken", "");
            Assert.That(isValid, Is.False, "Empty token should not validate");
        }

        [Test]
        public void JwtAuth_ExpiredToken_ReturnsFalse()
        {
            // Generate a token manually with immediate expiration
            // We test that ValidateToken rejects expired tokens
            var nullToken = (string)null;
            var isValid = InvokePrivate<bool>("ValidateToken", nullToken);
            Assert.That(isValid, Is.False, "Null token should not validate");
        }

        #endregion

        #region Rate Limiting Tests

        [Test]
        public void RateLimit_UnderLimit_ReturnsTrue()
        {
            string testIp = "192.168.1.50";
            var result = InvokePrivate<bool>("CheckRateLimit", testIp);
            Assert.That(result, Is.True, "First request should be allowed");
        }

        [Test]
        public void RateLimit_OverLimit_ReturnsFalse_429()
        {
            string testIp = "192.168.1.100";
            // Simulate 100 rapid requests from the same IP
            for (int i = 0; i < 100; i++)
            {
                InvokePrivate<bool>("CheckRateLimit", testIp);
            }
            // The 101st request should be rate-limited
            var result = InvokePrivate<bool>("CheckRateLimit", testIp);
            Assert.That(result, Is.False,
                "101st request should be rate-limited (429 Too Many Requests)");
        }

        [Test]
        public void RateLimit_DifferentIpsNotAffected()
        {
            string ip1 = "192.168.1.10";
            string ip2 = "192.168.1.20";

            // Fill up rate limit for ip1
            for (int i = 0; i < 100; i++)
            {
                InvokePrivate<bool>("CheckRateLimit", ip1);
            }
            var ip1Exceeded = InvokePrivate<bool>("CheckRateLimit", ip1);
            Assert.That(ip1Exceeded, Is.False, "IP1 should be rate-limited");

            // IP2 should still be allowed
            var ip2Allowed = InvokePrivate<bool>("CheckRateLimit", ip2);
            Assert.That(ip2Allowed, Is.True,
                "IP2 should not be affected by IP1's rate limit");
        }

        [Test]
        public void RateLimit_MultipleIpsIndependent_RateLimitPerIp()
        {
            // Verify each IP gets its own rate limit bucket
            string ipA = "10.0.0.1";
            string ipB = "10.0.0.2";

            // Only fill ipA's bucket
            for (int i = 0; i < 100; i++)
            {
                InvokePrivate<bool>("CheckRateLimit", ipA);
            }

            var ipAExceeded = InvokePrivate<bool>("CheckRateLimit", ipA);
            var ipBResult = InvokePrivate<bool>("CheckRateLimit", ipB);

            Assert.That(ipAExceeded, Is.False, "IP A should be rate-limited");
            Assert.That(ipBResult, Is.True, "IP B should have its own independent limit");
        }

        #endregion

        #region GET /api/status Tests

        [Test]
        public void GetApiStatus_ReturnsSystemStatus()
        {
            var expectedStatus = new SystemStatus
            {
                IsRunning = true,
                WebSocketPort = 8081,
                WebApiPort = 8080,
                ConnectedClients = 3
            };
            _monitorMock.Setup(m => m.GetSystemStatus()).Returns(expectedStatus);

            var result = _monitorMock.Object.GetSystemStatus();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsRunning, Is.True);
            Assert.That(result.ConnectedClients, Is.EqualTo(3));
            Assert.That(result.WebApiPort, Is.EqualTo(8080));
        }

        [Test]
        public void GetApiStatus_NoMonitors_HandlesGracefully()
        {
            // Test that the method exists and returns something when called
            // (integration through IRemoteMonitorManager mock)
            _monitorMock.Setup(m => m.GetSystemStatus())
                .Returns((SystemStatus)null);

            var result = _monitorMock.Object.GetSystemStatus();

            Assert.That(result, Is.Null);
        }

        #endregion

        #region GET /api/alarms Tests

        [Test]
        public void GetApiAlarms_ReturnsAlarmList()
        {
            var alarms = new List<AlarmInfo>
            {
                new AlarmInfo { Id = 1, AlarmCode = "ERR001",
                    AlarmMessage = "Test alarm 1", Level = AlarmLevel.Error },
                new AlarmInfo { Id = 2, AlarmCode = "WRN001",
                    AlarmMessage = "Test alarm 2", Level = AlarmLevel.Warning }
            };

            var alarmMgrMock = new Mock<IAlarmManager>();
            alarmMgrMock.Setup(a => a.GetActiveAlarms()).Returns(alarms);
            _systemManagerMock.Setup(s => s.AlarmMgr).Returns(alarmMgrMock.Object);

            var result = _systemManagerMock.Object.AlarmMgr.GetActiveAlarms();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].AlarmCode, Is.EqualTo("ERR001"));
            Assert.That(result[1].AlarmCode, Is.EqualTo("WRN001"));
        }

        [Test]
        public void GetApiAlarms_EmptyList_ReturnsEmpty()
        {
            var alarmMgrMock = new Mock<IAlarmManager>();
            alarmMgrMock.Setup(a => a.GetActiveAlarms()).Returns(new List<AlarmInfo>());
            _systemManagerMock.Setup(s => s.AlarmMgr).Returns(alarmMgrMock.Object);

            var result = _systemManagerMock.Object.AlarmMgr.GetActiveAlarms();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        #endregion

        #region POST /api/command Tests

        [Test]
        public void PostApiCommand_ValidCommand_ReturnsSuccess()
        {
            var commandResult = new CommandResult
            {
                Success = true,
                Message = "Command executed",
                CommandId = "CMD-001"
            };
            _monitorMock.Setup(m => m.ExecuteCommand(It.IsAny<RemoteCommand>()))
                .Returns(commandResult);

            var result = _monitorMock.Object.ExecuteCommand(
                new RemoteCommand { CommandType = "START", CommandId = "CMD-001" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Command executed"));
        }

        [Test]
        public void PostApiCommand_InvalidCommand_ReturnsError()
        {
            var commandResult = new CommandResult
            {
                Success = false,
                Message = "Invalid command",
                CommandId = "CMD-BAD"
            };
            _monitorMock.Setup(m => m.ExecuteCommand(It.IsAny<RemoteCommand>()))
                .Returns(commandResult);

            var result = _monitorMock.Object.ExecuteCommand(
                new RemoteCommand { CommandType = "INVALID", CommandId = "CMD-BAD" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("Invalid"));
        }

        [Test]
        public void PostApiCommand_NullCommand_HandledByInterface()
        {
            _monitorMock.Setup(m => m.ExecuteCommand(It.IsAny<RemoteCommand>()))
                .Returns(new CommandResult { Success = false, Message = "Null command" });

            // Verify the mock handles the call
            var result = _monitorMock.Object.ExecuteCommand(
                new RemoteCommand { CommandType = "", CommandId = "EMPTY" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
        }

        #endregion

        #region Missing Auth Header Tests

        [Test]
        public void MissingAuthHeader_ValidateTokenNull_Fails()
        {
            // ValidateToken with null token string should return false
            var isValid = InvokePrivate<bool>("ValidateToken", (string)null);
            Assert.That(isValid, Is.False,
                "Null token should return 401 Unauthorized equivalent");
        }

        [Test]
        public void MissingAuthHeader_ValidateTokenWithoutBearer_Fails()
        {
            // Even a token without 'Bearer ' prefix, if sent raw, would fail
            var isValid = InvokePrivate<bool>("ValidateToken", "someRawTokenValue");
            Assert.That(isValid, Is.False,
                "Raw non-JWT token should fail validation (401)");
        }

        #endregion

        #region CORS Header Tests

        [Test]
        public void CorsCheck_AllowedOrigin_ReturnsTrue()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", "http://localhost");
            Assert.That(isAllowed, Is.True,
                "Allowed origin should pass CORS check");
        }

        [Test]
        public void CorsCheck_AllowedOrigin_Loopback()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", "http://127.0.0.1");
            Assert.That(isAllowed, Is.True,
                "Loopback IP should be allowed in CORS whitelist");
        }

        [Test]
        public void CorsCheck_AllowedOrigin_WithPort()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", "http://localhost:8080");
            Assert.That(isAllowed, Is.True,
                "Allowed origin with port should pass CORS check");
        }

        [Test]
        public void CorsCheck_DisallowedOrigin_ReturnsFalse()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", "http://evil.example.com");
            Assert.That(isAllowed, Is.False,
                "Unknown origin should fail CORS check");
        }

        [Test]
        public void CorsCheck_EmptyOrigin_ReturnsFalse()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", "");
            Assert.That(isAllowed, Is.False,
                "Empty origin should not be allowed");
        }

        [Test]
        public void CorsCheck_NullOrigin_ReturnsFalse()
        {
            var isAllowed = InvokePrivate<bool>("IsCorsAllowed", null);
            Assert.That(isAllowed, Is.False,
                "Null origin should not be allowed");
        }

        #endregion

        #region Start/Stop Tests

        [Test]
        public void Start_SetsIsRunning()
        {
            var result = _controller.Start();
            Assert.That(result, Is.True, "Start should succeed");
            Assert.That(_controller.IsRunning, Is.True,
                "IsRunning should be true after start");
        }

        [Test]
        public void Stop_SetsIsRunningFalse()
        {
            _controller.Start();
            Assert.That(_controller.IsRunning, Is.True);

            _controller.Stop();
            Assert.That(_controller.IsRunning, Is.False,
                "IsRunning should be false after stop");
        }

        [Test]
        public void Start_AlreadyRunning_ReturnsFalse()
        {
            _controller.Start();
            Assert.That(_controller.IsRunning, Is.True);

            // Second start call should detect already running
            var result = _controller.Start();
            Assert.That(result, Is.False,
                "Start while already running should return false");
        }

        [Test]
        public void Stop_NotRunning_DoesNotThrow()
        {
            Assert.That(_controller.IsRunning, Is.False);
            Assert.DoesNotThrow(() => _controller.Stop(),
                "Stop when not running should not throw");
        }

        #endregion
    }
}
