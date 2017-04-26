using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectMemberMap : EntityMap<ProjectMember>
    {
        public ProjectMemberMap()
        {
            ToTable("ProjectMember");
            HasKey(c => c.Id);
            Property(c => c.UserId).IsRequired();
            Property(c => c.ProjectId).IsRequired();
        }
    }
}
