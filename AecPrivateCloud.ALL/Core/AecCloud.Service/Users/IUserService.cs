using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Service.Users
{
    public interface IUserService
    {
        IList<User> GetAllUsers();

        User GetUserById(long userId);

        User GetUserByEmail(string email);

        User GetUserByAccountName(string accountName);

        IList<User> GetOnlineUsers(DateTime lastActivityDateUtc);

        void DeleteUser(User user);

        void InsertUser(User user);

        void UpdateUser(User user);

        IList<UserRole> GetUserRoles();

        UserRole GetUserRoleById(long roleId);

        UserRole GetUserRoleByName(string name);

        void DeleteUserRole(UserRole role);

        void InsertUserRole(UserRole role);

        void UpdateUserRole(UserRole role);

        CscecRole GetDefaultRole();


        PersonnelCategory GetDefaultCategory();

        PositionInfo GetDefaultPosition();

    }
}
