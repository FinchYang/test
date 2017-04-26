using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using SimulaDesign.WPFPluginCore.Commands;
using SimulaDesign.WPFPluginCore.Workspaces;

namespace DBWorld.EnterpriseCloud.ViewModels
{
    public class MfilesVaultListViewModel : ViewModelBase, INavigableWorkspace
    {
        /// <summary>
        /// 关联对象
        /// </summary>
        private EnterpriseCloudViewModel _parent;

        /// <summary>
        /// 是否显示加载动画
        /// </summary>
        private bool _isShowAdorner;

        /// <summary>
        /// 选择的vault
        /// </summary>
        private MfilesVaultViewModel _selectedVm;

        /// <summary>
        /// mfile库列表
        /// </summary>
        private NotifyTaskCompletion<List<MfilesVaultViewModel>> _vaultList;

        /// <summary>
        /// 选项改变命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> VaultSelectionChangedCmd { get; set; }

        /// <summary>
        /// 双击选项命令
        /// </summary>
        public DelegateCommand<ExCommandParameter> VaultMouseDoubleClickCmd { get; set; } 

        public MfilesVaultListViewModel(EnterpriseCloudViewModel parent)
        {
            _parent = parent;

            DisplayName = "总揽";

            VaultSelectionChangedCmd = new DelegateCommand<ExCommandParameter>(VaultSelectionChanged);
            VaultMouseDoubleClickCmd = new DelegateCommand<ExCommandParameter>(VaultMouseDoubleClick);
        }

        /// <summary>
        /// 获取vault列表
        /// </summary>
        /// <returns></returns>
        private async Task<List<MfilesVaultViewModel>> GetVaultList()
        {
            IsShowAdorner = true;
            var vmList = new List<MfilesVaultViewModel>();
            var vaultList = ClientUtils.GetVaultConnections(_parent.IgnoreVaults.AsEnumerable()); //_parent.WebUri, 
            foreach (var connection in vaultList)
            {
                var vm = new MfilesVaultViewModel(_parent, connection);
                vmList.Add(vm);
            }
            IsShowAdorner = false;

            return await Task.FromResult(vmList);
        }

        /// <summary>
        /// vault列表
        /// </summary>
        public NotifyTaskCompletion<List<MfilesVaultViewModel>> VaultList
        {
            get
            {
                if (_vaultList == null)
                {
                    _vaultList = new NotifyTaskCompletion<List<MfilesVaultViewModel>>(
                        Task.Run(()=>GetVaultList()));
                }

                return _vaultList;
            }
        }

        /// <summary>
        /// 是否显示加载动画
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
        /// Vault选项改变命令函数
        /// </summary>
        /// <param name="param"></param>
        private void VaultSelectionChanged(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            _selectedVm = param.Parameter as MfilesVaultViewModel;
        }

        /// <summary>
        /// 双击vault命令函数
        /// </summary>
        /// <param name="param"></param>
        private void VaultMouseDoubleClick(ExCommandParameter param)
        {
            if (param.Parameter == null) return;
            _selectedVm = param.Parameter as MfilesVaultViewModel;
            _parent.CurrWorkspace = _selectedVm;
        }

        public string Id { get; private set; }
        public string DisplayName { get; private set; }

        public string IconPath
        {
            get { return String.Empty; }
        }


        public void Refresh()
        {
            return;
        }

        public System.Windows.Input.ICommand GoBack
        {
            get { return null; }
        }

        public System.Windows.Input.ICommand GoForward
        {
            get { return null; }
        }

        public string SourcePath { get; set; }

        public bool NavigatedFromBrowser { get; set; }


        public void NavigateTo(string uri)
        {
        }


        public System.Windows.Input.ICommand SearchCommand
        {
            get { return null; }
        }

        public string SearchString { get; set; }
    }
}
