using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class VaultTemplateMap : EntityMap<VaultTemplate>
    {
        public VaultTemplateMap()
        {
            ToTable("VaultTemplate");
            HasKey(c => c.Id);
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
            Property(c => c.StructurePath).IsRequired();
            Ignore(c => c.ImageUrl);
        }
    }
}
