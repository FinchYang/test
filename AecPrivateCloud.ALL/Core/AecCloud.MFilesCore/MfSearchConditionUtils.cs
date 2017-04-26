using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public class MFSearchConditionUtils
    {
        public static ObjectSearchResults SearchObjectById(Vault vault, int objType,
            int objId, bool deleted = false)
        {
            if (objId < 0) throw new ArgumentException("对象ID必须非负");
            var scs = new SearchConditions();
            scs.Add(-1, ObjType(objType));
            scs.Add(-1, ObjId(objId));
            scs.Add(-1, Deleted(deleted));
            return vault.ObjectSearchOperations.SearchForObjectsByConditions(
                scs, MFSearchFlags.MFSearchFlagNone, false);
        }
        public static ObjectSearchResults SearchObjectsByType(Vault vault, int objType, bool deleted = false)
        {
            var scs = new SearchConditions();
            AddBaseConditions(scs, objType, null, deleted);
            return vault.ObjectSearchOperations.SearchForObjectsByConditions(
                scs, MFSearchFlags.MFSearchFlagNone, false);
        }
        public static ObjectSearchResults SearchObjectsByClass(Vault vault, int objType,
            int classId, bool deleted = false)
        {
            var scs = new SearchConditions();
            AddBaseConditions(scs, objType, classId, deleted);
            return vault.ObjectSearchOperations.SearchForObjectsByConditions(
                scs, MFSearchFlags.MFSearchFlagNone, false);
        }

        public static ObjectSearchResults SearchObjects(Vault vault, int objType,
            int? classId, SearchConditions otherConditions, bool deleted = false)
        {
            var scs = otherConditions != null ? otherConditions.Clone() : new SearchConditions();
            AddBaseConditions(scs, objType, classId, deleted);
            return vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(
                scs, MFSearchFlags.MFSearchFlagNone, false, 0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scs">搜索条件的列表</param>
        /// <param name="objType">对象类型ID</param>
        /// <param name="classId">类别ID</param>
        /// <param name="deleted">是否对象被删除</param>
        public static void AddBaseConditions(SearchConditions scs, int objType,
            int? classId=null, bool deleted = false)
        {
            scs.Add(-1, ObjType(objType));
            if (classId != null) scs.Add(-1, Class(classId.Value));
            scs.Add(-1, Deleted(deleted));
        }
        /// <summary>
        /// 类别搜索
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public static SearchCondition Class(int classId)
        {
            return Property(MFConditionType.MFConditionTypeEqual,
                (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
                MFDataType.MFDatatypeLookup, classId);
        }
        /// <summary>
        /// 对象是否删除搜索
        /// </summary>
        /// <param name="deleted"></param>
        /// <returns></returns>
        public static SearchCondition Deleted(bool deleted)
        {
            return Status(MFConditionType.MFConditionTypeEqual,
                MFStatusType.MFStatusTypeDeleted, MFDataType.MFDatatypeBoolean, deleted);
        }
        /// <summary>
        /// 对象类型搜索
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static SearchCondition ObjType(int objType)
        {
            return Status(MFConditionType.MFConditionTypeEqual,
                MFStatusType.MFStatusTypeObjectTypeID, MFDataType.MFDatatypeLookup, objType);
        }
        /// <summary>
        /// 对象ID搜索
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static SearchCondition ObjId(int objId)
        {
            if (objId < 0) throw new ArgumentException("对象ID必须非负");
            return Status(MFConditionType.MFConditionTypeEqual,
                MFStatusType.MFStatusTypeOriginalObjectID, MFDataType.MFDatatypeInteger, objId);
        }
        /// <summary>
        /// 其中之一的搜索条件
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static SearchCondition OneOf(int propDef, int item)
        {
            if (item < 0) throw new ArgumentException("对象ID必须非负");
            var sc = new SearchCondition();
            sc.ConditionType = MFConditionType.MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = propDef;
            MFPropertyUtils.SetValue(sc.TypedValue, MFDataType.MFDatatypeMultiSelectLookup, item);
            return sc;
        }

        /// <summary>
        /// 对象属性搜索
        /// </summary>
        /// <param name="cType"></param>
        /// <param name="propDef"></param>
        /// <param name="dType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SearchCondition Property(MFConditionType cType,
            int propDef, MFDataType dType, object value)
        {
            var sc = new SearchCondition();
            sc.ConditionType = cType;
            sc.Expression.DataPropertyValuePropertyDef = propDef;
            MFPropertyUtils.SetValue(sc.TypedValue, dType, value);
            return sc;
        }
        /// <summary>
        /// 对象状态搜索
        /// </summary>
        /// <param name="cType"></param>
        /// <param name="sType"></param>
        /// <param name="dType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SearchCondition Status(MFConditionType cType,
            MFStatusType sType, MFDataType dType, object value)
        {
            var sc = new SearchCondition();
            sc.ConditionType = cType;
            sc.Expression.DataStatusValueType = sType;
            MFPropertyUtils.SetValue(sc.TypedValue, dType, value);
            return sc;
        }

        public static ValueListItem GetValueListItem(Vault vault, int valueList,
            string itemName, bool throwOnMoreThanOne = true)
        {
            var nameSearch = new SearchCondition();
            nameSearch.Expression.SetValueListItemExpression(
                MFValueListItemPropertyDef.MFValueListItemPropertyDefName,
                MFParentChildBehavior.MFParentChildBehaviorNone);
            nameSearch.ConditionType = MFConditionType.MFConditionTypeEqual;
            nameSearch.TypedValue.SetValue(MFDataType.MFDatatypeText, itemName);

            var noDeleteSearch = new SearchCondition();
            noDeleteSearch.Expression.SetValueListItemExpression(
                MFValueListItemPropertyDef.MFValueListItemPropertyDefDeleted,
                MFParentChildBehavior.MFParentChildBehaviorNone);
            noDeleteSearch.ConditionType = MFConditionType.MFConditionTypeEqual;
            noDeleteSearch.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);

            var searchs = new SearchConditions();
            searchs.Add(-1, nameSearch);
            searchs.Add(-1, noDeleteSearch);
            var res = vault.ValueListItemOperations.SearchForValueListItemsEx2(
                valueList, searchs); //, false, MFExternalDBRefreshType.MFExternalDBRefreshTypeNone, false
            if (res.Count == 0) return null;
            if (res.Count == 1) return res[1];
            if (throwOnMoreThanOne)
            {
                throw new Exception("值列表有重名");
            }
            return res[1];
        }
    }
}
