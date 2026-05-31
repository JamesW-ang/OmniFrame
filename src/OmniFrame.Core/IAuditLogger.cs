using System;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 审计日志记录器接口
    /// </summary>
    public interface IAuditLogger : IDisposable
    {
        void Initialize();
        void LogOperation(string userId, string userName, UserLevel userLevel, string operation, bool success);
        void LogParameterChange(string userId, string userName, UserLevel userLevel, string operation, string parameters, string oldValue, string newValue);
    }
}
