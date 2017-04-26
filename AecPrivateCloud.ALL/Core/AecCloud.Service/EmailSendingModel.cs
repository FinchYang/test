using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service
{
    public class EmailSendingModel
    {
        public string MailTo { get;set;}
        public string Title{get;set;} 
        public string Body {get;set;}
        public bool IsHtml {get;set;}
        public Encoding TextEncoding { get; set; }
    }
}
