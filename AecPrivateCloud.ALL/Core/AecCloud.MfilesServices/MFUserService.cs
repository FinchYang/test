using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using log4net;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MFUserService : IMFUserService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void CreateMFilesLoginAccount(User user, VaultServer server)
        {
            var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd, MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
            try
            {
                var has = MfUserUtils.HasLoginAccount(app, GetAccountName(user));
                if (!has)
                {
                    var accType = MFLoginAccountType.MFLoginAccountTypeWindows;
                    if (string.IsNullOrEmpty(user.Domain)) accType = MFLoginAccountType.MFLoginAccountTypeMFiles;
                    CreateMFilesLoginAccount(user, app, accType,MFLicenseType.MFLicenseTypeConcurrentUserLicense);
                }
            }
            finally
            {
                app.Disconnect(); //todo 多线程时是否对其他会话有影响
            }
        }

        internal void CreateMFilesLoginAccount(User user, MFilesServerApplication app,
            MFLoginAccountType accountType, MFLicenseType licenseType)
        {
            var account = new LoginAccount();
            var fullName = user.FullName;
            if (String.IsNullOrEmpty(fullName))
            {
                fullName = user.UserName;
            }
            account.Set(accountType, user.Domain.ToUpper(), user.UserName,
                MFLoginServerRole.MFLoginServerRoleLogIn, fullName, user.Email, licenseType);
            app.LoginAccountOperations.AddLoginAccount(account);
        }
        /// <summary>
        /// 在指定的MFiles服务器中添加登陆账户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="server">MFiles Server对象 </param>
        internal void CreateMFilesLoginAccount(User user, VaultServer server,
            MFLoginAccountType accountType, MFLicenseType licenseType)
        {
            var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd,
                MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
            try
            {
                var accountName = GetAccountName(user);
                var hasAccount = MfUserUtils.HasLoginAccount(app, accountName);
                if (!hasAccount)
                {
                    var account = new LoginAccount();
                    var fullName = user.FullName;
                    if (String.IsNullOrEmpty(fullName))
                    {
                        fullName = user.UserName;
                    }
                    account.Set(accountType, user.Domain, user.UserName,
                        MFLoginServerRole.MFLoginServerRoleLogIn, fullName, user.Email, licenseType);
                    app.LoginAccountOperations.AddLoginAccount(account);
                    
                }
            }
            finally
            {
                app.Disconnect(); //todo 多线程时是否对其他会话有影响
            }
        }

        public static int CreateVaultUser(Vault mVault, User user)
        {
            var loginName = GetAccountName(user);
            var hasAccount = MfUserUtils.HasUserAccount(mVault, loginName);
            if (!hasAccount)
            {
                var account = new UserAccount
                {
                    LoginName = loginName,
                    InternalUser = true,
                    VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles
                };
                return mVault.UserOperations.AddUserAccount(account).ID;
            }
            var uId = MfUserUtils.EnableVaultUser(mVault, loginName);
            if (uId != null) return uId.Value;
            return -1;
        }
        /// <summary>
        /// 在指定的Vault中添加用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="vault">库对象</param>
        public int CreateVaultUser(User user, MFilesVault vault)
        {
            var server = vault.Server;
            var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd,
                MFServerUtility.GetVaultServerLocalIp(vault.Server), server.ServerPort);
            try
            {
                var mVault = app.LogInToVault(vault.Guid);

                return CreateVaultUser(mVault, user);
                
            }
            finally
            {
                app.Disconnect();
            }
        }

        private static string GetAccountName(User user)
        {
            if (user.DomainUser) return user.Domain + "\\" + user.UserName;
            return user.UserName;
        }

        public void ChangeVaultUserInfo(User user, VaultServer server)
        {
            var accountName = GetAccountName(user);
            var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd,
                MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
            try
            {
                var la = MfUserUtils.GetLoginAccount(app, accountName);
                if (la == null) return;
                var needChange = false;
                if (la.FullName != user.FullName)
                {
                    needChange = true;
                    la.FullName = user.FullName;
                }
                if (la.EmailAddress != user.Email)
                {
                    la.EmailAddress = user.Email;
                    needChange = true;
                }
                if (needChange)
                {
                    app.LoginAccountOperations.ModifyLoginAccount(la);
                }
            }
            finally
            {
                app.Disconnect();
            }
        }

        public void DisableVaultUser(string userName, MFilesVault vault)
        {
            var mVault = MFServerUtility.GetVault(vault);
            if (!HasVaultUser(userName, vault))
            {
                Log.Error(string.Format("用户({0})不存在于vault({1}),vaultname={2},中！", userName, vault.Guid, mVault.Name));
                return;
            }
            MfUserUtils.DisableVaultUser(mVault, userName);
        }

        public void DisableVaultUser(int userId, MFilesVault vault)
        {
            var mVault = MFServerUtility.GetVault(vault);
            MfUserUtils.DisableVaultUser(mVault, userId);
        }

        public bool HasVaultUser(string userName, MFilesVault vault)
        {
            var mVault = MFServerUtility.GetVault(vault);
            return MfUserUtils.HasUserAccount(mVault, userName);
        }

        public bool HasVaultUser(int userId, MFilesVault vault)
        {
            var mVault = MFServerUtility.GetVault(vault);
            return MfUserUtils.HasUserAccount(mVault, userId);
        }

        public void EnableVaultUser(string userName, MFilesVault vault)
        {
            var mVault = MFServerUtility.GetVault(vault);
            if (!HasVaultUser(userName, vault))
            {
                var err = string.Format("用户{0}不存在于vault{1},vaultname={2},中！", userName, vault.Guid, mVault.Name);
                Log.Error(err);
                throw new Exception(err);
            }
            MfUserUtils.EnableVaultUser(mVault, userName);
        }

        public int? GetUserId(string userName, MFilesVault vault)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return MfUserUtils.GetUserAccount(mfVault, userName);
        }

        public UserInfo GetUserInfo(VaultServer server, string userName)
        {
            var app = MFServerUtility.ConnectToServer(server);
            try
            {
                var la = MfUserUtils.GetLoginAccount(app, userName);
                if (la != null)
                {
                    if (String.IsNullOrEmpty(la.FullName)) la.FullName = userName;
                    return new UserInfo {UserName = userName, Email = la.EmailAddress, Fullname = la.FullName};
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("获取登录账户({0})异常：" + ex.Message, userName), ex);
            }
            finally
            {
                app.Disconnect();
            }
            
            return null;
        }

        public string GetAccountName(int userId, MFilesVault vault)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return MfUserUtils.GetUserNameWithoutDomain(mfVault, userId);
        }

        public bool ConnectToServer(string userName, string password, VaultServer server, bool windowsUser)
        {
            var app = new MFilesServerApplication();
            try
            {
                var authType = MFAuthType.MFAuthTypeSpecificMFilesUser;
                if (windowsUser) authType = MFAuthType.MFAuthTypeSpecificWindowsUser;
                var status = app.Connect(authType, userName, password, "", "ncacn_ip_tcp",
                    server.LocalIp, server.ServerPort);
                var ok = status == MFServerConnection.MFServerConnectionAuthenticated;
                try
                {
                    app.Disconnect();
                }
                catch
                {
                }
                return ok;
            }
            catch(Exception ex)
            {
                Log.Error("登录服务器失败：" + ex.Message, ex);
            }
            return false;
        }

        public IList<MfUser> GetMFilesLoginAccounts(VaultServer server)
        {
            var res = new List<MfUser>();
            var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd, MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
            try
            {
                var accs = MfUserUtils.GetLoginAccounts(app);
                res.AddRange(accs.Select(a => new MfUser
                                              {
                                                  AccountName = a.AccountName, 
                                                  Domain = a.DomainName,
                                                  UserName = a.UserName, 
                                                  Email = a.EmailAddress,
                                                  FullName = string.IsNullOrEmpty(a.FullName) ? a.UserName : a.FullName
                                              }));
            }
            finally
            {
                app.Disconnect(); //todo 多线程时是否对其他会话有影响
            }
            return res;
        }
    }

   
}
