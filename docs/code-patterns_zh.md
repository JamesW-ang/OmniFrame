# 代码模式与编码规范

本文档定义了 OmniFrame 全项目中使用的编码模式。编写新代码时请遵循以下规范。这些规范的存在是为了让任何开发者阅读任何文件时，都能立即理解其结构。

---

## 依赖注入注册

所有注册均在 `src/OmniFrame/DiConfigurator.cs` 的 `ConfigureServices` 方法中完成（由 `Program.cs` 在启动时调用）。使用 **Microsoft.Extensions.DependencyInjection**。

| 生命周期 | 使用场景 | 示例 |
|---|---|---|
| **Singleton** | 整个应用程序生命周期内仅一个实例 | `SystemManager`、`AlarmManager`、`StationBase` 子类、`ConfigurationManager` |
| **Transient** | 每次请求时创建新实例 | WinForms（`Form` 子类）、`UserControl` 子类 |
| **Scoped** | 在 WinForms 中极少使用。仅保留给 Web 上下文插件。 | （不常见） |

```csharp
// 正确 — 管理器应为 Singleton
services.AddSingleton<SystemManager>();
services.AddSingleton<IAlarmManager, AlarmManager>();

// 正确 — 窗体应为 Transient（WinForms 会反复创建它们）
services.AddTransient<MainForm>();
services.AddTransient<LoginForm>();

// 正确 — 存在接口时按接口注册
services.AddSingleton<IMotionController, RealMotionController>();

// 错误 — 不要将 Form 或 UserControl 注册为 Singleton
services.AddSingleton<SomeForm>(); // 第二次打开时会导致状态污染
```

---

## 管理器生命周期

每个主要管理器都遵循以下三阶段生命周期，由 `SystemManager` 在启动时调用：

```csharp
public interface IManager
{
    void Initialize();  // 加载配置、打开连接、验证前置条件
    void Start();       // 开始主动工作（启动定时器、开始监听等）
    void Stop();        // 优雅关闭（刷新缓冲区、关闭连接）
}
```

| 阶段 | 执行内容 | 失败处理 |
|---|---|---|
| `Initialize()` | 读取配置，打开数据库/文件/网络连接，创建内部状态。 | 如果缺少前置条件，抛出具体异常。应用程序将显示错误对话框并拒绝启动。 |
| `Start()` | 启动后台线程、定时器，开始轮询。 | 记录错误并尝试以降级功能继续运行。 |
| `Stop()` | 取消定时器，刷新缓冲区，关闭连接。在应用退出时调用。 | 记录并继续。绝不要在 Stop 中抛出异常——应用正在关闭过程中。 |

---

## 错误处理模式

每个 `catch` 块必须遵循以下结构：

```csharp
try
{
    // 操作
}
catch (TimeoutException ex)
{
    // 处理已知的具体异常，并给出针对性响应。
    Logger.Error("运动控制器在规定时间内未响应。", ex);
    SetAlarm("MotionTimeout", timeoutMs);
}
catch (IOException ex)
{
    // 另一种具体异常类型。
    Logger.Error("IO 板卡通信失败。", ex);
    SetAlarm("IOCommFailure", 0);
}
catch (Exception ex)
{
    // 通用兜底捕获。始终记录完整的异常对象，绝不要只传 ex.Message。
    Logger.Error("工站循环中发生意外错误。", ex);
    SetAlarm("UnexpectedError", 0);
}
```

**规则：**

1. **先捕获具体异常**，通用 `Exception` 放在最后。
2. **始终将完整的异常对象**传递给日志记录器——绝不要只传 `ex.Message`。Serilog 从异常对象中捕获堆栈跟踪和内部异常。
3. **为任何操作员应看到的错误设置报警**。使用简短的、描述性的报警码。
4. **绝不要静默吞掉异常。**即使你恢复了，也要记录一条 `Warning`。

---

## 信号命名规范

所有信号使用 **`DeviceName_Action`** 格式：

| 信号 | 含义 |
|---|---|
| `Robot1_AtPick` | 机器人 1 已到达取料位置。 |
| `Clamp1_Closed` | 夹爪 1 气缸处于闭合状态。 |
| `PlcConv1_Running` | PLC 上报传送带 1 电机正在运行。 |
| `Feeder_HasPart` | 送料传感器检测到工件。 |
| `Camera1_InspectionPass` | 视觉相机上报检测通过。 |

**规则：**

- `DeviceName` 使用电气原理图或 PLC 标签表中的确切设备名称。
- `Action` 为动词或状态，使用 PascalCase 命名。
- 使用下划线分隔设备名和动作名。
- 不要缩写名称 — 用 `Conveyor1`，而不是 `Cnv1`。

信号名称在 `SignalNames` 或 `SignalConstants` 类中定义为常量。绝不要在工站逻辑中将信号名称硬编码为字符串字面量。

```csharp
// 正确
WaitSignal(SignalNames.FeederHasPart, true, TimeoutConstants.Medium);

// 错误
WaitSignal("Feeder_HasPart", true, 5000);
```

---

## 超时常量

绝不要将数值超时写为魔法数字。请使用 `TimeoutConstants`：

```csharp
public static class TimeoutConstants
{
    public const int Short   = 1000;   // 1 秒 — 快速传感器、气动气缸
    public const int Medium  = 5000;   // 5 秒 — 机器人移动、PLC 握手
    public const int Long    = 30000;  // 30 秒 — 回零序列、预热
    public const int Extreme = 120000; // 2 分钟 — 烘箱温度爬升等
}
```

如果某个工站需要一个非常特定的超时时间（例如某个气动阀需要 3500ms），则在工站类自身中添加一个命名常量。不要将 `3500` 内联写在代码中。

---

## 日志记录

OmniFrame 使用 **Serilog**，由 `Common` 中的静态 `Logger` 类进行封装：

```csharp
// 信息 — 正常运行事件
Logger.Info("工站 {StationName} 已启动循环。", StationName);

// 警告 — 意外但可恢复的情况
Logger.Warning("信号 {SignalName} 重试 {RetryCount}/{MaxRetries}。", retry, max, signal);

// 错误 — 发生了故障。始终传入 Exception 对象。
Logger.Error(ex, "写入 PLC 寄存器 {Register}（地址 {Address}）失败。", register, address);
```

**规则：**

1. **始终将完整的 `Exception` 对象**传递给 `Logger.Error`，而不是 `ex.Message`。
2. **使用结构化日志** — 在消息模板中使用 `{Name}` 占位符放置变量。Serilog 会将其捕获为属性。
3. **不要在紧密循环中记录日志。**`WaitSignal` 重试循环中最多每秒记录一次。
4. 记录主要操作的进入和退出（工站启动、循环完成、报警设置/清除）。

---

## 命名规范

| 元素 | 规范 | 示例 |
|---|---|---|
| **接口** | 以 `I` 为前缀，独立文件存放 | `IMotionController`、`IAlarmManager` (见 `src/OmniFrame.Core/I*.cs`) |
| **管理器类** | 以 `Manager` 为后缀 | `SystemManager`、`DatabaseManager` |
| **工站类** | 描述性名称，以 `Station` 结尾 | `OHSLoadingStation`、`InspectionStation` |
| **常量类** | 以 `Constants` 为后缀 | `TimeoutConstants`、`SignalConstants` |
| **窗体** | 以 `Form` 为后缀 | `MainForm`、`LoginForm` |
| **用户控件** | 以 `Control` 为后缀 | `StationControl`、`AlarmPanelControl` |
| **枚举成员** | PascalCase | `WaitForPartAtPick` |
| **私有字段** | camelCase，无下划线前缀 | `currentStep`、`logger` |
| **方法参数** | camelCase | `signalName`、`timeoutMs` |
| **局部变量** | camelCase | `result`、`elapsed` |
