using System.Collections.Generic;

namespace OmniFrame.Core
{
    public interface ICylinderManager
    {
        bool Extend(int cylinderId);
        bool Retract(int cylinderId);
        bool GetStatus(int cylinderId);
        IReadOnlyDictionary<int, bool> GetAllStates();
    }
}
