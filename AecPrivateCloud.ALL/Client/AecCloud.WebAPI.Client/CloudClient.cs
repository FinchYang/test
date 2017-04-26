using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using AecCloud.WebAPI.Models;

namespace AecCloud.WebAPI.Client
{
    public class CloudClient
    {
        private readonly HttpClient _client;
        private readonly string _routePrefix = "api/Cloud";

        internal CloudClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> DelClouds(UserCloudModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/DelClouds/", model);
        }

        public Task<HttpResponseMessage> GetCompanies(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Companies/");
        }

        public Task<HttpResponseMessage> GetAreas(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Areas/");
        }

        public Task<HttpResponseMessage> GetClouds(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Clouds/");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="clouds">App的ID列表</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> LoadClouds(TokenModel token, params int[] clouds)
        {
            if (clouds == null || clouds.Length == 0) throw new ArgumentException("clouds");
            TokenClient.RefreshToken(_client, token);
            var model = new UserCloudModel();
            model.Apps.AddRange(clouds.Select(c => new CloudModel { App = new CloudDto{Id=c}}));
            return LoadClouds(model);
        }

        internal Task<HttpResponseMessage> LoadClouds(UserCloudModel model)
        {
            return _client.PostAsJsonAsync(_routePrefix + "/Clouds/", model);
        }

        public Task<HttpResponseMessage> GetTemplates(int appId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Templates/" + appId);
            //return _client.GetAsync(_routePrefix + "/Templates/" + "?app=" + appId);
        }

        public Task<HttpResponseMessage> GetAllClouds(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/AllClouds");
        }
    }
}
