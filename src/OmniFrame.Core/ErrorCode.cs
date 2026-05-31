using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 错误码枚举
        /// </summary>
    public enum ErrorCode
    {
        // 系统级错误 (1xxx)
        SystemCrash = 1001,            // 主程序崩溃
        ConfigFileCorrupted = 1002,     // 配置文件损坏
        PluginLoadFailed = 1003,        // 插件加载失败
        DatabaseError = 1004,           // 数据库错误
        MemoryError = 1005,             // 内存错误
        ResourceError = 1006,           // 资源错误
        SystemTimeout = 1007,           // 系统超时
        LicenseError = 1008,            // 许可证错误
        SystemOverload = 1009,          // 系统过载
        SystemUnknown = 1999,           // 系统未知错误

        // 通讯类错误 (2xxx)
        PlcDisconnect = 2001,           // PLC断连
        MotionCardCommError = 2002,     // 运动卡通讯失败
        IoModuleDisconnect = 2003,       // IO模块失联
        NetworkError = 2004,            // 网络错误
        MqttError = 2005,               // MQTT通讯错误
        OpcError = 2006,                // OPC通讯错误
        SerialPortError = 2007,          // 串口通讯错误
        TcpError = 2008,                // TCP通讯错误
        MesCommError = 2009,            // MES通讯错误
        CommunicationUnknown = 2999,     // 通讯未知错误

        // 运动控制错误 (3xxx)
        XAxisOverTravel = 3001,         // X轴超程
        AxisAlarm = 3002,               // 轴报警
        MotionTimeout = 3003,           // 运动超时
        YAxisOverTravel = 3004,         // Y轴超程
        ZAxisOverTravel = 3005,         // Z轴超程
        MotionCommandError = 3006,      // 运动命令错误
        ServoError = 3007,              // 伺服错误
        EncoderError = 3008,            // 编码器错误
        HomeFailed = 3009,              // 回原点失败
        MotionUnknown = 3999,           // 运动控制未知错误

        // 业务类错误 (4xxx)
        ProductNotFound = 4001,         // 料号不存在
        YieldRateBelowThreshold = 4002,  // 良率低于阈值
        StationBlocked = 4003,           // 工位堵料
        WorkOrderError = 4004,          // 工单错误
        RecipeError = 4005,             // 配方错误
        MaterialError = 4006,           // 物料错误
        QualityError = 4007,            // 质量错误
        ProductionError = 4008,         // 生产错误
        EquipmentError = 4009,          // 设备错误
        BusinessUnknown = 4999,         // 业务未知错误

        // 未知错误
        UnknownError = 9999             // 未知错误
    }

    /// <summary>
    /// 错误码扩展
        /// </summary>
    public static class ErrorCodeExtensions
    {
        /// <summary>
        /// 获取错误码描述
        /// </summary>
        public static string GetDescription(this ErrorCode errorCode)
        {
            switch (errorCode)
            {
                // 系统级错误
                case ErrorCode.SystemCrash: return "主程序崩溃";
                case ErrorCode.ConfigFileCorrupted: return "配置文件损坏";
                case ErrorCode.PluginLoadFailed: return "插件加载失败";
                case ErrorCode.DatabaseError: return "数据库错误";
                case ErrorCode.MemoryError: return "内存错误";
                case ErrorCode.ResourceError: return "资源错误";
                case ErrorCode.SystemTimeout: return "系统超时";
                case ErrorCode.LicenseError: return "许可证错误";
                case ErrorCode.SystemOverload: return "系统过载";
                case ErrorCode.SystemUnknown: return "系统未知错误";

                // 通讯类错误
                case ErrorCode.PlcDisconnect: return "PLC断连";
                case ErrorCode.MotionCardCommError: return "运动卡通讯失败";
                case ErrorCode.IoModuleDisconnect: return "IO模块失联";
                case ErrorCode.NetworkError: return "网络错误";
                case ErrorCode.MqttError: return "MQTT通讯错误";
                case ErrorCode.OpcError: return "OPC通讯错误";
                case ErrorCode.SerialPortError: return "串口通讯错误";
                case ErrorCode.TcpError: return "TCP通讯错误";
                case ErrorCode.MesCommError: return "MES通讯错误";
                case ErrorCode.CommunicationUnknown: return "通讯未知错误";

                // 运动控制错误
                case ErrorCode.XAxisOverTravel: return "X轴超程";
                case ErrorCode.AxisAlarm: return "轴报警";
                case ErrorCode.MotionTimeout: return "运动超时";
                case ErrorCode.YAxisOverTravel: return "Y轴超程";
                case ErrorCode.ZAxisOverTravel: return "Z轴超程";
                case ErrorCode.MotionCommandError: return "运动命令错误";
                case ErrorCode.ServoError: return "伺服错误";
                case ErrorCode.EncoderError: return "编码器错误";
                case ErrorCode.HomeFailed: return "回原点失败";
                case ErrorCode.MotionUnknown: return "运动控制未知错误";

                // 业务类错误
                case ErrorCode.ProductNotFound: return "料号不存在";
                case ErrorCode.YieldRateBelowThreshold: return "良率低于阈值";
                case ErrorCode.StationBlocked: return "工位堵料";
                case ErrorCode.WorkOrderError: return "工单错误";
                case ErrorCode.RecipeError: return "配方错误";
                case ErrorCode.MaterialError: return "物料错误";
                case ErrorCode.QualityError: return "质量错误";
                case ErrorCode.ProductionError: return "生产错误";
                case ErrorCode.EquipmentError: return "设备错误";
                case ErrorCode.BusinessUnknown: return "业务未知错误";

                case ErrorCode.UnknownError: return "未知错误";
                default: return "未知错误";
            }
        }

        /// <summary>
        /// 获取错误级别
        /// </summary>
        public static AlarmLevel GetAlarmLevel(this ErrorCode errorCode)
        {
            int code = (int)errorCode;
            
            // 系统级错误和通讯类错误通常为严重级别
            if (code >= 1000 && code < 3000)
                return AlarmLevel.Critical;
            // 运动控制错误通常为错误级别
            else if (code >= 3000 && code < 4000)
                return AlarmLevel.Error;
            // 业务类错误通常为警告级别
            else if (code >= 4000 && code < 5000)
                return AlarmLevel.Warning;
            else
                return AlarmLevel.Error;
        }

        /// <summary>
        /// 获取错误来源
        /// </summary>
        public static string GetSource(this ErrorCode errorCode)
        {
            int code = (int)errorCode;
            
            if (code >= 1000 && code < 2000)
                return "System";
            else if (code >= 2000 && code < 3000)
                return "Communication";
            else if (code >= 3000 && code < 4000)
                return "Motion";
            else if (code >= 4000 && code < 5000)
                return "Business";
            else
                return "Unknown";
        }
    }

    /// <summary>
    /// 错误排查指南
        /// </summary>
    public class ErrorGuide
    {
        private static Dictionary<ErrorCode, string> _guideMap = new Dictionary<ErrorCode, string>
        {
            // 系统级错误
            { ErrorCode.SystemCrash, "排查步骤：① 检查系统日志 ② 检查内存使用 ③ 重启应用程序 ④ 检查硬件状态" },
            { ErrorCode.ConfigFileCorrupted, "排查步骤：① 检查配置文件完整性 ② 从备份恢复配置 ③ 重新生成配置文件" },
            { ErrorCode.PluginLoadFailed, "排查步骤：① 检查插件文件是否存在 ② 检查插件版本兼容性 ③ 重新安装插件" },
            { ErrorCode.DatabaseError, "排查步骤：① 检查数据库连接 ② 检查数据库文件权限 ③ 检查数据库完整性" },
            
            // 通讯类错误
            { ErrorCode.PlcDisconnect, "排查步骤：① 检查PLC电源 ② 检查网线连接 ③ ping PLC IP ④ 重启PLC" },
            { ErrorCode.MotionCardCommError, "排查步骤：① 检查运动卡电源 ② 检查通讯线缆 ③ 检查驱动安装 ④ 重启运动卡" },
            { ErrorCode.IoModuleDisconnect, "排查步骤：① 检查IO模块电源 ② 检查通讯线缆 ③ 检查模块地址设置 ④ 重启IO模块" },
            
            // 运动控制错误
            { ErrorCode.XAxisOverTravel, "排查步骤：① 检查机械限位 ② 检查行程参数 ③ 手动复位轴 ④ 重新回原点" },
            { ErrorCode.AxisAlarm, "排查步骤：① 检查伺服驱动器报警代码 ② 检查机械负载 ③ 检查电机温度 ④ 复位报警" },
            { ErrorCode.MotionTimeout, "排查步骤：① 检查机械卡顿 ② 检查伺服参数 ③ 检查运动指令 ④ 增加超时时间" },
            
            // 业务类错误
            { ErrorCode.ProductNotFound, "排查步骤：① 检查料号是否存在 ② 检查料号输入是否正确 ③ 检查料号数据库" },
            { ErrorCode.YieldRateBelowThreshold, "排查步骤：① 检查生产工艺 ② 检查原材料质量 ③ 检查设备状态 ④ 调整工艺参数" },
            { ErrorCode.StationBlocked, "排查步骤：① 检查工位物料 ② 检查传感器状态 ③ 检查机械结构 ④ 手动清理工位" }
        };

        /// <summary>
        /// 获取错误排查指南
        /// </summary>
        public static string GetGuide(ErrorCode errorCode)
        {
            if (_guideMap.TryGetValue(errorCode, out string guide))
                return guide;
            return "请参考系统手册或联系技术支持";
        }

        /// <summary>
        /// 获取错误排查指南
        /// </summary>
        public static string GetGuide(int errorCode)
        {
            if (Enum.IsDefined(typeof(ErrorCode), errorCode))
            {
                return GetGuide((ErrorCode)errorCode);
            }
            return "请参考系统手册或联系技术支持";
        }
    }
}