var CC = CC || {};
(function(u, undefined) {
    var draft = {
        //获取显示的列
        GetArray: function (vault) {
            var res = [];
            var array = md.mail.propDefs;
            for (var p in array) {
                if (array[p] === md.mail.propDefs.PropMailReceiver
                    || array[p] === md.mail.propDefs.PropMailSubject) {
                    var propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            } 
            return res;
        }
    };
    u.draft = draft;
})(CC);
