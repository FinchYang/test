using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class ProjectStatusDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class ProjectCostDto : EntityDto
    {
        public string Name { get; set; }
    }

    public class ProjectProgressDto : EntityDto
    {
        public long ProjId { get; set; }

        public string Month { get; set; }

        public bool OK { get; set; }
    }
}
