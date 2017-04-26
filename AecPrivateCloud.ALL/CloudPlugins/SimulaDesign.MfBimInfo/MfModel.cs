using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;
using SimulaDesign.BimInfo;

namespace SimulaDesign.MfBimInfo
{
    public class MfModel
    {
        private readonly Model _model;
        private string _name;

        private readonly string _filePath;

        public static bool SetModelId(Model model)
        {
            string filePath = model.Filepath;
            if (!ClientUtils.IsInMf(filePath))
            {
                return false;
            }
            var obj = ClientUtils.GetObjectFromURL(filePath);
            model.Id = obj.ObjVer.ID;
            return true;
        }

        public static MfModel GetModel(string filePath)
        {
            var model = new MfModel(filePath);
            model.Initialize();
            return model;
        }

        public static MfModel GetClientModel(Model model)
        {
            var mfm = new MfModel(model, null);
            mfm.Initialize();
            return mfm;
        }

        public static MfModel GetServerModel(Model model, Vault vault)
        {
            var mfm = new MfModel(model, vault);
            mfm.Initialize();
            return mfm;
        }

        private MfModel(Model model, Vault vault)
        {
            _model = model;
            _name = model.Name;
            _filePath = model.Filepath;
            _vault = vault;
            if (vault != null)
            {
                _clientVault = false;
            }
            if (vault != null)
            {
                _aliases = VaultAliases.GetAliases(_vault);  
            }
        }

        private MfModel(string filePath)
        {
            _filePath = filePath;
        }

        public string GetXbimUploadUrl()
        {
            var name = "GetWebHost";
            if (_vault.ExtensionMethodOperations.DoesActiveVaultExtensionMethodExist(name))
            {
                var url = _vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(name, "xbim");
                if (!url.EndsWith("/"))
                {
                    url += "/";
                }
                var guid = _vault.GetGUID().TrimStart('{').TrimEnd('}');
                var objType = _obj.ObjVer.Type;
                var objId = _obj.ObjVer.ID;
                return url + String.Format("Model/Upload?Guid={0}&TypeId={1}&ObjId={2}", guid, objType, objId);
            }
            return String.Empty;
        }

        public string GetModelUrl()
        {
            var name = "GetWebHost";
            if (_vault.ExtensionMethodOperations.DoesActiveVaultExtensionMethodExist(name))
            {
                var url = _vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(name, "xbim");
                if (!url.EndsWith("/"))
                {
                    url += "/";
                }
                var guid = _vault.GetGUID().TrimStart('{').TrimEnd('}');
                var objType = _obj.ObjVer.Type;
                var objId = _obj.ObjVer.ID;
                return url + String.Format("Model/Show?Guid={0}&TypeId={1}&ObjId={2}", guid, objType, objId);
            }
            return String.Empty;
        }

        private ObjectVersion _obj;
        private Vault _vault;
        private readonly bool _clientVault=true;
        private VaultAliases _aliases;

        private bool _inited;

        private void Initialize()
        {
            if (_inited) return;
            if (_clientVault)
            {
                if (!ClientUtils.IsInMf(_filePath))
                {
                    throw new Exception("文件未在云系统中！");
                }
                if (_vault == null)
                {
                    var obj = ClientUtils.GetObjectFromURL(_filePath);
                    _vault = obj.Vault;
                    _obj = obj.VersionData;
                }
            }
            else
            {
                if (_vault == null)
                {
                    throw new Exception("服务端必须指定文档库");
                }
                var objID = new ObjID();
                objID.SetIDs((int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument, _model.Id);
                var objAndProps = _vault.ObjectOperations.GetLatestObjectVersionAndProperties(objID, true);
                _obj = objAndProps.VersionData;
            }
            
            _name = _obj.Title;
            if (_aliases == null)
            {
                _aliases = VaultAliases.GetAliases(_vault);
            }
            if (!_aliases.IsValid)
            {
                throw new Exception("文档库缺少必须的元数据！");
            }
            _inited = true;
        }

        public bool IsPublished()
        {
            Initialize();


            var propDef = _aliases.PdDict[PD.OwnedModel];
            var modelId = _obj.ObjVer.ID;

            var scs = new SearchConditions();

            var delSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            delSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSC.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSC);

            var modelSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            modelSC.Expression.DataPropertyValuePropertyDef = propDef;
            modelSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, modelId);
            scs.Add(-1, modelSC);

            var res = _vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            return res.Count > 0;

        }


        internal static ObjectSearchResults GetParts(Vault vault, VaultAliases aliases, string guid)
        {
            var partObjTypeId = aliases.ObDict[OB.Part];
            var guidPropId = aliases.PdDict[PD.Guid];

            var scs = new SearchConditions();

            var typeSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            typeSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, partObjTypeId);
            scs.Add(-1, typeSC);

            var ifcSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            ifcSC.Expression.DataPropertyValuePropertyDef = guidPropId;
            ifcSC.TypedValue.SetValue(MFDataType.MFDatatypeText, guid);
            scs.Add(-1, ifcSC);

            var delSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            delSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSC.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSC);

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);

            return res;
        }

        

        public string GetPart(string guid)
        {
            Initialize();
            if (!_clientVault)
            {
                throw new Exception("必须在客户端使用！");
            }
            var res = GetParts(_vault, _aliases, guid);
            if (res.Count == 0) return null;
            var objVer = res[1].ObjVer;
            return _vault.ObjectOperations.GetMFilesURLForObject(objVer.ObjID, objVer.Version, false,
                                MFilesURLType.MFilesURLTypeShow);
        }

        

        private void OperateFamilyModel()
        {
            if (_model.IsProject)
            {
                throw new Exception("必须是族模型");
            }
            //todo
            var model = (FamilyModel)_model;
            var modelId = _obj.ObjVer.ID;

            var ctypeId = _aliases.ObDict[OB.Category];
            var cobjType = _aliases.Vault.ObjectTypeOperations.GetObjectType(ctypeId);
            var cacl = cobjType.AccessControlList;
            ObjVer cateObjVer = null;
            if (model.Category != null)
            {
                cateObjVer = model.Category.CreateCategory(_aliases, modelId, cacl);
            }
            else if (model.Family != null && model.Family.Category != null)
            {
                cateObjVer = model.Family.Category.CreateCategory(_aliases, modelId, cacl);
            }
            
            var objId = _aliases.ObDict[OB.Part];
            var classId = _aliases.CsDict[CS.FamilyPart];

            var pvs = new PropertyValues();

            var classPV = new PropertyValue {PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass};
            classPV.Value.SetValue(MFDataType.MFDatatypeLookup, classId);
            pvs.Add(-1, classPV);

            var namePV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.Name]};
            namePV.Value.SetValue(MFDataType.MFDatatypeText, _name);
            pvs.Add(-1, namePV);

            ObjVer famObjVer = null;
            if (model.Family != null)
            {
                var typeId = _aliases.ObDict[OB.Family];
                var objType = _aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                var cateId = 0;
                if (cateObjVer != null)
                {
                    cateId = cateObjVer.ID;
                }
                famObjVer = model.Family.CreateFamily(_aliases, cateId, modelId, acl);
            }
            if (cateObjVer != null)
            {
                var catePV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.PartCategory]};
                catePV.Value.SetValue(MFDataType.MFDatatypeLookup, cateObjVer.ID);
                pvs.Add(-1, catePV);
            }
            if (famObjVer != null)
            {
                var famPV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.OwnedFamily]};
                famPV.Value.SetValue(MFDataType.MFDatatypeLookup, famObjVer.ID);
                pvs.Add(-1, famPV);
            }
            if (model.Parameters.Count > 0)
            {
                var paramPV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.ParamList]};
                paramPV.Value.SetValue(MFDataType.MFDatatypeMultiLineText,
                    String.Join("\r\n", model.Parameters.Select(c => c.ToString())));
                pvs.Add(-1, paramPV);
            }
            if (model.FamParameters.Count > 0)
            {
                var famParamPV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.FamParamList]};
                famParamPV.Value.SetValue(MFDataType.MFDatatypeMultiLineText,
                    String.Join("\r\n", model.FamParameters.Select(c => c.ToString())));
                pvs.Add(-1, famParamPV);
            }
            var modelPV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.OwnedModel]};
            modelPV.Value.SetValue(MFDataType.MFDatatypeLookup, modelId);
            pvs.Add(-1, modelPV);
            var objVer =
                _vault.ObjectOperations.CreateNewObject(objId, pvs, new SourceObjectFiles(), new AccessControlList())
                    .ObjVer;
            var objVers = new ObjVers();
            if (cateObjVer != null)
            {
                objVers.Add(-1, cateObjVer);
            }
            if (famObjVer != null)
            {
                objVers.Add(-1, famObjVer);
            }
            objVers.Add(-1, objVer);
            _vault.ObjectOperations.CheckInMultipleObjects(objVers);
        }

        

        private void OperateProjectModel()
        {
            if (!_model.IsProject)
            {
                throw new Exception("必须是项目模型");
            }
            var model = (ProjectModel)_model;
            var url = GetModelUrl();
            var mfProjModel = new MfProjectModel {Model = model, ModelUrl = url};
            FillModelProps(mfProjModel);
            mfProjModel.Run(_aliases);

        }

        private void FillModelProps(MfProjectModel model)
        {
            var modelAt = _aliases.PdDict[PD.ModelUnitAt];
            var floorAt = _aliases.PdDict[PD.FloorAt];
            var discAt = _aliases.PdDict[PD.DiscAt];
            var props = _vault.ObjectPropertyOperations.GetProperties(_obj.ObjVer);
            if (modelAt != -1)
            {
                var modelPV = props.SearchForPropertyEx(modelAt, true);
                if (modelPV != null && !modelPV.Value.IsNULL())
                {
                    model.UnitId = modelPV.Value.GetLookupID();
                }
            }
            if (floorAt != -1)
            {
                var floorPV = props.SearchForPropertyEx(floorAt, true);
                if (floorPV != null && !floorPV.Value.IsNULL())
                {
                    model.FloorId = floorPV.Value.GetLookupID();
                }
            }
            if (discAt != -1)
            {
                var discPV = props.SearchForPropertyEx(discAt, true);
                if (discPV != null && !discPV.Value.IsNULL())
                {
                    model.DiscId = discPV.Value.GetLookupID();
                }
            }
        }

        public void ToMf()
        {
            Initialize();

            if (_model == null)
            {
                throw new Exception("还未获取BIM数据");
            }

            if (_model.IsProject)
            {
                OperateProjectModel();
            }
            else
            {
                OperateFamilyModel();
            }

        }

        private PropertyValues GetAdditionalPropsForIFC()
        {
            var pvs = new PropertyValues();
            var modelAt = _aliases.PdDict[PD.ModelUnitAt];
            var floorAt = _aliases.PdDict[PD.FloorAt];
            var discAt = _aliases.PdDict[PD.DiscAt];
            var props = _vault.ObjectPropertyOperations.GetProperties(_obj.ObjVer);
            if (modelAt != -1)
            {
                var modelPV = props.SearchForPropertyEx(modelAt, true);
                if (modelPV != null)
                {
                    pvs.Add(-1, modelPV.Clone());
                }
            }
            if (floorAt != -1)
            {
                var floorPV = props.SearchForPropertyEx(floorAt, true);
                if (floorPV != null)
                {
                    pvs.Add(-1, floorPV.Clone());
                }
            }
            if (discAt != -1)
            {
                var discPV = props.SearchForPropertyEx(discAt, true);
                if (discPV != null)
                {
                    pvs.Add(-1, discPV.Clone());
                }
            }

            var modelUrlPD = _aliases.PdDict[PD.ModelUrl];
            if (modelUrlPD != -1)
            {
                var url = GetModelUrl();
                var pv = new PropertyValue {PropertyDef = modelUrlPD};
                pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, url);
                pvs.Add(-1, pv);
            }

            return pvs;
        }

        private ObjectVersion GetIfc()
        {
            var scs = new SearchConditions();

            var typeSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            typeSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup,
                (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
            scs.Add(-1, typeSC);

            if (_aliases.CsDict[CS.IfcModel] != -1)
            {
                var classSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
                classSC.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                classSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, _aliases.CsDict[CS.IfcModel]);
                scs.Add(-1, classSC);
            }

            var ownerSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            ownerSC.Expression.DataPropertyValuePropertyDef = _aliases.PdDict[PD.OwnedModel];
            ownerSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, _obj.ObjVer.ID);
            scs.Add(-1, ownerSC);

            var delSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            delSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSC.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSC);

            var namePattern = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeMatchesWildcardPattern };
            namePattern.Expression.SetFileValueExpression(MFFileValueType.MFFileValueTypeFileName);
            namePattern.TypedValue.SetValue(MFDataType.MFDatatypeText, "*.ifc");
            scs.Add(-1, namePattern);

            var res = _vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            if (res.Count != 1)
            {
                return null;
            }
            return res[1];
        }

        public void AddIfc(string ifcPath)
        {
            Initialize();

            var ifcObj = GetIfc();

            var pvs = GetAdditionalPropsForIFC();

            if (ifcObj != null)
            {
                ClientUtils.AddFiles(_vault, ifcObj, true, new[]{ifcPath}, pvs);
                return;
            }

            var ifcClassId = _aliases.CsDict[CS.IfcModel];
            if (ifcClassId == -1)
            {
                ifcClassId = (int) MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument;
            }
            var objClass = _vault.ClassOperations.GetObjectClass(ifcClassId);
            var nameProp = objClass.NamePropertyDef;

            var objType = objClass.ObjectType;

            var classPV = new PropertyValue {PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass};
            classPV.Value.SetValue(MFDataType.MFDatatypeLookup, ifcClassId);
            pvs.Add(-1, classPV);

            var namePV = new PropertyValue {PropertyDef = nameProp};
            namePV.Value.SetValue(MFDataType.MFDatatypeText, _name);
            pvs.Add(-1, namePV);


            var filesPV = new PropertyValue {PropertyDef = _aliases.PdDict[PD.OwnedModel]};
            filesPV.Value.SetValue(MFDataType.MFDatatypeLookup, _obj.ObjVer.ID);
            pvs.Add(-1, filesPV);

            var sf = new SourceObjectFile();
            sf.Extension = Path.GetExtension(ifcPath).TrimStart('.');
            sf.Title = _name;
            sf.SourceFilePath = ifcPath;


            _vault.ObjectOperations.CreateNewSFDObject(objType, pvs, sf, true, new AccessControlList());
        }

        private static readonly int MaxCount = 500;

        private List<MfProjectModel> GetBasicModels(ModelLists list, TraceSource log, string url)
        {
            var listWithoutParts = new ModelLists();
            listWithoutParts.Cates.AddRange(list.Cates);
            listWithoutParts.Fams.AddRange(list.Fams);
            listWithoutParts.Floors.AddRange(list.Floors);
            listWithoutParts.Mats.AddRange(list.Mats);
            listWithoutParts.Types.AddRange(list.Types);
            listWithoutParts.Views.AddRange(list.Views);
            var m = (ProjectModel)_model;
            var pModelList = new List<MfProjectModel>();
            if (m.Categories.Count + m.Families.Count + m.Levels.Count + m.Materials.Count + m.Types.Count +
                m.Views.Count < MaxCount)
            {
                var pModel = new MfProjectModel{ModelUrl = url};
                FillModelProps(pModel);
                var mm = new ProjectModel {Id = m.Id, Name = m.Name, Project = m.Project};
                mm.Categories.AddRange(m.Categories);
                mm.Families.AddRange(m.Families);
                mm.Levels.AddRange(m.Levels);
                mm.Views.AddRange(m.Views);
                mm.Materials.AddRange(m.Materials);
                mm.Types.AddRange(m.Types);
                pModel.Model = mm;
                pModel.Lists = listWithoutParts;
                pModelList.Add(pModel);
            }
            else
            {
                var pModel1 = new MfProjectModel{ModelUrl = url};
                FillModelProps(pModel1);
                var mm1 = new ProjectModel {Id = m.Id, Name = m.Name, Project = m.Project};
                mm1.Categories.AddRange(m.Categories);
                mm1.Families.AddRange(m.Families);
                mm1.Levels.AddRange(m.Levels);
                mm1.Views.AddRange(m.Views);
                mm1.Materials.AddRange(m.Materials);
                
                pModel1.Model = mm1;
                pModel1.Lists = listWithoutParts;
                pModelList.Add(pModel1);

                var pModel = new MfProjectModel{ModelUrl = url};
                FillModelProps(pModel);
                var mm = new ProjectModel();
                mm.Id = m.Id;
                mm.Name = m.Name;
                mm.Project = m.Project;
                mm.Types.AddRange(m.Types);
                
                pModel.Model = mm;
                pModel.Lists = listWithoutParts;
                pModelList.Add(pModel);
                
            }

            return pModelList;
        }

        private MfProjectModel GetBasicModel(ModelLists list, string url)
        {
            var m = (ProjectModel)_model;
            var pModel = new MfProjectModel{ModelUrl = url};
            FillModelProps(pModel);
            var mm = new ProjectModel();
            mm.Categories.AddRange(m.Categories);
            mm.Families.AddRange(m.Families);
            mm.Id = m.Id;
            mm.Levels.AddRange(m.Levels);
            mm.Materials.AddRange(m.Materials);
            mm.Name = m.Name;
            mm.Project = m.Project;
            mm.Types.AddRange(m.Types);
            mm.Views.AddRange(m.Views);
            pModel.Model = mm;
            pModel.Lists = list;
            return pModel;
        }

        private List<MfProjectModel> GetPartModel(MfModelDicts dicts, ModelLists list, TraceSource ts, string url)
        {
            var ms = new List<MfProjectModel>();
            var m = (ProjectModel)_model;
            var group = MfProjectModel.GroupArrays(m.Elements, MaxCount);
            ts.TraceInformation("分组个数：" + group.Count);
            foreach (var g in group)
            {
                ts.TraceInformation("每组个数：" + g.Count);
                var pModel = new MfProjectModel { Dicts = dicts, ModelUrl = url };
                FillModelProps(pModel);
                var mm = new ProjectModel {Id = m.Id, Name = m.Name, Project = m.Project};
                mm.Elements.AddRange(g);
                pModel.Model = mm;
                pModel.Lists = list;
                ms.Add(pModel);
            }
            
            return ms;
        }

        private static readonly string VaultExtensionMethodName = "BimInfo";

        public string ServerRun(Func<object, string> toJson, Func<string, MfModelDicts> toDict, string modelUrl )
        {
            var hasExtensionMethod = 
                _vault.ExtensionMethodOperations.DoesActiveVaultExtensionMethodExist(VaultExtensionMethodName);
            if (!hasExtensionMethod)
            {
                return "服务器未安装或启用扩展方法：" + VaultExtensionMethodName;
            }
            var log = MfProjectModel.GetTrace<MfModel>();
            var list = ((ProjectModel) _model).GetLists(); //总的模型数据(元素唯一标识)列表

            //var basicModel = GetBasicModel(list);
            //var basicContent = ModelUtility.GetZippedContent(basicModel, toJson);
            //var basicInfo = _vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(VaultExtensionMethodName, basicContent);
            //var dict = ModelUtility.FromZippedContent<MfModelDicts>(basicInfo, toDict);

            var basicInfo = String.Empty;
            var partInfo = String.Empty;
            var basicDict = new MfModelDicts();
            try
            {

                var bms = GetBasicModels(list, log, modelUrl);
                
                foreach (var bm in bms)
                {
                    var basicContent = ModelUtility.GetZippedContent(bm, toJson);
                    basicInfo = _vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(VaultExtensionMethodName, basicContent);
                    try
                    {
                        var pDict = ModelUtility.FromZippedContent<MfModelDicts>(basicInfo, toDict);
                        basicDict.AddFrom(pDict);

                    }
                    catch (Exception e)
                    {
                        var err = ModelUtility.FromZippedContent(basicInfo, s => s);
                        log.TraceEvent(TraceEventType.Warning, 0, err + "\r\n" + e.Message);
                        return err;
                    }
                }

                
                var partModels = GetPartModel(basicDict, list, log, modelUrl);

                foreach (var partModel in partModels)
                {
                    var partContent = ModelUtility.GetZippedContent(partModel, toJson);
                    partInfo = _vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(VaultExtensionMethodName, partContent);
                    try
                    {
                        var pDict = ModelUtility.FromZippedContent<MfModelDicts>(partInfo, toDict);

                    }
                    catch (Exception e)
                    {
                        var err = ModelUtility.FromZippedContent(partInfo, s => s);
                        log.TraceEvent(TraceEventType.Warning, 0, err + "\r\n" + e.Message);
                        return  err;
                    }
                }

                return String.Empty;
            }
            catch (Exception ex)
            {
                var errInfo = basicInfo;
                if (!String.IsNullOrEmpty(partInfo))
                {
                    errInfo = partInfo;
                }
                var err = ModelUtility.FromZippedContent(errInfo, s => s);
                log.TraceEvent(TraceEventType.Warning, 0, err+"\r\n"+ ex.Message);
                return err;
            }
            finally
            {
                log.Close();
            }
        }
    }
}
