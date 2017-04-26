var wshUtils = {

    readRegValue: function (regPath) {
        ///<param>HKEY_CURRENT_USER\Software\DBWorld\Client\INSTDIR</param>
        var shell = new ActiveXObject("WScript.Shell");
        return shell.RegRead(regPath);
    },

    writeTextFile: function (filePath, contents) {
        //var tFile = tempPath + '\\' + k + currentUserId + ".txt";
        var fso = new ActiveXObject('Scripting.FileSystemObject');
        var fh;
        if (!fso.FileExists(filePath)) {
            fh = fso.CreateTextFile(filePath, 2, false);
        } else {
            var atts = fso.GetFile(filePath).Attributes;
            if (atts & 1) {
                fso.GetFile(filePath).Attributes = atts - 1;
            }
            fh = fso.OpenTextFile(filePath, 2, false);
        }
        for (var i = 0; i < contents.length; i++) {
            fh.WriteLine(contents[i]);
        }
        fh.Close();
        fso = null;
    },
    //仅能在Dashboard中使用
    getBinaryFileContentAsByteArray: function(filePath) {
        var binStream = new ActiveXObject("ADODB.Stream");
        binStream.Type = 1; //adTypeBinary
        binStream.Open();
        binStream.loadFromFile(filePath);
        var binVariant = binStream.Read();
        return new VBArray(binVariant).toArray();
    },
    

    getBinaryFileContent: function(filePath) {
        var binStream = new ActiveXObject("ADODB.Stream");
        var fs = new ActiveXObject("Scripting.FileSystemObject");
        var size = (fs.getFile(filePath)).Size;

        binStream.Type = 1; //adTypeBinary
        binStream.Open();
        binStream.loadFromFile(filePath);
        var binVariant = binStream.Read();
        var adLongVarChar = 201;
        var rs = new ActiveXObject("ADODB.Recordset");

        rs.Fields.append("mBinary", adLongVarChar, size);
        rs.open();
        rs.addNew();
        var binField = rs.Fields.Item("mBinary"); //rs("mBinary");
        binField.AppendChunk(binVariant);
        rs.update();
        return binField.value;
    },

    openFile: function (filePath) {
        var shell = new ActiveXObject("WScript.Shell");
        shell.Run('cmd /K "' + filePath + '"', 0, false);
        shell = null;
    },

    fileExists: function (filePath) {
        var fso = new ActiveXObject('Scripting.FileSystemObject');
        var ok = fso.FileExists(filePath);
        fso = null;
        return ok;
    },

    runCmd: function (cmdName, args, waited) {
        ///<summary>运行DOS命令, 如：copy</summary>
        var shell = new ActiveXObject('WScript.Shell');
        var cmd = 'cmd /c ' + cmdName;
        args = args || [];
        for (var i = 0; i < args.length; i++) {
            cmd = cmd + ' ' + '"' + args[i] + '"';
        }
        if (!waited) waited = false;
        else waited = true;
        var errorCode;
        if (args.length > 0) {
            errorCode = shell.Run(cmd, 0, waited);
        } else {
            errorCode = shell.Run(cmd);
        }
        shell = null;
        return errorCode;
    },

    runProgram: function (exeFile, args, waited) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments. []</param>
        /// <param name="waited" type="bool"> whether to wait for exit </param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject('WScript.Shell');
        var cmd = '"' + exeFile + '"';
        args = args || [];
        for (var i = 0; i < args.length; i++) {
            cmd = cmd + ' ' + '"' + args[i] + '"';
        }
        if (!waited) waited = false;
        else waited = true;
        var errorCode;
        if (args.length > 0) {
            errorCode = shell.Run(cmd, 0, waited);
        } else {
            errorCode = shell.Run(cmd);
        }
        shell = null;
        return errorCode;
    },

    runProgramWithUI: function (exeFile, args) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments. []</param>
        /// <param name="waited" type="bool"> whether to wait for exit </param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject('WScript.Shell');
        var cmd = '"' + exeFile + '"';
        args = args || [];
        for (var i = 0; i < args.length; i++) {
            cmd = cmd + ' ' + '"' + args[i] + '"';
        }
        var errorCode = shell.Run(cmd);
        shell = null;
        return errorCode;
    },

    getOsArch: function () {
        ///<summary> get Os Architecture, x86 or other </summary>
        var shell = new ActiveXObject('WScript.Shell');
        var env = shell.Environment('Process');
        var arch = env('PROCESSOR_ARCHITECTURE');
        var sysArch;
        if (arch === 'x86' || arch === 'X86') {
            sysArch = env('PROCESSOR_ARCHITEW6432');
            if (sysArch === '') {
                sysArch = 'x86';
            }
        } else {
            sysArch = arch;
        }
        return sysArch;
    }
};