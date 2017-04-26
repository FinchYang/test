using System.Reflection;
using log4net;

namespace DBWorld.MailCore.Common
{
    public static class Logger
    {
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Configure()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
