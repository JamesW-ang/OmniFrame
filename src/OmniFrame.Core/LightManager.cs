using System;
using System.Collections.Generic;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    public record LightState(bool IsOn, string Color, int BlinkIntervalMs);

    public class LightManager : ILightManager
    {
        private readonly Dictionary<int, LightState> _lightStates = new Dictionary<int, LightState>();
        private readonly object _lock = new object();

        private static readonly HashSet<string> ValidColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Red", "Green", "Yellow", "Blue", "White", "Off"
        };

        public LightManager() { }

        public bool SetLight(int lightId, bool on)
        {
            lock (_lock)
            {
                if (!_lightStates.TryGetValue(lightId, out var state))
                {
                    state = new LightState(false, "Off", 0);
                    _lightStates[lightId] = state;
                }
                _lightStates[lightId] = state with { IsOn = on };
            }
            Logger.Info($"设置灯光 {lightId} {(on ? "开启" : "关闭")}");
            return true;
        }

        public bool SetLightColor(int lightId, string color)
        {
            if (!ValidColors.Contains(color))
            {
                Logger.Warning($"LightManager: 无效的颜色值 '{color}'，有效值: {string.Join(", ", ValidColors)}");
                return false;
            }

            lock (_lock)
            {
                if (!_lightStates.TryGetValue(lightId, out var state))
                {
                    state = new LightState(false, "Off", 0);
                    _lightStates[lightId] = state;
                }
                _lightStates[lightId] = state with { Color = color };
            }
            Logger.Info($"设置灯光 {lightId} 颜色为 {color}");
            return true;
        }

        public bool SetLightBlink(int lightId, int interval)
        {
            lock (_lock)
            {
                if (!_lightStates.TryGetValue(lightId, out var state))
                {
                    state = new LightState(false, "Off", 0);
                    _lightStates[lightId] = state;
                }
                _lightStates[lightId] = state with { BlinkIntervalMs = interval };
            }
            Logger.Info($"设置灯光 {lightId} 闪烁间隔为 {interval}ms");
            return true;
        }

        public LightState GetLightState(int lightId)
        {
            lock (_lock)
            {
                return _lightStates.TryGetValue(lightId, out var state)
                    ? state
                    : new LightState(false, "Off", 0);
            }
        }
    }
}
