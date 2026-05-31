using System;
using System.Threading.Tasks;

namespace OmniFrame.Core
{
    /// <summary>
    /// 告警通知管理器接口
    /// </summary>
    public interface IAlarmNotification : IDisposable
    {
        Task SendNotification(AlarmInfo alarm, string webhookUrl = null, string emailRecipients = null, string smsRecipients = null);
    }
}
