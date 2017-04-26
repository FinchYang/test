var cd = cd || {};
(function(a, undefined) {
    a.getToken = function (vault) {
        if (!this._userToken) {
            var ns = "DBWorld." + vault.SessionInfo.UserID;
            var nvs = vault.NamedValueStorageOperations.GetNamedValues(MFUserDefinedValue,
                ns);
            this._userToken = nvs.Value('UserToken');
        }
        return this._userToken;
    };

    a.getApiHost = function (vault) {
        if (!this._apiHost) {
            var ns = "DBWorld." + vault.SessionInfo.UserID;
            var nvs = vault.NamedValueStorageOperations.GetNamedValues(MFUserDefinedValue,
                ns);
            this._apiHost = nvs.Value('ApiHost');
        }
        return this._apiHost;
    };

    a.getCloudAppUrl = function (vault) {
        if (!this._cloudappHost) {
            var ns = "DBWorld." + vault.SessionInfo.UserID;
            var nvs = vault.NamedValueStorageOperations.GetNamedValues(MFUserDefinedValue,
                ns);
            this._cloudappHost = nvs.Value('CloudApp');
        }
        return this._cloudappHost;
        //return "http://192.168.2.101";
        //return "http://211.152.38.103";
        //return "http://vapp.dbworld.cn";
    };

}(cd));