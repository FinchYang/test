using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class ProjectMemberDto : EntityDto
    {
        public long UserId { get; set; }

        public long ProjectId { get; set; }

        public bool Accepted { get; set; }
    }
}
