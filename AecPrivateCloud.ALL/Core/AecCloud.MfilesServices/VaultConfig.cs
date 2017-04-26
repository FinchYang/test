using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;

namespace AecCloud.MfilesServices
{
    public class VaultConfig
    {
        private static VaultConfig _default;

        public static VaultConfig Default
        {
            get { return _default ?? (_default = GetConfigFromFile()); }
        }

        public static VaultConfig GetConfigFromString(string content)
        {
            return JsonConvert.DeserializeObject<VaultConfig>(content);
        }

        public static VaultConfig GetConfigFromFile(string configFile="")
        {
            if (!File.Exists(configFile))
            {
#if DEBUG
                var location = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
#else
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif
                configFile = Path.Combine(location, "mfilesSettings.json");
            }
            return JsonConvert.DeserializeObject<VaultConfig>(File.ReadAllText(configFile));
        }
        public MFObjectDef Assignment { get; set; }

        public MFObjectDef Project { get; set; }
        /// <summary>
        /// 设计进度
        /// </summary>
        public MFObjectDef DesignProgress { get; set; }
        /// <summary>
        /// 施工进度
        /// </summary>
        public MFObjectDef BuildProgress { get; set; }
        /// <summary>
        /// 库的所有值列表
        /// </summary>
        public MFObjectTypeList ValueLists { get; set; }
        /// <summary>
        /// 用户组
        /// </summary>
        public MFObjectTypeList UserGroups { get; set; }
        /// <summary>
        /// 文档的类别
        /// </summary>
        public MFObjectTypeList DocClasses { get; set; }
    }
}
