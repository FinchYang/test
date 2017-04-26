using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core.Domain.Vaults
{
    /// <summary>
    /// 库所在的服务器表
    /// </summary>
    public class VaultServer : Entity, IEquatable<VaultServer>
    {
        public VaultServer()
        {
            ServerPort = "2266";
        }
        public string ServerPort { get; set; }
        public string Ip { get; set; }

        public string LocalIp { get; set; }

        public string Port { get; set; }

        public string DnsAlias { get; set; }

        public string AdminName { get; set; }

        public string AdminPwd { get; set; }

        public int DomainId { get; set; }

        //public virtual ActiveDirectory Domain { get; set; }

        public bool Equals(VaultServer other)
        {
            if (other == null) return false;
            return StringComparer.OrdinalIgnoreCase.Equals(LocalIp, other.LocalIp);
        }

        public override bool Equals(object obj)
        {
            var other = obj as VaultServer;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return LocalIp == null ? 0 : LocalIp.GetHashCode();
        }
    }
}
