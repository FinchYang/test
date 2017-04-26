using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace AecCloud.Core.Domain
{
    /// <summary>
    /// 用户角色，按照角色授权
    /// </summary>
    public class UserRole : Entity, IRole<long>
    {
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        public string Description { get; set; }

    }
}
