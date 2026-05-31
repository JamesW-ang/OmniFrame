using System;

namespace OmniFrame.Core
{
    [Serializable]
    public class TriggerLogic
    {
        public string Name { get; set; }
        public string InputCondition { get; set; }
        public string OutputAction { get; set; }
    }
}
