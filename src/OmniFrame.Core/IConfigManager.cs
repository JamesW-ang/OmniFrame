using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 配置管理器接口
    /// </summary>
    public interface IConfigManager
    {
        event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        T GetConfig<T>(string configFileName, T defaultValue = default(T));
        T GetEncryptedConfig<T>(string configFileName, string encryptionKey, T defaultValue = default(T));
        bool SaveConfig<T>(string configFileName, T config);
        bool SaveEncryptedConfig<T>(string configFileName, T config, string encryptionKey);
        bool RollbackConfig(string backupFileName);
        List<string> GetBackupFiles();
        bool ExportConfig(string exportPath);
        bool ImportConfig(string importPath);
        bool ValidateConfig<T>(T config, out List<string> errors);
        void StartWatching();
        void StopWatching();
        void StartAutoBackup(int intervalHours = 6);
        void StopAutoBackup();
    }
}
