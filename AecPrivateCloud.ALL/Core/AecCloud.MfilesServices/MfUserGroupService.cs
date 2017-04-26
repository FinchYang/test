using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MfUserGroupService : IMfUserGroupService
    {
        public void AddUserGroupToVault(MFilesVault vault, MFilesUserGroup userGroup)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var newuga = new UserGroupAdmin
            {
                UserGroup = new UserGroup { Name = userGroup.Name }
            };
            if (!String.IsNullOrEmpty(userGroup.Alias))
            {
                newuga.SemanticAliases = new SemanticAliases {Value = userGroup.Alias};
            }
            var ug = ovault.UserGroupOperations.AddUserGroupAdmin(newuga);
            userGroup.Id = ug.UserGroup.ID;
        }

        public MFilesUserGroup GetUserGroupByName(MFilesVault vault, string groupName)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return GetGroupByName(mfVault, groupName, vault.Id);
        }

        public ICollection<MFilesUserGroup> GetUserGroupsContainsString(MFilesVault vault, string groupStr)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var groups = mfVault.UserGroupOperations.GetUserGroupsAdmin();
            var list = (from UserGroupAdmin uga in groups
                    where !uga.UserGroup.Predefined && uga.UserGroup.Name.Contains(groupStr) && uga.UserGroup.Name != groupStr
                    select ConvertTo(uga, vault.Id)).ToList();
            list.Sort();
            return list;
        }

        public MFilesUserGroup GetUserGroupById(MFilesVault vault, int groupId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return GetGroupById(mfVault, groupId, vault.Id);
        }

        public ICollection<MFilesUserGroup> GetUserGroups(MFilesVault vault, bool includeInternalGroup = false)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var groups = mfVault.UserGroupOperations.GetUserGroupsAdmin();
            if (includeInternalGroup)
            {
                return (from UserGroupAdmin uga in groups select ConvertTo(uga, vault.Id)).ToList();
            }
            return (from UserGroupAdmin uga in groups where !uga.UserGroup.Predefined select ConvertTo(uga, vault.Id)).ToList();
        }

        public void RemoveUserGroupById(MFilesVault vault, int groupId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            mfVault.UserGroupOperations.RemoveUserGroupAdmin(groupId);
        }

        public void AddUserToGroup(MFilesVault vault, string userName, string groupName)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var groupId = GetGroupId(ovault, groupName);
            var userId = GetUserId(ovault, userName);
            AddUserToGroup(ovault, userId, groupId);
        }

        public void AddUserToGroup(Vault vault, int userId, int groupId)
        {
            var uga = vault.UserGroupOperations.GetUserGroupAdmin(groupId);
            //若已存在，则不添加
            if (uga.UserGroup.Members.Cast<int>().Any(c => c == userId)) return;
            uga.UserGroup.Members.Add(-1, userId);
            vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
        }

        public void AddUserToGroup(MFilesVault vault, string userName, params int[] groupIds)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var userId = GetUserId(ovault, userName);
            foreach (var groupId in groupIds)
            {
                AddUserToGroup(ovault, userId, groupId);
            }
        }



        public void RemoveUserFromGroup(MFilesVault vault, string userName, params int[] groupIds)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var userid = -1;
            var users = ovault.UserOperations.GetUserAccounts();
            var un = MfUserUtils.GetUserNameWithoutDomain(userName).ToUpper();
            foreach (UserAccount ua in users)
            {
                var un0 = MfUserUtils.GetUserNameWithoutDomain(ua.LoginName);
                if (un0.ToUpper() == un)
                {
                    userid = ua.ID;
                    break;
                }
            }
            foreach (var groupId in groupIds)
            {
                RemoveUserFromGroupById(ovault, userid, groupId);
            }
        }

        private void RemoveUserFromGroupById(Vault vault, int userId, int groupId)
        {
            var uga = vault.UserGroupOperations.GetUserGroupAdmin(groupId);

            var index = uga.UserGroup.Members.IndexOf(userId);
            if (index != -1)
            {
                uga.UserGroup.Members.Remove(index);
                vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
            }
        }

        public void RemoveUserFromGroup(MFilesVault vault, string userName, params string[] groupNames)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var userid = -1;
            var users = ovault.UserOperations.GetUserAccounts();
            var un = MfUserUtils.GetUserNameWithoutDomain(userName).ToUpper();
            foreach (UserAccount ua in users)
            {
                var un0 = MfUserUtils.GetUserNameWithoutDomain(ua.LoginName);
                if (un0.ToUpper() == un)
                {
                    userid = ua.ID;
                    break;
                }
            }
            foreach (var g in groupNames)
            {
                var groupId = GetGroupId(ovault, g);
                RemoveUserFromGroupById(ovault, userid, groupId);
            }
        }

        public void RenameGroupName(MFilesVault vault, int groupId, string newName)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var uga = ovault.UserGroupOperations.GetUserGroupAdmin(groupId);
            uga.UserGroup.Name = newName;
            ovault.UserGroupOperations.UpdateUserGroupAdmin(uga);
        }

        public ICollection<MFilesUserGroup> GetGroupsByUser(MFilesVault vault, string userName)
        {
            var ret = new List<MFilesUserGroup>();
            var ovault = MFServerUtility.GetVault(vault);
            var userid = GetUserId(ovault, userName);
            var usergroups = ovault.UserGroupOperations.GetUserGroupsAdmin();
            foreach (UserGroupAdmin uga in usergroups)
            {
                foreach (int id in uga.UserGroup.Members)
                {
                    if (id < 0)
                    {
                        ret.AddRange(GetUserGroupsOfUserEx(userid, -id, uga, ovault));
                    }
                    else
                    {
                        if (id == userid && !uga.UserGroup.Predefined)
                        {
                            ret.Add(ConvertTo(uga,vault.Id));
                        }
                    }
                }
            }
            var newret = new List<MFilesUserGroup>();
            foreach (var a in ret)
            {
                var found = false;
                foreach (var b in newret)
                {
                    if (b.Id == a.Id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) newret.Add(a);
            }
            return newret;
        }

        private string GetUsername(Vault vault, int mfUserId)
        {
            try
            {
                var user = vault.UserOperations.GetUserAccount(mfUserId);
                return MfUserUtils.GetUserNameWithoutDomain(user.LoginName);
            }
            catch
            {
                return String.Empty;
            }
        }

        public ICollection<string> GetUsersInGroup(MFilesVault vault, int groupId)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var users = GetUsersFromGroup(ovault, groupId);
            return users.Select(uid => ovault.UserOperations.GetUserAccount(uid)).Select(la => MfUserUtils.GetUserNameWithoutDomain(la.LoginName)).ToList();
        }

        public ICollection<string> GetUsersInGroup(MFilesVault vault, string groupName)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var group = GetGroupByName(ovault, groupName, vault.Id);
            return GetUsersInGroup(ovault, group.GroupId);
        }

        internal ICollection<string> GetUsersInGroup(Vault ovault, int groupId)
        {
            var users = GetUsersFromGroup(ovault, groupId);
            return users.Select(uid => ovault.UserOperations.GetUserAccount(uid)).Select(la => MfUserUtils.GetUserNameWithoutDomain(la.LoginName)).ToList();
        }

        private int GetGroupId(Vault vault, string groupName)
        {
            var groups = vault.UserGroupOperations.GetUserGroupsAdmin();
            var ugroup = (from UserGroupAdmin ug in groups where ug.UserGroup.Name == groupName select ug.UserGroup).FirstOrDefault();
            if (ugroup == null) throw new AecCloud.Core.AecException("不存在此用户组：" + groupName);
            return ugroup.ID;
        }


        private MFilesUserGroup GetGroupByName(Vault vault, string groupName, long vaultId)
        {
            var groups = vault.UserGroupOperations.GetUserGroupsAdmin();
            return (from UserGroupAdmin ug in groups where ug.UserGroup.Name == groupName select ConvertTo(ug, vaultId)).FirstOrDefault();
        }

        private MFilesUserGroup GetGroupById(Vault vault, int groupId, long vaultId)
        {
            var group = vault.UserGroupOperations.GetUserGroupAdmin(groupId);
            return ConvertTo(group, vaultId);
        }

        private MFilesUserGroup ConvertTo(UserGroup group, long vaultId)
        {
            return new MFilesUserGroup
            {
                Id = group.ID,
                Name = group.Name,
                VaultId = vaultId,
                GroupId = group.ID
            };
            
        }

        private MFilesUserGroup ConvertTo(UserGroupAdmin groupAdmin, long vaultId)
        {
            var group = ConvertTo(groupAdmin.UserGroup, vaultId);
            group.Alias = groupAdmin.SemanticAliases.Value;
            return group;
        }

        public int GetUserId(Vault vault, string userName)
        {
            var userid =-1;
            var users = vault.UserOperations.GetUserAccounts();
            var un = MfUserUtils.GetUserNameWithoutDomain(userName).ToUpper();
            foreach (UserAccount ua in users)
            {
                var un0 = MfUserUtils.GetUserNameWithoutDomain(ua.LoginName).ToUpper();
                if (un0 == un)
                {
                    userid = ua.ID;
                    break;
                }
            }
            return userid;
        }

        private IEnumerable<MFilesUserGroup> GetUserGroupsOfUserEx(long userid, int ugid, UserGroupAdmin fatherug, Vault ovault)
        {
            var uga = ovault.UserGroupOperations.GetUserGroupAdmin(ugid);
            var ret = new List<MFilesUserGroup>();
            var fatherhave = false ;
            foreach (int id in uga.UserGroup.Members)
            {
                if (id < 0)
                {
                    var ugs = GetUserGroupsOfUserEx(userid, -id, uga, ovault).ToArray();
                    if (ugs.Any())
                    {
                        ret.AddRange(ugs);
                        fatherhave = true;
                    }
                   
                }
                else
                {
                    if (id == userid && !uga.UserGroup.Predefined)
                    {
                        ret.Add(ConvertTo(uga, 0));
                        fatherhave = true;
                       
                    }
                }
            }
            if (fatherhave)
            {
                ret.Add(ConvertTo(fatherug, 0));
            }
            return ret;
        }
        //returns UserIds
        private IEnumerable<int> GetUsersFromGroup(Vault ovault, int ugid)
        {
            var ret = new List<int>();
            var uga = ovault.UserGroupOperations.GetUserGroupAdmin(ugid);
            foreach (int id in uga.UserGroup.Members)
            {
                if (id < 0)
                {
                    ret.AddRange(GetUsersFromGroup(ovault, -id));
                }
                else
                {
                    ret.Add(id);
                }
            }
            return ret;
        }


        public ICollection<string> GetUsersInGroupByAlias(MFilesVault vault, string groupAlias)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var groupId = MfAlias.GetUsergroup(ovault, groupAlias);
            return GetUsersInGroup(ovault, groupId);
        }
    }
}
