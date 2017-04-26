/**********************
*MFiles一般任务对象操作：包括新建、更新等
*依赖文件：alias.js, property.js, objectOps.js
***********************/
var CC = CC || {};

(function(u) {
    var taskOps = {
        newGeneralTask: function (vault, propValues) {
            ///<summary>新建一般任务</summary>
            ///<param name="propValues" type="object">各属性值:{designplan:'int or Lookup',title:'string',description:'string',userIds:'int[]',deadline:'date',jobTime:'float'}</param>
            ///<returns type="ObjectVersion"></returns>
            var classId = MF.alias.classType(vault, md.genericTask.classAlias);
            var propIdTitle = MF.alias.propertyDef(vault, md.genericTask.propDefs.TaskTitle);
            var propIdStartDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.StartDate); 
            var propIdDesc = MF.alias.propertyDef(vault, md.genericTask.propDefs.Description);
            var propIdSendTo = MF.alias.propertyDef(vault, md.genericTask.propDefs.AssignedTo);
            var propIdDeadline = MF.alias.propertyDef(vault, md.genericTask.propDefs.Deadline);
            var propIdJobTime = MF.alias.propertyDef(vault, md.genericTask.propDefs.JobTime);
            var propIdPlan = MF.alias.propertyDef(vault, md.genericTask.propDefs.DrawingPlan);

            var pvs = MFiles.CreateInstance('PropertyValues');
            //名称
            pvs.Add(-1, MF.property.newTextProperty(propIdTitle, propValues.title));           
            //说明
            if (propValues.description) {
                pvs.Add(-1, MF.property.newMultiLineTextProperty(propIdDesc, propValues.description));
            }
            //指派给
            var userIds = propValues.userIds;
            if (userIds && userIds.length > 0) {
                pvs.Add(-1, MF.property.newMultiSelectLookupProperty(propIdSendTo, userIds));
            }
            //设计策划
            pvs.Add(-1, MF.property.newLookupProperty(propIdPlan, propValues.designplan));
            //开始日期
            var now = new Date();
            pvs.Add(-1, MF.property.newDateProperty(propIdStartDate, now));

            //截止日期
            if (propValues.deadline) {
                pvs.Add(-1, MF.property.newDateProperty(propIdDeadline, propValues.deadline));
            }
            //工时
            pvs.Add(-1, MF.property.newFloatProperty(propIdJobTime, propValues.jobTime));

            return MF.ObjectOps.createObject(vault, 10, classId, pvs, undefined);
        },
        updateGeneralTask: function (vault, objectVersion, updatedPropValues) {
            ///<summary>更新一般任务</summary>
            ///<param name="objectVersion" type="ObjectVersion">任务</param>
            ///<param name="updatedPropValues" type="object">各属性值:{designplan:'int or Lookup',title:'string',description:'string',userIds:'int[]',deadline:'date',jobTime:'float'}</param>
            ///<returns type="ObjectVersion">若失败， 返回null</returns>
            var propIdTitle = MF.alias.propertyDef(vault, md.genericTask.propDefs.TaskTitle);
            var propIdDesc = MF.alias.propertyDef(vault, md.genericTask.propDefs.Description);
            var propIdSendTo = MF.alias.propertyDef(vault, md.genericTask.propDefs.AssignedTo);
            var propIdDeadline = MF.alias.propertyDef(vault, md.genericTask.propDefs.Deadline);
            var propIdJobTime = MF.alias.propertyDef(vault, md.genericTask.propDefs.JobTime);

            var pvs = MFiles.CreateInstance('PropertyValues');
            if (updatedPropValues.title) {
                pvs.Add(-1, MF.property.newTextProperty(propIdTitle, updatedPropValues.title));
            }
            //说明
            if (updatedPropValues.description) {
                pvs.Add(-1, MF.property.newMultiLineTextProperty(propIdDesc, updatedPropValues.description));
            }
            //指派给
            var userIds = updatedPropValues.userIds;
            if (userIds && userIds.length > 0) {
                pvs.Add(-1, MF.property.newMultiSelectLookupProperty(propIdSendTo, userIds));
            }
            //截止日期
            if (updatedPropValues.deadline) {
                pvs.Add(-1, MF.property.newDateProperty(propIdDeadline, updatedPropValues.deadline));
            }
            //工时
            pvs.Add(-1, MF.property.newFloatProperty(propIdJobTime, updatedPropValues.jobTime));

            return MF.ObjectOps.updateObject(vault, objectVersion, pvs);
        },
        searchDesignPlanTasks: function (vault, designPlanId) {
            ///<summary>搜索一般任务</summary>
            ///<returns type="ObjectSearchResults"></returns>
            var classId = MF.alias.classType(vault, md.genericTask.classAlias);
            var propIdPlan = MF.alias.propertyDef(vault, md.genericTask.propDefs.DrawingPlan);

            var sConditions = MFiles.CreateInstance("SearchConditions");
            //类别
            var conditionClass = MFiles.CreateInstance("SearchCondition");
            conditionClass.ConditionType = MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
            sConditions.Add(-1, conditionClass);
            //设计策划
            var conditionDoc = MFiles.CreateInstance("SearchCondition");
            conditionDoc.ConditionType = MFConditionTypeEqual;
            conditionDoc.Expression.DataPropertyValuePropertyDef = propIdPlan;
            conditionDoc.TypedValue.SetValue(MFDatatypeLookup, designPlanId);
            sConditions.Add(-1, conditionDoc);

            var res = MF.ObjectOps.SearchObjects(vault, 10, sConditions);
            return res;
        }
    };
    u.taskOps = taskOps;
})(CC);