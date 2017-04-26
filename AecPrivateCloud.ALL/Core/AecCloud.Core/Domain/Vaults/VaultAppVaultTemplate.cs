using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    public class VaultAppVaultTemplate : Entity
    {
        public long VaultAppId { get; set; }

        public long VaultTemplateId { get; set; }

        public bool Default { get; set; }
    }
}
