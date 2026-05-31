using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniFrame.Common;
using OmniFrame.Core.AdvancedFeatures;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// BlockCut MES 通信客户端 — Qt 协议对齐版
    ///
    /// 协议:
    ///   1. HTTP GET  — 卡片验证 (替代 HttpToMes::CheckCard)
    ///   2. MQTT Pub  — 状态上报 (data.device.status, AES-128-ECB)
    ///   3. MQTT Pub  — 工单上报 (data.device.workorder.report, AES-128-ECB)
    ///   4. MQTT Sub  — 工单回复 (device/workreport/reply/xqgzpzddw, AES-128-ECB)
    /// </summary>
    public class BlockCutMesClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IMqttManager _mqtt;
        private readonly string _aesKey;
        private bool _disposed;

        // Pending work report confirmations (msg_id → TaskCompletionSource)
        private readonly ConcurrentDictionary<string, TaskCompletionSource<WorkReportReply>> _pendingReports
            = new ConcurrentDictionary<string, TaskCompletionSource<WorkReportReply>>();

        // MES 服务地址 (从 XML 配置读取)
        public string MesBaseUrl { get; set; } = "http://mes-server/api";
        public string DeviceNo { get; set; } = "xqgzpzddw";

        /// <summary>仿真模式 — true 时直接返回验证成功，不连真实 MES</summary>
        public bool SimulationMode { get; set; }

        // === MQTT Topics (4a: 对齐 Qt 名称) ===
        public string MqttStatusTopic { get; set; } = "data.device.status";
        public string MqttWorkTopic { get; set; } = "data.device.workorder.report";
        public string MqttWorkReplyTopic { get; set; } = "device/workreport/reply/xqgzpzddw";

        /// <summary>MES 工单回复事件</summary>
        public event Action<string, string> OnWorkReplyReceived;

        /// <param name="mqtt">MQTT 管理器 (DI 注入)</param>
        /// <param name="aesKey">AES-128-ECB 密钥 (4c: Qt 密钥，可从环境变量覆盖)</param>
        public BlockCutMesClient(IMqttManager mqtt, string aesKey)
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            _mqtt = mqtt ?? throw new ArgumentNullException(nameof(mqtt));
            _aesKey = aesKey ?? throw new ArgumentNullException(nameof(aesKey));

            // 订阅工单回复
            _mqtt.Subscribe(MqttWorkReplyTopic, (Action<string, string>)OnMqttWorkReply);
        }

        #region HTTP — 卡片验证 (替代 HttpToMes::CheckCard)

        /// <summary>验证卡片/条码 (HTTP GET)</summary>
        public async Task<MesValidateResult> ValidateCardAsync(string cardId, CancellationToken token)
        {
            if (SimulationMode)
                return new MesValidateResult { IsValid = true, FileId = cardId, AlertMsg = "仿真验证通过" };

            try
            {
                string url = $"{MesBaseUrl}/validateCard?cardId={Uri.EscapeDataString(cardId)}";
                var response = await _httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<MesValidateResult>(json);
                return result ?? new MesValidateResult { IsValid = false, AlertMsg = "响应解析失败" };
            }
            catch (TaskCanceledException)
            {
                return new MesValidateResult { IsValid = false, AlertMsg = "MES 验证超时" };
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"MES HTTP 请求失败: {ex.Message}");
                return new MesValidateResult { IsValid = false, AlertMsg = $"MES 连接失败: {ex.Message}" };
            }
        }

        /// <summary>片源验证</summary>
        public async Task<MesValidateResult> ValidateSliceAsync(string sliceId, CancellationToken token)
        {
            if (SimulationMode)
                return new MesValidateResult { IsValid = true, FileId = sliceId, AlertMsg = "仿真验证通过" };

            try
            {
                string url = $"{MesBaseUrl}/validateSlice?sliceId={Uri.EscapeDataString(sliceId)}";
                var response = await _httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MesValidateResult>(json)
                       ?? new MesValidateResult { IsValid = false };
            }
            catch (Exception ex)
            {
                Logger.Error($"MES 片源验证失败: {ex.Message}");
                return new MesValidateResult { IsValid = false, AlertMsg = ex.Message };
            }
        }

        #endregion

        #region MQTT — 状态上报 (4b: 对齐 Qt JSON 结构)

        /// <summary>
        /// 发送机器状态 — 对齐 Qt JSON 结构
        /// </summary>
        public async Task SendStatusAsync(MachineStatus status, CancellationToken token)
        {
            string json = JsonConvert.SerializeObject(status);
            string encrypted = Security.Aes128EcbEncrypt(json, _aesKey);

            await _mqtt.Publish(MqttStatusTopic, encrypted);
        }

        #endregion

        #region MQTT — 工单上报 (4d: 订阅确认超时 + 4e: msg_id)

        /// <summary>
        /// 发送工单报告 — 8s 超时等待 MQTT 回复 (4d)
        /// </summary>
        public async Task<WorkReportReply> SendWorkReportAsync(WorkReport report, CancellationToken token)
        {
            // 4e: 生成 msg_id — 格式: "wpd-{deviceNo}-{epochMs}"
            report.MsgId = $"wpd-{DeviceNo}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            string json = JsonConvert.SerializeObject(report);
            string encrypted = Security.Aes128EcbEncrypt(json, _aesKey);

            var tcs = new TaskCompletionSource<WorkReportReply>();
            _pendingReports[report.MsgId] = tcs;

            try
            {
                await _mqtt.Publish(MqttWorkTopic, encrypted);

                // 8 秒超时等待 MQTT 回复
                using var timeoutCts = new CancellationTokenSource(8000);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);

                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(8000, linked.Token));
                if (completedTask == tcs.Task)
                {
                    return await tcs.Task;
                }

                // 超时 → 返回失败状态
                return new WorkReportReply { Accepted = false, BizErrMsg = "MES 确认超时" };
            }
            catch (OperationCanceledException)
            {
                return new WorkReportReply { Accepted = false, BizErrMsg = "请求已取消" };
            }
            finally
            {
                _pendingReports.TryRemove(report.MsgId, out _);
            }
        }

        /// <summary>MQTT 工单回复回调 — 匹配 msg_id 唤醒等待方 (4d)</summary>
        private void OnMqttWorkReply(string topic, string payload)
        {
            try
            {
                string decrypted = Security.Aes128EcbDecrypt(payload, _aesKey);
                var reply = JsonConvert.DeserializeObject<WorkReportReply>(decrypted);

                if (reply != null && !string.IsNullOrEmpty(reply.MsgId))
                {
                    if (_pendingReports.TryGetValue(reply.MsgId, out var tcs))
                    {
                        tcs.TrySetResult(reply);
                    }
                }

                OnWorkReplyReceived?.Invoke(reply?.FileId ?? "", reply?.BizErrMsg ?? "");
            }
            catch (Exception ex)
            {
                Logger.Error($"MES 回复解密失败: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                // 取消所有待确认的报告
                foreach (var kv in _pendingReports)
                    kv.Value.TrySetCanceled();
                _pendingReports.Clear();

                _mqtt?.Unsubscribe(MqttWorkReplyTopic);
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }

    #region DTO 类型 (4b: 对齐 Qt JSON 结构)

    /// <summary>
    /// 机器状态 DTO — 对齐 Qt JSON: o_workStation, o_machineNo, o_machineState...
    /// </summary>
    public class MachineStatus
    {
        [JsonProperty("o_workStation")]
        public string WorkStation { get; set; }

        [JsonProperty("o_machineNo")]
        public string MachineNo { get; set; }

        [JsonProperty("o_machineState")]
        public int MachineState { get; set; }

        [JsonProperty("o_stateName")]
        public string StateName { get; set; }

        [JsonProperty("o_stateStartTime")]
        public string StateStartTime { get; set; }

        [JsonProperty("o_alertCode")]
        public string AlertCode { get; set; }

        [JsonProperty("o_alertInfo")]
        public string AlertInfo { get; set; }
    }

    /// <summary>
    /// 工单报告 DTO — 对齐 Qt JSON + msg_id (4e)
    /// </summary>
    public class WorkReport
    {
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }

        [JsonProperty("o_workStation")]
        public string WorkStation { get; set; }

        [JsonProperty("o_machineNo")]
        public string MachineNo { get; set; }

        [JsonProperty("o_bottomCode")]
        public string BottomCode { get; set; }

        [JsonProperty("o_jigCode")]
        public string JigCode { get; set; }

        [JsonProperty("o_blockCode")]
        public string BlockCode { get; set; }

        [JsonProperty("o_position")]
        public int Position { get; set; }

        [JsonProperty("o_fileId")]
        public string FileId { get; set; }

        [JsonProperty("o_timestamp")]
        public string Timestamp { get; set; }
    }

    /// <summary>
    /// 工单回复 DTO — 对齐 Qt JSON
    /// </summary>
    public class WorkReportReply
    {
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }

        [JsonProperty("o_accepted")]
        public bool Accepted { get; set; }

        [JsonProperty("o_fileId")]
        public string FileId { get; set; }

        [JsonProperty("o_bizErrMsg")]
        public string BizErrMsg { get; set; }
    }

    #endregion
}
