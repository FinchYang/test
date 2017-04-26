using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using MFilesAPI;
using MfNotification.Core.NotifyObject;
using System.Net.Http;
using Microsoft.AspNet.SignalR.Client;

namespace Notification
{
    // [Guid("6EC27C89-EFF2-4E1E-9E86-C2717C3DA793")]
    [Guid("6E8ADBC6-2E94-4B6C-8849-5A55359E863B")]
    public interface INotification
    {
        [DispId(1)]
        void LogNewNotice(Vault vault, ObjVer objver, string server="");
        [DispId(3)]
        void LogCheckInNotice(Vault vault, ObjVer objver, string server = "");
        [DispId(12)]
        void LogDelNotice(Vault vault, ObjVer objver, string server = "");
    }

    [Guid("7B634074-B8AB-4A14-98D6-1492BE90804B")]

    [ClassInterface(ClassInterfaceType.None)]

    [ProgId("Notification.Notification")]

    public class Notification : INotification
    {
       // private static readonly string tmpfile;
        //    private const string Logfile = "NotificationComLog.txt";
        private static readonly string Webserver;
        private static readonly string Logfile;
        private static IHubProxy HubProxy { get; set; }
        private static HubConnection Connection { get; set; }
        ~Notification()
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }
        static Notification()
        {
          //  tmpfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Logfile);
            try
            {
                Logfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NoticeComLog.txt");
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                Webserver = config.AppSettings.Settings["notificationserver"].Value;
                WritelogStatic(string.Format("read notificationserver from app.config, the value is {0},System.Environment.CurrentDirectory={1}", Webserver, System.Environment.CurrentDirectory));
            }
            catch (Exception ex)
            {
                WritelogStatic(string.Format("read notificationserver from app.config, error {0}.", ex.Message));
            }
            try
            {
                string ServerURI = string.Format("http://{0}/", Webserver);
                WritelogStatic("CreateHubProxy-" + ServerURI);
                Connection = new HubConnection(ServerURI);
                HubProxy = Connection.CreateHubProxy("CscecPushHub");
            }
            catch (Exception ex)
            {
                WritelogStatic("CreateHubProxy." + ex.Message);
            }
        }
        static void WritelogStatic(string logtext)
        {
            try
            {
                using (var sw = System.IO.File.AppendText(Logfile))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }

        private async void pushmsg(MfTask mt)
        {
            if (Connection.State.Equals(Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected))
            {
                try
                {
                    //string ServerURI = string.Format("http://{0}/", Webserver);
                    //WritelogStatic("CreateHubProxy-" + ServerURI);
                    //Connection = new HubConnection(ServerURI);
                    //HubProxy = Connection.CreateHubProxy("CscecPushHub");
                    await Connection.Start();
                }
                catch (HttpRequestException hex)
                {
                    WritelogStatic(
                        "pushmsg.HttpRequestException" +
                        hex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    WritelogStatic("pushmsg." + ex.Message);
                    return;
                }
            }
            try
            {
                await HubProxy.Invoke("PushMsg", mt);
            }
            catch (Exception ex)
            {
                WritelogStatic("HubProxy.Invoke(PushMsg, mt);." + ex.Message);
            }
        }
        private  void Logonenotice(Vault vault, MfTask mt, string server)
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                foreach (string userId in mt.UserIds)
                {
                    mt.UserNameLists.Add(vault.UserOperations.GetUserAccount(int.Parse(userId)).LoginName);
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("Logonenotice,mt.UserNameLists.Add(vault.UserOperations.GetUserAccount(int.Parse(userId)).LoginName);{0},{1},{2}", vault.Name, server, ex.Message));
            }
            pushmsg(mt);
            //var jsonSerializer = new JavaScriptSerializer();
            //var paras = jsonSerializer.Serialize(mt);
            //var url = string.Format("http://{0}/api/notices?request={1}", server, paras);
            //var response1 = string.Empty;
            //try
            //{
            //    var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            //    using (var http = new HttpClient(handler))
            //    {
            //        //await异步等待回应
            //        var tmp = new Dictionary<string, string> { { "info", "selri" } };
            //        var request = new FormUrlEncodedContent(tmp);
            //        var response = await http.PostAsync(url, request);
            //        response.EnsureSuccessStatusCode();
            //        watch.Stop();
            //        response1 = await response.Content.ReadAsStringAsync();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Writelog(string.Format("logtask error：url={0},paras={1},{2}", url, paras, ex.Message));
            //}
            watch.Stop();
            Writelog(string.Format("in Logonetask,userid={0},id={1},Name={2},Type={3},ElapsedMilliseconds={4}，vault={5},userids.count={6},.", mt.UserId, mt.Id, mt.Name, mt.Type,
                                  watch.ElapsedMilliseconds, mt.VaultGuid, mt.UserIds.Count));
        }
        public void Writelog(string logtext)
        {
            try
            {
                using (var sw = System.IO.File.AppendText(Logfile))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }
        public void LogNewNotice(Vault vault, ObjVer objver, string server = "")
        {
            var watch = new Stopwatch();
            watch.Start();

            var pvs = vault.ObjectPropertyOperations.GetProperties(objver);
            var otask = new MfTask
            {
                Version = objver.Version,
                ClientName = vault.Name,
                LastModifiedTime = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                      .GetValueAsLocalizedText(),
                Type = objver.Type,
                VaultGuid = vault.GetGUID(),
                Id = objver.ID,
                Name = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle).
                    Value.GetValueAsLocalizedText(),
                Time = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).Value.GetValueAsLocalizedText(),

                Url = vault.ObjectOperations.GetMFilesURLForObject(objver.ObjID, objver.Version, true)
            };

            switch (objver.Type)
            {
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument:
                    otask.NotificationType = NotificationTypeEnum.NewDoc;
                    break;
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment:
                    otask.NotificationType = NotificationTypeEnum.NewTask;
                    break;
                default:
                    otask.NotificationType = NotificationTypeEnum.NewOtherObj;
                    break;
            }

            Lognotices(vault, objver, otask, server.Trim());
            watch.Stop();
            Writelog(string.Format("LogNewNotice record,ElapsedMilliseconds={0},objver.type={1},id={2},version={4}, vault.Name={5},server={6}", watch.ElapsedMilliseconds, objver.Type, objver.ID, objver.ObjID, objver.Version, vault.Name, server));
        }

        public void LogCheckInNotice(Vault vault, ObjVer objver, string server = "")
        {
            var watch = new Stopwatch();
            watch.Start();
            var pvs = vault.ObjectPropertyOperations.GetProperties(objver);
            var objname = pvs.SearchForProperty((int)
                MFBuiltInPropertyDef.
                    MFBuiltInPropertyDefNameOrTitle).
                Value.GetValueAsLocalizedText();
            var time = pvs.SearchForProperty((int)
                MFBuiltInPropertyDef.
                    MFBuiltInPropertyDefCreated).
                Value.GetValueAsLocalizedText();
            var creator = pvs.SearchForProperty((int)
             MFBuiltInPropertyDef.
                 MFBuiltInPropertyDefCreatedBy).
             Value.GetValueAsLocalizedText();
            var classid =
              pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass).GetValueAsLocalizedText();
            Writelog(string.Format("CheckIn Log info,类别={0}，对象类型={1},对象id={2},version={4},名称={3}， vault.Name={5},creator={6}",
                classid, objver.Type, objver.ID, objname, objver.Version, vault.Name, creator));
            var otask = new MfTask
            {
                Version = objver.Version,
                ClientName = vault.Name,
                LastModifiedTime = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                      .GetValueAsLocalizedText(),
                Type = objver.Type,
                VaultGuid = vault.GetGUID(),
                Id = objver.ID,
                Name = objname,
                Time = time,
                Url = vault.ObjectOperations.GetMFilesURLForObject(objver.ObjID, objver.Version, true)
            };

            switch (objver.Type)
            {
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument:
                    otask.NotificationType = NotificationTypeEnum.UpdateDoc;
                    break;
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment:
                    if (DealTaskDone(otask, vault, objver, server.Trim()))
                    {
                        watch.Stop();
                        Writelog(string.Format("CheckIn Log End--TaskDone,Com耗时={0}毫秒,对象类型={1},对象id={2},version={4}, vault.Name={5},server={6}",
                  watch.ElapsedMilliseconds, objver.Type, objver.ID, objver.ObjID, objver.Version, vault.Name, server));
                        return;
                    }

                    otask.NotificationType = NotificationTypeEnum.UpdateTask;
                    break;
                default:
                    otask.NotificationType = NotificationTypeEnum.UpdateOtherObj;
                    break;
            }
            if (objver.Type != (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment)
            {
                var pvs1 = vault.ObjectPropertyOperations.GetProperties(objver);
                foreach (PropertyValue pv in pvs1)
                {
                    // Writelog(string.Format("in LogCheckInNotice,debug,type={0},id={1},{2}", objver.Type, objver.ID, pv.PropertyDef));
                    if (pv.PropertyDef == (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo)
                    {
                        otask.NotificationType = NotificationTypeEnum.WorkFlowAssigned;
                        break;
                    }
                }
            }
            Lognotices(vault, objver, otask, server.Trim());
            watch.Stop();
            Writelog(string.Format("CheckIn Log End,Com耗时={0}毫秒,对象类型={1},对象id={2},version={4}, vault.Name={5},server={6}",
                watch.ElapsedMilliseconds, objver.Type, objver.ID, objver.ObjID, objver.Version, vault.Name, server));
        }

        private bool DealTaskDone(MfTask otask, Vault vault, ObjVer objver, string server)
        {
            try
            {
                if (objver.Version < 2) return false;

                var pvs = vault.ObjectPropertyOperations.GetProperties(objver);
                var taskdoneuserlist = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCompletedBy);
                if (PropertyIsNull(taskdoneuserlist)) return false;

                var lastobjver = new ObjVer();
                lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - 1);
                var pvslast = vault.ObjectPropertyOperations.GetProperties(lastobjver);
                var taskdoneuserlistlast =
                    pvslast.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCompletedBy);
                //Writelog(
                //    string.Format(
                //        "in dealTaskDone,debug,MFBuiltInPropertyDefCompletedBy={0},taskdoneuserlist={1},taskdoneuserlistlast={2}",
                //        (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCompletedBy, taskdoneuserlist.Value.DisplayValue,
                //        taskdoneuserlistlast.Value.DisplayValue));
                if (PropertyIsNull(taskdoneuserlistlast) || taskdoneuserlist.Value.DisplayValue != taskdoneuserlistlast.Value.DisplayValue)
                {
                    var monitor = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy);
                    otask.UserIds.Add(monitor.TypedValue.GetValueAsLookup().Item.ToString(CultureInfo.InvariantCulture));
                    otask.NotificationType = NotificationTypeEnum.TaskDone;
                    Logonenotice(vault, otask, server);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("dealtaskdone error : objver.ID={0},type={1},version={2},message={3}", objver.ID, objver.Type, objver.Version, ex.Message));
            }
            return false;
        }

        private bool PropertyIsNull(PropertyValue taskdoneuserlist)
        {
            return taskdoneuserlist.Value.IsUninitialized() || taskdoneuserlist.Value.IsNULL() ||
                   taskdoneuserlist.Value.IsEmpty();
        }

        private void Lognotices(Vault vault, ObjVer objver, MfTask otask, string server)
        {
            //   Writelog(string.Format("in Lognotices,objver.type={0},id={1},vaultname={2}", objver.Type, objver.ID, vault.Name));
            var userIds = new Lookups();
            try
            {
                userIds = vault.ObjectPropertyOperations.GetProperty(objver, (int)MFBuiltInPropertyDef.
                                                                                       MFBuiltInPropertyDefAssignedTo)
                    .Value.GetValueAsLookups();
            }
            #region no propertydefassignedto
            catch (Exception ex)
            {
                //  Writelog(string.Format("in Lognotices,big catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name,ex.Message ));
                try
                {
                    var perms = vault.ObjectOperations.GetObjectPermissions(objver);
                    var aceks = perms.CustomACL
                                                       ? perms.AccessControlList.CustomComponent.AccessControlEntries.
                                                             GetKeysWithPseudoUserDefinitions()
                                                       : perms.NamedACL.AccessControlList.CustomComponent.
                                                             AccessControlEntries.GetKeysWithPseudoUserDefinitions();
                    var aclc = perms.CustomACL ? perms.AccessControlList.CustomComponent
                                                      : perms.NamedACL.AccessControlList.CustomComponent;
                    Writelog(string.Format("perms：Count={0},CustomACL={1},Type={2},ID={3}", aceks.Count, perms.CustomACL, objver.Type, objver.ID));
                    foreach (AccessControlEntryKey acek in aceks)
                    {
                        Writelog(string.Format("perms：IsGroup={0},IsPseudoUser={1},PseudoUserID={2},UserOrGroupID={3}", acek.IsGroup, acek.IsPseudoUser, acek.PseudoUserID, acek.UserOrGroupID));
                        if (acek.UserOrGroupID == 1) continue;//m-files api 获取结果不正确，暂时回避内部所有用户
                        #region deal with group
                        if (acek.IsGroup)
                        {
                            if (acek.IsPseudoUser)
                            {
                                foreach (UserOrUserGroupID uoug in acek.GetResolvedPseudoUserOrGroupIDs())
                                {
                                    #region deal with MFUserOrUserGroupTypeUserAccount
                                    if (uoug.UserOrGroupType == MFUserOrUserGroupType.MFUserOrUserGroupTypeUserAccount)
                                    {
                                        try
                                        {
                                            Writelog(string.Format("in Lognotices,big catch,first little try111,type={0},id={1},UserOrGroupID={2}", objver.Type, objver.ID, uoug.UserOrGroupID));
                                            //var perm = aclc.GetACEByUserOrGroupID(uoug.UserOrGroupID, false);
                                            //Writelog(string.Format("in Lognotices,big catch,first little try222,type={0},id={1},vaultname={2}", objver.Type, objver.ID, vault.Name));
                                            //var url =
                                            //    string.Format(
                                            //        "user1,AttachObjectsPermission={0},DeletePermission={1},ChangePermissionsPermission={2},EditPermission={3},ReadPermission={4},UserOrGroupID={5},CustomACL={6},ID={7}",
                                            //        perm.AttachObjectsPermission, perm.DeletePermission,
                                            //        perm.ChangePermissionsPermission, perm.EditPermission, perm.ReadPermission, uoug.UserOrGroupID, perms.CustomACL, objver.ID);
                                            //Writelog(string.Format("url：{0}", url)); //{"Id":1,"Name":"小白"}
                                            //if (Checkperm(perm)) continue;
                                            var lu = new Lookup { Item = uoug.UserOrGroupID };
                                            if (userIds.GetLookupIndexByItem(lu.Item) < 0)
                                            {
                                                userIds.Add(-1, lu);
                                            }
                                        }
                                        catch (Exception ex1)
                                        {
                                            Writelog(string.Format("in Lognotices,big catch,first little catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name, ex1.Message));
                                        }
                                    }
                                    #endregion deal with MFUserOrUserGroupTypeUserAccount
                                    #region deal with MFUserOrUserGroupTypeUserGroup
                                    else if (uoug.UserOrGroupType == MFUserOrUserGroupType.MFUserOrUserGroupTypeUserGroup)
                                    {
                                        try
                                        {
                                            Writelog(string.Format("in Lognotices,big catch,second little try 111,type={0},id={1},UserOrGroupID={2},acek.UserOrGroupID={3}", objver.Type, objver.ID, uoug.UserOrGroupID, acek.UserOrGroupID));
                                            //var perm = aclc.GetACEByUserOrGroupID(uoug.UserOrGroupID, true);
                                            //Writelog(string.Format("in Lognotices,big catch,second little try 222,type={0},id={1},vaultname={2}", objver.Type, objver.ID, vault.Name));
                                            //var url =
                                            //   string.Format(
                                            //        "group1,AttachObjectsPermission={0},DeletePermission={1},ChangePermissionsPermission={2},EditPermission={3},ReadPermission={4},UserOrGroupID={5},CustomACL={6},ID={7}",
                                            //        perm.AttachObjectsPermission, perm.DeletePermission,
                                            //        perm.ChangePermissionsPermission, perm.EditPermission, perm.ReadPermission, uoug.UserOrGroupID, perms.CustomACL, objver.ID);
                                            //Writelog(string.Format("url：{0}", url)); //{"Id":1,"Name":"小白"}
                                            //if (Checkperm(perm)) continue;
                                            var retul = GetUsers(vault, uoug.UserOrGroupID);
                                            foreach (var lu in retul)
                                            {
                                                if (userIds.GetLookupIndexByItem(lu.Item) < 0)
                                                {
                                                    userIds.Add(-1, lu);
                                                }
                                            }
                                        }
                                        catch (Exception ex2)
                                        {
                                            Writelog(string.Format("in Lognotices,big catch,2nd little catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name, ex2.Message));
                                        }
                                    }
                                    #endregion deal MFUserOrUserGroupTypeUserGroup
                                    //    Writelog(
                                    //string.Format(
                                    //    "in LogCheckInNotice,catch'catch,UserOrGroupID={0},IsPseudoUser={1},Type={2},ID={3},UserOrGroupID={4},UserOrGroupType={5}",
                                    //    acek.UserOrGroupID,
                                    //    acek.IsPseudoUser, objver.Type, objver.ID, uoug.UserOrGroupID, uoug.UserOrGroupType));
                                }
                                continue;
                            }
                            IEnumerable<Lookup> ul = new List<Lookup>();
                            try
                            {
                                var perm = aclc.GetACEByUserOrGroupID(acek.UserOrGroupID, true);
                                var url =
                                               string.Format(
                                                "group2,AttachObjectsPermission={0},DeletePermission={1},ChangePermissionsPermission={2},EditPermission={3},ReadPermission={4},UserOrGroupID={5},CustomACL={6},ID={7}",
                                                perm.AttachObjectsPermission, perm.DeletePermission,
                                                perm.ChangePermissionsPermission, perm.EditPermission, perm.ReadPermission, acek.UserOrGroupID, perms.CustomACL, objver.ID);
                                Writelog(string.Format("url：{0}", url));
                                if (Checkperm(perm)) continue;
                                ul = GetUsers(vault, acek.UserOrGroupID);
                                //  ul = GetUsers(vault, uoug.UserOrGroupID);
                            }
                            catch (Exception exson)
                            {
                                Writelog(
                                    string.Format(
                                        "in Lognotices,catch'catch,UserOrGroupID={0},IsPseudoUser={1},Type={2},ID={3},Message={4}",
                                        acek.UserOrGroupID,
                                        acek.IsPseudoUser, objver.Type, objver.ID, exson.Message));
                            }
                            try
                            {
                                foreach (var lu in ul)
                                {
                                    if (userIds.GetLookupIndexByItem(lu.Item) < 0)
                                    {
                                        userIds.Add(-1, lu);
                                    }
                                }
                            }
                            catch (Exception ex3)
                            {
                                Writelog(string.Format("in Lognotices,big catch,third little catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name, ex3.Message));
                            }
                        }
                        #endregion deal with group
                        else
                        {
                            try
                            {
                                var perm = aclc.GetACEByUserOrGroupID(acek.UserOrGroupID, false);
                                var url = string.Format(
                                                    "user2,AttachObjectsPermission={0},DeletePermission={1},ChangePermissionsPermission={2},EditPermission={3},ReadPermission={4},UserOrGroupID={5},CustomACL={6},ID={7}",
                                                    perm.AttachObjectsPermission, perm.DeletePermission,
                                                    perm.ChangePermissionsPermission, perm.EditPermission, perm.ReadPermission, acek.UserOrGroupID, perms.CustomACL, objver.ID);
                                Writelog(string.Format("url：{0}", url)); //{"Id":1,"Name":"小白"}
                                if (Checkperm(perm)) continue;
                                var lu = new Lookup { Item = acek.UserOrGroupID };
                                if (userIds.GetLookupIndexByItem(lu.Item) < 0)
                                {
                                    userIds.Add(-1, lu);
                                }
                            }
                            catch (Exception ex4)
                            {
                                Writelog(string.Format("in Lognotices,big catch,4th little catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name, ex4.Message));
                            }
                        }
                    }
                }
                catch (Exception exx)
                {
                    Writelog(string.Format("in Lognotices,in big catch,another catch,type={0},id={1},vaultname={2},exception={3}", objver.Type, objver.ID, vault.Name, exx.Message));
                }
                //   Writelog(string.Format("in Lognotices,end big catch,type={0},id={1},vaultname={2}", objver.Type, objver.ID, vault.Name));
            }
            #endregion no propertydefassignedto
            try
            {
                foreach (Lookup a in userIds)
                {
                    otask.UserIds.Add(a.Item.ToString(CultureInfo.InvariantCulture));
                }
                //Writelog(string.Format("Users Info,type={0},id={1},vaultname={2},counts={3},users={4}",
                //    objver.Type, objver.ID, vault.Name, userIds.Count, userIds.ToString()));

                Logonenotice(vault, otask, server);
            }
            catch (Exception ex)
            {
                Writelog(string.Format("in Lognotices,Logonetask,type={0},id={1},{2}", objver.Type, objver.ID, ex.Message));
            }
            // Writelog(string.Format("in Lognotices,the end,type={0},id={1},vaultname={2}", objver.Type, objver.ID, vault.Name));
        }

        private bool Checkperm(AccessControlEntryData perm)
        {

            return !(
                //perm.AttachObjectsPermission == MFPermission.MFPermissionAllow ||
                   perm.ChangePermissionsPermission == MFPermission.MFPermissionAllow ||
                   perm.DeletePermission == MFPermission.MFPermissionAllow ||
                   perm.EditPermission == MFPermission.MFPermissionAllow ||
                   perm.ReadPermission == MFPermission.MFPermissionAllow);
        }

        public void LogDelNotice(Vault vault, ObjVer objver, string server = "")
        {
            var pvs = vault.ObjectPropertyOperations.GetProperties(objver);
            var otask = new MfTask
            {
                Version = objver.Version,
                ClientName = vault.Name,
                LastModifiedTime = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                      .GetValueAsLocalizedText(),
                Type = objver.Type,
                VaultGuid = vault.GetGUID(),
                Id = objver.ID,
                Name = pvs.SearchForProperty((int)
                                                                  MFBuiltInPropertyDef.
                                                                      MFBuiltInPropertyDefNameOrTitle).
                    Value.GetValueAsLocalizedText(),
                Time = pvs.SearchForProperty((int)
                                                                  MFBuiltInPropertyDef.
                                                                      MFBuiltInPropertyDefCreated).
                    Value.GetValueAsLocalizedText(),

                Url = vault.ObjectOperations.GetMFilesURLForObject(objver.ObjID, objver.Version, true)
            };
            switch (objver.Type)
            {
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument:
                    otask.NotificationType = NotificationTypeEnum.DelDoc;
                    break;
                case (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment:
                    otask.NotificationType = NotificationTypeEnum.Other;
                    break;
                default:
                    otask.NotificationType = NotificationTypeEnum.DelOtherObj;
                    break;
            }
            Lognotices(vault, objver, otask, server.Trim());
        }
        private static IEnumerable<Lookup> GetUsers(Vault oVault, int ugid)
        {
            var ug = oVault.UserGroupOperations.GetUserGroup(ugid);
            var ul = new List<Lookup>();
            foreach (var ugoru in ug.Members)
            {
                var id = int.Parse(ugoru.ToString());
                if (id < 0)
                {
                    var ll = GetUsers(oVault, -id);
                    ul.AddRange(ll);
                }
                else
                {
                    ul.Add(new Lookup { Item = id });
                }
            }
            return ul;
        }


    }
}
