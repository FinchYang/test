using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using AecCloud.Core.Domain;
using AecCloud.Core;

namespace AecCloud.Service.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserRole> _roleRepo;

        private readonly IRepository<Company> _companyRepo;

        private readonly IRepository<Department> _departmentRepo;

        private readonly IRepository<CscecRole> _cRoleRepo;

        private readonly IRepository<PersonnelCategory> _cateRepo;

        private readonly IRepository<PositionInfo> _positionRepo;

        public UserService(IRepository<User> userRepo, IRepository<UserRole> roleRepo,
            IRepository<Company> companyRepo, IRepository<Department> departmentRepo,
            IRepository<CscecRole> cRoleRepo, IRepository<PersonnelCategory> cateRepo,
            IRepository<PositionInfo> positionRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _companyRepo = companyRepo;
            _departmentRepo = departmentRepo;
            _cRoleRepo = cRoleRepo;
            _cateRepo = cateRepo;
            _positionRepo = positionRepo;
        }

        public IList<User> GetAllUsers()
        {
            return _userRepo.Table.Include(c=>c.Roles).ToList(); //InClude(c=>c.Roles)
        }

        public User GetUserById(long userId)
        {
            if (userId <= 0) return null;
            var user = _userRepo.GetById(userId);
            AddOthers(user);
            return user;
        }

        private void AddOthers(User user)
        {
            if (user == null) return;
            if (user.CompanyId > 0)
            {
                user.Company = _companyRepo.GetById(user.CompanyId);
            }
            if (user.DepartmentId > 0)
            {
                user.Department = _departmentRepo.GetById(user.DepartmentId);
            }
        }

        public User GetUserByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");
            email = email.ToUpper();
            var user = _userRepo.Table.Include(c => c.Roles).FirstOrDefault(c => c.Email.ToUpper() == email);
            AddOthers(user);
            return user;
        }

        public User GetUserByAccountName(string accountName)
        {
            if (String.IsNullOrWhiteSpace(accountName)) throw new ArgumentException("accountName");
            var userName = accountName.ToUpper();
            var user = _userRepo.Table.Include(c => c.Roles).FirstOrDefault(
                c => c.UserName.ToUpper() == userName);
            AddOthers(user);
            return user;
        }

        public IList<User> GetOnlineUsers(DateTime lastActivityDateUtc)
        {
            return _userRepo.Table.Where(c => c.LastActivityDateUtc >= lastActivityDateUtc).ToList();
        }

        public void DeleteUser(User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _userRepo.Delete(user);
        }

        public void InsertUser(User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            CheckUser(user);
            _userRepo.Insert(user);
        }

        public void UpdateUser(User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            CheckUser(user);
            _userRepo.Update(user);
        }

        protected virtual void CheckUser(User user)
        {
            //if (user.Email != null) user.Email = user.Email.ToUpper();
        }

        public IList<UserRole> GetUserRoles()
        {
            return _roleRepo.Table.ToList();
        }

        public UserRole GetUserRoleById(long roleId)
        {
            if (roleId <= 0) return null;
            return _roleRepo.GetById(roleId);
        }

        public void DeleteUserRole(UserRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            _roleRepo.Delete(role);
        }

        public void InsertUserRole(UserRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            _roleRepo.Insert(role);
        }

        public void UpdateUserRole(UserRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            _roleRepo.Update(role);
        }


        public UserRole GetUserRoleByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("rolename");
            return _roleRepo.Table.FirstOrDefault(c => c.Name == name);
        }


        public CscecRole GetDefaultRole()
        {
            return _cRoleRepo.Table.FirstOrDefault();
        }


        public PersonnelCategory GetDefaultCategory()
        {
            return _cateRepo.Table.FirstOrDefault();
        }


        public PositionInfo GetDefaultPosition()
        {
            return _positionRepo.Table.FirstOrDefault();
        }
    }
}
