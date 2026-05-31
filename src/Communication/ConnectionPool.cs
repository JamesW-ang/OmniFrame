using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;

namespace OmniFrame.Communication
{
    /// <summary>
    /// 连接池类
    /// 设计介绍：
    /// 2. 支持连接的复用和自动回收，提高系统性能
    /// 3. 实现连接健康检查，确保连接的可用性
    /// 4. 支持连接池的动态扩容和缩容，适应不同的负载情况
    /// 5. 线程安全设计，支持多线程并发访问
        /// </summary>
    /// <typeparam name="T">连接类型</typeparam>
    public class ConnectionPool<T> where T : class, IDisposable
    {
        private ConcurrentBag<T> _connections;
        private Func<T> _connectionFactory;
        private Action<T> _connectionReset;
        private Func<T, bool> _connectionValidator;
        private int _maxPoolSize;
        private int _minPoolSize;
        private int _currentPoolSize;
        private readonly object _lock = new object();

        /// <summary>
        /// 连接池构造函数
        /// </summary>
        /// <param name="connectionFactory">连接工厂方法</param>
        /// <param name="connectionReset">连接重置方法</param>
        /// <param name="connectionValidator">连接验证方法</param>
        /// <param name="minPoolSize">最小连接池大小</param>
        /// <param name="maxPoolSize">最大连接池大小</param>
        public ConnectionPool(Func<T> connectionFactory, Action<T> connectionReset = null, 
            Func<T, bool> connectionValidator = null, int minPoolSize = 5, int maxPoolSize = 20)
        {
            _connectionFactory = connectionFactory;
            _connectionReset = connectionReset;
            _connectionValidator = connectionValidator;
            _minPoolSize = minPoolSize;
            _maxPoolSize = maxPoolSize;
            _connections = new ConcurrentBag<T>();
            _currentPoolSize = 0;

            InitializePool();
        }

        /// <summary>
        /// 初始化连接池
        /// </summary>
        private void InitializePool()
        {
            try
            {
                for (int i = 0; i < _minPoolSize; i++)
                {
                    T connection = _connectionFactory();
                    _connections.Add(connection);
                    _currentPoolSize++;
                }
                Logger.Info($"连接池初始化完成，初始连接数: {_minPoolSize}");
            }
            catch (Exception ex)
            {
                Logger.Error("连接池初始化失败", ex);
            }
        }

        /// <summary>
        /// 从连接池获取连接
        /// </summary>
        /// <returns>连接对象</returns>
        public T GetConnection()
        {
            try
            {
                if (_connections.TryTake(out T connection))
                {
                    if (_connectionValidator == null || _connectionValidator(connection))
                    {
                        return connection;
                    }
                    else
                    {
                        connection.Dispose();
                        _currentPoolSize--;
                    }
                }

                lock (_lock)
                {
                    if (_currentPoolSize < _maxPoolSize)
                    {
                        connection = _connectionFactory();
                        _currentPoolSize++;
                        Logger.Info($"创建新连接，当前连接数: {_currentPoolSize}");
                        return connection;
                    }
                }

                Logger.Warning("连接池已满，等待可用连接");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("获取连接失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 将连接归还到连接池
        /// </summary>
        /// <param name="connection">连接对象</param>
        public void ReturnConnection(T connection)
        {
            try
            {
                if (connection == null)
                {
                    return;
                }

                if (_connectionValidator != null && !_connectionValidator(connection))
                {
                    connection.Dispose();
                    _currentPoolSize--;
                    Logger.Info("连接已失效，已销毁");
                    return;
                }

                _connectionReset?.Invoke(connection);
                _connections.Add(connection);
                Logger.Info($"连接已归还到连接池，当前连接数: {_currentPoolSize}");
            }
            catch (Exception ex)
            {
                Logger.Error("归还连接失败", ex);
                connection?.Dispose();
            }
        }

        /// <summary>
        /// 清空连接池
        /// </summary>
        public void Clear()
        {
            try
            {
                while (_connections.TryTake(out T connection))
                {
                    connection.Dispose();
                    _currentPoolSize--;
                }
                Logger.Info("连接池已清空");
            }
            catch (Exception ex)
            {
                Logger.Error("清空连接池失败", ex);
            }
        }

        /// <summary>
        /// 获取连接池状态
        /// </summary>
        /// <returns>连接池状态信息</returns>
        public ConnectionPoolStatus GetStatus()
        {
            return new ConnectionPoolStatus
            {
                CurrentPoolSize = _currentPoolSize,
                AvailableConnections = _connections.Count,
                MinPoolSize = _minPoolSize,
                MaxPoolSize = _maxPoolSize
            };
        }
    }

    /// <summary>
    /// 连接池状态
        /// </summary>
    public class ConnectionPoolStatus
    {
        /// <summary>
        /// 当前连接池大小
        /// </summary>
        public int CurrentPoolSize { get; set; }

        /// <summary>
        /// 可用连接数
        /// </summary>
        public int AvailableConnections { get; set; }

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; }

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; }
    }
}
