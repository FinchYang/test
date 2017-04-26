using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MfNotification.Core.Cryptography
{
    /// <summary>
    /// Message加解密帮助类
    /// </summary>
    public class CryptUtil
    {
        //这两个对key，iv是用来加密数据库中User表的msgKey，msgIv
        private static readonly CryptInfo KeyCryptInfo = new CryptInfo {KeyStr = "KeyCloud", IvStr = "KeyIvCad"};
        private static readonly CryptInfo IvCryptInfo = new CryptInfo { KeyStr = "IvKeyCad", IvStr = "IvClouds" };

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="msg">原始内容</param>
        /// <param name="cryptInfo">加密信息(key,iv)</param>
        /// <returns></returns>
        public static string Encryption(string msg, CryptInfo cryptInfo)
        {
            var orgInfo = DecryptCryptInfo(cryptInfo);
            return Encryption(msg, orgInfo.KeyStr, orgInfo.IvStr);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="msg">加密内容</param>
        /// <param name="cryptInfo">解密信息(key,iv)</param>
        /// <returns></returns>
        public static string Decryption(string msg,CryptInfo cryptInfo)
        {
            var orgInfo = DecryptCryptInfo(cryptInfo);
            return Decryption(msg, orgInfo.KeyStr, orgInfo.IvStr);
        }

        /// <summary>
        /// 生成加密信息(key,iv)
        /// </summary>
        /// <returns></returns>
        public static CryptInfo CreateCryptInfo()
        {
            var guid = Guid.NewGuid().ToString();
            var md5 = MD5Util.StringToMD5(MD5Type.Capital16, guid);
            var orgKey = md5.Substring(0, 8);
            var orgIv = md5.Substring(8, 8);
            var key = Encryption(orgKey, KeyCryptInfo.KeyStr, KeyCryptInfo.IvStr);
            var iv = Encryption(orgIv, IvCryptInfo.KeyStr, IvCryptInfo.IvStr);
            return new CryptInfo {KeyStr = key, IvStr = iv};
        }

        /// <summary>
        /// 解密用户的加密信息
        /// </summary>
        /// <returns></returns>
        private static CryptInfo DecryptCryptInfo(CryptInfo cryptInfo)
        {
            
            var key = Decryption(cryptInfo.KeyStr, KeyCryptInfo.KeyStr, KeyCryptInfo.IvStr);
            var iv = Decryption(cryptInfo.IvStr, IvCryptInfo.KeyStr, IvCryptInfo.IvStr);
            return new CryptInfo {KeyStr = key, IvStr = iv};
        }


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="msg">原始信息</param>
        /// <param name="keyStr">秘钥</param>
        /// <param name="ivStr">偏移向量</param>
        /// <returns></returns>
        private static string Encryption(string msg, string keyStr, string ivStr)
        {
            keyStr = keyStr.PadLeft(8, '0').Substring(0, 8);
            byte[] key = Encoding.UTF8.GetBytes(keyStr);
            byte[] iv = Encoding.UTF8.GetBytes(ivStr);
            byte[] context = Encoding.UTF8.GetBytes(msg);

            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();
            //创建加密流，创建加密使用 des.CreateEncryptor
            var cs = new CryptoStream(ms, des.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            //向加密流写入正文
            cs.Write(context, 0, context.Length);
            //将缓冲区数据写入，然后清空缓冲区
            cs.FlushFinalBlock();

            //从内存流返回结果，并编码为 base64string 
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="msg">加密信息</param>
        /// <param name="keyStr">秘钥</param>
        /// <param name="ivStr">便宜向量</param>
        /// <returns></returns>
        private static string Decryption(string msg, string keyStr, string ivStr)
        {
            keyStr = keyStr.PadLeft(8, '0').Substring(0, 8);
            byte[] key = Encoding.UTF8.GetBytes(keyStr);
            byte[] iv = Encoding.UTF8.GetBytes(ivStr);
            //将解密正文返回到 byte 数组，加密时编码为 base64string ，这里要使用 FromBase64String 直接取回 byte 数组
            byte[] context = Convert.FromBase64String(msg);

            var des = new DESCryptoServiceProvider();
            //创建内存流，用于取解密结果
            var ms = new MemoryStream();
            //创建解密的流， 这里的是 des.CreateDecryptor 
            var cs = new CryptoStream(ms, des.CreateDecryptor(key, iv), CryptoStreamMode.Write);

            //向解密流写入数据
            cs.Write(context, 0, context.Length);
            //将当前缓冲区写入绑定的内存流，然后清空缓冲区
            cs.FlushFinalBlock();

            //从内存流返回值，并编码到 UTF8 输出原文
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
