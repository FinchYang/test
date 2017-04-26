using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.CloudServices
{
    /// <summary>
    /// Citrix云应用服务
    /// </summary>
    public interface ICitrixAppService
    {
        IList<CloudApplication> GetAllCloudApp(string url);
    }
}
