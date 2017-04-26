using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AecCloud.CloudServices.Utils
{
    internal class JsonUtil
    {
        /// <summary>
        /// 将对象序列化为Json字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="head">返回的字符串头</param>
        /// <param name="dateFormat">序列化时日期的格式</param>
        /// <returns></returns>
        public static string ConventToJson(object obj, string head = "parames=", string dateFormat = "yyyy-MM-dd HH:mm:ss")
        {
            var jsonSetting = new JsonSerializerSettings { DateFormatString = dateFormat };
            return head + JsonConvert.SerializeObject(obj, jsonSetting);
        }

        /// <summary>
        /// Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ConventToObject<T>(string json)
        {
           return JsonConvert.DeserializeObject<T>(json);
        }

    }
}
