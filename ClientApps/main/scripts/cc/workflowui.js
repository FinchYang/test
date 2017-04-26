/*!
 * 工作流UI接口封装
 */
var wfUi = wfUi || {};
(function (u, undefined) {
    //UI初始化
    u.uiInitialize = function (srcSates, title, attachments) {
        var len = srcSates.length;
        $('.main-title>span').text(title + '流程图');
        if (len <= 1) return;
        this.appendAttachment(attachments);
        
        var currentI = this.indexOf(srcSates, "Status", "current");
        var currentState = currentI > 0 ? srcSates[currentI] : srcSates[1];
        $('.f-current-state').text(currentState.Name);
        this.appendTotalTable(srcSates);

        $('.f-current>a').click(function () {
            var mId = 'tModal';
            $('#' + mId).modal();
        });

        for (var i = 0; i < len; i++) {
            var state = srcSates[i];
            this.appendStateTable(state);
            if (i !== len - 1) {
                this.appendArrow();
            }
            this.appendPartTable(state);
        }
        $('.f-panel>a').click(function () {
            var pDiv = $(this).parent('div');
            if (pDiv.length) {
                var mId = 'tModal_' + pDiv.attr('id');
                $('#' + mId).modal();
            }
        });

        this.areaAutoShowing('tbody textarea');
        $("tbody input").attr("readOnly", true);

        $('.modal').on('shown.bs.modal', function () {
            u.areaAutoShowing('tbody textarea');
        });

        this.autoResize();
    }
    u.appendAttachment = function(files){
        //附件
        if(files && files.length){
            $('.main-title').append($('<img src="images/attachment.jpg" title="查看附件" />'));
            for (var i = 0; i < files.length; i++) {
                var f = files[i];
                //var id = "f-"+f.FileVer.ID;
                var rowText = "<tr><td>"+(i+1)+"</td>";
                rowText += "<td><a href='"+f.Url+"'>"+f.Name+"</a></td></td>";
                //rowText += "<td><a id='"+id+"'>"+f.Name+"</a></td></tr>";
                //rowText += "<td><textarea>"+f.Url+"</textarea></td></tr>";
                rowText += "<td><input type='text' value='"+f.Url+"+' /></td>";
                rowText += "</tr>"
                $('#tModal_Attach tbody').append($(rowText));
                // $('#'+id).click((function(file){
                //     return function(){
                //         file.Open();
                //     }   
                // })(f));
            }

            $('.main-title>img').click(function(){
                $('#tModal_Attach').modal();
            });
        }
    }
    //总表
    u.appendTotalTable = function (srcSates) {
        var rowText = "";
        if (srcSates && srcSates.length > 0) {
            for (var i = 1; i < srcSates.length; i++) {
                var s = srcSates[i];
                var isCurrent = (s.Status === "current");
                var str = "";
                if (!s.Data || !s.Data.length) {
                    if (isCurrent) {
                        str += "<tr class='f-current-tr'>";
                    } else {
                        str += "<tr>";
                    }
                    str += "<td><input type='text' value='" + s.Name + "' />" + "</td>";
                    str += "<td></td><td></td><td></td>";
                    str += "</tr>";

                    rowText += str;
                    continue;
                }
                var len = s.Data.length;
                for (var j = 0; j < len; j++) {
                    if (isCurrent) {
                        str += "<tr class='f-current-tr'>";
                    } else {
                        str += "<tr>";
                    }
                    if (len <= 1) {
                        str += "<td><input type='text' value='" + s.Name + "' /></td>";
                    } else if (j == 0) {
                        str += "<td rowspan='" + len + "' style='vertical-align: middle;'>";
                        str += "<input type='text' value='" + s.Name + "' />" + "</td>";
                    }
                    var item = s.Data[j];
                    str += "<td><input type='text' value='" + item.Operator + "' /></td>";
                    str += "<td><input type='text' value='" + item.Time + "' /></td>";
                    str += "<td><textarea>" + item.Comment + "</textarea></td>";
                    str += "</tr>";
                }
                rowText += str;
            }
        }
        $('#tModal tbody').append($(rowText));
    }
    //流程节点
    u.appendStateTable = function (srcState) {
        var len = 0;
        if (srcState.Data && srcState.Data.length) len = srcState.Data.length;
        var typeCl = "panel";
        if (srcState.Status === "past") {
            typeCl += " panel-success";
        }
        if (srcState.Status === "current") {
            typeCl += " panel-danger";
        }
        if (srcState.Status === "future") {
            typeCl += " panel-info";
        }
        typeCl += " f-panel";
        if (len === 0) typeCl += " f-panel-none";
        var txt = '';
        txt += '<div id="' + srcState.Id + '" class="' + typeCl + '">';
        txt += '<div class="panel-heading f-panel-heading">';
        if (srcState.Status === "current") {
            txt += '<div class="panel-title f-panel-title">' + srcState.Name + '</div>';
        } else {
            txt += '<div class="panel-title">' + srcState.Name + '</div>';
        }
        txt += '</div>';
        if (len) {
            txt += '<table class="table table-condensed">';
            var str = "";
            for (var i = 0; i < len; i++) {
                if (i < 2) {
                    var item = srcState.Data[i];
                    str += "<tr>";
                    str += "<td>" + item.Operator + "</td>";
                    str += "<td>" + item.Time + "</td>";
                    str += "<td title='" + item.Comment + "'>" + this.subString(item.Comment, 6, true) + "</td>";
                    str += "</tr>";
                } else if (i === 2) {
                    str += '<tr><td>...</td><td>...</td><td></td></tr>';
                }
            }
            txt += str + '</table>';
            txt += '<a title="查看详情">详细>></a>';
        }
        txt += '</div>';
        $('.f-wrap').append($(txt));
    }
    //节点弹出表
    u.appendPartTable = function (srcState) {
        if (!srcState.Data || !srcState.Data.length) return;
        var mId = "tModal_" + srcState.Id;
        var txt = '<div class="modal fade" id="' + mId + '" role="dialog" aria-hidden="true">';
        txt += '<div class="modal-dialog">';
        txt += '<div class="modal-content">';
        txt += '<a class="close f-close" data-dismiss="modal" aria-hidden="true" ></a>';
        txt += '<table class="table table-condensed">';
        txt += '<caption>' + srcState.Name + '信息表' + '</caption>';
        txt += '<thead><tr><th>操作者</th><th>时间</th><th>备注</th></tr></thead>';
        txt += '<tbody>';
        var len = srcState.Data.length;
        var str = "";
        for (var i = 0; i < len; i++) {
            var item = srcState.Data[i];
            str += "<tr>";
            str += "<td><input type='text' value='" + item.Operator + "' /></td>";
            str += "<td><input type='text' value='" + item.Time + "' /></td>";
            str += "<td><textarea>" + item.Comment + "</textarea></td>";
            str += "</tr>";
        }
        txt += str;
        txt += '<tbody>';
        txt += '</table>';
        txt += '</div></div></div>';
        $('.t-wrap').append($(txt));
    }
    //箭头
    u.appendArrow = function () {
        var txt = '<div class="f-arrow"><img src="images/arrow.jpg" /></div>';
        $('.f-wrap').append($(txt));
    }

    u.autoResize = function() {
        //居中div
        $('.main-title').css({
            'margin-left': ($(window).width() - $(".main-title").width()) * 0.85 / 2
        });

        $('.f-wrap').css({
            'margin-left': ($(window).width() - $(".f-panel").width()) * 0.85 / 2
        });

        $(window).resize(function () {
            $('.main-title').css({
                'margin-left': ($(window).width() - $(".main-title").width()) * 0.85 / 2
            });

            $('.f-wrap').css({
                'margin-left': ($(window).width() - $(".f-panel").width()) * 0.85 / 2
            });
        });
    }
    u.areaAutoShowing = function (selector) {
        //textarea标签处理
        var doms = $(selector);
        for (var i = 0; i < doms.length; i++) {
            var item = doms[i];
            item.style.posHeight = item.scrollHeight;
            if (item.scrollHeight < 120) {
                item.style.overflowY = 'hidden';
            } else {
                item.style.overflowY = 'auto';
            }
            //item.disabled = true;
            $(item).attr('readOnly', true);
            //item.readOnly = true;
        }
    }
    u.indexOf = function (src, keyName, kValue) {
        var index = -1;
        for (var i = 0; i < src.length; i++) {
            if (src[i][keyName] === kValue) {
                index = i;
                break;
            }
        }
        return index;
    }
    //截取字符串 包含中文处理(串,长度,增加...) 
    u.subString = function (str, len, hasDot) {
        var newLength = 0;
        var newStr = "";
        var chineseRegex = /[^\x00-\xff]/g;
        var singleChar = "";
        var strLength = str.replace(chineseRegex, "**").length;
        for (var i = 0; i < strLength; i++) {
            singleChar = str.charAt(i).toString();
            if (singleChar.match(chineseRegex) != null) {
                newLength += 2;
            }
            else {
                newLength++;
            }
            if (newLength > len) {
                break;
            }
            newStr += singleChar;
        }

        if (hasDot && strLength > len) {
            newStr += "...";
        }
        return newStr;
    }
    //UI数据及格式 测试数据
    u.getUiData = function() {
        var res = [];
        res.push({
            "Id": 0,
            "Name": "开始",
            "Status": "past",// only past,current and future
            "Data": []
        });
        res.push({
            "Id": 1,
            "Name": "起草",
            "Status": "past",// only past,current and future
            "Data": [
            {
                "Operator": "A君",
                "Time": "2016/06/20 09:30",
                "Comment": "无备注"
            }]
        });
        res.push({
            "Id": 2,
            "Name": "审核",
            "Status": "past",// only past,current and future
            "Data": [
            {
                "Operator": "B君",
                "Time": "2016/06/20 13:30",
                "Comment": "无备注"
            }]
        });
        res.push({
            "Id": 3,
            "Name": "审阅",
            "Status": "current",// only past,current and future
            "Data": [
            {
                "Operator": "国际中心-张三丰",
                "Time": "2016/06/21 09:30",
                "Comment": "aawq12"
            }, {
                "Operator": "国际中心-李四收",
                "Time": "2016/06/21 12:30",
                "Comment": "备注信息XXXXX\r\nxxxxxx"
            }, {
                "Operator": "国际中心-AAAE君",
                "Time": "2016/06/21 14:30",
                "Comment": "aaaaa1112121"
            }]
        });
        res.push({
            "Id": 4,
            "Name": "归档",
            "Status": "future",// only past,current and future
            "Data": []
        });
        return res;
    }
})(wfUi);
