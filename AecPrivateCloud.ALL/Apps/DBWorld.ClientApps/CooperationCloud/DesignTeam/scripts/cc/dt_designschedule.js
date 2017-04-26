/*
设计进度管理
*/
var CC = CC || {};
(function (u, undefined) {
    var designschedule = {
        /*创建视图搜索条件*/
        searchCriteria: function (vault, designPhaseId, professionalId, statusId) {
            
            var sConditons = MFiles.CreateInstance("SearchConditions");
  
            var classId = MF.alias.classType(vault, md.drawingPlan.classAlias);
            var classS = MFiles.CreateInstance("SearchCondition");//创建搜索条件
            classS.ConditionType = MFConditionTypeEqual;//搜索操作
            classS.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;//操作属性
            classS.TypedValue.SetValue(MFDatatypeLookup, classId);//属性值
            sConditons.Add(-1, classS);

            if (designPhaseId !== "全部") {
                var propIdDesignPhase = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DesignPhase);//阶段
                var designPhase = MFiles.CreateInstance("SearchCondition");//创建搜索条件
                designPhase.ConditionType = MFConditionTypeEqual;//搜索操作
                designPhase.Expression.DataPropertyValuePropertyDef = propIdDesignPhase;//操作属性
                designPhase.TypedValue.SetValue(MFDatatypeLookup, designPhaseId);//属性值
                sConditons.Add(-1, designPhase);
            }

            if (professionalId !== "全部") {
                var propIdDiscipline = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Discipline);//专业
                var professional = MFiles.CreateInstance("SearchCondition");
                professional.ConditionType = MFConditionTypeEqual;
                professional.Expression.DataPropertyValuePropertyDef = propIdDiscipline;
                professional.TypedValue.SetValue(MFDatatypeLookup, professionalId);
                sConditons.Add(-1, professional);
            }


            if (statusId !== "全部") {
                var propIdPlanProgress = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.PlanProgress);//进度
                var status = MFiles.CreateInstance("SearchCondition");
                status.ConditionType = MFConditionTypeEqual;
                status.Expression.DataPropertyValuePropertyDef = propIdPlanProgress;
                status.TypedValue.SetValue(MFDatatypeLookup, statusId);
                sConditons.Add(-1, status);
            }

            return sConditons;
        },
        /*获取临时视图路径*/
        getTempviewpath: function (vault, view) {
            var retPath = MF.ObjectOps.getSearchViewLoc(vault, view);
            var arry = retPath.split("\\");
            var tempView = "";
            for (var i = 2; i < arry.length - 1; i++) {
                tempView += arry[i] + "\\";
            }
            return tempView;
        },
        /*创建临时视图*/
        createDashboard: function (shellFrame, loc, createPath, vault, viewId) {
            try {

                var listingElem = '<div id="listings" class="mf-panel-container" style="min-width: 790px; height: auto;">' +
                                  '<div id="listing_model" class="mf-panel" style="width: 100%; height: 50%; padding: 0px;margin: 0px; border: 0px; overflow: hidden;">' +
                                  '<div class="mf-listing-content" style="height: 900px">' +
                                  '</div>' +
                                  '</div>' +
                                  '</div>';
                $('#listings').remove();
                $('#designprogress').append(listingElem);
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

                if (listing) CC.SetListingTheme(listing);

                this.hiddenColumn(vault, viewId);

                shellFrame.Events.Register(MFiles.Event.ActiveListingChanged, getActiveListingChangedHandler(shellFrame));

                $(window).resize(resized);
                resized();

            } catch (e) { }
        },
        /*填充计阶段下拉列表*/
        setSelest: function (valuelst,select) {
            var selectid = document.getElementById(select);
            for (var i = 1; i <= valuelst.length; i++) {
                var newOption = new Option(valuelst.item(i).Name, valuelst.item(i).ID);
                selectid[i] = newOption;
            }
            //$("#status").find("option:first").remove();
        },
        /*填充专业下拉列表*/
        setSelectProfessional: function (obj,select) {
            var selectid = document.getElementById(select);
            for (var i = 1; i <= obj.Count; i++) {
                selectid[i] = new Option(obj.item(i).Title, obj.item(i).ObjVer.ID);
            }
            //$("#professional").find("option:first").remove();
        },
        /*根据别名获取对应的值列表 alias:别名名称*/
        getValuelist: function (vault, alias) {
            var id = MF.alias.valueList(vault, alias);//通过别名获取ID
            var valuelst = vault.ValueListItemOperations.GetValueListItems(id, false, MFExternalDBRefreshTypeNone);//获取值列表
            return valuelst;
        },
        /*获取专业*/
        getProfessional: function (vault) {
            var classId = MF.alias.classType(vault, md.planDiscipline.classAlias);
            var sConditons = MFiles.CreateInstance("SearchConditions");

            var conditionClass = MFiles.CreateInstance("SearchCondition");
            conditionClass.ConditionType = MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
            sConditons.Add(-1, conditionClass);

            var typeId = MF.alias.objectType(vault, md.planDiscipline.typeAlias); 
            var results = MF.ObjectOps.SearchObjects(vault, typeId, sConditons);

            return results;
        },
        /*获取select的选中项的Id*/
        getSelestedItemId: function(selectId) {
            try {
                var obj = document.getElementById(selectId);
                var index = obj.selectedIndex;
                var val = obj.options[index].value;
                return val;
            } catch (e) {
                return undefined;
            }
        }
    };
    u.designschedule = designschedule;
})(CC);