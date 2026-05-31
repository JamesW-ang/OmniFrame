# 常见问题排查 FAQ

编译、运行或调试 OmniFrame 时遇到的常见问题及解决方法。

---

## 编译问题

### 编译失败：NuGet 包还原报错

**现象：** `NU1101: Unable to find package ...`，或 `Package ... is not found on source ...`。

**原因：**
- 企业网络屏蔽了 NuGet.org。
- NuGet 缓存已损坏。
- NuGet.config 中的源地址不正确。

**解决方法：**

```powershell
# 1. 检查已配置的源
dotnet nuget list source

# 2. 如果缺少 nuget.org，则添加
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# 3. 清空本地缓存并强制重新还原
dotnet nuget locals all --clear
dotnet restore src/OmniFrame.sln --force
```

如果处于企业代理环境中，请在 `nuget.config` 中添加代理设置：

```xml
<configuration>
  <config>
    <add key="http_proxy" value="http://proxy.company.com:8080" />
    <add key="https_proxy" value="http://proxy.company.com:8080" />
  </config>
</configuration>
```

---

### "IsExternalInit" 编译错误（CS0518）

**现象：** `Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported.`

**原因：** 此 C# 9.0 特性在 .NET Framework 4.8 上需要一个 polyfill。编译器期望运行时中存在 `IsExternalInit`，但 .NET Framework 4.8 并不提供该类型。

**解决方法：** OmniFrame 解决方案在 `Common/IsExternalInit.cs` 中包含了 polyfill。请确认此文件存在：

```csharp
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

如果该文件丢失，请在 `Common` 项目中重新创建。这是来自[微软官方的标准 polyfill](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init)。

---

### 运动控制库"DLL 未找到"

**现象：** `System.DllNotFoundException: Unable to load DLL 'xxx_motion_api.dll'.`

**原因：** 原生运动控制器驱动 DLL 不在应用程序的工作目录或 PATH 中。

**解决方法：**
1. 在仓库根目录的 `lib/` 目录中找到原生 DLL。
2. 将它们复制到输出目录 `src/OmniFrame/bin/Debug/`（或 `Release/`）。
3. 或者，在 `.csproj` 中添加生成后事件：

```xml
<ItemGroup>
  <Content Include="..\..\lib\*.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

4. 清理并重新编译。

---

## 运行时问题

### PLC 连接超时

**现象：** `SocketException: A connection attempt failed`，或工站卡在等待 PLC 信号的 `WaitSignal` 处。

**排查清单：**

1. **从 Windows 机器上 Ping PLC 的 IP 地址：**
   ```powershell
   ping 192.168.1.100
   ```
   如果 Ping 失败，说明物理网络未连通。

2. **验证配置文件中的 IP 和端口**是否与 PLC 的设置一致。Modbus TCP 默认端口为 502。

3. **检查 Windows 防火墙。**它可能阻止了到 PLC 端口的出站 TCP 连接。暂时关闭防火墙进行测试：
   ```powershell
   # 仅用于测试 — 测试后请重新开启
   netsh advfirewall set allprofiles state off
   ```

4. **检查物理网线。**确保以太网线已插入，且两端的链路指示灯在闪烁。

5. **尝试 Telnet 到 PLC 端口：**
   ```powershell
   telnet 192.168.1.100 502
   ```
   如果连接被拒绝，说明 PLC 未在该端口上监听，或者防火墙在阻止连接。

---

### OPC DA 初始化失败

**现象：** 异常信息中提到 `OPCServer`、`IOPCServer`，或 `Class not registered (HRESULT: 0x80040154)`。

**原因：** OPC DA 使用 COM/DCOM 技术。OPC 服务器的 COM 组件必须在 Windows 机器上注册，且 DCOM 权限必须授予访问权限。

**解决方法：**

1. **以管理员身份运行 Visual Studio。**OPC DA 的 COM 注册需要管理员权限。
2. **验证 OPC 服务器**是否已安装在本机上。在"程序和功能"中检查厂家的 OPC 服务器。
3. **注册 COM 组件**（查阅厂家文档确认 DLL 名称）：
   ```powershell
   regsvr32 "C:\Path\To\Vendor\OPCServer.dll"
   ```
4. **通过 `dcomcnfg.exe` 配置 DCOM 权限** — 运行应用程序的用户账户需要具有对 OPC 服务器组件的启动和访问权限。

如果不需要真实的 OPC DA，设置 `UseSimulation = true` 即可完全绕过 OPC。

---

### SQLite 数据库被锁定

**现象：** 写入时出现 `SQLiteException: database is locked`。

**原因：** 另一个进程持有 SQLite 文件的写锁。通常发生在以下情况：
- 另一个 OmniFrame 实例正在运行。
- 数据库浏览工具（DB Browser for SQLite、DBeaver）已打开该文件。
- 上次运行未正常关闭数据库连接。

**解决方法：**
1. 关闭所有其他 OmniFrame 实例。
2. 关闭已打开 `.db` 文件的所有数据库工具。
3. 如果所有实例都已关闭但锁定仍存在，可能是残留的 WAL（预写式日志）或日志文件导致。从数据目录中删除 `database.db-wal` 和 `database.db-shm` 文件，然后重新启动。
4. 如果部署在网络共享上，请将 SQLite 文件移至本地驱动器。SQLite 在 SMB 上无法可靠处理并发访问。

---

### WebSocket 端口已被占用

**现象：** 启动 WebSocket 服务器时出现 `System.Net.Sockets.SocketException: Only one usage of each socket address is normally permitted.`

**原因：** 另一个进程已绑定到端口 8080（WebSocket）或 8081（REST API）。

**解决方法：**

```powershell
# 查找使用端口 8080 的进程
netstat -ano | findstr :8080

# 示例输出：TCP 0.0.0.0:8080 0.0.0.0:0 LISTENING 12345
# 按 PID 终止进程（将 12345 替换为实际 PID）
taskkill /PID 12345 /F
```

如需永久更改端口，编辑配置文件：

```json
{
  "RemoteMonitor": {
    "WebSocketPort": 8080,
    "RestApiPort": 8081
  }
}
```

改为未占用的端口（例如 WebSocket 使用 9090，REST 使用 9091）。

---

### 登录后主窗体不显示

**现象：** 登录成功但主窗体始终不出现，或应用程序静默退出。

**原因：**
- DI 注册中缺少 `MainForm` 或其某个依赖项。
- `MainForm` 的构造函数或 `Load` 事件中存在未处理的异常，导致应用程序崩溃。

**解决方法：**

1. 打开 `Program.cs`，确认 `MainForm` 已注册：
   ```csharp
   services.AddTransient<MainForm>();
   ```

2. 检查 `MainForm` 的构造函数参数是否全部已注册。如果 `MainForm` 需要 `ISystemManager`，则 `SystemManager` 必须以 `ISystemManager` 注册。

3. 以调试模式运行，并启用**在 CLR 异常处中断**：
   - 在 Visual Studio 中：调试 → 窗口 → 异常设置 → 勾选"Common Language Runtime Exceptions"。
   - 这将在抛出异常的精确行处中断，包括构造函数内部。

4. 检查 Serilog 日志文件（通常为 `logs/app-YYYYMMDD.log`），查看崩溃前记录的异常。

---

## 通用调试技巧

| 场景 | 排查方法 |
|---|---|
| 工站卡在某个步骤 | 附加调试器，暂停，检查 `currentStep` 以及是哪个 `WaitSignal` 在阻塞。检查 I/O 或 PLC 是否正在设置预期的信号。 |
| UI 冻结 | 工站的 `ExecuteCycle` 正在 UI 线程上同步运行（而非后台线程）。检查工站循环是否运行在 `Task` 或 `BackgroundWorker` 上。 |
| 内存持续增长 | 查找事件处理程序泄漏——窗体订阅了管理器事件但关闭时未取消订阅。使用内存分析器（Visual Studio 诊断工具）。 |
| COM 口间歇性故障 | USB 转串口适配器可能从设备树中消失。检查设备管理器是否有黄色感叹号。尝试更换 USB 口或适配器。 |
| 配置更改不生效 | 应用程序仅在启动时读取配置。任何配置更改后需重启。检查是否正确读取了配置文件（查看 `bin/Debug/` 中部署的副本）。 |

---

## v3.0.1 新增问题排查 (2026-05-10)

### OMNI-001: 插件管理界面打不开

**现象**: 点击"插件管理"菜单后，界面报错或空白。

**原因**: `SplitContainer` 在创建时设置 `Panel1MinSize=350` 和 `Panel2MinSize=350`，但容器宽度可能为 0，导致 `SplitterDistance` 超出有效范围。

**排查步骤**:
1. 查看应用程序日志中的 `SplitterDistance` 相关错误
2. 检查日志文件位置：`Logs/log-*.txt`

**解决方法**:
- 已修复：将 `Panel1MinSize` 和 `Panel2MinSize` 的设置移到 `HandleCreated` 事件中动态计算
- 文件：`PluginManagerForm.cs`

---

### OMNI-002: 操作日志查询无结果显示

**现象**: 打开操作日志界面后，表格为空，点击查询按钮也无结果。

**原因**:
1. 日期范围默认设置为 7 天，但历史日志数据为 1 个月前
2. `UpdateUserList()` 在 `LoadLogs()` 之后调用，导致 `SelectedItem` 为 null

**排查步骤**:
1. 检查日志文件 `operation_logs.json` 中的数据日期
2. 修改日期范围为更大的区间（如1个月）测试

**解决方法**:
- 已修复：默认日期范围改为 1 个月，调整了初始化顺序
- 文件：`OperationLogForm.cs`

---

### OMNI-003: BlockCut 配置不显示内容

**现象**: 进入配置管理界面，BlockCut 标签页中没有显示任何配置值。

**原因**: `BuildBlockCutTab()` 在 `LoadConfig()` 之后调用，导致控件未准备好就开始设置值。

**排查步骤**:
1. 检查 `ConfigForm` 构造函数的执行顺序
2. 查看日志中是否有配置加载相关记录

**解决方法**:
- 已修复：调整 `InitUI()` 和 `LoadConfig()` 的调用顺序
- 文件：`ConfigForm.cs`

---

### OMNI-004: 环境变量缺失导致启动失败

**现象**: 应用启动时报错，提示 `OMNIFRAME_MES_AES_KEY` 或 `OMNIFRAME_BARCODE_HOST` 未设置。

**原因**: 代码在环境变量未设置时直接抛出异常拒绝启动。

**排查步骤**:
1. 检查系统环境变量是否配置
2. 查看启动日志中的具体错误信息

**解决方法**:
- 已修复：仿真模式下使用默认值，无需配置环境变量
- 文件：`Program.cs`

---

### OMNI-005: BlockCutConfig 仿真模式默认值为 false

**现象**: 即使不配置任何硬件，系统也尝试连接真实设备。

**原因**: `IsSimulation` 属性默认值为 `false`，导致系统默认以生产模式启动。

**解决方法**:
- 已修复：`IsSimulation` 默认值改为 `true`
- 文件：`BlockCutConfig.cs`
