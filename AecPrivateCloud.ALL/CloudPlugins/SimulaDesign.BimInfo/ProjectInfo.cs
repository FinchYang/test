using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulaDesign.BimInfo
{
    /// <summary>
    /// 项目信息，对应Revit中的ProjectInfo
    /// </summary>
    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string Address { get; set; }
        public string ClientName { get; set; }
        public string IssueDate { get; set; }
        public string Status { get; set; }
    }
}
