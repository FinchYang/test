using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class UserDto : EntityDto
    {
        private ICollection<UserRoleDto> _roles;


        /// <summary>
        /// 用户帐户名，唯一
        /// </summary>
        public string UserName { get; set; }


        public string Email { get; set; }

        public string Password { get; set; }


        #region UserInfo



        #endregion UserInfo

        ///// <summary>
        ///// Gets or sets the last IP address
        ///// </summary>
        //public string LastIpAddress { get; set; }

        ///// <summary>
        ///// Gets or sets the date and time of entity creation
        ///// </summary>
        //public DateTime CreatedTimeUtc { get; set; }

        ///// <summary>
        ///// Gets or sets the date and time of last login
        ///// </summary>
        //public DateTime? LastLoginDateUtc { get; set; }

        ///// <summary>
        ///// Gets or sets the date and time of last activity
        ///// </summary>
        //public DateTime LastActivityDateUtc { get; set; }
        /// <summary>
        /// 用户的角色列表
        /// </summary>
        public ICollection<UserRoleDto> Roles
        {
            get { return _roles ?? (_roles = new List<UserRoleDto>()); }
            protected set { _roles = value; }
        }

        public string Domain { get; set; }


        public string FullName { get; set; }

        public byte[] Image { get; set; }
        /// <summary>
        /// 个人说明：
        /// </summary>
        public string Description { get; set; }
        public string Phone { get; set; }
        public string QQ { get; set; }

        /// <summary>
        /// 行业：
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 单位：
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 部门：
        /// </summary>
        public string Department { get; set; }

        public string Post { get; set; }
    }

    
}
