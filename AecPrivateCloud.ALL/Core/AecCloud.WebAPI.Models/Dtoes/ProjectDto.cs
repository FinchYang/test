using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class ProjectDto : EntityDto
    {
        public bool NotDisplay = false;
        public string Url { get; set; }
        public string Name { get; set; }

        public string Number { get; set; }
        public string ProjectClass { get; set; }

        public string Description { get; set; }

        public DateTime StartDateUtc { get; set; }

        public DateTime EndDateUtc { get; set; }

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
        /// <summary>
        /// 监理单位
        /// </summary>
        public string InvestigateUnit { get; set; }
        public string PmUnit { get; set; }
        public string SupervisionUnit { get; set; }

        public byte[] Cover { get; set; }

        public long VaultId { get; set; }

        public VaultDto Vault { get; set; }

        public long TemplateId { get; set; }

        //public VaultTemplateDto Template { get; set; }

        public ProjectStatusDto Status { get; set; }

        public long CompanyId { get; set; }
        public string Company { get; set; }
        public string ContractAmount { get; set; }
        public string ConstructionScale { get; set; }
        
        public long AreaId { get; set; }
        public string Area { get; set; }
        public string Level { get; set; }

        public bool HasParty { get; set; }

        public ProjectCostDto Cost { get; set; }

        //public CloudDto Cloud { get; set; }

        private readonly List<ProjectProgressDto> _progresses = new List<ProjectProgressDto>();

        public List<ProjectProgressDto> Progresses
        {
            get { return _progresses; }
        }
    }
}
