using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;
using Newtonsoft.Json;

namespace AecCloud.WebAPI.Client
{
    public class AuthenticationClient
    {
        private readonly HttpClient _client;
        private readonly string _routePrefix = "api/Account";

        internal AuthenticationClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> UserRegister(string userName, string email, string password)
        {
            var model = new RegisterBindingModel { UserName = userName, Email = email, Password = password, ConfirmPassword = password };
            return _client.PostAsJsonAsync(_routePrefix+"/Register", model);

        }

        public Task<HttpResponseMessage> LoginInfo(LoginStatusModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/LoginInfo", model);
        }

        public Task<HttpResponseMessage> UserLogout(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/Logout", null);
        }

        public Task<HttpResponseMessage> UserActivate(string email)
        {
            var model = new EmailActivateModel { Email = email };
            return _client.PostAsJsonAsync(_routePrefix + "/EmailActivate", model);
        }

        public Task<HttpResponseMessage> SendActivateEmail(SendEmailMessage model)
        {
            return _client.PostAsJsonAsync(_routePrefix + "/SendActivateEmail", model);
        }

        public Task<HttpResponseMessage> SendForgotPasswordEmail(SendEmailMessage model)
        {
            return _client.PostAsJsonAsync(_routePrefix + "/SendForgotPasswordEmail", model);
        }

        public Task<HttpResponseMessage> SendActivateCodeEmail(SendEmailMessage model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/SendActivateCodeEmail", model);
        }

        public Task<HttpResponseMessage> GetUserProfile(int userId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/UserProfile/" + userId);
        }
        public Task<HttpResponseMessage> GetUserProfile(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/UserProfile/");
        }

        public Task<HttpResponseMessage> ChangeUserProfile(UserProfileModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/UserProfile", model);
        }
        

        public Task<HttpResponseMessage> ChangePassword(string oldPassword, string newPassword, TokenModel token)
        {
            var model = new ChangePasswordModel
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/ChangePassword", model);
        }

        public Task<HttpResponseMessage> SetPassword(string email, string newPassword)
        {
            var model = new SetPasswordModel
            {
                Email = email,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };
            return _client.PostAsJsonAsync(_routePrefix + "/SetPassword", model);
        }
    }
}
