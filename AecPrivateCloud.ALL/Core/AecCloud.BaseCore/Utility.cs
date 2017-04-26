using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AecCloud.BaseCore
{
    public static class Utility
    {
        public const string EmailPattern =
            "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$";

        public const string InvalidChars = "/\\[]:;|=,+*?<>@ ";
        public const string ProjectNamePattern = "[^`~!@#$%^&*()+=|{}':;',\\[\\].<>/?~！@#￥%……&*（）——+|{}【】‘；：”“’。，、？]*";

        public const int MinimumLength = 6;

        public const int MaxImageLength = 1024*200;

        /// <summary>
        /// Verifies that a string is in valid e-mail format
        /// </summary>
        /// <param name="email">Email to verify</param>
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        public static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) return false;

            email = email.Trim();
            var result = Regex.IsMatch(email, EmailPattern, RegexOptions.IgnoreCase);
            return result;
        }

        public static string GetHost(Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority);
        }

        public static bool IsValidAdName(string name)
        {
            return !name.Any(c => InvalidChars.Contains(c));
        }
        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
            {
                str = String.Concat(str, random.Next(10).ToString());
            }
            return str;
        }

        public static string ToHexStr(string text)
        {
            var bytes = Encoding.Default.GetBytes(text);
            return ByteToHexStr(bytes);
        }

        public static string FromHexStr(string hexStr)
        {
            var bytes = StrToHexByte(hexStr);
            return Encoding.Default.GetString(bytes);
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");

            if ((hexString.Length % 2) != 0) hexString += " ";

            var returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;

        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        public static string Hash2HexStr(string str)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.Default.GetBytes(str);
            var hash = md5.ComputeHash(bytes);
            var hashStr = ByteToHexStr(hash);
            return hashStr;
        }

        public static string Encrypt2Hex(string plainText, string encryptionPrivateKey = "")
        {
            byte[] encryptedBinary = Encrypt(plainText, encryptionPrivateKey);
            if (encryptedBinary.Length == 0) return plainText;
            return ByteToHexStr(encryptedBinary);
        }

        private static byte[] Encrypt(string plainText, string encryptionPrivateKey)
        {
            if (String.IsNullOrEmpty(plainText)) return new byte[0];
            var length = encryptionPrivateKey.Length;
            if (length%2 != 0) throw new ArgumentException("必须为偶数位", "encryptionPrivateKey");
            var mid = length/2;
            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, length));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(mid, mid));

            return EncryptTextToMemory(plainText, tDESalg.Key, tDESalg.IV);
        }

        public static string EncryptText(string plainText, string encryptionPrivateKey = "")
        {
            byte[] encryptedBinary = Encrypt(plainText, encryptionPrivateKey);
            if (encryptedBinary.Length == 0) return plainText;
            return Convert.ToBase64String(encryptedBinary);
        }

        public static string DecryptFromHex(string cipherText, string encryptionPrivateKey = "")
        {
            if (String.IsNullOrEmpty(cipherText)) return cipherText;

            var length = encryptionPrivateKey.Length;
            if (length != 16) throw new ArgumentException("必须为16位", "encryptionPrivateKey");
            var mid = length / 2;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, length));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(mid, mid));

            byte[] buffer = StrToHexByte(cipherText);
            return DecryptTextFromMemory(buffer, tDESalg.Key, tDESalg.IV);
        }

        public static string DecryptText(string cipherText, string encryptionPrivateKey = "")
        {
            if (String.IsNullOrEmpty(cipherText)) return cipherText;

            var length = encryptionPrivateKey.Length;
            if (length != 16) throw new ArgumentException("必须为16位", "encryptionPrivateKey");
            var mid = length / 2;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, length));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(mid, mid));

            byte[] buffer = Convert.FromBase64String(cipherText);
            return DecryptTextFromMemory(buffer, tDESalg.Key, tDESalg.IV);
        }

        private static byte[] EncryptTextToMemory(string data, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = new UnicodeEncoding().GetBytes(data);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }

        private static string DecryptTextFromMemory(byte[] data, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    var sr = new StreamReader(cs, new UnicodeEncoding());
                    return sr.ReadLine();
                }
            }
        }

        public static bool IsHexStr(string content)
        {
            foreach (var c in content)
            {
                var d = Char.IsDigit(c);
                var uppper = c >= 'A' && c <= 'F';
                var lower = c >= 'a' && c <= 'f';
                if (!d && !uppper && !lower) return false;
            }
            return true;
        }
    }
}
