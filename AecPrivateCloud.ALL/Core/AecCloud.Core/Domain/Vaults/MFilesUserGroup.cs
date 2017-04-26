using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Core.Domain.Vaults
{
    public class MFilesUserGroup : Entity, IComparable<MFilesUserGroup>, IComparable
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public long VaultId { get; set; }
        public long PartId { get; set; }
        public virtual ProjectParty Party { get; set; }

        public int CompareTo(MFilesUserGroup other)
        {
            if (other == null) return 1;
            return GroupId.CompareTo(other.GroupId);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((MFilesUserGroup)obj);
        }
    }
}
