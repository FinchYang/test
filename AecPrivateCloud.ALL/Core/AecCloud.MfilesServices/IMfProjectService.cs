using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.MfilesServices
{
    public interface IMfProjectService
    {
        Result Create(Project proj, User creator, VaultTemplate template, MFilesVault vault, MFSqlDatabase sqlDb, string userName, string password,
            ProjectParty party);
        
    }
}
