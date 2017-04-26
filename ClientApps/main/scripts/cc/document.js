/**
 * 转发、工程联系单回复
 * 依赖文件: alias.js, mfProperty.js, objejctOps.js, core.js
 */
var CC = CC || {};
(function(u, undefined) {
    var document ={
        forwardDoc: function(shellFrame){
            var sel = shellFrame.ActiveListing.CurrentSelection;
            var hasFiles = true;
            if(sel.Count === 0) hasFiles = false;
            var items = shellFrame.ActiveListing.CurrentSelection.ObjectVersions;
            if(items.Count === 0) hasFiles = false;
            var fileset = this.getSelectedFiles(items);
            var files = fileset.files;
            var title = "转发："+fileset.title;
            if(files.length === 0) hasFiles = false;
            if(!hasFiles){
                this.showMessage(shellFrame,"请选中待转发的工作流文档");
                return;
            }
            var vault = shellFrame.ShellUI.Vault;
            var typeId = MF.alias.objectType(vault, md.genericTask.typeAlias);
            var classId = MF.alias.classType(vault, md.genericTask.classAlias);
            
            var pvs = MFiles.CreateInstance("PropertyValues");
            var classDefId = parseInt(MFBuiltInPropertyDefClass);
            var classValue = MF.property.newLookupProperty(classDefId, classId);
            pvs.Add(-1, classValue);

            var titleValue = MF.property.newTextProperty(0, title);
            pvs.Add(-1, titleValue);

            var filesDefId = MF.alias.propertyDef(vault, "PropForwardedFile");
            if(filesDefId > 0){
                var filesValue = MF.property.newMultiSelectLookupProperty(filesDefId, files);
                pvs.Add(-1, filesValue);
            }      
            MF.ObjectOps.createObjPrefilled(vault, pvs, typeId, undefined);
        },
        responseDoc: function (shellFrame, userRoles) { 
            var sel = shellFrame.ActiveListing.CurrentSelection;
            var vault = shellFrame.ShellUI.Vault;

            var contacts = this.getContactDocs(vault);
            var partyRoles = this.getPartyRoles(userRoles);
            var docClass = this.getDocClass(contacts, partyRoles);
            //shellFrame.ShowMessage("userRoles:"+userRoles.length+"-docClass:"+docClass.length);

            var hasFiles = true;
            if(sel.Count !== 1) hasFiles = false;
            var items = shellFrame.ActiveListing.CurrentSelection.ObjectVersions;
            if(items.Count !== 1) hasFiles = false;
            if(items.Count >= 1){
                var objVn = items.Item(1);
                if(objVn.ObjVer.Type !== 0) hasFiles = false;
                if(hasFiles){
                    var className = vault.ObjectPropertyOperations.GetProperty(objVn.ObjVer, 100).Value.DisplayValue;
                    if(className.search(/工作联系单/) === -1) hasFiles = false;
                }
                if(hasFiles){
                    if(!this.hasWorkFlow(vault, objVn)) hasFiles = false;
                }
            }
            if(!hasFiles){
                this.showMessage(shellFrame,"请选中一个待回复的工程联系单");
                return;
            }
            var files = [];
            files.push(objVn.ObjVer.ID);
            var title = "回复："+objVn.title;
  
            var typeId = objVn.ObjVer.Type;
            //var classId = objVn.Class;
            
            var pvs = MFiles.CreateInstance("PropertyValues");

            if(docClass.length === 1){
                var classDefId = parseInt(MFBuiltInPropertyDefClass);
                var classValue = MF.property.newLookupProperty(classDefId, docClass[0]);
                pvs.Add(-1, classValue);
            }

            var titleValue = MF.property.newTextProperty(0, title);
            pvs.Add(-1, titleValue);

            var filesDefId = MF.alias.propertyDef(vault, "PropResponsedFile");
            if(filesDefId > 0){
                var filesValue = MF.property.newMultiSelectLookupProperty(filesDefId, files);
                pvs.Add(-1, filesValue);
            }          
            MF.ObjectOps.createObjPrefilled(vault, pvs, typeId, undefined, true);
         },

        getSelectedFiles: function (ObjectVersions) {
            var res = {
                title: "",
                files: []
            };
            for(var i = 1; i <= ObjectVersions.Count; i++){
                var item = ObjectVersions.Item(i);
                if(item.ObjVer.Type === 0){
                    res.files.push(item.ObjVer.ID);
                    if(!res.title) res.title= item.Title;
                }
            }
            return res;
        },
        hasWorkFlow: function(vault, objVersion){
            var res = false;
            var wf = vault.ObjectPropertyOperations.GetWorkflowState(objVersion.ObjVer, false);
            if(wf.State.Value.DisplayValue){
                 res = true;
            }
            return res;
        },
        showMessage: function(shellFrame, title){
            shellFrame.ShellUI.ShowMessage({
                caption: "MFiles提示",
                message: title,
                icon: "warning",
                button1_title: "确定",
                defaultButton: 1,
                timeOutButton: 1,
                timeOut: 30
            });
        },
        //获取当前参与方的工作联系单
        getDocClass: function(contactDocs, partyRoles){
            var res = [];
            for (var i = 0; i < contactDocs.length; i++) {
                var doc = contactDocs[i];
                if(this._searchIndex(partyRoles, doc.Name) !== -1){
                    res.push(doc.ID);
                }
            }
            return res;
        },
        //获取参与方
        getPartyRoles: function(userGroups){
            var res = [];
            for (var i = 0; i < userGroups.length; i++) {
                var g = userGroups[i];
                var index = g.Name.search(/(监理|监测|检测|总包|业主|中民)/i);
                if(index !== -1){
                    var role = g.Name.substring(index, index+2);
                    if(this._indexOf(res, "", role) === -1){
                        res.push(role);
                    }
                }
            }
            return res;
        },
        //获取工作联系单
        getContactDocs: function(vault){
            this.contactDocs = this.contactDocs || [];
            if(this.contactDocs.length === 0){
                var objClasses = this._getObjClasses(vault);
                for(var i=0; i < objClasses.length; i++){
                    var item = objClasses[i];
                    if(item.Name.search(/工作联系单/) !== -1){
                        this.contactDocs.push(item);
                    }
                }
            }
            return this.contactDocs;
        },
        _getObjClasses: function(vault) {
            this.objClasses = this.objClasses || [];
            if(this.objClasses.length === 0){
                var values = vault.ValueListItemOperations.GetValueListItems(MFBuiltInValueListClasses);
                for (var i = 1; i <= values.Count; i++) {
                    var item = values.Item(i);
                    if (!item.Deleted) {
                        this.objClasses.push({
                            "ID": item.ID,
                            "Name": item.Name
                        });
                    }
                }
            }
            return this.objClasses;
        },
        _searchIndex: function(searchKeys, str) {
            var index = -1;
            for (var i = 0; i < searchKeys.length; i++) {
                var item = searchKeys[i];
                if(item === "中民") item = "业主";
                if (str.search(item) !== -1) {
                    index = i;
                    break;
                }
            }
            return index;
        },
        _indexOf: function(src, keyName, kValue) {
            var index = -1;
            for (var i = 0; i < src.length; i++) {
                var item = src[i][keyName];
                if (!keyName) {
                    item = src[i];
                }
                if (item === kValue) {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
    u.document = document;
})(CC);