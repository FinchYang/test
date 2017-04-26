using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.WorkingHour
{
    public class PersonalBudget: Entity
    {
        public string VaultGuid { get; set; }
        /// <summary>
        /// MFiles库中的用户ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// MFiles登录用户名，不含域名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 成员对象ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 工时细化(按月)，json字符串
        /// </summary>
        public string HoursDetail { get; set; }

    }
}
