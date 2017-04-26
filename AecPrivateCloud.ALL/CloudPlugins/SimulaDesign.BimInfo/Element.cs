using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 构件
    /// </summary>
    public class Element : ElementWithParameter
    {
        public string IfcId { get; set; }
        public string ElemType { get; set; }
        public string Level { get; set; }

        public ElementCategory Category { get; set; }

        public string Material { get; set; }

        //private List<ElementParameter> _parameters = new List<ElementParameter>();

        //public List<ElementParameter> Parameters
        //{
        //    get { return _parameters; }
        //}
        public string Family { get; set; }
    }
}
