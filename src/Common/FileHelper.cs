using System;
using System.IO;
using System.Text;

namespace OmniFrame.Common
{
    public static class FileHelper
    {
        public static bool EnsureDirectoryExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"创建目录失败: {path}", ex);
                return false;
            }
        }

        public static string ReadAllText(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return string.Empty;
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error($"读取文件失败: {path}", ex);
                return string.Empty;
            }
        }

        public static bool WriteAllText(string path, string content)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                EnsureDirectoryExists(dir);
                File.WriteAllText(path, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"写入文件失败: {path}", ex);
                return false;
            }
        }

        public static bool AppendText(string path, string content)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                EnsureDirectoryExists(dir);
                File.AppendAllText(path, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"追加文件失败: {path}", ex);
                return false;
            }
        }

        public static bool CopyFile(string sourcePath, string destPath, bool overwrite = true)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return false;

                string dir = Path.GetDirectoryName(destPath);
                EnsureDirectoryExists(dir);
                File.Copy(sourcePath, destPath, overwrite);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"复制文件失败: {sourcePath} -> {destPath}", ex);
                return false;
            }
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"删除文件失败: {path}", ex);
                return false;
            }
        }
    }
}
