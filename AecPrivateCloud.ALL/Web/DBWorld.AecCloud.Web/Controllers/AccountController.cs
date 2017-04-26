using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using AecCloud.BaseCore;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.Models;
using log4net;
using MFilesAPI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserService _userService;
        //private readonly IUserCloudService _usercloudService;
        private readonly IUserVaultService _uservaultService;
        private readonly ICloudService _cloudService;
        private readonly IVaultTemplateService _vaultTemplateService;
        private readonly IProjectService _projService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IMFilesVaultService _mfvaultService;
        private readonly IVaultServerService _vaultserverService;
        private readonly IMFUserService _mfuserService;
        private readonly IMFVaultService _vaultService;
        private readonly IRepository<MFilesVault> _vaultRepository;
        public AccountController(UserManager<User, long> userManager, SignInManager<User, long> signInManager, IAuthenticationManager authManager, IUserService userService
            , IUserVaultService uservaultService, IRepository<MFilesVault> vaultRepository, IMFilesVaultService mfvaultService
            , ICloudService cloudService, IVaultTemplateService vaultTemplateService, IProjectService projService,  IProjectMemberService projectMemberService
            , IVaultServerService vaultserverService, IMFUserService mfuserService, IMFVaultService vaultService) //IUserCloudService usercloudService, 
            : base(authManager, signInManager, userManager)
        {
            _userService = userService;
            //_usercloudService = usercloudService;
            _uservaultService = uservaultService;
            _cloudService = cloudService;
            _vaultTemplateService = vaultTemplateService;
            _projService = projService;
            _projectMemberService = projectMemberService;
            _mfvaultService = mfvaultService;
            _vaultserverService = vaultserverService;
            _mfuserService = mfuserService;
            _vaultService = vaultService;
            _vaultRepository = vaultRepository;
        }

        // 登录
        [AllowAnonymous]
        public ActionResult LogOn(string returnUrl = null, string token = null)
        {
            
            if (returnUrl != null)
            {
                if (returnUrl.Contains('%'))
                {
                    try
                    {
                        returnUrl = Server.HtmlDecode(returnUrl);
                    }
                    catch {}
                }
            }

            if (AuthUtility.IsAuthenticated(User.Identity))
            {
                return RedirectToLocal(returnUrl);
            }
          
            if (!string.IsNullOrEmpty(token))
            {
                AuthenticationTicket ticket = IdentityAuth.GetTicketFromToken(token);
                if (ticket != null)
                {
                    var identity = ticket.Identity;
                    if (identity != null && identity.IsAuthenticated)
                    {
                        var userId = identity.GetUserId<long>();
                        var user = _userManager.FindById(userId);
                        if (user != null)
                        {
                            _signInManager.SignIn(user, false, false);
                          //  Writelog(string.Format("allowanonymous log: {0}",user.UserName));
                            return RedirectToLocal(returnUrl);
                        }
                    }
                }
            }
          
            return View();
        }
    

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return ReturnUserHome(); //RedirectToAction("Welcome", "Manage");
        }
        private void SimpleLog(string logtext)
        {
            Log.Info("SimpleLog"+logtext);
            //var tmpfile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"),"log", "AccountControllerLog.xml");
            //try
            //{
            //    using (var sw = System.IO.File.AppendText(tmpfile))
            //    {
            //        sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
            //        sw.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex.Message);
            //}
        }
        //通过页面提交方式登录
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> LogOn(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // 这不会计入到为执行帐户锁定而统计的登录失败次数中
            // 若要在多次输入错误密码的情况下触发帐户锁定，请更改为 shouldLockout: true

            var userRes = await Task.Run(() => AuthUtility.Login(model.UsernameOrEmail, model.Password, model.IsDomainUser,
                _userManager, _vaultserverService, _mfuserService, _userService));
            if (userRes.User != null)
            {
                var user = userRes.User;
             
                await _signInManager.SignInAsync(user, model.RememberMe, false);
                //   Log.InfoFormat("登录账户：{0}, 公司：{1}，部门：{2}", user.UserName, user.Company.Name, user.Department.Name);
                DBWorldCache.Add(user.Id.ToString(), model.Password);
                //check whether current user can connect contractor vault.
               try {
                    //分包商菜单处理，使用缓存
                    var thevault = _vaultRepository.Table.FirstOrDefault(c => c.CloudId == 3);
                    var app = MFServerUtility.ConnetToMfApp(user, model.Password, thevault.Server);
                    var vault = app.LogInToVault(thevault.Guid);
                    DBWorldCache.Add(user.Id.ToString() + "canManageContractor", "true");
                  //  SimpleLog(user.FullName + "can view contractors,id=" + user.Id);
               }
               catch (Exception) { }
                try
                {
                    var vs = _vaultserverService.GetServer();
                    var mfapp = MFServerUtility.ConnectToServer(vs);
                    var vaults = mfapp.GetOnlineVaults();
                    foreach (VaultOnServer vaultOnServer in vaults)
                    {
                      //  SimpleLog(vaultOnServer.Name + "  task check!");
                        Vault vault;
                        try
                        {
                            vault = vaultOnServer.LogIn();
                        }
                        catch (Exception )
                        {
                            SimpleLog(string.Format("Info: vault:{0},{1}", vaultOnServer.Name, "no right"));
                            continue;
                        }
                        try
                        {
                            var havetask = CheckTaskInOneVault(vault, user.Id);
                            if (havetask)
                            {
                                DBWorldCache.Add(user.Id.ToString() + "havetask", havetask.ToString());
                              //  SimpleLog(vault.Name + " havetask !");
                                break;
                            }
                          //  SimpleLog(vault.Name + " have no task !");
                        }
                        catch (Exception ex)
                        {
                            SimpleLog("havetask check error:" + ex.Message);
                        }
                    }
                    foreach (VaultOnServer vaultOnServer in vaults)
                    {
                      //  SimpleLog(vaultOnServer.Name + "  notice check!");
                        Vault vault;
                        try
                        {
                            vault = vaultOnServer.LogIn();
                        }
                        catch (Exception )
                        {
                            SimpleLog(string.Format("Info: vault:{0},{1}", vaultOnServer.Name, "no right"));
                            continue;
                        }
                        try
                        {
                            var havenotice = CheckNoticeInOneVault(vault,user.Id);
                           
                            if (havenotice)
                            {
                                DBWorldCache.Add(user.Id.ToString() + "havenotice", havenotice.ToString());
                                SimpleLog(vault.Name + " havenotice !");
                                break;
                            }
                          //  SimpleLog(vault.Name + " have no notice !");
                        }
                        catch (Exception ex)
                        {
                            SimpleLog("havenotice check error:" + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog("check whether current user can connect contractor vault." + ex.Message);
                }
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", userRes.Error);
            }

            return View(model);
            
        }

        private bool CheckNoticeInOneVault(Vault vault, long currentuserid)
        {
            //var currentuserid = long.Parse(User.Identity.GetUserId());
            var loginName = _userService.GetUserById(currentuserid).GetAccountName();
            var mfuserid = MfUserUtils.GetUserAccount(vault, loginName);
            Log.Info(string.Format("mfuserid:{0}", mfuserid));
            var ClassNotification =
                vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                    "ClassNotification");
            var pos = vault.Name.LastIndexOf('-');
            if (pos < 1) pos = vault.Name.Length;
            var tasktitle = vault.Name.Substring(0, pos);

            if (mfuserid != null)
            {
                var scs = new SearchConditions();
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = ClassNotification });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = (int)mfuserid });
                    scs.Add(-1, sc);
                }
                var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                  MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions.Count;
                return ovs > 0;
            }
            return false;
        }

        private bool CheckTaskInOneVault(MFilesAPI.Vault vault, long currentuserid)
        {
            try
            {
                var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                var mfuserid = MfUserUtils.GetUserAccount(vault, loginName);
                //var ClassTaskApprove =
                //    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                //        "ClassTaskApprove");
                var ClassNotification =
                      vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                          MfilesAliasConfig.ClassNotification);
                var pos = vault.Name.LastIndexOf('-');
                if (pos < 1) pos = vault.Name.Length;
              //  var tasktitle = vault.Name.Substring(0, pos) + "-";
                if (mfuserid != null)
                {
                    //{
                    //    var scs = new SearchConditions();
                    //    {
                    //        var sc = new SearchCondition();
                    //        sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    //        sc.Expression.DataPropertyValuePropertyDef =
                    //            (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    //        sc.TypedValue.SetValueToLookup(new Lookup
                    //        {
                    //            Item = (int) MFBuiltInObjectClass.MFBuiltInObjectClassGenericAssignment
                    //        });
                    //        scs.Add(-1, sc);
                    //    }
                    //    {
                    //        var sc = new SearchCondition();
                    //        sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    //        sc.Expression.DataPropertyValuePropertyDef =
                    //            (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                    //        sc.TypedValue.SetValueToLookup(new Lookup {Item = (int) mfuserid});
                    //        scs.Add(-1, sc);
                    //    }
                    //    var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                    //        MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions.Count;
                    //    if (ovs > 0) return true;
                    //}
                    {
                        var scs = new SearchConditions();
                        {
                            var sc = new SearchCondition();
                            sc.ConditionType = MFConditionType.MFConditionTypeNotEqual;
                            sc.Expression.DataPropertyValuePropertyDef =
                                (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                            sc.TypedValue.SetValueToLookup(new Lookup { Item = ClassNotification });
                            scs.Add(-1, sc);
                        }
                        {
                            var sc = new SearchCondition();
                            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                            sc.Expression.DataPropertyValuePropertyDef =
                                (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                            sc.TypedValue.SetValueToLookup(new Lookup {Item = (int) mfuserid});
                            scs.Add(-1, sc);
                        }
                        var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                            MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions.Count;
                        if (ovs > 0) return true;
                    }
//                    {
////工作流任务

//                        var scs = new SearchConditions();
//                        {
//                            var sc = new SearchCondition();
//                            sc.ConditionType = MFConditionType.MFConditionTypeNotEqual;
//                            sc.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);

//                            sc.TypedValue.SetValueToLookup(new Lookup
//                            {
//                                Item = (int) MFBuiltInObjectType.MFBuiltInObjectTypeAssignment
//                            });
//                            scs.Add(-1, sc);
//                        }
//                        {
//                            var sc = new SearchCondition();
//                            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
//                            sc.Expression.DataPropertyValuePropertyDef =
//                                (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
//                            sc.TypedValue.SetValueToLookup(new Lookup {Item = (int) mfuserid});
//                            scs.Add(-1, sc);
//                        }
//                        var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
//                            MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions.Count;
//                        if (ovs > 0) return true;
//                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("CheckTaskInOneVault {0},{1} error:{2}",vault.Name,currentuserid,ex.Message));
            }
            return false;
        }
        //
        // POST: 登出
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Signout(true);
            return ReturnUserHome(); //RedirectToAction("Index", "Home");
        }

        

        [HttpPost]
        //[AllowAnonymous]
        public ActionResult Invitelogin(int inviterId, int projectId)
        {
            string tempIp = Request.UserHostAddress;
            var loginStatusModel = new LoginStatusModel { Ip = tempIp, LoginDateUtc = DateTime.UtcNow };
            Api.AccountController.UpdateLoginInfo(loginStatusModel, AuthUtility.GetUserId(User),
                _userManager, _userService);

            var res = GetUserProfile(_userManager);
            var user = res;
            Session["UserId"] = user.Id;
            Session["Email"] = user.Email;

            var host = Utility.GetHost(Request.Url);
            var roles = AuthUtility.GetUserRoles(User.Identity);
            var userId = AuthUtility.GetUserId(User);
            var userEntity = _userManager.FindById(userId);
            //获取邀请人名字和项目内容
            var appModel = Api.CloudController.GetClouds(userEntity,
                _uservaultService, _cloudService, _vaultTemplateService, _vaultService, _mfvaultService, host, roles); //_usercloudService,
            Session["userApp"] = appModel;
            var inviterRes = GetUserProfile(_userManager, inviterId);
            var inviterProfile = inviterRes;
            var projectRes = Api.ProjectController.GetProject(projectId, _projService, _mfvaultService,
                _vaultTemplateService);
            var project = projectRes;
            var returnStr = "{'state':'success','inviter':'" + inviterProfile.UserName + "','projectName':'" + project.Name + "','projectDes':'" + project.Description + "'}";
            return Content(returnStr);
        }

        private static readonly string InviteQuery = "params";

        [AllowAnonymous]
        public ActionResult LoginForInvite()
        {
            var qs = Request.Url.Query.TrimStart('?');
            var qsBytes = Encoding.UTF8.GetBytes(qs);
            var sqEncode = Convert.ToBase64String(qsBytes);
            var returnUrl = "/Account/InviteLogOn?" + InviteQuery + "=" + sqEncode;
            returnUrl = Server.HtmlEncode(returnUrl); //host + 
            if (User.Identity.IsAuthenticated)
            {
                Signout(true);
                return Redirect(returnUrl);
            }
            return Redirect(returnUrl);
        }

        //项目邀请成员验证登陆视图
        public ViewResult InviteLogOn()
        {

            var qsDict = new Dictionary<string, string>(); //Request.QueryString
            var encodeStr = Request.QueryString[InviteQuery];

            //Log.Info("InviteQuery EncodedStr: " + encodeStr);

            var qsBytes = Convert.FromBase64String(encodeStr);
            var qsStr = Encoding.UTF8.GetString(qsBytes);

            //Log.Info("InviteQuery: " + qsStr);

            qsStr = qsStr.Replace("%2B", "+").Replace("%2b", "+");

            //Log.Info("InviteQuery Decoded: " + qsStr);

            var qsArray = qsStr.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in qsArray)
            {
                var index = s.IndexOf('=');
                var k = s.Substring(0, index);
                var v = s.Substring(index+1);
                qsDict.Add(k, v);
            }

            if (qsDict.ContainsKey("loginEmail") && qsDict["loginEmail"] != null)
            {
                ViewBag.loginEmail = qsDict["loginEmail"];
            }

            
            
            //解密出url参数中的参数
            //Log.Info("InviteQuery projectId: " + qsDict["projectId"]);
            int projectId = int.Parse(EncipherAndDecrypt.DecryptText(qsDict["projectId"]));
            //Log.Info("InviteQuery partyId: " + qsDict["partyId"]);
            int partyId = int.Parse(EncipherAndDecrypt.DecryptText(qsDict["partyId"]));
            //Log.Info("InviteQuery userId: " + qsDict["userId"]);
            int userId = int.Parse(EncipherAndDecrypt.DecryptText(qsDict["userId"]));
            //Log.Info("InviteQuery email: " + qsDict["email"]);
            string email = EncipherAndDecrypt.DecryptText(qsDict["email"]);
            //Log.Info("InviteQuery inviteEmail: " + qsDict["inviteEmail"]);
            string inviteEmail = EncipherAndDecrypt.DecryptText(qsDict["inviteEmail"]);
            ViewBag.email = email;
            ViewBag.inviteEmail = inviteEmail;
            var p = new ProjectLoginViewModel
            {
                ProjectId = projectId,
                UserId = userId,
                PartyId = partyId
            };
            return View(p);
        }


        [AllowAnonymous]
        public ViewResult Clause()
        {
            return View();
        }
    }
}