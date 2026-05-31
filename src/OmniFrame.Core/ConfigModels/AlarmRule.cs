using System;

namespace OmniFrame.Core
{
    [Serializable]
    public class AlarmRule
    {
        public string Name { get; set; }
        public string Condition { get; set; }
        public string Level { get; set; }
    }
}
