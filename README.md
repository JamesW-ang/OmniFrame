# AOIFrame Lite

<div align="center">

![AOIFrame Logo](https://img.shields.io/badge/AOIFrame-Lite-blue?style=for-the-badge&logo=microsoft)
![Version](https://img.shields.io/badge/Version-1.1.0-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)
![.NET](https://img.shields.io/badge/.NET-4.8+-purple?style=flat-square)
![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=flat-square)

**🏗️ 面向精密制造的工业自动化上位机开发框架**

[文档](#-文档) • [快速开始](#-快速开始) • [示例](#-示例代码) • [API参考](#-api参考) • [贡献](#-贡献)

</div>

---

## 📖 项目概述

AOIFrame Lite 是一个基于 **C# / .NET Framework 4.8** 构建的轻量级工业自动化上位机框架，专为精密制造行业设计。

框架以**四层架构**为核心设计原则，通过**反射桥接**机制实现运动控制卡、PLC等硬件的零编译依赖集成，内置 **WebSocket 数字孪生桥接**，可与 Three.js、Unity 等3D前端无缝对接。

### 🎯 核心优势

| 特性 | 描述 | 优势 |
|------|------|------|
| **四层架构** | UI/业务逻辑/数据访问/通信分离 | 模块化、易维护、低耦合 |
| **反射桥接** | 动态加载硬件驱动DLL | 升级驱动无需重编译主程序 |
| **数字孪生** | WebSocket实时数据推送 | 支持3D可视化和实时监控 |
| **插件化** | 动态加载业务功能插件 | 功能扩展无需修改核心代码 |
| **多协议支持** | ModbusTCP、PLC、OPC、MQTT等 | 支持主流工业通信标准 |
| **高可靠性** | 自动重连、故障恢复、日志审计 | 生产级稳定性 |

---

## 🏗️ 架构设计

```
┌─────────────────────────────────────────────────────┐
│              UI Layer (界面层)                       │
│    WinForms UI / 自定义控件 / 数据展示              │
└────────────────────┬────────────────────────────────┘
                     │ 依赖关系
┌────────────────────▼────────────────────────────────┐
│         Business Logic Layer (业务逻辑层)           │
│    MotionMgr / IoMgr / SystemManager / PluginMgr   │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│         Data Access Layer (数据访问层)              │
│    ConfigManager / IniHelper / AuditLogger          │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│        Communication Layer (通信层)                 │
│  TcpLink/ComLink/OpcLink/PlcLink/DigitalTwinBridge │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│       Hardware Abstraction (硬件抽象层)            │
│  反射机制动态加载: DMC3000.dll / GTS.dll / ...      │
└─────────────────────────────────────────────────────┘
```

### 关键特性解析

#### 🔌 反射桥接机制

```csharp
// 零编译依赖 - 运行时动态加载驱动
Assembly asm = Assembly.LoadFrom("Motion_DMC3000.dll");
Type type = asm.GetType("Motion.DMC3000.Controller");
IMotionDevice device = (IMotionDevice)Activator.CreateInstance(type);
```

**优势：** 升级驱动无需重新编译主程序，只需更新DLL文件

#### 🌐 WebSocket 数字孪生

```csharp
// 实时推送设备状态到Web前端（3D可视化）
var bridge = new DigitalTwinBridge("ws://localhost:3001");
await bridge.ConnectAsync();
bridge.PushAxisPosition(0, 123.45);  // 100ms更新频率
bridge.PushIOStatus("motor", true);   // 500ms更新频率
```

#### 🔌 插件化扩展

```csharp
// 动态加载业务插件，功能扩展无需修改核心代码
PluginManager.Instance.LoadPlugin("MyPlugin.dll");
PluginManager.Instance.ExecutePluginFunction("MyPlugin", "DoWork", parameters);
```

#### 🦪 单例管理器

```csharp
// 全局访问 - 简洁易用
MotionMgr.Instance.MoveAbsolute(0, 100.0);
IoMgr.Instance.WriteDO(0, true);
ConfigManager.Instance.GetConfig("System", "Timeout");
```

---

## 🚀 快速开始

### 前置要求

- Windows 7 SP1 / Windows 10 / Windows 11
- .NET Framework 4.8+
- Visual Studio 2019+ 或 Visual Studio Code

### 安装

#### 方式1：从GitHub克隆

```bash
git clone https://github.com/yourusername/AOIFrame-Lite.git
cd AOIFrame-Lite
dotnet build AOIFrame-Lite.sln
```

#### 方式2：从Release包下载

访问 [Releases](https://github.com/yourusername/AOIFrame-Lite/releases) 页面下载最新版本

### 最小化示例

```csharp
using AOIFrame.Core;

// 初始化
ConfigManager.Instance.Initialize("Config/SystemCfg.xml");
MotionMgr.Instance.Initialize();

// 运动到目标位置
MotionMgr.Instance.SetAxisVelocity(0, 100.0);
MotionMgr.Instance.MoveAbsolute(0, 50.0);

// 等待完成
if (MotionMgr.Instance.WaitMotionDone(0, timeout: 5000))
{
    Console.WriteLine("运动完成");
}
```

更多示例详见 [快速开始指南](docs/getting-started.md)

---

## 📚 文档

| 文档 | 描述 |
|------|------|
| [快速开始](docs/getting-started.md) | 5分钟上手 AOIFrame Lite |
| [架构设计](docs/architecture.md) | 详细的架构解析和设计模式 |
| [API参考](docs/api-reference.md) | 完整的API文档和用法 |
| [安装指南](docs/installation.md) | 系统要求和配置说明 |
| [贡献指南](CONTRIBUTING.md) | 参与项目贡献的方式 |
| [版本更新](CHANGELOG.md) | 版本历史和更新日志 |

---

## 💻 示例代码

### 1️⃣ 基础运动控制

```csharp
// 设置轴运动参数
MotionMgr.Instance.SetAxisVelocity(0, 100.0);   // 速度: 100 mm/s
MotionMgr.Instance.SetAxisAccel(0, 500.0);     // 加速度: 500 mm/s²

// 绝对运动
MotionMgr.Instance.MoveAbsolute(0, 50.0);

// 相对运动
MotionMgr.Instance.MoveRelative(0, 10.0);

// 获取轴状态
double position = MotionMgr.Instance.GetAxisPosition(0);
bool isMoving = MotionMgr.Instance.IsMoving(0);
```

### 2️⃣ IO控制

```csharp
// 读写数字IO
bool diStatus = IoMgr.Instance.ReadDI(0);
IoMgr.Instance.WriteDO(0, true);

// 模拟输入/输出
double aiValue = IoMgr.Instance.ReadAI(0);
IoMgr.Instance.WriteAO(0, 5.0);

// 监听IO变化事件
IoMgr.Instance.OnDIChanged += (index, state) =>
{
    Console.WriteLine($"DI{index} changed to {state}");
};
```

### 3️⃣ PLC通信

```csharp
// ModbusTCP协议
PlcLink plc = new PlcLink("ModbusTCP");
plc.Connect("192.168.1.100", 502);

// 读写寄存器
ushort value = plc.ReadRegister("40001");
plc.WriteRegister("40001", 1000);

// 读写线圈
bool coilValue = plc.ReadCoil("00001");
plc.WriteCoil("00001", true);
```

### 4️⃣ 数字孪生连接

```csharp
// WebSocket连接到3D前端
var bridge = new DigitalTwinBridge("ws://localhost:3001");
await bridge.ConnectAsync();

// 实时推送数据
Task.Run(async () =>
{
    while (true)
    {
        double pos = MotionMgr.Instance.GetAxisPosition(0);
        bridge.PushAxisPosition(0, pos);
        await Task.Delay(100);
    }
});
```

更多示例详见 [examples/](examples/) 目录

---

## 🔌 通信协议支持

AOIFrame Lite 支持主流工业通信协议：

| 协议 | 应用场景 | 支持状态 |
|------|--------|--------|
| **ModbusTCP** | 标准工业协议（以太网） | ✅ 完全支持 |
| **ModbusRTU** | 标准工业协议（串口） | ✅ 完全支持 |
| **三菱MC协议** | 三菱PLC通信 | ✅ 完全支持 |
| **汇川Inovance** | 汇川PLC通信 | ✅ 完全支持 |
| **基恩士HostLink** | 基恩士PLC通信 | ✅ 完全支持 |
| **OPC UA** | 企业级集成 | ✅ 完全支持 |
| **MQTT** | 边缘计算/物联网 | ✅ 完全支持 |
| **WebSocket** | 数字孪生/Web通信 | ✅ 完全支持 |
| **TCP/UDP** | 通用网络通信 | ✅ 完全支持 |
| **串口通信** | 本地设备通信 | ✅ 完全支持 |

---

## 🛠️ API参考

### 运动控制 (MotionMgr)

```csharp
// 初始化与清理
MotionMgr.Instance.Initialize();
MotionMgr.Instance.SoftReset(0);

// 位置操作
double pos = MotionMgr.Instance.GetAxisPosition(0);
MotionMgr.Instance.SetAxisPosition(0, 0);

// 运动参数
MotionMgr.Instance.SetAxisVelocity(0, 100);
MotionMgr.Instance.SetAxisAccel(0, 500);

// 运动控制
MotionMgr.Instance.MoveAbsolute(0, 100);
MotionMgr.Instance.MoveRelative(0, 10);
MotionMgr.Instance.WaitMotionDone(0, 5000);
```

### IO管理 (IoMgr)

```csharp
// 数字IO
bool di = IoMgr.Instance.ReadDI(0);
IoMgr.Instance.WriteDO(0, true);

// 模拟IO
double ai = IoMgr.Instance.ReadAI(0);
IoMgr.Instance.WriteAO(0, 5.0);

// 事件订阅
IoMgr.Instance.OnDIChanged += (idx, state) => { };
```

### 配置管理 (ConfigManager)

```csharp
// 初始化
ConfigManager.Instance.Initialize("Config/SystemCfg.xml");

// 读写配置
string value = ConfigManager.Instance.GetConfig("Section", "Key");
ConfigManager.Instance.SetConfig("Section", "Key", value);

// 保存配置
ConfigManager.Instance.SaveConfig();
```

更多API详见 [API参考文档](docs/api-reference.md)

---

## 🎯 应用案例

### 🤖 精密AOI检测系统

- 控制多轴运动系统（XYZ三轴+旋转轴）
- 采集高分辨率相机图像
- 实时数据分析与异常告警
- WebSocket 3D可视化展示

### 🔧 装配自动化设备

- 机器人运动规划与控制
- 工位IO协调和同步
- 实时数据采集与质量追溯
- 数字孪生远程监控

### 📊 能耗监测平台

- 多台设备状态实时采集
- MQTT边缘计算和本地存储
- 云端大屏可视化展示
- 能效分析与优化建议

### 🏭 MES系统集成

- 与工业PLC无缝对接
- 生产配方管理与下发
- 实时产能监测
- 质量追溯和数据分析

---

## 📊 性能指标

| 指标 | 值 | 说明 |
|------|-----|------|
| 轴控制精度 | ±0.01mm | 取决于硬件 |
| 响应时间 | <50ms | 正常工作条件下 |
| WebSocket延迟 | 100ms | 轴数据更新频率 |
| 同时控制轴数 | 32+ | 可扩展 |
| 支持IO点数 | 128+ | 硬件相关 |
| 最大连接数 | 64 | WebSocket连接 |

---

## 🔒 安全性

- ✅ 代码签名认证
- ✅ 配置文件加密存储（v1.1+）
- ✅ 用户权限管理系统
- ✅ 完整的操作审计日志
- ✅ 通信数据可选加密
- ✅ 异常自动告警和恢复

详见 [安全政策](SECURITY.md)

---

## 🤝 贡献

我们欢迎任何形式的贡献！

### 贡献方式

- 🐛 **报告Bug** - 在 [Issues](https://github.com/yourusername/AOIFrame-Lite/issues) 中提交
- 💡 **功能建议** - 在 [Discussions](https://github.com/yourusername/AOIFrame-Lite/discussions) 讨论
- 💻 **代码贡献** - Fork → 修改 → Pull Request
- 📖 **文档改进** - 完善项目文档

详细步骤见 [贡献指南](CONTRIBUTING.md)

---

## 📄 许可证

本项目采用 [MIT License](LICENSE) 开源许可证。

---

## 📞 技术支持

- 📖 [完整文档](docs/)
- 🐛 [Issue Tracker](https://github.com/yourusername/AOIFrame-Lite/issues)
- 💬 [讨论区](https://github.com/yourusername/AOIFrame-Lite/discussions)
- 📧 联系方式：support@example.com
- 🌐 官网：https://example.com

---

## 🌟 Stars 历史

[![Star History Chart](https://api.github.com/repos/yourusername/AOIFrame-Lite/stargazers)](https://github.com/yourusername/AOIFrame-Lite)

---

<div align="center">

Made with ❤️ by AOIFrame Team

如果这个项目对你有帮助，请给个 ⭐ Star 支持一下！

</div>
