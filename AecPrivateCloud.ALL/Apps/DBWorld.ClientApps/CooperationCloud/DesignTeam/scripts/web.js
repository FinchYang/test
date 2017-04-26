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

    w.getBase64StrFromFile = function(filePath) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        return client.GetBase64(filePath);
    };

    w.getApiHost = function(vault) {
        if (!this.apiHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.apiHost = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'ApiHost');
        }
        return this.apiHost;

    };

    w.getWebHost = function (vault) {
        if (!this.webHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.webHost = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'WebHost');
        }
        return this.webHost;

    };

    w.getProjHost = function(vault) {
        if (!this.projHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.projHost = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'ProjHost');
        }
        return this.projHost;
    };

    w.getProjWebHost = function(vault) {
        if (!this.projWebHost) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this.projWebHost = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'ProjWebHost');
        }
        return this.projWebHost;
    }

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

    w.getProjectCover = function(vault, projId, token) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Project/GetImage/' + projId, defaultApiKey, token);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    return resJson.response;
        //}
    };

    w.getProject = function (vault, projId, token) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var res = client.Get(apiHost, 'api/Project/AllProjects/' + projId, defaultApiKey, token);
        return res;
        //var resJson = JSON.parse(res);
        //if (resJson.status === 200) {
        //    //ok
        //} else {
        //    return resJson.response;
        //}
    };

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

    w.createContactProj = function(vault, model, token) {
        if (!client) {
            client = MFiles.CreateObjectCLR('DBWorld.MfilesNetCore.dll', 'DBWorld.MfilesNetCore.GeneralClient');
        }
        var apiHost = this.getApiHost(vault);
        var modelJson = JSON.stringify(model);
        var res = client.Post(apiHost, 'api/Project/Contract', defaultApiKey, token, modelJson); //client.PostWithoutAuth(webHost, 'api/Invite', modelJson);
        return res;
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

    /*

    $.ajaxSetup({
        beforeSend: function (xhr) {
            xhr.setRequestHeader("ApiKey", apiKey);
        }
    });

    w.getToken = function(userName, password) {

    };

    w._post = function(url, token, data, tokenType, callback) {
        tokenType = tokenType || defaultTokenType;
        $.ajax({
            type: "POST",
            beforeSend: function(request) {
                request.setRequestHeader(authKey, tokenType + ' ' + token);
            },
            url: url,
            data: data,
            //processData: false,
            success: function(msg) {
                callback(msg);
            }
        });
    };

    w.updateProj = function (token, editModel, tokenType, callback) {
        this._post(apiHost + 'api/Project/Update', token, editModel, tokenType, callback);
    };

    w.inviteMember = function (token, inviteModel, tokenType, callback) {
        this._post(webHost + 'api/Invite', inviteModel, tokenType, callback);
    };

    w.removeMember = function (token, memberModel, tokenType, callback) {
        this._post(apiHost + 'api/ProjectMembers/RemoveMember', token, memberModel, tokenType, callback);
    };
    */
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
    w.getSofts = function (vault, token) {//获取云应用列表
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
})(webapi);