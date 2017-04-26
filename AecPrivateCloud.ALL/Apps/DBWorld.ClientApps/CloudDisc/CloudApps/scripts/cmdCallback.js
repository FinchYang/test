var CC = {
    cmds: {},

    createCmds: function (shellFrame, globalObject) {
        if (!shellFrame.TaskPane.Available) return;
        this.createContextMenuCmds(shellFrame, globalObject);
    },

    createContextMenuCmds: function (shellFrame, globalObject) {
        //var vault = shellFrame.ShellUI.Vault;
        var shareCmdName = "分享...";
        var shareCmdId = shellFrame.Commands.CreateCustomCommand(shareCmdName);
        this.cmds.cmdShareDoc = shareCmdId;
        shellFrame.Commands.Events.Register(Event_CustomCommand, function (cmdId) {
            if (cmdId === shareCmdId) {
                cmdCallbackFn.shareOp(shellFrame);
            }
        });
        shellFrame.Commands.AddCustomCommandToMenu(shareCmdId, MenuLocation_ContextMenu_Top, 0);
        shellFrame.Commands.SetCommandState(shareCmdId, CommandLocation_All, CommandState_Hidden);

        var openCmdId = shellFrame.Commands.CreateCustomCommand("从云端打开");
        this.cmds.cmdOpenDoc = openCmdId;
        shellFrame.Commands.Events.Register(Event_CustomCommand, function(cmdId) {
            if (cmdId === openCmdId) {
                cmdCallbackFn.openAtCloud(shellFrame, globalObject);
            }
        });
        shellFrame.Commands.AddCustomCommandToMenu(openCmdId, MenuLocation_ContextMenu_Top, 1);
        shellFrame.Commands.SetCommandState(openCmdId, CommandLocation_All, CommandState_Hidden);

        var deleteCmdId = shellFrame.Commands.CreateCustomCommand("删除文件夹...");
        this.cmds.cmdDeleteFolder = deleteCmdId;
        shellFrame.Commands.Events.Register(Event_CustomCommand, function (cmdId) {
            if (cmdId === deleteCmdId) {
                //shellFrame.ShellUI.ShowMessage("删除文件夹");
                cmdCallbackFn.deletePropertyFolder(shellFrame);
            }
        });
        shellFrame.Commands.AddCustomCommandToMenu(deleteCmdId, MenuLocation_ContextMenu_Top, 2);
        shellFrame.Commands.SetCommandState(deleteCmdId, CommandLocation_All, CommandState_Hidden);
    },

    setCmdSateAsSelectionChanged: function (shellFrame) {

        if (!shellFrame.Listing || !shellFrame.Listing.CurrentSelection) return;
        try {
            //设置“分享”显示状态
            var objFiles = shellFrame.Listing.CurrentSelection.ObjectFiles;
            if (objFiles.Count == 1) {
                shellFrame.Commands.SetCommandState(this.cmds.cmdShareDoc, CommandLocation_All, CommandState_Active);
            } else {
                shellFrame.Commands.SetCommandState(this.cmds.cmdShareDoc, CommandLocation_All, CommandState_Hidden);
            }
            //设置"从云端打开"显示状态
            if (objFiles.Count > 0) {
                shellFrame.Commands.SetCommandState(this.cmds.cmdOpenDoc, CommandLocation_All, CommandState_Active);
            } else {
                shellFrame.Commands.SetCommandState(this.cmds.cmdOpenDoc, CommandLocation_All, CommandState_Hidden);
            }
            //设置"删除文件夹"显示状态
            var folders = shellFrame.Listing.CurrentSelection.Folders;
            if (folders.Count == 1) {
                if (folders.Item(1).FolderDefType == MFFolderDefTypePropertyFolder) {
                    shellFrame.Commands.SetCommandState(this.cmds.cmdDeleteFolder, CommandLocation_All, CommandState_Active);
                } else {
                    shellFrame.Commands.SetCommandState(this.cmds.cmdDeleteFolder, CommandLocation_All, CommandState_Hidden);
                }
            } else {
                shellFrame.Commands.SetCommandState(this.cmds.cmdDeleteFolder, CommandLocation_All, CommandState_Hidden);
            }

        } catch (e) {
            return;
        }
    }
};



///<summary>命令的回调函数</summary>
var cmdCallbackFn = cmdCallbackFn || {};

(function (fn) {
    ///<summary>从云端打开</summary>
    fn.openAtCloud = function(sf, globalObject) {
        var vault = sf.shellUI.Vault;
        var accountName = vault.SessionInfo.AccountName;
        //sf.shellUI.ShowMessage(accountName + "-"+shellFrame.Listing.CurrentSelection.Count);
        if (sf.Listing.CurrentSelection.ObjectFiles.Count > 0) {
            var selectedItems = sf.Listing.CurrentSelection.ObjectFiles;
            for (var i = 1; i <= selectedItems.Count; i++) {
                var sItem = selectedItems.Item(i);
                var ext = sItem.ObjectFile.Extension;
                //var appSofts = softConfigOp.getAppsByExtension(ext);
                var mySoftList = globalObject.mySoftList;
                //var appSoft = this.getOpenSoftFromBoughts(mySoftList, appSofts);
                var appSoft = this.getOpenSoftFromBoughtsByExt(mySoftList, ext);
                if (appSoft) {
                    var path0 = vault.ObjectFileOperations.GetPathInDefaultView(sItem.ObjVer.ObjID, sItem.ObjVer.Version,
                        sItem.ObjectFile.ID, sItem.ObjectFile.Version, MFLatestSpecificBehaviorAutomatic, false);
                    //转换为服务端路径
                    path0 = this.trans2ServerPath(path0);
                    this.openFileFromMfile(sf, appSoft, path0, accountName, sItem.ObjVer.ObjID.ID, globalObject);
                } else {
                    sf.shellUI.ShowMessage("您未购买相应的软件！");
                }
                break;
            };
        }
    };
    fn.getOpenSoftFromBoughtsByExt = function (boughtSofts, ext) {
        for (var i = 0; i < boughtSofts.length; i++) {
            var extstr = boughtSofts[i].OpenExt;
            if (extstr) {
                var exts = extstr.split(",");
                for (var j = 0; j < exts.length; j++) {
                    if (exts[j] == ext || exts[j] == "."+ext) {
                        return boughtSofts[i];
                    }
                }
            }
        }
        return null;
    };
    fn.getOpenSoftFromBoughts = function (boughtSofts, canOpenSofts) {
        for (var i = 0; i < canOpenSofts.length; i++) {
            var appSoft = canOpenSofts[i];
            for (var j = 0; j < boughtSofts.length; j++) {
                if (boughtSofts[j].ApplicationID == appSoft.appID) {
                    return appSoft;
                }
            }
        }
        return null;
    }
    //从云端打开文件
    fn.openFileFromMfile = function (shellFrame, app, filePath, accountName, objId, globalObject) {
        var domain = "";
        var userName = "";
        var account = accountName.split('\\');
        if (account.length > 1) {
            userName = account[1];
            domain = account[0];
        } else {
            userName = account[0];
        }
        //var pwd = this.getPassword();
        //if (!pwd) pwd = "abcde12345!";
        var appID = app.ApplicationID;
        var appName = app.AppName;
        //var vault = shellFrame.shellUI.Vault;
        //文件路径:中间件
        //var vaultGuid = vault.SessionInfo.VaultGUID;
        //var vaultName = vault.Name;
        //filePath = vaultGuid + " " + vaultName + " " + appID + " " + filePath;
        //var appIdM = "Citrix.MPS.App.Farm1.AppManagement";
        //appIdM = encodeURIComponent(appIdM);
        //var appNameM = "AppManagement";
        //appNameM = encodeURIComponent(appNameM);

        //转码，保证IE设别 中文字符串
        domain = encodeURIComponent(domain);
        userName = encodeURIComponent(userName);
        var pwd = encodeURIComponent(globalObject.password);
        appID = encodeURIComponent(appID);
        appName = encodeURIComponent(appName);
        filePath = encodeURIComponent(filePath);
        //var token = encodeURIComponent(globalObject.accessToken);
        var cloudUrl = globalObject.cloudUrl;

        //var url = cloudUrl + "/JSONAPI/JSONAPI.ashx?action=GetApplicationCall&Domain="
        //    + domain + "&Token=" + token + "&ApplicationID=" + appID + "&AppName=" + appName + "&ProjectName=&FilePath=" + filePath;
        var url = cloudUrl+"/Common/launch.aspx?Domain=" + domain + "&UserName=" + userName + "&Password=" + pwd
                + "&ApplicationID=" + appID + "&AppName=" + appName + "&ProjectName=&FilePath=" + filePath;

        var appPath = MFiles.ApplicationPath + "WebConsole.exe";

        var fso = new ActiveXObject('Scripting.FileSystemObject');
        if (!fso.FileExists(appPath)) {
            shellFrame.shellUI.ShowMessage("缺少WebConcole！");
            return;
        }
        var fileName = objId || userName;
        var urlTempFile = fso.GetSpecialFolder(2) + "\\" + appName + "-" + fileName + ".txt";
        this.createFile(urlTempFile, url);
        var tempPath = fso.GetSpecialFolder(2) + "\\" + appName + "-" + fileName + ".ica";
        //shellFrame.shellUI.ShowMessage(tempPath);
        this.runProgram(appPath, [url, tempPath]);
        this.runProgram(tempPath);
        //this.runProgramUrl(url);
    };
    //把客户端文件路径转换为服务器端文件路径
    fn.trans2ServerPath = function (pathSrc) {
        var sPath = "";
        var pArr = pathSrc.split("\\");
        if (pArr.length > 0) {
            for (var i = 0; i < pArr.length; i++) {
                if (i == 0) {
                    sPath += softConfigOp.driveLetterOnServer;
                }
                if (i == 1) {
                    sPath += softConfigOp.vaultNameOnServer;
                }
                if (i != 1 && i != 0) {
                    sPath += pArr[i];
                }
                if (i != pArr.length - 1) {
                    sPath += "\\";
                }
            };
        }
        return sPath;
    };
    fn.createFile = function(filePath, content) { //for write
        var fso = new ActiveXObject("Scripting.FileSystemObject");
        try {
            //判断文件是否已经存在
            if (fso.FileExists(filePath)) {
                fso.DeleteFile(filePath);
            }
            var fh = fso.CreateTextFile(filePath, 2, false); //8
            fh.WriteLine(content);
            fh.Close();
        } catch (e) {
            throw ('无权限创建文件！');
        } finally {
            fso = null;
        }
    };
    fn.runProgram = function(exeFile, args) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments.</param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject("WScript.Shell");
        var cmd = '"' + exeFile + '"';
        if (args != undefined) {
            for (var i = 0; i < args.length; i++) {
                cmd = cmd + ' ' + args[i];
            }
        }
        var errorCode = shell.Run(cmd, 0, true);
        //shell = null;
        return errorCode;
    };
    fn.runProgramUrl = function(exeFile, args, wait, windowId) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments. []</param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject("WScript.Shell");
        var cmd = '"' + exeFile + '"';
        args = args || [];
        for (var i = 0; i < args.length; i++) {
            if (args[i] === '') continue;
            cmd = cmd + ' ' + '"' + args[i] + '"';
        }
        //shellFrame.ShellUI.ShowMessage(cmd);
        if (!windowId) windowId = 0;
        var errorCode;
        if (args.length > 0) {
            errorCode = shell.Run(cmd, windowId, wait);
        } else {
            errorCode = shell.Run(cmd);
        }
        shell = null;
        return errorCode;
    };
    //获取密码
    fn.getPassword = function() {
        var pwd = "";
        try {
            var fso = new ActiveXObject("Scripting.FileSystemObject");
            var tmppath = fso.GetSpecialFolder(2);
            var filePath = tmppath + "\\AecPass.txt";
            if (fso.FileExists(filePath)) {
                var file = fso.OpenTextFile(filePath, 1);
                pwd = file.ReadLine();
                pwd = pwd.replace(/(^\s*)|(\s*$)/g, "");
                file.Close();
            }
            fso = null;
        } catch (e) {
        }
        return pwd;
    };
})(cmdCallbackFn);

(function (fn) {
    ///<summary>根据选中文档，弹出Web分享界面</summary>
    fn.shareOp = function(sf) {
        var selectedItems = sf.Listing.CurrentSelection.ObjectFiles;
        if (selectedItems.Count <= 0) return;
        var info = {};

        var vault = sf.ShellUI.Vault;
        var vGuid = vault.SessionInfo.VaultGUID;
        //var vUserId = vault.SessionInfo.UserID;
        for (var i = 1; i <= selectedItems.Count; i++) {
            var item = selectedItems.Item(i);
            var objVnType = item.ObjVer.Type;
            var objVnId = item.ObjVer.ID;
            //var objVnVersion = item.ObjVer.Version;
            var fileId = item.ObjectFile.ID;
            //var fileVersion = item.ObjectFile.Version;
            var fileName = item.ObjectFile.Title + "." + item.ObjectFile.Extension;

            //var filePath = vault.ObjectFileOperations.GetPathInDefaultView(item.ObjVer.ObjID, item.ObjVer.Version, item.ObjectFile.ID,
            //item.ObjectFile.Version, MFLatestSpecificBehaviorAutomatic, false);
            //sf.ShellUI.ShowMessage("(fileVerId,fileVersion,fileName)+\r\n" + fileId + "-" + fileVersion + "-" + fileName);
            info.FileVerID = fileId;
            //info.FileVerVersion = fileVersion;
            info.Filename = fileName;
            info.VaultGuid = vGuid;
            //info.UserID = vUserId;
            info.ObjType = objVnType;
            info.ObjId = objVnId;
            break;
        }
        var dashboardData = { 'SharedFiles': info };
        sf.ShowPopupDashboard('webSharing', true, dashboardData);
    }
})(cmdCallbackFn);

(function (fn) {
    ///<summary>删除MFiles中属性文件夹</summary>
    fn.deletePropertyFolder = function(shellFrame) {
        var vault = shellFrame.ShellUI.Vault;
        var folders = shellFrame.Listing.CurrentSelection.Folders;
        for (var i = 1; i <= folders.Count; i++) {
            if (folders.Item(i).FolderDefType != MFFolderDefTypePropertyFolder) continue;

            var propId = this._getFolderPropDef(shellFrame);
            if (propId < 0) {
                shellFrame.ShellUI.ShowMessage("获取所选属性文件的属性ID失败！");
                continue;
            }
            var oTvalue = folders.Item(i).PropertyFolder;
            var oLookup;
            if (oTvalue.DataType == MFDatatypeLookup) {
                oLookup = oTvalue.GetValueAsLookup();
            } else if (oTvalue.DataType == MFDatatypeMultiSelectLookup) {
                oLookup = oTvalue.GetValueAsLookups().Item(1);
            }
            if (oLookup != undefined && oLookup.Deleted == false) {
                var objTypeId = this._getPropFolderObjType(shellFrame, propId);
                if (objTypeId == -1) {
                    shellFrame.ShellUI.ShowMessage("无权删除文件夹(" + oLookup.DisplayValue + ")");
                    continue;
                }
                try {
                    this._deleteObject(vault, objTypeId, oLookup.Item);
                } catch (e) {
                    //shellFrame.ShellUI.ShowMessage("您无权删除文件夹(" + oLookup.DisplayValue + "):" + e.message);
                    shellFrame.ShellUI.ShowMessage("您无权删除文件夹(" + oLookup.DisplayValue + ")");
                }
                shellFrame.Listing.RefreshListing(true, true, false); //F5刷新
            }
        }
    };
    //获取所选属性文件夹的属性ID
    fn._getFolderPropDef = function(shellFrame) {
        //shellFrame.ShellUI.ShowMessage(shellFrame.CurrentPath);
        var vault = shellFrame.ShellUI.Vault;
        var currentFolders = shellFrame.CurrentFolder;
        var depth = currentFolders.Count;
        var viewId = -1;
        var levelId = 1;
        for (var i = depth; i > 0; i--) {
            var item = currentFolders[i - 1];
            if (item.FolderDefType == MFFolderDefTypeViewFolder) {
                viewId = item.View;
                break;
            }
            levelId++;
        }
        if (viewId != -1) {
            var parentView = vault.ViewOperations.GetView(viewId);
            // return shellFrame.ShellUI.ShowMessage("Count:" + parentView.Levels.Count + "-level:" + levelId);
            if (parentView.Levels.Count >= levelId) {
                var folderLevel = parentView.Levels.Item(levelId);
                var propId = folderLevel.Expression.DataPropertyValuePropertyDef;
                //shellFrame.ShellUI.ShowMessage("propId:" + propId);
                return propId;
            }
        }
        return -1;
    };
    //获取所选属性文件夹的所对应的对象类型
    fn._getPropFolderObjType = function(shellFrame, propertyDefId) {
        var vault = shellFrame.ShellUI.Vault;
        var propDef = vault.PropertyDefOperations.GetPropertyDef(propertyDefId);
        if (propDef.BasedOnValueList) {
            //shellFrame.ShellUI.ShowMessage("ValueList:" + propDef.ValueList);
            return propDef.ValueList;
        }
        return -1;
    };
    ///<summery>删除MFiles对象</summery>
    fn._deleteObject = function(vault, typeId, objectId) {
        try {
            var oObjID = MFiles.CreateInstance("ObjID");
            oObjID.SetIDs(typeId, objectId);
            vault.ObjectOperations.RemoveObject(oObjID);
        } catch (e) {
            var mes = "删除对象(Type:" + typeId + ", ID:" + objectId + ")失败:" + e.message;
            throw new Error(mes);
        }
    };
})(cmdCallbackFn);