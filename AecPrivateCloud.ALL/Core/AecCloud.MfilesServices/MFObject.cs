using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace AecCloud.MfilesServices
{
    public class MFObjectType
    {
        public string Alias { get; set; }
    }
    public class MFObject
    {
        public readonly MFObjectDef ObjDef;

        public MFObject(MFObjectDef objDef)
        {
            ObjDef = objDef;
            Filepaths = new List<string>();
        }

        private IDictionary<string, object> _props;

        public IDictionary<string, object> Properties
        {
            get { return _props ?? (_props = new Dictionary<string, object>()); }
            set { _props = value; }
        }
        public ICollection<string> Filepaths { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return String.Join(", ", Properties.Select(c=>c.Key+": "+c.Value));
        }
    }

    public class MFObjectDef
    {
        public string TypeAlias { get; set; }

        public string ClassAlias { get; set; }

        public MFObjectDef()
        {
            Properties = new Dictionary<string, string>();
        }


        public IDictionary<string, string> Properties { get; set; }

        //private static MFObject _default;

        //public static MFObject GetDefault()
        //{
        //    if (_default == null)
        //    {
        //        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //        var settingFile = Path.Combine(location, "projectSettings.json");
        //        if (File.Exists(settingFile))
        //        {
        //            var content = File.ReadAllText(settingFile);
        //            _default = JsonConvert.DeserializeObject<MFObject>(content);
        //        }
        //    }
        //    return _default;
        //}
    }

    public class MFObjectTypeList
    {
        public MFObjectTypeList()
        {
            Items = new Dictionary<string, string>();
        }
        public IDictionary<string, string> Items { get; set; }
    }
}
