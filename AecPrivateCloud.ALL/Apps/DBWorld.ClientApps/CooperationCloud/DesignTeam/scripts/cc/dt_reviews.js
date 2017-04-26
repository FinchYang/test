/***********************
* 校审相关代码
************************/
"use strict";

var CC = CC || {};
(function (u, undefined) {
    var reviews = {
        sheetPass: function (sf) {
            ///<summary>通过/提交</summary>
            var selection = sf.ActiveListing.CurrentSelection;
            if (this.setFlowMenuVisible(selection, sf)) {
                this.passAction(sf, selection.ObjectVersionsAndProperties);
                sf.ShellUI.ShowMessage('校审完成！');
                this._refreshCurrentListing(sf);
            } else {
                sf.ShellUI.ShowMessage('请选择待校审的成果图纸！');
            }
        },
        sheetReject: function (sf) {
            ///<summary>退回</summary>
            var selection = sf.ActiveListing.CurrentSelection;
            if (this.setFlowMenuVisible(selection, sf)) {
                var errs = this.rejectAction(sf, selection.ObjectVersionsAndProperties);
                var tip = "";
                for (var e in errs) {
                    if (errs[e]) {
                         tip += errs[e];
                    }
                }
                if (tip) {
                    sf.ShellUI.ShowMessage(tip);
                } else {
                    sf.ShellUI.ShowMessage('校审完成！');
                }
                this._refreshCurrentListing(sf);
            } else {
                sf.ShellUI.ShowMessage('请选择待校审的成果图纸！');
            }
        },
        setFlowMenuVisible: function (selItems, shellFrame) {
            /*
            * @summary 控制自定义命令菜单以及‘标记完成’等菜单的可见性
            * @return 是否需要设置可见性
            */
            var count = selItems.ObjectVersions.Count;
            var visible = false;
            var i;
            if (count > 0) {
                var vault = shellFrame.ShellUI.Vault;
                var docClassId = MF.alias.classType(vault, md.drawingSheet.classAlias);

                for (i = 1; i <= count; i++) {
                    var objVersion = selItems.ObjectVersions.Item(i);
                    if (objVersion.ObjVer.Type === 0 && objVersion.Class === docClassId) {
                        var tasks = this._getFlowTasks(vault, objVersion);
                        if (tasks.length) {
                            visible = true;
                            break;
                        }
                    }
                }
            }
            return visible;
        },
        getFlowTaskState: function (selItems, shellFrame) {
            /*
            * @summary: 判断选中任务的状态（单选时）
            */
            var state = -1;
            var count = selItems.ObjectVersionsAndProperties.Count;
            var vault = shellFrame.ShellUI.Vault;
            var docClassId = MF.alias.classType(vault, md.drawingSheet.classAlias);
            if (count > 0) {
                var objVnProps = selItems.ObjectVersionsAndProperties.Item(1);
                var objVersion = objVnProps.VersionData;
                var props = objVnProps.Properties;
                if (objVersion.ObjVer.Type === 0 && objVersion.Class === docClassId) {
                    var wTask = props.SearchForProperty(MFBuiltInPropertyDefWorkflowAssignment).Value;
                    if (wTask.IsNULL()) {
                        //无工作流任务
                    }
                }
            }
            return state;
        },
        passAction: function (sf, drawObjs) {
            ///<summary>校审-通过的入口</summary>
            ///<param name="drawObjs" type="ObjectVersionAndPropertiesOfMultipleObjects">图纸对象的集合</param>

            var vault = sf.ShellUI.Vault;
            var signSheetList = [];

            var docClassId = MF.alias.classType(vault, md.drawingSheet.classAlias);
            var currentUserId = vault.SessionInfo.UserID;

            var errList = {};
            var completeTasks = [];
            for (var i = 1; i <= drawObjs.Count; i++) {
                var sheetObjProps = drawObjs.Item(i);

                var sheetKey = sheetObjProps.VersionData.Title + ' # ' + sheetObjProps.ObjVer.ID;
                var tasks = this._getFlowTasks(vault, sheetObjProps.VersionData);

                if (tasks.length === 0) {
                    errList[sheetKey] = '未找到相关的工作流任务！';
                    continue;
                }
                var taskObjVer = tasks[0].ObjVer;

                //工作流检查
                var workflowState = vault.ObjectPropertyOperations.GetWorkflowState(sheetObjProps.ObjVer, false);
                var wWorkflow = workflowState.Workflow.Value;
                if (wWorkflow.IsNULL() || wWorkflow.IsUninitialized()) continue;
                var workFlowId = wWorkflow.GetValueAsLookup().Item;
                if (workFlowId !== MF.alias.workflow(vault, md.reviewFlow.alias)) {
                    errList[sheetKey] = '必须是出图流程！';
                    continue;
                }
                //流程状态检查
                var wState = workflowState.State.Value;
                if (wState.IsNULL() || wState.IsUninitialized()) {
                    errList[sheetKey] = '流程状态未赋值！';
                    continue;
                }
                var stateId = wState.GetValueAsLookup().Item;
                this._markTaskCompleted(vault, taskObjVer.ID);
            }
        },
        rejectAction: function (sf, drawObjs) {
            ///<summary>校审-退回的入口</summary>
            ///<param name="drawObjs" type="ObjectVersionAndPropertiesOfMultipleObjects">图纸对象的集合</param>

            var vault = sf.ShellUI.Vault;
            var docClassId = MF.alias.classType(vault, md.drawingSheet.classAlias);
            var errList = {};
            var completeTasks = [];
            for (var i = 1; i <= drawObjs.Count; i++) {
                var sheetObjProps = drawObjs.Item(i);
                var sheetObjVn = sheetObjProps.VersionData;
                var sheetProps = sheetObjProps.Properties;

                if (docClassId !== sheetObjVn.Class) continue;
                var sheetKey = sheetObjVn.Title + ' # ' + sheetObjVn.ObjVer.ID;
                var tasks = this._getFlowTasks(vault, sheetObjVn);

                if (tasks.length === 0) {
                    errList[sheetKey] = "\r\n<" + sheetObjVn.Title + '>: 未找到相关的工作流任务！';
                    continue;
                }
                var taskObjVer = tasks[0].ObjVer;

                //工作流检查
                var workflowState = vault.ObjectPropertyOperations.GetWorkflowState(sheetObjVn.ObjVer, false);
                var wWorkflow = workflowState.Workflow.Value;
                if (wWorkflow.IsNULL() || wWorkflow.IsUninitialized()) continue;
                var workFlowId = wWorkflow.GetValueAsLookup().Item;
                if (workFlowId !== MF.alias.workflow(vault, md.reviewFlow.alias)) {
                    errList[sheetKey] = "\r\n<" + sheetObjVn.Title + ">: 必须是校审流程！";
                    continue;
                }
                //流程状态检查
                var wState = workflowState.State.Value;
                if (wState.IsNULL() || wState.IsUninitialized()) {
                    errList[sheetKey] = "\r\n<" + sheetObjVn.Title + '>: 流程状态未赋值！';
                    continue;
                }
                var stateId = wState.GetValueAsLookup().Item;
                if (stateId === MF.alias.workflowState(vault, md.reviewFlow.stateAlias.Designer)) {
                    if (errList.designState) {
                        errList.designState += "<" + sheetObjVn.Title + ">: 设计流程状态，无法退回！\r\n";
                    } else {
                        errList.designState = "\r\n<" + sheetObjVn.Title + ">: 设计流程状态，无法退回！\r\n";
                    }
                    continue;
                }
                var objInfo = {
                    fruitDoc: {
                        ID: sheetObjVn.ObjVer.ID,
                        Name: sheetObjVn.Title
                    },
                    docVersionId: sheetObjVn.ObjVer.Version,
                    notPass: true,
                    reviewState: stateId
                }
                try {
                    this._newReviewComment(vault, objInfo);
                    this._markTaskCompleted(vault, taskObjVer.ID);
                } catch (e) {

                }
            }
            return errList;
        },
        newReviewComment: function (sf) {
            var selection = sf.ActiveListing.CurrentSelection;
            if (selection.Count == 0 || selection.ObjectVersionsAndProperties.Count == 0) return;
            var drawObjs = selection.ObjectVersionsAndProperties;
            var vault = sf.ShellUI.Vault;
            var docClassId = MF.alias.classType(vault, md.drawingSheet.classAlias);
            var errList = {};
            for (var i = 1; i <= drawObjs.Count; i++) {
                var sheetObjProps = drawObjs.Item(i);
                var sheetObjVn = sheetObjProps.VersionData;
                var sheetProps = sheetObjProps.Properties;

                if (docClassId !== sheetObjVn.Class) continue;
                var sheetKey = sheetObjVn.Title + ' # ' + sheetObjVn.ObjVer.ID;
                var tasks = this._getFlowTasks(vault, sheetObjVn);

                if (tasks.length === 0) {
                    errList[sheetKey] = '未找到相关的工作流任务！';
                    continue;
                }
                //工作流检查
                var workflowState = vault.ObjectPropertyOperations.GetWorkflowState(sheetObjVn.ObjVer, false);
                var wWorkflow = workflowState.Workflow.Value;
                if (wWorkflow.IsNULL() || wWorkflow.IsUninitialized()) continue;
                var workFlowId = wWorkflow.GetValueAsLookup().Item;
                if (workFlowId !== MF.alias.workflow(vault, md.reviewFlow.alias)) {
                    continue;
                }
                //流程状态检查
                var wState = workflowState.State.Value;
                if (wState.IsNULL() || wState.IsUninitialized()) {
                    continue;
                }
                var stateId = wState.GetValueAsLookup().Item;
                var objInfo = {
                    fruitDoc: {
                        ID: sheetObjVn.ObjVer.ID,
                        Name: sheetObjVn.Title
                    },
                    docVersionId: sheetObjVn.ObjVer.Version,
                    notPass: true,
                    reviewState: stateId
                }
                //sf.ShowMessage(stateId);
                this._newReviewComment(vault, objInfo, sf);
            }
        },
        _refreshCurrentListing: function (shellFrame, placeHolder) {//刷新
            var cPath = shellFrame.CurrentPath;
            try {
                if (shellFrame.ActiveListing.CurrentSelection.Count > 0) {
                    shellFrame.ActiveListing.RefreshSelectedObjects(true, false); // RefreshListingAsync(); // RefreshListing(true, true, false);
                } else {
                    shellFrame.ActiveListing.RefreshListing(true, true, false); //RefreshListingAsync(); // 
                }
            } catch (e) {
                try {
                    if (placeHolder || placeHolder === '') {
                        shellFrame.CurrentPath = placeHolder;
                    }
                } catch (e) {
                }
                shellFrame.CurrentPath = cPath;
            }
        },
        _markTaskCompleted: function (vault, taskId) {
            var tobjId = new MFiles.ObjID();
            tobjId.SetIDs(MFBuiltInObjectTypeAssignment, taskId);
            var tObjVer = vault.ObjectOperations.GetLatestObjVer(tobjId, true, true);
            vault.ObjectPropertyOperations.MarkAssignmentComplete(tObjVer);
        },
        _newReviewComment: function (vault, objInfo) {//新建校审意见
            //objInfo:{fruitDoc:{ID, Name}, docVersionId:int, notPass:bool, reviewState: int}
            var typeId = MF.alias.objectType(vault, md.reviewLog.typeAlias);
            var classId = MF.alias.classType(vault, md.reviewLog.classAlias);
            var propIdTitle = MF.alias.propertyDef(vault, md.reviewLog.propDefs.NameOrTitle);
            var propIdFruitDoc = MF.alias.propertyDef(vault, md.reviewLog.propDefs.FruitDrawing);
            var propIdDocVer = MF.alias.propertyDef(vault, md.reviewLog.propDefs.DrawSheetVersion);
            var propIdNotPass = MF.alias.propertyDef(vault, md.reviewLog.propDefs.NotPass);
            var propIdReviewState = MF.alias.propertyDef(vault, md.reviewLog.propDefs.ReviewState);
            var pvs = new MFiles.PropertyValues();
            var pass = objInfo.notPass ? "退回" : "通过";
            var title = "<校审意见>" + objInfo.fruitDoc.Name + "(" + pass + "-By_" + vault.SessionInfo.AccountName + ")";
            var pvTitle = MFiles.CreateInstance("PropertyValue");
            pvTitle.PropertyDef = propIdTitle;
            pvTitle.TypedValue.SetValue(MFDatatypeText, title);
            pvs.Add(-1, pvTitle);
            if (objInfo.fruitDoc.ID) {
                var pvFruitDoc = MFiles.CreateInstance("PropertyValue");
                pvFruitDoc.PropertyDef = propIdFruitDoc;
                pvFruitDoc.TypedValue.SetValue(MFDatatypeLookup, objInfo.fruitDoc.ID);
                pvs.Add(-1, pvFruitDoc);
            }
            if (objInfo.docVersionId) {
                var pvDocVer = MFiles.CreateInstance("PropertyValue");
                pvDocVer.PropertyDef = propIdDocVer;
                pvDocVer.TypedValue.SetValue(MFDatatypeInteger, objInfo.docVersionId);
                pvs.Add(-1, pvDocVer);
            }
            var pvNotPass = MFiles.CreateInstance("PropertyValue");
            pvNotPass.PropertyDef = propIdNotPass;
            pvNotPass.TypedValue.SetValue(MFDatatypeBoolean, objInfo.notPass);
            pvs.Add(-1, pvNotPass);
            if (objInfo.reviewState) {
                var pvReviewState = MFiles.CreateInstance("PropertyValue");
                pvReviewState.PropertyDef = propIdReviewState;
                pvReviewState.TypedValue.SetValue(MFDatatypeInteger, objInfo.reviewState);
                pvs.Add(-1, pvReviewState);
            }
            MF.ObjectOps.createObject(vault, typeId, classId, pvs);
        },
        _getFlowTasks: function (vault, drawingVersion) {
            ///<summary>得到工作流任务</summary>
            //if (drawingVersion.HasAssignments === false) return [];
            var relations = vault.ObjectOperations.GetRelationships(drawingVersion.ObjVer, MFRelationshipsModeToThisObject);
            var tasks = [];
            for (var i = 1; i <= relations.Count; i++) {
                var objVersion = relations.Item(i);
                if (objVersion.ObjVer.Type !== MFBuiltInObjectTypeAssignment) continue;
                if (objVersion.Class !== MFBuiltInObjectClassGenericAssignment) continue;
                var taskFlags = objVersion.ObjectVersionFlags;
                if (taskFlags == 1 || taskFlags == 3) {
                    //showMessage('此任务已经被标记为完成！');
                    continue;
                }
                var currentUser = MF.vault.getCurrentUserId(vault);
                try {
                    var assigns = vault.ObjectPropertyOperations.GetProperty(objVersion.ObjVer, MFBuiltInPropertyDefAssignedTo).Value.GetValueAsLookups();
                    if (assigns.Count > 0) {
                        for (var j = 1; j <= assigns.Count; j++) {
                            if (assigns.Item(j).Item === currentUser) {
                                tasks.push(objVersion);
                            }
                        }
                    }

                } catch (e) {
                }
            }
            return tasks;
        }
    };
    u.reviews = reviews;
})(CC);