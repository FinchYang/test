using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFilesAPI;
using SimulaDesign.BimInfo;

namespace SimulaDesign.MfBimInfo
{
    internal static class BaseElementExtensions
    {
        public static void AddBasicProperties(this ElementWithGuid elem, PropertyValues pvs, VaultAliases vaultAlias, int modelId)
        {
            var idPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Id]};
            idPV.Value.SetValue(MFDataType.MFDatatypeInteger, elem.Id);
            pvs.Add(-1, idPV);

            var guidPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Guid]};
            guidPV.Value.SetValue(MFDataType.MFDatatypeText, elem.Guid);
            pvs.Add(-1, guidPV);

            var namePV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Name]};
            namePV.Value.SetValue(MFDataType.MFDatatypeText, elem.Name);
            pvs.Add(-1, namePV);

            var modelPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.OwnedModel]};
            modelPV.Value.SetValue(MFDataType.MFDatatypeLookup, modelId);
            pvs.Add(-1, modelPV);
        }

        public static ObjVer CreateBasicObject(int objTypeId, int classId, PropertyValues pvs,
            Vault vault, AccessControlList acl)
        {
            //var vault = vaultAlias.Vault;

            var classPV = new PropertyValue { PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass };
            classPV.Value.SetValue(MFDataType.MFDatatypeLookup, classId);
            pvs.Add(-1, classPV);

            if (acl == null)
            {
                var objType = vault.ObjectTypeOperations.GetObjectType(objTypeId);
                acl = objType.AccessControlList;
            }
            var obj = vault.ObjectOperations.CreateNewObject(objTypeId, pvs, AccessControlList: acl); //, new SourceObjectFiles(), new AccessControlList()
            return obj.ObjVer;
            //return vault.ObjectOperations.CheckIn(obj.ObjVer).ObjVer.ID;
        }

        public static PropertyValue CreateParameterProp(List<ElementParameter> parameters, VaultAliases vaultAlias)
        {
            parameters.Sort();
            var pPV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.ParamList] };
            pPV.Value.SetValue(MFDataType.MFDatatypeMultiLineText,
                String.Join("\r\n", parameters.Select(c => c.ToString())));
            return pPV;
        }
    }

    internal static class FloorExtensions
    {
        public static void AddProperties(this LevelElement level, PropertyValues pvs, VaultAliases vaultAlias, int modelId)
        {
            level.AddBasicProperties(pvs, vaultAlias, modelId);
            var elevPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Elevation]};
            elevPV.Value.SetValue(MFDataType.MFDatatypeText, level.Elevation);
            pvs.Add(-1, elevPV);
        }

        public static ObjVer CreateFloor(this LevelElement level, VaultAliases vaultAlias, int modelId, AccessControlList acl)
        {
            var objTypeId = vaultAlias.ObDict[OB.Level];
            var classId = vaultAlias.CsDict[CS.Level];
            var pvs = new PropertyValues();
            AddProperties(level, pvs, vaultAlias, modelId);

            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);

        }

        public static PropertyValues UpdateFloor(this LevelElement level, VaultAliases vaultAlias, int modelId)
        {
            var pvs = new PropertyValues();
            AddProperties(level, pvs, vaultAlias, modelId);

            return pvs;
        }
    }

    internal static class ViewExtensions
    {
        public static void AddProperties(this ViewElement view, PropertyValues pvs, VaultAliases vaultAlias, int levelId, int modeId)
        {
            view.AddBasicProperties(pvs, vaultAlias, modeId);

            var vtPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.ViewType]};
            vtPV.Value.SetValue(MFDataType.MFDatatypeInteger, view.ViewType);
            pvs.Add(-1, vtPV);

            if (view.ViewDiscipline != null)
            {
                var vdPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.ViewDisc]};
                vdPV.Value.SetValue(MFDataType.MFDatatypeInteger, view.ViewDiscipline.Value);
                pvs.Add(-1, vdPV);
            }
            if (view.GenLevel != null)
            {
                if (levelId <= 0) throw new Exception("必须指定楼层");
                var glPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Level]};
                glPV.Value.SetValue(MFDataType.MFDatatypeLookup, levelId);
                pvs.Add(-1, glPV);
            }
        }
        public static ObjVer CreateView(this ViewElement view, VaultAliases vaultAlias, int levelId, int modelId, AccessControlList acl)
        {

            var objTypeId = vaultAlias.ObDict[OB.View];
            var classId = vaultAlias.CsDict[CS.View];

            var pvs = new PropertyValues();
            AddProperties(view, pvs, vaultAlias, levelId, modelId);

            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdateView(this ViewElement view, VaultAliases vaultAlias, int levelId, int modelId)
        {
            var pvs = new PropertyValues();
            AddProperties(view, pvs, vaultAlias, levelId, modelId);

            return pvs;
        }
    }

    internal static class CategoryExtensions
    {
        public static void AddProperties(this ElementCategory cate, PropertyValues pvs, VaultAliases vaultAlias, int modelId)
        {
            if (cate == null) return;
            var idPV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.Id] };
            idPV.Value.SetValue(MFDataType.MFDatatypeInteger, cate.Id);
            pvs.Add(-1, idPV);

            var namePV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.Name] };
            namePV.Value.SetValue(MFDataType.MFDatatypeText, cate.Name);
            pvs.Add(-1, namePV);

            var modelPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.OwnedModel]};
            modelPV.Value.SetValue(MFDataType.MFDatatypeLookup, modelId);
            pvs.Add(-1, modelPV);
        }

        public static ObjVer CreateCategory(this ElementCategory cate, VaultAliases vaultAlias, int modelId, AccessControlList acl)
        {
            if (cate == null) return null;
            var objTypeId = vaultAlias.ObDict[OB.Category];
            var classId = vaultAlias.CsDict[CS.Category];
            var pvs = new PropertyValues();
            AddProperties(cate, pvs, vaultAlias, modelId);

            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdateCategory(this ElementCategory cate, VaultAliases vaultAlias, int modelId)
        {
            if (cate == null) return null;
            var pvs = new PropertyValues();
            AddProperties(cate, pvs, vaultAlias, modelId);

            return pvs;
        }
    }

    internal static class PartTypeExtensions
    {
        public static void AddProperties(this ElementType elemType, PropertyValues pvs, VaultAliases vaultAlias, int modelId)
        {
            elemType.AddBasicProperties(pvs, vaultAlias, modelId);

            if (elemType.Parameters.Count > 0)
            {
                var pPV = BaseElementExtensions.CreateParameterProp(elemType.Parameters, vaultAlias);
                pvs.Add(-1, pPV);
            }
        }

        public static ObjVer CreatePartType(this ElementType elemType, VaultAliases vaultAlias, int modelId, AccessControlList acl)
        {
            var objTypeId = vaultAlias.ObDict[OB.PartType];
            var classId = vaultAlias.CsDict[CS.PartType];
            var pvs = new PropertyValues();
            AddProperties(elemType, pvs, vaultAlias, modelId);

            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdatePartType(this ElementType elemType, VaultAliases vaultAlias, int modelId)
        {
            var pvs = new PropertyValues();
            AddProperties(elemType, pvs, vaultAlias, modelId);

            return pvs;
        }
    }

    internal static class MaterialExtensions
    {
        public static void AddProperties(this MaterialElement mat, PropertyValues pvs, VaultAliases vaultAlias, int modelId)
        {
            mat.AddBasicProperties(pvs, vaultAlias, modelId);
            
            if (mat.Parameters.Count > 0)
            {

                var pPV = BaseElementExtensions.CreateParameterProp(mat.Parameters, vaultAlias);
                pvs.Add(-1, pPV);
            }
            
        }

        public static ObjVer CreateMaterial(this MaterialElement mat, VaultAliases vaultAlias, int modelId, AccessControlList acl)
        {
            var objTypeId = vaultAlias.ObDict[OB.Material];
            var classId = vaultAlias.CsDict[CS.Material];
            var pvs = new PropertyValues();
            AddProperties(mat, pvs, vaultAlias, modelId);
            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdateMaterial(this MaterialElement mat, VaultAliases vaultAlias, int modelId)
        {
            var pvs = new PropertyValues();
            AddProperties(mat, pvs, vaultAlias, modelId);

            return pvs;
        }
    }

    internal static class FamilyExtensions
    {
        public static void AddProperties(this ElementFamily fam, PropertyValues pvs, VaultAliases vaultAlias, int cateId, int modelId)
        {
            fam.AddBasicProperties(pvs, vaultAlias, modelId);
            var catePV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.PartCategory] };
            catePV.Value.SetValue(MFDataType.MFDatatypeLookup, cateId);
            pvs.Add(-1, catePV);
            if (fam.Parameters.Count > 0)
            {
                var pPV = BaseElementExtensions.CreateParameterProp(fam.Parameters, vaultAlias);
                pvs.Add(-1, pPV);
            }
        }

        public static ObjVer CreateFamily(this ElementFamily fam, VaultAliases vaultAlias, int cateId, int modelId, AccessControlList acl)
        {
            var objTypeId = vaultAlias.ObDict[OB.Family];
            var classId = vaultAlias.CsDict[CS.Family];

            var pvs = new PropertyValues();
            AddProperties(fam, pvs, vaultAlias, cateId, modelId);
            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdateFamily(this ElementFamily fam, VaultAliases vaultAlias, int cateId, int modelId)
        {
            var pvs = new PropertyValues();
            AddProperties(fam, pvs, vaultAlias, cateId, modelId);

            return pvs;
        }
    }

    internal static class PartExtensions
    {
        public static void AddProperties(this Element elem, PropertyValues pvs, VaultAliases vaultAlias
            , int cateId, int typeId, int matId, int levelId, int famId, int modelId, string modelUrl, int? unitId, int? floorId, int? discId)
        {
            elem.AddBasicProperties(pvs, vaultAlias, modelId);

            if (elem.IfcId != null)
            {
                var ifcPV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.IfcId] };
                ifcPV.Value.SetValue(MFDataType.MFDatatypeText, elem.IfcId);
                pvs.Add(-1, ifcPV);
                if (!String.IsNullOrEmpty(modelUrl))
                {
                    var urlPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.ModelUrl]};
                    urlPV.Value.SetValue(MFDataType.MFDatatypeMultiLineText, modelUrl+"&ifcguid="+elem.IfcId);
                    pvs.Add(-1, urlPV);
                }
                
            }

            var catePV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.PartCategory]};
            catePV.Value.SetValue(MFDataType.MFDatatypeLookup, cateId);
            pvs.Add(-1, catePV);
            var typePV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.PartType]};
            typePV.Value.SetValue(MFDataType.MFDatatypeLookup, typeId);
            pvs.Add(-1, typePV);
            if (matId > 0)
            {
                var matPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Material]};
                matPV.Value.SetValue(MFDataType.MFDatatypeLookup, matId);
                pvs.Add(-1, matPV);
            }
            if (levelId > 0)
            {
                var levelPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.Level]};
                levelPV.Value.SetValue(MFDataType.MFDatatypeLookup, levelId);
                pvs.Add(-1, levelPV);
            }
            if (elem.Parameters.Count > 0)
            {
                var pPV = BaseElementExtensions.CreateParameterProp(elem.Parameters, vaultAlias);
                pvs.Add(-1, pPV);
            }
            if (elem.Family != null)
            {
                var pPV = new PropertyValue { PropertyDef = vaultAlias.PdDict[PD.OwnedFamily] };
                pPV.Value.SetValue(MFDataType.MFDatatypeLookup, famId);
                pvs.Add(-1, pPV);
            }

            if (unitId != null)
            {
                var pPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.ModelUnitAt]};
                pPV.Value.SetValue(MFDataType.MFDatatypeLookup, unitId.Value);
                pvs.Add(-1, pPV);
            }
            if (floorId != null)
            {
                var pPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.FloorAt]};
                pPV.Value.SetValue(MFDataType.MFDatatypeLookup, floorId.Value);
                pvs.Add(-1, pPV);
            }
            if (discId != null)
            {
                var pPV = new PropertyValue {PropertyDef = vaultAlias.PdDict[PD.DiscAt]};
                pPV.Value.SetValue(MFDataType.MFDatatypeLookup, discId.Value);
                pvs.Add(-1, pPV);
            }

        }

        public static ObjVer CreatePart(this Element elem, VaultAliases vaultAlias
            , int cateId, int typeId, int matId, int levelId, int famId, int modelId, AccessControlList acl, string modelUrl, int? unitId, int? floorId, int? discId)
        {
            var objTypeId = vaultAlias.ObDict[OB.Part];
            var classId = vaultAlias.CsDict[CS.Part];
            var pvs = new PropertyValues();
            AddProperties(elem, pvs, vaultAlias, cateId, typeId, matId, levelId, famId, modelId, modelUrl, unitId, floorId, discId);
            return BaseElementExtensions.CreateBasicObject(objTypeId, classId, pvs, vaultAlias.Vault, acl);
        }

        public static PropertyValues UpdatePart(this Element elem, VaultAliases vaultAlias
            , int cateId, int typeId, int matId, int levelId, int famId, int modelId, string modelUrl, int? unitId, int? floorId, int? discId)
        {

            var pvs = new PropertyValues();
            AddProperties(elem, pvs, vaultAlias, cateId, typeId, matId, levelId, famId, modelId, modelUrl, unitId, floorId, discId);

            return pvs;
        }
    }

    
}
