# 安装指南

## 系统要求

### 最低配置

| 项目 | 要求 |
|------|------|
| **操作系统** | Windows 7 SP1 / Windows 10 / Windows 11 / Windows Server 2016+ |
| **.NET Framework** | 4.8+ |
| **内存** | 1 GB (建议 4 GB) |
| **存储空间** | 500 MB |
| **显示器** | 1024×768 分辨率 |

### 开发环境

| 组件 | 版本 |
|------|------|
| **Visual Studio** | 2019 / 2022 |
| **Visual Studio Code** | 1.50+ (需要C#扩展) |
| **.NET SDK** | 4.8+ Framework |

### 硬件支持（可选）

- **运动控制卡**：研华 DMC3000/4000、科运通 GTS、汇川 ECAT等
- **PLC**：三菱/西门子/汇川/基恩士 等主流品牌
- **通信接口**：COM串口、以太网、CAN总线

---

## 安装步骤

### 方案一：从GitHub克隆（推荐）

#### 1. 克隆项目

```bash
git clone https://github.com/yourusername/AOIFrame-Lite.git
cd AOIFrame-Lite
```

#### 2. 恢复NuGet依赖包

```bash
# 方式1：使用Visual Studio
右键点击解决方案 → 还原NuGet程序包

# 方式2：使用命令行
dotnet restore AOIFrame-Lite.sln
```

#### 3. 编译项目

```bash
# 方式1：使用Visual Studio
打开Visual Studio → 构建 → 生成解决方案 (Ctrl+Shift+B)

# 方式2：使用命令行
dotnet build AOIFrame-Lite.sln --configuration Release
```

#### 4. 运行应用

```bash
# 使用Visual Studio
按 F5 或 调试 → 开始调试

# 或者直接运行编译后的exe
bin/Release/AOIFrame.exe
```

---

### 方案二：从Release包安装

#### 1. 下载Release包

访问 [Releases](https://github.com/yourusername/AOIFrame-Lite/releases) 页面，下载最新版本

```bash
# 或使用命令行下载
wget https://github.com/yourusername/AOIFrame-Lite/releases/download/v1.0.0/AOIFrame-Lite-1.0.0.zip
unzip AOIFrame-Lite-1.0.0.zip
cd AOIFrame-Lite-1.0.0
```

#### 2. 安装驱动依赖（如需要）

如果使用硬件加速，安装相应的驱动DLL到 `lib/drivers/` 目录

#### 3. 配置系统参数

编辑 `Config/SystemCfg.xml`，根据实际硬件修改配置

#### 4. 运行应用

```bash
# Windows
AOIFrame.exe

# 或双击 AOIFrame.exe
```

---

## 配置文件说明

### 目录结构

```
AOIFrame-Lite/
├── Config/                    # 配置文件夹
│   ├── SystemCfg.xml         # 系统主配置
│   ├── MotionCfg.ini          # 运动参数配置
│   ├── IOMapping.xml          # IO映射配置
│   └── Database.xml           # 数据库配置
├── bin/
│   ├── Debug/                 # Debug编译输出
│   └── Release/               # Release编译输出
├── lib/
│   ├── drivers/               # 硬件驱动DLL目录
│   └── plugins/               # 插件DLL目录
├── docs/                      # 文档文件夹
├── examples/                  # 示例代码
├── Logs/                      # 日志输出目录
└── README.md                  # 项目说明
```

### SystemCfg.xml 配置示例

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <!-- 系统基本信息 -->
  <System>
    <Name>精密检测系统</Name>
    <Version>1.0.0</Version>
    <Description>基于AOIFrame的高精度光学检测系统</Description>
    <Language>zh-CN</Language>
    <Timeout>5000</Timeout>
    <LogLevel>Info</LogLevel>
  </System>

  <!-- 数据库配置 -->
  <Database>
    <Provider>SqlServer</Provider>
    <ConnectionString>Server=localhost;Database=AOI_DB;User Id=sa;Password=123456;</ConnectionString>
    <MaxConnections>10</MaxConnections>
    <CommandTimeout>30</CommandTimeout>
  </Database>

  <!-- 运动控制配置 -->
  <Motion>
    <DeviceCount>1</DeviceCount>
    
    <!-- 运动卡0配置 -->
    <Device id="0">
      <Type>Motion_DMC3000</Type>
      <Port>COM1</Port>
      <Baudrate>115200</Baudrate>
      <AxisCount>4</AxisCount>
      <AxisNames>X,Y,Z,R</AxisNames>
      
      <!-- 轴参数配置 -->
      <Axis id="0">
        <Name>X轴</Name>
        <MaxVelocity>500.0</MaxVelocity>      <!-- mm/s -->
        <MaxAccel>2000.0</MaxAccel>            <!-- mm/s² -->
        <MaxTravel>500.0</MaxTravel>           <!-- mm -->
        <HomeMethod>0</HomeMethod>             <!-- 回零方式 -->
      </Axis>
      
      <Axis id="1">
        <Name>Y轴</Name>
        <MaxVelocity>500.0</MaxVelocity>
        <MaxAccel>2000.0</MaxAccel>
        <MaxTravel>500.0</MaxTravel>
      </Axis>
      
      <Axis id="2">
        <Name>Z轴</Name>
        <MaxVelocity>300.0</MaxVelocity>
        <MaxAccel>1500.0</MaxAccel>
        <MaxTravel>200.0</MaxTravel>
      </Axis>
      
      <Axis id="3">
        <Name>R轴</Name>
        <MaxVelocity>360.0</MaxVelocity>
        <MaxAccel>1800.0</MaxAccel>
        <MaxTravel>360.0</MaxTravel>
      </Axis>
    </Device>
  </Motion>

  <!-- IO配置 -->
  <IO>
    <DICount>16</DICount>
    <DOCount>16</DOCount>
    <AICount>8</AICount>
    <AOCount>8</AOCount>
    
    <!-- 数字输入映射 -->
    <DIMap>
      <Map index="0" name="Emergency_Stop" />
      <Map index="1" name="Start_Button" />
      <Map index="2" name="Stop_Button" />
      <Map index="3" name="Motor_Ready" />
    </DIMap>
    
    <!-- 数字输出映射 -->
    <DOMap>
      <Map index="0" name="Motor_Enable" />
      <Map index="1" name="Light_Enable" />
      <Map index="2" name="Alarm_Output" />
    </DOMap>
  </IO>

  <!-- PLC通信配置 -->
  <PLC>
    <Enabled>true</Enabled>
    <Protocol>ModbusTCP</Protocol>
    <IPAddress>192.168.1.100</IPAddress>
    <Port>502</Port>
    <SlaveID>1</SlaveID>
    <Timeout>5000</Timeout>
  </PLC>

  <!-- 数字孪生配置 -->
  <DigitalTwin>
    <Enabled>true</Enabled>
    <WebSocketUrl>ws://localhost:3001</WebSocketUrl>
    <ReconnectInterval>5000</ReconnectInterval>
    <AxisUpdateInterval>100</AxisUpdateInterval>
    <IOUpdateInterval>500</IOUpdateInterval>
    <MachineStatusUpdateInterval>1000</MachineStatusUpdateInterval>
  </DigitalTwin>

  <!-- 日志配置 -->
  <Logging>
    <Path>./Logs</Path>
    <Level>Info</Level>
    <MaxFileSize>10485760</MaxFileSize>  <!-- 10MB -->
    <ArchivePath>./Logs/Archive</ArchivePath>
  </Logging>
</Configuration>
```

---

## 硬件驱动安装

### 支持的硬件设备

#### 运动控制卡

- **研华 DMC3000/4000** - ModuleConnect模块化运动控制
- **科运通 GTS** - 高可靠性运动卡
- **汇川 ECAT从站** - EtherCAT实时通信

#### PLC支持

| PLC品牌 | 协议类型 | 支持状态 |
|---------|--------|--------|
| 三菱 | MC/QPLC | ✅ 完全支持 |
| 西门子 | S7/STEP7 | ✅ 完全支持 |
| 汇川 | Inovance | ✅ 完全支持 |
| 基恩士 | HostLink | ✅ 完全支持 |
| 欧姆龙 | FINS | ✅ 完全支持 |

### 安装驱动

#### 1. 复制驱动文件

```bash
# 将硬件驱动DLL复制到lib/drivers目录
cp Motion_DMC3000.dll ./lib/drivers/
cp Motion_GTS.dll ./lib/drivers/
```

#### 2. 配置驱动参数

在 `Config/SystemCfg.xml` 中配置相应参数：

```xml
<Motion>
  <Device id="0">
    <Type>Motion_DMC3000</Type>
    <Port>COM1</Port>
    <Baudrate>115200</Baudrate>
  </Device>
</Motion>
```

#### 3. 验证驱动

应用启动时会自动检测并加载驱动，检查日志确认加载成功

---

## 常见安装问题

### 问题 1：无法找到 .NET Framework 4.8

**解决方案：**

```bash
# 下载并安装 .NET Framework 4.8
# https://dotnet.microsoft.com/download/dotnet-framework/net48

# 验证安装
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"
```

### 问题 2：NuGet 包还原失败

**解决方案：**

```bash
# 清除NuGet缓存
nuget locals all -clear

# 重新还原包
dotnet restore AOIFrame-Lite.sln

# 或使用Visual Studio
# 工具 → 选项 → NuGet包管理器 → 包源 → 检查源
```

### 问题 3：编译错误 - 无法加载驱动DLL

**解决方案：**

1. 确认DLL文件存在于 `lib/drivers/` 目录
2. 检查DLL文件是否被损坏或不兼容
3. 查看项目输出窗口的详细错误信息
4. 如果使用32位系统，确保使用32位DLL；64位系统使用64位DLL

### 问题 4：应用启动闪退

**解决方案：**

```bash
# 查看错误日志
# 检查 ./Logs/ 目录下的日志文件

# 检查配置文件是否有效
# 运行配置验证工具
ConfigValidator.exe Config/SystemCfg.xml
```

### 问题 5：COM端口访问被拒绝

**解决方案：**

```bash
# Windows: 以管理员身份运行应用
# 右键点击AOIFrame.exe → 以管理员身份运行

# Linux/Mac: 赋予端口权限
sudo chmod 666 /dev/ttyUSB0
```

---

## Docker 部署（可选）

### 创建 Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8

WORKDIR /app

# 复制编译后的应用
COPY bin/Release . 

# 复制配置文件
COPY Config ./Config
COPY lib ./lib

# 设置环境变量
ENV MACHINE_NAME="AOIFrame_Docker"

# 运行应用
ENTRYPOINT ["AOIFrame.exe"]
```

### 构建和运行

```bash
# 构建镜像
docker build -t aoiframe:latest .

# 运行容器
docker run -d \
  --name aoiframe_container \
  -p 3001:3001 \
  -v $(pwd)/Config:/app/Config \
  -v $(pwd)/Logs:/app/Logs \
  aoiframe:latest
```

---

## 升级指南

### 从旧版本升级

```bash
# 1. 备份当前配置和数据
cp -r Config Config.backup
cp -r Logs Logs.backup

# 2. 更新代码
git pull origin main
git checkout v2.0.0  # 指定要升级的版本

# 3. 重新编译
dotnet build --configuration Release

# 4. 检查配置文件兼容性
# 比较 Config.backup/ 和 Config/，合并必要的修改

# 5. 测试应用
# 在测试环境验证功能正常
```

---

## 技术支持

- 📖 [文档中心](../README.md)
- 🐛 [Issue Tracker](https://github.com/yourusername/AOIFrame-Lite/issues)
- 💬 [讨论区](https://github.com/yourusername/AOIFrame-Lite/discussions)
- 📧 联系方式：support@example.com

