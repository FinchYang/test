using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace MfMsmqCom
{
    public class LocalConfig
    {
        public static string GetComputerFullName()
        {
            //return ConfigurationManager.AppSettings["ComputerName"];
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            return appConfig.AppSettings.Settings["ComputerName"].Value;
        }
    }
}
