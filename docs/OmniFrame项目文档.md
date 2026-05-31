# OmniFrame 项目文档

> 基于 C# / WPF / .NET 的工业自动化上位机系统（同时兼容 WinForms 遗留模块）
> 适用领域：AOI 自动光学检测设备 / 通用运动控制

---

## 目录

1. [项目概述](#1-项目概述)
2. [解决方案结构](#2-解决方案结构)
3. [依赖注入与 IoC](#3-依赖注入与-ioc)
4. [模块详解](#4-模块详解)
   - 4.1 OmniFrame (UI 层)
   - 4.2 OmniFrame.Core (业务逻辑层)
   - 4.3 DataAccess (数据访问层)
   - 4.4 Communication (通信层)
   - 4.5 MotionIO (运动控制层)
   - 4.6 Plc (PLC 通信层)
   - 4.7 Common (公共基础库)
   - 4.8 OmniFrame.Sdk (插件 SDK)
   - 4.9 RemoteMonitor (远程监控)
   - 4.10 ToolEx (工具扩展)
5. [权限体系](#5-权限体系)
6. [插件系统](#6-插件系统)
7. [报警管理](#7-报警管理)
8. [数据管理](#8-数据管理)
9. [构建与部署](#9-构建与部署)
10. [常见任务](#10-常见任务)
11. [产线就绪服务](#11-产线就绪服务)
   - 11.1 [设备自动重连](#111-设备自动重连-reconnectionservice)
   - 11.2 [看门狗](#112-看门狗-watchdogservice)
   - 11.3 [工站自动恢复](#113-工站自动恢复-runwithautorecoveryasync)
   - 11.4 [配置](#114-配置)

---

## 1. 项目概述

### 1.1 软件定位

| 属性 | 值 |
|---|---|
| 软件类型 | 工业自动化上位机 (SCADA/HMI) |
| 应用领域 | AOI 自动光学检测、通用运动控制 |
| 开发语言 | C# 8.0 |
| 运行时 | .NET Framework 4.8 |
| UI 框架 | Windows Forms |
| DI 容器 | Microsoft.Extensions.DependencyInjection |
| 数据存储 | SQLite (System.Data.SQLite) |
| 插件架构 | 运行时加载 .dll，支持版本管理 |

### 1.2 核心功能

- **运动控制**：多轴控制、插补运动、回原点、IO 控制
- **PLC 通信**：Modbus TCP/RTU、三菱、汇川、基恩士
- **数据采集**：生产数据统计、报警记录、设备状态监控
- **MES 集成**：与制造执行系统对接，工单管理
- **权限管理**：三级角色体系（Operator / Engineer / Administrator）
- **多语言支持**：中英文界面切换
- **MQTT 通信**：设备间消息通信
- **OEE / UPH 管理**：设备综合效率与每小时产能统计
- **视觉系统**：图像采集与分析
- **远程监控**：WebSocket + HTTP API
- **数字孪生**：设备状态镜像
- **插件系统**：动态加载，版本管理，沙箱测试

### 1.3 用户角色

| 角色 | 权限级别 | 典型操作 |
|---|---|---|
| Operator | 基础操作 | 启动/停止工位、查看状态与报警 |
| Engineer | 工程维护 | 参数修改、单轴调试、日志查看、插件管理 |
| Administrator | 完全控制 | 用户管理、系统配置、远程调试 |

---

## 2. 解决方案结构

```
OmniFrame.sln
├── src/
│   ├── OmniFrame.Wpf/              # WPF 主程序 (UI 层, MVVM)
│   │   ├── Views/                  #   - XAML 界面
│   │   ├── ViewModels/             #   - 数据+命令
│   │   ├── Themes/                 #   - 主题与样式
│   │   └── DiConfigurator.cs       #   - DI 容器配置
│   ├── OmniFrame/                  # WinForms 兼容模块 (遗留窗体)
│   │
│   ├── OmniFrame.Core/             # 业务逻辑层
│   │   ├── AdvancedFeatures/       #   - OEE/UPH/MQTT/MES/视觉
│   │   ├── PluginSystem/           #   - 插件引擎
│   │   │   └── Testing/            #     - 插件测试框架
│   │   └── *.cs                    #   - 管理器类
│   │
│   ├── DataAccess/                 # 数据访问层
│   ├── Communication/              # 通信层 (TCP/OPC/串口)
│   ├── MotionIO/                   # 运动控制与 IO
│   ├── Plc/                        # PLC 通信协议
│   ├── Common/                     # 公共基础库
│   ├── OmniFrame.Sdk/              # 插件 SDK
│   ├── RemoteMonitor/              # 远程监控服务
│   └── ToolEx/                     # 工具扩展库
│
└── tools/                          # 独立工具
```

### 2.1 项目依赖关系

```
OmniFrame.Wpf  (WPF UI)
    │
    ├── OmniFrame.Core      (业务逻辑)
    │     ├── Common
    │     ├── DataAccess
    │     └── OmniFrame.Sdk
    │
    ├── Communication       (通信)
    ├── MotionIO            (运动控制)
    ├── Plc                 (PLC)
    ├── Common
    └── ToolEx
```

---

## 3. 依赖注入与 IoC

### 3.1 容器配置 (DiConfigurator.cs)

所有服务在 `src/OmniFrame.Wpf/DiConfigurator.cs` 中集中注册到 `ServiceCollection`：

```csharp
// Program.cs 简化示例
var services = new ServiceCollection();

// 核心服务
services.AddSingleton<IUserManager, UserManager>();
services.AddSingleton<IPermissionManager, PermissionManager>();
services.AddSingleton<IAlarmManager, AlarmManager>();
services.AddSingleton<IPluginManager, PluginManager>();

// 数据访问
services.AddSingleton<IProductDb, ProductDb>();
services.AddSingleton<IAlarmDb, AlarmDb>();
services.AddSingleton<IDataManager, DataManager>();

// 管理器
services.AddSingleton<IConfigManager, ConfigManager>();
services.AddSingleton<IRecipeManager, RecipeManager>();
services.AddSingleton<IDeviceManager, DeviceManager>();

// 主窗体
services.AddTransient<MainForm>();
services.AddTransient<LoginForm>(sp => new LoginForm(sp));
services.AddTransient<MonitorForm>();
```

### 3.2 ServiceProviderCache

WPF 通过 DI 构造器注入；遗留 WinForms Designer 要求无参构造函数，设计时实例使用静态 Service Locator 模式：

```csharp
// 仅用于 WinForms Designer 生成代码
public PluginManagerForm()
{
    _pluginManager = ServiceProviderCache.GetService<IPluginManager>();
    InitializeComponent();
}
```

运行时实例走正常的 DI 构造器注入，`ServiceProviderCache` 只作为过时的 Designer 后门。

### 3.3 接口清单

| 接口 | 实现 | 生命周期 | 用途 |
|---|---|---|---|
| `IUserManager` | `UserManager` | Singleton | 用户认证与会话管理 |
| `IPermissionManager` | `PermissionManager` | Singleton | 权限校验与审计 |
| `IAlarmManager` | `AlarmManager` | Singleton | 报警规则引擎 |
| `IPluginManager` | `PluginManager` | Singleton | 插件加载与生命周期 |
| `IDataManager` | `DataManager` | Singleton | 生产数据队列与持久化 |
| `IRecipeManager` | `RecipeManager` | Singleton | 配方管理 |
| `IConfigManager` | `ConfigManager` | Singleton | 系统配置 |
| `IDeviceManager` | `DeviceManager` | Singleton | 硬件设备抽象 |
| `IProductDb` | `ProductDb` | Singleton | 产品数据 SQLite 访问 |
| `IAlarmDb` | `AlarmDb` | Singleton | 报警数据 SQLite 访问 |

---

## 4. 模块详解

### 4.1 OmniFrame.Wpf (UI 层) + OmniFrame (遗留 WinForms)

WPF 主程序（MVVM 架构）+ WinForms 兼容模块，项目结构：

| 文件 | 说明 |
|---|---|
| `MainForm.cs` | 主框架窗口，菜单 + 工具栏 + 导航树 + 状态栏。支持操作员/工程师视图切换 |
| `MonitorForm.cs` | AOI 监控主界面，OK/NG 大指示器 + 良率 + 缺陷列表 + 实时状态 |
| `LoginForm.cs` | 登录窗口，DI 或无参两种构造方式 |
| `DashboardForm.cs` | 仪表板，生产概览 |
| `StationForm.cs` | 工位控制面板 |
| `EquipmentControlForm.cs` | 设备控制 |
| `AlarmCenterForm.cs` | 报警中心，实时报警 + 历史查询 |
| `OperationLogForm.cs` | 操作日志查询与导出 |
| `RecipeForm.cs` | 配方管理 |
| `ConfigWizardForm.cs` | 配置向导 |
| `SettingForm.cs` | 系统设置 |
| `RoleManagerForm.cs` | 角色与用户管理 |
| `PluginManagerForm.cs` | 插件管理界面 |
| `ProductionReportForm.cs` | 生产报表 |
| `ReportCenterForm.cs` | 报表中心 |
| `DigitalTwinForm.cs` | 数字孪生可视化 |
| `Form_Oee.cs` | OEE 分析 |
| `Form_VisionAnalysis.cs` | 视觉分析 |
| `Controls/DeviceStatusCard.cs` | 设备状态卡片控件 |
| `Theme/DialogHelper.cs` | 统一消息对话框 |
| `Theme/ThemeExtensions.cs` | 主题扩展方法 |
| `Theme/UiTheme.cs` | UI 主题定义 |
| `Theme/ValidationHelper.cs` | 输入验证辅助 |

#### 操作员/工程师视图模式

`MainForm` 通过 `lblViewMode` 状态栏标签切换模式：

- **操作员模式**：隐藏高级管理节点（系统配置、插件管理、用户管理、远程调试），导航树折叠
- **工程师模式**：显示全部功能节点

`MonitorForm` 同步模式状态，`EngineerOnly` 标记的控件在操作员模式下隐藏。

### 4.2 OmniFrame.Core (业务逻辑层)

核心业务组件：

| 类 | 说明 |
|---|---|
| `SystemManager` | 系统级生命周期管理，协调所有管理器 |
| `PermissionManager` | 权限引擎，基于 UserLevel 的操作鉴权 + 审计日志 |
| `OperationPermissions` | 操作权限常量定义 |
| `AuditLogger` | 安全审计日志记录 |
| `PermissionExtensions` | WinForms 控件权限扩展方法 |
| `AlarmManager` | 报警规则引擎，报警产生/确认/清除生命周期 |
| `AlarmNotification` | 报警通知（声音/弹窗/MQTT） |
| `DataManager` | 生产数据队列缓冲 + 异步持久化 + 统计 |
| `ProductManager` | 产品数据管理 |
| `ConfigManager` | XML 配置文件读写 |
| `ConfigImportExport` | 配置导入导出 |
| `DeviceManager` | 硬件设备管理 |
| `MotionManager` | 运动控制管理器 |
| `IoManager` | IO 点管理 |
| `PlcManager` | PLC 管理器 |
| `CylinderManager` | 气缸控制 |
| `LightManager` | 光源控制 |
| `ErrorCode` | 错误码定义 |
| `OperationLog` | 操作日志 DTO |
| `OperationLogService` | 操作日志服务 (JSON 文件存储) |
| `DigitalTwinBridge` | 数字孪生数据桥接 |
| `AdvancedFeatures/` | 高级功能（OEE/UPH/MQTT/MES/视觉/数据分析） |

### 4.3 DataAccess (数据访问层)

| 类 | 说明 |
|---|---|
| `ProductDb` | 产品数据 SQLite 表操作 |
| `AlarmDb` | 报警数据 SQLite 表操作 |
| `SqliteHelper` | SQLite 通用辅助类 |
| `MySQLHelper` | MySQL 数据库辅助（MES 集成用） |
| `MesClient` | MES HTTP 客户端 |
| `IProductDb` | 产品数据访问接口 |
| `IAlarmDb` | 报警数据访问接口 |

### 4.4 Communication (通信层)

| 类 | 说明 |
|---|---|
| `TcpServer` / `TcpManager` | TCP 服务器 |
| `AsyncTcpClient` / `AsyncTcpServer` | 异步 TCP 通信 |
| `ComLink` / `ComManager` | 串口通信 |
| `OpcLink` / `OpcManager` | OPC 通信 |
| `ConnectionPool` | 连接池管理 |
| `Notify` | 通知机制 |

### 4.5 MotionIO (运动控制层)

| 类 | 说明 |
|---|---|
| `MotionIOManager` | 运动控制总管理器 |
| `Motion` | 运动控制抽象基类 |
| `MotionDevice` | 运动设备抽象 |
| `IoCtrl` / `IoCtrl_PCIeM60` | IO 控制 |
| `Motion_Dmc3000` | 雷赛 DMC3000 控制卡 |
| `Motion_DMC3400` | 雷赛 DMC3400 控制卡 |
| `Motion_GTS` | 固高 GTS 控制卡 |
| `Motion_PCIeM60` | 雷赛 PCIeM60 控制卡 |
| `Motion_InoEcat` | 伊诺斯 EtherCAT 控制卡 |

### 4.6 Plc (PLC 通信)

| 类 | 说明 |
|---|---|
| `Plc_Mitsubishi` | 三菱 PLC |
| `Plc_ModbusTcp` | Modbus TCP |
| `Plc_ModbusRtu` | Modbus RTU |
| `PlcDevice` | PLC 设备抽象基类 |
| `DataBlock` | 数据块定义 |
| `Modbus` | Modbus 协议封装 |
| `PlcMonitor` | PLC 状态监视 |

### 4.7 Common (公共基础库)

| 类 | 说明 |
|---|---|
| `Logger` | 日志记录（文件 + 控制台） |
| `Security` | 加密/解密工具（AES + Base64） |
| `AppServiceLocator` | 应用级服务定位器 |
| `LanguageManager` | 多语言管理 |
| `IniHelper` | INI 文件读写 |
| `XmlHelper` | XML 工具 |
| `CsvOperation` | CSV 导入导出 |
| `FileHelper` | 文件系统工具 |
| `StringHelper` | 字符串工具 |
| `ByteHelper` | 字节操作工具 |
| `TimeHelper` | 时间工具 |
| `EnumHelper` | 枚举工具 |
| `HelpTool` | 帮助工具 |
| `DeviceInterface` | 设备接口定义 |
| `EventArgs` | 自定义事件参数 |

### 4.8 OmniFrame.Sdk (插件 SDK)

| 类 | 说明 |
|---|---|
| `IPlugin` | 插件核心接口：`Initialize()` / `Start()` / `Stop()` / `Shutdown()` |
| `PluginBase` | 插件抽象基类，提供版本、名称、日志、配置模板 |
| `PluginInfo` | 插件元数据（名称、版本、描述、是否官方） |
| `PluginContext` | 插件运行时上下文（日志、配置、事件） |

### 4.9 RemoteMonitor (远程监控)

| 类 | 说明 |
|---|---|
| `RemoteMonitorManager` | 远程监控总管理器 |
| `WebSocketServer` | WebSocket 实时推送 |
| `WebApiController` | HTTP API 控制器 |
| `SecurityAuditAttribute` | API 安全审计装饰器 |

### 4.10 ToolEx (工具扩展)

工具扩展库，提供项目中使用的通用扩展方法和工具类。

---

## 5. 权限体系

### 5.1 角色等级

```
UserLevel.Operator      (值: 10)  — 操作员
UserLevel.Engineer      (值: 20)  — 工程师
UserLevel.Administrator (值: 30)  — 管理员
```

### 5.2 操作权限定义 (OperationPermissions)

| 常量 | 最低角色 | 说明 |
|---|---|---|
| `StartStation` | Operator | 启动工位 |
| `StopStation` | Operator | 停止工位 |
| `ViewStatus` | Operator | 查看运行状态 |
| `ViewAlarmLog` | Operator | 查看报警日志 |
| `ModifyParameters` | Engineer | 修改运行参数 |
| `ClearAlarm` | Engineer | 清除报警 |
| `SingleAxisDebug` | Engineer | 单轴调试 |
| `ViewAllLogs` | Engineer | 查看所有日志 |
| `ModifySystemConfig` | Administrator | 修改系统配置 |
| `ManagePlugins` | Administrator | 管理插件 |
| `ManageUsers` | Administrator | 管理用户 |
| `RemoteDebug` | Administrator | 远程调试 |

### 5.3 权限校验流程

```csharp
// PermissionManager 内部
bool CheckPermission(UserLevel requiredLevel)
{
    var currentUser = _userManager.CurrentUser;
    if (currentUser == null) return false;

    // 审计日志记录 (不论成功失败)
    _auditLogger.LogAccess(currentUser.Name, operationName, ...);

    return currentUser.Level >= requiredLevel;
}
```

### 5.4 UI 级权限

`PermissionExtensions` 提供控件级权限扩展：

- `SetPermission(this Control control, string operationName)` — 绑定操作权限
- 权限不足时控件自动 `Enabled = false`
- 角色切换时自动刷新

---

## 6. 插件系统

### 6.1 架构

```
IPluginManager  (OmniFrame.Core.PluginSystem)
    ├── LoadPlugin(name, version)      — 加载指定版本插件
    ├── UnloadPlugin(name)             — 卸载插件
    ├── GetPlugins()                   — 列出所有可用插件
    ├── GetLoadedPlugins()             — 获取已加载实例
    ├── PluginLoaded / PluginUnloaded  — 生命周期事件
    └── ScanPluginDirectory()          — 扫描插件目录
```

### 6.2 插件基类 (MotionPlugin)

```csharp
public abstract class MotionPlugin : PluginBase
{
    public abstract string DeviceType { get; }       // 设备类型标识
    public abstract bool Connect();                  // 连接设备
    public abstract void Disconnect();               // 断开连接
    public abstract bool Home();                     // 回原点
    public abstract bool MoveAbsolute(double pos);   // 绝对定位
    public abstract bool MoveRelative(double dist);  // 相对定位
    public abstract bool Stop();                     // 停止
}
```

### 6.3 插件测试框架

`PluginTestManager` 提供自动化测试能力：

1. 加载插件
2. 运行 `PluginTestSuite` 中的所有测试用例
3. 生成 HTML 测试报告
4. 卸载插件

---

## 7. 报警管理

### 7.1 报警生命周期

```
产生 (Active) → 确认 (Acknowledged) → 恢复 / 清除 (Cleared)
```

### 7.2 报警等级

| 等级 | 说明 | 颜色 |
|---|---|---|
| `Info` | 提示信息 | 蓝色 |
| `Warning` | 警告 | 黄色 |
| `Error` | 错误 | 橙色 |
| `Critical` | 严重错误 | 红色 |

### 7.3 核心组件

- **AlarmManager**: 报警规则引擎，报警产生/确认/清除生命周期
- **AlarmDb**: SQLite 持久化，支持历史查询
- **AlarmCenterForm**: 实时报警列表 + 历史查询 UI
- **AlarmNotification**: 多通道通知（声音播放、弹窗提示、MQTT 推送）

---

## 8. 数据管理

### 8.1 数据流

```
PLC/设备 → DataManager (内存队列)
                ↓ 异步 (1s 定时刷新)
           ProductDb (SQLite)
                ↓
           统计 / 报表 / 导出 (CSV)
```

### 8.2 生产统计

`ProductionStatistics` 包含：
- 总产量 (TotalCount)
- 良品/不良品计数 (PassCount / FailCount)
- 良率 (PassRate)
- 平均周期 (AverageCycleTime)
- OEE (设备综合效率)

### 8.3 产品记录查询

`DataManager.QueryProducts()` 支持按序列号、时间范围、检测结果筛选，返回 `ProductRecord` 列表。

---

## 9. 构建与部署

### 9.1 构建

```bash
# 还原 → 构建
dotnet restore OmniFrame.sln
dotnet build OmniFrame.sln

# 发布
dotnet publish src/OmniFrame/OmniFrame.csproj -c Release
```

### 9.2 运行环境

- Windows 10/11 或 Windows Server 2019+
- .NET Framework 4.8
- SQLite (System.Data.SQLite 通过 NuGet 自动部署)

### 9.3 配置

系统配置存储在 XML 文件中，`ConfigManager` 负责统一读写。数据文件（SQLite）存储在 `Data/` 目录下。

#### JSON 统一配置（v3.1.0+）

`Config/appsettings.json` 提供产线级运行参数的统一入口，首次运行自动创建默认文件：

```json
{
  "Reconnect": {       // 设备自动重连
    "Enabled": true,
    "MaxBackoffMs": 15000,
    "AlarmThreshold": 3,
    "MaxRetries": 10
  },
  "Watchdog": {        // 看门狗
    "Enabled": true,
    "IntervalMs": 1000,
    "FailureThreshold": 5
  },
  "System": {          // 系统
    "LogLevel": "Info",
    "EnableWatchdog": true,
    "EnableAutoReconnect": true
  }
}
```

### 9.4 环境变量

| 变量 | 必填 | 说明 |
|:---|---|:---|
| `OMNIFRAME_CONFIG_ENCRYPTION_KEY` | ✅ | MES 配置加密密钥 |
| `OMNIFRAME_MES_AES_KEY` | ✅ | MES 通信 AES 密钥 |
| `OMNIFRAME_BARCODE_HOST` | ✅ | 扫码枪主机地址 |
| `OMNIFRAME_BARCODE_PORT` | ✅ | 扫码枪端口 |
| `OMNIFRAME_SIMULATION` | — | `=1` 启用仿真模式（跳过硬件检测） |

### 9.5 崩溃转储

未处理的异常会自动写入 `Logs/CrashDumps/crash_yyyyMMdd_HHmmss.log`，包含异常类型、消息、完整堆栈和内部异常。

---

## 10. 常见任务

### 10.1 添加新窗体

1. 在 `src/OmniFrame/` 下创建窗体文件
2. 如果在 UI 导航树中需要入口，在 `MainForm.LoadTreeview()` 中添加节点
3. 如果需要 DI 注入，在 `Program.cs` 中注册服务
4. 如果包含敏感操作，创建对应的 `OperationPermissions` 常量

### 10.2 添加新设备驱动

1. 继承 `MotionPlugin` (运动控制) 或 `PlcDevice` (PLC)
2. 在 `DeviceManager` 或 `PlcManager` 中注册
3. 插件方式：放置到插件目录，通过 `IPluginManager` 动态加载

### 10.3 添加权限检查

```csharp
// 定义操作权限 (OperationPermissions.cs)
public const string MyOperation = "我的操作";

// 代码中校验
_permissionManager.CheckPermission(UserLevel.Engineer, OperationPermissions.MyOperation);

// 控件级绑定
btnMyControl.SetPermission(OperationPermissions.MyOperation);
```

### 10.4 注册新服务到 DI

```csharp
// Program.cs 中
services.AddSingleton<IMyService, MyService>();

// 窗体通过构造函数注入
public MyForm(IMyService myService) { ... }
```

---

## 11. 产线就绪服务

> v3.1.0 新增 — 面向 7×24 无人值守产线的运行保障体系。

### 11.1 设备自动重连 (`ReconnectionService`)

当 PLC、运动卡、IO 模块因网络抖动或电源波动断连时自动恢复。

```
DeviceDisconnected 事件
        ↓
ReconnectionService 接管
        ↓
指数退避重试: 1s → 2s → 4s → 8s → 15s(上限)
        ↓
  成功 → 恢复正常
  失败×3 → AlarmManager.AddAlarm("RECONNECT_FAILED", Error)
  失败×10 → AlarmManager.AddAlarm("RECONNECT_EXCEEDED", Critical) → 停止自动重连
```

### 11.2 看门狗 (`WatchdogService`)

独立 `Timer` 线程（不受 UI 线程影响），周期调用 `HealthCheckService`。

```
每 1 秒检查所有组件状态
        ↓
  全部健康 → 重置失败计数器
  有组件不健康 → 计数 +1
        ↓
  连续 5 次 → EmergencyStop()
            → 写入诊断快照 Logs/Watchdog/*.log
            → 记录 CPU、内存、线程数、组件状态
```

### 11.3 工站自动恢复 (`RunWithAutoRecoveryAsync`)

每个 BlockCut 工站的 `RunAsync` 自动包裹重试逻辑：

| 异常次数 | 动作 |
|:---:|:---|
| 第 1 次 | 延迟 500ms 自动重试 |
| 第 2 次 | 延迟 1s 自动重试 |
| 第 3 次 | 延迟 2s 自动重试 |
| > 3 次 | 触发 StationErrorOccurred + 暂停，人工介入 |

### 11.4 配置

3 个服务都通过 `appsettings.json` 控制启停和参数，详见 [9.3](#93-配置)。

---

## 更新日志

### v3.1.0 (2026-05-10)

**新增产线就绪特性**:

| 特性 | 文件 | 说明 |
|:---|---|:---|
| 设备自动重连 | `Core/ReconnectionService.cs` | 断连自动恢复，指数退避重试 |
| 系统看门狗 | `Core/WatchdogService.cs` | 1s 心跳，连续失败触发安全停机+诊断快照 |
| 统一配置 | `Core/JsonConfigProvider.cs`（规划中） | appsettings.json 统一入口 |
| 工站自动恢复 | `BlockCutStationBase.cs` | RunWithAutoRecoveryAsync 异常重试 |
| 崩溃转储 | `Program.cs` | 未处理异常写入 Logs/CrashDumps/ |

### v3.0.1 (2026-05-10)

**修复内容**:

| 问题编号 | 描述 | 影响模块 |
|:---|:---|:---|
| OMNI-001 | 插件管理界面 SplitterDistance 错误导致界面打不开 | PluginManagerForm |
| OMNI-002 | 操作日志查询无结果显示 | OperationLogForm |
| OMNI-003 | BlockCut 配置不显示内容 | ConfigForm |
| OMNI-004 | 环境变量缺失导致启动失败 | Program.cs |
| OMNI-005 | BlockCutConfig 仿真模式默认值为 false | BlockCutConfig |

### v3.0.0 (2024-01-15)

- 首个正式发布版本
- BlockCut 工站系统
- MES 集成
- 扫码枪集成
- 数字孪生
- 插件系统

---

> **文档版本**: v4.0.0
