using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class AreaMap : EntityMap<Area>
    {
        public AreaMap()
        {
            ToTable("Area");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
        }
    }
}
