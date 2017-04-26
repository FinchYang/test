using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulaDesign.ImportUICore;

namespace BimIfcUploader
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string vaultName = null;
            var vsf = new VaultSelectForm();
            if (vsf.ShowDialog() == DialogResult.OK)
            {
                vaultName = vsf.GetVaultName();
            }
            else
            {
                MessageBox.Show("必须指定文档库名称！");
                return;
            }


            Application.Run(new Form1(vaultName));
        }
    }
}
