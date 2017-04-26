using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Service.Projects
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IRepository<ProjectMember> _contactRepo;
        private readonly IRepository<ProjectInvitation> _invitationRepo;

        public ProjectMemberService(IRepository<ProjectMember> contactRepo, IRepository<ProjectInvitation> invitationRepo)
        {
            _contactRepo = contactRepo;
            _invitationRepo = invitationRepo;
        }

        public ICollection<ProjectMember> GetAlls()
        {
            return _contactRepo.Table.ToList();
        }

        public ICollection<ProjectMember> GetMembersInProject(long projId)
        {
            return _contactRepo.Table.Where(c => c.ProjectId == projId).ToList();
        }

        public ICollection<ProjectMember> GetProjectsByUser(long userId)
        {
            return _contactRepo.Table.Where(c => c.UserId == userId).ToList();
        }
        public ICollection<ProjectMember> GetProjects()
        {
            return _contactRepo.Table.ToList();
        }
        public ProjectMember GetMember(long projId, long userId)
        {
            return _contactRepo.Table.FirstOrDefault(c => c.ProjectId == projId && c.UserId == userId);
        }

        public ProjectMember GetMemberByContactId(long projId, int contactId)
        {
            return _contactRepo.Table.FirstOrDefault(c => c.ProjectId == projId && c.ContactId == contactId);
        }

        public void AddMember(long projId, long userId, int contactId, bool isCreator = false, bool display = true)
        {
            _contactRepo.Insert(new ProjectMember { ProjectId = projId, UserId = userId, ContactId = contactId, IsDirector = isCreator,Display=display });
        }

        public void RemoveMember(long projId, long userId)
        {
            var member = GetMember(projId, userId);
            if (member != null)
            {
                _contactRepo.Delete(member);
            }
        }

        public void RemoveMemberByContactId(long projId, int contactId)
        {
            var member = GetMemberByContactId(projId, contactId);
            if (member != null)
            {
                _contactRepo.Delete(member);
            }
        }

        public ICollection<ProjectInvitation> GetInvitationsByInviter(long inviterId)
        {
            return _invitationRepo.Table.Where(c => c.InviterId == inviterId).ToList();
        }

        public ICollection<ProjectInvitation> GetInvitationsForProject(long projId)
        {
            return _invitationRepo.Table.Where(c => c.ProjectId == projId&&!c.Accepted).ToList();
        }


        public void AddInvitationByInviteeId(long projectId, long inviterId, long inviteeId, string invitationMessage, long inviteePartId = 0)
        {
            var invitation = GetInvitation(projectId, inviterId, inviteeId, inviteePartId);
            if (invitation != null) return;
            invitation = new ProjectInvitation
            {
                ProjectId = projectId,
                InviterId = inviterId,
                InviteeId = inviteeId,
                InviteePartId = inviteePartId,
                InvitationMessage = invitationMessage
            };
            _invitationRepo.Insert(invitation);
        }

        public void AddInvitationByEmail(long projectId, long inviterId, string inviteeEmail, string invitationMessage, long inviteePartId = 0, int bidProjId = 0)
        {
            var invitation = GetInvitation(projectId, inviterId, inviteeEmail, inviteePartId);
            if (invitation != null)
            {
                //invitation.ProjectId = projectId;
                //invitation.InviterId = inviterId;
                //invitation.InviteeEmail = inviteeEmail;
                //invitation.InviteePartId = inviteePartId;
                invitation.InviteeId = 0;
                invitation.InvitationMessage = invitationMessage;
                invitation.Accepted = false;
                invitation.InviteeConfirmMessage = null;
                invitation.BidProjectId = bidProjId;
                _invitationRepo.Update(invitation);
            }
            else
            {
                invitation = new ProjectInvitation
                {
                    ProjectId = projectId,
                    InviterId = inviterId,
                    InviteeEmail = inviteeEmail,
                    InviteePartId = inviteePartId,
                    InvitationMessage = invitationMessage,
                    BidProjectId = bidProjId
                };
                _invitationRepo.Insert(invitation);
            }
        }

        public ProjectInvitation GetInvitation(long projectId, long inviterId, long inviteeId, long inviteePartId = 0)
        {
            return
                _invitationRepo.Table.FirstOrDefault(
                    c =>
                        c.ProjectId == projectId && c.InviterId == inviterId && c.InviteeId == inviteeId &&
                        c.InviteePartId == inviteePartId);
        }

        public ProjectInvitation GetInvitation(long projectId, long inviterId, string inviteeEmail, long inviteePartId = 0)
        {
            return
                _invitationRepo.Table.FirstOrDefault(
                    c =>
                        c.ProjectId == projectId && c.InviterId == inviterId && c.InviteePartId == inviteePartId &&
                        c.InviteeEmail == inviteeEmail);
        }

        public void ConfirmInvitationByEmail(long projectId, long inviterId, string inviteeEmail, string confirmMessage, long inviteePartId = 0)
        {
            var invitation = GetInvitation(projectId, inviterId, inviteeEmail, inviteePartId);
            if (invitation != null)
            {
                invitation.InviteeConfirmMessage = confirmMessage;
                _invitationRepo.Update(invitation);
            }
        }

        public void ConfirmInvitationByInviteeId(long projectId, long inviterId, long inviteeId, string confirmMessage, long inviteePartId = 0)
        {
            var invitation = GetInvitation(projectId, inviterId, inviteeId, inviteePartId);
            if (invitation != null)
            {
                invitation.InviteeConfirmMessage = confirmMessage;
                _invitationRepo.Update(invitation);
            }
        }

        public ICollection<ProjectInvitation> GetInvitationsByInviter(long projectId, long inviterId)
        {
            return
                _invitationRepo.Table.Where(
                    c => c.ProjectId == projectId && c.InviterId == inviterId).ToList();
        }

        public ICollection<ProjectInvitation> GetInvitations(long projectId, long inviteeId)
        {
            return
                _invitationRepo.Table.Where(
                    c => c.ProjectId == projectId&& c.InviteeId == inviteeId).ToList();
        }

        public ICollection<ProjectInvitation> GetInvitations(long projectId, long inviterId, long inviteeId)
        {
            return
                _invitationRepo.Table.Where(
                    c => c.ProjectId == projectId && c.InviterId == inviterId && c.InviteeId == inviteeId).ToList();
        }

        public ICollection<ProjectInvitation> GetInvitations(long projectId, long inviterId, string inviteeEmail)
        {
            return
                _invitationRepo.Table.Where(
                    c => c.ProjectId == projectId && c.InviterId == inviterId
                        && !String.IsNullOrEmpty(c.InviteeEmail) && c.InviteeEmail.ToUpper() == inviteeEmail.ToUpper()).ToList();
        }


        public void UpdateInvitation(ProjectInvitation invitation)
        {
            if (invitation == null) throw new ArgumentNullException("invitation");
            _invitationRepo.Update(invitation);
        }
    }
}
