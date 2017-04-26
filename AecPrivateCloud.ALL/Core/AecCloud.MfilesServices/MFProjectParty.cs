using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;

namespace AecCloud.MfilesServices
{
    public class MFProjectParty : InternalEntity
    {
        public string Name { get; set; }

        public int ManagerCount { get; set; }

        public int ViceManagerCount { get; set; }

        public int MemberCount { get; set; }

        public bool IsCurrentManager { get; set; }

        public bool IsCurrentViceManager { get; set; }

        public bool IsCurrentMember { get; set; }
    }
}
