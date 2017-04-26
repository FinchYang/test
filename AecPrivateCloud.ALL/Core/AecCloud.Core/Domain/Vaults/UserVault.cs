using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    /// <summary>
    /// 用户与库的实例关系表
    /// </summary>
    public class UserVault : Entity
    {
        public long UserId { get; set; }

        public long VaultId { get; set; }

        public bool UserIsCreator { get; set; }

        //public virtual MFilesVault Vault { get; set; }
    }
}
