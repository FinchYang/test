using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AecCloud.Client.Util;
using DBWorld.Config.Helper;
using SimulaDesign.WPFCustomUI.Controls;

namespace AecCloud.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += TitleGridOnMouseLeftButtonDown;
            //this.TitleGrid.MouseLeftButtonDown += TitleGridOnMouseLeftButtonDown;
        }

        private void TitleGridOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();

            //双击
            if (e.ClickCount == 2)
            {
                //窗口变化
                if (this.WindowState == System.Windows.WindowState.Maximized)
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                    SetWindowMaxButtonImg();
                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                    SetWindowNorButtonImg();
                }
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// 最大化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMaxWin(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                SetWindowMaxButtonImg();
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
                SetWindowNorButtonImg();
            }
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinWin(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// 主窗口正常化时的按钮样式
        /// </summary>
        private void SetWindowNorButtonImg()
        {
            this.BtnMaxOrNor.ToolTip = "还原";
            this.BtnMaxOrNor.MoverBrush =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/nor_m.png", UriKind.RelativeOrAbsolute)));
            this.BtnMaxOrNor.EnterBrush =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/nor_e.png", UriKind.RelativeOrAbsolute)));
            this.BtnMaxOrNor.Background =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/nor_bg.png", UriKind.RelativeOrAbsolute)));
        }

        /// <summary>
        /// 窗口最大化时的按钮样式
        /// </summary>
        private void SetWindowMaxButtonImg()
        {
            this.BtnMaxOrNor.ToolTip = "最大化";
            this.BtnMaxOrNor.MoverBrush =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/max_m.png", UriKind.RelativeOrAbsolute)));
            this.BtnMaxOrNor.EnterBrush =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/max_e.png", UriKind.RelativeOrAbsolute)));
            this.BtnMaxOrNor.Background =
                           new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/SimulaDesign.WPFCustomUI;component/Resource/CaptionBtn/max_bg.png", UriKind.RelativeOrAbsolute)));
        }

        #region 托盘操作

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            var msg = String.Format("{0}\r\n{1}",
                AssemblyInfoHelper.ProductInfo,
                AssemblyInfoHelper.Copyright);

            MetroMessageBox.Show(msg,
              AssemblyInfoHelper.Product,
              MetroMessageBoxButton.OK,
              MetroMessageBoxImage.None,
              MetroMessageBoxDefaultButton.OK);
        }

        private void Show_OnClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        //private void Setting_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var instance = UserConfigHelper.GetInstence();
        //    var config = instance.LoadLastUserConfig();

        //    var vw = new TraySettingView();
        //    var vm = new TraySettingViewModel(config);
        //    vw.DataContext = vm;
        //    vw.ShowDialog();
        //}

        private void Switch_OnClick(object sender, RoutedEventArgs e)
        {
            this.TrayIcon.Dispose();
            this.Close();

            var loginView = new LoginView();
            loginView.ShowDialog();
        }

        private void AutoLogin_OnClick(object sender, RoutedEventArgs e)
        {
            var config = UserConfigHelper.GetInstence();
            var userConfig = config.LoadLastUserConfig();
            userConfig.AutoLogin = 0;
            config.SaveConfig(userConfig);
        }
        private void ExitApp_OnClick(object sender, RoutedEventArgs e)
        {
            this.TrayIcon.Dispose();
            Application.Current.Shutdown();
        }
        #endregion
    }
}
