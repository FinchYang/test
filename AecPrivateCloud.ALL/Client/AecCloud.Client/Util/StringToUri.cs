using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Policy;
using System.Windows.Data;
using System.Windows.Documents;
using AecCloud.MfilesClientCore;

namespace AecCloud.Client.Util
{
    public class StringToUri : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            const string bigImage = "big";
            const string smallImage = "small";
            const string bigPath = "pack://application:,,,/Image/Software/Big/";
            const string smallPath = "pack://application:,,,/Image/Software/Small/";
            const string defaultName = "default.png";

            //大小
            string str = String.Empty;
            if (parameter != null)
            {
                str = parameter.ToString().ToLower();
            }
            else
            {
                str = smallImage;
            }

            //目录
            string path = String.Empty;
            if (str == bigImage)
            {
                path = bigPath;
            }
            else
            {
                path = smallPath;
            }

            //路径
            if (value == null)
            {
                path += defaultName;
                
            }
            else
            {
                path += value.ToString();
            }

            return new Uri(path, UriKind.RelativeOrAbsolute);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
