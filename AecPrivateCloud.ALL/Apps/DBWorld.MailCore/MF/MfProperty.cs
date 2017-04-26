using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBWorld.MailCore.MF
{
    public class MfProperty
    {

        public MfProperty(string name, MFilesAPI.MFDataType type, object value)
        {
            PropertyName = name;
            PropertyType = type;
            PropertyValue = value;
        }

        public string PropertyName { get; set; }

        public MFilesAPI.MFDataType PropertyType { get; set; }

        public object PropertyValue { get; set; }
    }
}
