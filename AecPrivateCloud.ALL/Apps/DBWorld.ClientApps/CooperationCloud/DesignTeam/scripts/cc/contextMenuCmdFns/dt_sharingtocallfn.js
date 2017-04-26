/*******************************************
*右键功能"提资给"回调函数
*依赖文件alias.js, property.js, objectOps.js
*******************************************/
var cmdCallbackFn = cmdCallbackFn || {};

(function (fn) {
    fn.sharingTo = function (shellFrame) {
        var that = this;
        return function () {
            //shellframe.ShellUI.ShowMessage("未实现！");
            var objVnsAndProps = shellFrame.Listing.CurrentSelection.ObjectVersionsAndProperties;
            if (objVnsAndProps.Count == 0) return;
            that._getSharingTos(shellFrame, objVnsAndProps);
        }
    };
    //从弹出框中选择提资给
    fn._getSharingTos = function (shellFrame, objVnsAndProps) {
        var vault = shellFrame.ShellUI.Vault;
        var srcUsers = this._getUsers(vault);
        var srcPhases = this._getPhases(vault);
        var srcDisplines = this._getDisplines(vault);
        var dashboardData = {
            'Users': srcUsers,
            'SelectedUsers': [],
            'Cancelled': false,
            'Phases': srcPhases,
            'SelectedPhase': null,
            'Disciplines': srcDisplines,
            'SelectedDiscipline': null,
            'LogTitle': ""
        };
        shellFrame.ShowPopupDashboard('dtsharingtolog', true, dashboardData);
        if (dashboardData.Cancelled == false && dashboardData.LogTitle) {
            //shellFrame.ShowMessage(dashboardData.LogTitle);
            var propIdTitle = MF.alias.propertyDef(vault, md.designDoc.propDefs.NameOrTitle);
            var propIdDisci = MF.alias.propertyDef(vault, md.designDoc.propDefs.Discipline);
            var objIds = [];
            for (var i = 1; i <= objVnsAndProps.Count; i++) {
                var objVnProps = objVnsAndProps.Item(i);
                var properties = objVnProps.Properties;
                var objVersion = objVnProps.VersionData;
                var disciId = properties.SearchForProperty(propIdDisci).Value.GetLookupID();
                var title = properties.SearchForProperty(propIdTitle).Value.DisplayValue;
                if (objVersion.FilesCount == 0) {
                    //continue;
                }
                var srcObjectFiles = MFiles.CreateInstance("SourceObjectFiles");
                var objFiles = objVersion.Files;
                for (var j = 1; j <= objFiles.Count; j++) {
                    var objFile = objFiles.Item(j);
                    var srcFilePath = vault.ObjectFileOperations.GetPathInDefaultView(objVersion.ObjVer.ObjID, objVersion.ObjVer.Version,
                            objFile.ID, objFile.Version, MFLatestSpecificBehaviorAutomatic, false);
                    var srcObjectFile = MFiles.CreateInstance("SourceObjectFile");
                    srcObjectFile.title = objFile.Title;
                    srcObjectFile.Extension = objFile.Extension;
                    srcObjectFile.SourceFilePath = srcFilePath;
                    srcObjectFiles.Add(-1, srcObjectFile);
                }

                var objvn = this._newSharingDoc(shellFrame, title, disciId, srcObjectFiles);
                objIds.push(objvn.ObjVer.ID);
            }
            var userIds = [];
            for (var k = 0; k < dashboardData.SelectedUsers.length; k++) {
                userIds.push(dashboardData.SelectedUsers[k].id);
            }
            try {
                this._newSharingLog(shellFrame, dashboardData.LogTitle, dashboardData.SelectedPhase.id,
                    dashboardData.SelectedDiscipline.id, objIds, userIds, new Date());
                shellFrame.ShellUI.ShowMessage("提资成功！");
            } catch (e) {
                shellFrame.ShellUI.ShowMessage("提资失败：" + e.message);
            }
        }
    };
    fn._getUsers = function (vault) {
        var users = [];
        var pairs = vault.UserOperations.GetUserList();

        for (var i = 1; i <= pairs.Count; i++) {
            var item = {
                id: parseInt(pairs.Item(i).Key),
                label: pairs.Item(i).Name
            };
            if (item.id > 1 && item.label != "admin") {
                users.push(item);
            }
        }
        return users;
    };
    fn._getDisplines = function (vault) {
        var res = [];
        var typeId = MF.alias.objectType(vault, md.planDiscipline.typeAlias);
        var objVns = MF.ObjectOps.SearchObjectsByType(vault, typeId);
        for (var i = 1; i <= objVns.Count; i++) {
            var item = {
                id: objVns.Item(i).ObjVer.ID,
                label: objVns.Item(i).Title
            }
            res.push(item);
        }
        return res;
    };
    fn._getPhases = function (vault) {
        var res = [];
        var valueListId = MF.alias.valueList(vault, md.valueList.DesignPhase);
        var values = vault.ValueListItemOperations.GetValueListItems(valueListId);
        for (var i = 1; i <= values.Count; i++) {
            var item = {
                id: values.Item(i).ID,
                label: values.Item(i).Name
            }
            res.push(item);
        }
        return res;
    };
    fn._newSharingDoc = function (shellFrame, title, disciplineId, srcObjFiles) {
        var vault = shellFrame.ShellUI.Vault;
        var sdClass = MF.alias.classType(vault, md.sharingDoc.classAlias);
        var propIdTitle = MF.alias.propertyDef(vault, md.sharingDoc.propDefs.NameOrTitle);
        var propIdDisci = MF.alias.propertyDef(vault, md.sharingDoc.propDefs.Discipline);
        var propIdStatus = MF.alias.propertyDef(vault, md.sharingDoc.propDefs.DocStatus);

        var prfillProps = new MFiles.PropertyValues();

        prfillProps.Add(-1, MF.property.newTextProperty(propIdTitle, title));
        if (disciplineId > 0) {
            prfillProps.Add(-1, MF.property.newLookupProperty(propIdDisci, disciplineId));
        }
        //文档状态
        if (!this.docSharingStatus) {
            var valueListId = MF.alias.valueList(vault, md.valueList.DocStatus);
            this.docSharingStatus = MF.vault.getValueListItemId(vault, valueListId, "提资");
        }
        prfillProps.Add(-1, MF.property.newLookupProperty(propIdStatus, this.docSharingStatus));

        if (srcObjFiles.Count === 1) {
            var pvIsSingleDoc = MF.property.newBooleanProperty(MFBuiltInPropertyDefSingleFileObject, true);
            prfillProps.Add(-1, pvIsSingleDoc);
        }
        var obj = MF.ObjectOps.createObject(vault, 0, sdClass, prfillProps, srcObjFiles);
        return obj;
    };
    fn._newSharingLog = function (shellFrame, title, phaseId, disciplineId, docIds, sharingTos, date) {
        var vault = shellFrame.ShellUI.Vault;
        var typeId = MF.alias.objectType(vault, md.sharingLog.typeAlias);
        var classId = MF.alias.classType(vault, md.sharingLog.classAlias);
        var propIdTitle = MF.alias.propertyDef(vault, md.sharingLog.propDefs.NameOrTitle);
        var propIdPhase = MF.alias.propertyDef(vault, md.sharingLog.propDefs.DesignPhase);
        var propIdDisci = MF.alias.propertyDef(vault, md.sharingLog.propDefs.Discipline);
        var propIdDocs = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharedDocs);
        var propIdSharingTo = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharingTo);
        var propIdDate = MF.alias.propertyDef(vault, md.sharingLog.propDefs.SharingDate);

        var prfillProps = new MFiles.PropertyValues();

        prfillProps.Add(-1, MF.property.newTextProperty(propIdTitle, title));
        //阶段
        prfillProps.Add(-1, MF.property.newLookupProperty(propIdPhase, phaseId));
        //专业
        prfillProps.Add(-1, MF.property.newLookupProperty(propIdDisci, disciplineId));
        //提资文档
        prfillProps.Add(-1, MF.property.newMultiSelectLookupProperty(propIdDocs, docIds));
        //提资给
        prfillProps.Add(-1, MF.property.newMultiSelectLookupProperty(propIdSharingTo, sharingTos));
        //日期
        prfillProps.Add(-1, MF.property.newDateProperty(propIdDate, date));

        MF.ObjectOps.createObject(vault, typeId, classId, prfillProps, undefined);
    };
})(cmdCallbackFn);