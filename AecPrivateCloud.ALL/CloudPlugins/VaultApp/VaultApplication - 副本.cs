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
        /// 创建履约率对象时 根据项目的所属单位赋予不同的命名访问控制列表
        /// </summary>
        /// <param name="env"></param>
        [EventHandler(MFEventHandlerType.MFEventHandlerAfterCreateNewObjectFinalize)]
        private void SetNacl2PerformRateObj(EventHandlerEnvironment env)
        {
            var vault = env.Vault;
            var objVer = env.ObjVer;
            var performObjType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjPerformRate");
            if (objVer.Type == performObjType) 
            {
                //查询项目
                var projPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProject");
                var projObjId = vault.ObjectPropertyOperations.GetProperty(objVer, projPropId).Value.GetValueAsLookup().Item;
                var projType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
                var projObjID = new ObjID();
                projObjID.SetIDs(projType,projObjId);
                var projObjVer = vault.ObjectOperations.GetLatestObjVer(projObjID, false);
                //查询所属公司
                var projOwnerPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
                var projOwner = vault.ObjectPropertyOperations.GetProperty(projObjVer, projOwnerPropId).Value.DisplayValue;
                if (projOwner == "中建八局第二建设有限公司") //公司总部
                {
                    var nacl = vault.NamedACLOperations.GetNamedACLIDByAlias("NaclPerformRateHead");
                    vault.ObjectOperations.ChangePermissionsToNamedACL(objVer, nacl, true);
                }
                else//分公司 
                {
                    var nacl = vault.NamedACLOperations.GetNamedACLIDByAlias("NaclPerformRateAll");
                    vault.ObjectOperations.ChangePermissionsToNamedACL(objVer, nacl, true);
                }
            }
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
            }
        }
        [StateAction("S.DelayAnalysis.Normal.Create")]
        private void DelayAnalysisNormalCreate(StateEnvironment env)
        {
            try
            {
                Writelog("DelayAnalysisNormalCreate 11");
                var me = new ConstructionPeriodDelayAnalysis(env);
                Writelog("DelayAnalysisNormalCreate 22");
                me.DelayAnalysisNormalCreate();
                Writelog("DelayAnalysisNormalCreate 33");
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalCreate" + ex.Message);
            }
        }
        #endregion 工期延误分析及措施-正常延误- 工作流处理

        #region 监理例会纪要审核- 工作流处理
        [StatePostConditions("S.SupervisionMeetingMinutesReview.Update")]
        private bool SupervisionMeetingMinutesReviewUpdate(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env, out message);
        }


        [StateAction("S.SupervisionMeetingMinutesReview.Create")]
        private void SupervisionMeetingMinutesReviewCreate(StateEnvironment env)
        {
            try
            {
                var me = new SupervisionMeetingMinutesReview(env);
                me.Create();
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
            }
        }
        #endregion 监理例会纪要审核- 工作流处理
        #region 专业项目主要控制点计划-公司直属 工作流处理
        [StatePostConditions("S.MCPP.UpdatePlanDirectlyControl")]
        private bool MCPPUpdatePlanDirectlyControl(StateEnvironment env, out string message)
        {
            return CheckToWhereFrom(env,out message);
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
                Writelog(ex.Message);
            }
        }
        #endregion 专业项目主要控制点计划-公司直属 工作流处理
        #region 专业项目主要控制点计划-二级单位 工作流处理
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
            }
        }
        [StateAction("S.CPDA.CounterSignContractLegalManagerDirectlyControl")]
        private void CounterSignContractLegalManagerDirectlyControl(StateEnvironment env)
        {
            try
            {
                var me = new ConstructionPeriodDelayApproval(env);
                me.CounterSignContractLegalManager();
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
            }
        }

        #endregion 工程延期审批-一般项目（二级单位项目）工作流处理

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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
            }
        }
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
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
                Writelog(ex.Message);
            }
        }
        [StateAction("WfsCounterSignDirectlyControl")]
        private void WfsCounterSignDirectlyControlStateAction(StateEnvironment env)
        {
            try
            {
                var me = new MonthlyEvaluation(env);
                me.FillDocDirectlyControl(env);
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
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
                Writelog( ex.Message);
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
                 Writelog(ex.Message);
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
                 Writelog(ex.Message);
             }
         }
      
         
        [StateAction("WfsSecureAdjust")]
        private void WfsSecureAdjustStateAction(StateEnvironment ehe)
        {
            try
            {
                var pv = ehe.PropertyValues.SearchForProperty(PropSecureAdjustDate.ID);
                pv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.ToString("d"));
                ehe.Vault.ObjectPropertyOperations.SetProperty(ehe.ObjVer, pv);
            }
            catch (Exception ex)
            {
                Writelog(PropSecureAdjustDate.Alias + PropSecureAdjustDate.ID + ex.Message);
            }
        }
        
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
