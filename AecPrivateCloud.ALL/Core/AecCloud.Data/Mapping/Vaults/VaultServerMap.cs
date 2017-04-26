using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class VaultServerMap : EntityMap<VaultServer>
    {
        public VaultServerMap()
        {
            ToTable("VaultServer");
            HasKey(c => c.Id);
            Property(c => c.Ip).IsRequired();
            Property(c => c.Port).IsRequired();
            Property(c => c.AdminName).IsRequired();
            Property(c => c.AdminPwd).IsRequired();
            Ignore(c => c.ServerPort);
        }
    }
}
