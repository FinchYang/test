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

})(CC);

/*****************************
 * 获取当前用户的项目角色
 *****************************/
var CC = CC || {};
(function (u, undefined) {
    var role = {
        //判断是否项目经理
        isPM: function (vault) {
            var ugId = MF.alias.usergroup(vault, md.userGroups.PM);
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
            if (this.groups.length == 0) {
                var ugs = vault.SessionInfo.UserAndGroupMemberships;
                for (var i = 1; i <= ugs.Count; i++) {
                    var ug = ugs.Item(i);
                    if (ug.UserOrGroupType === MFUserOrUserGroupTypeUserGroup && ug.UserOrGroupID > 100) {
                        this.groups.push(ug.UserOrGroupID);
                    }
                }
            }
            return this.groups;
        }
    };
    u.userRole = role;
})(CC);