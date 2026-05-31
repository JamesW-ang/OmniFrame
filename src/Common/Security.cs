using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OmniFrame.Common
{
    public static class Security
    {
        /// <summary>
        /// MD5加密
        /// <summary>
        /// 简单的字符串加密（XOR）
        /// 注意：XOR加密不是安全的加密方式，仅适用于轻量级数据混淆（如非敏感配置），
        /// 不应替代AES等标准加密算法
        /// </summary>
        public static string Encrypt(string input, string key)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] resultBytes = new byte[inputBytes.Length];

            for (int i = 0; i < inputBytes.Length; i++)
            {
                resultBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Convert.ToBase64String(resultBytes);
        }

        /// <summary>
        /// 简单的字符串解密（XOR）
        /// </summary>
        public static string Decrypt(string input, string key)
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] resultBytes = new byte[inputBytes.Length];

            for (int i = 0; i < inputBytes.Length; i++)
            {
                resultBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(resultBytes);
        }

        /// <summary>
        /// 生成随机密码（使用加密安全随机数）
        /// </summary>
        public static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
            return GenerateRandomStringFromCharset(chars, length);
        }

        /// <summary>
        /// 生成随机字符串（使用加密安全随机数）
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return GenerateRandomStringFromCharset(chars, length);
        }

        #region AES-128-ECB 加密 (替代 qaesencryption.cpp)

        /// <summary>
        /// AES-128-ECB 加密 (替代 qaesencryption::encode)
        /// BlockCut MES 通信使用此算法加密 JSON 载荷
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="key">16 字节密钥 (ASCII)</param>
        /// <returns>Base64 编码的密文</returns>
        [Obsolete("AES-ECB used for Qt backward compatibility only. Migrate to CBC+HMAC for new integrations.")]
        public static string Aes128EcbEncrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] keyBytes = GetAesKeyBytes(key);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
        }

        /// <summary>
        /// AES-128-ECB 解密 (替代 qaesencryption::decode)
        /// </summary>
        /// <param name="cipherText">Base64 编码的密文</param>
        /// <param name="key">16 字节密钥 (ASCII)</param>
        /// <returns>明文字符串</returns>
        [Obsolete("AES-ECB used for Qt backward compatibility only. Migrate to CBC+HMAC for new integrations.")]
        public static string Aes128EcbDecrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] keyBytes = GetAesKeyBytes(key);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] cipherBytes;
                    try
                    {
                        cipherBytes = Convert.FromBase64String(cipherText);
                    }
                    catch (FormatException)
                    {
                        Logger.Error("AES 解密失败: 无效的 Base64 输入");
                        return string.Empty;
                    }

                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        /// <summary>
        /// 将 ASCII 密钥补齐/截断为 16 字节 (AES-128)
        /// </summary>
        private static byte[] GetAesKeyBytes(string key)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(key ?? "");
            byte[] key16 = new byte[16];
            int copyLen = Math.Min(bytes.Length, 16);
            Array.Copy(bytes, key16, copyLen);
            // 不足 16 字节补 0
            return key16;
        }

        #endregion

        private static string GenerateRandomStringFromCharset(string charset, int length)
        {
            byte[] randomBytes = new byte[length * 4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                int index = BitConverter.ToInt32(randomBytes, i * 4) & 0x7FFFFFFF;
                result[i] = charset[index % charset.Length];
            }

            return new string(result);
        }
    }
}
