# 术语表（英中对照）

> **新人优先看：** 标 ⭐ 的是入职第 1 周最常见的术语。

---

## 新人必知（⭐ 标）

| 英文术语 | 中文翻译 | 一句话解释 |
|:---|:---|:---|
| **PLC** ⭐ | 可编程逻辑控制器 | 管开关量的小电脑（气缸伸/缩、传送带启/停） |
| **运动控制卡** ⭐ | Motion Controller | 插在电脑里的板卡，精确控制伺服电机（微米级精度） |
| **DI** ⭐ | 数字量输入 | 读传感器状态（有料/没料、到没到位）→ `GetDI()` |
| **DO** ⭐ | 数字量输出 | 控制执行器（开灯、气缸伸缩）→ `SetDO()` |
| **工站 / Station** ⭐ | 生产单元 | 产线上一个独立的加工单元（上料、加工、检测...） |
| **DI（依赖注入）** | Dependency Injection | 不自己 new 对象，让别人传进来。和硬件的 DI 是两码事！ |

---

## 制造领域术语

| 英文术语 | 中文翻译 | 解释 |
|---|---|---|
| **OHS** | 天车上下料站 | Overhead Hoist Station 的缩写。一种自动天车，用于从 CNC 机床装载/卸载工件。 |
| **Station** | 工站 / 工位 | 生产线上的单个制造单元。每个工站对应一个 `StationBase` 子类。 |
| **MES** | 制造执行系统 | Manufacturing Execution System 的缩写。工厂级系统，用于跟踪生产、质量和追溯。OmniFrame 向 MES 上报数据。 |
| **OEE** | 设备综合效率 | Overall Equipment Effectiveness 的缩写。综合衡量设备可用性、性能和质量水平的 KPI 指标。 |
| **UPH** | 每小时产出 | Units Per Hour 的缩写。衡量生产吞吐量的指标。 |
| **Digital Twin** | 数字孪生 | 物理制造单元的虚拟表示，用于模拟和监控。 |
| **Cycle Time** | 周期时间 | 完成一个完整工站循环（Idle → Complete）所需的时间。 |
| **Takt Time** | 节拍时间 | 为满足客户需求所允许的最大周期时间（生产节奏）。 |
| **C/T** | 周期时间 | Cycle Time 的缩写，在代码和日志中常使用此简写。 |

---

## PLC 与工业通信

| 英文术语 | 中文翻译 | 解释 |
|---|---|---|
| **PLC** | 可编程逻辑控制器 | Programmable Logic Controller 的缩写。用于控制机械设备的工业计算机。 |
| **PLC Soft Elements** | PLC 软元件 | PLC 内部的逻辑存储区，通过地址访问。 |
| **X (Input)** | 输入继电器 | PLC 输入继电器。对应接入 PLC 的物理传感器或开关。从 PC 侧只能读取。 |
| **Y (Output)** | 输出继电器 | PLC 输出继电器。对应物理执行器（灯、阀、电机接触器）。可读写。 |
| **M (Internal Relay)** | 中间继电器 | PLC 内部/中间继电器。仅软件层面的标志位，用于逻辑和状态存储。可读写。 |
| **D (Data Register)** | 数据寄存器 | PLC 数据寄存器。16 位整型存储单元。用于存储数值（计数、编码器位置、设定值）。 |
| **Modbus TCP** | Modbus TCP 协议 | 基于 TCP/IP 的工业通信协议，用于读写 PLC 寄存器。默认端口 502。 |
| **Modbus RTU** | Modbus RTU 协议 | 基于串口（RS-232/RS-485）的 Modbus 协议。使用二进制帧格式和 CRC 错误校验。 |
| **OPC DA** | OPC 数据访问 | OLE for Process Control Data Access 的缩写。基于 Windows COM 的工业设备数据实时读写标准。 |
| **OPC UA** | OPC 统一架构 | OPC Unified Architecture 的缩写。OPC DA 的现代化、跨平台继任者。本项目尚未使用。 |
| **EtherCAT** | EtherCAT 协议 | 由 Beckhoff 开发的高速工业以太网协议，用于运动控制。用于伺服驱动和 I/O。 |
| **Register** | 寄存器 | PLC 中单个可寻址的存储单元（Modbus 中为 16 位）。 |
| **Coil** | 线圈 | PLC 中的单比特输出（Modbus 术语）。 |
| **Holding Register** | 保持寄存器 | Modbus 中的 16 位读写寄存器。用于设定值和参数。 |
| **Discrete Input** | 离散输入 | Modbus 中的单比特输入。只读。 |

---

## 硬件

| 英文术语 | 中文翻译 | 解释 |
|---|---|---|
| **Motion Controller** | 运动控制器 | 用于控制伺服电机和步进电机并实现精确定位的专用设备。 |
| **Servo Drive** | 伺服驱动器 | 为伺服电机供电并闭合位置/速度/转矩控制环的放大器。 |
| **I/O Board** | IO 板卡 | 用于将传感器和执行器与 PC 连接的数字量/模拟量输入输出模块。 |
| **Sensor** | 传感器 | 检测物理条件（接近、温度、压力）并输出电信号的设备。 |
| **Actuator** | 执行器 | 执行物理动作的设备（气缸、电机、阀门）。 |

---

## 软件架构术语

| 英文术语 | 中文翻译 | 解释 |
|---|---|---|
| **DI / IoC** | 依赖注入 / 控制反转 | Dependency Injection / Inversion of Control 的缩写。一种设计模式，对象从容器接收其依赖项，而非自行创建。OmniFrame 使用 `Microsoft.Extensions.DependencyInjection`。 |
| **Singleton** | 单例 | 一种 DI 生命周期，在整个应用程序中共享一个实例。用于管理器。 |
| **Transient** | 瞬态 / 每次新建 | 一种 DI 生命周期，每次请求时创建新实例。用于窗体和控件。 |
| **State Machine** | 状态机 | 一种设计模式，对象根据事件在离散状态（步骤）之间转换。每个工站都是一个状态机。 |
| **Repository Pattern** | 仓储模式 | 一种数据访问模式，仓储对象在领域层和数据库之间起中介作用。 |
| **ORM** | 对象关系映射 | Object-Relational Mapper 的缩写。DataAccess 层使用 EF6（Entity Framework 6）。 |

---

## 安全与基础设施

| 英文术语 | 中文翻译 | 解释 |
|---|---|---|
| **JWT** | JSON Web 令牌 | JSON Web Token 的缩写。一种紧凑、URL 安全的令牌，用于在各方之间传递声明。用于端口 8081 的 REST API 认证。 |
| **BCrypt** | BCrypt 加密 | 基于 Blowfish 密码算法的密码哈希函数。用于安全存储用户密码。 |
| **WebSocket** | WebSocket 协议 | 基于单个 TCP 连接的全双工通信协议。用于通过端口 8080 向远程监控端推送实时状态。 |
| **REST API** | RESTful 接口 | Representational State Transfer 的缩写。基于 HTTP 的 API，用于通过端口 8081 查询工站状态、报警和生产数据。 |

---

## 代码中常见缩写

| 缩写 | 全称 | 使用场景 |
|---|---|---|
| `OHS` | Overhead Hoist Station | 工站类名：`OHSLoadingStation`、`OHSUnloadingStation` |
| `IO` | Input/Output | `MotionIO` 项目、`IIoCtrl` 接口 |
| `PLC` | Programmable Logic Controller | `Plc` 项目、`IPlcDevice` 接口 |
| `DI` | Dependency Injection | `Program.cs` 中 DI 容器设置 |
| `Sln` | Solution | `OmniFrame.sln` |
| `Sdk` | Software Development Kit | `OmniFrame.Sdk` 项目 |
| `Ctrl` | Control（controller 的缩写） | `SimulatedIoCtrl`、`IIoCtrl` |
| `Svc` | Service | 偶尔在 WebSocket/服务类名中使用 |
| `Config` | Configuration | `AppConfig`、`config` 变量 |
| `Alm` | Alarm | `AlarmManager`、报警码 |
| `Stn` | Station | 偶尔在旧代码的变量名中使用 |
