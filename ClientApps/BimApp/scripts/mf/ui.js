/***********************************************
 * M-Files UI Operation...
 * like View, Menu, Dialog etc...
 * export name: MF.ui
 * dependency: client.js, vault.js
 ************************************************/
var MF = MF || {};
(function (u, undefined) {
    var ui = {
        ///<summary>m-files ui utils</summary> 

        setCmdsState: function (shellFrame, location, state, cmds) {
            ///<param name="cmds" type="Array">命令ID的数组</param>
            for (var i = 0; i < cmds.length; i++) {
                shellFrame.Commands.SetCommandState(cmds[i], location, state);
            }
        },

        setCurrentPath: function(shellFrame, relativePath) {
            try {
                shellFrame.CurrentPath = relativePath;
            } catch (e) {
                shellFrame.ShellUI.ShowMessage('路径(' + relativePath + ')没有权限或不存在！');
            }
        },

        setCurrentView: function(shellFrame, viewId, subFolder) {
            ///<param name="subFolder" type="String">视图下的子文件夹名称</param>
            var vault = shellFrame.ShellUI.Vault;
            try {
                var location = vault.ViewOperations.GetViewLocationInClient(viewId, true);
                var url = u.vault.getVaultURL(vault);
                var folderPath = location.substring(url.length);
                if (subFolder && subFolder !== '') {
                    folderPath = folderPath + subFolder;
                }
                this.setCurrentPath(shellFrame, folderPath);
            } catch(e) {
                shellFrame.ShellUI.ShowMessage('视图(ID=' + viewId + ')没有权限或不存在！');
            }

        },
        
        getFilePath : function(shellFrame, objVersion, fileIndex) {
            ///<summary>获得指定文件的全路径</summary>
            ///<param name="objVersion" type="ObjectVersion">M-Files对象</param>
            ///<param name="fileIndex" type="int">在对象文件中的编号</param>
            var vault = shellFrame.ShellUI.Vault;
            if (fileIndex < 0 || fileIndex > objVersion.FilesCount) {
                throw new Error('文件的编号超出了文件列表的长度');
            }
            var file = objVersion.Files.Item(fileIndex);
            return vault.ObjectFileOperations.GetPathInDefaultView(
                objVersion.ObjVer.ObjID, objVersion.ObjVer.Version, file.ID, file.Version,
                MFLatestSpecificBehaviorAutomatic, false);
        },

        createNewObjectWithWindow: function (shellFrame, objTypeID) {
            var vault = shellFrame.ShellUI.Vault;
            var objType = vault.ObjectTypeOperations.GetObjectType(objTypeID);
            objType.ID = objTypeID;
            return vault.ObjectOperations.ShowBasicNewObjectWindow(0, objType);
        },

        createNewObjectShowWindow: function(shellFrame, objTypeID, classID, propertyValues, templateObjVer) {
            ///<param name="templateObjVer" type="ObjVer">ObjVer of template object</param>
            var oVault = shellFrame.ShellUI.Vault;
            var pvs = u.createObject("PropertyValues");
            if (classID || classID.value === 0) {
                var pvClass = u.vault.createProperty(classID.type, MFDatatypeLookup, classID.value);
                pvs.Add(-1, pvClass);
            }
            if (propertyValues && propertyValues.Count > 0) {
                for (var i = 1; i <= propertyValues.Count; i++) {
                    pvs.Add(-1, propertyValues.Item(i));
                }
            }
            var objCreationInfo = u.createObject("ObjectCreationInfo");
            objCreationInfo.SetObjectType(objTypeID, false);
            objCreationInfo.SetSingleFileDocument(true, true);
            if (templateObjVer) {
                objCreationInfo.SetTemplate(templateObjVer);
            }
            var acl = u.createObject("AccessControlList");

            oVault.ObjectOperations.ShowPrefilledNewObjectWindow(0, MFObjectWindowModeInsert, objCreationInfo, pvs, acl);
        },

        createCustomTaskGroup: function(shellFrame, groupName, orderPriority, theme) {
            ///<summary>创建自定义任务栏组</summary>
            if (!shellFrame.TaskPane.Available) return;
            var groupId = shellFrame.TaskPane.CreateGroup(groupName, orderPriority);
            theme = theme || { groupHeader_TextColor: '#5182C4', groupHeader_FontSize: '135%' };
            try {
                shellFrame.TaskPane.SetTheme(theme);
            } catch(e) {

            }
            return groupId;
        },

        createCommandAtTaskGroup: function(shellFrame, groupId, mfCustomCmd, orderPriority) {
            ///<param name="mfCustomCmd" type="MfCommand"></param>
            ///<param name="orderPriority" type="long">Ascending ordering.
            /// If the group contains built-in commands,
            /// then negative values are sorted before the built-in commands and positive values after them. 
            /// </param>
            if (!shellFrame.TaskPane.Available) return;
            var cmdName = mfCustomCmd.name;
            var cmdId = mfCustomCmd.command;
            if (!cmdId && cmdId !== 0) {
                cmdId = shellFrame.Commands.CreateCustomCommand(cmdName);
            }
            mfCustomCmd.command = cmdId;
            var iconPath = mfCustomCmd.iconPath;
            if (iconPath && iconPath !== '') {
                shellFrame.Commands.SetIconFromPath(cmdId, iconPath);
            }
            shellFrame.TaskPane.AddCustomCommandToGroup(cmdId, groupId, orderPriority);
            return cmdId;
        },

        getVault: function(dashboard) {
            var vault;
    	    try {
    	        vault = dashboard.Parent.ShellUI.Vault; //shellFrame is the parent
    	    } catch (e) {
    	        try {
    	            vault = dashboard.Parent.ShellFrame.ShellUI.Vault; //IShellPaneContainer is the parent
    	        } catch (e) {
    	            vault = dashboard.Parent.Vault; //shellUI or vaultUI is the parent
    	        }
    	    }
            return vault;
        },

        getShellFrame: function(dashboard) {
            var vault;
            var shellFrame = null;
    	    try {
    	        vault = dashboard.Parent.ShellUI.Vault; //shellFrame is the parent
                shellFrame = dashboard.Parent;
    	    } catch (e) {
    	        try {
    	            vault = dashboard.Parent.ShellFrame.ShellUI.Vault; //IShellPaneContainer is the parent
                    shellFrame = dashboard.Parent.ShellFrame;
    	        } catch (e) {
    	            vault = dashboard.Parent.Vault; //shellUI or vaultUI is the parent
    	        }
    	    }
            return shellFrame;
        },

        _iconCache: {},

        getIconFromUI: function(shellFrame, valueList) {
            var key = "0-" + valueList;
            var path = this._iconCache[key];
            // found something in the cache! return it and be done
            if (path !== undefined) {
                return path;
            }
            try {
                path = shellFrame.ShellUI.GetObjectTypeIconURL(valueList);
                return path;
            } catch(e) {
                return this.getIcon(shellFrame.ShellUI.Vault, valueList);
            }
        },

        getIcon: function (vault, valueList, valueListItem) {
            /// <signature>
            ///     <summary>
            ///      Resolves the icon for an ObjectType, saves it locally, and returns the path.
            ///      Assumes 'this.vault' has been set (i.e., $.mf.vault).
            ///     </summary>
            ///     <param name="vault" type="Vault"></param>
            ///     <param name="objectTypeID" type="Number"></param>
            ///     <returns type="string">Icon Path</returns>
            /// </signature>
            /// <signature>
            ///     <summary>
            ///      Resolves the icon for a ValueListItem, saves it locally, and returns the path.
            ///      Assumes 'this.vault' has been set (i.e., $.mf.vault).
            ///     </summary>
            ///     <param name="vault" type="Vault"></param>
            ///     <param name="valueList" type="Number"></param>
            ///     <param name="valueListItem" type="Number"></param>
            ///     <returns type="string">Icon Path</returns>
            /// </signature>


            var key = (valueListItem === undefined) ? "0-" + valueList : valueList + "-" + valueListItem,
                path,
				fs,
				//icon,
				//stream,
				adBinaryMode = 1,
				adCreateOverwriteMode = 2;

            //check cache
            path = this._iconCache[key];

            // found something in the cache! return it and be done
            if (path !== undefined) {
                return path;
            }

            // Start up the file sytem object and build our path
            fs = new ActiveXObject('Scripting.FileSystemObject');
            path = fs.GetSpecialFolder(2);

            // Create ui ext icon folder
            path += "\\CADS_UIEXT_Icons";
            if (!fs.FolderExists(path)) {
                fs.CreateFolder(path);
            }

            // create a folder for each vault
            path += "\\" + vault.getGUID();
            if (!fs.FolderExists(path)) {
                fs.CreateFolder(path);
            }

            // add file to path
            path += "\\" + key + ".ico";

            // if file already exists, don't load it...
            if (fs.FileExists(path)) {
                return path;
            }


            // just in case something goes wrong (like the item doesn't have an icon)
            try {
                var icon;
                // load the image
                if (valueListItem === undefined) {

                    // ObjectType icon
                    icon = vault.ObjectTypeOperations.GetObjectType(valueList).Icon;

                } else {

                    //ValueList icon
                    icon = vault.ValueListItemOperations.GetValueListItemByID(valueList, valueListItem).Icon;
                }

                // save icon data to file
                var stream = new ActiveXObject("ADODB.Stream");
                stream.Type = adBinaryMode;
                stream.Open();
                stream.Write(icon);
                stream.SaveToFile(path, adCreateOverwriteMode);

                // save to cache
                this._iconCache[key] = path;

                // return the local path to the icon
                return path;

            } catch (e) {

                // cache that we were unable to generate an icon
                this._iconCache[key] = "";
            }

        },

        getCurrentListing: function(shellFrame) {
            var listing;
            try {
                listing = shellFrame.ActiveListing;
            } catch (e) {
                listing = shellFrame.Listing;
            }
            return listing;
        },
		
        selectObject: function (shellFrame, objVer, novirtial) {
            var listing = this.getCurrentListing(shellFrame);
            if (!novirtial) {
                var objOrFileVers = MF.createObject("ObjOrFileVers");
                var objOrFileVer = MF.createObject("ObjOrFileVer");
                objOrFileVer.ObjVer = objVer;
                objOrFileVers.Add(-1, objOrFileVer);
                try {
                    listing.UnselectAll();
                    listing.SetVirtualSelection(objOrFileVers);
                } catch (e) {
                    if (setTimeout) {
                        setTimeout(function () { selObj0(shellFrame, listing, objVer); }, 0);
                    } else {
                        selObj0(shellFrame, listing, objVer);
                    }
                }
            } else {
                if (setTimeout) {
                    setTimeout(function () { selObj0(shellFrame, listing, objVer); }, 0);
                } else {
                    selObj0(shellFrame, listing, objVer);
                }
            }

            function selObj0(sf, li, ov) {
                
                try {
					// select the item natively in the listing so tasks and properties pane are shown
					// this will succeed on top level items
                    li.SelectObjectVersion(ov);
                    if (sf.BottomPane.Visible) sf.BottomPane.ShowDefaultContent();

				} catch (ee) {
					// lower level items will fall back to this
					// no tasks, manually load properties into property pane (double-click is broken this way ;-( )
				    if (sf.BottomPane.Visible) {
				        sf.BottomPane.ShowObjectVersionProperties(sf.ShellUI.vault.ObjectPropertyOperations.GetPropertiesWithIconClues(ov, false));
				    }
					li.UnselectAll();
				}
            }
        }
    };
    u.ui = ui;
}(MF));