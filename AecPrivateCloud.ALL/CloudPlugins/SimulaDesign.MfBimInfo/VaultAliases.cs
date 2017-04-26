using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFilesAPI;

namespace SimulaDesign.MfBimInfo
{
    public class VaultAliases
    {
        public Vault Vault { get; private set; }

        private VaultAliases()
        {
            ObDict = new Dictionary<string, int>();
            CsDict = new Dictionary<string, int>();
            PdDict = new Dictionary<string, int>();
        }

        public static VaultAliases GetAliases(Vault vault)
        {

            var va = new VaultAliases { Vault = vault };
            try
            {
                var ocId = MfAlias.GetObjType(vault, OB.Category);
                va.ObDict.Add(OB.Category, ocId);
                var cateObjType = vault.ObjectTypeOperations.GetObjectType(ocId);
                va.Categories = cateObjType.DefaultPropertyDef;

                var ofId = MfAlias.GetObjType(vault, OB.Level);
                va.ObDict.Add(OB.Level, ofId);
                var floorObjType = vault.ObjectTypeOperations.GetObjectType(ofId);
                va.Levels = floorObjType.DefaultPropertyDef;

                var opId = MfAlias.GetObjType(vault, OB.Part);
                va.ObDict.Add(OB.Part, opId);
                var partObjType = vault.ObjectTypeOperations.GetObjectType(opId);
                va.Parts = partObjType.DefaultPropertyDef;

                var parttId = MfAlias.GetObjType(vault, OB.PartType);
                va.ObDict.Add(OB.PartType, parttId);
                var parttObjType = vault.ObjectTypeOperations.GetObjectType(parttId);
                va.PartTypes = parttObjType.DefaultPropertyDef;

                var famId = MfAlias.GetObjType(vault, OB.Family);
                va.ObDict.Add(OB.Family, famId);
                var famObjType = vault.ObjectTypeOperations.GetObjectType(famId);
                va.PartFamilies = famObjType.DefaultPropertyDef;

                var matId = MfAlias.GetObjType(vault, OB.Material);
                va.ObDict.Add(OB.Material, matId);
                var matObjType = vault.ObjectTypeOperations.GetObjectType(matId);
                va.Materials = matObjType.DefaultPropertyDef;

                var viewId = MfAlias.GetObjType(vault, OB.View);
                va.ObDict.Add(OB.View, viewId);
                var viewObjType = vault.ObjectTypeOperations.GetObjectType(viewId);
                va.Views = viewObjType.DefaultPropertyDef;





                var olId = MfAlias.GetObjType(vault, OB.Floor);
                va.ObDict.Add(OB.Floor, olId);

                var mduId = MfAlias.GetObjType(vault, OB.ModelUnit);
                va.ObDict.Add(OB.ModelUnit, mduId);

                var mdiId = MfAlias.GetObjType(vault, OB.ModelDisc);
                va.ObDict.Add(OB.ModelDisc, mdiId);




                var ccId = MfAlias.GetObjectClass(vault, CS.Category);
                va.CsDict.Add(CS.Category, ccId);

                var cfId = MfAlias.GetObjectClass(vault, CS.Level);
                va.CsDict.Add(CS.Level, cfId);

                var cpId = MfAlias.GetObjectClass(vault, CS.Part);
                va.CsDict.Add(CS.Part, cpId);

                var cpfId = MfAlias.GetObjectClass(vault, CS.PartType);
                va.CsDict.Add(CS.PartType, cpfId);

                var cFamId = MfAlias.GetObjectClass(vault, CS.Family);
                va.CsDict.Add(CS.Family, cFamId);

                var cppId = MfAlias.GetObjectClass(vault, CS.View);
                va.CsDict.Add(CS.View, cppId);

                var cmatId = MfAlias.GetObjectClass(vault, CS.Material);
                va.CsDict.Add(CS.Material, cmatId);


                var cfamPartId = MfAlias.GetObjectClass(vault, CS.FamilyPart);
                va.CsDict.Add(CS.FamilyPart, cfamPartId);

                var ifcModelId = MfAlias.GetObjectClass(vault, CS.IfcModel, false);
                va.CsDict.Add(CS.IfcModel, ifcModelId);

                //var cparamId = MfAlias.GetObjectClass(vault, CS.Parameter);
                //va.CsDict.Add(CS.Parameter, cparamId);

                var docbId = MfAlias.GetObjectClass(vault, CS.DocBimModel);
                va.CsDict.Add(CS.DocBimModel, docbId);

                var clId = MfAlias.GetObjectClass(vault, CS.Floor);
                va.CsDict.Add(CS.Floor, clId);

                var cmduId = MfAlias.GetObjectClass(vault, CS.ModelUnit);
                va.CsDict.Add(CS.ModelUnit, cmduId);

                var cmdiId = MfAlias.GetObjectClass(vault, CS.ModelDisc);
                va.CsDict.Add(CS.ModelDisc, cmdiId);




                ///////



                var fpId = MfAlias.GetPropDef(vault, PD.OwnedFamily);
                va.PdDict.Add(PD.OwnedFamily, fpId);

                var glId = MfAlias.GetPropDef(vault, PD.Level);
                va.PdDict.Add(PD.Level, glId);

                var guidId = MfAlias.GetPropDef(vault, PD.Guid);
                va.PdDict.Add(PD.Guid, guidId);

                var idId = MfAlias.GetPropDef(vault, PD.Id);
                va.PdDict.Add(PD.Id, idId);

                var pdMatId = MfAlias.GetPropDef(vault, PD.Material);
                va.PdDict.Add(PD.Material, pdMatId);

                var pnId = MfAlias.GetPropDef(vault, PD.Name);
                va.PdDict.Add(PD.Name, pnId);

                var pdCateId = MfAlias.GetPropDef(vault, PD.PartCategory);
                va.PdDict.Add(PD.PartCategory, pdCateId);

                var pdTypeId = MfAlias.GetPropDef(vault, PD.PartType);
                va.PdDict.Add(PD.PartType, pdTypeId);

                var vdId = MfAlias.GetPropDef(vault, PD.ViewDisc);
                va.PdDict.Add(PD.ViewDisc, vdId);

                var vtId = MfAlias.GetPropDef(vault, PD.ViewType);
                va.PdDict.Add(PD.ViewType, vtId);

                var elevId = MfAlias.GetPropDef(vault, PD.Elevation);
                va.PdDict.Add(PD.Elevation, elevId);

                var plId = MfAlias.GetPropDef(vault, PD.ParamList);
                va.PdDict.Add(PD.ParamList, plId);

                var omId = MfAlias.GetPropDef(vault, PD.OwnedModel);
                va.PdDict.Add(PD.OwnedModel, omId);

                var ofpId = MfAlias.GetPropDef(vault, PD.FamParamList);
                va.PdDict.Add(PD.FamParamList, ofpId);

                var ifcId = MfAlias.GetPropDef(vault, PD.IfcId);
                va.PdDict.Add(PD.IfcId, ifcId);

                var modelId = MfAlias.GetPropDef(vault, PD.ModelUrl, false);
                va.PdDict.Add(PD.ModelUrl, modelId);

                var docObjType =
                    vault.ObjectTypeOperations.GetBuiltInObjectType(MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
                va.Documents = docObjType.DefaultPropertyDef;


                var pmnId = MfAlias.GetPropDef(vault, PD.ModelName, false);
                va.PdDict.Add(PD.ModelName, pmnId);

                var pmuaId = MfAlias.GetPropDef(vault, PD.ModelUnitAt, false);
                va.PdDict.Add(PD.ModelUnitAt, pmuaId);

                var pfaId = MfAlias.GetPropDef(vault, PD.FloorAt, false);
                va.PdDict.Add(PD.FloorAt, pfaId);

                var pdaId = MfAlias.GetPropDef(vault, PD.DiscAt, false);
                va.PdDict.Add(PD.DiscAt, pdaId);

                if (va.ObDict[OB.ModelUnit] != -1)
                {
                    

                    var muObjType = vault.ObjectTypeOperations.GetObjectType(va.ObDict[OB.ModelUnit]);
                    va.OwnerModelUnit = muObjType.OwnerPropertyDef;
                }

                if (va.ObDict[OB.Floor] != -1)
                {
                    

                    var flObjType = vault.ObjectTypeOperations.GetObjectType(va.ObDict[OB.Floor]);
                    va.OwnerFloor = flObjType.OwnerPropertyDef;
                }

                if (va.ObDict[OB.ModelDisc] != -1)
                {
                    

                    var mdObjType = vault.ObjectTypeOperations.GetObjectType(va.ObDict[OB.ModelDisc]);
                    va.OwnerModelDisc = mdObjType.OwnerPropertyDef;
                }


                va.IsValid = true;
            }
            catch(Exception ex)
            {
                throw;
                va.IsValid = false;
            }

            return va;
        }

        public int Documents { get; private set; }

        public int Levels { get; private set; }

        public int Categories { get; private set; }

        public int Parts { get; private set; }

        public int PartTypes { get; private set; }

        public int PartFamilies { get; set; }

        public int Materials { get; private set; }

        public int Views { get; private set; }

        //public int Parameters { get; set; }

        public int OwnerModelUnit { get; private set; }

        public int OwnerFloor { get; private set; }

        public int OwnerModelDisc { get; private set; }

        public int OwnerModelType { get; private set; }

        public bool IsValid { get; private set; }

        public IDictionary<string, int> ObDict
        {
            get;
            private set;
        }

        public IDictionary<string, int> CsDict
        {
            get;
            private set;
        }

        public IDictionary<string, int> PdDict
        {
            get;
            private set;
        }
    }
}
