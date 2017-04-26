/****************************************
 * 协同云 模型预览
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var browser = {
        //作用：显示选择的对象的元数据
        //参数：shellFrame, 对象类型，对象ID
        showMetaData: function (shellFrame, type, id) {
            var objectId = MFiles.CreateInstance('ObjID');
            objectId.SetIDs(type, id);
            var vault = shellFrame.ShellUI.Vault;
            var objVer = vault.ObjectOperations.GetLatestObjVer(objectId, true, false);
            MF.ui.selectObject(shellFrame, objVer);
        },

        //作用：显示模型的元数据
        //参数：shellFrame，对象id，对象名称，是否显示Tab标签
        showModelInfo: function(shellFrame, id, title, visible) {
            var appTabId = "_apptab";
            var appTab = null;
            try {
                appTab = shellFrame.RightPane.GetTab(appTabId);
            } catch (e) {

            }
            if (!appTab) {
                appTab = shellFrame.RightPane.AddTab(appTabId, "构件附件", "_last");
            }

            if (appTab) {
                var model = {};
                model.Id = id;
                model.Name = title;
                var customData = { 'shellframe': shellFrame, 'noteData': model };
                appTab.ShowDashboard('elementAnnex', customData);
                appTab.Visible = visible;
            }
        },

        //作用：搜索当前节点下的构件模型对象
        //参数：vault，节点对象
        searchModelDocument: function (vault, domObj) {
            var classId = MF.alias.classType(vault, md.previewModel.classAlias);
            var sConditons = MFiles.CreateInstance("SearchConditions");
            //文档类别
            var conditionClass = MFiles.CreateInstance("SearchCondition");
            conditionClass.ConditionType = MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
            sConditons.Add(-1, conditionClass);

            var propIdDomAt = this._getPropDefByType(vault, domObj.ObjType);
            if (propIdDomAt > 0) {
                var condition1 = MFiles.CreateInstance("SearchCondition");
                condition1.ConditionType = MFConditionTypeEqual;
                condition1.Expression.DataPropertyValuePropertyDef = propIdDomAt;
                condition1.TypedValue.SetValue(MFDatatypeLookup, domObj.ID);
                sConditons.Add(-1, condition1);
            } else {
                return null;
            }

            propIdDomAt = this._getNextPropDefByType(vault, domObj.ObjType);
            if (propIdDomAt > 0) {
                var condition2 = MFiles.CreateInstance("SearchCondition");
                condition2.ConditionType = MFConditionTypeEqual;
                condition2.Expression.DataPropertyValuePropertyDef = propIdDomAt;
                condition2.TypedValue.SetValueToNULL(MFDatatypeLookup);
                sConditons.Add(-1, condition2);
            }

            var objVns = MF.ObjectOps.SearchObjects(vault, 0, sConditons);
            return objVns;
        },

        //作用：获取当前选择节点的类型
        //参数：vault，对象类型
        _getPropDefByType: function (vault, objType) {
            var propIdDomAt = -1;
            if (CC.treeDomOps.isProjectDom(vault, objType)) {//当前节点是项目

            } else if (CC.treeDomOps.isUnitDom(vault, objType)) {//单体
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.UnitAt);
            } else if (CC.treeDomOps.isFloorDom(vault, objType)) {//楼层
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.FloorAt);
            } else if (CC.treeDomOps.isDisciplineDom(vault, objType)) {//专业
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.DisciplineAt);
            } else if (CC.treeDomOps.isCatagoryDom(vault, objType)) {//类别
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentClassAt);
            } else if (CC.treeDomOps.isTypeDom(vault, objType)) {//类型
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentTypeAt);
            } else if (CC.treeDomOps.isComponentDom(vault, objType)) {//模型
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentAt);
            }
            return propIdDomAt;
        },

        //作用：获取当前选择节点的下个节点类型
        //参数：vault，对象类型
        _getNextPropDefByType: function (vault, objType) {
            var propIdDomAt = -1;
            if (CC.treeDomOps.isProjectDom(vault, objType)) {//当前节点是项目
            } else if (CC.treeDomOps.isUnitDom(vault, objType)) {//单体
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.FloorAt);
            } else if (CC.treeDomOps.isFloorDom(vault, objType)) {//楼层
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.DisciplineAt);
            } else if (CC.treeDomOps.isDisciplineDom(vault, objType)) {//专业
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentClassAt);
            } else if (CC.treeDomOps.isCatagoryDom(vault, objType)) {//类别
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentTypeAt);
            } else if (CC.treeDomOps.isTypeDom(vault, objType)) {//类型
                propIdDomAt = MF.alias.propertyDef(vault, md.previewModel.propDefs.ComponentAt);
            } else if (CC.treeDomOps.isComponentDom(vault, objType)) {//模型
               
            }
            return propIdDomAt;
        }
    };

    u.browser = browser;
})(CC);