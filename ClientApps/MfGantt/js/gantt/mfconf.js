"use strict";

(function ($, undefined) {
	
	var runningItemID = 1,
		idPrefix = "mfconf-item-";

	function getNewItemID() {
		return idPrefix + runningItemID++;
	}
	
	
    $.widget("ui.mfconf", {
		options: { 
			panels: ["settings"],
			defaultPanel: "settings",
			items: [],
			editors: {
				base: {
					type: "base",
					canEdit: function(item) {
						return false;
					},
					isGroup: function(item) {
						return !!this.members  || !!this.group;
					},
					getName: function(item) {
						return item.name || this.name || item.key || item.value.toString();
					},
					initInput: function(item, elem) {
						return $('<input id="'+item.id+'" class="mfconf-input" type="text" />').appendTo(elem);
					},
					create: function(item, elem) {
						var input = this.initInput(item, elem);
						
						if( item.options ) {
							if( this.isInheritable(item) ) {
								elem.addClass("mfconf-inheritable");
								//input.prop("disabled", item.inherited);
								var cb = $('<input id="'+item.id+'-inherit" class="mfconf-inherit" type="checkbox" title="Override inherited value" />').prependTo(elem).prop("checked", !item.inherited);							
								cb.change(function() {
									item.inherited = !item.inherited;
									
									if( item.inherited ) {
									
										// removes the local property, so it will default to prototype
										delete item.parent.value[item.key];
										
									} else {
									
										// copies prototype value to local object
										var wrapped = { value:item.parent.value[item.key] },
											result = Parent.$.extend(true, Parent.newObject(), wrapped);
										item.parent.value[item.key] = result.value;										
									}														
									
									//force a change to propogate even if the perceived value hasn't changed
									item.editor.change(item, item.parent.value[item.key], true); 
									item.editor.update(item);
									
									if( typeof item.parent.value[item.key] === "object" ) {
										item.confPanel._refreshChildren(item);
									}											
									
								});
		
							}
						}

						if( this.behavior ) {
							elem.addClass("mfconf-behavior");
							this.initBehavior(item);
						}
						
						if( item.childBehavior ) {
							elem.addClass("mfconf-behavior");
							item.parent.editor.initChildBehavior(item.parent, item);
						}
						
						this.update(item);											
					},
					update: function(item) {
						$("#"+item.id).val(item.value).prop("disabled", this.isDisabled(item));		
					},
					change: function(item, newValue, force) {
						
						if( item.value !== newValue || force) {
							 
							try {							
								if( !item.inherited ) {
									item.parent.value[item.key] = newValue;
								}
								
								//update item
								item.oldValue = item.value;
								item.value = newValue;
								
								/*
								if( item.subItems ) {
									item.confPanel._refreshChildren(item);
								}
								*/
								
								//trigger callbacks
								this.bubble(item, item);
							} catch(e){}

						}
					},					
					bubble: function(item, origItem) {						
						var bubble;
						if( $.isFunction(item.onChange) && bubble !== false ) {
							bubble = item.onChange(item, origItem);
						} 
						
						if( $.isFunction(item.editor.onChange) && bubble !== false) {
							bubble = item.editor.onChange(item, origItem);
						}
						
						if( item.parent && item.editor.bubble && bubble !== false ) {
							item.editor.bubble(item.parent, origItem);
						}						
					},
					isInherited: function(item) {
						if(item.inherited) {
							return true;
						} else if( item.parent ) {
							return this.isInherited(item.parent);
						}
						return false;
					},
					isInheritanceRestricted: function(item) {
						if(item.restrictInheritance) {
							return true;
						} else if( item.parent ) {
							return this.isInheritanceRestricted(item.parent);
						}
						return false;
					},
					isInheritable: function(item) {
						return ( item.options && item.options.inheritable && !this.isInheritanceRestricted(item) );						
					},
					isDisabled: function(item) {
						var disabled = !!( this.isInherited(item) || this.readOnly || this.disabled || item.disabled || item.readOnly );
						$("#"+item.id).closest(".mfconf-value").toggleClass("mfconf-disabled", disabled);
						return disabled;
					},
					initBehavior: function(item) {
					
					},					
					initChildBehavior: function(item, child) {
					
					}
				},
				string: {
					type: "string",
					canEdit: function(item) {
						return typeof item.value === "string"
					},
					initInput: function(item, elem) {
						var that = this;
						return $('<input id="'+item.id+'" class="mfconf-input" type="text" />').appendTo(elem)
									.blur( function() {
										that.change(item, $(this).val());
									});
					},
				},
				number: {
					type: "number",
					canEdit: function(item) {
						return $.isNumeric(item.value);
					},
					initInput: function(item, elem) {
						var that = this,
							opts = $.extend(item.options, {
								spin: function (event, ui) {
									if( $("#"+item.id).prop("disabled") ) {
										return false;
									}
									that.change(item, ui.value);									
								},						
								change: function (event, ui) {
									var valStr = $("#"+item.id).val(),
										val = (valStr.indexOf(".") !== -1) ? parseFloat(valStr) : parseInt(valStr);
									if(item.options && item.options.max && val > item.options.max) {
										val = item.options.max;
									} else if(item.options && item.options.min && val < item.options.min) {
										val = item.options.min;										
									}
								
									that.change(item, val);
									that.update(item);
									
								}
							});
						return $('<input id="'+item.id+'" class="mfconf-input" type="text" />').appendTo(elem).spinner(opts);
						//this.update(item);
					}
				},
				bool: {
					extend: "options",
					type: "bool",
					canEdit: function(item) {
						return typeof item.value === "boolean";
					},
					getOptions: function(item) {
						return ["是", "否"]; //True  False
					},
					update: function(item) {
						this.normalize(item);
						$("#"+item.id).val( this.serialize(item.value) ).prop("disabled", this.isDisabled(item));
					},
					onChange: function(item, origItem) {
						this.normalize(item);
						if(!item.inherited) {
							item.parent.value[item.key] = item.value;		
						}
					},
					normalize: function(item) {
						if (typeof item.value === "string") {
							item.value = this.deserialize(item.value);
						}				
					},
					serialize: function(v) {
						return (v) ? "是" : "否"; //True   False
					},
					deserialize: function(v) {
						return (v === "True" || v === "是");
					},					
				},
				array: {
					extend: "object",
					type: "array",
					group: true,
					behavior: true,
					canEdit: function(item) {
						return $.isArray(item.value);
					},
					
					getSubItems: function(item) {
						item.subItems = [];
						item.subItems = $.map(item.value, function(v, i) {		
							var subItem = { 
								name: ( item.childName || "" ) + " (" + (i+1) + ")",
								"value":v,
								"key":i,
								parent:item,
								/*id:getNewItemID(),*/
								childBehavior:true,
								options: item.options && item.options.childOptions || {}
							};
							if( item.options && item.options.childType) {
								subItem.type = item.options.childType;
							}
							return subItem;
						});

						return item.subItems;
					},
					initBehavior: function(item) {
						var icon = $('<span class="ui-icon ui-icon-plus">Add</span>');
						$("#"+item.id).parent().append(icon);
						icon.click(function() {																			
							
							if( item.options.protoChild ) {
								var wrapped = { value:item.options.protoChild },
									result = Parent.$.extend(true, Parent.newObject(), wrapped);											
								item.value.push(result.value);
							} else {
								item.value.push(null);
							}
							
							item.confPanel._refreshChildren(item);
							
							item.editor.change(item, item.value, true);
						});
					},
					initChildBehavior: function(item, child) {
						var icon = $('<span class="ui-icon ui-icon-minus">Add</span>');
						$("#"+child.id).parent().append(icon);
						icon.click(function() {						
							item.value.splice(child.key, 1);
							item.confPanel._refreshChildren(item);
							item.editor.change(item, item.value, true);
						});
					}
				},				
				object: {
					type: "object",
					group: true,
					
					canEdit: function(item) {
						return typeof item.value === "object"
					},
					
					initInput: function(item, elem) {
						return $('<input id="'+item.id+'" class="mfconf-input" type="text" disabled="disabled" />').appendTo(elem);
					},
					
					getSubItems: function(item) {
						item.subItems = [];
						if( this.members ) {
							item.subItems = $.map(this.members, function(v,k) {
								return $.extend({key:k, value:item.value[k], parent:item, /*id:getNewItemID(),*/ inherited:!item.value.hasOwnProperty(k)}, v);
							});
						} else {
							item.subItems = $.map(item.value, function(v, k) {		
								var member = this.members && this.members[k];
								return {"value":v, "key":k, parent:item, /*id:getNewItemID(),*/ inherited:!item.value.hasOwnProperty(k)};								
							});
						}

						return item.subItems;
					},
					
					update: function(item) {
						this.isDisabled(item);
						if( $.isArray(item.subItems) ) {
							$.each(item.subItems, function(i, subItem) {
								subItem.value = item.value[subItem.key];
								subItem.editor.update(subItem);
							});
						}
					}
				},
				date: {
					type: "date",
					canEdit: function(item) {
						return Object.prototype.toString.call(item.value) === '[object Date]';
					},					
					initInput: function(item, elem) {
						var that = this,
							input = $('<input id="'+item.id+'" class="mfconf-input" type="text" />').appendTo(elem).datepicker({
								onSelect: function (dateText, inst) {
									that.change(item, $(this).datepicker("getDate"));
								}
							});
						if(item.options && item.options.readOnly === true) {
							input.attr("disabled","disabled");
						}
						
						return input;
						//this.update(item);
					},
					update: function(item) {
						$("#"+item.id).datepicker("setDate", item.value).prop("disabled", this.isDisabled(item));
					}
				},
				options: {
					type: "options",				
					sort: true,
					initInput: function(item, elem) {
						var that = this,
							input = $('<select id="'+item.id+'" class="mfconf-input" ></select>').appendTo(elem),
							options = this.getOptions(item),
							useKeysAsValues = !$.isArray(options),
							sort = (item.options && item.options.sort !== undefined) ? item.options.sort : this.sort;
							
						$.each(options, function(k,v) {
							if( useKeysAsValues ) {
								input.append('<option value="' + k + '">' + v + '</option>');
							} else {
								input.append('<option>' + v + '</option>');
							}
						});

						if( sort ) {
							input.children().sortElements(function(a, b){
								var textA = $(a).text(),
									textB = $(b).text(),
									result;
								
								if ( textA === textB ) {
									result = 0;
								} else if ( textA > textB ) {
									result = 1;
								} else {
									result = -1;
								}
									
								return result;
							});
						}
						
						if( this.multiple || (item.options && item.options.multiple) ) {
							elem.css("overflow", "visible");
							input.multiSelect({
								selectAll: false,
								noneSelected: item.options.noneSelected || "",
								oneOrMoreSelected: '*'
							}, function(lastChecked){
								var checked = lastChecked.closest(".multiSelectOptions").find("label.checked"),
									valArr = $.map( checked, function(label,i) {
										var val = $(label).find("input").val() || $(label).text();
									
										if($.isNumeric(val)) {
											val = parseInt(val);
										}

										return val;
									});
			
								if( !item.loading ) {
									that.change(item, valArr || [] );
								}
							});
							
							elem.find(".multiSelect, .multiSelectOptions").addClass("mfconf-input");
						} else {
							input.change(function(){
								var val = $("#"+item.id).val();
								
								if($.isNumeric(val)) {
									val = parseInt(val);
								}								
								
								that.change(item, val);
							});						
						}											
						return input;
						
					},				
					getOptions: function(item) {
						return item.options.options;
					},
					update: function(item) {
						var node = $("#"+item.id);
						
						item.loading = true;
						if( this.multiple || (item.options && item.options.multiple) ) {
							node.parent().find("label").each(function(i, elem) {
								var optInput = $(this).find("input"),
									val = optInput.val() || $(this).text(),
									sel;
								
								if($.isNumeric(val)) {
									val = parseInt(val);
								}
								
								sel = ($.inArray(val, item.value) !== -1);

								// simulate click to toggle value
								if( optInput.prop("checked") !== sel) {
									optInput.click();
									optInput.toggleClass("checked");
								}

							});
						} else {
							node.val(item.value);
						}

						node.prop("disabled", this.isDisabled(item));
						item.loading = false;

					}
				},
				color: {
					type: "color",
					initInput: function(item, elem) {
						var that = this,
							input = $('<input id="'+item.id+'" class="mfconf-input" type="text" />').appendTo(elem);
						input.miniColors({
							open: function(hex, rgb) {
								$(".miniColors-selector").css({right:"2px", left:""});
							},
							change: function(hex, rgb) {
								that.change(item, hex);
							},							
							close: function(hex, rgb) {
								that.change(item, hex);
							}
						});
						
						//this.update(item);
						
						return input;
					},
					update: function(item) {
						$("#"+item.id).miniColors('value',item.value).prop("disabled", this.isDisabled(item));
					}
				},
				linearGradient: {
					extend: "object",
					type: "linearGradient",
					autoLoadChildren: true,
					members: {
						angle: {name:"角度", type:"number", options: {step:5, min:0, max:359} }, // Angle
						color1: {name:"颜色1", type:"color"},	 //Color #1
						color2: {name:"颜色2", type:"color"}		 //Color #2								
					},				
					initInput: function(item, elem) {
						var input = $('<input id="'+item.id+'" class="mfconf-input" type="text"/>').appendTo(elem);							
						item.value = this.deserialize(item.value);
						return input;
						//this.update(item);
					},
					update: function(item) {
						this.normalize(item);
						$("#"+item.id).val(this.serialize(item.value)).prop("disabled", true/*this.isDisabled(item)*/);			
					},
					onChange: function(item, origItem) {
						this.normalize(item);
						
						if( !item.inherited ) { 	
						
							// set parent value if not inherited
							item.parent.value[item.key] = this.serialize(item.value);							
						}
						
						// update visible input value
						this.update(item);
					},
					normalize: function(item) {
						if( typeof item.value === "string") {
							// value was just re-inherited or copied from prototype
							
							// revert value to object and update subvalues							
							item.value = this.deserialize(item.value);
							
							$.each(item.subItems, function(i, subItem) {
								subItem.value = item.value[subItem.key];
								subItem.editor.update(subItem);
							});			
						}					
					},
					serialize: function(obj) {
						return ([obj.angle, obj.color1, obj.color2]).join("-");
					},
					deserialize: function(str) {
						var parts = str.split("-");
						return 	{
							angle: parts[0],
							color1: parts[1],
							color2: parts[2]
						}				
					}
				},
				dayOfWeek: {
					extend: "options",
					type: "dayOfWeek",				
					getOptions: function(item) {
						var props = {};
					
						$.each( Date.CultureInfo.dayNames, function(i,name) {
							props[i] = name;
						});

						return props;
					},
					sort: false
				}



			},
		},
		
		_create: function() {
			var that = this,
				o = this.options;

			this.element
				.addClass("mfconf")
				.html( '<ul class="mfconf-tabs"></ul>' )
				.on("click", ".mfconf-open, .mfconf-closed", function(){
					if( $(this).hasClass("mfconf-open") ) {
						that.closeGroup(this)
					} else {
						that.openGroup(this)
					}
				});
				

			this._initEditors();			
			this.items = {};
			this.allItems = {};
			
			this._initPanels();

			
			$(window).resize(function() {
				if( that.element.find(".mfconf-label").length > 0 ) {
					that.resize();
				}
			});
			
			this.element.find(".mfconf-splitter").draggable({
				axis:"x",				
				stop: function() { that.resize(true); },
				zIndex: 2
			});
		},
		
		
		_init: function() {
			
		},
		
		resize: function(keepProportions) {
			var panel = this.element.find(".mfconf-panel:visible"),
				label = panel.find(".mfconf-label:visible:first"),
				splitter = panel.find(".mfconf-splitter");

			if(label.length) {
				panel.find(".mfconf-value").outerWidth(1);
			
				var labelOffset = label.offset(),			
					totalWidth = panel.width() - labelOffset.left,				
					nameColWidth = Math.floor( splitter.offset().left - labelOffset.left ),
					valColWidth = Math.floor( totalWidth-nameColWidth+4 ), //hack
					lastItem = panel.find(".mfconf-label:visible:last");
					
				panel.find(".mfconf-label").outerWidth(nameColWidth);				
				panel.find(".mfconf-value").outerWidth(valColWidth);
				
				panel.find(".mfconf-input").each(function() {
					$(this).outerWidth($(this).closest(".mfconf-value").width());
				});
				
				splitter.css("top", labelOffset.top+"px").height( (lastItem.offset().top + lastItem.outerHeight()) - labelOffset.top + 2);
			}
			
		},

		
		_initEditors: function() {
			var that = this,
				deps = {};
			this.editors = {};
			
			$.each(this.options.editors, function(i, def){
				if( def.type === "base") {
					that.editors["base"] = def;
				} else {
					var extend = def.extend || "base",
						editor;
					if( that.editors[extend] ) {
						editor = that.addEditor(def);
						if( $.isArray(deps[def.type]) ) {
							$.each(deps[def.type],function(j, subDef) {
								that.addEditor(subDef);
							});
						}
						delete deps[def.type];
					} else {
						if(!$.isArray(deps[extend])) {
							deps[extend] = [];
						}
						deps[extend].push(def);
					}
				}				
			});

			//alert( $.map(this.editors, function(v,k){return k;}) );
			
		},
		
		addEditor: function(def) {
			var extend = def.extend || "base";
			if( this.editors[extend] ) {
				this.editors[def.type] = Object.create(this.editors[extend]);
				return $.extend(this.editors[def.type], def);
			} else {
				throw "未找到编辑器: " + extend; //Editor not found
			}
		},		
		
		_initPanels: function() {
			var that = this,
				o = this.options;
				
			// create widget panel reference
			this.panels = {};
				
			// process user passed panel defintions
			$.each(o.panels, function(i, panel) {
				if( typeof panel === "object" && !panel.id) {
					panel.id = i;
				}
				that.addPanel(panel);
			});

			// Make sure default panel is set
			if( !o.defaultPanel || !this.panels[o.defaultPanel] ) {
				$.each(this.panels, function(i, panel) {
					o.defaultPanel = panel.id;
					return false;
				});				
			}
			
			// Create tabs, and bind resize event
			this.element.tabs({
				activate: function(event, ui) {
					that.resize();
				}
			});			
			
		},
		
		_createPanel: function(panel) {
			this.panels[panel.id] = panel;
			
			var name = panel.name;
			if (name === 'View') {
				name = '视图';
			} else if (name === 'Vault') {
				name = "库";
			}

			this.element.find(".mfconf-tabs").append(
				'<li><a href="#' + panel.elemID + '">' + name + '</a></li>'
			);
			
			return $(
				'<div id="' + panel.elemID + '" class="mfconf-panel">' +
					'<div class="mfconf-splitter"> </div>' +
					'<ul class="mfconf-groups"></ul>' +
				'</div>'
			).appendTo(this.element);
		},	

		addPanel: function(panel) {
		
			// Normalize panel definition
			if( typeof panel === "string" ) {
				panel = { id: panel };
			}
			
			if( !panel.name ) {
				panel.name = panel.id.charAt(0).toUpperCase() + panel.id.slice(1); ;
			}
			
			panel.elemID = "mfconf-panel-" + panel.id;
			
			this._createPanel(panel);
			
		},
		
		_getPanel: function(panelID) {
			if(panelID && this.panels[panelID]) {
				return this.panels[panelID];
			} else {
				return this.panels[this.options.defaultPanel];
			}
		},
		
		_getPanelGroupNode: function(panelID) {
			var panel = this._getPanel(panelID);
			return this.element.find("#"+panel.elemID+">.mfconf-groups");
		},
		
		addItem: function(item, panelID) {
			var panel = this._getPanelGroupNode(panelID);
			item.id = getNewItemID();
			item.level = 0;
			this.items[item.id] = item;
			this._insertItem(item, panel);
			this._sort(panel);
			this.resize();
		},
		
		//removes any specific item
		removeItem: function(item) {
			var that = this;
			
			//remove children first
			if( item.subItems ) {
				$.each(item.subItems, function(i, subItem) {
					that.removeItem(subItem);
				});
			}
			
			// remove item nodes
			$("#"+item.id).closest("li").remove();
			
			// remove item entry
			delete this.items[item.id];

		},
		
		//removes top level items of a certain type
		removeItems: function(types) {
			var that = this;
			
			// normalize types for when a single value is passed
			if(!$.isArray(types)) {
				types = [types];
			}
			
			// loop through each type
			$.each(types, function(i, type) {
			
				//loop through each top-level item
				$.each(that.items, function(id,item) {
				
					//remove item if the type matches
					if( item.type === type ) {
						that.removeItem(item);
					}
					
				});
				
			});
		},
		
		_insertItem: function(item, parent) {
			var that = this;

			this.allItems[item.id] = item;
			
			this._resolveEditor(item);
			item.confPanel = this;
			
			if( item.editor.isGroup(item) ) {
				var group;
				if( item.level === 0) {
					this._createGroupNode(item, parent);
					this._loadChildren(item);
				} else {
					this._createSubGroupNode(item, parent);
					
					if( item.editor.autoLoadChildren || (item.options && item.options.autoLoadChildren) ) {
						this._loadChildren(item);
					}
				}
			} else {
				this._createNameValueNode(item, parent);				
			}
		},

		
		_loadChildren: function(item) {
			var that = this,
				childrenNode = $("#"+item.id).closest("li").find("ul:first");
				
			if(item.editor.getSubItems) {	
				$.each( item.editor.getSubItems(item), function(name, sub) {
					sub.id = sub.id || getNewItemID();
					sub.level = item.level + 1;
					sub.options = sub.options || {};
					that._insertItem(sub, childrenNode);
				});	
			}
			
			this._sort(childrenNode);
			this.resize();
		},
		
		_refreshChildren: function(item) {
			var that = this;
			
			//remove children first
			if( item.subItems ) {
				$.each(item.subItems, function(i, subItem) {
					that.removeItem(subItem);
				});
			}

			delete item.subItems ;
			
			this._loadChildren(item);
			
		},
		
		_resolveEditor: function(item) {
			var that = this,
				editor = this.editors[item.type];
			
			if(!editor) {
				$.each(this.editors, function(name,e) {
					if( e.canEdit(item) ) {
						editor = e;
						return false; //break
					}
				});
			}

			if(!editor) {
				throw "未找到编辑器: " + item.type; //Could not find editor
			}
			
			item.editor = editor;
		},
		
		_createGroupNode: function(item, elem) {
			return $('<li id="' + item.id + '" class="mfconf-group">' +						
						'<div class="mfconf-group-label">' +
							'<div class="mfconf-spacer mfconf-open"> </div>' +
							item.editor.getName(item) + '</div>' +
						'<ul class="mfconf-properties"></ul>' +
					'</li>').appendTo(elem);
		},
		
		_createSubGroupNode: function(item, elem) {			
			var node = $('<li class="mfconf-group">' +
						'<div class="mfconf-label">' +
							'<div class="mfconf-spacer mfconf-open"> </div>' +
							'<span>' + item.editor.getName(item) /* + ' (' + item.inherited + ')' */ + '</span></div>' +
						'<div class="mfconf-value">&nbsp;</div>' +
						'<ul class="mfconf-properties"></ul>' +
					'</li>').appendTo(elem),
				label = node.find(".mfconf-label");
					
			for(var i=1; i<item.level; i++)	{
				label.prepend('<div class="mfconf-spacer"> </div>');
			}
					
			this.closeGroup(node);		
				
			item.editor.create(item, node.find(".mfconf-value"));
			
			return node;					
		},		
		
		_createNameValueNode: function(item, elem) {			
			var node = $('<li>' +
						'<div class="mfconf-label"><span>' + item.editor.getName(item) /*+ ' (' + item.inherited + ')'*/ + '</span></div>' +
						'<div class="mfconf-value"></div>' +
					'</li>').appendTo(elem),
				label = node.find(".mfconf-label");
					
			for(var i=0; i<item.level; i++)	{
				label.prepend('<div class="mfconf-spacer"> </div>');
			}					
					
			item.editor.create(item, node.find(".mfconf-value"));
			
			return node;
		},
		
		closeGroup: function(node) {
			var group = $(node).closest(".mfconf-group"),
				toggler = group.find(".mfconf-open:first"),
				children = group.find("ul.mfconf-properties:first");

			if( toggler.length ) {
				toggler.removeClass("mfconf-open").addClass("mfconf-closed");
				children.hide();
			}
			this.resize();
		},
		
		openGroup: function(node) {
			var group = $(node).closest(".mfconf-group"),
				toggler = group.find(".mfconf-closed:first"),
				children = group.find("ul.mfconf-properties:first");
			

			if( children.children().length === 0 ) {
				var itemID = group.find(".mfconf-input").attr("id"),
					item = this.allItems[itemID];
					
				if( item ) {					
					this._loadChildren(item);
				}
			}

				
			if( toggler.length ) {
				toggler.removeClass("mfconf-closed").addClass("mfconf-open");
				children.show();
			}
			this.resize();			
		},
		
		refreshAll: function() {
			$.each(this.items,function(i, item) {
				item.editor.update(item);
			});
		},
		
		_sort: function(parent) {
			var that = this;
			
			parent.children().sortElements(function(a, b){
				var itemA = that.allItems[$(a).attr("id")] || that.allItems[$(a).find(".mfconf-input:first").attr("id")],
					itemB = that.allItems[$(b).attr("id")] || that.allItems[$(b).find(".mfconf-input:first").attr("id")],
					result;
					

				if ( itemA.sortPriority === itemB.sortPriority ) {
					result = 0;
				} else if ( itemA.sortPriority < itemB.sortPriority ) {
					result = 1;
				} else {
					result = -1;
				}
						
				return result;
			});	

		},


		
    });

})(jQuery);