using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MFServerUtility
    {
      
        public static Vault GetVault(MFilesVault vault)
        {
            var app = ConnectToMfApp(vault);
            var mVault = app.LogInToVault(vault.Guid);
            return mVault;
        }

        public static MFilesServerApplication ConnetToMfApp(User user, string password, VaultServer server)
        {
            var accountName = user.GetAccountName();
            var app = ConnectToServer(accountName, password, GetVaultServerLocalIp(server), server.ServerPort);
            return app;
        }
        public static MFilesServerApplication ConnectToMfApp(MFilesVault vault)
        {
            var server = vault.Server;
            var app = ConnectToServer(server.AdminName, server.AdminPwd, GetVaultServerLocalIp(vault.Server), vault.Server.ServerPort);
            return app;
        }
        public static string GetVaultServerLocalIp(VaultServer vaultServer)
        {
            return vaultServer.LocalIp;
        }

        public static MFilesServerApplication ConnectToServer(VaultServer server)
        {
            return ConnectToServer(server.AdminName, server.AdminPwd, server.LocalIp, server.ServerPort);
        }

        /// <summary>
        /// 获取MFiles服务端连接
        /// </summary>
        /// <param name="adminName">管理员账户</param>
        /// <param name="adminPwd">管理员密码</param>
        /// <param name="ip">服务器ip</param>
        /// <param name="port">服务端口</param>
        /// <returns></returns>
        public static MFilesServerApplication ConnectToServer(string adminName, string adminPwd, string ip, string port)
        {
            var app = new MFilesServerApplication();
            if (adminName.Contains('\\'))
            {
                var strs = adminName.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
                var domain = strs[0];
                var userName = strs[1];
                app.Connect(MFAuthType.MFAuthTypeSpecificWindowsUser, userName, adminPwd, domain, "ncacn_ip_tcp", ip, port);
            }
            else
            {
                app.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, adminName, adminPwd, "", "ncacn_ip_tcp", ip, port);
            }
            
            return app;
        }
      
        public static MFilesServerApplication ConnectToServer(string userName, string password, string ip, string port, bool isAdUser)
        {
            var mfAuthType = MFAuthType.MFAuthTypeSpecificMFilesUser;
            if (isAdUser)
            {
                mfAuthType = MFAuthType.MFAuthTypeSpecificWindowsUser;
            }
            var app = new MFilesServerApplication();
            app.Connect(mfAuthType, userName, password, "", "ncacn_ip_tcp", ip, port);
            return app;
        }
    }
}
