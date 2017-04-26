using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Projects
{
    public class ProjectProgressStatus : Entity
    {
        public ProjectProgressStatus()
        {
            OK = true;
        }
        public long ProjectId { get; set; }
        public string Month { get; set; }
        public bool OK { get; set; }
    }
}
