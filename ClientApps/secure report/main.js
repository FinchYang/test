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
    return function () {       
        var vault = shellFrame.ShellUI.Vault;
        if (shellFrame.CurrentPath === '安全台账') {              
                 var customData = {Vault:vault};
                 shellFrame.RightPane.Visible = false;
                    //    shellFrame.ShellUI.ShowPopupDashboard ("doccategory",false, customData);
                    shellFrame.ShowDashboard("doccategory", customData);
        }
         try {
            var group = shellFrame.TaskPane.CreateGroup('安全台账', 1);
            var cadCmd = shellFrame.Commands.CreateCustomCommand("安全台账");
            shellFrame.Commands.SetIconFromPath(cadCmd, "1.ico");
            shellFrame.TaskPane.AddCustomCommandToGroup(cadCmd, group, 0);
            shellFrame.Commands.Events.Register(Event_CustomCommand, function (command) {
                if (command === cadCmd) {
                    if (shellFrame.CurrentPath === '') {
                        shellFrame.RightPane.Visible = false;
                        shellFrame.CurrentPath="安全台账";
                        shellFrame.RightPane.Visible = false;
                    }
               
                }
            });
        }
        catch (e) {
            shellFrame.ShellUI.ShowMessage(e);
        }

	    // if (shellFrame.CurrentPath === '') {
        //     shellFrame.RightPane.Visible = false;
        //     var customData = {Vault:vault};
        //     shellFrame.ShowDashboard("doccategory", customData);
	    // }
    };
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

