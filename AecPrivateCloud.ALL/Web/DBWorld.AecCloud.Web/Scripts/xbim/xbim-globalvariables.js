//模型浏览界面-全局变量

//文件-数据结构 
guid_file_Nodes = [];

//文件-模型结构
guid_file_asstypesLibrary = [];

//当前选中文档GUID
guid_file_selected = [];

//当前加载的文档GUID
guid_file_loaded = [];

//当前选中构件ID
ids_selected = [];
ids_hideed = [];
isOnLoadSelectEvent = false;

//是否移动端界面
isMobileVar = false;

//是否加载界面
isLoadViewerHost = false;

//主视图控制器
xViewerHost = null;

//缩略图控制器
xViewerMini = null;

//模型区域
xBrowserHost = null;

//缩略图区域
xBrowserMini = null;

//模型库构件ＩＤ
xIdMappingHost = [];

//默认视角
xCameraPostion = [0, 0, 0];

//问题反馈视角
xCameraQurstion = [0, 0, 0];

//问题记录状态监控
xQuestionLoaedState = {
    isQuestionCallback: false,//是否问题反馈触发
    Question: null, //当前问题反馈
    AllModels: [], //目标模型
    LoadedModels: [], //已加载模型
    Progress: 0, //总进度
    LoadedAll: false, //是否加载完毕
    CheckedAll: false, //是否要重新检查模型加载进度
    ModelList: [],//需要加载的模型列表

    //初始化进度，检查模型状态
    init: function (nodes, qa) {
        //空记录
        this.LoadedAll = true;
        this.CheckedAll = true;
        if (!qa) alert("没有选中问题记录");
        if (this.Progress == 0) {//新进度
            this.CheckedAll = true;
            this.LoadedAll = false;
            this.Question = qa;
        }
        else if (this.Progress > 0 && this.Progress < 1) {//进度加载中。。。
            if (this.hasSameQa(qa)) {//同一进度
                this.CheckedAll = false;
                this.LoadedAll = true;
                alert("当前还有未加载模型，请等待加载结束...");
            } else {//开启一个新进度
                this.CheckedAll = false;
                this.LoadedAll = false;
                alert("另一操作正在进行中，请等待操作结束...");
            }
        }
        else if (this.Progress == 1) {//当前加载进度完成
            //alert("加载完成");
            if (this.hasSameQa(qa)) {//同一进度
                this.CheckedAll = false;
                this.LoadedAll = true;
                //alert("相同模型，切换视图...");
            } else {//开启一个新进度
                this.CheckedAll = true;
                this.LoadedAll = true;
                this.Question = qa;
                //alert("不同模型，重新加载...");
            }
        }
    },
    //检查新进度已经追加
    checkLoaded: function (model) {
        //需要加载
        if (this.hasProgress(model, this.AllModels)) {
            if (!this.hasProgress(model, this.LoadedModels)) {//未加载
                this.LoadedModels.push(model);
            }    
        }
        //更新进度
        this.updateProgress();
    },
    //检查新进度需要追加
    checkProgress: function (nodes, vGuid, qa) {
        if (this.CheckedAll) {
            //alert("重新加载模型");
            //已有模型
            this.allProgress(nodes, qa);
            //alert(this.AllModels);
            //删除多余模型
            this.unloadProgress(nodes, qa);
            //未加载模型
            this.toloadProgress(vGuid);

        } else {
            //alert("无需加载模型");
        }
    },
    //需要重新加载的模型
    toloadProgress: function (vGuid) {
        for (var key in this.AllModels) {
            var obj = this.AllModels[key];
            var loadedGuid = xbim_currentGuidByModel(obj.modelId, obj.modelType);
            if (loadedGuid) {
                //alert(obj.modelId + "_已加载");
                this.crtProgress(obj, vGuid, true);
            } else {
                //alert(obj.modelId + "_未加载");
                //追加模型
                //alert(JSON.stringify(obj.modelNode));
                this.addProgress(obj, vGuid, true);
            }
        }
    },
    //需要卸载的模型
    unloadProgress: function (nodes, qa) {
        //需要删除的模型
        var list = [];
        for (var key in guid_file_asstypesLibrary) {
            var obj = guid_file_asstypesLibrary[key];
            if (!this.hasProgress(obj, this.AllModels)) {
                list.push(obj);
            }
        }
        //alert(list);//测试
        //根据modelId删除
        for (var keyUnload in list) {
            var partInfo = list[keyUnload];
            var node = getNodeByModel(nodes, partInfo.modelType, partInfo.modelId);
            var nodeId = node.Level + "_" + node.Type + "_" + node.ID;
            //alert(JSON.stringify(node));
            try {
                removeNodeId(node, guid_file_loaded);
                var guid = partInfo.name.replace('{', '').replace('}', '');
                var model = {};
                if (node.Model) model = node.Model;
                xbim_unload_GetAssType(guid);
                var nodeCk = $('#ck_' + nodeId)[0];
                nodeCk.checked = false;
                //alert(JSON.stringify(modelId));//测试
            }
            catch (err) {
                //在此处理错误
            }

            try {
                //在此运行代码
                var modelId = $("#" + nodeId).data("modelId");
                if (modelId !== undefined) {                  
                    viewer.unload(modelId);                  
                }
            }
            catch (err) {
            }
        }
    },
    //获取当前最新进度
    updateProgress: function () {
        xQuestionLoaedState.isQuestionCallback = true;
        //更新进度
        this.Progress = this.getProgress();
        if (this.Progress > 1) this.Progress = 1;

        if (this.Progress == 0) {
            this.LoadedAll = false;
        }
        else if (0 < this.Progress && this.Progress < 1) {
            this.LoadedAll = false;
        }
        else if (this.Progress >= 1) {
            this.LoadedAll = true;
        }
        //this.LoadedAll = true;
    },
    //是否包含进度
    hasProgress: function(model, list) {
        for (var key in list) {
            if (model.modelId == list[key].modelId && model.modelType == list[key].modelType)return true;
        }
        return false;
    },
    //目标进度
    allProgress: function (nodes, qa) {
        var parts = qa.Components;//构件
        this.ModelList = [];
        for (var key in parts) {
            var partInfo = parts[key];
            //alert(JSON.stringify(partInfo) + "\r\n");//测试
            var modelInfo = {
                modelType: partInfo.ModelType, //模型Type
                modelId: partInfo.ModelID, //模型ID
                modelNode: getNodeByModel(nodes, partInfo.ModelType, partInfo.ModelID),
                ifcIds: [],
                entityIds: [],
            };
            //新模型，添加
            var modelInfoById = this.hasModelInfo(modelInfo, this.ModelList);
            if (!modelInfoById) {
                modelInfo.ifcIds.push(partInfo.IfcId);
                this.ModelList.push(modelInfo);
            } else {//已有模型，追加构件
                modelInfoById.ifcIds.push(partInfo.IfcId);
            }
        }
        if (this.ModelList.length && this.ModelList.length != 0) {
            this.AllModels = [];
            for (var pkey in this.ModelList) {
                this.AllModels.push(this.ModelList[pkey]);
            }
        }
    },
    crtProgress: function (model) {
        var node = model.modelNode;
        nodeId = node.Level + "_" + node.Type + "_" + node.ID;
        var nodeCk = $('#ck_' + nodeId)[0];
        nodeCk.checked = true;
 
        if (this.hasProgress(model, this.AllModels)) {
            if (!this.hasProgress(model, this.LoadedModels)) {//未加载
                this.LoadedModels.push(model);
            }
        }
        //更新进度
        this.updateProgress();
    },
    //增加进度
    addProgress: function (modelInfo, vGuid, isQuestionCallBack) {
        var node = modelInfo.modelNode;
        var nodeGuid = vGuid;
        nodeId = node.Level + "_" + node.Type + "_" + node.ID;
        //$('#ck_' + nodeId).attr('checked', true);
        var nodeCk = $('#ck_' + nodeId)[0];
        nodeCk.checked = true;
        //nodeId = node.Level + "_" + node.Type + "_" + node.ID;
        //alert(nodeId);
        var guid = nodeGuid.replace('{', '').replace('}', '');
        var model = {};
        if (node.Model) model = node.Model;
        var data = {
            Guid: guid,
            TypeId: model.Type,
            ObjId: model.ID
        }
        guid_file_selected = data;
        //if (ifcGuid) data.ifcGuid = ifcGuid;
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
           //从问题反馈加载模型需要设置视点，结构树加载不需要
           xQuestionLoaedState.isQuestionCallback = true;

           var viewer = loadModel(result, data, isQuestionCallBack);

            btnsClickOfViewer(viewer);

        });

        if (!hasNodeId(node, guid_file_loaded)) guid_file_loaded.push(node);
        else {
            //$('.l-wrapper').hide();//加载中
        }
    },
    //获得进度
    getProgress: function () {
        if (this.AllModels.length == 0) return 0;
        return this.LoadedModels.length / this.AllModels.length;
    },
    //获得问题模型信息和IfcId关联
    getModelIdList: function () {
        if (this.LoadedAll) {
            //alert("可以设置视点");
            //视点信息
            var qa = this.Question;
            var viewPortText = qa.ViewPort;//问题视点
            var viewPortArray = [];
            var hideIds = [];
            var hideIdsText = "";
            var modelxViewer = null;
            var modelxViewerText = "";
            viewPortArray = viewPortText.split("^");
            if (viewPortArray.length >=2) {
                viewPortText = viewPortArray[0];
                hideIdsText = viewPortArray[1];
                if(viewPortArray.length >2)modelxViewerText = viewPortArray[2];
                if (viewPortArray[1] != "") {
                    hideIds = JSON.parse(hideIdsText); //隐藏构件
                } else {
                    hideIds = [];
                }
                if (viewPortArray.length > 2) {
                    var guid = modelxViewerText;//隐藏构件
                    modelxViewer = xbim_currentViewer(guid);
                }
            } else if(viewPortArray.length == 1) {
                viewPortText = qa.ViewPort;
                hideIds = [];
                modelxViewer = null;
            }
            var viewPort = JSON.parse(viewPortText);//问题视点坐标
            if (viewPort) xCameraQurstion = viewPort;//切换坐标
            if (!modelxViewer) modelxViewer = xViewerHost;
            //更新视点
            if (modelxViewer) {
                modelxViewer.resetStates(undefined, true);
                if (xCameraQurstion) modelxViewer.setCameraPosition([xCameraQurstion[0], xCameraQurstion[1], xCameraQurstion[2]]);
            }
            //去除遮挡
            if (modelxViewer) {
                if (hideIds.length && hideIds.length != 0) {
                    modelxViewer.renderingMode = "normal";
                    modelxViewer.setState(xState.HIDDEN, hideIds);
                }
            }
            if (modelxViewer) {
                modelxViewer.renderingMode = "x-ray";
                for (var mid in this.ModelList) {
                    var mInfo = this.ModelList[mid];
                    var idMapping = xbim_currentIdMapping("", mInfo.modelId, mInfo.modelType);
                    var entityIds = xbim_getIdsByIdMapping(mInfo.ifcIds, idMapping);
                    //alert(JSON.stringify(entityIds) + "\r\n");//测试
                    modelxViewer.setState(xState.HIGHLIGHTED, entityIds);
                }
            }
        } else {
            //alert("无法设置视点");
        }

        //return;
    },
    //是否相同模型不同问题记录
    hasSameQa: function (nQa) {
    if (this.Progress == 1) {
        if (sameQa(nQa, this.Question)) {
            return true;
        }
        return false;
    }
    return false;

    function sameQa(qa1, qa2) {
        var parts1 = qa1.Components;
        var parts2 = qa2.Components;
        for (var p in parts1) {
            var p1 = parts1[p];
            if (hasModel(p1, parts2))return true;
        }
        return false;
    }
    function hasModel(p1, parts) {
        for (var p in parts) {
            var p2 = parts[p];
            if (p2.ModelID == p1.ModelID && p2.ModelType == p1.ModelType) return true;
        }
        return false;
    }
    },
    //是否已有模型信息
    hasModelInfo: function (model, list) {
        for (var m in list) {
            if (model.modelId == list[m].modelId) return list[m];
        }
        return null;
    }
};