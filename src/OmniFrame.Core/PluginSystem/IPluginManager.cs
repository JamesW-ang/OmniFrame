using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OmniFrame.Core.PluginSystem
{
    public interface IPluginManager : IDisposable
    {
        bool Initialize(string pluginPath = null);
        void ScanPlugins();
        bool LoadPlugin(string pluginName, Version version = null);
        void UnloadPlugin(string pluginName);
        Task<bool> UpdatePlugin(string pluginName, string updateUrl);
        List<PluginInfo> GetPlugins();
        List<PluginInfo> GetPluginsByType(PluginType type);
        T GetLoadedPlugin<T>(string name) where T : OmniFrame.Sdk.PluginSystem.PluginBase;
    }
}
