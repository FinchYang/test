using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectLevelMap : EntityMap<ProjectLevel>
    {
        public ProjectLevelMap()
        {
            ToTable("ProjectLevel");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
        }
    }

    public class ProjectClassMap : EntityMap<ProjectClass>
    {
        public ProjectClassMap()
        {
            ToTable("ProjectClass");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
        }
    }
}
