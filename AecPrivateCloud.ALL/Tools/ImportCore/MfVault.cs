using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using MFilesAPI;

namespace SimulaDesign.ImportCore
{
    [XmlRoot]
    public class MfVault
    {
        private Vault _vault;

        public static bool TestLogin(string vaultName)
        {
            try
            {
                var vault = App.BindToVault(vaultName, IntPtr.Zero, true, true);
                return vault != null;
            }
            catch
            {
            }
            return false;
        }

        public string Guid { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Only for Serialize
        /// </summary>
        public MfVault()
        {
            
        }

        private MfVault(string vaultName)
        {
            Name = vaultName;
        }

        private List<ClassPropSets> _propSets = new List<ClassPropSets>();

        public List<ClassPropSets> PropSets
        {
            get { return _propSets; }
        }

        public ClassPropSets GetLastMapping()
        {
            return _propSets.FirstOrDefault(c => c.LastUpdated);
            
        }

        public static List<string> GetVaultList()
        {
            return App.GetVaultConnections().OfType<VaultConnection>().Select(c=>c.Name).ToList();
        }

        private static readonly MFilesClientApplication App = new MFilesClientApplication();

        private void Initialize()
        {
            _vault = App.BindToVault(Name, IntPtr.Zero, true, true);
            Guid = _vault.GetGUID();
        }

        public static MfVault GetVault(string name)
        {
            var mfVault = new MfVault(name);
            mfVault.Initialize();
            return mfVault;
        }

        public void Save(string fileFolder)
        {
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }
            var filePath = Path.Combine(fileFolder, Guid.TrimStart('{').TrimEnd('}') + ".xml");
            var ser = new XmlSerializer(GetType());
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                ser.Serialize(fs, this);
            }
        }

        public static MfVault GetFromConfig(string fileFolder, string vaultName)
        {
            var vault = App.BindToVault(vaultName, IntPtr.Zero, true, true);
            if (vault != null)
            {
                var guid = vault.GetGUID();
                var filePath = Path.Combine(fileFolder, guid.TrimStart('{').TrimEnd('}') + ".xml");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var ser = new XmlSerializer(typeof(MfVault));
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var obj = ser.Deserialize(fs) as MfVault;
                            if (obj != null)
                            {
                                obj._vault = vault;
                                return obj;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.Copy(filePath, filePath + ".bk"+DateTime.Now.ToString("MMddHHmm"), true); //备份出错的配置文件
                        File.Delete(filePath);
                        var log = TraceLog.GetLogger<MfVault>();
                        log.TraceEvent(TraceEventType.Error, 0, String.Format("读取配置文件({0})失败："+ ex.Message, filePath));
                        log.Close();
                    }
                }
            }
            return null;
        }

        public void AddOrUpdateClassPropSet(MfObjType objType, MfClass mfClass, List<MfClassPropDef> props)
        {
            var set = _propSets.FirstOrDefault(c => c.ObjectClass.Equals(mfClass));
            if (set != null)
            {
                _propSets.Remove(set);
            }
            set = new ClassPropSets {ObjType =objType, ObjectClass = mfClass, LastUpdated = true};
            set.Props.AddRange(props);
            _propSets.Add(set);
        }

        private List<MfObjType> _objTypes;

        public List<MfObjType> GetObjectTypes(bool withDoc=false)
        {
            if (_objTypes == null)
            {
                _objTypes = new List<MfObjType>();
                var ots = _vault.ObjectTypeOperations.GetObjectTypes();
                foreach (ObjType ot in ots)
                {
                    if ( (ot.ID == (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument 
                        || ot.ID == (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocumentCollection)
                        && !withDoc) continue;
                    _objTypes.Add(new MfObjType {Id = ot.ID, Name = ot.NameSingular});
                }

            }
            return _objTypes;
        }

        private SortedList<int, List<MfClass>> _objTypesWithClasses = new SortedList<int, List<MfClass>>();

        public List<MfClass> GetClasses(int objType)
        {
            if (!_objTypesWithClasses.ContainsKey(objType))
            {
                var csList = new List<MfClass>();
                var cs = _vault.ClassOperations.GetObjectClasses(objType);
                foreach (ObjectClass oc in cs)
                {
                    var mfC = new MfClass {Id = oc.ID, Name = oc.Name};
                    csList.Add(mfC);
                }
                _objTypesWithClasses.Add(objType, csList);
            }
            return _objTypesWithClasses[objType];
        }

        private MfObjType _docType;

        public MfObjType GetDocType()
        {
            if (_docType == null)
            {
                var ot =
                    _vault.ObjectTypeOperations.GetBuiltInObjectType(MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
                _docType = new MfObjType {Id = ot.ID, Name = ot.NameSingular};
            }
            return _docType;
        }

        private List<MfClass> _docClasses;

        public List<MfClass> GetDocClasses(bool withBuiltin=false)
        {
            if (_docClasses == null)
            {
                var objClasses = 
                    _vault.ClassOperations.GetObjectClasses((int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
                if (withBuiltin)
                {
                    _docClasses =
                        objClasses.OfType<ObjectClass>().Select(c => new MfClass {Id = c.ID, Name = c.Name}).ToList();
                }
                else
                {
                    var builtIns = Enum.GetValues(typeof(MFBuiltInDocumentClass)).OfType<MFBuiltInDocumentClass>().Select(c=>(int)c).ToList();
                    _docClasses =
                        objClasses.OfType<ObjectClass>().Where(c=>!builtIns.Contains(c.ID) ).Select(
                        c => new MfClass { Id = c.ID, Name = c.Name }).ToList();
                }
                
            }
            return _docClasses;
        }

        public List<MfClassPropDef> GetClassProps(int classId)
        {
            return ClassProps.GetClassProps(_vault, classId);
        }

        public List<MfClassPropDef> GetMappedProps(int classId)
        {
            var ps = _propSets.FirstOrDefault(c => c.ObjectClass.Id == classId);
            if (ps != null)
            {
                return ps.Props;
            }
            return null;
        }

        private static SourceObjectFile GetFileObject(string rootPath, SelectedFile file)
        {
            var sourcePath = Path.Combine(rootPath, file.Filepath);
            var ext = Path.GetExtension(sourcePath);
            ext = ext.TrimStart('.');
            var sof = new SourceObjectFile {Title = file.NewFilename, SourceFilePath = sourcePath, Extension = ext};

            return sof;
        }

        private PropertyValues GetCreationPropValues(int classId, SelectedFiles files, SelectedFile file,
            List<MfClassPropDef> selProps, List<string> addedPropValues, TraceSource log)
        {
            var pvs = new PropertyValues();

            var classPV = new PropertyValue {PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass};
            classPV.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            pvs.Add(-1, classPV);

            var values = files.GetProps(file);
            for (var i = 0; i < values.Count; i++)
            {
                var vv = values[i];
                if (i == 0)
                {
                    var dotIndex = vv.LastIndexOf('.');
                    vv = vv.Substring(0, dotIndex);
                }
                var v = GetValue(selProps[i].PropDef, vv, log);
                if (v != null)
                {
                    var pv = new PropertyValue {PropertyDef = selProps[i].Def};
                    pv.TypedValue.SetValue((MFDataType) selProps[i].DataType, v);
                    pvs.Add(-1, pv);
                }
            }
            var diff = selProps.Count - values.Count;
            if (diff > 0)
            {
                for (var i = 0; i < diff; i++)
                {
                    var p = selProps[i + values.Count];
                    var vv = addedPropValues[i];
                    var v = GetValue(p.PropDef, vv, log);
                    if (v != null)
                    {
                        var pv = new PropertyValue { PropertyDef = p.Def };
                        pv.TypedValue.SetValue((MFDataType)p.DataType, v);
                        pvs.Add(-1, pv);
                    }
                    else if (p.Required)
                    {
                        
                    }
                    //var pv = new PropertyValue {PropertyDef = p.Def};
                    //if (String.IsNullOrEmpty(v))
                    //{
                    //    pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeText);
                    //}
                    //else
                    //{
                    //    pv.TypedValue.SetValue(MFDataType.MFDatatypeText, v);
                    //}
                    //pvs.Add(-1, pv);
                }
            }
            return pvs;
        }

        public string ObjectToSystem(int objType, MfClass mfClass, List<MfClassPropDef> selProps, List<string> propValues)
        {
            var err = String.Empty;
            var log = TraceLog.GetLogger<MfVault>(MethodBase.GetCurrentMethod().Name);
            var classId = mfClass.Id;
            try
            {
                var scs = new SearchConditions();
                for (var i = 0; i < selProps.Count; i++)
                {
                    var p = selProps[i];
                    var vv = propValues[i];
                    try
                    {
                        var sc = GetCondition(p, vv, log);
                        scs.Add(-1, sc);
                    }
                    catch (Exception ex)
                    {
                        err = "创建对象失败：" + ex.Message; //files.GetFullpath(f.Filepath) + " # " + 
                        log.TraceEvent(TraceEventType.Error, 0, err);
                        return err;
                    }
                }
                var classSC = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
                classSC.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                classSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
                scs.Add(-1, classSC);

                var res = _vault.ObjectSearchOperations.SearchForObjectsByConditions(scs,
                    MFSearchFlags.MFSearchFlagNone, false).GetAsObjectVersions();
                if (res.Count > 0)
                {
                    err = "对象已存在：" + res[1].Title;
                    log.TraceEvent(TraceEventType.Warning, 0, err);
                    return err;
                }
                else
                {
                    try
                    {
                        var pvs = new PropertyValues();
                        for (var i = 0; i < selProps.Count; i++)
                        {
                            var p = selProps[i];
                            var vv = propValues[i];
                            var val = GetValue(p.PropDef, vv, log);
                            var pv = new PropertyValue {PropertyDef = p.Def};
                            if (val == null)
                            {
                                pv.Value.SetValueToNULL((MFDataType) p.DataType);
                            }
                            else
                            {
                                pv.Value.SetValue((MFDataType) p.DataType, val);
                            }
                            pvs.Add(-1, pv);
                        }
                        var cPV = new PropertyValue {PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass};
                        cPV.Value.SetValue(MFDataType.MFDatatypeLookup, classId);
                        pvs.Add(-1, cPV);
                        var obj =
                            _vault.ObjectOperations.CreateNewObject(objType, pvs);
                        _vault.ObjectOperations.CheckIn(obj.ObjVer);
                        log.TraceInformation("创建对象：" + obj.VersionData.Title);
                        return err;
                    }
                    catch (Exception ex)
                    {
                        err = "创建对象失败：" + ex.Message; //files.GetFullpath(f.Filepath) + " # " + 
                        log.TraceEvent(TraceEventType.Error, 0, err);
                        return err;
                    }
                }
            }
            catch (Exception ex)
            {
                err = "属性值有误：" + ex.Message;
                log.TraceEvent(TraceEventType.Error, 0, err);
                return err;
            }
            finally
            {
                log.Close();
            }
        }

        public string FileToSystem(SelectedFiles files, SelectedFile f, MfClass mfClass, List<MfClassPropDef> selProps, List<string> addedPropValues)
        {
            var err = String.Empty;
            var log = TraceLog.GetLogger<MfVault>(MethodBase.GetCurrentMethod().Name);
            var classId = mfClass.Id;
            try
            {
                var scs = GetSearchPropValues(files, f, selProps, addedPropValues, log);
                var classSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
                classSC.Expression.DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                classSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
                scs.Add(-1, classSC);

                var ext = Path.GetExtension(f.Filepath) ?? String.Empty;
                ext = ext.TrimStart('.');
                if (!String.IsNullOrEmpty(ext))
                {
                    var namePattern = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeMatchesWildcardPattern
                    };
                    namePattern.Expression.SetFileValueExpression(MFFileValueType.MFFileValueTypeFileName);
                    namePattern.TypedValue.SetValue(MFDataType.MFDatatypeText, "*." + ext);
                    scs.Add(-1, namePattern);
                }
                var res = _vault.ObjectSearchOperations.SearchForObjectsByConditions(scs,
                    MFSearchFlags.MFSearchFlagNone, false).GetAsObjectVersions();
                if (res.Count > 0)
                {
                    err = "对象已存在：" + res[1].Title;
                    log.TraceEvent(TraceEventType.Warning, 0, err);
                    return err;
                }
                else
                {
                    try
                    {
                        var pvs = GetCreationPropValues(classId, files, f, selProps, addedPropValues, log);
                        var sof = GetFileObject(files.RootDir, f);
                        var obj =
                            _vault.ObjectOperations.CreateNewSFDObject(
                                (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument
                                , pvs, sof, true, new AccessControlList());
                        log.TraceInformation("创建对象：" + obj.VersionData.Title);
                        return err;
                    }
                    catch (Exception ex)
                    {
                        err = "创建对象失败：" + ex.Message; //files.GetFullpath(f.Filepath) + " # " + 
                        log.TraceEvent(TraceEventType.Error, 0, err);
                        return err;
                    }
                }
            }
            catch (Exception ex)
            {
                err = "属性值有误："+ex.Message;
                log.TraceEvent(TraceEventType.Error, 0, err);
                return err;
            }
            finally
            {
                log.Close();
            }
        }
        ///// <summary>
        ///// 写入MF系统
        ///// </summary>
        ///// <param name="files">选择的文件集合</param>
        ///// <param name="mfClass">对象类别</param>
        ///// <param name="selProps">选择的对应属性</param>
        //public void ToSystem(SelectedFiles files, MfClass mfClass, List<MfClassPropDef> selProps)
        //{
        //    var log = TraceLog.GetLogger<MfVault>(MethodBase.GetCurrentMethod().Name);
        //    try
        //    {
        //        foreach (var f in files.Files)
        //        {
        //            FileToSystem(files, f, mfClass, selProps);
        //        }
        //    }
        //    finally
        //    {
        //        log.Close();
        //    }
            
        //}

        private SearchCondition GetCondition(MfClassPropDef propDef, string vv, TraceSource log)
        {
            var def = propDef.Def;
            var sc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
            sc.Expression.DataPropertyValuePropertyDef = def;
            if (!String.IsNullOrEmpty(vv))
            {
                var mfVal = GetValue(propDef.PropDef, vv, log);
                sc.TypedValue.SetValue((MFDataType)propDef.DataType, mfVal);
            }
            else
            {
                sc.TypedValue.SetValueToNULL((MFDataType)propDef.DataType);
            }
            return sc;
        }

        private SearchConditions GetSearchPropValues(SelectedFiles files, SelectedFile file,
            List<MfClassPropDef> selProps, List<string> addedValues, TraceSource log)
        {
            var scs = new SearchConditions();
            var values = files.GetProps(file);
            for (var i = 0; i < values.Count; i++)
            {
                var vv = values[i];
                if (i == 0)
                {
                    var dotIndex = vv.LastIndexOf('.');
                    vv = vv.Substring(0, dotIndex);
                    
                }
                //if (selProps[i].Required)
                {
                    var def = selProps[i].Def;
                    var sc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
                    sc.Expression.DataPropertyValuePropertyDef = def;
                    if (!String.IsNullOrEmpty(vv))
                    {
                        var mfVal = GetValue(selProps[i].PropDef, vv, log);
                        sc.TypedValue.SetValue((MFDataType)selProps[i].DataType, mfVal);
                    }
                    else
                    {
                        sc.TypedValue.SetValueToNULL((MFDataType) selProps[i].DataType);
                    }
                    
                    scs.Add(-1, sc);
                }
            }
            var diff = selProps.Count - values.Count;
            if (diff > 0)
            {
                if (addedValues.Count < diff)
                {
                    addedValues =
                        addedValues.Concat(Enumerable.Range(1, diff - addedValues.Count).Select(c => String.Empty))
                            .ToList();
                }
                for (var i = 0; i < diff; i++)
                {
                    var selProp = selProps[values.Count + i];
                    var def = selProp.Def;
                    var sc = new SearchCondition { ConditionType = MFConditionType.MFConditionTypeEqual };
                    sc.Expression.DataPropertyValuePropertyDef = def;
                    var vv = addedValues[i];
                    if (!String.IsNullOrEmpty(vv))
                    {
                        var mfVal = GetValue(selProp.PropDef, vv, log);
                        sc.TypedValue.SetValue((MFDataType)selProp.DataType, mfVal);
                    }
                    else
                    {
                        sc.TypedValue.SetValueToNULL((MFDataType)selProp.DataType);
                    }
                    scs.Add(-1, sc);
                }
            }
            return scs;
        }

        private static bool GetBoolValue(string value)
        {
            var isTrue = value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("T", StringComparison.OrdinalIgnoreCase) || value.Equals("是");
            return isTrue;
        }

        private static object GetDate(string value, TraceSource log)
        {
            DateTime dt;
            var ok = DateTime.TryParse(value, out dt);
            if (!ok)
            {
                if (log != null)
                {
                    log.TraceEvent(TraceEventType.Warning, 0, "不是日期格式：" + value);
                }
                return null;
            }
            var ts = new Timestamp { Year = (uint)dt.Year, Month = (uint)dt.Month, Day = (uint)dt.Day };

            return ts.GetValue();
        }

        private Dictionary<int, Dictionary<string, int>> _valueListDict = new Dictionary<int, Dictionary<string, int>>();
        /// <summary>
        /// 获取所有对象集合
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="dict"></param>
        private void GetObjects(int objType, Dictionary<string, int> dict)
        {
            var scs = new SearchConditions();

            var typeSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            typeSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
            typeSC.TypedValue.SetValue(MFDataType.MFDatatypeLookup, objType);
            scs.Add(-1, typeSC);

            var delSC = new SearchCondition {ConditionType = MFConditionType.MFConditionTypeEqual};
            delSC.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            delSC.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            scs.Add(-1, delSC);

            var res = _vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            if (res.Count == 0) return;
            foreach (ObjectVersion ov in res)
            {
                var name = ov.Title;
                var id = ov.ObjVer.ID;
                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, id);
                }
                if (dict[name] < id)
                {
                    dict[name] = id;
                }
            }
        }

        private int? GetIdFromValueList(int valueList, string itemName, TraceSource log)
        {
            Dictionary<string, int> dict = null;
            if (!_valueListDict.ContainsKey(valueList))
            {
                dict = new Dictionary<string, int>();
                _valueListDict.Add(valueList, dict);
            }
            dict = _valueListDict[valueList];
            if (dict.Count == 0)
            {
                var objType = _vault.ValueListOperations.GetValueList(valueList);
                if (objType.RealObjectType)
                {
                    GetObjects(valueList, dict);
                }
                else if (dict.Count == 0)
                {
                    var items = _vault.ValueListItemOperations.GetValueListItems(valueList);
                    foreach (ValueListItem i in items)
                    {
                        if (dict.ContainsKey(i.Name) && log != null)
                        {
                            log.TraceEvent(TraceEventType.Warning, 0,
                                String.Format("值列表项({0})有重名：{1}", objType.NameSingular, i.Name));
                        }
                        else
                        {
                            dict.Add(i.Name, i.ID);
                        }
                    }
                }
            }

            if (dict.ContainsKey(itemName))
            {
                return dict[itemName];
            }
            return null;
        }

        public object GetValue(MfPropDef prop, string value)
        {
            return GetValue(prop, value, null);
        }

        private object GetValue(MfPropDef prop, string value, TraceSource log)
        {
            value = value ?? String.Empty;
            switch ((MFDataType)prop.DataType)
            {
                case MFDataType.MFDatatypeText:
                case MFDataType.MFDatatypeMultiLineText:
                    return value;
                case MFDataType.MFDatatypeBoolean:
                    return GetBoolValue(value);
                case MFDataType.MFDatatypeDate:
                    return GetDate(value, log);
                case MFDataType.MFDatatypeLookup:
                case MFDataType.MFDatatypeMultiSelectLookup:
                    var itemId = GetIdFromValueList(prop.ValueList, value, log);
                    if (itemId != null)
                    {
                        return itemId.Value;
                    }
                    break;
                case MFDataType.MFDatatypeInteger:
                case MFDataType.MFDatatypeInteger64:
                    int val;
                    var ok = int.TryParse(value, out val);
                    if (ok)
                    {
                        return val;
                    }
                    if (log != null)
                    {
                        log.TraceEvent(TraceEventType.Warning, 0, "不是数字：" + value);
                    }
                    break;
                case MFDataType.MFDatatypeFloating:
                    double dVal;
                    ok = double.TryParse(value, out dVal);
                    if (ok)
                    {
                        return dVal; 
                    }
                    if (log != null)
                    {
                        log.TraceEvent(TraceEventType.Warning, 0, "不是数字：" + value);
                    }
                    break;
            }
            if (!String.IsNullOrEmpty(value))
            {
                throw new Exception("未能找到对应的属性值：" + value);
            }
            return null;
        }


    }

    
}
