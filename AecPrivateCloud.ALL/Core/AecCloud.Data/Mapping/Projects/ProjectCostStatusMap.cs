using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectCostStatusMap : EntityMap<ProjectCostStatus>
    {
        public ProjectCostStatusMap()
        {
            ToTable("ProjectCostStatus");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
        }
    }
}
