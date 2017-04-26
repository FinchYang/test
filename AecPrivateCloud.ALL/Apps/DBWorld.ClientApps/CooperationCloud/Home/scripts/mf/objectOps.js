/****************************
*MFiles对象的操作：增、查、改、删(CRUD)
****************************/
var MF = MF || {};
(function(v) {
    var o = {
        ///<summery>创建非单文档对象</summery>
        ///<params>srcFiles: SourceObjectFiles</params>
        createObject: function (vault, typeId, classId, propValues, srcFiles) {
            if (!propValues) return undefined;
            var classDefId = parseInt(MFBuiltInPropertyDefClass);
            var pvs = propValues.Clone();
            if (pvs.IndexOf(classDefId) === -1) {
                //class
                var classValue = MFiles.CreateInstance("PropertyValue");
                classValue.PropertyDef = classDefId;
                classValue.TypedValue.SetValue(MFDatatypeLookup, classId);
                pvs.Add(-1, classValue);
            }
            var singleFileDefId = parseInt(MFBuiltInPropertyDefSingleFileObject);
            if (pvs.IndexOf(singleFileDefId) === -1) {
                //是否为单文档(*)
                var singleFileValue = MFiles.CreateInstance("PropertyValue");
                singleFileValue.PropertyDef = singleFileDefId;
                singleFileValue.TypedValue.SetValue(MFDatatypeBoolean, false);
                pvs.Add(-1, singleFileValue);
            }
            if (!srcFiles) srcFiles = MFiles.CreateInstance("SourceObjectFiles");
            var objvnAndProps = vault.ObjectOperations.CreateNewObject(typeId, pvs, srcFiles, MFiles.CreateInstance("AccessControlList"));
            var res = vault.ObjectOperations.CheckIn(objvnAndProps.ObjVer);
            return res;
        },
        ///<summery>创建单文档对象</summery>
        ///<params>srcObjectFile: SourceObjectFile</params>
	    createSingleFile: function (vault, classId, propValues, srcObjectFile) {
			if(!propValues) return;
			var classDefId = parseInt(MFBuiltInPropertyDefClass);
			var pvs = propValues.Clone();
			if(pvs.IndexOf(classDefId) == -1){
				//class
				var classValue = MFiles.CreateInstance('PropertyValue');
				classValue.PropertyDef = classDefId;
				classValue.TypedValue.SetValue(MFDatatypeLookup, classId);
				pvs.Add(-1, classValue);
			}
            /*
			if(!srcFileObj) return;
			var title = srcFileObj.Title;//不带扩展名
			var extension = srcFileObj.Extension;//不带前导符'.'
			var filePath = srcFileObj.FilePath;//test.txt
			
			var srcObjectFile = MFiles.CreateInstance('SourceObjectFile');
			srcObjectFile.SourceFilePath = filePath;
			srcObjectFile.Title = title;
			srcObjectFile.Extension = extension;
			*/
			vault.ObjectOperations.CreateNewSFDObject(0, pvs, srcObjectFile, true, MFiles.CreateInstance("AccessControlList"));
		},
		createObjPrefilled: function(vault, prefilledPropValues, typeId, sourceObjectFiles){
			var pvs;
			if(!prefilledPropValues) pvs = MFiles.CreateInstance("PropertyValues");
			else pvs = prefilledPropValues.Clone();
            var oCreationInfo = MFiles.CreateInstance("ObjectCreationInfo");
            oCreationInfo.SetObjectType(typeId, true);
			if(sourceObjectFiles) oCreationInfo.SetSourceFiles(sourceObjectFiles);
            vault.ObjectOperations.ShowPrefilledNewObjectWindow(0, MFObjectWindowModeInsert,
                oCreationInfo, pvs, MFiles.CreateInstance("AccessControlList"));
		},
        ///<summery>删除对象</summery>
		DeleteObject: function(vault, typeId, objectId){
			 try{
				var oObjID = MFiles.CreateInstance("ObjID");
				oObjID.SetIDs(typeId, objectId);
			     try {
			         vault.ObjectOperations.DeleteObject(oObjID); //2015
			     } catch (e) {
			         vault.ObjectOperations.RemoveObject(oObjID); //10.2
			     }
			 }catch(e){
				var mes = "删除对象(Type:" + typeId + ", ID:" + objectId +")失败:" + e.message;
				throw new Error(mes);
			 }	
		},
        ///<summery>根据对象类型，搜索对象</summery>
		SearchObjectsByType: function (vault, typeId) {
		    return this.SearchObjects(vault, typeId, undefined);
        },
        ///<summery>搜索对象</summery>
        SearchObjects: function (vault, typeId, searchConditions) {
            var sConditions;
            if (!searchConditions) sConditions = MFiles.CreateInstance("SearchConditions");
            else sConditions = searchConditions.Clone();

            //对象类型
            var conditionType = MFiles.CreateInstance("SearchCondition");
            conditionType.ConditionType = MFConditionTypeEqual;
            conditionType.Expression.DataStatusValueType = MFStatusTypeObjectTypeID;
            conditionType.TypedValue.SetValue(MFDatatypeLookup, typeId);
            sConditions.Add(-1, conditionType);
            //是否删除
            var conditionDeleted = MFiles.CreateInstance("SearchCondition");
            conditionDeleted.ConditionType = MFConditionTypeEqual;
            conditionDeleted.Expression.DataStatusValueType = MFStatusTypeDeleted;
            conditionDeleted.TypedValue.SetValue(MFDatatypeBoolean, false);
            sConditions.Add(-1, conditionDeleted);
            var sResults = vault.ObjectSearchOperations.SearchForObjectsByConditions(sConditions, MFSearchFlagNone, false);
            return sResults;
        },
        ///<summery>获取临时搜索视图</summery>
        getTempSearchView: function (vault, searchText, typeId,objId, oSearchConditions) {
            var viewName = "临时视图" + " - " + typeId + "-" + objId +"-"+ new Date().toString();
            var oViewNew = MFiles.CreateInstance('View');
            oViewNew.Name = viewName;

            var oSCriteria = MFiles.CreateInstance('SearchCriteria');
            if (searchText) {
                oSCriteria.FullTextSearchString = searchText;
                oSCriteria.FullTextSearchFlags = (MFFullTextSearchFlagsStemming
                                           | MFFullTextSearchFlagsLookInMetaData
                                           | MFFullTextSearchFlagsLookInFileData
                                           | MFFullTextSearchFlagsTypeAnyWords);
            }
            oSCriteria.SearchFlags = MFSearchFlagNone;
            oSCriteria.ExpandUI = false;
            if (typeId && typeId >= 0) {
                var scObjType = MFiles.CreateInstance("SearchCondition");
                scObjType.ConditionType = MFConditionTypeEqual;
                scObjType.Expression.DataStatusValueType = MFStatusTypeObjectTypeID;
                scObjType.TypedValue.SetValue(MFDatatypeLookup, typeId);
                var oObjTypeEx = MFiles.CreateInstance('SearchConditionEx');
                oObjTypeEx.Enabled = true;
                oObjTypeEx.Ignored = false;
                oObjTypeEx.SpecialNULL = false;
                oObjTypeEx.SearchCondition = scObjType;
                oSCriteria.ObjectTypeCondition = oObjTypeEx;
            }

            var oExpression = MFiles.CreateInstance('Expression');
            oExpression.DataStatusValueType = MFStatusTypeDeleted;
            oExpression.DataStatusValueDataFunction = MFDataFunctionNoOp;
            var oTypedValue = MFiles.CreateInstance('TypedValue');
            oTypedValue.SetValue(MFDatatypeBoolean, false);
            var oDeletedEx = MFiles.CreateInstance('SearchConditionEx');
            oDeletedEx.SearchCondition.Set(oExpression, MFConditionTypeEqual, oTypedValue);
            oDeletedEx.Enabled = true;
            oDeletedEx.Ignored = false;
            oDeletedEx.SpecialNULL = false;
            oSCriteria.AdditionalConditions.Add(-1, oDeletedEx);

            if (oSearchConditions) {
                for (var i = 1; i <= oSearchConditions.Count; i++) {
                    var oScEx = MFiles.CreateInstance('SearchConditionEx');
                    oScEx.SearchCondition = oSearchConditions.Item(i);
                    oScEx.Enabled = true;
                    oScEx.Ignored = false;
                    oScEx.SpecialNULL = false;
                    oSCriteria.AdditionalConditions.Add(-1, oScEx);
                }
            }

            var oTempViewNew = vault.ViewOperations.AddTemporarySearchView(oViewNew, oSCriteria, MFiles.CreateInstance('SearchConditions'));
            return oTempViewNew;
        },
        ///<summery>获取临时搜索视图位置</summery>
        getSearchViewLoc: function (vault, oView) {
            return vault.ViewOperations.GetViewLocationInClient(oView.ID, true);
        }
    };
	v.ObjectOps = o;
})(MF);