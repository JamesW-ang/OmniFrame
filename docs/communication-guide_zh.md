# OmniFrame 通信层开发指南

## 目录

1. [概述](#1-概述)
2. [TCP 客户端](#2-tcp-客户端)
3. [TCP 服务器](#3-tcp-服务器)
4. [串口通信](#4-串口通信)
5. [OPC DA 通信](#5-opc-da-通信)
6. [连接池](#6-连接池)
7. [PLC 通信](#7-plc-通信)
8. [消息通知](#8-消息通知)
9. [添加新协议](#9-添加新协议)
10. [XML 配置格式](#10-xml-配置格式)
11. [DI 注册总览](#11-di-注册总览)
12. [故障排查](#12-故障排查)

---

## 1. 概述

### 架构图

```
+------------------------------------------------------------------+
|                        OmniFrame 应用层                            |
|  (MainForm / DashboardForm / EquipmentControlForm / ...)          |
+------------------------------------------------------------------+
|                      Core 管理层 (DI 容器)                         |
|  PlcManager | DeviceManager | TaskManager | DataManager | ...     |
+------------------------------------------------------------------+
|                      通信抽象层 (Communication)                     |
|  +------------------+  +---------------+  +--------------------+  |
|  |   TcpLink/TcpManager  |  ComLink/ComManager  |  OpcLink/OpcManager    |  |
|  |   AsyncTcpClient     |               |                    |  |
|  |   AsyncTcpServer     |               |                    |  |
|  |   TcpServer/TcpServerManager |       |                    |  |
|  +------------------+  +---------------+  +--------------------+  |
|  |   ConnectionPool<T>  |  Notify (企业微信/邮件/短信)            |  |
|  +---------------------+  +-----------------------------------+  |
+------------------------------------------------------------------+
|                      PLC 协议层 (Plc)                              |
|  PlcDevice (抽象基类 / 模板方法)  ←── SoftElement (X/Y/M/D)        |
|  +-- Plc_ModbusTcp    (Modbus TCP over TcpLink)                  |
|  +-- Plc_ModbusRtu    (Modbus RTU over ComLink, CRC-16)          |
|  +-- Plc_Mitsubishi   (三菱 MC 协议 over TcpLink)                 |
+------------------------------------------------------------------+
|                       物理/网络层                                   |
|  TCP/IP 网络  |  RS-232/485 串口  |  OPC DA COM 组件               |
+------------------------------------------------------------------+
```

### 我应该用哪种通信方式？

| 场景 | 推荐方案 | 关键类 |
|------|----------|--------|
| 与 PLC 通信 (Modbus TCP) | Modbus TCP 协议 | `Plc_ModbusTcp` + `TcpLink` |
| 与 PLC 通信 (Modbus RTU/串口) | Modbus RTU 协议 | `Plc_ModbusRtu` + `ComLink` |
| 与三菱 PLC 通信 | 三菱 MC 协议 | `Plc_Mitsubishi` + `TcpLink` |
| 与上位机/第三方 TCP 通信 | 原始 TCP 客户端 | `TcpLink` / `AsyncTcpClient` |
| 作为 TCP 服务器接收连接 | TCP 服务器 | `AsyncTcpServer` / `TcpServer` |
| 串口设备 (扫码枪/仪表) | 串口通信 | `ComLink` |
| SCADA/OPC 服务器读写 | OPC DA 客户端 | `OpcLink` / `OpcManager` |
| 高并发短连接 | 连接池 | `ConnectionPool<TcpLink>` |
| 报警/通知推送 | 消息通知 | `Notify` |

### 命名空间

```csharp
using OmniFrame.Communication;  // TcpLink, ComLink, OpcLink, ConnectionPool, Notify
using Plc;                       // PlcDevice, Plc_ModbusTcp, Plc_ModbusRtu, Plc_Mitsubishi, SoftElement
using OmniFrame.Common;          // Logger, ByteHelper, etc.
```

---

## 2. TCP 客户端

### 2.1 TcpLink（同步 TCP 客户端）

`TcpLink` 是同步阻塞式 TCP 客户端，线程安全（`lock(_lock)`），适合简单请求-响应模式。

**构造函数：**

```csharp
public TcpLink(int index, string name, string ip, int port, int timeout = 5000)
```

**完整示例 -- 创建、连接、发送、接收、关闭：**

```csharp
using OmniFrame.Communication;
using OmniFrame.Common;
using System.Text;

public class TcpClientExample
{
    private TcpLink _tcpLink;

    public void Run()
    {
        // 1. 创建 TcpLink 实例
        _tcpLink = new TcpLink(
            index: 1,
            name: "设备A",
            ip: "192.168.1.100",
            port: 502,
            timeout: 3000
        );

        // 2. 订阅状态变化事件
        _tcpLink.StateChangedEvent += OnStateChanged;

        // 3. 打开连接
        if (!_tcpLink.Open())
        {
            Logger.Error("TCP 连接失败");
            return;
        }

        // 4. 发送数据
        byte[] sendData = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x01 };
        if (!_tcpLink.Write(sendData))
        {
            Logger.Error("TCP 发送失败");
            _tcpLink.Close();
            return;
        }

        // 5. 接收数据
        byte[] buffer = new byte[256];
        int readLen = _tcpLink.Read(buffer, 0, buffer.Length);
        if (readLen > 0)
        {
            byte[] receivedData = new byte[readLen];
            Array.Copy(buffer, 0, receivedData, 0, readLen);
            Logger.Info($"收到 {readLen} 字节: {BitConverter.ToString(receivedData)}");
        }
        else if (readLen == 0)
        {
            Logger.Info("暂无可用数据");
        }
        else
        {
            Logger.Error("接收数据失败，连接已断开");
        }

        // 6. 也可以发送文本行
        _tcpLink.WriteLine("HELLO");

        // 7. 读取文本行
        int lineResult = _tcpLink.ReadLine(out string lineData);
        if (lineResult >= 0)
        {
            Logger.Info($"收到行数据: {lineData}");
        }

        // 8. 关闭连接
        _tcpLink.Close();

        // 9. 释放资源
        _tcpLink.Dispose();
    }

    private void OnStateChanged(object sender, bool isConnected)
    {
        var link = sender as TcpLink;
        Logger.Info($"TCP [{link?.Name}] 状态变化: {(isConnected ? "已连接" : "已断开")}");
    }
}
```

### 2.2 TcpManager（TCP 连接管理器）

`TcpManager` 管理多个 `TcpLink` 实例，支持从 XML 加载/保存配置。

```csharp
using OmniFrame.Communication;
using System.Xml;

public class TcpManagerExample
{
    private ITcpManager _tcpManager;

    public TcpManagerExample(ITcpManager tcpManager)  // 通过 DI 注入
    {
        _tcpManager = tcpManager;
    }

    public void Run()
    {
        // 1. 手动添加 TcpLink
        var link1 = new TcpLink(1, "PLC1", "192.168.1.10", 502);
        _tcpManager.AddTcpLink(link1);

        var link2 = new TcpLink(2, "视觉相机", "192.168.1.20", 8000);
        _tcpManager.AddTcpLink(link2);

        // 2. 通过索引获取
        TcpLink plc = _tcpManager.GetTcpLink(1);

        // 3. 通过名称获取
        TcpLink camera = _tcpManager.GetTcpLink("视觉相机");

        // 4. 获取所有连接
        List<TcpLink> allLinks = _tcpManager.GetAllLinks();
        foreach (var link in allLinks)
        {
            Logger.Info($"TCP Link: {link.Name} -> {link.IP}:{link.Port}");
        }

        // 5. 打开所有连接
        foreach (var link in allLinks)
        {
            link.Open();
        }

        // 6. 移除连接（会自动 Close）
        _tcpManager.RemoveTcpLink(2);

        // 7. 清空所有连接
        _tcpManager.Clear();

        // 8. 从 XML 加载配置
        XmlDocument doc = new XmlDocument();
        doc.Load("config.xml");
        _tcpManager.ReadCfgFromXml(doc);

        // 9. 保存配置到 XML
        _tcpManager.SaveCfgToXml(doc);
        doc.Save("config.xml");
    }
}
```

### 2.3 AsyncTcpClient（异步 TCP 客户端）

`AsyncTcpClient` 支持异步连接、自动重连、心跳检测、数据缓冲。

```csharp
using OmniFrame.Communication;
using System.Text;

public class AsyncTcpClientExample
{
    private AsyncTcpClient _client;

    public async Task RunAsync()
    {
        _client = new AsyncTcpClient();

        // 1. 订阅事件
        _client.Connected += (s, e) => Logger.Info("TCP 客户端已连接");
        _client.Disconnected += (s, e) => Logger.Info("TCP 客户端已断开");
        _client.DataReceived += OnDataReceived;
        _client.ErrorOccurred += (s, ex) => Logger.Error($"TCP 错误: {ex.Message}");

        // 2. 异步连接
        bool connected = await _client.ConnectAsync("192.168.1.100", 502);
        if (!connected)
        {
            Logger.Error("连接失败");
            return;
        }

        // 3. 发送数据
        byte[] data = Encoding.UTF8.GetBytes("Hello Server");
        bool sent = await _client.SendDataAsync(data);

        // 4. 如果连接断开，可以触发自动重连（最多 3 次，间隔 5 秒）
        // 注意：ReconnectAsync 需要在连接断开后手动调用
        bool reconnected = await _client.ReconnectAsync();
        if (reconnected)
        {
            Logger.Info("重连成功");
        }

        // 5. 断开连接
        _client.Disconnect();

        // 6. 释放资源
        _client.Dispose();
    }

    private void OnDataReceived(object sender, byte[] data)
    {
        Logger.Info($"收到 {data.Length} 字节: {BitConverter.ToString(data)}");
        // 在此处处理接收到的数据
    }
}
```

---

## 3. TCP 服务器

### 3.1 AsyncTcpServer（异步 TCP 服务器）

`AsyncTcpServer` 支持多客户端并发连接、点对点发送、广播。

```csharp
using OmniFrame.Communication;
using System.Text;

public class AsyncTcpServerExample
{
    private AsyncTcpServer _server;

    public async Task RunAsync()
    {
        // 1. 创建服务器（监听端口 9000）
        _server = new AsyncTcpServer(port: 9000);

        // 2. 订阅事件
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
        _server.DataReceived += OnDataReceived;
        _server.ErrorOccurred += (s, ex) => Logger.Error($"服务器错误: {ex.Message}");

        // 3. 启动服务器
        bool started = await _server.StartAsync();
        if (!started)
        {
            Logger.Error("服务器启动失败");
            return;
        }
        Logger.Info($"服务器已启动，当前连接数: {_server.ConnectionCount}");

        // 4. 广播数据到所有客户端
        byte[] broadcastData = Encoding.UTF8.GetBytes("系统通知: 即将维护");
        int sentCount = await _server.BroadcastAsync(broadcastData);
        Logger.Info($"广播完成: {sentCount} 个客户端收到");

        // 5. 发送到指定客户端
        // （需要从 ClientConnected 事件中获取 clientId）
        string targetClientId = "abc123..."; // 从事件中获取
        byte[] msg = Encoding.UTF8.GetBytes("Hello Client");
        await _server.SendDataAsync(targetClientId, msg);

        // 6. 获取所有客户端信息
        List<TcpClientInfo> allClients = _server.GetAllClients();
        foreach (var client in allClients)
        {
            Logger.Info($"客户端: {client.ClientId}, 连接时间: {client.ConnectTime}, 最后活跃: {client.LastActiveTime}");
        }

        // 7. 停止服务器
        _server.Stop();

        // 8. 释放资源
        _server.Dispose();
    }

    private void OnClientConnected(object sender, TcpClientInfo clientInfo)
    {
        Logger.Info($"客户端已连接: {clientInfo.ClientId}");
    }

    private void OnClientDisconnected(object sender, TcpClientInfo clientInfo)
    {
        Logger.Info($"客户端已断开: {clientInfo.ClientId}");
    }

    private void OnDataReceived(object sender, TcpDataReceivedEventArgs e)
    {
        Logger.Info($"来自客户端 [{e.ClientId}] 的数据: {e.Data.Length} 字节");
        // 在此处理接收数据
    }
}
```

### 3.2 TcpServer / TcpServerManager（同步 TCP 服务器）

```csharp
using OmniFrame.Communication;

public class TcpServerManagerExample
{
    private ITcpServerManager _serverManager;

    public TcpServerManagerExample(ITcpServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    public void Run()
    {
        // 1. 通过管理器添加并启动 TCP 服务器
        _serverManager.AddServer("数据采集服务", port: 8001);
        _serverManager.AddServer("控制指令服务", port: 8002);

        // 2. 获取指定服务器
        TcpServer controlServer = _serverManager.GetServer("控制指令服务");
        if (controlServer != null)
        {
            // 3. 订阅事件
            controlServer.ClientConnected += (s, client) =>
            {
                Logger.Info("新客户端已连接");
            };
            controlServer.DataReceived += (s, data) =>
            {
                Logger.Info($"收到数据: {data.Length} 字节");
            };

            // 4. 发送数据到所有客户端
            byte[] msg = new byte[] { 0x01, 0x02, 0x03 };
            controlServer.SendToAll(msg);
        }

        // 5. 查看所有服务器
        List<string> serverNames = _serverManager.GetServerNames();

        // 6. 停止指定服务器
        _serverManager.RemoveServer("数据采集服务");

        // 7. 停止所有服务器
        _serverManager.StopAll();
    }
}
```

---

## 4. 串口通信

### ComLink

`ComLink` 封装 `System.IO.Ports.SerialPort`，线程安全，支持 RS-232/RS-485。

**构造函数：**

```csharp
public ComLink(int comNo, string name, int baudRate = 9600)
```

**完整示例 -- 配置、打开、发送、接收、关闭：**

```csharp
using OmniFrame.Communication;
using OmniFrame.Common;
using System.IO.Ports;
using System.Text;

public class ComLinkExample
{
    private ComLink _comLink;

    public void Run()
    {
        // 1. 创建 ComLink 实例
        _comLink = new ComLink(
            comNo: 3,           // COM3
            name: "扫码枪",
            baudRate: 115200
        );

        // 2. 配置串口参数（如需要非默认参数）
        _comLink.Parity = Parity.None;       // 无校验
        _comLink.DataBits = 8;               // 8 数据位
        _comLink.StopBits = StopBits.One;    // 1 停止位
        _comLink.ReadTimeout = 3000;         // 读取超时 3 秒
        _comLink.WriteTimeout = 3000;        // 写入超时 3 秒

        // 3. 订阅数据接收事件（异步接收）
        _comLink.DataReceived += OnDataReceived;

        // 4. 打开串口
        if (!_comLink.Open())
        {
            Logger.Error($"打开 COM{_comLink.ComNo} 失败");
            return;
        }

        // 5. 发送字节数组
        byte[] sendData = new byte[] { 0x02, 0x00, 0x03 };
        if (!_comLink.Write(sendData))
        {
            Logger.Error("串口发送失败");
        }

        // 6. 发送字符串行
        _comLink.WriteLine("TRIGGER");

        // 7. 主动读取（同步阻塞）
        byte[] readBuffer = new byte[256];
        int readLen = _comLink.Read(readBuffer, 0, readBuffer.Length);
        if (readLen > 0)
        {
            string result = Encoding.ASCII.GetString(readBuffer, 0, readLen);
            Logger.Info($"串口读到 [{result}]");
        }
        else if (readLen == 0)
        {
            Logger.Info("读取超时，无数据");
        }
        else
        {
            Logger.Error("读取失败");
        }

        // 8. 读取一行
        string line = _comLink.ReadLine();
        if (line != null && line.Length > 0)
        {
            Logger.Info($"读取到行: {line}");
        }

        // 9. 查看缓冲区可用字节数
        int available = _comLink.BytesToRead;
        Logger.Info($"缓冲区可用: {available} 字节");

        // 10. 关闭串口
        _comLink.Close();
    }

    private void OnDataReceived(object sender, byte[] data)
    {
        Logger.Info($"串口 [{((ComLink)sender).Name}] 事件接收: {data.Length} 字节");
        // 在此处理异步接收到的数据
    }
}
```

### ComManager（串口管理器）

```csharp
using OmniFrame.Communication;
using System.Xml;

public class ComManagerExample
{
    private IComManager _comManager;

    public ComManagerExample(IComManager comManager)
    {
        _comManager = comManager;
    }

    public void Run()
    {
        // 1. 手动添加串口
        var comLink = new ComLink(1, "温控仪表", 9600);
        _comManager.AddComLink(comLink);

        // 2. 通过串口号获取
        ComLink link1 = _comManager.GetComLink(1);

        // 3. 通过名称获取
        ComLink link2 = _comManager.GetComLink("温控仪表");

        // 4. 获取所有串口
        List<ComLink> allLinks = _comManager.GetAllLinks();

        // 5. 从 XML 加载
        XmlDocument doc = new XmlDocument();
        doc.Load("config.xml");
        _comManager.ReadCfgFromXml(doc);

        // 6. 保存到 XML
        _comManager.SaveCfgToXml(doc);
        doc.Save("config.xml");

        // 7. 移除并关闭串口
        _comManager.RemoveComLink(1);

        // 8. 清空所有
        _comManager.Clear();
    }
}
```

---

## 5. OPC DA 通信

### 概述

`OpcLink` 封装 OPC DA (Data Access) 客户端，基于 COM 互操作。使用 `OpcInfo` 类描述 OPC 项的数据（Value、Quality、TimeStamp、ClientHandle）。

**前置条件：**
- 目标机器必须安装 OPC 服务器软件
- 需要引用 `OPCAutomation.dll`（OPC 基金会标准 COM 组件）
- 运行前需注册 COM 组件（`regsvr32 OPCAutomation.dll` 或以管理员身份安装）
- OPC DA 依赖 DCOM，需要配置 DCOM 权限（`dcomcnfg`）
- 如果 OPC 服务器在远程机器，两台机器的 Windows 账户需要匹配

### OpcManager 完整示例

```csharp
using OmniFrame.Communication;
using OmniFrame.Common;

public class OpcExample
{
    private IOpcManager _opcManager;

    public OpcExample(IOpcManager opcManager)
    {
        _opcManager = opcManager;
    }

    public void Run()
    {
        // 1. 通过 OpcManager 添加 OPC 连接并自动 Connect
        //    参数: 连接名称, OPC 服务器 ProgID/名称, 主机名(默认 localhost)
        bool connected = _opcManager.AddOpcLink(
            name: "Kepware",
            serverName: "Kepware.KEPServerEx.V6",
            hostName: "localhost"
        );

        if (!connected)
        {
            Logger.Error("OPC 连接失败，请检查: OPC 服务器是否运行、DCOM 配置是否正确");
            return;
        }

        // 2. 获取 OPC 连接
        OpcLink opcLink = _opcManager.GetOpcLink("Kepware");
        if (opcLink == null)
        {
            Logger.Error("获取 OPC 连接失败");
            return;
        }

        // 3. 添加要监控的 OPC 项
        opcLink.AddItem("Channel1.Device1.Tag1");
        opcLink.AddItem("Channel1.Device1.Tag2");

        // 4. 读取 OPC 项的值
        string value1 = opcLink.ReadItem("Channel1.Device1.Tag1");
        Logger.Info($"Tag1 值: {value1}");

        // 5. 写入 OPC 项
        bool writeSuccess = opcLink.WriteItem("Channel1.Device1.Tag1", "123.45");
        if (writeSuccess)
        {
            Logger.Info("写入成功");
        }

        // 6. 列出所有 OPC 连接
        List<string> linkNames = _opcManager.GetOpcLinkNames();
        foreach (var name in linkNames)
        {
            Logger.Info($"OPC 连接: {name}");
        }

        // 7. 移除单个 OPC 连接
        _opcManager.RemoveOpcLink("Kepware");

        // 8. 断开所有 OPC 连接（程序退出时调用）
        _opcManager.DisconnectAll();
    }
}
```

### OpcInfo 数据结构

```csharp
public class OpcInfo
{
    public object OpcItem { get; set; }     // OPC Item 对象引用
    public int ClientHandle { get; set; }    // 客户端句柄
    public string Value { get; set; }        // 当前值
    public int Quality { get; set; }         // 质量码 (192=Good, 0=Bad)
    public string TimeStamp { get; set; }    // 时间戳
}
```

### DCOM 配置要点

OPC DA 远程连接需要在两台机器上进行 DCOM 配置：

1. 运行 `dcomcnfg` -> 组件服务 -> 计算机 -> 我的电脑 -> 属性
2. 默认属性：启用 DCOM，默认身份验证级别设为"无"
3. COM 安全 -> 访问权限/启动和激活权限：添加 `Everyone` 和 `ANONYMOUS LOGON`
4. 找到 OPC 服务器组件 -> 属性 -> 标识：设为"交互式用户"
5. 两台机器必须有相同的用户名和密码（或使用域账户）

---

## 6. 连接池

### ConnectionPool\<T\>

泛型连接池，支持连接复用、健康检查、自动伸缩。

**构造函数：**

```csharp
public ConnectionPool<T>(
    Func<T> connectionFactory,           // 连接工厂方法
    Action<T> connectionReset = null,    // 连接归还时的重置方法
    Func<T, bool> connectionValidator = null,  // 连接有效性验证
    int minPoolSize = 5,                 // 最小连接数
    int maxPoolSize = 20                 // 最大连接数
) where T : class, IDisposable
```

### 完整示例

```csharp
using OmniFrame.Communication;
using OmniFrame.Common;

public class ConnectionPoolExample
{
    private ConnectionPool<TcpLink> _pool;

    public void Run()
    {
        // 1. 创建连接池
        _pool = new ConnectionPool<TcpLink>(
            // 工厂方法：创建新连接
            connectionFactory: () =>
            {
                var link = new TcpLink(0, "PooledLink", "192.168.1.100", 502, 3000);
                link.Open();
                return link;
            },
            // 归还时重置：可在此清理状态
            connectionReset: (link) =>
            {
                // 例如清空缓冲区等
                Logger.Info($"连接 [{link.Name}] 已重置");
            },
            // 有效性验证：归还时检查连接是否仍然可用
            connectionValidator: (link) =>
            {
                return link.IsConnected;
            },
            minPoolSize: 3,
            maxPoolSize: 10
        );

        // 2. 从池中获取连接
        TcpLink link1 = _pool.GetConnection();
        if (link1 != null)
        {
            // 使用连接
            byte[] cmd = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02 };
            link1.Write(cmd);

            byte[] buffer = new byte[256];
            link1.Read(buffer, 0, buffer.Length);

            // 3. 归还连接到池（而非 Dispose）
            _pool.ReturnConnection(link1);
        }

        // 4. 查看连接池状态
        ConnectionPoolStatus status = _pool.GetStatus();
        Logger.Info($"连接池状态: 当前总数={status.CurrentPoolSize}, " +
                    $"可用={status.AvailableConnections}, " +
                    $"范围=[{status.MinPoolSize}-{status.MaxPoolSize}]");

        // 5. 清空连接池（释放所有连接）
        _pool.Clear();
    }
}
```

> **注意：** 本项目中 `GetConnection()` 和 `ReturnConnection()` 方法名就是这两个。如果在某些版本中看到 `Rent()` / `Return()` 方法名，它们是对应的别名。

---

## 7. PLC 通信

### 7.1 架构概述

所有 PLC 驱动继承自抽象基类 `PlcDevice`，使用**模板方法模式**：

- 公开方法（`ReadBit` / `WriteBit` / `ReadWord` / `WriteWord` ...）包含日志记录和计时
- 子类实现 `InternalXxx` 方法完成具体协议操作

```
PlcDevice (抽象基类)
  ├── Plc_ModbusTcp     (Modbus TCP, 基于 TcpLink)
  ├── Plc_ModbusRtu     (Modbus RTU, 基于 ComLink, CRC-16)
  └── Plc_Mitsubishi    (三菱 MC 协议, 基于 TcpLink)
```

### SoftElement 枚举

```csharp
public enum SoftElement
{
    X,  // 输入继电器（PLC 输入点）
    Y,  // 输出继电器（PLC 输出点）
    M,  // 辅助继电器（内部标志位）
    D   // 数据寄存器（16位字）
}
```

### 7.2 Plc_ModbusTcp（Modbus TCP）

基于 `TcpLink` 实现 Modbus TCP 协议。使用内部 `Modbus` 类构建请求帧（MBAP 头 + PDU）。

**构造函数：**

```csharp
public Plc_ModbusTcp(int index, string name, TcpLink tcpLink)
```

**完整示例：**

```csharp
using Plc;
using OmniFrame.Communication;
using OmniFrame.Common;

public class ModbusTcpExample
{
    public void Run()
    {
        // 1. 先创建 TcpLink
        TcpLink tcpLink = new TcpLink(1, "PLC_Station1", "192.168.1.50", 502);

        // 2. 创建 ModbusTCP PLC 设备
        Plc_ModbusTcp plc = new Plc_ModbusTcp(1, "Station1_PLC", tcpLink);

        // 3. 订阅错误事件
        plc.ErrorOccurred += (s, e) =>
        {
            Logger.Error($"PLC 错误: [{e.ErrorCode}] {e.ErrorMessage}");
        };

        // 4. 打开连接（内部会调用 tcpLink.Open()）
        if (!plc.Open())
        {
            Logger.Error("PLC 连接失败");
            return;
        }

        // --- 读取操作 ---

        // 5. 读取位（M100 -- 辅助继电器）
        bool m100 = false;
        if (plc.ReadBit(SoftElement.M, 100, ref m100))
        {
            Logger.Info($"M100 = {m100}");
        }

        // 6. 读取字（D200 -- 数据寄存器）
        ushort d200 = 0;
        if (plc.ReadWord(SoftElement.D, 200, ref d200))
        {
            Logger.Info($"D200 = {d200}");
        }

        // 7. 读取双字（D300 -- 两个连续寄存器组成 32 位）
        uint d300 = 0;
        if (plc.ReadDWord(SoftElement.D, 300, ref d300))
        {
            Logger.Info($"D300 = {d300}");
        }

        // 8. 读取浮点数（D400 -- 两个连续寄存器组成 IEEE 754 单精度浮点）
        float d400 = 0f;
        if (plc.ReadFloat(SoftElement.D, 400, ref d400))
        {
            Logger.Info($"D400 = {d400:F3}");
        }

        // --- 写入操作 ---

        // 9. 写入位
        plc.WriteBit(SoftElement.M, 100, true);     // 置位 M100
        plc.WriteBit(SoftElement.Y, 5, false);      // 复位 Y5

        // 10. 写入字
        plc.WriteWord(SoftElement.D, 200, (ushort)1234);

        // 11. 写入双字
        plc.WriteDWord(SoftElement.D, 300, 999999U);

        // 12. 写入浮点数
        plc.WriteFloat(SoftElement.D, 400, 3.14159f);

        // --- 批量操作 ---

        // 13. 批量读取字
        ushort[] values = new ushort[10];
        if (plc.ReadWords(SoftElement.D, 500, 10, values))
        {
            for (int i = 0; i < values.Length; i++)
            {
                Logger.Info($"D{500 + i} = {values[i]}");
            }
        }

        // 14. 批量写入字
        ushort[] writeValues = new ushort[] { 10, 20, 30, 40, 50 };
        plc.WriteWords(SoftElement.D, 600, 5, writeValues);

        // 15. 关闭连接
        plc.Close();

        // 16. 释放资源
        plc.Dispose();
        tcpLink.Dispose();
    }
}
```

### 7.3 Plc_ModbusRtu（Modbus RTU）

基于 `ComLink` 实现 Modbus RTU 协议，自带 CRC-16 校验。

**初始化流程：** 先调用 `Init(param)` 设置参数，再调用 `Open()`。

```csharp
using Plc;
using OmniFrame.Communication;
using OmniFrame.Common;
using System.Collections.Generic;

public class ModbusRtuExample
{
    public void Run()
    {
        // 1. 创建 Modbus RTU PLC 设备
        Plc_ModbusRtu plc = new Plc_ModbusRtu();

        // 2. 设置通信参数
        var parameters = new Dictionary<string, object>
        {
            { "Port", "COM2" },
            { "BaudRate", 9600 },
            { "DataBits", 8 },
            { "Parity", "None" },     // None / Odd / Even
            { "StopBits", 1 },        // 1 / 2
            { "SlaveId", 1 }          // 从站地址
        };
        plc.Init(parameters);

        // 3. 打开连接（内部创建 ComLink 并 Open）
        if (!plc.Open())
        {
            Logger.Error("Modbus RTU 连接失败");
            return;
        }

        // 4. 读写操作（与 ModbusTCP 相同的接口）
        bool m0 = false;
        plc.ReadBit(SoftElement.M, 0, ref m0);

        ushort d100 = 0;
        plc.ReadWord(SoftElement.D, 100, ref d100);

        plc.WriteBit(SoftElement.M, 0, true);
        plc.WriteWord(SoftElement.D, 100, (ushort)5678);

        // 5. 也可以直接调用底层 Modbus RTU 方法
        bool[] coilValues;
        plc.ReadCoils(address: 0, count: 8, out coilValues);

        ushort[] regValues;
        plc.ReadHoldingRegisters(address: 100, count: 4, out regValues);

        // 6. 关闭
        plc.Close();
        plc.Dispose();
    }
}
```

### 7.4 Plc_Mitsubishi（三菱 MC 协议）

基于 `TcpLink` 实现三菱 MELSEC 通信协议（MC 协议 1E 帧格式）。

**初始化流程：** 先调用 `Initialize(ip, port)` 设置参数，再调用 `Open()`。

```csharp
using Plc;
using OmniFrame.Common;

public class MitsubishiExample
{
    public void Run()
    {
        // 1. 创建三菱 PLC 设备
        Plc_Mitsubishi plc = new Plc_Mitsubishi();

        // 2. 初始化连接参数
        plc.Initialize(ipAddress: "192.168.1.60", port: 6000);

        // 3. 打开连接
        if (!plc.Open())
        {
            Logger.Error("三菱 PLC 连接失败");
            return;
        }

        // 4. 读取位（X/Y/M 软元件）
        bool x0 = false;
        plc.ReadBit(SoftElement.X, 0, ref x0);
        Logger.Info($"X0 = {x0}");

        bool y10 = false;
        plc.ReadBit(SoftElement.Y, 10, ref y10);
        Logger.Info($"Y10 = {y10}");

        // 5. 读取字（D 寄存器）
        ushort d0 = 0;
        plc.ReadWord(SoftElement.D, 0, ref d0);
        Logger.Info($"D0 = {d0}");

        // 6. 读取浮点数
        float fVal = 0f;
        plc.ReadFloat(SoftElement.D, 100, ref fVal);
        Logger.Info($"D100(float) = {fVal}");

        // 7. 写入位
        plc.WriteBit(SoftElement.Y, 10, true);

        // 8. 写入字
        plc.WriteWord(SoftElement.D, 0, (ushort)999);

        // 9. 写入双字和浮点数
        plc.WriteDWord(SoftElement.D, 50, 888888U);
        plc.WriteFloat(SoftElement.D, 100, 2.71828f);

        // 10. 关闭
        plc.Close();
        plc.Dispose();
    }
}
```

### 7.5 DataBlock（数据块批量读取优化）

当需要频繁读取大量连续地址时，使用 `BitBlock` / `WordBlock` 可减少通信次数。

```csharp
using Plc;

public class DataBlockExample
{
    public void BatchRead(PlcDevice plc)
    {
        // 1. 创建位数据块（读取 M100 ~ M199）
        BitBlock bitBlock = new BitBlock(start: 100, count: 100);
        bitBlock.ReadFromPlc(plc, SoftElement.M);

        // 2. 从缓存中读取指定位
        if (bitBlock.Contain(150))
        {
            bool m150 = bitBlock.ReadBit(150);
            Logger.Info($"M150 = {m150}");
        }

        // 3. 创建字数据块（读取 D200 ~ D299）
        WordBlock wordBlock = new WordBlock(start: 200, count: 100);
        wordBlock.ReadFromPlc(plc, SoftElement.D);

        // 4. 从缓存中读取指定字
        if (wordBlock.Contain(250))
        {
            ushort d250 = 0;
            wordBlock.ReadWord(250, ref d250);
            Logger.Info($"D250 = {d250}");
        }

        // 5. 合并相邻地址的读取（IntervalMax 控制最大合并间隔）
        // BitBlock.IntervalMax = 128
        // WordBlock.IntervalMax = 16
        if (wordBlock.AllowMerge(270))
        {
            wordBlock.Merge(270);  // 扩展数据块范围以覆盖新地址
        }
    }
}
```

### 7.6 通过 PlcManager（统一管理）

`OmniFrame.Core.PlcManager` 提供统一的 PLC 操作接口，支持自动类型路由。

```csharp
using OmniFrame.Core;
using Plc;

public class PlcManagerExample
{
    private IPlcManager _plcManager;

    public PlcManagerExample(IPlcManager plcManager)
    {
        _plcManager = plcManager;
    }

    public void Run()
    {
        // 1. 初始化（根据类型名称自动创建对应的 PLC 驱动）
        bool initOk = _plcManager.Initialize(
            plcType: "MITSUBISHI",
            ip: "192.168.1.60",
            port: 6000
        );

        if (!initOk)
        {
            Logger.Error("PLC 初始化失败");
            return;
        }

        // 2. 写入（自动根据值类型选择 WriteBit/WriteWord/WriteDWord/WriteFloat）
        _plcManager.Write(SoftElement.M, 100, true);           // bool -> WriteBit
        _plcManager.Write(SoftElement.D, 200, (ushort)12345);  // ushort -> WriteWord
        _plcManager.Write(SoftElement.D, 300, 888888U);        // uint -> WriteDWord
        _plcManager.Write(SoftElement.D, 400, 3.14f);          // float -> WriteFloat

        // 3. 读取（指定返回类型）
        object val1 = _plcManager.Read(SoftElement.M, 100, typeof(bool));     // -> bool
        object val2 = _plcManager.Read(SoftElement.D, 200, typeof(ushort));   // -> ushort
        object val3 = _plcManager.Read(SoftElement.D, 300, typeof(uint));     // -> uint
        object val4 = _plcManager.Read(SoftElement.D, 400, typeof(float));    // -> float

        // 4. 检查连接状态
        if (_plcManager.IsConnected)
        {
            Logger.Info("PLC 已连接");
        }

        // 5. 断开
        _plcManager.Disconnect();
    }
}
```

### 7.7 PlcMonitor（PLC 监控器）

位于 `src/Plc/PlcMonitor.cs`，提供持续监控 PLC 状态的能力。

```csharp
using Plc;

public class PlcMonitorExample
{
    public void Setup(PlcDevice plc)
    {
        PlcMonitor monitor = new PlcMonitor(plc);

        // 监控特定位
        monitor.AddBitMonitor(SoftElement.M, 100);  // 监控 M100
        monitor.AddBitMonitor(SoftElement.Y, 0);    // 监控 Y0

        // 监控特定寄存器
        monitor.AddWordMonitor(SoftElement.D, 200); // 监控 D200
        monitor.AddWordMonitor(SoftElement.D, 300); // 监控 D300

        // 设置监控间隔
        monitor.Interval = 100;  // 100ms

        // 值变化事件
        monitor.ValueChanged += (s, e) =>
        {
            // e.Element, e.Address, e.OldValue, e.NewValue
            Logger.Info($"PLC 值变化: {e.Element}{e.Address} = {e.NewValue}");
        };

        // 启动监控
        monitor.Start();

        // ... 运行期间 ...

        // 停止监控
        monitor.Stop();
    }
}
```

---

## 8. 消息通知

`Notify` 支持企业微信、邮件、SMS 等通知渠道，提供统一的通知接口。

```csharp
using OmniFrame.Communication;

public class NotifyExample
{
    private INotify _notify;

    public NotifyExample(INotify notify)
    {
        _notify = notify;
    }

    public void Run()
    {
        // 1. 订阅通知事件
        _notify.NotificationOccurred += (s, info) =>
        {
            Logger.Info($"[{info.Type}] {info.Source} - {info.Title}: {info.Content}");
            // 在此可自定义处理（如弹窗、语音播报等）
        };

        // 2. 发送各类通知
        _notify.SendInfo("设备就绪", "PLC1 已成功连接", "通信模块");
        _notify.SendWarning("温度偏高", "加热区温度达到 80度", "温控模块");
        _notify.SendError("通信断开", "PLC2 连接超时", "PLC管理");
        _notify.SendSuccess("任务完成", "批次 B2024-001 加工完成", "生产管理");

        // 3. 通用发送方法
        _notify.SendNotification(
            title: "自定义通知",
            content: "这是一条自定义通知内容",
            type: NotifyType.Info,
            source: "自定义模块"
        );

        // 4. 查看历史通知
        List<NotifyInfo> history = _notify.GetNotifications();
        foreach (var n in history)
        {
            Logger.Info($"[{n.Time:HH:mm:ss}] {n.Title}: {n.Content}");
        }

        // 5. 设置最大保留条数
        _notify.SetMaxNotifications(500);

        // 6. 清空历史
        _notify.ClearNotifications();
    }
}
```

---

## 9. 添加新协议

假设我们要添加一个 **西门子 S7 协议** 的 PLC 驱动，步骤如下：

### 第 1 步：创建协议类

在 `src/Plc/` 下创建 `Plc_SiemensS7.cs`：

```csharp
using System;
using System.Net.Sockets;
using OmniFrame.Communication;
using OmniFrame.Common;

namespace Plc
{
    /// <summary>
    /// 西门子 S7 协议 PLC 驱动
    /// </summary>
    public class Plc_SiemensS7 : PlcDevice
    {
        private TcpLink _tcpLink;
        private string _ip;
        private int _port;
        private short _rack;
        private short _slot;

        public Plc_SiemensS7(int index, string name, string ip, int port = 102, short rack = 0, short slot = 2)
            : base(index, name)
        {
            _ip = ip;
            _port = port;
            _rack = rack;
            _slot = slot;
        }

        public override bool Open()
        {
            try
            {
                if (IsOpen) return true;

                _tcpLink = new TcpLink(Index, Name, _ip, _port);
                if (!_tcpLink.Open())
                {
                    LogError("S7 TCP 连接失败");
                    return false;
                }

                // S7 协议特有的连接建立（COTP + S7 Setup Communication）
                if (!S7ConnectSetup())
                {
                    _tcpLink.Close();
                    return false;
                }

                IsOpen = true;
                LogInfo($"S7 PLC 连接成功 ({_ip}:{_port})");
                return true;
            }
            catch (Exception ex)
            {
                LogError("S7 连接异常", ex);
                return false;
            }
        }

        public override void Close()
        {
            IsOpen = false;
            _tcpLink?.Close();
            LogInfo("S7 PLC 已断开");
        }

        // --- 实现抽象方法 ---

        protected override bool InternalReadBit(SoftElement element, int addr, ref bool bVal)
        {
            // 1. 将 SoftElement + addr 映射到 S7 地址（DB/位偏移）
            (int dbNumber, int byteOffset, int bitOffset) = MapToS7Address(element, addr);

            // 2. 构建 S7 Read 请求
            byte[] request = BuildS7ReadRequest(dbNumber, byteOffset, 1);

            // 3. 发送并接收
            if (!_tcpLink.Write(request)) return false;

            byte[] buffer = new byte[256];
            int len = _tcpLink.Read(buffer, 0, buffer.Length);
            if (len < 0) return false;

            // 4. 解析响应
            bVal = ((buffer[25] >> bitOffset) & 0x01) == 1;
            return true;
        }

        protected override bool InternalReadWord(SoftElement element, int addr, ref ushort nVal)
        {
            (int dbNumber, int byteOffset, _) = MapToS7Address(element, addr);

            byte[] request = BuildS7ReadRequest(dbNumber, byteOffset, 2);
            if (!_tcpLink.Write(request)) return false;

            byte[] buffer = new byte[256];
            int len = _tcpLink.Read(buffer, 0, buffer.Length);
            if (len < 0) return false;

            nVal = (ushort)((buffer[25] << 8) | buffer[26]);
            return true;
        }

        protected override bool InternalReadDWord(SoftElement element, int addr, ref uint nVal)
        {
            (int dbNumber, int byteOffset, _) = MapToS7Address(element, addr);

            byte[] request = BuildS7ReadRequest(dbNumber, byteOffset, 4);
            if (!_tcpLink.Write(request)) return false;

            byte[] buffer = new byte[256];
            int len = _tcpLink.Read(buffer, 0, buffer.Length);
            if (len < 0) return false;

            nVal = (uint)((buffer[25] << 24) | (buffer[26] << 16) | (buffer[27] << 8) | buffer[28]);
            return true;
        }

        protected override bool InternalReadFloat(SoftElement element, int addr, ref float fVal)
        {
            uint raw = 0;
            if (!InternalReadDWord(element, addr, ref raw)) return false;
            fVal = BitConverter.ToSingle(BitConverter.GetBytes(raw), 0);
            return true;
        }

        protected override bool InternalWriteBit(SoftElement element, int addr, bool bVal)
        {
            (int dbNumber, int byteOffset, int bitOffset) = MapToS7Address(element, addr);

            // 先读取当前字节
            byte[] readReq = BuildS7ReadRequest(dbNumber, byteOffset, 1);
            _tcpLink.Write(readReq);
            byte[] readBuf = new byte[256];
            _tcpLink.Read(readBuf, 0, readBuf.Length);

            // 修改目标位
            byte currentByte = readBuf[25];
            if (bVal)
                currentByte |= (byte)(1 << bitOffset);
            else
                currentByte &= (byte)~(1 << bitOffset);

            // 写回
            byte[] writeReq = BuildS7WriteRequest(dbNumber, byteOffset, new byte[] { currentByte });
            _tcpLink.Write(writeReq);
            byte[] writeBuf = new byte[256];
            return _tcpLink.Read(writeBuf, 0, writeBuf.Length) > 0;
        }

        protected override bool InternalWriteWord(SoftElement element, int addr, ushort nVal)
        {
            (int dbNumber, int byteOffset, _) = MapToS7Address(element, addr);
            byte[] data = new byte[] { (byte)(nVal >> 8), (byte)nVal };
            byte[] request = BuildS7WriteRequest(dbNumber, byteOffset, data);
            _tcpLink.Write(request);
            byte[] buffer = new byte[256];
            return _tcpLink.Read(buffer, 0, buffer.Length) > 0;
        }

        protected override bool InternalWriteDWord(SoftElement element, int addr, uint nVal)
        {
            (int dbNumber, int byteOffset, _) = MapToS7Address(element, addr);
            byte[] data = new byte[] {
                (byte)(nVal >> 24), (byte)(nVal >> 16),
                (byte)(nVal >> 8),  (byte)nVal
            };
            byte[] request = BuildS7WriteRequest(dbNumber, byteOffset, data);
            _tcpLink.Write(request);
            byte[] buffer = new byte[256];
            return _tcpLink.Read(buffer, 0, buffer.Length) > 0;
        }

        protected override bool InternalWriteFloat(SoftElement element, int addr, float fVal)
        {
            byte[] bytes = BitConverter.GetBytes(fVal);
            uint raw = BitConverter.ToUInt32(bytes, 0);
            return InternalWriteDWord(element, addr, raw);
        }

        // --- S7 协议私有方法 ---

        private bool S7ConnectSetup()
        {
            // COTP 连接请求 + S7 Setup Communication PDU
            byte[] cotpRequest = new byte[] {
                0x03, 0x00, 0x00, 0x16, 0x11, 0xE0, 0x00, 0x00,
                0x00, 0x00, 0x00, 0xC0, 0x01, 0x0A, 0xC1, 0x02,
                0x01, 0x00, 0xC2, 0x02, 0x01, 0x02
            };
            _tcpLink.Write(cotpRequest);
            byte[] resp = new byte[256];
            int len = _tcpLink.Read(resp, 0, resp.Length);
            return len > 0;
        }

        private (int dbNumber, int byteOffset, int bitOffset) MapToS7Address(SoftElement element, int addr)
        {
            switch (element)
            {
                case SoftElement.M:  return (1, addr, 0);      // DB1 = M 区
                case SoftElement.D:  return (10, addr * 2, 0); // DB10 = D 区，每个字 2 字节
                case SoftElement.X:  return (2, addr / 8, addr % 8);
                case SoftElement.Y:  return (3, addr / 8, addr % 8);
                default:             return (1, addr, 0);
            }
        }

        private byte[] BuildS7ReadRequest(int dbNumber, int byteOffset, int length)
        {
            // 构建完整 S7 Read 请求（TPKT + COTP + S7 Header + S7 Read Item）
            byte[] request = new byte[31];
            // ... 填充请求字节（此处省略详细字节填充）
            return request;
        }

        private byte[] BuildS7WriteRequest(int dbNumber, int byteOffset, byte[] data)
        {
            // 构建完整 S7 Write 请求
            byte[] request = new byte[35 + data.Length];
            // ... 填充请求字节（此处省略详细字节填充）
            return request;
        }
    }
}
```

### 第 2 步：在 PlcManager 中注册新类型

编辑 `src/OmniFrame.Core/PlcManager.cs`，在 `switch` 中添加新 case：

```csharp
case "SIEMENSS7":
    var siemensPlc = new Plc_SiemensS7(0, name, ip, port);
    siemensPlc.Initialize(ip, port);
    _plc = siemensPlc;
    break;
```

### 第 3 步：在 DI 容器中注册（如需要独立管理）

编辑 `src/OmniFrame/Program.cs` 的 `ConfigureServices` 方法：

```csharp
services.AddSingleton<IPlcManager, PlcManager>();
```

（`PlcManager` 通常已注册，只需在内部 switch 中添加即可）

### 第 4 步：创建新协议的管理器（可选）

如果新协议需要独立的管理器（类似 `TcpManager` / `ComManager`），在 `src/Communication/` 下创建：

```csharp
public interface IS7Manager
{
    void AddDevice(Plc_SiemensS7 device);
    Plc_SiemensS7 GetDevice(string name);
    void RemoveDevice(string name);
    void DisconnectAll();
}

public class S7Manager : IS7Manager
{
    private Dictionary<string, Plc_SiemensS7> _devices = new Dictionary<string, Plc_SiemensS7>();
    // ... 实现各方法 ...
}
```

然后在 `Program.cs` 注册：

```csharp
services.AddSingleton<IS7Manager, S7Manager>();
```

---

## 10. XML 配置格式

`TcpManager` 和 `ComManager` 支持从 XML 读取/保存配置。

### TCP 配置 (`TcpLink`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <TcpLinks>
    <!-- Index: 唯一编号  Name: 连接名称  IP: IP地址  Port: 端口  Timeout: 超时(毫秒) -->
    <TcpLink Index="1" Name="PLC1" IP="192.168.1.10" Port="502" Timeout="3000" />
    <TcpLink Index="2" Name="视觉相机" IP="192.168.1.20" Port="8000" Timeout="5000" />
    <TcpLink Index="3" Name="MES服务器" IP="10.0.0.100" Port="9000" Timeout="10000" />
  </TcpLinks>
</Configuration>
```

### 串口配置 (`ComLink`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <ComLinks>
    <!-- ComNo: 串口号  Name: 名称  BaudRate: 波特率
         Parity: 0=None, 1=Odd, 2=Even  DataBits: 数据位
         StopBits: 1=One, 2=Two   ReadTimeout/WriteTimeout: 超时(毫秒) -->
    <ComLink ComNo="1" Name="温控仪表" BaudRate="9600" Parity="0" DataBits="8"
             StopBits="1" ReadTimeout="3000" WriteTimeout="3000" />
    <ComLink ComNo="2" Name="扫码枪" BaudRate="115200" Parity="0" DataBits="8"
             StopBits="1" ReadTimeout="2000" WriteTimeout="2000" />
  </ComLinks>
</Configuration>
```

### 加载配置示例

```csharp
using System.Xml;
using OmniFrame.Communication;

// 加载 TCP 配置
XmlDocument doc = new XmlDocument();
doc.Load("communication_config.xml");

var tcpManager = serviceProvider.GetRequiredService<ITcpManager>();
tcpManager.ReadCfgFromXml(doc);

var comManager = serviceProvider.GetRequiredService<IComManager>();
comManager.ReadCfgFromXml(doc);

// 打开所有已加载的连接
foreach (var link in tcpManager.GetAllLinks())
{
    link.Open();
}
foreach (var link in comManager.GetAllLinks())
{
    link.Open();
}
```

---

## 11. DI 注册总览

以下是 `Program.cs` 中通信层的完整 DI 注册（`ConfigureServices` 方法）：

```csharp
// ===== OmniFrame.Communication =====
services.AddSingleton<IComManager, ComManager>();              // 串口管理器
services.AddSingleton<ITcpManager, TcpManager>();              // TCP 客户端管理器
services.AddSingleton<IOpcManager, OpcManager>();              // OPC DA 管理器
services.AddSingleton<ITcpServerManager, TcpServerManager>();  // TCP 服务器管理器
services.AddSingleton<INotify, Notify>();                      // 消息通知

// ===== OmniFrame.Core (PLC 等) =====
services.AddSingleton<IPlcManager, PlcManager>();              // PLC 统一管理
services.AddSingleton<IDeviceManager, DeviceManager>();        // 设备管理器
```

在业务代码中通过构造函数注入使用：

```csharp
public class MyService
{
    private readonly ITcpManager _tcpManager;
    private readonly IComManager _comManager;
    private readonly IPlcManager _plcManager;

    public MyService(ITcpManager tcpManager, IComManager comManager, IPlcManager plcManager)
    {
        _tcpManager = tcpManager;
        _comManager = comManager;
        _plcManager = plcManager;
    }
}
```

也可以通过 `ServiceProviderCache` 全局获取：

```csharp
var tcpManager = ServiceProviderCache.Get<ITcpManager>();
var comManager = ServiceProviderCache.Get<IComManager>();
```

---

## 12. 故障排查

### 12.1 TCP 连接超时

**现象：** `TcpLink.Open()` 返回 `false`，日志显示"TCP连接超时"。

**排查步骤：**

1. 确认目标 IP 和端口正确：`ping <IP>`、`telnet <IP> <Port>`
2. 检查 Windows 防火墙是否拦截
   - 控制面板 -> Windows Defender 防火墙 -> 允许应用或功能 -> 添加入站规则
3. 检查 `Timeout` 参数是否过短（默认 5000ms），对于远程慢速网络可调大到 10000ms
4. 如果连 PLC，确认 PLC 的 Modbus TCP 服务已启用且端口正确（默认 502）

```csharp
// 调大超时
var link = new TcpLink(1, "PLC", "192.168.1.10", 502, timeout: 10000);
```

### 12.2 串口访问被拒绝 (UnauthorizedAccessException)

**现象：** `ComLink.Open()` 抛出 `UnauthorizedAccessException`。

**排查步骤：**

1. 检查串口号是否正确（`COM1` / `COM2` / ...）
2. 检查是否有其他程序（如串口调试助手、PLC 编程软件）占用了该串口
3. 以管理员权限运行程序
4. 如果是 USB 转串口，检查驱动是否安装正确

```csharp
// 列出系统可用串口
string[] ports = System.IO.Ports.SerialPort.GetPortNames();
foreach (var port in ports)
{
    Logger.Info($"可用串口: {port}");
}
```

### 12.3 OPC COM 注册问题

**现象：** OPC 连接失败，报 COM 相关异常。

**排查步骤：**

1. 确认 `OPCAutomation.dll` 已注册（`regsvr32 OPCAutomation.dll`）
2. 确认 OPC 服务器在运行（如 Kepware、RSLinx 等）
3. 运行 `dcomcnfg` 检查 DCOM 权限配置
4. 在 64 位系统上，注意 32 位 COM 组件需要注册到 `C:\Windows\SysWOW64\` 下的 `regsvr32`
5. 如果目标 OPC 服务器在远程，确保两台机器账户匹配（相同用户名/密码）
6. 防火墙允许 DCOM 端口（135）和 OPC 动态端口

### 12.4 Modbus CRC 校验错误

**现象：** Modbus RTU 通信不稳定，偶尔返回 CRC 校验错误。

**排查步骤：**

1. 检查串口参数配置与从站是否匹配：波特率、数据位、停止位、校验位
2. 检查 RS-485 接线方式（A+/B-）是否正确，终端电阻是否已加
3. 增加 `Thread.Sleep` 延迟时间（在 `Plc_ModbusRtu.cs` 中默认 `Thread.Sleep(100)`，可适当增大）
4. 检查从站地址（`SlaveId`）是否正确

### 12.5 PLC Read 返回 false 但连接正常

**现象：** `plc.IsConnected` 为 `true`，但 `ReadBit` / `ReadWord` 返回 `false`。

**排查步骤：**

1. 确认地址是否正确（Modbus 地址通常从 0 开始，而不是 1）
2. 检查 `SoftElement` 映射：`X`/`Y`/`M`/`D` 分别映射到不同 Modbus 地址偏移
3. 检查 PLC 中该地址是否确实存在（有些 PLC 内存区域有范围限制）
4. 用 Modbus 调试工具（如 Modbus Poll）验证地址是否可读写

### 12.6 AsyncTcpClient 频繁断开重连

**现象：** `AsyncTcpClient` 日志显示频繁的"TCP客户端连接已断开"和"尝试重连"。

**排查步骤：**

1. 检查心跳间隔（默认 30 秒），如果服务端无响应可能导致误断
2. 调整重连参数
3. 检查网络稳定性

```csharp
// AsyncTcpClient 的重连和心跳参数是通过私有字段设置的，
// 如需修改，可扩展 AsyncTcpClient 或使用 TcpLink 代替
```

### 12.7 日志分析

所有通信模块的错误都会通过 `Logger` 输出到 `Logs/` 目录。关键日志标记：

| 日志内容 | 含义 |
|----------|------|
| `TCP连接成功: 设备名 (IP:Port)` | TcpLink 连接建立 |
| `TCP连接超时: 设备名 (IP:Port)` | TcpLink 连接超时 |
| `串口打开成功: 名称 (COMx, 波特率)` | ComLink 打开成功 |
| `串口打开失败(权限不足): 名称 (COMx)` | 串口被占用或无权限 |
| `OPC服务器连接成功` | OPC 连接建立 |
| `TCP客户端连接已断开` | AsyncTcpClient 连接丢失 |
| `[PLC名称] 读取位 X0: 值=True, 结果=True, 耗时=5.23ms` | PLC 读取操作日志 |

---

## 附录 A：完整设备生命周期模板

```csharp
using System;
using OmniFrame.Communication;
using OmniFrame.Common;
using Plc;

public class DeviceLifecycleTemplate
{
    public void FullLifecycle()
    {
        // ========== 初始化阶段 ==========
        TcpLink tcpLink = new TcpLink(1, "MyDevice", "192.168.1.100", 502);
        Plc_ModbusTcp plc = new Plc_ModbusTcp(1, "MyPLC", tcpLink);

        plc.ErrorOccurred += (s, e) =>
        {
            Logger.Error($"[{e.ErrorCode}] {e.ErrorMessage}");
            // 在此可触发报警、写入故障日志等
        };

        // ========== 连接阶段 ==========
        try
        {
            if (!plc.Open())
            {
                Logger.Error("PLC 连接失败，启动重试...");
                for (int i = 0; i < 3; i++)
                {
                    System.Threading.Thread.Sleep(2000);
                    if (plc.Open())
                    {
                        Logger.Info("重试连接成功");
                        break;
                    }
                }
                if (!plc.IsConnected)
                {
                    Logger.Fatal("PLC 连接最终失败，系统退出");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal("PLC 连接异常", ex);
            return;
        }

        // ========== 运行阶段 ==========
        try
        {
            // 读取初始状态
            bool running = false;
            plc.ReadBit(SoftElement.M, 0, ref running);

            ushort temperature = 0;
            plc.ReadWord(SoftElement.D, 100, ref temperature);

            Logger.Info($"初始状态: 运行={running}, 温度={temperature}");

            // 发送启动指令
            plc.WriteBit(SoftElement.M, 0, true);

            // 设置目标温度
            plc.WriteWord(SoftElement.D, 200, (ushort)120);

            // 持续监控（假设在主循环中调用）
            // while (running) { ... plc.ReadWord(...) ... }
        }
        catch (Exception ex)
        {
            Logger.Error("运行异常", ex);
        }

        // ========== 关闭阶段 ==========
        try
        {
            // 先发送停机指令
            plc.WriteBit(SoftElement.M, 0, false);

            // 再断开连接
            plc.Close();
            plc.Dispose();
            tcpLink.Dispose();

            Logger.Info("设备已安全关闭");
        }
        catch (Exception ex)
        {
            Logger.Error("关闭异常", ex);
        }
    }
}
```

---

## 附录 B：常用 Modbus 功能码

| 功能码 | 名称 | 说明 |
|--------|------|------|
| 0x01 | Read Coils | 读取线圈（位） |
| 0x02 | Read Discrete Inputs | 读取离散输入 |
| 0x03 | Read Holding Registers | 读取保持寄存器（字） |
| 0x04 | Read Input Registers | 读取输入寄存器 |
| 0x05 | Write Single Coil | 写单个线圈 |
| 0x06 | Write Single Register | 写单个寄存器 |
| 0x0F | Write Multiple Coils | 写多个线圈 |
| 0x10 | Write Multiple Registers | 写多个寄存器 |

---

## 附录 C：Modbus 异常码

| 异常码 | 含义 |
|--------|------|
| 0x01 | 非法功能 |
| 0x02 | 非法数据地址 |
| 0x03 | 非法数据值 |
| 0x04 | 从站设备故障 |
| 0x05 | 确认 |
| 0x06 | 从站设备忙 |
| 0x08 | 存储奇偶性差错 |
| 0x0A | 不可用网关路径 |
| 0x0B | 网关目标设备响应失败 |

---

*文档版本: 1.0 | 基于 OmniFrame 源码生成 | 适用于 .NET Framework 4.8 WinForms 应用*
