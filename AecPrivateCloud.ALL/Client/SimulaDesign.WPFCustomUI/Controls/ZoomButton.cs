using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class ZoomButton : ButtonBase
    {
        static ZoomButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomButton), new FrameworkPropertyMetadata(typeof(ZoomButton)));
        }

    public BitmapImage IconImage
    {
      get { return (BitmapImage)GetValue(IconImageProperty); }
      set { SetValue(IconImageProperty, value); }
    }

    public static readonly DependencyProperty IconImageProperty =
        DependencyProperty.Register("IconImage", typeof(BitmapImage), typeof(ZoomButton), new UIPropertyMetadata(null));
    }
}
