using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.Data;
using AecCloud.WebAPI.Models;
using log4net;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web
{
    public sealed class Configuration : DbMigrationsConfiguration<EntityContext>
    {
        private readonly string _appdataFolder;

        public Configuration(string appdataFolder)
        {
            _appdataFolder = appdataFolder;
            AutomaticMigrationsEnabled = false;
        }
        /// <summary>
        /// This method will be called after migrating to the latest version.
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(EntityContext context)
        {
            EfConfig.AddOrUpdateData(context, _appdataFolder);
        }
    }

    public class EfConfig
    {
        private static readonly bool IsFirstTimeStart = bool.Parse(ConfigurationManager.AppSettings["FirstTimeStart"]);

        internal static bool AddOrUpdateData(EntityContext context, string appdataFolder)
        {
            context.Set<UserRole>().AddOrUpdate(ur => ur.Id
                , new UserRole { Id = 1, Name = SystemUserRoleNames.Registered, DisplayName = "注册用户", Description = "一般注册用户" }
                , new UserRole { Id = 2, Name = SystemUserRoleNames.Admins, DisplayName="管理员", Description = "系统管理员" }
                , new UserRole { Id = 3, Name = SystemUserRoleNames.CorperationLeaders, DisplayName="集团领导", Description = "集团领导，可以查看各个项目的情况" }
                , new UserRole { Id = 4, Name = SystemUserRoleNames.ProjectDirectors, DisplayName="项目总监", Description = "可以创建项目" }
                , new UserRole { Id = 5, Name = SystemUserRoleNames.SubContractors, DisplayName = "分包管理", Description = "管理分包商的数据" }
                );

            var myProjCloud = new Cloud { Id = CloudConstants.MyProjects, Name = "我的项目", Description = "我的项目", Version = "1.0", Default = true };
            var projManageCloud = new Cloud { Id = CloudConstants.ProjManagements, Name = "项目管理", Description = "项目管理，多项目信息汇总管理", Version = "1.0", Default = false };
            var subconCloud = new Cloud { Id = CloudConstants.SubContracts, Name = "分包商管理", Description = "分包商信息管理", Version = "1.0", Default = false };

            context.Set<Cloud>().AddOrUpdate(ur => ur.Id, myProjCloud, projManageCloud, subconCloud);

            context.Set<ProjectStatus>().AddOrUpdate(
            ur => ur.Id,
              new ProjectStatus { Id = ProjectStatusConstants.CreateProjectId, Name = "立项", Description = "成立项目" }
            , new ProjectStatus { Id = ProjectStatusConstants.StartProjectId, Name = "启动", Description = "项目启动" }
            , new ProjectStatus { Id = ProjectStatusConstants.PauseProjectId, Name = "终止", Description = "项目被终止" }
            , new ProjectStatus { Id = ProjectStatusConstants.EndProjectId, Name = "结束", Description = "项目完成" }
            );

            context.Set<ProjectParty>().AddOrUpdate(
                ur => ur.Id
                    , new ProjectParty { Id = 1, Name = "设计方", Description = "建筑、结构、机电等设计，一般为设计院" }
                    , new ProjectParty { Id = 2, Name = "建设方", Description = "业主方" }
                    , new ProjectParty { Id = 3, Name = "施工方", Description = "施工方" }
                    , new ProjectParty { Id = 4, Name = "分包商", Description = "分包商" }
                    , new ProjectParty { Id = 5, Name = "供应商", Description = "供应商" }
                    , new ProjectParty { Id = 6, Name = "咨询方", Description = "专业或专项咨询方" }
                    , new ProjectParty { Id = 7, Name = "监理方", Description = "监理方" }
                );

            var costJson = File.ReadAllText(Path.Combine(appdataFolder, "projectCost.json"));
            var costs = JsonConvert.DeserializeObject<ProjectCostStatus[]>(costJson);
            context.Set<ProjectCostStatus>().AddOrUpdate(ur => ur.Id, costs);

           

            var companyJson = File.ReadAllText(Path.Combine(appdataFolder, "company.json"));
            var companies = JsonConvert.DeserializeObject<Company[]>(companyJson);
            context.Set<Company>().AddOrUpdate(ur => ur.Id, companies);

            var areaJson = File.ReadAllText(Path.Combine(appdataFolder, "area.json"));
            var areas = JsonConvert.DeserializeObject<Area[]>(areaJson);
            context.Set<Area>().AddOrUpdate(ur => ur.Id, areas);

            var ProjectLevelJson = File.ReadAllText(Path.Combine(appdataFolder, "projectlevel.json"));
            var ProjectLevels = JsonConvert.DeserializeObject<ProjectLevel[]>(ProjectLevelJson);
            context.Set<ProjectLevel>().AddOrUpdate(ur => ur.Id, ProjectLevels);

            var ProjectTimeLimitStatusJson = File.ReadAllText(Path.Combine(appdataFolder, "projecttimelimit.json"));
            var ProjectTimeLimitStatuss = JsonConvert.DeserializeObject<ProjectTimeLimitStatus[]>(ProjectTimeLimitStatusJson);
            context.Set<ProjectTimeLimitStatus>().AddOrUpdate(ur => ur.Id, ProjectTimeLimitStatuss);

            var departmentJson = File.ReadAllText(Path.Combine(appdataFolder, "department.json"));
            var departments = JsonConvert.DeserializeObject<Department[]>(departmentJson);
            context.Set<Department>().AddOrUpdate(ur => ur.Id, departments);

            if (!context.Set<MFilesVault>().Any())
            {
                var vaultsJson = File.ReadAllText(Path.Combine(appdataFolder, "vaults.json"));
                var vaults = JsonConvert.DeserializeObject<List<MFilesVault>>(vaultsJson);
                context.Set<MFilesVault>().AddOrUpdate(ur => ur.Id, vaults.ToArray());
            }
            if (!context.Set<Project>().Any())
            {
                var vaultsJson = File.ReadAllText(Path.Combine(appdataFolder, "projects.json"));
                var vaults = JsonConvert.DeserializeObject<List<Project>>(vaultsJson);
                context.Set<Project>().AddOrUpdate(ur => ur.Id, vaults.ToArray());
            }

            if (!context.Set<VaultServer>().Any())
            {
                var vsJson = File.ReadAllText(Path.Combine(appdataFolder, "vaultserver.json"));
                var servers = JsonConvert.DeserializeObject<List<VaultServer>>(vsJson);
                context.Set<VaultServer>().AddOrUpdate(ur => ur.Id, servers.ToArray());
            }

            if (!context.Set<VaultTemplate>().Any())
            {
                var templateJson = File.ReadAllText(Path.Combine(appdataFolder, "vaultTemplates.json"));
                var templates = JsonConvert.DeserializeObject<List<VaultTemplate>>(templateJson);
                context.Set<VaultTemplate>().AddOrUpdate(ur => ur.Id, templates.ToArray());

                var appJson = File.ReadAllText(Path.Combine(appdataFolder, "vaultApps.json"));
                var apps = JsonConvert.DeserializeObject<List<VaultApp>>(appJson);
                context.Set<VaultApp>().AddOrUpdate(ur => ur.Id, apps.ToArray());

                var tempAppJson = File.ReadAllText(Path.Combine(appdataFolder, "vaultApp_template.json"));
                var tempApps = JsonConvert.DeserializeObject<List<VaultAppVaultTemplate>>(tempAppJson);
                context.Set<VaultAppVaultTemplate>().AddOrUpdate(ur => ur.Id, tempApps.ToArray());
            }

            return true;
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void AddData(string appdataFolder)
        {
            //初次运行，后台无数据库时，注释掉该行
            //System.Data.Entity.Database.SetInitializer<EntityContext>(null);
            
            try
            {
                using (var context = new EntityContext())
                {
                    var ok = AddOrUpdateData(context, appdataFolder);
                    context.SaveChanges();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errList = new List<string>();
                var res = ex.EntityValidationErrors.ToArray();
                foreach (var r in res)
                {
                    errList.AddRange(r.ValidationErrors.Select(c => c.PropertyName + " # " + c.ErrorMessage));
                }
                Log.Error("导入数据出错：" + String.Join(Environment.NewLine, errList));
                throw;
            }
        }

        public static IReadOnlyList<ProjectRole> Roles { get; private set; }


        public static void Initialize(string appdataFolder)
        {
            if (IsFirstTimeStart) //首次，需要初始化数据库
            {
                AddData(appdataFolder);
            }
            else
            {
                Database.SetInitializer<EntityContext>(null);
            }
            
            //RunMigrations(appdataFolder);
        }

        private static void RunMigrations(string appdataFolder)
        {
            var efMigrationSettings = new Configuration(appdataFolder)
            {
                AutomaticMigrationsEnabled = true,
                AutomaticMigrationDataLossAllowed = true
            };
            var efMigrator = new DbMigrator(efMigrationSettings);
            try
            {
                efMigrator.Update();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var res = ex.EntityValidationErrors.ToArray();
                foreach (var r in res)
                {

                }
            }
        }
    }
}