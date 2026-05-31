using System;

namespace OmniFrame.Common
{
    public static class TimeHelper
    {
        public static string GetCurrentTimeString(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.Now.ToString(format);
        }

        public static string GetCurrentDateString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static long GetTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static string FormatMilliseconds(long milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
            if (ts.Hours > 0)
                return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.Minutes > 0)
                return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}.{ts.Milliseconds:000}s";
        }

        public static bool IsTimeout(DateTime startTime, int timeoutMs)
        {
            return (DateTime.Now - startTime).TotalMilliseconds > timeoutMs;
        }

        public static void Wait(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }
    }
}
