/****************************************
 * 协同云 设计策划
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var designplan = {

        /*****************************************************************************
        *搜索
        *****************************************************************************/
        //func:获取所有的设计策划
        _getDesignPlanByFilterId: function (vault, phaseId, majorId) {
            var classId = MF.alias.classType(vault, md.drawingPlan.classAlias);
            var conditions = MF.createObject("SearchConditions");
            var condition1 = MF.createObject("SearchCondition");
            condition1.ConditionType = MFConditionTypeEqual;
            condition1.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition1.TypedValue.SetValue(MFDatatypeLookup, classId);
            conditions.Add(-1, condition1);

            var condition2 = MF.createObject("SearchCondition");
            condition2.ConditionType = MFConditionTypeEqual;
            condition2.Expression.DataPropertyValuePropertyDef = 
                MF.alias.propertyDef(vault, md.drawingPlan.propDefs.DesignPhase);
            condition2.TypedValue.SetValue(MFDatatypeLookup, phaseId);
            conditions.Add(-1, condition2);

            var condition3 = MF.createObject("SearchCondition");
            condition3.ConditionType = MFConditionTypeEqual;
            condition3.Expression.DataPropertyValuePropertyDef = 
                MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Discipline);
            condition3.TypedValue.SetValue(MFDatatypeLookup, majorId);
            conditions.Add(-1, condition3);

            return vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlagNone, false);
        },

        //获取策划专业组
        _getPlanMajorObjByUserId: function (vault, alias, userid) {
            var classId = MF.alias.classType(vault, md.planDiscipline.classAlias);
            var conditions = MF.createObject("SearchConditions");
            var condition1 = MF.createObject("SearchCondition");
            condition1.ConditionType = MFConditionTypeEqual;
            condition1.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition1.TypedValue.SetValue(MFDatatypeLookup, classId);
            conditions.Add(-1, condition1);

            var condition2 = MF.createObject("SearchCondition");
            condition2.ConditionType = MFConditionTypeEqual;
            condition2.Expression.DataPropertyValuePropertyDef = MF.alias.propertyDef(vault, alias);
            condition2.TypedValue.SetValue(MFDatatypeLookup, userid);
            conditions.Add(-1, condition2);

            return vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlagNone, false);
        },

       //func:获取项目策划
        _getAllProjectPlanObj: function (vault) {
            var classId = MF.alias.classType(vault, md.projectPlan.classAlias);
            var conditions = MF.createObject("SearchConditions");
            var condition1 = MF.createObject("SearchCondition");
            condition1.ConditionType = MFConditionTypeEqual;
            condition1.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition1.TypedValue.SetValue(MFDatatypeLookup, classId);
            conditions.Add(-1, condition1);

            //condition2 :未删除

            return vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlagNone, false);
        },

        //func:获取项目策划
        _getProjectPlanObjByUserId: function(vault, alias, userId) {
            var classId = MF.alias.classType(vault, md.projectPlan.classAlias);
            var conditions = MF.createObject("SearchConditions");
            var condition1 = MF.createObject("SearchCondition");
            condition1.ConditionType = MFConditionTypeEqual;
            condition1.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition1.TypedValue.SetValue(MFDatatypeLookup, classId);
            conditions.Add(-1, condition1);

            var condition2 = MF.createObject("SearchCondition");
            condition2.ConditionType = MFConditionTypeEqual;
            condition2.Expression.DataPropertyValuePropertyDef = MF.alias.propertyDef(vault, alias);
            condition2.TypedValue.SetValue(MFDatatypeLookup, userId);
            conditions.Add(-1, condition2);

            return vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlagNone, false);
        },
        /*****************************************************************************/

        /*****************************************************************************
        *获取筛选数据
        *****************************************************************************/
        //获取项目名称
        getProjectName: function(vault) {
            var title = "项目名称";
            var typeId = MF.alias.objectType(vault, md.proj.typeAlias);
            var objVns = MF.ObjectOps.SearchObjectsByType(vault, typeId);
            if (objVns.Count > 0) {
                var objVer = objVns.Item(1).ObjVer;
                var verAndPorp = vault.ObjectOperations.GetObjectVersionAndProperties(objVer, false);
                title = verAndPorp.VersionData.Title;
            }

            return title;
        },

        //获取当前设计阶段
        getCurrDesignPhase: function (vault) {
            var result = {};
            var objsVer = this._getAllProjectPlanObj(vault);
            if (objsVer.Count > 0) {
                var propDef = MF.alias.propertyDef(vault, md.projectPlan.propDefs.DesignPhase);
                result.name = vault.ObjectPropertyOperations.GetProperty(objsVer.item(1).ObjVer, propDef).Value.DisplayValue;
                result.id = this._getIdByLookupValue(vault, md.projectPlan.propDefs.DesignPhase, result.name);
            }

            return result;
        },

        //获取所有设计阶段
        getAllDesignPhase: function (vault) {
            return  this._getMultiSelectItemsByAlias(vault, md.projectPlan.propDefs.DesignPhase);
        },

        //获取当前用户所在专业组
        getCurrMajorGroup: function (vault) {
            var groupAlias = [
                md.planDiscipline.propDefs.DisciplineLead, //专业负责人
                md.drawingPlan.propDefs.DrawingPerson, //绘图人
                md.drawingPlan.propDefs.Designer, //设计人
                md.drawingPlan.propDefs.Verifier, //校对人
                md.drawingPlan.propDefs.Checker, //审核人
                md.drawingPlan.propDefs.Validator //审定人
            ];

            var result = {};
            for (var i = 0; i < groupAlias.length; i++) {
                var objsVer = this._getPlanMajorObjByUserId(vault, groupAlias[i], vault.SessionInfo.UserID);
                if (objsVer.Count > 0) {
                    result.name = objsVer.item(1).Title;
                    result.id = objsVer.item(1).ObjVer.ObjID;
                }
            }

            return result;
        },

        //获取项目策划里的所有专业
        getAllMajorGroup: function (vault) {
            var result = [];
            var objsVer = this._getAllProjectPlanObj(vault);
            if (objsVer.Count > 0) {
                var propDef = MF.alias.propertyDef(vault, md.projectPlan.propDefs.Disciplines);
                var value = vault.ObjectPropertyOperations.GetProperty(objsVer.item(1).ObjVer, propDef).Value.DisplayValue;

                if (!value) {
                    return result;
                }

                var names = value.split(';');
                var allMajorGroup = this._getMultiSelectItemsByAlias(vault, md.projectPlan.propDefs.Disciplines);
                for (var i = 0; i < names.length; i++) {
                    for (var j = 0; j < allMajorGroup.length; j++) {
                        if ($.trim(names[i]) === allMajorGroup[j].name) {
                            result.push(allMajorGroup[j]);
                        }
                    }
                }
            }

            return result;
        },
        /*****************************************************************************/


        /*****************************************************************************
        *获取表数据
        *****************************************************************************/
        //获取设计策划行数据
        getDesignPlanHeaderAndData: function (vault, allAlias, phaseId, majorId, isReadOnly) {
            var result = {};
            var header = this._getHeaderByAlias(vault, allAlias, majorId, isReadOnly);
            result.header = header.header; //表头
            result.columns = header.columns; //下拉选项
            result.type = header.type; //数据类型

            var objsVer = this._getDesignPlanByFilterId(vault, phaseId, majorId);
            if (objsVer.Count > 0) {
                var data = [];
                for (var i = 1; i <= objsVer.Count; i++) {
                    var lineData = this._getDataByAliasDef(vault, objsVer.item(i), header.def);
                    data.push(lineData);
                }
                result.data = data; //数据
            } else {
                result.data = [];
            }

            return result;
        },

        //获取表头
        _getHeaderByAlias: function (vault, allAlias, majorId, isReadOnly) {
            //获取专业组源数据
            var majorDrop = this._getPlanMajorDropdownSource(vault, majorId);
            //获取设总和项目经理数据
            var projPlan = this._getProjPlanDropdownSource(vault);

            //
            var header = [], def = [], type = [], columns = [];
            for (var j = 0; j < allAlias.length; j++) {
                var propertyDef = undefined;
                var currPropDef = MF.alias.propertyDef(vault, allAlias[j]);
                propertyDef = vault.PropertyDefOperations.GetPropertyDef(currPropDef);

                def.push(currPropDef);
                header.push(propertyDef.Name);
                type.push(propertyDef.DataType);
                columns.push(this._getDropdownSource(vault, allAlias[j], majorDrop, projPlan, isReadOnly));
            }

            return { header: header, def: def, type: type, columns: columns };
        },

        //获取数据
        _getDataByAliasDef: function (vault, objVer, allAliasDef) {
            var data = [];
            var propsDisplay = vault.ObjectPropertyOperations.GetPropertiesForDisplay(objVer.ObjVer);
            for (var j = 0; j < allAliasDef.length; j++) {
                for (var i = 1; i <= propsDisplay.Count; i++) {
                    var propDef = propsDisplay.item(i).PropertyDef;
                    var displayValue = propsDisplay.item(i).DisplayValue;
                    if (propDef === allAliasDef[j]) {
                        data.push(displayValue);
                    }
                }
            }
            data.push(objVer.ObjVer.ID);

            return data;
        },
        /*****************************************************************************/

        /*****************************************************************************
        *权限判断 项目经理：5；设总：4；专业负责人：3；专业组：2；一般成员：1
        *****************************************************************************/
        getRoleId: function (vault) {
            if (this._isProjManager(vault, vault.SessionInfo.UserID)) {
                return 5;
            } else if (this._isChiefDesigner(vault, vault.SessionInfo.UserID)) {
                return 4;
            } else if (this._isDisciplineLead(vault, vault.SessionInfo.UserID)) {
                return 3;
            } else if (this._isMajorMember(vault, vault.SessionInfo.UserID)) {
                return 2;
            } else {
                return 1;
            }
        },

        //项目经理
        _isProjManager: function(vault, userId) {
            var objsVer = this._getProjectPlanObjByUserId(vault, md.projectPlan.propDefs.ProjManager, userId);
            if (objsVer.Count > 0) {
                return true;
            } else {
                return false;
            }
        },

        //设总
        _isChiefDesigner: function (vault, userId) {
            var objsVer = this._getProjectPlanObjByUserId(vault, md.projectPlan.propDefs.ChiefDesigner, userId);
            if (objsVer.Count > 0) {
                return true;
            } else {
                return false;
            }
        },

        //专业负责人
        _isDisciplineLead: function (vault, userId) {
            var objsVer = this._getPlanMajorObjByUserId(vault, md.planDiscipline.propDefs.DisciplineLead, userId);
            if (objsVer.Count > 0) {
                return true;
            } else {
                return false;
            }
        },

        //专业组
        _isMajorMember: function (vault, userId) {
            var groupAlias = [
                md.drawingPlan.propDefs.DrawingPerson, //绘图人
                md.drawingPlan.propDefs.Designer, //设计人
                md.drawingPlan.propDefs.Verifier, //校对人
                md.drawingPlan.propDefs.Checker, //审核人
                md.drawingPlan.propDefs.Validator //审定人
            ];

            for (var i = 0; i < groupAlias.length; i++) {
                var objsVer = this._getPlanMajorObjByUserId(vault, groupAlias[i], userId);
                if (objsVer.Count > 0) {
                    return true;
                }
            }

            return false;
        },
        /*****************************************************************************/

        /*****************************************************************************
        *获取下拉项
        *****************************************************************************/
        //类型的所有选择项
        _getDropdownSource: function (vault, alias, majorDrop, projPlan, isReadOnly) {

            if (alias === md.drawingPlan.propDefs.FrameSize) { //图幅
                return {
                    editor: 'select',
                    selectOptions: this._getSinglSelectDropdownSource(vault, md.valueList.FrameSize),
                    readOnly: isReadOnly
                };
            } else if (alias === md.drawingPlan.propDefs.DrawingPerson) { //绘图人
                if (!majorDrop) { return { readOnly: isReadOnly }}
                return { editor: 'select', selectOptions: majorDrop.drawingPerson, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.Designer) { //设计人
                if (!majorDrop) { return { readOnly: isReadOnly }; }
                return { editor: 'select', selectOptions: majorDrop.designer, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.Verifier) { //校对人
                if (!majorDrop) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: majorDrop.verifier, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.Checker) { //审核人
                if (!majorDrop) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: majorDrop.checker, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.Validator) { //审定人
                if (!majorDrop) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: majorDrop.validator, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.DisciplineLead) { //专业负责人
                if (!majorDrop) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: majorDrop.disciplineLead, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.ChiefDesigner) { //设总
                if (!projPlan) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: projPlan.chiefDesigner, readOnly: isReadOnly };
            } else if (alias === md.drawingPlan.propDefs.PlanProgress) { //进度
                return  {
                    editor: 'select', 
                    selectOptions: this._getSinglSelectDropdownSource(vault, md.valueList.ProgressStatus),
                    readOnly: isReadOnly
                };
            } else if (alias === md.drawingPlan.propDefs.Deadline) { //截止日期
                var myDate = new Date();
                var currDate = myDate.toLocaleDateString();
                return {
                    type: 'date',
                    dateFormat: 'YYYY/MM/DD',
                    correctFormat: true,
                    defaultDate: currDate,
                    readOnly: isReadOnly
                };
            } else if (alias === md.drawingPlan.propDefs.ProjManager) { //项目经理
                if (!projPlan) { return { readOnly: isReadOnly } }
                return { editor: 'select', selectOptions: projPlan.projManager, readOnly: isReadOnly };
            } else {
                return { readOnly: isReadOnly };
            }
        },

        //获取(单选)类型的所有选择项
        _getSinglSelectDropdownSource: function(vault, alias) {
            var id = vault.ValueListOperations.GetValueListIDByAlias(alias);
            if (id !== -1) {
                var source = [];
                var items = vault.ValueListItemOperations.GetValueListItems(id);
                for (var i = 1; i <= items.Count; i++) {
                    source.push(items.item(i).Name);
                }
                return source;
            }

            return null;
        },

        //获取图专业组的所有选择项
        _getPlanMajorDropdownSource: function (vault, majorId) {
            var objVer = this._getObjVerById(vault, MF.alias.objectType(vault, md.planDiscipline.typeAlias), majorId);
            //绘图人
            var propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.DrawingPerson);
            var propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var drawingPerson = this._getCombinationItems(propValue);
            //设计人
            propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.Designer);
            propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var designer = this._getCombinationItems(propValue);
            //校对人
            propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.Verifier);
            propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var verifier = this._getCombinationItems(propValue);
            //审核人
            propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.Checker);
            propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var checker = this._getCombinationItems(propValue);
            //审定人
            propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.Validator);
            propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var validator = this._getCombinationItems(propValue);
            //专业负责人
            propDef = MF.alias.propertyDef(vault, md.planDiscipline.propDefs.DisciplineLead);
            propValue = vault.ObjectPropertyOperations.GetProperty(objVer, propDef).Value.DisplayValue;
            var disciplineLead = this._getCombinationItems(propValue);

            return { drawingPerson: drawingPerson, designer: designer, verifier: verifier, checker: checker, validator: validator, disciplineLead: disciplineLead };
        },

        //获取设总和项目经理所有选项
        _getProjPlanDropdownSource: function (vault) {
            var objsVer = this._getAllProjectPlanObj(vault);
            if (objsVer.Count > 0) {
                var objVer = objsVer.item(1);

                //设总
                var propDef = MF.alias.propertyDef(vault, md.projectPlan.propDefs.ChiefDesigner);
                var propValue = vault.ObjectPropertyOperations.GetProperty(objVer.ObjVer, propDef).Value.DisplayValue;
                var chiefDesigner = this._getCombinationItems(propValue);

                //项目经理
                propDef = MF.alias.propertyDef(vault, md.projectPlan.propDefs.ProjManager);
                propValue = vault.ObjectPropertyOperations.GetProperty(objVer.ObjVer, propDef).Value.DisplayValue;
                var projManager = [];
                projManager.push(propValue);

                return { chiefDesigner: chiefDesigner, projManager: projManager };
            }

            return null;
        },
        /*****************************************************************************/

        /*****************************************************************************
        *更新，插入，删除
       *****************************************************************************/
        //更新属性
        UpdatePropperties: function (vault, result, allAlias, changes) {
            for (var i = 0; i < changes.length; i++) {
                var row = changes[i][0];
                var col = changes[i][1];
                var oldValue = changes[i][2];
                var newValue = changes[i][3];

                if (oldValue === newValue) {
                    return;
                }

                var colAlias = allAlias[col];
                var type = result.type[col];
                var rowData = result.data[row];

                var dataObjVer = undefined;
                if (rowData[allAlias.length] !== undefined) {
                    dataObjVer = this._getObjVerById(vault,
                        MF.alias.objectType(vault, md.drawingPlan.typeAlias),
                        rowData[allAlias.length]);
                }

                if (!dataObjVer) {
                    return;
                }

                //更新对象
                var verAndPorp = vault.ObjectOperations.GetObjectVersionAndProperties(dataObjVer, false);

                //签出对象
                var checkOutVn;
                if (!verAndPorp.VersionData.ObjectCheckedOut) {
                    checkOutVn = vault.ObjectOperations.CheckOut(verAndPorp.ObjVer.ObjID).ObjVer;
                } else {
                    checkOutVn = verAndPorp.VersionData.ObjVer;
                }

                //修改属性
                var properties = MF.createObject("PropertyValues");
                var property = MF.createObject("PropertyValue");
                property.PropertyDef = MF.alias.propertyDef(vault, colAlias);
                if (type === MFDatatypeLookup) {
                    var lookup = this._getIdByLookupValue(vault, colAlias, newValue);
                    property.TypedValue.SetValue(type, lookup);
                } else if (type === MFDatatypeMultiSelectLookup) {
                    var lookups = this._getMultiSelectLookupValue(vault, colAlias, newValue);
                    property.TypedValue.SetValue(type, lookups);
                } else {
                    property.TypedValue.SetValue(type, newValue);
                }

                properties.Add(-1, property);
                checkOutVn = vault.ObjectPropertyOperations.SetProperties(checkOutVn, properties).ObjVer;

                //签入对象
                var objVersion = vault.ObjectOperations.CheckIn(checkOutVn);

                //更新任务
                this._updateDesignPlanTask(vault, objVersion);
            }
        },

        //删除和新建
        SaveData: function (vault, result, allAlias, phaseId, majorId) {
            var tableData = result.data;
            var objsVer = this._getDesignPlanByFilterId(vault, phaseId, majorId);

            var existObjId = [];
            for (var i = 0; i < tableData.length; i++) {
                var rowData = tableData[i];
                if (rowData[allAlias.length] !== undefined) {
                    existObjId.push(rowData[allAlias.length]);
                }
            }

            //删除对象
            for (var j = 1; j <= objsVer.Count; j++) {
                var isExist = false;
                for (var k = 0; k < existObjId.length; k++) {
                    if (objsVer.item(j).ObjVer.ID === existObjId[k]) {
                        isExist = true;
                    }
                }

                if (isExist === false) {
                    vault.ObjectOperations.DeleteObject(objsVer.item(j).ObjVer.ObjID);
                }
            }

            //数据验证
            for (var n = 0; n < tableData.length; n++) {
                var line = n + 1;
                var lineData = tableData[n];
                if (!$.trim(lineData[0])) {
                    if (n !== (tableData.length - 1)) {
                        alert("第" + line + "行，图名不能为空！");
                        return false;
                    } 
                }

                if (!$.trim(lineData[1])) {
                    if (n !== (tableData.length - 1)) {
                        alert("第" + line + "行，图号不能为空！");
                        return false;
                    }
                }
            }

            //创建对象
            for (var m = 0; m < tableData.length; m++) {
                var newData = tableData[m];

                if (!$.trim(newData[0]) || !$.trim(newData[1])) {
                    //图名、图号必填
                    continue;
                }

                if (newData[allAlias.length] === undefined) {
                    //创建策划对象
                    var properties = this._createProperties(vault, result.type, newData, allAlias, phaseId, majorId);
                    var verAndProp = vault.ObjectOperations.CreateNewObject(
                        MF.alias.objectType(vault, md.drawingPlan.typeAlias),
                        properties,
                        MF.createObject('SourceObjectFiles'),
                        MF.createObject('AccessControlList'));
                    vault.ObjectOperations.CheckIn(verAndProp.ObjVer);
                    newData[allAlias.length] = verAndProp.ObjVer.ID;
                    //创建任务
                    this._createDesignPlanTask(vault, verAndProp);
                }
            }

            return true;
        },
        /*****************************************************************************/

        /*****************************************************************************
        *新建，更新任务
        *****************************************************************************/
        //新建任务
        _createDesignPlanTask: function (vault, verAndProp) {
            var propValues = {};
            propValues.title = verAndProp.VersionData.Title;
            propValues.description = verAndProp.VersionData.Title;
            propValues.userIds = this._getUsersForDesignPlan(vault, verAndProp.Properties);
            propValues.designplan = verAndProp.ObjVer.ObjID.ID;
            var deadline =  this._getValueByAlias(vault, verAndProp.Properties, md.drawingPlan.propDefs.Deadline)
            propValues.deadline = new Date(deadline);
            propValues.jobTime = this._getValueByAlias(vault, verAndProp.Properties, md.drawingPlan.propDefs.JobTime);
            CC.taskOps.newGeneralTask(vault, propValues);
        },

        //更新任务
        _updateDesignPlanTask: function (vault, objVersion) {
            var properties = vault.ObjectPropertyOperations.GetProperties(objVersion.ObjVer);
            var propValues = {};
            propValues.title = objVersion.Title;
            propValues.description = objVersion.Title;
            propValues.userIds = this._getUsersForDesignPlan(vault, properties);
            var deadline = this._getValueByAlias(vault, properties, md.drawingPlan.propDefs.Deadline);
            propValues.deadline = new Date(deadline);
            propValues.jobTime = this._getValueByAlias(vault, properties, md.drawingPlan.propDefs.JobTime);
            var taskObjsVer = CC.taskOps.searchDesignPlanTasks(vault, objVersion.ObjVer.ObjID.ID);
            if (taskObjsVer.Count > 0) {
                CC.taskOps.updateGeneralTask(vault, taskObjsVer.item(1), propValues);
            }
        },

        //获取设计策划里所有成员
        _getUsersForDesignPlan:function(vault, props) {
            var alias = [
                md.drawingPlan.propDefs.DrawingPerson, //绘图人
                md.drawingPlan.propDefs.Designer //设计人
                //md.drawingPlan.propDefs.Verifier, //校对人
                //md.drawingPlan.propDefs.Checker, //审核人
                //md.drawingPlan.propDefs.Validator, //审定人
                //md.drawingPlan.propDefs.DisciplineLead, //专业负责人
                //md.drawingPlan.propDefs.ChiefDesigner, //设总
                //md.drawingPlan.propDefs.ProjManager //项目经理
            ];

            var userIds = [];
            for (var i = 0; i < alias.length; i++) {
                var propId = MF.alias.propertyDef(vault, alias[i]);
                var value = props.SearchForProperty(propId).Value;
                if (value.DataType === MFDatatypeMultiSelectLookup) {
                    //多选
                    var lookups = value.GetValueAsLookups();
                    for (var j = 1; j <= lookups.Count; j++) {
                        var itemId = lookups.Item(j).Item;
                        userIds.push(itemId);
                    }
                } else {
                    //单选
                    var lookup = value.GetValueAsLookup();
                    var id = lookup.Item;
                    userIds.push(id);
                }
            }
            $.unique(userIds);

            return userIds;
        },
        /*****************************************************************************/

        /*****************************************************************************
        *MFiles帮助
        *****************************************************************************/
        //根据别名获取属性值
        _getValueByAlias: function(vault, props, alias) {
            var propId = MF.alias.propertyDef(vault, alias);
            return props.SearchForProperty(propId).Value.Value;
        },

        //获取单选id
        _getIdByLookupValue: function (vault, alias, value) {
            var valueListId = undefined;
            if (alias === md.drawingPlan.propDefs.FrameSize) { //图幅
                valueListId = MF.alias.valueList(vault, md.valueList.FrameSize);
                return MF.vault.getValueListItemId(vault, valueListId, value);
            } else if (alias === md.drawingPlan.propDefs.PlanProgress) { //进度
                valueListId = MF.alias.valueList(vault, md.valueList.ProgressStatus);
                return MF.vault.getValueListItemId(vault, valueListId, value);
            } else if (alias === md.projectPlan.propDefs.DesignPhase) { //设计阶段
                valueListId = MF.alias.valueList(vault, md.valueList.DesignPhase);
                return  MF.vault.getValueListItemId(vault, valueListId, value);
            } else if (alias === md.projectPlan.propDefs.ProjManager) { //项目经理
                var result = this._getMultiSelectItemsByAlias(vault, alias);
                for (var i = 0; i < result.length; i++) {
                    if (result[i].name === $.trim(value)) {
                        return result[i].id;
                    }
                }
            }

            return null;
        },

        //根据文本获取多选项id
        _getMultiSelectLookupValue: function (vault, alias, value) {
            if (!value) {
                return null;
            }
            var names = value.split(';');

            //匹配名称并获取id
            var itemsId = [];
            var result = this._getMultiSelectItemsByAlias(vault, alias);
            for (var l = 0; l < names.length; l++) {
                for (var i = 0; i < result.length; i++) {
                    if ($.trim(names[l]) === result[i].name) {
                        itemsId.push(result[i].id);
                    }
                }
            }

            var lookups = MF.createObject('Lookups');
            for (var j = 0; j < itemsId.length; j++) {
                var lu = MF.createObject('Lookup');
                lu.Item = itemsId[j];
                lookups.Add(-1, lu);
            }

            return lookups;
        },

        //获取多选列表{id, name}
        _getMultiSelectItemsByAlias: function (vault, alias) {
            var result = [];
            var propDef = MF.alias.propertyDef(vault, alias);
            var propDefObj = vault.PropertyDefOperations.GetPropertyDef(propDef);
            var isVaultList = propDefObj.BasedOnValueList;
            if (isVaultList) {
                var vaultList = propDefObj.ValueList;
                var items = vault.ValueListItemOperations.GetValueListItems(vaultList);
                for (var i = 1; i <= items.length; i++) {
                    var item = {};
                    item.name = items.item(i).Name;
                    item.id = items.item(i).ID;
                    result.push(item);
                }
            }

            return result;
        },

        //生成属性
        _createProperties: function (vault, type, data, allAlias, phaseId, majorId) {
            var propertiesValue = MF.createObject("PropertyValues");

            //类别
            var propObj1 = MF.createObject("PropertyValue");
            propObj1.PropertyDef = MFBuiltInPropertyDefClass;
            propObj1.TypedValue.SetValue(MFDatatypeLookup,
                MF.alias.classType(vault, md.drawingPlan.classAlias));
            propertiesValue.Add(-1, propObj1);

            //名称或标题
            var propObj2 = MF.createObject("PropertyValue");
            propObj2.PropertyDef = MFBuiltInPropertyDefNameOrTitle;
            propObj2.TypedValue.SetValue(MFDatatypeText, data[0] + data[1]);
            propertiesValue.Add(-1, propObj2);

            //阶段
            var propObj3 = MF.createObject("PropertyValue");
            propObj3.PropertyDef = MF.alias.propertyDef(vault, "PropDesignPhaseAuto");
            propObj3.TypedValue.SetValue(MFDatatypeLookup, phaseId);
            propertiesValue.Add(-1, propObj3);

            //专业
            var propObj4 = MF.createObject("PropertyValue");
            propObj4.PropertyDef = MF.alias.propertyDef(vault, md.drawingPlan.propDefs.Discipline);
            propObj4.TypedValue.SetValue(MFDatatypeLookup, majorId);
            propertiesValue.Add(-1, propObj4);
            
            for (var i = 0; i < allAlias.length; i++) {
                var propObj = MF.createObject("PropertyValue");
                propObj.PropertyDef = MF.alias.propertyDef(vault, allAlias[i]);
                if (type[i] === MFDatatypeLookup) {
                    propObj.TypedValue.SetValue(type[i], this._getIdByLookupValue(vault, allAlias[i], data[i]));
                } else if (type[i] === MFDatatypeMultiSelectLookup) {
                    propObj.TypedValue.SetValue(type[i], this._getMultiSelectLookupValue(vault, allAlias[i], data[i]));
                } else if (type[i] === MFDatatypeFloating) {
                    if (!data[i]) {
                        propObj.TypedValue.SetValue(type[i], data[i]);
                    } else {
                        propObj.TypedValue.SetValue(type[i], parseFloat(data[i]));
                    }
                } else if (type[i] === MFDatatypeDate) {
                    propObj.TypedValue.SetValue(type[i], data[i]);
                } else {
                    propObj.TypedValue.SetValue(type[i], data[i]);
                }
                propertiesValue.Add(-1, propObj);
            }

            return propertiesValue;
        },

        //根据类型和id获取对象
        _getObjVerById: function(vault, type, id) {
            var objectId = MF.createObject('ObjID');
            objectId.SetIDs(type, id);
            return vault.ObjectOperations.GetLatestObjVer(objectId, true, false);
        },
        /*****************************************************************************/

        /*****************************************************************************
        *公共帮助
        *****************************************************************************/
        //分割并组合字符串
        _getCombinationItems: function (value) {
            if (!value) {
                return [""];
            }

            var arr = [];
            var names = value.split(';');
            for (var p = 0; p < names.length; p++) {
                arr.push(p);
            }

            var out = [];
            for (var i = 1; i <= arr.length; i++) {
                var temp = [];
                this._combineArrayElem(arr, 0, temp, i, i, arr.length, out);
            }

            var result = [];
            for (var j = 0; j < out.length; j++) {
                var list = out[j];
                var lineItem = "";
                for (var k = 0; k < list.length; k++) {
                    var subItem = names[list[k]];
                    lineItem += subItem;
                    if (k < list.length - 1) {
                        lineItem += ';';
                    }
                }
                result.push(lineItem);
            }

            return result;
        },
        
        //组合数组元素
        _combineArrayElem: function (arr, start, result, count, num, len, out)
        {
            for (var i = start; i < len + 1 - count; i++) {
                result[count - 1] = i;
                if (count - 1 == 0) {
                    var item = [];
                    for (var j = num - 1; j >= 0; j--) {
                        //数组：一维
                        item.push(arr[result[j]]);
                    }
                    //数组：二维
                    out.push(item);
                } else {
                    this._combineArrayElem(arr, i + 1, result, count - 1, num, len, out);
                }
            }
        }
        /*****************************************************************************/
    };

    u.designplan = designplan;

})(CC);
