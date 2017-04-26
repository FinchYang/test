using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using AecCloud.BaseCore;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MFilesCore.Metadata;
using AecCloud.MfilesServices;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using AecCloud.WebAPI.Models.DataAnnotations;
using DBWorld.AecCloud.Web.ApiRequests;
using DBWorld.AecCloud.Web.Models;
using log4net;
using MFilesAPI;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Lookup = MFilesAPI.Lookup;
using Task = System.Threading.Tasks.Task;

namespace DBWorld.AecCloud.Web.Api
{
    //[Authorize]
    public class ProjectController : ProjectBaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly IProjectService _projectService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IUserService _userService;
        private readonly IVaultServerService _vaultServerService;

        private readonly ICloudService _cloudService;
        private readonly IMFVaultService _vaultService;
        private readonly IMFUserService _vaultUserService;
        private readonly IUserVaultService _uservaultService;
    //    private readonly IRepository<Company> _companyRepo;
        //private readonly IEmailService _emailService;
     //   private readonly IMfUserGroupService _mfusergroupService;
        //private readonly IMFilesVaultService _mfvaultService;
        //private readonly IUserCloudService _userCloudService;
        private readonly IMfProjectService _mfProjService;

        public ProjectController(IProjectService projectService, IProjectMemberService projectMemberService, IUserService userService
            , IMFObjectService mfilesObjService, IVaultTemplateService vaultTemplateService, IVaultServerService vaultServerService
            , ICloudService cloudService, IMFUserService vaultUserService, IMFVaultService vaultService
            , IUserVaultService uservaultService, IMfUserGroupService mfusergroupService, IMFilesVaultService mfvaultService
            , IAuthenticationManager authenticationManager, IMfProjectService mfProjService, IRepository<Company> companyRepo)
            : base(projectService, mfvaultService, mfusergroupService, vaultTemplateService, mfilesObjService, authenticationManager) //, IUserCloudService userCloudService
        {
            //_projectService = projectService;
            _projectMemberService = projectMemberService;
            _userService = userService;
            _vaultService = vaultService;
            _vaultUserService = vaultUserService;
            _vaultServerService = vaultServerService;
            _cloudService = cloudService;
            _uservaultService = uservaultService;
        //    _mfusergroupService = mfusergroupService;
            //_emailService = emailService;
            //_mfvaultService = mfvaultService;
            //Authentication = authenticationManager;
            _mfProjService = mfProjService;
            //_userCloudService = userCloudService;
         //   _companyRepo = companyRepo;
        }

        //private IAuthenticationManager Authentication { get; set; }
        /// <summary>
        /// 获取当前用户所参与的项目列表:
        /// 状态码：正常=>OK；异常=>其他
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Projects()
        {
            try
            {
                var userId = GetUserId();

                IEnumerable<ProjectDto> projs = await GetProjectByUser(userId);
                return Ok(projs);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取用户的项目列表失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Areas()
        {
            try
            {
                IEnumerable<AreaDto> projs = await Task.Run(() => _projectService.GetAllArea().Select(c => c.ToDto()));
                return Ok(projs);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取地区列表失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Companies()
        {
            try
            {
                IEnumerable<CompanyDto> projs = await Task.Run(() => _projectService.GetAllCompany().Select(c => c.ToDto()));
                return Ok(projs);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取公司列表失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Levels()
        {
            try
            {
                IEnumerable<ProjectLevelDto> projs = await Task.Run(() => _projectService.GetLevels().Select(c => c.ToDto()));
                return Ok(projs);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取项目级别列表失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        private Task<List<ProjectDto>> GetProjectByUser(long userId)
        {
            return Task.Run(() => GetProjectByUserId(userId));
            //return Task.FromResult(GetProjectByUserId(userId));//debug
        }

        private List<ProjectDto> GetProjectByUserId(long userId)
        {
            // 1. 通过用户ID获取到用户与项目的关联表信息；
            var members = _projectMemberService.GetProjectsByUser(userId);
            //    var members = _projectMemberService.GetProjects();
            // 2. 通过关联表信息中的项目ID获取项目信息
            var list = new List<ProjectDto>();
            foreach (var m in members)
            {
                try
                {
                    var proj = _projectService.GetProjectById(m.ProjectId);
                    if (proj == null)
                    {
                        Log.WarnFormat("项目({0})已被删除", m.ProjectId);
                        continue;
                    }
                    var vault = GetVault(proj);
                    if (vault == null)
                    {
                        Log.WarnFormat("文档库({0})已被删除", proj.VaultId);
                        continue;
                    }
                    var projDto = proj.ToDto(vault, false);
                    list.Add(projDto);
                }
                catch (Exception ex)
                {
                    Log.Info("GetProjectsByUser by client error，最大可能是测试项目数据清除造成，可忽略:" + ex.Message);
                }
            }
            return list;
        }
        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projMemberService"></param>
        /// <param name="projService"></param>
        /// <param name="mfvaultService"></param>
        /// <param name="vaultTemplateService"></param>
        /// <returns></returns>
        public static List<ProjectDto> GetProjectsByUser(long userId, IProjectMemberService projMemberService,
            IProjectService projService, IMFilesVaultService mfvaultService, IVaultTemplateService vaultTemplateService)
        {
            //  Log.Info("GetProjectsByUser 11");
            // 1. 通过用户ID获取到用户与项目的关联表信息；
            var members = projMemberService.GetProjectsByUser(userId);
            // 2. 通过关联表信息中的项目ID获取项目信息
            //   Log.Info("GetProjectsByUser 22");
            var list = new List<ProjectDto>();
            //   Log.Info("GetProjectsByUser 33");
            foreach (var m in members)
            {
                try
                {
                    //  Log.Info("GetProjectsByUser 44");
                    var proj = projService.GetProjectById(m.ProjectId);
                    //   Log.Info("GetProjectsByUser 55");
                    var vault = GetVault(mfvaultService, proj);
                    //   Log.Info("GetProjectsByUser 66");
                    //  var template = GetTemplateByTempId(vaultTemplateService, proj.TemplateId);
                    //   Log.Info("GetProjectsByUser 77");
                    //   var tempDto = template.ToDto();
                    //   Log.Info("GetProjectsByUser 88");
                    var projDto = proj.ToDto(vault, false);
                    //    Log.Info("GetProjectsByUser 99");
                    list.Add(projDto);
                    //   Log.Info("GetProjectsByUser aa");
                }
                catch (Exception ex)
                {
                    Log.Info("GetProjectsByUser error:" + ex.Message);
                }
            }
            //   Log.Info("GetProjectsByUser bb");
            return list;
        }

        /// <summary>
        /// 获取参与方的列表
        /// 状态码：正常=>OK；异常=>其他
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> AllParties()
        {
            var ps = await Task.Run(() => GetParties(_projectService));
            return Ok(ps);
        }

        public static List<ProjectPartyDto> GetParties(IProjectService projService)
        {
            return projService.GetAllParties().Select(c => c.ToDto()).ToList();
        }
        /// <summary>
        /// 根据ID获取参与方对象
        /// </summary>
        /// <param name="id">参与方ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Parties(int id)
        {
            var party = await Task.Run(() => GetParty(_projectService, id));
            return Ok(party);
        }

        public static ProjectPartyDto GetParty(IProjectService projService, int partyId)
        {
            return projService.GetPartyById(partyId).ToDto();
        }

        public static ProjectPartyDto GetPartyByName(IProjectService projService, string name)
        {
            var party = projService.GetPartyByName(name);
            if (party == null) return null;
            return party.ToDto();
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> GetPartyByName(string name)
        {
            var party = await Task.Run(() => GetPartyByName(_projectService, name));
            if (party == null) return CreateResponse(HttpStatusCode.NotFound, "未找到参与方");
            return Ok(party);
        }

        /// <summary>
        /// 通过ID获取项目，在所有项目中搜寻
        /// 状态码：正常=>OK；异常=>其他
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns></returns>
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> AllProjects(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest("未登录");
            }
            try
            {
                ProjectDto proj0 = await Task.Run(() =>
                {
                    var proj = _projectService.GetProjectById(id);
                    Log.Info(string.Format("all projects:{0},{1}", id, proj.TemplateId));
                    var vault = GetVault(proj);
                    //  var template = GetTemplateByTempId(proj.TemplateId);
                    //    var tempDto = template.ToDto();
                    return proj.ToDto(vault, false);
                }
                    );
                return Ok(proj0);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取指定项目失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static ProjectDto GetProject(long prjId, IProjectService projService,
            IMFilesVaultService mfvaultService, IVaultTemplateService vaultTemplateService)
        {
            //  Log.Info("  public static ProjectDto GetProject( 111");
            var proj = projService.GetProjectById(prjId);
            //   Log.Info("  public static ProjectDto GetProject( 222");
            var vault = GetVault(mfvaultService, proj);
            //   Log.Info("  public static ProjectDto GetProject( 333");
            //   var template = GetTemplateByTempId(vaultTemplateService, proj.TemplateId);
            //   Log.Info("  public static ProjectDto GetProject( 444");
            //    var tempDto = template.ToDto();
            //   Log.Info("  public static ProjectDto GetProject( 555");
            var projDto = proj.ToDto(vault, false);
            var app = MFServerUtility.ConnectToMfApp(vault);

            var thevault = app.LogInToVault(vault.Guid);
            var sc = new SearchCondition
            {
                ConditionType = MFConditionType.MFConditionTypeEqual,
                Expression = { DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID }
            };
            sc.TypedValue.SetValueToLookup(new Lookup
            {
                Item =
                    thevault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemObjectType, MfilesAliasConfig.ObjProject)
            });
            var sr = thevault.ObjectSearchOperations.SearchForObjectsByCondition(sc, false).ObjectVersions;
            foreach (ObjectVersion objectVersion in sr)
            {
                var pvs = thevault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);


                projDto.ContractAmount =
                    pvs.SearchForPropertyByAlias(thevault, MfilesAliasConfig.PropContactValue, true)
                        .GetValueAsLocalizedText();
                projDto.ConstructionScale =
                    pvs.SearchForPropertyByAlias(thevault, MfilesAliasConfig.PropConstructionScale, true)
                        .GetValueAsLocalizedText();
                projDto.ProjectClass =
                    pvs.SearchForPropertyByAlias(thevault, MfilesAliasConfig.PropProjClass, true)
                        .GetValueAsLocalizedText();


                break;
            }
            thevault.LogOutSilent();
            app.Disconnect();
            return projDto;
        }
        public static ProjectDto GetProjectNoLimit(long prjId, IProjectService projService,
         IMFilesVaultService mfvaultService, IVaultTemplateService vaultTemplateService)
        {
            var proj = projService.GetProjectById(prjId);
            var vault = GetVault(mfvaultService, proj);
            var projDto = proj.ToDto(vault, false);
            return projDto;
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetImage(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            try
            {
                var proj0 = await Task.Run(() =>
                {
                    var proj = _projectService.GetProjectById(id);
                    return Convert.ToBase64String(proj.Cover);
                }
                    );
                return Ok(proj0);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取指定项目的封面失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        /// <summary>
        /// 获取协同云模板列表
        /// 状态码：正常=>OK；异常=>其他
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> ProjectTemplates()
        {
            try
            {
                var cates = await GetProjectTemplates();
                return Ok(cates.AsEnumerable());
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取项目模板失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        //todo
        private async Task<List<VaultTemplateDto>> GetProjectTemplates()
        {
            // var templates = await GetTemplates(4)
            var templates = await GetTemplatesNew(CloudConstants.MyProjects);
            return templates.Select(c => c.ToDto()).ToList();
        }

        private static void ToLocalTimeProj(Project proj)
        {
            proj.StartDateUtc = proj.StartDateUtc.ToLocalTime();
            proj.EndDateUtc = proj.EndDateUtc.ToLocalTime();
        }

        private static void ToUtcTimeProj(Project proj)
        {
            proj.StartDateUtc = proj.StartDateUtc.ToUniversalTime();
            proj.EndDateUtc = proj.EndDateUtc.ToUniversalTime();
        }
        /// <summary>
        /// 单独创建项目对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> CreateProjObj(ProjectCreateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();//Authentication.User.Identity.GetUserId<int>();
            var creator = await Task.Run(() => _userService.GetUserById(userId));
            var projs = await Task.Run(() => _projectService.GetProjectsByOwner(userId));//_projectMemberService.GetProjectsByUser(userId).Where(c => c.IsCreator);
            if (creator.MaxProjectCount > 0 && projs.Count >= creator.MaxProjectCount)
            {
                return BadRequest(String.Format("您的项目配额({0})已用完！", creator.MaxProjectCount));
            }

            var sb = new StringBuilder();
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            sb.Append(invalidChars);
            sb.Append(System.IO.Path.GetInvalidFileNameChars());
            if (sb.ToString().Any(c => model.Name.Contains(c)))
            {
                ModelState.AddModelError("项目名称", "项目名称包含非法字符");
                return BadRequest(ModelState);
            }
            if (model.StartDateUtc > model.EndDateUtc)
            {
                return BadRequest(String.Format("起始日期({0})必须小于终止日期({1})", model.StartDateUtc.ToLocalTime(), model.EndDateUtc.ToLocalTime()));
            }


            var project = new Project
            {
                Name = model.Name,
                Number = model.Number,
                Cover = model.Cover,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                TemplateId = model.TemplateId,
                Description = model.Description,
                CloudId = CloudConstants.MyProjects,
                OwnerId = userId
            };

            try
            {
                var template = await Task.Run(() => GetTemplateByTempId(project.TemplateId));//获得库模版对象
                if (template == null)
                {
                    return BadRequest(String.Format("找不到库模板({0})！", project.TemplateId));
                }

                var partyId = model.ProjectPartyId;
                var templateDto = template.ToDto();
                if (templateDto.HasParty)
                {
                    if (partyId <= 0)
                    {
                        return BadRequest("必须提供参与方信息");
                    }
                    project.PartyId = partyId;
                }

                //1. 设置项目基本信息
                project.OwnerId = userId; //拥有者
                project.StatusId = 1; //设置项目状态为立项

                if (project.CloudId <= 0) //设置属于协同云
                {
                    var cloud = _cloudService.GetCloud();
                    if (cloud != null) project.CloudId = cloud.Id;
                }

                _projectService.CreateProject(project);//数据库操作
                return Ok(project.ToDto(templateDto.HasParty));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("  public async Task<IHttpActionResult> CreateProjObj(ProjectCreateModel model) error:{0}", ex.Message));
                return CreateErrorResponse("创建项目失败3：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static ProjectResult CreateProjectForAllBackup(ProjectCreateModel model, long userId, IUserService _userService, IProjectService _projectService
            , IVaultServerService _vaultServerService, ICloudService _cloudService, IMFVaultService _vaultService, IVaultTemplateService vaultTemplateService
            , IMFUserService _vaultUserService, IMFObjectService _mfilesObjService, IMFilesVaultService _mfvaultService,
            IProjectMemberService _projectMemberService, IUserVaultService _uservaultService, IMfUserGroupService _mfusergroupService, string password, string[] modelUnits)
        {
            var creator = _userService.GetUserById(userId);
            var projs = _projectService.GetProjectsByOwner(userId);//_projectMemberService.GetProjectsByUser(userId).Where(c => c.IsCreator);
            if (projs.Count >= creator.MaxProjectCount && creator.MaxProjectCount > 0)
            {
                return new ProjectResult { Error = String.Format("您的项目配额({0})已用完！", creator.MaxProjectCount) };
            }            
            var sb = new StringBuilder();
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            sb.Append(invalidChars);            
            sb.Append(System.IO.Path.GetInvalidFileNameChars());
            if (sb.ToString().Any(c => model.Name.Contains(c)))
            {
                return new ProjectResult { Error = "项目名称包含非法字符" };
            }
            if (model.StartDateUtc > model.EndDateUtc)
            {
                return new ProjectResult { Error = String.Format("起始日期({0})必须小于终止日期({1})", model.StartDateUtc.ToLocalTime(), model.EndDateUtc.ToLocalTime()) };
            }
            var project = new Project
            {
                Name = model.Name,
                Number = model.Number,
                Cover = model.Cover,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                TemplateId = model.TemplateId,
                Description = model.Description,
                CloudId = CloudConstants.MyProjects,
                OwnerId = userId,
                CompanyId = model.CompanyId,
                LevelId = model.ProjectLevelId,                
                OwnerUnit = model.OwnerUnit,
                ConstructionScale = model.ConstructionScale,
                AreaId = model.AreaId,
                SupervisionUnit = model.PropSupervisorUnit,
                PmUnit = model.PmUnit,
                InvestigateUnit = model.InvestigateUnit,
                DesignUnit = model.PropDesignUnit,
                ContractAmount = model.ContractAmount,
                // Area=model.Area.ToString(),
               // ProjClass = model.ProjectClass
                ClassId = model.ProjectClassId,

            };

            CreateRes cr = null;
            MFilesVault vault;
            try
            {
                //2. 获得MFiles服务器对象，并设置库的属性
                var template = GetTemplateByTempId(vaultTemplateService, 4);//获得库模版对象
                if (template == null)
                {
                    return new ProjectResult { Error = String.Format("找不到库模板({0})！", project.TemplateId) };
                }
                var partyId = 1;
                ProjectParty party = _projectService.GetPartyById(partyId);
                project.PartyId = partyId;

                //1. 设置项目基本信息
                project.OwnerId = userId; //拥有者
                project.StatusId = 1; //设置项目状态为立项

                var server = _vaultServerService.GetServer();
                if (server == null)
                {
                    return new ProjectResult { Error = "默认服务器不存在!" };
                }
                if (project.CloudId <= 0) //设置属于协同云
                {
                    var cloud = _cloudService.GetCloud();
                    if (cloud != null) project.CloudId = cloud.Id;
                }
           //     var userName = creator.UserName;
                vault = new MFilesVault
                {
                    Server = server,
                    Description = project.Description,
                    //  Name = project.Name + "-" + userName.Replace("\\","-"),
                    Name = project.Name + "-" + userId,
                    TemplateId = template.Id,
                    TemplateVersion = template.Version,
                    CreatedTimeUtc = DateTime.UtcNow,
                    CloudId = project.CloudId,
                    ServerPath = _vaultServerService.GetVaultMainDataPath()//StorageUtility.GetVaultDataPath()
                };
                var sqlDb =
                    StorageUtility.GetMfSqlDb("Proj_" + userId + "_" + vault.CreatedTimeUtc.ToString("yyyyMMddHHmm"));
                vault.SqlConnectionString = sqlDb.ToConnectionString();
                var has = _vaultService.HasVault(vault);
                if (has)
                {
                    return new ProjectResult { Error = "已存在同名的项目库(Vault)！" };
                }
                cr = CreateProjectForAllBackupInternal(project, creator, template, vault, sqlDb, party, _vaultService, _vaultUserService, _mfilesObjService, _mfvaultService
                    , _projectService, _projectMemberService, _uservaultService, _userService, _mfusergroupService, password, modelUnits);
                if (!String.IsNullOrEmpty(cr.Err))
                {
                    return new ProjectResult { Error = cr.Err };
                }
                return new ProjectResult { Project = project.ToDto(vault, false) };
            }
            catch (Exception ex)
            {
                var err = "创建项目失败4：" + ex.Message;
                Log.Error(err);
                return new ProjectResult { Error = "创建项目失败5：" + ex.Message, Exception = ex };
            }
        }

        public static ProjectResult CreateProjectALL(ProjectCreateModel model, long userId, IUserService _userService, IProjectService _projectService
            , IVaultServerService _vaultServerService, ICloudService _cloudService, IMFVaultService _vaultService, IVaultTemplateService vaultTemplateService
            , IMFUserService _vaultUserService, IMFObjectService _mfilesObjService, IMFilesVaultService _mfvaultService, IProjectMemberService _projectMemberService, IUserVaultService _uservaultService)
        {
            var creator = _userService.GetUserById(userId);
            var projs = _projectService.GetProjectsByOwner(userId);//_projectMemberService.GetProjectsByUser(userId).Where(c => c.IsCreator);
            if (projs.Count >= creator.MaxProjectCount && creator.MaxProjectCount > 0)
            {
                return new ProjectResult { Error = String.Format("您的项目配额({0})已用完！", creator.MaxProjectCount) };
            }

            var sb = new StringBuilder();
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            sb.Append(invalidChars);
            sb.Append(System.IO.Path.GetInvalidFileNameChars());
            if (sb.ToString().Any(c => model.Name.Contains(c)))
            {
                return new ProjectResult { Error = "项目名称包含非法字符" };
            }
            if (model.StartDateUtc > model.EndDateUtc)
            {
                return new ProjectResult { Error = String.Format("起始日期({0})必须小于终止日期({1})", model.StartDateUtc.ToLocalTime(), model.EndDateUtc.ToLocalTime()) };
            }
            var project = new Project
            {
                Name = model.Name,
                Number = model.Number,
                Cover = model.Cover,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                TemplateId = model.TemplateId,
                Description = model.Description,
                CloudId = CloudConstants.MyProjects,
                OwnerId = userId,
                CompanyId = model.CompanyId,
                AreaId = model.AreaId
            };
            VaultTemplateDto templateDto = null;
            CreateRes cr = null;
            MFilesVault vault;
            try
            {
                //2. 获得MFiles服务器对象，并设置库的属性
                var template = GetTemplateByTempId(vaultTemplateService, project.TemplateId);//获得库模版对象
                if (template == null)
                {
                    return new ProjectResult { Error = String.Format("找不到库模板({0})！", project.TemplateId) };
                }
                var partyId = model.ProjectPartyId;
                ProjectParty party = null;
                templateDto = template.ToDto();
                if (templateDto.HasParty)
                {
                    if (partyId <= 0)
                    {
                        return new ProjectResult { Error = "必须提供参与方信息" };
                    }
                    party = _projectService.GetPartyById(partyId);
                    project.PartyId = partyId;
                }
                //1. 设置项目基本信息
                project.OwnerId = userId; //拥有者
                project.StatusId = 1; //设置项目状态为立项

                var server = _vaultServerService.GetServer();
                if (server == null)
                {
                    return new ProjectResult { Error = "默认服务器不存在!" };
                }
                if (project.CloudId <= 0) //设置属于协同云
                {
                    var cloud = _cloudService.GetCloud();
                    if (cloud != null) project.CloudId = cloud.Id;
                }
           //     var userName = creator.UserName;
                vault = new MFilesVault
                {
                    Server = server,
                    Description = project.Description,
                    Name = project.Name + "-" + userId,
                    TemplateId = template.Id,
                    TemplateVersion = template.Version,
                    CreatedTimeUtc = DateTime.UtcNow,
                    CloudId = project.CloudId,
                    ServerPath = _vaultServerService.GetVaultMainDataPath()//StorageUtility.GetVaultDataPath()
                };
                var sqlDb =
                    StorageUtility.GetMfSqlDb("Proj_" + userId + "_" + vault.CreatedTimeUtc.ToString("yyyyMMddHHmm"));
                vault.SqlConnectionString = sqlDb.ToConnectionString();
                var has = _vaultService.HasVault(vault);
                if (has)
                {
                    return new ProjectResult { Error = "已存在同名的项目库(Vault)！" };
                }
                cr = CreateProject(project, creator, template, vault, sqlDb, party, _vaultService, _vaultUserService, _mfilesObjService, _mfvaultService
                    , _projectService, _projectMemberService, _uservaultService);
                if (!String.IsNullOrEmpty(cr.Err))
                {
                    return new ProjectResult { Error = cr.Err };
                }
                return new ProjectResult { Project = project.ToDto(vault, templateDto.HasParty) };
            }
            catch (Exception ex)
            {
                var err = "创建项目失败4：" + ex.Message;
                Log.Error(err);
                return new ProjectResult { Error = "创建项目失败5：" + ex.Message, Exception = ex };
            }
        }
        /// <summary>
        /// 创建项目
        /// 状态码：正常 => Created; 异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Create(ProjectCreateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();//Authentication.User.Identity.GetUserId<int>();
            var creator = await Task.Run(() => _userService.GetUserById(userId));
            var projs = await Task.Run(() => _projectService.GetProjectsByOwner(userId));//_projectMemberService.GetProjectsByUser(userId).Where(c => c.IsCreator);
            if (projs.Count >= creator.MaxProjectCount && creator.MaxProjectCount > 0)
            {
                return BadRequest(String.Format("您的项目配额({0})已用完！", creator.MaxProjectCount));
            }

            var sb = new StringBuilder();
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            sb.Append(invalidChars);
            sb.Append(System.IO.Path.GetInvalidFileNameChars());
            if (sb.ToString().Any(c => model.Name.Contains(c)))
            {
                ModelState.AddModelError("项目名称", "项目名称包含非法字符");
                return BadRequest(ModelState);
            }
            if (model.StartDateUtc > model.EndDateUtc)
            {
                return BadRequest(String.Format("起始日期({0})必须小于终止日期({1})", model.StartDateUtc.ToLocalTime(), model.EndDateUtc.ToLocalTime()));
            }
            var password = DBWorldCache.Get(GetUserId().ToString());
            var projRes = CreateProjectForAllBackup(model, userId, _userService, _projectService, _vaultServerService,
                _cloudService, _vaultService,
                _vaultTemplateService, _vaultUserService, _mfilesObjService, _mfvaultService, _projectMemberService,
                _uservaultService, _mfusergroupService, password, null);

            if (projRes.Project == null)
            {
                var errMsg = "创建项目失败：" + projRes.Error;
                if (projRes.Exception != null)
                {
                    errMsg += " " + projRes.Exception.Message;
                    Log.Error(errMsg, projRes.Exception);
                }
                else
                {
                    Log.Error(errMsg);
                }

                return BadRequest(errMsg);
            }

            var project = projRes.Project;
            var requestURL = GetHost();
            //var vault = cr.Vault; // GetVault(project);
            return Created(requestURL + "/api/Project/AllProjects/" + project.Id, project);

            //var project = new Project
            //{
            //    Name = model.Name,
            //    Number = model.Number,
            //    Cover = model.Cover,
            //    StartDateUtc = model.StartDateUtc,
            //    EndDateUtc = model.EndDateUtc,
            //    TemplateId = model.TemplateId,
            //    Description = model.Description,
            //    CloudId = CloudConstants.MyProjects,
            //    OwnerId = userId,
            //    CompanyId = model.CompanyId,
            //    AreaId = model.AreaId
            //};
            //VaultTemplateDto templateDto = null;
            //CreateRes cr = null;
            //MFilesVault vault;
            //try
            //{
            //    var template = await Task.Run(() => GetTemplateByTempId(project.TemplateId));//获得库模版对象
            //    if (template == null)
            //    {
            //        return BadRequest(String.Format("找不到库模板({0})！", project.TemplateId));
            //    }

            //    var partyId = model.ProjectPartyId;
            //    ProjectParty party = null;
            //    templateDto = template.ToDto();
            //    if (templateDto.HasParty)
            //    {
            //        if (partyId <= 0)
            //        {
            //            return BadRequest("必须提供参与方信息");
            //        }
            //        party = _projectService.GetPartyById(partyId);
            //        project.PartyId = partyId;
            //    }

            //    //1. 设置项目基本信息
            //    project.OwnerId = userId; //拥有者
            //    project.StatusId = 1; //设置项目状态为立项

            //    ////2. 获得MFiles服务器对象，并设置库的属性

            //    var server = _vaultServerService.GetServer();
            //    if (server == null)
            //    {
            //        return BadRequest("默认服务器不存在!");
            //    }

            //    if (project.CloudId <= 0) //设置属于协同云
            //    {
            //        var cloud = _cloudService.GetCloud();
            //        if (cloud != null) project.CloudId = cloud.Id;
            //    }
            //    var userName = creator.UserName;
            //    vault = new MFilesVault
            //    {
            //        Server = server,
            //        Description = project.Description,
            //        Name = project.Name + "-" + userId,
            //        TemplateId = template.Id,
            //        TemplateVersion = template.Version,
            //        CreatedTimeUtc = DateTime.UtcNow,
            //        CloudId = project.CloudId,
            //        ServerPath = _vaultServerService.GetVaultMainDataPath()//StorageUtility.GetVaultDataPath()
            //    };
            //    var sqlDb =
            //        StorageUtility.GetMfSqlDb("Proj_" + userId + "_" + vault.CreatedTimeUtc.ToString("yyyyMMddHHmm"));
            //    vault.SqlConnectionString = sqlDb.ToConnectionString();

            //    var has = await Task.Run(() => _vaultService.HasVault(vault));
            //    if (has)
            //    {
            //        return BadRequest("已存在同名的项目库(Vault)！");
            //    }

            //    //var userName = creator.UserName; //GetUserName();//Authentication.User.Identity.GetUserName();
            //    Log.Info("11 Create");
            //    cr = CreateProjectForAllBackupInternal(project, creator, template, vault, sqlDb, party, _vaultService, _vaultUserService, _mfilesObjService
            //        , _mfvaultService, _projectService, _projectMemberService, _uservaultService);
            //    Log.Info("22 Create");
            //    if (!String.IsNullOrEmpty(cr.Err))
            //    {
            //        return BadRequest(cr.Err);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Info(string.Format(" public async Task<IHttpActionResult> Create(ProjectCreateModel model) error:{0}", ex.Message));
            //    Log.Error("创建项目失败1：" + ex.Message, ex);
            //    return CreateErrorResponse("创建项目失败2：", HttpStatusCode.ServiceUnavailable, ex, Log);
            //}
            //var requestURL = GetHost();
            ////var vault = cr.Vault; // GetVault(project);
            //return Created(requestURL + "/api/Project/AllProjects/" + project.Id, project.ToDto(vault, templateDto.HasParty));
            //return CreatedAtRoute("DefaultApi", new { project.Id }, project.ToDto());
        }

        class CreateRes
        {
            public string Err { get; set; }

            public MFilesVault Vault { get; set; }
        }
        /// <summary>
        /// 根据项目对象创建文档库
        /// </summary>
        /// <param name="projId">项目ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> CreateVault(long projId)
        {
            var project = await Task.FromResult(_projectService.GetProjectById(projId));
            var template = await Task.Run(() => GetTemplateByTempId(project.TemplateId));//获得库模版对象
            if (template == null)
            {
                return BadRequest(String.Format("找不到库模板({0})！", project.TemplateId));
            }
            var server = _vaultServerService.GetServer();
            if (server == null)
            {
                return BadRequest("默认服务器不存在!");
            }
            var userName = GetUserName();
            var userId = GetUserId();
            var creator = _userService.GetUserById(userId);

            if (string.IsNullOrEmpty(creator.Domain) && creator.DomainUser)
            {
                creator.Domain = DomainClient.Domain;
            }

            var vault = new MFilesVault
            {
                Server = server,
                Description = project.Description,
                //     Name = project.Name + "-" + userName,
                // Name = project.Name + "-" + userName.Replace('\\', '-'),
                // Name = project.Name + "-" + userName.Substring(userName.IndexOf('\\') + 1),
                Name = project.Name + "-" + userId,
                TemplateId = template.Id,
                TemplateVersion = template.Version,
                CreatedTimeUtc = DateTime.UtcNow,
                CloudId = project.CloudId,
                ServerPath = StorageUtility.GetVaultDataPath()
            };
            var sqlDb =
                StorageUtility.GetMfSqlDb("Proj_" + userId + "_" + vault.CreatedTimeUtc.ToString("yyyyMMddHHmm"));
            vault.SqlConnectionString = sqlDb.ToConnectionString();

            ProjectParty party = null;
            if (project.PartyId > 0)
            {
                party = _projectService.GetPartyById(project.PartyId);
            }

            try
            {
                var cr0 = await Task.Run(() =>
                {
                    var mfUsername = System.Configuration.ConfigurationManager.AppSettings["mfusername"];
                    var mfPassword = System.Configuration.ConfigurationManager.AppSettings["mfpassword"];
                    var res = _mfProjService.Create(project, creator, template, vault, sqlDb, mfUsername, mfPassword, party);

                    //6. 更新数据库
                    _mfvaultService.InsertVault(vault);
                    project.VaultId = vault.Id;
                    _projectService.UpdateProject(project); //更新数据库中的项目对象

                    if (vault.Id == 0)
                    {
                        Log.Error("库ID为0， 插入时未更新ID");
                    }
                    //数据库中关联库和用户
                    try
                    {
                        _uservaultService.AddUserVault(userId, vault.Id, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("添加创建项目的用户为库用户失败：" + ex.Message, ex);
                        throw;
                    }
                    _projectMemberService.AddMember(project.Id, userId, res.Contact.InternalId, true); //数据库关联项目成员

                    return res;
                });

                if (!cr0.Success)
                {
                    if (cr0.Exception == null) Log.Warn(cr0.Message);
                    else Log.Warn(cr0.Message, cr0.Exception);
                    return BadRequest(cr0.Message);
                }
                return Ok(vault.ToDtoWithoutTemplate());
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("创建项目空间失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        private static void CreateVaultForAllBackup(MFilesVault vault, VaultTemplate template, MFSqlDatabase sqlDb, IMFVaultService _vaultService)
        {
            //3. 创建相应的M-Files库
            try
            {
                var mfUsername = System.Configuration.ConfigurationManager.AppSettings["mfusername"];
                var mfPassword = System.Configuration.ConfigurationManager.AppSettings["mfpassword"];

                _vaultService.CreateForAllBackup(vault, template.StructurePath, mfUsername, mfPassword, sqlDb, null);
            }
            catch (Exception ex)
            {
                Log.Error("创建项目库失败：" + ex.Message, ex);
                throw;
            }
        }
        private static void CreateVault(MFilesVault vault, VaultTemplate template, MFSqlDatabase sqlDb, IMFVaultService _vaultService)
        {
            //3. 创建相应的M-Files库
            try
            {
                var mfUsername = System.Configuration.ConfigurationManager.AppSettings["mfusername"];
                var mfPassword = System.Configuration.ConfigurationManager.AppSettings["mfpassword"];
                _vaultService.Create(vault, template.StructurePath, mfUsername, mfPassword, sqlDb, null, false);
            }
            catch (Exception ex)
            {
                Log.Error("创建项目库失败：" + ex.Message, ex);
                throw;
            }
        }

        private static int CreateLoginAccount(MFilesVault vault, User creator, IMFUserService _vaultUserService)
        {
            //4. 添加库用户和组用户
            var loginAccount = -1;
            try
            {
                //if (string.IsNullOrEmpty(creator.Domain) && AuthUtility.IsDomainEnabled())
                //{
                //    creator.Domain = DomainClient.Domain;
                //}
                //登录账户
                _vaultUserService.CreateMFilesLoginAccount(creator, vault.Server);

                //Log.Info("Creator Domain:" + creator.Domain);
                loginAccount = _vaultUserService.CreateVaultUser(creator, vault);
            }
            catch (Exception ex)
            {
                Log.Error("添加库用户失败：" + ex.Message, ex);
                throw;
            }
            return loginAccount;
        }


        private static MfContact CreateContact(MFilesVault vault, MetadataAliases aliases, User creator, int loginAccount, string partyName
            , IMFObjectService mfObjService)
        {
            //4.1创建联系人对象
            //var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            MfContact contact = null;
            try
            {
                contact = new MfContact
                {
                    User = creator,
                    Id = loginAccount,
                    IsCreator = true,
                    RoleAlias = aliases.UserGroups["UGroupPM"]
                };
                if (partyName != null)
                {
                    contact.PartName = partyName;
                }
                CreateMfObj(vault, aliases, contact, mfObjService);
            }
            catch (Exception ex)
            {
                Log.Error("创建库中的联系人对象失败：" + ex.Message, ex);
                throw;
            }
            return contact;
        }

        private static void CreateMfProject(Project proj, MetadataAliases aliases, MFilesVault vault, IMFObjectService mfObjService)
        {
            try
            {
                ToLocalTimeProj(proj);
                CreateMfObj(vault, aliases, proj, mfObjService);
                ToUtcTimeProj(proj);
            }
            catch (Exception ex)
            {
                Log.Error("创建库中的项目对象失败：" + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// 通过对象别名和title销毁对象
        /// </summary>
        /// <param name="classAlias"></param>
        /// <param name="title"></param>
        private static void DestoryTemplateObj(Vault vault,string classAlias,string title)
        {
            try
            {
                var scs = new SearchConditions();
                var scClass = MFSearchConditionUtils.Class(vault.ClassOperations.GetObjectClassIDByAlias(classAlias));
                scs.Add(-1, scClass);
                var scTitle = MFSearchConditionUtils.Property(MFConditionType.MFConditionTypeEqual, 0, MFDataType.MFDatatypeText, title);
                scs.Add(-1, scTitle);
                var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone, false);
                vault.ObjectOperations.DestroyObjects(res.GetAsObjectVersions().GetAsObjVers().GetAllDistinctObjIDs());
            }
            catch { }
        }

        private static CreateRes CreateProjectForAllBackupInternal(Project proj, User creator, VaultTemplate template, MFilesVault vault, MFSqlDatabase sqlDb, ProjectParty party
         , IMFVaultService _vaultService, IMFUserService _vaultUserService, IMFObjectService mfObjService, IMFilesVaultService _mfvaultService, IProjectService _projectService
         , IProjectMemberService _projectMemberService, IUserVaultService _uservaultService, IUserService _userService, IMfUserGroupService _mfusergroupService, string password, string[] modelUnits)
        {
            var cr = new CreateRes();
            var userId = creator.Id;
            //3. 创建相应的M-Files库  CreateVaultForAllBackup
            CreateVaultForAllBackup(vault, template, null, _vaultService);
            cr.Vault = vault;
            //4. 添加库用户和组用户
            var loginAccount = CreateLoginAccount(vault, creator, _vaultUserService);
            //6. 更新数据库
            _mfvaultService.InsertVault(vault);
            proj.VaultId = vault.Id;
            try
            {
                Log.Info("AreaId:-" + proj.AreaId);
                Log.Info("CompanyId:-" + proj.CompanyId);
                Log.Info("Name:-" + proj.Name);
                Log.Info("TemplateId:-" + proj.TemplateId);
                Log.Info("Description:-" + proj.Description);
                Log.Info("EndDateUtc:-" + proj.EndDateUtc);
                Log.Info("StartDateUtc:-" + proj.StartDateUtc);
                Log.Info("PartyId:-" + proj.PartyId);
                _projectService.CreateProject(proj); //创建数据库中的项目对象
            }
            catch (Exception ex)
            {
                Log.Error("_projectService.CreateProject(proj); error:-" + ex.Message, ex);
            }
            //数据库中关联库和用户
            try
            {
                _uservaultService.AddUserVault(userId, vault.Id, true);
            }
            catch (Exception ex)
            {
                Log.Error("添加创建项目的用户为库用户失败：" + ex.Message, ex);
                // throw;
            }

            try
            {
                var server = vault.Server;
                var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd,
                    MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
                var mVault = app.LogInToVault(vault.Guid);
                var views = mVault.ViewOperations.GetViews();
                //      CuttingTemplate(mVault, proj, loginAccount);
                CreateMfProject(proj, mVault);
                //    ClassProjectHandOverListProcessing(proj, mVault);//deprecated

                //  CreateProjectManagerForVault(mVault, loginAccount);
                //需求更改m-files://show/09E18EF0-8ED1-4CB0-B80C-565190B3A638/0-28518/36042?object=C227B1B0-BF42-4873-9675-6477EED16782&file=36E1F1D0-12E2-470D-B08A-8219093F4332
                //18条需求
                var users = _userService.GetAllUsers();
                var companies = _projectService.GetAllCompany().ToList();
                CreateHeadquatersStaff(mVault, users, companies, proj, _projectMemberService);
                //    UserAndGroupProcessing(mVault, proj.CompanyId);//need modification,to be continued//deprecated
                if (proj.Company.Code == "0001A210000000002ORS") //0001A210000000002ORS,中建八局第二建设有限公司,
                {
                    RemoveSecondLevelGroups(mVault);

                    DisableClass(mVault, "ClassAuditSupervisionMeetingSummarySecondLevel");
                    DisableClass(mVault, "ClassProjectDelayAnalysisSecondLevel");

                    DisableClass(mVault, "ClassVisaAndMeasureSecondLevel");
                    DisableClass(mVault, "ClassConstructionPeriodDelayApproval");
                    DisableClass(mVault, "ClassProjectCompletionConfirmSecondLevel");
                    DisableView(mVault, views, "项目竣工确认单-一般项目");
                  
                    DisableClass(mVault, "ClassMainControlPointPlanSecondLevel");

                    //删除二级单位监理例会模版
                    DestoryTemplateObj(mVault,"ClassSupervisorMeeting", "6.CSCEC82-PM-B3306《监理例会纪要》审核记录表(新增）-一般项目");
                    switch (proj.LevelId)
                    {
                        case 1:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelNotImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlImportant");
                            break;
                        case 2:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelNotImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlNotImportant");
                            break;
                        default:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelNotImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlNotImportant");
                            break;
                    }
                }
                else //二级单位
                {
                    SecondLevelUserAndGroupProcessing(mVault, users, companies, proj, _projectMemberService);

                    DisableClass(mVault, "ClassAuditSupervisionMeetingSummaryDirectlyControl");
                    DisableClass(mVault, "ClassProjectDelayAnalysisDirectlyControl");

                    DisableClass(mVault, "ClassVisaAndMeasureDirectlyControl");
                    DisableClass(mVault, "ClassMainControlPointPlanDirectlyControl");
                    DisableClass(mVault, "ClassConstructionPeriodDelayApprovalDirectlyControl");
                    DisableClass(mVault, "ClassProjectCompletionConfirmDirectlyControl");
                    DisableView(mVault,views, "项目竣工确认单-直属项目");
                  
                    //删除总公司监理例会模版
                    DestoryTemplateObj(mVault,"ClassSupervisorMeeting", "6.CSCEC82-PM-B3306《监理例会纪要》审核记录表(新增）-直属项目");
                    switch (proj.LevelId)
                    {
                        case 1:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlNotImportant");
                            break;
                        case 2:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelNotImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlNotImportant");
                            break;
                        default:
                            DisableClass(mVault, "ClassMonthlyEvaluationSecondLevelNotImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlImportant");
                            DisableClass(mVault, "ClassMonthlyEvaluationDirectlyControlNotImportant");
                            break;
                    }
                }

                SetVicePresident(mVault, proj, _userService, _mfusergroupService);

                ////失败，需要另外使用app登录
                //var passWord = DBWorldCache.Get(userId.ToString());
                //var authType = MFAuthType.MFAuthTypeSpecificMFilesUser;
                //var domain = creator.Domain ?? String.Empty;
                //if (String.IsNullOrEmpty(domain))
                //{
                //    authType = MFAuthType.MFAuthTypeSpecificWindowsUser;
                //}
                //try
                //{
                //    mVault = app.LogInAsUserToVault(vault.Guid, AuthType: authType, UserName: creator.UserName,
                //        Password: passWord, Domain: domain);
                //    CreateModelUnits(mVault, modelUnits);
                //}
                //catch (Exception e)
                //{
                //    Log.Error("创建单体失败：" + e.Message, e);
                //    throw;
                //}
            }
            catch (Exception ex)
            {
                var err = " private static CreateRes CreateProject 创建库中vaultapp error：" + ex.Message;
                Log.Error(err);
                throw;
            }
           
            if (modelUnits != null && modelUnits.Length > 0)
            {
                try
                {
                    foreach (var unit in modelUnits)
                    {
                        var server = vault.Server;
                        //   Log.Info(string.Format("user {0} ,{2}, pass {1},userId={3},creator.Id={4}", creator.UserName, String.Empty, creator.FullName, userId, creator.Id));
                        var app = MFServerUtility.ConnetToMfApp(creator, password, server);
                        var mVault = app.LogInToVault(vault.Guid);
                        CreateModelUnit(mVault,unit);
                        mVault.LogOutSilent();
                        app.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("创建单体失败：" + ex.Message, ex);
                }
            }

            try
            {
                _projectMemberService.AddMember(proj.Id, userId, 0, true); //数据库关联项目成员
            }
            catch (Exception ex)
            {
                Log.Error(" _projectMemberService.AddMember？？？:" + ex.Message, ex);
            }
            return cr;
        }
     
        private static void DisableView(Vault mVault, Views views, string name)
        {
            try
            {
                foreach (View view in views)
                {
                    if (view.Name == name)
                    {
                        mVault.ViewOperations.RemoveView(view.ID);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("DisableView {0} error in {1} ",name,mVault.Name+ex.Message));
            }
        }
        /// <summary>
        /// 创建单体
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="units"></param>
        private static void CreateModelUnit(Vault vault, string unit)
        {
            //if (units == null || units.Length == 0) return;
            try
            {
                //var objVers = new ObjVers();
               // foreach (var u in units)
               // {
                   // var vault = app.LogInToVault(vaultGuid);
                    var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjModelUnit");
                    var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassModelUnit");

                    var namePD = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle;
                    var pvs = new PropertyValues();

                    var classPV = new PropertyValue { PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass };
                    classPV.Value.SetValue(MFDataType.MFDatatypeLookup, classId);
                    pvs.Add(-1, classPV);

                    var namePV = new PropertyValue { PropertyDef = namePD };
                    namePV.Value.SetValue(MFDataType.MFDatatypeText, unit);
                    pvs.Add(-1, namePV);

                    var objVersionAndProps = vault.ObjectOperations.CreateNewObject(typeId, pvs);
                    vault.ObjectOperations.CheckIn(objVersionAndProps.ObjVer);
                   // objVers.Add(-1, objVersionAndProps.ObjVer);
               // }
               // vault.ObjectOperations.CheckInMultipleObjects(objVers);
            }
            catch (Exception ex)
            {
                Log.Info("CreateModelUnits error:"+ex.Message);
            }
        }

        private static void SetVicePresident(Vault mVault, Project proj, IUserService _userService, IMfUserGroupService _mfusergroupService)
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;

            var sqlc = new SqlConnection(connstr);
            sqlc.Open();
            SetVicePresidentByCompany(proj, sqlc, _userService, _mfusergroupService, proj.CompanyId);
            //try
            //{
            //    var select = string.Format("select userid,groupid from usergroup where id = '{0}' ", proj.CompanyId);
            //    var sqlcommand = new SqlCommand(select, sqlc);
            //    var rds = new SqlDataAdapter(sqlcommand);
            //    var dt = new DataTable();
            //    rds.Fill(dt);
            //    if (dt.Rows.Count > 0)
            //    {
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            var index = 0;
            //            var userg = new UserGroupDb();
            //            foreach (DataColumn column in dt.Columns)
            //            {
            //                switch (index)
            //                {
            //                    case 0:
            //                        userg.UserId = int.Parse(row[column].ToString());
            //                        break;
            //                    case 1:
            //                        userg.GroupId = int.Parse(row[column].ToString());
            //                        break;
            //                }
            //                index++;
            //            }
            //            var username = _userService.GetUserById(userg.UserId).UserName;
            //            string mfigroupid = GetMfGroupName(userg.GroupId);
            //            _mfusergroupService.AddUserToGroup(proj.Vault, username, mfigroupid);
            //        }
            //    }
            if (proj.Company.Code != "0001A210000000002ORS")
            {

                SetVicePresidentByCompany(proj, sqlc, _userService, _mfusergroupService, 1);
            }
            //}
            //catch (Exception ex)
            //{
            //    Log.Info("SetVicePresident error:" + ex.Message);
            //}
            SetErpPmUser(proj, sqlc, _userService, _mfusergroupService, mVault);
            sqlc.Close();
        }
        private static void SetErpPmUser(Project proj, SqlConnection sqlc, IUserService _userService, IMfUserGroupService _mfusergroupService, Vault ovault)
        {
            try
            {
                var select1 = string.Format("select userid from erppm  ");
                var sqlcommand1 = new SqlCommand(select1, sqlc);
                var rds1 = new SqlDataAdapter(sqlcommand1);
                var dt1 = new DataTable();
                rds1.Fill(dt1);
                var userid = 1;
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        foreach (DataColumn column in dt1.Columns)
                        {
                            userid = int.Parse(row[column].ToString());
                                    break;
                            
                        }
                        var username = _userService.GetUserById(userid).UserName;
                        var userId = _mfusergroupService.GetUserId(ovault, username);
                        var ugid =
                            ovault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                                MfilesAliasConfig.UgErpPrincipal);
                        _mfusergroupService.AddUserToGroup(ovault, userId, ugid);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info( "SetErpPmUser error:" + ex.Message);
            }
        }
        private static void SetVicePresidentByCompany(Project proj, SqlConnection sqlc, IUserService _userService, IMfUserGroupService _mfusergroupService, long companyid)
        {
            try
            {
                var select1 = string.Format("select userid,groupid from usergroup where id = '{0}' ", companyid);
                var sqlcommand1 = new SqlCommand(select1, sqlc);
                var rds1 = new SqlDataAdapter(sqlcommand1);
                var dt1 = new DataTable();
                rds1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        var index = 0;
                        var userg = new UserGroupDb();
                        foreach (DataColumn column in dt1.Columns)
                        {
                            switch (index)
                            {
                                case 0:
                                    userg.UserId = int.Parse(row[column].ToString());
                                    break;
                                case 1:
                                    userg.GroupId = int.Parse(row[column].ToString());
                                    break;
                            }
                            index++;
                        }
                        var username = _userService.GetUserById(userg.UserId).UserName;
                        string mfigroupid = GetMfGroupName(userg.GroupId);
                        _mfusergroupService.AddUserToGroup(proj.Vault, username, mfigroupid);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(companyid + "SetVicePresidentByCompany error:" + ex.Message);
            }
        }

        private static string GetMfGroupName(long groupid)
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            var ret = string.Empty;
            var sqlc = new SqlConnection(connstr);
            sqlc.Open();
            try
            {
                var select = string.Format("select name from groupcategory where id = '{0}' ", groupid);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();
                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        foreach (DataColumn column in dt.Columns)
                        {

                            ret = row[column].ToString();
                            break;

                        }
                        break;
                    }
                }
                else
                {
                    Log.Info(select + "GetMfGroupName there is no name for --" + groupid);

                }
            }
            catch (Exception ex)
            {
                Log.Info("GetMfGroupName error:" + ex.Message);
            }
            sqlc.Close();
            return ret;
        }

        private static void SecondLevelUserAndGroupProcessing(Vault mVault, IList<global::AecCloud.Core.Domain.User> users, List<Company> companies, Project proj, IProjectMemberService _projectMemberService)
        {
            var ugkey_department = new string[]
                {
                   "施工管理部","工程部","经营部","商务部","物资部","财务部","办公室","综合部","生产部","物资合约部","租赁经营部","幕墙设计部","装饰设计部","领导班子"
             
                };

            var UgSecondLevelLeaders =
                mVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgSecondLevelLeaders);
            var uga = mVault.UserGroupOperations.GetUserGroupAdmin(UgSecondLevelLeaders);
            var ugs = mVault.UserGroupOperations.GetUserGroups();
            var allcount = 0;
            var invaultcount = 0;
            foreach (User user in users)
            {
                if (user.CompanyId != proj.Company.Id) continue;
                allcount++;
                foreach (string s in ugkey_department)
                {
                    if (user.Department.Name != s) continue;
                    var account = new UserAccount
                  {
                      LoginName = user.UserName,
                      InternalUser = true,
                      VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles
                  };

                    try
                    {
                        var ua = mVault.UserOperations.AddUserAccount(account);
                        Log.Info(string.Format("SecondLevelUserAndGroupProcessing  UserAccount {0},aecUser {1},vault {2},FullName={3},Company={4},PositionInfo={5},Department={6}", ua.LoginName, user.CscecRole.Name,
  mVault.Name, user.FullName, user.Company.Name, user.PositionInfo.Name, user.Department.Name));
                        if (user.Department.Name.Contains("领导班子"))
                        {
                            uga.UserGroup.AddMember(ua.ID);
                        }


                        var groupidentifer = (s == "施工管理部" || s == "施工管理部") ? "工程管理部" : s;
                        if (user.CscecRole.Name == "经理" || user.PositionInfo.Name == "经理" || user.CscecRole.Name == "主任" || user.PositionInfo.Name == "主任")
                        {
                            foreach (UserGroup userGroup in ugs)
                            {
                                if (userGroup.Name.Contains("二级单位-" + groupidentifer + "经理"))
                                {
                                    AddUserIntoGroup(ua, userGroup, mVault);
                                    break;
                                }
                            }
                        }
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name=="二级单位-" + groupidentifer)
                            {
                                AddUserIntoGroup(ua, userGroup, mVault);
                                break;
                            }
                        }
                        SecondLevelGroupProcessing(ua, user, mVault);

                        _projectMemberService.AddMember(proj.Id, user.Id, 0, false, false);
                        invaultcount++;
                        break;
                    }
                    catch (Exception) { }
                }
            }
            mVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
            Log.Info(string.Format("SecondLevelUserAndGroupProcessing vault {0}, all employees {1}, created users {2},{3},{4}", mVault.Name, allcount, invaultcount, proj.Company.Name, proj.Name));

        }
        private static void SecondLevelGroupProcessing(UserAccount ua, global::AecCloud.Core.Domain.User user, Vault vault)
        {
            try
            {
                var ugs = vault.UserGroupOperations.GetUserGroups();

                var ugkey_chiefpostion = new string[]
                {
                   "总工程师", "总会计师", "总经济师"
             
                };
                foreach (string chief in ugkey_chiefpostion)
                {
                    if ((user.CscecRole.Name.Contains(chief) || user.PositionInfo.Name.Contains(chief)) &&
                        user.Department.Name == "领导班子")
                    {
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("二级单位-" + chief))
                            {

                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }

                ManagerGroupProcessing("经理", ugs, user, vault, ua);
                ManagerGroupProcessing("书记", ugs, user, vault, ua);
                var ugkey = new string[]
                {
                    "分公司经理", "副经理",  "副经理（生产）", "工会主席",
                "财务部","审计部"
             
                };
                foreach (string s in ugkey)
                {
                    if (user.CscecRole.Name == s || user.Department.Name == s || user.PositionInfo.Name == s)
                    {

                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("二级单位-" + s))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("SecondLevelGroupProcessing error {0},{1},{2}:" + ex.Message, user.UserName, vault.Name, ua.ID));
            }
        }

        private static void AddUserIntoGroup(UserAccount ua, UserGroup userGroup, Vault vault)
        {
            try
            {
                var uga = vault.UserGroupOperations.GetUserGroupAdmin(userGroup.ID);
                uga.UserGroup.AddMember(ua.ID);
                vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                Log.Info(string.Format("AddUser {0}  IntoGroup {1} UpdateUserGroupAdmin ok {2}", ua.LoginName,
                    userGroup.Name, vault.Name));
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("AddUser {0}  IntoGroup {1} UpdateUserGroupAdmin error {2},{3}", ua.LoginName,
                    userGroup.Name, vault.Name, ex.Message));
            }
        }

        private static void ManagerGroupProcessing(string smanager, UserGroups ugs, global::AecCloud.Core.Domain.User user, Vault vault, UserAccount ua)
        {
            if ((user.CscecRole.Name == smanager || user.PositionInfo.Name == smanager) && user.Department.Name == "领导班子")
            {
                foreach (UserGroup userGroup in ugs)
                {
                    if (userGroup.Name.Contains("二级单位-" + smanager))
                    {
                        var uga = vault.UserGroupOperations.GetUserGroupAdmin(userGroup.ID);
                        uga.UserGroup.AddMember(ua.ID);
                        vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                        break;
                    }
                }
            }
        }
        private static void RemoveSecondLevelGroups(Vault mVault)
        {
            try
            {
                var groups = mVault.UserGroupOperations.GetUserGroups();
                foreach (UserGroup userGroup in groups)
                {
                    if (userGroup.Name.StartsWith("二级单位"))
                    {
                        mVault.UserGroupOperations.RemoveUserGroupAdmin(userGroup.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("RemoveSecondLevelGroups error:" + ex.Message);
            }
        }

        private static void CreateHeadquatersStaff(Vault mVault, IList<global::AecCloud.Core.Domain.User> list, List<Company> companies, Project proj, IProjectMemberService _projectMemberService)
        {
            Log.Info(string.Format("in CreateHeadquatersStaff, {0}", mVault.Name));
            var UgHLeaders =
                mVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgHLeaders);
            if (UgHLeaders < 0)
            {
                Log.Info(string.Format("CreateHeadquatersStaff, alias {0} is not found  in vault {1}", MfilesAliasConfig.UgHLeaders, mVault.Name));
                return;
            }
            try
            {
                foreach (Company headquarter in companies)
                {
                    if (headquarter.Code == "0001A210000000002OSD")// 0001A210000000002OSD,总部机关, 0001A210000000002ORS,中建八局第二建设有限公司,
                    {
                        var ug = mVault.UserGroupOperations.GetUserGroupAdmin(UgHLeaders);
                        var headquarterusercount = 0;
                        var correctcount = 0;
                        foreach (User user in list)
                        {
                            if (user.CompanyId == headquarter.Id)
                            {
                                headquarterusercount++;
                                var account = new UserAccount
                                {
                                    LoginName = user.UserName,
                                    InternalUser = true,
                                    VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles
                                };
                                if (user.Disabled) account.Enabled = false;
                                try
                                {
                                    //   account.AddVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleManageCommonViews);
                                    var ua = mVault.UserOperations.AddUserAccount(account);
                                    if (user.Department.Code == "1001A210000000001M1E") //公司领导{
                                    {
                                        ug.UserGroup.AddMember(ua.ID);
                                    }
                                    //  if(user.CscecRoleId)
                                    UserGroupProcessing(ua, user, mVault);
                                    //      _projectMemberService.AddMember(proj.Id, user.Id, 0);
                                    _projectMemberService.AddMember(proj.Id, user.Id, 0, false, false);
                                    correctcount++;
                                }
                                catch (Exception) { }
                            }
                        }
                        mVault.UserGroupOperations.UpdateUserGroupAdmin(ug);
                        Log.Info(string.Format("CreateHeadquatersStaff vault {0}, all headquarter employees {1}, created users {2}", mVault.Name, headquarterusercount, correctcount));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("CreateHeadquatersStaff error：" + ex.Message);
            }
        }


        private static void UserGroupProcessing(UserAccount ua, global::AecCloud.Core.Domain.User user, Vault vault)
        {
            try
            {
                Log.Info(string.Format("UserGroupProcessing  UserAccount {0},aecUser {1},vault {2},FullName={3},Company={4},PositionInfo={5},Department={6}", ua.LoginName, user.CscecRole.Name,
                    vault.Name, user.FullName, user.Company.Name, user.PositionInfo.Name, user.Department.Name));
                var ugs = vault.UserGroupOperations.GetUserGroups();

                var ugkey_department = new string[]
                {
                    "安全环境部","安全生产管理部",
                    "施工管理部","工程管理部",
                    "成本管理部","工程结算部","商务管理部",
                    "工会","工会工作部",
                    "党委工作部","政工部",
                     "纪检监察室", "纪检监察部",
                    "办公室","人力资源部","市场一部","市场二部","物资部","技术中心","财务部","审计部", "合约法务部","投资与资金部","投标管理部","科技部","海外部"
                };
                foreach (string s in ugkey_department)
                {
                    if (user.Department.Name != s) continue;
                    var groupidentifer = s;
                    switch (s)
                    {
                        case "安全环境部":
                            groupidentifer = "安全生产管理部";
                            break;
                        case "施工管理部":
                            groupidentifer = "工程管理部";
                            break;
                        case "成本管理部":
                        case "工程结算部":
                            groupidentifer = "商务管理部";
                            break;
                        case "工会":
                            groupidentifer = "工会工作部";
                            break;
                        case "党委工作部":
                        case "政工部":
                            groupidentifer = "党委工作部";
                            break;
                        case "纪检监察室":
                            groupidentifer = "纪检监察部";
                            break;
                    }
                    foreach (UserGroup userGroup in ugs)
                    {
                        if (userGroup.Name=="公司总部-" + groupidentifer)
                        {
                            AddUserIntoGroup(ua, userGroup, vault);
                            break;
                        }
                    }
                    if ((user.CscecRole.Name == "经理" || user.PositionInfo.Name == "经理" || user.CscecRole.Name == "主任" || user.PositionInfo.Name == "主任"))
                    {
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("公司总部-" + groupidentifer + "经理"))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }

                var ugkey = new string[]
                {
                    "总经理", "副总经理", "总工程师", "党委副书记", "总经济师", "董事长", "党委书记","纪委书记","工会主席"
                };
                foreach (string s in ugkey)
                {
                    if (user.CscecRole.Name == s || user.PositionInfo.Name == s || user.Department.Name == s)
                    {
                        Log.Info(string.Format("UserGroupProcessing  UserAccount {0},aecUser {1},vault {2},match group {3}",
                            ua.LoginName, user.CscecRole.Name, vault.Name, s));
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("公司总部-" + s))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("UserGroupProcessing error {0},{1},{2}:" + ex.Message, user.UserName, user.FullName, ua.ID));
            }
        }

        private static void CreateProjectManagerForVault(Vault mVault, int loginAccount)
        {
            try
            {
                //var mfilesusername = creator.UserName;
                //var users = mVault.UserOperations.GetUserAccounts();
                //var mfilesuserid = -1;
                //foreach (UserAccount userAccount in users)
                //{
                //    if (userAccount.LoginName == mfilesusername)
                //    {
                //        mfilesuserid = userAccount.ID;   
                //        break;
                //    };
                //}
                //if (mfilesuserid < 0)
                //{
                //    mVault.
                //}

                //创建者自动作为项目经理进入用户组
                var id = mVault.GetMetadataStructureItemIDByAlias(
                    MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UGroupPM);
                // var ug=  mVault.UserGroupOperations.GetUserGroup(id);//.AddMember(loginAccount);
                var uga = mVault.UserGroupOperations.GetUserGroupAdmin(id);
                uga.UserGroup.Members.Add(-1, loginAccount);
                mVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                Log.Info(string.Format("add user <{0}> into pm usergroup in vault <{1}> ok,{2}", loginAccount, mVault.Name, id));
            }
            catch (Exception ex)
            {
                Log.Error("add creator in pm usergroup error:" + ex.Message);
            }
        }

        private static void ClassProjectHandOverListProcessing(Project proj, Vault vault)
        {//此路不通，放弃
            //try
            //{
            //    var sc = new SearchCondition();
            //    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            //    sc.Expression.DataPropertyValuePropertyDef =(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            //    sc.TypedValue.SetValueToLookup(new Lookup
            //    {
            //        Item =
            //            vault.GetMetadataStructureItemIDByAlias(
            //                MFMetadataStructureItem.MFMetadataStructureItemClass, MfilesAliasConfig.ClassProjectHandOverList)
            //    });
            //    var sr = vault.ObjectSearchOperations.SearchForObjectsByCondition(sc, false).ObjectVersions;
            //  Log.Info("11");
            //    var objver = sr[1].ObjVer;
            //    var files = vault.ObjectFileOperations.GetFiles(objver);
            //    var filepath = Path.GetTempFileName();
            //    var objectFile = files[1];
            //    vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, filepath);
            //    Log.Info("22");
            //   var app = new Application();
            //    object unknow = Type.Missing;
            //   var doc = app.Documents.Open(filepath,
            //        ref unknow, false, ref unknow, ref unknow, ref unknow,
            //        ref unknow, ref unknow, ref unknow, ref unknow, ref unknow,
            //        ref unknow, ref unknow, ref unknow, ref unknow, ref unknow);
            //  var  table = doc.Tables[doc.Tables.Count];
            //  Log.Info("33");
            //  table.Cell(5, 2).Range.Text = proj.Name+proj.Number;
            //  doc.Save();
            //  doc.Close();
            //  app.Quit();
            ////  vault.ObjectFileOperations.UploadFile(objectFile.ID, objectFile.Version, filepath);
            //  Log.Info("44");
            //  var filesup = vault.ObjectFileOperations.GetFilesForModificationInEventHandler(objver);
            //  Log.Info("55");
            //  foreach (ObjectFile objectFile1 in filesup)
            //  {
            //      vault.ObjectFileOperations.UploadFile(objectFile1.ID, objectFile1.Version, filepath);
            //      break;
            //  }
            //  Log.Info("66");
            //}
            //catch (Exception ex)
            //{
            //    Log.Info("ClassProjectHandOverListProcessing:"+ex.Message);
            //}
        }


        private static void UserAndGroupProcessing(Vault mVault, long company)
        {
            for (long i = 2; i < 15; i++)
            {
                if (i != company)
                {
                    try
                    {
                        var ug =
                            mVault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemUserGroup, i.ToString());
                        var oug = mVault.UserGroupOperations.GetUserGroup(ug);
                        foreach (int id in oug.Members)
                        {
                            if (id < 0) continue;
                            mVault.UserOperations.RemoveUserAccount(id);
                        }
                        mVault.UserGroupOperations.RemoveUserGroupAdmin(ug);
                    }
                    catch (Exception ex)
                    {
                        Log.Info("UserAndGroupProcessing" + ex.Message);
                    }
                }
            }

        }

        private static void DisableClass(Vault mVault, string oneclassalias)
        {
            try
            {
                var oca =
                    mVault.ClassOperations.GetObjectClassAdmin(
                        mVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                            oneclassalias));

                mVault.ClassOperations.RemoveObjectClassAdmin(oca.ID);
            }
            catch (Exception ex)
            {
                Log.Info("RemoveObjectClassAdmin:" + oneclassalias + ex.Message);
            }
        }

        private static void CreateMfProject(Project proj, Vault vault)
        {
            var objtype =
                vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType, MfilesAliasConfig.ObjProject);
            var pvs = new PropertyValues();
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                           MfilesAliasConfig.PropProjName)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.Name);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropProjNum)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.Number);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue { PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass };
                pv.Value.SetValueToLookup(new Lookup { Item = vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass, MfilesAliasConfig.ClassProject) });
                pvs.Add(-1, pv);
            }

            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropProprietorUnit)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.OwnerUnit);
                pvs.Add(-1, pv);
            }

            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropSupervisorUnit)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.SupervisionUnit);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropDesignUnit)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.DesignUnit);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PmUnit)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeMultiLineText, proj.PmUnit);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.InvestigateUnit)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeMultiLineText, proj.InvestigateUnit);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropStartDate)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeDate, proj.StartDateUtc);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeDate, proj.EndDateUtc);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropContactValue)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.ContractAmount);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropDescription)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeMultiLineText, proj.Description);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropProjectArea)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.Area);
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                           MfilesAliasConfig.PropConstructionScale)
                };
                pv.Value.SetValue(MFilesAPI.MFDataType.MFDatatypeText, proj.ConstructionScale);
                pvs.Add(-1, pv);
            }

            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropCompany)
                };
                pv.Value.SetValueToLookup(new Lookup { Item = (int)proj.CompanyId });
                pvs.Add(-1, pv);
            }
            {  //PropProjClassDetail
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropProjClassDetail)
                };
                pv.Value.SetValueToLookup(new Lookup { Item = (int)proj.ClassId} );
                pvs.Add(-1, pv);
            }
            {
                var pv = new PropertyValue
                {
                    PropertyDef =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                           MfilesAliasConfig.PropProjectLevel)
                };
                //  Log.Info(string.Format("plevel-{0}", proj.Level + proj.LevelId.ToString()));
                pv.Value.SetValueToLookup(new Lookup { Item = (int)proj.LevelId });
                pvs.Add(-1, pv);
            }
            vault.ObjectOperations.CreateNewObjectExQuick(objtype, pvs);
        }

        private static int GetVlItemByName(Vault vault, string vlAlias, string vlItemName)
        {
            var vlId = vault.ValueListOperations.GetValueListIDByAlias(vlAlias);
            var vlItems = vault.ValueListItemOperations.GetValueListItems(vlId);
            foreach (ValueListItem vlItem in vlItems)
            {
                if (vlItem.Name == vlItemName)
                {
                    return vlItem.ID;
                }
            }
            return -1;
        }
        private static CreateRes CreateProject(Project proj, User creator, VaultTemplate template, MFilesVault vault, MFSqlDatabase sqlDb, ProjectParty party
            , IMFVaultService _vaultService, IMFUserService _vaultUserService, IMFObjectService mfObjService, IMFilesVaultService _mfvaultService, IProjectService _projectService
            , IProjectMemberService _projectMemberService, IUserVaultService _uservaultService)
        {
            var cr = new CreateRes();
            var userId = creator.Id;
            //3. 创建相应的M-Files库
            CreateVault(vault, template, null, _vaultService);
            cr.Vault = vault;
            //4. 添加库用户和组用户
            var loginAccount = CreateLoginAccount(vault, creator, _vaultUserService);
            //4.1创建联系人对象
            string partyName = null;
            if (party != null)
            {
                partyName = party.Name;
            }
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            MfContact contact = CreateContact(vault, aliases, creator, loginAccount, partyName, mfObjService);
            //5. 创建MFiles对象(项目对象)
            CreateMfProject(proj, aliases, vault, mfObjService);
            //6. 更新数据库
            _mfvaultService.InsertVault(vault);
            proj.VaultId = vault.Id;
            proj.LevelId = 2;//暂时约定为重点工程
            try
            {
                //Log.Info("AreaId:-" + proj.AreaId);
                //Log.Info("CompanyId:-" + proj.CompanyId);
                //Log.Info("Name:-" + proj.Name);
                //Log.Info("TemplateId:-" + proj.TemplateId);
                //Log.Info("Description:-" + proj.Description);
                //Log.Info("EndDateUtc:-" + proj.EndDateUtc);
                //Log.Info("StartDateUtc:-" + proj.StartDateUtc);
                //Log.Info("PartyId:-" + proj.PartyId);
                _projectService.CreateProject(proj); //创建数据库中的项目对象
            }
            catch (Exception ex)
            {
                Log.Error("_projectService.CreateProject(proj); error:-" + ex.Message, ex);
            }
            //数据库中关联库和用户
            try
            {
                _uservaultService.AddUserVault(userId, vault.Id, true);
            }
            catch (Exception ex)
            {
                Log.Error("添加创建项目的用户为库用户失败：" + ex.Message, ex);
                // throw;
            }
            //install vaultapp
            try
            {
                var server = vault.Server;
                var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd, MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);

                var mVault = app.LogInToVault(vault.Guid);
                var tmpfile = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~"), "vaultapp.mfappx");
                mVault.CustomApplicationManagementOperations.InstallCustomApplication(tmpfile);
                app.VaultManagementOperations.TakeVaultOffline(vault.Guid, true);
                app.VaultManagementOperations.BringVaultOnline(vault.Guid);
            }
            catch (Exception ex)
            {
                var err = " private static CreateRes CreateProject 创建库中vaultapp error：" + ex.Message;
                Log.Error(err);
                throw;
            }
            try
            {
                _projectMemberService.AddMember(proj.Id, userId, contact.InternalId, true); //数据库关联项目成员
            }
            catch (Exception ex)
            {
                Log.Error(" _projectMemberService.AddMember:" + ex.Message, ex);
            }
            return cr;
        }


        /// <summary>
        /// 更新项目信息
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model">可编辑的项目信息模型</param>
        /// <returns></returns>
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> Update(ProjectEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.StartDateUtc > model.EndDateUtc)
            {
                return BadRequest(String.Format("起始日期({0})必须小于终止日期({1})", model.StartDateUtc.ToLocalTime(), model.EndDateUtc.ToLocalTime()));
            }
            try
            {
                var err = await Task.Run(() =>
                    UpdateProject(model, GetUserName(), _projectService, _mfvaultService, _mfusergroupService, _vaultTemplateService, _mfilesObjService));
                if (!String.IsNullOrEmpty(err))
                {
                    return BadRequest(err);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("编辑项目({0})失败：", model.Id), HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 移交项目
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Transfer(ProjectTransferModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (model.ToUserId == GetUserId()) return StatusCode(HttpStatusCode.NotModified);
                var err = await TransferProject(model);
                if (!String.IsNullOrEmpty(err)) return BadRequest(err);

                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("移交项目({0})失败：", model.Id), HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        private Task<string> TransferProject(ProjectTransferModel model)
        {
            return Task.Run(() =>
            {
                var proj = _projectService.GetProjectById(model.Id);
                var vault = GetVault(proj);
                var isMgr = GetUserProjectManagerGroup(vault);
                if (isMgr == null) return "您的角色没有权限移交项目";
                var parties = _projectService.GetAllParties();
                var mgrName = GetUserName();
                var mgrGroups = _mfusergroupService.GetGroupsByUser(vault, mgrName);
                string partName = null;
                foreach (var p in parties)
                {
                    var pName = mgrGroups.FirstOrDefault(c => c.Name.StartsWith(p.Name));
                    if (pName != null)
                    {
                        partName = p.Name;
                        break;
                    }
                }
                if (partName == null)
                {
                    return "移交给的用户与您不在同一个参与方组";
                }
                var grps = _mfusergroupService.GetUserGroupsContainsString(vault, partName);
                var toUser = _userService.GetUserById(model.ToUserId);
                _mfusergroupService.RemoveUserFromGroup(vault, toUser.UserName, grps.Select(c => c.GroupId).ToArray());
                _mfusergroupService.RemoveUserFromGroup(vault, mgrName, grps.Select(c => c.GroupId).ToArray());
                _mfusergroupService.AddUserToGroup(vault, toUser.UserName,
                    grps.FirstOrDefault(c => c.Name.Contains(ProjectRoleConstants.ProjectManager)).GroupId);
                _mfusergroupService.AddUserToGroup(vault, mgrName,
                    grps.FirstOrDefault(c => c.Name.Contains(ProjectRoleConstants.ProjectMember)).GroupId);
                return String.Empty;
            });
        }

        public static string UpdateProject(ProjectEditModel model, string userName, IProjectService _projectService, IMFilesVaultService _mfvaultService
            , IMfUserGroupService _mfusergroupService, IVaultTemplateService _vaultTemplateService, IMFObjectService _mfilesObjService)
        {
            var proj = _projectService.GetProjectById(model.Id);
            if (proj == null) return "项目指定错误";
            var vault = GetVault(_mfvaultService, proj);
            var isManager = IsCreateProjectManager(vault, userName, _mfusergroupService, _vaultTemplateService);
            if (!isManager) return "只有项目创建者才可以修改项目信息";
            VaultTemplate template = null;
            if (model.TemplateId > 0)
            {
                template = GetTemplateByTempId(_vaultTemplateService, model.TemplateId);
                if (template != null && model.TemplateId != proj.TemplateId)
                {
                    proj.TemplateId = model.TemplateId;
                }
            }
            if (template == null)
            {
                template = GetTemplateByTempId(_vaultTemplateService, proj.TemplateId);
            }
            if (model.StatusId > 0)
            {
                var status = _projectService.GetStatus(model.StatusId);
                if (status != null && model.StatusId != proj.StatusId)
                {
                    proj.StatusId = model.StatusId;
                }
            }
            //1. 按照需要填充对应的项目字段信息
            //var needChangeVaultName = false;
            //var originalProjName = proj.Name;
            if (!String.IsNullOrEmpty(model.Name))
            {
                //if (model.Name.ToUpper() != originalProjName.ToUpper())
                //{
                //    var projs = _projectMemberService.GetProjectsByUser(GetUserId());
                //    var conflict =
                //        projs.Select(c => _projectService.GetProjectById(c.ProjectId))
                //            .Any(c => StringComparer.OrdinalIgnoreCase.Equals(model.Name, c.Name));
                //    if (conflict)
                //    {
                //        return "指定的新的名称与现有的项目名称相同！";
                //    }
                //    needChangeVaultName = true;
                //}
                proj.Name = model.Name;
            }
            proj.Number = model.Number;
            if (model.Cover != null && model.Cover.Length > 0) proj.Cover = model.Cover;
            proj.Description = model.Description;

            proj.OwnerUnit = model.OwnerUnit;
            proj.ConstructionUnit = model.ConstructionUnit;
            proj.DesignUnit = model.DesignUnit;
            proj.SupervisionUnit = model.SupervisionUnit;

            proj.StartDateUtc = model.StartDateUtc;
            proj.EndDateUtc = model.EndDateUtc;

            //2. 修改相应的库中的信息
            ////按需修改MFiles库的名称
            //if (needChangeVaultName)
            //{
            //    vault.Name = model.Name + "-" + ownerName;
            //    _vaultService.ChangeVaultName(vault);
            //}
            try
            {
                var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
                ToLocalTimeProj(proj);
                UpdateMfObj(vault, aliases, proj, _mfilesObjService); //更新M-Files库中的项目对象属性
                ToUtcTimeProj(proj);
            }
            catch (Exception ex)
            {
                Log.Error("更新库中的项目对象失败：" + ex.Message, ex);
                throw;
            }

            //3. 更新数据库信息
            _projectService.UpdateProject(proj);

            return String.Empty;
        }


        #region ProjectStatus
        /// <summary>
        /// 修改项目状态为立项
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Proposal(int id)
        {
            try
            {
                var err = await ChangeProjectStatus(id, ProjectStatusConstants.CreateProjectId); //启动
                if (!String.IsNullOrEmpty(err)) return BadRequest(err);
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("启动项目失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 修改项目状态为启动
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Start(int id)
        {
            try
            {
                var err = await ChangeProjectStatus(id, ProjectStatusConstants.StartProjectId); //启动
                if (!String.IsNullOrEmpty(err)) return BadRequest(err);
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("启动项目失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 修改项目状态为暂停
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Pause(int id)
        {
            try
            {
                var err = await ChangeProjectStatus(id, ProjectStatusConstants.PauseProjectId); //暂停
                if (!String.IsNullOrEmpty(err)) return BadRequest(err);
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("暂停项目失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 修改项目状态为结束
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> End(int id)
        {
            try
            {
                var err = await ChangeProjectStatus(id, ProjectStatusConstants.EndProjectId); //结束
                if (!String.IsNullOrEmpty(err)) return BadRequest(err);
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("终止项目失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        private Task<string> ChangeProjectStatus(int projId, int statusId)
        {
            return Task.Run(() =>
            {
                var proj = _projectService.GetProjectById(projId);
                if (proj.StatusId == statusId) return String.Empty;
                //var vault = GetVault(proj);
                var ok = proj.OwnerId == GetUserId();//IsCreateProjectManager(vault, GetUserName(), _mfusergroupService, _vaultTemplateService);
                if (!ok) return "您没有权限修改项目的状态！";
                proj.StatusId = statusId;
                _projectService.UpdateProject(proj);
                return String.Empty;
            });
        }

        public static string ChangeProjectStatus(int projId, int statusId, string userName, long userId, IProjectService _projectService, IMFilesVaultService _mfvaultService
            , IMfUserGroupService userGroupService, IVaultTemplateService vaultTempService)
        {
            var proj = _projectService.GetProjectById(projId);
            if (proj.StatusId == statusId) return String.Empty;
            var vault = GetVault(_mfvaultService, proj);
            var ok = proj.OwnerId == userId;//IsCreateProjectManager(vault, userName, userGroupService, vaultTempService);
            if (!ok) return "您没有权限修改项目的状态！";
            proj.StatusId = statusId;
            _projectService.UpdateProject(proj);
            return String.Empty;
        }

        #endregion ProjectStatus


    }
}
