using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.WorkingHour
{
    public class HourLog: Entity
    {
        public string VaultGuid { get; set; }
        /// <summary>
        /// MFiles库中的用户ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 日志日期
        /// </summary>
        public DateTime LogDate { get; set; }
        /// <summary>
        /// 工时
        /// </summary>
        public double LogHours { get; set; }
    }
}
