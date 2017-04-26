using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace SimulaDesign.RevitBimInfo
{
    public static class ParameterUtils
    {
        //private static string GetParamaterUnit(DisplayUnitType uniType)
        //{
        //    switch (uniType)
        //    {
        //        case DisplayUnitType.DUT_MILLIMETERS:
        //            return "mm";
        //        case DisplayUnitType.DUT_METERS:
        //            return "m";
        //        case DisplayUnitType.DUT_CENTIMETERS:
        //            return "cm";
        //        default:
        //            return string.Empty;
        //    }
        //}

        private static string FormatNumber(Document doc, UnitType unitType, double value)
        {
#if R2014
                    var uStr = FormatUtils.Format(doc, unitType, value);
#else
            var uStr = UnitFormatUtils.Format(doc.GetUnits(), unitType, value, false, false);
#endif
            return uStr;
        }

        public static string ParameterToString(Document doc, Parameter param)
        {
            if (!param.HasValue)
            {
                return "无";
            }
            if (param.Definition.ParameterType == ParameterType.Invalid)
            {
                return "不可用";
            }
            if (doc == null) doc = param.Element.Document;
            switch (param.StorageType)
            {
                case StorageType.Double:
                    var uStr = FormatNumber(doc, param.Definition.UnitType, param.AsDouble());
                    return uStr;
                    //var uStr = string.Empty;
                    //if (param.Definition.ParameterType == ParameterType.Length)
                    //{
                    //    uStr = GetParamaterUnit(param.DisplayUnitType);
                    //}
                    //var dStr = param.AsValueString();
                    //if (!String.IsNullOrEmpty(uStr) && !dStr.EndsWith(uStr)) dStr += uStr;
                    //return dStr;
                case StorageType.Integer:
                    var v = param.AsInteger();
                    if (param.Definition.ParameterType == ParameterType.YesNo)
                    {
                        if (v == 0) return "否";
                        return "是";
                    }
                    return FormatNumber(doc, param.Definition.UnitType, v);
                case StorageType.String:
                    return param.AsString();
                case StorageType.ElementId:
                    ElementId idVal = param.AsElementId();
                    return AsElementName(doc, idVal);
                case StorageType.None:
                default:
                    return "无";
            }
        }

        public static string ParameterToString(Document doc, FamilyParameter param, FamilyType type)
        {
            if (!type.HasValue(param))
            {
                return "无";
            }
            if (param.Definition.ParameterType == ParameterType.Invalid)
            {
                return "不可用";
            }
            switch (param.StorageType)
            {
                case StorageType.Double:
#if R2014
                    var uStr = FormatUtils.Format(doc, param.Definition.UnitType, type.AsDouble(param).Value);
#else
                    var uStr = UnitFormatUtils.Format(doc.GetUnits(), param.Definition.UnitType, type.AsDouble(param).Value, false, false);
#endif
                    return uStr;
                //var uStr = string.Empty;
                //if (param.Definition.ParameterType == ParameterType.Length)
                //{
                //    uStr = GetParamaterUnit(param.DisplayUnitType);
                //}
                //var dStr = param.AsValueString();
                //if (!String.IsNullOrEmpty(uStr) && !dStr.EndsWith(uStr)) dStr += uStr;
                //return dStr;
                case StorageType.Integer:
                    var v = type.AsInteger(param).Value;
                    if (param.Definition.ParameterType == ParameterType.YesNo)
                    {
                        if (v == 0) return "否";
                        return "是";
                    }
#if R2014
                    return FormatUtils.Format(doc, param.Definition.UnitType, v);
#else
                    return UnitFormatUtils.Format(doc.GetUnits(), param.Definition.UnitType, v, false, false);
#endif
                case StorageType.String:
                    return type.AsString(param);
                case StorageType.ElementId:
                    ElementId idVal = type.AsElementId(param);
                    return AsElementName(doc, idVal);
                case StorageType.None:
                default:
                    return "无";
            }
        }

        private static string AsElementName(Document doc, ElementId idVal)
        {
            if (idVal.Compare(ElementId.InvalidElementId) == 0)
            {
                return Convert.ToString(idVal.IntegerValue);
            }
            var ele = ElementFilterUtils.GetElement(doc, idVal);
            if (ele == null) return null;
            return ele.Name;
        }
    }
}
