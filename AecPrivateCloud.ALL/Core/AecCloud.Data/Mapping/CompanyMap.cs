using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
    public class CompanyMap : EntityMap<Company>
    {
        public CompanyMap()
        {
            ToTable("Company");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
    public class CscecRoleMap : EntityMap<CscecRole>
    {
        public CscecRoleMap()
        {
            ToTable("CscecRole");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
    public class PersonnelCategoryMap : EntityMap<PersonnelCategory>
    {
        public PersonnelCategoryMap()
        {
            ToTable("PersonnelCategory");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
    public class PositionInfoMap : EntityMap<PositionInfo>
    {
        public PositionInfoMap()
        {
            ToTable("PositionInfo");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
        }
    }
}
