
// 截取请求参数
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
/**
 * 目录树入口,多选:checkbox
 */
function treeViewEntry(nodes, vaultGuid, projName, typeId, objId, modelCall) {
    //选中默认项
    var defaultNode = getNodeByModel(nodes, typeId, objId);
    if (defaultNode) defaultNode.Guid = vaultGuid;
    var showAll = (defaultNode && defaultNode.Level > 2) ? true : false;

    var rootDom = $(".m-tree-view");
    if (projName) {
        var projNode = getProject(projName);
        projNode.ShowSubs = true;
        rootDom = createLiDom(projNode, ".m-tree-view", undefined);
    }

    var units = getUnits(nodes);

    for (var i = 0; i < units.length; i++) {
        var unitNode = units[i];
        unitNode.ShowSubs = true;//是否展开子项
        var unitDom = createLiDom(unitNode, rootDom, doWithModel);

        var floors = getFloors(nodes, units[i].ID);
        for (var j = 0; j < floors.length; j++) {
            var floorNode = floors[j];
            floorNode.ShowSubs = showAll;
            var floorDom = createLiDom(floorNode, unitDom, doWithModel);

            var discs = getDisciplines(nodes, floors[j].ID);
            for (var k = 0; k < discs.length; k++) {
                var discNode = discs[k];
                //discNode.ShowSubs = false;
                var discDom = createLiDom(discNode, floorDom, doWithModel);
            }
        }
    }

    //tree view 收缩、展开
    $(".m-tree-view li>div").click(function (event) {
        if ($(event.target).is("input")) return;
        if ($(this).children("label").hasClass("collapsed")) {//隐藏
            $(this).find(">label>img.toggle-img").attr({ "src": "/Content/Images/model_hide.png", "title": "点击隐藏" });
        } else {//展开
            $(this).find(">label>img.toggle-img").attr({ "src": "/Content/Images/model_toggle.png", "title": "点击展开" });
        }
    });
    //隐藏没子节点的图标
    var domLis = $(".m-tree-view").find("li");
    $.each(domLis, function (key, val) {
        var len = $(val).find(">ul>li").length;
        if (!len) {
            var domImg = $(val).find(">div>label>img.toggle-img");
            domImg.css({ "display": "none" });
        }
    });
 
    //全选
    //$('.m-tree-view input').click(function () {
    //    var state = event.target.checked;// this.checked
    //    var domLi = $(event.target).parents("li").filter(":first");
    //    var subLen = domLi.find('>ul>li').length;
    //    if (subLen) {
    //        checkAll(domLi, state);
    //        checkPart(domLi, state);
    //    } else {
    //        checkPart(domLi, state);
    //    }
    //});

    return defaultNode;
 
    function doWithModel(selectedLi) {
        var currentNode = toNode(selectedLi);
        //alert(JSON.stringify(node));
        var leafNodesPast = subLeafNodes(selectedLi);
        //alert(JSON.stringify(leafNodesPast));
        //<----复选
        var state = selectedLi.find('>div>input')[0].checked;
        var subLen = selectedLi.find('>ul>li').length;
        if (subLen) {
            checkAll(selectedLi, state);
            checkPart(selectedLi, state);
        } else {
            checkPart(selectedLi, state);
        }
        //----> 

        var leafNodesNow = subLeafNodes(selectedLi);
        //alert(JSON.stringify(leafNodesNow));
        if (typeof modelCall === 'function') {
            modelCall(currentNode, leafNodesPast, leafNodesNow);
            //modelCall(node, oldSubNodes, subNodes);
        }
    }
    function toNode(domLi) {
        if (!domLi) return null;
        var len = domLi.find('>div>input:checked').length;
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
    function subLeafNodes(domLi) {//叶子节点
        var res = [];
        var lies = domLi.find('>ul li');
        for (var l = 0; l < lies.length; l++) {
            var li = lies.eq(l);
            var level = li.attr("id").split("_")[0];
            if(level != 3) continue;
            var node = toNode(li);
            res.push(node);
        }
        return res;
    }
}
//获取当前选择的节点
function currentNodes(nodes, vGuid) {
    var res = [];
    var inputs = $(".m-tree-view input[type='checkbox']");
    for (var i = 0; i < inputs.length; i++) {
        if (!inputs[i].checked) continue;
        var oldLi = $(inputs[i]).parents("li").filter(":first");
        var arr = oldLi.attr("id").split("_");
        var n = { 'Guid': vGuid, 'Level': arr[0], 'Type': arr[1], 'ID': arr[2], 'Checked': true };
        for (var j = 0; j < nodes.length; j++) {
            if (nodes[j].Level == n.Level && nodes[j].Type == n.Type && nodes[j].ID == n.ID) {
                n.Model = nodes[j].Model;
                break;
            }
        }
        res.push(n);
    }
    return res;
}

function toCurrentNode(nodes, nodeId) {
    if (!nodeId) return null;
    var arr = nodeId.split("_");
    var n = {
        'Level': arr[0],
        'Type': arr[1],
        'ID': arr[2]
    }
    for (var j = 0; j < nodes.length; j++) {
        if (nodes[j].Level == n.Level && nodes[j].Type == n.Type && nodes[j].ID == n.ID) {
            n.Model = nodes[j].Model;
            break;
        }
    }
    return n;
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
        var pInput = pDomLi.find('>div>input')[0];
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
//
function createLiDom(node, parent, callback) {
    var id = node.Level + "_" + node.Type + "_" + node.ID;
    var targetid = "target_" + id;
    var title = node.Title;
    var len = $(parent).children('ul').length;
    if (!len) {
        var cclass = getLevelClass(node.Level.toString());
        if (cclass) {
            $(parent).append('<ul class="' + cclass + '"></ul>');
        } else {
            $(parent).append('<ul></ul>');
        }
    };
    var nclass = getLevelClass((node.Level + 1).toString());
    var txt = "<li id='" + id + "'><div><input type='checkbox' value='' id='ck_" + id + "' />";
    if (!node.ShowSubs) {
        txt += "<label data-toggle='collapse' data-target='#" + targetid + "' class='collapsed' >";
    } else {
        txt += "<label data-toggle='collapse' data-target='#" + targetid + "' >";
    }

    txt += "<span>" + title + "</span>";
    if (node.Level > 2) {
        if (node.HasModel) txt += "<img class='model-img' src='/Content/Images/model_public.png' title='" + "模型已发布" + "' />";
        else txt += "<img class='model-img' src='/Content/Images/model_empty.png' title='" + "缺少模型" + "' />";
    }
    if (node.Level <= 2) {
        txt += "<img class='toggle-img' src='/Content/Images/model_hide.png' title='" + "点击隐藏" + "' />";
    }
    txt += "</label></div>";
    if (nclass) {
        if (!node.ShowSubs) txt += "<ul id='" + targetid + "' class='collapse " + nclass + "'></ul>";
        else txt += "<ul id='" + targetid + "' class='collapse in " + nclass + "'></ul>";
    } else {
        if (!node.ShowSubs) txt += "<ul id='" + targetid + "' class='collapse'></ul>";
        else txt += "<ul id='" + targetid + "' class='collapse in'></ul>";
    }
    txt += "</li>";
    var newDom = $(txt);
    newDom.appendTo($(parent).find('>ul:first'));

    $("#" + id + ">div").click(function (event) {
        var selectedLi = $(event.target).parents("li").filter(":first");

        if ($(event.target).is("input")) {
            
            if (typeof callback === 'function') {
                callback(selectedLi);
            }
        }
    });
    return newDom;
}
function getLevelClass(level) {
    var res = "";
    switch (level) {
        case "1":
            res = "level-one";
            break;
        case "2":
            res = "level-two";
            break;
        case "3":
            res = "level-three";
            break;
        default:
    }
    return res;
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
function getNodeByModel(nodes, typeId, objId) {
    if (!objId) return null;
    for (var i = 0; i < nodes.length; i++) {
        if (!nodes[i].HasModel) continue;
        if (nodes[i].Model.Type == typeId && nodes[i].Model.ID == objId) {
            return nodes[i];
        }
    }
    return null;
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
        { 'Type': -1, 'ID': -1, 'Title': '项目结构', 'Parent': -1, 'Level': 0, 'HasModel': false },
        //单体
        { 'Type': 2, 'ID': 1, 'Title': '厂房1', 'Parent': -1, 'Level': 1, 'HasModel': false },
        { 'Type': 2, 'ID': 2, 'Title': '厂房2', 'Parent': -1, 'Level': 1, 'HasModel': false },
        { 'Type': 2, 'ID': 3, 'Title': '厂房3', 'Parent': -1, 'Level': 1, 'HasModel': false },
        //楼层
        { 'Type': 3, 'ID': 1, 'Title': '1层', 'Parent': 1, 'Level': 2, 'HasModel': true },
        { 'Type': 3, 'ID': 2, 'Title': '2层', 'Parent': 1, 'Level': 2, 'HasModel': true },
        { 'Type': 3, 'ID': 3, 'Title': '3层', 'Parent': 1, 'Level': 2, 'HasModel': false },
        { 'Type': 3, 'ID': 4, 'Title': '4层', 'Parent': 1, 'Level': 2, 'HasModel': false },
        { 'Type': 3, 'ID': 5, 'Title': '1层', 'Parent': 2, 'Level': 2, 'HasModel': true },
        { 'Type': 3, 'ID': 6, 'Title': '2层', 'Parent': 2, 'Level': 2, 'HasModel': true },
        { 'Type': 3, 'ID': 7, 'Title': '3层', 'Parent': 2, 'Level': 2, 'HasModel': false },
        { 'Type': 3, 'ID': 8, 'Title': '1层', 'Parent': 3, 'Level': 2, 'HasModel': false },
        //专业
        { 'Type': 4, 'ID': 1, 'Title': '建筑', 'Parent': 1, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 2, 'Title': '结构', 'Parent': 1, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 3, 'Title': '建筑', 'Parent': 2, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 4, 'Title': '建筑', 'Parent': 3, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 5, 'Title': '建筑', 'Parent': 4, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 6, 'Title': '给排水', 'Parent': 5, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 7, 'Title': '机电', 'Parent': 6, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 8, 'Title': '建筑', 'Parent': 7, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 9, 'Title': '结构', 'Parent': 7, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 11, 'Title': '建筑', 'Parent': 1, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 12, 'Title': '结构', 'Parent': 1, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 13, 'Title': '建筑', 'Parent': 2, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 14, 'Title': '建筑', 'Parent': 3, 'Level': 3, 'HasModel': true },
        { 'Type': 4, 'ID': 15, 'Title': '建筑', 'Parent': 4, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 16, 'Title': '给排水', 'Parent': 5, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 17, 'Title': '机电', 'Parent': 6, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 18, 'Title': '建筑', 'Parent': 7, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 19, 'Title': '结构', 'Parent': 7, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 21, 'Title': '建筑', 'Parent': 1, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 22, 'Title': '结构', 'Parent': 1, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 23, 'Title': '建筑', 'Parent': 2, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 24, 'Title': '建筑', 'Parent': 3, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 25, 'Title': '建筑', 'Parent': 4, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 26, 'Title': '给排水', 'Parent': 5, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 27, 'Title': '机电', 'Parent': 6, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 28, 'Title': '建筑', 'Parent': 7, 'Level': 3, 'HasModel': false },
        { 'Type': 4, 'ID': 29, 'Title': '结构', 'Parent': 7, 'Level': 3, 'HasModel': false }
    ];
}