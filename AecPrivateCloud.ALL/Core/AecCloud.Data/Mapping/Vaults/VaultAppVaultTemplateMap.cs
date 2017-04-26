using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class VaultAppVaultTemplateMap : EntityMap<VaultAppVaultTemplate>
    {
        public VaultAppVaultTemplateMap()
        {
            ToTable("VaultApp_VaultTemplate");
            HasKey(c => c.Id);
            Property(c => c.VaultAppId).IsRequired();
            Property(c => c.VaultTemplateId).IsRequired();
        }
    }
}
