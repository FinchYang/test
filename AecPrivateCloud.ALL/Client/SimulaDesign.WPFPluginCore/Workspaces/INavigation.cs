using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimulaDesign.WPFPluginCore.Workspaces
{
    public interface INavigation
    {
        ICommand GoBack { get; }

        ICommand GoForward { get; }

        string SourcePath { get; set; }

        bool NavigatedFromBrowser { get; set; }

        void NavigateTo(string uri);

    }
}
