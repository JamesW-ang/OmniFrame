using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    [Serializable]
    public class MotionConfig
    {
        public string Brand { get; set; }
        public string IP { get; set; }
        public int AxisCount { get; set; }
        public List<AxisParam> AxisParams { get; set; }

        public MotionConfig()
        {
            AxisParams = new List<AxisParam>();
        }
    }
}
