using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using AecCloud.Core;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Service.Projects
{
    public class SharedFileService : ISharedFileService
    {
        private readonly IRepository<SharedFile> _repo;

        public SharedFileService(IRepository<SharedFile> repo)
        {
            _repo = repo;
        }
        public SharedFile GetByUrlPart(string urlPart, string key)
        {
            return _repo.TableNoTracking.FirstOrDefault(c => c.UrlPart == urlPart);
        }

        public SharedFile GetByUrlHash(string urlHash, string password)
        {
            return _repo.TableNoTracking.FirstOrDefault(c => c.UrlHash == urlHash && c.Password == password);
        }

        public void Insert(SharedFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            _repo.Insert(file);
        }

        public SharedFile Get(string urlPart, string password)
        {
            return _repo.TableNoTracking.FirstOrDefault(c => c.UrlPart == urlPart&& c.Password == password);
        }
    }
}
