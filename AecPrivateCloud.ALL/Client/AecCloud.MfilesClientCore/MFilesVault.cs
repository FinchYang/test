using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MFilesAPI;
using AecCloud.WebAPI.Models;

namespace AecCloud.MfilesClientCore
{
    /// <summary>
    /// MFiles 库相关
    /// </summary>
    public static class MFilesVault
    {
        private static readonly List<string> VaultGuids = new List<string>();
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static VaultConnection InitialVaultConnection(UserDto user, VaultDto vault)
        {
            var app = new MFilesClientApplication();
            var conns = app.GetVaultConnectionsWithGUID(vault.Guid);
            //var count = conns.Count;
            var removeConns = new List<VaultConnection>();
            VaultConnection connection = null;
            foreach (VaultConnection vc in conns)
            {
                if (vc.NetworkAddress != vault.Server.Ip
                    || vc.Name != vault.Name
                    || vc.Endpoint != vault.Server.Port)
                {
                    removeConns.Add(vc);
                }
                else
                {
                    connection = vc;
                }
            }
            if (removeConns.Count > 0)
            {
                foreach (var vc in removeConns)
                {
                    app.RemoveVaultConnection(vc.Name, vc.UserSpecific);
                }
            }
            if (connection == null)
            {
                connection = new VaultConnection
                {
                    AuthType = MFAuthType.MFAuthTypeSpecificWindowsUser,
                    AutoLogin = false,
                    NetworkAddress = vault.Server.Ip,
                    Endpoint = vault.Server.Port,
                    Name = vault.Name,
                    ServerVaultName = vault.Name,
                    ServerVaultGUID = vault.Guid,
                    UserName = user.UserName,
                    Password = user.Password,
                    Domain = user.Domain,
                    UserSpecific = true,
                    ProtocolSequence = "ncacn_ip_tcp"
                };
                app.AddVaultConnection(connection);
            }
            //var now = DateTime.Now;
            Vault mfVault = null;
            if (connection.IsLoggedIn())
            {
                var v = connection.BindToVault(IntPtr.Zero, true, true);
                if (v != null)
                {
                    var accountName = v.SessionInfo.AccountName;
                    var index = accountName.IndexOf('\\');
                    var userName = accountName.Substring(index + 1);
                    if (StringComparer.OrdinalIgnoreCase.Equals(userName, user.UserName))
                    {
                        mfVault = v;
                    }
                    else
                    {
                        v.LogOutWithDialogs(IntPtr.Zero);
                    }
                }
            }
            return connection;
        }


        private static bool Need2Remove(VaultDto vault, VaultConnection vc, string vaultName)
        {
            vaultName = vaultName ?? vault.Name;
            if (vc.Name == vaultName)
            {
                if (vc.NetworkAddress != vault.Server.Ip) return true;
                if (vc.Endpoint != vault.Server.Port) return true;
                if (vc.ServerVaultGUID != vault.Guid) return true;
            }
            if (vc.ServerVaultGUID != vault.Guid) return false;
            if (vc.Name != vaultName) return true;
            if (vc.NetworkAddress != vault.Server.Ip) return true;
            if (vc.Endpoint != vault.Server.Port) return true;
            return false;
        }

        public static Vault GetUserVault(UserDto user, VaultDto vault, bool forceLogout)
        {
          //  (string.Format(" in GetUserVault(),userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, user.Domain));
            return GetUserVault(user, vault, forceLogout, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vault"></param>
        /// <param name="forceLogout">是否强制退出</param>
        /// <param name="vaultName">库名称</param>
        /// <returns></returns>
        public static Vault GetUserVault(UserDto user, VaultDto vault, bool forceLogout, string vaultName)
        {
            log.Info(" GetUserVault username="+user.FullName+",vault="+vault.Name);
            if (VaultGuids.Contains(vault.Guid))
            {
                return GetVault(user, vault.Guid);
            }
            vaultName = vaultName ?? vault.Name;
            var app = new MFilesClientApplication();
            var removeConns = new List<VaultConnection>();
            try
            {
                var sameVC = app.GetVaultConnection(vault.Name);
                var needR = Need2Remove(vault, sameVC, vaultName);
                if (needR) removeConns.Add(sameVC);
            }
            catch (Exception ex)
            {
              log.Info(string.Format("GetUserVault error:{0},{1}",vaultName,ex.Message));
            }
            var conns = app.GetVaultConnectionsWithGUID(vault.Guid);
            
            VaultConnection connection = null;
            foreach (VaultConnection vc in conns)
            {
                var needR = Need2Remove(vault, vc, vaultName);
                if (needR) removeConns.Add(vc);
                else connection = vc;
            }
            if (removeConns.Count > 0)
            {
                foreach (var vc in removeConns)
                {
                    app.RemoveVaultConnection(vc.Name, vc.UserSpecific);
                }
            }
            if (connection == null)
            {
                connection = new VaultConnection
                {
                    AuthType = MFAuthType.MFAuthTypeSpecificWindowsUser,
                    AutoLogin = false,
                    NetworkAddress = vault.Server.Ip,
                    Endpoint = vault.Server.Port,
                    Name = vaultName,
                    ServerVaultName = vault.Name,
                    ServerVaultGUID = vault.Guid,
                    UserName = user.UserName,
                    Password = user.Password,
                    Domain = user.Domain,
                    UserSpecific = true,
                    ProtocolSequence = "ncacn_ip_tcp"
                };
                if (String.IsNullOrEmpty(user.Domain))
                {
                    connection.AuthType = MFAuthType.MFAuthTypeSpecificMFilesUser;
                }
                app.AddVaultConnection(connection);
            }
            //var now = DateTime.Now;
            Vault mfVault = null;
            if (connection.IsLoggedIn())
            {
                var v = connection.BindToVault(IntPtr.Zero, true, true);
                if (v != null)
                {
                    if (forceLogout)
                    {
                        try
                        {
                            v.LogOutWithDialogs(IntPtr.Zero);
                        }
                        catch
                        {
                            log.Info("Remote Loggin time11111: " + DateTime.Now);
                        }
                    }
                    else
                    {
                        var accountName = v.SessionInfo.AccountName;
                        var index = accountName.IndexOf('\\');
                        var userName = accountName.Substring(index + 1);
                        if (StringComparer.OrdinalIgnoreCase.Equals(userName, user.UserName))
                        {
                            mfVault = v;
                        }
                        else
                        {
                            try
                            {
                                v.LogOutWithDialogs(IntPtr.Zero);
                            }
                            catch
                            {
                                log.Info("Remote Loggin time 22222: " + DateTime.Now);
                            }
                        }
                    }
                }
            }
            log.Info("Remote Loggin time: " + DateTime.Now );
            
            try
            {
                //now = DateTime.Now;
                var has = false;
                log.Info(string.Format(" in getuservault,userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, user.Domain));
                if (forceLogout)
                {
                  
                    mfVault = LoginVault(connection, user.UserName, user.Password, user.Domain);
                    has = true;
                }
                if (mfVault == null || !has)
                {
                    mfVault = LoginVault(connection, user.UserName, user.Password, user.Domain);
                }
                log.Info("Loggin time: " + DateTime.Now );
            }
            catch
            {
                log.Info("Remote Loggin time: 33333" + DateTime.Now);
            }
            VaultGuids.Add(vault.Guid);
            return mfVault;
        }
        /// <summary>
        /// 设置客户端MFiles连接
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vault"></param>
        public static Vault GetUserVault1(UserDto user, VaultDto vault)
        {
            if (VaultGuids.Contains(vault.Guid))
            {
                return GetVault(user, vault.Guid);
            }
            var server = vault.Server;
            var conn = new VaultConnection();
            var domain = user.Domain;
            conn.AuthType = MFAuthType.MFAuthTypeSpecificWindowsUser;
            conn.AutoLogin = false;
            conn.NetworkAddress = server.Ip;// "192.168.2.129";
            conn.Endpoint = server.Port;// "2266";
            conn.Name = vault.Name;//"我的云盘";
            conn.ServerVaultName = vault.Name;// "示例库";
            conn.ServerVaultGUID = vault.Guid;// "{08ED46E7-C0FF-4D16-BA38-5043144CCD15}";
            conn.UserName = user.UserName;// "qiuge";
            conn.Password = user.Password;// "sd2350139";
            conn.Domain = domain;// "simuladesign";
            conn.UserSpecific = true;
            conn.ProtocolSequence = "ncacn_ip_tcp";
            var app = new MFilesClientApplication();
            var conns = app.GetVaultConnections();
            var connsSameName = new VaultConnections();
            foreach (VaultConnection co in conns)
            {
                var coGUID = co.ServerVaultGUID;
                if (coGUID == conn.ServerVaultGUID) // && co.Name == conn.Name
                {
                    connsSameName.Add(-1, co);
                }
               
            }
            if (connsSameName.Count > 0)
            {
                foreach (VaultConnection co in connsSameName)
                {
                    app.RemoveVaultConnection(co.Name, co.UserSpecific);
                }
            }
            app.AddVaultConnection(conn);
            Vault mfVault = null;
            try
            {
             //   Writelog(string.Format(" in getuservault1,userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, domain));
                mfVault = LoginVault(conn, user.UserName, user.Password, domain);
                
            }
            catch
            {
            }
            VaultGuids.Add(vault.Guid);
            return mfVault;
        }
      

        public static Vault LoginVault(VaultConnection vc, UserDto user)
        {
          //  Writelog(string.Format(" in loginvault,userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, user.Domain));
            return LoginVault(vc, user.UserName, user.Password, user.Domain);
        }
        private static Vault LoginVault(VaultConnection vc, string userName, string pwd, string domain)
        {
          //  Writelog(string.Format(" userName={0}, pwd={1}, domai={2}", userName, pwd, domain));
            
            if (!String.IsNullOrEmpty(domain))
            {
                return vc.LogInAsUser(MFAuthType.MFAuthTypeSpecificWindowsUser, userName, pwd, domain);
            }
            else
            {
                return vc.LogInAsUser(MFAuthType.MFAuthTypeSpecificMFilesUser, userName, pwd, "");
            }
        }
        /// <summary>
        /// 连接MFiles库
        /// </summary>
        /// <returns>MFiles 对象Vault</returns>
        public static Vault GetVault(UserDto user, string vaultGuid, bool login=true)
        {
            //string vaultName = "设计云";
            Vault vault = null;
            var clientApp = new MFilesClientApplication();
            VaultConnections conns = clientApp.GetVaultConnections();
            string domain = user.Domain;
       //     Writelog(string.Format(" in GetVault,userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, user.Domain));
            foreach (VaultConnection vConn in conns)
            {
                if (vConn.GetGUID() == vaultGuid)
                {
                    var loggedIn = vConn.IsLoggedIn();
                    if (!loggedIn && !login) return null;
                    if (vConn.IsLoggedIn())
                    {
                        vault = vConn.BindToVault(IntPtr.Zero, true, true);
                        string account = (domain == "" ? user.UserName : domain + "\\" + user.UserName);
                        if (vault.SessionInfo.AccountName != account)
                        {
                            vault.LogOutWithDialogs(IntPtr.Zero);
                            vault = LoginVault(vConn, user.UserName, user.Password, domain);
                        }
                    }
                    else
                    {
                        vault = LoginVault(vConn, user.UserName, user.Password, domain);
                    }
                    break;
                }
            }
            return vault;
        }

        public static void LogoutVault(string vaultGuid)
        {
            var app = new MFilesClientApplication();
            var vcs = app.GetVaultConnectionsWithGUID(vaultGuid);
            foreach (VaultConnection vc in vcs)
            {
                if (vc.IsLoggedIn())
                {
                    var v = vc.BindToVault(IntPtr.Zero, true, true);
                    try
                    {
                        v.LogOutWithDialogs(IntPtr.Zero);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 对"参数oVault是否有效"进行核实，若登出，则重连
        /// </summary>
        /// <param name="oVault">MFilesAPI.Vault</param>
        /// <param name="user"></param>
        /// <param name="vaultGuid">客户端库GUID</param>
        public static Vault Connect2Vault(Vault oVault, UserDto user, string vaultGuid)
        {
          //  Writelog(string.Format(" in Connect2Vault,userName={0}, pwd={1}, domai={2}", user.UserName, user.Password, user.Domain));
            Vault vault = oVault;
            if (!oVault.LoggedIn)
            {
                var clientApp = new MFilesClientApplication();
                VaultConnections conns = clientApp.GetVaultConnections();
                foreach (VaultConnection vConn in conns)
                {
                    if (vConn.GetGUID() == vaultGuid)
                    {
                        vault = LoginVault(vConn, user.UserName, user.Password, user.Domain);
                        break;
                    }
                }
            }
            return vault;
        }
        //添加基本搜索条件,类别、未删除
        public static void AddSearchBaseCondition(SearchConditions oSearchConditions, int classId)
        {
            var oSearchCondition1 = new SearchCondition();
            oSearchCondition1.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition1.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            oSearchCondition1.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            oSearchConditions.Add(-1, oSearchCondition1);
            var oSearchCondition2 = new SearchCondition();
            oSearchCondition2.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition2.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            oSearchCondition2.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            oSearchConditions.Add(-1, oSearchCondition2);
        }

        //public static async Task<string> SearchView(Vault vault, string search)
        //{
        //    var viewName = "搜索：" + search + " - "+DateTime.Now.ToString("yyyyMMddHHmmss");
        //    var oViewNew = new View { Name = viewName };
        //    var oSc = new SearchCriteria
        //    {
        //        FullTextSearchString = search,
        //        FullTextSearchFlags = (MFFullTextSearchFlags.MFFullTextSearchFlagsStemming
        //                               | MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData
        //                               | MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData
        //                               | MFFullTextSearchFlags.MFFullTextSearchFlagsTypeAnyWords),
        //        SearchFlags = MFSearchFlags.MFSearchFlagNone,
        //        ExpandUI = false
        //    };

        //    var oExpression = new Expression();
        //    oExpression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
        //    oExpression.DataStatusValueDataFunction = MFDataFunction.MFDataFunctionNoOp;
        //    var oTypedValue = new TypedValue();
        //    oTypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
        //    var oDeletedEx = new SearchConditionEx();
        //    oDeletedEx.SearchCondition.Set(oExpression, MFConditionType.MFConditionTypeEqual, oTypedValue);
        //    oDeletedEx.Enabled = true;
        //    oDeletedEx.Ignored = false;
        //    oDeletedEx.SpecialNULL = false;
        //    oSc.AdditionalConditions.Add(-1, oDeletedEx);

        //    var oViewNew1 =
        //        await Task.Run(() => vault.ViewOperations.AddTemporarySearchView(oViewNew, oSc)).ConfigureAwait(false);
        //    return await Task.Run(() => vault.ViewOperations.GetViewLocationInClient(oViewNew1.ID)).ConfigureAwait(false);
        //}

        internal static SearchConditionEx GetDelStatusSearchEx(bool deleted)
        {
            var delExp = new Expression();
            delExp.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delExp.DataStatusValueDataFunction = MFDataFunction.MFDataFunctionNoOp;
            var delTv = new TypedValue();
            delTv.SetValue(MFDataType.MFDatatypeBoolean, deleted);
            var delSearchEx = new SearchConditionEx();
            delSearchEx.SearchCondition.Set(delExp, MFConditionType.MFConditionTypeEqual, delTv);
            delSearchEx.Enabled = true;
            delSearchEx.Ignored = false;
            delSearchEx.SpecialNULL = false;
            return delSearchEx;
        }
    }
}
