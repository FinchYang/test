using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Core.Domain
{
    /// <summary>
    /// 客户端的插件App
    /// </summary>
    public class Cloud : Entity
    {
        //private ICollection<VaultTemplate> _templates;
        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public bool Default { get; set; }

    }
}
