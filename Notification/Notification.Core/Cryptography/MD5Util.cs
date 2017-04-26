using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace MfNotification.Core.Cryptography
{
    /// <summary>
    /// MD5加密类
    /// </summary>
    public class MD5Util
    {
        /// <summary>
        /// 将字符串转换为MD5值
        /// </summary>
        /// <param name="md5Type"></param>
        /// <param name="inStr"></param>
        /// <returns></returns>
        public static string StringToMD5(MD5Type md5Type,string inStr)
        {
            var strbyteArray = Encoding.UTF8.GetBytes(inStr);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var date = md5.ComputeHash(strbyteArray);
            var md5Str = BitConverter.ToString(date).Replace("-", "");
            switch (md5Type)
            {
                case MD5Type.Capital16:
                    return md5Str.Substring(8, 16).ToUpper();
                case MD5Type.Capital32:
                    return md5Str.ToUpper();
                case MD5Type.Lowercase16:
                    return md5Str.Substring(8, 16).ToLower();
                case MD5Type.Lowercase32:
                    return md5Str.ToLower();
            }
            return string.Empty;
        }
    }

    public enum MD5Type
    {
        /// <summary>
        /// 大写 16 位
        /// </summary>
        Capital16,
        /// <summary>
        /// 大写 32 位
        /// </summary>
        Capital32,
        /// <summary>
        /// 小写 16 位
        /// </summary>
        Lowercase16,
        /// <summary>
        /// 小写 32 位
        /// </summary>
        Lowercase32
    }
}
