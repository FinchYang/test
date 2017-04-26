using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AecCloud.PluginInstallation.RevitPlugins
{
    [XmlRoot("RevitAddIns")]
    public class RevitAddinFile : IEquatable<RevitAddinFile>
    {
        public AddinItem AddIn { get; set; }


        public string GetAssemblyPath()
        {
            if (AddIn == null) return null;
            return AddIn.AssemblyPath;
        }

        public bool SetAssemblyPath(string newPath)
        {
            if (AddIn == null) return false;
            AddIn.AssemblyPath = newPath;
            return true;
        }

        public static RevitAddinFile GetFromFile(string filePath)
        {
            string err = null;
            var obj = SerialUtils.GetObject<RevitAddinFile>(filePath, out err);
            if (!String.IsNullOrEmpty(err)) Trace.WriteLine(err);
            return obj;
        }

        public string SaveToFile(string filePath)
        {
            var err = SerialUtils.ToFile(filePath, this);
            return err;
        }

        public bool Equals(RevitAddinFile other)
        {
            if (other == null) return false;

            if (AddIn != null) return AddIn.Equals(other.AddIn);
            if (AddIn == null && other.AddIn != null) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as RevitAddinFile;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return AddIn == null ? 0 : AddIn.GetHashCode();
        }
    }

    public class AddinItem : IEquatable<AddinItem>
    {
        public static readonly string Application = "Application";

        public static readonly string Command = "Command";


        [XmlAttribute("Type")]
        public string TypeName { get; set; }

        #region Name
        /// <summary>
        /// Application
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Command
        /// </summary>
        public string Text { get; set; }

        #endregion

        [XmlElement("Assembly")]
        public string AssemblyPath { get; set; }

        #region PluginId
        /// <summary>
        /// Application
        /// </summary>
        public string AddInId { get; set; }
        /// <summary>
        /// Command
        /// </summary>
        public string ClientId { get; set; }

        #endregion PluginId

        public string FullClassName { get; set; }

        public string VendorId { get; set; }

        public string VendorDescription { get; set; }

        public string GetPluginId()
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(Application, TypeName)) return AddInId;
            if (StringComparer.OrdinalIgnoreCase.Equals(Command, TypeName)) return ClientId;
            return String.Empty;
        }

        public string GetPluginName()
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(Application, TypeName)) return Name;
            if (StringComparer.OrdinalIgnoreCase.Equals(Command, TypeName)) return Text;
            return String.Empty;
        }

        public bool Equals(AddinItem other)
        {
            if (other == null) return false;

            var comparer = StringComparer.OrdinalIgnoreCase;

            if (!comparer.Equals(TypeName, other.TypeName)) return false;
            if (!comparer.Equals(GetPluginId(), other.GetPluginId())) return false;
            if (!comparer.Equals(GetPluginName(), other.GetPluginName())) return false;
            if (!comparer.Equals(FullClassName, other.FullClassName)) return false;
            if (!comparer.Equals(AssemblyPath, other.AssemblyPath)) return false;

            return true;

        }

        public override bool Equals(object obj)
        {
            var other = obj as AddinItem;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            var id = GetPluginId();
            return id == null ? 0 : id.GetHashCode();
        }
    }
}
