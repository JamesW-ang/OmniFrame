using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 操作权限定义
        /// </summary>
    public static class OperationPermissions
    {
        public const UserLevel Operator = UserLevel.Operator;
        public const UserLevel Engineer = UserLevel.Engineer;
        public const UserLevel Administrator = UserLevel.Administrator;

        public const string StartStation = "启动工位";
        public const string StopStation = "停止工位";
        public const string ViewStatus = "查看运行状态";
        public const string ViewAlarmLog = "查看报警日志";
        public const string ModifyParameters = "修改运行参数";
        public const string ClearAlarm = "清除报警";
        public const string SingleAxisDebug = "单轴调试";
        public const string ViewAllLogs = "查看所有日志";
        public const string ModifySystemConfig = "修改系统配置";
        public const string ManagePlugins = "管理插件";
        public const string ManageUsers = "管理用户";
        public const string RemoteDebug = "远程调试";

        // English aliases for non-Chinese-speaking developers
        public const string StartStationEn = "StartStation";
        public const string StopStationEn = "StopStation";
        public const string ViewStatusEn = "ViewStatus";
        public const string ViewAlarmLogEn = "ViewAlarmLog";
        public const string ModifyParametersEn = "ModifyParameters";
        public const string ClearAlarmEn = "ClearAlarm";
        public const string SingleAxisDebugEn = "SingleAxisDebug";
        public const string ViewAllLogsEn = "ViewAllLogs";
        public const string ModifySystemConfigEn = "ModifySystemConfig";
        public const string ManagePluginsEn = "ManagePlugins";
        public const string ManageUsersEn = "ManageUsers";
        public const string RemoteDebugEn = "RemoteDebug";
    }
}
