using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DBWorld.AecCloud.Web
{
    
    public class EncipherAndDecrypt
    {
        

        private const string encipherKey = "5489651235478512";

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainText">需要加密的字符串</param>
        /// <param name="encipherKey">钥匙码</param>
        /// <returns></returns>
        public static string EncryptText(string plainText)
        {
            if (String.IsNullOrEmpty(plainText)) return plainText;

            //if (String.IsNullOrEmpty(encipherKey)) encipherKey = _securitySettings.EncryptionKey;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encipherKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encipherKey.Substring(8, 8));

            byte[] encryptedBinary = EncryptTextToMemory(plainText, tDESalg.Key, tDESalg.IV);
            return Convert.ToBase64String(encryptedBinary);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipherText">需要解密的字符串</param>
        /// <param name="encipherKey">钥匙码</param>
        /// <returns></returns>
        public static string DecryptText(string cipherText)
        {
            if (String.IsNullOrEmpty(cipherText)) return cipherText;

            //if (String.IsNullOrEmpty(encipherKey)) encipherKey = _securitySettings.EncryptionKey;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encipherKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encipherKey.Substring(8, 8));

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
    }
}