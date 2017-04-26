using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using log4net;

namespace DBWorld.AecCloud.Web
{
    public class Logger
    {
        public static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Configure()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}