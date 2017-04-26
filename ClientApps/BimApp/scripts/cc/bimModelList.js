var modellst = {
    init: function (vault, shellFrame,loc) {
        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        //文档类别
        var conditionClass = MFiles.CreateInstance("SearchCondition");
        conditionClass.ConditionType = MFConditionTypeEqual;
        conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, conditionClass);

        var unitId = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.UnitAt);//单体ID
        //过滤没有单体的文件
        var conditionFilter = MFiles.CreateInstance("SearchCondition");
        conditionFilter.ConditionType = MFConditionTypeNotEqual;
        conditionFilter.Expression.DataPropertyValuePropertyDef = unitId;
        conditionFilter.TypedValue.SetValueToNULL(MFDatatypeLookup);
        sConditons.Add(-1, conditionFilter);

        var view = MF.ObjectOps.getTempSearchView(vault, undefined, 0, undefined, sConditons);//获取临时视图 
        var tempView = modellst.getTempViewPath(vault, view);//得到临时视图路径
        var viewId = view.ID;
        modellst.create_dashboard(shellFrame, loc, tempView, vault, viewId);//默认显示所有的文件
    },

    hiddenColumn: function (vault, viewId) {
        var folderDefs = publicUtils.getFolderDefs(viewId);

        var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
        uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;

        for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
            if (uiState.ListingUIState.Columns.Item(j).ID === MFFolderColumnIdScore ||
                uiState.ListingUIState.Columns.Item(j).ID === MFFolderColumnIdName) {
                uiState.ListingUIState.Columns.Item(j).Visible = false;

                break;
            }
        }
        var index = 0;
        for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
            if (uiState.ListingUIState.Columns.Item(j).Visible == true) {
                uiState.ListingUIState.Columns.Item(j).Position = index++;
            }
        }
        vault.ViewOperations.SaveFolderUIStateForFolder(false, false, folderDefs, false, uiState);
    },

    search_criteria: function (vault, arr) {
        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        //文档类别
        var conditionClass = MFiles.CreateInstance("SearchCondition");
        conditionClass.ConditionType = MFConditionTypeEqual;
        conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, conditionClass);

        var propId = modellst.nextPropDefByType(vault, arr[0]);
         
        if (propId !== -1) {
            //其他节点时则搜索出对应节点内容
            //类似文件
            var conditionLike = MFiles.CreateInstance("SearchCondition");//创建搜索条件
            conditionLike.ConditionType = MFConditionTypeEqual;//搜索操作
            conditionLike.Expression.DataPropertyValuePropertyDef = propId;//操作属性
            conditionLike.TypedValue.SetValue(MFDatatypeLookup, arr[1]);//属性值
            sConditons.Add(-1, conditionLike);
        } else {
            var unitId = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.UnitAt);//单体ID
            //过滤没有单体的文件
            var conditionFilter = MFiles.CreateInstance("SearchCondition");
            conditionFilter.ConditionType = MFConditionTypeNotEqual;
            conditionFilter.Expression.DataPropertyValuePropertyDef = unitId;
            conditionFilter.TypedValue.SetValueToNULL(MFDatatypeLookup);
            sConditons.Add(-1, conditionFilter);
        }

        return sConditons;
    },

    getTempViewPath: function (vault, view) {
        var retPath = MF.ObjectOps.getSearchViewLoc(vault, view);
        var arry = retPath.split("\\");
        var tempView = "";
        for (var i = 2; i < arry.length - 1; i++) {
            tempView += arry[i] + "\\";
        }
        return tempView;
    },

    create_dashboard: function (shellFrame, loc, createPath, vault, viewId) {
        try {

            var listingElem = '<div id="listings" class="mf-panel-container" style="min-width: 790px; height: auto;">' +
                              '<div id="listing_model" class="mf-panel" style="width: 100%; height: 50%; padding: 0px;margin: 0px; border: 0px; overflow: hidden;">' +
                              '<div class="mf-listing-content" style="height: 900px">' +
                              '</div>' +
                              '</div>' +
                              '</div>';
            $('#listings').remove();
            $('#fullarea').append(listingElem);
            var listing = createListingForPath(shellFrame, loc, $('#listing_model'), createPath,
                function (container, objectsCount) {
                    var heightNumber = 20;
                    if (objectsCount <= 2)
                        heightNumber = 15;
                    if (objectsCount <= 1)
                        heightNumber = 10;
                    if (objectsCount > 20)
                        heightNumber = 30;
                    container.data("heightNumber", heightNumber);
                });

            this.hiddenColumn(vault, viewId);

            shellFrame.Events.Register(MFiles.Event.ActiveListingChanged, getActiveListingChangedHandler(shellFrame));

            $(window).resize(resized);
            resized();


        } catch (e) { }
    },
    /*
      方法： 向右侧文件列表插入多个文件
      参数： img 图片信息, file 文件信息, parent 父元素id, objNum 文件对象个数
    */
    insert_files: function (plan, img, parent, shellFrame) {
        //var tempTitle = this.subStringByPath(plan.Title);
        var tempTitle = plan.Title + ".rvt";
        var insertText = '<li id=' + plan.ID + '><img src="' + img + '" alt=icon" /><label>' + tempTitle + '</label></li>';
        $('#' + parent).first().append($(insertText));
        $("#" + plan.ID + "").click(function () {
            var objId = plan.ID;
            var objType = plan.Type;
            var objectId = MFiles.CreateInstance('ObjID');
            objectId.SetIDs(objType, objId);
            var vault = shellFrame.ShellUI.Vault;
            var objVer = vault.ObjectOperations.GetLatestObjVer(objectId, true, false);
            MF.ui.selectObject(shellFrame, objVer);
            $(this).addClass("on").siblings().removeClass("on");
        });
        $("#" + plan.ID + "").dblclick(function (e) {
            var objId = plan.ID;
            var objType = plan.Type;
            var objectId = MFiles.CreateInstance('ObjID');
            objectId.SetIDs(objType, objId);
            var vault = shellFrame.ShellUI.Vault;
            var objVer = vault.ObjectOperations.GetLatestObjVer(objectId, true, false);
            var files = vault.ObjectFileOperations.GetFiles(objVer);
            if (files.Count > 0) {
                vault.ObjectFileOperations.OpenFileInDefaultApplication(0, objVer, files.Item(1).FileVer, MFFileOpenMethodOpen);
            }
        });
    },
    subStringByPath: function (path) {
        var str = path;
        var slashIndex = str.lastIndexOf('-');
        if (slashIndex === -1) return path;
        return str.substr(slashIndex + 1);
    },
    searchModelPlans: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var objVns = this.getModelPlansByDomAt(vault, currentDomObj);
        var res = [];
        var isProject = CC.treeDomOps.isProjectDom(vault, currentDomObj.ObjType);
        for (var i = 1; i <= objVns.Count; i++) {
            var obj = objVns.Item(i);
            var props = vault.ObjectPropertyOperations.GetProperties(obj.ObjVer, true);

            var currentObjId = this.nextPropDefByType(vault, currentDomObj.ObjType);
            if (currentObjId === -1) return;
            //this.isNextNull(currentObjId, vault, props) 
            if (props.SearchForProperty(currentObjId).Value.IsNULL() == false) {
                //忽略模板
                var plan = this.getPlanByProps(vault, obj.ObjVer.Type, obj.ObjVer.ID, props);
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
            if (planObj === undefined) return;
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
    createModelPlan: function (shellFrame, currentDomObj) {
        var vault = shellFrame.ShellUI.Vault;
        var propIdDomAt = this.nextPropDefByType(vault, currentDomObj.Type);
        if (propIdDomAt === -1) return undefined;
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
    getModelPlansByDomAt: function (vault, currentDomObj) {

        var classId = MF.alias.classType(vault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        //文档类别
        var conditionClass = MFiles.CreateInstance("SearchCondition");
        conditionClass.ConditionType = MFConditionTypeEqual;
        conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
        sConditons.Add(-1, conditionClass);
        var propIdDomAt = this.nextPropDefByType(vault, currentDomObj.ObjType);

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
    getPlanByProps: function (vault, type, id, props) {
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
        var unitStatus = this.lookupDeleted(props.SearchForProperty(propUnit));
        var propFloor = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.FloorAt);
        var floorStatus = this.lookupDeleted(props.SearchForProperty(propFloor));
        var propDiscipine = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciplineAt);
        var discipineStatus = this.lookupDeleted(props.SearchForProperty(propDiscipine));
        if (unitStatus || floorStatus || discipineStatus) {
            deletedStatus = true;
        }
        return {
            'Type': type,
            'ID': id,
            'Title': title,
            'Template': template,
            'Number': number,
            'Designer': designer,
            'DisciLeader': disciLeader,
            'Deadline': deadline,
            'FinishedTime': finishedTime,
            'DeletedStatus': deletedStatus
        };
    },
    nextPropDefByType: function (vault, objType) {
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
    isNextNull: function (currentObjId, vault, props) {
        var modelunitid = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.UnitAt);  //获取单体id通过别名
        var floorid = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.FloorAt);
        var disciplineid = MF.alias.propertyDef(vault, md.bimModelDoc.propDefs.DisciplineAt);

        switch (currentObjId) {

            case modelunitid://如果是单体就判断楼层是不是为空 
                if (props.SearchForProperty(floorid).Value.IsNULL() == true) {
                    return true;
                }
                break;
            case floorid://如果是楼层就判断专业是不是为空
                if (props.SearchForProperty(disciplineid).Value.IsNULL() == true) {
                    return true;
                }
                break;
            case disciplineid://如果是单体就判断专业是不是为空
                return true;
        }
        return false;
    },
    //单选的属性值是否被删除
    lookupDeleted: function (propValue) {
        var tvalue = propValue.Value;
        if (tvalue.IsNULL() == false) {
            var lookup = tvalue.GetValueAsLookup();
            if (lookup.Deleted) {
                return true;
            }
        }
        return false;
    }
}