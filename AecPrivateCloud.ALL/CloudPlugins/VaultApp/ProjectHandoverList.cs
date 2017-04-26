using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;

namespace VaultApp
{
    class ProjectHandoverList:DocumentOperation
    {
        public ProjectHandoverList(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
            //  
        }

        public void ProjectHandoverListCreate()
        {
            try
            {
                table.Cell(5, 2).Range.Text = Project.PropProjName + Project.PropProjNum;

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("ProjectHandoverListCreate" + ex.Message);
            }
        }
    }
}
