using System.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Communication
{
    /// <summary>
    /// 异步TCP服务器类
    /// 设计介绍：
    /// 2. 实现客户端连接管理，支持客户端的连接、断开和状态监控
    /// 3. 支持消息广播和点对点消息发送
    /// 4. 实现连接限流和负载控制，防止服务器过载
    /// 5. 提供事件驱动的编程接口，方便集成到应用程序中
    /// 6. 线程安全设计，支持多线程并发访问
        /// </summary>
    public class AsyncTcpServer : IDisposable
    {
        private TcpListener _tcpListener;
        private int _port;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, TcpClientInfo> _clients;
        private int _maxConnections = 100;

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event EventHandler<TcpClientInfo> ClientConnected;

        /// <summary>
        /// 客户端断开事件
        /// </summary>
        public event EventHandler<TcpClientInfo> ClientDisconnected;

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

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
        public AsyncTcpServer(int port)
        {
            _port = port;
            _clients = new ConcurrentDictionary<string, TcpClientInfo>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <returns>是否启动成功</returns>
        public async Task<bool> StartAsync()
        {
            try
            {
                if (_isRunning)
                {
                    Logger.Warning("TCP服务器已在运行中");
                    return false;
                }

                _tcpListener = new TcpListener(IPAddress.Any, _port);
                _tcpListener.Start();
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                Logger.Info($"TCP服务器启动成功，监听端口: {_port}");

                // 启动接受客户端的任务，不需要等待完成
                _ = AcceptClientsAsync(_cancellationTokenSource.Token);

                return true;
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP服务器启动失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("TCP服务器启动失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("TCP服务器启动失败(连接已断开)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("TCP服务器启动失败", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            try
            {
                if (!_isRunning)
                {
                    return;
                }

                _cancellationTokenSource?.Cancel();
                _tcpListener?.Stop();

                foreach (var client in _clients.Values)
                {
                    client.TcpClient?.Close();
                }
                _clients.Clear();

                _isRunning = false;
                Logger.Info("TCP服务器已停止");
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP服务器停止失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP服务器停止失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("TCP服务器停止失败(连接已断开)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (Exception ex)
            {
                Logger.Error("TCP服务器停止失败", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// 接受客户端连接
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    if (_clients.Count >= _maxConnections)
                    {
                        Logger.Warning("TCP服务器连接数已达上限，拒绝新连接");
                        tcpClient.Close();
                        continue;
                    }

                    string clientId = GenerateClientId();
                    TcpClientInfo clientInfo = new TcpClientInfo
                    {
                        ClientId = clientId,
                        TcpClient = tcpClient,
                        ConnectTime = DateTime.Now,
                        LastActiveTime = DateTime.Now
                    };

                    _clients.TryAdd(clientId, clientInfo);
                    Logger.Info($"客户端连接成功，ID: {clientId}，当前连接数: {_clients.Count}");

                    ClientConnected?.Invoke(this, clientInfo);

                    // 启动处理客户端的任务，不需要等待完成
                    _ = HandleClientAsync(clientInfo, cancellationToken);
                }
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP服务器接受客户端失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP服务器接受客户端失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("TCP服务器接受客户端失败(连接已断开)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("TCP服务器接受客户端任务已取消");
            }
            catch (Exception ex)
            {
                Logger.Error("TCP服务器接受客户端失败", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// 处理客户端连接
        /// </summary>
        /// <param name="clientInfo">客户端信息</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task HandleClientAsync(TcpClientInfo clientInfo, CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream stream = clientInfo.TcpClient.GetStream();
                byte[] buffer = new byte[8192];

                while (!cancellationToken.IsCancellationRequested && clientInfo.TcpClient.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, 0, receivedData, 0, bytesRead);
                        clientInfo.LastActiveTime = DateTime.Now;

                        Logger.Info($"接收客户端数据，ID: {clientInfo.ClientId}，长度: {bytesRead}");
                        DataReceived?.Invoke(this, new TcpDataReceivedEventArgs
                        {
                            ClientId = clientInfo.ClientId,
                            Data = receivedData
                        });
                    }
                    else
                    {
                        Logger.Warning($"客户端连接已断开，ID: {clientInfo.ClientId}");
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Logger.Error($"处理客户端失败(网络错误)，ID: {clientInfo.ClientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"处理客户端失败(IO错误)，ID: {clientInfo.ClientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"处理客户端失败(连接已断开)，ID: {clientInfo.ClientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"处理客户端任务已取消，ID: {clientInfo.ClientId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"处理客户端失败，ID: {clientInfo.ClientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                RemoveClient(clientInfo.ClientId);
            }
        }

        /// <summary>
        /// 发送数据到指定客户端
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="data">要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendDataAsync(string clientId, byte[] data)
        {
            try
            {
                if (_clients.TryGetValue(clientId, out TcpClientInfo clientInfo))
                {
                    NetworkStream stream = clientInfo.TcpClient.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                    clientInfo.LastActiveTime = DateTime.Now;
                    Logger.Info($"发送数据到客户端，ID: {clientId}，长度: {data.Length}");
                    return true;
                }
                else
                {
                    Logger.Warning($"客户端不存在，ID: {clientId}");
                    return false;
                }
            }
            catch (SocketException ex)
            {
                Logger.Error($"发送数据失败(网络错误)，ID: {clientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error($"发送数据失败(IO错误)，ID: {clientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"发送数据失败(连接已断开)，ID: {clientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"发送数据失败，ID: {clientId}", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        /// <summary>
        /// 广播数据到所有客户端
        /// </summary>
        /// <param name="data">要广播的数据</param>
        /// <returns>发送成功的客户端数量</returns>
        public async Task<int> BroadcastAsync(byte[] data)
        {
            int successCount = 0;
            foreach (var client in _clients.Values)
            {
                bool success = await SendDataAsync(client.ClientId, data);
                if (success)
                {
                    successCount++;
                }
            }
            Logger.Info($"广播数据完成，成功发送到 {successCount}/{_clients.Count} 个客户端");
            return successCount;
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        private void RemoveClient(string clientId)
        {
            try
            {
                if (_clients.TryRemove(clientId, out TcpClientInfo clientInfo))
                {
                    clientInfo.TcpClient?.Close();
                    Logger.Info($"客户端已断开，ID: {clientId}，当前连接数: {_clients.Count}");
                    ClientDisconnected?.Invoke(this, clientInfo);
                }
            }
            catch (SocketException ex)
            {
                Logger.Error($"移除客户端失败(网络错误)，ID: {clientId}", ex);
            }
            catch (IOException ex)
            {
                Logger.Error($"移除客户端失败(IO错误)，ID: {clientId}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"移除客户端失败(连接已断开)，ID: {clientId}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"移除客户端失败，ID: {clientId}", ex);
            }
        }

        /// <summary>
        /// 生成客户端ID
        /// </summary>
        /// <returns>客户端ID</returns>
        private string GenerateClientId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 获取所有客户端信息
        /// </summary>
        /// <returns>客户端信息列表</returns>
        public List<TcpClientInfo> GetAllClients()
        {
            return _clients.Values.ToList();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// TCP客户端信息
        /// </summary>
    public class TcpClientInfo
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// TCP客户端
        /// </summary>
        public TcpClient TcpClient { get; set; }

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
    /// TCP数据接收事件参数
        /// </summary>
    public class TcpDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 接收的数据
        /// </summary>
        public byte[] Data { get; set; }
    }
}
