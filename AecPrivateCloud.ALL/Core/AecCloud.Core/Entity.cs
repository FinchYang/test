using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Core
{
    public abstract class Entity
    {
        public long Id { get; set; }

        public override int GetHashCode()
        {
            if (Id == default(int)) return base.GetHashCode();
            return Id.GetHashCode();
        }
    }

    public abstract class InternalEntity : Entity
    {
        /// <summary>
        /// M-Files ID
        /// </summary>
        public int InternalId { get; set; }
    }
}
