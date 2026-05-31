using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OmniFrame.Common
{
    /// <summary>
    /// INI文件操作类
    /// 设计介绍：
    /// 2. 封装了Windows API的WritePrivateProfileString和GetPrivateProfileString函数
    /// 3. 提供读取、写入、删除节和键的功能
    /// 5. 提供文件存在性检查和创建功能
        /// </summary>
    public class IniHelper
    {
        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string iniPath);

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="buffer">缓冲区</param>
        /// <param name="bufferSize">缓冲区大小</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder buffer, int bufferSize, string iniPath);

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static string ReadIni(string section, string key, string defaultValue, string iniPath)
        {
            StringBuilder buffer = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, buffer, buffer.Capacity, iniPath);
            return buffer.ToString();
        }

        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static bool WriteIni(string section, string key, string value, string iniPath)
        {
            return WritePrivateProfileString(section, key, value, iniPath);
        }

        /// <summary>
        /// 删除节
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static bool DeleteSection(string section, string iniPath)
        {
            return WritePrivateProfileString(section, null, null, iniPath);
        }

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static bool DeleteKey(string section, string key, string iniPath)
        {
            return WritePrivateProfileString(section, key, null, iniPath);
        }

        /// <summary>
        /// 检查INI文件是否存在
        /// </summary>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static bool Exists(string iniPath)
        {
            return File.Exists(iniPath);
        }

        /// <summary>
        /// 创建INI文件
        /// </summary>
        /// <param name="iniPath">INI文件路径</param>
        /// <returns></returns>
        public static bool Create(string iniPath)
        {
            try
            {
                if (!Exists(iniPath))
                {
                    using (File.Create(iniPath)) { }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"创建默认INI文件失败, Path={iniPath}: {ex.Message}", ex);
                return false;
            }
        }
    }
}