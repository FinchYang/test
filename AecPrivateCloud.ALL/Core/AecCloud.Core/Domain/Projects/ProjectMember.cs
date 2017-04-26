using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Projects
{
    public class ProjectMember : Entity
    {
        //public ProjectMember ()
        //{
        //    Display = true;
        //}
        public long UserId { get; set; }

        public long ProjectId { get; set; }
        /// <summary>
        /// MF库联系人ID
        /// </summary>
        public int ContactId { get; set; }

        public bool Disabled { get; set; }
        /// <summary>
        /// 是否是负责人
        /// </summary>
        public bool IsDirector { get; set; }
        public bool Display { get; set; }
    }
}
