
"use strict";

function OnNewShellUI( shellUI ) {
	/// <summary>The entry point of ShellUI module.</summary>
    /// <param name="shellUI" type="MFiles.ShellUI">The new shell UI object.</param> 
    var globalObject = { "mySoftList": [], "accessToken": "", "cloudUrl": "", "password": "" };
    //shellUI.Events.Register(MFiles.Event.Started, shellUIStartHandler(shellUI, globalObject));
    
	// Register to listen new shell frame creation event. This reacts to normal shell frames only (not e.g. common dialog nor embedded shell frames).
    shellUI.Events.Register(MFiles.Event.NewShellFrame, newShellFrameHandler(globalObject));
}

//function shellUIStartHandler(shellUI, globalObject) {
//    /// <summary>Event handler to handle new shell frame object creations.</summary>
//    /// <param name="shellFrame" type="MFiles.ShellFrame">The new shell frame object.</param> 
//    return function () {
//        // Register to listen the started event.
//        globalObject.sessionId = shellUI.CreatePersistentBrowserContent('http://www.dbworld.cn', { persistentid: 0, defaultsize: 0, defaultvisibility: true });
//    }
//}

function newShellFrameHandler(globalObject) {
	/// <summary>Event handler to handle new shell frame object creations.</summary>
    /// <param name="shellFrame" type="MFiles.ShellFrame">The new shell frame object.</param> 

    

    return function(shellFrame) {
        // Register to listen the started event.
        
        shellFrame.Events.Register(MFiles.Event.Started, getShellFrameStartedHandler(shellFrame, globalObject));
    }
}

function getShellFrameStartedHandler(shellFrame, globalObject) {
	/// <summary>Gets the event handler for "started" event of a shell frame.</summary>
	/// <param name="shellFrame" type="MFiles.ShellFrame">The current shell frame object.</param>
	/// <returns type="MFiles.Event.OnStarted">The event handler object</returns>
    
	// Return the handler function for Started event.
	return function() {
		/// <summary>The "started" event handler implementation for a shell frame.</summary>
	    // Shell frame object is now started. Check if this is the root view.
	    //if (!globalObject.sessionId) {
	    //    globalObject.sessionId = shellFrame.ShellUI.CreatePersistentBrowserContent('http://www.dbworld.cn', { persistentid: 0, defaultsize: 0, defaultvisibility: true });
	    //}


	    var vault = shellFrame.ShellUI.Vault;

	    shellFrame.RightPane.Visible = true;
        
	    //主目录
	    if (shellFrame.CurrentPath == "") {
	        //转到"我的文档"
            try {
                shellFrame.CurrentPath = "我的文档";
            }catch(e){} 
	    }
	    if (shellFrame.CurrentPath == "我的文档" || shellFrame.CurrentPath == "") {

	        //if (globalObject.sessionId || globalObject.sessionId === 0) {
	        //    var homeTabId = "_hometab";
	        //    var homeTab = null;
	        //    try {
	        //        homeTab = shellFrame.RightPane.GetTab(homeTabId);
	        //    } catch (e) {

	        //    }
	        //    if (!homeTab) {
	        //        homeTab = shellFrame.RightPane.AddTab(homeTabId, "首页", "_last");
	        //    }

	        //    if (homeTab) {
	        //        homeTab.ShowPersistentContent(globalObject.sessionId);
	        //        homeTab.Visible = true;
	        //        //appTab.select();
	        //    }
	        //}
	        // Replace the listing with a dashboard.
	        var listData = {};
	        //shellFrame.ShowDashboard( "home", listData );
	        var token = cd.getToken(vault);
	        var cloudUrl = cd.getCloudAppUrl(vault);
	        globalObject.cloudUrl = cloudUrl;

	        var tokenObj = eval('(' + token + ')');
	        globalObject.accessToken = tokenObj["access_token"];

	        var softs = webapi.getSofts(vault, token);
            //获取我购买的软件列表
	        var jsonRes = eval('(' + softs + ')');
	        if (jsonRes.status === 200) {
	            var jsonData = jsonRes.response;
	            for (var i = 0; i < jsonData.length; i++) {
	                for (var j = 0;
                        j < globalObject.mySoftList.length &&
                        (jsonData[i].ApplicationID != globalObject.mySoftList[j].ApplicationID ||
	                    jsonData[i].AppName != globalObject.mySoftList[j].AppName);
                        j++);
	                if (j === globalObject.mySoftList.length) {
	                    //globalObject.mySoftList.push({ "ApplicationID": jsonData[i].ApplicationID, "AppName": jsonData[i].AppName });
	                    globalObject.mySoftList.push(jsonData[i]);
	                    //shellFrame.shellUI.ShowMessage(jsonData[i].ApplicationID);
	                }
	            }
	        }
	        //获取密码
	        var userPrivateStr = webapi.getUserPrivate(vault, token);
	        var jsonResUp = eval('(' + userPrivateStr + ')');
	        if (jsonResUp.status === 200) {
	            var userPrivate = jsonResUp.response;
	            if (userPrivate.Password) {
	                globalObject.password = userPrivate.Password;
	            } else {
                    //测试用
	                globalObject.password = "1111111";
	            }
	        }

		    var accountName = vault.SessionInfo.AccountName;
	        var appTabId = "_apptab";
		    var appTab = null;
            try {
                appTab = shellFrame.RightPane.GetTab(appTabId);
            } catch (e) {
                
            }
            if (!appTab) {
                appTab = shellFrame.RightPane.AddTab(appTabId, "云应用", "_last");
            }

            if (appTab) {
                var custData = { accountName: accountName, softs: softs, token: token, cloudUrl: cloudUrl, password: globalObject.password };
                appTab.ShowDashboard('cloudView', custData);
                appTab.Visible = true;
                //appTab.select();
            }
	        //var hometabId = "_home";
	        //var hometab = null;
	        //try {
	        //    hometab = shellFrame.RightPane.GetTab(hometabId);
	        //} catch (e) {
	        //}
	        //if (!hometab) {
	        //    hometab = shellFrame.RightPane.AddTab(hometabId, "主页", "_last");
	        //}
            //if (hometab) {
            //    hometab.ShowDashboard('cate', null);
            //    hometab.Visible = true;
            //    hometab.select();
            //}
		}

		CC.createCmds(shellFrame, globalObject);
		shellFrame.Listing.Events.Register(Event_SelectionChanged, function (sItems) {
		    //设置“共享...”等按钮的显示状态
		    CC.setCmdSateAsSelectionChanged(shellFrame);
		});
	};
}