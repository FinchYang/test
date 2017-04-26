using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AecCloud.Core.Domain.Vaults;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using log4net;

namespace DBWorld.AecCloud.Web.Api
{
    [Authorize]
    public class VaultController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IVaultAppService _vaultappService;
        private readonly IMFilesVaultService _vaultService;

        public VaultController(IMFilesVaultService vaultService, IVaultAppService vaultappService)
        {
            _vaultService = vaultService;
            _vaultappService = vaultappService;
        }

        /// <summary>
        /// 获取对应Vault上所有的VaultApp:
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Apps(long id)
        {
            var apps = await Task.Run(() => GetAppsByVault(id));
            return Ok(apps);
        }

        [HttpPost]
        public async Task<IHttpActionResult> AppsNeeded([FromUri]long id, [FromBody] AppDescList appList)
        {
            AppDesc[] apps0 = null;
            if (appList != null && appList.Apps != null)
            {
                apps0 = appList.Apps.ToArray();
            }
            var apps = await Task.Run(() => GetAppsByVault(id, apps0));
            Log.Info(" AppsNeeded :"+apps.Count);
            return Ok(apps);
        }
        [HttpGet]
        public async Task<IHttpActionResult> CloudDisc()
        {
            var apps = await Task.Run(() => GetCloudDiscApps(null));
            return Ok(apps);
        }

        [HttpPost]
        public async Task<IHttpActionResult> CloudDiscAppNeeded(AppDescList appList)
        {
            var apps = await Task.Run(() => GetCloudDiscApps(appList));
            return Ok(apps);
        }

        private List<VaultAppModel> GetCloudDiscApps(AppDescList appList)
        {
            var appPath = Path.Combine(Global.ServerPath, @"App_Data\CloudApps.zip");
            var va = new VaultApp {Filepath = appPath};
            va.SetPropertiesFromAppDefFile();
            var models = new List<VaultAppModel>();
            var needGet = true;
            if (appList != null && appList.Apps != null && appList.Apps.Count > 0)
            {
                var a = //是否存在版本和GUID相同的App
                    appList.Apps.FirstOrDefault(c => c.Guid.ToUpper() == va.Guid.ToUpper() && c.Version == va.Version);
                if (a != null) needGet = false; //若存在，则不再下载
            }
            if (needGet)
            {
                var m = ToModel(va, false);
                models.Add(m);
            }
            return models;
        }
        //todo
        private List<VaultAppModel> GetAppsByVault(long vaultId, params AppDesc[] apps)
        {
            //1. 获取库的模板
            var vault = _vaultService.GetVaultById(vaultId);
            var tempId = vault.TemplateId;
            //2. 获取跟库模板关联的VaultApp
            Log.Info("GetAppsByVault tempid:"+tempId);
            return GetAppsByTemplate(tempId, apps);

        }

        private VaultAppModel ToModel(VaultApp app, bool needImpersonate, bool isUpdate = false)
        {
            if (app == null) throw new ArgumentNullException("app");
            var vam = new VaultAppModel
            {
                AppId = app.Id,
                Guid = app.Guid,
                //CloudAppEnabled = app.CloudAppEnabled,
                IsUpdate = isUpdate,
                Version = app.Version
            };
            try
            {
                if (needImpersonate)
                {
                    using (StorageUtility.GetImpersonator())
                    {
                        vam.ZipFile = File.ReadAllBytes(app.Filepath);
                    }
                }
                else
                {
                    vam.ZipFile = File.ReadAllBytes(app.Filepath);
                }
            }
            catch (Exception ex)
            {
                Log.Info(app.Filepath+ex.Message);
            }
            return vam;
        }

        private List<VaultAppModel> GetAppsByTemplate(long tempId , params AppDesc[] apps)
        {
            var appsWithTemplates = _vaultappService.GetAppsWithTemplate(tempId);
            var apps0 = appsWithTemplates.Select(c => _vaultappService.GetById(c.VaultAppId)).ToList();
            if (apps == null || apps.Length == 0)
            {
              //  Log.Info("vault controller GetAppsByTemplate 3 :" + apps0.Select(a => ToModel(a, false)).ToList().Count);
                return apps0.Select(a => ToModel(a, false)).ToList();
            }
            var appList = new List<VaultAppModel>();
            foreach (var a in apps0)
            {
                var aa = apps.FirstOrDefault(c => c.Guid.ToUpper() == a.Guid.ToUpper() && c.Version == a.Version);
                if (aa != null) continue;
                appList.Add(ToModel(a, true));
            }
            return appList;
        }
  
    }
}
