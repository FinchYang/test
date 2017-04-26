using System.Windows;
using System.Windows.Controls;


namespace SimulaDesign.WPFCustomUI.Controls
{
    public class AeroExplorerBar : ItemsControl
    {
        static AeroExplorerBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AeroExplorerBar), new FrameworkPropertyMetadata(typeof(AeroExplorerBar)));
        }
    }
}
