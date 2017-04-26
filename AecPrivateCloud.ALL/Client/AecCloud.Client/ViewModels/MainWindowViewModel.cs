using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Documents;
using AecCloud.Client.Models;
using AecCloud.Client.Util;
using AecCloud.Client.Views;
using AecCloud.MfilesClientCore;
using AecCloud.PluginInstallation.RevitPlugins;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using log4net;
using Newtonsoft.Json;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;
using DelegateCommand = AecCloud.Client.Command.DelegateCommand;

namespace AecCloud.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private UserDto _user;
        private UserCloudModel _appModels;
        private TokenModel _token;
        private AuthenticationClient _authClient;

        /// <summary>
        /// 插件列表
        /// </summary>
        private NotifyTaskCompletion<List<IWorkspace>> _apps;

        /// <summary>
        /// 用户个人信息
        /// </summary>
        public UserProfile UserProfile { get; private set; }

        /// <summary>
        /// 显示用户信息命令
        /// </summary>
        public DelegateCommand ShowProfileCmd { get; private set; }

        public string ProductName
        {
            get { return AssemblyInfoHelper.Product; }
        }

        internal string GetUserProfileUrl()
        {
            var host = ConfigurationManager.AppSettings["homeweb"];
            return host + "/Account/logon?token=" + _token.AccessToken + "&returnUrl=" + host + "/Manage/UserInfo";
        }

        public MainWindowViewModel(UserDto user, UserCloudModel appModels, TokenModel token)
        {
            _user = user;
            _appModels = appModels;
            _token = token;
            _authClient = GetAuthClient();

            UserProfile = new UserProfile(user) { Url = GetUserProfileUrl() };
            Log.Info(string.Format("in MainWindowViewModel ,UserProfile={0}", UserProfile.Url));
            //命令
            ShowProfileCmd = new DelegateCommand(new Action(ShowProfile));
        }

        /// <summary>
        /// 验证对象
        /// </summary>
        /// <returns></returns>
        private AuthenticationClient GetAuthClient()
        {
            return _authClient ?? (_authClient = LoginViewModel.Context.GetAuthClient());
        }

        /// <summary>
        /// 显示用户信息
        /// </summary>
        private void ShowProfile()
        {
            var url = GetUserProfileUrl();
            Log.Info(string.Format("in ShowProfile ,url={0}",url));
            Process.Start(url);
        }

        protected static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        private static string GetLoginUrl()
        {
            return ConfigurationManager.AppSettings["sso"]+"Account/LogOn";
        }
        

        /// <summary>
        /// 获取协同云插件
        /// </summary>
        /// <returns></returns>
        private IWorkspace GetDesignCloud(out string[] vaultGuids)
        {
            vaultGuids = null;

            try
            {
                var designApp = _appModels.Apps.FirstOrDefault(c => c.App.Id == CloudConstants.MyProjects);

                if (_appModels == null)
                {
                    Log.Error("User apps information is invalid." );
                }

                if (designApp == null) return null;
                vaultGuids = designApp.Vaults.Select(c => c.Guid).ToArray();
                var projClient = LoginViewModel.GetProjectClient();
                var projsMessage = projClient.GetProjects(_token).Result;
                if (projsMessage.IsSuccessStatusCode)
                {
                    var projsContent = projsMessage.Content.ReadAsStringAsync().Result;
                    var projs = JsonConvert.DeserializeObject<List<ProjectDto>>(projsContent);
                    designApp.Projects.AddRange(projs);
                    LoadRevitApps();
                }
                var weburi = ConfigurationManager.AppSettings["website"];
                Log.Info(string.Format("_user={0},designApp,{1}_token,{2}LoginViewModel.Context,{3}weburi{4}",_user.Domain,
                    designApp.App.Name,_token.AccessToken,LoginViewModel.Context.BaseUri.AbsoluteUri,weburi));
                return new DBWorld.DesignCloud.ViewModels.DesignCloudViewModel(_user,
                    designApp,
                    _token,
                    LoginViewModel.Context,
                    weburi);
            }
            catch (Exception ex)
            {
                Log.Error("Loading Design Cloud failed. Exception: " + ex.Message, ex);
            }

            return null;
        }

        private void LoadRevitApps()
        {
            try
            {
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (location != null)
                {

                    var basePath = Path.Combine(location, "RevitPlugin");
                    Log.Info(string.Format("basePath={0}", basePath));
                    var pluginDict = AddinPathUtils.GetAddinDict(basePath);
                    Log.Info(string.Format("pluginDict={0}", pluginDict));
                    var needInstallDict = new Dictionary<string, string[]>();
                    foreach (var p in pluginDict)
                    {
                        var version = p.Key;
                        AddinPathUtils.RemovePluginsInSpecificFolder(version, false, location);
                        var revitInstalled = RevitInstallInfoUtils.CanbeInstalled(version);
                        if (!revitInstalled) continue;
                        var needInstallPaths = new List<string>();
                        foreach (var d in p.Value)
                        {
                            var installed = AddinPathUtils.PluginInstalledOrNoNeed(version, d, false);
                            if (!installed)
                            {
                                needInstallPaths.Add(d);
                            }
                        }
                        needInstallDict.Add(version, needInstallPaths.ToArray());
                    }
                    foreach (var d in needInstallDict)
                    {
                        foreach (var p in d.Value)
                        {
                            try
                            {
                                var err = AddinPathUtils.InstallPlugin(d.Key, p, false);
                                if (!String.IsNullOrEmpty(err))
                                {
                                    Log.Warn(err);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Loading Revit plugin failed 1. Exception: " + ex.Message, ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Loading Revit plugin failed 2. Exception: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取分包商管理
        /// </summary>
        /// <returns></returns>
        private IWorkspace GetContractManages()
        {
            var app = _appModels.Apps.FirstOrDefault(c => c.App.Id == CloudConstants.SubContracts);
            if (app == null) return null;
            //var weburi = ConfigurationManager.AppSettings["website"];
            var cm = new DBWorld.CloudDrive.ViewModels.MyCloudDriveViewModel(app, _token, _user, ConfigurationManager.AppSettings["api"]);
            return cm;
            
        }

        /// <summary>
        /// 获取项目管理
        /// </summary>
        /// <returns></returns>
        private IWorkspace GetProjectManages()
        {
            var app = _appModels.Apps.FirstOrDefault(c => c.App.Id == CloudConstants.ProjManagements);
            if (app == null) return null;
            var loginUrl = GetLoginUrl();
            return new DBWorld.EnterpriseCloud.ViewModels.EnterpriseCloudViewModel(app, _token, loginUrl);
        }

        /// <summary>
        /// 加载所有的插件
        /// </summary>
        /// <returns></returns>
        private async Task<List<IWorkspace>> LoadAllApps()
        {
            return await Task.Run(() =>
            {
                string[] designGuids;
                var apps = new List<IWorkspace>
                {        
                    GetDesignCloud(out designGuids) //协同云 
                };

                var dc = GetProjectManages(); //项目管理
                if (dc != null)
                {
                    apps.Add(dc);
                }

                var ec = GetContractManages();
                if (ec != null)
                {
                    apps.Add(ec);
                }

                return apps;
            });
        }

        /// <summary>
        /// 获取插件
        /// </summary>
        public NotifyTaskCompletion<List<IWorkspace>> Apps
        {
            get
            {
                if (_apps == null)
                {
                    _apps = new NotifyTaskCompletion<List<IWorkspace>>(
                        Task.Run(() => LoadAllApps()));
                }

                return _apps;
            }
        }
    }

}
