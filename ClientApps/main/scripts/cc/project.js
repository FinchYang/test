/****************************************
 * 协同云项目
 * 依赖文件： alias.js, objectOps.js
 ****************************************/

var CC = CC || {};
(function(u, undefined) {
    var proj = {
        updateProject: function (shellFrame, objectVersion, projInfo) {
            //var projectInfo = {
            //    Name: $("#inputName").val(),
            //    Description: $("#inputDescription").val(),
            //    OwnerUnit: $("#inputConstruction").val(),
            //    DesignUnit: $("#inputDesign").val(),
            //    ConstructionUnit: $("#inputBuild").val(),
            //    SupervisionUnit: $("#inputSupervisor").val(),
            //    StartDateUtc: $("#startPicker").datepicker("getDate"),
            //    EndDateUtc: $("#endPicker").datepicker("getDate")
            //};
            var vault = shellFrame.ShellUI.Vault;
            var pvs = new MFiles.PropertyValues();
            //var pvs = MFiles.CreateInstance('PropertyValues');
            //项目名称
            var projTitlePropId = MF.alias.propertyDef(vault, md.proj.propDefs.ProjName);
            if (projTitlePropId >= 0 && projInfo.Name) {
                var propTitle = new MFiles.PropertyValue();
                propTitle.PropertyDef = projTitlePropId;
                propTitle.TypedValue.SetValue(MFDatatypeText, projInfo.Name);
                pvs.Add(-1, propTitle);
            }
            //项目描述
            var descriptionPropId = MF.alias.propertyDef(vault, md.proj.propDefs.Description);
            if (descriptionPropId > 0) {
                var propDesc = new MFiles.PropertyValue();
                propDesc.PropertyDef = descriptionPropId;
                if (projInfo.Description) {
                    propDesc.TypedValue.SetValue(MFDatatypeMultiLineText, projInfo.Description);
                } else {
                    propDesc.TypedValue.SetValueToNULL(MFDatatypeMultiLineText);
                }
                pvs.Add(-1, propDesc);
            }
            //建设单位
            var constructionPropId = MF.alias.propertyDef(vault, md.proj.propDefs.ProprietorUnit);
            if (constructionPropId) {
                var propProprietorUnit = new MFiles.PropertyValue();
                propProprietorUnit.PropertyDef = constructionPropId;
                if (projInfo.OwnerUnit) {
                    propProprietorUnit.TypedValue.SetValue(MFDatatypeText, projInfo.OwnerUnit);
                } else {
                    propProprietorUnit.TypedValue.SetValueToNULL(MFDatatypeText);
                }
                pvs.Add(-1, propProprietorUnit);
            }
            // 设计单位
            var designPropId = MF.alias.propertyDef(vault, md.proj.propDefs.DesignUnit);
            if (designPropId) {
                var propDesignUnit = new MFiles.PropertyValue();
                propDesignUnit.PropertyDef = designPropId;
                if (projInfo.DesignUnit) {
                    propDesignUnit.TypedValue.SetValue(MFDatatypeText, projInfo.DesignUnit);
                } else {
                    propDesignUnit.TypedValue.SetValueToNULL(MFDatatypeText);
                }
                pvs.Add(-1, propDesignUnit);
            }
            //施工单位
            var buildPropId = MF.alias.propertyDef(vault, md.proj.propDefs.BuilderUnit);
            if (buildPropId) {
                var propBuilderUnit = new MFiles.PropertyValue();
                propBuilderUnit.PropertyDef = buildPropId;
                if (projInfo.ConstructionUnit) {
                    propBuilderUnit.TypedValue.SetValue(MFDatatypeText, projInfo.ConstructionUnit);
                } else {
                    propBuilderUnit.TypedValue.SetValueToNULL(MFDatatypeText);
                }
                pvs.Add(-1, propBuilderUnit);
            }
            //监理单位
            var supervisorPropId = MF.alias.propertyDef(vault, md.proj.propDefs.SupervisorUnit);
            if (supervisorPropId) {
                var propSupervisorUnit = new MFiles.PropertyValue();
                propSupervisorUnit.PropertyDef = supervisorPropId;
                if (projInfo.SupervisionUnit) {
                    propSupervisorUnit.TypedValue.SetValue(MFDatatypeText, projInfo.SupervisionUnit);
                } else {
                    propSupervisorUnit.TypedValue.SetValueToNULL(MFDatatypeText);
                }
                pvs.Add(-1, propSupervisorUnit);
            }
            //开始时间
            var startDatePropId = MF.alias.propertyDef(vault, md.proj.propDefs.StartDate);
            if (startDatePropId) {
                var propStartDate = new MFiles.PropertyValue();
                propStartDate.PropertyDef = startDatePropId;
                if (projInfo.StartDateUtc) {
                    var startDate = this._getMfDate(projInfo.StartDateUtc);
                    propStartDate.TypedValue.SetValue(MFDatatypeDate, startDate);
                } else {
                    propStartDate.TypedValue.SetValueToNULL(MFDatatypeDate);
                }
                pvs.Add(-1, propStartDate);
            }
            //结束时间
            var endDatePropId = MF.alias.propertyDef(vault, md.proj.propDefs.Deadline);
            if (endDatePropId) {
                var propDeadline = new MFiles.PropertyValue();
                propDeadline.PropertyDef = endDatePropId;
                if (projInfo.EndDateUtc) {
                    var deadline = this._getMfDate(projInfo.EndDateUtc);
                    propDeadline.TypedValue.SetValue(MFDatatypeDate, deadline);
                } else {
                    propDeadline.TypedValue.SetValueToNULL(MFDatatypeDate);
                }
                pvs.Add(-1, propDeadline);
            }
            MF.ObjectOps.updateObject(vault, objectVersion, pvs);
        },
        _getMfDate: function (jsDate) {//将js日期转换成Mfiles日期
            var ts = MFiles.CreateInstance('Timestamp');
            //ts.SetValue(jsDate);
            ts.Year = jsDate.getFullYear();
            ts.Month = jsDate.getMonth() + 1;
            ts.Day = jsDate.getDate();
            return ts.GetValue();
        }
    };
    u.proj = proj;
    
    var task ={
        markTask: function(shellFrame){
            //任务标记完成
            if (!shellFrame.ActiveListing || !shellFrame.ActiveListing.CurrentSelection) return;
            var objVnsProps = shellFrame.ActiveListing.CurrentSelection.ObjectVersionsAndProperties
            if(objVnsProps.Count !== 1) return;
            var objVn = objVnsProps.Item(1).VersionData;
            var properties = objVnsProps.Item(1).Properties;

            var vault = shellFrame.ShellUI.Vault;
            var classId = MF.alias.classType(vault, md.noticeTask.classAlias);
            if(objVn.Class !== classId) return;
            
            var userId= vault.SessionInfo.UserID;
            var tValue = properties.SearchForProperty(44).Value;
            if(tValue.IsNULL()) return;
            var toLookups = tValue.GetValueAsLookups();
            var index = toLookups.GetLookupIndexByItem(userId);
            if(index === -1) return;
            toLookups.Remove(index);
            
            var item = new MFiles.Lookup();
            item.Item = userId;
            
            var markedLookups;
            var markedtValue = properties.SearchForProperty(45).Value;
            if(markedtValue.IsNULL()){
                markedLookups = new MFiles.Lookups();
            }else{
                markedLookups = markedtValue.GetValueAsLookups().Clone();
            }
            markedLookups.Add(-1, item);
            
            var propValues = new MFiles.PropertyValues();
            var propValue2 = MFiles.CreateInstance("PropertyValue");
            propValue2.PropertyDef = 44; //分配给
            propValue2.TypedValue.SetValue(MFDatatypeMultiSelectLookup, toLookups);
            propValues.Add(-1, propValue2);  
            
            var propValue = MFiles.CreateInstance("PropertyValue");
            propValue.PropertyDef = 45; //被标记完成
            propValue.TypedValue.SetValue(MFDatatypeMultiSelectLookup, markedLookups);
            propValues.Add(-1, propValue);  
            try {
                MF.ObjectOps.updateObject(vault, objVn, propValues);
            } catch (error) {      
            }    
        }
    };
    u.task = task;
})(CC);