# 快速开始指南

## 5分钟上手AOIFrame Lite

### 前置要求

- **.NET Framework 4.8** 或更高版本
- **Visual Studio 2019+** 或 **Visual Studio Code**
- 根据硬件配置相应的驱动DLL（研华/科运通/汇川等）

### 安装步骤

#### 1. 克隆或下载项目

```bash
git clone https://github.com/yourusername/AOIFrame-Lite.git
cd AOIFrame-Lite
```

#### 2. 打开解决方案

```bash
# 使用Visual Studio
start AOIFrame-Lite.sln

# 或使用命令行编译
dotnet build AOIFrame-Lite.sln
```

#### 3. 编译项目

```bash
# Debug编译
Build → Build Solution (Ctrl+Shift+B)

# 或Release编译
Build → Configuration Manager → 选择Release
```

### 最小化示例

#### 示例1：初始化系统与读取轴位置

```csharp
using AOIFrame.Core;
using AOIFrame.Common;

class Program
{
    static void Main()
    {
        try
        {
            // 1. 初始化配置管理器
            ConfigManager.Instance.Initialize("Config/SystemCfg.xml");
            
            // 2. 初始化运动控制管理器
            MotionMgr.Instance.Initialize();
            
            // 3. 读取第0号轴的当前位置
            double position = MotionMgr.Instance.GetAxisPosition(0);
            Console.WriteLine($"轴0当前位置: {position} mm");
            
            // 4. 设置轴的运动参数
            MotionMgr.Instance.SetAxisVelocity(0, 100.0);   // 速度: 100 mm/s
            MotionMgr.Instance.SetAxisAccel(0, 500.0);      // 加速度: 500 mm/s²
            
            // 5. 执行绝对运动
            MotionMgr.Instance.MoveAbsolute(0, 50.0);
            
            // 6. 等待运动完成（最长5秒）
            bool done = MotionMgr.Instance.WaitMotionDone(0, timeout: 5000);
            if (done)
                Console.WriteLine("运动完成");
            else
                Console.WriteLine("运动超时");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}
```

#### 示例2：IO操作

```csharp
using AOIFrame.Core;

// 初始化IO管理器
IoMgr.Instance.Initialize();

// 读取DI（数字输入）
bool diStatus = IoMgr.Instance.ReadDI(0);
Console.WriteLine($"DI0状态: {diStatus}");

// 写入DO（数字输出）
IoMgr.Instance.WriteDO(0, true);  // 设置DO0为高
IoMgr.Instance.WriteDO(0, false); // 设置DO0为低

// 读取AI（模拟输入）
double aiValue = IoMgr.Instance.ReadAI(0);
Console.WriteLine($"AI0值: {aiValue} V");

// 写入AO（模拟输出）
IoMgr.Instance.WriteAO(0, 5.0);   // 设置AO0输出5V

// 订阅IO变化事件
IoMgr.Instance.OnDIChanged += (index, newState) =>
{
    Console.WriteLine($"DI[{index}]变化 -> {newState}");
};
```

#### 示例3：配置管理

```csharp
using AOIFrame.Core;

// 初始化配置管理器
ConfigManager.Instance.Initialize("Config/SystemCfg.xml");

// 读取配置
string dbConnection = ConfigManager.Instance.GetConfig("Database", "ConnectionString");
int timeout = ConfigManager.Instance.GetConfig<int>("System", "Timeout");

// 修改配置
ConfigManager.Instance.SetConfig("Machine", "MaxSpeed", 500);
ConfigManager.Instance.SetConfig("Machine", "Enabled", true);

// 保存配置到文件
ConfigManager.Instance.SaveConfig();
```

#### 示例4：数字孪生连接

```csharp
using AOIFrame.Communication;
using System.Threading.Tasks;

async Task SetupDigitalTwin()
{
    // 创建数字孪生桥接器
    var bridge = new DigitalTwinBridge("ws://localhost:3001");
    
    try
    {
        // 连接WebSocket服务器
        await bridge.ConnectAsync();
        
        // 推送轴位置（100ms更新一次）
        Task.Run(async () =>
        {
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    double pos = MotionMgr.Instance.GetAxisPosition(i);
                    bridge.PushAxisPosition(i, pos);
                }
                await Task.Delay(100);
            }
        });
        
        // 推送IO状态（500ms更新一次）
        Task.Run(async () =>
        {
            while (true)
            {
                for (int i = 0; i < 16; i++)
                {
                    bool state = IoMgr.Instance.ReadDI(i);
                    bridge.PushIOStatus($"di_{i}", state);
                }
                await Task.Delay(500);
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"连接失败: {ex.Message}");
    }
}
```

#### 示例5：加载自定义硬件驱动

```csharp
using System.Reflection;
using AOIFrame.Hardware;

// 动态加载运动卡驱动
void LoadMotionDriver(string dllPath, string driverTypeName)
{
    Assembly driverAsm = Assembly.LoadFrom(dllPath);
    Type driverType = driverAsm.GetType(driverTypeName);
    
    if (driverType == null)
        throw new Exception($"找不到类型: {driverTypeName}");
    
    // 创建驱动实例
    IMotionDevice device = (IMotionDevice)Activator.CreateInstance(driverType);
    
    // 初始化驱动
    device.Initialize(new Dictionary<string, string>
    {
        { "port", "COM1" },
        { "baudrate", "115200" }
    });
    
    // 注册到运动管理器
    MotionMgr.Instance.RegisterDevice(device);
}

// 使用示例
LoadMotionDriver("Motion_DMC3000.dll", "Motion.DMC3000.MotionController");
```

### 配置文件示例

#### SystemCfg.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <System>
    <Name>AOI检测系统</Name>
    <Version>1.0</Version>
    <Timeout>5000</Timeout>
  </System>
  
  <Database>
    <ConnectionString>Server=localhost;Database=aoi_db;User Id=sa;Password=123456;</ConnectionString>
    <MaxConnections>10</MaxConnections>
  </Database>
  
  <Motion>
    <DeviceCount>1</DeviceCount>
    <Device id="0">
      <Type>Motion_DMC3000</Type>
      <Port>COM1</Port>
      <Baudrate>115200</Baudrate>
      <AxisCount>4</AxisCount>
    </Device>
  </Motion>
  
  <IO>
    <DICount>16</DICount>
    <DOCount>16</DOCount>
    <AICount>8</AICount>
    <AOCount>8</AOCount>
  </IO>
  
  <DigitalTwin>
    <Enabled>true</Enabled>
    <WebSocketUrl>ws://localhost:3001</WebSocketUrl>
    <AxisUpdateInterval>100</AxisUpdateInterval>
    <IOUpdateInterval>500</IOUpdateInterval>
  </DigitalTwin>
</Configuration>
```

### 常见问题

**Q: 运动控制不工作？**

A: 检查以下几点：
1. 确认硬件驱动DLL在正确位置
2. 检查Configuration Manager中的Device配置
3. 查看日志文件获取详细错误信息
4. 确认硬件连接正常（串口/网络）

**Q: 如何连接不同的PLC？**

A: AOIFrame Lite支持多种PLC协议，在通信层配置：

```csharp
// 三菱PLC (ModbusTCP)
PlcLink plc = new PlcLink("三菱Q系列");
plc.Connect("192.168.1.100", 502);

// 汇川PLC
PlcLink plc = new PlcLink("汇川H5U");
plc.Connect("COM1", 115200);

// 基恩士PLC
PlcLink plc = new PlcLink("基恩士KV");
plc.Connect("192.168.1.50", 8000);
```

**Q: 如何扩展功能？**

A: 通过插件机制：

```csharp
// 1. 创建插件项目，实现 IPlugin 接口
public class MyPlugin : IPlugin
{
    public void Initialize(IServiceProvider services) { }
    public void Execute(Dictionary<string, object> parameters) { }
}

// 2. 编译成DLL
// 3. 加载插件
PluginManager.Instance.LoadPlugin("MyPlugin.dll");
```

### 下一步

- 📖 查看 [API参考文档](api-reference.md)
- 🏗️ 阅读 [架构设计文档](architecture.md)
- 💡 浏览 [示例代码](../examples/)
- 🐛 报告问题到 [Issues](https://github.com/yourusername/AOIFrame-Lite/issues)

