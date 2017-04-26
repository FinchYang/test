using System.Windows;
using System.Windows.Input;

namespace SimulaDesign.WPFCustomUI.Controls
{
    /// <summary>
    /// MetroMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxView
    {
        /// <summary>
        /// 消息对话框按钮样式
        /// </summary>
        public enum ButtonStyle
        {
            NormalButtonStyle = 0,
            NotNormalButtonStyle = 1
        }

        #region 成员
        private Style normalButtonStyle;

        private Style notNormalButtonStyle;
        #endregion

        #region 属性
        public string MessageBoxTitle
        {
            get;
            set;
        }

        public string MessageBoxText
        {
            get;
            set;
        }

        public string ImagePath
        {
            get;
            set;
        }

        public Visibility OKButtonVisibility
        {
            get;
            set;
        }

        public Visibility CancelButtonVisibility
        {
            get;
            set;
        }

        public Visibility YesButtonVisibility
        {
            get;
            set;
        }

        public Visibility NoButtonVisibility
        {
            get;
            set;
        }

        public ButtonStyle OKButtonStyle
        {
            set
            {
                if(value == ButtonStyle.NormalButtonStyle)
                {
                    OKButton.Style = normalButtonStyle;
                }
                else if(value == ButtonStyle.NotNormalButtonStyle)
                {
                    OKButton.Style = notNormalButtonStyle;
                }
            }
        }

        public ButtonStyle CancelButtonStyle
        {
            set
            {
                if (value == ButtonStyle.NormalButtonStyle)
                {
                    CancelButton.Style = normalButtonStyle;
                }
                else if (value == ButtonStyle.NotNormalButtonStyle)
                {
                    CancelButton.Style = notNormalButtonStyle;
                }
            }
        }

        public ButtonStyle YesButtonStyle
        {
            set
            {
                if (value == ButtonStyle.NormalButtonStyle)
                {
                    YesButton.Style = normalButtonStyle;
                }
                else if (value == ButtonStyle.NotNormalButtonStyle)
                {
                    YesButton.Style = notNormalButtonStyle;
                }
            }
        }

        public ButtonStyle NoButtonStyle
        {
            set
            {
                if (value == ButtonStyle.NormalButtonStyle)
                {
                    NoButton.Style = normalButtonStyle;
                }
                else if (value == ButtonStyle.NotNormalButtonStyle)
                {
                    NoButton.Style = notNormalButtonStyle;
                }
            }
        }

        public MetroMessageBoxResult Result;
        #endregion

        #region 构造函数
        public MessageBoxView()
        {
            InitializeComponent();
            this.DataContext = this;

            MessageBoxTitle = "消息提示";
            OKButtonVisibility = System.Windows.Visibility.Collapsed;
            CancelButtonVisibility = System.Windows.Visibility.Collapsed;
            YesButtonVisibility = System.Windows.Visibility.Collapsed;
            NoButtonVisibility = System.Windows.Visibility.Collapsed;

            normalButtonStyle = this.FindResource("NormalButtonStyle") as Style;
            notNormalButtonStyle = this.FindResource("NotNormalButtonStyle") as Style;

            Result = MetroMessageBoxResult.None;
        }
        #endregion

        #region 事件
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MetroMessageBoxResult.OK;
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MetroMessageBoxResult.Yes;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MetroMessageBoxResult.No;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MetroMessageBoxResult.Cancel;
            this.Close();
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MetroMessageBoxResult.None;
            this.Close();
        }

        #endregion

        private void OnMouseLeftButtonDownAtTitlee(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
