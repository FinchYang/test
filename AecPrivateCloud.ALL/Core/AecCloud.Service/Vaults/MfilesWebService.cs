using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;
using System.IO;

namespace AecCloud.Service.Vaults
{
    public class MfilesWebService: IMfilesWebService
    {
        public List<ScheduleNode> ScheduleControlStatistics(Dictionary<string, string> guidAndIps, string userName, string pwd, string name)
        {
            var list = new List<ScheduleNode>();
            if (guidAndIps.Count == 0) return list;
            //循环每一个库
            foreach (var guidAndIp in guidAndIps)
            {
                try
                {
                    //登录vault客户端
                    var user = new UserDto(MFAuthType.MFAuthTypeSpecificMFilesUser, userName, pwd, "", guidAndIp.Value);
                    var vault = Connect2VaultOnServer(user, guidAndIp.Key);
                    if (vault == null) continue;
                    //搜索工期节点对象
                    var scheduleObjs = GetscheduleObjs(vault, name);
                    //循环每隔一个节点
                    foreach (ObjectVersion objVersion in scheduleObjs)
                    {
                        var objVer = objVersion.ObjVer;
                        var planPeriodPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropPlanPeriod");
                        var realPeriodPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRealPeriod");
                        var snPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropSN");
                        var pvs = vault.ObjectPropertyOperations.GetProperties(objVer, false);
                        var sn = pvs.SearchForProperty(snPropId).GetValueAsLocalizedText();
                        var nodeName = pvs.SearchForProperty(0).GetValueAsLocalizedText();
                        var planPeriod = pvs.SearchForProperty(planPeriodPropId).Value.Value;
                        var realPeriod = pvs.SearchForProperty(realPeriodPropId).Value.Value;
                        if (planPeriod is int && realPeriod is int)
                        {//计划工期和实际工期都存在
                            var obj = list.Find(x => x.Sn == sn);
                            if (obj != null)
                            {//已存在该节点
                                var periodPair = new PeriodPair() { PlanPeriod = planPeriod, RealPeriod = realPeriod };
                                obj.Schedule.Add(periodPair);
                            }
                            else
                            {//不存在该节点
                                var node = new ScheduleNode();
                                node.Sn = sn;
                                node.Name = nodeName.Substring(nodeName.IndexOf('-') + 1);
                                node.Schedule.Add(new PeriodPair() { PlanPeriod = planPeriod, RealPeriod = realPeriod });
                                list.Add(node);
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
            //整理统计值
            foreach (ScheduleNode scheduleNode in list)
            {
                int miniDev = 10000000;
                int maxDev = -10000000;
                int totalDev = 0;
                double totalDevRate = 0.0;
                int nodeNum = 0;
                foreach(PeriodPair periodPair in scheduleNode.Schedule)
                {
                    var currentDev = periodPair.RealPeriod - periodPair.PlanPeriod;
                    if (currentDev < miniDev)
                    {
                        miniDev = currentDev;
                    }
                    if (currentDev > maxDev)
                    {
                        maxDev = currentDev;
                    }
                    totalDev += Math.Abs(periodPair.RealPeriod - periodPair.PlanPeriod);
                    totalDevRate += Math.Abs((double)(periodPair.RealPeriod - periodPair.PlanPeriod) / periodPair.PlanPeriod);
                    nodeNum++;
                }
                scheduleNode.DevMax = maxDev;
                scheduleNode.DevMini = miniDev;
                scheduleNode.DevAvg = Math.Round((double)totalDev/nodeNum,2).ToString();
                var devAvgRate = totalDevRate/nodeNum;
                scheduleNode.DevAvgRate = (Math.Round(devAvgRate, 4) * 100).ToString();
            }

            //排序
            list = list.OrderBy(x => OrderOperator(x.Sn)).ToList();
            return list;
        }

        public static void Writelog(string logtext)
        {
            try
            {
                using (
                    StreamWriter sw =
                        File.AppendText(Path.Combine(Path.GetTempPath(),
                            DateTime.Now.Date.ToString("yyyy-MM-dd") + "vaultWebLog.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        private string OrderOperator(string sn)
        {
            if (sn.IndexOf('_') < 0)
            {//没有子项
                if (sn.Length == 1)
                {
                    return "0" + sn + "_00";
                }
                else
                {
                    return sn + "_00";
                }
            }
            else
            {//有子项
                var strArr = sn.Split('_');
                if (strArr[0].Length == 1)
                {
                    strArr[0] = "0" + strArr[0];
                }
                if (strArr[1].Length == 1)
                {
                    strArr[1] = "0" + strArr[1];
                }
                return strArr[0] + "_" + strArr[1];
            }
        }
        /// <summary>
        /// 搜索工期对象
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private ObjectSearchResults GetscheduleObjs(Vault vault, string name)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjScheduleControl");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassScheduleControl");
            var scs = AddBaseConditions(typeId, classId, false);
            if (!String.IsNullOrEmpty(name))
            {
                var sc = new SearchCondition();
                sc.ConditionType = MFConditionType.MFConditionTypeContains;
                sc.Expression.DataPropertyValuePropertyDef = 0;
                sc.TypedValue.SetValue(MFDataType.MFDatatypeText, name);
                scs.Add(-1, sc);
            }
            return vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
        }
        /// <summary>
        /// 监理例会统计
        /// </summary>
        /// <param name="guidAndIps"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public List<CompanyMeetingStatics> SupervisorMeetingStatics(List<CompanyMeetingStatics> list,Dictionary<string, string> guidAndIps, string userName, string pwd, string year, string month)
        {
            //var list = new List<CompanyMeetingStatics>();
            if (guidAndIps.Count == 0) return list;
            //循环每一个库
            //var isFstVault = true;
            foreach (var guidAndIp in guidAndIps)
            {
                try
                {
                    //登录vault客户端
                    var user = new UserDto(MFAuthType.MFAuthTypeSpecificMFilesUser, userName, pwd, "", guidAndIp.Value);
                    var vault = Connect2VaultOnServer(user, guidAndIp.Key);
                    if (vault == null) continue;
                    //第一个库时 通过所属公司值列表 获取公司信息 并初始化list
                    //if (isFstVault)
                    //{
                    //    list = GetUnitInfo(vault, list);
                    //    isFstVault = false;
                    //}
                    //搜索符合要求的监理会议申请对象
                    var meetingObjs = GetMeetingObjs(vault, year, month);
                    var projName = GetProjName(vault);
                    var companyName = GetCompanyName(vault);
                    if (meetingObjs.Count != 0) //有例会
                    {
                        var projPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProject");
                        var projMeeting = new ProjMeetingStatics();
                        foreach (var objVersion in meetingObjs)
                        {
                            projMeeting.ProjName = projName;
                            projMeeting.MeetingNums += 1;
                           // Writelog(objVersion.Title + "|" + vault.Name);
                        }
                        list.First(x => x.CompanyName == companyName).ProjMeetingList.Add(projMeeting);
                    }
                    else//无例会
                    {
                        var undoingObj = GetUndoingReasonObj(vault, year, month);
                        if (undoingObj != null)
                        {
                            var reasonPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropReason");
                            var reason = vault.ObjectPropertyOperations.GetProperty(undoingObj.ObjVer, reasonPropId)
                                                    .GetValueAsLocalizedText();
                            var projMeeting = new ProjMeetingStatics();
                            projMeeting.ProjName = projName;
                            projMeeting.UndidReason = reason;
                            list.First(x => x.CompanyName == companyName).ProjMeetingList.Add(projMeeting);
                        }
                    } //Writelog("vaultname:" + vault.Name);  

                }
                catch { }
            }
            //计算公司所有项目的例会总数
            foreach (var cmpItem in list)
            {
                // Writelog("meetnum:" + cmpItem.ProjMeetingList.Count);
                foreach (var projItem in cmpItem.ProjMeetingList)
                {
                    cmpItem.MeetingNums += projItem.MeetingNums;
                }
            }

            return list;
        }
        /// <summary>
        /// 获取项目名称
        /// </summary>
        /// <returns></returns>
        private string GetProjName(Vault vault)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassProject");
            var scs = AddBaseConditions(typeId, classId, false);
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);

            if (res.Count > 0)
            {
                return res[1].Title;
            }

            return "";
        }
        /// <summary>
        /// 获取公司名称
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 返回搜索当月的未召开会议原因对象
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private ObjectVersion GetUndoingReasonObj(Vault vault, string year, string month)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjUnMeetingReason");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassUnMeetingReason");
            var scs = AddBaseConditions(typeId, classId, false);
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            var datePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropUnMeetingDate");
            foreach (ObjectVersion objVersion in res)
            {
                var date = vault.ObjectPropertyOperations.GetProperty(objVersion.ObjVer, datePropId).GetValueAsLocalizedText();
                var dateArr = date.Split('/');
                if (dateArr[0] == year && dateArr[1] == month)
                {
                    return objVersion;
                }
            }
            return null;
        }
        /// <summary>
        /// 搜索符合要求的监理会议申请对象
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private List<ObjectVersion> GetMeetingObjs(Vault vault, string year, string month)
        {
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassSupervisorMeeting");
            var scs = AddBaseConditions(0, classId, false);
            //状态
            var stateId = vault.WorkflowOperations.GetWorkflowStateIDByAlias("WFSSupervisorMeetingFinished");
            var sc = new SearchCondition();
            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = 39;
            sc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, stateId);
            scs.Add(-1, sc);

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            var objVersons = new List<ObjectVersion>();
            var datePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRegularMeetingDate");
            foreach (ObjectVersion objVersion in res)
            {
                var date = vault.ObjectPropertyOperations.GetProperty(objVersion.ObjVer, datePropId).GetValueAsLocalizedText();
                var dateArr = date.Split('/');
                if (dateArr[0] == year && dateArr[1] == month)
                {
                    objVersons.Add(objVersion);
                }
            }
            return objVersons;
        }
        /// <summary>
        /// 返回通过类型，类别和删除搜索 searchConditions
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="classId"></param>
        /// <param name="deleted"></param>
        /// <returns></returns>
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
        private List<CompanyMeetingStatics> GetUnitInfo(Vault vault, List<CompanyMeetingStatics> list)
        {
            var vlId = vault.ValueListOperations.GetValueListIDByAlias("VlCompanies"); 
            var items = vault.ValueListItemOperations.GetValueListItems(vlId);
            for (int i = 1; i <= items.Count; i++)
            {
                list.Add(new CompanyMeetingStatics { CompanyName = items[i].Name });
            }
            return list;
        }
        private Vault Connect2VaultOnServer(UserDto user, string vaultGuid)
        {
            var serverApp = new MFilesServerApplication();
            serverApp.Connect(user.MfType, user.UserName, user.PassWord, user.Domain, "ncacn_ip_tcp", user.ip);
            Vault gVault = serverApp.LogInToVault(vaultGuid);
            //conn
            return gVault;
        }   
    }
}
