using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public class MFilesVaultService : IMFilesVaultService
    {
        private readonly IRepository<MFilesVault> _vaultRepo;

        public MFilesVaultService(IRepository<MFilesVault> vaultRepo)
        {
            _vaultRepo = vaultRepo;
        }
        public MFilesVault GetVaultById(long vaultId)
        {
            if (vaultId <= 0) throw new ArgumentException("vaultId");
            return _vaultRepo.GetById(vaultId);
        }

        public MFilesVault GetVaultByGuid(string guid)
        {
            Guid g;
            if (!Guid.TryParse(guid, out g)) throw new ArgumentException("guid");
            return _vaultRepo.Table.FirstOrDefault(c => c.Guid == guid.ToUpper());
        }

        public ICollection<MFilesVault> GetVaultsByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            return _vaultRepo.Table.Where(c => c.Name == name).ToList();
        }

        public ICollection<MFilesVault> GetAllVaults()
        {
            return _vaultRepo.Table.ToList();
        }

        public ICollection<MFilesVault> GetDefaultVaults()
        {
            return _vaultRepo.Table.Where(c => c.Default).ToList();
        }

        public void InsertVault(MFilesVault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            _vaultRepo.Insert(vault);
        }

        public void UpdateVault(MFilesVault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            _vaultRepo.Update(vault);
        }

        public void DeleteVault(MFilesVault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            _vaultRepo.Delete(vault);
        }

        public VaultServer GetServer(long vaultId)
        {
            if (vaultId <= 0) throw new ArgumentException("vaultId");
            var vault = _vaultRepo.GetById(vaultId);
            if (vault != null) return vault.Server;
            return null;
        }


        public ICollection<MFilesVault> GetVaultsByTemplate(long templateId)
        {
            if (templateId <= 0) throw new ArgumentException("templateId");
            return _vaultRepo.Table.Where(c => c.TemplateId == templateId).ToList();
        }

        public ICollection<MFilesVault> GetVaultsByCloud(long cloudId)
        {
            return _vaultRepo.Table.Where(c => c.CloudId == cloudId).ToList();
        }


        public string CreateObjectUrl(string vaultGuid, int objType, int classId, Dictionary<string, string> propValues, int? template)
        {
            var sb = new StringBuilder();
            sb.Append("m-files://newobject/");
            if (vaultGuid.StartsWith("{"))
            {
                vaultGuid = vaultGuid.Substring(1);
            }
            if (vaultGuid.EndsWith("}"))
            {
                vaultGuid = vaultGuid.Substring(0, vaultGuid.Length - 1);
            }
            sb.Append(vaultGuid + "/");
            sb.Append(objType);
            sb.Append("?class=" + classId);
            if (template != null)
            {
                sb.Append("&template=" + template.Value);
            }
            foreach (var d in propValues)
            {
                sb.Append("&property=" + d.Key + "/" + d.Value);
            }
            return sb.ToString();
        }
    }
}
