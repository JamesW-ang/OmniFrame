# API 参考文档

## 核心命名空间

```csharp
using AOIFrame.Core;              // 核心管理器
using AOIFrame.Communication;     // 通信层
using AOIFrame.Common;            // 公共工具类
using AOIFrame.Hardware;          // 硬件抽象
using AOIFrame.DataAccess;        // 数据访问层
```

---

## MotionMgr - 运动控制管理器

### 属性

```csharp
// 运动轴数量
public int AxisCount { get; }

// 是否已初始化
public bool IsInitialized { get; }
```

### 主要方法

#### 初始化与清理

```csharp
/// <summary>初始化运动管理器</summary>
public void Initialize();

/// <summary>释放资源</summary>
public void Dispose();

/// <summary>软件复位</summary>
public void SoftReset(int axis);

/// <summary>硬件复位</summary>
public void HardReset(int axis);
```

#### 位置读写

```csharp
/// <summary>获取轴的当前位置</summary>
/// <param name="axis">轴号（0开始）</param>
/// <returns>位置值（mm）</returns>
public double GetAxisPosition(int axis);

/// <summary>设置轴的当前位置（不移动）</summary>
/// <param name="axis">轴号</param>
/// <param name="position">新位置值</param>
public void SetAxisPosition(int axis, double position);

/// <summary>清除轴的误差值</summary>
public void ClearAxisError(int axis);
```

#### 运动参数设置

```csharp
/// <summary>设置轴的速度</summary>
/// <param name="axis">轴号</param>
/// <param name="velocity">速度值（mm/s）</param>
public void SetAxisVelocity(int axis, double velocity);

/// <summary>设置轴的加速度</summary>
/// <param name="axis">轴号</param>
/// <param name="accel">加速度值（mm/s²）</param>
public void SetAxisAccel(int axis, double accel);

/// <summary>设置轴的减速度</summary>
/// <param name="axis">轴号</param>
/// <param name="decel">减速度值（mm/s²）</param>
public void SetAxisDecel(int axis, double decel);

/// <summary>获取轴的速度</summary>
public double GetAxisVelocity(int axis);

/// <summary>获取轴的实时速度</summary>
public double GetAxisRealVelocity(int axis);
```

#### 运动控制

```csharp
/// <summary>绝对位置运动</summary>
/// <param name="axis">轴号</param>
/// <param name="targetPos">目标位置（mm）</param>
public void MoveAbsolute(int axis, double targetPos);

/// <summary>相对位置运动</summary>
/// <param name="axis">轴号</param>
/// <param name="distance">相对距离（mm）</param>
public void MoveRelative(int axis, double distance);

/// <summary>连续速度运动</summary>
/// <param name="axis">轴号</param>
/// <param name="direction">运动方向（1=正方向，-1=负方向）</param>
public void ContinuousMove(int axis, int direction);

/// <summary>立即停止运动</summary>
public void ImmediateStop(int axis);

/// <summary>急停（所有轴停止）</summary>
public void EmergencyStop();

/// <summary>暂停运动</summary>
public void Pause(int axis);

/// <summary>继续运动</summary>
public void Resume(int axis);
```

#### 运动状态查询

```csharp
/// <summary>是否在运动中</summary>
public bool IsMoving(int axis);

/// <summary>是否到达目标位置</summary>
public bool IsInPosition(int axis);

/// <summary>是否故障</summary>
public bool HasError(int axis);

/// <summary>获取故障代码</summary>
public int GetErrorCode(int axis);

/// <summary>获取故障描述</summary>
public string GetErrorMessage(int axis);

/// <summary>等待运动完成</summary>
/// <param name="axis">轴号</param>
/// <param name="timeout">超时时间（ms）</param>
/// <returns>是否在超时前完成</returns>
public bool WaitMotionDone(int axis, int timeout);
```

#### 多轴同步

```csharp
/// <summary>同步多轴运动到同一位置</summary>
/// <param name="axes">轴号数组</param>
/// <param name="targetPos">目标位置</param>
public void SyncMove(int[] axes, double targetPos);

/// <summary>等待多轴同时完成</summary>
public bool WaitMultiAxisDone(int[] axes, int timeout);
```

### 事件

```csharp
/// <summary>轴位置变化事件</summary>
public event Action<int, double> PositionChanged;

/// <summary>轴状态变化事件</summary>
public event Action<int, AxisState> StateChanged;

/// <summary>轴出错事件</summary>
public event Action<int, int> ErrorOccurred;
```

---

## IoMgr - IO管理器

### 主要方法

#### 数字IO（DI/DO）

```csharp
/// <summary>读取数字输入</summary>
/// <param name="index">输入号（0开始）</param>
/// <returns>输入状态</returns>
public bool ReadDI(int index);

/// <summary>写入数字输出</summary>
/// <param name="index">输出号</param>
/// <param name="value">输出值</param>
public void WriteDO(int index, bool value);

/// <summary>读取多个DI</summary>
/// <param name="startIndex">起始号</param>
/// <param name="count">个数</param>
public bool[] ReadDIArray(int startIndex, int count);

/// <summary>写入多个DO</summary>
public void WriteDOArray(int startIndex, bool[] values);

/// <summary>获取DI计数</summary>
public int GetDICount();

/// <summary>获取DO计数</summary>
public int GetDOCount();
```

#### 模拟IO（AI/AO）

```csharp
/// <summary>读取模拟输入</summary>
/// <param name="index">通道号</param>
/// <returns>模拟值（0-10V对应0-1024）</returns>
public double ReadAI(int index);

/// <summary>写入模拟输出</summary>
/// <param name="index">通道号</param>
/// <param name="value">输出值（0-10V对应0-1024）</param>
public void WriteAO(int index, double value);

/// <summary>获取AI计数</summary>
public int GetAICount();

/// <summary>获取AO计数</summary>
public int GetAOCount();
```

### 事件

```csharp
/// <summary>DI变化事件</summary>
public event Action<int, bool> OnDIChanged;

/// <summary>DO变化事件</summary>
public event Action<int, bool> OnDOChanged;

/// <summary>AI变化事件</summary>
public event Action<int, double> OnAIChanged;

/// <summary>AO变化事件</summary>
public event Action<int, double> OnAOChanged;
```

---

## ConfigManager - 配置管理器

### 主要方法

#### 初始化与保存

```csharp
/// <summary>初始化配置管理器</summary>
/// <param name="configFilePath">配置文件路径</param>
public void Initialize(string configFilePath);

/// <summary>重新加载配置</summary>
public void Reload();

/// <summary>保存配置到文件</summary>
public void SaveConfig();

/// <summary>备份配置文件</summary>
/// <param name="backupPath">备份路径</param>
public void BackupConfig(string backupPath);
```

#### 配置读写

```csharp
/// <summary>获取配置值（字符串）</summary>
/// <param name="section">分区名</param>
/// <param name="key">键名</param>
/// <returns>配置值</returns>
public string GetConfig(string section, string key);

/// <summary>获取配置值（泛型）</summary>
public T GetConfig<T>(string section, string key);

/// <summary>设置配置值</summary>
/// <param name="section">分区名</param>
/// <param name="key">键名</param>
/// <param name="value">值</param>
public void SetConfig(string section, string key, object value);

/// <summary>删除配置项</summary>
public void RemoveConfig(string section, string key);

/// <summary>检查配置是否存在</summary>
public bool ConfigExists(string section, string key);
```

#### 批量操作

```csharp
/// <summary>获取一个分区下的所有配置</summary>
public Dictionary<string, string> GetSection(string section);

/// <summary>设置整个分区</summary>
public void SetSection(string section, Dictionary<string, string> values);

/// <summary>获取所有分区名称</summary>
public string[] GetAllSections();
```

---

## PlcLink - PLC通信链接

### 主要方法

#### 连接管理

```csharp
/// <summary>连接到PLC（TCP）</summary>
/// <param name="ipAddress">IP地址</param>
/// <param name="port">端口号</param>
public void Connect(string ipAddress, int port);

/// <summary>连接到PLC（串口）</summary>
/// <param name="portName">串口名（如COM1）</param>
/// <param name="baudrate">波特率</param>
public void Connect(string portName, int baudrate);

/// <summary>断开连接</summary>
public void Disconnect();

/// <summary>检查连接状态</summary>
public bool IsConnected { get; }

/// <summary>重新连接</summary>
public void Reconnect();
```

#### 数据读写

```csharp
/// <summary>读取线圈（离散输出）</summary>
/// <param name="address">地址</param>
/// <returns>状态值</returns>
public bool ReadCoil(string address);

/// <summary>读取输入（离散输入）</summary>
public bool ReadInput(string address);

/// <summary>读取保持寄存器</summary>
/// <param name="address">寄存器地址</param>
/// <returns>寄存器值</returns>
public ushort ReadRegister(string address);

/// <summary>读取输入寄存器</summary>
public ushort ReadInputRegister(string address);

/// <summary>写入线圈</summary>
public void WriteCoil(string address, bool value);

/// <summary>写入寄存器</summary>
public void WriteRegister(string address, ushort value);

/// <summary>读取多个寄存器</summary>
public ushort[] ReadRegisters(string address, ushort count);

/// <summary>写入多个寄存器</summary>
public void WriteRegisters(string address, ushort[] values);
```

---

## PluginManager - 插件管理器

### 主要方法

#### 插件管理

```csharp
/// <summary>加载插件</summary>
/// <param name="pluginPath">插件DLL路径</param>
public void LoadPlugin(string pluginPath);

/// <summary>卸载插件</summary>
/// <param name="pluginName">插件名称</param>
public void UnloadPlugin(string pluginName);

/// <summary>获取所有已加载插件</summary>
public IEnumerable<IPlugin> GetLoadedPlugins();

/// <summary>获取指定插件</summary>
public IPlugin GetPlugin(string pluginName);
```

#### 插件执行

```csharp
/// <summary>执行插件方法</summary>
/// <param name="pluginName">插件名称</param>
/// <param name="methodName">方法名</param>
/// <param name="parameters">参数字典</param>
public object ExecutePluginFunction(
    string pluginName, 
    string methodName, 
    Dictionary<string, object> parameters);
```

---

## DigitalTwinBridge - 数字孪生桥接

### 主要方法

```csharp
/// <summary>创建数字孪生桥接</summary>
/// <param name="webSocketUrl">WebSocket服务器地址</param>
public DigitalTwinBridge(string webSocketUrl);

/// <summary>连接到WebSocket服务器</summary>
public async Task ConnectAsync();

/// <summary>断开连接</summary>
public async Task DisconnectAsync();

/// <summary>检查连接状态</summary>
public bool IsConnected { get; }

/// <summary>推送轴位置数据</summary>
public void PushAxisPosition(int axis, double position);

/// <summary>推送IO状态</summary>
public void PushIOStatus(string ioName, bool state);

/// <summary>推送机器状态</summary>
public void PushMachineStatus(string statusKey, object value);

/// <summary>发送自定义消息</summary>
public async Task SendMessageAsync(string messageType, object data);
```

### 事件

```csharp
/// <summary>连接成功事件</summary>
public event Action OnConnected;

/// <summary>连接断开事件</summary>
public event Action OnDisconnected;

/// <summary>接收消息事件</summary>
public event Action<string, object> OnMessageReceived;

/// <summary>错误事件</summary>
public event Action<string> OnError;
```

---

## 错误处理

所有主要API都可能抛出异常，建议使用 try-catch：

```csharp
try
{
    MotionMgr.Instance.MoveAbsolute(0, 100.0);
}
catch (ArgumentException ex)
{
    // 参数错误
    Logger.Error($"参数错误: {ex.Message}");
}
catch (TimeoutException ex)
{
    // 操作超时
    Logger.Error($"操作超时: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // 设备未初始化或不可用
    Logger.Error($"操作无效: {ex.Message}");
}
catch (Exception ex)
{
    // 其他异常
    Logger.Error($"未知错误: {ex.Message}");
}
```

---

## 更多帮助

- 📖 [完整文档](../README.md)
- 🏗️ [架构设计](architecture.md)
- 💡 [快速开始](getting-started.md)
- 🐛 [Issue Tracker](https://github.com/yourusername/AOIFrame-Lite/issues)

