/*****************
*上传到校审区的命令回调函数,只能一张张上传
*****************/
var cmdCallbackFn = cmdCallbackFn || {};

(function (fn) {
    fn.copy2Review = function (shellFrame) {
        ///<summary>上传校审文档</summary>
        var that = this;
        return function() {
            var objVnsAndProps = shellFrame.Listing.CurrentSelection.ObjectVersionsAndProperties;
            if (objVnsAndProps.Count == 0 || objVnsAndProps.Count > 1) {
                return;
            }
            that.copy2ReviewArea(shellFrame, objVnsAndProps.Item(1));
        };
    };
    fn.copy2ReviewArea = function(shellFrame, objVnProps) {
        var vault = shellFrame.ShellUI.Vault;
        var docClassId = MF.alias.classType(vault, md.designDoc.classAlias);
        var propIdDisci = MF.alias.propertyDef(vault, md.designDoc.propDefs.Discipline);
        
        var properties = objVnProps.Properties;
        var objVersion = objVnProps.VersionData;

        if (objVersion.Class !== docClassId) {
            shellFrame.ShellUI.ShowMessage("请选择编辑区的设计文档！");
            return;
        }
        if (!this._hasPermission(vault, properties)) {
            shellFrame.ShellUI.ShowMessage("您无权限上传该文档！");
            return;
        }
        var files = objVersion.Files;
        if (files.Count == 0 || files.Count > 1) {
            shellFrame.ShellUI.ShowMessage("选中的设计文档应为单文档！");
            return;
        }
        var objFile = files.Item(1);
        if (objFile.Extension !== "pdf") {
            shellFrame.ShellUI.ShowMessage("请选中PDF格式的设计文档！");
            return;
        }
        var dtvalue = properties.SearchForProperty(propIdDisci).Value;
        var disciplineId = dtvalue.GetLookupID();
        var disciplineTitle = dtvalue.DisplayValue;
        var drawingTitle = objFile.Title + "." + objFile.Extension;
        
        var selectedPlan = this._getSelectedPlan(shellFrame, disciplineId, disciplineTitle, drawingTitle);
        if (selectedPlan) { 
            var srcFilePath = vault.ObjectFileOperations.GetPathInDefaultView(objVersion.ObjVer.ObjID,
                objVersion.ObjVer.Version, objFile.ID, objFile.Version, MFLatestSpecificBehaviorAutomatic, false);
            var srcObjectFile = MFiles.CreateInstance("SourceObjectFile");
            srcObjectFile.Title = objFile.Title;
            srcObjectFile.Extension = objFile.Extension;
            srcObjectFile.SourceFilePath = srcFilePath;
            try {
                var fruitDocVn = this._searchFruitDoc(vault, selectedPlan.ID);
                if (fruitDocVn) {
                    if (this._isReviewing(vault, fruitDocVn)) {
                        shellFrame.ShellUI.ShowMessage("正在校审中，不允许更新！");
                        return;
                    }
                    var flag = this._updatingTip(shellFrame, selectedPlan.Title +"."+ srcObjectFile.Extension);
                    if (flag) {
                        this._updateFruitDoc(vault, fruitDocVn, srcObjectFile);
                    }
                } else {
                    this._createFruitDocs(vault, selectedPlan, srcObjectFile);
                    shellFrame.ShellUI.ShowMessage("已成功上传校审文件!");
                }
            }catch (e){}
        };
    };

    //从库中搜索设计策划
    fn._getAllDesignPlans = function(vault, disciplineId) {
        var typeIdPlan = MF.alias.objectType(vault, md.drawingPlan.typeAlias);
        var propIdDisci = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Discipline);

        var sConditons = MFiles.CreateInstance("SearchConditions");
        if (disciplineId) {
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef = propIdDisci;
            condition.TypedValue.SetValue(MFDatatypeLookup, disciplineId);
            sConditons.Add(-1, condition);
        }
        var sResults = MF.ObjectOps.SearchObjects(vault, typeIdPlan, sConditons);
        var plans = [];
        for (var i = 1; i <= sResults.Count; i++) {
            var item = sResults.Item(i);
            var plan = {
                'ID': item.ObjVer.ID,
                'Title': item.Title
            }
            plans.push(plan);
        }
        return plans;
    };
        //从弹出框中选择设计策划
    fn._getSelectedPlan = function(shellFrame, disciplineId, disciplineTitle, drawingTitle) {
        var vault = shellFrame.ShellUI.Vault;
        var srcPlans = this._getAllDesignPlans(vault, disciplineId);
        //shellFrame.ShellUI.ShowMessage(srcPlans.length);
        var selectedPlan = undefined;
        var dashboardData = {
            'Plans': srcPlans,
            'SelectedPlan': selectedPlan,
            'Cancelled': false,
            'DisciplineTitle': disciplineTitle,
            'DrawingTitle': drawingTitle
        };
        shellFrame.ShowPopupDashboard('dtselectdocplan', true, dashboardData);
        if (dashboardData.Cancelled == false) {
            return dashboardData.SelectedPlan;
        }
        return undefined;
    };
    //判断当前用户是否有共享的权限
    fn._hasPermission = function(vault, properties) {
            var userId = vault.SessionInfo.UserID;
            var creator = properties.SearchForProperty(25).Value.GetLookupID();
            return userId === creator;
        };
    //设计策划==>成果图纸
    fn._copyProps2FruitDoc = function (vault, srcProperties) {
        
        var propIdTitle = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.NameOrTitle); //名称或标题
        var propIdPhase = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DesignPhase); //阶段
        var propIdDiscipline = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Discipline); //专业
        var propIdPhase2 = MF.alias.propertyDef(vault, md.drawingSheet.propDefs.DesignPhase); //设计阶段
        var propIdDiscipline2 = MF.alias.propertyDef(vault, md.drawingSheet.propDefs.Discipline); //策划专业
        var propIdDrawingTitle = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DrawingTitle); //图名
        var propIdDrawingNumber = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DrawingNumber); //图号
        var propIdDrawingScale = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DrawingScale); //比例
        var propIdFrameSize = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.FrameSize); //图幅
        var propIdDrawingPerson = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DrawingPerson); //绘图人
        var propIdDesigner = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Designer); //设计人
        var propIdVerifier = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Verifier); //校对人
        var propIdChecker = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Checker); //审核人
        var propIdValidator = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Validator); //审定人
        var propIdDisciLead = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DisciplineLead); //专业负责人
        var propIdChiefDesigner = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.ChiefDesigner); //设总
        var propIdPm = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.ProjManager); //项目经理

        var prfillProps = new MFiles.PropertyValues();

        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdTitle));

        var phase = srcProperties.SearchForProperty(propIdPhase).Value.GetValueAsLookup();
        var pvPhase = new MFiles.PropertyValue();
        pvPhase.PropertyDef = propIdPhase2;
        pvPhase.TypedValue.SetValue(MFDatatypeLookup, phase);
        prfillProps.Add(-1, pvPhase);

        var discipline = srcProperties.SearchForProperty(propIdDiscipline).Value.GetValueAsLookup();
        var pvDiscipline = new MFiles.PropertyValue();
        pvDiscipline.PropertyDef = propIdDiscipline2;
        pvDiscipline.TypedValue.SetValue(MFDatatypeLookup, discipline);
        prfillProps.Add(-1, pvDiscipline);

        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDrawingTitle));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDrawingNumber));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDrawingScale));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdFrameSize));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDrawingPerson));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDesigner));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdVerifier));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdChecker));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdValidator));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdDisciLead));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdChiefDesigner));
        prfillProps.Add(-1, srcProperties.SearchForProperty(propIdPm));
        
        return prfillProps;
    };
    //新建成果图纸
    fn._createFruitDocs = function(vault, designPlan, srcObjFile) {
        var typeIdPlan = MF.alias.objectType(vault, md.drawingPlan.typeAlias);
        var fruitDocClass = MF.alias.classType(vault, md.drawingSheet.classAlias);
        var propIdPlan = MF.alias.propertyDef(vault, md.drawingSheet.propDefs.DrawingPlan);
        var propIdState = MF.alias.propertyDef(vault, md.drawingSheet.propDefs.DocStatus);

        var oObjVer = new MFiles.ObjVer();
        oObjVer.SetIDs(typeIdPlan, designPlan.ID, -1);
        var oProperties = vault.ObjectPropertyOperations.GetProperties(oObjVer, false);
        var prfillProps = this._copyProps2FruitDoc(vault, oProperties);

        //设计策划
        var pvPlan = MFiles.CreateInstance('PropertyValue');
        pvPlan.PropertyDef = propIdPlan;
        pvPlan.TypedValue.SetValue(MFDatatypeLookup, designPlan.ID);
        prfillProps.Add(-1, pvPlan);
        //文档状态
        if (!this.docStatus) {
            var valueListId = MF.alias.valueList(vault, md.valueList.DocStatus);
            this.docStatus = MF.vault.getValueListItemId(vault, valueListId, "校审");
        }
        var pvState = MFiles.CreateInstance('PropertyValue');
        pvState.PropertyDef = propIdState;
        pvState.TypedValue.SetValue(MFDatatypeLookup, this.docStatus);
        prfillProps.Add(-1, pvState);

        MF.ObjectOps.createSingleFile(vault, fruitDocClass, prfillProps, srcObjFile);
    };
    //搜索成果图纸
    fn._searchFruitDoc = function (vault, drawingPlanId) {
        if (drawingPlanId < 1) return null;
        var classId = MF.alias.classType(vault, md.drawingSheet.classAlias);
        var propIdPlan = MF.alias.propertyDef(vault, md.drawingSheet.propDefs.DrawingPlan);

        var sConditons = MFiles.CreateInstance("SearchConditions");
        var scClass = MFiles.CreateInstance("SearchCondition");
        scClass.ConditionType = MFConditionTypeEqual;
        scClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        scClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, scClass);

        if (drawingPlanId) {
            var scPlan = MFiles.CreateInstance("SearchCondition");
            scPlan.ConditionType = MFConditionTypeEqual;
            scPlan.Expression.DataPropertyValuePropertyDef = propIdPlan;
            scPlan.TypedValue.SetValue(MFDatatypeLookup, drawingPlanId);
            sConditons.Add(-1, scPlan);
        }
        var sResults = MF.ObjectOps.SearchObjects(vault, 0, sConditons);
        return sResults.Count > 0 ? sResults.Item(1) : null;
    };
    //判断是否正在校审，只有在"设计"才能更新
    fn._isReviewing = function (vault, objversion) {
        var flag = true;
        var props = vault.ObjectPropertyOperations.GetProperties(objversion.ObjVer, false);
        var stateId = MF.alias.workflowState(vault, md.reviewFlow.stateAlias.Designer);
        if (props.IndexOf(MFBuiltInPropertyDefState) != -1) {
            var tValue = props.SearchForProperty(MFBuiltInPropertyDefState).Value;
            if (tValue.isNULL() == false && tValue.GetLookupID() === stateId) {
                flag = false;
            }
        }
        return flag;
    };
    //更新成果图纸
    fn._updateFruitDoc = function (vault, objversion, srcObjFile) {
        var checkedOutVn;
        if (objversion.ObjectCheckedOut) checkedOutVn = objversion;
        else checkedOutVn = vault.ObjectOperations.CheckOut(objversion.ObjVer.ObjID);
        try {
            if (checkedOutVn.SingleFile) {
                vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, false);
            }
            for (var i = 1; i <= checkedOutVn.FilesCount; i++) {
                var fileItem = checkedOutVn.Files.Item(i);
                vault.ObjectFileOperations.RemoveFile(checkedOutVn.ObjVer, fileItem.FileVer);
            }
            try {
                vault.ObjectFileOperations.AddFile(checkedOutVn.ObjVer, srcObjFile.Title, srcObjFile.Extension, srcObjFile.SourceFilePath);
            } catch (e) {
            }
            vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, true);
        } catch (e) {
        }
        vault.ObjectOperations.CheckIn(checkedOutVn.ObjVer);
    };
    //
    fn._updatingTip = function(shellFrame, title) {
        var msg = "您确定要覆盖图纸<" + title + ">？";
        var clickBtn = shellFrame.ShellUI.ShowMessage({
            caption: "更新提示",
            message: msg,
            icon: "warning",
            button1_title: "确定",
            button2_title: "取消",
            defaultButton: 1,
            timeOutButton: 2,
            timeOut: 30
        });
        if (clickBtn == 1) return true;
        return false;
    };
})(cmdCallbackFn);