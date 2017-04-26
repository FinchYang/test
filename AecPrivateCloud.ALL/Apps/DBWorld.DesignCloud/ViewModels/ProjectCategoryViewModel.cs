using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using DBWorld.DesignCloud.Models;
using DBWorld.DesignCloud.Util;
using DBWorld.DesignCloud.Views;
using Newtonsoft.Json;
using SimulaDesign.WPFCustomUI.Controls;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.DesignCloud.ViewModels
{
    public class ProjectCategoryViewModel : ViewModelBase, INavigableWorkspace
    {
        #region 属性 字段
        private const string SetupedString = "已立项项目";
        private const string StartingString = "启动中的项目";
        private const string PausedString = "已暂停项目";
        private const string EndString = "已结束项目";

        private const string BackgroudImgPath = "/DBWorld.DesignCloud;component/Images/Bg/empty.png";

        /// <summary>
        /// 关联对象
        /// </summary>
        private readonly DesignCloudViewModel _parent;

        /// <summary>
        /// 初始化数据工作线程
        /// </summary>
        private BackgroundWorker _initBkWork;

        /// <summary>
        /// 获取项目操作
        /// </summary>
        private ProjectClient _projClient;

        /// <summary>
        /// 是否显示动画
        /// </summary>
        private bool _isShowAdorner = true;

        /// <summary>
        /// 修改项目状态的状态条是否可用
        /// </summary>
        private bool _isEnableChange = false;

        /// <summary>
        /// 上次的项目状态
        /// </summary>
        private string _lastStatus;

        /// <summary>
        ///选择的项目状态的描述的序号
        /// </summary>
        private int _statusStringIndex;

        /// <summary>
        /// 选择的项目状态（与项目状态描述有区别）
        /// </summary>
        private string _currProjStatus = ProjectModel.ProjStatusStarted;

        /// <summary>
        /// 选择的项目
        /// </summary>
        private ProjectModel _projectSelected;

        /// <summary>
        /// 默认项目
        /// </summary>
        private static ProjectModel _defaultProject;

        /// <summary>
        /// 项目分类列表
        /// </summary>
        private ObservableCollection<ProjectModel> _categoryProjects;

        /// <summary>
        /// 创建新项目命令
        /// </summary>
        public DelegateCommand AddProjectCmd { get; private set; }

        /// <summary>
        /// 选择项目命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> ProjSelectionChangedCmd { get; private set; }

        /// <summary>
        /// 双击的项目命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> ProjMouseDoubleClickCmd { get; private set; }

        /// <summary>
        /// 项目状态选择
        /// </summary>
        public DelegateCommand<ExCommandParameter> StatusSelectionChangedCmd { get; private set; }

        /// <summary>
        /// 修改项目状态命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> SettingSelectionChangedCmd { get; private set; }

        /// <summary>
        /// 项目工时汇总命令
        /// </summary>
        public DelegateCommand ShowProjectsHoursCmd { get; private set; }
        /// <summary>
        /// 项目综合管理命令
        /// </summary>
        public DelegateCommand ShowProjectsIntegratedManagementCmd { get; private set; } 

        #endregion

        #region 构造函数
        public ProjectCategoryViewModel(DesignCloudViewModel parent)
        {
            _parent = parent;

            //命令
            AddProjectCmd = new DelegateCommand(new Action(AddProjectForAllBackup));
            ProjSelectionChangedCmd = new DelegateCommand<ExCommandParameter>(
                new Action<ExCommandParameter>(ProjSelectionChanged));
            ProjMouseDoubleClickCmd = new DelegateCommand<ExCommandParameter>(
                new Action<ExCommandParameter>(ProjMouseDoubleClick));
            StatusSelectionChangedCmd = new DelegateCommand<ExCommandParameter>(
                new Action<ExCommandParameter>(StatusSelectionChanged));
            SettingSelectionChangedCmd = new DelegateCommand<ExCommandParameter>(
                new Action<ExCommandParameter>(SettingSelectionChanged));
            ShowProjectsHoursCmd = new DelegateCommand(ShowProjectsHours);
            ShowProjectsIntegratedManagementCmd = new DelegateCommand(ShowProjectsIntegratedManagement);

            //加载数据
            _initBkWork = new BackgroundWorker();
            _initBkWork.DoWork += InitBkWorker;
            _initBkWork.RunWorkerCompleted += InitBkWorkerCompleted;
            _initBkWork.RunWorkerAsync();
        }
        #endregion

        #region 工作区信息

        public string Id { get; set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        public string DisplayName { get { return "项目总览"; } }

        /// <summary>
        /// 插件图标
        /// </summary>
        public string IconPath
        {
            get { return ""; }
        }
        #endregion


        #region 绑定属性

        /// <summary>
        /// 默认项目（暂时保留）
        /// </summary>
        public ProjectModel DefaultProject
        {
            get
            {
                if (_defaultProject == null)
                {
                    _defaultProject = new ProjectModel();
                }

                return _defaultProject;
            }
            set
            {
                _defaultProject = value;
                OnPropertyChanged("DefaultProject");
            }
        }

        /// <summary>
        /// 是否显示动画
        /// </summary>
        public bool IsShowAdorner
        {
            get { return _isShowAdorner; }
            set
            {
                _isShowAdorner = value;
                OnPropertyChanged("IsShowAdorner");
            }
        }

        /// <summary>
        /// 项目状态描述列表
        /// </summary>
        public List<string> StatusStringList
        {
            get
            {
                return new List<string>
                {
                    SetupedString,
                    StartingString,
                    PausedString,
                    EndString
                };
            }
        }

        /// <summary>
        /// 选择的项目状态的描述的序号
        /// </summary>
        public int StatusStringIndex
        {
            get
            {
                return _statusStringIndex;
            }
            set
            {
                _statusStringIndex = value;
                OnPropertyChanged("StatusStringIndex");
            }
        }

        /// <summary>
        /// 项目状态列表
        /// </summary>
        public List<string> ProjStatusList
        {
            get
            {
                return new List<string>
                {
                    ProjectModel.ProjStatusSetUped,
                    ProjectModel.ProjStatusStarted,
                    ProjectModel.ProjStatusPaused,
                    ProjectModel.ProjStatusOvered
                };
            }
        }

        /// <summary>
        /// 修改项目状态的状态条是否可用
        /// </summary>
        public bool IsEnableChange
        {
            get { return _isEnableChange; }
            set
            {
                _isEnableChange = value;
                OnPropertyChanged("IsEnableChange");
            }
        }

        #endregion

        #region 绑定数据
        /// <summary>
        /// 项目分类列表
        /// </summary>
        public ObservableCollection<ProjectModel> CategoryProjects
        {
            get
            {
                if (_categoryProjects == null)
                {
                    _categoryProjects = new ObservableCollection<ProjectModel>();
                }

                _categoryProjects.Clear();
                var list = _parent.AllProjects.Where(p => p.ProjStatus == _currProjStatus).ToList();
                foreach (var model in list)
                {
                    _categoryProjects.Add(model);
                }

                return _categoryProjects;
            }
        }

        /// <summary>
        ///  背景图片
        /// </summary>
        public string BackgroundImgPath
        {
            get
            {
                if (_parent.AllProjects.Count == 0)
                {
                    return BackgroudImgPath;
                }
                return null;
            }
        }

        /// <summary>
        /// 是否显示背景图片
        /// </summary>
        public bool ShowBackgroundImg
        {
            get
            {
                if (_parent.AllProjects.Count == 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否显示项目列表
        /// </summary>
        public bool ShowProjectList
        {
            get
            {
                if (_parent.AllProjects.Count == 0)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region 命令函数
        /// <summary>
        /// 获取参与方
        /// </summary>
        /// <returns></returns>
        private List<UserGroupModel> GetAllParties()
        {
            var list = new List<UserGroupModel>();
            var res = _projClient.GetAllParties(_parent.BimToken);
            if (res.Result.StatusCode != HttpStatusCode.OK) return null;
            var resStr = res.Result.Content.ReadAsStringAsync().Result;
            var parties = JsonConvert.DeserializeObject<List<ProjectPartyDto>>(resStr);
            if (parties.Count > 0)
            {
                list.AddRange(parties.Select(party => new UserGroupModel
                {
                    Id = party.Id,
                    Name = party.Name,
                    Description = party.Description,
                    Alias = party.Alias
                }));
            }

            return list;
        }

        private CloudClient _appClient;

        /// <summary>
        /// 获取app对象
        /// </summary>
        /// <returns></returns>
        internal CloudClient GetAppClient()
        {
            return _appClient ?? (_appClient = _parent.Context.GetCloudClient());
        }

        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<CompanyDto> GetCompanies(TokenModel token)
        {
            var appClient = GetAppClient();
            var res = appClient.GetCompanies(token).Result;
            if (!res.IsSuccessStatusCode) return new List<CompanyDto>();
            return JsonConvert.DeserializeObject<List<CompanyDto>>(res.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<AreaDto> GetAreas(TokenModel token)
        {
            var appClient = GetAppClient();
            var res = appClient.GetAreas(token).Result;
            if (!res.IsSuccessStatusCode) return new List<AreaDto>();
            return JsonConvert.DeserializeObject<List<AreaDto>>(res.Content.ReadAsStringAsync().Result);
        }

        private List<ProjectLevelDto> GetLevels(TokenModel token)
        {
            var appClient = GetClient();
            var res = appClient.GetLevels(token).Result;
            if (!res.IsSuccessStatusCode)
            {
                return new List<ProjectLevelDto>
                {
                    new ProjectLevelDto {Id = 1, Name = "一般直属"},
                    new ProjectLevelDto {Id = 2, Name = "公司重点"},
                    new ProjectLevelDto {Id = 3, Name = "局重点"}
                };
            }
            return JsonConvert.DeserializeObject<List<ProjectLevelDto>>(res.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// 添加新项目
        /// </summary>
        private void AddProject()
        {
            //判断是否有模板

            try
            {
                var res = _projClient.GetProjectTemplates(_parent.BimToken);
                if (res.Result.StatusCode == HttpStatusCode.OK)
                {
                    var json = res.Result.Content.ReadAsStringAsync().Result;
                    var templates = JsonConvert.DeserializeObject<List<VaultTemplateDto>>(json);
                    if (templates == null || templates.Count == 0)
                    {
                        var dr = MetroMessageBox.Show(
                            "无法获取模版",
                            "新建项目",
                            MetroMessageBoxButton.OK,
                            MetroMessageBoxImage.Info,
                            MetroMessageBoxDefaultButton.OK);
                        return;
                    }
                }
                else
                {
                    var dr = MetroMessageBox.Show(
                            "获取模版出错: " + res.Result.Content.ReadAsStringAsync().Result,
                            "新建项目",
                            MetroMessageBoxButton.OK,
                            MetroMessageBoxImage.Info,
                            MetroMessageBoxDefaultButton.OK);
                    return;
                }
            }
            catch (Exception)
            {
                MetroMessageBox.Show("初始化项目模板失败！",
                    "新建项目",
                    MetroMessageBoxButton.OK,
                    MetroMessageBoxImage.Error,
                    MetroMessageBoxDefaultButton.OK);
                return;
            }

            var companies = GetCompanies(_parent.BimToken);

            //新建项目
            var model = new ProjectModel();
            var vm = new ProjectSettingViewModel(_parent, GetAllParties(), companies, GetAreas(_parent.BimToken), model);
            var vw = new ProjectSettingView { Title = vm.DisplayName, DataContext = vm };
            vw.ShowDialog();
            if (vw.DialogResult == true)
            {
                //获取输入的项目信息
                var newProj = new ProjectCreateModel
                {
                    Name = model.ProjName,
                    Number = model.ProjNumber,
                    Description = model.ProjDescription,
                    TemplateId = model.ProjTemplateId, //.ProjTemplate.TemplateId,
                    Cover = model.ProjCover,
                    StartDateUtc = model.ProjStartTime.ToUniversalTime(),
                    EndDateUtc = model.ProjEndTime.ToUniversalTime(),
                    ProjectPartyId = model.PartyId,
                    CompanyId = model.CompanyId,
                    AreaId = model.AreaId
                };

                //Action<object> action = NewProjectTask;
                //Task.Factory.StartNew(action, newProj);
                ThreadPool.QueueUserWorkItem(NewProjectTask, newProj);
            }
        }
        private void AddProjectForAllBackup()
        {
            var companies = GetCompanies(_parent.BimToken);

            //新建项目
            var model = new ProjectModel();
            var vm = new ProjectSettingViewModelForAllBackup(_parent, GetAllParties(), 
                companies, GetAreas(_parent.BimToken), GetLevels(_parent.BimToken), model);
            var vw = new ProjectSettingViewForAllBackup() { Title = vm.DisplayName, DataContext = vm };
            vw.ShowDialog();
            if (vw.DialogResult == true)
            {
                //获取输入的项目信息
                var newProj = new ProjectCreateModel
                {
                    Name = model.ProjName,
                    Number = model.ProjNumber,
                    Description = model.ProjDescription,
                    //  TemplateId = model.ProjTemplateId, //.ProjTemplate.TemplateId,
                    TemplateId = 4, //.ProjTemplate.TemplateId,
                    Cover = model.ProjCover,
                    StartDateUtc = model.ProjStartTime.ToUniversalTime(),
                    EndDateUtc = model.ProjEndTime.ToUniversalTime(),
                    ProjectPartyId = model.PartyId,
                    CompanyId = model.CompanyId,
                    AreaId = model.AreaId,
                    ProjectClass = model.ProjectClass,
                    OwnerUnit = model.OwnerUnit,
                    PropSupervisorUnit = model.SupervisionUnit,
                    ConstructionScale = model.ConstructScale,
                    ContractAmount = model.ContractAmount,
                    PropDesignUnit = model.DesignUnit,
                    ProjectLevelId = model.LevelId
                };

                //Action<object> action = NewProjectTask;
                //Task.Factory.StartNew(action, newProj);
                ThreadPool.QueueUserWorkItem(NewProjectTask, newProj);
            }
        }
        /// <summary>
        /// 选择的项目
        /// </summary>
        private void ProjSelectionChanged(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            _projectSelected = param.Parameter as ProjectModel;

            if (_projectSelected != null)
            {  
                //保存项目状态
                _lastStatus = _projectSelected.ProjStatus;

                //保存选择的项目
                _parent.CurrProject = _projectSelected;
                WriteDefProjectToXml(_projectSelected);
            }
        }

        /// <summary>
        ///  双击的项目
        /// </summary>
        /// <param name="param"></param>
        private void ProjMouseDoubleClick(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            _parent.SetCurrentViewFromProject(_parent.CurrProject);
        }

        /// <summary>
        /// 选择项目状态
        /// </summary>
        /// <param name="param"></param>
        private void StatusSelectionChanged(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            var str = param.Parameter as string;
            _currProjStatus = GetProjectsStatus(str);
            OnPropertyChanged("CategoryProjects");
        }

        /// <summary>
        /// 设置立项状态
        /// </summary>
        /// <param name="param"></param>
        private string SetSetupedStatus(long param)
        {
            var res = _projClient.ProposalProject(param, _parent.BimToken);
            if (res.Result.StatusCode != HttpStatusCode.OK)
            {
                return res.Result.Content.ReadAsStringAsync().Result;
            }

            var oldProject = _parent.AllProjects.FirstOrDefault(p => p.ProjId == param);
            if (oldProject == null) return null;
            var newProject = UpdateProjectStatus(oldProject, ProjectModel.ProjStatusSetUped);
            _parent.AllProjects.Remove(oldProject);
            _parent.AllProjects.Add(newProject);

            OnPropertyChanged("CategoryProjects");

            return null;
        }

        /// <summary>
        /// 设置启动状态
        /// </summary>
        /// <param name="param"></param>
        private string SetStartedStatus(long param)
        {
            var res = _projClient.StartProject(param, _parent.BimToken);
            if (res.Result.StatusCode != HttpStatusCode.OK)
            {
                return res.Result.Content.ReadAsStringAsync().Result;
            }

            var oldProject = _parent.AllProjects.FirstOrDefault(p => p.ProjId == param);
            if (oldProject == null) return null;
            var newProject = UpdateProjectStatus(oldProject, ProjectModel.ProjStatusStarted);
            _parent.AllProjects.Remove(oldProject);
            _parent.AllProjects.Add(newProject);

            OnPropertyChanged("CategoryProjects");

            return null;
        }

        /// <summary>
        /// 设置暂停状态
        /// </summary>
        /// <param name="param"></param>
        private string SetPausedStatus(long param)
        {
            var res = _projClient.PauseProject(param, _parent.BimToken);
            if (res.Result.StatusCode != HttpStatusCode.OK)
            {
                return res.Result.Content.ReadAsStringAsync().Result;
            }

            var oldProject = _parent.AllProjects.FirstOrDefault(p => p.ProjId == param);
            if (oldProject == null) return null;
            var newProject = UpdateProjectStatus(oldProject, ProjectModel.ProjStatusPaused);
            _parent.AllProjects.Remove(oldProject);
            _parent.AllProjects.Add(newProject);

            OnPropertyChanged("CategoryProjects");

            return null;
        }

        /// <summary>
        /// 设置结束状态
        /// </summary>
        /// <param name="param"></param>
        private string SetOveredStatus(long param)
        {
            var res = _projClient.EndProject(param, _parent.BimToken);
            if (res.Result.StatusCode != HttpStatusCode.OK)
            {
                return res.Result.Content.ReadAsStringAsync().Result;
            }

            var oldProject = _parent.AllProjects.FirstOrDefault(p => p.ProjId == param);
            if (oldProject == null) return null;
            var newProject = UpdateProjectStatus(oldProject, ProjectModel.ProjStatusOvered);
            _parent.AllProjects.Remove(oldProject);
            _parent.AllProjects.Add(newProject);

            OnPropertyChanged("CategoryProjects");

            return null;
        }

        /// <summary>
        /// 修改项目状态命令
        /// </summary>
        /// <param name="param"></param>
        private void SettingSelectionChanged(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            var status = param.Parameter.ToString();

            //当前项目状态与修改的项目状态一致，不需要修改
            if (String.Compare(_projectSelected.ProjStatus, _lastStatus, StringComparison.OrdinalIgnoreCase) == 0) return;

            //项目状态为”结束“的项目，不允许修改
            if (String.Compare(_lastStatus, ProjectModel.ProjStatusOvered, StringComparison.OrdinalIgnoreCase) == 0)
            {
                MetroMessageBox.Show("已结束项目，项目状态不可修改！",
                    "设置项目状态",
                    MetroMessageBoxButton.OK,
                    MetroMessageBoxImage.Info,
                    MetroMessageBoxDefaultButton.OK);
                //恢复项目状态
                _projectSelected.ProjStatus = _lastStatus;
                return;
            }

            var projId = _projectSelected.ProjId;
            string result = string.Empty;
            var msg = String.Format("确定将项目“{0}”状态修改为“{1}”?",
                _projectSelected.ProjName,
                status);
            if (MetroMessageBox.Show(msg,
               "设置项目状态",
               MetroMessageBoxButton.OKCancel,
               MetroMessageBoxImage.Question,
               MetroMessageBoxDefaultButton.OK) == MetroMessageBoxResult.OK)
            {
                WaitCursorUtil.SetBusyState();
                if (String.Compare(status, ProjectModel.ProjStatusSetUped, StringComparison.OrdinalIgnoreCase) == 0)
                {
                   result = SetSetupedStatus(projId);
                }
                else if (String.Compare(status, ProjectModel.ProjStatusStarted, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = SetStartedStatus(projId);
                }
                else if (String.Compare(status, ProjectModel.ProjStatusPaused, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = SetPausedStatus(projId);
                }
                else if (String.Compare(status, ProjectModel.ProjStatusOvered, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = SetOveredStatus(projId);
                }
            }
            else
            {
                //恢复项目状态
                _projectSelected.ProjStatus = _lastStatus;
            }

            //显示操作结果
            if (!string.IsNullOrEmpty(result))
            {
                MetroMessageBox.Show(ResponContentUtil.GetResponResult(result), 
                    "设置项目状态",
                    MetroMessageBoxButton.OK,
                    MetroMessageBoxImage.Error,
                    MetroMessageBoxDefaultButton.OK);
                //恢复项目状态
                _projectSelected.ProjStatus = _lastStatus;
            }
        }

        /// <summary>
        /// 项目工时汇总
        /// </summary>
        private void ShowProjectsHours()
        {
            var hoursUrl = string.Format("{0}/Account/LogOn?token={1}&returnurl=/WorkingHour", 
                _parent.WebURL, _parent.BimToken.AccessToken);
            Process.Start(hoursUrl);
        }
        /// <summary>
        /// 项目综合管理
        /// </summary>
        private void ShowProjectsIntegratedManagement()
        {
            var hoursUrl = string.Format("{0}/Account/LogOn?token={1}&returnurl=/bim/index",
                _parent.WebURL, _parent.BimToken.AccessToken,_parent.AppModel.Url);
            Process.Start(hoursUrl);
        }
        #endregion

        #region 操作函数
        private ProjectClient GetClient()
        {
            return _projClient ?? (_projClient = _parent.Context.GetProjectClient());
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitProjectData()
        {
            //项目操作对象
            _projClient = GetClient();

            //项目列表
            if (_parent.AllProjects == null) return;

            //初始显示的项目分类
            if (_parent.AllProjects.Count == 0)
            {
                _statusStringIndex = -1;
            }
            else
            {
                var list = _parent.AllProjects.Where(p => p.ProjStatus == ProjectModel.ProjStatusStarted).ToList();
                if (list.Count > 0)
                {
                    _currProjStatus = ProjectModel.ProjStatusStarted;
                    _statusStringIndex = 1;
                }
                else
                {
                    _currProjStatus = ProjectModel.ProjStatusSetUped;
                    _statusStringIndex = 0;
                }
            }
           

            //默认项目和当前项目
            var model = ReadDefProjectFromXml();
            _projectSelected = model;
            _parent.CurrProject = model;
            _defaultProject = model;
            if (_defaultProject != null)
            {
                _lastStatus = _defaultProject.ProjStatus;
            }
        }

        /// <summary>
        /// 从配置中读取项目id
        /// </summary>
        private ProjectModel ReadDefProjectFromXml()
        {
            var config = UserConfig.LoadConfig(_parent.BimUser.Id);
            var index = config.DefProject;
            return _parent.AllProjects.FirstOrDefault(p => p.ProjId == index);
        }

        /// <summary>
        /// 项目id写入配置
        /// </summary>
        private void WriteDefProjectToXml(ProjectModel model)
        {
            var config = UserConfig.LoadConfig(_parent.BimUser.Id);
            config.UserId = _parent.BimUser.Id;
            config.DefProject = model.ProjId;
            config.Save();
        }

        /// <summary>
        /// 创建项目
        /// </summary>
        /// <param name="mfModel"></param>
        /// <param name="newMfProj"></param>
        /// <returns></returns>
        private ProjectModel CreateProject(ProjectCreateModel mfModel, out ProjectDto newMfProj)
        {
            newMfProj = null;
            HttpResponseMessage res;

            try
            {
                res = _projClient.CreateProject(mfModel, _parent.BimToken).Result;
                if (res.StatusCode != HttpStatusCode.Created)
                {
                    var resContent = res.Content.ReadAsStringAsync().Result;
                    IsShowAdorner = false;
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new Action(() => MetroMessageBox.Show(
                            ResponContentUtil.GetResponResult(resContent),
                            "新建项目",
                            MetroMessageBoxButton.OK,
                            MetroMessageBoxImage.Info,
                            MetroMessageBoxDefaultButton.OK)));
                    return null;
                }
            }
            catch (WebException ex)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(() => MetroMessageBox.Show(
                        String.Format("新建项目“{0}”失败: {1}！", mfModel.Name, ex.Message),
                        "新建项目",
                        MetroMessageBoxButton.OK,
                        MetroMessageBoxImage.Error,
                        MetroMessageBoxDefaultButton.OK)));
                return null;
            }
            catch (TaskCanceledException ex)
            {
                string err = null;
                var cts = new CancellationTokenSource();
                if (ex.CancellationToken == cts.Token)
                {
                    // a real cancellation, triggered by the caller
                    err = "用户取消操作！";
                }
                else
                {
                    // a web request timeout (possibly other things!?)
                    err = "操作超时！";
                }
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(() => MetroMessageBox.Show(
                        String.Format("新建项目“{0}”失败: {1}", mfModel.Name, err),
                        "新建项目",
                        MetroMessageBoxButton.OK,
                        MetroMessageBoxImage.Error,
                        MetroMessageBoxDefaultButton.OK)));
                return null;
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                if (ex.InnerException != null)
                {
                    ex0 = ex.InnerException;
                }
                if (ex0 is TaskCanceledException)
                {
                    var tcEx = ex0 as TaskCanceledException;
                    string err = null;
                    var cts = new CancellationTokenSource();
                    if (tcEx.CancellationToken == cts.Token)
                    {
                        // a real cancellation, triggered by the caller
                        err = "用户取消操作！";
                    }
                    else
                    {
                        // a web request timeout (possibly other things!?)
                        err = "操作超时！";
                    }
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new Action(() => MetroMessageBox.Show(
                            String.Format("新建项目“{0}”异常: {1}", mfModel.Name, err),
                            "新建项目",
                            MetroMessageBoxButton.OK,
                            MetroMessageBoxImage.Warning,
                            MetroMessageBoxDefaultButton.OK)));
                }
                else if (ex0 is WebException)
                {
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(() => MetroMessageBox.Show(
                        String.Format("新建项目“{0}”失败: {1}！", mfModel.Name, ex0.Message),
                        "新建项目",
                        MetroMessageBoxButton.OK,
                        MetroMessageBoxImage.Error,
                        MetroMessageBoxDefaultButton.OK)));
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new Action(() => MetroMessageBox.Show(
                            String.Format("新建项目“{0}”失败或超时，请检查您的网络连接是否正常！", mfModel.Name),
                            "新建项目",
                            MetroMessageBoxButton.OK,
                            MetroMessageBoxImage.Error,
                            MetroMessageBoxDefaultButton.OK)));
                }
                return null;
            }

            //服务端更新MFiles项目信息
            var paths = res.Headers.Location.OriginalString.TrimEnd('/')
                    .Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var projId = int.Parse(paths[paths.Length - 1]);
            res = _projClient.GetProject(projId, _parent.BimToken).Result;
            if (res.StatusCode != HttpStatusCode.OK) return null;
            var projContent = res.Content.ReadAsStringAsync().Result;
            newMfProj = JsonConvert.DeserializeObject<ProjectDto>(projContent);

            //项目信息
            var projModel = new ProjectModel
            {
                ProjId = newMfProj.Id,
                ProjName = newMfProj.Name,
                ProjNumber = newMfProj.Number,
                ProjDescription = newMfProj.Description,
                ProjCover = newMfProj.Cover,
                ProjTemplateId = newMfProj.TemplateId,
                ProjStatus = newMfProj.Status.Name,
                ProjStartTime = newMfProj.StartDateUtc.ToLocalTime(),
                ProjEndTime = newMfProj.EndDateUtc.ToLocalTime(),
            };

            return projModel;
        }

        /// <summary>
        /// 更新项目状态
        /// </summary>
        /// <param name="model"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private ProjectModel UpdateProjectStatus(ProjectModel model, string status)
        {
            var newProject = model;
            newProject.ProjStatus = status;
            return newProject;
        }

        /// <summary>
        /// 根据项目状态显示项目列表
        /// </summary>
        /// <param name="projStauts"></param>
        private string GetProjectsStatus(string projStauts)
        {
            if (projStauts.Contains(ProjectModel.ProjStatusArchived))
            {
                return ProjectModel.ProjStatusArchived;
            }
            if (projStauts.Contains(ProjectModel.ProjStatusOvered))
            {
                return ProjectModel.ProjStatusOvered;
            }
            if (projStauts.Contains(ProjectModel.ProjStatusPaused))
            {
                return ProjectModel.ProjStatusPaused;
            }
            if (projStauts.Contains(ProjectModel.ProjStatusSetUped))
            {
                return ProjectModel.ProjStatusSetUped;
            }
            if (projStauts.Contains(ProjectModel.ProjStatusStarted))
            {
                return ProjectModel.ProjStatusStarted;
            }

            return ProjectModel.ProjStatusPaused;
        }

        #endregion

        #region 线程函数
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitBkWorker(object sender, DoWorkEventArgs e)
        {
            IsShowAdorner = true;
            //初始化数据
            InitProjectData();
        }

        /// <summary>
        /// 数据加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitBkWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsShowAdorner = false;
        }

        /// <summary>
        /// 新建项目
        /// </summary>
        private void NewProjectTask(object obj)
        {
            IsShowAdorner = true;
            //服务端更新项目信息
            ProjectDto projDto = null;
            var model = obj as ProjectCreateModel;
            var newModel = CreateProject(model, out projDto);

            //更新UI
            if (newModel != null)
            {
                _parent.AllProjects.Insert(0, newModel);
                _parent.AppModel.Projects.Insert(0, projDto);

                //切换到立项项目列表
                if (StatusStringIndex == 0)
                {
                    OnPropertyChanged("CategoryProjects");
                }
                else
                {
                    StatusStringIndex = 0;
                }

                //显示背景图片或项目列表
                OnPropertyChanged("ShowBackgroundImg");
                OnPropertyChanged("ShowProjectList");
            }

            IsShowAdorner = false;
        }

        #endregion

        #region 实现接口
        public void Refresh()
        {
            OnPropertyChanged("CategoryProjects");
        }

        public ICommand GoBack
        {
            get { return null; }
        }

        public ICommand GoForward
        {
            get { return null; }
        }

        public string SourcePath { get; set; }

        public bool NavigatedFromBrowser { get; set; }


        public void NavigateTo(string uri)
        {
        }


        public ICommand SearchCommand
        {
            get { return null; }
        }

        public string SearchString { get; set; }
     
        #endregion
    }
}