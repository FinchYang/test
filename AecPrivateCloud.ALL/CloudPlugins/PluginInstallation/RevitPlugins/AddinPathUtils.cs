using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AecCloud.PluginInstallation.RevitPlugins
{
    public class AddinPathUtils
    {
        private const string RevitFragment = @"Autodesk\Revit\Addins";

        private static string _alluserPath;

        internal static string GetAllUserAppDataPath()
        {
            return _alluserPath ??
                   (_alluserPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        }

        public static string GetAllUserPath(string revitVersion)
        {
            var appData = GetAllUserAppDataPath();
            return Path.Combine(appData, RevitFragment, revitVersion);
        }

        private static string _currentuserPath;

        internal static string GetCurrentUserAppDataPath()
        {
            return _currentuserPath ??
                   (_currentuserPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }

        public static string GetCurrentUserPath(string revitVersion)
        {
            var appData = GetCurrentUserAppDataPath();
            return Path.Combine(appData, RevitFragment, revitVersion);
        }

        internal static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        /// <summary>
        /// baseFolder\Version\PluginFolder
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <returns></returns>
        public  static Dictionary<string, string[]> GetAddinDict(string baseFolder)
        {
            var dict = new Dictionary<string, string[]>();
            var startVersion = 2014;
            var endVersion = DateTime.Today.Year + 1;
            for (var version = startVersion; version < endVersion; version++)
            {
                var dir = Path.Combine(baseFolder, version.ToString());
                if (!Directory.Exists(dir)) continue;
                var pluginDirs = Directory.GetDirectories(dir);
                dict.Add(version.ToString(), pluginDirs);
            }
            return dict;
        }
        /// <summary>
        /// 插件是否已经安装或因为错误无法安装
        /// </summary>
        /// <param name="revitVersion"></param>
        /// <param name="pluginDir"></param>
        /// <param name="alluser"></param>
        /// <returns></returns>
        public static bool PluginInstalledOrNoNeed(string revitVersion, string pluginDir, bool alluser)
        {
            string addinPath = alluser ? GetAllUserPath(revitVersion) : GetCurrentUserPath(revitVersion);


            var addinFiles = Directory.GetFiles(pluginDir, "*.addin", SearchOption.TopDirectoryOnly);
            if (addinFiles.Length != 1)
            {
                return true; //"未找到Addin文件"
            }
            var addinFilepath = addinFiles[0];
            var addin = RevitAddinFile.GetFromFile(addinFilepath);
            if (addin == null)
            {
                return true; //"Addin文件格式不正确"
            }
            var assemblyName = Path.GetFileName(addin.GetAssemblyPath());
            var destAssemblyPath = Path.Combine(Path.GetDirectoryName(addinFilepath), assemblyName);
            addin.SetAssemblyPath(destAssemblyPath);
            var destFilepath = Path.Combine(addinPath, Path.GetFileName(addinFilepath));
            if (File.Exists(destFilepath))
            {
                var destAddin = RevitAddinFile.GetFromFile(destFilepath);
                if (addin.Equals(destAddin)) return true;
            }
            return false;
        }
        /// <summary>
        /// 安装Revit插件
        /// </summary>
        /// <param name="revitVersion">Revit版本，如：2014</param>
        /// <param name="pluginDir">插件所在的文件夹，addin文件必须在此目录下</param>
        /// <param name="alluser">是否所有用户</param>
        /// <returns></returns>
        public static string InstallPlugin(string revitVersion, string pluginDir, bool alluser)
        {
            if (alluser && !IsAdministrator())
            {
                return "没有权限安装给所有用户";
            }

            string addinPath = alluser ? GetAllUserPath(revitVersion) : GetCurrentUserPath(revitVersion);


            var addinFiles = Directory.GetFiles(pluginDir, "*.addin", SearchOption.TopDirectoryOnly);
            if (addinFiles.Length != 1)
            {
                return "未找到Addin文件";
            }
            var addinFilepath = addinFiles[0];
            var addin = RevitAddinFile.GetFromFile(addinFilepath);
            if (addin == null)
            {
                return "Addin文件格式不正确";
            }
            var assemblyName = Path.GetFileName(addin.GetAssemblyPath());
            var destAssemblyPath = Path.Combine(Path.GetDirectoryName(addinFilepath), assemblyName);
            addin.SetAssemblyPath(destAssemblyPath);
            var destFilepath = Path.Combine(addinPath, Path.GetFileName(addinFilepath));
            if (File.Exists(destFilepath))
            {
                var destAddin = RevitAddinFile.GetFromFile(destFilepath);
                if (addin.Equals(destAddin)) return String.Empty;
            }
            File.Copy(addinFilepath, destFilepath, true);
            addin.SaveToFile(destFilepath);
            return String.Empty;
        }

        public static bool RemovePluginsInSpecificFolder(string revitVersion, bool allUser, string dllFolder)
        {
            if (allUser && !IsAdministrator())
            {
                return false;
            }
            string addinPath = allUser ? GetAllUserPath(revitVersion) : GetCurrentUserPath(revitVersion);
            if (!Directory.Exists(addinPath)) return false;

            var pluginFiles = Directory.GetFiles(addinPath, "*.addin", SearchOption.TopDirectoryOnly);

            foreach (var p in pluginFiles)
            {
                var addinFile = RevitAddinFile.GetFromFile(p);
                var pFile = addinFile.GetAssemblyPath();
                if (pFile.StartsWith(dllFolder, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Delete(p);
                    }
                    catch
                    {
                    }
                }
            }
            return true;
        }
    }
}
