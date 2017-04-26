/****************************************
 * 协同云 项目状态统计
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var taskStatusReport = {

        //作用：搜索所有任务
        //参数：vault
        searchAllTasks: function  (vault) {
            var sConditons = MFiles.CreateInstance("SearchConditions");
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition.TypedValue.SetValue(MFDatatypeLookup, -100);
            sConditons.Add(-1, condition);
            return  MF.ObjectOps.SearchObjects(vault, MFBuiltInObjectTypeAssignment, sConditons);
        },

        //作用：判断任务是否完成
        //参数：vault；任务对象版本
        isTaskCompleted: function (vault, objVer) {
            var props = vault.ObjectPropertyOperations.GetProperties(objVer, false);
            var pIdAssignTop = MF.alias.propertyDef(vault, md.genericTask.propDefs.AssignedTo);

            //指派给
            var assignToLen = 0;
            var assignToTvalue = props.SearchForProperty(pIdAssignTop).Value;
            if (assignToTvalue.IsNULL() == false) {
                var ups1 = assignToTvalue.GetValueAsLookups();
                assignToLen = ups1.Count;
            }

            if (assignToLen !== 0 ) {
                return false;
            }
            return true;
        },

        //作用：根据时间过滤任务
        //参数：vault；任务对象列表；开始日期；结束日期
        filterTaskFromDate: function (vault, tasks, startDate, endDate) {
            var taskData = [];
            for (var i = 1; i <= tasks.Count; i++) {
                var task = tasks.Item(i);
                var props = vault.ObjectPropertyOperations.GetProperties(task.ObjVer, false);
                var starId = MF.alias.propertyDef(vault, md.genericTask.propDefs.StartDate);
                var start = props.SearchForProperty(starId).Value.DisplayValue;
                var endId = MF.alias.propertyDef(vault, md.genericTask.propDefs.Deadline);
                var end = props.SearchForProperty(endId).Value.DisplayValue;

                if (this._compareDate(startDate, start) && this._compareDate(end, endDate)) {
                    taskData.push(task);
                }
            }
            return taskData;
        },

        //作用：时间比较tagDate >= srcDate : true
        //参数：开始时间；结束时间
        _compareDate: function (srcDate, tagDate) {
            if (srcDate == null || tagDate == null) {
                return false;
            }

            srcDate = srcDate.replace(/\-/g, "/");
            tagDate = tagDate.replace(/\-/g, "/");

            var startDate = new Date(srcDate);
            var endDate = new Date(tagDate);

            if (startDate > endDate) {
                return false;
            } else {
                return true;
            }
        }
    };

    u.taskStatusReport = taskStatusReport;
})(CC);