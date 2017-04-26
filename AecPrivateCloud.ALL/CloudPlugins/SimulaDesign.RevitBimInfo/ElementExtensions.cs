using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using SimulaDesign.BimInfo;
using Element = SimulaDesign.BimInfo.Element;
using ElementType = SimulaDesign.BimInfo.ElementType;

namespace SimulaDesign.RevitBimInfo
{
    public static class ElementExtensions
    {
        static string s_ConversionTable_2X = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_$";

        static private string ConvertToIFCGuid(Guid guid)
        {
            byte[] byteArray = guid.ToByteArray();
            ulong[] num = new ulong[6];
            num[0] = byteArray[3];
            num[1] = byteArray[2] * (ulong)65536 + byteArray[1] * (ulong)256 + byteArray[0];
            num[2] = byteArray[5] * (ulong)65536 + byteArray[4] * (ulong)256 + byteArray[7];
            num[3] = byteArray[6] * (ulong)65536 + byteArray[8] * (ulong)256 + byteArray[9];
            num[4] = byteArray[10] * (ulong)65536 + byteArray[11] * (ulong)256 + byteArray[12];
            num[5] = byteArray[13] * (ulong)65536 + byteArray[14] * (ulong)256 + byteArray[15];

            char[] buf = new char[22];
            int offset = 0;

            for (int ii = 0; ii < 6; ii++)
            {
                int len = (ii == 0) ? 2 : 4;
                for (int jj = 0; jj < len; jj++)
                {
                    buf[offset + len - jj - 1] = s_ConversionTable_2X[(int)(num[ii] % 64)];
                    num[ii] /= 64;
                }
                offset += len;
            }

            return new string(buf);
        }
        public static string GetIFCGuid(this Autodesk.Revit.DB.Element e)
        {
            var guid = ExportUtils.GetExportId(e.Document, e.Id);
            return ConvertToIFCGuid(guid);
        }
        public static ElementType GetElementType(this Autodesk.Revit.DB.ElementType type, Document doc)
        {
            var t = new ElementType {Id = type.Id.IntegerValue, Name = type.Name, Guid = type.UniqueId};
            var pIter = type.Parameters.ForwardIterator();
            while (pIter.MoveNext())
            {
                var p = (Parameter) pIter.Current;
                var pp = p.GetParameter(doc);
                t.Parameters.Add(pp);
            }
            return t;
        }
        public static Element GetElement(this Autodesk.Revit.DB.Element e, Document doc)
        {
            var elem = new Element
            {
                Id = e.Id.IntegerValue,
                Guid = e.UniqueId,
                Name = e.Name
            };
            try
            {
                var guid = e.GetIFCGuid();
                elem.IfcId = guid;
            }
            catch
            {
            }
            if (e.Category != null)
            {
                elem.Category = new ElementCategory { Id = e.Category.Id.IntegerValue, Name = e.Category.Name };
            }
           
            var pIter = e.Parameters.ForwardIterator();
            while (pIter.MoveNext())
            {
                var param = GetParameter((Parameter)pIter.Current, doc);
                elem.Parameters.Add(param);
            }

            

            return elem;
        }

        public static ElementFamily GetFamily(this Family fam, Document doc)
        {
            var f = new ElementFamily {Id = fam.Id.IntegerValue, Guid = fam.UniqueId, Name = fam.Name};
#if R2014
            if (fam.FamilyCategory != null)
            {
                f.Category = new ElementCategory
                {
                    Id = fam.FamilyCategory.Id.IntegerValue,
                    Name = fam.FamilyCategory.Name
                };
            }
#else
            if (fam.FamilyCategory != null)
            {
                f.Category = new ElementCategory
                {
                    Id = fam.FamilyCategory.Id.IntegerValue,
                    Name = fam.FamilyCategory.Name
                };
            }
#endif
            var famPIter = fam.Parameters.ForwardIterator();
            while (famPIter.MoveNext())
            {
                var param = GetParameter((Parameter)famPIter.Current, doc);
                f.Parameters.Add(param);
            }
            if (String.IsNullOrEmpty(f.Name) && f.Category != null) //族文件中族的名称可能为空
            {
                f.Name = f.Category.Name;
            }
            return f;
        }

        public static ElementParameter GetParameter(this Parameter p, Document doc)
        {
            var param = new ElementParameter
            {
                Id = p.Id.IntegerValue,
                Name = p.Definition.Name,
                Value = ParameterUtils.ParameterToString(doc, p)
            };
            if (p.IsShared)
            {
                param.Guid = p.GUID.ToString().TrimStart('{').TrimEnd('}');
            }
            return param;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public static LevelElement GetLevel(this Level level, UnitConversionFactors units)
        {
            var dLevel = new LevelElement
            {
                Id = level.Id.IntegerValue,
                Name = level.Name,
                Guid = level.UniqueId,
                Elevation = Math.Round(units.LengthRatio * level.Elevation, 0)+"mm",
                ProjectElevation = Math.Round(units.LengthRatio * level.ProjectElevation, 0)+"mm"
            };
            return dLevel;
        }

        private static Level GetLevelFromParam(Document doc, Autodesk.Revit.DB.Element e)
        {
            //var name = "参照标高";
            //string value = string.Empty;
            var ps = e.Parameters;
            var psIter = ps.ForwardIterator();
            while (psIter.MoveNext())
            {
                var p = (Parameter)psIter.Current;
                if (p.StorageType == StorageType.ElementId)
                {
                    var element = ElementFilterUtils.GetElement(doc, p.AsElementId()) as Level;
                    if (element != null)
                    {
                        return element;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 从构件上获取楼层信息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="doc"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public static LevelElement GetLevel(this Autodesk.Revit.DB.Element e, Document doc, UnitConversionFactors units)
        {
            if (!e.LevelId.Equals(ElementId.InvalidElementId))
            {
                var level = (Level)ElementFilterUtils.GetElement(doc, e.LevelId);
                return level.GetLevel(units);
            }
            var l = GetLevelFromParam(doc, e);
            if (l != null) return l.GetLevel(units);
            return null;
        }
    }
}
