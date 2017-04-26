using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public class VaultTemplateService : IVaultTemplateService
    {
        private readonly IRepository<VaultTemplate> _templateRepo;
        private readonly IRepository<CloudVaultTemplate> _cloudTemplateRepo;

        public VaultTemplateService(IRepository<VaultTemplate> templateRepo, IRepository<CloudVaultTemplate> cloudTemplateRepo)
        {
            _templateRepo = templateRepo;
            _cloudTemplateRepo = cloudTemplateRepo;
        }

        public ICollection<VaultTemplate> GetTemplates()
        {
            return _templateRepo.Table.ToList();
        }

        public VaultTemplate GetTemplateById(long templateId)
        {
            return _templateRepo.GetById(templateId);
        }

        public ICollection<VaultTemplate> GetTemplatesByCloud(long cloudId)
        {
            var templateIds = _cloudTemplateRepo.Table.Where(c => c.CloudId == cloudId).Select(c => c.VaultTemplateId).ToArray();
            return templateIds.Select(c => _templateRepo.GetById(c)).ToList();
        }

        public void AddTemplate(VaultTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _templateRepo.Insert(template);
        }

        public void UpdateTemplate(VaultTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _templateRepo.Update(template);
        }

        public void DeleteTemplate(VaultTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _templateRepo.Delete(template);
        }

        public void AddCloudTemplate(long cloudId, long templateId)
        {
            var ct =
                _cloudTemplateRepo.TableNoTracking.FirstOrDefault(
                    c => c.CloudId == cloudId && c.VaultTemplateId == templateId);
            if (ct == null)
            {
                ct = new CloudVaultTemplate {CloudId = cloudId, VaultTemplateId = templateId};
                _cloudTemplateRepo.Insert(ct);
            }
        }
    }
}
