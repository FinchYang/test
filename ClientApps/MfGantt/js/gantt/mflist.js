"use strict";

(function ($, undefined) {

	
	// arguments: objectVersion, PropertyValues
	var ListItem = function(ov, props) {
		this.verData = ov;
		this.properties = props;
		this.objVer = ov.ObjVer;
		this.id = ov.objVer.Type + "-" + ov.objVer.ID;
	}
	
	$.extend(ListItem.prototype, {
		resolveDisplayValue: function(v) {
			var myVal = ""
		
			if( $.isFunction(v) ) {
				myVal = v.call(this);
			} else if( $.isNumeric(v) && this.properties.indexOf( parseInt(v) ) !== -1 ) {
				myVal = this.properties.searchForProperty(v).GetValueAsLocalizedText();
			} else if( this[v] !== undefined ){
				if( $.isPlainObject(this[v])) {
					myVal = this[v].toString();
				}
				return this[v];
			}
			
			return myVal.replace(/</g,"&lt;").replace(/\s/g, "&nbsp;");

		},
		
		resolveValue: function(v) {
		
			if( typeof v === "string" && v.indexOf("_str") !== -1 ) {
				v = v.replace("_str", "");
			}
		
			if( $.isFunction(v) ) {
				return v.call(this);
			} else if( $.isNumeric(v) && this.properties.indexOf( parseInt(v) ) !== -1 ) {
				return this.properties.searchForProperty(v).Value.Value;
			} else if( this[v] !== undefined ){
				return this[v];
			} else {
				return "";
			}
		},		
		
		getIcon: function(v) {
			var prop, propDef, failWithOT, lookup, lookups, icon;
			
			if ( $.isNumeric(v) ) {
				if( v === 0 ) {
					v = 100;
					failWithOT = true;
				}
				
				try {
					if( this.properties.indexOf(v) !== -1 ) {
						prop = this.properties.searchForProperty(v),
						propDef = $.mf.vault.PropertyDefOperations.GetPropertyDef(prop.PropertyDef);
						if( propDef.BasedOnValueList) {
							if( propDef.DataType ===  9 ) {
								lookup = prop.Value.GetValueAsLookup();
							} else {
								lookups = prop.Value.GetValueAsLookups();
								if( lookups.Count === 1) {
									lookup = lookups.Item(1);
								}
							}
							
							if(lookup) {
								//icon = Vault.ValueListItemOperations.GetValueListItemByID(propDef.ValueList, lookup.Item).Icon.toArray();
								icon = $.mf.icon(propDef.ValueList, lookup.Item);
							}
						}
					}
				} 
				catch(e) {}
				
				if( (!icon || !icon.length) && failWithOT) {
					//icon = Vault.ObjectTypeOperations.GetObjectType(this.objVer.Type).Icon.toArray();
					icon = $.mf.icon(this.objVer.Type);
				}	
			}
			
			return icon;
		}
		
	});

	/*
		Listing Widget
		==========================================
		
		ColumnDefinition: {
			id*:
			title: "Column [this.id]" 
			valueKey: (should map to something in each listing item - "." (dots) are navigated, so you can reference sub object properties)
			width: 100
			visible: true
		}
		
		ListingItem: {
			id*:
			leaf: true
			[colN.valueKey]: string / ListingItemValue
		}
		
		ListingItemValue: {
			value*: undefined 
			displayValue:
			icon:
			
		}
	
	
	*/
	$.widget("ui.mflist", {
	

		// #########################################
		//
		//  DEFAULT OPTIONS + EVENTS
		//
		// #########################################

		options: {
			headerHeight: 23,
			widthOffset: 0,
			heightOffset: 0,
			cols: [
				{
					name: "Name",
					value: 0,
					width: 150
				}
			],

			// EVENTS

			columnsHidden: $.noop(),
			columnsShown: $.noop(),
			columnsAdded: $.noop(),
			columnsRemoved: $.noop(),
			columnsReordered: $.noop(),
			columnsResized: $.noop(),


			itemsAdded: $.noop(),
			itemsRemoved: $.noop(),
			itemsUpdated: $.noop(),
            
			hoverChanged: $.noop(),
			selectionChanged: $.noop(),
			itemsActivated: $.noop(),

			itemExpanding: $.noop(),
			itemExpanded: $.noop(),
			itemCollapsed: $.noop(),

			itemsSorted: $.noop(),

			itemLoading: $.noop(), //deprecated 
			itemAdded: $.noop(), //deprecated - use itemsAdded
			itemRemoved: $.noop(), //deprecated - use itemsRemoved
			itemUpdated: $.noop(), //deprecated - use itemsUpdated
			itemsChanged: $.noop(), //deprecated - use itemsUpdated
			
			beforeLoadChildren: $.noop(), //deprecated - use beforeExpandItem

			// Styling
			classItemSelected: "mflist-item-select",
			classItemHovered: "mflist-item-hilite"

		},
		

	

		// #########################################
		//
		//  STANDARD WIDGET METHODS
		//
		// #########################################        

		_create: function() {
			var that = this;

			this.cols = [];
			this.headWidth = 0;
			this.items = {};
			this._selectedItems = [];

			this.element
				.addClass("mflist-container")
				.html(
					'<div class="mflist">' + 
						'<div class="mflist-head-scroller">' +
							'<div class="mflist-header"></div>' +
						'</div>' +
						'<div class="mflist-bg"></div>' +
						'<ul class="mflist-items"></ul>' +
					'</div>'
				);

			//Bind Size Events
			this.resizeTo = (this.element.get(0).tagName.toLowerCase() == "body") ? $(window) : this.element;
			this.resizeTo.resize($.proxy(that, "resize"));					
				
				
			var clicks = 0;
			// Bind Item Hover Event
			this.element.on({
				mouseenter: function(event) {
					that.setHoveredItem( that.getItem(this) );
					return false;
				},
				mouseleave: function(event) {
					that.setHoveredItem();
					return false;
				},
				click: function(event) {
					var me = this;
					clicks++;
					if (clicks === 1) {							
						setTimeout( function(){					
							clicks = 0;
						}, 250);
						that.focusItem( that.getItem(me), event);
						//that.setSelectedItems( that.getItem(me) );
					} else if ( clicks === 2 ) {
						that.activateItems( that.getItem(me) );
						clicks = 0;
					}
					event.stopPropagation();
				}						
			}, 
				".mflist-item"		
			).on({
				mouseenter: function(event) {
					$(".mflist-treepart").removeClass("mflist-tree-hilite");
					$(this).addClass("mflist-tree-hilite");
				},
				mouseleave: function(event) {
					$(".mflist-treepart").removeClass("mflist-tree-hilite");
				},
				click: function(event) {
					if( $(this).hasClass("mflist-tree-open") ) {
						that.collapseItem(this);
					} else if( $(this).hasClass("mflist-tree-closed") ) {
						that.expandItem(this);
					}
					event.stopPropagation();
				}				
			}, 
				".mflist-treepart"		
			).click(function(event){
				that.setSelectedItems();
			});
			
			//bind header scroll
			$(".mflist").scroll(function () {
				$(".mflist-head-scroller").scrollLeft($(this).scrollLeft());
			});
				
			this._generateHeader();						
			this.resize();
			
			this.updateSortOrder();
			
			//Bind listing events if a shellFrame object was passed
			//otherwise implementers have to manage items manually
			if(this.options.shellFrame && false) {
				this._bindToShellFrame(this.options.shellFrame);				
			}
			
			this._bindNavigation();
		},
		
		_init: function () {

		},
		

		// #########################################
		//
		//  MFILES METHODS THAT SHOULD BE MOVED OUT
		//
		// #########################################     

		//deprecated
		_bindToShellFrame: function (shellFrame) {
			console.log("DEPRECATED! mflist._bindToShellFrame");
			var ovaps = shellFrame.Listing.Items.ObjectVersionsAndProperties,
				that = this;
				
			$.each(ovaps, function(i,v) {
				that._insertItem(new ListItem(v.VersionData, v.Properties));
			});
			
			this.shellFrameHandler = shellFrame.Listing.Events.Register(Event_ContentChanged, function( items ) {
				
				var ovaps = items.ObjectVersionsAndProperties,
					foundIDs = [];
			
				// Add/Update items
				$.each(ovaps, function(i,v) {
					var newItem = new ListItem(v.VersionData, v.Properties),
						oldItem = that.items[newItem.id];
							
					foundIDs.push(newItem.id);
					
					if(!oldItem) {
						that._insertItem(newItem);
					} else if( newItem.objVer.Version !== oldItem.objVer.Version ) {
						that._refreshItem(newItem);
					}
						
				});			

				// Remove items
				$.each(that.items, function(i,item) {					
					if( $.inArray(item.id, foundIDs) === -1) {
						that._delItem(item.path);
					}
				});

			});

			
		},
		



		// #########################################
		//
		//  GENERAL LAYOUT / BEHAVIOR
		//
		// #########################################  


		// PRIVATE METHODS

		_generateHeader: function () {
			var that = this,
				head = this.element.find(".mflist-header").empty(),
				bg = this.element.find(".mflist-bg");
			this.headWidth = 0;
			$('<div class="mflist-col-header mflist-col-last"><div class="mflist-col-header-label" >&nbsp;</div></div>').appendTo(head);

			$.each(this.options.cols, function (i, col) {
				that.addColumn(col, true);
			});

			/*
			
			head.find(".mflist-col-header")				
				.mousemove(function(event) {
					var s = $(this),
						left = $(this).offset().left,
						right = $(this).outerWidth() + left,
						resizePrev = ( event.pageX >= left && event.pageX <= left + 5 && !$(this).is(":first-child") ),
						resizeMe = ( event.pageX <= right && event.pageX >= right - 5 && !$(this).is(":last-child") ),
						cursor = (resizeMe || resizePrev) ? "w-resize" : "default";										
					
					that.element.find(".mflist-items").text(resizeMe + ", " + resizePrev);
					
					//things have changed... switch drag handler settings
					if( cursor != $(this).css("cursor") || true )
						$(this).css("cursor", cursor);
						
						
						if(resizeMe || resizePrev) {
						
							// Add resize drag handler
							$(this).draggable({
								axis: "x",
								scroll: false,
								appendTo: that.element.find(".mflist-overlay"),
								cursorAt: {left:0},
								helper: function() {
									return $('<div class="mflist-col-sizer"></div>').outerHeight( that.element.find(".mflist-bg").outerHeight() + head.outerHeight());
								},
								stop: function(event, ui) {
									that.element.find(".mflist-items").text("hello!");
								},		
								
							});
							
						} else {

							// Add reorder column drag handler
							$(this).draggable({
								axis: "x",
								scroll: false,
								helper: "clone",
							});						
						}

						$(this).draggable("destroy");
						}
				})
				.filter(":not(:last-child)")
				.hover(
					function() {
						$(this).addClass("mflist-col-header-hilite");
					},
					function() {
						$(this).removeClass("mflist-col-header-hilite");
					}			
				);
				*/

		},

		_positionColBackground: function (col) {

		},
		
		_sort: function(listNode, recurse) {
			var that = this,
				o = this.options,
				sort = o.sortBy.concat([1]),
				children;
				
			if(!listNode) {
				children = this.element.find(".mflist-items>li");
				recurse = true;
			} else {
				children = listNode.children("li");
			}
					
			children.sortElements(function(a, b){

				var result = 0,
					itemA = that.getItem($(a).attr("id")),
					itemB = that.getItem($(b).attr("id")),
					reverse = 1;

				for (var i = 0; i < sort.length && result === 0; i++) {
					
					var colID = Math.abs(sort[i]) - 1,
						col = o.cols[colID];

					var valA = itemA.cols[col.key].value,
						valB = itemB.cols[col.key].value;

					reverse = (sort[i] < 0) ? -1 : 1;
					
					if ( (valA && valB && valA.valueOf() == valB.valueOf() ) || valA === valB ) {
						continue; //result = 0; //(itemA.id > itemB.id) ? 1 : -1;
					} else if ( valA && !valB || valA > valB ) {
						result = 1 * reverse;
					} else {
						result = -1 * reverse;
					}
					
				}

				return result;
			});

			
			if(recurse) {
				$.each(children, function(i,child) {
					var node = $(child).children("ul:first");
					if( node.length ) {
						that._sort(node, true);
					}
				});
			}
		},
		
		refresh: function() {
			
			var that = this;

			this.element.find(".mflist-items").children().hide();
				
			$.each(this.items, function(i, item) {
				that._clearChildren(item);
				that._refreshItem(item);
			});
			
			this.element.find(".mflist-items").children().show();

		},

		_bindNavigation: function () {
			var that = this;
			this.ctrlDown = false;
			this.shiftDown = false;
			$(document).keydown(function (e) {
				if( e.which === 17 ) { that.ctrlDown = true; }
				if( e.which === 16 ) { that.shiftDown = true; }
				if (e.which === 38) { that._focusPrev(e); return false; } //up key
				if (e.which === 40) { that._focusNext(e); return false; } //down key
				if (e.which === 13) { that.activateItems(); return false; } //enter key
			}).keyup( function( e ) {
				if( e.which === 17 ) { that.ctrlDown = false; }
				if( e.which === 16 ) { that.shiftDown = false; }
			} )
		},


		//PUBLIC METHODS

		resize: function () {

			//Update outer container, and widget dimensions
			var o = this.options,
				h = ((this.resizeTo) ? this.resizeTo.height() : o.height) + o.heightOffset,
				w = ((this.resizeTo) ? this.resizeTo.width() : o.width) + o.widthOffset,
				mf = this.element.find(".mflist").outerHeight(h).outerWidth(w),
				p = mf.position();

			// Calculate Width of End Cap & whether the horizontal scroll bar will be displayed
			var iw = mf.outerWidth(),
				capWidth = Math.max(30, iw - this.headWidth),
				vertScrollBarOffset = (this.headWidth + capWidth > w || mf.css("overflow") === "scroll") ? 17 : 0;
			this.element.find(".mflist-col-last").outerWidth(capWidth);


			var items = this.element.find(".mflist-items"),
				ih = items.outerHeight(),
				vObjs = this.element.find(".mflist-bg-col"),
				horizScrollbarOffset = (ih + o.headerHeight > h || mf.css("overflow") === "scroll") ? 17 : 0;
			vObjs.outerHeight(Math.max(ih, h - o.headerHeight - vertScrollBarOffset)).add(items).css("top", o.headerHeight + "px");
			items.outerWidth(this.headWidth + capWidth - horizScrollbarOffset);



			this.element.find(".mflist-head-scroller").css({ top: p.top + "px", left: p.left + "px" }).outerWidth(w - horizScrollbarOffset).height(o.headerHeight);
			this.element.find(".mflist-col-header-label").outerHeight(o.headerHeight - 1).css("padding-top", ((o.headerHeight - 23) / 2 + 3) + "px");

			//this.element.find(".mflist-items").text(this.headWidth + ", " + scrollbarOffset + ", " + (new Date));

		},

		updateSortOrder: function(sortBy) {
			var that = this;
			
			if( sortBy ) {
				if( $.isArray(sortBy) ) {
					this.options.sortBy = sortBy;
				} else {
					this.options.sortBy = [sortBy];
				}
			}
			
			sortBy = this.options.sortBy;
			
			// remove all sort classes from column elements
			that.element.find(".mflist-col-header").removeClass("mflist-sorted mflist-desc");
			that.element.find(".mflist-bg-col").removeClass("mflist-sorted");

			$.each(sortBy, function(i, sortCol) {
				var colID = Math.abs(sortCol)-1,
					col = that.element.find(".mflist-col-" + colID),
					bg = that.element.find(".mflist-bg-" + colID);
					
				col.addClass("mflist-sorted");
				
				if( sortBy.length === 1 ) {
					bg.addClass("mflist-sorted");
				}
				
				if( sortCol < 0 ) {
					col.addClass("mflist-desc");
				}
				
			});
		
			this._sort();
			
			that._trigger('itemsSorted', {type:"itemsSorted"});
			
		},




		// #########################################
		//
		//  COLUMN METHODS
		//
		// #########################################  
	
		addColumn: function(col, batch) {
			
			var that = this;
		
			col.id = this.cols.length;
			this.cols.push(col);
			var e = $(
						'<div class="mflist-col-header mflist-col-'+col.id+'">' +
							'<div class="mflist-col-header-label" >'+col.name+'</div>' +
						'</div>'
					).insertBefore(this.element.find(".mflist-col-last"))
					.outerWidth(col.width);				
			$('<div class="mflist-bg-col mflist-bg-'+col.id+'"></div>')
				.appendTo(this.element.find(".mflist-bg"))
				.outerWidth(col.width)
				.css("left", e.position().left+"px");
			this.headWidth += col.width;	

			if(!batch) this.resize();
			
			function getResizeTarget(s, x) {
				var left = s.offset().left,
					right = s.outerWidth() + left;
				if ( x >= left && x <= left + 5 && !s.is(":first-child") ) return s.prev();
				if ( x <= right && x >= right - 5 && !s.is(":last-child") ) return s;
				return false;	
			}
			
			/*
			e.draggable({
				
			})			
			.mousemove(function(event) {
				$(this).css("cursor", getResizeTarget($(this),event.pageX) ? "w-resize" : "default");
			})
			*/
			e.hover(
				function() {
					$(this).addClass("mflist-col-header-hilite");
				},
				function() {
					$(this).removeClass("mflist-col-header-hilite");
				}			
			).click(function(evt){
				var curSort = that.options.sortBy.concat(),
					sortCol = col.id+1,
					pos;
				
				// handle ctrl key click add/modify current sory
				if (evt.ctrlKey) {

					// see if clicked column was already a sort column
					pos = $.inArray(sortCol, curSort);

					// if we didn't find it, look for the negative equivilent
					if( pos === -1 ) {
						pos = $.inArray(sortCol*-1, curSort);
					}
					
					if (pos === -1) {

						// add column to sort list
						curSort.push(sortCol);

					} else {

						// toggle direction of already sorted column
						curSort[pos] *= -1;

					}

				} else {

					// no ctrl key -> change sort with one sort column only

					if( sortCol === curSort[0] ) {
						sortCol *= -1;
					}

					curSort = [sortCol]
				}
				
				// update sort order
				that.updateSortOrder(curSort);
				
			});
			
			this._trigger("columnsAdded",{},{columns:[col]});

			if( !batch ) {

				// loop over each item and update the row/column contents
				$.each(this.items, function(i, item) {
							
					// refresh row
					that._populateRow(item);

				});
			}

		},

		getColumns: function() {

			return $.map(this.cols, function(c) {
				return c.key;
			});

		},

		getVisibleColumns: function() {

			return $.map(this.cols, function(c) {
				if( c.visible !== false ) {
					return c.key;
				}
			});

		},
		



		// #########################################
		//
		//  ITEM UTILITY METHODS
		//
		// #########################################  	

		getItem: function (path) {

			if (path) {
				if (typeof (path) !== "string") {
					if (path.nodeType) {
						path = $(path).closest("li").attr("id");
					} else if (path.jquery) {
						path = path.closest("li").attr("id");
					} else if (path.verData) {
						return path;
					}
				}
			}

			var that = this,
				item;

			if (path) {
				$.each(path.split("_"), function (i, v) {
					if (!item) {
						item = that.items[v];
					} else {
						item = item.children[v];
					}
				});
			}

			return item;
		},
        
		getItemNode: function (item, innerNode) {
			if( innerNode ) {
				return this.element.find("#" + item.path + ">.mflist-item:first");				
			} else {
				return this.element.find("#" + item.path);
			}
		},

		hasChildren: function ( parent ) {

			return !!parent.children;
		},

		getChildren: function( parent ) {

			if( parent ) {
				return $.extend( {}, parent.children );
			} else {
				return $.extend( {}, this.items );
			}

		},

		getChildrenNode: function (item) {
			return this.getItemNode(item).children("ul");
		},

		getItemsByID: function (id) {
			var toSearch = [this.items],
				found = [],
				items;

			while (toSearch.length) {
				items = toSearch.shift();

				$.each(items, function (i, item) {
					if (item.id === id) {
						found.push(item);
					}

					if (item.children) {
						toSearch.push(item.children);
					}
				});
			}

			return found;
		},




		// #########################################
		//
		//  ITEM MANIPULATION METHODS
		//
		// #########################################  	


		// PRIVATE METHODS

		_populateRow: function (item) {
			var that = this,

				// get row and empty it out
				row = this.getItemNode(item, true).empty();

			// append columns to the row
			$.each(this.options.cols, function (i, col) {
				var c = $('<div class="mflist-col mflist-col-' + i + '"></div>').appendTo(row).outerWidth(col.width).text(item.cols[col.key].display),
					align = col.align || "left",
					icon = item.cols[col.key].icon;

				c.css("text-align", align);

				if (i !== 0 && icon && icon.length) {
					//c.prepend('<img class="mflist-icon" src="data:image/x-icon;base64,' + Base64.encode(icon) + '" width="16" height="16"/>');
					c.prepend('<img class="mflist-icon" src="' + icon + '" width="16" height="16"/>');
				}

				// do heirarchy stuff, if its the main column
				if (i === 0) {

					if( icon && icon.length ) {
						var imgContainer = $('<div class="mflist-icon-container"><img class="mflist-icon" src="' + icon + '" width="16" height="16"/></div>').prependTo(c);

						if( item.overlay ) {
							imgContainer.append('<img class="mflist-icon-overlay" src="' + item.overlay + '" width="16" height="16"/>');
						}
					}

					//hierarchy control
					if (!item.leaf) {						
						c.prepend('<span class="mflist-treepart mflist-tree-' + ((item.expanded) ? "open" : "closed") + '">&nbsp;</span>');
					} else {
						c.prepend('<span class="mflist-treepart">&nbsp;</span>');
					}

					// indenting
					for (var i = 0; i < item.level; i++) {
						c.prepend('<span class="mflist-treepart">&nbsp;</span>');
					}
				}
			});
		},

		//deprecated
		_insertItem: function (item, parent) {
			console.log("Deprecated: List._insertItem");
			var that = this,
				listItem,
				container;

			if (parent) {
				item.path = parent.path + "_" + item.id;
				item.parent = parent.path;
				item.level = parent.level + 1;
				parent.children = parent.children || {}
				parent.children[item.id] = item;
				container = this.getChildrenNode(parent);
			} else {
				item.path = item.id;
				item.level = 0;
				this.items[item.id] = item;
				container = this.element.find(".mflist-items");
			}

			this._trigger('itemLoading', { type: "itemLoading" }, item);

			// create row element
			listItem = $('<li id="' + item.path + '"><div class="mflist-item"></div></li>').appendTo(container).data({ id: item.id, parent: item.parent });

			if (!item.leaf) {
				$('<ul><ul>').appendTo(listItem).hide();
			}

			this._populateRow(item);

			this.resize();

			this._trigger('itemAdded', { type: "itemAdded" }, item);
		},

		//deprecated???
		_refreshItem: function (newItem) {
			//console.log("DEPRECATED! mflist._refreshItem");
			var that = this,
				oldItems = this.getItemsByID(newItem.id),
				parents = [];

			$.each(oldItems, function (i, item) {


				item.objVer = newItem.objVer;
				item.verData = newItem.verData;
				item.properties = newItem.properties;

				//that._trigger('itemLoading', { type: "itemLoading" }, item);
				that._populateRow(item);


				//that._trigger('itemUpdated', { type: "itemUpdated" }, item);
			});

		},

		//deprecated
		_delItem: function (itemPath) {
			console.log("DEPRECATED! mflist._delItem");
			var item = this.getItem(itemPath),
				parent = item.parent;

			//remove children
			this._clearChildren(item);

			//remove element
			$("#" + item.path).remove();

			//remove reference to the item
			if (parent) {
				delete parent.children[item.id];
			} else {
				delete this.items[item.id];
			}

			this._trigger('itemRemoved', { type: "itemRemoved" }, item);

		},

		//deprecated
		loadChildren: function (item) {
			console.log("DEPRECATED! mflist.loadChildren");
			// allow whatever callbacks to make changes
			// primarily to the leaf & children properties
			this._trigger('beforeLoadChildren', { type: "beforeLoadChildren" }, item);

			if (!item.leaf && item.hierarchy) {
				var that = this;

				// Process "To" Relationship definitions with search
				// Defintion should be {property: propID, propID-1:value, propID-2: [value1, value2], propID-3:....}
				// property entry is transformed to -  propID:item.ObjVer.id
				$.each(item.hierarchy.to, function (i, def) {

					try {
						// Make a copy of the definition with a deleted(s5)=false condition
						def = $.extend({ s5: false }, def);

						// Transform property entry
						if (def.property) {
							def[def.property] = item.objVer.ID;
							delete def.property;
						}

						// run search
						//results = mfSearch(def);
						results = $.mf.search(def);

						// turn each result into a list item
						if (results.Count) {
							$.each(results, function (i, verData) {
								var props = $.mf.vault.ObjectPropertyOperations.GetProperties(verData.ObjVer);
								that._insertItem(new ListItem(verData, props), item);
							});
						}
					} catch (e) { }

				});

				// Process "From" Relationship definitions
				// item.hierarchy.from = array of property id's
				$.each(item.hierarchy.from, function (i, propID) {

					// Process Property if it exists in this object
					if (item.properties.indexOf(propID) !== -1) {

						var prop = item.properties.searchForProperty(propID),
							propDef = $.mf.vault.PropertyDefOperations.GetPropertyDef(prop.propertyDef),
							lookups;

						if (!prop.value.isNull()) {

							// Get Lookups
							if (propDef.dataType === MFDatatypeLookup) {
								lookups = MFiles.CreateInstance("Lookups");
								lookups.add(-1, prop.value.GetValueAsLookup());
							} else if (propDef.dataType === MFDatatypeMultiSelectLookup) {
								lookups = prop.value.GetValueAsLookups();
							}

							// turn each lookup into a list item
							if (lookups) {
								$.each(lookups, function (i, l) {

									var objVer = MFiles.CreateInstance("ObjVer"),
										ovap;

									objVer.setIDs(propDef.ValueList, l.item, l.version);
									ovap = $.mf.vault.ObjectOperations.GetObjectVersionAndProperties(objVer, false);
									that._insertItem(new ListItem(ovap.versionData, ovap.properties), item);
								});
							}
						}

					}
				});

				this._sort(this.getChildrenNode(item));
			}
		},

		//deprecated
		_clearChildren: function (item) {
			console.log("DEPRECATED! mflist._clearChildren");
			var that = this;
			if (item.children) {
				$.each(item.children, function (i, child) {
					that._delItem(child.path);
				});
			}
			delete item.children;
		},

		//deprecated
		_refreshChildren: function (item) {
			console.log("DEPRECATED! mflist._refreshChildren");
			this._clearChildren(item);
			this.loadChildren(item);
		},


		// PUBLIC METHODS

		addItems: function (items , parent) {
			/// <signature>
			///		<summary> Insert items into the listing at the top level </summary>
			///		<param name="items" type="Array" > Listing Items to add</array>
			/// </signature>
			/// <signature>
			///		<summary> Insert items into the listing under the specified parent </summary>
			///		<param name="items" type="Array" > Listing Items to add</array>
			///		<param name="items" type="ListingItem" > The item under which to add the new items</array>
			/// </signature>

			// declare variables with default (top level parent) values
			var that = this,
				container = this.element.find(".mflist-items"),
				children = this.items,
				basePath = "",
				level = 0;

			// adjust values for specific parent passed
			if( parent ) {
				container = this.getChildrenNode(parent);
				children = parent.children = parent.children || {};
				basePath = parent.path + "_";
				level = parent.level + 1;
			}

			// normalize arg to array
			items = $.normalizeToArray(items);


			// loop over each item
			$.each(items, function(i, item) {

				var listItem;

				// set some item properties
				item.path = basePath + item.id;
				item.parent = parent;
				item.level = level;
				children[item.id] = item;


				// create row element
				listItem = $('<li id="' + item.path + '"><div class="mflist-item"></div></li>').appendTo(container);

				// add child container if not a leaf
				if (!item.leaf) {
					$('<ul><ul>').appendTo(listItem).hide();
					item.expanded = false;
				}

				// populate the row
				that._populateRow(item);

			});


			// update the layout
			this.resize();

			//this._sort();

			// trigger event notification
			//this._trigger('itemsAdded', { type: "itemsAdded" }, items);

	
		},
		
		updateItems: function( items ) {
			/// <summary> public wrapper for _refreshItem </summary>

			var that = this;

			// normalize arg to array
			items = $.normalizeToArray(items);

			// loop over each item and update the row/column contents
			$.each(items, function(i, item) {
							
				// refresh row
				that._populateRow(item);

			});
		
			this._trigger('itemsUpdated', { type: "itemsUpdated" }, items);
		
		},

		removeItems: function( items ) {
			/// <summary> Removes the passed items from the listing </summary>
			/// <param name="items" type="Array"> </params>

			var that = this;

			// normalize arg to array
			items = $.normalizeToArray(items);

			// loop over each item
			$.each(items, function(i, item) {								

				//remove children
				if( that.hasChildren(item) ) {
					that.removeItems( that.getChildren(item) );
				}

				//remove element node
				that.getItemNode( item ).remove();

				//remove reference to the item
				if (item.parent) {
					delete item.parent.children[item.id];
				} else {
					delete that.items[item.id];
				}
			});

			// trigger event notification
			this._trigger('itemsRemoved', { type: "itemsRemoved" }, items);
		},
		
		//deprecated
		refreshColValues: function(items) {
			console.log("DEPRECATED! mflist.refreshColValues");
			var that = this;
			
			//assume top-level items if none passed (this will recurse down to all
			items = items || this.items;
						
			$.each(items, function(i,item) {
				var row = that.getItemNode(item);
				$.each(that.options.cols, function(i,col) {
					if( i !== 0) {						
						var c = row.find('.mflist-col-'+i).text( item.cols[col.key].display ),
							icon = item.cols[col.key].icon;

						if(icon) {
							c.prepend('<img class="mflist-icon" src="data:image/x-icon;base64,' + Base64.encode(icon) + '" width="16" height="16"/>');
						}
					}
				});
				
				//update children with recursion
				if( item.children ) {
					that.refreshColValues(item.children);
				}
			});
		},
		
		



		// #########################################
		//
		//  ITEM NAVIGATION METHODS
		//
		// ######################################### 

		// Public Methods

		getHoveredItem: function() {
			return this._hoveredItem;
		},

		setHoveredItem: function(item) {
			/// <signature>
			///		<summary> Set the items that is currently hovered </summary>
			///		<param name="item" type="ListItem"> Item to be hovered </param>
			/// </siganature>
			/// <signature>
			///		<summary> Unhover all items </summary>
			/// </siganature>

			var that = this;

			// nothing has changed, return
			if( item === this._hoveredItem) {
				return;
			}

			// if something is currently hovered
			if( this._hoveredItem ) {

				// undo hover & appearance
				this._hoveredItem.hovered = false;				
			}

			this.element.find("." + this.options.classItemHovered).removeClass(this.options.classItemHovered);

			// check if we have something new to hover
			if( item ) {

				// set hover & appearance
				item.hovered = true;
				this.getItemNode( item, true ).addClass( this.options.classItemHovered );
			}

			// update hovered
			this._hoveredItem = item;

			// trigger hover change
			setTimeout( function() {
				that._trigger('hoverChanged', {type:"hoverChanged"}, {"item":this._hoveredItem});
			}, 0);

		},

		focusItem: function(item, e) {

			this.focusedItem = item;

			if( e.shiftKey ) {

				this._selectRange(item);

			} else if (e.ctrlKey) {

				this._toggleSelected(item);

			} else {

				this.setSelectedItems(item);
			}			

		},

		_selectRange: function(rangeEnd, rangeStart) {
			/// <summary>
			var that = this,
				c = this.options.classItemSelected,
				items = this.element.find(".mflist-item"),
				inRange = false,
				selected = [],
				startNode,
				endNode;
			
			if( !rangeStart ){
				if ( this.selectedRangeStart ) {
					rangeStart = this.selectedRangeStart;
				} else {
					rangeStart = items.eq(0);
				}
			}

			this.selectedRangeStart = rangeStart;
			this.selectedRangeEnd = rangeEnd;

			startNode = this.getItemNode(rangeStart, true)[0];
			endNode = this.getItemNode(rangeEnd, true)[0];

			items.each( function() {				

				var item = that.getItem(this),
					endRange = false;

				// update whether we're selecting or deselecting items
				if (this == startNode || this == endNode) {

					if (inRange || startNode === endNode) {
						endRange = true;
					}
					
					inRange = true;

				}

				// add selected item 
				if ( inRange ) {
					selected.push(item);
				} 

				// update whether we're in range
				if ( endRange ) {
					inRange = false;
				}

			});

			return this.setSelectedItems(selected, "range");

		},

		_toggleSelected: function( item ) {

			var selected = this._selectedItems.concat(),
				index = $.inArray(item, selected);

			this.selectedRangeStart = item;

			// determine item's selection state
			if (index === -1) {

				// item not selected, add...
				selected.push(item);

			} else {

				//item selected, remove
				selected.splice(index, 1);
			}

			return this.setSelectedItems(selected, "toggle");
		},


		getSelectedItems: function() {
			/// <summary> Get all the items that are currently selected in the listing </summary>
			/// <returns type="Array" elementType="ListItem"> </returns>

			//return copy of the selected items array
			return this._selectedItems.concat();

		},
		
		setSelectedItems: function(items, mode) {
			/// <summary> Get all the items that are currently selected in the listing </summary>
			/// <param name="limit" type="Number"> Limit the number of items that are returned </param>
			/// <returns type="Array" elementType="ListItem"> </returns>

			var that = this,
				result = {
					selected: [],
					added: [],
					removed: [],
					mode: mode || "default"
				};


			items = $.normalizeToArray(items);

			if( items.length === 1 ) {
				this.selectedRangeStart = items[0];
			}

			// loop over every node to update selected state
			$.each(this.element.find(".mflist-item"), function (i, node) {

				var item = that.getItem(node),
					elem = $(node),
					selected = ( $.inArray(item, items) !== -1 &&  $(elem).is(":visible") );


				if( !!item.selected != selected ) {
					if (selected) {
						result.added.push(item);
					} else {
						result.removed.push(item);
					}
				}

				if (selected) {
					result.selected.push(item);
				}

				item.selected = selected;
				$(node).toggleClass(that.options.classItemSelected, selected);

			});

			this._selectedItems = result.selected.concat();


			// trigger change in selection
			this._trigger('selectionChanged', { type: "selectionChanged" }, result);

			return result;
		},
		
		_focusNext: function(e) {
			
			var that = this,
				items = this.element.find(".mflist-item"),
				selectNext = false;

			// see if no item is currently in focus
			if (!that.focusedItem) {

				// focus the first item and be done
				this.focusItem(this.getItem(items.filter(":visible:first")), e);
				return;
			}

			// loop through a list of all items - ordered as they're displayed
			$.each(items, function (i, node) {

				var item = that.getItem(node);

				// see if we've matched the currently focused item
				if (item === that.focusedItem) {

					// indicate we want to focus the next item
					selectNext = true;

				} else if (selectNext && $(node).is(":visible")) {

					// focus the current item, and break the loop
					that.focusItem(item, e);
					return false;
				}

			});
			
		},
		
		_focusPrev: function(e) {
			var that = this,
				items = this.element.find(".mflist-item"),
				prev = false;

			// see if no item is currently in focus
			if (!that.focusedItem) {

				// focus the first item and be done
				this.focusItem(this.getItem(items.filter(":visible:first")), e);
				return;
			}

			// loop through a list of all items - ordered as they're displayed
			$.each(items, function (i, node) {

				var item = that.getItem(node);

				// see if we've matched the currently focused item
				if (item === that.focusedItem && prev) {

					// focus the previous item, and break the loop
					that.focusItem(prev, e);
					return false;
				}

				// keep track of the previous item
				if ($(node).is(":visible")) {
					prev = item;
				}

			});
		},
		
		activateItems: function(items) {

			// normalize arg to array
			items = $.normalizeToArray(items);

			if( items && items.length ) {

				this._trigger('itemsActivated', {type:"itemsActivated"}, {items:items});

				// TODO: Remove once update implemented in mfgantt
				var that = this;
				$.each(items, function(i, item) {
					that._refreshItem(item);
				});

			}

		},
		
		expandItem: function(item) {
			item = this.getItem(item);
			var node = this.getItemNode(item),
				childrenNode = node.children("ul"),
				iconNode = node.find(".mflist-tree-closed:first").first().addClass("mflist-tree-open").removeClass("mflist-tree-closed"),
				hadChildren = !!item.children;
			
			this._trigger('itemExpanding', {type:"itemExpanding"}, item);

			item.expanded = true;

			childrenNode.show();

			this.resize();

			
			if (!hadChildren && item.children) {
				this._sort(childrenNode);
			}

			this._trigger('itemExpanded', {type:"itemExpanded"}, item);
		},
		
		collapseItem: function(item) {
			item = this.getItem(item);
			var node = this.getItemNode(item);			
			node.find(".mflist-tree-open:first").addClass("mflist-tree-closed").removeClass("mflist-tree-open");
			node.children("ul").hide();
			
			item.expanded = false;
			
			this.resize();
			this.setSelectedItems(this._selectedItems);


			this._trigger('itemCollapsed', {type:"itemCollapsed"}, item);
		},
		


	});

})(jQuery);
