"use strict";


var bim = {
	alias : {
		ObPart:"ObjPart",
		PdIfcId: "PropIfcId",
		pdOwnerModel: "PropOwnedModel"
	},

	createBIMCmds : function (shellFrame) {
        ///<summary>BIM相关命令</summary>
		if (!shellFrame.TaskPane.Available) return;
		
        var bimManagerName = "BIM模型管理";
        var bimManagerId = shellFrame.TaskPane.CreateGroup(bimManagerName, -5);

        var cmdModelPlan = new MF.Command("模型策划", 'icons/模型策划.ico');
        cmdModelPlan.create(shellFrame);
        cmdModelPlan.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\模型策划";
        });
        cmdModelPlan.add2TaskGroup(shellFrame, bimManagerId, 0);

        var bimMouldListName = "模型组织";
        var bimMouldListCmd = new MF.Command(bimMouldListName, 'icons/文件浏览.ico');
        bimMouldListCmd.create(shellFrame);
        bimMouldListCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\" + "BIM模型";
        });
        bimMouldListCmd.add2TaskGroup(shellFrame, bimManagerId, 1);

        var bimMouldBrowseName = "模型浏览";
        var bimMouldBrowseCmd = new MF.Command(bimMouldBrowseName, 'icons/模型浏览.ico');
        bimMouldBrowseCmd.create(shellFrame);
        bimMouldBrowseCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\" + "模型预览";
        });
        bimMouldBrowseCmd.add2TaskGroup(shellFrame, bimManagerId, 2);


		if (shellFrame.CurrentPath === "BIM模型管理\\模型策划") {
            shellFrame.RightPane.Visible = false;
            customData = {};
            shellFrame.ShowDashboard("bimModelPlan", customData);
        } else if (shellFrame.CurrentPath === "BIM模型管理\\模型预览") {
            shellFrame.RightPane.Minimized = true;
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties };
            shellFrame.ShowDashboard("bimmodelbrowser", customData);

        } else if (shellFrame.CurrentPath === "BIM模型管理\\BIM模型") {
            shellFrame.RightPane.Minimized = true;
            customData = { objsAndProps: shellFrame.Listing.items.ObjectVersionsAndProperties, parentPath: shellFrame.CurrentPath };
            shellFrame.ShowDashboard("bimModelList", customData);
        }

    },

	addBimCmd : function(shellFrame) {
		var cmd = shellFrame.Commands.CreateCustomCommand('预览构件');
		shellFrame.Commands.AddCustomCommandToMenu(cmd, MenuLocation_ContextMenu_Top, -1);
		var qrcodeCmd = shellFrame.Commands.CreateCustomCommand('生成二维码');
		shellFrame.Commands.AddCustomCommandToMenu(qrcodeCmd, MenuLocation_ContextMenu_Top, -1);
		var that = this;
		shellFrame.Commands.Events.Register(Event_CustomCommand, function(cmdId) {
			var vault = shellFrame.ShellUI.Vault;
			var listing = shellFrame.ActiveListing;
			var selItems = listing.CurrentSelection;
			if (cmdId === cmd) {				
				if (that._canShowPreviewCmd(vault, selItems)) {
					var err = that._addGuidAndOpenIfc(vault, selItems);
					if (err) {
						shellFrame.ShellUI.ShowMessage(err);
					}
				}				
			} else if (cmdId === qrcodeCmd) {
				if (that._canShowPreviewCmd(vault, selItems)) {
					var qrcodes = [];//fill data
					for (i = 0; i < selItems.Count; i++) {
						var ovap = selItems.ObjectVersionsAndProperties.Item(i + 1);								
						qrcodes[i] = vault.ObjectOperations.GetMFilesURLForObjectOrFile(ovap.ObjVer.ObjID);
					}
					var customdata = {caption: "构件二维码", message: "构件二维码", qrcodes: qrcodes};
					shellFrame.ShowPopupDashboard( "qrcode", true, customdata );
				}
			}
		});
		shellFrame.Commands.SetCommandState(cmd, CommandLocation_All, CommandState_Hidden);
		shellFrame.Commands.SetCommandState(qrcodeCmd, CommandLocation_All, CommandState_Hidden);
		shellFrame.ActiveListing.Events.Register(Event_SelectionChanged, function(sItems) {
			var vault = shellFrame.ShellUI.Vault;
			if (that._canShowPreviewCmd(vault, sItems)) {
				shellFrame.Commands.SetCommandState(cmd, CommandLocation_All, CommandState_Active);
				shellFrame.Commands.SetCommandState(qrcodeCmd, CommandLocation_All, CommandState_Active);
			} else {
				shellFrame.Commands.SetCommandState(cmd, CommandLocation_All, CommandState_Hidden);
				shellFrame.Commands.SetCommandState(qrcodeCmd, CommandLocation_All, CommandState_Hidden);
			}
		});
		
		shellFrame.Events.Register(Event_NewShellListing, function(listing) {
			listing.Events.Register(Event_SelectionChanged, function(sItems) {
				var vault = shellFrame.ShellUI.Vault;
				if (that._canShowPreviewCmd(vault, sItems)) {
					shellFrame.Commands.SetCommandState(cmd, CommandLocation_All, CommandState_Active);
					shellFrame.Commands.SetCommandState(qrcodeCmd, CommandLocation_All, CommandState_Active);
				} else {
					shellFrame.Commands.SetCommandState(cmd, CommandLocation_All, CommandState_Hidden);
					shellFrame.Commands.SetCommandState(qrcodeCmd, CommandLocation_All, CommandState_Hidden);
				}
			});
		});
	},

	_isPartObj: function(vault, objType) {
		var typeId = vault.ObjectTypeOperations.GetObjectTypeIDByAlias(this.alias.ObPart);
		return typeId === objType;
	},

	_canShowPreviewCmd: function(vault, selItems) {
		if (selItems.ObjectVersionsAndProperties.Count !== 1) return false;
		var objType = selItems.ObjectVersionsAndProperties.Item(1).ObjVer.Type;
		if (!this._isPartObj(vault, objType)) return false;
		return true;
	},

	_addGuidAndOpenIfc: function(vault, selItems) {
		var itemAndProps = selItems.ObjectVersionsAndProperties.Item(1);
		var pvs = itemAndProps.Properties;

		var ifcPD = vault.PropertyDefOperations.GetPropertyDefIDByAlias(this.alias.PdIfcId);
		var ifcPV = pvs.SearchForPropertyEx(ifcPD, true);
		if (!ifcPV) {
			return "未找到IFC的唯一标识！";
		}

		var ifcGuid = ifcPV.GetValueAsLocalizedText();

		var ownerPD = vault.PropertyDefOperations.GetPropertyDefIDByAlias(this.alias.pdOwnerModel);
		var ownModelPV = pvs.SearchForPropertyEx(ownerPD, true);
		if (!ownModelPV) {
			return "未找到关联的模型！";
		}
		var owerId = ownModelPV.Value.GetLookupID();

		var scs = MFiles.CreateInstance('SearchConditions');

		var typeSC = MFiles.CreateInstance('SearchCondition');
		typeSC.ConditionType = MFConditionTypeEqual;
		typeSC.Expression.DataStatusValueType = MFStatusTypeObjectTypeID;
		typeSC.TypedValue.SetValue(MFDatatypeLookup, MFBuiltInObjectTypeDocument);
		scs.Add(-1, typeSC);


		var ownerSC = MFiles.CreateInstance('SearchCondition');
		ownerSC.ConditionType = MFConditionTypeEqual;
		ownerSC.Expression.DataPropertyValuePropertyDef = ownerPD;
		ownerSC.TypedValue.SetValue(MFDatatypeLookup, owerId);
		scs.Add(-1, ownerSC);


		var namePattern = MFiles.CreateInstance('SearchCondition');
		namePattern.ConditionType = MFConditionTypeMatchesWildcardPattern;
		namePattern.Expression.SetFileValueExpression(MFFileValueTypeFileName);
    	namePattern.TypedValue.SetValue(MFDatatypeText, "*.ifc");
		scs.Add(-1, namePattern);

		var delSC = MFiles.CreateInstance('SearchCondition');
		delSC.ConditionType = MFConditionTypeEqual;
		delSC.Expression.DataStatusValueType = MFStatusTypeDeleted;
		delSC.TypedValue.SetValue(MFDatatypeBoolean, false);
		scs.Add(-1, delSC);

		var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlagNone, false);

		if (res.Count === 0) return "未能找到对应的IFC文件！";
		

		var ifcObjVersion = res.Item(1);

		var userId = vault.CurrentLoggedInUserID;
		var key = userId + "-"+ifcObjVersion.ObjVer.ID;
		var ns = "BimPartNS";
		var nvs = vault.NamedValueStorageOperations.GetNamedValues(MFUserDefinedValue, ns);
		if (!nvs) {
			nvs = MFiles.CreateInstance("NamedValues");
		}
		nvs.Value(key) = ifcGuid;
		vault.NamedValueStorageOperations.SetNamedValues(MFUserDefinedValue, ns, nvs);

		vault.ObjectFileOperations.OpenFileInDefaultApplication(0, ifcObjVersion.ObjVer, ifcObjVersion.Files.Item(1).FileVer, MFFileOpenMethodOpen);
	}
};