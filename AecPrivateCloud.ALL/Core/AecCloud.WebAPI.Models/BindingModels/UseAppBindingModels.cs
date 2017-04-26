using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AecCloud.WebAPI.Models
{
    public class UserCloudModel
    {
        public List<CloudModel> Apps { get; private set; }

        public UserCloudModel()
        {
            Apps = new List<CloudModel>();
        }
    }


    public class CloudModel
    {
        public string Url { get; set; }
        public CloudDto App { get; set; }

        public List<VaultDto> Vaults { get; private set; }

        public CloudModel()
        {
            Vaults = new List<VaultDto>();
            Projects = new List<ProjectDto>();
        }

        public List<ProjectDto> Projects { get; private set; }
    }
}