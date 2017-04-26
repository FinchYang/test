using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace AecCloud.Service.Vaults
{
    public class MFilesPerformService:IMFilesPerformService
    {
         public List<UnitPerformaceModel> GetPerformRateUnit(Dictionary<string,string> guidAndIps, string username, string pwd, int year, int month,string compName)
        {
            var list = new List<UnitPerformaceModel>();
            if (guidAndIps.Count == 0) return list;
            //初始化列表
            list = InitialUnitPerformList(list);
            //循环每一个库
            foreach (var guidAndIp in guidAndIps)
            {
                //登录vault客户端
                var user = new UserDto(MFAuthType.MFAuthTypeSpecificMFilesUser, username, pwd, "", guidAndIp.Value);
                Vault vault = null;
                try
                {
                    vault = Connect2VaultOnServer(user, guidAndIp.Key);
                    if (vault == null) continue;
                    //搜索选定公司 年份 和月份下的履约率对象
                    var performRateObj = GetPerformRateObj(vault, year, month,compName);
                    if (performRateObj == null) continue;
                    //将履约率对象信息插入list
                    InsertPerformRateInList(vault, performRateObj, list);
                    //按类别统计履约率
                    StatisticsPerformRate(list);
                }
                catch { }
            }
            return list;
        }

        /// <summary>
        /// 按类别统计履约率
        /// </summary>
        /// <param name="list"></param>
        private void StatisticsPerformRate(List<UnitPerformaceModel> list)
        {
            foreach(var item in list)
            {
                if(item.ProjPerformInfos.Count >0)
                {
                    var totalRate = 0.0;
                    foreach(var projRate in item.ProjPerformInfos)
                    {
                        totalRate += projRate.PerformRate;
                    }
                    item.PerformRate = totalRate / item.ProjPerformInfos.Count;
                }
            }
        }


        /// <summary>
        /// 履约率对象信息插入信息列表
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="performRateObj"></param>
        /// <param name="list"></param>
        private void InsertPerformRateInList(Vault vault,ObjectVersion performRateObj,List<UnitPerformaceModel> list)
        {
            //属性id
            //var projPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProject");
            var ContactValuePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropContactValue");
            var startDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropStartDate");
            var completeDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompleteDate");
            var changedDatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropChangedDate");
            var projPeroidPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProjPeroid");
            var realDelayPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRealDelayTime");
            var comfirmDelayPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropComfirmDelayTime");
            var uncomfirmDelayPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropUncomfirmDelayTime");
            var totalProjValuePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropTotalProjValue");
            var comfirmCompenValuePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropComfirmCompenValue");
            var comfirmCompenExplainPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropComfirmCompenExplain");
            var isWarningPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropIsWarning");
            var remarkPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRemark");
            var performRatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropPerformRate");
            //属性值
            //var projName = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, projPropId).Value.DisplayValue;
            var contactValue = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, ContactValuePropId).Value.DisplayValue;
            var startDate = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, startDatePropId).Value.DisplayValue;
            var completeDate = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, completeDatePropId).Value.DisplayValue;
            var changedDate = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, changedDatePropId).Value.DisplayValue;
            var projPeroid = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, projPeroidPropId).Value.DisplayValue;
            var realDelay = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, realDelayPropId).Value.DisplayValue;
            var comfirmDelay = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, comfirmDelayPropId).Value.DisplayValue;
            var uncomfirmDelay = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, uncomfirmDelayPropId).Value.DisplayValue;
            var totalProjValue = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, totalProjValuePropId).Value.DisplayValue;
            var comfirmCompenValue = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, comfirmCompenValuePropId).Value.DisplayValue;
            var comfirmCompenExplain = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, comfirmCompenExplainPropId).Value.DisplayValue;
            var isWarning = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, isWarningPropId).Value.DisplayValue;
            var remark = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, remarkPropId).Value.DisplayValue;
            var performRate = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, performRatePropId).Value.Value;
            //创建模型
            var model = new ProjPerformInfo();
            model.ProjName = GetProjObjVersion(vault).Title;
            model.ProjValue = contactValue;
            model.StartDate = startDate;
            model.CompleteDate = completeDate;
            model.ChangedCompeteDate = changedDate;
            model.ProjPeroid = projPeroid;
            model.RealDelayTime = realDelay;
            model.ComfirmDelayTime = comfirmDelay;
            model.UncomfirmDelayTime = uncomfirmDelay;
            model.TotalProjValue = totalProjValue;
            model.ComfirmCompenValue = comfirmCompenValue;
            model.ComfirmCompenExplain = comfirmCompenExplain;
            model.IsWarning = isWarning;
            model.Remark = remark;
            model.PerformRate = performRate;
            //计算项目类别
            //var projObjId = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, projPropId).Value.GetValueAsLookup().Item;
            //var projType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
            //var projObjID = new ObjID();
            //projObjID.SetIDs(projType,projObjId);
            //var projObjVer = vault.ObjectOperations.GetLatestObjVer(projObjID,false,false);
            var projObjVer = GetProjObjVersion(vault).ObjVer;
            var projClassPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProjClass");
            var projClass = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, projClassPropId).Value.DisplayValue;

            model.Sn = list.First(x => x.ProjClass == projClass).ProjPerformInfos.Count + 1;
            list.First(x => x.ProjClass == projClass).ProjPerformInfos.Add(model);     
        }
        /// <summary>
        /// 搜索选定公司 年份 和月份下的履约率对象
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="compName"></param>
        /// <returns></returns>
        private ObjectVersion GetPerformRateObj(Vault vault,int year,int month,string compName)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjPerformRate");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassPerformRate");
            var scs = AddBaseConditions(typeId, classId, false);
            //履约率日期  
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);

            var propId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropPerformRateDate");
            //var projProId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProject");
            var projType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
            var compPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
            var dic = new Dictionary<ObjectVersion, DateTime>();
            foreach (ObjectVersion objVersion in res)
            {
                var objVer = objVersion.ObjVer;
                //公司名匹配
                //var projId = vault.ObjectPropertyOperations.GetProperty(objVer, projProId).Value.GetValueAsLookup().Item;
                //var objID = new ObjID();
                //objID.SetIDs(projType,projId);
               // var projObjVer = vault.ObjectOperations.GetLatestObjVer(objID,false,false);
                var projObjVer = GetProjObjVersion(vault).ObjVer;
                var unitName = vault.ObjectPropertyOperations.GetProperty(projObjVer, compPropId).Value.DisplayValue;
                if(unitName != compName)continue; //单位过滤                
                var date = vault.ObjectPropertyOperations.GetProperty(objVer, propId).Value.DisplayValue;
                var dateArr = date.Split('/');
                if (dateArr[0] == year.ToString() && dateArr[1] == month.ToString())
                {
                    dic.Add(objVersion, objVersion.CreatedUtc);
                }
            }

            if (dic.Count == 0)
            {
                return null;
            }
            else
            {
                return dic.First(y => y.Value == dic.Max(x => x.Value)).Key;
            } 
        }
        /// <summary>
        /// 初始化列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<UnitPerformaceModel> InitialUnitPerformList(List<UnitPerformaceModel> list)
        {
            list.Add(new UnitPerformaceModel { Sn = 1, ProjClass = "公建项目", ProjPerformInfos = new List<ProjPerformInfo>() });
            list.Add(new UnitPerformaceModel { Sn = 2, ProjClass = "房地产项目" , ProjPerformInfos = new List<ProjPerformInfo>()});
            list.Add(new UnitPerformaceModel { Sn = 3, ProjClass = "基础设施项目", ProjPerformInfos = new List<ProjPerformInfo>() });
            list.Add(new UnitPerformaceModel { Sn = 4, ProjClass = "融资类项目" , ProjPerformInfos = new List<ProjPerformInfo>()});
            list.Add(new UnitPerformaceModel { Sn = 5, ProjClass = "工期变更大的项目", ProjPerformInfos = new List<ProjPerformInfo>() });

            return list;
        }

        public List<PerformanceRateModel> GetPerformRate(List<PerformanceRateModel>list,Dictionary<string, string> guidAndIps, string username, string pwd, int year, int month)
        {
           // var list = new List<PerformanceRateModel>();
            if (guidAndIps.Count == 0) return list;
            //循环每一个库
           // var isFstVault = true;
            foreach (var guidAndIp in guidAndIps)
            { 
                //登录vault客户端
                var user = new UserDto(MFAuthType.MFAuthTypeSpecificMFilesUser, username, pwd, "", guidAndIp.Value);
                Vault vault = null;
                try
                {
                    vault = Connect2VaultOnServer(user, guidAndIp.Key);
                    if (vault == null) continue;
                    //第一个库时 通过所属公司值列表 获取公司信息 并初始化list
                    //if (isFstVault)
                    //{
                    //    list = GetUnitInfo(vault,list);
                    //    isFstVault = false;
                    //}
                    //搜索符合要求的履约率对象
                    var performRateObj = GetPerformRateObj(vault,year,month);
                    if (performRateObj == null) continue;
                    //计算履约率
                    var performRatePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropPerformRate");
                    var companyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropCompany");
                    var projClassPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProjClass");
                    var projType = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
                   // var projPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropProject");                          
                    //计算项目objVer
                    var objVer = performRateObj.ObjVer;              
                   // var projObjId = vault.ObjectPropertyOperations.GetProperty(objVer,projPropId).Value.GetValueAsLookup().Item;
                   // var projObjID = new ObjID();
                   // projObjID.SetIDs(projType,projObjId);
                   // var projObjVer = vault.ObjectOperations.GetLatestObjVer(projObjID, false, false);
                    var projObjVer = GetProjObjVersion(vault).ObjVer;

                    var company = vault.ObjectPropertyOperations.GetProperty(projObjVer, companyPropId).Value.DisplayValue;
                    var projClass = vault.ObjectPropertyOperations.GetProperty(performRateObj.ObjVer, projClassPropId).Value.DisplayValue;
                    var performRate = vault.ObjectPropertyOperations.GetProperty(objVer, performRatePropId).Value.Value;

                    if (projClass == "房地产项目")
                    {
                        list.First(x => x.UnitName == company).RealEstateRate += performRate;
                        list.First(x => x.UnitName == company).RealEstateNum++;
                        list.First(x => x.UnitName == company).UnitRateNum++;
                    }
                    if (projClass == "公建项目")
                    {
                        list.First(x => x.UnitName == company).ComFacilityRate += performRate;
                        list.First(x => x.UnitName == company).ComFacilityNum++;
                        list.First(x => x.UnitName == company).UnitRateNum++;
                    }
                    if (projClass == "基础设施项目")
                    {
                        list.First(x => x.UnitName == company).InfrastructureRate += performRate;
                        list.First(x => x.UnitName == company).InfrastructureNum++;
                        list.First(x => x.UnitName == company).UnitRateNum++;
                    }
                    if (projClass == "融资类项目")
                    {
                        list.First(x => x.UnitName == company).FinancingRate += performRate;
                        list.First(x => x.UnitName == company).FinancingNum++;
                        list.First(x => x.UnitName == company).UnitRateNum++;
                    }
                    if (projClass == "工期变更大的项目")
                    {
                        list.First(x => x.UnitName == company).ChangedProjRate += performRate;
                        list.First(x => x.UnitName == company).ChangedProjNum++;
                        list.First(x => x.UnitName == company).UnitRateNum++;
                    }
                }
                catch { }
            }
           return ArrangePerformList(list);
        }
        /// <summary>
        /// 计算每项平均履约率和单位总履约率
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<PerformanceRateModel> ArrangePerformList(List<PerformanceRateModel> list)
        {
            foreach(PerformanceRateModel model in list)
            {
                var num = 0;
                if(model.RealEstateNum != 0)
                {//房地产项目
                    model.RealEstateRate /=model.RealEstateNum;
                    model.UintRate += model.RealEstateRate;
                    num++;
                }
                if (model.ComFacilityNum != 0)
                {//公建项目
                    model.ComFacilityRate /= model.ComFacilityNum;
                    model.UintRate += model.ComFacilityRate;
                    num++;
                }
                if (model.InfrastructureNum != 0)
                {//基础设施项目
                    model.InfrastructureRate /= model.InfrastructureNum;
                    model.UintRate += model.InfrastructureRate;
                    num++;
                }
                if (model.FinancingNum != 0)
                {//融资类项目
                    model.FinancingRate /= model.FinancingNum;
                    model.UintRate += model.FinancingRate;
                    num++;
                }
                if (model.ChangedProjNum != 0)
                {//工期变更重大的项目
                    model.ChangedProjRate /= model.ChangedProjNum;
                    model.UintRate += model.ChangedProjRate;
                    num++;
                }
                if(num != 0)
                { //公司履约率
                    model.UintRate /= num;
                }
                
            }
            return list;
        }

        /// <summary>
        /// 返回项目的objVersion
        /// </summary>
        /// <returns></returns>
        private ObjectVersion GetProjObjVersion(Vault vault)
        {
            var projTypeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjProject");
            var projClassId = vault.ClassOperations.GetObjectClassIDByAlias("ClassProject");
            var scs = AddBaseConditions(projTypeId, projClassId, false);
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            if (res.Count > 0)
            {
                return res[1];
            }
            return null;
        }
        /// <summary>
        /// 获取履约率对象
        /// </summary>
        /// <param name="vault"></param>
        private ObjectVersion GetPerformRateObj(Vault vault, int year, int month)
        {
            var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjPerformRate");
            var classId = vault.ClassOperations.GetObjectClassIDByAlias("ClassPerformRate");
            var scs = AddBaseConditions(typeId, classId, false);
            //履约率日期  
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            var propId = vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropPerformRateDate");
            var dic = new Dictionary<ObjectVersion,DateTime>();
            foreach(ObjectVersion objVersion in res)
            {
                var objVer = objVersion.ObjVer;
                var date = vault.ObjectPropertyOperations.GetProperty(objVer, propId).Value.DisplayValue;
                var dateArr = date.Split('/');
                if (dateArr[0] == year.ToString() && dateArr[1] == month.ToString())
                {
                    dic.Add(objVersion,objVersion.CreatedUtc);
                }
            }

            if (dic.Count == 0)
            {
                return null;
            }
            else
            {
                var max = dic.Max(x => x.Value);
                return dic.First(y => y.Value == max).Key;
            } 
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

        /// <summary>
        /// 获取公司名
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
        private List<PerformanceRateModel> GetUnitInfo(Vault vault,List<PerformanceRateModel> list)
        {
            //var list = new List<PerformanceRateModel>();
            var vlId = vault.ValueListOperations.GetValueListIDByAlias("VlCompanies");
           // var vl = vault.ValueListOperations.GetValueList(vlId);
            var items = vault.ValueListItemOperations.GetValueListItems(vlId);
            for(int i = 1;i<=items.Count;i++)
            {
                list.Add(new PerformanceRateModel { UnitName = items[i].Name });
            }
            return list;
        }

        /// <summary>
        /// vault登录服务端
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vaultGuid"></param>
        /// <returns></returns>
        private Vault Connect2VaultOnServer(UserDto user, string vaultGuid)
        {
            var serverApp = new MFilesServerApplication();
            serverApp.Connect(user.MfType, user.UserName, user.PassWord, user.Domain, "ncacn_ip_tcp", user.ip);
            Vault gVault = serverApp.LogInToVault(vaultGuid);
            //conn
            return gVault;
        }    
    }
    /// <summary>
    /// 用户登录信息
    /// </summary>
    class UserDto
    {
        public MFAuthType MfType;
        public string UserName;
        public string PassWord;
        public string Domain;
        public string ip;
        public UserDto(MFAuthType oMfType, string oUserName, string oPassWord, string oDomain, string oip)
        {
            MfType = oMfType;
            UserName = oUserName;
            PassWord = oPassWord;
            Domain = oDomain;
            ip = oip;
        }
    }
}
