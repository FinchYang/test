/**
 * 添加附件相关
 * 依赖文件: alias.js, objectOps.js
 */
var CC = CC || {};
(function(u, undefined){
     var attachment = {
         setCmdState: function(shellFrame, cmdId){
            var selection = shellFrame.ActiveListing.CurrentSelection;
            if(selection.Count> 0 && selection.ObjectVersions.Count === 1){
                shellFrame.Commands.SetCommandState(cmdId, CommandLocation_All, CommandState_Active);
            }else{
                shellFrame.Commands.SetCommandState(cmdId, CommandLocation_All, CommandState_Hidden);
            }
         },
         attachCmdCall: function(shellFrame){
             //shellFrame.ShellUI.ShowMessage("未实现");
             var vault = shellFrame.ShellUI.Vault;
             var selection = shellFrame.ActiveListing.CurrentSelection;
             if(selection.Count> 0 && selection.ObjectVersions.Count === 1){
                 var objVn = selection.ObjectVersions.Item(1);
                 var args = [];
                 args.push(objVn.ObjVer.ID);
                 args.push(objVn.ObjVer.Type);
                 args.push("\""+vault.Name+"\"");
                 
                 this.startExeApplication("AttachFiles.exe", args);
                 //刷新
                 shellFrame.ActiveListing.RefreshListing(false, true, false);
             }
         },
         //MFiles: 本地启动应用程序,exeTitle:'XX.exe'
         startExeApplication: function(exeTitle, strArgs) {
            var appPath = MFiles.ApplicationPath; //"path\\"
            var exeFile = 'file:///' + appPath + exeTitle;
            exeFile = exeFile.replace(/\\/g, '/');
            exeFile = exeFile.replace(/{/g, '%7B');
            exeFile = exeFile.replace(/}/g, '%7D');
            exeFile = exeFile.replace(/ /g, '%20');

            var args = strArgs;
            var errorCode = this.runProgram(exeFile, args);
            if (errorCode != 0) {
               // MFiles.ThrowError(errorCode.toString());
            }
         },
         runProgram: function(exeFile, args) {
            /// <summary>Run external program.</summary>
            /// <param name="exeFile" type="String">the program file(*.exe).</param>
            /// <param name="args" type="Array">commandline arguments.</param>
            /// <returns>errorCode:0, success;other, error</returns>
            var shell = new ActiveXObject("WScript.Shell");
            var cmd = exeFile;
            if (args) {
                for (var i = 0; i < args.length; i++) {
                    cmd = cmd + ' ' + args[i];
                }
            }
            var errorCode = shell.Run(cmd, 1, true);
            shell = null;
            return errorCode;
         }
    }
    CC.attachment = attachment;
})(CC);