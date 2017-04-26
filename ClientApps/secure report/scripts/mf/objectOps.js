/****************************
*MFiles对象的操作：增、查、改、删(CRUD)
****************************/
var MF = MF || {};
(function (v) {
    var o = {
        ///<summery>获取临时搜索视图</summery>
        getTempSearchView: function (vault, searchText, typeId, objId, oSearchConditions) {
            var viewName = "临时视图" + " - " + typeId + "-" + objId + "-" + new Date().toString();
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