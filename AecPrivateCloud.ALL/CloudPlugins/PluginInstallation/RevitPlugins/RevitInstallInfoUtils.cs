using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AecCloud.PluginInstallation.RevitPlugins
{
    public class RevitInstallInfoUtils
    {
        public const string RevitKey = @"SOFTWARE\Autodesk\Revit";
        /// <summary>
        /// 是否可以安装此版本插件
        /// </summary>
        /// <param name="version">Revit版本，如：2014</param>
        /// <returns></returns>
        public static bool CanbeInstalled(string version)
        {
            RegistryKey rootKey = Registry.LocalMachine;
            //if (AddinPathUtils.IsAdministrator())
            //{
            //    rootKey = Registry.LocalMachine;
            //}
            //else
            //{
            //    rootKey = Registry.CurrentUser;
            //}
            try
            {
                var revitKey = rootKey.OpenSubKey(RevitKey, false);
                if (revitKey == null) return false;
                var versionKey = revitKey.OpenSubKey(version, false);
                revitKey.Close();
                if (versionKey == null) return false;
                versionKey.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
