using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 视图
    /// </summary>
    public class ViewElement : ElementWithGuid
    {
        public int ViewType { get; set; }

        public int? ViewDiscipline { get; set; }

        public string GenLevel { get; set; }

    }
}
