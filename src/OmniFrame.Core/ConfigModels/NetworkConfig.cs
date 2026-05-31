using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    [Serializable]
    public class NetworkConfig
    {
        public string PlcIp { get; set; }
        public int PlcPort { get; set; }
        public int WebApiPort { get; set; }
        public int WebSocketPort { get; set; }
        public List<string> CorsWhitelist { get; set; }

        public NetworkConfig()
        {
            PlcIp = "192.168.1.100";
            PlcPort = 502;
            WebApiPort = 8080;
            WebSocketPort = 8081;
            CorsWhitelist = new List<string> { "http://localhost:3000", "http://127.0.0.1:3000" };
        }
    }
}
