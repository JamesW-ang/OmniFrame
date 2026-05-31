# 🔌 工业 DLL 接入实战教程

> **目标读者：** 刚毕业大学生，没接过第三方 DLL。  
> **前置：** 会写 C# 类和方法。  
> **目标：** 拿到厂商给的 .dll 文件后，能独立接入项目。

---

## 目录

1. [先理解：为什么会有 .dll 文件](#1-先理解为什么会有-dll-文件)
2. [两种 DLL 的区别](#2-两种-dll-的区别)
3. [实战 1：接雷赛运动控制卡 DLL](#3-实战-1接雷赛运动控制卡-dll)
4. [实战 2：接海康工业相机 DLL](#4-实战-2接海康工业相机-dll)
5. [x86 vs x64：新人必踩的坑](#5-x86-vs-x64新人必踩的坑)
6. [如何在项目里使用](#6-如何在项目里使用)
7. [OmniFrame 里的实际例子](#7-omniframe-里的实际例子)
8. [调试技巧](#8-调试技巧)

---

## 1. 先理解：为什么会有 .dll 文件

你写的 C# 代码编译成 `.exe`（程序）或 `.dll`（库）。**厂商给的 .dll 是 C/C++ 写的**，因为：

- 运动控制卡、相机驱动需要直接操作硬件
- C/C++ 可以直接调操作系统内核 API
- 性能要求高（微秒级响应）

C# 不能直接调 C/C++ 的函数。需要一个「翻译层」——这就是 **P/Invoke（平台调用）**。

```
你的 C# 代码
    ↓ 调用
[DllImport] 声明的方法
    ↓ P/Invoke 翻译
厂商的 C/C++ DLL
    ↓ 操作
硬件设备（运动控制卡 / 相机）
```

### 一个最简单的例子

```csharp
using System.Runtime.InteropServices;

// 1. 声明 DLL 里的函数
[DllImport("user32.dll")]
public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

// 2. 调用
MessageBox(IntPtr.Zero, "Hello!", "标题", 0);
```

`user32.dll` 是 Windows 自带的 C DLL。你不需要引用它，P/Invoke 会在系统目录自动找到。

---

## 2. 两种 DLL 的区别

| | 托管 DLL (.NET) | 非托管 DLL (C/C++) |
|:---|:---|:---|
| **编写语言** | C# / VB.NET / F# | C / C++ |
| **如何引用** | 项目引用 或 NuGet | DllImport + 文件复制 |
| **跨平台** | ✅ .NET 通用 | ❌ 绑定 Windows 或特定 CPU |
| **分版本** | 不需要 | **x86 和 x64 必须匹配** |
| **例子** | `Newtonsoft.Json.dll` | `GTS800.dll`（固高运动卡） |

**判断方法：** 双击 .dll 文件 → 如果 Windows 报「不是有效的 .NET 程序集」→ 非托管 DLL。

---

## 3. 实战 1：接雷赛运动控制卡 DLL

### 3.1 雷赛给你什么

```
厂商给的文件:
  DMC3000.dll           ← 核心驱动 DLL（C/C++）
  DMC3000.h             ← C 头文件（声明了所有函数）
  DMC3000.lib           ← 静态链接库（本项目不用）
  DMC3000_Manual.pdf    ← 函数手册
```

### 3.2 从头文件提取函数签名

打开 `.h` 文件，找到你需要的函数：

```c
// DMC3000.h 里的内容（C 语言）
int  __stdcall dmc_board_init();
int  __stdcall dmc_board_close();
int  __stdcall dmc_set_position(int axis, int pos);
int  __stdcall dmc_get_position(int axis, int* pos);
int  __stdcall dmc_t_pmove(int axis, int dist, int start_vel, int speed, int acc, int dec);
```

### 3.3 翻译成 C# 的 DllImport

```csharp
using System.Runtime.InteropServices;

public static class Dmc3000
{
    // ① DLL 文件名（放在 exe 同目录下）
    private const string DllName = "DMC3000.dll";

    // ② 每个 C 函数 → 一个 C# 静态 extern 方法
    [DllImport(DllName)]
    public static extern int dmc_board_init();

    [DllImport(DllName)]
    public static extern int dmc_board_close();

    [DllImport(DllName)]
    public static extern int dmc_set_position(int axis, int pos);

    [DllImport(DllName)]
    public static extern int dmc_get_position(int axis, out int pos);

    [DllImport(DllName)]
    public static extern int dmc_t_pmove(int axis, int dist, int start_vel, int speed, int acc, int dec);
}
```

### 3.4 类型对应表（C → C#）

| C 类型 | C# 类型 | 注意 |
|:---|:---|:---|
| `int` | `int` | 直接对应 |
| `short` | `short` | 直接对应 |
| `double` | `double` | 直接对应 |
| `char*` | `string` 或 `byte[]` | 看是文本还是二进制 |
| `void*` | `IntPtr` | 指针用 IntPtr |
| `int*` | `out int` 或 `ref int` | 输出参数 |
| `void` | `void` | 无返回值 |

### 3.5 在 OmniFrame 里怎么用

```csharp
// MotionIO/Motion_Dmc3000.cs — 我们的封装
public class Motion_Dmc3000 : Motion
{
    public override bool Init()
    {
        int result = Dmc3000.dmc_board_init();  // ← 调 DLL
        if (result != 0)
        {
            LogError($"雷赛初始化失败, 错误码: {result}");
            return false;
        }
        return true;
    }

    public override bool AbsMove(int axisNo, double pos, double speed)
    {
        int result = Dmc3000.dmc_t_pmove(axisNo, (int)(pos * 1000), 100, (int)speed, 5000, 5000);
        return result == 0;
    }
}
```

---

## 4. 实战 2：接海康工业相机 DLL

### 4.1 海康给你什么

```
厂商给的文件:
  MvCameraControl.dll    ← .NET 封装 DLL（海康已经包好了！）
  MvCameraControl.xml    ← XML 注释文件
  各种 C++ DLL            ← 底层驱动（自动加载）
```

海康比较友好——**已经提供了 .NET 版本的 DLL**，不需要自己写 DllImport。

### 4.2 引用方式

```
1. 右键项目 → 添加 → 项目引用 → 浏览
2. 选择 MvCameraControl.dll
3. 现在可以直接 using 了
```

```csharp
using MvCameraControl;

// 创建相机
var camera = new MvCamera();

// 枚举设备
DeviceInfoList devices = new DeviceInfoList();
int ret = MvCamera.MV_CC_EnumDevices(0x01, ref devices);

// 打开相机
ret = camera.MV_CC_CreateDevice(ref devices.DeviceInfoList[0]);
ret = camera.MV_CC_OpenDevice();

// 开始采集
camera.MV_CC_StartGrabbing();
```

### 4.3 海康 vs 雷赛：为什么不一样

| | 雷赛 | 海康 |
|:---|:---|:---|
| DLL 类型 | 纯 C DLL | .NET 封装 DLL |
| 引用方式 | DllImport + 手写声明 | 直接添加引用 |
| 函数名 | C 风格 `dmc_board_init()` | C# 风格 `MV_CC_EnumDevices()` |
| 工作量 | 自己翻译 .h 文件 | 厂商做好了 |

---

## 5. x86 vs x64：新人必踩的坑

### 问题

```csharp
[DllImport("DMC3000.dll")]  // DLL 是 64 位的
```

你的 C# 项目编译成了 **32 位（x86）**。运行时：

```
❌ BadImageFormatException:
   试图加载格式不正确的程序。
```

### 原因

C/C++ DLL 编译时固定了位数。32 位 DLL 只能被 32 位程序加载，64 位同理。

### 解决

```xml
<!-- .csproj 文件 -->
<PropertyGroup>
    <!-- 方案 A: 强制 64 位 -->
    <PlatformTarget>x64</PlatformTarget>

    <!-- 方案 B: 根据 DLL 位数自动选择
         (需要分别准备 x86 和 x64 版本的 DLL) -->
    <PlatformTarget>AnyCPU</PlatformTarget>
    <Prefer32Bit>false</Prefer32Bit>
</PropertyGroup>
```

### 检查 DLL 是 32 还是 64 位

```powershell
# 用 VS 自带的 dumpbin 工具
dumpbin /headers DMC3000.dll | findstr "machine"

# 输出:
#   8664 machine (x64)        ← 64 位
#   14C machine (x86)         ← 32 位
```

或者在 VS 里：文件 → 打开 → 文件 → 选 .dll → 看文件头。

### 常见情况

| 你的程序 | DLL | 结果 |
|:---|:---|:---|
| x86 | x86 | ✅ OK |
| x64 | x64 | ✅ OK |
| **x86** | **x64** | ❌ BadImageFormatException |
| x64 | x86 | ❌ BadImageFormatException |
| AnyCPU | x86 | ⚠️ 64 位系统上可能报错 |

---

## 6. 如何在项目里使用

### 步骤总结

```
① 把 .dll 文件放到项目根目录
   或者放到 src/项目名/Libs/ 子目录

② 在 .csproj 中确保 DLL 被复制到输出目录
   <ItemGroup>
     <Content Include="DMC3000.dll">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </Content>
   </ItemGroup>

③ 写 P/Invoke 声明（如果用纯 C DLL）
   或 添加引用（如果厂商已提供 .NET 封装）

④ 编译 → 确认 bin/Debug/ 下有 .dll 文件

⑤ 运行 → 如果报 BadImageFormatException → 检查 32/64 位
   如果报 DllNotFoundException → 检查 DLL 是否在输出目录
```

### 常见错误速查

| 错误 | 原因 | 解决 |
|:---|:---|:---|
| `DllNotFoundException` | DLL 不在 exe 同目录 | 检查 `CopyToOutputDirectory` |
| `BadImageFormatException` | 32/64 位不匹配 | 统一改为 x64 |
| `EntryPointNotFoundException` | 函数名写错了 | 用 `dumpbin /exports xxx.dll` 查看实际函数名 |
| `SEHException` | DLL 内部崩溃 | 检查传参是否正确，必要时联系厂商 |

---

## 7. OmniFrame 里的实际例子

### 7.1 骨架代码模式

所有运动控制卡品牌都用一个模式：

```
Motion (抽象基类)
  └── Motion_GTS.cs       ← 固高
  └── Motion_Dmc3000.cs   ← 雷赛 DMC3000
  └── Motion_DMC3400.cs   ← 雷赛 DMC3400
  └── Motion_InoEcat.cs   ← 汇川 EtherCAT
  └── Motion_APS.cs       ← APS
```

每个子类都遵循同一个模板：

```csharp
public class Motion_Dmc3000 : Motion
{
    // ① DLL 声明
    [DllImport("DMC3000.dll")]
    private static extern int dmc_board_init();

    // ② Init: 初始化硬件
    public override bool Init()
    {
        int ret = dmc_board_init();
        if (ret != 0) { LogError($"初始化失败: {ret}"); return false; }
        return true;
    }

    // ③ AbsMove: 绝对定位
    public override bool AbsMove(int axisNo, double pos, double speed)
    {
        // 单位转换: mm → 脉冲
        int pulse = (int)(pos * PulsePerMM);
        int ret = dmc_t_pmove(axisNo, pulse, 100, (int)speed, 5000, 5000);
        return ret == 0;
    }
}
```

### 7.2 仿真模式绕过硬件

```csharp
// DiConfigurator.cs
services.AddSingleton<IBlockCutHardware>(sp =>
{
    if (cfg.IsSimulation)
        return new SimulatedHardware(16);  // ← 不加载 DLL
    else
        return new ApsHardware(0);         // ← 加载真实 DLL
});
```

仿真模式不加载任何 DLL。真实硬件模式才加载。这就是为什么新人在自己电脑上也能跑——仿真模式不需要硬件。

---

## 8. 调试技巧

### DLL 到底加载了吗？

```csharp
// 方法 1: 检查文件是否存在
string dllPath = Path.Combine(AppContext.BaseDirectory, "DMC3000.dll");
if (!File.Exists(dllPath))
    Logger.Error($"DLL 未找到: {dllPath}");

// 方法 2: 用 try-catch 捕获具体异常
try
{
    int ret = dmc_board_init();
}
catch (DllNotFoundException ex)
{
    Logger.Error("DLL 文件未找到，请确认 DMC3000.dll 在程序目录下", ex);
}
catch (BadImageFormatException ex)
{
    Logger.Error("DLL 位数不匹配，请检查项目是 x64 还是 x86", ex);
}
catch (SEHException ex)
{
    Logger.Error("DLL 内部异常（通常是传参错误或驱动未安装）", ex);
}
```

### 用 dumpbin 查看 DLL 导出了哪些函数

```powershell
dumpbin /exports DMC3000.dll
# 输出:
#   1    dmc_board_close
#   2    dmc_board_init
#   3    dmc_get_position
#   4    dmc_set_position
#   ...
```

如果 DllImport 里的函数名和这里不一致，就会报 `EntryPointNotFoundException`。

---

## 📚 相关文档

| 主题 | 文档 |
|:---|:---|
| 新人训练营 | [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) |
| WPF 基础 | [WPF_BASICS.md](WPF_BASICS.md) |
| 架构决策 | [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) |
| 运动控制详解 | [motion-control-guide_zh.md](motion-control-guide_zh.md) |
| 通信协议详解 | [communication-guide_zh.md](communication-guide_zh.md) |
