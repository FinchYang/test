using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Projects
{
    public interface IProjectService
    {
        ICollection<Project> GetAllProjects();
        IEnumerable<Area> GetAllArea();
        ICollection<Project> GetProjectsByCloud(long cloudId);

        Project GetProjectById(long projId);

        Project GetProjectByVault(long vaultId);

        ICollection<Project> GetProjectsByOwner(long ownerId);
         
        void CreateProject(Project proj);

        void UpdateProject(Project proj);

        void DeleteProject(Project proj);


        ICollection<ProjectStatus> GetStatuses();

        ProjectStatus GetStatus(long statusId);

        ProjectStatus GetStatusByName(string statusName);

        void CreateStatus(ProjectStatus status);

        void UpdateStatus(ProjectStatus status);

        void DeleteStatus(ProjectStatus status);

        ICollection<ProjectParty> GetAllParties();
        IEnumerable<Company> GetAllCompany();
         IEnumerable<ProjectLevel> GetLevels();
        IEnumerable<ProjectTimeLimitStatus> GetTimeStatus();
        IEnumerable<ProjectCostStatus> GetAllCostStatus();
        MFilesVault GetContractorVault();
        ProjectParty GetPartyById(long partyId);

        ProjectParty GetPartyByName(string partName);

        void InsertParty(ProjectParty party);

        void UpdateParty(ProjectParty party);

        void DeleteParty(ProjectParty party);

        ICollection<ProjectProgressStatus> GetProgress(long projId);

        void AddOrUpdateProgress(long projId, DateTime month, bool ok);

        void UpdateProgress(ProjectProgressStatus progress);
    }
}
