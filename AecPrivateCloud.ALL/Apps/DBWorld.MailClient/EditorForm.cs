using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using DBWorld.MailClient.Mail;
using DBWorld.MailClient.Util;
using DBWorld.MailCore;
using DBWorld.MailCore.Common;
using DBWorld.MailCore.MF;
using DBWorld.MailCore.Models;
using MFilesAPI;
using SimulaDesign.WPFPluginCore;

namespace DBWorld.MailClient
{
    public partial class EditorForm : Form
    {
        private string _filename = null;

        /// <summary>
        /// 鼠标移动位置变量  
        /// </summary>
        Point _mouseOff;

        /// <summary>
        /// 标签是否为左键 
        /// </summary>
        bool _leftFlag;

        /// <summary>
        /// M-Files对象
        /// </summary>
        readonly Vault _vault = MailCore.MF.MFilesUtil.GetVaultByName();

        //M-Files邮箱配置信息
        private MailConfig _config;

        /// <summary>
        /// 邮件消息对象
        /// </summary>
        private MailMessage _mMsg;

        /// <summary>
        /// 动作类型
        /// </summary>
        private readonly int _actionType = 0;

        /// <summary>
        /// MFiles中邮件对象ID
        /// </summary>
        private readonly int _objId = -1;

        /// <summary>
        /// 外部附件路径
        /// </summary>
        private string _extAttachPath;

        /// <summary>
        /// 获取草稿GUID
        /// </summary>
        private string _guidDraft = String.Empty;

        /// <summary>
        /// 后台异步线程
        /// </summary>
        private BackgroundWorker _worker;

        /// <summary>
        /// 联系人
        /// </summary>
        private IEnumerable<Linkman> _linkmans;

        /// <summary>
        /// 外部成员
        /// </summary>
        private readonly List<Linkman> _external = new List<Linkman>();

        /// <summary>
        /// 内部成员
        /// </summary>
        private readonly List<Linkman> _internal = new List<Linkman>();

        /// <summary>
        /// 最近联系人
        /// </summary>
        private List<Linkman> _recent = new List<Linkman>();

        /// <summary>
        /// 记录选中过的文本框
        /// </summary>
        private TextBox _txtBox;

        /// <summary>
        /// 邮件地址正则表达式
        /// </summary>
        private const string EmailExpression = "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$";

        /// <summary>
        /// 联系人正则表达式
        /// </summary>
        private const string EmailAddrExpression = "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+<(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))>$";

        /// <summary>
        /// mfiles生成的链接表达式
        /// </summary>
        private const string MfLinkExpression = @"^([\s\S]*)(m-files://)(\w+/)(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})(/\S*\?object=)(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})([\s\S]*)$";

        public EditorForm(string[] args)
        {
            InitializeComponent();

            editor.Tick += new EditorCtrl.TickDelegate(editor_Tick);

            //注册窗体拖动事件
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(Form_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(Form_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(Form_MouseUp);

            if (!String.IsNullOrEmpty(args[2]))
            {
                _actionType = Convert.ToInt32(args[2]);
                if (_actionType == MailInfo.MAILTYPE_EXT)
                {
                    _extAttachPath = args[4];
                }
            }

            if (!String.IsNullOrEmpty(args[3]))
            {
                _objId = Convert.ToInt32(args[3]);
            }

            //默认记录收件人文本框对象
            _txtBox = txtboxReceiver;
        }

        #region 窗体拖动

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseOff = new Point(-e.X, -e.Y); //得到变量的值  
                _leftFlag = true;                  //点击左键按下时标注为true;  
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (_leftFlag)
            {
                var mouseSet = Control.MousePosition;
                mouseSet.Offset(_mouseOff.X, _mouseOff.Y);  //设置移动后的位置  
                this.Location = mouseSet;
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (_leftFlag)
            {
                _leftFlag = false;//释放鼠标后标注为false;  
            }
        }

        #endregion

        #region 事件函数
        /// <summary>
        /// 窗口过程
        /// </summary>
        /// <param name="m"></param>
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

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorForm_Load(object sender, EventArgs e)
        {
            //初始化邮件配置对象
            _config = MailCore.MF.MfMailConfig.GetMailConfig(_vault);

            if (!String.IsNullOrEmpty(_config.Email) && _config != null)
            {
                //发件人
                lblSender.Text = MailUtil.FormatToContacts(_config.UserName, _config.Email, true);
                txtboxSubject.Text = _config.MarkUp + "  ";
            }
            else
            {
                MessageBox.Show("未填写发件人邮箱地址，或邮箱设置未设置正确!");
                this.Close();
            }

            var mfMail = MailCore.MF.MessageFromMf.GetMailFromMf(_vault, _objId);
            _guidDraft = mfMail.Tag;

            var obj = MailFactory.CreateInstance(_actionType);
            obj.MailContext = mfMail;
            obj.MsgConfig = _config;
            var context = obj.GetMailContext();

            if (!String.IsNullOrEmpty(_extAttachPath))
            {
                //bimvision 生成的附件
                context.AttachsPath.Add(_extAttachPath);
            }

            txtboxSubject.Text += context.Subject;
            lblSender.Text = context.Sender;
            txtboxReceiver.Text = context.Recivers;
            txtboxCC.Text = context.CC;
            if (context.AttachsPath != null)
            {
                foreach (var path in context.AttachsPath)
                {
                    attachCtrl.AddAttachmentPath(path);
                }
            }
            editor.DocumentText = context.Content;

            //通讯录数据
            SetLinkmanToTreeView();
        }


        /// <summary>
        /// 保存邮件草稿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //判断库对象、邮件配置对象、邮箱对象
            if (_vault == null || _config == null || editor == null) return;

            try
            {
                _mMsg = new MailMessage();
                var consignee = CheckMails(txtboxReceiver.Text); //收件地址转换
                var copySend = CheckMails(txtboxCC.Text); //抄送地址转换

                _mMsg.Subject = txtboxSubject.Text.Trim(); //标题

                _mMsg.From = new MailAddress(_config.Email, _config.UserName);

                //收件地址集
                foreach (var item in consignee)
                {
                    _mMsg.To.Add(new MailAddress(CheckContacts(item)));
                }

                //抄送地址集
                foreach (var item in copySend)
                {
                    _mMsg.CC.Add(CheckContacts(item));
                }

                //附件集
                foreach (var attachment in GetAttachments())
                {
                    _mMsg.Attachments.Add(attachment);
                }

                _mMsg.Body = editor.GetBodyText();

                if (_actionType == MailInfo.MAILTYPE_DARFT) //如果是草稿箱模式则更改原文件
                {
                    //增加邮件唯一标识
                    _mMsg.Headers.Add("MessageId", _guidDraft);
                }
                else //否则重新建一个草稿
                {
                    //增加邮件唯一标识 
                    _mMsg.Headers.Add("MessageId", System.Guid.NewGuid().ToString());
                }

                var isOk = MessageToMf.SaveSendMailToMf(_vault, _mMsg, MailCore.MF.MessageToMf.GetFolderIdByName(_vault, "草稿箱")); ////写入MFils 文件夹ID3 草稿箱 
                if (isOk)
                {
                    MessageBox.Show("草稿保存成功！");
                }
                else
                {
                    MessageBox.Show("草稿保存失败！");
                }

            }
            catch (Exception)
            {
                MessageBox.Show("草稿保存失败！");
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblSend_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var recvAddr = CheckMails(txtboxReceiver.Text); //收件地址转换
            var ccAddr = CheckMails(txtboxCC.Text);   //抄送地址转换

            if (recvAddr == null || ccAddr == null) return;

            var content = editor.GetBodyText();
            if (IsContainsOutContacts() && Regex.IsMatch(content, MfLinkExpression))
            {
                var result = MessageBox.Show(
                    "收件人包含外部成员，可能无法查看正文中DBWorld链接，\r\n是否继续发送邮件？",
                    "提示",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result != DialogResult.OK)
                {
                    return;
                }
            }

            //发送邮件
            SendMail(_config,recvAddr, GetAttachments(), ccAddr, txtboxSubject.Text, editor.ToMailMessage(), _vault);
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblMin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion

        #region 操作函数
        /// <summary>
        /// 检查邮箱地址剔除重复地址
        /// </summary>
        private IEnumerable<string> CheckMails(string mails)
        {
            try
            {
                var mail = mails.Split(';');
                var list = mail.ToList();
                list.Remove("");
                return list;
            }
            catch (Exception)
            {
                MessageBox.Show("地址有误！请检查。");
            }

            return null;
        }

        /// <summary>
        /// 格式化邮件联系人
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string CheckContacts(string value)
        {
            try
            {
                if (!Regex.IsMatch(value, EmailAddrExpression))
                {
                    if (Regex.IsMatch(value, EmailExpression))
                    {
                        var str = value.Substring(0, value.IndexOf('@'));
                        return MailUtil.FormatToContacts(str,value, false);
                    }
                    else
                    {
                        return value;
                    }
                }
            }
            catch (Exception)
            {

            }

            return value;
        }

        /// <summary>
        /// 删除临时文件
        /// </summary>
        /// <returns></returns>
        private void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region 邮件主题栏
        /// <summary>
        /// 改变光标所在位置
        /// </summary>
        /// <param name="point"></param>
        /// <param name="sender"></param>
        private void ChangeSeletion(int point, object sender)
        {
            var text = sender as TextBox;

            if (text.SelectionStart < point) text.SelectionStart = point;
        }

        /// <summary>
        /// 鼠标点击时判断改变光标位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_MouseClick(object sender, MouseEventArgs e)
        {
            ChangeSeletion(_config.MarkUp.Length + 2, sender);
        }

        /// <summary>
        /// 鼠标抬起时判断改变光标位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_MouseUp(object sender, MouseEventArgs e)
        {
            ChangeSeletion(_config.MarkUp.Length + 2, sender);
        }

        /// <summary>
        /// 禁止删除标签 例：[DBW#111]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_KeyPress(object sender, KeyPressEventArgs e)
        {
            var text = sender as TextBox;
            if (e.KeyChar == (char)Keys.Back)
            {
                if (text.SelectionStart == _config.MarkUp.Length + 2)
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 当光标位置到标记末尾时禁用左方向键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_KeyDown(object sender, KeyEventArgs e)
        {
            var text = sender as TextBox;
            if (e.KeyCode == Keys.Left)
            {
                if (text.SelectionStart == _config.MarkUp.Length + 2)
                {
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region 邮件地址栏
        private void txtConsignee_Click(object sender, EventArgs e)
        {
            _txtBox = sender as TextBox;
            var address = new EmailAddressUtil(txtboxReceiver.Text, ';', txtboxReceiver.SelectionStart);
            int nStart = 0, nLen = 0;
            address.GetSelectedItemSection(ref nStart, ref nLen);
            if (txtboxReceiver.TextLength != txtboxReceiver.SelectionStart)
            {
                txtboxReceiver.SelectionStart = nStart;
                txtboxReceiver.SelectionLength = nLen;
            }
        }

        private void txtCopySend_Click(object sender, EventArgs e)
        {
            _txtBox = sender as TextBox;
            var address = new EmailAddressUtil(txtboxCC.Text, ';', txtboxCC.SelectionStart);
            int nStart = 0, nLen = 0;
            address.GetSelectedItemSection(ref nStart, ref nLen);
            if (txtboxCC.TextLength != txtboxCC.SelectionStart)
            {
                txtboxCC.SelectionStart = nStart;
                txtboxCC.SelectionLength = nLen;
            }
        }

        private void txtConsignee_KeyDown(object sender, KeyEventArgs e)
        {
            var ctrl = sender as TextBox;

            if (e.KeyData == Keys.Delete)
            {
                if (ctrl.SelectionLength == 0)
                {
                    e.Handled = true;

                    if (ctrl.Text.Length == ctrl.SelectionStart)
                    {
                        return;
                    }

                    var nLen = ctrl.Text.Length;
                    var address = new EmailAddressUtil(ctrl.Text, ';', ctrl.SelectionStart);
                    var itemIndex = address.GetSelectedItemIndex();
                    var subAddress = address.DeleteSelectedItemAt(itemIndex);

                    ctrl.Text = subAddress.Str;
                    ctrl.SelectionStart = subAddress.Pos;
                    ctrl.SelectionLength = 0;
                }
            }
        }

        private void txtConsignee_KeyPress(object sender, KeyPressEventArgs e)
        {
            var ctrl = sender as TextBox;

            if (e.KeyChar == (char)Keys.Back)
            {
                if (ctrl.SelectionLength == 0)
                {
                    e.Handled = true;

                    var address = new EmailAddressUtil(ctrl.Text, ';', ctrl.SelectionStart);
                    var itemIndex = address.GetSelectedItemIndex();
                    var subAddress = address.BackSelectedItemAt(itemIndex);

                    ctrl.Text = subAddress.Str;
                    ctrl.SelectionStart = subAddress.Pos;
                    ctrl.SelectionLength = 0;
                }
            }
        }

        private void txtCopySend_KeyDown(object sender, KeyEventArgs e)
        {
            var ctrl = sender as TextBox;

            if (e.KeyData == Keys.Delete)
            {
                if (ctrl.SelectionLength == 0)
                {
                    e.Handled = true;

                    if (ctrl.Text.Length == ctrl.SelectionStart)
                    {
                        return;
                    }

                    var nLen = ctrl.Text.Length;
                    var address = new EmailAddressUtil(ctrl.Text, ';', ctrl.SelectionStart);
                    var itemIndex = address.GetSelectedItemIndex();
                    var subAddress = address.DeleteSelectedItemAt(itemIndex);

                    ctrl.Text = subAddress.Str;
                    ctrl.SelectionStart = subAddress.Pos;
                    ctrl.SelectionLength = 0;
                }
            }
        }

        private void txtCopySend_KeyPress(object sender, KeyPressEventArgs e)
        {
            var ctrl = sender as TextBox;

            if (e.KeyChar == (char)Keys.Back)
            {
                if (ctrl.SelectionLength == 0)
                {
                    e.Handled = true;

                    var address = new EmailAddressUtil(ctrl.Text, ';', ctrl.SelectionStart);
                    var itemIndex = address.GetSelectedItemIndex();
                    var subAddress = address.BackSelectedItemAt(itemIndex);

                    ctrl.Text = subAddress.Str;
                    ctrl.SelectionStart = subAddress.Pos;
                    ctrl.SelectionLength = 0;
                }
            }
        }

        private void txtConsignee_DoubleClick(object sender, EventArgs e)
        {
            var ctrl = sender as TextBox;
            ctrl.SelectionStart = 0;
            ctrl.SelectionLength = ctrl.TextLength;
        }

        private void txtCopySend_DoubleClick(object sender, EventArgs e)
        {
            var ctrl = sender as TextBox;
            ctrl.SelectionStart = 0;
            ctrl.SelectionLength = ctrl.TextLength;
        }


        #endregion

        #region 附件编辑框
        /// <summary>
        /// 判断附件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsSize(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Length > 10 * 1024 * 1024)
            {
                MessageBox.Show("附件不大小不能超过10M");
            }
            else
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add email attachments.
        /// </summary>
        private IEnumerable<Attachment> GetAttachments()
        {
            var list = new List<Attachment>();
            foreach (var filePath in this.attachCtrl.GetAttachmentsPath())
            {
                if (String.IsNullOrEmpty(filePath)) continue;
                var extName = Path.GetExtension(filePath).ToLower();
                Attachment attachment;
                if (extName == ".rar" || extName == ".zip")
                {
                    attachment = new Attachment(filePath, MediaTypeNames.Application.Zip);
                }
                else
                {
                    attachment = new Attachment(filePath, MediaTypeNames.Application.Octet);
                }

                //设置附件的MIME信息
                ContentDisposition cd = attachment.ContentDisposition;
                cd.CreationDate = File.GetCreationTime(filePath); //设置附件的创建时间
                cd.ModificationDate = File.GetLastWriteTime(filePath); //设置附件的修改时间
                cd.ReadDate = File.GetLastAccessTime(filePath); //设置附件的访问时间
                list.Add(attachment);
            }

            return list;
        }

        private void linklblAttach_Click(object sender, EventArgs e)
        {
            var openFileDlg = new OpenFileDialog
            {
                Title = "选择附件",
                Filter = "所有文件(*.*)|*.*|可执行文件(*.exe)|*.exe|文档文件(*.doc)|*.doc|文本文件(*.txt)|*.txt",
                Multiselect = true,
                CheckFileExists = true,
                ValidateNames = true,
                FileName = string.Empty,
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var path in openFileDlg.FileNames)
                {
                    if (this.IsSize(path))
                    {
                        attachCtrl.AddAttachmentPath(path);
                    }
                }
            }
        }

        #endregion

        #region 拖入附件
        private void EditorForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        }

        private void EditorForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (this.IsSize(file))
                    {
                        attachCtrl.AddAttachmentPath(file);
                    }
                }
            }
        }

        private void editor_WbNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.IsFile)
            {
                e.Cancel = true;
                attachCtrl.AddAttachmentPath(e.Url.LocalPath);
            }
        }

        #endregion

        #region 邮件正文中MFiles链接
        private bool IsContainsOutContacts()
        {
            var str = txtboxReceiver.Text + txtboxCC.Text;
            if (_linkmans == null)
            {
                return false;
            }

            foreach (var item in _linkmans)
            {
                if (String.IsNullOrEmpty(item.InnerUser))
                {
                    if (str.Contains(item.Mail))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region 编辑签名
        private void lnklblEditSign_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var signFrm = new SignEditor(_vault, _config);
            signFrm.Show();
        }
        #endregion

        #region 邮件编辑区
        private void editor_Tick()
        {
            undoToolStripMenuItem.Enabled = editor.CanUndo();
            redoToolStripMenuItem.Enabled = editor.CanRedo();
            cutToolStripMenuItem.Enabled = editor.CanCut();
            copyToolStripMenuItem.Enabled = editor.CanCopy();
            pasteToolStripMenuItem.Enabled = editor.CanPaste();
            imageToolStripMenuItem.Enabled = editor.CanInsertLink();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _filename = null;
            Text = null;
            editor.BodyHtml = string.Empty;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_filename == null)
            {
                if (!SaveFileDialog())
                    return;
            }
            SaveFile(_filename);
        }

        private bool SaveFileDialog()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.DefaultExt = "htm";
                dlg.Filter = "HTML files (*.html;*.htm)|*.html;*.htm";
                DialogResult res = dlg.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    _filename = dlg.FileName;
                    return true;
                }
                else
                    return false;
            }
        }

        private void SaveFile(string filename)
        {
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.Write(editor.DocumentText);
                writer.Flush();
                writer.Close();
            }
        }

        private void LoadFile(string filename)
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                editor.Html = reader.ReadToEnd();
                Text = editor.DocumentTitle;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "HTML files (*.html;*.htm)|*.html;*.htm";
                DialogResult res = dlg.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    _filename = dlg.FileName;
                }
                else
                    return;
            }
            LoadFile(_filename);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveFileDialog())
                SaveFile(_filename);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SearchDialog dlg = new SearchDialog(editor))
            {
                dlg.ShowDialog(this);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.SelectAll();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Paste();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Redo();
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, editor.BodyText);
        }

        private void htmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, editor.BodyHtml);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Print();
        }

        private void breakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.InsertBreak();
        }

        private void textToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (TextInsertForm form = new TextInsertForm(editor.BodyHtml))
            {
                form.ShowDialog(this);
                if (form.Accepted)
                {
                    editor.BodyHtml = form.HTML;
                }
            }
        }

        private void paragraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.InsertParagraph();
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.InsertImage();
        }

        private void emailToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        #endregion

        #region 搜索栏
        /// <summary>
        /// 初始化搜索设置
        /// </summary>
        private void InitSearch(IEnumerable<Linkman> linkmen)
        {
            if (!linkmen.Any()) return;
            // declare custom source.
            var source = new AutoCompleteStringCollection();

            foreach (var t in linkmen)
            {
                source.Add(MailUtil.FormatToContacts(t.Name, t.Mail, true));
            }

            txtSearch.AutoCompleteCustomSource = source;
            txtSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        /// <summary>
        /// 按下回车键时将邮件地址添追加到地址栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                var search = sender as TextBox;
                if (!string.IsNullOrEmpty(search.Text))
                {
                    _txtBox.Text += search.Text.Trim();
                }
            }
        }

        #endregion

        #region 显示联系人

        /// <summary>
        /// 双击节点讲选中节点对象的邮箱赋值给收件地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvLinkMan_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var currentNode = (sender as TreeView).SelectedNode;
            var currentNodeCount = currentNode.GetNodeCount(false);
            var nodeTag = (sender as TreeView).SelectedNode.Tag;
            try
            {
                if (currentNodeCount == 0)
                {
                    var lnkmanObj = (nodeTag as Linkman);
                    _txtBox.Text += MailUtil.FormatToContacts(lnkmanObj.Name, lnkmanObj.Mail, true);
                }
            }
            catch (NullReferenceException a)
            {
                _txtBox.Text += nodeTag + ";";
            }
            catch (Exception exception)
            {
                MessageBox.Show("该联系人没有设置邮箱！");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvLinkMan_AfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.Expand(); // 展开选中的节点

            TreeNodeCollection c = (e.Node.Parent == null)
                ? this.tvLinkMan.Nodes
                : e.Node.Parent.Nodes; // 获取选中节点同级的所有节点的集合。这里这所以要判断，因 e.Node.Parent == null 时表示最顶级的节点，这部分节点没有 Parent

            foreach (TreeNode node in c)
            {
                // 若是非选中且已展开的节点调用 Collapse 折叠。
                if (node == e.Node) continue;
                if (node.IsExpanded)
                    node.ImageIndex = 0;
                node.Collapse();
            }
        }

        /// <summary>
        /// 单击节点时当该节点是
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvLinkMan_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var currentNode = 0;
                currentNode = e.Node.GetNodeCount(false);
                if (currentNode == 0)
                {
                    e.Node.Parent.ImageIndex = 1;
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 将获取到的联系人信息设置到TreeView
        /// </summary>
        private void SetLinkmanToTreeView()
        {
            try
            {
                //初始化iamgelist
                var myImageList = new ImageList();
                myImageList.Images.Add(Image.FromFile(@"Icon/tree_close.png"));
                myImageList.Images.Add(Image.FromFile(@"Icon/tree_open.png"));
                this.tvLinkMan.ImageList = myImageList;

                _worker = new BackgroundWorker();
                _worker.DoWork += OnDoWork;
                _worker.RunWorkerCompleted += OnRunWorkerCompleted;
                _worker.RunWorkerAsync();
            }
            catch (Exception)
            {
            }
        }

        private void AddMemberToTreeview(string nodeName, IEnumerable<Linkman> linkmen)
        {
            if (!linkmen.Any()) return;
            var member = new TreeNode(nodeName + "(" + linkmen.Count() + ")");
            member.SelectedImageIndex = 1;
            //member.NodeFont = new Font("微软雅黑", 9, FontStyle.Bold);
            TreeNode tmepNode;
            foreach (var item in linkmen)
            {
                tmepNode = new TreeNode();
                tmepNode.SelectedImageIndex = 3;
                tmepNode.ImageIndex = 3;
                if (string.IsNullOrEmpty(item.InnerUser) && string.IsNullOrEmpty(item.Name))
                {
                    tmepNode.Tag = item.Mail;
                    tmepNode.Text = item.Mail;
                }
                else if (!string.IsNullOrEmpty(item.Mail))
                {
                    tmepNode.Tag = item;
                    tmepNode.Text = MailUtil.FormatToContacts(item.Name, item.Mail, false);
                }

                member.Nodes.Add(tmepNode);
            }

            tvLinkMan.Nodes.Add(member);
        }

        void OnDoWork(object sender, DoWorkEventArgs e)
        {
            _linkmans = LinkmanByMf.GetLinkman(_vault);
            _recent = LinkmanByMf.SearchSendmailObj(_vault, 4);
        }

        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!_linkmans.Any()) return;

            foreach (var item in _linkmans) //将所有联系人根据“内部用户”属性来区分
            {
                if (string.IsNullOrEmpty(item.InnerUser))
                {
                    _external.Add(item);
                }
                else
                {
                    _internal.Add(item);
                }
            }
            AddMemberToTreeview("最近联系人", _recent);
            AddMemberToTreeview("项目成员", _internal);
            AddMemberToTreeview("外部成员", _external);
            AddMemberToTreeview("所有联系人", _linkmans);
            InitSearch(_linkmans);
        }
        #endregion

        #region 发送邮件
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="config">配置</param>
        /// <param name="consigneeAddress">收件地址集合</param>
        /// <param name="subject">标题</param>
        /// <param name="msg">邮件信息对象</param>
        /// <param name="attachList">附件集合</param>
        /// <param name="copySend">抄送人集合</param>
        /// <param name="vault"></param>
        private void SendMail(MailConfig config, IEnumerable<string> consigneeAddress,
            IEnumerable<Attachment> attachList, IEnumerable<string> copySend,
            string subject, MailMessage msg, Vault vault)
        {
            //初始化SmtpClient
            var client = new SmtpClient
            {
                Host = config.SendAddr,
                Port = config.SendPort,
                Credentials = new NetworkCredential(config.Email, config.PassWord),
                EnableSsl = config.SendSSL
            };

            try
            {
                if (msg != null)
                {
                    msg.From = new MailAddress(config.Email, config.UserName); //发件地址
                    msg.Subject = subject.Trim();//标题

                    //收件地址集
                    foreach (var item in consigneeAddress)
                    {
                        msg.To.Add(new MailAddress(CheckContacts(item)));
                    }

                    //抄送地址集
                    foreach (var item in copySend)
                    {
                        msg.CC.Add(CheckContacts(item));
                    }

                    msg.Body = editor.DocumentText;

                    //附件集
                    foreach (var attachment in attachList)
                    {
                        msg.Attachments.Add(attachment);
                    }

                    if (_actionType != MailInfo.MAILTYPE_DARFT)
                    {
                        //增加邮件唯一标识
                        msg.Headers.Add("MessageId", System.Guid.NewGuid().ToString());
                    }
                    else
                    {
                        if (_guidDraft == null) return;
                        //在草稿箱模式下 获取邮件唯一标识
                        msg.Headers.Add("MessageId", _guidDraft);
                    }

                    //注册异步发送回调函数
                    client.SendCompleted += OnSendCompleted;
                    //异步发送邮件 
                    client.SendAsync(msg, "ok");

                    this._mMsg = msg;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "邮件发送失败！", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 当邮件发送完毕后，使用该函数提示用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //if (e.Cancelled)
            //{
                
            //}
            //if (e.Error != null)
            //{
            //    var msg = "邮件主题为【" + this._mMsg.Subject + "】发送失败！\r\n失败原因：" + e.Error.Message;
            //    MessageBox.Show(msg);
            //}
            //else
            //{
                var isOk = MessageToMf.SaveSendMailToMf(_vault, this._mMsg, MailCore.MF.MessageToMf.GetFolderIdByName(_vault, "发件箱"));
                if (isOk)
                {
                    MessageBox.Show("邮件主题为【" + this._mMsg.Subject + "】发送成功，写入DBWorld成功！");
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("邮件主题为【" + this._mMsg.Subject + "】发送成功，写入DBWorld失败！");
                }
            //}
        }

        #endregion

    }
}