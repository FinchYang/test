using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DBWorld.AecCloud.Web.Startup))]
namespace DBWorld.AecCloud.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }

    }
}
