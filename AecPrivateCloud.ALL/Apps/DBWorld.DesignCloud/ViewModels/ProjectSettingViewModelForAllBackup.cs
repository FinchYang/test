using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows;
using AecCloud.BaseCore;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using DBWorld.DesignCloud.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using SimulaDesign.WPFCustomUI.Controls;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.DesignCloud.ViewModels
{
    public class ProjectSettingViewModelForAllBackup : ViewModelBase, IWorkspace
    {
        #region 属性 字段
        /// <summary>
        /// 封面路径
        /// </summary>
        private const string CoverPath = "/DBWorld.DesignCloud;component/Images/Cover/";

        private const string TemplatePath = "/DBWorld.DesignCloud;component/Images/Template/default.jpg";

        /// <summary>
        /// 关联对象
        /// </summary>
        private DesignCloudViewModel _parent;

        /// <summary>
        /// 项目操作对象
        /// </summary>
        private ProjectClient _client;

        /// <summary>
        /// 所有参入方
        /// </summary>
        private List<UserGroupModel> _allParties;

        private List<CompanyDto> _companies;

        /// <summary>
        /// 选择的参入方
        /// </summary>
        private UserGroupModel _selectParty;

        /// <summary>
        /// 项目信息
        /// </summary>
        private ProjectModel _project;

        /// <summary>
        /// 封面图片路径
        /// </summary>
        private string _projCoverImage;

        /// <summary>
        /// 项目模板
        /// </summary>
        private List<TemplateModel> _projTemplates;

        /// <summary>
        /// 是否显示参与方
        /// </summary>
        private bool _isVisibilityParties = false;

        /// <summary>
        /// 确定命令
        /// </summary>
        public DelegateCommand<Window> OnOKCmd { get; set; }

        /// <summary>
        /// 更换封面命令
        /// </summary>
        public DelegateCommand<string> ChangeProjectCoverCmd { get; private set; }

        /// <summary>
        /// 选择本地封面命令
        /// </summary>
        public DelegateCommand SelectProjectCoverCmd { get; private set; }

        /// <summary>
        /// 选择项目模板命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> SelectionChangedCmd { get; private set; } 

        #endregion

        #region 构造函数
        public ProjectSettingViewModelForAllBackup(DesignCloudViewModel parent, 
            List<UserGroupModel> allParties, List<CompanyDto> companies, List<AreaDto> areas,List<ProjectLevelDto> levels,
            ProjectModel model)
        {
            //关联对象
            _parent = parent;
            _allParties = allParties;
            _companies = companies;
            _project = model;
            _areas = areas;
            _pClasses = ProjectClassList.Items;
            _levels = levels;

            if (_areas.Count > 0)
            {
                SelectedArea = _areas[0];
            }

            //默认参入方
            if (_allParties.Count > 0)
            {
                SelectedParty = _allParties[0];
            }
            if (_companies.Count > 0)
            {
                SelectedCompany = _companies[0];
            }

            if (_pClasses.Count > 0)
            {
                ProjectClass = _pClasses[0];
            }

            if (_levels.Count > 0)
            {
                SelectLevel = _levels[0];
            }

            //命令
            OnOKCmd = new DelegateCommand<Window>(new Action<Window>(OnOK), 
                new Func<Window, bool>(CanSave));
            ChangeProjectCoverCmd = new DelegateCommand<string>(
                new Action<string>(ChangeProjectCover));
            SelectProjectCoverCmd = new DelegateCommand(new Action(SelectProjectCover));
            SelectionChangedCmd = new DelegateCommand<ExCommandParameter>(
                new Action<ExCommandParameter>(SelectionChanged));
        }
        #endregion

        #region 工作区信息
        /// <summary>
        /// 插件名称
        /// </summary>
        public string DisplayName { get { return "新建项目"; } }

        /// <summary>
        /// 插件图标
        /// </summary>
        public string IconPath
        {
            get { return ""; }
        }
        #endregion

        #region 项目操作对象
        private ProjectClient GetClient()
        {
            return _client ?? (_client = _parent.Context.GetProjectClient());
        }
        #endregion

        #region 项目信息
        /// <summary>
        /// 项目模板列表
        /// </summary>
        public List<TemplateModel> ProjTemplates
        {
            get 
            {
                if (_projTemplates == null)
                {
                    _projTemplates = new List<TemplateModel>(GetAllProjectTemplates());
                }
                if (_projTemplates.Count == 1)
                {
                    _project.ProjTemplateId = _projTemplates.First().TemplateId;
                }
                return _projTemplates;
            }
        }

        /// <summary>
        /// 封面图片路径
        /// </summary>
        public string ProjCoverImage
        {
            get
            {
                if (_projCoverImage == null)
                {
                    _projCoverImage = CoverPath + "default.png";
                }
                return _projCoverImage;
            }
            set
            {
                _projCoverImage = value;
                OnPropertyChanged("ProjCoverImage");
            }
        }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName
        {
            get { return _project.ProjName; }
            set
            {
                _project.ProjName = value;
                OnPropertyChanged("ProjName");
            }
        }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string ProjDescription
        {
            get { return _project.ProjDescription; }
            set
            {
                _project.ProjDescription = value;
                OnPropertyChanged("ProjDescription");
            }
        }

        public string ProjNumber
        {
            get { return _project.ProjNumber; }
            set
            {
                _project.ProjNumber = value;
                OnPropertyChanged("ProjNumber");
            }
        }

        public string OwnerUnit
        {
            get { return _project.OwnerUnit; }
            set
            {
                _project.OwnerUnit = value;
                OnPropertyChanged("OwnerUnit");
            }
        }

        public string SupervisionUnit
        {
            get { return _project.SupervisionUnit; }
            set
            {
                _project.SupervisionUnit = value;
                OnPropertyChanged("SupervisionUnit");
            }
        }

        public string DesignUnit
        {
            get { return _project.DesignUnit; }
            set
            {
                _project.DesignUnit = value;
                OnPropertyChanged("DesignUnit");
            }
        }

        public string ConstructScale
        {
            get { return _project.ConstructScale; }
            set
            {
                _project.ConstructScale = value;
                OnPropertyChanged("ConstructScale");
            }
        }

        public string ContractAmount
        {
            get { return _project.ContractAmount; }
            set
            {
                _project.ContractAmount = value;
                OnPropertyChanged("TotalCost");
            }
        }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime ProjStartTime
        {
            get{ return _project.ProjStartTime; }
            set
            {
                _project.ProjStartTime = value;
                OnPropertyChanged("ProjStartTime");
            }
        }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime ProjEndTime
        {
            get { return _project.ProjEndTime; }
            set
            {
                _project.ProjEndTime = value;
                OnPropertyChanged("ProjEndTime");
            }
        }

        /// <summary>
        /// 所有参与方
        /// </summary>
        public List<UserGroupModel> AllParties
        {
            get { return _allParties; }
        }

        public List<CompanyDto> Companies
        {
            get { return _companies; }
        }

        private CompanyDto _selectCompany;

        public CompanyDto SelectedCompany
        {
            get
            {
                if (_selectCompany == null)
                {
                    _selectCompany = new CompanyDto();
                }
                return _selectCompany;
            }
            set
            {
                _selectCompany = value;
                _project.CompanyId = _selectCompany.Id;
                OnPropertyChanged("SelectedCompany");
            }
        }

        private List<ProjectLevelDto> _levels;

        public List<ProjectLevelDto> Levels
        {
            get { return _levels; }
        }

        private ProjectLevelDto _sLevel;

        public ProjectLevelDto SelectLevel
        {
            get
            {
                if (_sLevel == null)
                {
                    _sLevel = new ProjectLevelDto();
                }
                return _sLevel;
            }
            set
            {
                _sLevel = value;
                _project.LevelId = _sLevel.Id;
                OnPropertyChanged("SelectLevel");
            }
        }

        private List<AreaDto> _areas;

        public List<AreaDto> Areas
        {
            get { return _areas; }
        }

        private AreaDto _selectArea;

        public AreaDto SelectedArea
        {
            get
            {
                if (_selectArea == null)
                {
                    _selectArea = new AreaDto();
                }
                return _selectArea;
            }
            set
            {
                _selectArea = value;
                _project.AreaId = _selectArea.Id;
                OnPropertyChanged("SelectedArea");
            }
        }

        private string _pClass;

        public string ProjectClass
        {
            get
            {
                return _pClass;
            }
            set
            {
                _pClass = value;
                _project.ProjectClass = value;
                OnPropertyChanged("ProjectClass");
            }
        }

        private List<string> _pClasses;

        public List<string> ProjectClasses
        {
            get { return _pClasses; }
        }

        /// <summary>
        /// 选择的参入方
        /// </summary>
        public UserGroupModel SelectedParty
        {
            get
            {
                if (_selectParty == null)
                {
                    _selectParty = new UserGroupModel();
                }
                return _selectParty;
            }
            set
            {
                _selectParty = value;
                _project.PartyId = _selectParty.Id;
                OnPropertyChanged("SelectedParty");
            }
        }

        /// <summary>
        /// 是否显示参与方
        /// </summary>
        public bool IsVisibilityParties
        {
            get { return _isVisibilityParties; }
            set
            {
                _isVisibilityParties = value;
                OnPropertyChanged("IsVisibilityParties");
            }
        }

        #endregion

        #region  命令函数

        /// <summary>
        /// OK
        /// </summary>
        /// <param name="win"></param>
        private void OnOK(Window win)
        {
            //封面
            if (_project.ProjCover == null)
            {
                var path = CoverPath + "default.png";
                _project.ProjCover = GetResourceBytes(path);
            }

            //关闭窗口
            win.DialogResult = true;
            win.Close();
        }

        /// <summary>
        /// 更换封面
        /// </summary>
        private void ChangeProjectCover(string path)
        {
            ProjCoverImage = path;
            _project.ProjCover = GetResourceBytes(path);
        }

        /// <summary>
        /// 选择本地封面
        /// </summary>
        private void SelectProjectCover()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择文件",
                Filter = "jpg文件(*.jpg)|*.jpg|jpeg文件(*.jpeg)|*.jpg|png文件(*.png)|*.png|bmp文件(*.bmp)|*.bmp",
                FileName = string.Empty,
                FilterIndex = 1,
                RestoreDirectory = true,
                DefaultExt = "jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var file = new FileInfo(openFileDialog.FileName);
                if (file.Length > Utility.MaxImageLength)
                {
                    MetroMessageBox.Show(String.Format("图片大小不能超过{0}K！", Utility.MaxImageLength/1000),
                        "选择文件",
                        MetroMessageBoxButton.OK,
                        MetroMessageBoxImage.Warning,
                        MetroMessageBoxDefaultButton.OK);
                    file = null;
                    return;
                }

                ProjCoverImage = openFileDialog.FileName;
                _project.ProjCover = File.ReadAllBytes(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 选择项目模板
        /// </summary>
        /// <param name="param"></param>
        private void SelectionChanged(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            var model = param.Parameter as TemplateModel;
            //if (model.HasParty)
            //{
            //    //显示参与方
            //    IsVisibilityParties = true;
            //}
            //else
            //{
            //    //不显示参与方
            //    IsVisibilityParties = false;
            //    _project.PartyId = 0;
            //}
            _project.ProjTemplateId = model.TemplateId;
        }

        #endregion

        #region 操作函数
        /// <summary>
        /// 是否能保存
        /// </summary>
        /// <returns></returns>
        private bool CanSave(Window win)
        {
           

            //名称
            if (string.IsNullOrEmpty(_project.ProjName))
            {
                return false;
            }

            if (String.IsNullOrEmpty(_project.ContractAmount))
            {
                return false;
            }
            if (String.IsNullOrEmpty(_project.DesignUnit))
            {
                return false;
            }

            if (String.IsNullOrEmpty(_project.OwnerUnit))
            {
                return false;
            }

            if (String.IsNullOrEmpty(_project.ProjNumber))
            {
                return false;   
            }

            if (String.IsNullOrEmpty(_project.SupervisionUnit))
            {
                return false;
            }

            //时间
            if (_project.ProjEndTime < _project.ProjStartTime)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取项目模板
        /// </summary>
        /// <returns></returns>
        private IEnumerable<TemplateModel> GetAllProjectTemplates()
        {
            var models = new List<TemplateModel>();
            var client = GetClient();
            var res = client.GetProjectTemplates(_parent.BimToken);
            if (res.Result.StatusCode == HttpStatusCode.OK)
            {
                var json = res.Result.Content.ReadAsStringAsync().Result;
                var templates = JsonConvert.DeserializeObject<List<VaultTemplateDto>>(json);
                foreach (var template in templates)
                {
                    var model = new TemplateModel
                    {
                        TemplateId = template.Id,
                        TemplateName = template.Name,
                        TemplateDescription = template.Description,
                        HasParty = template.HasParty,
                        ImageUrl = template.ImageUrl ?? TemplatePath
                    };
                    models.Add(model);
                }
            }

            return models;
        }

        /// <summary>
        /// 转换为二进制
        /// </summary>
        /// <param name="resourceFile"></param>
        private  byte[] GetResourceBytes(string resourceFile)
        {
            var uri = new Uri(resourceFile, UriKind.RelativeOrAbsolute);

            var info = Application.GetResourceStream(uri);
            if (info == null || info.Stream == null)
                throw new ApplicationException("Missing file: " + resourceFile);

            var bytes = new byte[info.Stream.Length]; 
            info.Stream.Read(bytes, 0, bytes.Length);
            info.Stream.Seek(0, SeekOrigin.Begin);

            return bytes;
        }

        #endregion


        #region 实现接口

        public string Id { get; private set; }
        public void Refresh()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

