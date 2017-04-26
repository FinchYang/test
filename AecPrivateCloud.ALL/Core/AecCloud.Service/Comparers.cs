using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service
{
    public class ServerComparer : IEqualityComparer<VaultServer>
    {

        public bool Equals(VaultServer x, VaultServer y)
        {
            if (x == null && y == null) return true;
            if (x == null) return false;
            if (y == null) return false;
            return x.Id == y.Id;

        }

        public int GetHashCode(VaultServer obj)
        {
            return obj == null ? 0 : obj.Id.GetHashCode();
        }
    }
}
