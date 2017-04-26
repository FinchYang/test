using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DBWorld.EnterpriseCloud.ViewModels;
using SimulaDesign.WPFCustomUI.Controls.BreadcrumbBar;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.EnterpriseCloud
{
    /// <summary>
    /// EnterpriseCloudView.xaml 的交互逻辑
    /// </summary>
    public partial class EnterpriseCloudView : UserControl, IWebBrowserView
    {

        private EnterpriseCloudViewModel _vm;
        public EnterpriseCloudView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            AddressBar.PathChanged += AddressBarOnPathChanged;
            AddressBar.PopulateItems += AddressBarOnPopulateItems;
            AddressBar.BreadcrumbItemDropDownOpened += AddressBarOnBreadcrumbItemDropDownOpened;
        }

        #region 事件函数
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _vm = (DataContext as EnterpriseCloudViewModel);
            if (_vm != null)
            {
                _vm.BrowserView = this;
                SetBreadcrumb(_vm.DisplayName);
            }
        }

        private string GetPathFromBreadcrumbBar(BreadcrumbBar selectedBar)
        {
            var sourcePath = _vm.CurrWorkspace.SourcePath;
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

        private void AddressBarOnPopulateItems(object sender, BreadcrumbItemEventArgs e)
        {
            if (String.IsNullOrEmpty(_vm.CurrWorkspace.SourcePath)) return;

            BreadcrumbItem item = e.Item;
            if (item.Items.Count == 0)
            {
                PopulateFolders(item);
                e.Handled = true;
            }
        }

        private void AddressBarOnPathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            if (_vm.CurrWorkspace.NavigatedFromBrowser) return;

            if (e.NewValue == _vm.DisplayName)
            {
                //_vm.ShowHomeViewCmd.Execute();
            }
            else if (!String.IsNullOrEmpty(_vm.CurrWorkspace.SourcePath)) //是一个真实的物理路径(MF路径)
            {
                var selectedBar = e.Source as BreadcrumbBar;
                string path = null;
                var sourcePath = _vm.CurrWorkspace.SourcePath;
                if (selectedBar != null)
                {
                    path = GetPathFromBreadcrumbBar(selectedBar);
                }
                else
                {
                    path = sourcePath; //path = Path.Combine(GetSourcePath(), e.NewValue);

                }
                _vm.CurrWorkspace.NavigateTo(path);
            }
            _vm.CurrWorkspace.NavigatedFromBrowser = false;

            e.Handled = true;
        }

        private void AddressBarOnBreadcrumbItemDropDownOpened(object sender, BreadcrumbItemEventArgs e)
        {
            BreadcrumbItem item = e.Item;
            // only repopulate, if the BreadcrumbItem is dynamically generated which means, item.Data is a  pointer to itself:
            if (!(item.Data is BreadcrumbItem))
            {
                item.Items.Clear();
                PopulateFolders(item);
                e.Handled = true;
            }
        }
        #endregion

        #region  操作函数
        /// <summary>
        /// Populate the Items of the specified BreadcrumbItem with the sub folders if necassary.
        /// </summary>
        /// <param name="item"></param>
        private void PopulateFolders(BreadcrumbItem item)
        {
            try
            {
                string path0 = AddressBar.PathFromBreadcrumbItem(item);
                string trace = item.TraceValue;
                var sourcePath = GetSourcePath();
                var path = sourcePath;
                if (trace != _vm.CurrWorkspace.DisplayName) //trace != _vm.CurrWorkspace.DisplayName + "\\" && 
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
            }
            catch
            {
            }
        }

        /// <summary>
        /// 根据给出的路径设置地址栏的项
        /// </summary>
        /// <param name="path">Full URL</param>
        private void SetBreadcrumb(string path)
        {
            var paths = path.Split('\\');
            var plength = paths.Length;

            AddressBar.RootItem.Header = _vm.CurrWorkspace.DisplayName;// + "\\";
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

        private string GetSourcePath()
        {
            var sourcePath = _vm.CurrWorkspace.SourcePath;
            if (sourcePath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                sourcePath = sourcePath.Substring(8).Replace("/", "\\");
            }
            return sourcePath;
        }

        public void SetAddressBar(string path)
        {
            _vm.CurrWorkspace.NavigatedFromBrowser = true;
            SetBreadcrumb(path);
            _vm.CurrWorkspace.NavigatedFromBrowser = false;
        }

        #endregion

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void GoForward()
        {
            throw new NotImplementedException();
        }

        public string CurrentPath
        {
            get { throw new NotImplementedException(); }
        }


        public void Refresh()
        {
            
        }
    }
}
