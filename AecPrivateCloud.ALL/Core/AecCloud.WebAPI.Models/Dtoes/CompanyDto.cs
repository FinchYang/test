using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class CompanyDto : EntityDto
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AreaDto : EntityDto
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ProjectLevelDto : EntityDto
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
