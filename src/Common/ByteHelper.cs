using System;
using System.Text;

namespace OmniFrame.Common
{
    public static class ByteHelper
    {
        public static byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public static short ToInt16(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToInt16(data, startIndex);
        }

        public static ushort ToUInt16(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToUInt16(data, startIndex);
        }

        public static int ToInt32(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToInt32(data, startIndex);
        }

        public static uint ToUInt32(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToUInt32(data, startIndex);
        }

        public static float ToSingle(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToSingle(data, startIndex);
        }

        public static double ToDouble(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToDouble(data, startIndex);
        }

        public static void SwapBytes(byte[] data)
        {
            for (int i = 0; i < data.Length / 2; i++)
            {
                byte temp = data[i];
                data[i] = data[data.Length - 1 - i];
                data[data.Length - 1 - i] = temp;
            }
        }

        public static void SwapBytes16(byte[] data, int startIndex)
        {
            byte temp = data[startIndex];
            data[startIndex] = data[startIndex + 1];
            data[startIndex + 1] = temp;
        }

        public static void SwapBytes32(byte[] data, int startIndex)
        {
            byte temp = data[startIndex];
            data[startIndex] = data[startIndex + 3];
            data[startIndex + 3] = temp;
            temp = data[startIndex + 1];
            data[startIndex + 1] = data[startIndex + 2];
            data[startIndex + 2] = temp;
        }
    }
}
