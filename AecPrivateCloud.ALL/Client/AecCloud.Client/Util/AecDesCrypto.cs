using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AecCloud.Client.Util
{
    public static class AecDesCrypto
    {
        private const string Key = "20150302";
         /// <summary>
        /// 进行DES加密。
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串。</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public static string Encrypt(string pToEncrypt, string sKey)
         {
             try
             {
                 var inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                 using (var des = new DESCryptoServiceProvider())
                 {
                     des.Key = Encoding.ASCII.GetBytes(sKey);
                     des.IV = Encoding.ASCII.GetBytes(sKey);
                     var ms = new System.IO.MemoryStream();
                     using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                     {
                         cs.Write(inputByteArray, 0, inputByteArray.Length);
                         cs.FlushFinalBlock();
                         cs.Close();
                     }
                     var str = Convert.ToBase64String(ms.ToArray());
                     ms.Close();
                     return str;
                 }
             }
             catch (Exception)
             {
                 return "";
             }
         }

        /// <summary>
        /// 进行DES解密。
        /// </summary>
        /// <param name="pToDecrypt">要解密的以Base64</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>已解密的字符串。</returns>
        public static string Decrypt(string pToDecrypt, string sKey)
        {
            try
            {
                var inputByteArray = Convert.FromBase64String(pToDecrypt);
                using (var des = new DESCryptoServiceProvider())
                {
                    des.Key = Encoding.ASCII.GetBytes(sKey);
                    des.IV = Encoding.ASCII.GetBytes(sKey);
                    var ms = new MemoryStream();
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    var str = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch (Exception)
            {
                return "";
            }
         
        }   
     
        /// <summary>
        /// 创建密钥
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            //var desCrypto = (DESCryptoServiceProvider)DES.Create();
            //return Encoding.ASCII.GetString(desCrypto.Key);
            return Key;
        }
    }
}
