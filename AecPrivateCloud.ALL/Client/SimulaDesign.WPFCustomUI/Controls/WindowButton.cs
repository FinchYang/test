using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class WindowButton : Button
    {
        public static readonly DependencyProperty MoverBrushProperty;
		public static readonly DependencyProperty EnterBrushProperty;
		public Brush MoverBrush
		{
			get
			{
				return base.GetValue(WindowButton.MoverBrushProperty) as Brush;
			}
			set
			{
				base.SetValue(WindowButton.MoverBrushProperty, value);
			}
		}
		public Brush EnterBrush
		{
			get
			{
				return base.GetValue(WindowButton.EnterBrushProperty) as Brush;
			}
			set
			{
				base.SetValue(WindowButton.EnterBrushProperty, value);
			}
		}
		static WindowButton()
		{
            WindowButton.MoverBrushProperty = DependencyProperty.Register("MoverBrush", typeof(Brush), typeof(WindowButton), new PropertyMetadata(null));
            WindowButton.EnterBrushProperty = DependencyProperty.Register("EnterBrush", typeof(Brush), typeof(WindowButton), new PropertyMetadata(null));
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowButton), new FrameworkPropertyMetadata(typeof(WindowButton)));
		}
        public WindowButton()
		{
			base.Content = "";
			base.Background = Brushes.Orange;
		}
    }
}
