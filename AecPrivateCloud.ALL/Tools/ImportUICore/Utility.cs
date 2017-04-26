using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SimulaDesign.ImportCore;

namespace SimulaDesign.ImportUICore
{
    public class Utility
    {
        public static DataGridViewColumn GetColumn(string name, bool readOnly)
        {
            return new DataGridViewTextBoxColumn { Name = name, DataPropertyName = name, ReadOnly = readOnly };
        }
        public static DataGridViewColumn[] GetColumns(int count)
        {
            var cols = Enumerable.Range(1, count - 1).Select(c => GetColumn("属性列" + c.ToString("00"), true)).ToList();
            cols.Insert(0, GetColumn("文件名", false));
            return cols.ToArray();
        }
        public static string MoveUp(ListBox listBox)
        {
            var selIndices = new List<object>();
            for (var i = 0; i < listBox.SelectedIndices.Count; i++)
            {
                var index = listBox.SelectedIndices[i];
                if (index == 0)
                {
                    return "不能选择第一项！";
                }
                var obj = listBox.Items[index - 1];
                var obj0 = listBox.Items[index];
                listBox.Items[index] = obj;
                listBox.Items[index - 1] = obj0;
                selIndices.Add(obj0);
            }
            if (selIndices.Count > 0)
            {
                listBox.SelectedIndices.Clear();
                listBox.SelectedItems.Clear();
                foreach (var i in selIndices)
                {
                    listBox.SelectedItems.Add(i);
                }
            }
            return String.Empty;
        }

        public static string MoveDown(ListBox listBox)
        {
            var selIndices = new List<object>();
            for (var i = listBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                var index = listBox.SelectedIndices[i];
                if (index == listBox.Items.Count - 1)
                {
                    return "不能选择最后一项！";
                }
                var obj = listBox.Items[index];
                var obj0 = listBox.Items[index + 1];
                listBox.Items[index + 1] = obj;
                listBox.Items[index] = obj0;
                selIndices.Add(obj);
            }
            if (selIndices.Count > 0)
            {
                listBox.SelectedIndices.Clear();
                listBox.SelectedItems.Clear();
                foreach (var i in selIndices)
                {
                    listBox.SelectedItems.Add(i);
                }
            }
            return String.Empty;
        }

        public static void AddSelectedItems(ListBox listBox, object[] classPropDefs)
        {
            listBox.Items.AddRange(classPropDefs);
        }

        public static List<MfClassPropDef> GetPropsFromDataColumns(DataGridView dataGridView1)
        {
            var list = new List<MfClassPropDef>();
            for (var i = 0; i < dataGridView1.ColumnCount; i++)
            {
                var pd = dataGridView1.Columns[i].Tag as MfClassPropDef;
                list.Add(pd);
            }
            return list;
        }

        public static void LoadData(DataGridView dgv, List<List<string>> data)
        {
            if (dgv.RowCount > 0)
            {
                dgv.Rows.Clear();
            }
            if (dgv.ColumnCount > 0)
            {
                dgv.Columns.Clear();
            }
            var colNames = data[0];
            foreach (var c in colNames)
            {
                var col = GetColumn(c, true);
                dgv.Columns.Add(col);
            }
            for (var i = 1; i < data.Count; i++)
            {
                var rowData = data[i];
                var dataRow = new DataGridViewRow();
                dataRow.CreateCells(dgv, rowData.Select(c => (object)c).ToArray());
                //dataRow.Cells[0].ToolTipText = f.NewFilename;
                dgv.Rows.Add(dataRow);
            }
        }

        public static List<List<string>> GetData(DataGridView dgv)
        {
            var list = new List<List<string>>();
            var headers = new List<string>();
            for (var i = 0; i < dgv.ColumnCount; i++)
            {
                headers.Add(dgv.Columns[i].Name);
            }
            list.Add(headers);

            for (var i = 0; i < dgv.RowCount; i++)
            {
                var row = dgv.Rows[i];
                var rows = new List<string>();
                for (var j = 0; j < dgv.ColumnCount; j++)
                {
                    var val = String.Empty;
                    if (row.Cells[j].Value != null)
                    {
                        val = row.Cells[j].Value.ToString();
                    }
                    rows.Add(val);
                }
                list.Add(rows);
            }
            return list;
        }

        public static object[] RemoveSelectedItems(ListBox listBox)
        {
            var items = listBox.SelectedItems.OfType<object>().ToArray();
            for (var i = listBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                var index = listBox.SelectedIndices[i];
                listBox.Items.RemoveAt(index);
            }
            return items;
        }
    }

    public class PropValue
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}


