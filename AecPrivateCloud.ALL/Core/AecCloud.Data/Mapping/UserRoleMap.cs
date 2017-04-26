using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class UserRoleMap : EntityMap<UserRole>
    {
        public UserRoleMap()
        {
            ToTable("UserRole");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
}
