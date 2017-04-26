using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.CloudServices
{
    /// <summary>
    /// Citrix用户服务
    /// </summary>
    public interface ICitrixUserService
    {
        void CreateCitrixUserAndApp(User user,string url,IList<CloudApplication> appList);
    }
}
