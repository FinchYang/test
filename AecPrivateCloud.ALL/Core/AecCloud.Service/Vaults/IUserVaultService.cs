using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public interface IUserVaultService
    {
        ICollection<MFilesVault> GetVaults(long userId);

        //ICollection<User> GetUsers(int vaultId);

        void AddUserVault(long userId, long vaultId, bool isCreator = false);

        void RemoveUserVault(long userId, long vaultId);

        bool UserHasVault(long userId, long vaultId);
    }
}
