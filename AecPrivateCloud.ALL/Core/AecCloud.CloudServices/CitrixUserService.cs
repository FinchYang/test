using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.CloudServices.RequestObject;
using AecCloud.CloudServices.Utils;
using AecCloud.Core.Domain;

namespace AecCloud.CloudServices
{
    public class CitrixUserService : ICitrixUserService
    {
        /// <summary>
        /// 创建云应用账户，并设置应用权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <param name="appList"></param>
        public void CreateCitrixUserAndApp(User user, string url, IList<CloudApplication> appList)
        {
            var apps = "";
            if (appList != null && appList.Count > 0)
            {
                var appArray = appList.Select(c => c.AppName).ToArray();
                apps = string.Join(",", appArray);
            }
            var requestObj = new RequestUserAndApp { data = new RequestUserAndAppData { Domain = user.Domain, UserName = user.UserName, GroupName = "", IsVip = false, Apps = apps } };
            var requestStr = JsonUtil.ConventToJson(requestObj);
            HttpUtil.GetResponseJson(url, requestStr);
        }
    }
}
