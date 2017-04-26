using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service.Vaults
{
    /// <summary>
    /// 公司例会统计类
    /// </summary>
    public class CompanyMeetingStatics
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { set; get; }
        /// <summary>
        /// 项目例会召开列表
        /// </summary>
        public List<ProjMeetingStatics> ProjMeetingList = new List<ProjMeetingStatics>();
        /// <summary>
        /// 例会频次
        /// </summary>
        public int MeetingNums { set; get; }
    }
    /// <summary>
    /// 项目例会统计类
    /// </summary>
    public class ProjMeetingStatics
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName { set; get; }
        /// <summary>
        /// 会议次数
        /// </summary>
        public int MeetingNums { set; get; }
        /// <summary>
        /// 未召开原因
        /// </summary>
        public string UndidReason { set; get; }
    }

    /// <summary>
    /// 工期对象
    /// </summary>
    public class ScheduleNode
    {
        /// <summary>
        /// 节点编号
        /// </summary>
        public string Sn { set; get; }
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 最大偏差
        /// </summary>
        public int DevMax { set; get; }
        /// <summary>
        /// 最小偏差
        /// </summary>
        public int DevMini { set; get; }
        /// <summary>
        /// 平均偏差
        /// </summary>
        public string DevAvg { set; get; }
        /// <summary>
        /// 平均偏差比例
        /// </summary>
        public string DevAvgRate { set; get; }

        /// <summary>
        /// 工期（计划工期，实际工期）
        /// </summary>
        public List<PeriodPair> Schedule = new List<PeriodPair>();
    }
    /// <summary>
    /// 工期对
    /// </summary>
    public class PeriodPair
    {
        /// <summary>
        /// 计划工期
        /// </summary>
        public int PlanPeriod { set; get; }
        /// <summary>
        /// 实际工期
        /// </summary>
        public int RealPeriod { set; get; }
    }
}
