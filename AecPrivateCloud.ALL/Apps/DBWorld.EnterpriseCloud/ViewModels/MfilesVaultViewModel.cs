using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;
using AecCloud.MFilesCore;
using MFilesAPI;

namespace DBWorld.EnterpriseCloud.ViewModels
{
    public class MfilesVaultViewModel : ViewModelBase, INavigableWorkspace
    {
        public IWebBrowserView BrowserView { get; set; }
        /// <summary>
        /// 关联对象
        /// </summary>
        private EnterpriseCloudViewModel _parent;

        /// <summary>
        /// 库图标路径
        /// </summary>
        private string _iconPath;

        /// <summary>
        /// 库链接对象
        /// </summary>
        private readonly MfVaultConnection _mfConnection;

        public MfilesVaultViewModel(EnterpriseCloudViewModel parent, MfVaultConnection connection)
        {
            _parent = parent;
            _mfConnection = connection;
            DisplayName = connection.Name;
            _goback = new RelayCommand(_ =>
            {
                if (BrowserView != null) BrowserView.GoBack();
            });
            _goforward = new RelayCommand(_ =>
            {
                if (BrowserView != null) BrowserView.GoForward();
            });
            _searchCmd = new RelayCommand(_=>SearchOp());
        }

        private Vault _vault;

        Vault GetVault()
        {
            if (_vault == null)
            {
                _vault = _mfConnection.BindToVault();
            }
            return _vault;
        }

        public string Id { get; private set; }

        private string _sourcePath;

        public string SourcePath
        {
            get
            {
                if (_sourcePath == null)
                {
                    try
                    {
                        var vault = GetVault();
                        _sourcePath = vault.GetVaultURL();
                    }
                    catch (Exception)
                    {
                        _parent.GoHomeCmd.Execute();
                    }
                }
                return _sourcePath;
            }
            set
            {
                _sourcePath = value;
                OnPropertyChanged("SourcePath");
            }
        }

        public string DisplayName { get; private set; }

        public string IconPath
        {
            get
            {
                if (_iconPath == null)
                {
                    var majorVersion = ClientUtils.GetMajorVersion();
                    if (String.IsNullOrEmpty(_mfConnection.IconPath) || !File.Exists(_mfConnection.IconPath))
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
                        if (majorVersion < 11)
                        {
                            _iconPath = String.Format("/{0};Component/Icons/default.ico", assemblyName);
                        }
                        else
                        {
                            _iconPath = String.Format("/{0};Component/Icons/default2015.ico", assemblyName);
                        }
                    }
                    else
                    {
                        _iconPath = _mfConnection.IconPath;
                    }
                }

                return _iconPath;
            }
        }

        public void Refresh()
        {
        }

        private RelayCommand _goback;

        public ICommand GoBack
        {
            get { return _goback; }
        }

        private RelayCommand _goforward;

        public ICommand GoForward
        {
            get { return _goforward; }
        }


        public bool NavigatedFromBrowser
        {
            get;
            set;
        }

        public void NavigateTo(string uri)
        {
            if (BrowserView != null)
            {
                BrowserView.SetAddressBar(uri);
            }
        }

        public void SetAddress(string uri)
        {
            _parent.BrowserView.SetAddressBar(uri);
        }

        private void SearchOp()
        {
            var vault = GetVault();
            try
            {
                var url = vault.GetSearchView(SearchString);
                NavigateTo(url);
            }
            catch
            {
            }
            //SourcePath = url;
        }

        private readonly ICommand _searchCmd;

        public ICommand SearchCommand
        {
            get { return _searchCmd; }
        }

        private string _searchStr;

        public string SearchString
        {
            get { return _searchStr; }
            set
            {
                if (_searchStr == value) return;
                _searchStr = value;
                OnPropertyChanged("SearchString");
            }
        }
    }
}
