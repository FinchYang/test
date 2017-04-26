using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.Data.Mapping.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.ApiRequests;
using log4net;
using DBWorld.AecCloud.Web.Models;
using MFilesAPI;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Win32;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using PagedList;
using AecCloud.Core.Domain;
using Lookup = MFilesAPI.Lookup;
using View = MFilesAPI.View;

namespace DBWorld.AecCloud.Web.Controllers
{
    [Authorize]
    public class IntegratedManagementController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserService _userService;
        private readonly IVaultServerService _mFilesVaultService;
        private readonly IProjectService _projService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IProjectService _projectService;
        private readonly IMFilesVaultService _mfvaultService;
        private readonly IMfilesWebService _mfilesWebService;
       // private readonly IAuthenticationManager _authManager;

        public IntegratedManagementController(IVaultServerService mFilesVaultService, IUserService userService, IProjectService projService,
            IProjectMemberService projectMemberService, IProjectService projectService, IMFilesVaultService mfvaultService,
            IMfilesWebService mfilesWebService, IAuthenticationManager authManager,
            Microsoft.AspNet.Identity.Owin.SignInManager<User, long> signInManager, UserManager<User, long> userManager)
            : base(authManager, signInManager, userManager)
        {
            _mFilesVaultService = mFilesVaultService;
            _projService = projService;
            _userService = userService;
            _projectMemberService = projectMemberService;
            _projectService = projectService;
            _mfvaultService = mfvaultService;
            _mfilesWebService = mfilesWebService;
            //_authManager = authManager;
        }

        /// <summary>
        /// 获取当前用户的项目库
        /// </summary>
        /// <returns></returns>
        IEnumerable<ProjectDto> GetProjects4CurrentUser()
        {
            var list = new List<ProjectDto>();

                var userId = AuthUtility.GetUserId(User);//User.Identity.GetUserId();
                var projs = _projectMemberService.GetProjectsByUser(userId);

                foreach (var m in projs)
                {
                    try
                    {
                        var proj = _projectService.GetProjectById(m.ProjectId);
                        var vault = _mfvaultService.GetVaultById(proj.VaultId);
                        var projDto = proj.ToDto(vault, false);
                        list.Add(projDto);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("获取用户项目异常：{0}", ex.Message), ex);
                    }                    
                 }

            return list;
        }

        public ActionResult GetProjsByCompany(string companyName)
        {
            if (!IsPasswordAvailable())
            {//密码失效 重新登录
                return ReloginForCurrentUser();
            }
            var projList = new List<string>();
            var allProjs = _projService.GetAllProjects();
            foreach (var proj in allProjs)
            {
                if (proj.Company.Name == companyName || companyName == "所属单位")
                {
                    projList.Add(proj.Name);
                }
            }
            //projList.Add("项目A");
            //projList.Add("项目B");
            // return Json(projList, JsonRequestBehavior.AllowGet);
            var json = JsonConvert.SerializeObject(projList,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(json);
        }
        /// <summary>
        /// 获取工期节点统计数据
        /// </summary>
        [OutputCache(Duration=300)]
        public ActionResult GetScheduleControlData(string company, string proj, string projClass, string projLevel, string searchStr)
        {
            if (!IsPasswordAvailable())
            {//密码失效 重新登录
                return ReloginForCurrentUser();
            }           
            var guidAndIps = new Dictionary<string, string>();
            //查询符合条件的项目库
            var projs = GetProjects4CurrentUser();
            foreach (var project in projs)
            {
                if (company != project.Company && company != "所属单位") continue; //公司
                if (proj != project.Name && proj != "项目") continue; //项目
                if (projClass != project.ProjectClass && projClass != "项目类别") continue; //项目类别
                if (projLevel != project.Level && projLevel != "项目级别") continue; //项目等级
                var guid = project.Vault.Guid;
                var ip = project.Vault.Server.LocalIp;
                if (guidAndIps.ContainsKey(guid))
                {
                    Log.Error("已存在相同guid的库:" + guid);
                    continue;
                }
                guidAndIps.Add(guid, ip);
            }

            //取值
            var username = AuthUtility.GetUserName(User);
            var pwd = AuthUtility.GetUserPassword(User);
            var list = new List<ScheduleNode>();
            if (username != null && pwd != null)
            {
                try
                {
                    list = _mfilesWebService.ScheduleControlStatistics(guidAndIps, username, pwd, searchStr);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Get Schedule node  Data Error: {0}", ex.Message), ex);
                }
            }

            //return Json(list, JsonRequestBehavior.AllowGet);
            var json = JsonConvert.SerializeObject(list,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None
            });
            return Content(json);
        }


        /// <summary>
        /// 工期模块节点
        /// </summary>
        /// <param name="selectCorporation"></param>
        /// <param name="selectProjs"></param>
        /// <param name="selectProjClass"></param>
        /// <param name="selectLevel"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public ActionResult ScheduleControlStatistics(string selectCorporation, string selectProjs, string selectProjClass, string selectLevel, string searchString)
        {
            //公司名称
            var companys = _projService.GetAllCompany().Select(c => c.Name);
            ViewBag.Companies = String.Join("$", companys.ToArray());
            //项目类别
            var projClasses = ProjectClassList.Items;
            ViewBag.Projclass = String.Join("$", projClasses.ToArray());
            //项目级别
            var levels = _projService.GetLevels().Select(c => c.Name);
            ViewBag.projLevel = String.Join("$", levels.ToArray());
            return View();

        }
        /// <summary>
        /// 统计监理例会纪要审核记录
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 300)]
        public ActionResult SupervisorMeetingStatistics(string year, string month)
        {
            if (!IsPasswordAvailable())
            {//密码失效 重新登录
                return ReloginForCurrentUser();
            }
            if (string.IsNullOrEmpty(year) && string.IsNullOrEmpty(month))
            {
                year = DateTime.Now.Year.ToString();
                month = DateTime.Now.Month.ToString();
            }
            ViewBag.year0 = year;
            ViewBag.month0 = month;

            var guidAndIps = new Dictionary<string, string>();
            var list = new List<CompanyMeetingStatics>();
            var companies = _projService.GetAllCompany().Select(c => c.Name);
            foreach (string company in companies)
            {                
                if (company != "总部机关")
                {
                    list.Add(new CompanyMeetingStatics { CompanyName = company });
                } 
            }
  
            //获取项目
            var projs = GetProjects4CurrentUser();
            foreach (var proj in projs)
            {
                var guid = proj.Vault.Guid;
                var ip = proj.Vault.Server.LocalIp;
                if (guidAndIps.ContainsKey(guid))
                {
                    Log.Error("已存在相同guid的库:" + guid);
                    continue;
                }
                guidAndIps.Add(guid, ip);
            }
            //登录库 查询数据
            if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(month))
            {
                var username = AuthUtility.GetUserName(User);
                var pwd = AuthUtility.GetUserPassword(User);
                if (username != null && pwd != null)
                {
                    try
                    {
                        list = _mfilesWebService.SupervisorMeetingStatics(list, guidAndIps, username, pwd, year, month);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("Get Supervisor Meeting  Data Error: {0}", ex.Message), ex);
                    }
                }
            }

            return View(list.ToPagedList(1, 100000));
        }

        /// <summary>
        /// 履约率总表
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [OutputCache(Duration = 300)]
        public ActionResult PerformanceRate(string year, string month)
        {
            if (!IsPasswordAvailable())
            {//密码失效 重新登录
                return ReloginForCurrentUser();
            }
            if (string.IsNullOrEmpty(year) && string.IsNullOrEmpty(month))
            {
                year = DateTime.Now.Year.ToString();
                month = DateTime.Now.Month.ToString();
            }
            ViewBag.year0 = year;
            ViewBag.month0 = month;
            Log.Info("履约率查询年份=" + year + "月份=" + month);
            //IEnumerable<Project> allProjects = _projService.GetAllProjects(); //获取所有项目
            var guidAndIps = new Dictionary<string, string>();
            //获取所有项目guid和ip
            var projs = GetProjects4CurrentUser();
            foreach (var proj in projs)
            {
                var guid = proj.Vault.Guid;
                var ip = proj.Vault.Server.LocalIp;
                if (guidAndIps.ContainsKey(guid))
                {
                    Log.Error("已存在相同guid的库:" + guid);
                    continue;
                }
                guidAndIps.Add(guid, ip);
                Log.Info("履约率查询库：" + proj.Vault.Name);
            }
            //登录库 查询数据
            var list = new List<PerformanceRateModel>();
            var companies = _projService.GetAllCompany().Select(c => c.Name);
            foreach (string company in companies)
            {
                if (company != "总部机关")
                {
                    list.Add(new PerformanceRateModel { UnitName = company });
                }                
            }

            if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(month))
            {
                var username = AuthUtility.GetUserName(User);
                var pwd = AuthUtility.GetUserPassword(User);
                Log.Info("履约率查询用户名：" + username + "密码：" + pwd);
                if (username != null && pwd != null)
                {
                    try
                    {
                        var performRate = new MFilesPerformService();
                        Log.Info("履约率查询：调用查询程序");
                        list = performRate.GetPerformRate(list, guidAndIps, username, pwd, int.Parse(year), int.Parse(month));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("Get Performance Rate Data Error: {0}", ex.Message), ex);
                    }
                }
            }            
            return View(list.ToPagedList(1, 100000));
        }
        /// <summary>
        /// 单位履约率表
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 300)]
        public ActionResult PerformanceRateUnit(string unitName, string year, string month)
        {
            if (!IsPasswordAvailable())
            {//密码失效 重新登录
                return ReloginForCurrentUser();
            }
            var guidAndIps = new Dictionary<string, string>();
            var projs = GetProjects4CurrentUser();
            foreach (var proj in projs)
            {
                var guid = proj.Vault.Guid;
                var ip = proj.Vault.Server.LocalIp;
                if (guidAndIps.ContainsKey(guid))
                {
                    Log.Error("已存在相同guid的库:" + guid);
                    continue;
                }
                guidAndIps.Add(guid, ip);
            }
            //登录库 查询数据
            var list = new List<UnitPerformaceModel>();
            var username = AuthUtility.GetUserName(User);
            var pwd = AuthUtility.GetUserPassword(User);
            if (username != null && pwd != null)
            {
                try
                {
                    var performRate = new MFilesPerformService();
                    list = performRate.GetPerformRateUnit(guidAndIps, username, pwd, int.Parse(year), int.Parse(month), unitName);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Get Unit Performance Rate Data Error: {0}", ex.Message), ex);
                }
            }
            return View(list.ToPagedList(1, 100000));
        }
    }
}
