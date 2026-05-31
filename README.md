# OmniFrame

<div align="center">

![C#](https://img.shields.io/badge/C%23-.NET%20Framework%204.8%20|%20.NET%208-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-green.svg)
![UI](https://img.shields.io/badge/UI-WPF%20%2B%20MVVM-purple.svg)
![Version](https://img.shields.io/badge/Version-4.0.0-yellow.svg)

**面向精密制造的工业自动化上位机平台**

[快速开始](#快速开始) · [架构](#架构设计) · [文档](#文档) · [模块](#核心模块)

</div>

---

## 项目概述

OmniFrame 是基于 **C# / WPF / .NET** 构建的工业自动化上位机平台，面向精密制造行业的运动控制、PLC 通信、视觉检测、数据采集等场景。

采用 **MVVM 架构 + 依赖注入**，内置 WebSocket 数字孪生、远程监控、插件系统。支持多品牌运动控制卡（固高、雷赛、汇川）和多协议通信（Modbus TCP/RTU、三菱 MC、OPC DA、串口、TCP）。仿真模式无需硬件即可开发调试。

---

## 核心特性

| 特性 | 描述 |
|:---|:---|
| **分层架构** | 7 层架构：UI / 领域层 / 通信 / 硬件抽象 / 数据访问 / 远程监控 / 公共基础设施 |
| **依赖注入** | Microsoft.Extensions.DependencyInjection + Service Locator 桥接模式，34+ 管理器全部面向接口编程 |
| **数字孪生** | WebSocket 实时推送轴状态、IO 状态、设备状态到 3D 可视化前端 |
| **插件系统** | 运行时动态加载运动控制/PLC/业务插件，支持版本管理和远程更新回滚 |
| **多协议通信** | Modbus TCP/RTU、三菱 MC、OPC DA、TCP Server/Client、RS-232 串口，统一接口接入 |
| **多品牌运动控制** | 固高 GTS、雷赛 DMC3000/DMC3400、汇川 EtherCAT，Factory Method 工厂模式创建 |
| **配置热加载** | FileSystemWatcher + 轮询双机制，配置文件变更实时生效 |
| **密码安全** | BCrypt 哈希 + 5 种旧格式向下兼容的多级验证链 |
| **告警分级通知** | 4 级告警分别路由到日志/企业微信/Email/SMS，5 分钟去重 |
| **远程监控** | 内置 WebSocket (8080) + REST API (8081)，JWT 认证 + 速率限制 |
| **审计日志** | Serilog JSON Lines 审计日志，记录用户操作、配置变更、设备指令 |
| **设备自动重连** | 断连自动恢复，指数退避重试(1s→2s→4s→8s→15s)，3次失败触发报警 |
| **系统看门狗** | 独立心跳线程，连续5次健康检查失败触发安全停机+诊断快照 |
| **工站自动恢复** | 工站异常自动重试(500ms→1s→2s退避)，3次失败后暂停 |
| **崩溃转储** | 未处理异常自动写入 Logs/CrashDumps/ 供事后分析 |

---

## 架构设计

```
┌──────────────────────────────────────────────────────────┐
│                    Presentation Layer                     │
│              OmniFrame.Wpf (WPF + MVVM)                  │
├──────────────────────────────────────────────────────────┤
│                    Domain Layer                           │
│              OmniFrame.Core                              │
│   SystemManager (Facade) ─── 协调 13 个子系统             │
│   ├── DeviceManager    ─── 硬件设备管理                    │
│   ├── TaskManager      ─── 生产者-消费者任务调度            │
│   ├── AlarmManager     ─── 分级告警 + 多通道通知           │
│   ├── DataManager      ─── 异步队列数据持久化               │
│   ├── UserManager      ─── BCrypt 用户权限管理              │
│   ├── RecipeManager    ─── 配方 XML 持久化                 │
│   ├── PermissionManager ── 权限审计 + CheckAndExecute      │
│   ├── PlcMgr           ─── PLC 适配器                     │
│   ├── MotionMgr        ─── 运动控制适配器                  │
│   ├── IoMgr            ─── IO 适配器                      │
│   ├── RobotMgr         ─── 机器人适配器                    │
│   └── ProductMgr       ─── 产品/工单生命周期管理            │
│   其他: StationMgr / StationBase / ProductionManager       │
│   ReportManager / DigitalTwinBridge / WarningMgr           │
│   ReconnectionService ─── 设备自动重连                      │
│   WatchdogService ────── 系统看门狗 + 诊断快照               │
│   JsonConfigProvider ─── JSON 统一配置（规划中）                │
├──────────────────────────────────────────────────────────┤
│               Communication Layer                         │
│   OmniFrame.Communication + Plc                          │
│   Modbus TCP/RTU │ Mitsubishi MC │ OPC DA │ TCP │ 串口    │
├──────────────────────────────────────────────────────────┤
│               Hardware Abstraction Layer                  │
│   MotionIO + OmniFrame.Communication                     │
│   固高GTS │ 雷赛DMC3000/3400 │ 汇川EtherCAT │ PCIeM60    │
├──────────────────────────────────────────────────────────┤
│               Data Access Layer                           │
│   OmniFrame.DataAccess                                   │
│   SQLite │ MySQL │ MES HTTP Client                       │
├──────────────────────────────────────────────────────────┤
│               Cross-Cutting Layer                         │
│   OmniFrame.Common                                       │
│   DI Bridge │ Serilog │ Security │ I18N │ 工具类          │
└──────────────────────────────────────────────────────────┘
```

## 文档

### 📖 新人入门（按顺序阅读）

| # | 文档 | 说明 | 预计时间 |
|:---|:---|:---|:---|
| 1 | [WPF_BASICS.md](docs/WPF_BASICS.md) | 🎓 WPF 零基础教学（XAML/绑定/MVVM/DataTemplate） | 1 小时 |
| 2 | [quickstart_zh.md](docs/quickstart_zh.md) | ⚡ 5 分钟从零跑到主界面 | 30 分钟 |
| 3 | [ONBOARDING_GUIDE.md](docs/ONBOARDING_GUIDE.md) | 🗓️ 新人训练营 Day 0 → Week 3 完整学习路径 | 3 周 |
| 4 | [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md) | 🏗️ 架构决策指南（新功能放哪里 / 为什么分层 / 反模式清单） | 反复查 |
| 5 | [code-patterns_zh.md](docs/code-patterns_zh.md) | 📏 编码规范（DI 注册 / 错误处理 / 命名规范） | 随手翻 |

### 📘 开发实战

| 文档 | 说明 |
|:---|:---|
| [adding-new-station_zh.md](docs/adding-new-station_zh.md) | 如何添加新工站（状态机模式 / SetSignal / WaitSignal / DI 注册 / 测试） |
| [simulation-guide_zh.md](docs/simulation-guide_zh.md) | 模拟层使用指南（无需真实硬件即可开发调试） |
| [NATIVE_DLL_GUIDE.md](docs/NATIVE_DLL_GUIDE.md) | 工业 DLL 接入实战（P/Invoke / 雷赛 / 海康 / x86 vs x64） |

### 📙 深度专题

| 文档 | 说明 |
|:---|:---|
| [communication-guide_zh.md](docs/communication-guide_zh.md) | 通信层开发指南（TCP/串口/OPC DA/Modbus/三菱MC/连接池） |
| [motion-control-guide_zh.md](docs/motion-control-guide_zh.md) | 运动控制开发指南（固高/雷赛/汇川 / 回原点 / IO / 安全规范） |
| [mes-integration-guide_zh.md](docs/mes-integration-guide_zh.md) | MES 集成开发指南（JWT 认证 / 数据上传 / OEE / Mock Server） |
| [blockcut-migration-plan_zh.md](docs/blockcut-migration-plan_zh.md) | BlockCut Qt C++ → .NET C# 迁移方案（模块映射 / 16 轴 / 22 周计划） |
| [OmniFrame项目文档.md](docs/OmniFrame项目文档.md) | 📚 项目完整技术文档（10 模块详解 / DI / 插件 / 报警 / 权限） |

### 📋 参考查询

| 文档 | 说明 |
|:---|:---|
| [glossary_zh.md](docs/glossary_zh.md) | 术语表（工业自动化 / PLC / 运动控制 / 软件架构 中英对照） |
| [troubleshooting_zh.md](docs/troubleshooting_zh.md) | 常见问题排查 FAQ（编译 / 运行时 / v3.0.1 已知问题） |
| [CODE_REVIEW_REPORT.md](docs/CODE_REVIEW_REPORT.md) | 🔍 代码审阅报告（架构评分 / 问题分析 / 改进方案） |
| [IMPROVEMENT_GUIDE.md](docs/IMPROVEMENT_GUIDE.md) | 🛠️ 代码改进实施指南（异常处理 / 并发优化 / 数据库抽象 / Polly 重试） |

### 🚀 交付文档 (delivery/)

| 文档 | 说明 |
|:---|:---|
| [版本说明.md](docs/delivery/版本说明.md) | 版本历史（v3.0.0 / v3.0.1 / v3.1.0）& 升级指南 |
| [系统部署指南.md](docs/delivery/系统部署指南.md) | 硬件/软件要求 / 安装步骤 / 配置 / 验证测试 |
| [维护与故障排除手册.md](docs/delivery/维护与故障排除手册.md) | 日常/周/月/季维护 / P0-P3 故障分类 / 应急处理 |
| [软件用户手册.md](docs/delivery/软件用户手册.md) | 产品简介 / 操作指南 / 快捷键 / FAQ |

> 💡 文档提供**中英双语版本**（`_zh` 后缀为中文版），英文版见同名不带后缀的文件。

### 项目结构

```
OmniFrame.sln
├── src/
│   ├── OmniFrame.Wpf/                # 🖥️ WPF UI (MVVM, 20+ 页面)
│   │   ├── Program.cs                # DI 容器配置入口
│   │   ├── Forms/                    # LoginForm, MainForm, DashboardForm 等
│   │   └── AdvancedFeatures/         # OEE, MQTT, MES, 视觉, 数据分析
│   │
│   ├── OmniFrame.Core/              # 领域层 (IA 命名规范)
│   │   ├── SystemManager.cs         # 核心协调器
│   │   ├── ConfigManager.cs         # 配置管理
│   │   ├── UserManager.cs           # 用户权限
│   │   ├── AlarmManager.cs          # 告警管理
│   │   ├── TaskManager.cs           # 任务调度
│   │   ├── DeviceManager.cs         # 设备管理
│   │   ├── DataManager.cs           # 数据管理
│   │   ├── RecipeManager.cs         # 配方管理
│   │   ├── StationMgr.cs            # 工站管理
│   │   ├── StationBase.cs           # 工站基类
│   │   ├── PermissionManager.cs     # 权限审计
│   │   ├── PlcMgr.cs                # PLC 管理
│   │   ├── MotionMgr.cs             # 运动控制管理
│   │   ├── IoMgr.cs                 # IO 管理
│   │   ├── RobotMgr.cs              # 机器人管理
│   │   ├── ProductMgr.cs            # 产品管理
│   │   ├── ProductionManager.cs     # 生产管理
│   │   ├── ReportManager.cs         # 报表管理
│   │   ├── WarningMgr.cs            # 警告管理
│   │   ├── LightMgr.cs              # 灯光控制
│   │   ├── CylinderMgr.cs           # 气缸控制
│   │   ├── DigitalTwinBridge.cs     # 数字孪生
│   │   ├── ErrorCode.cs             # 错误码定义 (1000+)
│   │   ├── AlarmNotification.cs     # 告警通知 (企微/邮件/SMS)
│   │   ├── ConfigImportExport.cs    # 配置导入导出
│   │   ├── SystemMgr.cs             # 遗留系统管理
│   │   └── PluginSystem/            # 插件系统
│   │
│   ├── OmniFrame.Common/           # 公共基础设施
│   │   ├── AppServiceLocator.cs     # DI 桥接器
│   │   ├── Singleton.cs             # 泛型单例基类
│   │   ├── Logger.cs                # Serilog 日志 (文件 + 控制台)
│   │   ├── AuditLogger.cs           # 审计日志 (JSON Lines)
│   │   ├── LanguageMgr.cs           # 多语言 (XML)
│   │   ├── Security.cs              # 加解密工具
│   │   └── Helpers/                 # Byte/CSV/INI/XML/File/Time/Enum 工具类
│   │
│   ├── OmniFrame.Communication/    # 通信层
│   │   ├── ComMgr + ComLink        # 串口 (SerialPort)
│   │   ├── TcpMgr + TcpLink        # TCP 客户端
│   │   ├── TcpServerMgr + TcpServer # TCP 服务器
│   │   ├── AsyncTcpClient          # 异步 TCP (重连/心跳)
│   │   ├── AsyncTcpServer          # 异步 TCP 服务器
│   │   ├── OpcMgr + OpcLink        # OPC DA (COM)
│   │   ├── ConnectionPool.cs       # 泛型连接池
│   │   └── Notify.cs               # 通知系统
│   │
│   ├── Plc/                        # PLC 协议层
│   │   ├── PlcDevice.cs            # 抽象基类 (模板方法)
│   │   ├── Plc_ModbusTcp.cs        # Modbus TCP
│   │   ├── Plc_ModbusRtu.cs        # Modbus RTU (CRC-16)
│   │   ├── Plc_Mitsubishi.cs       # 三菱 MC 协议
│   │   ├── Modbus.cs               # Modbus 帧构建
│   │   ├── DataBlock.cs            # 批量数据块
│   │   └── PlcMonitor.cs           # PLC 地址监控
│   │
│   ├── MotionIO/                   # 运动控制层
│   │   ├── Motion.cs               # 抽象基类
│   │   ├── Motion_GTS.cs           # 固高 GTS
│   │   ├── Motion_Dmc3000.cs       # 雷赛 DMC3000
│   │   ├── Motion_DMC3400.cs       # 雷赛 DMC3400
│   │   ├── Motion_InoEcat.cs       # 汇川 EtherCAT
│   │   ├── Motion_PCIeM60.cs       # PCIeM60
│   │   ├── IoCtrl.cs               # IO 控制抽象
│   │   ├── IoCtrl_PCIeM60.cs       # PCIeM60 IO
│   │   ├── MotionDevice.cs         # 运动设备包装器
│   │   └── MotionMgr.cs            # 运动管理器 (Factory Method)
│   │
│   ├── RemoteMonitor/              # 远程监控
│   │   ├── RemoteMonitorManager.cs # 远程监控管理器
│   │   ├── WebSocketServer.cs      # WebSocket 服务 (:8080)
│   │   ├── WebApiController.cs     # REST API (:8081, JWT)
│   │   └── SecurityAuditAttribute.cs
│   │
│   ├── OmniFrame.DataAccess/      # 数据持久化
│   │   ├── SqliteHelper.cs         # SQLite 封装
│   │   ├── MySQLHelper.cs          # MySQL 封装
│   │   ├── AlarmDb.cs              # 告警数据库
│   │   ├── ProductDb.cs            # 产品数据库
│   │   └── MesClient.cs            # MES HTTP 客户端
│   │
│   ├── OmniFrame.Sdk.PluginSystem/ # 插件 SDK
│   │   ├── PluginBase.cs           # 抽象基类
│   │   ├── MotionPlugin.cs         # 运动控制插件
│   │   ├── PlcPlugin.cs            # PLC 插件
│   │   └── BusinessPlugin.cs       # 业务插件
│   │
│   └── AutoFrame.Tests/            # 单元测试 (NUnit + Moq)
│
├── lib/                            # 第三方 DLL 引用
├── docs/                           # 项目文档
├── packages/                       # NuGet 包缓存
└── tools/                          # 构建工具
```

---

## 核心技术栈

| 层级 | 技术 |
|:---|:---|
| **语言** | C# 9.0 / .NET Framework 4.8 |
| **UI 框架** | WinForms (20+ 窗体) |
| **DI 容器** | Microsoft.Extensions.DependencyInjection |
| **日志** | Serilog (滚动文件 + 控制台) |
| **数据库** | SQLite (System.Data.SQLite)、MySQL |
| **通信** | Modbus TCP/RTU、三菱 MC、OPC DA、TCP、串口、WebSocket |
| **远程监控** | WebSocket + REST API (JWT 认证) |
| **单元测试** | NUnit + Moq |
| **序列化** | XmlSerializer、Newtonsoft.Json |

---

## 设计模式应用

| 模式 | 位置 |
|:---|:---|
| **Singleton** | 所有 Manager (通过 AppServiceLocator 统一管理) |
| **Facade** | SystemManager 统一协调 13 个子系统 |
| **Observer** | 30+ event 类型 (StateChanged, AlarmOccurred, DataReceived...) |
| **Template Method** | PlcDevice 协议实现骨架 · TaskBase 执行骨架 · SystemManager 初始化 |
| **State** | SystemState 7 状态 · TaskState 6 状态 · StationState 5 状态 |
| **Producer-Consumer** | TaskManager 任务队列 · DataManager 数据持久化队列 |
| **Adapter** | PlcMgr/MotionMgr/IoMgr 包装硬件驱动 |
| **Factory Method** | MotionMgr.CreateMotion 根据配置创建运动卡实例 |
| **Chain of Resp.** | 密码验证链: bcrypt → MD5 → SHA1 → XOR → Base64 |
| **Object Pool** | ConnectionPool\<T\> 泛型连接池 |
| **Command** | TaskBase 子类作为命令对象 |

---

## 核心模块

### SystemManager — 系统协调器 (Facade)

统一管理系统生命周期：`Initialize()` → `Start()` → `Stop()` → `EmergencyStop()` → `Reset()`。

```csharp
// 通过 DI 容器获取 (推荐)
var systemManager = serviceProvider.GetRequiredService<ISystemManager>();
systemManager.Initialize();
systemManager.Start();

// 或通过 Instance 桥接 (兼容旧代码)
SystemManager.Instance.StateChanged += (s, e) => UpdateUI(e.NewState);
```

初始化顺序：DeviceManager → TaskManager → AlarmManager → PluginManager → DataManager → UserManager → PermissionManager → RecipeManager，失败时反向回滚。

### DI 容器配置

Program.cs 作为组合根，注册所有服务：

```csharp
var services = new ServiceCollection();

// 注册所有管理器服务
services.AddSingleton<ISystemManager, SystemManager>();
services.AddSingleton<IDeviceManager, DeviceManager>();
services.AddSingleton<IUserManager, UserManager>();
services.AddSingleton<IRecipeManager, RecipeManager>();
services.AddSingleton<IDataManager, DataManager>();
services.AddSingleton<IPermissionManager, PermissionManager>();

// 注册 WinForms 窗体
services.AddTransient<LoginForm>();
services.AddSingleton<MainForm>();
services.AddTransient<DashboardForm>();
services.AddTransient<ConfigWizardForm>();

var provider = services.BuildServiceProvider();
AppServiceLocator.Initialize(provider);
```

### 运动控制 — 工厂方法模式

```csharp
// 从配置读取运动卡类型，自动创建对应实现
MotionMgr.Instance.CreateMotion("GTS", 0, "X-Axis", 1, 4);
// 或
MotionMgr.Instance.CreateMotion("DMC3400", 0, "Y-Axis", 1, 8);
```

### PLC 通信 — 模板方法模式

```csharp
// 统一接口，不同协议
var plc = new Plc_ModbusTcp("192.168.1.100", 502);
plc.ReadBit(SoftElement.X, 0);       // 读取 X0 输入
plc.WriteWord(SoftElement.D, 100, 1234); // 写入 D100

var mitsubishi = new Plc_Mitsubishi("192.168.1.200", 5000);
mitsubishi.ReadWord(SoftElement.D, 0);
```

### 数字孪生 — WebSocket 实时推送

```csharp
// 实时推送设备状态到 3D 可视化
await DigitalTwinBridge.Instance.ConnectAsync();
await DigitalTwinBridge.Instance.SendAxisUpdateAsync(
    new Dictionary<string, double> { { "axis_1", 123.45 } });
await DigitalTwinBridge.Instance.SendMachineStateAsync(
    MachineState.Running, true, false, false);
```

### 配置热加载

```csharp
// 获取配置（自动热加载）
var motionConfig = ConfigManager.Instance.GetConfig<MotionConfig>();
Console.WriteLine($"轴数: {motionConfig.AxisCount}");

// 保存配置（自动备份旧配置）
ConfigManager.Instance.SaveConfig(motionConfig);
// 触发 ConfigChanged 事件 → UI 自动更新
```

### 远程监控 API

```csharp
// REST API (:8081) — JWT 认证
GET  /api/status          # 系统状态
GET  /api/devices         # 设备状态  
POST /api/command         # 执行指令
POST /api/login           # 登录获取 JWT

// WebSocket (:8080) — 实时推送
实时推送轴位置、IO 状态、设备运行状态、告警信息
```

---

## 快速开始

### 环境要求

- Windows 10 / 11 x64
- .NET Framework 4.8 Developer Pack
- Visual Studio 2022（Community 即可）

### 构建运行

```powershell
git clone <repo-url> OmniFrame
cd OmniFrame

# 设置环境变量（仅首次）
[Environment]::SetEnvironmentVariable("OMNIFRAME_MES_AES_KEY", "DefaultSimulationKey123", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_HOST", "127.0.0.1", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_PORT", "5000", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY", "dev-key-placeholder", "User")

# 编译运行
dotnet restore OmniFrame.sln
dotnet build OmniFrame.sln
dotnet run --project src/OmniFrame.Wpf
```

默认登录：`admin / admin123`。仿真模式无需硬件即可体验全部功能。

> 📖 详细入门 [quickstart_zh.md](docs/quickstart_zh.md) | WPF 零基础教学 [WPF_BASICS.md](docs/WPF_BASICS.md)

---

## 业务功能模块

| 模块 | 功能 |
|:---|:---|
| **AlarmManager** | 分级告警 (Info/Warning/Error/Critical)，多通道通知，SQLite 持久化 |
| **UserManager** | BCrypt 密码哈希，5 级权限 (Operator/FAE/Adjustor/Engineer/Administrator) |
| **PermissionManager** | 细粒度权限检查 + 审计日志 (CheckAndExecute guard pattern) |
| **RecipeManager** | 配方创建/编辑/导入/导出，XML 文件存储，自动创建默认配方 |
| **TaskManager** | 生产者-消费者任务调度，支持优先级、暂停、恢复、中止 |
| **DataManager** | 异步队列持久化产品数据，批量写入 SQLite |
| **ProductionManager** | 生产订单管理，产量统计 |
| **StationMgr** | 多工站协同控制，状态机驱动 |
| **ReportManager** | 日报/周报/月报生成，CSV 导出 |
| **DigitalTwinBridge** | WebSocket 数字孪生桥接 (ws://localhost:3001) |
| **RemoteMonitor** | WebSocket 实时推送 (:8080) + REST API (:8081, JWT) |

---

## 隐私说明

本仓库仅用于**个人能力展示**，框架源代码**不公开**。

如需了解更多技术细节或商业合作，欢迎联系：

- Email: [您的邮箱]
- GitHub: [JamesW-ang](https://github.com/JamesW-ang)

---

## 更新日志

### v3.1.0 - 2026-05-10

**新增产线就绪特性**：

1. **设备自动重连 (ReconnectionService)**
   - 监听 `DeviceDisconnected` 事件，自动重试接接
   - 指数退避：1s → 2s → 4s → 8s → 15s（上限）
   - 3次失败触发报警，10次失败停止自动重连
   - 通过 `appsettings.json` 控制启停

2. **系统看门狗 (WatchdogService)**
   - 独立 `Timer` 线程（不受 UI 线程影响）
   - 每秒调用 `HealthCheckService` 检查组件状态
   - 连续5次不健康 → `EmergencyStop()` + 写入诊断快照
   - 诊断信息含 CPU、内存、线程数、组件故障详情

3. **统一配置 (JsonConfigProvider — 规划中)**
   - `Config/appsettings.json` 统一入口
   - 覆盖 Reconnect / Watchdog / System 参数
   - 首次运行自动创建默认文件

4. **工站自动恢复 (RunWithAutoRecoveryAsync)**
   - 6个 BlockCut 工站全部接入
   - 异常自动重试：500ms → 1s → 2s 退避
   - 超过3次 → 暂停并等待人工介入

5. **崩溃转储**
   - 未处理异常自动写入 `Logs/CrashDumps/crash_*.log`
   - 完整记录异常类型、消息、堆栈、内部异常、进程ID

### v3.0.1 - 2026-05-10

**修复内容**：

1. **插件管理界面 SplitterDistance 错误**
   - 问题：`SplitContainer` 在创建时设置 `Panel1MinSize` 和 `Panel2MinSize` 导致 `SplitterDistance` 超出有效范围
   - 修复：将尺寸设置移到 `HandleCreated` 事件中，确保容器宽度已确定后再动态计算

2. **操作日志不显示问题**
   - 问题1：查询按钮事件绑定和加载逻辑
   - 问题2：日期范围默认设置为7天，但历史日志数据为1个月前，查询不到结果
   - 修复：将默认日期范围从7天改为1个月

3. **BlockCut 配置不显示内容问题**
   - 问题：`BuildBlockCutTab()` 在 `LoadConfig()` 之后调用，导致控件未准备好
   - 修复：调整初始化顺序，确保控件创建完成后再加载配置

4. **环境变量缺失导致启动失败**
   - 问题：`OMNIFRAME_MES_AES_KEY` 和 `OMNIFRAME_BARCODE_HOST` 等环境变量未设置时应用无法启动
   - 修复：在仿真模式下使用默认值，避免启动失败

---

## License

本项目采用 **Private License** — 仅授权个人使用，未经许可不得开源或商用。

---

<div align="center">

**Built for Precision Manufacturing**

*© 2024-2026 OmniFrame. All rights reserved.*

</div>
