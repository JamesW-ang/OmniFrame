using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using OmniFrame.Common;

namespace OmniFrame.DataAccess
{
    public class SqliteHelper
    {
        private string _connectionString;

        public SqliteHelper(string dbPath)
        {
            string directory = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            _connectionString = $"Data Source={dbPath};Version=3;Pooling=True;Max Pool Size=10;";
        }

        private SQLiteConnection CreateConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public bool Open()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    Logger.Info("SQLite数据库连接测试成功");
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Logger.Error("SQLite数据库连接测试失败", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error("SQLite数据库连接测试失败", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("SQLite数据库连接测试失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 连接按需创建并自动释放，无需显式关闭
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// 创建并返回已打开的连接（供 Dapper 使用）
        /// </summary>
        public SQLiteConnection GetConnection()
        {
            return CreateConnection();
        }

        /// <summary>
        /// 使用 Dapper 查询强类型结果集
        /// </summary>
        public List<T> Query<T>(string sql, object parameters = null)
        {
            using (var conn = CreateConnection())
            {
                return conn.Query<T>(sql, parameters).AsList();
            }
        }

        /// <summary>
        /// 使用 Dapper 执行命令
        /// </summary>
        public int Execute(string sql, object parameters = null)
        {
            using (var conn = CreateConnection())
            {
                return conn.Execute(sql, parameters);
            }
        }

        /// <summary>
        /// 使用 Dapper 查询单值
        /// </summary>
        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            using (var conn = CreateConnection())
            {
                return conn.ExecuteScalar<T>(sql, parameters);
            }
        }

        /// <summary>
        /// Dapper 查询返回 DataTable（用于需要 DataTable 绑定的场景）
        /// </summary>
        public DataTable QueryDataTable(string sql, object parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = CreateConnection())
                using (var reader = conn.ExecuteReader(sql, parameters))
                {
                    dt.Load(reader);
                }
            }
            catch (SQLiteException ex)
            {
                Logger.Error($"查询SQL失败: {sql}", ex);
            }
            catch (DbException ex)
            {
                Logger.Error($"查询SQL失败: {sql}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"查询SQL失败: {sql}", ex);
            }
            return dt;
        }

        /// <summary>
        /// Dapper 查询单值（返回 object，用于类型不确定的场景）
        /// </summary>
        public object ExecuteScalarDynamic(string sql, object parameters = null)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    return conn.ExecuteScalar(sql, parameters);
                }
            }
            catch (SQLiteException ex)
            {
                Logger.Error($"执行Scalar SQL失败: {sql}", ex);
                return null;
            }
            catch (DbException ex)
            {
                Logger.Error($"执行Scalar SQL失败: {sql}", ex);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"执行Scalar SQL失败: {sql}", ex);
                return null;
            }
        }

        /// <summary>
        /// 使用标准 ADO.NET 事务执行一组操作
        /// </summary>
        public bool ExecuteInTransaction(Action<SQLiteConnection> action)
        {
            try
            {
                using (var connection = CreateConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        action(connection);
                        transaction.Commit();
                        return true;
                    }
                    catch (SQLiteException)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    catch (DbException)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Logger.Error($"执行事务失败", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error($"执行事务失败", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"执行事务失败", ex);
                return false;
            }
        }

        public bool TableExists(string tableName)
        {
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name;";
            object result = ExecuteScalarDynamic(sql, new { name = tableName });
            return result != null;
        }

        public bool CreateTable(string tableName, string columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("表名不能为空", nameof(tableName));

            for (int i = 0; i < tableName.Length; i++)
            {
                char c = tableName[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    throw new ArgumentException($"表名包含非法字符: {tableName}");
            }

            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({columns});";
                Execute(sql);
                return true;
            }
            catch (SQLiteException ex)
            {
                Logger.Error($"创建表失败: {tableName}", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error($"创建表失败: {tableName}", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"创建表失败: {tableName}", ex);
                return false;
            }
        }
    }
}
