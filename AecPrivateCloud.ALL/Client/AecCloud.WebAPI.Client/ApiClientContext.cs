using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using log4net;

namespace AecCloud.WebAPI.Client
{
    public class ApiClientContext
    {
        public static ILog Log { get; set; }
        private ApiClientContext() { }

        private static readonly Lazy<ConcurrentDictionary<Type, object>> _clients =
            new Lazy<ConcurrentDictionary<Type, object>>(() =>
                new ConcurrentDictionary<Type, object>(), isThreadSafe: true);

        private static readonly Lazy<HttpClient> _httpClient =
                    new Lazy<HttpClient>(
                        () =>
                        {

                            Assembly assembly = Assembly.GetExecutingAssembly();
                            HttpClient httpClient = HttpClientFactory.Create(innerHandler: new HttpClientHandler
                            { AutomaticDecompression = DecompressionMethods.GZip, UseCookies=true, 
                                CookieContainer = new CookieContainer() });
                            httpClient.Timeout = TimeSpan.FromMinutes(5);//todo
                            httpClient.DefaultRequestHeaders.Accept.Add(
                                new MediaTypeWithQualityHeaderValue("application/json"));
                            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(
                                new StringWithQualityHeaderValue("gzip"));
                            httpClient.DefaultRequestHeaders.Add("X-UserAgent",
                                string.Concat(assembly.FullName, "( ", 
                                FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion, ")"));

                            return httpClient;

                        }, isThreadSafe: true);

        internal ConcurrentDictionary<Type, object> Clients
        {

            get { return _clients.Value; }
        }

        public Uri BaseUri { get; internal set; }
        internal string ApiKey { get; set; }

        internal HttpClient HttpClient
        {

            get
            {

                if (!_httpClient.IsValueCreated)
                {

                    InitializeHttpClient();
                }

                return _httpClient.Value;
            }
        }

        internal static string EncodeToBase64(string value)
        {

            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static ApiClientContext Create(string baseUri, string apiKey)
        {

            var apiClientContext = new ApiClientContext
            {
                BaseUri = new Uri(baseUri),
                ApiKey = apiKey
            };

            return apiClientContext;
        }

        private void InitializeHttpClient()
        {

            if (BaseUri == null)
            {
                throw new ArgumentNullException("BaseUri");
            }

            //if (string.IsNullOrEmpty(ApiKey))
            //{
            //    throw new ArgumentNullException("ApiKey");
            //}

            // Set BaseUri
            _httpClient.Value.BaseAddress = BaseUri;
            if (!String.IsNullOrWhiteSpace(ApiKey))
            {
                _httpClient.Value.DefaultRequestHeaders.Add("apikey", ApiKey);
            }
            // Set default headers
            //_httpClient.Value.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", AuthenticationValue);
        }
    }
}
