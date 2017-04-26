using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;
using Newtonsoft.Json;

namespace DBWorld.DesignCloud.Util
{
    public class ResponContentUtil
    {
        public static string GetResponResult(string param)
        {
            string result = string.Empty;

            try
            {

                var dic = JsonConvert.DeserializeObject<BadRequestResponseModel>(param);
                if (dic.ModelState == null)
                {
                    result = dic.Message;
                }
                else
                {
                    foreach (var arry in dic.ModelState.Values)
                    {
                        result = arry[0];
                    }
                }
            }
            catch (Exception)
            {
                result = param;
            }
            
            return result;
        }
    }
}
