// anonymous closure encapsulating jQuery shorthand
(function ($, undefined) {

	// Default GanttItem map properties
	var GanttItemMap = {
		properties: {
			type: "Activity", //
			timedEvent: false,
			label: "%0%",
			icon: -1,
			startDate: -1,
			endDate: -1,
			startTime: -1,
			endTime: -1,
			progress: -1,
			duration: -1,
			durationUnit: -1,
			recurInterval: -1,
			recurUnit: -1,
			dependency: -1,
			allocation: {
				objectType: -1,
				resource: -1,
				target: -1
			}
		},
		relationships: {
			to: [],
			from: []
		},
		options: {
			showProgress: true,
			showLabel: "Left"
		},
		itemStyle: {
			fill: "270-#ffffff-#888888",
			stroke: "#888888",
			//"fill-opacity":.5,
			//"stroke-opacity":.5
		},
		progressStyle: {
			fill: "#888888",
			"fill-opacity": .5,
			"stroke-width": 1,
			"stroke": "#888888",
			"stroke-opacity": .5
		},
		labelStyle: {
			'text-anchor': 'end',
			fill: "#666666",
			"font-style": "normal",
			"font-size": "10px",
			"font-family": "Segoe UI"
		}
	}

	// GanttItemMap properties wich are objects, whose properties can be inherited
	var extendMapProps = ["properties", "relationships", "options", "itemStyle", "progressStyle", "labelStyle"];


	var GanttMFProperty = function (props, propId) {
		/// <summary> GanttMFProperty Constructor: utility object for extacting data from a property</summary>
		/// <param name="props" type="MFilesAPI.PropertyValues"> 
		///  The property values from which our property should be found
		/// </param>
		/// <param name="propId" type="number"> the id of the property </param>


		// save the prop id as an object property
		this.id = propId;

		// try to get a handle on the actual PropertyValue
		if (props.IndexOf(propId) != -1) {
			this.prop = props.SearchForProperty(propId);
		}
	}


	// GanttMFProperty Methods
	$.extend(GanttMFProperty.prototype, {


		toString: function () {
			/// <summary> Gets the string value of the property value </summary>
			/// <returns type="string"> </returns>

			// check if there is a valid property value and it isn't null
			if (!this.prop || this.prop.Value.IsNULL()) {
				
				// return string indicating there is no value
				return "-";
			
			} else {

				// return property's string value
				return this.prop.GetValueAsLocalizedText();
			}
		},

		valueOf: function (forSort) {
			/// <summary> 
			///  Gets the value of the property value normalized for javascript
			///   Handles lookup and date properties specially
			/// </summary>
			/// <param name="forSort" type="bool"> indicates if the value will be used for sorting </param>
			/// <returns>Return type varies </returns>

			var objType;


			// if there is no property value or it's null, return undefined
			if (!this.prop || this.prop.Value.IsNULL()) {
				return;
			}


			// handle property values based on their datatypes
			switch (this.prop.value.datatype) {

				//MFDatatypeDate
				case 5:
				case 6:

					// convert value into javascript Date object
					return new Date(this.prop.Value.Value);

				//MFDatatypeLookup
				case 9:

					// check if we're sorting
					if (!forSort) {

						// not sorting, return objID in string form
						objType = $.mf.vault.PropertyDefOperations.GetPropertyDef(this.prop.PropertyDef).ValueList;
						return objType + "-" + this.prop.Value.GetLookupID();

					} else {

						// sorting... return text value of lookup
						return this.prop.GetValueAsLocalizedText();
					}

				//MFDatatypeMultiSelectLookup
				case 10:

					// check if we're sorting
					if (!forSort) {

						// not sorting, return array of the objIDs in string form

						objType = $.mf.vault.PropertyDefOperations.GetPropertyDef(this.prop.PropertyDef).ValueList;

						// loop over values
						return $.map(this.prop.Value.GetValueAsLookups(), function (v, i) {
							
							// return objID in string format
							return objType + "-" + v.Item;
						});

					} else {

						// sorting... return text value of multiselect lookup
						return this.prop.GetValueAsLocalizedText();
					}

				default:
					// no special data type, simply return the value
					return this.prop.Value.Value;

			}
		}

	});


	/// <summary>
	///
	/// 	MFGANTT ( jQuery Widget )
	///
	/// <summary>	
	$.widget("mf.mfgantt", {

		// default widget options
		options: {
			baseStorageNamespace: "MFiles.GanttView",
			// Item Map Defs
			defs: {
				vaultObjTypes: {
					"default": {}
				},
				viewObjTypes: {},
				vaultClasses: {},
				viewClasses: {},
				vaultTimeUnits: {},
			},
			behavior: {
				autoRefresh: false,
				autoRefreshInterval: 5
			}
		},


		_init: function () {
			/// <summary> standard jQuery widget init function </summary>

			var that = this,
				ganttOptions;

			// load relevant settings from the vault
			this._loadSettings();
			
			// setup our gantt options based on the loaded settings
			ganttOptions = {
				shellFrame: $.mf.shellFrame,
				defs: {
					vaultObjTypes: this._vaultSettings.vaultObjTypes,
					vaultClasses: this._vaultSettings.vaultClasses,
					viewObjTypes: this._viewSettings.viewObjTypes,
					viewClasses: this._viewSettings.viewClasses,
					vaultTimeUnits: this._vaultSettings.timeUnits,
				},
				commands: this._getCommands()
			};

			if (this._vaultSettings.nonWorkingTime) {
				ganttOptions["nonWorkingTime"] = this._vaultSettings.nonWorkingTime;
			}

			// Initialize gantt chart
			this.element.gantt(ganttOptions);

			// obtain direct reference to our gantt widget object
			this.gantt = this.element.data("gantt");


			// bind to the gantt chart's item event handlers
			this._bindItemEvents();

			// initalize the configuration panel
			this._initConfPanel();

			// bind to the content of the view's listing and load initial content
			this._bindToListing();

			// stay aware of random changes
			this._bindToVault();

			//scroll and adjust zoom level to show content
			this.gantt.autoFit();

			// start refresh interval, if applicable
			this.updateRefreshInterval();
		},

		_bindToListing: function () {
			/// <summary> Loads the listing's initial content and listens for changes to keep it up to date </summary>

			var that = this;

			// bind to the listing, so our list of top level items is always up to date 
			this.shellFrameHandler = $.mf.shellFrame.Listing.Events.Register(Event_ContentChanged, function (items) {
				that._updateContent(items.ObjectVersionsAndProperties);
			});

			// load the listing's current items
			this._updateContent($.mf.shellFrame.Listing.Items.ObjectVersionsAndProperties);

			this.gantt.mflist._sort();

		},

		_bindToVault: function () {
			/// <summary>
			///  Binds to our vault core module and listens for any random changes
			///  that occur in this vault, on this client, and automatically 
			///  updates, inserts and/or removes items as needed
			/// </summary>

			var that = this,
                id = $.mf.shellUI.NotifyVaultEntry("VaultChangeTracker", "attach", "");

			// set the id in the customData variable, so the module will unregister the client on close
			$.mf.customData.vaultListenerID = id;

			// poll for changes
			setInterval(function () {

				try {

					// query changes in vault since last we queried, and parse the json response into an object
					var changesStr = $.mf.shellUI.NotifyVaultEntry("VaultChangeTracker", "getChanges", id),
						changes = JSON.parse(changesStr);					

					// check if anything has changed
					if (changes.deleted.length || changes.updated.length || changes.created.length) {


						// something has changed!

						// suspend the layout til all our updates have been made
						that.gantt.suspendLayout();

						// check if any items have been deleted (removed) from the vault
						if (changes.deleted.length) {

							// loop over deleted objects
							$.each(changes.deleted, function (i, id) {
								
								// get a handle on every item in the chart representing the deleted object
								// and remove them from the chart

								var items = that.gantt.getItemsByID(id);

								if (items.length) {
									that.gantt.removeItems(items);
								}
							});
						}


						// check if any items have been updated
						if (changes.updated.length) {

							// loop over updated objects
							$.each(changes.updated, function (i, id) {

								var objID = MFiles.CreateInstance("ObjID"),
									idParts = id.split("-"),
									ovap;

								// get the latest version info and properties of the updated object
								objID.SetIDs(parseInt(idParts[0]), parseInt(idParts[1]));
								ovap = $.mf.vault.ObjectOperations.GetLatestObjectVersionAndProperties(objID, true, false);								

								// update gantt listing structure to make sure all items representing the updated object
								// appear in the correct position
								//that._refreshHierarchyPlacement(ovap.VersionData, ovap.Properties);

								// loop over each item representing the updated object
								$.each(that.gantt.getItemsByID(id), function (j, item) {

									// update the item itself
									that._mapItem(ovap.VersionData, ovap.Properties, item);
									that.gantt.updateItems(item);

									// make sure the item has the correct children based on the updates
									//that._refreshChildHierarchyPlacement(item);
								});

							});
						}


						// check if any new object have been created
						if (changes.created.length) {

							// loop over newly created objects
							$.each(changes.created, function (i, id) {
								var objID = MFiles.CreateInstance("ObjID"),
									idParts = id.split("-"),
									ovap;

								// get new object's version info and properties
								objID.SetIDs(parseInt(idParts[0]), parseInt(idParts[1]));
								ovap = $.mf.vault.ObjectOperations.GetLatestObjectVersionAndProperties(objID, true, false);

								// place the new object in the correct place in the hierarchy (if at all)
								that._refreshHierarchyPlacement(ovap.VersionData, ovap.Properties);


							});
						}


						// refresh chart now that we've done all our changes
						that.gantt.resumeLayout();
					}
				} catch(e) {}

			}, 1000);

		},

		_getCommands: function () {
			/// <summary> Get's command definitions to pass to the gantt widget </summary>

			var that = this;

			return {

				// Toggle Configuration Panel command
				conf: {
					title: "设置", //Settings
					icon: "images/icons/conf.png",
					keycode: "Alt+C",
					key: [18, 67], //  Alt+C
					align: "right",
					showText: false,
					useModalOverlay: false,
					trigger: function (e) {

						// toggle the visibility of the configuration panel
						that._confVisible = !that._confVisible;
						that._updateConfPanel();
					}
				},

				// Refresh Chart command
				/*
				print: {
					title: "Print",
					icon: "images/icons/printer.png",
					keycode: "Ctrl+P",
					align: "right",
					showText: false,
					key: [17, 80], //  Ctrl+P
					trigger: function (e) {
						
						// print gantt chart
						that.print();
					}
				},
				*/

				// Refresh Chart command
				refresh: {
					title: "刷新图表", //Refresh Chart
					icon: "images/icons/reset_small.png",
					keycode: "F5",
					align: "right",
					showText: false,
					key: 116, //F5
					trigger: function (e) {
						
						// reload all gantt content
						that.reloadAll();
					}
				},

				// Auto fit command
				autoFit: {
					title: "缩放适应视图", //Zoom to Fit
					icon: "images/icons/AutoFit.png",
					keycode: "Ctrl+Z",
					align: "right",
					showText: false,
					useModalOverlay: false,
					key: [17, 90], //Ctrl+Z
					trigger: function (e) {

						// zoom to fit items in the chart
						that.gantt.autoFit();
					}
				},

				// Select All command
				selectAll: {
					title: "全选", //Select All
					icon: "images/icons/CheckInAll.png",
					keycode: "Ctrl+A",
					key: [17, 65], // Ctrl+A 
					visible: false,
					useModalOverlay: false,
					trigger: function (e) {

						var items = $.map(that.gantt.getItems(), function (item) {
								return item;
						});

						that.gantt.setSelectedItems(items);
						
					}
				},

				// Check in All command
				checkInAll: {
					title: "签入所有", //Check In All
					icon: "images/icons/CheckInAll.png",
					keycode: "Ctrl+Alt+I",
					key: [17, 18, 73], // Ctrl+I
					trigger: function (e) {

						var items = that.gantt.getItems(),
							result = that._checkInItems(items, false);

						// refresh the items on the chart
						that.gantt.updateItems(result.updated);
					}
				},

				// Check out selected items command
				checkOut: {
					title: "签出", //Check Out
					icon: "images/icons/CheckOut.png",
					keycode: "Ctrl+Alt+O",
					key: [17, 18, 79], // Ctrl+Alt+O
					enabled: function(e) {
						var items = [],
							enabled = false;

						// get selected items
						if( that.gantt ) {
							items = that.gantt.getSelectedItems();							
						}

						// loop over selected items
						$.each(items, function(i, item) {

							// if there is an item that can be checked out, enable the button and break
							if( item.editable && !item.verData.ObjectCheckedOut ) {
								enabled = true;
								return false;
							}
						});						

						return enabled;
					},
					trigger: function (e) {

						var items = that.gantt.getSelectedItems(),
							result = that._checkOutItems(items, false);

						// refresh the items on the chart
						//that.gantt.updateItems(result.updated);

					}
				},

				// Check in selected items command
				checkIn: {
					title: "签入", //Check In
					icon: "images/icons/CheckIn.png",
					keycode: "Ctrl+I",
					key: [17, 73], // Ctrl+I
					enabled: function(e) {
						var items = [],
							enabled = false;

						// get selected items
						if( that.gantt ) {
							items = that.gantt.getSelectedItems();							
						}

						// loop over selected items
						$.each(items, function(i, item) {

							// if there is an item that can be checked in, enable the button and break
							if( item.verData.ObjectCheckedOutToThisUser ) {
								enabled = true;
								return false;
							}
						});						

						return enabled;
					},
					trigger: function (e) {

						var items = that.gantt.getSelectedItems(),
							result = that._checkInItems(items, false);

						// refresh the items on the chart
						that.gantt.updateItems(result.updated);
					}
				},


				// Undo checkout command
				undoCheckOut: {
					title: "撤销签出", //Undo Check Out
					icon: "images/icons/UndoCheckout.png",
					keycode: "Ctrl+U",
					key: [17, 85], // Ctrl+U
					enabled: function(e) {
						var items = [],
							enabled = false;

						// get selected items
						if( that.gantt ) {
							items = that.gantt.getSelectedItems();							
						}

						// loop over selected items
						$.each(items, function(i, item) {

							// if there is an item that can have its checkout undone, enable the button and break
							if( item.verData.ObjectCheckedOutToThisUser ) {
								enabled = true;
								return false;
							}
						});						

						return enabled;
					},
					trigger: function (e) {

						// loop over selected items
						var toUpdate = $.map(that.gantt.getSelectedItems(), function (item) {

							// only handle if it's checked out to this user
							if (item.verData.ObjectCheckedOutToThisUser) {

								// undo check out for item
								var verData = $.mf.vault.ObjectOperations.UndoCheckOut(item.objVer),
									props = $.mf.vault.ObjectPropertyOperations.GetProperties(verData.ObjVer, true);

								// update item
								return that._mapItem(verData, props, item);

							}

						});

						// refresh items in the chart
						that.gantt.updateItems(toUpdate);

					}
				},

				deleteItem: {
					title: "删除", //Delete
					icon: "images/icons/Delete.png",
					keycode: "Del",
					key: [46], // Del
					enabled: function(e) {
						var items = [],
							enabled = false;

						// get selected items
						if( that.gantt ) {
							items = that.gantt.getSelectedItems();							
						}

						// loop over selected items
						$.each(items, function(i, item) {

							// if there is an item that can be deleted, enable the button and break
							if( item.editable ) {
								enabled = true;
								return false;
							}
						});						

						return enabled;
					},
					trigger: function (e) {

						
						var message = {
							caption: "确认删除", //Confirm Delete
							message: "您确定要删除选中的对象吗？", //Are you sure you want to delete the selected objects?
							//icon: "question",
							button1_title: "是", //Yes
							button2_title: "否", //No
							defaultButton: 1
						},
							button, toRemove;


						// confirm delete
						button = $.mf.shellFrame.ShowMessage(message);

						if (button === 1) {

							// confirmed

							// loop over items
							toRemove = $.map(that.gantt.getSelectedItems(), function (item) {

								// remove item
								$.mf.vault.ObjectOperations.RemoveObject(item.objVer.ObjID);

								return item;

							});

							// remove all the items from the chart
							that.gantt.removeItems(toRemove);

							that.gantt.setSelectedItems();
						}
						

					}
				},

			};

		},


		// #########################################
		//
		//  SETTINGS PERSISTENCE METHODS
		//
		// #########################################

		_getView: function () {
			/// <summary> Finds the inner most view in the current path </summary>
			/// <returns type="MFilesAPI.View"></returns>

			// Check if we have cached the view already
			if (!this._view) {

				// we haven't cached the view, so we load it
				
				// loop through folder path (in reverse).
				for( var i=$.mf.shellFrame.CurrentFolder.Count; i>0; i-- ) {
					var folderDef = $.mf.shellFrame.CurrentFolder.Item(i);

					// detect if this part of the path represents a view
					if (folderDef.FolderDefType == MFFolderDefTypeViewFolder) {

						// view found!  grab handle and break
						this._view = $.mf.vault.ViewOperations.GetView(folderDef.View);
						break;
					}
				}

			}

			return this._view;
		},

		_loadSettings: function () {
			/// <summary> Load gantt settings from Vault NamedValueStorage </summary>


			// resolve user's language for month names, etc...
			this._loadLanguage();

			// get the current view
			var view = this._getView();

			// load general vault settings
			this._vaultSettings = $.mf.settings("MFiles.GanttView.VaultSettings", MFFolderConfiguration);

			// load view specific settings
			this._viewSettings = $.mf.settings("MFiles.GanttView." + view.ID + ".ViewSettings", (view.Common) ? MFFolderConfiguration : MFUserDefinedValue);

			// load user specific settings
			this._userSettings = $.mf.settings("MFiles.GanttView." + view.ID + ".UserSettings", MFUserDefinedValue);

			$.extend(this.options, {
				defs: {
					vaultObjTypes: this._vaultSettings.vaultObjTypes || { "default": {} },
					vaultClasses: this._vaultSettings.vaultClasses,
					viewObjTypes: this._viewSettings.viewObjTypes,
					viewClasses: this._viewSettings.viewClasses,
					vaultTimeUnits: this._vaultSettings.timeUnits || [],					
				},
				behavior: this._viewSettings.behavior//this._userSettings.behavior
			});


			// initialize the item maps now that the definitions have been loaded
			this._initMaps();

		},

		_loadLanguage: function () {
			/// <summary> Detects the current langauge settings for the client to determine date format, etc.. </summary>

			var langPriority = [navigator.systemLanguage, navigator.browserLanguage, "zh-CN"], // en-US
				fso = new ActiveXObject('Scripting.FileSystemObject'),
				basePath = fso.GetParentFolderName(decodeURI(location.pathname)).substr(1),
				fileFormat = "js/date/date-{0}.js",
				file;

			$.each($.mf.vault.VaultLanguages, function (i, lang) {
				if (lang.id === $.mf.vault.SessionInfo.Language) {
					langPriority.unshift(lang.LanguageCode);
				}
			});

			$.each(langPriority, function (i, langCode) {
				if (langCode) {
					file = basePath + "/" + fileFormat.replace("{0}", langCode);
					if (fso.FileExists(file.replace(/\//g, "\\"))) {
						return false;
					}
				}
			});

			$(document).append('<script src="' + file + '"></script>');

		},




		// #########################################
		//
		//  ITEM METHODS
		//
		// #########################################

		_bindItemEvents: function () {
			/// <summary> Bind to the gantt chart's item events so we can react to and handle them </summary>

			var that = this;

			this.element.bind({

				// listing item expanding
				"ganttitemexpanding.mfgantt": function (event, item) {
					that._loadChildren(item);
				},

				// item(s) activated
				"ganttitemsactivated.mfgantt": function (event, data) {
					that._activateItems(data.items);

				},

				// selected item(s) changed
				"ganttselectionchanged.mfgantt": function (event, data) {
					that._selectionChanged(data.items);
				},

				// item(s) changed
				"ganttitemschanged.mfgantt": function (event, data) {
					that._itemsChanged(data.items);
				},

				// time selection activated
				"gantttimeselectionactivated.mfgantt": function (event, data) {
					that._insertNewAllocation(data.item, data.start, data.end);
				}

			});

		},

		_loadChildren: function (item, forceReload) {
			/// <summary> Attempts to load an items children objects based on it's map settings </summary>
			/// <param name="item" type="GanttItem">The items whose children to load</param>
			/// <param name="forceReload" type="boolean">
			///  Indicates whether children should be reloaded, if they have already been loaded once. Default = False;
			/// </param>

			var map, from, to, children;

			// check if the item already has children (no need to load them twice)
			if (forceReload || !item.children) {

				// get the item's map definition and search for children based on the hierarchy settings
				map = this.getMap("viewClasses", item.verData["Class"]);
				to = this._searchRelatedTo(item, map);
				from = this._searchRelatedFrom(item, map);

				// combine all children found
				children = to.concat(from);

				// add the found children to the chart with item as the parent
				if (children.length) {
					this._updateContent(children, item);
				}
			}

		},

		// dreprecated / or not in use yet
		_loadResourceAllocations: function (item, map) {
			/// <summary>

			var a = map.properties.allocation,
				search = {
					"s6": a.objectType,
					s5: false,
				},
				results;

			if (a.objectType && a.resource) {

				search[a.resource] = item.objVer.ID;

				// run search
				results = $.mf.search(search);

				if (results.Count) {
					found = $.map(results, function (verData) {
						var props = $.mf.vault.ObjectPropertyOperations.GetProperties(verData.ObjVer);
						return {
							VersionData: verData,
							Properties: props
						};
					});

					return found;
				}
			}

			return [];

		},

		_updateContent: function (current, parent) {
			/// <summary> 
			///	 Takes a list of mfiles objects and updates the parent so it contains
			///  only those items.  It only adds and removes as necessary.
			///  If no parent is passed, the top-level (listing) is assumed.
			/// </summary>
			/// <param name="ovaps" type="MFiles.ObjectVersionAndPropertiesOfMultipleObjects"> </param>
			/// <param name="parent" type="GanttItem"> The node which is to contain the items</param>

			var that = this,
				existing = this.gantt.getChildren(parent),
				toAdd = [],
				toRemove = [],
				toUpdate = [];

			// Add/Update items
			$.each(current, function (i, ovap) {

				var item, found;

				$.each(existing, function (i, existingItem) {
					if (ovap.VersionData.ObjVer.Type === existingItem.objVer.Type
						&& ovap.VersionData.ObjVer.ID === existingItem.objVer.ID) {

						found = existingItem;
						return false;
					}

				});

				if (found) {

					//existing item

					// check if exsisting item  version has changed or if it is checked out
					// (i.e the data can change without a version change)
					if (ovap.VersionData.ObjectCheckedOutToThisUser
						|| ovap.VersionData.ObjVer.Version !== found.objVer.Version) {

						// update existing item
						item = that._mapItem(ovap.VersionData, ovap.Properties, found);
						toUpdate.push(item);
					}
				} else {

					// new item
					item = that._mapItem(ovap.VersionData, ovap.Properties);
					toAdd.push(item);
				}

			});

			// Remove items
			$.each(existing, function (i, existingItem) {

				var found = false;

				$.each(current, function (i, ovap) {
					if (ovap.VersionData.ObjVer.Type === existingItem.objVer.Type
						&& ovap.VersionData.ObjVer.ID === existingItem.objVer.ID) {

						found = true;
						return false;
					}
				});

				if (!found) {
					toRemove.push(existingItem);
				}

			});

			this.gantt.suspendLayout();

			this.gantt.removeItems(toRemove);
			this.gantt.addItems(toAdd, parent);
			this.gantt.updateItems(toUpdate);

			this.gantt.resumeLayout();
		},

		_selectionChanged: function (items) {
			/// <summary> 
			///  Respond to items being selected
			///  try to notify shell and show properties in properties pane and tasks in task pane
			/// </summary>
			/// <param name="items"> items that are currently selected </param>
			

			try {

				// attempt to do a virtual selection in M-Files v. 9.0.3374 or greater

				var objOrFileVers = MFiles.CreateInstance("ObjOrFileVers");

				if (items.length) {

					// attempt to use virtual selection (not supported in all v9 builds)
					$.each(items, function (i, item) {
						var objOrFileVer = MFiles.CreateInstance("ObjOrFileVer");
						objOrFileVer.ObjVer = item.objVer;
						objOrFileVers.Add(-1, objOrFileVer);
					});

					$.mf.shellFrame.Listing.SetVirtualSelection(objOrFileVers);

				} else {

					// nothing selected... clear selection in the listing
					$.mf.shellFrame.Listing.UnselectAll();
					$.mf.shellFrame.Listing.SetVirtualSelection(objOrFileVers);
				}

			} catch (e) {

				// virtual selection did not work, revert to old, less desirable way...

				// 1 item selected
				if (items.length === 1) {

					// defer loading properties pane so we don't slow down any other rendering
					setTimeout(function () {

						try {

							// select the item natively in the listing so tasks and properties pane are shown
							// this will succeed on top level items
							$.mf.shellFrame.Listing.SelectObjectVersion(items[0].objVer);
							$.mf.shellFrame.BottomPane.ShowDefaultContent();

						} catch (e) {

							// lower level items will fall back to this
							// no tasks, manually load properties into property pane (double-click is broken this way ;-( )
							$.mf.shellFrame.BottomPane.ShowObjectVersionProperties($.mf.vault.ObjectPropertyOperations.GetPropertiesWithIconClues(items[0].objVer, false));
							$.mf.shellFrame.Listing.UnselectAll();
						}

					}, 0);

				} else {

					// 0 or many items selected

					// don't show any tasks or properties pane content
					$.mf.shellFrame.Listing.UnselectAll();
					$.mf.shellFrame.BottomPane.ShowDefaultContent();
				}

			}

		},

		_activateItems: function (items) {
			/// <summary> activate items (open meta-data card) </summary>
			/// <param name="items" type="array"> items to activate </param>			

			try {

				//normalize to 1 item
				// TODO: figure out how to activate multiple objects at the same time (can be done internally by task pane)
				var item = items[0],
					result, ov;
					
				// open document, otherwise open metadatacard
				if( item.objVer.Type === 0 && item.verData.SingleFile ){ 
						
					$.mf.vault.ObjectFileOperations.OpenFileInDefaultApplication(0, item.objVer, item.verData.Files.Item(1).FileVer, MFFileOpenMethodView);	
					 
				} else {
					
					result = $.mf.vault.ObjectOperations.ShowEditObjectWindow(0, 3, item.objVer);
				
					// handle meta-data card closure
					if (result.Result === 0 || result.Result === 2) {
						ov = $.mf.vault.ObjectOperations.GetObjectInfo(result.ObjVer, true);
						this._mapItem(ov, result.Properties, item);

						if (item.resource) {

							// if item is a resource, update it's inline children in chart
							this.gantt.updateItems(item.resource);

						} else {

							// update item in chart
							this.gantt.updateItems(item);
						}

					}
				}
			} catch (e) { }
		},

		_searchRelatedFrom: function (item, map) {
			/// <summary> Find all the children that are referenced by properties in the parent </summary>
			/// <param name="item" type="GanttItem">The parent item whose children should be searched for</param>
			/// <param name="map" type="GanttItemMap">The map definition that contains information on how the children are related</param>
			/// <returns type="array"> The children found based on the related from definitions </param>

			var that = this,
				related = [];

			// Process "From" Relationship definitions
			// map.relationships.to = array of property id's
			$.each(map.relationships.from, function (i, propID) {

				// Process Property if it exists in this object
				if (item.properties.indexOf(propID) !== -1) {

					var prop = item.properties.searchForProperty(propID),
						propDef = $.mf.vault.PropertyDefOperations.GetPropertyDef(prop.propertyDef),
						lookups, found;

					// check if the property has any values
					if (!prop.value.isNull()) {

						// Get Lookups (normalize lookup -> 1 item lookups is needed)
						if (propDef.dataType === MFDatatypeLookup) {
							lookups = MFiles.CreateInstance("Lookups");
							lookups.add(-1, prop.value.GetValueAsLookup());
						} else if (propDef.dataType === MFDatatypeMultiSelectLookup) {
							lookups = prop.value.GetValueAsLookups();
						}

						// turn each lookup into a list item
						if (lookups) {
							found = $.map(lookups, function (l) {

								var objVer = MFiles.CreateInstance("ObjVer"),
									ovap;

								objVer.setIDs(propDef.ValueList, l.item, l.version);

								// lookup latest ovap
								return $.mf.vault.ObjectOperations.GetObjectVersionAndProperties(objVer, false);

							});

						}

						related = related.concat(found);

					}

				}
			});

			return related;

		},

		_searchRelatedTo: function (item, map) {
			/// <summary> Find all the children that reference the parent item in their properties</summary>
			/// <param name="item" type="GanttItem">The parent item whose children should be searched for</param>
			/// <param name="map" type="GanttItemMap">The map definition that contains information on how the children are related</param>
			/// <returns type="array"> The children found based on the related to definitions </param>

			var that = this,
				related = [];


			// Process "To" Relationship definitions with search
			// Defintion should be {property: propID, propID-1:value, propID-2: [value1, value2], propID-3:....}
			// property entry is transformed to -  propID:item.ObjVer.id
			$.each(map.relationships.to, function (i, def) {

				var search, results, found;

				try {
					// Make a copy of the definition with a deleted(s5)=false condition
					search = $.extend(true, { s5: false }, def);

					// Transform property entry
					if (search.property) {
						search[search.property] = item.objVer.ID;
						delete search.property;
					}

					// run search
					results = $.mf.search(search);

					// turn each result into a list item
					if (results.Count) {
						found = $.map(results, function (verData) {
							var props = $.mf.vault.ObjectPropertyOperations.GetProperties(verData.ObjVer);
							return {
								VersionData: verData,
								Properties: props
							};
						});

						related = related.concat(found);
					}

				} catch (e) { }

			});

			return related;
		},

		_refreshChildHierarchyPlacement: function(item) {
			/// <summary>
			///  Updates which items are displayed as children of the item, due to the item being updated.
			///	 This can only affect children who are referenced from the item's own property
			/// </summary>
			/// <param name="item" type="MFGanttItem"> the item whose children to update

			var that = this,
				children;

			// make sure item has or can have children
			if (!item.leaf) {

				// check if there are children
				if (item.children) {

					// remove exisitng children that don't belong
					$.each(item.children, function (i, child) {

						// check if the child is not a child
						if (!that._isChild(child.verData, child.properties, item)) {

							// remove it
							that.gantt.removeItems(child);
						}
					});

				}

				// check if there are children, or if the item is expanded by had no children
				if (item.children || item.expanded) {

					// add missing children
					children = that._searchRelatedFrom(item, this.getMap("viewClasses", item.verData["Class"]) );

					if (!item.children) {

						// simply add all newly found children
						that.gantt.addItems(children, item);
					} else {

						// loop over newly found children
						$.each(children, function (i, newChild) {
							
							var id = newChild.VersionData.ObjVer.Type + "-" + newChild.VersionData.ObjVer.ID,
								found = false;

							// loop over existing children
							$.each(item.children, function (j, oldChild) {

								if (id === oldChild.id) {
									found = true;
									return false;
								}

							});

							// add the item if it wasn't found
							if (!found) {
								that.gantt.addItems(that._mapItem(newChild.VersionData, newChild.Properties), item);
							}


						});


					}
				}

			}


		},

		_refreshHierarchyPlacement: function (verData, props ) {
			/// <summary>
			///  Ensures an item exsits in the hierarchy every place it should for the object information given
			///  after it has been changed
			/// </summary>
			/// <param name="verDate" type="MFilesAPI.ObjectVersion"> The ObjectVersion of the changed object</param>
			/// <param name="props" type="MFilesAPI.PropertyValues"> The PropertyValues of the changed object</param>
			var that = this,
				id = verData.ObjVer.Type + "-" + verData.ObjVer.ID;

			$.each(this.gantt.getItems(), function (i, parent) {

				var existingChild;

				// check if an item should have any children and whether they should be showing, and that it's not the child itself
				if (!parent.isLeaf && parent.id != id && ( parent.children || parent.expanded) ) {

					// check if this is already a child
					if (parent.children) {

						// loop over children
						$.each(parent.children, function (i, child) {

							// see if this child represents the current object
							if (id === child.id) {
								existingChild = child;
								return false;
							}
						});
					}


					// check if it should be the child
					if (that._isChild(verData, props, parent)) {					

						// add it if it's not there already
						if (!existingChild) {

							// create new item
							that.gantt.addItems(that._mapItem(verData, props), parent);
						} else {

							// update existing item
							that._mapItem(verData, props, existingChild)
						}

					} else if (existingChild) {

						// remove the item, as it's no longer a child
						that.gantt.removeItems(existingChild);
					}

				}

			});
		},

		_isChild: function (verData, props, item) {
			/// <summary> Determines if the object should be displayed as a child of the item </summary>
			/// <param name="verData" type="MFilesAPI.ObjectVersion">Potential child's version data</param>
			/// <param name="props" type="MFilesAPI.PropertyValues">Potential child's properties</param>
			/// <param name="item" type="GanttItem">Potential parent item</param>
			/// <returns type="Boolean"></returns>

			var that = this,
				isChild = false,
				map = this.getMap("viewClasses", item.verData["Class"]);

			// loop over related from definitions
			$.each(map.relationships.from, function (i, propID) {

				// see if our parent item even has the property that might reference a child
				if (item.properties.indexOf(propID) != -1) {

					// determine if the property references our child
					isChild = that._hasReference(item.properties.SearchForProperty(propID), verData.ObjVer)

					// Check if we've found something
					if (isChild) {

						// break from the loop, we know it's a child!
						return false;
					}

				}
			}); // end each map.relationships.from loop


			// bail early if we've already found evidence it's a child
			if (isChild) {
				return isChild;
			}

			// loop over related to definitions
			$.each(map.relationships.to, function (i, def) {

				var prop;

				// if the child has a property that could reference a parent
				// and there is no class filter, or the child's class is in the filter
				if (props.indexOf(def.property) != -1
				  && (!def["100"].length || ($.isArray(def["100"]) && $.inArray(parseInt(verData["Class"]), def["100"]) != -1))) {

					// determine if the property references our child
					isChild = that._hasReference(props.SearchForProperty(def.property), item.objVer);

					// Check if we've found something
					if (isChild) {

						// break from the loop, we know it's a child!
						return false;
					}

				}
			}); // end each map.relationships.from loop

			return isChild;
		},

		_itemsChanged: function (items) {
			/// <summary> 
			///  Responds to the gantt.itemchanged event (which indicates an item has be moved or resized)
			///  by trying to check out the item (if not alreay) and updating the properties in m-files
			/// <summary>
			/// <param name="items" type="array">Array of GanttItems that have been changed</param>
			var that = this;

			$.each(items, function (i, item) {

				var ov = item.verData,
					props = item.properties,
					map = that.getMap("viewClasses", ov["Class"]),
					newPropVals, startDateProp, endDateProp;

				try {
					
					newPropVals = MFiles.CreateInstance("PropertyValues");

					if (!item.verData.ObjectCheckedOut) {
						ov = $.mf.vault.ObjectOperations.CheckOut(ov.objVer.ObjID);
						props = $.mf.vault.ObjectPropertyOperations.GetProperties(ov.ObjVer, false);
					}					

					startDateProp = props.searchForProperty(map.properties.startDate);
					startDateProp.Value.SetValue(MFDatatypeDate, item.start.getVarDate());
					newPropVals.Add(-1, startDateProp)
					

					if (item.end) {
						endDateProp = props.searchForProperty(map.properties.endDate);
						endDateProp.Value.SetValue(MFDatatypeDate, item.end.getVarDate());
						newPropVals.Add(-1, endDateProp)
					}

					$.mf.vault.ObjectPropertyOperations.SetProperties(ov.ObjVer, newPropVals);

				} catch(e) {
					var x = $.mf.shellFrame.ShowMessage( e.message.substr( 0, e.message.indexOf("\n") ) );
				}
				
				props = $.mf.vault.ObjectPropertyOperations.GetProperties(ov.ObjVer, false);
				that._mapItem(ov, props, item);				

				if (item.resource) {
					that.gantt.updateItems(item.resource);
				} else {
					that.gantt.updateItems(item);
				}

			});

		},

		_insertNewAllocation: function (resource, start, end) {
			/// <summary>Creates a new allocation object in M-Files for a resource</summary>
			/// <param name="resource" type="GanttItem">Resource for which to create the allocation</param>
			/// <param name="start" type="Date">Start Date of the allocation</param>
			/// <param name="end" type="Date">End Date of the allocation</param>
			/// <returns type="undefined"></returns>

			var resourceProps = this.getMap("viewClasses", resource.verData["Class"]).properties,
				createInfo = MFiles.CreateInstance("ObjectCreationInfo"),
				acl = MFiles.CreateInstance("AccessControlList"),
				props = MFiles.CreateInstance("PropertyValues"),
				resourceProp = MFiles.CreateInstance("PropertyValue"),
				startProp = MFiles.CreateInstance("PropertyValue"),
				endProp = MFiles.CreateInstance("PropertyValue"),
				startTimeProp = MFiles.CreateInstance("PropertyValue"),
				endTimeProp = MFiles.CreateInstance("PropertyValue"),
				startTime, endTime, allocationMap, result, verData, props, newItem;


			if (resourceProps.allocation.objectType !== -1 && resourceProps.allocation.resource !== -1) {

				allocationProps = this.getMap("viewObjTypes", resourceProps.allocation.objectType).properties;


				if (allocationProps.startDate && allocationProps.endDate) {
					createInfo.SetObjectType(resourceProps.allocation.objectType, false) // booking				

					/*
					// Copy Team Property From Employee Item
					if( item.properties.indexOf( 1034 ) !== -1 ) {
						props.Add( -1, item.properties.searchForProperty( 1034 ) );
					}
					*/

					// Set Resource Property
					resourceProp.PropertyDef = resourceProps.allocation.resource;
					resourceProp.Value.SetValue(MFDatatypeLookup, resource.objVer.ID);
					props.Add(-1, resourceProp);

					// Set Start Date Property
					startProp.PropertyDef = allocationProps.startDate;
					startProp.Value.SetValue(MFDatatypeDate, (new Date(start)).getVarDate());
					props.Add(-1, startProp);

					// Set End Date Property
					endProp.PropertyDef = allocationProps.endDate;
					endProp.Value.SetValue(MFDatatypeDate, (new Date(end)).addSeconds(-1).getVarDate());
					props.Add(-1, endProp);

					/*
					// Set Start Time Property
					startTimeProp.PropertyDef = 1042;
					startTimeProp.Value.SetValue( MFDatatypeTime, ( new Date( horiz.start ) ).getVarDate() );
					props.Add( -1, startTimeProp );
		
					// Set End Time Property
					endTimeProp.PropertyDef = 1043;
					endTimeProp.Value.SetValue( MFDatatypeTime, ( new Date( horiz.end ) ).getVarDate() );
					props.Add( -1, endTimeProp );
					*/

					result = $.mf.vault.ObjectOperations.ShowPrefilledNewObjectWindow(0, MFObjectWindowModeInsert, createInfo, props, acl);

					if (result.Result === MFObjectWindowResultCodeOk) {
						verData = $.mf.vault.ObjectOperations.GetObjectInfo(result.ObjVer, false, true);
						props = $.mf.vault.ObjectPropertyOperations.GetProperties(verData.ObjVer, true);
						newItem = this._mapItem(verData, props);

						this.gantt.addItems(newItem, resource);
					}

				}
			}


		},

		updateAll: function () {
			/// <summary> Updates all loaded Gantt Items on the chart </summary>
			var that = this,
				toUpdate = $.map(this.gantt.getItems(), function (item) {
					return that._mapItem(item.verData, item.properties, item);
				});

			this.gantt.updateItems(toUpdate);
		},


		updateHierarchyRecurse: function (item) {

			var that = this;

			if( item.expanded || item.type === "Resource") { //资源

				// NODE'S children should be visible

				// Update the direct children
				this._loadChildren(item, true);

				// recursively apply to the direct children
				if( item.children ) {
					$.each(item.children, function (i, child) {
						that.updateHierarchyRecurse(child, true);
					});
				}

			} else if( item.children ) {

				// NODE'S children should not be visible, but children have been loaded.

				// remove children; they will auto-load, when the node is opened.
				this.gantt.removeItems($.map( item.children, function( v ) { return v; } ));
				delete item.children;
			}

		},


		reloadAll: function (restoreTreeState) {
			/// <summary> Refreshes the chart, reloading everything </summary>
			var that = this,
				topLevelItems = [],
				toRemove = [],
				toExpand = [];

			
			var listing = $.mf.shellFrame.ActiveListing || $.mf.shellFrame.Listing;

			// UPDATE view's (top level) items
			// Only possible with builds > 11.0.4029, > 10.1.3915.11, > 10.0.3911.44
			try {
				$.mf.shellFrame.ActiveListing.RefreshListing(true, true, true);
			} catch(e) { 
				alert(e);
			}


			// find remaining top level items.
			$.each(this.gantt.getItems(), function (i, item) {
				if ( item.level == 0 ) {
					topLevelItems.push(item);
				}
			});

			// Update parent's children.
			$.each(topLevelItems, function (i, item) {
				that.updateHierarchyRecurse(item);
			});

			this.gantt.mflist._sort();

		},


		// #########################################
		//
		//  M-Files Utility Method
		//
		// #########################################

		_mfObjToStr: function(obj) {
			return obj.Type + "-" + obj.ID;
		},

		_mfStrToObjID: function(str) {
			var objID = MFiles.CreateInstance("ObjID"),
				parts = str.split("-");

			objID.SetIDs(parseInt(parts[0]), parseInt(parts[1]));
			return objID;
		},

		_hasReference: function (prop, objID) {
			/// <summary> Determines if the property contains a reference to the ObjID </summary>
			/// <param name="prop" type="MFilesAPI.PropertyValue">The property value that might contain a reference</param>
			/// <param name="objID" type="MFilesAPI.ObjID">ObjID of object that might be referenced</param>
			/// <returns type="Boolean"></returns>

			var hasRef = false,
				propDef;

			// make sure the property isn't empty
			if (!prop.Value.IsNULL()) {

				// get a handle on the property def
				propDef = $.mf.vault.PropertyDefOperations.GetPropertyDef(prop.PropertyDef);

				// make sure the property contains items of our objID's type
				if (propDef.BasedOnValueList && propDef.ValueList == objID.Type) {

					// handle single lookup property
					if (propDef.Datatype === MFDatatypeLookup && prop.Value.GetLookupID() == objID.ID) {

						hasRef = true;

						// handle multi-select lookup property
					} else if (propDef.Datatype === MFDatatypeMultiSelectLookup) {

						// loop over each lookup
						$.each(prop.Value.GetValueAsLookups(), function (i, lookup) {

							if (lookup.Item == objID.ID) {

								// objID matches a value in the multiselect lookup property
								hasRef = true;
								return false; // break from lookups loop
							}
						});
					}

				}
			}

			return hasRef;

		},

		_checkInItems: function (items, showError) {

			// loop over all items in the chart
			var objVers = MFiles.CreateInstance("ObjVers"),
				result = {
					updated: [],
					skipped: []
				},
				toUpdate = $.map(items, function (item) {

					// only handle if it's checked out to this user
					if (item.verData.ObjectCheckedOutToThisUser) {

						// add it to our objVers collection
						objVers.Add(-1, item.objVer);

						// add item to the toUpdate list
						return item;

					} else {

						result.skipped.push(item);
					}

				}),
				props;

			// check if we should change anything
			if (toUpdate.length) {

				try {
					// Check in all objects and load their properties
					objVers = $.mf.vault.ObjectOperations.CheckInMultipleObjects(objVers);
					props = $.mf.vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objVers.getAsObjVers());

					// update all item settings
					for (var i = 1; i <= objVers.Count; i++) {
						this._mapItem(objVers.Item(i), props.Item(i), toUpdate[i - 1]);
					}

					result.updated = toUpdate;

				} catch (e) {
					result.skipped = items;
					result.error = e;

					if (showError) {
						$.mf.shellFrame.ShowMessage(e);
					}
				}

			}

			return result;
		},

		_checkOutItems: function (items, showError) {

			// loop over all items in the chart
			var objVers = MFiles.CreateInstance("ObjVers"),
				result = {
					updated: [],
					skipped: []
				},
				toUpdate = $.map(items, function (item) {

					// only handle if it's checked out to this user
					if (!item.verData.ObjectCheckedOut) {

						// add it to our objVers collection
						objVers.Add(-1, item.objVer);

						// add item to the toUpdate list
						return item;

					} else {
						result.skipped.push(item);
					}

				}),
				props,
				valsById = {};

			// check if we should change anything
			if (toUpdate.length) {

				try {

					// Check in all objects and load their properties
					objVers = $.mf.vault.ObjectOperations.CheckOutMultipleObjects(objVers.GetAllDistinctObjIDs());
					props = $.mf.vault.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objVers.getAsObjVers());

					// update all item settings
					for (var i = 1; i <= objVers.Count; i++) {
						valsByID[this._mfObjToStr(objVers.Item(i).ObjVer)] = {
							ov: objVers.Item(i),
							props: props.Item(i)
						};
					}

					$.each(toUpdate, function (i, item) {
						this._mapItem(valsByID[item.id].ov, valsByID[item.id].props, item);
					});

					result.updated = toUpdate;

				} catch (e) {
					result.skipped = items;
					result.error = e;

					if (showError) {
						$.mf.shellFrame.ShowMessage(e);
					}
				}

			}

			return result;
		},

		_undoCheckOutItems: function( items, showError ) {

		},

		// #########################################
		//
		//  ITEM MAPPING METHODS
		//
		// #########################################


		// PRIVATE METHODS

		_resolveTimeUnit: function (vliPath) {
			/// <summary></summary>
			/// <param name="vliPath" type="string">
			///  A string that refers to a time unit (in English) or the path to a value list item. 
			///  Value List Item path format: [ValueList ID]-[ValueListItem ID]
			/// </param>
			/// <returns type="string"> Time Unit Name (in English)</returns>
			var parts, vl, vli, unitName;

			if ($.inArray(vliPath, this.gantt.time.BaseUnits) !== -1) {
				alert(vliPath);
				return vliPath;
			}

			try {
				parts = vliPath.split("-");
				vl = parseInt(parts[0]);
				vli = parseInt(parts[1]);

				$.each(this.options.defs.vaultTimeUnits, function (i, def) {
					if (def.valueList === vl) {
						$.each(def.units, function (unit, id) {
							if (vli === id) {
								unitName = unit.charAt(0).toUpperCase() + unit.slice(1);
								return false;
							}
						});
						return false;
					}
				});


			} catch (e) { }

			return unitName;
		},

		_refineItemTimes: function (item, map) {
			/// <summary>Extracts time information from M-Files objects, and updates the time in the Gantt Item</summary>
			/// <param name="item" type="GanttItem">Gantt Item whose date/time info to update</param>
			/// <param name="map" type="GanttItemMap">The Gantt Item map to determine which date/time properties to use</param>
			/// <returns type="undefined"></returns>

			item.cols.start = $.extend({}, item.cols.startDate);
			item.cols.end = $.extend({}, item.cols.endDate);

			if (item.startDate) {

				item.start = item.startDate.clone();

				if (item.endDate) {
					item.end = item.endDate.clone();
				}

				//Check if start time is set
				if (item.startTime) {

					//add start time if set
					item.start.add({
						hours: item.startTime.getHours(),
						minutes: item.startTime.getMinutes(),
						seconds: item.startTime.getSeconds(),
					});

					item.cols.start.display += item.cols.startTime.display

				} else if (item.type === "Milestone") { //里程碑

					item.start.addHours(12);
				}

				//Check if end time is set
				if (item.endTime) {

					//add end time if set, otherwise
					item.end.add({
						hours: item.endTime.getHours(),
						minutes: item.endTime.getMinutes(),
						seconds: item.endTime.getSeconds(),
					});

					item.cols.end.display += item.cols.endTime.display

				} else if (item.end) {

					//no end time
					//normalize endDate to last second of the day.
					item.end.addDays(1).addSeconds(-1);
				}

			}

		},

		_expandLabel: function (item) {
			/// <summary>Resolves an items calculated label by dynamically inserting property values into placeholders</summary>
			/// <param name="item" type="GanttItem">Gantt Item whose label to calculate</param>
			/// <returns type="undefined"></returns>
			

			var label = item.label || "%PROPERTY_0%",
				matches = label.match(/%\d+?%|%PROPERTY_\d+?%/g);

			if (matches) {

				// make sure we only have unique values
				matches = $.grep(matches, function (v, k) {
					return $.inArray(v, matches) === k;
				});

				$.each(matches, function (i, match) {
					var token, replace;

					token = match.replace(/%/g, "").replace(/PROPERTY_/g, "");

					if ($.isNumeric(token)) {

						replace = "";

						// Expand with property value
						token = parseInt(token);

						if (item.properties.IndexOf(token) !== -1) {
							replace = item.properties.SearchForProperty(token).Value.DisplayValue;
						}

					} else if (token.toLowerCase() === "objid") {

						// Expand with object's id
						replace = item.objVer.ID;

					} else if (token.toLowerCase() === "objver") {

						// Expand with object's version
						replace = item.objVer.Version;
					}


					if (replace !== undefined) {
						match = new RegExp(match, "gi");
						label = label.replace(match, replace);
					}

				});

			}

			item.label = label;
			item.cols.label.display = label;
			item.cols.label.value = label;
		},

		_loadIcon: function (item, map) {
			/// <summarySets column value icons as applicable</summary>
			/// <param name="item" type="GanttItem">Gantt Item whose icons should be set</param>
			/// <param name="map" type="GanttItemMap">
			///		The Gantt Item map to determine from which properties the icons are resolved
			/// </param>

			var propVal, id;

			if (map.properties.icon !== -1) {

				item.showIcon = true;

				if (map.properties.icon > 0) {

					// if this fails we show nothing
					item.icon = undefined;

					// show property based icon if we can find one
					if (item.properties.IndexOf(map.properties.icon) !== -1) {

						propVal = item.properties.SearchForProperty(map.properties.icon);

						if (!propVal.Value.IsNULL()) {

							propDef = $.mf.vault.PropertyDefOperations.GetPropertyDef(propVal.PropertyDef);

							if (propVal.Value.Datatype === MFDatatypeLookup) {
								id = propVal.Value.GetLookupID();
							} else if (propVal.Value.Datatype === MFDatatypeMultiSelectLookup) {
								id = propVal.Value.GetValueAsLookups().Item(1).Item;
							}

							item.icon = $.mf.icon(propDef.ValueList, id);

						}
					}

				}

			} else {

				item.showIcon = false;

			}
		},

		_loadMFColVals: function (item) {
			/// <summary>Turns property values into column values that can be parsed by mflist</summary>
			/// <param name="item" type="GanttItem">Gantt Item whose date/time info to update</param>
			/// <returns type="undefined"></returns>

			// resolve the columns that mflist currently displays
			var cols = this.gantt.mflist.getColumns();

			$.each(cols, function (i, p) {

				if ($.isNumeric(p)) {

					ganttProp = new GanttMFProperty(item.properties, p);

					item.cols[p] = {
						value: ganttProp.valueOf(true),
						display: ganttProp.toString()
					};
					
				}

			});

		},

		_mapItem: function (ov, props, item) {
			/// <summary>Creates or updates a GanttItem from the details on an MFiles object version</summary>
			/// <param name="ov" type="MFilesAPI.ObjectVersion">The ObjectVersion of the MFiles object version to map</param>
			/// <param name="props" type="MFilesAPI.PropertyValues">The Property Values of the MFiles object version to map</param>
			/// <param name="item" type="GanttItem">Optional. Existing Gantt Item to update</param>			
			/// <returns type="GanttItem">Returns the item passed as an argument, or else a new object</returns>

			var that = this,
				o = this.options,
				map = this.getMap("viewClasses", ov["Class"]),
				id = this._mfObjToStr(ov.objVer),
				perm;


			// create new item if needed
			if (!item) {

				item = {
					id: id,
					rendering: {},
					values: {}
				};

			}

			// update item properties
			item.verData = ov;
			item.properties = props;
			item.objVer = ov.ObjVer;
			item.icon = this.getObjIcon(ov);

			item.editable = true;
			item.overlay = false;
			if (ov.ObjectCheckedOut) {

				if (ov.ObjectCheckedOutToThisUser) {

					item.overlay = "./images/overlays/CheckedOutCurrent.png";

				} else {

					item.editable = false;
					item.overlay = "./images/overlays/CheckedOutOther.png";
				}

			}


			// figure out permissions if it's not clear from checkout status
			if (item.editable) {

				// see if there are edit rights to the object
				perm = $.mf.vault.ObjectOperations.GetObjectPermissions(item.objVer)
				item.editable = $.mf.vault.SessionInfo.CheckObjectAccess(perm.AccessControlList, MFObjectAccessEdit);

				//TODO: Check if we have edit rights for properties that will change after drag events
			}			

			item.cols = {
				title: {
					value: ov.Title,
					display: ov.Title,
					icon: this.getObjIcon(ov)
				}
			};


			$.each(map.properties, function (p, v) {
				var ganttProp;
				if (p !== "icon") {

					
					if (p === "活动") {
						p = "Activity";
					} else if (p === "里程碑") {
						p = "Milestone";
					} else if (p === "资源") {
						p = "Resource";
					} else if (p === "循环") {
						p = "Recurring";
					}
					

					if ($.isNumeric(v)) {

						ganttProp = new GanttMFProperty(props, v);						

						item[p] = ganttProp.valueOf();

						item.cols[p] = {
							value: ganttProp.valueOf(),
							display: ganttProp.toString()
						};
						

					} else {

						
						if (v === "活动") {
							v = "Activity";
						} else if (v === "里程碑") {
							v = "Milestone";
						} else if (v === "资源") {
							v = "Resource";
						} else if (v === "循环") {
							v = "Recurring";
						}
						

						item[p] = v;
						item.cols[p] = {
							value: v,
							display: v
						};
						
						if (item.cols[p].display === "Activity") {
							item.cols[p].display = "活动";
						} else if (item.cols[p].display === "Milestone") {
							item.cols[p].display = "里程碑";
						} else if (item.cols[p].display === "Resource") {
							item.cols[p].display = "资源";
						} else if (item.cols[p].display === "Recurring") {
							item.cols[p].display = "循环";
						}
					}
				}
			});
			

			// translate time units
			item.durationUnit = this._resolveTimeUnit(item.durationUnit);
			item.recurUnit = this._resolveTimeUnit(item.recurUnit);

			// Calculate missing endDate for activities if possible
			if (item.type === "Activity" && !item.endDate && item.duration && item.durationUnit) {

				item.endDate = item.startDate.clone()["add" + item.durationUnit](item.duration);
				item.editable = false;
				/*
					// try to figure out how the end date would be displayed by the client.
					var tv = MFiles.CreateInstance("TypedValue");
					tv.SetValue( MFDatatypeDate, item.endDate);
					item.endDate_str = tv.DisplayValue;
				*/

			}

			

			this._refineItemTimes(item, map);
			
			// copy each map.options directly to item
			$.each(map.options, function (p, v) {
				item[p] = v;
				if (p === '活动') {
					item['Activity'] = v;
				} else if (p === '里程碑') {
					item['Milestone'] = v;
				} else if (p === '资源') {
					item['Resource'] = v;
				} else if (p === '循环') {
					item['Recurring'] = v;
				}
			});
			
			// update Label
			this._expandLabel(item)
			
			//update label position			
			item.labelPosition = map.options.showLabel;

			// hierarchy information
			item.leaf = !(map.relationships.to.length || map.relationships.from.length) || item.type === "Resource";


			item.itemStyle = map.itemStyle;
			item.progressStyle = map.progressStyle;
			item.labelStyle = map.labelStyle;

			this._loadIcon(item, map);

			this._loadMFColVals(item);

			return item;
		},

		_initMaps: function () {
			/// <summary>Initialize the item maps, seting up the Prototypical inheritance from the loaded defs</summary>

			var that = this;
			this.maps = {
				vaultObjTypes: {},
				viewObjTypes: {},
				vaultClasses: {},
				viewClasses: {}
			};

			this.maps.vaultObjTypes.default = $.extend(true, GanttItemMap, this.options.defs.vaultObjTypes.default);
			this.maps.vaultObjTypes.default.objTypeID = "default";

			$.each(this.maps, function (scope) {

				if (that.options.defs[scope]) {

					$.each(that.options.defs[scope], function (id, def) {
						that.getMap(scope, id);
					});

				} else {

					that.options.defs[scope] = {};
				}

			});

		},

		getObjIcon: function (ov) {
			/// <summarySets the item's default icon</summary>
			/// <param name="ov" type="MFilesAPI.ObjectVersion">Mfiles Object Version whose icon should be resolved</param>
			/// <returns type="string">icon url (possibly a base64 encoded data uri)</returns>

			// try and load a class icon
			icon = $.mf.icon(MFBuiltInValueListClasses, ov["Class"]);

			// fall back to object type icon
			if (!icon) {
				icon = $.mf.icon(ov.objVer.Type);
			}

			return icon;
		},

		// PUBLIC METHODS

		// needed?
		getClassDef: function (id) {
			/// <summary>Returns the item map for a specific class</summary>
			/// <param name="id" type="Number">Class ID</param>
			/// <returns type="GanttItemMap"></returns>
			return this.getMap("viewClasses", id);
		},

		getMap: function (scope, id) {
			/// <summary>
			///  Get the current Item Map for "id" at the specified scope 
			///  Used for serializing current settings.
			/// </summary>
			/// <returns type="object">Item Map definitionss</returns>



			var that = this,
				scopeChain = $.map(this.maps, function (v, k) { return k; }),
				superMap = GanttItemMap,
				classID = (scope.indexOf("Classes") !== -1) ? id : false,
				objTypeID = (!classID) ? id : $.mf.vault.ClassOperations.GetObjectClass(classID).ObjectType,
				map, def;


			$.each(scopeChain, function (i, curScope) {
				var scopeID = (curScope.indexOf("Classes") !== -1) ? classID : objTypeID;
				map = that.maps[curScope][scopeID];

				if (!map) {
					map = that.maps[curScope][scopeID] = {};

					map.objTypeID = objTypeID;

					if (curScope.indexOf("Classes") !== -1) {
						map.classID = classID;
					}

					def = that.options.defs[curScope][scopeID];
					$.each(extendMapProps, function (i, p) {
						map[p] = Object.create(superMap[p]);
						if (def && def[p]) {
							$.extend(map[p], def[p]);
						}
					});
				}

				superMap = map;

				//break the loop if we've reached the scope.
				if (curScope === scope) {
					return false;
				}
			});

			return map;

		},

		getMapDefs: function () {
			/// <summary>
			///  Get the current Item Map definitions (not the maps themselves)
			///  Used for serializing current settings.
			/// </summary>
			/// <returns type="object">Item Map definitionss</returns>
			var mapDefs = {};

			// loop over map types
			$.each(this.maps, function (type, maps) {

				var defs = mapDefs[type] = {};

				// loop over maps
				$.each(maps, function (id, map) {

					var def = {},
						propCount = 0;

					// loop over high level map properties
					$.each(map, function (prop, obj) {

						var subDef = {},
							keyCount = 0;

						// check if this is a map property that can be extended
						if ($.inArray(prop, extendMapProps) !== -1) {

							// loop over extendable subObj properties 
							$.each(obj, function (key, val) {

								// extract value if self-defined
								if (obj.hasOwnProperty(key)) {

									subDef[key] = val;
									keyCount++
								}

							});

							// if at least one key is self defined
							// add the sub def to the map definition
							if (keyCount) {
								def[prop] = subDef;
								propCount++;
							}

						}

					});

					// if at least one prop is self defined
					// add the map definition to the mapDefs array
					if (propCount) {
						defs[id] = def;
					}

				});
			});

			return mapDefs;
		},

		getTimeUnits: function () {
			/// <summary>Get the current Time Unit mapping definitions</summary>
			/// <returns type="object">TimeUnit Mapping Defintions</returns>

			return this.options.defs.vaultTimeUnits;
		},

		getNonWorkingTime: function () {
			/// <summary>Get the current Time Unit mapping definitions</summary>
			/// <returns type="object">TimeUnit Mapping Defintions</returns>

			return this.gantt.options.nonWorkingTime;
		},


		// #########################################
		//
		//  CONF PANEL METHODS
		//
		// #########################################

		_initConfPanel: function () {
			/// <summary>
			///  Resolves what the state of the configuration panel should be when the chart first opens
			///  and makes sure it's in that state
			/// </summary>

			this._confOrigOpen = $.mf.shellFrame.RightPane.Visible;
			this._confVisible = false;
			this._updateConfPanel();

		},

		_updateConfPanel: function () {
			/// <summary>Toggles the configuration panel on and off</summary>

			if (this._confVisible) {
				$.mf.shellFrame.RightPane.ShowDashboard("conf", { elem: this.element, parent: window });
				this._confOrigOpen = $.mf.shellFrame.RightPane.Visible;
				$.mf.shellFrame.RightPane.Visible = true;
			} else {
				$.mf.shellFrame.RightPane.ShowDefaultContent();
				$.mf.shellFrame.RightPane.Visible = this._confOrigOpen;
			}

		},


		// #########################################
		//
		//  BEHAVIOR METHODS
		//
		// #########################################

		updateRefreshInterval: function() {
			/// <summary> 
			///	 Resets the refresh interval based on current settings. Enables/Disables and/or updates interval
			/// </summary>

			var self = this,
				b = this.options.behavior;

			clearInterval( this.refreshInterval );

			if( b.autoRefresh ) {
				this.refreshInterval = setInterval( function() {

					self.reloadAll(true);


				}, b.autoRefreshInterval * 1000 * 60); //minutes
			}

		},

		print: function() {
			var documentContainer = document.getElementsByTagName( 'html' )[0];
			var win = window.open( '', '_blank', 'width=' + $( window ).width() + ',height=' + $( window ).height() + ',top=50,left=50,toolbars=no,scrollbars=yes,status=no,resizable=yes,menubar=no,status=no,toolbar=no' );
			var doc = win.document;
			doc.writeln( '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">' );
			doc.writeln( '<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">' );
			doc.writeln( documentContainer.innerHTML );
			doc.writeln( '</html>' );
			doc.close();
			win.focus();

		setTimeout( function() {
			win.$(".gantt-modal-overlay").hide();
			win.$(".gantt-splitter").hide();
			win.$(".gantt-toolbar").hide();
			win.$(".mflist-head-scroller").css("top","0px");
			var leftWidth = win.$(".mflist-items").outerWidth();
			var rightWidth = win.$(".gantt-canvas").outerWidth()
			win.$(".gantt-left, .mflist, .mflist-head-scroller").width(leftWidth);
			win.$(".mflist-items").css("top","0px");
			win.$(".gantt-right")
				.css("margin-left", leftWidth + "px")
				.width(rightWidth);
			win.$(".gantt-time-scroller").width(rightWidth);
			win.$(".mflist-head-scroller").css("position", "static"); 
			win.$(".gantt-canvas-scroller, .mflist").css("overflow","hidden");
			win.$("body").css({
				"overflow": "scroll !important",
				"background-image": undefined
			});
		}, 5000 );
		}


	});

})(jQuery);