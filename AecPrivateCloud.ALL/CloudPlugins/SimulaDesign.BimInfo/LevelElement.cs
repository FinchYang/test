using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 楼层定义
    /// </summary>
    public class LevelElement : ElementWithGuid
    {

        public string Elevation { set; get; }

        public string ProjectElevation { set; get; }
    }
}
