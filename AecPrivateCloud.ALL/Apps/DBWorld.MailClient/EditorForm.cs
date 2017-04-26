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
        /// ����ƶ�λ�ñ���  
        /// </summary>
        Point _mouseOff;

        /// <summary>
        /// ��ǩ�Ƿ�Ϊ��� 
        /// </summary>
        bool _leftFlag;

        /// <summary>
        /// M-Files����
        /// </summary>
        readonly Vault _vault = MailCore.MF.MFilesUtil.GetVaultByName();

        //M-Files����������Ϣ
        private MailConfig _config;

        /// <summary>
        /// �ʼ���Ϣ����
        /// </summary>
        private MailMessage _mMsg;

        /// <summary>
        /// ��������
        /// </summary>
        private readonly int _actionType = 0;

        /// <summary>
        /// MFiles���ʼ�����ID
        /// </summary>
        private readonly int _objId = -1;

        /// <summary>
        /// �ⲿ����·��
        /// </summary>
        private string _extAttachPath;

        /// <summary>
        /// ��ȡ�ݸ�GUID
        /// </summary>
        private string _guidDraft = String.Empty;

        /// <summary>
        /// ��̨�첽�߳�
        /// </summary>
        private BackgroundWorker _worker;

        /// <summary>
        /// ��ϵ��
        /// </summary>
        private IEnumerable<Linkman> _linkmans;

        /// <summary>
        /// �ⲿ��Ա
        /// </summary>
        private readonly List<Linkman> _external = new List<Linkman>();

        /// <summary>
        /// �ڲ���Ա
        /// </summary>
        private readonly List<Linkman> _internal = new List<Linkman>();

        /// <summary>
        /// �����ϵ��
        /// </summary>
        private List<Linkman> _recent = new List<Linkman>();

        /// <summary>
        /// ��¼ѡ�й����ı���
        /// </summary>
        private TextBox _txtBox;

        /// <summary>
        /// �ʼ���ַ������ʽ
        /// </summary>
        private const string EmailExpression = "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$";

        /// <summary>
        /// ��ϵ��������ʽ
        /// </summary>
        private const string EmailAddrExpression = "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+<(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))>$";

        /// <summary>
        /// mfiles���ɵ����ӱ��ʽ
        /// </summary>
        private const string MfLinkExpression = @"^([\s\S]*)(m-files://)(\w+/)(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})(/\S*\?object=)(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})([\s\S]*)$";

        public EditorForm(string[] args)
        {
            InitializeComponent();

            editor.Tick += new EditorCtrl.TickDelegate(editor_Tick);

            //ע�ᴰ���϶��¼�
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

            //Ĭ�ϼ�¼�ռ����ı������
            _txtBox = txtboxReceiver;
        }

        #region �����϶�

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseOff = new Point(-e.X, -e.Y); //�õ�������ֵ  
                _leftFlag = true;                  //����������ʱ��עΪtrue;  
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (_leftFlag)
            {
                var mouseSet = Control.MousePosition;
                mouseSet.Offset(_mouseOff.X, _mouseOff.Y);  //�����ƶ����λ��  
                this.Location = mouseSet;
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (_leftFlag)
            {
                _leftFlag = false;//�ͷ������עΪfalse;  
            }
        }

        #endregion

        #region �¼�����
        /// <summary>
        /// ���ڹ���
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
        /// �������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorForm_Load(object sender, EventArgs e)
        {
            //��ʼ���ʼ����ö���
            _config = MailCore.MF.MfMailConfig.GetMailConfig(_vault);

            if (!String.IsNullOrEmpty(_config.Email) && _config != null)
            {
                //������
                lblSender.Text = MailUtil.FormatToContacts(_config.UserName, _config.Email, true);
                txtboxSubject.Text = _config.MarkUp + "  ";
            }
            else
            {
                MessageBox.Show("δ��д�����������ַ������������δ������ȷ!");
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
                //bimvision ���ɵĸ���
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

            //ͨѶ¼����
            SetLinkmanToTreeView();
        }


        /// <summary>
        /// �����ʼ��ݸ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //�жϿ�����ʼ����ö����������
            if (_vault == null || _config == null || editor == null) return;

            try
            {
                _mMsg = new MailMessage();
                var consignee = CheckMails(txtboxReceiver.Text); //�ռ���ַת��
                var copySend = CheckMails(txtboxCC.Text); //���͵�ַת��

                _mMsg.Subject = txtboxSubject.Text.Trim(); //����

                _mMsg.From = new MailAddress(_config.Email, _config.UserName);

                //�ռ���ַ��
                foreach (var item in consignee)
                {
                    _mMsg.To.Add(new MailAddress(CheckContacts(item)));
                }

                //���͵�ַ��
                foreach (var item in copySend)
                {
                    _mMsg.CC.Add(CheckContacts(item));
                }

                //������
                foreach (var attachment in GetAttachments())
                {
                    _mMsg.Attachments.Add(attachment);
                }

                _mMsg.Body = editor.GetBodyText();

                if (_actionType == MailInfo.MAILTYPE_DARFT) //����ǲݸ���ģʽ�����ԭ�ļ�
                {
                    //�����ʼ�Ψһ��ʶ
                    _mMsg.Headers.Add("MessageId", _guidDraft);
                }
                else //�������½�һ���ݸ�
                {
                    //�����ʼ�Ψһ��ʶ 
                    _mMsg.Headers.Add("MessageId", System.Guid.NewGuid().ToString());
                }

                var isOk = MessageToMf.SaveSendMailToMf(_vault, _mMsg, MailCore.MF.MessageToMf.GetFolderIdByName(_vault, "�ݸ���")); ////д��MFils �ļ���ID3 �ݸ��� 
                if (isOk)
                {
                    MessageBox.Show("�ݸ屣��ɹ���");
                }
                else
                {
                    MessageBox.Show("�ݸ屣��ʧ�ܣ�");
                }

            }
            catch (Exception)
            {
                MessageBox.Show("�ݸ屣��ʧ�ܣ�");
            }
        }

        /// <summary>
        /// �����ʼ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblSend_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var recvAddr = CheckMails(txtboxReceiver.Text); //�ռ���ַת��
            var ccAddr = CheckMails(txtboxCC.Text);   //���͵�ַת��

            if (recvAddr == null || ccAddr == null) return;

            var content = editor.GetBodyText();
            if (IsContainsOutContacts() && Regex.IsMatch(content, MfLinkExpression))
            {
                var result = MessageBox.Show(
                    "�ռ��˰����ⲿ��Ա�������޷��鿴������DBWorld���ӣ�\r\n�Ƿ���������ʼ���",
                    "��ʾ",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result != DialogResult.OK)
                {
                    return;
                }
            }

            //�����ʼ�
            SendMail(_config,recvAddr, GetAttachments(), ccAddr, txtboxSubject.Text, editor.ToMailMessage(), _vault);
        }

        /// <summary>
        /// �˳�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// ��С��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkblMin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��������ַ�޳��ظ���ַ
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
                MessageBox.Show("��ַ�������顣");
            }

            return null;
        }

        /// <summary>
        /// ��ʽ���ʼ���ϵ��
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
        /// ɾ����ʱ�ļ�
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

        #region �ʼ�������
        /// <summary>
        /// �ı�������λ��
        /// </summary>
        /// <param name="point"></param>
        /// <param name="sender"></param>
        private void ChangeSeletion(int point, object sender)
        {
            var text = sender as TextBox;

            if (text.SelectionStart < point) text.SelectionStart = point;
        }

        /// <summary>
        /// �����ʱ�жϸı���λ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_MouseClick(object sender, MouseEventArgs e)
        {
            ChangeSeletion(_config.MarkUp.Length + 2, sender);
        }

        /// <summary>
        /// ���̧��ʱ�жϸı���λ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSubject_MouseUp(object sender, MouseEventArgs e)
        {
            ChangeSeletion(_config.MarkUp.Length + 2, sender);
        }

        /// <summary>
        /// ��ֹɾ����ǩ ����[DBW#111]
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
        /// �����λ�õ����ĩβʱ���������
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

        #region �ʼ���ַ��
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

        #region �����༭��
        /// <summary>
        /// �жϸ�����С
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsSize(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Length > 10 * 1024 * 1024)
            {
                MessageBox.Show("��������С���ܳ���10M");
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

                //���ø�����MIME��Ϣ
                ContentDisposition cd = attachment.ContentDisposition;
                cd.CreationDate = File.GetCreationTime(filePath); //���ø����Ĵ���ʱ��
                cd.ModificationDate = File.GetLastWriteTime(filePath); //���ø������޸�ʱ��
                cd.ReadDate = File.GetLastAccessTime(filePath); //���ø����ķ���ʱ��
                list.Add(attachment);
            }

            return list;
        }

        private void linklblAttach_Click(object sender, EventArgs e)
        {
            var openFileDlg = new OpenFileDialog
            {
                Title = "ѡ�񸽼�",
                Filter = "�����ļ�(*.*)|*.*|��ִ���ļ�(*.exe)|*.exe|�ĵ��ļ�(*.doc)|*.doc|�ı��ļ�(*.txt)|*.txt",
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

        #region ���븽��
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

        #region �ʼ�������MFiles����
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

        #region �༭ǩ��
        private void lnklblEditSign_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var signFrm = new SignEditor(_vault, _config);
            signFrm.Show();
        }
        #endregion

        #region �ʼ��༭��
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

        #region ������
        /// <summary>
        /// ��ʼ����������
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
        /// ���»س���ʱ���ʼ���ַ��׷�ӵ���ַ��
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

        #region ��ʾ��ϵ��

        /// <summary>
        /// ˫���ڵ㽲ѡ�нڵ��������丳ֵ���ռ���ַ
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
                MessageBox.Show("����ϵ��û���������䣡");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvLinkMan_AfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.Expand(); // չ��ѡ�еĽڵ�

            TreeNodeCollection c = (e.Node.Parent == null)
                ? this.tvLinkMan.Nodes
                : e.Node.Parent.Nodes; // ��ȡѡ�нڵ�ͬ�������нڵ�ļ��ϡ�����������Ҫ�жϣ��� e.Node.Parent == null ʱ��ʾ����Ľڵ㣬�ⲿ�ֽڵ�û�� Parent

            foreach (TreeNode node in c)
            {
                // ���Ƿ�ѡ������չ���Ľڵ���� Collapse �۵���
                if (node == e.Node) continue;
                if (node.IsExpanded)
                    node.ImageIndex = 0;
                node.Collapse();
            }
        }

        /// <summary>
        /// �����ڵ�ʱ���ýڵ���
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
        /// ����ȡ������ϵ����Ϣ���õ�TreeView
        /// </summary>
        private void SetLinkmanToTreeView()
        {
            try
            {
                //��ʼ��iamgelist
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
            //member.NodeFont = new Font("΢���ź�", 9, FontStyle.Bold);
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

            foreach (var item in _linkmans) //��������ϵ�˸��ݡ��ڲ��û�������������
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
            AddMemberToTreeview("�����ϵ��", _recent);
            AddMemberToTreeview("��Ŀ��Ա", _internal);
            AddMemberToTreeview("�ⲿ��Ա", _external);
            AddMemberToTreeview("������ϵ��", _linkmans);
            InitSearch(_linkmans);
        }
        #endregion

        #region �����ʼ�
        /// <summary>
        /// �����ʼ�
        /// </summary>
        /// <param name="config">����</param>
        /// <param name="consigneeAddress">�ռ���ַ����</param>
        /// <param name="subject">����</param>
        /// <param name="msg">�ʼ���Ϣ����</param>
        /// <param name="attachList">��������</param>
        /// <param name="copySend">�����˼���</param>
        /// <param name="vault"></param>
        private void SendMail(MailConfig config, IEnumerable<string> consigneeAddress,
            IEnumerable<Attachment> attachList, IEnumerable<string> copySend,
            string subject, MailMessage msg, Vault vault)
        {
            //��ʼ��SmtpClient
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
                    msg.From = new MailAddress(config.Email, config.UserName); //������ַ
                    msg.Subject = subject.Trim();//����

                    //�ռ���ַ��
                    foreach (var item in consigneeAddress)
                    {
                        msg.To.Add(new MailAddress(CheckContacts(item)));
                    }

                    //���͵�ַ��
                    foreach (var item in copySend)
                    {
                        msg.CC.Add(CheckContacts(item));
                    }

                    msg.Body = editor.DocumentText;

                    //������
                    foreach (var attachment in attachList)
                    {
                        msg.Attachments.Add(attachment);
                    }

                    if (_actionType != MailInfo.MAILTYPE_DARFT)
                    {
                        //�����ʼ�Ψһ��ʶ
                        msg.Headers.Add("MessageId", System.Guid.NewGuid().ToString());
                    }
                    else
                    {
                        if (_guidDraft == null) return;
                        //�ڲݸ���ģʽ�� ��ȡ�ʼ�Ψһ��ʶ
                        msg.Headers.Add("MessageId", _guidDraft);
                    }

                    //ע���첽���ͻص�����
                    client.SendCompleted += OnSendCompleted;
                    //�첽�����ʼ� 
                    client.SendAsync(msg, "ok");

                    this._mMsg = msg;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "�ʼ�����ʧ�ܣ�", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ���ʼ�������Ϻ�ʹ�øú�����ʾ�û�
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
            //    var msg = "�ʼ�����Ϊ��" + this._mMsg.Subject + "������ʧ�ܣ�\r\nʧ��ԭ��" + e.Error.Message;
            //    MessageBox.Show(msg);
            //}
            //else
            //{
                var isOk = MessageToMf.SaveSendMailToMf(_vault, this._mMsg, MailCore.MF.MessageToMf.GetFolderIdByName(_vault, "������"));
                if (isOk)
                {
                    MessageBox.Show("�ʼ�����Ϊ��" + this._mMsg.Subject + "�����ͳɹ���д��DBWorld�ɹ���");
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("�ʼ�����Ϊ��" + this._mMsg.Subject + "�����ͳɹ���д��DBWorldʧ�ܣ�");
                }
            //}
        }

        #endregion

    }
}