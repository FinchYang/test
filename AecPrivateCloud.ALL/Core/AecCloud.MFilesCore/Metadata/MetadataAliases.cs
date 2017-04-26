using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AecCloud.MFilesCore.Metadata
{
    public class MetadataAliases
    {
        private readonly Dictionary<string, string> _valueList = new Dictionary<string, string>();

        [JsonProperty(PropertyName = "valuelist")]
        public Dictionary<string, string> ValueLists
        {
            get { return _valueList; }
        }

        private readonly Dictionary<string, string> _usergroups = new Dictionary<string, string>();
         [JsonProperty(PropertyName = "usergroups")]
        public Dictionary<string, string> UserGroups
        {
            get { return _usergroups; }
        }

         private readonly Dictionary<string, MfObjectAliases> _objs
             = new Dictionary<string, MfObjectAliases>();
         [JsonProperty(PropertyName = "objs")]
         public Dictionary<string, MfObjectAliases> Objects
        {
            get { return _objs; }
        }

         private readonly Dictionary<string, MfViewAliases> _views = new Dictionary<string, MfViewAliases>();
        [JsonProperty(PropertyName = "views")]
         public Dictionary<string, MfViewAliases> Views
        {
            get { return _views; }
        }
    }

    public class MfObjectAliases
    {
        public string Alias { get; set; }

        public string Owner { get; set; }

        private readonly Dictionary<string, MfClassAliases> _classDict = new Dictionary<string, MfClassAliases>();

        public Dictionary<string, MfClassAliases> ClassDict
        {
            get { return _classDict; }
        }

    }

    public class MfClassAliases
    {
        public string Alias { get; set; }

        private readonly Dictionary<string, string> _propDict = new Dictionary<string, string>();

        public Dictionary<string, string> PropDict
        {
            get { return _propDict; }
        }
    }

    public class MfViewAliases
    {
        public string Alias { get; set; }
        public int Id { get; set; }

        public string Guid { get; set; }

        public string Name { get; set; }

        public bool Common { get; set; }

        public bool Deleted { get; set; }
    }
}
