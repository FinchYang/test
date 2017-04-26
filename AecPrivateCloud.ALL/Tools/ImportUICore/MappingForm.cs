using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SimulaDesign.ImportCore;

namespace SimulaDesign.ImportUICore
{
    public partial class MappingForm : Form
    {
        private readonly List<MfClassPropDef> _props;
        private MfObjType _objType;
        private List<MfClassPropDef> _selProps;
        private readonly List<PropValue> _fileProps;
        private readonly MfVault _vault;
        public MappingForm(MfVault vault, MfObjType objType, List<MfClassPropDef> props, List<PropValue> fileProps)
        {
            InitializeComponent();
            _vault = vault;
            _objType = objType;
            _props = props;
            _fileProps = fileProps;
            AddToolTip();
        }

        private void AddToolTip()
        {
            toolTip1.SetToolTip(listBoxProps, "对象类别的属性列表");
            toolTip1.SetToolTip(listBoxSelProps, "已选择的对象类别属性列表");
            if (_objType.IsDocType())
            {
                toolTip1.SetToolTip(listBoxFiles, "文件路径中获取的属性列表");
            }
            else
            {
                toolTip1.SetToolTip(listBoxFiles, "Excel列中获取的属性列表");
            }
            toolTip1.SetToolTip(buttonAdd, "从左侧添加属性");
            toolTip1.SetToolTip(buttonDel, "从右侧删除属性");
            toolTip1.SetToolTip(buttonUp, "选择的属性上移");
            toolTip1.SetToolTip(buttonDown, "选择的属性下移");
        }

        public void SetSelectedProps(List<MfClassPropDef> selProps)
        {
            if (selProps != null)
            {
                _selProps = selProps;
                foreach (var sp in selProps)
                {
                    var p = _props.FirstOrDefault(c => c.Def == sp.Def);
                    if (p != null)
                    {
                        _props.Remove(p);
                    }
                }
            }
        }

        private void MappingForm_Load(object sender, EventArgs e)
        {
            listBoxFiles.DataSource = _fileProps;
            foreach (var p in _props)
            {
                listBoxProps.Items.Add(p);
            }
            if (_selProps != null)
            {
                foreach (var p in _selProps)
                {
                    listBoxSelProps.Items.Add(p);
                }
            }
            listBoxFiles.SelectedItems.Clear();
            //listBoxProps.DataSource = _props;
        }

        public List<MfClassPropDef> GetSelectedProps()
        {
            return listBoxSelProps.Items.OfType<MfClassPropDef>().ToList();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private string CheckPropValid()
        {
            var propCount = listBoxFiles.Items.Count;
            var errList = new List<string>();
            for (var i = 0; i < propCount; i++)
            {
                var cpd = listBoxSelProps.Items[i] as MfClassPropDef;
                var pd = cpd.PropDef;
                var fileProp = listBoxFiles.Items[i] as PropValue;
                var value = fileProp.Value;
                var pVal = _vault.GetValue(pd, value);
                if (pVal == null)
                {
                    errList.Add(pd.GetDesc() + "\t#\t属性值：" + value);
                }
            }
            //var diff = listBoxSelProps.Items.Count - listBoxFiles.Items.Count;
            //if (diff > 0)
            //{
            //    for (var i = listBoxFiles.Items.Count; i < listBoxSelProps.Items.Count; i++)
            //    {
            //        var p = listBoxSelProps.Items[i] as MfClassPropDef;
            //        var pd = p.PropDef;
            //        if (!pd.IsTextProp())
            //        {
            //            errList.Add(pd.GetDesc() + "\t#\t新增属性必须是文本属性");
            //        }
            //    }
            //}
            if (errList.Count == 0)
            {
                return String.Empty;
            }
            return "以下属性类型与属性值不匹配：\r\n" + String.Join("\r\n", errList);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var err = CheckPropValid();
            if (!String.IsNullOrEmpty(err))
            {
                MessageBoxUtil.Error(err);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (listBoxProps.Items.Count == 0)
            {
                MessageBoxUtil.Exclamation("左边列表为空，无法选择属性！");
                return;
            }
            if (listBoxProps.SelectedItems.Count == 0)
            {
                MessageBoxUtil.Exclamation("请选择左边的属性！");
                return;
            }
            //if (listBoxSelProps.Items.Count == listBoxFiles.Items.Count)
            //{
            //    MessageBoxUtil.Exclamation("选择的属性数已经达到了上限:" + listBoxFiles.Items.Count);
            //    return;
            //}
            //var diff = listBoxProps.SelectedItems.Count + listBoxSelProps.Items.Count - listBoxFiles.Items.Count;
            //if (diff > 0)
            //{
            //    MessageBoxUtil.Exclamation("选择的属性数已经超过了上限,超过个数：" + diff);
            //    return;
            //}
            var startIndex = listBoxProps.SelectedIndices[0];
            var items = Utility.RemoveSelectedItems(listBoxProps);
            Utility.AddSelectedItems(listBoxSelProps, items);
            if (listBoxProps.Items.Count > 0)
            {
                if (startIndex >= listBoxProps.Items.Count)
                {
                    startIndex = listBoxProps.Items.Count - 1;
                }
                listBoxProps.SelectedIndex = startIndex;
            }
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (listBoxSelProps.Items.Count == 0)
            {
                MessageBoxUtil.Exclamation("右边列表为空，无法选择属性！");
                return;
            }
            if (listBoxSelProps.SelectedItems.Count == 0)
            {
                MessageBoxUtil.Exclamation("请选择右边的属性！");
                return;
            }
            var startIndex = listBoxSelProps.SelectedIndices[0]-1;
            if (startIndex == -1)
            {
                startIndex = 0;
            }
            var items = Utility.RemoveSelectedItems(listBoxSelProps);
            Utility.AddSelectedItems(listBoxProps, items);
            if (listBoxSelProps.Items.Count > 0)
            {
                listBoxSelProps.SelectedIndex = startIndex;
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (listBoxSelProps.Items.Count == 0)
            {
                MessageBoxUtil.Exclamation("右边列表为空，无法选择属性！");
                return;
            }
            if (listBoxSelProps.SelectedItems.Count == 0)
            {
                MessageBoxUtil.Exclamation("请选择右边的属性！");
                return;
            }
            var err = Utility.MoveUp(listBoxSelProps);
            if (!String.IsNullOrEmpty(err))
            {
                MessageBoxUtil.Exclamation(err);
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (listBoxSelProps.Items.Count == 0)
            {
                MessageBoxUtil.Exclamation("右边列表为空，无法选择属性！");
                return;
            }
            if (listBoxSelProps.SelectedItems.Count == 0)
            {
                MessageBoxUtil.Exclamation("请选择右边的属性！");
                return;
            }
            var err = Utility.MoveDown(listBoxSelProps);
            if (!String.IsNullOrEmpty(err))
            {
                MessageBoxUtil.Exclamation(err);
            }
        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = listBoxFiles.SelectedIndex;
            if (index != -1 && listBoxSelProps.Items.Count > index)
            {
                listBoxSelProps.SelectedIndex = index;
            }
        }
    }
}
