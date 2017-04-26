using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.EnterpriseCloud.ViewModels
{
    public class EnterpriseCloudViewModel : ViewModelBase, IWorkspace
    {
        /// <summary>
        /// 插件图标路径
        /// </summary>
        private string _iconPath;

        /// <summary>
        /// 当前工作区
        /// </summary>
        private INavigableWorkspace _currworkSpace;

        /// <summary>
        /// 工作区列表
        /// </summary>
        private readonly List<INavigableWorkspace> _workSpaces = new List<INavigableWorkspace>();

        /// <summary>
        /// web地址
        /// </summary>
        internal readonly string LoginUrl;

        internal CloudModel Model;

        internal TokenModel Token;

        internal string HomeUrl;

        /// <summary>
        /// 显示首页命令
        /// </summary>
        public DelegateCommand GoHomeCmd { get; set; }

        public IWebBrowserView BrowserView { get; set; }

        internal List<string> IgnoreVaults { get; private set; }

        private Uri GetFirstUrl()
        {
            return new Uri(LoginUrl + "?token=" + Token.AccessToken + "&returnurl=" + HomeUrl);
        }

        public EnterpriseCloudViewModel(CloudModel model, TokenModel token, string loginUrl)
        {
            Model = model;
            Token = token;
            LoginUrl = loginUrl;
            HomeUrl = model.Url;
            _dispName = model.App.Name;
            GoHomeCmd = new DelegateCommand(GoHome);
        }

        public string Id { get; private set; }

        private string _dispName;

        public string DisplayName
        {
            get { return _dispName; }
        }

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

        /// <summary>
        /// 当前工作区
        /// </summary>
        public INavigableWorkspace CurrWorkspace
        {
            get
            {
                if (_currworkSpace == null)
                {
                    _currworkSpace = new MfilesVaultListViewModel(this);
                    _workSpaces.Add(_currworkSpace);
                }

                return _currworkSpace;
            }
            set
            {
                _currworkSpace = value;
                OnPropertyChanged("CurrWorkspace");
            }
        }

        /// <summary>
        /// 返回主页命令函数
        /// </summary>
        private void GoHome()
        {
            CurrWorkspace = _workSpaces[0];
        }

        /// <summary>
        ///  刷新视图
        /// </summary>
        public void Refresh()
        {
            
        }
    }
}
