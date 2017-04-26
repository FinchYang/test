using System;
using System.Windows.Data;
using System.Windows.Media;

namespace SimulaDesign.WPFCustomUI.Converters
{
    public class BytesToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var imageConverter = new ImageSourceConverter();
                return imageConverter.ConvertFrom(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
