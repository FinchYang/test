using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain
{
    /// <summary>
    /// 常用用户角色的名称列表
    /// </summary>
    public class SystemUserRoleNames
    {
        /// <summary>
        /// 管理员
        /// </summary>
        public const string Admins = "Admin";
        /// <summary>
        /// 注册用户
        /// </summary>
        public const string Registered = "Registered";
        /// <summary>
        /// 项目总监
        /// </summary>
        public const string ProjectDirectors = "ProjectDirector";
        /// <summary>
        /// 集团领导
        /// </summary>
        public const string CorperationLeaders = "CorperationLeader";
        /// <summary>
        /// 分包商管理
        /// 
        /// </summary>
        public const string SubContractors = "SubContractor";
    }
}
