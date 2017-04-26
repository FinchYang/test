//using System;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using MFiles.VAF.Common;
//using MFilesAPI;

//namespace VaultApp
//{
//    public class FlowNumberOp
//    {
//        /// <summary>
//        /// 只能通过BeforeCheckinChanges(修改属性后触发)以及BeforeCreateNewObjectFinalize事件修改对象的属性
//        /// </summary>
//        /// <param name="env"></param>
//        public static void SetAutomaticFlowNumber(EventHandlerEnvironment env)
//        {
//            if (env.ObjVer.Type != (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument)
//            {
//                return;
//            }

//            var flowNumberOp = new FlowNumberOp(env.Vault, env.ObjVer, env.PropertyValues, env.CurrentUserID, env.GetObjectClass(), env.EventType);
//            flowNumberOp.SetFlowNumber();
//        }

//        private MFEventHandlerType _eventType;

//        /// <summary>
//        /// 项目
//        /// </summary>
//        string projPropAlias = "PropProjectName";
//        /// <summary>
//        /// 分区
//        /// </summary>
//        string areaPropAlias = "PropArea";
//        /// <summary>
//        /// 公司
//        /// </summary>
//        string companyAlias = "PropCompanyName";

//        /// <summary>
//        /// 自动生成的流水号
//        /// </summary>
//        string flowNumberAlias = "PropFlowNumber";
//        /// <summary>
//        /// 自定义流水号
//        /// </summary>
//        string flowNumberIntAlias = "PropFlowNumberInt";

//        /// <summary>
//        /// 阶段
//        /// </summary>
//        string ProjectStageAlias = "PropProjectStage";
//        /// <summary>
//        /// 专业
//        /// </summary>
//        string specialtyPropAlias = "PropSpecialty";
//        /// <summary>
//        /// 收件单位
//        /// </summary>
//        string PropDefReceiveGroup = "PropReceiveGroup";

//        /// <summary>
//        /// 总包分包
//        /// </summary>
//        private string ContractorOrSubcontractorAlias = "PropContractorOrSubcontractor";
//        /// <summary>
//        /// 简称的分隔符
//        /// </summary>
//        private static readonly char FlowSep = '_';

//        private Vault vault;
//        private ObjVer objVer;
//        private PropertyValues pvs;
//        private int userId;
//        private int currentClassId;
//        public FlowNumberOp(Vault vault, ObjVer objVer, PropertyValues pvs, int userId, int objClass, MFEventHandlerType eventType)
//        {
//            this.vault = vault;
//            this.objVer = objVer;
//            this.pvs = pvs;
//            this.userId = userId;
//            this.currentClassId = objClass;
//            _eventType = eventType;
//        }

//        public void SetFlowNumber()
//        {
//            var gotProps = true;
//            if (pvs == null)
//            {
//                gotProps = false;
//                pvs = vault.ObjectPropertyOperations.GetProperties(objVer, false);
//            }

//            var flowNumberId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(flowNumberAlias);
//            var flowNumberIntPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(flowNumberIntAlias);
//            var pv = pvs.SearchForPropertyEx(flowNumberId, true);
//            if (pv == null) //无流水号属性
//            {
//                return;
//            }

//            var outStr = String.Empty;

//            var flowNumberIntProp = pvs.SearchForPropertyEx(flowNumberIntPropId, true);
//            if (!pv.Value.IsNULL()) //若已存在生成的流水号
//            {
//                if (flowNumberIntProp == null || flowNumberIntProp.Value.IsNULL()) //若无自定义流水号
//                {
//                    return;
//                }
//                var customNumber = Int32.Parse(flowNumberIntProp.GetValueAsLocalizedText());
//                var strs = pv.GetValueAsLocalizedText().Split(FlowNumberObject.Sep.ToArray());
//                var oldNumber = Int32.Parse(strs[strs.Length - 1].TrimStart('0'));
//                if (customNumber != oldNumber) //若自定义流水号跟已生成的流水号不一致
//                {
//                    var num = GetNumberStr(customNumber);
//                    outStr = String.Join(FlowNumberObject.Sep, strs.Take(strs.Length - 1).Concat(new[] { num }));
//                }
//            }
//            else //还未生成过流水号
//            {
//                var objClass = vault.ClassOperations.GetObjectClass(currentClassId);
//                var className = objClass.Name;

//                var projPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(projPropAlias);
//                var areaPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(areaPropAlias);
//                var companyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(companyAlias);


//                var stagePropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(ProjectStageAlias);
//                var specialtyPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(specialtyPropAlias);
//                var contractorPropId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(ContractorOrSubcontractorAlias);

//                var propReceiveGroupId = vault.PropertyDefOperations.GetPropertyDefIDByAlias(PropDefReceiveGroup);

//                var classStr = GetFlowNumber(className);

//                var flowNumber = "";
//                var projStr = "";
//                var areaStr = "";
//                var companyStr = "";
//                var stageStr = "";
//                var specialtyStr = "";

//                var count = 0;

//                //获取项目简称
//                var projProp = pvs.SearchForPropertyEx(projPropId, true);
//                if (projProp != null && !projProp.Value.IsNULL())
//                {
//                    projStr = GetFlowNumber(projProp.GetValueAsLocalizedText());
//                }

//                //获取公司和分区简称
//                var receiveGroupPV = pvs.SearchForPropertyEx(propReceiveGroupId, true);
//                if (receiveGroupPV != null && !receiveGroupPV.Value.IsNULL()) //从收件单位获取
//                {
//                    var receiveGroupStr = receiveGroupPV.Value.DisplayValue;
//                    var ta = receiveGroupStr.Split(new char[] { FlowSep });
//                    companyStr = ta[ta.Length - 1];
//                    areaStr = ta[ta.Length - 2];
//                }
//                else //从当前用户的用户组获取
//                {
//                    GetInfoFromGroup(vault, userId, out companyStr, out areaStr);
//                }
//                //通过公司属性获取公司
//                var companyPV = pvs.SearchForPropertyEx(companyPropId, true);
//                if (companyPV != null && !companyPV.Value.IsNULL())
//                {
//                    var cc = companyPV.GetValueAsLocalizedText();
//                    var cStrs = cc.Split(new char[] { FlowSep });
//                    companyStr = cStrs[cStrs.Length - 1];
//                }
//                //从分区属性获取分区
//                var areaPV = pvs.SearchForPropertyEx(areaPropId, true);
//                if (areaPV != null && !areaPV.Value.IsNULL())
//                {
//                    var aa = areaPV.GetValueAsLocalizedText();
//                    var aStrs = aa.Split(new char[] { FlowSep });
//                    areaStr = aStrs[aStrs.Length - 1];
//                }
//                //给分区添加总包和分包的信息
//                var contractorPV = pvs.SearchForPropertyEx(contractorPropId, true);
//                if (contractorPV != null && !contractorPV.Value.IsNULL())
//                {
//                    var cc = contractorPV.GetValueAsLocalizedText();
//                    var cStrs = cc.Split(new char[] { FlowSep });
//                    areaStr += cStrs[cStrs.Length - 1];
//                }
//                //阶段简称
//                var stageProp = pvs.SearchForPropertyEx(stagePropId, true);
//                if (stageProp != null && !stageProp.Value.IsNULL())
//                {
//                    stageStr = GetFlowNumber(stageProp.GetValueAsLocalizedText());
//                }
//                //专业简称
//                var specialtyProp = pvs.SearchForPropertyEx(specialtyPropId, true);
//                if (specialtyProp != null && !specialtyProp.Value.IsNULL())
//                {
//                    specialtyStr = GetFlowNumber(specialtyProp.GetValueAsLocalizedText());
//                }

//                var flowObj = new FlowNumberObject
//                {
//                    Project = projStr,
//                    Class = classStr,
//                    Area = areaStr,
//                    Company = companyStr,
//                    Stage = stageStr,
//                    Speciality = specialtyStr
//                };

//                var numberPattern = flowObj.GetSearchPattern();

//                //获取最近的流水号

//                if (flowNumberIntProp != null && !flowNumberIntProp.Value.IsNULL())
//                {
//                    count = Int32.Parse(flowNumberIntProp.GetValueAsLocalizedText());
//                }
//                else
//                {
//                    count = GetPreviewsWorkFlowNum(vault, flowNumberId,
//                            GetHistories(vault, currentClassId, flowNumberId, numberPattern)) + 1;
//                }

//                flowNumber = GetNumberStr(count);

//                flowObj.CustomNumber = flowNumber;

//                outStr = flowObj.ToString();
//            }


//            if (!String.IsNullOrEmpty(outStr))
//            {
//                var pv0 = new PropertyValue { PropertyDef = flowNumberId };
//                pv0.Value.SetValue(MFDataType.MFDatatypeText, outStr);
//                if (gotProps && (_eventType == MFEventHandlerType.MFEventHandlerBeforeSetProperties
//                    || _eventType == MFEventHandlerType.MFEventHandlerAfterSetProperties)) //设置属性事件无法设置此属性
//                {
//                    pvs.SetProperty(pv0);
//                }
//                else
//                {
//                    //throw new Exception("set PropertyValue from vault: " + outStr);
//                    vault.ObjectPropertyOperations.SetProperty(objVer, pv0); //MFEventHandlerBeforeCreateNewObjectFinalize
//                }
//            }

//        }


//        private static int GetPreviewsWorkFlowNum(Vault vault, int flowNumberId, ObjectSearchResults count)
//        {
//            var num = 0;
//            foreach (ObjectVersion ov in count)
//            {
//                var objVer = ov.ObjVer;
//                var pvs = vault.ObjectPropertyOperations.GetProperties(objVer);
//                var pv = pvs.SearchForPropertyEx(flowNumberId, true);
//                if (pv != null && !pv.Value.IsNULL())
//                {
//                    var flowNumberString = pv.GetValueAsLocalizedText();
//                    var tempFlowStr = GetFlowNumberFromObject(flowNumberString).TrimStart('0');
//                    var number = Int32.Parse(tempFlowStr);
//                    if (number > num)
//                    {
//                        num = number;
//                    }
//                }
//            }
//            return num;
//        }
//        /// <summary>
//        /// 前位补0
//        /// </summary>
//        /// <param name="count"></param>
//        /// <returns></returns>
//        private static string GetNumberStr(int count)
//        {
//            return String.Format("{0:000}", count);
//        }
//        /// <summary>
//        /// 通过用户组获取用户所在的公司和分区
//        /// </summary>
//        /// <param name="vault"></param>
//        /// <param name="userId"></param>
//        /// <param name="companyStr">公司编码</param>
//        /// <param name="areaStr">分区编码</param>
//        /// <returns></returns>
//        private static bool GetInfoFromGroup(Vault vault, int userId, out string companyStr, out string areaStr)
//        {
//            companyStr = null;
//            areaStr = null;
//            var ugs = vault.UserGroupOperations.GetGroupsOfUserOrGroup(userId, false);
//            foreach (UserGroup ug in ugs)
//            {
//                var ugName = ug.Name;
//                var ok = Regex.IsMatch(ugName, "^[南,北]区");
//                if (ok)
//                {
//                    var splitName = ugName.Split(new char[] { FlowSep });
//                    companyStr = splitName[splitName.Length - 1];
//                    areaStr = splitName[splitName.Length - 2];
//                    return true;
//                }
//            }
//            return false;
//        }
//        /// <summary>
//        /// 获取自动生成的流水号
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        private static string GetFlowNumberFromObject(string str)
//        {
//            var strs = str.Split(FlowNumberObject.Sep.ToArray());
//            return strs[strs.Length - 1];
//        }

//        /// <summary>
//        /// 搜索类似流水号的对象
//        /// </summary>
//        /// <param name="vault"></param>
//        /// <param name="classId"></param>
//        /// <param name="flowNumberDef"></param>
//        /// <param name="flowNumberPattern"></param>
//        /// <returns></returns>
//        private static ObjectSearchResults GetHistories(Vault vault, int classId, int flowNumberDef, string flowNumberPattern)
//        {
//            var scs = new SearchConditions();

//            var classSc = new SearchCondition
//            {
//                ConditionType = MFConditionType.MFConditionTypeEqual
//            };
//            classSc.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
//            classSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
//            scs.Add(-1, classSc);

//            var flowSc = new SearchCondition
//            {
//                ConditionType = MFConditionType.MFConditionTypeMatchesWildcardPattern
//            };
//            flowSc.Expression.DataPropertyValuePropertyDef = flowNumberDef;
//            flowSc.TypedValue.SetValue(MFDataType.MFDatatypeText, flowNumberPattern);
//            scs.Add(-1, flowSc);

//            //var idSc = new SearchCondition
//            //{
//            //    ConditionType = MFConditionType.MFConditionTypeNotEqual
//            //};
//            //idSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectID;
//            //idSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, excludeId);
//            //scs.Add(-1, idSc);


//            //var sb = new StringBuilder();
//            //sb.Append("ClassId: " + classId + "\r\n");
//            //sb.Append("Pattern: " + flowNumberPattern + "\r\n");
//            //sb.Append("Exclude: " + excludeId);
//            //throw new Exception(sb.ToString());
//            try
//            {
//                return
//                    vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone, false);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(flowNumberPattern + ": " + ex.Message, ex);
//            }

//        }
//        /// <summary>
//        /// 获取名称里的简称
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        private static string GetFlowNumber(string str)
//        {
//            var strs = str.Split(new char[] { FlowSep });
//            return strs[strs.Length - 1];
//        }
//    }

//    public class FlowNumberObject
//    {
//        public static readonly string Sep = "-";
//        /// <summary>
//        /// 1 项目简称
//        /// </summary>
//        public string Project { get; set; }
//        /// <summary>
//        /// 2 类别简称
//        /// </summary>
//        public string Class { get; set; }
//        /// <summary>
//        /// 3 分区简称
//        /// </summary>
//        public string Area { get; set; }
//        /// <summary>
//        /// 4 公司简称
//        /// </summary>
//        public string Company { get; set; }
//        /// <summary>
//        /// 5 阶段简称
//        /// </summary>
//        public string Stage { get; set; }
//        /// <summary>
//        /// 6 专业简称
//        /// </summary>
//        public string Speciality { get; set; }
//        /// <summary>
//        /// 5 流水号
//        /// </summary>
//        public string CustomNumber { get; set; }

//        /// <summary>
//        /// 除了最后流水号的前缀
//        /// </summary>
//        /// <returns></returns>
//        private string GetPrefix()
//        {
//            var sb = new StringBuilder();
//            sb.Append(Project + Sep + Class);
//            if (!String.IsNullOrEmpty(Area))
//            {
//                sb.Append(Sep + Area);
//            }
//            if (!String.IsNullOrEmpty(Stage))
//            {
//                sb.Append(Sep + Stage);
//            }
//            if (!String.IsNullOrEmpty(Speciality))
//            {
//                sb.Append(Sep + Speciality);
//            }
//            return sb.ToString();
//        }
//        /// <summary>
//        /// 相同前缀流水号的搜索字符串
//        /// </summary>
//        /// <returns></returns>
//        public string GetSearchPattern()
//        {
//            return GetPrefix() + Sep + "*";
//        }

//        public override string ToString()
//        {
//            return GetPrefix() + Sep + CustomNumber;
//        }
//    }
//}
