
"use strict";

function OnNewShellUI(shellUI) {
    var globalObject = {};//{"mySoftList": [], "accessToken": "", "cloudUrl": "", "password": ""...}
    shellUI.Events.Register(MFiles.Event.NewShellFrame, newShellFrameHandler(globalObject));
    shellUI.Events.Register(MFiles.Event.NewEmbeddedShellFrame, newEmbeddedShellFrameHandler);
}

function newEmbeddedShellFrameHandler(shellFrame) {
    shellFrame.Events.Register(MFiles.Event.Started, function () {
        setNewInvisible(shellFrame);
    });
}

function setNewInvisible(shellFrame) {
    var location = CommandLocation_TaskPane;
    var state = CommandState_Hidden;
    var cmds = [BuiltinCommand_NewObject, BuiltinCommand_NewAssignment, BuiltinCommand_NewView];
    MF.ui.setCmdsState(shellFrame, location, state, cmds);
}

function newShellFrameHandler(globalObject) {

    return function (shellFrame) {
        shellFrame.Events.Register(MFiles.Event.Started, getShellFrameStartedHandler(shellFrame, globalObject));
    }
}

function getShellFrameStartedHandler(shellFrame, globalObject) {
    // Return the handler function for Started event.
    return function () {
        /// <summary>The "started" event handler implementation for a shell frame.</summary>

        CC.createCmds(shellFrame, globalObject);
        var vault = shellFrame.ShellUI.Vault;
        shellFrame.ActiveListing.Events.Register(Event_SelectionChanged, function (selectedItems) {
            //设置“共享”、“删除文件夹”等按钮的显示状态
            CC.setCmdSateAsSelectionChanged(shellFrame);
            //选中对象时，添加Tab
            CC.addTabAsSelectionChanged(shellFrame);
        });
        //新建的listing时触发
        shellFrame.Events.Register(Event_ActiveListingChanged, function (previousActiveListing, newActiveListing) {

            shellFrame.ActiveListing.Events.Register(Event_SelectionChanged, function (selectedItems) {
                //设置“共享”、“删除文件夹”等按钮的显示状态
                CC.setCmdSateAsSelectionChanged(shellFrame);
            });
        });
        var currentFolders = shellFrame.CurrentFolder;
       
        shellFrame.RightPane.Visible = true;

        var customData;
        var res;
        var array;
        var propId;
        if (shellFrame.CurrentPath === '') {
            //主目录
            shellFrame.RightPane.Visible = false;
            customData = {};
            shellFrame.ShowDashboard("dtaggregation", customData);

        } else if (shellFrame.CurrentPath === "项目概况") {
            //shellFrame.RightPane.Visible = false;
            //var srcParties = [];
            //if (MF.alias.objectType(vault, md.participant.typeAlias) !== -1) {
            //    srcParties = CC.invite.GetAllParties(vault);
            //}
            //var tasks = 0;
            //var emails = 0;
            //var gToken = CC.getToken(vault);
            //projId = CC.getProjectId(vault);
            
            //var project;
            //try {
            //    var projectInfo = webapi.getProject(vault, projId, gToken);
            //    var projObj = eval('('+projectInfo+')');
            //    if (projObj.status !== 200) {
            //        shellFrame.ShellUI.ShowMessage(projObj.response.Message);
            //        return;
            //    }
            //    project = projObj.response;
            //} catch (e) {
            //    shellFrame.ShellUI.ShowMessage(e.message);
            //    return;
            //}
            ////var cover = webapi.getProjectCover(vault, projId, gToken);
            //var cover = project.Cover;
            //// Replace the listing with a dashboard.
            //customData = {
            //    token: gToken,
            //    projId: projId,
            //    cover: cover,
            //    srcParties: srcParties,
            //    tasks: tasks,
            //    emails: emails,
            //    project: project
            //};
            ////if (shellFrame.CurrentPath === "项目概况") {
            ////    customData.objsAndProps = shellFrame.Listing.items.ObjectVersionsAndProperties.Item(1);
            ////} else {
            ////    var objSearch = MF.ObjectOps.SearchObjectsByType(vault, MF.alias.objectType(vault, md.proj.typeAlias));
            ////    if (objSearch.Count > 0) {
            ////        customData.objsAndProps = vault.ObjectOperations.GetLatestObjectVersionAndProperties(objSearch.Item(1).ObjVer.ObjID, false, false);
            ////    }
            ////}
            //shellFrame.ShowDashboard("home", customData);

        } else if (shellFrame.CurrentPath === "策划管理\\设计策划") {
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties };
            shellFrame.ShowDashboard("dtdesignplan", customData);

        } else if (shellFrame.CurrentPath === "指派给我的任务") {//shellFrame.CurrentPath = "我的任务"
            res = [];
            array = md.genericTask.propDefs;
            for (var item in array) {
                if (array[item] === md.genericTask.propDefs.Deadline) {
                    propId = MF.alias.propertyDef(vault, array[item]);
                    res.push(propId);
                }
            }
            publicUtils.SetListingHeader(shellFrame.CurrentFolder, shellFrame, res);
            //CC.SetListingTheme(shellFrame.Listing);

        } else if (shellFrame.CurrentPath === "我指派的任务") {
            res = [];
            array = md.genericTask.propDefs;
            for (var p in array) {
                if (array[p] === md.genericTask.propDefs.Deadline
                    || array[p] === md.genericTask.propDefs.AssignedTo) {
                    propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            }
            publicUtils.SetListingHeader(shellFrame.CurrentFolder, shellFrame, res);
            //CC.SetListingTheme(shellFrame.Listing);

        } else if (shellFrame.CurrentPath === "项目文档\\个人文档") {
            CC.SetListingTheme(shellFrame.Listing);
        } else if (shellFrame.CurrentPath === "项目文档\\工作空间") {
            CC.SetListingTheme(shellFrame.Listing);
        } else if (shellFrame.CurrentPath === "项目报表") {
            //CC.SetListingTheme(shellFrame.Listing);
        } else if (shellFrame.CurrentPath === "众包管理\\众包文档") {
            CC.SetListingTheme(shellFrame.Listing);
        } else if (shellFrame.CurrentPath === "项目任务") {
            var viewId = currentFolders.Item(currentFolders.Count).View;
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, viewId: viewId };
            shellFrame.ShowDashboard("alltasks", customData);

        } else if (shellFrame.CurrentPath === "BIM模型管理\\模型策划") {
            shellFrame.RightPane.Visible = false;
            customData = {};
            shellFrame.ShowDashboard("bimModelPlan", customData);
        } else if (shellFrame.CurrentPath === "BIM模型管理\\模型预览") {

            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties };
            shellFrame.ShowDashboard("bimmodelbrowser", customData);

        } else if (shellFrame.CurrentPath === "BIM模型管理\\BIM模型") {

            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, parentPath: shellFrame.CurrentPath };
            shellFrame.ShowDashboard("bimModelList", customData);

        } else if (shellFrame.CurrentPath === "邮件系统\\收件箱") { //视图路径
            var emailIdF = currentFolders.Item(currentFolders.Count).View; //结构找当前文件夹 ， view 视图id
            var childrenFolders = shellFrame.Listing.items.Folders; //子文件夹 (文件夹 or more)
            var emailIds = [];
            var emailPaths = [];
            for (var i = 1; i <= childrenFolders.Count; i++) {
                var pfId = childrenFolders.Item(i).View; //视图
                emailIds.push(pfId); //视图id
                var pfView = vault.ViewOperations.GetView(pfId);
                emailPaths.push(shellFrame.CurrentPath + "\\" + pfView.Name); //视图名称 
            }
            var vaultName = vault.Name;
            var projId = CC.getProjectId(shellFrame.ShellUI.Vault);

            var views = emailIds;
            var viewp = emailPaths;
            // Replace the listing with a dashboard.
            var dbworldPath = CC.getInstallPath(shellFrame.ShellUI.Vault);

            customData = {
                objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties,
                viewIds: views,
                emailIdFather: emailIdF,
                dbworldPath: dbworldPath,
                viewsPath: viewp,
                vaultName: vaultName,
                projId: projId
            };
            shellFrame.ShowDashboard("email", customData);

        } else if (shellFrame.CurrentPath === "项目成员") {
            var memberId = currentFolders.Item(currentFolders.Count).View;

            customData = {
                objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties,
                mbrIdFather: memberId
            };
            shellFrame.ShowDashboard("members", customData);

        } else if (shellFrame.CurrentPath === "邮件系统\\发件箱") {
            var sendIdF = currentFolders.Item(currentFolders.Count).View;
            //shellFrame.ShellUI.ShowMessage(sendIdF + ' - ' + currentFolders.Count);
            var svaultName = vault.Name;
            var sdbworldPath = CC.getInstallPath(vault);
            // Replace the listing with a dashboard.
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, viewId: sendIdF, vaultName: svaultName, dbworldPath: sdbworldPath };
            shellFrame.ShowDashboard("sendm", customData);

        } else if (shellFrame.CurrentPath === "邮件系统\\草稿箱") {
            var draftIdF = currentFolders.Item(currentFolders.Count).View;
            var dvaultName = vault.Name;
            var draftId = draftIdF;
            var ddbworldPath = CC.getInstallPath(vault);
            // Replace the listing with a dashboard.
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, viewId: draftId, vaultName: dvaultName, dbworldPath: ddbworldPath };
            shellFrame.ShowDashboard("draftm", customData);
        }
        //else if (shellFrame.CurrentPath === "众包管理\\众包任务") {
        //    var viewIdParent = currentFolders.Item(currentFolders.Count).View;
//    var childrenFolders = shellFrame.Listing.items.Folders;//子文件夹 (文件夹 or more)
        //    var views = [];
        //    var vPath = [];
        //    for (var i = 1; i <= childrenFolders.Count; i++) {
        //        var pfId = childrenFolders.Item(i).View;//视图
        //        views.push(pfId); //视图id
        //        var pfView = vault.ViewOperations.GetView(pfId);
        //        vPath.push("众包管理\\众包任务\\" + pfView.Name); //视图名称 
        //    }
        //    customData = {
        //        "viewId": viewIdParent,
        //        "subViewIds": views,
        //        "viewsPath": vPath
        //    };
        //    shellFrame.ShowDashboard("crowdsourceTask", customData);
        //}
        else if (shellFrame.CurrentPath === "邮件通讯录") {
            var malContsIdF = currentFolders.Item(currentFolders.Count).View;
            var malContsFather = malContsIdF;
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, malId: malContsFather };
            shellFrame.ShowDashboard("mailcontacts", customData);
        } else if (shellFrame.CurrentPath === "策划管理\\设计进度") {
            customData = {};
            shellFrame.ShowDashboard("dtdesignschedule", customData);
        } else if (shellFrame.CurrentPath === "策划管理\\专业配置") {
            customData = {};
            shellFrame.ShowDashboard("dtsetprofessional", customData);
        } else if (shellFrame.CurrentPath === "策划管理\\项目策划") {
            var typeId = MF.alias.objectType(vault, md.projectPlan.typeAlias);

            var prefilledPropValues = CC.projectplanning.getPrefilledPropValues(vault);

            var num = shellFrame.ActiveListing.items.Count;
            var currentItem = shellFrame.ActiveListing.items.ObjectVersions;
            if (num === 0 || currentItem.Count === 0) {
                MF.ObjectOps.createObjPrefilled(vault, prefilledPropValues, typeId, undefined);
            } else {
                var objVer = currentItem.Item(1).ObjVer;
                vault.ObjectOperations.ShowBasicEditObjectWindow(0, objVer);
            }
        } else if (shellFrame.CurrentPath === "项目文档\\设计文档\\提资区") {
            customData = { currentPath: "项目文档\\设计文档\\提资区" };
            shellFrame.ShowDashboard("dtextract", customData);

        } else if (shellFrame.CurrentPath === "项目文档\\设计文档\\提资区\\提资记录") {
            customData = { currentPath: "项目文档\\设计文档\\提资区\\提资记录" };
            shellFrame.ShowDashboard("dtextract", customData);

        } else if (shellFrame.CurrentPath === "项目文档\\设计文档\\校审区") {
            customData = { currentPath: "项目文档\\设计文档\\校审区" };
            shellFrame.ShowDashboard("dtreview", customData);

        }

        var currentPth = shellFrame.CurrentPath;
        if (currentPth && currentPth.split("\\").length >= 2 && currentPth.split("\\")[0] === "工作日志") {
            customData = {};
            shellFrame.ShowDashboard("viewlog", customData);
        }

        var child = shellFrame.CurrentPath.split('\\');
        if (child.length>=2 && child[0] === "模板" && child[1] !== undefined) { 
            publicUtils.setListingCreator(currentFolders, vault, false);
            //CC.SetListingTheme(shellFrame.Listing);

        } else if (shellFrame.CurrentPath === "项目文档\\共享文档\\查看共享给我的") {
            publicUtils.setListingCreator(currentFolders, vault, true);
            //CC.SetListingTheme(shellFrame.Listing);

        } else if (child.length > 2 && child[0] === "项目文档") {

            var folders = shellFrame.ActiveListing.items.Folders;
            if (folders.Count === 0) {
                publicUtils.setListingCreator(currentFolders, vault, false);
                //CC.SetListingTheme(shellFrame.Listing);
            }
        } 
    };
}
