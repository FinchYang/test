using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.ActiveDirectoryService
{
    /// <summary>
    /// AD域操作服务
    /// </summary>
    public interface IADService
    {
        void CreateADUser(User user, ActiveDirectory ad, ActiveDirectoryGroup group);

        void ChangeUserPassword(User user, ActiveDirectory ad, ActiveDirectoryGroup group);

        void ChangeUserInfo(User user, ActiveDirectory ad, ActiveDirectoryGroup group);

        bool HasUser(User user, ActiveDirectory ad, ActiveDirectoryGroup group);

        ActiveDirectory GetCurrentAD(IEnumerable<ActiveDirectory> ads);

    }
}
