using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBWorld.MailCore.Common;
using DBWorld.MailCore.MF;

using SimulaDesign.WPFPluginCore;

namespace DBWorld.MailConfig
{
    public partial class MainFrm : Form
    {
        private const string DigitExpression = @"^\d+\.?\d+$";

        /// <summary>
        /// MFiles库
        /// </summary>
        private  MFilesAPI.Vault _vault;

        /// <summary>
        /// 初始配置
        /// </summary>
        private MailCore.Common.MailConfig _initConfig;

        /// <summary>
        /// 当前配置
        /// </summary>
        private MailCore.Common.MailConfig _currConfig;

        /// <summary>
        /// 线程对象
        /// </summary>
        private readonly BackgroundWorker _bkWorker;

        /// <summary>
        /// 初始化界面
        /// </summary>
        private readonly BackgroundWorker _bkInitWorker;

        /// <summary>
        /// 项目ID
        /// </summary>
        private string _projId;

        private string _webUrl;

        public MainFrm(string projId, string weburl)
        {
            InitializeComponent();

            _projId = projId;

            _webUrl = weburl ?? String.Empty;
            _webUrl = _webUrl.TrimEnd('/');
            
            _bkWorker = new BackgroundWorker();
            _bkInitWorker = new BackgroundWorker();
            Load += OnLoad;
        }

        private async void OnLoad(object sender, EventArgs eventArgs)
        {
            Text = "邮箱设置 - 正在加载配置，请耐心等待...";
            Enabled = false;

            await Task.Run(() =>
            {
                _vault = MFilesUtil.GetVaultByName();
                _initConfig = MfMailConfig.GetMailConfig(_vault);
            });

            Text = "邮箱设置";
            Enabled = true;

            //获取用户名和邮箱
            if (String.IsNullOrEmpty(_initConfig.UserName) ||
                String.IsNullOrEmpty(_initConfig.Email))
            {
                var name = String.Empty;
                var email = String.Empty;
                MfMailConfig.GetNameAndEmailFromVault(
                    _vault,
                    ref name,
                    ref email);
                _initConfig.UserName = name;
                _initConfig.Email = email;
            }

            if (String.IsNullOrEmpty(_initConfig.RecvAddr))
            {
                _initConfig.RecvAddr = "pop3.dbworld.com";
            }

            if (String.IsNullOrEmpty(_initConfig.SendAddr))
            {
                _initConfig.SendAddr = "smtp.dbworld.com";
            }

            //初始化UI
            txtBoxUserName.Text = _initConfig.UserName;
            txtBoxEmail.Text = _initConfig.Email;
            txtBoxPwd.Text = _initConfig.PassWord;
            txtBoxPopAddr.Text = _initConfig.RecvAddr;
            txtBoxSmtpAddr.Text = _initConfig.SendAddr;
            chkBoxPopSSL.Checked = _initConfig.RecvSSL;
            chkBoxSmtpSSL.Checked = _initConfig.SendSSL;
            txtBoxPopPort.Text = _initConfig.RecvPort.ToString();
            txtBoxSmtpPort.Text = _initConfig.SendPort.ToString();
            txtBoxMarkup.Text = _initConfig.MarkUp;
            txtBoxUserName.SelectionStart = txtBoxUserName.Text.Length;
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

        private void btnMarkUp_Click(object sender, EventArgs e)
        {
            txtBoxMarkup.Text = String.Format("[DBW]#{0}", _projId);
        }

        private MailCore.Common.MailConfig GetConfigFromUI()
        {
            return new MailCore.Common.MailConfig
            {
                UserName = txtBoxUserName.Text,
                Email = txtBoxEmail.Text,
                PassWord = txtBoxPwd.Text,
                RecvAddr = txtBoxPopAddr.Text,
                SendAddr = txtBoxSmtpAddr.Text,
                RecvSSL = chkBoxPopSSL.Checked,
                SendSSL = chkBoxSmtpSSL.Checked,
                RecvPort = Convert.ToInt32(txtBoxPopPort.Text),
                SendPort = Convert.ToInt32(txtBoxSmtpPort.Text),
                MarkUp = txtBoxMarkup.Text
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var config = GetConfigFromUI();

            if (config.Equals(_initConfig))
            {
                //未修改配置不需要保存
            }
            else
            {
                if (MfMailConfig.SetMailConfig(_vault, config))
                {
                    //保存配置失败
                }
            }

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //获取配置
            _currConfig = GetConfigFromUI();

            //测试链接
            _bkWorker.DoWork += BkWorkerOnDoWork;
            _bkWorker.RunWorkerCompleted += BkWorkerOnRunWorkerCompleted;
            if (!_bkWorker.IsBusy)
            {
                _bkWorker.RunWorkerAsync();
            }
        }

        private void chkBoxPopSSL_CheckedChanged(object sender, EventArgs e)
        {
            txtBoxPopPort.Text = chkBoxPopSSL.Checked ? MailCore.Common.MailConfig.PopSSLPort.ToString()
                : MailCore.Common.MailConfig.PopDefPort.ToString();
        }

        private void chkBoxSmtpSSL_CheckedChanged(object sender, EventArgs e)
        {
            txtBoxSmtpPort.Text = chkBoxSmtpSSL.Checked ? MailCore.Common.MailConfig.SmtpSSLPort.ToString() 
                : MailCore.Common.MailConfig.SmtpDefPort.ToString();
        }

        private void BkWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            //开始测试
            SetControlText(lblResult, "正在测试POP服务器...");
            var err = _currConfig.TestReceiveEmails();
            if (!String.IsNullOrEmpty(err))
            {
                SetControlText(lblResult, err);
                e.Result = false;
                return;
            }

            SetControlText(lblResult, "正在测试SMTP服务器...");
            try
            {
                SetControlText(lblResult, "正在发送测试邮件...");
                _currConfig.TestSendEmail();
            }
            catch (Exception ex)
            {
                SetControlText(lblResult, ex.Message);
                e.Result = false;
                return;
            }

            e.Result = true;
        }

        private void BkWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (bool)e.Result;
            if (result)
            {
                SetControlText(lblResult, "测试完成！");
            }
        }


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

        private void txtBoxPopPort_Validating(object sender, CancelEventArgs e)
        {
            var str = ((TextBox)sender).Text;

            if (!(Regex.IsMatch(str, DigitExpression)))
            {
                errorProvider1.SetError((Control) sender, "只能输入数字！");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError((Control)sender, null); 
            }
        }

        private void txtBoxSmtpPort_Validating(object sender, CancelEventArgs e)
        {
            var str = ((TextBox)sender).Text;

            if (!(Regex.IsMatch(str, DigitExpression)))
            {
                errorProvider2.SetError((Control)sender, "只能输入数字！");
                e.Cancel = true;
            }
            else
            {
                errorProvider2.SetError((Control)sender, null);
            }
        }

        private void txtBoxEmail_Validating(object sender, CancelEventArgs e)
        {
            var str = ((TextBox)sender).Text;

            if (!(Regex.IsMatch(str, Constants.EmailPattern)))
            {
                errorProvider3.SetError((Control)sender, "请输入有效的邮箱地址！");
                e.Cancel = true;
            }
            else
            {
                errorProvider3.SetError((Control)sender, null);
                ParseMailHost(str);
            }
        }

        private void txtBoxPwd_Validating(object sender, CancelEventArgs e)
        {
            var str = ((TextBox)sender).Text;

            if (String.IsNullOrEmpty(str))
            {
                errorProvider4.SetError((Control)sender, "密码不能为空！");
                e.Cancel = true;
            }
            else
            {
                errorProvider4.SetError((Control)sender, null);
            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(_webUrl))
            {
                MessageBox.Show(String.Format("网络地址({0})配置错误", _webUrl), "提示", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            var url = _webUrl + "/Helps/DBWorld邮箱设置指南.pdf";
            System.Diagnostics.Process.Start(url);
        }

        private void ParseMailHost(string addr)
        {
          
            Task.Factory.StartNew(() =>
            {
                int pos = addr.IndexOf('@');
                var host = addr.Substring(pos + 1, addr.Length - pos - 1);
                var smtpAddr = "smtp." + host;
                var popAddr = "pop." + host;

                //特殊处理hotmail
                if (SpecifyMailServer(host)) return;

                //ping smtp server
                if (PingMailServer(smtpAddr))
                {
                    SetControlText(txtBoxPopAddr, popAddr);
                    SetControlText(txtBoxSmtpAddr,smtpAddr);
                }
                else
                {
                    smtpAddr = "mail." + host;
                    popAddr = "mail." + host;
                    if (PingMailServer(smtpAddr))
                    {
                        SetControlText(txtBoxPopAddr, popAddr);
                        SetControlText(txtBoxSmtpAddr, smtpAddr);
                    }
                }
            });
        }

        /// <summary>
        /// 需要特殊处理的邮箱服务器地址
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private bool SpecifyMailServer(string host)
        {
            if (String.Compare(host, "hotmail.com",
                   StringComparison.OrdinalIgnoreCase) == 0)
            {
                SetControlText(txtBoxPopAddr, "pop-mail.outlook.com");
                SetControlText(txtBoxSmtpAddr, "smtp-mail.outlook.com");
                return true;
            }

            return false;
        }

        /// <summary>
        /// ping mail server
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private bool PingMailServer(string addr)
        {
            try
            {
                var ping = new Ping();
                var pingReply = ping.Send(addr);
                if (pingReply != null)
                {
                    if (pingReply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
