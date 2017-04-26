using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class ProjectPartyDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Alias { get; set; }
    }
}
