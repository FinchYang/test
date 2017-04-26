using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class VaultAppMap : EntityMap<VaultApp>
    {
        public VaultAppMap()
        {
            HasKey(c => c.Id);
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(c => c.Guid).IsRequired().HasMaxLength(Guid.Empty.ToString().Length);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
            Property(c => c.Filepath).IsRequired();
            Property(c => c.Version).IsRequired();
        }
    }
}
