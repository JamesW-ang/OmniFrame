using System;
using System.Collections.Generic;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public class CylinderManager : ICylinderManager
    {
        private readonly Dictionary<int, bool> _cylinderStates = new Dictionary<int, bool>();
        private readonly object _lock = new object();

        public CylinderManager() { }

        public bool Extend(int cylinderId)
        {
            lock (_lock)
            {
                _cylinderStates[cylinderId] = true;
            }
            Logger.Info($"气缸 {cylinderId} 伸出");
            return true;
        }

        public bool Retract(int cylinderId)
        {
            lock (_lock)
            {
                _cylinderStates[cylinderId] = false;
            }
            Logger.Info($"气缸 {cylinderId} 缩回");
            return true;
        }

        public bool GetStatus(int cylinderId)
        {
            lock (_lock)
            {
                return _cylinderStates.TryGetValue(cylinderId, out var state) ? state : false;
            }
        }

        public IReadOnlyDictionary<int, bool> GetAllStates()
        {
            lock (_lock)
            {
                return new Dictionary<int, bool>(_cylinderStates);
            }
        }
    }
}
