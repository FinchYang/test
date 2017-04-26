using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AecCloud.WebAPI.Models;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Notification.Client;

namespace SoftwareRelease
{
    public partial class Form1 : Form
    {
        private LocalSettings _localSettings = new LocalSettings();
        public Form1()
        {
            InitializeComponent();
            if (LocalSettings.Isconfexists())
            {
                _localSettings = LocalSettings.Load();
            }
            else
            {
                _localSettings.Save();
            }
            textBoxversion.Text = _localSettings.Version;
            textBoxwebpath.Text = _localSettings.WebPath;
            textBoxweburl.Text = _localSettings.Weburl;
            textBoxapppath.Text = _localSettings.AppPath;
            textBoxappver.Text = _localSettings.AppVersion;
            textBoxguid.Text = _localSettings.AppGuid;
        }
        private static IHubProxy HubProxy { get; set; }
        private static HubConnection Connection { get; set; }
        private async void buttonnotice_Click(object sender, EventArgs e)
        {
            try
            {
                var ret = new UpdateInfo();
                try
                {
                    var ppath = System.IO.Path.Combine(textBoxwebpath.Text, "installer");
                    richTextBoxlog.AppendText(Environment.NewLine + "升级文件目录:" + ppath);
                    var di = new DirectoryInfo(ppath);
                    var files=di.GetFiles();
                  //  richTextBoxlog.AppendText(Environment.NewLine + "textBoxversion:" + textBoxversion.Text);
                    var releaseversion = long.Parse(textBoxversion.Text.Replace(".", ""));
                 //   richTextBoxlog.AppendText(Environment.NewLine + "releaseversion:" + releaseversion);
                    foreach (FileInfo fileInfo in files)
                    {
                     //   richTextBoxlog.AppendText(Environment.NewLine + "aaa:" + releaseversion);
                        if (fileInfo.Name.Contains("NoticeSetup"))
                        {
                        //    richTextBoxlog.AppendText(Environment.NewLine + "bbb:" + releaseversion);
                            try
                            {
                                var tmp = fileInfo.Name.Replace(".", "");
                                var reg = new Regex(@"\d+");
                                var m = reg.Match(tmp).ToString();
                                //   richTextBoxlog.AppendText(Environment.NewLine + "ccc:" + m);
                                if (long.Parse(m) >= releaseversion)
                                {
                                    //   richTextBoxlog.AppendText(Environment.NewLine + "ddd:" + releaseversion);
                                    ret.Name = fileInfo.Name;
                                    ret.Date = fileInfo.CreationTime.ToLocalTime().ToString("F");
                                 //   ret.FileContent = File.ReadAllBytes(fileInfo.FullName);
                                  //  richTextBoxlog.AppendText(Environment.NewLine + "eee:" + releaseversion);
                                    break;
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    richTextBoxlog.AppendText(Environment.NewLine + string.Format("file operation, error:{0},{1},{2},{3}",
                        textBoxversion.Text,textBoxweburl.Text,textBoxwebpath.Text, ex.Message));
                    return;
                }
                if (ret.Name == string.Empty)
                {
                    richTextBoxlog.AppendText(Environment.NewLine + "没有找到此版本及更高版本." + textBoxversion.Text);
                    return;
                }
                richTextBoxlog.AppendText(Environment.NewLine + "PushNoticeUpdatePackage. " + " before invoke.");
               await  HubProxy.Invoke("PushNoticeUpdatePackage", ret);
                 richTextBoxlog.AppendText(Environment.NewLine + "PushNoticeUpdatePackage." + " ok.");
            }
            catch (Exception ex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + "PushNoticeUpdatePackage." + ex.Message);
            }
        }

        private void textBoxversion_TextChanged(object sender, EventArgs e)
        {
            _localSettings.Version = textBoxversion.Text;
        }

        private void textBoxweburl_TextChanged(object sender, EventArgs e)
        {
            _localSettings.Weburl = textBoxweburl.Text;
        }

        private void textBoxwebpath_TextChanged(object sender, EventArgs e)
        {
            _localSettings.WebPath = textBoxwebpath.Text;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Connection = new HubConnection(textBoxweburl.Text);
            
            HubProxy = Connection.CreateHubProxy("CscecPushHub");
            try
            {
                richTextBoxlog.AppendText(Environment.NewLine + "before await Connection.Start();" + textBoxweburl.Text);
                await Connection.Start();
                richTextBoxlog.AppendText(Environment.NewLine + "after connected;" + Connection.Protocol);
            }
            catch (Exception ex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + "Unable to connect to server: Start server before connecting clients." + textBoxweburl.Text + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _localSettings.Save();
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

        private async void buttonapp_Click(object sender, EventArgs e)
        {
            try
            {
                var appver = textBoxappver.Text;
                var ret = new VaultAppModel
            {
                AppId = 1,
                Guid = textBoxguid.Text,
                IsUpdate = true,
                Version =appver
            };
          
                //try
                //{
                //    var di = new DirectoryInfo(textBoxapppath.Text);
                //    var files = di.GetFiles();
                //    foreach (FileInfo fileInfo in files)
                //    {
                //        if (fileInfo.Name.Contains(textBoxappver.Text))
                //        {
                //            var apppath = Path.Combine(textBoxapppath.Text, string.Format("1-{0}.zip", appver));
                //            try
                //            {
                               
                //                ret.ZipFile = File.ReadAllBytes(apppath);
                //            }
                //            catch (Exception ex)
                //            {
                //                richTextBoxlog.AppendText(Environment.NewLine + apppath + ex.Message);
                //                return;
                //            }
                //                break;
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    richTextBoxlog.AppendText(Environment.NewLine + string.Format("app release error:{0},{1},{2},{3}",
                //        textBoxappver.Text, textBoxweburl.Text, textBoxapppath.Text, ex.Message));
                //    return;
                //}
                richTextBoxlog.AppendText(Environment.NewLine + "PushNewApp. " + " before invoke.");
                await HubProxy.Invoke("PushNewApp", ret);
                richTextBoxlog.AppendText(Environment.NewLine + "PushNewApp." +" ok.");
            }
            catch (Exception ex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + "PushNewApp." + ex.Message);
            }
        }

        private void textBoxappver_TextChanged(object sender, EventArgs e)
        {
            _localSettings.AppVersion = textBoxappver.Text;
        }

        private void textBoxapppath_TextChanged(object sender, EventArgs e)
        {
            _localSettings.AppPath = textBoxapppath.Text;
        }

        private void textBoxguid_TextChanged(object sender, EventArgs e)
        {
            _localSettings.AppGuid = textBoxguid.Text;
        }
    }
}
