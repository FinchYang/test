using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class LinkLabelBlock : Label
    {
        static LinkLabelBlock() 
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkLabelBlock), new FrameworkPropertyMetadata(typeof(LinkLabelBlock)));
        }
    }
}
