using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using AecCloud.MfilesClientCore;
using AecCloud.MFilesCore;
using AecCloud.PluginInstallation;
using AecCloud.PluginInstallation.VaultApps;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using DBWorld.DesignCloud.Models;
using DBWorld.DesignCloud.Util;
using log4net;
using MFilesAPI;
using Newtonsoft.Json;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBWorld.DesignCloud.ViewModels
{
    public class DesignCloudViewModel : ViewModelBase, IWorkspace
    {
        /// <summary>
        /// 是否显示左侧导航
        /// </summary>
        private bool _isShowNavigate = false;

        /// <summary>
        /// 图标路径
        /// </summary>
        private string _iconPath;

        /// <summary>
        /// 当前工作区
        /// </summary>
        private INavigableWorkspace _currWorkspace;

        /// <summary>
        /// 项目列表
        /// </summary>
        private readonly List<ProjectModel> _allProject = new List<ProjectModel>();

        /// <summary>
        /// 当前项目
        /// </summary>
        private ProjectModel _currProject;

        /// <summary>
        /// 工作区列表
        /// </summary>
        private readonly List<INavigableWorkspace> _workspaces = new List<INavigableWorkspace>();

        /// <summary>
        /// 显示项目总览（首页）
        /// </summary>
        public DelegateCommand ShowHomeViewCmd { get; private set; }

        /// <summary>
        /// 刷新命令
        /// </summary>
        public DelegateCommand RefreshCmd { get; private set; }


        internal CloudModel AppModel {get;private set;}

        internal UserDto BimUser { get; private set; }
        //public static Vault BimVault;
        internal TokenModel BimToken {get; set;}
        internal ApiClientContext Context {get; private set;}

        internal string WebURL { get; private set; }



        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region 构造函数
        public DesignCloudViewModel(UserDto user, CloudModel appModel, TokenModel token, ApiClientContext apiContext, 
            string webUrl)
        {
            //关联对象
            BimToken = token;
            BimUser = user;
            AppModel = appModel;
            Context = apiContext;

            _dispName = appModel.App.Name;

            VaultClient = Context.GetVaultClient();
            WebURL = webUrl;

            //所有项目
            foreach (var proj in appModel.Projects)
            {
                _allProject.Add(ProjDtoToProjModel(proj));
            }
            _allProject = _allProject.OrderByDescending(p => p.ProjId).ToList();

            //命令
            ShowHomeViewCmd = new DelegateCommand(ShowHomeView);
            RefreshCmd = new DelegateCommand(Refresh);
        }
        #endregion

        #region 属性
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserDto User
        {
            get { return BimUser; }
        }

        /// <summary>
        /// 插件图标
        /// </summary>
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

        private readonly string _dispName;
        /// <summary>
        /// 插件名称
        /// </summary>
        public string DisplayName { get { return _dispName; } }

        /// <summary>
        /// 当前工作区
        /// </summary>
        public INavigableWorkspace CurrWorkspace
        {
            get
            {
                if (_currWorkspace == null)
                {
                    _currWorkspace = new ProjectCategoryViewModel(this);
                    _workspaces.Add(_currWorkspace);
                }
                return _currWorkspace;
            }
        }

        /// <summary>
        /// 所有项目
        /// </summary>
        public List<ProjectModel> AllProjects
        {
            get
            {
                return _allProject;
            }
        }

        /// <summary>
        /// 当前项目
        /// </summary>
        public ProjectModel CurrProject
        {
            get
            {
                if (_currProject == null)
                {
                    _currProject = new ProjectModel();
                }
                return _currProject;
            }
            set
            {
                _currProject = value;
                OnPropertyChanged("CurrProject");
                //ShowOrHideBimTab();
            }
        }

        /// <summary>
        /// 是否显示左侧导航
        /// </summary>
        public bool IsShowNavigate
        {
            get
            {
                return _isShowNavigate;
            }
            set
            {
                _isShowNavigate = value;
                OnPropertyChanged("IsShowNavigate");
            }
        }
        #endregion

        #region 命令函数
        /// <summary>
        /// 显示项目总览（首页）
        /// </summary>
        private void ShowHomeView()
        {
            WaitCursorUtil.SetBusyState();
            var workspace = _workspaces.FirstOrDefault(c => c is ProjectCategoryViewModel);
            if (workspace == null)
            {
                workspace = new ProjectCategoryViewModel(this);
                _workspaces.Add(workspace);
            }
            _currWorkspace = workspace;
            OnPropertyChanged("CurrWorkspace");
        }
      
        public void SetCurrentViewFromProject(ProjectModel proj0)
        {
          //  Writelog("in SetCurrentViewFromProject");
            if (!_updatedTemplates)
            {
              //  Writelog("in 11");
                try
                {
                  //  Writelog("in 22");
                    var client = Context.GetProjectClient();
                  //  Writelog("in 33");
                    var res = client.GetProjectTemplates(BimToken).Result;
                  //  Writelog("in 44");
                    Log.Info("初始化模版信息："+res.StatusCode);
                }
                catch (Exception ex)
                {
Log.Error("in 55"+ex.Message);
                }
                _updatedTemplates = true;
            }
          //  Writelog("in 66");
            IsShowNavigate = true;
            var proj = AppModel.Projects.FirstOrDefault(c => c.Id == proj0.ProjId);
            var workspace = _workspaces.FirstOrDefault(c => c.Id == proj.Id.ToString());
            if (workspace == null)
            {
              //  Writelog("in 77");
                workspace = new MfVaultViewModel(this, proj);
                _workspaces.Add(workspace);
              //  Writelog("in 88");
            }
            _currWorkspace = workspace;
            OnPropertyChanged("CurrWorkspace");
         //   Writelog("in 99");
        }

        private bool _updatedTemplates;
        /// <summary>
        /// 对象转换
        /// </summary>
        /// <returns></returns>
        internal ProjectModel ProjDtoToProjModel(ProjectDto proj)
        {
            if (proj == null) return null;
            var model = new ProjectModel
            {
                ProjId = proj.Id,
                ProjName = proj.Name,
                ProjNumber = proj.Number,
                ProjDescription = proj.Description,
                ProjCover = proj.Cover,
                ProjTemplateId = proj.TemplateId,
                //ProjTemplate = new TemplateModel
                //{
                //    TemplateId = proj.Template.Id,
                //    TemplateName = proj.Template.Name,
                //    TemplateDescription = proj.Template.Description
                //},
                //ProjMembers = GetProjectMembers(proj.Id),
                ProjStatus = proj.Status.Name,
                ProjEndTime = proj.EndDateUtc.ToLocalTime(),
                ProjStartTime = proj.StartDateUtc.ToLocalTime(),
                OwnerUnit = proj.OwnerUnit,
                DesignUnit = proj.DesignUnit,
                ConstructionUnit = proj.ConstructionUnit,
                SupervisionUnit = proj.SupervisionUnit
            };
            return model;
        }

        ///// <summary>
        ///// 获取项目成员
        ///// </summary>
        ///// <param name="projId"></param>
        ///// <returns></returns>
        //private List<UserModel> GetProjectMembers(long projId)
        //{
        //    var client = Context.GetProjectClient();
        //    if (client == null) return null;
        //    var members = new List<UserModel>();
        //    var res = client.GetProjectMembers(projId, BimToken);
        //    if (res.Result.StatusCode != HttpStatusCode.OK) return null;
        //    var json = res.Result.Content.ReadAsStringAsync().Result;
        //    var users = JsonConvert.DeserializeObject<List<UserDto>>(json);
        //    if (users.Count > 0)
        //    {
        //        foreach (var user in users)
        //        {
        //            var userModel = new UserModel
        //            {
        //                UserId = user.Id,
        //                UserName = user.UserName,
        //                Email = user.Email,
        //                Password = user.Password
        //            };
        //            members.Add(userModel);
        //        }
        //    }

        //    return members;
        //}

        internal readonly VaultClient VaultClient;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proj"></param>
        /// <returns>是否需要加载App</returns>
        internal bool LoadApps(ProjectDto proj)
        {
            var token = BimToken;
            //var proj = GetProject();
            //if (proj == null) return;
            var guid = proj.Vault.Guid;
            var loaded = VaultAppDict.ContainsKey(guid);
            if (loaded) return false;

            var appDefs = ClientUtils.GetVaultSysAppDefFiles(guid);

            //if (appDefs.Count > 0) return true; // todo

            var appList = new AppDescList();
            foreach (var a in appDefs)
            {
                string err;
                var defObj = SerialUtils.GetObject<VaultAppDefFile>(a, out err);
                if (defObj == null) continue;
                appList.Apps.Add(new AppDesc { Guid = defObj.Guid, Version = defObj.Version });
            }


            var res = VaultClient.AppsNeeded(proj.Vault.Id, appList, token).Result;
            var resStr = res.Content.ReadAsStringAsync().Result;
            if (!res.IsSuccessStatusCode)
            {
                Log.Info("LoadApps failure?");
                return false;
                //throw new Exception("无法获取需要加载的库应用：" + resStr);
            }
            var apps = JsonConvert.DeserializeObject<List<VaultAppModel>>(resStr);
            Log.Info("loadapps apps.count:"+apps.Count);
            //if (!User.CloudAppEnabled)
            //{
            //    apps = apps.Where(c => !c.CloudAppEnabled).ToList();
            //}
            
            var zipFiles = new List<string>();
            var exportPath = Path.GetTempPath();
            var appPath = ClientUtils.GetAppPath(guid);

            foreach (var a in apps)
            {
                var path = Path.Combine(exportPath, a.Guid + ".zip");
                File.WriteAllBytes(path, a.ZipFile);
                var needUpdate = VaultAppUtils.NeedUpdate(appPath, a.Guid, path);
                if (!needUpdate) continue;
                zipFiles.Add(path);
            }
            if (zipFiles.Count == 0)
            {
                VaultAppDict.Add(guid, apps);
                return false;
            }
            var isAdmin = IsAdministrator();
            if (isAdmin)
            {
                var errs = VaultAppUtils.ExtractApps(appPath, zipFiles.ToArray(), Log);
                if (errs.Count > 0 && Log != null)
                {
                    Log.Warn(String.Join("; ", errs));
                }
            }
            else
            {
                var fileStr = zipFiles.Select(c => "\"" + c + "\"").ToArray();
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var exeFile = Path.Combine(location, "AecCloud.ClientConsole.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = exeFile,
                    Arguments = String.Format("-u 1 -t vaultapp -p \"{0}\" -d {1}", appPath.TrimEnd('\\'), String.Join(" ", fileStr)),
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Environment.CurrentDirectory
                };
                try
                {
                    var p = Process.Start(startInfo);
                    var exitCode = -1;
                    if (p != null)
                    {
                        p.WaitForExit();
                        exitCode = p.ExitCode;
                    }
                    if (exitCode != 0) Log.Error("加载VaultApp失败：" + exitCode);
                }
                catch (System.ComponentModel.Win32Exception)//用户点击取消的异常
                {
                    
                }
            }
            VaultAppDict.Add(guid, apps);
            return true;
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

        internal Vault GetVault(ProjectDto proj)
        {
            var vault = proj.Vault;
            if (!VaultDict.ContainsKey(vault.Guid) || VaultDict[vault.Guid] == null)
            {
             //   var vaultName = proj.Name + "-" + proj.Id;
                var needLoad = LoadApps(proj);
                Log.Info(string.Format(" in cloudv,GetVault(),userName={0}, pwd={1}, domai={2}", User.UserName, User.Password, User.Domain));
               // var mfVault = MFilesVault.GetUserVault(User, vault, needLoad, vaultName);
                var mfVault = MFilesVault.GetUserVault(User, vault, needLoad, null);
                AddUserInfo2Vault(mfVault, proj);
                VaultDict.Add(vault.Guid, mfVault);
            }

            return VaultDict[vault.Guid];
        }

        /// <summary>
        /// 获取Vault
        /// </summary>
        /// <returns></returns>
        internal Vault GetCurrVault()
        {
            ProjectDto proj = null;
            if (_currProject != null)
            {
                proj = AppModel.Projects.FirstOrDefault(p => p.Id == _currProject.ProjId);
            }
            else
            {
                proj = AppModel.Projects.FirstOrDefault();
            }

            if (proj == null) return null;
            return GetVault(proj);
        }
        

        private void AddUserInfo2Vault(Vault vault, ProjectDto proj)
        {
            var ns = "DBWorld." + vault.GetCurrentUserId();
            var nv = new NamedValues();
            //nv.Names.Add(-1, MFUserToken);
            nv[MfNamedValueCollection.DBUserId] = BimUser.Id;
            nv[MfNamedValueCollection.DBUserEmail] = BimUser.Email;
            nv[MfNamedValueCollection.DBUserToken] = JsonConvert.SerializeObject(BimToken);
            nv[MfNamedValueCollection.DBProjectId] = proj.Id;
            nv[MfNamedValueCollection.DBApiHost] = Context.BaseUri.ToString();
            nv[MfNamedValueCollection.DBWebHost] = WebURL;
            nv[MfNamedValueCollection.DBInstallPath] = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            vault.NamedValueStorageOperations.SetNamedValues(MFNamedValueType.MFUserDefinedValue, ns, nv);
        }

        #endregion

        #region Vaults

        private readonly Dictionary<string, List<VaultAppModel>> _vaultappDict = new Dictionary<string, List<VaultAppModel>>();

        public IDictionary<string, List<VaultAppModel>> VaultAppDict
        {
            get { return _vaultappDict; }
        }

        private readonly Dictionary<string, Vault> _vaultDict = new Dictionary<string, Vault>();

        public IDictionary<string, Vault> VaultDict
        {
            get { return _vaultDict; }
        }

        private readonly List<ProjectPartyDto> _vaultParties =
            new List<ProjectPartyDto>();

        public List<ProjectPartyDto> VaultParties
        {
            get { return _vaultParties; }
        }

        private readonly Dictionary<int, Dictionary<string, List<MFilesUserGroupDto>>> _projUserGroupsDict =
            new Dictionary<int, Dictionary<string, List<MFilesUserGroupDto>>>();

        public Dictionary<int, Dictionary<string, List<MFilesUserGroupDto>>> ProjectUserGroupsDict
        {
            get { return _projUserGroupsDict; }
        }

        private readonly Dictionary<int, Dictionary<string, List<UserDto>>> _projMembersDict = new Dictionary<int, Dictionary<string, List<UserDto>>>();

        public Dictionary<int, Dictionary<string, List<UserDto>>> ProjectMembersDict
        {
            get { return _projMembersDict; }
        }

        #endregion Vaults

        #region 实现接口

        public string Id { get; private set; }

        public IWebBrowserView BrowserView { get; set; }
        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            WaitCursorUtil.SetBusyState();
            CurrWorkspace.Refresh();
        }
        #endregion
    }
}
