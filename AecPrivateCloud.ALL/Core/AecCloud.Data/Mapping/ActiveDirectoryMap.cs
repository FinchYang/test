//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AecCloud.Core.Domain;

//namespace AecCloud.Data.Mapping
//{
//    public class ActiveDirectoryMap :EntityMap<ActiveDirectory>
//    {
//        public ActiveDirectoryMap()
//        {
//            ToTable("ActiveDirectory");
//            HasKey(c => c.Id);
//            Property(c => c.AdminName).IsRequired();
//            Property(c => c.AdminPwd).IsRequired();
//            Property(c => c.DCInfo).IsRequired();
//            Property(c => c.Domain).IsRequired();
//            Property(c => c.LDAPRoot).IsRequired();
//        }
//    }
//}
