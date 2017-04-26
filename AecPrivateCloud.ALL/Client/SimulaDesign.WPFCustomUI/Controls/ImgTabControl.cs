using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class ImgTabControl : TabControl
    {
        static ImgTabControl()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ImgTabControl), new FrameworkPropertyMetadata(typeof(ImgTabControl)));
		}

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ImgTabItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ImgTabItem;
        }
    }
}
