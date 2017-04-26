using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Data.Mapping.Projects
{
  
   
    public class ProjectMap : EntityMap<Project>
    {
        public ProjectMap()
        {
            ToTable("Project");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired();
            //Property(c => c.Number).IsRequired();
            Property(c => c.TemplateId).IsRequired();
            Property(c => c.CloudId).IsRequired();
            Property(c => c.StatusId).IsRequired();
            Property(c => c.OwnerId).IsRequired();
            Property(c => c.CompanyId).IsRequired();
            Ignore(c => c.OwnerContact);
            Ignore(c => c.OwnerName);
            Ignore(c => c.Deviation);
            Ignore(c => c.ConstructionScale);
            Ignore(c => c.Ignore);
            Ignore(c => c.Url);
           // Ignore(c => c.ProjClass);
            Ignore(c => c.ContractAmount);
            Property(c => c.ClassId).IsRequired();
        }
    }
}
