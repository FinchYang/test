using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Service.Users;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.Models;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Api
{
    [Authorize]
    public class AccountController : ErrorHandlingApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserService _userService;

        private readonly UserManager<User, long> _userManager;
        private IRepository<Company> _companyRepo;
        private IRepository<Department> _departmentRepo;

        public AccountController(IUserService userService, IRepository<Company> companyRepo, IRepository<Department> departmentRepo, UserManager<User, long> userManager
            , IAuthenticationManager authenticationManager)
            : base(authenticationManager)
        {
            _userService = userService;
            _userManager = userManager;
            _companyRepo = companyRepo;
            _departmentRepo = departmentRepo;
        }
        
        /// <summary>
        /// 获得当前用户的详细信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<UserDto> UserProfile()
        {
            var userId = User.Identity.GetUserId<long>();
            return Task.Run(() => UserProfile(userId));
        }
        /// <summary>
        /// 获取指定用户的详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public UserDto UserProfile(long id)
        {
            var user = _userManager.FindById(id);
            return user.ToDto();

        }

        public static UserDto GetUserProfile(UserManager<User, long> userManager, long userId)
        {
            var user = userManager.FindById(userId);
            return user.ToDto();
        }

        [HttpGet]
        public UserPrivate UserPrivate()
        {
            var userName = User.Identity.GetUserName();
            var userId = User.Identity.GetUserId<long>();
            var pass = DBWorldCache.Get(userId.ToString());
            return new UserPrivate { UserName = userName, Password = pass };
        }
        private static void SimpleLog(string logtext)
        {
            //var tmpfile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "log", DateTime.Now.Date.ToString("yyyy-MM-dd") + "ApiAccountControllerLog.xml");
            //try
            //{
            //    using (var sw = System.IO.File.AppendText(tmpfile))
            //    {
            //        sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
            //        sw.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("SimpleLog ApiAccountControllerLog-" + ex.Message, ex);
            //}
        }
        public static async Task<UserDto> UpdateProfile(UserProfileModel model, long userId, UserManager<User, long> _userManager, IUserService _userService
            , IRepository<Company> _companyRepo, IRepository<Department> _departmentRepo)
        {
            SimpleLog("in public static async Task<UserDto> UpdateProfile(UserProfileModel model, ");
            var user = await _userManager.FindByIdAsync(userId);
            SimpleLog("in 111");
            user.Email = model.Email;
            user.FullName = model.Name;
            user.Post = model.Post;
            user.Description = model.Description;
            user.Industry = model.Industry;
            user.Phone = model.Phone;
            user.QQ = model.QQ;
            user.Image = model.Image;
            SimpleLog("in 222");
            var com = _companyRepo.Table.FirstOrDefault(c => c.Name == model.Company);
            SimpleLog("in 333");
            if (com != null)
            {
                SimpleLog("in 444");
                user.Company = com;
            }
            SimpleLog("in 555");
            var depart = _departmentRepo.Table.FirstOrDefault(c => c.Name == model.Department);
            SimpleLog("in 666");
            if (depart != null)
            {
                SimpleLog("in 777");
                user.Department = depart;
            }
            SimpleLog("in 888");
            _userService.UpdateUser(user);
            SimpleLog("in 999");
            return user.ToDto();
        }

        /// <summary>
        /// 修改当前用户的详细信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> UserProfile(UserProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Identity.GetUserId<int>();

            var userDto = await UpdateProfile(model, userId, _userManager, _userService, _companyRepo, _departmentRepo);
            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IHttpActionResult> LoginInfo(LoginStatusModel model)
        {
            var userId = User.Identity.GetUserId<int>();
            await Task.Run(() => UpdateLoginInfo(model, userId, _userManager, _userService));
            return Ok();
        }

        public static void UpdateLoginInfo(LoginStatusModel model, long userId, UserManager<User, long> _userManager,
            IUserService _userService)
        {
            var user = _userManager.FindById(userId);
            user.LastIpAddress = model.Ip;
            var date = model.LoginDateUtc;
            if (date < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
            {
                date = DateTime.UtcNow;
            }
            user.LastLoginDateUtc = date;
            _userService.UpdateUser(user);
        }

    }
}
