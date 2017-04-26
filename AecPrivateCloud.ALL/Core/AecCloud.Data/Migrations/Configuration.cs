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
//                'Assignment': { //����orָ��
//                    'TypeAlias': '10',
//                    'ClassAlias': '-100',
//                    'Properties': {
//                        'PropTaskTitle': 'PropTaskTitle', //�������,
//                        'PropTaskNum': 'PropTaskNum', //������
//                        'PropTaskDescript': '41', //����˵��
//                        'PropProject': 'PropProject', //��Ŀ
//                        'PropStartDate': 'PropStartDate', //��ʼ����
//                        'PropDeadLine': '42', //��ֹ����
//                        'PropAssignTo': '44', //�����
//                        'PropMonitor': '43', //�����
//                        'PropMarkedBy': '45', //��������
//                        'PropJobTime': 'PropJobTime', //Ԥ�ƹ�ʱ
//                        'PropDifficultyLevel': 'PropDifficultyLevel', //�����Ѷȵȼ�
//                        'PropProgressPercentage': 'PropProgressPercentage', //���Ȱٷֱ�
//                        'PropTaskDescriptDoc': 'PropTaskDescriptDoc', //˵���ĵ�
//                        'PropTaskFruitDoc': 'PropTaskFruitDoc', //�ɹ��ĵ�
//                        'Creator': '25' //������
//                    }
//                },
//
//                'Project': { //��Ŀ
//                    'TypeAlias': 'ObjProject',
//                    'ClassAlias': 'ClassProject',
//                    'Properties': {
//                        'PropProjName': 'PropProjName', //��Ŀ����
//                        'PropProjNum': 'PropProjNum', //��Ŀ���
//                        'PropProjPhase': 'PropProjPhase', //��Ŀ�׶�
//                        'PropDescription': 'PropDescription', //����
//                        'PropStartDate': 'PropStartDate', //��ʼ����
//                        'PropDeadLine': '42', //��ֹ����
//                        'PropProjPM': 'PropProjPM', //��Ŀ����
//                        'PropProjMembers': 'PropProjMembers', //��Ŀ��Ա(+)
//                        'PropProjProgress': 'PropProjProgress', //��Ŀ�������
//                        //'PropProgressState': 'PropProgressState' //��Ŀ��չ״̬
//                        'PropDesignUnit': 'PropDesignUnit', //���赥λ
//                        'PropBuilder': 'PropBuilder', //ʩ����λ
//                        'PropProjSupervisor': 'PropProjSupervisor', //���̼���
//                        'PropInvestSupervisor': 'PropInvestSupervisor' //Ͷ�ʼ���
//                    }
//                },
//
//	            'ValueLists': { //ֵ�б�
//		            'Items':{
//			            //'VLIdentity': 'VLIdentity', //���
//			            'VLDiscipline': 'VLDiscipline', //רҵ
//			            'VLProjPhase': 'VLProjPhase', //��Ŀ״̬(�׶�)
//			            //'VLProjProgressState': 'VLProjProgressState', //��Ŀ��չ״̬
//			            'VLDifficultyLevel': 'VLDifficultyLevel' //�����Ѷȵȼ�
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
//                new UserRole { Id = 1, Name=SystemUserRoleNames.Admins, Description="����Ա"}
//                , new UserRole { Id = 2, Name = SystemUserRoleNames.Guests, Description = "�ÿ�" }
//                , new UserRole { Id = 3, Name = SystemUserRoleNames.Registered, Description = "ע���û�" }
//                , new UserRole { Id = 4, Name = SystemUserRoleNames.Consumers, Description = "������" }
//                , new UserRole { Id = 5, Name = SystemUserRoleNames.Vendors, Description = "����" }
//                );

//            context.Set<ActiveDirectory>().AddOrUpdate(
//                ur => ur.Id,
//                new ActiveDirectory { Id = 1, Domain = "simuladesign", AdminName = "Administrator", AdminPwd = "admin", DCInfo = "DC=simuladesign,DC=com", LDAPRoot = "LDAP://192.168.2.189" }
//                );

//            var designPath = @"C:\Users\Administrator\Desktop\Эͬ�Ʊ���\20150204\{ADE61168-310E-4556-B9CB-610FF56769D3}_20150204_170355";
//            //var designCloudTemplate = context.Set<VaultTemplate>().Find(3);
//            //if (designCloudTemplate != null)
//            //{
//            //    designPath = designCloudTemplate.Path = designPath;
//            //}
//            var json = GetJson();
//            context.Set<VaultTemplate>().AddOrUpdate(
//                  ur => ur.Id,
//                  new VaultTemplate { Id = 1, Name = "�ҵ�����", Description = "�ҵ����̿�ģ��", Path = @"c:\vaults\CloudDrive.mfb" }
//                , new VaultTemplate { Id = 2, Name = "֪ʶ��", Description = "֪ʶ�ƿ�ģ��", Path = @"c:\vaults\KnowledgeSpace.mfb" }
//                , new VaultTemplate { Id = 3, Name = "ͨ����Ŀ", Description = "����һ����Ŀ�����ܣ��������г�����Ŀ", Path = designPath, MetadataJson = json }
//                , new VaultTemplate { Id = 4, Name = "��ת����Ŀ", Description = "�����ת�׹��ܣ�������ͼת��ͼ��Ŀ", Path = designPath, MetadataJson = json }
//                , new VaultTemplate { Id = 5, Name = "BIM��Ŀ", Description = "���BIM����/ģ�͹������ô���BIM��Ŀ", Path = designPath, MetadataJson = json }
//                );

//            context.Set<VaultServer>().AddOrUpdate(
//                    ur => ur.Id,
//                    new VaultServer { Id = 1, Ip = "192.168.2.129", LocalIp="192.168.2.129", Port = "2266", AdminName = "admin", AdminPwd = "111111", DomainId=1 }
//                );

//            var diskCloud = new Cloud {Id = 1, Name = "�ҵ�����", Description = "�ҵ�����", Version = "1.0", Default = true};
//            //diskCloud.Templates.Add(context.Set<VaultTemplate>().Find(1));
//            var knowledgeCloud = new Cloud
//            {
//                Id = 2,
//                Name = "֪ʶ��",
//                Description = "֪ʶ�ƣ����ʦ��֪ʶƽ̨",
//                Version = "1.0",
//                Default = true
//            };
//            //knowledgeCloud.Templates.Add(context.Set<VaultTemplate>().Find(2));

//            var designCloud = new Cloud
//            {
//                Id = 4,
//                Name = "Эͬ��",
//                Description = "Эͬ�ƣ����̽�����ҵ��Э��ƽ̨",
//                Version = "1.0",
//                Default = true
//            };
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(3));
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(4));
//            //designCloud.Templates.Add(context.Set<VaultTemplate>().Find(5));

//            context.Set<Cloud>().AddOrUpdate(
//                ur => ur.Id, diskCloud, knowledgeCloud, designCloud
//                , new Cloud { Id = 3, Name = "�����", Description = "����ƣ����ϵ����", Version = "1.0", Default = true }
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
//                  new ProjectStatus { Id = EfConfig.CreateProjectId, Name = "����", Description = "������Ŀ" }
//                , new ProjectStatus { Id = EfConfig.StartProjectId, Name = "����", Description = "��Ŀ����" }
//                , new ProjectStatus { Id = EfConfig.PauseProjectId, Name = "��ͣ", Description = "��Ŀ����ͣ" }
//                , new ProjectStatus { Id = EfConfig.EndProjectId, Name = "����", Description = "��Ŀ���" }
//                , new ProjectStatus { Id = EfConfig.ArchiveProjectId, Name = "�鵵", Description = "��Ŀ�ѹ鵵" }
//                );

//            context.Set<MFilesVault>().AddOrUpdate(
//                ur => ur.Id
//                , new MFilesVault { Id = 1, Name = "�ҵ�����", Description = "�ҵĴ洢�ռ�", Guid = "{F5150C33-4D98-4C7A-B40B-41772519198E}", ServerId = 1, ServerPath = "", TemplateId = 1, CreatedTimeUtc = DateTime.UtcNow }
//                , new MFilesVault { Id = 2, Name = "֪ʶ��", Description = "֪ʶ�ƣ����ʦ��֪ʶƽ̨", Guid = "{A681E219-D780-4DAD-9DAA-0AC542FAD0CD}", ServerId = 1, ServerPath = "", TemplateId = 2, CreatedTimeUtc = DateTime.UtcNow }
//                );
//        }
//    }
//}
