/****************************************
 * 协同云 邀请成员
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var invite = {
        //作用：从库中搜索其他参与方
        //参数：vault
        GetAllParties: function (vault) {
            var typeIdPart = MF.alias.objectType(vault, md.participant.typeAlias);
            var sResults = MF.ObjectOps.SearchObjectsByType(vault, typeIdPart);
            var parts = [];
            for (var i = 1; i <= sResults.Count; i++) {
                var item = sResults.Item(i);
                var part = {
                    'ID': item.ObjVer.ID,
                    'Title': item.Title
                }
                parts.push(part);
            }
            return parts;
        }
    };
    u.invite = invite;
})(CC);