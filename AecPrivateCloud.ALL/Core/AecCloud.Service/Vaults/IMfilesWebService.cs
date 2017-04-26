using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service.Vaults
{
    public interface IMfilesWebService
    {
        /// <summary>
        /// 监理例会统计
        /// </summary>
        /// <param name="guidAndIps"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        List<CompanyMeetingStatics> SupervisorMeetingStatics(List<CompanyMeetingStatics>list,Dictionary<string, string> guidAndIps,
            string userName, string pwd, string year, string month);
        /// <summary>
        /// 工期节点统计
        /// </summary>
        /// <param name="guidAndIps"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        List<ScheduleNode> ScheduleControlStatistics(Dictionary<string, string> guidAndIps, string userName, string pwd,
            string name);
    }
}
