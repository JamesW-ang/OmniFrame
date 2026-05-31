# Glossary (Chinese-English)

This glossary covers the terms and acronyms used throughout the OmniFrame codebase and documentation. It is designed for bilingual (Chinese/English) developers in the industrial automation domain.

---

## Manufacturing Domain Terms

| Term | English Definition | Chinese |
|---|---|---|
| **OHS** | Overhead Hoist Station. An automated overhead crane that loads/unloads parts from a CNC machine. | 天车上下料站 |
| **Station** | A single manufacturing cell in the production line. Each station has a `StationBase` subclass. | 工站 / 工位 |
| **MES** | Manufacturing Execution System. The factory-level system that tracks production, quality, and traceability. OmniFrame reports data to MES. | 制造执行系统 |
| **OEE** | Overall Equipment Effectiveness. A KPI combining availability, performance, and quality. | 设备综合效率 |
| **UPH** | Units Per Hour. The production throughput metric. | 每小时产出 |
| **Digital Twin** | A virtual representation of a physical manufacturing cell, used for simulation and monitoring. | 数字孪生 |
| **Cycle Time** | The time required to complete one full station cycle (Idle → Complete). | 节拍时间 |
| **Takt Time** | The maximum allowed cycle time to meet customer demand. | 节拍时间 (生产节奏) |
| **C/T** | Abbreviation for Cycle Time, commonly used in code and logs. | 周期时间 |

---

## PLC and Industrial Communication

| Term | English Definition | Chinese |
|---|---|---|
| **PLC** | Programmable Logic Controller. The industrial computer that controls machinery. | 可编程逻辑控制器 |
| **PLC Soft Elements** | The logical memory areas inside a PLC, accessed by address. | PLC 软元件 |
| **X (Input)** | PLC input relay. Represents a physical sensor or switch wired to the PLC. Read-only from the PC side. | 输入继电器 |
| **Y (Output)** | PLC output relay. Represents a physical actuator (light, valve, motor contactor). Can be read and written. | 输出继电器 |
| **M (Internal Relay)** | PLC internal/middle relay. A software-only flag used for logic and state. Can be read and written. | 中间继电器 |
| **D (Data Register)** | PLC data register. A 16-bit integer storage location. Used for numeric values (counts, encoder positions, setpoints). | 数据寄存器 |
| **Modbus TCP** | Industrial communication protocol that uses TCP/IP to read/write PLC registers. Default port 502. | Modbus TCP 协议 |
| **Modbus RTU** | Modbus protocol over serial (RS-232/RS-485). Uses binary framing with CRC error checking. | Modbus RTU 协议 |
| **OPC DA** | OLE for Process Control Data Access. A Windows COM-based standard for reading/writing industrial device data in real time. | OPC 数据访问 |
| **OPC UA** | OPC Unified Architecture. The modern, platform-independent successor to OPC DA. Not yet used in this project. | OPC 统一架构 |
| **EtherCAT** | A high-speed industrial Ethernet protocol for motion control, developed by Beckhoff. Used for servo drives and I/O. | EtherCAT 协议 |
| **Register** | A single addressable memory location in a PLC (16-bit for Modbus). | 寄存器 |
| **Coil** | A single bit output in a PLC (Modbus terminology). | 线圈 |
| **Holding Register** | A 16-bit read/write register in Modbus. Used for setpoints and parameters. | 保持寄存器 |
| **Discrete Input** | A single bit input in Modbus. Read-only. | 离散输入 |

---

## Hardware

| Term | English Definition | Chinese |
|---|---|---|
| **Motion Controller** | A specialized device that controls servo motors and stepper motors with precise positioning. | 运动控制器 |
| **Servo Drive** | The amplifier that powers a servo motor and closes the position/velocity/torque control loop. | 伺服驱动器 |
| **I/O Board** | A digital/analog input/output module that interfaces sensors and actuators to the PC. | IO 板卡 |
| **Sensor** | A device that detects physical conditions (proximity, temperature, pressure) and outputs an electrical signal. | 传感器 |
| **Actuator** | A device that performs a physical action (cylinder, motor, valve). | 执行器 |

---

## Software Architecture Terms

| Term | English Definition | Chinese |
|---|---|---|
| **DI / IoC** | Dependency Injection / Inversion of Control. A design pattern where objects receive their dependencies from a container rather than creating them. OmniFrame uses `Microsoft.Extensions.DependencyInjection`. | 依赖注入 / 控制反转 |
| **Singleton** | A DI lifetime where one instance is shared across the entire application. Used for managers. | 单例 |
| **Transient** | A DI lifetime where a new instance is created each time it is requested. Used for forms and controls. | 瞬态 / 每次新建 |
| **State Machine** | A design pattern where an object transitions through discrete states (steps) based on events. Every station is a state machine. | 状态机 |
| **Repository Pattern** | A data access pattern where a repository object mediates between the domain layer and the database. | 仓储模式 |
| **ORM** | Object-Relational Mapper. EF6 (Entity Framework 6) is used in the DataAccess layer. | 对象关系映射 |

---

## Security and Infrastructure

| Term | English Definition | Chinese |
|---|---|---|
| **JWT** | JSON Web Token. A compact, URL-safe token for representing claims between parties. Used for REST API authentication on port 8081. | JSON Web 令牌 |
| **BCrypt** | A password hashing function based on the Blowfish cipher. Used for storing user passwords securely. | BCrypt 加密 |
| **WebSocket** | A full-duplex communication protocol over a single TCP connection. Used for real-time status push to remote monitors on port 8080. | WebSocket 协议 |
| **REST API** | Representational State Transfer. HTTP-based API for querying station status, alarms, and production data on port 8081. | RESTful 接口 |

---

## Common Abbreviations in Code

| Abbreviation | Full Form | Where Used |
|---|---|---|
| `OHS` | Overhead Hoist Station | Station class names: `OHSLoadingStation`, `OHSUnloadingStation` |
| `IO` | Input/Output | `MotionIO` project, `IIoCtrl` interface |
| `PLC` | Programmable Logic Controller | `Plc` project, `IPlcDevice` interface |
| `DI` | Dependency Injection | `Program.cs` DI container setup |
| `Sln` | Solution | `OmniFrame.sln` |
| `Sdk` | Software Development Kit | `OmniFrame.Sdk` project |
| `Ctrl` | Control (short for controller) | `SimulatedIoCtrl`, `IIoCtrl` |
| `Svc` | Service | Occasionally used in WebSocket/service class names |
| `Config` | Configuration | `AppConfig`, `config` variables |
| `Alm` | Alarm | `AlarmManager`, alarm codes |
| `Stn` | Station | Sometimes used in variable names in older code |
