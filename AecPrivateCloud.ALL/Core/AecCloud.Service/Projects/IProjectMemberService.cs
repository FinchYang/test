using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Projects;

namespace AecCloud.Service.Projects
{
    public interface IProjectMemberService
    {
        ICollection<ProjectMember> GetAlls();

        ICollection<ProjectMember> GetMembersInProject(long projId);

        ProjectMember GetMember(long projId, long userId);

        ProjectMember GetMemberByContactId(long projId, int contactId);

        ICollection<ProjectMember> GetProjectsByUser(long userId);
        ICollection<ProjectMember> GetProjects();

        void AddMember(long projId, long userId, int contactId, bool isDirector = false,bool display = true);

        void RemoveMember(long projId, long userId);

        void RemoveMemberByContactId(long projId, int contactId);

        ICollection<ProjectInvitation> GetInvitationsByInviter(long inviterId);

        ICollection<ProjectInvitation> GetInvitationsByInviter(long projectId, long inviterId);

        ICollection<ProjectInvitation> GetInvitationsForProject(long projId);

        ICollection<ProjectInvitation> GetInvitations(long projectId, long inviterId, long inviteeId);

        ICollection<ProjectInvitation> GetInvitations(long projectId, long inviterId, string inviteeEmail);

        ICollection<ProjectInvitation> GetInvitations(long projectId, long inviteeId);

        ProjectInvitation GetInvitation(long projectId, long inviterId, long inviteeId, long inviteePartId = 0);

        ProjectInvitation GetInvitation(long projectId, long inviterId, string inviteeEmail, long inviteePartId = 0);

        void AddInvitationByInviteeId(long projectId, long inviterId, long inviteeId, string invitationMessage, long inviteePartId = 0);

        void AddInvitationByEmail(long projectId, long inviterId, string inviteeEmail, string invitationMessage, long inviteePartId = 0, int bidProjId=0);

        void ConfirmInvitationByEmail(long projectId, long inviterId, string inviteeEmail, string confirmMessage, long inviteePartId = 0);

        void ConfirmInvitationByInviteeId(long projectId, long inviterId, long inviteeId, string confirmMessage, long inviteePartId = 0);

        //void AcceptInvitationConfirm(int projectId, int inviterId, int inviteeId, int inviteePartId);

        void UpdateInvitation(ProjectInvitation invitation);
    }
}
