/**
 * 问题反馈相关节点
 */
var modelQa = modelQa || {};
(function (u, undefined) {
    //流程的结束状态
    u.wfEndStates = (function() {
        var res = ["结束","反馈意见"];
        return res;
    })();
    u.finished = function(state) {
        var ends = this.wfEndStates;
        for (var i = 0; i < ends.length; i++) {
            if (state == ends[i]) return true;
        }
        return false;
    };
    u.initTable = function () {
        var txt = "<thead><tr><th>名称</th>" +
            "<th>流程节点</th>" +
            "<th>结论</th>" +
            "<th></th><tr></thead>";
        txt += "<tbody></tbody>";
        $('#qaTableWrap>table').append($(txt));
    }
    u.appendClass = function (classes) {
        if (!classes) return;
        var txt = "";
        for (var i = 0; i < classes.length; i++) {
            var c = classes[i];
            txt += '<option value="' + c.Class + '">' + c.Title + '</option>';
        }
        if (txt) $('#qa-class').append($(txt));
    }
    u.fillTable = function (qas) {
        $('#qaTableWrap>table>tbody').empty();
        var txt = "";
        for (var i = 0; i < qas.length; i++) {
            var qa = qas[i];
            var id = "qa_" + qa.TypeId + "_" + qa.ObjId;
            txt += '<tr id="' + id + '"><td><a>' + qa.Title + '</a></td>' +
                '<td>' + qa.WfState + '</td>' +
                '<td>' + qa.Verdict + '</td>' +
                '<td><img src="/Content/Images/model_File.jpg" title="查看文件" /></td></tr>';
        }
        $('#qaTableWrap>table>tbody').append($(txt));
    }
    u.getClasses = function (vGuid, userName) {
        var that = this;
        var res = [];
        var url = "/Model/QaClassList?guid=" + encodeURIComponent(vGuid) + "&username=" + encodeURIComponent(userName);
        $.ajax({
            type: "GET",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            success: function (data, textStatus) {
                //alert(JSON.stringify(data));
                if (data) res = that._toClassUiData(data);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                alert(" # Error: " + errorThrown);
            }
        });
        return res;
    };
    u._toClassUiData = function (src) {
        var des = [];
        for (var c in src) {
            if (src.hasOwnProperty(c)) {
                des.push({
                    "Class": c,
                    "Title": src[c]
                });
            }
        }
        return des;
    };
    u.getQaList = function (vGuid, classAlias, state) {
        var that = this;
        var res = [];
        var url = "/Model/QaList?guid=" + encodeURIComponent(vGuid) + "&classAlias=" + encodeURIComponent(classAlias) + "&state=" + encodeURIComponent(state);
        $.ajax({
            type: "GET",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false,
            success: function (data, textStatus) {
                //alert(JSON.stringify(data));
                if (data) res = that._toQaUiData(data, state);
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                alert(" # Error: " + errorThrown);
            }
        });
        return res;
    };
    u._toQaUiData = function (src, state) {
        var res = [];
        for (var i = 0; i < src.length; i++) {
            var q = {
                "Title": src[i].Title,
                "ObjId": src[i].ID,
                "TypeId": src[i].Type,
                "WfState": src[i].FlowState,
                "Verdict": !src[i].Verdict ? "--" : src[i].Verdict,
                "Url": src[i].Url,
                "ViewPort": src[i].ViewPort,
                "Components": []
            }
            var parts = src[i].Parts;
            if (parts) {
                for (var j = 0; j < parts.length; j++) {
                    var c = {
                        //"Id": parts[j].Id,
                        "IfcId": parts[j].IfcId,
                        "ModelType": !parts[j].Model ? 0 : parts[j].Model.Type,
                        "ModelID": !parts[j].Model ? 0 : parts[j].Model.ID
                    }
                    q.Components.push(c);
                }
            }
            var status = this.finished(q.WfState);
            if (state <= 1 || (state == 2 && !status) || (state == 3 && status)) {
                res.push(q);
            }
        }
        return res;
    };
    u.getQaByIdKey = function (qas, idKey) {
        var arr = idKey.split('_');
        if (arr.length < 3) return null;
        for (var i = 0; i < qas.length; i++) {
            if (qas[i].TypeId == arr[1] && qas[i].ObjId == arr[2]) {
                return qas[i];
            }
        }
        return null;
    };
    u.getTestQaList = function () {
        var res = [];
        res.push({
            "Title": "安全日常检查1",
            "ObjId": 1,
            "TypeId": 0,
            "WfState": "订制整改措施",
            "Verdict": "通过",
            "Url": "http://www.baidu.com",
            "Components": [
                {
                    //"Id": 2560,
                    "IfcId": "xxxxxxx",
                    "ModelType": 1,
                    "ModelID": 1,
                    "ViewPort":"XXXXXx"
                }
            ]
        });
        res.push({
            "Title": "安全日常检查2",
            "ObjId": 2,
            "TypeId": 0,
            "WfState": "订制整改措施",
            "Verdict": "未通过",
            "Url": "http://www.baidu.com",
            "Components": [
                {
                    //"Id": 2560,
                    "IfcId": "xxxxxxx",
                    "ModelType": 1,
                    "ModelID": 2,
                    "ViewPort": "XXXXXx"
                }
            ]
        });
        return res;
    };
    u.getTestClasses = function() {
        return [
            {
                "Title": "安全日常检查",
                "Class": "alias"
            },
            {
                "Title": "安全周检查",
                "Class": 2
            },
            {
                "Title": "安全专项检查",
                "Class": 3
            },
            {
                "Title": "质量整改",
                "Class": 4
            },
            {
                "Title": "问题反馈",
                "Class": 5
            }
        ];
    };
})(modelQa);
function modelQaEntry(nodes, vGuid, callback) {
    var guid = $('#CurrentVaultGuid').val();
    var userName = $('#AecuserIdInDatabase').val();
    var classes = modelQa.getClasses(guid, userName);
    //var classes = modelQa.getTestClasses();
    modelQa.appendClass(classes);
    modelQa.initTable();

    $('.qa-search>img').click(function () {
        var classAlias = $('#qa-class').val();
        var state = $('#qa-state').val();
        var qas = modelQa.getQaList(guid, classAlias, state);
        //var qas = modelQa.getTestQaList();
        modelQa.fillTable(qas);

        $('#qaTableWrap tr>td').click(function (event) {
            if ($(event.target).is('a')) {
                var mid = $(this).parents('tr:first').attr('id');
                var qa = modelQa.getQaByIdKey(qas, mid);
                if (typeof callback === 'function') {
                    callback(nodes, vGuid, qa);
                }
                var domAs = $("#qaTableWrap tr>td>a");
                $.each(domAs, function(k, v) {
                    $(v).css("color", "#428bca");
                });
                $(event.target).css("color", "rgb(179, 20, 120)");
            }
            if ($(event.target).is('img')) {
                mid = $(this).parents('tr:first').attr('id');
                qa = modelQa.getQaByIdKey(qas, mid);
                if (!qa) return;
                window.location.href = qa.Url;
                //window.open("http://www.baidu.com");
                //window.location.href = "http://www.baidu.com";
            }
        });
    });
}
function modelQaResize() {
    var wd2 = $('.model-question').width() - 36;
    $('.qa-search>select').width(wd2 / 2);
    $(window).resize(function () {
        wd2 = $('.model-question').width() - 36;
        $('.qa-search>select').width(wd2 / 2);
    });
}