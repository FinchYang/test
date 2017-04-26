/**
 * 发图相关
 * 依赖文件: mfProperty.js, objectOps.js
 */
var CC = CC || {};
(function(u, undefined) {
    var drawList ={
        creare: function(shellFrame, type) {
            var sel = shellFrame.Listing.CurrentSelection;
            var hasFiles = true;
            if(sel.Count === 0){
                //shellFrame.ShellUI.ShowMessage("请先选中待发图的图纸");
                //return;
                hasFiles = false;
            }
            var items = shellFrame.Listing.CurrentSelection.ObjectVersions;
            if(items.Count === 0){
                //shellFrame.ShellUI.ShowMessage("请先选中待发图的图纸");
                //return;
                hasFiles = false;
            }
            var files = this.getSelectedFiles(items);
            if(files.length === 0){
                // shellFrame.ShellUI.ShowMessage("请先选中待发图的图纸");
                // return;
                hasFiles = false;
            }

            var vault = shellFrame.ShellUI.Vault;
            var typeId = MF.alias.objectType(vault, md.drawingList.typeAlias);
            var classId = MF.alias.classType(vault, md.drawingList.classAlias);
            
            var pvs = MFiles.CreateInstance("PropertyValues");
            var classDefId = parseInt(MFBuiltInPropertyDefClass);
            var classValue = MF.property.newLookupProperty(classDefId, classId);
            pvs.Add(-1, classValue);

            if(hasFiles){
                var filesDefId = MF.alias.propertyDef(vault, "PropDrawings");
                var filesValue = MF.property.newMultiSelectLookupProperty(filesDefId, files);
                pvs.Add(-1, filesValue);
            }
        
            var wf = this.getWorkFlow(vault, type);
            if(wf.flow){
                var wfDefId = parseInt(MFBuiltInPropertyDefWorkflow);
                var wfValue = MF.property.newLookupProperty(wfDefId, wf.flow);
                pvs.Add(-1, wfValue);
            }
            if(wf.state){
                var wfStateDefId = parseInt(MFBuiltInPropertyDefState);
                var wfStateValue = MF.property.newLookupProperty(wfStateDefId, wf.state);
                pvs.Add(-1, wfStateValue);
            }

            var ugIdZmCost = MF.alias.usergroup(vault, md.userGroups.ZmCost);
            if(ugIdZmCost !== -1){
                var costArr = [];
                costArr.push(ugIdZmCost);
                var ccDefId = MF.alias.propertyDef(vault, md.drawingList.propDefs.PropCcGroup);
                var ccValue = MF.property.newMultiSelectLookupProperty(ccDefId, costArr);
                pvs.Add(-1, ccValue);
            }
            var ugIdZmEngineer = MF.alias.usergroup(vault, md.userGroups.ZmEngineering);
            if(ugIdZmEngineer !== -1){
                var receiveDefId = MF.alias.propertyDef(vault, md.drawingList.propDefs.PropReceiveGroup);
                var receiveValue = MF.property.newLookupProperty(receiveDefId, ugIdZmEngineer);
                pvs.Add(-1, receiveValue);
            }
            MF.ObjectOps.createObjPrefilled(vault, pvs, typeId, undefined);
        },
        getSelectedFiles: function (ObjectVersions) {
            var res = [];
            for(var i = 1; i <= ObjectVersions.Count; i++){
                var item = ObjectVersions.Item(i);
                if(item.ObjVer.Type === 0){
                    res.push(item.ObjVer.ID);
                }
            }
            return res;
        },
        getWorkFlow: function(vault, opType) {
            var res = {
                flow: -1,
                state: -1
            }
            if(opType){
                res.flow = MF.alias.workflow(vault, "WFEngineerSending");
                res.state = MF.alias.workflowState(vault, "WFSEngineerSendDraft");
            }else{
                res.flow = MF.alias.workflow(vault, "WFDesignSending");
                res.state = MF.alias.workflowState(vault, "WFSDesginSendDraft");
            }
            return res;
        },
        getSendingPhase: function (vault, opType) {
            var res = -1;
            var vlId = MF.alias.valueList(vault, "VLDrawSendPhase");
            if(vlId){
                if(!opType){
                    res = this._getValueListItemId(vault, vlId, "发图");
                }else{
                    res = this._getValueListItemId(vault, vlId, "指令");
                }
            }
            return res;
        },
        _getValueListItemId: function(vault, valueListId, itemName) {
            ///<summary>根据值列表项的名称获取对应的值列表项ID</summary>
            ///<param name="valueListId" type="long">值列表的ID</param>
            ///<param name="itemName" type="String">值列表项的名称</param>
            var values = vault.ValueListItemOperations.GetValueListItems(valueListId);
            var id;
            for (var i = 1; i <= values.Count; i++) {
                if (values.Item(i).Name === itemName) {
                    id = values.Item(i).ID;
                    break;
                }
            }
            return id;
        }
    }
    u.drawList = drawList;
})(CC);