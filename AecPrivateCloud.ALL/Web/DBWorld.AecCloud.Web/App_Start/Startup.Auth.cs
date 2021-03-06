﻿using System;
using System.Web.Http;
using DBWorld.AecCloud.Web.Providers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace DBWorld.AecCloud.Web
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        // add this static variable
        internal static IDataProtectionProvider DataProtectionProvider { get; private set; }

        public static string PublicClientId { get; private set; }

        // 有关配置身份验证的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            DataProtectionProvider = app.GetDataProtectionProvider();

            var config = GlobalConfiguration.Configuration;
            
            AutofacConfig.Initialize(config, app);
            

            // 配置数据库上下文、用户管理器和登录管理器，以便为每个请求使用单个实例

            // 使应用程序可以使用 Cookie 来存储已登录用户的信息
            // 并使用 Cookie 来临时存储有关使用第三方登录提供程序登录的用户的信息
            // 配置登录 Cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/LogOn")
                //Provider = new CookieAuthenticationProvider
                //{
                //    // 当用户登录时使应用程序可以验证安全戳。
                //    // 这是一项安全功能，当你更改密码或者向帐户添加外部登录名时，将使用此功能。
                //    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                //        validateInterval: TimeSpan.FromMinutes(30),
                //        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                //}
                //,Provider = new CookieAuthenticationProvider { OnApplyRedirect = ApplyRedirect }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.UseAutofacWebApi(config);


            // 针对基于 OAuth 的流配置应用程序
            PublicClientId = "self";
            //OAuthOptions = new OAuthAuthorizationServerOptions
            //{
            //    TokenEndpointPath = new PathString("/Token"),
            //    Provider = new DBWorldOAuthProvider(PublicClientId),
            //    AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
            //    AllowInsecureHttp = true
            //};

            //// 使应用程序可以使用不记名令牌来验证用户身份
            //app.UseOAuthBearerTokens(OAuthOptions);
            IdentityAuth.ConfigureOAuthServer(app, new DBWorldOAuthProvider(PublicClientId));

            //// 使应用程序可以在双重身份验证过程中验证第二因素时暂时存储用户信息。
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            //// 使应用程序可以记住第二登录验证因素，例如电话或电子邮件。
            //// 选中此选项后，登录过程中执行的第二个验证步骤将保存到你登录时所在的设备上。
            //// 此选项类似于在登录时提供的“记住我”选项。
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // 取消注释以下行可允许使用第三方登录提供程序登录
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});

            //app.UseWebApi(config);
        }
        //http://stackoverflow.com/questions/21275399/login-page-on-different-domain
        //private static void ApplyRedirect(CookieApplyRedirectContext context)
        //{
        //    Uri absoluteUri;
        //    if (Uri.TryCreate(context.RedirectUri, UriKind.Absolute, out absoluteUri))
        //    {
        //        var ssoLoginUrl = AuthUtility.GetSSOLoginUri();
        //        var path = PathString.FromUriComponent(absoluteUri);
        //        if (path == context.OwinContext.Request.PathBase + context.Options.LoginPath)
        //            context.RedirectUri = ssoLoginUrl +
        //                new QueryString(
        //                    context.Options.ReturnUrlParameter,
        //                    context.Request.Uri.ToString());
        //    }

        //    context.Response.Redirect(context.RedirectUri);
        //}
    }

    public class IdentityAuth
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static readonly string OAuthType = DefaultAuthenticationTypes.ExternalBearer;

        private static ISecureDataFormat<AuthenticationTicket> GetTokenTicketFormat(IAppBuilder app)
        {
            var tokenDataProtector = app.CreateDataProtector(
                typeof(OAuthBearerAuthenticationMiddleware).Namespace,
                "Access_Token", "v1");
            var tokenTicketFormat = new TicketDataFormat(tokenDataProtector);

            return tokenTicketFormat;
        }
        /// <summary>
        /// OAuth Token Producer
        /// </summary>
        public static void ConfigureOAuthServer(IAppBuilder app, IOAuthAuthorizationServerProvider provider)
        {
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                AuthenticationType = OAuthType,
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenFormat = GetTokenTicketFormat(app),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };
            if (provider != null)
            {
                OAuthOptions.Provider = provider;
            }

            app.UseOAuthBearerTokens(OAuthOptions);
        }
        public static AuthenticationTicket GetTicketFromToken(string token)
        {
            if (OAuthOptions == null) return null;
            try
            {
                return OAuthOptions.AccessTokenFormat.Unprotect(token);
            }
            catch { }
            return null;
        }
    }
}