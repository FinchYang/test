using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using SimulaDesign.BimInfo;
using ElementType = SimulaDesign.BimInfo.ElementType;

namespace SimulaDesign.RevitBimInfo
{
    public class RevitModel
    {
        private readonly Document _doc;
        private readonly Family _fam;
        private readonly string _name;
        private readonly string _filePath;

        private readonly UnitConversionFactors _units = UnitConversionFactors.Nmm;

        public RevitModel(Document doc) : this(doc, null)
        {
        }

        public RevitModel(Document doc, string name)
        {
            _doc = doc;
            _filePath = doc.PathName;
            if (!String.IsNullOrEmpty(name)) _name = name;
            else _name = Path.GetFileNameWithoutExtension(doc.PathName);
            if (doc.IsFamilyDocument)
            {
                _fam = doc.OwnerFamily;
            }
        }

        private Model _model;


        private Model GetProjectData()
        {
            var model = new ProjectModel
            {
                Name = _name,
                Filepath = _filePath
            };
            GetLevels(_doc, model, _units); //视图可能根据楼层生成，因此这里获取所有楼层
            GetViews(_doc, model);
            //GetMats(_doc, model);
            //GetCates(_doc, model);
            //GetElemTypes(_doc, model);
            GetElements(_doc, model, _units);

            return (_model=model);
        }

        private Model GetFamilyData()
        {
            var model = new FamilyModel {Name = _name, Filepath = _filePath};

            //todo
            var fam = _fam;
            var doc = _doc;
            var paramSet = fam.Parameters.ForwardIterator();
            //var paramList = new List<ElementParameter>();
            while (paramSet.MoveNext())
            {
                var p = (Parameter)paramSet.Current;
                var name = p.Definition.Name;
                var value = ParameterUtils.ParameterToString(doc, p);
                if (String.IsNullOrWhiteSpace(value))
                {
                    value = "空";
                }
                var mfP = new ElementParameter {Id = p.Id.IntegerValue, Name = name, Value = value};
                if (p.IsShared)
                {
                    mfP.Guid = p.GUID.ToString();
                }
                model.Parameters.Add(mfP);
            }
            var type = doc.FamilyManager.CurrentType;
            var famParams = doc.FamilyManager.Parameters;
            //var typeParamList = new List<ElementParameter>();
            if (famParams != null)
            {
                var famIter = famParams.ForwardIterator();

                while (famIter.MoveNext())
                {
                    var p = (FamilyParameter)famIter.Current;
                    var pDef = p.Definition;
                    var name = pDef.Name;
                    var value = ParameterUtils.ParameterToString(doc, p, type);
                    if (String.IsNullOrWhiteSpace(value))
                    {
                        value = "空";
                    }
                    var param = new ElementParameter {Id = p.Id.IntegerValue, Name = name, Value = value};
                    if (p.IsShared)
                    {
                        param.Guid = p.GUID.ToString();
                    }
                    model.FamParameters.Add(param);
                }

            }

            Category cate = null;
            if (fam.Category != null)
            {
                cate = fam.Category;
            }
            if (fam.FamilyCategory != null)
            {
                cate = fam.FamilyCategory;
            }
            if (cate != null)
            {
                model.Category = new ElementCategory {Id = cate.Id.IntegerValue, Name = cate.Name};
            }

            model.Family = fam.GetFamily(doc);

            return (_model = model);
        }

        public Model GetData(bool update=true)
        {
            if (!update && _model != null) return _model;

            if (_fam == null)
            {
                return GetProjectData();
            }
            return GetFamilyData();
        }
        /// <summary>
        /// 获取所有的材料
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="model"></param>
        private static void GetMats(Document doc, ProjectModel model)
        {
            var mats = doc.GetElements<Material>();
            foreach (var m in mats)
            {
                var mm = GetMat(doc, m);
                model.Materials.Add(mm);
            }
        }
        /// <summary>
        /// 获取所有的类别
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="model"></param>
        private static void GetCates(Document doc, ProjectModel model)
        {
            var cates = doc.Settings.Categories;
            var cIter = cates.ForwardIterator();
            while (cIter.MoveNext())
            {
                var c = (Category) cIter.Current;
                model.Categories.Add(new ElementCategory {Id = c.Id.IntegerValue, Name = c.Name});
            }
        }

        private static MaterialElement GetMat(Document doc, Material m)
        {
            var mm = new MaterialElement
            {
                Id = m.Id.IntegerValue,
                Name = m.Name,
                Guid = m.UniqueId
            };
            var pIter = m.Parameters.ForwardIterator();
            while (pIter.MoveNext())
            {
                var p = (Parameter)pIter.Current;
                mm.Parameters.Add(p.GetParameter(doc));
            }
            return mm;
        }
        /// <summary>
        /// 获取所有楼层
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="model"></param>
        /// <param name="units"></param>
        private static void GetLevels(Document doc, ProjectModel model, UnitConversionFactors units)
        {
            var levels = doc.GetElements<Level>();
            foreach (var l in levels)
            {
                var ll = l.GetLevel(units);
                model.Levels.Add(ll);
            }
        }


        private static void GetViews(Document doc, ProjectModel model)
        {
            var views = doc.GetElements<View>();
            foreach (var v in views)
            {
                var vv = new ViewElement
                {
                    Id = v.Id.IntegerValue,
                    Guid = v.UniqueId,
                    Name = v.ViewName,
                    //ViewName = v.ViewName,
                    ViewType = (int) v.ViewType
                    
                };
                if (v.HasViewDiscipline())
                {
                    vv.ViewDiscipline = (int) v.Discipline;
                }
                if (v.GenLevel != null)
                {
                    var lev = doc.GetElement(v.GenLevel.Id);
                    vv.GenLevel = lev.UniqueId;
                }
                
                model.Views.Add(vv);
            }
        }
        /// <summary>
        /// 获取所有构件类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="model"></param>
        private static void GetElemTypes(Document doc, ProjectModel model)
        {
            var types = doc.GetElements<Autodesk.Revit.DB.ElementType>();
            foreach (var t in types)
            {
                var tt = new ElementType
                {
                    Id = t.Id.IntegerValue,
                    Guid = t.UniqueId,
                    Name = t.Name
                };
                var pIter = t.Parameters.ForwardIterator();
                while (pIter.MoveNext())
                {
                    var p = (Parameter) pIter.Current;
                    tt.Parameters.Add(p.GetParameter(doc));
                }
                model.Types.Add(tt);
            }
        }

        private static void GetElements(Document doc, ProjectModel model, UnitConversionFactors units)
        {
            ElementId viewId = null;
            if (doc.ActiveView != null)
            {
                viewId = doc.ActiveView.Id;
            }
            var elems = doc.GetAllElements(viewId);
            foreach (var e in elems)
            {
                if (e is Level || e is View || e is Material || e is Family || e is RevitLinkInstance) continue;
                var elem = e.GetElement(doc);
                if (elem == null) continue;
                //类别
                if (elem.Category == null)//过滤没有类别的构件
                {
                    continue;
                }
                if (!model.Categories.Contains(elem.Category))
                {
                    model.Categories.Add(elem.Category);
                }
                //类型
                var type = e.GetElementType(doc);
                if (type == null) //过滤没有类型的构件
                {
                    continue;
                }
                var t = model.Types.FirstOrDefault(c => c.Id == type.Id.IntegerValue);
                if (t == null)
                {
                    t = type.GetElementType(doc);
                    model.Types.Add(t);
                }
                elem.ElemType = t.GetKey();

                //族
                var famInst = e as FamilyInstance;
                if (famInst != null)
                {
                    var fam = model.Families.FirstOrDefault(c => c.Id == famInst.Symbol.Family.Id.IntegerValue);//
                    if (fam == null)
                    {
                        fam = famInst.Symbol.Family.GetFamily(doc);
                        model.Families.Add(fam);
                    }
                    elem.Family = fam.GetKey();
                }
                
                //楼层
                var l = e.GetLevel(doc, units);
                if (l != null)
                {
                    elem.Level = l.GetKey();
                    var ll = model.Levels.FirstOrDefault(c => c.Id == l.Id);
                    if (ll == null)
                    {
                        model.Levels.Add(l);
                    }
                }

                //材料
                var mat = MaterialUtils.GetMaterial(e, doc);
                if (mat != null)
                {
                    var mm = model.Materials.FirstOrDefault(c => c.Id == mat.Id.IntegerValue);//
                    if (mm == null)
                    {
                        mm = GetMat(doc, mat);
                        model.Materials.Add(mm);
                    }
                    elem.Material = mm.GetKey();
                }

                model.Elements.Add(elem);
                
            }
        }

        
    }
}
