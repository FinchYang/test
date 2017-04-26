using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimulaDesign.WPFPluginCore.Workspaces
{
    public class PathItem
    {
        public string Folder { get; set; }
        public ImageSource Image { get; set; }

        public bool IsFolder { get; set; }

        public override string ToString()
        {
            return Folder ?? String.Empty;
        }
    }
}
