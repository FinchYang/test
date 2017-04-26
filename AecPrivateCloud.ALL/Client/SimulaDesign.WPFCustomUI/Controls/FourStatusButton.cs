using System;
using System.Windows;
using System.Windows.Controls;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class FourStatusButton:Button
    {
        #region 属性
        /// <summary>
        /// 按钮处于正常状态下的背景图片的路径
        /// </summary>
        public string NormalBackgroundImage
        {
            get { return (string)GetValue(NormalBackgroundImageProperty); }
            set { SetValue(NormalBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 鼠标移到按钮上面，按钮的背景图片的路径
        /// </summary>
        public string MouseoverBackgroundImage
        {
            get { return (string)GetValue(MouseoverBackgroundImageProperty); }
            set { SetValue(MouseoverBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 鼠标按下按钮，按钮的背景图片的路径
        /// </summary>
        public string MousedownBackgroundImage
        {
            get { return (string)GetValue(MousedownBackgroundImageProperty); }
            set { SetValue(MousedownBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 当按钮不可用时按钮的背景图片
        /// </summary>
        public string DisabledBackgroundImage
        {
            get { return (string)GetValue(DisabledBackgroundImageProperty); }
            set { SetValue(DisabledBackgroundImageProperty, value); }
        }
        #endregion

        #region 依赖属性
        /// <summary>
        /// 按钮处于正常状态下的背景图片的路径
        /// </summary>
        public static readonly DependencyProperty NormalBackgroundImageProperty =
            DependencyProperty.Register("NormalBackgroundImage", typeof(string), typeof(FourStatusButton), new PropertyMetadata(null));

        /// <summary>
        /// 鼠标移到按钮上面，按钮的背景图片的路径
        /// </summary>
        public static readonly DependencyProperty MouseoverBackgroundImageProperty =
            DependencyProperty.Register("MouseoverBackgroundImage", typeof(string), typeof(FourStatusButton), new PropertyMetadata(null));

        /// <summary>
        /// 鼠标按下按钮，按钮的背景图片的路径
        /// </summary>
        public static readonly DependencyProperty MousedownBackgroundImageProperty =
            DependencyProperty.Register("MousedownBackgroundImage", typeof(string), typeof(FourStatusButton), new PropertyMetadata(null));

        /// <summary>
        /// 当按钮不可用时按钮的背景图片
        /// </summary>
        public static readonly DependencyProperty DisabledBackgroundImageProperty =
            DependencyProperty.Register("DisabledBackgroundImage", typeof(string), typeof(FourStatusButton), new PropertyMetadata(null));
        #endregion

        #region 构造函数
        public FourStatusButton() 
            : base()
        {
            var rd = new ResourceDictionary();
            rd.Source = new Uri("/SimulaDesign.WPFCustomUI;component/Themes/FourStatusButton.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(rd);
            this.Style = this.FindResource("FourStatusButtonStyle") as Style;
        }
        #endregion
    }
}


