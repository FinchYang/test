using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    public class VaultApp : Entity
    {
        /// <summary>
        /// 从App包的appdef.xml中获取
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 从App包的appdef.xml中获取
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 从App包的appdef.xml中获取
        /// </summary>
        public string Description { get; set; }
        ///// <summary>
        ///// 需要先设置此路径
        ///// </summary>
        //public byte[] File { get; set; }

        public string Filepath { get; set; }
        /// <summary>
        /// 每次重新发布App时，+1
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 从App包的appdef.xml中获取
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// 是否为通用App，即：与库的自定义元数据无关
        /// </summary>
        public bool IsUniversal { get; set; }

        public string Url { get; set; }

        //public bool CloudAppEnabled { get; set; }

    }
}
