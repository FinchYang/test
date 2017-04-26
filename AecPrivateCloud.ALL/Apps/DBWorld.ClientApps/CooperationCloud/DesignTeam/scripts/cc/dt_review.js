/*
  校审区
*/
var CC = CC || {};
(function(u, undefined) {
    var review = {
        //获取显示的列
        getArray: function (vault) {
            var res = [];
            var propId = MF.alias.propertyDef(vault, MFBuiltInPropertyDefState);
            res.push(propId);
            return res;
        },
        //作用：设置Listing以列来显示
        //参数：文件夹ID，shellFrame，valueId，propDefIds
        setListingHeader: function (folderDefs, shellFrame, propDefIds) {
            var vault = shellFrame.ShellUI.Vault;
            var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
            uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;

            for (var i = 1; i <= uiState.ListingUIState.Columns.Count; i++) {
                //获取Listing中的列ID
                var id = uiState.ListingUIState.Columns.Item(i).ID;

                //将数组中需要显示的ID和Listing中匹配的列显示出来
                if (this.isMember(propDefIds, id) ||
                    uiState.ListingUIState.Columns.Item(i).ID === MFFolderColumnIdName) {
                    //如果GetArray函数中包含的列，显示出来否则反之
                    uiState.ListingUIState.Columns.Item(i).Visible = true;

                } else {
                    //uiState.ListingUIState.Columns.Item(i).Visible = false; 
                }
            }

            var index = 0;
            for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
                if (uiState.ListingUIState.Columns.Item(j).Visible == true) {
                    uiState.ListingUIState.Columns.Item(j).Position = index++;

                }
            }

            vault.ViewOperations.SaveFolderUIStateForFolder(false, false, folderDefs, false, uiState);
        },
        //筛选 GetArray函数中取出的ID
        isMember: function (ids, id) {
            for (var i = 0; i < ids.length; i++) {
                if (ids[i] === id) {
                    return true;
                }
            }
            return false;
        }
    };
u.review = review;
})(CC);