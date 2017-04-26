using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.MFilesCore.Metadata
{
    public class MfPropertyDef
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

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("datatype")]
        public MfDataType DataType { get; set; }
        
    }

    public class MfDataType
    {
        public MfLookup Lookup { get; set; }

        public MfMultiSelectLookup MultiSelectLookup { get; set; }

        public string Text { get; set; }

        public string Timestamp { get; set; }

        public string Boolean { get; set; }

        public string Integer { get; set; }

        public string MultiLineText { get; set; }

        public string Date { get; set; }

        public string Float { get; set; }
    }

    public class MfLookup
    {
        [XmlAttribute("otid")]
        public string ObjTypeId { get; set; }
    }

    public class MfMultiSelectLookup
    {
        [XmlAttribute("otid")]
        public string ObjTypeId { get; set; }
    }
}
