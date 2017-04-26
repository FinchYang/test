using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class UserMap : EntityMap<User>
    {
        public UserMap()
        {
            ToTable("AecUser");
            HasKey(c => c.Id);
            //Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(c => c.UserName).IsRequired().HasMaxLength(255);
            HasMany(c => c.Roles).WithMany().Map(m=>m.ToTable("User_UserRole_Mapping"));
            //Ignore(c => c.Company);
            //Ignore(c => c.Department);
        }
    }
}
