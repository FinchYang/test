using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DBWorld.DesignCloud.Util
{
    public class BytesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value != null && value is byte[])
                {
                    var bytes = value as byte[];
                    if (bytes.Length == 0) return null;
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = new MemoryStream(bytes);
                    img.EndInit();

                    return img;
                }
            }
            catch (Exception)
            {
               
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

