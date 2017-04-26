var CC = CC || {};
(function(u, undefined) {
    var mailcontacts = {

        //获取显示的列
        GetArray: function (vault) {
            var res = [];
            var array = md.emailAdressBook.propDefs;
            for (var p in array) {
                if ( array[p] === md.emailAdressBook.propDefs.Email
                    || array[p] === md.emailAdressBook.propDefs.InnerUser) {
                    var propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            }

            if (md.emailAdressBook.ownerAlias) {
                var ownerTypeId = MF.alias.objectType(vault, md.contacts.ownerAlias);
                if (ownerTypeId !== -1) {
                    var ownerType = vault.ObjectTypeOperations.GetObjectType(ownerTypeId);
                    res.push(ownerType.OwnerPropertyDef);
                }
            }
            return res;
        },
        //提示框校验
        deleteDomTip: function (shellFrame) {
            var msg = "确定删除选中内容？";
            var clickBtn = shellFrame.ShellUI.ShowMessage({
                caption: "删除提示",
                message: msg,
                icon: "warning",
                button1_title: "确定",
                button2_title: "取消",
            });
            if (clickBtn == 1) return true;
            return false;
        }
    };
    u.mailcontacts = mailcontacts;
})(CC);
