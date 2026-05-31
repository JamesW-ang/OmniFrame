using System;

namespace OmniFrame.Core
{
    [Serializable]
    public class AxisParam
    {
        public int AxisNo { get; set; }
        public double Speed { get; set; }
        public double Acceleration { get; set; }
        public double PositiveLimit { get; set; }
        public double NegativeLimit { get; set; }
    }
}
