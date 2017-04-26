/************************
*Vault UI的入口
*************************/
"use strict";
function OnNewVaultUI(vaultUI) {

    // Register to listen new vault entry creation event. 
    vaultUI.Events.Register(Event_NewVaultEntry, newVaultEntryHandler);
}

function newVaultEntryHandler(vaultEntry) {

    // Register to listen event.
    //vaultEntry.Events.Register(Event_SetPropertiesOfObjectVersion, SetPropertiesHandler(vaultEntry));
    vaultEntry.Events.Register(Event_SetPropertiesOfObjectVersion, SetUserPropertiesHandler(vaultEntry));
    //新建提资记录
    vaultEntry.Events.Register(Event_CreateObject, newSharingLogHandler(vaultEntry));
}
function newSharingLogHandler(vaultEntry) {
    return function(objectType, propertyValues) {
        var vault = vaultEntry.Vault;
        var typeId = MF.alias.objectType(vault, md.sharingLog.typeAlias);
        if (objectType !== typeId) return;

        var propIdSharedBy = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharedBy);
        var propIdSharedDocs = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharedDocs);
        var propIdSharingTo = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharingTo);

        var stPv = propertyValues.SearchForProperty(propIdSharingTo);
        if (stPv.Value.IsNULL()) return;
        var sdValue = propertyValues.SearchForProperty(propIdSharedDocs).Value;
        if (sdValue.IsNULL()) return;

        //var sharedById = propertyValues.SearchForProperty(propIdSharedBy).Value.GetLookupID();
        var sharedById = vault.SessionInfo.UserID;
        var sharedDocs = sdValue.GetValueAsLookups();

        var shareDocObjVns = getShareDocObjVns(vault, sharedDocs);
        if (hasPermission2Share(vault, shareDocObjVns, sharedById) == false) {
            //return vaultEntry.VaultUI.ShowMessage("无权提资其中文档");
            MFiles.ThrowError("无权提资其中文档");
        }  
        return {
            OnSuccess: function(objectVersion) {
                updateSharingDocs(vault, shareDocObjVns, stPv);
            }
        }
    }
}
//修改模型策划的"模型"属性时，修改其文件
function SetPropertiesHandler(vaultEntry) {
    return function (setPropertiesParams) {
        var oObjVer = setPropertiesParams.ObjVer;
        if (oObjVer.Type !== 0) return;

        var vault = vaultEntry.Vault;
        var classIdBimModel = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var settingProps = setPropertiesParams.PropertyValuesToSet;
        if (settingProps.IndexOf(parseInt(MFBuiltInPropertyDefClass)) === -1) return;
        var classId = settingProps.SearchForProperty(parseInt(MFBuiltInPropertyDefClass)).Value.GetLookupID();

        if (classIdBimModel != classId) return;
        var temChanged = true;

        var currentProps = vault.ObjectPropertyOperations.GetProperties(oObjVer, false);
        var propIdTemplate = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.TemplateAt);
        var temTvalue = settingProps.SearchForProperty(propIdTemplate).Value;
        var temTitle = temTvalue.DisplayValue;
        var temTitleOld = currentProps.SearchForProperty(propIdTemplate).Value.DisplayValue;
        //vaultEntry.VaultUI.ShowMessage(temTitleOld+"-"+temTitle);
        if (temTitle == temTitleOld || temTitle === "") {
            //return;
            temChanged = false;
        } else {
            var temId = temTvalue.GetLookupID();
            var temObjver = MFiles.CreateInstance("ObjVer");
            temObjver.SetIDs(0, temId, -1);
            var temVn = vault.ObjectOperations.GetObjectInfo(temObjver, true, false);

            if (temVn.FilesCount < 1) {
                //return;
                temChanged = false;
            }
        }
        return {
            OnSuccess: function (objectVersion) {
                if (temChanged) {
                    var temFile = temVn.Files.Item(1);
                    var ext = temFile.Extension;
                    var filePath = vault.ObjectFileOperations.GetPathInDefaultView(temVn.ObjVer.ObjID, temVn.ObjVer.Version,
                        temFile.ID, temFile.Version, MFLatestSpecificBehaviorAutomatic, false);
                    //vaultEntry.VaultUI.ShowMessage(filePath);
                    var objVn = objectVersion;
                    var checkedOutVn;
                    if (objVn.ObjectCheckedOut) checkedOutVn = objVn;
                    else checkedOutVn = vault.ObjectOperations.CheckOut(oObjVer.ObjID);
                    try {
                        if (checkedOutVn.SingleFile) {
                            vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, false);
                        }
                        for (var i = 1; i <= checkedOutVn.FilesCount; i++) {
                            var fileItem = checkedOutVn.Files.Item(i);
                            //vaultEntry.VaultUI.ShowMessage(fileItem.Title);
                            if (fileItem.Title === objectVersion.Title && fileItem.Extension === ext) continue;
                            vault.ObjectFileOperations.RemoveFile(checkedOutVn.ObjVer, fileItem.FileVer);
                        }
                        try {
                            vault.ObjectFileOperations.AddFile(checkedOutVn.ObjVer, objectVersion.Title, ext, filePath);
                        } catch (e) {
                        }
                        vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, true);
                    } catch (e) {
                        //vaultEntry.VaultUI.ShowMessage(e.message);
                    }
                    vault.ObjectOperations.CheckIn(checkedOutVn.ObjVer);
                }
                //vaultEntry.VaultUI.ShowMessage(temChanged);
                //新建任务
                try {
                    //newTaskByModelPlan(vaultEntry,vault, currentProps, settingProps, objectVersion.ObjVer.ID, objectVersion.Title);
                } catch (e) {
                    //vaultEntry.VaultUI.ShowMessage(e.message);
                } 
            }
        }
    }
}
//修改"建模人","专业负责人","预计完成工时"属性时，添加新任务
function SetUserPropertiesHandler(vaultEntry) {
    return function (setPropertiesParams) {
        var oObjVer = setPropertiesParams.ObjVer;
        if (oObjVer.Type !== 0) return;
        var vault = vaultEntry.Vault;
        var classIdBimModel = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var settingProps = setPropertiesParams.PropertyValuesToSet;
        if (settingProps.IndexOf(MFBuiltInPropertyDefClass) === -1) return;
        var classId = settingProps.SearchForProperty(MFBuiltInPropertyDefClass).Value.GetLookupID();
        if (classIdBimModel != classId) return;

        var currentProps = vault.ObjectPropertyOperations.GetProperties(oObjVer, false);
        return {
            OnSuccess: function (objectVersion) {
                //新建或更新任务
                try {
                    //vaultEntry.VaultUI.ShowMessage("新建或更新任务");
                    newOrUpdateTaskByModelPlan(vaultEntry, vault, currentProps, settingProps, objectVersion.ObjVer.ID, objectVersion.Title);
                } catch (e) {
                }
            }
        }
    }
}
//添加建模人、专业负责人时，指派任务
function newOrUpdateTaskByModelPlan(vaultEntry, vault, modelProps, modelPropsSet, modelId, modelTile) {
    var propIdDesigner = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ModelCreator);
    var propIdDisciLead = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciLeader);
    var propIdExpectedDate = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.Deadline);

    var deadline = "";
    var dlTvalue = modelPropsSet.SearchForProperty(propIdExpectedDate).Value;
    if (dlTvalue.IsNULL() == false) {
        deadline = dlTvalue.Value;
    }

    var designers = getUsers(vault, modelProps, propIdDesigner);
    var leaders = getUsers(vault, modelProps, propIdDisciLead);
    var designersSet = getUsers(vault, modelPropsSet, propIdDesigner);
    var leadersSet = getUsers(vault, modelPropsSet, propIdDisciLead);
    var iCount = designers.Count + leaders.Count;
    var iCountSet = designersSet.Count + leadersSet.Count;
    var users;
    var desc;
    if (iCount === 0 && iCountSet > 0) {
        //新建任务
        users = mergeLookups(designersSet, leadersSet);
        desc = getDescription(leadersSet, "专业负责人") + "\r\n"
            + getDescription(designersSet, "建模人") + "\r\n具体可参考附带的模型策划文档<" + modelTile + ">。";
        //vaultEntry.VaultUI.ShowMessage("新建任务2:\r\n" + desc);
        newTask(vault, users, deadline, desc, modelId, modelTile);
    }
    if (iCount > 0 && iCountSet > 0) {
        //更新任务
        if (isChanged(modelProps, modelPropsSet, propIdDesigner) ||
            isChanged(modelProps, modelPropsSet, propIdDisciLead) ||
            isChanged(modelProps, modelPropsSet, propIdExpectedDate)) {
            users = mergeLookups(designersSet, leadersSet);
            desc = getDescription(leadersSet, "专业负责人") + "\r\n"
                + getDescription(designersSet, "建模人") + "\r\n具体可参考附带的模型策划文档<" + modelTile + ">。";

            var taskVns = searchTask(vault, modelId);
            if (taskVns.Count > 0) {
                //vaultEntry.VaultUI.ShowMessage("更新任务:\r\n" + desc);
                updateTask(vault, taskVns.Item(1), users, deadline, desc, modelTile);
            } else {
                //vaultEntry.VaultUI.ShowMessage("新建任务2:\r\n" + desc);
                newTask(vault, users, deadline, desc, modelId, modelTile);
            }
        }
    }
}
function newTask(vault, users, deadline, desc, modelId, modelTile) {

    var propIdTitle = MF.alias.propertyDef(vault, md.genericTask.propDefs.TaskTitle);
    //var propIdStartDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.StartDate); 
    var propIdDoc = vault.ObjectTypeOperations.GetBuiltInObjectType(MFBuiltInObjectTypeDocument).DefaultPropertyDef;//1011

    var pvs = MFiles.CreateInstance('PropertyValues');
    //名称
    var propTitle = MFiles.CreateInstance('PropertyValue');
    propTitle.PropertyDef = propIdTitle;
    propTitle.TypedValue.SetValue(MFDatatypeText, "模型策划任务：" + modelTile);
    pvs.Add(-1, propTitle);
    //说明
    var propDesc = MFiles.CreateInstance('PropertyValue');
    propDesc.PropertyDef = MFBuiltInPropertyDefAssignmentDescription;
    propDesc.TypedValue.SetValue(MFDatatypeMultiLineText, desc);
    pvs.Add(-1, propDesc);
    //指派给
    var toValue = MFiles.CreateInstance("PropertyValue");
    toValue.PropertyDef = MFBuiltInPropertyDefAssignedTo;
    toValue.TypedValue.SetValue(MFDatatypeMultiSelectLookup, users);
    pvs.Add(-1, toValue);

    //文档-->模型策划文档
    var docValue = MFiles.CreateInstance("PropertyValue");
    docValue.PropertyDef = propIdDoc;
    docValue.TypedValue.SetValue(MFDatatypeMultiSelectLookup, modelId);
    pvs.Add(-1, docValue);

    ////开始日期
    //var now = new Date();
    //var startDateValue = MFiles.CreateInstance("PropertyValue");
    //startDateValue.PropertyDef = propIdStartDate;
    //startDateValue.TypedValue.SetValue(MFDatatypeDate, now);
    //pvs.Add(-1, startDateValue);

    //截止日期
    if (deadline != "") {
        var deadlineValue = MFiles.CreateInstance("PropertyValue");
        deadlineValue.PropertyDef = MFBuiltInPropertyDefDeadline;
        deadlineValue.TypedValue.SetValue(MFDatatypeDate, deadline);
        pvs.Add(-1, deadlineValue);
    }
    MF.ObjectOps.createObject(vault, 10, -100, pvs, undefined);
}

function isChanged(props1, props2, propDef) {
    var value1 = props1.SearchForProperty(propDef).Value.DisplayValue;
    var value2 = props2.SearchForProperty(propDef).Value.DisplayValue;
    return value1 != value2;
}
function getDescription(src, role) {
    var desc = role + "：";
    for (var i = 1; i <= src.Count; i++) {
        desc += src.Item(i).DisplayValue;
        if (i != src.Count) {
            desc += ", ";
        }
    }
    return desc;
}
function mergeLookups(src1, src2) {
    var res = src1.Clone();
    for (var i = 1; i <= src2.Count; i++) {
        if (res.GetLookupIndexByItem(src2.Item(i).Item) === -1) {
            res.Add(-1, src2.Item(i));
        }
    }
    return res;
}
function getUsers(vault,props,propDef) {
    var tvalue = props.SearchForProperty(propDef).Value;
    var ids = tvalue.IsNULL() == false ? tvalue.GetValueAsLookups() : MFiles.CreateInstance('Lookups');
    return ids;
}
function getAddUsers(vault, modelProps, modelPropsSet, propDef) {
    var tvalue = modelProps.SearchForProperty(propDef).Value;
    var tvalueSet = modelPropsSet.SearchForProperty(propDef).Value;
    //if (tvalue.IsNULL() && tvalueSet.IsNULL()) return [];
    var ids = tvalue.IsNULL() == false ? tvalue.GetValueAsLookups() : MFiles.CreateInstance('Lookups');
    var idsSet = tvalueSet.IsNULL() == false ? tvalueSet.GetValueAsLookups() : MFiles.CreateInstance('Lookups');
    var res = [];
    for (var i = 1; i <= idsSet.Count; i++) {
        var item = idsSet.Item(i);
        var flag = false;
        for (var j = 1; j <= ids.Count; j++) {
            if (item.Item == ids.Item(j).Item) {
                flag = true;
                break;
            }
        }
        if (!flag) {
            res.push(item.Item);
        }
    }
    return res;
}
//搜索
function searchTask(vault, modelId) {
    var propIdTitle = MF.alias.propertyDef(vault, md.genericTask.propDefs.TaskTitle);
    var propIdDoc = vault.ObjectTypeOperations.GetBuiltInObjectType(MFBuiltInObjectTypeDocument).DefaultPropertyDef;//1011
    var sConditions = MFiles.CreateInstance("SearchConditions");
    //名称
    var conditionTitle = MFiles.CreateInstance("SearchCondition");
    conditionTitle.ConditionType = MFConditionTypeStartsWith;
    conditionTitle.Expression.DataPropertyValuePropertyDef = propIdTitle;
    conditionTitle.TypedValue.SetValue(MFDatatypeText, '模型策划任务');
    sConditions.Add(-1, conditionTitle);
    //类别
    var conditionClass = MFiles.CreateInstance("SearchCondition");
    conditionClass.ConditionType = MFConditionTypeEqual;
    conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
    conditionClass.TypedValue.SetValue(MFDatatypeLookup, -100);
    sConditions.Add(-1, conditionClass);
    //文档
    var conditionDoc = MFiles.CreateInstance("SearchCondition");
    conditionDoc.ConditionType = MFConditionTypeEqual;
    conditionDoc.Expression.DataPropertyValuePropertyDef = propIdDoc;
    conditionDoc.TypedValue.SetValue(MFDatatypeMultiSelectLookup, modelId);
    sConditions.Add(-1, conditionDoc);

    var res = MF.ObjectOps.SearchObjects(vault, 10, sConditions);
    return res;
}
//更新操作
function updateTask(vault, taskObjVersion, users, deadline, desc, modelTile) {
    //
    var propIdTitle = MF.alias.propertyDef(vault, md.genericTask.propDefs.TaskTitle);

    var pvs = MFiles.CreateInstance('PropertyValues');
    //名称
    if (modelTile) {
        var propTitle = MFiles.CreateInstance('PropertyValue');
        propTitle.PropertyDef = propIdTitle;
        propTitle.TypedValue.SetValue(MFDatatypeText, "模型策划任务：" + modelTile);
        pvs.Add(-1, propTitle);
    }
    //说明
    if (desc) {
        var propDesc = MFiles.CreateInstance('PropertyValue');
        propDesc.PropertyDef = MFBuiltInPropertyDefAssignmentDescription;
        propDesc.TypedValue.SetValue(MFDatatypeMultiLineText, desc);
        pvs.Add(-1, propDesc);
    }
    //指派给
    if (users && users.Count > 0) {
        var toValue = MFiles.CreateInstance("PropertyValue");
        toValue.PropertyDef = MFBuiltInPropertyDefAssignedTo;
        toValue.TypedValue.SetValue(MFDatatypeMultiSelectLookup, users);
        pvs.Add(-1, toValue);
    }
    //截止日期
    if (deadline != "") {
        var deadlineValue = MFiles.CreateInstance("PropertyValue");
        deadlineValue.PropertyDef = MFBuiltInPropertyDefDeadline;
        deadlineValue.TypedValue.SetValue(MFDatatypeDate, deadline);
        pvs.Add(-1, deadlineValue);
    }

    updateObject(vault, taskObjVersion, pvs);
}
function updateObject(vault, objectVersion, propValues) {
    var oObjVn = objectVersion;
    var checkOutVn;
    if (oObjVn.ObjectCheckedOut) {
        if (oObjVn.CheckedOutTo === vault.SessionInfo.UserID || oObjVn.CheckedOutTo === -103) {
            checkOutVn = oObjVn;
        } else { //
            var oldObjVer;
            try {
                oldObjVer = vault.ObjectOperations.ForceUndoCheckout(oObjVn.ObjVer).ObjVer;
            } catch (e) {
                //throw new Error("当前账户无权限撤销对象:" + oObjVn.Title + " 的签出\r\n" + e.Message);
                return;
            }
            try {
                checkOutVn = vault.ObjectOperations.CheckOut(oldObjVer.ObjID);
            } catch (e) {
                //throw new Error("当前账户无权限签出对象:" + oObjVn.Title + "\r\n" + e.Message);
                return;
            }
        }
    } else {
        checkOutVn = vault.ObjectOperations.CheckOut(oObjVn.ObjVer.ObjID);
    }
    vault.ObjectPropertyOperations.SetProperties(checkOutVn.ObjVer, propValues);
    try {
        vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
    } catch (e) {
        //throw new Error("签入修改对象:" + oObjVn.Title + "失败！\r\n" + e.Message);
        try {
            vault.ObjectOperations.ForceUndoCheckout(checkOutVn.ObjVer);
        } catch (e) {
            try {
                vault.ObjectOperations.UndoCheckout(checkOutVn.ObjVer);
            } catch (e) {
            }
        }
    }
}

////提资记录、提资文档相关操作
//更新提资文档
function updateSharingDocs(vault, shareDocObjVns, pvShareTos) {
    var pvs = MFiles.CreateInstance('PropertyValues');
    pvs.Add(-1, pvShareTos);

    for (var i = 0; i < shareDocObjVns.length; i++) {
        var objVn = shareDocObjVns[i];
        try {
            updateObject(vault, objVn, pvs);
        } catch (e) {
        }   
    }
}

function getShareDocObjVns(vault, sharedDocs) {
    var objvns = [];
    for (var i = 1; i <= sharedDocs.Count; i++) {
        var item = sharedDocs.Item(i);
        var oObVer = new MFiles.ObjVer();
        oObVer.SetIDs(0, item.Item, -1);
        var objVn = vault.ObjectOperations.GetObjectInfo(oObVer, true, false);
        objvns.push(objVn);
    }
    return objvns;
}

function hasPermission2Share(vault,shareDocObjVns, sharedById) {
    var flag = true;
    for (var i = 0; i < shareDocObjVns.length; i++) {
        var item = shareDocObjVns[i];
        var creator = vault.ObjectPropertyOperations.GetProperty(item.ObjVer, 25).Value.GetLookupID();
        if (creator != sharedById) {
            flag = false;
            break;
        }
    }
    return flag;
}