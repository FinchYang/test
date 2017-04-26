using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AecCloud.Core.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(
            UserManager<User, long> userManager, SignInManager<User, long> signInManager, IAuthenticationManager authManager)
            : base(authManager, signInManager, userManager)
        {
            //_userManager = userManager;
            //_signInManager = signInManager;

        }
        // GET: Home
        public ActionResult Index()
        {
            return RedirectToAction("Welcome", new { controller = "Manage" });//RedirectToAction("Index", new {controller = "BIM"})
            //if (!User.Identity.IsAuthenticated)//&& AuthUtility.IsCookieAuthenticated() 
            //{
            //    var userName = AuthUtility.GetUserName(User);
            //    var userId = AuthUtility.GetUserId(User);
            //    await Signin(userName, userId.ToString(), true);
            //}
            //return View();
        }
    }
}