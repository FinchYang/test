using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain
{
    public class Company : Entity
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
    public class CscecRole : Entity
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
    public class PersonnelCategory : Entity
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
    public class PositionInfo : Entity
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
}
