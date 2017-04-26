using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SimulaDesign.ImportCore;
using SimulaDesign.ImportUICore;

namespace SimulaDesign.ImportObjectTool
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var vaultName = String.Empty;
            if (args.Length == 0)
            {
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

            }
            else
            {
                vaultName = args[0];
            }
            var ok = MfVault.TestLogin(vaultName);
            if (!ok)
            {
                MessageBox.Show("连接库("+vaultName+")失败！");
                return;
            }
            Trace.AutoFlush = true;
            var logDir = "Log\\";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            Trace.Listeners.Add(new CustomTextListener(logDir + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
            Application.Run(new MainForm(vaultName));
        }
    }
}
