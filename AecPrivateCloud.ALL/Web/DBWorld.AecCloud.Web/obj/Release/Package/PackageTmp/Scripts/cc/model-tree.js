/**
    * 截取请求参数
    */
QueryString = {
    data: {},
    Initial: function () {
        var aPairs, aTmp;
        var queryString = new String(window.location.search);
        queryString = queryString.substr(1, queryString.length); //remove   "?"
        aPairs = queryString.split("&");
        for (var i = 0; i < aPairs.length; i++) {
            aTmp = aPairs[i].split("=");
            this.data[aTmp[0]] = aTmp[1];
        }
    },
    GetValue: function (key) {
        return this.data[key];
    }
}
//目录树入口
function treeViewEntry(nodes, vaultGuid, projName, modelCall) {
    var rootDom = $("#treeWrap");
    if (projName) {
        var projNode = getProject(projName);
        rootDom = createLiDom(projNode, "#treeWrap", undefined);
    }
   
    var units = getUnits(nodes);
    
    for (var i = 0; i < units.length; i++) {
        var unitNode = units[i];
        var unitDom = createLiDom(unitNode, rootDom, doWithModel);

        var floors = getFloors(nodes, units[i].ID);
        for (var j = 0; j < floors.length; j++) {
            var floorNode = floors[j];
            var floorDom = createLiDom(floorNode, unitDom, doWithModel);

            var discs = getDisciplines(nodes, floors[j].ID);
            for (var k = 0; k < discs.length; k++) {
                var discNode = discs[k];
                var discDom = createLiDom(discNode, floorDom, doWithModel);
            }
        }
    }
    $("#directoryTree").treeview({
        unique: false
    });
    /*
    //全选
    $('#treeWrap input').click(function () {
        var state = event.target.checked;// this.checked
        var domLi = $(event.target).parents("li").filter(":first");
        var ulLen = domLi.find('>ul').length;
        if (ulLen) {
            checkAll(domLi, state);
            checkPart(domLi, state);
        } else {
            checkPart(domLi, state);
        }
    });
    */

    //选中:排斥性
    //$("#treeWrap input[type='radio']").click(function () {
    //    var state = event.target.checked;// this.checked
    //    var inputs = $("#treeWrap input[type='radio']");
    //    for (var i = 0; i < inputs.length; i++) {
    //        if (this != inputs[i]) {
    //            inputs[i].checked = !state;
    //        }
    //    }
    //});

    function doWithModel(selectedLi, oldLi) {
        //var len = selectedLi.find('>span>input:checked').length;
        //var checked = len ? true : false;
        //var arr = selectedLi.attr("id").split("_");
        //var nodeInfo = { 'Guid': vaultGuid, 'Level': arr[0], 'Type': arr[1], 'ID': arr[2], 'Checked': checked };
        //for (var i = 0; i < nodes.length; i++) {
        //    if (nodes[i].Level == nodeInfo.Level && nodes[i].Type == nodeInfo.Type && nodes[i].ID == nodeInfo.ID) {
        //        nodeInfo.Model = nodes[i].Model;
        //        break;
        //    }
        //}
        var node = toNode(selectedLi);
        //alert(JSON.stringify(node));
        var oldNode = toNode(oldLi);
        //alert(JSON.stringify(oldNode));
        if (typeof modelCall === 'function') {
            modelCall(node, oldNode);
        }
    }
    function toNode(domLi) {
        if (!domLi) return null;
        var len = domLi.find('>span>input:checked').length;
        var checked = len ? true : false;
        var arr = domLi.attr("id").split("_");
        var nodeInfo = { 'Guid': vaultGuid, 'Level': arr[0], 'Type': arr[1], 'ID': arr[2], 'Checked': checked };
        for (var i = 0; i < nodes.length; i++) {
            if (nodes[i].Level == nodeInfo.Level && nodes[i].Type == nodeInfo.Type && nodes[i].ID == nodeInfo.ID) {
                nodeInfo.Model = nodes[i].Model;
                break;
            }
        }
        return nodeInfo;
    }
}

//全选部分
function checkAll(domLi, state) {
    var inputs = domLi.find('>ul input');
    for (var i = 0; i < inputs.length; i++) {
        if (this != inputs[i]) {
            inputs[i].checked = state;
        }
    }
}
function checkPart(domLi, state) {
    var pDomlis = domLi.parents('li');
    var tempState = state;
    var tempDom = domLi;
    for (var j = 0; j < pDomlis.length; j++) {
        var pDomLi = pDomlis.eq(j);
        var pInput = pDomLi.find('>span>input')[0];
        var inputs = pDomLi.find('>ul input');
        var ii = getSameCount(inputs, tempState);

        if (tempState && ii === inputs.length) {
            pInput.checked = true;
            tempState = true;
        }
        if (!tempState) {
            var subs = tempDom.find('>ul input');
            var jj = getSameCount(subs, tempState);
            if (ii === jj + 1) {
                pInput.checked = false;
                tempState = false;
            }
        }
        tempDom = pDomLi;
    }
}
function getSameCount(inputs, state) {
    var icount = 0;
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].checked === state) {
            icount++;
        }
    }
    return icount;
}
function createLiDom(node, parent, callback) {
    var id = node.Level + "_" + node.Type + "_" + node.ID;
    var title = node.Title;
    var len = $(parent).children('ul').length;
    if (!len) $(parent).append('<ul></ul>');
    var txt = "<li id='" + id + "'><span>" + //class='folder'
        "<input type='radio' value='' id='ck_" + id + "' /><label >" + title + "</label>"; //for='ck_" + id + "'
    if (node.HasModel == false) txt += "<img src='/Content/Images/model_empty.png' title='" + "缺少模型" + "' />";//type='checkbox'
    txt += "</span></li>";
    var newDom = $(txt);
    newDom.appendTo($(parent).find('>ul:first'));

    $("#" + id + ">span").click(function (event) {
        var selectedLi = $(event.target).parents("li").filter(":first");
        var sLab = selectedLi.find("label").eq(0);
        $('#directoryTree .selected-span').removeClass("selected-span");
        sLab.addClass("selected-span");

        if ($(event.target).is("input")) {
            //radio----->
            var oldLi = null;
            var state = event.target.checked;// this.checked
            var inputs = $("#treeWrap input[type='radio']");
            for (var i = 0; i < inputs.length; i++) {
                if (event.target != inputs[i] && inputs[i].checked === state) {
                    inputs[i].checked = !state;
                    oldLi = $(inputs[i]).parents("li").filter(":first");
                }
            }
            //<-------
            if (typeof callback === 'function') {
                callback(selectedLi, oldLi);
            }

        }
        if ($(event.target).is("label")) {
            $("#" + id + " >.hitarea").click();
        }
    });
    return newDom;
}

function getNodes(vaultGuid) {
    var nodes = [];
    $.ajax({
        type: "GET",
        url: '/Model/TreeNodes?guid=' + vaultGuid,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (data, textStatus) {
            nodes = data;
        },
        error: function (xmlHttpRequest, textStatus, errorThrown) {
            alert(" # Error: " + errorThrown);
        }
    });
    return nodes;
}
//项目
function getProject(projName) {
    if (projName) return { 'Type': -1, 'ID': -1, 'Title': projName, 'Parent': -1, 'Level': 0 };
    var nodes = getTestNodes();
    var res = getNodesOnLevel(nodes, 0, -1);
    if (res.length) return res[0];
    return null;
}
//单体
function getUnits(nodes) {
    var res = getNodesOnLevel(nodes, 1, -1);
    return res;
}
//楼层
function getFloors(nodes, parentId) {
    var res = getNodesOnLevel(nodes, 2, parentId);
    return res;
}
//专业
function getDisciplines(nodes, parentId) {
    var res = getNodesOnLevel(nodes, 3, parentId);
    return res;
}
function getNodesOnLevel(nodes, levelId, parentId) {
    var res = [];
    for (var i = 0; i < nodes.length; i++) {
        if (nodes[i].Level === levelId && nodes[i].Parent === parentId) {
            res.push(nodes[i]);
        }
    }
    return res;
}
function getTestNodes() {
    return [
        //项目
        { 'Type': -1, 'ID': -1, 'Title': '项目结构', 'Parent': -1, 'Level': 0 },
        //单体
        { 'Type': 2, 'ID': 1, 'Title': '厂房1', 'Parent': -1, 'Level': 1 },
        { 'Type': 2, 'ID': 2, 'Title': '厂房2', 'Parent': -1, 'Level': 1 },
        { 'Type': 2, 'ID': 3, 'Title': '厂房3', 'Parent': -1, 'Level': 1 },
        //楼层
        { 'Type': 3, 'ID': 1, 'Title': '1层', 'Parent': 1, 'Level': 2 },
        { 'Type': 3, 'ID': 2, 'Title': '2层', 'Parent': 1, 'Level': 2 },
        { 'Type': 3, 'ID': 3, 'Title': '3层', 'Parent': 1, 'Level': 2 },
        { 'Type': 3, 'ID': 4, 'Title': '4层', 'Parent': 1, 'Level': 2 },
        { 'Type': 3, 'ID': 5, 'Title': '1层', 'Parent': 2, 'Level': 2 },
        { 'Type': 3, 'ID': 6, 'Title': '2层', 'Parent': 2, 'Level': 2 },
        { 'Type': 3, 'ID': 7, 'Title': '3层', 'Parent': 2, 'Level': 2 },
        { 'Type': 3, 'ID': 8, 'Title': '1层', 'Parent': 3, 'Level': 2 },
        //专业
        { 'Type': 4, 'ID': 1, 'Title': '建筑', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 2, 'Title': '结构', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 3, 'Title': '建筑', 'Parent': 2, 'Level': 3 },
        { 'Type': 4, 'ID': 4, 'Title': '建筑', 'Parent': 3, 'Level': 3 },
        { 'Type': 4, 'ID': 5, 'Title': '建筑', 'Parent': 4, 'Level': 3 },
        { 'Type': 4, 'ID': 6, 'Title': '给排水', 'Parent': 5, 'Level': 3 },
        { 'Type': 4, 'ID': 7, 'Title': '机电', 'Parent': 6, 'Level': 3 },
        { 'Type': 4, 'ID': 8, 'Title': '建筑', 'Parent': 7, 'Level': 3 },
        { 'Type': 4, 'ID': 9, 'Title': '结构', 'Parent': 7, 'Level': 3 },
        { 'Type': 4, 'ID': 11, 'Title': '建筑', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 12, 'Title': '结构', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 13, 'Title': '建筑', 'Parent': 2, 'Level': 3 },
        { 'Type': 4, 'ID': 14, 'Title': '建筑', 'Parent': 3, 'Level': 3 },
        { 'Type': 4, 'ID': 15, 'Title': '建筑', 'Parent': 4, 'Level': 3 },
        { 'Type': 4, 'ID': 16, 'Title': '给排水', 'Parent': 5, 'Level': 3 },
        { 'Type': 4, 'ID': 17, 'Title': '机电', 'Parent': 6, 'Level': 3 },
        { 'Type': 4, 'ID': 18, 'Title': '建筑', 'Parent': 7, 'Level': 3 },
        { 'Type': 4, 'ID': 19, 'Title': '结构', 'Parent': 7, 'Level': 3 },
        { 'Type': 4, 'ID': 21, 'Title': '建筑', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 22, 'Title': '结构', 'Parent': 1, 'Level': 3 },
        { 'Type': 4, 'ID': 23, 'Title': '建筑', 'Parent': 2, 'Level': 3 },
        { 'Type': 4, 'ID': 24, 'Title': '建筑', 'Parent': 3, 'Level': 3 },
        { 'Type': 4, 'ID': 25, 'Title': '建筑', 'Parent': 4, 'Level': 3 },
        { 'Type': 4, 'ID': 26, 'Title': '给排水', 'Parent': 5, 'Level': 3 },
        { 'Type': 4, 'ID': 27, 'Title': '机电', 'Parent': 6, 'Level': 3 },
        { 'Type': 4, 'ID': 28, 'Title': '建筑', 'Parent': 7, 'Level': 3 },
        { 'Type': 4, 'ID': 29, 'Title': '结构', 'Parent': 7, 'Level': 3 }
    ];
}