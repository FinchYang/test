using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class ImgTabItem : TabItem
    {
        public static readonly DependencyProperty MoverBrushProperty;
        public static readonly DependencyProperty EnterBrushProperty;
        public Brush MoverBrush
        {
	        get
	        {
		        return base.GetValue(ImgTabItem.MoverBrushProperty) as Brush;
	        }
	        set
	        {
		        base.SetValue(ImgTabItem.MoverBrushProperty, value);
	        }
        }
        public Brush EnterBrush
        {
	        get
	        {
		        return base.GetValue(ImgTabItem.EnterBrushProperty) as Brush;
	        }
	        set
	        {
		        base.SetValue(ImgTabItem.EnterBrushProperty, value);
	        }
        }
        static ImgTabItem()
        {
	        ImgTabItem.MoverBrushProperty = DependencyProperty.Register("MoverBrush", typeof(Brush), typeof(ImgTabItem), new PropertyMetadata(null));
	        ImgTabItem.EnterBrushProperty = DependencyProperty.Register("EnterBrush", typeof(Brush), typeof(ImgTabItem), new PropertyMetadata(null));
	        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ImgTabItem), new FrameworkPropertyMetadata(typeof(ImgTabItem)));
        }
        //public ImgTabItem():base()
        //{
        //    base.Header = "Item Title";
        //    base.Background = Brushes.Blue;
        //}
    }
}
