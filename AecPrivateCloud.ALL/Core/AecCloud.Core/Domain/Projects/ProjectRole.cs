using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Projects
{
    public class ProjectRole : Entity
    {
        public string Alias { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
