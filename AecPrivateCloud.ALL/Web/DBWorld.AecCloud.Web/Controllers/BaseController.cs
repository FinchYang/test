using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AecCloud.BaseCore;
using AecCloud.Core.Domain;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.ApiRequests;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected BaseController(IAuthenticationManager authManager,
            SignInManager<User, long> signInManager, UserManager<User, long> userManager)
        {
            _authManager = authManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }


        protected bool IsPasswordAvailable()
        {
            var pass = GetPassword();
            return !String.IsNullOrEmpty(pass);
        }

        protected ActionResult ReloginForCurrentUser()
        {
            Signout(true);
            return RedirectToAction("LogOn", "Account", new { returnUrl = Request.Url.AbsolutePath });
        }
        protected ActionResult ReturnUserHome()
        {
            return RedirectToAction("Index", "BIM");
        }

        protected void Signout(bool removeToken)
        {
            _authManager.SignOut();
            //if (removeToken) RemoveToken();
        }

        private readonly IAuthenticationManager _authManager;
        protected readonly SignInManager<User, long> _signInManager;
        protected readonly UserManager<User, long> _userManager;


        protected string GetUserName()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.Identity.GetUserName();
            }
            return null;
        }

        protected string GetPassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            return DBWorldCache.Get(User.Identity.GetUserId<long>().ToString());
        }
        
        protected UserDto GetUserProfile(UserManager<User, long> userManager)
        {
            return GetUserProfile(_userManager, User.Identity.GetUserId<long>());
        }

        protected UserDto GetUserProfile(UserManager<User, long> userManager, long userId)
        {
            return Api.AccountController.GetUserProfile(_userManager, userId);
        }
    }
}