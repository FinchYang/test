using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesServices
{
    public class MFDownloadFile
    {
        public byte[] Content { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public int Version { get; set; }
    }
}
