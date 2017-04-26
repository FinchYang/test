using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using MFilesAPI;
using MfNotification.Core.NotifyObject;
using NetSparkle;
using System.Net.Http;
using System.Reflection;
namespace Notification.Client
{
    public partial class Form1 : Form
    {
        private Sparkle _sparkle;
        private delegate void UpdateStatusDelegate(string status);
        const int Cols = 9;
        private readonly List<Thread> _threads = new List<Thread>();
        private Thread _tcheck;
        private MfTask _curmft;
        private Threadmessage _curtm;
        private readonly List<Threadmessage> _ltm = new List<Threadmessage>();
        private readonly Mutex _updatedataview = new Mutex();
        public List<MfTask> Listmftasksfordata = new List<MfTask>();
        private VaultConnections _vcs;
        private NoticeSet _formset;
        private SeismicControlSettings _scsconf = new SeismicControlSettings();

        private Icon _mIcon1;
        private Icon _mIcon2;
        private bool _mBFlag;

        public Form1()
        {
            InitializeComponent();
            _mBFlag = true;
            timer1.Interval = 500;
          
          //  var url = "http://" + ConfigurationManager.AppSettings["notificationserver"] + "/versioninfo.xml";
          //  _sparkle = new Sparkle(url)
          //  {
          //      ShowDiagnosticWindow = true,
          //      TrustEverySSLConnection = true,

          //    //  EnableSystemProfiling = true,
          //     // SystemProfileUrl = new Uri("http://update.applimit.com/netsparkle/stat/profileInfo.php")
          //  };

          //  _sparkle.updateDetected += new UpdateDetected(_sparkle_updateDetected);
          ////  _sparkle.EnableSilentMode = true;
          //  _sparkle.HideReleaseNotes = true;

          //  _sparkle.StartLoop(true, TimeSpan.FromMilliseconds(1000));
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
          try{  Process.Start(link);}catch (Exception ){}

            //ProcessStartInfo startInfo = new ProcessStartInfo(ie);
            //startInfo.UseShellExecute = false;
            //startInfo.Arguments = link;
            //startInfo.CreateNoWindow = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden ;

            //Process.Start(startInfo);
        }
        //void _sparkle_updateDetected(object sender, UpdateDetectedEventArgs e)
        //{
        //    DialogResult res = MessageBox.Show("Update detected, perform unattended", "Update", MessageBoxButtons.YesNoCancel);

        //    if (res == System.Windows.Forms.DialogResult.Yes)
        //        e.NextAction = nNextUpdateAction.performUpdateUnattended;
        //    else if (res == System.Windows.Forms.DialogResult.Cancel)
        //        e.NextAction = nNextUpdateAction.prohibitUpdate;
        //    else
        //        e.NextAction = nNextUpdateAction.showStandardUserInterface;
        //}
        private void Form1_Load(object sender, EventArgs e) 
        {
            var url=ConfigurationManager.AppSettings["notificationserver"];
            var ttt = Environment.ExpandEnvironmentVariables("%temp%");
            BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                           new object[] { string.Format("automatic update url: " + url+ttt) });
            _formset = new NoticeSet
                           {
                               textBoxmfnotificationserver =
                                   {Text = url}
                           };
       

           //自动升级
            System.Drawing.Icon icon =
                System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _sparkle = new Sparkle("http://" + url + "/versioninfo.xml", icon);
           
          //  _sparkle.Configuration.ShowDiagnosticWindow = true;
           
            _sparkle.StartLoop(true, true, TimeSpan.FromMinutes(1));
      

            //取得程序路径
            string startup = Application.ExecutablePath;
            //   BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("Application.ExecutablePath:{0}", Application.ExecutablePath) });
            try
            { //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装
                //对应于HKEY_LOCAL_MACHINE主键
                RegistryKey rKey = Registry.LocalMachine;
                //开机自动运行
                RegistryKey autoRun = rKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                autoRun.SetValue("M-Files Notification", startup);
                rKey.Close();
            }
            catch (Exception exp)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                            new object[] { string.Format("应用程序目前没有注册表修改权限，实现不了开机自动运行，请赋予程序注册表修改权限！") });
            }

            if (!SeismicControlSettings.Isconfexists())
            {
                _scsconf = new SeismicControlSettings { Name = "all", Nchecked = true };
                var app = new MFilesClientApplication();
                var vcs = app.GetVaultConnections();
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
        }

        private void Initdataview()
        {
            dataGridView1.EditMode= DataGridViewEditMode.EditProgrammatically;
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
        //private readonly string[] _noticetypes = { "新建任务", "工作流指派", "新建文档", "删除文档", "删除其它", "新建其它", "更新文档", "更新其它", "其它消息" };
        private readonly string[] _noticetypes = { "新建任务", "工作流指派", "任务更新", "任务指派" };
        private void Inittree()
        {
            var app = new MFilesClientApplication();
            var vcs = app.GetVaultConnections();
            var all = new TreeNode("全部");
            _scsconf = SeismicControlSettings.Load();
            foreach (VaultConnection vc in vcs)
            {
                if (!Getnodecheck(vc.Name)) continue;
                var onevault = new TreeNode(vc.Name);
                foreach (var a in _noticetypes)
                {
                    if (!Getnodecheck(vc.Name, a)) continue;
                    onevault.Nodes.Add(a);
                }
                all.Nodes.Add(onevault);
            }
            treeView1.Nodes.Add(all);
            foreach (TreeNode b in treeView1.Nodes)
            {
                b.Expand();
            }
        }

        private void Checkmflogin()
        {
            do
            {
                try
                {
                    Startdaemon();
                    Flushconcerned();
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("checkmflogin error:{0}", ex.Message) });
                }
                Thread.Sleep(1000 * 60 * 1);
            } while (true);
            return;
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
        private void Startdaemon()
        {
            var capp = new MFilesClientApplication();
            _vcs = capp.GetVaultConnections();
            foreach (VaultConnection vc in _vcs)
            {
                if (!vc.IsLoggedIn()) continue;
                var alreadystart = false;
                foreach (Threadmessage otm in _ltm)
                {
                    if (otm.Clientname == vc.Name)
                    {
                        alreadystart = true;
                        break;
                    }
                }
                if (alreadystart) continue;
                var oneclient = vc.BindToVault((IntPtr)0, true, true);
                var parStart = new ParameterizedThreadStart(InitiativeQuery);
                var myThread = new Thread(parStart);
                var tm = new Threadmessage
                             {
                                 IsConcerned = GetConcernedConf(vc.Name),
                                 OVault = oneclient,
                                 VaultGuid = vc.ServerVaultGUID,
                                 Userid = oneclient.CurrentLoggedInUserID,
                                 Clientname = vc.Name,
                                 Vaultname = vc.ServerVaultName,
                                 WebServer = ConfigurationManager.AppSettings["notificationserver"]
                             };
                myThread.Start(tm);
                if (tm.IsConcerned)
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus),
                                new object[]
                                {
                                    string.Format("客户端名称={0},vault={1},UserID={2}，启动查询线程", vc.Name, vc.ServerVaultName ,
                                                  oneclient.CurrentLoggedInUserID)
                                });
                _threads.Add(myThread);
                _ltm.Add(tm);
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
        private void InitiativeQuery(object tm)
        {
            var otm = (Threadmessage)tm;
            do
            {
                if (otm.IsConcerned)
                    Queryone(otm);
                //var tmpp=new NetSparkleAppCastItem[1] ;
                //_sparkle.ShowUpdateNeededUI(tmpp ,true);
                //try
                //{
                //    var strtmp = string.Format("AppcastUrl={0},CheckForUpdatesAtUserRequest={},InstalledVersion={},LastCheckTime={},LastProfileUpdate={}",
                //        _sparkle.AppcastUrl, _sparkle.CheckForUpdatesAtUserRequest().ToString(),
                //        _sparkle.Configuration.InstalledVersion, _sparkle.Configuration.LastCheckTime, _sparkle.Configuration.LastProfileUpdate);
                //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { strtmp });
                //}
                //catch(Exception ee){
                //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { "sparckle error: "+ee.Message });
                //}
                // try{   var strtmp1 = string.Format(" SkipThisVersion={0},ApplicationName={1},ShowDiagnosticWindow={2},UseReflectionBasedAssemblyAccessor={3}",
                //          _sparkle.Configuration.SkipThisVersion, _sparkle.GetApplicationConfig().ApplicationName,
                //          _sparkle.GetApplicationConfig().ShowDiagnosticWindow, _sparkle.GetApplicationConfig().UseReflectionBasedAssemblyAccessor);
                //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { strtmp1 });
                //    var str2 = string.Format("HideReleaseNotes={0},HideReleaseNotes={1},IsUpdateLoopRunning={2},ProgressWindow={3},AbsolutePath={4},CurrentItem={5}",
                //        _sparkle.HideReleaseNotes, _sparkle.IsUpdateLoopRunning, _sparkle.ProgressWindow, _sparkle.SystemProfileUrl.AbsolutePath, _sparkle.UserWindow.CurrentItem);
                //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { str2 });
                //}
                //catch(Exception ee){
                //    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { "sparckle error1: "+ee.Message });
                //}
                //   MessageBox.Show(strtmp + str2);
                Thread.Sleep(1000 * 60 * 1);
            } while (true);
        }
        // static async void dooPost()
        //{
        //  

        //}
             /// <summary>
        /// HttpClient实现Get请求
        /// </summary>
        static async void dooGet()
        {
          
        }
        private async void Queryone1(Threadmessage otm)
        {
            var jsonSerializer = new JavaScriptSerializer();
            // var paras = string.Format("JsonStr={0}", jsonSerializer.Serialize(mt));
            var paras = jsonSerializer.Serialize(otm);
            var url = string.Format("http://{0}/api/notice", _formset.textBoxmfnotificationserver.Text, paras);

            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                var payload = Encoding.UTF8.GetBytes(paras);
                request.ContentLength = payload.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);

                writer.Close();
                writer.Dispose();
                //var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                ////创建HttpClient（注意传入HttpClientHandler）
                //using (var http = new HttpClient(handler))
                //{
                //    //使用FormUrlEncodedContent做HttpContent
                //    var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                //         {
                //             {"Info", paras} //键名必须为空
                //         });

                //    //await异步等待回应

                //    var response = await http.PostAsync(url, content);
                //    //确保HTTP成功状态值
                //    response.EnsureSuccessStatusCode();
                    //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("ResultProcessing,url={0},",
                        url) });
                 //   Writelog(await response.Content.ReadAsStringAsync());
                
            }
            
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("queryone error:{0},url={1},{2}", otm.Userid, url, ex.Message) });
                return;
            }
    
        }
        private  async void Queryone(Threadmessage otm)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var task = new MfTask
                           {
                               VaultGuid = otm.VaultGuid,
                               ClientType = 1,
                               UserId = otm.Userid.ToString(CultureInfo.InvariantCulture)
                           };
            var paras = jsonSerializer.Serialize(task);
            string srcString=string.Empty;
         //   var url = string.Format("http://{0}/api/notice?request={1}", _formset.textBoxmfnotificationserver.Text, paras);
            var url = string.Format("http://{0}/api/notices?request={1}", _formset.textBoxmfnotificationserver.Text, paras);
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

                using (var http = new HttpClient(handler))
                {
                    //await异步等待回应
                    var response = await http.GetAsync(url);
                    //确保HTTP成功状态值
                    response.EnsureSuccessStatusCode();
                    srcString = await response.Content.ReadAsStringAsync();
                  
                    watch.Stop();
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("ResultProcessing,url={0},ElapsedMilliseconds={1}，{2}",
                        url, watch.ElapsedMilliseconds,srcString) });
                    ////await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                 //   Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
         
                //BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("ResultProcessing,url={0},ElapsedMilliseconds={1}，VaultGuid={2},result length={3},{4}",
                //        url, watch.ElapsedMilliseconds,task.VaultGuid,srcString.Length,await response.Content.ReadAsStringAsync()) });
               
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("queryone error:{0},url={1},{2}", otm.Userid, url, ex.Message) });
                return;
            }
            try
            {
                if (srcString == "\"\"") return;
                srcString = srcString.Substring(1, srcString.Length - 2);
                srcString = srcString.Replace("\\", "");
                ResultProcessing(srcString, otm);
            }
            catch (Exception ex)
            {
                BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("ResultProcessing,srcString={0},ex={1},url={2}", srcString, ex.Message, url) });
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
                var returnTask=new MfTask
                {
                    NotificationType = (NotificationTypeEnum) onetask.notificationtype,
                    Type = onetask.type,
                    Id = onetask.objectid,
                    Version = onetask.version,
                    Url = onetask.url,
                    Name=onetask.Name,
                    Time=onetask.CreationTime,
                    ClientName = otm.Clientname
                };

                if (returnTask.NotificationType != NotificationTypeEnum.TaskDone && returnTask.NotificationType != NotificationTypeEnum.UpdateTask
                    && returnTask.NotificationType != NotificationTypeEnum.NewTask && returnTask.NotificationType != NotificationTypeEnum.WorkFlowAssigned)
                    continue;
                var objver = new ObjVer();
                try
                {
                    objver.SetIDs(returnTask.Type, returnTask.Id, returnTask.Version);
                    var pvs = otm.OVault.ObjectPropertyOperations.GetProperties(objver);
                    returnTask.LastModifiedTime =
                        pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                            .GetValueAsLocalizedText();
                    returnTask.Desc =
                        pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription)
                            .GetValueAsLocalizedText();
                    returnTask.Date = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline)
                        .GetValueAsLocalizedText();
                    returnTask.Monitor = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy)
                        .GetValueAsLocalizedText();
                }
                catch (Exception ex)
                {
                    BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetProperties：Exception,{0},{1},{2},{3},{4}", otm.OVault.Name, returnTask.Type, returnTask.Id, returnTask.Version,ex.Message) });
                }
              
                var jsonSerializer = new JavaScriptSerializer();
                BeginInvoke(new UpdateStatusDelegate(UpdateDataView), new object[] { jsonSerializer.Serialize(returnTask) });
                BeginInvoke(new UpdateStatusDelegate(UpdatetreeEx), new object[] { "" });
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
                    var ndcount = 0;
                    var others = 0;
                    var DelDocdcount = 0;
                    var taskupdatecount = 0;
                        var taskdonecount = 0;
                    var DelOtherObjcount = 0;
                    var NewOtherObjcount = 0;
                    var UpdateDoccount = 0;
                    var UpdateOtherObjcount = 0;
                    var onecount = 0;
                    foreach (MfTask mft in Listmftasksfordata)
                    {
                        if (mft.ClientName == vc.Name)
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
            catch (Exception ex) { }
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
         //   MessageBox.Show(Listmftasksfordata.Count.ToString()+"--in up");
            lock (_updatedataview)
            {
             //   MessageBox.Show(Listmftasksfordata.Count.ToString()+"-- in lock");
                try
                {
                    dataGridView1.RowCount = Listmftasksfordata.Count;
                }
                catch (Exception e) {
                  //  MessageBox.Show(e.Message ); 
                }

                var rowindex = 0;
             //   MessageBox.Show(Listmftasksfordata.Count.ToString() + "-- before foreach");
                foreach (MfTask mft in Listmftasksfordata)
                {
                //    MessageBox.Show(Listmftasksfordata.Count.ToString() + "-- in foreach");
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
          //      MessageBox.Show(Listmftasksfordata.Count.ToString()+"--"+rowindex );
                if (rowindex==0)
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
                try
                {
                    _sparkle.StopLoop();
                    _tcheck.Abort();
                }
                catch (Exception) { }
                foreach (Thread t in _threads)
                {
                    t.Abort();
                }
                notifyIcon1.Visible = false;
                this.Close();
                this.Dispose();
                Application.Exit();
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
         //   MessageBox.Show(Listmftasksfordata.Count.ToString ()+"111aaa");
            foreach (var mft in Listmftasksfordata.Where(mft => mft.Time == _curmft.Time && mft.Id == _curmft.Id
                && mft.LastModifiedTime == _curmft.LastModifiedTime && mft.Type == _curmft.Type))
            {
                Listmftasksfordata.Remove(mft);
                break;
            }
         //   MessageBox.Show(Listmftasksfordata.Count.ToString()+"after del");
            UpdateDataView("");
        //    MessageBox.Show(Listmftasksfordata.Count.ToString() + "222");
            if (Listmftasksfordata.Count(mft => mft.IsNoticed == 0) == 0)
            {
                timer1.Stop();
              //  MessageBox.Show(Listmftasksfordata.Count.ToString() + "333");
                notifyIcon1.Icon = _mIcon1;
            }
            UpdatetreeEx("");
         //   MessageBox.Show(Listmftasksfordata.Count.ToString() + "444");
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
            if(dataGridView1.SelectedRows[0].Cells[dataGridView1.ColumnCount - 1].Value==null) return;
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
    }
}
