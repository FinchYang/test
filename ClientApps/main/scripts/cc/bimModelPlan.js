var modelPlanOps = {
    createDomObject: function (shellFrame, parentDomObj) {
        //取消新建 返回 undefined
        var dashboardData = { "ParentTitle": parentDomObj.DisplayName, "Title": "", "BtnTitle": "新建子项" };
        shellFrame.ShowPopupDashboard('bimTreeDom', true, dashboardData);
        if (dashboardData.Title) {
            var vault = shellFrame.ShellUI.Vault;
            //shellFrame.ShellUI.ShowMessage(dashboardData.Title);
            var obj = undefined;
            if (CC.treeDomOps.isProjectDom(vault, parentDomObj.ObjType)) {
                
                obj = CC.treeDomOps.createUnit(vault, dashboardData.Title);                
            } else if (CC.treeDomOps.isUnitDom(vault, parentDomObj.ObjType)) {
                obj = CC.treeDomOps.createFloor(vault, dashboardData.Title, parentDomObj.ID);
            } else if (CC.treeDomOps.isFloorDom(vault, parentDomObj.ObjType)) {
                obj = CC.treeDomOps.createDiscipline(vault, dashboardData.Title, parentDomObj.ID);
            }
            if (obj) {
                try {
                    //this.createModelPlan(shellFrame, obj);
                } catch (e) {
                    shellFrame.ShellUI.ShowMessage(e.message);
                }
            }
            return obj;
        }
        return undefined;
        //return {"Type":1, "ID":1, "Title": "123"};
    },
    renameDomObject: function (shellFrame, currentDomObj) {
        var dashboardData = { "ParentTitle": currentDomObj.ParentTitle, "Title": currentDomObj.DisplayName, "BtnTitle": "重命名" };
        shellFrame.ShowPopupDashboard('bimTreeDom', true, dashboardData);
        if (dashboardData.Title) {
            var vault = shellFrame.ShellUI.Vault;
            //shellFrame.ShellUI.ShowMessage(dashboardData.Title);
            CC.treeDomOps.renameDom(vault, currentDomObj.ObjType, currentDomObj.ID, dashboardData.Title);
            return { "Type": currentDomObj.ObjType, "ID": currentDomObj.ID, "Title": dashboardData.Title };
        }
        return undefined;
        //return {"Type":1, "ID":1, "Title": "123"};
    },
    deleteDomObject: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        CC.treeDomOps.deleteDom(vault, currentDomObj.ObjType, currentDomObj.ID);
    },
    deleteDomTip: function (shellFrame, currentObj) {
        var msg = "您确定要删除<" + currentObj.DisplayName + ">及其子项相关内容？";
        var clickBtn = shellFrame.ShellUI.ShowMessage({
            caption: "删除提示",
            message: msg,
            icon: "warning",
            button1_title: "确定",
            button2_title: "取消",
            defaultButton: 2,
            timeOutButton: 2,
            timeOut: 30
        });
        if (clickBtn == 1) return true;
        return false;
    },
    createModelPlan: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var propIdDomAt = this._getPropDefByType(vault, currentDomObj.Type);
        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var propValues = MFiles.CreateInstance('PropertyValues');
        var propIdTitle = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ModelName);
        //模型名称
        var propTitle = MFiles.CreateInstance('PropertyValue');
        propTitle.PropertyDef = propIdTitle;
        propTitle.TypedValue.SetValue(MFDatatypeText, currentDomObj.Title);
        propValues.Add(-1, propTitle);
        //alert(propIdDomAt + "-" + currentDomObj.ID);
        if (propIdDomAt > 0) {
            //所在...
            var propAt = MFiles.CreateInstance('PropertyValue');
            propAt.PropertyDef = propIdDomAt;
            propAt.TypedValue.SetValue(MFDatatypeLookup, currentDomObj.ID);
            propValues.Add(-1, propAt);
        }
        var resVn = MF.ObjectOps.createObject(vault, 0, classId, propValues, undefined);
        return { "Type": resVn.ObjVer.Type, "ID": resVn.ObjVer.ID, "Title": resVn.Title };
    },
    updateModelPlan: function (shellFrame, currentObj) {
        var vault = shellFrame.ShellUI.Vault;
        var oObjVer = MFiles.CreateInstance("ObjVer");
        oObjVer.SetIDs(currentObj.Type, currentObj.ID, -1);
        var winResult = vault.ObjectOperations.ShowEditObjectWindow(0, MFObjectWindowModeEdit, oObjVer);
        if (winResult.Result == MFObjectWindowResultCodeOk) {
            return this._getPlanByProps(vault, currentObj.Type, currentObj.ID, winResult.Properties);
        }
        return undefined;
    },
    deleteModelPlan: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var plans = this._getModelPlansByDomAt(vault, currentDomObj);
        for (var i = 1; i <= plans.Count; i++) {			
            var planVn = plans.Item(i);
            var isTemplate = false;
            try {
                isTemplate = vault.ObjectPropertyOperations.GetProperties(planVn.ObjVer, 37).Value.Value;
            } catch (e) {
                //alert(e.message);prop
            }
			//alert("delete:" + planVn.Title + " plansLenth:" + plans.Count + " isTemplate:" + isTemplate);
            try {
                if (!isTemplate) {					
					//删除模型策划（BIM模型）的关联对象
					var conditions = MFiles.CreateInstance("SearchConditions");
					var condition = MFiles.CreateInstance("SearchCondition");
					condition.ConditionType = MFConditionTypeEqual;
					condition.Expression.DataPropertyValuePropertyDef = MF.alias.propertyDef(vault, md.bimPart.propDefs.OwnedModel);
					condition.TypedValue.SetValue(MFDatatypeLookup, planVn.ObjVer.ID);
					conditions.Add(-1,condition);
					var objVns = vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlagNone, false);
					for(var j = 1; j <= objVns.Count; j++){
						var obj = objVns.Item(j);
						MF.ObjectOps.DeleteObject(vault,obj.ObjVer.Type,obj.ObjVer.ID);
					}	
					//删除模型策划（BIM模型）
                    MF.ObjectOps.DeleteObject(vault, planVn.ObjVer.Type, planVn.ObjVer.ID);
                }
            } catch (e) {
                alert(e.message);
            }
        }

    },
    searchModelPlansOld: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var objVns = this._getModelPlansByDomAt(vault, currentDomObj);
        var res = [];
        var isProject = CC.treeDomOps.isProjectDom(vault, currentDomObj.ObjType);
        for (var i = 1; i <= objVns.Count; i++) {
            var obj = objVns.Item(i);
            var props = vault.ObjectPropertyOperations.GetProperties(obj.ObjVer, false);
            if (this._isTemplate(props) === false) {
                //忽略模板
                var plan = this._getPlanByProps(vault, obj.ObjVer.Type, obj.ObjVer.ID, props);
                if (isProject && plan.DeletedStatus === false) {
                    //当点击项目时，忽略那些 文档上有属性残留的文档(事实上这些文档应该被删除的)
                    res.push(plan);
                } else {
                    res.push(plan);
                }
            }
        }
        if (res.length === 0 && isProject) {//首次点击项目节点时
            //alert(currentDomObj.DisplayName);
            var planObj = this.createModelPlan(shellFrame, currentDomObj);
            res.push({
                'Type': planObj.Type,
                'ID': planObj.ID,
                'Title': planObj.Title,
                'Template': "",
                'Number': "",
                'Designer': "",
                'DisciLeader': "",
                'Deadline': "",
                'FinishedTime': "",
                'DeletedStatus': false
            });
        }
        res.sort(function (a, b) {
            return a.ID - b.ID;
        });
        return res;

    },
    _isTemplate:function(properties) {//判断是否为模板
        if (properties.IndexOf(37) == -1) return false;
        else {
            var tvalue = properties.SearchForProperty(37).Value;
            if (tvalue.IsNULL() == false && tvalue.Value == true) return true;
            else return false;
        }
    },
    _getModelPlansByDomAt: function (vault, currentDomObj) {

        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        //文档类别
        var conditionClass = MFiles.CreateInstance("SearchCondition");
        conditionClass.ConditionType = MFConditionTypeEqual;
        conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, conditionClass);
        var propIdDomAt = this._getPropDefByType(vault, currentDomObj.ObjType);

        if (propIdDomAt > 0) {
            var conditionDomAt = MFiles.CreateInstance("SearchCondition");
            conditionDomAt.ConditionType = MFConditionTypeEqual;
            conditionDomAt.Expression.DataPropertyValuePropertyDef = propIdDomAt;
            conditionDomAt.TypedValue.SetValue(MFDatatypeLookup, currentDomObj.ID);
            sConditons.Add(-1, conditionDomAt);
        }
        var objVns = MF.ObjectOps.SearchObjects(vault, 0, sConditons);
        return objVns;
    },
    _getPlanByProps: function (vault, type, id, props) {
        var deletedStatus = false;//对象状态正常
        var propTitle = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ModelName);
        var title = props.SearchForProperty(propTitle).Value.DisplayValue;
        var propTemplate = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.TemplateAt);
        var template = props.SearchForProperty(propTemplate).Value.DisplayValue;
        var propNumber = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ModelNumber);
        var number = props.SearchForProperty(propNumber).Value.DisplayValue;
        var propDesigner = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ModelCreator);
        var designer = props.SearchForProperty(propDesigner).Value.DisplayValue;
        var propDisciLeader = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciLeader);
        var disciLeader = props.SearchForProperty(propDisciLeader).Value.DisplayValue;
        var propDeadline = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.Deadline);
        var deadline = props.SearchForProperty(propDeadline).Value.DisplayValue;
        var propFinishedTime = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.ActualFinishDate);
        var finishedTime = props.SearchForProperty(propFinishedTime).Value.DisplayValue;
        var propUnit = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.UnitAt);
        var unitStatus = this._lookupDeleted(props.SearchForProperty(propUnit));
        var propFloor = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.FloorAt);
        var floorStatus = this._lookupDeleted(props.SearchForProperty(propFloor));
        var propDiscipine = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciplineAt);
        var discipineStatus = this._lookupDeleted(props.SearchForProperty(propDiscipine));
        if (unitStatus || floorStatus || discipineStatus) {
            deletedStatus = true;
        }
        return {
            'Type': type,
            'ID': id,
            'Title': title,
            //'Template': template,
            'Number': number,
            'Designer': designer,
            'DisciLeader': disciLeader,
            'Deadline': deadline,
            'FinishedTime': finishedTime,
            'DeletedStatus': deletedStatus
        };
    },
    //单选的属性值是否被删除
    _lookupDeleted: function (propValue) {
        var tvalue = propValue.Value;
        if (tvalue.IsNULL() == false) {
            var lookup = tvalue.GetValueAsLookup();
            if (lookup.Deleted) {
                return true;
            }
        }
        return false;
    },
    _getPropDefByType: function (vault, objType) {
        var propIdDomAt = -1;
        if (CC.treeDomOps.isProjectDom(vault, objType)) {//当前节点是项目

        } else if (CC.treeDomOps.isUnitDom(vault, objType)) {//单体
            propIdDomAt = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.UnitAt);
        } else if (CC.treeDomOps.isFloorDom(vault, objType)) {//楼层
            propIdDomAt = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.FloorAt);
        } else if (CC.treeDomOps.isDisciplineDom(vault, objType)) {//专业
            propIdDomAt = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciplineAt);
        }
        return propIdDomAt;
    },

    setSearchConditons: function(shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        //文档类别
        var conditionClass = MFiles.CreateInstance("SearchCondition");
        conditionClass.ConditionType = MFConditionTypeEqual;
        conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, conditionClass);
        var propIdDomAt = modelPlanOps._getPropDefByType(vault, currentDomObj.ObjType);

        if (propIdDomAt > 0) {
            var conditionDomAt = MFiles.CreateInstance("SearchCondition");
            conditionDomAt.ConditionType = MFConditionTypeEqual;
            conditionDomAt.Expression.DataPropertyValuePropertyDef = propIdDomAt;
            conditionDomAt.TypedValue.SetValue(MFDatatypeLookup, currentDomObj.ID);
            sConditons.Add(-1, conditionDomAt);
        }
        //对象类型
        var conditionType = MFiles.CreateInstance("SearchCondition");
        conditionType.ConditionType = MFConditionTypeEqual;
        conditionType.Expression.DataStatusValueType = MFStatusTypeObjectTypeID;
        conditionType.TypedValue.SetValue(MFDatatypeLookup, 0);
        sConditons.Add(-1, conditionType);
        //是否删除
        var conditionDeleted = MFiles.CreateInstance("SearchCondition");
        conditionDeleted.ConditionType = MFConditionTypeEqual;
        conditionDeleted.Expression.DataStatusValueType = MFStatusTypeDeleted;
        conditionDeleted.TypedValue.SetValue(MFDatatypeBoolean, false);
        sConditons.Add(-1, conditionDeleted);
        return sConditons;
    }
}

var modelPlanOps = modelPlanOps || {};
//更新模板
(function(o) {
    o.setModelPlan = function(shellFrame, modelPlan) {
        //modelPlan:{Type: int, ID: int, Template: string}
        //return: bool
        var vault = shellFrame.ShellUI.Vault;
        var propIdTemplate = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.TemplateAt);
        var planVnProps = this._getObjVersionProps(vault, modelPlan.Type, modelPlan.ID, -1);
        if (!planVnProps) return false;
        
        var version = planVnProps.ObjVer.Version;
        
        var planTitleOld = "";
        if (version > 2) {
            var planVnPropsOld = this._getObjVersionProps(vault, modelPlan.Type, modelPlan.ID, version - 1);
            var tValueOld = planVnPropsOld.Properties.SearchForProperty(propIdTemplate).Value;
            planTitleOld = tValueOld.DisplayValue;
            //if (tValueOld.IsNULL() === false) {
            //    planIdOld = tValueOld.GetLookupID();
            //}
        }
        
        var tValue = planVnProps.Properties.SearchForProperty(propIdTemplate).Value;
        var planTitle = tValue.DisplayValue;
        if (planTitleOld != "" && planTitle === planTitleOld) {
            return false;
        }

        var templateFile = this._getTemplateFile(vault, tValue.GetLookupID());
        this._setModelPlanTemplate(vault, planVnProps.VersionData, templateFile);
        return true;
    };
    o._getObjVersionProps = function (vault, type, id, version) {
        try {
            var oObjver = MFiles.CreateInstance("ObjVer");
            oObjver.SetIDs(type, id, version);
            var versionProps = vault.ObjectOperations.GetObjectVersionAndProperties(oObjver,false);
            return versionProps;//{ VersionData, Properties}
        } catch (e) {
            return null;
        }
    };
    o._getTemplateFile = function(vault, templateId) {
        var temObjver = MFiles.CreateInstance("ObjVer");
        temObjver.SetIDs(0, templateId, -1);
        var temVn = vault.ObjectOperations.GetObjectInfo(temObjver, true, false);
        if (temVn.FilesCount < 1) {
            return null;
        }
        var temFile = temVn.Files.Item(1);
        var ext = temFile.Extension;
        var filePath = vault.ObjectFileOperations.GetPathInDefaultView(temVn.ObjVer.ObjID, temVn.ObjVer.Version,
            temFile.ID, temFile.Version, MFLatestSpecificBehaviorAutomatic, false);
        return { filePath: filePath, extension: ext }
    };
    o._setModelPlanTemplate = function(vault,objectVersion, templateFile) {
        var checkedOutVn;
        if (objectVersion.ObjectCheckedOut) checkedOutVn = objectVersion;
        else checkedOutVn = vault.ObjectOperations.CheckOut(objectVersion.ObjVer.ObjID);
        try {
            if (checkedOutVn.SingleFile) {
                vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, false);
            }
            for (var i = 1; i <= checkedOutVn.FilesCount; i++) {
                var fileItem = checkedOutVn.Files.Item(i);
                //if (fileItem.Title === objectVersion.Title && fileItem.Extension === templateFile.extension) continue;
                vault.ObjectFileOperations.RemoveFile(checkedOutVn.ObjVer, fileItem.FileVer);
            }
            try {
                vault.ObjectFileOperations.AddFile(checkedOutVn.ObjVer, objectVersion.Title, templateFile.extension, templateFile.filePath);
            } catch (e) {
            }
            vault.ObjectOperations.SetSingleFileObject(checkedOutVn.ObjVer, true);
        } catch (e) {
        }
        vault.ObjectOperations.CheckIn(checkedOutVn.ObjVer);
    };
})(modelPlanOps);