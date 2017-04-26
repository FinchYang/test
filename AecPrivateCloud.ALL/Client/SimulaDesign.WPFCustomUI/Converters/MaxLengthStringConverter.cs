using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SimulaDesign.WPFCustomUI.Converters
{
    public class MaxLengthStringConverter : IValueConverter
    {
        public MaxLengthStringConverter() : this(8)
        {
        }

        public MaxLengthStringConverter(int maxLength)
        {
            _count = maxLength;
        }

        private readonly int _count;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return String.Empty;
            var count = _count;
            if (parameter != null)
            {
                int c0;
                var ok = int.TryParse(parameter.ToString(), out c0);
                if (ok)
                {
                    count = c0;
                }
            }
            var str = value.ToString();
            if (str.Length <= count) return str;
            return str.Substring(0, count) + "...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return String.Empty;
            }
            return str;
        }
    }
}
