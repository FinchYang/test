using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Core.Domain.Projects
{
   
   
    public class Project : InternalEntity
    {
        public Project()
        {
            CostId = 1;
            LevelId = 1;
            CompanyId = 1;
            TimeLimitStatusId = 1;
            Ignore = false;
            Url = string.Empty;
            Deviation = string.Empty;
            OwnerContact = string.Empty;
        }
        public string Url { get; set; }
        public string Name { get; set; }

        public string Number { get; set; }

        public string Description { get; set; }

        public DateTime StartDateUtc { get; set; }

        public DateTime EndDateUtc { get; set; }

        public byte[] Cover { get; set; }
        /// <summary>
        /// 建设单位
        /// </summary>
        public string OwnerUnit { get; set; }
        /// <summary>
        /// 设计单位
        /// </summary>
        public string DesignUnit { get; set; }
        /// <summary>
        /// 施工单位
        /// </summary>
        public string ConstructionUnit { get; set; }
        public string PmUnit { get; set; }
        public string InvestigateUnit { get; set; }
        public string ConstructionScale { get; set; }
        public string ContractAmount { get; set; }

        
        /// <summary>
        /// 监理单位
        /// </summary>
        public string SupervisionUnit { get; set; }

        public long OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerContact { get; set; }
        public long TemplateId { get; set; }

        //public virtual VaultTemplate Template { get; set; }

        public bool Deleted { get; set; }

        public long StatusId { get; set; }

        public virtual ProjectStatus Status { get; set; }

        public long VaultId { get; set; }

        public virtual MFilesVault Vault { get; set; }

        public long PartyId { get; set; }

        //public virtual ProjectParty Party { get; set; }

        public long CloudId { get; set; }

        //public virtual Cloud Cloud { get; set; }

        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }

        public long AreaId { get; set; }

        public virtual Area Area { get; set; }

        public long CostId { get; set; }
        public virtual ProjectCostStatus Cost { get; set; }
        public long PlanCost { get; set; }
        public long ActualCost { get; set; }
        public long TimeLimitStatusId { get; set; }
        public virtual ProjectTimeLimitStatus TimeLimitStatus { get; set; }
        public string Deviation { get; set; }
        public bool Ignore { get; set; }
        public long LevelId { get; set; }
        public virtual ProjectLevel Level { get; set; }

      //  public string ProjClass { set; get; }

        public long ClassId { get; set; }


    }
}
