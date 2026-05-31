using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 扫码枪 TCP 客户端 — 替代 TcpSocket 扫码枪模块
    /// 支持双扫码枪同时监听
    /// </summary>
    public class BarcodeScannerClient : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly string _host;
        private readonly int _port;
        private CancellationTokenSource _cts;
        private bool _disposed;

        /// <summary>扫码成功事件</summary>
        public event Action<string> OnCodeScanned;
        /// <summary>连接状态变更</summary>
        public event Action<bool> OnConnectionChanged;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        /// <param name="host">扫码枪 IP</param>
        /// <param name="port">扫码枪端口</param>
        public BarcodeScannerClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        /// <summary>建立 TCP 连接并开始接收数据</summary>
        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_host, _port);
                _stream = _tcpClient.GetStream();
                _cts = new CancellationTokenSource();

                OnConnectionChanged?.Invoke(true);

                // 启动接收循环
                _ = ReceiveLoopAsync(_cts.Token);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"扫码枪连接失败 ({_host}:{_port}): {ex.Message}");
                return false;
            }
        }

        /// <summary>接收循环 — 逐行读取条码数据</summary>
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[4096];
            var sb = new StringBuilder();

            try
            {
                while (!token.IsCancellationRequested && _tcpClient?.Connected == true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                        break;

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    sb.Append(data);

                    // 条码通常以 \r\n 结尾
                    int newlineIdx;
                    while ((newlineIdx = sb.ToString().IndexOf('\n')) >= 0)
                    {
                        string code = sb.ToString(0, newlineIdx).TrimEnd('\r');
                        sb.Remove(0, newlineIdx + 1);

                        if (!string.IsNullOrEmpty(code))
                        {
                            OnCodeScanned?.Invoke(code);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logger.Error($"扫码枪接收异常: {ex.Message}");
            }
            finally
            {
                OnConnectionChanged?.Invoke(false);
            }
        }

        /// <summary>主动扫码 — 发送触发指令并等待结果返回</summary>
        public async Task<string> ScanAsync(string scannerId, int timeoutMs = 5000)
        {
            var tcs = new TaskCompletionSource<string>();
            var cts = new CancellationTokenSource(timeoutMs);

            Action<string> handler = null;
            handler = code =>
            {
                tcs.TrySetResult(code);
            };

            OnCodeScanned += handler;
            cts.Token.Register(() => tcs.TrySetResult(null));

            try
            {
                // 发送触发指令 (部分扫码枪需要)
                if (_stream != null && _tcpClient?.Connected == true)
                {
                    byte[] trigger = Encoding.ASCII.GetBytes("TRIGGER\r\n");
                    await _stream.WriteAsync(trigger, 0, trigger.Length);
                }
                var result = await tcs.Task;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"扫码枪扫描失败: {ex.Message}", ex);
                return null;
            }
            finally
            {
                OnCodeScanned -= handler;
                cts.Dispose();
            }
        }

        /// <summary>断开连接</summary>
        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();
        }

        /// <summary>注入虚拟扫码 — 供空跑/仿真模式驱动工站流程</summary>
        public void SimulateScan(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            Logger.Info($"[BarcodeScanner] 虚拟扫码: {code}");
            OnCodeScanned?.Invoke(code);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _cts?.Dispose();
                _tcpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
