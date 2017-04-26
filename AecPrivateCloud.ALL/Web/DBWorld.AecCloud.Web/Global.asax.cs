using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DBWorld.AecCloud.Web
{
    public class Global : System.Web.HttpApplication
    {
        internal static string ServerPath;
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Logger.Configure();
            if (ServerPath == null)
            {
                ServerPath = Server.MapPath("~");
            }
            var appdataFolder = Server.MapPath("~/App_Data/");
            EfConfig.Initialize(appdataFolder);
            ModelUtility.SetRootFolder(ServerPath, "ModelsRoot");
            
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            
        }

        protected void Application_Error(object sender, EventArgs e)
        {

            var err = Server.GetLastError();
            Logger.Log.Error(err.Message, err);
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}