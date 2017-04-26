using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AecCloud.MFilesCore;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;
using Newtonsoft.Json;
using SimulaDesign.MfBimInfo;

namespace VaultApp
{
    public class VaultApplication : VaultApplicationBase
    {
        [StatePreConditions("WfsUpdateAndReturnLastState")]
        private bool WfsUpdateAndReturnLastState(StateEnvironment env, out string message)
        {
            var vault = env.Vault;
            var objver = env.ObjVer;
            var laststate = GetStateId(vault, objver, 1);
            env.InitialNextStateID = laststate;
            Writelog(string.Format("----laststate= {0}, StateID={1},InitialNextStateID={2}", laststate, env.StateID, env.InitialNextStateID));
            try
            {
                var MFBuiltInPropertyDefStateTransition =
                    vault.ObjectOperations.GetObjectVersionAndProperties(objver)
                        .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefStateTransition);//2015.3才有的属性，不过有也没用
                Writelog(string.Format("MFBuiltInPropertyDefStateTransition {0},{1}",
                    MFBuiltInPropertyDefStateTransition.GetValueAsLocalizedText(),
                    MFBuiltInPropertyDefStateTransition.Value.GetLookupID()));
            }
            catch (Exception ex)
            {
                Writelog("WfsUpdateAndReturnLastState :" + ex.Message);
            }
            message = "if everything is ok, this shouldn't display!";
            return true;
        }

        [EventHandler(MFEventHandlerType.MFEventHandlerBeforeCreateNewObjectFinalize)]
        public void CopeWithIosTemplateBeforeCreateNewObjectFinalize(EventHandlerEnvironment e)
        {
            var vault = e.Vault;
            var objver = e.ObjVer;
            if (objver.Type != (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument) return;
            if (vault.ObjectOperations.IsSingleFileObject(objver)) return;
            var oi = vault.ObjectOperations.GetObjectInfo(objver, true);
            if (oi.FilesCount > 0) return;
          

            var classid = oi.Class;
            Writelog(string.Format("debug info:class{0},vault{1},type{2},version{3},id{4}",classid,vault.Name,objver.Type,objver.Version,objver.ID));
            // vault.ObjectPropertyOperations.GetProperty(objver,(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass);
            var scs = new SearchConditions();
            {
                var sc = new SearchCondition();
                  sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            sc.TypedValue.SetValueToLookup(new Lookup {Item = classid});
                scs.Add(-1,sc);
            }
           {
                var sc = new SearchCondition();
                  sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate;
               sc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
                scs.Add(-1,sc);
            }


            var sr = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs,
                MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, false);
            var tempfile = Path.GetTempFileName();
            Writelog(string.Format("debug info--:search result count={0},vault{1},type{2},version{3},id{4}", sr.Count, vault.Name, objver.Type, objver.Version, objver.ID));
            foreach (ObjectVersion ov in sr.ObjectVersions)
            {
                vault.ObjectFileOperations.DownloadFile(ov.Files[1].ID,ov.Files[1].Version,tempfile);
                break;
            }
            vault.ObjectFileOperations.AddFile(objver, "title", "docx", tempfile);
            vault.ObjectOperations.SetSingleFileObject(objver, true);
        }
        private int GetStateId(Vault vault, ObjVer objver, int p)
        {
            var lastobjver = new ObjVer();
            lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - p);
            var last =
                vault.ObjectOperations.GetObjectVersionAndProperties(lastobjver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            return last.Value.GetLookupID();
        }
        private bool CheckToWhereFrom(StateEnvironment env, out string message)
        {
            var vault = env.Vault;
            var objver = env.ObjVer;
            var lastobjver = new ObjVer();
            lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - 2);
            var last =
                vault.ObjectOperations.GetObjectVersionAndProperties(lastobjver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var laststate = last.Value.GetLookupID();
            var next =
                vault.ObjectOperations.GetObjectVersionAndProperties(objver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var nextstate = next.Value.GetLookupID();
            //Writelog(string.Format("---- {0}, {1},{2}", laststate, env.StateID, nextstate));
            //env.InitialNextStateID = laststate;
            message = "会签不通过返回修改后，再发起会签流程，需跳过会签通过的部门直接提交会签不通过的部门->" + last.GetValueAsLocalizedText();
            return nextstate == laststate;
        }
        private static string GetHost()
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var hostSettings = config.AppSettings.Settings["host"];
            if (hostSettings == null) return String.Empty;
            return hostSettings.Value;
        }
        static VaultApplication()
        {
            Trace.AutoFlush = true;
        }

        private TraceSource GetTrace(string name)
        {
            var trace = new TraceSource(name, SourceLevels.Information);
            trace.Listeners.Add(
                new CustomTextListener(Path.Combine(Path.GetTempPath(),
                    "vaultapp" + DateTime.Now.ToString("yyyy-MM-dd") + ".log")));
            return trace;
        }

        private Func<object, string> ToJson = o => JsonConvert.SerializeObject(o, Formatting.None);


        [VaultExtensionMethod("BimInfo", RequiredVaultAccess= MFVaultAccess.MFVaultAccessNone)]
        public string CreateOrUpdateBimInfo(EventHandlerEnvironment env)
        {
            var ts = GetTrace(MethodBase.GetCurrentMethod().Name);

            var vault = env.Vault;

            try
            {
                var model = ModelUtility.FromZippedContent(env.Input, JsonConvert.DeserializeObject<MfProjectModel>);//JsonConvert.DeserializeObject<MfProjectModel>(info);
                ts.TraceInformation("输入信息："+model.Model.Name);
                ts.TraceInformation("楼层个数： " + model.Model.Levels.Count);
                ts.TraceInformation("材料个数： " + model.Model.Materials.Count);
                ts.TraceInformation("视图个数： " + model.Model.Views.Count);
                ts.TraceInformation("类别个数： " + model.Model.Categories.Count);
                ts.TraceInformation("类型个数： " + model.Model.Types.Count);
                ts.TraceInformation(" 族个数： " + model.Model.Families.Count);
                ts.TraceInformation("构件个数： " + model.Model.Elements.Count);
                try
                {
                    var aliases = VaultAliases.GetAliases(vault);
                    var dict = model.Run(aliases);
                    return ModelUtility.GetZippedContent(dict, ToJson);
                }
                catch (Exception ex)
                {
                    ts.TraceEvent(TraceEventType.Error, 0, "运行失败：" + ex.Message + "r\n" + ex.StackTrace);
                    return ModelUtility.GetZippedContent("运行失败：" + ex.Message, s=>s.ToString());
                }
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, "获取信息失败：" + ex.Message + "r\n" + ex.StackTrace);
                return ModelUtility.GetZippedContent("获取信息失败：" + ex.Message, s=>s.ToString());
            }
            finally
            {
                ts.Close();
            }            
        }
               /// <summary>
        /// 监理例会纪要审核单签章
        /// </summary>
        /// <param name="env"></param>
        [StateAction("WFSSupervisorMeetingFinished")]
        private void SupervisorMeetingSign(StateEnvironment env)
        {
            var vault = env.Vault;
            var objVer = env.ObjVer;
            var companyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
            var company = vault.ObjectPropertyOperations.GetProperty(objVer, companyPropId).GetValueAsLocalizedText();
            var docOps = new DocumentOperation(env);
            try
            {
                if (company == "中建八局第二建设有限公司") //公司总部签章
                {
                    //var content = docOps.table.Cell(19, 1).Range.Text;
                    //content = GetContent(content, "公司分管生产副总经理审批：", "日期：");
                    //if (content == "")
                    //{
                    //    throw new Exception("审批意见不能为空！");
                    //}
                    docOps.SignPicture(docOps.table.Cell(8, 1), MfilesAliasConfig.UGroupPM, 360, 7, "项目经理：");
                    docOps.SignPicture(docOps.table.Cell(9, 1), MfilesAliasConfig.UGroupPM, 360, 7, "项目经理：");
                    docOps.SignPicture(docOps.table.Cell(10, 1), MfilesAliasConfig.UgLegalServiceManager, 360, 7, "合约法务部：");
                    docOps.SignPicture(docOps.table.Cell(11, 1), MfilesAliasConfig.UgBusinessManager, 360, 7, "商务管理部：");
                    docOps.SignPicture(docOps.table.Cell(12, 1), MfilesAliasConfig.UgHeadquartersEngineeringManager, 360, 7, "工程管理部：");
                    docOps.SignPicture(docOps.table.Cell(13, 1), MfilesAliasConfig.UgSafetyProductionManagementDeptManager, 360, 7, "安全生产管理部：");
                    docOps.SignPicture(docOps.table.Cell(14, 1), MfilesAliasConfig.UgMaterialManager, 360, 7, "物资部：");
                    docOps.SignPicture(docOps.table.Cell(15, 1), MfilesAliasConfig.UgHeadquartersScienceManager, 360, 7, "科技部：");
                    docOps.SignPicture(docOps.table.Cell(16, 1), MfilesAliasConfig.UgHeadquartersTechCenterManager, 360, 7, "技术中心：");
                    docOps.SignPicture(docOps.table.Cell(17, 1), MfilesAliasConfig.UgFinanceManager, 360, 7, "财务部：");
                    docOps.SignPicture(docOps.table.Cell(18, 1), MfilesAliasConfig.UgHeadquartersFundsManager, 360, 7, "投资与资金部：");
                    docOps.SignPicture(docOps.table.Cell(19, 1), MfilesAliasConfig.UgViceExecutive, 360, 7, "签字：");
                }
                else
                {
                    //var content = docOps.table.Cell(14, 1).Range.Text;
                    //content = GetContent(content, "二级单位分管副经理（生产）审批：", "日期：");
                    //if (content == "")
                    //{
                    //    throw new Exception("审批意见不能为空！");
                    //}
                    docOps.SignPicture(docOps.table.Cell(8, 1), MfilesAliasConfig.UGroupPM, 320, 35, "项目经理：");
                    docOps.SignPicture(docOps.table.Cell(9, 1), MfilesAliasConfig.UGroupPM, 320, 35, "项目经理：");
                    docOps.SignPicture(docOps.table.Cell(10, 1), MfilesAliasConfig.UgSecondLevelBusinessManager, 320, 35, "商务部：");
                    docOps.SignPicture(docOps.table.Cell(11, 1), MfilesAliasConfig.UgEngineeringManagerSecond, 320, 35, "施工管理部：");
                    docOps.SignPicture(docOps.table.Cell(12, 1), MfilesAliasConfig.UgMaterialManagerSecondLevel, 320, 35, "物资部：");
                    docOps.SignPicture(docOps.table.Cell(13, 1), MfilesAliasConfig.UgSecondLevelFinanceManager, 320, 35, "财务部：");
                    docOps.SignPicture(docOps.table.Cell(14, 1), MfilesAliasConfig.UgSecondLevelDeputyManager, 320, 35, "签字：");
                }
            }
            catch(Exception ex){
                throw new Exception(ex.Message);
            }
            finally{
                docOps.CloseWord();
                docOps.UpDateFile();
            }
        }
        /// <summary>
        /// 项目经理创建《监理例会纪要》审核记录表 进入创建
        /// </summary>
        /// <param name="env"></param>
        [StateAction("WFSSupervisorMeetingDraft")]
        private void SupervisorMeetingDraft(StateEnvironment env)
        {
            var meetingNumPropId = env.Vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRegularMeetingNum");
            var meetingTimePropId = env.Vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRegularMeetingTime");
            var meetingDatePropId = env.Vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRegularMeetingDate");
            //查找项目经理
            var ugId = env.Vault.UserGroupOperations.GetUserGroupIDByAlias("UGroupPM");
            var members = env.Vault.UserGroupOperations.GetUserGroup(ugId).Members;
            var pm = "";
            var userList = env.Vault.UserOperations.GetUserList();
            for (var i = 1; i <= members.Count; i++)
            {
                var userId = env.Vault.UserOperations.GetUserAccount(members[i]).ID;
                foreach (KeyNamePair kp in userList)
                {
                    if (kp.Key == userId)
                    {
                        pm += kp.Name + " ";
                    }
                }
            }
            var docOps = new DocumentOperation(env);
            try
            {
                //项目名称
                docOps.table.Cell(5, 2).Range.Text = docOps.Project.PropProjName;
                //建设单位
                docOps.table.Cell(5, 4).Range.Text = docOps.Project.PropProprietorUnit;
                //项目经理
                docOps.table.Cell(6, 2).Range.Text = pm;
                //例会编号
                docOps.table.Cell(7, 2).Range.Text = docOps.vault.ObjectPropertyOperations.GetProperty(docOps.objver, meetingNumPropId).GetValueAsLocalizedText();
                //例会时间
                docOps.table.Cell(7, 4).Range.Text = docOps.vault.ObjectPropertyOperations.GetProperty(docOps.objver, meetingDatePropId).GetValueAsLocalizedText() + " " +
                                               docOps.vault.ObjectPropertyOperations.GetProperty(docOps.objver, meetingTimePropId).GetValueAsLocalizedText();
            }
            catch { 
            
            }
            finally
            {
                docOps.CloseWord();
                docOps.UpDateFile();

            } 
        }
        /// <summary>
        /// 项目经理创建《监理例会纪要》审核记录表后 提交审批
        /// </summary>
        /// <param name="env"></param>
        [StateAction("WFSSupervisorMeetingSubmit")]
        private void SupervisorMeetingSubmit(StateEnvironment env)
        {
            var docOps = new DocumentOperation(env);
            try
            {
                //检查信息是否完整
                var content1 = docOps.table.Cell(8, 1).Range.Text;
                content1 = GetContent(content1, "对上周提出的问题整改情况说明：", "项目经理：");
                var content2 = docOps.table.Cell(9, 1).Range.Text;
                content2 = GetContent(content2, "本周监理例会主要问题：", "项目经理：");
                if (content1 == "" || content2 == "")
                {
                    throw new Exception("请项目经理对上周提出的问题整改情况说明和本周监理例会主要问题进行描述！");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally{
               docOps.CloseWord();
               docOps.UpDateFile();
            }
        }
        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetContent(string str, string start, string end)
        {
            var res = "";
            try
            {
                var sIndex = str.IndexOf(start);
                var eIndex = str.LastIndexOf(end);
                res = str.Substring(sIndex, eIndex).Replace(start, "").Trim();
            }
            catch (Exception ex)
            {
               // Writelog("获取监理会议纪要项目经理填写内容异常：" + ex.Message);
            }


            return res;
        }
        /// <summary>
        /// 监理例会纪要自动填充所属公司
        /// </summary>
        /// <param name="env"></param>
        [EventHandler(MFEventHandlerType.MFEventHandlerAfterCreateNewObjectFinalize)]
        private void FillCompanyInSupervisorMeeting(EventHandlerEnvironment env)
        {
            try
            {
                var vault = env.Vault;
                var objVer = env.ObjVer;
                var objVerEx = env.ObjVerEx;
                var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassSupervisorMeeting");
                if (objVerEx.Class == classId)
                {
                    var projTypeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
                    var projClassId = vault.ClassOperations.GetObjectClassIDByAlias("ClassProject");
                    var scs = AddBaseConditions(projTypeId, projClassId, false);
                    var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                    if (res.Count > 0)
                    {
                        var proj = res[1];
                        var projObjVer = proj.ObjVer;
                        var companyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
                        var companyId = vault.ObjectPropertyOperations.GetProperty(projObjVer, companyPropId).Value.GetValueAsLookup().Item;
                        var pv = new PropertyValue();
                        pv.PropertyDef = companyPropId;
                        pv.Value.SetValue(MFDataType.MFDatatypeLookup, companyId);
                        vault.ObjectPropertyOperations.SetProperty(objVer, pv);
                    }
                }
            }
            catch(Exception ex) {
                Writelog("FillCompanyInSupervisorMeeting"+ex.Message);
            }
        }

        ///// <summary>
        ///// 创建履约率对象时 根据项目的所属单位赋予不同的命名访问控制列表
        ///// </summary>
        ///// <param name="env"></param>
        //[EventHandler(MFEventHandlerType.MFEventHandlerAfterCreateNewObjectFinalize)]
        //private void SetNacl2PerformRateObj(EventHandlerEnvironment env)
        //{
        //    var vault = env.Vault;
        //    var objVer = env.ObjVer;
        //    var performObjType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjPerformRate");
        //    if (objVer.Type == performObjType) 
        //    {
        //        var projOwner = GetCompanyName(vault);
        //        if (projOwner == "中建八局第二建设有限公司") //公司总部
        //        {
        //            var nacl = vault.NamedACLOperations.GetNamedACLIDByAlias("NaclPerformRateHead");
        //            vault.ObjectOperations.ChangePermissionsToNamedACL(objVer, nacl, true);
        //        }
        //        else//分公司 
        //        {
        //            var nacl = vault.NamedACLOperations.GetNamedACLIDByAlias("NaclPerformRateAll");
        //            vault.ObjectOperations.ChangePermissionsToNamedACL(objVer, nacl, true);
        //        }
        //    }
        //}
        /// <summary>
        /// 工期节点设置实际工期和实际添加附件日期
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
        [EventHandler(MFEventHandlerType.MFEventHandlerBeforeCheckInChanges)]
        private void SetScheduledNodeRealPeriodAndAddAttachDate(EventHandlerEnvironment env)
        {
            var vault = env.Vault;
            var objVer = env.ObjVer;
            var scheduledNodeType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjScheduleControl");
            if (objVer.Type == scheduledNodeType)
            {
                var props = vault.ObjectPropertyOperations.GetProperties(objVer);
                var startDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRealStartDate");
                var endDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRealEndDate");
                var realPeroidPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRealPeriod");
                var startDate = props.SearchForProperty(startDatePropId).Value;
                var endDate = props.SearchForProperty(endDatePropId).Value;
                //计算实际工期
                if (!startDate.IsNULL() && !endDate.IsNULL())
                {
                    //Writelog("eee" + endDate.ToString() + startDate.ToString());
                    var realDays = (endDate.Value - startDate.Value).Days + 1;
                    var pvRealDays = new PropertyValue();
                    pvRealDays.PropertyDef = realPeroidPropId;
                    pvRealDays.Value.SetValue(MFDataType.MFDatatypeInteger,realDays);
                    vault.ObjectPropertyOperations.SetProperty(objVer,pvRealDays);
                }
                //查看最后上传附件日期
                var attachPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropAttachment");
                var attach = props.SearchForProperty(attachPropId).Value;
                var setAddAttachDate = false;
                if (!attach.IsNULL())
                {
                    if (objVer.Version == 1)
                    {
                        setAddAttachDate = true;
                    }
                    else
                    {
                        var oldObjVer = new ObjVer();
                        oldObjVer.SetIDs(objVer.Type, objVer.ID, objVer.Version - 1);
                        var oldProps = vault.ObjectPropertyOperations.GetProperties(oldObjVer);
                        var oldAttach = oldProps.SearchForProperty(attachPropId).Value;
                        if (!attach.Equals(oldAttach))
                        {
                            setAddAttachDate = true;
                        }
                    }
                    if (setAddAttachDate)
                    {
                        var addAttachDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropAddAttachmentDate");
                        var datePv = new PropertyValue();
                        datePv.PropertyDef = addAttachDatePropId;
                        datePv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now);
                        vault.ObjectPropertyOperations.SetProperty(objVer, datePv);
                    }
                }
                
            }
        }

        /// <summary>
        /// 创建新的BIM模型对象之前需要处理的事件
        /// </summary>
        /// <param name="env"></param>
        [EventHandler(MFEventHandlerType.MFEventHandlerBeforeCreateNewObjectFinalize)]
        private void BeforeCreateNewBIMModelEventHandler(EventHandlerEnvironment env)
        {
            CheckBIMModleDuplicate(env);
        }
        /// <summary>
        /// 迁入BIM模型对象之前需要处理的事件
        /// </summary>
        /// <param name="env"></param>
        [EventHandler(MFEventHandlerType.MFEventHandlerBeforeCheckInChanges)]
        private void BeforeCheckInBIMModelEventHandler(EventHandlerEnvironment env)
        {
            CheckBIMModleDuplicate(env);
        }
        /// <summary>
        /// 判断BIM模型对象是否重复
        /// </summary>
        /// <param name="env"></param>
        private void CheckBIMModleDuplicate(EventHandlerEnvironment env)
        {
            var vault = env.Vault;
            var objVer = env.ObjVer;
            var objVerEx = env.ObjVerEx;
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassBimModelDoc");
            if (objVerEx.Class == classId)
            {
                var unitPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropModelUnitAt");
                var flootPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropFloorAt");
                var discPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropDisciplineAt");
                var props = vault.ObjectPropertyOperations.GetProperties(objVer);
                var unit = props.SearchForProperty(unitPropId).Value.GetValueAsLookup();
                var floot = props.SearchForProperty(flootPropId).Value.GetValueAsLookup();
                var disc = props.SearchForProperty(discPropId).Value.GetValueAsLookup();
                //搜索库中的BIM模型对象
                var scs = AddBaseConditions(0, objVerEx.Class, false);
                var unitSc = new SearchCondition(); //单体
                unitSc.Expression.DataPropertyValuePropertyDef = unitPropId;
                unitSc.ConditionType = MFConditionType.MFConditionTypeEqual;
                unitSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, unit);
                scs.Add(-1, unitSc); //楼层
                var footSc = new SearchCondition();
                footSc.Expression.DataPropertyValuePropertyDef = flootPropId;
                footSc.ConditionType = MFConditionType.MFConditionTypeEqual;
                footSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, floot);
                scs.Add(-1, footSc);
                var discSc = new SearchCondition();//专业
                discSc.Expression.DataPropertyValuePropertyDef = discPropId;
                discSc.ConditionType = MFConditionType.MFConditionTypeEqual;
                discSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, disc);
                scs.Add(-1, discSc);

                var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                if (res.Count > 1)
                {
                    throw new Exception("已存在相同单体，楼层和专业的BIM模型！");
                }
            }
        }

        private string GetCompanyName(Vault vault)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassProject");
            var scs = AddBaseConditions(typeId, classId, false);
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);

            if (res.Count > 0)
            {
                var objVer = res[1].ObjVer;
                var companyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
                return vault.ObjectPropertyOperations.GetProperty(objVer, companyPropId).GetValueAsLocalizedText();
            }
            return "";
        }
        private static SearchConditions AddBaseConditions(int typeId, int classId, bool deleted)
        {
            var oSearchConditions = new SearchConditions();
            var oSearchContitionType = new SearchCondition();
            oSearchContitionType.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchContitionType.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            oSearchContitionType.TypedValue.SetValue(MFDataType.MFDatatypeLookup, typeId);
            oSearchConditions.Add(-1, oSearchContitionType);

            var oSearchContitionClass = new SearchCondition();
            oSearchContitionClass.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchContitionClass.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            oSearchContitionClass.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            oSearchConditions.Add(-1, oSearchContitionClass);

            var oSearchContidionDeleted = new SearchCondition();
            oSearchContidionDeleted.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchContidionDeleted.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            oSearchContidionDeleted.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, deleted);
            oSearchConditions.Add(-1, oSearchContidionDeleted);

            return oSearchConditions;
        }

        [MFPropertyDef(Required = true)]
        public static MFIdentifier PropSecureAdjustDate = MfilesAliasConfig.PropSecureAdjustDate;

        //[EventHandler(MFEventHandlerType.MFEventHandlerBeforeCreateNewObjectFinalize)]
        //private void ClassProjectHandOverListProcessing(EventHandlerEnvironment env)//this  road is blocked
        //{
        //    try
        //    {
        //      //  Writelog("ClassProjectHandOverListProcessing 11 info:" +env.ObjVer.ID);
        //        var vault = env.Vault;
        //        var classid =
        //            env.PropertyValues.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass)
        //                .Value.GetLookupID();
        //        var classsid2 =
        //            vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
        //                "ClassProjectHandOverList");
              
        //        if (classid != classsid2) return;
        //        Writelog("ClassProjectHandOverListProcessing 33 info:" + classid + "--" + classsid2);
        //        var opw = new DocumentOperation(env);
        //        Writelog("ClassProjectHandOverListProcessing 44 info:" + classid + "--" + classsid2);
        //        opw.table.Cell(5, 2).Range.Text = opw.Project.PropProjName + opw.Project.PropProjNum;
        //        Writelog("ClassProjectHandOverListProcessing 55 info:" + classid + "--" + classsid2);
        //        opw.CloseWord();
        //        opw.UpDateFile2();

               
        //        Writelog("ClassProjectHandOverListProcessing 66 info:" + classid + "--" + classsid2);
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog("ClassProjectHandOverListProcessing error:"+ex.Message);
        //    }
        //}

        #region State Attributes 
        #region 质量整改通知单  
        [StateAction("WfsAssignAdjustmentSelfInspection")]
        private void WfsAssignAdjustmentSelfInspection(StateEnvironment env)
        {
            try
            {
                var me = new QualityAdjustmentNotice(env);
                me.WfsAssignAdjustmentSelfInspection();
            }
            catch (Exception ex)
            {
                Writelog("WfsAssignAdjustmentSelfInspection" + ex.Message);
            }
        }
        [StateAction("WfsCreateQualityNotice")]//WfsCreateQualityNotice
        private void WfsCreateQualityNotice(StateEnvironment env)
        {
            try
            {
                var me = new QualityAdjustmentNotice(env);
                me.CreateQualityNotice();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreateQualityNotice" + ex.Message);
            }
        }
        [StateAction("WfsSelfInspection")]
        private void WfsSelfInspection(StateEnvironment env)
        {
            try
            {
                var me = new QualityAdjustmentNotice(env);
                me.WfsSelfInspection();
            }
            catch (Exception ex)
            {
                Writelog("WfsSelfInspection" + ex.Message);
            }
        }
        [StateAction("WfsQualityAdjustmentAcceptance")]
        private void WfsQualityAdjustmentAcceptance(StateEnvironment env)
        {
            try
            {
                var me = new QualityAdjustmentNotice(env);
                me.WfsQualityAdjustmentAcceptance();
            }
            catch (Exception ex)
            {
                Writelog("WfsQualityAdjustmentAcceptance"+ex.Message);
            }
        }
        #endregion 质量整改通知单
        #region 安全检查管理流程- 工作流处理
        [StateAction("WfsSecureCheckStart")]
        private void WfsSecureCheckStart(StateEnvironment env)
        {
            try
            {
                var vault = env.Vault;
                var objver = env.ObjVer;
                var pv = new PropertyValue();
                pv.PropertyDef =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                        MfilesAliasConfig.PropCheckDate);
                pv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.Date);
                vault.ObjectPropertyOperations.SetProperty(objver, pv);
            }
            catch (Exception ex)
            {
                Writelog("WfsSecureCheckStart" + ex.Message);
            }
        }

        [StateAction("WfsSecureCheckReview")]
        private void WfsSecureCheckReview(StateEnvironment env)
        {
            try
            {
                var vault = env.Vault;
                var objver = env.ObjVer;
                var pv = new PropertyValue();
                pv.PropertyDef =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
                        MfilesAliasConfig.PropSecureAdjustDate);
                pv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.Date);
                vault.ObjectPropertyOperations.SetProperty(objver, pv);
            }
            catch (Exception ex)
            {
                Writelog("WfsSecureCheckReview"+ex.Message);
            }
        }
        #endregion 安全检查管理流程- 工作流处理

        #region 工程移交单- 工作流处理


        [StateAction("S.ProjectHandoverList.Create")]
        private void ProjectHandoverListCreate(StateEnvironment env)
        {
            try
            {
                var me = new ProjectHandoverList(env);
                me.ProjectHandoverListCreate();
            }
            catch (Exception ex)
            {
                Writelog("ProjectHandoverListCreate"+ex.Message);
            }
        }
        #endregion 工程移交单- 工作流处理
        #region 相关经济签证及采取措施-直属项目- 工作流处理
        [StateAction("S.VisaAndMeasure.EndDirectlyControl")]
        private void EndDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.EndDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("EndDirectlyControl"+ex.Message);
            }
        }
      

        [StateAction("S.VisaAndMeasure.CounterSignBusinessDirectlyControl")]
        private void VAMCounterSignSecondLevelBusinessDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.BusinessDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("VAMCounterSignSecondLevelBusinessDirectlyControl"+ex.Message);
            }
        }
        [StatePostConditions("S.VisaAndMeasure.UpdateDirectlyControl")]
        private bool UpdateDirectlyControl(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env, out message);
        }


        [StateAction("S.VisaAndMeasure.CreateDirectlyControl")]
        private void VisaAndMeasureCreateDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.CreateDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasureCreateDirectlyControl"+ex.Message);
            }
        }
        #endregion 相关经济签证及采取措施-一般项目- 工作流处理
        #region 相关经济签证及采取措施-一般项目- 工作流处理 
        [StateAction("S.VAM.CounterSign.SecondLevelEnd")]
        private void VAMCounterSignSecondLevelEnd(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.SecondLevelEnd();
            }
            catch (Exception ex)
            {
                Writelog("VAMCounterSignSecondLevelEnd"+ex.Message);
            }
        }
        [StateAction("S.VAM.CounterSign.SecondLevelChiefEconomist")]
        private void VAMCounterSignSecondLevelChiefEconomist(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.VAMCounterSignSecondLevelChiefEconomist();
            }
            catch (Exception ex)
            {
                Writelog("VAMCounterSignSecondLevelChiefEconomist"+ex.Message);
            }
        }

        [StateAction("S.VAM.CounterSign.SecondLevelBusiness")]
        private void VAMCounterSignSecondLevelBusiness(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.VAMCounterSignSecondLevelBusiness();
            }
            catch (Exception ex)
            {
                Writelog("VAMCounterSignSecondLevelBusiness"+ex.Message);
            }
        }
        [StatePostConditions("S.VisaAndMeasure.Update")]
        private bool VisaAndMeasureUpdate(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env, out message);
        }


        [StateAction("S.VisaAndMeasure.Create")]
        private void VisaAndMeasureCreate(StateEnvironment env)
        {
            try
            {
                var me = new VisaAndMeasure(env);
                me.Create();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasureCreate"+ex.Message);
            }
        }
        #endregion 相关经济签证及采取措施-一般项目- 工作流处理

        #region 工期延误分析及措施-正常延误- 工作流处理
        [StateAction("S.DelayAnalysis.Delay.DirectlyControl.End")]
        private void DelayAnalysisDelayDirectlyControlEnd(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisDelayDirectlyControlEnd();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayDirectlyControlEnd"+ex.Message);
            }
        }
        [StateAction("S.DelayAnalysis.Delay.General.End")]
        private void DelayAnalysisDelayGeneralEnd(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisDelayGeneralEnd();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayGeneralEnd"+ex.Message);
            }
        }
        [StateAction("S.DelayAnalysis.Normal.End")]
        private void DelayAnalysisNormalEnd(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisNormalEnd();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalEnd"+ex.Message);
            }
        }
        [StatePostConditions("S.DelayAnalysis.Normal.Update")]
        private bool DelayAnalysisNormalUpdate(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env, out message);
        }
        [StateAction("S.DelayAnalysis.Normal.CounterSign.PM")]
        private void DelayAnalysisNormalCounterSignPM(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisNormalCounterSignPM();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalCounterSignPM"+ex.Message);
            }
        }
         [StateAction("S.DelayAnalysis.Delay.DirectlyControl.Create")]
        private void DelayAnalysisDelayDirectlyControlCreate(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisDelayDirectlyControlCreate();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayDirectlyControlCreate"+ex.Message);
            }
        }
        [StateAction("S.DelayAnalysis.Delay.General.Create")]
        private void DelayAnalysisDelayGeneralCreate(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayAnalysis(env);
                me.DelayAnalysisDelayGeneralCreate();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayGeneralCreate"+ex.Message);
            }
        }
        [StateAction("S.DelayAnalysis.Normal.Create")]
        private void DelayAnalysisNormalCreate(StateEnvironment env)
        {
            try
            {
              //  Writelog("DelayAnalysisNormalCreate 11");
                var me = new ConstructionPeriodDelayAnalysis(env);
              //  Writelog("DelayAnalysisNormalCreate 22");
                me.DelayAnalysisNormalCreate();
             //   Writelog("DelayAnalysisNormalCreate 33");
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalCreate" + ex.Message);
            }
        }
        #endregion 工期延误分析及措施-正常延误- 工作流处理

        //#region 监理例会纪要审核- 工作流处理  //obsoleted
        //[StatePostConditions("S.SupervisionMeetingMinutesReview.Update")]
        //private bool SupervisionMeetingMinutesReviewUpdate(StateEnvironment env, out string message)
        //{
        //    return CheckToWhereFrom(env, out message);
        //}


        //[StateAction("S.SupervisionMeetingMinutesReview.Create")]
        //private void SupervisionMeetingMinutesReviewCreate(StateEnvironment env)
        //{
        //    try
        //    {
        //        var me = new SupervisionMeetingMinutesReview(env);
        //        me.Create();
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog(ex.Message);
        //    }
        //}
        //#endregion 监理例会纪要审核- 工作流处理
        #region 专业项目主要控制点计划-公司直属 工作流处理 S.MCPP.
        [StateAction("S.MCPP.DirectlyControlOver")]
        private void DirectlyControlOver(StateEnvironment env)
        {
            try
            {
                var me = new MainControlPointPlan(env);
                me.DirectlyControlOver();
            }
            catch (Exception ex)
            {
                Writelog("DirectlyControlOver"+ex.Message);
            }
        }
        [StatePostConditions("S.MCPP.UpdatePlanDirectlyControl")]
        private bool MCPPUpdatePlanDirectlyControl(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env,out message);
        }

     
        [StateAction("S.MCPP.CreatePlanDirectlyControl")]
        private void CreatePlanDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new MainControlPointPlan(env);
                me.MCPPCreatePlan();
            }
            catch (Exception ex)
            {
                Writelog("CreatePlanDirectlyControl"+ex.Message);
            }
        }
        #endregion 专业项目主要控制点计划-公司直属 工作流处理
        #region 专业项目主要控制点计划-二级单位 工作流处理 S.MCPP.SecondLevelOver
        [StateAction("S.MCPP.SecondLevelOver")]
        private void SecondLevelOver(StateEnvironment env)
        {
            try
            {
                var me = new MainControlPointPlan(env);
                me.SecondLevelOver();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelOver"+ex.Message);
            }
        }
        [StatePostConditions("S.MCPP.UpdatePlan")]
        private bool MCPPUpdatePlan(StateEnvironment env, out string message)
        {
            var vault = env.Vault;
            var objver = env.ObjVer;
            var lastobjver = new ObjVer();
            lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - 2);
            var last =
                vault.ObjectOperations.GetObjectVersionAndProperties(lastobjver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var laststate = last.Value.GetLookupID();
            var next =
                vault.ObjectOperations.GetObjectVersionAndProperties(objver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var nextstate = next.Value.GetLookupID();
            Writelog(string.Format("---- {0}, {1},{2}", laststate, env.StateID, nextstate));
            env.InitialNextStateID = laststate;
            message = "会签不通过返回修改后，再发起会签流程，需跳过会签通过的部门直接提交会签不通过的部门->" + last.GetValueAsLocalizedText();
            return nextstate == laststate;
        }
        [StateAction("S.MCPP.CreatePlan")]
        private void MCPPCreatePlan(StateEnvironment env)
        {
            try
            {
                var me = new MainControlPointPlan(env);
                me.MCPPCreatePlan();
            }
            catch (Exception ex)
            {
                Writelog("MCPPCreatePlan"+ex.Message);
            }
        }
        #endregion 专业项目主要控制点计划-二级单位 工作流处理

        #region 工程延期审批- 公司直属项目流程 工作流处理
        [StatePostConditions("S.CPDA.UpdateApprovalFormDirectlyControl")]
        private bool UpdateApprovalFormDirectlyControl(StateEnvironment env, out string message)
        {
            var vault = env.Vault;
            var objver = env.ObjVer;
            var lastobjver = new ObjVer();
            lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - 2);
            var last =
                vault.ObjectOperations.GetObjectVersionAndProperties(lastobjver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var laststate = last.Value.GetLookupID();
            var next =
                vault.ObjectOperations.GetObjectVersionAndProperties(objver)
                    .Properties.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var nextstate = next.Value.GetLookupID();
            Writelog(string.Format("---- {0}, {1},{2}", laststate, env.StateID, nextstate));
            env.InitialNextStateID = laststate;
            message = "会签不通过返回修改后，再发起会签流程，需跳过会签通过的部门直接提交会签不通过的部门->" + last.GetValueAsLocalizedText();
            return nextstate == laststate;
        }

        [StateAction("S.CPDA.PushOtherDepartmentsDirectlyControl")]
        private void PushOtherDepartmentsDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.PushOtherDepartments();
            }
            catch (Exception ex)
            {
                Writelog("PushOtherDepartmentsDirectlyControl"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignVicePresidentDirectlyControl")]
        private void CounterSignVicePresidentDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignVicePresident();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignVicePresidentDirectlyControl"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignConstructionManagerDirectlyControl")]
        private void CounterSignConstructionManagerDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignConstructionManager();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignConstructionManagerDirectlyControl"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignContractLegalManagerDirectlyControl")]
        private void CounterSignContractLegalManagerDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignContractLegalManagerDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignContractLegalManagerDirectlyControl"+ex.Message);
            }
        }
        [StateAction("S.CPDA.SecondLevelCounterSignManagerDirectlyControl")]
        private void SecondLevelCounterSignManagerDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.SecondLevelCounterSignManagerDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignManagerDirectlyControl"+ex.Message);
            }
        }

        [StateAction("WfsCreateConstructionPeriodDelayApprovalDirectlyControl")]
        private void WfsCreateConstructionPeriodDelayApprovalDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.WfsCreateConstructionPeriodDelayApproval();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreateConstructionPeriodDelayApprovalDirectlyControl"+ex.Message);
            }
        }

        #endregion 工程延期审批- 公司直属项目流程 工作流处理

        #region 工程延期审批-一般项目（二级单位项目）工作流处理
        [StatePostConditions("S.CPDA.UpdateApprovalForm")]
        private bool UpdateApprovalFormPostConditions(StateEnvironment env, out string message)
        {
            var vault = env.Vault;
            var objver = env.ObjVer;
            var lastobjver = new ObjVer();
            lastobjver.SetIDs(objver.Type, objver.ID, objver.Version - 2);
            var last =
                vault.ObjectOperations.GetObjectVersionAndProperties(lastobjver)
                    .Properties.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var laststate = last.Value.GetLookupID();
            var next =
                vault.ObjectOperations.GetObjectVersionAndProperties(objver)
                    .Properties.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefState);
            var nextstate =next.Value.GetLookupID();
            Writelog(string.Format("---- {0}, {1},{2}", laststate, env.StateID, nextstate));
            env.InitialNextStateID = laststate;
            message = "会签不通过返回修改后，再发起会签流程，需跳过会签通过的部门直接提交会签不通过的部门->" + last.GetValueAsLocalizedText();
            return nextstate==laststate;
        }

        [StateAction("S.CPDA.PushOtherDepartments")]
        private void PushOtherDepartmentsStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.PushOtherDepartments();
            }
            catch (Exception ex)
            {
                Writelog("PushOtherDepartmentsStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignVicePresident")]
        private void CounterSignVicePresidentStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignVicePresident();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignVicePresidentStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignConstructionManager")]
        private void CounterSignConstructionManagerStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignConstructionManager();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignConstructionManagerStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignContractLegalManager")]
        private void CounterSignContractLegalManagerStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignContractLegalManager();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignContractLegalManagerStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.SecondLevelCounterSignManager")]
        private void SecondLevelCounterSignManagerStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.SecondLevelCounterSignManager();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignManagerStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.SecondLevelCounterSignVicePresident")]
        private void SecondLevelCounterSignVicePresidentStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.SecondLevelCounterSignVicePresident();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignVicePresidentStateAction"+ex.Message);
            }
        }
        [StateAction("S.CPDA.SecondLevelChiefEconomistCounterSign")]
        private void SecondLevelChiefEconomistCounterSignStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.SecondLevelChiefEconomistCounterSign();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelChiefEconomistCounterSignStateAction"+ex.Message);
            }
        }
        [StateAction("WfsCreateConstructionPeriodDelayApproval")]
        private void WfsCreateConstructionPeriodDelayApprovalStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.WfsCreateConstructionPeriodDelayApproval();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreateConstructionPeriodDelayApprovalStateAction"+ex.Message);
            }
        }

        #endregion 工程延期审批-一般项目（二级单位项目）工作流处理

        #region 项目竣工确认-公司直属
        [StateAction("WfsForTheRecordDirectlyControl")]
        private void WfsForTheRecordDirectlyControlStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ProjectCompletionConfirm(env);
                me.WfsForTheRecordDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("WfsForTheRecordDirectlyControlStateAction"+ex.Message);
            }
        }
        [StateAction("WfsCreatePccDirectlyControl")]
        private void WfsCreatePccDirectlyControlStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ProjectCompletionConfirm(env);
                me.WfsCreatePccDirectlyControl();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreatePccDirectlyControlStateAction"+ex.Message);
            }
        }
        #endregion 项目竣工确认-公司直属
        [StateAction("WfsForTheRecordSecondLevel")]
        private void WfsForTheRecordSecondLevelStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ProjectCompletionConfirm(env);
                me.FillCounterSign();
            }
            catch (Exception ex)
            {
                Writelog("WfsForTheRecordSecondLevelStateAction"+ex.Message);
            }
        }
        [StateAction("WfsProjectCompletionConfirmSecondLevel")]
        private void WfsProjectCompletionConfirmSecondLevelStateAction(StateEnvironment env)
        {
            try
            {
                var me = new ProjectCompletionConfirm(env);
                me.FillCreatePcc();
            }
            catch (Exception ex)
            {
                Writelog("WfsProjectCompletionConfirmSecondLevelStateAction"+ex.Message);
            }
        }
        [StateAction("WfsUndoCommandOver")]
        private void WfsUndoCommandOverStateAction(StateEnvironment env)
        {
            try
            {
                var me = new UndoCommand(env);
                me.FillApprovedInfo();
            }
            catch (Exception ex)
            {
                Writelog("WfsUndoCommandOverStateAction"+ex.Message);
            }
        }
        [StateAction("WfsUndoCommandApproved")]
        private void WfsUndoCommandApprovedStateAction(StateEnvironment env)
        {
            try
            {
                var me = new UndoCommand(env);
                me.FillAuditedInfo();
            }
            catch (Exception ex)
            {
                Writelog("WfsUndoCommandApprovedStateAction"+ex.Message);
            }
        }
        [StateAction("WfsCreateUndoCommand")]
        private void WfsCreateUndoCommandStateAction(StateEnvironment env)
        {
            try
            {
                var me = new UndoCommand(env);
                me.FillCreateInfo();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreateUndoCommandStateAction"+ex.Message);
            }
        }
        [StateAction("WfsCounterSignDirectlyControl")]
        private void WfsCounterSignDirectlyControlStateAction(StateEnvironment env)
        {
            try
            {
                var me = new MonthlyEvaluation(env,true,false);
                me.FillDocDirectlyControl(env);
            }
            catch (Exception ex)
            {
                Writelog("WfsCounterSignDirectlyControlStateAction"+ex.Message);
            }
        }
         [StateAction("WfsCounterSignSecondLevel")]
        private void WfsCounterSignSecondLevelStateAction(StateEnvironment env)
        {
            try
            {
                var me = new MonthlyEvaluation(env);
                me.FillDocSecondLevel(env);
            }
            catch (Exception ex)
            {
                Writelog("WfsCounterSignSecondLevelStateAction"+ ex.Message);
            }
        }
         [StateAction("WfsMonthlyEvaluationDirectlyControlApproved")]
         private void WfsMonthlyEvaluationDirectlyControlApprovedStateAction(StateEnvironment env)
         {
             try
             {
                 var me = new MonthlyEvaluation(env, false);
                 me.DirectlyTablePartThree();
             }
             catch (Exception ex)
             {
                 Writelog("WfsMonthlyEvaluationDirectlyControlApprovedStateAction"+ex.Message);
             }
         }
         [StateAction("WfsMonthlyEvaluationSecondLevelApproved")]
         private void WfsMonthlyEvaluationSecondLevelApprovedStateAction(StateEnvironment env)
         {
             try
             {
                 var me = new MonthlyEvaluation(env,false);
                 me.SecondLevelTablePartThree();
             }
             catch (Exception ex)
             {
                 Writelog("WfsMonthlyEvaluationSecondLevelApprovedStateAction"+ex.Message);
             }
         }
      
         
        //[StateAction("WfsSecureAdjust")]
        //private void WfsSecureAdjustStateAction(StateEnvironment ehe)
        //{
        //    try
        //    {
        //        var pv = ehe.PropertyValues.SearchForProperty(PropSecureAdjustDate.ID);
        //        pv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.ToString("d"));
        //        ehe.Vault.ObjectPropertyOperations.SetProperty(ehe.ObjVer, pv);
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog(PropSecureAdjustDate.Alias + PropSecureAdjustDate.ID + ex.Message);
        //    }
        //}
        
        #endregion State Attributes

//        [PropertyCustomValue("PropConstructionScaleAuto")]
//        private TypedValue PropConstructionScaleAutoCustomValue(PropertyEnvironment env)
//        {
//            /* for backup
//             * 
//             * dim sc : set sc = CreateObject("MFilesAPI.SearchCondition")
//sc.ConditionType = MFConditionType.MFConditionTypeEqual
//sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID

//dim lu : set  lu = CreateObject("MFilesAPI.Lookup")
//lu.Item = vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType, "ObjProject")
//sc.TypedValue.SetValueToLookup(lu)

//dim sr : set sr = vault.ObjectSearchOperations.SearchForObjectsByCondition(sc, false).ObjectVersions

//dim pid :  pid = vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef, "PropConstructionScale", false)

//dim oneobjver
//For Each one in sr
//    set oneobjver = one.ObjVer
//    exit for
//Next   
 
//dim str :  str = vault.ObjectPropertyOperations.GetProperty(oneobjver, pid).GetValueAsLocalizedText()

//dim pv : set pv = CreateObject("MFilesAPI.TypedValue")
//pv.SetValue MFDataType.MFDatatypeText, str

//output =  pv
//             * */
//            var vault = env.Vault;
//            var sc = new SearchCondition
//            {
//                ConditionType = MFConditionType.MFConditionTypeEqual,
//                Expression = { DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID }
//            };
//            sc.TypedValue.SetValueToLookup(new Lookup { Item = vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType, "ObjProject") });
//            var sr = vault.ObjectSearchOperations.SearchForObjectsByCondition(sc, false).ObjectVersions;
//            var pv = new TypedValue();
//            pv.SetValue(MFDataType.MFDatatypeText, "");
//            foreach (ObjectVersion objectVersion in sr)
//            {
//                pv.SetValue(MFDataType.MFDatatypeText, vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer,
//                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
//                        "PropConstructionScale")).GetValueAsLocalizedText());
//                break;
//            }
//            return pv;
//        }

//        [PropertyCustomValue("PropProprietorUnitAuto")]
//        private TypedValue PropProprietorUnitAutoCustomValue(PropertyEnvironment env)
//        {
//            //var vault = env.Vault;
//            //var sc = new SearchCondition
//            //{
//            //    ConditionType = MFConditionType.MFConditionTypeEqual,
//            //    Expression = {DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID}
//            //};
//            //sc.TypedValue.SetValueToLookup(new Lookup{Item = vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType, "ObjProject")});
//            //var sr = vault.ObjectSearchOperations.SearchForObjectsByCondition(sc,false).ObjectVersions;
//            //var pv = new TypedValue();
//            //pv.SetValue(MFDataType.MFDatatypeText, "");
//            //foreach (ObjectVersion objectVersion in sr)
//            //{
//            //    pv.SetValue(MFDataType.MFDatatypeText, vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer,
//            //        vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
//            //            "PropProprietorUnit")).GetValueAsLocalizedText()); 
//            //    break;
//            //}
//            var pv = new TypedValue();
//            pv.SetValue(MFDataType.MFDatatypeText, "haha");
//            return pv;
//        }
        //[EventHandler(MFEventHandlerType.MFEventHandlerBeforeCheckInChanges)]
        //private void MFEventHandlerBeforeCheckInChanges(EventHandlerEnvironment env)
        //{
        //    MonthlyEvaluation.FillDoc1(env);
        //}

        [VaultExtensionMethod("GetWebHost", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string GetWebHost(EventHandlerEnvironment env)
        {
            return GetHost();
        }
        /// <summary>
        /// A vault extension method, that will be installed to the vault with the application.
        /// The vault extension method can be called through the API.
        /// </summary>
        /// <param name="env">The event handler environment for the method.</param>
        /// <returns>The output string to the caller.</returns>
        [VaultExtensionMethod("GetFilterPrincipal", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string GetFilterPrincipal(EventHandlerEnvironment env)
        {
            return SecureNotice.GetFilterPrincipal(env);
        }
        [VaultExtensionMethod("GetFilterReceiver", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string GetFilterReceiver(EventHandlerEnvironment env)
        {
            return SecureNotice.GetFilterReceiver(env);
        }


        [VaultExtensionMethod("GetWfnStates", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string GetWfnStates(EventHandlerEnvironment env)
        {
            return WorkFlowNew.GetWfnStates(env);
        }
      
        [VaultExtensionMethod("GetSecureNoticeV2", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string GetSecureNoticeV2(EventHandlerEnvironment env)
        {
            return SecureNotice.GetSecureNoticeNew(env);
        }
       // [VaultExtensionMethod("getWorkFlowStates", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        //public string getWorkFlowStates(EventHandlerEnvironment env)
        //{
        //    return MfWorkflow.GetWorkflow(env);
        //}
        private  void Writelog(string logtext)
        {
            var ts = GetTrace(MethodBase.GetCurrentMethod().Name);
            ts.TraceInformation(DateTime.Now.ToLocalTime() + "参考信息：" + logtext);
            ts.Close();
        }
    }
}
