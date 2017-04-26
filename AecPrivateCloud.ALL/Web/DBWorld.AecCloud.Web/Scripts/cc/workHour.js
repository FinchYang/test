/*工时管理-报表统计*/
var CC = CC || {};
(function(u, undefined) {
    var workHour = {
        baseUrl: (function() {
            //return "http://localhost:45725";
            return "";
        })(),
        initDate: function(defaultS, defaultE) {
            //初始化日期
            $("#startDate").datepicker({
                changeMonth: true,
                changeYear: true,
                showMonthAfterYear: true,
                dateFormat: "yy/mm/dd",
                defaultDate: defaultS,//默认开始日期
                onSelect: function (dateText, inst) {
                    $("#endDate").datepicker("option", "minDate", dateText);
                }
            });

            $("#endDate").datepicker({
                changeMonth: true,
                changeYear: true,
                showMonthAfterYear: true,
                dateFormat: "yy/mm/dd",
                defaultDate: defaultE,//默认结束日期
                onSelect: function (dateText, inst) {
                    $("#startDate").datepicker("option", "maxDate", dateText);
                }
            });

            $("#startDate").removeAttr("disabled");
            $("#endDate").removeAttr("disabled");
        },
        /**
         * 初始化下拉选择框数据
         */
        initDropdownCheckBox: function (dropData) {
            var dropDown = $('.dropdown-checkbox-project').dropdownCheckbox({
                data: dropData,
                autosearch: true,
                title: "选择项目",
                hideHeader: false,
                showNbSelected: true,
                templateButton: '<a class="dropdown-checkbox-toggle" data-toggle="dropdown" href="#">选择项目<span class="dropdown-checkbox-nbselected"></span><b class="caret"></b>'
            });
            return dropDown;
        },
        projStatus: function() {
            var res = [];
            var url = this.baseUrl+"/WorkingHour/ProjStatus";
            //$.support.cors = true;
            $.ajax({
                type: "GET",
                url: url,
                async: false,
                success: function (data, textStatus) {
                    if (data) {
                        for (var i = 0; i < data.length; i++) {
                            res.push({
                                "id": data[i].Id,
                                "label": data[i].Name
                            });
                        }
                    }
                },
                error: function (xmlHttpRequest, textStatus, errorThrown) {
                    alert("获取项目状态失败: " + textStatus + " # Error: " + errorThrown);
                }
            });
            return res;
        },
        projects: function() {
            var res = [];
            var url = this.baseUrl + "/WorkingHour/Projects";
            //$.support.cors = true;
            $.ajax({
                type: "GET",
                url: url,
                async: false,
                success: function (data, textStatus) {
                    if (data) {
                        //alert(JSON.stringify(data));
                        for (var i = 0; i < data.length; i++) {
                            res.push({
                                "id": data[i].Guid,
                                "label": data[i].Name,
                                "isChecked": false
                            });
                        }
                    }
                },
                error: function (xmlHttpRequest, textStatus, errorThrown) {
                    alert("获取项目列表失败: " + textStatus + " # Error: " + errorThrown);
                }
            });
            return res;
        },
        getReportData: function (vaultGuids, showType, beginDate, endDate) {
            var that = this;
            //var res;
            var url = this.baseUrl + "/WorkingHour/ReportData";
            url += "?vaultGuids=" + encodeURIComponent(JSON.stringify(vaultGuids))
                + "&showType=" + encodeURIComponent(showType)
                + "&beginDate=" + encodeURIComponent(beginDate)
                + "&endDate=" + encodeURIComponent(endDate);
            //$.support.cors = true;
            $.ajax({
                type: "GET",
                url: url,
                async: true,
                success: function (data, textStatus) {
                    //alert(JSON.stringify(data));
                    if (data) {
                        //res = data;
                        that.updateTable(data);
                    }
                },
                error: function (xmlHttpRequest, textStatus, errorThrown) {
                    alert("获取统计数据失败: " + textStatus + " # Error: " + errorThrown);
                }
            });
           // return res;
        },
        
        updateTable: function (srcData) {
            if (!srcData || srcData.length === 0) return;
            this.addTableHead(srcData[0].TimeSpans);
            var totalHours = this.addTableBody(srcData);
            this.addTableFoot(srcData[0].TimeSpans.length, totalHours.budgetTotal, totalHours.actualTotal);
        },
        addTableHead: function (timeSpanArr) {
            var row = "<tr><th scope='col'>项目</th><th>姓名</th><th>工时类型</th>";
            for (var i = 0; i < timeSpanArr.length; i++) {
                row += "<th>" + timeSpanArr[i] + "</th>";
            }
            row += "<th>工时小计</th><th>已用预计工时百分比（%）</th></tr>";
            $('#tableHour>thead').append(row);
        },
        addTableFoot: function (tSpanLength, budgetTotal, actualTotal) {
            var percent = this.getPercent(actualTotal,budgetTotal);
            var row = "<tr><th scope='row' rowspan='2'>总计</th><td colspan='" + (tSpanLength + 2) + "'>项目预算总工时</td>";
            row += "<td>" + budgetTotal + "</td><td rowspan='2'>" + percent + "</td></tr>";
            row += "<tr><td colspan='" + (tSpanLength + 2) + "'>实际总工时</td><td>" + actualTotal + "</td></tr>";
            $('#tableHour>tfoot').append(row);
        },
        addTableBody: function (projHourArr) {
            var projTotal= {
                budgetTotal: 0.0,
                actualTotal: 0.0
            }
            if (!projHourArr || projHourArr.length === 0) return projTotal;
            for (var i = 0; i < projHourArr.length; i++) {
                var proj = projHourArr[i];
                projTotal.budgetTotal += proj.BudgetHours;
                projTotal.actualTotal += proj.ActualHours;

                var uLen = proj.UserList.length;
                var pRow = "";
                for (var j = 0; j < proj.UserList.length; j++) {
                    var user = proj.UserList[j];
                    pRow += "<tr>";
                    if (j === 0) {
                        pRow += "<th scope='row' rowspan='" + uLen * 2 + "'>" + proj.ProjName + "</th>";
                    }
                    //1 row
                    pRow += "<td rowspan='2'>" + user.UserName + "</td><td>预算</td>";
                    for (var k = 0; k < user.BudgetHours.length; k++) {
                        var bHour = user.BudgetHours[k];
                        pRow += "<td>" + bHour.Hours + "</td>";
                    }
                    pRow += "<td>" + user.BudgetTotal + "</td>";
                    pRow += "<td rowspan='2'>" + this.getPercent(user.ActualTotal, user.BudgetTotal) + "</td>";
                    pRow += "</tr>";
                    //2 row
                    pRow += "<tr><td>实际</td>";
                    for (k = 0; k < user.ActualHours.length; k++) {
                        var aHour = user.ActualHours[k];
                        pRow += "<td>" + aHour.Hours + "</td>";
                    }
                    pRow += "<td>" + user.ActualTotal + "</td>";
                    pRow += "</tr>";
                }
                if (pRow) {
                    $('#tableHour>tbody').append(pRow);
                }
            }
            return projTotal;
        },
        emptyTable: function () {
            var foots = $("#tableHour>tfoot").children();
            for (var i = 0; i < foots.length; i++) {
                foots.eq(i).remove();
            }
            var rows = $("#tableHour>tbody").children();
            for (i = 0; i < rows.length; i++) {
                rows.eq(i).remove();
            }
            var heads = $("#tableHour>thead").children();
            for (i = 0; i < heads.length; i++) {
                heads.eq(i).remove();
            }
        },
        addOption2Select: function (selectId, data, prefix) {
            for (var i = 0; i < data.length; i++) {
                var op = "<option id='" + prefix + data[i].id.toString()
                    + "' value='" + data[i].id.toString() + "'>"
                    + data[i].label + "</option>";
                $('#' + selectId).append(op);
            }
        },

        getTableText: function (tableId) {
            var csv = [];          
            var oTable = document.getElementById(tableId);
            for (var i = 0; i < oTable.tBodies[0].rows.length; i++) {
                var column = oTable.tBodies[0].rows[i].cells;
                var rows = [];
                for (var c = 0; c < column.length; c++) {
                    rows.push(column[c].innerText);
                }
                csv.push(rows);
            };
            //alert(JSON.stringify(csv));
            return csv;
        },
        setFlie: function (csv) {
            var shell = new ActiveXObject('WScript.Shell');
            var myDocFolder = shell.SpecialFolders.Item("Desktop");//桌面:Desktop 我的文档:MyDocuments
            var myDate = new Date();
            var fileName = myDate.getTime() + ".csv";
            var flieObj = myDocFolder + "\\" + fileName;

            var objFso = new ActiveXObject("Scripting.FileSystemObject");
            if (!objFso.FileExists(flieObj)) {
                var objStream = objFso.CreateTextFile(flieObj, true);
                for (var i = 0; i < csv.length; i++) {
                    objStream.WriteLine(csv[i]);
                }
                objStream.Close();
                this.addTip("文件已创建到桌面！" + "\r" + "[创建文件路径为: " + flieObj + "]" + "\r" + '[' + "文件名为:" + fileName + ']');
            }
            else {
                this.addTip("文件: " + flieObj + "已经存在");
            }
        },
        addTip: function (tip) {
            $('#tip').text(tip);
        },

        //转换百分比 a/b
        getPercent: function(a, b) {
            if (!b) return "";
            return this.converter((a / (b + 0.0) * 100),2);
        },
        ///floatNum:浮点数值，dotNum:小数点位数
        converter: function (floatNum, dotNum) {
            return floatNum ? new Number(floatNum.toFixed(dotNum)) : 0;
        },
        //判断日期的有效性
        //参数：日期字符串(年/月/日)；日期分隔符
        isValidDate: function (date, flag) {
            var bits = date.split(flag);
            var d = new Date(bits[0], bits[1] - 1, bits[2]);
            return d && (d.getMonth() + 1) == bits[1] && d.getDate() == Number(bits[2]);
        }
    }
    u.workHour = workHour;
})(CC);