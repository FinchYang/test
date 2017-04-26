using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.CloudServices.ResponseObject
{
    /// <summary>
    /// 应用列表返回对象
    /// </summary>
    internal class AppNameList
    {
        public string returnCode { get; set; }

        public string returnMsg { get; set; }

        public int totalCount { get; set; }

        public List<CloudApplication> resultList { get; set; }
    }
}
