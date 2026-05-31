using System;
using OmniFrame.Common;

namespace Plc
{
    /// <summary>
    /// PLC错误事件参数
        /// </summary>
    public class PlcErrorEventArgs : EventArgs
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Time { get; set; }

        public PlcErrorEventArgs()
        {
            Time = DateTime.Now;
        }
    }

}
