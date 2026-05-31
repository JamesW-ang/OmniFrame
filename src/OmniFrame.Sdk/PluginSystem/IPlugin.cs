using System;
using System.Linq;

namespace OmniFrame.Sdk.PluginSystem
{
    /// <summary>
    /// 插件版本特性
        /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginVersionAttribute : Attribute
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
        public PluginVersionAttribute(string version)
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
        public PluginVersionAttribute(int major, int minor, int build)
        {
            _version = new Version(major, minor, build);
        }
    }

    /// <summary>
    /// 插件基类
        /// </summary>
    public abstract class PluginBase
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <returns>初始化是否成功</returns>
        public abstract bool Initialize();

        /// <summary>
        /// 卸载插件
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// 获取插件版本
        /// </summary>
        /// <returns>插件版本</returns>
        public Version GetVersion()
        {
            var attribute = GetType().GetCustomAttributes(typeof(PluginVersionAttribute), false)
                .FirstOrDefault() as PluginVersionAttribute;

            return attribute?.Version ?? new Version(0, 0, 0);
        }
    }

    /// <summary>
    /// 运动卡插件接口
        /// </summary>
    public abstract class MotionPlugin : PluginBase
    {
        /// <summary>
        /// 连接运动卡
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>连接是否成功</returns>
        public abstract bool Connect(string ipAddress);

        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// 轴运动
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <param name="position">目标位置</param>
        /// <param name="speed">速度</param>
        /// <returns>运动是否成功</returns>
        public abstract bool Move(int axisId, double position, double speed);

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <returns>回原点是否成功</returns>
        public abstract bool Home(int axisId);

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <returns>当前位置</returns>
        public abstract double GetCurrentPosition(int axisId);
    }

    /// <summary>
    /// PLC插件接口
        /// </summary>
    public abstract class PlcPlugin : PluginBase
    {
        /// <summary>
        /// 连接PLC
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>连接是否成功</returns>
        public abstract bool Connect(string ipAddress, int port);

        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// 读取寄存器
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>寄存器值</returns>
        public abstract int ReadRegister(string address);

        /// <summary>
        /// 写入寄存器
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">值</param>
        /// <returns>写入是否成功</returns>
        public abstract bool WriteRegister(string address, int value);
    }

    /// <summary>
    /// 自定义业务插件接口
        /// </summary>
    public abstract class BusinessPlugin : PluginBase
    {
        /// <summary>
        /// 执行业务逻辑
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns>执行结果</returns>
        public abstract object Execute(object parameters);
    }
}
