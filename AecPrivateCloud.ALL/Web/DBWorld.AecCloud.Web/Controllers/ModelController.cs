using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AecCloud.Core.Domain;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;
using DBWorld.AecCloud.Web.Models;
using log4net;
using MFilesAPI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class ModelController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserService _userService;
        private readonly IVaultServerService _mFilesVaultService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IProjectService _projService;
        private readonly IMFilesVaultService _mfvaultService;


        public ModelController(IVaultServerService mFilesVaultService, IUserService userService, IProjectService projService
            , IProjectMemberService projectMemberService, IMFilesVaultService mfvaultService
            , IAuthenticationManager authManager,
            SignInManager<User, long> signInManager, UserManager<User, long> userManager) :
            base(authManager, signInManager, userManager)
        {
            _mFilesVaultService = mFilesVaultService;
            _projService = projService;
            _userService = userService;
            _projectMemberService = projectMemberService;
            _mfvaultService = mfvaultService;
        }
        /// <summary>
        /// 单个项目的BIM模型列表
        /// </summary>
        /// <param name="guid">库的GUID</param>
        /// <param name="name">库的名称</param>
        /// <param name="typeid">模型type</param>
        /// <param name="objid">模型对象Id</param>
        /// <param name="ifcguid">构件Id</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Project(string guid, string name = "", int typeid = 0, long objid = 0, string ifcguid="")
        {
            ViewBag.CurrentVaultGuid = guid;
            return View();
        }
        //for test
        [Authorize]
        public ActionResult ModelList(string guid, string name = "", int typeid = 0, long objid = 0, string ifcguid = "")
        {
            ViewBag.CurrentVaultGuid = guid;
            return View();
        }
        public ActionResult TreeNodes(string guid)
        {
            var res = GetTreeNodes(guid);
            foreach (ObjInfo o in res)
            {
                var hasModel = false;
                if (o.Model != null)
                {
                    hasModel = HasModel(new ModelFile()
                    {
                        Guid = guid.Replace("{", "").Replace("}", ""),
                        ObjId = o.Model.ID,
                        TypeId = o.Model.Type
                    });
                }
                o.HasModel = hasModel;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        private IEnumerable<ObjInfo> GetTreeNodes(string guid)
        {
            var res = new List<ObjInfo>();
            try
            {
                var server = _mFilesVaultService.GetServer();
                Log.Info("GetTreeNodes-server Ip:" + server.Ip);
                var app = MFServerUtility.ConnectToServer(server);
                Log.Info("GetTreeNodes-vault Guid:" + guid);
                var vault = app.LogInToVault(guid);
                var units = MFModelUtil.GetUnits(vault).ToList();
                res.AddRange(units);
                foreach (ObjInfo u in units)
                {
                    var floors = MFModelUtil.GetFloors(vault, u.ID).ToList();
                    res.AddRange(floors);
                    foreach (ObjInfo f in floors)
                    {
                        var majors = MFModelUtil.GetDisciplines(vault, f.ID).ToList();
                        res.AddRange(majors);
                    }
                }

                vault.LogOutSilent();
                app.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error("GetTreeNodes:"+ ex.Message);
            }
            return res;
        }

        public ActionResult QaList(string guid, string classAlias, string state)
        {
            var res = GetQaList(guid, classAlias, state);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<Qa> GetQaList(string guid, string classAlias, string state)
        {
            var res = new List<Qa>();
            try
            {
                var server = _mFilesVaultService.GetServer();
                Log.Info("GetQaList-server Ip:" + server.Ip);
                var userName = AuthUtility.GetUserName(User);
                Log.Info("GetQaList-userName:" + userName);
                var password = AuthUtility.GetUserPassword(User);
                Log.Info("GetQaList-password:" + password);
                var app = MFServerUtility.ConnectToServer(userName, password, server.LocalIp, server.ServerPort);
                Log.Info("GetQaList-vault Guid:" + guid);
                var vault = app.LogInToVault(guid);
                var qas = MFModelUtil.GetQaList(vault, classAlias, state).ToList();
                Log.Info("GetQaList-qas Count:" + qas.Count);
                res.AddRange(qas);
                vault.LogOutSilent();
                app.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error("GetQaList:" + ex.Message);
            }
            return res;
        }
        IEnumerable<ProjectDto> GetProjects4CurrentUser()
        {
            var list = new List<ProjectDto>();
            try
            {
                var userId = AuthUtility.GetUserId(User);//User.Identity.GetUserId();
                var projs = _projectMemberService.GetProjectsByUser(userId);
                Log.InfoFormat("用户({0})获取项目总数：{1}", AuthUtility.GetUserName(User), projs.Count);
                foreach (var m in projs)
                {

                    var proj = _projService.GetProjectById(m.ProjectId);
                    if (proj == null) 
                    {
                    //    Log.WarnFormat("项目({0})已被删除", m.ProjectId);
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
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取用户项目异常：{0}", ex.Message), ex);
            }
            return list;
        }

        [Authorize]
        public ActionResult Index()
        {
            //先获取用户ID，一边后期在存储默认项目时区分不同用户使用
            try
            {
                var profile = GetUserProfile(_userManager);

                ViewBag.userId = profile.Id;
                var projs = GetProjects4CurrentUser();
                var currentuserid = long.Parse(User.Identity.GetUserId());
              //  var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                //  var userId =Authentication.User.Identity.GetUserId<int>();

                var user = _userService.GetUserById(currentuserid);
             //   var password = DBWorldCache.Get(user.Id.ToString());
                foreach (ProjectDto projectDto in projs)
                {
                //    Log.Info(string.Format("project center---{0},{1},{2}",
                //        currentuserid, loginName, projectDto.Vault.Name));
                    try
                    {
                        //Log.Info(string.Format("before connect:{0},{1},{2},{3},{4},{5},{6},{7},{8}", loginName, password,
                        //    projectDto.Vault.Server.Ip,
                        //    projectDto.Vault.Server.Port, user.Id, user.FullName, user.UserName, user.Password,
                        //    user.PasswordHash));
                        var app = MFServerUtility.ConnectToServer(_mfvaultService.GetServer(projectDto.VaultId));
                        ;
                        var vault = app.LogInToVault(projectDto.Vault.Guid);
                        var viewid = -1;
                        try
                        {
                            var views = vault.ViewOperations.GetViews();
                            foreach (View view in views)
                            {
                                if (view.Name == "主目录")
                                {
                                    viewid = view.ID;
                                    break;
                                }
                            }
                            if (viewid < 0)
                            {
                                foreach (View view in views)
                                {
                                    if (view.Name == "根目录")
                                    {
                                        viewid = view.ID;
                                        break;
                                    }
                                }
                            }
                            projectDto.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                        }
                        catch (Exception ex)
                        {
                            Log.Info("search time project :" + projectDto.Name, ex);
                        }
                        vault.LogOutSilent();
                        app.Disconnect();
                    }
                    catch (Exception)
                    {
                        projectDto.NotDisplay = true;
                        Log.Info("查询有权限的项目：" + "测试库已清除");
                        continue;
                    }
                }
                var bim = new BIMModel()
                {
                    ProjectDto = projs.Where(c => c.NotDisplay != true).ToList()
                };
            //    Log.Info(string.Format("project center：all={0},display={1}", projs.Count(), bim.ProjectDto.Count));
                return View(bim);
            }
            catch (Exception ex)
            {
                return View(new BIMModel
                {
                    ProjectDto = new List<ProjectDto>()
                });
            }
        }

        public ActionResult Show(ModelFile model)
        {
            var dic = new RouteValueDictionary();
            string vGuid = string.Empty;
            if (!model.Guid.StartsWith("{"))
            {
                vGuid += "{";
            }
            vGuid += model.Guid;
            if (!model.Guid.EndsWith("}"))
            {
                vGuid += "}";
            }
            dic.Add("guid", vGuid);
            dic.Add("name", "");
            dic.Add("typeid", model.TypeId);
            dic.Add("objid", model.ObjId);
            if (!string.IsNullOrEmpty(model.IfcGuid)) dic.Add("ifcguid", model.IfcGuid);
            return RedirectToAction("Project", "Model", dic);
            //return View(model);
        }
      
        public ActionResult ClassList()
        {
            var dict = new Dictionary<string, string>()
            {
                {"ClassSecureNoticeDailyCheck", "安全日常检查"},
                {"ClassSecureNoticeWeeklyCheck", "安全周检查"},
                {"ClassSecureNoticeSpecialCheck", "安全专项检查"}
            };

            return Json(dict, JsonRequestBehavior.AllowGet);
        }
        public ActionResult QaClassList(string guid, string username)
        {
            var dict = new Dictionary<string, string>()
            {
                {"ClassIssueFeedback", "问题反馈"}
            };
            bool q = false, s = false;
            string prefix = guid.Replace("{", "").Replace("}", "");
            string key = prefix + "_permission";
            bool ok = true;
            if (Session[key] != null)
            {
                var arr = Session[key].ToString().ToLower().Split(new[] { ',' }).ToList();
                if (arr.Count != 2) ok = false;
                else
                {
                    s = arr[0] == "true";
                    q = arr[1] == "true";
                }
                Log.Info(string.Format("【Session of permission in QaClassList】{0},{1}", key, Session[key])); 
            }
            if (Session[key] == null || !ok)
            {
                var arr = FeedbackData(guid, username).ToLower().Split(new[] { ',' }).ToList();
                if (arr.Count == 2)
                {
                    s = arr[0] == "true";
                    q = arr[1] == "true";
                }
            }
            if (s)
            {
                dict.Add("ClassSecureNoticeDailyCheck", "安全日常检查");
                dict.Add("ClassSecureNoticeWeeklyCheck", "安全周检查");
                dict.Add("ClassSecureNoticeSpecialCheck", "安全专项检查");
            }
            if (q) dict.Add("ClassQualityAdjustmentNotice", "质量整改通知单");
            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        private static int? GetTemplate(Vault vault, int classId)
        {
            var scs = new SearchConditions();

            var classSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            classSC.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            classSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            scs.Add(-1, classSC);

            var delSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            delSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSc);

            var tempSc = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            tempSc.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate;
            tempSc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
            scs.Add(-1, tempSc);

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            foreach (ObjectVersion ov in res)
            {
                return ov.ObjVer.ID;
            }
            return null;
        }

        private static int? GetPartId(Vault vault, int partTypeId, int propIfcId, string ifcGuid)
        {
            var scs = new SearchConditions();
            {
                var sc = new SearchCondition();
                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
                sc.TypedValue.SetValueToLookup(new Lookup { Item = partTypeId });
                scs.Add(-1, sc);
            }
            {
                var sc = new SearchCondition();
                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                sc.Expression.DataPropertyValuePropertyDef = propIfcId;
                sc.TypedValue.SetValue(MFDataType.MFDatatypeText, ifcGuid);
                scs.Add(-1, sc);
            }
            var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
            //SimpleLog("result num=" + ret.Count + ",id=" + id + ",objtype:" + ObjPart);
            foreach (ObjectVersion objectVersion in ret)
            {
                //var ao = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID,
                //    objectVersion.ObjVer.Version, false);
                return objectVersion.ObjVer.ID;
                //return Json("OK-" + ao, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult GetFeedbackData(string guid, string userid)
        {
            string data = "";
            string key = guid.Replace("{", "").Replace("}", "") + "_permission";
            bool ok = true;
            if (Session[key] != null)
            {
                data = Session[key].ToString();
                var arr = data.Split(new[] { ',' }).ToList();
                if (arr.Count != 2) ok = false;
                Log.Info(string.Format("【Session of permission in GetFeedbackData】{0},{1}", key, Session[key]));
            }
            if (Session[key] == null || !ok) data = FeedbackData(guid, userid);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private string FeedbackData(string guid, string userid)
        {//UgSafetyProductionManagementDepartment,UgSecurityGuard,UgSafetySupervisor,UgQualityInspector 
            var ret = "false,false";
            try
            {
                var serv = _mFilesVaultService.GetServer();
                var app = MFServerUtility.ConnectToServer(serv);
                var vault = app.LogInToVault(guid);
                var vaultuserid = -1;
                var users = vault.UserOperations.GetUserAccounts();
                foreach (UserAccount userAccount in users)
                {
                    if (userAccount.LoginName == userid)
                    {
                        vaultuserid = userAccount.ID;
                        break;
                    }
                }

                var UgSafetyProductionManagementDepartment =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        MfilesAliasConfig.UgSafetyProductionManagementDepartment);
                var UgSecurityGuard =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        MfilesAliasConfig.UgSecurityGuard);
                var UgSafetySupervisor =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        MfilesAliasConfig.UgSafetySupervisor);
                var UgQualityInspector =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                        MfilesAliasConfig.UgQualityInspector);
                var safetypermission = IsUserInGroup(vaultuserid, UgSafetyProductionManagementDepartment, vault) ||
                                       IsUserInGroup(vaultuserid, UgSecurityGuard, vault)
                                       || IsUserInGroup(vaultuserid, UgSafetySupervisor, vault);
                var qualitypermission = IsUserInGroup(vaultuserid, UgQualityInspector, vault);
                vault.LogOutSilent();
                app.Disconnect();
                ret = safetypermission + "," + qualitypermission;
                Log.Info(string.Format("GetFeedbackData,{0},{1},{2}", guid, userid, ret)); 
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("GetFeedbackData,{0},{1},{2}", guid, userid, ex.Message));
            }
            //记录到session
            string key = guid.Replace("{", "").Replace("}", "") + "_permission";
            Session[key] = ret;
            return ret;
        }
        private bool IsUserInGroup(int uid, int ugid, Vault vault)
        {
            try
            {
                var ids = vault.UserGroupOperations.GetUserGroup(ugid).Members;
                foreach (int id in ids)
                {
                    if (id < 0)
                    {
                        return IsUserInGroup(uid, -id, vault);
                    }
                    if (id == uid) return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ActionResult WorkFlow(string vaultGuid,string classAlias, string id, string viewPort)
        {
            try
            {
                var serv = _mFilesVaultService.GetServer();
                var app = MFServerUtility.ConnectToServer(serv);

                if (String.IsNullOrEmpty(classAlias))
                {
                    classAlias = "ClassSecureNotice1";
                }
                var guidstring = vaultGuid;
                if (!guidstring.StartsWith("{"))
                {
                    guidstring = "{" + guidstring;
                }
                if (!guidstring.EndsWith("}"))
                {
                    guidstring += "}";
                }
                var vault = app.LogInToVault(guidstring);
                var workflowClassId = vault.ClassOperations.GetObjectClassIDByAlias(classAlias);
                var workflowClass = vault.ClassOperations.GetObjectClass(workflowClassId);
                var workflowObjType = workflowClass.ObjectType;

                int? template = GetTemplate(vault, workflowClassId);

                var objPart =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                        MfilesAliasConfig.ObjPart);
                var propIfcId =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                        MfilesAliasConfig.PropIfcId);

                var objPartType = vault.ObjectTypeOperations.GetObjectType(objPart);
                var objPartsDef = objPartType.DefaultPropertyDef;
                var parts = new List<int>();
                var pp = id.Split(new[] {','});
                foreach (var p in pp)
                {
                    var partId = GetPartId(vault, objPart, propIfcId, p);
                    if (partId != null) parts.Add(partId.Value);
                }
                int pIdViewPort = MfAlias.GetPropDef(vault, "PropViewPortParams"); //模型视口参数

                vault.LogOutSilent();
                app.Disconnect();
                if (parts.Count == 0)
                {
                    return Json(new Dictionary<string, string>
                    {
                        {"status", "500"},
                        {"message", string.Format("在vault-{0}中没有找到ifcguid={1}的构件", vaultGuid, id)}
                    }, JsonRequestBehavior.AllowGet);
                }
                
                
                var propValues = new Dictionary<string, string>();
                propValues.Add(objPartsDef.ToString(), String.Join(",", parts));

                propValues.Add(pIdViewPort.ToString(), viewPort);//test
                var url = _mfvaultService.CreateObjectUrl(vaultGuid, workflowObjType, workflowClassId, propValues,
                    template);


                return Json("OK-" + url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("{0},{1},{2}", vaultGuid, id, ex.Message), ex);

                return Json(new Dictionary<string, string>
            {
                {"status", "500"},
                {"message", string.Format("{0},{1},{2}", vaultGuid, id,ex.Message)}
            }, JsonRequestBehavior.AllowGet);
            }
            //var retjs = new Dictionary<string, string>
            //{
            //    {"status", "500"},
            //    {"message", string.Format("在vault-{0}中没有找到ifcguid={1}的构件", vaultGuid, id)}
            //};
            //return Json(retjs, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ModelPath(ModelFile model)
        {
            var path = ModelUtility.GetModelPath(model);
            if (String.IsNullOrEmpty(path))
            {
                path = "模型不存在或正在转换中...";
            }
            return Json(path, "text/plain", JsonRequestBehavior.AllowGet);
        }

        private bool HasModel(ModelFile model)
        {
            var res = false;
            var path = ModelUtility.GetModelPath(model);
            if (!String.IsNullOrEmpty(path))
            {
                res = true;
            }
            return res;
        }
        //http://stackoverflow.com/questions/4731295/asp-net-http-404-file-not-found-instead-of-maxrequestlength-exception
        //http://www.cnblogs.com/smallerpig/p/3646171.html
        [HttpPost]
        public ActionResult Upload(ModelFile model, HttpPostedFileBase file) //
        {
            if (ModelState.IsValid)
            {

                if (file != null)
                {
                    var contents = new byte[file.ContentLength];
                    file.InputStream.Read(contents, 0, file.ContentLength);
                    var filePath = ModelUtility.GetModel(model, "ifc");
                    System.IO.File.WriteAllBytes(filePath, contents);
                    Task.Run(() => ModelUtility.ConvertModel(filePath));
                    return Json("OK");
                }
                //TempData["message"] = string.Format("{0} has been saved", product.Name);
                return Json("no file");
            }
            else
            {
                // there is something wrong with the data values
                return Json("输入数据有误");
            }
        }
    }
}