using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DBWorld.DesignCloud.ViewModels;
using SimulaDesign.WPFCustomUI.Util;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.DesignCloud.Views
{
    /// <summary>
    /// MfVaultView.xaml 的交互逻辑
    /// </summary>
    public partial class MfVaultView : UserControl, IWebBrowserView
    {
        private MfVaultViewModel _viewModel;
        public bool NavigatedFromBrowser { get; set; }

        public MfVaultView()
        {
            InitializeComponent();
            Loaded += MfVaultView_Loaded;
            Wb.Navigated += WebBrowser_Navigated;
            NavigatedFromBrowser = true;
            KeyDown+=MfVaultView_KeyDown;
        }

        private void MfVaultView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                GoBack();
            }
        }

        private void MfVaultView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MfVaultViewModel;
            if (_viewModel != null)
            {
                _viewModel.BrowserView = this;
                if (_viewModel.SourcePath != null) Wb.Navigate(new Uri(_viewModel.SourcePath));
            }
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            NavigatedFromBrowser = false;

            try
            {
                dynamic ai = WebBrowserUtility.GetActiveXInstance(Wb);
                //ai.ExecWB(1000000, 0, false, null); //Task Bar
                ai.ExecWB(1000001, 0, false, null); //Search Bar
                //SetAddressBar(e.Uri.LocalPath);
                if (_viewModel != null && _viewModel.BrowserView != null)
                {
                    _viewModel.SetAddress(e.Uri.LocalPath);
                }
            }
            catch
            {
            }
        }

        public void GoBack()
        {
            if (Wb.CanGoBack)
            {
                Wb.GoBack();
                NavigatedFromBrowser = true;
            }
        }

        public void GoForward()
        {
            if (Wb.CanGoForward)
            {
                Wb.GoForward();
                NavigatedFromBrowser = true;
            }
        }

        public void SetAddressBar(string path)
        {
            //if (System.IO.Directory.Exists(path))
            //{
                //var uri = new Uri(path);
                //if (Wb.WebBrowser.Source != uri)
                //{
                Wb.Navigate(new Uri(path));
                //}
            //}
        }

        public string CurrentPath
        {
            get { return Wb.Source.LocalPath; }
        }


        public void Refresh()
        {
            Wb.Navigate(Wb.Source);
        }
    }
}
