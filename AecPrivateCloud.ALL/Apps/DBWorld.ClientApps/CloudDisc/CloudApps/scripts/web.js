/*********************************
 * webapi call...
 * dependency: DBWorld.MfilesNetCore.dll, jquery
 *********************************/
var webapi = webapi || {};
(function(w, undefined) {
    var authKey = 'Authorization';
    var defaultApiKey = 'DBWorldWeb';
    var defaultTokenType = "Bearer";

    var client; //避免未使用时DLL被加载到Explorer进程，此处不初始化，在需要使用时再初始化

    w.getApiHost = function(vault) {
        if (!this.apiHost) {
            this.apiHost = cd.getApiHost(vault);
        }
        return this.apiHost;

    };

    w.getWebHost = function (vault) {
        if (!this.webHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.webHost = cd.getNamedValue(vault, MFUserDefinedValue, ns, 'WebHost');
        }
        return this.webHost;

    };

    w.getProjHost = function(vault) {
        if (!this.projHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.projHost = cd.getNamedValue(vault, MFUserDefinedValue, ns, 'ProjHost');
        }
        return this.projHost;
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

    w.getSofts = function(vault, token) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Cloud/Softwares', defaultApiKey, token);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    return resJson.response;
        //}
    };

    w.openSoft = function(cloudUrl, requestUrl, formData) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var res = client.PostWithoutAuthWithFormData(cloudUrl, requestUrl, formData);
        return res;
    }

    w.updateProj = function(vault, model, token) {
        ///<summary></summary>
        ///<param name="model" type="ProjectEditModel"></param>
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var jsonStr = JSON.stringify(model);
        var res = client.Post(apiHost, 'api/Project/Update', defaultApiKey, token, jsonStr);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 201) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
    };

    w.getPartyFromName = function(vault, partyName, token) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Project/GetPartyByName/?name=' + partyName, defaultApiKey, token);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
    };

    w.inviteMember = function (vault, model, token) {
        ///<summary></summary>
        ///<param name="model" type="InviteModel"></param>
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var modelJson = JSON.stringify(model);
        var res = client.Post(apiHost, 'api/Invite', defaultApiKey, token, modelJson);//client.PostWithoutAuth(webHost, 'api/Invite', modelJson);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
    };

    w.removeMember = function (vault, model, token) {
        ///<summary></summary>
        ///<param name="model" type="ProjectMemberModel"></param>
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var modelJson = JSON.stringify(model);
        var res = client.Post(apiHost, 'api/ProjectMembers/RemoveMember', defaultApiKey, token, modelJson);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
    };

    w.shareFile2Web = function(vault, model, token) {
        ///<summary></summary>
        ///<param name="model" type="ProjectEditModel"></param>
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var jsonStr = JSON.stringify(model);
        var res = client.Post(apiHost, 'api/Files/Share', defaultApiKey, token, jsonStr);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 201) {
        //    //ok
        //} else {
        //    //error
        //    return resJson.response;
        //}
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