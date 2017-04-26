using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.MfilesServices;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using log4net;
using Microsoft.AspNet.Identity;

namespace DBWorld.AecCloud.Web
{
    public static class AuthUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region CreateUser
        public static UserCreateModel CreateUser(UserManager<User, long> userManager, IUserService userService, string userName, string email, bool isDomainUser, string domain="",string fullName="")
        {
            var user = new User
            {
                UserName = userName,
                Email = email,
                CreatedTimeUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                Domain = domain,
                DomainUser= isDomainUser,
                FullName = string.IsNullOrEmpty(fullName) ? userName : fullName,
                CompanyId = 1,
                DepartmentId = 1
            };

            var cRole = userService.GetDefaultRole();
            if (cRole != null)
            {
                user.CscecRoleId = cRole.Id;
            }

            var cate = userService.GetDefaultCategory();
            if (cate != null)
            {
                user.PersonnelCategoryId = cate.Id;
            }

            var pos = userService.GetDefaultPosition();
            if (pos != null)
            {
                user.PositionInfoId = pos.Id;
            }

            if (isDomainUser && String.IsNullOrEmpty(domain))
            {
                return new UserCreateModel
                {
                    Error = "invalid_creation",
                    ErrorDescription = "域账户未指定域名"
                };
            }

            try
            {
                var res = userManager.Create(user);
                if (res.Succeeded)
                {
                    //await SendEmailAsync(host, email);
                    return new UserCreateModel {User = user};
                }
                var errDesc = "未知错误";
                if (res.Errors != null)
                {
                    var errs = res.Errors.ToArray();
                    if (errs.Length > 0)
                    {
                        errDesc = String.Join("; ", errs);
                    }
                }
                return new UserCreateModel
                {
                    Error = "invalid_creation",
                    ErrorDescription = errDesc
                };

            }
            catch (Exception ex)
            {
                var ex0 = ex;
                if (ex.InnerException != null)
                {
                    ex0 = ex.InnerException;
                }
                Log.Error("创建用户异常：" + ex0.Message, ex0);
                return new UserCreateModel
                {
                    Error = "unknown_error",
                    ErrorDescription = String.Join("; ", ex0.Message)
                };
            }
        }
        

        #endregion CreateUser

        #region UserAuthentication

        /// <summary>
        /// 获取用户密码
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUserPassword(IPrincipal user)
        {
            var userId = user.Identity.GetUserId();
            return DBWorldCache.Get(userId);
        }

        public static bool IsAuthenticated(IPrincipal user)
        {
            return IsAuthenticated(user.Identity);
        }

        internal static bool IsAuthenticated(IIdentity userIdentity)
        {
            return userIdentity.IsAuthenticated;
        }

        public static string GetUserName(IPrincipal user)
        {
            return user.Identity.GetUserName();
        }

        public static string GetFullname(IPrincipal user)
        {
            var id = user.Identity as ClaimsIdentity;
            if (id != null)
            {
                var fn = id.Claims.FirstOrDefault(c => c.Type == AecUserManager.FullnameClaim);
                if (fn != null) return fn.Value;
            }
            return GetUserName(user);
        }
        /// <summary>
        /// 获取用户的域名
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserDomain(IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            if (id != null)
            {
                var fn = id.Claims.FirstOrDefault(c => c.Type == AecUserManager.UserDomain);
                if (fn != null) return fn.Value;
            }
            return String.Empty;
        }

        public static long GetUserId(IPrincipal user)
        {
            return user.Identity.GetUserId<long>();
        }
        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string[] GetUserRoles(IIdentity user)
        {
            var ci = user as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            }
            return null;
        }

        #endregion UserAuthentication
        
        private static UserInfo Login(string userName, string password, bool isDomainUser, IVaultServerService serverServ, IMFUserService mfuserServ)
        {
            if (isDomainUser) return Login2ActiveDirectory(userName, password);
            var server = serverServ.GetServer();
            var ok = mfuserServ.ConnectToServer(userName, password, server, false);
            if (!ok) return null;
            return mfuserServ.GetUserInfo(server, userName);
        }
     
        /// <summary>
        /// 主登录的方法，根据UserType的配置来选择是MF用户登录，还是域账户登录，注：MF登录选择数据库中的服务器进行登录，默认选择第一个
        /// </summary>
        public static UserLoginModel Login(string userName, string password, bool isDomainUser, UserManager<User, long> userManager
            , IVaultServerService serverServ, IMFUserService mfuserServ, IUserService userService)
        {
            var model = new UserLoginModel();
            var userInfo = Login(userName, password, isDomainUser, serverServ, mfuserServ);
            if (userInfo == null)
            {
                model.Error = userName + ":" + "无效用户或密码！";
                return model;
            }
            Log.Info("用户登录信息：" + userInfo.Fullname + "; " + userInfo.Email);
            var user = userManager.FindByName(userName);
            if (user == null)
            {
                var domain = String.Empty;
                if (isDomainUser)
                {
                    domain = ConfigurationManager.AppSettings["Domain"] ?? String.Empty;
                }
                var userModel = CreateUser(userManager, userService, userName, userInfo.Email, isDomainUser, domain, userInfo.Fullname);
                if (userModel.User == null)
                {
                    model.Error = userModel.Error + ":" + userModel.ErrorDescription;
                    return model;
                }
                user = userModel.User;
            }
            if (user == null)
            {
                model.Error = userName + ": 未知错误！";
                return model;
            }
            var needUpdate = false;
            if (userInfo.Fullname != user.FullName)
            {
                user.FullName = userInfo.Fullname;
                needUpdate = true;
            }
            if (userInfo.Email != user.Email)
            {
                user.Email = userInfo.Email;
                needUpdate = true;
            }
            model.User = user;
            if (needUpdate)
            {
                Log.Info("更新用户信息：" + user.FullName + "; " + user.Email);
                userManager.Update(user);
            }
            return model;
        }


        #region 域认证

        private static readonly string LdapRoot = ConfigurationManager.AppSettings["LdapRoot"];
        private static readonly string DcInfo = ConfigurationManager.AppSettings["DcInfo"];

        private static UserInfo Login2ActiveDirectory(string username, string password)
        {
            var domainPath = GetDomainPath(LdapRoot, DcInfo, "", "");
            var fullName = String.Empty;
            var ui = new UserInfo { UserName = username };
            using (var entry = new DirectoryEntry(domainPath))
            {
                entry.Username = username;
                entry.Password = password;
                //http://www.cnblogs.com/flyinthesky/archive/2010/02/21/1670574.html
                var searcher = new DirectorySearcher(entry) { Filter = String.Format("(&(objectclass=user)(sAMAccountName={0}))", username) };
                try
                {
                    //http://stackoverflow.com/questions/23693209/how-to-get-all-the-users-details-from-active-directory-using-ldap
                    //http://www.kouti.com/tables/userattributes.htm
                    searcher.PropertiesToLoad.Add("sn");
                    searcher.PropertiesToLoad.Add("givenName");//first name
                    searcher.PropertiesToLoad.Add("mail");
                    searcher.PropertiesToLoad.Add("displayName");
                    SearchResult sr = searcher.FindOne();
                    var sde = sr.GetDirectoryEntry();
                    try
                    {
                        var fn = GetFirstValue(sde.Properties, "sn"); //surname, lastname
                        var ln = GetFirstValue(sde.Properties, "givenName");
                        fullName = fn + ln;
                        //Log.Info("用户姓名：" + fullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(String.Format("未能找到域账户({0})的姓名信息：{1}", username, ex.Message), ex);
                    }

                    if (String.IsNullOrEmpty(fullName))
                    {
                        try
                        {
                            ui.Fullname = GetFirstValue(sde.Properties, "displayName"); //sr.Properties["displayname"][0].ToString();
                            //Log.Info("用户显示名称：" + fullName);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn(String.Format("未能找到域账户({0})的显示名称信息：{1}", username, ex.Message), ex);
                        }
                    }

                    try
                    {
                        ui.Email = GetFirstValue(sde.Properties, "mail"); //sr.Properties["mail"][0].ToString();
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(String.Format("未能找到域账户({0})的邮箱信息：{1}", username, ex.Message), ex);
                    }
                    
                    
                    if (String.IsNullOrEmpty(fullName))
                    {
                        Log.Warn("未能找到用户姓名或全名：" + username);
                        fullName = username;
                    }
                    ui.Fullname = fullName;
                    if (ui.Email == null) ui.Email = String.Empty;
                    return ui;
                }
                catch (Exception e)
                {
                    Log.Error("搜索域异常："+e.Message, e);
                }
            }

            return null;
        }

        private static string GetFirstValue(PropertyCollection res, string key)
        {
            if (res.Contains(key))
            {
                var values = res[key];
                if (values.Count > 0 && values[0] != null)
                {
                    return values[0].ToString();
                }
                else
                {
                    Log.Warn("域中此字段无值：" + key);
                }
            }
            else
            {
                Log.Warn("域中不包含此字段：" + key);
            }
            return String.Empty;
        }
        private static string GetDomainPath(string ldapRoot, string dc, string ou, string cn)
        {
            ldapRoot = ldapRoot.ToUpper();
            if (ldapRoot[ldapRoot.Length - 1] != '/') ldapRoot = ldapRoot + "/";
            if (!String.IsNullOrEmpty(ou))
            {
                if (ou != string.Empty && ou[ou.Length - 1] != ',') ou = ou + ",";
            }
            if (cn != string.Empty && cn[cn.Length - 1] != ',') cn = cn + ",";
            return ldapRoot + cn + ou + dc;
        }


        #endregion

        

    }

    public class UserCreateModel
    {
        public User User { get; set; }

        public string Error { get; set; }

        public string ErrorDescription { get; set; }
    }

    public class UserLoginModel
    {
        public User User { get; set; }

        public string Error { get; set; }
    }

    
}