using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    [Serializable]
    public class SystemConfig
    {
        public string LogPath { get; set; }
        public string DataSavePath { get; set; }
        public int WatchdogInterval { get; set; }
        public bool EnableWatchdog { get; set; } = true;
        public bool EnableAutoReconnect { get; set; } = true;
        public int HealthPort { get; set; } = 8080;
        public NetworkConfig NetworkConfig { get; set; }
        public List<AlarmRule> AlarmRules { get; set; }

        public SystemConfig()
        {
            AlarmRules = new List<AlarmRule>();
            NetworkConfig = new NetworkConfig();
        }
    }
}
