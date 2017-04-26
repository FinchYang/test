using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.CloudServices.RequestObject
{
    /// <summary>
    /// 创建云应用账户，并设置应用权限的请求类  
    /// </summary>
    internal class RequestUserAndApp
    {

        public string action { get { return "SetUserPermission"; } }

        public RequestUserAndAppData data { get; set; }
    }
    /// <summary>
    /// RequestUserAndApp的数据类
    /// </summary>
    internal class RequestUserAndAppData
    {
        public string Domain { get; set; }

        public string UserName { get; set; }

        public string GroupName { get; set; }

        public bool IsVip { get; set; }

        public string Apps { get; set; }
    }
}
