using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Client
{
    public static class ResponseMessageExtensions
    {
        public static ResponseModel GetResponse(this HttpResponseMessage resp)
        {
            if (resp == null) return null;
            var rm = new ResponseModel
            {
                Success = resp.IsSuccessStatusCode,
                Status = resp.StatusCode,
                Content = resp.Content.ReadAsStringAsync().Result,
                Location = resp.Headers.Location
            };
            return rm;
        }
    }
}
