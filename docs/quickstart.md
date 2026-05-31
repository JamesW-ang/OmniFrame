# Developer Quickstart Guide

This guide gets a new developer from zero to running OmniFrame in under 30 minutes.

---

## Prerequisites

| Requirement | Details |
|---|---|
| **Operating System** | Windows 10/11 (x64). OmniFrame is a WinForms app and requires Windows. |
| **.NET SDK** | .NET Framework 4.8 Developer Pack. Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48). |
| **IDE** | Visual Studio 2022 (Community edition is fine). Ensure the **.NET desktop development** workload is installed. |
| **Git** | Any recent version. |
| **NuGet** | NuGet.org must be accessible. If behind a corporate proxy, configure `nuget.config` with your proxy settings. |

---

## First Build

Open a terminal (PowerShell or Command Prompt) and run:

```powershell
# Clone the repository
git clone <repo-url> OmniFrame
cd OmniFrame

# Restore NuGet packages
dotnet restore src/OmniFrame.sln

# Build the solution
dotnet build src/OmniFrame.sln
```

After a successful build, you should see `Build succeeded.` with zero errors and zero warnings.

Alternatively, open `src/OmniFrame.sln` in Visual Studio and press **Ctrl+Shift+B** to build.

---

## First Run

1. Open `src/OmniFrame.sln` in Visual Studio.
2. Right-click the **OmniFrame** project in Solution Explorer and select **Set as Startup Project**.
3. Press **F5** to run with debugging.
4. The login form will appear. Use the default credentials:

   | Field | Value |
   |---|---|
   | **Username** | `admin` |
   | **Password** | `admin123` |

5. After login, the main station overview UI loads. You will see station tiles and a navigation bar.

---

## Project Map

The solution (`OmniFrame.sln`) contains 12 projects. Each has a single responsibility:

| Project | Purpose |
|---|---|
| **OmniFrame** | WinForms UI (forms, controls, user interface). The startup project. |
| **OmniFrame.Core** | Domain/business logic layer: station state machines, managers (SystemManager, AlarmManager, etc.), orchestration. |
| **Common** | Cross-cutting utilities: logging, configuration, data types, constants, extension methods. Shared by every other project. |
| **Communication** | Communication adapters for TCP sockets, serial ports, and OPC DA clients. Wraps raw I/O into typed events. |
| **MotionIO** | Hardware abstraction for motion controllers and I/O boards. Provides a unified interface over vendor-specific drivers. |
| **Plc** | PLC protocol handlers (Modbus TCP/RTU, EtherCAT). Decodes/encodes protocol frames. |
| **DataAccess** | SQLite (local) and MySQL (remote) data access via repository pattern. EF6-based ORM layer. |
| **RemoteMonitor** | WebSocket server (port 8080) and REST API (port 8081) for remote monitoring by an MES or dashboard. |
| **OmniFrame.Sdk** | Public API and base classes for building third-party plugins (IPlugin, StationBase, etc.). |
| **OmniFrame.Simulation** | Simulated hardware devices. Replaces real motion/I/O/PLC drivers so the app runs on a plain dev laptop. |
| **ToolEx** | Extension methods and helper utilities that supplement `Common` with more specialized tooling. |
| **OmniFrame.Tests** | NUnit + Moq test project. 159 tests covering stations, managers, and communication adapters. |

---

## Key Files to Read First

Read these files in this order to understand the architecture:

1. **`src/OmniFrame/Program.cs`** — Entry point. Shows DI container construction and startup flow.

2. **`src/OmniFrame/DiConfigurator.cs`** — ⭐ DI container configuration (all `AddSingleton` / `AddTransient` registrations in one place).

3. **`src/OmniFrame.Core/ISystemManager.cs`** — Interface contract for the central orchestrator. All other managers are accessed through `ISystemManager`. Follows the `Initialize → Start → Stop` lifecycle.
   - Implementation: `src/OmniFrame.Core/SystemManager.cs`

4. **`src/OmniFrame.Core/I*.cs`** — All interface files (22+), browse directly to understand system contracts.

3. **`src/OmniFrame.Sdk/StationBase.cs`** — Base class for every station. Shows the `Step` enum pattern, `SetSignal`/`WaitSignal` API, and the state machine loop. Every station you will work with inherits from this.

4. **`src/OmniFrame.Core/Stations/OHSloading1.cs`** (or similar) — A concrete station implementation. Study one to understand how steps are sequenced, how signals are waited on, and how timeouts are handled.

5. **`src/Common/Logger.cs`** — The Serilog wrapper. Shows `Logger.Info`, `Logger.Warning`, and `Logger.Error` signatures.

---

## Running Without Hardware

If you do not have physical motion controllers, I/O boards, or PLCs connected to your dev machine:

1. Set the configuration to use the **Simulation** layer. The simulation project (`OmniFrame.Simulation`) provides software-only implementations of motion, I/O, and PLC devices.
2. Build and run normally. Simulated devices will respond with deterministic, near-instant responses.
3. This is the normal development workflow — 90% of feature development is done against simulators.

See **simulation-guide.md** for details on configuring and extending the simulation layer.

---

## Next Steps

| Topic | Document |
|---|---|
| Full onboarding path (Day 1-5 + Week 2-3) | [ONBOARDING_GUIDE.md](ONBOARDING_GUIDE.md) |
| Architecture decisions & extension guide | [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) |
| Run against simulated hardware | [simulation-guide.md](simulation-guide.md) |
| Add a new station type | [adding-new-station.md](adding-new-station.md) |
| Coding conventions and patterns | [code-patterns.md](code-patterns.md) |
| Debug common problems | [troubleshooting.md](troubleshooting.md) |
| Terminology and acronyms | [glossary.md](glossary.md) |
