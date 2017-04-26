using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models
{
    public class CloudDto : EntityDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        //public bool Default { get; set; }

        //private ICollection<VaultTemplateDto> _templates;

        //public ICollection<VaultTemplateDto> Templates
        //{
        //    get { return _templates ?? (_templates = new List<VaultTemplateDto>()); }
        //    set { _templates = value; }
        //}
    }
}
