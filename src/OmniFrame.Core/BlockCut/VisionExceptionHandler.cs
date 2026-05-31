using System;
using System.Drawing;
using System.Threading.Tasks;
using OmniFrame.Core;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    public class VisionExceptionHandler
    {
        public static bool TryExecute<T>(Func<T> operation, out T result, out string errorMessage)
        {
            try
            {
                result = operation();
                errorMessage = null;
                return true;
            }
            catch (CameraDisconnectedException ex)
            {
                result = default;
                errorMessage = $"相机未连接: {ex.Message}";
                BlockCutAlarmHelper.ReportError(errorMessage);
                return false;
            }
            catch (ImageProcessingException ex)
            {
                result = default;
                errorMessage = $"图像处理失败: {ex.Message}";
                BlockCutAlarmHelper.ReportError(errorMessage);
                return false;
            }
            catch (TimeoutException ex)
            {
                result = default;
                errorMessage = $"采集超时: {ex.Message}";
                BlockCutAlarmHelper.ReportWarning(errorMessage);
                return false;
            }
            catch (Exception ex)
            {
                result = default;
                errorMessage = $"未知错误: {ex.Message}";
                BlockCutAlarmHelper.ReportError(errorMessage, ex);
                return false;
            }
        }

        public static async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    BlockCutAlarmHelper.ReportWarning($"操作失败，重试 {retryCount}/{maxRetries}: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        await Task.Delay(1000 * retryCount);
                    }
                }
            }

            BlockCutAlarmHelper.ReportError("操作失败，已达到最大重试次数");
            return default;
        }

        public static async Task<T> ExecuteWithRetry<T>(Func<T> operation, int maxRetries = 3)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    BlockCutAlarmHelper.ReportWarning($"操作失败，重试 {retryCount}/{maxRetries}: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        await Task.Delay(1000 * retryCount);
                    }
                }
            }

            BlockCutAlarmHelper.ReportError("操作失败，已达到最大重试次数");
            return default;
        }

        public static Bitmap GetDefaultImage()
        {
            var bmp = new Bitmap(640, 480);
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.DarkGray, 0, 0, 640, 480);
                g.DrawString("图像采集失败", new Font("Arial", 16), Brushes.Red, 250, 220);
            }
            return bmp;
        }

        public static Bitmap GetDefaultImage(string message)
        {
            var bmp = new Bitmap(640, 480);
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.DarkGray, 0, 0, 640, 480);
                g.DrawString(message, new Font("Arial", 12), Brushes.Red, 50, 220);
            }
            return bmp;
        }
    }

    public class CameraDisconnectedException : Exception
    {
        public CameraDisconnectedException(string message) : base(message) { }
        public CameraDisconnectedException(string message, Exception inner) : base(message, inner) { }
    }

    public class ImageProcessingException : Exception
    {
        public ImageProcessingException(string message) : base(message) { }
        public ImageProcessingException(string message, Exception inner) : base(message, inner) { }
    }
}
