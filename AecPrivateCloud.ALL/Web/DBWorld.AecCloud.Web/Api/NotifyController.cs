using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using log4net;
using System.IO;

namespace DBWorld.AecCloud.Web.Api
{
    
    public class NotifyController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> PostMsg(string phone, string msg)
        {
            await Task.Run(() => SendMessage(phone, msg));
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void SendMessage(string mobleNumber, string msg)
        {
            string smsurl = ConfigurationManager.AppSettings["Smsurl"];
            string account = ConfigurationManager.AppSettings["Account"];
            string pswd = ConfigurationManager.AppSettings["Pswd"];
            string needstatus = "false";
            string url = "{0}?account={1}&pswd={2}&mobile={3}&msg={4}&needstatus={5}";
            url = string.Format(url, smsurl, account, pswd, mobleNumber, HttpUtility.UrlEncode(msg, Encoding.GetEncoding("UTF-8")), needstatus);

            string strResult = GetSmsResponse(url);
            Log.Info(String.Format("发送短信：phone:{0},msg:{1},result:{2}", mobleNumber,msg,strResult));
        }
        private static string GetSmsResponse(string url)
        {
            string strResult = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //声明一个HttpWebRequest请求   
            request.Timeout = 30000;
            //设置连接超时时间   
            request.Headers.Set("Pragma", "no-cache");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamReceive = response.GetResponseStream();
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            StreamReader streamReader = new StreamReader(streamReceive, encoding);
            strResult = streamReader.ReadToEnd();
            streamReader.Close();
            response.Close();
            return strResult;
        }
    }
}
