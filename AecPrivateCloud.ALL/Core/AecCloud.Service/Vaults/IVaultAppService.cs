using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public interface IVaultAppService
    {
        IList<VaultApp> GetAllApps();

        VaultApp GetById(long id);

        void InsertVaultApp(VaultApp app);

        void UpdateVaultApp(VaultApp app);

        void DeleteVaultApp(VaultApp app);

        IList<VaultAppVaultTemplate> GetAllAppsWithTemplates();

        IList<VaultAppVaultTemplate> GetAppsWithTemplate(long vaulttemplateId);

        VaultAppVaultTemplate GetAppWithTemplateById(long id);

        VaultAppVaultTemplate GetAppWithTemplate(long templateId, long vaultappId);

        void InsertAppWithTemplate(VaultAppVaultTemplate appWithTemplate);

        void UpdateAppWithTemplate(VaultAppVaultTemplate appWithTemplate);

        void DeleteAppWithTemplate(VaultAppVaultTemplate appWithTemplate);
    }
}
