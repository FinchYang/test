using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
namespace Notification.Client
{
    static class Program
    {
        [STAThread]
       
        static void Main()
        {
            //try
            //{
            //    Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\Notification.Client");
            //}
            //catch { }
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
              //  MessageBox.Show("程序已启动！");
                return;
            }
         
                var admin = IsAdministrator();
                if (!admin)
                {
                    // Launch itself as administrator
                    var proc = new ProcessStartInfo();
                    proc.UseShellExecute = true;
                //    proc.Arguments = "\"" + args[0] + "\"";
                    proc.WorkingDirectory = Environment.CurrentDirectory;
                    proc.FileName = Application.ExecutablePath;
                    proc.Verb = "runas";
                    try
                    {
                        Process.Start(proc);
                    }
                    catch
                    {
                        return;
                    }
                    return;  // Quit itself
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
        }
        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
    }
}
