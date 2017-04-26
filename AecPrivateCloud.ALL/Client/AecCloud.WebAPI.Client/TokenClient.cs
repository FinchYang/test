using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using AecCloud.WebAPI.Models;
using Newtonsoft.Json;

namespace AecCloud.WebAPI.Client
{
    public class TokenClient
    {
        private readonly HttpClient _client;
        private readonly string _routePrefix = "/Token";

        internal TokenClient(HttpClient client)
        {
            _client = client;
        }

        internal static void RefreshToken(HttpClient client, TokenModel token)
        {
            IEnumerable<string> values;
            if (client.DefaultRequestHeaders.TryGetValues(AuthHeaderKey, out values))
            {
                client.DefaultRequestHeaders.Remove(AuthHeaderKey);
            }
            client.DefaultRequestHeaders.Add(AuthHeaderKey, token.TokenType + " " + token.AccessToken);
        }

        private const string AuthHeaderKey = "Authorization";

        public async Task<TokenModel> GetToken(string userName, string password, bool domainUser=true)
        {
            //password = AecCloudCrypto.HashPassword(password);
            var dict = new Dictionary<string, string>
                {
                    {"UserName", userName},
                    {"Password", password},
                    {"grant_type", "password"}
                };
            var m = new FormUrlEncodedContent(dict);
            var obj = await _client.PostAsync(_routePrefix + "?domain=" + domainUser, m);
            var resContent = await obj.Content.ReadAsStringAsync();
            if (!obj.IsSuccessStatusCode)
            {
                var errToken = new TokenModel { Success = false };
                try
                {
                    var errDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(resContent);
                    errToken.Errors = errDict;
                    return errToken;
                }
                catch (Exception)
                {
                    if (ApiClientContext.Log != null)
                    {
                        ApiClientContext.Log.Error("获取Token的反馈出错：" + resContent);
                    }
                    errToken.Errors.Add("", resContent); //"系统错误！" + 
                    return errToken;
                }
            }
            var token = JsonConvert.DeserializeObject<TokenModel>(resContent);
            token.Success = true;
            return token;
        }
    }
}
