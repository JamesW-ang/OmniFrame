# ⚡ 开发者快速入门指南

> **目标读者：** 刚毕业大学生。学过 C# 基础。不需要懂工业自动化、不需要用过 WPF。
> **预计时间：** 30 分钟从零到看到主界面。

---

## 0. 先搞懂：你在开发什么东西？

见 [WPF_BASICS.md](WPF_BASICS.md) 第 1 章 — WPF 是什么、和 WinForms 的区别、XAML 基础。

工业自动化基础（PLC、运动控制、DI/DO）见本文第 5 章。

---

## 1. 环境准备

| 组件 | 要求 | 说明 |
|:---|:---|:---|
| **操作系统** | Windows 10/11 x64 | WPF 依赖 DirectX，仅限 Windows |
| **.NET Framework 4.8** | Developer Pack | [微软下载](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) |
| **Visual Studio 2022** | Community（免费） | 安装时勾选「.NET 桌面开发」 |
| **Git** | 任意版本 | `git --version` |

---

## 2. 环境变量（仿真模式用）

```powershell
[Environment]::SetEnvironmentVariable("OMNIFRAME_MES_AES_KEY", "DefaultSimulationKey123", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_HOST", "127.0.0.1", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_BARCODE_PORT", "5000", "User")
[Environment]::SetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY", "dev-key-placeholder", "User")
```

---

## 3. 第一次编译运行

```powershell
cd OmniFrame
dotnet restore src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
dotnet build src/OmniFrame.Wpf/OmniFrame.Wpf.csproj
```

> ❓ `restore` 是下载项目依赖的第三方库（NuGet 包）。类似 `npm install`。

VS 中：打开 `src/OmniFrame.Wpf/OmniFrame.Wpf.csproj` → F5。

登录：`admin / admin123`

---

## 4. 项目长什么样

```
src/
├── OmniFrame.Wpf/          ← 🖥️ UI (WPF/XAML) — 你改界面就在这
│   ├── App.xaml(.cs)        ← 程序入口
│   ├── DiConfigurator.cs    ← DI 容器（注册所有服务）
│   ├── Themes/DarkTheme.xaml← 全局样式
│   ├── ViewModels/          ← 数据+命令（15 个 VM）
│   └── Views/               ← XAML 界面（15 个 View）
├── OmniFrame.Core/          ← 🧠 大脑（Manager + 工站 + 接口）
│   └── I*.cs                ← 22 个接口定义
├── Plc/                     ← PLC 通信协议
├── MotionIO/                ← 运动控制卡驱动
├── DataAccess/              ← SQLite/MySQL
├── Communication/           ← TCP/串口/OPC
├── RemoteMonitor/           ← WebSocket + REST API
├── Common/                  ← 工具类（日志/加密）
├── OmniFrame.Simulation/    ← 仿真硬件
└── tests/                   ← 单元测试
```

**你现在只需要关注前两行：** `OmniFrame.Wpf` (界面) 和 `OmniFrame.Core` (逻辑)。

---

## 5. 三个最重要的工业概念

| 概念 | 一句话 | 代码里对应什么 |
|:---|:---|:---|
| **PLC** | 管开关量的小电脑 | `PlcDevice` → `Plc_ModbusTcp` |
| **运动控制卡** | 精确控制伺服电机 | `Motion` → `Motion_GTS` |
| **DI / DO** | 读传感器 / 控制执行器 | `IoCtrl.GetDI()` / `IoCtrl.SetDO()` |

> 📖 术语全部见 [glossary_zh.md](glossary_zh.md)

---

## 6. WPF 基础（必读）

如果你从没用过 WPF，先花 1 小时读 [WPF_BASICS.md](WPF_BASICS.md)。它只讲 OmniFrame 实际用到的概念，每个都有代码示例。

核心记住四个词：**XAML（画界面）→ 绑定（自动同步）→ ViewModel（数据+命令）→ DataTemplate（自动切换界面）**。

---

## 7. 无硬件也能跑

仿真模式：`SimulatedHardware` 假装自己是真实硬件。90% 开发在仿真下完成。

> 📖 [simulation-guide_zh.md](simulation-guide_zh.md)

---

## 接下来怎么学

| 阶段 | 文档 | 时间 |
|:---|:---|:---|
| **Day 0** | [WPF_BASICS.md](WPF_BASICS.md) — 必读 | 1 小时 |
| **Day 1-5** | [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) | 每天 2-3h |
| **Day 3+** | [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) | 反复查 |
| **写代码** | [code-patterns_zh.md](code-patterns_zh.md) | 随手翻 |
| **报错** | [troubleshooting_zh.md](troubleshooting_zh.md) | 随时查 |
