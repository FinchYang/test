using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
    public class SharedFileMap : EntityMap<SharedFile>
    {
        public SharedFileMap()
        {
            ToTable("SharedFile");
            HasKey(c => c.Id);
            Property(c => c.UrlPart).IsRequired();
            Property(c => c.Password).IsRequired();
            Property(c => c.UrlKey).IsRequired();
        }
    }
}
