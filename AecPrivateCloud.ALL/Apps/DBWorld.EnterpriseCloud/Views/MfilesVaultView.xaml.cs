using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DBWorld.EnterpriseCloud.ViewModels;
using SimulaDesign.WPFCustomUI.Util;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.EnterpriseCloud.Views
{
    /// <summary>
    /// MfilesVaultView.xaml 的交互逻辑
    /// </summary>
    public partial class MfilesVaultView : UserControl, IWebBrowserView
    {
        private MfilesVaultViewModel _viewModel;

        public bool NavigatedFromBrowser { get; set; }

        public MfilesVaultView()
        {
            InitializeComponent();
            Loaded+=MfilesVaultView_Loaded;
            Wb.WebBrowser.Navigated+=WebBrowser_Navigated;
            KeyDown+=MfilesVaultView_KeyDown;
        }

        private void MfilesVaultView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                GoBack();
            }
        }

        private void MfilesVaultView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MfilesVaultViewModel;
            if (_viewModel != null)
            {
                _viewModel.BrowserView = this;
                Wb.WebBrowser.Navigate(new Uri(_viewModel.SourcePath));
            }
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            NavigatedFromBrowser = false;

            try
            {
                dynamic ai = WebBrowserUtility.GetActiveXInstance(Wb.WebBrowser);
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
            if (Wb.WebBrowser.CanGoBack)
            {
                Wb.WebBrowser.GoBack();
            }
        }

        public void GoForward()
        {
            if (Wb.WebBrowser.CanGoForward)
            {
                Wb.WebBrowser.GoForward();
            }
        }

        public void SetAddressBar(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                Wb.WebBrowser.Navigate(new Uri(path));
            }
        }

        public string CurrentPath
        {
            get { return Wb.WebBrowser.Source.LocalPath; }
        }


        public void Refresh()
        {
            Wb.WebBrowser.Navigate(Wb.WebBrowser.Source);
        }
    }
}
