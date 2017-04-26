using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.CloudServices.RequestObject;
using AecCloud.CloudServices.ResponseObject;
using AecCloud.CloudServices.Utils;

namespace AecCloud.CloudServices
{
    /// <summary>
    /// 云应用相关服务
    /// </summary>
    public class CitrixAppService : ICitrixAppService
    {
        /// <summary>
        /// 获取全部的云应用列表
        /// </summary>
        /// <returns></returns>
        public IList<CloudApplication> GetAllCloudApp(string url)
        {
            var requestObj = new RequestAppList();
            var requestStr = JsonUtil.ConventToJson(requestObj);
            var json = HttpUtil.GetResponseJson(url, requestStr);
            var appList = JsonUtil.ConventToObject<AppNameList>(json);
            return appList.resultList;
        }
    }
}
