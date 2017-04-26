using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Service.Projects
{
    public interface ISharedFileService
    {
        SharedFile GetByUrlPart(string urlPart, string key);

        SharedFile GetByUrlHash(string urlHash, string password);

        SharedFile Get(string urlPart, string password);

        void Insert(SharedFile file);


    }
}
