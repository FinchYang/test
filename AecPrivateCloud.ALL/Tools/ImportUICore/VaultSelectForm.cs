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
    public partial class VaultSelectForm : Form
    {
        public VaultSelectForm()
        {
            InitializeComponent();
            listBox1.DoubleClick += listBox1_DoubleClick;
        }

        private void VaultSelectForm_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = MfVault.GetVaultList();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("没有选择库！");
                return;
            }
            DialogResult = DialogResult.OK;
        }

        public string GetVaultName()
        {
            return listBox1.SelectedItem.ToString();
        }
    }
}
