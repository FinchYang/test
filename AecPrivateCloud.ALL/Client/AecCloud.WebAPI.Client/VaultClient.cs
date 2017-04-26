using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using AecCloud.WebAPI.Models;
using Newtonsoft.Json;

namespace AecCloud.WebAPI.Client
{
    public class VaultClient
    {
        private readonly HttpClient _client;
        private readonly string _routePrefix = "api/Vault";

        internal VaultClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> GetCloudDiscApps(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/CloudDisc");
        }

        public Task<HttpResponseMessage> CloudDiscAppsNeeded(AppDescList apps, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/CloudDiscAppNeeded", apps);
        }

        public Task<HttpResponseMessage> GetApps(long vaultId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Apps/" + vaultId);
        }

        public Task<HttpResponseMessage> AppsNeeded(long vaultId, AppDescList apps, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/AppsNeeded/" + vaultId, apps);
        }
        //[Obsolete]
        //public Task<HttpResponseMessage> GetNeededApps(int vaultId, TokenModel token)
        //{
        //    TokenClient.RefreshToken(_client, token);
        //    return _client.GetAsync(_routePrefix + "/NeededApps/"+vaultId);
        //}

        //public Task<HttpResponseMessage> UpdateLoadedAppVersion(int vaultId, int vaultappId, TokenModel token)
        //{
        //    TokenClient.RefreshToken(_client, token);
        //    return _client.PostAsync(_routePrefix + 
        //        String.Format("/UpdateLoadedAppVersion?vaultId={0}&vaultappId={1}", vaultId, vaultappId), null);
        //}
    }
}
