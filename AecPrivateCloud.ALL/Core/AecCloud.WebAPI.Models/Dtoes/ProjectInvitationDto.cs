using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class ProjectInvitationDto : EntityDto
    {
        public long ProjectId { get; set; }

        public long InviterId { get; set; }

        public long InviteePartId { get; set; }

        public long InviteeId { get; set; }

        public string InviteeEmail { get; set; }

        public string InvitationMessage { get; set; }

        public string InviteeConfirmMessage { get; set; }

        public bool Accepted { get; set; }
    }
}
