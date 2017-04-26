using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace AecCloud.CloudServices.Utils
{
    /// <summary>
    /// Http ashx请求帮助类
    /// </summary>
    internal class HttpUtil
    {
        /// <summary>
        /// 获取请求的返回json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestStr"></param>
        /// <returns></returns>
        public static string GetResponseJson(string url,string requestStr)
        {
            //var url = string.Format("http://211.152.38.124/JSONAPI/JSONAPI.ashx");
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var payload = Encoding.UTF8.GetBytes(requestStr);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            writer.Dispose();

            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            string srcString;
            response = request.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new StreamReader(responseStream, Encoding.UTF8);
            srcString = reader.ReadToEnd();
            reader.Close();
            return srcString;
        }
    }
}
