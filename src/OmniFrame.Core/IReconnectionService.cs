using System;

namespace OmniFrame.Core
{
    /// <summary>
    /// 设备自动重连服务 — 监听设备断连事件，按指数退避策略自动重试。
    /// </summary>
    public interface IReconnectionService
    {
        bool IsAnyReconnecting { get; }
        event EventHandler<string> ReconnectStarted;
        event EventHandler<string> ReconnectSucceeded;
        event EventHandler<(string Name, string Message)> ReconnectFailed;

        void Start();
        void Stop();
        void TriggerReconnect(string deviceName);
    }
}
