using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using AecCloud.BaseCore;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
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
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class NewReportController : BaseController
    { private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserService _userService;
        private readonly IVaultServerService _mFilesVaultService;
        private readonly IProjectService _projService;
        private readonly IProjectMemberService _projectMemberService;
        //private readonly IProjectService _projectService;
        private readonly IMFilesVaultService _mfvaultService;
        //private readonly IMfilesWebService _mfilesWebService;

        public NewReportController(IAuthenticationManager authManager, SignInManager<User, long> signInManager, UserManager<User, long> userManager
            , IVaultServerService mFilesVaultService, IUserService userService, IProjectService projService, IProjectMemberService projectMemberService, IMFilesVaultService mfvaultService)
            : base(authManager, signInManager, userManager)
        {
            _mFilesVaultService = mFilesVaultService;
            _projService = projService;
            _userService = userService;
            _projectMemberService = projectMemberService;
            //_projectService = projectService;
            _mfvaultService = mfvaultService;
            //_mfilesWebService = mfilesWebService;
        }
        public ActionResult SecureReport(string selectSecureClass, string selectLevel, string selectCorporation, string searchString, string currentFilter, int? page)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var aecuser = GetAecuser();
                var secureclasses = InitializeSecureWeb(aecuser);
                var loginName = aecuser.GetAccountName();
                if (!String.IsNullOrEmpty(searchString))
                {
                    allProjects = allProjects.Where(s => s.Name.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if (selectLevel != "项目级别")
                    allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }
                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                    allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
               
                var chartdatalist = new List<ChartData>();
               // var allissuecount = 0;
                foreach (Project aProject in allProjects)
                {
                    if (!ViewBag.IsHeadquarterMember && aProject.Company.Code != aecuser.Company.Code) continue;
                    try
                    {
                      //  Log.Info(string.Format("SecureReport project:{0}", aProject.Name));
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(aProject.Vault, loginName);
                        if (mfresource == null) continue;
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            //var UgSecurityGuard =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup, MfilesAliasConfig.UgSecurityGuard);
                            //var UgSafetyProductionManagementDepartment =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgSafetyProductionManagementDepartment);

                            //bool useringroup = IsUserInGroup((int)mfuserid, UgSecurityGuard, vault);
                            //bool useringroup1 = IsUserInGroup((int)mfuserid, UgSafetyProductionManagementDepartment, vault);
                            //if (!(useringroup || useringroup1))
                            //{
                            //    ReleaseMfilesresource(mfresource);
                            //    continue;
                            //}
                        }
                        else
                        {
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }
                        var vl = vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemValueList, MfilesAliasConfig.VlSecureCategory);
                        var vlis = vault.ValueListItemOperations.GetValueListItems(vl);
                        var PropRectificationConclusion =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropRectificationConclusion);
                        var PropIssueCategory =
                           vault.GetMetadataStructureItemIDByAlias(
                               MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropIssueCategory);
                        var OtSecureAdjustNotice =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                                MfilesAliasConfig.OtSecureAdjustNotice);
                        var classid = -1;
                        foreach (Secureclass secureclass in secureclasses)
                        {
                            if (selectSecureClass == secureclass.Name)
                            {
                                classid =
                                     vault.GetMetadataStructureItemIDByAlias(
                                    MFMetadataStructureItem.MFMetadataStructureItemClass, secureclass.Alias);
                                break;
                            }
                        }

                        foreach (ValueListItem valueListItem in vlis)
                        {
                            var scs = new SearchConditions();
                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = OtSecureAdjustNotice });
                                scs.Add(-1, sc);
                            }
                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataPropertyValuePropertyDef = PropIssueCategory;
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = valueListItem.ID });
                                scs.Add(-1, sc);
                            }
                            if ((!string.IsNullOrEmpty(selectSecureClass)) && selectSecureClass != "检查类别")
                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;

                                sc.TypedValue.SetValueToLookup(new Lookup { Item = classid });
                               // Log.Info(
                               //string.Format(
                               //    "SecureReport project:classid,{0},selectSecureClass:{1}", classid, selectSecureClass));
                                scs.Add(-1, sc);
                            }

                            var num = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                                MFSearchFlags.MFSearchFlagNone, false, 0, 0).Count;
                            //Log.Info(
                            //    string.Format(
                            //        "SecureReport project:{0},ValueListItem:{1},num:{2},PropIssueCategory:{3},OtSecureAdjustNotice={4}",
                            //        aProject.Name, valueListItem.ID, num, PropIssueCategory, OtSecureAdjustNotice));
                        //    allissuecount += num;

                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataPropertyValuePropertyDef = PropRectificationConclusion;
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = 1 });
                                scs.Add(-1, sc);
                            }
                            var srqualified = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                              MFSearchFlags.MFSearchFlagNone, false, 0, 0).Count;

                            var notfound = true;
                            foreach (ChartData chartData in chartdatalist)
                            {
                                if (chartData.Name == valueListItem.Name)
                                {
                                    chartData.Number += num;
                                    chartData.QualifiedNumber += srqualified;
                                    notfound = false;
                                    break;
                                }
                            }
                            if (notfound)
                            {
                                chartdatalist.Add(new ChartData { Name = valueListItem.Name, Number = num, QualifiedNumber = srqualified });
                            }
                        }

                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("SecureReport mfiles processing error: {0}", aProject.Name), ex);
                    }
                }
                //if (allissuecount > 0)
                //{
                //    foreach (ChartData chartData in chartdatalist)
                //    {
                //        chartData.Number = chartData.Number * 100 / allissuecount;
                //    }
                //}
                ViewBag.chartdata = JsonConvert.SerializeObject(chartdatalist, Formatting.None);
             //   Log.Info(string.Format("SecureReport chartdatalist:{0},chartdata:{1}", chartdatalist.Count, ViewBag.chartdata));
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("SecureReport error 222: {0}", ex.Message));
            }
            return View();
        }

        public ActionResult GetContractorListData(string selectContractorProfession, string searchString,  string selectCity, string selectProvince,
           string selectIsQualified)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            var listcContrators = new List<Contrator>();
            Log.Info("开始 M-Files 操作");
            try
            {
                var thevault = _projService.GetContractorVault();
                if (thevault != null)
                {
                    var app = MFServerUtility.ConnectToMfApp(thevault);
                    var vault = app.LogInToVault(thevault.Guid);
                    //var viewid = -1;
                    //try
                    //{
                    //    ViewBag.Url = Utility.GetHost(Request.Url);
                    //    var views = vault.ViewOperations.GetViews();
                    //    foreach (View view in views)
                    //    {
                    //        if (view.Name == "主目录")
                    //        {
                    //            viewid = view.ID;
                    //            break;
                    //        }
                    //    }
                    //    if (viewid < 0)
                    //    {
                    //        foreach (View view in views)
                    //        {
                    //            if (view.Name == "根目录")
                    //            {
                    //                viewid = view.ID;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    ViewBag.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                    //    Log.Info("GetContractorListData ? :" + ViewBag.Url);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Log.Info("GetContractorListData url :" + vault.Name+ex.Message);
                    //}

                    var OtContractor =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                            MfilesAliasConfig.OtContractor);
                    var PropContractorName =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropContractorName);
                    var PropContractedProfession =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropContractedProfession);
                    var PropBusinessLicenseNumber =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropBusinessLicenseNumber);
                    var PropTaxRegistrationNumber =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropTaxRegistrationNumber);
                    var PropQualificationCertificateNumber =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropQualificationCertificateNumber);
                    var PropLevelOfQualification =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropLevelOfQualification);
                    var PropSafetyProductionLicenseNumber =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropSafetyProductionLicenseNumber);
                    var PropRegisteredCapital =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropRegisteredCapital);
                    var PropTelephoneAndFaxOfLegalRepresentative =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropTelephoneAndFaxOfLegalRepresentative);
                    var PropDetailedAddress =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropDetailedAddress);
                    var PropDeputiesAndTelephones =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropDeputiesAndTelephones);
                    var PropLevel =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropLevel);
                    var PropIsQualified =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropIsQualified);
                    var PropComment =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                            MfilesAliasConfig.PropComment);
                    var scs = new SearchConditions();
                    {
                        var sc = new SearchCondition();
                        sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                        sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
                        sc.TypedValue.SetValueToLookup(new Lookup { Item = OtContractor });
                        scs.Add(-1, sc);
                    }
                    {
                        var sc = new SearchCondition();
                        sc.ConditionType = MFConditionType.MFConditionTypeNotEqual;
                        sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
                        sc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
                        scs.Add(-1, sc);
                    }
                    var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                        MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
                    Log.Info("contractorlist vault search:" + ret.Count);
                    var pvss = vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(ret.GetAsObjVers());
                    foreach (PropertyValues pvs in pvss)
                    {
                        var onecontractor = new Contrator
                        {
                            Name = pvs.SearchForProperty(PropContractorName).GetValueAsLocalizedText(),
                            PropBusinessLicenseNumber =
                                pvs.SearchForProperty(PropBusinessLicenseNumber).GetValueAsLocalizedText(),
                            PropComment = pvs.SearchForProperty(PropComment).GetValueAsLocalizedText(),
                            PropContractedProfession =
                                pvs.SearchForProperty(PropContractedProfession).GetValueAsLocalizedText(),
                            PropDeputiesAndTelephones =
                                pvs.SearchForProperty(PropDeputiesAndTelephones).GetValueAsLocalizedText(),
                            PropDetailedAddress = pvs.SearchForProperty(PropDetailedAddress).GetValueAsLocalizedText(),
                            PropIsQualified = pvs.SearchForProperty(PropIsQualified).GetValueAsLocalizedText(),
                            PropLevel = pvs.SearchForProperty(PropLevel).GetValueAsLocalizedText(),
                            PropLevelOfQualification =
                                pvs.SearchForProperty(PropLevelOfQualification).GetValueAsLocalizedText(),
                            PropQualificationCertificateNumber =
                                pvs.SearchForProperty(PropQualificationCertificateNumber).GetValueAsLocalizedText(),
                            PropRegisteredCapital =
                                pvs.SearchForProperty(PropRegisteredCapital).GetValueAsLocalizedText(),
                            PropSafetyProductionLicenseNumber =
                                pvs.SearchForProperty(PropSafetyProductionLicenseNumber).GetValueAsLocalizedText(),
                            PropTaxRegistrationNumber =
                                pvs.SearchForProperty(PropTaxRegistrationNumber).GetValueAsLocalizedText(),
                            PropTelephoneAndFaxOfLegalRepresentative =
                                pvs.SearchForProperty(PropTelephoneAndFaxOfLegalRepresentative)
                                    .GetValueAsLocalizedText(),
                            Url = string.Empty,
                        };
                        listcContrators.Add(onecontractor);
                    }

                  
                    vault.LogOutSilent();
                    app.Disconnect();
                    Log.Info("操作结束");
                }
                else
                {
                    Log.Info("取分包商管理 vault 出错");
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("ListcContrators error: {0}", ex.Message));
            }
           
            var tmplist = FilterContrators(listcContrators, selectContractorProfession, selectIsQualified,
                selectCity, searchString, selectProvince);
           
            var json = JsonConvert.SerializeObject(tmplist,
                             new JsonSerializerSettings
                             {
                                 NullValueHandling = NullValueHandling.Ignore,
                                 Formatting = Newtonsoft.Json.Formatting.None
                             });
            return Content(json);
        }
        public ActionResult ContractorList()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                var thevault = _projService.GetContractorVault();
                if (thevault == null)
                {
                    return View();
                }
               
                var app = MFServerUtility.ConnectToMfApp(thevault);
                    var vault = app.LogInToVault(thevault.Guid);
                    try
                    {
                        ViewBag.Url = Utility.GetHost(Request.Url);
                        var views = vault.ViewOperations.GetViews();
                        var viewid = -1;
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
                        ViewBag.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                        Log.Info("contractor url ? :" + ViewBag.Url);
                    }
                    catch (Exception ex)
                    {
                        Log.Info("GetContractor url :" + vault.Name + ex.Message);
                    }
                var VlContractedProfession =
                       vault.GetMetadataStructureItemIDByAlias(
                           MFMetadataStructureItem.MFMetadataStructureItemValueList,
                           MfilesAliasConfig.VlContractedProfession);
                var VlProvince =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemValueList,
                        MfilesAliasConfig.VlProvince);
                var VlCity =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemValueList,
                        MfilesAliasConfig.VlCity);
                var VLIsQualified =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemValueList,
                        MfilesAliasConfig.VLIsQualified);

                {
                    var vls = vault.ValueListItemOperations.GetValueListItems(VlContractedProfession);
                    var sl = (from ValueListItem valueListItem in vls select valueListItem.Name).ToList();
                    ViewBag.selectContractorProfession = new SelectList(sl);
                }
                {
                    var vls = vault.ValueListItemOperations.GetValueListItems(VlProvince);
                    var sl = (from ValueListItem valueListItem in vls select valueListItem.Name).ToList();
                    ViewBag.selectProvince = new SelectList(sl);
                }
                {
                    var vls = vault.ValueListItemOperations.GetValueListItems(VlCity);
                    var sl = (from ValueListItem valueListItem in vls select valueListItem.Name).ToList();
                    ViewBag.selectCity = new SelectList(sl);
                }
                {
                    var vls = vault.ValueListItemOperations.GetValueListItems(VLIsQualified);
                    var sl = (from ValueListItem valueListItem in vls select valueListItem.Name).ToList();
                    ViewBag.selectIsQualified = new SelectList(sl);
                } 
                vault.LogOutSilent();
                app.Disconnect();
               
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("TimeLimitWarning   error: {0}", ex.Message));
            }


            return View();
        }
        public ActionResult WorkWaitingList()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            return View();
        }
        IEnumerable<ProjectDto> GetProjects4User(long userid)
        {
            var list = new List<ProjectDto>();
            try
            {
                var projs = _projectMemberService.GetProjectsByUser(userid);
                foreach (var m in projs)
                {
                    try
                    {
                        var proj = _projService.GetProjectById(m.ProjectId);
                        var vault = _mfvaultService.GetVaultById(proj.VaultId);
                        var projDto = proj.ToDto(vault, false);
                        list.Add(projDto);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Format("用户项目 ProjectId= {1} 可能已被清除：{0}", ex.Message, m.ProjectId), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取用户项目异常：{0}", ex.Message), ex);
            }
         //   Log.Info(string.Format("GetProjects4User return list count={0} ", list.Count));
            return list;
        }
        public ActionResult GetWorkWaitingListData()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            List<TaskOrNoticeNew> forworklist = new List<TaskOrNoticeNew>();
            try
            {
                var currentuserid = long.Parse(User.Identity.GetUserId());
                var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                var vs = _mFilesVaultService.GetServer();
                var mfapp = MFServerUtility.ConnectToServer(vs);
                var projs = GetProjects4User(currentuserid);
                Log.Info(string.Format("用户 {0} 查询个人待办任务,项目数={1}", loginName, projs.Count()));
                foreach (ProjectDto projectDto in projs)
                {
                    Vault vault;
                    try
                    {
                        vault = mfapp.LogInToVault(projectDto.Vault.Guid);
                    }
                    catch (Exception )
                    {
                        continue;
                    }

                    var mfuserid = MfUserUtils.GetUserAccount(vault, loginName);

                    var pos = vault.Name.LastIndexOf('-');
                    if (pos < 1) pos = vault.Name.Length;
                    var tasktitle = vault.Name.Substring(0, pos) + " ";
                    if (mfuserid != null)
                    {
                     //   forworklist.AddRange(GetGenericAssignment(vault, mfuserid, tasktitle));
                        forworklist.AddRange(GetTaskApprove(vault, mfuserid, tasktitle));
                        forworklist.AddRange(GetTaskWorkflow(vault, mfuserid, tasktitle));
                        Log.Info(string.Format("项目 {0} ，个人待办任务,任务数={1}", vault.Name, forworklist.Count()));
                    }

                    vault.LogOutSilent();
                }
                mfapp.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("WorkWaitingList error: {0}", ex.Message));
            }
            var json = JsonConvert.SerializeObject(forworklist,
                             new JsonSerializerSettings
                             {
                                 NullValueHandling = NullValueHandling.Ignore,
                                 Formatting = Newtonsoft.Json.Formatting.None
                             });
            return Content(json);
        }
        private IEnumerable<TaskOrNoticeNew> GetTaskWorkflow(Vault vault, int? mfuserid, string tasktitle)
        {
            var forworklist = new List<TaskOrNoticeNew>();
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
                           pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                               .GetValueAsLocalizedText();
              //     Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "ddd"));
                   var Content = string.Empty;
                     try{Content=  pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription)
                           .GetValueAsLocalizedText();
                     }
                     catch (Exception) { }
              //     Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "eee"));
                   var Date =
                       pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated)
                           .GetValueAsLocalizedText();
             //      Log.Info(string.Format("mfuserid:{0},工作流普通任务 tasks:{1}", mfuserid, "fff"));
                   var tonn=new  TaskOrNoticeNew
                    {
                        ProjectName = tasktitle + "工作流任务", 
                        Url = link,
                        Name = Name,
                        Assigner = Assigner,
                        Content = Content,
                        Date = Date
                    };
                   forworklist.Add(tonn);
               }
                //forworklist.AddRange(from ObjectVersion objectVersion in ovs
                //    let pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer)
                //    let link = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID, objectVersion.ObjVer.Version, true)
                //    select new TaskOrNoticeNew
                //    {
                //        ProjectName = tasktitle + "工作流任务", 
                //        Url = link,
                //        Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                //        Assigner = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText(),
                //        Content = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription).GetValueAsLocalizedText(),
                //        Date = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).GetValueAsLocalizedText()
                //    });
            }
            catch (Exception ex)
            {
                Log.Info("GetTaskWorkflow error:" + ex.Message);
            }
            return forworklist;
        }

        private IEnumerable<TaskOrNoticeNew> GetTaskApprove(Vault vault, int? mfuserid, string tasktitle)
        {
            var forworklist = new List<TaskOrNoticeNew>();
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

                   var relations= vault.ObjectOperations.GetRelationships(objectVersion.ObjVer,
                        MFRelationshipsMode.MFRelationshipsModeAll);
                    var creator = string.Empty;
                    foreach (ObjectVersion relation in relations)
                    {
                     //   var objpvs = vault.ObjectPropertyOperations.GetProperties(relation.ObjVer);
                   //     var name = objpvs.SearchForProperty(0).GetValueAsLocalizedText();
                      //   creator = objpvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                         creator = vault.ObjectPropertyOperations.GetProperty(relation.ObjVer,(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                        break;
                        //   Log.Info(string.Format("{4},relation: {0},{1},{2},{3},{5}", name, relation.ObjVer.Type, relation.ObjVer.ID, relation.ObjVer.Version, taskname, creator));
                    }
                    var taskOrNoticeOfVault = new TaskOrNoticeNew
                    {
                        ProjectName = tasktitle ,
                        Url = link,
                        Name = taskname,
                        Assigner = creator,
                        Content = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription).GetValueAsLocalizedText(),
                        Date = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).GetValueAsLocalizedText()
                    };
                    forworklist.Add(taskOrNoticeOfVault);
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("GetTaskApprove {0},{1} error:{2}",vault.Name,mfuserid,ex.Message));
            }
            return forworklist;
        }

        private IEnumerable<TaskOrNoticeNew> GetGenericAssignment(Vault vault, int? mfuserid, string tasktitle)
        {
            var forworklist = new List<TaskOrNoticeNew>();
            try
            {
                var scs = new SearchConditions();
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef =
                        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    sc.TypedValue.SetValueToLookup(new Lookup
                    {
                        Item = (int)MFBuiltInObjectClass.MFBuiltInObjectClassGenericAssignment
                    });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef =
                        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = (int)mfuserid });
                    scs.Add(-1, sc);
                }
                var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                    MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
                //Log.Info(string.Format("mfuserid:{0},MFBuiltInObjectClassGenericAssignment tasks:{1}", mfuserid,
                //    ovs.Count));

                forworklist.AddRange(from ObjectVersion objectVersion in ovs
                    let pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer)
                    let link = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID, objectVersion.ObjVer.Version, true)
                    select new TaskOrNoticeNew
                    {
                        ProjectName = tasktitle + "任务", Url = link, Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(), 
                        Assigner = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText(), 
                        Content = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription).GetValueAsLocalizedText(), 
                        Date = pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).GetValueAsLocalizedText()
                    });
            }
            catch (Exception ex)
            {
                Log.Info("GetGenericAssignment error:" + ex.Message);
            }
            return forworklist;
        }
        public ActionResult MessageNotification()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            return View();
        }
        public ActionResult GetMessageNotificationData()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            var forworklist = new List<TaskOrNoticeNew>();
            try
            {
                var currentuserid = long.Parse(User.Identity.GetUserId());
                var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                var vs = _mFilesVaultService.GetServer();
                var mfapp = MFServerUtility.ConnectToServer(vs);
                var projs = GetProjects4User(currentuserid);

                Log.Info(string.Format("用户 {0} 查询个人通知消息,项目数={1}", loginName,projs.Count()));
                foreach (ProjectDto projectDto in projs)
                {
                    Vault vault;
                    try
                    {
                        vault = mfapp.LogInToVault(projectDto.Vault.Guid);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    Log.Info(string.Format("vault:{0}", vault.Name));
                    var mfuserid = MfUserUtils.GetUserAccount(vault, loginName);
                    if (mfuserid != null)
                    {
                        var ClassNotification =
                            vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                                MfilesAliasConfig.ClassNotification);
                        var pos = vault.Name.LastIndexOf('-');
                        if (pos < 1) pos = vault.Name.Length;
                        var tasktitle = vault.Name.Substring(0, pos);

                        var scs = new SearchConditions();
                        {
                            var sc = new SearchCondition();
                            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                            sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                            sc.TypedValue.SetValueToLookup(new Lookup { Item = ClassNotification });
                            scs.Add(-1, sc);
                        }
                        {
                            var sc = new SearchCondition();
                            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                            sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo;
                            var lus = new Lookups();
                            var lu = new Lookup {Item = (int) mfuserid};
                            lus.Add(-1,lu);
                            sc.TypedValue.SetValueToMultiSelectLookup(lus);
                            scs.Add(-1, sc);
                        }
                        var ovs = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                          MFSearchFlags.MFSearchFlagNone, false, 0, 0).ObjectVersions;
                        Log.Info(string.Format("mfuserid:{0},notice:{1}", mfuserid, ovs.Count));

                        foreach (ObjectVersion objectVersion in ovs)
                        {
                            var pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                            var link = vault.ObjectOperations.GetMFilesURLForObject(objectVersion.ObjVer.ObjID,
                                objectVersion.ObjVer.Version, true);
                            var taskOrNoticeOfVault = new TaskOrNoticeNew
                            {
                                ProjectName = tasktitle,
                                Url = link,
                                Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                                Date = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated).GetValueAsLocalizedText(),
                                Content = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription).GetValueAsLocalizedText()
                            };
                            forworklist.Add(taskOrNoticeOfVault);
                        }
                    }
                    vault.LogOutSilent();
                }
                mfapp.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("MessageNotification error: {0}", ex.Message));
            }
            var json = JsonConvert.SerializeObject(forworklist,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                Formatting = Newtonsoft.Json.Formatting.None
                            });
            return Content(json);
        }
        public ActionResult CostWarning()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                ViewBag.selectRegion = new SelectList(_projService.GetAllArea().Select(a => a.Name));
                ViewBag.selectLevel = new SelectList(_projService.GetLevels().Select(c => c.Name));
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Select(c => c.Name));
                ViewBag.selectStatus = new SelectList(_projService.GetAllCostStatus().Select(a => a.Name));
               
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("CostWarning   error: {0}", ex.Message));
            }


            return View();
        }
        public ActionResult GetCostWarningData(string selectRegion, string selectLevel, string selectCorporation, string selectStatus, string searchString)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var currentuserid = long.Parse(User.Identity.GetUserId());
                var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                foreach (var oneProject in allProjects)
                {
                    try
                    {
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(oneProject.Vault, loginName);
                        if (mfresource == null)
                        {
                            oneProject.Ignore = true;
                            continue;
                        }
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            if (PermissionCheck(vault, mfuserid))
                            {
                                oneProject.Ignore = true;
                                ReleaseMfilesresource(mfresource);
                                continue;
                            }
                        }
                        else
                        {
                            oneProject.Ignore = true;
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }
                        try
                        {
                            var viewid = -1;
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
                            oneProject.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                        }
                        catch (Exception ex)
                        {
                            Log.Info("search project :" + oneProject.Name, ex);
                            oneProject.Ignore = true;
                            continue;
                        }
                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("权限判断 process error", ex);
                        oneProject.Ignore = true;
                        continue;
                    }
                    var oneUser = _userService.GetUserById(oneProject.OwnerId);
                    oneProject.OwnerName = oneUser.FullName;
                    oneProject.OwnerContact = oneUser.Phone;
                    oneProject.OwnerContact = string.IsNullOrEmpty(oneProject.OwnerContact)
                        ? string.Empty
                        : oneProject.OwnerContact;
                    if (oneProject.PlanCost > 0 && oneProject.ActualCost > 0)
                    {
                        var dev = (oneProject.ActualCost - oneProject.PlanCost) * 100 / oneProject.PlanCost;
                        oneProject.Deviation = (dev).ToString("#0.00") + "%";
                    }
                }
               
                allProjects = allProjects.Where(a => a.Ignore != true);
                if (!String.IsNullOrEmpty(searchString))
                {
                    allProjects = allProjects.Where(s => s.Name.Contains(searchString));
                }
                if (!string.IsNullOrEmpty(selectRegion))
                {
                    if (selectRegion != "地区")
                    allProjects = allProjects.Where(a => a.Area.Name == selectRegion);
                }
                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if (selectLevel != "项目级别")
                    allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }
                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                    allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
                if (!string.IsNullOrEmpty(selectStatus))
                {
                    if (selectStatus != "状态级别")
                    allProjects = allProjects.Where(a => a.Cost.Name == selectStatus);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("CostWarning error: {0}", ex.Message));
            }
            var tlist = new List<CostWarningModel>();
            foreach (Project project in allProjects.Where(c => c.Ignore != true))
            {
                tlist.Add(new CostWarningModel
                {
                    Id = project.Id.ToString(),
                    Url = project.Url,
                    Name = project.Name,
                    OwnerContact = project.OwnerContact,
                    OwnerName = project.OwnerName,
                    Deviation = project.Deviation,
                    Cost = project.Cost.Name,
                    PlanCost = project.PlanCost.ToString(),
                    ActualCost=project.ActualCost.ToString(),
                    CostId=project.CostId
                });
            }
            var json = JsonConvert.SerializeObject(tlist,
                             new JsonSerializerSettings
                             {
                                 NullValueHandling = NullValueHandling.Include,
                                 Formatting = Newtonsoft.Json.Formatting.None
                             });
            return Content(json);
        }

        public ActionResult GetSecureIssueListData(string selectSecureClass, string selectLevel, string selectCorporation, string searchString)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            var Silist = new List<SecureIssue>();
            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var secureclasses =GetSecureClasses();
                var aecuser = GetAecuser();
                var loginName = aecuser.GetAccountName();
                if (!String.IsNullOrEmpty(searchString))
                {
                    allProjects = allProjects.Where(s => s.Name.Contains(searchString));
                }
                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if (selectLevel != "项目级别")
                    allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }
                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                    allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
              
                var allissuecount = 0;
                foreach (Project aProject in allProjects)
                {
                    if (!ViewBag.IsHeadquarterMember && aProject.Company.Code != aecuser.Company.Code) continue;
                    try
                    {
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(aProject.Vault, loginName);
                        if (mfresource == null) continue;
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            //var UgSecurityGuard =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup, MfilesAliasConfig.UgSecurityGuard);
                            //var UgSafetyProductionManagementDepartment =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgSafetyProductionManagementDepartment);

                            //bool useringroup = IsUserInGroup((int)mfuserid, UgSecurityGuard, vault);
                            //bool useringroup1 = IsUserInGroup((int)mfuserid, UgSafetyProductionManagementDepartment, vault);
                            //if (!(useringroup || useringroup1))
                            //{
                            //    ReleaseMfilesresource(mfresource);
                            //    continue;
                            //}
                        }
                        else
                        {
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }
                        var PropAdjustMan =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropAdjustMan);

                        var PropAdjustMeasure =
                              vault.GetMetadataStructureItemIDByAlias(
                                  MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropAdjustMeasure);
                        var PropSecureAdjustDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropSecureAdjustDate);
                        var vl = vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemValueList, MfilesAliasConfig.VlSecureCategory);
                        var vlis = vault.ValueListItemOperations.GetValueListItems(vl);
                        var PropIssueCategory =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropIssueCategory);
                        var OtSecureAdjustNotice =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                                MfilesAliasConfig.OtSecureAdjustNotice);
                        foreach (ValueListItem valueListItem in vlis)
                        {
                            var scs = new SearchConditions();
                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = OtSecureAdjustNotice });
                                scs.Add(-1, sc);
                            }
                            {
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataPropertyValuePropertyDef = PropIssueCategory;
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = valueListItem.ID });
                                scs.Add(-1, sc);
                            }
                            if ((!string.IsNullOrEmpty(selectSecureClass)) && selectSecureClass != "检查类别")
                            {
                                var classid = -1;
                                foreach (Secureclass secureclass in secureclasses)
                                {
                                    if (selectSecureClass == secureclass.Name)
                                    {
                                        classid =
                                             vault.GetMetadataStructureItemIDByAlias(
                                            MFMetadataStructureItem.MFMetadataStructureItemClass, secureclass.Alias);
                                        break;
                                    }
                                }
                                var sc = new SearchCondition();
                                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                                sc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;

                                sc.TypedValue.SetValueToLookup(new Lookup { Item = classid });
                               // Log.Info(
                               //string.Format(
                               //    "安全问题清单 :classid,{0},selectSecureClass:{1}", classid, selectSecureClass));
                                scs.Add(-1, sc);
                            }

                            var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                                MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                            var num = ret.Count;

                            //Log.Info(
                            //    string.Format(
                            //        "SecureReport project:{0},ValueListItem:{1},num:{2},PropIssueCategory:{3},OtSecureAdjustNotice={4}",
                            //        aProject.Name, valueListItem.Name, num, PropIssueCategory, OtSecureAdjustNotice));
                            allissuecount += num;
                            foreach (ObjectVersion ov in ret.ObjectVersions)
                            {
                                var pvs = vault.ObjectPropertyOperations.GetProperties(ov.ObjVer);
                                var si = new SecureIssue
                                {
                                    ProjectName = vault.Name.Substring(0, vault.Name.LastIndexOf('-') < 0 ? vault.Name.Length : vault.Name.LastIndexOf('-')),
                                    Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                                    Measure = pvs.SearchForProperty(PropAdjustMeasure).GetValueAsLocalizedText(),
                                    Time = pvs.SearchForProperty(PropSecureAdjustDate).GetValueAsLocalizedText(),
                                    Person = pvs.SearchForProperty(PropAdjustMan).GetValueAsLocalizedText(),
                                    Type = pvs.SearchForProperty(PropIssueCategory).GetValueAsLocalizedText(),
                                    url = vault.ObjectOperations.GetMFilesURLForObject(ov.ObjVer.ObjID,
                                ov.ObjVer.Version, true)
                                };
                                Silist.Add(si);
                            }
                           
                        }
                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("SecureReport little error: {0}", ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("SecureIssueList error: {0}", ex.Message));
            }
           
            var json = JsonConvert.SerializeObject(Silist,
                           new JsonSerializerSettings
                           {
                               NullValueHandling = NullValueHandling.Ignore,
                               Formatting = Newtonsoft.Json.Formatting.None
                           });
            return Content(json);
        }
        public ActionResult SecureIssueList()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                var aecuser = GetAecuser();
                InitializeSecureWeb(aecuser);
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("SecureIssueList   error: {0}", ex.Message));
            }


            return View();
        }

        private List<Secureclass> InitializeSecureWeb(User aecuser)
        {
            var secureclasses = GetSecureClasses();

            if (ViewBag.IsHeadquarterMember)
            {
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Where(c => c.Code != "0001A210000000002OSD").Select(c => c.Name));
            }
            else
            {
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Where(c => c.Code == aecuser.Company.Code).Select(c => c.Name));
            }
            ViewBag.selectSecureClass = new SelectList(secureclasses.Select(a => a.Name));
            ViewBag.selectLevel = new SelectList(_projService.GetLevels().Select(c => c.Name));
            return secureclasses;
        }

        private List<Secureclass> GetSecureClasses()
        {
          return  new List<Secureclass>
                {
                    new Secureclass {Name = "安全日常检查", Alias = MfilesAliasConfig.ClassSecureNoticeDailyCheck},
                    new Secureclass {Name = "安全周检查", Alias =MfilesAliasConfig.ClassSecureNoticeWeeklyCheck},
                    new Secureclass {Name = "安全专项检查", Alias = MfilesAliasConfig.ClassSecureNoticeSpecialCheck}
                };
        }

        private User GetAecuser()
        {
            var currentuserid = long.Parse(User.Identity.GetUserId());
            var aecuser=_userService.GetUserById(currentuserid);
            if (aecuser.Company.Code == "0001A210000000002OSD" || aecuser.Company.Code == "0001A210000000002ORS" || aecuser.Company.Code == "0001A210000000002OX8" ||
                aecuser.Company.Code == "0001A210000000002OXB" || aecuser.Company.Code == "0001A210000000002OXE")
            // 0001A210000000002OSD,总部机关, 0001A210000000002ORS,中建八局第二建设有限公司,
                //0001A210000000002OX8，华山指挥部；公司直属项目，0001A210000000002OXB；局直属项目，0001A210000000002OXE
            {
                ViewBag.IsHeadquarterMember = true;
            }
            else
            {
                ViewBag.IsHeadquarterMember = false;
            }
            return aecuser;
        }
        private string GetStatus(Vault vault, Project oneProject)
        {
            var status = "正常";
            try
            {
                var ClassMonthReportAlias = string.Empty;
                if (oneProject.Company.Code == "0001A210000000002ORS") //0001A210000000002ORS,中建八局第二建设有限公司,
                {
                    switch (oneProject.LevelId)
                    {
                        case 1:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationDirectlyControlNotImportant";
                            break;
                        case 2:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationDirectlyControlImportant";
                            break;
                        default:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationDirectlyControlImportant";
                            break;
                    }
                }
                else //二级单位
                {
                    switch (oneProject.LevelId)
                    {
                        case 1:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationSecondLevelNotImportant";
                            break;
                        case 2:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationSecondLevelImportant";
                            break;
                        default:
                            ClassMonthReportAlias = "ClassMonthlyEvaluationSecondLevelImportant";
                            break;
                    }
                }
            //    Log.Info("222 ClassMonthReportAlias-" + ClassMonthReportAlias);
                var ClassMonthReport =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemClass, ClassMonthReportAlias);
                //var PropMonthReportName =
                //    vault.GetMetadataStructureItemIDByAlias(
                //        MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropMonthReportName);
                var PropReportMonth =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropReportDate);
                var PropDelayDays =
                    vault.GetMetadataStructureItemIDByAlias(
                        MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropDelayDays);

                var scs = new SearchConditions();
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                    sc.Expression.DataPropertyValuePropertyDef =
                        (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    sc.TypedValue.SetValueToLookup(new Lookup {Item = ClassMonthReport});
                    scs.Add(-1, sc);
                }
                var strcompare = DateTime.Now.Year + "/" + DateTime.Now.Month.ToString("D2");
                var threemonthago = DateTime.Now.AddMonths(-3);
                var threemonthagoym = threemonthago.Year + "/" + threemonthago.Month.ToString("D2") + "/" + "01";
                var thomonthago = DateTime.Now.AddMonths(-2);
                var thomonthagoym = thomonthago.Year + "/" + thomonthago.Month.ToString("D2");
                var onemonthago = DateTime.Now.AddMonths(-1);
                var onemonthagoym = onemonthago.Year + "/" + onemonthago.Month.ToString("D2");
                  //    Log.Info(string.Format("{0},{1},{2},{3},{4},", threemonthagoym, strcompare, ClassMonthReport,  PropReportMonth, PropDelayDays));
                {
                    var sc = new SearchCondition();
                    sc.ConditionType = MFConditionType.MFConditionTypeGreaterThan;
                    sc.Expression.DataPropertyValuePropertyDef = PropReportMonth;
                    sc.TypedValue.SetValue(MFDataType.MFDatatypeDate, threemonthagoym);
                    scs.Add(-1, sc);
                }
                var result = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                    MFSearchFlags.MFSearchFlagNone, true, 0, 0).ObjectVersions;
                var currentmonthflag = false;
                var onemonthagoflag = false;
                var twomonthagoflag = false;
             //   Log.Info(result.Count);
                foreach (ObjectVersion objectVersion in result)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                   
                    var name = pvs.SearchForProperty(PropReportMonth).GetValueAsLocalizedText();
                    var tmp = name.Split('/');
                    name = string.Format("{0}/{1}", tmp[0], int.Parse(tmp[1]).ToString("D2"));
                    var days = int.Parse(pvs.SearchForProperty(PropDelayDays).GetValueAsLocalizedText());
                   //   Log.Info(string.Format("{0},",name));
                    if (days > 0)
                    {
                        if (name.Contains(strcompare))
                        {
                            currentmonthflag = true;
                        }
                        else if (name.Contains(onemonthagoym))
                        {
                            onemonthagoflag = true;
                        }
                        else if (name.Contains(thomonthagoym))
                        {
                            twomonthagoflag = true;
                        }
                        //   Log.Info(string.Format("{0} 延误", name));
                    }
                }
                if (currentmonthflag)
                {
                    if (onemonthagoflag)
                    {
                        if (twomonthagoflag)
                        {
                            status = "严重延误";
                            oneProject.TimeLimitStatusId = 4;
                        }
                        else
                        {
                            status = "一般延误";
                            oneProject.TimeLimitStatusId = 3;
                        }
                    }
                    else
                    {
                        status = "正常延误";
                        oneProject.TimeLimitStatusId = 2;
                    }
                }
                else
                {
                    status = "正常";
                    oneProject.TimeLimitStatusId = 1;
                }
             //   Log.Info(string.Format("{0},{1},{2},", currentmonthflag, onemonthagoflag, twomonthagoflag));
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("get status error:{0},{1},{2}",vault.Name,oneProject.Name,ex.Message));
            }

         //   Log.Info(string.Format("{0},{1},,", status, oneProject.TimeLimitStatusId));
            return status;
        }

        public ActionResult TimeLimitWarning()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                ViewBag.timeNormal = 0;
                ViewBag.timeNormalDelay = 0;
                ViewBag.timeGeneralDelay = 0;
                ViewBag.timeSeriousDelay = 0;
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Select(c => c.Name));

                ViewBag.selectRegion = new SelectList(_projService.GetAllArea().Select(a => a.Name));
                ViewBag.selectLevel = new SelectList(_projService.GetLevels().Select(c => c.Name));
              
                ViewBag.selectStatus = new SelectList(_projService.GetTimeStatus().Select(c => c.Name));
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("TimeLimitWarning   error: {0}", ex.Message));
            }

            return View();
        }
        public ActionResult GetTimeLimitWarningData(string selectRegion, string selectLevel, string selectCorporation, string selectStatus, string searchString)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var currentuserid = long.Parse(User.Identity.GetUserId());
                var loginName = _userService.GetUserById(currentuserid).GetAccountName();
                foreach (var oneProject in allProjects)
                {
                    var oneUser = _userService.GetUserById(oneProject.OwnerId);
                    oneProject.OwnerName = oneUser.FullName;
                    oneProject.OwnerContact = oneUser.Phone;
                    oneProject.OwnerContact = string.IsNullOrEmpty(oneProject.OwnerContact)
                        ? string.Empty
                        : oneProject.OwnerContact;
                    try
                    {
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(oneProject.Vault, loginName);
                        if (mfresource == null)
                        {
                            oneProject.Ignore = true;
                            continue;
                        }
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            //if (PermissionCheck(vault, (int)mfuserid))
                            //{
                            //    oneProject.Ignore = true;
                            //    ReleaseMfilesresource(mfresource);
                            //    continue;
                            //}
                        }
                        else
                        {
                            oneProject.Ignore = true;
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }

                        oneProject.TimeLimitStatus.Name = GetStatus(vault, oneProject);
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
                            oneProject.Url = vault.ViewOperations.GetMFilesURLForView(viewid);
                        }
                        catch (Exception ex)
                        {
                            Log.Info("search time project :" + oneProject.Name, ex);
                            oneProject.Ignore = true;
                            continue;
                        }
                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception)
                    {
                        oneProject.Ignore = true;
                    //    Log.Error("status process error", new Exception("test vault has been  removed."));
                    }
                }
              
                allProjects = allProjects.Where(a => a.Ignore != true);
                if (!String.IsNullOrEmpty(searchString))
                {
                    allProjects = allProjects.Where(s => s.Name.Contains(searchString));
                }
                if (!string.IsNullOrEmpty(selectRegion))
                {
                    if (selectRegion != "地区")
                    allProjects = allProjects.Where(a => a.Area.Name == selectRegion);
                }
                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if (selectLevel != "项目级别")
                    allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }
                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                    allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
                if (!string.IsNullOrEmpty(selectStatus))
                {
                    if (selectStatus != "状态级别")
                    allProjects = allProjects.Where(a => a.TimeLimitStatus.Name == selectStatus);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("TimeLimitWarning error: {0}", ex.Message));
            }
            var tlist = new List<TimeLimitWarningModel>();
            foreach (Project project in allProjects.Where(c => c.Ignore != true))
            {
              //  Log.Info(string.Format("GetTimeLimitWarningData, {0},{1},{2},{3},{4}", project.Name, project.OwnerContact, project.TimeLimitStatus.Id, project.TimeLimitStatus.Name, project.TimeLimitStatusId));
                tlist.Add(new TimeLimitWarningModel
                {
                    Id=project.Id.ToString(),
                    Url = project.Url,
              Name=project.Name,OwnerContact=project.OwnerContact,OwnerName=project.OwnerName,TimeLimitStatus = project.TimeLimitStatus.Name,
              TimeLimitStatusId = (int)project.TimeLimitStatusId
                });
            }
            var json = JsonConvert.SerializeObject(tlist,
                             new JsonSerializerSettings
                             {
                                 NullValueHandling = NullValueHandling.Ignore,
                                 Formatting = Newtonsoft.Json.Formatting.None
                             });
          //  Log.Info(string.Format("out GetTimeLimitWarningData,"));
            return Content(json);
        }
        private IEnumerable<Contrator> FilterContrators(List<Contrator> listcContrators, string selectRegion, string selectIsQualified,
           string selectCity, string searchString, string selectProvince)
        {
            IEnumerable<Contrator> ret = listcContrators;
            if (!String.IsNullOrEmpty(searchString))
            {
                ret = ret.Where(s => s.Name.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(selectRegion))
            {
                if (selectRegion != "承包专业")
                ret = ret.Where(a => a.PropContractedProfession == selectRegion);
            }
            if (!string.IsNullOrEmpty(selectIsQualified))
            {
                if (selectIsQualified != "合格？")
                ret = ret.Where(a => a.PropIsQualified == selectIsQualified);
            }
            if (!string.IsNullOrEmpty(selectCity))
            {
                if (selectCity != "城市")
                ret = ret.Where(a => a.PropDetailedAddress.Contains(selectCity) || a.Name.Contains(selectCity));
            }
            if (!string.IsNullOrEmpty(selectProvince))
            {
                if(selectProvince!="省份")
                ret = ret.Where(a => a.PropDetailedAddress.Contains(selectProvince) || a.Name.Contains(selectProvince));
            }
            return ret;
        }

        public ActionResult ThreeControlsAll()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                var aecuser = GetAecuser();
                InitializeThreeControls(aecuser);
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("ThreeControlsAll   error: {0}", ex.Message));
            }

            return View();
        }
        public ActionResult GetThreeControlsAllData(string selectCorporation, string selectLevel, string searchString)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            var silist = new List<ThreeControls>();
            IEnumerable<ThreeControls> temp = silist;
            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var aecuser = GetAecuser();
                var loginName = aecuser.GetAccountName();
              
                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if (selectLevel != "项目级别")
                        allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }

                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                        allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
                foreach (Project aProject in allProjects)
                {
                    if (!ViewBag.IsHeadquarterMember && aProject.Company.Code != aecuser.Company.Code) continue;
                    try
                    {
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(aProject.Vault, loginName);
                        if (mfresource == null) continue;
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            //var UgHeadquartersEngineeringManagementDepartment =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgHeadquartersEngineeringManagementDepartment);
                            //var UgProjectSecretary =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgProjectSecretary);


                            //bool isUgHeadquartersEngineeringManagementDepartment = IsUserInGroup((int)mfuserid, UgHeadquartersEngineeringManagementDepartment, vault);
                            //bool isUgProjectSecretary = IsUserInGroup((int)mfuserid, UgProjectSecretary, vault);
                            //if (PermissionCheck(vault, (int)mfuserid) || !(isUgProjectSecretary || isUgHeadquartersEngineeringManagementDepartment))
                            //{
                            //    aProject.Ignore = true;
                            //    Log.Info(string.Format("user {0} have no rights (such as : UgProjectSecretary,UgHeadquartersEngineeringManagementDepartment) to perform threecontrols in vault {1}", loginName, aProject.Vault.Name));
                                
                            //    ReleaseMfilesresource(mfresource);
                            //    continue;
                            //}
                        }
                        else
                        {
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }
                        var PropFundamentalCloseDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropFundamentalCloseDate);

                        var PropPlanFinishDate =
                              vault.GetMetadataStructureItemIDByAlias(
                                  MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropPlanFinishDate);
                        var PropPrincipalPartPlanDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropPrincipalPartPlanDate);

                        var PropFundamentalPlanDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropFundamentalPlanDate);
                        var PropCompany =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropCompany);
                        var ClassThreeControls =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemClass,
                                MfilesAliasConfig.ClassThreeControls);
                        Log.Info(string.Format("{0},{1},{2},{3}，{4}，{5}", PropFundamentalCloseDate, PropPlanFinishDate, PropPrincipalPartPlanDate, PropFundamentalPlanDate, PropCompany, ClassThreeControls));
                        var serial = 1;
                        #region fundamental
                        {
                            var scs = new SearchConditions();
                            {
                                var sc = new SearchCondition
                                {
                                    ConditionType = MFConditionType.MFConditionTypeEqual,
                                    Expression =
                                    {
                                        DataPropertyValuePropertyDef =
                                            (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
                                    }
                                };
                                sc.TypedValue.SetValueToLookup(new Lookup { Item = ClassThreeControls });
                                scs.Add(-1, sc);
                            }

                            var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                                MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                            foreach (ObjectVersion ov in ret.ObjectVersions)
                            {
                                var pvs = vault.ObjectPropertyOperations.GetProperties(ov.ObjVer);

                                var tc = new ThreeControls
                                {
                                    Serial = serial++,
                                    Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                                    Company = aProject.Company.Name,
                                    Manager =
                                        pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                                            .GetValueAsLocalizedText(),
                                    Fundamental =
                                        pvs.SearchForProperty(PropFundamentalPlanDate).GetValueAsLocalizedText(),
                                    PrincipalPart =
                                   pvs.SearchForProperty(PropPrincipalPartPlanDate).GetValueAsLocalizedText(),
                                    Finish =
                                        pvs.SearchForProperty(PropPlanFinishDate).GetValueAsLocalizedText(),
                                };
                                silist.Add(tc);
                            }
                        }
                        #endregion fundamental
                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("ThreeControlsAll error: {0}", ex.Message));
                    }
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    temp = temp.Where(s => s.Name.Contains(searchString));
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("three controls  error: {0}", ex.Message));
            }

            var json = JsonConvert.SerializeObject(temp,
                     new JsonSerializerSettings
                     {
                         NullValueHandling = NullValueHandling.Ignore,
                         Formatting = Newtonsoft.Json.Formatting.None
                     });
            return Content(json);
        }
        private MfilesResource GetMfilesResourceFromOneVaultForCurrentUser(MFilesVault mFilesVault, string loginName)
        {
            MFilesServerApplication app;
            try
            {
                app = MFServerUtility.ConnectToMfApp(mFilesVault);
            }
            catch (Exception ex)
            {
                Log.Info("getMfilesResourceFromOneVaultForCurrentUser,  MFServerUtility.ConnectToMfApp(mFilesVault)." + ex.Message);
                return null;
            }
            Vault vault;
            try { vault = app.LogInToVault(mFilesVault.Guid); }
            catch (Exception ex)
            {
                app.Disconnect();
                if (!ex.Message.Contains("指定的文档库不存在")) //Log.Info(mFilesVault.Name + ",测试库，已清除。");
               // else
                    Log.Info("getMfilesResourceFromOneVaultForCurrentUser,  app.LogInToVault(mFilesVault.Guid)." + ex.Message);
                return null;
            }
            try
            {
                var ret = new MfilesResource { Vault = vault, MFilesServerApplication = app, Muserid = -1 };
                var users = vault.UserOperations.GetUserAccounts();
                foreach (UserAccount userAccount in users)
                {
                    if (userAccount.LoginName == loginName)
                    {
                        ret.Muserid = userAccount.ID;
                        break;
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Log.Info("GetMfilesUseridFromOneVaultForCurrentUser,GetUserAccounts" + ex.Message);
                vault.LogOutSilent();
                app.Disconnect();
                return null;
            }
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
        private bool PermissionCheck(Vault vault, int mfuserid)
        {
            var UgEngineeringManagementDepartment =
                               vault.GetMetadataStructureItemIDByAlias(
                                   MFMetadataStructureItem.MFMetadataStructureItemUserGroup, MfilesAliasConfig.UgEngineeringManagementDepartment);
            var UGroupPM =
                vault.GetMetadataStructureItemIDByAlias(
                    MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UGroupPM);
            var UgViceExecutive =
             vault.GetMetadataStructureItemIDByAlias(
                 MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                 MfilesAliasConfig.UgViceExecutive);
            var UgChiefEngineer =
                vault.GetMetadataStructureItemIDByAlias(
                    MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgChiefEngineer);
            var UGroupVicePM =
                vault.GetMetadataStructureItemIDByAlias(
                    MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UGroupVicePM);
            var UgBigChiefEngineer =
                vault.GetMetadataStructureItemIDByAlias(
                    MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgBigChiefEngineer);

            bool useringroup = IsUserInGroup((int)mfuserid, UgEngineeringManagementDepartment, vault);
            bool useringroup1 = IsUserInGroup((int)mfuserid, UGroupPM, vault);
            bool isUgViceExecutive = IsUserInGroup((int)mfuserid, UgViceExecutive, vault);
            bool isUgChiefEngineer = IsUserInGroup((int)mfuserid, UgChiefEngineer, vault);
            bool isUGroupVicePM = IsUserInGroup((int)mfuserid, UGroupVicePM, vault);
            bool isUgBigChiefEngineer = IsUserInGroup((int)mfuserid, UgBigChiefEngineer, vault);
            return
                !(useringroup || useringroup1 || isUgViceExecutive || isUgChiefEngineer || isUGroupVicePM ||
                  isUgBigChiefEngineer);
        }
        private void ReleaseMfilesresource(MfilesResource mfresource)
        {
            try
            {
                mfresource.Vault.LogOutSilent();
                mfresource.MFilesServerApplication.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Info("ReleaseMfilesresource error" + ex.Message);
            }
        }
       
        private void AddConditionsClassAndDate(SearchConditions scs, int classThreeControls, int planDate)
        {
            try
            {
                {
                    var sc = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeEqual,
                        Expression =
                        {
                            DataPropertyValuePropertyDef =
                                (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
                        }
                    };
                    sc.TypedValue.SetValueToLookup(new Lookup { Item = classThreeControls });
                    scs.Add(-1, sc);
                }
                {
                    var sc = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeLessThanOrEqual,
                        Expression = { DataPropertyValuePropertyDef = planDate }
                    };
                    sc.TypedValue.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.AddMonths(1).Date);
                    scs.Add(-1, sc);
                }
            }
            catch (Exception ex)
            {
                Log.Info("AddConditionsClassAndDate:" + ex.Message);
            }
        }
        private int CopeWithFundamentalWarning(Vault vault, SearchConditions scs, int propFundamentalPlanDate, List<Models.ThreeControls> silist, string propCompany, int serial, int CloseDate, int PropFundamentalAcceptanceCertificateAttachment)
        {
            try
            {
                var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                                   MFSearchFlags.MFSearchFlagNone, false, 0, 0);
             //   Log.Info("CopeWithFundamentalWarning search=" + ret.ObjectVersions.Count);
                foreach (ObjectVersion ov in ret.ObjectVersions)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(ov.ObjVer);
                    var pvclosedate = pvs.SearchForProperty(CloseDate).Value;
                    bool nulldate = IsNullProperty(pvclosedate);
                    
                    var pvattachment = pvs.SearchForProperty(PropFundamentalAcceptanceCertificateAttachment).Value;
                    bool nullattachment = IsNullProperty(pvattachment);
                    if (!nullattachment && !nulldate) continue;
                    var tc = new ThreeControls
                    {
                        Serial = serial++,
                        Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                        Company = propCompany,
                        Manager =
                            pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                                .GetValueAsLocalizedText(),
                        Fundamental =
                            pvs.SearchForProperty(propFundamentalPlanDate).GetValueAsLocalizedText(),
                            FundamentalDoubleWarning = nullattachment&&nulldate
                    };
                    silist.Add(tc);

                }
              //  Log.Info("CopeWithFundamentalWarning silist=" + silist.Count);
            }
            catch (Exception ex)
            {
                Log.Info("CopeWithFundamentalWarning:" + ex.Message);
            }
            return serial;
        }

        private bool IsNullProperty(TypedValue value)
        {
           return value.IsNULL()||value.IsEmpty()||value.IsUninitialized();
        }
      
        private int CopeWithPrincipalPartWarning(Vault vault, SearchConditions scs, int PropPrincipalPartPlanDate, List<Models.ThreeControls> silist, string PropCompany, int serial, int CloseDate, int PropFundamentalAcceptanceCertificateAttachment)
        {
            try
            {
                var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                    MFSearchFlags.MFSearchFlagNone, false, 0, 0);

                foreach (ObjectVersion ov in ret.ObjectVersions)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(ov.ObjVer);
                    var pvclosedate = pvs.SearchForProperty(CloseDate).Value;
                    bool nulldate = IsNullProperty(pvclosedate);
                    var pvattachment = pvs.SearchForProperty(PropFundamentalAcceptanceCertificateAttachment).Value;
                    bool nullattachment = IsNullProperty(pvattachment);
                    if (!nullattachment && !nulldate) continue;
                    var tc = new ThreeControls
                    {
                        Serial = serial++,
                        Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                        Company = PropCompany,
                        Manager =
                            pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                                .GetValueAsLocalizedText(),
                        PrincipalPart =
                            pvs.SearchForProperty(PropPrincipalPartPlanDate).GetValueAsLocalizedText(),
                        PrincipalPartDoubleWarning = nullattachment && nulldate
                    };
                    silist.Add(tc);
                }
            }
            catch (Exception ex)
            {
                Log.Info("CopeWithPrincipalPartWarning:" + ex.Message);
            }
            return serial;
        }
        private int CopeWithFinishDateWarning(Vault vault, SearchConditions scs, int PropPlanFinishDate, List<ThreeControls> silist, string PropCompany, int serial, int CloseDate, int PropFundamentalAcceptanceCertificateAttachment)
        {
            try
            {
                var ret = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
                                    MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                foreach (ObjectVersion ov in ret.ObjectVersions)
                {
                    var pvs = vault.ObjectPropertyOperations.GetProperties(ov.ObjVer);
                    var pvclosedate = pvs.SearchForProperty(CloseDate).Value;
                    bool nulldate = IsNullProperty(pvclosedate);

                    var pvattachment = pvs.SearchForProperty(PropFundamentalAcceptanceCertificateAttachment).Value;
                    bool nullattachment = IsNullProperty(pvattachment);
                    if (!nullattachment && !nulldate) continue;
                    var tc = new ThreeControls
                    {
                        Serial = serial++,
                        Name = pvs.SearchForProperty(0).GetValueAsLocalizedText(),
                        Company = PropCompany,
                        Manager =
                            pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy)
                                .GetValueAsLocalizedText(),
                        Finish = pvs.SearchForProperty(PropPlanFinishDate).GetValueAsLocalizedText(),
                        FinishDoubleWarning = nullattachment && nulldate
                    };
                    silist.Add(tc);
                }
            }
            catch (Exception ex)
            {
                Log.Info("CopeWithFinishDateWarning:" + ex.Message);
            }
            return serial;
        }

        public ActionResult GetThreeControlsData(string selectCorporation, string selectLevel, string searchString)
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            var silist = new List<ThreeControls>();
            IEnumerable<ThreeControls> temp = silist;
            IEnumerable<Project> allProjects = _projService.GetAllProjects();
            try
            {
                var aecuser = GetAecuser();
                var loginName = aecuser.GetAccountName();
                if (!string.IsNullOrEmpty(selectLevel))
                {
                    if(selectLevel!="项目级别")
                    allProjects = allProjects.Where(a => a.Level.Name == selectLevel);
                }
               
                if (!string.IsNullOrEmpty(selectCorporation))
                {
                    if (selectCorporation != "公司")
                    allProjects = allProjects.Where(a => a.Company.Name == selectCorporation);
                }
               // Log.Info("GetThreeControlsData project count:" + allProjects.Count());
                foreach (Project aProject in allProjects)
                {
                    if (!ViewBag.IsHeadquarterMember && aProject.Company.Code != aecuser.Company.Code) continue;
                    try
                    {
                        var mfresource = GetMfilesResourceFromOneVaultForCurrentUser(aProject.Vault, loginName);
                        if (mfresource == null) continue;
                        var mfuserid = mfresource.Muserid;
                        var vault = mfresource.Vault;
                        if (mfuserid > 0)
                        {
                            //Log.Info("1111111111");
                            //var UgHeadquartersEngineeringManagementDepartment =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgHeadquartersEngineeringManagementDepartment);
                            //Log.Info("222");
                            //var UgProjectSecretary =
                            //    vault.GetMetadataStructureItemIDByAlias(
                            //        MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                            //        MfilesAliasConfig.UgProjectSecretary);
                            //Log.Info("333");

                            //bool isUgHeadquartersEngineeringManagementDepartment = IsUserInGroup(mfuserid, UgHeadquartersEngineeringManagementDepartment, vault);
                            //Log.Info("444");
                            //bool isUgProjectSecretary = IsUserInGroup(mfuserid, UgProjectSecretary, vault);
                            //Log.Info("555");
                            //if (PermissionCheck(vault, mfuserid) || !(isUgProjectSecretary || isUgHeadquartersEngineeringManagementDepartment))
                            //{
                            //    aProject.Ignore = true;
                            //    Log.Info("666");
                            //    ReleaseMfilesresource(mfresource);
                            //    Log.Info(string.Format("user {0} have no rights (such as : UgProjectSecretary,UgHeadquartersEngineeringManagementDepartment) to perform threecontrols in vault {1}", loginName, aProject.Vault.Name));
                            //    continue;
                            //}
                        }
                        else
                        {
                            ReleaseMfilesresource(mfresource);
                            continue;
                        }
                        var PropFundamentalCloseDate = GetPropertyIdByAlias(vault,MfilesAliasConfig.PropFundamentalCloseDate);
                           
                        var PropPlanFinishDate =
                              GetPropertyIdByAlias(vault, MfilesAliasConfig.PropPlanFinishDate);
                        var PropPrincipalPartPlanDate =
                            GetPropertyIdByAlias(vault, MfilesAliasConfig.PropPrincipalPartPlanDate);

                        var PropFundamentalPlanDate =
                            GetPropertyIdByAlias(vault, MfilesAliasConfig.PropFundamentalPlanDate);
                        var PropCompany =
                            GetPropertyIdByAlias(vault, MfilesAliasConfig.PropCompany);
                        var ClassThreeControls =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemClass,
                                MfilesAliasConfig.ClassThreeControls);
                        //Log.Info(string.Format("important alias,{0},{1},{2},{3}，{4}，{5}", 
                        //    PropFundamentalCloseDate, PropPlanFinishDate, PropPrincipalPartPlanDate, PropFundamentalPlanDate, PropCompany, ClassThreeControls));
                        var serial = 1;
                        #region fundamental
                        {
                            var PropFundamentalAcceptanceCertificateAttachment = GetPropertyIdByAlias(vault, MfilesAliasConfig.PropFundamentalAcceptanceCertificateAttachment);
                            
                                var scs = new SearchConditions();
                                AddConditionsClassAndDate(scs, ClassThreeControls, PropFundamentalPlanDate);
                                serial = CopeWithFundamentalWarning(vault, scs, PropFundamentalPlanDate, silist, aProject.Company.Name, serial, PropFundamentalCloseDate, PropFundamentalAcceptanceCertificateAttachment);
                           
                        }
                        #endregion fundamental

                        #region principalpart
                        {
                            var PropPrincipalPartCloseDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropPrincipalPartCloseDate);
                             var PropPincipalPartAcceptanceCertificateAttachment = GetPropertyIdByAlias(vault, MfilesAliasConfig.PropPincipalPartAcceptanceCertificateAttachment);
                                var scs = new SearchConditions();
                                AddConditionsClassAndDate(scs, ClassThreeControls, PropPrincipalPartPlanDate);

                                serial = CopeWithPrincipalPartWarning(vault, scs, PropPrincipalPartPlanDate, silist, aProject.Company.Name, serial,PropPrincipalPartCloseDate,PropPincipalPartAcceptanceCertificateAttachment);
                           
                        }
                        #endregion principalpart

                        #region finish
                        {
                            var PropFinishDate =
                            vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, MfilesAliasConfig.PropFinishDate);
                                 var PropFinishedAcceptanceCertificateAttachment = GetPropertyIdByAlias(vault, MfilesAliasConfig.PropFinishedAcceptanceCertificateAttachment);
                                var scs = new SearchConditions();
                                AddConditionsClassAndDate(scs, ClassThreeControls, PropPlanFinishDate);
                                serial = CopeWithFinishDateWarning(vault, scs, PropPlanFinishDate, silist, aProject.Company.Name, serial, PropFinishDate, PropFinishedAcceptanceCertificateAttachment);
                           
                        }
                        #endregion finish
                        ReleaseMfilesresource(mfresource);
                    }
                    catch (Exception exception)
                    {
                        Log.Info(string.Format("three controls little error: {0}", exception.Message));
                    }
                }
              
                if (!String.IsNullOrEmpty(searchString))
                {
                    temp = temp.Where(s => s.Name.Contains(searchString));
                }
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("three controls  error: {0}", ex.Message));
            }


            var json = JsonConvert.SerializeObject(temp,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None
            });
            return Content(json);
        }

        private int GetPropertyIdByAlias(Vault vault,string p)
        {
           var id= vault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, p);
           if (id < 0) Log.Info(string.Format("GetPropertyIdByAlias error: {0}",p));
            return id;

        }

        public ActionResult ThreeControls()
        {
            var passOk = IsPasswordAvailable();
            if (!passOk)
            {
                return ReloginForCurrentUser();
            }
            try
            {
                var aecuser = GetAecuser();
                InitializeThreeControls(aecuser);
            }
            catch (Exception ex)
            {
                Log.Info("ThreeControls"+ex.Message);
            }

            return View();
        }
        private void InitializeThreeControls(User aecuser)
        {
            if (ViewBag.IsHeadquarterMember)
            {
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Where(c => c.Code != "0001A210000000002OSD").Select(c => c.Name));
            }
            else
            {
                ViewBag.selectCorporation = new SelectList(_projService.GetAllCompany().Where(c => c.Code == aecuser.Company.Code).Select(c => c.Name));
            }
            ViewBag.selectLevel = new SelectList(_projService.GetLevels().Select(c => c.Name));
        }
    }
}
