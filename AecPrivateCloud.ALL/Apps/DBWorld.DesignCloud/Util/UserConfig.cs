using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace DBWorld.DesignCloud.Util
{
    public class UserConfig
    {
        /// <summary>
        /// 目录名称
        /// </summary>
        private const string AppFolder = "DbWorld";

        /// <summary>
        /// 配置文件名
        /// </summary>
        private const string SettingFilename = "projconfig.xml";

        /// <summary>
        /// 文件夹id
        /// </summary>
        private static long _folderId;

        /// <summary>
        /// 配置文件实例
        /// </summary>
        private static UserConfig _default;

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 默认项目id
        /// </summary>
        public long DefProject { get; set; }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public static UserConfig LoadConfig(long folderId)
        {
            _folderId = folderId;

            if (_default != null)
            {
                return _default;
            }

            var configFile = GetConfigPath();
            if (!File.Exists(configFile))
            {
                return (_default = new UserConfig());
            }
            return (_default = GetFromConfig(configFile));
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            var configFile = GetConfigPath();
            SerializerToXml(configFile);
        }

        /// <summary>
        /// 获取配置文件名
        /// </summary>
        /// <returns></returns>
        private static string GetConfigPath()
        {
            var mydocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folder = Path.Combine(mydocuments, AppFolder, _folderId.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, SettingFilename);
        }

        /// <summary>
        /// 序列到xml文件
        /// </summary>
        /// <param name="xmlFile"></param>
        internal void SerializerToXml(string xmlFile)
        {
            var se = new XmlSerializer(this.GetType());
            using (var fs = new FileStream(xmlFile, FileMode.Create, FileAccess.Write))
            {
                se.Serialize(fs, this);
            }
        }

        /// <summary>
        /// 反序列到对象
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        internal static UserConfig GetFromConfig(string configFile = null)
        {
            if (configFile == null)
            {
                var assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFile = Path.Combine(assPath, SettingFilename);
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("用户配置文件不存在！" + configFile, configFile);
            }

            try
            {
                XDocument.Load(configFile);
            }
            catch (XmlException)
            {
                var txt = File.ReadAllText(configFile, Encoding.Default);
                File.WriteAllText(configFile, txt, Encoding.UTF8);
            }

            var se = new XmlSerializer(MethodBase.GetCurrentMethod().DeclaringType);
            using (var fs = new FileStream(configFile, FileMode.Open, FileAccess.Read))
            {
                var vault = se.Deserialize(fs) as UserConfig;
                return vault;
            }
        }
    }

}
