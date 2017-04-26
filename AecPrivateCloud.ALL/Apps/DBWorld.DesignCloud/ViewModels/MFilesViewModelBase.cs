using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MfilesClientCore;
using AecCloud.MFilesCore;
using AecCloud.PluginInstallation.VaultApps;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using MFilesAPI;
using Newtonsoft.Json;

namespace DBWorld.DesignCloud.ViewModels
{
    public abstract class MFilesViewModelBase : ViewModelBase
    {
        private readonly VaultClient _httpClient;

        protected internal DesignCloudViewModel _parent;
        protected MFilesViewModelBase(DesignCloudViewModel parent)
        {
            _parent = parent;
            _httpClient = _parent.Context.GetVaultClient();
        }
        /// <summary>
        /// 通过视图的相对路径获取URL
        /// </summary>
        /// <param name="viewName">去掉根目录后视图的全名</param>
        /// <returns></returns>
        protected internal string GetViewPath(string viewName)
        {
            var mfVault = _parent.GetCurrVault();
            if (mfVault == null) return String.Empty;
            return Path.Combine(mfVault.GetVaultURL(), viewName);
        }

        protected virtual ProjectDto GetProject()
        {
            ProjectDto proj = null;
            var currentProj = _parent.CurrProject;
            if (currentProj != null)
            {
                proj = _parent.AppModel.Projects.FirstOrDefault(c => c.Id == currentProj.ProjId);
            }
            else
            {
                proj = _parent.AppModel.Projects.FirstOrDefault();
            }
            return proj;
        }

        //protected virtual void LoadApps(ProjectDto proj)
        //{
        //    var token = _parent.BimToken;
        //    //var proj = GetProject();
        //    //if (proj == null) return;
        //    var guid = proj.Vault.Guid;
        //    var loaded = _parent.VaultAppDict.ContainsKey(guid);
        //    if (loaded) return;
        //    var res = _httpClient.GetApps(proj.Vault.Id, token).Result;
        //    var resStr = res.Content.ReadAsStringAsync().Result;
        //    if (!res.IsSuccessStatusCode)
        //    {
        //        return;
        //        //throw new Exception("无法获取需要加载的库应用：" + resStr);
        //    }
        //    var apps = JsonConvert.DeserializeObject<List<VaultAppModel>>(resStr);
        //    _parent.VaultAppDict.Add(guid, apps);
        //    var zipFiles = new List<string>();
        //    var exportPath = Path.GetTempPath();
        //    var appPath = ClientUtils.GetAppPath(guid);
        //    foreach (var a in apps)
        //    {
        //        var path = Path.Combine(exportPath, a.Guid + ".zip");
        //        File.WriteAllBytes(path, a.ZipFile);
        //        var needUpdate = VaultAppUtils.NeedUpdate(appPath, a.Guid, path);
        //        if (!needUpdate) continue;
        //        zipFiles.Add(path);
        //        //try
        //        //{
        //        //    ClientUtils.InstallAppAsVaultSysApp(guid, a.Guid, path);
        //        //    _parent.Log.Info("加载VaultApp成功!");
        //        //    //var updateRes = _httpClient.UpdateLoadedAppVersion(proj.Vault.Id, a.AppId, token).Result;
        //        //    //if (updateRes.IsSuccessStatusCode)
        //        //    //{
                        
        //        //    //}
        //        //}
        //        //catch(Exception ex)
        //        //{
        //        //    _parent.Log.Error("加载VaultApp失败：" + ex.Message);
        //        //}
        //    }
        //    try
        //    {
        //        VaultAppUtils.ExtractApps(appPath, zipFiles.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        _parent.Log.Warn("加载VaultApp失败： "+ ex.Message);
        //        var fileStr = zipFiles.Select(c => "\"" + c + "\"").ToArray();
        //        var location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //        var exeFile = Path.Combine(location, "AecCloud.ClientConsole.exe");
        //        var startInfo = new ProcessStartInfo
        //        {
        //            FileName = exeFile,
        //            Arguments = String.Format("-u 1 -t vaultapp -p \"{0}\" -d {1}", appPath.TrimEnd('\\'), String.Join(" ", fileStr)),
        //            UseShellExecute = true
        //        };
        //        var isAdmin = IsAdministrator();
        //        if (!isAdmin)
        //        {
        //            startInfo.Verb = "runas";
        //            startInfo.WorkingDirectory = Environment.CurrentDirectory;
        //        }
        //        var p = Process.Start(startInfo);
        //        var exitCode = -1;
        //        if (p != null)
        //        {
        //            p.WaitForExit();
        //            exitCode = p.ExitCode;
        //        }
        //        if (exitCode != 0)_parent.Log.Error("加载VaultApp失败："+exitCode+"\t" + ex.Message);
        //    }
        //}

        //protected static bool IsAdministrator()
        //{
        //    WindowsIdentity identity = WindowsIdentity.GetCurrent();

        //    if (identity != null)
        //    {
        //        var principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.Administrator);
        //    }

        //    return false;
        //}
        ///// <summary>
        ///// 获取当前项目所在的MFiles库对象
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Vault GetCurrentVault()
        //{
        //    var proj = GetProject();
        //    if (proj == null) return null;
        //    var vault = proj.Vault;
        //    if (!_parent.VaultDict.ContainsKey(vault.Guid))
        //    {
        //        _parent.LoadApps(proj);
        //         _parent.VaultDict.Add(vault.Guid, MFilesVault.GetUserVault(_parent.User, vault));
        //    }
        //    return _parent.VaultDict[vault.Guid];
        //}
        /// <summary>
        /// 通过视图ID获取URL
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        protected internal string GetViewPath(int viewId)
        {
            var mfVault = _parent.GetCurrVault();
            if (mfVault == null) return String.Empty;
            return mfVault.ViewOperations.GetViewLocationInClient(viewId, true);
        }
    }
}
