using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;

namespace AecCloud.Service.Users
{
    public class WinActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IRepository<ActiveDirectory> _adRepo;
        private readonly IRepository<ActiveDirectoryGroup> _groupRepo;

        public WinActiveDirectoryService(IRepository<ActiveDirectory> adRepo, IRepository<ActiveDirectoryGroup> groupRepo)
        {
            _adRepo = adRepo;
            _groupRepo = groupRepo;
        }
        public ICollection<ActiveDirectory> GetAllActiveDirectories()
        {
            return _adRepo.Table.ToList();
        }

        public ActiveDirectory GetActiveDirectoryById(long adId)
        {
            return _adRepo.GetById(adId);
        }

        public ActiveDirectory GetActiveDirectoryByName(string name)
        {
            return _adRepo.Table.FirstOrDefault(c => c.Domain == name.ToUpper());
        }

        public void AddActiveDirectory(ActiveDirectory ad)
        {
            if (ad == null) throw new ArgumentNullException("ad");
            _adRepo.Insert(ad);
        }

        public void UpdateActiveDirectory(ActiveDirectory ad)
        {
            if (ad == null) throw new ArgumentNullException("ad");
            _adRepo.Update(ad);
        }

        public void RemoveActiveDirectory(ActiveDirectory ad)
        {
            if (ad == null) throw new ArgumentNullException("ad");
            _adRepo.Delete(ad);
        }


        public ICollection<ActiveDirectoryGroup> GetAllGroups()
        {
            return _groupRepo.Table.ToList();
        }

        public ICollection<ActiveDirectoryGroup> GetGroupsByDomain(long domainId)
        {
            return _groupRepo.Table.Where(c => c.DomainId == domainId).ToList();
        }

        public ICollection<ActiveDirectoryGroup> GetGroupsByName(string name)
        {
            name = name.ToUpper();
            return _groupRepo.Table.Where(c => c.Name.ToUpper() == name).ToList();
        }

        public ActiveDirectoryGroup GetGroupByNameInDomain(string name, long domainId)
        {
            name = name.ToUpper();
            return _groupRepo.Table.FirstOrDefault(c => c.Name.ToUpper() == name && c.DomainId == domainId);
        }

        public void AddActiveDirectoryGroup(ActiveDirectoryGroup group)
        {
            if (group == null) throw new ArgumentNullException("group");
            _groupRepo.Insert(group);
        }

        public void UpdateActiveDirectoryGroup(ActiveDirectoryGroup group)
        {
            if (group == null) throw new ArgumentNullException("group");
            _groupRepo.Update(group);
        }

        public void RemoveActiveDirectory(ActiveDirectoryGroup group)
        {
            if (group == null) throw new ArgumentNullException("group");
            _groupRepo.Delete(group);
        }

    }
}
