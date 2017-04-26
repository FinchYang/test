using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;
using AecCloud.MfilesServices;
using AecCloud.Service;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.ApiRequests;
using DBWorld.AecCloud.Web.Models;
using log4net;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Api
{
    [Authorize]
    public class ProjectMembersController : ProjectBaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProjectMemberService _projectMemberService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IMFUserService _vaultUserService;
        private readonly IUserVaultService _uservaultService;
        //private readonly IMFilesVaultService _mfvaultService;
        //private readonly IUserCloudService _userCloudService;

        public ProjectMembersController(IProjectService projectService, IProjectMemberService projMemberService, IUserService userService,
            IEmailService emailService, IUserVaultService uservaultService, IMFUserService mfuserService,
            IMfUserGroupService mfgroupService, IMFilesVaultService mfilesvaultService,
            IVaultTemplateService vaultTemplateService, IMFObjectService mfilesObjService, IAuthenticationManager authManager)
            : base(projectService, mfilesvaultService, mfgroupService, vaultTemplateService, mfilesObjService, authManager) //, IUserCloudService userCloudService
        {
            _projectMemberService = projMemberService;
            _userService = userService;
            _emailService = emailService;
            //_mfvaultService = mfilesvaultService;
            _uservaultService = uservaultService;
            _vaultUserService = mfuserService;

            //_userCloudService = userCloudService;
        }

        //private IAuthenticationManager Authentication { get; set; }

        private static void SetEmailInfo(IEmailService _emailService)
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var host = appSettings["emailhost"];
            var email = appSettings["email"];
            var emailUserName = appSettings["emailusername"];
            var emailPassword = appSettings["emailpassword"];
            var emailDisplayName = appSettings["emaildisplayname"];
            _emailService.SetAccountEmailInfo(host, email, emailPassword, emailUserName, emailDisplayName);
            _emailService.SetInvitationEmailInfo(host, email, emailPassword, emailUserName, emailDisplayName);
            _emailService.SetPasswordEmailInfo(host, email, emailPassword, emailUserName, emailDisplayName);
        }

        /// <summary>
        /// 获得共同参与项目的所有联系人
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Contacts()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            var userId = GetUserId();
            try
            {
                var users = await Task.Run(() =>
                {
                    var projects = _projectMemberService.GetProjectsByUser(userId);
                    var users1 = new List<User>();
                    foreach (var p in projects)
                    {
                        var members = _projectMemberService.GetMembersInProject(p.ProjectId);
                        foreach (var m in members)
                        {
                            if (users1.Any(c => c.Id == m.UserId)) continue;
                            var user = _userService.GetUserById(m.UserId);
                            users1.Add(user);
                        }
                    }
                    return users1;
                }
                    );
                return Ok(users.Select(c => c.ToDto()).ToList());
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取联系人失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        /// <summary>
        /// 获取项目中的参与人员列表
        /// 状态码：正常=>OK；异常=>其他
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> InProject(int id)
        {
            try
            {
                var projUsers = await Task.Run(() =>
                GetProjectUsers(id, _projectMemberService, _userService));
                return Ok(projUsers);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取项目的用户失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static List<UserDto> GetProjectUsers(int projId, IProjectMemberService _projectMemberService, IUserService _userService)
        {
            var users = new List<User>();
            //1. 根据项目成员表获取指定项目与用户的关联列表
            var pms = _projectMemberService.GetMembersInProject(projId);
            //2. 根据关联列表中的用户ID获取用户信息
            foreach (var p in pms)
            {
                var user = _userService.GetUserById(p.UserId);
                users.Add(user);
            }
            return users.Select(c => c.ToDto()).ToList();
        }
        public static List<UserDto> GetProjectUsersDisplay(int projId, IProjectMemberService _projectMemberService, IUserService _userService)
        {
            var users = new List<User>();
            //1. 根据项目成员表获取指定项目与用户的关联列表
            var pms = _projectMemberService.GetMembersInProject(projId);
            //2. 根据关联列表中的用户ID获取用户信息
            foreach (var p in pms)
            {
                if (!p.Display) continue;
                var user = _userService.GetUserById(p.UserId);
                users.Add(user);
            }
            return users.Select(c => c.ToDto()).ToList();
        }


        #region ProjectMembers

        [HttpGet]
        public async Task<IHttpActionResult> MFUserId(int projId, int userId)
        {
            var mfUserId = await Task.Run(() =>
            {
                var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                var user = _userService.GetUserById(userId);
                var userName = user.UserName;//_vaultUserService.GetAccountName(userId, vault);
                return _vaultUserService.GetUserId(userName, vault);
            });
            if (mfUserId.HasValue)
            {
                return Ok(mfUserId.Value);
            }
            return NotFound();
        }

        public static int? GetMFUserId(int projId, int userId, IUserService userService, IMFUserService vaultUserService
            , IProjectService projService, IMFilesVaultService _mfvaultService)
        {
            var vault = GetVaultFromProject(projId, projService, _mfvaultService);
            var user = userService.GetUserById(userId);
            var userName = user.UserName;//_vaultUserService.GetAccountName(userId, vault);
            return vaultUserService.GetUserId(userName, vault);
        }

        public static string SendInvitationEmail(SendEmailMessage model, IEmailService _emailService)
        {
            try
            {
                SetEmailInfo(_emailService);
                _emailService.SendInviteMemberEmail(model.ToSendingModel());
                return String.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("发送邮件失败：" + ex.Message, ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// 发送邀请邮件
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IHttpActionResult> SendInvitationEmail(SendEmailMessage model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var environment = Request.GetOwinEnvironment();
            var isLocal = false;
            if (environment.ContainsKey("server.IsLocal"))
            {
                isLocal = (bool)environment["server.IsLocal"];
            }
            if (!isLocal && !User.Identity.IsAuthenticated) return BadRequest("必须登录！");
            var err = await Task.Run(() => SendInvitationEmail(model, _emailService));
            if (String.IsNullOrEmpty(err)) return Ok();
            return BadRequest("发送邀请邮件失败：" + err);
        }

        public static List<ProjectInvitationDto> GetInvitations(long userId, IProjectMemberService _projectMemberService, IProjectService _projectService)
        {
            var projMembers = _projectMemberService.GetProjectsByUser(userId);
            var inviteList = new List<ProjectInvitation>();
            foreach (var p in projMembers)
            {
                var proj = _projectService.GetProjectById(p.ProjectId);
                if (proj.PartyId > 0) //有参与方的项目
                {
                    inviteList.AddRange(_projectMemberService.GetInvitationsByInviter(proj.Id, userId)
                        .Where(c => !c.Accepted));
                }
                else //无参与方的项目
                {
                    var creatorId = proj.OwnerId;
                    if (creatorId != userId) continue;
                    inviteList.AddRange(_projectMemberService.GetInvitationsByInviter(p.ProjectId,
                            userId)
                            .Where(c => !c.Accepted));
                }
            }
            return inviteList.Select(c => c.ToDto()).ToList();
        }

        [HttpGet]
        public async Task<IHttpActionResult> Invitations()
        {
            var userId = GetUserId();
            try
            {
                var invitations = await
                    Task.Run(() => GetInvitations(userId, _projectMemberService, _projectService));
                return Ok(invitations);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("获取邀请列表失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 判断是否允许邀请:
        /// 1.邀请人必须是项目经理；
        /// 2.创建方的项目经理可以邀请其他参与方
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="inviteePartId"></param>
        /// <param name="invitee">被邀请人</param>
        /// <returns>若不为空，则为错误信息</returns>
        private static string InviteOK(long projectId, long inviteePartId, MetadataAliases aliases, int currentUserId, string userName, IProjectService _projectService
            , IMFilesVaultService _mfvaultService, IMFObjectService _mfobjService, IMfUserGroupService _mfusergroupService, User invitee = null)
        {
            //var userName = GetUserName();

            Project proj = _projectService.GetProjectById(projectId);
            MFilesVault vault = null;
            try
            {
                vault = GetVault(_mfvaultService, proj);
            }
            catch (Exception ex)
            {
                var err = String.Format("项目({0})对应的文档库不存在: {1}", proj.Name, ex.Message);
                Log.Error(err, ex);
                return err;
            }
            //var inviteePart = _projectService.GetPartyById(inviteePartId);
            //if (inviteePart != null)
            //{
            //    try
            //    {
            //        var mfParty = GetMfParty(vault, aliases, inviteePart.Name, _mfobjService, currentUserId);


            //        if (!mfParty.IsCurrentManager && !mfParty.IsCurrentMember && !mfParty.IsCurrentViceManager) //不同参与方
            //        {
            //            var party = _projectService.GetPartyById(proj.PartyId);
            //            try
            //            {

            //                var userPart = GetMfParty(vault, aliases, party.Name, _mfobjService, currentUserId);
            //                if (!userPart.IsCurrentManager)
            //                {
            //                    return "必须是项目创建方的项目经理才能邀请其他参与方的成员"; //必须是项目创建方的项目经理
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                var err = String.Format("获取项目({0})中创建方({1})失败: {2}", proj.Name, party.Name, ex.Message);
            //                Log.Error(err, ex);
            //                return err;
            //            }
            //            if (mfParty.ManagerCount > 0)
            //            {
            //                return String.Format("{0}已经指定了项目经理！", inviteePart.Name);
            //            }
            //        }
            //        else if (!mfParty.IsCurrentManager)
            //        {
            //            return "必须是邀请方的项目经理才能邀请其成员"; //必须是项目经理
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        var err = String.Format("获取项目({0})中邀请方({1})失败: {2}", proj.Name, inviteePart.Name, ex.Message);
            //        Log.Error(err, ex);
            //        return err;
            //    }
            //}
            //else //无参与方
            //{
                
            //}
            if (invitee != null)
            {
                ICollection<MFilesUserGroup> iGroups = null;
                try
                {
                    iGroups = _mfusergroupService.GetGroupsByUser(vault, invitee.UserName); //被邀请人所在的参与组
                }
                catch (Exception ex)
                {
                    var err = String.Format("项目({0})中被邀请成员({1})的用户组获取失败: {2}", proj.Name, userName, ex.Message);
                    Log.Error(err, ex);
                    return err;
                }
                if (iGroups.Count > 0) return "此用户已在当前项目中";
            }
            return String.Empty;
        }

        public static string InviteMemberByEmailAnonymous(ProjectInvitationEmailModel model, IUserService _userService
            , IProjectMemberService _projectMemberService, IProjectService _projectService)
        {
            var projectId = model.ProjectId;
            var inviterId = model.InviterId;
            var inviter = _userService.GetUserById(inviterId);
            var inviteeEmail = model.InviteeEmail;
            Log.Info("按照邮件邀请开始： " + inviter.Email + " # " + inviteeEmail);
            if (StringComparer.OrdinalIgnoreCase.Equals(inviter.Email, inviteeEmail))
            {
                return "不能邀请自己！";
            }
            var inviteePartId = model.InviteePartId;
            var invitationMessage = model.InvitationMessage;
            Log.Info("获取邀请...");
            var invitations = _projectMemberService.GetInvitations(projectId, inviterId, inviteeEmail);
            //Log.Info("获取第一个邀请...");
            var invitation = invitations.FirstOrDefault(c => c.InviteeId > 0);
            if (invitation != null)
            {
                Log.Info("获取项目成员： " + invitation.InviteeEmail);
                var member = _projectMemberService.GetMember(projectId, invitation.InviteeId);
                if (member != null)
                {
                    return "被邀请的用户已经是该项目的成员！";
                }
            }
            else
            {
                invitation = invitations.FirstOrDefault(c => !c.Accepted);
                if (invitation != null)
                {
                    Log.Info("获取项目成员(已在)： " + invitation.InviteeEmail);
                    if (inviteePartId > 0)
                    {
                        return "被邀请用户已经在邀请过程中，参与方为：" + _projectService.GetPartyById(inviteePartId);
                    }
                    return "被邀请用户已经在邀请过程中";
                }
            }
            _projectMemberService.AddInvitationByEmail(
                projectId, inviterId, inviteeEmail, invitationMessage, inviteePartId, model.BidProjId);
            return String.Empty;

        }
        /// <summary>
        /// 通过邮件邀请项目成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IHttpActionResult> InviteMemberByEmailAnonymous(ProjectInvitationEmailModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.InviterId <= 0)
            {
                ModelState.AddModelError("InviterId", "必须指定发送人");
                return BadRequest(ModelState);
            }
            var projectId = model.ProjectId;
            var inviterId = model.InviterId;
            var inviteeEmail = model.InviteeEmail;
            try
            {
                var errInfo =
                    await Task.Run(() => InviteMemberByEmailAnonymous(model, _userService, _projectMemberService
                        , _projectService));
        
                if (!String.IsNullOrEmpty(errInfo))
                {
                    Log.ErrorFormat("根据邮件({0})邀请项目({1})成员失败：{2}", inviteeEmail, projectId, errInfo);
                    return BadRequest(errInfo);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("根据邮件({0})邀请项目({1})成员出现异常：{2}", inviteeEmail, projectId, ex.Message);
                return CreateErrorResponse(
                    String.Format("根据邮件({0})邀请项目({1})成员失败：", inviteeEmail, projectId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static string InviteMembersOK(InviteModel model, long inviterId, IProjectService _projectService,
            IVaultTemplateService _vaultTempService, IMFilesVaultService _mfvaultService
            , IMFObjectService _mfobjService, IMfUserGroupService _mfusergroupService)
        {
            if (model.MFUserId <= 0) return "必须指定项目中的用户ID";
            var projectId = model.ProjectId;

            //var proj = _projectService.GetProjectById(projectId);
            //var template = GetTemplateByTempId(_vaultTempService, proj.TemplateId);
            MetadataAliases aliases = null;//JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            var errInfo = InviteOK(projectId, model.PartyId, aliases, model.MFUserId, "", _projectService, _mfvaultService, _mfobjService, _mfusergroupService);

            if (!String.IsNullOrEmpty(errInfo))
            {
                return errInfo;
            }
            return String.Empty;
        }
        public static string InviteMemberByEmail(ProjectInvitationEmailModel model, long inviterId, string userName,
            IUserService _userService, IProjectMemberService _projectMemberService
            , IProjectService _projectService, IVaultTemplateService _vaultTempService, IMFilesVaultService _mfvaultService
            , IMFObjectService _mfobjService, IMfUserGroupService _mfusergroupService)
        {
            if (model.MFUserId <= 0) return "必须指定项目中的用户ID";
            var projectId = model.ProjectId;
            var inviter = _userService.GetUserById(inviterId);
            var inviteeEmail = model.InviteeEmail;

            Log.Info("按照邮件邀请开始： " + inviter.Email + " # " + inviteeEmail);
            if (StringComparer.OrdinalIgnoreCase.Equals(inviter.Email, inviteeEmail))
            {
                return "不能邀请自己！";
            }

            var inviteePartId = model.InviteePartId;
            var invitationMessage = model.InvitationMessage;

            Log.Info("获取邀请...");
            var invitations = _projectMemberService.GetInvitations(projectId, inviterId, inviteeEmail);
            //Log.Info("获取第一个邀请...");
            var invitation = invitations.FirstOrDefault(c => c.InviteeId > 0);
            if (invitation != null)
            {
                Log.Info("获取项目成员： " + invitation.InviteeEmail);
                var member = _projectMemberService.GetMember(projectId, invitation.InviteeId);
                if (member != null)
                {
                    return "被邀请的用户已经是该项目的成员！";
                }
            }
            else
            {
                invitation = invitations.FirstOrDefault(c => !c.Accepted);
                if (invitation != null)
                {
                    Log.Info("获取项目成员(已在)： " + invitation.InviteeEmail);
                    if (inviteePartId > 0)
                    {
                        return "被邀请用户已经在邀请过程中，参与方为：" + _projectService.GetPartyById(inviteePartId);
                    }
                    return "被邀请用户已经在邀请过程中";
                }
            }
            //if (inviteePartId == 0) return String.Empty;
            var proj = _projectService.GetProjectById(projectId);
            var template = GetTemplateByTempId(_vaultTempService, proj.TemplateId);
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            var invitee = _userService.GetUserByEmail(inviteeEmail);
            var errInfo = InviteOK(projectId, model.InviteePartId, aliases, model.MFUserId, userName, _projectService, _mfvaultService,_mfobjService, _mfusergroupService, invitee);


            if (!String.IsNullOrEmpty(errInfo))
            {
                Log.ErrorFormat("根据邮件({0})邀请项目({1})成员失败：{2}", inviteeEmail, projectId, errInfo);
                return errInfo;
            }
            _projectMemberService.AddInvitationByEmail(
                projectId, inviterId, inviteeEmail, invitationMessage, inviteePartId, model.BidProjId);
            return String.Empty;
        }
        /// <summary>
        /// 通过邮件邀请项目成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> InviteMemberByEmail(ProjectInvitationEmailModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var inviterId = GetUserId();


            if (model.MFUserId <= 0) return BadRequest("必须指定项目中的用户ID");
            var projectId = model.ProjectId;
            
            var inviter = _userService.GetUserById(inviterId);
            var inviteeEmail = model.InviteeEmail;
            Log.Info("按照邮件邀请开始： " + inviter.Email + " # " + inviteeEmail);
            if (StringComparer.OrdinalIgnoreCase.Equals(inviter.Email, inviteeEmail))
            {
                return BadRequest("不能邀请自己！");
            }
            var inviteePartId = model.InviteePartId;
            var invitationMessage = model.InvitationMessage;
            try
            {
                var errInfo = await Task.Run(() =>
                {
                    Log.Info("获取邀请...");
                    var invitations = _projectMemberService.GetInvitations(projectId, inviterId, inviteeEmail);
                    //Log.Info("获取第一个邀请...");
                    var invitation = invitations.FirstOrDefault(c => c.InviteeId > 0);
                    if (invitation != null)
                    {
                        Log.Info("获取项目成员： " + invitation.InviteeEmail);
                        var member = _projectMemberService.GetMember(projectId, invitation.InviteeId);
                        if (member != null)
                        {
                            return "被邀请的用户已经是该项目的成员！";
                        }
                    }
                    else
                    {
                        invitation = invitations.FirstOrDefault(c => !c.Accepted);
                        if (invitation != null)
                        {
                            Log.Info("获取项目成员(已在)： " + invitation.InviteeEmail);
                            if (inviteePartId > 0)
                            {
                                return "被邀请用户已经在邀请过程中，参与方为：" + _projectService.GetPartyById(inviteePartId);
                            }
                            return "被邀请用户已经在邀请过程中";
                        }
                    }
                    if (inviteePartId == 0) return String.Empty;
                    var proj = _projectService.GetProjectById(projectId);
                    var template = GetTemplateByTempId(proj.TemplateId);
                    var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
                    var invitee = _userService.GetUserByEmail(inviteeEmail);
                    return InviteOK(projectId, model.InviteePartId, aliases, model.MFUserId, GetUserName(), _projectService, _mfvaultService, _mfilesObjService, _mfusergroupService, invitee);
                });
                if (!String.IsNullOrEmpty(errInfo))
                {
                    Log.ErrorFormat("根据邮件({0})邀请项目({1})成员失败：{2}", inviteeEmail, projectId, errInfo);
                    return BadRequest(errInfo);
                }
                await Task.Run(() => _projectMemberService.AddInvitationByEmail(
                    projectId, inviterId, inviteeEmail, invitationMessage, inviteePartId, model.BidProjId));
                return Ok();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("根据邮件({0})邀请项目({1})成员出现异常：{2}", inviteeEmail, projectId, ex.Message);
                return CreateErrorResponse(
                    String.Format("根据邮件({0})邀请项目({1})成员失败：", inviteeEmail, projectId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static string InviteMemberByUserId(ProjectInvitationUserModel model, long userId, string userName, IProjectService _projectService
            , IProjectMemberService _projectMemberService, IVaultTemplateService _vaultTemplateService,
            IMFilesVaultService _mfvaultService, IUserService _userService
            , IMFObjectService _mfilesObjService, IMfUserGroupService _mfusergroupService)
        {
            var projectId = model.ProjectId;
            var inviteeId = model.InviteeId;
            int inviteePartId = model.InviteePartId;
            var invitationMessage = model.InvitationMessage;

            var proj = _projectService.GetProjectById(projectId);
            var template = GetTemplateByTempId(_vaultTemplateService, proj.TemplateId);
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);

            if (inviteePartId > 0)
            {
                var invitee = _userService.GetUserById(inviteeId);
                return InviteOK(projectId, model.InviteePartId, aliases, model.MFUserId, userName, _projectService, _mfvaultService, _mfilesObjService, _mfusergroupService, invitee);
            }

            _projectMemberService.AddInvitationByInviteeId(
                projectId, userId, inviteeId, invitationMessage, inviteePartId);

            return String.Empty;
        }

        /// <summary>
        /// 通过用户ID邀请项目成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> InviteMemberByUserId(ProjectInvitationUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var inviterId = GetUserId();
            var userName = GetUserName();
            var projectId = model.ProjectId;
            var inviteeId = model.InviteeId;
            
            try
            {
                var errInfo = await Task.Run(() => InviteMemberByUserId(model, inviterId, userName, _projectService, _projectMemberService
                    , _vaultTemplateService, _mfvaultService, _userService, _mfilesObjService, _mfusergroupService));
                if (!String.IsNullOrEmpty(errInfo))
                {
                    Log.ErrorFormat("根据用户({0})邀请项目({1})成员失败：{2}", inviteeId, projectId, errInfo);
                    return BadRequest(errInfo);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    String.Format("根据用户({0})邀请项目({1})成员失败：", inviteeId, projectId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 被邀请人确认接受邀请
        /// 状态码：正常 => OK；未找到 => NotFound; 异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> InviteeConfirmInvitation(ProjectInvitationConfirmEmailModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var inviteeId = GetUserId();
            try
            {
                var res = await Task.Run(() => InviteeConfirmInvitation(model, inviteeId, _projectMemberService));
                if (!String.IsNullOrEmpty(res))
                {
                    return CreateResponse(HttpStatusCode.NotFound, res); //NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("被邀请人确认邀请失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        public static string InviteeConfirmInvitation(ProjectInvitationConfirmEmailModel model, long inviteeId,
            IProjectMemberService _projectMemberService)
        {
            var projectId = model.ProjectId;
            var inviterId = model.InviterId;
            var inviteeEmail = model.InviteeEmail;
            var inviteePartId = model.InviteePartId;
            ProjectInvitation invitation = null;
            if (String.IsNullOrEmpty(inviteeEmail))
            {
                invitation = _projectMemberService.GetInvitation(projectId, inviterId, inviteeId, inviteePartId);
            }
            else
            {
                invitation = _projectMemberService.GetInvitation(projectId, inviterId, inviteeEmail, inviteePartId);
            }
            if (invitation == null)
            {
                return "未找到此邀请！"; //NotFound();
            }
            invitation.InviteeId = inviteeId;
            invitation.InviteeConfirmMessage = model.ConfirmMessage;
            _projectMemberService.UpdateInvitation(invitation);
            return String.Empty;
        }

        public static string AcceptInvitationConfirmOneStep(int projectId, long inviteeId,int inviteePartId , long inviterId, IProjectMemberService projectMemberService
            , IProjectService projectService, IUserService userService, IVaultTemplateService vaultTemplateService, IMFilesVaultService mfvaultService
            , IMFUserService vaultUserService, IMFObjectService mfilesObjService, IUserVaultService uservaultService)
        {
            var proj = projectService.GetProjectById(projectId);
            var template = GetTemplateByTempId(vaultTemplateService, proj.TemplateId);
        //    MetadataAliases aliases = null;//JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);

            var vault = GetVaultFromProject(projectId, projectService, mfvaultService);

            var invitee = userService.GetUserById(inviteeId);

         //   ProjectParty inviteePart = null;
         //   MFProjectParty mfPart = null;
            //if (inviteePartId > 0)
            //{
            //    inviteePart = projectService.GetPartyById(inviteePartId);
            //    mfPart = GetMfParty(vault, aliases, inviteePart.Name, mfilesObjService);
            //}

            int vaultUserId = -1;
            try
            {
                vaultUserId = vaultUserService.CreateVaultUser(invitee, vault);
            }
            catch (Exception ex)
            {
                var err = "添加到DBWorld库失败：" + ex.Message;
                Log.Error(err, ex);
                return err;
            }
            var contactInternalId = 0;
            //MfContact mfContact = null;
            //try
            //{
            //    mfContact = new MfContact { User = invitee, Id = vaultUserId };

            //    if (inviteePart != null)
            //    {
            //        mfContact.PartName = inviteePart.Name;
            //        mfContact.RoleAlias = mfPart.ManagerCount > 0 ? "UGroupMember" : "UGroupPM";
            //    }
            //    else
            //    {
            //        mfContact.RoleAlias = "UGroupMember";
            //    }

            //    CreateMfObj(vault, aliases, mfContact, mfilesObjService);
            //}
            //catch (Exception ex)
            //{
            //    var err = "添加到DBWorld项目联系人失败：" + ex.Message;
            //    Log.Error(err, ex);
            //    return err;
            //}
            //if (mfContact != null)
            //{
            //    contactInternalId = mfContact.InternalId;
                
            //}

            var hasUser = uservaultService.UserHasVault(inviteeId, vault.Id);
            if (!hasUser) uservaultService.AddUserVault(inviteeId, vault.Id);
            projectMemberService.AddMember(projectId, inviteeId, contactInternalId);

            return String.Empty;
        }

        public static string AcceptInvitationConfirm(ProjectInvitationConfirmAcceptModel model, long inviterId, IProjectMemberService _projectMemberService
            , IProjectService _projectService, IUserService _userService, IVaultTemplateService _vaultTemplateService, IMFilesVaultService _mfvaultService
            , IMFUserService _vaultUserService, IMFObjectService _mfilesObjService, IUserVaultService _uservaultService)
        {
            var projectId = model.ProjectId;
            var inviteeId = model.InviteeId;
            var inviteePartId = model.InviteePartId;

            ProjectInvitation invitation = null;

            if (inviteePartId <= 0) //没有参与方的处理
            {
                invitation = _projectMemberService.GetInvitations(projectId, inviteeId).LastOrDefault();
            }
            else
            {
                invitation = _projectMemberService.GetInvitation(projectId, inviterId, inviteeId, inviteePartId);
            }
            if (invitation == null)
            {
                var message = String.Format("未找到此邀请！项目ID={0}， 被邀请用户ID={1}", model.ProjectId, inviteeId);
                Log.Warn(message);
                return message;
            }
            if (invitation.Accepted)
            {
                var message = String.Format("此邀请已被确认过！项目ID={0}， 被邀请用户ID={1}", invitation.ProjectId, inviteeId);
                Log.Warn(message);
                return message;
            }

            var proj = _projectService.GetProjectById(projectId);
            var template = GetTemplateByTempId(_vaultTemplateService, proj.TemplateId);
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);

            var vault = GetVaultFromProject(projectId, _projectService, _mfvaultService);

            var invitee = _userService.GetUserById(inviteeId);

            ProjectParty inviteePart = null;
            MFProjectParty mfPart = null;
            if (invitation.InviteePartId > 0)
            {
                inviteePart = _projectService.GetPartyById(invitation.InviteePartId);
                mfPart = GetMfParty(vault, aliases, inviteePart.Name, _mfilesObjService);
            }

            int vaultUserId = -1;
            try
            {
                //if (string.IsNullOrEmpty(invitee.Domain) && AuthUtility.IsDomainEnabled())
                //{
                //    invitee.Domain = DomainClient.Domain;
                //}
                vaultUserId = _vaultUserService.CreateVaultUser(invitee, vault);
            }
            catch (Exception ex)
            {
                var err = "添加到DBWorld库失败：" + ex.Message;
                Log.Error(err, ex);
                return err;
            }
            MfContact mfContact = null;
            try
            {
                mfContact = new MfContact { User = invitee, Id = vaultUserId };
                var isContractor = invitation.BidProjectId > 0;
                if (isContractor)
                {
                    mfContact.RoleAlias = "UGroupOutsource";
                }
                else
                {
                    if (inviteePart != null)
                    {
                        mfContact.PartName = inviteePart.Name;
                        if (mfPart.ManagerCount > 0)
                        {
                            mfContact.RoleAlias = "UGroupMember";
                        }
                        else
                        {
                            mfContact.RoleAlias = "UGroupPM";
                        }
                    }
                    else
                    {
                        mfContact.RoleAlias = "UGroupMember";
                    }
                }

                CreateMfObj(vault, aliases, mfContact, _mfilesObjService);
                //_mfusergroupService.AddUserToGroup(vault, invitee.UserName, groupName);
            }
            catch (Exception ex)
            {
                var err = "添加到DBWorld项目联系人失败：" + ex.Message;
                Log.Error(err, ex);
                return err;
            }

            var hasUser = _uservaultService.UserHasVault(inviteeId, vault.Id);
            if (!hasUser) _uservaultService.AddUserVault(inviteeId, vault.Id);
            _projectMemberService.AddMember(projectId, inviteeId, mfContact.InternalId);
            //if (!String.IsNullOrEmpty(err))
            //{
            //    Log.Warn("确认添加成员失败：" + err);
            //    return BadRequest(err);
            //}
            invitation.Accepted = true;
            invitation.AcceptedBy = inviterId;
            _projectMemberService.UpdateInvitation(invitation);

            return String.Empty;
        }
        /// <summary>
        /// 确认接受邀请的人进入项目
        /// 状态码：正常 => OK；已确认 => NotModified; 异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> AcceptInvitationConfirm(ProjectInvitationConfirmAcceptModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var inviterId = GetUserId();
            //var inviterName = GetUserName();

            var projectId = model.ProjectId;
            var inviteeId = model.InviteeId;
            var inviteePartId = model.InviteePartId;
            
            try
            {
                var invitation =
                    await Task.Run(() =>
                    {
                        if (inviteePartId <= 0) //没有参与方的处理
                        {
                            return _projectMemberService.GetInvitations(projectId, inviteeId).LastOrDefault();
                        }
                        else
                        {
                           return _projectMemberService.GetInvitation(projectId, inviterId, inviteeId, inviteePartId);
                        }
                    });
                if (invitation == null)
                {
                    var message = String.Format("未找到此邀请！项目ID={0}， 被邀请用户ID={1}", model.ProjectId, inviteeId);
                    Log.Warn(message);
                    return CreateResponse(HttpStatusCode.NotFound, message);// NotFound();
                }
                if (invitation.Accepted)
                {
                    var message = String.Format("此邀请已被确认过！项目ID={0}， 被邀请用户ID={1}", invitation.ProjectId, inviteeId);
                    Log.Warn(message);
                    return CreateResponse(HttpStatusCode.NotModified, message);// StatusCode(HttpStatusCode.NotModified);
                }

                var err = await Task.Run(() =>
                {
                    var proj = _projectService.GetProjectById(projectId);
                    var template = GetTemplateByTempId(_vaultTemplateService, proj.TemplateId);
                    var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);

                    var vault = GetVaultFromProject(projectId, _projectService, _mfvaultService);

                    var invitee = _userService.GetUserById(inviteeId);

                    ProjectParty inviteePart = null;
                    MFProjectParty mfPart = null;
                    if (invitation.InviteePartId > 0)
                    {
                        inviteePart = _projectService.GetPartyById(invitation.InviteePartId);
                        mfPart = GetMfParty(vault, aliases, inviteePart.Name, _mfilesObjService);
                    }

                    int vaultUserId = -1;
                    try
                    {
                        if (string.IsNullOrEmpty(invitee.Domain) && invitee.DomainUser)
                        {
                            invitee.Domain = DomainClient.Domain;
                        }
                        vaultUserId = _vaultUserService.CreateVaultUser(invitee, vault);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("添加到DBWorld库失败：" + ex.Message);
                        throw;
                    }
                    MfContact mfContact = null;
                    try
                    {
                        mfContact = new MfContact { User = invitee, Id = vaultUserId };
                        var isContractor = invitation.BidProjectId > 0;
                        if (isContractor)
                        {
                            mfContact.RoleAlias = "UGroupOutsource";
                        }
                        else
                        {
                            if (inviteePart != null)
                            {
                                mfContact.PartName = inviteePart.Name;
                                if (mfPart.ManagerCount > 0)
                                {
                                    mfContact.RoleAlias = "UGroupMember";
                                }
                                else
                                {
                                    mfContact.RoleAlias = "UGroupPM";
                                }
                            }
                            else
                            {
                                mfContact.RoleAlias = "UGroupMember";
                            }
                        }

                        CreateMfObj(vault, aliases, mfContact, _mfilesObjService);
                        //_mfusergroupService.AddUserToGroup(vault, invitee.UserName, groupName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("添加到DBWorld项目联系人失败：" + ex.Message, ex);
                        throw;
                    }

                    var hasUser = _uservaultService.UserHasVault(inviteeId, vault.Id);
                    if (!hasUser) _uservaultService.AddUserVault(inviteeId, vault.Id);
                    _projectMemberService.AddMember(projectId, inviteeId, mfContact.InternalId);

                    //向数据库添加用户的模板列表(云列表), Added by bert
                    //AddUserCloud(inviteeId, 1);
                    //AddUserCloud(inviteeId, vault.CloudId);

                    return String.Empty;
                });
                if (!String.IsNullOrEmpty(err))
                {
                    Log.Warn("确认添加成员失败：" + err);
                    return BadRequest(err);
                }
                invitation.Accepted = true;
                invitation.AcceptedBy = inviterId;
                _projectMemberService.UpdateInvitation(invitation);
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("邀请者确认邀请人信息失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 删除项目成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> RemoveMember(ProjectMemberModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var member = await Task.Run(() => _projectMemberService.GetMemberByContactId(model.ProjectId, model.ContactId));
                if (member == null)
                {
                    return CreateResponse(HttpStatusCode.NotFound, "未找到成员");
                }
                return await Task.Run<IHttpActionResult>(() =>
                {
                    var proj = _projectService.GetProjectById(model.ProjectId);
                    var vault = GetVault(proj);
                    var template = GetTemplateByTempId(proj.TemplateId);
                    var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
                    var removeContact = GetMfContact(vault, aliases, model.ContactId);
                    if (!String.IsNullOrEmpty(removeContact.PartName))
                    {
                        var mfParty = GetMfParty(vault, aliases, removeContact.PartName, _mfilesObjService, model.MFUserId);

                        if (!mfParty.IsCurrentManager)
                        {
                            Log.ErrorFormat("必须是删除方的项目经理才有权限删除成员！参与方({0}), 执行人的用户ID({1})", removeContact.PartName,
                                model.MFUserId);
                            return BadRequest("必须是删除方的项目经理才有权限删除成员！");
                        }
                    }
                    var thisContact = GetMfContactByUserId(vault, aliases, model.MFUserId);
                    if (!String.IsNullOrEmpty(removeContact.PartName))
                    {
                        if (thisContact.PartName != removeContact.PartName)
                        {
                            return BadRequest("只有本参与方的项目经理才能删除成员");
                        }
                    }
                    if (thisContact.RoleName != ProjectRoleConstants.ProjectManager)
                    {
                        return BadRequest("只有项目经理才能删除成员！");
                    }
                    try
                    {
                        var mfContact = new MfContact { InternalId = model.ContactId, Disabled = true, IsCreator = false };
                        UpdateMfObj(vault, aliases, mfContact, _mfilesObjService);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("禁用联系人出错：" + ex.Message, ex);
                        throw;
                    }
                    try
                    {
                        _vaultUserService.DisableVaultUser(removeContact.UserId, vault); //model.UserName
                    }
                    catch (Exception ex)
                    {
                        Log.Error("禁用用户出错：" + ex.Message, ex);
                        throw;
                    }
                    _projectMemberService.RemoveMemberByContactId(model.ProjectId, model.ContactId);
                    return Ok();
                });
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("删除项目成员失败：", HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }

        #endregion ProjectMembers


        #region UserGroups

        private static MFilesVault GetVaultFromProject(long projId, IProjectService _projectService, IMFilesVaultService _mfvaultService)
        {
            var project = _projectService.GetProjectById(projId);
            if (project == null)
            {
                throw new AecException("未能获取到项目，项目ID:" + projId);
            }
            return _mfvaultService.GetVaultById(project.VaultId);
        }
        /// <summary>
        /// 根据项目获取用户组
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> UserGroups(int id)
        {
            var projId = id;
            try
            {
                var groups = await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    return _mfusergroupService.GetUserGroups(vault, false).Select(c => c.ToDto()).ToList();
                });
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("获取项目({0})的用户组失败：", projId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 根据用户组ID获取项目中用户组信息
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GroupInProject(int projId, int groupId)
        {
            try
            {
                var group = await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    return _mfusergroupService.GetUserGroupById(vault, groupId).ToDto();
                });
                return Ok(group);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("获取项目({0})的用户组{1}失败：", projId, groupId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 在项目中添加用户组
        /// 状态码：正常 => Created； 未找到 => NotModified; 异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> AddUserGroup(ProjectCreateUserGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var projId = model.ProjectId;
            try
            {
                var group0 = await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    var group = new MFilesUserGroup { Name = model.GroupName };
                    var g = _mfusergroupService.GetUserGroupByName(vault, model.GroupName);
                    if (g != null) return null;
                    _mfusergroupService.AddUserGroupToVault(vault, group);
                    return group;
                });
                if (group0 == null)
                {
                    return StatusCode(HttpStatusCode.NotModified);
                }
                var requestHost = GetHost();
                return Created(requestHost + "/Project/GroupInProject/" + projId + "/" + group0.Id, group0);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("添加项目({0})的用户组({1})失败：", projId, model.GroupName),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 删除项目中的用户组
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> RemoveUserGroup(ProjectRemoveUserGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var projId = model.ProjectId;
            var groupId = model.GroupId;
            try
            {
                await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    _mfusergroupService.RemoveUserGroupById(vault, groupId);
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("删除项目({0})中的用户组({1})失败：", projId, groupId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }

        }
        /// <summary>
        /// 获取指定用户组中所有用户名
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetUsernamesInGroup(int projId, int groupId)
        {
            try
            {
                var users = await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    return _mfusergroupService.GetUsersInGroup(vault, groupId);
                });
                return Ok(users);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(String.Format("获取项目({0})的用户组({1})成员失败：", projId, groupId),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }

        }
        /// <summary>
        /// 是否有删除权限
        /// 项目经理有权限删除本方的成员
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="party"></param>
        /// <returns></returns>
        private string RemoveOK(Project proj, ProjectParty party, MetadataAliases aliases, int? currentUserId = null)
        {
            var vault = GetVault(proj);
            
            var userName = GetUserName();
            var groups = _mfusergroupService.GetGroupsByUser(vault, userName);//当前用户的参与组
            var g = groups.FirstOrDefault(c => c.Name.Contains(ProjectRoleConstants.ProjectManager));
            if (g == null)
            {
                return "您没有足够的权限删除成员";
            }
            var mfParty = GetMfParty(vault, aliases, party.Name, _mfilesObjService, currentUserId);
            var g0 = groups.FirstOrDefault(c => c.Name.Contains(party.Name));
            if (g0 == null)
            {
                return "不能删除其他参与方的成员，请联系其经理";
            }

            return String.Empty;
        }
        /// <summary>
        /// 从参与方中删除成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> RemoveUserFromParty(ProjectRemoveUserFromPartyModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var projId = model.ProjectId;
            var partyId = model.PartyId;
            var userName = model.UserName;
            var proj = _projectService.GetProjectById(projId);
            var template = GetTemplateByTempId(proj.TemplateId);
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            var party = _projectService.GetPartyById(partyId);
            var err = await Task.Run(() => RemoveOK(proj, party, aliases, model.MFUserId));

            if (!String.IsNullOrEmpty(err)) return BadRequest(err);

            try
            {
                await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    var gIds = _mfusergroupService.GetGroupsByUser(vault, userName).Where(c => c.Name.Contains(party.Name)).Select(c => c.GroupId).ToArray();
                    _mfusergroupService.RemoveUserFromGroup(vault, userName, gIds);
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    String.Format("删除项目({0})中参与方({1})成员({2})失败：", projId, party.Name, userName),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 从用户组中删除成员
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> RemoveUsersFromGroup(ProjectRemoveUserFromGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var projId = model.ProjectId;
            var groupId = model.GroupId;
            var userName = model.UserName;
            if (userName == GetUserName())
            {
                return BadRequest("不能删除自己");
            }
            try
            {
                var err = await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    var group = _mfusergroupService.GetUserGroupById(vault, groupId);
                    var partName = group.Name.Substring(0, group.Name.IndexOf('-'));
                    var groups = _mfusergroupService.GetGroupsByUser(vault, GetUserName());
                    var mgrgrpName = partName + "-" + ProjectRoleConstants.ProjectManager;
                    if (groups.All(c => c.Name != mgrgrpName))
                    {
                        return "只有本参与方的项目经理才能删除本方的成员";
                    }
                    _mfusergroupService.RemoveUserFromGroup(vault, userName, groupId);
                    return String.Empty;
                });
                if (!String.IsNullOrEmpty(err))
                {
                    Log.ErrorFormat("删除项目({0})中用户组({1})成员({2})失败：{3}", projId, groupId, userName, err);
                    return BadRequest(err);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    String.Format("删除项目({0})中用户组({1})成员({2})失败：", projId, groupId, userName),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 添加成员到用户组
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> AddUserToGroup(ProjectAddUserToGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var projId = model.ProjectId;
            var groupId = model.GroupId;
            var userName = model.UserName;

            try
            {
                await Task.Run(() =>
                {
                    var vault = GetVaultFromProject(projId, _projectService, _mfvaultService);
                    _mfusergroupService.AddUserToGroup(vault, userName, groupId);
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    String.Format("添加项目({0})中用户组({1})成员({2})失败：", projId, groupId, userName),
                    HttpStatusCode.ServiceUnavailable, ex, Log);
            }
        }
        /// <summary>
        /// 获取参与方的用户组列表
        /// 状态码：正常 => OK；异常 => 其他
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="partyId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> UserGroupsFromParty([FromUri]int projId, int partyId)
        {
            var groups = await Task.Run(() =>
            {
                var proj = _projectService.GetProjectById(projId);
                var vault = GetVault(proj);
                var party = _projectService.GetPartyById(partyId);
                var partyName = party.Name;
                return _mfusergroupService.GetUserGroupsContainsString(vault, partyName).Select(c => c.ToDto()).ToList();
            }
                );
            return Ok(groups);
        }

        #endregion UserGroups
    }
}
