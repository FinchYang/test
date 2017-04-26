using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.MfilesServices;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Autofac.Integration.Owin;
using Autofac;

namespace DBWorld.AecCloud.Web.Providers
{
    public class DBWorldOAuthProvider : OAuthAuthorizationServerProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _publicClientId;

        public DBWorldOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public static readonly string DomainKey = "domain";

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var isUserId = false; //是否Password是UserId
            var headers = context.Request.Headers;
            var isDomainUser = true;
            if (headers.ContainsKey(DomainKey))
            {
                var v = headers.GetValues(DomainKey).FirstOrDefault();
                if (v != null)
                {
                    var notDomain = String.Equals(false.ToString(), v, StringComparison.CurrentCultureIgnoreCase);
                    if (notDomain)
                    {
                        isDomainUser = false;
                    }
                }
            }
            else
            {
                var qs = context.Request.Query;
                if (qs != null)
                {
                    var domain = qs.Any(c => c.Key.ToUpper() == "DOMAIN");
                    if (domain)
                    {
                        var v = qs.FirstOrDefault(c => c.Key.ToUpper() == "DOMAIN").Value.FirstOrDefault();
                        var notDomain = String.Equals(false.ToString(), v, StringComparison.CurrentCultureIgnoreCase);
                        if (notDomain)
                        {
                            isDomainUser = false;
                        }
                    }
                }
            }

            var scope = context.OwinContext.GetAutofacLifetimeScope();
            var userManager = scope.Resolve<UserManager<User, long>>();
            var vaultService = scope.Resolve<IVaultServerService>();
            var mfuserService = scope.Resolve<IMFUserService>();
            var userService = scope.Resolve<IUserService>();
            var res = await Task.Run(() => AuthUtility.Login(context.UserName, context.Password, isDomainUser, userManager
                , vaultService, mfuserService, userService));
            if (!string.IsNullOrEmpty(res.Error))
            {
                context.SetError(res.Error);
                return;
            }
            var user = res.User;

            var oAuthIdentity = await userManager.CreateIdentityAsync(user,
               OAuthDefaults.AuthenticationType);
            var cookiesIdentity = await userManager.CreateIdentityAsync(user,
                CookieAuthenticationDefaults.AuthenticationType);
            var data = new Dictionary<string, string>
            {
                {"userName", user.UserName},
                {"disabled", user.Disabled.ToString()},
                {"email", user.Email}
            };

            var properties = CreateProperties(data);
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
            user.LastIpAddress = context.Request.RemoteIpAddress;
            user.LastLoginDateUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            //if (!isUserId)
            //{
            DBWorldCache.Add(user.Id.ToString(), context.Password);
            //}
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // 资源所有者密码凭据未提供客户端 ID。
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(IDictionary<string, string> data, bool isPersistent = false)
        {
            return new AuthenticationProperties(data) { IsPersistent = isPersistent };
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}