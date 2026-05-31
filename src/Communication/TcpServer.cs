using System;
using System.Collections.Generic;
using System.IO;
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
    /// TCP服务器
        /// </summary>
    public class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private List<TcpClient> _clients;
        private object _lock = new object();
        private int _port;

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event EventHandler<TcpClient> ClientConnected;
        /// <summary>
        /// 客户端断开事件
        /// </summary>
        public event EventHandler<TcpClient> ClientDisconnected;
        /// <summary>
        /// 接收数据事件
        /// </summary>
        public event EventHandler<byte[]> DataReceived;

        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
        }

        /// <summary>
        /// 客户端数量
        /// </summary>
        public int ClientCount
        {
            get { return _clients.Count; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">端口</param>
        public TcpServer(int port)
        {
            _port = port;
            _clients = new List<TcpClient>();
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;
                
                // 开始接受客户端连接
                Task.Run(() => AcceptClients());
                
                Logger.Info($"TCP服务器启动成功，监听端口: {_port}");
                return true;
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP服务器启动失败(端口绑定错误)", ex);
                _isRunning = false;
                return false;
            }
            catch (IOException ex)
            {
                Logger.Error("TCP服务器启动失败(IO错误)", ex);
                _isRunning = false;
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("TCP服务器启动失败(连接已断开)", ex);
                _isRunning = false;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("TCP服务器启动失败", ex);
                _isRunning = false;
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
                _isRunning = false;
                
                // 关闭所有客户端连接
                lock (_lock)
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch (SocketException ex) { Logger.Error("关闭客户端连接失败", ex); }
                        catch (IOException ex) { Logger.Error("关闭客户端连接失败", ex); }
                        catch (ObjectDisposedException ex) { Logger.Error("关闭客户端连接失败", ex); }
                        catch (Exception ex) { Logger.Error("关闭客户端连接失败", ex); }
                    }
                    _clients.Clear();
                }
                
                // 停止监听器
                if (_listener != null)
                {
                    _listener.Stop();
                }
                
                Logger.Info($"TCP服务器停止成功");
            }
            catch (SocketException ex)
            {
                Logger.Error("TCP服务器停止失败(网络错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("TCP服务器停止失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("TCP服务器停止失败(连接已断开)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("TCP服务器停止失败", ex);
            }
        }

        /// <summary>
        /// 接受客户端连接
        /// </summary>
        private async Task AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    lock (_lock)
                    {
                        _clients.Add(client);
                    }
                    
                    // 触发客户端连接事件
                    ClientConnected?.Invoke(this, client);
                    Logger.Info($"客户端连接成功: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                    
                    // 开始处理客户端数据，不需要等待完成
                    _ = HandleClient(client);
                }
                catch (SocketException ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error("接受客户端连接失败(网络错误)", ex);
                    }
                }
                catch (IOException ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error("接受客户端连接失败(IO错误)", ex);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error("接受客户端连接失败(监听器已释放)", ex);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error("接受客户端连接失败(操作无效)", ex);
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error("接受客户端连接失败", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 处理客户端数据
        /// </summary>
        /// <param name="client">客户端</param>
        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            
            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        
                        // 触发数据接收事件
                        DataReceived?.Invoke(this, data);
                        Logger.Info($"接收数据: {Encoding.UTF8.GetString(data)}");
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                if (_isRunning)
                {
                    Logger.Error("处理客户端数据失败(网络错误)", ex);
                }
            }
            catch (IOException ex)
            {
                if (_isRunning)
                {
                    Logger.Error("处理客户端数据失败(IO错误)", ex);
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (_isRunning)
                {
                    Logger.Error("处理客户端数据失败(连接已断开)", ex);
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Logger.Error("处理客户端数据失败", ex);
                }
            }
            finally
            {
                // 客户端断开连接
                lock (_lock)
                {
                    _clients.Remove(client);
                }
                
                // 触发客户端断开事件
                ClientDisconnected?.Invoke(this, client);
                Logger.Info($"客户端断开连接: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                
                try
                {
                    client.Close();
                }
                catch (SocketException ex) { Logger.Error("关闭客户端连接失败", ex); }
                catch (IOException ex) { Logger.Error("关闭客户端连接失败", ex); }
                catch (ObjectDisposedException ex) { Logger.Error("关闭客户端连接失败", ex); }
                catch (Exception ex) { Logger.Error("关闭客户端连接失败", ex); }
            }
        }

        /// <summary>
        /// 发送数据给所有客户端
        /// </summary>
        /// <param name="data">数据</param>
        public void SendToAll(byte[] data)
        {
            lock (_lock)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch (SocketException ex)
                    {
                        Logger.Error("发送数据失败(网络错误)", ex);
                    }
                    catch (IOException ex)
                    {
                        Logger.Error("发送数据失败(IO错误)", ex);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Logger.Error("发送数据失败(连接已断开)", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("发送数据失败", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 发送数据给指定客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">数据</param>
        public void SendToClient(TcpClient client, byte[] data)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch (SocketException ex)
            {
                Logger.Error("发送数据失败(网络错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("发送数据失败(IO错误)", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("发送数据失败(连接已断开)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("发送数据失败", ex);
            }
        }
    }
}