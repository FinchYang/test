using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using AecCloud.Core.Domain;
using AecCloud.MfilesServices;
using AecCloud.Service;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.Models;
using log4net;
using Microsoft.AspNet.Identity;

namespace DBWorld.AecCloud.Web.Api
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InviteController : BaseApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UserManager<User, long> _userManger;
        private readonly IUserService _userService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IProjectService _projectService;
        private readonly IVaultTemplateService _vaultTempService;
        private readonly IMFilesVaultService _mfvaultService;
        private readonly IMFObjectService _mfobjService;
        private readonly IMfUserGroupService _mfusergroupService;
        private readonly IEmailService _emailService;
        public InviteController(UserManager<User, long> userManager, IUserService userService, IProjectMemberService projectMemberService
            , IProjectService projectService, IVaultTemplateService vaultTempService, IMFilesVaultService mfvaultService
            , IMFObjectService mfobjService, IMfUserGroupService mfusergroupService, IEmailService emailService)
        {
            _userManger = userManager;
            _userService = userService;
            _projectMemberService = projectMemberService;
            _projectService = projectService;
            _vaultTempService = vaultTempService;
            _mfvaultService = mfvaultService;
            _mfobjService = mfobjService;
            _mfusergroupService = mfusergroupService;
            _emailService = emailService;
        }

        public static string Invite(InviteModel inviteModel, UserManager<User, long> _userManger, long userId, string userName
            , IUserService _userService, IProjectMemberService _projectMemberService, IProjectService _projectService
            , IVaultTemplateService _vaultTempService, IMFilesVaultService _mfvaultService, IMFObjectService _mfobjService
            , IMfUserGroupService _mfusergroupService, IEmailService _emailService, string baseUri)
        {
            if (String.IsNullOrEmpty(inviteModel.Email))
            {
                var userDto = AccountController.GetUserProfile(_userManger, userId);//UserClient.GetProfile(baseUri, token);
                //if (!userRes.Success)
                //{
                //    return BadRequest("当前用户认证失效");
                //}
                //var userDto = JsonConvert.DeserializeObject<UserDto>(userRes.Content);
                inviteModel.Email = userDto.Email;
            }
            //Log.Info("API获取ProjectClient...");
            if (inviteModel.InviteEmail != "")//分email和用户ID两种记录方式
            {
                //Log.Info("API生成BindingModel...");
                var model = new ProjectInvitationEmailModel
                {
                    ProjectId = inviteModel.ProjectId,
                    InviteeEmail = inviteModel.InviteEmail,
                    InvitationMessage = "请加入" + inviteModel.ProjectName + "项目",
                    InviteePartId = inviteModel.PartyId,
                    MFUserId = inviteModel.MFUserId,
                    BidProjId = inviteModel.BidProjId
                };
                //Log.Info("API添加数据库信息...");
                var res = Api.ProjectMembersController.InviteMemberByEmail(model, userId, userName
                    , _userService, _projectMemberService, _projectService, _vaultTempService
                    , _mfvaultService, _mfobjService, _mfusergroupService);
                if (String.IsNullOrEmpty(res))
                {
                    //Log.Info("API添加数据库信息成功...");
                    var sendEmailError = SendInviteEmail(baseUri, inviteModel.PartyName, inviteModel.InviteEmail,
                        inviteModel.Email, inviteModel.ProjectName, inviteModel.ProjectId, inviteModel.UserId,
                        inviteModel.PartyId, _emailService);
                    if (sendEmailError == "success")
                    {
                        return String.Empty;
                    }
                    else
                    {
                        return sendEmailError;
                    }
                }
                else
                {
                    var err = res;
                    Log.Error("API添加数据库信息失败：" + err);
                    return err;
                }
            }
            else
            {
                var model = new ProjectInvitationUserModel
                {
                    ProjectId = inviteModel.ProjectId,
                    InviteeId = inviteModel.UserId,
                    InvitationMessage = "请加入" + inviteModel.ProjectName + "项目",
                    InviteePartId = inviteModel.PartyId
                };
                var res = Api.ProjectMembersController.InviteMemberByUserId(model, userId, userName,
                    _projectService, _projectMemberService, _vaultTempService
                    , _mfvaultService, _userService, _mfobjService, _mfusergroupService);
                if (String.IsNullOrEmpty(res))
                {
                    var sendEmailError = SendInviteEmail(baseUri, inviteModel.PartyName, inviteModel.InviteEmail,
                        inviteModel.Email, inviteModel.ProjectName, inviteModel.ProjectId, inviteModel.UserId,
                        inviteModel.PartyId, _emailService);
                    if (sendEmailError == "success")
                    {
                        return String.Empty;
                    }
                    else
                    {
                        return sendEmailError;
                    }
                }
                else
                {
                    var err = res;
                    return err;
                }
            }
        }

        public IHttpActionResult Post(InviteModel inviteModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var baseUri = GetHost();

            var err = Invite(inviteModel, _userManger, GetUserId(), GetUserName(), _userService, _projectMemberService
                , _projectService, _vaultTempService, _mfvaultService, _mfobjService, _mfusergroupService, _emailService,
                baseUri);

            if (String.IsNullOrEmpty(err)) return Ok("success");
            return BadRequest(err);
            
        }

        internal static string SendInviteEmailAnomymous(string host, string partyName, string inviteEmail, string email, 
            string projectName, long projectId, long userId, long partyId, IEmailService _emailService)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(inviteEmail, email))
            {
                return "不能邀请自己！";
            }

            //string url = System.Configuration.ConfigurationManager.AppSettings["WebAPI"].ToString();
            var message = GenerateMessage(host, partyName, inviteEmail, email,
                projectName, projectId, userId, partyId);
            //调用api，通过api发送邮件给邀请人
            //Log.Info("API开始发送邀请邮件...");
            var sendEmailRes = Api.ProjectMembersController.SendInvitationEmail(message, _emailService);//await ProjectClient.SendInvitationEmailAnonymous(host, message);
            if (String.IsNullOrEmpty(sendEmailRes))
            {
                return "success";
            }
            return sendEmailRes;
        }

        private static SendEmailMessage GenerateMessage(string host, string partyName, string inviteEmail, string email,
            string projectName, long projectId, long userId, long partyId)
        {
            var url = host + "/Account/LoginForInvite?projectName=" + EncipherAndDecrypt.EncryptText(projectName)
                + "&projectId=" + EncipherAndDecrypt.EncryptText(projectId.ToString())
                + "&userId=" + EncipherAndDecrypt.EncryptText(userId.ToString())
                + "&inviteEmail=" + EncipherAndDecrypt.EncryptText(inviteEmail)
                + "&email=" + EncipherAndDecrypt.EncryptText(email)
                + "&partyId=" + EncipherAndDecrypt.EncryptText(partyId.ToString());

            var partyStr = String.Empty;
            if (!String.IsNullOrEmpty(partyName))
            {
                partyStr = "作为“" + partyName + "”";
            }
            //var ssoUrl = AuthUtility.GetSSORegisterUri();
            //var dbworldHomeUrl = AuthUtility.GetHomeHost().TrimEnd('/');
            string temp = "<p>尊敬的用户，您好！</p>"
                    + "<p>有人通过【DBWorld】邀请您，" + partyStr + "参与到“" + projectName + "”中，点击下方地址申请加入该项目，或者将以下地址直接复制到地址栏回车</p>"
                    + "<a href='" + HttpUtility.HtmlEncode(url) + "'>" + HttpUtility.HtmlEncode(url) + "</a>"
                    + "<p>（如无法打开链接，请复制上面的链接粘贴到浏览器地址栏完成邀请。）</p>"
                    //+ "<p>DBWorld工程云客户端下载地址：<a href='" + dbworldHomeUrl + "/intro/product/" + "'>客户端下载</a></p>"
                    //+ "<p>DBWorld工程云账号注册地址：<a href='" + ssoUrl + "'>注册账户</a></p>"
                    + "<p>感谢您对DBWorld工程云的关注！DBWorld祝您一切顺利！</p>"
                    + "<p>来自：DBWorld工程云【DBWorld】</p>";
            temp = temp.Replace("+", "%2B");//+号会解析为空所以这里转换下

            var message = new SendEmailMessage
            {
                MailTo = inviteEmail,
                Title = "请加入" + projectName + "项目",
                Body = temp,
                IsHtml = true
            };
            return message;
        }

        internal static string SendInviteEmail(string host, string partyName, string inviteEmail, string email, 
            string projectName, long projectId, long userId, long partyId, IEmailService _emailService)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(inviteEmail, email))
            {
                return "不能邀请自己！";
            }
            
            var message = GenerateMessage(host, partyName, inviteEmail, email,
                projectName, projectId, userId, partyId);
            //调用api，通过api发送邮件给邀请人
            //Log.Info("API开始发送邀请邮件...");
            var sendEmailRes = Api.ProjectMembersController.SendInvitationEmail(message, _emailService);//await ProjectClient.SendInvitationEmail(host, token, message);
            if (String.IsNullOrEmpty(sendEmailRes))
            {
                return "success";
            }
            return sendEmailRes;
        }

        public static string SendEmail2Invitee(string host, string partyName, string inviteeEmail,
             string projectName, string inviterName, IEmailService emailService)
        {
            var url = host;
            var partyStr = String.Empty;
            if (!String.IsNullOrEmpty(partyName))
            {
                partyStr = "作为“" + partyName + "”";
            }
            string temp = "<p>尊敬的用户，您好！</p>"
                    + "<p>" + inviterName + "已通过【DBWorld】邀请您，" + partyStr + "参与到“" + projectName + "”中，请点击下方地址及时查看。</p>"
                    + "<a href='" + HttpUtility.HtmlEncode(url) + "'>" + HttpUtility.HtmlEncode(url) + "</a>"
                    + "<p>（如无法打开链接，请复制上面的链接粘贴到浏览器地址栏。）</p>"
                    + "<p>感谢您对DBWorld工程云的关注！DBWorld祝您一切顺利！</p>"
                    + "<p>来自：DBWorld工程云【DBWorld】</p>";
            temp = temp.Replace("+", "%2B");//+号会解析为空所以这里转换下

            var message = new SendEmailMessage
            {
                MailTo = inviteeEmail,
                Title = "您已被邀请到" + projectName + "项目",
                Body = temp,
                IsHtml = true
            };
            //Log.Info("API开始发送邀请邮件...");
            var sendEmailRes = ProjectMembersController.SendInvitationEmail(message, emailService);
            if (String.IsNullOrEmpty(sendEmailRes))
            {
                return string.Empty;
            }
            return sendEmailRes;
        }
    }
}
