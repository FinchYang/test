using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public class UserVaultService : IUserVaultService
    {
        private readonly IRepository<UserVault> _userVaultRepo;
        private readonly IMFilesVaultService _vaultService;

        public UserVaultService(IRepository<UserVault> userVaultRepo, IMFilesVaultService vaultService)
        {
            _userVaultRepo = userVaultRepo;
            _vaultService = vaultService;
        }
        public ICollection<MFilesVault> GetVaults(long userId)
        {
            var vaultIds = _userVaultRepo.Table.Where(c => c.UserId == userId).Select(c=>c.VaultId);
            var vaults = new List<MFilesVault>();
            foreach (var c in vaultIds)
            {
                try
                {
                    var v = _vaultService.GetVaultById(c);
                    vaults.Add(v);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return vaults;
        }

        //public ICollection<User> GetUsers(int vaultId)
        //{
        //    throw new NotImplementedException();
        //}

        public void AddUserVault(long userId, long vaultId, bool isCreator = false)
        {
            var has = UserHasVault(userId, vaultId);
            if (!has)
            {
                var uv = new UserVault {UserId = userId, VaultId = vaultId, UserIsCreator = isCreator};
                _userVaultRepo.Insert(uv);
            }
        }

        public void RemoveUserVault(long userId, long vaultId)
        {
            var uv = _userVaultRepo.Table.FirstOrDefault(c => c.UserId == userId && c.VaultId == vaultId);
            if (uv != null)
            {
                _userVaultRepo.Delete(uv);
            }
        }

        public bool UserHasVault(long userId, long vaultId)
        {
            return _userVaultRepo.Table.Any(c => c.UserId == userId && c.VaultId == vaultId);
        }
    }
}
