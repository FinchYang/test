using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.MFilesCore.Metadata
{
    public class MfUserGroup
    {

        [XmlAttribute("aliases")]
        public string Aliases { get; set; }

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
    }
}
