//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;

//namespace AecCloud.Client.Util
//{
//    public class WebBrowserUtil
//    {
//        internal static object GetActiveXInstance(WebBrowser wb)
//        {
//            return wb.GetType().InvokeMember("ActiveXInstance",
//                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
//                null, wb, new object[] { }); // as SHDocVw.WebBrowser; //incloud Microsoft Internet Control
//        }


//        public static readonly DependencyProperty BindableSourceProperty =
//           DependencyProperty.RegisterAttached("BindableSource", typeof(Uri), typeof(WebBrowserUtil), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

//        public static Uri GetBindableSource(DependencyObject obj)
//        {
//            return (Uri)obj.GetValue(BindableSourceProperty);
//        }

//        public static void SetBindableSource(DependencyObject obj, Uri value)
//        {
//            obj.SetValue(BindableSourceProperty, value);
//        }

//        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
//        {
//            WebBrowser browser = o as WebBrowser;
//            if (browser != null)
//            {
//                Uri uri = e.NewValue as Uri;
//                browser.Navigate(uri);
//            }
//        }
//    }
//}
