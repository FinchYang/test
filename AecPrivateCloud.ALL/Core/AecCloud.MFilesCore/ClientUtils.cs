using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Ionic.Zip;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    /// <summary>
    /// 关于客户端的帮助类
    /// </summary>
    public static class ClientUtils
    {
        private static MFilesClientApplication _app;

        public static MFilesClientApplication GetClientApp()
        {
            return _app ?? (_app = new MFilesClientApplication());
        }

        private static string _versionString;
        /// <summary>
        /// version.Major + "." + version.Minor + "." + version.Build + "." + version.Patch
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            if (_versionString == null)
            {
                var app = GetClientApp();
                var version = app.GetClientVersion();
                _versionString = version.Major + "." + version.Minor + "." + version.Build + "." + version.Patch;
            }
            return _versionString;
        }

        private static string _driveLetter;

        public static string GetDriveLetter()
        {
            if (_driveLetter == null)
            {
                var app = GetClientApp();
                _driveLetter = app.GetDriveLetter();
            }
            return _driveLetter;
        }

        private static bool IsSameHost(string host1, string host2)
        {
            return false;
        }

        private static MfVaultConnection ConvertToData(VaultConnection conn)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            path = Path.Combine(path, @"M-Files\Icons");
            var iconPath = Path.Combine(path, conn.ServerVaultGUID + ".ico");
            var mfConn = new MfVaultConnection {Name = conn.Name, Guid = conn.ServerVaultGUID};
            if (File.Exists(iconPath))
            {
                mfConn.IconPath = iconPath;//File.ReadAllBytes(iconPath);
            }
            return mfConn;
        }

        class HostEqualComparer : EqualityComparer<string>
        {

            public override bool Equals(string x, string y)
            {
                if (x == null && y != null) return false;
                if (x != null && y == null) return false;
                if (x == null && y == null) return true;
                return x.Contains(y) || y.Contains(x);
            }

            public override int GetHashCode(string obj)
            {
                return base.GetHashCode();
            }
        }

        static List<string> ResolveIp(string host)
        {
            var ipEntry = Dns.GetHostEntry(host);
            return ipEntry.AddressList.Select(c => c.ToString()).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreHosts">需要忽略的主机</param>
        /// <returns></returns>
        public static List<MfVaultConnection> GetVaultConnections(params string[] ignoreHosts)
        {
            var app = GetClientApp();
            var conns = app.GetVaultConnections().Cast<VaultConnection>();
            if (ignoreHosts != null && ignoreHosts.Length > 0)
            {
                return conns.Where(c => !ignoreHosts.Contains(c.NetworkAddress, new HostEqualComparer())).Select(ConvertToData).ToList();
            }
            return conns.Select(ConvertToData).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreGuids">需要忽略的连接名称列表</param>
        /// <returns></returns>
        public static List<MfVaultConnection> GetVaultConnections(IEnumerable<string> ignoreGuids)
        {
            var app = GetClientApp();
            var conns = app.GetVaultConnections().Cast<VaultConnection>();
            if (ignoreGuids != null)
            {
                return conns.Where(c => !ignoreGuids.Any(
                    d => StringComparer.OrdinalIgnoreCase.Equals(c.GetGUID(), d))).Select(ConvertToData).ToList();
            }
            return conns.Select(ConvertToData).ToList();
        }

        public static ObjectVersionFile GetObjectFile(string objURL, bool updateFromServer = false)
        {
            try
            {
                var app = GetClientApp();
                return app.FindFile(objURL, updateFromServer);
            }
            catch
            {
                return null;
            }
        }

        public static ObjectVersionAndProperties GetObjectFromURL(string objURL, bool updateFromServer=false)
        {
            try
            {
                var app = GetClientApp();
                return app.FindObjectVersionAndProperties(objURL, updateFromServer);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFilename(string filePath, bool updateFromServer = false)
        {
            var file = GetObjectFile(filePath, updateFromServer);
            if (file == null) return Path.GetFileName(filePath);
            return file.ObjectFile.Title + "." + file.ObjectFile.Extension;
        }

        private static string _installDir;

        public static string GetMFilesInstallDir()
        {
            if (_installDir == null)
            {
                var versionStr = GetVersionString();
                var lm = Registry.LocalMachine;
                var mfiles = lm.OpenSubKey(@"SOFTWARE\Motive\M-Files\" + versionStr);
                if (mfiles == null)
                {
                    throw new Exception("mfiles注册表项丢失");
                }
                var dirValue = mfiles.GetValue("InstallDir");
                mfiles.Close();
                if (dirValue != null)
                {
                    _installDir = dirValue.ToString();
                }
            }
            return _installDir;
        }

        private static readonly string ClientSysAppsPath = @"Client\Apps\sysapps\";

        private static readonly string VaultSysAppsPathFormat = @"Client\Apps\{0}\sysapps\";


        public static int GetMajorVersion()
        {
            var app = GetClientApp();
            var version = app.GetClientVersion();
            return version.Major;
        }

        public static string GetAppPath(string vaultGuid)
        {
            var clientPath = GetMFilesInstallDir();
            if (!String.IsNullOrEmpty(vaultGuid))
            {
                return Path.Combine(clientPath, String.Format(VaultSysAppsPathFormat, vaultGuid));
            }
            return Path.Combine(clientPath, ClientSysAppsPath);
        }

        public static List<string> GetVaultSysAppDefFiles(string vaultGuid)
        {
            var list = new List<string>();
            var sysappPath = GetAppPath(vaultGuid);
            if (!Directory.Exists(sysappPath)) return list; //第一次没有sysapps
            var dirs = Directory.GetDirectories(sysappPath);
            foreach (var d in dirs)
            {
                var appFile = Path.Combine(d, "appdef.xml");
                if (!File.Exists(appFile)) continue;
                list.Add(appFile);
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultGuid"></param>
        /// <param name="appGuid"></param>
        /// <param name="zipPath">Zip文件内必须有一个顶层文件夹</param>
        public static List<string> InstallAppAsVaultSysApp(string vaultGuid, string appGuid, string zipPath)
        {
            var clientPath = GetMFilesInstallDir();
            var appPath = Path.Combine(clientPath, String.Format(VaultSysAppsPathFormat, vaultGuid));
            return ExtractApp(appPath, appGuid, zipPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appGuid"></param>
        /// <param name="zipPath">Zip文件内必须有一个顶层文件夹</param>
        public static List<string> InstallAppAsClientSysApp(string appGuid, string zipPath)
        {
            var clientPath = GetMFilesInstallDir();
            var appPath = Path.Combine(clientPath, ClientSysAppsPath);
            return ExtractApp(appPath, appGuid, zipPath);
        }

        internal static List<string> ExtractApp(string appPath, string appGuid, string appZipPath)
        {
            var extPath = Path.Combine(appPath, appGuid);
            if (!Directory.Exists(extPath))
            {
                Directory.CreateDirectory(extPath);
            }
            var errFileList = new List<string>();
            var zipFilename = Path.GetFileNameWithoutExtension(appZipPath);
            using (var zip = new ZipFile(appZipPath, Encoding.Default))
            {
                var noDir = zip.Entries.Any(c => !c.FileName.Contains("/"));
                if (noDir)
                {
                    zip.ExtractAll(extPath);
                }
                else
                {
                    var entryName = zip.Entries.First().FileName; 
                    var dirNameIndex = entryName.IndexOf('/');
                    var dirName = entryName.Substring(0, dirNameIndex + 1);
                    var dirEntries = zip.Entries.Where(c => c.FileName != dirName).ToList();
                    foreach (var d in dirEntries)
                    {
                        var fileName = d.FileName.Substring(dirName.Length);
                        d.FileName = fileName;
                        try
                        {
                            d.Extract(extPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                        catch
                        {
                            errFileList.Add(zipFilename+ " # " + fileName);
                        }
                    }
                }
            }
            return errFileList;
        }
    }
}
