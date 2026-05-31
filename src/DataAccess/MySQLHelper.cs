using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniFrame.Common;
using MySql.Data.MySqlClient;

namespace OmniFrame.DataAccess
{
    /// <summary>
    /// MySQL数据库操作助手
    /// 设计介绍：
    /// 2. 实现了基本的SQL执行、查询、标量查询功能
    /// 3. 支持事务处理，确保数据操作的原子性
    /// 4. 支持批量插入，提高数据导入效率
    /// 5. 提供连接测试功能，验证数据库连接状态
    /// 6. 使用using语句确保数据库连接和命令正确释放
    /// 7. 集成日志系统，记录数据库操作的执行情况
    /// 8. 提供完善的异常处理机制
        /// </summary>
    public class MySQLHelper
    {
        private string _connectionString;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public MySQLHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        int result = command.ExecuteNonQuery();
                        Logger.Info($"执行SQL语句成功，影响行数: {result}");
                        return result;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("执行SQL语句失败", ex);
                return -1;
            }
            catch (DbException ex)
            {
                Logger.Error("执行SQL语句失败", ex);
                return -1;
            }
            catch (Exception ex)
            {
                Logger.Error("执行SQL语句失败", ex);
                return -1;
            }
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQuery(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            Logger.Info($"执行查询成功，返回行数: {dt.Rows.Count}");
                            return dt;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("执行查询失败", ex);
                return null;
            }
            catch (DbException ex)
            {
                Logger.Error("执行查询失败", ex);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("执行查询失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 执行查询，返回第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        object result = command.ExecuteScalar();
                        Logger.Info("执行标量查询成功");
                        return result;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("执行标量查询失败", ex);
                return null;
            }
            catch (DbException ex)
            {
                Logger.Error("执行标量查询失败", ex);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("执行标量查询失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="sqlList">SQL语句列表</param>
        /// <param name="parametersList">参数列表</param>
        /// <returns>是否成功</returns>
        public bool ExecuteTransaction(List<string> sqlList, List<MySqlParameter[]> parametersList)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 0; i < sqlList.Count; i++)
                            {
                                using (MySqlCommand command = new MySqlCommand(sqlList[i], connection, transaction))
                                {
                                    if (parametersList != null && i < parametersList.Count && parametersList[i] != null)
                                    {
                                        command.Parameters.AddRange(parametersList[i]);
                                    }
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                            Logger.Info("事务执行成功");
                            return true;
                        }
                        catch (MySqlException ex)
                        {
                            transaction.Rollback();
                            Logger.Error("事务执行失败", ex);
                            return false;
                        }
                        catch (DbException ex)
                        {
                            transaction.Rollback();
                            Logger.Error("事务执行失败", ex);
                            return false;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Logger.Error("事务执行失败", ex);
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("事务执行失败", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error("事务执行失败", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("事务执行失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    connection.Close();
                    Logger.Info("MySQL连接测试成功");
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("MySQL连接测试失败", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error("MySQL连接测试失败", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("MySQL连接测试失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 批量插入（使用参数化查询防止SQL注入）
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dataTable">数据</param>
        /// <returns>是否成功</returns>
        public bool BulkInsert(string tableName, DataTable dataTable)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("表名不能为空", nameof(tableName));
            if (dataTable == null || dataTable.Rows.Count == 0)
                return false;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        var sb = new StringBuilder();
                        sb.Append("INSERT INTO ");
                        sb.Append(tableName);
                        sb.Append(" (");

                        // 列名（表名列名不参数化，但来自 DataTable 结构而非用户输入）
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            sb.Append(dataTable.Columns[i].ColumnName);
                        }
                        sb.Append(") VALUES ");

                        // 数据行 — 参数化每个值
                        var parameters = new List<MySqlParameter>();
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            sb.Append("(");
                            for (int j = 0; j < dataTable.Columns.Count; j++)
                            {
                                if (j > 0) sb.Append(", ");
                                string paramName = $"@p{i}_{j}";
                                sb.Append(paramName);
                                parameters.Add(new MySqlParameter(paramName, dataTable.Rows[i][j] ?? DBNull.Value));
                            }
                            sb.Append(")");
                        }

                        command.CommandText = sb.ToString();
                        command.Parameters.AddRange(parameters.ToArray());
                        int rowsAffected = command.ExecuteNonQuery();
                        Logger.Info($"批量插入成功，插入行数: {rowsAffected}");
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Error("批量插入失败", ex);
                return false;
            }
            catch (DbException ex)
            {
                Logger.Error("批量插入失败", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("批量插入失败", ex);
                return false;
            }
        }
    }
}