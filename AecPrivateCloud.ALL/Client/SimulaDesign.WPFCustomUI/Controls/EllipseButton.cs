using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class EllipseButton : Button
    {
        static EllipseButton()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EllipseButton), new FrameworkPropertyMetadata(typeof(EllipseButton)));
        }
    }
}
