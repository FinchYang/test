using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.Revit.DB;

namespace SimulaDesign.RevitBimInfo
{
    public static class ElementFilterUtils
    {
        /// <summary>
        /// 根据ID获取对象，适配不同的Revit版本
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Element GetElement(this Document doc, ElementId id)
        {
            return doc.GetElement(id);
        }

        public static ElementType GetElementType(this Element elem, Document doc)
        {
            try
            {
                //var doc = elem.Document;
                var typeId = elem.GetTypeId();
                if (!typeId.Equals(ElementId.InvalidElementId))
                {
                    return (ElementType)GetElement(doc, typeId);
                }
            }
            catch
            {
            }
            var t = elem.GetType();
            var ps = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var psList = new List<PropertyInfo>();
            var eType = typeof (ElementType);
            foreach (var p in ps)
            {
                var method = p.GetGetMethod();
                if (method == null) continue;
                if (eType.IsAssignableFrom(method.ReturnType))
                {
                    psList.Add(p);
                }
            }
            if (psList.Count == 1)
            {
                return (ElementType)psList[0].GetValue(elem, null);
            }
            return null;
        }
        /// <summary>
        /// 按照类型获得构件，T必须是Element的派生类
        /// </summary>
        /// <typeparam name="T">包含的类型</typeparam>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetElements<T>(this Document doc) where T : Element
        {
            return GetFilteredCollectorByType(doc, new[] { typeof(T) }).Cast<T>();
        }

        /// <summary>
        /// 不包含类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public static IEnumerable<Element> GetAllElements(this Document doc, ElementId viewId=null)
        {
            if (viewId != null)
            {
                return new FilteredElementCollector(doc, viewId).WhereElementIsNotElementType();
            }
            return new FilteredElementCollector(doc).WhereElementIsNotElementType();
        }

        private static FilteredElementCollector GetFilteredCollectorByType(Document doc, Type[] types)
        {
            if (types == null || types.Length == 0) throw new ArgumentNullException("types", "必须提供类型(Type)");
            var collector = new FilteredElementCollector(doc);
            ElementFilter filter;
            if (types.Length > 1)
            {
                var listFilters = types.Select(t => new ElementClassFilter(t)).Cast<ElementFilter>().ToList();
                filter = new LogicalOrFilter(listFilters);
            }
            else
            {
                filter = new ElementClassFilter(types[0]);
            }
            return collector.WherePasses(filter);
        }

        private static FilteredElementCollector GetFilteredCollectorByCategory(
            Document doc, bool includeElemType, BuiltInCategory[] category)
        {
            if (category == null || category.Length == 0) throw new ArgumentNullException("category", "必须提供类别(Category)");
            var collector = new FilteredElementCollector(doc);

            ElementFilter filter;
            if (category.Length > 1)
            {
                var catesFilters = category.Select(c => new ElementCategoryFilter(c)).Cast<ElementFilter>().ToList();
                filter = new LogicalOrFilter(catesFilters);
            }
            else
            {
                filter = new ElementCategoryFilter(category[0]);
            }
            if (includeElemType)
            {
                return collector.WherePasses(filter);
            }
            return collector.WhereElementIsNotElementType().WherePasses(filter);
        }

        private static IList<Element> GetElementsByType(Document doc, bool includeElemType, Type[] types)
        {
            if (includeElemType)
            {
                return GetFilteredCollectorByType(doc, types).ToElements();
            }
            return GetFilteredCollectorByType(doc, types).WhereElementIsNotElementType().ToElements();
        }

        private static IList<Element> GetElementsByCategory(Document doc, BuiltInCategory[] cates)
        {
            return GetFilteredCollectorByCategory(doc, false, cates).ToElements();
        }


        public static Document[] GetLinkedDocuments(this Document doc)
        {

#if R2013
            var docs = new List<Document>();
            var collector = new FilteredElementCollector(doc);
            IList<Element> elems = collector.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkType)).ToElements();
            foreach (Element e in elems)
            {
                var linkType = e as RevitLinkType;
                if (linkType != null)
                {
                    foreach (Document linkedDoc in doc.Application.Documents)
                    {
                        if (linkedDoc.Title.Equals(linkType.Name))
                        {
                            docs.Add(linkedDoc);
                        }
                    }
                }
            }
            return docs.ToArray();
#elif R2012
            var collector = new FilteredElementCollector(doc);
            var linkedFiles = collector.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(
            typeof(RevitLinkType)).Select(d => d.GetExternalFileReference());
            var linkedFileNames = linkedFiles.Select(x => 
                ModelPathUtils.ConvertModelPathToUserVisiblePath(x.GetAbsolutePath())).ToList();

            return doc.Application.Documents.Cast<Document>()
                .Where(d => linkedFileNames.Any(fileName => d.PathName.Equals(fileName))).ToArray();
#else
            var collector = new FilteredElementCollector(doc);
            var elemFilter = new ElementClassFilter(typeof(RevitLinkInstance));
            var elementids = collector.WherePasses(elemFilter).OfType<RevitLinkInstance>().Select(d => d.Id);
            var docs = new List<Document>();
            foreach (var id in elementids)
            {
                var lnkElem = GetElement(doc, id) as RevitLinkInstance;
                if (lnkElem == null) continue;
                var linkDoc = lnkElem.GetLinkDocument();
                docs.Add(linkDoc);
            }
            return docs.ToArray();
#endif

        }
    }
}
