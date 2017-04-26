using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using MfNotification.Core.NotifyObject;
using Microsoft.AspNet.SignalR.Client;
using System.Net.Http;
using System.Windows.Forms;

using System.Threading;
using System.Web.Script.Serialization;
using System.Diagnostics;
using AecCloud.MFilesCore;
using AecCloud.PluginInstallation.VaultApps;
using AecCloud.WebAPI.Models;
using AecCloud.WebAPI.Models.DataAnnotations;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using MFilesAPI;
using System.Reflection;
using Newtonsoft.Json;
using Notice;
using File = System.IO.File;
using MfTask = MfNotification.Core.NotifyObject.MfTask;
using NotificationTypeEnum = MfNotification.Core.NotifyObject.NotificationTypeEnum;

namespace Notification.Client
{
    public partial class Form1 : Form
    {
        private delegate void UpdateStatusDelegate(string status);
        const int Cols = 9;
        const string Localsettings = "local.settings";
        private readonly List<Thread> _threads = new List<Thread>();
        private Thread _tcheck;
        private Thread _tCheckNewProject;
        private Thread _tCheckClientApps;
        private Thread _tCheckContractor;
        private Thread _tCheckSelfUpdate;
        private Thread _tCheckSignalr;
        private MfTask _curmft=new MfTask();
        private Threadmessage _curtm;
        private readonly List<Threadmessage> _ltm = new List<Threadmessage>();
        private readonly Mutex _updatedataview = new Mutex();
        private readonly static Mutex _mfilesconnectMutex = new Mutex();

        public List<MfTask> Listmftasksfordata = new List<MfTask>();
        private VaultConnections _vcs;
        private NoticeSet _formset;
        private SeismicControlSettings _scsconf = new SeismicControlSettings();
        private LocalSettings _localSettings = new LocalSettings();
        private Icon _mIcon1;
        private Icon _mIcon2;
        private bool _mBFlag;
        private bool IsSignalrConnected = false;
        private string version;
        private string ServerURI;
        private IHubProxy HubProxy { get; set; }
        private HubConnection Connection { get; set; }
        public class Stock
        {
            private decimal _price;

            public string Symbol { get; set; }

            public decimal DayOpen { get; private set; }

            public decimal DayLow { get; private set; }

            public decimal DayHigh { get; private set; }

            public decimal LastChange { get; private set; }

            public decimal Change
            {
                get
                {
                    return Price - DayOpen;
                }
            }

            public double PercentChange
            {
                get
                {
                    return (double)Math.Round(Change / Price, 4);
                }
            }

            public decimal Price
            {
                get
                {
                    return _price;
                }
                set
                {
                    if (_price == value)
                    {
                        return;
                    }

                    LastChange = value - _price;
                    _price = value;

                    if (DayOpen == 0)
                    {
                        DayOpen = _price;
                    }
                    if (_price < DayLow || DayLow == 0)
                    {
                        DayLow = _price;
                    }
                    if (_price > DayHigh)
                    {
                        DayHigh = _price;
                    }
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
            _mBFlag = true;
            timer1.Interval = 500;
        }


        private object NewProjectProcessing(MfilesClientConfig mfilesClientConfig)
        {
            BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("NewProjectProcessing.{0},{1},{2}", mfilesClientConfig.Name, mfilesClientConfig.Guid, mfilesClientConfig.Host) });
            lock (_mfilesconnectMutex)
            {
                var mcr = MfilesClientResource.GetMfilesClientResource();
                if (mcr.IsError())
                {
                    return string.Empty;
                }
                var vcs = mcr.VaultConnections;
                var app = mcr.MFilesClientApplication;
                var notfound = true;
                foreach (VaultConnection vaultConnection in vcs)
                {
                    if (vaultConnection.ServerVaultGUID == mfilesClientConfig.Guid)
                    {
                        notfound = false;
                        break;
                    }
                }
                if (!notfound)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                        new object[]
                        {
                            string.Format("NewProjectProcessing,已有相同guid连接{0},{1},{2}", mfilesClientConfig.Name,
                                mfilesClientConfig.Guid, mfilesClientConfig.Host)
                        });

                    return string.Empty;
                }
                //     var app = new MFilesClientApplication();
                VaultConnection vc = new VaultConnection();
                try
                {
                    vc = new VaultConnection
                    {
                        AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser,
                        AutoLogin = false,
                        Name = mfilesClientConfig.Name,
                        NetworkAddress = mfilesClientConfig.Host,
                        UserName = _formset.textBoxUsername.Text,
                        Endpoint = "2266",
                        ProtocolSequence = "ncacn_ip_tcp",
                        ServerVaultGUID = mfilesClientConfig.Guid,
                    };
                    app.AddVaultConnection(vc);
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                    {
                        string.Format("receive server push,AddVaultConnection,new project,guid={0},name={1},host={2},ok!{3},{4}",
                            mfilesClientConfig.Guid, mfilesClientConfig.Name, mfilesClientConfig.Host,
                            _formset.textBoxUsername.Text,
                            _formset.textBoxpassword.Text)
                    });
                    AppCheckForOneVault(vc, app);
                    app.LogInAsUser(mfilesClientConfig.Name, MFAuthType.MFAuthTypeSpecificMFilesUser,
                        _formset.textBoxUsername.Text,
                        _formset.textBoxpassword.Text);
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("login,ok!") });
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("指定的文档库不存在") || ex.Message.Contains("您没有该文档库的用户账号"))
                    {
                        removeVaultConnection(vc, mcr.MFilesClientApplication);
                    }
                    else
                    {
                        if (!ex.Message.Contains("您已经登录该库"))
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                        {
                            string.Format("new project,AddVaultConnection=={0},={1},={2},{3},",
                                mfilesClientConfig.Guid, mfilesClientConfig.Name, mfilesClientConfig.Host, ex.Message)
                        });
                    }
                }
                //  }
            }
            return "";
        }
        private void Showball(MfTask mt, Threadmessage otm)
        {
            _curmft = mt;
            _curtm = otm;
            BeginInvoke(new UpdateStatusDelegate(UpdateFlashStatus), new object[]
                                                                         {
                                                                             string.Format(
                                                                                 "有新消息，请查看，名称={0}，Userid={1},Clientname={2}，url<{3}>",
                                                                                 _curmft.Name ,_curtm.Userid, _curtm.Clientname, _curmft.Url)
                                                                         });
            int tipShowMilliseconds = 1000 * 10;
            string tipTitle;
            string context;
            var allnotice = Listmftasksfordata.Count(mft => otm.Clientname == mft.ClientName);
            var unnoticed = Listmftasksfordata.Count(mft => mft.IsNoticed == 0 && otm.Clientname == mft.ClientName);
            if (_curmft.Type == (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment)
            {
                tipTitle = string.Format("任务通知 : ");//"Clientname={0},Vaultname={1}", otm.Clientname, otm.Vaultname);

                context = string.Format("任务名称：{0}{1}监控人：{2}{3}截止日期：{4}{5}任务说明：{6}{7}创建时间：{8}{9}消息总计：{10}{11}未查看消息{12}",
                                       mt.Name, Environment.NewLine, mt.Monitor,
                                       Environment.NewLine, mt.Date, Environment.NewLine, mt.Desc, Environment.NewLine,
                                       mt.Time, Environment.NewLine, allnotice, Environment.NewLine,
                                       unnoticed);
            }
            else
            {
                tipTitle = string.Format("消息通知 :");//" Clientname={0},Vaultname={1}", otm.Clientname, otm.Vaultname);

                context = string.Format("名称：{0}{1}修改时间：{2}{3}截止日期：{4}{5}任务说明：{6}{7}创建时间：{8}{9}消息总计：{10}{11}未查看消息{12}",
                                       mt.Name, Environment.NewLine, mt.LastModifiedTime,
                                       Environment.NewLine, mt.Date, Environment.NewLine, mt.Desc, Environment.NewLine,
                                       mt.Time, Environment.NewLine, allnotice, Environment.NewLine,
                                       unnoticed);
            }
            notifyIcon1.ShowBalloonTip(tipShowMilliseconds, tipTitle, context, ToolTipIcon.Info);
        }
        private void openlink(string link)
        {
            // Process myProcess = new Process();
            //var ie = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Internet Explorer", "iexplore.exe");
            //myProcess.StartInfo.FileName = ie;
            //myProcess.StartInfo.Arguments = link;
            //myProcess.StartInfo.CreateNoWindow = true;
            //myProcess.StartInfo.UseShellExecute = false;
            //myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try { Process.Start(link); }
            catch (Exception) { }

            //ProcessStartInfo startInfo = new ProcessStartInfo(ie);
            //startInfo.UseShellExecute = false;
            //startInfo.Arguments = link;
            //startInfo.CreateNoWindow = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden ;

            //Process.Start(startInfo);
        }
        //public static void Writelog(string logtext)
        //{
        //    try
        //    {
        //        using (
        //            StreamWriter sw =
        //                File.AppendText(Path.Combine(Path.GetTempPath(),
        //                    DateTime.Now.Date.ToString("yyyy-MM-dd") + "noticsetlog.txt")))
        //        {
        //            sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
        //            sw.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        private void Form1_Load(object sender, EventArgs e)
        {
            CreateDesktopWebShortCut();
            if (LocalSettings.Isconfexists(Localsettings))
            {
                _localSettings = LocalSettings.Load(Localsettings);
            }
            else
            {
                _localSettings.Save(Localsettings);
            }
            var url = ConfigurationManager.AppSettings["notificationserver"];
            _formset = new NoticeSet
                           {
                               textBoxmfnotificationserver = { Text = url },
                               textBoxUsername = { Text = _localSettings.User },
                               textBoxpassword = { Text = _localSettings.Pass },
                           };

            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("软件版本:{0},",
               version) });

            //取得程序路径
            string startup = Application.ExecutablePath;
            //   BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("Application.ExecutablePath:{0}", Application.ExecutablePath) });
            try
            { //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装
                //对应于HKEY_LOCAL_MACHINE主键
                RegistryKey rKey = Registry.LocalMachine;
                //开机自动运行
                RegistryKey autoRun = rKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (autoRun != null) autoRun.SetValue("M-Files Notification", startup);
                rKey.Close();
            }
            catch (Exception exp)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                            new object[] { string.Format("应用程序目前没有注册表修改权限，实现不了开机自动运行，请赋予程序注册表修改权限！{0}", exp.Message) });
            }

            if (!SeismicControlSettings.Isconfexists())
            {
                _scsconf = new SeismicControlSettings { Name = "all", Nchecked = true };

                var mcr = MfilesClientResource.GetMfilesClientResource();
                if (!mcr.IsError())
                {
                    var vcs = mcr.VaultConnections;
                    var app = mcr.MFilesClientApplication;
                    foreach (VaultConnection vc in vcs)
                    {
                        var aconf = new SeismicControlSettings { Name = vc.Name, Nchecked = true };
                        foreach (var nt in _noticetypes)
                        {
                            var lconf = new SeismicControlSettings { Name = nt, Nchecked = true };
                            aconf.Tnodes.Add(lconf);
                        }
                        _scsconf.Tnodes.Add(aconf);
                    }
                }
                _scsconf.Save();
            }
            else
            {
                _scsconf = SeismicControlSettings.Load();
            }
            try
            {
                notifyIcon1.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "0.ico"));
                _mIcon1 = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "0.ico"));//导入图标文件
                _mIcon2 = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "2.ico"));
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("Icon error:{0}", ex.Message) });
            }
            Inittree();
            Initdataview();

            _tcheck = new Thread(new ThreadStart(Checkmflogin));
            _tcheck.Start();

            _tCheckNewProject = new Thread(new ThreadStart(CheckNewProject));
            _tCheckNewProject.Start();

            _tCheckClientApps = new Thread(new ThreadStart(CheckClientApps));
            _tCheckClientApps.Start();

            _tCheckContractor = new Thread(new ThreadStart(CheckContractor));
            _tCheckContractor.Start();

            _tCheckSelfUpdate = new Thread(new ThreadStart(CheckSelfUpdate));
            _tCheckSelfUpdate.Start();

            _tCheckSignalr = new Thread(new ThreadStart(CheckSignalr));
            _tCheckSignalr.Start();
        }

        private void CheckSelfUpdate()
        {
            CheckUpdate(version);
        }

        private async void CheckUpdate(string version)
        {
            var lv = long.Parse(version.Replace(".", ""));
            var url = string.Format("http://{0}/api/notices/GetNoticeUpdatePackage?version={1}", _formset.textBoxmfnotificationserver.Text, lv);
            var srcString = string.Empty;
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                    var response = await http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    srcString = response.Content.ReadAsStringAsync().Result;
                }
                try
                {
                    var ui = JsonConvert.DeserializeObject<UpdateInfo>(srcString);
                    if (ui.Name == string.Empty) return;
                    var exportPath = AppDomain.CurrentDomain.BaseDirectory;

                    var path = Path.Combine(exportPath, ui.Name);
                    File.WriteAllBytes(path, ui.FileContent);
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckUpdate download  {2} ok :{0},{1}", 
                                version, ui.Date,ui.Name) });
                    if (MessageBox.Show("软件有新的版本，点击确定开始升级。", "确认", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                    {
                        Process.Start(path);
                        Process.GetCurrentProcess().Kill();
                      //  noticeExit();
                    }
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckUpdate processing :{0},url={1},{2}", version, url, ex.Message) });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("发送请求时出错"))
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("软件更新查询:{0},url={1},{2}", version, url, "网站可能在更新，下次启动再查。") });
                else
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("软件更新查询 error:{0},url={1},{2}", version, url, ex.Message) });
            }
        }

        private  void CheckSignalr()
        {
            do
            {
                 ServerURI = string.Format("http://{0}/", _formset.textBoxmfnotificationserver.Text);
                Connection = new HubConnection(ServerURI);
                var pass = _formset.textBoxpassword.Text;
                var username = _formset.textBoxUsername.Text;
                if (pass.Trim() == string.Empty || username.Trim() == string.Empty)
                {
                    break;
                }
                connectSignalr();

                Thread.Sleep(1000 * 60 * 2);
            } while (!IsSignalrConnected);
            do
            {
                var pass = _formset.textBoxpassword.Text;
                var username = _formset.textBoxUsername.Text;
                if (pass.Trim() == string.Empty || username.Trim() == string.Empty)
                {
                    Thread.Sleep(1000 * 60 * 1);
                    continue;
                }
                try
                {
                    if (Connection.State.Equals(Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected))
                    {
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckSignalr reconnecting:{0}", _formset.textBoxmfnotificationserver.Text) });
                        connectSignalr();
                    }
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckSignalr reconnecting error:{0},{1}", 
                        _formset.textBoxmfnotificationserver.Text,ex.Message) });
                }
                Thread.Sleep(1000 * 60 * 1);
            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private async void connectSignalr()
        {
            try
            {
                HubProxy = Connection.CreateHubProxy("CscecPushHub");
                HubProxy.On<MfilesClientConfig>("NewProject", (mcc) =>
                    this.Invoke((Action)(() => NewProjectProcessing(mcc)
                    ))
                );
                HubProxy.On<UpdateInfo>("NoticeUpdate", (ui) =>
                   this.Invoke((Action)(() => NoticeUpdate(ui)
                   ))
               );
                HubProxy.On<VaultAppModel>("NewApp", (ui) =>
                 this.Invoke((Action)(() => NewApp(ui)
                 ))
             );
                HubProxy.On<MfTask>("NewMsg", (ui) =>
               this.Invoke((Action)(() => NewMsg(ui)
               ))
           );
                try
                {
                    await Connection.Start();
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckSignalr connected:{0}", ServerURI) });
                    IsSignalrConnected = true;
                    await HubProxy.Invoke("Login", _formset.textBoxUsername.Text, Connection.ConnectionId);
                }
                catch (Exception hex)
                {
                    if (hex.Message.Contains("发送请求时出错"))
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("signalr 查询:url={0},{1}", ServerURI, "网站可能在更新，稍后再查。") });
                    else
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                                     new object[] { string.Format("signalr 查询: error.{0}", hex.Message) });
                    IsSignalrConnected = false;
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckSignalr error:{0}", ex.Message) });
            }
        }

        private object NewMsg(MfTask returnTask)
        {
            if (returnTask.NotificationType != NotificationTypeEnum.TaskDone && returnTask.NotificationType != NotificationTypeEnum.UpdateTask
                && returnTask.NotificationType != NotificationTypeEnum.NewTask && returnTask.NotificationType != NotificationTypeEnum.WorkFlowAssigned)
                return string.Empty;

            var jsonSerializer = new JavaScriptSerializer();
            BeginInvoke(new UpdateStatusDelegate(UpdateDataView), new object[] { jsonSerializer.Serialize(returnTask) });
            BeginInvoke(new UpdateStatusDelegate(UpdatetreeEx), new object[] { "" });
            _curmft.Url = returnTask.Url;
            notifyIcon1.ShowBalloonTip(1000 * 10, "消息通知 :", returnTask.Name + returnTask.Desc, ToolTipIcon.Info);
            return string.Empty;
        }

        private object NewApp(VaultAppModel a)
        {
            try
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("AppUpdate broadcast message {2} ok :{0},{1},{3}", _localSettings.Version, a.Version, a.Guid, a.AppId) });
                CheckClientApps();
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("NewApp processing app:{0},{1},{2}", a.Guid, a.Version, ex.Message) });
            }
            return string.Empty;
        }

        private object NoticeUpdate(UpdateInfo ui)
        {
            try
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("NoticeUpdate broadcast :{0},{1}", ui.Date, ui.Name) });
                CheckUpdate(version);
                //if (ui.Name == string.Empty) return string.Empty;
                //var tmp = ui.Name.Replace(".", "");
                //var reg = new Regex(@"\d+");
                //var newversion =long.Parse( reg.Match(tmp).ToString());
                //var oldversion = long.Parse(version.Replace(".", ""));
                //if (newversion <= oldversion) return string.Empty;

                //var exportPath = AppDomain.CurrentDomain.BaseDirectory;

                //var path = Path.Combine(exportPath, ui.Name);
                //File.WriteAllBytes(path, ui.FileContent);
                //BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("NoticeUpdate download  ok :{0},{1}", 
                //                 ui.Date,ui.Name) });
                //if (MessageBox.Show("软件有新的版本，点击确定开始升级。", "确认", MessageBoxButtons.OKCancel,
                //    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                //{
                //    Process.Start(path);
                //    noticeExit();
                //}
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("NoticeUpdate processing :{0},{1},{2}", ui.Date, ui.Name, ex.Message) });
            }
            return string.Empty;
        }

        //private void logsetting(string p)
        //{
        //    Writelog(string.Format("{5}, Pass={0}, User={1}, Version={2}, Guid={3}, FilePath={4}",
        //     _localSettings.Pass, _localSettings.User, _localSettings.Version, _localSettings.Guid, _localSettings.FilePath,p));
        //}

        private async void CheckContractor()
        {
            var url = string.Format("http://{0}/api/notices/GetContractor?username={1}", _formset.textBoxmfnotificationserver.Text, _formset.textBoxUsername.Text);
            var srcString = string.Empty;
            try
            {
                const string connectname = "分包商管理";
                var mcr = MfilesClientResource.GetMfilesClientResource();
                if (mcr.IsError())
                {
                    Thread.Sleep(1000 * 60 * 1);
                    return;
                }
                var vcs = mcr.VaultConnections;
                var app = mcr.MFilesClientApplication;
                var notfound = true;
                foreach (VaultConnection vaultConnection in vcs)
                {
                    if (vaultConnection.Name == connectname && _formset.textBoxmfnotificationserver.Text.Contains(vaultConnection.NetworkAddress))
                    {
                        notfound = false;
                        break;
                    }
                }
                if (!notfound)
                {
                    Thread.Sleep(1000 * 60 * 1);
                    return;
                }
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                    var response = await http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    srcString = response.Content.ReadAsStringAsync().Result;
                }
                //     BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckContractor,ok!{0}", srcString) });
                if (srcString.Length < 40)
                {
                    Thread.Sleep(1000 * 60 * 1);
                    return;
                }
                var guid = srcString.Substring(1, 38);
                var host = srcString.Substring(39, srcString.Length - 40);
                lock (_mfilesconnectMutex)
                {
                    try
                    {
                        app.AddVaultConnection(new VaultConnection
                        {
                            AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser,
                            AutoLogin = false,
                            Name = connectname,
                            NetworkAddress = host,
                            UserName = _formset.textBoxUsername.Text,
                            Endpoint = "2266",
                            ProtocolSequence = "ncacn_ip_tcp",
                            ServerVaultGUID = guid,
                        });
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                        {
                            string.Format("AddVaultConnection,分包商管理,guid={0},host={1},ok!",
                                guid, host)
                        });
                        app.LogInAsUser(connectname, MFAuthType.MFAuthTypeSpecificMFilesUser,
                            _formset.textBoxUsername.Text,
                            _formset.textBoxpassword.Text);
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] {string.Format("login,ok!")});
                    }
                    catch (Exception ex)
                    {
                        //   BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckContractor processing :url={0},{1},", url, ex.Message) });
                    }
                }
            }
            catch (Exception ex)
            {
                //   BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckContractor error:url={0},{1},", url, ex.Message) });
            }
        }

        private async void CheckClientApps()
        {
            var url = string.Format("http://{0}/api/notices/getapp?version={1}", _formset.textBoxmfnotificationserver.Text, _localSettings.Version);
            var srcString = string.Empty;
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                    var response = await http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    srcString = response.Content.ReadAsStringAsync().Result;
                }
                try
                {
                    var retsList = JsonConvert.DeserializeObject<List<VaultAppModel>>(srcString);
                    var exportPath = AppDomain.CurrentDomain.BaseDirectory;
                    foreach (var a in retsList)
                    {
                        var path = Path.Combine(exportPath, a.Guid + ".zip");
                        File.WriteAllBytes(path, a.ZipFile);
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckClientApps download app {2} ok :{0},{1},{3}", 
                                _localSettings.Version, a.Version, a.Guid,a.AppId) });
                        _localSettings.Version = a.Version;
                        _localSettings.FilePath = path;
                        _localSettings.Guid = a.Guid;
                        _localSettings.User = _formset.textBoxUsername.Text;
                        _localSettings.Pass = _formset.textBoxpassword.Text;
                        _localSettings.Save(Localsettings);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckClientApps processing app:{0},url={1},{2}", _localSettings.Version, url, ex.Message) });
                }
                AppUpdate();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("发送请求时出错"))
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("应用更新查询:{0},url={1},{2}", _localSettings.Version, url, "网站可能在更新，稍后再查。") });
                else
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("应用更新查询 error:{0},url={1},{2}", _localSettings.Version, url, ex.Message) });
            }
        }


        private void AppUpdate()
        {
            lock (_mfilesconnectMutex)
            {
                try
                {
                    var mcr = MfilesClientResource.GetMfilesClientResource();
                    if (mcr.IsError()) return;
                    var app = mcr.MFilesClientApplication;
                    var vcs = mcr.VaultConnections;
                    foreach (VaultConnection vaultConnection in vcs)
                    {
                        if (!_formset.textBoxmfnotificationserver.Text.Contains(vaultConnection.NetworkAddress) ||
                            vaultConnection.Name.Contains("分包商")) continue;
                        AppCheckForOneVault(vaultConnection, app);
                     
                    }
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("指定的文档库不存在"))
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                        {
                            string.Format("AppUpdate processing app:{0},{1},{2},{3}",
                                _localSettings.Version, _localSettings.Guid, _localSettings.FilePath, ex.Message)
                        });
                }
            }
        }

        private void AppCheckForOneVault(VaultConnection vaultConnection, MFilesClientApplication app)
        {
            var appPath = ClientUtils.GetAppPath(vaultConnection.ServerVaultGUID);
            var needUpdate = VaultAppUtils.NeedUpdate(appPath, _localSettings.Guid, _localSettings.FilePath);
            //       BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("66,{0},{1},{2}", appPath, _localSettings.Guid, _localSettings.FilePath) });
            if (!needUpdate)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("VaultConnection {0} don't need update app {1} .", 
                                        vaultConnection.Name, _localSettings.Version) });
                return;
            }
            if (vaultConnection.IsLoggedIn())
            {
                try
                {
                    removeVaultConnection(vaultConnection, app);
                    app.AddVaultConnection(new VaultConnection
                    {
                        AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser,
                        AutoLogin = false,
                        Name = vaultConnection.Name,
                        NetworkAddress = vaultConnection.NetworkAddress,
                        UserName = _formset.textBoxUsername.Text,
                        Endpoint = "2266",
                        ProtocolSequence = "ncacn_ip_tcp",
                        ServerVaultGUID = vaultConnection.ServerVaultGUID,
                    });
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("appupdate,AddVaultConnection,new project,guid={0},name={1},host={2},ok!",
                                        vaultConnection.ServerVaultGUID, vaultConnection.Name,
                                        vaultConnection.NetworkAddress)
                                });
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("Remove and add VaultConnection {0} ,{1}，{2}",
                                        vaultConnection.Name, _localSettings.Version, ex.Message)
                                });
                }
            }
            var errs = VaultAppUtils.ExtractApp(appPath, _localSettings.Guid, _localSettings.FilePath);
            if (errs.Count > 0)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                            {
                                string.Format("ExtractApps processing app:{0},{1},{2}",
                                    vaultConnection.Name, appPath, String.Join("; ", errs))
                            });
            }
            else
            {
                try
                {
                    vaultConnection.LogInAsUser(MFAuthType.MFAuthTypeSpecificMFilesUser,
                        _formset.textBoxUsername.Text,
                        _formset.textBoxpassword.Text);
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("VaultConnection {0}  update app {1},relogin ok.",
                                        vaultConnection.Name, _localSettings.Version)
                                });
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("您已经登录该库"))
                    {
                        if (ex.Message.Contains("指定的文档库不存在"))
                        {
                            removeVaultConnection(vaultConnection, app);
                        }
                        else
                        {
                            BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                    {
                                        string.Format("VaultConnection {0}  重新登录以便加载更新的应用出错{1}，{2},{3},{4}",
                                            vaultConnection.Name, _localSettings.Version, _formset.textBoxUsername.Text,
                                            _formset.textBoxpassword.Text, ex.Message)
                                    });
                        }
                    }
                }
            }
        }

        private void CheckNewProject()
        {
            do//
            {
                var pass = _formset.textBoxpassword.Text;
                var username = _formset.textBoxUsername.Text;
                if (pass.Trim() == string.Empty || username.Trim() == string.Empty)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("请进入设置输入用户名称和密码，方便自动登录！") });
                    Thread.Sleep(1000 * 60 * 1);
                    continue;
                }
            try
            {
                OnceCheck();
                break;
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckNewProject error:{0}", ex.Message) });
            }
                Thread.Sleep(1000 * 60 * 1);
            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }
        private async void OnceCheck()
        {
            var username = _formset.textBoxUsername.Text;

            string srcString = string.Empty;
            var url = string.Format("http://{0}/api/notices/GetProjects?username={1}", _formset.textBoxmfnotificationserver.Text, username);
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                    var response = await http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    srcString = await response.Content.ReadAsStringAsync();
                    watch.Stop();
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("OnceCheck error:{0},url={1},{2}", username, url, ex.Message) });
                return;
            }
            lock (_mfilesconnectMutex)
            {
                try
                {
                    //   BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckNewProject,return srcString={0}.", srcString) });
                    if (srcString == "\"\"") return;
                    srcString = srcString.Substring(1, srcString.Length - 2);
                    srcString = srcString.Replace("\\", "");

                    if (srcString == "[]") return;
                    //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("CheckNewProject,return srcString={0}.", srcString) });
                    var js = new JavaScriptSerializer();
                    var retsList = js.Deserialize<IList<MfilesClientConfig>>(srcString);

                    var mcr = MfilesClientResource.GetMfilesClientResource();
                    if (mcr.IsError()) return;
                    var app = mcr.MFilesClientApplication;
                    var vcs = mcr.VaultConnections;
                    foreach (VaultConnection vaultConnection in vcs)
                    {
                        try
                        {
                            if (!_formset.textBoxmfnotificationserver.Text.Contains(vaultConnection.NetworkAddress))
                                continue;
                            app.LogInAsUser(vaultConnection.Name, MFAuthType.MFAuthTypeSpecificMFilesUser, username,
                                _formset.textBoxpassword.Text);
                        }
                        catch (Exception ex) //
                        {
                            if (ex.Message.Contains("您已经登录该库")) continue;
                            if (ex.Message.Contains("指定的文档库不存在"))
                                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("项目已清除,={0},={1},={2}",
                                        vaultConnection.Name, vaultConnection.NetworkAddress,
                                        vaultConnection.ServerVaultGUID)
                                });
                            else
                            {
                                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("LogInAsUser,error,={0},={1},={2},{3}",
                                        vaultConnection.Name, vaultConnection.NetworkAddress,
                                        vaultConnection.ServerVaultGUID, ex.Message)
                                });
                            }
                            removeVaultConnection(vaultConnection, app);
                        }
                    }
                    foreach (MfilesClientConfig mfilesClientConfig in retsList)
                    {
                        //BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("mfilesClientConfig,={0},={1},={2}",
                        //    mfilesClientConfig.Guid,mfilesClientConfig.Name, mfilesClientConfig.Host) });
                        var notfound = true;
                        foreach (VaultConnection vaultConnection in vcs)
                        {
                            if (vaultConnection.ServerVaultGUID == mfilesClientConfig.Guid)
                            {
                                notfound = false;
                                break;
                            }
                        }
                        if (notfound)
                        {
                            var vc = new VaultConnection();
                            try
                            {
                                vc = new VaultConnection
                                {
                                    AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser,
                                    AutoLogin = false,
                                    Name = mfilesClientConfig.Name,
                                    NetworkAddress = mfilesClientConfig.Host,
                                    UserName = username,
                                    Endpoint = "2266",
                                    ProtocolSequence = "ncacn_ip_tcp",
                                    ServerVaultGUID = mfilesClientConfig.Guid,
                                };
                                app.AddVaultConnection(vc);
                                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                {
                                    string.Format("auto check,AddVaultConnection,new project,guid={0},name={1},host={2},ok!",
                                        mfilesClientConfig.Guid, mfilesClientConfig.Name, mfilesClientConfig.Host)
                                });
                                AppCheckForOneVault(vc, app);
                                app.LogInAsUser(mfilesClientConfig.Name, MFAuthType.MFAuthTypeSpecificMFilesUser,
                                    username,
                                    _formset.textBoxpassword.Text);
                                BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                                    new object[] {string.Format("login,ok!")});
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("指定的文档库不存在"))
                                {
                                    removeVaultConnection(vc, app);
                                }//
                                else if (ex.Message.Contains("验证失败"))
                                {
                                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                    {
                                        string.Format("begin check,AddVaultConnection=={0},={1},={2},user={4},pass={5},{3},",
                                            mfilesClientConfig.Guid, mfilesClientConfig.Name, mfilesClientConfig.Host,
                                            "用户名称或密码错误，请重新检查设置！",username,_formset.textBoxpassword.Text)
                                    });
                                }
                                else
                                {
                                    if (!ex.Message.Contains("您已经登录该库"))
                                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                                    {
                                        string.Format("begin check,AddVaultConnection=={0},={1},={2},user={4},pass={5},{3},",
                                            mfilesClientConfig.Guid, mfilesClientConfig.Name, mfilesClientConfig.Host,
                                            ex.Message,username,_formset.textBoxpassword.Text)
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("指定的文档库不存在"))
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                            new object[]
                            {
                                string.Format("CheckNewProject,srcString={0}{2},ex={1}", srcString, ex.Message,
                                    Environment.NewLine)
                            });
                }
            }
        }


        private void Initdataview()
        {
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.ColumnCount = Cols;
            dataGridView1.MultiSelect = true;
            dataGridView1.ContextMenuStrip = contextMenuStripdataview;
            var col = 0;
            dataGridView1.Columns[col++].HeaderText = "客户端名称";
            dataGridView1.Columns[col++].HeaderText = "名称";
            dataGridView1.Columns[col++].HeaderText = "创建时间";
            dataGridView1.Columns[col++].HeaderText = "修改时间";
            dataGridView1.Columns[col++].HeaderText = "监控人";
            dataGridView1.Columns[col++].HeaderText = "截止日期";
            dataGridView1.Columns[col++].HeaderText = "分类";
            dataGridView1.Columns[col++].HeaderText = "说明";
            dataGridView1.Columns[col].HeaderText = "url";
            dataGridView1.Columns[col].Visible = false;
            for (var ii = 0; ii < Cols; ii++)
            {
                dataGridView1.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[ii].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        private readonly string[] _noticetypes = { "新建任务", "工作流指派", "任务更新", "任务指派" };
        private void Inittree()
        {
            try
            {
                var all = new TreeNode("全部");
                _scsconf = SeismicControlSettings.Load();
                var mcr = MfilesClientResource.GetMfilesClientResource();
                if (!mcr.IsError())
                {
                    var vcs = mcr.VaultConnections;
                    foreach (VaultConnection vc in vcs)
                    {
                        if (!_formset.textBoxmfnotificationserver.Text.Contains(vc.NetworkAddress)) continue;
                        if (!Getnodecheck(vc.Name)) continue;
                        var onevault = new TreeNode(vc.Name);
                        foreach (var a in _noticetypes)
                        {
                            if (!Getnodecheck(vc.Name, a)) continue;
                            onevault.Nodes.Add(a);
                        }
                        all.Nodes.Add(onevault);
                    }
                }
                treeView1.Nodes.Add(all);
                foreach (TreeNode b in treeView1.Nodes)
                {
                    b.Expand();
                }
            }
            catch (Exception ex)
            {
                //  MessageBox.Show("Inittree"+ex.Message);
            }
        }

        private void Checkmflogin()
        {
                try
                {
                    GetAllTasks();
                    //Flushconcerned();
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("指定的文档库不存在"))
                        BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("checkmflogin error:{0}", ex.Message) });
                }
        }

        private void Flushconcerned()
        {
            _scsconf = SeismicControlSettings.Load();
            foreach (var tm in _ltm)
            {
                foreach (var a in _scsconf.Tnodes)
                {
                    if (a.Name == tm.Clientname)
                    {
                        tm.IsConcerned = a.Nchecked;
                        break;
                    }
                }
            }
        }
      

        private void removeVaultConnection(VaultConnection vaultConnection, MFilesClientApplication app)
        {

            try
            {
                app.RemoveVaultConnection(vaultConnection.Name, false);
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                    {
                        string.Format("RemoveVaultConnection,ok,={0},={1},={2}",
                            vaultConnection.Name, vaultConnection.NetworkAddress,
                            vaultConnection.ServerVaultGUID)
                    });
            }
            catch (Exception exx)
            {
                //BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[]
                //            {
                //                string.Format("RemoveVaultConnection,error,={0},={1},={2},{3}",
                //                    vaultConnection.Name, vaultConnection.NetworkAddress,
                //                    vaultConnection.ServerVaultGUID,exx.Message)
                //            });
            }

        }

        private bool GetConcernedConf(string p)
        {
            foreach (var a in _scsconf.Tnodes)
            {
                if (a.Name == p)
                {
                    return a.Nchecked;
                }
            }
            return false;
        }
       
        private async void GetAllTasks()
        {
            var jsonSerializer = new JavaScriptSerializer();
            var task = new RequestAllTasks
            {
                UserName =  _formset.textBoxUsername.Text,
                PassWord = _formset.textBoxpassword.Text,
                Guids = new List<string>()
            };
            if (task.UserName == string.Empty || task.PassWord == string.Empty) return;
            var mcr = MfilesClientResource.GetMfilesClientResource();
            if (mcr.IsError())
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetAllTasks,GetMfilesClientResource error,需关机重启清除mfiles僵尸连接{0},", mcr.Error) });
                return;
            }
            _vcs = mcr.VaultConnections;
                    foreach (VaultConnection vaultConnection in _vcs)
                    {
                        if (!_formset.textBoxmfnotificationserver.Text.Contains(vaultConnection.NetworkAddress))
                            continue;
                       task. Guids.Add(vaultConnection.ServerVaultGUID);
                    }
            
            var paras = jsonSerializer.Serialize(task);
            string srcString = string.Empty;
            var url = string.Format("http://{0}/api/notices/GetAllTasks?request={1}", _formset.textBoxmfnotificationserver.Text, paras);
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                using (var http = new HttpClient(handler))
                {
                  
                    var response = await http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    srcString = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("发送请求时出错"))
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetAllTasks:{0},url={1},",  url, "网站可能在更新，稍后再查。") });
                else
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetAllTasks error:{0},url={1},",  url, ex.Message) });
                return;
            }
            try
            {
                if (srcString == "\"\"") return;
                srcString = srcString.Substring(1, srcString.Length - 2);
                srcString = srcString.Replace("\\", "");
          //      BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetAllTasks:{0},srcString={1},", url, srcString) });
                ResultProcessing(srcString);
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetAllTasks,srcString={0},ex={1},url={2}", srcString, ex.Message, url) });
            }
        }
    
        public class Tasks
        {
            //数据库表序列Id，对客户端无用，忽略处理
            public int Id { get; set; }
            public string Name { get; set; }

            public string CreationTime { get; set; }
            public string userid { get; set; }
            public int isNoticed { get; set; }
            public int notificationtype { get; set; }
            public int objectid { get; set; }
            public int type { get; set; }
            public int version { get; set; }
            public string url { get; set; }
            public string vaultguid { get; set; }

        }
        private void ResultProcessing(string response)
        {
            IList<MfTask> retsList;
            try
            {
                var js = new JavaScriptSerializer();
                retsList = js.Deserialize<IList<MfTask>>(response);
            }
            catch (ArgumentNullException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize ArgumentNullException,{0}", ex.Message) });
                return;
            }
            catch (ArgumentException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize ArgumentException,{0}", ex.Message) });
                return;
            }
            catch (InvalidOperationException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize InvalidOperationException,{0},response=-{1}-", ex.Message, response) });
                return;
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize Exception,{0}", ex.Message) });
                return;
            }
            foreach (var onetask in retsList)
            {
                //var returnTask = new MfTask
                //{
                //    NotificationType = (NotificationTypeEnum)onetask.notificationtype,
                //    Type = onetask.type,
                //    Id = onetask.objectid,
                //    Version = onetask.version,
                //    Url = onetask.url,
                //    Name = onetask.Name,
                //    Time = onetask.CreationTime,
                //  //  ClientName = otm.Clientname
                //};
                Onemsg(onetask);
            }
            if (retsList.Count > 0)
            {
                _curmft.Url = retsList[0].Url;
                notifyIcon1.ShowBalloonTip(1000 * 10, "消息通知 :", string.Format("{0}条新消息", retsList.Count), ToolTipIcon.Info);
            }
        }
        private void ResultProcessing(string response, Threadmessage otm)
        {
            IList<Tasks> retsList;
            try
            {
                var js = new JavaScriptSerializer();
                retsList = js.Deserialize<IList<Tasks>>(response);
            }
            catch (ArgumentNullException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize ArgumentNullException,{0}", ex.Message) });
                return;
            }
            catch (ArgumentException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize ArgumentException,{0}", ex.Message) });
                return;
            }
            catch (InvalidOperationException ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize InvalidOperationException,{0},response=-{1}-", ex.Message, response) });
                return;
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("JavaScriptSerializer：Deserialize Exception,{0}", ex.Message) });
                return;
            }
            foreach (var onetask in retsList)
            {
                var returnTask = new MfTask
                {
                    NotificationType = (NotificationTypeEnum)onetask.notificationtype,
                    Type = onetask.type,
                    Id = onetask.objectid,
                    Version = onetask.version,
                    Url = onetask.url,
                    Name = onetask.Name,
                    Time = onetask.CreationTime,
                    ClientName = otm.Clientname
                };
                Onemsg(returnTask, otm);

            }
            IEnumerable<MfTask> query = from items in Listmftasksfordata orderby items.LastModifiedTime descending where items.IsNoticed != 1 && items.ClientName == otm.Clientname select items;
            // var havenews = false;
            foreach (MfTask mft in query)
            {
                Showball(mft, otm);
                //   havenews = true;
                break;
            }
        }

        private void Onemsg(MfTask returnTask,Threadmessage otm)
        {
            if (returnTask.NotificationType != NotificationTypeEnum.TaskDone && returnTask.NotificationType != NotificationTypeEnum.UpdateTask
                  && returnTask.NotificationType != NotificationTypeEnum.NewTask && returnTask.NotificationType != NotificationTypeEnum.WorkFlowAssigned)
                return;
            var objver = new ObjVer();
            try
            {
                objver.SetIDs(returnTask.Type, returnTask.Id, returnTask.Version);
                var pvs = otm.OVault.ObjectPropertyOperations.GetProperties(objver);
                returnTask.LastModifiedTime =
                    pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                        .GetValueAsLocalizedText();
                returnTask.Desc =
                    pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription)
                        .GetValueAsLocalizedText();
                returnTask.Date = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline)
                    .GetValueAsLocalizedText();
                returnTask.Monitor = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy)
                    .GetValueAsLocalizedText();
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetProperties：Exception,{0},{1},{2},{3},{4}", otm.OVault.Name, returnTask.Type, returnTask.Id, returnTask.Version, ex.Message) });
            }

            var jsonSerializer = new JavaScriptSerializer();
            BeginInvoke(new UpdateStatusDelegate(UpdateDataView), new object[] { jsonSerializer.Serialize(returnTask) });
            BeginInvoke(new UpdateStatusDelegate(UpdatetreeEx), new object[] { "" });
        }
        private void Onemsg(MfTask returnTask)
        {
            //if (returnTask.NotificationType != NotificationTypeEnum.TaskDone && returnTask.NotificationType != NotificationTypeEnum.UpdateTask
            //      && returnTask.NotificationType != NotificationTypeEnum.NewTask && returnTask.NotificationType != NotificationTypeEnum.WorkFlowAssigned)
            //    return;
            //var objver = new ObjVer();
            //try
            //{
            //    objver.SetIDs(returnTask.Type, returnTask.Id, returnTask.Version);
            //    var pvs = otm.OVault.ObjectPropertyOperations.GetProperties(objver);
            //    returnTask.LastModifiedTime =
            //        pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
            //            .GetValueAsLocalizedText();
            //    returnTask.Desc =
            //        pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription)
            //            .GetValueAsLocalizedText();
            //    returnTask.Date = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline)
            //        .GetValueAsLocalizedText();
            //    returnTask.Monitor = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy)
            //        .GetValueAsLocalizedText();
            //}
            //catch (Exception ex)
            //{
            //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetProperties：Exception,{0},{1},{2},{3},{4}", otm.OVault.Name, returnTask.Type, returnTask.Id, returnTask.Version, ex.Message) });
            //}

            var jsonSerializer = new JavaScriptSerializer();
            BeginInvoke(new UpdateStatusDelegate(UpdateDataView), new object[] { jsonSerializer.Serialize(returnTask) });
            BeginInvoke(new UpdateStatusDelegate(UpdatetreeEx), new object[] { "" });
        }
        private void UpdatetreeEx(string notforuse)
        {
            try
            {
                treeView1.Nodes.Clear();
                var tn = new TreeNode("所有vaults");
                var allcount = 0;
                _scsconf = SeismicControlSettings.Load();
                foreach (VaultConnection vc in _vcs)
                {
                    if (!_formset.textBoxmfnotificationserver.Text.Contains(vc.NetworkAddress)) continue;
                    var vaultcheck = Getnodecheck(vc.Name);
                    foreach (var tm in _ltm)
                    {
                        if (tm.Clientname == vc.Name)
                        {
                            tm.IsConcerned = vaultcheck;
                            break;
                        }
                    }
                    if (!vaultcheck) continue;
                    var cnode = new TreeNode(vc.Name);
                    var tcount = 0;
                    var wtcount = 0;
                    //   var ndcount = 0;
                    var others = 0;
                    //   var DelDocdcount = 0;
                    var taskupdatecount = 0;
                    var taskdonecount = 0;
                    //  var DelOtherObjcount = 0;
                    //   var NewOtherObjcount = 0;
                    //  var UpdateDoccount = 0;
                    //  var UpdateOtherObjcount = 0;
                    var onecount = 0;
                    foreach (MfTask mft in Listmftasksfordata)
                    {
                        if (mft.ClientName .Contains(vc.Name))
                        {
                            switch (mft.NotificationType)
                            {
                                case NotificationTypeEnum.NewTask:
                                    tcount++;
                                    break;
                                case NotificationTypeEnum.WorkFlowAssigned:
                                    wtcount++;
                                    break;
                                /*       case NotificationTypeEnum.NewDoc:
                                           ndcount++;
                                           break;
                                       case NotificationTypeEnum.DelDoc:
                                           DelDocdcount++;
                                           break;
                                       case NotificationTypeEnum.DelOtherObj:
                                           DelOtherObjcount++;
                                           break;
                                       case NotificationTypeEnum.NewOtherObj:
                                           NewOtherObjcount++;
                                           break;
                                       case NotificationTypeEnum.UpdateDoc:
                                           UpdateDoccount++;
                                           break;
                                       case NotificationTypeEnum.UpdateOtherObj:
                                           UpdateOtherObjcount++;
                                           break;*/
                                case NotificationTypeEnum.UpdateTask:
                                    taskupdatecount++;
                                    break;
                                case NotificationTypeEnum.TaskDone:
                                    taskdonecount++;
                                    break;
                                default:
                                    others++;
                                    break;
                            }
                        }
                    }
                    if (Getnodecheck(vc.Name, "新建任务"))
                    {
                        cnode.Nodes.Add(string.Format("新建任务({0})", tcount));
                        onecount += tcount;
                    }
                    if (Getnodecheck(vc.Name, "工作流指派"))
                    {
                        cnode.Nodes.Add(string.Format("工作流指派({0})", wtcount));
                        onecount += wtcount;
                    }
                    if (Getnodecheck(vc.Name, "任务更新"))
                    {
                        cnode.Nodes.Add(string.Format("任务更新({0})", taskupdatecount));
                        onecount += taskupdatecount;
                    }
                    if (Getnodecheck(vc.Name, "任务完成"))
                    {
                        cnode.Nodes.Add(string.Format("任务完成({0})", taskdonecount));
                        onecount += taskdonecount;
                    }
                    /*    if (Getnodecheck(vc.Name, "新建文档"))
                        {
                            cnode.Nodes.Add(string.Format("新建文档({0})", ndcount));
                            onecount += ndcount;
                        }
                        if (Getnodecheck(vc.Name, "删除文档"))
                        {
                            cnode.Nodes.Add(string.Format("删除文档({0})", DelDocdcount));
                            onecount += DelDocdcount;
                        }
                        if (Getnodecheck(vc.Name, "删除其它"))
                        {
                            cnode.Nodes.Add(string.Format("删除其它({0})", DelOtherObjcount));
                            onecount += DelOtherObjcount;
                        }
                        if (Getnodecheck(vc.Name, "新建其它"))
                        {
                            cnode.Nodes.Add(string.Format("新建其它({0})", NewOtherObjcount));
                            onecount += NewOtherObjcount;
                        }
                        if (Getnodecheck(vc.Name, "更新文档"))
                        {
                            cnode.Nodes.Add(string.Format("更新文档({0})", UpdateDoccount));
                            onecount += UpdateDoccount;
                        }
                        if (Getnodecheck(vc.Name, "更新其它"))
                        {
                            cnode.Nodes.Add(string.Format("更新其它({0})", UpdateOtherObjcount));
                            onecount += UpdateOtherObjcount;
                        }
                        if (Getnodecheck(vc.Name, "其它消息"))
                        {
                            cnode.Nodes.Add(string.Format("其它消息({0})", others));
                            onecount += others;
                        }
                        */
                    cnode.Text += string.Format("({0})", onecount);
                    tn.Nodes.Add(cnode);
                    allcount += onecount;
                }
                tn.Text += string.Format("({0})", allcount);
                treeView1.Nodes.Add(tn);
                treeView1.ExpandAll();
            }
            catch (Exception) { }
        }
        private bool Getnodecheck(string p, string sonnode)
        {
            foreach (var stn in _scsconf.Tnodes)
            {
                if (stn.Name == p)
                {
                    foreach (var sontn in stn.Tnodes)
                    {
                        if (sontn.Name == sonnode)
                            return sontn.Nchecked;
                    }
                }
            }

            return false;
        }
        private bool Getnodecheck(string p)
        {
            foreach (var stn in _scsconf.Tnodes)
            {
                //BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                //  new object[] { string.Format("param={0},nodes.name={1},nodes.bool={2}！",p,stn.Name ,stn.Nchecked ) });
                if (stn.Name == p)
                {
                    return stn.Nchecked;
                }
            }

            return false;
        }

        private void UpdateDataView(string jsonmft)
        {
            if (jsonmft != string.Empty)
            {
                timer1.Start();
                var js = new JavaScriptSerializer();
                var oMfTask = js.Deserialize<MfTask>(jsonmft);
                Listmftasksfordata.Add(oMfTask);
            }
            lock (_updatedataview)
            {
                try
                {
                    dataGridView1.RowCount = Listmftasksfordata.Count;
                }
                catch (Exception)
                {
                    //  MessageBox.Show(e.Message ); 
                }

                var rowindex = 0;
                foreach (MfTask mft in Listmftasksfordata)
                {
                    if (mft.IsDeleted) continue;
                    var col = 0;
                    dataGridView1[col++, rowindex].Value = mft.ClientName;
                    dataGridView1[col++, rowindex].Value = mft.Name;
                    dataGridView1[col++, rowindex].Value = mft.Time;
                    dataGridView1[col++, rowindex].Value = mft.LastModifiedTime;
                    dataGridView1[col++, rowindex].Value = mft.Monitor;
                    dataGridView1[col++, rowindex].Value = mft.Date;
                    dataGridView1[col++, rowindex].Value = mft.NotificationType;
                    dataGridView1[col++, rowindex].Value = mft.Desc;
                    dataGridView1[col++, rowindex].Value = mft.Url;
                    rowindex++;
                }
                if (rowindex == 0)
                {
                    var col = 0;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                    dataGridView1[col++, rowindex].Value = string.Empty;
                }
                dataGridView1.EndEdit();
                // dataGridView1.Refresh();
            }
        }
        private void UpdateStatus(string status)
        {
            richTextBox1.AppendText(Environment.NewLine + string.Format("{0}--{1}", DateTime.Now, status));
        }
        private void UpdateFlashStatus(string status)
        {
            timer1.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭逻辑
            e.Cancel = true;
            this.Hide();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要退出M-Files Notification程序吗？", "确认", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
               // noticeExit();
                Process.GetCurrentProcess().Kill();
            }
        }

        private void noticeExit()
        {
            try
            {
                if (Connection != null)
                {
                    Connection.Stop();
                    Connection.Dispose();
                }
                _tcheck.Abort();
                _tCheckNewProject.Abort();
                _tCheckClientApps.Abort();
                _tCheckContractor.Abort();
                _tCheckSelfUpdate.Abort();
                _tCheckSignalr.Abort();
            }
            catch (Exception ex)
            {
              //  MessageBox.Show("1-"+ex.Message);
            }
            try{
            foreach (Thread t in _threads)
            {
                t.Abort();
            }
            notifyIcon1.Visible = false;
            this.Close();
            this.Dispose();
            Application.Exit();
             }
            catch (Exception ex)
            {
              //  MessageBox.Show("2-"+ex.Message);
            }
        }

        private void hide_Click(object sender, EventArgs e)
        {
            this.Hide();
            if (_formset != null) _formset.Hide();
        }

        private void show_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            openlink(_curmft.Url);
            foreach (var mft in Listmftasksfordata.Where(mft => mft.Time == _curmft.Time && mft.Id == _curmft.Id
                && mft.LastModifiedTime == _curmft.LastModifiedTime && mft.Type == _curmft.Type))
            {
                Listmftasksfordata.Remove(mft);
                break;
            }
            UpdateDataView("");
            if (Listmftasksfordata.Count(mft => mft.IsNoticed == 0) == 0)
            {
                timer1.Stop();
                notifyIcon1.Icon = _mIcon1;
            }
            UpdatetreeEx("");
        }

        private void textBoxmfclientname_TextChanged(object sender, EventArgs e)
        {
            Setconfig("mfclientname", textBoxmfclientname.Text);
        }
        private void Setconfig(string p1, string p2)
        {
            if (p1 == null || p2 == null) return;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSection = (AppSettingsSection)config.GetSection("appSettings");

            appSection.Settings[p1].Value = p2;
            config.Save();
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //for(int i=0;i<e.RowCount ;i++)
            //{
            //    dataGridView1.Rows[e.RowIndex +i].HeaderCell.Style .Alignment =DataGridViewContentAlignment.MiddleRight;
            //    dataGridView1.Rows[e.RowIndex + i].HeaderCell.Value = (e.RowIndex + i + 1).ToString();
            //}
            //for (int i = e.RowIndex +e.RowCount ; i < dataGridView1.Rows.Count ; i++)
            //{
            //    dataGridView1.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            //    dataGridView1.Rows[ i].HeaderCell.Value = ( i + 1).ToString();
            //}
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //for (int i = 0; i < e.RowCount; i++)
            //{
            //    dataGridView1.Rows[e.RowIndex + i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            //    dataGridView1.Rows[e.RowIndex + i].HeaderCell.Value = (e.RowIndex + i + 1).ToString();
            //}
            //for (int i = e.RowIndex + e.RowCount; i < dataGridView1.Rows.Count; i++)
            //{
            //    dataGridView1.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            //    dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            //}
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count < 1) return;
            if (dataGridView1.SelectedRows[0].Cells[dataGridView1.ColumnCount - 1].Value == null) return;
            var url = dataGridView1.SelectedRows[0].Cells[dataGridView1.ColumnCount - 1].Value.ToString();
            if (!url.Contains("m-files")) return;
            openlink(url);
            var dgvr = dataGridView1.SelectedRows[0];
            Deleteonenotice(dgvr);

            if (Listmftasksfordata.Count(mft => mft.IsNoticed == 0) == 0)
            {
                timer1.Stop();
                notifyIcon1.Icon = _mIcon1;
            }
            UpdatetreeEx("");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            lock (_updatedataview)
            {
                if (e.Node.Level == 0)
                {
                    if (Checkcount(Listmftasksfordata.Count)) return;
                    dataGridView1.RowCount = Listmftasksfordata.Count;
                    var rowindex = 0;
                    foreach (var mft in Listmftasksfordata)
                    {
                        if (mft.IsDeleted) continue;
                        var col = 0;
                        dataGridView1[col++, rowindex].Value = mft.ClientName;
                        dataGridView1[col++, rowindex].Value = mft.Name;
                        dataGridView1[col++, rowindex].Value = mft.Time;
                        dataGridView1[col++, rowindex].Value = mft.LastModifiedTime;
                        dataGridView1[col++, rowindex].Value = mft.Monitor;
                        dataGridView1[col++, rowindex].Value = mft.Date;
                        dataGridView1[col++, rowindex].Value = mft.NotificationType;
                        dataGridView1[col++, rowindex].Value = mft.Desc;
                        dataGridView1[col, rowindex].Value = mft.Url;
                        rowindex++;
                    }
                }
                else if (e.Node.Level == 1)
                {
                    var count = Listmftasksfordata.Count(mft => e.Node.Text.Contains(mft.ClientName));
                    if (Checkcount(count)) return;
                    dataGridView1.RowCount = count;
                    var rowindex = 0;
                    foreach (MfTask mft in Listmftasksfordata)
                    {
                        if (mft.IsDeleted) continue;
                        if (!e.Node.Text.Contains(mft.ClientName)) continue;
                        var col = 0;
                        dataGridView1[col++, rowindex].Value = mft.ClientName;
                        dataGridView1[col++, rowindex].Value = mft.Name;
                        dataGridView1[col++, rowindex].Value = mft.Time;
                        dataGridView1[col++, rowindex].Value = mft.LastModifiedTime;
                        dataGridView1[col++, rowindex].Value = mft.Monitor;
                        dataGridView1[col++, rowindex].Value = mft.Date;
                        dataGridView1[col++, rowindex].Value = mft.NotificationType;
                        dataGridView1[col++, rowindex].Value = mft.Desc;
                        dataGridView1[col, rowindex].Value = mft.Url;
                        rowindex++;
                    }
                }
                else
                {
                    var ntype = NotificationTypeEnum.Other;
                    if (e.Node.Text.Contains("新建任务"))
                        ntype = NotificationTypeEnum.NewTask;
                    else if (e.Node.Text.Contains("工作流指派"))
                        ntype = NotificationTypeEnum.WorkFlowAssigned;
                    /*      else if (e.Node.Text.Contains("新建文档"))
                              ntype = NotificationTypeEnum.NewDoc;
                          else if (e.Node.Text.Contains("更新其它"))
                              ntype = NotificationTypeEnum.UpdateOtherObj;
                          else if (e.Node.Text.Contains("删除文档"))
                              ntype = NotificationTypeEnum.DelDoc;
                          else if (e.Node.Text.Contains("删除其它"))
                              ntype = NotificationTypeEnum.DelOtherObj;
                          else if (e.Node.Text.Contains("新建其它"))
                              ntype = NotificationTypeEnum.NewOtherObj;
                          else if (e.Node.Text.Contains("更新文档"))
                              ntype = NotificationTypeEnum.UpdateDoc;*/
                    else if (e.Node.Text.Contains("任务完成"))
                        ntype = NotificationTypeEnum.TaskDone;
                    else if (e.Node.Text.Contains("任务更新"))
                        ntype = NotificationTypeEnum.UpdateTask;
                    var count = Listmftasksfordata.Count(mft => e.Node.Parent.Text.Contains(mft.ClientName) && mft.NotificationType == ntype);

                    if (Checkcount(count)) return;
                    dataGridView1.RowCount = count;
                    var rowindex = 0;
                    foreach (MfTask mft in Listmftasksfordata)
                    {
                        if (mft.IsDeleted) continue;
                        if (!e.Node.Parent.Text.Contains(mft.ClientName) || mft.NotificationType != ntype) continue;
                        var col = 0;
                        dataGridView1[col++, rowindex].Value = mft.ClientName;
                        dataGridView1[col++, rowindex].Value = mft.Name;
                        dataGridView1[col++, rowindex].Value = mft.Time;
                        dataGridView1[col++, rowindex].Value = mft.LastModifiedTime;
                        dataGridView1[col++, rowindex].Value = mft.Monitor;
                        dataGridView1[col++, rowindex].Value = mft.Date;
                        dataGridView1[col++, rowindex].Value = mft.NotificationType;
                        dataGridView1[col++, rowindex].Value = mft.Desc;
                        dataGridView1[col, rowindex].Value = mft.Url;
                        rowindex++;
                    }
                }
                dataGridView1.EndEdit();
            }

        }

        private bool Checkcount(int count)
        {
            if (count < 1)
            {
                const int rowindex = 0;
                dataGridView1.RowCount = 1;
                var col = 0;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col++, rowindex].Value = string.Empty;
                dataGridView1[col, rowindex].Value = string.Empty;
                dataGridView1.EndEdit();
                return true;
            }
            return false;
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_formset == null)
            {
                _formset = new NoticeSet();
                _formset.textBoxmfnotificationserver.Text = ConfigurationManager.AppSettings["notificationserver"];
                _formset.textBoxUsername.Text = _localSettings.User;
                _formset.textBoxpassword.Text = _localSettings.Pass;
            }
            if (!_formset.Visible)
            {
                _formset.ShowDialog();
            }
            UpdatetreeEx("");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_mIcon1 != null && _mIcon2 != null)
            {
                //只要timer1被启动，则在两个图标之间不断进行选择变换，实现动画效果
                if (_mBFlag == true)
                {
                    notifyIcon1.Icon = _mIcon2; _mBFlag = false;
                }
                else
                {
                    notifyIcon1.Icon = _mIcon1; _mBFlag = true;
                }
            }
        }

        private void buttonset_Click(object sender, EventArgs e)
        {
            if (_formset == null)
            {
                _formset = new NoticeSet();
                _formset.textBoxmfnotificationserver.Text = ConfigurationManager.AppSettings["notificationserver"];
                _formset.textBoxUsername.Text = _localSettings.User;
                _formset.textBoxpassword.Text = _localSettings.Pass;
            }
            if (!_formset.Visible)
            {
                _formset.ShowDialog();
            }
            UpdatetreeEx("");
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvr in dataGridView1.SelectedRows)
            {
                Deleteonenotice(dgvr);
            }
            if (Listmftasksfordata.Count(mft => mft.IsNoticed == 0) == 0)
            {
                timer1.Stop();
                notifyIcon1.Icon = _mIcon1;
            }
            if (dataGridView1.SelectedRows.Count > 0)
                UpdatetreeEx("");
        }

        private void 查看ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvr in dataGridView1.SelectedRows)
            {
                openlink(dgvr.Cells[Cols - 1].Value.ToString());
                Deleteonenotice(dgvr);
            }
            if (Listmftasksfordata.Count(mft => mft.IsNoticed == 0) == 0)
            {
                timer1.Stop();
                notifyIcon1.Icon = _mIcon1;
            }
            if (dataGridView1.SelectedRows.Count > 0)
                UpdatetreeEx("");
        }

        private void Deleteonenotice(DataGridViewRow dgvr)
        {
            foreach (var mft in Listmftasksfordata)
            {
                if (mft.ClientName == dgvr.Cells[0].Value.ToString() && mft.Name == dgvr.Cells[1].Value.ToString()
                    && mft.Time == dgvr.Cells[2].Value.ToString() && mft.LastModifiedTime == dgvr.Cells[3].Value.ToString()
                    && mft.Monitor == dgvr.Cells[4].Value.ToString() && mft.Date == dgvr.Cells[5].Value.ToString()
                    && mft.NotificationType.ToString() == dgvr.Cells[6].Value.ToString() && mft.Desc == dgvr.Cells[7].Value.ToString()
                    && mft.Url == dgvr.Cells[8].Value.ToString())
                {
                    Listmftasksfordata.Remove(mft);

                    break;
                }
            }

            try
            {
                if (dgvr.Index == dataGridView1.RowCount - 1)
                {
                    for (var i = 0; i < Cols; i++)
                    {
                        dgvr.Cells[i].Value = string.Empty;
                    }
                    //if(dataGridView1 .RowCount >1)
                    //dataGridView1.RowCount--;
                }
                else dgvr.Visible = false;
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                new object[] { string.Format("delete notice in view :{0}！", ex.Message) });
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void bIM平台网站ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://gc.cscec82.com:8000/");
            CreateDesktopWebShortCut();

        }

        private void CreateDesktopWebShortCut()
        {
            var name = "项目管理BIM云平台-网页端";

            string deskTop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            if (System.IO.File.Exists(deskTop + "\\" + name + ".lnk"))  //
            {
                return;
            }
            try
            {
                WshShell shell = new WshShell();
                //快捷键方式创建的位置、名称
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + name + ".lnk");

                shortcut.TargetPath = @"http://gc.cscec82.com:8000/"; //目标文件
                //shortcut.WorkingDirectory = System.Environment.CurrentDirectory;//该属性指定应用程序的工作目录，当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。
                //shortcut.WindowStyle = 1; //目标应用程序的窗口状态分为普通、最大化、最小化【1,3,7】
                //shortcut.Description = "自动更新程序"; //描述
                shortcut.IconLocation = Application.StartupPath + "\\logo.ico";  //快捷方式图标
                //shortcut.Arguments = "";
                //shortcut.Hotkey = "CTRL+ALT+F11"; // 快捷键
                shortcut.Save(); //必须调用保存快捷才成创建成功
            }
            catch (System.Exception ex)
            {
                return;
            }
        }
    }

}
