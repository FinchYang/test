using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;
using Newtonsoft.Json;

namespace AecCloud.WebAPI.Client
{

    //public class TokenModel
    //{
    //    [JsonProperty(PropertyName = "access_token")]
    //    public string AccessToken { get; set; }

    //    [JsonProperty(PropertyName = "token_type")]
    //    public string TokenType { get; set; }

    //    [JsonProperty(PropertyName = "userName")]
    //    public string UserName { get; set; }

    //    [JsonProperty(PropertyName = "activated")]
    //    public string Activated { get; set; }

    //    public bool IsActivated
    //    {
    //        get { return StringComparer.OrdinalIgnoreCase.Equals(Activated,"TRUE"); }
    //    }

    //    public bool Success { get; set; }

    //    private IDictionary<string, string> _errors;

    //    public IDictionary<string, string> Errors
    //    {
    //        get { return _errors ?? (_errors = new Dictionary<string, string>()); }
    //        set { _errors = value; }
    //    }

    //}

    public class ResponseModel
    {
        public bool Success { get; internal set; }

        public HttpStatusCode Status { get; internal set; }

        public string Content { get; internal set; }
        /// <summary>
        /// Created状态码下需要
        /// </summary>
        public Uri Location { get; internal set; }

        internal ResponseModel()
        {
        }
    }
    
}
