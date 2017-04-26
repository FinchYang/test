using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AecCloud.Client.ViewModels;
using System.Reflection;
using System.Configuration;
using NetSparkle;

namespace AecCloud.Client
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView
    {
        /// <summary>
        /// 自动升级
        /// </summary>
        private Sparkle _sparkle;
        /// <summary>
        /// 初始化
        /// </summary>
        public LoginView()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.MouseLeftButtonDown += OnMouseLeftButtonDown;
            pbPwd.KeyDown +=pbPwd_KeyDown;
            this.DataContext = new LoginViewModel(LoginClose);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.TbUserName.Focus();
            var updateUrl = ConfigurationManager.AppSettings["api"];
            //自动升级
            System.Drawing.Icon icon =
                System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _sparkle = new Sparkle(updateUrl + "/Installer/versioninfo.xml", icon);
            _sparkle.StartLoop(true, true, TimeSpan.FromHours(3));
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void pbPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var model = DataContext as LoginViewModel;
                if (model != null)
                {
                    model.LoginCmd.Execute();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose(object sender, RoutedEventArgs e)
        {
            this.TrayIcon.Dispose();
            _sparkle.StopLoop();
            Application.Current.Shutdown();
        }

        private void LoginClose()
        {
            this.TrayIcon.Dispose();
            _sparkle.StopLoop();
            this.Close();
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinWin(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void LinkReg_Click(object sender, RoutedEventArgs e)
        {
            var website = ConfigurationManager.AppSettings["sso"];
            System.Diagnostics.Process.Start(website + "/Register");
        }

        private void LinRecoverPwd_Click(object sender, RoutedEventArgs e)
        {
            //var website = ConfigurationManager.AppSettings["sso"];
            //System.Diagnostics.Process.Start(website + "/ForgotPassword");
        }

        private void PbPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordtext = (PasswordBox)sender;
            SetPasswordBoxSelection(passwordtext, passwordtext.Password.Length + 1, passwordtext.Password.Length + 1);
        }

        private static void SetPasswordBoxSelection(PasswordBox passwordBox, int start, int length)
        {
            var select = passwordBox.GetType().GetMethod("Select",
                BindingFlags.Instance | BindingFlags.NonPublic);

            select.Invoke(passwordBox, new object[] { start, length });
        }

        private void ChkAutoLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChkAutoLogin.IsChecked == true)
            {
                this.ChkRemeberPwd.IsChecked = true;
            }
        }

        private void ChkRemeberPwd_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ChkRemeberPwd.IsChecked == false)
            {
                this.ChkAutoLogin.IsChecked = false;
            }
        }

        private void Show_OnClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void ExitApp_OnClick(object sender, RoutedEventArgs e)
        {
            this.TrayIcon.Dispose();
            Application.Current.Shutdown();
        }
    }  
}
