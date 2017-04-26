/*******************
 * M-Files Alias Utility...
 * export Name: MF.alias
 **************************/
var MF = MF || {};
(function (v) {
    var a = {
        objectType: function (vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.ObjectTypeOperations.GetObjectTypeIDByAlias(alias);
        },
        valueList: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.ObjectTypeOperations.GetObjectTypeIDByAlias(alias);
        },
        classType: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.ClassOperations.GetObjectClassIDByAlias(alias);
        },
        propertyDef: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.PropertyDefOperations.GetPropertyDefIDByAlias(alias);
        },
        workflow: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.WorkflowOperations.GetWorkflowIDByAlias(alias);
        },
        workflowState: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.WorkflowOperations.GetWorkflowStateIDByAlias(alias);
        },
        namedACL: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.NamedACLOperations.GetNamedACLIDByAlias(alias);
        },
        usergroup: function(vault, alias) {
            var b = parseInt(alias);
            return !isNaN(b) ? b : vault.UserGroupOperations.GetUserGroupIDByAlias(alias);
        }
    };
    v.alias = a;
})(MF);