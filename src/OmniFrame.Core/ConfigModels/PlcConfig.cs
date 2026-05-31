using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    [Serializable]
    public class PlcConfig
    {
        public string Brand { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public List<RegisterMap> RegisterMaps { get; set; }

        public PlcConfig()
        {
            RegisterMaps = new List<RegisterMap>();
        }
    }
}
