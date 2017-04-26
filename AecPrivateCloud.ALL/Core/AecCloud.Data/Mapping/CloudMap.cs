using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class CloudMap : EntityMap<Cloud>
    {
        public CloudMap()
        {
            ToTable("Cloud");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
            Property(c => c.Version).IsRequired();
            //HasMany(c => c.Templates).WithMany().Map(m => m.ToTable("Cloud_VaultTemplate_Mapping"));
        }
    }
}
