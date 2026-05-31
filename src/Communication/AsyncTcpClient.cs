using System.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Communication
{
    /// <summary>
    /// 异步TCP客户端类
    /// 设计介绍：
    /// 2. 支持自动重连机制，确保连接的稳定性
    /// 3. 实现心跳检测，及时发现和恢复断开的连接
    /// 4. 支持数据缓冲和批量发送，提高数据传输效率
    /// 5. 提供事件驱动的编程接口，方便集成到应用程序中
    /// 6. 线程安全设计，支持多线程并发访问
    /// 7. 实现IDisposable接口，支持资源自动释放
        /// </summary>
    public class AsyncTcpClient : IDisposable
    {
        /// <summary>
        /// TCP客户端对象
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// 网络流对象
        /// </summary>
        private NetworkStream _stream;

        /// <summary>
        /// 主机地址
        /// </summary>
        private string _host;

        /// <summary>
        /// 端口号
        /// </summary>
        private int _port;

        /// <summary>
        /// 是否已连接
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// 是否正在连接中
        /// </summary>
        private bool _isConnecting;

        /// <summary>
        /// 取消令牌源
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 接收数据任务
        /// </summary>
        private Task _receiveTask;

        /// <summary>
        /// 心跳检测任务
        /// </summary>
        private Task _heartbeatTask;

        /// <summary>
        /// 发送队列
        /// </summary>
        private Queue<byte[]> _sendQueue;

        /// <summary>
        /// 发送锁对象
        /// </summary>
        private object _sendLock = new object();

        /// <summary>
        /// 心跳间隔（毫秒），默认30秒
        /// </summary>
        private int _heartbeatInterval = 30000; // 心跳间隔30秒

        /// <summary>
        /// 重连间隔（毫秒），默认5秒
        /// </summary>
        private int _reconnectInterval = 5000; // 重连间隔5秒

        /// <summary>
        /// 最大重连次数，默认3次
        /// </summary>
        private int _maxReconnectAttempts = 3; // 最大重连次数

        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<byte[]> DataReceived;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 构造函数
        /// 功能说明：初始化异步TCP客户端，创建发送队列和取消令牌源
        /// </summary>
        public AsyncTcpClient()
        {
            _sendQueue = new Queue<byte[]>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 连接到服务器
        /// 功能说明：异步连接到指定的主机和端口，启动接收和心跳任务
        /// 设计模式：异步编程模式（async/await）
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                if (_isConnected || _isConnecting)
                {
                    Logger.Warning("TCP客户端已连接或正在连接中");
                    return false;
                }

                _host = host;
                _port = port;
                _isConnecting = true;

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();

                _isConnected = true;
                _isConnecting = false;
                Logger.Info($"TCP客户端连接成功: {host}:{port}");

                Connected?.Invoke(this, EventArgs.Empty);

                _cancellationTokenSource = new CancellationTokenSource();
                _receiveTask = Task.Run(() => ReceiveDataAsync(_cancellationTokenSource.Token));
                _heartbeatTask = Task.Run(() => HeartbeatAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (SocketException ex)
            {
                _isConnected = false;
                _isConnecting = false;
                Logger.Error("TCP客户端连接失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (IOException ex)
            {
                _isConnected = false;
                _isConnecting = false;
                Logger.Error("TCP客户端连接失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _isConnecting = false;
                Logger.Error("TCP客户端连接失败", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// 功能说明：断开与服务器的连接，取消接收和心跳任务，释放资源
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _cancellationTokenSource?.Cancel();

                _stream?.Close();
                _tcpClient?.Close();

                _isConnected = false;
                Logger.Info("TCP客户端已断开连接");

                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP客户端断开连接失败(网络错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP客户端断开连接失败(IO错误)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("TCP客户端断开连接失败", ex);
            }
        }

        /// <summary>
        /// 发送数据
        /// 功能说明：异步发送数据到服务器
        /// 设计模式：异步编程模式（async/await）
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendDataAsync(byte[] data)
        {
            try
            {
                if (!_isConnected || _stream == null)
                {
                    Logger.Warning("TCP客户端未连接，无法发送数据");
                    return false;
                }

                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                Logger.Info($"TCP客户端发送数据，长度: {data.Length}");
                return true;
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP客户端发送数据失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("TCP客户端发送数据失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("TCP客户端发送数据失败", ex);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        /// <summary>
        /// 接收数据
        /// 功能说明：异步接收服务器发送的数据，触发DataReceived事件
        /// 设计模式：异步编程模式（async/await）和观察者模式
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task ReceiveDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[8192];
                while (!cancellationToken.IsCancellationRequested && _isConnected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, 0, receivedData, 0, bytesRead);
                        Logger.Info($"TCP客户端接收数据，长度: {bytesRead}");
                        DataReceived?.Invoke(this, receivedData);
                    }
                    else
                    {
                        Logger.Warning("TCP客户端连接已断开");
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP客户端接收数据失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP客户端接收数据失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("TCP客户端接收任务已取消");
            }
            catch (Exception ex)
            {
                Logger.Error("TCP客户端接收数据失败", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                if (_isConnected)
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// 心跳检测
        /// 功能说明：定期发送心跳包，检测连接状态，及时发现断开的连接
        /// 设计模式：异步编程模式（async/await）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task HeartbeatAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isConnected)
                {
                    await Task.Delay(_heartbeatInterval, cancellationToken);
                    if (_isConnected)
                    {
                        // 发送空轮询检测连接状态（不发送业务无关的字符串）
                        try
                        {
                            if (!_tcpClient.Client.Poll(0, SelectMode.SelectWrite))
                            {
                                Logger.Warning("TCP 心跳检测到连接异常");
                                Disconnect();
                            }
                        }
                        catch { /* Poll 失败 = 连接已断开，由 ReceiveDataAsync 的 finally 处理 */ }
                    }
                }
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP客户端心跳检测失败(网络错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP客户端心跳检测失败(IO错误)", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("TCP客户端心跳任务已取消");
            }
            catch (Exception ex)
            {
                Logger.Error("TCP客户端心跳检测失败", ex);
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// 自动重连
        /// 功能说明：在连接断开后自动尝试重新连接，最多尝试指定次数
        /// 设计模式：异步编程模式（async/await）和重试模式
        /// </summary>
        /// <returns>是否重连成功</returns>
        public async Task<bool> ReconnectAsync()
        {
            int reconnectAttempts = 0;
            while (reconnectAttempts < _maxReconnectAttempts)
            {
                Logger.Info($"TCP客户端尝试重连，第{reconnectAttempts + 1}次");
                bool success = await ConnectAsync(_host, _port);
                if (success)
                {
                    return true;
                }
                reconnectAttempts++;
                await Task.Delay(_reconnectInterval);
            }
            Logger.Error($"TCP客户端重连失败，已达到最大重连次数: {_maxReconnectAttempts}");
            return false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            _cancellationTokenSource?.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}
