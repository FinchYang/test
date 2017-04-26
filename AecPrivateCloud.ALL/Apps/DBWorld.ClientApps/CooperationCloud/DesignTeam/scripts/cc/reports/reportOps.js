/***********
*报表统计功能的公用接口
*依赖文件：alias.js
************/
var reportOps = reportOps || {};
(function (o) {
    //更新项目报表对象
    o.updateReport = function(vault, classAlias, objId, newReportData) {
        //newPropsData:{ 'startDate':date, 'deadline':date, 'checkedItems':[], 'reportInfo':string}
        var typeId = MF.alias.objectType(vault, md.taskTimeReport.typeAlias);
        var propIdBDate = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.StartDate);
        var propIdEDate = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.Deadline);
        var propIdInfo = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.ReportInfo);

        var startDate = newReportData.startDate;
        var deadline = newReportData.deadline;
        var checkedItems = newReportData.checkedItems;
        var reportInfo = newReportData.reportInfo;

        var pValues = MFiles.CreateInstance('PropertyValues');
        if (startDate) {
            startDate = this._getMfDate(startDate);
            var pValueStartDate = MFiles.CreateInstance('PropertyValue');
            pValueStartDate.PropertyDef = propIdBDate;
            pValueStartDate.TypedValue.SetValue(MFDatatypeDate, startDate);
            pValues.Add(-1, pValueStartDate);
        }
        if (deadline) {
            deadline = this._getMfDate(deadline);
            var pValueDeadline = MFiles.CreateInstance('PropertyValue');
            pValueDeadline.PropertyDef = propIdEDate;
            pValueDeadline.TypedValue.SetValue(MFDatatypeDate, deadline);
            pValues.Add(-1, pValueDeadline);
        }
        var propId = this._getPropDefByClass(vault, classAlias);
        if (propId > 0 && checkedItems) {
            var pValueTasks = MFiles.CreateInstance('PropertyValue');
            pValueTasks.PropertyDef = propId;
            if (checkedItems.length > 0) {
                var arr = MFiles.CreateInstance('Lookups');
                for (var i = 0; i < checkedItems.length; i++) {
                    var item = MFiles.CreateInstance('Lookup');
                    item.Item = checkedItems[i].id;
                    item.DisplayValue = checkedItems[i].label;
                    arr.Add(-1, item);
                }
                pValueTasks.TypedValue.SetValue(MFDatatypeMultiSelectLookup, arr);
            } else {
                pValueTasks.TypedValue.SetValueToNULL(MFDatatypeMultiSelectLookup);
            }
            pValues.Add(-1, pValueTasks);
        }
        //报表信息
        var pValueInfo = MFiles.CreateInstance('PropertyValue');
        pValueInfo.PropertyDef = propIdInfo;
        pValueInfo.TypedValue.SetValue(MFDatatypeMultiLineText, reportInfo);
        pValues.Add(-1, pValueInfo);

        var oObjVer = MFiles.CreateInstance('ObjVer');
        oObjVer.SetIDs(typeId, objId, -1);
        var oObjVn = vault.ObjectOperations.GetObjectInfo(oObjVer, true, true);
        var checkOutVn;
        if (oObjVn.ObjectCheckedOut) {
            if (oObjVn.CheckedOutTo === vault.SessionInfo.UserID || oObjVn.CheckedOutTo === -103) {
                checkOutVn = oObjVn;
            } else { //
                var oldObjVer;
                try {
                    oldObjVer = vault.ObjectOperations.ForceUndoCheckout(oObjVn.ObjVer).ObjVer;
                } catch (e) {
                    //throw new Error("当前账户无权限撤销对象:" + oObjVn.Title + " 的签出\r\n" + e.Message);
                    return;
                }
                try {
                    checkOutVn = vault.ObjectOperations.CheckOut(oldObjVer.ObjID);
                } catch (e) {
                    //throw new Error("当前账户无权限签出对象:" + oObjVn.Title + "\r\n" + e.Message);
                    return;
                }
            }
        } else {
            checkOutVn = vault.ObjectOperations.CheckOut(oObjVer.ObjID);
        }
        vault.ObjectPropertyOperations.SetProperties(checkOutVn.ObjVer, pValues);
        try {
            vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
        } catch (e) {
            //throw new Error("签入修改对象:" + oObjVn.Title + "失败！\r\n" + e.Message);
            try {
                vault.ObjectOperations.ForceUndoCheckout(checkOutVn.ObjVer);
            } catch (e) {
                try {
                    vault.ObjectOperations.UndoCheckout(checkOutVn.ObjVer);
                } catch (e) {
                }
            }
        }
    };
    o._getMfDate = function(jsDate) {//将js日期转换成Mfiles日期
            var ts = MFiles.CreateInstance('Timestamp');
            //ts.SetValue(jsDate);
            ts.Year = jsDate.getFullYear();
            ts.Month = jsDate.getMonth() + 1;
            ts.Day = jsDate.getDate();
            return ts.GetValue();
        };
    o._getPropDefByClass = function(vault, classAlias) {
        var propIdTasks = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.Task);
        var propIdMembers = MF.alias.propertyDef(vault, md.projCostReport.propDefs.ReportUser);
        var propDefId = -1;
        if (classAlias === md.taskTimeReport.classAlias || classAlias === md.taskStatusReport.classAlias) {
            propDefId = propIdTasks;
        } else if (classAlias === md.projCostReport.classAlias || classAlias === md.hoursReport.classAlias) {
            propDefId = propIdMembers;
        }
        return propDefId;
    };
    //读取项目报表历史数据
    o.getHistoryReportData = function (vault, classAlias, properties) {
        //返回值：{ 'startDate': string, 'deadline': string, 'checkedItems': [{ "id": int, "label": string,"isChecked": bool }], 'reportInfo': string }
        var propIdBDate = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.StartDate);
        var propIdEDate = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.Deadline);
        var propIdInfo = MF.alias.propertyDef(vault, md.taskTimeReport.propDefs.ReportInfo);
        var startDate = undefined;
        var bDateTvalue = properties.SearchForProperty(propIdBDate).Value;
        if (bDateTvalue.DisplayValue) {
            startDate = bDateTvalue.DisplayValue;
        }
        var deadline = undefined;
        var eDateTvalue = properties.SearchForProperty(propIdEDate).Value;
        if (eDateTvalue.DisplayValue) {
            deadline = eDateTvalue.DisplayValue;
        }
        var checkedItems = [];
        /*
        var propId = this._getPropDefByClass(vault, classAlias);
        if (propId > 0) {
            var tvalue = properties.SearchForProperty(propId).Value;
            if (tvalue.DisplayValue) {
                var lookups = tvalue.GetValueAsLookups();
                for (var i = 1; i <= lookups.Count; i++) {
                    var item = lookups.Item(i);
                    if (item.Deleted === false) {
                        checkedItems.push({ "id": item.Item, "label": item.DisplayValue,"isChecked":true });
                    }
                }
            }
        }
        */
        var reportInfo = properties.SearchForProperty(propIdInfo).Value.DisplayValue;
        return { 'startDate': startDate, 'deadline': deadline, 'checkedItems': checkedItems, 'reportInfo': reportInfo };
    };

    //判断日期的有效性
    //参数：日期字符串(年/月/日)；日期分隔符
    o.isValidDate = function (date, flag) {
        var bits = date.split(flag);
        var d = new Date(bits[0], bits[1] - 1, bits[2]);
        return d && (d.getMonth() + 1) == bits[1] && d.getDate() == Number(bits[2]);
    };
})(reportOps);