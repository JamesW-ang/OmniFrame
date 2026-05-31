using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace RemoteMonitor
{
    /// <summary>
    /// WebSocket服务器类
    /// 设计介绍：
    /// 1. 实现WebSocket协议，支持实时双向通信
    /// 2. 支持多客户端连接，实现并发数据推送
    /// 3. 提供消息广播和点对点消息发送功能
    /// 4. 实现心跳检测，及时发现和清理断开的连接
    /// 5. 支持消息压缩和优化，提高数据传输效率
    /// 6. 线程安全设计，支持多线程并发访问
        /// </summary>
    [Obsolete("Hand-rolled WebSocket server. Consider migrating to OWIN/Katana for production.")]
    public class WebSocketServer
    {
        private HttpListener _httpListener;
        private int _port;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, WebSocketClient> _clients;
        private const int MaxConnections = 100;

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event EventHandler<string> ClientConnected;

        /// <summary>
        /// 客户端断开事件
        /// </summary>
        public event EventHandler<string> ClientDisconnected;

        /// <summary>
        /// 消息接收事件
        /// </summary>
        public event EventHandler<WebSocketMessageEventArgs> MessageReceived;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 当前连接数
        /// </summary>
        public int ConnectionCount => _clients.Count;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听端口</param>
        public WebSocketServer(int port)
        {
            _port = port;
            _clients = new ConcurrentDictionary<string, WebSocketClient>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 启动WebSocket服务器
        /// </summary>
        /// <returns>是否启动成功</returns>
        public bool Start()
        {
            try
            {
                if (_isRunning)
                {
                    Logger.Warning("WebSocket服务器已在运行中");
                    return false;
                }

                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://+:{_port}/");
                _httpListener.Start();
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                Logger.Info($"WebSocket服务器启动成功，监听端口: {_port}");

                Task.Run(() => AcceptConnectionsAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error("WebSocket服务器启动失败(WebSocket错误)", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("WebSocket服务器启动失败(IO错误)", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("WebSocket服务器启动失败(连接已断开)", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("WebSocket服务器启动失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止WebSocket服务器
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                if (!_isRunning)
                {
                    return;
                }

                _cancellationTokenSource?.Cancel();
                _httpListener?.Stop();

                var tasks = new List<Task>();
                foreach (var client in _clients.Values)
                {
                    if (client.WebSocket != null && client.WebSocket.State == WebSocketState.Open)
                    {
                        tasks.Add(client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopping", CancellationToken.None));
                    }
                }
                await Task.WhenAll(tasks);
                _clients.Clear();

                _isRunning = false;
                Logger.Info("WebSocket服务器已停止");
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error("WebSocket服务器停止失败(WebSocket错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("WebSocket服务器停止失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("WebSocket服务器停止失败(连接已断开)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("WebSocket服务器停止失败", ex);
            }
        }

        /// <summary>
        /// 接受客户端连接
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    
                    if (context.Request.IsWebSocketRequest)
                    {
                        // Enforce connection limit
                        if (_clients.Count >= MaxConnections)
                        {
                            Logger.Warning($"WebSocket connection rejected: max connections ({MaxConnections}) reached");
                            context.Response.StatusCode = 503;
                            context.Response.StatusDescription = "Service Unavailable: Too many connections";
                            context.Response.Close();
                            continue;
                        }
                        _ = Task.Run(async () => await HandleWebSocketConnectionAsync(context, cancellationToken));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error("WebSocket服务器接受连接失败(WebSocket错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("WebSocket服务器接受连接失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("WebSocket服务器接受连接失败(连接已断开)", ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("WebSocket服务器接受连接任务已取消");
            }
            catch (Exception ex)
            {
                Logger.Error("WebSocket服务器接受连接失败", ex);
            }
        }

        /// <summary>
        /// 处理WebSocket连接
        /// </summary>
        /// <param name="context">HTTP监听器上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task HandleWebSocketConnectionAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            WebSocket webSocket = null;
            string clientId = null;
            CancellationTokenSource clientCts = null;

            try
            {
                WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
                webSocket = webSocketContext.WebSocket;
                clientId = Guid.NewGuid().ToString("N");
                clientCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                WebSocketClient client = new WebSocketClient
                {
                    ClientId = clientId,
                    WebSocket = webSocket,
                    ConnectTime = DateTime.Now,
                    LastActiveTime = DateTime.Now
                };

                _clients.TryAdd(clientId, client);
                Logger.Info($"WebSocket客户端连接成功，ID: {clientId}，当前连接数: {_clients.Count}");

                ClientConnected?.Invoke(this, clientId);

                // 启动心跳检测
                var heartbeatTask = Task.Run(async () => await HeartbeatAsync(client, clientCts.Token));
                // 接收消息
                var receiveTask = ReceiveMessagesAsync(client, clientCts.Token);

                // 等待任一任务完成
                await Task.WhenAny(heartbeatTask, receiveTask);

                // 取消客户端的所有任务
                clientCts.Cancel();
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"WebSocket客户端连接失败(WebSocket错误)，ID: {clientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"WebSocket客户端连接失败(IO错误)，ID: {clientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"WebSocket客户端连接失败(连接已断开)，ID: {clientId}", ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"WebSocket客户端连接任务已取消，ID: {clientId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"WebSocket客户端连接失败，ID: {clientId}", ex);
            }
            finally
            {
                clientCts?.Cancel();
                clientCts?.Dispose();
                await RemoveClientAsync(clientId);
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="client">WebSocket客户端</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task ReceiveMessagesAsync(WebSocketClient client, CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[8192];
                while (!cancellationToken.IsCancellationRequested && client.WebSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        client.LastActiveTime = DateTime.Now;

                        // Input validation: reject empty or excessively large messages
                        if (string.IsNullOrEmpty(message))
                        {
                            Logger.Warning($"WebSocket empty message received, ID: {client.ClientId}");
                            continue;
                        }
                        if (message.Length > 65536) // 64KB max
                        {
                            Logger.Warning($"WebSocket oversized message ({message.Length} bytes), ID: {client.ClientId}");
                            await client.WebSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message too large", cancellationToken);
                            break;
                        }

                        // 处理心跳消息
                        if (message == "Ping")
                        {
                            Logger.Debug($"WebSocket ping received, ID: {client.ClientId}");
                            // 回复Pong
                            await SendPongAsync(client, cancellationToken);
                            continue;
                        }

                        Logger.Info($"接收WebSocket消息，ID: {client.ClientId}，内容: {message}");
                        MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
                        {
                            ClientId = client.ClientId,
                            Message = message
                        });
                    }
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"WebSocket接收消息失败(WebSocket错误)，ID: {client.ClientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"WebSocket接收消息失败(IO错误)，ID: {client.ClientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"WebSocket接收消息失败(连接已断开)，ID: {client.ClientId}", ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"WebSocket接收消息任务已取消，ID: {client.ClientId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"WebSocket接收消息失败，ID: {client.ClientId}", ex);
            }
        }

        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <param name="client">WebSocket客户端</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task HeartbeatAsync(WebSocketClient client, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && client.WebSocket.State == WebSocketState.Open)
                {
                    // 检查最后活动时间
                    if ((DateTime.Now - client.LastActiveTime).TotalSeconds > 60)
                    {
                        // 60秒无心跳，主动断开连接
                        Logger.Warning($"WebSocket客户端无心跳，ID: {client.ClientId}，断开连接");
                        await client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "No heartbeat", cancellationToken);
                        break;
                    }

                    // 每30秒检查一次
                    await Task.Delay(30000, cancellationToken);
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"WebSocket心跳检测失败(WebSocket错误)，ID: {client.ClientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"WebSocket心跳检测失败(IO错误)，ID: {client.ClientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"WebSocket心跳检测失败(连接已断开)，ID: {client.ClientId}", ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"WebSocket心跳任务已取消，ID: {client.ClientId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"WebSocket心跳检测失败，ID: {client.ClientId}", ex);
            }
        }

        /// <summary>
        /// 发送Pong消息
        /// </summary>
        /// <param name="client">WebSocket客户端</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task SendPongAsync(WebSocketClient client, CancellationToken cancellationToken)
        {
            try
            {
                if (client.WebSocket.State == WebSocketState.Open)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes("Pong");
                    await client.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
                    client.LastActiveTime = DateTime.Now;
                    Logger.Debug($"发送Pong消息，ID: {client.ClientId}");
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"发送Pong消息失败(WebSocket错误)，ID: {client.ClientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"发送Pong消息失败(IO错误)，ID: {client.ClientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"发送Pong消息失败(连接已断开)，ID: {client.ClientId}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"发送Pong消息失败，ID: {client.ClientId}", ex);
            }
        }

        /// <summary>
        /// 发送消息到指定客户端
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="message">要发送的消息</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendMessageAsync(string clientId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(message))
                {
                    Logger.Warning("SendMessageAsync rejected: empty clientId or message");
                    return false;
                }
                if (message.Length > 65536)
                {
                    Logger.Warning($"SendMessageAsync rejected: message too large ({message.Length} bytes)");
                    return false;
                }
                if (_clients.TryGetValue(clientId, out WebSocketClient client))
                {
                    if (client.WebSocket.State == WebSocketState.Open)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        await client.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        client.LastActiveTime = DateTime.Now;
                        Logger.Info($"发送WebSocket消息，ID: {clientId}，内容: {message}");
                        return true;
                    }
                    else
                    {
                        Logger.Warning($"WebSocket客户端未连接，ID: {clientId}");
                        return false;
                    }
                }
                else
                {
                    Logger.Warning($"WebSocket客户端不存在，ID: {clientId}");
                    return false;
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"发送WebSocket消息失败(WebSocket错误)，ID: {clientId}", ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error($"发送WebSocket消息失败(IO错误)，ID: {clientId}", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"发送WebSocket消息失败(连接已断开)，ID: {clientId}", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"发送WebSocket消息失败，ID: {clientId}", ex);
                return false;
            }
        }

        /// <summary>
        /// 广播消息到所有客户端
        /// </summary>
        /// <param name="message">要广播的消息</param>
        /// <returns>发送成功的客户端数量</returns>
        public async Task<int> BroadcastAsync(string message)
        {
            int successCount = 0;
            foreach (var client in _clients.Values)
            {
                bool success = await SendMessageAsync(client.ClientId, message);
                if (success)
                {
                    successCount++;
                }
            }
            Logger.Info($"广播WebSocket消息完成，成功发送到 {successCount}/{_clients.Count} 个客户端");
            return successCount;
        }

        /// <summary>
        /// 广播实时数据到所有客户端
        /// </summary>
        /// <param name="data">实时数据</param>
        /// <returns>发送成功的客户端数量</returns>
        public async Task<int> Broadcast(RealTimeData data)
        {
            try
            {
                string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                return await BroadcastAsync(jsonMessage);
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error("广播实时数据失败(WebSocket错误)", ex);
                return 0;
            }
            catch (IOException ex)
            {
                Logger.Error("广播实时数据失败(IO错误)", ex);
                return 0;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("广播实时数据失败(连接已断开)", ex);
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("广播实时数据失败", ex);
                return 0;
            }
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        private async Task RemoveClientAsync(string clientId)
        {
            try
            {
                if (_clients.TryRemove(clientId, out WebSocketClient client))
                {
                    if (client.WebSocket != null)
                    {
                        WebSocketState state = client.WebSocket.State;
                        if (state == WebSocketState.Open || state == WebSocketState.CloseReceived)
                        {
                            try
                            {
                                // Perform proper close handshake with timeout
                                using (var closeCts = new CancellationTokenSource(5000))
                                {
                                    await client.WebSocket.CloseAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Server closing connection",
                                        closeCts.Token);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Logger.Warning($"WebSocket close handshake timed out, ID: {clientId}");
                            }
                        }
                        else if (state == WebSocketState.Aborted)
                        {
                            Logger.Warning($"WebSocket connection was aborted, ID: {clientId}");
                        }
                        try { client.WebSocket.Dispose(); } catch { /* best effort */ }
                    }
                    Logger.Info($"WebSocket客户端已断开，ID: {clientId}，当前连接数: {_clients.Count}");
                    ClientDisconnected?.Invoke(this, clientId);
                }
            }
            catch (System.Net.WebSockets.WebSocketException ex)
            {
                Logger.Error($"移除WebSocket客户端失败(WebSocket错误)，ID: {clientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"移除WebSocket客户端失败(IO错误)，ID: {clientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"移除WebSocket客户端失败(连接已断开)，ID: {clientId}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"移除WebSocket客户端失败，ID: {clientId}", ex);
            }
        }

        /// <summary>
        /// 获取所有客户端信息
        /// </summary>
        /// <returns>客户端信息列表</returns>
        public List<WebSocketClient> GetAllClients()
        {
            return _clients.Values.ToList();
        }
    }

    /// <summary>
    /// WebSocket客户端
        /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// WebSocket连接
        /// </summary>
        public WebSocket WebSocket { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; set; }
    }

    /// <summary>
    /// WebSocket消息事件参数
        /// </summary>
    public class WebSocketMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
    }
}
