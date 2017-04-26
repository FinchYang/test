using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AecCloud.MFilesCore;
using AecCloud.WebAPI.Models;
using log4net;
using MFilesAPI;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.DesignCloud.ViewModels
{
    public class MfVaultViewModel : INavigableWorkspace
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string Id { get; set; }
        public IWebBrowserView BrowserView { get; set; }
        /// <summary>
        /// 关联对象
        /// </summary>
        private readonly DesignCloudViewModel _parent;

        public ProjectDto Project { get; private set; }

        public MfVaultViewModel(DesignCloudViewModel parent, ProjectDto project)
        {
            _parent = parent;
            Id = project.Id.ToString();
            Project = project;
            DisplayName = project.Name;
            _goback = new RelayCommand(obj =>
            {
                if (BrowserView != null) BrowserView.GoBack();
            });
            _goforward = new RelayCommand(obj =>
            {
                if (BrowserView != null) BrowserView.GoForward();
            });
            _searchCmd = new RelayCommand(_ => SearchOp());
        }

        private Vault _vault;

        Vault GetVault()
        {
            return _vault ?? (_vault = _parent.GetVault(Project));
        }

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
                    catch (Exception ex)
                    {
                        Log.Error("登录库失败："+ ex.Message, ex);
                        _parent.ShowHomeViewCmd.Execute();
                    }
                }
                return _sourcePath;
            }
            set { _sourcePath = value; }
        }

        public string DisplayName { get; private set; }


        public string IconPath
        {
            get { return null; }
        }

        public void Refresh()
        {
            if (BrowserView != null) BrowserView.Refresh();
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

        public string SearchString
        {
            get;
            set;
        }
    }
}
