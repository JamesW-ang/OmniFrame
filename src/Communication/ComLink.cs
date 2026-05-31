using System.IO;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using OmniFrame.Common;

namespace OmniFrame.Communication
{
    /// <summary>
    /// 串口通信类
    /// 设计介绍：
    /// 2. 实现线程安全设计，使用锁机制确保多线程环境下的数据安全
    /// 4. 支持同步和异步读写操作，适应不同的应用场景
    /// 5. 实现资源自动管理，确保串口资源的正确释放
    /// 6. 提供完善的错误处理和日志记录，便于问题排查
    /// 7. 支持超时设置，提高通信的可靠性
        /// </summary>
    public class ComLink
    {
        /// <summary>
        /// 串口号
        /// </summary>
        public int ComNo { get; private set; }

        /// <summary>
        /// 串口名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 读取超时时间（毫秒）
        /// </summary>
        public int ReadTimeout { get; set; }

        /// <summary>
        /// 写入超时时间（毫秒）
        /// </summary>
        public int WriteTimeout { get; set; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpen => _serialPort?.IsOpen ?? false;

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<byte[]> DataReceived;

        /// <summary>
        /// 串口对象
        /// </summary>
        private SerialPort _serialPort;

        /// <summary>
        /// 线程锁对象
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="comNo">串口号</param>
        /// <param name="name">串口名称</param>
        /// <param name="baudRate">波特率，默认9600</param>
        public ComLink(int comNo, string name, int baudRate = 9600)
        {
            ComNo = comNo;
            Name = name;
            BaudRate = baudRate;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.One;
            ReadTimeout = 5000;
            WriteTimeout = 5000;
        }

        /// <summary>
        /// 打开串口
        /// 功能说明：打开指定的串口，配置通信参数，并订阅数据接收事件
        /// 设计模式：使用双重检查锁定模式，确保线程安全
        /// </summary>
        /// <returns>是否打开成功</returns>
        public bool Open()
        {
            lock (_lock)
            {
                try
                {
                    // 如果串口已经打开，直接返回成功
                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        return true;
                    }

                    // 关闭已存在的串口
                    CloseInternal();

                    // 创建新的串口实例并配置参数
                    _serialPort = new SerialPort($"COM{ComNo}", BaudRate, Parity, DataBits, StopBits);
                    _serialPort.ReadTimeout = ReadTimeout;
                    _serialPort.WriteTimeout = WriteTimeout;
                    _serialPort.DataReceived += OnDataReceived;

                    // 打开串口
                    _serialPort.Open();
                    Logger.Info($"串口打开成功: {Name} (COM{ComNo}, {BaudRate})");
                    return true;
                }
                catch (IOException ex)
                {
                    Logger.Error($"串口打开失败(IO错误): {Name} (COM{ComNo})", ex);
                    return false;
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"串口打开失败(超时): {Name} (COM{ComNo})", ex);
                    return false;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error($"串口打开失败(操作无效): {Name} (COM{ComNo})", ex);
                    return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Error($"串口打开失败(权限不足): {Name} (COM{ComNo})", ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"串口打开失败: {Name} (COM{ComNo})", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 关闭串口
        /// 功能说明：关闭串口连接，释放资源
        /// 设计模式：使用锁机制确保线程安全
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                CloseInternal();
            }
        }

        /// <summary>
        /// 内部关闭方法
        /// 功能说明：执行实际的关闭操作，包括取消事件订阅和资源释放
        /// </summary>
        private void CloseInternal()
        {
            try
            {
                if (_serialPort != null)
                {
                    // 取消事件订阅
                    _serialPort.DataReceived -= OnDataReceived;
                    
                    // 如果串口已打开，先关闭
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    
                    // 释放资源
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch (IOException ex)
            {
                Logger.Error($"关闭串口失败(IO错误): {Name}", ex);
            }
            catch (TimeoutException ex)
            {
                Logger.Error($"关闭串口失败(超时): {Name}", ex);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"关闭串口失败(操作无效): {Name}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error($"关闭串口失败(权限不足): {Name}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"关闭串口失败: {Name}", ex);
            }
        }

        /// <summary>
        /// 数据接收事件处理
        /// 功能说明：处理串口数据接收事件，读取数据并触发DataReceived事件
        /// 设计模式：事件驱动模式
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // 读取可用字节数
                    int bytesToRead = _serialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);
                    
                    // 触发数据接收事件
                    DataReceived?.Invoke(this, buffer);
                }
            }
            catch (IOException ex)
            {
                Logger.Error($"串口接收数据失败(IO错误): {Name}", ex);
            }
            catch (TimeoutException ex)
            {
                Logger.Error($"串口接收数据失败(超时): {Name}", ex);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"串口接收数据失败(操作无效): {Name}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error($"串口接收数据失败(权限不足): {Name}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"串口接收数据失败: {Name}", ex);
            }
        }

        /// <summary>
        /// 写入数据
        /// 功能说明：向串口写入指定长度的数据
        /// 设计模式：使用锁机制确保线程安全
        /// </summary>
        /// <param name="data">要写入的数据</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="length">要写入的长度</param>
        /// <returns>是否写入成功</returns>
        public bool Write(byte[] data, int offset, int length)
        {
            lock (_lock)
            {
                if (!IsOpen)
                    return false;

                try
                {
                    _serialPort.Write(data, offset, length);
                    return true;
                }
                catch (IOException ex)
                {
                    Logger.Error($"串口写入失败(IO错误): {Name}", ex);
                    return false;
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"串口写入失败(超时): {Name}", ex);
                    return false;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error($"串口写入失败(操作无效): {Name}", ex);
                    return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Error($"串口写入失败(权限不足): {Name}", ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"串口写入失败: {Name}", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 写入数据
        /// 功能说明：向串口写入完整的字节数组
        /// </summary>
        /// <param name="data">要写入的数据</param>
        /// <returns>是否写入成功</returns>
        public bool Write(byte[] data)
        {
            return Write(data, 0, data.Length);
        }

        /// <summary>
        /// 写入一行数据
        /// 功能说明：向串口写入一行字符串数据
        /// </summary>
        /// <param name="data">要写入的字符串</param>
        /// <returns>是否写入成功</returns>
        public bool WriteLine(string data)
        {
            lock (_lock)
            {
                if (!IsOpen)
                    return false;

                try
                {
                    _serialPort.WriteLine(data);
                    return true;
                }
                catch (IOException ex)
                {
                    Logger.Error($"串口写入行失败(IO错误): {Name}", ex);
                    return false;
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"串口写入行失败(超时): {Name}", ex);
                    return false;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error($"串口写入行失败(操作无效): {Name}", ex);
                    return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Error($"串口写入行失败(权限不足): {Name}", ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"串口写入行失败: {Name}", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 读取数据
        /// 功能说明：从串口读取指定长度的数据
        /// </summary>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="length">要读取的长度</param>
        /// <returns>实际读取的字节数，-1表示失败，0表示超时</returns>
        public int Read(byte[] buffer, int offset, int length)
        {
            lock (_lock)
            {
                if (!IsOpen)
                    return -1;

                try
                {
                    return _serialPort.Read(buffer, offset, length);
                }
                catch (IOException ex)
                {
                    Logger.Error($"串口读取失败(IO错误): {Name}", ex);
                    return -1;
                }
                catch (TimeoutException)
                {
                    return 0;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error($"串口读取失败(操作无效): {Name}", ex);
                    return -1;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Error($"串口读取失败(权限不足): {Name}", ex);
                    return -1;
                }
                catch (Exception ex)
                {
                    Logger.Error($"串口读取失败: {Name}", ex);
                    return -1;
                }
            }
        }

        /// <summary>
        /// 读取一行数据
        /// 功能说明：从串口读取一行字符串数据
        /// </summary>
        /// <returns>读取的字符串，null表示失败，空字符串表示超时</returns>
        public string ReadLine()
        {
            lock (_lock)
            {
                if (!IsOpen)
                    return null;

                try
                {
                    return _serialPort.ReadLine();
                }
                catch (IOException ex)
                {
                    Logger.Error($"串口读取行失败(IO错误): {Name}", ex);
                    return null;
                }
                catch (TimeoutException)
                {
                    return string.Empty;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error($"串口读取行失败(操作无效): {Name}", ex);
                    return null;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Error($"串口读取行失败(权限不足): {Name}", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Error($"串口读取行失败: {Name}", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取可读取的字节数
        /// 功能说明：获取串口缓冲区中可读取的字节数
        /// </summary>
        /// <returns>可读取的字节数</returns>
        public int BytesToRead
        {
            get
            {
                lock (_lock)
                {
                    return _serialPort?.BytesToRead ?? 0;
                }
            }
        }
    }
}
