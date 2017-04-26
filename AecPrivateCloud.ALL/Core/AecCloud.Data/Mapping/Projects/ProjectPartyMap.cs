using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectPartyMap : EntityMap<ProjectParty>
    {
        public ProjectPartyMap()
        {
            ToTable("ProjectParty");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired();
            Property(c => c.Description).IsRequired();
        }
    }
}
