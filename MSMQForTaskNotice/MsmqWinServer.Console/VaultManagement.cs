using System;
using System.Collections.Generic;
using System.Linq;
using MFilesAPI;

namespace MsmqWinServer.Console
{
    public class VaultManagement
    {
        private class SererVault
        {
            internal string Guid { get; set; }

            internal string Name { get; set; }

            internal Vault Vault { get; set; }
        }

        private MFilesServerApplication _serverApp;
        private readonly IList<SererVault> _vaults = new List<SererVault>();
        private readonly ServerAdminUser _adminUser;

        public VaultManagement(ServerAdminUser adminUser)
        {
            _adminUser = adminUser;
        }

        /// <summary>
        /// 获取服务端应用
        /// </summary>
        /// <returns></returns>
        public MFilesServerApplication GetServerApplication()
        {
            return GetServerApp();
        }

        public void ServerAppDisconnect()
        {
            if (ServerAppConnected()) _serverApp.Disconnect();
        }

        public void VaultLogOut(Vault vault)
        {
            if(VaultLoggedin(vault)) vault.LogOutSilent();
        }
        /// <summary>
        /// 获取指定的Vault
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Vault GetServerVault(string guid)
        {
            return GetVault(guid);
        }
        /// <summary>
        /// 获取库列表
        /// </summary>
        /// <returns></returns>
        public IList<MFVaultInfo> GetVaultList()
        {
            return GetVaults();
        }


        /// <summary>
        /// 获取服务端连接
        /// </summary>
        /// <returns></returns>
        private MFilesServerApplication GetServerApp()
        {
            return ServerAppConnected() ? _serverApp : ConnectToServerApplication();
        }

        /// <summary>
        /// 获取库
        /// </summary>
        private Vault GetVault(string vaultGuid)
        {
            var vaultItem = _vaults.FirstOrDefault(c => c.Guid == vaultGuid);
            if (vaultItem == null)
            {
                var vault = LoginToVault(vaultGuid);
                vaultItem = new SererVault { Guid = vault.GetGUID(), Name = vault.Name, Vault = vault };
                _vaults.Add(vaultItem);
                return vault;
            }
            var v = vaultItem.Vault;
            if (!VaultLoggedin(v))
            {
                v = LoginToVault(vaultGuid);
            }
            return v;
        }

        private IList<MFVaultInfo> GetVaults()
        {
            var list = new List<MFVaultInfo>();
            var app = GetServerApplication();
            if (app == null) return list;
            var vaults = app.GetOnlineVaults().Cast<VaultOnServer>();
            return vaults.Select(c => new MFVaultInfo { Guid = c.GUID, Name = c.Name }).ToList();
        }

        /// <summary>
        /// 判断库是否登录
        /// </summary>
        /// <returns></returns>
        private bool VaultLoggedin(Vault vault)
        {
            try
            {
                return vault.LoggedIn;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 登录到库
        /// </summary>
        /// <param name="vaultGuid"></param>
        /// <returns></returns>
        private Vault LoginToVault(string vaultGuid)
        {
            var app = GetServerApp();
            if (app == null) throw new Exception("连接到ServerApplication失败");
            var v = app.GetOnlineVaults().Cast<VaultOnServer>().FirstOrDefault(c => c.GUID == vaultGuid);
            if (v == null) return null;
            var vault = app.LogInToVaultAdministrative(vaultGuid);
            var added = AddVaultAdmin(vault);
            if (added) vault = app.LogInToVaultAdministrative(vaultGuid);
            return vault;
        }
        /// <summary>
        /// 增加库用户
        /// </summary>
        private bool AddVaultAdmin(Vault vault)
        {
            var user = vault.UserOperations.GetUserAccounts().Cast<UserAccount>().FirstOrDefault(c => c.LoginName == _adminUser.Name);
            if (user != null) return false;
            var account = new UserAccount
            {
                LoginName = _adminUser.Name,
                VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleFullControl,
                Enabled = true,
                InternalUser = true
            };
            vault.UserOperations.AddUserAccount(account);
            return true;
        }
        /// <summary>
        /// 判断服务端是否已连接
        /// </summary>
        /// <returns></returns>
        private bool ServerAppConnected()
        {
            if (_serverApp == null) return false;
            try
            {
                _serverApp.GetServerVersion();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 连接到ServerApplication
        /// </summary>
        private MFilesServerApplication ConnectToServerApplication()
        {
            try
            {
                if (_serverApp == null) _serverApp = new MFilesServerApplication();
                var authType = MFAuthType.MFAuthTypeSpecificMFilesUser;
                if (_adminUser.AccountType == MFLoginAccountType.MFLoginAccountTypeWindows)
                    authType = MFAuthType.MFAuthTypeSpecificWindowsUser;
                _serverApp.ConnectAdministrative(null, authType, _adminUser.Name, _adminUser.Pwd,"","ncacn_ip_tcp",_adminUser.ServerIp);
            }
            catch (Exception)
            {
                _serverApp = null;
            }
            return _serverApp;
        }
    }

    public class MFVaultInfo
    {
        /// <summary>
        /// 库的guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 库名称
        /// </summary>
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is MFVaultInfo)) return false;
            var v = obj as MFVaultInfo;
            return v.Guid == Guid && v.Name == Name;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() ^ Name.GetHashCode();
        }
    }
}
