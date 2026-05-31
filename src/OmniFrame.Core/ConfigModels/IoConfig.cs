using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    [Serializable]
    public class IoConfig
    {
        public List<IoPoint> Inputs { get; set; }
        public List<IoPoint> Outputs { get; set; }
        public List<TriggerLogic> TriggerLogics { get; set; }

        public IoConfig()
        {
            Inputs = new List<IoPoint>();
            Outputs = new List<IoPoint>();
            TriggerLogics = new List<TriggerLogic>();
        }
    }
}
