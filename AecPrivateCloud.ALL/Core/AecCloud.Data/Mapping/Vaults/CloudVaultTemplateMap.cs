using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class CloudVaultTemplateMap : EntityMap<CloudVaultTemplate>
    {
        public CloudVaultTemplateMap()
        {
            ToTable("Cloud_VaultTemplate");
            Property(c => c.CloudId).IsRequired();
            Property(c => c.VaultTemplateId).IsRequired();
        }
    }
}
