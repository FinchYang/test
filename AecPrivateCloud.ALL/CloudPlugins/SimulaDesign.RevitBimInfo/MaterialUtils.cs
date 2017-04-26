using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace SimulaDesign.RevitBimInfo
{
    public static class MaterialUtils
    {
        public static Material GetMaterial(Element elem, Document doc)
        {
            if (doc == null) doc = elem.Document;
            if (elem is Wall)
            {
                var mat0 = GetMaterial(doc, (Wall)elem);
                if (mat0 != null) return mat0;
            }
            else if (elem is Floor)
            {
                var mat0 = GetMaterial(doc, (Floor)elem);
                if (mat0 != null) return mat0;
            }
            else
            {
                var mat0 = GetParamMaterial(doc, elem);
                if (mat0 != null) return mat0;
            }
            var ids = elem.GetMaterialIds(false);
            if (ids.Count > 0)
            {
                foreach (var mzId in ids)
                {
                    var mz = ElementFilterUtils.GetElement(doc, mzId) as Material;
                    Material mat0 = mz;
                    if (mat0 != null)
                    {
                        return mat0;
                    }
                }
            }
            //附加材质
            if (elem.Category != null)
            {
                Material mat = elem.Category.Material;
                if (mat != null) return mat;
                return mat;
            }
            return null;
        }
        static Material GetParamMaterial(Document doc, Element inst)
        {
            foreach (Parameter p in inst.Parameters)
            {
                var def = p.Definition;
                if (p.StorageType == StorageType.ElementId)
                {
                    if (def.ParameterType == ParameterType.Material) //||def.ParameterGroup == BuiltInParameterGroup.PG_MATERIALS 
                    {
                        var matId = p.AsElementId();
                        if (matId.Compare(ElementId.InvalidElementId) == 0)
                        {
                            if (inst.Category != null)
                            {
                                return inst.Category.Material;
                            }
                        }
                        else
                        {
                            return ElementFilterUtils.GetElement(doc, matId) as Material;
                        }
                    }
                }
            }
            return null;
        }
        static Material GetMaterial(Document doc, Floor floor)
        {
            FloorType aFloorType = floor.FloorType;
            if (!aFloorType.IsFoundationSlab)
            {
                CompoundStructure comStruct = aFloorType.GetCompoundStructure();
                Categories allCategories = doc.Settings.Categories;

                Category floorCategory = allCategories.get_Item(BuiltInCategory.OST_Floors);
                Material floorMat = floorCategory.Material;

                foreach (var structLayer in comStruct.GetLayers())
                {
                    var layerMat = ElementFilterUtils.GetElement(doc, structLayer.MaterialId) as Material;
                    if (layerMat == null)
                    {
                        switch (structLayer.Function)
                        {
                            case MaterialFunctionAssignment.Structure:
                                layerMat = allCategories.get_Item(BuiltInCategory.OST_FloorsStructure).Material;
                                break;
                        }
                    }
                    if (layerMat != null)
                    {
                        floorMat = layerMat;
                        break;
                    }
                }
                return floorMat;
            }
            return null;
        }
        internal static Material GetMaterial(Document doc, Wall wall)
        {
            WallType aWallType = wall.WallType;
            if (WallKind.Basic == aWallType.Kind)
            {
                CompoundStructure comStruct = aWallType.GetCompoundStructure();
                Categories allCategories = doc.Settings.Categories;
                // Get the category OST_Walls default Material;
                // use if that layer's default Material is <By Category>
                Category wallCategory = allCategories.get_Item(BuiltInCategory.OST_Walls);
                Material wallMaterial = wallCategory.Material;
                foreach (CompoundStructureLayer structLayer in comStruct.GetLayers())
                {
                    var layerMaterial = ElementFilterUtils.GetElement(doc, structLayer.MaterialId) as Material;
                    if (layerMaterial == null)
                    {
                        switch (structLayer.Function)
                        {
                            case MaterialFunctionAssignment.Structure:
                                layerMaterial = allCategories.get_Item(BuiltInCategory.OST_WallsStructure).Material;
                                break;
                        }
                    }
                    if (layerMaterial != null)
                    {
                        wallMaterial = layerMaterial;
                        break;
                    }
                }
                return wallMaterial;
            }

            return null;
        }
    }
}
