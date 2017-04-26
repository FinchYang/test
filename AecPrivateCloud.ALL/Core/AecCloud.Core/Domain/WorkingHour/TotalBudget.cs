using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.WorkingHour
{
    public class TotalBudget: Entity
    {
        public string VaultGuid { get; set; }
        public string ProjectName { get; set; }
        public double TotalHours { get; set; }
        /// <summary>
        /// 项目开始日期
        /// </summary>
        public DateTime? BeginDate { get; set; }
        /// <summary>
        /// 项目截止日期
        /// </summary>
        public DateTime? Deadline { get; set; }
    }
}
