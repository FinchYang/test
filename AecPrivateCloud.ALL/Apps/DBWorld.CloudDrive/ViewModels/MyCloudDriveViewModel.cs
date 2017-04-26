using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Input;
using AecCloud.MfilesClientCore;
using AecCloud.MFilesCore;
using AecCloud.PluginInstallation;
using AecCloud.PluginInstallation.VaultApps;
using AecCloud.WebAPI.Client;
using log4net;
using MFilesAPI;
using Newtonsoft.Json;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;
using AecCloud.WebAPI.Models;

namespace DBWorld.CloudDrive.ViewModels
{
    public class MyCloudDriveViewModel : ViewModelBase, INavigableWorkspace
    {
        private readonly CloudModel _appModel;
        private readonly TokenModel _token;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _apiHost;

        public string Id { get; private set; }

        internal VaultDto Vault;

        public IWebBrowserView View { get; set; }

        internal UserDto User;

        private readonly string _dispName;

        public MyCloudDriveViewModel(CloudModel model, TokenModel token, UserDto user, string apiHost)
        {
            Vault = model.Vaults.FirstOrDefault();
            _log.Info("MyCloudDriveViewModel vault="+Vault.Guid);
            _appModel = model;
            _dispName = model.App.Name;
            User = user;
            _token = token;
            _apiHost = apiHost;
            _refreshCmd = new RelayCommand(_ => Refresh());
        }

        public string DisplayName { get { return _dispName; } }

        private string _iconPath = null;

        public string IconPath
        {
            get
            {
                if (_iconPath == null)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
                    _iconPath = String.Format("/{0};Component/Icons/Icon.png", assemblyName);
                }
                return _iconPath;
            }
        }

        private RelayCommand _home;

        public ICommand HomeCmd
        {
            get
            {
                if (_home == null)
                {
                    _home = new RelayCommand(obj =>
                    {
                        if (View != null)
                        {
                            var sourcePath = SourcePath;
                            View.SetAddressBar(sourcePath);
                        }
                    }
                        );
                }
                return _home;
            }
        }

        private IAsyncCommand _searchCmd;

        public ICommand SearchCommand
        {
            get
            {
                return _searchCmd;
            }
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


        private NotifyTaskCompletion<Uri> _currentPath;

        public NotifyTaskCompletion<Uri> CurrentPath
        {
            get
            {
                if (_currentPath == null)
                {
                    _currentPath = new NotifyTaskCompletion<Uri>(GetPath());
                }
                return _currentPath;
            }
            set
            {
                if (_currentPath == value) return;
                _currentPath = value;
                OnPropertyChanged("CurrentPath");
            }
        }

        internal async Task<Uri> GetPath()
        {
            var vaultRes = await GetVaultURL();
            return vaultRes;
        }

        private async Task<Uri> GetVaultURL()
        {
            try
            {
                if (_sourcePath != null)
                {
                    return await Task.FromResult(new Uri(_sourcePath));
                }

                _sourcePath = await Task.Run(() =>
                {
                    var v = GetVault();
                    if (v == null)
                    {
                        return null;
                    }
                    return v.GetVaultURL();
                });
                return new Uri(_sourcePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Vault _vault;

        private Vault GetVault()
        {
            if (_vault == null)
            {
                //var span = DateTime.Now - now;
                //System.Diagnostics.Debug.WriteLine("LoadApp: " + span.TotalSeconds);
                //now = DateTime.Now;
                _vault = MFilesVault.GetUserVault(User, Vault, false, null);
                if (_vault != null) AddInfo2Vault(_vault);
                //span = DateTime.Now - now;
                //System.Diagnostics.Debug.WriteLine("Connect Time: " + span.TotalSeconds);
            }
            return _vault;
        }


        void AddInfo2Vault(Vault vault)
        {
            var ns = "DBWorld." + vault.GetCurrentUserId();
            var nv = new NamedValues();
            nv[MfNamedValueCollection.DBUserToken] = JsonConvert.SerializeObject(_token);
            nv[MfNamedValueCollection.DBApiHost] = _apiHost;
            vault.NamedValueStorageOperations.SetNamedValues(MFNamedValueType.MFUserDefinedValue, ns, nv);
        }

        private RelayCommand _back;

        public ICommand GoBack
        {
            get
            {
                if (_back == null)
                {
                    _back = new RelayCommand(obj =>
                    {
                        if (View != null)
                        {
                            View.GoBack();
                        }
                    });
                }
                return _back;
            }
        }

        private RelayCommand _forward;

        public ICommand GoForward
        {
            get
            {
                if (_forward == null)
                {
                    _forward = new RelayCommand(obj =>
                    {
                        if (View != null)
                        {
                            View.GoForward();
                        }
                    });
                }
                return _forward;
            }
        }

        internal string _sourcePath;

        public string SourcePath
        {
            get
            {
                if (_sourcePath == null)
                {
                    if (CurrentPath.IsSuccessfullyCompleted)
                    {
                        _sourcePath = CurrentPath.Result.ToString();
                    }
                }
                return _sourcePath;
            } //GetVaultURL().Result.ToString()
            set { _sourcePath = value; }
        }

        public bool NavigatedFromBrowser { get; set; }

        public void NavigateTo(string uri)
        {
            if (View != null)
            {
                View.SetAddressBar(uri);
            }
        }

        private RelayCommand _refreshCmd;

        public ICommand RefreshCmd
        {
            get { return _refreshCmd; }
        }

        public void Refresh()
        {
            if (View != null) View.Refresh();
            //var path = SourcePath;
            //NavigateTo(path);
        }
    }
}
