using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    /// <summary>
    /// 库实例表
    /// </summary>
    public class MFilesVault : Entity
    {
        public MFilesVault()
        {
            CreatedTimeUtc = DateTime.UtcNow;
        }
        public long TemplateId { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateVersion { get; set; }
        public long CloudId { get; set; }
        public long ServerId { get; set; }
        public DateTime CreatedTimeUtc { get; set; }
        /// <summary>
        /// 服务器上的存储路径
        /// </summary>
        public string ServerPath { get; set; }

        public string SqlConnectionString { get; set; }

        public bool Default { get; set; }
        /// <summary>
        /// vault所在的Server
        /// </summary>
        public virtual VaultServer Server { get; set; }
    }
}
