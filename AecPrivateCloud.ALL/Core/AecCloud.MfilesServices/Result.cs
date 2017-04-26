using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesServices
{
    public class Result
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }

        public MfContact Contact { get; set; }
    }
}
