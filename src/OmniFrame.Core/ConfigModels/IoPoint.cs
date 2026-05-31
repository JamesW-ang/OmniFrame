using System;

namespace OmniFrame.Core
{
    [Serializable]
    public class IoPoint
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public int Pin { get; set; }
    }
}
