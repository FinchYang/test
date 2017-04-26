using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DBWorld.CloudDrive.ViewModels;
using SimulaDesign.WPFCustomUI.Controls.BreadcrumbBar;
using SimulaDesign.WPFCustomUI.Util;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.CloudDrive.Views
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MyCloudDrive : UserControl, IWebBrowserView
    {
        private static string _sourcePath;
        private MyCloudDriveViewModel _vm;
        private bool _isFromWebBrowser = false;
        private bool _isFromBreadCrumb = false;
        private bool _firstLoad = true;

        public MyCloudDrive()
        {
            InitializeComponent();
            AddressBar.PathChanged += AddressBar_OnPathChanged;
            AddressBar.PopulateItems += AddressBar_OnPopulateItems;
            AddressBar.BreadcrumbItemDropDownOpened += AddressBar_OnBreadcrumbItemDropDownOpened;
            Wb.WebBrowser.Navigated += Wb_OnNavigated;
            Loaded += async (o,e) => await MyCloudDrive_Loaded();
            KeyDown += MyCloudDrive_KeyDown;
        }

        private void MyCloudDrive_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                GoBack();
            }
        }

        private async Task MyCloudDrive_Loaded()
        {
            GetViewModel();
            //_vm = DataContext as MyCloudDriveViewModel;
            if (_vm != null)
            {                                                                                                              

                //_vm.LoadApp();

                _vm.View = this;

                AddressBar.RootItem.Header = _vm.DisplayName;
                var sp = await _vm.CurrentPath.Task;
                var pp = sp.ToString();
                if (pp.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    pp = pp.Substring(8).Replace("/", "\\");
                }
                _sourcePath = pp;
                if (_sourcePath != null)
                {
                    SetAddressBar(_sourcePath);
                }
            }
        }

        private MyCloudDriveViewModel GetViewModel()
        {
            return _vm ?? (_vm = DataContext as MyCloudDriveViewModel);
        }

        private string GetSourcePath()
        {
            var sourcePath = _sourcePath ??(_sourcePath=_vm.SourcePath);
            if (sourcePath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                sourcePath = sourcePath.Substring(8).Replace("/", "\\");
            }

            return sourcePath;
        }

        private static string GetPathFromBreadcrumbBar(BreadcrumbBar selectedBar, string sourcePath)
        {
            var list = new List<string>();
            var selectedItem = selectedBar.SelectedBreadcrumb;
            while (selectedItem != null)
            {
                var p = selectedItem.TraceValue;
                list.Add(p);
                selectedItem = selectedItem.ParentBreadcrumbItem;
            }
            if (list.Count == 0) return sourcePath;
            list.RemoveAt(list.Count - 1);
            list.Add(sourcePath);
            list.Reverse();
            return Path.Combine(list.ToArray());
        }

        public void SetAddressBar(string path)
        {
            var paths = path.Split('\\');
            var plength = paths.Length;
            GetViewModel();
            AddressBar.RootItem.Header = _vm.DisplayName;//+"\\";//_sourcePath;
            AddressBar.RootItem.Items.Clear();
            if (AddressBar.RootItem.Items.Count == 0)
            {
                PopulateFolders(AddressBar.RootItem);
            }
            if (plength > 2)
            {
                var item = AddressBar.RootItem.Items.Cast<PathItem>().FirstOrDefault(c => c.Folder == paths[2]);
                AddressBar.RootItem.SelectedItem = item;
                for (int i = 3; i < plength; i++)
                {
                    item = AddressBar.SelectedBreadcrumb.Items.Cast<PathItem>().FirstOrDefault(c => c.Folder == paths[i]);
                    AddressBar.SelectedBreadcrumb.SelectedItem = item;
                }
            }
        }

        /// <summary>
        /// Populate the Items of the specified BreadcrumbItem with the sub folders if necassary.
        /// </summary>
        /// <param name="item"></param>
        private void PopulateFolders(BreadcrumbItem item)
        {
            try
            {
                GetViewModel();
                string path0 = AddressBar.PathFromBreadcrumbItem(item);
                string trace = item.TraceValue;
                var sourcePath = GetSourcePath();
                var path = sourcePath;
                if (trace != _vm.DisplayName) //trace != _vm.DisplayName + "\\" && 
                {
                    path = Path.Combine(sourcePath, path0);
                }
                string[] paths = Directory.GetDirectories(path);
                foreach (string s in paths)
                {
                    string file = Path.GetFileName(s);
                    var fi = new PathItem();
                    fi.Folder = file;
                    item.Items.Add(fi);
                }
                if (_firstLoad)
                {
                    _firstLoad = false;
                    if (_vm._sourcePath == null)
                    {
                        _vm.SourcePath = path;
                    }
                    Wb.WebBrowser.Navigate(new Uri(path));
                }
            }
            catch
            {
            }
        }

        private void Wb_OnNavigated(object sender, NavigationEventArgs e)
        {
            try
            {
                dynamic ai = WebBrowserUtility.GetActiveXInstance(Wb.WebBrowser);
                //ai.ExecWB(1000000, 0, false, null); //Task Bar
                ai.ExecWB(1000001, 0, false, null); //Search Bar
                //SetAddressBar(e.Uri.LocalPath);
            }
            catch
            {
            }

            if (_isFromBreadCrumb)
            {
                _isFromWebBrowser = false;
                _isFromBreadCrumb = false;
                return;
            }

            

            _isFromWebBrowser = true;
            _isFromBreadCrumb = false;
            if (e.Uri != null)
            {
                SetAddressBar(e.Uri.LocalPath);
            }
        }

        private void AddressBar_OnPathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            if (_isFromWebBrowser)
            {
                _isFromWebBrowser = false;
                _isFromBreadCrumb = false;
            }
            GetViewModel();
            if (!String.IsNullOrEmpty(_vm.SourcePath))
            {
                var sourcePath = GetSourcePath();
                var selectedBar = e.Source as BreadcrumbBar;
                string path = null;
                if (selectedBar != null)
                {
                    path = GetPathFromBreadcrumbBar(selectedBar, sourcePath);
                }
                else
                {
                    path = sourcePath; //Path.Combine(sourcePath, e.NewValue);

                }
                //var path = Path.Combine(GetSourcePath(), e.NewValue);

                _isFromWebBrowser = false;
                _isFromBreadCrumb = true;
                if (path != null) this.Wb.WebBrowser.Navigate(new Uri(path));

            }
            e.Handled = true;
        }

        private void AddressBar_OnPopulateItems(object sender, BreadcrumbItemEventArgs e)
        {
            var item = e.Item;
            if (item.Items.Count == 0)
            {
                PopulateFolders(item);
                e.Handled = true;
            }
        }

        private void AddressBar_OnBreadcrumbItemDropDownOpened(object sender, BreadcrumbItemEventArgs e)
        {
            var item = e.Item;

            // only repopulate, if the BreadcrumbItem is dynamically generated which means, item.Data is a  pointer to itself:
            if (!(item.Data is BreadcrumbItem))
            {
                item.Items.Clear();
                PopulateFolders(item);
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
