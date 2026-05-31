using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OmniFrame.Common;

namespace OmniFrame.Communication
{
    public delegate void StateChangedHandler(object sender, bool isConnected);

    public class TcpLink : IDisposable
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public string IP { get; private set; }
        public int Port { get; private set; }
        public int Timeout { get; set; }
        public bool IsConnected => _client?.Connected ?? false;

        public event StateChangedHandler StateChangedEvent;

        private TcpClient _client;
        private NetworkStream _stream;
        private readonly object _lock = new object();
        private bool _isConnecting = false;
        private bool _disposed;

        public TcpLink(int index, string name, string ip, int port, int timeout = 5000)
        {
            Index = index;
            Name = name;
            IP = ip;
            Port = port;
            Timeout = timeout;
        }

        public bool Open()
        {
            lock (_lock)
            {
                if (_isConnecting)
                    return false;

                _isConnecting = true;
                try
                {
                    CloseInternal();

                    _client = new TcpClient();

                    var result = _client.BeginConnect(IPAddress.Parse(IP), Port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout));

                    if (!success)
                    {
                        CloseInternal();
                        Logger.Error($"TCP连接超时: {Name} ({IP}:{Port})");
                        return false;
                    }

                    _client.EndConnect(result);
                    _stream = _client.GetStream();
                    _stream.ReadTimeout = Timeout;
                    _stream.WriteTimeout = Timeout;

                    Logger.Info($"TCP连接成功: {Name} ({IP}:{Port})");
                    StateChangedEvent?.Invoke(this, true);
                    return true;
                }
                catch (SocketException ex)
                {
                    Logger.Error($"TCP连接失败(网络错误): {Name} ({IP}:{Port})", ex);
                    CloseInternal();
                    return false;
                }
                catch (IOException ex)
                {
                    Logger.Error($"TCP连接失败(IO错误): {Name} ({IP}:{Port})", ex);
                    CloseInternal();
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"TCP连接失败: {Name} ({IP}:{Port})", ex);
                    CloseInternal();
                    return false;
                }
                finally
                {
                    _isConnecting = false;
                }
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                CloseInternal();
            }
        }

        private void CloseInternal()
        {
            try
            {
                _stream?.Close();
                _stream = null;
            }
            catch (Exception ex) { Logger.Error($"TCP关闭流资源失败: {Name}", ex); }

            try
            {
                _client?.Close();
                _client = null;
            }
            catch (Exception ex) { Logger.Error($"TCP关闭客户端资源失败: {Name}", ex); }

            StateChangedEvent?.Invoke(this, false);
        }

        public bool Write(byte[] data, int offset, int length)
        {
            lock (_lock)
            {
                if (!IsConnected || _stream == null)
                    return false;

                try
                {
                    _stream.Write(data, offset, length);
                    return true;
                }
                catch (IOException ex)
                {
                    Logger.Error($"TCP写入失败(IO错误): {Name}", ex);
                    CloseInternal();
                    return false;
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Error($"TCP写入失败(对象已释放): {Name}", ex);
                    CloseInternal();
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"TCP写入失败: {Name}", ex);
                    CloseInternal();
                    return false;
                }
            }
        }

        public bool Write(byte[] data)
        {
            return Write(data, 0, data.Length);
        }

        public bool WriteLine(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data + "\r\n");
            return Write(bytes);
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            lock (_lock)
            {
                if (!IsConnected || _stream == null)
                    return -1;

                try
                {
                    if (_stream.DataAvailable)
                    {
                        return _stream.Read(buffer, offset, length);
                    }
                    return 0;
                }
                catch (IOException ex)
                {
                    Logger.Error($"TCP读取失败(IO错误): {Name}", ex);
                    CloseInternal();
                    return -1;
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Error($"TCP读取失败(对象已释放): {Name}", ex);
                    CloseInternal();
                    return -1;
                }
                catch (Exception ex)
                {
                    Logger.Error($"TCP读取失败: {Name}", ex);
                    CloseInternal();
                    return -1;
                }
            }
        }

        public int ReadLine(out string data)
        {
            data = string.Empty;
            lock (_lock)
            {
                if (!IsConnected || _stream == null)
                    return -1;

                try
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] buffer = new byte[1];
                    DateTime startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalMilliseconds < Timeout)
                    {
                        if (_stream.DataAvailable)
                        {
                            int read = _stream.Read(buffer, 0, 1);
                            if (read > 0)
                            {
                                char c = (char)buffer[0];
                                if (c == '\n')
                                {
                                    data = sb.ToString().TrimEnd('\r');
                                    return data.Length;
                                }
                                sb.Append(c);
                            }
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }

                    return -2;
                }
                catch (Exception ex)
                {
                    Logger.Error($"TCP读取行失败: {Name}", ex);
                    CloseInternal();
                    return -1;
                }
            }
        }

        public bool IsOpen()
        {
            return IsConnected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Close();
                _client?.Dispose();
            }

            _disposed = true;
        }
    }
}
