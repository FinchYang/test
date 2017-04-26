using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MFilesAPI;
using SimulaDesign.BimInfo;

namespace SimulaDesign.MfBimInfo
{
    public class MfProjectModel
    {
        /// <summary>
        /// 所在单体
        /// </summary>
        public int? UnitId { get; set; }
        /// <summary>
        /// 所在楼层
        /// </summary>
        public int? FloorId { get; set; }
        /// <summary>
        /// 所在专业
        /// </summary>
        public int? DiscId { get; set; }
        public ProjectModel Model { get; set; }

        public MfModelDicts Dicts { get; set; }

        public ModelLists Lists { get; set; }

        private static readonly int MaxSearchCount = 500;

        public int MaxCount { get; set; }

        public string ModelUrl;

        public MfProjectModel()
        {
            MaxCount = 500;
            Dicts = new MfModelDicts();
        }

        private ModelLists GetLists()
        {
            return Lists ?? (Lists = Model.GetLists());
        }

        public MfModelDicts Run(VaultAliases aliases)
        {
            if (Model == null)
            {
                throw new Exception("未指定模型！");
            }
            var modelId = Model.Id;

            var list = GetLists();

            var idKey = aliases.PdDict[PD.Id];
            var guidKey = aliases.PdDict[PD.Guid];

            //处理构件类型
            if (Model.Types.Count > 0)
            {
                var typeId = aliases.ObDict[OB.PartType];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                OperateElements(aliases, typeId, guidKey, Model.Types, Dicts.Types, list.Types
                , t => t.CreatePartType(aliases, modelId, acl), t => t.UpdatePartType(aliases, modelId));

            }
            
            //处理类别
            if (Model.Categories.Count > 0)
            {
                var typeId = aliases.ObDict[OB.Category];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                OperateElements(aliases, typeId, idKey, Model.Categories, Dicts.Cates, list.Cates
                , c => c.CreateCategory(aliases, modelId, acl), c => c.UpdateCategory(aliases, modelId));
            }
            

            //处理楼层
            if (Model.Levels.Count > 0)
            {
                var typeId = aliases.ObDict[OB.Level];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                OperateElements(aliases, typeId, guidKey, Model.Levels, Dicts.Floors, list.Floors
                , fl => fl.CreateFloor(aliases, modelId, acl), fl => fl.UpdateFloor(aliases, modelId));
            }
            

            //处理材料
            if (Model.Materials.Count > 0)
            {
                var typeId = aliases.ObDict[OB.Material];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                OperateElements(aliases, typeId, guidKey, Model.Materials, Dicts.Mats, list.Mats
                , m => m.CreateMaterial(aliases, modelId, acl), m => m.UpdateMaterial(aliases, modelId));
            }
            

            //处理视图
            if (Model.Views.Count > 0)
            {
                var typeId = aliases.ObDict[OB.View];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                Func<ViewElement, ObjVer> viewCreateFunc = v =>
                {
                    var lId = 0;
                    if (v.GenLevel != null)
                    {
                        lId = Dicts.Floors[v.GenLevel];
                    }
                    var vId = v.CreateView(aliases, lId, modelId, acl);
                    return vId;
                };

                Func<ViewElement, PropertyValues> viewUpdateFunc = v =>
                {
                    var lId = 0;
                    if (v.GenLevel != null)
                    {
                        lId = Dicts.Floors[v.GenLevel];
                    }
                    var vId = v.UpdateView(aliases, lId, modelId);
                    return vId;
                };
                OperateElements(aliases, typeId, guidKey, 
                    Model.Views, Dicts.Views, list.Views, viewCreateFunc, viewUpdateFunc);
            }
            


            //处理族
            if (Model.Families.Count > 0)
            {
                var typeId = aliases.ObDict[OB.Family];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(typeId);
                var acl = objType.AccessControlList;
                Func<ElementFamily, ObjVer> famCreateFunc = f =>
                {
                    var cateId = 0;
                    if (f.Category != null)
                    {
                        cateId = Dicts.Cates[f.Category.GetKey()];
                    }
                    return f.CreateFamily(aliases, cateId, modelId, acl);
                };
                Func<ElementFamily, PropertyValues> famUpdateFunc = f =>
                {
                    var cateId = 0;
                    if (f.Category != null)
                    {
                        cateId = Dicts.Cates[f.Category.GetKey()];
                    }
                    return f.UpdateFamily(aliases, cateId, modelId);
                };
                OperateElements(aliases, typeId, guidKey, 
                    Model.Families, Dicts.Fams, list.Fams, famCreateFunc, famUpdateFunc);
            }
            

            //处理构件
            if (Model.Elements.Count > 0)
            {
                var partObjId = aliases.ObDict[OB.Part];
                var objType = aliases.Vault.ObjectTypeOperations.GetObjectType(partObjId);
                var acl = objType.AccessControlList;
                Func<Element, ObjVer> partCreateFunc = p =>
                {
                    int cateId = Dicts.Cates[p.Category.GetKey()];
                    int typeId = Dicts.Types[p.ElemType];
                    int matId = 0;
                    if (p.Material != null)
                    {
                        matId = Dicts.Mats[p.Material];
                    }
                    int levelId = 0;
                    if (p.Level != null)
                    {
                        levelId = Dicts.Floors[p.Level];
                    }
                    int famId = 0;
                    if (p.Family != null)
                    {
                        famId = Dicts.Fams[p.Family];
                    }
                    return p.CreatePart(aliases, cateId, typeId, matId, levelId, famId, modelId, acl, ModelUrl, UnitId, FloorId, DiscId);
                };

                Func<Element, PropertyValues> partUpdateFunc = p =>
                {
                    int cateId = Dicts.Cates[p.Category.GetKey()];
                    int typeId = Dicts.Types[p.ElemType];
                    int matId = 0;
                    if (p.Material != null)
                    {
                        matId = Dicts.Mats[p.Material];
                    }
                    int levelId = 0;
                    if (p.Level != null)
                    {
                        levelId = Dicts.Floors[p.Level];
                    }
                    int famId = 0;
                    if (p.Family != null)
                    {
                        famId = Dicts.Fams[p.Family];
                    }
                    return p.UpdatePart(aliases, cateId, typeId, matId, levelId, famId, modelId, ModelUrl, UnitId, FloorId, DiscId);
                };

                var log = GetTrace<Element>();
                try
                {
                    log.TraceInformation("类别词典：" + Dicts.Cates.Count);
                    log.TraceInformation("类型词典：" + Dicts.Types.Count);
                    log.TraceInformation("材料词典：" + Dicts.Mats.Count);
                    log.TraceInformation("楼层词典：" + Dicts.Floors.Count);
                    log.TraceInformation("族词典：  " + Dicts.Fams.Count);
                    log.TraceInformation("构件个数：  " + Model.Elements.Count);
                    OperateElements(aliases, partObjId, guidKey, 
                        Model.Elements, Dicts.Elems, list.Elems, partCreateFunc, partUpdateFunc);
                }
                catch (Exception ex)
                {
                    log.TraceEvent(TraceEventType.Error, 0, "构件个数：" + Model.Elements.Count + "; " + ex.Message);
                    throw;
                }
                finally
                {
                    log.Close();
                }
            }
            return Dicts;
        }
        public static TraceSource GetTrace<T>()
        {
            Trace.AutoFlush = true;
            var ts = new TraceSource(typeof(T).Name, SourceLevels.Information);
            var tempPath = Path.GetTempPath();

            ts.Listeners.Add(
                new CustomTextListener(Path.Combine(tempPath, "mfmodel" + DateTime.Now.ToString("yyyy-MM-dd") + ".log")));
            return ts;
        }
        internal static List<List<T>> GroupArrays<T>(List<T> objIds, int mCount)
        {
            if (objIds.Count <= mCount)
            {
                return new List<List<T>> { objIds };
            }
            var count = objIds.Count / mCount;
            var list = new List<List<T>>();
            var startIndex = 0;
            for (var i = 0; i < count; i++)
            {
                var os = objIds.Skip(startIndex).Take(mCount).ToList();
                list.Add(os);
                startIndex += mCount;
            }
            var dCount = objIds.Count - count * mCount;
            if (dCount > 0)
            {
                var os = objIds.Skip(startIndex).Take(dCount).ToList();
                list.Add(os);
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vaultAlias"></param>
        /// <param name="objType"></param>
        /// <param name="keyDef"></param>
        /// <param name="objs">要处理的对象列表</param>
        /// <param name="objDict">已处理的对象词典</param>
        /// <param name="objList">所有的对象集合</param>
        /// <param name="createFunc"></param>
        /// <param name="updateFunc"></param>
        private void OperateElements<T>(VaultAliases vaultAlias, int objType, int keyDef, List<T> objs, Dictionary<string, int> objDict
            , List<string> objList, Func<T, ObjVer> createFunc, Func<T, PropertyValues> updateFunc) where T : BaseElement
        {
            var ts = GetTrace<T>();
            var vault = vaultAlias.Vault;
            try
            {
                var objsHas = GetElements(vaultAlias, objType, keyDef, objList.Count); //获取已有的对象词典
                List<T> createObjs, updateObjs;
                var updateDict = new Dictionary<string, ObjVer>();
                if (objsHas.Count > 0)
                {
                    createObjs = new List<T>();
                    updateObjs = new List<T>();
                    var delObjs = SplitElements(objs, objsHas, objList, createObjs, updateObjs, updateDict);
                    if (delObjs.Count > 0)
                    {
                        foreach (var ov in delObjs)
                        {
                            vault.ObjectOperations.DeleteObject(ov.ObjID);
                        }
                    }
                }
                else
                {
                    createObjs = objs;
                    updateObjs = new List<T>();
                }
                //var objVers = new ObjVers();
                if (updateObjs.Count > 0)
                {
                    ts.TraceInformation("UpdateElementsWithParams: " + updateObjs.Count);
                    try
                    {
                        var uObjGrp = GroupArrays(updateObjs, MaxCount);
                        foreach (var ug in uObjGrp)
                        {
                            var objsU = UpdateElementsWithParams(vault, ug, objDict, updateDict, updateFunc);
                            var res = vault.ObjectPropertyOperations.SetPropertiesOfMultipleObjects(objsU);
                            var objVers = new ObjVers();
                            foreach (ObjectVersionAndProperties vp in res)
                            {
                                objVers.Add(-1, vp.ObjVer);
                            }
                            vault.ObjectOperations.CheckInMultipleObjects(objVers);
                        }

                    }
                    catch (Exception ex)
                    {
                        ts.TraceEvent(TraceEventType.Error, 0, "UpdateElementsWithParams：" + updateObjs.Count + "; 错误：" + ex.Message);
                        throw;
                    }

                }
                if (createObjs.Count > 0)
                {
                    ts.TraceInformation("CreateElementsWithParams: " + createObjs.Count);
                    try
                    {
                        var cObjGrp = GroupArrays(createObjs, MaxCount);
                        foreach (var ug in cObjGrp)
                        {
                            var objsC = CreateElementsWithParams(ug, objDict, createFunc);
                            var objVers = new ObjVers();
                            foreach (ObjVer v in objsC)
                            {
                                objVers.Add(-1, v);
                            }
                            vault.ObjectOperations.CheckInMultipleObjects(objVers);
                        }

                    }
                    catch (Exception ex)
                    {
                        ts.TraceEvent(TraceEventType.Error, 0, "CreateElementsWithParams：" + createObjs.Count + "; 错误：" + ex.Message);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, "需要创建或更新的对象个数：" + objs.Count + "; 错误：" + ex.Message);
                throw;
            }
            finally
            {
                ts.Close();
            }
        }

        private static ObjVers CreateElementsWithParams<T>(List<T> list,
            Dictionary<string, int> idDict, Func<T, ObjVer> createFunc) where T : BaseElement
        {
            var objVers = new ObjVers();

            foreach (var t in list)
            {
                var o = t;
                var info = createFunc(o);
                var tId = info.ID;
                if (idDict != null)
                {
                    var key = o.GetKey();
                    //if (idDict.ContainsKey(key))
                    //{
                    //    throw new Exception("已存在此构件：" + key);
                    //}
                    idDict.Add(key, tId);
                }
                objVers.Add(-1, info);
            }


            return objVers;
        }


        private static SetPropertiesParamsOfMultipleObjects UpdateElementsWithParams<T>(Vault vault, List<T> list
            , Dictionary<string, int> idDict, Dictionary<string, ObjVer> objVerDict,
            Func<T, PropertyValues> updateFunc) where T : BaseElement
        {
            var objVers = new SetPropertiesParamsOfMultipleObjects();

            foreach (var tt in list)
            {
                var t = tt;
                var info = updateFunc(t);
                var setProps = new SetPropertiesParams();
                var key = t.GetKey();
                var objID = objVerDict[key].ObjID;
                var mfId = objID.ID;
                var objVer = vault.ObjectOperations.CheckOut(objID).ObjVer;
                setProps.ObjVer = objVer;
                setProps.PropertyValuesToSet = info;
                objVers.Add(-1, setProps);
                if (idDict != null)
                {
                    idDict.Add(key, mfId);
                }
            }


            return objVers;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs">本次需要操作的对象列表</param>
        /// <param name="objsHas">已存在的此类型对象的词典</param>
        /// <param name="objList">所有的对象(唯一标识)列表</param>
        /// <param name="createObjs">创建对象列表，需要先初始化</param>
        /// <param name="updateObjs">更新对象列表，需要先初始化</param>
        /// <param name="updateObjVers">需要更新的对象词典，需要先初始化</param>
        /// <returns></returns>
        private static List<ObjVer> SplitElements<T>(IList<T> objs
            , Dictionary<string, List<ObjVer>> objsHas, List<string> objList
            , List<T> createObjs, List<T> updateObjs, Dictionary<string,ObjVer> updateObjVers)
            where T : BaseElement
        {
            foreach (var o in objs)
            {
                var id = o.GetKey();
                if (objsHas.ContainsKey(id))
                {
                    updateObjs.Add(o);
                }
                else
                {
                    createObjs.Add(o);
                }
            }
            var delList = new List<ObjVer>();
            foreach (var oh in objsHas)
            {
                if (objList.Contains(oh.Key))
                {
                    var val = objsHas[oh.Key];
                    var up = val[val.Count-1];
                    updateObjVers.Add(oh.Key, up);
                    if (val.Count > 1)
                    {
                        delList.AddRange(val.Take(val.Count - 1));
                    }
                }
                else
                {
                    delList.AddRange(oh.Value);
                }
            }
            //var delObjs = (from oh in objsHas where objs.All(c => c.GetKey() != oh.Key && !objList.Contains(oh.Key)) select oh.Value).ToList();
            return delList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultAlias"></param>
        /// <param name="objType"></param>
        /// <param name="keyDef">唯一标识的属性定义ID</param>
        /// <param name="objCount">期望的对象个数</param>
        /// <returns>所有存在的对象</returns>
        private Dictionary<string, List<ObjVer>> GetElements(VaultAliases vaultAlias, int objType, int keyDef, int objCount)
        {
            var dict = new Dictionary<string, List<ObjVer>>();
            var res = SearchObjects(vaultAlias, objType, objCount);
            var guidDef = keyDef;//_aliases.PdDict[PD.Guid];

            var objVers = new ObjVers();

            foreach (ObjectVersion ver in res)
            {
                objVers.Add(-1, ver.ObjVer);
            }
            var vault = vaultAlias.Vault;
            var mm = vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objVers);
            var hasDict = new Dictionary<string, List<ObjVer>>();
            for (var i = 0; i < mm.Count; i++)
            {
                var j = i + 1;
                var guid = mm[j].SearchForProperty(guidDef).GetValueAsLocalizedText();
                if (dict.ContainsKey(guid))
                {
                    dict[guid].Add(objVers[j]);
                }
                else
                {
                    dict.Add(guid, new List<ObjVer> { objVers[j] });
                }
                
            }
            return dict;
        }

        /// <summary>
        /// 根据类型和所属模型搜索构件、类别、类型、楼层、材料等
        /// </summary>
        /// <param name="aliases"></param>
        /// <param name="objType"></param>
        /// <param name="objCount">期望的对象个数</param>
        /// <returns></returns>
        private ObjectSearchResults SearchObjects(VaultAliases aliases, int objType, int objCount)
        {
            var scs = new SearchConditions();

            var typeSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            typeSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, objType);
            scs.Add(-1, typeSc);

            var ownerSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            ownerSc.Expression.DataPropertyValuePropertyDef = aliases.PdDict[PD.OwnedModel];
            ownerSc.TypedValue.SetValue(MFDataType.MFDatatypeLookup, Model.Id);
            scs.Add(-1, ownerSc);

            var delSc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            delSc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSc);
            var vault = aliases.Vault;
            if (objCount < MaxSearchCount)
            {
                return vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                    false);
            }
            else
            {
                return vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone,
                    false, objCount*3, 120);
            }
        }
        
    }

    

    public class MfModelDicts
    {
        public void AddFrom(MfModelDicts dict)
        {
            foreach (var c in dict.Cates)
            {
                if (!Cates.ContainsKey(c.Key))
                {
                    Cates.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Elems)
            {
                if (!Elems.ContainsKey(c.Key))
                {
                    Elems.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Fams)
            {
                if (!Fams.ContainsKey(c.Key))
                {
                    Fams.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Floors)
            {
                if (!Floors.ContainsKey(c.Key))
                {
                    Floors.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Mats)
            {
                if (!Mats.ContainsKey(c.Key))
                {
                    Mats.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Types)
            {
                if (!Types.ContainsKey(c.Key))
                {
                    Types.Add(c.Key, c.Value);
                }
            }
            foreach (var c in dict.Views)
            {
                if (!Views.ContainsKey(c.Key))
                {
                    Views.Add(c.Key, c.Value);
                }
            }
        }
        public MfModelDicts()
        {
            Cates = new Dictionary<string, int>();
            Fams = new Dictionary<string, int>();
            Floors = new Dictionary<string, int>();
            Mats = new Dictionary<string, int>();
            Types = new Dictionary<string, int>();
            Views = new Dictionary<string, int>();
            Elems = new Dictionary<string, int>();
        }
        /// <summary>
        /// 楼层词典
        /// </summary>
        public Dictionary<string, int> Floors { get; set; }
        /// <summary>
        /// 视图词典
        /// </summary>
        public Dictionary<string, int> Views { get; set; }
        /// <summary>
        /// 材料词典
        /// </summary>
        public Dictionary<string, int> Mats { get; set; }
        /// <summary>
        /// 构件类别词典
        /// </summary>
        public Dictionary<string, int> Cates { get; set; }
        /// <summary>
        /// 构件类型词典
        /// </summary>
        public Dictionary<string, int> Types { get; set; }
        /// <summary>
        /// 构件族词典
        /// </summary>
        public Dictionary<string, int> Fams { get; set; }
        /// <summary>
        /// 构件词典
        /// </summary>
        public Dictionary<string, int> Elems { get; set; }


    }
}
