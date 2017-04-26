using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesClientCore
{
    /// <summary>
    /// 客户端与App数据传递类
    /// </summary>
    public class MfNamedValueCollection
    {
        /// <summary>
        /// 当前用户的DBWorld用户ID
        /// </summary>
        public static readonly string DBUserId = "DBUserId";
        /// <summary>
        /// 当前用户的DBWorld注册邮箱
        /// </summary>
        public static readonly string DBUserEmail = "UserEmail";
        /// <summary>
        /// 当前用户的Token
        /// </summary>
        public static readonly string DBUserToken = "UserToken";
        /// <summary>
        /// 当前文档库的DBWorld中的项目ID
        /// </summary>
        public static readonly string DBProjectId = "ProjectId";
        /// <summary>
        /// 当前用户是否有云应用的权限
        /// </summary>
        public static readonly string DBCloudAppEnabled = "CloudAppEnabled";
        /// <summary>
        /// 协同云WebAPI的地址
        /// </summary>
        public static readonly string DBApiHost = "ApiHost";
        /// <summary>
        /// 协同云网站的地址
        /// </summary>
        public static readonly string DBWebHost = "WebHost";
        /// <summary>
        /// 项目API站点的地址
        /// </summary>
        public static readonly string DBProjApiHost = "ProjHost";
        /// <summary>
        /// 项目站点的地址
        /// </summary>
        public static readonly string DBProjWebHost = "ProjWebHost";
        /// <summary>
        /// DBWorld客户端的安装路径，获取邮件等其他系统的可执行程序的路径
        /// </summary>
        public static readonly string DBInstallPath = "InstallPath";

        public static readonly string DBCloudAppHost = "CloudApp";
    }
}
