using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.PluginInstallation;
using AecCloud.PluginInstallation.RevitPlugins;
using AecCloud.PluginInstallation.VaultApps;

namespace AecCloud.ClientConsole
{
    class Tests
    {
        internal static void TestVaultAppDefFile()
        {
            var appDefFile =
                @"D:\Program Files\M-Files\10.2.3920.54\Client\Apps\{B33E2A16-C8BA-410A-BE31-61CB2B369215}\sysapps\F101258B-FD65-4199-B22F-240B507C0DCC\appdef.xml";
            string err;
            var ad = SerialUtils.GetObject<VaultAppDefFile>(appDefFile, out err);
            Console.WriteLine(ad == null ? err : ad.Guid);
            Console.ReadKey();
        }

        internal static void TestNeedUpdateVaultApp()
        {
            var appFolder =
                @"D:\Program Files\M-Files\10.2.3920.54\Client\Apps\{B33E2A16-C8BA-410A-BE31-61CB2B369215}\sysapps";
            var appGuid = "F101258B-FD65-4199-B22F-240B507C0DCC";
            var zipFile = @"E:\Dev\Cloud\CloudCode\BimClouds\BimInstaller\ProjectTaskApp.zip";
            var need = VaultAppUtils.NeedUpdate(appFolder, appGuid, zipFile);
            Console.WriteLine(need);
            Console.ReadKey();
        }

        internal static void TestNeedUpdateRevitPlugin()
        {
            var version = "2014";
            var pluginDir = @"E:\Dev\Cloud\CloudCode\AecCloud\bin\Debug\RevitPlugin\2014\BimCloud";
            var alluser = false;
            var installed = AddinPathUtils.PluginInstalledOrNoNeed(version, pluginDir, alluser);
            Console.WriteLine(installed);
            Console.ReadKey();
        }
    }
}
