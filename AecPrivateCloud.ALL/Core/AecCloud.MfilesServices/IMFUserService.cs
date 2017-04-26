using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.MfilesServices
{
    /// <summary>
    /// MFiles User服务
    /// </summary>
    public interface IMFUserService
    {
        void CreateMFilesLoginAccount(User user, VaultServer server);

        int CreateVaultUser(User user, MFilesVault vault);

        void ChangeVaultUserInfo(User user, VaultServer server);

        void DisableVaultUser(string userName, MFilesVault vault);

        void DisableVaultUser(int userId, MFilesVault vault);

        void EnableVaultUser(string userName, MFilesVault vault);

        bool HasVaultUser(string userName, MFilesVault vault);

        bool HasVaultUser(int userId, MFilesVault vault);

        int? GetUserId(string userName, MFilesVault vault);

        string GetAccountName(int userId, MFilesVault vault);

        bool ConnectToServer(string userName, string password, VaultServer server, bool windowsUser);

        IList<MfUser> GetMFilesLoginAccounts(VaultServer server);

        UserInfo GetUserInfo(VaultServer server, string userName);
    }

    public class MfUser
    {
        public string AccountName { get; set; }
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
