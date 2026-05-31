using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 告警通知管理器
        /// </summary>
    public class AlarmNotification : IDisposable, IAlarmNotification
    {
        private readonly object _lock = new object();
        private Dictionary<string, DateTime> _lastNotificationTime;
        private HttpClient _httpClient;
        private bool _isDisposed;
        private Dictionary<string, string> _notificationConfig;
        private string _configFilePath = "Config/notification.cfg";


        public AlarmNotification()
        {
            _lastNotificationTime = new Dictionary<string, DateTime>();
            _httpClient = new HttpClient();
            _notificationConfig = new Dictionary<string, string>();
            LoadNotificationConfig();
        }

        /// <summary>
        /// 加载通知配置
        /// </summary>
        private void LoadNotificationConfig()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    // 如果配置文件不存在，创建默认配置并加密保存
                    CreateDefaultNotificationConfig();
                    return;
                }

                // 读取加密的配置文件
                byte[] encryptedData = File.ReadAllBytes(_configFilePath);
                string decryptedContent = DecryptData(encryptedData);

                // 解析配置内容
                foreach (string line in decryptedContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        _notificationConfig[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                Logger.Info("通知配置加载成功");
            }
            catch (Exception ex)
            {
                Logger.Error("加载通知配置失败", ex);
                // 如果加载失败，使用默认配置
                CreateDefaultNotificationConfig();
            }
        }

        /// <summary>
        /// 创建默认通知配置
        /// </summary>
        private void CreateDefaultNotificationConfig()
        {
            // 设置默认配置
            _notificationConfig["SmtpUsername"] = "username";
            _notificationConfig["SmtpPassword"] = "password";
            _notificationConfig["SmsApiKey"] = "your_api_key";

            // 保存到加密文件
            SaveNotificationConfig();
            Logger.Warning("创建了默认通知配置文件，请尽快修改默认密码和API密钥");
        }

        /// <summary>
        /// 保存通知配置到加密文件
        /// </summary>
        private void SaveNotificationConfig()
        {
            try
            {
                // 确保配置目录存在
                string configDir = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 构建配置内容
                StringBuilder content = new StringBuilder();
                foreach (var config in _notificationConfig)
                {
                    content.AppendLine($"{config.Key}={config.Value}");
                }

                // 加密并保存
                byte[] encryptedData = EncryptData(content.ToString());
                File.WriteAllBytes(_configFilePath, encryptedData);

                Logger.Info("通知配置保存成功");
            }
            catch (Exception ex)
            {
                Logger.Error("保存通知配置失败", ex);
            }
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        private byte[] EncryptData(string data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (MemoryStream ms = new MemoryStream())
                {
                    // 先写入IV
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        private string DecryptData(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // 从加密数据中读取IV
                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 获取加密密钥
        /// </summary>
        private byte[] GetEncryptionKey()
        {
            // 优先从环境变量获取密钥
            string envKey = Environment.GetEnvironmentVariable("AUTOFRAME_ENCRYPTION_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                // 确保密钥长度为32字节
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(envKey));
                }
            }

            // 如果环境变量不存在，拒绝启动
            throw new InvalidOperationException("环境变量 AUTOFRAME_ENCRYPTION_KEY 未设置，拒绝启动。请设置此环境变量后重新运行。");
        }

        /// <summary>
        /// 获取机器指纹
        /// </summary>
        private string GetMachineFingerprint()
        {
            // 简单的机器指纹生成方法，实际应用中可能需要更复杂的实现
            string fingerprint = $"{Environment.MachineName}_{Environment.OSVersion}_{Environment.ProcessorCount}";
            return fingerprint;
        }

        /// <summary>
        /// 发送告警通知
        /// </summary>
        public async Task SendNotification(AlarmInfo alarm, string webhookUrl = null, string emailRecipients = null, string smsRecipients = null)
        {
            if (alarm == null)
                return;

            // 检查重复告警
            string alarmKey = $"{alarm.AlarmCode}_{alarm.Level}";
            if (IsDuplicateAlarm(alarmKey))
            {
                Logger.Info($"重复告警，跳过通知: {alarm.AlarmCode}");
                return;
            }

            // 记录通知时间
            lock (_lock)
            {
                _lastNotificationTime[alarmKey] = DateTime.Now;
            }

            // 根据告警级别决定推送渠道
            switch (alarm.Level)
            {
                case AlarmLevel.Info:
                    // 普通告警，仅日志
                    break;
                case AlarmLevel.Warning:
                    // 重要告警，界面弹窗 + 可选推送
                    await SendWechatNotification(alarm, webhookUrl);
                    break;
                case AlarmLevel.Error:
                case AlarmLevel.Critical:
                    // 紧急告警，多渠道推送
                    await SendWechatNotification(alarm, webhookUrl);
                    await SendEmailNotification(alarm, emailRecipients);
                    await SendSmsNotification(alarm, smsRecipients);
                    break;
            }
        }

        /// <summary>
        /// 发送企业微信通知
        /// </summary>
        private async Task SendWechatNotification(AlarmInfo alarm, string webhookUrl)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                return;

            try
            {
                var message = new
                {
                    msgtype = "markdown",
                    markdown = new
                    {
                        content = $"##设备告警\n" +
                                 $"**告警时间**: {alarm.OccurTime.ToString("yyyy-MM-dd HH:mm:ss")}\n" +
                                 $"**告警代码**: {alarm.AlarmCode}\n" +
                                 $"**告警信息**: {alarm.AlarmMessage}\n" +
                                 $"**告警来源**: {alarm.Source}\n" +
                                 $"**排查指南**: {ErrorGuide.GetGuide(int.Parse(alarm.AlarmCode))}\n"
                    }
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(webhookUrl, content);
                response.EnsureSuccessStatusCode();

                Logger.Info($"企业微信通知发送成功: {alarm.AlarmCode}");
            }
            catch (Exception ex)
            {
                Logger.Error("发送企业微信通知失败", ex);
            }
        }

        /// <summary>
        /// 发送邮件通知
        /// </summary>
        private async Task SendEmailNotification(AlarmInfo alarm, string recipients)
        {
            if (string.IsNullOrEmpty(recipients))
                return;

            try
            {
                // 从配置中获取SMTP用户名和密码
                string smtpUsername = _notificationConfig.TryGetValue("SmtpUsername", out string username) ? username : "username";
                string smtpPassword = _notificationConfig.TryGetValue("SmtpPassword", out string password) ? password : "password";

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("alarm@example.com", "设备告警系统");
                    foreach (var recipient in recipients.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(recipient))
                            message.To.Add(recipient);
                    }

                    message.Subject = $"{alarm.AlarmCode}: {alarm.AlarmMessage}";
                    message.Body = $"告警时间: {alarm.OccurTime.ToString("yyyy-MM-dd HH:mm:ss")}\n" +
                                  $"告警代码: {alarm.AlarmCode}\n" +
                                  $"告警信息: {alarm.AlarmMessage}\n" +
                                  $"告警来源: {alarm.Source}\n" +
                                  $"排查指南: {ErrorGuide.GetGuide(int.Parse(alarm.AlarmCode))}\n";
                    message.IsBodyHtml = false;

                    using (var client = new SmtpClient("smtp.example.com", 587))
                    {
                        client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                        client.EnableSsl = true;
                        await client.SendMailAsync(message);
                    }
                }

                Logger.Info($"邮件通知发送成功: {alarm.AlarmCode}");
            }
            catch (Exception ex)
            {
                Logger.Error("发送邮件通知失败", ex);
            }
        }

        /// <summary>
        /// 发送短信通知
        /// </summary>
        private async Task SendSmsNotification(AlarmInfo alarm, string recipients)
        {
            if (string.IsNullOrEmpty(recipients))
                return;

            try
            {
                // 从配置中获取API密钥
                string apiKey = _notificationConfig.TryGetValue("SmsApiKey", out string key) ? key : "your_api_key";

                // 这里使用第三方短信API，需要根据实际情况修改
                string apiUrl = "https://api.example.com/sms/send";
                var requestData = new
                {
                    apiKey = apiKey,
                    recipients = recipients,
                    content = $"{alarm.AlarmCode}: {alarm.AlarmMessage}，请及时处理。"
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                Logger.Info($"短信通知发送成功: {alarm.AlarmCode}");
            }
            catch (Exception ex)
            {
                Logger.Error("发送短信通知失败", ex);
            }
        }

        /// <summary>
        /// 检查是否为重复告警
        /// </summary>
        private bool IsDuplicateAlarm(string alarmKey)
        {
            lock (_lock)
            {
                if (_lastNotificationTime.TryGetValue(alarmKey, out DateTime lastTime))
                {
                    // 5分钟内的相同告警视为重复
                    return (DateTime.Now - lastTime).TotalMinutes < 5;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取告警级别文本
        /// </summary>
        private string GetLevelText(AlarmLevel level)
        {
            switch (level)
            {
                case AlarmLevel.Info: return "信息";
                case AlarmLevel.Warning: return "警告";
                case AlarmLevel.Error: return "错误";
                case AlarmLevel.Critical: return "严重";
                default: return "未知";
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _httpClient?.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 告警通知配置
        /// </summary>
    public class AlarmNotificationConfig
    {
        public string WechatWebhookUrl { get; set; }
        public string EmailRecipients { get; set; }
        public string SmsRecipients { get; set; }
        public int DuplicateIntervalMinutes { get; set; } = 5;
    }
}
