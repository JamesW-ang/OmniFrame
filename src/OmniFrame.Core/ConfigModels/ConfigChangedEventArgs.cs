using System;

namespace OmniFrame.Core
{
    public class ConfigChangedEventArgs : EventArgs
    {
        public string ConfigFileName { get; set; }
    }
}
