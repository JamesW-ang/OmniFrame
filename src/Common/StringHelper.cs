using System;
using System.Collections.Generic;
using System.Text;

namespace OmniFrame.Common
{
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string ToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(" ");
            }
            return sb.ToString().Trim();
        }

        public static byte[] FromHexString(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("-", "");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("十六进制字符串长度必须是偶数");

            byte[] result = new byte[hexString.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static string[] Split(string str, char separator, bool removeEmpty = true)
        {
            if (string.IsNullOrEmpty(str))
                return new string[0];

            var options = removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            return str.Split(new[] { separator }, options);
        }

        public static string Join<T>(IEnumerable<T> values, string separator)
        {
            return string.Join(separator, values);
        }

        public static string SafeSubstring(string str, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (startIndex < 0)
                startIndex = 0;

            if (startIndex >= str.Length)
                return string.Empty;

            if (length < 0)
                length = 0;

            if (startIndex + length > str.Length)
                length = str.Length - startIndex;

            return str.Substring(startIndex, length);
        }
    }
}
