using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{

    public class ProjectClassList
    {
        public static List<string> Items
        {
            get
            {
                return new List<string>
                {
                    "公建项目",
                    "房地产项目",
                    "基础设施项目",
                    "融资类项目",
                    "工期变更大的项目"
                };
            }
        }
    }

    public static class CloudConstants
    {
        /// <summary>
        /// 我的项目
        /// </summary>
        public static readonly long MyProjects = 1;

        /// <summary>
        /// 项目管理
        /// </summary>
        public static readonly long ProjManagements = 2;

        /// <summary>
        /// 分包商管理
        /// </summary>
        public static readonly long SubContracts = 3;
    }

    public static class ProjectRoleConstants
    {
        /// <summary>
        /// 项目经理
        /// </summary>
        public static readonly string ProjectManager = "项目经理";
        /// <summary>
        /// 项目副理
        /// </summary>
        public static readonly string ProjectAssistant = "项目副理";
        /// <summary>
        /// 一般成员
        /// </summary>
        public static readonly string ProjectMember = "一般成员";
        /// <summary>
        /// 外包成员
        /// </summary>
        public static readonly string ProjecyContractor = "外包成员";
    }

    public static class ProjectStatusConstants
    {
        /// <summary>
        /// 立项
        /// </summary>
        public static readonly int CreateProjectId = 1;
        /// <summary>
        /// 启动
        /// </summary>
        public static readonly int StartProjectId = 2;
        /// <summary>
        /// 暂停
        /// </summary>
        public static readonly int PauseProjectId = 3;
        /// <summary>
        /// 结束
        /// </summary>
        public static readonly int EndProjectId = 4;
    }
}
