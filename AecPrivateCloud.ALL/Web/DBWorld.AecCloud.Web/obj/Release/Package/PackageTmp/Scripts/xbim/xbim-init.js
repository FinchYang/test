//模型浏览界面-初始化函数
$(document).ready(function () {

    if (!isLoadViewerHost) {

        isMobileVar = browserRedirect();
        //isMobileVar = true;//brow/serRedirect(); 这里改下

        //加载必要控件
        layoutload();
        //初始化控件布局
        layoutinit();

        //移动端
        if (isMobileVar) propshow = false;

        //初始化界面布局
        resizeViewSize();

        isLoadViewerHost = true;
    }
});

//判断设备是否移动端打开
function browserRedirect() {
    var sUserAgent = navigator.userAgent.toLowerCase();
    var bIsIpad = sUserAgent.match(/ipad/i) == "ipad";
    var bIsIphoneOs = sUserAgent.match(/iphone os/i) == "iphone os";
    var bIsMidp = sUserAgent.match(/midp/i) == "midp";
    var bIsUc7 = sUserAgent.match(/rv:1.2.3.4/i) == "rv:1.2.3.4";
    var bIsUc = sUserAgent.match(/ucweb/i) == "ucweb";
    var bIsAndroid = sUserAgent.match(/android/i) == "android";
    var bIsCE = sUserAgent.match(/windows ce/i) == "windows ce";
    var bIsWM = sUserAgent.match(/windows mobile/i) == "windows mobile";
    //document.writeln("您的浏览设备为：");
    if (bIsIpad || bIsIphoneOs || bIsMidp || bIsUc7 || bIsUc || bIsAndroid || bIsCE || bIsWM) {
        return true; //document.writeln("phone");
    } else {
        return false; //document.writeln("pc");
    }
}

//PC&移动端加载初始控件
function layoutload() {
    //顶部LOGO（固定）
 
    //左面板
    //中面板（模型）
    if (!isMobileVar) {
        createviewdivPc();
    } else {
        createviewdivPad();
    }
    //右面板（固定）

}
 
//PC端/移动端界面布局
function layoutinit() {
    //中面板
    var viewercontainerPc = document.getElementById("viewer-container");
    //var viewercontainerPad = document.getElementById("viewer-container-pad");
    //左面板
    //var modelpagePc = document.getElementById("modelpage");
    //var modelpagePad = document.getElementById("modelpage-pad");
    //有面板
    //var semanticdescriptiveinfocontainer = document.getElementById("semantic-descriptive-info-container");
    if (!isMobileVar) {
        layoutintPc();
    }
    else {
        layoutintPad();
    }

      function layoutintPc() {
          //viewercontainerPc.style.display = 'block';
          //viewercontainerPad.style.display = 'none';
          //modelpagePc.style.display = 'block';
          //modelpagePad.style.display = 'none';
          //semanticdescriptiveinfocontainer.style.display = 'none';
      };
      function layoutintPad() {
          //viewercontainerPc.style.display = 'none';
          //viewercontainerPad.style.display = 'block';
          //modelpagePc.style.display = 'none';
          //modelpagePad.style.display = 'none';
          //semanticdescriptiveinfocontainer.style.display = 'none';
      }
}

//动态添加界面
function createviewdivPc() {
    var content = document.getElementById("content");
    //var objDiv = document.createElement("div");//创建div
    //objDiv.setAttribute("id", "viewer-container");
    var objDiv = document.getElementById("viewer-container");
    objDiv.style.cssText = 'Content/xbim/xbrowser-styles.css';
    //显示区
    createviewdivoverbar(objDiv);
    createviewdivtoolbar(objDiv);
    //createviewcanvas(objDiv);
    content.appendChild(objDiv);
}
function createviewdivPad() {
    var content = document.getElementById("content");
    //var objDiv = document.createElement("div");//创建div
    //objDiv.setAttribute("id", "viewer-container-pad");
    var objDiv = document.getElementById("viewer-container");
    objDiv.style.cssText = 'Content/xbim/xbrowser-styles.css';
    objDiv.style.position = "absolute";
    //显示区
    createviewdivoverbar(objDiv);
    createviewdivtoolbar(objDiv);
    //createviewdivtoolbarZoom(objDiv);
    //createviewcanvas(objDiv);
    content.appendChild(objDiv);
}
//动态添加子界面
function createviewdivtoolbar(parent) {
    var toolbar = document.createElement("div");//创建div
    toolbar.setAttribute("id", "viewer-container-toolbar");
    //toolbar.setAttribute("class", "ui-widget-header ui-corner-all");
    toolbar.style.cssText = 'Scripts/xbim/xbim-index.css';
    //添加按钮
    createviewdivtoobarButton(toolbar);
    parent.appendChild(toolbar);
    //<!--<div style="position: absolute; left: 70px; top: 100px; padding: 5px;width:90%;height: 35px;background-color:whitesmoke;color:black;" id="toolbar" class="ui-corner-all">
}
function createviewdivoverbar(parent) {
 
    var toolbarleft = document.createElement("div");//创建div
    toolbarleft.setAttribute("id", "viewer-container-toolbar-over-left");
    toolbarleft.setAttribute("class", "ui-widget-header ui-corner-all");
    toolbarleft.style.cssText = 'Scripts/xbim/xbim-index.css';

    var over = document.createElement('input');
    over.setAttribute("id", "view-toolbar-bnt-over-left");
    over.setAttribute("class", " ui-corner-all");
    over.setAttribute("title", "收起左边栏");
    over.type = 'button';
    over.value = "《";
    over.onclick = function () { xbim_hidecontainer(true, over); }//"Url.Action('Views/Model/Index.cshtml')";
    over.style.cssText = 'Scripts/xbim/xbim-index.css';
    toolbarleft.appendChild(over);
    parent.appendChild(toolbarleft);

    if (isMobileVar) return;
 
    var toolbarright = document.createElement("div");//创建div
    toolbarright.setAttribute("id", "viewer-container-toolbar-over-right");
    toolbarright.setAttribute("class", "ui-widget-header ui-corner-all");
    toolbarright.style.cssText = 'Scripts/xbim/xbim-index.css';

    var span = document.createElement('input');
    span.setAttribute("id", "view-toolbar-bnt-over-right");
    span.setAttribute("class", " ui-corner-all");
    span.setAttribute("title", "收起右边栏");
    span.type = 'button';
    span.value = "》";
    span.onclick = function () { xbim_hidecontainer(false, span); }//"Url.Action('Views/Model/Index.cshtml')";
    span.style.cssText = 'Scripts/xbim/xbim-index.css';
    toolbarright.appendChild(span);
    parent.appendChild(toolbarright);
    //<!--<div style="position: absolute; left: 70px; top: 100px; padding: 5px;width:90%;height: 35px;background-color:whitesmoke;color:black;" id="toolbar" class="ui-corner-all">
}

function createviewcanvas(parent) {
    var objCanvasViewer = document.createElement('canvas');
    objCanvasViewer.setAttribute("id", "viewer-canvas");
    objCanvasViewer.style.marginTop = "5px";
    parent.appendChild(objCanvasViewer);
}
//function createviewdivtoolbarPcTest(parent) {

//   // <div style="position: absolute; right: 20px; top: 5px; padding: 8px 30px 8px 30px;" id="toolbar" class="ui-widget-header ui-corner-all">
//   //    <input type="radio" id="pan-btn" value="Pan" name="action1" /><span class="radio-text">平移</span>
//   //    <input type="radio" id="rotate-btn" value="Rotate" name="action1" checked /><span class="radio-text">旋转</span>
//   //    <input type="radio" id="zoom-btn" value="Zoom" name="action1" /><span class="radio-text">缩放</span>
//   //    <input type="checkbox" id="multi-select" value="MultiSelect" name="action1" /><span class="radio-text">多选</span>
//   //</div>
//   //<div style="position: absolute; right: 20px; top: 50px; padding: 5px;" id="toolbar" class="ui-widget-header ui-corner-all">
//   //    <button id="clip-btn">裁剪</button>
//   //    <button id="unclip-btn">取消裁剪</button>
//   //    <button id="reset-btn">重置状态</button>
//    //</div>
//    var toolbar = document.createElement("div");//创建div
//    toolbar.setAttribute("id", "toolbar");
//    toolbar.setAttribute("class", "ui-widget-header ui-corner-all");
//    toolbar.style.position = 'absolute';
//    toolbar.style.right = '20px';
//    toolbar.style.top = '5px';
//    toolbar.style.background = 'red';
//    toolbar.style.width = '200px';
//    toolbar.style.height = '50px';
//    //toolbar.style.padding=
//    createviewdivtoolbarPcButtonTest1(toolbar);
//    parent.appendChild(toolbar);
//    toolbar = document.createElement("div");//创建div
//    toolbar.setAttribute("id", "toolbar");
//    toolbar.setAttribute("class", "ui-widget-header ui-corner-all");
//    toolbar.style.position = 'absolute';
//    toolbar.style.right = '20px';
//    toolbar.style.top = '55px';
//    toolbar.style.background = 'red';
//    toolbar.style.width = '200px';
//    toolbar.style.height = '50px';
//    //toolbar.style.padding=
//    createviewdivtoolbarPcButtonTest1(toolbar);
//    parent.appendChild(toolbar);

//}
//function createviewdivtoolbarPcButtonTest1(parent) {
 
//    var button = document.createElement("button");//创建div
//    button.setAttribute("id", "clip-btn");
//    button.setAttribute("value", "裁剪");
//    button.style.position = 'absolute';
//    button.style.right = '20px';
//    button.style.top = '5px';
//    button.style.background = 'green';
//    button.style.width = '40px';
//    button.style.height = '40px';
//    //toolbar.style.padding=
//    button.onclick = function() { alert('裁剪'); };
//    parent.appendChild(button);
//}

//动态添加选项
function appendSelectOptions(parent, options,event) {
    for (var i = 0; i < options.length; i++) {
        parent.options[i] = new Option(options[i], options[i]);
        //parent.options[i].id = 'view-toolbar-select-option';
        
    }
    //<option value="漫游">漫游</option>
    //<option value="旋转">旋转</option>
    //<option value="平移">平移</option>
    //<option value="缩放">缩放</option>         
}
function createviewdivtoobarButton(parent) {

    var homebnt = document.createElement('input');
    homebnt.setAttribute("id", "view-toolbar-bnt1");
    homebnt.type = 'button';
    homebnt.value = "首页";
    homebnt.setAttribute("title", "返回首页");
    homebnt.onclick = function () { window.location.href = '/Model'; }//"Url.Action('Views/Model/Index.cshtml')";
    homebnt.style.cssText = 'Scripts/xbim/xbim-index.css';

    //tree
    var tree = document.createElement('input');
    tree.setAttribute("id", "view-toolbar-bnt2");
    tree.setAttribute("title", "显示目录树");
    tree.type = 'button';
    tree.value = "结构树";
    tree.style.cssText = 'Scripts/xbim/xbim-index.css';
    
    //prop
    var proptry = document.createElement('input');
    proptry.setAttribute("id", "view-toolbar-bnt1");
    proptry.setAttribute("title", "显示构件属性");
    proptry.type = 'button';
    proptry.value = "属性";
    proptry.style.cssText = 'Scripts/xbim/xbim-index.css';

    //var reset
    var reset = document.createElement('input');
    reset.setAttribute("id", "view-toolbar-bnt1");
    reset.setAttribute("title", "默认视图");
    reset.type = 'button';
    reset.value = "重置";
    reset.onclick = function () { xbim_reset(); }
    reset.style.cssText = 'Scripts/xbim/xbim-index.css';
 
    //var multselect = document.createElement('button');
    var multselect = document.createElement('input');
    multselect.setAttribute("id", "view-toolbar-bnt1");
    multselect.setAttribute("title", "切换多选模式");
    multselect.type = 'button';
    multselect.value = "多选";
    multselect.onclick = function () { xbim_multSelectButton(multselect); }
    multselect.style.cssText = 'Scripts/xbim/xbim-index.css';
 
    //var rotate = document.createElement('button');
    var rotate = document.createElement('select');//select
    rotate.setAttribute("id", "view-toolbar-select");
    rotate.setAttribute("title", "切换显示模式");
    rotate.type = 'select';
    //rotate.value = "旋转";
    rotate.onchange = function () {
         xbim_navigationModel(rotate.value);
    }
    rotate.style.cssText = 'Scripts/xbim/xbim-index.css';
    appendSelectOptions(rotate, ["旋转", "平移", "缩放"], "rotate");


    //var clip = document.createElement('button');
    var clip = document.createElement('input');
    clip.setAttribute("id", "view-toolbar-bnt1");
    clip.setAttribute("title", "切换裁剪模式");
    clip.type = 'button';
    clip.value = "裁剪";
    clip.onclick = function () { xbim_clipModel(clip); }
    clip.style.cssText = 'Scripts/xbim/xbim-index.css';

    //viewport(Pad only)
    //var selfirstView = false;
    //var selindexView = 0;
    if (isMobileVar) {
        //viewport
        var viewport = document.createElement('select');
        viewport.setAttribute("id", "view-toolbar-select");
        viewport.setAttribute("title", "切换视图");
        viewport.type = 'select';
        //viewport.value = "视口";
        //viewport.onchange = function() {
        //     xbim_viewportModel(viewport.value);
        //}
        viewport.onchange = function () {
            xbim_viewportModel(viewport.options[viewport.options.selectedIndex].value);
            viewport.options[0].selected = true;
        }
        viewport.style.cssText = 'Scripts/xbim/xbim-index.css';
        appendSelectOptions(viewport, ["3D","俯视", "正视", "左视", "底视", "右视", "后视"], "viewport");//
    }
    //question.setAttribute("id", "view-toolbar-bnt");
    //var selfirst = false;
    //var selindex = 0;
    //var question = document.createElement('select');
    //question.setAttribute("id", "view-toolbar-select");
    //question.setAttribute("title", "选择问题处理方式");
    //question.type = 'select';
    ////question.value = "视口";
    //question.onchange = function () {
    //    xbim_questionModel(question.options[question.options.selectedIndex].value);
    //    question.options[0].selected = true;
    //}
    //question.style.cssText = 'Scripts/xbim/xbim-index.css';
    //appendSelectOptions(question, ["问题处理","问题反馈", "问题记录"], "question");// 
    var question = document.createElement('input');
    question.setAttribute("id", "view-toolbar-bnt3");
    question.type = 'button';
    question.value = "问题反馈";
    question.onclick = function () { xbim_questionModel(question.value); }
    question.style.cssText = 'Scripts/xbim/xbim-index.css';
    var questionlist = document.createElement('input');
    questionlist.setAttribute("id", "view-toolbar-bnt3");
    questionlist.type = 'button';
    questionlist.value = "问题记录";
    questionlist.onclick = function () { xbim_questionModel(questionlist.value); }
    questionlist.style.cssText = 'Scripts/xbim/xbim-index.css';
    $("#view-toolbar-select").click(function() {
        var value = $("#view-toolbar-select").val();
        alert(value);
    });
 
    var hide = document.createElement('input');
    hide.setAttribute("id", "view-toolbar-bnt1");
    hide.setAttribute("title", "隐藏选中构件");
    hide.type = 'button';
    hide.value = "隐藏";
    hide.onclick = function () { xbim_hideEntity(hide); }
    hide.style.cssText = 'Scripts/xbim/xbim-index.css';
 
    //add button
    //if (isMobileVar) parent.appendChild(homebnt);
    //tree.onclick = function () { xbim_showcontainer("结构树"); };
    //parent.appendChild(tree);
    //if (!isMobileVar) {
    //    parent.appendChild(hide);
    //    parent.appendChild(reset);
    //}
    //parent.appendChild(multselect);
    //parent.appendChild(rotate);
    //parent.appendChild(clip);
    //if (isMobileVar) parent.appendChild(viewport);
    //parent.appendChild(question);
    //parent.appendChild(questionlist);
    //proptry.onclick = function () { xbim_showcontainer("属性"); }
    //if (isMobileVar) parent.appendChild(proptry);

    //if (isMobileVar) {
    //    parent.appendChild(hide);
    //    parent.appendChild(reset);
    //}
    if (!isMobileVar) {
        tree.onclick = function() { xbim_showcontainer("结构树"); };
        parent.appendChild(tree);
        parent.appendChild(hide);
        parent.appendChild(reset);
        parent.appendChild(multselect);
        parent.appendChild(rotate);
        parent.appendChild(clip);
        parent.appendChild(question);
        parent.appendChild(questionlist);
    } else {
        tree.onclick = function () { xbim_showcontainer("结构树"); };
        parent.appendChild(tree);
        parent.appendChild(multselect);
        parent.appendChild(clip);
        parent.appendChild(rotate);
        parent.appendChild(question);

        proptry.onclick = function () { xbim_showcontainer("属性"); }
        parent.appendChild(proptry);
        parent.appendChild(hide);
        parent.appendChild(reset);
        parent.appendChild(viewport);
        parent.appendChild(questionlist);
        
    }

}

//PC 端

//PAD端
function createviewdivtoolbarZoom(parent) {
    var toolbar = document.createElement("div");//创建div
    toolbar.setAttribute("id", "viewer-container-toolbar-zoom");
    //toolbar.setAttribute("class", "ui-widget-header ui-corner-all");
    toolbar.style.cssText = 'Scripts/xbim/xbim-index.css';
    //添加按钮
    createviewdivtoobarPadButtonZoom(toolbar);
    parent.appendChild(toolbar);
    //<!--<div style="position: absolute; left: 70px; top: 100px; padding: 5px;width:90%;height: 35px;background-color:whitesmoke;color:black;" id="toolbar" class="ui-corner-all">
}
function createviewdivtoobarPadButtonZoom(parent) {
    var homebnt = document.createElement('input');
    homebnt.setAttribute("id", "view-toolbar-bnt1");
    homebnt.type = 'button';
    homebnt.value = "+";
    homebnt.onclick = function () {   }//"Url.Action('Views/Model/Index.cshtml')";
    homebnt.style.cssText = 'Scripts/xbim/xbim-index.css';
    parent.appendChild(homebnt);
    //tree
    var tree = document.createElement('input');
    tree.setAttribute("id", "view-toolbar-bnt3");
    tree.type = 'button';
    tree.value = "-";
    tree.style.cssText = 'Scripts/xbim/xbim-index.css';
    tree.onclick = function () {   };
    parent.appendChild(tree);
    //var reset
    var reset = document.createElement('input');
    reset.setAttribute("id", "view-toolbar-bnt1");
    reset.type = 'button';
    reset.value = "重置";
    reset.onclick = function () { xbim_resetCamera(); }
    reset.style.cssText = 'Scripts/xbim/xbim-index.css';
    parent.appendChild(reset);
 
}






