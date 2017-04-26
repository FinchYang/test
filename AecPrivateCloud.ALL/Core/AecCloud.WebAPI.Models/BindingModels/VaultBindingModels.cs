using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class UpdateInfo
    {
        public UpdateInfo()
        {
            Name = string.Empty;
            Date = string.Empty;
            FileContent = new byte[1];
        }
        public string Name { set; get; }
        public string Date { set; get; }
        public byte[] FileContent { set; get; }
    }
    public class VaultAppModel
    {
        public VaultAppModel()
        {
            ZipFile=new byte[1];
        }
        public bool IsUpdate { get; set; }
        public long AppId { get; set; }
        public string Guid { get; set; }
        public byte[] ZipFile { get; set; }

        public string Version { get; set; }

        //public bool CloudAppEnabled { get; set; }
    }

    public class AppDescList
    {
        public AppDescList()
        {
            Apps = new List<AppDesc>();
        }
        public List<AppDesc> Apps { get; set; }
    }

    public class AppDesc
    {
        public string Guid { get; set; }

        public string Version { get; set; }
    }
}
