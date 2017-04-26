using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public static class MFPropertyUtils
    {
        public static PropertyValue SingleFile(bool sf)
        {
            return Create((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
                MFDataType.MFDatatypeBoolean, sf);
        }
        /// <summary>
        /// 类别
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public static PropertyValue Class(int classId)
        {
            return Create((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
                MFDataType.MFDatatypeLookup, classId);
        }
        public static PropertyValue Real(int propDef, double value)
        {
            return Create(propDef, MFDataType.MFDatatypeFloating, value);
        }

        public static PropertyValue Integer(int propDef, int value)
        {
            return Create(propDef, MFDataType.MFDatatypeInteger, value);
        }
        /// <summary>
        /// 单行文本属性
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="content"></param>
        /// <param name="multiLine">是否为多行文本</param>
        /// <returns></returns>
        public static PropertyValue Text(int propDef, string content, bool multiLine = false)
        {
            if (!multiLine)
            {
                return Create(propDef, MFDataType.MFDatatypeText, content);
            }
            return Create(propDef, MFDataType.MFDatatypeMultiLineText, content);
        }
        /// <summary>
        /// 布尔型属性
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static PropertyValue Bool(int propDef, bool value)
        {
            return Create(propDef, MFDataType.MFDatatypeBoolean, value);
        }
        /// <summary>
        /// 单值列表属性
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static PropertyValue Lookup(int propDef, int objId)
        {
            return Create(propDef, MFDataType.MFDatatypeLookup, objId);
        }
        /// <summary>
        /// 多值列表属性
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static PropertyValue Lookups(int propDef, int[] objs)
        {
            return Create(propDef, MFDataType.MFDatatypeMultiSelectLookup, objs);
        }
        /// <summary>
        /// 一般属性
        /// </summary>
        /// <param name="propDef"></param>
        /// <param name="dataType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static PropertyValue Create(int propDef, MFDataType dataType, object value)
        {
            var p = new PropertyValue { PropertyDef = propDef };
            SetValue(p.Value, dataType, value);
            return p;
        }

        public static PropertyValue Create(Vault vault, int propDef, object value)
        {
            var mfPropDef = vault.PropertyDefOperations.GetPropertyDef(propDef);
            var dataType = mfPropDef.DataType;
            var valueListId = mfPropDef.ValueList;
            if (valueListId == 0)
            {
                return Create(propDef, dataType, value);
            }
            var mfObjectType = vault.ValueListOperations.GetValueList(valueListId);
            if (!mfObjectType.RealObjectType)
            {
                var str = value as String;
                if (str != null)
                {
                    var item = MFSearchConditionUtils.GetValueListItem(vault, valueListId, str);
                    return Create(propDef, dataType, item.ID);
                }
            }
            int id = Convert.ToInt32(value);
            return Create(propDef, dataType, id);
        }

        internal static void SetValue(TypedValue tv, MFDataType dataType, object value)
        {
            if (value == null)
            {
                tv.SetValueToNULL(dataType);
            }
            else
            {
                Validate(dataType, value);
                tv.SetValue(dataType, value);
            }
        }

        internal static void Validate(MFDataType dType, object value)
        {
            System.Diagnostics.Debug.Assert(value != null);
            switch (dType)
            {
                case MFDataType.MFDatatypeACL:
                case MFDataType.MFDatatypeInteger:
                case MFDataType.MFDatatypeLookup:
                    if (value is Int32) return;
                    break;
                case MFDataType.MFDatatypeBoolean:
                    if (value is Boolean) return;
                    break;
                case MFDataType.MFDatatypeDate:
                case MFDataType.MFDatatypeFILETIME:
                case MFDataType.MFDatatypeTime:
                case MFDataType.MFDatatypeTimestamp:
                    if (value is DateTime) return;
                    break;
                case MFDataType.MFDatatypeFloating:
                    if (value is Single || value is Double || value is Int32) return;
                    break;
                case MFDataType.MFDatatypeInteger64:
                    if ((value is Int64) || (value is Int32)) return;
                    break;
                case MFDataType.MFDatatypeText:
                case MFDataType.MFDatatypeMultiLineText:
                    if (value is String) return;
                    break;
                case MFDataType.MFDatatypeMultiSelectLookup:
                    if (value is Int32 || value is Int32[]) return;
                    break;
                case MFDataType.MFDatatypeUninitialized:
                    break;

            }
            throw new ArgumentException(string.Format(
                "数据类型({0})与值给出的类型({1})不匹配", dType.ToString(), value.GetType()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pv"></param>
        /// <returns>若未初始化或为null，返回null</returns>
        public static dynamic GetValue(PropertyValue pv)
        {
            if (pv.Value.IsNULL() || pv.Value.IsUninitialized()) return null;
            var dt = pv.Value.DataType;
            if (dt == MFDataType.MFDatatypeText || dt == MFDataType.MFDatatypeMultiLineText)
            {
                return pv.Value.GetValueAsLocalizedText();
            }
            return pv.Value.Value;
        }
    }
}
