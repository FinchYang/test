( function( $, undefined ) {

	// TimeUnits can have the following parameters
	//
	// increment: int or function (required) 
	//		represents or returns the minimum time step in ticks (milliseconds), before the system should check 
	//		if a new unit should be drawn. Equals the size of the unit, or with divisor, approx. size/divisor
	//
	// divisor:	int (optional) 
	//		this is a simple hack to have a uniform ticksPerPixel value across non-congruent units of time,
	//		like month and year.  Also helps aligning non-matching units of time like week/month/year.
	//
	// value: callback (optional)
	//		this value must be the same for any point in time contained within the unit
	//		keep them as snappy as possible! They're called alot!
	// 			- no value : auto-calculated as... date - date % increment
	//			- function(date) { return value }

	// The time object holds some general definitions for process date/time information
	// as well as a reference to the defined TimeUnits and TimeIntervals
	var Time = {

		// This is a helper, to determine the method of the date object to use to get the value of the given time unit
		BaseUnitGetters: {
			year: "getFullYear",
			month: "getMonth",
			day: "getDate",
			hour: "getHours",
			minute: "getMinutes",
			second: "getSeconds",
			millisecond: "getMilliseconds"
		},

		BaseUnits: [
            "Milliseconds",
            "Seconds",
            "Minutes",
            "Hours",
            "Days",
            "Weeks",
            "Months",
            "Years"
		],

		// This object holds all the defined Time Units at runtime (they only extend the base TimeUnit object
		Unit: {},

		// This object holds all the defined TimeInterals at runtime (TimeUnits are also TimeIntervals)
		Interval: {},


		roundTo: function( d, unit, count ) {
			/// <summary> Cuts off the irrelevant details of a date </summary>
			/// <param name="d" type="Date"> The date to be rounded </param>
			/// <param name="unit" type="string"> 
			///  Name of the unit of time to round to i.e. "Year", "Month", "Day", ...
			///  The possible values are the keys of the Time.BaseUnitGetters object
			/// </param>
			/// <param name="count"> 
			///  The count of the unit to round to, i.e., if unit=Hours, and count=4 the
			///  date will be rounded to the nearest 4 hour interval.
			/// </param>
			/// <returns type="Date"> The rounded date value </returns>

			// used in the loop to determine when to start zero-ing out units of the date 
			var zero = false,

				// loop through each unit of the date from lease specific to specific
				// and return an array with the values we want to keep, and the other
				// units zero-ed out
				a = $.map( this.BaseUnitGetters, function( method, name ) {
					var val = ( zero ) ? 0 : d[method]();
					if( !zero && name === unit ) {
						zero = true;
						val = ( count && val !== 0 ) ? Math.floor( val - val % count ) : val;
					}
					return val;
				} );

			// return a new date built with the units we just resolved
			// no such thing as day 0 - must be at least 1;
			return new Date( a[0], a[1], Math.max( 1, a[2] ), a[3], a[4], a[5], a[6] );
		}

	};

	// TIME UNIT CONSTRUCTOR
	var TimeUnit = function( def ) {
		/// <summary> The TimeUnit constructor </summary>
		/// <param name="def" type="Object"> The defintion of the time unit </def>
		/// <returns type="TimeUnit"> A new TimeUnit object </returns>

		// copy the defintion to the newly created object
		// which has inherited the TimeUnit prototype
		$.extend( this, def );

		// Register as a TimeUnit & TimeInterval in the Time object
		Time.Unit[this.unit] = this;
		Time.Interval[this.name] = this;
	};

	// Define the TimeUnit Prototype
	// ( these will also be the base of TimeIntervals, which inherit from TimeUnits )
	//  All properties should be accessed with the resolve method as it is callback safe.
	$.extend( TimeUnit.prototype, {

		// Set some TimeUnit and TimeInterval defaults

		// the minimum width in pixels this TimeUnit should be rendered at in the TimeScale 
		minWidth: 20,

		// the maximum width in pixels this TimeUnit should be rendered at in the TimeScale 
		maxWidth: 80,

		// What approximate fraction of the Unit/Interval's value is represented by the value of increment
		divisor: 1,

		// The number of units to be grouped into one block (only TimeIntervals should define a value other than 1)
		count: 1,

		resolve: function( propertyName, callBackArgs ) {
			/// <summary> 
			///  This method is used as a getter for defined properties. It allows callbacks to be defined in place values,
			///  and will resovle the values in those cases.  It also allows extra paramters to be passed to the callback.
			///  Properties should be accessed via this property.
			/// </summary>
			/// <param name="propertyName" type="string"> The name of the property </param>
			/// <param name="callBackArgs"> 
			///  A value or array of values to be passed as parameters to a property's value resolving callback.
			/// </param>
			/// <returns> The resolved property value </returns>

			// Check if a callback (function) is specified for the property
			if( $.isFunction( this[propertyName] ) ) {

				// there's a callback

				// normalize the arguments passed
				var args = Array.prototype.slice.call( arguments ); //( $.isArray( callBackArgs ) ) ? callBackArgs : [callBackArgs];
				args.shift();

				// call the callback with the arguments and return the returned value
				return this[propertyName].apply( this, args );
			}

			// there wasn't a callback, just return the value of the property
			return this[propertyName];
		},

		value: function( d ) {
			/// <summary>
			///  Returns the Unit or Interval value of the date passed 
			/// </summary>
			/// <param name="d" type="Date"> The date from which to derive the value </param>
			/// <returns type="Date">A copy of the date rounded to this specific TimeUnit/Interval </returns>

			// round the date using this TimeUnit/Interval's parameters
			return Time.roundTo( d, ( this.unit || this.name ).toLowerCase(), this.count );
		},

		format: function( d, context ) {
			/// <summary> 
			///  Produces a string describing the value of the date's Unit/Interval given a certain environment.
			/// </summary>
			/// <param name="d" type="date"> The date from which to determine the value to be formatted</param>
			/// <param name="context" type="object">
			///  An object describing the environment the string will be displayed.
			/// </param>

			// declare our variables
			var unit = this.unit || this.name,
				value;

			// Get the TimeUnit/Interval value of the date (rounded)
			d = new Date( this.value( d ) );

			// determine if a specific mask is defined for the object
			if( this.mask ) {

				if( $.isPlainObject( this.mask ) ) {

					// find the optimal mask based on context.width
					$.each( this.mask, function( i, mask ) {
						if( i < context.width ) {
							value = d.toString( mask );
						} else {
							return false;
						}
					} );

				} else {

					// format the mask to the date
					value = d.toString( this.mask );

				}

			} else {

				try {

					// Try to return the raw unit value of the date, by creating an accessor method from the unit name
					value = d["get" + unit + "s"]();

				} catch( e ) {

					// Raw value failed, just return a full mask of the date
					value = d.toString( "yyyy-MM-dd HH:mm:ss" );
				}
			}

			// if this is the top tier, make sure there is enough context information
			if( context.top ) {
				value = this.addTimeContext( d, unit, value );
			}

			return value;

		},

		addTimeContext: function( d, unit, value ) {

			switch( $.inArray( unit + "s", Time.BaseUnits ) ) {
				// FALL THROUGH ALL....

				case 0: // Milliseconds
				case 1: // Seconds
				case 2: // Minutes
				case 3: // Hours... 

					// add Date
					value += d.toString( " ddd, dd" );

				case 4: // Date
				case 5: // Weeks

					// add Month
					value += d.toString( " MMM" );

				case 6: // Month

					// add year
					value += d.toString( ", yyyy" );

			}

			return value;
		}

	} );

	// TIME INTERVAL CONSTRUCTOR
	var TimeInterval = function( def ) {
		/// <summary> The TimeInvterval constructor </summary>
		/// <param name="def" type="Object"> The defintion of the time interval </def>
		/// <returns type="TimeInterval"> A new TimeInterval object extending the defined TimeUnit</returns>

		// Create a new object that extends the TimeUnit object specified in the def
		var me = Object.create( Time.Unit[def.unit] );

		// Copy the passed definition to the new object
		$.extend( me, def );

		// Register the object as a TimeInterval with the Time object
		Time.Interval[me.name] = me;

		// Return the new TimeInterval
		return me;
	};

	/*
	//expects function, then arg1, arg2, etc.... for resolve
	TimeInterval.test = function(f, d) {
		var output = {};
		$.each(Time.Interval, function(name,interval) {
			output[name] = interval.resolve(f, d.clone());
		});
		return output;
	}
	*/



	// DEFAULT TIME UNIT DEFINTIONS

	// loop over each (default) TimeUnit def and instantiate them as TimeUnits
	$.each(
		[
			// MILLISECOND
			{
				name: "毫秒", //Millisecond
				unit: "Millisecond",
				increment: 1,
				maxWidth: 20
			},

			// SECOND
			{
				name: "秒", //Second
				unit: "Second",
				increment: 1000,
				mask: "ss",
				maxWidth: 20
			},

			// MINUTE
			{
				name: "分钟", //Minute
				unit: "Minute",
				increment: 1000 * 60,
				mask: "mm",
				maxWidth: 20
			},

			// HOUR
			{
				name: "小时", //Hour
				unit: "Hour",
				increment: 1000 * 60 * 60,
				mask: "HH",
				maxWidth: 20
			},

			// DAY
			{
				name: "天", //Day
				unit: "Day",
				increment: 1000 * 60 * 60 * 24,
				mask: {
					0: "d",
					40: "ddd d"
				},
				maxWidth: 80
			},

			// WEEK
			{
				name: "周", //Week
				unit: "Week",
				// increment by day
				increment: 1000 * 60 * 60 * 24,

				// x7
				divisor: 7,
				minWidth: 40,

				// override
				value: function( d ) {

					// round to nearest day, move to first day of this week, return
					return Time.roundTo( d, "day" ).moveToDayOfWeek( 1, -1 );
				},

				// override
				format: function( d, context ) {

					//prepend "W" to the week number
					var value = "周" + d.getWeek(); //Date.CultureInfo.timeUnitNamePrefixes.week

					if( context.top ) {
						value = this.addTimeContext( d, this.name, value );
					}

					return value;
				}
			},

			// MONTH
			{
				name: "月", //Month
				unit: "Month",
				// increment by 30 days 
				//(line and shape uniformity may be slightly off (pixel or two), 
				// but it's much faster than incrementing by day when divisor=30)
				increment: 1000 * 60 * 60 * 24 * 30,
				mask: "MMM",
				minWidth: 30
			},

			// YEAR
			{
				name: "年", //Year
				unit: "Year",
				// increment by 365 days 
				//(line and shape uniformity may be slightly off (pixel or two),  
				// but it's much faster than incrementing by day when divisor=365)				
				increment: 1000 * 60 * 60 * 24 * 365,
				mask: "yyyy"
			}
		],

		function( i, def ) {

			// instantiate default TimeUnit
			new TimeUnit( def );
		}

	); // end $.each






	// DEFAULT TIME INTERVAL DEFINTIONS

	// loop over each (default) TimeInterval def and instantiate them as TimeIntervals	
	$.each(
		[
			// SIX (6) MINUTE
			{
				name: "6分钟", //6 Minutes
				unit: "Minute",
				dispUnit: "分钟",
				count: "6"
			},

			// TEN (10) MINUTE
			{
				name: "10分钟", //10 Minutes
				unit: "Minute",
				dispUnit: "分钟",
				count: "10"
			},

			// FIFTEEN (15) MINUTE
			{
				name: "15分钟", //15 Minutes
				unit: "Minute",
				dispUnit: "分钟",
				count: "15",
				maxWidth: 40
			},

			// THIRTY(30) MINUTE
			{
				name: "30分钟", //30 Minutes
				unit: "Minute",
				dispUnit: "分钟",
				count: "30",
				maxWidth: 40
			},

			// FOUR (4) HOUR
			{
				name: "4小时", //4 Hours
				unit: "Hour",
				dispUnit: "小时",
				count: "4",
				maxWidth: 40
			},

			// EIGHT (8) HOUR
			{
				name: "8小时", //8 Hours
				unit: "Hour",
				dispUnit: "小时",
				count: "8",
				maxWidth: 35
			},

			// TWO (2) WEEK
			{
				name: "两周", //2 Weeks
				unit: "Week",
				dispUnit: "周",
				count: "2",

				// override
				value: function( d ) {

					// resolve the first day of this week
					d = Time.roundTo( d, "day" ).moveToDayOfWeek( 1, -1 );

					// if the week is odd return it, otherwise go back another week
					return ( d.getWeek() % 2 ) ? d : d.addDays( -7 );
				},

				// override
				format: function( d, context ) {

					var weekNum = d.getWeek(),
						value = "周" + weekNum + "-" + ( weekNum + 1 ); //Date.CultureInfo.timeUnitNamePrefixes.week

					if( context.top ) {
						value = this.addTimeContext( d, this.unit, value );
					}

					return value;
				}
			},

			// QUARTER ( THREE (3) MONTH )
			{
				name: "季度", //Quarter
				unit: "Month",
				dispUnit: "月",
				count: "3",

				// override
				format: function( d, context ) {

					// prepend Q to the quarter number 
					var value = "季度" + Math.ceil(( d.getMonth() + 1 ) / 3 ); //Date.CultureInfo.timeUnitNamePrefixes.quarter

					if( context.top ) {
						value = this.addTimeContext( d, this.unit, value );
					}

					return value;
				},
				minWidth: 40
			},

			// HALF YEAR ( SIX (6) MONTH )
			{
				name: "半年", //Half Year
				unit: "Month",
				dispUnit: "月",
				count: "6",
				format: function( d, context ) {

					// prepend H to the half number
					var halfNumber = Math.ceil(( d.getMonth() + 1 ) / 6 );
					var value = "前半年"; //Date.CultureInfo.timeUnitNamePrefixes.half
					if (halfNumber === 2) {
						value = "后半年";
					}

					if( context.top ) {
						value = this.addTimeContext( d, this.unit, value );
					}

					return value;

				},
				minWidth: 40
			},

			// FIVE (5) YEAR
			{
				name: "5年", //5 Years
				unit: "Year",
				dispUnit: "年",
				count: "5"
			},

			// DECADE ( TEN (10) YEAR )
			{
				name: "十年", //Decade
				unit: "Year",
				dispUnit: "年",
				count: "10"
			}
		],

		function( i, def ) {

			// Instantiate default TimeInterval from def
			new TimeInterval( def );
		}
	); // end $.each loop


	var GanttMFProperty = function( props, propId ) {
		this.id = propId;
		if( props.IndexOf( propId ) != -1 ) {
			this.prop = props.SearchForProperty( propId );
		}
	}

	$.extend( GanttMFProperty.prototype, {

		toString: function() {
			if( !this.prop || this.prop.Value.IsNULL() ) {
				return "-";
			} else {
				return this.prop.GetValueAsLocalizedText();
			}
		},

		valueOf: function() {
			var objType;

			if( !this.prop || this.prop.Value.IsNULL() ) {
				return;
			}

			switch( this.prop.value.datatype ) {

				//MFDatatypeDate
				case 5:
				case 6:
					return new Date( this.prop.Value.Value );

					//MFDatatypeLookup
				case 9:
					objType = Vault.PropertyDefOperations.GetPropertyDef( this.prop.PropertyDef ).ValueList;
					return objType + "-" + this.prop.Value.GetLookupID();

					//MFDatatypeMultiSelectLookup
				case 10:
					objType = Vault.PropertyDefOperations.GetPropertyDef( this.prop.PropertyDef ).ValueList;
					return $.map( this.prop.Value.GetValueAsLookups(), function( v, i ) {
						return objType + "-" + v.Item;
					} );

				default:
					return this.prop.Value.Value;

			}
		}

	} );

	var extendMapProps = ["properties", "relationships", "options", "itemStyle", "progressStyle", "labelStyle"];

	var GanttItem = function() {
		/// <summary> An item to be displayed/rendered in the Gantt chart </summary>
		/// <field name="Types" static="true" type="Array">Possible Gantt Item types</field>
		/// <field name="id" type="String">External ID</field>
		/// <field name="path" type="String">Unique internal id based on placement in heirarchy</field>
		/// <field name="type" type="String">Type of Gantt Item. Types can be found at GanttItem.Types </field>
		/// <field name="label" type="String">Label to display in the rendering</field>
		/// <field name="icon" type="String">Icon to display in the rendering (only used for activities and resources) </field>
		/// <field name="startDate" type="Date"> When the item starts or is  (time ignored)</field>
		/// <field name="endDate" type="Date">  When the item ends  (time ignored)</field>
		/// <field name="startTime" type="Date"> The time when the item starts or is (date ignored)</field>
		/// <field name="endTime" type="Date"> The time when the item ends (date ignored) </field>
		/// <field name="progress" type="Number"> How complete the item is by %. Possible values: 0-100 </field>
		/// <field name="duration" type="Number"> How long the item lasts. Put into context by durationUnit value.</field>
		/// <field name="durationUnit" type="Number"> The unit in which duration is specified. Possible ptions in Time.BaseUnits </field>
		/// <field name="recur" type="Number"> How long between occurences of a recurring item. Put into context by recurUnit value.</field>
		/// <field name="recurUnit" type="String"> The unit in which recur is specified. Possible ptions in Time.BaseUnits</field>
		/// <field name="editable" type="Boolean"> Indicates whether the item can be check out and/or edited in the current (user) context </field>
		/// <field name="overlay" type="String"> The source of any status overlay image that should be rendered with the item </field>
		/// <field name="rendering" type="GanttRenderingDefinition"> Stores all the informationg </field>
	}

	var GanttRenderingDefinition = function() {
		/// <summary> The gantt rendering information/reference of an item or resource. </summary>
		/// <field name="visible" type="Boolean" value="{x:0,y:0,width:0,height:0}">The area where the primary shape should be/is rendered</field>
		/// <field name="type" type="String">The type of item last/currently drawn. GanttItem.Types</field>
		/// <field name="bb" type="Object" value="{x:0,y:0,width:0,height:0}">The area where the primary shape should be/is rendered</field>
		/// <field name="shape" type="Raphael.Element/Set">The primary shape or set of shapes rendered for the item/resource</field>
		/// <field name="label" type="Raphael.Text"> </field>
		/// <field name="icon" type="Raphael.Image"> </field>
		/// <field name="statusOverlay" type="Raphael.Image"> </field>
		/// <field name="progress" type="Raphael.Rect"> </field>
		/// <field name="uiOverlay" type="Raphael.Image"> </field>
		/// <field name="glow" type="Raphael.Set"> </field>
	}

	var x = new GanttItem();


	GanttItem.Types = [
	    "Activity", //活动
        "Milestone", //里程碑
	    "Resource", //资源
        "Recurring" //循环
	];

	GanttItem.TypeNames = {
	    "Activity": "活动", 
        "Milestone": "里程碑", //
	    "Resource": "资源", //
        "Recurring": "循环" //
	};

	var GanttItemMap = {
		properties: {
			type: "Activity",
			timedEvent: false,
			label: [0],
			startDate: -1,
			endDate: -1,
			startTime: -1,
			endTime: -1,
			progress: -1,
			duration: -1,
			durationUnit: -1,
			recurInterval: -1,
			recurUnit: -1,
			dependency: -1
		},
		relationships: {
			to: [],
			from: []
		},
		options: {
			showProgress: true,
			showLabel: true
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


	$.widget( "mf.gantt", {
		widgetEventPrefix: 'gantt',


		// #########################################
		//
		//  DEFAULT OPTIONS + EVENTS
		//
		// #########################################

		options: {
			startDate: Date.today().addMonths( -3 ),
			endDate: Date.today().addMonths( 3 ),
			focus: Date.today().addWeeks( 1 ),

			// TimeScale Settings
			scaleWidth: 4000,
			baseWidth: 30,
			zoomLevel: 0,
			zoomLevels: [
				//{ tiers: ["15 Minutes", "Hour", "Day"] },
				//{ tiers: ["Hour", "8 Hours", "Day"] },
				//{ tiers: ["4 Hours", "Day", "Week"] },
				//{ tiers: ["8 Hours", "Day", "Week"] },
				{ tiers: ["天", "周", "月"] }, //["Day", "Week", "Month"]
				{ tiers: ["周", "月", "季度"] }, //["Week", "Month", "Quarter"]
				{ tiers: ["两周", "月", "季度"] }, //["2 Weeks", "Month", "Quarter"]
				{ tiers: ["月", "季度", "半年"] }, //["Month", "Quarter", "Half Year"]
				{ tiers: ["季度", "半年"] }, //["Quarter", "Half Year"]
				{ tiers: ["半年", "年"] }, // ["Half Year", "Year"]
				//{ tiers: ["Year", "5 Years", "Decade"] }									
			],

			// Events
			itemLoading: $.noop(),
			itemAdded: $.noop(),
			itemRemoved: $.noop(),
			itemUpdated: $.noop(),
			itemsChanged: $.noop(),
			selectionChanged: $.noop(),
			itemExpanded: $.noop(),
			itemCollapsed: $.noop(),
			optionsChanged: $.noop(),
			timeSelectionActivated: $.noop(),

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

			//styling
			hoverGlow: {
				width: 5,
				color: "black",
				opacity: .2
			},

			selectGlow: {
				width: 5,
				color: "orange",
				opacity: .7
			},

			conflictGlow: {
				width: 2,
				color: "red",
				opacity: 1
			},

			selectionStyle: {
				fill: "#888",
				opacity: ".4",
				"stroke-width": 1,
				stroke: "#444"
			},


			shadeOffDays: true,
			shadeHolidays: true,
			showCurrentTime: true,
			nonWorkingTime: {
				offDays: [0, 6],
				holidays: [
					"01-01",
					"01-06",
					"05-01",
					"12-06",
					"12-24",
					"12-25",
					"12-26",

					// Good Fridays
					"2010-04-02",
					"2011-04-22",
					"2012-04-06",
					"2013-03-29",
					"2014-04-18",
					"2015-04-03",
					"2016-03-25",
					"2017-04-14",
					"2018-03-30",

					// Easters
					"2010-04-04",
					"2011-04-24",
					"2012-04-08",
					"2013-03-31",
					"2014-04-20",
					"2015-04-05",
					"2016-03-27",
					"2017-04-16",
					"2018-04-01",

					// Easter Mondays
					"2010-04-05",
					"2011-04-25",
					"2012-04-09",
					"2013-04-01",
					"2014-04-21",
					"2015-04-06",
					"2016-03-28",
					"2017-04-17",
					"2018-04-02",


					// Ascension
					"2010-05-13",
					"2011-06-02",
					"2012-05-17",
					"2013-05-09",
					"2014-05-29",
					"2015-05-14",
					"2016-05-05",
					"2017-05-25",
					"2018-05-10",

					// Juhannus
					"2010-06-25",
					"2011-06-25",
					"2012-06-23",
					"2013-06-21",
					"2014-06-21",
					"2015-06-26",
					"2016-06-28",
					"2017-06-27",
					"2018-06-22",


				]
			},

			commands: []

		},




		// #########################################
		//
		//  STANDARD WIDGET METHODS
		//
		// #########################################  

		_create: function() {
			/// <summary> standard jQuery widget _create function </summary>

			var that = this,
				o = this.options;
			this.items = {};
			this._selectedItems = [];
			this.lastAutoScroll = new Date();


			// Create DOM Structure
			this.element
                .addClass( "gantt-container" )
                .html(
                    '<div class="gantt">' +
						'<div class="gantt-toolbar">' +
							//'<a href="#" class="gantt-icon gantt-refresh-icon"><img src="images/icons/reset_small.png" alt="Refresh Chart" /></a>' +	
							//'<a href="#" class="gantt-icon gantt-conf-icon"><img src="images/icons/conf.png" alt="Toggle Configuration Panel"/></a>' +

						'</div>' +
                        '<div class="gantt-left"></div>' +
                        '<div class="gantt-right">' +
                            '<div class="gantt-canvas-header">' +
                                '<div class="gantt-time-scroller">' +
                                    '<div class="gantt-time-scale"></div>' +
                                '</div>' +
                            '</div>' +
                            '<div class="gantt-canvas-scroller">' +
                                '<div class="gantt-canvas">&nbsp;</div>' +
                            '</div>' +
                        '</div>' +
                        '<div class="gantt-splitter"></div>' +
						'<div class="gantt-modal-overlay">Please Wait...</div>' +
                    '</div>'
                ).disableSelection();


			this.canvas = Raphael( $( ".gantt-canvas" ).get( 0 ), 100, 100 );

			// this doesn't work in IE 9 :-( http://stackoverflow.com/questions/8086292/significant-whitespace-in-svg-embedded-in-html
			this.canvas.canvas.setAttributeNS( "http://www.w3.org/XML/1998/namespace", "xml:space", "preserve" );


			//this._initMaps();

			this.element.find( ".gantt-left" ).mflist( {
				shellFrame: o.shellFrame,
				widthOffset: 17,
				cols: [
					{ name: "名称", key: "title", width: 250 }, //Name
					//{ name: "Rooli", key: "1087", width: 100 },
					{ name: "类型", key: "type", width: 75 }, //Type
					{ name: "开始", key: "start", width: 75 }, //Start
					{ name: "结束", key: "end", width: 75 },  //End
					{ name: "完成%", key: "progress", width: 60, align: "right" } //% Done


				],
				itemLoading: function( event, item ) {
					//that._mapItem( item.verData, item.properties, item );
					//that._trigger( 'itemLoading', event, item );
				},
				itemAdded: function( event, item ) {
					//that.addItem( item );
					//that._trigger( 'itemAdded', event, item );
				},
				itemUpdated: function( event, item ) {
					//that.refreshItems();
					//that._trigger( 'itemUpdated', event, item );
				},
				itemRemoved: function( event, item ) {
					//delete that.items[item.path];
					//that.refreshItems();
					//that._trigger( 'itemRemoved', event, item );
				},
				hoverChanged: function( event, data ) {
					//that._hoverItem(data.item);
					//that._trigger('hoverChanged', event, data);
				},
				selectionChanged: function( event, data ) {
					that._setSelectedItems( data.selected, data.mode );
				},
				itemsActivated: function( event, data ) {
					that._trigger( 'itemsActivated', event, data );
				},
				itemExpanding: function( event, item ) {
					that.suspendLayout();
					that._trigger( 'itemExpanding', event, item );
				},
				itemExpanded: function( event, item ) {
					//that.refreshItems();
					that._trigger( 'itemExpanded', event, item );
					that.resumeLayout();
				},
				itemCollapsed: function( event, item ) {
					that._drawAll();
					that._trigger( 'itemCollapsed', event, item );
				},
				itemsSorted: function( event ) {
					that.refreshItems();
				},
				sortBy: [2, 3, 1]
			} );

			// store a reference directly to the mflist object (so we don't always need to look it up)
			this.mflist = this.element.find( ".gantt-left" ).data( "mflist" );
			this.time = Time;

			this.ctrlDown = false;
			this.shiftDown = false;
			this.keysDown = [];
			$( document ).keydown( function( e ) {

				var handled = false;

				// update magic key status
				if( e.which === 17 ) { that.ctrlDown = true; }
				if( e.which === 16 ) { that.shiftDown = true; }
				if( e.which === 18 ) { that.altDown = true; }

				// update keys down array if the key wasn't already known to be down
				if( $.inArray( e.which, that.keysDown ) === -1 ) {

					that.keysDown.push( e.which );

					//trigger commands matching current key code
					$.each( that.options.commands, function( k, cmd ) {
						var keyCode = cmd.key,
							match = true;
						if( keyCode !== undefined ) {

							keyCode = $.normalizeToArray( keyCode );

							if( keyCode.length !== that.keysDown.length ) {
								match = false;
							} else {
								$.each( keyCode, function( i, code ) {
									if( $.inArray( code, that.keysDown ) === -1 ) {
										match = false;
										return false;
									}
								} );
							}

							if( match ) {
								handled = true;

								if( cmd.useModalOverlay !== false ) {
									that.showModalOverlay();
								}

								setTimeout( function() {
									try {
										cmd.trigger();
									} catch( e ) { }
									that.showModalOverlay( false );
								}, 0 );

							}

						}
					} );
				}

				if( handled ) {

					e.preventDefault();
					e.stopPropagation();


					//empty keydown array in case an error happens
					that.keysDown.splice( 1, that.keysDown.length );

				}

				return false;

			} ).keyup( function( e ) {

				// update keys down array
				var pos = $.inArray( e.which, that.keysDown );
				if( pos !== -1 ) {
					that.keysDown.splice( pos, 1 );
				}

				// update magic key status
				if( e.which === 17 ) { that.ctrlDown = false; }
				if( e.which === 16 ) { that.shiftDown = false; }
				if( e.which === 18 ) { that.altDown = false; }

			} )


			var autoPanInterval;
			//Bind Scroll Events
			$( ".gantt-canvas-scroller" ).scroll( function() {
				var me = $( this );
				$( ".mflist" ).scrollTop( me.scrollTop() );
				$( ".gantt-time-scroller" ).scrollLeft( me.scrollLeft() );

				/*
				if( ( me.scrollLeft() === 0 || ( me.scrollLeft() + me.outerWidth() >= $( '.gantt-canvas' ).width() ) ) && !autoPanInterval ) {
					autoPanInterval = setInterval( function() {
						if( me.scrollLeft() === 0 ) {
							that.options.focus = that.getTimeAt( that.actualWidth / 2 - 100 );
							var d = that.getVisibleDateReference();
							that.refresh( true );
							that.scrollToTime( d );
						} else if( me.scrollLeft() + me.outerWidth() >= $( '.gantt-canvas' ).width() ) {
							that.options.focus = that.getTimeAt( that.actualWidth / 2 + 100 );
							var d = that.getVisibleDateReference();
							that.refresh( true );
							that.scrollToTime( d );
						} else {
							clearInterval( autoPanInterval );
							autoPanInterval = undefined;
						}
					}, 150 );

					//setTimeout($.proxy(that._autoScroll, that), 250);
				}

				*/
			} );

			//Bind item Mousewheel events including default scroll (which we need to implement cause we never show the vertical scrollbar)
			$( ".mflist" ).mousewheel( function( event, delta ) {
				if( that.shiftDown ) {
					$( ".mflist" ).scrollLeft( $( ".mflist" ).scrollLeft() + -65 * delta );
				} else {
					$( ".gantt-canvas-scroller" ).scrollTop( $( ".gantt-canvas-scroller" ).scrollTop() + $( ".gantt-canvas-scroller" ).height() / -7 * delta );
					//$( ".mflist" ).scrollTop( $( ".mflist" ).scrollTop() + -100*delta );
				}
				return false;
			} );

			//Bind Special Canvas Mousewheel events
			$( ".gantt-canvas-scroller, .gantt-time-scroller" ).mousewheel( function( event, delta ) {
				if( that.ctrlDown ) {
					//console.log("Zoom - ", Date.now().getTime());
					// -1 = up = zoom-in
					// +1 = down = zoom-out
					var o = that.options;
					i = Time.Interval[o.zoomLevels[o.zoomLevel].tiers[0]],
					w = o.baseWidth + 5 * delta;

					that.focusX = event.pageX;

					if( w < i.minWidth ) { //zoomOut
						that.zoomTimeScale( 1 );
					} else if( w > i.maxWidth ) { //zoomIn 
						that.zoomTimeScale( -1 );
					} else if( w != o.baseWidth ) { //adjust unit size
						o.baseWidth = w;
						that.refresh();
						that._trigger( "optionsChanged", event );
					}

					return false;
				} else if( that.shiftDown ) {
					var canvas = $( ".gantt-canvas-scroller" );
					canvas.scrollLeft( canvas.scrollLeft() + ( ( delta > 0 ) ? -65 : 65 ) );
					return false;
				}
			} );

			//Bind Size Events
			this.resizeTo = ( this.element.get( 0 ).tagName.toLowerCase() == "body" ) ? $( window ) : this.element;
			this.resizeTo.resize( function() {
				that._size();
			} );



			//Setup split panel resizer
			$( ".gantt-splitter" ).draggable( {
				axis: "x",
				drag: function( event, ui ) {
					ui.position.left = Math.max( Math.min( ui.position.left, $( ".mflist-items" ).width() ), 100 );
					$( ".gantt-left" ).width( ui.position.left );
					$( ".gantt-right" ).css( "margin-left", ui.position.left );
					$( ".mflist-head-scroller" ).scrollLeft( $( ".mflist" ).scrollLeft() );
					that._size( event );
				}
			} );

			//Setup scale highlighting
			$( ".gantt-canvas, .gantt-time-scroller" ).mousemove( function( event2 ) {
				$( ".gantt-time-unit-hilite" ).removeClass( "gantt-time-unit-hilite" );
				$( ".mflist-item-hilite" ).removeClass( "mflist-item-hilite" );
				$( ".gantt-time-tier" ).each( function() {
					var y = $( this ).position().top + $( this ).height() / 2;
					that.hoveredTimeUnit = $( document.elementFromPoint( event2.pageX, y ) ).addClass( "gantt-time-unit-hilite" );
				} );
				var x = $( ".mflist-items" ).position().left + 50 + $( ".mflist-items" ).scrollLeft();
				$( document.elementFromPoint( x, event2.pageY ) ).closest( ".mflist-item" ).addClass( "mflist-item-hilite" );

			} )
            .mouseleave( function( event ) {
            	$( ".gantt-time-unit-hilite" ).removeClass( "gantt-time-unit-hilite" );
            	$( ".gantt-item-hilite" ).removeClass( "gantt-item-hilite" );
            } );

			this._updateCommands();

			this._drawScale();
			this._bindScaleEvents();
			this.refresh();

			this._bindCanvasEvents();

		},

		_init: function() {
			/// <summary> standard jQuery widget init function </summary>

			//var items = this.element.find(".gantt-left");
		},

		_setOption: function( name, value ) {
			/// <summary> standard jQuery widget _setOption function </summary>
			/// <param name="name" type="string">Name of the option being updated</param>
			/// <param name="value"> The new value of the option </param>

			this.refresh();
			this._trigger( 'optionsChanged' );
		},




		// #########################################
		//
		//  LAYOUT METHODS
		//
		// #########################################  

		_size: function() {
			// make room to get accurate size calculations (in case we're shrinking)
			var total = this.resizeTo.height(),
				toolbar = $( ".gantt-toolbar" ).outerHeight(),
                header = $( ".gantt-canvas-header" ).outerHeight(),
                outer = $( ".gantt-split-container" ).outerHeight() - $( ".gantt-split-container" ).outerHeight();

			$( ".gantt-canvas-scroller" ).outerHeight( total - header - toolbar );
			$( ".gantt-right" ).width( this.resizeTo.width() - $( ".gantt-left" ).width() );
			$( ".gantt-left" ).height( total - toolbar );
			$( ".gantt-splitter" ).css( "top", $( ".gantt-left" ).position().top );
			$( ".gantt-splitter" ).css( "left", $( ".gantt-left" ).width() );
			$( ".gantt-splitter" ).height( $( ".gantt" ).outerHeight() );
			$( ".gantt-time-scroller" ).width( $( ".gantt-right" ).width() - 17 );

			this._setCanvasHeight();

			$( ".gantt-left" ).mflist( "option", "headerHeight", header ).mflist( "resize" );

		},

		_setCanvasHeight: function() {
			var height = Math.max( $( ".gantt-canvas-scroller" ).height() - 17, $( ".mflist-items" ).height() );
			$( ".gantt-canvas" ).height( height );
			this.canvas.setSize( $( ".gantt-canvas" ).width(), height );

			if( this.highlightArea ) {
				this.highlightArea.attr( "height", height ).toBack();
			}

			if( this.offTime ) {
				this.offTime.attr( "height", height ).toBack();
			}
		},

		_updateCommands: function() {
			var that = this;

			this.element.find( ".gantt-toolbar" ).empty();

			$.each( this.options.commands, function( k, cmd ) {

				var enabled = ($.isFunction(cmd.enabled)) ? cmd.enabled() : cmd.enabled;

				if( cmd.visible !== false && enabled != false) {
					var button = $(
						'<a href="#" class="gantt-icon" title="' + cmd.title + ' (' + cmd.keycode + ')" >' +
							'<img src="' + cmd.icon + '" title="' + cmd.title + ' (' + cmd.keycode + ')" />' +
						'</a>'
					).appendTo( that.element.find( ".gantt-toolbar" ) )
					.bind( "click", function() {
						if( cmd.useModalOverlay !== false ) {
							that.showModalOverlay();
						}
						setTimeout( function() {
							try {
								cmd.trigger();
							} catch( e ) { }
							that.showModalOverlay( false );
						}, 0 );
					} );

					if( cmd.showText !== false ) {
						button.append( '<span>' + cmd.title + '</span' );
					}

					if( cmd.align ) {
						button.css( {
							"display": "block",
							"float": cmd.align
						} );
					}
				}

			} );
		},

		refresh: function( focusAlreadySet ) {

			var that = this,
				refreshID;

			if( !this._refreshCount ) {
				this._refreshCount = 1;
			} else {
				this._refreshCount++;
			}

			refreshID = this._refreshCount;

			if( this.suspendRefresh ) return;

			if( !focusAlreadySet ) {
				this.options.focus = this.getVisibleDateReference();
			}

			//this.canvas.clear();
			$( this.canvas.canvas ).hide();

			this._drawScale();

			setTimeout( function() {

				if( refreshID === that._refreshCount ) {
					$( that.canvas.canvas ).show();
					that.refreshItems();
				}

			}, 0 );

		},

		autoFit: function() {
			/// <summary> Center's the chart based on all the top level items </summary>

			var o = this.options,
				minStart = Infinity,
				maxEnd = -Infinity,
				viewWidth = $( ".gantt-time-scroller" ).width() - 100, // 50 px margins
				timeSpan;

			// loop through each item, to figure out the max and min date
			$.each( this.items, function( i, item ) {

				// only handle if startDate is set
				if( item.startDate ) {

					// update minStart as needed
					minStart = Math.min( minStart, item.startDate.valueOf() );

					// check if the object specifies an end date
					if( item.endDate ) {

						// end date specified, update maxEnd as needed
						maxEnd = Math.max( maxEnd, item.endDate.valueOf() );

					} else {

						// end date not specified, update maxEnd as needed with the startDate
						maxEnd = Math.max( maxEnd, item.startDate.valueOf() );
					}

				}
			} );

			// set center focus date it we found a date
			if( minStart < Infinity ) {
				o.focus = new Date( Math.round(( minStart + maxEnd ) / 2 ) );

				// calculate the amount of time we want displayed
				timeSpan = ( maxEnd - minStart );

				// check that there is a bit of time span
				if( timeSpan ) {

					// try to find the optimal zoomLevel and base unit width

					// loop through each zoom level
					$.each( o.zoomLevels, function( i, level ) {
						var bt = Time.Interval[level.tiers[0]],
							ticksPerPixel, timeInView;

						// loop through each valid baseWidth for this zoomLevel (largest to smallest)
						for( var baseWidth = bt.maxWidth; baseWidth >= bt.minWidth; baseWidth -= 5 ) {
							ticksPerPixel = Math.round( bt.increment / ( baseWidth / bt.divisor / bt.count ) );
							timeInView = ticksPerPixel * viewWidth;

							// if the current settings span our desired time span, apply them and be done 
							if( timeInView > timeSpan ) {
								o.zoomLevel = i;
								o.baseWidth = baseWidth;
								return false;
							}
						}

					} );

				}


				this.refresh( true );
			}

		},

		showModalOverlay: function( state ) {
			/// <summary> Gray the screen and prevent interaction. For use while the chart is working </summary>
			/// <parma name="state" type="boolean"> The state of the overlay (show or hide) </param>
			if( state !== false ) {
				$( ".gantt-modal-overlay" ).show();
			} else {
				$( ".gantt-modal-overlay" ).hide();
			}

			this.busy = ( state !== false );
		},




		// #########################################
		//
		//  TIMESCALE METHODS
		//
		// #########################################  


		// PRIVATE METHODS

		_drawScale: function() {
			/// <summary> Draw or Re-Draw the Time scale, which updates the whole chart (to adpat to the new times) <summary>

			var perfStart = new Date(),
				that = this,
				ts = $( ".gantt-time-scale" ), //timeScale element
				o = this.options,
				tiers = $.map( o.zoomLevels[o.zoomLevel].tiers, function( v ) { return Time.Interval[v]; } ),
				bt = tiers[0],  //base tier at this zoom level
				lv = [], // stores the last values calculated for each tier
                html = [], // stores the html for each tier
				unitCount = [],
				lastRun = false,
				curDate, focusOffset, tmpStart, tmpEnd;

			// here we try to normalize transitions between zoomLevels
			// the idea is that we keep the canvas rougly the same width
			// and adjust the canvas start and end time to match.
			// we also ensure that we always render at least as much time as
			// the viewable area, and that the same moment is kept centered

			//o.baseWidth = Math.coerce(o.baseWidth, bt.minWidth, bt.maxWidth);
			//console.log("DrawScale - zoom:", o.zoomLevel, "width:", o.baseWidth, Date.now().getTime());

			// calculate time/pixel resolution for the new zoomLevel and baseWidth
			this.ticksPerPixel = Math.round( bt.increment / ( o.baseWidth / bt.divisor / bt.count ) );

			focusOffset = ( o.scaleWidth / 2 ) * this.ticksPerPixel;

			//adjust to the boudaries of the smallest interval
			tmpStart = o.focus.getTime() - focusOffset;
			tmpEnd = o.focus.getTime() + focusOffset;
			o.startDate = bt.resolve( "value", new Date( tmpStart ) );
			o.endDate = bt.resolve( "value", new Date( tmpEnd ) );

			/*
			if( isNaN( o.startDate.getTime() ) || isNaN( o.startDate.getTime() ) ) {
				alert(( [bt.increment, o.baseWidth, bt.divisor, bt.count, this.ticksPerPixel, o.focus, focusOffset, tmpStart, o.startDate, tmpEnd, o.endDate] ).join( "," ) );
				return;
			}
			*/

			// cache this for rendering calcuations...
			this.actualWidth = this._getTimeWidth( o.startDate, o.endDate );

			this.unitTime = {};
			if( this.offTime ) {
				this.offTime.remove();
			}
			this.offTime = this.canvas.set();

			// keep incrementing curDate until we reach the end
			// Building html strings for each tier as we go (much faster than appending each element)
			curDate = o.startDate.clone();
			while( true ) {

				//update drawn intervals for each tier, if their values have changed this step
				$.each( tiers, function( i, t ) {
					var v = Math.max( t.resolve( "value", curDate ), o.startDate ),
						unitClasses = ["gantt-time-unit"],
						id, width, text, context;

					if( v != lv[i] || lastRun ) {

						//something has changed, we need to update html

						// check if this is our first time through
						if( lv[i] === undefined ) {

							// start tier element our first time through
							html[i] = '<div id="gantt-tier-' + i + '" class="gantt-time-tier">';
							unitCount[i] = 0;

						} else {

							id = "gantt-tu-" + i + "-" + unitCount[i]++;
							that.unitTime[id] = lv[i];

							// close previously finished interval
							width = that._getTimeWidth( lv[i], ( ( !lastRun ) ? v : o.endDate ) ) - 1; // 1 is for border

							// calculate/shade off time
							if( t.unit === "Day" || t.name === "Day" ) {
								var tempDate = new Date( lv[i] );
								if( ( o.shadeOffDays && $.inArray( tempDate.getDay(), o.nonWorkingTime.offDays ) !== -1 )
								  || ( o.shadeHolidays && ( $.inArray( tempDate.toString( "yyyy-MM-dd" ), o.nonWorkingTime.holidays ) !== -1
								  || $.inArray( tempDate.toString( "MM-dd" ), o.nonWorkingTime.holidays ) !== -1 ) ) ) {
									unitClasses.push( "gantt-offtime" );
									that.offTime.push( that.canvas.rect( that._getTimeOffset( new Date( lv[i] ) ), 0, width + 1, 2000 ).attr( { fill: "#aaaaaa", "stroke-width": 0, opacity: .1 } ) );
								}
							}

							// define the context, so the unit can determine what  label format to use
							context = {
								width: width,
								tier: i,
								bottom: ( i === 0 ),
								top: ( i + 1 === tiers.length )
							};

							text = t.resolve( "format", new Date( lv[i] ), context )
							html[i] += '<div id="' + id + '" class="' + unitClasses.join( " " ) +
											'" style="width:' + width + 'px">' + text + '</div>';

						}

						//close tier element last time through
						if( lastRun ) html[i] += '</div>';

						// setup next interval
						lv[i] = v;

					}

				} );


				if( lastRun ) break;
				if( o.endDate <= curDate ) { lastRun = true };

				//increment the date by the smalles unit (tier 0)
				curDate.addMilliseconds( bt.increment );

			}

			// Draw now line
			if( o.showCurrentTime && that._onChart( new Date() ) ) {
				that.offTime.push( that.canvas.rect( that._getTimeOffset( new Date() ) + .5, 0, 1, 2000 ).attr( { fill: "#ffaaaa", "stroke-width": 0, } ) );
			}


			// dump all html content to our timescale element
			// this is much faster than adding elements one at a time
			ts.html( html.reverse().join( "\n" ) );

			// adapt to new timescale size
			//$(".gantt-toolbar").height($(".gantt-canvas-header").height() - $(".gantt-items-header").outerHeight() - 1);
			$( ".gantt-canvas, .gantt-time-scale" ).width( this.actualWidth );
			this._size( true );

			this.scrollToTime( o.focus );

			if( this.highlightArea ) {
				this.highlightArea.attr( { x: 0, width: 0 } );
			}


			//$("#perf").text( (new Date()) - perfStart );
		},

		_bindScaleEvents: function() {
			/// <summary> Wire up time scale events to certain user interactions </summary>
			var that = this,
				timeUnitSelector = ".gantt-time-tier:last>.gantt-time-unit",
				selClass = "gantt-time-unit-select";

			this.highlightArea = this.canvas.rect( 0, 0, 0, 0 ).attr( { fill: "orange", "stroke-width": 1, stroke: 1, opacity: .2 } ).toBack(),

			$( ".gantt-time-scale" ).on( {

				"mousedown": function() {
					var start = that.hoveredTimeUnit,
						end = start;

					$( document ).bind( "mouseup.timeUnitSelection", function() {
						$( this ).unbind( ".timeUnitSelection" );

					} ).bind( "mousemove.timeUnitSelection", function() {
						var open;

						first = start;
						end = that.hoveredTimeUnit || end;

						$( timeUnitSelector ).removeClass( selClass );

						if( end.is( ".gantt-time-unit" ) ) {

							$( timeUnitSelector ).each( function() {
								if( start.is( this ) || end.is( this ) ) {
									if( open || end.is( start ) ) {
										$( this ).addClass( selClass );
										return false;
									} else {
										open = true;
									}
								}

								if( open ) {
									$( this ).addClass( selClass );
								}
							} );



							that.highlightArea.attr( that._getTimeUnitRange( start, end ) );
						}

					} );

					$( timeUnitSelector ).not( this ).removeClass( selClass );

					$( this ).toggleClass( selClass );

					if( !$( this ).hasClass( selClass ) ) {
						that.highlightArea.attr( { x: 0, width: 0 } );
					} else {
						that.highlightArea.attr( that._getTimeUnitRange( start, start ) );
					}
				}

			}, timeUnitSelector );
		},

		_getTimeUnitTime: function( timeUnitNode ) {
			/// <summary>Get the time represented by a certain timeUnitNode in the time scale </summary>
			/// <returns type="Date"></returns>
			return this.unitTime[timeUnitNode.attr( "id" )];
		},

		_getTimeUnitRange: function( timeUnitNode1, timeUnitNode2 ) {
			/// <summary>Gets the time between two timeUnitNodes in the time scale </summary>
			/// <param name="timeUnitNode1" type="DOM.Node">First Node</param>
			/// <param name="timeUnitNode2" type="DOM.Node">Second Node</param>
			/// <returns type="Number">ticks between</returns>

			var that = this,
				nodes = $.map( [timeUnitNode1, timeUnitNode2], function( n ) {
					return {
						start: that._getTimeUnitTime( n ),
						end: that._getTimeUnitTime( n.next() ),
						width: n.outerWidth(),
						left: n.offset().left
					};
				} );

			if( nodes[0].start > nodes[1].start ) {
				nodes.reverse();
			}

			var x = this._getTimeOffset( nodes[0].start );

			return {
				start: nodes[0].start,
				end: nodes[1].end,
				x: x,
				width: this._getTimeOffset( nodes[1].end ) - x
			}

		},

		// PUBLIC METHODS

		zoomTimeScale: function( i ) {
			/// <summary> Adjust the zoom level of the time scale</summary>
			/// <param name="i" type="Number">Levels to zoom. Negative values zoom in, positive values zoom out<summary>

			var o = this.options,
				newLevel = o.zoomLevel + i,
				ti;

			if( newLevel < 0 || newLevel >= o.zoomLevels.length ) return;

			o.zoomLevel = newLevel;
			ti = Time.Interval[o.zoomLevels[o.zoomLevel].tiers[0]];
			o.baseWidth = ( i < 0 ) ? ti.minWidth : ti.maxWidth;

			this.refresh();
			this._trigger( 'optionsChanged' );

		},

		scrollToTime: function( d ) {
			/// <summary> Scroll the time scale to a certain time </summary>
			/// <param name="d" type="Date"> The Date/Time to scroll to </param>
			var o = this.options,
                offset = ( d - o.startDate ) / this.ticksPerPixel - $( ".gantt-time-scroller" ).outerWidth() / 2,
				adjust = this.restoreScrollOffset || 0;

			$( ".gantt-canvas-scroller, .gantt-time-scroller" ).scrollLeft( offset - adjust );

			this.restoreScrollOffset = 0;
		},


		// #########################################
		//
		//  CANVAS METHODS
		//
		// #########################################  

		_bindCanvasEvents: function() {
			/// <summary> Wire up canvas based user interactions </summary>
			var that = this;
			this.element.find( ".gantt-canvas" ).mousedown( function( evt ) {

				// Check if this click has been handled by an item in the gantt chart
				if( that._mousedownOnItem ) {

					// reset
					that._mousedownOnItem = false;

				} else {

					// handle mousedown			
					var itemNode = that.element.find( ".mflist-item-hilite" ).parent(),
						item = that.mflist.getItem( itemNode.attr( "id" ) ),
						origX = evt.clientX,
						startTimeNode, endTimeNode, vert, bb, clicks = 0;

					// deslect any item
					that.setSelectedItems();

					//remove any previous time selection
					if( that.timeSelection ) {
						that.timeSelection.remove();
						delete that.timeSelection;
					}

					// Handle resource area selection
					if( item && item.type === "Resource" ) {

						startTimeNode = that.hoveredTimeUnit;
						endTimeNode = startTimeNode;
						vert = that._getItemVertCoords( item, 1 );
						bb = $.extend( that._getTimeUnitRange( startTimeNode, endTimeNode ), vert );

						that.timeSelection = that.canvas.rect();

						that.timeSelection.attr( that.options.selectionStyle )
							.mousedown( function() {
								that._mousedownOnItem = true;
							} )
							.click( function( evt ) {
								clicks++;

								if( clicks === 1 ) {
									setTimeout( function() {
										clicks = 0;
									}, 250 );
								} else if( clicks > 1 ) {
									clicks = 0;
									setTimeout( function() {
										that._trigger( 'timeSelectionActivated', evt, $.extend( { item: item }, bb ) );
									}, 5 );
									that.timeSelection.remove();
									delete that.timeSelection;
								}
								return false;
							} );

						$( this ).bind( "mouseup.timeselection", function( evt ) {
							$( this ).unbind( ".timeselection" );
						} );

						$( this ).bind( "mousemove.timeselection", function( evt ) {
							var curNode = that.hoveredTimeUnit;
							if( curNode.length && curNode.is( ".gantt-time-unit" ) ) {
								endTimeNode = curNode;
								bb = $.extend( that._getTimeUnitRange( startTimeNode, endTimeNode ), vert );
								that.timeSelection.attr( bb );
							}
						} );


					}

				}
			} );

		},


		// #########################################
		//
		//  DRAWING UTILITY METHODS
		//
		// #########################################  


		// PRIVATE METHODS

		_onChart: function( start, end ) {
			/// <summary>
			/// Determines if the time period between start and end overlaps with 
			/// the period currently loaded (rendered, but not necessarily visible)
			/// in the chart.  If end is not specified, it returns whether the start
			/// time falls between the loaded start and end times.
			/// </summary>
			/// <param name="start" type="Date">start date</param>
			/// <param name="end" type="Date">end date (optional)</param>
			/// <returns type="bool"></returns>

			end = end || start;
			return !( this.options.startDate > end || start > this.options.endDate );
		},

		_getTimeWidth: function( start, end ) {
			/// <summary>
			///  Gets the width in pixels between two dates (times) based on the current timescale
			/// </summary>
			/// <param name="start" type="Date">start date</param>
			/// <param name="end" type="Date">end date</param>
			/// <returns type="int"></returns>

			return ( end - start ) / this.ticksPerPixel;
		},

		_getTimeOffset: function( dt ) {
			/// <summary>
			/// Gets the horizontal offset in pixels from the beginning of the chart to the provided date (time)
			///	based on the current timescale
			/// </summary>
			/// <param name="dt">PropertyValues to be used in the document.</param>
			/// <returns>int</returns>

			return this._getTimeWidth( this.options.startDate, dt );
		},

		_getTimeCoords: function( start, end ) {
			/// <summary>
			/// Gets the horizontal offset (x) and width (w) in pixels of a period between 
			/// two dates (times) based on the current timescale.
			/// Width (w) is undefined if no end paramater is specified.
			/// </summary>
			/// <param name="start">start date</param>
			/// <param name="end">end date (optional)</param>	
			/// <returns>{x:int, w:int} or {x:int}</returns>
			var hCoords = {};
			hCoords.x = Math.max( Math.min( this._getTimeOffset( start ), this.actualWidth ), 0 );

			if( end ) {
				hCoords.width = Math.max( Math.min( this._getTimeWidth( start, end ), this.actualWidth ), 0 );
			}

			return hCoords;
		},

		_getItemVertCoords: function( item, pad ) {
			/// <summary>
			/// Gets the vertical offset (y) and height(h) in pixels corresponding to a 
			/// listing item (found in this.items)
			/// </summary>
			/// <param name="item">listing item</param>
			/// <returns>{y:int, h:int}</returns>	
			var vCoords = { y: 0, height: 0 },
				row;

			if( pad === undefined ) {
				pad = 3;
			}

			if( !item.parent || item.parent.type !== "Resource" ) { //资源
				row = $( "#" + item.path ).find( ".mflist-item" );
			} else {
				row = $( "#" + item.parent.path ).find( ".mflist-item" );
			}

			if( row.length ) {

				// item was found in the listing, calculate coords
				vCoords.y = row.offset().top - $( ".mflist-items" ).offset().top + pad;
				vCoords.height = row.outerHeight() - pad * 2;

			}

			return vCoords;
		},

		_getItemBB: function( item, start, end, pad ) {
			/// <summary>
			/// Gets a bounding box where an items rendering should be confined
			/// based on start and end times. If no end date is provided, then
			/// width (w) is undefined
			/// </summary>
			/// <param name="item">listing item</param>
			/// <param name="start">start date</param>
			/// <param name="end">end date (optional)</param>
			/// <returns>{x:int, y:int, h:int} or {x:int, y:int, w:int, h:int}</returns>

			return $.extend(
						this._getItemVertCoords( item, pad ),
						this._getTimeCoords( start, end )
					);
		},


		// PUBLIC METHODS

		getVisibleDateReference: function() {
			/// <summary> 
			///  Determines the center or focus date of the chart.
			///  Used to reset the position of a chart after zooming in or out
			/// </summary>
			/// <returns type="Date"> </returns>

			var gts = $( ".gantt-time-scroller" ),
				centerOffset = gts.outerWidth() / 2 + gts.scrollLeft(),
				offset;

			if( this.focusX ) {
				offset = this.focusX + gts.scrollLeft() - gts.offset().left;
				this.restoreScrollOffset = offset - centerOffset
			} else {
				offset = centerOffset;
			}

			this.focusX = null;
			return this.getTimeAt( offset );
		},

		getTimeAt: function( x ) {
			/// <summary> Determines the time represented at a certain chart offset </summary>
			/// <param name="x" type="Number"> Horizontal pixel offset </param>
			/// <returns type="Date"></returns>

			return this.options.startDate.clone().addMilliseconds( x * this.ticksPerPixel );
		},

		suspendLayout: function() {
			/// <summary>
			///  Indicates that chart drawing opertaions should be suspended until resumeLayout() is called
			///  Improves performance for batch operations where there is no need to update the chart until
			///  after a bunch of changes have been made.
			/// </summary>

			if( !$.isNumeric( this._suspendLayout ) ) {
				this._suspendLayout = 0;
			}

			this._suspendLayout += 1;
		},

		resumeLayout: function() {
			/// <summary> Indicates that items on the chart should be drawn. Also updates past changes. </summary>
			this._suspendLayout -= 1;

			if( this._suspendLayout < 1 ) {
				this._suspendLayout = 0;
				this._drawAll();
			}
		},

		getCanvasOffset: function() {
			/// <summary> Determine how far the canvas is currently scrolled </summary>
			/// <returns type="Number"> Horizonal pixel offset </returns> 
			var gts = this.element.find( ".gantt-time-scroller" );
			return gts.scrollLeft() - gts.offset().left;
		},




		// #########################################
		//
		//  LOW LEVEL DRAWING METHODS
		//
		// ######################################### 

		_drawBar: function( bb, style, progress, progressStyle, cornerR ) {
			/// <summary>
			/// Renders and returns bar shape for the item
			/// if progress is provided, it renders a spearate mask for the uncompleted
			/// portion. The mask is found at <return obj>.progress
			/// </summary>
			/// <param name="item">listing item</param>
			/// <param name="start">start date</param>
			/// <param name="end">end date (optional)</param>
			/// <returns>raphael.element</returns>		

			// declare variables
			var bar;

			// add cornerR default value as needed
			if( !$.isNumeric( cornerR ) ) {
				cornerR = 0;
			}

			// Draw Bar
			bar = this.canvas.rect( bb.x, bb.y, bb.w, bb.h, cornerR ).attr( $.extend( {}, style ) );

			if( progress || progress === 0 ) {
				var px = bb.w * ( Math.max( Math.min( progress, 100 ), 0 ) / 100 );
				progressStyle = progressStyle || { fill: "#888", opacity: .5, "stroke-width": 1, "stroke": "#888" };
				bar.progress = this.canvas.rect( px + bb.x + 1, bb.y + 1, bb.w - px - 2, bb.h - 2 ).attr( $.extend( {}, progressStyle ) );
			}

			return bar;
		},

		_getDiamondPath: function( bb ) {
			/// <summary>
			///  Creates a path that represents a diamond shape
			///      a
			///    /   \
			///   b     d
			///    \   /
			///      c
			/// </summary>
			/// <param name="bb" type="Rafael.BBox"> Bounding box params indicating size and location <param>
			/// <returns type="string"> SVG path string </returns>

			var r = bb.height / 2,
				a = [bb.x + r, bb.y],
				b = [bb.x, bb.y + r],
				c = [bb.x + r, bb.y + bb.height],
				d = [bb.x + bb.width, bb.y + r];

			return "M" + a + "L" + b + "L" + c + "L" + d + "Z";

		},

		_drawText: function( bb, text, style ) {
			/// <summary> Draw text on the chart </summary>
			/// <param name="bb" type="Rafael.BBox">Bounding Box (location and size) of text area</param>
			/// <param name="text" type="string"> Text string to draw </param>
			/// <param name="style" type="Object"> Object representing the style of the text </param>
			/// <returns type="Rafael.Text"></returns>

			var xOffset = 20;
			style = style || { 'text-anchor': 'end', fill: "#888", "font-style": "normal", "font-size": "10px" };

			return this.canvas.text( bb.x - xOffset, bb.y + bb.h / 2, text ).attr( $.extend( {}, style ) );
		},

		_drawProgressBar: function( item ) {
			/// <summary> Draws progress information over Activity bar</summary>
			/// <param name="item" type="GanttItem"> item for which to draw progress information </param>

			// declare variables
			var r = item.rendering,
				progress = ( item.showProgress !== false ) ? item.progress : false,
				xo, attr;

			// only draw progress for Activities
			if( item.type === "Activity" && item.rendering.visible && ( progress || progress === 0 ) ) {

				// calculate width of progress (offset where to start "not done" bar)
				ox = r.bb.width * ( Math.max( Math.min( progress, 100 ), 0 ) / 100 );

				// calcualate progress bar attributes
				attr = $.extend( {
					x: ox + r.bb.x + 1,
					y: r.bb.y + 1,
					width: r.bb.width - ox - 2,
					height: r.bb.height - 2
				}, item.progressStyle );


				// ensure progress element exists 
				if( !r.progress ) {
					r.progress = this.canvas.rect();
				}

				// update progress bar
				r.progress.attr( attr ).toFront();

			} else {

				if( !item.rendering.visible ) {

					if( r.progress ) {
						r.progress.remove();
						delete r.progress;
					}

				}
			}
		},

		_drawStatusOverlay: function( item ) {
			/// <summary> 
			///  Creates and/or updates the status overlay image (check-in/out status) and position of the item.
			/// </summary>
			/// <param name="item" type="GanttItem"> the item whose overlay should be updated </param>

			// declare variables
			var r = item.rendering,
				attr;


			// check if it should be displayed
			if( !item.overlay || !r.visible ) {

				// hide overlay
				if( r.statusOverlay ) {
					r.statusOverlay.remove();
					delete r.statusOverlay;
				}

			} else {

				// calculate overlay attributes
				attr = {
					x: r.bb.x - 10,
					y: r.bb.y + r.bb.height - 14,
					src: item.overlay,
					width: 16,
					height: 16
				};

				// ensure overlay element exists
				if( !r.statusOverlay ) {

					// create overlay if it doesn't exist
					r.statusOverlay = this.canvas.image();
				}

				// update and show overlay
				r.statusOverlay.attr( attr ).toFront();

			}

		},

		_drawIcon: function( item ) {
			/// <summary>Creates and/or updates an item's icon.</summary>
			/// <param name="item" type="GanttItem"> the item whose icon should be updated </param>

			// declare variables
			var r = item.rendering,
				dim, attr;

			// check if it should be displayed
			if( !item.icon || !r.visible || !item.showIcon ) {

				// hide overlay
				if( r.icon ) {
					r.icon.remove();
					delete r.icon;
				}

			} else {

				dim = r.bb.height - 4;

				// calculate overlay attributes
				attr = {
					x: r.bb.x + 2,
					y: r.bb.y + 2,
					src: item.icon,
					width: dim,
					height: dim,
					"clip-rect": ( r.bb.x + 2 ) + " " + r.bb.y + " " + ( r.bb.width - 4 ) + " " + r.bb.height,
				};

				// ensure overlay element exists
				if( !r.icon ) {

					// create overlay if it doesn't exist
					r.icon = this.canvas.image();
				}

				// update and show overlay
				r.icon.attr( attr ).toFront();

			}
		},

		_drawLabel: function( item ) {
			/// <summary>Creates and/or updates an item's label.</summary>
			/// <param name="item" type="GanttItem"> the item whose text should be updated </param>

			var r = item.rendering,
				xPadding = 2,
				xOffset = 5,
				attr;

			// remove label no matter what to delete the mask
			if( r.label ) {
				r.label.remove();
				delete r.label;
			}

			if( !r.visible || !item.showLabel || !item.label ) {

				return;

			} else {

				if( item.labelPosition == 2 ) { // Inside

					if( item.icon && item.showIcon ) {
						xOffset += r.bb.height - 2;
					}

					attr = {
						x: r.bb.x + xOffset,
						y: r.bb.y + r.bb.height / 2,
						fill: "black",
						'text-anchor': 'start',
						"clip-rect": ( r.bb.x + xOffset ) + " " + r.bb.y + " " + ( r.bb.width - xOffset - xPadding ) + " " + r.bb.height,
					};

				} else if( item.labelPosition == 3 ) { // Right

					attr = {
						x: r.bb.x + r.bb.width + 20,
						y: r.bb.y + r.bb.height / 2,
						'text-anchor': 'start',
						fill: "#888",
					};

				} else { // Left (All other values)

					attr = {
						x: r.bb.x - 20,
						y: r.bb.y + r.bb.height / 2,
						'text-anchor': 'end',
						fill: "#888",
					};
				}

				$.extend( attr, {
					text: item.label,
					"font-family": "Segoe UI",
					"font-style": "normal",
					"font-size": "10px"
				} );

				// update attributes
				r.label = this.canvas.text().attr( attr ).toFront();

				// IE9 HACK!  By appending an empty text node to the Text element, the clipping works
				$( r.label.node ).append( " " );
			}
		},

		_drawUiOverlay: function( item ) {
			/// <summary>Creates and/or updates an item's ui overlay (used for user interaction).</summary>
			/// <param name="item" type="GanttItem"> the item whose ui overlay should be (re)drawn </param>
			/// <returns type="boolean">Indicates whether a new overlay element was created (events should be rebound)</returns>

			var r = item.rendering,
				created = false;

			if( !r.visible || (r.uiOverlay && !r.uiOverlay.editable && item.editable) ) {

				if( r.uiOverlay ) {
					r.uiOverlay.remove();
					delete r.uiOverlay;
				}
			}

			if( r.visible ) {

				if( !r.uiOverlay ) {
					r.uiOverlay = this.canvas.rect().attr({ fill: "green", stroke: "green", "stroke-width": 1, opacity: 0 });
					r.uiOverlay.editable = item.editable;
					created = true;
				}

				// update attributes
				r.uiOverlay.attr( r.bb ).toFront();

			}

			return created;
		},

		_drawDependencyLine: function( dep, item ) {
			/// <summary> Draws a dependency line between two items on the chart </summary>
			/// <param name="dep" type="GanttItem">The item from which the line should be drawn</param>
			/// <param name="item" type="GanttItem">The item to which the line should be drawn (arrow end)</param>
			/// <returns type="Rafael.Path">The drawn line object </returns>

			// initialize variables and some simple initial calculations
			var that = this,
				depBB = dep.rendering.bb,
				itemBB = item.rendering.bb,
				x1 = depBB.x + depBB.width,
				y1 = depBB.y + depBB.height / 2,
				x2 = itemBB.x,
				y2 = itemBB.y + itemBB.height / 2,
				offChart = false,
				yOffset = 3.5,
				xOffset = 10,
				point = [[x1, y1]],
				attr = {
					"stroke-width": 2,
					"arrow-end": "block",
					stroke: "#666",
					opacity: 1,
					"class": "dependencyLine"
				},
				path, line;

			$.each( [x1, y1, x2, y2], function( i, v ) {
				if( v < 0 || v > that.actualSize ) {
					offChart = true;
				}
			} );

			//one of the items isn't on the chart, bail! (don't draw line)
			if( offChart ) return;

			// determine if this dependency is violated
			// dependency should always come before item horizontally
			if( item.start < (dep.end || dep.start) ) {

				// violation!  turn the line red
				attr.stroke = "#ff4444";
				attr["stroke-width"] = 2.5;
			}

			if( x2 - x1 < xOffset * 2 ) {

				// items are close together, or invalid -> draw with cutbacks
				//
				//   #####dep##### 0 -- 1
				//                      |
				//             3 ------ 2
				//			   |
				//             4 -- 5 #####item#####
				//


				yOffset += depBB.height / 2;

				if( y1 < y2 ) {
					yOffset *= -1;
				}

				point[1] = [x1 + xOffset, y1];
				point[2] = [x1 + xOffset, y2 + yOffset];
				point[3] = [x2 - xOffset, y2 + yOffset];
				point[4] = [x2 - xOffset, y2];
				point[5] = [x2, y2];

			} else {

				// draw normal
				//
				//   #####dep##### 0 -- 1
				//                      |
				//                      2 -- 3 #####item#####
				//

				point[1] = [x2 - xOffset, y1];
				point[2] = [x2 - xOffset, y2];
				point[3] = [x2, y2];

			}

			//draw path and style
			path = "M" + point.join( "L" );
			line = this.canvas.path( path ).attr( attr ).toBack();

			return line;
		},

		_drawGlow: function( item ) {
			/// <summary>Draws a glow, normalizing for a set (due to raphael bug) </summary>
			/// <param name="r" type="GanttItemRendering"> </param>
			/// <param name="attrs" type="object"> Attributes defining the glow, i.e. width, color, opacity </param>

			var r = item.rendering,
				attrs;

			// always remove previous glow
			if( r.glow ) {
				r.glow.remove();
			}

			if( r.staticGlow ) {
				r.staticGlow.remove();
			}

			// check if the item is even visible
			if( r.visible && !item.dragging ) {


				// update static glow if there is a conflict
				if( item.conflict ) {
					r.staticGlow = r.shape.glow( this.options.conflictGlow );

				}

				// determine item state and load appropriate glow settings
				if( item.selected ) {

					// get selected glow settings
					attrs = this.options.selectGlow;

				} else if( item.hovered ) {

					// get hovered glow settings
					attrs = this.options.hoverGlow;
				}

				// if we've loaded some glow settings, draw it...
				if( attrs ) {

					// Detect if the shape is a set of shapes
					if( r.shape.forEach ) {

						// create a set to store the glow of each shape
						r.glow = this.canvas.set();

						//loop over each shape in the passed set
						r.shape.forEach( function( inst ) {

							// draw a glow for this shape, and add it to our glow set 
							r.glow.push( inst.glow( attrs ) );
						} );


					} else {

						// single shape, apply glow normally
						r.glow = r.shape.glow( attrs );
					}
				}

			}

		},

		_drawToolTip: function( x, y, text, textAttr, tipAttr ) {
			/// <summary> Draws a tool tip on the chart </summary>
			/// <param name="x" type="number"> Horizontal location of the tooltip</param>
			/// <param name="y" type="number"> Vertical location of the tooltip</param>
			/// <param name="text" type="string">The text to appear in the tool tip</param>
			/// <param name="textAttr" type="object">Object with style parameters of the text</param>
			/// <param name="tipAttr" type="objectr">Object with style parameters of the tooltip</param>

			var tri = 5,
				padX = 4,
				padY = 1,
				s = this.canvas.set(),
				point = [[x, y]],
				t = this.canvas.text( x, y, text ).attr( textAttr ),
				bb = $.extend( {}, t.getBBox() ),
				path, p;

			bb.width += padX * 2;
			bb.height += padY * 2;

			if( textAttr["text-anchor"] === "end" ) {
				//             0
				//           / |
				// 2 ------ 1  |
				// |           |
				// 3 ---------4
				//

				//relative commands
				point[1] = [-tri, tri];
				point[2] = [tri - bb.width, 0];
				point[3] = [0, bb.height];
				point[4] = [bb.width, 0];

			} else if( textAttr["text-anchor"] === "start" ) {
				// 0
				// |\  
				// | 4 ------- 3
				// |           |
				// 1 --------- 2
				//

				point[1] = [0, bb.height + tri];
				point[2] = [bb.width, 0];
				point[3] = [0, -bb.height];
				point[4] = [tri - bb.width, 0];

			} else { //center
				//          0
				//        /   \
				//  2 -- 1     6 -- 5
				//  |               |
				//  3 ------------- 4
				//

				point[1] = [-tri, tri];
				point[2] = [tri - bb.width / 2, 0];
				point[3] = [0, bb.height];
				point[4] = [bb.width, 0];
				point[5] = [0, -bb.height];
				point[6] = [tri - bb.width / 2, 0];

			}

			path = "m" + point.join( " " ) + "Z";
			p = this.canvas.path( path ).attr( tipAttr );

			//center text
			bb = p.getBBox();
			t.attr( {
				"text-anchor": "middle",
				x: bb.x + bb.width / 2,
				y: bb.y + ( bb.height + tri ) / 2
			} );

			t.toFront();
			s.push( p );
			s.push( t );

			s.attr( "opacity", ".8" );

			return s;

		},

		_drawDateBlobs: function( item ) {
			/// <summary> Draws date tool tips at the beginning and end of a Gantt Item </summary>
			/// <param name="item" type="GanttItem"> Item for which to draw the date blobgs </param>

			var r = item.rendering,
				textAttr = {
					fill: "#444",
					"font-family": "Segoe UI",
					"font-weight": "bold",
					"font-size": "10px"
				},

				tipAttr = {
					fill: "270-#ffffff-#fcf49a",
					stroke: "#444",
				},
				startAttr = $.extend( {}, textAttr ),
				startX, endAttr;

			if( r.startBlob ) {
				r.startBlob.remove();
				delete r.startBlob;
			}

			if( r.endBlob ) {
				r.endBlob.remove();
				delete r.endBlob;
			}

			if( r.visible && ( item.dragging /*|| item.hovered */ ) ) {

				if( item.end ) {
					endAttr = $.extend( { "text-anchor": "start" }, textAttr );
					r.endBlob = this._drawToolTip( r.bb.x + r.bb.width, r.bb.y + r.bb.height + 2, item.end.toString( "ddd, dd-MMM-yyyy" ), endAttr, tipAttr );

					startAttr["text-anchor"] = "end";
					startX = r.bb.x;
				} else {
					startX = r.bb.x + r.bb.width / 2; // center under item
				}

				r.startBlob = this._drawToolTip( startX, r.bb.y + r.bb.height + 2, item.start.toString( "ddd, dd-MMM-yyyy" ), startAttr, tipAttr );

			}

		},





		// #########################################
		//
		//  HIGH LEVEL DRAWING METHODS
		//
		// ######################################### 

		_showItem: function( r, shape ) {
			/// <summary> Ensure an item is visible in the chart (creating if needed) </summary>
			/// <param name="r" type="GanttItemRendering"> Rendering object of the item</param>
			/// <param name="shape" type="string">The type of shape that should be displayed for the item </param>

			// set the rendering to visible
			r.visible = true;

			// remove any wrong shape that may be set
			if( r.shape && r.shape.type !== shape ) {
				r.shape.remove();
				delete r.shape;
			}

			// create the shape if it doesn't exist
			if( !r.shape ) {

				r.shape = this.canvas[shape]();
			}

		},

		_hideItem: function( r ) {
			/// <summary> Ensure an item is not drawn in the chart (removing if needed) </summary>
			/// <param name="r" type="GanttItemRendering"> Rendering object of the item</param>

			// set the rendering to not visible
			r.visible = false;

			// remove any shape from canvas and rendering
			if( r.shape ) {
				r.shape.remove();
				delete r.shape;
			}
		},

		_drawAll: function() {
			/// <summary>(Re)Draws all loaded items on the chart</summary>

			var that = this;

			if( this._suspendLayout ) return;

			this._setCanvasHeight();

			$.each( this.items, function( i, item ) {

				that._drawItem( item );
			} );

		},

		_drawItem: function( item, remove ) {
			/// <summary>Updates the rendering of an item on the chart.</summary>
			/// <param name="item" type="GanttItem"> The item whose rendering should be updated </param>
			/// <param name="remove" type="boolean"> Indicates that the update operation is to remove the item</param>

			if( this._suspendLayout && !remove ) return;
			
			var key = "_draw" + item.type;
			//todo
			if (item.type === '活动') {
				key = "_draw" + 'Activity';
			} else if (item.type === '里程碑') {
				key = "_draw" + 'Milestone';
			} else if (item.type === '资源') {
				key = "_draw" + 'Resource';
			} else if (item.type === '循环') {
				key = "_draw" + 'Recurring';
			} 
			this[key]( item, remove );

		},

		_drawActivity: function( item, remove ) {
			/// <signature>
			///		<summary> Create or update the rendering of an activity </summary>
			///		<param name="item" type="GanttItem"> The item representing the activity being rendered </param>
			///</signature>

			// declare variables
			var that = this,
				r = item.rendering,
				start, end, attr;

			// No date information for this object, or it's off the chart
			if( !item.start || !item.end || item.start > item.end
				|| !this._onChart( item.start, item.end ) || remove ) {

				this._hideItem( r );

			} else {

				// show element in the chart
				this._showItem( r, "rect" );

				//calculate start and end times (visible in chart)
				start = new Date( Math.max( item.start, this.options.startDate ) );
				end = new Date( Math.min( item.end, this.options.endDate ) );

				// calculate attributes
				r.bb = this._getItemBB( item, start, end );
				attr = $.extend( {}, r.bb, item.itemStyle );

				//render
				r.shape.attr( attr ).toFront();

			}


			this._drawDependencies( item );
			this._drawGlow( item );
			this._drawProgressBar( item );
			this._drawIcon( item );
			this._drawLabel( item );
			this._drawStatusOverlay( item );

			if( this._drawUiOverlay( item ) ) {
				this._bindItemEvents( item );
			}

			this._drawDateBlobs( item );


		},

		_drawMilestone: function( item, remove ) {
			/// <signature>
			///		<summary> Create or update the rendering of an Milestone </summary>
			///		<param name="item" type="GanttItem"> The item representing the Milestone being rendered </param>
			///</signature>

			var r = item.rendering,
				start, attr;

			// No date information for this item, or it's off the chart
			if( !item.start || !this._onChart( item.start ) || remove ) {

				this._hideItem( item.rendering );

			} else {

				this._showItem( item.rendering, "path" );

				r.bb = this._getItemBB( item, item.start );
				r.bb.width = r.bb.height;
				r.bb.x -= r.bb.width / 2;

				attr = $.extend(
						{
							path: this._getDiamondPath( r.bb )
						},
						item.itemStyle
					);

				//update attributes
				r.shape.attr( attr ).toFront();

			}

			this._drawDependencies( item );
			this._drawLabel( item );
			this._drawStatusOverlay( item );
			this._drawGlow( item );
			this._drawIcon( item );
			if( this._drawUiOverlay( item ) ) {
				this._bindItemEvents( item );
			}

			this._drawDateBlobs( item );

		},

		_drawRecurring: function( item, remove ) {
			/// <signature>
			///		<summary> Create or update the rendering of a recurring activity </summary>
			///		<param name="item" type="GanttItem"> The item representing the recurring activity being rendered </param>
			///</signature>

			var o = this.options,
				r = item.rendering,
				twoDim, v, start, end, fin, bb, shape;

			// Missing date or recurrence information for this object, or it's off the chart
			if( !item.start || !item.end || !item.recurInterval || !item.recurUnit || !this._onChart( item.start, item.end ) || remove ) {

				this._hideItem( item.rendering );

			} else {

				r.visible = true;

				// show element in the chart
				if( r.shape ) {
					r.shape.remove();
				}

				r.shape = this.canvas.set();


				// determine if each event is 1 or 2 dimensional
				twoDim = ( item.duration && item.durationUnit );

				// Get item's vertical coordinates (only need to calculate once)
				v = this._getItemVertCoords( item );

				r.bb = $.extend( {
					x: 0,
					width: 10000,
				}, v );

				// calculate first instance start, and final instance cutoff
				start = item.start.clone();

				// calculate max start date of an occurance to draw
				fin = new Date( Math.min( this.options.endDate, item.endDate ) );

				//offset twelve hours if there is no duration, to center the symbol on the day
				if( !twoDim ) {
					start.addHours( 12 ); //center on the day
					fin.addHours( 12 )
				}

				//render relevant instances  
				while( fin >= start ) {
					end = ( !twoDim ) ? start : start.clone()["add" + item.durationUnit]( item.duration );

					if( this._onChart( start, end ) ) {

						bb = $.extend( this._getTimeCoords( start, end ), v );

						if( twoDim ) {
							shape = this.canvas.rect().attr( $.extend( bb, item.itemStyle ) );
						} else {
							bb.width = bb.height;
							bb.x -= bb.width / 2;
							shape = this.canvas.path().attr( $.extend( { path: this._getDiamondPath( bb ) }, item.itemStyle ) );
						}
						r.shape.push( shape );

					}

					// increment start time based on recurrence properties
					start["add" + item.recurUnit]( item.recurInterval );
				}

				this._drawGlow( item );
				if( this._drawUiOverlay( item ) ) {
					this._bindItemEvents( item );
				}


			}

		},

		_drawResource: function( item, remove ) {

			return;
		},

		_drawDependencies: function( item ) {
			/// <summary> Draw all the item dependencies of the current item </summary>
			/// <param name="item" type="GanttItem"> current item </param>

			var that = this;

			// remove lines to
			if( item.rendering.linesTo ) {
				$.each( item.rendering.linesTo, function( path, line ) {
					if( line.from ) {
						delete line.from.rendering.linesFrom[item.path];
					}
					line.remove();
				} );
			}
			item.rendering.linesTo = {};

			// draw lines to
			if( item.rendering.visible && item.dependentOn ) {
				$.each( item.dependentOn, function( path, other ) {
					var line;
					if( other.rendering.visible ) {
						line = that._drawDependencyLine( other, item );
						if( line ) {
							line.to = item;
							line.from = other;
							item.rendering.linesTo[other.path] = line;

							other.rendering.linesFrom = other.rendering.linesFrom || {};
							other.rendering.linesFrom[item.path] = line;
						}
					}
				} );
			}


			// remove lines from
			if( item.rendering.linesFrom ) {
				$.each( item.rendering.linesFrom, function( path, line ) {
					if( line.to ) {
						delete line.to.rendering.linesTo[item.path];
					}
					line.remove();
				} );
			}
			item.rendering.linesFrom = {};

			// draw lines from
			if( item.rendering.visible && item.dependants ) {
				$.each( item.dependants, function( path, other ) {
					var line;
					if( other.rendering.visible ) {
						line = that._drawDependencyLine( item, other );
						if( line ) {
							line.from = item;
							line.to = other;
							item.rendering.linesFrom[other.path] = line;

							other.rendering.linesTo = other.rendering.linesTo || {};
							other.rendering.linesTo[item.path] = line;
						}
					}

				} );
			}


		},






		// #########################################
		//
		//  ITEM MANIPULATION METHODS
		//
		// #########################################


		// PRIVATE METHODS

		_updateDependencyReferences: function( item ) {
			/// <summary> Resolve references to all  dependencies of the current item that are in the chart</summary>
			/// <param name="item" type="GanttItem">Current Item</param>

			var that = this;

			// loop over all items to check if there are dependencies
			$.each( that.items, function( i, other ) {

				// are we dependent on this other item?
				if( $.isArray( item.dependency ) && $.inArray( other.id, item.dependency ) !== -1 ) {

					item.dependentOn = item.dependentOn || {};
					item.dependentOn[other.path] = other;

					other.dependants = other.dependants || {};
					other.dependants[item.path] = item;

				}

				// is this other item dependent on us?
				if( $.isArray( other.dependency ) && $.inArray( item.id, other.dependency ) !== -1 ) {

					other.dependentOn = other.dependentOn || {};
					other.dependentOn[item.path] = item;

					item.dependants = item.dependants || {};
					item.dependants[other.path] = other;

				}

			} );

		},

		_removeDependencyReferences: function( item ) {
			/// <summary> Remove all the dependency references of the current item</summary>
			/// <param name="item" type="GanttItem">Current Item</param>

			var that = this;

			if( item.dependentOn ) {
				$.each( item.dependentOn, function( path, other ) {
					delete item.dependentOn[path];
					delete other.dependants[item.path];
				} );
			}

			if( item.dependants ) {
				$.each( item.dependants, function( path, other ) {
					delete item.dependants[path];
					delete other.dependentOn[item.path];
				} );
			}

		},

		_updateUtilizationConflictStatus: function( resource ) {
			/// <summary> Determine if the current resource has any conflicting allocations</summary>
			/// <param name="resource" type="GanttItem">Current resource</param>

			var allocs = $.map( resource.children, function( a ) {
				a.conflict = false;
				return a;
			} ),
				l = allocs.length,
				i = 0,
				j, a1, a2;

			for( i; i < l - 1; i++ ) {

				a1 = allocs[i];

				for( j = i + 1; j < l; j++ ) {

					a2 = allocs[j];

					if( ( a2.start > a1.start && a2.start < a1.end ) || ( a2.end > a1.start && a2.end < a1.end ) ) {
						a1.conflict = true;
						a2.conflict = true;
					}

				}
			}

		},

		// PUBLIC METHODS

		hasChildren: function( parent ) {
			/// <summary> Determine if the parent item has children</summary>
			/// <param name="parent" type="GanttItem"> Parent Item </param>
			/// <returns type="boolean"></returns>

			return this.mflist.hasChildren( parent );
		},

		getChildren: function( parent ) {
			/// <summary> Returns the children items ofthe parent (if any)</summary>
			/// <param name="parent" type="GanttItem"> Parent Item </param>
			/// <returns type="array"></returns>

			return this.mflist.getChildren( parent );
		},

		getItems: function() {
			/// <summary> Get a copied object containing all the items loaded in the chart</summary>
			/// <returns type="object"></returns>

			return $.extend( {}, this.items );
		},

		getItemsByID: function( id ) {
			/// <summary>Returns all items loaded in the chart with a certain id</summary>
			/// <param name="id" type="string">Id string</param>
			/// <returns type="array"></returns>

			return $.map( this.items, function( item ) {

				if( item.id === id ) {

					return item;
				}
			} );

		},

		addItems: function( items, parent ) {
			/// <summary> Adds items to the chart</summary>
			/// <param name="items" type="array"> GanttItems to add </param>
			/// <param name="parent" type="GanttItem"> Parent Item </param>

			var that = this;

			this.suspendLayout();

			// normalize arg to array
			items = $.normalizeToArray( items );

			// treat items as allocations if parent is a resource
			if( parent && parent.type === "Resource" ) { //资源

				// add child container to resource if it doesn't already exist
				if( !parent.children ) {
					parent.children = [];
				}

				// loop over each allocations and setup in the resource
				$.each( items, function( i, alloc ) {

					// do some setup that is handled by mflist of normal items
					alloc.parent = parent;
					alloc.path = parent.path + "_" + alloc.id;
					parent.children.push( alloc );

					// add item to the item list
					that.items[alloc.path] = alloc;
				} );

				that._updateUtilizationConflictStatus( parent );

			} else {

				// add items as children in the listing
				this.mflist.addItems( items, parent );

				// loop over each item
				$.each( items, function( i, item ) {

					if( item.type === "Resource" ) {
						that._trigger( "itemExpanding", { type: "itemExpanding" }, item );
					}

					// add item to the item list
					that.items[item.path] = item;

					//update dependency references
					that._updateDependencyReferences( item );

				} );

			}

			// redraw all
			this.resumeLayout();			

		},

		updateItems: function( items ) {
			/// <summary> Updates certain items in the chart</summary>
			/// <param name="items" type="array"> GanttItems to update </param>

			var that = this,
				normalItems = [],
				resourcesToUpdate = [];

			this.suspendLayout();

			// normalize arg to array
			items = $.normalizeToArray( items );

			// redraw only those items that have been passed
			$.each( items, function( i, item ) {

				if( !item.parent || item.parent.type !== "Resource" ) { //资源

					// keep reference to update in listing
					normalItems.push( item );

				} else if( $.inArray( item.parent, resourcesToUpdate ) === -1 ) {

					// keep references of resources whose allocations have changed to update conflict status
					resourcesToUpdate.push( item.parent );
				}

				//update dependency references
				that._updateDependencyReferences( item );

			} );

			// update resource allocation conflic statuses
			$.each( resourcesToUpdate, function( i, resource ) {
				that._updateUtilizationConflictStatus( resource );
			} );

			// update listing with relevent items
			this.mflist.updateItems( normalItems );

			// redraw all
			this.resumeLayout();
			
			// update commands associated with selected items
			this._updateCommands();

		},

		removeItems: function( items ) {
			/// <summary> Removes items from the chart</summary>
			/// <param name="items" type="array"> GanttItems to add </param>
			/// <param name="parent" type="GanttItem"> Parent Item </param>
			var that = this,
				normalItems = [],
				resourcesToUpdate = [];

			// normalize arg to array
			items = $.normalizeToArray( items );

			this.suspendLayout();

			// loop over each item
			$.each( items, function( i, item ) {

				//removeChildElements
				if( item.children ) {
					that.removeItems( $.map( item.children, function( v ) { return v; } ) );
				}

				delete item.children;

				// remove from the item list
				delete that.items[item.path];

				// remove dependencies
				that._removeDependencyReferences( item );

				// remove any drawn elements
				that._drawItem( item, true );

				//keep a list of items that must be removed from mflist
				if( !item.parent || item.parent.type !== "Resource" ) { //资源
					normalItems.push( item );
				} else if( item.parent ) {

					if( $.inArray( item.parent, resourcesToUpdate ) === -1 ) {
						// keep references of resources whose allocations have changed to update conflict status
						resourcesToUpdate.push( item.parent );
					}

					delete item.parent.children[item.id];
				}

			} );

			// update resource allocation conflic statuses
			$.each( resourcesToUpdate, function( i, resource ) {
				that._updateUtilizationConflictStatus( resource );
			} );

			// remove "normal" items from mflist
			this.mflist.removeItems( normalItems );

			// redraw all as we don't want to calculate which items have moved vertically
			this.resumeLayout();

		},

		reloadAll: function() {
			/// <summary> Reloads all base items in the chart, collapsing the hierarchy </summary>

			var that = this;

			$.each( this.mflist.getChildren(), function( i, item ) {

				if( item.children ) {
					that.removeItems( $.map( item.children, function( v ) { return v; } ) );
				}

				that.mflist.collapseItem( item );

				delete item.children;

			} );

		},

		//deprecated
		refreshItems: function() {

			this._drawAll();
		},




		// #########################################
		//
		//  ITEM EVENT METHODS
		//
		// #########################################


		// PRIVATE METHODS

		_bindItemEvents: function( item ) {
			/// <summary> Binds standard item events to the shape passed </summary>
			/// <param type="GanttItem" name="item"> The item context for the bound events </param>
			/// <param type="RaphaelElement" name="shape"> The shape to bind the events to </param>

			// Set this to specific variable for reference in callbacks
			var that = this,
				r = item.rendering,
				ui = r.uiOverlay,
				clicks = 0,
				drag,
				start;

			ui.hover(
				function() {
					that._hoverItem( item );
				},
				function() {
					that._hoverItem();

					if( !that.dragging ) {
						$( that.canvas.canvas ).css( "cursor", "default" );
					}
				}
			);

			ui.click( function( evt ) {
				clicks++;
				if( clicks === 1 ) {
					setTimeout( function() {
						clicks = 0;
					}, 250 );
					that.focusItem( item, evt );
				} else if( clicks === 2 ) {
					that.activateItems( item );
					clicks = 0;
				}
				return false;
			} );


			ui.mousedown( function( evt ) {
				that._mousedownOnItem = true;

			} );

			ui.mouseup( function( evt ) {
				that.dragging = false;

			} );

			if( item.editable && item.type !== "Recurring" ) {

				ui.mousemove( function( evt ) {
					var x, c;

					if( !that.dragging ) {

						x = evt.clientX + that.getCanvasOffset()
						c = "default";

						if( r.bb.width > 15 && item.end ) {
							if( x >= r.bb.x && x <= r.bb.x + 5 ) {
								c = "w-resize";
							} else if( x >= r.bb.x + r.bb.width - 5 && x <= r.bb.x + r.bb.width ) {
								c = "e-resize";
							}
						}

						$( that.canvas.canvas ).css( "cursor", c );
					}

				} );


				// Bind drag events
				ui.drag(
					function( dx, dy, x, y, evt ) {

						var c = $( that.canvas.canvas ).css( "cursor" ),
							diff;

						//initialize dragging after the mouse has been moved sufficiently
						if( item.editable && !drag && Math.abs( dx ) > 3 ) {

							if( c === "default" ) {
								c = "move";
								$( that.canvas.canvas ).css( "cursor", c );
							}

							drag = {
								start: that._getTimeUnitTime( that.hoveredTimeUnit ),
								items: [],
								mode: c
							};

							if( !item.selected ) {
								that.setSelectedItems( item );
							}

							item.dragging = true;

							$.each( that._selectedItems, function( i, dragItem ) {

								if( dragItem.editable && ( c === "move" || dragItem.type === "Activity" ) && dragItem.type !== "Resource" ) {

									drag.items.push( dragItem );

									that._drawGlow( dragItem );

									dragItem.dragStart = dragItem.start.clone();

									if( dragItem.end ) {
										dragItem.dragEnd = dragItem.end.clone();
									}

								}

							} );

						}

						// update dates
						if( that.hoveredTimeUnit && drag ) {

							diff = that._getTimeUnitTime( that.hoveredTimeUnit ) - drag.start;

							$.each( drag.items, function( i, dragItem ) {

								var newStart = dragItem.dragStart.clone(),
									newEnd;

								if( dragItem.editable ) {

									if( c === "w-resize" || c === "move" ) {
										newStart.addMilliseconds( diff );
									}

									if( dragItem.dragEnd ) {
										newEnd = dragItem.dragEnd.clone();
										if( c === "e-resize" || c === "move" ) {
											newEnd.addMilliseconds( diff );
										}
									}


									if( !newEnd || newEnd > newStart ) {
										dragItem.start = newStart;

										if( newEnd ) {
											dragItem.end = newEnd;
										}

										that._drawItem( dragItem );
									}

								}
							} );

						}

					},

					function( x, y, evt ) {
						that.dragging = true;
					},

					function( evt ) {
						var selectedItems = that._selectedItems.concat( [item] ),
							changedItems = [],
							items;

						$( that.canvas.canvas ).css( "cursor", "default" );

						if( drag ) {

							items = drag.items;
							drag = undefined;

							setTimeout( function() {
								that.setSelectedItems( selectedItems );
							}, 50 );

							that.dragging = false;
							item.dragging = false;

							$.each( items, function( i, dragItem ) {
								//dragItem.dragging = false;

								if( dragItem.start.getTime() != dragItem.dragStart.getTime() || ( dragItem.end && dragItem.end.getTime() != dragItem.dragEnd.getTime() ) ) {

									changedItems.push( dragItem );
								}

								delete dragItem.dragStart
								delete dragItem.dragEnd

								that._drawItem( dragItem );

							} );


							if( changedItems.length ) {

								that.showModalOverlay();
								setTimeout( function() {
									try {
										that._trigger( 'itemsChanged', { type: 'itemsChanged' }, { items: changedItems } );
									} catch( e ) { }
									that.showModalOverlay( false );
								}, 0 );

							}

						}
					}
				);

			}

		},

		_hoverItem: function( item ) {
			/// <signature>
			///		<summary> Unhover all items </summary>
			/// </signature>
			/// <signature>
			///		<summary> Update item appearance for hover (mouse over) </summary>
			///		<param type="GanttItem" name="item"> </param>
			/// </signature>

			//update appearance of previously hovered item
			if( this._hoveredItem ) {
				this._hoveredItem.hovered = false;
				this._drawGlow( this._hoveredItem );
				//this._drawDateBlobs( this._hoveredItem );
			}

			//update hover appearance of passed item
			if( item ) {
				item.hovered = true;
				this._drawGlow( item );
				//this._drawDateBlobs( item );
			}

			// update which item is hovered
			this._hoveredItem = item;
		},

		_setSelectedItems: function( items, mode ) {
			/// <signature>
			///     <summary> Selects all the items in the passed array 
			///		 This method should not be used! Except from mflist.selectionChanged event handler.
			///			Use the public version instead!
			///		</summary>
			///     <param type="Array" elementType="GanttItem" name="items"> </param>
			/// </signature>

			var that = this;

			items = $.normalizeToArray( items );

			// loop over previously selected
			$.each( this._selectedItems, function( i, item ) {

				// handle resource allocations special if special mode of selection
				if( mode === "toggle" && item.parent && item.parent.type == "Resource" ) { //资源

					// keep them selected, in toggle mode
					items.push( item );

				} else if( mode === "range" && item.parent && item.parent.type == "Resource" && $.inArray( item.parent, items ) !== -1 ) { //资源

					// keep them selected in range mode, if their parent is selected
					items.push( item );

				} else {

					// unselect and update appearance
					item.selected = false;
					that._drawGlow( item );

				}

			} );


			// loop over newly selected
			$.each( items, function( i, item ) {

				// select and update appearance
				item.selected = true;
				that._drawGlow( item );
			} );


			// make sure we only have unique values
			items = $.grep( items, function( v, k ) {
				return $.inArray( v, items ) === k;
			} );

			// update selected
			this._selectedItems = items;

			//update commands
			this._updateCommands();

			this._trigger( 'selectionChanged', {}, { items: items } );


		},


		// PUBLIC METHODS

		hoverItem: function( item ) {
			/// <summary> Selects the passed item </summary>
			/// <param type="GanttItem" name="item"> </param>

			// delegate to mflist, which will propogate to 
			this.mflist.setHoveredItem( item );
		},

		focusItem: function( item, event ) {
			/// <summary> Sets an item as focused </summary>
			/// <param name="item" type="GanttItem"> The item to be focused</param>
			/// <param name="event" type="Event"> The event object describing the focus trigger</param>

			var selected = item,
				index;

			// see if we're dealing with a normal (not inline) item
			if( !item.parent || item.parent.type != "Resource" ) { //资源

				// if we aren't dealing with an inline item, let the listing handle the focus and selecting
				this.mflist.focusItem( item, event );

			} else {

				// handle the selection

				// see if the shift or ctrl key is down
				if( event.shiftKey || event.ctrlKey ) {

					// toggle selected state and don't interfere with other selected items

					// make a copy of the current selecte items array
					selected = this._selectedItems.concat();

					// find the position of the current item
					index = $.inArray( item, selected );

					if( index != -1 ) {

						// remove the item from the list of selected items
						selected.splice( index, 1 );

					} else {

						// add the item to the list of selected items
						selected.push( item );
					}

				}

				// update the selection
				this._setSelectedItems( selected );

			}

		},

		setSelectedItems: function( items ) {
			/// <signature>
			///     <summary> Deselects all items </summary>
			/// </signature>
			/// <signature>
			///     <summary> Selects the passed item </summary>
			///     <param type="GanttItem" name="item"> </param>
			/// </signature>
			/// <signature>
			///     <summary> Selects all the items in the passed array </summary>
			///     <param type="Array" elementType="GanttItem" name="items"> </param>
			/// </signature>

			// delegate to mflist, which will propogate to 
			// the local private version... this._setSelectItems

			this.mflist.setSelectedItems( items );
			this._setSelectedItems( items );

		},

		getSelectedItems: function() {
			/// </summary> Gets the items currently selected in the chart </summary>
			/// <returns type="array"></returns>
			return this._selectedItems.concat();
		},

		activateItems: function( items ) {
			/// <summary> 
			///	 Activates the passed items, if no items are passes, it activates the currently selected items 
			/// </summary>
			/// <param type="GanttItem" name="items"> </param>

			this.mflist.activateItems( items );

		}


	} );





} )( jQuery );