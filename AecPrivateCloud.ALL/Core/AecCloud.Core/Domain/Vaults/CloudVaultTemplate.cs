using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    public class CloudVaultTemplate : Entity
    {
        public long CloudId { get; set; }

        public long VaultTemplateId { get; set; }
    }
}
