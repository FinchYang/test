/****************************************
 * 协同云基础
 ****************************************/

var CC = CC || {};
(function (u, undefined) {
     
    u.listingThemeObject = {
        //// Listing area colors for texts and backgrounds for listing items.
        ////item_TextColor_Hot: '#000000',//'#ffffff',
        ////item_TextColor_Selected: '#000000',//'#ffffff',
        ////item_TextColor_HotSelected: '#000000',//'#ffffff',
        ////item_TextColor_SelectedNoFocus: '#000000',//'#ffffff',
        //item_BackgroundColor_Hot: '#b7d8f3',
        //item_BackgroundColor_Selected: '#f5c779',
        //item_BackgroundColor_HotSelected: '#ffe2b0',
        //item_BackgroundColor_SelectedNoFocus: '#f5c779',

        //// Listing area colors for texts and backgrounds for grouping headers.
        //groupHeader_LabelColor: '#ffffff',
        //groupHeader_LineColor: '#ffffff',
        //groupHeader_ButtonTextColor: 'default',
        //groupHeader_ButtonEdgeHighlightColor: 'default',
        //groupHeader_ButtonHighlightColor: 'default',
        //groupHeader_BackgroundColor: '#000000',
        //groupHeader_BackgroundColor_Hot: '#000000',
        //groupHeader_BackgroundColor_Selected: '#000000',		
        //groupHeader_BackgroundColor_HotSelected: '#000000',

        // Listing area colors for sorting headers (main headers).
        //sortableHeader_DividerColor_Inactive: '#000000',
        //sortableHeader_DividerColor_Active: '#000000',
        sortableHeader_BackgroundColor_Inactive: '#c9caca', //'#3589c4',
        sortableHeader_BackgroundColor_Active: '#c9caca',//'#1d639d',

        // The listing area background image.
        //backgroundImage: '',

        last: 0
    };
    u.commThemeObject = {
        sortableHeader_BackgroundColor_Inactive: '#2276bc',
        sortableHeader_BackgroundColor_Active: '#0087d1'
    };

    u.SetListingTheme = function(listingObj) {
        listingObj.SetTheme(this.listingThemeObject);
    };

    u.SetCommTheme = function (listingObj) {
        listingObj.SetTheme(this.commThemeObject);
    };

    u.getInstallPath = function (vault) {
        ///<summary>获取DBWorld客户端的安装路径</summary>
        if (vault) {
            var ns = 'DBWorld.' + vault.SessionInfo.UserID;
            this._installPath = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, "InstallPath");
        }
        try {
            this._installPath = wshUtils.readRegValue('HKCU\\Software\\DBWorld\\Client\\INSTDIR');
        } catch (e) {
            this._installPath = '';
        }
        return this._installPath;
    };

    

    u.getUserId = function(vault) {
        if (this.userId === undefined) {
            try {
                var ns = 'DBWorld.' + vault.SessionInfo.UserID;
                this.userId = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'DBUserId');
            } catch (e) {
                MFiles.ReportException(e);
                this.userId = null;
            }
        }
        return this.userId;
    };

    u.getUserEmail = function(vault) {
        if (this.userEmail === undefined) {
            try {
                var ns = 'DBWorld.' + vault.SessionInfo.UserID;
                this.userEmail = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'UserEmail');
            } catch (e) {
                MFiles.ReportException(e);
                this.userEmail = null;
            }
        }
        return this.userEmail;
    };

    u.getToken = function (vault) {
        ///<summary>获取token</summary>
        if (this.token === undefined) {
            try {
                var ns = 'DBWorld.' + vault.SessionInfo.UserID;
                this.token = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'UserToken');
            } catch (e) {
                MFiles.ReportException(e);
                this.token = null;
            }
        }
        return this.token;
    };

    u.isCloudAppEnabled = function (vault) {
        ///<summary>当前用户是否启用了云应用</summary>
        if (this.cloudAppEnabled === undefined) {
            try {
                var ns = 'DBWorld.' + vault.SessionInfo.UserID;
                this.cloudAppEnabled = MF.getNamedValue(vault, MFUserDefinedValue, ns, 'CloudAppEnabled');
            } catch (e) {
                this.cloudAppEnabled = null;
            }
        }
        return this.cloudAppEnabled;
    };

    u.getProjectId = function(vault) {
        ///<summary>获取当前库的项目ID</summary>
        if (this.projId === undefined) {
            try {
                var ns = 'DBWorld.' + vault.SessionInfo.UserID;
                this.projId = MF.vault.getNamedValue(vault, MFUserDefinedValue, ns, 'ProjectId');
            } catch (e) {
                MFiles.ReportException(e);
                this.projId = null;
            }
        }
        return this.projId;
    };
    
    // mfAlias.js, metadataAlias.js, client.js
    u.getProjectName = function(vault) {
        var projType = MF.alias.objectType(vault, md.proj.typeAlias);
        var projClass = MF.alias.classType(vault, md.proj.classAlias);

        var scs = MF.createObject('SearchConditions');

        MF.vault.addBaseConditions(scs, projType, projClass, false);

        var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlagNone, false);

        if (res.Count > 0) {
            return res.Item(1).Title;
        }
    };

    u.getView = function(vault, guid) {
        this.views = this.views || {};
        if (!this.views[guid] && this.views[guid] !== 0) {
            var vId = vault.ViewOperations.GetViewIDByGUID(guid);
            this.views[guid] = vId;
        }
        return this.views[guid];
    };

    u.getCloudAppUrl = function (vault) {
        //云应用服务器
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

})(CC);

/*****************************
 * 获取当前用户的项目角色
 *****************************/
var CC = CC || {};
(function (u, undefined) {
    var role = {
        //判断是否中民设计部
        isZmDesign: function (vault) {
            if(vault.SessionInfo.AccountName === "admin") return true;
            var ugId = MF.alias.usergroup(vault, md.userGroups.ZmDesign);
            return this.hasRole(vault, ugId);
        },
        //判断是否项目经理
        isPM: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.PM);
            return this.hasRole(vault, ugId);
        },
          //判断是否工程管理部成员
        isEMD: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.EMD);
            return this.hasRole(vault, ugId);
        },
         isSG: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.SG);
            return this.hasRole(vault, ugId);
        },
         isSPMD: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.SPMD);
            return this.hasRole(vault, ugId);
        },
         isGM: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.GM);
            return this.hasRole(vault, ugId);
        },
         isCC: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.CC);
            return this.hasRole(vault, ugId);
        },
         isCE: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.CE);
            return this.hasRole(vault, ugId);
        },
        
        //公司总部工程管理部
        isHEMD:function(vault){
            var ugId = MF.alias.usergroup(vault, md.userGroups.HEMD);
            return this.hasRole(vault, ugId);
        },
      
        //项目副理
        isVicePM: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.VicePM);
            return this.hasRole(vault, ugId);
        },
        //一般成员
        isMember: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.Member);
                 return this.hasRole(vault, ugId);
        },
        //外包成员
        isOutsourceMember: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.Outsource);
            return this.hasRole(vault, ugId);
        },
        //是否包含某角色: ugId,角色对应的用户组
        hasRole: function (vault, ugId) {
            var flag = false;
            var ugs = this._getUserGroups(vault);
            for (var i = 0; i < ugs.length; i++) {
                if (ugs[i] === ugId) {
                    flag = true;
                    break;
                }
            }
            return flag;
        },
        _getUserGroups: function (vault) {
            this.groups = this.groups || [];
            if (this.groups.length === 0) {
                var ugs = vault.SessionInfo.UserAndGroupMemberships;
                for (var i = 1; i <= ugs.Count; i++) {
                    var ug = ugs.Item(i);
                    if (ug.UserOrGroupType === MFUserOrUserGroupTypeUserGroup && ug.UserOrGroupID > 100) {
                        this.groups.push(ug.UserOrGroupID);
                    }
                }
            }
            return this.groups;
        },
        //获取当前用户角色用户组[{ID, Name}]
        getUserRoles: function(vault){
            this.userRoles = this.userRoles || [];
            if(this.userRoles.length === 0){
                var ugIds = this._getUserGroups(vault);
                var groups = this._getAllGroups(vault);
                for(var i = 0; i < ugIds.length; i++){
                    var index = this._indexOf(groups, "ID", ugIds[i]);
                    if(index === -1) continue;
                    this.userRoles.push(groups[index]);
                }
            }
            return this.userRoles;
        },
        _getAllGroups: function(vault){
            this.allGroups = this.allGroups || [];
            if(this.allGroups.length === 0){
                var values = vault.ValueListItemOperations.GetValueListItems(MFBuiltInValueListUserGroups);
                for (var i = 1; i <= values.Count; i++) {
                    var item = values.Item(i);
                    if (!item.Deleted) {
                        this.allGroups.push({
                            "ID": item.ID,
                            "Name": item.Name
                        });
                    }
                }
            }            
            return this.allGroups;
        },
        _indexOf: function(src, keyName, kValue) {
            var index = -1;
            for (var i = 0; i < src.length; i++) {
                var item = src[i][keyName];
                if (!keyName) {
                    item = src[i];
                }
                if (item === kValue) {
                    index = i;
                    break;
                }
            }
            return index;
        }
    };
    u.userRole = role;
})(CC);