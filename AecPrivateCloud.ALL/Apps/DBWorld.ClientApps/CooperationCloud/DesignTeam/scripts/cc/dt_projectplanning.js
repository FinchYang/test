/*
项目策划
*/
var CC = CC || {};
(function (u, undefined) {
    var projectplanning = {
        /*获取设计阶段*/
        getDesignphase: function (vault, alias) {
            var id = MF.alias.valueList(vault, alias);//通过别名获取ID
            var valuelst = vault.ValueListItemOperations.GetValueListItems(id, false, MFExternalDBRefreshTypeNone);//获取值列表
            return valuelst;
        },
        getLoginUser: function (vault, objVer, propertyDef) {
            var userId = vault.ObjectPropertyOperations.GetProperty(objVer, propertyDef).Value.GetLookupID();
            return userId;
        },
        /*获取角色信息*/
        getRole: function (vault, alias) {
            var id = MF.alias.usergroup(vault, alias);//通过别名获取ID
            var propIdPropProjectRole = MF.alias.propertyDef(vault, md.contacts.propDefs.PropProjectRole);
            var sConditons = MFiles.CreateInstance("SearchConditions");

            var conditionClass = MFiles.CreateInstance("SearchCondition");
            conditionClass.ConditionType = MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = propIdPropProjectRole;
            conditionClass.TypedValue.SetValue(MFDatatypeLookup, id);
            sConditons.Add(-1, conditionClass);
             
            var typeId = MF.alias.objectType(vault, md.contacts.typeAlias);
            var results = MF.ObjectOps.SearchObjects(vault, typeId, sConditons);
            return results;
        },
        /*获取策划专业组*/
        getProfessionalgroup: function (vault, alias) {
            var classId = MF.alias.classType(vault, alias);
            var sConditons = MFiles.CreateInstance("SearchConditions");

            var conditionClass = MFiles.CreateInstance("SearchCondition");
            conditionClass.ConditionType = MFConditionTypeEqual;
            conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);
            sConditons.Add(-1, conditionClass);

            var typeId = MF.alias.objectType(vault, md.planDiscipline.typeAlias);
            var results = MF.ObjectOps.SearchObjects(vault, typeId, sConditons);

            return results;
        },
        /*编辑窗口填值*/
        getPrefilledPropValues: function (vault) {
            var prefilledPropValues = MF.createObject("PropertyValues");

            var nameId = MF.alias.propertyDef(vault, md.projectPlan.propDefs.NameOrTitle);
            var prtyName = MF.property.newTextProperty(nameId, "项目策划");
            prefilledPropValues.Add(-1, prtyName);

            var designPhase = CC.projectplanning.getDesignphase(vault, md.valueList.DesignPhase);//设计阶段 值列表  
            var designPhaseId = MF.alias.propertyDef(vault, md.projectPlan.propDefs.DesignPhase);
            var prtyDesignPhase = MF.property.newLookupProperty(designPhaseId, designPhase.Item(1).ID);
            prefilledPropValues.Add(-1, prtyDesignPhase);

            var loginId = MF.alias.propertyDef(vault, md.contacts.propDefs.PropAccount);

            var pm = CC.projectplanning.getRole(vault, md.userGroups.PM);//经理 属性搜索
            if (pm.Count > 0) {
                var pmId = MF.alias.propertyDef(vault, md.projectPlan.propDefs.ProjManager);
                var userId = CC.projectplanning.getLoginUser(vault, pm.Item(1).ObjVer, loginId);
                var prtyPm = MF.property.newLookupProperty(pmId, userId);
                prefilledPropValues.Add(-1, prtyPm);
            }

            var chiefDesigner = CC.projectplanning.getRole(vault, md.userGroups.ChiefDesigner);//设总 属性搜索
            if (chiefDesigner.Count > 0) {
                var chiefDesignerId = MF.alias.propertyDef(vault, md.projectPlan.propDefs.ChiefDesigner);
                var temp = [];
                for (var j = 1; j <= chiefDesigner.Count; j++) {
                    var userId2 = CC.projectplanning.getLoginUser(vault, chiefDesigner.Item(j).ObjVer, loginId);
                    temp.push(userId2);
                }
                var prtyChiefDesigner = MF.property.newMultiSelectLookupProperty(chiefDesignerId, temp);
                prefilledPropValues.Add(-1, prtyChiefDesigner);
            }

            var professional = CC.projectplanning.getProfessionalgroup(vault, md.planDiscipline.classAlias);//专业 类别搜索
            if (professional.Count > 0) {
                var professionalId = MF.alias.propertyDef(vault, md.projectPlan.propDefs.Disciplines);
                var temp2 = [];
                for (var k = 1; k <= professional.Count; k++) {  
                    temp2.push(professional.Item(k).ObjVer.ID);
                }
                var prtyProfessional = MF.property.newMultiSelectLookupProperty(professionalId, temp2);
                prefilledPropValues.Add(-1, prtyProfessional);
            }

            return prefilledPropValues;
        }
    };
    u.projectplanning = projectplanning;
})(CC);