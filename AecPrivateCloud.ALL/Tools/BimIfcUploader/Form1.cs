using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFilesAPI;

namespace BimIfcUploader
{
    public partial class Form1 : Form
    {
        private readonly string _vaultName;

        private Vault vault;
        private ObjectVersions ifcs;
        public Form1(string vaultName)
        {
            InitializeComponent();
            _vaultName = vaultName;
        }
        private CancellationTokenSource cts;

        private void ReportProgress(int percent)
        {
            if (InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Value = percent; })); //progressBar1.PerformStep(); 
            }
            else
            {
                progressBar1.Value = percent;  //progressBar1.PerformStep();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            vault = Utility.GetVault(_vaultName);
            ifcs = Utility.SearchFiles(vault, "ifc");
            var modelDef = Utility.GetModelPropDef(vault);
            var objs = ifcs.OfType<ObjectVersion>().Select(c => new MfObj
            {
                Id = c.ObjVer.ID, Title = c.Title, ModelId = vault.ObjectPropertyOperations.GetProperty(c.ObjVer, modelDef).Value.GetLookupID()
            });
            
            foreach (var o in objs)
            {
                checkedListBox1.Items.Add(o);
            }
            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private ObjectVersions GetIfcs()
        {
            var ifcObjs = ifcs.OfType<ObjectVersion>().ToList();
            var versions = new ObjectVersions();
            foreach (MfObj o in checkedListBox1.CheckedItems)
            {
                var obj = ifcObjs.FirstOrDefault(c => c.ObjVer.ID == o.Id);
                versions.Add(-1, obj);
            }
            return versions;
        }

        private async void buttonUp_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            var progressIndicator = new Progress<int>(ReportProgress);
            cts = new CancellationTokenSource();
            var guid = vault.GetGUID().TrimStart(new[] { '{' }).TrimEnd(new[] { '}' });
            try
            {
                var vers = GetIfcs();
                int x = await Utility.UploadIfcsAsync(vault, guid, vers, progressIndicator, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                //Do stuff to handle cancellation
                MessageBox.Show("用户取消！");
            }
        }

        private void buttonPartPath_Click(object sender, EventArgs e)
        {
            try
            {
                Utility.UpdatePartPaths(vault);
                MessageBox.Show("成功！");
            }
            catch(Exception ex)
            {
                MessageBox.Show("出错：" + ex.Message);
            }
        }

    }
}
