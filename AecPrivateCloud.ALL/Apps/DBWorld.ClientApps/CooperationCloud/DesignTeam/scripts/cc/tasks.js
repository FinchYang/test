/****************************************
 * 协同云 项目任务
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var task = {
        //作用：设置Listing以列的显示
        //参数：文件夹ID，shellFrame，valueId，propDefIds
        SetListingHeader: function (shellFrame, valueId, propDefIds) {
            //根据ID获取父文件夹对象
            var vault = shellFrame.ShellUI.Vault;
            var folderDefs = MFiles.CreateInstance("FolderDefs"); 

            //获取子视图ID
            var folderDefProp = MFiles.CreateInstance("FolderDef");
            folderDefProp.SetView(valueId);
            folderDefs.add(-1, folderDefProp);

            var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
            uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;

            for (var i = 1; i <= uiState.ListingUIState.Columns.Count; i++) {
                //获取Listing中的列ID
                var id = uiState.ListingUIState.Columns.Item(i).ID;

                //将数组中需要显示的ID和Listing中匹配的列显示出来
                if (CC.task.isMember(propDefIds, id) ||
                    uiState.ListingUIState.Columns.Item(i).ID === MFFolderColumnIdName) {

                    //如果GetArray函数中包含的列，显示出来否则反之
                    uiState.ListingUIState.Columns.Item(i).Visible = true;

                } else {

                    uiState.ListingUIState.Columns.Item(i).Visible = false;

                }
            }

            var indexObj = {};
            indexObj[MFFolderColumnIdName.toString()] = 0;
            for (var i = 0; i < propDefIds.length; i++) {
                indexObj[propDefIds[i].toString()] = i + 1;
            }

            var index = 0;
            for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
                if (uiState.ListingUIState.Columns.Item(j).Visible) {
                    var pId = uiState.ListingUIState.Columns.Item(j).ID;
                    uiState.ListingUIState.Columns.Item(j).Position = indexObj[pId.toString()]; //index++;
                }
            }

            vault.ViewOperations.SaveFolderUIStateForFolder(false, false, folderDefs, false, uiState);
        },
        //作用：筛选 GetArray函数中取出的ID
        //参数：【ids】 【id】获取Listing中的列ID
        isMember: function (ids, id) {
            for (var i = 0; i < ids.length; i++) {
                if (ids[i] == id) {
                    return true;
                }
            }
            return false;
        },
        //作用：取出metadataAlias.js 中MFBuiltInObjectTypeAssignment 所有值
        //参数：【vault】vault对象
        GetArray: function (vault) {
            var res = []; 
            //取出metadataAlias.js 中ClassContacts 所有值 （返回值为字典集合）
            var array = md.genericTask.propDefs;

            var temp = [md.genericTask.propDefs.StartDate, md.genericTask.propDefs.Deadline, md.genericTask.propDefs.AssignedTo];

            for (var item in temp) {
                var propId = MF.alias.propertyDef(vault, temp[item]);
                res.push(propId);
            } 
             
            return res;
        },

        GetAssignToMeTasks: function (vault) {
            var sConditons = MFiles.CreateInstance("SearchConditions");
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefAssignedTo;
            condition.TypedValue.SetValue(MFDatatypeMultiSelectLookup, vault.SessionInfo.UserID);
            sConditons.Add(-1, condition);
            return MF.ObjectOps.SearchObjects(vault, MFBuiltInObjectTypeAssignment, sConditons);
        }
    };
    u.task = task;
})(CC);