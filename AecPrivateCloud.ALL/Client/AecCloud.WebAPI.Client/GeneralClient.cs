using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;

namespace AecCloud.WebAPI.Client
{
    /// <summary>
    /// 一般的WebAPI请求操作
    /// </summary>
    public class GeneralClient
    {
        private readonly HttpClient _client;

        internal GeneralClient(HttpClient client)
        {
            _client = client;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUri">相对于BaseAddress的URI部分，如：api/Account</param>
        /// <param name="content">需要提交的内容</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PostAsync<T>(string relativeUri, T content, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(relativeUri, content);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeUri">相对于BaseAddress的URI部分，如：api/Account</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> GetAsync(string relativeUri, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(relativeUri);
        }
    }
}
