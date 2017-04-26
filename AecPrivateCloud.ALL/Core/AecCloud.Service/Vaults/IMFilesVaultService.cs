using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public interface IMFilesVaultService
    {
        MFilesVault GetVaultById(long vaultId);

        MFilesVault GetVaultByGuid(string guid);

        ICollection<MFilesVault> GetVaultsByName(string name);

        ICollection<MFilesVault> GetAllVaults();

        ICollection<MFilesVault> GetDefaultVaults();

        void InsertVault(MFilesVault vault);

        void UpdateVault(MFilesVault vault);

        void DeleteVault(MFilesVault vault);

        VaultServer GetServer(long vaultId);

        ICollection<MFilesVault> GetVaultsByTemplate(long templateId);

        ICollection<MFilesVault> GetVaultsByCloud(long cloudId);

        string CreateObjectUrl(string vaultGuid, int objType, int classId, Dictionary<string, string> propValues,
            int? template);
    }
}
