using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using AecCloud.Core.Domain;
using AecCloud.Service.Users;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using AecPasswordHasher = AecCloud.Core.AecPasswordHasher;

namespace DBWorld.AecCloud.Web
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class AecUserStore : IUserStore<User, long>, IUserPasswordStore<User, long>, //IUserEmailStore<User, long>,
        IUserLockoutStore<User, long>, IUserTwoFactorStore<User, long>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserService _userService;

        public AecUserStore(IUserService userService)
        {
            _userService = userService;
        }

        public Task CreateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            try
            {
                _userService.InsertUser(user);
                //_regService.RegisterUser(user);
            }
            catch (Exception ex)
            {
                Log.Error("创建用户失败: " + ex.Message, ex);
                throw;
            }
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            _userService.DeleteUser(user);
            return Task.FromResult<object>(null);
        }

        public Task<User> FindByIdAsync(long userId)
        {
            if (userId <= 0) throw new ArgumentException("userId");
            return Task.Run(() => _userService.GetUserById(userId));
        }

        public Task<User> FindByNameAsync(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName)) throw new ArgumentException("userName");
            return Task.Run(() => _userService.GetUserByAccountName(userName));
        }

        public Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            return Task.Run(() => _userService.UpdateUser(user));
        }

        public void Dispose()
        {

        }

        //public Task<User> FindByEmailAsync(string email)
        //{
        //    if (String.IsNullOrWhiteSpace(email))
        //    {
        //        throw new ArgumentException("email");
        //    }
        //    return Task.Run(() => _userService.GetUserByEmail(email));
        //}

        //public Task<string> GetEmailAsync(User user)
        //{
        //    if (user == null) throw new ArgumentNullException("user");
        //    return Task.FromResult(user.Email);
        //}

        //public Task<bool> GetEmailConfirmedAsync(User user)
        //{
        //    if (user == null) throw new ArgumentNullException("user");
        //    return Task.FromResult(user.Activated);
        //}

        //public Task SetEmailAsync(User user, string email)
        //{
        //    user.Email = email;
        //    _userService.UpdateUser(user);
        //    return Task.FromResult<object>(null);
        //}

        //public Task SetEmailConfirmedAsync(User user, bool confirmed)
        //{
        //    if (user == null) throw new ArgumentNullException("user");
        //    if (user.Activated != confirmed)
        //    {
        //        user.Activated = confirmed;
        //        _userService.UpdateUser(user);
        //    }
        //    return Task.FromResult<object>(null);
        //}

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.Run(() => _userService.UpdateUser(user));
        }

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            return Task.FromResult(0);
        }

    }

    public static class UserExtensions
    {
        public static Task<ClaimsIdentity> CreateUserIdentityAsync(this User user, UserManager<User, long> userManager)
        {
            return userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }

    public class AecSignInManager : SignInManager<User, long>
    {
        public AecSignInManager(UserManager<User, long> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.CreateUserIdentityAsync(UserManager);
        }

        public override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            //var user = UserManager.FindByNameAsync(userName);
            //user.Wait();
            //var ok = UserManager.CheckPasswordAsync(user.Result, password);
            //ok.Wait();
            //password will be hashed in UserManager using its PasswordHash Property
            return base.PasswordSignInAsync(userName, password, isPersistent, shouldLockout);
        }

    }

    public class AecUserManager : UserManager<User, long>
    {
        public AecUserManager(IUserStore<User, long> store)
            : base(store)
        {
            UserValidator = new UserValidator<User, long>(this)
            {
                AllowOnlyAlphanumericUserNames = false
                //, RequireUniqueEmail = true
            };

            PasswordHasher = new AecPasswordHasher();

            // Configure validation logic for passwords
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = Utility.MinimumLength,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            MaxFailedAccessAttemptsBeforeLockout = 5;

            var dataProtectionProvider = Startup.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                IDataProtector dataProtector = dataProtectionProvider.Create("ASP.NET Identity");

                UserTokenProvider = new DataProtectorTokenProvider<User, long>(dataProtector);
            }

        }
        /// <summary>
        /// FullnameClaim Key
        /// </summary>
        public static readonly string FullnameClaim = "fullname";

        /// <summary>
        /// DomainUserClaim Key
        /// </summary>
        public static readonly string UserDomain = "UserDomain";

        public override async Task<ClaimsIdentity> CreateIdentityAsync(User user, string authenticationType)
        {
            var id = await base.CreateIdentityAsync(user, authenticationType);
            if (user.FullName != user.UserName)
            {
                id.AddClaim(new Claim(FullnameClaim, user.FullName));
            }
            var userDomain = user.Domain ?? String.Empty;
            id.AddClaim(new Claim(UserDomain, userDomain.Trim()));
            return id;
        }

        public override Task<bool> CheckPasswordAsync(User user, string password)
        {
            try
            {
                //Convert.FromBase64String(password);
                return Task.FromResult(user.PasswordHash == password);
            }
            catch
            {
                return base.CheckPasswordAsync(user, password);
            }
        }

        public override async Task<User> FindAsync(string userName, string password)
        {
            try
            {
                //Convert.FromBase64String(password);
                User local;
                User user = await FindByNameAsync(userName);
                if (user == null)
                {
                    local = default(User);
                }
                else
                {
                    bool ok = await CheckPasswordAsync(user, password);
                    local = ok ? user : default(User);
                }
                return local;

            }
            catch
            {

            }
            return await base.FindAsync(userName, password);
        }
    }
}
