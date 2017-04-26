//模型浏览界面-触发事件：改变布局，响应交互

//滚动条
var pre_scrollTop = 0;  // 滚动条事件之前文档滚动高度
var obj_topic;
var obj_tableWrap;

//显示搜索结果
function xbim_showSearchResult() {
    $('#m-tree-view').css("display", "none");
    $('#tableWrap').css("display", "inline-block");
    tableshow = true;
    //for (var i in guid_file_asstypesLibrary) {
    //    alert(guid_file_asstypesLibrary[i].children.length);//guid_file_asstypesLibrary[i].name + "\n" +
    //}
    //obj_topic = document.getElementById("topic");
    //obj_tableWrap = document.getElementById("tableWrap");
    //pre_scrollTop = obj_tableWrap.scrollTop;
 
    //obj_tableWrap.onscroll = function () {
    //    //if ( obj_tableWrap.scrollTop===pre_scrollTop) {
    //    //    obj_topic.style.display = "inline-block";
    //    //} else {
    //    //    obj_topic.display = "inline-block";
    //    //}
    //    //////if (pre_scrollTop != obj_topic.style.top) {
    //    //obj_topic.style.width = "120%";
    //    //obj_topic.style.top = 30 + pre_scrollTop + "px";
    //    //////}
    //}
}
 

//模型区宽度
var cwd = $(window).width();
var chd = $(window).height();
//面板重新布局
function resizeViewSize() {
    //alert(treeshow + "_" + propshow);
    cwd = $(window).width() > $(window).height() ? $(window).width() : $(window).height();
    chd = $(window).width() < $(window).height() ? $(window).width() : $(window).height();
 
    var cwdl = $('#semantic-model-container').width();
    var cwdr = $('#semantic-descriptive-info-container').width();
    var tooltop = $('#viewer-container-toolbar').width();
    var toolright = $('#viewer-container-toolbar-zoom').width();
    if (cwdl < 240 || cwdr < 240) {
        $('#semantic-model-container').width(240);
        $('#semantic-descriptive-info-container').width(240);
         cwdl = 240;
         cwdr = 240;
    }
    //标题
    if (!isMobileVar) {//PC
        $('#top1').css("display", "inline-block");
        $('#top1').css("left", 0.1* cwd);
        //$('#content').css("top", 0);
        //$('#content').height(chd);
    } else {//PAD
        $('#top1').css("display", "none");
        $('#content').css("top",0);
        $('#content').height(1.02*chd);
    }
    //左中右
    if (treeshow) {
        if (propshow) { //最小尺寸          
            if (!isMobileVar) {//PC
                //alert("PC1");
                //显示
                $('#semantic-model-container').css("display", "inline-block");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "inline-block");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", cwdl);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl) + 2);
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd - cwdl - cwdr));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl - cwdr) / 1.5);
                if (((cwd - cwdl - cwdr) / 5) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                } 
            } else {//PAD
                //alert("PAD1");
                //显示
                $('#semantic-model-container').css("display", "inline-block");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "none");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", cwdl);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl));
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd - cwdl));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl) / 1.5);
                if(((cwd - cwdl - cwdr) / 5)<50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
            }
        }
        else {
            //右对齐
            if (!isMobileVar) {//PC
                //alert("PC2");
                //显示
                $('#semantic-model-container').css("display", "inline-block");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "none");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", cwdl);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl));
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd - cwdl ));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl - cwdr) / 1);
                if (((cwd - cwdl - cwdr) / 5) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
            } else {//PAD
                //alert("PAD2");
                //显示
                $('#semantic-model-container').css("display", "inline-block");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "none");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", cwdl);
                $('#semantic-descriptive-info-container').css("left", cwdl);
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd - cwdl));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl) / 1);
                if (((cwd - cwdl - cwdr) / 5) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
                //alert("L:" + cwdl);
                //var pad = $('#viewer-container');
                //alert("M:" + pad[0].style.left);
            }

            //$('#viewer-container-toolbar-zoom').css("left", ((cwd - cwdl) - 1.5 * (toolright)));
            //$('div#viewer-container-toolbar-over-left').css("left", cwdl / 0.97 + 5);
            //$('div#viewer-container-toolbar-over-right').css("right", 5);
        }
    } else {
        if (propshow) {//左对齐
            if (!isMobileVar) { //PC
                //alert("PC3");
                //显示
                $('#semantic-model-container').css("display", "none");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "inline-block");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", 0);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl) + 2);
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd -  cwdr));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl - cwdr) / 1);
                if (((cwd - cwdl - cwdr) / 5) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
            } else {//PAD
                //alert("PAD3");
                //显示
                $('#semantic-model-container').css("display", "none");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "inline-block");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", cwdl);
                $('#semantic-descriptive-info-container').css("left", 0);
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd - cwdl));
                $('#semantic-descriptive-info-container').width(cwdl);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 5));
                $('#viewer-container-toolbar').width((cwd - cwdl ) / 1);
                if (((cwd - cwdl - cwdr) / 5) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
            }
  
            //if (isMobileVar) $('#viewer-container-toolbar-zoom').css("left", ((cwd - cwdl) - 1.5 * (toolright)));
            //$('div#viewer-container-toolbar-over-left').css("left", 5);
            //$('div#viewer-container-toolbar-over-right').css("right", cwdr / 0.97 + 5);
        }
        else {  //最大尺寸
            if (!isMobileVar) {//PC
                //alert("PC_MAX");
                //显示
                $('#semantic-model-container').css("display", "none");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "none");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", 0);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl));
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 3));
                $('#viewer-container-toolbar').width((cwd - cwdl - cwdr) / 1);
            } else {//PAD
                //alert("PAD_MAX");
                //显示
                $('#semantic-model-container').css("display", "none");
                $('#viewer-container').css("display", "inline-block");
                $('#semantic-descriptive-info-container').css("display", "none");
                //位置
                $('#semantic-model-container').css("left", 0);
                $('#viewer-container').css("left", 0);
                $('#semantic-descriptive-info-container').css("left", (cwd - cwdl));
                //大小
                $('#semantic-model-container').width(cwdl);
                $('#viewer-container').width((cwd));
                $('#semantic-descriptive-info-container').width(cwdr);
                //插件
                $('#viewer-container-toolbar').css("left", ((cwd - cwdl - cwdr) / 3));
                $('#viewer-container-toolbar').width((cwd - 80 ) / 1);
                if (((cwd - cwdl - cwdr) /3) < 50) {
                    $('#viewer-container-toolbar').css("left", 50);
                }
            }
            //if (isMobileVar) $('#viewer-container-toolbar-zoom').css("left", (cwd - 1.5 * (toolright)));
            //$('div#viewer-container-toolbar-over-left').css("left", 5);
            //$('div#viewer-container-toolbar-over-right').css("right", 5);
        }
        
    }
 
    //$(window).resize(function () {
    //    resizeViewSize();
    //});


    if (isMobileVar) {//PAD
 
        var bnts = $('#viewer-container-toolbar input');
        var selects = $('#viewer-container-toolbar select');
        if (cwd < 1000) {  //小屏幕        
            for (var i = 0; i < bnts.length; i++) {
                if (bnts[i].id === "view-toolbar-bnt3") {
                    bnts[i].style.cssText = '\
            margin-left: 4px;\
            width: 79px;\
            height: 28px;\
            font-size: 10px;\
            vertical-align:middle;\
            text-align:center;';
                } else {
                    bnts[i].style.cssText = '\
            margin-left: 4px;\
            width: 46px;\
            height: 28px;\
            font-size: 10px;\
            vertical-align:middle;\
            text-align:center;';
                }
            }           
            for (var j = 0; j < selects.length; j++) {
                selects[j].style.cssText = '\
            margin-left: 4px;\
            width: 59px;\
            height: 30px;\
            font-size: 10px;\
            vertical-align: middle;\
            cursor: pointer;\
            outline: none;';
            }
        } else {//大屏幕
            for (var m = 0; m < bnts.length; m++) {
                bnts[m].style.cssText = '\
            width: 80px;\
            height: 36px;\
            font-size: 16px;\
            vertical-align:middle;\
            text-align:center;';
            }
            for (var n = 0; n < selects.length; n++) {
                selects[n].style.cssText = '\
            width: 65px;\
            height: 38px;\
            font-size: 16px;\
            vertical-align: middle;\
            cursor: pointer;\
            outline: none;';
            }
        }
        $('#viewer-container-toolbar').css("left", 40);
        if (!treeshow && !propshow) {
            $('#viewer-container-toolbar').width((cwd - 80) / 1);
        } else {
            $('#viewer-container-toolbar').width(0.70*(cwd - cwdl - 40) / 1);
        }

        //$('div#viewer-container-toolbar').css("left", 50);
        //$('div#viewer-container-toolbar').width(1.2 * (5 * buttonwidth * 0.5 + 3 * buttonwidth + 2 * buttonwidth * 0.8));
    }

}

var treeshow = true;
var propshow = true;
var questionshow = false;
var tableshow = false;
//属性/隐藏属性切换
function xbim_questionshow() {
    if (questionshow) {
        $('.model-tree').css("display", "none");
        $('.model-question').css("display", "inline-block");
        if (!propshow) $('#semantic-descriptive-info-container').css("display", "none");
    } else {
        $('.model-question').css("display", "none");
        if (propshow) $('#semantic-descriptive-info-container').css("display", "inline-block");
    }
}
function xbim_treeshow() {
    if (treeshow) {
        $('.model-tree').css("display", "inline-block");
        //$('#searchWarp').css("display", "inline-block");
        //$('#m-tree-view').css("display", "inline-block");
    } else {
        $('.model-tree').css("display", "none");
        //$('#searchWarp').css("display", "inline-block");
        //$('#m-tree-view').css("display", "inline-block");
    }
}
function xbim_tableshow() {
    if (tableshow) {
        $('#m-tree-view').css("display", "none");
        $('#tableWrap').css("display", "inline-block");
        $('#searchWarp').css("display", "inline-block");   
    } else {
        $('#searchWarp').css("display", "inline-block");
        $('#m-tree-view').css("display", "inline-block");
        $('#tableWrap').css("display", "none");       
    }
}
function xbim_propshow() {
    if (propshow) {
        $('#semantic-descriptive-info-container').css("display", "inline-block");
    } else {
        $('#semantic-descriptive-info-container').css("display", "none");
    }
}

function xbim_showcontainer(value) {
    treeshow = true;
    tableshow = false;
    questionshow = false;

    if (isMobileVar) {
        if (!questionshow && !tableshow && !propshow) {
            treeshow = false;
            propshow = true;
        } else {
            treeshow = true;
            propshow = false;
        }
    }

    if (isMobileVar) {
        if (value === "结构树") {
            treeshow = true;
            propshow = false;
        } else if (value === "属性") {
            propshow = true;
            treeshow = false;
        }
    }

    xbim_treeshow();
    xbim_tableshow();
    xbim_questionshow();

    if (isMobileVar) {
        xbim_propshow();
        resizeViewSize();
    }

}
function xbim_hidecontainer(isLeft,bnt) {
    if (isLeft) xbim_hidecontainerLeft(bnt);
    else xbim_hidecontainerRight(bnt);
}
function xbim_hidecontainerLeft(bnt) {
    if (!isMobileVar) {
        if (treeshow||tableshow||questionshow ) {
            bnt.value = "》";
            treeshow = false;
            tableshow = false;
            questionshow = false;
            
        }
        else if (!treeshow && !tableshow && !questionshow) {
            bnt.value = "《";
            treeshow = true;
            tableshow = false;
            questionshow = false;
        } else {
            bnt.value = "《";
            treeshow = true;
            tableshow = false;
            questionshow = false;
        }
    } else {
        if (treeshow || propshow) {
            bnt.value = "》";
            treeshow = false;
            propshow = false;
        } else {
            bnt.value = "《";
            treeshow = true;
            propshow = false;
            xbim_treeshow();
            xbim_tableshow();
            xbim_questionshow();
            xbim_propshow();
        }

    }    
    resizeViewSize();
}
function xbim_hidecontainerRight(bnt) {
    if (propshow) {
        bnt.value = "《";
        propshow = false;
    } else {
        bnt.value = "》";
        propshow = true;
    }
    resizeViewSize();
}
 
lastid = null;
var multSelec = false;
//多选/单选模式切换
function xbim_multSelectButton(bnt) {
    if (!multSelec) {//单选》多选
        multSelec = true;
        //bnt.value = "单选";
        bnt.children = [];
        bnt.style.background = "#00344f";
        if (!$('#multi-select').checked) {
            $('#multi-select').click();
        }
    } else {//多选》单选
        multSelec = false;
        //bnt.value = "多选";
        $('.entity-visible').prop('checked', true);
        //if (multSels.length!=0) {
        //    multSels = [];
        //}
        //if ($('#multi-select').checked) {
             $('#multi-select').click();
        //}
             bnt.style.background = "#008ad2";
             ids_selected = [];
             xbim_selectedRender();
    }

}
function xbim_multSelectModeChange() {
    xViewerHost.on("pick", function (args) {
        if (!args || !args.id)return;
        var mSel = $('#multi-select').prop("checked");
        var index = ids_selected.indexOf(args.id);
        if (!mSel) { //单选模式
            ids_selected = [args.id];
        } else {//多选模式
            if (index === -1) ids_selected.push(args.id);
        }
        if (index != -1) {
            ids_selected.splice(index, 1);
        }
        var currentbrower = xbim_search_GetBrower(args.id, guid_file_Nodes);
        if (currentbrower.length!=0) {
            currentbrower[0].activateEntity(args.id);
        }
        else{
            xBrowserHost.activateEntity(args.id);
        }

        xbim_selectedRender();
    });
}
function xbim_selectedRender() {
    viewer.resetStates(undefined, true);
    if (ids_selected.length > 0) {
        xViewerHost.renderingMode = "x-ray";
    } else {
        xViewerHost.renderingMode = "normal";
    }
    xViewerHost.setState(xState.HIGHLIGHTED, ids_selected);

    var ids = ids_selected;
    if (ids_selected.length > 0) {
        var idStr = ids.join(' ');
        $("#btnworkflow").data("id", idStr);
        $("#btnworkflow").data("type", "asset");
    } else
    {
        $("#btnworkflow").data("id", undefined);
        $("#btnworkflow").data("type", undefined);
    }
}
//隐藏单选/多选构件
function xbim_hideEntity(bnt) {
    ids_hideed = [];
    ids_hideed = ids_hideed.concat(ids_selected);
    xViewerHost.renderingMode = "normal";
    xViewerHost.setState(xState.HIDDEN, ids_hideed);

    ids_selected = [];
    xbim_selectedRender();
}
//重置取消选择
function xbim_reset() {

    var viewer = xViewerHost;

    viewer.renderingMode = "normal";
    viewer.resetStates();
    //viewer.setState(xState.UNSTYLED, ids);
    //keepTarget = false;
    $("#btnworkflow").data("id", "");

    ids_hideed = [];
    ids_selected = [];
    xbim_selectedRender();
}

 
var clicpshow = true;
//裁剪/取消模式切换
function xbim_clipModel(bnt) {

    //重置
    xViewerHost.resetStates(undefined, true);
    xViewerHost.renderingMode = "normal";
    xViewerHost.resetStates();
    xViewerHost.setState(xState.UNSTYLED, multSels);
    xViewerHost.keepTarget = false;
    
    //裁剪
    if (clicpshow) {
        clicpshow = false;
        bnt.style.background = "#00344f";
        xViewerHost.clip();
    } else {
        clicpshow = true;
        xViewerHost.unclip();
        bnt.style.background = "#008ad2";

    }

}
 
//旋转模式选择
function xbim_navigationModel(name) {
    //$('#pan-btn').click(function () {
    //    viewer.navigationMode = 'pan';
    //});
    //$('#rotate-btn').click(function () {
    //    viewer.navigationMode = 'orbit';
    //});
    //$('#zoom-btn').click(function () {
    //    viewer.navigationMode = 'zoom';
    //});
    if (xViewerHost == null)return;
    switch (name) {
        case "旋转":
            xViewerHost.navigationMode = 'orbit';
            break;
        case "平移":
            xViewerHost.navigationMode = 'pan';
            break;
        case "缩放":
            xViewerHost.navigationMode = 'zoom';
            break;
    default:
    }
}

//旋转模式选择
function xbim_viewportModel(name) {
 
    switch (name) {
        case "3D":
            xViewerHost.setCameraPosition(xCameraPostion);
            break;
        case "俯视":
            xViewerHost.show('top');
            break;
        case "底视":
            xViewerHost.show('bottom');
            break;
        case "左视":
            xViewerHost.show('left');
            break;
        case "右视":
            xViewerHost.show('right');
            break;
        case "正视":
            xViewerHost.show('front');
            break;
        case "后视":
            xViewerHost.show('back');
            break;
        default:
            break;
    }
}

//问题反馈选择
function xbim_questionModel(name) {

    switch (name) {
        case "问题反馈":
            xCameraQurstion = xViewerHost.getCameraPosition();
            xbim_workflow();
            break;
        case "问题记录":
            treeshow = true;
            tableshow = false;
            questionshow = true;

            xbim_treeshow();
            xbim_tableshow();
            xbim_questionshow();

            resizeViewSize();
            break;
        default:
            break;
    }
}

//工作流
function xbim_workflow() {
    var idsStr = $("#btnworkflow").button().data("id");
    if (!idsStr) {
        alert("请选择构件！");
        return;
    }

    var entityType = $("#btnworkflow").button().data("type");
    if (entityType && entityType === "asset") {
        var id = idsStr;
        //console.log("godspeed-btnworkflow--:" + id);

        //console.log("ifc:" + ifc);
        $(".projectListBtnConfirm").data("id", id);
     //  
        $.ajax({
            url: "/model/GetFeedbackData",
            data: {
                guid: $('#CurrentVaultGuid').val(),
                userid: $('#AecuserIdInDatabase').val()
            },
            success: function (res) {
                console.log("success" + res);
                $(".projectListBtnConfirm #select-class option").remove();
                var option;
                option = "<option value='ClassIssueFeedback'>问题反馈</option>";
                $(".projectListBtnConfirm #select-class").append(option);
                if (res.search("True,") != -1) {
                    option = "<option value='ClassSecureNoticeDailyCheck'>安全日常检查</option>";
                     $(".projectListBtnConfirm #select-class").append(option);
                     option = "<option value='ClassSecureNoticeWeeklyCheck'>安全周检查</option>";
                     $(".projectListBtnConfirm #select-class").append(option);
                     option = "<option value='ClassSecureNoticeSpecialCheck'>安全专项检查</option>";
                     $(".projectListBtnConfirm #select-class").append(option);
                }
                if (res.search(",True") != -1) {
                     option = "<option value='ClassQualityAdjustmentNotice'>质量整改通知单</option>";
                    $(".projectListBtnConfirm #select-class").append(option);
                }
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                console.log("获取失败：(status:" + xmlHttpRequest.status + ", readyState:" + xmlHttpRequest.readyState +
                        ", textStatus:" + textStatus + ", errorThrown: " + errorThrown + ")");
            }
        });
        $(".projectListBtnConfirm").show();
    }
}

//定位搜索结果
function xbim_zoomtoentity(tr) {

    var guid = tr.title;
    var id = tr.id;
    var viewer = xbim_currentViewer(guid);
    var browers = xbim_currentBrowers(guid);
    //alert(parseInt(id));
    if (viewer && browers) {
        viewer.zoomTo(parseInt(id));
        viewer.resetStates(undefined, true);
        viewer.renderingMode = "x-ray";
        viewer.setState(xState.HIGHLIGHTED, [parseInt(id)]);//parseInt(id)//479038
        viewer.zoomTo(parseInt(id));
        viewer.keepTarget = true;
        browers.activateEntity(parseInt(id));
        $("#btnLocate").data("id", parseInt(id));

        //viewer.setCameraTarget(parseInt(id));
        //alert(parseInt('481188'));
    }

}

//摄像头复位
function xbim_resetCamera() {
    //var mat4 = xViewerHost.mat4;
    //var origin = xViewerHost._origin;
    //xViewerHost.setCameraTarget(origin);
    xViewerHost.setCameraPosition(xCameraPostion);

}
