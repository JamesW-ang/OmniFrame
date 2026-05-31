using System;

namespace OmniFrame.Core
{
    public class OperationLog
    {
        public DateTime Time { get; set; }
        public string User { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public bool Success { get; set; }
    }
}
