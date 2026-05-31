using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 配置管理器
    /// 功能说明：管理系统配置，支持热重载、校验、备份/回滚
        /// </summary>
    public class ConfigManager : IConfigManager
    {
        private string _configPath;
        private string _backupPath;
        private Dictionary<string, DateTime> _configFileTimes;
        private FileSystemWatcher _fileWatcher;
        private bool _isWatching;
        private System.Threading.Timer _backupTimer;
        private const int DefaultBackupIntervalHours = 6;
        private const int MaxBackupFiles = 50;

        // 文件变更防抖
        private readonly Dictionary<string, DateTime> _lastChangeTimes = new();
        private readonly object _debounceLock = new();
        private const int DebounceMs = 500;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConfigManager()
        {
            _configPath = Path.Combine(AppContext.BaseDirectory, "Config");
            _backupPath = Path.Combine(_configPath, "Backup");
            _configFileTimes = new Dictionary<string, DateTime>();
            _isWatching = false;

            // 确保目录存在
            FileHelper.EnsureDirectoryExists(_configPath);
            FileHelper.EnsureDirectoryExists(_backupPath);

            // 初始化配置文件时间
            InitConfigFileTimes();
        }

        /// <summary>
        /// 初始化配置文件时间
        /// </summary>
        private void InitConfigFileTimes()
        {
            string[] configFiles = Directory.GetFiles(_configPath, "*.xml");
            foreach (string file in configFiles)
            {
                _configFileTimes[Path.GetFileName(file)] = File.GetLastWriteTime(file);
            }
        }

        /// <summary>
        /// 开始监视配置文件变化（使用 FileSystemWatcher）
        /// </summary>
        public void StartWatching()
        {
            if (_isWatching)
                return;

            try
            {
                _fileWatcher = new FileSystemWatcher(_configPath, "*.xml")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };

                _fileWatcher.Changed += OnConfigFileChanged;
                _fileWatcher.Created += OnConfigFileChanged;
                _fileWatcher.Renamed += OnConfigFileRenamed;
                _isWatching = true;

                // 刷新文件时间缓存
                InitConfigFileTimes();
                Logger.Info("配置文件监视已启动（FileSystemWatcher）");
            }
            catch (Exception ex)
            {
                Logger.Error("启动配置文件监视失败", ex);
            }
        }

        /// <summary>
        /// 停止监视配置文件变化
        /// </summary>
        public void StopWatching()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
            _isWatching = false;
            Logger.Info("配置文件监视已停止");
        }

        /// <summary>
        /// 启动自动备份（定时全量备份配置文件）
        /// </summary>
        public void StartAutoBackup(int intervalHours = 6)
        {
            StopAutoBackup();
            int ms = Math.Max(1, intervalHours) * 60 * 60 * 1000;
            _backupTimer = new System.Threading.Timer(_ => AutoBackupCallback(), null, ms, ms);
            Logger.Info($"自动备份已启动，间隔 {intervalHours}h");
        }

        /// <summary>
        /// 停止自动备份
        /// </summary>
        public void StopAutoBackup()
        {
            _backupTimer?.Dispose();
            _backupTimer = null;
        }

        private void AutoBackupCallback()
        {
            try
            {
                string[] configFiles = Directory.GetFiles(_configPath, "*.xml");
                foreach (string file in configFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string backupFile = Path.Combine(_backupPath,
                        $"{Path.GetFileNameWithoutExtension(fileName)}_auto_{DateTime.Now:yyyyMMddHHmmss}.xml");
                    File.Copy(file, backupFile, true);
                }

                // 清理旧备份（保留最近 MaxBackupFiles 个）
                var backups = Directory.GetFiles(_backupPath, "*_auto_*.xml")
                    .OrderByDescending(f => f)
                    .ToList();
                foreach (string old in backups.Skip(MaxBackupFiles))
                {
                    File.Delete(old);
                }

                Logger.Info($"自动备份完成: {configFiles.Length} 个文件, 保留 {Math.Min(backups.Count, MaxBackupFiles)} 个备份");
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("自动备份失败", ex);
            }
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            string fileName = Path.GetFileName(e.Name);

            // 防抖：500ms 内的重复变更只处理一次（不阻塞线程）
            lock (_debounceLock)
            {
                if (_lastChangeTimes.TryGetValue(fileName, out var last) &&
                    (DateTime.Now - last).TotalMilliseconds < DebounceMs)
                    return;
                _lastChangeTimes[fileName] = DateTime.Now;
            }

            try
            {
                _configFileTimes[fileName] = File.GetLastWriteTime(e.FullPath);
                Logger.Info($"配置文件已变更: {fileName}");
                OnConfigChanged(fileName);
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"处理配置文件变更失败: {fileName}", ex);
            }
        }

        private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
        {
            string oldName = Path.GetFileName(e.OldName);
            string newName = Path.GetFileName(e.Name);

            _configFileTimes.Remove(oldName);
            _configFileTimes[newName] = File.GetLastWriteTime(e.FullPath);
            Logger.Info($"配置文件已重命名: {oldName} -> {newName}");
            OnConfigChanged(newName);
        }

        /// <summary>
        /// 触发配置变更事件
        /// </summary>
        /// <param name="configFileName">配置文件名</param>
        private void OnConfigChanged(string configFileName)
        {
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs { ConfigFileName = configFileName });
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="configFileName">配置文件名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置对象</returns>
        public T GetConfig<T>(string configFileName, T defaultValue = default(T))
        {
            try
            {
                string configFile = Path.Combine(_configPath, configFileName);
                if (!File.Exists(configFile))
                    return defaultValue;

                using (FileStream stream = new FileStream(configFile, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("配置文件XML序列化失败", ex);
                return defaultValue;
            }
            catch (Exception ex)
            {
                Logger.Error($"获取配置失败: {configFileName}", ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="configFileName">配置文件名</param>
        /// <param name="config">配置对象</param>
        /// <returns>是否保存成功</returns>
        public bool SaveConfig<T>(string configFileName, T config)
        {
            try
            {
                string configFile = Path.Combine(_configPath, configFileName);

                // 备份当前配置
                BackupConfig(configFileName);

                // 保存新配置
                using (FileStream stream = new FileStream(configFile, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, config);
                }

                // 更新配置文件时间
                _configFileTimes[configFileName] = File.GetLastWriteTime(configFile);

                // 触发配置变更事件
                OnConfigChanged(configFileName);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"保存配置失败: {configFileName}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取加密配置（自动 AES 解密）
        /// </summary>
        public T GetEncryptedConfig<T>(string configFileName, string encryptionKey, T defaultValue = default(T))
        {
            try
            {
                string configFile = Path.Combine(_configPath, configFileName);
                if (!File.Exists(configFile))
                    return defaultValue;

                byte[] cipherBytes = File.ReadAllBytes(configFile);
                string xml = DecryptString(cipherBytes, encryptionKey);

                using (var reader = new StringReader(xml))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("配置文件XML序列化失败", ex);
                return defaultValue;
            }
            catch (Exception ex)
            {
                Logger.Error($"获取加密配置失败: {configFileName}", ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存加密配置（自动 AES 加密）
        /// </summary>
        public bool SaveEncryptedConfig<T>(string configFileName, T config, string encryptionKey)
        {
            try
            {
                string configFile = Path.Combine(_configPath, configFileName);

                BackupConfig(configFileName);

                string xml;
                using (var writer = new StringWriter())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, config);
                    xml = writer.ToString();
                }

                byte[] cipherBytes = EncryptString(xml, encryptionKey);
                File.WriteAllBytes(configFile, cipherBytes);

                _configFileTimes[configFileName] = File.GetLastWriteTime(configFile);
                OnConfigChanged(configFileName);

                return true;
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"保存加密配置失败: {configFileName}", ex);
                return false;
            }
        }

        #region AES Encryption Helpers

        private static byte[] EncryptString(string plainText, string password)
        {
            // Generate random salt for PBKDF2 (16 bytes)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate random IV separately (NOT derived from same salt as key)
            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] key;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                key = deriveBytes.GetBytes(32);  // 256-bit key
            }

            byte[] ciphertext;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;

                using (var ms = new MemoryStream())
                using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(plainText);
                    writer.Flush();
                    cryptoStream.FlushFinalBlock();
                    ciphertext = ms.ToArray();
                }
            }

            // HMAC-SHA256 authentication tag over the ciphertext
            byte[] hmac;
            using (var hmacSha256 = new HMACSHA256(key))
            {
                hmac = hmacSha256.ComputeHash(ciphertext);
            }

            // Format: Salt(16) + IV(16) + Ciphertext + HMAC(32)
            byte[] result = new byte[salt.Length + iv.Length + ciphertext.Length + hmac.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, result, salt.Length + iv.Length, ciphertext.Length);
            Buffer.BlockCopy(hmac, 0, result, salt.Length + iv.Length + ciphertext.Length, hmac.Length);

            return result;
        }

        private static string DecryptString(byte[] cipherData, string password)
        {
            // Format: Salt(16) + IV(16) + Ciphertext + HMAC(32)
            const int saltLen = 16;
            const int ivLen = 16;
            const int hmacLen = 32;

            if (cipherData.Length < saltLen + ivLen + hmacLen + 1)
                throw new InvalidOperationException("解密失败: 密文数据格式无效（数据过短）");

            byte[] salt = new byte[saltLen];
            byte[] iv = new byte[ivLen];
            Buffer.BlockCopy(cipherData, 0, salt, 0, saltLen);
            Buffer.BlockCopy(cipherData, saltLen, iv, 0, ivLen);

            int ciphertextLen = cipherData.Length - saltLen - ivLen - hmacLen;
            byte[] ciphertext = new byte[ciphertextLen];
            byte[] storedHmac = new byte[hmacLen];
            Buffer.BlockCopy(cipherData, saltLen + ivLen, ciphertext, 0, ciphertextLen);
            Buffer.BlockCopy(cipherData, saltLen + ivLen + ciphertextLen, storedHmac, 0, hmacLen);

            // Derive key from password + salt
            byte[] key;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                key = deriveBytes.GetBytes(32);
            }

            // Verify HMAC before decrypting
            using (var hmacSha256 = new HMACSHA256(key))
            {
                byte[] computedHmac = hmacSha256.ComputeHash(ciphertext);
                if (!ConstantTimeEquals(computedHmac, storedHmac))
                {
                    throw new InvalidOperationException("解密失败: HMAC 验证失败，密文可能已被篡改或密钥错误");
                }
            }

            // Decrypt
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;

                using (var ms = new MemoryStream(ciphertext))
                using (var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 常量时间字节数组比较（防止计时攻击）
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        #endregion

        /// <summary>
        /// 备份配置
        /// </summary>
        private void BackupConfig(string configFileName)
        {
            try
            {
                string configFile = Path.Combine(_configPath, configFileName);
                if (File.Exists(configFile))
                {
                    string backupFile = Path.Combine(_backupPath, $"{Path.GetFileNameWithoutExtension(configFileName)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xml");
                    File.Copy(configFile, backupFile, true);
                }
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"备份配置失败: {configFileName}", ex);
            }
        }

        /// <summary>
        /// 回滚配置
        /// </summary>
        /// <param name="backupFileName">备份文件名</param>
        /// <returns>是否回滚成功</returns>
        public bool RollbackConfig(string backupFileName)
        {
            try
            {
                string backupFile = Path.Combine(_backupPath, backupFileName);
                if (!File.Exists(backupFile))
                    return false;

                string configFileName = Path.GetFileNameWithoutExtension(backupFileName).Split('_')[0] + ".xml";
                string configFile = Path.Combine(_configPath, configFileName);

                // 备份当前配置
                BackupConfig(configFileName);

                // 恢复备份
                File.Copy(backupFile, configFile, true);

                // 更新配置文件时间
                _configFileTimes[configFileName] = File.GetLastWriteTime(configFile);

                // 触发配置变更事件
                OnConfigChanged(configFileName);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"回滚配置失败: {backupFileName}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取备份文件列表
        /// </summary>
        /// <returns>备份文件列表</returns>
        public List<string> GetBackupFiles()
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(_backupPath, "*.xml");
                List<string> fileNames = new List<string>();
                foreach (string file in backupFiles)
                {
                    fileNames.Add(Path.GetFileName(file));
                }
                fileNames.Sort((a, b) => b.CompareTo(a)); // 按时间倒序
                return fileNames;
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
                return new List<string>();
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
                return new List<string>();
            }
            catch (Exception ex)
            {
                Logger.Error("获取备份文件列表失败", ex);
                return new List<string>();
            }
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="exportPath">导出路径</param>
        /// <returns>是否导出成功</returns>
        public bool ExportConfig(string exportPath)
        {
            try
            {
                // 创建配置包
                using (FileStream zipFile = File.Create(exportPath))
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    // 添加版本信息
                    var versionEntry = archive.CreateEntry("version.txt");
                    using (StreamWriter writer = new StreamWriter(versionEntry.Open()))
                    {
                        writer.WriteLine($"Version: 1.0");
                        writer.WriteLine($"ExportDate: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        writer.WriteLine($"MachineName: {Environment.MachineName}");
                    }

                    // 添加所有配置文件
                    string[] configFiles = Directory.GetFiles(_configPath, "*.xml");
                    foreach (string file in configFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        var entry = archive.CreateEntry(fileName);
                        using (Stream entryStream = entry.Open())
                        using (FileStream fileStream = File.OpenRead(file))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                Logger.Info($"配置导出成功: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("导出配置失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="importPath">导入路径</param>
        /// <returns>是否导入成功</returns>
        public bool ImportConfig(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    Logger.Error("导入文件不存在");
                    return false;
                }

                // 验证导入文件
                if (!VerifyImportFile(importPath))
                {
                    Logger.Error("导入文件验证失败");
                    return false;
                }

                // 备份当前配置
                BackupCurrentConfig();

                // 解压并导入配置
                using (FileStream zipFile = File.OpenRead(importPath))
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 跳过版本文件
                        if (entry.Name == "version.txt")
                            continue;

                        // 确保配置目录存在
                        FileHelper.EnsureDirectoryExists(_configPath);

                        // 解压配置文件
                        string targetPath = Path.Combine(_configPath, entry.Name);
                        using (Stream entryStream = entry.Open())
                        using (FileStream fileStream = File.Create(targetPath))
                        {
                            entryStream.CopyTo(fileStream);
                        }

                        // 更新配置文件时间
                        _configFileTimes[entry.Name] = File.GetLastWriteTime(targetPath);

                        // 触发配置变更事件
                        OnConfigChanged(entry.Name);
                    }
                }

                Logger.Info($"配置导入成功: {importPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("导入配置失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 验证导入文件
        /// </summary>
        /// <param name="importPath">导入文件路径</param>
        /// <returns>是否验证通过</returns>
        private bool VerifyImportFile(string importPath)
        {
            try
            {
                using (FileStream zipFile = File.OpenRead(importPath))
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                {
                    // 检查版本文件
                    var versionEntry = archive.GetEntry("version.txt");
                    if (versionEntry == null)
                    {
                        Logger.Error("导入文件缺少版本信息");
                        return false;
                    }

                    // 读取版本信息
                    using (StreamReader reader = new StreamReader(versionEntry.Open()))
                    {
                        string content = reader.ReadToEnd();
                        if (!content.Contains("Version: 1.0"))
                        {
                            Logger.Error("导入文件版本不兼容");
                            return false;
                        }
                    }

                    // 检查是否包含必要的配置文件
                    bool hasSystemConfig = archive.Entries.Any(e => e.Name == "SystemCfg.xml");
                    if (!hasSystemConfig)
                    {
                        Logger.Error("导入文件缺少SystemCfg.xml");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("验证导入文件失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 备份当前配置
        /// </summary>
        private void BackupCurrentConfig()
        {
            try
            {
                string backupDir = Path.Combine(_backupPath, $"ImportBackup_{DateTime.Now.ToString("yyyyMMddHHmmss")}");
                Directory.CreateDirectory(backupDir);

                string[] configFiles = Directory.GetFiles(_configPath, "*.xml");
                foreach (string file in configFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string targetPath = Path.Combine(backupDir, fileName);
                    File.Copy(file, targetPath, true);
                }

                Logger.Info("当前配置备份成功");
            }
            catch (IOException ex)
            {
                Logger.Error("配置文件IO操作失败", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("配置文件访问权限不足", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("备份当前配置失败", ex);
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <param name="errors">错误信息</param>
        /// <returns>是否验证通过</returns>
        public bool ValidateConfig<T>(T config, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                // 根据配置类型进行不同的验证
                if (config is MotionConfig motionConfig)
                {
                    ValidateMotionConfig(motionConfig, errors);
                }
                else if (config is PlcConfig plcConfig)
                {
                    ValidatePlcConfig(plcConfig, errors);
                }
                else if (config is IoConfig ioConfig)
                {
                    ValidateIoConfig(ioConfig, errors);
                }
                else if (config is SystemConfig systemConfig)
                {
                    ValidateSystemConfig(systemConfig, errors);
                }

                return errors.Count == 0;
            }
            catch (Exception ex)
            {
                Logger.Error("验证配置失败", ex);
                errors.Add("验证配置时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 验证运动卡配置
        /// </summary>
        /// <param name="config">运动卡配置</param>
        /// <param name="errors">错误信息</param>
        private void ValidateMotionConfig(MotionConfig config, List<string> errors)
        {
            // 验证轴数量
            if (config.AxisCount < 1 || config.AxisCount > 16)
            {
                errors.Add("轴数量必须在1-16之间");
            }

            // 验证轴参数
            foreach (var axisParam in config.AxisParams)
            {
                // 验证轴号
                if (axisParam.AxisNo < 0 || axisParam.AxisNo >= config.AxisCount)
                {
                    errors.Add($"轴号 {axisParam.AxisNo} 超出范围");
                }

                // 验证速度
                if (axisParam.Speed < 0 || axisParam.Speed > 2000)
                {
                    errors.Add($"轴 {axisParam.AxisNo} 速度超出范围（0-2000mm/s）");
                }

                // 验证加速度
                if (axisParam.Acceleration < 0 || axisParam.Acceleration > 10000)
                {
                    errors.Add($"轴 {axisParam.AxisNo} 加速度超出范围（0-10000mm/s²）");
                }

                // 验证限位
                if (axisParam.PositiveLimit <= axisParam.NegativeLimit)
                {
                    errors.Add($"轴 {axisParam.AxisNo} 正限位必须大于负限位");
                }
            }

            // 验证轴号是否重复
            HashSet<int> axisNos = new HashSet<int>();
            foreach (var axisParam in config.AxisParams)
            {
                if (!axisNos.Add(axisParam.AxisNo))
                {
                    errors.Add($"轴号 {axisParam.AxisNo} 重复");
                }
            }
        }

        /// <summary>
        /// 验证PLC配置
        /// </summary>
        /// <param name="config">PLC配置</param>
        /// <param name="errors">错误信息</param>
        private void ValidatePlcConfig(PlcConfig config, List<string> errors)
        {
            // 验证IP地址
            if (!System.Net.IPAddress.TryParse(config.IP, out _))
            {
                errors.Add("PLC IP地址格式不正确");
            }

            // 验证端口
            if (config.Port < 1 || config.Port > 65535)
            {
                errors.Add("PLC端口必须在1-65535之间");
            }
        }

        /// <summary>
        /// 验证IO配置
        /// </summary>
        /// <param name="config">IO配置</param>
        /// <param name="errors">错误信息</param>
        private void ValidateIoConfig(IoConfig config, List<string> errors)
        {
            // 验证输入输出点位是否重复
            HashSet<string> ioPoints = new HashSet<string>();
            foreach (var input in config.Inputs)
            {
                string key = $"{input.Port}:{input.Pin}";
                if (!ioPoints.Add(key))
                {
                    errors.Add($"输入点位 {key} 重复");
                }
            }

            foreach (var output in config.Outputs)
            {
                string key = $"{output.Port}:{output.Pin}";
                if (!ioPoints.Add(key))
                {
                    errors.Add($"输出点位 {key} 重复或与输入点位冲突");
                }
            }
        }

        /// <summary>
        /// 验证系统配置
        /// </summary>
        /// <param name="config">系统配置</param>
        /// <param name="errors">错误信息</param>
        private void ValidateSystemConfig(SystemConfig config, List<string> errors)
        {
            // 验证日志路径
            if (string.IsNullOrEmpty(config.LogPath))
            {
                errors.Add("日志路径不能为空");
            }
            else
            {
                try
                {
                    string logPath = Path.Combine(AppContext.BaseDirectory, config.LogPath);
                    FileHelper.EnsureDirectoryExists(logPath);
                }
                catch
                {
                    errors.Add("日志路径无效");
                }
            }

            // 验证看门狗参数
            if (config.WatchdogInterval < 1000 || config.WatchdogInterval > 60000)
            {
                errors.Add("看门狗间隔必须在1000-60000ms之间");
            }
        }
    }
}
