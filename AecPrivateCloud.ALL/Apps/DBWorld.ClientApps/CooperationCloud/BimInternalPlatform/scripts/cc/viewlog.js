/****************************************
 * 查看工作日志
 ****************************************/

var CC = CC || {};
(function (u, undefined) {

    u.viewlog = {
        //获取listing需要显示的列
        GetArray: function (vault) {
            var res = [];
            var array = md.jobLog.propDefs;
            for (var p in array) {
                if (array[p] === md.jobLog.propDefs.JobTask
                    || array[p] === md.jobLog.propDefs.LogTitle
                    || array[p] === md.jobLog.propDefs.JobDate
                    || array[p] === md.jobLog.propDefs.JobTime) {
                    var propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            }
            return res;
        },
        CreateWorkLog: function (vault) {
            var typeId = MF.alias.objectType(vault, md.jobLog.typeAlias);
            var objType = vault.ObjectTypeOperations.GetObjectType(typeId);
            vault.ObjectOperations.ShowBasicNewObjectWindow(0, objType);
        }
    }

})(CC);