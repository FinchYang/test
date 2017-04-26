using System;
using System.IO;
using System.Xml.Serialization;

namespace DBWorld.Config.Helper
{
    public static class XmlSerializerUtil
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="filePath">文件名</param>
        /// <param name="sourceObj">实例</param>
        /// <param name="type">类型</param>
        /// <param name="xmlRootName">根节点</param>
        public static void SaveToXml(string filePath, object sourceObj, Type type, string xmlRootName = null)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && sourceObj != null)
            {
                type = type ?? sourceObj.GetType();

                using (var writer = new StreamWriter(filePath))
                {
                    var xmlSerializer = string.IsNullOrWhiteSpace(xmlRootName) ?
                        new XmlSerializer(type) :
                        new XmlSerializer(type, new XmlRootAttribute(xmlRootName));
                    xmlSerializer.Serialize(writer, sourceObj);
                }
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="type">类型</param>
        /// <returns>实例</returns>
        public static object LoadFromXml(string filePath, Type type)
        {
            object result = null;

            try
            {
                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        var xmlSerializer = new XmlSerializer(type);
                        result = xmlSerializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
           
            return result;
        }
    }
}
