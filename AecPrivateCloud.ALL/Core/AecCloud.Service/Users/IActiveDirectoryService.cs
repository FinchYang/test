using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Service.Users
{
    public interface IActiveDirectoryService
    {
        ICollection<ActiveDirectory> GetAllActiveDirectories();

        ActiveDirectory GetActiveDirectoryById(long adId);

        ActiveDirectory GetActiveDirectoryByName(string name);

        void AddActiveDirectory(ActiveDirectory ad);

        void UpdateActiveDirectory(ActiveDirectory ad);

        void RemoveActiveDirectory(ActiveDirectory ad);


        ICollection<ActiveDirectoryGroup> GetAllGroups();

        ICollection<ActiveDirectoryGroup> GetGroupsByDomain(long domainId);

        ICollection<ActiveDirectoryGroup> GetGroupsByName(string name);

        ActiveDirectoryGroup GetGroupByNameInDomain(string name, long domainId);

        void AddActiveDirectoryGroup(ActiveDirectoryGroup group);

        void UpdateActiveDirectoryGroup(ActiveDirectoryGroup group);

        void RemoveActiveDirectory(ActiveDirectoryGroup group);

    }
}
