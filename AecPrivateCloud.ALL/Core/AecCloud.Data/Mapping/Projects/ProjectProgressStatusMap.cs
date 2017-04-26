using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectProgressStatusMap : EntityMap<ProjectProgressStatus>
    {
        public ProjectProgressStatusMap()
        {
            ToTable("ProjectProgress");
            HasKey(c => c.Id);
            Property(c => c.ProjectId).IsRequired();
            Property(c => c.Month).IsRequired();
        }
    }
}
