using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class ProjectInvitationMap : EntityMap<ProjectInvitation>
    {
        public ProjectInvitationMap()
        {
            ToTable("ProjectInvitation");
            HasKey(c => c.Id);
            Property(c => c.ProjectId).IsRequired();
            Property(c => c.InviterId).IsRequired();
            Property(c => c.InvitationMessage).IsRequired();
            //Property(c => c.InviteePartId).IsRequired();
            Ignore(c => c.AcceptedBy);
        }
    }
}
