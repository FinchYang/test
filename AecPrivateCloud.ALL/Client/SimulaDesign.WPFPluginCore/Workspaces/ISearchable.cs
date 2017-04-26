using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimulaDesign.WPFPluginCore.Workspaces
{
    public interface ISearchable
    {
        ICommand SearchCommand { get; }

        string SearchString { get; set; }
    }
}
