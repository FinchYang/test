using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SimulaDesign.WPFCustomUI.Converters
{
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return new Uri("");
            return new Uri(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Uri)) return "";
            var str = (value as Uri).AbsolutePath;
            if (str.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Substring(8).Replace("/", "\\");
            }
            return str;
        }
    }
}
