//存储文件构件关联
function xbim_load_GetAssType(guid,modeldata) {

    //guid_file_asstypesLibrary = [];
    var model = {
        name: guid,
        modelId: modeldata.ObjId,
        modelType:modeldata.TypeId,
        children: xBrowserHost._model.assetTypes,
        viewer: xViewerHost,
        browser: xBrowserHost,
        idMapping: xIdMappingHost
    }; 
    var hasguid = isContainGuid(guid);
    if (!hasguid) {
        guid_file_asstypesLibrary.push(model);
    }
    //准备加载
    $('.l-wrapper').show();
    setTimeout(function () {
        $('.l-wrapper').hide();//加载中
    }, 500);
    //加载完成
    if (xQuestionLoaedState.isQuestionCallback) {
        setTimeout(function () {
            xQuestionIntervalCount = 0;
            xQuestionLoaedState.checkLoaded(model);
            xQuestionInterval = window.setInterval(" xQuestionLoaedState.getModelIdList()", 500);
        }, 500);
        xQuestionLoaedState.isQuestionCallback = false;
    }
}
//删除文件构件关联
function xbim_unload_GetAssType(guid) {

    var hasguid = isContainGuid(guid);
    if (hasguid) {
        for (var i=0;i< guid_file_asstypesLibrary.length;i++) {
            if (guid_file_asstypesLibrary[i].name === guid) {
                guid_file_asstypesLibrary.splice(i, 1);break;
            }
        }
    }
    //准备加载
    $('.l-wrapper').show();
    setTimeout(function () {
        $('.l-wrapper').hide();//加载中
    }, 500);
}

//获取当前文件
function xbim_currentGuid(guid) {
    for (var i = 0; i < guid_file_asstypesLibrary.length; i++) {
        if (guid_file_asstypesLibrary[i].name === guid) {
            return guid_file_asstypesLibrary[i];
        }
    }
    return null;
}
function xbim_currentGuidByModel(modelId,typeId) {
    for (var i = 0; i < guid_file_asstypesLibrary.length; i++) {
        if (guid_file_asstypesLibrary[i].modelId === modelId && guid_file_asstypesLibrary[i].modelType === typeId) {
            return guid_file_asstypesLibrary[i];
        }
    }
    return null;
}
//获取当前文件_模型
function xbim_currentViewer(guid) {
    var currentGuid = xbim_currentGuid(guid);
    if (currentGuid != null) return currentGuid.viewer;
    return null;
    //for (var i = 0; i < guid_file_asstypesLibrary.length; i++) {
    //    if (guid_file_asstypesLibrary[i].name === guid) {
    //        return guid_file_asstypesLibrary[i].viewer;
    //    }
    //}
    //return null;
}
//获取当前文件_数据
function xbim_currentBrowers(guid) {
    var currentGuid = xbim_currentGuid(guid);
    if (currentGuid != null) return currentGuid.browser;
    return null;
    //for (var i = 0; i < guid_file_asstypesLibrary.length; i++) {
    //    if (guid_file_asstypesLibrary[i].name === guid) {
    //        return guid_file_asstypesLibrary[i].browser;
    //    }
    //}
    //return null;
}
//获取当前文件_构件库
function xbim_currentIdMapping(guid,modelId,typeId) {
    var currentGuid = xbim_currentGuid(guid);
    if (currentGuid != null) return currentGuid.idMapping;
    currentGuid = xbim_currentGuidByModel(modelId, typeId);
    if (currentGuid != null) return currentGuid.idMapping;
    return null;
    //for (var i = 0; i < guid_file_asstypesLibrary.length; i++) {
    //    if (guid_file_asstypesLibrary[i].name === guid) {
    //        return guid_file_asstypesLibrary[i].browser;
    //    }
    //}
    //return null;
}
//根据库构件ID获取本地构件ID
function xbim_getIdsByIdMapping(ifcIds, idMapping) {
    var list = [];
    for (var key in ifcIds) {
        var ifcId = ifcIds[key];
        var id = xbim_getIdByIdMapping(ifcId, idMapping);
        if (id)list.push(id);
    }
    return list;
}
function xbim_getIdByIdMapping(ifcId, idMapping) {
    for (var key in idMapping) {
        if (key == ifcId) {
            return idMapping[key];
        }
    }
    return null;
}

//判断是否已存在文件
function isContainGuid(guid) {
    if (guid_file_asstypesLibrary.length == 0) return false;
    var hasguid = Enumerable.From(guid_file_asstypesLibrary).Where(function(c) { return c.name === guid; }).ToArray();
    return hasguid.length!=0;
}

//搜索id到数据存储（完整流程）
function xbim_search_GetBrower(id, nodes) {
    var list = [];
    if (!id || !nodes || nodes.length == 0) return list;
    //Step
    //1:根据搜索ID找到所有匹配构件所在文件
    var nodesRowInfoList = getNodesInfoById(nodes, id);
    //2：根据匹配文件列表查找数据存储
    if (nodesRowInfoList.length == 0) return list;
    return searchBrowerByGuid(nodesRowInfoList);
}

//搜索关键字到表格（完整流程）
function xbim_search_GetAssType(xSearchKeyword, nodes) {
    var keyword = xSearchKeyword;
    if (keyword === "") {
        appendTableRow("#tableWrap tbody", []);
        return;
    }
    else if (keyword.indexOf("*") != -1) {
        keyword = "";
    }
    //Step
    //1:根据搜索关键字找到所有匹配构件所在文件
    //2：根据匹配文件列表查找文件层级
    var nodesRowInfoList = getNodesInfo(nodes, keyword);
    //3：添加文件层级信息到表格
    appendTableRow("#tableWrap tbody",nodesRowInfoList);
}

//返回包含关键字构件的文件信息（含重复）
function getGuidsAsstypeisContainKeyword(keyword){
    var list=[];
    for(var guidkey in guid_file_asstypesLibrary)
    {
        var guid =guid_file_asstypesLibrary[guidkey];
        var keywordAsstypes=getAsstypeisContainKeyword(guid.children,keyword);
        if(keywordAsstypes.length!=0)
        {
            for (var asstypekey in keywordAsstypes) {
                for (var asstypechildkey in keywordAsstypes[asstypekey].children) {
                    list.push({ guid: guid.name, asstypename: keywordAsstypes[asstypekey].children[asstypechildkey].name, asstypeid: keywordAsstypes[asstypekey].children[asstypechildkey].id });
                }
            }
        }
    }
    return list;
}
//返回包含ID构件的文件信息（第一个）
function getGuidsAsstypeisContainId(id) {
    var list = [];
    for (var guidkey in guid_file_asstypesLibrary) {
        var guid = guid_file_asstypesLibrary[guidkey];
        var keywordAsstypes = getAsstypeisContainId(guid.children, id);
        if (keywordAsstypes.length != 0) {
            for (var asstypekey in keywordAsstypes) {
                list.push({ guid: guid.name, asstypename: keywordAsstypes[asstypekey].name, asstypeid: keywordAsstypes[asstypekey].id });
            }
        }
    }
    return list;
}

//判断构件名称是否包含关键字
function getAsstypeisContainKeyword(asstypes, keyword) {
    return Enumerable.From(asstypes).Where(function (c) { return c.name.indexOf(keyword.trim()) != -1; }).ToArray();
}
//判断构件是否包含ID
function getAsstypeisContainId(asstypes, id) {
    var array = Enumerable.From(asstypes).Where(function (c) { return c.id === id; }).ToArray();
    if (array.length != 0)return array;
    for (var key in asstypes) {
        array = Enumerable.From(asstypes[key].children).Where(function (c) { return c.id === id.toString(); }).ToArray();
        if (array.length != 0) return array;
    }
    return [];
}

//获取层级结果
function getNodesInfo(nodes, keyword) {
    var list = [];
    var guidInfoList = getGuidsAsstypeisContainKeyword(keyword);
    for (var guidInfokey in guidInfoList) {
        var guidInfo = guidInfoList[guidInfokey];
        var guid = guidInfo.guid;
        var asstypeid = guidInfo.asstypeid;
        var asstypename = guidInfo.asstypename;
        var nodesArray = getRelativeNodes(nodes, guid);
        var nodesRowInfo = convertNodesInfo(guid,asstypeid, asstypename, nodesArray);
        list.push(nodesRowInfo);
    }
    return list;
}
function getNodesInfoById(nodes, id) {
    var list = [];
    var guidInfoList = getGuidsAsstypeisContainId(id);
    for (var guidInfokey in guidInfoList) {
        var guidInfo = guidInfoList[guidInfokey];
        var guid = guidInfo.guid;
        var asstypeid = guidInfo.asstypeid;
        var asstypename = guidInfo.asstypename;
        var nodesRowInfo = {guid:guid,id:asstypeid,name:asstypename};
        list.push(nodesRowInfo);
    }
    return list;
}

//搜索文件构件关联
function getRelativeNodes(nodes, key) {//E7E9577E-7A28-493D-B4B2-FC14E31C7E92-0-74
    if (typeof key !== "string") {
        return [];
    }
    var keyArr = key.split("-");
    var len = keyArr.length;
    if (len < 3) {
        return [];
    }
    var typeId = parseInt(keyArr[len - 2]);
    var objId = parseInt(keyArr[len - 1]);
    if (typeId === NaN || objId === NaN) {
        return [];
    }
    var currentNode = undefined;
    var res = [];
    for (var i = 0; i < nodes.length; i++) {
        if (!nodes[i].HasModel) continue;
        if (nodes[i].Model.Type == typeId && nodes[i].Model.ID == objId) {
            currentNode = nodes[i];
            res.push(currentNode);
            break;
        }
    }
    if (!currentNode) {
        return [];
    }
    var tempNode = currentNode;
    for (var j = currentNode.Level; j > 0; j--) {
        var level = j - 1;
        if (level < 1) continue;
        for (var k = 0; k < nodes.length; k++) {
            var node = nodes[k];
            if (node.Level == level && node.ID == tempNode.Parent) {
                tempNode = node;
                res.push(node);
                break;
            }
        }
    }
    res.sort(function (a, b) {
        return a.Level - b.Level;
    });
    //alert(JSON.stringify(res));
    return res;//{Level, Title,...}
}

//转换层级结果
function convertNodesInfo(guid, asstypeid, asstypename, nodesArray)
{
    var list =[];
    var lv1S = Enumerable.From(nodesArray).Where(function (c) { return c.Level===1; }).ToArray();
    var lv2S = Enumerable.From(nodesArray).Where(function (c) { return c.Level === 2; }).ToArray();
    var lv3S = Enumerable.From(nodesArray).Where(function (c) { return c.Level === 3; }).ToArray();
    list.push(guid);
    list.push(asstypeid);
    list.push(asstypename);
    list.push(lv1S.length != 0 ? lv1S[0].Title : "-");
    list.push(lv2S.length != 0 ? lv2S[0].Title : "-");
    list.push(lv3S.length != 0 ? lv3S[0].Title : "-");
    //list.push(asstypeid); 
    return list;
}

//添加行
function appendTableRow(parten,nodesRowInfoList)
{
    $(parten).empty();
    for (var key in nodesRowInfoList)
    {
        var nodesRows = nodesRowInfoList[key];
        $(parten).append("<tr title=" + nodesRows[0] + " id=" + nodesRows[1] + " onclick='xbim_zoomtoentity(this)'><td>" + nodesRows[2] + "</td><td>" + nodesRows[3] + "</td><td>" + nodesRows[4] + "</td><td>" + nodesRows[5] + "</td></tr>");
    }
}

//查找数据存储集合
function searchBrowerByGuid(nodesRowInfoList) {
    var list = [];
    for (var guidkey in guid_file_asstypesLibrary) {
        var guid = guid_file_asstypesLibrary[guidkey];
        for (var key in nodesRowInfoList) {
            if (nodesRowInfoList[key].guid == guid.name) {
                list.push(guid.browser);
            }
        }
    }
    return list;
}

 



