using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DBWorld.AecCloud.Web.ApiRequests
{
    public class DomainClient
    {
        public static readonly string Domain = ConfigurationManager.AppSettings["Domain"];
        //private static readonly string Host = ConfigurationManager.AppSettings["domainapi"];
        //public static async Task<bool> HasADUser(long userId)
        //{
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.GetAsync("api/Domain/User/" + userId);
        //        return res.StatusCode == HttpStatusCode.OK;
        //    }
        //}

        //public static async Task<ResponseModel> CreateUser(CreateUserModel model)
        //{
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.PostAsJsonAsync("api/Domain/CreateUser", model);
        //        return await res.GetResponse();
        //    }
        //}

        //public static async Task<ResponseModel> GetDomain(long userId)
        //{
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.GetAsync("api/Domain/Domain?userId=" + userId);
        //        return await res.GetResponse();
        //    }
        //}

        //public static async Task<ResponseModel> ChangeUserInfo(ChangeInfoModel model)
        //{
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.PostAsJsonAsync("api/Domain/ChangeInfo", model);
        //        return await res.GetResponse();
        //    }
        //}

        //public static async Task<ResponseModel> SetPassword(long userId, string newPassword)
        //{
        //    var model = new DomainSetPasswordModel { UserId = userId, Password = newPassword };
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.PostAsJsonAsync("api/Domain/SetPassword", model);
        //        return await res.GetResponse();
        //    }
        //}

        //public static async Task<ResponseModel> GetDefaultDomain()
        //{
        //    using (var client = TokenClient.GetClientWithBaseUri(Host))
        //    {
        //        var res = await client.GetAsync("api/Domain/DomainName");
        //        return await res.GetResponse();
        //    }
        //}
    }

    public class ChangeInfoModel
    {
        public long UserId { get; set; }
        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; }

        public string Email { get; set; }
    }

    public class DomainSetPasswordModel
    {
        public long UserId { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserModel
    {

        public long UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; } //全名

        public string Email { get; set; }
    }
}