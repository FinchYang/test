using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;

namespace AecCloud.Data.Mapping
{
   
    public class TasksMap : EntityMap<Tasks>
    {
        public TasksMap()
        {
            ToTable("Tasks");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(80);
            Property(c => c.CreationTime).IsRequired().HasMaxLength(30);
            Property(c => c.Userid).IsRequired().HasMaxLength(10);
            Property(c => c.IsNoticed).IsRequired();
            Property(c => c.Notificationtype).IsRequired();
            Property(c => c.Objectid).IsRequired();
            Property(c => c.Type).IsRequired();
            Property(c => c.Version).IsRequired();
            Property(c => c.Url).IsRequired().HasMaxLength(300);
            Property(c => c.Vaultguid).IsRequired().HasMaxLength(50);
        }
    }
}
