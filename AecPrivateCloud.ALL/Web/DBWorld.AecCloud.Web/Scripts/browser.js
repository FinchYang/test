$(document).ready(function() {
    var data = getModelData();
    if (!data) {
        alert('模型不存在或正在转换中...');
        return;
    }
    $.getJSON("/Model/ModelPath?Guid=" + data.Guid + "&TypeId=" + data.TypeId + "&ObjId=" + data.ObjId, function (result) {
        if (!isValidModelpath(result)) {
            alert(result);
        }
        viewer = loadModel(result, data.ifcGuid);
        if (viewer) {
            $('#clip-btn').click(function() {
                viewer.clip();
            });
            $('#unclip-btn').click(function() {
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
    });

    $('#multi-select').change(function() {
        var mSel = $(this).prop("checked");
        if (!mSel) {
            var idsStr = $("#btnworkflow").data("id");
            if (idsStr) {
                var ids = idsStr.split(' ');
                $("#btnworkflow").data("id", ids[ids.length-1]);
            }
            if (multSels) {
                multSels = [];
            }
        }
    });
});

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
    var guids = getQuerystring("guid");
    if (guids.length === 0) return;
    var typeIds = getQuerystring("typeid");
    if (typeIds.length === 0) return;
    var ids = getQuerystring("objid");
    if (ids.length === 0) return;
    var data = {
        Guid: guids[0],
        TypeId: parseInt(typeIds[0]),
        ObjId: parseInt(ids[0])
    };
    var ifcGuid = getQuerystring("ifcguid");
    if (ifcGuid.length > 0) {
        data.ifcGuid = ifcGuid[0];
    }
    return data;
}

function getQuerystring(key) {
    var re = new RegExp('(?:\\?|&)' + key + '=(.*?)(?=&|$)', 'gi');
    var r = [], m;
    while ((m = re.exec(document.location.search)) != null) r.push(m[1]);
    return r;
}

function loadModel(modelName, ifcGuid) {
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
            //console.log("godspeed-btnLocate--:" + id);
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
                var id = $(this).data("id");
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

                var guid11 = getQuerystring("guid");
                var prefix = document.getElementById("homeurl").value;

                var ifc = [];
                if (idMapping && id) {
                    var idsStr = id.toString().split(' ');
                    for (var ii = 0; ii < idsStr.length; ii++) {
                        var entId = idsStr[ii];
                        for (var kk in idMapping) {
                            if (idMapping.hasOwnProperty(kk)) {
                                if (idMapping[kk] == entId) {
                                    ifc.push(kk);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (ifc.length === 0) {
                    alert("未选择构件！");
                    return;
                }

                var classAlias = $('#select-class').val();
                
                var apiUrl = prefix + "Model/WorkFlow?vaultGuid=" + guid11 + "&classAlias=" + classAlias + "&id=" + ifc.join(',');
                $.ajax({
                    url: apiUrl,
                    async: true,
                    success: function(data) {
                        //console.log("success 3:" + data.substring(3));
                        window.location.href = data.substring(3);
                    },
                    error: function(xmlHttpRequest, textStatus, errorThrown) {
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

    var keepTarget = false;
    browser = new xBrowser();
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
        $("#structure > ul > li").click();

        if (!idMapping) {
            idMapping = args.model.guidMapping;
            if (idMapping && ifcGuid) {
                currentEntityId = idMapping[ifcGuid];
                if (currentEntityId) {
                    browser.activateEntity(currentEntityId);
                    $("#btnLocate").data("id", currentEntityId);
                    $("#btnworkflow").data("id", currentEntityId);
                    //console.log("idMapping");
                    keepTarget = true;
                }
            }
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
        //console.log("entityActive");
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


    //viewer set up
    var check = xViewer.check();
    if (check.noErrors) {
        //alert('WebGL support is OK');
        viewer = new xViewer("viewer-canvas");
        viewer.background = [249, 249, 249, 255];
        viewer.on("mouseDown", function (args) {
            if (!keepTarget && args.id) viewer.setCameraTarget(args.id);
            
        });
        viewer.on("pick", function (args) {
            if (args && args.id) {
                viewer.resetStates(undefined, true);			
                browser.activateEntity(args.id);


                var mSel = $('#multi-select').prop("checked");

                var ids = [args.id];

                $("#btnLocate").data("id", args.id);
                $("#btnLocate").data("type", "asset");
                if (!mSel) {
                    $("#btnworkflow").data("id", args.id);
                } else {
                    var has = false;
                    for (var ii = 0; ii < multSels.length; ii++) {
                        if (multSels[ii] == args.id) {
                            has = true;
                        }
                    }
                    if (!has) {
                        multSels.push(args.id);
                    }
                    var idStr = multSels.join(' ');
                    $("#btnworkflow").data("id", idStr);
                    ids = multSels;
                }
                
                $("#btnworkflow").data("type", "asset");
                //console.log("pick");
              
                viewer.renderingMode = "x-ray";
                viewer.setState(xState.HIGHLIGHTED, ids);
                keepTarget = true;
            } else {
                $("#btnLocate").data("id", undefined);
                $("#btnLocate").data("type", undefined);
                $("#btnworkflow").data("id", undefined);
                $("#btnworkflow").data("type", undefined);
                viewer.renderingMode = "normal";
                //console.log("not pick");
                keepTarget = false;
            }
        });
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
                viewer.renderingMode = "normal";
                keepTarget = false;
            }
            
        });

        viewer.on("loaded", function (args) { //args:  { id: handle.id, tag: tag }
            if (currentEntityId) {
                var id = currentEntityId;
                //returnHome(viewer);
                //viewer.resetStates(undefined, true);
                setTimeout(function () {
                    viewer.renderingMode = "x-ray";
                    viewer.setState(xState.HIGHLIGHTED, [id]);
                    viewer.zoomTo(id);
                    keepTarget = true;
                }, 500);
            }
        });

        viewer.load("/" + modelName + ".wexbim", modelName); //, tag
        browser.load("/" + modelName + ".json");
        

        //var cube = new xNavigationCube();
        //viewer.addPlugin(cube);

        viewer.start();

        var cube = new xNavigationCube();
        viewer.addPlugin(cube);

        var home = new xNavigationHome();
        viewer.addPlugin(home);
    }
    else {
        alert("WebGL support is unsufficient");
        var msg = document.getElementById("msg");
        msg.innerHTML = "";
        for (var i in check.errors) {
            if (check.errors.hasOwnProperty(i)) {
                var error = check.errors[i];
                msg.innerHTML += "<div style='color: red;'>" + error + "</div>";
            }
        }
    }
    return viewer;
}