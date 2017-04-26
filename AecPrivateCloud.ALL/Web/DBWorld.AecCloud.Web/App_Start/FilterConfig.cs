using System.Web;
using System.Web.Mvc;

namespace DBWorld.AecCloud.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}