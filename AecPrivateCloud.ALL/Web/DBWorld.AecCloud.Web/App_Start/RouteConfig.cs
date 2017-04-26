using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DBWorld.AecCloud.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ModelUpload",
                url: "Model/Upload",
                defaults: new { controller = "Model", action = "Upload" }
            );

            routes.MapRoute(
                name: "FilesDownload",
                url: "Files/Download",
                defaults: new { controller = "Files", action = "Download" }
            );

            routes.MapRoute(
                name: "Files",
                url: "Files/{id}",
                defaults: new { controller = "Files", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "BIM", action = "Index", id = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { id = UrlParameter.Optional }
            //);
        }
    }
}