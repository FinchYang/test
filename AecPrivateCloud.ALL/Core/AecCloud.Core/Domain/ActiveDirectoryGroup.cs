using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain
{
    public class ActiveDirectoryGroup : Entity
    {
        public string Name { get; set; }

        public string OrganizationUnits { get; set; }

        public long DomainId { get; set; }

        public virtual ActiveDirectory Domain { get; set; }
    }
}
