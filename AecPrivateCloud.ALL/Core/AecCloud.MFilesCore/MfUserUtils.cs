using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public static class MfUserUtils
    {
        public static string GetUserNameWithoutDomain(string userName)
        {
            var index = userName.IndexOf('\\');
            return userName.Substring(index + 1);
        }

        public static int? EnableVaultUser(Vault gVault, string accountOrUserName)
        {
            var hasDomain = accountOrUserName.Contains('\\');
            accountOrUserName = accountOrUserName.ToUpper();
            var accounts = gVault.UserOperations.GetUserAccounts();
            if (hasDomain)
            {
                foreach (UserAccount ua in accounts)
                {
                    if (ua.LoginName.ToUpper() == accountOrUserName)
                    {
                        ua.AddVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleLogIn);
                        gVault.UserOperations.ModifyUserAccount(ua);
                        return ua.ID;
                    }
                }
            }
            else
            {
                foreach (UserAccount ua in accounts)
                {
                    if (GetUserNameWithoutDomain(ua.LoginName).ToUpper() == accountOrUserName)
                    {
                        ua.AddVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleLogIn);
                        gVault.UserOperations.ModifyUserAccount(ua);
                        return ua.ID;
                    }
                }
            }
            return null;
        }

        public static void DisableVaultUser(Vault gVault, int userId)
        {
            var ua = gVault.UserOperations.GetUserAccount(userId);
            ua.RemoveVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleLogIn);
            gVault.UserOperations.ModifyUserAccount(ua);
        }

        public static void DisableVaultUser(Vault gVault, string accountName)
        {
            accountName = accountName.ToUpper();
            var accounts = gVault.UserOperations.GetUserAccounts();
            foreach (UserAccount ua in accounts)
            {
                if (GetUserNameWithoutDomain(ua.LoginName).ToUpper() == accountName)
                {
                    ua.RemoveVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleLogIn);
                    gVault.UserOperations.ModifyUserAccount(ua);
                    break;
                }
            }
        }
        /// <summary>
        /// 判断库中是否存在指定的UserAccount
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static int? GetUserAccountByFullName(Vault vault, string fullName)
        {
            var keyNames = vault.UserOperations.GetUserList();
            var ua = keyNames.Cast<KeyNamePair>().FirstOrDefault(kn => kn.Name.ToUpper() == fullName.ToUpper());
            if (ua == null) return null;
            return ua.Key;
        }
        /// <summary>
        /// 获取库的登录账户ID
        /// </summary>
        /// <param name="gVault"></param>
        /// <param name="accountName">登录账户的名称(若为域账户，则带有域名)</param>
        /// <returns></returns>
        public static int? GetUserAccount(Vault gVault, string accountName)
        {
            accountName = accountName.ToUpper();
            var hasDomain = accountName.Contains('\\');
            var accounts = gVault.UserOperations.GetUserAccounts();
            UserAccount la = null;
            la = hasDomain ? accounts.OfType<UserAccount>().FirstOrDefault(c => c.LoginName.ToUpper() == accountName)
                : accounts.OfType<UserAccount>().FirstOrDefault(c => GetUserNameWithoutDomain(c.LoginName).ToUpper() == accountName);
            if (la == null) return null;
            return la.ID;
        }

        public static bool HasUserAccount(Vault gVault, int userId)
        {
            try
            {
                var uas = gVault.UserOperations.GetLoginAccountOfUser(userId);
                return uas != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasUserAccount(Vault gVault, string accountName)
        {
            return GetUserAccount(gVault, accountName) != null;
        }
        /// <summary>
        /// 获取MF服务器的登录账户
        /// </summary>
        /// <param name="app"></param>
        /// <param name="accountName">用户名(若为Windows账户，需要包含域名)</param>
        /// <returns></returns>
        public static LoginAccount GetLoginAccount(MFilesServerApplication app, string accountName)
        {
            try
            {
                var la = app.LoginAccountOperations.GetLoginAccount(accountName);
                return la;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 判断LoginAccount是否已存在
        /// </summary>
        /// <param name="app"></param>
        /// <param name="accountName">用户名</param>
        /// <returns></returns>
        public static bool HasLoginAccount(MFilesServerApplication app, string accountName)
        {
            return GetLoginAccount(app, accountName) != null;
        }

        public static string GetUserNameWithoutDomain(Vault gVault, int userId)
        {
            var user = gVault.UserOperations.GetUserAccount(userId);
            return GetUserNameWithoutDomain(user.LoginName);
        }

        public static IList<LoginAccount> GetLoginAccounts(MFilesServerApplication app)
        {
            var accs = app.LoginAccountOperations.GetLoginAccounts();
            return (from LoginAccount a in accs 
                    where a.Enabled && a.LicenseType != MFLicenseType.MFLicenseTypeNone 
                    && a.ServerRoles != MFLoginServerRole.MFLoginServerRoleSystemAdministrator 
                    select a).ToList();
        }
    }
}
