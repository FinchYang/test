using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class DepartmentMap : EntityMap<Department>
    {
        public DepartmentMap()
        {
            ToTable("Department");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
}
