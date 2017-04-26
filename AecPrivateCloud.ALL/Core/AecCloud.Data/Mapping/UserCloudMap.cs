//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AecCloud.Core.Domain;

//namespace AecCloud.Data.Mapping
//{
//    public class UserCloudMap : EntityMap<UserCloud>
//    {
//        public UserCloudMap()
//        {
//            ToTable("UserCloud");
//            HasKey(c => c.Id);
//            Property(c => c.UserId).IsRequired();
//            Property(c => c.CloudId).IsRequired();
//        }
//    }
//}
