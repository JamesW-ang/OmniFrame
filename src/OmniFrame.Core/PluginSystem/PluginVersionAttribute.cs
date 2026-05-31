using System;
using OmniFrame.Sdk.PluginSystem;

namespace OmniFrame.Core.PluginSystem
{
    /// <summary>
    /// 插件版本特性
        /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MotionPluginVersionAttribute : Attribute
    {
        private readonly Version _version;

        /// <summary>
        /// 版本号
        /// </summary>
        public Version Version => _version;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="version">版本号字符串，格式：x.y.z</param>
        public MotionPluginVersionAttribute(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));

            if (!Version.TryParse(version, out Version parsedVersion))
                throw new ArgumentException($"无效的版本号格式: {version}", nameof(version));

            _version = parsedVersion;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="major">主版本号</param>
        /// <param name="minor">次版本号</param>
        /// <param name="build">构建号</param>
        public MotionPluginVersionAttribute(int major, int minor, int build)
        {
            _version = new Version(major, minor, build);
        }
    }
}