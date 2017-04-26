using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.MFilesCore.Metadata
{
    public class MfObjOwner
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
    public class MfObjType
    {
        [XmlAttribute("aliases")]
        public string Aliases { get; set; }

        [XmlElement("owner")]
        public MfObjOwner Owner { get; set; }

        [XmlIgnore]
        public bool IsBuiltin
        {
            get { return MetadataStructure.IsTrue(Builtin); }
        }

        [XmlAttribute("builtin")]
        public string Builtin { get; set; }

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("ownerpdid")]
        public string OwnerPdId { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlIgnore]
        public bool IsRealObj
        {
            get { return MetadataStructure.IsTrue(RealObj); }
        }

        [XmlAttribute("realobj")]
        public string RealObj { get; set; }

        [XmlElement("flags")]
        public ObjectFlags Flags { get; set; }
    }

    public class ObjectFlags
    {
        [XmlIgnore]
        public int? Value
        {
            get
            {
                if (String.IsNullOrEmpty(ValueStr)) return null;
                return int.Parse(ValueStr);
            }
        }
        [XmlElement("Value")]
        public string ValueStr { get; set; }
        public int? CanHaveFiles
        {
            get
            {
                if (String.IsNullOrEmpty(CanHaveFilesStr)) return null;
                return int.Parse(CanHaveFilesStr);
            }
        }
        [XmlElement("CanHaveFiles")]
        public string CanHaveFilesStr { get; set; }

        public int? AllowAddingNewObjects
        {
            get
            {
                if (String.IsNullOrEmpty(AllowAddingNewObjectsStr)) return null;
                return int.Parse(AllowAddingNewObjectsStr);
            }
        }
        [XmlElement("AllowAddingNewObjects")]
        public string AllowAddingNewObjectsStr { get; set; }

        public int? ShowCreationCommand
        {
            get
            {
                if (String.IsNullOrEmpty(ShowCreationCommandStr)) return null;
                return int.Parse(ShowCreationCommandStr);
            }
        }
        [XmlElement("ShowCreationCommand")]
        public string ShowCreationCommandStr { get; set; }
    }
}
