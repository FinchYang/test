using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.ApiRequests;
using DBWorld.AecCloud.Web.Models;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Api
{
    //[Authorize]
    public abstract class ProjectBaseController : ErrorHandlingApiController
    {

        protected readonly IMfUserGroupService _mfusergroupService;
        protected readonly IProjectService _projectService;
        protected readonly IMFilesVaultService _mfvaultService;
        protected readonly IMFObjectService _mfilesObjService;
        protected readonly IVaultTemplateService _vaultTemplateService;

        protected ProjectBaseController(IProjectService projectService, IMFilesVaultService mfvaultService,
            IMfUserGroupService mfgroupService, IVaultTemplateService vaultTemplateService, IMFObjectService mfilesObjService,
            IAuthenticationManager authManager)
            : base(authManager)
        {
            _mfvaultService = mfvaultService;
            _mfusergroupService = mfgroupService;
            _projectService = projectService;
            _vaultTemplateService = vaultTemplateService;
            _mfilesObjService = mfilesObjService;
        }

        protected internal MFilesVault GetVault(Project proj)
        {
            return GetVault(_mfvaultService, proj); // _mfvaultService.GetVaultById(proj.VaultId);
        }

        protected static MFilesVault GetVault(IMFilesVaultService mfvaultService, Project proj)
        {
            return mfvaultService.GetVaultById(proj.VaultId);
        }

        protected internal VaultTemplate GetTemplateByTempId(long templateId)
        {
            return GetTemplateByTempId(_vaultTemplateService, templateId); //_vaultTemplateService.GetTemplateById(templateId);
        }

        protected static VaultTemplate GetTemplateByTempId(IVaultTemplateService vaultTemplateService, long tempId)
        {
            return vaultTemplateService.GetTemplateById(tempId);
        }

        protected internal async Task<ICollection<VaultTemplate>> GetTemplatesNew(long cloudId)
        {
            //var scope = Request.GetOwinContext().GetAutofacLifetimeScope();
            var templates = await Task.Run(() => HomeClient.GetProjectTemplates(_vaultTemplateService)); // HomeClient.GetProjectTemplatesNew(scope); 
            //var templates = JsonConvert.DeserializeObject<List<VaultTemplate>>(templatesRes.Content);
            return templates;
        }
        protected static void CreateMfObj<T>(MFilesVault vault, MetadataAliases aliases, T entity, IMFObjectService mfilesObjService) where T : InternalEntity
        {
            mfilesObjService.Create(vault, aliases, entity);
        }

        protected static void UpdateMfObj<T>(MFilesVault vault, MetadataAliases aliases, T entity, IMFObjectService mfilesObjService) where T : InternalEntity
        {
            mfilesObjService.Update(vault, aliases, entity);
        }

        protected static MFProjectParty GetMfParty(MFilesVault vault, MetadataAliases aliases, string partyName, IMFObjectService mfilesObjService, int? currentUserId = null)
        {
            return mfilesObjService.GetParty(vault, partyName, aliases, currentUserId);
        }

        protected MfContact GetMfContact(MFilesVault vault, MetadataAliases aliases, int contactId)
        {
            return _mfilesObjService.GetContactByContactId(vault, aliases, contactId);
        }

        protected MfContact GetMfContactByUserId(MFilesVault vault, MetadataAliases aliases, int userId)
        {
            return _mfilesObjService.GetContactByUserId(vault, aliases, userId);
        }

        /// <summary>
        /// 是否为创建项目的参与方的项目经理
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
        protected static internal bool IsCreateProjectManager(MFilesVault vault, string userName, IMfUserGroupService mfusergroupService, IVaultTemplateService vautTempService)
        {
            //var userName = GetUserName();
            var mgrGroup = ProjectRoleConstants.ProjectManager;
            var groups = mfusergroupService.GetGroupsByUser(vault, userName);
            var template = GetTemplateByTempId(vautTempService, vault.TemplateId);
            var tempDto = template.ToDto();
            if (tempDto.HasParty)
            {
                return groups.Any(c => c.Name == mgrGroup) && groups.Any(c => c.Name == "项目创建者");
            }
            return groups.Any(c => c.Name == mgrGroup);
        }
        /// <summary>
        /// 是否为项目经理角色，不限参与方
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
        protected internal MFilesUserGroup GetUserProjectManagerGroup(MFilesVault vault)
        {
            var userName = GetUserName();
            var groups = _mfusergroupService.GetGroupsByUser(vault, userName);
            var isManager = groups.FirstOrDefault(c => c.Name.Contains(ProjectRoleConstants.ProjectManager));
            return isManager;
        }
    }

    public class ProjectResult
    {
        public string Error { get; set; }

        public Exception Exception { get; set; }

        public ProjectDto Project { get; set; }
    }
}
