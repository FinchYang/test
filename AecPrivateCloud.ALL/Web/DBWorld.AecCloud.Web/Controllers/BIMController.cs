using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using AecCloud.BaseCore;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using AecCloud.WebAPI.Models.DataAnnotations;
using DBWorld.AecCloud.Web.Api;
using DBWorld.AecCloud.Web.ApiRequests;
using DBWorld.AecCloud.Web.Models;
using log4net;
using MFilesAPI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Controllers
{

    [Authorize]
    public class BIMController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProjectMemberService _projMemberService;
        private readonly IProjectService _projService;
        private readonly IMFilesVaultService _mfvaultService;
        private readonly IVaultTemplateService _vaultTemplateService;
        private readonly IMFUserService _vaultUserService;
        private readonly IMFObjectService _mfilesObjService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IUserVaultService _uservaultService;
        private readonly IUserService _userService;
        private readonly IVaultServerService _vaultServerService;
        private readonly ICloudService _cloudService;
        private readonly IMFVaultService _vaultService;
        private readonly IMfUserGroupService _userGroupService;
        private readonly IEmailService _emailService;
        private readonly IRepository<Area> _areaRepo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<ProjectLevel> _ProjectLevelRepo;
        private readonly IRepository<ProjectClass> _ProjectClassRepo;
        public BIMController(IAuthenticationManager authManager, SignInManager<User, long> signInManager, UserManager<User, long> userManager
            , IProjectMemberService projMemberService, IRepository<Area> areaRepo,
            IRepository<Company> companyRepo, IRepository<ProjectLevel> ProjectLevelRepo,IRepository<ProjectClass> ProjectClassRepo,
            IProjectService projService, IMFilesVaultService mfvaultService, IVaultTemplateService vaultTemplateService, IEmailService emailService
            , IMFUserService vaultUserService, IMFObjectService mfilesObjService, IProjectMemberService projectMemberService, IUserVaultService uservaultService
            , IUserService userService, IVaultServerService vaultServerService, ICloudService cloudService, IMFVaultService vaultService, IMfUserGroupService userGroupService)
            : base(authManager, signInManager, userManager)
        {
            _projMemberService = projMemberService;
            _projService = projService;
            _mfvaultService = mfvaultService;
            _vaultTemplateService = vaultTemplateService;
            _vaultUserService = vaultUserService;
            _mfilesObjService = mfilesObjService;
            _projectMemberService = projectMemberService;
            _uservaultService = uservaultService;
            _userService = userService;
            _vaultServerService = vaultServerService;
            _cloudService = cloudService;
            _vaultService = vaultService;
            _userGroupService = userGroupService;
            _emailService = emailService;
            _areaRepo = areaRepo;
            _companyRepo = companyRepo;
            _ProjectLevelRepo = ProjectLevelRepo;
            _ProjectClassRepo = ProjectClassRepo;
        }


        private List<ProjectDto> GetProjectList()
        {
            return Api.ProjectController.GetProjectsByUser(User.Identity.GetUserId<long>(), _projMemberService,
                _projService, _mfvaultService, _vaultTemplateService);
        }
        IEnumerable<ProjectDto> GetProjects4CurrentUser()
        {
            var list = new List<ProjectDto>();
            try
            {
                var userId = AuthUtility.GetUserId(User);//User.Identity.GetUserId();
                var projs = _projectMemberService.GetProjectsByUser(userId);
            //    Log.InfoFormat("in bim GetProjects4CurrentUser _projectMemberService.GetProjectsByUser count={0}, userId: {1} ",projs.Count, userId);
                foreach (var m in projs)
                {
                    try { 
                    var proj = _projService.GetProjectById(m.ProjectId);
                    if (proj == null)
                    {
                      //  Log.WarnFormat("项目({0})已被删除", m.ProjectId);
                        continue;
                    }
                    var vault = _mfvaultService.GetVaultById(proj.VaultId);
                    if (vault == null)
                    {
                      //  Log.WarnFormat("文档库({0})已被删除", proj.VaultId);
                        continue;
                    }
                    var projDto = proj.ToDto(vault, false);
                    list.Add(projDto);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("用户项目 ProjectId= {1} 可能已被清除：{0}", ex.Message, m.ProjectId), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取用户项目异常：{0}", ex.Message), ex);
            }
          //  Log.Info(string.Format("in bim GetProjects4CurrentUser return list count={0} ", list.Count));
            return list;
        }
        private string ProjectCreationPermission(long userid)
        {
            var ret = string.Empty;
            using (var sqlc = new SqlConnection(ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString))
            using ( var sqlcommand = new SqlCommand("select id from erppm where userid =@id ", sqlc))
            {
                sqlcommand.Parameters.Add("id",SqlDbType.BigInt).Value=userid;
                sqlc.Open();
                try
                {
                    var result = sqlcommand.ExecuteScalar();
                    Log.Info(string.Format("result=[{0}]",result));
                    if(result==DBNull.Value||result==null||result.ToString()==string.Empty)
                    {
                        ret = string.Format("您没有创建项目的权限，请联系公司总部工程管理部ERP立项信息负责人! ");
                    }
                }
                catch (Exception ex)
                {
                    Log.Info(string.Format("ProjectCreationPermission {0}  ", ex.Message));
                    ret = ex.Message;
                }
            }
            Log.Info(string.Format("ProjectCreationPermission return={0}.", ret));
            return ret;
        }
        public ActionResult Index()
        {
            //先获取用户ID，一边后期在存储默认项目时区分不同用户使用
            try
            {
                var profile = GetUserProfile(_userManager);

                ViewBag.userId = profile.Id;
                ViewBag.cancreateproject = ProjectCreationPermission(profile.Id);
                var projs = GetProjects4CurrentUser();
                var currentuserid = long.Parse(User.Identity.GetUserId());
                Log.Info(string.Format("bim index,{0},{1},{2}", profile.Id, currentuserid, ViewBag.cancreateproject));
                foreach (ProjectDto projectDto in projs)
                {
                    try
                    {
                        //Log.Info(string.Format("before connect:{0},{1},{2},{3},{4},{5},{6},{7},{8}", loginName, password,
                        //    projectDto.Vault.Server.Ip,
                        //    projectDto.Vault.Server.Port, user.Id, user.FullName, user.UserName, user.Password,
                        //    user.PasswordHash));
                        var app = MFServerUtility.ConnectToServer(_mfvaultService.GetServer(projectDto.VaultId));
                        ;
                     //   Log.Info("after connect" + projectDto.Vault.Guid);
                        var vault = app.LogInToVault(projectDto.Vault.Guid);
                     //   Log.Info("after login");
                     //   var mfuserid = MfUserUtils.GetUserAccount(vault, loginName);
                        //if (mfuserid != null)
                        //{
                        //    var UgEngineeringManagementDepartment =
                        //        vault.GetMetadataStructureItemIDByAlias(
                        //            MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        //            "UgEngineeringManagementDepartment");
                        //    var UGroupPM =
                        //        vault.GetMetadataStructureItemIDByAlias(
                        //            MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        //            "UGroupPM");
                        //    Log.Info(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                        //        currentuserid, mfuserid, loginName, projectDto.Vault.Name, vault.Name, UGroupPM,
                        //        UgEngineeringManagementDepartment));
                        //    bool useringroup = IsUserInGroup((int) mfuserid, UgEngineeringManagementDepartment, vault);
                        //    bool useringroup1 = IsUserInGroup((int) mfuserid, UGroupPM, vault);
                        //    if (!(useringroup || useringroup1))
                        //    {
                        //        projectDto.NotDisplay = true;
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    projectDto.NotDisplay = true;
                        //    continue;
                        //}
                        var viewid = -1;
                        try
                        {
                            var views = vault.ViewOperations.GetViews();
                            foreach (View view in views)
                            {
                                if (view.Name == "主目录")
                                {
                                    viewid = view.ID;
                                    break;
                                }
                            }
                            if (viewid < 0)
                            {
                                foreach (View view in views)
                                {
                                    if (view.Name == "根目录")
                                    {
                                        viewid = view.ID;
                                        break;
                                    }
                                }
                            }
                            projectDto.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                        }
                        catch (Exception ex)
                        {
                            Log.Info("search time project :" + projectDto.Name, ex);
                        }
                        vault.LogOutSilent();
                        app.Disconnect();
                    }
                    catch (Exception )
                    {
                        projectDto.NotDisplay = true;
                     //   Log.Info("查询有权限的项目：" + "测试库已清除");
                        continue;
                    }
                }
                var bim = new BIMModel()
                {
                    ProjectDto = projs.Where(c => c.NotDisplay != true).ToList()
                };
             //   Log.Info(string.Format("project center：all={0},display={1}" ,projs.Count(),bim.ProjectDto.Count));
                return View(bim);
            }
            catch (Exception ex)
            {
              //  SimpleLog("bim index error :"+ex.Message);
                return View(new BIMModel
                {
                    ProjectDto = new List<ProjectDto>()
                });
            }
        }
        //private bool IsUserInGroup(int uid, int ugid, Vault vault)
        //{
        //    var ids = vault.UserGroupOperations.GetUserGroup(ugid).Members;
        //    foreach (int id in ids)
        //    {
        //        if (id < 0)
        //        {
        //            return IsUserInGroup(uid, -id, vault);
        //        }
        //        if (id == uid) return true;
        //    }
        //    return false;
        //}
        //获取项目
        //public ActionResult GetProjects()
        //{
        //    //var baseUri = GetHost();
        //    //var token = await GetToken();
        //    //if (token == null) return await ReturnToLogin();
        //    //var res = await ProjectClient.GetProjects(baseUri, token);

        //    //var json = res.Content;
        //    var projs = GetProjectList(); //JsonConvert.DeserializeObject<List<ProjectDto>>(json);
        //    var bim = new BIMModel()
        //    {
        //        ProjectDto = projs
        //    };
        //    return View(bim);
        //}

        //获取类别
        public ActionResult GetTemplates()
        {
         //   var res = HomeClient.GetProjectTemplates(_vaultTemplateService).Where(c=>c.Id<4).Select(c=>c.ToDto());//await ProjectClient.GetProjectTemplates(baseUri, token);
            var res = HomeClient.GetProjectTemplates(_vaultTemplateService).Select(c => c.ToDto());//await ProjectClient.GetProjectTemplates(baseUri, token);
            var json = JsonConvert.SerializeObject(res,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(json);
        }

        public ViewResult ProjectDetail()
        {
            var id = long.Parse(Request.QueryString["projectId"]);
            var p = Api.ProjectController.GetProject(id, _projService, _mfvaultService, _vaultTemplateService);  //JsonConvert.DeserializeObject<ProjectDto>(json);
            var pe = new ProjectEditModel
            {
                Id = p.Id,
                Name = p.Name,
                Number = p.Number,
                Description = p.Description,
                StartDateUtc = p.StartDateUtc.ToLocalTime(),
                EndDateUtc = p.EndDateUtc.ToLocalTime(),
                Cover = p.Cover,
                CompanyId= p.CompanyId,
               ProjectClass=p.ProjectClass,
               Level=p.Level,
               Area = p.Area,
               ConstructionScale = p.ConstructionScale,
               ContractAmount = p.ContractAmount,
                PmUnit = p.PmUnit,
                InvestigateUnit = p.InvestigateUnit,
                Company = p.Company,
                TemplateId = p.TemplateId

            };
            return View(pe);
        }

        //根据项目ID获取项目封面
        public ActionResult GetImage()
        {
            int id = int.Parse(Request.QueryString["projectId"]);
            var p = Api.ProjectController.GetProject(id, _projService, _mfvaultService, _vaultTemplateService);  //JsonConvert.DeserializeObject<ProjectDto>(json);
            if (p.Cover != null)
            {
                return File(p.Cover, "image/jpeg");
            }
            else
            {

                return File(Server.MapPath("~/Content/Images/11.jpg"), "image/jpeg");
            }
        }

        //根据项目ID获取项目详情（项目封面采用action方式获取）
        public ActionResult GetProjectDetail(int id)
        {
            var p = Api.ProjectController.GetProject(id, _projService, _mfvaultService, _vaultTemplateService);  //JsonConvert.DeserializeObject<ProjectDto>(json);
            p.Cover = null;
            p.StartDateUtc = p.StartDateUtc.ToLocalTime();
            p.EndDateUtc = p.EndDateUtc.ToLocalTime();
            var str = JsonConvert.SerializeObject(p);
            return Content(str);
        }

        //更新项目，图片采用流的形式提交
        [HttpPost]
        public ActionResult UpdateProject(ProjectEditModel model, string imgSrc)
        {
            HttpPostedFileBase file = Request.Files["projectImg"];
            Stream fileStream = file.InputStream;
            int fileLength = file.ContentLength;
            if (fileLength == 0 && imgSrc == "")
            {                
                ProjectDto p = Api.ProjectController.GetProject(model.Id, _projService, _mfvaultService, _vaultTemplateService);  //projs.FirstOrDefault(c => c.Id == model.Id);
                model.Cover = p.Cover;
            }
            else if (fileLength == 0 && imgSrc != "")
            {
                byte[] img = GetPictureData(Server.MapPath(imgSrc));
                model.Cover = img;
            }
            else
            {
                byte[] img = new byte[fileLength];
                fileStream.Read(img, 0, fileLength);
                fileStream.Close();
                model.Cover = img;
            }

            var res = Api.ProjectController.UpdateProject(model, User.Identity.GetUserName(), _projService,
                _mfvaultService, _userGroupService, _vaultTemplateService, _mfilesObjService);//await ProjectClient.UpdateProject(baseUri,token, model);
            if (String.IsNullOrEmpty(res))
            {
                return RedirectToAction("ProjectDetail", "BIM", new { projectId = model.Id });
            }
            return RedirectToAction("ProjectDetail", "BIM", new { projectId = model.Id });
        }
      
        public byte[] GetPictureData(string imagepath)
        {
            //根据图片文件的路径使用文件流打开，并保存为byte[]

            var fs = new FileStream(imagepath, FileMode.Open, FileAccess.Read);
            var byData = new byte[fs.Length];
            fs.Read(byData, 0, byData.Length);
            fs.Close();
            return byData;
        }

      
        public ViewResult CreateProject(string areas,string gongsi)
        {
            ViewBag.areas = new SelectList(_areaRepo.Table,  "Id", "Name");
            ViewBag.gongsi = new SelectList(_companyRepo.Table, "Id", "Name");

            return View();
        }
    
     
        //创建项目，图片采用流的形式提交
        [HttpPost]
        //[AllowAnonymous]
        public ActionResult CreateProject(ProjectCreateModel model, string imgSrc)
        {
         //   if (!ModelState.IsValid) return View(model);
            ViewBag.areas = new SelectList(_areaRepo.Table, "Id", "Name");
            ViewBag.gongsi = new SelectList(_companyRepo.Table, "Id", "Name");
            model.AreaId = int.Parse(model.Area);
            model.CompanyId = int.Parse(model.Company);
            model.TemplateId = 4;
            if (model.StartDateUtc > model.EndDateUtc)
            {
                Log.Info(model.StartDateUtc + "开始日期不能大于截止日期" + model.EndDateUtc);
                ModelState.AddModelError("", "开始日期不能大于截止日期");
                return View(model);
            }
            if (model.TemplateId <= 0)
            {
                ModelState.AddModelError("", "未给出模板ID="+model.TemplateId);
                Log.Error("未给出模板ID="+model.TemplateId);
                return View(model);
            }
            model.StartDateUtc = model.StartDateUtc.ToUniversalTime();
            model.EndDateUtc = model.EndDateUtc.ToUniversalTime();
            HttpPostedFileBase file = Request.Files["projectImg"];
            Stream fileStream = file.InputStream;
            int fileLength = file.ContentLength;
            //如果前段没有采用本地上传图片方式，则采用默认记录的图片为cover的值
            if (fileLength == 0)
            {
                try
                {
                    byte[] img = GetPictureData(Server.MapPath(imgSrc));
                    if (img.Length > Utility.MaxImageLength)
                    {
                        ModelState.AddModelError("", String.Format("图片大小不能超过{0}K", Utility.MaxImageLength / 1024));
                        return View(model);
                    }
                    model.Cover = img;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    ModelState.AddModelError("", ex);
                    return View(model);
                }
            }
            else
            {
                try
                {
                    var img = new byte[fileLength];
                    if (img.Length > Utility.MaxImageLength)
                    {
                        ModelState.AddModelError("", String.Format("图片大小不能超过{0}K", Utility.MaxImageLength/1024));
                        return View(model);
                    }
                    fileStream.Read(img, 0, fileLength);
                    fileStream.Close();
                    model.Cover = img;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    ModelState.AddModelError("", "设置图片异常："+ex.Message);
                    return View(model);
                }

            }
            try
            {
                var projRes = Api.ProjectController.CreateProjectALL(model, User.Identity.GetUserId<long>(), _userService,
                    _projService, _vaultServerService
                    , _cloudService, _vaultService, _vaultTemplateService, _vaultUserService, _mfilesObjService,
                    _mfvaultService, _projectMemberService, _uservaultService);
                if (projRes.Project != null)
                {
                    return RedirectToAction("Index", "BIM");
                }

                Log.Error(projRes.Error);
                ModelState.AddModelError("", projRes.Error);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "创建项目异常："+ex.Message);
                Log.Error("创建项目异常：" + ex.Message, ex);
                return View(model);
            }
            return View(model);
        }

       
        public ActionResult CreateProjectForFullBackup(string areas, string gongsi)
        {
            var passOK = IsPasswordAvailable();
            if (!passOK)
            {
                return ReloginForCurrentUser();
            }
            ViewBag.areas = new SelectList(_areaRepo.Table, "Id", "Name");
            ViewBag.gongsi = new SelectList(_companyRepo.Table.Where(c => c.Code != "0001A210000000002OSD"), "Id", "Name");
            ViewBag.projectlevel = new SelectList(_ProjectLevelRepo.Table, "Id", "Name");            
         //   ViewBag.projectlevel = new SelectList(new String[] { "一般/直属项目", "公司重点项目", "局重点项目" }, "Id", "Name");
            ViewBag.projectclass = new SelectList(_ProjectClassRepo.Table.ToList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        //[AllowAnonymous]
        public async Task<ActionResult> CreateProjectForFullBackup(ProjectCreateModel model, string imgSrc)
        {
            var passOK = IsPasswordAvailable();
            if (!passOK)
            {
                return ReloginForCurrentUser();
            }
       
            ViewBag.projectlevel = new SelectList(_ProjectLevelRepo.Table, "Id", "Name");
            ViewBag.projectClass = new SelectList(_ProjectClassRepo.Table, "Id", "Name");
            ViewBag.areas = new SelectList(_areaRepo.Table, "Id", "Name");
            ViewBag.gongsi = new SelectList(_companyRepo.Table, "Id", "Name");
            model.AreaId = int.Parse(model.Area);
            model.CompanyId = int.Parse(model.Company);
            model.ProjectLevelId = int.Parse(model.ProjectLevel);
            model.ProjectClassId = int.Parse(model.ProjectClass);
           // model.ProjectClass = Request.Form["projClass"];
            //Log.Info("Company?=" + model.CompanyId);
            //Log.Info("id?=" + model.AreaId);
            //Log.Info("ProjectLevelId?=" + model.ProjectLevelId);
            if (model.StartDateUtc > model.EndDateUtc)
            {
                Log.Info(model.StartDateUtc + "开始日期不能大于截止日期" + model.EndDateUtc);
                ModelState.AddModelError("", "开始日期不能大于截止日期");
                return View(model);
            }

            Log.Info("模型列表：" + model.ModelList);
           
            model.StartDateUtc = model.StartDateUtc.ToUniversalTime();
            model.EndDateUtc = model.EndDateUtc.ToUniversalTime();
            HttpPostedFileBase file = Request.Files["projectImg"];
            Stream fileStream = file.InputStream;
            int fileLength = file.ContentLength;
            //如果前段没有采用本地上传图片方式，则采用默认记录的图片为cover的值
            if (fileLength == 0)
            {
                try
                {
                    byte[] img = GetPictureData(Server.MapPath(imgSrc));
                    if (img.Length > Utility.MaxImageLength)
                    {
                        ModelState.AddModelError("", String.Format("图片大小不能超过{0}K", Utility.MaxImageLength / 1024));
                        return View(model);
                    }
                    model.Cover = img;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    ModelState.AddModelError("", ex);
                    return View(model);
                }
            }
            else
            {
                try
                {
                    var img = new byte[fileLength];
                    if (img.Length > Utility.MaxImageLength)
                    {
                        ModelState.AddModelError("", String.Format("图片大小不能超过{0}K", Utility.MaxImageLength / 1024));
                        return View(model);
                    }
                    fileStream.Read(img, 0, fileLength);
                    fileStream.Close();
                    model.Cover = img;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    ModelState.AddModelError("", "设置图片异常：" + ex.Message);
                    return View(model);
                }

            }            
            try
            {
                if (NeedWait())
                {
                    ModelState.AddModelError("", "有其他人先于您 正在 创建项目，请5分钟后再尝试点击确定按钮创建项目，谢谢！");
                    return View(model);
                }
                string[] models = null;
                //  Log.Info("ModelList: " + model.ModelList);
                if (!String.IsNullOrEmpty(model.ModelList))
                {
                    models = model.ModelList.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                }
                var userId = User.Identity.GetUserId<long>();
                var password = GetPassword(); 
                var projRes = ProjectController.CreateProjectForAllBackup(model, userId, _userService,
                    _projService, _vaultServerService
                    , _cloudService, _vaultService, _vaultTemplateService, _vaultUserService, _mfilesObjService,
                    _mfvaultService, _projectMemberService, _uservaultService, _userGroupService, password, models);
                ReleaseMutexForProjectCreating();
               
                if (projRes.Project != null)
                {
                    var url = Utility.GetHost(Request.Url);
                    var proj = projRes.Project;
                   await Task.Run(() => PushNewProject(new MfilesClientConfig
                    {
                        Guid = proj.Vault.Guid,
                        Name = proj.Name,
                        Host = proj.Vault.Server.Ip
                    }, url));
                    return RedirectToAction("Index", "BIM");
                }

                Log.Error(projRes.Error);
                ModelState.AddModelError("", projRes.Error);
            }
            catch (Exception ex)
            {
                ReleaseMutexForProjectCreating();
                ModelState.AddModelError("", "创建项目异常：" + ex.Message);
                Log.Error("创建项目异常：" + ex.Message, ex);
                return View(model);
            }
            return View(model);
        }
        private static IHubProxy HubProxy { get; set; }
        private static HubConnection Connection { get; set; }
        private static async void PushNewProject(MfilesClientConfig mcc,string homeurl)
        {
            try
            {
                Connection = new HubConnection(homeurl);
                HubProxy = Connection.CreateHubProxy("CscecPushHub");
                try
                {
                    await Connection.Start();
                }
                catch (HttpRequestException hex)
                {
                    Log.Info(
                        "Unable to connect to server: Start server before connecting clients.HttpRequestException" +
                        hex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Info("Unable to connect to server: Start server before connecting clients." + ex.Message);
                    return;
                }
                await HubProxy.Invoke("PushNewProject", mcc);
            }
            catch (Exception ex)
            {
                Log.Info("PushNewProject." + ex.Message);
            }
            finally
            {
                if (Connection != null)
                {
                    Connection.Stop();
                    Connection.Dispose();
                }
            }
        }

        private void ReleaseMutexForProjectCreating()
        {
            try {
                using ( var sqlc = new SqlConnection( ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString))
                {
                    sqlc.Open();
                    var update =
                              string.Format("update MutexForProjectCreating set  creating = 0 ");
                    var sqlcommand1 = new SqlCommand(update, sqlc);
                    sqlcommand1.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Info("ReleaseMutexForProjectCreating error:" + ex.Message);
            }
        }

        private bool NeedWait()
        {
            var ret = false;
            try
            {
                var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select1 = string.Format("select * from MutexForProjectCreating where creating != 0 ");
                var sqlcommand1 = new SqlCommand(select1, sqlc);
                var rds1 = new SqlDataAdapter(sqlcommand1);
                var dt1 = new DataTable();
                rds1.Fill(dt1);
              
                if (dt1.Rows.Count > 0)
                {
                   ret= true;
                }
                else
                {
                    var update =
                         string.Format("update MutexForProjectCreating set  creating = 1 ");
                    sqlcommand1 = new SqlCommand(update, sqlc);
                    sqlcommand1.ExecuteNonQuery();
                }
              
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Log.Info("NeedWait error:" + ex.Message);
            }

            return ret;
        }

        [HttpPost]
        [AllowAnonymous]
        //被邀请人发送请求验证
        public ActionResult SendVerify(string message, long userId, long projectId, string email, string inviteEmail, long partyId)
        {
            //var baseUri = GetHost();
            //var token = await GetToken();
            //if (token == null) return await ReturnToLogin();

            var inviteeId = User.Identity.GetUserId<long>();//(long)Session["UserId"];

            var inviterProfile = GetUserProfile(_userManager, inviteeId);//UserClient.GetProfile(baseUri, token, inviteeId);
            //var json = inviterRes.Content;
            //var inviterProfile = JsonConvert.DeserializeObject<UserDto>(json);
            //var projectRes = await ProjectClient.GetProject(baseUri, token, projectId);
            //var projectJson = projectRes.Content;
            var project = Api.ProjectController.GetProject(projectId, _projService, _mfvaultService, _vaultTemplateService); 

            //string url = baseUri;//获取当前URL
            //url += "/Manage/UserNews";
            //string temp = "<p>请求加入项目</p>"
            //        + "<p>" + inviterProfile.UserName + "(" + inviteEmail + ")请求加入项目" + project.Name + "</p>"
            //        + "<a href='" + Server.HtmlEncode(url) + "'>" + Server.HtmlEncode(url) + "</a>"
            //        + "<p>（如无法打开链接，请复制上面的链接粘贴到浏览器地址栏完成邀请。）</p>"
            //        + "<p>来自：DBWorld工程云【DBWorld】</p>";
            //temp = temp.Replace("+", "%2B");//+号会解析为空所以这里转换下

            var model = new ProjectInvitationConfirmEmailModel
            {
                ProjectId = projectId,
                InviterId = userId,
                ConfirmMessage = message,
                InviteeEmail = inviteEmail,
                InviteePartId = partyId
            };
            var res = Api.ProjectMembersController.InviteeConfirmInvitation(model, User.Identity.GetUserId<long>(),
                _projMemberService);
            if (String.IsNullOrEmpty(res))
            {
                /*
                //发送一封邮件到邀请人邮箱里验证
                var emailMessage = new SendEmailMessage
                {
                    MailTo = email,
                    Title = inviterProfile.UserName + "(" + inviteEmail + ")申请加入项目" + project.Name + ":" + message,
                    Body = temp,
                    IsHtml = true
                };
                //VerifyRecord(userId, inviterProfile.UserName, project.Name, projectId, InviteeId);
                var res2 = await ProjectClient.SendInvitationEmail(baseUri, token, emailMessage);
                if (res2.Success)
                {
                    return Content("success");
                }
                else
                {
                    var err = res2.Content;
                    try
                    {
                        var errModel = JsonConvert.DeserializeObject<BadRequestResponseModel>(err);
                        var errMessage = errModel.GetErrorMessage();
                        if (!String.IsNullOrWhiteSpace(errMessage)) err = errMessage;
                    }
                    catch
                    {
                    }
                    Log.Error("发送邀请邮件失败：" + err);
                    return Content("发送邀请邮件失败：" + err);
                }
                 */
                return Content("success");
            }
            else
            {
                var err = res;
                Log.Error("确认邀请失败：" + err);
                return Content("确认邀请失败：" + err);
            }
        }
        /// <summary>
        /// 一次邀请多人
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectName"></param>
        /// <param name="partyId"></param>
        /// <param name="partyName"></param>
        /// <param name="inviteeAccounts">MfUser列表</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> InviteMembers2Project(int projectId, string projectName, int partyId, string partyName,
                    IList<MfUser> inviteeAccounts)
        {
            var res = await Task.Run(() => InviteMembers2Vault(projectId, projectName, partyId, partyName, inviteeAccounts));
            
            if (!String.IsNullOrEmpty(res))
            {
                return Content(res);
            }
            return Content("success");
        }
        [HttpPost]
        public async Task<ActionResult> ConfirmProjectRoles(int projectId, int userid, IList<string> Roles)
        {
         //   Log.Info("in ConfirmProjectRoles" + userid + "--" + projectId);
            var res = await Task.Run(() => ConfirmProjectRoles2Vault(projectId, userid, Roles));

            if (!String.IsNullOrEmpty(res))
            {
                return Content(res);
            }
            return Content("success");
        }
        private string ConfirmProjectRoles2Vault(int projectId, int userid, IList<string> ugs)
        {
            try
            {
             //   Log.Info("111");
                var mfilesvault = _projService.GetProjectById(projectId).Vault;

             //   Log.Info("222:");
              //  var username = _vaultUserService.GetAccountName(userid, mfilesvault);
                var username = _userService.GetUserById(userid).UserName;

                //   var mfilesuserid=_vaultUserService.GetUserId(username, mfilesvault);
             //   Log.Info("aaa" + projectId);
                string errInfo = string.Empty;
                foreach (string u in ugs)
                {
               //     Log.Info("333"+u);
                    _userGroupService.AddUserToGroup(mfilesvault, username, u);
           //   if(u=="安全员"||u=="项目经理")
                    Log.Info(string.Format("user {0} added into usergroup {1}", username, u));
                }
                return errInfo;
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("user {0} added into usergroup ", ex.Message));
                return ex.Message;
            }
        }
        private string InviteMembers2Vault(int projectId, string projectName, int partyId, string partyName,
                    IEnumerable<MfUser> inviteeAccounts)
        {
            //1.验证有权限邀请
            //2 注册网站用户
            //3.增加MFiles库用户记录，更新表userVault，projectMember
            //4.发送邮件

            var inviterId = User.Identity.GetUserId<int>();
            var inviterName = User.Identity.GetUserName();
            var mfuseridRes = Api.ProjectMembersController.GetMFUserId(projectId, inviterId, _userService,
                _vaultUserService, _projService, _mfvaultService);
            if (mfuseridRes == null)
            {
                return "获取项目中的联系人失败！";
            }
            var userProfile = GetUserProfile(_userManager, inviterId);
            var mfUserId = mfuseridRes.Value;
            string email = userProfile.Email;
            var model = new InviteModel
            {
                Email = email,
                ProjectName = projectName,
                ProjectId = projectId,
                UserId = inviterId,
                PartyId = partyId,
                PartyName = partyName,
                MFUserId = mfUserId
            };
            var baseUrl = Utility.GetHost(Request.Url);
            //验证是否有权限邀请
            var response = Api.ProjectMembersController.InviteMembersOK(model, inviterId, _projService, _vaultTemplateService
                  , _mfvaultService, _mfilesObjService, _userGroupService);
            if (!String.IsNullOrEmpty(response))
            {
                Log.Error(response);
                return response;
                
            }
            Log.Info(inviterName+"有权限邀请成员,"+DateTime.Now.ToLocalTime());

            string errInfo = string.Empty;
            foreach (MfUser u in inviteeAccounts)
            {
                var userName = u.UserName;
                var domain = u.Domain?? "";
                var isDomainUser = !String.IsNullOrEmpty(domain);
                var inviteeEmail = u.Email;
                var user = _userManager.FindByName(userName);
                if (user == null)
                {
                    var userRes = AuthUtility.CreateUser(_userManager, _userService, userName, inviteeEmail, isDomainUser, domain, u.FullName);
                    if (userRes.User == null)
                    {
                        var err = "新增<"+userName+""+userRes.Error + ":" + userRes.ErrorDescription;
                        errInfo += err + "\r\n";
                        Log.Error(err);
                        continue;
                    }
                    user = userRes.User;
                }
                //判断是否是项目成员
                var member = _projectMemberService.GetMember(projectId, user.Id);
                if(member != null) continue;

                //新建库账户及更新数据库
                var mfRes = Api.ProjectMembersController.AcceptInvitationConfirmOneStep(projectId, user.Id, partyId, inviterId,
                    _projMemberService, _projService, _userService, _vaultTemplateService, _mfvaultService
                    , _vaultUserService, _mfilesObjService, _uservaultService);
                if (!string.IsNullOrEmpty(mfRes))
                {
                    Log.Error(mfRes);
                    errInfo += mfRes + "\r\n";
                    continue;
                }
                //发送邮件
                /*
                 工程部-李晓明(1061198635) 2016/11/16 15:45:06
那个正式库的邮箱是还没有设置发件的邮箱是吗
史益军<leoshiyj@qq.com> 2016/11/16 15:46:01
这个功能忽略，邮件省略了
                 */
                //var emailRes = string.Empty;
                //if (!string.IsNullOrEmpty(u.Email))
                //{
                //    emailRes = Api.InviteController.SendEmail2Invitee(baseUrl, partyName, u.Email, projectName,
                //        inviterName, _emailService);
                //}
                //else
                //{
                //    Log.Error("【发送邮件失败】" + u.AccountName + "的邮件地址为空.");
                //}
                //if (!string.IsNullOrEmpty(emailRes))
                //{
                //    emailRes = "【发送邮件失败】<" + u.Email + ">：" + emailRes;
                //    Log.Error(emailRes);
                //    errInfo += emailRes + "\r\n";
                //}
            }
            return errInfo;

        }

        //邀请人参加项目
        public ActionResult InviteMember(string inviteEmail, int projectId, string projectName, int partyId, string partyName)
        {
            //Log.Info("获取用户信息...");
            var userId = User.Identity.GetUserId<int>();
            var userName = User.Identity.GetUserName();
            var mfuseridRes = Api.ProjectMembersController.GetMFUserId(projectId, userId, _userService,
                _vaultUserService, _projService, _mfvaultService);
            if (mfuseridRes == null)
            {
                return Content("获取项目中的联系人失败！");
            }
            var userProfile = GetUserProfile(_userManager, userId);
            var mfUserId = mfuseridRes.Value;
            string email = userProfile.Email;
            var model = new InviteModel
            {
                InviteEmail = inviteEmail,
                Email = email,
                //TokenJson = tokenJson,
                ProjectName = projectName,
                ProjectId = projectId,
                UserId = userId,
                PartyId = partyId,
                PartyName = partyName,
                MFUserId = mfUserId
            };
            //Log.Info("开始提交邀请信息...");
            var baseUrl = Utility.GetHost(Request.Url);
         
            var response = Api.InviteController.Invite(model, _userManager, userId, userName, _userService
                , _projectMemberService, _projService, _vaultTemplateService, _mfvaultService, _mfilesObjService
                , _userGroupService, _emailService, baseUrl);
            if (String.IsNullOrEmpty(response))
            {
                //Log.Info("提交邀请成功...");
                return Content("success");
            }
            else
            {
                Log.Error("提交邀请失败...");
                return Content(response);
            }
        }

        //邀请人接受被邀请人验证信息
        //public ViewResult AcceptAsk()
        //{
        //    //var ead = new EncipherAndDecrypt();
        //    int projectId = int.Parse(Request.QueryString["projectId"]);
        //    int InviteeId = int.Parse(Request.QueryString["InviteeId"]);
        //    ViewBag.projectId = projectId;
        //    ViewBag.InviteeId = InviteeId;
        //    return View();
        //}

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AccpetAskAction(int projectId, int inviteeId, int partyId)
        {
            var model = new ProjectInvitationConfirmAcceptModel
            {
                ProjectId = projectId,
                InviteeId = inviteeId,
                InviteePartId = partyId
            };
            var res = Api.ProjectMembersController.AcceptInvitationConfirm(model, User.Identity.GetUserId<long>(),
                _projMemberService, _projService, _userService, _vaultTemplateService, _mfvaultService, 
                _vaultUserService, _mfilesObjService, _uservaultService);
            if (String.IsNullOrEmpty(res))
            {
                return Content("success");
            }
            else
            {
                return Content(res);
            }
        }

        /// <summary>
        /// 根据邀请人ID已经 被邀请人ID+项目ID组合，判断出这条消息的唯一性
        /// </summary>
        /// <param name="inviterId">邀请人ID</param>
        /// <param name="projectId">项目ID</param>
        /// <param name="inviteeId">被邀请人ID</param>
        /// <param name="agreeOrNot">是否同意，0代表不同意，1代表同意</param>
        public void UpdateVerifyRecord(int inviterId, int projectId, int inviteeId, int agreeOrNot)
        {
            var tempId = inviteeId + "_" + projectId;
            var xmldoc = new XmlDocument();
            xmldoc.Load(Server.MapPath("~/Installer/VerifyRecord.xml"));
            XmlNodeList nodeList = xmldoc.SelectSingleNode("List").ChildNodes;
            foreach (XmlNode xnode in nodeList) //遍历所节点 
            {
                if (xnode.Attributes["inviterId"].InnerText == inviterId.ToString() && tempId == xnode.Attributes["id"].InnerText)
                {
                    xnode.Attributes["statu"].InnerText = agreeOrNot.ToString();
                }
            }
            xmldoc.Save(Server.MapPath("~/Installer/VerifyRecord.xml"));
        }

        //获取项目成员
        public ActionResult GetProjectMembers(int projectId)
        {
            var res = Api.ProjectMembersController.GetProjectUsersDisplay(projectId, _projMemberService, _userService);
            var json = JsonConvert.SerializeObject(res,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(json);
        }

        protected internal string ChangeStatus(int projectId, int statusId)
        {
            return Api.ProjectController.ChangeProjectStatus(projectId, statusId, User.Identity.GetUserName(), User.Identity.GetUserId<long>()
                , _projService, _mfvaultService, _userGroupService, _vaultTemplateService);
        }
        //改变项目状态
        public  ActionResult ChangeProjectStatu(int projectId, string oprType)
        {
            string returnStr = "fail";

            switch (oprType)
            {
                case "start":
                    var res = ChangeStatus(projectId, ProjectStatusConstants.StartProjectId);//await ProjectClient.StartProject(baseUri, token, projectId);
                    if (String.IsNullOrEmpty(res))
                    {
                        returnStr = "success";
                    }
                    else
                    {
                        returnStr = res;
                    }
                    break;
                case "pause":
                    res = ChangeStatus(projectId, ProjectStatusConstants.PauseProjectId);//await ProjectClient.StartProject(baseUri, token, projectId);
                    if (String.IsNullOrEmpty(res))
                    {
                        returnStr = "success";
                    }
                    else
                    {
                        returnStr = res;
                    }
                    break;
                case "end":
                    res = ChangeStatus(projectId, ProjectStatusConstants.EndProjectId);//await ProjectClient.StartProject(baseUri, token, projectId);
                    if (String.IsNullOrEmpty(res))
                    {
                        returnStr = "success";
                    }
                    else
                    {
                        returnStr = res;
                    }
                    break;
                case "proposal":
                    res = ChangeStatus(projectId, ProjectStatusConstants.CreateProjectId);//await ProjectClient.StartProject(baseUri, token, projectId);
                    if (String.IsNullOrEmpty(res))
                    {
                        returnStr = "success";
                    }
                    else
                    {
                        returnStr = res;
                    }
                    break;
                default:
                    break;
            }
            return Content(returnStr);
        }

        //获取项目成员详情
        //public ActionResult GetUserInfo(int userId)
        //{
        //    //var token = GetToken();
        //    return Content("");
        //}

        //获取参与方
        public ActionResult GetAllParties()
        {
            var res = Api.ProjectController.GetParties(_projService); 
            var resStr = JsonConvert.SerializeObject(res,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(resStr);
        }

        //获取用户头像
        public ActionResult GetUserImage()
        {
            int userId = int.Parse(Request.QueryString["userId"]);

            var users = GetUserProfile(_userManager, userId); 

            if (users.Image != null)
            {
                return File(users.Image, "image/jpeg");
            }
            else
            {

                return File(Server.MapPath("~/Content/Images/users/defaultimage.png"), "image/png");
            }
        }
        //[AllowAnonymous]
        //获取被邀请人列表
        public async Task<JsonResult> GetMfAccountList(int projectId)
        {
            var res = await Task.Run(() =>
                     {
                        var server = _vaultServerService.GetServer();
                        var mfUsers = _vaultUserService.GetMFilesLoginAccounts(server);
                        var members = Api.ProjectMembersController.GetProjectUsers(projectId, _projMemberService, _userService).ToList();
                        return(from u in mfUsers 
                                    let m = members.FirstOrDefault(c => c.UserName == u.UserName 
                                                && (c.Domain == u.Domain 
                                                ||(string.IsNullOrEmpty(c.Domain) && string.IsNullOrEmpty(u.Domain)))) 
                                    where m == null && u.UserName != "admin"
                                    select u).ToList();
                     });
            
            return Json(res);
        }
    }
}