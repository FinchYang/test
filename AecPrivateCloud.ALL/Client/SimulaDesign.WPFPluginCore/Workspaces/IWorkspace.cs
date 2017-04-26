using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulaDesign.WPFPluginCore.Workspaces
{
    public interface IWorkspace
    {
        string Id { get; }
        string DisplayName { get; }

        string IconPath { get; }

        void Refresh();

    }
}
