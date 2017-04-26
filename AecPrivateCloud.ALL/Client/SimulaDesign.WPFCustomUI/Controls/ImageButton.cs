using System;
using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class ImageButton : Button
    {
        public ImageButton() : base()
        {
            var rd = new ResourceDictionary();
            rd.Source = new Uri("/SimulaDesign.WPFCustomUI;component/Themes/ImageButton.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(rd);
            this.Style = this.FindResource("ImageButtonStyle") as Style;
        }

        #region 属性
        public string BackgroundImage
        {
            get { return (string)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }
        #endregion

        #region 依赖项属性
        public static readonly DependencyProperty BackgroundImageProperty =
           DependencyProperty.Register("BackgroundImage", typeof(string), typeof(ImageButton), new PropertyMetadata(null));
        #endregion
    }
}
