using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using AecCloud.WebAPI.Models.DataAnnotations;
using DBWorld.AecCloud.Web.Controllers;
using DBWorld.AecCloud.Web.Models;
using log4net;
using MFilesAPI;
using MfNotification.Core.NotifyObject;

namespace DBWorld.AecCloud.Web.Api
{
    public class NoticesController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProjectMemberService _projMemberService;
        private readonly IProjectService _projService;
        private readonly IMFilesVaultService _mfvaultService;
        // private readonly ITasksService _tasksService;
        private readonly IUserService _iUserService;
        private readonly IVaultAppService _vaultappService;
        private readonly IVaultServerService _vaultServerService;
        public NoticesController(IProjectService projectService, IProjectMemberService projectMemberService,
            IMFilesVaultService mfvaultService, IUserService userService, IVaultServerService vaultServerService, IVaultAppService vaultappService)
        {
            //   _tasksService = tasksService;
            _projService = projectService;
            _vaultServerService = vaultServerService;
            _projMemberService = projectMemberService;
            _mfvaultService = mfvaultService;
            _iUserService = userService;
            _vaultappService = vaultappService;
        }
        private VaultAppModel ToModel(VaultApp app, bool isUpdate = false)
        {
            var vam = new VaultAppModel
            {
                AppId = app.Id,
                Guid = app.Guid,
                IsUpdate = isUpdate,
                Version = app.Version
            };
            try
            {
                vam.ZipFile = File.ReadAllBytes(app.Filepath);
            }
            catch (Exception ex)
            {
                Log.Info(app.Filepath + ex.Message);
            }
            return vam;
        }

        public IHttpActionResult GetNoticeUpdatePackage(long version)
        {
            var ret = new UpdateInfo();
            try
            {
                var ppath = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~"), "installer");
                Log.Info("path:" + ppath);
                var di = new DirectoryInfo(ppath).GetFiles();

                foreach (FileInfo fileInfo in di)
                {
                    if (fileInfo.Name.Contains("NoticeSetup"))
                    {
                        var tmp = fileInfo.Name.Replace(".", "");
                        var reg = new Regex(@"\d+");
                        var m = reg.Match(tmp).ToString();
                        if (long.Parse(m) > version)
                        {
                            ret.Name = fileInfo.Name;
                            ret.Date = fileInfo.CreationTime.ToLocalTime().ToString("F");
                            ret.FileContent = File.ReadAllBytes(fileInfo.FullName);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("noticescontroller,GetNoticeUpdatePackage error:{0}", version), ex);
            }

            return Ok(ret);
        }
        public async Task<IHttpActionResult> GetApp(string version)
        {
            var appList = new List<VaultAppModel>();
            try
            {
                var apps0 = await Task.Run(() => _vaultappService.GetById(1));
                if (int.Parse(version) < int.Parse(apps0.Version))
                    appList.Add(ToModel(apps0, true));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("noticescontroller,GetApp error:{0}", version), ex);
            }

            return Ok(appList);
        }
        IEnumerable<ProjectDto> GetProjects4User(string uname)
        {
            var list = new List<ProjectDto>();
            try
            {
                var user = _iUserService.GetUserByAccountName(uname);
                var projs = _projMemberService.GetProjectsByUser(user.Id);
                //    Log.InfoFormat(" GetProjects4User _projectMemberService.GetProjectsByUser count={0}, userId: {1},{2} ", projs.Count, user.Id,uname);
                foreach (var m in projs)
                {
                    try
                    {
                        var proj = _projService.GetProjectById(m.ProjectId);
                        if (proj == null)
                        {
                            //   Log.WarnFormat("项目({0})已被删除", m.ProjectId);
                            continue;
                        }
                        var vault = _mfvaultService.GetVaultById(proj.VaultId);
                        if (vault == null)
                        {
                            //   Log.WarnFormat("文档库({0})已被删除", proj.VaultId);
                            continue;
                        }
                        var projDto = proj.ToDto(vault, false);
                        list.Add(projDto);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("用户项目 ProjectId= {1} 已被清除：{0}", ex.Message, m.ProjectId), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取用户项目异常：{0}", ex.Message), ex);
            }
            //   Log.Info(string.Format("noticecontroller GetProjects4User return list count={0} ", list.Count));
            return list;
        }
        public string GetContractor(string username)
        {
            var thevault = _projService.GetContractorVault();
            if (thevault == null)
            {
                return string.Empty;
            }
            var ret = string.Empty;
            var guid = thevault.Guid;
            try
            {
                var serv = _vaultServerService.GetServer();
                var app = MFServerUtility.ConnectToServer(serv);

                var vault = app.LogInToVault(guid);
                var users = vault.UserOperations.GetUserAccounts();
                vault.LogOutSilent();
                app.Disconnect();
                foreach (UserAccount userAccount in users)
                {
                    if (userAccount.LoginName == username)
                    {
                        ret = guid + serv.Ip;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("noticescontroller,GetContractor connect error:{0}", username), ex);
            }
            return ret;
        }
        public string GetProjects(string username)
        {
            try
            {
                var jsonSerializer = new JavaScriptSerializer();

                var lists = GetProjects4User(username);
                var clients = new List<MfilesClientConfig>();
                var app = MFServerUtility.ConnectToServer(_vaultServerService.GetServer());
                foreach (ProjectDto projectDto in lists)
                {
                    Vault vault;
                    try
                    {
                        vault = app.LogInToVault(projectDto.Vault.Guid);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    vault.LogOutSilent();
                    clients.Add(new MfilesClientConfig
                    {
                        Guid = projectDto.Vault.Guid,
                        Name = projectDto.Name,
                        Host = projectDto.Vault.Server.Ip
                    });
                }
                app.Disconnect();
                var paras = jsonSerializer.Serialize(clients);
                return paras;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("noticescontroller,GetProjects error:{0}", username), ex);
            }
            return "";
        }
        //public string Get(string request)
        //{
        //    try
        //    {
        //        var jsonSerializer = new JavaScriptSerializer();
        //        var otask = jsonSerializer.Deserialize<MfTask>(request);

        //        var lists = _tasksService.GetTasksByUser(otask.UserId, otask.VaultGuid);
        //        foreach (var taskse in lists)
        //        {
        //            _tasksService.DeleteTasks(taskse);
        //        }

        //        var paras = jsonSerializer.Serialize(lists);
        //        return paras;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(string.Format("get error:{0}", request), ex);
        //    }
        //    return "";
        //}
        public string GetAllTasks(string request)
        {
            try
            {
                var jsonSerializer = new JavaScriptSerializer();
                var otask = jsonSerializer.Deserialize<RequestAllTasks>(request);
                var lists = new List<MfTask>();
                var serv = _vaultServerService.GetServer();
                Log.Info(string.Format("GetAllTasks,{0},{1},{2},{3}", serv.Ip, serv.LocalIp, serv.Port, otask.Guids.Count));
                var app = MFServerUtility.ConnectToServer(otask.UserName, otask.PassWord, serv.LocalIp, serv.Port);
               // app.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, otask.UserName, otask.PassWord, "", "ncacn_ip_tcp", serv.LocalIp, serv.ServerPort);
                foreach (string guid in otask.Guids)
                {
                    try
                    {
                        var vault = app.LogInToVault(guid);
                        Log.Info(string.Format("GetAllTasks,check {0} ,{1} ", vault.CurrentLoggedInUserID, vault.SessionInfo.UserID));
                        var pos = vault.Name.LastIndexOf('-');
                        if (pos < 1) pos = vault.Name.Length;
                        var tasktitle = vault.Name.Substring(0, pos) + " ";
                        lists.AddRange(GetTaskApprove(vault, vault.CurrentLoggedInUserID, tasktitle));
                        lists.AddRange(GetTaskWorkflow(vault, vault.SessionInfo.UserID, tasktitle));
                        vault.LogOutSilent();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("GetAllTasks vault:{0},{1}", guid, ex.Message.Substring(0, 90)));
                    }
                }
                app.Disconnect();
                Log.Info(string.Format("GetAllTasks,{0}  tasks", lists.Count));
                var paras = jsonSerializer.Serialize(lists);
                return paras;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("GetAllTasks error:{0}", request), ex);
            }
            return "";
        }
        private IEnumerable<MfTask> GetTaskWorkflow(Vault vault, int? mfuserid, string tasktitle)
        {
            var forworklist = new List<MfTask>();
            try
            {//工作流任务
                var scs = new SearchConditions();
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeNotEqual;
                    sc.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);

                    sc.TypedValue.SetValueToLookup(new Lookup
                    {
                        Item = (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment
                    });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = (int)mfuserid });
                    scs.Add(-1, sc);
                }
                var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                  MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
                Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, ovs.Count));
                foreach (ObjectVersion objectVersion in ovs)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                    //       Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "aaa"));
                    var link = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID,
                        objectVersion.ObjVer.Version, true);
                    //    Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "bbb"));
                    var Name = pvs.SearchForProperty(0).GetValueAsLocalizedText();
                    //    Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "ccc"));
                    var Assigner =
                            pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                                .GetValueAsLocalizedText();
                    //     Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "ddd"));
                    var Content = string.Empty;
                    try
                    {
                        Content = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription)
                          .GetValueAsLocalizedText();
                    }
                    catch (Exception) { }
                    //     Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "eee"));
                    var Date =
                        pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated)
                            .GetValueAsLocalizedText();
                    //      Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "fff"));
                    var tonn = new MfTask
                    {
                        Url = link,
                        Name = Name,
                        Createby = Assigner,
                        ClientName = vault.Name,
                        LastModifiedTime = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                                .GetValueAsLocalizedText(),
                        Desc = Content,
                        Time = Date
                    };
                    forworklist.Add(tonn);
                }
            }
            catch (Exception ex)
            {
                Log.Info("GetTaskWorkflow error:" + ex.Message);
            }
            return forworklist;
        }

        private IEnumerable<MfTask> GetTaskApprove(Vault vault, int? mfuserid, string tasktitle)
        {
            var forworklist = new List<MfTask>();
            try
            {
                var ClassNotification =
                         vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                             MfilesAliasConfig.ClassNotification);
                //      var ClassTaskApprove =
                //vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                //    MfilesAliasConfig.ClassTaskApprove);
                var scs = new SearchConditions();
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);

                    sc.TypedValue.SetValueToLookup(new Lookup
                    {
                        Item = (int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment
                    });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeNotEqual;
                    sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = ClassNotification });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = (int)mfuserid });
                    scs.Add(-1, sc);
                }
                var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                  MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
                //    Log.Info(string.Format("mfuserid:{0},tasks:{1},ClassTaskApprove:{2}", mfuserid, ovs.Count, ClassTaskApprove));

                foreach (ObjectVersion objectVersion in ovs)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                    var link = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID,
                        objectVersion.ObjVer.Version, true);
                    var taskname = pvs.SearchForProperty(0).GetValueAsLocalizedText();

                    var relations = vault.ObjectOperations.GetRelationships(objectVersion.ObjVer,
                         MFRelationshipsMode.MFRelationshipsModeAll);
                    var creator = string.Empty;
                    foreach (ObjectVersion relation in relations)
                    {
                        //   var objpvs = vault.ObjectPropertyOperations.GetProperties(relation.ObjVer);
                        //     var name = objpvs.SearchForProperty(0).GetValueAsLocalizedText();
                        //   creator = objpvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                        creator = vault.ObjectPropertyOperations.GetProperty(relation.ObjVer, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                        break;
                        //   Log.Info(string.Format("{4},relation: {0},{1},{2},{3},{5}", name, relation.ObjVer.Type, relation.ObjVer.ID, relation.ObjVer.Version, taskname, creator));
                    }
                    var taskOrNoticeOfVault = new MfTask
                    {
                        Url = link,
                        Name = taskname,
                        ClientName = vault.Name,
                        LastModifiedTime = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified)
                              .GetValueAsLocalizedText(),
                        Createby = creator,
                        Time = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).GetValueAsLocalizedText()
                    };
                    forworklist.Add(taskOrNoticeOfVault);
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("GetTaskApprove {0},{1} error:{2}", vault.Name, mfuserid, ex.Message));
            }
            return forworklist;
        }
        // POST api/notices
        //public string Post(string request)
        //{
        //    //   Writelog(string.Format("in post : income=-{0}-",request));
        //    try
        //    {
        //        var jsonSerializer = new JavaScriptSerializer();
        //        var otask = jsonSerializer.Deserialize<MfTask>(request);
        //        if (otask.ClientType == 0)
        //        {
        //            //foreach (string userNameL in otask.UserNameLists)
        //            //{

        //            //}
        //            foreach (var userid in otask.UserIds)
        //            {
        //                var task = new Tasks
        //                {
        //                    CreationTime = otask.Time,
        //                    IsNoticed = otask.IsNoticed,
        //                    Name = otask.Name,
        //                    Notificationtype = (int)otask.NotificationType,
        //                    Objectid = otask.Id,
        //                    Type = otask.Type,
        //                    Url = otask.Url,
        //                    Version = otask.Version,
        //                    Userid = userid,
        //                    Vaultguid = otask.VaultGuid
        //                };
        //                //    Writelog(string.Format("post info1:{0}", request));
        //                _tasksService.InsertTasks(task);
        //                //   Writelog(string.Format("post info2:{0}", request));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(string.Format("get error:{0}", request), ex);
        //       //  Writelog(string.Format("get error:{0},err={1}", request, ex.Message));
        //    }
        //    return "";
        //  //  return "ok,request is :" + request;
        //}
    }
}
