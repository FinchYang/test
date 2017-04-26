using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Service.Users
{
    public static class UserExtension
    {
        public static bool IsRegistered(this User user)
        {
            return IsInRole(user, SystemUserRoleNames.Registered);
        }

        public static bool IsInRole(this User user, string roleName)
        {
            if (String.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("roleName");

            var res = user.Roles.FirstOrDefault(c => c.Name == roleName) != null;
            return res;
        }
    }
}
