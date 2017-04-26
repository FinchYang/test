using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.PluginInstallation
{
    /// <summary>
    /// 序列化
    /// </summary>
    public class SerialUtils
    {
        public static T GetObject<T>(string xmlFile, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var xs = new XmlSerializer(typeof(T));
                using(Stream dstream = new FileStream(xmlFile,FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    var obj = (T) xs.Deserialize(dstream);
                    dstream.Close();
                    return obj;
                }
            }
            catch (SerializationException ex)
            {
                errorMessage = string.Format("获取内容失败：{0};文件：{1}", ex.Message, xmlFile);
                return default(T);
            }
            catch(InvalidOperationException ex)
            {
                var innerEx = ex.InnerException;
                errorMessage = (innerEx == null) ? string.Format("非法操作：{1}；文件：{0}", xmlFile, ex.Message) 
                                  : string.Format("非法操作：{1}；错误：{2}；文件：{0}", xmlFile, ex.Message, innerEx.Message);
                return default(T);
            }
            catch(SecurityException)
            {
                errorMessage = string.Format("没有权限：{0}", xmlFile);
                return default(T);
            }
            catch(ArgumentException)
            {
                errorMessage = string.Format("读取文件流错误：{0}", xmlFile);
                return default(T);
            }
            catch(Exception ex)
            {
                errorMessage = string.Format("未知错误：{1}；文件：{0}", xmlFile, ex.Message);
                return default(T);
            }
        }


        /// <summary>
        /// 序列化到XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFile">要序列到的文件名</param>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>ErrorMessage</returns>
        public static string ToFile(string xmlFile, object obj)
        {
            try
            {
                var xs = new XmlSerializer(obj.GetType());
                using (var fs = new FileStream(xmlFile,FileMode.Create))
                {
                    xs.Serialize(fs,obj);
                }
                return string.Empty;
            }
            catch (SerializationException ex)
            {
                return string.Format("获取内容失败：{1}；文件：{0}",xmlFile,ex.Message);
            }
            catch(InvalidOperationException ex)
            {
                var innerEx = ex.InnerException;
                return (innerEx==null)? string.Format("非法操作：{1}；文件：{0}", xmlFile, ex.Message)
                    : string.Format("非法操作：{1}；错误：{2}；文件：{0}", xmlFile, ex.Message, innerEx.Message);
            }
            catch (SecurityException)
            {
                return string.Format("没有权限：{0}", xmlFile);
            }
            catch (ArgumentNullException)
            {
                return string.Format("读取文件流错误：{0}", xmlFile);
            }
            catch (Exception ex)
            {
                return string.Format("未知错误：{1}；文件：{0}", xmlFile, ex.Message);
            }
        }

    }
}
