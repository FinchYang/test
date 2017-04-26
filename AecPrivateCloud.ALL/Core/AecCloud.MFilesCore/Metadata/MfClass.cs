using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.MFilesCore.Metadata
{
    public class MfClass
    {
        [XmlAttribute("aliases")]
        public string Aliases { get; set; }

        [XmlIgnore]
        public bool IsBuiltin
        {
            get { return MetadataStructure.IsTrue(Builtin); }
        }

        [XmlAttribute("builtin")]
        public string Builtin { get; set; }

        [XmlIgnore]
        public bool IsDeleted
        {
            get { return MetadataStructure.IsTrue(Deleted); }
        }

        [XmlAttribute("deleted")]
        public string Deleted { get; set; }

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("otid")]
        public string ObjTypeId { get; set; }

        private readonly List<MfAssociatedProp> _props = new List<MfAssociatedProp>();

        [XmlArray("associatedproperties")]
        [XmlArrayItem(elementName: "property", type: typeof(MfAssociatedProp))]
        public List<MfAssociatedProp> Props
        {
            get { return _props; }
        }

    }

    public class MfAssociatedProp
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlIgnore]
        public bool IsRequired
        {
            get { return MetadataStructure.IsTrue(Required); }
        }

        [XmlAttribute("required")]
        public string Required { get; set; }
    }
}
