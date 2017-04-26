using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimIfcUploader
{
    public class MfObj
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int ModelId { get; set; }

        public override string ToString()
        {
            return "MId: " + ModelId + " # " + Title + " # " + Id;
        }
    }
}
