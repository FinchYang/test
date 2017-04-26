
//多选
function multiSelectChange() {
    $('#multi-select').change(function () {
        var mSel = $(this).prop("checked");
        if (!mSel) {
            var idsStr = $("#btnworkflow").data("id");
            if (idsStr) {
                var ids = idsStr.split(' ');
                $("#btnworkflow").data("id", ids[ids.length - 1]);
            }
            multSels = [];
        }
    });
}
//点击问题记录列表中一记录的回调
function modelQaCallback(nodes, vGuid, qa) {
    //qa= {Components[{IfcId,ModelType,ModelID}]
    var parts = qa.Components;//构件
    if (!parts.length) {
        alert("缺少构件");
        return;
    }
    //alert(JSON.stringify(parts) + "\r\n" );
    xQuestionLoaedState.isQuestionCallback = true;
    xQuestionLoaedState.init(nodes, qa);
    xQuestionLoaedState.checkProgress(nodes, vGuid, qa);
    //var tCount = 0;
    xQuestionLoaedState.updateProgress();
    if (xQuestionLoaedState.Progress != 1)return;
    var t = window.setTimeout("xQuestionLoaedState.getModelIdList()", 500);
    //t=window.clearTimeout(t);
    //xQuestionInterval = window.setInterval("xQuestionLoaedState.getModelIdList()", 500);
}
//点击目录节点的回调
//function modelCallFn(node, oldNode, ifcGuid, isQuestionCallBack) {//{Guid, Level, Type, ID, Checked, Model{ID, Type}}
//}
//点击目录节点的回调, checkbox多选
function multiModelCallFn(currentNode, pastLeafNodes, nowLeafNodes, ifcGuid) {
    //node: {Guid, Level, Type, ID, Checked, Model{ID, Type}}

    //第三层（叶子节点-专业）
    if (  currentNode.Level == 3) {
        singlModelCallFn(currentNode, ifcGuid);
    }//第一层，第二层（根节点-单体，根节点-楼层）
    else {
        for (var key in nowLeafNodes) {
            var leafnode = nowLeafNodes[key];
            singlModelCallFn(leafnode, pastLeafNodes, nowLeafNodes, ifcGuid);
        }
    }
} 
function singlModelCallFn(node, pastLeafNodes, nowLeafNodes, ifcGuid) {
    nodeId = node.Level + "_" + node.Type + "_" + node.ID;
    var modelId = $("#" + nodeId).data("modelId");
    //alert(nodeId);

    if (node.Checked) {
        if (hasModelId(node, pastLeafNodes, nowLeafNodes)) {
        } else {
            var guid = node.Guid.replace('{', '').replace('}', '');
            var model = {};
            if (node.Model) model = node.Model;
            var data = {
                Guid: guid,
                TypeId: model.Type,
                ObjId: model.ID
            }
            if (ifcGuid) data.ifcGuid = ifcGuid;
            guid_file_selected = data;
            $.getJSON("/Model/ModelPath?Guid=" + data.Guid + "&TypeId=" + data.TypeId + "&ObjId=" + data.ObjId, function (result) {
                if (!isValidModelpath(result)) {
                    alert(result);
                    var pInput = $('#' + nodeId).find('>div>input')[0];
                    pInput.checked = false;
                    return;
                }
                //准备加载
                $('.l-wrapper').hide();
                setTimeout(function () {
                    $('.l-wrapper').show();//加载中
                }, 500);

                xQuestionLoaedState.isQuestionCallback = false;

                viewer = loadModel(result, data, false);

                btnsClickOfViewer(viewer);

            });

            if (!hasNodeId(node,guid_file_loaded)) guid_file_loaded.push(node);
        }
    } else {     
        ////alert(nodeId+":"+ modelId);    
        //if (hasModelId(node, pastLeafNodes, nowLeafNodes)) {
        //var nodeId = node.Level + "_" + node.Type + "_" + node.ID;

        //var modelId = $("#" + nodeId).data("modelId");
        if (modelId !== undefined) {
            removeNodeId(node, guid_file_loaded);
            viewer.unload(modelId);
            guid = node.Guid.replace('{', '').replace('}', '');
            model = {};
            if (node.Model) model = node.Model;
            xbim_unload_GetAssType(guid + "-" + model.Type + "-" + model.ID);
        }
        //data = {
        //    Guid: guid,
        //    TypeId: model.Type,
        //    ObjId: model.ID
        //}
        //if (modelId !== undefined) xbim_unload_GetAssType(data.Guid + "-" + data.TypeId + "-" + data.ObjId);
        //}
        //if (modelId !== undefined) xbim_unload_GetAssType(data.Guid + "-" + data.TypeId + "-" + data.ObjId);
    }
}
function hasModelId(node, pastLeafNodes, nowLeafNodes) {
    if (node) {
        var past = false;
        var now = false;
        for (var keypast in pastLeafNodes) {
            var pastLeafNode = pastLeafNodes[keypast];
            if (pastLeafNode.ID == node.ID) {
                past = pastLeafNode.Checked;
                break;
            }
        }
        for (var keynow in nowLeafNodes) {
            var nowLeafNode = nowLeafNodes[keynow];
            if (nowLeafNode.ID == node.ID) {
                now = nowLeafNode.Checked;
                break;
            }
        }
        if (past && now) {
            return true;
        } else if (past && !now) {
            return true;
        } else {
            return false;
        }
    }
    return false;
}
function hasNodeId(node, nodeList) {
    for (var key in nodeList) {
        var hasnode = nodeList[key];
        if (node.ID == hasnode.ID && node.Type == hasnode.Type) return true;
    }
    return false;
}
//删除模型时移除相关全局信息
function removeNodeId(node, nodeList) {
    if (!node || !nodeList || nodeList.length == 0) return;
    var index = -1;
    for (var i in nodeList) {
        if (nodeList[i].ID == node.ID && nodeList[i].Type == node.Type) {
            index = i;
            break;
        }
    }
    if (index != -1) nodeList.splice(index, 1);
    $('.l-wrapper').hide();//加载中
}
//按钮状态
function btnsClickOfViewer(viewer) {
    if (viewer) {
        $('#clip-btn').click(function () {
            viewer.clip();
        });
        $('#unclip-btn').click(function () {
            viewer.unclip();
        });
        $('#pan-btn').click(function () {
            viewer.navigationMode = 'pan';
        });
        $('#rotate-btn').click(function () {
            viewer.navigationMode = 'orbit';
        });
        $('#zoom-btn').click(function () {
            viewer.navigationMode = 'zoom';
        });
        $('.radio-text').click(function () {
            $(this).prev().click();
        });
        $('#reset-btn').click(function () {
            viewer.resetStates();
            $('.entity-visible').prop('checked', true);
            if (multSels) {
                multSels = [];
            }
            $("#btnworkflow").data("id", "");
        });
    }
}
function isValidModelpath(modelName) {
    var sep = "-";
    var index = modelName.lastIndexOf(sep);
    if (index === -1) return false;
    var str = modelName.substr(0, index);
    index = str.lastIndexOf(sep);
    if (index === -1) return false;
    return true;
}
function getModelData() {
    var data = {};
    var guids = getQuerystring("guid");
    if (guids.length > 0) {
        data.Guid = guids[0];
    }
    var typeIds = getQuerystring("typeid");
    if (typeIds.length > 0) {
        data.Type = parseInt(typeIds[0]);
    };
    var ids = getQuerystring("objid");
    if (ids.length > 0) {
        data.ObjId = parseInt(ids[0]);
    };
    var ifcGuid = getQuerystring("ifcguid");
    if (ifcGuid.length > 0) {
        data.ifcGuid = ifcGuid[0];
    }
    return data;
}
//添加问题反馈
function getQuerystring(key) {
    var re = new RegExp('(?:\\?|&)' + key + '=(.*?)(?=&|$)', 'gi');
    var r = [], m;
    while ((m = re.exec(document.location.search)) != null) r.push(m[1]);
    return r;
}
function loadControls() {
    //declare viewer and browser at the beginning so that it can be used as a variable before it is initialized.
    function initControls() {
        $("#semantic-descriptive-info").accordion({
            heightStyle: "fill"
        });
        $("#semantic-model").accordion({
            heightStyle: "fill"
        });
        $("#btnLocate").button().click(function () {
            var id = $(this).data("id");
            if (typeof (id) != "undefined" && viewer) {
                var n = parseInt(id);
                viewer.zoomTo(n);
                viewer.setState(xState.HIGHLIGHTED, [n]);
                var entityType = $(this).data("type");
                if (entityType && entityType === "asset") {
                    var scrollTo = $("#" + entityType + id);
                    if (!scrollTo.prop("checked")) {
                        scrollTo.click();
                        viewer.setState(xState.HIGHLIGHTED, [n]);
                    }
                    scrollTo.parents("ul").css("display", "block");
                    scrollTo.next().addClass("ui-selected");
                    // http://stackoverflow.com/questions/20101059/scroll-element-into-view-in-scrollable-container
                    // http://stackoverflow.com/questions/2905867/how-to-scroll-to-specific-item-using-jquery
                    var myContainer = $("#structure").parent();  // scrollable element
                    myContainer.animate({
                        scrollTop: scrollTo.offset().top - myContainer.height() / 2 - myContainer.offset().top + myContainer.scrollTop()
                    }, 500);
                }
            }
        });
        $("#btnworkflow").button().click(function () {

            var idsStr = $(this).data("id");
            if (!idsStr) {
                alert("请选择构件！");
                return;
            }

            var entityType = $(this).data("type");
            if (entityType && entityType === "asset") {
                var id = idsStr;
                //console.log("godspeed-btnworkflow--:" + id);

                //console.log("ifc:" + ifc);
                $(".projectListBtnConfirm").data("id", id);
                $(".projectListBtnConfirm").show();


            }
        });

        $(".projectListBtnConfirm a").click(function () {

            var id = $(this).parent().parent().data("id");


            $(".projectListBtnConfirm").fadeOut("fast");
            if ($(this).attr("name") === "sure") {

                var guid11 = getQuerystring("guid")[0];
                var prefix = document.getElementById("homeurl").value;

                var ifc = [];
                var filelList = guid_file_asstypesLibrary;
                for (var key in filelList) {
                    var file = filelList[key];
                    if (file.idMapping && id) {
                            var idsStr = id.toString().split(' ');
                        for (var ii = 0; ii < idsStr.length; ii++) {
                                    var entId = idsStr[ii];
                                        for (var kk in file.idMapping) {
                                if (file.idMapping.hasOwnProperty(kk)) {
                                        if (file.idMapping[kk]== entId) {
                                                    ifc.push(kk);
                                            break;
                                             }
                                         }
                                 }
                         }
                  }
            }
                //if (idMapping && id) {
                //    var idsStr = id.toString().split(' ');
                //    for (var ii = 0; ii < idsStr.length; ii++) {
                //        var entId = idsStr[ii];
                //        for (var kk in idMapping) {
                //            if (idMapping.hasOwnProperty(kk)) {
                //                if (idMapping[kk] == entId) {
                //                    ifc.push(kk);
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //}

                ////if (idMappings && idMappings.length && id) {
                ////    var idsStr = id.toString().split(' ');
                ////    for (var i = 0; i < idMappings.length; i++) {
                ////        var idMap = idMappings[i];
                ////        for (var ii = 0; ii < idsStr.length; ii++) {
                ////            var entId = idsStr[ii];
                ////            for (var kk in idMap) {
                ////                if (idMap.hasOwnProperty(kk)) {
                ////                    if (idMap[kk] == entId) {
                ////                        ifc.push(kk);
                ////                        break;
                ////                    }
                ////                }
                ////            }
                ////        }
                ////    }
                ////}
                if (ifc.length === 0) {
                    alert("未选择构件！");
                    return;
                }
                //测试视点
                var xCameraQurstionText = JSON.stringify(xCameraQurstion);
                var xHideIdsText = JSON.stringify(ids_hideed);
                if (ids_hideed.length && ids_hideed.length != 0) {
                    xCameraQurstionText += "^" + xHideIdsText;
                } else {
                    xCameraQurstionText += "^" + "";
                }
                if (guid_file_asstypesLibrary.length && guid_file_asstypesLibrary.length != 0) {
                    xCameraQurstionText += "^" + guid_file_asstypesLibrary[guid_file_asstypesLibrary.length-1].name;
                } else {
                    xCameraQurstionText += "^" + "";
                }
                //alert(xCameraQurstionText);

                var classAlias = $('#select-class').val();

                var apiUrl = "/Model/WorkFlow?vaultGuid=" + guid11 + "&classAlias=" + classAlias + "&id=" + ifc.join(',') + "&viewPort=" + "M" + encodeURIComponent(xCameraQurstionText); //prefix + //encodeURIComponent(xCameraQurstionText)
                $.ajax({
                    url: apiUrl,
                    async: true,
                    success: function (data) {
                        if (typeof data !== "string") {
                            alert(data.message);
                            return;
                        }
                        //console.log("success 3:" + data.substring(3));
                        window.location.href = data.substring(3);
                    },
                    error: function (xmlHttpRequest, textStatus, errorThrown) {
                        console.log("获取失败：(status:" + xmlHttpRequest.status + ", readyState:" + xmlHttpRequest.readyState +
                            ", textStatus:" + textStatus + ", errorThrown: " + errorThrown + ")");
                    }
                });
            }
        });

        $("#toolbar button").button();
    }
    function reinitControls() {
        $("#semantic-model").accordion("refresh");
        $("#semantic-descriptive-info").accordion("refresh");
    }
    initControls();
    $(window).resize(function () {
        reinitControls();
    });
}
//加载模型
function loadModel(modelName, modeldata, isQuestionCallBack) {
    var ifcGuid = modeldata.ifcGuid;
    //declare viewer and browser at the beginning so that it can be used as a variable before it is initialized.
    function browserOn(browser) {
        browser.on("loaded", function (args) {
            var facility = args.model.facility;
            //render parts
            browser.renderSpatialStructure("structure", true);
            browser.renderAssetTypes("assetTypes", true);
            browser.renderSystems("systems");
            browser.renderZones("zones");
            browser.renderContacts("contacts");
            browser.renderDocuments(facility[0], "facility-documents");
            //open and selectfacility node
            //$("#structure > ul > li").click();

            //if (idMappings) {
            //    idMappings.push(args.model.guidMapping);
            //}
            //if (!idMapping) {
                idMapping = args.model.guidMapping;
                if (idMapping && ifcId) {
                    currentEntityId = idMapping[ifcId];
                    if (currentEntityId) {
                        browser.activateEntity(currentEntityId);
                        $("#btnLocate").data("id", currentEntityId);
                        $("#btnworkflow").data("id", currentEntityId);
                        //console.log("idMapping");
                        keepTarget = true;
                    }
                }
                if (idMapping && ifcIds && ifcIds.length) {
                    currentEntityIds = [];
                    for (var i = 0; i < ifcIds.length; i++) {
                        var iid = ifcIds[i];
                        var entityId = idMapping[iid];
                        if (entityId) {
                            currentEntityIds.push(entityId);
                            browser.activateEntity(entityId);
                            $("#btnLocate").data("id", entityId);
                            $("#btnworkflow").data("id", entityId);
                        }
                    }
                    if(currentEntityIds.length) keepTarget = true;
                }
            //}
            //存储新加载模型信息判断已有
            var data = guid_file_selected;
            xViewerHost = viewer;
            xBrowserHost = browser;
            xIdMappingHost = idMapping;
            xbim_load_GetAssType(data.Guid + "-" + data.TypeId + "-" + data.ObjId, modeldata);
            xCameraPostion = viewer.getCameraPosition();
            viewer.renderingMode = "normal";
            if (!isOnLoadSelectEvent) { xbim_multSelectModeChange();
                isOnLoadSelectEvent = true;
            }
        });
        browser.on("entityClick", function (args) {
            var span = $(args.element).children("span.xbim-entity");
            if (document._lastSelection)
                document._lastSelection.removeClass("ui-selected");
            span.addClass("ui-selected");
            document._lastSelection = span;
        });
        browser.on("entityActive", function (args) {
            var isRightPanelClick = false;
            if (args.element)
                if ($(args.element).parents("#semantic-descriptive-info").length != 0)
                    isRightPanelClick = true;

            //set ID for location button
            $("#btnLocate").data("id", args.entity.id);
            $("#btnLocate").data("type", args.entity.type);
            $("#btnworkflow").data("id", args.entity.id);
            $("#btnworkflow").data("type", args.entity.type);
            //$("#btnworkflow").data("ifcguid", args.entity.ifcGuid);
            browser.renderPropertiesAttributes(args.entity, "attrprop");
            browser.renderAssignments(args.entity, "assignments");
            browser.renderDocuments(args.entity, "documents");
            browser.renderIssues(args.entity, "issues");

            if (isRightPanelClick)
                $("#attrprop-header").click();
        });

        browser.on("entityDblclick", function (args) {
            var entity = args.entity;
            var allowedTypes = ["space", "assettype", "asset"];
            if (allowedTypes.indexOf(entity.type) === -1) return;

            var id = parseInt(entity.id);
            if (id && viewer) {
                viewer.resetStates(undefined, true);
                viewer.renderingMode = "x-ray";
                if (entity.type === "assettype") {
                    var ids = [];
                    for (var i = 0; i < entity.children.length; i++) {
                        id = parseInt(entity.children[i].id);
                        ids.push(id);
                    }
                    viewer.setState(xState.HIGHLIGHTED, ids);
                }
                else {
                    viewer.setState(xState.HIGHLIGHTED, [id]);
                }
                viewer.zoomTo(id);
                keepTarget = true;
            }
        });
        browser.on("entityCheck", function (args) {
            var entity = args.entity;
            var allowedTypes = ["floor", "space", "assettype", "asset"];
            if (allowedTypes.indexOf(entity.type) === -1) return;
            var state = xState.HIDDEN;
            if (args.checked) {
                state = xState.UNDEFINED; //UNSTYLED
            }
            var id = parseInt(entity.id);
            if (id && viewer) {
                //viewer.resetStates();
                //viewer.renderingMode = "x-ray";
                if (entity.type === "assettype" || entity.type === "space") {
                    var ids = [];
                    for (var i = 0; i < entity.children.length; i++) {
                        id = parseInt(entity.children[i].id);
                        ids.push(id);
                    }
                    viewer.setState(state, ids);
                }
                else if (entity.type === "floor") {
                    var ids0 = [];
                    for (var j = 0; j < entity.children.length; j++) {
                        for (var jj = 0; jj < entity.children[j].children.length; jj++) {
                            var id0 = parseInt(entity.children[j].children[jj].id);
                            ids0.push(id0);
                        }
                    }
                    viewer.setState(state, ids0);
                }
                else {
                    viewer.setState(state, [id]);
                }
                //viewer.zoomTo(id);
                //keepTarget = true;
            }
        });
    }
    //if (!viewer) loadControls();

    var keepTarget = true;//false;
    //if (!browser) {
        var browser = new xBrowser();
        browserOn(browser);
    //}
    //viewer set up
    var check = xViewer.check();
    if (check.noErrors) {
        //alert('WebGL support is OK');
        var needLoad = !viewer;
        if (needLoad) {
            viewer = new xViewer("viewer-canvas");
            viewer.background = [249, 249, 249, 255];
            viewer.on("mouseDown", function (args) {
                if (!keepTarget && args.id) viewer.setCameraTarget(args.id);
                //if (args) {
                //    viewer.resetStates();
                //    $("#btnworkflow").data("id", "");
                //}
            });
            //viewer.on("pick", function (args) {
            //    if (args && args.id) {
            //}
            //});
            viewer.on("dblclick", function (args) {
                var id = args.id;
                if (id) {
                    viewer.resetStates(undefined, true);
                    viewer.renderingMode = "x-ray";
                    viewer.setState(xState.HIGHLIGHTED, [id]);
                    viewer.zoomTo(id);
                    keepTarget = true;
                } else {
                    $("#btnLocate").data("id", undefined);
                    $("#btnworkflow").data("id", undefined);
                    //console.log("dblclick");
                    //viewer.renderingMode = "normal";
                    keepTarget = false;
                }

            });
            viewer.on("loaded", function (args) {
                var modelId = args.id;
                //alert(nodeId + ":" + modelId);
                if (nodeId) $("#" + nodeId).data("modelId", modelId);
                if (currentEntityId && ifcId) {
                    var id = currentEntityId;
                    //returnHome(viewer);
                    //viewer.resetStates(undefined, true);
                    setTimeout(function () {
                        if (!isQuestionCallBack) {
                            viewer.renderingMode = "x-ray";
                            viewer.setState(xState.HIGHLIGHTED, [id]);
                            viewer.zoomTo(id);
                        }
                        keepTarget = true;
                    }, 500);
                    ifcId = null;
                }
                if (currentEntityIds && currentEntityIds.length > 0 && ifcIds) {
                    id = currentEntityIds[0];
                    setTimeout(function () {
                        if (!isQuestionCallBack) {
                            viewer.renderingMode = "x-ray";
                            viewer.setState(xState.HIGHLIGHTED, currentEntityIds);
                            viewer.zoomTo(id);
                        }
                        keepTarget = true;
                    }, 500);
                    ifcIds = null;
                }
            });
        }
        if (modelName) {
            viewer.load("/" + modelName + ".wexbim");
            browser.load("/" + modelName + ".json");
            //xViewerHost = viewer;
            //xBrowserHost = browser;
        }
        if (needLoad) {
            viewer.start();
            if (!isMobileVar) {
             var cube = new xNavigationCube();
            viewer.addPlugin(cube);
            }
            var home = new xNavigationHome();
            viewer.addPlugin(home);
         
        } else {
            //viewer.draw();
        }
    }
    else {
        alert("WebGL support is unsufficient");
        /*
        var msg = document.getElementById("msg");
        msg.innerHTML = "";
        for (var i in check.errors) {
            if (check.errors.hasOwnProperty(i)) {
                var error = check.errors[i];
                msg.innerHTML += "<div style='color: red;'>" + error + "</div>";
            }
        }
        */
    }
    return viewer;
}