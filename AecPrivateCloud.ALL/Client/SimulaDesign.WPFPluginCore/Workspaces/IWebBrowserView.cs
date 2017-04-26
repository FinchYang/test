using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulaDesign.WPFPluginCore.Workspaces
{
    public interface IWebBrowserView
    {
        void GoBack();

        void GoForward();

        void SetAddressBar(string path);

        string CurrentPath { get; }

        void Refresh();
    }
}
