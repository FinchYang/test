/*******************************************************
* M-Files Client Operation, no need Vault.
* like M-Files Version, Drive Letter etc...
* export name: MF
********************************************************/
var MF = MF || {}; //必须预先声明，否则M-Files会报未定义变量
(function (u, undefined) {
    u._clientApp = null;

    u._driveStr = null; //盘符

    u.createObject = function(objDesc) {
        /*
         * @param {objDesc} 示例：MFilesClientApplication
         */
        var obj;
	    try {
		    obj = MFiles.CreateInstance(objDesc);
	    } catch(e) {
		    obj = new ActiveXObject('MFilesAPI.'+objDesc);
	    }
	    return obj;
    };

    u._getClient = function () {
        if (this._clientApp === null) {
            this._clientApp = this.createObject('MFilesClientApplication');
        }
        return this._clientApp;
    };

    u.getMFilesVersion = function () {
        ///<returns type="MFilesVersion"> </returns>
        var clientApp = this._getClient();
        return clientApp.GetClientVersion();
    };

    u.getDriveLetter = function () {
        ///<summary>获取盘符</summary>
        if (this._driveStr === null) {
            var clientApp = this._getClient();
            this._driveStr = clientApp.GetDriveLetter();
        }
        return this._driveStr;
    };

    u.showBalloonTip = function (msg, title, timeOut, infoFlags, removePre) {
        var clientApp = this._getClient();
        timeOut = timeOut || 10;
        infoFlags = infoFlags || 0;
        removePre = removePre || true;
        clientApp.ShowBalloonTip(msg, title, timeOut, infoFlags, removePre);
    };

    u.getConnections = function() {
        var clientApp = this._getClient();
        var conns = clientApp.GetVaultConnections();
        var vs = [];
        for (var i = conns.Count; i > 0; i--) {
            var c = conns.Item(i);
            var v = {
                name: c.Name,
                guid: c.GetGUID(),
                host: c.NetworkAddress
            };
            vs.push(v);
        }
        return vs;
    };

    u.getVault = function (guid) {
        var clientApp = this._getClient();
        var conns = clientApp.GetVaultConnectionsWithGUID(guid);
        if (conns.Count == 0) return null;
        var vault = conns.Item(1).BindToVault(0, true, true);
        return vault;
    };
} (MF));