using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MFilesAPI;
namespace Notification.Client
{
    public partial class NoticeSet : Form
    {
        public class VaultConf
        {
            public string Vaultname;
            public bool IsConcerned;
        }
        private SeismicControlSettings _scsconf = new SeismicControlSettings();
        public List<VaultConf> VaultConfs = new List<VaultConf>();
      //  private readonly string[] _noticetypes = { "新建任务", "工作流指派", "新建文档", "删除文档", "删除其它", "新建其它", "更新文档", "更新其它", "其它消息" };
        private readonly string[] _noticetypes = { "新建任务", "工作流指派", "任务更新", "任务完成" };
        public NoticeSet()
        {
            InitializeComponent();
        }

        private void treeViewConcern_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void NoticeSet_Load(object sender, EventArgs e)
        {
            treeViewConcern.Nodes.Clear();
            var app = new MFilesClientApplication();
            var vcs = app.GetVaultConnections();
            var all = new TreeNode("全部");
            _scsconf = SeismicControlSettings.Load();
            foreach (VaultConnection vc in vcs)
            {
                var onevault = new TreeNode(vc.Name);

                foreach (var a in _noticetypes)
                {
                    onevault.Nodes.Add(a);
                }
                all.Nodes.Add(onevault);
            }
            treeViewConcern.CheckBoxes = true;
            treeViewConcern.Nodes.Add(all);
            foreach (TreeNode b in treeViewConcern.Nodes)
            {
                b.Expand();
            }
            
            foreach (TreeNode tn in treeViewConcern.Nodes)
            {
                tn.Checked = true;
                foreach (TreeNode stn in tn.Nodes)
                {
                    stn.Checked = InitFromConf(_scsconf, stn.Text);
                    VaultConfs.Add(new VaultConf { Vaultname = stn.Text, IsConcerned = stn.Checked });
                    foreach (TreeNode sstn in stn.Nodes)
                    {
                        sstn.Checked = InitFromConf(_scsconf, stn.Text, sstn.Text); 
                    }
                }
            }
        }
        private bool InitFromConf(SeismicControlSettings conf, string text, string sontext)
        {
            foreach (var node in conf.Tnodes)
            {
                if (node.Name == text)
                {
                    foreach (var snode in node.Tnodes)
                    {
                        if (snode.Name == sontext)
                            return snode.Nchecked;
                    }
                }
            }
            return false;
        }

        private bool InitFromConf(SeismicControlSettings conf, string text)
        {
            foreach (var node in conf.Tnodes)
            {
                if (node.Name == text) return node.Nchecked;
            }
            return false;
        }
        /// <summary>
        /// 系列节点 Checked 属性控制
        /// </summary>
        /// <param name="e"></param>
        public static void CheckControl(TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node != null && !Convert.IsDBNull(e.Node))
                {
                    CheckParentNode(e.Node);
                    if (e.Node.Nodes.Count > 0)
                    {
                        CheckAllChildNodes(e.Node, e.Node.Checked);
                    }
                }
            }
        }

        #region 私有方法

        //改变所有子节点的状态
        private static void CheckAllChildNodes(TreeNode pn, bool IsChecked)
        {
            foreach (TreeNode tn in pn.Nodes)
            {
                tn.Checked = IsChecked;

                if (tn.Nodes.Count > 0)
                {
                    CheckAllChildNodes(tn, IsChecked);
                }
            }
        }

        //改变父节点的选中状态，此处为所有子节点不选中时才取消父节点选中，可以根据需要修改
        private static void CheckParentNode(TreeNode curNode)
        {
            bool bChecked = false;

            if (curNode.Parent != null)
            {
                foreach (TreeNode node in curNode.Parent.Nodes)
                {
                    if (node.Checked)
                    {
                        bChecked = true;
                        break;
                    }
                }

                if (bChecked)
                {
                    curNode.Parent.Checked = true;
                    CheckParentNode(curNode.Parent);
                }
                else
                {
                    curNode.Parent.Checked = false;
                    CheckParentNode(curNode.Parent);
                }
            }
        }

        #endregion
        private void treeViewConcern_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CheckControl(e);
        }

        private void NoticeSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            _scsconf = new SeismicControlSettings { Name = "all", Nchecked = true };
            foreach (TreeNode ftn in treeViewConcern.Nodes)
            {
                foreach (TreeNode tn in ftn.Nodes)
                {
                    var aconf = new SeismicControlSettings { Name = tn.Text, Nchecked = tn.Checked };
                    foreach (TreeNode stn in tn.Nodes)
                    {
                        var lconf = new SeismicControlSettings { Name = stn.Text, Nchecked = stn.Checked };
                        aconf.Tnodes.Add(lconf);
                    }
                    _scsconf.Tnodes.Add(aconf);

                    foreach (var a in VaultConfs)
                    {
                        if (a.Vaultname == tn.Text)
                        {
                            a.IsConcerned = tn.Checked;
                            break;
                        }
                    }
                }
            }
            _scsconf.Save();
        }
        private void Setconfig(string p1, string p2)
        {
            if (p1 == null || p2 == null) return;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSection = (AppSettingsSection)config.GetSection("appSettings");

            appSection.Settings[p1].Value = p2;
            config.Save();
        }
        private void textBoxmfnotificationserver_TextChanged(object sender, EventArgs e)
        {
            Setconfig("notificationserver", textBoxmfnotificationserver.Text);
        }

        private void NoticeSet_SizeChanged(object sender, EventArgs e)
        {
           
        }
    }
}
