/******
*任务工时统计相关操作
******/
var taskTimeReportOps = {
    //更新项目报表对象
    updateReport: function (vault, objId, newReportData) {
        //newPropsData:{ 'startDate', 'deadline''checkedItems':[task], 'reportInfo':string}
        reportOps.updateReport(vault, md.taskTimeReport.classAlias, objId, newReportData);
    },
     
    _getMfDate: function(jsDate) {
        var ts = MFiles.CreateInstance('Timestamp');
        //ts.SetValue(jsDate);
        ts.Year = jsDate.getFullYear();
        ts.Month = jsDate.getMonth() + 1;
        ts.Day = jsDate.getDate();
        return ts.GetValue();
    },
    //读取历史数据
    getOldReportData: function (vault, props) {
        ////返回值：{ 'startDate': string, 'deadline': string, 'checkedItems': [{ "id": int, "label": string,"isChecked": bool }], 'reportInfo': string }
        return reportOps.getHistoryReportData(vault, md.taskTimeReport.classAlias, props);
    },

    //筛选任务list
    getTasksOnCondition: function (vault, beginDate, endDate) {
        
        var tasks = this.getAllTasks(vault);
        var list = [];
        for (var j = 0; j < tasks.length; j++) {
            var item = tasks[j];
            if (!beginDate && !endDate) {
                list.push(item);
            }
            if (!beginDate && endDate) {
                var endDateStr = item.EndDateString;
                if (endDateStr) {
                    var eTime = new Date(endDateStr.replace(/\-/g, "/"));
                    if (endDate >= eTime) {
                        list.push(item);
                    }
                }
            }
            if (beginDate && !endDate) {
                var beginDateStr = item.BeginDateString;
                if (beginDateStr) {
                    var bTime = new Date(beginDateStr.replace(/\-/g, "/"));
                    if (beginDate <= bTime) {
                        list.push(item);
                    }
                }
            }
            if (beginDate && endDate) {
                var flag = false;
                endDateStr = item.EndDateString;
                beginDateStr = item.BeginDateString;
                if (endDateStr && beginDateStr) {
                    eTime = new Date(endDateStr.replace(/\-/g, "/"));
                    bTime = new Date(beginDateStr.replace(/\-/g, "/"));
                    if (endDate >= eTime && beginDate <= bTime) {
                        list.push(item);
                    }
                }
            }
        }       
        return list;
    },
    //搜索所有任务
    getAllTasks: function (vault) {
        this.allTasks = this.allTasks || [];
        if (this.allTasks.length === 0) {
            var propIdBDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.StartDate);
            var propIdEDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.Deadline);

            var sConditons = MFiles.CreateInstance("SearchConditions");
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition.TypedValue.SetValue(MFDatatypeLookup, -100);
            sConditons.Add(-1, condition);
            var objvns = MF.ObjectOps.SearchObjects(vault, MFBuiltInObjectTypeAssignment, sConditons);

            for (var i = 1; i <= objvns.Count; i++) {
                var item = objvns.Item(i);
                var beginDateString = vault.ObjectPropertyOperations.GetProperty(item.ObjVer, propIdBDate).Value.DisplayValue;
                var endDateString = vault.ObjectPropertyOperations.GetProperty(item.ObjVer, propIdEDate).Value.DisplayValue;
                this.allTasks.push({ 'ID': item.ObjVer.ID, 'Title': item.Title, 'BeginDateString': beginDateString, 'EndDateString': endDateString });
            }
        }
        return this.allTasks;
    },
    //转为checklist
    tasks2CheckList:function(tasks) {
        var checklist = [];
        for (var i = 0; i < tasks.length; i++) {
            checklist.push({ 'id': tasks[i].ID, 'label': tasks[i].Title, 'isChecked': false });
        }
        return checklist;
    },
    getTasksOnConditionEx: function (vault, beginDate, endDate) {
        var sConditons = MFiles.CreateInstance("SearchConditions");
        var condition = MFiles.CreateInstance("SearchCondition");
        condition.ConditionType = MFConditionTypeEqual;
        condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        condition.TypedValue.SetValue(MFDatatypeLookup, -100);
        sConditons.Add(-1, condition);
        if (beginDate) {
            beginDate = this._getMfDate(beginDate);
            var propIdBDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.StartDate);
            var scBDate = MFiles.CreateInstance("SearchCondition");
            scBDate.ConditionType = MFConditionTypeGreaterThanOrEqual;
            scBDate.Expression.DataPropertyValuePropertyDef = propIdBDate;
            scBDate.TypedValue.SetValue(MFDatatypeDate, beginDate);
            sConditons.Add(-1, scBDate);
        }
        if (endDate) {
            endDate = this._getMfDate(endDate);
            var propIdEDate = MF.alias.propertyDef(vault, md.genericTask.propDefs.Deadline);
            var scEDate = MFiles.CreateInstance("SearchCondition");
            scEDate.ConditionType = MFConditionTypeLessThanOrEqual;
            scEDate.Expression.DataPropertyValuePropertyDef = propIdEDate;
            scEDate.TypedValue.SetValue(MFDatatypeDate, endDate);
            sConditons.Add(-1, scEDate);
        }
        var objvns = MF.ObjectOps.SearchObjects(vault, MFBuiltInObjectTypeAssignment, sConditons);
        var tasks = [];
        for (var i = 1; i <= objvns.Count; i++) {
            var item = objvns.Item(i);
            tasks.push({ 'id': item.ObjVer.ID, 'label': item.Title, 'isChecked': false });
        }
        return tasks;
    },
    
    //获取工时统计信息
    getJobTimeReportData: function (vault, tasks) {
        //tasks:[]
        var statistics = [];// { id:1, man: "刘建立", finished: 30, unfinished: 20, total: 50 },
        for (var i = 0; i < tasks.length; i++) {
            var taskInfo = this.getTaskJobTime(vault, tasks[i]);
            if (taskInfo) {
                var finishedMen = taskInfo.finishedMen;
                var unfinishedMen = taskInfo.unfinishedMen;
                for (var j = 0; j < finishedMen.length; j++) {
                    this._add2Statistics(statistics, finishedMen[j], true);
                }
                for (var k = 0; k < unfinishedMen.length; k++) {
                    this._add2Statistics(statistics, unfinishedMen[k], false);
                }
            }
        }
        var totalMan = {'id':0, 'man':'总计', 'finished':0,'unfinished':0,'total':0}
        for (var l = 0; l < statistics.length; l++) {
            var item = statistics[l];
            totalMan.finished += item.finished;
            totalMan.unfinished += item.unfinished;
            totalMan.total += item.total;
        }
        statistics.push(totalMan);
        var reportData = { 'statistics': statistics };
        return reportData;
    },
    _add2Statistics: function(statistics, man, finished) {
        var flag = false;
        for (var i = 0; i < statistics.length; i++) {
            if (statistics[i].id === man.ID) {
                if (finished) {
                    statistics[i].finished += man.Hours;
                } else {
                    statistics[i].unfinished += man.Hours;
                }
                statistics[i].total += man.Hours;
                flag = true;
                break;
            }
        }
        if (!flag) {
            var hours = 0;
            var unHours = 0;
            if (finished) {
                hours = man.Hours;
            } else {
                unHours = man.Hours;
            }
            statistics.push({ 'id': man.ID, 'man': man.Name, 'finished': hours, 'unfinished': unHours, 'total': man.Hours });
        } 
    },
    //从任务中读取工时信息
    getTaskJobTime: function (vault, task) {
        //task:{id, label}
        var typeId = MF.alias.objectType(vault, md.genericTask.typeAlias);
        var pIdAssignTop = MF.alias.propertyDef(vault, md.genericTask.propDefs.AssignedTo);
        var pIdMarkedBy = MF.alias.propertyDef(vault, md.genericTask.propDefs.CompletedBy);
        var pIdTimeHour = MF.alias.propertyDef(vault, md.genericTask.propDefs.JobTime);

        var objId = task.id;
        var oObjVer = MFiles.CreateInstance('ObjVer');
        oObjVer.SetIDs(typeId, objId, -1);
        var props = vault.ObjectPropertyOperations.GetProperties(oObjVer, false);
        if (props.IndexOf(pIdTimeHour) == -1) return undefined;

        var hours = 0.0;
        var timeHourTvalue = props.SearchForProperty(pIdTimeHour).Value;
        if (timeHourTvalue.IsNULL() == false) {
            hours = timeHourTvalue.Value;
        }
        var assignTos = [];
        var assignToTvalue = props.SearchForProperty(pIdAssignTop).Value;
        if (assignToTvalue.IsNULL() == false) {
            var ups = assignToTvalue.GetValueAsLookups();
            for (var i = 1; i <= ups.Count; i++) {
                assignTos.push({ 'ID': ups.Item(i).Item, 'Name': ups.Item(i).DisplayValue ,'Hours': 0});
            }
        }
        var markedBys = [];
        var markedByTvalue = props.SearchForProperty(pIdMarkedBy).Value;
        if (markedByTvalue.IsNULL() == false) {
            var ups2 = markedByTvalue.GetValueAsLookups();
            for (var j = 1; j <= ups2.Count; j++) {
                markedBys.push({ 'ID': ups2.Item(j).Item, 'Name': ups2.Item(j).DisplayValue, 'Hours': 0 });
            }
        }
        var len = assignTos.length + markedBys.length;
        if (len == 0) return undefined;
        var hour = hours / len;
        //hour = new Number(hour.toFixed(2));
        for (var k = 0; k < assignTos.length; k++) {
            assignTos[k].Hours = hour;
        }
        for (var l = 0; l < markedBys.length; l++) {
            markedBys[l].Hours = hour;
        }
        return { 'finishedMen': markedBys, 'unfinishedMen': assignTos };
    }
    
}