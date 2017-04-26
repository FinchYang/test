using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MfilesServices;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using DBWorld.AecCloud.Web.Models;
using log4net;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;

using AecCloud.Service.Apps;
using AecCloud.WebAPI.Models;

namespace DBWorld.AecCloud.Web.Api
{
    [Authorize]
    public class CloudController : ErrorHandlingApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICloudService _cloudService;
        //private readonly IUserCloudService _usercloudService;
        private readonly IUserVaultService _uservaultService;
        private readonly IVaultTemplateService _vaultTemplateService;
        private readonly IRepository<Company> _companyRepo;
        private readonly IMFVaultService _vaultService;
        private readonly IUserService _userService;
        private readonly IRepository<Area> _areaRepo;
        private readonly IMFilesVaultService _mfvaultService;

        public CloudController(ICloudService cloudService
            , IUserVaultService uservaultService, IVaultTemplateService vaultTemplateService, IRepository<Company> companyRepo, IRepository<Area> areaRepo
            ,IMFVaultService vaultService, IUserService userService, IMFilesVaultService mfvaultService, IAuthenticationManager authenticationManager)
            : base(authenticationManager) //, IUserCloudService usercloudService
        {
            _cloudService = cloudService;
            _companyRepo = companyRepo;
            _areaRepo = areaRepo;
            _uservaultService = uservaultService;
            _vaultTemplateService = vaultTemplateService;
            _vaultService = vaultService;
            _userService = userService;
            _mfvaultService = mfvaultService;
        }

        
        /// <summary>
        /// 获取用户的云列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Clouds()
        {
            var userId = Authentication.User.Identity.GetUserId<int>();
            try
            {
                var user = _userService.GetUserById(userId);
                var roles = AuthUtility.GetUserRoles(User.Identity);
                var host = GetHost();
                Log.Info("before await Task.Run(() => GetClouds");
                var uApps = await Task.Run(() => GetClouds(user, _uservaultService, _cloudService, _vaultTemplateService, _vaultService,_mfvaultService, host, roles));
                Log.Info("after await Task.Run(() => GetClouds");
                return Ok(uApps);
            }
            catch (Exception ex)
            {
                Log.Info("Exception await Task.Run(() => GetClouds");
                return CreateErrorResponse("获取用户的云失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 获取云的模板列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Templates(int id)
        {
            try
            {
                var vaults =
                    await Task.Run(() => _vaultTemplateService.GetTemplatesByCloud(id).Select(c => c.ToDto()).ToList());
                return Ok(vaults);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(string.Format("获取模板出错,id={0}：",id), HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Companies()
        {
            return Ok(_companyRepo.Table.Select(c=>new CompanyDto{Id = c.Id, Name=c.Name}));
        }

        /// <summary>
        /// 获取地区列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Areas()
        {
            return Ok(_areaRepo.Table.Select(c => new AreaDto { Id = c.Id, Name = c.Name }));
        }

        public static UserCloudModel GetClouds(User user, IUserVaultService _uservaultService //IUserCloudService _usercloudService, 
            , ICloudService _cloudService, IVaultTemplateService _vaultTemplateService, IMFVaultService _vaultService, IMFilesVaultService _mfvaultService, string host, params string[] roleNames)
        {
            var clouds = _cloudService.GetCloudsByUserRoles(roleNames);
            var userId = user.Id;

            var appModel = new UserCloudModel();
            var userVaults = _uservaultService.GetVaults(userId);
            foreach (var app in clouds)
            {
                var a = app;
                var appM = new CloudModel { App = a.ToDto() };
                if (app.Id == CloudConstants.MyProjects)
                {
                    var templates = new List<VaultTemplate>();
                    foreach (var t in _vaultTemplateService.GetTemplatesByCloud(app.Id))
                    {
                        templates.Add(t);
                        //a.Templates.Add(t);
                    }
                    if (templates.Count > 0)
                    {
                        var appVaults =
                            userVaults.Where(c => templates.Any(d => c.TemplateId > 0 && c.TemplateId == d.Id));
                        appM.Vaults.AddRange(appVaults.Select(c => c.ToDtoWithoutTemplate()));
                    }
                    appM.Url = "/IntegratedManagement/Index";
                }
                //else if (app.Id == CloudConstants.ProjManagements)
                //{
                //    //todo 指定路径
                    
                //}
                appModel.Apps.Add(appM);
            }
            var password = DBWorldCache.Get(userId.ToString());
            var appVaultsC = _mfvaultService.GetVaultsByCloud(CloudConstants.SubContracts);
            var vv = new List<MFilesVault>();
            foreach (var v in appVaultsC)
            {
                if (_vaultService.HasUser(user, password, v))
                {
                    vv.Add(v);
                }
            }
            if (vv.Count > 0)
            {
                var cc = _cloudService.GetCloudById(CloudConstants.SubContracts);
                var cM = new CloudModel { App = cc.ToDto() };
                cM.Vaults.AddRange(vv.Select(c => c.ToDtoWithoutTemplate()));
                appModel.Apps.Add(cM);
            }
            return appModel;
        }
        /// <summary>
        /// 获取所有的云
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> AllClouds()
        {
            try
            {
                var apps = await Task.Run(() => _cloudService.GetAllClouds());
                var clouds = apps.Select(c => c.ToDto()).ToList();
                return Ok(clouds);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取所有云失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

    }
}
