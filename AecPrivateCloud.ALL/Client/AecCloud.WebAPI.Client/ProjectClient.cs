using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AecCloud.WebAPI.Models;

namespace AecCloud.WebAPI.Client
{
    public class ProjectClient
    {
        private readonly HttpClient _client;
        private readonly string _routePrefix = "api/Project";
        private readonly string _memberRoutePrefix = "api/ProjectMembers";

        internal ProjectClient(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> GetProjects(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Projects");
        }

        public Task<HttpResponseMessage> GetAreas(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Areas");
        }

        public Task<HttpResponseMessage> GetCompanies(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Companies");
        }

        public Task<HttpResponseMessage> GetLevels(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Levels");
        }

        public Task<HttpResponseMessage> GetProject(int projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/AllProjects/"+projId);
        }

        public Task<HttpResponseMessage> GetAllParties(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/AllParties");
        }

        public Task<HttpResponseMessage> GetParty(int partyId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/Parties/"+partyId);
        }

        public Task<HttpResponseMessage> GetPartyByName(string partyName, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_routePrefix + "/GetPartyByName/?name=" + partyName);
        }

        public Task<HttpResponseMessage> GetProjectTemplates(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/ProjectTemplates", null);
        }
        /// <summary>
        /// 仅创建项目对象，不创建文档库
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> CreateProjObj(ProjectCreateModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/CreateProjObj", model);
        }
        public Task<HttpResponseMessage> CreateProject(ProjectCreateModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/Create", model);
        }
        /// <summary>
        /// 根据项目创建文档库
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> CreateVault(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/CreateVault?projId="+projId, null);
        }

        public Task<HttpResponseMessage> UpdateProject(ProjectEditModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/Update", model);
        }

        public Task<HttpResponseMessage> TransferProject(ProjectTransferModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_routePrefix + "/Transfer", model);
        }
        /// <summary>
        /// 立项
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> ProposalProject(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/Proposal/" + projId, null);
        }
        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> StartProject(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/Start/" + projId, null);
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PauseProject(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/Pause/" + projId, null);
        }
        /// <summary>
        /// 终止或结束
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> EndProject(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsync(_routePrefix + "/End/" + projId, null);
        }

        /// <summary>
        /// 获取联系人，获取所有项目相关的成员联系人
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> GetContacts(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/Contacts");
        }
        /// <summary>
        /// 获取相关项目成员
        /// </summary>
        /// <param name="projId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> GetProjectMembers(long projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/InProject/" + projId);
        }

        public Task<HttpResponseMessage> SendInvitationEmail(SendEmailMessage model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/SendInvitationEmail", model);
        }
        /// <summary>
        /// 按照Email进行邀请
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> InviteMemberByEmail(ProjectInvitationEmailModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/InviteMemberByEmail", model);
        }
        /// <summary>
        /// 使用UserId进行邀请
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> InviteMemberByUserId(ProjectInvitationUserModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/InviteMemberByUserId", model);
        }
        /// <summary>
        /// 被邀请人接受邀请，若此邀请通过Email字段邀请，则请提供Email
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> InviteeConfirmInvitation(ProjectInvitationConfirmEmailModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/InviteeConfirmInvitation", model);
        }
        /// <summary>
        /// 邀请者确认被邀请者加入项目
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> AcceptInvitationConfirm(ProjectInvitationConfirmAcceptModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/AcceptInvitationConfirm", model);
        }
        ///// <summary>
        ///// 添加项目成员
        ///// </summary>
        ///// <param name="projId"></param>
        ///// <param name="userId"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public Task<HttpResponseMessage> AddProjectMember(int projId, int userId, TokenModel token)
        //{
        //    var model = new ProjectMemberModel { ProjectId = projId, UserId = userId };
        //    TokenClient.RefreshToken(_client, token);
        //    return _client.PostAsJsonAsync(_memberRoutePrefix + "/AddMember/", model);
        //}

        public Task<HttpResponseMessage> RemoveProjectMember(ProjectMemberModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/RemoveMember/", model);
        }

        public Task<HttpResponseMessage> GetInvitationsByInviter(TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/Invitations/");
        }

        public Task<HttpResponseMessage> UserGroups(int projId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/UserGroups/" + projId);
        }

        public Task<HttpResponseMessage> GetUserGroupsByParty(int projId, int partyId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/UserGroupsFromParty?projId=" + projId + "&partyId=" + partyId);
        }

        public Task<HttpResponseMessage> GroupInProject(int projId, int groupId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/GroupInProject/" + projId + "/" + groupId);
        }

        public Task<HttpResponseMessage> AddUserGroup(ProjectCreateUserGroupModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/AddUserGroup/", model);
        }

        public Task<HttpResponseMessage> RemoveUserGroup(ProjectRemoveUserGroupModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/RemoveUserGroup/", model);
        }

        public Task<HttpResponseMessage> GetUsernamesInGroup(int projId, int groupId, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.GetAsync(_memberRoutePrefix + "/GetUsernamesInGroup/" + projId + "/" + groupId);
        }

        public Task<HttpResponseMessage> RemoveUserFromParty(ProjectRemoveUserFromPartyModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/RemoveUserFromParty/", model);
        }

        public Task<HttpResponseMessage> RemoveUsersFromGroup(ProjectRemoveUserFromGroupModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/RemoveUsersFromGroup/", model);
        }

        public Task<HttpResponseMessage> AddUserToGroup(ProjectAddUserToGroupModel model, TokenModel token)
        {
            TokenClient.RefreshToken(_client, token);
            return _client.PostAsJsonAsync(_memberRoutePrefix + "/AddUserToGroup/", model);
        }
    }
}
