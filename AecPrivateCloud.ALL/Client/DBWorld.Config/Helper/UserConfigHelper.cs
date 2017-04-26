using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DBWorld.Config.Models;

namespace DBWorld.Config.Helper
{
    public class UserConfigHelper
    {
        /// <summary>
        /// 文件夹路径
        /// </summary>
        public static string AppFolderName = "DBWorld\\";

        /// <summary>
        /// 用户配置文件文件名
        /// </summary>
        private const string UserConfigFileName = "userconfig.xml";

        /// <summary>
        /// 消息通知配置文件名
        /// </summary>
        private const string NotificationConfigFileName = "notificationconfig.xml";

        /// <summary>
        /// vault列表配置文件名
        /// </summary>
        private const string VaultConfigFileName = "vaultconfig.xml";

        private static UserConfigHelper _instence;
        private static readonly object SyncLock = new object();

        private UserConfigHelper()
        {
            
        }

        public static UserConfigHelper GetInstence()
        {
            if (_instence == null)
            {
                lock (SyncLock)
                {
                    if (_instence == null)
                    {
                        _instence = new UserConfigHelper();
                    }
                }
            }

            return _instence;
        }

        /// <summary>
        /// 获取所有用户配置
        /// </summary>
        /// <returns></returns>
        public List<UserConfigModel> LoadAllConfigs(string configName, Type configType)
        {
            var configs = new List<UserConfigModel>();

            var dir = GetConfigFileDir(AppFolderName);
            var filesPath = GetAllConfiglPath(dir, configName);
            foreach (var path in filesPath)
            {
                var model = XmlSerializerUtil.LoadFromXml(path, configType)
                    as UserConfigModel;
                configs.Add(model);
            }

            return configs;
        }

        /// <summary>
        /// 获取最后登录的用户配置
        /// </summary>
        /// <returns></returns>
        public UserConfigModel LoadLastUserConfig()
        {
            var config = new UserConfigModel();

            var configs = LoadAllConfigs(UserConfigFileName, typeof(UserConfigModel));
            var lastTimes = new List<DateTime>();
            foreach (var model in configs)
            {
                if (model.LastLoginTime != null)
                {
                    lastTimes.Add(Convert.ToDateTime(model.LastLoginTime));
                }
            }

            if (lastTimes.Count > 0)
            {
                lastTimes.Sort();
                var strTime = lastTimes[lastTimes.Count - 1].ToString(CultureInfo.InvariantCulture);
                config = configs.FirstOrDefault(p => p.LastLoginTime == strTime);
            }

            return config;
        }

        /// <summary>
        /// 获取消息通知配置文件
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public NotificationConfigModel LoadCurrNotificationConfig(long userId)
        {
            var dir = GetConfigFileDir(AppFolderName);
            var folder = Path.Combine(dir, userId.ToString(CultureInfo.InvariantCulture));
            var filePath = Path.Combine(folder, NotificationConfigFileName);
            var model = XmlSerializerUtil.LoadFromXml(filePath, typeof (NotificationConfigModel));

            if (model == null)
            {
                return new NotificationConfigModel();
            }

            return model as NotificationConfigModel;
        }

        /// <summary>
        /// 获取vault列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public VaultConfigModel LoadCurrVaultListConfig(long userId)
        {
            var dir = GetConfigFileDir(AppFolderName);
            var folder = Path.Combine(dir, userId.ToString(CultureInfo.InvariantCulture));
            var filePath = Path.Combine(folder, VaultConfigFileName);
            var models = XmlSerializerUtil.LoadFromXml(filePath, typeof(VaultConfigModel));

            if (models == null)
            {
                return new VaultConfigModel();
            }

            return models as VaultConfigModel;
        }

        /// <summary>
        /// 保存用户配置信息
        /// </summary>
        /// <param name="model"></param>
        public void SaveConfig(UserConfigModel model)
        {
            var dir = GetConfigFileDir(AppFolderName);
            var folder = Path.Combine(dir, model.UserId.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, UserConfigFileName);

            XmlSerializerUtil.SaveToXml( filePath, model, typeof(UserConfigModel));
        }

        /// <summary>
        /// 保存消息通知配置信息
        /// </summary>
        /// <param name="model"></param>
        public void SaveConfig(NotificationConfigModel model)
        {
            var dir = GetConfigFileDir(AppFolderName);
            var folder = Path.Combine(dir, model.UserId.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, VaultConfigFileName);

            XmlSerializerUtil.SaveToXml(filePath, model, typeof(NotificationConfigModel));
        }

        /// <summary>
        /// 保存Vault列表信息
        /// </summary>
        /// <param name="model"></param>
        public void SaveConfig(VaultConfigModel model)
        {
            var dir = GetConfigFileDir(AppFolderName);
            var folder = Path.Combine(dir, model.UserId.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, VaultConfigFileName);

            XmlSerializerUtil.SaveToXml(filePath, model, typeof(VaultConfigModel));
        }

        /// <summary>
        /// 获取所有配置文件路径
        /// </summary>
        private IEnumerable<string> GetAllConfiglPath(string dir, string fileName)
        {
            //获取所有路径
            var filesPath = new List<string>();
            FindAllConfigFiles(dir, fileName, ref filesPath);

            return filesPath;
        }
      
        /// <summary>
        /// 遍历配置文件
        /// </summary>
        /// <param name="dirPath">目录</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filesPath">文件路径</param>
        private void FindAllConfigFiles(string dirPath, string fileName, ref List<string> filesPath)
        {
            var dir = new DirectoryInfo(dirPath);
            foreach (var dirInfo in dir.GetDirectories())
            {
                FindAllConfigFiles(dir + dirInfo.ToString() + "\\", fileName, ref filesPath);
            }

            foreach (var fileInfo in dir.GetFiles("*.xml"))
            {
                var fullName = fileInfo.FullName;
                if (Path.GetFileName(fullName) == fileName)
                {
                    filesPath.Add(fileInfo.FullName);
                }
            }
        }

        /// <summary>
        /// 根据目录名获取目录路径
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private string GetConfigFileDir(string dirName)
        {
            var dir = Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                 dirName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }
    }
}
