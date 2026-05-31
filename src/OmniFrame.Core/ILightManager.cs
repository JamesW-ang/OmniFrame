namespace OmniFrame.Core
{
    public interface ILightManager
    {
        bool SetLight(int lightId, bool on);
        bool SetLightColor(int lightId, string color);
        bool SetLightBlink(int lightId, int interval);
        LightState GetLightState(int lightId);
    }
}
