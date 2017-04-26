using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AecCloud.MFilesCore.Metadata
{
    [XmlRoot("content")]
    public class ValueListContents
    {
        private readonly List<MfValueListItem> _items = new List<MfValueListItem>();
        [XmlElement(ElementName = "vlitem", Type = typeof(MfValueListItem))]
        public List<MfValueListItem> Items
        {
            get { return _items; }
        }
    }
    //[XmlRoot("vlitem")]
    public class MfValueListItem
    {
        [XmlAttribute("deleted")]
        public string Deleted { get; set; }
        [XmlIgnore]
        public bool IsDeleted
        {
            get { return StringComparer.OrdinalIgnoreCase.Equals(Deleted, "true"); }
        }
        [XmlAttribute("guid")]
        public string Guid { get; set; }
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("otid")]
        public string ObjTypeId { get; set; }

        [XmlAttribute("value")]
        public string Name { get; set; }
    }
}
