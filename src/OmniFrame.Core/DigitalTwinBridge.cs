using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    public class DigitalTwinBridge : IDisposable
    {
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private readonly string _bridgeUrl;
        private int _reconnectAttempts;
        private const int MaxReconnectAttempts = 5;
        private const int InitialReconnectDelay = 1000;
        private const int MaxReconnectDelay = 30000;

        public event Action<string, JsonElement?> CommandReceived;
        public event Action<ConnectionState> ConnectionStateChanged;

        public bool IsConnected => _ws?.State == WebSocketState.Open;
        public ConnectionState CurrentState { get; private set; } = ConnectionState.Disconnected;

        public DigitalTwinBridge(string url = "ws://localhost:3001")
        {
            _bridgeUrl = url;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            try
            {
                UpdateState(ConnectionState.Connecting);
                _cts = new CancellationTokenSource();
                _ws = new ClientWebSocket();
                var fullUrl = $"{_bridgeUrl}?role=upper-computer";
                await _ws.ConnectAsync(new Uri(fullUrl), _cts.Token);
                UpdateState(ConnectionState.Connected);
                _reconnectAttempts = 0;

                _ = ReceiveLoopAsync(_cts.Token);
                await SendFullSyncAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("WebSocket连接失败", ex);
                UpdateState(ConnectionState.Disconnected);
                _ = ReconnectAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                }
                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("关闭WebSocket连接失败", ex);
            }
            finally
            {
                UpdateState(ConnectionState.Disconnected);
                _ws = null;
                _cts = null;
            }
        }

        private async Task ReconnectAsync()
        {
            if (CurrentState == ConnectionState.Connected)
                return;

            int delay = Math.Min(InitialReconnectDelay * (1 << _reconnectAttempts), MaxReconnectDelay);
            _reconnectAttempts++;

            Logger.Warning($"尝试重连WebSocket，延迟 {delay}ms，尝试次数 {_reconnectAttempts}");
            UpdateState(ConnectionState.Reconnecting);

            await Task.Delay(delay);
            await ConnectAsync();
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[4096];
            var sb = new StringBuilder();

            while (!ct.IsCancellationRequested && IsConnected)
            {
                try
                {
                    sb.Clear();
                    WebSocketReceiveResult result;
                    do
                    {
                        var segment = new ArraySegment<byte>(buffer);
                        result = await _ws.ReceiveAsync(segment, ct);
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    } while (!result.EndOfMessage);

                    var msg = JsonSerializer.Deserialize<JsonNode>(sb.ToString());
                    if (msg?["type"]?.ToString() == "command")
                    {
                        var action = msg["action"]?.ToString() ?? "";
                        var param = msg["params"];
                        CommandReceived?.Invoke(action, null);
                        Logger.Info($"收到命令: {action}");
                    }
                }
                catch (Exception ex)
                {
                    if (!ct.IsCancellationRequested)
                    {
                        Logger.Error("接收消息失败", ex);
                        UpdateState(ConnectionState.Disconnected);
                        _ = ReconnectAsync();
                    }
                    break;
                }
            }
        }

        public async Task SendAxisUpdateAsync(Dictionary<string, double> axes)
        {
            if (!IsConnected)
                return;

            var msg = new { type = "axis-update", data = axes };
            await SendJsonAsync(msg);
        }

        public async Task SendIoUpdateAsync(Dictionary<string, bool> ioStates)
        {
            if (!IsConnected)
                return;

            var msg = new { type = "io-update", data = ioStates };
            await SendJsonAsync(msg);
        }

        public async Task SendMachineStateAsync(string state, bool red, bool yellow, bool green)
        {
            if (!IsConnected)
                return;

            var msg = new
            {
                type = "machine-state",
                data = new
                {
                    state,
                    lights = new { red, yellow, green }
                }
            };
            await SendJsonAsync(msg);
        }

        public async Task SendFullSyncAsync(Dictionary<string, double> axes = null, Dictionary<string, bool> ioStates = null, string machineState = null, bool? red = null, bool? yellow = null, bool? green = null)
        {
            if (!IsConnected)
                return;

            var msg = new
            {
                type = "full-sync",
                data = new
                {
                    axes = axes ?? new Dictionary<string, double> { { "X", 0 }, { "Y", 0 }, { "Z", 0 }, { "Theta", 0 } },
                    io = ioStates ?? new Dictionary<string, bool> { { "VacDown", false }, { "Blow", false }, { "Clamp", false } },
                    state = machineState ?? "idle",
                    lights = new { red = red ?? false, yellow = yellow ?? false, green = green ?? true }
                }
            };
            await SendJsonAsync(msg);
        }

        private async Task SendJsonAsync(object obj)
        {
            try
            {
                var json = JsonSerializer.Serialize(obj);
                var bytes = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(bytes);
                await _ws.SendAsync(segment, WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                Logger.Error("发送消息失败", ex);
                UpdateState(ConnectionState.Disconnected);
                _ = ReconnectAsync();
            }
        }

        private void UpdateState(ConnectionState state)
        {
            CurrentState = state;
            ConnectionStateChanged?.Invoke(state);
        }

        public void Dispose()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                }
                if (_ws != null)
                {
                    if (_ws.State == WebSocketState.Open)
                    {
                        var task = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        task.GetAwaiter().GetResult();
                    }
                    _ws.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Dispose WebSocket 失败", ex);
            }
            finally
            {
                UpdateState(ConnectionState.Disconnected);
                _ws = null;
                _cts = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
