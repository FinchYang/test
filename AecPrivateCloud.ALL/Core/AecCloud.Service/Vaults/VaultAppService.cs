using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public class VaultAppService : IVaultAppService
    {
        private readonly IRepository<VaultApp> _appRepo;
        private readonly IRepository<VaultAppVaultTemplate> _apptempRepo;

        public VaultAppService(IRepository<VaultApp> appRepo, IRepository<VaultAppVaultTemplate> apptempRepo)
        {
            _appRepo = appRepo;
            _apptempRepo = apptempRepo;
        }

        public IList<VaultApp> GetAllApps()
        {
            return _appRepo.Table.ToList();
        }

        public VaultApp GetById(long id)
        {
            return _appRepo.GetById(id);
        }

        public void InsertVaultApp(VaultApp app)
        {
            if (app == null) throw new ArgumentNullException("app");
            //var app0 = _appRepo.Table.FirstOrDefault(c => c.Guid == app.Guid);
            //if (app0 != null) throw new AecException("此VaultApp已存在：" + app.Guid);
            _appRepo.Insert(app);
        }

        public void UpdateVaultApp(VaultApp app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _appRepo.Update(app);
        }

        public void DeleteVaultApp(VaultApp app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _appRepo.Delete(app);
        }


        public IList<VaultAppVaultTemplate> GetAllAppsWithTemplates()
        {
            return _apptempRepo.Table.ToList();
        }

        public IList<VaultAppVaultTemplate> GetAppsWithTemplate(long vaulttemplateId)
        {
            return _apptempRepo.Table.Where(c => c.VaultTemplateId == vaulttemplateId).ToList();
        }

        public VaultAppVaultTemplate GetAppWithTemplateById(long id)
        {
            return _apptempRepo.GetById(id);
        }

        public VaultAppVaultTemplate GetAppWithTemplate(long templateId, long vaultappId)
        {
            return _apptempRepo.Table.FirstOrDefault(c => c.VaultTemplateId == templateId && c.VaultAppId == vaultappId);
        }

        public void InsertAppWithTemplate(VaultAppVaultTemplate appWithTemplate)
        {
            if (appWithTemplate == null) throw new ArgumentNullException("appWithTemplate");
            var app0 = GetAppWithTemplate(appWithTemplate.VaultTemplateId, appWithTemplate.VaultAppId);
            if (app0 != null) return;
            _apptempRepo.Insert(appWithTemplate);
        }

        public void UpdateAppWithTemplate(VaultAppVaultTemplate appWithTemplate)
        {
            if (appWithTemplate == null) throw new ArgumentNullException("appWithTemplate");
            _apptempRepo.Update(appWithTemplate);
        }

        public void DeleteAppWithTemplate(VaultAppVaultTemplate appWithTemplate)
        {
            if (appWithTemplate == null) throw new ArgumentNullException("appWithTemplate");
            _apptempRepo.Delete(appWithTemplate);
        }
    }
}
