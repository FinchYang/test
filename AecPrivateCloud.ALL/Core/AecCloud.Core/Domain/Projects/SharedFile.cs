using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Projects
{
    public class SharedFile : Entity
    {
        public SharedFile()
        {
            CreatedUtc = DateTime.UtcNow;
        }
        public string UrlPart { get; set; }

        public string Password { get; set; }

        public string UrlHash { get; set; }

        public string UrlKey { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime ExpiredTimeUtc { get; set; }
    }
}
