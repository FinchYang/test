/*********************************
 * webapi call...
 * dependency: DBWorld.MfilesNetCore.dll, jquery
 *********************************/
var webapi = webapi || {};
(function(w, undefined) {
    //var apiHost;// = 'http://www.dbworld.cn:8080/';
    //var webHost;// = 'http://www.dbworld.cn/';
    var authKey = 'Authorization';
    var defaultApiKey = 'DBWorldWeb';
    var defaultTokenType = "Bearer";

    var client; //避免未使用时DLL被加载到Explorer进程，此处不初始化，在需要使用时再初始化

    w.getApiHost = function(vault) {
        if (!this.apiHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.apiHost = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'ApiHost');
        }
        return this.apiHost;

    };


    w.getToken = function (vault, userName, password) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.GetToken(apiHost, "Token", userName, password, defaultApiKey);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
    };

    w.getWorkflow = function(vault, projId, workflowId, token) {
    	if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Vault/' + projId+'?workflow='+workflowId, defaultApiKey, token);
        return res;
    };    
    
    w.getUserPrivate = function (vault, token) {
        //return: { UserName , Password }
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Account/UserPrivate', defaultApiKey, token);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    return resJson.response;
        //}
    };    
    
})(webapi);