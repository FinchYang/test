using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectTimeLimitStatusMap : EntityMap<ProjectTimeLimitStatus>
    {
        public ProjectTimeLimitStatusMap()
        {
            ToTable("ProjectTimeLimitStatus");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
        }
    }
}
