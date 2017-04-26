using System;
using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class FlatButton : Button
    {
        static FlatButton()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatButton), new FrameworkPropertyMetadata(typeof(FlatButton)));
        }
    }
}