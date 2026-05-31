using System;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 配置导入导出工具
    /// 功能说明：支持配置文件的XML和JSON格式导入导出
        /// </summary>
    public class ConfigImportExport
    {
        /// <summary>
        /// 从XML文件导入配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置对象</returns>
        public static T ImportFromXml<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.Error($"XML文件不存在: {filePath}");
                    return default(T);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"从XML导入配置失败: {filePath}", ex);
                return default(T);
            }
        }

        /// <summary>
        /// 从JSON文件导入配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置对象</returns>
        public static T ImportFromJson<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.Error($"JSON文件不存在: {filePath}");
                    return default(T);
                }

                string jsonContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(jsonContent);
            }
            catch (Exception ex)
            {
                Logger.Error($"从JSON导入配置失败: {filePath}", ex);
                return default(T);
            }
        }

        /// <summary>
        /// 导出配置到XML文件
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportToXml<T>(T config, string filePath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    FileHelper.EnsureDirectoryExists(directory);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, config);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"导出配置到XML失败: {filePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 导出配置到JSON文件
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportToJson<T>(T config, string filePath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    FileHelper.EnsureDirectoryExists(directory);
                }

                string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, jsonContent);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"导出配置到JSON失败: {filePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 从文件导入配置（自动检测格式）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置对象</returns>
        public static T Import<T>(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".xml":
                    return ImportFromXml<T>(filePath);
                case ".json":
                    return ImportFromJson<T>(filePath);
                default:
                    Logger.Error($"不支持的文件格式: {extension}");
                    return default(T);
            }
        }

        /// <summary>
        /// 导出配置到文件（根据扩展名选择格式）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否导出成功</returns>
        public static bool Export<T>(T config, string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".xml":
                    return ExportToXml(config, filePath);
                case ".json":
                    return ExportToJson(config, filePath);
                default:
                    Logger.Error($"不支持的文件格式: {extension}");
                    return false;
            }
        }
    }
}
