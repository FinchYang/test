/****************************************
 * 协同云 成员工时统计
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var projectCostReport = {

        //搜索所有项目成员
        searchPorjMmebers: function (vault) {
            var typeId = MF.alias.objectType(vault, md.contacts.typeAlias);
            var sConditons = MFiles.CreateInstance("SearchConditions");
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            condition.TypedValue.SetValue(MFDatatypeLookup,
                MF.alias.classType(vault, md.contacts.classAlias));
            sConditons.Add(-1, condition);
            return MF.ObjectOps.SearchObjects(vault, typeId, sConditons);
        },

        //成员工时
        getMembersHour: function (vault, members, startDate, endDate) {
            var reportData = [];
            for (var i = 0; i < members.length; i++) {
                var memberInfo = this._getMemberInfo(vault, members[i].ObjVer, startDate, endDate);
                reportData.push({ id: members[i].ObjVer.ID, name: members[i].Title, hours: memberInfo.hours, cost: memberInfo.cost });
            }
            return reportData;
        },

        _getMemberInfo: function (vault, memberObjVer, startDate, endDate) {
            var propIdAccount = MF.alias.propertyDef(vault, md.contacts.propDefs.PropAccount);
            var propIdDailyCost = MF.alias.propertyDef(vault, md.contacts.propDefs.PropDailyCost);
            var userId = vault.ObjectPropertyOperations.GetProperty(memberObjVer, propIdAccount).Value.GetLookupID();
            var dailyCost = 0.0;
            var tValue = vault.ObjectPropertyOperations.GetProperty(memberObjVer, propIdDailyCost).Value;
            if (tValue.IsNULL() == false) {
                dailyCost = tValue.Value;
            }
            var data = this._getUserLogs(vault, userId, startDate, endDate);
            return { id: memberObjVer.ID, cost: dailyCost, hours: data.hours };
        },

        _getUserLogs: function (vault, userId, startDate, endDate) {
            var typeId = MF.alias.objectType(vault, md.jobLog.typeAlias);
            var propIdHour = MF.alias.propertyDef(vault, md.jobLog.propDefs.JobTime);

            var sConditons = MFiles.CreateInstance("SearchConditions");
            var conditon1 = MFiles.CreateInstance("SearchCondition");
            conditon1.ConditionType = MFConditionTypeEqual;
            conditon1.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefCreatedBy;
            conditon1.TypedValue.SetValue(MFDatatypeLookup, userId);
            sConditons.Add(-1, conditon1);

            var jobDateId = MF.alias.propertyDef(vault, md.jobLog.propDefs.JobDate);
            if (startDate) {
                startDate = this._getMfDate(startDate);
                var conditon2 = MFiles.CreateInstance("SearchCondition");
                conditon2.ConditionType = MFConditionTypeGreaterThanOrEqual;
                conditon2.Expression.DataPropertyValuePropertyDef = jobDateId;
                conditon2.TypedValue.SetValue(MFDatatypeDate, startDate);
                sConditons.Add(-1, conditon2);
            }

            if (endDate) {
                endDate = this._getMfDate(endDate);
                var conditon3 = MFiles.CreateInstance("SearchCondition");
                conditon3.ConditionType = MFConditionTypeLessThanOrEqual;
                conditon3.Expression.DataPropertyValuePropertyDef = jobDateId;
                conditon3.TypedValue.SetValue(MFDatatypeDate, endDate);
                sConditons.Add(-1, conditon3);
            }

            var objvns = MF.ObjectOps.SearchObjects(vault, typeId, sConditons);
            var hours = 0.0;
            for (var i = 1; i <= objvns.Count; i++) {
                var tvalue = vault.ObjectPropertyOperations.GetProperty(objvns.Item(i).ObjVer, propIdHour).Value;
                if (tvalue.DisplayValue) {
                    hours += tvalue.Value;
                }
            }

            return { userId: userId, hours: hours };
        },

        //将js日期转换成Mfiles日期
        _getMfDate: function (jsDate) {
            var ts = MFiles.CreateInstance('Timestamp');
            //ts.SetValue(jsDate);
            ts.Year = jsDate.getFullYear();
            ts.Month = jsDate.getMonth() + 1;
            ts.Day = jsDate.getDate();
            return ts.GetValue();
        },

        //计算百分比
        getPercent: function (num, total) {
            num = parseFloat(num);
            total = parseFloat(total);
            if (isNaN(num) || isNaN(total)) {
                return "-";
            }
            return total <= 0 ? "0%" : (Math.round(num / total * 10000) / 100.00 + "%");
        },

        //获取名称和工时
        getNameAndHours: function (memberHours) {
            var labels = [], hoursData = [], rate = [];
            for (var j = 0; j < memberHours.length; j++) {
                labels.push(memberHours[j].name);
                hoursData.push(memberHours[j].hours);
                rate.push(memberHours[j].cost);
            }
            return [labels, hoursData, rate];
        }
    };

    u.projectCostReport = projectCostReport;
})(CC);