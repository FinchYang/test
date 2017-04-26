using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain
{
    
    /// <summary>
    /// 客户端的通知
    /// </summary>
    public class Tasks : Entity
    {
       
        public string Name { get; set; }

        public string CreationTime { get; set; }
        public string Userid { get; set; }
        public int IsNoticed { get; set; }
        public int Notificationtype { get; set; }
        public int Objectid { get; set; }
        public int Type { get; set; }
        public int Version { get; set; }
        public string Url { get; set; }
        public string Vaultguid { get; set; }

     

    }
}
