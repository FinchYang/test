using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfMsmqCom
{
    [Serializable]
    public class MfTask
    {
        public MfTask() { }
        public string VualtGuid { get; set; }
        public int ObjType { get; set; }
        public int ObjClass { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int AssignTo { get; set; }
        public List<MfProperty> OtherProps { get; set; }  

        //public int OtherPropDef { get; set; }
        //public int OtherPropValue { get; set; }
        public override string ToString()
        {
            return string.Format("{0}--AssignTo--{1}", Title, AssignTo);
        }  
    }

    [Serializable]
    public class MfProperty
    {
        public MfProperty() { }
        public int PropDef { get; set; }
        //public int Type { get; set; }
        public string Value { get; set; }
    }
}
