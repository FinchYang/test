using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AecCloud.PluginInstallation;
using AecCloud.PluginInstallation.RevitPlugins;
using AecCloud.PluginInstallation.VaultApps;

namespace AecCloud.ClientConsole
{
    class Program
    {
        static void ShowHelper()
        {
            Console.WriteLine("Usages: ");
            Console.WriteLine("1:\t -u 1 -t vaultapp -p appFolder -d zipFiles");
            Console.WriteLine("2:\t -u 0 -t revitplugin -d pluginDirectory");
        }
        /// <summary>
        /// -u 1 #表示需要启用管理员权限；-u 0 #表示不需要启用管理员权限
        /// 1. -u 1 -t vaultapp -p appFolder -d zipFiles
        /// 2. -u 0 -t revitplugin -d pluginDirectory
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Trace.WriteLine("未指定命令行参数！");
                ShowHelper();
                return;
            }
            Logger.Configure();

            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"DBWorld\Log\console.log")));
            
            var needAdmin = Convert.ToInt32(args[1]) == 1;
            if (needAdmin)
            {
                var admin = IsAdministrator();
                if (!admin)
                {
                    // Launch itself as administrator
                    var proc = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        Arguments = JoinArgs(args),
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                        Verb = "runas"
                    };
                    try
                    {
                        Process.Start(proc);
                    }
                    catch
                    {
                        return;
                    }
                    return; // Quit itself
                }
            }
            Run(args);
        }

        static void Run(string[] args)
        {
            var needAdmin = Convert.ToInt32(args[1]) == 1;
            switch (args[3])
            {
                case "vaultapp":
                    var appFolder = args[5];
                    var files = args.SkipWhile((c, i) => i < 7).ToArray();
                    try
                    {
                       
                        var errList = VaultAppUtils.ExtractApps(appFolder, files, Logger.Log);
                        Logger.Log.Error("加载App失败：" + String.Join("\r\n", errList));
                        Trace.WriteLine("加载App成功：");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("加载App失败：" + ex.Message);
                    }
                    break;
                case "revitplugin":
                    var baseFolder = args[5];
                    var dict = AddinPathUtils.GetAddinDict(baseFolder);
                    foreach (var d in dict)
                    {
                        foreach (var f in d.Value)
                        {
                            AddinPathUtils.InstallPlugin(d.Key, f, needAdmin);
                        }
                    }
                    break;
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

        static string JoinArgs(string[] args)
        {
            var argList = new List<string>();
            foreach (var a in args)
            {
                if (a.Contains(" ")) argList.Add("\"" + a + "\"");
                else argList.Add(a);
            }
            return String.Join(" ", argList.ToArray());
        }
    }
}
