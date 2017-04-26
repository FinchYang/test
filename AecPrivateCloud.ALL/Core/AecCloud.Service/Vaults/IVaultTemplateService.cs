using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public interface IVaultTemplateService
    {

        ICollection<VaultTemplate> GetTemplates();

        VaultTemplate GetTemplateById(long templateId);

        ICollection<VaultTemplate> GetTemplatesByCloud(long cloudId);

        void AddTemplate(VaultTemplate template);

        void UpdateTemplate(VaultTemplate template);

        void DeleteTemplate(VaultTemplate template);

        void AddCloudTemplate(long cloudId, long templateId);


    }
}
