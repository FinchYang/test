using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesServices
{
    public class MFSqlDatabase
    {
        public string Server { get; set; }

        public bool SqlserverUser { get; set; }

        public string AdminUserName { get; set; }

        public string AdminPassword { get; set; }

        public string Catelog { get; set; }

        public string ToConnectionString()
        {
            return String.Format("Data Source={0};Initial Catalog={1};user id={2};password={3}", Server, Catelog, AdminUserName, AdminPassword);
        }
    }
}
