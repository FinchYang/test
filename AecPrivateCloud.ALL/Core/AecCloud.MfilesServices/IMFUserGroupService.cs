using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public interface IMfUserGroupService
    {
        void AddUserGroupToVault(MFilesVault vault, MFilesUserGroup userGroup);

        MFilesUserGroup GetUserGroupByName(MFilesVault vault, string groupName);

        ICollection<MFilesUserGroup> GetUserGroupsContainsString(MFilesVault vault, string groupStr);

        MFilesUserGroup GetUserGroupById(MFilesVault vault, int groupId);

        ICollection<MFilesUserGroup> GetUserGroups(MFilesVault vault, bool includeInternalGroup=false);

        void RemoveUserGroupById(MFilesVault vault, int groupId);
        void AddUserToGroup(Vault vault, int userId, int groupId);
         int GetUserId(Vault vault, string userName);
        void AddUserToGroup(MFilesVault vault, string userName, params int[] groupIds);

        void AddUserToGroup(MFilesVault vault, string userName, string groupName);

        void RemoveUserFromGroup(MFilesVault vault, string userName, params int[] groupIds);

        void RemoveUserFromGroup(MFilesVault vault, string userName, params string[] groupNames);

        void RenameGroupName(MFilesVault vault, int groupId, string newName);

        ICollection<MFilesUserGroup> GetGroupsByUser(MFilesVault vault, string userName);

        ICollection<string> GetUsersInGroup(MFilesVault vault, int groupId);

        ICollection<string> GetUsersInGroup(MFilesVault vault, string groupName);

        ICollection<string> GetUsersInGroupByAlias(MFilesVault vault, string groupAlias);

    }
}
