using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core.AdvancedFeatures
{
    public interface IMqttManager : IDisposable
    {
        Task InitializeAsync(string broker, int port, string clientId);
        Task Publish(string topic, string payload);
        Task Subscribe(string topic);
        Task Subscribe(string topic, Func<string, string, Task> handler);
        void Subscribe(string topic, Action<string, string> handler);
        void Unsubscribe(string topic);
        Task Disconnect();
        Task DisconnectAsync();
        bool IsConnected { get; }
    }

    /// <summary>
    /// MQTT 管理器 — 仿真模式下提供进程内消息代理。
    /// 支持通配符订阅 (单层 + 匹配, 多层 # 匹配) 及消息统计。
    /// </summary>
    public class MqttManager : IMqttManager
    {
        private readonly ConcurrentDictionary<string, List<Action<string, string>>> _subscriptions
            = new ConcurrentDictionary<string, List<Action<string, string>>>(StringComparer.OrdinalIgnoreCase);
        private string _broker = string.Empty;
        private string _clientId = string.Empty;
        private int _port;

        // ---- 消息统计 ----
        private long _messagesSent;
        private long _messagesReceived;
        public long MessagesSent => Interlocked.Read(ref _messagesSent);
        public long MessagesReceived => Interlocked.Read(ref _messagesReceived);

        public event EventHandler<(string Topic, string Payload)> MessageReceived;

        public MqttManager() { }

        public bool IsConnected { get; private set; }

        public Task InitializeAsync(string broker, int port, string clientId)
        {
            _broker = broker;
            _port = port;
            _clientId = clientId;
            IsConnected = true;
            Logger.Info($"[MQTT] Broker 初始化完成: {broker}:{port}, ClientId={clientId}");
            return Task.CompletedTask;
        }

        public async Task Publish(string topic, string payload)
        {
            if (!IsConnected)
            {
                Logger.Warning("[MQTT] 发布失败: 未连接");
                return;
            }

            Interlocked.Increment(ref _messagesSent);
            Logger.Info($"[MQTT] PUBLISH topic={topic} payload={payload}");

            // 收集匹配的处理器
            var handlers = new List<Action<string, string>>();
            foreach (var kvp in _subscriptions)
            {
                if (TopicMatches(kvp.Key, topic))
                {
                    handlers.AddRange(kvp.Value);
                }
            }

            foreach (var handler in handlers)
            {
                try
                {
                    handler(topic, payload);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[MQTT] 处理器异常: topic={topic}", ex);
                }
            }

            Interlocked.Increment(ref _messagesReceived);
            OnMessageReceived(topic, payload);
            await Task.CompletedTask;
        }

        /// <summary>
        /// MQTT 通配符匹配: '+' 匹配单层, '#' 匹配多层
        /// </summary>
        private static bool TopicMatches(string subscription, string topic)
        {
            if (string.Equals(subscription, topic, StringComparison.OrdinalIgnoreCase))
                return true;

            // '#' 匹配所有剩余层级
            if (subscription == "#")
                return true;

            if (subscription.Contains("#") || subscription.Contains("+"))
            {
                var pattern = "^" + Regex.Escape(subscription)
                    .Replace("\\+", "[^/]+")
                    .Replace("\\#", ".*") + "$";
                return Regex.IsMatch(topic, pattern, RegexOptions.IgnoreCase);
            }

            return false;
        }

        public Task Subscribe(string topic)
        {
            Subscribe(topic, (t, p) =>
            {
                Logger.Info($"[MQTT] 收到消息: topic={t}, payload={p}");
            });
            return Task.CompletedTask;
        }

        public Task Subscribe(string topic, Func<string, string, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _subscriptions.AddOrUpdate(topic,
                _ => new List<Action<string, string>> { (t, p) => handler(t, p).GetAwaiter().GetResult() },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add((t, p) => handler(t, p).GetAwaiter().GetResult());
                    }
                    return list;
                });

            Logger.Info($"[MQTT] 订阅: topic={topic}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 订阅主题 (同步 Action 重载)
        /// </summary>
        public void Subscribe(string topic, Action<string, string> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _subscriptions.AddOrUpdate(topic,
                _ => new List<Action<string, string>> { handler },
                (_, list) =>
                {
                    lock (list) { list.Add(handler); }
                    return list;
                });

            Logger.Info($"[MQTT] 订阅: topic={topic}");
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(string topic)
        {
            _subscriptions.TryRemove(topic, out _);
            Logger.Info($"[MQTT] 取消订阅: topic={topic}");
        }

        /// <summary>
        /// 断开连接 (同步)
        /// </summary>
        public Task Disconnect()
        {
            _subscriptions.Clear();
            IsConnected = false;
            Logger.Info("[MQTT] 已断开连接");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 优雅关闭 (异步)
        /// </summary>
        public async Task DisconnectAsync()
        {
            Logger.Info($"[MQTT] 正在优雅关闭... (已发送={MessagesSent}, 已接收={MessagesReceived})");
            await Task.Delay(100); // 模拟清理等待
            _subscriptions.Clear();
            IsConnected = false;
            Logger.Info("[MQTT] 已断开连接 (async)");
        }

        /// <summary>
        /// 获取订阅列表快照 (调试用)
        /// </summary>
        public Dictionary<string, int> GetSubscriptionStats()
        {
            var stats = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in _subscriptions)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }

        public void Dispose()
        {
            Disconnect().GetAwaiter().GetResult();
        }

        protected virtual void OnMessageReceived(string topic, string payload)
        {
            MessageReceived?.Invoke(this, (topic, payload));
        }
    }
}
