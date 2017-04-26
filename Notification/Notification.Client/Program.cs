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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1());
        //}
      
        static void Main()
        {
            //try
            //{
            //    Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\Notification.Client");
            //}
            //catch { }

            bool isAppRunning = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true,
                                                                      System.Diagnostics.Process.GetCurrentProcess().
                                                                          ProcessName,
                                                                      out isAppRunning);

            if (!isAppRunning)
            {
              //  MessageBox.Show("本程序已经在运行了，请不要重复运行！");
                Environment.Exit(1);
            }
            else
            {
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
