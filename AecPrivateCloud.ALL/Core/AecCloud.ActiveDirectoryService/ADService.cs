using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using AecCloud.BaseCore;
using AecCloud.Core;
using AecCloud.Core.Domain;

namespace AecCloud.ActiveDirectoryService
{
    /// <summary>
    /// 
    /// </summary>
    public class ADService : IADService
    {

        private static string GetOU(ActiveDirectoryGroup group)
        {
            return group.OrganizationUnits;
        }
        /// <summary>
        /// 在AD域中新建用户
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="ad"></param>
        /// <param name="group">用户隶属于的组格式：OU=Web组,OU=开发部</param>
        public void CreateADUser(User user, ActiveDirectory ad, ActiveDirectoryGroup group)
        {
            throw new NotImplementedException();
            //var ou = GetOU(group);
            ////var hasUser = HasUser(user,ou);
            ////if(hasUser) throw new Exception("域中已存在该用户"+user.UserName);
            ////user.ActiveDirectoryId = ad.Id;
            //user.Domain = ad.Domain;
            //var adInfo = ad; //user.ActiveDirectory;
            //var domainPath = GetDomainPath(adInfo.LDAPRoot, adInfo.DCInfo, ou, "");
            //using (
            //    var de = new DirectoryEntry(domainPath, adInfo.AdminName, adInfo.AdminPwd, AuthenticationTypes.Secure))
            //{
            //    using (var us = de.Children.Add("CN=" + user.UserName, "user"))
            //    {
            //        var name = user.UserName;
            //        if (user.Profile != null)
            //        {
            //            name = user.Profile.FullName ?? user.UserName;
            //        }
            //        us.Properties["sAMAccountName"].Add(user.UserName); //account
            //        us.Properties["userPrincipalName"].Value = user.UserName; //user logon name,xxx@bdxy.com
            //        us.Properties["givenName"].Value = name; //名
            //        //us.Properties["sn"].Value = "张";
            //        us.Properties["name"].Value = name; //full name

            //        us.Properties["displayName"].Value = name;

            //        us.Properties["mail"].Value = user.Email;

            //        us.CommitChanges();
            //        //反射调用设置密码的方法（注意端口号的问题  端口号会引起方法调用异常）
            //        var pass = Utility.DecryptText(user.Password, user.PrivateKey);
            //        us.Invoke("SetPassword", new object[] {pass});
            //        //默认设置新增账户启用
            //        us.Properties["userAccountControl"].Value = 0x10200;
            //        us.CommitChanges();
            //    }

            //    var groupDe = GetADGroup(de, ad, group.Name);
            //    if (groupDe == null)
            //    {
            //        groupDe = CreateADGroup(de, group.Name);
            //    }
            //    if (groupDe != null)
            //    {
            //        using (groupDe)
            //        {
            //            var userDN = "CN=" + user.UserName + "," + group.OrganizationUnits + "," + adInfo.DCInfo;
            //            groupDe.Properties["member"].Add(userDN);
            //            groupDe.CommitChanges();
            //        }
            //    }

            //}
        }

        private static DirectoryEntry CreateADGroup(DirectoryEntry root, string groupName)
        {
            try
            {
                DirectoryEntry group = root.Children.Add("CN=" + groupName, "group");
                group.Properties["sAmAccountName"].Value = groupName;
                group.CommitChanges();
                return group;
            }
            catch
            {
                return null;
            }
        }

        private static DirectoryEntry GetADGroup(DirectoryEntry root, ActiveDirectory ad, string groupName)
        {
            var deSearch = new DirectorySearcher(root);
            deSearch.Filter = "(&(objectClass=group)(cn=" + groupName.Replace("\\", "") + "))";
            deSearch.SearchScope = SearchScope.Subtree;
            try
            {
                SearchResult result = deSearch.FindOne();
                if (result != null)
                {
                    var de1 = new DirectoryEntry(result.Path, ad.AdminName, ad.AdminPwd);
                    return de1;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        ///// <summary>
        ///// 获取AD组
        ///// </summary>
        ///// <param name="ad"></param>
        ///// <param name="group"></param>
        ///// <returns></returns>
        //internal DirectoryEntry GetADGroup(ActiveDirectory ad, ActiveDirectoryGroup group)
        //{
        //    var groupName = group.Name;
        //    try
        //    {
        //        var domainPath = GetDomainPath(ad.LDAPRoot, ad.DCInfo, group.OrganizationUnits, "");
        //        var de = new DirectoryEntry(domainPath, ad.AdminName, ad.AdminPwd, AuthenticationTypes.Secure);
        //        return GetADGroup(de, ad, group.Name);
        //    }
        //    catch
        //    {
        //    }
        //    return null;
        //}
        /// <summary>
        /// 修改用户全名、邮件地址
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="ad"></param>
        /// <param name="group">用户隶属于组</param>
        public void ChangeUserInfo(User user, ActiveDirectory ad, ActiveDirectoryGroup group)
        {
            var ou = GetOU(group);
            //throw new NotImplementedException();
            var adInfo = ad; //user.ActiveDirectory;
            var domainPath = GetDomainPath(adInfo.LDAPRoot, adInfo.DCInfo, ou, "CN=" + user.UserName);
            using (var us = new DirectoryEntry(domainPath, adInfo.AdminName, adInfo.AdminPwd, AuthenticationTypes.Secure))
            {
                if (user.Profile != null)
                {
                    var comparer = StringComparer.OrdinalIgnoreCase;
                    var oldEmail = us.Properties["mail"].Value == null ? null : us.Properties["mail"].Value.ToString();
                    var oldFullname = us.Properties["displayName"].Value == null ? null : us.Properties["displayName"].Value.ToString();
                    //us.Properties["name"].Value = user.Profile.FullName ?? user.UserName;
                    if (!comparer.Equals(oldEmail, user.Profile.Email))
                    {
                        us.Properties["mail"].Value = user.Profile.Email ?? user.Email;
                    }
                    if (!comparer.Equals(oldFullname, user.Profile.FullName))
                    {
                        us.Properties["displayName"].Value = user.Profile.FullName ?? user.UserName;
                    }
                    us.CommitChanges();
                }
            }
        }

        /// <summary>
        /// 变更用户密码
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="ad"></param>
        /// <param name="ou">用户隶属于组</param>
        public void ChangeUserPassword(User user, ActiveDirectory ad, ActiveDirectoryGroup group)
        {
            var ou = GetOU(group);
            var adInfo = ad; //user.ActiveDirectory;
            var domainPath = GetDomainPath(adInfo.LDAPRoot, adInfo.DCInfo, ou, "CN=" + user.UserName);
            using (var de = new DirectoryEntry(domainPath, adInfo.AdminName, adInfo.AdminPwd, AuthenticationTypes.Secure))
            {
                var pass = Utility.DecryptText(user.Password, user.PrivateKey);
                de.Invoke("SetPassword", new object[] { pass });
                de.CommitChanges();
                de.RefreshCache();
            }
        }

        /// <summary>
        /// 判断域中是否存在指定的用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ad"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool HasUser(User user, ActiveDirectory ad, ActiveDirectoryGroup group)
        {
            var userDir = GetUser(user, ad, group);
            return userDir != null;
        }

        internal DirectoryEntry GetUser(User user, ActiveDirectory ad, ActiveDirectoryGroup group)
        {
            var ou = GetOU(group);
            var adInfo = ad; //user.ActiveDirectory;
            var domainPath = GetDomainPath(adInfo.LDAPRoot, adInfo.DCInfo, ou, "");
            using (
                var de = new DirectoryEntry(domainPath, adInfo.AdminName, adInfo.AdminPwd, AuthenticationTypes.Secure))
            {
                var deSearch = new DirectorySearcher(de);
                deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + user.UserName + "))";
                deSearch.SearchScope = SearchScope.Subtree;

                try
                {
                    SearchResult result = deSearch.FindOne();
                    var sde = new DirectoryEntry(result.Path);

                    return sde;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取完整的LDAP路径
        /// </summary>
        /// <param name="ldapRoot"></param>
        /// <param name="dc"></param>
        /// <param name="ou"></param>
        /// <param name="cn"></param>
        /// <returns></returns>
        private string GetDomainPath(string ldapRoot ,string dc,string ou,string cn)
        {
            ldapRoot = ldapRoot.ToUpper();
            if (ldapRoot[ldapRoot.Length - 1] != '/') ldapRoot = ldapRoot + "/";
            if (ou != string.Empty && ou[ou.Length - 1] != ',') ou = ou + ",";
            if (cn != string.Empty && cn[cn.Length - 1] != ',') cn = cn + ",";
            return ldapRoot + cn + ou + dc;
        }

        /// <summary>
        /// 获取当前AD域
        /// </summary>
        /// <returns></returns>
        public ActiveDirectory GetCurrentAD(IEnumerable<ActiveDirectory> ads)
        {
            return ads.FirstOrDefault(c => c.Id != 0);
        }
    }
}
