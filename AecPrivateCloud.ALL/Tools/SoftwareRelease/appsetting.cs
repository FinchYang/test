using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using AecCloud.WebAPI.Models.DataAnnotations;

namespace Notification.Client
{
    public class AppSettings<T> where T : new()
    {
        private const string DEFAULT_FILENAME = "app.settings";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            var filen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            File.WriteAllText(filen, (new JavaScriptSerializer()).Serialize(this));
        }

        public static void Save(T pSettings, string fileName = DEFAULT_FILENAME)
        {
            var filen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            File.WriteAllText(filen, (new JavaScriptSerializer()).Serialize(pSettings));
        }
        public static bool Isconfexists(string fileName = DEFAULT_FILENAME)
        {
            var filen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            return File.Exists(filen);
        }
        public static T Load(string fileName = DEFAULT_FILENAME)
        {
            T t = new T();
            var filen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(filen))
                t = (new JavaScriptSerializer()).Deserialize<T>(File.ReadAllText(filen));
            return t;
        }
    }
    public class LocalSettings : AppSettings<LocalSettings>
    {
        public LocalSettings()
        {
            Weburl ="http://gc.cscec82.com:8000/";
            WebPath = @"C:\PrivateCloud";
            Version = "1.0.0.1";
            AppPath = @"C:\privatecloud\0installersource\vaultapps";
            AppVersion = "28";
            AppGuid = "F101258B-FD65-4199-B22F-240B507C0DCC";
        }
        public string Weburl;
        public string WebPath;
        public string Version;
        public string AppPath;
        public string AppVersion;
        public string AppGuid;
    }
}
