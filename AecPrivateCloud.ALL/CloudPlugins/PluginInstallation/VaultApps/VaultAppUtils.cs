using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using log4net;

namespace AecCloud.PluginInstallation.VaultApps
{
    public class VaultAppUtils
    {
        private static string _tempPath = Path.GetTempPath();

        public static VaultAppDefFile GetAppDef(string zipFile)
        {
            using (var zip = new ZipFile(zipFile, Encoding.Default))
            {
                var defEntry = zip.Entries.FirstOrDefault(c => 
                    c.FileName.EndsWith(VaultAppDefFile.FileName, StringComparison.OrdinalIgnoreCase));
                var appdefFile = Path.Combine(_tempPath, defEntry.FileName);
                if (File.Exists(appdefFile))
                {
                    File.Delete(appdefFile);
                }
                defEntry.Extract(_tempPath, ExtractExistingFileAction.OverwriteSilently);
                
                string err;
                var defObj = SerialUtils.GetObject<VaultAppDefFile>(appdefFile, out err);
                if (defObj == null)
                {
                    Trace.WriteLine("appdef.xml文件格式不正确:"+err);
                    return null;
                }
                return defObj;
            }
        }
        public static List<string> ExtractApps(string appFolder, string[] zipFiles)
        {
            var errFileList = new List<string>();
            if (zipFiles == null || zipFiles.Length == 0) return errFileList;
            foreach (var z in zipFiles)
            {
                var appdef = GetAppDef(z);
                if (appdef == null) continue;
                errFileList.AddRange(ExtractApp(appFolder, appdef.Guid, z));
            }
            return errFileList;
        }
        public static List<string> ExtractApp(string appFolder, string appGuid, string appZipPath)
        {
            var extPath = Path.Combine(appFolder, appGuid);
            if (!Directory.Exists(extPath))
            {
                CreateDirectory(extPath);
                //Directory.CreateDirectory(extPath);
            }
            var zipFilename = Path.GetFileNameWithoutExtension(appZipPath);
            var errFileList = new List<string>();
            using (var zip = new ZipFile(appZipPath, Encoding.Default))
            {
                var noDir = zip.Entries.Any(c => !c.FileName.Contains("/"));
                if (noDir)
                {
                    try
                    {
                        zip.ExtractAll(extPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch (Exception ex)
                    {
                        errFileList.Add(zipFilename + " # " + appZipPath);
                    }
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
                        catch (Exception ex)
                        {
                            errFileList.Add(zipFilename + " # " + fileName);
                        }
                    }
                }
            }
            return errFileList;
        }
        public static List<string> ExtractApps(string appFolder, string[] zipFiles, ILog log)
        {
            var errFileList = new List<string>();
            if (zipFiles == null || zipFiles.Length == 0) return errFileList;
            foreach (var z in zipFiles)
            {
                var appdef = GetAppDef(z);
                if (appdef == null) continue;
                if (log != null)
                {
                    log.Info("加载App：" + appdef.Name);
                }
                errFileList.AddRange(ExtractApp(appFolder, appdef.Guid, z, log));
            }
            return errFileList;
        }
        /// <summary>
        /// 是否需要更新App
        /// </summary>
        /// <param name="appFolder"></param>
        /// <param name="appGuid"></param>
        /// <param name="appZipFile"></param>
        /// <returns></returns>
        public static bool NeedUpdate(string appFolder, string appGuid, string appZipFile)
        {
            var destAppDefFile = Path.Combine(appFolder, appGuid, VaultAppDefFile.FileName);
            if (!File.Exists(destAppDefFile)) return true;
            var appDef = VaultAppDefFile.GetFromFile(destAppDefFile);
            var zipAppDef = GetAppDef(appZipFile);
            if (zipAppDef.Equals(appDef)) return false;
            return true;
        }

        private static void CreateDirectory(string dir)
        {
            var dir0 = dir.TrimEnd('\\');
            var paths = dir0.Split(new char[] {'\\'});
            int count = paths.Length;
            int index = count - 1;
            var p0 = dir0;
            while (!Directory.Exists(p0))
            {
                p0 = p0.Substring(0, p0.Length - paths[index].Length).TrimEnd('\\');
                index--;
            }
            for (var i = index+1; i < count; i++)
            {
                p0 = Path.Combine(p0, paths[i]);
                Directory.CreateDirectory(p0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appFolder">客户端的Apps目录</param>
        /// <param name="appGuid">app的GUID</param>
        /// <param name="appZipPath">App的Zip文件路径</param>
        internal static List<string> ExtractApp(string appFolder, string appGuid, string appZipPath, ILog log)
        {
            var extPath = Path.Combine(appFolder, appGuid);
            if (!Directory.Exists(extPath))
            {
                CreateDirectory(extPath);
                //Directory.CreateDirectory(extPath);
            }
            var zipFilename = Path.GetFileNameWithoutExtension(appZipPath);
            var errFileList = new List<string>();
            using (var zip = new ZipFile(appZipPath, Encoding.Default))
            {
                var noDir = zip.Entries.Any(c => !c.FileName.Contains("/"));
                if (noDir)
                {
                    try
                    {
                        zip.ExtractAll(extPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch (Exception ex)
                    {
                        if (log != null)
                        {
                            log.Error(zipFilename + " # " + appZipPath + ": " + ex.Message, ex);
                        }
                        errFileList.Add(zipFilename + " # " + appZipPath);
                    }
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
                        catch(Exception ex)
                        {
                            if (log != null)
                            {
                                log.Error(zipFilename + " # " + fileName + ": " + ex.Message, ex);
                            }
                            errFileList.Add(zipFilename+ " # "+ fileName);
                        }
                    }
                }
            }
            return errFileList;
        }
    }
}
