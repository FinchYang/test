using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{

    public class ActiveDirectoryDto : EntityDto
    {
        ///// <summary>
        ///// AD域管理员用户名
        ///// </summary>
        //public string AdminName { get; set; }
        ///// <summary>
        ///// AD域管理员密码
        ///// </summary>
        //public string AdminPwd { get; set; }
        ///// <summary>
        ///// AD域LDAP根路径
        ///// #LDAP://192.168.2.189#格式
        ///// </summary>
        //public string LDAPRoot { get; set; }
        ///// <summary>
        ///// AD域DC路径
        ///// #LDAP://192.168.2.189/DC=simuladesign,DC=com中DC=simuladesing,DC=com的部分字符串#
        ///// #DC不区分大小写#
        ///// </summary>
        //public string DCInfo { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }
    }

    public class UserRoleDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }
    }

    public class PermissionLevelDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
