/*************
*众包管理相关操作
**************/
var CC = CC || {};
(function(u) {
    var c = {
        //改变状态为=>招投标中
        publishTask: function(sf,objVn,publishId) {
            var vault = sf.ShellUI.Vault;
            var vlId = MF.alias.objectType(vault, md.valueList.CrowdSrcTaskState);
            var itemId = this._getValueListItem(vault, vlId, "招投标中");
            var pvs = MFiles.CreateInstance("PropertyValues");
            if (itemId > 0) {
                //任务状态
                var propIdStatus = MF.alias.propertyDef(vault, md.crowdSrcTask.propDefs.CrowdSrcState);
                var pvStatus = MFiles.CreateInstance("PropertyValue");
                pvStatus.PropertyDef = propIdStatus;
                pvStatus.TypedValue.SetValue(MFDatatypeLookup, itemId);
                pvs.Add(-1, pvStatus);
                
            } else {
                sf.ShellUI.ShowMessage("不存在<招投标中>状态，请核实！");
            }
            if (publishId) {
                //发布ID
                var propIdPubId = MF.alias.propertyDef(vault, md.crowdSrcTask.propDefs.PublishID);
                var propPubId = MFiles.CreateInstance('PropertyValue');
                propPubId.PropertyDef = propIdPubId;
                propPubId.TypedValue.SetValue(MFDatatypeText, publishId);
                pvs.Add(-1, propPubId);
            }
            if (pvs.Count > 0) {
                try {
                    var checkOutVn;
                    if (objVn.ObjectCheckedOut) checkOutVn = objVn;
                    else {
                        checkOutVn = vault.ObjectOperations.CheckOut(objVn.ObjVer.ObjID);
                    }
                    vault.ObjectPropertyOperations.SetProperties(checkOutVn.ObjVer, pvs);
                    vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
                    sf.ShellUI.ShowMessage("<" + objVn.Title + ">已发布完毕！");
                } catch (e) {
                    sf.ShellUI.ShowMessage(e.message);
                }
            }
        },
        createTask: function (sf) {
            var vault = sf.ShellUI.Vault;
            var classId = MF.alias.classType(vault, md.crowdSrcTask.classAlias);
            var pvs = MFiles.CreateInstance("PropertyValues");
            var pvClass = MFiles.CreateInstance("PropertyValue");
            pvClass.PropertyDef = MFBuiltInPropertyDefClass;
            pvClass.TypedValue.SetValue(MFDatatypeLookup, classId);
            pvs.Add(-1, pvClass);
            var vlId = MF.alias.objectType(vault, md.valueList.CrowdSrcTaskState);
            var itemId = this._getValueListItem(vault, vlId, "编辑中");
            if (itemId > 0) {
                var propIdStatus = MF.alias.propertyDef(vault, md.crowdSrcTask.propDefs.CrowdSrcState);
                var pvStatus = MFiles.CreateInstance("PropertyValue");
                pvStatus.PropertyDef = propIdStatus;
                pvStatus.TypedValue.SetValue(MFDatatypeLookup, itemId);
                pvs.Add(-1, pvStatus);
            }
            MF.ObjectOps.createObjPrefilled(vault, pvs, MFBuiltInObjectTypeAssignment, undefined);
        },
        _getValueListItem: function (vault, valueListId, itemName) {
            ///<summary>根据值列表项的名称获取对应的值列表项ID</summary>
            var values = vault.ValueListItemOperations.GetValueListItems(valueListId);
            var id = -1;
            for (var i = 1; i <= values.Count; i++) {
                if (values.Item(i).Name === itemName) {
                    id = values.Item(i).ID;
                    break;
                }
            }
            return id;
        },
        //作用：动态添加HTML标签
        //参数：需要创建listing的个数，容器标签的id
        insertHtml: function (listingNum, containerId) {
            var idArray = [];
            var divHeight = 100 / listingNum;
            for (var i = 1; i <= listingNum; i++) {
                var listingId = "listing_id" + i;
                idArray.push(listingId);
                var insertText =
               '<div id="' + listingId + '" class="mf-panel" style= "width: 100%; height: ' + divHeight + '%; padding: 0px; margin:0px; border:0px; overflow:hidden;"><div id="' + listingId + 'c" class="mf-listing-header mf-listing-new-header"></div><div class="mf-listing-content"></div></div>';
                $('#' + containerId).first().append($(insertText));
            }
            return idArray;
        },
        /*
        //作用：设置Listing以列的显示
        //参数：文件夹ID，shellFrame，valueId，propDefIds
        setListingHeader: function (shellFrame, floderId, subFloderId) {
            
            //根据ID获取父文件夹对象
            var vault = shellFrame.ShellUI.Vault;
            var folderDefs = MFiles.CreateInstance("FolderDefs");
            var folderDef = MFiles.CreateInstance("FolderDef");
            folderDef.SetView(floderId);
            folderDefs.add(-1, folderDef);

            //获取子视图ID
            //var folderDefProp = MFiles.CreateInstance("FolderDef");
            //var propTvalue = MFiles.CreateInstance("TypedValue");
            //propTvalue.SetValue(MFDatatypeLookup, valueId);
            //folderDefProp.SetPropertyFolder(propTvalue);
            //folderDefs.add(-1, folderDefProp);
            var subFolderDef = MFiles.CreateInstance("FolderDef");
            subFolderDef.SetView(subFloderId);
            folderDefs.add(-1, subFolderDef);
            var propDefIds = this.getHeadCols(vault);

            var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
            uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;

            for (var i = 1; i <= uiState.ListingUIState.Columns.Count; i++) {
                //获取Listing中的列ID
                var id = uiState.ListingUIState.Columns.Item(i).ID;
                //将数组中需要显示的ID和Listing中匹配的列显示出来
                if (this._isMember(propDefIds, id) == true ||
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
                    //uiState.ListingUIState.Columns.Item(j).Position = index++;
                    if (uiState.ListingUIState.Columns.Item(j).ID === MFFolderColumnIdName) {
                        uiState.ListingUIState.Columns.Item(j).Position = 0;
                    } else {
                        uiState.ListingUIState.Columns.Item(j).Position = ++index;
                    }
                }
            }

            vault.ViewOperations.SaveFolderUIStateForFolder(false, false, folderDefs, false, uiState);
        },
        _isMember: function (ids, id) {
            for (var i = 0; i < ids.length; i++) {
                if (ids[i] == id) {
                    return true;
                }
            }
            return false;
        },
        */
        //listing中要显示的列(属性ID)
        getHeadCols: function (vault) {
            var res = [];
            var aliaArr = [md.crowdSrcTask.propDefs.Budget,
                            //parseInt(MFBuiltInPropertyDefDeadline),
                           md.crowdSrcTask.propDefs.ClosingCost,
                           //"MFBuiltInPropertyDefAssignedTo",
                           md.crowdSrcTask.propDefs.CrowdSrcState];
            for (var item in aliaArr) {
                var propId = MF.alias.propertyDef(vault, aliaArr[item]);
                res.push(propId);
            }
            //res.push(parseInt(MFBuiltInPropertyDefAssignedTo));
            res.push(parseInt(MFBuiltInPropertyDefDeadline));
            
            return res;
        }
    };
    u.crowdsourcing = c;
})(CC);
