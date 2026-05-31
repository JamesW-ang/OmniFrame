# 🏗️ 架构指南：理解 OmniFrame 的设计

> **目标读者：** 已完成 [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) Day 1-3 的新人。
> 本文档解释「为什么这么设计」和「新功能应该放哪里」。
> **不是睡前读物。** 是你在动手加功能前、或者看不懂某段代码时查阅的参考书。

---

## 0. 读前须知

### 你应该已经理解

- DI 的基本概念：`services.AddSingleton<IXxx, Xxx>()` 是什么意思（见 ONBOARDING Day 2）
- 接口和实现的关系：`I*.cs` = 合约，`*Manager.cs` = 实现
- 能用 F10 跟踪一遍按钮点击到硬件调用的链路（见 ONBOARDING Day 3）

### 本文将回答

| 你想知道 | 跳到 |
|:---|:---|
| 加一个新功能，放哪个项目？ | [第 1 章：新功能放哪里（决策树）](#1-新功能放哪里决策树) |
| 这个项目为什么要分这么多层？ | [第 2 章：为什么分层](#2-为什么分层) |
| 我能从 A 项目引用 B 项目吗？ | [第 3 章：层间依赖规则](#3-层间依赖规则) |
| 什么时候要建新接口？ | [第 4 章：接口契约体系](#4-接口契约体系) |
| DI 注册要注意什么？ | [第 5 章：DI 容器使用规约](#5-di-容器使用规约) |
| 测试应该怎么写？测什么？ | [第 7 章：测试分层策略](#7-测试分层策略) |
| 有哪些绝对不能做的？ | [第 8 章：反模式清单](#8-反模式清单) |

---

## 1. 新功能放哪里（决策树）

**这是本文档最重要的一章。拿到一个新需求时，按下面的树找到应该改哪个项目、哪个文件。**

```
你要做什么？
│
├── 加一个 UI 页面
│   ├── ① src/OmniFrame.Wpf/ViewModels/XxxViewModel.cs  ← 新 ViewModel
│   ├── ② src/OmniFrame.Wpf/Views/XxxView.xaml          ← 新 XAML
│   ├── ③ MainViewModel.BuildNavigation() 加导航项
│   └── ④ DiConfigurator.cs 注册 AddTransient<XxxViewModel>()
│
├── 加一个新的全局管理器（比如「能源管理器」管设备功耗）
│   ├── ① src/OmniFrame.Core/IEnergyManager.cs    ← 先写接口
│   ├── ② src/OmniFrame.Core/EnergyManager.cs     ← 再写实现
│   └── ③ DiConfigurator.cs 注册 AddSingleton<IEnergyManager, EnergyManager>()
│
├── 给已有的管理器加方法（比如 AlarmManager 加一个 ExportToExcel）
│   ├── 在 IAlarmManager.cs 加方法声明
│   └── 在 AlarmManager.cs 加方法实现
│
├── 加一个新的生产工站
│   ├── 简单工站（同步步骤）:
│   │    继承 src/OmniFrame.Core/StationBase → 重写 DoExecute()
│   │    在 DiConfigurator.cs 注册 AddSingleton<MyStation>()
│   │
│   └── BlockCut 工站（异步循环）:
│        继承 src/OmniFrame.Core/BlockCut/BlockCutStationBase → 重写 RunAsync()
│        在 DiConfigurator.cs 注册 AddSingleton<MyBlockCutStation>()
│        在 StationCoordinator.cs 的 WireCrossStationEvents() 中连接跨工站事件
│
├── 加一个新的通信协议（比如 EtherNet/IP）
│   ├── 在 src/Plc/ 新建 Plc_EtherNetIP.cs → 继承 PlcDevice
│   └── 在 PlcManager.Initialize() 的 switch 里加新 case
│
├── 加一个新的运动控制卡品牌
│   ├── 在 src/MotionIO/ 新建 Motion_NewBrand.cs → 继承 Motion
│   └── 在 MotionManager 的工厂方法里加新 case
│
├── 加一个数据库表（比如存操作日志）
│   ├── 在 DataAccess 项目加 CreateTable / Insert / Query 方法
│   └── 在 Core 某 Manager 中调用
│
├── 加一个远程 API（给 MES 系统调用的接口）
│   └── 在 src/RemoteMonitor/ 加新 Controller 方法
│
└── 不确定放哪？
    → 问 mentor。但大概率在 Core 项目。
```

### 快速判断：文件放哪个项目

| 特征 | 放哪 |
|:---|:---|
| 里面用了 `Window`、`UserControl`、XAML | `OmniFrame.Wpf`（UI 层） |
| 里面用了 `Motion`、`IoCtrl`、`PlcDevice`，但不是 UI | `OmniFrame.Core`（领域层） |
| 里面用了 `Socket`、`TcpClient`、`SerialPort` | `Communication` |
| 里面拼了 Modbus 帧（`byte[]` 拼接） | `Plc` |
| 里面调了运动控制卡的 DLL（`GT_Open()` 之类） | `MotionIO` |
| 里面写了 SQL（`SELECT ... FROM ...`） | `DataAccess` |
| 纯工具函数（编码转换、文件读写） | `Common` |
| 有 `[Test]` 标记 | `tests/OmniFrame.Tests` |

---

## 2. 为什么分层

### 用一个真实场景解释

假设生产线要换一个品牌的运动控制卡（固高 → 汇川）。

**不分层的项目：**
你需要改 20 个文件。每个 `MoveAbs()` 调用都要找到、改成新 API。测试？不存在。上线 = 赌命。

**分层的项目：**
1. 写一个新的 `Motion_Inovance.cs`（继承 `Motion` 抽象类）
2. 在 `DiConfigurator.cs` 里改一行注册
3. **其他 0 个文件需要改。**

### 每一层的职责

```
┌────────────────────────────────────┐
│ OmniFrame.Wpf (WPF UI)            │  ← 只管界面：按钮、输入框、表格
│   依赖 → Core 接口                  │     绝不包含运动控制代码
├────────────────────────────────────┤
│ OmniFrame.Core (领域/大脑)          │  ← 核心：Manager + 工站 + 业务规则
│   依赖 → Communication/Plc/MotionIO │     绝不包含 UI 代码
│         DataAccess/Common           │
├──────────┬──────────┬──────────────┤
│ Comm     │ Plc      │ MotionIO     │  ← 硬件抽象：统一接口，隐藏品牌差异
│ TCP/串口 │ Modbus/  │ 固高/雷赛/   │
│ OPC      │ 三菱MC   │ 汇川         │
├──────────┴──────────┴──────────────┤
│ DataAccess (SQLite/MySQL)          │  ← 只管存和取，不管业务逻辑
├────────────────────────────────────┤
│ Common (日志/加密/工具函数)         │  ← 零业务依赖，纯工具
└────────────────────────────────────┘
```

**记忆口诀：UI 不碰硬件，大脑不画界面，硬件不管业务。**

---

## 3. 层间依赖规则

### 3.1 允许的依赖

```
OmniFrame.Wpf (UI) ────→ Core 的接口 (I*.cs)     ✅
Core             ────→ Comm / Plc / MotionIO   ✅
Core             ────→ DataAccess              ✅
Comm / Plc / MotionIO → Common                ✅
DataAccess       ────→ Common                  ✅

Common ──────────→ 任何人                       ❌ Common 是叶子节点
Core    ──────────→ OmniFrame.Wpf (UI)             ❌ 大脑不知道界面存在
UI      ──────────→ MotionIO / Plc (直接调)     ❌ 必须通过 Core 的接口
```

### 3.2 两条铁律

**铁律 1：UI 只通过接口访问 Core**

```csharp
// ✅ MainForm 里
private readonly IAlarmManager _alarmMgr;     // 接口！
public MainForm(IAlarmManager alarmMgr) { ... }

// ❌ 绝对不要
private readonly AlarmManager _alarmMgr;       // 具体类！
```

**铁律 2：Common 不引用任何 OmniFrame 项目**

Common 是纯工具库。它不知道 UI、不知道工站、不知道 PLC。

### 3.3 如何自查

在 PowerShell 里运行：

```powershell
# Common 不该引用任何 OmniFrame 项目
dotnet list src/Common/Common.csproj reference

# Core 不该引用 OmniFrame (UI)
dotnet list src/OmniFrame.Core/OmniFrame.Core.csproj reference
```

---

## 4. 接口契约体系

### 4.1 当前有哪些接口

所有接口放在 `src/OmniFrame.Core/` 下，文件名为 `I*.cs`（23 个）：

| 接口文件 | 管什么 |
|:---|:---|
| `IAlarmManager.cs` | 告警的增删查 |
| `IAlarmNotification.cs` | 告警通知（企微/邮件/短信） |
| `IAuditLogger.cs` | 审计日志 |
| `IConfigManager.cs` | 配置文件读写、热加载 |
| `ICylinderManager.cs` | 气缸控制 |
| `IDataManager.cs` | 产品数据持久化 |
| `IDeviceManager.cs` | 硬件设备管理 |
| `IHealthCheckService.cs` | 系统健康检查 |
| `IIoManager.cs` | IO 输入输出 |
| `ILightManager.cs` | 灯光控制 |
| `IMotionManager.cs` | 运动控制 |
| `IPermissionManager.cs` | 权限检查 |
| `IPlcManager.cs` | PLC 通信 |
| `IProductManager.cs` | 产品生命周期 |
| `IProductionManager.cs` | 生产订单管理 |
| `IRecipeManager.cs` | 配方管理 |
| `IReconnectionService.cs` | 设备自动重连 |
| `IReportManager.cs` | 报表生成 |
| `IStationManager.cs` | 工站管理 |
| `ISystemManager.cs` | 系统核心协调器 |
| `ITaskManager.cs` | 任务调度 |
| `IUserManager.cs` | 用户登录/权限 |
| `IWatchdogService.cs` | 系统看门狗 |

### 4.2 什么时候需要新建接口

| 场景 | 动作 |
|:---|:---|
| 新增全局功能（如能源管理） | 建 `IEnergyManager.cs` + `EnergyManager.cs` |
| 已有功能加方法 | 直接在已有接口上加 |
| 已有功能换实现 | 接口不变，新建实现类 |

### 4.3 一个好的接口长什么样

```csharp
// 好的接口：职责单一，一看就懂
public interface IAlarmManager : IDisposable
{
    bool HasActiveAlarm { get; }                    // 只读查询
    int ActiveAlarmCount { get; }                   // 只读查询

    AlarmInfo AddAlarm(string code, ...);           // 命令（有副作用）
    bool ClearAlarm(int alarmId, ...);              // 命令
    List<AlarmInfo> GetActiveAlarms();              // 查询（无副作用）

    event EventHandler<AlarmInfo> AlarmOccurred;    // 通知事件
}
```

**规则：** 属性 = 名词/问句（`HasActiveAlarm`）。方法 = 动词（`AddAlarm`）。事件 = 过去式（`AlarmOccurred`）。

---

## 5. DI 容器使用规约

### 5.1 只有一个注册入口

**`src/OmniFrame.Wpf/DiConfigurator.cs`** 是唯一注册所有服务的地方。

```csharp
// 日常最常用的 3 种注册方式：

// ① 接口 → 实现  (最常用)
services.AddSingleton<IAlarmManager, AlarmManager>();

// ② 接口 → 工厂方法  (需要额外初始化逻辑时)
services.AddSingleton<IBlockCutHardware>(sp =>
{
    var cfg = sp.GetRequiredService<BlockCutConfig>();
    if (cfg.IsSimulation)
        return SimulationContext.CreateSimulatedHardware(16);
    return new ApsHardware(0);
});

// ③ 直接注册类型  (Form 等)
services.AddTransient<LoginForm>();
```

### 5.2 Singleton vs Transient

| | Singleton | Transient |
|:---|:---|:---|
| 实例数量 | 全局 1 个 | 每次请求新建 |
| 用在哪里 | Manager、工站、硬件 | Form、UserControl |
| 为什么 | 全局唯一（告警系统不能有两个） | 每次打开窗口需要新状态 |

> ⚠️ **不要把 Form 注册为 Singleton。** 关闭再打开窗口时会带着上一次的脏数据。

### 5.3 不要做的事

| ❌ | ✅ |
|:---|:---|
| 在 Form 里 `new AlarmManager()` | 构造器注入 `IAlarmManager` |
| 在 Core 项目引用 DI 库 | Core 不依赖任何 DI 框架 |
| 创建第二个 `ServiceProvider` | 全局只有一个容器 |
| 注册了但不用接口 | `AddSingleton<AlarmManager>` → 应用里应该 `IAlarmManager` |

---

## 6. 工站架构

### 6.1 两条继承链

```
StationBase                          简单同步工站
  ├── 你的简单工站                    继承它，重写 DoExecute()
  │
  └── BlockCutStationBase            复杂异步工站
        ├── Station_Adjust           矫正工站
        ├── Station_CasselZ          料塔工站
        ├── Station_Load             上料工站
        ├── Station_Load2            二次上料工站
        ├── Station_BottomGet        底板取放工站
        └── Station_Safe             安全工站
```

### 6.2 什么时候用哪个

| 场景 | 继承 |
|:---|:---|
| 工站是一次性的（做完一个循环就结束） | `StationBase` |
| 工站是持续运行的（while 循环一直跑） | `BlockCutStationBase` |
| 工站需要控制气缸、运动轴、读传感器 | `BlockCutStationBase`（封装好了） |

### 6.3 工站之间怎么通信

**通过 StationCoordinator，而不是直接互相调用。**

```csharp
// ✅ 正确：StationCoordinator 连接两个工站
_load.OnNoticeCasselLoad1Out += () => _casselZ.ReadyFromLoad1YOut();

// ❌ 错误：工站 A 直接调工站 B
// stationA.DoSomethingThatCallsStationB();  ← 耦合！
```

---

## 7. 测试分层策略

### 7.1 测试金字塔

```
      ╱  E2E  ╲          手动操作一遍完整流程（极少，发布前做）
     ╱  集成   ╲          多个 Manager 协作（少）
    ╱  单元测试 ╲         单个类 + Mock 依赖（多，每次改代码都跑）
```

### 7.2 各类测什么、怎么测

| 测试对象 | 用什么 | 示例 |
|:---|:---|:---|
| Manager | Mock 外部依赖（用 Moq 假装有 DB） | Mock `IAlarmDb` → 测 `AlarmManager.AddAlarm()` |
| StationBase 工站 | 继承 TestStation，覆盖 `DoExecute()` | 测 Execute 方法的重试逻辑 |
| BlockCut 工站 | `SimulatedHardware` 替代真硬件 | 测 `RunAsync()` 的 while 循环 |
| StationCoordinator | 真 Station + SimulatedHardware | `StationCoordinatorTests.cs`（28 个测试） |

### 7.3 命名规范

```
方法名_什么场景_期望什么结果

StartAll_SetsIsRunningTrue
StartAll_Twice_DoesNotDoubleStart
Constructor_NullAdjust_ThrowsArgumentNullException
Execute_FailsThreeTimes_EntersErrorState
```

---

## 8. 反模式清单

### 🔴 绝对不要（会导致架构腐化）

| 反模式 | 例子 | 后果 |
|:---|:---|:---|
| **Form 里写业务逻辑** | `MainForm.cs` 里调 `MoveAbs()` | UI 和业务纠缠，换不了 UI，测不了逻辑 |
| **跳过接口直接调实现** | `new AlarmManager()` 而不是 `IAlarmManager` | Mock 不了，测试写不了 |
| **Common 引用业务项目** | Common 里 `using OmniFrame.Core` | 循环依赖，Common 变成垃圾场 |
| **工站间直接互相调用** | StationA 直接调 StationB 的方法 | 耦合死，测试不了 |
| **硬编码配置** | `Thread.Sleep(3500)` | 不知道 3500 是什么意思 |

### 🟡 尽量避免（降低可维护性）

| 反模式 | 建议 |
|:---|:---|
| `catch { }` 空捕获 | 至少 `Logger.Warning("xxx失败", ex)` |
| Manager 类超过 500 行 | 拆成子模块 |
| 多个文件注册 DI | 全放在 `DiConfigurator.cs` |

### 改完代码自查

- [ ] Form 里有 `Motion.` 或 `PlcDevice.` 开头的方法吗？（不应该有）
- [ ] 有没有用 `new ServiceCollection()` 创建新容器？（不应该有）
- [ ] 新增功能能在仿真模式下跑起来吗？（不能 = 耦合了真实硬件）
- [ ] 新类有对应的接口吗？在 `DiConfigurator` 注册了吗？

---

## 📚 延伸阅读

| 你需要 | 读这个 |
|:---|:---|
| 新人完整学习路径（Day 0 → Week 3） | [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) |
| 5 分钟跑起来 | [quickstart_zh.md](quickstart_zh.md) |
| 编码规范 | [code-patterns_zh.md](code-patterns_zh.md) |
| 创建新工站 | [adding-new-station_zh.md](adding-new-station_zh.md) |
| 仿真模式 | [simulation-guide_zh.md](simulation-guide_zh.md) |
| 代码审查报告 | [CODE_REVIEW_REPORT.md](CODE_REVIEW_REPORT.md) |
| 改进指南 | [IMPROVEMENT_GUIDE.md](IMPROVEMENT_GUIDE.md) |
