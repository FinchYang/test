using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimulaDesign.BimInfo;

namespace SimulaDesign.MfBimInfo
{
    public static class ModelUtility
    {
        public static string GetZippedContent(object model, Func<object, string> toJson)
        {
            string json = toJson(model);
            var zipContent = Utility.Zip(json);
            var content = Utility.ByteToHexStr(zipContent);
            return content;
        }

        public static TModel FromZippedContent<TModel>(string content, Func<string, TModel> fromJson)
        {
            var infoArray = Utility.StrToHexByte(content);
            var info = Utility.Unzip(infoArray);
            return fromJson(info);
        }
    }
}
