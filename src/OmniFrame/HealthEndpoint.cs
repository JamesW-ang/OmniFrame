using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniFrame.Common;
using OmniFrame.Core;

namespace OmniFrame
{
    /// <summary>
    /// 轻量级 HTTP 健康检查端点（无外部依赖）
    /// </summary>
    public class HealthEndpoint : IDisposable
    {
        private readonly IHealthCheckService _healthCheck;
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenTask;
        private readonly int _port;

        public HealthEndpoint(IHealthCheckService healthCheck, int port = 8080)
        {
            _healthCheck = healthCheck;
            _port = port;
        }

        public void Start()
        {
            if (_listener != null)
                return;

            try
            {
                _cts = new CancellationTokenSource();
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{_port}/");
                _listener.Start();
                _listenTask = ListenLoopAsync(_cts.Token);
                Logger.Info($"健康检查端点已启动，端口: {_port}");
            }
            catch (Exception ex)
            {
                Logger.Warning("健康检查端点启动失败（端口可能被占用）", ex);
            }
        }

        private async Task ListenLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync().ConfigureAwait(false);
                    await HandleRequestAsync(context);
                }
                catch (ObjectDisposedException) { break; }
                catch (HttpListenerException) { break; }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var result = _healthCheck.CheckHealth();
                string json = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                context.Response.StatusCode = result.Status == "Unhealthy" ? 503 : 200;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Logger.Error("健康检查端点处理请求失败", ex);
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.Close();
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _cts?.Dispose();
        }
    }
}
