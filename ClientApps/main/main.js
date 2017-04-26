
"use strict";

/*******************
 * ShellUI module entry
 **************************/	

function OnNewShellUI(shellUI) {
    shellUI.Events.Register(MFiles.Event.NewShellFrame, newShellFrameHandler(shellUI));
}


function newShellFrameHandler(shellUI) {    
    return function(shellFrame) {
        //MF.ui.resetTempSearchView(shellUI.Vault);
        shellFrame.Events.Register(MFiles.Event.Started, getShellFrameStartedHandler(shellFrame));
    }    
}


function getShellFrameStartedHandler(shellFrame) {
    // Return the handler function for Started event.
    return function () {
        bim.addBimCmd(shellFrame);	
         bim.createBIMCmds(shellFrame);
        /// <summary>The "started" event handler implementation for a shell frame.</summary>        
        createRootCmd(shellFrame);
        createWorkspaceCmds(shellFrame);
        var attachCmdId = createAttachCmd(shellFrame);
        shellFrame.RightPane.Visible = true;
        
        var customData = {};
         if (shellFrame.CurrentPath === ''||shellFrame.CurrentPath === "主目录") {       
      //  if (shellFrame.CurrentPath === '') {            
            shellFrame.ShowDashboard("root", customData);             
        } else if (shellFrame.CurrentPath === "根目录") {
            shellFrame.ShowDashboard("home", customData);
        } else if (shellFrame.CurrentPath === '周报月报\\月报') {
            shellFrame.ShowDashboard("monthreport", customData);
        }else if (shellFrame.CurrentPath === '项目信息\\项目概况') {
            shellFrame.RightPane.Visible = false;
            var projItems = shellFrame.Listing.items.ObjectVersionsAndProperties;
            if(projItems.Count>0){
                customData.objsAndProps = projItems.Item(1);
                shellFrame.ShowDashboard("projHome", customData);
            }else{
                shellFrame.ShellUI.ShowMessage("缺少项目对象！");
            }            
        }else if (shellFrame.CurrentPath === '项目信息\\成员列表') {   
            shellFrame.RightPane.Visible = false;     
            shellFrame.ShowDashboard("projMember", customData);
        }
        
        if (shellFrame.Listing) {
          //  shellFrame.ShowMessage("mfflow in main.js")
            shellFrame.Listing.Events.Register(Event_SelectionChanged, CC.mfflow.selectionChangedHandler(shellFrame));
        }
        
        shellFrame.ActiveListing.Events.Register(Event_SelectionChanged, function (selectedItems) {
               CC.task.markTask(shellFrame);
               if (attachCmdId) {
                    CC.attachment.setCmdState(shellFrame, attachCmdId);
               }
        });
        //新建的listing时触发
        shellFrame.Events.Register(Event_ActiveListingChanged, function (previousActiveListing, newActiveListing) {

            shellFrame.ActiveListing.Events.Register(Event_SelectionChanged, function (selectedItems) {
                CC.task.markTask(shellFrame);
                if (attachCmdId) {
                    CC.attachment.setCmdState(shellFrame, attachCmdId);
                }
            });
        });	
          var vault = shellFrame.ShellUI.Vault;
        if (shellFrame.CurrentPath === '安全台账') {              
                 var customData = {Vault:vault};
                 shellFrame.RightPane.Visible = false;
                    //    shellFrame.ShellUI.ShowPopupDashboard ("doccategory",false, customData);
                    shellFrame.ShowDashboard("securereport", customData);
        }   
       
    //    try {
    //         var group = shellFrame.TaskPane.CreateGroup('导入计划', 1);
    //         var cadCmd = shellFrame.Commands.CreateCustomCommand("导入计划");
    //         shellFrame.Commands.SetIconFromPath(cadCmd, "exchange.ico");
    //         shellFrame.TaskPane.AddCustomCommandToGroup(cadCmd, group, 0);
    //         shellFrame.Commands.Events.Register(Event_CustomCommand, function (command) {
    //             if (command === cadCmd) {
    //                           var appPath = GetImport() + '\\ImportObjectTool.exe';
    //                 var dllPath = vault.Name;
    //                 if (fileExists(appPath)) {
    //                     runProgram(appPath, [dllPath]);
    //                 } else {
    //                     shellFrame.ShellUI.ShowMessage("导入计划启动工具缺失！"+appPath+dllPath);
    //                 }
    //             }
    //         });
    //     }
    //     catch (e) {
    //     }
        // if (shellFrame.CurrentPath === '导入计划') { 
        //          var appPath = GetImport() + '\\ImportObjectTool.exe';
        //             var dllPath = vault.Name;
        //             if (fileExists(appPath)) {
        //                 runProgram(appPath, [dllPath]);
        //             } else {
        //                 shellFrame.ShellUI.ShowMessage("导入计划启动工具缺失！"+appPath+dllPath);
        //             }
        // }      
    };
}
 
function fileExists(filePath) {
    var fso = new ActiveXObject('Scripting.FileSystemObject');
    var ok = fso.FileExists(filePath);
    fso = null;
    return ok;
}

function runProgram(exeFile, args) {
    /// <summary>Run external program.</summary>
    /// <param name="exeFile" type="String">the program file(*.exe).</param>
    /// <param name="args" type="Array">commandline arguments. []</param>
    /// <returns>errorCode:0, success;other, error</returns>
    var shell = new ActiveXObject('WScript.Shell');
    var cmd = '"' + exeFile + '"';
    args = args || [];
    for (var i = 0; i < args.length; i++) {
        cmd = cmd + ' ' + '"' + args[i] + '"';
    }
    var errorCode;
    if (args.length > 0) {
        errorCode = shell.Run(cmd, 1, true);
    } else {
        errorCode = shell.Run(cmd);
    }
    shell = null;
    return errorCode;
}
 function GetImport() {
     var _installPath = '';
        try {
            _installPath = readRegValue('HKCU\\Software\\CSCEC82Client\\Client\\INSTDIR');
           
        } catch (e) {
            alert(e);
        }
        return _installPath;
    };
 function readRegValue (regPath) {
        ///<param>HKEY_CURRENT_USER\Software\DBWorld\Client\INSTDIR</param>
        var shell = new ActiveXObject("WScript.Shell");
        return shell.RegRead(regPath);
    };

function createRootCmd(shellFrame) {
	if (!shellFrame.TaskPane.Available) {
        return;
    }
    var rootCmdName = "根目录";
    var rootCmd = new MF.Command(rootCmdName, 'ico/根目录.ico');
    rootCmd.create(shellFrame);
    rootCmd.registerEvent(shellFrame, function (sf) {
        sf.CurrentPath = rootCmdName;
    });
    rootCmd.add2TaskGroup(shellFrame, 4, -1);
}
function createAttachCmd(shellFrame){
	if (!shellFrame.TaskPane.Available) {
        return;
    }
    //添加附件
    var attachCmd = new MF.Command('添加附件', "ico/添加附件.ico");
    attachCmd.create(shellFrame);
    attachCmd.registerEvent(shellFrame, function(sf) {
        CC.attachment.attachCmdCall(sf);
    });
    attachCmd.add2TaskGroup(shellFrame, TaskPaneGroup_ViewAndModify, 1);
    shellFrame.Commands.SetCommandState(attachCmd.command, CommandLocation_All, CommandState_Hidden);
    return attachCmd.command;
}
function createWorkspaceCmds(shellFrame) {
    var vault = shellFrame.ShellUI.Vault;
    
    // if(CC.userRole.isPM(vault) || CC.userRole.isEMD(vault)){
    //     var contractorId = shellFrame.TaskPane.CreateGroup('分包商管理', -11);    
    //     var contractorCmd1 = new MF.Command("分包商管理", 'ca.ico');
    //     contractorCmd1.create(shellFrame);
    //     contractorCmd1.registerEvent(shellFrame, function (sf) {
    //         sf.CurrentPath = '分包商管理';
    //     });
    //     contractorCmd1.add2TaskGroup(shellFrame, contractorId, 0);  
    // }

    if (!shellFrame.TaskPane.Available) {
        return;
    }
  
   // if(CC.userRole.isPM(vault) || CC.userRole.isSG(vault)|| CC.userRole.isSPMD(vault)|| CC.userRole.isGM(vault)|| CC.userRole.isCC(vault)|| CC.userRole.isCE(vault)){
       
            
        var secureId = shellFrame.TaskPane.CreateGroup('安全台账', -12);    
         
        var secureCmd = new MF.Command("安全台账", '1.ico');
         
        secureCmd.create(shellFrame);
         
        secureCmd.registerEvent(shellFrame, function (sf) {
           try{
            sf.CurrentPath = '安全台账';
              }
       catch(eafadd){
          // shellFrame.ShellUI.ShowMessage("haha");
           shellFrame.ShellUI.ShowMessage("没有安全台账查看权限！\n有权限的有：\n一般项目：项目经理，项目副经理，项目书记，项目总工程师，工长，施工员，安全员，安全主管，"+
           "二级单位-工程管理部，二级单位-工程管理部经理，二级单位-领导，二级单位-经理，二级单位-副经理，二级单位-总工程师，二级单位-总会计师，"+
           "二级单位-总经济师，二级单位-经理，二级单位-副经理，二级单位-副经理（生产），公司总部-安全生产管理部，公司总部-安全生产管理部经理，"+
           "公司总部-领导，公司总部-总经理，公司总部—副总经理，公司总部-副总经理（生产），公司总部-董事长，公司总部-总工程师，公司总部-总方案师，"+
           "公司总部-总会计师，公司总部-总经济师；\n"+
"直属项目：项目经理，项目副经理，项目书记，项目总工程师，工长，施工员，安全员，安全主管，公司总部-安全生产管理部，公司总部-安全生产管理部经理，"+
"公司总部-领导，公司总部-总经理，公司总部—副总经理，公司总部-副总经理（生产），公司总部-董事长，公司总部-总工程师，公司总部-总方案师，公司总部-总会计师，公司总部-总经济师；\n");
       }
        });
        secureCmd.add2TaskGroup(shellFrame, secureId, 0);  
      
   // }
	
	//工期模块
	if(CC.userRole.isPM(vault) ){
		var scheduledId = shellFrame.TaskPane.CreateGroup('工期模块控制点', -13);  
        var scheduledCmd = new MF.Command("工期模块节点",'icons/工期.ico');
        scheduledCmd.create(shellFrame);
        scheduledCmd.registerEvent(shellFrame, function (sf) {
             	var custData = {shellFrame:shellFrame};
            if(CC.userRole.isCE(vault) || CC.userRole.isVicePM(vault) || CC.userRole.isPM(vault) || CC.userRole.isEMD(vault) 
                || CC.userRole.isHEMD(vault)){
                shellFrame.ShowPopupDashboard('ScheduleControl', true, custData);
            }			
        });
        scheduledCmd.add2TaskGroup(shellFrame, scheduledId, 0);  
	}
	
	//打印二维码
	/*var printQRCodeCmd = new MF.Command("打印二维码",'');
	printQRCodeCmd.create(shellFrame);
	printQRCodeCmd.registerEvent(shellFrame,function(sf){
		var listing = shellFrame.ActiveListing;
		var selItems = listing.CurrentSelection;
		var custData = {shellFrame:shellFrame,selItems:selItems};
		shellFrame.ShowPopupDashboard('printQRCode', true, custData);
	});
	printQRCodeCmd.add2ContextMenu(shellFrame,MenuLocation_ContextMenu_Top,0);*/
	
	
//  var secureId = shellFrame.TaskPane.CreateGroup('导入计划', -11);    
//         var secureCmd = new MF.Command("导入计划", 'exchange.ico');
//         secureCmd.create(shellFrame);
//         secureCmd.registerEvent(shellFrame, function (sf) {
//             sf.CurrentPath = '导入计划';
//         });
//         secureCmd.add2TaskGroup(shellFrame, secureId, 0);  

    // var flowGrpId = shellFrame.TaskPane.CreateGroup('工作流管理', -10);    
    // var flowCmd1 = new MF.Command("待办工作流", 'ico/待办工作流_03.ico');
    // flowCmd1.create(shellFrame);
    // flowCmd1.registerEvent(shellFrame, function (sf) {
    //     sf.CurrentPath = '工作流\\待办工作流';
    // });
    // flowCmd1.add2TaskGroup(shellFrame, flowGrpId, 1);    
    // var flowCmd2 = new MF.Command("我发起的工作流", 'ico/发起的工作流_03.ico');
    // flowCmd2.create(shellFrame);
    // flowCmd2.registerEvent(shellFrame, function (sf) {
    //     sf.CurrentPath = '工作流\\我发起的工作流';
    // });
    // flowCmd2.add2TaskGroup(shellFrame, flowGrpId, 0);    
    // var flowCmd3 = new MF.Command("所有工作流", 'ico/所有工作流_03.ico');
    // flowCmd3.create(shellFrame);
    // flowCmd3.registerEvent(shellFrame, function (sf) {
    //     sf.CurrentPath = '工作流\\所有工作流';
    // });
    // flowCmd3.add2TaskGroup(shellFrame, flowGrpId, 2);
    // var flowCmd4 = new MF.Command("我的多级审核模板", 'ico/审核模板_03.ico');
    // flowCmd4.create(shellFrame);
    // flowCmd4.registerEvent(shellFrame, function (sf) {
    //     sf.CurrentPath = '工作流\\我的多级审核人模板';
    // });
    // flowCmd4.add2TaskGroup(shellFrame, flowGrpId, 3);
    
    // //var calendarGrpId = shellFrame.TaskPane.CreateGroup('日程管理', -9);
    // var hasDrawList = MF.alias.objectType(vault, md.drawingList.typeAlias) !== -1;
    // if(hasDrawList && CC.userRole.isZmDesign(vault)){//发图
    //     var CheckGrpId = shellFrame.TaskPane.CreateGroup('发图管理', -9);
    //     var dSendDrawCmd = new MF.Command('设计部发图', "ico/设计院发图.ico");
    //     dSendDrawCmd.create(shellFrame);
    //     dSendDrawCmd.registerEvent(shellFrame, function(sf) {
    //         CC.drawList.creare(sf, 0);
    //     });
    //     dSendDrawCmd.add2TaskGroup(shellFrame, CheckGrpId, 10);//TaskPaneGroup_New

    //     var fSendDrawCmd = new MF.Command('发图目录', "ico/发图目录.ico");
    //     fSendDrawCmd.create(shellFrame);
    //     fSendDrawCmd.registerEvent(shellFrame, function(sf) {
    //         sf.CurrentPath = '文档管理\\发图目录';
    //     });
    //     fSendDrawCmd.add2TaskGroup(shellFrame, CheckGrpId, 12);
    // }  
    
    // var docGrpId = shellFrame.TaskPane.CreateGroup('文档管理', -8);
    // var docCmd = new MF.Command("文档", "ico/文档_03.ico");
    // docCmd.create(shellFrame);
    // docCmd.registerEvent(shellFrame, function (sf) {
    //    sf.CurrentPath = "文档管理";
    // });
    // docCmd.add2TaskGroup(shellFrame, docGrpId, 0);  

    // var userRoles = CC.userRole.getUserRoles(vault);
    // //转发
    // var transCmd = new MF.Command('转发', 'ico/转发.ico');
    // transCmd.create(shellFrame);
    // transCmd.registerEvent(shellFrame, function(sf) {
    //     CC.document.forwardDoc(sf);
    // });
    // transCmd.add2TaskGroup(shellFrame, docGrpId, 1);
    ////回复
    // var replyCmd = new MF.Command('联系单回复', 'ico/回复.ico');
    // replyCmd.create(shellFrame);
    // replyCmd.registerEvent(shellFrame, function(sf) {
    //      CC.document.responseDoc(sf, userRoles);
    // });
    // replyCmd.add2TaskGroup(shellFrame, docGrpId, 2);  
    
    // var noticeGrpId = shellFrame.TaskPane.CreateGroup('消息通知', -7);
    // var noticeCmd1 = new MF.Command("未阅", "ico/未阅_03.ico");
    // noticeCmd1.create(shellFrame);
    // noticeCmd1.registerEvent(shellFrame, function(sf) {
    //     sf.CurrentPath = '消息通知\\未阅通知';
    // });
    // noticeCmd1.add2TaskGroup(shellFrame, noticeGrpId, 0);
    // var noticeCmd2 = new MF.Command("已阅", "ico/已读_03.ico");
    // noticeCmd2.create(shellFrame);
    // noticeCmd2.registerEvent(shellFrame, function(sf) {
    //     sf.CurrentPath = '消息通知\\已阅通知';
    // });
    // noticeCmd2.add2TaskGroup(shellFrame, noticeGrpId, 1);
    // var noticeCmd3 = new MF.Command("我发送的通知", "ico/发送的通知_03.ico");
    // noticeCmd3.create(shellFrame);
    // noticeCmd3.registerEvent(shellFrame, function(sf) {
    //     sf.CurrentPath = '消息通知\\我发送的通知';
    // });
    // noticeCmd3.add2TaskGroup(shellFrame, noticeGrpId, 2);
    
    
    //var reportGrpId = shellFrame.TaskPane.CreateGroup('周报月报', -6);
    
    //var weekCmd = new MF.Command('查看周报', "images\\周报_03.png");
    //weekCmd.create(shellFrame);
    //weekCmd.registerEvent(shellFrame, function(sf) {
        //sf.CurrentPath = '文档管理\\进度控制\\施工周报';
    //});
    //weekCmd.add2TaskGroup(shellFrame, reportGrpId, 0);
    
    //var monthCmd = new MF.Command('查看月报', 'images\\月报_03.png');
    //monthCmd.create(shellFrame);
    //monthCmd.registerEvent(shellFrame, function(sf) {
        //sf.CurrentPath = '周报月报\\月报';
    //})
    //monthCmd.add2TaskGroup(shellFrame, reportGrpId, 1);

    // var hasProj = MF.alias.objectType(vault, md.proj.typeAlias) !== -1;
    // if(hasProj){
    //     var projGrpId = shellFrame.TaskPane.CreateGroup('项目信息查询', -5);
    //     var projInfoCmd = new MF.Command('项目概况', "ico/项目概况.ico");
    //     projInfoCmd.create(shellFrame);
    //     projInfoCmd.registerEvent(shellFrame, function(sf) {
    //         sf.CurrentPath = '项目信息\\项目概况';
    //     });
    //     projInfoCmd.add2TaskGroup(shellFrame, projGrpId, 0);
        
    //     var projMemberCmd = new MF.Command('成员列表', "ico/成员列表.ico");
    //     projMemberCmd.create(shellFrame);
    //     projMemberCmd.registerEvent(shellFrame, function(sf) {
    //         sf.CurrentPath = '项目信息\\成员列表';
    //     });
    //     projMemberCmd.add2TaskGroup(shellFrame, projGrpId, 1);
        
        // var projReportCmd = new MF.Command('项目报表', "");
        // projReportCmd.create(shellFrame);
        // projReportCmd.registerEvent(shellFrame, function(sf) {
        //     sf.CurrentPath = '项目信息\\项目报表';
        // });
        // projReportCmd.add2TaskGroup(shellFrame, projGrpId, 3);
   // } 
}