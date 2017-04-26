using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace AecCloud.Core.Domain
{
    public class User : Entity, IUser<long>
    {
        private ICollection<UserRole> _roles;

        public User()
        {
            MaxProjectCount = 0;
            CreatedTimeUtc = DateTime.UtcNow;
            LastActivityDateUtc = DateTime.UtcNow;
            CscecRoleId = 1;
            PersonnelCategoryId = 1;
            PositionInfoId = 1;
            Phone = string.Empty;
        }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PasswordHash { get; set; }

        public string PrivateKey { get; set; }
        /// <summary>
        /// 是否被禁用
        /// </summary>
        public bool Disabled { get; set; }

        public int MaxProjectCount { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        /// <summary>
        /// 用户的角色列表
        /// </summary>
        public virtual ICollection<UserRole> Roles
        {
            get { return _roles ?? (_roles = new List<UserRole>()); }
            protected set { _roles = value; }
        }

        public string Domain { get; set; }
        /// <summary>
        /// 是否是域账户
        /// </summary>
        public bool DomainUser { get; set; }

        public string GetAccountName()
        {
            if (DomainUser)
            {
                return Domain + "\\" + UserName;
            }
            return UserName;
        }

        /// <summary>
        /// 单位
        /// </summary>
        public long CompanyId { get; set; }
        
        public virtual Company Company { get; set; } //单位

        public long CscecRoleId { get; set; }

        public virtual CscecRole CscecRole { get; set; } //单位


        public long PersonnelCategoryId { get; set; }

        public virtual PersonnelCategory PersonnelCategory { get; set; } //单位

        public long PositionInfoId { get; set; }

        public virtual PositionInfo PositionInfo { get; set; } //单位
        /// <summary>
        /// 部门
        /// </summary>
        public long DepartmentId { get; set; }

        public virtual Department Department { get; set; } //部门
        public string Post { get; set; } //职位

        public string FullName { get; set; } //姓名
        public byte[] Image { get; set; } //头像
        public string Phone { get; set; } //电话
        public string QQ { get; set; } //QQ
        public string Industry { get; set; } //行业


        public string Description { get; set; } //个人描述

    }
}
