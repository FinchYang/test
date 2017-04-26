using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projRepo;
        private readonly IRepository<ProjectStatus> _statusRepo;
        private readonly IRepository<ProjectParty> _partyRepo;
        private readonly IRepository<ProjectProgressStatus> _progressRepo;
        private readonly IRepository<ProjectCostStatus> _ProjectCostStatus;
        private readonly IRepository<Area> _areaRepo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<ProjectLevel> _plevelRepository;
        private readonly IRepository<ProjectTimeLimitStatus> _ProjectTimeLimitStatus;
        private readonly IRepository<MFilesVault> _vaultRepository;
        public ProjectService(IRepository<Project> projRepo, IRepository<Area> areaRepo, IRepository<ProjectStatus> statusRepo, IRepository<ProjectParty> partyRepo
            , IRepository<ProjectProgressStatus> progressRepo, IRepository<ProjectTimeLimitStatus> ProjectTimeLimitStatus, IRepository<Company> companyRepo,
             IRepository<ProjectLevel> plevelRepository, IRepository<ProjectCostStatus> ProjectCostStatus, IRepository<MFilesVault> vaultRepository)
        {
            _companyRepo = companyRepo;
            _projRepo = projRepo;
            _statusRepo = statusRepo;
            _partyRepo = partyRepo;
            _progressRepo = progressRepo;
            _vaultRepository = vaultRepository;
            _areaRepo = areaRepo;
            _plevelRepository = plevelRepository;
            _ProjectCostStatus = ProjectCostStatus;
            _ProjectTimeLimitStatus = ProjectTimeLimitStatus;
        }

        public MFilesVault GetContractorVault()
        {
            return _vaultRepository.Table.FirstOrDefault(c => c.CloudId == 3);
        }
        public IEnumerable<ProjectCostStatus> GetAllCostStatus()
        {
            return _ProjectCostStatus.Table;
        }
        public IEnumerable<ProjectTimeLimitStatus> GetTimeStatus()
        {
            return _ProjectTimeLimitStatus.Table;
        }
        public IEnumerable<ProjectLevel> GetLevels()
        {
            return _plevelRepository.Table;
        }
        public IEnumerable<Company> GetAllCompany()
        {
            return _companyRepo.Table;
        }
      
        public void CreateProject(Project proj)
        {
            if (proj == null) throw new ArgumentNullException("proj");
            //因为创建库之后才有GUID，所以最后再加入数据库
            _projRepo.Insert(proj);
        }
        public IEnumerable<Area> GetAllArea()
        {
            return _areaRepo.Table;
        }
        public void UpdateProject(Project proj)
        {
            if (proj == null) throw new ArgumentNullException("proj");
            _projRepo.Update(proj);
        }

        public void DeleteProject(Project proj)
        {
            if (proj == null) throw new ArgumentNullException("proj");
            if (!proj.Deleted)
            {
                proj.Deleted = true;
                _projRepo.Update(proj);
            }
            //_projRepo.Delete(proj);
        }

        public ICollection<Project> GetAllProjects()
        {
            return _projRepo.Table.ToList();
        }

        public ICollection<Project> GetProjectsByCloud(long cloudId)
        {
            return _projRepo.Table.Where(c => c.CloudId == cloudId).ToList();
        }

        public Project GetProjectById(long projId)
        {
            return _projRepo.GetById(projId);
        }

        public Project GetProjectByVault(long vaultId)
        {
            return _projRepo.TableNoTracking.FirstOrDefault(p => p.VaultId == vaultId);
        }
        public ICollection<Project> GetProjectsByOwner(long ownerId)
        {
            return _projRepo.TableNoTracking.Where(c => c.OwnerId == ownerId).ToList();
        }

        public ICollection<ProjectStatus> GetStatuses()
        {
            return _statusRepo.Table.ToList();
        }

        public ProjectStatus GetStatus(long statusId)
        {
            if (statusId <= 0) return null;
            return _statusRepo.GetById(statusId);
        }

        public ProjectStatus GetStatusByName(string statusName)
        {
            if (String.IsNullOrWhiteSpace(statusName)) throw new ArgumentException("statusName");
            return _statusRepo.Table.FirstOrDefault(c => c.Name == statusName);
        }

        public void CreateStatus(ProjectStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            _statusRepo.Insert(status);
        }

        public void UpdateStatus(ProjectStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            _statusRepo.Update(status);
        }

        public void DeleteStatus(ProjectStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            _statusRepo.Delete(status);
        }


        public ICollection<ProjectParty> GetAllParties()
        {
            return _partyRepo.Table.ToList();
        }

        public ProjectParty GetPartyById(long partyId)
        {
            return _partyRepo.GetById(partyId);
        }

        public ProjectParty GetPartyByName(string partName)
        {
            return _partyRepo.Table.FirstOrDefault(c => c.Name == partName);
        }

        public void InsertParty(ProjectParty party)
        {
            if (party == null) throw new ArgumentNullException("party");
            _partyRepo.Insert(party);
        }

        public void UpdateParty(ProjectParty party)
        {
            if (party == null) throw new ArgumentNullException("party");
            _partyRepo.Update(party);
        }

        public void DeleteParty(ProjectParty party)
        {
            if (party == null) throw new ArgumentNullException("party");
            _partyRepo.Delete(party);
        }



        public ICollection<ProjectProgressStatus> GetProgress(long projId)
        {
            return _progressRepo.Table.Where(c => c.ProjectId == projId).ToList();
        }

        private static string GetMonth(DateTime month)
        {
            return month.ToString("yyyy-MM");
        }

        public void AddOrUpdateProgress(long projId, DateTime month, bool ok)
        {
            var mStr = GetMonth(month);
            var progress = _progressRepo.Table.FirstOrDefault(c => c.ProjectId == projId && mStr == c.Month);
            if (progress == null)
            {
                progress = new ProjectProgressStatus {ProjectId = projId, Month = mStr, OK = ok};
                _progressRepo.Insert(progress);
            }
            else
            {
                progress.OK = ok;
                UpdateProgress(progress);
            }
        }

        public void UpdateProgress(ProjectProgressStatus progress)
        {
            _progressRepo.Update(progress);
        }
    }
}
