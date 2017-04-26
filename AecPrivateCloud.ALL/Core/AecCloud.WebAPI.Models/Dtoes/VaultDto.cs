using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class MFilesUserGroupDto : EntityDto
    {
        public string Name { get; set; }

        public string Alias { get; set; }
    }
    public class VaultDto : EntityDto
    {

        public string Guid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTimeUtc { get; set; }

        /// <summary>
        /// vault所在的Server
        /// </summary>
        public VaultServerDto Server { get; set; }

        //public VaultTemplateDto Template { get; set; }
    }

    public class VaultTemplateDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool HasParty { get; set; }

        public string ImageUrl { get; set; }

        //public byte[] StructureFile { get; set; }
    }

    public class VaultServerDto : EntityDto
    {

        public string Ip { get; set; }

        public string Port { get; set; }

        public string LocalIp { get; set; }

        //public string DnsAlias { get; set; }

        //public string AdminName { get; set; }

        //public string AdminPwd { get; set; }
    }
}
