using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace DBWorld.AecCloud.Web.ApiRequests
{
    public class ResponseModel : ResponseModel<string>
    {
        internal ResponseModel():base()
        {
        }
    }

    public class ResponseModel<T>
    {
        public bool Success { get; internal set; }

        public HttpStatusCode Status { get; internal set; }

        public T Content { get; internal set; }
        /// <summary>
        /// Created状态码下需要
        /// </summary>
        public Uri Location { get; internal set; }

        internal ResponseModel()
        {
        }
    }

    public static class ResponseMessageExtensions
    {
        public static async Task<ResponseModel> GetResponse(this HttpResponseMessage resp)
        {
            if (resp == null) return null;
            var rm = new ResponseModel
            {
                Success = resp.IsSuccessStatusCode,
                Status = resp.StatusCode,
                Content = await resp.Content.ReadAsStringAsync(),
                Location = resp.Headers.Location
            };
            return rm;
        }
        //TODO 待测试
        public static async Task<ResponseModel<T>> GetResponse<T>(this HttpResponseMessage resp)
        {
            if (resp == null) return null;
            var formatters = new List<MediaTypeFormatter>
            {
                new JsonMediaTypeFormatter(),
                new XmlMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter()
            };

            var rm = new ResponseModel<T>
            {
                Success = resp.IsSuccessStatusCode,
                Status = resp.StatusCode,
                Content = await resp.Content.ReadAsAsync<T>(formatters),
                Location = resp.Headers.Location
            };
            return rm;
        }
    }
}