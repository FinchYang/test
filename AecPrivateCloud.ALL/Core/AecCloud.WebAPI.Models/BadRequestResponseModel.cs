using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class BadRequestResponseModel
    {
        public string Message { get; set; }

        public Dictionary<string, string[]> ModelState { get; set; }
        /// <summary>
        /// 反馈的错误信息
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            if (ModelState==null || ModelState.Count == 0) return Message;
            var errList = new List<string>();
            foreach (var k in ModelState)
            {
                errList.AddRange(k.Value);
            }
            if (errList.Count == 0) return Message;
            if (errList.Count == 1) return errList[0];
            return String.Join("\r\n", errList.ToArray());
        }
    }

    //public class ResponseModel
    //{
    //    public bool Success { get; set; }

    //    public HttpStatusCode Status { get; set; }

    //    public string Content { get; set; }
    //    /// <summary>
    //    /// Created状态码下需要
    //    /// </summary>
    //    public Uri Location { get; set; }

    //    //internal ResponseModel()
    //    //{
    //    //}
    //}
}
