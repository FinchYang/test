using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MFModelUtil
    {
        /// <summary>
        /// 获取单体
        /// </summary>
        public static IEnumerable<ObjInfo> GetUnits(Vault vault)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjModelUnit");
            var objs = SearchObjectsByType(vault, typeId);
            var res = new List<ObjInfo>();
            for (var i = 1; i <= objs.Count; i++) {
                var obj = objs[i];
                res.Add(ToObjInfo(obj, 1));
            }
           var units =  (from ObjInfo o in res orderby o.ID select o).ToList();
           foreach (ObjInfo o in units)
           {
               o.Model = SearchModel(vault, o);
           }
           return units;
        }
        /// <summary>
        /// 获取楼层：parentId为父对象单体 ID
        /// </summary>
        public static IEnumerable<ObjInfo> GetFloors(Vault vault, int parentId)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjFloor");
            var parentTypeId = MfAlias.GetObjType(vault, "ObjModelUnit");

            var floors = SearchObjsByParent(vault, typeId, parentTypeId, parentId,2);
            foreach (ObjInfo o in floors)
            {
                o.Model = SearchModel(vault, o);
            }
            return floors;
        }
        /// <summary>
        /// 获取专业：parentId为父对象楼层 ID
        /// </summary>
        public static IEnumerable<ObjInfo> GetDisciplines(Vault vault, int parentId)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjModelDiscipline");
            var parentTypeId = MfAlias.GetObjType(vault, "ObjFloor");

            var majors = SearchObjsByParent(vault, typeId, parentTypeId, parentId,3);
            foreach (ObjInfo o in majors)
            {
                o.Model = SearchModel(vault, o);
            }
            return majors;
        }
        private static Model SearchModel(Vault vault, ObjInfo owner)
        {
            int classIdModel = MfAlias.GetObjectClass(vault, "ClassBimModelDoc");
            int propIdUnit = MfAlias.GetPropDef(vault, "PropModelUnitAt");
            int propIdFloor = MfAlias.GetPropDef(vault, "PropFloorAt");
            int propIdMajor = MfAlias.GetPropDef(vault, "PropDisciplineAt");
           
            var sConditons = new SearchConditions();
            var scClass = new SearchCondition();
            scClass.ConditionType = MFConditionType.MFConditionTypeEqual;
            scClass.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            scClass.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classIdModel);
            sConditons.Add(-1, scClass);

            if (owner.Level == 1) {
                sConditons.Add(-1, LookupProperty(propIdUnit, owner.ID));
                sConditons.Add(-1, LookupProperty(propIdFloor, 0));
            }
            else if (owner.Level == 2) {
                sConditons.Add(-1, LookupProperty(propIdFloor, owner.ID));
                sConditons.Add(-1, LookupProperty(propIdMajor, 0));
            }
            else if (owner.Level == 3) {
                sConditons.Add(-1, LookupProperty(propIdMajor, owner.ID));
            }
            else
            {
                return null;
            }
            //所在...
            var objVns = SearchObjects(vault, 0, sConditons);
            if (objVns.Count == 0) return null;
            var objVn = objVns[1];
            return new Model
            {
                ID = objVn.ObjVer.ID,
                Type = objVn.ObjVer.Type,
                Title = objVn.Title
            };
        }
        private static SearchCondition LookupProperty(int propDef, int val)
        {
            var sc = new SearchCondition();
            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = propDef;
            if (val > 0) sc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, val);
            else sc.TypedValue.SetValueToNULL(MFDataType.MFDatatypeLookup);
            return sc;
        }
        private static IEnumerable<ObjInfo> SearchObjsByParent(Vault vault, int type, int parentType, int parentId, int level)
        {
            var propIdOwner = vault.ObjectTypeOperations.GetObjectType(parentType).OwnerPropertyDef;

            var sConditons = new SearchConditions();
            //所属对象
            var conditionOwner = new SearchCondition();
            conditionOwner.ConditionType = MFConditionType.MFConditionTypeEqual;
            conditionOwner.Expression.DataPropertyValuePropertyDef = propIdOwner;
            conditionOwner.TypedValue.SetValue(MFDataType.MFDatatypeLookup, parentId);
            sConditons.Add(-1, conditionOwner);
            var res = new List<ObjInfo>();
            try {
                var objs = SearchObjects(vault, type, sConditons);
 
                for (var i = 1; i <= objs.Count; i++) {
                    var obj = objs[i];
                    res.Add(ToObjInfo(obj, level, parentId));
                }
                return (from ObjInfo o in res orderby o.ID select o).ToList();
            }
            catch(Exception ex) {
                throw new Exception("通过父类型搜索失败："+parentType + " # " + ex.Message);
            }
        }
        private static ObjectVersions SearchObjectsByType(Vault vault, int typeId)
        {
            return SearchObjects(vault, typeId);
        }
        private static ObjectVersions SearchObjects(Vault vault, int typeId, SearchConditions searchConditions=null)
        {
            SearchConditions scs = searchConditions == null ? new SearchConditions() : searchConditions.Clone();
            //对象类型
            var conditionType = new SearchCondition();
            conditionType.ConditionType = MFConditionType.MFConditionTypeEqual;
            conditionType.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            conditionType.TypedValue.SetValue(MFDataType.MFDatatypeLookup, typeId);
            scs.Add(-1, conditionType);
            //是否删除
            var conditionDeleted = new SearchCondition();
            conditionDeleted.ConditionType = MFConditionType.MFConditionTypeEqual;
            conditionDeleted.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            conditionDeleted.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, conditionDeleted);
            var sResults = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            return sResults.GetAsObjectVersions();
        }
        private static ObjectVersions SearchObjectsByClass(Vault vault, int classId, SearchConditions searchConditions = null)
        {
            SearchConditions scs = searchConditions == null ? new SearchConditions() : searchConditions.Clone();
            //对象类别
            var conditionClass = new SearchCondition();
            conditionClass.ConditionType = MFConditionType.MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = 100;
            conditionClass.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            scs.Add(-1, conditionClass);
            //是否删除
            var conditionDeleted = new SearchCondition();
            conditionDeleted.ConditionType = MFConditionType.MFConditionTypeEqual;
            conditionDeleted.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            conditionDeleted.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, conditionDeleted);
            var sResults = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            return sResults.GetAsObjectVersions();
        }

        private static ObjInfo ToObjInfo(ObjectVersion objVn, int level = 0, int parent = -1)
        {
            return new ObjInfo(level, parent)
                   {
                       ID = objVn.ObjVer.ID,
                       Type = objVn.ObjVer.Type,
                       Title = objVn.Title
                   };
        }

        public static IEnumerable<Qa> GetQaList(Vault vault, string classAlias, string state)
        {
            var res = new List<Qa>();
            int classId = MfAlias.GetObjectClass(vault, classAlias);
            int pIdVerdic = MfAlias.GetPropDef(vault, "PropRectificationConclusion", false);//整改意见
            int pIdVerdic2 = MfAlias.GetPropDef(vault, "PropVerdicQ", false);//整改意见-质量整改通知单
            int pIdFeedback = MfAlias.GetPropDef(vault, "PropFeedback", false);//反馈意见
            int pIdViewPort = MfAlias.GetPropDef(vault, "PropViewPortParams", false); //模型视口参数
            int partClass = MfAlias.GetObjectClass(vault, "ClassPart");
            //int pId = MfAlias.GetPropDef(vault, "PropID");
            int pIfcId = MfAlias.GetPropDef(vault, "PropIfcId");
            int pModel = MfAlias.GetPropDef(vault, "PropOwnedModel");
            var objVns = SearchObjectsByClass(vault, classId);
            foreach (ObjectVersion o in objVns)
            {
                var partVns = GetRelativeParts(vault, o.ObjVer, partClass).ToList();
                if (partVns.Count == 0) continue;
                var qa = new Qa();
                var qaProps = vault.ObjectPropertyOperations.GetProperties(o.ObjVer);
                if (qaProps.IndexOf(pIdVerdic) != -1)
                {
                    qa.Verdict = qaProps.SearchForProperty(pIdVerdic).Value.DisplayValue;
                }
                if (qaProps.IndexOf(pIdVerdic2) != -1)
                {
                    qa.Verdict = qaProps.SearchForProperty(pIdVerdic2).Value.DisplayValue;
                }
                if (qaProps.IndexOf(pIdFeedback) != -1)
                {
                    qa.Verdict = qaProps.SearchForProperty(pIdFeedback).Value.DisplayValue;
                }
                if (qaProps.IndexOf(39) != -1)
                {
                    qa.FlowState = qaProps.SearchForProperty(39).Value.DisplayValue;
                }
                if (qaProps.IndexOf(pIdViewPort) != -1)
                {
                    qa.ViewPort = qaProps.SearchForProperty(pIdViewPort).Value.DisplayValue;
                }
                qa.ID = o.ObjVer.ID;
                qa.Type = o.ObjVer.Type;
                qa.Title = o.Title;
                qa.Url = vault.ObjectOperations.GetMFilesURLForObjectOrFile(o.ObjVer.ObjID, -1, false, -1,
                    MFilesURLType.MFilesURLTypeShow);
                var parts = new List<Part>();
                //构件详情
                foreach (ObjectVersion p in partVns)
                {
                    var ptProps = vault.ObjectPropertyOperations.GetProperties(p.ObjVer);
                    var pt = new Part {Title = p.Title};
                    //pt.Id = ptProps.SearchForProperty(pId).Value.DisplayValue;
                    pt.IfcId = ptProps.SearchForProperty(pIfcId).Value.DisplayValue;
                    var mTvalue = ptProps.SearchForProperty(pModel).Value;
                    if (!mTvalue.IsNULL())
                    {
                        var mLookup = mTvalue.GetValueAsLookup();
                        if (!mLookup.Deleted && !mLookup.Hidden)
                        {
                            pt.Model = new Model()
                            {
                                ID = mLookup.Item,
                                Type = mLookup.ObjectType,
                                Title = mLookup.DisplayValue
                            };
                        }
                    }
                    parts.Add(pt);
                }
                qa.Parts = parts;
                res.Add(qa);
            }
            return res;
        }
        //获取相关构件
        private static IEnumerable<ObjectVersion> GetRelativeParts(Vault vault, ObjVer objVer, int desClass)
        {
            var rs = vault.ObjectOperations.GetRelationships(objVer, MFRelationshipsMode.MFRelationshipsModeFromThisObject);
            var res = (from ObjectVersion r in rs 
                       where r.Deleted == false && r.Class == desClass 
                       select r).ToList();
            return res;
        }
    }

    public class ObjInfo
    {
        public ObjInfo(int level, int parent)
        {
            Level = level;
            Parent = parent;
        }
        public string Title { get; set; }
        public int Type { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// 层
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 父对象，上一层
        /// </summary>
        public int Parent { get; set; }
        public bool HasModel { get; set; }
        public Model Model { get; set; }
    }
    public class Model
    {
        public string Title { get; set; }
        public int Type { get; set; }
        public int ID { get; set; }
    }

    public class Qa
    {
        public string Title { get; set; }
        public int Type { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// 流程节点
        /// </summary>
        public string FlowState { get; set; }
        /// <summary>
        /// 模型视口参数
        /// </summary>
        public string ViewPort { get; set; }
        /// <summary>
        /// 整改结论
        /// </summary>
        public string Verdict { get; set; }
        public string Url { get; set; }
        public IEnumerable<Part> Parts { get; set; } 
    }
    //构件
    public class Part
    {
        public string Title { get; set; }
        /// <summary>
        /// External ID??
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// IFC标识
        /// </summary>
        public string IfcId { get; set; }
        public Model Model { get; set; }
    }
}
