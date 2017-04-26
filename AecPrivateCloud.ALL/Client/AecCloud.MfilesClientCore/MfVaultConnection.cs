using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.MfilesClientCore
{
    public class MfVaultConnection : IEquatable<MfVaultConnection>
    {
        public MfVaultConnection()
        {
            ProtocolSequence = "ncacn_ip_tcp";
            UserSpecific = true;
        }

        public string Name { get; set; }

        public string NetworkAddress { get; set; }

        public string Port { get; set; }

        public string VaultGuid { get; set; }

        public string ProtocolSequence { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool UserSpecific { get; set; }

        public bool Equals(MfVaultConnection other)
        {
            if (other == null) return false;

            var strComparer = StringComparer.OrdinalIgnoreCase;
            if (!strComparer.Equals(Name, other.Name)) return false;
            if (!strComparer.Equals(NetworkAddress, other.NetworkAddress)) return false;
            if (!strComparer.Equals(VaultGuid, other.VaultGuid)) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MfVaultConnection;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return NetworkAddress == null ? 0 : NetworkAddress.GetHashCode();
        }
    }
}
