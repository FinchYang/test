using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AecCloud.PluginInstallation.VaultApps
{
    [XmlRoot("application")]
    public class VaultAppDefFile : IEquatable<VaultAppDefFile>
    {
        public static readonly string FileName = "appdef.xml";

        [XmlElement("guid")]
        public string Guid { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("publisher")]
        public string Publisher { get; set; }

        public bool Equals(VaultAppDefFile other)
        {
            if (other == null) return false;
            if (!StringComparer.OrdinalIgnoreCase.Equals(Guid, other.Guid)) return false;
            if (!StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name)) return false;
            if (!StringComparer.OrdinalIgnoreCase.Equals(Version, other.Version)) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VaultAppDefFile;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Guid == null ? 0 : Guid.GetHashCode();
        }

        public static VaultAppDefFile GetFromFile(string appdefXml)
        {
            string err;
            var obj = SerialUtils.GetObject<VaultAppDefFile>(appdefXml, out err);
            if (!String.IsNullOrEmpty(err)) Trace.WriteLine(err);
            return obj;
        }
    }
}
