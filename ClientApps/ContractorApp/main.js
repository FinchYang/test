/*******************
 * shellUI模块入口
 **************************/	
 
"use strict";

function OnNewShellUI(shellUI) {
    shellUI.Events.Register(MFiles.Event.NewShellFrame, newShellFrameHandler);
}

function newShellFrameHandler(shellFrame) {
    shellFrame.Events.Register(MFiles.Event.Started, getShellFrameStartedHandler(shellFrame));
}

/**
 * shellFrame对象启动的函数入口
 * @param {string} shellFrame shellUI的入口实例
 */
function getShellFrameStartedHandler(shellFrame) {   
        // Return the handler function for Started event.
    return function () {       
          createRootCmd(shellFrame);
          var vault = shellFrame.ShellUI.Vault;
           var customData = {Vault:vault};
          if (shellFrame.CurrentPath === ''||shellFrame.CurrentPath === "分包商管理") { 
                
                 shellFrame.RightPane.Visible = false;
                    shellFrame.ShowDashboard("doccategory", customData);
        }else if (shellFrame.CurrentPath === "根目录") {
            shellFrame.ShowDashboard("home", customData);
        }
        //  try {
        //     var group = shellFrame.TaskPane.CreateGroup('分包商管理', 1);
        //     var cadCmd = shellFrame.Commands.CreateCustomCommand("分包商管理");
        //     shellFrame.Commands.SetIconFromPath(cadCmd, "ca.ico");
        //     shellFrame.TaskPane.AddCustomCommandToGroup(cadCmd, group, 0);
        //     shellFrame.Commands.Events.Register(Event_CustomCommand, function (command) {
        //         if (command === cadCmd) {
        //             if (shellFrame.CurrentPath === '') {
        //                 shellFrame.CurrentPath="分包商管理";
        //                 shellFrame.RightPane.Visible = false;
        //             }
               
        //         }
        //     });
        // }
        // catch (e) {
        //     shellFrame.ShellUI.ShowMessage(e);
        // }
    };
}
function createRootCmd(shellFrame) {
	if (!shellFrame.TaskPane.Available) {
        return;
    }
    var rootCmdName = "根目录";
    var rootCmd = new MF.Command(rootCmdName, 'images/根目录.ico');
    rootCmd.create(shellFrame);
    rootCmd.registerEvent(shellFrame, function (sf) {
        sf.CurrentPath = rootCmdName;
    });
    rootCmd.add2TaskGroup(shellFrame, 4, -1);
}
function test(shellFrame){
    var vault = shellFrame.ShellUI.Vault;
    var doms = knowDocOps.getClassesTree(vault);

    var info ="";
    for(var i = 0; i < doms.length; i++){
        info += doms[i].Name+":";
        for(var j=0; j < doms[i].Classes.length; j++){
            info += doms[i].Classes[j].Name+";";
        }
        info += "\n";
    }
    //shellFrame.ShellUI.ShowMessage(info);
    
    shellFrame.ShellUI.ShowMessage(knowDocOps.getDisciplineList(vault).length);
}

