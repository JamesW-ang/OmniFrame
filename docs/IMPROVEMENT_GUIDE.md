# OmniFrame 代码改进实施指南

**版本：** 1.0  
**最后更新：** 2026年5月29日  

本文档为代码审阅报告的补充，提供具体的改进实施步骤和代码示例。

---

## 目录

1. [第 1 阶段：紧急安全性修复](#第-1-阶段紧急安全性修复)
2. [第 2 阶段：架构优化](#第-2-阶段架构优化)
3. [第 3 阶段：代码质量](#第-3-阶段代码质量)
4. [检查清单](#检查清单)

---

## 第 1 阶段：紧急安全性修复

### 任务 1.1：消除异常吞噬

**目标：** 将 4 处空 catch 块改为正确的异常处理  
**工作量：** 1 天  
**优先级：** 🔴 P0

#### 1.1.1 PluginManager.cs - 插件卸载

**现场位置：** Line 690

```csharp
// ❌ 当前代码
try { plugin.Unload(); } catch { }

// ✅ 改进后
try 
{ 
    Logger.Info($"卸载插件: {plugin.Name}");
    plugin.Unload(); 
}
catch (Exception ex) 
{ 
    Logger.Error($"插件卸载失败: {plugin.Name}", ex);
    
    // 是否需要阻止后续操作？
    if (ex is OutOfMemoryException)
        throw; // 致命异常需要重抛
}
```

#### 1.1.2 ReconnectionService.cs - 设备重连

**现场位置：** Line 77

```csharp
// ❌ 当前代码
try { _device.Reconnect(); } catch { }

// ✅ 改进后
try 
{ 
    Logger.Info($"尝试重连设备: {_device.Name}");
    _device.Reconnect(); 
    Logger.Info($"设备重连成功: {_device.Name}");
}
catch (TimeoutException ex)
{
    Logger.Warning($"设备连接超时: {_device.Name} ({ex.Message})");
    SetDeviceOfflineAlarm(_device.Name);
}
catch (IOException ex)
{
    Logger.Error($"设备通信失败: {_device.Name}", ex);
    SetDeviceOfflineAlarm(_device.Name);
}
catch (Exception ex)
{
    Logger.Error($"意外异常: {_device.Name}", ex);
    throw;
}
```

#### 1.1.3 其他位置的异常吞噬修复

**查找命令：**
```bash
# 在所有 .cs 文件中搜索空 catch 块
grep -r "catch\s*{" src/ | grep -v "catch.*Exception"
```

**修复模板：**
```csharp
try 
{ 
    // 操作 
}
catch (SpecificException ex)
{
    // 1. 记录日志（必须）
    Logger.Warning($"[操作名] 失败: {ex.Message}");
    
    // 2. 设置告警（如果是用户需要知道的）
    SetAlarm("OperationFailed", 0);
    
    // 3. 是否降级或重抛？
    // return false;  // 降级：返回失败状态
    // throw;         // 重抛：调用者处理
}
catch (Exception ex)
{
    Logger.Error($"[操作名] 异常", ex);
    throw;  // 未知异常必须重抛
}
```

---

### 任务 1.2：告警通知添加重试机制

**目标：** 将同步 Fire-and-Forget 改为带重试的异步通知  
**工作量：** 2 天  
**优先级：** 🔴 P0

**涉及文件：** AlarmManager.cs, AlarmNotification.cs

#### 步骤 1：安装 Polly NuGet 包

```bash
cd /path/to/OmniFrame
nuget install Polly -Version 8.2.0
# 或在 .csproj 中添加
# <PackageReference Include="Polly" Version="8.2.0" />
```

#### 步骤 2：创建 AlarmNotificationWithRetry 类

```csharp
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/// <summary>
/// 带重试机制的告警通知服务
/// </summary>
public class AlarmNotificationWithRetry : IAlarmNotificationService
{
    private readonly IAlarmNotification _innerNotification;
    private readonly IAsyncPolicy<bool> _retryPolicy;
    private readonly IAsyncPolicy<bool> _circuitBreakerPolicy;
    private readonly ConcurrentQueue<AlarmInfo> _deadLetterQueue;
    private readonly string _dlqPath = "Logs/DeadLetterQueue";
    
    public AlarmNotificationWithRetry(IAlarmNotification innerNotification)
    {
        _innerNotification = innerNotification;
        _deadLetterQueue = new ConcurrentQueue<AlarmInfo>();
        
        // 定义重试策略：指数退避
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult<bool>(r => !r)  // 返回 false 也认为是失败
            .WaitAndRetryAsync<bool>(
                retryCount: 3,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 500),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Logger.Warning($"告警通知重试 [{retryCount}/3], " +
                        $"延迟 {timespan.TotalSeconds}s");
                });
        
        // 定义熔断器：连续 5 次失败后打开熔断
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult<bool>(r => !r)
            .CircuitBreakerAsync<bool>(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, timespan) =>
                {
                    Logger.Error($"告警通知熔断器打开，将在 {timespan.TotalSeconds}s 后尝试恢复");
                },
                onReset: () =>
                {
                    Logger.Info("告警通知熔断器重置");
                });
    }
    
    public async Task<bool> SendNotificationAsync(AlarmInfo alarm)
    {
        try
        {
            // 使用 Wrap 组合重试和熔断策略
            var wrappedPolicy = Policy.WrapAsync<bool>(
                _retryPolicy,
                _circuitBreakerPolicy);
            
            // 设置 10 秒超时
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                bool result = await wrappedPolicy.ExecuteAsync(
                    ct => _innerNotification.SendNotificationAsync(alarm, ct),
                    cts.Token);
                
                if (!result)
                {
                    Logger.Warning($"告警通知发送返回 false");
                    await AddToDeadLetterQueueAsync(alarm);
                }
                
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Error($"告警通知发送超时，加入死信队列");
            await AddToDeadLetterQueueAsync(alarm);
            return false;
        }
        catch (BrokenCircuitException)
        {
            Logger.Error($"告警通知熔断器打开，加入死信队列");
            await AddToDeadLetterQueueAsync(alarm);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"告警通知异常", ex);
            await AddToDeadLetterQueueAsync(alarm);
            return false;
        }
    }
    
    private async Task AddToDeadLetterQueueAsync(AlarmInfo alarm)
    {
        _deadLetterQueue.Enqueue(alarm);
        
        // 异步持久化到磁盘
        try
        {
            if (!Directory.Exists(_dlqPath))
                Directory.CreateDirectory(_dlqPath);
            
            string fileName = $"alarm_{alarm.AlarmId}_{DateTime.Now:yyyyMMddHHmmss}.json";
            string filePath = Path.Combine(_dlqPath, fileName);
            
            string json = JsonConvert.SerializeObject(alarm, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
            
            Logger.Warning($"告警已添加到死信队列: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.Error($"保存死信队列失败", ex);
        }
    }
    
    /// <summary>
    /// 定期重试死信队列中的消息
    /// </summary>
    public async Task RetryDeadLetterQueueAsync()
    {
        int retried = 0;
        
        while (_deadLetterQueue.TryDequeue(out var alarm))
        {
            try
            {
                bool success = await SendNotificationAsync(alarm);
                if (success)
                {
                    Logger.Info($"死信队列重试成功: {alarm.AlarmId}");
                    retried++;
                }
                else
                {
                    // 重新加入队列
                    _deadLetterQueue.Enqueue(alarm);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"死信队列重试失败: {ex.Message}");
                _deadLetterQueue.Enqueue(alarm);
            }
        }
        
        if (retried > 0)
            Logger.Info($"死信队列重试完成，成功 {retried} 条");
    }
}
```

#### 步骤 3：在 DI 容器中注册

**Program.cs 或 ConfigureServices：**

```csharp
// 原始注册
// services.AddSingleton<IAlarmNotification, AlarmNotification>();

// 改进后
services.AddSingleton<IAlarmNotification>(sp =>
{
    var baseNotification = new AlarmNotification(
        sp.GetRequiredService<ILogger>());
    
    return new AlarmNotificationWithRetry(baseNotification);
});

// 定期重试死信队列
services.AddSingleton<IHostedService>(sp =>
{
    var notificationService = (AlarmNotificationWithRetry)sp
        .GetRequiredService<IAlarmNotification>();
    
    return new DeadLetterQueueRetryService(notificationService);
});
```

#### 步骤 4：创建定期重试服务

```csharp
public class DeadLetterQueueRetryService : BackgroundService
{
    private readonly AlarmNotificationWithRetry _notificationService;
    private readonly TimeSpan _retryInterval = TimeSpan.FromHours(1);
    
    public DeadLetterQueueRetryService(
        AlarmNotificationWithRetry notificationService)
    {
        _notificationService = notificationService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _notificationService.RetryDeadLetterQueueAsync();
                await Task.Delay(_retryInterval, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.Error("死信队列重试服务异常", ex);
            }
        }
    }
}
```

---

### 任务 1.3：配置变更异步防抖

**目标：** 将阻塞的 `Task.Delay().Wait()` 改为异步防抖  
**工作量：** 1 天  
**优先级：** 🔴 P0

**涉及文件：** ConfigManager.cs（Line 197）

```csharp
public class ConfigManager : IConfigManager
{
    private readonly object _fileWatcherLock = new object();
    private readonly SemaphoreSlim _configProcessSemaphore = 
        new SemaphoreSlim(1, 1);
    
    private Dictionary<string, DateTime> _lastFileChangeTime = 
        new Dictionary<string, DateTime>();
    
    private System.Timers.Timer _debounceTimer;
    private string _pendingFileName;
    
    private const int DebounceMs = 500;
    private const int FileWatchDebounceTimeoutMs = 1000;
    
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        
        lock (_fileWatcherLock)
        {
            // 检查防抖时间
            if (_lastFileChangeTime.TryGetValue(fileName, out var lastChange))
            {
                if ((DateTime.Now - lastChange).TotalMilliseconds < FileWatchDebounceTimeoutMs)
                {
                    Logger.Debug($"配置文件变更防抖: {fileName}");
                    return;
                }
            }
            
            _lastFileChangeTime[fileName] = DateTime.Now;
            _pendingFileName = fileName;
        }
        
        // 使用异步防抖，不阻塞主线程
        _debounceTimer?.Stop();
        _debounceTimer = new System.Timers.Timer(DebounceMs);
        _debounceTimer.Elapsed += async (s, args) => 
            await ProcessConfigChangeAsync(_pendingFileName);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }
    
    private async Task ProcessConfigChangeAsync(string fileName)
    {
        // 使用信号量防止并发处理同一个文件
        if (!await _configProcessSemaphore.WaitAsync(TimeSpan.FromSeconds(5)))
        {
            Logger.Debug($"配置处理已在进行中，跳过: {fileName}");
            return;
        }
        
        try
        {
            Logger.Info($"处理配置文件变更: {fileName}");
            OnConfigChanged(fileName);
            Logger.Info($"配置文件已重新加载: {fileName}");
        }
        catch (XmlException ex)
        {
            Logger.Error($"配置文件格式错误: {fileName}", ex);
            BackupCorruptedConfig(fileName);
        }
        catch (Exception ex)
        {
            Logger.Error($"配置变更处理失败: {fileName}", ex);
        }
        finally
        {
            _configProcessSemaphore.Release();
        }
    }
    
    private void BackupCorruptedConfig(string fileName)
    {
        try
        {
            string filePath = Path.Combine(_configPath, fileName);
            string backupName = $"{Path.GetFileNameWithoutExtension(fileName)}" +
                $"_corrupted_{DateTime.Now:yyyyMMddHHmmss}" +
                $"{Path.GetExtension(fileName)}";
            
            string backupPath = Path.Combine(_backupPath, backupName);
            
            if (!Directory.Exists(_backupPath))
                Directory.CreateDirectory(_backupPath);
            
            File.Copy(filePath, backupPath, true);
            Logger.Warning($"已备份损坏的配置: {backupPath}");
        }
        catch (Exception ex)
        {
            Logger.Error("备份配置失败", ex);
        }
    }
}
```

**测试用例：**

```csharp
[TestClass]
public class ConfigManagerTests
{
    [TestMethod]
    public async Task TestAsyncDebounce_DebounceMultipleChanges()
    {
        // Arrange
        var configManager = new ConfigManager();
        int processCount = 0;
        configManager.OnConfigChanged += (sender, args) => processCount++;
        
        // Act - 快速触发 5 次文件变更
        for (int i = 0; i < 5; i++)
        {
            configManager.SimulateFileChange("test.config");
            await Task.Delay(100); // 100ms < 防抖 500ms
        }
        
        // 等待防抖完成
        await Task.Delay(1000);
        
        // Assert - 只处理了 1 次（防抖成功）
        Assert.AreEqual(1, processCount, "防抖失败：应该只处理一次");
    }
}
```

---

### 任务 1.4：插件卸载完整化

**目标：** 改进插件卸载流程，确保资源正确释放  
**工作量：** 1 天  
**优先级：** 🔴 P0

**涉及文件：** PluginManager.cs（Line 690）

```csharp
public class PluginUnloadManager
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// 安全卸载插件，包含完整的清理流程
    /// </summary>
    public void UnloadPluginSafely(PluginBase plugin)
    {
        string pluginName = plugin?.GetType().Name ?? "Unknown";
        
        try
        {
            _logger.Info($"[插件卸载开始] {pluginName}");
            
            // Step 1: 停止插件执行
            if (plugin != null)
            {
                try
                {
                    plugin.Stop();
                    _logger.Info($"[插件停止] {pluginName}");
                }
                catch (Exception ex)
                {
                    _logger.Warning(
                        $"[插件停止失败] {pluginName}: {ex.Message}");
                }
            }
            
            // Step 2: 清理托管资源
            if (plugin is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                    _logger.Info($"[资源清理] {pluginName}");
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        $"[资源清理失败] {pluginName}", ex);
                }
            }
            
            // Step 3: 强制垃圾回收
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();  // 二次回收
                
                _logger.Debug($"[垃圾回收完成] {pluginName}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"[垃圾回收异常] {pluginName}", ex);
            }
            
            // Step 4: 验证卸载（检查是否还有引用）
            WeakReference weakRef = new WeakReference(plugin);
            _logger.Info($"[插件卸载完成] {pluginName}, " +
                $"引用已清理: {!weakRef.IsAlive}");
        }
        catch (Exception ex)
        {
            _logger.Error(
                $"[插件卸载异常] {pluginName}", ex);
            
            // ✅ 不能吞噬异常
            throw new PluginUnloadException(
                $"插件卸载失败: {pluginName}", ex);
        }
    }
}

public class PluginUnloadException : Exception
{
    public PluginUnloadException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**使用示例：**

```csharp
public class PluginManager
{
    private readonly PluginUnloadManager _unloadManager;
    
    public void RemovePlugin(string pluginName)
    {
        if (!_plugins.TryGetValue(pluginName, out var plugin))
        {
            throw new InvalidOperationException($"插件不存在: {pluginName}");
        }
        
        try
        {
            // 使用完整的卸载流程
            _unloadManager.UnloadPluginSafely(plugin);
            
            // 移除引用
            _plugins.Remove(pluginName);
            
            Logger.Info($"插件已移除: {pluginName}");
        }
        catch (PluginUnloadException ex)
        {
            Logger.Error($"插件卸载失败: {pluginName}", ex);
            throw;
        }
    }
}
```

---

## 第 2 阶段：架构优化

### 任务 2.1：创建数据访问抽象层

**目标：** 支持 SQLite ↔ MySQL 数据库切换，消除代码重复  
**工作量：** 5 天  
**优先级：** 🟡 P1

#### 步骤 1：定义数据提供者接口

**新建文件：** `src/DataAccess/IDataProvider.cs`

```csharp
namespace OmniFrame.DataAccess
{
    /// <summary>
    /// 数据库提供者接口，支持多种数据库
    /// </summary>
    public interface IDataProvider : IDisposable
    {
        /// <summary>数据库提供者名称</summary>
        string ProviderName { get; }
        
        /// <summary>创建数据库连接</summary>
        IDbConnection CreateConnection();
        
        /// <summary>测试连接</summary>
        bool TestConnection();
    }
}
```

#### 步骤 2：实现具体提供者

**新建文件：** `src/DataAccess/Providers/SqliteDataProvider.cs`

```csharp
namespace OmniFrame.DataAccess.Providers
{
    public class SqliteDataProvider : IDataProvider
    {
        private readonly string _connectionString;
        
        public string ProviderName => "SQLite";
        
        public SqliteDataProvider(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
        }
        
        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }
        
        public bool TestConnection()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        cmd.ExecuteScalar();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SQLite 连接测试失败", ex);
                return false;
            }
        }
        
        public void Dispose()
        {
            // SQLite 无需特殊清理
        }
    }
}
```

**新建文件：** `src/DataAccess/Providers/MySqlDataProvider.cs`

```csharp
namespace OmniFrame.DataAccess.Providers
{
    public class MySqlDataProvider : IDataProvider
    {
        private readonly string _connectionString;
        
        public string ProviderName => "MySQL";
        
        public MySqlDataProvider(string server, int port, 
            string database, string userId, string password)
        {
            _connectionString = 
                $"Server={server};Port={port};Database={database};" +
                $"Uid={userId};Pwd={password};Connection Timeout=30;";
        }
        
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        
        public bool TestConnection()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        cmd.ExecuteScalar();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"MySQL 连接测试失败", ex);
                return false;
            }
        }
        
        public void Dispose()
        {
            // MySQL 无需特殊清理
        }
    }
}
```

#### 步骤 3：创建通用仓储基类

**新建文件：** `src/DataAccess/Repository/BaseRepository.cs`

```csharp
namespace OmniFrame.DataAccess.Repository
{
    /// <summary>
    /// 数据仓储基类，提供通用的 CRUD 操作
    /// </summary>
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly IDataProvider _dataProvider;
        protected readonly string _tableName;
        
        protected BaseRepository(IDataProvider dataProvider, string tableName)
        {
            _dataProvider = dataProvider ?? 
                throw new ArgumentNullException(nameof(dataProvider));
            _tableName = tableName ?? 
                throw new ArgumentNullException(nameof(tableName));
        }
        
        /// <summary>添加记录</summary>
        public virtual bool Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            try
            {
                using (var conn = _dataProvider.CreateConnection())
                {
                    conn.Open();
                    string sql = GenerateInsertSql();
                    int affected = conn.Execute(sql, entity);
                    
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"添加记录失败 [{_tableName}]", ex);
                return false;
            }
        }
        
        /// <summary>获取所有记录</summary>
        public virtual List<T> GetAll()
        {
            try
            {
                using (var conn = _dataProvider.CreateConnection())
                {
                    conn.Open();
                    string sql = $"SELECT * FROM {_tableName}";
                    
                    return conn.Query<T>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取记录失败 [{_tableName}]", ex);
                return new List<T>();
            }
        }
        
        /// <summary>生成 INSERT 语句</summary>
        protected abstract string GenerateInsertSql();
        
        /// <summary>生成 SELECT 语句</summary>
        protected abstract string GenerateSelectSql();
    }
}
```

#### 步骤 4：迁移现有的 AlarmDb 和 ProductDb

**修改文件：** `src/DataAccess/AlarmDb.cs`

```csharp
namespace OmniFrame.DataAccess
{
    /// <summary>告警数据库操作</summary>
    public class AlarmRepository : BaseRepository<AlarmRecord>, IAlarmDb
    {
        public AlarmRepository(IDataProvider dataProvider) 
            : base(dataProvider, "Alarms") { }
        
        public bool AddAlarm(AlarmRecord record)
        {
            return Add(record);
        }
        
        public List<AlarmRecord> GetAlarmHistory(int limit = 1000)
        {
            try
            {
                using (var conn = _dataProvider.CreateConnection())
                {
                    conn.Open();
                    string sql = $@"
                        SELECT * FROM {_tableName}
                        ORDER BY OccurTime DESC
                        LIMIT {limit}";
                    
                    return conn.Query<AlarmRecord>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("获取告警历史失败", ex);
                return new List<AlarmRecord>();
            }
        }
        
        public bool ClearAlarms()
        {
            try
            {
                using (var conn = _dataProvider.CreateConnection())
                {
                    conn.Open();
                    string sql = $"DELETE FROM {_tableName}";
                    
                    return conn.Execute(sql) >= 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("清空告警失败", ex);
                return false;
            }
        }
        
        protected override string GenerateInsertSql() => @"
            INSERT INTO Alarms 
                (AlarmId, AlarmCode, Message, Level, OccurTime, Source, IsCleared)
            VALUES 
                (@AlarmId, @AlarmCode, @Message, @Level, @OccurTime, @Source, @IsCleared)";
        
        protected override string GenerateSelectSql() => @"
            SELECT * FROM Alarms WHERE IsCleared = 0
            ORDER BY OccurTime DESC";
    }
}
```

#### 步骤 5：在 DI 容器中注册

**修改文件：** `Program.cs`

```csharp
private static void ConfigureServices(IServiceCollection services, 
    IConfiguration config)
{
    // 选择数据提供者
    string dbProvider = config["Database:Provider"]?.ToUpper() ?? "SQLITE";
    
    services.AddSingleton<IDataProvider>(sp =>
    {
        if (dbProvider == "MYSQL")
        {
            return new MySqlDataProvider(
                server: config["Database:MySQL:Server"],
                port: int.Parse(config["Database:MySQL:Port"] ?? "3306"),
                database: config["Database:MySQL:Database"],
                userId: config["Database:MySQL:UserId"],
                password: config["Database:MySQL:Password"]);
        }
        else
        {
            return new SqliteDataProvider(
                dbPath: config["Database:SQLite:Path"] ?? "Data/OmniFrame.db");
        }
    });
    
    // 注册仓储
    services.AddSingleton<IAlarmDb>(sp => 
        new AlarmRepository(sp.GetRequiredService<IDataProvider>()));
    
    services.AddSingleton<IProductDb>(sp => 
        new ProductRepository(sp.GetRequiredService<IDataProvider>()));
}
```

#### 步骤 6：配置文件示例

**appsettings.json：**

```json
{
  "Database": {
    "Provider": "SQLite",
    "SQLite": {
      "Path": "Data/OmniFrame.db"
    },
    "MySQL": {
      "Server": "localhost",
      "Port": "3306",
      "Database": "omnif rame",
      "UserId": "root",
      "Password": "password"
    }
  }
}
```

---

### 任务 2.2：优化并发控制（ReaderWriterLockSlim）

**目标：** 将粗粒度 lock 改为 ReaderWriterLockSlim，提升并发性能  
**工作量：** 3 天  
**优先级：** 🟡 P1

**改进示例：** AlarmManager.cs

```csharp
public class ImprovedAlarmManager : IAlarmManager
{
    private readonly ReaderWriterLockSlim _alarmLock = 
        new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    
    private List<AlarmInfo> _activeAlarms = new List<AlarmInfo>();
    private List<AlarmInfo> _alarmHistory = new List<AlarmInfo>();
    
    /// <summary>获取活跃告警（读操作，允许并发）</summary>
    public List<AlarmInfo> GetActiveAlarms()
    {
        _alarmLock.EnterReadLock();
        try
        {
            return _activeAlarms
                .Where(a => !a.IsCleared)
                .OrderByDescending(a => a.OccurTime)
                .ToList();
        }
        finally
        {
            _alarmLock.ExitReadLock();
        }
    }
    
    /// <summary>添加告警（写操作，独占锁）</summary>
    public AlarmInfo AddAlarm(string code, string message, 
        AlarmLevel level, string source)
    {
        _alarmLock.EnterWriteLock();
        try
        {
            // 检查去重
            var existing = _activeAlarms.FirstOrDefault(
                a => a.AlarmCode == code && !a.IsCleared);
            
            if (existing != null)
                return existing;
            
            // 创建告警
            var alarm = new AlarmInfo
            {
                AlarmId = Guid.NewGuid().ToString(),
                AlarmCode = code,
                Message = message,
                Level = level,
                Source = source,
                OccurTime = DateTime.Now,
                IsCleared = false
            };
            
            _activeAlarms.Add(alarm);
            _alarmHistory.Add(alarm);
            
            return alarm;
        }
        finally
        {
            _alarmLock.ExitWriteLock();
        }
        
        // ✅ 在 lock 外触发事件，避免死锁
        AlarmOccurred?.Invoke(this, alarm);
        
        // 异步通知，不阻塞
        _ = SendNotificationAsync(alarm);
    }
    
    /// <summary>清除告警（写操作，升级锁）</summary>
    public bool ClearAlarm(string alarmId)
    {
        _alarmLock.EnterUpgradeableReadLock();
        try
        {
            var alarm = _activeAlarms.FirstOrDefault(a => a.AlarmId == alarmId);
            
            if (alarm == null || alarm.IsCleared)
                return false;
            
            // 升级为写锁
            _alarmLock.EnterWriteLock();
            try
            {
                alarm.IsCleared = true;
                alarm.ClearTime = DateTime.Now;
                
                return true;
            }
            finally
            {
                _alarmLock.ExitWriteLock();
            }
        }
        finally
        {
            _alarmLock.ExitUpgradeableReadLock();
        }
        
        AlarmCleared?.Invoke(this, alarm);
    }
    
    public void Dispose()
    {
        _alarmLock?.Dispose();
    }
}
```

**性能对比测试：**

```csharp
[TestClass]
public class ConcurrencyPerformanceTests
{
    [TestMethod]
    public void BenchmarkLock_vs_ReaderWriterLockSlim()
    {
        const int threadCount = 10;
        const int operationsPerThread = 1000;
        
        // 测试 1: 原始 lock
        var sw1 = Stopwatch.StartNew();
        TestWithLock(threadCount, operationsPerThread);
        sw1.Stop();
        
        // 测试 2: ReaderWriterLockSlim
        var sw2 = Stopwatch.StartNew();
        TestWithReaderWriterLock(threadCount, operationsPerThread);
        sw2.Stop();
        
        Console.WriteLine($"lock 耗时: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"ReaderWriterLockSlim 耗时: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"性能提升: {(sw1.ElapsedMilliseconds / (double)sw2.ElapsedMilliseconds):P}");
    }
    
    private void TestWithLock(int threadCount, int opsPerThread)
    {
        object lockObj = new object();
        List<int> data = new List<int>();
        
        var threads = Enumerable.Range(0, threadCount)
            .Select(_ => new Thread(() =>
            {
                for (int i = 0; i < opsPerThread; i++)
                {
                    lock (lockObj) { data.Add(i); }
                }
            }))
            .ToList();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
    }
    
    private void TestWithReaderWriterLock(int threadCount, int opsPerThread)
    {
        var rwLock = new ReaderWriterLockSlim();
        List<int> data = new List<int>();
        
        var threads = Enumerable.Range(0, threadCount)
            .Select(_ => new Thread(() =>
            {
                for (int i = 0; i < opsPerThread; i++)
                {
                    rwLock.EnterWriteLock();
                    try { data.Add(i); }
                    finally { rwLock.ExitWriteLock(); }
                }
            }))
            .ToList();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
    }
}
```

---

## 第 3 阶段：代码质量

### 任务 3.1：提取魔法数字为常量

**目标：** 消除 20+ 处魔法数字  
**工作量：** 2 天  
**优先级：** 🟡 P1

**新建文件：** `src/Common/Constants.cs`

```csharp
namespace OmniFrame.Common
{
    /// <summary>
    /// 全局常量定义
    /// </summary>
    public static class Constants
    {
        /// <summary>ID 和标识符相关</summary>
        public static class Identifiers
        {
            /// <summary>自动生成 ID 的长度</summary>
            public const int GeneratedIdLength = 8;
            
            /// <summary>ID 的 GUID 格式（N = 无连字符）</summary>
            public const string IdGuidFormat = "N";
        }
        
        /// <summary>超时设置</summary>
        public static class Timeouts
        {
            /// <summary>快速操作超时（传感器、气缸）</summary>
            public const int ShortMs = 1000;
            
            /// <summary>中等操作超时（机器人、PLC）</summary>
            public const int MediumMs = 5000;
            
            /// <summary>长操作超时（回零、升温）</summary>
            public const int LongMs = 30000;
            
            /// <summary>极长操作超时（烤箱升温等）</summary>
            public const int ExtremeMs = 120000;
            
            /// <summary>数据库连接超时</summary>
            public const int DatabaseConnectionSeconds = 30;
            
            /// <summary>网络请求超时</summary>
            public const int HttpRequestSeconds = 10;
        }
        
        /// <summary>配置文件相关</summary>
        public static class Configuration
        {
            /// <summary>文件系统监听器防抖延迟</summary>
            public const int FileWatcherDebounceMs = 500;
            
            /// <summary>配置处理超时</summary>
            public const int ConfigProcessTimeoutSeconds = 5;
            
            /// <summary>备份相关</summary>
            public const int DefaultBackupIntervalHours = 6;
            public const int MaxBackupFiles = 50;
            public const string BackupFolderName = "Backups";
        }
        
        /// <summary>插件系统</summary>
        public static class Plugins
        {
            /// <summary>插件目录</summary>
            public const string PluginsFolder = "Plugins";
            
            /// <summary>最大支持的插件版本</summary>
            public const int MaxPluginVersion = 10;
        }
        
        /// <summary>数据库相关</summary>
        public static class Database
        {
            /// <summary>默认 SQLite 数据库路径</summary>
            public const string DefaultSqlitePath = "Data/OmniFrame.db";
            
            /// <summary>批量操作大小</summary>
            public const int BatchSize = 1000;
        }
        
        /// <summary>告警系统</summary>
        public static class Alarms
        {
            /// <summary>告警去重时间窗口（分钟）</summary>
            public const int DeduplicationWindowMinutes = 5;
            
            /// <summary>告警保留天数</summary>
            public const int RetentionDays = 30;
        }
    }
}
```

**使用示例：**

```csharp
// 之前
int delay = 100;  // 为什么是 100？
Task.Delay(delay).Wait();

// 之后
await Task.Delay(Constants.Configuration.FileWatcherDebounceMs);

// 之前
var timeout = 5000;
await _client.ConnectAsync(host, port, timeout);

// 之后
using (var cts = new CancellationTokenSource(
    TimeSpan.FromSeconds(Constants.Timeouts.HttpRequestSeconds)))
{
    await _client.ConnectAsync(host, port, cts.Token);
}
```

---

## 检查清单

### P0 检查清单（第 1 周）

- [ ] **异常吞噬修复**
  - [ ] PluginManager.cs Line 690
  - [ ] ReconnectionService.cs Line 77
  - [ ] 其他 2 处空 catch 块
  - [ ] 编写单元测试

- [ ] **告警通知重试**
  - [ ] 安装 Polly 包
  - [ ] 创建 AlarmNotificationWithRetry 类
  - [ ] 实现死信队列
  - [ ] 编写集成测试

- [ ] **配置防抖改进**
  - [ ] 改为异步防抖
  - [ ] 添加信号量保护
  - [ ] 测试多文件并发变更

- [ ] **插件卸载完整化**
  - [ ] 创建 PluginUnloadManager
  - [ ] 改进资源清理
  - [ ] 添加卸载验证

### P1 检查清单（第 2-4 周）

- [ ] **数据访问层抽象**
  - [ ] 创建 IDataProvider 接口
  - [ ] 实现 SqliteDataProvider
  - [ ] 实现 MySqlDataProvider
  - [ ] 创建 BaseRepository
  - [ ] 迁移 AlarmDb / ProductDb
  - [ ] 配置文件支持切换

- [ ] **并发优化**
  - [ ] AlarmManager 改用 ReaderWriterLockSlim
  - [ ] DeviceManager 优化
  - [ ] 添加超时保护
  - [ ] 性能基准测试

- [ ] **代码质量**
  - [ ] 创建 Constants.cs
  - [ ] 提取魔法数字
  - [ ] 消除代码重复
  - [ ] 降低圆复杂度

### P2 检查清单（持续）

- [ ] 单元测试覆盖（目标 80%）
- [ ] 集成测试覆盖
- [ ] 代码覆盖率报告（Sonar）
- [ ] 性能基准测试
- [ ] 文档更新

---

**更新历史：**

| 版本 | 日期 | 更新内容 |
|------|------|--------|
| 1.0 | 2026-05-29 | 初版发布 |

