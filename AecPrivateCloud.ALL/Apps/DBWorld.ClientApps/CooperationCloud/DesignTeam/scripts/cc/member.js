/****************************************
 * 协同云项目成员
 * 
 ****************************************/

var CC = CC || {};
(function (u, undefined) {
    var member = {
       
        //作用：文件夹排序
        //参数：文件夹路径数组，文件夹ID数组
        FloderSort: function (folderPaths, folderIds) {
    
            //保存排序之后的数组
            var sortId = folderIds; 

            //最后返回出已经排好序的【文件夹路径数组】
            var newFloderPath = [];

            //将路径和ID整合到字典里
            var sortFloderPaths = {};
            for (var k = 0; k < sortId.length; k++) {
                sortFloderPaths[folderIds[k].toString()] = folderPaths[k];
            }

            sortId = member.Sort(sortId);
            
            //通过排好序的ID数组sortId,在sortFloderPaths取出对应的值（路径）
            for (var l = 0; l < sortId.length; l++) {
                newFloderPath.push(sortFloderPaths[sortId[l].toString()]);
            }

            return newFloderPath;
        },
        //作用：排序
        //参数：需要排序的数组
        Sort: function (sortId) {
            //冒泡排序从小到大
            var i, j, stop, len = sortId.length;
            for (i = 0; i < len; i = i + 1) {
                for (j = 0, stop = len - i; j < stop; j = j + 1) {
                    // 将这里的'>'换成'<'即为降序排列
                    if (sortId[j] > sortId[j + 1]) {
                        var temp = sortId[j];
                        sortId[j] = sortId[j + 1];
                        sortId[j + 1] = temp;
                    }
                }
            }
            return sortId;
        }, 
        //作用：md.objs.ObjContacts.ClassDict.ClassContacts.PropDict取出metadataAlias.js 中ClassContacts 所有值
        //参数：【vault】vault对象
        GetArray: function (vault) {
            var res = [];
            //取出metadataAlias.js 中ClassContacts 所有值 （返回值为字典集合）
            var array = md.contacts.propDefs;
            for (var p in array) {
                //只显示PropLinkmanName，PropProjectRole，PropTelPhone，PropEmail
                
                if (array[p] === md.contacts.propDefs.PropProjectRole 
                    || array[p] === md.contacts.propDefs.PropEmail) {
                    var propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            }
            var ownerTypeId = MF.alias.objectType(vault, md.contacts.ownerAlias);
            if (ownerTypeId !== -1) {
                var ownerType = vault.ObjectTypeOperations.GetObjectType(ownerTypeId);
                res.push(ownerType.OwnerPropertyDef);
            }
            return res;
        },

        getAccount: function(vault, contactId) {
            var objId = MF.createObject('ObjID');
            var typeId = MF.alias.objectType(vault, md.contacts.typeAlias);
            objId.SetIDs(typeId, contactId);
            var objVer = vault.ObjectOperations.GetLatestObjVer(objId, false, false);
            var laPDId = MF.alias.propertyDef(vault, md.contacts.propDefs.PropAccount);
            var accountTV = vault.ObjectPropertyOperations.GetProperty(objVer, laPDId).Value;
            var userId = accountTV.GetLookupID();

            var partyId = MF.alias.objectType(vault, md.contacts.ownerAlias);
            var partyName = '';
            if (partyId !== -1) {
                var partyOwner = vault.ObjectTypeOperations.GetObjectType(partyId);
                var partyPdId = partyOwner.OwnerPropertyDef;
                var partyTV = vault.ObjectPropertyOperations.GetProperty(objVer, partyPdId).Value;
                partyName = partyTV.DisplayValue;
            }
            var name = accountTV.DisplayValue;
            var index = name.indexOf('\\');
            return { name: name.substring(index + 1), userId: userId, partyName: partyName }
            //var users = vault.UserOperations.GetUserList();
            //for (var i = 1; i <= users.Count; i++) {
            //    if (users.Item(i).Key === userId) {
            //        var index = users.Item(i).Name.indexOf('\\');
            //        return { name: users.Item(i).Name.substring(index + 1), userId: userId };
            //    }
            //}

        },

        accounts: {
            
        },

        getAccountByUserId: function (vault, userId) {

            if (this.accounts[userId]) {
                return this.accounts[userId];
            }

            var contactType = MF.alias.objectType(vault, md.contacts.typeAlias);
            var contactClass = MF.alias.classType(vault, md.contacts.classAlias);

            var scs = MF.createObject('SearchConditions');

            MF.vault.addBaseConditions(scs, contactType, contactClass, false);

            var laPDId = MF.alias.propertyDef(vault, md.contacts.propDefs.PropAccount);
            var sc = MF.createObject('SearchCondition');
            sc.ConditionType = MFConditionTypeEqual;
            sc.Expression.DataPropertyValuePropertyDef = laPDId;
            sc.TypedValue.SetValue(MFDatatypeLookup, userId);
            scs.Add(-1, sc);

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlagNone, false);
            if (res.Count > 0) {
                var contactId = res.Item(1).ObjVer.ID;
                var a = this.getAccount(vault, contactId);
                this.accounts[userId] = a;
                return this.accounts[userId];
            }
        },

        inviteMember: function(sf, bidProjId) {
            var vault = sf.ShellUI.Vault;
            var srcParts = [];
            if (MF.alias.objectType(vault, md.participant.typeAlias) !== -1) {
                srcParts = CC.invite.GetAllParties(vault);
            }
            var selectedPart = "";
            var currentUserId = MF.vault.getCurrentUserId(vault);
            var contact = CC.member.getAccountByUserId(vault, currentUserId);
            if (contact) {
                selectedPart = contact.partyName;
            }
            var inviteEmail = "";
            var dashboardData = { Vault: vault, SrcParts: srcParts, SelectedPart: selectedPart, InviteEmail: inviteEmail, Cancelled: true, bidProjId: 0 };
            if (bidProjId) {
                dashboardData.bidProjId = bidProjId;
            }
            sf.ShowPopupDashboard("invitemember", true, dashboardData);
        }
    };
    u.member = member;
})(CC);