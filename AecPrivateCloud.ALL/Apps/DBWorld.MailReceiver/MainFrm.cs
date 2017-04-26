using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBWorld.MailCore.Common;
using MailKit.Net.Pop3;
using MimeKit;
using SimulaDesign.WPFPluginCore;

namespace DBWorld.MailReceiver
{
    public partial class MainFrm : Form
    {
        /// <summary>
        /// 获取邮件结果描述
        /// </summary>
        private string _result = string.Empty;
        
        /// <summary>
        /// pop对象
        /// </summary>
        private readonly Pop3Client _pop3Client;

        /// <summary>
        /// 线程对象
        /// </summary>
        private readonly BackgroundWorker _bkWorker;

        /// <summary>
        /// mfiles vault对象
        /// </summary>
        private MFilesAPI.Vault _vault;

        /// <summary>
        /// 文件列表
        /// </summary>
        private List<string> _fileList = new List<string>();

        /// <summary>
        /// 邮箱配置
        /// </summary>
        private MailConfig _config;

        public MainFrm()
        {
            InitializeComponent();

            _pop3Client = new Pop3Client();
            _bkWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
        }

        #region 事件函数
        private async void MainFrm_Load(object sender, EventArgs e)
        {
            this.lblStatus.Text = "正在读取邮箱配置...";
            await Task.Run(() =>
            {
                _vault = MailCore.MF.MFilesUtil.GetVaultByName();
                _config = MailCore.MF.MfMailConfig.GetMailConfig(_vault);
            });
         
            if (String.IsNullOrEmpty(_config.Email))
            {
                MessageBox.Show("邮件地址为空，请先点击“设置”配置邮箱信息！", 
                    "收信", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                this.Close();
            }

            if (String.IsNullOrEmpty(_config.RecvAddr))
            {
                MessageBox.Show("POP服务器地址为空，请先点击“设置”配置邮箱信息！",
                   "收信",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information,
                   MessageBoxDefaultButton.Button1);
                this.Close();
            }

            _bkWorker.DoWork += BkWorkerOnDoWork;
            _bkWorker.ProgressChanged += BkWorkerOnProgressChanged;
            _bkWorker.RunWorkerCompleted += BkWorkerOnRunWorkerCompleted;
            _bkWorker.RunWorkerAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this.btnClose.Text == "取消(&C)")
            {
                _bkWorker.CancelAsync();
            }

            this.Close();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case IPCUtil.WM_COPYDATA:
                    //var st = (IPCUtil.CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(IPCUtil.CopyDataStruct));
                    //var strData = Marshal.PtrToStringUni(st.lpData);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region 线程函数
        private void BkWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            SetControlText(lblMailAddr, String.Format("{0}", _config.Email));
            if (!String.IsNullOrEmpty(_config.MarkUp))
            {
                SetControlText(lblFilter, String.Format("过滤规则：主题中包含“{0}”的邮件。", _config.MarkUp));
            }

            try
            {
                if (_bkWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                ConnectPopClient(_config);
                var count = _pop3Client.Count;
                var lastTime = MailCore.MF.MessageToMf.GetLastTimeFromMail(_vault);
                for (int i = count-1; i >= 0; i--)
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    var str = String.Empty;
                    try
                    {
                        //获取邮件成功
                        var messge = _pop3Client.GetMessage(i);
                        if (messge.Date.DateTime > lastTime)
                        {
                            switch (SaveMessages(messge))
                            {
                                case -1:
                                    str = String.Format("正在获取邮件...({0} of {1})    保存邮件失败.", count - i, count);
                                    break;
                                case 0:
                                    str = String.Format("正在获取邮件...({0} of {1})    获取成功.", count - i, count);
                                    break;
                                case 1:
                                    str = String.Format("正在获取邮件...({0} of {1})    主题不符合规则.", count - i, count);
                                    break;
                            }
                        }
                        else
                        {
                            i = 1;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        str = String.Format("正在获取邮件...({0} of {1})    获取失败.", count - i, count);
                    }

                    SetControlText(lblStatus, str);
                    _bkWorker.ReportProgress((int)(((double)(count - i) / count) * 100));
                }

                _result = string.Format("任务完成！");
            }
            catch (Exception ex)
            {
                _result = ex.Message;
            }

            _bkWorker.ReportProgress(100);
        }

        private void BkWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                progressBar.Value = e.ProgressPercentage;
            }
            catch (Exception)
            {
                progressBar.Value = 0;
            }
        }

        private void BkWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _pop3Client.Dispose();
            btnClose.Text = "关闭(&C)";
            lblStatus.Text = _result;
        }
        #endregion

        #region 操作函数
        /// <summary>
        /// 线程中设置控件文本
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="text"></param>
        private void SetControlText(Control obj, string text)
        {
            if (obj.InvokeRequired)
            {
                obj.Invoke(new MethodInvoker(delegate()
                {
                    obj.Text = text;
                }));
            }
            else
            {
                obj.Text = text;
            }
        }

        /// <summary>
        /// 保存邮件
        /// </summary>
        /// <param name="msg"> 邮件</param>
        /// <returns>
        /// 0保存到MFiles中
        /// 1:需要过滤的邮件
        /// 2:MFiles中已存在
        /// </returns>
        private int SaveMessages(MimeMessage msg)
        {
            //主题中不含过滤条件，不保存
           if (!msg.Subject.Contains(_config.MarkUp))
           {
               return 1;
           }

            //保存邮件
            if (!MailCore.MF.MessageToMf.SaveRecvMailToMf(_vault, 
                msg, 
                MailCore.MF.MessageToMf.GetFolderIdByName(_vault, "收件箱")))
            {
                return -1;
            }

            return 0;
        }

       /// <summary>
        /// 连接pop服务
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private void ConnectPopClient(MailConfig config)
        {
            _pop3Client.Connect(config.RecvAddr, config.RecvPort, config.RecvSSL);
            _pop3Client.AuthenticationMechanisms.Remove("XOAUTH2");
            _pop3Client.Authenticate(config.Email, config.PassWord);
        }

        #endregion

    }
}
