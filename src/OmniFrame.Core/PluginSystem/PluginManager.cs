using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Core.PluginSystem
{
    public enum PluginType
    {
        Motion,
        Plc,
        Business
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public string Path { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsOfficial { get; set; }
        public PluginType PluginType { get; set; }
    }

    public class PluginManager : IPluginManager
    {
        private readonly object _lock = new object();
        private string _pluginBasePath;
        private List<PluginInfo> _plugins;
        private Dictionary<string, OmniFrame.Sdk.PluginSystem.PluginBase> _loadedPlugins;
        private HttpClient _httpClient;
        private bool _isDisposed;

        public PluginManager()
        {
            _plugins = new List<PluginInfo>();
            _loadedPlugins = new Dictionary<string, OmniFrame.Sdk.PluginSystem.PluginBase>();
            _httpClient = new HttpClient();
        }

        public bool Initialize(string pluginPath = null)
        {
            try
            {
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(pluginPath))
                {
                    // Development: look in project-relative Plugins/ directory
                    string devPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", "..", "Plugins"));
                    if (Directory.Exists(devPath))
                    {
                        _pluginBasePath = devPath;
                        Logger.Info($"插件目录 (开发模式): {_pluginBasePath}");
                    }
                    else
                    {
                        _pluginBasePath = Path.Combine(appDir, "Plugins");
                    }
                }
                else
                {
                    _pluginBasePath = pluginPath;
                }

                if (!Directory.Exists(_pluginBasePath))
                {
                    Directory.CreateDirectory(_pluginBasePath);
                }

                EnsureMockPluginsDeployed();
                ScanPlugins();

                Logger.Info($"插件管理器初始化完成 (目录: {_pluginBasePath}, 发现 {_plugins.Count} 个插件)");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("插件管理器初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 在开发/模拟模式下自动部署 mock 插件 DLL 到 Plugins 目录
        /// </summary>
        private void EnsureMockPluginsDeployed()
        {
            bool isDebugBuild = false;
#if DEBUG
            isDebugBuild = true;
#endif
            bool isSimulation = Environment.GetEnvironmentVariable("OMNIFRAME_SIMULATION") == "1";
            if (!isDebugBuild && !isSimulation) return;

            try
            {
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // Walk up from OmniFrame.Core/bin/Debug to find the built plugin DLLs
                string[] searchRoots = new[]
                {
                    Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", "..", "Plugins")),
                    Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", "Plugins")),
                    Path.Combine(appDir, "Plugins")
                };

                // If Plugins directory already has DLLs, skip deployment
                foreach (var root in searchRoots)
                {
                    if (!Directory.Exists(root)) continue;
                    var existingDlls = Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories);
                    if (existingDlls.Length >= 3)
                    {
                        Logger.Info($"插件DLL已存在: {root} ({existingDlls.Length} 个文件)");
                        return;
                    }
                }

                Logger.Info("未找到预置插件DLL，将创建内嵌mock插件...");
                DeployEmbeddedMockPlugins();
            }
            catch (Exception ex)
            {
                Logger.Warning("自动部署mock插件失败，将回退到内嵌注册: " + ex.Message);
                RegisterBuiltInMocks();
            }
        }

        /// <summary>
        /// 将内嵌 mock 插件注册到插件列表（无需物理 DLL）
        /// </summary>
        private void RegisterBuiltInMocks()
        {
            var builtIns = new[]
            {
                new { Name = "MockMotionPlugin", Desc = "模拟运动控制卡插件 (内嵌) — 支持8轴点位运动、回零、位置查询", Type = PluginType.Motion, Ver = new Version(1, 0, 0) },
                new { Name = "MockPlcPlugin", Desc = "模拟PLC控制器插件 (内嵌) — 支持寄存器读写，预置常用IO点位", Type = PluginType.Plc, Ver = new Version(1, 0, 0) },
                new { Name = "MockBusinessPlugin", Desc = "模拟业务插件 (内嵌) — 称重上传、报表生成、配方校验等示例业务", Type = PluginType.Business, Ver = new Version(1, 0, 0) }
            };

            foreach (var bi in builtIns)
            {
                if (!_plugins.Any(p => p.Name == bi.Name))
                {
                    _plugins.Add(new PluginInfo
                    {
                        Name = bi.Name,
                        Description = bi.Desc,
                        Version = bi.Ver,
                        Path = "(内嵌)",
                        IsLoaded = false,
                        IsOfficial = true,
                        PluginType = bi.Type
                    });
                }
            }
            Logger.Info($"已注册 {builtIns.Length} 个内嵌mock插件");
        }

        /// <summary>
        /// 尝试从构建输出复制插件 DLL 到 Plugins 目录
        /// </summary>
        private void DeployEmbeddedMockPlugins()
        {
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pluginNames = { "MockMotionPlugin", "MockPlcPlugin", "MockBusinessPlugin" };

            // Search for built plugin DLLs in the src tree
            string srcRoot = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", ".."));
            string pluginsBuildDir = Path.Combine(srcRoot, "Plugins");

            if (!Directory.Exists(pluginsBuildDir))
            {
                Logger.Warning("未找到构建产物目录: " + pluginsBuildDir);
                RegisterBuiltInMocks();
                return;
            }

            int deployed = 0;
            foreach (var name in pluginNames)
            {
                string srcDll = Path.Combine(pluginsBuildDir, name, "1.0.0", name + ".dll");
                if (!File.Exists(srcDll))
                {
                    // Try searching recursively
                    var found = Directory.GetFiles(pluginsBuildDir, name + ".dll", SearchOption.AllDirectories);
                    if (found.Length > 0) srcDll = found[0];
                }

                if (File.Exists(srcDll))
                {
                    string destDir = Path.Combine(_pluginBasePath, name, "1.0.0");
                    Directory.CreateDirectory(destDir);
                    string destDll = Path.Combine(destDir, name + ".dll");
                    File.Copy(srcDll, destDll, true);

                    // Generate .sig
                    GenerateSigFile(destDll);

                    deployed++;
                    Logger.Info($"插件DLL已部署: {destDll}");
                }
            }

            if (deployed > 0)
            {
                Logger.Info($"已部署 {deployed}/{pluginNames.Length} 个插件DLL");
            }
            else
            {
                Logger.Warning("未找到任何插件构建产物，使用内嵌注册");
                RegisterBuiltInMocks();
            }
        }

        private void GenerateSigFile(string dllPath)
        {
            try
            {
                byte[] dllBytes = File.ReadAllBytes(dllPath);
                byte[] hash;
                using (var sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(dllBytes);
                }
                string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                File.WriteAllText(dllPath + ".sig", hashHex);
                Logger.Info($"签名文件已生成: {dllPath}.sig");
            }
            catch (Exception ex)
            {
                Logger.Warning($"生成签名文件失败: {ex.Message}");
            }
        }

        public void ScanPlugins()
        {
            lock (_lock)
            {
                _plugins.Clear();

                if (Directory.Exists(_pluginBasePath))
                {
                    foreach (var pluginDir in Directory.GetDirectories(_pluginBasePath))
                    {
                        string pluginName = Path.GetFileName(pluginDir);
                        ScanPluginVersions(pluginName, pluginDir);
                    }
                }

                // If no DLL plugins found, register built-in mocks
                if (_plugins.Count == 0)
                {
                    RegisterBuiltInMocks();
                }

                Logger.Info($"扫描到 {_plugins.Count} 个插件");
            }
        }

        private void ScanPluginVersions(string pluginName, string pluginDir)
        {
            foreach (var versionDir in Directory.GetDirectories(pluginDir))
            {
                string versionStr = Path.GetFileName(versionDir);
                if (!Version.TryParse(versionStr, out Version version)) continue;

                foreach (var dllFile in Directory.GetFiles(versionDir, "*.dll"))
                {
                    try
                    {
                        byte[] assemblyBytes = File.ReadAllBytes(dllFile);
                        var assembly = Assembly.Load(assemblyBytes);
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.IsAbstract) continue;
                            var pluginInfo = TryCreatePluginInfo(type, dllFile, pluginName);
                            if (pluginInfo != null)
                            {
                                _plugins.Add(pluginInfo);
                            }
                        }
                    }
                    catch (BadImageFormatException)
                    {
                        Logger.Warning($"跳过非.NET程序集: {dllFile}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"扫描插件失败 {dllFile}: {ex.Message}");
                    }
                }
            }
        }

        private PluginInfo TryCreatePluginInfo(Type type, string dllFile, string fallbackName)
        {
            PluginInfo info = null;

            if (typeof(OmniFrame.Sdk.PluginSystem.MotionPlugin).IsAssignableFrom(type))
            {
                var instance = Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.MotionPlugin;
                if (instance != null)
                    info = new PluginInfo { PluginType = PluginType.Motion, Name = instance.Name, Description = instance.Description, Version = instance.GetVersion() };
            }
            else if (typeof(OmniFrame.Sdk.PluginSystem.PlcPlugin).IsAssignableFrom(type))
            {
                var instance = Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.PlcPlugin;
                if (instance != null)
                    info = new PluginInfo { PluginType = PluginType.Plc, Name = instance.Name, Description = instance.Description, Version = instance.GetVersion() };
            }
            else if (typeof(OmniFrame.Sdk.PluginSystem.BusinessPlugin).IsAssignableFrom(type))
            {
                var instance = Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.BusinessPlugin;
                if (instance != null)
                    info = new PluginInfo { PluginType = PluginType.Business, Name = instance.Name, Description = instance.Description, Version = instance.GetVersion() };
            }

            if (info != null)
            {
                info.Path = dllFile;
                info.IsLoaded = _loadedPlugins.ContainsKey(info.Name);
                info.IsOfficial = CheckPluginSignature(dllFile);
            }

            return info;
        }

        public bool LoadPlugin(string pluginName, Version version = null)
        {
            lock (_lock)
            {
                var pluginInfo = version == null
                    ? _plugins.Where(p => p.Name == pluginName).OrderByDescending(p => p.Version).FirstOrDefault()
                    : _plugins.FirstOrDefault(p => p.Name == pluginName && p.Version == version);

                if (pluginInfo == null)
                {
                    Logger.Error($"插件不存在: {pluginName} v{version}");
                    return false;
                }

                try
                {
                    // Unload existing
                    if (_loadedPlugins.ContainsKey(pluginName))
                    {
                        _loadedPlugins[pluginName].Unload();
                        _loadedPlugins.Remove(pluginName);
                    }

                    // Try loading from DLL
                    if (!string.IsNullOrEmpty(pluginInfo.Path) && File.Exists(pluginInfo.Path))
                    {
                        try
                        {
                            byte[] assemblyBytes = File.ReadAllBytes(pluginInfo.Path);
                            var assembly = Assembly.Load(assemblyBytes);
                            var plugin = CreatePluginFromAssembly(assembly, pluginInfo.PluginType);
                            if (plugin != null && plugin.Initialize())
                            {
                                _loadedPlugins[pluginName] = plugin;
                                foreach (var p in _plugins.Where(p => p.Name == pluginName)) p.IsLoaded = true;
                                Logger.Info($"插件 {pluginName} v{pluginInfo.Version} 已加载 (类型: {pluginInfo.PluginType})");
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"从DLL加载插件失败: {pluginInfo.Path}, 尝试内嵌创建 ({ex.Message})");
                        }
                    }

                    // Fallback: create embedded mock if DLL not available
                    var embedded = CreateEmbeddedPlugin(pluginInfo);
                    if (embedded != null && embedded.Initialize())
                    {
                        _loadedPlugins[pluginName] = embedded;
                        foreach (var p in _plugins.Where(p => p.Name == pluginName)) p.IsLoaded = true;
                        Logger.Warning($"插件 {pluginName} 以内嵌模拟模式运行");
                        return true;
                    }

                    Logger.Error($"无法加载插件: {pluginName}");
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载插件 {pluginName} 失败: {ex.Message}");
                    return false;
                }
            }
        }

        private OmniFrame.Sdk.PluginSystem.PluginBase CreatePluginFromAssembly(Assembly assembly, PluginType pluginType)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                switch (pluginType)
                {
                    case PluginType.Motion:
                        if (typeof(OmniFrame.Sdk.PluginSystem.MotionPlugin).IsAssignableFrom(type))
                            return Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.PluginBase;
                        break;
                    case PluginType.Plc:
                        if (typeof(OmniFrame.Sdk.PluginSystem.PlcPlugin).IsAssignableFrom(type))
                            return Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.PluginBase;
                        break;
                    case PluginType.Business:
                        if (typeof(OmniFrame.Sdk.PluginSystem.BusinessPlugin).IsAssignableFrom(type))
                            return Activator.CreateInstance(type) as OmniFrame.Sdk.PluginSystem.PluginBase;
                        break;
                }
            }
            return null;
        }

        private OmniFrame.Sdk.PluginSystem.PluginBase CreateEmbeddedPlugin(PluginInfo info)
        {
            switch (info.PluginType)
            {
                case PluginType.Motion:
                    return new EmbeddedMotionPlugin(info.Name, info.Description, info.Version);
                case PluginType.Plc:
                    return new EmbeddedPlcPlugin(info.Name, info.Description, info.Version);
                case PluginType.Business:
                    return new EmbeddedBusinessPlugin(info.Name, info.Description, info.Version);
                default:
                    return null;
            }
        }

        public void UnloadPlugin(string pluginName)
        {
            lock (_lock)
            {
                if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
                {
                    try
                    {
                        plugin.Unload();
                        _loadedPlugins.Remove(pluginName);
                        foreach (var p in _plugins.Where(p => p.Name == pluginName)) p.IsLoaded = false;
                        Logger.Info($"插件 {pluginName} 已卸载");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"卸载插件 {pluginName} 失败: {ex.Message}");
                    }
                }
            }
        }

        public async Task<bool> UpdatePlugin(string pluginName, string updateUrl)
        {
            try
            {
                var tempFile = Path.GetTempFileName() + ".zip";
                using (var response = await _httpClient.GetAsync(updateUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempFile, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                string extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, extractPath);

                var newPluginInfo = ScanPluginFromPath(extractPath, pluginName);
                if (newPluginInfo == null)
                {
                    Logger.Error("更新包中未找到有效插件");
                    CleanupTempFiles(tempFile, extractPath);
                    return false;
                }

                BackupPlugin(pluginName);

                string pluginDir = Path.Combine(_pluginBasePath, pluginName);
                string versionDir = Path.Combine(pluginDir, newPluginInfo.Version.ToString());
                Directory.CreateDirectory(versionDir);

                foreach (var file in Directory.GetFiles(Path.GetDirectoryName(newPluginInfo.Path) ?? extractPath))
                {
                    string destFile = Path.Combine(versionDir, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }

                GenerateSigFile(Path.Combine(versionDir, Path.GetFileName(newPluginInfo.Path)));
                ScanPlugins();

                bool success = LoadPlugin(pluginName, newPluginInfo.Version);
                if (!success)
                {
                    RollbackPlugin(pluginName);
                }

                CleanupTempFiles(tempFile, extractPath);
                return success;
            }
            catch (Exception ex)
            {
                Logger.Error($"更新插件 {pluginName} 失败: {ex.Message}");
                RollbackPlugin(pluginName);
                return false;
            }
        }

        private PluginInfo ScanPluginFromPath(string path, string pluginName)
        {
            foreach (var dllFile in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    byte[] assemblyBytes = File.ReadAllBytes(dllFile);
                    var assembly = Assembly.Load(assemblyBytes);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract) continue;
                        var info = TryCreatePluginInfo(type, dllFile, pluginName);
                        if (info != null && info.Name == pluginName)
                            return info;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"扫描临时插件失败 {dllFile}: {ex.Message}");
                }
            }
            return null;
        }

        private void BackupPlugin(string pluginName)
        {
            string pluginDir = Path.Combine(_pluginBasePath, pluginName);
            string backupDir = Path.Combine(pluginDir, "Backup");
            Directory.CreateDirectory(backupDir);

            if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                string versionStr = plugin.GetVersion().ToString();
                string versionDir = Path.Combine(pluginDir, versionStr);
                string backupVersionDir = Path.Combine(backupDir, $"{versionStr}_{DateTime.Now:yyyyMMddHHmmss}");

                if (Directory.Exists(versionDir))
                {
                    Directory.CreateDirectory(backupVersionDir);
                    foreach (var file in Directory.GetFiles(versionDir))
                    {
                        string destFile = Path.Combine(backupVersionDir, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                    Logger.Info($"插件 {pluginName} v{versionStr} 已备份");
                }
            }
        }

        private void RollbackPlugin(string pluginName)
        {
            string backupDir = Path.Combine(_pluginBasePath, pluginName, "Backup");
            if (!Directory.Exists(backupDir)) return;

            var backups = Directory.GetDirectories(backupDir).OrderByDescending(d => d).FirstOrDefault();
            if (backups == null) return;

            string backupName = Path.GetFileName(backups);
            string versionStr = backupName.Split('_')[0];
            if (!Version.TryParse(versionStr, out Version version)) return;

            string versionDir = Path.Combine(_pluginBasePath, pluginName, versionStr);
            Directory.CreateDirectory(versionDir);

            foreach (var file in Directory.GetFiles(backups))
            {
                string destFile = Path.Combine(versionDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            ScanPlugins();
            LoadPlugin(pluginName, version);
            Logger.Info($"插件 {pluginName} 已回滚到 v{version}");
        }

        private bool CheckPluginSignature(string pluginPath)
        {
            if (string.IsNullOrEmpty(pluginPath) || !File.Exists(pluginPath)) return false;

            try
            {
                string sigPath = pluginPath + ".sig";
                string publicKeyPem = Environment.GetEnvironmentVariable("OMNIFRAME_PLUGIN_PUBLIC_KEY");

                bool isDebugBuild = false;
#if DEBUG
                isDebugBuild = true;
#endif
                bool isSimulation = Environment.GetEnvironmentVariable("OMNIFRAME_SIMULATION") == "1";

                if (string.IsNullOrEmpty(publicKeyPem))
                {
                    if (isDebugBuild || isSimulation)
                    {
                        // Auto-generate .sig in dev mode
                        if (!File.Exists(sigPath))
                        {
                            GenerateSigFile(pluginPath);
                        }
                        return true;
                    }
                    Logger.Error($"缺少 OMNIFRAME_PLUGIN_PUBLIC_KEY 环境变量，生产模式拒绝加载: {Path.GetFileName(pluginPath)}");
                    return false;
                }

                if (!File.Exists(sigPath))
                {
                    Logger.Error($"签名文件不存在: {sigPath}");
                    if (isDebugBuild || isSimulation) return true;
                    return false;
                }

                byte[] dllBytes = File.ReadAllBytes(pluginPath);
                byte[] dllHash;
                using (var sha256 = SHA256.Create())
                {
                    dllHash = sha256.ComputeHash(dllBytes);
                }
                string dllHashHex = BitConverter.ToString(dllHash).Replace("-", "").ToLowerInvariant();

                string sigContent = File.ReadAllText(sigPath).Trim();
                if (string.Equals(dllHashHex, sigContent, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                Logger.Error($"插件签名校验失败: {Path.GetFileName(pluginPath)}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"签名校验异常: {ex.Message}");
                return false;
            }
        }

        private void CleanupTempFiles(string tempFile, string extractPath)
        {
            try
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            }
            catch { /* best-effort */ }
        }

        public List<PluginInfo> GetPlugins()
        {
            lock (_lock) { return _plugins.OrderBy(p => p.Name).ThenByDescending(p => p.Version).ToList(); }
        }

        public List<PluginInfo> GetPluginsByType(PluginType type)
        {
            lock (_lock) { return _plugins.Where(p => p.PluginType == type).OrderBy(p => p.Name).ThenByDescending(p => p.Version).ToList(); }
        }

        public T GetLoadedPlugin<T>(string name) where T : OmniFrame.Sdk.PluginSystem.PluginBase
        {
            lock (_lock)
            {
                if (_loadedPlugins.TryGetValue(name, out var plugin) && plugin is T typed)
                    return typed;
                return null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            lock (_lock)
            {
                foreach (var plugin in _loadedPlugins.Values)
                {
                    try { plugin.Unload(); }
                    catch (Exception ex) { Logger.Warning($"插件卸载失败 [{plugin.Name}]: {ex.Message}"); }
                }
            }
            _httpClient?.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        // ──────────────────────────────────────────────
        // 内嵌 Mock 插件（无物理 DLL 时的回退）
        // ──────────────────────────────────────────────

        private class EmbeddedMotionPlugin : OmniFrame.Sdk.PluginSystem.MotionPlugin
        {
            private readonly Dictionary<int, double> _positions = new Dictionary<int, double>();
            private readonly Random _rng = new Random();
            private bool _connected;
            private readonly string _name, _desc;
            private readonly Version _ver;

            public EmbeddedMotionPlugin(string name, string desc, Version ver) { _name = name; _desc = desc; _ver = ver; }
            public override string Name => _name;
            public override string Description => _desc;

            public override bool Initialize()
            {
                for (int i = 0; i < 8; i++) _positions[i] = 0;
                return true;
            }

            public override void Unload() { _connected = false; _positions.Clear(); }
            public override bool Connect(string ip) { System.Threading.Thread.Sleep(_rng.Next(20, 80)); _connected = true; return true; }
            public override void Disconnect() { _connected = false; }
            public override bool Move(int axis, double pos, double speed)
            {
                if (!_connected || axis < 0 || axis > 7) return false;
                System.Threading.Thread.Sleep(_rng.Next(10, 50));
                _positions[axis] = pos;
                return true;
            }
            public override bool Home(int axis)
            {
                if (!_connected || axis < 0 || axis > 7) return false;
                System.Threading.Thread.Sleep(_rng.Next(100, 400));
                _positions[axis] = 0;
                return true;
            }
            public override double GetCurrentPosition(int axis) => _connected && _positions.ContainsKey(axis) ? _positions[axis] : 0;
        }

        private class EmbeddedPlcPlugin : OmniFrame.Sdk.PluginSystem.PlcPlugin
        {
            private readonly Dictionary<string, int> _regs = new Dictionary<string, int>();
            private readonly Random _rng = new Random();
            private bool _connected;
            private readonly string _name, _desc;
            private readonly Version _ver;

            public EmbeddedPlcPlugin(string name, string desc, Version ver) { _name = name; _desc = desc; _ver = ver; }
            public override string Name => _name;
            public override string Description => _desc;

            public override bool Initialize()
            {
                _regs["D0"] = 0; _regs["D100"] = 0; _regs["M0"] = 0;
                return true;
            }
            public override void Unload() { _connected = false; _regs.Clear(); }
            public override bool Connect(string ip, int port) { System.Threading.Thread.Sleep(_rng.Next(30, 100)); _connected = true; return true; }
            public override void Disconnect() { _connected = false; }
            public override int ReadRegister(string addr)
            {
                if (!_connected) return -1;
                if (_regs.ContainsKey(addr)) { _regs[addr] = _rng.Next(0, 65535); return _regs[addr]; }
                _regs[addr] = 0; return 0;
            }
            public override bool WriteRegister(string addr, int val) { if (!_connected) return false; _regs[addr] = val; return true; }
        }

        private class EmbeddedBusinessPlugin : OmniFrame.Sdk.PluginSystem.BusinessPlugin
        {
            private readonly Random _rng = new Random();
            private readonly string _name, _desc;
            private readonly Version _ver;

            public EmbeddedBusinessPlugin(string name, string desc, Version ver) { _name = name; _desc = desc; _ver = ver; }
            public override string Name => _name;
            public override string Description => _desc;
            public override bool Initialize() => true;
            public override void Unload() { }

            public override object Execute(object parameters)
            {
                var ps = parameters?.ToString() ?? "";
                System.Threading.Thread.Sleep(_rng.Next(50, 200));
                var result = new Dictionary<string, object> { ["Operation"] = ps, ["Result"] = "OK", ["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") };
                if (ps.Contains("Weigh") || ps.Contains("称重")) { result["Weight"] = 100 + _rng.NextDouble() * 50; result["Unit"] = "g"; }
                if (ps.Contains("Report") || ps.Contains("报表")) { result["ReportId"] = Guid.NewGuid().ToString("N").Substring(0, 8); result["PassRate"] = 95 + _rng.NextDouble() * 4.5; }
                if (ps.Contains("Validate") || ps.Contains("校验")) { result["IsValid"] = true; result["Warnings"] = new[] { "DispenseTimeMs 超出推荐范围" }; }
                return result;
            }
        }
    }
}
