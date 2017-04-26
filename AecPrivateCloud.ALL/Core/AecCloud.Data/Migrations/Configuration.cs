//using AecCloud.Core.Domain.Projects;
//using AecCloud.Core.Domain.Vaults;

//namespace AecCloud.Data.Migrations
//{
//    using System;
//    using System.Data.Entity;
//    using System.Data.Entity.Migrations;
//    using System.Linq;
//    using System.Reflection;
//    using System.IO;
//    using AecCloud.Core.Domain;

//    public sealed class Configuration : DbMigrationsConfiguration<EntityContext>
//    {
//        public Configuration()
//        {
//            AutomaticMigrationsEnabled = false;
//        }

//        private string GetJson()
//        {
//            return @"{
//                'Assignment': { //任务or指派
//                    'TypeAlias': '10',
//                    'ClassAlias': '-100',
//                    'Properties': {
//                        'PropTaskTitle': 'PropTaskTitle', //任务标题,
//                        'PropTaskNum': 'PropTaskNum', //任务编号
//                        'PropTaskDescript': '41', //任务说明
//                        'PropProject': 'PropProject', //项目
//                        'PropStartDate': 'PropStartDate', //开始日期
//                        'PropDeadLine': '42', //截止日期
//                        'PropAssignTo': '44', //分配给
//                        'PropMonitor': '43', //监控人
//                        'PropMarkedBy': '45', //被标记完成
//                        'PropJobTime': 'PropJobTime', //预计工时
//                        'PropDifficultyLevel': 'PropDifficultyLevel', //任务难度等级
//                        'PropProgressPercentage': 'PropProgressPercentage', //进度百分比
//                        'PropTaskDescriptDoc': 'PropTaskDescriptDoc', //说明文档
//                        'PropTaskFruitDoc': 'PropTaskFruitDoc', //成果文档
//                        'Creator': '25' //创建者
//                    }
//                },
//
//                'Project': { //项目
//                    'TypeAlias': 'ObjProject',
//                    'ClassAlias': 'ClassProject',
//                    'Properties': {
//                        'PropProjName': 'PropProjName', //项目名称
//                        'PropProjNum': 'PropProjNum', //项目编号
//                        'PropProjPhase': 'PropProjPhase', //项目阶段
//                        'PropDescription': 'PropDescription', //描述
//                        'PropStartDate': 'PropStartDate', //开始日期
//                        'PropDeadLine': '42', //截止日期
//                        'PropProjPM': 'PropProjPM', //项目经理
//                        'PropProjMembers': 'PropProjMembers', //项目成员(+)
//                        'PropProjProgress': 'PropProjProgress', //项目总体进度
//                        //'PropProgressState': 'PropProgressState' //项目进展状态
//                        'PropDesignUnit': 'PropDesignUnit', //建设单位
//                        'PropBuilder': 'PropBuilder', //施工单位
//                        'PropProjSupervisor': 'PropProjSupervisor', //工程监理
//                        'PropInvestSupervisor': 'PropInvestSupervisor' //投资监理
//                    }
//                },
//
//	            'ValueLists': { //值列表
//		            'Items':{
//			            //'VLIdentity': 'VLIdentity', //身份
//			            'VLDiscipline': 'VLDiscipline', //专业
//			            'VLProjPhase': 'VLProjPhase', //项目状态(阶段)
//			            //'VLProjProgressState': 'VLProjProgressState', //项目进展状态
//			            'VLDifficultyLevel': 'VLDifficultyLevel' //任务难度等级
//		            }
//                }
//            }";
//        }

//        protected override void Seed(EntityContext context)
//        {
//            //  This method will be called after migrating to the latest version.

//            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
//            //  to avoid creating duplicate seed data. E.g.
//            //
//            //    context.People.AddOrUpdate(
//            //      p => p.FullName,
//            //      new Person { FullName = "Andrew Peters" },
//            //      new Person { FullName = "Brice Lambson" },
//            //      new Person { FullName = "Rowan Miller" }
//            //    );
//            //
//            context.Set<UserRole>().AddOrUpdate(
//                ur=>ur.Id,
//                new UserRole { Id = 1, Name=SystemUserRoleNames.Admins, Description="管理员"}
//                , new UserRole { Id = 2, Name = SystemUserRoleNames.Guests, Description = "访客" }
//                , new UserRole { Id = 3, Name = SystemUserRoleNames.Registered, Description = "注册用户" }
//                , new UserRole { Id = 4, Name = SystemUserRoleNames.Consumers, Description = "消费者" }
//                , new UserRole { Id = 5, Name = SystemUserRoleNames.Vendors, Description = "卖主" }
//                );

//            context.Set<ActiveDirectory>().AddOrUpdate(
//                ur => ur.Id,
//                new ActiveDirectory { Id = 1, Domain = "simuladesign", AdminName = "Administrator", AdminPwd = "admin", DCInfo = "DC=simuladesign,DC=com", LDAPRoot = "LDAP://192.168.2.189" }
//                );

//            var designPath = @"C:\Users\Administrator\Desktop\协同云备份\20150204\{ADE61168-310E-4556-B9CB-610FF56769D3}_20150204_170355";
//            //var designCloudTemplate = context.Set<VaultTemplate>().Find(3);
//            //if (designCloudTemplate != null)
//            //{
//            //    designPath = designCloudTemplate.Path = designPath;
//            //}
//            var json = GetJson();
//            context.Set<VaultTemplate>().AddOrUpdate(
//                  ur => ur.Id,
//                  new VaultTemplate { Id = 1, Name = "我的云盘", Description = "我的云盘库模版", Path = @"c:\vaults\CloudDrive.mfb" }
//                , new VaultTemplate { Id = 2, Name = "知识云", Description = "知识云库模版", Path = @"c:\vaults\KnowledgeSpace.mfb" }
//                , new VaultTemplate { Id = 3, Name = "通用项目", Description = "包含一般项目管理功能，适用所有常规项目", Path = designPath, MetadataJson = json }
//                , new VaultTemplate { Id = 4, Name = "蓝转白项目", Description = "兼顾蓝转白功能，适用蓝图转白图项目", Path = designPath, MetadataJson = json }
//                , new VaultTemplate { Id = 5, Name = "BIM项目", Description = "兼顾BIM构件/模型管理，适用大型BIM项目", Path = designPath, MetadataJson = json }
//                );

//            context.Set<VaultServer>().AddOrUpdate(
//                    ur => ur.Id,
//                    new VaultServer { Id = 1, Ip = "192.168.2.129", LocalIp="192.168.2.129", Port = "2266", AdminName = "admin", AdminPwd = "111111", DomainId=1 }
//                );

//            var diskCloud = new Cloud {Id = 1, Name = "我的云盘", Description = "我的云盘", Version = "1.0", Default = true};
//            //diskCloud.Templates.Add(context.Set<VaultTemplate>().Find(1));
//            var knowledgeCloud = new Cloud
//            {
//                Id = 2,
//                Name = "知识云",
//                Description = "知识云，设计师的知识平台",
//                Version = "1.0",
//                Default = true
//            };
//            //knowledgeCloud.Templates.Add(context.Set<VaultTemplate>().Find(2));

//            var designCloud = new Cloud
//            {
//                Id = 4,
//                Name = "协同云",
//                Description = "协同云，工程建设行业的协作平台",
//                Version = "1.0",
//                Default = true
//            };
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(3));
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(4));
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(5));

//            context.Set<Cloud>().AddOrUpdate(
//                ur => ur.Id, diskCloud, knowledgeCloud, designCloud
//                , new Cloud { Id = 3, Name = "软件云", Description = "软件云，云上的软件", Version = "1.0", Default = true }
//                );

//            context.Set<CloudVaultTemplate>().AddOrUpdate(
//                ur=>ur.Id,
//                new CloudVaultTemplate { Id = 1, CloudId = 1, VaultTemplateId = 1},
//                new CloudVaultTemplate { Id = 2, CloudId = 2, VaultTemplateId = 2},
//                new CloudVaultTemplate { Id = 3, CloudId = 4, VaultTemplateId = 3},
//                new CloudVaultTemplate { Id = 4, CloudId = 4, VaultTemplateId = 4},
//                new CloudVaultTemplate { Id = 5, CloudId = 4, VaultTemplateId = 5}
//                );

//            context.Set<ProjectStatus>().AddOrUpdate(
//                ur => ur.Id,
//                  new ProjectStatus { Id = EfConfig.CreateProjectId, Name = "立项", Description = "成立项目" }
//                , new ProjectStatus { Id = EfConfig.StartProjectId, Name = "启动", Description = "项目启动" }
//                , new ProjectStatus { Id = EfConfig.PauseProjectId, Name = "暂停", Description = "项目被暂停" }
//                , new ProjectStatus { Id = EfConfig.EndProjectId, Name = "结束", Description = "项目完成" }
//                , new ProjectStatus { Id = EfConfig.ArchiveProjectId, Name = "归档", Description = "项目已归档" }
//                );

//            context.Set<MFilesVault>().AddOrUpdate(
//                ur => ur.Id
//                , new MFilesVault { Id = 1, Name = "我的云盘", Description = "我的存储空间", Guid = "{F5150C33-4D98-4C7A-B40B-41772519198E}", ServerId = 1, ServerPath = "", TemplateId = 1, CreatedTimeUtc = DateTime.UtcNow }
//                , new MFilesVault { Id = 2, Name = "知识云", Description = "知识云，设计师的知识平台", Guid = "{A681E219-D780-4DAD-9DAA-0AC542FAD0CD}", ServerId = 1, ServerPath = "", TemplateId = 2, CreatedTimeUtc = DateTime.UtcNow }
//                );
//        }
//    }
//}
