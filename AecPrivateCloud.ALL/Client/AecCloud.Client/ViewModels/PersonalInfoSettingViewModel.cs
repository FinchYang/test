using System;
using System.IO;
using System.Windows;
using AecCloud.Client.Command;
using AecCloud.Client.Models;
using AecCloud.Client.Util;
using Microsoft.Win32;
using SimulaDesign.WPFCustomUI.Controls;

namespace AecCloud.Client.ViewModels
{
    public class PersonalInfoSettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 目录名称
        /// </summary>
        private const string AppFolder = "DbWorld\\";

        /// <summary>
        /// 配置文件名
        /// </summary>
        private const string ConfigName = "loginconfig.xml";

        ///// <summary>
        ///// 配置文件路径
        ///// </summary>
        //private string _configPath;

        /// <summary>
        /// 是否可编辑
        /// </summary>
        private readonly bool _isEdit = false;

        /// <summary>
        /// 是否自动登录
        /// </summary>
        private bool _isAutoLogin;

        /// <summary>
        /// 用户信息
        /// </summary>
        private readonly UserModel _userModel;

        /// <summary>
        /// 用户配置信息
        /// </summary>
        private LoginConfigModel _configModel;

        /// <summary>
        /// 确定命令
        /// </summary>
        public DelegateCommand<Window> OnOKCmd { get; private set; }

        /// <summary>
        /// 更新用户头像命令
        /// </summary>
        public DelegateCommand UpdateImageCmd { get; private set; }

        #region 构造函数
        public PersonalInfoSettingViewModel(bool isEdit, UserModel model)
        {
            _isEdit = isEdit;
            _userModel = model;
            _isAutoLogin = GetAutoLogin();

            //
            OnOKCmd = new DelegateCommand<Window>(new Action<Window>(OnOk));
            UpdateImageCmd = new DelegateCommand(new Action(UpdateImage));
        }
        #endregion

        #region 工作区信息
        /// <summary>
        /// 插件名称
        /// </summary>
        public string DisplayName { get { return "个人信息"; } }

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
        /// 用户名
        /// </summary>
        public string FullName
        {
            get { return _userModel.FullName; }
            set
            {
                _userModel.FullName = value;
                OnPropertyChanged("FullName");
            }
        }

        /// <summary>
        /// 用户头像
        /// </summary>
        public byte[] PersonalImage
        {
            get { return _userModel.Image; }
            set
            {
                _userModel.Image = value;
                OnPropertyChanged("PersonalImage");
            }
        }

        /// <summary>
        /// 用户email
        /// </summary>
        public string Email
        {
            get { return _userModel.Email; }
            set
            {
                _userModel.Email = value;
                OnPropertyChanged("Email");
            }
        }

        /// <summary>
        /// 手机号
        /// </summary>
        public string MobilePhoneNum
        {
            get { return _userModel.MobilePhoneNum; }
            set
            {
                _userModel.MobilePhoneNum = value;
                OnPropertyChanged("MobilePhoneNum");
            }
        }

        /// <summary>
        /// 电话号
        /// </summary>
        public string TelephoneNum
        {
            get { return _userModel.TelephoneNum; }
            set
            {
                _userModel.TelephoneNum = value;
                OnPropertyChanged("TelephoneNum");
            }
        }

        /// <summary>
        /// QQ号
        /// </summary>
        public string QQNum
        {
            get { return _userModel.QQNum; }
            set
            {
                _userModel.QQNum = value;
                OnPropertyChanged("QQNum");
            }
        }

        /// <summary>
        /// 公司
        /// </summary>
        public string Company
        {
            get { return _userModel.Company; }
            set
            {
                _userModel.Company = value;
                OnPropertyChanged("Company");
            }
        }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department
        {
            get { return _userModel.Department; }
            set
            {
                _userModel.Department = value;
                OnPropertyChanged("Department");
            }
        }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position
        {
            get { return _userModel.Position; }
            set
            {
                _userModel.Position = value;
                OnPropertyChanged("Position");
            }
        }

        /// <summary>
        /// 行业
        /// </summary>
        public string Identity
        {
            get { return _userModel.Identity; }
            set
            {
                _userModel.Identity = value;
                OnPropertyChanged("Identity");
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get { return _userModel.Remark; }
            set
            {
                _userModel.Remark = value;
                OnPropertyChanged("Remark");
            }
        }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool IsAutoLogin
        {
            get { return _isAutoLogin; }
            set
            {
                _isAutoLogin = value;
                OnPropertyChanged("IsAutoLogin");
            }
        }

        /// <summary>
        /// 界面元素是否可用
        /// </summary>
        public bool IsOkEnable
        {
            get { return _isEdit; }
        }
        #endregion

        #region 命令函数
        /// <summary>
        /// 确定命令
        /// </summary>
        /// <param name="win"></param>
        private void OnOk(Window win)
        {
            SetAutoLogin(_isAutoLogin);
            win.DialogResult = true;
            win.Close();
        }

        /// <summary>
        /// 更新头像
        /// </summary>
        private void UpdateImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择文件",
                Filter = "jpg文件(*.jpg)|*.jpg|jpeg文件(*.jpeg)|*.jpg|png文件(*.png)|*.png|bmp文件(*.bmp)|*.bmp",
                FileName = string.Empty,
                FilterIndex = 1,
                RestoreDirectory = true,
                DefaultExt = "png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var file = new FileInfo(openFileDialog.FileName);
                if (file.Length > 200 * 1024)
                {
                    MetroMessageBox.Show("图片大小不能超过200K！",
                       "选择文件",
                       MetroMessageBoxButton.OK,
                       MetroMessageBoxImage.Warning,
                       MetroMessageBoxDefaultButton.OK);
                    file = null;
                    return;
                }
                PersonalImage = File.ReadAllBytes(openFileDialog.FileName);
            }
        }

        #endregion

        #region 操作函数
        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns></returns>
        private string GetConfigPath()
        {
            var mydocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folder = Path.Combine(mydocuments, AppFolder, _userModel.UserId.ToString());
            return Path.Combine(folder, ConfigName);
        }
        /// <summary>
        /// 从配置中读取是否登录
        /// </summary>
        /// <returns></returns>
        private bool GetAutoLogin()
        {
            _configModel = XmlSerializerUtil.LoadFromXml(GetConfigPath(), typeof(LoginConfigModel))
                    as LoginConfigModel;
            if (_configModel == null) return false;

            return Convert.ToBoolean(_configModel.AutoLogin);
        }

        /// <summary>
        /// 设置是否登录到配置中
        /// </summary>
        /// <param name="flag"></param>
        private void SetAutoLogin(bool flag)
        {
            _configModel.AutoLogin = Convert.ToInt32(flag);

            XmlSerializerUtil.SaveToXml(
                GetConfigPath(),
                _configModel,
                typeof(LoginConfigModel));
        }
        #endregion
    }
}
