using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using MFilesAPI;
using MfNotification.Core.NotifyObject;

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
    public class SeismicControlSettings : AppSettings<SeismicControlSettings>
    {
        public bool Nchecked ;
        public string Name ;
        public List<SeismicControlSettings> Tnodes = new List<SeismicControlSettings>();
    }
    public class LocalSettings : AppSettings<LocalSettings>
    {
        public LocalSettings()
        {
            FilePath = string.Empty;
            Guid = string.Empty;
            User = string.Empty;
            Pass = string.Empty;
            Version = "0";
        }
        public string User;
        public string Pass;
        public string Version;
        public string FilePath;
        public string Guid;
    }
    class Threadmessage
    {
        public Threadmessage()
        {
            Userid = -1;
            OVault = null;
            Username = string.Empty;
            WebServer = string.Empty;
            VaultGuid = string.Empty;
            Vaultname = string.Empty;
            Clientname = string.Empty;
            IsConcerned = false;
        }

        public bool IsConcerned;
        public Vault OVault;
        public int Userid;
        public string Username;
        public string WebServer;
        public string VaultGuid;
        public string Vaultname;
        public string Clientname;
        public List<MfTask> Listmftasks = new List<MfTask>();
    }
}
