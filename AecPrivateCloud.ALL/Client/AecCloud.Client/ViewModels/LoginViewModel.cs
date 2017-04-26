using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AecCloud.Client.Models;
using AecCloud.Client.Util;
using AecCloud.MfilesClientCore;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using DBWorld.Config.Helper;
using DBWorld.Config.Models;
using log4net;
using Newtonsoft.Json;
using System.Net;
using SimulaDesign.WPFPluginCore.Commands;


namespace AecCloud.Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region  属性字段
     
        private static AuthenticationClient _authClient;
        private static CloudClient _appClient;
        private static TokenClient _tokenClient;
        private static ProjectClient _projClient;
        internal static readonly ApiClientContext Context = ApiClientContext.Create(
            System.Configuration.ConfigurationManager.AppSettings["api"], "dbworldclient");

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// //关闭窗口
        /// </summary>
        private readonly Action _closeAction;

        /// <summary>
        /// 密钥
        /// </summary>
        private string _key;
       
        /// <summary>
        /// 用户信息
        /// </summary>
        private readonly UserModel _userInfo = new UserModel();

        /// <summary>
        /// 用户配置信息
        /// </summary>
        private UserConfigModel _config;

        /// <summary>
        /// 取消登录
        /// </summary>
        private CancellationTokenSource _cts;

        private bool _domainUser=false;
        public bool DomainUser
        {
            get { return _domainUser; }
            set
            {
                if (_domainUser != value)
                {
                    _domainUser = value;
                    OnPropertyChanged("DomainUser");
                }
            }
        }

        /// <summary>
        /// 是否登录成功
        /// </summary>
        public static bool LoggedIn;

        /// <summary>
        /// 登录命令
        /// </summary>
        public DelegateCommand LoginCmd { get; private set; }

        /// <summary>
        /// 取消登录
        /// </summary>
        public DelegateCommand CancelCmd { get; private set; }

        /// <summary>
        /// 隐藏登录结果信息命令
        /// </summary>
        public DelegateCommand HideLoginResultCmd { get; set; }


        public string ProductInfo
        {
            get { return AssemblyInfoHelper.ProductInfo; }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _userPwd;

        public string UserPwd
        {
            get { return _userPwd; }
            set
            {
                _userPwd = value;
                OnPropertyChanged("UserPwd");
            }
        }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        private bool _isRememberPwd;

        public bool IsRememberPwd
        {
            get { return _isRememberPwd; }
            set
            {
                _isRememberPwd = value;
                OnPropertyChanged("IsRememberPwd");
            }
        }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        private bool _isAutoLogin;

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
        /// 用户名列表
        /// </summary>
        private List<string> _userNameList;

        public List<string> UserNameList
        {
            get
            {
                if (_userNameList == null)
                {
                    _userNameList = new List<string>();
                }
                return _userNameList;
            }
            set
            {
                _userNameList = value;
                OnPropertyChanged("UserNameList");
            }
        }

        /// <summary>
        /// 正在登录状态
        /// </summary>
        private bool _isLogging;

        public bool IsLogging
        {
            get { return _isLogging; }
            set
            {
                _isLogging = value;
                OnPropertyChanged("IsLogging");
                OnPropertyChanged("IsNotLogging");
            }
        }

        /// <summary>
        /// 非登录状态
        /// </summary>
        public bool IsNotLogging
        {
            get { return !_isLogging; }
        }

        /// <summary>
        /// 是否显示登录结果
        /// </summary>
        private bool _isShowDescription;
        public bool IsShowDescription
        {
            get { return _isShowDescription; }
            set
            {
                _isShowDescription = value;
                OnPropertyChanged("IsShowDescription");
            }
        }

        /// <summary>
        /// 登录结果描述
        /// </summary>
        private string _loginDescription;
        public string LoginDescription
        {
            get { return _loginDescription; }
            set
            {
                _loginDescription = value;
                OnPropertyChanged("LoginDescription");
            }
        }
        #endregion

        /// <summary>
        /// 获取app对象
        /// </summary>
        /// <returns></returns>
        internal static CloudClient GetAppClient()
        {
            return _appClient ?? (_appClient = Context.GetCloudClient());
        }

        /// <summary>
        /// 获取验证对象
        /// </summary>
        /// <returns></returns>
        internal static AuthenticationClient GetAuthClient()
        {
            return _authClient ?? (_authClient = Context.GetAuthClient());
        }

        /// <summary>
        /// 获取token对象
        /// </summary>
        /// <returns></returns>
        internal static TokenClient GetTokenClient()
        {
            return _tokenClient ?? (_tokenClient = Context.GetTokenClient());
        }

        /// <summary>
        /// 获取项目对象
        /// </summary>
        /// <returns></returns>
        internal static ProjectClient GetProjectClient()
        {
            return _projClient ?? (_projClient = Context.GetProjectClient());
        }

        public LoginViewModel(Action closeAction)
        {
            //关闭窗口
            _closeAction = closeAction;

            InitUserConfig();
            InitCommand();

            //自动登录
            if (IsAutoLogin)
            {
                LoginCmd.Execute();
            }
        }

        private void InitCommand()
        {
            //登录命令
            LoginCmd = new DelegateCommand(InvokeLogin);
            //取消登录命令
            CancelCmd = new DelegateCommand(InvokeCancel);
            //隐藏登录结果信息
            HideLoginResultCmd = new DelegateCommand(HideLoginResult);
        }

        /// <summary>
        /// 判断是否安装了MFiels
        /// </summary>
        /// <returns></returns>
        private bool IsInstalledMf()
        {
            try
            {
                MFilesCore.ClientUtils.GetClientApp();
            }
            catch (COMException ex)
            {
                //未安装MFiles
                Log.Error("Uninstalled component. Exception: " + ex.Message, ex);
                return false;
            }

            return true;
        }


        #region 加载配置
        /// <summary>
        /// 加载所有用户配置
        /// </summary>
        /// <returns></returns>
        private void InitUserConfig()
        {
            UserConfigHelper.AppFolderName = AssemblyInfoHelper.Product + "\\";
            var configHepler = UserConfigHelper.GetInstence();
            _config = configHepler.LoadLastUserConfig();

            if (_config != null)
            {
                UserName = _config.UserName;
                IsRememberPwd = Convert.ToBoolean(_config.RememberPwd);
                if (IsRememberPwd)
                {
                    //生成密钥
                    _key = AecDesCrypto.GenerateKey();
                    //解密
                    UserPwd = AecDesCrypto.Decrypt(_config.UserPwd, _key);
                }
                IsAutoLogin = Convert.ToBoolean(_config.AutoLogin);
            }
        }
        #endregion

        #region 用户登录
        /// <summary>
        /// 用户登录
        /// </summary>
        private async void InvokeLogin()
        {
            //本地认证
            if (!VerifyUserInfoFromLocal())
            {
                IsShowDescription = true;
                return;
            }

            IsShowDescription = false;
            IsLogging = true;

            //开始登陆
            var result = false;
            _cts = new CancellationTokenSource();

            try
            {
                result = await ImplLogin(_cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                Log.Error("登录失败 1：" + ex.Message, ex);
                //LoginDescription = ex.Message;//取消登录
            }
            catch (HttpRequestException ex)
            {
                Log.Error("登录失败 2：" + ex.Message, ex);
                LoginDescription = "登录失败，请检查您的网络或本机防火墙设置！";
            }
            catch (Exception ex)
            {
                Log.Error("登录失败 1111：" + ex.Message, ex);
            }
            finally
            {
                _cts.Dispose();
            }

            //完成登录
            IsLogging = false;

            if (result)
            {
                LoggedIn = true;
                //显示主窗口
                ShowMainWindow();
            }
            else
            {
                if (!_cts.IsCancellationRequested)
                {
                    IsShowDescription = true;
                }
            }
        }

        /// <summary>
        /// 取消登录
        /// </summary>
        private void InvokeCancel()
        {
            _cts.Cancel();
            IsLogging = false;
        }

        /// <summary>
        /// 隐藏登录结果信息
        /// </summary>
        private void HideLoginResult()
        {
            IsShowDescription = false;
        }

        /// <summary>
        /// 执行登录
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<bool> ImplLogin(CancellationToken ct)
        {
            return await Task.Run(async () =>
            {
                //服务端认证
                ct.ThrowIfCancellationRequested();
                var token = await GetUserToken();
                
                if (!token.Success)
                {
                    LoginDescription = "登录失败 3："+token.GetErrorMessage();
                    return false;
                }

                //获取用户信息
                ct.ThrowIfCancellationRequested();
                var tasks = new[] {GetUserProfileJson(token), GetUserAppsJson(token)};
                var results = await Task.WhenAll(tasks);
                var user = JsonConvert.DeserializeObject<UserDto>(results[0]);
                user.Password = UserPwd;
                var app = JsonConvert.DeserializeObject<UserCloudModel>(results[1]);

                ct.ThrowIfCancellationRequested();
                try
                {
                    if (user.Domain == string.Empty)
                    {
                        user.Domain = user.UserName.Substring(0, UserName.IndexOf('\\'));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("implogin deal domain:{0},user={1},domain={2}",ex.Message,user.UserName,user.Domain));
                }
                UpdateUserInfo(token, user, app);
                SaveUserInfoToConfig();
                return true;
            }, ct);
        }

        /// <summary>
        /// 用户名和密码校验
        /// </summary>
        private bool VerifyUserInfoFromLocal()
        {
            if (string.IsNullOrEmpty(UserName))
            {
                LoginDescription = "请输入用户名后再登录！";
                return false;
            }

            if (string.IsNullOrEmpty(UserPwd))
            {
                LoginDescription = "请输入密码后再登录！";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从服务器获取用户Token
        /// </summary>
        /// <returns></returns>
        private async Task<TokenModel> GetUserToken()
        {
            var tokenClient = GetTokenClient();
            return await tokenClient.GetToken(UserName, UserPwd, DomainUser);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string> GetUserProfileJson(TokenModel token)
        {
            var loginclient = GetAuthClient();
            var res = loginclient.GetUserProfile(token).Result;
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 获取app信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string> GetUserAppsJson(TokenModel token)
        {
            var appclient = GetAppClient();
            //var clientSettings = new ClientSettings { CoAvailable = true };
            var res = await appclient.GetClouds(token);
            if (res.StatusCode != HttpStatusCode.OK) return null;
            return await res.Content.ReadAsStringAsync();
        }
        

        //private async 

        /// <summary>
        /// 设置用户数据
        /// </summary>
        /// <param name="token"></param>
        /// <param name="user"></param>
        /// <param name="appModel"></param>
        private void UpdateUserInfo(TokenModel token, UserDto user, UserCloudModel appModel)
        {
            _userInfo.Token = token;

            //客户端UI中用户基本信息
            _userInfo.UserId = user.Id;
            _userInfo.Password = user.Password;
            _userInfo.UserName = user.UserName;
            _userInfo.Email = user.Email;

            //从服务端返回的用户信息
            _userInfo.UserWeb = user;

            //用户app信息，测试数据
            _userInfo.UserApp = appModel;

            App.UserInfo = _userInfo;
        }

        /// <summary>
        /// /保存登录成功用户配置
        /// </summary>
        private void SaveUserInfoToConfig()
        {
            _config.UserId = _userInfo.UserId;
            _config.UserName = _userInfo.UserName;
            _config.UserPwd = AecDesCrypto.Encrypt(UserPwd, _key); //加密
            _config.RememberPwd = Convert.ToInt32(IsRememberPwd);
            _config.AutoLogin = Convert.ToInt32(_isAutoLogin);
            _config.LastLoginTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _config.OnLine = 1; //在线

            var configHepler = UserConfigHelper.GetInstence();
            configHepler.SaveConfig(_config);
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        private void ShowMainWindow()
        {
            var winMain = new MainWindow();
            Application.Current.MainWindow = winMain;
            winMain.DataContext = new MainWindowViewModel(
                _userInfo.UserWeb, 
                _userInfo.UserApp, 
                _userInfo.Token);
            _closeAction.Invoke();
            winMain.Show();
        }
        #endregion

    }
}
