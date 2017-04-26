using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Projects
{
    public class ProjectInvitation : Entity
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        public long ProjectId { get; set; }
        /// <summary>
        /// 发起邀请的用户ID
        /// </summary>
        public long InviterId { get; set; }
        /// <summary>
        /// 邀请为的参与方
        /// </summary>
        public long InviteePartId { get; set; }
        /// <summary>
        /// 接受邀请后的用户ID
        /// </summary>
        public long InviteeId { get; set; }
        /// <summary>
        /// 受邀请人的邮箱
        /// </summary>
        public string InviteeEmail { get; set; }
        /// <summary>
        /// 邀请时提供的信息
        /// </summary>
        public string InvitationMessage { get; set; }
        /// <summary>
        /// 被邀请人确认接受邀请的回复
        /// </summary>
        public string InviteeConfirmMessage { get; set; }
        /// <summary>
        /// 是否由项目内人员确认加入项目
        /// </summary>
        public bool Accepted { get; set; }
        /// <summary>
        /// 若没有参与方，则需要知道最后的确认人是谁
        /// </summary>
        public long AcceptedBy { get; set; }

        public long BidProjectId { get; set; }
    }
}
