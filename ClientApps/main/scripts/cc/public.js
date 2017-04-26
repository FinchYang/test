/****************************************
 * 公共代码调用文件
 ****************************************/

var publicUtils = {

    getParticipants: function (vault) {
        var propId = 0;
        var arr = md.sharedDoc.propDefs;
        for (var item in arr) {
            if (arr[item] === md.sharedDoc.propDefs.ParticipantAt) {
                propId = MF.alias.propertyDef(vault, arr[item]);
                break;
            }
        }
        return propId;
    },

    setListingCreator: function (folderDefs, vault, participants) {
        var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
        uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;

        if (participants === true) {
            var propId = this.getParticipants(vault);
            for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
                if (uiState.ListingUIState.Columns.Item(j).ID === MFBuiltInPropertyDefCreatedBy ||
                    uiState.ListingUIState.Columns.Item(j).ID === propId) {
                    uiState.ListingUIState.Columns.Item(j).Visible = true;
                }
            }
        } else {
            for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
                if (uiState.ListingUIState.Columns.Item(j).ID === MFBuiltInPropertyDefCreatedBy) {
                    uiState.ListingUIState.Columns.Item(j).Visible = true;
                    break;
                }
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
    },
    //作用：设置动态添加HTML标签的DIV宽度
    //参数：div的id
    SetDivWidth: function (elementId, fontWidth) {
        var getElment = document.getElementById(elementId);
        var getInnerText = getElment.innerText.length;
        if (fontWidth === undefined || fontWidth === null) {
            fontWidth = 8.66;
        }
        getElment.style.width = getInnerText * fontWidth + "px";
    },
    //作用：动态添加HTML标签
    //参数：需要创建listing的个数，容器标签的id
    InsertHtml: function (listingNum, containerId, divClass) {
        //mf-listing-header
        //mf-listing-header mf-listing-new-header
        var idArray = [];
        var divHeight = 100 / listingNum;
        for (var i = 1; i <= listingNum; i++) {
            var listingId = "listing_id" + i;
            idArray.push(listingId);
            var insertText =
           '<div id="' + listingId + '" class="mf-panel" style= "width: 100%; height: ' + divHeight + '%; padding: 0px; margin:0px; border:0px; overflow:hidden;"><div id="' + listingId + 'c" class="' + divClass + '"></div><div class="mf-listing-content"></div></div>';
            $('#' + containerId).first().append($(insertText));
        }
        return idArray;
    },
    //作用：设置Listing以列来显示
    //参数：文件夹ID，shellFrame，valueId，propDefIds
    SetListingHeader: function (folderDefs, shellFrame, propDefIds) { 
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
                uiState.ListingUIState.Columns.Item(i).Visible = false;

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

    getFolderDefs: function (floderId, subFloderId ,subFloderType) {
        //根据ID获取父文件夹对象 
        var folderDefs = MFiles.CreateInstance("FolderDefs");
        var folderDef = MFiles.CreateInstance("FolderDef");
        folderDef.SetView(floderId);
        folderDefs.add(-1, folderDef);
        if (subFloderId || subFloderId === 0) {
            var subFolderDef = MFiles.CreateInstance("FolderDef");
            if (subFloderType === "view") {
                subFolderDef.SetView(subFloderId);
            } else {
                var propTvalue = MFiles.CreateInstance("TypedValue");
                propTvalue.SetValue(MFDatatypeLookup, subFloderId);
                subFolderDef.SetPropertyFolder(propTvalue);
            }
            folderDefs.add(-1, subFolderDef);
        }

        return folderDefs;
    }
}
