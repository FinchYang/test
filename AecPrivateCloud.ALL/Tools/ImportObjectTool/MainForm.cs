using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SimulaDesign.ImportCore;
using SimulaDesign.ImportUICore;

namespace SimulaDesign.ImportObjectTool
{
    public partial class MainForm : Form
    {
        private string _vaultName;

        private MfVault _vault;

        private static readonly string _title = "对象批量导入工具";

        private string ConfigPath = "MappingConfig";

        private BackgroundWorker _worker = new BackgroundWorker();
        public MainForm(string vaultName)
        {
            InitializeComponent();
            _vaultName = vaultName;
            buttonMapping.Enabled = false;
            buttonImport.Enabled = false;
            FormClosing += MainForm_FormClosing;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel)
            {
                if (_worker.IsBusy)
                {
                    var dr = MessageBoxUtil.Question("当前正在导入文档，是否取消导入？");
                    if (dr == DialogResult.Yes)
                    {
                        _worker.CancelAsync();
                        if (_worker.IsBusy)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                if (!e.Cancel)
                {
                    if (_vault != null && _vault.PropSets.Count > 0)
                    {
                        _vault.Save(ConfigPath);
                    }
                    //ExportExcel(false, false);
                }

            }
        }

        private MfObjType _currentObjType;

        private void comboBoxObjType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentObjType = comboBoxObjType.SelectedItem as MfObjType;
            var csList = _vault.GetClasses(_currentObjType.Id);
            if (comboBoxClasses.Items.Count > 0)
            {
                comboBoxClasses.Items.Clear();
            }
            foreach (var c in csList)
            {
                comboBoxClasses.Items.Add(c);
            }
            comboBoxClasses.SelectedIndex = 0;
        }

        private MfClass _currentClass;

        private void comboBoxClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentClass = comboBoxClasses.SelectedItem as MfClass;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _vault = MfVault.GetFromConfig(ConfigPath, _vaultName);
            if (_vault == null) //从配置中加载失败
            {
                _vault = MfVault.GetVault(_vaultName);
            }
            var objTypes = _vault.GetObjectTypes();
            comboBoxObjType.DataSource = objTypes;
            var lastConfig = _vault.GetLastMapping();
            var objTypeIndex = 0;
            MfClass mfClass = null;
            if (lastConfig != null)
            {
                _currentObjType = lastConfig.ObjType;

                for (var i = 0; i < objTypes.Count; i++)
                {
                    if (_currentObjType.Equals(objTypes[i]))
                    {
                        objTypeIndex = i;
                        break;
                    }
                }
                mfClass = lastConfig.ObjectClass;
            }
            else
            {
                _currentObjType = objTypes[objTypeIndex];
            }
            comboBoxObjType.SelectedIndex = objTypeIndex;
            var mcs = _vault.GetClasses(_currentObjType.Id);
            var classIndex = 0;
            for (var i = 0; i < mcs.Count; i++)
            {
                comboBoxClasses.Items.Add(mcs[i]);
                if (mfClass != null && mfClass.Equals(mcs[i]))
                {
                    classIndex = i;
                }
            }
            comboBoxClasses.SelectedIndex = classIndex;
            _currentClass = mcs[classIndex];
            this.comboBoxObjType.SelectedIndexChanged += new System.EventHandler(this.comboBoxObjType_SelectedIndexChanged);
            this.comboBoxClasses.SelectedIndexChanged += new System.EventHandler(this.comboBoxClasses_SelectedIndexChanged);
        }

        private List<List<string>> dataList;

        private List<string> _colNames;

        private void buttonImportExcel_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.Filter = "Excel文件|*.xlsx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var excelFilepath = ofd.FileName;
                try
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();
                    dataList = ExcelUtility.GetFromExcel(excelFilepath);
                    Utility.LoadData(dataGridView1, dataList);
                    _colNames = dataList[0];
                    buttonMapping.Enabled = true;
                    Text = _title + " 路径：" + excelFilepath;
                }
                catch (Exception ex)
                {
                    MessageBoxUtil.Error(ex.Message);
                }
            }
        }

        private List<MfClassPropDef> _selProps;

        private void buttonMapping_Click(object sender, EventArgs e)
        {
            if (comboBoxClasses.Items.Count == 0)
            {
                MessageBoxUtil.Exclamation("无法获取对象类别列表!");
                return;
            }
            var pvs = _colNames.Select(c => new PropValue {Name = c}).ToList();
            var mf = new MappingForm(_vault, _currentObjType, _vault.GetClassProps(_currentClass.Id), pvs);
            var selProps = _vault.GetMappedProps(_currentClass.Id);
            if (selProps != null)
            {
                mf.SetSelectedProps(selProps);
            }
            var dr = mf.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                _selProps = mf.GetSelectedProps();
                for (var i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    dataGridView1.Columns[i].Tag = _selProps[i];
                }
                _vault.AddOrUpdateClassPropSet(_currentObjType, _currentClass, _selProps);
                buttonImport.Enabled = true;
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (dataGridView1.ColumnCount == 0)
            {
                MessageBoxUtil.Exclamation("还未获取数据！");
                return;
            }
            var classPropDef = dataGridView1.Columns[0].Tag as MfClassPropDef;
            if (classPropDef == null)
            {
                MessageBoxUtil.Exclamation("还未进行属性映射！");
                return;
            }
            var currentClass = _currentClass;
            if (classPropDef.ClassId != currentClass.Id)
            {
                MessageBoxUtil.Exclamation("您重新选择了文档类别，请重新进行属性映射！");
                return;
            }
            if (_worker.IsBusy)
            {
                MessageBox.Show("导入进行中...");
                return;
            }

            _worker.RunWorkerAsync();
        }

        private int _currentStep = 0;

        private List<Tuple<int, string>> _errFiles = new List<Tuple<int, string>>();


        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _errFiles.Clear();
            var count = dataList.Count - 1;//dataGridView1.RowCount;//_files.Files.Count;
            for (var i = 0; i < count; i++)
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                var precent = ((double)i) / count;
                var progress = (int)(precent * 100);
                if (_currentStep < progress)
                {
                    _worker.ReportProgress(progress);
                    _currentStep = progress;
                }
                var f = dataList[i+1];
                var ok = _vault.ObjectToSystem(_currentObjType.Id, _currentClass, _selProps, f);
                if (!String.IsNullOrEmpty(ok)) _errFiles.Add(new Tuple<int, string>(i, ok));
            }
            _worker.ReportProgress(100);
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage; //.PerformStep();//
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var log = TraceLog.GetLogger<MainForm>();
                log.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, e.Error.Message + "\r\n" + e.Error.StackTrace);
                log.Close();
                MessageBoxUtil.Error("导入出错:" + e.Error.Message);
            }
            else if (e.Cancelled)
            {
                MessageBoxUtil.Exclamation("导入取消！");
            }
            else
            {
                var msg = "导入完成！";
                if (_errFiles.Count > 0)
                {
                    msg += "有对象未能被导入，请查看输出文件！";
                }
                MessageBox.Show(msg);
            }
            if (_errFiles.Count > 0)
            {
                var tPath = Path.GetTempPath();
                var tFile = Path.Combine(tPath, "已存在或创建失败列表" + DateTime.Now.ToString("-yyyy-MM-dd-HH-mm") + ".txt");
                using (var sw = new StreamWriter(tFile, false, Encoding.Default))
                {
                    foreach (var tf in _errFiles)
                    {
                        var err = "第"+(tf.Item1+1)+"行数据" + "\t原因：" + tf.Item2;
                        sw.WriteLine(err);
                    }
                    sw.Close();
                }
                System.Diagnostics.Process.Start(tFile);
            }
            ResetProgressBar();
        }

        private void ResetProgressBar()
        {
            progressBar1.Value = 0;
            _currentStep = 0;
        }
    }
}
