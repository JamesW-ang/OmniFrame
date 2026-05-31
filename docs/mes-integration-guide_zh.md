# OmniFrame MES 集成开发指南

本文档面向需要在 OmniFrame 平台上与 MES（Manufacturing Execution System，制造执行系统）进行集成的开发人员。文档涵盖了从架构理解到实际编码、从故障排查到系统切换的全流程。

---

## 目录

1. [MES 概述](#1-mes-概述)
2. [架构总览](#2-架构总览)
3. [配置 MES 连接](#3-配置-mes-连接)
4. [JWT 认证机制](#4-jwt-认证机制)
5. [上传生产数据](#5-上传生产数据)
6. [上传设备状态](#6-上传设备状态)
7. [上传报警数据](#7-上传报警数据)
8. [下载生产计划](#8-下载生产计划)
9. [OEE 计算与上传](#9-oee-计算与上传)
10. [扩展 MesClient——添加新 API](#10-扩展-mesclient添加新-api)
11. [切换 MES 系统供应商](#11-切换-mes-系统供应商)
12. [故障排查指南](#12-故障排查指南)
13. [无 MES 环境下的开发与测试](#13-无-mes-环境下的开发与测试)

---

## 1. MES 概述

### 1.1 什么是 MES

MES（制造执行系统）是连接企业 ERP 层与车间设备层的中间系统。在 Apple 供应链体系中，MES 承担以下核心职责：

| 职责 | 说明 |
|---|---|
| **生产计划下发** | 从 ERP/SAP 接收生产订单，分解后下发到各产线工位 |
| **生产过程追溯** | 记录每件产品的序列号、工位、操作员、测试结果，实现全生命周期追溯 |
| **质量管控** | 实时采集良率数据，触发质量门禁（如连续 N 件不良自动停线） |
| **设备管理** | 采集设备运行状态、运行时间、故障信息，支撑 OEE 计算 |
| **物料防错** | 校验物料编码与工单匹配性，防止用错物料 |
| **报表与分析** | 产出日报、周报、月报，支撑管理决策 |

### 1.2 OmniFrame 与 MES 的数据交互

在 Apple 精密制造场景中，OmniFrame 作为上位机运行于每个工位，负责设备控制、数据采集和人机交互。MES 则在车间/工厂层面进行全局管理。两者的数据交互如下：

**上行数据（OmniFrame -> MES）：**

| 数据类型 | 来源 | 触发频率 | 包含内容 |
|---|---|---|---|
| 生产数据 | `ProductionManager` | 每件产品完成时 | 订单号、产品编码、数量、合格/不良数、操作员 |
| 设备状态 | `DeviceManager` | 状态变化时 / 定时心跳 (30s) | 设备编码、运行/停止/故障状态、运行时间 |
| 报警数据 | `AlarmManager` | 报警产生/清除时 | 报警编码、描述、级别、时间戳 |
| OEE 数据 | `OeeManager` | 定时汇总 (每班/每天) | 可用率、性能率、质量率、综合 OEE |

**下行数据（MES -> OmniFrame）：**

| 数据类型 | 目标 | 触发场景 | 包含内容 |
|---|---|---|---|
| 生产计划 | `ProductionManager` | 计划发布/变更时 | 计划 ID、订单号、产品编码、计划数量、起止时间 |
| 工艺参数 | `RecipeManager` | 产品切换时 | 压力、温度、速度阈值等工艺参数 |
| 远程指令 | `RemoteMonitorManager` | 远程操控时 | 停线、重启、锁定工位等指令 |

### 1.3 Apple 供应链的特殊要求

- **全程追溯**：每个产品必须能够追溯到生产过程中的每一个工位、每一条测试数据。
- **数据完整性**：网络中断时本地缓存，恢复后补传。不允许数据丢失。
- **安全性**：所有通信必须经过认证（JWT）+ 加密（HTTPS）。
- **实时性**：关键报警必须在 5 秒内上传到 MES。

---

## 2. 架构总览

### 2.1 数据流架构图（文本描述）

```
┌──────────────────────────────────────────────────────────────────────┐
│                           MES Server                                  │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐               │
│  │ /api/auth   │  │ /api/        │  │ /api/         │               │
│  │   /login    │  │  production/ │  │  equipment/   │               │
│  │             │  │  data        │  │  status        │               │
│  │             │  │  plan        │  │               │               │
│  └──────┬──────┘  └──────┬───────┘  └───────┬───────┘               │
│         │                │                   │                        │
│         ▼                ▼                   ▼                        │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │                      MES Database                             │    │
│  │  (production_orders, product_trace, equipment_status,         │    │
│  │   alarm_log, oee_records, ...)                                │    │
│  └──────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
                              │
                     HTTP/HTTPS (REST)
                              │
┌─────────────────────────────┼────────────────────────────────────────┐
│                  OmniFrame 上位机 (每个工位)                           │
│                             │                                         │
│  ┌──────────────────────────▼──────────────────────────────────┐    │
│  │                    MesClient                                  │    │
│  │  - LoginAsync()       - UploadProductionDataAsync()          │    │
│  │  - TestConnectionAsync() - UploadEquipmentStatusAsync()      │    │
│  │  - DownloadProductionPlanAsync() - UploadAlarmDataAsync()    │    │
│  │  HttpClient (30s timeout) + Newtonsoft.Json                   │    │
│  └──────┬──────────────────────────────────┬────────────────────┘    │
│         │                                  │                          │
│         ▼                                  ▼                          │
│  ┌─────────────────┐             ┌────────────────────┐             │
│  │ ProductionManager│             │   AlarmManager     │             │
│  │ - CurrentOrder   │             │ - AddAlarm()       │             │
│  │ - RecordProdData │             │ - ClearAlarm()     │             │
│  └────────┬─────────┘             └─────────┬──────────┘             │
│           │                                 │                         │
│           ▼                                 ▼                         │
│  ┌─────────────────┐             ┌────────────────────┐             │
│  │   OeeManager    │             │   DeviceManager    │             │
│  │ - CalculateOee()│             │ - DeviceStatus     │             │
│  └────────┬─────────┘             └─────────┬──────────┘             │
│           │                                 │                         │
│           ▼                                 ▼                         │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │                    Station 层 (各工位)                     │        │
│  │  OHSLoadingStation / InspectionStation / AssemblyStation  │        │
│  └─────────────────────────────────────────────────────────┘        │
└──────────────────────────────────────────────────────────────────────┘
```

### 2.2 上行数据流（Station -> MES）

```
Station 完成一件产品
    │
    ▼
ProductionManager.RecordProductionData(isPass)
    │
    ▼
DataManager.AddProductData(ProductData)
    │
    ▼ (异步队列，每 1 秒刷盘)
本地 SQLite 落库 (Production.db)
    │
    ▼ (同时触发上传)
MesClient.UploadProductionDataAsync(ProductionData)
    │
    ▼
HTTP POST /api/production/data (JSON Body)
    │
    ▼
MES Server 接收并入库
```

### 2.3 下行数据流（MES -> Station）

```
MES Server 发布新生产计划
    │
    ▼
OmniFrame 定时轮询 或 RemoteMonitor 接收推送
    │
    ▼
MesClient.DownloadProductionPlanAsync(workshopId)
    │
    ▼
HTTP GET /api/production/plan?workshopId=WS001
    │
    ▼
返回 List<ProductionPlan>
    │
    ▼
ProductionManager.LoadProductionOrders() 更新当前工单列表
    │
    ▼
操作员可在 UI 中选择新工单开始生产
```

---

## 3. 配置 MES 连接

### 3.1 MesConfig.xml 配置文件

MES 连接 URL 以 AES 加密方式存储在 `Config/MesConfig.xml` 文件中。该文件的基础明文内容为 MES 服务器的 URL 字符串。

创建配置文件时，可以通过 `ConfigManager` 的加密写入 API 生成：

```csharp
// 在配置向导或首次配置时调用
var configManager = ServiceProviderCache.Get<IConfigManager>();
string url = "http://192.168.1.50:5000";
string encryptionKey = Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY")
    ?? "OmniFrame2024!";

// 加密并保存 MesConfig.xml
configManager.SaveEncryptedConfig("MesConfig.xml", url, encryptionKey);
Logger.Info("MES 配置已加密保存");
```

`SaveEncryptedConfig` 内部流程：
1. 将 URL 字符串序列化为 XML
2. 使用 AES-256-CBC + PBKDF2 (100000 轮迭代) 加密
3. IV 作为文件头 16 字节写入
4. 密文写入 `Config/MesConfig.xml`

配置文件的目录结构如下：
```
{AppBaseDirectory}/
  └── Config/
        ├── MesConfig.xml          (AES 加密的 MES URL)
        ├── SystemCfg.xml          (系统配置)
        ├── MotionCfg.xml          (运动控制配置)
        └── Backup/                (自动备份)
              ├── MesConfig_auto_20250503120000.xml
              └── ...
```

### 3.2 加密密钥管理

加密密钥通过环境变量 `OMNIFRAME_CONFIG_ENCRYPTION_KEY` 传入。在 `Program.cs` 中通过 DI 注册时读取：

```csharp
// Program.cs ConfigureServices 中
services.AddSingleton<MesClient>(sp =>
{
    var config = sp.GetRequiredService<IConfigManager>();
    string encryptionKey = Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY")
        ?? "OmniFrame2024!";
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY")))
    {
        Logger.Warning("环境变量 OMNIFRAME_CONFIG_ENCRYPTION_KEY 未设置，使用默认密钥");
    }
    string url = config.GetEncryptedConfig("MesConfig.xml", encryptionKey,
        defaultValue: "http://localhost:5000");
    return new MesClient(url);
});
```

**环境变量设置方式：**

| 方式 | 命令 |
|---|---|
| 系统环境变量 (推荐生产环境) | `setx OMNIFRAME_CONFIG_ENCRYPTION_KEY "YourSecureKey2025!" /M` |
| 当前会话 (调试用) | `$env:OMNIFRAME_CONFIG_ENCRYPTION_KEY="YourSecureKey2025!"` |
| 应用启动脚本 (批量部署) | 在启动 `.bat` 中添加 `set OMNIFRAME_CONFIG_ENCRYPTION_KEY=YourSecureKey2025!` |

**安全注意事项：**

1. 生产环境中**必须**设置环境变量，不能依赖默认密钥 `OmniFrame2024!`。
2. 系统启动时会检查环境变量是否设置，未设置则输出 `Logger.Warning`。
3. 密钥至少 12 个字符，建议包含大小写字母、数字和特殊字符。
4. 不要在代码仓库中提交包含密钥的 `.bat` 或 `.ps1` 启动脚本。

### 3.3 备选配置方式（未加密）

对于开发环境或测试环境，也可以直接使用未加密的 XML 配置（通过 `GetConfig` 而非 `GetEncryptedConfig`）。此时 `MesConfig.xml` 的内容为：

```xml
<?xml version="1.0" encoding="utf-8"?>
<string>http://localhost:5000</string>
```

对应的代码读取：
```csharp
string url = config.GetConfig<string>("MesConfig.xml", defaultValue: "http://localhost:5000");
```

### 3.4 DI 容器中的 MesClient 生命周期

`MesClient` 以 **Singleton** 生命周期注册在 DI 容器中，这意味着整个应用程序生命周期内只存在一个 `MesClient` 实例。所有通过 DI 获取的 `MesClient` 都是同一个实例。

```csharp
// 在其他 Manager 或 Form 中使用 MesClient
public class ProductionDashboardForm : Form
{
    private readonly MesClient _mesClient;

    public ProductionDashboardForm(MesClient mesClient)
    {
        _mesClient = mesClient;
    }
}
```

如需手动获取：
```csharp
var mesClient = ServiceProviderCache.Get<MesClient>();
```

---

## 4. JWT 认证机制

### 4.1 登录流程

```csharp
// 登录 MES 系统
var mesClient = new MesClient("http://192.168.1.50:5000");
bool loginSuccess = await mesClient.LoginAsync("omniFrame_user", "securePassword123");
if (loginSuccess)
{
    Logger.Info("MES 认证成功，后续请求将携带 JWT Token");
}
else
{
    Logger.Error("MES 认证失败，请检查用户名/密码或网络连接");
}
```

### 4.2 LoginAsync 内部流程

```csharp
public async Task<bool> LoginAsync(string username, string password)
{
    try
    {
        // Step 1: 构建 JSON 请求体
        var data = new { username, password };
        var content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json");

        // Step 2: POST /api/auth/login
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Step 3: 解析响应，提取 JWT Token
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            if (tokenData.ContainsKey("token"))
            {
                _token = tokenData["token"];

                // Step 4: 设置 Authorization 请求头（后续所有请求自动携带）
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                Logger.Info("MES系统登录成功");
                return true;
            }
        }
        Logger.Error("MES系统登录失败");
        return false;
    }
    catch (Exception ex)
    {
        Logger.Error("MES系统登录失败", ex);
        return false;
    }
}
```

### 4.3 JSON 请求/响应格式

**请求：**
```
POST /api/auth/login HTTP/1.1
Host: 192.168.1.50:5000
Content-Type: application/json

{
  "username": "omniFrame_user",
  "password": "securePassword123"
}
```

**成功响应 (200)：**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJvbW5pRnJhbWVfdXNlciIsImV4cCI6MTcxNDcyOTYwMCwiaWF0IjoxNzE0NjQzMjAwfQ.xxx",
  "expiresIn": 86400
}
```

**失败响应 (401)：**
```json
{
  "error": "Invalid credentials",
  "message": "用户名或密码错误"
}
```

### 4.4 Token 管理与会话保持

当前实现的特点和注意事项：

1. **Token 存储在内存中**（`_token` 字段），应用程序重启后需重新登录。
2. **Token 过期处理**：当前版本（v1.7）未实现自动 Token 刷新。如果 MES 返回 401，需要重新调用 `LoginAsync`。生产环境建议实现自动重登录机制（见下方示例）。

```csharp
/// <summary>
/// 带自动重登录的上传方法（扩展 MesClient）
/// </summary>
public async Task<bool> UploadProductionDataWithRetryAsync(
    ProductionData data,
    string username,
    string password,
    int maxRetries = 2)
{
    for (int retry = 0; retry <= maxRetries; retry++)
    {
        var response = await _httpClient.PostAsync(
            "/api/production/data",
            new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Logger.Info("生产数据上传成功");
            return true;
        }

        // 401 表示 Token 过期，尝试重新登录
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retry < maxRetries)
        {
            Logger.Warning("Token 已过期，尝试重新登录...");
            bool loggedIn = await LoginAsync(username, password);
            if (!loggedIn)
            {
                Logger.Error("重新登录失败，放弃重试");
                return false;
            }
            Logger.Info("重新登录成功，继续上传");
            continue;
        }

        Logger.Error($"生产数据上传失败 (HTTP {(int)response.StatusCode})");
        return false;
    }

    return false;
}
```

3. **启动时的认证**：建议在应用程序启动后（`MainForm_Load` 或 `SystemManager.Start()` 中）立即进行 MES 登录：

```csharp
// 在 MainForm 加载完成后
private async void MainForm_Load(object sender, EventArgs e)
{
    try
    {
        var mesClient = ServiceProviderCache.Get<MesClient>();
        var config = ServiceProviderCache.Get<IConfigManager>();

        // 从配置文件读取 MES 凭据
        var mesCred = config.GetConfig<MesCredential>("MesCredential.xml",
            defaultValue: new MesCredential
            {
                Username = "omniFrame_user",
                Password = "default123"
            });

        bool loggedIn = await mesClient.LoginAsync(mesCred.Username, mesCred.Password);
        if (loggedIn)
        {
            toolStripStatusLabel.Text = "MES: 已连接";
            toolStripStatusLabel.ForeColor = Color.Green;
        }
        else
        {
            toolStripStatusLabel.Text = "MES: 未连接";
            toolStripStatusLabel.ForeColor = Color.Red;
        }
    }
    catch (Exception ex)
    {
        Logger.Error("MES 初始化连接失败", ex);
    }
}
```

---

## 5. 上传生产数据

### 5.1 ProductionData 数据模型

```csharp
public class ProductionData
{
    /// <summary>生产订单号</summary>
    public string OrderId { get; set; }

    /// <summary>产品编码</summary>
    public string ProductCode { get; set; }

    /// <summary>本批生产数量</summary>
    public int Quantity { get; set; }

    /// <summary>本批合格数量</summary>
    public int PassQuantity { get; set; }

    /// <summary>本批不合格数量</summary>
    public int FailQuantity { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>结束时间</summary>
    public DateTime EndTime { get; set; }

    /// <summary>操作员</summary>
    public string Operator { get; set; }
}
```

### 5.2 API 端点

```
POST /api/production/data
Content-Type: application/json
Authorization: Bearer <JWT Token>
```

### 5.3 请求/响应示例

**请求体：**
```json
{
  "OrderId": "PO-2025-0503-001",
  "ProductCode": "IPHONE17-PRO-BACKPLATE",
  "Quantity": 500,
  "PassQuantity": 487,
  "FailQuantity": 13,
  "StartTime": "2025-05-03T08:00:00",
  "EndTime": "2025-05-03T12:00:00",
  "Operator": "ZhangWei"
}
```

**成功响应 (200)：**
```json
{
  "success": true,
  "message": "生产数据已接收",
  "recordId": "REC-20250503-00001"
}
```

**失败响应 (400) —— 数据校验失败：**
```json
{
  "success": false,
  "message": "订单号 PO-2025-0503-001 不存在"
}
```

### 5.4 完整代码示例：从工位采集数据到上传 MES

以下示例展示了一个组装工站在完成一批产品后，如何收集数据并上传到 MES：

```csharp
using System;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.Core;
using OmniFrame.DataAccess;

namespace OmniFrame.Stations
{
    /// <summary>
    /// 产品组装完成后，收集生产数据并上传 MES
    /// </summary>
    public class ProductionUploadHelper
    {
        private readonly MesClient _mesClient;
        private readonly IProductionManager _productionManager;
        private readonly string _stationName;

        // 本批次计数器
        private int _batchPassCount;
        private int _batchFailCount;
        private DateTime _batchStartTime;

        public ProductionUploadHelper(
            MesClient mesClient,
            IProductionManager productionManager,
            string stationName)
        {
            _mesClient = mesClient;
            _productionManager = productionManager;
            _stationName = stationName;
        }

        /// <summary>
        /// 开始新批次
        /// </summary>
        public void BeginBatch()
        {
            _batchPassCount = 0;
            _batchFailCount = 0;
            _batchStartTime = DateTime.Now;
            Logger.Info($"工位 {_stationName} 开始新批次");
        }

        /// <summary>
        /// 记录单件产品结果（每件产品调用一次）
        /// </summary>
        public void RecordSingleResult(bool isPass)
        {
            if (isPass)
                _batchPassCount++;
            else
                _batchFailCount++;

            // 同步记录到 ProductionManager
            _productionManager.RecordProductionData(isPass);
        }

        /// <summary>
        /// 批次结束时，汇总数据并上传到 MES
        /// 通常在班次结束、工单完成、或达到批次上限时调用
        /// </summary>
        public async Task<bool> EndBatchAndUploadAsync()
        {
            var currentOrder = _productionManager.GetCurrentOrder();
            if (currentOrder == null)
            {
                Logger.Warning("无当前生产订单，跳过 MES 上传");
                return false;
            }

            var productionData = new ProductionData
            {
                OrderId = currentOrder.OrderId,
                ProductCode = currentOrder.ProductCode,
                Quantity = _batchPassCount + _batchFailCount,
                PassQuantity = _batchPassCount,
                FailQuantity = _batchFailCount,
                StartTime = _batchStartTime,
                EndTime = DateTime.Now,
                Operator = Environment.UserName  // 或从登录系统获取
            };

            // 数据校验
            if (!ValidateProductionData(productionData))
            {
                Logger.Error("生产数据校验失败，取消 MES 上传");
                return false;
            }

            // 上传到 MES
            return await UploadWithRetryAsync(productionData, maxRetries: 3);
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        private bool ValidateProductionData(ProductionData data)
        {
            if (string.IsNullOrEmpty(data.OrderId))
            {
                Logger.Error("校验失败：订单号不能为空");
                return false;
            }

            if (string.IsNullOrEmpty(data.ProductCode))
            {
                Logger.Error("校验失败：产品编码不能为空");
                return false;
            }

            if (data.Quantity <= 0)
            {
                Logger.Error("校验失败：生产数量必须大于 0");
                return false;
            }

            if (data.PassQuantity + data.FailQuantity != data.Quantity)
            {
                Logger.Error($"校验失败：合格数({data.PassQuantity}) + 不合格数({data.FailQuantity}) != 总数({data.Quantity})");
                return false;
            }

            if (data.EndTime < data.StartTime)
            {
                Logger.Error("校验失败：结束时间早于开始时间");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 带重试的上传逻辑
        /// </summary>
        private async Task<bool> UploadWithRetryAsync(ProductionData data, int maxRetries)
        {
            int retryCount = 0;
            while (retryCount <= maxRetries)
            {
                try
                {
                    bool success = await _mesClient.UploadProductionDataAsync(data);
                    if (success)
                    {
                        Logger.Info($"生产数据上传成功: 订单 {data.OrderId}, "
                            + $"总数 {data.Quantity}, "
                            + $"合格 {data.PassQuantity}, "
                            + $"不合格 {data.FailQuantity}");
                        return true;
                    }

                    retryCount++;
                    if (retryCount <= maxRetries)
                    {
                        int delayMs = retryCount * 2000; // 递增等待: 2s, 4s, 6s
                        Logger.Warning($"上传失败，第 {retryCount}/{maxRetries} 次重试，等待 {delayMs}ms...");
                        await Task.Delay(delayMs);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error("网络请求异常", ex);
                    retryCount++;
                    if (retryCount <= maxRetries)
                    {
                        await Task.Delay(retryCount * 2000);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Error("请求超时（30秒）", ex);
                    retryCount++;
                    if (retryCount <= maxRetries)
                    {
                        await Task.Delay(retryCount * 2000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("上传发生未知异常", ex);
                    return false; // 未知异常不重试
                }
            }

            Logger.Error($"上传最终失败，已重试 {maxRetries} 次。"
                + $"数据已保存在本地数据库，等待网络恢复后补传。");

            // TODO: 将失败的数据写入重试队列（DataManager 的待补传队列）
            return false;
        }
    }
}
```

### 5.5 在 Station 中调用

```csharp
// 在某个 Station 的 step 逻辑中
public override async Task ExecuteStepAsync()
{
    switch (CurrentStep)
    {
        case StationStep.Processing:
        {
            // 执行装配操作...
            bool assemblyResult = ExecuteAssembly();

            // 记录单件结果
            _uploadHelper.RecordSingleResult(assemblyResult);
            break;
        }

        case StationStep.BatchComplete:
        {
            // 批次完成，汇总并上传
            bool uploaded = await _uploadHelper.EndBatchAndUploadAsync();
            if (!uploaded)
            {
                SetAlarm("MES_UploadFailed", 0);
            }
            _uploadHelper.BeginBatch(); // 开始下一批次
            break;
        }
    }
}
```

---

## 6. 上传设备状态

### 6.1 EquipmentData 数据模型

```csharp
public class EquipmentData
{
    /// <summary>设备编码（与 MES 系统中的设备台账对应）</summary>
    public string EquipmentCode { get; set; }

    /// <summary>设备状态: "运行", "停止", "故障"</summary>
    public string Status { get; set; }

    /// <summary>状态采集时间</summary>
    public DateTime StatusTime { get; set; }

    /// <summary>累计运行时间（小时）</summary>
    public double RunTime { get; set; }

    /// <summary>累计故障时间（小时）</summary>
    public double FaultTime { get; set; }
}
```

### 6.2 API 端点

```
POST /api/equipment/status
Content-Type: application/json
Authorization: Bearer <JWT Token>
```

### 6.3 请求/响应示例

**请求体：**
```json
{
  "EquipmentCode": "ASM-LINE01-ST01",
  "Status": "运行",
  "StatusTime": "2025-05-03T14:30:00",
  "RunTime": 6.5,
  "FaultTime": 0.0
}
```

**成功响应 (200)：**
```json
{
  "success": true,
  "message": "设备状态已更新"
}
```

### 6.4 完整代码示例：设备状态监控与上报

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 设备状态监控器
    /// 每 30 秒采集一次设备状态，状态变化时立即上报 MES
    /// </summary>
    public class EquipmentStatusReporter : IDisposable
    {
        private readonly MesClient _mesClient;
        private readonly string _equipmentCode;
        private Timer _heartbeatTimer;
        private CancellationTokenSource _cts;

        // 设备状态枚举
        public enum EquipStatus
        {
            运行,
            停止,
            故障
        }

        private EquipStatus _currentStatus = EquipStatus.停止;
        private DateTime _statusChangeTime;
        private DateTime _runningStartTime;
        private double _accumulatedRunTime;   // 累计运行时间（小时）
        private double _accumulatedFaultTime; // 累计故障时间（小时）

        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

        public EquipmentStatusReporter(MesClient mesClient, string equipmentCode)
        {
            _mesClient = mesClient ?? throw new ArgumentNullException(nameof(mesClient));
            _equipmentCode = equipmentCode ?? throw new ArgumentNullException(nameof(equipmentCode));
            _statusChangeTime = DateTime.Now;
        }

        /// <summary>
        /// 启动设备状态监控
        /// </summary>
        public void Start()
        {
            _cts = new CancellationTokenSource();
            _heartbeatTimer = new Timer(
                async _ => await SendHeartbeatAsync(),
                null,
                HeartbeatInterval,
                HeartbeatInterval);

            Logger.Info($"设备状态监控已启动: {_equipmentCode}");
        }

        /// <summary>
        /// 状态变化时调用此方法
        /// </summary>
        public async Task ReportStatusChangeAsync(EquipStatus newStatus)
        {
            if (newStatus == _currentStatus)
                return; // 状态未变化，不上报

            var previousStatus = _currentStatus;
            var now = DateTime.Now;

            // 累计之前状态的持续时间
            AccumulateTime(previousStatus, now);

            // 更新当前状态
            _currentStatus = newStatus;
            _statusChangeTime = now;

            if (newStatus == EquipStatus.运行)
            {
                _runningStartTime = now;
            }

            // 立即上报状态变化
            var equipmentData = BuildEquipmentData();
            await UploadStatusAsync(equipmentData);

            Logger.Info($"设备状态变化: {_equipmentCode} [{previousStatus}] -> [{newStatus}]");
        }

        /// <summary>
        /// 定时心跳上报
        /// </summary>
        private async Task SendHeartbeatAsync()
        {
            try
            {
                // 累计当前状态的持续时间
                AccumulateTime(_currentStatus, DateTime.Now);

                var equipmentData = BuildEquipmentData();
                bool success = await _mesClient.UploadEquipmentStatusAsync(equipmentData);

                if (success)
                {
                    Logger.Info($"设备心跳上报成功: {_equipmentCode}, 状态: {_currentStatus}, "
                        + $"运行时间: {_accumulatedRunTime:F1}h, 故障时间: {_accumulatedFaultTime:F1}h");
                }
                else
                {
                    Logger.Warning($"设备心跳上报失败: {_equipmentCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"设备心跳上报异常: {_equipmentCode}", ex);
            }
        }

        /// <summary>
        /// 累计状态时间
        /// </summary>
        private void AccumulateTime(EquipStatus status, DateTime toTime)
        {
            TimeSpan duration = toTime - _statusChangeTime;
            if (duration <= TimeSpan.Zero) return;

            switch (status)
            {
                case EquipStatus.运行:
                    _accumulatedRunTime += duration.TotalHours;
                    break;
                case EquipStatus.故障:
                    _accumulatedFaultTime += duration.TotalHours;
                    break;
            }
            _statusChangeTime = toTime;
        }

        /// <summary>
        /// 构建 EquipmentData
        /// </summary>
        private EquipmentData BuildEquipmentData()
        {
            return new EquipmentData
            {
                EquipmentCode = _equipmentCode,
                Status = _currentStatus.ToString(),
                StatusTime = DateTime.Now,
                RunTime = Math.Round(_accumulatedRunTime, 2),
                FaultTime = Math.Round(_accumulatedFaultTime, 2)
            };
        }

        /// <summary>
        /// 上传设备状态（带重试）
        /// </summary>
        private async Task<bool> UploadStatusAsync(EquipmentData data)
        {
            for (int retry = 0; retry <= 2; retry++)
            {
                try
                {
                    bool success = await _mesClient.UploadEquipmentStatusAsync(data);
                    if (success) return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"设备状态上传异常 (重试 {retry})", ex);
                }

                if (retry < 2)
                {
                    await Task.Delay(2000);
                }
            }

            Logger.Error($"设备状态上传最终失败: {_equipmentCode}");
            return false;
        }

        public void Stop()
        {
            _cts?.Cancel();
            _heartbeatTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            Logger.Info($"设备状态监控已停止: {_equipmentCode}");
        }

        public void Dispose()
        {
            Stop();
            _heartbeatTimer?.Dispose();
            _cts?.Dispose();
        }
    }
}
```

### 6.5 注册到 DI 并在 Program.cs 中使用

```csharp
// 在 ConfigureServices 中注册
services.AddSingleton(sp =>
{
    var mesClient = sp.GetRequiredService<MesClient>();
    // 设备编码可以从配置文件读取
    var systemConfig = sp.GetRequiredService<IConfigManager>()
        .GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
    string equipmentCode = systemConfig.EquipmentCode ?? $"{Environment.MachineName}-ST01";
    return new EquipmentStatusReporter(mesClient, equipmentCode);
});

// 在 StartupInfrastructure 中启动
private static void StartupInfrastructure(ServiceProvider provider)
{
    // ... 其他初始化 ...

    // 启动设备状态上报
    var statusReporter = provider.GetRequiredService<EquipmentStatusReporter>();
    statusReporter.Start();
}
```

---

## 7. 上传报警数据

### 7.1 AlarmData 数据模型

```csharp
public class AlarmData
{
    /// <summary>报警 ID（用于后续清除时关联）</summary>
    public string AlarmId { get; set; }

    /// <summary>设备编码</summary>
    public string EquipmentCode { get; set; }

    /// <summary>报警类型编码</summary>
    public string AlarmType { get; set; }

    /// <summary>报警描述</summary>
    public string AlarmDescription { get; set; }

    /// <summary>报警发生时间</summary>
    public DateTime AlarmTime { get; set; }

    /// <summary>报警恢复时间（null 表示未恢复）</summary>
    public DateTime? RestoreTime { get; set; }

    /// <summary>处理人员</summary>
    public string Handler { get; set; }
}
```

### 7.2 API 端点

```
POST /api/alarm/data
Content-Type: application/json
Authorization: Bearer <JWT Token>
```

### 7.3 请求/响应示例

**报警发生请求：**
```json
{
  "AlarmId": "ALM-20250503-00042",
  "EquipmentCode": "ASM-LINE01-ST01",
  "AlarmType": "MOTOR_OVERTEMP",
  "AlarmDescription": "伺服电机温度超过85度阈值",
  "AlarmTime": "2025-05-03T14:35:22",
  "RestoreTime": null,
  "Handler": null
}
```

**报警清除请求：**
```json
{
  "AlarmId": "ALM-20250503-00042",
  "EquipmentCode": "ASM-LINE01-ST01",
  "AlarmType": "MOTOR_OVERTEMP",
  "AlarmDescription": "伺服电机温度超过85度阈值",
  "AlarmTime": "2025-05-03T14:35:22",
  "RestoreTime": "2025-05-03T14:42:10",
  "Handler": "LiuMing"
}
```

**成功响应 (200)：**
```json
{
  "success": true,
  "message": "报警数据已接收",
  "alarmId": "ALM-20250503-00042"
}
```

### 7.4 完整代码示例：AlarmManager 集成 MES 上报

以下代码展示如何在 `AlarmManager` 产生和清除报警时，同步上报到 MES：

```csharp
using System;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 报警 MES 同步器
    /// 订阅 AlarmManager 的事件，在报警产生/清除时自动上传 MES
    /// </summary>
    public class AlarmMesSyncService : IDisposable
    {
        private readonly IAlarmManager _alarmManager;
        private readonly MesClient _mesClient;
        private readonly string _equipmentCode;
        private bool _isSubscribed;

        public AlarmMesSyncService(
            IAlarmManager alarmManager,
            MesClient mesClient,
            string equipmentCode)
        {
            _alarmManager = alarmManager ?? throw new ArgumentNullException(nameof(alarmManager));
            _mesClient = mesClient ?? throw new ArgumentNullException(nameof(mesClient));
            _equipmentCode = equipmentCode ?? throw new ArgumentNullException(nameof(equipmentCode));
        }

        /// <summary>
        /// 启动报警同步（订阅事件）
        /// </summary>
        public void Start()
        {
            if (_isSubscribed) return;

            _alarmManager.AlarmOccurred += OnAlarmOccurred;
            _alarmManager.AlarmCleared += OnAlarmCleared;
            _isSubscribed = true;

            Logger.Info("报警 MES 同步服务已启动");
        }

        /// <summary>
        /// 报警产生时的事件处理
        /// </summary>
        private async void OnAlarmOccurred(object sender, AlarmInfo alarmInfo)
        {
            try
            {
                var alarmData = new AlarmData
                {
                    AlarmId = $"ALM-{alarmInfo.OccurTime:yyyyMMdd}-{alarmInfo.Id:D5}",
                    EquipmentCode = _equipmentCode,
                    AlarmType = alarmInfo.AlarmCode,
                    AlarmDescription = alarmInfo.AlarmMessage,
                    AlarmTime = alarmInfo.OccurTime,
                    RestoreTime = null,
                    Handler = null
                };

                bool success = await UploadAlarmWithRetryAsync(alarmData);
                if (success)
                {
                    Logger.Info($"报警已同步到 MES: [{alarmInfo.Level}] {alarmInfo.AlarmCode}");
                }
                else
                {
                    Logger.Error($"报警同步 MES 失败: {alarmInfo.AlarmCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理报警同步异常: {alarmInfo.AlarmCode}", ex);
            }
        }

        /// <summary>
        /// 报警清除时的事件处理
        /// </summary>
        private async void OnAlarmCleared(object sender, AlarmInfo alarmInfo)
        {
            try
            {
                var alarmData = new AlarmData
                {
                    AlarmId = $"ALM-{alarmInfo.OccurTime:yyyyMMdd}-{alarmInfo.Id:D5}",
                    EquipmentCode = _equipmentCode,
                    AlarmType = alarmInfo.AlarmCode,
                    AlarmDescription = alarmInfo.AlarmMessage,
                    AlarmTime = alarmInfo.OccurTime,
                    RestoreTime = alarmInfo.ClearTime ?? DateTime.Now,
                    Handler = alarmInfo.ClearUser ?? "System"
                };

                bool success = await UploadAlarmWithRetryAsync(alarmData);
                if (success)
                {
                    Logger.Info($"报警清除已同步到 MES: {alarmInfo.AlarmCode}, "
                        + $"处理人: {alarmInfo.ClearUser}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理报警清除同步异常: {alarmInfo.AlarmCode}", ex);
            }
        }

        /// <summary>
        /// 上传报警数据（带重试），特别针对严重报警使用更激进的重试策略
        /// </summary>
        private async Task<bool> UploadAlarmWithRetryAsync(AlarmData data)
        {
            int maxRetries = data.AlarmType switch
            {
                // Critical 级别报警：5 次重试
                var t when t.StartsWith("CRITICAL_") => 5,
                // Error 级别：3 次重试
                var t when t.StartsWith("ERROR_") => 3,
                // 其他：2 次重试
                _ => 2
            };

            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    bool success = await _mesClient.UploadAlarmDataAsync(data);
                    if (success) return true;
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error($"报警上传网络异常 (重试 {retry}/{maxRetries})", ex);
                }
                catch (TaskCanceledException)
                {
                    Logger.Error($"报警上传超时 (重试 {retry}/{maxRetries})");
                }
                catch (Exception ex)
                {
                    Logger.Error($"报警上传未知异常", ex);
                    return false; // 未知异常不重试
                }

                if (retry < maxRetries)
                {
                    // 指数退避: 1s, 2s, 4s, 8s, 16s
                    int delayMs = (int)Math.Pow(2, retry) * 1000;
                    await Task.Delay(delayMs);
                }
            }

            Logger.Error($"报警上传最终失败: {data.AlarmId}");
            return false;
        }

        /// <summary>
        /// 停止同步
        /// </summary>
        public void Stop()
        {
            if (!_isSubscribed) return;

            _alarmManager.AlarmOccurred -= OnAlarmOccurred;
            _alarmManager.AlarmCleared -= OnAlarmCleared;
            _isSubscribed = false;

            Logger.Info("报警 MES 同步服务已停止");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
```

### 7.5 在 Program.cs 中注册

```csharp
// 在 ConfigureServices 中注册
services.AddSingleton<AlarmMesSyncService>(sp =>
{
    var alarmManager = sp.GetRequiredService<IAlarmManager>();
    var mesClient = sp.GetRequiredService<MesClient>();
    var config = sp.GetRequiredService<IConfigManager>();
    var sysConfig = config.GetConfig<SystemConfig>("SystemCfg.xml", new SystemConfig());
    string equipCode = sysConfig.EquipmentCode ?? Environment.MachineName;
    return new AlarmMesSyncService(alarmManager, mesClient, equipCode);
});

// 在 StartupInfrastructure 中启动
private static void StartupInfrastructure(ServiceProvider provider)
{
    // ... 其他初始化 ...

    var alarmSync = provider.GetRequiredService<AlarmMesSyncService>();
    alarmSync.Start();
}
```

---

## 8. 下载生产计划

### 8.1 ProductionPlan 数据模型

```csharp
public class ProductionPlan
{
    /// <summary>计划 ID（MES 系统内的唯一标识）</summary>
    public string PlanId { get; set; }

    /// <summary>生产订单号</summary>
    public string OrderId { get; set; }

    /// <summary>产品编码</summary>
    public string ProductCode { get; set; }

    /// <summary>产品名称</summary>
    public string ProductName { get; set; }

    /// <summary>计划生产数量</summary>
    public int PlanQuantity { get; set; }

    /// <summary>计划开始日期</summary>
    public DateTime StartDate { get; set; }

    /// <summary>计划结束日期</summary>
    public DateTime EndDate { get; set; }

    /// <summary>计划状态: "待生产", "生产中", "已完成", "已取消"</summary>
    public string Status { get; set; }
}
```

### 8.2 API 端点

```
GET /api/production/plan?workshopId={workshopId}
Authorization: Bearer <JWT Token>
```

### 8.3 请求/响应示例

**请求：**
```
GET /api/production/plan?workshopId=WS-ASSEMBLY-01 HTTP/1.1
Host: 192.168.1.50:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**成功响应 (200)：**
```json
[
  {
    "PlanId": "PLAN-20250503-001",
    "OrderId": "PO-2025-0503-001",
    "ProductCode": "IPHONE17-PRO-BACKPLATE",
    "ProductName": "iPhone 17 Pro 背板组件",
    "PlanQuantity": 5000,
    "StartDate": "2025-05-03T08:00:00",
    "EndDate": "2025-05-03T20:00:00",
    "Status": "生产中"
  },
  {
    "PlanId": "PLAN-20250503-002",
    "OrderId": "PO-2025-0503-002",
    "ProductCode": "IPHONE17-PRO-FRAME",
    "ProductName": "iPhone 17 Pro 中框组件",
    "PlanQuantity": 5000,
    "StartDate": "2025-05-03T20:00:00",
    "EndDate": "2025-05-04T08:00:00",
    "Status": "待生产"
  }
]
```

### 8.4 完整代码示例：计划下载与同步

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Core
{
    /// <summary>
    /// 生产计划同步器
    /// 从 MES 下载生产计划，并同步到本地 ProductionManager
    /// </summary>
    public class ProductionPlanSyncService
    {
        private readonly MesClient _mesClient;
        private readonly IProductionManager _productionManager;
        private readonly string _workshopId;
        private System.Threading.Timer _syncTimer;
        private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);

        public ProductionPlanSyncService(
            MesClient mesClient,
            IProductionManager productionManager)
        {
            _mesClient = mesClient ?? throw new ArgumentNullException(nameof(mesClient));
            _productionManager = productionManager ?? throw new ArgumentNullException(nameof(productionManager));

            // workshopId 可以从配置或环境变量获取
            _workshopId = Environment.GetEnvironmentVariable("OMNIFRAME_WORKSHOP_ID") ?? "DEFAULT-WS";
        }

        /// <summary>
        /// 启动定时同步（每 5 分钟）
        /// </summary>
        public void StartPeriodicSync()
        {
            _syncTimer = new System.Threading.Timer(
                async _ => await SyncPlansAsync(),
                null,
                TimeSpan.Zero,    // 立即执行第一次
                SyncInterval);

            Logger.Info($"生产计划定时同步已启动 (间隔 {SyncInterval.TotalMinutes} 分钟)");
        }

        /// <summary>
        /// 执行一次计划同步
        /// </summary>
        public async Task<bool> SyncPlansAsync()
        {
            try
            {
                Logger.Info($"开始同步生产计划 (车间: {_workshopId})...");

                // Step 1: 从 MES 下载计划
                List<ProductionPlan> plans = await _mesClient.DownloadProductionPlanAsync(_workshopId);

                if (plans == null || plans.Count == 0)
                {
                    Logger.Info($"MES 返回空计划列表 (车间: {_workshopId})");
                    return true;
                }

                Logger.Info($"从 MES 下载到 {plans.Count} 条生产计划");

                // Step 2: 转换 ProductionPlan -> ProductionOrder
                var orders = plans.Select(plan => new ProductionOrder
                {
                    OrderId = plan.OrderId,
                    ProductCode = plan.ProductCode,
                    ProductName = plan.ProductName,
                    PlanQuantity = plan.PlanQuantity,
                    ActualQuantity = 0,
                    PassQuantity = 0,
                    FailQuantity = 0,
                    StartTime = plan.StartDate,
                    EndTime = plan.EndDate,
                    Status = MapStatus(plan.Status)
                }).ToList();

                // Step 3: 更新本地 ProductionManager
                SyncToProductionManager(orders);

                Logger.Info($"生产计划同步完成: {orders.Count} 条工单已更新");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error("下载生产计划时网络异常", ex);
                return false;
            }
            catch (TaskCanceledException)
            {
                Logger.Error("下载生产计划超时");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("同步生产计划失败", ex);
                return false;
            }
        }

        /// <summary>
        /// MES 状态映射到本地状态
        /// </summary>
        private string MapStatus(string mesStatus)
        {
            return mesStatus switch
            {
                "待生产" => "待生产",
                "生产中" => "生产中",
                "已完成" => "已完成",
                "已取消" => "已取消",
                _ => "待生产"
            };
        }

        /// <summary>
        /// 将下载的订单同步到 ProductionManager
        /// 注意：此处为示例逻辑。实际项目可能需要与 ProductionManager.LoadProductionOrders() 配合，
        /// 通过修改 ProductionManager 添加新订单或更新已有订单。
        /// </summary>
        private void SyncToProductionManager(List<ProductionOrder> orders)
        {
            // ProductionManager 当前没有公开的替换方法，需要通过 LoadProductionOrders 加载。
            // 实际实现中，你需要：
            // 1. 扩展 IProductionManager 接口，添加 SyncOrders(List<ProductionOrder>) 方法
            // 2. 在 ProductionManager 中实现该方法的逻辑

            // 注意：下列代码展示了调用 LoadProductionOrders 加载本地存储的工单。
            // 同步 MES 下载的订单需要修改 ProductionManager 源码，参考下方扩展建议。
            _productionManager.LoadProductionOrders();

            var existingOrders = _productionManager.GetProductionOrders();

            foreach (var newOrder in orders)
            {
                var existing = existingOrders.FirstOrDefault(o => o.OrderId == newOrder.OrderId);
                if (existing != null)
                {
                    // 更新现有订单的计划信息（不覆盖实际生产数据）
                    existing.PlanQuantity = newOrder.PlanQuantity;
                    existing.EndTime = newOrder.EndTime;
                    if (existing.Status == "待生产")
                    {
                        existing.Status = newOrder.Status;
                    }
                }
                else
                {
                    // 新订单：通过 StartProductionOrder / LoadProductionOrders 的扩展逻辑添加
                    Logger.Info($"发现新生产订单: {newOrder.OrderId} - {newOrder.ProductName}");
                }
            }
        }

        /// <summary>
        /// 扩展建议：在 IProductionManager 接口中添加 SyncOrders 方法
        /// </summary>
        // 建议添加到 IProductionManager：
        // bool SyncOrders(List<ProductionOrder> orders);

        // 建议在 ProductionManager 中实现：
        // public bool SyncOrders(List<ProductionOrder> orders)
        // {
        //     lock (_orderLock)
        //     {
        //         foreach (var order in orders)
        //         {
        //             var existing = _productionOrders.FirstOrDefault(o => o.OrderId == order.OrderId);
        //             if (existing != null)
        //             {
        //                 existing.PlanQuantity = order.PlanQuantity;
        //                 existing.EndTime = order.EndTime;
        //             }
        //             else
        //             {
        //                 _productionOrders.Add(order);
        //             }
        //         }
        //     }
        //     Logger.Info($"生产订单已同步，总数: {orders.Count}");
        //     return true;
        // }

        public void Stop()
        {
            _syncTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            Logger.Info("生产计划定时同步已停止");
        }
    }
}
```

### 8.5 在 UI 中手动触发计划刷新

```csharp
// 在主界面添加"刷新计划"按钮
private async void btnRefreshPlan_Click(object sender, EventArgs e)
{
    btnRefreshPlan.Enabled = false;
    toolStripStatusLabel.Text = "正在从 MES 下载生产计划...";

    try
    {
        var syncService = ServiceProviderCache.Get<ProductionPlanSyncService>();
        bool success = await syncService.SyncPlansAsync();

        if (success)
        {
            toolStripStatusLabel.Text = "生产计划已更新";
            // 刷新 UI 中的工单列表
            RefreshOrderList();
        }
        else
        {
            toolStripStatusLabel.Text = "生产计划下载失败";
            MessageBox.Show("从 MES 下载生产计划失败，请检查网络连接后重试。",
                "同步失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    catch (Exception ex)
    {
        Logger.Error("手动刷新计划失败", ex);
        MessageBox.Show($"刷新失败: {ex.Message}", "错误",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        btnRefreshPlan.Enabled = true;
    }
}
```

---

## 9. OEE 计算与上传

### 9.1 OEE 计算公式

OEE（Overall Equipment Effectiveness，设备综合效率）由三个子指标组成：

```
OEE = 可用率 (Availability) x 性能率 (Performance) x 质量率 (Quality)

其中：
  可用率 = 实际运行时间 / 计划运行时间
  性能率 = (总产量 x 理想节拍) / 实际运行时间
  质量率 = 合格品数 / 总产量
```

### 9.2 OeeManager 内部实现

`OeeManager` 的 `CalculateOee` 方法已经实现了完整的 OEE 计算：

```csharp
public double CalculateOee(string lineName)
{
    lock (_lock)
    {
        if (!_lines.TryGetValue(lineName, out var data) || data.TotalCount == 0)
        {
            return 0.0;
        }

        var availableTime = data.AvailableTime;
        if (availableTime.TotalSeconds <= 0) return 0.0;

        // 可用率 = 实际可用时间 / 计划运行时间 (从生产开始至今)
        double availability = availableTime.TotalSeconds /
            Math.Max((DateTime.Now - (data.ProductionStart ?? DateTime.Now)).TotalSeconds, 1);

        // 性能率 = (总产量 x 理想节拍) / 实际可用时间
        double performance = (data.TotalCount * IdealCycleTimeSeconds) / availableTime.TotalSeconds;

        // 质量率 = 合格品 / 总产量
        double quality = data.PassRate;

        // 综合 OEE
        double oee = availability * performance * quality * 100;
        oee = Math.Round(Math.Min(oee, 100.0), 1);

        Logger.Info($"OEE: 计算生产线 {lineName} 综合效率 = {oee}% "
            + $"(A={availability:P2}, P={performance:P2}, Q={quality:P2})");
        return oee;
    }
}
```

其中 `IdealCycleTimeSeconds = 1.0`（理想节拍 1 秒/件），可根据实际产品调整。

### 9.3 OEE 数据的 MES 数据模型

MesClient 当前没有内置的 OEE 上传方法。我们需要扩展 MesClient（参考第 10 章）。首先定义 OEE 数据模型和 API 端点：

```csharp
// 新增 OEE 数据模型
public class OeeData
{
    /// <summary>产线名称</summary>
    public string LineName { get; set; }

    /// <summary>统计时间</summary>
    public DateTime ReportTime { get; set; }

    /// <summary>可用率 (0-1)</summary>
    public double Availability { get; set; }

    /// <summary>性能率 (0-1)</summary>
    public double Performance { get; set; }

    /// <summary>质量率 (0-1)</summary>
    public double Quality { get; set; }

    /// <summary>综合 OEE (0-100%)</summary>
    public double OeeValue { get; set; }

    /// <summary>总产量</summary>
    public int TotalCount { get; set; }

    /// <summary>合格品数</summary>
    public int GoodCount { get; set; }

    /// <summary>不良品数</summary>
    public int BadCount { get; set; }

    /// <summary>计划运行时间 (小时)</summary>
    public double PlannedRunTime { get; set; }

    /// <summary>实际运行时间 (小时)</summary>
    public double ActualRunTime { get; set; }

    /// <summary>停机时间 (小时)</summary>
    public double Downtime { get; set; }
}
```

### 9.4 完整代码示例：OEE 计算、汇总与上传

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;
using OmniFrame.Core.AdvancedFeatures;

namespace OmniFrame.Core
{
    /// <summary>
    /// OEE 数据上报服务
    /// 定时计算各产线的 OEE 并上传到 MES
    /// </summary>
    public class OeeReportService : IDisposable
    {
        private readonly IOeeManager _oeeManager;
        private readonly MesClient _mesClient;
        private readonly List<string> _lineNames;
        private System.Threading.Timer _reportTimer;

        // 默认每 15 分钟上报一次 OEE 快照
        private static readonly TimeSpan ReportInterval = TimeSpan.FromMinutes(15);

        public OeeReportService(IOeeManager oeeManager, MesClient mesClient)
        {
            _oeeManager = oeeManager ?? throw new ArgumentNullException(nameof(oeeManager));
            _mesClient = mesClient ?? throw new ArgumentNullException(nameof(mesClient));
            _lineNames = new List<string>();
        }

        /// <summary>
        /// 注册要监控的产线
        /// </summary>
        public void RegisterLine(string lineName)
        {
            if (!_lineNames.Contains(lineName))
            {
                _lineNames.Add(lineName);
                _oeeManager.StartProduction(lineName);
                Logger.Info($"OEE 上报服务已注册产线: {lineName}");
            }
        }

        /// <summary>
        /// 启动定时 OEE 上报
        /// </summary>
        public void Start()
        {
            if (_lineNames.Count == 0)
            {
                Logger.Warning("OEE 上报服务启动失败：未注册任何产线");
                return;
            }

            _reportTimer = new System.Threading.Timer(
                async _ => await ReportOeeForAllLinesAsync(),
                null,
                ReportInterval,   // 首次延迟
                ReportInterval);

            Logger.Info($"OEE 定时上报已启动 (间隔 {ReportInterval.TotalMinutes} 分钟, "
                + $"{_lineNames.Count} 条产线)");
        }

        /// <summary>
        /// 为所有注册产线计算并上报 OEE
        /// </summary>
        private async Task ReportOeeForAllLinesAsync()
        {
            foreach (var lineName in _lineNames.ToList())
            {
                try
                {
                    await ReportOeeForLineAsync(lineName);
                }
                catch (Exception ex)
                {
                    Logger.Error($"OEE 上报异常 (产线: {lineName})", ex);
                }
            }
        }

        /// <summary>
        /// 为单条产线计算并上报 OEE
        /// </summary>
        private async Task<bool> ReportOeeForLineAsync(string lineName)
        {
            try
            {
                // Step 1: 计算 OEE
                double oeeValue = _oeeManager.CalculateOee(lineName);

                if (oeeValue <= 0)
                {
                    Logger.Info($"OEE 上报跳过 (产线 {lineName} 暂无数据)");
                    return true;
                }

                // Step 2: 由于 OeeManager 的 LineOeeData 是 internal 类，
                // 我们需要通过 IOeeManager 接口获取更多数据。
                // 建议扩展 IOeeManager 接口添加: OeeSnapshot GetOeeSnapshot(string lineName);

                // 当前使用 CalculateOee 的返回值和其他公开方法构建 OeeData
                var oeeData = new OeeData
                {
                    LineName = lineName,
                    ReportTime = DateTime.Now,
                    OeeValue = oeeValue,
                    // 以下字段需要通过扩展 IOeeManager 接口获取，
                    // 当前使用估算值。
                    Availability = 0.0,    // TODO: 扩展 IOeeManager.GetAvailability(lineName)
                    Performance = 0.0,     // TODO: 扩展 IOeeManager.GetPerformance(lineName)
                    Quality = 0.0,         // TODO: 扩展 IOeeManager.GetQuality(lineName)
                    TotalCount = 0,        // TODO: 扩展 IOeeManager.GetTotalCount(lineName)
                    GoodCount = 0,         // TODO: 扩展 IOeeManager.GetGoodCount(lineName)
                };

                // Step 3: 上传到 MES
                // 需要先在 MesClient 中添加 UploadOeeDataAsync 方法（见第 10 章）
                // bool success = await _mesClient.UploadOeeDataAsync(oeeData);

                // 临时方案：通过扩展方法或直接 POST
                bool success = await UploadOeeDataViaExtendedClient(oeeData);

                if (success)
                {
                    Logger.Info($"OEE 上报成功: 产线 {lineName}, OEE = {oeeValue}%");
                }
                else
                {
                    Logger.Warning($"OEE 上报失败: 产线 {lineName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.Error($"OEE 上报异常: 产线 {lineName}", ex);
                return false;
            }
        }

        /// <summary>
        /// 临时 OEE 上传方法（在 MesClient 扩展 UploadOeeDataAsync 之前使用）
        /// 直接使用 HttpClient 发送 POST 请求
        /// </summary>
        private async Task<bool> UploadOeeDataViaExtendedClient(OeeData data)
        {
            try
            {
                // 获取 MesClient 内部的 HttpClient（需要 MesClient 暴露或使用反射）
                // 更好的做法是在 MesClient 中添加 UploadOeeDataAsync 方法
                // 此处仅作示例，实际请参考第 10 章扩展 MesClient
                Logger.Info($"OEE 数据 (产线 {data.LineName}): "
                    + $"OEE={data.OeeValue}%, "
                    + $"总产量={data.TotalCount}, "
                    + $"合格={data.GoodCount}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("OEE 数据上传失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 手动触发一次 OEE 上报
        /// </summary>
        public async Task ReportNowAsync()
        {
            Logger.Info("手动触发 OEE 上报...");
            await ReportOeeForAllLinesAsync();
        }

        public void Stop()
        {
            _reportTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            // 停止各产线的 OEE 跟踪
            foreach (var lineName in _lineNames)
            {
                _oeeManager.StopProduction(lineName);
            }

            Logger.Info("OEE 定时上报已停止");
        }

        public void Dispose()
        {
            Stop();
            _reportTimer?.Dispose();
        }
    }
}
```

### 9.5 OEE 扩展建议

建议在 `IOeeManager` 接口中添加获取 OEE 详细快照的方法：

```csharp
// 建议添加到 IOeeManager 接口
public interface IOeeManager
{
    // ... 现有方法 ...

    /// <summary>
    /// 获取产线的 OEE 详细快照（包含 A/P/Q 子指标）
    /// </summary>
    OeeSnapshot GetOeeSnapshot(string lineName);
}

// 建议添加的数据类
public class OeeSnapshot
{
    public string LineName { get; set; }
    public double Availability { get; set; }
    public double Performance { get; set; }
    public double Quality { get; set; }
    public double OeeValue { get; set; }
    public int TotalCount { get; set; }
    public int GoodCount { get; set; }
    public int BadCount { get; set; }
    public TimeSpan AvailableTime { get; set; }
    public TimeSpan PlannedTime { get; set; }
    public TimeSpan Downtime { get; set; }
    public DateTime SnapshotTime { get; set; }
}
```

---

## 10. 扩展 MesClient——添加新 API

`MesClient` 当前提供了基本的 CRUD 方法。在实际项目中，你可能需要添加新的 MES API 端点。下面以添加 `UploadOeeDataAsync` 为例，展示完整的扩展步骤。

### 10.1 Step 1：定义新的数据模型

在 `MesClient.cs` 所在的 `DataAccess` 命名空间中，或在新文件中定义数据模型：

```csharp
// 文件: src/DataAccess/MesModels.cs (新建)
namespace OmniFrame.DataAccess
{
    /// <summary>
    /// OEE 上报数据模型
    /// </summary>
    public class OeeData
    {
        public string LineName { get; set; }
        public DateTime ReportTime { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }
        public double Quality { get; set; }
        public double OeeValue { get; set; }
        public int TotalCount { get; set; }
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
        public double PlannedRunTime { get; set; }
        public double ActualRunTime { get; set; }
        public double Downtime { get; set; }
    }

    /// <summary>
    /// 工艺参数数据模型（另一个扩展示例）
    /// </summary>
    public class RecipeData
    {
        public string RecipeId { get; set; }
        public string ProductCode { get; set; }
        public string RecipeName { get; set; }
        public string Version { get; set; }
        public Dictionary<string, double> Parameters { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
```

### 10.2 Step 2：在 MesClient 中添加新方法

```csharp
// 在 MesClient 类中添加以下方法

/// <summary>
/// 上传 OEE 数据到 MES
/// </summary>
/// <param name="oeeData">OEE 数据</param>
/// <returns>是否上传成功</returns>
public async Task<bool> UploadOeeDataAsync(OeeData oeeData)
{
    try
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(oeeData),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/api/oee/data", content);

        if (response.IsSuccessStatusCode)
        {
            Logger.Info("OEE 数据上传成功");
            return true;
        }

        // 读取错误响应体（有助于调试）
        var errorBody = await response.Content.ReadAsStringAsync();
        Logger.Error($"OEE 数据上传失败 (HTTP {(int)response.StatusCode}): {errorBody}");
        return false;
    }
    catch (HttpRequestException ex)
    {
        Logger.Error("OEE 数据上传网络异常", ex);
        return false;
    }
    catch (TaskCanceledException ex)
    {
        Logger.Error("OEE 数据上传超时", ex);
        return false;
    }
    catch (JsonSerializationException ex)
    {
        Logger.Error("OEE 数据 JSON 序列化失败", ex);
        return false;
    }
    catch (Exception ex)
    {
        Logger.Error("OEE 数据上传失败", ex);
        return false;
    }
}

/// <summary>
/// 下载指定产品的工艺参数
/// </summary>
/// <param name="productCode">产品编码</param>
/// <returns>工艺参数列表</returns>
public async Task<List<RecipeData>> DownloadRecipeAsync(string productCode)
{
    try
    {
        var response = await _httpClient.GetAsync(
            $"/api/recipe?productCode={Uri.EscapeDataString(productCode)}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var recipes = JsonConvert.DeserializeObject<List<RecipeData>>(result);
            Logger.Info($"工艺参数下载成功，数量: {recipes.Count}");
            return recipes;
        }

        Logger.Error($"工艺参数下载失败 (HTTP {(int)response.StatusCode})");
        return new List<RecipeData>();
    }
    catch (JsonSerializationException ex)
    {
        Logger.Error("工艺参数 JSON 反序列化失败", ex);
        return new List<RecipeData>();
    }
    catch (Exception ex)
    {
        Logger.Error("工艺参数下载失败", ex);
        return new List<RecipeData>();
    }
}

/// <summary>
/// 上报产品序列号绑定（Apple 供应链的追溯要求）
/// </summary>
/// <param name="serialNumber">产品序列号</param>
/// <param name="orderId">工单号</param>
/// <param name="stationName">工位名称</param>
/// <param name="testResult">测试结果</param>
/// <returns>是否上报成功</returns>
public async Task<bool> UploadSerialBindingAsync(
    string serialNumber,
    string orderId,
    string stationName,
    bool testResult)
{
    try
    {
        var data = new
        {
            serialNumber,
            orderId,
            stationName,
            testResult,
            timestamp = DateTime.Now
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/api/trace/serial", content);

        if (response.IsSuccessStatusCode)
        {
            Logger.Info($"序列号绑定上报成功: {serialNumber}");
            return true;
        }

        Logger.Error($"序列号绑定上报失败: {serialNumber}");
        return false;
    }
    catch (Exception ex)
    {
        Logger.Error($"序列号绑定上报失败: {serialNumber}", ex);
        return false;
    }
}
```

### 10.3 Step 3：编写错误处理模式

扩展方法需要遵循 `MesClient` 已有的错误处理模式：

1. **捕获特定异常**，最后是通用 `Exception`
2. **读取 HTTP 响应体**中的错误信息用于日志
3. **返回 bool 或空列表**，而非抛出异常（MesClient 为网络容错设计）
4. **通过 `Logger.Error` 记录完整的异常对象**

### 10.4 Step 4：编写单元测试

```csharp
using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using OmniFrame.DataAccess;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class MesClientExtensionTests
    {
        [Test]
        public async Task UploadOeeData_ValidData_ReturnsTrue()
        {
            // 使用 Mock 替代真实 HTTP 请求
            // 此处展示测试结构，实际测试需要使用 HttpMessageHandler Mock
            // 或搭建 MES Mock Server（见第 13 章）

            var oeeData = new OeeData
            {
                LineName = "TestLine",
                OeeValue = 85.5,
                TotalCount = 100,
                GoodCount = 95,
                ReportTime = DateTime.Now
            };

            // TODO: 使用 HttpMessageHandler Mock 验证
            Assert.That(oeeData.OeeValue, Is.EqualTo(85.5));
        }
    }
}
```

---

## 11. 切换 MES 系统供应商

在 Apple 供应链中，不同代工厂可能使用不同的 MES 系统（如 Siemens Opcenter、SAP ME、富士康自研 MES 等）。以下设计允许 OmniFrame 灵活切换 MES 后端。

### 11.1 策略：提取 IMesClient 接口

```csharp
// 文件: src/DataAccess/IMesClient.cs (新建)

namespace OmniFrame.DataAccess
{
    /// <summary>
    /// MES 客户端统一接口
    /// 所有 MES 供应商实现必须实现此接口
    /// </summary>
    public interface IMesClient : IDisposable
    {
        // 认证
        Task<bool> LoginAsync(string username, string password);

        // 生产数据
        Task<bool> UploadProductionDataAsync(ProductionData data);

        // 设备状态
        Task<bool> UploadEquipmentStatusAsync(EquipmentData data);

        // 报警数据
        Task<bool> UploadAlarmDataAsync(AlarmData data);

        // 生产计划
        Task<List<ProductionPlan>> DownloadProductionPlanAsync(string workshopId);

        // 连接测试
        Task<bool> TestConnectionAsync();

        // OEE 数据
        Task<bool> UploadOeeDataAsync(OeeData data);

        // 序列号追溯
        Task<bool> UploadSerialBindingAsync(
            string serialNumber, string orderId,
            string stationName, bool testResult);
    }
}
```

### 11.2 实现不同供应商的客户端

#### Siemens Opcenter (Camstar) 适配器

```csharp
// 文件: src/DataAccess/MesClients/SiemensMesClient.cs (新建)

namespace OmniFrame.DataAccess.MesClients
{
    /// <summary>
    /// Siemens Opcenter (Camstar) MES 适配器
    /// Siemens CAMSTAR MES 使用 SOAP/XML Web Service，而非 REST/JSON
    /// 本适配器将 OmniFrame 的数据模型转换为 Siemens 所需的 XML 格式
    /// </summary>
    public class SiemensMesClient : IMesClient
    {
        private readonly string _baseUrl;
        private HttpClient _httpClient;
        private string _sessionId;

        public SiemensMesClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // Siemens Opcenter 使用 SOAP/XML 认证
                var soapEnvelope = $@"
                    <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                      <soap:Body>
                        <Login xmlns='http://www.siemens.com/opcenter'>
                          <username>{username}</username>
                          <password>{password}</password>
                        </Login>
                      </soap:Body>
                    </soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                var response = await _httpClient.PostAsync("/opcenter/services/Auth", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    // 解析 SOAP 响应获取 sessionId
                    _sessionId = ExtractSessionId(result);
                    Logger.Info("Siemens MES 登录成功");
                    return true;
                }

                Logger.Error("Siemens MES 登录失败");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Siemens MES 登录失败", ex);
                return false;
            }
        }

        public async Task<bool> UploadProductionDataAsync(ProductionData data)
        {
            // 将 ProductionData 转为 Siemens Opcenter 的 ServiceOrder 格式
            // Opcenter 使用 XML 格式而非 JSON
            var xml = ConvertToSiemensProductionXml(data);

            try
            {
                var content = new StringContent(xml, Encoding.UTF8, "text/xml");
                var response = await _httpClient.PostAsync("/opcenter/services/Production", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.Error("Siemens MES 生产数据上传失败", ex);
                return false;
            }
        }

        // ... 其他方法实现类似，将 OmniFrame 模型转为 Siemens XML 格式 ...

        private string ExtractSessionId(string soapResponse)
        {
            // 解析 SOAP XML 提取 sessionId（简化实现）
            return Guid.NewGuid().ToString();
        }

        private string ConvertToSiemensProductionXml(ProductionData data)
        {
            return $@"
                <ProductionData>
                  <OrderId>{data.OrderId}</OrderId>
                  <Quantity>{data.Quantity}</Quantity>
                  <PassQuantity>{data.PassQuantity}</PassQuantity>
                  <FailQuantity>{data.FailQuantity}</FailQuantity>
                </ProductionData>";
        }

        public Task<bool> UploadEquipmentStatusAsync(EquipmentData data) =>
            Task.FromResult(false); // TODO: 实现

        public Task<bool> UploadAlarmDataAsync(AlarmData data) =>
            Task.FromResult(false); // TODO: 实现

        public Task<List<ProductionPlan>> DownloadProductionPlanAsync(string workshopId) =>
            Task.FromResult(new List<ProductionPlan>()); // TODO: 实现

        public Task<bool> TestConnectionAsync() =>
            Task.FromResult(true); // TODO: 实现

        public Task<bool> UploadOeeDataAsync(OeeData data) =>
            Task.FromResult(false); // TODO: 实现

        public Task<bool> UploadSerialBindingAsync(string sn, string orderId, string station, bool result) =>
            Task.FromResult(false); // TODO: 实现

        public void Dispose() => _httpClient?.Dispose();
    }
}
```

### 11.3 在 DI 中配置供应商切换

```csharp
// Program.cs ConfigureServices 中

// 从配置文件读取 MES 供应商类型
var mesVendor = ConfigurationManager.AppSettings["MES:Vendor"] ?? "Default";

switch (mesVendor)
{
    case "Siemens":
        services.AddSingleton<IMesClient>(sp =>
        {
            var url = sp.GetRequiredService<IConfigManager>()
                .GetEncryptedConfig("MesConfig.xml", GetEncryptionKey(),
                    defaultValue: "http://localhost:5000");
            return new SiemensMesClient(url);
        });
        break;

    case "SAP_ME":
        // services.AddSingleton<IMesClient, SapMeMesClient>();
        // break;
        // TODO: 实现 SAP ME 适配器
        Logger.Warning("SAP ME MES 适配器暂未实现，使用默认 MesClient");
        goto default;

    case "Foxconn":
        // 富士康自研 MES 通常兼容 REST/JSON，但端点路径不同
        // services.AddSingleton<IMesClient, FoxconnMesClient>();
        // break;
        Logger.Warning("Foxconn MES 适配器暂未实现，使用默认 MesClient");
        goto default;

    default:
        services.AddSingleton<IMesClient>(sp =>
        {
            var url = sp.GetRequiredService<IConfigManager>()
                .GetEncryptedConfig("MesConfig.xml", GetEncryptionKey(),
                    defaultValue: "http://localhost:5000");
            return new MesClient(url);
        });
        break;
}
```

### 11.4 修改现有代码中的依赖

将所有直接依赖 `MesClient` 的类改为依赖 `IMesClient` 接口：

```csharp
// 修改前:
public class EquipmentStatusReporter
{
    private readonly MesClient _mesClient;
    public EquipmentStatusReporter(MesClient mesClient) { ... }
}

// 修改后:
public class EquipmentStatusReporter
{
    private readonly IMesClient _mesClient;
    public EquipmentStatusReporter(IMesClient mesClient) { ... }
}
```

---

## 12. 故障排查指南

### 12.1 连接被拒绝 (Connection Refused)

**现象：**
```
Logger.Error("MES系统连接测试失败");
// HttpRequestException: No connection could be made because the target machine actively refused it.
```

**可能原因与解决方案：**

| 原因 | 排查方法 | 解决方案 |
|---|---|---|
| MES 服务器未启动 | `ping 192.168.1.50` 确认可达 | 联系 MES 管理员启动服务 |
| 端口号错误 | 检查 `MesConfig.xml` 中的 URL | 修改 URL 中的端口号 |
| 防火墙拦截 | `telnet 192.168.1.50 5000` | 添加防火墙入站规则 |
| URL 配置错误 | 检查解密后的 URL 是否正确 | 重新配置 MesConfig.xml |

**代码中的检测方式：**
```csharp
public async Task<bool> CheckMesConnectivityAsync()
{
    try
    {
        // 先 ping 再 HTTP
        using (var ping = new System.Net.NetworkInformation.Ping())
        {
            var reply = await ping.SendPingAsync("192.168.1.50", 3000);
            if (reply.Status != System.Net.NetworkInformation.IPStatus.Success)
            {
                Logger.Error($"MES 服务器 Ping 不通: {reply.Status}");
                return false;
            }
        }

        // HTTP 健康检查
        return await _mesClient.TestConnectionAsync();
    }
    catch (Exception ex)
    {
        Logger.Error("MES 连通性检查失败", ex);
        return false;
    }
}
```

### 12.2 认证失败 (401 Unauthorized)

**现象：**
```
Logger.Error("MES系统登录失败");
```

**排查步骤：**

1. 检查用户名/密码是否正确：
```csharp
// 临时调试代码：打印凭据（生产环境禁止）
Logger.Info($"尝试登录: user={username}, url={_baseUrl}");
```

2. 检查 Token 响应字段名是否匹配。`LoginAsync` 期望响应中包含 `"token"` 字段：
```json
{ "token": "eyJhbGciOi..." }
```
如果 MES 返回的字段名不同（如 `accessToken` 或 `jwt`），需要修改反序列化逻辑。

3. 检查 Token 是否已被服务器端撤销（如果 MES 支持 Token 黑名单）。

### 12.3 JWT Token 过期

**现象：**
- 之前正常的请求突然返回 401
- `Logger.Error("生产数据上传失败")` 无其他异常信息

**排查方法：**

检查 HTTP 响应状态码：
```csharp
public async Task<bool> UploadProductionDataAsync(ProductionData data)
{
    try
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/api/production/data", content);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Logger.Warning("Token 可能已过期，需要重新登录");
            // 触发重登录逻辑
            return false;
        }

        // ... 其余处理
    }
    // ...
}
```

**解决方案：**
- 调用 `LoginAsync` 重新获取 Token
- 或实现 Token 刷新机制（如果 MES 支持 `/api/auth/refresh`）

### 12.4 请求超时 (Timeout)

**现象：**
```
Logger.Error("MES系统连接测试失败");
// 或 TaskCanceledException after 30 seconds
```

`MesClient` 的 `HttpClient` 设置了 30 秒超时：
```csharp
_httpClient.Timeout = TimeSpan.FromSeconds(30);
```

**排查与解决：**

| 原因 | 现象 | 解决方案 |
|---|---|---|
| 网络延迟过高 | 跨网段通信延迟 > 30s | 增大 Timeout：`_httpClient.Timeout = TimeSpan.FromSeconds(60)` |
| MES 处理慢 | 服务器负载高 | 联系 MES 管理员优化数据库查询 |
| 请求数据量过大 | 批量上传大量产品数据 | 改用分页上传，每次 100 条 |
| DNS 解析慢 | 使用域名而非 IP | 改用 IP 地址，或在 hosts 文件中添加映射 |

### 12.5 JSON 序列化/反序列化错误

**现象：**
```
Newtonsoft.Json.JsonSerializationException: Error converting value...
```

**常见场景与解决：**

1. **字段名不匹配**：MES 返回的 JSON 字段名与 C# 属性名不一致。
```csharp
// C# 属性: ProductCode
// MES 返回: "product_code"
// 解决：使用 JsonProperty 特性
public class ProductionPlan
{
    [JsonProperty("plan_id")]
    public string PlanId { get; set; }

    [JsonProperty("product_code")]
    public string ProductCode { get; set; }

    [JsonProperty("plan_quantity")]
    public int PlanQuantity { get; set; }
}
```

2. **日期格式不匹配**：
```csharp
// 配置全局日期格式
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    DateFormatString = "yyyy-MM-ddTHH:mm:ss",
    DateTimeZoneHandling = DateTimeZoneHandling.Local
};
```

3. **null 值处理**：
```csharp
// 如果 MES 可能返回 null 而非空列表
var response = await _httpClient.GetAsync("/api/production/plan?...");
var result = await response.Content.ReadAsStringAsync();

if (string.IsNullOrEmpty(result) || result.Trim() == "null")
{
    return new List<ProductionPlan>();
}

// 安全反序列化
try
{
    var plans = JsonConvert.DeserializeObject<List<ProductionPlan>>(result);
    return plans ?? new List<ProductionPlan>();
}
catch (JsonSerializationException ex)
{
    Logger.Error($"JSON 反序列化失败: {result.Substring(0, Math.Min(200, result.Length))}", ex);
    return new List<ProductionPlan>();
}
```

### 12.6 MES 返回 HTTP 500

**现象：**
```
Logger.Error("生产数据上传失败");
// HTTP 500 Internal Server Error
```

**排查方法：**

在 `MesClient` 中添加响应体日志：
```csharp
if (!response.IsSuccessStatusCode)
{
    var errorBody = await response.Content.ReadAsStringAsync();
    Logger.Error($"MES 返回错误 (HTTP {(int)response.StatusCode}): {errorBody}");

    if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
    {
        Logger.Error("MES 服务器内部错误，请联系 MES 管理员");

        // 记录请求体用于诊断
        string requestBody = JsonConvert.SerializeObject(data);
        Logger.Error($"导致错误的请求数据: {requestBody}");
    }

    return false;
}
```

### 12.7 故障排查清单

| 检查项 | 命令/方法 | 预期结果 |
|---|---|---|
| MES URL 是否正确解密 | 在代码中临时输出解密后的 URL | 正确的 `http://host:port` |
| 网络可达性 | `ping <MES IP>` | 0% 丢包 |
| 端口可达性 | `telnet <MES IP> <port>` | 连接成功 |
| 健康检查 | `curl http://host:port/api/health` | HTTP 200 |
| 登录测试 | `curl -X POST http://host:port/api/auth/login -H 'Content-Type: application/json' -d '{"username":"...","password":"..."}'` | 返回 Token |
| Token 有效性 | 查看 OmniFrame 日志中 `MES系统登录成功` 日志 | 登录成功记录 |
| 上传测试 | 使用 `curl` 模拟 POST 生产数据 | HTTP 200 |
| 本地日志 | `Logs/log_YYYYMMDD.txt` | 查看完整错误堆栈 |

---

## 13. 无 MES 环境下的开发与测试

在实际开发中，经常需要在没有 MES 服务器的情况下开发和调试功能。以下提供多种策略。

### 13.1 策略 1：使用 OmniFrame.Simulation 模拟

OmniFrame 提供了 `SimulationContext` 来模拟硬件，同样可以扩展它来模拟 MES。

```csharp
// 文件: src/OmniFrame.Simulation/SimulatedMesClient.cs (新建)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniFrame.Common;
using OmniFrame.DataAccess;

namespace OmniFrame.Simulation
{
    /// <summary>
    /// 模拟 MES 客户端
    /// 不发起任何 HTTP 请求，直接返回成功结果和模拟数据
    /// 用于开发环境和单元测试
    /// </summary>
    public class SimulatedMesClient : IMesClient
    {
        private bool _isLoggedIn;
        private readonly List<ProductionPlan> _mockPlans;
        private readonly Random _random = new Random();

        public SimulatedMesClient()
        {
            // 预置一些模拟的生产计划数据
            _mockPlans = new List<ProductionPlan>
            {
                new ProductionPlan
                {
                    PlanId = "PLAN-SIM-001",
                    OrderId = "PO-SIM-20250503-001",
                    ProductCode = "IPHONE17-PRO-BACKPLATE",
                    ProductName = "iPhone 17 Pro 背板组件（模拟）",
                    PlanQuantity = 5000,
                    StartDate = DateTime.Now.Date,
                    EndDate = DateTime.Now.Date.AddHours(12),
                    Status = "生产中"
                },
                new ProductionPlan
                {
                    PlanId = "PLAN-SIM-002",
                    OrderId = "PO-SIM-20250503-002",
                    ProductCode = "IPHONE17-PRO-FRAME",
                    ProductName = "iPhone 17 Pro 中框组件（模拟）",
                    PlanQuantity = 5000,
                    StartDate = DateTime.Now.Date.AddHours(12),
                    EndDate = DateTime.Now.Date.AddDays(1),
                    Status = "待生产"
                }
            };
        }

        public Task<bool> LoginAsync(string username, string password)
        {
            // 模拟登录：接受任意凭据
            _isLoggedIn = true;
            Logger.Info("[模拟MES] 登录成功");
            return Task.FromResult(true);
        }

        public Task<bool> UploadProductionDataAsync(ProductionData data)
        {
            // 模拟数据校验
            if (string.IsNullOrEmpty(data.OrderId))
            {
                Logger.Warning("[模拟MES] 上传失败：订单号为空");
                return Task.FromResult(false);
            }

            // 模拟 5% 的随机失败率（用于测试重试逻辑）
            if (_random.Next(100) < 5)
            {
                Logger.Warning("[模拟MES] 模拟网络故障，上传失败");
                return Task.FromResult(false);
            }

            Logger.Info($"[模拟MES] 生产数据已接收: 订单 {data.OrderId}, "
                + $"总数 {data.Quantity}, 合格 {data.PassQuantity}");
            return Task.FromResult(true);
        }

        public Task<bool> UploadEquipmentStatusAsync(EquipmentData data)
        {
            Logger.Info($"[模拟MES] 设备状态已更新: {data.EquipmentCode} -> {data.Status}");
            return Task.FromResult(true);
        }

        public Task<bool> UploadAlarmDataAsync(AlarmData data)
        {
            Logger.Info($"[模拟MES] 报警已接收: {data.AlarmType} - {data.AlarmDescription}");
            return Task.FromResult(true);
        }

        public Task<List<ProductionPlan>> DownloadProductionPlanAsync(string workshopId)
        {
            Logger.Info($"[模拟MES] 返回 {_mockPlans.Count} 条模拟生产计划");
            return Task.FromResult(_mockPlans);
        }

        public Task<bool> TestConnectionAsync()
        {
            Logger.Info("[模拟MES] 连接测试成功");
            return Task.FromResult(true);
        }

        public Task<bool> UploadOeeDataAsync(OeeData data)
        {
            Logger.Info($"[模拟MES] OEE 数据已接收: 产线 {data.LineName}, OEE={data.OeeValue}%");
            return Task.FromResult(true);
        }

        public Task<bool> UploadSerialBindingAsync(
            string serialNumber, string orderId,
            string stationName, bool testResult)
        {
            Logger.Info($"[模拟MES] 序列号绑定: {serialNumber} (工位: {stationName}, "
                + $"结果: {(testResult ? "OK" : "NG")})");
            return Task.FromResult(true);
        }

        /// <summary>
        /// 模拟网络延迟（用于测试超时处理）
        /// </summary>
        public async Task<bool> UploadWithSimulatedDelayAsync<T>(T data, int delayMs)
        {
            Logger.Info($"[模拟MES] 模拟 {delayMs}ms 网络延迟...");
            await Task.Delay(delayMs);

            // 如果延迟超过 30 秒（客户端超时），模拟超时失败
            if (delayMs > 30000)
            {
                Logger.Error("[模拟MES] 模拟请求超时");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 添加模拟生产计划（用于测试时动态添加）
        /// </summary>
        public void AddMockPlan(ProductionPlan plan)
        {
            _mockPlans.Add(plan);
            Logger.Info($"[模拟MES] 添加模拟计划: {plan.PlanId}");
        }

        /// <summary>
        /// 设置模拟故障率（0-100）
        /// </summary>
        public int FailureRatePercent { get; set; } = 5;

        public void Dispose()
        {
            Logger.Info("[模拟MES] 已释放");
        }
    }
}
```

### 13.2 策略 2：DI 容器切换

在 `Program.cs` 中按环境切换 MesClient 实现：

```csharp
// Program.cs ConfigureServices 中
bool useSimulatedMes = ConfigurationManager.AppSettings["MES:UseSimulation"] == "true"
    || System.Diagnostics.Debugger.IsAttached;  // 调试模式下默认使用模拟

if (useSimulatedMes)
{
    // 使用模拟 MES（无需真实 MES 服务器）
    Logger.Info("使用模拟 MES 客户端");
    services.AddSingleton<SimulatedMesClient>();
    services.AddSingleton<IMesClient>(sp => sp.GetRequiredService<SimulatedMesClient>());
}
else
{
    // 使用真实 MES 连接
    services.AddSingleton<IMesClient>(sp =>
    {
        var config = sp.GetRequiredService<IConfigManager>();
        string encryptionKey = Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY")
            ?? "OmniFrame2024!";
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OMNIFRAME_CONFIG_ENCRYPTION_KEY")))
        {
            Logger.Warning("环境变量 OMNIFRAME_CONFIG_ENCRYPTION_KEY 未设置，使用默认密钥");
        }
        string url = config.GetEncryptedConfig("MesConfig.xml", encryptionKey,
            defaultValue: "http://localhost:5000");
        return new MesClient(url);
    });
}
```

App.config 配置：
```xml
<appSettings>
    <!-- MES 模式: true=模拟, false=真实连接 -->
    <add key="MES:UseSimulation" value="true" />
</appSettings>
```

### 13.3 策略 3：MES Mock Server（进阶）

对于集成测试，可以通过 `HttpListener` 或 ASP.NET Core 搭建一个轻量级的 MES Mock Server：

```csharp
// 文件: tests/OmniFrame.Tests/Helpers/MesMockServer.cs (新建)

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OmniFrame.Tests.Helpers
{
    /// <summary>
    /// MES Mock Server
    /// 在本地端口上启动一个 HTTP 服务器，完全模拟 MES 的 API 行为
    /// 用于集成测试
    /// </summary>
    public class MesMockServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly int _port;
        private readonly CancellationTokenSource _cts;
        private bool _isRunning;

        public string BaseUrl => $"http://localhost:{_port}";

        // 可配置的响应行为
        public bool SimulateAuthFailure { get; set; }
        public bool SimulateTimeout { get; set; }
        public int SimulatedDelayMs { get; set; }

        public MesMockServer(int port = 55000)
        {
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_port}/");
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _isRunning = true;
            Logger.Info($"[MockMES] 服务器已启动: {BaseUrl}");

            _ = Task.Run(() => ListenLoop(_cts.Token));
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context), token);
                }
                catch (HttpListenerException)
                {
                    break; // 监听器已停止
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // 模拟延迟
            if (SimulatedDelayMs > 0)
            {
                await Task.Delay(SimulatedDelayMs);
            }

            // 模拟超时
            if (SimulateTimeout)
            {
                await Task.Delay(35000); // 超过 MesClient 的 30s 超时
                return;
            }

            try
            {
                string responseBody = "";
                int statusCode = 200;

                // 路由分发
                switch (request.Url.AbsolutePath)
                {
                    case "/api/health":
                        responseBody = "{\"status\": \"healthy\"}";
                        break;

                    case "/api/auth/login":
                        if (SimulateAuthFailure)
                        {
                            statusCode = 401;
                            responseBody = "{\"error\": \"Invalid credentials\"}";
                        }
                        else
                        {
                            responseBody = "{\"token\": \"mock-jwt-token-"
                                + Guid.NewGuid().ToString("N") + "\", \"expiresIn\": 86400}";
                        }
                        break;

                    case "/api/production/data":
                        responseBody = "{\"success\": true, \"message\": \"生产数据已接收\", \"recordId\": \"REC-MOCK-00001\"}";
                        break;

                    case "/api/equipment/status":
                        responseBody = "{\"success\": true, \"message\": \"设备状态已更新\"}";
                        break;

                    case "/api/alarm/data":
                        responseBody = "{\"success\": true, \"message\": \"报警数据已接收\"}";
                        break;

                    case "/api/production/plan":
                        var plans = new[]
                        {
                            new { PlanId = "PLAN-MOCK-001", OrderId = "PO-MOCK-001",
                                ProductCode = "MOCK-PROD", ProductName = "模拟产品",
                                PlanQuantity = 1000, StartDate = DateTime.Now,
                                EndDate = DateTime.Now.AddDays(1), Status = "生产中" }
                        };
                        responseBody = JsonConvert.SerializeObject(plans);
                        break;

                    default:
                        statusCode = 404;
                        responseBody = "{\"error\": \"Not found\"}";
                        break;
                }

                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("[MockMES] 处理请求失败", ex);
                response.StatusCode = 500;
                response.OutputStream.Close();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();
            _listener.Stop();
            Logger.Info("[MockMES] 服务器已停止");
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}
```

### 13.4 使用 Mock Server 的集成测试

```csharp
using NUnit.Framework;
using OmniFrame.DataAccess;
using OmniFrame.Tests.Helpers;
using System;
using System.Threading.Tasks;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class MesClientIntegrationTests
    {
        private MesMockServer _mockServer;
        private MesClient _client;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _mockServer = new MesMockServer(55001);
            await _mockServer.StartAsync();
        }

        [SetUp]
        public void SetUp()
        {
            _client = new MesClient(_mockServer.BaseUrl);
            _mockServer.SimulateAuthFailure = false;
            _mockServer.SimulateTimeout = false;
            _mockServer.SimulatedDelayMs = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _mockServer?.Dispose();
        }

        [Test]
        public async Task TestConnection_ServerRunning_ReturnsTrue()
        {
            bool result = await _client.TestConnectionAsync();
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsTrue()
        {
            bool result = await _client.LoginAsync("test", "test");
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsFalse()
        {
            _mockServer.SimulateAuthFailure = true;
            bool result = await _client.LoginAsync("test", "wrong");
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UploadProductionData_ValidData_ReturnsTrue()
        {
            await _client.LoginAsync("test", "test");

            var data = new ProductionData
            {
                OrderId = "PO-TEST-001",
                ProductCode = "TEST-PROD",
                Quantity = 100,
                PassQuantity = 95,
                FailQuantity = 5,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                Operator = "TestUser"
            };

            bool result = await _client.UploadProductionDataAsync(data);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DownloadProductionPlan_ReturnsPlans()
        {
            await _client.LoginAsync("test", "test");

            var plans = await _client.DownloadProductionPlanAsync("WS-TEST");

            Assert.That(plans, Is.Not.Null);
            Assert.That(plans.Count, Is.GreaterThan(0));
            Assert.That(plans[0].ProductCode, Is.EqualTo("MOCK-PROD"));
        }

        [Test]
        public async Task UploadProductionData_Timeout_ReturnsFalse()
        {
            _mockServer.SimulateTimeout = true;

            await _client.LoginAsync("test", "test");

            var data = new ProductionData
            {
                OrderId = "PO-TEST-001",
                ProductCode = "TEST-PROD",
                Quantity = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };

            bool result = await _client.UploadProductionDataAsync(data);
            Assert.That(result, Is.False); // 超时应该返回 false 而非抛出异常
        }
    }
}
```

---

## 附录 A：MesClient API 速查表

| 方法 | HTTP 方法 | 端点 | 请求体 | 响应类型 |
|---|---|---|---|---|
| `LoginAsync` | POST | `/api/auth/login` | `{ username, password }` | `bool` |
| `UploadProductionDataAsync` | POST | `/api/production/data` | `ProductionData` | `bool` |
| `UploadEquipmentStatusAsync` | POST | `/api/equipment/status` | `EquipmentData` | `bool` |
| `UploadAlarmDataAsync` | POST | `/api/alarm/data` | `AlarmData` | `bool` |
| `DownloadProductionPlanAsync` | GET | `/api/production/plan?workshopId={id}` | N/A | `List<ProductionPlan>` |
| `TestConnectionAsync` | GET | `/api/health` | N/A | `bool` |

## 附录 B：关键依赖注入注册

| 类型 | 接口 | 生命周期 | 文件位置 |
|---|---|---|---|
| `MesClient` | (无接口) | Singleton | `src/DataAccess/MesClient.cs` |
| `ProductionManager` | `IProductionManager` | Singleton | `src/OmniFrame.Core/ProductionManager.cs` |
| `AlarmManager` | `IAlarmManager` | Singleton | `src/OmniFrame.Core/AlarmManager.cs` |
| `OeeManager` | `IOeeManager` | Singleton | `src/OmniFrame.Core/AdvancedFeatures/OeeManager.cs` |
| `UphManager` | `IUphManager` | Singleton | `src/OmniFrame.Core/AdvancedFeatures/UphManager.cs` |
| `ConfigManager` | `IConfigManager` | Singleton | `src/OmniFrame.Core/ConfigManager.cs` |
| `RemoteMonitorManager` | `IRemoteMonitorManager` | Singleton | `src/RemoteMonitor/RemoteMonitorManager.cs` |

## 附录 C：日志关键字速查

在排查 MES 集成问题时，可以在日志文件（`Logs/log_YYYYMMDD.txt`）中搜索以下关键字：

| 关键字 | 含义 |
|---|---|
| `MES系统登录成功` | JWT Token 获取成功 |
| `MES系统登录失败` | 认证失败（凭据错误或网络不通） |
| `生产数据上传成功` | 生产数据已成功推送到 MES |
| `生产数据上传失败` | 推送失败（检查网络或数据格式） |
| `MES系统连接测试成功` | TCP + HTTP 连通性正常 |
| `MES系统连接测试失败` | 无法连接到 MES 服务器 |
| `生产计划下载成功` | 计划数据已获取 |
| `OMNIFRAME_CONFIG_ENCRYPTION_KEY 未设置` | 使用了默认密钥的安全警告 |
| `OEE` | OEE 计算和上报相关的日志 |
| `Token` | Token 过期或重登录相关日志 |

---

> **文档版本**: v1.0
> **适用于**: OmniFrame v1.7+
> **维护者**: OmniFrame 开发团队
> **最后更新**: 2025-05-03
