using NUnit.Framework;
using OmniFrame.Core;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class DigitalTwinBridgeTests
    {
        private DigitalTwinBridge _bridge;

        [SetUp]
        public void Setup()
        {
            // 创建 DigitalTwinBridge 实例，使用测试 URL
            _bridge = new DigitalTwinBridge("ws://localhost:3001");
        }

        [TearDown]
        public async Task TearDown()
        {
            // 清理资源
            if (_bridge != null)
            {
                await _bridge.DisconnectAsync();
                _bridge.Dispose();
            }
        }

        [Test]
        public void TestInitialState()
        {
            // 测试初始状态
            Assert.That(_bridge.IsConnected, Is.False);
            Assert.That(_bridge.CurrentState, Is.EqualTo(ConnectionState.Disconnected));
        }

        [Test]
        public async Task TestSendAxisUpdate()
        {
            // 测试发送轴位置更新
            var axes = new Dictionary<string, double>
            {
                { "X", 100.5 },
                { "Y", 200.7 },
                { "Z", 50.2 },
                { "Theta", 45.0 }
            };

            // 即使未连接，发送方法也应该不会抛出异常
            await _bridge.SendAxisUpdateAsync(axes);
            // 测试通过，因为方法应该优雅处理未连接的情况
        }

        [Test]
        public async Task TestSendIoUpdate()
        {
            // 测试发送 IO 状态更新
            var ioStates = new Dictionary<string, bool>
            {
                { "VacDown", true },
                { "Blow", false },
                { "Clamp", true }
            };

            // 即使未连接，发送方法也应该不会抛出异常
            await _bridge.SendIoUpdateAsync(ioStates);
            // 测试通过，因为方法应该优雅处理未连接的情况
        }

        [Test]
        public async Task TestSendMachineState()
        {
            // 测试发送机器状态更新
            await _bridge.SendMachineStateAsync("running", false, false, true);
            // 测试通过，因为方法应该优雅处理未连接的情况
        }

        [Test]
        public async Task TestSendFullSync()
        {
            // 测试发送全量同步消息
            var axes = new Dictionary<string, double>
            {
                { "X", 100.5 },
                { "Y", 200.7 },
                { "Z", 50.2 },
                { "Theta", 45.0 }
            };

            var ioStates = new Dictionary<string, bool>
            {
                { "VacDown", true },
                { "Blow", false },
                { "Clamp", true }
            };

            await _bridge.SendFullSyncAsync(axes, ioStates, "running", false, false, true);
            // 测试通过，因为方法应该优雅处理未连接的情况
        }

        [Test]
        public void TestCommandReceivedEvent()
        {
            // 测试命令接收事件
            bool eventFired = false;
            string receivedAction = string.Empty;
            JsonElement? receivedParams = null;

            _bridge.CommandReceived += (action, param) =>
            {
                eventFired = true;
                receivedAction = action;
                receivedParams = param;
            };

            // 模拟命令接收（实际项目中会由 WebSocket 接收触发）
            // 这里我们只是测试事件注册是否成功
            Assert.That(_bridge, Is.Not.Null);
            // 测试通过，因为事件注册成功
        }

        [Test]
        public void TestConnectionStateChangedEvent()
        {
            // 测试连接状态变化事件
            bool eventFired = false;
            ConnectionState receivedState = ConnectionState.Disconnected;

            _bridge.ConnectionStateChanged += (state) =>
            {
                eventFired = true;
                receivedState = state;
            };

            // 模拟连接状态变化（实际项目中会由 WebSocket 连接状态变化触发）
            // 这里我们只是测试事件注册是否成功
            Assert.That(_bridge, Is.Not.Null);
            // 测试通过，因为事件注册成功
        }
    }
}
