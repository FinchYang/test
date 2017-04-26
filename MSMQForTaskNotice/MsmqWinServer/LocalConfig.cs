using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace MsmqWinService
{
    public class LocalConfig
    {
        public static ServerAdminUser GetAdminUser()
        {
            var user = new ServerAdminUser();
            user.ServerIp = ConfigurationManager.AppSettings["MFilesServer"];
            user.Name = ConfigurationManager.AppSettings["UserName"];
            user.Pwd = ConfigurationManager.AppSettings["UserPwd"];
            user.AccountType =(MFLoginAccountType)Enum.Parse(typeof(MFLoginAccountType), 
                ConfigurationManager.AppSettings["AccountType"]);
            return user;
        }
        public static int GetSleepTime()
        {
            var timeStr = ConfigurationManager.AppSettings["SleepTime"];
            var time = 10 * 1000;
            if (string.IsNullOrEmpty(timeStr) || string.IsNullOrWhiteSpace(timeStr)) return time;
            int outTime;
            if (int.TryParse(timeStr, out outTime)) time = outTime * 1000;
            return time;
        }

        public static string GetComputerFullName()
        {
            return ConfigurationManager.AppSettings["ComputerName"];
        }
    }

    public class ServerAdminUser
    {
        /// <summary>
        /// 登陆账户类型
        /// </summary>
        public MFLoginAccountType AccountType { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// MFiles服务器
        /// </summary>
        public string ServerIp { get; set; }
    }
}
