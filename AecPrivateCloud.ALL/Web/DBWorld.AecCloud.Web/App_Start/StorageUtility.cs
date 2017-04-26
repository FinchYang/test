using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using DBWorld.AecCloud.Web.ApiRequests;
using log4net;

namespace DBWorld.AecCloud.Web
{
    public class StorageUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string RootPath = System.Configuration.ConfigurationManager.AppSettings["storagePath"];

        public static readonly string TemplateFolder = "Templates";

        public static readonly string VaultAppFolder = "VaultApps";

        public static readonly string VaultSharedPath = "VaultSharedFiles";

        public static readonly string VaultDataPath = "Vaults";

        public static readonly string VaultSearchIndexPath = "VaultsIndex";

        public static readonly string BIMPreviewPath = "BIMPreviewFiles";

        public static readonly string MfSql = System.Configuration.ConfigurationManager.AppSettings["mfilessql"];

        /// <summary>
        /// 共享文件的存放路径
        /// </summary>
        /// <returns></returns>
        public static string GetVaultSharedRootPath()
        {
            return Path.Combine(RootPath, VaultSharedPath); //Path.Combine(Global.ServerPath, VaultSharedPath);
        }
        /// <summary>
        /// 是否需要代理账户
        /// </summary>
        /// <param name="path">要操作的文件(夹)路径</param>
        /// <returns></returns>
        public static bool NeedImpersonation(string path)
        {
            return !path.StartsWith(Global.ServerPath, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 确保路径存在
        /// </summary>
        /// <param name="folderPath"></param>
        public static void EnsureFolderExists(string folderPath)
        {
            if (!NeedImpersonation(folderPath))
            {
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            }
            else
            {
                using (GetImpersonator())
                {
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                }
            }
        }

        public static string[] GetFiles(string folderPath)
        {
            if (!NeedImpersonation(folderPath))
            {
                if (Directory.Exists(folderPath)) return Directory.GetFiles(folderPath);
            }
            else
            {
                using (GetImpersonator())
                {
                    if (Directory.Exists(folderPath)) return Directory.GetFiles(folderPath);
                }
            }
            return new string[0];
        }

        public static void WriteBytes(string filePath, byte[] contents, bool forceWrite)
        {
            if (!NeedImpersonation(filePath))
            {
                if (forceWrite || !File.Exists(filePath))
                {
                    File.WriteAllBytes(filePath, contents);
                }
            }
            else
            {
                using (GetImpersonator())
                {
                    if (forceWrite || !File.Exists(filePath))
                    {
                        File.WriteAllBytes(filePath, contents);
                    }
                }
            }
        }

        public static string GetVaultDataPath()
        {
            return Path.Combine(RootPath, VaultDataPath);
        }

        public static string GetVaultIndexPath()
        {
            return Path.Combine(RootPath, VaultSearchIndexPath);
        }

        public static MFSqlDatabase GetMfSqlDb(string catelog)
        {
            var connStr = MfSql;
            var strs = connStr.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            var server = strs[0].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries)[1];
            var userId = strs[1].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1];
            var password = strs[2].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1];
            return new MFSqlDatabase
            {
                Catelog = catelog,
                Server = server,
                AdminUserName = userId,
                AdminPassword = password,
                SqlserverUser = true
            };
        }

        internal static async Task<Stream> DownloadToStream(string url)
        {
            //Log.Info("要保存为流的文件路径: " + url);
            url = url.ToLower();
            var uri = new Uri(url);
            var host = uri.GetLeftPart(UriPartial.Authority);
            string requestUrl = host.EndsWith("/") ? url.Substring(host.Length) : url.Substring(host.Length + 1);
            var client = HttpClientContext.GetOrAddClient(host);
            //var content = await client.GetStreamAsync(requestUrl);
            var res = await client.GetAsync(requestUrl);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStreamAsync();
                var ms = new MemoryStream();
                await content.CopyToAsync(ms);
                //Log.Info("文件长度：" + ms.Length);
                ms.Position = 0;
                return ms;
            }
            Log.Warn("要保存为流的文件不存在: " + url);
            return null;
        }


        internal static async Task<bool> DownloadToPath(string url, string filePath)
        {
            Log.InfoFormat("压缩文件路径({0})：{1}",url, filePath);
            var content = await DownloadToStream(url);
            if (content == null) return false;
            Impersonator imp = null;
            if (NeedImpersonation(filePath))
            {
                imp = GetImpersonator();
            }
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    content.CopyTo(fs); //await content.CopyToAsync(fs);
                    fs.Flush();
                    fs.Close();
                }
            }
            finally
            {
                if (imp != null)
                {
                    imp.Dispose();
                }
            }
            
            return true;
        }

        internal static Impersonator GetImpersonator()
        {
            var userNameFull = System.Configuration.ConfigurationManager.AppSettings["mfusername"];
            var password = System.Configuration.ConfigurationManager.AppSettings["mfpassword"];
            var domainName = String.Empty;
            var userName = userNameFull;
            var strs = userNameFull.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length > 1)
            {
                domainName = strs[0];
                userName = strs[1];
            }
            return new Impersonator(userName, domainName, password);
        }

        public static async Task<string> AddTemplate(long templateId, string version, string url, bool forceUpdate)
        {
            Log.Info("开始添加模板: ");
            var tempFolder = Path.Combine(RootPath, TemplateFolder);
            Impersonator imp = null;
            if (NeedImpersonation(tempFolder))
            {
                imp = GetImpersonator();
            }
            try
            {

                try
                {
                    if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
                }
                catch (Exception ex)
                {
                    Log.Error("创建文件夹失败：" + tempFolder, ex);
                    return String.Empty;
                }
                var destPath = Path.Combine(tempFolder, templateId.ToString(), version);
                try
                {
                    if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                    else
                    {
                        //含有Index.xml文件
                        if (Directory.GetFiles(destPath, "*.xml").Length > 0 && !forceUpdate) return destPath;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("创建文件夹({0})失败：{1}", destPath, ex.Message), ex);
                    return String.Empty;
                }
                //Log.Info("TemplatePath: " + destPath);

                var zipPath = Path.Combine(destPath, templateId + "_" + version + ".zip");
                if (!File.Exists(zipPath))
                {
                    var ok = await DownloadToPath(url, zipPath);
                    if (!ok)
                    {
                        Log.Error("下载模版失败：" + url);
                        return String.Empty;
                    }
                }
                try
                {
                    ZipUtils.ExtractTemplate(zipPath, destPath);
                    try
                    {
                        File.Delete(zipPath);
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception ex)
                {
                    var ex0 = ex;

                    if (ex.InnerException != null) ex0 = ex.InnerException;

                    Log.Error("ZIP解压失败：" + zipPath + " # Error: " + ex0.Message, ex0);
                    return String.Empty;
                }
                return destPath;
            }
            finally
            {
                if (imp != null) imp.Dispose();
            }
        }

        internal static string GetVaultAppFolder()
        {
            return Path.Combine(RootPath, VaultAppFolder);
        }

        public static string GetAppPath(long appId, string version)
        {
            return Path.Combine(GetVaultAppFolder(), appId + "-" + version + ".zip");
        }

        public static async Task<string> AddVaultApp(long appId, string version, string url, bool forceUpdate)
        {
            Impersonator imp = null;
            var folder = GetVaultAppFolder();
            if (NeedImpersonation(folder))
            {
                imp = GetImpersonator();
            }
            try
            {

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var destPath = GetAppPath(appId, version); //Path.Combine(folder, appId + "-" + version + ".zip");
                if (File.Exists(destPath) && !forceUpdate) return destPath;
                //if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                return await DownloadApp(url, destPath);
            }
            finally
            {
                if (imp != null) imp.Dispose();
            }
        }

        internal static async Task<string> DownloadApp(string url, string appPath)
        {
            var ok = await DownloadToPath(url, appPath);
            if (!ok) return String.Empty;
            return appPath;
        }
    }
}