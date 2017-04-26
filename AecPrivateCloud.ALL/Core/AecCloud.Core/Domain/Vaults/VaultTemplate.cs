using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    /// <summary>
    /// 库模板表
    /// </summary>
    public class VaultTemplate : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// 1、备份时放存储目录，形如 D:\ttt
        /// 2、全备份建库存储全备份文件全路径，形如 D:\ttt\1.mfb
        /// 3、导出数据和结构建库存储导入路径，形如 D:\ttt\{6F0AB68C-2A32-44D1-A39D-32D9F8F433F2}_20150120_153050
        /// </summary>
        public string StructurePath { get; set; }

        public string MetadataJson { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

    }
}
