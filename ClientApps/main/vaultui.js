"use strict";

/*******************
 * VaultUI module entry
 **************************/	
 
function OnNewVaultUI(vaultUI) {
    vaultUI.Events.Register(Event_NewVaultEntry, newVaultEntry);
}

function newVaultEntry(vaultEntry) {
    vaultEntry.Events.Register(Event_SetPropertiesOfObjectVersion, setProps(vaultEntry));
}

function setProps(vaultEntry) {
    return function (setPropertiesParams, singlePropertyUpdate, singlePropertyRemove) {

        var vault = vaultEntry.Vault;
        var objVer = setPropertiesParams.ObjVer;
        if (objVer.Type !== MFBuiltInObjectTypeAssignment) { // 只针对任务
            return;
        }
        
        var vaultName = vault.Name;
        var today = new Date();
        var dateStr = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + today.getDate();
        var logFile = vaultName+'_log_'+dateStr+".txt";
        
        
        //writeTextFile(logFile, ["任务"]);
        var userId = vault.SessionInfo.UserID;
        var settingProps = setPropertiesParams.PropertyValuesToSet;
        
        var isMarkedCompleted = isTaskCompleted(settingProps, userId);
        var isRejected = false;
        if (!isMarkedCompleted) { // 标记拒绝                    
            isRejected = isTaskRejected(settingProps, userId);
        }
        
        if (!isMarkedCompleted && !isRejected) return;
       // writeTextFile(logFile, ["标记"]);
       var stateStr = '通过\t'; //任务状态
       if (isRejected) {
           stateStr = '拒绝\t';
       }
        
        var rObjVer = GetWorkflowObject(vault, objVer);

        if (!rObjVer) {
            //writeTextFile(logFile, ["未找到流程文档！ "+stateStr]);
            return;
        }
        //writeTextFile(logFile, ["rObjVer"]);

        var wfd = vault.ObjectPropertyOperations.GetWorkflowState(rObjVer, false);
        var wState = wfd.State.Value;
        if (wState.IsNULL() || wState.IsUninitialized()) {
            return;
        }
        var stateId = wState.GetLookupID();


        //writeTextFile(logFile, ["Before Task SetComments: "+stateStr]);
        var commentContent = stateStr; //备注内容
        
        var commentIndex = settingProps.IndexOf(MFBuiltInPropertyDefComment); //
        if (commentIndex === -1) {
            //writeTextFile(logFile, ["SetProps: "+commentContent]);
            var pv = MFiles.CreateInstance('PropertyValue');
            pv.PropertyDef = MFBuiltInPropertyDefComment;
            pv.Value.SetValue(MFDatatypeMultiLineText, commentContent);
            setPropertiesParams.PropertyValuesToSet.Add(-1,pv);
        } else {
            var pv = settingProps.SearchForProperty(MFBuiltInPropertyDefComment);
            commentContent = stateStr + pv.GetValueAsLocalizedText();
        }
        
        var fullName = GetUserFullname(vault, userId);

        var commentStr = fullName + ": " + commentContent;

        setTaskDocCommentValues(vault, objVer, commentStr, rObjVer, stateId);

        /*
        return {
            OnSuccess: function(objectVersion) {
                //writeTextFile(logFile, ["开始"]);
                if (isMarkedCompleted) {
                    AddVersionComment(vault, objectVersion.ObjVer, rObjVer, fullName+'：', logFile);
                } else if (isRejected) {
                    AddVersionComment(vault, objectVersion.ObjVer, rObjVer, fullName+'：', logFile);
                }
            },
            OnError: function (errorCode, errorMessage, errorStack) {
                //vaultEntry.VaultUI.ShowMessage('出错：'+errorMessage);
            },
            Finally: function() {
                
            }
        }; */       
    }
}

function getNamespaceForTaskDoc() {
    return "FlowTaskComment";
}

function setTaskDocCommentValues(vault, taskObjVer, commentContent, flowObjVer, stateId) {
    var values;
    var docType = flowObjVer.Type;
    var docId = flowObjVer.ID;
    var flowKey = docType + '#' + docId;
    var ns = getNamespaceForTaskDoc();
    try {
        values = vault.NamedValueStorageOperations.GetNamedValues(MFUserDefinedValue, ns);
    } catch(e) {

    }
    if (!values) {
        values = MFiles.CreateInstance('NamedValues');
    }
    var taskId = taskObjVer.ID;
    var taskVersion = taskObjVer.Version;
    var taskKey = taskId +'#'+taskVersion;
    values.Value(flowKey + ";" + taskKey) = stateId + ' ' + commentContent;

    vault.NamedValueStorageOperations.SetNamedValues(MFUserDefinedValue, ns, values);
}

function GetUserFullname(vault, userId) {
    var userList = vault.UserOperations.GetUserList();
    for(var i = 1;i<=userList.Count;i++) {
        if (userList.Item(i).Key === userId) {
            return userList.Item(i).Name;
        }
    }
    return vault.SessionInfo.AccountName;
}

function isTaskCompleted(settingProps, userId) {
    var completeIndex = settingProps.IndexOf(MFBuiltInPropertyDefCompletedBy); //45
    if (completeIndex !== -1) { // 标记完成
        var tv = settingProps.Item(completeIndex).Value;
        if (!tv.IsNULL()) {
            var lkps = tv.GetValueAsLookups();
            for(var i = 1;i<=lkps.Count;i++) {
                if (lkps.Item(i).Item === userId) {
                    return true;
                }
            }
        }
        
    }
    return false;
}

function isTaskRejected(settingProps, userId) {
    var rejectPropDef = 97;
    var rejectIndex = settingProps.IndexOf(rejectPropDef);
    if (rejectIndex !== -1) {
        var tv = settingProps.Item(rejectIndex).Value;
        if (!tv.IsNULL()) {
            var lkps = tv.GetValueAsLookups();
            for(var i = 1;i<=lkps.Count;i++) {
                if (lkps.Item(i).Item === userId) {
                    return true;
                }
            }
        }
    }
    return false;
}

function GetWorkflowObject(vault, taskObjVer) {
    var relations = vault.ObjectOperations.GetRelationships(taskObjVer, MFRelationshipsModeAll); //3
    if (relations.Count === 0) return;
    var flowTaskPropDef = MFBuiltInPropertyDefWorkflowAssignment; // 只针对工作流任务79
    for(var i = 1;i <= relations.Count; i++) {
        var rObjVer = relations.Item(i).ObjVer;
        var pvs = vault.ObjectPropertyOperations.GetProperties(rObjVer, false);
        var pIndex = pvs.IndexOf(flowTaskPropDef);
        if (pIndex !== -1) { // 只针对带有工作流任务的文档
            var pTV = pvs.Item(pIndex).Value;
            if (!pTV.IsNULL()) {
                var pLkps = pTV.GetValueAsLookups();
                var isThisDoc = false;
                for(var j = 1; j<=pLkps.Count;j++) {
                    if (pLkps.Item(j).Item === taskObjVer.ID) { // 文档必须与当前的工作流任务相关
                        isThisDoc = true;
                        break;
                    }
                }
                if (isThisDoc) {
                    return rObjVer;
                }
            }
        }      
    }
}


function AddVersionComment(vault, taskObjVer, rObjVer, descPrefix, logFile) {
    
    //writeTextFile(logFile, ["start version comment..."]);
    
    var content;
    try {
        var comment = vault.ObjectPropertyOperations.GetVersionComment(taskObjVer).VersionComment;
        content = comment.GetValueAsLocalizedText();
    } catch(e) {
        
    }
    writeTextFile(logFile, ["获取了备注"]);
    
    try {
        rObjVer = vault.ObjectOperations.GetLatestObjVer(rObjVer.ObjID, true, true);
        var str = rObjVer.Type + '-' + rObjVer.ID+'-' + (rObjVer.Version) + '-' + descPrefix+content;    
        writeTextFile(logFile, [str]);
        vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod('setFlowTaskComment', str);
    } catch(e) {
        var err = e.message || e.stack || e.toString();
        writeTextFile(logFile, ["version Comment Exception: "+ err]);
    }
    
    
    
    /*
    //writeTextFile("temptemp.txt", ["comment: "+content]); 
    var docComment = new MFiles.PropertyValue();
    docComment.PropertyDef = MFBuiltInPropertyDefVersionComment;
    docComment.Value.SetValue(MFDatatypeMultiLineText, descPrefix+content);
    try {
        rObjVer = vault.ObjectOperations.GetLatestObjVer(rObjVer.ObjID, true, true);
        vault.ObjectPropertyOperations.SetVersionComment(rObjVer, docComment);
    } catch(e) {
        var err = e.message || e.stack || e.toString();
        writeTextFile("temptemp.txt", ["version Comment Exception: "+ err]);
    }    
    
    //writeTextFile("temptemp.txt", ["end version comment..."]);
    */
}

function writeTextFile(fileName, contents) {
        //var tFile = tempPath + '\\' + k + currentUserId + ".txt";
        var fso = new ActiveXObject('Scripting.FileSystemObject');
        var tempFolder = fso.GetSpecialFolder(2);
        var filePath = tempFolder +"\\"+fileName;
        var fh;
        if (!fso.FileExists(filePath)) {
            fh = fso.CreateTextFile(filePath, 2, false);
        } else {
            var atts = fso.GetFile(filePath).Attributes;
            if (atts & 1) {
                fso.GetFile(filePath).Attributes = atts - 1;
            }
            fh = fso.OpenTextFile(filePath, 8, false); //ForReading = 1, ForWriting = 2, ForAppending = 3
        }
        for (var i = 0; i < contents.length; i++) {
            fh.WriteLine(contents[i]);
        }
        fh.Close();
        fso = null;
    }