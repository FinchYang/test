/*******************
* 文档归档
******************/
var cmdCallbackFn = cmdCallbackFn || {};

(function (fn) {
    fn.archiveFiles = function (shellFrame) {
        var that = this;
        return function () {
            //shellFrame.ShellUI.ShowMessage("未实现！");
            var objVnsAndProps = shellFrame.ActiveListing.CurrentSelection.ObjectVersionsAndProperties;
            if (objVnsAndProps.Count == 0 || objVnsAndProps.Count > 1) {
                return;
            }
            var vault = shellFrame.ShellUI.Vault;
            if (CC.userRole.isPM(vault) === false) {
                shellFrame.ShellUI.ShowMessage("您不是项目经理，无权归档！");
                return;
            }
            var canArchiveObjs = [];
            for (var i = 1; i <= objVnsAndProps.Count; i++) {
                var item = objVnsAndProps.Item(i);
                var props = item.Properties;
                var objVersion = item.VersionData;
                var pIdWflow = MFBuiltInPropertyDefWorkflow;
                if (props.IndexOf(pIdWflow) === -1 || that._validateReviewFlow(vault, props)) {
                    canArchiveObjs.push(objVersion);
                }
            }
            if (objVnsAndProps.Count != canArchiveObjs.length) {
                shellFrame.ShellUI.ShowMessage("不可选择正在校审的文档！");
                return;
            }
            if (canArchiveObjs.length) {
                var clickBtn = shellFrame.ShellUI.ShowMessage({
                    caption: "归档提示",
                    message: "您确定要归档选中的文档？",
                    icon: "question",
                    button1_title: "确定",
                    button2_title: "取消",
                    defaultButton: 1,
                    timeOutButton: 2,
                    timeOut: 30
                });
                if (clickBtn == 2) return;
                for (var j = 0; j < canArchiveObjs.length; j++) {
                    var objVn = canArchiveObjs[j];
                    var resVn = that._markArchived(shellFrame, objVn);
                    if (resVn) {
                        that._setArchivedDocPermissions(vault, resVn.ObjVer);
                    };
                }
            }
        }
    };
    //如果是校审流程，则只有在"完成校审"状态下，才能归档
    fn._validateReviewFlow = function (vault, props) {
        var pIdWflow = MFBuiltInPropertyDefWorkflow;
        var pIdFlowState = MFBuiltInPropertyDefState;
        var wFlowId = MF.alias.workflow(vault, md.reviewFlow.alias);
        var fState = MF.alias.workflowState(vault, md.reviewFlow.stateAlias.ReviewPass);
        if (props.IndexOf(pIdWflow) === -1) return true;
        if (props.IndexOf(pIdWflow) > -1) {
            var tvalue = props.SearchForProperty(pIdWflow).Value;
            if (tvalue.IsNULL()) return true;
            if (tvalue.GetLookupID() !== wFlowId) return true;
            var tvalueState = props.SearchForProperty(pIdFlowState).Value;
            if (tvalueState.IsNULL() == false && tvalueState.GetLookupID() == fState) {
                return true;
            }
        }
        return false;
    };
    fn._markArchived = function (shellFrame, objVersion) {
        var vault = shellFrame.ShellUI.Vault;
        var propIdStatus = MF.alias.propertyDef(vault, md.sharingDoc.propDefs.DocStatus);

        var prfillProps = new MFiles.PropertyValues();
        //文档状态
        if (!this.docArchiveStatus) {
            var valueListId = MF.alias.valueList(vault, md.valueList.DocStatus);
            this.docArchiveStatus = MF.vault.getValueListItemId(vault, valueListId, "归档");
        }
        var pvStatus = new MFiles.PropertyValue();
        pvStatus.PropertyDef = propIdStatus;
        pvStatus.TypedValue.SetValue(MFDatatypeLookup, this.docArchiveStatus);
        prfillProps.Add(-1, pvStatus);
        try {
            var obj = MF.ObjectOps.updateObject(vault, objVersion, prfillProps);
            return obj;
        } catch (e) {
            return null;
        }
    };
    /// 设置对象权限（通过命名访问权限列表）,只读
    fn._setArchivedDocPermissions = function (vault, oObjVer) {
        var namedAcl = MF.alias.namedACL(vault, "NaclAllInnerRead");
        vault.ObjectOperations.ChangePermissionsToNamedACL(oObjVer, namedAcl, false);
    };
})(cmdCallbackFn);