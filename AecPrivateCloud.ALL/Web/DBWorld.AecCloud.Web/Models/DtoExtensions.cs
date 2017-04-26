using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using AecCloud.BaseCore;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;
using AecCloud.Service;
using AecCloud.WebAPI.Models;
using log4net;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Models
{
    public class UserPrivate
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public static class EmailExtensions
    {
        public static EmailSendingModel ToSendingModel(this SendEmailMessage message)
        {
            if (message == null) return null;
            Encoding encoding = new UTF8Encoding();
            if (!String.IsNullOrWhiteSpace(message.EncodingName))
            {
                encoding = Encoding.GetEncoding(message.EncodingName);
            }
            return new EmailSendingModel
            {
                MailTo = message.MailTo,
                Title = message.Title,
                Body = message.Body,
                IsHtml = message.IsHtml,
                TextEncoding = encoding
            };
        }
    }
    public static class DtoExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static AreaDto ToDto(this Area a)
        {
            if (a == null) return null;
            return new AreaDto
            {
                Id = a.Id,
                Name = a.Name
            };
        }

        public static CompanyDto ToDto(this Company c)
        {
            if (c == null) return null;
            return new CompanyDto
            {
                Id = c.Id,
                Name = c.Name
            };
        }

        public static ProjectLevelDto ToDto(this ProjectLevel l)
        {
            if (l == null) return null;
            return new ProjectLevelDto
            {
                Id = l.Id,
                Name = l.Name
            };
        }

        public static ActiveDirectoryDto ToDto(this ActiveDirectory ad)
        {
            if (ad == null) return null;
            return new ActiveDirectoryDto
            {
                Domain = ad.Domain,
                Id = ad.Id
            };
        }

        public static CloudDto ToDto(this Cloud app)
        {
            if (app == null) return null;
            return new CloudDto
            {
                //Default = app.Default,
                Description = app.Description,
                Id = app.Id,
                Name = app.Name,
                //Templates = app.Templates.Select(c=>c.ToDto()).ToList(),
                Version = app.Version
            };
        }

        public static VaultServerDto ToDto(this VaultServer server)
        {
            if (server == null) return null;
            return new VaultServerDto
            {
                Id = server.Id,
                Ip = server.Ip,
                LocalIp = server.LocalIp,
                Port = server.Port
            };
        }

        public static VaultTemplateDto ToDto(this VaultTemplate vt)
        {
            if (vt == null) return null;
          //  var metaData = JsonConvert.DeserializeObject<MetadataAliases>(vt.MetadataJson);
          //  var hasParty = metaData.Objects.ContainsKey("ObjParticipant");
            return new VaultTemplateDto
            {
                Description = vt.Description,
                Id = vt.Id,
                Name = vt.Name,
                HasParty = false,
                ImageUrl = vt.ImageUrl
                //StructureFile = vt.StructureFile
            };
        }

        public static VaultDto ToDtoWithoutTemplate(this MFilesVault vault)
        {
            if (vault == null) return null;
            return new VaultDto
            {
                CreatedTimeUtc = vault.CreatedTimeUtc,
                Description = vault.Description,
                Guid = vault.Guid,
                Id = vault.Id,
                Name = vault.Name,
                Server = ToDto(vault.Server)
            };
        }

        public static UserRoleDto ToDto(this UserRole role)
        {
            if (role == null) return null;
            return new UserRoleDto
            {
                Description = role.Description,
                Name = role.Name,
                Id = role.Id,
                DisplayName = role.DisplayName
            };
        }
      
        public static UserDto ToDto(this User user)
        {
            if (user == null) return null;
            var dto = new UserDto
            {
                Id = user.Id,
                Domain = user.Domain,
                UserName = user.UserName,
                Email = user.Email
            };
            dto.Image = user.Image;
            dto.FullName = user.FullName;
            dto.Description = user.Description;
            dto.Industry = user.Industry;
            dto.Phone = user.Phone;
            dto.QQ = user.QQ;
            dto.Post = user.Post;

            if (user.Company != null)
            {
                dto.Company = user.Company.Name;
            }
            if (user.Department != null)
            {
                dto.Department = user.Department.Name;
            }
           

            foreach (var r in user.Roles)
            {
               
                var rd = ToDto(r);
                if (rd != null) dto.Roles.Add(rd);
            }
           
            return dto;
        }

        public static ProjectDto ToDto(this Project proj, bool hasParty)
        {
            if (proj == null) return null;
            return ToDto(proj, null, hasParty);
        }

        public static ProjectDto ToDto(this Project proj, MFilesVault vault, bool hasParty)
        {
            if (proj == null) return null;
            var projdto = new ProjectDto();
            try
            {
              //  Log.Info("id");
                projdto.Id = proj.Id;
                projdto.Name = proj.Name;
                projdto.Number = proj.Number;
                projdto.Description = proj.Description;
                projdto.Cover = proj.Cover;
                projdto.TemplateId = proj.TemplateId;
                // Template = proj.Template.ToDto(),
              //  Log.Info("Status");
                projdto.Status = proj.Status.ToDto();
                projdto.VaultId = proj.VaultId;
                projdto.Vault = vault.ToDtoWithoutTemplate();
                //Cloud = proj.Cloud.ToDto(),
             //   Log.Info("StartDateUtc");
                projdto.StartDateUtc = proj.StartDateUtc;
                projdto.EndDateUtc = proj.EndDateUtc;
                projdto.OwnerUnit = proj.OwnerUnit;
                projdto.ConstructionUnit = proj.ConstructionUnit;
                projdto.DesignUnit = proj.DesignUnit;
                projdto.SupervisionUnit = proj.SupervisionUnit;
                projdto.InvestigateUnit = proj.InvestigateUnit;
                projdto.PmUnit = proj.PmUnit;
             //   Log.Info("HasParty");
                projdto.HasParty = hasParty;
            //    Log.Info("ProjectClass");
             //   projdto.ProjectClass = proj.ProjClass;
             //   Log.Info("Company");
                projdto.Company = proj.Company.Name;
           //     Log.Info("ContractAmount");
                projdto.ContractAmount = proj.ContractAmount;
            //    Log.Info("ConstructionScale");
                projdto.ConstructionScale = proj.ConstructionScale;
            //    Log.Info("Area");
                projdto.Area = proj.Area.Name;
            //    Log.Info("Level");
                projdto.Level = proj.Level.Name;
            //    Log.Info("CompanyId");
                projdto.CompanyId = proj.CompanyId;
            //    Log.Info("Cost");
                if (proj.Cost != null)
                {
                    projdto.Cost = proj.Cost.ToDto();
                }
            //    Log.Info("??");
            }
            catch (Exception ex)
            {
                Log.Info("  public static ProjectDto ToDto error:"+ex.Message);
            }
            return projdto;
        }

        public static ProjectCostDto ToDto(this ProjectCostStatus cost)
        {
            if (cost == null) return null;
            return new ProjectCostDto {Id = cost.Id, Name = cost.Name};
        }

        public static ProjectProgressDto ToDto(this ProjectProgressStatus progress)
        {
            if (progress == null) return null;
            return new ProjectProgressDto {Id = progress.Id, ProjId = progress.ProjectId, Month = progress.Month, OK =progress.OK};
        }

        public static ProjectPartyDto ToDto(this ProjectParty party)
        {
            if (party == null) return null;
            return new ProjectPartyDto
            {
                Id = party.Id,
                Name = party.Name,
                Description = party.Description,
                Alias = party.Alias
            };
        }

        public static ProjectInvitationDto ToDto(this ProjectInvitation invitation)
        {
            if (invitation == null) return null;
            return new ProjectInvitationDto
            {
                Id = invitation.Id,
                Accepted = invitation.Accepted,
                InvitationMessage = invitation.InvitationMessage,
                InviteeConfirmMessage = invitation.InviteeConfirmMessage,
                InviteeEmail = invitation.InviteeEmail,
                InviteeId = invitation.InviteeId,
                InviteePartId = invitation.InviteePartId,
                InviterId = invitation.InviterId,
                ProjectId = invitation.ProjectId
            };
        }

        public static ProjectStatusDto ToDto(this ProjectStatus status)
        {
            if (status == null) return null;
            return new ProjectStatusDto
            {
                Id = status.Id,
                Name = status.Name,
                Description = status.Description
            };
        }

        public static ProjectMemberDto ToDto(this ProjectMember member)
        {
            if (member == null) return null;
            return new ProjectMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                ProjectId = member.ProjectId
            };
        }

        public static MFilesUserGroupDto ToDto(this MFilesUserGroup userGroup)
        {
            if (userGroup == null) return null;
            return new MFilesUserGroupDto
            {
                Id = userGroup.Id,
                Name = userGroup.Name,
                Alias = userGroup.Alias
            };
        }
    }
}