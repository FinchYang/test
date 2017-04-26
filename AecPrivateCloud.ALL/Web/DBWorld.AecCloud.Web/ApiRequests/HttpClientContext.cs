using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using log4net;

namespace DBWorld.AecCloud.Web.ApiRequests
{
    /// <summary>
    /// 对于不需要修改Headers的请求的HttpClient实例获取
    /// </summary>
    public class HttpClientContext
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ConcurrentDictionary<string, HttpClient> Dict = new ConcurrentDictionary<string, HttpClient>();

        public static HttpClient Create(string host)
        {
            var client = HttpClientFactory.Create(innerHandler: new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
                //, UseCookies = true,
                //CookieContainer = new CookieContainer()
            });

            try
            {
                client.BaseAddress = new Uri(host);
            }
            catch (Exception ex)
            {
                Log.Error("设置HttpClient的Uri出错：" + host, ex);
                return null;
            }

            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(
                new StringWithQualityHeaderValue("gzip"));
            return client;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriAuthority">请求的根URL</param>
        /// <returns></returns>
        public static HttpClient GetOrAddClient(string uriAuthority)
        {
            uriAuthority = uriAuthority.TrimEnd('/');
            var hostUpper = uriAuthority.ToUpper();
            HttpClient client = null;
            if (!Dict.ContainsKey(hostUpper))
            {
                client = HttpClientFactory.Create(innerHandler: new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                    //, UseCookies = true,
                    //CookieContainer = new CookieContainer()
                });

                try
                {
                    client.BaseAddress = new Uri(uriAuthority);
                }
                catch (Exception ex)
                {
                    Log.Error("设置HttpClient的Uri出错：" + uriAuthority, ex);
                    return null;
                }

                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(
                    new StringWithQualityHeaderValue("gzip"));
                var ok = Dict.TryAdd(hostUpper, client);
                client = !ok ? Dict.GetOrAdd(hostUpper, client) : Dict[hostUpper];
            }
            else
            {
                client = Dict[hostUpper];
            }
            if (client == null)
            {
                Log.ErrorFormat("HttpClient为空：{0}", uriAuthority);
            }
            return client;
        }
    }
}