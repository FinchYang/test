using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class VaultMap : EntityMap<MFilesVault>
    {
        public VaultMap()
        {
            ToTable("Vault");
            HasKey(c => c.Id);
            Property(c => c.Guid).IsRequired();
            Property(c => c.Name).IsRequired().HasMaxLength(255);
            Property(c => c.CloudId).IsRequired();
            //HasRequired(c => c.Template).WithMany().HasForeignKey(c=>c.TemplateId).WillCascadeOnDelete(false);
            HasRequired(c => c.Server).WithMany().HasForeignKey(c => c.ServerId);
        }
    }
}
