var CC = CC || {};
(function(u, undefined) {
    var alltask = {
        GetArray: function (vault) {
            var res = [];
            var array = md.genericTask.propDefs;
            for (var p in array) {
                if (array[p] === md.genericTask.propDefs.TaskTitle
                    || array[p] === md.genericTask.propDefs.AssignedTo
                    || array[p] === md.genericTask.propDefs.StartDate
                    || array[p] === md.genericTask.propDefs.Deadline) {
                    var propId = MF.alias.propertyDef(vault, array[p]);
                    res.push(propId);
                }
            } 
            return res;
        }
    };
    u.alltask = alltask;
})(CC);
