/************
*MFiles获取、操作目录树节点对象
*************/
var CC = CC || {};
(function (u) {
    var o = {
        //获取项目
        getProject: function (vault) {
            var projTypeId = MF.alias.objectType(vault, md.proj.typeAlias);
            var projs = MF.ObjectOps.SearchObjectsByType(vault, projTypeId);
            if (projs.Count < 1) {
                throw new Error("获取项目失败！");
            }
            var projObj = projs.Item(1);
            return this._getObjInfoString(projObj);
        },
        //获取单体
        getUnits: function (vault) {
            var typeId = MF.alias.objectType(vault, md.unit.typeAlias);
            var objs = MF.ObjectOps.SearchObjectsByType(vault, typeId);
            var res = [];
            for (var i = 1; i <= objs.Count; i++) {
                var obj = objs.Item(i);
                res.push(this._getObjInfoString(obj));
            }
            res.sort(function (a, b) {
                return a.ID - b.ID;
            });
            return res;
        },
        //获取楼层：parentId为父对象单体 ID
        getFloors: function (vault, parentId) {
            var typeId = MF.alias.objectType(vault, md.floor.typeAlias);
            var parentTypeId = MF.alias.objectType(vault, md.unit.typeAlias);

            return this._searchObjsByParent(vault, parentTypeId, parentId, typeId);
        },
        //获取专业：parentId为父对象楼层 ID
        getDisciplines: function (vault, parentId) {
            var typeId = MF.alias.objectType(vault, md.discipline.typeAlias);
            var parentTypeId = MF.alias.objectType(vault, md.floor.typeAlias);

            return this._searchObjsByParent(vault, parentTypeId, parentId, typeId);
        },
        //获取构件类别：parentId为父对象模型专业 ID
        getComponentClasses: function (vault, parentId) {
            var typeId = MF.alias.objectType(vault, md.componentClasses.typeAlias);
            var parentTypeId = MF.alias.objectType(vault, md.discipline.typeAlias);

            return this._searchObjsByParent(vault, parentTypeId, parentId, typeId);
        },
        //获取构件类型：parentId为父对象构件类别 ID
        getComponentTypes: function (vault, parentId) {
            var typeId = MF.alias.objectType(vault, md.componentType.typeAlias);
            var parentTypeId = MF.alias.objectType(vault, md.componentClasses.typeAlias);

            return this._searchObjsByParent(vault, parentTypeId, parentId, typeId);
        },
        //获取构件模型：parentId为父对象构件类型 ID
        getComponentModels: function (vault, parentId) {
            var typeId = MF.alias.objectType(vault, md.componentModel.typeAlias);
            var parentTypeId = MF.alias.objectType(vault, md.componentType.typeAlias);

            return this._searchObjsByParent(vault, parentTypeId, parentId, typeId);
        },
        _searchObjsByParent: function (vault, parentType, parentId, type) {

            var propIdOwner = vault.ObjectTypeOperations.GetObjectType(parentType).OwnerPropertyDef;
            var sConditons = MFiles.CreateInstance("SearchConditions");
            //所属对象
            var conditionOwner = MFiles.CreateInstance("SearchCondition");
            conditionOwner.ConditionType = MFConditionTypeEqual;
            conditionOwner.Expression.DataPropertyValuePropertyDef = propIdOwner;
            conditionOwner.TypedValue.SetValue(MFDatatypeLookup, parentId);
            sConditons.Add(-1, conditionOwner);

            var objs = MF.ObjectOps.SearchObjects(vault, type, sConditons);
            var res = [];
            for (var i = 1; i <= objs.Count; i++) {
                var obj = objs.Item(i);
                res.push(this._getObjInfoString(obj));
            }
            res.sort(function (a, b) {
                return a.ID - b.ID;
            });
            return res;
        },
        _getObjInfoString: function (obj) {
            return { Type: obj.ObjVer.Type, ID: obj.ObjVer.ID, Title: obj.Title };
        },
        createUnit: function (vault, title) {
            var typeId = MF.alias.objectType(vault, md.unit.typeAlias);
            var classId = MF.alias.classType(vault, md.unit.classAlias);

            var propValues = MFiles.CreateInstance('PropertyValues');
            //名称
            var propTitle = MFiles.CreateInstance('PropertyValue');
            propTitle.PropertyDef = 0;
            propTitle.TypedValue.SetValue(MFDatatypeText, title);
            propValues.Add(-1, propTitle);

            var res = MF.ObjectOps.createObject(vault, typeId, classId, propValues);
            return { "Type": res.ObjVer.Type, "ID": res.ObjVer.ID, "Title": res.Title };
        },
        createFloor: function (vault, title, parentId) {
            var typeId = MF.alias.objectType(vault, md.floor.typeAlias);
            var classId = MF.alias.classType(vault, md.floor.classAlias);

            var parentTypeId = MF.alias.objectType(vault, md.unit.typeAlias);
            return this._createObjWithOwner(vault, parentTypeId, parentId, typeId, classId, title);
        },
        createDiscipline: function (vault, title, parentId) {
            var typeId = MF.alias.objectType(vault, md.discipline.typeAlias);
            var classId = MF.alias.classType(vault, md.discipline.classAlias);

            var parentTypeId = MF.alias.objectType(vault, md.floor.typeAlias);
            return this._createObjWithOwner(vault, parentTypeId, parentId, typeId, classId, title);
        },
        _createObjWithOwner: function (vault, parentType, parentId, typeId, classId, title) {

            var propIdOwner = vault.ObjectTypeOperations.GetObjectType(parentType).OwnerPropertyDef;
            //alert(parentType + "-" + parentId + "::" + typeId + "-" + classId + "-" + title + "-" + propIdOwner);
            var propValues = MFiles.CreateInstance('PropertyValues');
            //名称
            var propTitle = MFiles.CreateInstance('PropertyValue');
            propTitle.PropertyDef = 0;
            propTitle.TypedValue.SetValue(MFDatatypeText, title);
            propValues.Add(-1, propTitle);
            //所属对象
            var propOwner = MFiles.CreateInstance('PropertyValue');
            propOwner.PropertyDef = propIdOwner;
            propOwner.TypedValue.SetValue(MFDatatypeLookup, parentId);
            propValues.Add(-1, propOwner);

            var res = MF.ObjectOps.createObject(vault, typeId, classId, propValues, undefined);
            return { "Type": res.ObjVer.Type, "ID": res.ObjVer.ID, "Title": res.Title };
        },
        deleteDom: function (vault, typeId, objId) {//项目除外
            MF.ObjectOps.DeleteObject(vault, typeId, objId);
        },
        renameDom: function (vault, typeId, objId, newTitle) {//项目除外
            var oObjVer = MFiles.CreateInstance('ObjVer');
            oObjVer.SetIDs(typeId, objId, -1);
            var oObjVn = vault.ObjectOperations.GetObjectInfo(oObjVer, true, true);
            var checkOutVn;
            if (oObjVn.ObjectCheckedOut) checkOutVn = oObjVn;
            else {
                checkOutVn = vault.ObjectOperations.CheckOut(oObjVer.ObjID);
            }
            //名称
            var propTitle = MFiles.CreateInstance('PropertyValue');
            propTitle.PropertyDef = 0;
            propTitle.TypedValue.SetValue(MFDatatypeText, newTitle);

            vault.ObjectPropertyOperations.SetProperty(checkOutVn.ObjVer, propTitle);
            vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
        },
        isProjectDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.proj.typeAlias);
            return objTypeId == typeId;
        },
        isUnitDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.unit.typeAlias);
            return objTypeId == typeId;
        },
        isFloorDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.floor.typeAlias);
            return objTypeId == typeId;
        },
        isDisciplineDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.discipline.typeAlias);
            return objTypeId == typeId;
        },
        isCatagoryDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.componentClasses.typeAlias);
            return objTypeId == typeId;
        },
        isTypeDom: function(vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.componentType.typeAlias);
            return objTypeId == typeId;
        },
        isComponentDom: function (vault, typeId) {
            var objTypeId = MF.alias.objectType(vault, md.componentModel.typeAlias);
            return objTypeId == typeId;
        }
    };
    u.treeDomOps = o;
})(CC);