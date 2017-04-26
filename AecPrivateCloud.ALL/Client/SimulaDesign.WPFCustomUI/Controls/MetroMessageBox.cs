using System;
using System.Windows;

namespace SimulaDesign.WPFCustomUI.Controls
{
    /// <summary>
    /// CMessageBox显示的按钮类型
    /// </summary>
    public enum MetroMessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNO = 2,
        YesNoCancel = 3
    }

    /// <summary>
    /// CMessageBox显示的图标类型
    /// </summary>
    public enum MetroMessageBoxImage
    {
        None = 0,
        Info = 1,
        Question = 2,
        Warning = 3,
        Error = 4
    }

    /// <summary>
    /// 消息的重点显示按钮
    /// </summary>
    public enum MetroMessageBoxDefaultButton
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 3,
        No = 4
    }

    /// <summary>
    /// 消息框的返回值
    /// </summary>
    public enum MetroMessageBoxResult
    {
        //用户直接关闭了消息窗口
        None = 0,
        //用户点击确定按钮
        OK = 1,
        //用户点击取消按钮
        Cancel = 2,
        //用户点击是按钮
        Yes = 3,
        //用户点击否按钮
        No = 4
    }

    public class MetroMessageBox
    {
        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="cmessageBoxText">消息内容</param>
        public static MetroMessageBoxResult Show(string cmessageBoxText)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = cmessageBoxText,
                OKButtonVisibility = Visibility.Visible
            };

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }

            return window.Result;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="cmessageBoxText">消息内容</param>
        /// <param name="caption">消息标题</param>
        public static MetroMessageBoxResult Show(string cmessageBoxText, string caption)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = cmessageBoxText,
                MessageBoxTitle = caption,
                OKButtonVisibility = Visibility.Visible
            };

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }

            return window.Result;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="metromessageBoxText">消息内容</param>
        /// <param name="metroMessageBoxButton">消息框按钮</param>
        public static MetroMessageBoxResult Show(string metromessageBoxText, MetroMessageBoxButton metroMessageBoxButton)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = metromessageBoxText
            };

            switch (metroMessageBoxButton)
            {
                case MetroMessageBoxButton.OK:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.OKCancel:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNO:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNoCancel:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
            }

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }

            return window.Result;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="metromessageBoxText">消息内容</param>
        /// <param name="caption">消息标题</param>
        /// <param name="metroMessageBoxButton">消息框按钮</param>
        public static MetroMessageBoxResult Show(string metromessageBoxText, string caption, MetroMessageBoxButton metroMessageBoxButton)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = metromessageBoxText, 
                MessageBoxTitle = caption
            };

            switch (metroMessageBoxButton)
            {
                case MetroMessageBoxButton.OK:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.OKCancel:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNO:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNoCancel:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
            }

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }

            return window.Result;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="metromessageBoxText">消息内容</param>
        /// <param name="caption">消息标题</param>
        /// <param name="metroMessageBoxButton">消息框按钮</param>
        /// <param name="metroMessageBoxImage">消息框图标</param>
        /// <returns></returns>
        public static MetroMessageBoxResult Show(string metromessageBoxText, string caption, MetroMessageBoxButton metroMessageBoxButton, MetroMessageBoxImage metroMessageBoxImage)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = metromessageBoxText, 
                MessageBoxTitle = caption
            };


            switch (metroMessageBoxButton)
            {
                case MetroMessageBoxButton.OK:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.OKCancel:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNO:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNoCancel:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
            }

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }

            return window.Result;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="metromessageBoxText">消息内容</param>
        /// <param name="caption">消息标题</param>
        /// <param name="metroMessageBoxButton">消息框按钮</param>
        /// <param name="metroMessageBoxImage">消息框图标</param>
        /// <param name="metroMessageBoxDefaultButton">消息框默认按钮</param>
        /// <returns></returns>
        public static MetroMessageBoxResult Show(string metromessageBoxText, string caption, 
            MetroMessageBoxButton metroMessageBoxButton,
            MetroMessageBoxImage metroMessageBoxImage,
            MetroMessageBoxDefaultButton metroMessageBoxDefaultButton)
        {
            var window = new MessageBoxView
            {
                MessageBoxText = metromessageBoxText, 
                MessageBoxTitle = caption
            };

            #region 按钮
            switch (metroMessageBoxButton)
            {
                case MetroMessageBoxButton.OK:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.OKCancel:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNO:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        break;
                    }
                case MetroMessageBoxButton.YesNoCancel:
                    {
                        window.YesButtonVisibility = Visibility.Visible;
                        window.NoButtonVisibility = Visibility.Visible;
                        window.CancelButtonVisibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        window.OKButtonVisibility = Visibility.Visible;
                        break;
                    }
            }
            #endregion

            #region 默认按钮
            switch (metroMessageBoxDefaultButton)
            {
                case MetroMessageBoxDefaultButton.OK:
                    {
                        window.OKButtonStyle = MessageBoxView.ButtonStyle.NormalButtonStyle;
                        window.CancelButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.YesButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.NoButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        break;
                    }
                case MetroMessageBoxDefaultButton.Cancel:
                    {
                        window.OKButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.CancelButtonStyle = MessageBoxView.ButtonStyle.NormalButtonStyle;
                        window.YesButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.NoButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        break;
                    }
                case MetroMessageBoxDefaultButton.Yes:
                    {
                        window.OKButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.CancelButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.YesButtonStyle = MessageBoxView.ButtonStyle.NormalButtonStyle;
                        window.NoButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        break;
                    }
                case MetroMessageBoxDefaultButton.No:
                    {
                        window.OKButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.CancelButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.YesButtonStyle = MessageBoxView.ButtonStyle.NotNormalButtonStyle;
                        window.NoButtonStyle = MessageBoxView.ButtonStyle.NormalButtonStyle;
                        break;
                    }
                case MetroMessageBoxDefaultButton.None:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            #endregion

            #region 图标

            switch (metroMessageBoxImage)
            {
                case MetroMessageBoxImage.None:
                    window.ImagePath = "/SimulaDesign.WPFCustomUI;component/Resource/MsgBox/none.png";
                    break;
                case MetroMessageBoxImage.Info:
                    window.ImagePath = "/SimulaDesign.WPFCustomUI;component/Resource/MsgBox/info.png";
                    break;
                case MetroMessageBoxImage.Question:
                    window.ImagePath = "/SimulaDesign.WPFCustomUI;component/Resource/MsgBox/question.png";
                    break;
                case MetroMessageBoxImage.Warning:
                    window.ImagePath = "/SimulaDesign.WPFCustomUI;component/Resource/MsgBox/warning.png";
                    break;
                case MetroMessageBoxImage.Error:
                    window.ImagePath = "/SimulaDesign.WPFCustomUI;component/Resource/MsgBox/error.png";
                    break;
                default :
                    break;
            }
            #endregion

            try
            {
                window.ShowDialog();
            }
            catch (Exception)
            {
            }
            
            return window.Result;
        }
    }
}
