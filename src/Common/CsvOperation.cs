using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniFrame.Common
{
    /// <summary>
    /// CSV文件操作工具类
    /// 提供CSV文件的读取、写入和追加功能
        /// </summary>
    public static class CsvOperation
    {
        /// <summary>
        /// 读取CSV文件内容
        /// </summary>
        /// <param name="path">CSV文件路径</param>
        /// <param name="delimiter">分隔符，默认为逗号</param>
        /// <returns>返回CSV文件内容的二维数组</returns>
        public static List<string[]> ReadCsv(string path, char delimiter = ',')
        {
            List<string[]> result = new List<string[]>();
            try
            {
                if (!File.Exists(path))
                    return result;

                using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(delimiter);
                        result.Add(parts);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"读取CSV文件失败: {path}", ex);
            }
            return result;
        }

        /// <summary>
        /// 写入CSV文件
        /// </summary>
        /// <param name="path">CSV文件路径</param>
        /// <param name="data">要写入的数据</param>
        /// <param name="delimiter">分隔符，默认为逗号</param>
        /// <returns>写入成功返回true，失败返回false</returns>
        public static bool WriteCsv(string path, List<string[]> data, char delimiter = ',')
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                FileHelper.EnsureDirectoryExists(dir);

                using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    foreach (string[] row in data)
                    {
                        string line = string.Join(delimiter.ToString(), row);
                        writer.WriteLine(line);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"写入CSV文件失败: {path}", ex);
                return false;
            }
        }

        /// <summary>
        /// 追加数据到CSV文件
        /// </summary>
        /// <param name="path">CSV文件路径</param>
        /// <param name="data">要追加的数据行</param>
        /// <param name="delimiter">分隔符，默认为逗号</param>
        /// <returns>追加成功返回true，失败返回false</returns>
        public static bool AppendCsv(string path, string[] data, char delimiter = ',')
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                FileHelper.EnsureDirectoryExists(dir);

                using (StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8))
                {
                    string line = string.Join(delimiter.ToString(), data);
                    writer.WriteLine(line);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"追加CSV文件失败: {path}", ex);
                return false;
            }
        }
    }
}