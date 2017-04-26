using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration;

namespace AecCloud.Data
{
    public abstract class EntityMap<T> : EntityTypeConfiguration<T> where T : class
    {
        protected EntityMap()
        {
            PostInitialize();
        }

        protected virtual void PostInitialize()
        {

        }
    }
}
